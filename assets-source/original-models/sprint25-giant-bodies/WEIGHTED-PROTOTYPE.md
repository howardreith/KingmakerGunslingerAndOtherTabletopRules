# Sprint 25 weighted motion prototype — what the skin actually does

The graybox rebuilt a static shape per state. It could show a silhouette and
nothing else: no vertex was bound to a bone, so none of its renders were
evidence about skinning, and re-deriving a pose by moving points proved only
that the script can move points.

This pass runs the same authored bodies through **the binding pipeline the
released generators use** — a donor-measured armature, one vertex group per
donor bone, normalised influences, an Armature modifier, FBX export — and then
moves them by **posing bones only**.

- Generator: `prototype_giant_bodies.py` (this directory)
- Bodies: `graybox_giant_bodies.build()`; the prototype consumes the
  influences that function already builds rather than a second copy of the
  body definition
- Donor: Mastodon prefab `fe9f25fe62e0f3b49a13910a996470b2`, 40 joints,
  `boneCount == bindPoseCount`, census SHA-256 `e2ce2a776dc06fdb…`
- Evidence: `runtime-evidence/sprint25-weighted-prototype/` — 99 PNG, 9 FBX,
  `prototype-report.json`
- Reviewed once, independently, read-only, against `REVIEW-PACKET.md`; the
  verdict was CHANGES_REQUIRED and this record was corrected by it (below)

## Why the pivot cannot be fixed later

`PteranodonAssetRuntime.TryResolveDonorBinding` resolves our bone **names**
against the live donor and takes `donorMesh.bindposes[donorSlot]` — the
donor's own bind poses. Unity then skins each vertex as
`sum_i w_i * bones[i].localToWorldMatrix * bindposes[i] * v`. So the frame a
vertex is authored in has to be the donor's bind frame, and a bone rotates the
skin about where that bone *is*, not where the art wishes it were. Everything
below follows from that one fact.

## What it proves, and what it cannot

**Proved: the deformation comes out of the skin cluster.** The unposed mesh is
hashed in every state — one distinct base-mesh digest per variant across all
pose evaluations, equal to the bind digest. The skinning path is also checked
against itself: every part driven by a single bone (skull, tusks) departs from
rigid motion by **at most 2e-6 units** in every pose.

**Proved: it survives the shipping export.** Every variant written to FBX and
read back — 40 bones, **0 unweighted vertices**, influence sums exactly 1.000,
at most 2 influences per vertex (3 for the damped weighting) against Unity's
limit of 4, Armature modifier present.

**Proved: the shells are sound.** 8 shells for the Brachiosaurus and 12 for
the Elephant, **0 open, 0 inside out**, 0 boundary edges, 0 non-manifold
edges, 0 loose vertices, in every variant.

**Not proved: native motion.** The guarded census captured clip *names*,
durations and event times — never curves. Every pose here is an authored probe
named after a donor clip (`walk`, `gore_attack`, `slam_attack`), not that
clip. Native animation qualification is outstanding and nothing here
substitutes for it.

## The decision this existed to settle

The donor's seven-joint forward chain is a **trunk**: its bind pose hangs
down. The Elephant's trunk is authored along it and is bind-faithful by
construction. A Brachiosaurus neck has to **rise**, so authoring it leaves the
chain's bind path — and by the binding contract above, those bones keep
pivoting where they are.

How far each chain bone sits from the skin it drives, in units and as a
multiple of that skin's own radius:

| geometry | Tail7_M | Tail21_M | Tail5_M | Tail6_M |
| --- | --- | --- | --- | --- |
| bind-faithful | 0.00 (0×) | 0.00 (0×) | 0.00 (0×) | 0.00 (0×) |
| authored upright | 3.14 (6.4×) | 4.48 (11.9×) | 6.64 (33.4×) | 7.48 (32.5×) |

Four weightings, against the chain-curl probe at 16°/joint:

| variant | visible stretch | floor | max displacement | verdict |
| --- | --- | --- | --- | --- |
| bind-faithful geometry | 1.54 | 0.81 | 4.06 | shape is wrong, not the skin |
| authored upright, full chain | **3.46** | 0.76 | **10.51** | destroyed |
| authored upright, chain damped to 0.25 | 1.01 | **0.14** | 2.86 | collapses |
| **authored upright, chain removed** | **1.01** | **0.96** | **1.24** | **holds** |

Damping to a quarter does not work: a quarter of a transform that is wrong by
7 units is still wrong by 1.8, and the surface collapses to 14% of its rest
length rather than tearing. Removing the chain influence does work, because
the remaining driver — `Head` — has its bind head **at the neck's own base**,
which is the pivot a sauropod neck should swing about anyway.

Swept across amplitudes (visible stretch, with displacement in brackets):

