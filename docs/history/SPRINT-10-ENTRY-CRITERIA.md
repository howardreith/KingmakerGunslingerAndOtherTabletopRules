# Sprint 10 entry criteria — pure per-item firearm state machine

## Goal

Create a game-independent immutable/state-transition model for loaded rounds, loaded ammunition identity, and firearm condition without yet attaching that state to a Kingmaker item.

## Preferred runtime gate

A compiled Sprint 9 package should eventually confirm close/far touch-AC behavior before the firearm system is promoted as runtime-complete. Sprint 10's pure model may proceed independently because it introduces no Kingmaker persistence or gameplay mutation.

## Allowed work

- `FirearmState` with schema version, loaded rounds, loaded ammunition ID, and condition.
- Conditions limited to normal, broken, and wrecked.
- Pure transitions for load, fire, misfire damage, repair, and wreck.
- Capacity and ammunition compatibility inputs.
- Deterministic serialization DTO shape without choosing a Kingmaker persistence mechanism.
- Dependency-free tests and modeled transition matrices.

## Forbidden work

- Attaching state to a character buff.
- Attaching or serializing state on a Kingmaker item.
- Inventory consumption.
- Reload abilities or action economy.
- Misfire roll interception or explosion damage.
- Vendors, crafting, grit, or class progression.

## Acceptance

1. Empty normal firearm can load a compatible round up to capacity.
2. Loaded firearm can consume exactly one round per fire transition.
3. Invalid over-capacity and incompatible-ammunition transitions are rejected without mutation.
4. Normal can become broken; broken can become wrecked.
5. Broken can be repaired to normal; wrecked cannot silently become normal.
6. Loaded ammunition and condition remain properties of the state object, never a unit.
7. All transitions are deterministic and covered by dependency-free tests.
