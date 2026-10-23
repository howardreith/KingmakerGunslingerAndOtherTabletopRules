# Original Sprint 21 bodies: the Bebelith and the Giant Crab

Both creatures' geometry, UVs and paintings are original project-owned
procedural work. No native vertex, triangle, texture, material or animation
is copied. The shipped meshes use only bone names and weights from the live
Giant Spider renderer; runtime supplies that donor's own bind poses during
the existing instance-local renderer swap.

The geometry-free request-local capture remains outside Git and the package.
Use Blender 4.5.10 LTS. Run each painter first, because the generator reads
the painting's bytes into the shipped manifest:

```
blender --background --factory-startup --python paint_bebelith_albedo.py -- \
    --out assets/sprint21-arthropods/bebelith-albedo.png

blender --background --factory-startup --python generate_bebelith.py -- \
    --capture <private census capture> \
    --albedo assets/sprint21-arthropods/bebelith-albedo.png \
    --mesh-data assets/sprint21-arthropods/bebelith-mesh.json \
    --report <private report.json> --blend-out <private source.blend> \
    --fbx-out <private preview.fbx>
```

and the same two commands for `giant-crab`.

Blender and FBX previews, reports, captures and review renders stay
machine-local. The shipped files contain original geometry, painting, UVs,
weights, reviewed bone names and the albedo hash only.

## Why one rig carries both

The Sprint 21 census surveyed two families independently - a Huge demonic
spider-hunter and a Medium lateral crab - with their own anchors, discovery
terms and prefab budgets, because the easiest wrong decision available was to
notice that both are arthropods, survey once, and author both against
whichever rig the single survey liked.

They landed on the same skeleton anyway, for different measured reasons.

For the Bebelith: its released donor is the Doomspider, and the census
measured the Doomspider and the Giant Spider to be the same 51-bone rig, bone
for bone. The only other demonic candidate the installed library offers is
`DemonEidolonUnit`, a biped with one leg pair. A Huge spider-hunter therefore
has exactly one rig available with more than two legs.

For the Giant Crab: it needs eight real leg chains and two forward
articulated chains. The centipedes the search found report four leg pairs of
nothing - that family names its chains differently - and the arachnid
skeleton is the only thing in the installation that has them.

That both answers are the same rig is a fact about what the game ships, not a
shared decision. Each is recorded under its own family in the census.

## What the census measured

| Chain | Bones | What it carries |
| --- | --- | --- |
| Walking legs | four complete pairs, upper/lower/foot | The crab's eight; the Bebelith's six |
| Pedipalps | seven a side, forward-reaching | The crab's pincers; the Bebelith's barbed foreclaws |
| Abdomen | `Tail1_M`, `UpperTorso`, `Tail3_M`, all level at about z 0.7 | The Bebelith's web-spinning abdomen. The crab must not touch `Tail3_M` at all |
| Jaw | `chelicera_L`, `chelicera_R` | The Bebelith's printed bite |

The jaw was reported absent by the census's first run. This skeleton spells
its legs and feet capitalised and its pedipalps and cheliceræ lowercase, in
one bone roster, and the candidate list had `Chelicera_L` with a capital. The
raw roster the capture also writes is what exposed it, which is the reason
the capture writes one; chain matching is case-insensitive now.

## What makes each body its own creature

The Bebelith is the first body in this series authored for a creature that
already ships wearing something, so its whole authoring problem is inverted:
not "what can this rig carry" but "what must this body do so it is not a
repaint of what the rig already wears". Four answers, and the generator
refuses to build the body without them:

- It walks on **six**. The fourth leg chain carries no geometry at all -
  `legChainUsage` must be `full, full, full, empty` exactly, because a
  bebelith that walks on seven legs is a defect and one that walks on eight
  is the Doomspider this body replaces.
- Two **elevated barbed foreclaws** on the complete pedipalp chains, held
  above the walking line, with three spines along each blade.
- A real **jaw**: heavy mandibles on the two cheliceræ, because the printed
  bite is a primary attack with its own 2d6 die and it carries rot.
- A **web-spinning abdomen** ending in two spinneret pairs.

The Giant Crab's own four, likewise enforced:

- A **broad shell**, measured from the built mesh: width over length must
  exceed one, which no other creature on this rig does.
- A **lateral stance**: each leg is carried out along ±X from its own chain
  before it bends down.
