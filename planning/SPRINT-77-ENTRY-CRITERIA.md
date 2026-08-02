# Sprint 77 entry criteria: Targeting Head native-damage evidence

The second and final guarded Targeting Head attempt observed an exact hit,
grit `3 -> 2`, chamber `1 -> 0`, target damage `0 -> 5`, one-round Confusion,
and cleanup. Its only failed assertion read `RuleAttackWithWeapon.MeleeDamage`,
but the deed deliberately dispatches the hit's native `RuleDealDamage` as a
separate rule, so that convenience property is not the damage authority.

Correct only the observer: measure the exact target damage delta around the
native firearm delivery. Require a positive delta alongside the existing hit,
grit, chamber, rider, duration, and isolation assertions. Do not change deed
mechanics, firearm delivery, or damage production.

The two-attempt limit prohibits another guarded Targeting Head run. Require
inherited source validation, the complete domain suite, runner/preflight checks,
clean Release build, and strict package validation. Record the corrected
observer as source-qualified with strong retained runtime evidence, then
continue to an independent coverage item.
