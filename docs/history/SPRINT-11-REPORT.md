# Sprint 11 report — process-local per-item firearm state

## Result

Sprint 11 is complete as a source milestone.

Version: `0.0.11-s11-runtime-item-state`

**Kingmaker status: NOT READY FOR KINGMAKER.** No compiled DLL or Unity Mod Manager install ZIP was produced in this environment.

## Goal

Associate one independent immutable `FirearmState` with each exact runtime Test Musket item instance during one Kingmaker process, without claiming save/load persistence.

## Delivered runtime boundary

### `IFirearmStateRepository`

The exclusive process-local state access contract supports get/create, existing lookup, assignment, atomic transition, explicit removal, and diagnostic counters.

### `WeakFirearmStateRepository`

The implementation uses:

```text
ConditionalWeakTable<object, Entry>
```

Each entry is keyed by exact object reference, begins with `FirearmState.CreateEmpty()`, owns a per-entry lock, and records a revision plus a monotonically increasing process-local diagnostic identity such as `kmg-item-000001`.

Equal assignments are no-ops. A failed transition leaves both state and revision unchanged. The weak table does not intentionally keep discarded Kingmaker item objects alive.

### `FirearmItemStateService`

The service validates firearm identity before touching the repository. A candidate must resolve to a concrete runtime `ItemEntityWeapon`, expose a `BlueprintItemWeapon`, resolve an exact `BlueprintWeaponType`, and contain exactly one valid `FirearmDefinitionComponent`.

Blueprint objects, slot wrappers, ordinary inventory objects, native Heavy Crossbows, zero-marker weapons, and multi-marker weapons are rejected before an entry is created.

### Diagnostic snapshots

`FirearmStateRepositorySnapshot` and `FirearmItemStateSnapshot` retain only immutable state and descriptive primitive/string metadata. They do not retain the runtime item, unit, inventory, blueprint, or Unity object.

## Development controls

The UMM panel source adds explicit controls to:

- print state for firearms equipped by the selected unit;
- print state for exact firearms visible in equipment and shared inventory;
- assign two unequipped Test Muskets independent debug states;
- load one debug round into the first equipped firearm;
- apply one misfire-damage transition;
- repair a broken firearm;
- reset a firearm to empty/Normal state.

The two-musket diagnostic assigns one firearm a loaded debug round and the other an empty Broken state, then verifies different repository entry IDs and no state leakage.

These controls consume no inventory ammunition and do not affect attacks.

## Acceptance mapping

| Sprint 11 acceptance criterion | Source result |
|---|---|
| Two identical item instances hold different state | Implemented and covered by repository/service tests and the two-musket debug control |
| Equip, unequip, and weapon-set switching do not merge state | Exact-reference architecture supports this while Kingmaker retains the same object; real-game behavior remains unproven |
| Party transfer retains state | State follows the exact object reference; real-game transfer behavior remains unproven |
| Native Heavy Crossbows never receive firearm state | Supported service rejects weapons without exactly one firearm marker before repository creation |
| Diagnostics identify item and immutable state | Implemented through repository identity, revision, runtime metadata, definition, and state snapshots |
| Save/load and restart are explicitly deferred | Documented as the Sprint 12 go/no-go gate |

## Tests

Sprint 11 adds **32** named dependency-free cases, increasing the complete harness from 155 to **187** cases.

The new cases cover:

- null and value-type key rejection;
- exact reference identity despite value equality;
- two-item isolation;
- default state, revision, mutation, and removal behavior;
- no-op assignments;
- atomic transitions and rejected-transition preservation;
- removal and re-creation with a new repository identity;
- reference-identity, removal, and re-creation behavior;
- resolver rejection before entry creation;
- canonical exact-item keys;
- independent state through service operations;
- immutable diagnostic metadata and deterministic formatting.

## Portable validation

The milestone validator checks:

- all C# and PowerShell sources for syntax;
- all JSON and MSBuild XML documents for parsing;
- stable blueprint IDs and unchanged active blueprint count;
- main and test project compile declarations;
- exactly 187 unique named C# cases with matching methods;
- exact-reference and weak-lifetime repository boundaries;
- absence of save APIs from the process-local repository;
- rejection of native or ambiguous nonfirearms before state creation;
- an independent process-local item-state model;
- retained touch-AC behavior;
- Markdown links, text hygiene, package safety, and forbidden binaries.

## What remains unvalidated

The current environment has no Windows MSBuild, .NET Framework 4.7 targeting pack, installed Kingmaker assemblies, UMM/Harmony assemblies, or running game process. Accordingly, this milestone does not claim:

- semantic compilation against the installed game;
- execution of the 187-test C# harness;
- that Kingmaker preserves one item object through equip, weapon-set switch, transfer, save/load, sale, or area transition;
- that the development controls resolve the installed build's item and inventory APIs;
- save persistence or process-restart persistence;
- a working UMM package.

## Blueprint and save impact

No blueprint GUID or activation status changed. The manifest still contains nine stable IDs with four active entries.

Sprint 11 introduces no serialized save payload. It can create process-local state only after an exact firearm item is observed. Existing manual controls can still add custom blueprint references to disposable saves, so important saves remain out of scope.

## Next sprint

Sprint 12 is the formal save/load persistence spike. It must prove that two identical firearms restore their own independent state after process restart without state migration, orphan leaks, or native-crossbow contamination before ammunition and reload work may proceed.
