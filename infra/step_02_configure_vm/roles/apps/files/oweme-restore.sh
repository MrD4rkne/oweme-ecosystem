#!/usr/bin/env bash
# Usage: oweme-restore.sh <run>   e.g. 20261104T030000Z
# Replaces api-db and keycloak-db with the dump pair of <run> (issue #101). Restores both as a pair:
# api rows are keyed by the Keycloak sub. Uses /opt/backups/<run>/ if present, otherwise downloads it from the bucket.
# Restore only an environment's own dumps (production dumps carry real users and client secrets).
set -euo pipefail
umask 077

run=${1:?usage: oweme-restore.sh <run>, e.g. 20261104T030000Z}
[[ $run =~ ^[0-9]{8}T[0-9]{6}Z$ ]] || { echo "Not a run id: $run" >&2; exit 1; }

source /etc/oweme-backup.env # BACKUP_PAR_URL, the read-write PAR (ends in /o/)
cd /opt/apps

dir=/opt/backups/$run
if [[ ! -d $dir ]]; then
  tmp=$(mktemp -d /opt/backups/.restore.XXXXXX)
  trap 'rm -rf "$tmp"' EXIT
  for db in api keycloak; do
    curl -fsS --proto '=https' --retry 3 --max-time 600 -o "$tmp/$db.dump" -K - <<< "url = \"${BACKUP_PAR_URL}$run/$db.dump\""
  done
  dir=$tmp
fi

# Both dumps must be readable before anything is dropped.
for db in api keycloak; do
  docker compose exec -T api-db pg_restore --list > /dev/null < "$dir/$db.dump"
done

read -rp "Replace the api and keycloak databases on $(hostname) with run $run? Type 'restore' to continue: " answer
[[ $answer == restore ]] || { echo "Aborted"; exit 1; }

# Safety dump of the current state, so a bad restore can be undone with this script.
/usr/local/sbin/oweme-backup.sh

# Hold the backup lock so the nightly cron can't dump half-restored databases.
exec 9>/run/oweme-backup.lock
flock -w 1800 9

docker compose stop api keycloak # also drops Keycloak's caches
trap 'echo "Restore FAILED: api and keycloak are stopped. Rerun with the safety run (newest in /opt/backups) to go back." >&2' ERR

restore() { # <service> <dump>
  docker compose exec -T "$1" sh -c 'dropdb -U "$POSTGRES_USER" --force "$POSTGRES_DB" && createdb -U "$POSTGRES_USER" "$POSTGRES_DB"'
  docker compose exec -T "$1" sh -c 'pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --no-owner --exit-on-error --single-transaction' < "$2"
}
restore api-db "$dir/api.dump"
restore keycloak-db "$dir/keycloak.dump"

docker compose up -d --wait keycloak
# An older dump has an older schema: migrate it forward with the current image, then start the api.
docker compose run --rm api-migrator
docker compose up -d --wait api
echo "Restored run $run. Check the app; the safety dump taken before the restore is the newest run in /opt/backups."
