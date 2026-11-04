#!/usr/bin/env bash
# Usage: oweme-restore.sh <run>   e.g. 20261104T030000Z
# Replaces api-db and keycloak-db with the dump pair of <run> (issue #101). Restores both as a pair:
# api rows are keyed by the Keycloak sub. Uses /opt/backups/<run>/ if present, otherwise downloads it from the bucket.
# Restore only an environment's own dumps (production dumps carry real users and client secrets).
set -euo pipefail # no -E: the ERR trap must not also fire inside $( ) (it would run twice)
umask 077

run=${1:?usage: oweme-restore.sh <run>, e.g. 20261104T030000Z}
[[ $run =~ ^[0-9]{8}T[0-9]{6}Z$ ]] || { echo "Not a run id: $run" >&2; exit 1; }

source /etc/oweme-backup.env # BACKUP_PAR_URL, the read-write PAR (ends in /o/)
cd /opt/apps

# Work on a private copy: the safety backup below prunes old local runs, possibly this one.
install -d -m 700 /opt/backups # absent on a replacement VM
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
# Only these get restarted if the safety dump fails: after an earlier failed restore they are stopped on purpose.
running=$(docker compose ps --services --status running api keycloak | xargs) # own assignment: set -e stops here if ps fails
read -ra were_running <<< "$running"
failed() {
  if [[ -z $safety ]]; then
    if (( ${#were_running[@]} )); then docker compose up -d --no-recreate "${were_running[@]}"; fi
    echo "Restore FAILED: the safety dump failed, no database was changed by this run. Restarted: ${were_running[*]:-nothing}." >&2
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

# Inline, not a function: the ERR trap doesn't fire inside functions without -E.
for pair in api-db:api keycloak-db:keycloak; do
  docker compose exec -T "${pair%%:*}" sh -c 'dropdb -U "$POSTGRES_USER" --force "$POSTGRES_DB" && createdb -U "$POSTGRES_USER" "$POSTGRES_DB"'
  docker compose exec -T "${pair%%:*}" sh -c 'pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --no-owner --exit-on-error --single-transaction' < "$dir/${pair##*:}.dump"
done

docker compose up -d --wait --wait-timeout 600 keycloak
# An older dump has an older schema: migrate it forward with the current image, then start the api.
docker compose run --rm api-migrator
docker compose up -d --wait --wait-timeout 300 api
echo "Restored run $run. Check the app. To undo: oweme-restore.sh $safety"
