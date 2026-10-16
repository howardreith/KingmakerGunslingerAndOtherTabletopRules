# Sprint 25 graybox motion prototype — visual review record

Low-detail prototype, built before any final geometry, to answer one measured
question: the donor's seven-joint forward chain is a **trunk**, and
Brachiosaurus needs a neck that rises. Faceting is expected and is not a defect
at this stage.

- Generator: `graybox_giant_bodies.py` (this directory)
- Donor: Mastodon prefab `fe9f25fe62e0f3b49a13910a996470b2`, 40 skinning joints
- Space: renderer-local **bind** frame from the guarded census, converted to
  Blender Z-up by `(x, y, z) -> (x, -z, y)`. Feet land on `z = 0`.
- Renders: `runtime-evidence/sprint25-graybox/` (20 PNG, 1000x720, EEVEE_NEXT)
- Inspection: pixels opened directly, not described from filenames or hashes.

## What the prototype establishes

**The upright neck is viable.** Standing the chain up takes a **127 degree**
re-aim of the bind pose. The resulting silhouette reads as a sauropod: long
rising neck, small head at the far end, barrel torso, four columnar legs, rear
tail untouched. The earlier "rigid pole neck" concern is not reproduced.

**Curl headroom is the real constraint, and it is tight.** Sweeping a
trunk-style progressive curl on the authored upright neck:

| curl per joint | total | skull aim along forward | verdict |
|---|---|---|---|
| 0 deg | 0 deg | +0.79 (looks ahead) | clear |
| 4 deg | 24 deg | +0.41 | clear |
| **8 deg** | **48 deg** | **-0.07 (looks straight up)** | clear |
| 12 deg | 72 deg | -0.53 (looks backward) | clear |
| 16 deg | 96 deg | -0.87 | head behind shoulder |
| 24 deg | 144 deg | -0.90 | head behind shoulder |
| **40 deg** | **240 deg** | +0.74 | **neck intersects torso** |

So the authored neck tolerates roughly **40-50 degrees of total curl** before
the skull stops facing forward, and the geometry only self-intersects at a very
large curl. An elephant trunk clip routinely curls well past 50 degrees, so the
native clips that drive this chain - `idle`, `idle2`, `idle_on_alert`, `walk`,
`slow_walk`, `run`, `gore_attack`, `gore_attack_02`, `slam_attack`,
`slam_attack2` - are the open risk.

**This is not motion evidence.** The curl is a geometric probe of the authored
rig. The census captured clip *names*, not animation curves, so the actual
per-joint amplitude of those clips is still unmeasured. That measurement, or
in-game action frames, is the next required evidence before final geometry.

## Correction cycles performed

Each cycle: render, open the pixels, name the defect, change the real geometry
or the authoring frame, render again.

### Cycle 1 — wrong authoring space
- **Image:** `elephant-rest-side.png` (first generation)
- **Observation:** the trunk arced upward over the back and the limbs splayed
  sideways; the creature did not stand.
- **Root cause, measured not guessed:** the generator used the census
  `worldPosition` rows, which carry an arbitrary Unity scene placement. Every
  released body in this project is authored in the renderer-local **bind**
  frame, which the census also captured.
- **Correction:** load `skinnedRenderers[0].bones[].bindPosition`; convert
  `(x, y, z) -> (x, -z, y)`; replace the hand-rolled camera with the released
  review scripts' Z-up `point_camera` helper.
- **Resolved:** `elephant-rest-side.png` now shows a standing quadruped with
  all four feet on the ground plane and the trunk descending.

### Cycle 2 — elephant skull at the wrong end of the chain
- **Image:** `elephant-rest-side.png` (second generation)
- **Observation:** the trunk read as a limb ending in a rounded foot; there was
  no head mass anywhere.
- **Root cause:** the head ellipsoid was placed at the chain **tip** for both
  creatures. That is right for a sauropod, whose skull is at the far end of the
  neck, and wrong for an elephant, whose head is at the chain **base** with the
  trunk descending from it.
- **Correction:** head placement is now per creature - tip for Brachiosaurus,
  `Head` joint for Elephant - with ears and tusks added so tusk clearance can
  be judged.
- **Resolved:** the head is now a distinct mass at the front-top with the trunk
  descending from it.

## Open defects after cycle 2

Observations from the current renders, for the next authoring pass.

1. **Neck hinge** — `brachiosaurus-upright-side.png`, neck lower third. A sharp
   angle change rather than a smooth curve. *Hypothesis:* the 44.9 degree rest
   turn between `Head -> Tail7_M` and `Tail7_M -> Tail11_M` is carried through
   the rigid re-aim. *Correction:* distribute that turn across the first three
   joints when authoring the neck curve, rather than preserving the bind
   curvature rigidly.
2. **Neck base placement and seam** — same image, shoulder. The neck emerges
   from the top-rear of the torso with a visible notch at the join.
   *Correction:* move the torso ellipsoid's forward pole to meet the neck base
   and weight the first neck ring partly to `UpperTorso`.
3. **Skull aim under curl** — `brachiosaurus-curled-side.png`. At 16 deg/joint
   the head tips up and back. *Correction:* damp the upper-joint weights so
   native trunk curl produces less neck bend, pending the real clip amplitudes.
