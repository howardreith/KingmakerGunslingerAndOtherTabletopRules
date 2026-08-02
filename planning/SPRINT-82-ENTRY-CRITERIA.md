# Sprint 82 entry criteria: native weapon-pipeline preservation

The mandatory coverage row requires firearms to preserve Kingmaker's native
concealment, mirror-image, cover, line-of-sight, range-penalty, critical, and
damage behavior. These are native pipeline responsibilities, not parallel
Gunslinger implementations.

Qualification therefore requires compositional evidence:

- ordinary firearm delivery remains `RuleAttackWithWeapon` on a production
  `BlueprintItemWeapon`;
- weapon-attack observation has no result replacement;
- attack-roll hooks bracket native execution and do not skip `OnTrigger`;
- misfire changes only the final native success boolean in a postfix and can
  only turn success into failure;
- touch-AC selection runs after native `RuleCalculateAC` and applies the
  ordinary-to-touch delta to the current contextual `TargetAC`;
- runtime acceptance retains native damage and critical evidence while the
  source-qualified domain suite covers contextual cover and flat-footed deltas,
  native isolation, range boundaries, and fail-closed behavior.

Do not create substitute concealment, mirror-image, cover, LOS, attack,
critical, or damage systems. No new runtime launch is needed unless this audit
changes the shipped assembly; the exact `58baf84` assembly already passed mod
load and two comprehensive fresh-process runs.
