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
