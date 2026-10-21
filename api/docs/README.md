# Docs

- `domain.MD` – ER diagram. Update it in the same PR as a migration (nothing enforces this).
- `features/<name>.md` – one file per feature: goal, rules, endpoints, open questions. Open an issue first; write or update the spec in the same PR as the code. Copy `groups.md` for the shape.
- Decisions below. Append only; mark replaced ones "superseded by".

## Decisions

### Wolverine as in-process mediator (2025)
Chosen over MediatR because it is free and the same handlers can later run over a message broker. Commands and queries are records handled by static `Handle` methods. Cross-cutting concerns (validation, user context, timing) are Wolverine middleware, never handler code.

### Keycloak for identity (2026-09, PR #29, replaced Duende)
The API does not own users. It reads `sub` as `UserId` and `email` from the JWT. There is no Users table; `Persona.UserId` is the Keycloak subject.

### Strongly typed ids (2026-10)
Every entity id is a `[StronglyTypedId]` Guid struct. Endpoints bind them directly from route and body. Entity configurations register the converter for their own id; audit columns get it once in the base DbContext. Known cost: the generated NSwag client sees plain `Guid`.

### How handlers report outcomes (2026-10, commit b031eb9)
Commands return a result enum and the endpoint maps it to a status code.
