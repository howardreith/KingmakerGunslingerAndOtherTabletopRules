# Sprint 35 Firearm Grit Recovery Qualification

## Qualified source scope

The production grit adapter now accounts separately for the two native firearm
recovery clauses: confirmed critical and killing blow. Each qualifying clause
restores exactly one point through Kingmaker's native per-unit resource. Weak
reference-identity gates prevent duplicate callbacks without retaining combat
events or sharing state between units.

## Exact native contracts

- `RuleAttackRoll.IsCriticalConfirmed` is final after its native trigger.
- The exact production firearm item is recovered from the attack roll and must
  contain exactly one Gunslinger firearm marker.
- Installed `RuleAttackWithWeapon.OnTrigger` IL creates the attack roll, stores
  the exact native damage rule in `MeleeDamage`, then triggers that same rule.
- `RuleDealDamage.AttackRoll` correlates damage to the exact attack roll.
- A killing blow requires the target to cross from positive `HPLeft` to zero or
  less with positive damage; already-disabled targets do not qualify.
- Explosion and secondary damage are excluded because they are not the exact
  `RuleAttackWithWeapon.MeleeDamage` reference.
- Native `IsInCombat`, `UnitState.IsHelpless`, and progression character levels
  implement heat-of-combat, helpless/unaware, and below-half-level exclusions.

## Duplicate and isolation policy

- Confirmed critical is marked once per `RuleAttackRoll` reference.
- Killing blow is marked once per attack-roll and exact target reference.
- The two clauses remain distinct, so one firearm shot satisfying both may
  restore one point for each clause.
- Only an initiator with an exact Gunslinger class level and owned grit resource
  may restore grit.

## Source gates

- Focused grit policy checks: 11 PASS.
- Focused detached recovery checks: 7 PASS.
- Runtime scenario preflight checks: 40 PASS.
- Complete dependency-free domain/reflection suite: 710/710 PASS.
- Repository validation, clean private-reference Release build, build-output
  validation, and strict standalone packaging: PASS.
- Candidate package SHA-256:
  `838a3882e5998da9496aaa608a55dfe73ced2433079f8f7ac97aee1b4d25047a`.
- Candidate DLL SHA-256:
  `d589582a7a27d1e146009f7352a62bbd5e0e8c97fd6e96a02fef513dd6122c90`.

## Runtime acceptance plan

The guarded `disposable-gunslinger-grit-recovery` scenario uses two detached
units and no save API. It creates an exact production Pistol attack context,
requires grit `0 -> 1` on confirmed critical and `1 -> 2` on killing blow,
replays both callbacks to prove no second mutation, then spends to one and
proves an unaware-target critical remains one. Temporary detached combat state,
target damage, controllers, and entities are cleared or disposed before exact
party/global-unit reference snapshots are compared.

Require exact-commit `mod-load-smoke` and two independent fresh-process PASS
runs before changing this row to `RUNTIME-QUALIFIED`.
