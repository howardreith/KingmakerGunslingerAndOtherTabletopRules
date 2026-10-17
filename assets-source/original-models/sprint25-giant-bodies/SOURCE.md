# Sprint 25 giant-body original textures and bodies

The Brachiosaurus and Elephant UVs and paintings are deterministic
project-owned procedural work, the same category as the released Sprint 23
armored-dinosaur and Sprint 24 theropod authoring. No native vertex, index,
UV, texture, material, animation curve or proprietary assembly is read or
redistributed, and no pixel is copied from another creature in this roster.

## What is authored today

`paint_giant_body.py` uses the Python standard library only — no image
generation and no Blender — and writes a 512x512 RGBA PNG from the shared
five-region UV atlas it reads out of the released pteranodon seam.

```text
python paint_giant_body.py --kind <brachiosaurus|elephant> --out <albedo.png>
```

| Output | SHA-256 | Size |
| --- | --- | --- |
| `assets/sprint25-giant-bodies/brachiosaurus-albedo.png` | `99b077a7f3453c34…` | 512x512 RGBA |
| `assets/sprint25-giant-bodies/elephant-albedo.png` | `b002da80a6ec4a56…` | 512x512 RGBA |

Both regenerate byte-identically; the authoring toolchain is CPython 3.14.6 on
HOWARD-LAPTOP and the script depends on nothing outside the standard library.

The two creatures deliberately do not share a grain algorithm, because they do
not share a skin:

- **Brachiosaurus** — a coarse 17px lattice of non-overlapping tubercles with
  a darker centre per cell, so sauropod hide reads as pebbled rather than
  noisy at party-camera distance, plus belly-ward counter-shading that keeps
  volume under flat lighting, and plain vertical banding on the columnar legs
  because weight-bearing pillars are not flank hide.
- **Elephant** — a two-frequency crease network, because elephant hide is a
  system of deep folds rather than cells, with dust settled in the folds,
  stronger transverse rings on the trunk, and tusks left smooth and bright so
  ivory reads as ivory.

### Atlas slot roles

The five shared slot names are generic and are **not** anatomy claims. `beak`
carries the Brachiosaurus's small head and the Elephant's tusks; neither
creature has a beak.

| Slot | Brachiosaurus | Elephant |
| --- | --- | --- |
| `membrane` | neck and dorsal expanse | hide expanse and ears |
| `body` | torso flank | torso flank |
| `crest` | tail | trunk |
| `beak` | small head | tusks |
| `limbs` | columnar legs | legs |

## The geometry step — no longer blocked on a capture

This section previously recorded the geometry step as blocked: the Mastodon
bind-frame capture `generate_armored_dinosaurs.py` requires was Sprint 11
machine-local evidence and was not on this machine.

**That is resolved.** The Sprint 25 guarded donor census captured the Mastodon
renderer bind frame geometry-free — 40 joints, `boneCount == bindPoseCount`,
prefab `fe9f25fe62e0f3b49a13910a996470b2` — and original geometry has since
been bound to it and posed through the real skin cluster. The census schema is
the Sprint 18 census shape (`donors[].skinnedRenderers[]`) rather than the
Sprint 12 shape (`renderers[]`) the older generator reads, which is why
`prototype_giant_bodies.py` carries its own reader with the same refusals:
one skinned renderer, counts agreeing, unique names, three-component
positions.

Two scripts stand today, both run under `--python-exit-code 1`:

```text
blender -b --python-exit-code 1 --python graybox_giant_bodies.py -- --census <sprint25-donor-census.json> --out <dir> [--diagnostic]
blender -b --python-exit-code 1 --python prototype_giant_bodies.py -- --census <sprint25-donor-census.json> --out <dir> [--albedo-dir <dir>]
```

The first renders authored shape. The second binds that shape to the donor
armature, moves it by posing bones only, measures what the skin does, and
exports FBX. `WEIGHTED-PROTOTYPE.md` records what it proved and what it
refused to prove, and the findings it hands to the final geometry pass:

- the Brachiosaurus neck must be weighted to `Head` alone — full donor-chain
  influence measures 5.08 visible stretch and 10.5 units of displacement, and
  damping to a quarter collapses the surface to 0.15 of its rest length;
- the shoulder and neck base need actual mass and a unioned surface — the
  wedge there survived all five seam weightings, so it is geometry;
- a seam that bends needs more rings than it has — the trunk base exceeds
  tolerance past 8 degrees per joint because its rings are as thick as its
  segments are long;
- the mid-limb donor joints `frontKnee_*` and `frontAnkle_*` carry no
  influence, and bending them stretches the visible shin seam to 1.05.

Still required and not done: final geometry, UVs on final geometry, the
textures inspected on the actual models, and native animation qualification.
The census captured clip names and event times, never curves, so every
amplitude measured so far is an authored probe.

Brachiosaurus is authored with a long elevated neck, a small head, tall
forequarters, a heavy quadruped torso, columnar legs and a balancing tapering
tail; a long neck is not permission to inflate its collision footprint beyond
the game size contract. Elephant is authored as a hairless elephant with an
articulated trunk and tusks and distinct gore and slam contacts — not a
recoloured or shrunken woolly Mastodon, and the actual Mastodon units and
assets are never modified.

The offline review must render clay, side silhouette, textured, unlit and
party-camera views plus walk/turn and contact extremes, and the textures must
be inspected on the actual models rather than judged as flat images. Those
posed frames review authored shape and weighting; they do not substitute for
native animation and contact qualification. Human aesthetic approval remains
`NOT_PERFORMED_NONBLOCKING`.

`originalBodyAssetsAuthored` stays **false** until both geometries, UVs,
albedos, provenance and the required inspection outputs all exist and verify.
Textures alone do not flip it.
