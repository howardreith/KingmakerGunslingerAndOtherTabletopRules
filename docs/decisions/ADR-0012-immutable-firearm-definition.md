# ADR-0012 — Immutable firearm definition and passive blueprint marker

- **Status:** Accepted
- **Date:** 2026-07-12
- **Decision owners:** Kingmaker Gunslinger project

## Context

Firearms need an identity independent of any vanilla category reused for animation or low-level engine compatibility. At the same time, loaded ammunition, damage state, and jams differ between individual item instances and cannot safely live on a shared blueprint component.

Cowboys and Demons demonstrates that firearm mechanics can be organized around weapon metadata, while Call of the Wild demonstrates that a custom Kingmaker marker can derive directly from `BlueprintComponent`. The project still requires a stricter separation between shared definition data and mutable item state.

## Decision

Create an immutable `FirearmDefinition` value object and a passive, publicly discoverable `FirearmDefinitionComponent` that stores only serializable definition fields. Its configuration API remains internal; public type visibility is limited to the Unity/Kingmaker component adapter.

- Firearm rules identify a weapon by the marker component, never solely by a borrowed `WeaponCategory`.
- Construction validates every invariant before a marker can be created.
- The component reconstructs a validated definition rather than exposing mutable public fields.
- Loaded rounds, ammunition type, condition, owner, and event state are prohibited from the component.
- The component registers no event handlers and requires no blueprint GUID by itself.

## Consequences

### Positive

- A crossbow compatibility shell cannot accidentally become the authoritative firearm identity.
- Invalid firearm blueprints fail during construction rather than later in combat.
- Definition equality supports deterministic tests and future caches.
- Per-item state remains free to use the safest persistence mechanism discovered in Sprint 12.

### Negative

- The component duplicates primitive fields instead of serializing the immutable object directly.
- Kingmaker's exact component serialization/cloning behavior still requires runtime validation when the Test Musket is attached in Sprint 6.
- The initial kind/era matrix is intentionally conservative and may need a schema migration for exotic future weapons.

## Rejected alternatives

### Use `WeaponCategory.HandCrossbow` as firearm identity

Rejected because every unrelated engine and mod consumer of that category would implicitly treat firearms and hand crossbows as equivalent.

### Store loaded state on the marker

Rejected because blueprint components are shared definition data, not individual item state.

### Store one mutable definition object

Rejected because later code could silently change range, capacity, or misfire values after registration.
