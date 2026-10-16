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

## What the geometry step still needs

`generate_armored_dinosaurs.py` — the quadruped precedent this sprint follows
— binds original geometry to a **geometry-free Mastodon bind-frame capture**
and refuses anything else: it requires a capture whose source names the
Mastodon, a single skinned renderer, and `boneCount == bindPoseCount`.

That capture was Sprint 11 private machine-local evidence and is **not on this
machine**. A repository-wide search for the capture shape (`bindPosition`
rows) finds only Pteranodon attached-rig captures, and no `.blend` or report
from the Sprint 23 generation survives either.

So the Brachiosaurus and Elephant meshes cannot be generated yet, and nothing
here fabricates a rig. The next step is the smallest approved donor
observation: one guarded, unelevated run against a valid artifact that
captures the Mastodon renderer bind frame geometry-free, exactly as Sprint 11
did. Blender 4.5.10 LTS is installed and verified working on this machine
(background render, real exit code 0), so authoring and the offline review
renders can proceed the moment the capture exists.

Planned commands once the capture is in hand, with `--python-exit-code 1`:

```text
blender -b --python-exit-code 1 --python generate_giant_bodies.py -- --kind <kind> --capture <private-capture.json> --albedo <albedo.png> --mesh-data <mesh.json> --report <private-report.json> --blend-out <private-source.blend>
blender -b --python-exit-code 1 --python review_giant_bodies.py -- --kind <kind> --source <private-source.blend> --out <private-review-directory>
```

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
