# ADR-0019: Provisional item-enchantment state tokens

- **Status:** Provisional; source implementation accepted, runtime architecture NO-GO
- **Date:** 2026-07-13

## Context

Sprint 11 proves that exact object identity can keep two firearm states independent during one process, but `ConditionalWeakTable` cannot survive item reconstruction or process restart.

The next carrier must belong to the gun rather than its wielder or blueprint, must avoid an unbounded global record, and must be testable without committing the entire ammunition and reload subsystem to an unproven save hook.

Kingmaker items already have an enchantment concept, and custom weapon-enchantment blueprints can carry passive marker components. This makes a dynamic no-op enchantment a plausible item-owned persistence token, but not proof of serialization.

## Decision

Implement a finite Sprint 12 persistence candidate using zero or one component-only `BlueprintWeaponEnchantment` on the exact `ItemEntityWeapon`.

Token absence represents empty/Normal. Four stable token blueprints represent the remaining states reachable by a capacity-one Test Musket with one diagnostic ammunition identity.

Use a token-backed repository whose source of truth is the item's enchantment set. Retain a weak table only for process-local diagnostics and revision tracking.

Require add/verify/remove/verify replacement with best-effort restoration of the previous token set. Reject unknown, duplicate, malformed, or foreign tokens.

## Provisional status

This ADR does not declare the carrier durable. The current decision is **NO-GO** until a compiled package passes save/load, process restart, inventory, merchant, deletion, old-save, and presentation tests.

Sprint 13 ammunition and reload work is blocked by that decision.

## Consequences

### Positive

- State is attached to the exact gun rather than a character.
- Two identical blueprints can carry different tokens.
- A reconstructed runtime object can recover state from the item without its old weak-table entry.
- Deleted objects do not require a permanent global registry.
- Stable blueprint GUIDs provide explicit schema/migration anchors.
- The finite diagnostic set is easy to inspect and fail closed.

### Negative

- The engine may not serialize dynamic enchantments as assumed.
- Add-first replacement temporarily exposes two tokens internally.
- Rollback cannot be fully atomic without native transaction support.
- A blueprint per complete state does not scale to many ammunition/capacity combinations.
- Custom enchantments may affect item UI, value, or compatibility.
- Saves containing the custom token blueprints are not uninstall-safe.

## Migration requirement

The four Sprint 12 token GUIDs are permanent once published. A later scalable representation must recognize and migrate them rather than reusing or reinterpreting their IDs.

## Rejected alternatives

### Wielder buff

Rejected because state follows the character rather than the gun.

### Blueprint-keyed global state

Rejected because every copy of one weapon blueprint would share state.

### Process-local weak repository as final storage

Rejected because it cannot survive process restart.

### Strong global item-object dictionary

Rejected because it retains discarded runtime objects and still does not serialize.

### Registry keyed by an unverified item ID

Rejected because the candidate ID may be regenerated, collide, or not serialize.

### Full custom save payload before proving an item identity

Deferred because a payload cannot reliably map back to the correct identical firearm without a proven durable identity or carrier.
