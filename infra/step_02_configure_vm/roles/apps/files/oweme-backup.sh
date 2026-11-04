#!/usr/bin/env bash
# Dumps api-db and keycloak-db into /opt/backups/<run>/ and uploads both to the backup bucket (issue #101).
# Run nightly by cron and by the deploy before migrating. Restore the two dumps as a pair: api rows are keyed by the Keycloak sub.
set -euo pipefail
umask 077

source /etc/oweme-backup.env # BACKUP_PAR_URL, the read-write PAR (ends in /o/)
cd /opt/apps

# Fresh VM: nothing to back up yet.
if [[ -z "$(docker compose ps -q --status running api-db)" ]]; then
  echo "api-db is not running, skipping backup"
  exit 0
fi

db_bytes() { docker compose exec -T "$1" sh -c 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -tAc "select pg_database_size(current_database())"'; }
dump() { docker compose exec -T "$1" sh -c 'pg_dump -U "$POSTGRES_USER" -Fc "$POSTGRES_DB"' > "$tmp/$2.dump"; }

# Backups share the disk with the Postgres volumes: refuse when free space is under 2x the databases.
need=$(( 2 * ($(db_bytes api-db) + $(db_bytes keycloak-db)) ))
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
  curl -fsS --retry 3 -T "/opt/backups/$run/$db.dump" "${BACKUP_PAR_URL}$run/$db.dump"
done

# Keep the last 3 runs locally; the bucket keeps 14 days.
find /opt/backups -mindepth 1 -maxdepth 1 -type d -name '2*' | sort | head -n -3 | xargs -r rm -rf --
echo "backup $run uploaded"
