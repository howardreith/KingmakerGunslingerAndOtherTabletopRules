# ADR-0018: Use a weak process-local repository for the first item-state binding

- **Status:** Accepted for Sprint 11; not accepted as the final persistence mechanism
- **Date:** 2026-07-13

## Context

The pure state machine from Sprint 10 needs to be associated with exact runtime firearm items before save persistence is investigated. Blueprint identity, item names, equipment slots, and wielder buffs cannot distinguish two copies of the same firearm.

A normal dictionary keyed by item entities would provide reference identity but would also retain discarded Kingmaker objects for the remainder of the process.

## Decision

Introduce `IFirearmStateRepository` as the exclusive state-access boundary and provide `WeakFirearmStateRepository` for Sprint 11.

The implementation uses:

```text
ConditionalWeakTable<object, Entry>
```

Each entry owns one immutable `FirearmState`, a revision, and a process-local diagnostic ID. State access is guarded by `FirearmItemStateService`, which first requires a concrete `ItemEntityWeapon` whose exact weapon type has one firearm marker.

## Consequences

- Two value-equal or blueprint-identical item instances remain independent.
- Equip, unequip, weapon-set changes, and party transfer retain state when Kingmaker retains the same object reference.
- Discarded item objects are not intentionally kept alive by the repository.
- Native Heavy Crossbows cannot receive state through the supported service boundary.
- Repository IDs are suitable for logs but not for serialization.
- State is lost if Kingmaker reconstructs a new item object during save/load or another lifecycle operation.

## Explicit non-decision

This ADR does not choose the final save persistence mechanism. Sprint 12 must compare native item-attached data, dynamic enchantment/state tokens, and a mod-owned serialized registry against real Kingmaker save/load behavior.

## Rejected alternatives

### Blueprint-keyed state

Rejected because every copy of one firearm blueprint would share state.

### Wielder buff

Rejected because state would follow the character rather than the gun and would fail weapon switching and transfer.

### Strong dictionary keyed by item object

Rejected for this spike because it would retain discarded game objects indefinitely.

### Persistent string key selected without a save/load spike

Rejected because guessed IDs can change, collide, or be regenerated when Kingmaker reconstructs item entities.
