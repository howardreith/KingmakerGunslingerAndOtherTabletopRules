# Sprint 10 report — pure immutable firearm state

## Result

Sprint 10 is complete as a source milestone.

Version: `0.0.10-s10-firearm-state`

**Kingmaker status: NOT READY FOR KINGMAKER.** No compiled DLL or Unity Mod Manager install ZIP was produced in this environment.

## Goal

Define loaded rounds, loaded ammunition identity, and firearm condition as deterministic per-item domain state before selecting a Kingmaker item or save-storage mechanism.

## Delivered domain types

### `AmmunitionId`

A stable ordinal value object with strict lowercase serializer-safe syntax and a 128-character maximum.

### `FirearmCondition`

The first state schema recognizes exactly:

```text
Normal
Broken
Wrecked
```

### `FirearmStateRules`

Immutable capacity and ammunition-compatibility inputs. The compatibility collection is null-checked, duplicate-rejected, sorted, copied, and exposed only through defensive copies.

### `FirearmState`

An immutable value object containing:

```text
schemaVersion
loadedRounds
loadedAmmunition
condition
```

It rejects impossible combinations such as loaded-without-ammunition, empty-with-ammunition, unknown condition, or a loaded wrecked firearm.

### `FirearmStateMachine`

Pure transitions for:

- Loading compatible rounds up to capacity.
- Firing exactly one round.
- Normal-to-Broken misfire damage.
- Broken-to-Wrecked misfire damage.
- Broken-to-Normal repair.
- Explicit wrecking.

Illegal state transitions use typed rejection errors. Input/programming errors use argument exceptions. No operation mutates its input.

### `FirearmStateData` and `FirearmStateCodec`

A primitive-only DTO and strict converter define deterministic field and token semantics without choosing how Kingmaker will store them.

## Deliberate state decisions

- Broken firearms remain loadable and fireable at the pure state level; later combat rules apply penalties.
- Normal-to-Broken preserves loaded payload.
- Broken-to-Wrecked and explicit Wreck clear all loaded payload.
- Ordinary Repair preserves loaded payload.
- Wrecked cannot use ordinary Repair.
- One loaded state can contain only one ammunition identity.
- State has no owner or game-object reference.

## Tests

Sprint 10 adds 61 named dependency-free cases, increasing the complete harness from 94 to 155 cases.

The new cases cover:

- Ammunition ID syntax, equality, hashing, and ordering.
- Rule capacity, compatibility, sorting, duplication, and defensive copying.
- State schema and invariant validation.
- Load, fire, damage, repair, and wreck transitions.
- Rejection reason codes and no-mutation behavior.
- DTO canonical form, round trips, schema rejection, capacity rejection, compatibility rejection, and malformed payload rejection.

## Portable validation

The milestone validator checks:

- All C# and PowerShell files for syntax.
- All JSON and MSBuild XML documents for parsing.
- Blueprint manifest schema and nine unchanged IDs.
- Four unchanged active blueprints.
- Main and test project compile declarations.
- Exactly 155 unique named C# cases with matching methods.
- Pure-state files for absence of Kingmaker, Unity, unit, item, buff, and inventory types.
- An independent transition model covering legal and rejected paths.
- Existing touch-AC model preservation.
- Markdown links, text encoding, final newlines, trailing whitespace, and forbidden binaries.

## What remains unvalidated

The current environment has no:

- Windows MSBuild.
- .NET Framework 4.7 targeting pack.
- Installed Kingmaker assemblies.
- Installed UMM/Harmony assemblies.
- Running Kingmaker process.

Accordingly, this milestone does not claim C# semantic compilation, execution of the 155-test harness, Harmony installation, blueprint initialization, in-game touch AC, or any item-state behavior.

## Blueprint and save impact

No blueprint ID, status, or registration changed. The manifest still contains nine stable IDs with four active entries.

No state is attached to a character, buff, item, blueprint, or save. Sprint 10 therefore introduces no new save payload.

## Next sprint

Sprint 11 is bounded to process-local association of independent `FirearmState` values with exact Test Musket item instances. Save/load persistence remains deferred to Sprint 12.
