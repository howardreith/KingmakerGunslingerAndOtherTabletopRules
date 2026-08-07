# Modifications

No modifications have been recorded yet.

Append every conversion or edit, including tool versions, geometry/audio
changes, original and derivative SHA-256 hashes, and the responsible commit.
Never overwrite the original downloaded source.

## 2026-08-07 held-prefab rendering repair

The preserved source payload was not modified. Unity 2018.4.10f1 prefab
authoring now removes any LODGroup after retaining LOD0, validates non-mirrored
positive hierarchy scales, and assigns a project-owned double-sided diffuse
shader to held Blunderbuss renderers only. Belt/back materials remain separately
calibrated and unchanged. The held grip, support target, muzzle, and animation
are unchanged.

The finishing audit measured the held result at only `0.024450602 m`. Visual
scale was corrected from `0.5` to `20`, producing `0.978023946 m` bounds without
changing the grip root, support target, muzzle, or Crossbow animation. Opaque
Standard materials and generated reverse-wound backfaces replace the failed
custom shader experiment.
