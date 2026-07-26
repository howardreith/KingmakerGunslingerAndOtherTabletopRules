# Sprint 9 report — range-limited touch AC

## Status

**Source milestone complete. Not UMM-installable and not runtime-certified.**

Version: `0.0.9-s09-touch-ac`

Sprint 9 implements the first gameplay-changing firearm rule in source. It does not include a compiled DLL because the current environment has no installed Kingmaker/Unity/UMM assemblies or Windows .NET Framework build toolchain.

## Goal

Make an exact early firearm attack touch AC in its first firearm range increment and ordinary AC beyond it, without converting the attack into a spell or bypassing Kingmaker's ordinary ranged-weapon pipeline.

## Implementation

### Pure selection service

`FirearmArmorClassService` receives only immutable values:

- Exact-marker status and marker count.
- `FirearmDefinition`.
- Distance in engine meters.
- Ordinary AC.
- Touch AC.
- The already-calculated rule-event `TargetAC`.
- Whether the event has already been adjusted.

For the Sprint 9 early-firearm rule, it selects touch AC only when the calculated range increment is exactly one. The calculation subtracts a 0.1-millimeter tolerance before applying `ceil`, avoiding boundary errors caused by float conversion noise.

The selected value is:

```text
selected TargetAC = current TargetAC + (touch AC - ordinary AC)
```

This intentionally differs from replacing `TargetAC` with the raw touch value. If Kingmaker has already added cover or removed Dexterity/dodge for a flat-footed attack, that contextual difference remains on the event.

### Marker-scoped attack context

The existing `RuleAttackRoll.OnTrigger()` patch now maintains a short-lived thread-local stack containing only:

- Runtime event identity.
- Immutable firearm-marker snapshot.

A `RuleCalculateAC` event uses a directly resolved weapon marker when available. Otherwise it inherits only the top-level marker from the currently executing attack roll. A concrete non-firearm or ambiguous weapon blocks inheritance and retains ordinary AC.

No Kingmaker rule, unit, item, or blueprint object is retained after the attack callback returns.

### AC event adapter

After Kingmaker's `RuleCalculateAC.OnTrigger()` has calculated its result, the adapter requires all of the following:

- Initiator and target.
- A working `DistanceTo` method.
- Ordinary and touch AC values.
- Exactly one writable Int32 `TargetAC` member.
- Exactly one firearm marker and an early-firearm definition.

Any missing or ambiguous contract fails closed. No attack is suppressed and no entire rule method is replaced.

### Duplicate protection

A `ConditionalWeakTable` records successfully selected touch-AC events. If the same rule object is observed again, the runtime adapter increments a duplicate counter, optionally emits `ac.duplicate-skipped`, and performs no second adjustment. The weak key does not keep completed rule events alive. The pure service also retains an `already-applied` fail-closed input for independent testing.

### Diagnostics

When the existing combat-trace toggle is enabled, the mod emits:

```text
[firearms][ac.touch-selected]
[firearms][ac.ordinary-selected]
[firearms][ac.duplicate-skipped]
```

with the weapon type, distance, range increment, previous and selected target AC, delta, resolved target member, and decision reason.

The UMM panel also exposes counters for touch selections, exact-firearm ordinary selections, duplicate events, faults, and active attack-context depth.

## Scope boundary

Included:

- Early-firearm first-increment touch AC.
- Ordinary AC beyond the first increment.
- Native Heavy Crossbow regression boundary.
- Pure tests and a documented eventual in-game matrix.

Not included:

- Advanced firearm five-increment penetration.
- Ammunition, reload, misfire, broken state, grit, class progression, vendors, assets, or AI.
- A compiled DLL or UMM install package.

## Validation completed in this environment

- All C# files parse without tree-sitter syntax errors.
- All PowerShell scripts parse without tree-sitter syntax errors.
- Blueprint manifest and stable GUIDs remain unchanged.
- The dependency-free test harness declares 94 uniquely named cases.
- Twenty-one new AC selection and strict-access cases cover close/far range, exact boundary, native/ambiguous markers, contextual modifiers, duplicate protection, unsupported advanced firearms, invalid data, overflow, participants, distance, AC values, private setters, fields, and ambiguous writable members.
- A separate Python model reproduces the core selection matrix.
- JSON, JSON Schema, MSBuild XML, Markdown links, text encoding, binary exclusions, internal manifests, checksums, and ZIP structure pass.

The C# tests could not be executed and the main project could not be compiled here. Those limitations are recorded rather than replaced with stub-linked binaries.

## Runtime gate

Sprint 9 should not be promoted to a player-test package until a local Windows build confirms:

1. The runtime-contract report passes.
2. The domain harness reports `Completed 94 tests; failures=0.`
3. A close Test Musket attack logs `ac.touch-selected`.
4. A distant Test Musket attack logs `ac.ordinary-selected` with reason `outside-first-range-increment`.
5. A native Heavy Crossbow produces no firearm AC-selection record.
6. Deadly Aim, cover, concealment, ordinary damage, and full attacks remain on Kingmaker's normal weapon path.
