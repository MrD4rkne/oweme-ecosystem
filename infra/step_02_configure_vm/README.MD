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

## CD and backups

Push to `main` (api/**) deploys staging, runs `OweMe.Api.SmokeTests` against it, then deploys production. Staging needs env variables `API_URL`, `IDP_REALM_URL` and secrets `SMOKE_USER` / `SMOKE_PASSWORD` (user in realm `oweme-staging`: first/last name, verified email, no admin roles).

Each API deploy first dumps the API database to `/opt/backups/api-<utc time>.dump` (root-only, last 10, on the VM only). Restore, setting `API_IMAGE_TAG` in `.env` to the image deployed at that time (see the environment's deployment history):

```bash
cd /opt/apps && docker compose stop api
docker compose exec -T api-db sh -c 'pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --clean --if-exists' < /opt/backups/<file>.dump
docker compose up -d api
```

