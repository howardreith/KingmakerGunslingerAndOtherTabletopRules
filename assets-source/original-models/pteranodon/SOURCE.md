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
   blender --background --factory-startup --python generate_pteranodon.py -- \
       --rig rig.measured.json --out pteranodon.fbx \
       --blend-out pteranodon.blend \
       --report pteranodon-build-report.json
   ```

   Add `--mesh-data assets/pteranodon/pteranodon-mesh.json` to emit the file
   the runtime loads. That is the whole build: there is no Unity editor step.
   `docs/EXPANDED-SUMMONING-PTERANODON-CUSTOM-ASSET-BUILD.md` records why this
   ships as mesh data rather than an AssetBundle like every other custom asset
   here.

## What the generator authors

`--parts membrane` emits the wing sheets alone: the smallest piece of original
geometry that can prove a replacement mesh deforms correctly under the donor's
own animations. `--parts all`, the default, adds the body on the same rig.

`body-plan.md` records where the donor's anatomy and a pterosaur's disagree and
what was decided in each case. The three that matter: the beak extends forward
of the last head bone on purpose, so an opening jaw swings a beak rather than
twitching a head; the crest is weighted entirely to `Head`, because a crest is
bone; and the donor's eagle tail fan is left carrying no geometry at all rather
than being dressed up as a tail a Pteranodon does not have.

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

The exported mesh data contains no bind poses at all, and the runtime builds the
mesh with identity ones. At attach time the loader replaces them from the live
donor: for each bone name the mesh declares, it takes that
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
