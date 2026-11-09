## R7 owner-playtest revision (Nalfeshnee)

The rounded figure and slab wings are replaced on the same joints: a hunched boar-headed ape with a scapular hump, ribcage and pectoral shelf over one sagging gut, a wedge skull with flat rooting disc, underslung jaw, four tusks and a bristled mane, and wings grown from scapular mounds with three fingers and scalloped membranes. Width stays within the prior silhouette apart from the wing tips; the tallest point rises from 1.875 to 1.990 through ears and mane, with no scale change. The wings still follow the chest.

Generation stays the existing Blender 4.5.10 generator interface with `PYTHONHASHSEED=0` and the local rig capture `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`; two clean runs produced byte-identical exports. Texture coordinates now come from the scoped `continuous-bodies/outsider_bake.py`, so the five atlas tiles are authoring material IDs and the albedo is baked from bind-pose position, normal and cavity. Only project-authored geometry and closed-form colour are inputs. No mechanics, reach, collision, attack, GUID or icon changed. Offline renders and native frames are presentation evidence; owner aesthetic approval is separate.

| Current export | Vertices / triangles (or size) | SHA-256 |
| --- | --- | --- |
| `nalfeshnee-mesh.json` | 12402 / 14392 | `19161c156fa8be5cf7e86a732146c7bc39465f40fc1d0f802de38c0f255c29ed` |
| `nalfeshnee-albedo.png` | 1024 x 1024 | `15c93ef8a379bfa24dc1177d4da032f28f70d46a3fa610fde164a075eab3088c` |

Pre-R7 exports are retained at commit `70d44c8be7` and are not packaged.

## Current owner visual-overhaul revision

The November 2026 owner mission brings the previously deferred cosmetic work into scope. The current exports use opt-in `continuous-bodies/outsider_surface.py` to unite organic skin and blend the existing two-influence weights. Individual teeth, horns, feather vanes and membranes retain intentional ownership. The original rig articulation limits remain. No mechanical profile, attack, route, GUID, icon or save identity changed.

Raw geometry-free input capture SHA-256 is `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`. Before revision, this locally retained capture regenerated the released Nalfeshnee geometry blob and albedo manifest byte-for-byte. New exports truthfully name this capture; the historical `727645...ad7a4` authority is retained separately. Both are an explicit closed loader allowlist.

Normal regeneration uses the existing generator/arguments below. Blender 4.5.10 and the existing pipeline are retained; private captures, blends, reports and previews stay out of Git/packages. Exact exported front, side and party-angle previews have been inspected offline. Native lighting, motion, measured world-space scale and owner aesthetic approval remain separate and unverified until the [active checkpoint](../../../planning/SUMMON-OUTSIDER-VISUAL-OVERHAUL-STATE.md) records actual observations.

| Current export | Vertices / triangles | SHA-256 |
| --- | --- | --- |
| `nalfeshnee-mesh.json` | 5919 / 10864 | `1caa91e1f79f40ec7c1a0ee251783665ad6b0ff0cbb2ea6fad7ff9ce2ef2fc2e` |
| `nalfeshnee-albedo.png` | - | `3fa71ffd8161700f69464b6aaf3da936d16826a418b0de6f00612847f9a40b84` |

## Earlier authoring record

PR #96 review correction retains the heavy boar head/trunk, enlarges beveled
palms and flesh digits, shortens ivory claws, raises brow planes clear of the
eyes, and replaces peg-like hocks/feet with tapered calves and planted toes.
The corrected surface exporter assigns each whole face to one atlas tile and
duplicates UV seams with identical smooth normals/weights. The previous 545
cross-tile faces are now zero; all 443 duplicated seam positions have matching
normals/weights. This establishes texture correctness, not owner art approval.
Current native limitations are in the active checkpoint; historical export
identities below describe the original release.

# Sprint 39 original Nalfeshnee body

The Nalfeshnee uses distinct original procedural geometry and an original
albedo authored for Sprint 39 by `generate_sprint39_nalfeshnee.py`. The script
reuses the project's established Blender 4.5 exporter and measured free-hands
rig capture; it does not distribute native geometry, textures, materials or
animation clips. The native rig remains a runtime dependency.

The Sprint 39 installed-library observation
(`20261104T0415356825476Z-observe-expanded-summoning-sprint39-native-inputs`)
found **no installed Kingmaker Nalfeshnee** â€” zero units and zero name matches
of any type â€” so none is reused. The mechanical donor, the animation donor and
the rendered mesh are three different things and are recorded separately here:

