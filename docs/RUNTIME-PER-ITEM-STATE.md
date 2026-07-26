# Runtime per-item firearm state

## Status

Sprint 11 proved the source-level exact-reference association using a process-local weak repository. Sprint 12 tried finite item-enchantment tokens. Sprint 13 keeps the same public service boundary but routes new state to a provisional save-owned `UnitPart` vault and reads tokens only for one-way migration.

Save/load and process-restart durability remain unproven, and firearm state still does not participate in reload or attack resolution. The persistence decision remains NO-GO pending the compiled lifecycle matrix.

## Objective

Two copies of the same Test Musket blueprint must hold different state:

```text
Musket A: loaded=1, ammunition=kmg.debug.lead-ball, condition=Normal
Musket B: loaded=0, ammunition=<none>, condition=Broken
```

The association must not be keyed by blueprint, weapon type, name, wielder, buff, equipment slot, inventory position, or a guessed runtime identifier.

## Current layering

```text
KingmakerFirearmRuntimeItemResolver
        │ verifies concrete ItemEntityWeapon + exactly one firearm marker
        ▼
FirearmItemStateService
        │ supported caller-facing state boundary
        ▼
MigratingFirearmStateRepository
        │ reads legacy tokens, then delegates new operations
        ▼
VaultBackedFirearmStateRepository
        │ immutable state and per-item diagnostics
        ▼
IFirearmStateVaultStore
        ▼
UnitPartFirearmStateVault
        │ direct ItemEntityWeapon reference + FirearmStateData
        ▼
main-character save graph
```

The pure state, vault-repository, and migration-policy files remain independent from Kingmaker and Unity. Only the UnitPart provider/store and runtime item resolver touch engine types.

## Exact identity

The vault stores a direct `ItemEntityWeapon` reference and compares records with `ReferenceEquals`. It never derives persistence identity from `Equals`, ordinary `GetHashCode`, a blueprint, or a display field.

A `ConditionalWeakTable` remains inside the repository only for process-local diagnostic identities and revision counters:

```text
kmg-item-000001
kmg-item-000002
```

Those values are not serialized and are never used to recover a firearm after restart.

## Canonical state and repository semantics

An exact firearm with no vault record reads as empty/Normal. A transition back to that canonical state removes its vault record.

Operations are synchronized per item. A Set or Transition:

1. reads and strictly decodes the current vault data;
2. computes a new immutable state;
3. performs expected-current replacement;
4. re-reads and verifies the stored state;
5. advances process-local diagnostics only after verification.

A failure preserves the previously observable state whenever the adapter can roll back safely.

## Firearm resolution boundary

`KingmakerFirearmRuntimeItemResolver` accepts only a concrete runtime weapon whose exact weapon type contains exactly one valid `FirearmDefinitionComponent`. It rejects native Heavy Crossbows, blueprint objects, slot wrappers, nonweapons, zero-marker types, multi-marker types, and malformed definitions before persistence access.

The Test Musket's borrowed Heavy Crossbow category therefore does not itself confer firearm state.

## Legacy-token migration

The four Sprint 12 token blueprints remain resolvable but are not used by normal new writes.

- Token only: write and verify vault data, then clear and verify the token.
- Equivalent token and vault: preserve the vault and clear the redundant token.
- Conflicting token and vault: preserve both and fail closed.
- Unknown or duplicate tokens: preserve evidence and fail closed.
- Token cleanup failure after a new vault write: attempt vault rollback and record any rollback failure.

## Development controls

The future compiled UMM panel can:

- print exact firearm state in equipment and shared inventory;
- assign two Test Muskets independent vault states;
- seed one supported legacy token without creating a vault record;
- trigger a normal read to exercise migration;
- load one diagnostic round;
- apply misfire damage;
- repair or reset the first equipped firearm.

These controls consume no inventory ammunition and do not yet affect attack execution.

## Blocking runtime questions

The lifecycle matrix must prove that the custom UnitPart and direct item references survive:

- same-process save/load;
- exit, process restart, and reload;
- equip/unequip and weapon-set switching;
- party transfer and shared stash movement;
- area transition and rest;
- sale and repurchase;
- respec or main-character reconstruction;
- deletion and repeated-save growth checks;
- migration from every legacy token state.

See [UNITPART-STATE-VAULT.md](UNITPART-STATE-VAULT.md), [PERSISTENCE-TEST-MATRIX.md](PERSISTENCE-TEST-MATRIX.md), and [ADR-0020](decisions/ADR-0020-save-owned-unitpart-state-vault.md).
