# Sprint 39 original Nalfeshnee body

The Nalfeshnee uses distinct original procedural geometry and an original
albedo authored for Sprint 39 by `generate_sprint39_nalfeshnee.py`. The script
reuses the project's established Blender 4.5 exporter and measured free-hands
rig capture; it does not distribute native geometry, textures, materials or
animation clips. The native rig remains a runtime dependency.

The Sprint 39 installed-library observation
(`20261104T0415356825476Z-observe-expanded-summoning-sprint39-native-inputs`)
found **no installed Kingmaker Nalfeshnee** — zero units and zero name matches
of any type — so none is reused. The mechanical donor, the animation donor and
the rendered mesh are three different things and are recorded separately here:

- animation carrier: native donor `a8cbd4cdb0feaf64eab1452257c3c971`, prefab
  `d6db6ba3b2c459a48ba0730beef0bac8` — the same measured free-hands rig the
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
   shoulder mass — present in the mesh, invisible in a render. They were
   lifted and pushed back until the membrane clears the torso silhouette.
2. The membrane was first filled with a grid of `mass()` ellipsoids, copying
   the icon renderer's approach. That renderer uses **metaballs**, which merge;
   this exporter has no merge, so the vane rendered as a string of beads —
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
