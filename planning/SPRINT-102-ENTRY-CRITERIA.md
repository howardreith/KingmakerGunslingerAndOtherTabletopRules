# Sprint 102 live inventory transfer

Extend the already-qualified guarded `gunslinger-starting-items` transaction to
move the exact request-created battered pistol from the working character's
native inventory into a detached native unit inventory and back with exact
`ItemsCollection.Extract/Add` calls.

The same item must retain its item-owned origin and state-token isolation. A
transfer flag drives guaranteed return in `finally` before the existing exact
grant rollback. No save-writing API is permitted, and all original inventory,
class, gold, and money evidence remains mandatory.
