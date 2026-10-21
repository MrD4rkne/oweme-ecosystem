# Docs

- `domain.MD` – ER diagram. Update it in the same PR as a migration.
- `features/<name>.md` – one file per feature: goal, rules, endpoints, open questions. Open an issue first; write or update the spec in the same PR as the code. Copy `groups.md` for the shape.
- Decisions below. Append only; mark replaced ones "superseded by".

## Decisions

### Wolverine as in-process mediator (2025)
Chosen over MediatR because it is free and the same handlers can later run over a message broker. Commands and queries are records handled by static `Handle` methods. Cross-cutting concerns (validation, user context, timing) are Wolverine middleware, never handler code.

### Keycloak for identity (2026-09, PR #29, replaced Duende)
The API does not own users. It reads `sub` as `UserId` and `email` from the JWT. There is no Users table; `Persona.UserId` is the Keycloak subject.

### Strongly typed ids (2026-10)
Every entity id is a `[StronglyTypedId]` Guid struct, bound directly from route and body. Known cost: the generated NSwag client sees plain `Guid`.

### How handlers report outcomes (2026-10)
Rule: expected outcomes (not found, not allowed) are results; broken invariants and infrastructure failures throw.

- Commands return a result enum and the endpoint maps it to a status code. Failures are written with `Results.Problem` so every error body is problem+json. 
- Known cost: an enum carries no payload and `null` carries no reason. When an operation needs both a payload and several distinct failures, switch to a union-style result (OneOf, ErrorOr).
