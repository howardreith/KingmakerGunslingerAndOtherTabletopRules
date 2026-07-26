# ADR-0032 — Resolve the second-misfire consequence through one exact-wielder native save and damage event

Status: Accepted for Sprint 25  
Date: 2026-07-16

## Context

Sprint 24.1 proved that one exact item can progress from Normal to Broken, reload while preserving Broken, then progress to Wrecked on a second natural-roll misfire. Sprint 25 must add the immediate consequence without weakening exact-item isolation or patching a broad damage pipeline.

The Pathfinder early-firearm rule calls for an explosion when a firearm misfires while already Broken. The wielder is affected, takes damage as though hit by the weapon, and may make a Reflex DC 12 save for half. The complete rule also describes a burst, but Kingmaker spatial-query and burst-geometry contracts have not yet been qualified.

The installed Kingmaker 2.1.7b reference surface provides:

- `RuleSavingThrow(UnitEntityData, SavingThrowType, int)`;
- `ItemEntityWeapon.Damage` and `ItemEntityWeapon.Size`;
- `BlueprintItemWeapon.DamageType`;
- `DamageTypeDescription.CreateDamage(DiceFormula, int)`;
- `DamageBundle(ItemEntityWeapon, Size, BaseDamage)`;
- `DamageTypeDescription.CreateDamage(DiceFormula, int)`;
- `DamageBundle(ItemEntityWeapon, Size, BaseDamage)`;
- `RuleDealDamage(UnitEntityData, UnitEntityData, DamageBundle)`;
- `RuleDealDamage.HalfBecauseSavingThrow`;
- `RuleDealDamage.DisablePrecisionDamage`;
- `RuleDealDamage.AttackRoll`; and
- `Rulebook.Trigger<TEvent>(TEvent)`.

Inspection also showed that `RuleAttackWithWeapon.CreateDamage(false)` is built from the original attack's calculated weapon-stat descriptions. Those descriptions may contain attack- or target-specific additions, so reusing that bundle for self-damage could accidentally carry data calculated for the original enemy.

## Decision

For Sprint 25:

1. Trigger the consequence only from the already-proven `BrokenToWrecked` misfire-condition decision.
2. Commit and verify the exact item as empty/Wrecked before damage.
3. Retain the original `RuleAttackRoll`, exact runtime item, repository identity, and exact current wielder in the eligible-attack context.
4. Revalidate the original `RuleAttackWithWeapon` source and all reference identities before applying damage.
5. Build exactly one base weapon-damage entry from the exact runtime weapon's current `Damage` dice and blueprint `DamageType`, with zero explicit bonus.
6. Trigger one native Reflex DC 12 save against the exact wielder.
7. Trigger one native self-targeted `RuleDealDamage`, disable precision damage, correlate the original attack roll, and set `HalfBecauseSavingThrow` from the save result.
8. Verify that the exact item remains empty/Wrecked afterward.
9. Reject or fault closed without broad fallback when any exactness or native event contract fails.
10. Apply at most once per `RuleAttackRoll` object.

Nearby-creature burst targeting is deferred to Sprint 26.

## Alternatives considered

### Directly subtract hit points

Rejected. It bypasses Kingmaker's rulebook, difficulty, temporary HP, damage reduction, immunities, and third-party event composition.

### Reuse `RuleAttackWithWeapon.CreateDamage(false)`

Rejected for this slice. Its exact IL builds a bundle from the original attack's `RuleCalculateWeaponStats.DamageDescription` collection. That is useful for the original target, but it can contain attack- or target-specific modifiers that should not be blindly transferred to the wielder.

### Hard-code the Test Musket's dice or physical damage form

Rejected. The runtime item already exposes its current scaled damage dice and blueprint damage type. Reading those exact installed values avoids duplicating firearm-specific constants in the explosion adapter.

### Patch a global save or damage method

Rejected. The consequence is a one-shot event correlated to one exact attack and item. A broad patch would create unnecessary compatibility and recursion risk.

### Apply damage before changing the item to Wrecked

Rejected. Sprint 24 established the authoritative item-state ordering. Native damage failure must not leave a second-misfired firearm merely Broken.

### Implement the entire burst immediately

Rejected for Sprint 25. Exact spatial-query and distance contracts were not yet recorded. The exact current wielder is unambiguous and sufficient for a bounded first runtime proof.

### Destroy the item immediately

Deferred. Deletion would combine explosion mechanics with inventory mutation, save migration, repair, and uninstall behavior. The diagnostic build retains the accepted empty/Wrecked item state.

## Consequences

- A second misfire can damage or kill the exact current wielder.
- A successful Reflex save invokes Kingmaker's native half-damage behavior.
- A first misfire remains non-explosive and is recorded as `notRequired` rather than as a rejection.
- Ordinary rolls, native Heavy Crossbows, empty firearms, Wrecked firearms, and unrelated Test Muskets remain outside the path.
- One attack-roll object cannot apply two explosion events.
- The firearm remains empty/Wrecked even if native damage resolution faults.
- Sprint 26 remains blocked until the exact 0.0.25 package passes its live smoke test.
