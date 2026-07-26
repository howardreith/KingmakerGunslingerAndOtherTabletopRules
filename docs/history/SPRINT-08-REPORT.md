# Sprint 8 completion report

**Milestone:** `0.0.8-s08-combat-tracing`
**Artifact class:** source milestone
**Kingmaker readiness:** **NOT READY FOR KINGMAKER**

## Goal

Observe the Kingmaker weapon-attack pipeline for a marker-identified firearm without changing attack rolls, armor class, damage, ammunition, or native weapon behavior. The resulting trace must contain enough evidence to design Sprint 9's range-limited touch-AC rule from observed engine data instead of assumptions.

## Delivered

Sprint 8 adds three disabled-by-default, read-only Harmony prefix/postfix pairs. Their targets are resolved from the installed `Assembly-CSharp.dll` at patch time:

- `Kingmaker.RuleSystem.Rules.RuleAttackWithWeapon.OnTrigger()`
- `Kingmaker.RuleSystem.Rules.RuleAttackRoll.OnTrigger()`
- `Kingmaker.RuleSystem.Rules.RuleCalculateAC.OnTrigger()`

The patch methods return `void`, accept no `ref`/`out` arguments, do not read or write `__result`, and do not suppress an original method. Missing or ambiguous targets skip only the optional trace patch and log the reason.

### Firearm identity

A rule event is eligible only when its concrete weapon item's exact `BlueprintWeaponType` contains exactly one `FirearmDefinitionComponent`. The reused Heavy Crossbow category is not consulted. A native Heavy Crossbow therefore produces no firearm trace.

### Correlation

The correlation engine:

- Creates a trace from a firearm `RuleAttackWithWeapon` or standalone firearm `RuleAttackRoll`.
- Joins nested attack-roll and AC callbacks by integer event identity and callback nesting.
- Numbers repeated callbacks instead of silently collapsing them.
- Completes at the root event's postfix.
- Retains only immutable strings, primitive values, and integer identities.
- Clears active state on diagnostic failure without changing the combat event.

### Captured fields

Each observation attempts to copy:

- Concrete weapon and runtime item identity.
- Item and weapon-type blueprint names and IDs.
- Firearm definition and marker count.
- Initiator and target.
- Natural d20.
- Attack bonus, attack total, result, and hit status.
- Target AC plus ordinary and touch AC from target stats.
- Distance in engine meters and calculated firearm range increment.
- Ability/reason source.
- Full-attack, first-attack, attack-of-opportunity, and attack-number flags.

Unavailable values are logged as `<unavailable>`; the adapter does not synthesize a plausible value.

### UMM controls

The development panel now contains a non-persistent toggle:

```text
Enable read-only firearm combat tracing (verbose log output)
```

It defaults to off. Disabling it clears current-thread diagnostic state. Counters display completed traces and contained diagnostic faults.

### Runtime-contract inspection

`scripts/inspect-runtime-contracts.ps1` now inspects the three rule-event types, their declared zero-argument `OnTrigger` methods, candidate data members, and `UnitEntityData.DistanceTo`. The script records the findings in ignored local JSON and fails if the minimum Sprint 8 contract is not present.

## Tests and validation

The dependency-free C# harness now declares **73 cases**: the previous 50 firearm/reflection cases plus 23 range, correlation, immutability, duplicate-callback, completion, reset, and formatter cases.

Validation performed in this environment:

- Parsed all 45 C# files with tree-sitter: zero syntax errors.
- Parsed all 11 PowerShell scripts with tree-sitter: zero syntax errors.
- Parsed all JSON and MSBuild XML documents.
- Confirmed 43 main-project compile items and 17 test-project compile items exist.
- Confirmed all ten external references remain `Private=False`.
- Confirmed all nine blueprint GUIDs are unchanged and exactly four remain active.
- Declared 73 dependency-free C# cases and modeled 14 independent trace/range scenarios; the C# harness could not be executed here.
- Confirmed exactly three read-only prefix/postfix pairs.
- Confirmed diagnostic code does not use `WeaponCategory` or `ProficiencyGroup` as firearm identity.
- Confirmed no DLL, executable, PDB, MDB, game path, or runtime-contract fingerprint is present.
- Verified archive paths, CRCs, internal SHA-256 manifest, and single-root layout.

## Not delivered

This environment still lacks Windows MSBuild, the .NET Framework 4.7 targeting pack, Kingmaker's installed managed assemblies, Unity Mod Manager, and a running game. Consequently this milestone does **not** include or claim:

- Semantic C# compilation against Kingmaker.
- Execution of the 73 C# tests.
- A compiled `KingmakerGunslinger.dll`.
- A Unity Mod Manager install ZIP.
- Successful Harmony patch installation.
- An observed Test Musket trace.
- Any touch-AC or other gameplay change.

## Readiness decision

The source archive is suitable for review and continuation, but it is not something to install. A valid user-test artifact must contain a locally compiled DLL and the UMM root folder. It will be labeled:

> **READY FOR KINGMAKER — INSTALL THIS ZIP THROUGH UNITY MOD MANAGER**
