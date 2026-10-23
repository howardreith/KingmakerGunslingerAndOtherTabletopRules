# Sprint 25 giant-body original textures and bodies

## Cleanup iteration 2 (2026-10-21)

`refine_giant_bodies.py` is the current authoring entry point. The older
prototype/export notes below describe the retained Sprint 25 baseline. On the
actual baseline exports, all 13 Elephant components and all 9 Brachiosaurus
components were closed but inward; their normals overwhelmingly opposed their
triangle winding. This explains an offline reproduction of dark/missing exterior
surfaces with normal backface culling. Native effect/material observations are
still required and are not inferred from this diagnosis.

The two `cleanup2-*-input.json` files retain the exact **original project mesh**
exports used as authoring inputs. They contain no native mesh/bind/animation data.
This makes regeneration independent of the unavailable later native census.
Closest-triangle barycentric weight transfer preserves the published donor bone
set, followed by smoothing and a spine-owned barrel rule. One voxel union forms
the torso, neck/trunk, limb roots and tail; closed features remain separate where
anatomically appropriate. Export refuses open, inconsistent or inward surfaces.

Elephant has broad columnar limbs, rounded foot pads/toenails, thin closed ear
fans, original tusks and a thin hanging tail. Brachiosaurus has columnar supports,
a connected long neck/small head and a balancing tapered tail. Both receive new
spatially painted 1024px skin albedos and UVs. The existing Brachiosaurus
`Head`-driven stiff-neck adaptation is retained; native movement is unverified.
The native Mastodon, other donor consumers and gameplay rules are not edited.

Regenerate with Blender 4.5.10 LTS, `PYTHONHASHSEED=0`, from the repository root:

```text
blender -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint25-giant-bodies/refine_giant_bodies.py -- --kind elephant --output-dir <absolute assets/sprint25-giant-bodies> --work-dir <absolute private output>
blender -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint25-giant-bodies/refine_giant_bodies.py -- --kind brachiosaurus --output-dir <absolute assets/sprint25-giant-bodies> --work-dir <absolute private output>
```

Two clean final runs produced identical mesh/albedo bytes. See
`planning/CLEANUP2-LARGE-CREATURES-NOTES.md` for exact identities and focused
offline evidence. Source `.blend` files, captures, previews and raw reports stay
local. Native visual/behavior evidence and owner acceptance remain unverified.

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

## Completion continuation: Brachiosaurus neck/chest finish (2026-10-22)

`refine_giant_bodies.py` now applies a Brachiosaurus-only local sculpture
after the stable union and UV operation. Original project neck ring scalars
showed a long near-vertical segment followed by a sharp bend. The centreline
now follows a continuous rising arc and the existing surface is relaxed
locally. A monotone anterior compression blends the hanging lower neck into
a sloping shoulder/chest. Anterior withers rise modestly and ankle shafts
taper without moving soles. The too-strong folded intermediate was rejected;
final full front/side/party/rear pixels were opened and inspected.

This preserves the exact preceding UV, triangle and complete skin-weight bytes,
all 605 tail position/normal rows and `attackGestureTailRig` metadata. The
40 Tail0-pinned junction vertices and 70 crossing triangles remain exact.
The original `repair_brachiosaurus_tail_weights.py` is unchanged. No private
gesture, source skeleton, native action, root/collider, mechanic or opacity
code is modified. The Head-driven stiff-neck adaptation remains explicit.
Elephant's generator branch and shipped assets are untouched.

Original scale-edge/fold diffuse paint replaces the flatter coat. No native
texture is sampled and no shader response is assumed. Current pair:

| File | SHA-256 |
| --- | --- |
| `brachiosaurus-mesh.json` | `baa97de9024fcbc9b07d8a1dc857813235aff8dcb276c4c2aa9be617a729fef7` |
| `brachiosaurus-albedo.png` | `160e4b71ae1335d6df37a0d8eac491e863b82dc272cfe24e42173a6d40107ea4` |

Regenerate using the existing `--kind brachiosaurus` command. The local tail
repair module path is now explicitly added by the entry point. Two clean
Blender 4.5.10 runs reproduce the complete pair exactly with LF JSON. Focused
checks pass: stable 29-driver palette, 8,302 vertices/12,836 triangles, closed
outward surfaces, normalized weights and exact protected tail/UV/skin data.