| °/joint | upright, full chain | upright, rigid |
| --- | --- | --- |
| 4 | 1.00 (3.33) | 1.00 (0.31) |
| 8 | 1.56 (6.33) | 1.01 (0.62) |
| 12 | 2.37 (8.77) | 1.01 (0.93) |
| 16 | 3.46 (10.51) | 1.01 (1.24) |
| 24 | 5.49 (11.48) | 1.01 (1.86) |

The rigid neck stays inside tolerance at **every** amplitude tested, including
24°/joint. Whatever the native clips do to that chain, this neck answers only
to the shoulder and head joints.

**What it costs.** 20 vertex groups instead of 27; the seven chain joints
carry nothing; the neck gains no secondary motion of its own.

**The alternative is not available.**
`brachiosaurus-bind-faithful-bind-side.png` is a Brachiosaurus authored on the
chain's own bind path: a quadruped with a fifth limb-shaped appendage reaching
the ground. Correct pivots everywhere, and not a sauropod.

## The seams, measured five ways

The Elephant is bind-faithful everywhere, so it is the body to measure seam
treatments on. Same poses, same metric, visible-surface stretch:

| treatment | slam | socket hull | gore | seam gap |
| --- | --- | --- | --- | --- |
| `blend` (add_tube default) | 1.68 | **1.22 — outside the body** | 1.20 | 0.137 |
| `pin` | **2.01** | 0.79 | 1.29 | 0.217 |
| `graded` (extra rings) | 1.52 | 0.79 | **1.58** | 0.217 |
| `chained` (graded through `Neck`) | 1.52 | 0.79 | **1.72** | 0.217 |
| **`mixed` — shipped** | **1.52** | **0.79** | **1.29** | **0.217** |

The default blend lets a slam carry the limb socket **out of the torso**.
Pinning keeps it in and makes a single band absorb the whole step. Extra
graded rings fix that for the limb and make the neck worse. `mixed` takes the
half that won in each case: graded limb sockets, pinned neck anchor.

Two refinements were tried and **lost**, and are kept in the generator as
measured alternatives rather than quietly dropped: grading the neck seam like
a limb (gore 1.29 → 1.58), and grading it along the donor's own
`UpperTorso → Neck → Head` chain (gore 1.29 → 1.72, and the curl floor from
0.84 to 0.13). Both fail for one reason: inserting rings **shortens** the
bands, so the same step spread over a third of the length reads as a larger
stretch. The `Neck` hypothesis was anatomically sound — that joint exists, the
gore pose swings it 24°, nothing weighted it — and the measurement refused it.

## What the independent review changed

A read-only reviewer opened all 18 assigned renders plus 4 it chose itself,
and returned **CHANGES_REQUIRED** with ten prioritised defects. Three were
corrections to claims **in this document**, and they were right:

### 1. "the other three are planted" was false

The walk probe rotates joints only — no root motion, no IK — so the body does
not settle onto the stance feet. The reviewer read the numbers off this
project's own report: three of four feet left the ground. The pose is now
named **`walk-swing`**, for what it does rather than for what it would need to
prove, and its per-foot lift is reported: `L_Palm +0.121, R_Palm −0.018,
L_Foot0 −0.036, R_Foot0 +0.202` above the sole plane. Stance contact across a
gait cycle is **unassessed** and needs native clips or an IK pass.

### 2. the four soles were not coplanar at bind

Measured −0.049 (fore) against +0.036 (hind). The donor is not the cause: its
own four foot joints are coplanar to 0.006 units. The spread was the foot ring
tilting about its joint. Every sole ring is now flattened onto the donor's own
sole plane, `z = 0.0352`, and the review ground plane is drawn there. At bind
all four soles now read **exactly 0.000** above it; `slam-foreleg` lifts one
forefoot to **+0.741** with the other three at 0.000.

### 3. the neck anchor ring was outside the torso in every pose

This document's seam table reported the **socket** ring at 0.79 — inside — and
never reported the **anchor** ring, which was at 1.008 (Brachiosaurus) and
1.145 (Elephant), i.e. partly outside the hull. The reviewer saw it directly,
as a flat cap and a protruding box on the chest of both creatures. The sink
was a guessed multiple of the neck radius; it is now **solved** against the
hull — the smallest sink that puts every vertex of that ring inside with
margin. Measured now: **0.840 and 0.844, with 100% of the ring inside, in
every pose of every variant.**

### What the review also fixed about the shape

The reviewer measured the Elephant trunk as a near-constant-width column
~80% of a leg's thickness that hooks at the end, and the 16°/joint curl as a
rigid boom plus a wrist flick. Both have one cause, which the generator could
not see and the eye could: **the donor's chain is not evenly spaced.**
`Head → Tail7_M` is 1.754 units; every joint after it is 0.44 to 0.54. Radii
were being tapered by ring *index*, so almost no narrowing happened over the
first 38% of the length and all of it over the short distal joints. Radii now
fall with **arc length**. The measured effect:

