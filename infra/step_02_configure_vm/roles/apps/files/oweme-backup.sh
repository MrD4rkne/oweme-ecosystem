#!/usr/bin/env bash
# Dumps api-db and keycloak-db into /opt/backups/<run>/ and uploads both to the backup bucket (issue #101).
# Run nightly by cron and by the deploy before migrating. Restore the two dumps as a pair: api rows are keyed by the Keycloak sub.
set -euo pipefail
umask 077

source /etc/oweme-backup.env # BACKUP_PAR_URL, the read-write PAR (ends in /o/)
cd /opt/apps

# Cron and a deploy can overlap: wait for the other run instead of racing it, but not forever.
exec 9>/run/oweme-backup.lock
flock -w 1800 9

# The migrator starts a stopped api-db itself, so never skip: start both DBs and dump them.
# On a fresh VM this creates and dumps empty databases, which is harmless.
# --no-recreate: back up the current container as it is, before a deploy applies a new image or config to it.
docker compose up -d --wait --no-recreate api-db keycloak-db

db_bytes() { docker compose exec -T "$1" sh -c 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -tAc "select pg_database_size(current_database())"'; }
# timeout runs inside the container so it kills pg_dump itself; --lock-wait-timeout only bounds getting the locks.
dump() { docker compose exec -T "$1" sh -c 'timeout 1800 pg_dump -U "$POSTGRES_USER" --lock-wait-timeout=300s -Fc "$POSTGRES_DB"' > "$tmp/$2.dump"; }

# Backups share the disk with the Postgres volumes: refuse when free space is under 2x the databases.
api_bytes=$(db_bytes api-db)      # separate assignments so set -e catches a failed psql
kc_bytes=$(db_bytes keycloak-db)
need=$(( 2 * (api_bytes + kc_bytes) ))
install -d -m 700 /opt/backups
free=$(df --output=avail -B1 /opt/backups | tail -n1)
if (( free < need )); then
  echo "Not enough disk for a backup: free=${free} need=${need}" >&2
  exit 1
fi

run=$(date -u +%Y%m%dT%H%M%SZ)
tmp="/opt/backups/.$run.partial"
trap 'rm -rf "$tmp"' EXIT
mkdir "$tmp"

# api first: a gap leaves Keycloak users without api rows (harmless); the other order orphans api rows.
dump api-db api
dump keycloak-db keycloak
mv "$tmp" "/opt/backups/$run"

for db in api keycloak; do
  # Bounded so a stalled upload can't hold the deploy; https only so a bad vault value can't send the dump in clear.
  # The URL goes in on stdin (-K -), not argv, so it never shows up in the process list.
  curl -fsS --proto '=https' --retry 3 --max-time 600 -T "/opt/backups/$run/$db.dump" -K - <<< "url = \"${BACKUP_PAR_URL}$run/$db.dump\""
done

# Keep the last 3 runs locally; the bucket keeps 14 days.
find /opt/backups -mindepth 1 -maxdepth 1 -type d -name '2*' | sort | head -n -3 | xargs -r rm -rf --
echo "backup $run uploaded"