- **Two large unequal pincers**, both open, on the complete pedipalp chains.
- **No abdomen and no sting**: `Tail3_M` is the bone a spider's abdomen and a
  scorpion's metasoma hang on, and the generator refuses to build if the crab
  reaches it.

## Recorded limitations

`BEBELITH_ABDOMEN_RIDES_A_TWO_BONE_CHAIN`. The abdomen chain is `Tail1_M` and
`Tail3_M` with `UpperTorso` between them, three bones, all level. A bebelith's
abdomen is one mass rather than a scorpion's articulated tail, so this costs
less here than it did in Sprint 20 - but the spinnerets cannot aim, and the
web is cast from a body that sways rather than from a turret.

`CRAB_WALKS_A_SPIDER_RIG_FRONT_TO_BACK`. The donor's four leg pairs are
positioned fore and aft, because they belong to a creature that walks
forwards. The geometry pushes each leg out sideways so the stance reads
laterally at rest and in a turn, but the walk cycle is the donor's, and a crab
that scuttles sideways is not something a borrowed clip can be made to do.

## What the build passes got wrong

Written down because each cost a render and the next body on this rig can
skip them.

- **Size is not geometry.** The Bebelith's first build multiplied every offset
  by 1.62 to make a Huge creature. The donor's bone positions do not scale
  with it: the body inflated around an unchanged skeleton, the legs stayed
  where the bones put them, and the render was a lump resting on the ground
  with spikes coming out of it. Size on this rig is the view scale's job,
  which scales skeleton and mesh together.
- **Separate ellipsoids are separate surfaces.** The crab's first carapace was
  four of them - a dome, a rim, a back and six anterolateral teeth - on the
  assumption they would read as one shell. The teeth rendered as six pebbles
  lying beside the animal. The shell is now one domed mass with one heavily
  overlapping front.
- **This donor has no knee.** Its upper, lower and foot bones are nearly
  collinear in bind pose, so following them exactly gives straight spikes and
  a body on the floor. Both bodies push the knee out from its own chain
  before dropping to the foot - and only barely up, because the donor is a
  squat spider whose feet sit about a tenth below its torso.
- **Anchor attachments to the geometry, not to the bone.** The Bebelith's
  spinnerets were placed from the abdomen chain's last bone after the abdomen
  mass had already moved forward and up off it, and rendered floating behind
  the animal.

## Outward repair, 2026-10-17 (visual refinement)