- animation carrier: native donor `a8cbd4cdb0feaf64eab1452257c3c971`, prefab
  `d6db6ba3b2c459a48ba0730beef0bac8` â€” the same measured free-hands rig the
  released Sprint 34 to 38 outsiders ride;
- rendered mesh: this project's own geometry, below;
- mechanics: the Sprint 39 builder, which shares nothing with the rig.

The local measured capture is
`20261014T0546181325834Z-observe-expanded-summoning-sprint24-theropod-census/sprint24-theropod-census.json`,
SHA-256 `727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`,
the same hash-verified capture Sprints 37 and 38 recovered from the owned `E:`
evidence archive; its hash was re-verified before this run. No fresh guarded
game transaction was spent on a new census.

## What the silhouette is, and what it deliberately is not

The Nalfeshnee is carried by **bulk**: a pear-shaped trunk far wider at the gut
than at the shoulders, a belly that overhangs its own short legs, a broad low
boar skull with a blunt snout disc and two long tusks curving up out of the
lower jaw, small swept-back ears, two short thick clawed arms held out from the
gut, and two membranous wings that are identifying precisely by being **too
small** for the body.

That is the inverse of its two nearest neighbours on this rig, and the contrast
is the point. The Sprint 37 Vrock is lean and bird-legged with a wing span
wider than it is tall; the Sprint 38 Glabrezu is a towering upright trunk whose
two huge pincers are the widest thing in its frame. Here the wings span less
than the belly and the limbs are short, so none of the three can be confused
even as thumbnails. It is also not a recoloured Hezrou or Babau: both are
ordinary two-armed shapes without a boar head, tusks or wings.

The body creates no gameplay attack, reach, nimbus, spell-like or teleport
behaviour of its own. All three printed attacks, the delayed Unholy Nimbus,
feeblemind, greater dispel magic, the constant aura and every spell-like live
in the builder and are observed separately.

## Recorded limitations

- The rig carries **two arm chains and no wing chain**. The two arms ride the
  real animated chains and the hand claws ride the two digit chains, so the
  native grip animation still opens and closes them. The **wings are project
  geometry weighted to `Torso_Upper`**: they move with the chest and do not
  articulate independently. That is the same accepted shape the released
  Sprint 37 Vrock's wings take, and it is a recorded limitation rather than a
  claim of wing articulation.
- The head merges into the shoulder mass more than intended, so the short neck
  does not fully read as a neck.
- Surface relief is low: the bulk reads as a smooth mass broken only by the
  albedo mottle rather than as folded hide.
- At the preview's three-quarter camera the far wing is largely occluded by
  the torso.

All of these are cosmetic and non-blocking, and native presentation proof is
required separately.

## Two authoring passes, and why each happened

Recorded because the second was a real defect rather than polish.

1. The wings were first rooted at `UP .070 / FRONT -.050` and sank into the
   shoulder mass â€” present in the mesh, invisible in a render. They were
   lifted and pushed back until the membrane clears the torso silhouette.
2. The membrane was first filled with a grid of `mass()` ellipsoids, copying
   the icon renderer's approach. That renderer uses **metaballs**, which merge;
   this exporter has no merge, so the vane rendered as a string of beads â€”
   exactly the failure the Sprint 37 Vrock docstring warns about. The membrane
   is now built from overlapping wide tapering **tubes**, which are continuous
   surfaces, as the Vrock's primaries are.

## Reproduction

```
blender.exe -b --factory-startup --python-exit-code 1 --python generate_sprint39_nalfeshnee.py -- \
  --kind nalfeshnee --capture <private capture> --albedo <export.png> \
  --mesh-data <export.json> --report <private report.json> \
  --blend-out <private source.blend> --preview <private preview.png>
```

Exported bytes:

| File | SHA-256 |
|---|---|
| `assets/sprint39-nalfeshnee/nalfeshnee-albedo.png` | `1390e4b6e3c6e4781e6e2f67c1bdb3e275492488d33da9dfc26ed7c9c036391e` |
| `assets/sprint39-nalfeshnee/nalfeshnee-mesh.json` | `7231b45fc8577bab8df73bf1833d001e367251f809a1f86ebc939a4237e01af0` |

Exported facts: 7,320 polygons over 6,742 vertices, 27 bone groups across the
39-bone measured rig, a 512x512 RGBA albedo, two visible arms, two visible
legs, two visible wings, four tusks and six hand claws, with
`distinctSilhouette = corpulent-boar-headed-winged-demon`,
`articulatedWings = false` and `handClawsOnDigitChains = true`.

The blend scene, the report and the preview render are private build outputs
and are not packaged.
