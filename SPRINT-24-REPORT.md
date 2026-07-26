# Sprint 24 report — 0.0.24 exact-item misfire condition transitions

## Outcome

Version 0.0.24 implements the bounded Sprint 24 slice: after an exact marked firearm has successfully discharged its loaded round and its final natural d20 is classified as a misfire, mutate only that exact item from Normal to Broken or from Broken to Wrecked.

The implementation is source-complete. Final exact-reference compilation, repeated tests, package validation, and artifact hashes are recorded below after qualification.

## Entry decision: user-approved carry-forward

The observed 0.0.23 evidence proved the core natural-roll boundary:

- forced natural 1 was classified as a misfire and missed;
- forced natural 2 converted a native hit into a final miss;
- forced natural 3 retained ordinary hit behavior; and
- forced natural 20 retained ordinary critical behavior.

Several formal Sprint 23 isolation and persistence controls were not separately captured. The user explicitly directed that Sprint 24 proceed and that those controls be tested with the next material. They are therefore not retroactively marked passed; they are carried into the combined 0.0.24 smoke test. The assessment and original screenshots are preserved under `evidence/sprint23-runtime-acceptance/user-approved-carry-forward-2026-07-16/`.

## Implementation

- `FirearmMisfireConditionService` derives a pure deterministic state consequence from one immutable natural-roll decision and one already-empty post-discharge state.
- `FirearmMisfireConditionDecision` verifies ordinary no-op behavior, exact Normal → Broken and Broken → Wrecked shapes, and empty state before and after.
- `FirearmDischargeRuntime` registers the exact runtime weapon object and verified post-discharge snapshot only after one-round consumption commits.
- `FirearmMisfireRuntime` retains that exact object, expected empty state, and repository identity in the short-lived `RuleAttackRoll` context.
- The first exact `IsSuccessRoll(int)` evaluation may commit one condition transition through `FirearmRuntimeState.Service.Transition`.
- The transition rejects intervening state changes and verifies both the final state and repository identity.
- Duplicate evaluations enforce the miss result but cannot repeat condition damage.
- Diagnostics add `normalToBroken` and `brokenToWrecked` counters plus complete pre/post state text.

## Persistence and identity

The accepted item-owned inert `BlueprintWeaponEnchantment` token repository remains the only runtime state carrier. Empty/Normal remains the absence of a state token; empty/Broken and empty/Wrecked use the existing stable token blueprints.

The runtime-rejected `ItemEntityWeapon.UniqueId` vault was not revived. No display name, Heavy Crossbow category, owner, equipment slot, inventory position, or process hash is used as persistent identity.

## Scope deliberately excluded

Version 0.0.24 adds no explosion, splash, area, or wielder damage; gameplay repair or Quick Clear; automatic iterative reload; Rapid Reload; pistols, scatter weapons, additional firearm blueprints, magical firearms, or Gunslinger class behavior.

## Qualification

**READY FOR KINGMAKER — Sprint 24 misfire-condition smoke test**

The non-runtime qualification gate passed:

- exact Kingmaker 2.1.7b private-reference Release compile;
- .NET Framework 4.7, C# 7.3, AnyCPU;
- warnings as errors and deterministic compiler mode;
- two same-output-path compiles with byte-identical DLL and PDB outputs;
- **501 tests × 3 runs, 0 failures**, with byte-identical output across all runs;
- strict standalone UMM package validation: 8 entries and exactly one project-owned binary; and
- no private Kingmaker, Unity, UMM, Harmony, or Newtonsoft assemblies redistributed.

Authoritative qualification hashes:

```text
KingmakerGunslinger.dll
eeb238615d53a45e6f8d8f55797c5b47918319b96b543d81c0b6dbe9fd33f040

KingmakerGunslinger.pdb
7d82bc41946d9e940433dd7510e8deb38424095954ece8540302868dcd3175cf

Repeated test output
3d097f8716ea6c976d5c23e1ed7b0fb5ef47c47edc334c8c98e4228ffea1f079

Standalone UMM ZIP
bc67df1ceda974dc44c8b8407798d919575facad1226d64ed969c698d96f8d4d
```

The user-approved carry-forward permits this combined test candidate to proceed without falsely claiming that the uncaptured Sprint 23 controls were already observed. Runtime acceptance remains pending for this exact 0.0.24 package, and Sprint 25 remains blocked.


## Runtime acceptance required

The standalone package is a runtime candidate only. `SMOKE-TEST-GUIDE-0.0.24.md` must prove:

- Normal misfire → empty/Broken on the exact firing item;
- Broken misfire → empty/Wrecked on the exact firing item;
- at-most-once transition behavior;
- ordinary natural 3/20 preservation;
- exact-item isolation across two Test Muskets;
- native Heavy Crossbow, empty, and Wrecked forced-roll queue isolation;
- one observed `noNaturalRoll` queue-preservation path;
- Broken and Wrecked quicksave plus full save/exit/restart/load persistence; and
- zero relevant faults, duplicates, and token conflicts.

Sprint 25 remains blocked until the complete combined gate passes.
