#!/usr/bin/env bash
# Usage: oweme-restore.sh <run>   e.g. 20261104T030000Z
# Replaces api-db and keycloak-db with the dump pair of <run> (issue #101). Restores both as a pair:
# api rows are keyed by the Keycloak sub. Uses /opt/backups/<run>/ if present, otherwise downloads it from the bucket.
# Restore only an environment's own dumps (production dumps carry real users and client secrets).
set -eEuo pipefail # -E: the ERR trap also fires inside functions
umask 077

run=${1:?usage: oweme-restore.sh <run>, e.g. 20261104T030000Z}
[[ $run =~ ^[0-9]{8}T[0-9]{6}Z$ ]] || { echo "Not a run id: $run" >&2; exit 1; }

source /etc/oweme-backup.env # BACKUP_PAR_URL, the read-write PAR (ends in /o/)
cd /opt/apps

# Work on a private copy: the safety backup below prunes old local runs, possibly this one.
dir=$(mktemp -d /opt/backups/.restore.XXXXXX)
trap 'rm -rf "$dir"' EXIT
if [[ -d /opt/backups/$run ]]; then
  cp /opt/backups/"$run"/{api,keycloak}.dump "$dir"/
else
  for db in api keycloak; do
    curl -fsS --proto '=https' --retry 3 --max-time 600 -o "$dir/$db.dump" -K - <<< "url = \"${BACKUP_PAR_URL}$run/$db.dump\""
  done
fi

# Read both archives completely (not just their table of contents) before anything is stopped or dropped.
docker compose up -d --wait --wait-timeout 300 --no-recreate api-db keycloak-db
for db in api keycloak; do
  docker compose exec -T api-db pg_restore -f /dev/null < "$dir/$db.dump"
done

read -rp "Replace the api and keycloak databases on $(hostname) with run $run? Type 'restore' to continue: " answer || true
[[ $answer == restore ]] || { echo "Aborted"; exit 1; }

# From here on, a failure leaves both apps stopped rather than half-running on a half-restored state.
safety=""
failed() {
  if [[ -z $safety ]]; then
    docker compose up -d --no-recreate api keycloak
    echo "Restore FAILED before any database changed (safety dump failed); api and keycloak started again." >&2
  else
    docker compose stop api keycloak
    echo "Restore FAILED: api and keycloak are stopped. To go back: oweme-restore.sh $safety" >&2
  fi
}
trap failed ERR
docker compose stop api keycloak # no writes after the safety dump; also drops Keycloak's caches

# Safety dump of the current state, so a bad restore can be undone with this script.
safety=$(/usr/local/sbin/oweme-backup.sh | tee /dev/stderr | awk '/^backup .* uploaded$/ { print $2 }')

# Hold the backup lock so the nightly cron can't dump half-restored databases.
exec 9>/run/oweme-backup.lock
flock -w 1800 9

restore() { # <service> <dump>
  docker compose exec -T "$1" sh -c 'dropdb -U "$POSTGRES_USER" --force "$POSTGRES_DB" && createdb -U "$POSTGRES_USER" "$POSTGRES_DB"'
  docker compose exec -T "$1" sh -c 'pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --no-owner --exit-on-error --single-transaction' < "$2"
}
restore api-db "$dir/api.dump"
restore keycloak-db "$dir/keycloak.dump"

docker compose up -d --wait --wait-timeout 600 keycloak
# An older dump has an older schema: migrate it forward with the current image, then start the api.
docker compose run --rm api-migrator
docker compose up -d --wait --wait-timeout 300 api
echo "Restored run $run. Check the app. To undo: oweme-restore.sh $safety"
