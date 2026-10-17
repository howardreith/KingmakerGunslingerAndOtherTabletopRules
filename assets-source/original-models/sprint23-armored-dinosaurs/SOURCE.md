# Sprint 23 armored-dinosaur original bodies

The Triceratops and Stegosaurus meshes, UVs, paintings, horns, frill, dorsal
plates and tail spikes are deterministic project-owned procedural work. No
native vertex, index, UV, texture, material, animation curve or proprietary
assembly is read or redistributed. The installable schema-2 files contain only
original geometry and painting plus reviewed native bone names and weights.

Triceratops uses the existing geometry-free Mastodon bind-frame capture from
Sprint 11. That rig was selected because its native primary gore and charge
motion drive the head forward. Stegosaurus uses the released Sprint 22 Hodag
survey, RunId
`20261012T1037196397495Z-observe-expanded-summoning-sprint22-contracts`, whose
six-joint tail chain and `Hodag_attack_tail` action were already measured. The
captures and editable Blender files remain private machine-local evidence.
The donor bodies are never packaged or modified.

Regenerate with Blender 4.5 LTS and Python 3, using `--python-exit-code 1`:

```text
python paint_armored_dinosaur.py --kind <triceratops|stegosaurus> --out <albedo.png>
blender -b --python-exit-code 1 --python generate_armored_dinosaurs.py -- --kind <kind> --capture <private-capture.json> --albedo <albedo.png> --mesh-data <mesh.json> --report <private-report.json> --blend-out <private-source.blend>
blender -b --python-exit-code 1 --python review_armored_dinosaurs.py -- --kind <kind> --source <private-source.blend> --out <private-review-directory>
```

The offline review renders clay, side silhouette, textured, unlit and
party-camera views plus walk/turn and creature-specific contact extremes.
Those posed frames review authored shape and weighting; they do not substitute
for native animation/contact qualification. Triceratops is authored with a
three-horned head and broad frill rather than a modified rhinoceros body.
Stegosaurus has a small head, raised hips, alternating plates and four terminal
tail spikes, with no Ankylosaurus club. Human aesthetic approval remains
NOT_PERFORMED_NONBLOCKING.

Exact capture and exported-asset hashes are recorded in each private generation
report and in the Sprint 23 qualification ledger.

## Triceratops refinement, 2026-10-17 (VIS-002)

The owner reported long segmented legs, a boxy body, an awkward olive colour,
poor proportions and open-looking feet and joints; the first export rendered
inside out (32 inward parts). `build_triceratops.py` (library in
`../continuous-bodies/`) now builds it on the same Mastodon frame as one
continuous body, as for the Rhinoceros: a heavy barrel whose back rises from
the shoulders to the high hips, hung low enough to bury the long upper legs,
short thick legs with nailed pads, a long thick tail weighted down the five
tail controls by its length, and the great head carried low and forward with
a parrot-like beak, a short nose horn, two long brow horns and a broad frill
with a scalloped rim swept back over the neck. Neck and Head take the neck and
skull through smooth ramps. The hide is an earthy grey-brown with dark dorsal
bands; the frill is rust with a dark rim and pale eye-spots.
`generate_armored_dinosaurs.py` now refuses `--kind triceratops` and keeps the
Stegosaurus and its loader.

- Capture: the Sprint 11 Mastodon capture (see `../ungulates/SOURCE.md`).
- `triceratops-mesh.json`: `f99f5d350a3a22e1a92f410687b2339c3a1107b8106c30a00744c834c7bf49fa`
  (7,618 vertices, 10,356 triangles, 30 closed outward parts, the reviewed 26
  Mastodon controls; three horns, a frill, no plates, five tail joints).
- `triceratops-albedo.png`: `eb458f118a7f3eebbc44d068ea6b306443cba54a3288e30269d535575037f9d0`.
- Superseded: mesh `ec2e38eae2cb3279a003ec2cf507f4eb30e4e3153905a60a697940833cd8c914`,
  albedo `15be7f5c733e770572513dc735cb85fa303ddaa8205de6edc722b558eb5e58e3`.