The owner approved repairing the original bodies that the visual
refinement found shipping inside out from the legacy shared writer
("Let's assume that your repairs are good. Move forward with them.").
The shipped Giant Crab body was repaired in place with `../continuous-bodies/repair_outward_winding.py`
(the generator's private donor capture is not needed): each triangle is
wound to agree with its own authored normals, and a closed part whose
volume is then negative has its triangles and normals turned. Positions,
UVs, weights and bones are unchanged.
It now declares `triangleWinding: outward-visual-refinement` and passes
the strict closed-and-consistent check. The Bebelith, the owner's accepted
visual control, is unchanged.

| File | Before | After |
| --- | --- | --- |
| `assets/sprint21-arthropods/giant-crab-mesh.json` | `c2b84d93f304…` | `dd7448eb9fc384350baf734fa8e7430994a5b4c08858f79b53eab1c30d2ed8eb` |

To reproduce the shipped bytes, run the generator, then the repair on its
output with `--marker outward-visual-refinement`. `tests/KingmakerGunslinger.DomainTests/RepairedBodyTests.cs` pins
the result and checks it is lit from the front.

## Cleanup 2 authored Crab revision (2026-10-21)

`build_giant_crab.py` supersedes the legacy crab generator. It replaces the
round separate palm and thin raised legs with a broad low shell, eight
segmented connected leg surfaces, and two continuous asymmetric chelae whose
open fingers grow from flattened palms. New original mottled warm-brown
chitin includes a lighter shell rim and dark working tips. This is an anatomy
and weight revision, beyond the previous outward-only repair above.

The input is the private Sprint 14 `sprint14-giant-spider-bind-rig.json`, SHA-256
`60074cc2edb939cd4ab3e49961eec470866f94096d40e97f490dedae60faa19b`.
Sprint 21 independently measured this same 51-bone Giant Spider frame. The
actual reused capture hash is recorded honestly in the payload. No native
vertices, pixels or animation curves are read. The donor still walks forward;
`CRAB_WALKS_A_SPIDER_RIG_FRONT_TO_BACK` remains explicit.

```text
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/sprint21-arthropods/build_giant_crab.py -- --capture <private sprint14-giant-spider-bind-rig.json> --albedo <out>/giant-crab-albedo.png --mesh-data <out>/giant-crab-mesh.json --report <private report.json> --blend-out <private source.blend>
python assets-source/original-models/sprint21-arthropods/verify_cleanup2_bodies.py --root <repository> --out <private report.json>
```

The existing continuous-body library is unchanged. New offline helper
`cleanup2_authored_parts.py` supplies union export, deterministic painting and
local driver interpolation for this explicitly assigned cleanup group.

| Current file | SHA-256 |
| --- | --- |
| `giant-crab-mesh.json` | `34833285f367258ed5d00127c72d86bb96897df94459740d3c9fd800e720893e` |
| `giant-crab-albedo.png` | `1182b139b3a8dc049eaa68d7db8d9bcf865d02e321af2e8d13b46688866409ce` |

Two clean Blender 4.5.10 processes produce identical mesh and PNG bytes.
Closed outward topology, eight complete leg chains, two whole chela chains,
normalized weights and seam continuity pass offline checks. Synthetic probes
in `cleanup2-arthropod-stress-poses.json` are authoring evidence only. Native
movement/turn/claw/contact evidence and owner visual acceptance remain with
the integrator; no native or aesthetic PASS is granted here.

Earlier sections are retained historical source and qualification records.


## Crab folded-chela contact repair, 2026-10-23

Retained native review27 showed an ordinary successful claw rule while the
visible pincer remained short of the target. The fixture target is the native
DefaultPlayerCharacter clone: observed footprint sum is Crab 1.1 + Human 0.5,
and unchanged native approach radius 2.2096. The prior seven-pivot pose curls
its authored tip across the midline and down. Exact project-mesh bind-frame
FK moved the left tip from (-0.54993,-1.42157,0.42573) to
(0.24500,-1.06146,0.04236). Old native FAIL records are preserved.

`build_giant_crab.py` now authors a folded upper arm, narrow elbow flex joint,
forearm, wrist and narrower elongated open pincer. Its source centerline is
also the exact private render-palette pivot contract in
`SummonAttackGestureVisual.AuthoredChelaPivot`. Smooth arclength weights avoid
abrupt nearest-branch ownership changes on the inside of the folded elbow.
The original shell, eyestalks, eyes and all eight legs retain all 3,984 unique
position/normal/weight records and all 7,916 protected triangles exactly.
UV seam duplication and the atlas are repacked; the painting is rebaked.
The bright/dark threshold formerly cutting across the forearm is replaced by
an elbow-local joint treatment and a smooth distal finger gradient.

The production gesture changes only Crab's private render palette. It maps
authored pivots through each actual live inverse bind, with a matching
recenter. Its existing synchronous native current-pose BakeMesh comparison
must still pass before admission. Source bones, renderer root, native
colliders, weapon/rule/action objects, native no-handle HasAnimation=false
branch and actual act timing remain untouched. Brachiosaurus tail logic is
unchanged. There is no fabricated pre-act windup: the strike begins at the
actual native act boundary and recovers over 0.5 actual game seconds.

Shipped identities, deliberately LF JSON:

- Mesh 8247 vertices / 11516 triangles / 39 used bones:
  `75a6d5d625f8e88d9cfccec0de3d411610123be0f94f4655d5fc9eeadaf05472`.
- Original 1024x1024 albedo:
  `ef06b21b0654bcee40ca893cfff28c20e5fea469402cadb1d7a3a524f8e48112`.

Two independent Blender 4.5.10 generations matched both files byte for byte.
Existing focused mesh/weight/closure/winding audit passed. Ignored local
`artifacts/cleanup2-crab-contact/folded-v4/` contains reviewed original-mesh
front, side and gameplay neutral/act/recovery previews plus exact synthetic
FK metrics. Fifty-two left/right post-act samples stayed finite, left all
non-chela vertices fixed, returned to neutral and kept edge-length ratios
within 0.6701..1.7057 (act p01..p99 0.9219..1.0977). These are authoring
checks, not native current-pose/contact or owner visual acceptance. Actual
native single/full contact and source/collider invariance remain required.
