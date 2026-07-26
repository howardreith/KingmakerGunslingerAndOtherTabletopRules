# Sprint 25 report — second-misfire exact-wielder damage

Version: `0.0.25-s25-second-misfire-explosion`  
Target: Pathfinder: Kingmaker 2.1.7b / UMM 0.32.4 / Harmony 1.2.0.1 / .NET Framework 4.7 / C# 7.3

## Entry decision

Sprint 24.1 is runtime-accepted from the supplied screenshots. The evidence proves:

- first misfire: exact loaded/Normal item became empty/Broken;
- empty/Broken reload: one powder and one Lead Ball were consumed while condition remained Broken;
- second misfire: exact loaded/Broken item became empty/Wrecked;
- Wrecked reload was unavailable;
- a Wrecked attack incremented `wreckedRejected` without another discharge; and
- attack, reload, misfire, AC, trace, and token-reconciliation fault/duplicate counters remained zero.

The preserved assessment and screenshot checksums are under `evidence/sprint24-repair/runtime-acceptance-2026-07-16/`.

## Bounded implementation

Sprint 25 adds only the immediate damage consequence of a detected Broken → Wrecked misfire.

The flow is:

1. The accepted discharge path consumes the exact loaded round once.
2. Natural-d20 misfire evaluation forces the attack to miss.
3. The exact item-owned state transitions from empty/Broken to empty/Wrecked.
4. A pure explosion policy schedules damage only for that `BrokenToWrecked` decision.
5. After `RuleAttackRoll.OnTrigger` returns, the runtime adapter validates the exact attack, source weapon event, runtime item, current wielder, repository identity, and committed empty/Wrecked state.
6. It constructs one native base-damage entry from the exact runtime weapon's dice formula and blueprint damage type, then wraps it in a `DamageBundle` tied to that exact item and size.
7. It triggers one native `RuleSavingThrow` for Reflex DC 12 against the exact current wielder.
8. It triggers one native self-targeted `RuleDealDamage`, disables precision damage, correlates the exact attack roll, and sets `HalfBecauseSavingThrow` from the save result.
9. It re-reads the item and requires the same repository identity and empty/Wrecked state after damage.

No raw HP mutation, global damage patch, target-specific attack bundle, guessed dice formula, or broad fallback is used. The exact runtime item supplies the dice formula and damage type.

## Isolation and ordering

The consequence is excluded when any of these conditions is true:

- the roll was ordinary;
- the misfire was the first Normal → Broken transition;
- the attack was not a successfully discharged exact marked firearm;
- the firearm was empty or Wrecked before attack-time discharge;
- the displayed Heavy Crossbow is a native Heavy Crossbow without the exact firearm marker;
- the attack, item, wielder, source weapon event, or repository identity no longer matches; or
- the exact item is not empty/Wrecked when damage is about to run.

Condition transition is authoritative and precedes damage. If the native save/damage event faults, the firearm remains empty/Wrecked and the runtime records the failure. It does not roll back the condition or attempt a broad retry.

A per-context scheduling gate plus a reference-identity event gate prevents one `RuleAttackRoll` object from applying damage twice.

## Diagnostics

The UMM panel exposes:

```text
Second-misfire explosion:
scheduled=<n>; attempts=<n>; applied=<n>; notRequired=<n>;
rejected=<n>; duplicates=<n>; faults=<n>; last=<details>
```

For one first misfire followed by one second misfire, the expected cumulative totals are:

```text
scheduled=1
attempts=1
applied=1
notRequired=1
rejected=0
duplicates=0
faults=0
```

The applied record includes the exact wielder, attack-roll reference identity, exact item repository identity, runtime weapon-damage formula, Reflex DC/natural roll/total/pass result, the native half-damage flag, native damage stages, HP before/after/loss, and final empty/Wrecked state.

## Deliberate deferrals

Sprint 25 does not add:

- nearby-creature burst enumeration or spatial geometry;
- nonmagical item destruction;
- repair, Gunsmithing, Quick Clear, or make whole behavior;
- automatic iterative reloads or Rapid Reload;
- pistols, scatter weapons, additional firearm blueprints, magical firearms, or custom assets; or
- Gunslinger class progression, grit, deeds, or enemy firearm AI.

The tabletop burst portion is bounded to Sprint 26 after the exact-wielder event is proven in Kingmaker.

## Qualification

The complete non-runtime qualification gate passed:

- exact Kingmaker 2.1.7b private-reference Release compile;
- .NET Framework 4.7, C# 7.3, AnyCPU;
- warnings as errors;
- two same-output-path deterministic builds;
- byte-identical DLL and PDB output;
- **513 tests × 3 runs, 0 failures**;
- byte-identical repeated test output;
- strict eight-entry standalone UMM package validation;
- exactly one packaged binary, `KingmakerGunslinger.dll`;
- no redistributed Kingmaker, Unity, UMM, Harmony, Newtonsoft, compiler, or framework assemblies; and
- portable Sprint 25 source invariant validation.

Authoritative hashes:

```text
KingmakerGunslinger.dll
04bef18c866d6279de9cde96583b77b7655286d2c2bfafff0d1a91b096903c9e

KingmakerGunslinger.pdb
e817ff2d260a0795d36ebddeb40413ae3b0c8b59ec8843a2cc157863fd1cd6b5

Repeated test output
0c080a58d1162dd6ba73afcdeba6aad7a286b1ce5b877bdb8c09d46e66320478

Standalone UMM ZIP
8afff4790f3af832993ae6901a3062f31791ea817188e7eca3673c4025f3fb21
```

The detailed machine-readable record is `BUILD-QUALIFICATION.json`. Executed compile, test, and package evidence is under `evidence/sprint25/`.

## Runtime status

**READY FOR KINGMAKER — Sprint 25 second-misfire explosion smoke test.**

The standalone 0.0.25 ZIP is artifact-qualified but not yet runtime-accepted. The exact package must pass `SMOKE-TEST-GUIDE-0.0.25.md`, including first-misfire no-damage behavior, one exact-wielder Reflex DC 12 save and one native damage event on the second misfire, at-most-once behavior, isolation, Wrecked rejection, and persistence. Sprint 26 remains blocked until that live gate passes.
