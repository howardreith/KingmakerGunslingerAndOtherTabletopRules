# Continuous skinned bodies (visual refinement)

`continuous_body.py` is the shared authoring library for the Expanded Summoning
visual-refinement lane. The pilot uses it for three creatures only:

| VIS | Creature | Builder | Donor frame |
| --- | --- | --- | --- |
| VIS-005 | Dire Rat | `sprint12-quadrupeds/build_dire_rat.py` | Dog (`sprint12-dog-bind-rig.json`) |
| VIS-024 | Rhinoceros | `ungulates/build_rhinoceros.py` | Mastodon (`sprint11-mastodon-bind-rig.json`) |
| VIS-001 | Tyrannosaurus | `sprint24-theropods/build_tyrannosaurus.py` | Lizardfolk Free Hands (`sprint24-theropod-census.json`) |

No legacy generator imports this library, so it changes no other creature's
output. The legacy generators refuse the three pilot kinds and name the new
builder.

## Why the first bodies looked broken

Two systematic causes, measured on the exact shipped bytes, explain most of the
owner's "holes", "transparent", "very dark" and "cup-like" reports. They are
recorded here because every remaining original creature still ships with them.

1. **Every triangle was reversed on export.** `write_mesh_data` in
   `pteranodon/generate_pteranodon.py` emits `(loop0, corner+1, corner)` on the
   theory that the donor renderer's space mirrors Blender's. It does not: the
   runtime (`PteranodonAssetRuntime.BuildMesh`) assigns positions, normals and
   indices verbatim, and each donor frame, read from its own `L_`/`R_` bones, is
   a proper rotation of Unity's (right, up, forward). Unity and Blender
   therefore agree that the front of triangle (a, b, c) is
   `Cross(b - a, c - a)`. Reversal turned every Blender-outward surface inward,
   so back-face culling drew the far inner walls; the normals were not
   reversed, so every face that did draw was lit from behind.
2. **Primitive winding disagreed with itself.** `add_tube` and the ungulate
   `elliptical_tube` wind both caps opposite to their walls, the Sprint 12
   caller-axis ellipsoid flips with the handedness of its axes, and the ungulate
   fixed-axis ellipsoid is inward. After the blanket reversal those parts came
   out outward while the walls came out inward, which is why a reversal alone
   never fixes these meshes.

| Shipped mesh (master `2eed3944f6`) | Components | Inward | With reversed caps | Normals opposing winding |
| --- | --- | --- | --- | --- |
| Dire Rat | 23 | 12 | 14 | 4,857 of 4,896 corners |
| Rhinoceros | 22 | 14 | 10 | 7,370 of 7,380 |
| Tyrannosaurus | 37 | 37 | 0 (1 open) | 8,026 of 8,106 |

An offline render of those exact bytes with back-face culling reproduces the
owner's screenshots (Rhinoceros background seen through its rear, Tyrannosaurus
teeth on the inside of a cupped skull), and the guarded native baseline on the
unchanged master build shows the Rhinoceros selection outline enclosing floor
seen through its head and a leg. `audit_mesh_orientation.py` and
`render_mesh_data.py` are those two diagnostics.

## How a refined body is built