- the Elephant trunk's curl tolerance rises from 8°/joint to **12°/joint**;
- the Brachiosaurus rigid neck now holds tolerance at every tested amplitude.

The per-joint turn is now reported directly, and it shows the rest shape the
eye was reading. At bind the chain turns **54.2° and 45.1°** at its first two
joints and about 0.4° at each of the remaining five: the donor's trunk is a
double bend at the top and a straight run below it. Under the 16° probe the
distal joints turn 15–21° each over 0.44–0.54 units (33–39° per unit) while
the long base turns 29° over 1.73 units (17° per unit). A uniform per-joint
probe is therefore **not** a faithful stand-in for a trunk clip on this donor,
and the record says so rather than reading the render as a rig defect.

A trunk-curl render at **8°/joint** was added, because the only curl frame
previously supplied was one the generator itself scored out of tolerance.

### What the review found that this pass did not fix

Geometry and art, handed to the final pass with the reviewer's own measurement
attached — not deferred silently:

- **the trunk still reads as a fifth leg** at party distance. Tapering helped
  and did not settle it: it is still a straight column reaching the ground,
  and it passes within **0.06 units** of the foreleg at rest. Wanted: a
  resting S-curve, a distinct surface treatment, and real separation. Note the
  trunk reaching near the ground is *faithful* — the donor's own chain tip
  sits 0.12 above the foot plane — so the fix is shape, not length;
- **the Brachiosaurus neck is a hinged boom with a knob for a head.** The
  crease is the donor's own 44.9° rest turn carried rigidly through the
  re-aim; the head is a terminal cap, not authored skull geometry;
- **the shoulder and neck-base wedge**, which survived all five seam
  weightings and is a cylinder leaving a sphere with no mass between them;
- **the mid-limb joints `frontKnee_*` / `frontAnkle_*` drive 16 vertices.**
  The shin does not change shape; only the foot block moves, 0.40 units, with
  a 1.05 visible step at the ankle. An Elephant slam with no carpal bend will
  not read as a stomp;
- **the tail is a quarter of the neck's length** and nearly absent from the
  party silhouette;
- **a UV pole whorl on the dorsal midline**, plus a texel-density step across
  the shoulder (neck stripe pitch ~5 px against the torso's ~25 px). The torso
  is an ellipsoid unwrap with its pole on the back. Note the trunk and the
  legs do **not** share a UV region — they are `crest` and `limbs` — so the
  reviewer's hypothesis there is wrong while the observation that they read
  alike stands;
- **the ears do not yet read as ears**: flat plates about 0.6× head diameter
  held close to the skull;
- **the gore pose tucks the trunk between the forelegs** to ground level.

### What the review confirmed

The neck-weighting decision is settled in pixels, not only in the table: the
full-chain variant's neck collapses into a folded slab across the back with
the head absorbed into it, and its chain tip punches 1.40 units **through the
ground**. The rigid variant at the same amplitude is visually indistinguishable
from its bind pose. The bind-faithful alternative is genuinely unavailable.
The "free-floating island" is resolved — contact with a shadow line, never
background, in every view. No hole and no flipped face anywhere. The camera
labels in this packet are correct. The tusk UV region is right: the only ivory
pixels on the Elephant are on the tusk, and the Brachiosaurus carries none.

## Defining motion, per creature

**Elephant — the trunk.** Bind-faithful: every chain bone sits exactly on the
skin it drives (lever arm 0.00). Within tolerance to **12°/joint** (96° total)
and 1.37 at 16°. Tusk-to-trunk clearance 0.113 at bind and 0.128–0.137 under
curl: they never touch. `gore-strike` 1.29, `slam-foreleg` 1.52 with the
raised forefoot at **+0.741** and the other three at 0.000.

**Brachiosaurus — the neck.** Rigid on `Head`, so its defining motion is the
shoulder-and-head swing: `gore-strike` moves the skull 3.24 units at 1.20
visible stretch, and the trunk-chain probe moves it 1.24 at 1.01.

## Required before final geometry

1. **Weight the Brachiosaurus neck to `Head` alone.** Not a style choice; the
   alternatives measure 3.46 and 0.14.
2. **Shoulder and neck-base mass, with a unioned surface.**
3. **More rings where a seam bends**, and a skull and ears that are authored
   rather than capped primitives.
4. **Weight the mid-limb joints**, or accept the 1.05 shin seam and say so.
5. **Separate the trunk from the foreleg** in shape and surface.
6. **Move the UV pole off the dorsal midline** and normalise texel density.
7. **Native clip amplitudes.** Every tolerance above is read against authored
   probes, and the donor's own chain spacing means a uniform probe is not the
   same shape of motion as a clip.

`originalBodyAssetsAuthored` stays **false**. This is a prototype: it proves
the binding and the motion, and it is not finished art. Human aesthetic
approval remains `NOT_PERFORMED_NONBLOCKING`.
