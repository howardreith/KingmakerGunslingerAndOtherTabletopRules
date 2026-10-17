# Sprint 24 theropod original bodies

The Deinonychus and Tyrannosaurus meshes, UVs and paintings are deterministic
project-owned procedural work. No native vertex, index, UV, texture, material,
animation curve or proprietary assembly is read or redistributed. The shipped
schema-2 files contain only original geometry and painting plus the reviewed
native bone names and weights required to ride the native animation rig.

Both bodies use the geometry-free `Lizardfolk Free Hands` bind-frame census from
guarded run `20261014T0546181426610Z-f8d247dbd44541ea8c8411e12a243680`.
The selected body renderer is `_lizardman001` (39 bones, root `Torso_Lower`).
The separate `_ammunition04` renderer is not an anatomy donor; the runtime hides
that exact per-instance accessory while an original theropod body is attached
and restores its prior enabled state during rollback and teardown. The private
census and editable Blender files remain machine-local evidence. Native meshes,
paintings and animation clips are never packaged or modified.

Regenerate with Blender 4.5 LTS and Python 3, using `--python-exit-code 1`:

```text
python paint_theropod.py --kind <deinonychus|tyrannosaurus> --out <albedo.png>
blender -b --python-exit-code 1 --python generate_theropods.py -- --kind <kind> --capture <private-census.json> --albedo <albedo.png> --mesh-data <mesh.json> --report <private-report.json> --blend-out <private-source.blend>
blender -b --python-exit-code 1 --python review_theropods.py -- --kind <kind> --source <private-source.blend> --out <private-review-directory>
```

The offline review renders clay, side silhouette, textured, unlit and
party-camera views, walk and turn extremes, and creature-specific bite, talon,
foreclaw and pounce extremes. These posed frames review authored silhouette and
weighting; they do not substitute for native animation/contact qualification.
Deinonychus has two weight-bearing hind limbs, two forelimbs, a balanced tail,
an articulated jaw and two enlarged sickle foot claws. Tyrannosaurus has the
same biped plan but a much deeper skull, tiny forearms, heavier hind limbs and a
counterbalancing tail. Human aesthetic approval remains
`NOT_PERFORMED_NONBLOCKING`.

Exact census and exported-asset hashes are recorded in each private generation
report and in the Sprint 24 qualification ledger.

## Tyrannosaurus refinement, 2026-10-16 (VIS-001)

The owner's review found a cup-like head, a body assembled from bulbous pieces,
poorly integrated arms and feet, and (earlier) detached arms with comical arm
motion. Every part of the shipped mesh faced inward: the generator made them
consistently outward and the shared exporter then reversed them all (see
`../continuous-bodies/SOURCE.md`), so the skull drew its own inside with the
teeth on the inner wall. The forelimbs were authored at fixed chest
coordinates but bound to `Arm_Upper`/`Arm_Lower`/`Palm`, whose measured pivots
sit about 0.45 m away in the Lizardfolk's A-pose, so every native arm rotation
swung them through a wide arc.

The Tyrannosaurus is now built by `build_tyrannosaurus.py` as one continuous
skinned surface on the same census frame: massive hips and a deep barrel
leaning forward, a short thick neck, a deep long skull carried ahead of and
below the head pivot, a separate two-joint lower jaw with tooth rows, heavy
thighs over the Lizardfolk's digitigrade shins, three-toed clawed feet and a
horizontal countertail blended across the four tail joints. The tiny
two-fingered forearms are rooted in the chest and follow `Torso_Upper` for
66-84 % of their weight. `generate_theropods.py` keeps the Deinonychus and
refuses `--kind tyrannosaurus`.

- Capture: `sprint24-theropod-census.json` from guarded run
  `20261017T0224053339821Z-observe-expanded-summoning-sprint24-theropod-census`
  on master `2eed3944f6` (SHA-256
  `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`).
- `tyrannosaurus-mesh.json`: `d9fe3f0ac21d5372bcb309f9b8d4790e379da9c2ee052737aeb1644a239fe320`
  (7,093 vertices, 8,956 triangles, 50 closed outward parts, the exact 35
  reviewed drivers).
- `tyrannosaurus-albedo.png`: `ff2b250fd753fd18a94fb7f00d94acaaf7467410a76a0b182de8eed36bd7c7af`
  (1024 x 1024 RGB).
- Superseded: mesh `308a9a9950227b538ce9e7c068e090bf9e81a95fb6a573e8e8a0a4ea62a9ae87`,
  albedo `3a4f09eabf7095c6f79fc4a7f0d230c458bdc2b7838fec6faaac86884b5ef7e8`.
- A read-only visual review of the first rebuild found the arms frozen and
  the claws and jaw left behind in offline poses. Those were preview
  artifacts: the review armature ignored vertex groups of non-deform bones,
  which `continuous_body.heat_weights` now restores. The revision also gives
  the arms a larger share of their own swing (16-34 % from upper arm to
  finger), tucks the thighs into the hips and softens the neck-to-skull
  ramp.
- The released v0.0.155 validator (inherited by the v0.0.157 release and
  development validators) still pins the superseded Tyrannosaurus bytes; see
  the lane state file for the open decision.