- **One closed surface.** Closed ellipsoids and capsule chains at their exact
  sizes, placed relative to the measured donor joints, are merged by a voxel
  remesh (their exact union), relaxed with volume-preserving Laplacian passes
  so the creases where they meet become fillets, then reduced. Limbs, neck and
  tail grow out of the trunk instead of meeting it at a cap. A first version
  blended metaballs; overlapping fields add, so dense sweeps swelled far past
  their radii (a Rhinoceros head swallowed its horns), and it was abandoned.
  Each relaxation pass reaches about one voxel, so where large masses meet
  over tenths of a metre (a bison's hump) the torso is united and relaxed on
  a coarser union of its own and joined to the limbs and head as a closed
  shell (`Volume.shell`, `relax`). Decimating a dense, lumpy union can leave
  two faces on the same three vertices; `fan_triangles` drops such zero-volume
  fins and the outward check still refuses anything else that is not closed.
- **Blended weights.** Blender's bone-heat solve over that surface, restricted
  to the reviewed donor controls, then Laplacian weight smoothing over the
  surface's edges. Species-local rules, each commented in its builder, cover
  what heat cannot know: a skull moves as one with its rigid jaw, eyes, horns
  and teeth; the Rhinoceros barrel follows the spine rather than the Mastodon
  upper-limb bones it buries, and its neck and skull fade onto `Neck` and
  `Head` along and across the head axis with no hard edge; the Tyrannosaurus
  countertail follows its four tail joints and its neck column shares the
  `neck` control.
- **Rigid features.** Horns, teeth, ears, eyes, claws, toes, whiskers and the
  rat's naked tail are closed solids rooted inside the surface and owned by
  fixed bones. The Tyrannosaurus forearms ride `Torso_Upper` for 66-84 % of
  their weight, so the Lizardfolk's broad A-pose arm swings reach them only as a
  restrained fraction.
- **Bent limbs.** `strand` builds each ring from that point's own tangent, so
  a sharply bent limb (the Deinonychus's folded arm) twists between rings and
  folds onto itself. `tube` carries one ring frame along the path by the
  rotation between successive tangents and is used for bent arms and fingers.
- **Offline preview.** Blender's armature modifier ignores a vertex group
  whose bone is not a deform bone, and the heat solve clears that flag on
  every bone it was not asked to weight. `heat_weights` restores it on every
  bone, so the saved source poses with exactly the weights that ship. Before
  that fix a first visual review saw rigid jaws, claws and arms left behind
  in offline poses that the game would have moved.
- **Painting.** UVs come from Smart UV Project; the albedo is rasterised from a
  deterministic procedural colour field over the surface (value and cell noise,
  countershading, skin regions taken from the weights) and written by a fixed
  PNG encoder. The colour values are stored as they are. The game lights them
  warmly and more brightly than the offline studio renders: the first native
  run showed the Rhinoceros's 0.41 grey hide as tan in torchlight, while the
  olive-brown Tyrannosaurus read as intended. Neutral greys are therefore kept
  darker and slightly cool; saturated browns and olives read as authored.
- **Export.** Blender's winding is kept, vertices are split only where a texture
  seam needs it (position, normal and weights are shared, so seams are
  invisible), and the exporter refuses any component that is open, wound
  inconsistently or not outward. The domain tests repeat that check on the
  shipped bytes (`OriginalMeshOrientation`).
- **Targeted repairs.** Where the owner asked for a repair rather than a
  rebuild (the crocodilians, the ants, the scorpion), the creature's own
  generator keeps its geometry and the legacy shared writer, makes each part
  consistently outward in Blender, and then calls `restore_outward_winding`,
  which undoes the writer's reversal in the payload under the same refusal.

## Determinism hazards found in Blender 4.5.10

Two clean runs of each builder are byte-identical. Getting there exposed four
hazards that any later builder must also avoid:

- `bpy.ops.object.join` appends parts in Blender's selection order, which varies
  between runs; a weight table written in part order then lands on the wrong
  vertices. `assemble` merges explicitly and checks every vertex.
- `bmesh.ops.recalc_face_normals` picks the wrong side for small closed shapes
  far from the origin and does not choose the same loop order from run to run.
  `orient_outward` decides by exact signed volume and refuses inconsistent
  winding instead.
- `bmesh.ops.create_uvsphere` and `bmesh.ops.triangulate` do not produce a
  stable element order; ellipsoids are built explicitly and triangulated as a
  fixed fan.
- New BMesh elements carry no valid index until `index_update()`.

## Regenerate

Blender 4.5.10 LTS, `PYTHONHASHSEED=0`, from the repository root:

```text
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/sprint12-quadrupeds/build_dire_rat.py -- --capture <private sprint12-dog-bind-rig.json> --albedo assets/sprint12-quadrupeds/dire-rat-albedo.png --mesh-data assets/sprint12-quadrupeds/dire-rat-mesh.json --report <private report.json> --blend-out <private source.blend>
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/ungulates/build_rhinoceros.py -- --capture <private sprint11-mastodon-bind-rig.json> --albedo assets/ungulates/rhinoceros-albedo.png --mesh-data assets/ungulates/rhinoceros-mesh.json --report <private report.json> --blend-out <private source.blend>
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/ungulates/build_rhinoceros.py -- --kind woolly-rhinoceros --capture <private sprint11-mastodon-bind-rig.json> --albedo assets/ungulates/woolly-rhinoceros-albedo.png --mesh-data assets/ungulates/woolly-rhinoceros-mesh.json --report <private report.json> --blend-out <private source.blend>
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/ungulates/build_cattle.py -- --kind aurochs --capture <private sprint11-horse-bind-rig.json> --albedo assets/ungulates/aurochs-albedo.png --mesh-data assets/ungulates/aurochs-mesh.json --report <private report.json> --blend-out <private source.blend>
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/ungulates/build_cattle.py -- --kind bison --capture <private sprint11-horse-bind-rig.json> --albedo assets/ungulates/bison-albedo.png --mesh-data assets/ungulates/bison-mesh.json --report <private report.json> --blend-out <private source.blend>
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/sprint24-theropods/build_tyrannosaurus.py -- --capture <private sprint24-theropod-census.json> --albedo assets/sprint24-theropods/tyrannosaurus-albedo.png --mesh-data assets/sprint24-theropods/tyrannosaurus-mesh.json --report <private report.json> --blend-out <private source.blend>
```

Review a private source with
`review_continuous_body.py -- --source <source.blend> --out <dir> --name <creature> --up z|y --forward=<frame forward> [--poses <json>] [--clay]`.
Render shipped bytes exactly as the runtime builds them with
`render_mesh_data.py -- --mesh <mesh.json> --out-dir <dir> --name <n> --up z|y --forward=<f>`.

The captures, Blender sources, reports and review frames are machine-local and
are never committed or packaged. Captures were taken by guarded runs on the
unchanged master build (see the lane state file); the shipped files contain
only original geometry, UVs, weights, reviewed bone names and the albedo hash.
