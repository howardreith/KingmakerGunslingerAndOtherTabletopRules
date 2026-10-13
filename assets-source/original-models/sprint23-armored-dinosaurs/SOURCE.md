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
