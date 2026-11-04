#!/usr/bin/env bash
# Fails unless the bucket behind PAR_URL (read-only PAR) has an api+keycloak dump pair from the last 26 hours.
# Used by .github/workflows/backup-check.yml (issue #101). Env: PAR_URL, SECRET_NAME (for the error message).
set -euo pipefail

if [[ -z "${PAR_URL:-}" ]]; then
  echo "::error::Secret $SECRET_NAME is not set"
  exit 1
fi

# Objects are <run>/{api,keycloak}.dump with run = UTC yyyymmddThhmmssZ, so names sort by time.
# start= lists only names from the cutoff on, so the 1000-object page limit can't hide fresh runs.
cutoff=$(date -u -d '26 hours ago' +%Y%m%dT%H%M%SZ)
latest=$(curl -fsS --proto '=https' --retry 3 --max-time 60 -K - <<< "url = \"${PAR_URL}?start=${cutoff}\"" \
  | jq -r '.objects[].name' \
  | grep -E '^[0-9]{8}T[0-9]{6}Z/(api|keycloak)\.dump$' \
  | cut -d/ -f1 | sort | uniq -c | awk '$1 == 2 { print $2 }' | tail -n1 || true)

if [[ -z "$latest" || "$latest" < "$cutoff" ]]; then
  echo "::error::No complete backup since $cutoff (latest complete run: ${latest:-none})"
  exit 1
fi
echo "Latest complete backup: $latest"
