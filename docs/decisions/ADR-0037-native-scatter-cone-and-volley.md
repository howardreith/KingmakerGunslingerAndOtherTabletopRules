# ADR-0037: native scatter cone geometry and independent volley rolls

## Status

Accepted for Sprint 32 source development. Numeric Blunderbuss cone distance
remains unresolved and fail-closed.

## Installed contract evidence

Read-only reflection and narrow IL disassembly of Kingmaker 2.1.7b established:

- `UnitEntityData` exposes `Position`, `EyePosition`, `Orientation`, and
  `OrientationDirection`.
- `Kingmaker.Designers.GameHelper.GetTargetsAround(Vector3, Feet, Boolean,
  Boolean)` provides the already-qualified native candidate enumeration path.
- `AbilityDeliverProjectile.WouldTargetUnitCone(UnitEntityData caster,
  UnitEntityData unit, Vector3 launchPos, Vector2 castDir, Single distance)`
  excludes the caster and delegates to the point-cone test using target eye
  position and view corpulence.
- `WouldTargetPointCone` rejects line-of-sight obstacles, limits center distance
  to `distance + targetRadius`, uses a fixed 45-degree half-angle, and includes
  corpulent targets intersecting either cone edge. This is a 90-degree cone.
- `RuleAttackWithWeapon(UnitEntityData, UnitEntityData, ItemEntityWeapon,
  Int32)` is the native per-target weapon attack constructor and exposes
  `AttackBonusPenalty`.
- `RuleAttackRoll` exposes attack penalty, hit, critical, target AC, weapon, and
  parent-attack surfaces needed for independent per-target evidence.

## Decision

Reuse the exact native enumeration and cone geometry semantics through a narrow
adapter. Do not manually substitute transform-center Euclidean geometry.

Build one native `RuleAttackWithWeapon` per accepted exact target, apply the
authoritative -2 scatter penalty, and retain native hit, concealment, critical,
and damage composition except where the tabletop scatter rule explicitly
excludes precision and Vital Strike-style increases.

Aggregate misfire only after every target has its independent natural roll. A
scatter volley misfires only when at least one attack roll exists and every
roll is within the firearm's misfire range. Critical confirmation remains
target-specific.

## Fail-closed boundary

The authorized local rules identify Blunderbuss range as `special` but do not
state a numeric cone distance. Neither the 90-degree engine shape nor a caller-
supplied `distance` proves that missing content value. The production
Blunderbuss remains unavailable and the runtime adapter must reject an absent
distance until explicit project authority resolves it.

No proprietary IL, assembly, or machine-local inspection artifact is committed.
