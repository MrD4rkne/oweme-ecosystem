# OweMe API

.NET web API. Specs and decisions live in [docs/](./docs/README.md).

## Projects

```text
api/src/
├── OweMe.Api/             # endpoints
├── OweMe.Application/     # commands, queries, handlers, validation
├── OweMe.Domain/          # entities
├── OweMe.Infrastructure/  # external services (empty so far)
└── OweMe.Persistence/     # EF Core, PostgreSQL, migrations
api/tests/                 # unit, integration (Testcontainers), smoke
client/                    # NSwag-generated API client
```

## Stack

.NET 10, PostgreSQL, Wolverine (mediator), FluentValidation, OpenAPI + Scalar + NSwag, Docker, GitHub Actions.

## Run locally

Needs the .NET 10 SDK and Docker.

```bash
git clone https://github.com/MrD4rkne/oweme-ecosystem/
cd oweme-ecosystem/api
docker compose -f compose.yaml -f compose.override.yaml up --build
```
