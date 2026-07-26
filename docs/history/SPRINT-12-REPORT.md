# Sprint 12 completion report

## Milestone

```text
0.0.12-s12-persistence-spike
```

## Result

Sprint 12 is complete as a **source and diagnostic persistence spike**.

The implementation selects item-owned, component-only weapon-enchantment tokens as the first persistence candidate and wires them into the exact-firearm state service. It also defines the installed-assembly contract inspection and full in-game lifecycle matrix required to accept or reject that candidate.

The architecture gate remains:

> **NO-GO pending a compiled Unity Mod Manager package and successful Kingmaker lifecycle matrix.**

Sprint 13 must not begin ammunition or reload implementation until the runtime evidence exists. A failed matrix redirects Sprint 13 to the next persistence candidate.

## Kingmaker package status

**NOT READY FOR KINGMAKER.**

The published archives contain source and documentation but no compiled `KingmakerGunslinger.dll`. They are not Unity Mod Manager install packages.

No runtime-ready ZIP can be honestly produced in this environment because it lacks Windows MSBuild, the .NET Framework 4.7 targeting pack, the target Kingmaker assemblies, the installed UMM/Harmony assemblies, and a running Kingmaker process.

## Implemented persistence candidate

The persisted state is represented on the exact firearm item as zero or one no-op `BlueprintWeaponEnchantment`:

```text
no token
  = schema 1, rounds 0, ammunition none, condition Normal

kmg.state.v1.loaded-normal.lead-ball
  = schema 1, rounds 1, ammunition kmg.debug.lead-ball, condition Normal

kmg.state.v1.broken-empty
  = schema 1, rounds 0, ammunition none, condition Broken

kmg.state.v1.broken-loaded.lead-ball
  = schema 1, rounds 1, ammunition kmg.debug.lead-ball, condition Broken

kmg.state.v1.wrecked
  = schema 1, rounds 0, ammunition none, condition Wrecked
```

This finite mapping covers every non-default state reachable by the capacity-one Test Musket and current debug ammunition identity.

## New domain types

### `FirearmStateTokenDefinition`

An immutable mapping between one stable serializer-facing token ID and one complete immutable `FirearmState`.

Token IDs are strict lowercase ASCII identifiers, not display names or localized strings.

### `FirearmStateTokenCatalog`

A bidirectional, fail-closed codec:

- token absence decodes to empty/Normal;
- exactly one known token decodes to a complete state;
- unknown, duplicate, null, malformed, or unsupported values are rejected;
- unsupported future state combinations cannot be silently collapsed.

### `IFirearmStateTokenStore`

A narrow engine boundary supporting:

```text
ReadTokenIds
ReplaceToken(expected, target)
ClearTokens
```

The pure repository does not know how Kingmaker stores enchantments.

### `TokenBackedFirearmStateRepository`

The source of truth is the item's token store. A `ConditionalWeakTable` retains only process-local diagnostics:

- entry ID;
- revision;
- runtime type;
- reference hash.

A newly reconstructed item object can therefore recover state from its own token even though it has no prior weak-table entry.

## New Kingmaker adapter

### `FirearmStateTokenComponent`

A passive `BlueprintComponent` containing the strict token payload. It has no event handlers and no gameplay effect.

### `FirearmStateTokenBlueprints`

Registers four `BlueprintWeaponEnchantment` assets. Each contains exactly one `FirearmStateTokenComponent` and no other components.

### `KingmakerFirearmStateTokenStore`

A reflection-contained adapter over candidate item-enchantment APIs. It:

1. Reads the item's enchantment collection.
2. Selects only blueprints carrying exactly one firearm-state token marker.
3. Requires the exact registered blueprint instance for that token.
4. Rejects duplicate, foreign, malformed, or future tokens.
5. Adds the target token first.
6. Verifies the target runtime enchantment exists exactly once.
7. Removes the previous token.
8. Verifies the final set.
9. Attempts to restore the previous set if any step fails.

Clear operations reject corrupt multi-token state rather than destroying diagnostic evidence.

## Blueprint registration

The manifest now contains 12 stable IDs, 8 active:

| Symbol | GUID | Status |
|---|---|---|
| `KMG.Test.LoadedStateToken` | `c11a8965dbdd43f08080f4dc51a29113` | activated |
| `KMG.Test.BrokenEmptyStateToken` | `5513972dd2624c9f86bc29c850dac736` | new |
| `KMG.Test.BrokenLoadedStateToken` | `f5fa460f93214458b6f59db24b0dfd12` | new |
| `KMG.Test.WreckedStateToken` | `877f65ca3a404f2e98af528b7fb1a2fb` | new |

The original nine GUID values remain unchanged. Blueprint bootstrap now expects exactly eight custom registrations and configures the token-backed repository only after the full token set validates.

## Development controls

The future locally compiled UMM panel can:

- add Test Muskets;
- grant Firearm Proficiency;
- stamp two inventory Test Muskets with different item-owned token states;
- print visible and equipped firearm states;
- load one debug round;
- apply a misfire-damage transition;
- repair a broken gun;
- remove its token by resetting to empty/Normal.

These controls still do not consume inventory ammunition or alter firing behavior.

## Runtime-contract inspection

`inspect-runtime-contracts.ps1` now records and gates:

- `BlueprintWeaponEnchantment` existence and constructibility;
- `ItemEntity` / `ItemEntityWeapon` relationship;
- item enchantment collection candidates;
- `ItemEnchantment` and its blueprint member;
- compatible `AddEnchantment` methods;
- compatible `RemoveEnchantment` methods;
- all retained bootstrap, blueprint, proficiency, inventory, combat-trace, and touch-AC contracts.

A report passing metadata inspection still does not prove serialization. The lifecycle matrix is authoritative.

## Test delta

Sprint 12 adds 52 named dependency-free C# cases:

- 24 token-definition and catalog cases;
- 28 token-backed repository cases.

The complete harness now declares **239** cases.

Portable validation additionally models:

- four state-token round trips;
- token absence as the default state;
- duplicate and unknown token rejection;
- two-item isolation;
- reconstruction from an item token with a fresh repository;
- simulated item recreation with copied token data;
- add and remove failure rollback;
- corrupt-token preservation;
- weak diagnostic metadata lifetime;
- native crossbow rejection before repository access.

## Validation completed

The source package was checked for:

- metadata and version consistency;
- stable GUID uniqueness and JSON Schema validity;
- project compile-item completeness;
- non-copying external references;
- pure-domain isolation from Kingmaker and Unity;
- exact token-blueprint count and passive component shape;
- declared test uniqueness and method coverage;
- C# and PowerShell syntax parsing;
- retained touch-AC reference behavior;
- documentation links;
- UTF-8 and text hygiene;
- proprietary or compiled binary exclusion;
- archive CRC, traversal, duplicate-entry, single-root, manifest, and checksum integrity.

## Not validated

The following remain unproven:

- compilation against the actual Kingmaker/UMM assembly set;
- the exact runtime overload chosen for `AddEnchantment`;
- dynamic enchantment save serialization;
- token restoration after process restart;
- state behavior during sale, repurchase, duplication, or item reconstruction;
- visual or price side effects from no-op token enchantments;
- interaction with other mods that alter item enchantments;
- safe migration if the finite token scheme is later replaced.

## Gate outcome

Sprint 12 has produced a concrete candidate and an executable test plan, but not the evidence required to accept it.

```text
Source spike: COMPLETE
Persistence candidate: IMPLEMENTED
Runtime proof: ABSENT
Gate decision: NO-GO
Next feature work: BLOCKED
```
