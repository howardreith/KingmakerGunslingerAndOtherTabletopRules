# ADR-0006: Per-Item Firearm State Repository

- **Status:** Accepted boundary; storage implementation deferred
- **Date:** 2026-07-12

## Context

Loaded rounds, ammunition type, and damage condition must follow an individual firearm through equipment changes, transfers, stash storage, and save/load. Cowboys and Demons’ wielder-buff shortcut is insufficient for this scope.

## Decision

Define versioned `FirearmState` and access it exclusively through `IFirearmStateRepository`. Evaluate native item-attached serialization, persistent enchantments, an external keyed manifest, and a hybrid. Select only after a save/load spike proves behavior.

## Consequences

- Combat/services can be built independently of the final persistence technique.
- The project accepts extra early design work to avoid state leakage later.
- Sprint 12 is a formal go/no-go gate.

## Rejected alternative

A persistent buff on the wielder representing loaded or damaged firearm state.
