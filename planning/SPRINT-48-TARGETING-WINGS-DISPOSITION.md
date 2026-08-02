# Sprint 48 Targeting Wings disposition

## Authority

The tabletop Targeting deed permits a wings shot only against a creature using
wings to fly and makes that creature fall. The autonomous mission permits
`OMITTED-NO-MEANINGFUL-INTERACTION` only after the installed Kingmaker contract
has been inspected and the disposition is recorded explicitly.

## Installed Kingmaker 2.1.7b inspection

A read-only exact-word IL metadata scan of the installed `Assembly-CSharp.dll`
searched for `flying`, `flight`, `wing`, `wings`, `airborne`, and `grounded`.
Controller-library (`Rewired`) names were excluded. Exactly two remaining
symbols were present:

- `Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.Wing`
- `Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.Wings`

The first is a visual attack-animation selector. The second only groups
activatable abilities. Neither identifies a unit as airborne, governs movement,
represents altitude, exposes wing count, applies falling, or provides a
flight-loss rule event. No general flying/airborne/grounded state or supported
flight maneuver was found.

## Disposition

`OMITTED-NO-MEANINGFUL-INTERACTION` is confirmed. Guessing from creature or
blueprint names, disabling arbitrary abilities in the `Wings` UI group, or
substituting prone would invent semantics and would affect creatures that are
not mechanically flying. Revisit only if a supported general flight-state and
flight-loss contract is introduced.
