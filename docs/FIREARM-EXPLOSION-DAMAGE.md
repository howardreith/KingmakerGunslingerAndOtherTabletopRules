# Firearm second-misfire explosion burst

## Trigger boundary

Only a successfully discharged exact marked firearm whose natural d20 is in its configured misfire range can enter this system.

```text
NormalToBroken  -> no explosion
BrokenToWrecked -> DamageBurst
```

The loaded round is consumed first. The attack is forced to miss. The exact item's token-backed state is committed empty/Wrecked before any save or damage event begins.

## Exact native spatial contract

Version 0.0.26 uses the installed Kingmaker 2.1.7b method:

```csharp
GameHelper.GetTargetsAround(
    Vector3 point,
    Feet radius,
    bool checkLOS,
    bool includeDead)
```

The call is:

```text
point       = exact current wielder.Position
radius      = exact firearm definition MisfireBurstRadiusFeet
checkLOS    = true
includeDead = false
```

The installed method enumerates game-state units, excludes dead units when requested, excludes untargetable units, uses native mechanics distance and unit corpulence, and applies native line of sight. The Test Musket radius is 5 feet.

## Deterministic target plan

The exact wielder is inserted explicitly once. Native query candidates are deduplicated by object reference. Qualified nearby units are sorted by:

1. native mechanics distance;
2. stable Kingmaker unit identity; and
3. display name.

The exact wielder is resolved last. This prevents lethal self-damage from stopping already-qualified nearby units while retaining deterministic evidence. It does not change which units qualify.

## Per-target native delivery

Each planned unit receives a fresh:

1. `RuleSavingThrow` for Reflex DC 12; and
2. `RuleDealDamage` using a fresh base-damage bundle created from the exact runtime firing weapon's current dice, exact blueprint damage type, exact item, and exact size.

The damage event is non-critical and precision damage is disabled. `HalfBecauseSavingThrow` exactly matches the native save result. No direct HP mutation, reused target damage bundle, global dice patch, or guessed damage type is used.

The forced-firearm natural d20 diagnostic does not affect any Reflex save.

## Correlation and at-most-once behavior

Before the query and before each target delivery, runtime code validates the exact attack roll, source `RuleAttackWithWeapon`, firing item reference, current wielder, item repository identity, and committed empty/Wrecked state.

One reference gate prevents the same attack-roll object from applying the burst twice. A second per-unit reference gate prevents any queried duplicate from receiving a second save/damage pair. The deterministic planner also reports duplicates observed from the native query.

## Failure behavior

Condition state is authoritative and is not rolled back if native spatial, save, or damage delivery fails. A failed target is not broadly retried. The runtime continues to remaining already-planned targets where safe, records per-target evidence, then records an aggregate burst fault if the applied count differs from the plan.

## Diagnostics

The UMM panel reports burst-level and target-level counters:

```text
scheduled; attempts; applied; notRequired; rejected; duplicates; faults
queries; queryCandidates; plannedTargets
targetAttempts; targetApplied; targetRejected; targetDuplicates; targetFaults
```

A successful second misfire with one nearby unit plus the exact wielder should add one scheduled burst, one attempt, one query, two planned targets, two target attempts, two target applications, and one applied burst, with no rejection, duplicate application, or fault.

The final `APPLIED` record lists every target's stable identity, native distance, exact-wielder flag, Reflex natural/total/result, half-damage flag, native damage stages, HP before/after/loss, and final empty/Wrecked firearm state.

## Deliberate deferrals

Version 0.0.26 does not implement scatter triple damage, firearm item destruction, repair, Gunsmithing, Quick Clear, make whole, Rapid Reload, new firearm types, custom explosion visuals, or class progression.
