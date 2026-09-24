# Building the Pteranodon original visual

This is the charter's Sprint 2 skinned-creature pipeline: the first replacement
body in the project that has to deform under animations it does not own.

## What is different about a creature

Every custom asset before this one - firearms, the Elven Branched Spear, the
eastern weapons - is a rigid prop. It is placed on a bone and it moves with that
bone. A creature body is skinned: its vertices are distributed across many bones
and blended, so it has to agree with the donor rig on three separate things at
once.

1. **Bone names.** The binding contract. Our mesh declares the names it expects;
   the loader resolves them against the live donor.
2. **Bone order.** The mesh's vertex weights are indices, so the order our bone
   name list is written in *is* the index space.
3. **Bind poses.** The frame the vertices were authored in.

The third is the one that is easy to get wrong, and it cost this sprint two
rejected iterations.

## Bind poses, and why the live pose is the wrong answer

Unity skins a vertex as

    v_world = sum_i w_i * bones[i].localToWorldMatrix * bindposes[i] * v

An obvious-looking approach is to compute bind poses at attach time from the
donor's own live bones:

    bindposes[i] = bones[i].worldToLocalMatrix * renderer.localToWorldMatrix

Both sides of that product do move together, so it always produces a
self-consistent binding. What it produces is a binding in which the mesh renders
exactly as authored *in whatever pose the donor was in at that instant*, and
deforms relative to that instant afterwards. Attach time is an arbitrary
animation frame.

Measured on the live donor, this matters a great deal. The resolved live pose of
a summoned Pteranodon has its wings folded: 1.638 units across against 3.152
long. The donor's actual bind pose is spread: 8.641 across against 2.937 long,
mirrored left to right to within 1e-5. They disagree by 3.954 units at
`L_Feather_1_end`. A membrane authored against the folded reading looks
plausible at rest and tears open the moment the wings spread - which is exactly
how the first two iterations failed visual review - and with attach-time bind
poses it would land somewhere different on every summon.

The authority is `SkinnedMeshRenderer.sharedMesh.bindposes`, which by definition
is

    bindposes[i] = bone[i].worldToLocalMatrix * renderer.localToWorldMatrix

captured when the donor mesh was bound, and which does not move with animation.
Inverting it gives each bone's position and rotation in the donor's mesh space,
and that is the frame the replacement must be authored in. So:

- the **generator** reads those inverted matrices and places geometry in that
  frame;
- the **loader** reads the same matrices at runtime and hands them straight to
  the custom renderer.

The two agree by construction.

## Restricted data

No native mesh, texture, controller, animation or rig is committed. Concretely:

- The rig capture stays in the runtime evidence directory it was produced in.
- The generated `.fbx` and `.blend` stay in
  `C:\Dev\KingmakerGunslingerLab\unity-asset-build\pteranodon-source`, because a
  skinned replacement can only exist in the donor's bind frame and therefore
  necessarily encodes the donor's skeleton geometry.
- The **shipped bundle carries no donor transforms at all.** The builder
  normalises the mesh's bind poses to identity and fails if any survive; the
  runtime rejects a bundle that ships non-identity bind poses. What ships is
  original geometry, its vertex weights, and a list of bone names.

Bone names themselves are treated the way blueprint GUIDs already are in this
repository: permitted structural facts, recorded in
`docs/EXPANDED-SUMMONING-PTERANODON-NATIVE-AUDIT.md`.

## Procedure

1. Capture the rig. `disposable-expanded-summoning` writes
   `pteranodon-attached-rig.json` into its evidence directory under
   `C:\Dev\KingmakerGunslingerLab\runtime-evidence`. The capture records each
   bone's live local transform *and* its bind pose, so the two can be compared
   rather than assumed.

2. Convert the bind poses into generator input:

   ```powershell
   python assets-source\original-models\pteranodon\convert_bind_rig.py `
       --capture <evidence>\pteranodon-attached-rig.json `
       --out C:\Dev\KingmakerGunslingerLab\unity-asset-build\pteranodon-source\rig.measured.json
   ```

   The converter refuses a capture that predates the bind-pose fix, and prints
   the worst left/right mirror error so an accidental live-pose capture is
   obvious immediately: the bind pose mirrors to 1e-5, an animated one does not.

