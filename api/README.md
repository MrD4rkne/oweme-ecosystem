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

See [the root README](../README.md): Aspire AppHost or Docker compose.
