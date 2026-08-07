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
