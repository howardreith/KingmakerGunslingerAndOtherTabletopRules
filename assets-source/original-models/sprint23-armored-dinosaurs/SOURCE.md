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

## Outward repair, 2026-10-17 (visual refinement)

The owner approved repairing the original bodies that the visual
refinement found shipping inside out from the legacy shared writer
("Let's assume that your repairs are good. Move forward with them.").
The shipped Stegosaurus body was repaired in place with `../continuous-bodies/repair_outward_winding.py`
(the generator's private donor capture is not needed): each triangle is
wound to agree with its own authored normals, and a closed part whose
volume is then negative has its triangles and normals turned. Positions,
UVs, weights and bones are unchanged.

| File | Before | After |
| --- | --- | --- |
| `assets/sprint23-armored-dinosaurs/stegosaurus-mesh.json` | `02a37218c41c…` | `6c5dcb055eafe011e76616c7a137a18d1c8ff1a63e603ddb4dbb8328b87c4117` |

To reproduce the shipped bytes, run the generator, then the repair on its
output. `tests/KingmakerGunslinger.DomainTests/RepairedBodyTests.cs` pins
the result and checks it is lit from the front.

## Cleanup 2 Stegosaurus revision (2026-10-21)

`build_stegosaurus.py` supersedes the old Stegosaurus generator. It replaces
the circular radius-.86 torso plus overlapping large ellipsoids with one
elongated elliptical barrel, supported by four connected weight-blended
legs. The narrow low neck, small eyed head, high hips, muscular six-joint
tail, nine staggered dorsal plates and four thagomizer spikes are distinct.
Opaque outward surfaces receive an original mottled olive skin with dorsal
bands and a lighter belly. This is a local anatomy revision, not a uniform
view scale change. The Triceratops source/runtime helpers and exported bytes
are unchanged.

The private `sprint22-ankylosaurus-contracts.json` supplies only the measured
Hodag bind skeleton/action metadata, SHA-256
`33ce3ad77a73d752d53aeae7de9effdc3b3b20a94fc8b2949d511d697f72a1c1`.
The actual native donor identity and existing six tail controls are retained.

```text
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/sprint23-armored-dinosaurs/build_stegosaurus.py -- --capture <private sprint22-ankylosaurus-contracts.json> --albedo <out>/stegosaurus-albedo.png --mesh-data <out>/stegosaurus-mesh.json --report <private report.json> --blend-out <private source.blend>
```

| Current file | SHA-256 |
| --- | --- |
| `stegosaurus-mesh.json` | `4ea887c98387d90d546e5512cf862cd47cb5694b0bbb1f99431c83c1a12c95ef` |
| `stegosaurus-albedo.png` | `27ef9768948c6f28a2836a28cfa45e9be03220b8f590d15af656212e540511bf` |

Two clean Blender 4.5.10 processes reproduce both files exactly. The scoped
`../sprint21-arthropods/verify_cleanup2_bodies.py` checks closed outward
components, normals, weights and one continuous body carrying all four limbs,
neck/head and six tail controls. `stegosaurus-stress-poses.json` probes stride
and tail extremes offline. Native side/rear views, walking/tail contact and
owner aesthetic acceptance remain separate and unverified by those probes.

Earlier sections are retained historical records.
