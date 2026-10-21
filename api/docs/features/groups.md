# Groups

**Goal:** a user creates a group of personas that share expenses.

## Rules
- The creator owns the group and is the only one who can see or change it.
- Name is required.

## Membership
- Only the creator adds members.
- Any existing persona can be added.
- Adding an existing member again is a no-op.
- No removal.

## Endpoints
| Method | Route | Success | Failures |
|---|---|---|---|
| POST | /api/groups | 201 | 400 |
| GET | /api/groups/{id} | 200 | 404 (also when not the creator) |
| POST | /api/groups/{id}/members | 204 | 404 group or persona |

## Open questions
- Should members be able to see the group?
- Should adding a persona require owning it?
