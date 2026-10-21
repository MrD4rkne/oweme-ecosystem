# Groups

**Goal:** a user creates a group of personas that share expenses.

## Rules
- The creator owns the group and is the only one who can read or change it.
- A group exposes nothing to anyone else: 404, never 403.
- Name is required, max length in `GroupConstants`.

## Membership
- Only the creator adds members.
- Any existing persona can be added.
- Adding an existing member is idempotent (204).
- No removal.

## Endpoints
| Method | Route | Success | Failures |
|---|---|---|---|
| POST | /api/groups | 201 | 400 |
| GET | /api/groups/{id} | 200 | 404 |
| POST | /api/groups/{id}/members | 204 | 404 group or persona |

## Open questions
- Should members be able to read the group?
- Should adding a persona require owning it?
