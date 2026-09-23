# Pteranodon original-visual source

The geometry generated here is an original, project-owned clean-room work. No
Owlcat, Paizo, marketplace, downloaded, traced, or generative-model asset is an
input. The repository license applies.

## What is committed here, and what deliberately is not

Committed: the generator, the rig converter, and the build report. The build
report carries counts, extents, and the bone *names* the mesh binds to - the
same class of fact the native audit already records - but no per-bone
coordinates.

Not committed: the captured donor rig, and the `.fbx` and `.blend` the
generator produces from it. A skinned replacement mesh can only exist in the
donor's bind frame, so those files necessarily encode the donor's skeleton
geometry. They stay in the approved local evidence and asset-build locations
and are rebuilt on the machine that has them, in the same way `GamePath.props`
and the private runtime-reference bundle are machine-local build inputs. The
shipped bundle is scrubbed of donor transforms as well - see below.

## Rebuilding

1. Capture the donor rig. It comes out of a guarded runtime scenario:
   `disposable-expanded-summoning` writes `pteranodon-attached-rig.json` into
   its evidence directory under
   `C:\Dev\KingmakerGunslingerLab\runtime-evidence`. The capture records each
   bone's live local transform *and* its bind pose.

2. Convert the bind poses into generator input:

   ```
   python convert_bind_rig.py \
       --capture <evidence>/pteranodon-attached-rig.json \
       --out rig.measured.json
   ```

   The converter reads `bindPosition`/`bindRotation`, which are
   `SkinnedMeshRenderer.sharedMesh.bindposes[i]` inverted and therefore already
   expressed in the donor's mesh space. It refuses a capture that predates the
   bind-pose fix.

   **Use the bind pose, never the live pose.** The two disagree by up to 3.954
   units at the wingtip: the live pose of a summoned unit has the wings folded
   (1.638 across against 3.152 long) because it is whatever frame the animation
   system is on, while the bind pose is spread (8.641 across against 2.937 long)
   and mirrors left to right to within 1e-5. Two earlier iterations of this mesh
   were authored against a resolved live pose and failed visual review; a
   membrane built that way looks plausible at rest and tears open the moment the
   wings spread.

3. Generate with Blender 4.5.10 LTS and `PYTHONHASHSEED=0`:

   ```
   blender --background --factory-startup --python generate_membrane.py -- \
       --rig rig.measured.json --out pteranodon-membrane.fbx \
       --blend-out pteranodon-membrane.blend \
       --report pteranodon-membrane-build-report.json
   ```

4. Stage and build the bundle: `.\scripts\Prepare-PteranodonAssets.ps1`, then
   the Unity 2018.4.10f1 batch build described in
   `docs/EXPANDED-SUMMONING-PTERANODON-CUSTOM-ASSET-BUILD.md`.

## What the generator authors

The current source is the **membrane test mesh** - the wing sheet alone, which
is the smallest piece of original geometry that can prove a replacement mesh
deforms correctly under the donor's own animations. The finished creature body
is authored on top of the same rig once that proof holds.

A pterosaur's brachiopatagium is one sheet bounded by the leading edge
(shoulder, elbow, wrist, elongated finger, wingtip), the trailing edge (wingtip
back along the finger fan to the ankle), and the root (ankle up the flank to the
shoulder). The donor rig ends each wing in six feather bones. `Feather_1` is the
longest and most forward, so it stands in for the elongated fourth finger and
carries the leading edge to the tip; `Feather_2` through `Feather_6` fan
backwards and their ends are the trailing-edge control points, with `Feather_6` -
which hangs off the shoulder - anchoring the innermost station above the ankle.

Pairing each trailing station with its own feather bone is the point of the
exercise: the trailing edge then follows those bones through the native
animations instead of sliding through them.

Every vertex carries at most three influences, inside Unity's limit of four.

## Bind poses are not shipped

The Unity builder normalises the mesh's bind poses before writing the bundle and
fails if any donor transform survives. At attach time the runtime loader rebuilds
them from the live donor: for each bone name the mesh declares, it takes that
transform's index in the donor renderer's `bones` array and reads the donor's own
`sharedMesh.bindposes` entry.

That is not merely a redistribution precaution, it is the only correct binding.
Unity skins a vertex as

    v_world = sum_i w_i * bones[i].localToWorldMatrix * bindposes[i] * v

so bind poses computed from the donor's pose at attach time would make the mesh
render as authored *in whatever animation frame the unit happened to be on*, and
attach time is arbitrary. Reusing the donor's own bind poses - the frame these
vertices were authored in - makes the binding deterministic.

## Reproducibility

The generator is deterministic given the same `rig.measured.json`: it performs no
random or hash-ordered operations and writes vertices in a fixed traversal order.
Blender's `.blend` container embeds session metadata and is semantically
reproducible but not byte-identical, which is why the `.blend` is a local build
artefact rather than the authority. The authority is this script plus the
recorded `rigSha256` in the build report.
