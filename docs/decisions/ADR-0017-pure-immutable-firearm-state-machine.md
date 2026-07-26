# ADR-0017 — Establish a pure immutable firearm state machine before item persistence

- **Status:** Accepted
- **Date:** 2026-07-13
- **Decision owners:** Kingmaker Gunslinger project

## Context

Loaded rounds, selected ammunition, and damage condition are mutable facts of one firearm item. Cowboys and Demons demonstrates a workable wielder-buff shortcut, but that approach cannot distinguish two identical guns, safely transfer a loaded gun, or preserve independent condition across equipment changes.

The project also does not yet know which Kingmaker persistence mechanism is safe. Choosing storage before defining valid state would couple combat rules to an unproven save implementation.

## Decision

Define an immutable `FirearmState` and a pure `FirearmStateMachine` before attaching anything to `ItemEntityWeapon`.

- State contains only schema version, loaded rounds, ammunition identity, and Normal/Broken/Wrecked condition.
- Capacity and compatibility are supplied through immutable `FirearmStateRules`.
- Every legal transition returns a new state.
- Every illegal transition throws without changing the input.
- Wrecked state is always empty and cannot use ordinary repair.
- A primitive `FirearmStateData` DTO defines strict conversion semantics without selecting a save location.
- No character, buff, item, blueprint, inventory, Unity object, or Kingmaker type appears in the state files.

## Consequences

### Positive

- Combat, reload, and persistence adapters can share one deterministic contract.
- Rejected operations cannot leave partially mutated state.
- Two item instances can later carry independent values without changing transition logic.
- Saved data can be validated against current capacity and ammunition compatibility.
- Persistence experiments in Sprint 12 can be replaced without rewriting firearm rules.

### Negative

- Callers must replace an old state with the returned state; ignoring the return value has no effect.
- Multi-ammunition chambers are intentionally unsupported in the first schema.
- Wrecked reconstruction requires a future explicit transition and migration policy.
- The DTO shape is not sufficient by itself to prove save safety.

## Rejected alternatives

### Store state in a wielder buff

Rejected because state would follow the unit rather than the gun and would collapse multiple firearms into one flag.

### Put mutable fields on `FirearmDefinitionComponent`

Rejected because blueprint components are shared definition data, not per-item state.

### Choose a Kingmaker save mechanism first

Rejected because the storage API remains unverified and should not define legal firearm behavior.

### Mutate one `FirearmState` object in place

Rejected because failed or interrupted transitions could expose partial state and complicate rollback, testing, and persistence.
