# ADR-0021 — Use Kingmaker's item identity as the Sprint 14 vault key

**Status:** Provisional; runtime lifecycle gate remains NO-GO
**Date:** 2026-07-13

## Context

Sprint 11 associated state with exact runtime object references. Sprint 12 tested finite enchantment tokens. Sprint 13 moved arbitrary state into a custom UnitPart but stored a direct `ItemEntityWeapon` reference in each record.

None could be promoted without a compiled lifecycle test. The next candidate must avoid depending on object reference preservation while still distinguishing two blueprint-identical guns.

## Decision

Use the engine-issued `ItemEntityWeapon.UniqueId` value as the candidate key for primary UnitPart records.

The adapter:

- accepts only exactly named `UniqueId`;
- accepts only Guid or string values;
- requires a nonempty standard GUID;
- canonicalizes to lowercase D form;
- never writes or generates an identity;
- fails closed when the contract is unavailable.

Keep Sprint 13 direct-reference records and Sprint 12 tokens only as one-way migration sources.

## Consequences

### Positive

- Primary records contain only primitive serializer data.
- Runtime item reconstruction can recover state when the engine ID is stable.
- Two identical weapon blueprints remain independent.
- The mod does not maintain a separate item-ID issuance system.
- Legacy carriers can be removed only after verified migration.

### Negative

- Correctness now depends on undocumented installed-engine behavior.
- Sale, duplication, deletion, and reconstruction semantics are unknown.
- A missing ID blocks firearm-state access rather than degrading gracefully.
- The custom UnitPart itself is still unproven.

## Rejected alternatives

- **Generate a mod GUID:** no proven hook exists to atomically assign and serialize it for every item lifecycle.
- **Blueprint GUID:** aliases every copy of the same gun.
- **Owner and slot:** state moves when equipment moves.
- **Inventory position:** unstable and aliases stacks/reordering.
- **Display name:** nonunique and localizable.
- **Runtime reference hash:** process-local only.
- **Character buff:** assigns state to the wielder, not the weapon.

## Promotion rule

This ADR becomes accepted only after the complete compiled lifecycle matrix passes. Any state transfer between distinct guns, identity instability for the same gun, or inability to restore the UnitPart rejects the carrier.
