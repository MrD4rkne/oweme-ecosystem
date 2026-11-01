# Working on OweMe

Layout, stack and run instructions: [README.md](README.md), [api/README.md](api/README.md). Specs and decisions: [api/docs/README.md](api/docs/README.md).

Every feature PR: spec → code → integration tests → regenerate the client.

## Feature specs

One file per feature in `api/docs/features/<name>.md`; `groups.md` is the reference shape. Update an existing spec in place; create a file only for a new feature.

1. Read the issue and the code it touches (`OweMe.Api/Endpoints/`, `OweMe.Application/`, `OweMe.Domain/`) so the spec matches reality or names the delta.
2. Rules must not contradict the Decisions in `api/docs/README.md`; if they must, append a new decision there and mark the old one "superseded by".
3. Shape: `**Goal:**` one sentence; `## Rules` one testable rule per bullet; `## Endpoints` table (Method, Route, Success, Failures); `## Open questions` only if something is undecided. When a question is decided, move it into Rules.
4. Tables changed → update `api/docs/domain.MD` in the same PR.

Status codes:
- 404 when the caller must not learn the entity exists (missing, stranger, foreign persona). 403 only when the caller can see it but may not change it.
- 400 for validation (FluentValidation runs on every command); list the rules in the spec.
- Create → 201 with Location; change with no body → 204; read → 200. Every failure is problem+json.
- Money is `decimal`, never `double`; state the rounding rule whenever an amount is divided.
- Every list endpoint is paged, from the first one. Reuse the paging request/response shape the first list endpoint introduces.

## Integration tests

Project `api/api/tests/OweMe.IntegrationTests/`, one file per endpoint in a folder per feature; `Groups/AddMemberEndpointTests.cs` is the reference.

- `OweMeApi` is an assembly fixture: one Postgres container, migrated once, shared by all tests. Isolate by creating fresh users and data per test, never by cleaning the DB.
- Auth is faked: `client.AsUser(new TestUser())` is a new random user, `client.AsAnonymous()` removes auth. Wrap with `api.WithTestLogging(output)`.
- Names `For_<Condition>_Should_<Outcome>`, Arrange/Act/Assert comments, Shouldly, `TestContext.Current.CancellationToken` on every async call.
- Arrange through the public API with private helpers (`CreateGroup`, `CreatePersona`); use the real request/response types, not hand-written DTOs. Check the DB only when the response cannot show the effect.
- Strongly typed ids: `new XId(Guid.NewGuid())` for "does not exist", `XId.Empty` for validation failures.

Every endpoint gets: happy path (status and effect), 400 per validation rule, 404 missing, 404 stranger (second `TestUser`, assert nothing changed), 403 only where the spec says so, no-op/idempotency rules, and an entry in `ApiShouldRequireAuthentication.cs`. Each spec Rule maps to a test. Pure logic (splits, rounding, balances) also gets unit tests in `OweMe.Domain.UnitTests` / `OweMe.Application.UnitTests`.

Run (Docker required):

```bash
dotnet test --project api/api/tests/OweMe.IntegrationTests/OweMe.IntegrationTests.csproj
dotnet test --project api/api/tests/OweMe.IntegrationTests/OweMe.IntegrationTests.csproj --filter-class "*AddMemberEndpointTests"
```

Report failures verbatim; never skip a test to get green.
