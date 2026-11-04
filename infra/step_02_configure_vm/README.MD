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

The script checks both dumps, asks for confirmation, takes a safety dump of the current state, stops `api` and `keycloak`, recreates and restores both databases, starts Keycloak, migrates the restored api schema forward with the current image and starts the api. To undo a restore, restore the safety run (the newest one after the restore). Restore staging only from staging dumps: production dumps carry real users and client secrets. The Keycloak admin password comes back from the dump; if the vault password changed since, update it in Keycloak before the next full provision.
