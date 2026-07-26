# ADR-0033: use Kingmaker's native spatial query for the second-misfire burst

- Status: Accepted for Sprint 26 smoke testing
- Date: 2026-07-16

## Context

Sprint 25 proved the exact current wielder can receive one native Reflex DC 12 save and one native base weapon-damage event after an exact loaded/Broken firearm misfires and becomes empty/Wrecked. The next bounded rule portion requires nearby living creatures in the firearm's listed burst radius without inventing incompatible scene geometry.

The exact Kingmaker 2.1.7b assembly exposes `GameHelper.GetTargetsAround(Vector3, Feet, bool checkLOS, bool includeDead)`. Inspection established that it enumerates game-state units, excludes dead units when requested, excludes untargetable units, uses `UnitEntityData.DistanceTo(Vector3)` and native corpulence, and optionally applies native line of sight.

## Decision

Use the exact current wielder's `Position` as the query origin. Pass the exact firearm definition's validated `MisfireBurstRadiusFeet` through Kingmaker's `Feet` type. Enable line of sight and exclude dead units.

Insert the exact wielder explicitly once, independently of the query. Deduplicate query results by object reference. Sort nearby targets by native mechanics distance, stable unit identity, and display name. Resolve nearby units first and the exact wielder last.

Create a fresh native Reflex DC 12 save and fresh exact-weapon base damage bundle for each planned unit. Keep the existing attack-roll reference gate and add a per-unit reference gate. Revalidate exact item identity, repository identity, and empty/Wrecked state before each target.

## Consequences

- Native Kingmaker geometry, occupied space, targetability, and line of sight determine qualification.
- The exact wielder is never lost because a native query omits self.
- Query duplicates cannot cause duplicate saves or damage.
- Deterministic ordering makes evidence reproducible and prevents lethal self-damage from aborting nearby deliveries.
- A partial native target failure is visible and does not roll back the already-committed Wrecked state.
- Visual center distance may not match native mechanics distance at the edge of the burst.

## Rejected alternatives

- Manual Euclidean distance over transform centers: rejects native corpulence and mechanics-distance behavior.
- Broad physics overlap queries: risks including non-unit colliders and bypassing Kingmaker targetability rules.
- Reusing one `DamageBundle` for multiple targets: risks mutable rule-event state leakage.
- Letting query order control resolution: produces nondeterministic evidence and may stop nearby delivery after lethal self-damage.
- Restoring or keying state through `ItemEntityWeapon.UniqueId`: unrelated to spatial delivery and already rejected by runtime evidence.

## Deferred

Scatter triple damage, nonmagical firearm destruction, repair gameplay, custom explosion visuals, and broader firearm content remain outside Sprint 26.
