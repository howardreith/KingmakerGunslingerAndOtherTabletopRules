# Pure firearm state machine

## Status

The current runtime source of truth is the exact item's inert `BlueprintWeaponEnchantment` state token. Save/restart durability, reload, loaded-round discharge, natural-roll misfires, condition damage, and the native five-foot second-misfire burst have been observed in Kingmaker. Historical weak, UnitPart, and `UniqueId` vault candidates remain documentation/test history only; the runtime-rejected `UniqueId` vault must not be revived.

Sprint 27 adds a development-only pure recovery boundary from Wrecked to Broken while preserving the exact item. It does not add player-facing repair gameplay.

## Purpose

A firearm's loaded rounds, loaded ammunition identity, and mechanical condition must belong to one concrete firearm item. They must not be represented by a buff on the wielder, a shared weapon blueprint, or a global flag.

## State shape

```text
FirearmState
  schemaVersion: 1
  loadedRounds: 0..64
  loadedAmmunition: AmmunitionId or null
  condition: Normal | Broken | Wrecked
```

Invariants:

- Schema version is exactly `1`.
- Zero loaded rounds requires a null ammunition ID.
- One or more loaded rounds requires a valid ammunition ID.
- A wrecked firearm is always empty.
- The immutable state contains no owner, unit, item, blueprint, inventory, event, Unity object, or persistence handle.

`FirearmState.CreateEmpty()` produces the canonical starting state:

```text
schema=1; rounds=0; ammunition=<none>; condition=Normal
```

## Ammunition identity

`AmmunitionId` is a stable value object rather than an inventory-item reference. It accepts lowercase ASCII letters and digits plus `.`, `_`, `-`, and `:` after the first character, using ordinal case-sensitive equality.

Example:

```text
kmg.ammunition.lead-ball
```

The identity can later map to an ammunition blueprint or recipe. No such inventory blueprint is active yet.

## Rules and transitions

`FirearmStateRules` supplies immutable capacity and ammunition-compatibility inputs. `FirearmStateMachine` owns pure transitions; every operation returns a new valid state or throws without modifying its input.

| Operation | Legal source | Result |
|---|---|---|
| `Load` | Normal or Broken, within capacity | Adds one compatible ammunition type |
| `Fire` | Normal or Broken with at least one round | Consumes one round; clears ammunition identity after the final round |
| `ApplyMisfireDamage` | Normal | Becomes Broken and preserves its load |
| `ApplyMisfireDamage` | Broken | Becomes Wrecked and empty |
| `Repair` | Broken | Becomes Normal and preserves its load |
| `OverhaulWrecked` | Wrecked | Becomes Broken and empty; exact-item gameplay delivery remains deferred |
| `Wreck` | Normal or Broken | Becomes Wrecked and empty |
| `Wreck` | Wrecked | Returns the same state |

Rejected gameplay transitions use typed reason codes such as `Empty`, `Wrecked`, `CapacityExceeded`, `IncompatibleAmmunition`, `MixedAmmunition`, `NotBroken`, and `NotWrecked`.

A broken gun remains loadable and fireable at the state-machine layer. Later combat rules determine penalties and misfire consequences. A wrecked gun cannot load, fire, or use ordinary repair.

## Serializer DTO

`FirearmStateData` is the primitive mutable transport shape:

```text
schemaVersion: int
loadedRounds: int
loadedAmmunitionId: string or null
condition: "normal" | "broken" | "wrecked"
```

`FirearmStateCodec` strictly converts between DTO and immutable state. Restoration always receives the current `FirearmStateRules`, so over-capacity, incompatible, or malformed data is rejected before becoming runtime state.

Sprint 14 stores this DTO in an identity-keyed record inside `UnitPartFirearmStateVault`. The DTO contract remains independent from the carrier, allowing another persistence mechanism to replace the vault without rewriting firearm rules.

## Explicitly excluded

The combined milestone still does not:

- prove that Kingmaker serializes the custom UnitPart, preserves the item identity, or restores either legacy carrier;
- consume inventory ammunition;
- add powder, bullet, or cartridge blueprints;
- add a reload action;
- block an empty firearm attack;
- consume a loaded round during an attack;
- intercept a misfire roll or deal explosion damage;
- add vendors, crafting, grit, deeds, or class progression.

## Test coverage

The dependency-free C# harness contains 373 named cases. Sixty-one cover the immutable state/codec layer; later groups cover weak, token, direct-reference, identity-vault, migration, evidence, and runtime-preflight behavior. Sprint 17 compiled the suite against the .NET Framework 4.7 reference surface and executed three byte-identical passing runs. The main Kingmaker integration still requires the exact game and UMM assemblies.

See [ENGINE-ITEM-IDENTITY-VAULT.md](ENGINE-ITEM-IDENTITY-VAULT.md) for the current candidate, [UNITPART-STATE-VAULT.md](UNITPART-STATE-VAULT.md) for the Sprint 13 predecessor, and [PERSISTENCE-TEST-MATRIX.md](PERSISTENCE-TEST-MATRIX.md) for the blocking runtime gate.
