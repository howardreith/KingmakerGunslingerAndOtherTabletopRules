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

The semantic-anchor pass declares source Grip `(0.01,0,-0.00316)`, Support
`(-0.0125,-0.00255,-0.00471)`, Butt `(0.01565,0,-0.00316)`, and Muzzle
`(-0.02675,0,-0.00316)`, yielding `0.848 m` length and Heavy-Crossbow-derived
support clearance `(-0.031,-0.051,0.45)` in the grip frame.

## 2026-08-20 normalized production derivative

Issue 11 derives
`../firearm-long-gun-derivatives/blunderbuss-normalized.fbx` deterministically
from the preserved original. The `0.86 m` derivative normalizes units and the
barrel axis, authors exact grip/support/butt/muzzle/back markers, and applies a
muted material tint. The original geometry and source files remain preserved.
