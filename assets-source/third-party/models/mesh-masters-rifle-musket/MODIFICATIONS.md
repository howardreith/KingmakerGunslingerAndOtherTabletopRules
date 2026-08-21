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

## 2026-08-21 semantic-frame calibration

Blender 4.5 deterministically regenerated the derivative from preserved source
SHA-256 `BD3AFC3372453FAFF4742220B5E49FC7E021F10D9596E5C7000D2555FE486E18`.
The physical source frame is `+X` butt-to-muzzle / `+Z` receiver-up; the grip is
the actual narrow wrist immediately behind the trigger guard. Renderer-bound
endpoints and the full WeaponForward/WeaponUp frame are recorded in the
generation report. The Musket support station is `0.374 m`, measured against
the exact native Heavy Crossbow IK control. Output SHA-256 is
`C5E2EA93E903782BF3110E50C1D6677C4E7C109248651495192D8B6063F73A0A`.
No preserved source geometry or texture was overwritten.
