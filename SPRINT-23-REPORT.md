# Sprint 23 report — 0.0.23 natural-roll misfire detection

## Outcome

Sprint 22's complete Kingmaker runtime gate is accepted. Version 0.0.23 implements the bounded Sprint 23 slice: observe the final main natural d20 of a successfully discharged exact firearm, classify the Test Musket's natural 1-2 misfire range, and force a detected misfire to miss without changing firearm condition.

The implementation is source-complete. Final exact-reference compilation, repeated tests, package validation, and artifact hashes are recorded in this report after qualification.

## Accepted entry evidence

The supplied 0.0.22.1 live evidence proves:

- loaded-state preservation through quicksave and complete save/exit/restart/load;
- one-round consumption from the exact firing Test Musket;
- functional full-round reload and atomic powder-plus-Lead-Ball consumption;
- empty forced miss without attack-time inventory consumption;
- loaded Broken discharge with Broken retained;
- Wrecked forced miss with Wrecked retained;
- native Heavy Crossbow exclusion; and
- zero observed attack-enforcement, AC, reload, token-reconciliation, duplicate, or Harmony faults.

The formal assessment and original screenshots are preserved under `evidence/sprint22-runtime-acceptance/`.

## Exact runtime contract

Private-reference inspection of Kingmaker 2.1.7b establishes this main-roll flow:

```text
RulebookEvent.Dice.D20
RuleAttackRoll.set_Roll(RulebookEvent.RollEntry)
RuleAttackRoll.get_Roll()
RollEntry.op_Implicit(RollEntry)
RuleAttackRoll.IsSuccessRoll(int)
```

Version 0.0.23 binds only the exact private setter and exact public evaluator. The contract evidence is under `evidence/sprint23/runtime-contracts/`.

## Implementation

- `FirearmMisfireDecision` and `FirearmMisfireService` implement the pure rule `nativeSuccess && naturalD20 > misfireValue`.
- `FirearmDischargeRuntime` registers a short-lived context only after exact firearm validation and a verified Fired state transition.
- `RuleAttackRollNaturalRollSetterPatch` observes or deterministically replaces only the main `RollEntry` for that registered attack.
- `RuleAttackRollMisfireDecisionPatch` can only force the native boolean result to false within the configured misfire range.
- `ForcedNaturalRollQueue` is a process-local, thread-safe, single-slot diagnostic queue.
- `FirearmMisfireRuntime.FinishAttack` removes the short-lived context and reports eligible attacks that completed without a natural d20 while preserving a pending forced roll.
- UMM controls queue natural 1, 2, 3, or 20 and cancel a pending value.
- Diagnostics expose eligible attacks, natural rolls, ordinary results, misfires, forced applications, duplicate callbacks, no-natural-roll completions, faults, and pending state.

## Isolation

A native Heavy Crossbow, empty firearm, and Wrecked firearm never receive a misfire context and cannot consume a pending forced roll. Critical confirmation uses a different property and is not patched by the force-next-roll diagnostic.

## Scope deliberately deferred

Version 0.0.23 does not call the firearm-state misfire transition. A detected misfire leaves Normal or Broken condition unchanged. Normal → Broken and Broken → Wrecked are bounded to Sprint 24 after live acceptance.

No explosions, area damage, repair gameplay, iterative automatic reload, Rapid Reload, additional firearms, scatter behavior, or Gunslinger class mechanics were added.

## Qualification

The source and install candidate completed the full non-runtime qualification gate:

- portable Sprint 23 source-invariant validation: **passed**;
- exact Kingmaker 2.1.7b private-reference Release compilation: **passed**;
- .NET Framework 4.7, C# 7.3, AnyCPU, warnings as errors: **passed**;
- same-output-path exact-reference compilation: **2 runs**;
- `KingmakerGunslinger.dll` and `KingmakerGunslinger.pdb`: **byte-identical across both runs**;
- dependency-free domain suite: **489 tests × 3 runs, 0 failures**;
- repeated test output: **byte-identical**;
- strict standalone UMM package validation: **passed**;
- standalone entries: **8**;
- packaged binaries: **exactly one project-owned DLL**; and
- private Kingmaker, Unity, UMM, Harmony, and Newtonsoft binaries redistributed: **none**.

Authoritative qualification hashes:

```text
KingmakerGunslinger.dll
1d04c99705279524d4877888d3803fded1ab04ca988669b501c53db451e47a63

KingmakerGunslinger.pdb
684d9b4eeaa85303062fcc75f46b30ed9f482e818f1de8200debbf3ea488ab1e

Repeated test output
86c65e395cc29674a5776e6f6f2468aa31ff57808fc5836de8fe1f51574da5f6

Standalone UMM ZIP
d09a3ced53c8f77c5b11dee9feb20b4e409a35d2dc67922ee81740d2d82c21b2
```

The install artifact is **READY FOR KINGMAKER — Sprint 23 natural-roll misfire smoke test**. This label means the strict standalone UMM ZIP exists and passed compile/package qualification; it does not mean the Sprint 23 live gate has passed.

## Runtime acceptance required

The standalone package is a runtime candidate only. `SMOKE-TEST-GUIDE-0.0.23.md` must prove forced 1/2 misfires, forced 3/20 ordinary results, exact one-round consumption, unchanged condition, native Heavy Crossbow isolation, empty/Wrecked queue preservation, no-natural-roll handling, persistence regression, and zero relevant faults before Sprint 24 opens.
