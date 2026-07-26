# Executed test evidence

## Scope

Sprint 17 closes the source milestone's largest non-game-runtime evidence gap: the dependency-free C# harness has now been compiled and executed rather than merely parsed or modeled.

This evidence does **not** prove that Kingmaker, Unity Mod Manager, Harmony, Unity serialization, item identity, or the in-game save lifecycle accepts the mod. Those boundaries still require the exact proprietary runtime assemblies and a running game.

## Exact compile contract

The harness was compiled with:

- C# language version 7.3.
- Warnings treated as errors.
- Optimization enabled.
- Deterministic compiler mode enabled.
- The official .NET Framework 4.7 reference surface: `mscorlib.dll`, `System.dll`, and `System.Core.dll`.
- Roslyn compiler 4.7.0.

The resulting .NET Framework executable was then run on a supplied CoreCLR 8.0 runtime. This validates the pure IL and behavior, but is not a substitute for Kingmaker's Mono process.

## Result

```text
Declared tests:             373
Compile exit code:          0
Compile warnings/errors:    0
Executed runs:              3
Passing tests per run:      373
Failures per run:           0
Repeated output identical:  yes
```

The machine-readable report is [executed-test-evidence.json](../evidence/executed-domain-tests/executed-test-evidence.json). The complete stdout from all three runs is retained beside it.

## Defects exposed by execution

The first compiled run found defects that syntax parsing and reference models did not expose.

### 1. Recursive firearm-item equality

`FirearmItemId.operator ==` used `left != null`, while `operator !=` called `operator ==`. Any non-null equality comparison recursively alternated between the two operators until stack overflow.

The fix uses `ReferenceEquals` for null checks in `Equals`, `CompareTo`, and the equality operator. Existing value-equality and null-operator tests now execute successfully.

### 2. Inaccurate missing-state diagnostic contract

The service returned a detailed message tied to an earlier repository implementation, while the test expected different stale wording. The message now states the durable contract directly:

```text
The exact firearm item has no existing firearm-state entry.
```

### 3. Wrong duplicate-token exception expectation

The implementation correctly treats multiple legacy state tokens as invalid serialized data and throws `InvalidDataException`. The test incorrectly expected `InvalidOperationException`; the test contract was corrected without weakening the implementation.

### 4. Invalid attack distance failed open

The armor-class selector converted `NaN`, infinity, and negative distance values to zero before range calculation. That could incorrectly select touch AC instead of retaining ordinary AC.

The selector now rejects invalid distances before tolerance adjustment. Separate regression cases cover:

- `NaN`.
- Negative distance.
- Positive infinity.

All return `invalid-range-input` and leave ordinary AC unchanged.

## Retained evidence

- [Original failing stdout](../evidence/defect-discovery/original-run.stdout.txt)
- [Original failing stderr](../evidence/defect-discovery/original-run.stderr.txt)
- [Pre-AC-fix stdout](../evidence/defect-discovery/pre-ac-fix-run.stdout.txt)
- [Pre-AC-fix stderr](../evidence/defect-discovery/pre-ac-fix-run.stderr.txt)
- [Sprint 16 fix patch](../evidence/defect-discovery/sprint16-fixes.patch)

Absolute packaging-environment source paths in the retained logs were replaced with stable placeholders.

## Evidence boundary

This evidence supports the following claims only:

1. The 373 dependency-free test cases compile against the .NET Framework 4.7 API surface with C# 7.3 and no warnings.
2. The compiled test executable completes three byte-identical successful runs on CoreCLR.
3. The four defects above are covered by executable regression tests.

It does not support claims about:

- Semantic compilation of the main mod against `Assembly-CSharp.dll`.
- Harmony patch installation.
- Blueprint registration.
- UMM panel behavior.
- `ItemEntityWeapon.UniqueId` in the selected Kingmaker build.
- Custom `UnitPart` serialization.
- Save/load, merchants, duplication, deletion, or migration.
- A UMM-installable package.
