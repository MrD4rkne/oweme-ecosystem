# Step 2: configure VM

Inventories live in `inventory/staging` and `inventory/production`; secrets in each `vars/<env>/vault.yml`.

```bash
ansible-playbook site.yml -i inventory/staging/hosts.ini --vault-password-file .vault_pass
```

Deploy only (what CI runs): add `--tags deploy -e image_tag=<tag> -e target_app=api`.

## API migrations

The deploy runs `api-migrator` (the api image with the `migrate` command) before recreating `api`; a failed migration stops the deploy and the old api keeps serving.

- Manual deploy on the VM: `docker compose run --rm api-migrator && docker compose up -d` (plain `up` skips the migrator).
- The old api serves on the new schema until the new one is up, and rollback never reverts migrations: keep each migration compatible with the previous release.
- Images older than the `migrate` command cannot be redeployed through this playbook; the migrator exits 1.

## CD

Push to `main` (api/**) deploys staging, runs `OweMe.Api.SmokeTests` against it, then deploys production. Staging needs env variables `API_URL`, `IDP_REALM_URL` and secrets `SMOKE_USER` / `SMOKE_PASSWORD` (user in realm `oweme-staging`: first/last name, verified email, no admin roles).

## Backups and restore

`/usr/local/sbin/oweme-backup.sh` dumps `api-db` and `keycloak-db` into `/opt/backups/<run>/` (root-only, last 3 runs) and uploads both to the environment's bucket (`<stack>-backups`, kept 14 days). It runs nightly at 03:00 UTC (cron, `journalctl -t oweme-backup`) and before every migration; the deploy fails if it fails. `.github/workflows/backup-check.yml` fails daily when a bucket has no fresh pair.

Restore a run (both databases, as a pair: api rows are keyed by the Keycloak `sub`):

```bash
sudo -i
source /etc/oweme-backup.env
curl -fsS -K - <<< "url = \"${BACKUP_PAR_URL}\"" | jq -r '.objects[].name' | grep -oE '^[0-9]{8}T[0-9]{6}Z' | sort -u   # runs in the bucket
oweme-restore.sh <run>
```

The script fully reads both dumps, asks for confirmation, stops `api` and `keycloak`, takes a safety dump of the current state, recreates and restores both databases, starts Keycloak, migrates the restored api schema forward with the current image (`API_IMAGE_TAG` in `/opt/apps/.env`) and starts the api. It prints the safety run id: `oweme-restore.sh <safety run>` undoes the restore. A failure before the confirmation changes nothing. If the safety dump fails, the apps that were running are started again. A failure after that leaves both apps stopped and prints the safety run to restore.

- Don't deploy during a restore: disable the workflow first (`gh workflow disable "Deploy — api"`, `enable` after).
- Restore staging only from staging dumps: production dumps carry real users and client secrets.
- Keycloak comes back exactly as it was in the dump, admin password included (`keycloak.env` only sets the initial admin). If `vault_keycloak_admin_password` changed since, set it again in Keycloak, or the next full provision's realm step can't log in. The api uses no client secret. A dump from a newer Keycloak won't start on an older image.
- The safety dump needs disk space and the bucket. If it can't run (disk full, bucket unreachable), fix that first.
- If a restore stops after `dropdb` and before `createdb` (the database is gone), create it by hand, then rerun: `cd /opt/apps && docker compose exec -T api-db sh -c 'createdb -U "$POSTGRES_USER" "$POSTGRES_DB"'` (or `keycloak-db`).
