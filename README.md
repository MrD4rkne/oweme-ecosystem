# OweMe

[![Build Nightly](https://github.com/MrD4rkne/oweme-ecosystem/actions/workflows/build-nightly.yml/badge.svg)](https://github.com/MrD4rkne/oweme-ecosystem/actions/workflows/build-nightly.yml)

Manage your debts and credits with ease. OweMe is a simple and intuitive application that helps you keep track of who owes you money and who you owe money to.

## How to run it

Both options need Docker.

### Aspire (dev loop: dashboard, logs, traces)

```bash
dotnet run --project api/aspire/OweMe.AppHost
```

### Docker compose (builds the API from source)

```bash
docker compose up --build
# with logs and traces in Seq (http://localhost:8081):
docker compose -f compose.yaml -f compose.observability.yaml up --build
```

API: http://localhost:5000, Keycloak: http://localhost:8080 (realm `oweme-dev`, user `apiuser` / `Password123!`).

## API

API part of the system, you can find more information in the [API readme](./api/README.md).

## Identity

Keycloak, configured in `compose.yaml`. The dev realm is imported from `auth/realms/dev.json`.

## Dependency updates

Dependabot (`.github/dependabot.yml`) opens weekly NuGet PRs grouped by family (`nuget-efcore`, `nuget-aspire`, `nuget-microsoft`, `nuget-other`, minor and patch together), with a 3-day cooldown on new releases.
`.github/workflows/dependabot-auto-merge.yml` enables auto-merge for any group named `nuget-*`; it merges once the required checks on `main` pass.
Major NuGet bumps and GitHub Actions PRs are not grouped and need a manual merge.
Smoke tests are not part of PR CI; the nightly workflow runs them against `docker compose`, so Keycloak/JWT regressions from an auto-merge show up the next day.

## License

See [License file](./LICENSE)
