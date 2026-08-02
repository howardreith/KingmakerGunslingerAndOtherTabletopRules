# Sprint 95 native scatter geometry adapter

## Acceptance

- Resolve the PnP 15-foot distance through the canonical Sprint 94 boundary.
- Query candidates through `GameHelper.GetTargetsAround`.
- Delegate angle, line of sight, corpulence, and boundary inclusion to exact
  `AbilityDeliverProjectile.WouldTargetUnitCone`.
- Aim from caster eye position toward the selected target eye position.
- Exclude caster, nulls, duplicate references, null enumeration, and zero
  direction without guessing.
- Do not grant an action, unlock the item, dispatch attacks, or consume state.

Qualification requires the focused source contract, repository validation,
all domain/reflection tests, clean exact-reference Release build, strict
package validation, and staged safety audits.
