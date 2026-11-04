# Setup VM

Pulumi (C#) for the OCI VM, network and backup bucket. One project, `OciVmInfra`, two stacks in Pulumi Cloud:

| Stack | Environment |
|---|---|
| `oweme-s` | staging |
| `oweme-p` | production |

Stack settings live in `Pulumi.<stack>.yaml` under the `OciVmInfra:` prefix, read with `new Config()`.

## One-time setup

1. OCI CLI config in `~/.oci/config` ([SDK config](https://docs.oracle.com/en-us/iaas/Content/API/Concepts/sdkconfig.htm)).
2. [Install Pulumi](https://www.pulumi.com/docs/get-started/install/), then `pulumi login`.

## Commands

Run from this directory. Always pass `-s`, so you never act on the wrong stack by accident.

```bash
pulumi stack ls                                  # stacks in this project
pulumi preview -s oweme-s --diff                 # what would change; read-only
pulumi up -s oweme-s                             # apply (shows the preview and asks first)
pulumi preview -s oweme-s --refresh              # compare with the real cloud; expect no changes
pulumi stack output -s oweme-s                   # outputs (secrets masked)
pulumi stack output BackupParUrl -s oweme-s --show-secrets
pulumi config -s oweme-s                         # this stack's settings
pulumi stack export -s oweme-s --file state.json #  back up the state before risky operations
```

## Workflow for a change

1. `pulumi preview` both stacks. Read every line: `+` create, `~` update, `+-` **replace** (delete, then recreate), `-` delete.
2. Any replace or delete on the VM, VCN, bucket or a PAR → stop and find out why.
3. `pulumi up -s oweme-s`, check it works, then `pulumi preview -s oweme-s --refresh` must show no changes.
4. Same for `oweme-p`.

## Gotchas

- **PARs (backup URLs) can't be updated.** Any input change replaces them and issues new URLs, so the vault copies (`backup_par_url`) must be updated. Keep `TimeExpires` a literal.
- **First `up` after adding the IAM policy** can fail on the lifecycle rule while IAM applies. Run it again.
- **The bucket refuses deletion while it has objects.** That protects the backups from `pulumi destroy`.