4. **Tail too short** — `brachiosaurus-upright-side.png`. The rear chain is
   only 0.63x torso; a sauropod tail should be long and tapering.
   *Correction:* author tail geometry that extends past the chain tip, with the
   last ring weighted to `Tail4_M`.
5. **Elephant head too high and too far back** — `elephant-rest-side.png`. It
   perches on the shoulders instead of integrating at the front.
6. **Ears barely read, tusks not visible** — `elephant-rest-side.png`. Tusk
   clearance against the trunk cannot be judged yet.
7. **Torso too short and round** for both creatures.

## Views not yet provided

No walk, turn, attack, trample, death or interruption frames, because those
require the native animation curves or in-game capture. No clay/wireframe pass
and no textured pass on real UVs. No native screenshots - the product candidate
is not valid while the painted icon is missing.


## Independent read-only review, and what it changed

A separate read-only reviewer (`.claude/agents/kingmaker-visual-reviewer.md`,
Read/Glob/Grep only) opened all eight assigned renders plus the shipped
reference icon and returned **CHANGES_REQUIRED** with eight prioritized
defects. It went further than the authoring pass had: it cropped and upscaled
regions from the same source files and ran per-column silhouette measurements,
so its findings carry source-pixel coordinates rather than impressions.

It found one blocking defect the authoring pass had under-reported. What had
been written down as "a seam at the neck join" measured **18 to 52 pixels of
background** between the torso and the chain root - 8 to 20 percent of body
length - on both creatures and from every angle shown. The head and neck were a
free-floating island, not a seam.

It also caught a systematic bug with a cross-check neither the eye nor a code
read had produced: the *same* chain overshot the foot plane by 11 px on one
body and floated 61 px above it on the other. The cause was a leg extension
pushing the feet below their measured joints, which destroyed the donor's own
chain-tip-to-ground relationship.

### Corrections applied, then re-inspected

| Defect | Correction | Verified by re-opening pixels |
|---|---|---|
| P1 chain detached from torso | chain, tail and limb roots sink toward the torso **centre**; the anchor ring is weighted to `UpperTorso` | island gone, neck meets the torso |
| P2 skull detaches under curl | skull sinks **into** the last segment rather than overhanging its cap | join closed |
| P3 limbs butt-joined, caps exposed | limb sockets sunk a radius into the hull | shoulder and haunch masses present |
| P4 neck had no taper (56/56/55/54 px) | base-to-tip ratio widened | taper visible |
| P5 ground relationship broken | removed the below-foot leg extension | see the correction below |
| P7 ears read as upward horns | rebuilt as lateral fans beside the skull, offset on `lateral` rather than a mostly-upward vector | no longer spikes |

A first attempt at P1 over-corrected: sinking the root *backward along the
chain* put it outside the hull on the far side and hung a wide slab down the
body's flank. Sinking toward the torso centre - the method the limb sockets
already used - lands inside by construction. The table above reports the
second render.

### A correction to this document's own earlier claim

The header previously said the forward chain "descends ~4.0 units to ground
level". That is imprecise. In the donor's own bind data the chain tip sits at
**y = 0.151** and the feet at **y = 0.035**: the trunk ends slightly *above*
the foot plane. Some trunk-to-ground clearance is therefore faithful to the
donor and is not a defect to author away.

### Still open after this pass

- The join is closed but still shows a thin seam: these are placed primitives,
  not a unioned mesh. Union at build time belongs to the real geometry pass.
- The elephant head, ear and trunk assembly reads cluttered from the party
  camera, and the ears do not yet clearly read as ears.
- **P6 is unresolved and unclassified.** An enclosed dark ellipse in the trunk
  surface at x494-506, y423-430 is either an unintended hole or a flipped
  face. Matcap-style shading cannot distinguish them; a backface/normals pass
  is required.
- **P8**, the vertically-standing elephant skull with a trunk broader than it,
  is not addressed.
- The tail is still short and flat where a sauropod needs a long tapering one.

### What the reviewer confirmed is NOT broken

- The 127 degree re-aim does not break the chain: a continuous,
  non-self-intersecting column, no collapsed, inverted or overtwisted segment.
- The 16 deg/joint curl is sound **along the shaft** - smooth C-curve, no
  pinching or self-intersection. The failure was confined to the tip.
- The re-aim does not compromise the rear tail: `Tail0_M..Tail4_M` renders
  identically across rest, upright and curled, confirming independence from
  the re-aimed chain.
- The brachiosaur forelimb-taller proportion is authored: 243 px against 200.
- Feet are coplanar in every provided state.

### Views the reviewer could not cover

Rear views of both creatures, the elephant curled state entirely, brachiosaurus
curled from a second camera, any wireframe/flat-shaded/normals pass, any motion
frames, a ground plane or scale reference, head close-ups, and a party-camera
render at the shipped 128 px icon scale. Its icon-scale readability remarks are
inference from full-size silhouettes.

A clean inspected subset is not a whole-model or runtime pass, and internal
visual acceptance is not the owner's aesthetic approval.

`originalBodyAssetsAuthored` stays **false**: this is a graybox, not final
geometry, and a prototype is not finished art.