Local `artifacts/cleanup2-completion-art/brachiosaurus/candidate` contains full
rest views and `gesture-synthetic` ready/act/recovery views. The latter use
only the project's own rest-palette pivots and original qualified gesture
angles; all non-tail/junction displacement is zero and the act edge-length
ratio is 0.752679–1.327635. No artificial pre-act windup is claimed. They prove
an authored deformation envelope, not current native TRS, ordinary approach,
enemy contact, timing or owner acceptance. Historical review18 native results
retain their old asset identity; the changed neck/body needs native inspection.

## Native28 follow-up: Brachiosaurus supports and chest junction

The actual native28 `creature-command-brachiosaurus-ready/contact/post-act-028`
frames in local run `20261023T0118396309129Z` showed uniform limb tubes,
flared foot ends and rounded shoulder/hip joins. Tree-driven blue occlusion
and changing camera views do not establish gait or tail contact. The existing
original-mesh side/front/gameplay previews independently confirmed the shapes.

`finish_brachiosaurus_supports` now narrows the lower shafts and foot rims,
blends the upper supports into the flank, and buries the pendant lower-neck
lip into the sloping chest. A skin-weight-dependent intermediate was rejected
for visible wrinkles; a stronger short relaxation was rejected for four new
opposed-normal corners. The final geometric envelope has broad feathered
boundaries and bounded surface relaxation. Opened final front/side/gameplay
clay and textured frames show the lower-neck stump and raised hindleg cap
removed. This is admission for native qualification, not owner art approval.

The original paint is baked before this local sculpture, preserving its exact
bytes and UV mapping. The exact current export is:

| File | SHA-256 |
| --- | --- |
| `brachiosaurus-mesh.json` | `73f3689c5abdda09fb68f9e08182cf671c6a753966bedaf21ebb16877c04c57c` |
| `brachiosaurus-albedo.png` (unchanged) | `160e4b71ae1335d6df37a0d8eac491e863b82dc272cfe24e42173a6d40107ea4` |

Two clean Blender 4.5.10 generations reproduce both files exactly. The focused
decoded-byte audit verifies 8,302 vertices/12,836 triangles; identical complete
UV/triangle/skin-weight bytes; identical metadata/scalars, 29-driver palette,
rig SHA and `attackGestureTailRig`; all 605 tail position/normal rows exact;
and 1,880 upper-neck/head position/normal rows at original authored Y>=4.10
exact. Lower neck/chest rows are deliberately allowed to change. The fixed
regions include a neighbouring normal-support ring. All source Y values below
1.30 and the minimum sole Y `-0.025092504918575287` remain exact. The four
existing pad driver sets retain their Y contacts with width about 0.833->0.674
and depth about 0.94->0.73 in the same authored frame. No source bone, bind,
private gesture, root, collider, mechanic, UV or material code changes.

All three surfaces remain closed, consistently wound and positive-volume;
weights/normals are finite and normalized. The baseline's two opposed-normal
corners in the protected tail remain exact; there are no new ones. Local
`artifacts/cleanup2-brachio-supports/verification.json` is the small current
receipt. `before/clay` and `candidate/clay` contain the three corresponding
front/side/gameplay views; `candidate/renders` contains the same three textured
views. These are original rest-frame art previews, not native posed evidence.

Reproduction from this worktree (all cache output stays local):

```powershell
$env:PYTHONDONTWRITEBYTECODE = '1'
& 'C:/Dev/.toolchains/blender-4.5.10-windows-x64/blender.exe' -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint25-giant-bodies/refine_giant_bodies.py -- --kind brachiosaurus --output-dir assets/sprint25-giant-bodies --work-dir artifacts/cleanup2-brachio-supports/regenerated
& 'C:/Dev/.toolchains/blender-4.5.10-windows-x64/4.5/python/bin/python.exe' artifacts/cleanup2-brachio-supports/verify.py
```

Final localized generation: 7.131 s; exact repeat: 6.879 s; final three textured
views: 1.857 s; focused decoded-byte audit: 0.340 s. A relative-path preview
invocation failed to load its albedo; absolute-path rerender succeeded and the
failure log is retained locally. No native run, production build, shared pin,
manifest or historical acceptance record was changed by this art task. Native
gait, ordinary approach/tail appearance, current lighting and owner approval
remain pending on the new mesh bytes.
