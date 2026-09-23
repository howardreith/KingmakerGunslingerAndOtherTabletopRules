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

3. Generate the mesh with Blender 4.5.10 LTS:

   ```powershell
   blender --background --factory-startup `
       --python assets-source\original-models\pteranodon\generate_membrane.py -- `
       --rig  C:\Dev\KingmakerGunslingerLab\unity-asset-build\pteranodon-source\rig.measured.json `
       --out  C:\Dev\KingmakerGunslingerLab\unity-asset-build\pteranodon-source\pteranodon-membrane.fbx `
       --blend-out C:\Dev\KingmakerGunslingerLab\unity-asset-build\pteranodon-source\pteranodon-membrane.blend `
       --report assets-source\original-models\pteranodon\pteranodon-membrane-build-report.json
   ```

   The generator refuses to emit a mesh with more than four influences per
   vertex, which Unity cannot represent.

4. Stage into the exact Unity project:

   ```powershell
   .\scripts\Prepare-PteranodonAssets.ps1
   ```

   Staging refuses an FBX whose build report does not record a rig SHA-256 and
   the donor renderer space, so a mesh of unknown provenance cannot be built
   into a bundle.

5. Build the bundle with the installed Unity 2018.4.10f1 editor:

   ```powershell
   & "C:\Program Files\Unity\Editor\Unity.exe" -batchmode -nographics -quit `
       -projectPath "C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1" `
       -executeMethod BuildPteranodonBundle.BuildBatch
   ```

   The builder rejects every Unity version except 2018.4.10f1. It validates that
   the source has exactly one `SkinnedMeshRenderer`, that every bone it binds to
   is in the declared set, that no bone name repeats, that every vertex weight
   sums to one and indexes a bone that exists, and that no donor bind pose
   survives normalisation.

6. Stage the emitted `Builds/Windows/kingmakergunslinger.pteranodon` into
   `assets/bundles/` and record its SHA-256 in
   `assets/bundles/asset-bundle-manifest.json`.

## Runtime behaviour

`PteranodonAssetRuntime.Configure` loads the bundle once per process and
validates it completely before anything is published. The validation is the same
list the builder enforces, re-checked on the shipped artefact, plus a readability
check - the loader has to replace bind poses on a copy of the mesh, which
requires the mesh to be readable.

`ExpandedSummoningPteranodonViewPatch` is a Harmony postfix on
`UnitEntityView.OnDataAttached`, keyed on the blueprint name
`KMG_Summoning_Unit_Pteranodon` and made idempotent by a
`ConditionalWeakTable`. For each attaching view it resolves the donor's single
`SkinnedMeshRenderer`, maps each declared bone name to that renderer's bone
array, takes the matching bind pose, builds a private mesh and material, adds a
sibling `SkinnedMeshRenderer`, and only then disables the donor's renderer
component.

Only the component's `enabled` flag is changed, and only on that one instance.
The donor GameObject stays active so anything parented under it - effects
anchors, colliders, the hit-FX locator - keeps working, and the shared prefab,
mesh, material and animator are never touched. Eagle, dire bat and roc share
this donor and are the negative controls.

Every failure path leaves the donor visual intact, which is the approved
fallback: a Pteranodon that still looks like a giant eagle is a cosmetic
shortfall, while an invisible or half-bound one is a defect. A failure after the
donor renderer is disabled re-enables it and destroys what was added.

## Scope

This does not change identity, Summon Monster IV / Summon Nature's Ally IV
placement, higher-tier quantities, stats, reach, attack cadence, alignment
templates, AI, casting, duration, footprint or persistence. It touches renderers
only. `SummonViewScaleCatalog`'s 0.82 view multiplier still applies, so the
replacement is authored to the donor's scale envelope.
