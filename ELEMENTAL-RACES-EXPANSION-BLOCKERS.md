# Elemental Races expansion blockers

## Active hard blockers

None established.

## Open engineering questions requiring evidence

- The installed native/Wrath precedents and safe Kingmaker donors for each new
  heritage SLA, flight abstraction, concealment catalog, breathing catalog,
  ray deflection, summon typing, and difficult terrain remain to be audited.
- Dirty Trick is implemented only if a genuine native player-facing combat
  maneuver path can be identified and qualified.
- Visual Adjustments remains NOT-RUN unless it is actually installed when the
  relevant compatibility checkpoint is executed.

These are investigation items, not hard stops. Features fail closed only under
the mission's hard-stop contract; independent work continues.

## Resolved foundation limitations

- Kingmaker 2.1.7b exposes only `RuleCalculateAbilityParams.AddBonusDC(int)`;
  it cannot attach `ModifierDescriptor.Racial` to that event. Exact
  nonduplication is enforced by the one-result affinity policy across the
  effective parent/variant chain.
- Raw `UnitUseAbility.CanStart` does not include resource availability.
  Player-path availability is the native combined
  `AbilityData.IsAvailable && CanStart` boundary and is qualified as such.
- Viewless chargen fixtures cannot safely evaluate `CurrentSpeedMps`.
  Movement qualification uses real native buffs/conditions plus the installed
  `CalculateSpeedModifier` contract; final in-area heritage qualification
  remains required in Release A.
