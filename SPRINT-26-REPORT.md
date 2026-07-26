# Sprint 26 report — native definition-sized misfire burst

Version: `0.0.26-s26-misfire-burst`  
Target: Pathfinder: Kingmaker 2.1.7b / UMM 0.32.4 / Harmony 1.2.0.1 / .NET Framework 4.7 / C# 7.3

## Entry decision

Sprint 25 is runtime-accepted from the supplied screenshots. They prove first-misfire no-damage behavior, condition-preserving Broken reload, one exact-wielder Reflex DC 12 save, native half-damage, one applied second-misfire event, empty/Wrecked final state, and zero relevant fault/duplicate counters.

## Bounded implementation

Sprint 26 adds only the nearby-unit portion of the already-proven second-misfire consequence.

- `FirearmDefinition` and `FirearmDefinitionComponent` now carry a validated misfire-burst radius. The Early Musket declares 5 feet.
- Exact Kingmaker 2.1.7b spatial contracts were inspected before implementation.
- The burst origin is the exact current wielder's native `Position`.
- `GameHelper.GetTargetsAround` receives the exact radius, line of sight enabled, and dead units excluded.
- The exact wielder is inserted explicitly once.
- Query candidates are deduplicated by object reference and nearby targets are deterministically ordered by native mechanics distance, stable unit identity, and display name.
- The exact wielder resolves last.
- Each planned unit gets a fresh native Reflex DC 12 save and fresh exact-weapon base damage bundle.
- Attack-level and per-unit reference gates prevent duplicate delivery.
- Exact attack, source weapon event, runtime item, current wielder, repository identity, and empty/Wrecked state are revalidated.
- Per-target failures are recorded without rolling back the committed Wrecked state or attempting broad retries.

No manual transform-center radius, physics overlap, reused mutable damage bundle, direct HP mutation, guessed damage type, or global dice patch is used.

## Diagnostics

The existing `Second-misfire explosion` line now includes query and target counters:

```text
queries; queryCandidates; plannedTargets
targetAttempts; targetApplied; targetRejected; targetDuplicates; targetFaults
```

The final applied record includes ordered per-target save, damage, distance, identity, exact-wielder, and HP evidence.

## Deliberate deferrals

Sprint 26 does not add scatter triple damage, firearm destruction, repair gameplay, Quick Clear, Gunsmithing, make whole, Rapid Reload, additional firearm blueprints, magical firearms, custom explosion assets, or Gunslinger class progression.

## Qualification

**READY FOR KINGMAKER — Sprint 26 native misfire-burst smoke test**

- Exact Kingmaker 2.1.7b private-reference Release compile: passed.
- .NET Framework 4.7 / C# 7.3 / AnyCPU: passed.
- Warnings as errors: enabled and passed.
- Same-output-path compile runs: 2.
- DLL and PDB outputs: byte-identical.
- Dependency-free suite: 540 tests × 3 runs, 0 failures.
- Repeated test output: byte-identical.
- Standalone UMM package: 8 entries and exactly one project-owned binary.
- Private Kingmaker, Unity, UMM, Harmony, Newtonsoft, compiler, and framework assemblies redistributed: none.

Authoritative hashes:

```text
KingmakerGunslinger.dll
04cf26cbd1e2e70662b9fb169508730052f314b312b881366cde4e0f20124512

KingmakerGunslinger.pdb
bb6fa86cc1b17acd082695648f02000fdffdfc7c0dae9b28c3df43a60244944b

Repeated test output
802b901e77a9dd3be20fb5cc969b8f563316584e8aa7510d84ab9a979b17378c

Standalone UMM ZIP
3f8bcd2aa22554b87ba2537506a5ffa74b288b966a99873827c4aa0416132bf3
```

## Runtime status

Runtime acceptance is pending for the exact 0.0.26 standalone package. Sprint 27 remains blocked until `SMOKE-TEST-GUIDE-0.0.26.md` passes.