3. Paint the albedo with Blender 4.5.10 LTS, whose Python carries numpy:

   ```powershell
   blender --background --factory-startup `
       --python assets-source\original-models\pteranodon\paint_pteranodon_albedo.py -- `
       --out assets\pteranodon\pteranodon-albedo.png
   ```

   Every mark is a closed-form or seeded-noise function of the atlas
   coordinates; two runs give the same bytes.

4. Generate the mesh and its runtime data with the same Blender and
   `PYTHONHASHSEED=0`:

   ```powershell
   blender --background --factory-startup `
       --python assets-source\original-models\pteranodon\generate_pteranodon.py -- `
       --rig  C:\Dev\KingmakerGunslingerLab\unity-asset-build\pteranodon-source\rig.measured.json `
       --out  C:\Dev\KingmakerGunslingerLab\unity-asset-build\pteranodon-source\pteranodon.fbx `
       --blend-out C:\Dev\KingmakerGunslingerLab\unity-asset-build\pteranodon-source\pteranodon.blend `
       --report assets-source\original-models\pteranodon\pteranodon-build-report.json `
       --albedo assets\pteranodon\pteranodon-albedo.png `
       --mesh-data assets\pteranodon\pteranodon-mesh.json
   ```

   The generator refuses to emit a mesh with more than four influences per
   vertex, which Unity cannot represent, and refuses `--mesh-data` without
   `--albedo`, because the runtime loads the two as one asset.

5. That is the whole build. There is no Unity editor step.

## Why this ships as mesh data and not an AssetBundle

Every previous custom asset in this repository - the firearms, the Elven
Branched Spear, the eastern weapons - ships as a Unity AssetBundle built by a
dedicated editor script in Unity 2018.4.10f1. The Pteranodon does not, and the
reasons are worth writing down because the pattern is otherwise consistent.

**It carries strictly less.** A bundle embeds bind poses, a material and import
settings. The mesh data carries our vertices, our normals, our triangles, our
vertex weights, and the donor's bone *names*. Names are the binding contract and
are already recorded in the native audit; no donor transform ships. For a
skinned replacement - which, unlike a rigid prop, can only exist in the donor's
bind frame - that distinction is the whole redistribution question.

**It has no editor dependency.** A bundle is tied to an exact Unity version and
to an activated editor licence. That is not a hypothetical cost: the
2018.4.10f1 install that built every previous bundle here stopped accepting its
licence between 2026-08-21 and 2026-09-23. The same install, the same project
and the same batch command succeeded on the earlier date and now reports
`BatchMode: Unity has not been activated with a valid License` and
`Failed to activate/update license. Missing or bad username and password`. Only
2018.4.10f1 and a Hub-installed 6000.5.6f1 are present, and a bundle built by
6000.x will not load in a 2018.4 game, so there was no second route.

**It is less code.** Around a hundred lines of exporter and a hundred of loader,
against an editor script plus an importer configuration plus a staging script.

`tools/unity/BuildPteranodonBundle.cs` and `scripts/Prepare-PteranodonAssets.ps1`
are retained: they are correct, and a bundle becomes the better answer again if
the asset ever needs compressed textures or several meshes. They are not on the
shipping path today.

## The mesh data format

`assets/pteranodon/pteranodon-mesh.json`, about 80 KB, beside
`pteranodon-albedo.png`, a 1024 x 1024 8-bit RGB PNG of about 630 KB:

| Field | Meaning |
|---|---|
| `schemaVersion` | 2; the runtime refuses anything else |
| `space` | `donor renderer local; +X left, +Y up, -Z forward` |
| `rigSha256` | the rig capture the geometry was authored against |
| `bones` | 46 names, in the order the vertex weights index |
| `uvAtlas` | the five named regions the texture coordinates map into; they tile the unit square exactly |
| `albedo` | the painting beside the file: bare file name, SHA-256, and the width, height, bit depth and colour type read from its PNG header |
| `vertexCount`, `triangleCount` | payload arithmetic, checked on load |
| `data` | base64: positions, normals, texture coordinates, triangle indices, then four (bone index, weight) pairs per vertex |

Triangle winding is reversed on export. Blender is right-handed with +Z up and
the donor renderer's space is left-handed with +Y up, so without the flip every
face would be inside out.

`PteranodonMeshDataTests` validates the shipped files on every build: schema,
bone count and membership, the atlas tiling the square, payload length, texture
coordinates inside the atlas, triangle indices in range, weights summing to one,
no vertex over four influences, every declared bone actually carrying geometry,
and the albedo on disk hashing to the value the mesh names with the header the
manifest declares. The runtime repeats those checks before it touches a donor
renderer, because a file can change between a build and a run.

## Runtime behaviour

`PteranodonAssetRuntime.Configure` builds the mesh once per process and
validates it completely before anything is published. A mesh built in code is
readable by construction, so the bundle path's separate readability check is not
needed.

The albedo is loaded in the same call. The bytes beside the mesh must hash to
the value the mesh names and the PNG header must carry the declared size before
`ImageConversion.LoadImage` is invoked; the result is a mip-mapped, trilinear,
clamped `Texture2D`. If either half fails, neither is published and the status
names the reason - `donor-visual:albedo-hash-mismatch`,
`donor-visual:invalid-mesh-data` and so on - because a mesh without its painting
is not the reviewed creature and is not shown.

`ExpandedSummoningPteranodonViewPatch` is a Harmony postfix on
`UnitEntityView.OnDataAttached`, keyed on the blueprint name
`KMG_Summoning_Unit_Pteranodon` and made idempotent by a
`ConditionalWeakTable`. For each attaching view it resolves the donor's single
`SkinnedMeshRenderer`, maps each declared bone name to that renderer's bone
array, takes the matching bind pose, builds a private mesh and a private copy
of the donor's material, puts the albedo in that copy's `_MainTex`, clears
whichever of a probed list of map slots the copy declares and carries (they are
indexed by the eagle's texture coordinates and mean nothing on this mesh),
resets the tint to white, and then swaps the mesh, the 46-bone array and the
material onto the donor's own `SkinnedMeshRenderer` component - on that one
instance. The outcome string records the shader, the slots declared and
cleared and the tint the donor carried, so the evidence shows exactly what was
done to the copy.

The swap rides the game's own component on purpose. Kingmaker drives a unit's
renderers by reference: `EntityFader` hides a fresh summon and fades it in,
`UnitFxVisibilityManager` and the occlusion highlighter cache the renderer
list, hit flashes and the death dissolve write to the renderer's materials. A
renderer added beside the donor's would sit outside all of that - visible
through fog, opaque during the fade, untouched by a hit - and the first live
isolation check showed exactly that: the fader had already disabled every fresh
summon's donor renderer, while a sibling would have stayed on. Root bone,
bounds, shadow modes, quality and the component's enabled state stay whatever
the game set; the shared prefab, its mesh, material and animator are never
touched. Eagle, dire bat and roc share this donor and are the negative
controls.

Every failure path leaves the donor visual intact, which is the approved
fallback: a Pteranodon that still looks like a giant eagle is a cosmetic
shortfall, while an invisible or half-bound one is a defect. A failure after
the swap puts the original mesh, bones and materials back on the same
component in the same frame and destroys what was made.

## Live qualification

Four guarded scenarios carry the creature's machine evidence, each run through
`scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1`, which snapshots the live
mod tree, builds and deploys the candidate, runs, and restores the snapshot:

| Scenario | What it proves |
|---|---|
| `observe-summon-pteranodon-view-contracts` | the loader published mesh and albedo (`visual:published`); the deformation proof on a detached, never-activated probe |
| `disposable-expanded-summoning` | every cast Pteranodon reports `visual:attached`; eagle, dire bat and roc on the shared donor are untouched; a 1d3 and a 1d4+1 Pteranodon cast attach on every unit; several casts in one lifecycle each attach exactly once and clean to the exact snapshot |
| `disposable-expanded-summoning-visual-contracts` | the Pteranodon mesh and material sit on the donor's own renderer, its 46 bones all on the view's skeleton with the root bone kept, the shader carrying the albedo at the catalog view scale, and the same state holds after the native locomotion, attack, hit and death paths |
| `disposable-expanded-summoning-pteranodon-fault-drill` | development-only: the first Pteranodon cast with the visual withdrawn comes up on the donor's own mesh and material; the second, faulted after the swap, is restored to them in the same frame; later casts attach |

Run identities are recorded in `EXPANDED-SUMMONING-PHASE0-AUTONOMOUS-STATE.md`.

## Scope

This does not change identity, Summon Monster IV / Summon Nature's Ally IV
placement, higher-tier quantities, stats, reach, attack cadence, alignment
templates, AI, casting, duration, footprint or persistence. It touches renderers
only. `SummonViewScaleCatalog`'s 0.82 view multiplier still applies, so the
replacement is authored to the donor's scale envelope.
