# Modifications

No modifications have been recorded yet.

Append every conversion or edit, including tool versions, geometry/audio
changes, original and derivative SHA-256 hashes, and the responsible commit.
Never overwrite the original downloaded source.

## 2026-08-07 held-prefab rendering repair

The preserved source payload was not modified. Unity 2018.4.10f1 prefab
authoring now removes any LODGroup after retaining LOD0, validates non-mirrored
positive hierarchy scales, and assigns a project-owned double-sided diffuse
shader to held Musket renderers only. Belt/back materials remain separately
calibrated and unchanged. This addresses view-dependent disappearance without
changing the accepted held grip, support target, muzzle, or animation.

The finishing audit retired the custom shader and generated reverse-wound
backfaces under opaque Standard materials. Musket's held transform was unchanged;
measured rendered bounds are `0.6529304 m`.

The semantic-anchor pass declares source Grip `(0.04,0,0)`, Support
`(-0.10,-0.0122,-0.0074)`, Butt `(0.0805,0,0)`, and Muzzle `(-0.242,0,0)`.
Scale is derived to produce `1.349985 m` butt-to-muzzle length around the grip.

## 2026-08-20 normalized production derivative

Issue 11 derives
`../firearm-long-gun-derivatives/musket-normalized.fbx` deterministically from
the preserved original. The `1.34 m` derivative normalizes units and the barrel
axis, authors exact grip/support/butt/muzzle/back markers, and applies a muted
material tint. The original geometry and source files remain preserved.
