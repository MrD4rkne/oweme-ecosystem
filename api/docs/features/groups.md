# Groups

**Goal:** a user creates a group of personas that share expenses.

## Rules
- The creator owns the group and is the only one who can see or change it.
- Name is required.

## Membership
- Only the creator adds members.
- A persona can be added only if the caller created it or is linked to it (`UserId`); otherwise 404.
- Adding an existing member again is a no-op.
- No removal.

## Endpoints
| Method | Route                    | Success | Failures                                                             |
|--------|--------------------------|---------|----------------------------------------------------------------------|
| POST   | /api/groups              | 201     | 400                                                                  |
| GET    | /api/groups/{id}         | 200     | 404 (also when not the creator)                                      |
| POST   | /api/groups/{id}/members | 204     | 403 not allowed to edit, 404 group or persona (missing or not yours) |

## Open questions
- Should members be able to see the group?