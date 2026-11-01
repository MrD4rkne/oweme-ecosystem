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
- Expand/contract: never drop or rename a column/table in the same release as the code that stops using it. Production deploys automatically.
- Deploys never go backwards: an image older than the running `API_IMAGE_TAG` is skipped. Roll forward with a revert commit; force a downgrade only by hand with `-e allow_downgrade=true`.
- Images older than the `migrate` command cannot be redeployed through this playbook; the migrator exits 1.

## Backups and restore

Every API deploy dumps the API database to `/opt/backups/api-<tag>-<utc time>.dump` (root-only, last 10 kept) before migrating, and refuses to deploy when free disk is under 2x the database size. Backups stay on the VM: losing the VM loses them too.

Restore (on the VM, as root):

```bash
cd /opt/apps && docker compose stop api
docker compose exec -T api-db sh -c 'pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --clean --if-exists' < /opt/backups/<file>.dump
# set API_IMAGE_TAG in .env to the tag in the file name, then:
docker compose up -d api
```

## CD pipeline

Push to `main` (api/**) builds the image, deploys staging, runs `OweMe.Api.SmokeTests` against it, then deploys production; no approvals. Full provision (`configure-all.yml`) runs staging on infra pushes and production only on manual dispatch.

Per-environment GitHub settings: variable `API_URL` (both), variable `IDP_REALM_URL` and secrets `SMOKE_USER` / `SMOKE_PASSWORD` (staging). The smoke user lives in realm `oweme-staging`, created by hand: first/last name, verified email, enabled, no admin roles.
