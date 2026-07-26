# Sprint 13 completion report — save-owned UnitPart firearm-state vault

## Status

```text
Milestone:                 0.0.13-s13-unitpart-vault
Source implementation:     COMPLETE
Portable validation:       PASSED
Compiled Kingmaker build:  NOT PRODUCED
UMM install package:       NOT PRODUCED
Persistence gate decision: NO-GO pending the full in-game lifecycle matrix
Ammunition/reload work:    BLOCKED
```

**NOT READY FOR KINGMAKER.** This repository contains source code and documentation, not a compiled Unity Mod Manager package.

## Why Sprint 13 remained a persistence sprint

Sprint 12 implemented item-owned state tokens as a concrete persistence candidate, but the current environment could not compile or run the required Kingmaker lifecycle matrix. The Sprint 12 gate therefore remained NO-GO. The sprint plan explicitly forbids proceeding into ammunition and reload work while exact per-gun persistence remains unproven.

Sprint 13 implements the next evidence-backed carrier without weakening the core rules:

- state still belongs to the exact firearm, not its wielder;
- no blueprint, display name, equipment slot, or guessed item identifier is used as a key;
- the four stable Sprint 12 token GUIDs remain reserved and readable for migration;
- new state writes do not use the legacy token carrier;
- failures preserve evidence rather than silently selecting one carrier.

## Delivered architecture

### Save-owned UnitPart vault

`UnitPartFirearmStateVault` is anchored to the current main-character entity as part of the save graph. It stores a list of records shaped as:

```text
FirearmStateVaultRecord
  RecordSchemaVersion
  exact ItemEntityWeapon reference
  primitive FirearmStateData
```

The main character is only the save-owned host. The record key is the exact `ItemEntityWeapon` reference, so changing wielders, slots, or inventories is not intended to change ownership of state.

The vault:

- compares item keys with `ReferenceEquals`;
- rejects duplicate records for one exact item reference;
- defensive-copies every mutable state DTO at its boundary;
- treats canonical empty/Normal as absence of a record;
- uses expected-current replacement semantics;
- rolls its record list back if a replacement fails verification;
- prunes deserialized records whose item reference is null;
- exposes no gameplay behavior of its own.

### Vault-backed repository

`VaultBackedFirearmStateRepository` preserves the existing `IFirearmStateRepository` contract. Its durable source of truth is the save-owned vault. A `ConditionalWeakTable` remains only for process-local diagnostic identity and revision counters.

The repository supports:

- reconstruction from a vault record when no process-local metadata exists;
- independent state for value-equal and blueprint-identical item objects;
- atomic Set and Transition operations;
- no-op suppression;
- canonical empty-state record removal;
- verification after every write;
- exact-item removal without a permanent external dictionary.

### One-way migration from Sprint 12 tokens

`MigratingFirearmStateRepository` wraps the vault repository. On the first normal read or write for a firearm carrying a Sprint 12 state token, it follows one of four paths:

| Existing state | Result |
|---|---|
| Legacy token only | Decode token, write vault, verify vault, clear token, verify cleanup |
| Vault and equivalent token | Preserve vault and clear the redundant token |
| Vault and conflicting token | Preserve both carriers and fail closed |
| Invalid, unknown, or duplicate token | Preserve the token evidence and fail closed |

If token cleanup fails after a new vault write, the migration attempts to remove that vault record. A rollback failure is counted and surfaced; it is never concealed.

New state operations are delegated only to the vault repository. The token blueprints and adapter remain present solely for compatibility and migration diagnostics.

### Development migration fixture

The future compiled UMM panel includes a development-only action that stamps one supported Sprint 12 token on an equipped Test Musket without writing a Sprint 13 vault record. The next ordinary state read must then exercise the real migration path. This makes migration testable without relying on an unpublished Sprint 12 binary.

## Stable blueprint IDs

Sprint 13 adds no blueprint IDs and changes no existing GUID.

```text
Manifest entries: 12
Active entries:    8
Reserved entries:  4
New Sprint 13 IDs: 0
```

The four Sprint 12 state-token enchantments remain active because old saves must be able to resolve and migrate them. They are no longer the carrier for new writes.

## Tests and portable validation

Sprint 13 adds 53 named dependency-free C# cases:

- 5 state-DTO utility cases;
- 25 vault-backed repository cases;
- 23 migration cases.

The complete harness now declares **292 unique cases**.

Portable validation also includes an independent Python model covering:

- canonical-empty absence;
- exact-reference isolation;
- value-equal item isolation;
- reconstruction from the save-owned vault;
- simulated save-graph reference rebinding;
- compare-and-replace conflict detection;
- vault write rollback;
- migration and verified token cleanup;
- equivalent-carrier cleanup;
- conflict preservation;
- unknown and duplicate token preservation;
- token-clear rollback;
- rollback-failure accounting;
- one-time migration;
- native-crossbow rejection before persistence access.

The validator parses all C# and PowerShell source, validates JSON and MSBuild declarations, checks local documentation links, verifies stable GUIDs, enforces source-layer boundaries, and rejects packaged binaries or local game paths.

## What remains unproven

The source work does not prove that Kingmaker will:

- serialize this custom `UnitPart`;
- preserve its concrete type metadata across a process restart;
- restore direct `ItemEntityWeapon` references to the same reconstructed item objects;
- keep records valid through stash, party transfer, area transition, rest, respec, merchants, and item reconstruction;
- serialize the nested `FirearmStateData` shape exactly as expected;
- tolerate a missing or future mod version without making the save unloadable;
- preserve a sold item because the vault directly references it without unacceptable save growth;
- coexist with mods that rebuild units, inventories, or item entities.

Those are blocking runtime questions, not documentation issues.

## Gate decision

**NO-GO.** Sprint 14 may add ammunition items only after a real compiled UMM package passes every critical row in [`docs/PERSISTENCE-TEST-MATRIX.md`](../PERSISTENCE-TEST-MATRIX.md). A source-only model cannot promote the carrier.

If the UnitPart carrier fails, Sprint 14 remains a persistence sprint and must preserve both the `IFirearmStateRepository` contract and the four legacy token GUIDs.
