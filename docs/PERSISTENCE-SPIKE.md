# Firearm-state persistence spikes

## Purpose

The persistence gate asks one architectural question:

> Can complete state for one exact firearm survive every Kingmaker lifecycle transformation without moving to another gun, disappearing, duplicating, or making the save unrecoverable?

The answer remains **unproven** until a compiled Unity Mod Manager package passes the full matrix.

## Sprint 12 candidate — item-enchantment state tokens

Sprint 12 represented each non-default capacity-one state with a component-only `BlueprintWeaponEnchantment` attached to the exact gun. Four stable token GUIDs encoded loaded/Normal, empty/Broken, loaded/Broken, and Wrecked.

The design proved a strict finite codec and transactional add/remove behavior in source models, but it was not promoted because dynamic item-enchantment serialization could not be observed in Kingmaker here. It is also not scalable to arbitrary capacities and ammunition families.

Those four token blueprints remain permanent compatibility identifiers. Sprint 13 treats them as legacy migration inputs only.

## Sprint 13 candidate — save-owned UnitPart vault

Sprint 13 stores arbitrary primitive firearm state in a custom `UnitPart` attached to the main-character save graph:

```text
UnitPartFirearmStateVault
  -> direct exact ItemEntityWeapon reference
  -> FirearmStateData
```

The host character is not the state owner. The record's direct item reference is the only persistent identity key.

### Advantages over finite tokens

- One record can represent arbitrary future capacities and ammunition IDs.
- No combinatorial blueprint catalog is required.
- No new blueprint GUID is needed.
- State remains separate from a wielder or equipment slot.
- The established repository and service contracts remain unchanged.

### Vault rules

- Absence means canonical empty/Normal.
- Keys compare by `ReferenceEquals`.
- DTOs are defensive-copied.
- Duplicate exact-item records fail closed.
- Writes use expected-current comparison and verification.
- Failed in-memory changes restore the previous record list.
- Null item references may be pruned after deserialization.

## Migration path

When the exact gun carries a Sprint 12 token:

1. Strictly decode the token set.
2. Read any existing vault record.
3. Write and verify the vault when absent.
4. Clear and verify removal of the token.
5. Roll back the new vault record if cleanup fails.

Equivalent carriers result in token cleanup. Conflicting carriers preserve both and fail. Unknown or duplicate tokens preserve evidence.

Normal new writes never create a state token.

## Why the gate remains NO-GO

Source work cannot prove that Kingmaker:

- serializes a custom mod-defined UnitPart;
- restores its concrete type after process restart;
- restores a direct item reference to the actual reconstructed item;
- preserves merchant items through sale and repurchase;
- retains the vault through respec or character reconstruction;
- handles permanent item deletion without save leaks;
- loads saves safely when the mod is absent or upgraded;
- performs the legacy-token migration exactly once.

These are runtime facts. The authoritative procedure is [PERSISTENCE-TEST-MATRIX.md](PERSISTENCE-TEST-MATRIX.md).

## Decision branches

### GO

All Critical lifecycle and migration rows pass. The next sprint may add inventory ammunition and an atomic consumption service while keeping loaded state in the accepted per-item carrier.

### NO-GO

Any Critical row fails. The next sprint remains a persistence sprint, preserves the immutable state/repository contracts, preserves all four legacy token GUIDs, and migrates readable Sprint 13 vault data where possible.
