# Original Sprint 14 insects

Giant Ant Soldier, Giant Ant Worker and Fire Beetle geometry, UVs and paintings
are original project-owned procedural work. No native vertices, triangles,
texture, material or animation is copied. All three original meshes use only
bone names and weights from the live Giant Spider renderer; runtime supplies
that individual donor's own bind poses during the existing instance-local
renderer swap.

The geometry-free request-local capture remains outside Git and the package.
Use Blender 4.5.10 LTS. For each key, run
`paint_sprint14_albedo.py -- --kind <key> --out
assets/sprint14-insects/<key>-albedo.png` under Blender's bundled Python, which
is where numpy lives, then run
`generate_sprint14_insects.py -- --kind <key> --capture <private capture>
--albedo assets/sprint14-insects/<key>-albedo.png --mesh-data
assets/sprint14-insects/<key>-mesh.json --report <private report.json>
--blend-out <private source.blend> --fbx-out <private preview.fbx>`.

All three keys use the Sprint 14 Giant Spider capture. The generator rejects an
incomplete, repeated, multi-renderer or wrong-family capture and names the
sprint whose capture each creature is entitled to, so a Sprint 12 or 13 file
cannot silently stand in for this one. Private reports record the exact capture
hash. Blender/FBX previews, reports, captures and review renders remain
machine-local. The shipped files contain original geometry, painting, UVs,
weights, reviewed bone names and the albedo hash only.

## Why one spider can carry an ant and a beetle

The tranche donor census established that Kingmaker has no beetle and no ant,
and that the Giant Spider is the only compact many-legged arthropod in the
game. That makes it the best available donor, which is not the same as proving
one rig can carry both a six-legged ant walking and a beetle flying, so the
owner's order required two minimal vertical slices before the five Sprint 14
and 15 models were authored. The Soldier and the Fire Beetle are those two
slices; the Worker follows from the Soldier once they pass.

The measured bind frame - 51 bones, one skinned renderer, Z up and negative Y
forward - answered the ground half immediately. The donor has four leg chains a
side, fanning front to back, and the first three a side are exactly an insect's
fore, mid and hind pair. In a standard alternating tetrapod gait the donor moves
`{L0, R1, L2}` against `{R0, L1, R2}`, which *is* the insect alternating
tripod, so six legs is a subtraction from this rig rather than an addition to
it and the gait comes out right rather than merely tolerable. The chelicerae
sit where mandibles go, the seven-bone pedipalps are more articulation than an
antenna needs, and the abdomen chain is a petiole and gaster already.

That left the fourth chain, and the two slices answer it differently. The ant
weights nothing to it at all, so there is no eighth leg to hide: a bone with no
vertices draws nothing, and nothing of the donor's fourth pair can reach the
ground. The beetle gives it the membranous wings, because `Leg3` is the one
spare chain already behind the body and already above it - its knee is at 1.064
over a body at 0.687 - which is where a beetle's wings are and nothing else on
this rig is. The wings are weighted to that chain but are not laid along it: a
wing runs out and back from the shoulder nearly level where a leg dives outward
and down, and geometry does not have to follow the bone that drives it. Nothing
is weighted to the fourth foot, so no part of a wing can reach the ground.

`EXPECTED_CHAINS` in the generator makes this an offline invariant rather than
something a reviewer has to catch by eye: the build fails unless the ant's
fourth chain is empty on both sides and the beetle's carries geometry on its
upper and lower bones and none on its foot. Each shipped report records
`legChainUsage` and `visibleLegs`, which is six for all three.

## The review sheet

`render_sprint14_review.py` takes clay, unlit, textured and silhouette shading;
three-quarter, side, front, top-down and party-camera framings; and two
synthetic poses. The poses use no donor animation - none leaves the game - and
are rotations the script invents on the private preview armature. `stride`
swings the three weighted chains into the alternating tripod at fourteen
degrees, which is the pose in which six visible legs and no phantom contact
either holds or does not; `wing-stroke` raises the beetle's fourth chain so the
wing's deformation at the top of a stroke can be seen rather than guessed at.

The two ant castes are one builder taking a caste rather than two near-copies,
because they differ in exactly the three things that separate them in the
source: head size, mandible weight and whether there is a sting. The worker's
stat block is the soldier's with the Worker template applied - no poison sting
and no grab, which leaves a bite alone - so its head no longer has to carry
one, its gaster is the larger mass, and its chitin is painted lighter and
duller. With both castes on screen the soldiers are the dark ones, and from
behind the sting tells them apart outright.

The expected visual reads are: a dark reddish-brown ant in three masses on a
thread - an oversized soldier's head with heavy curved mandibles, a humped
mesosoma, a hard narrow waist and an egg-shaped gaster carried above the thorax
line, with antennae arching over the head and a sting at the tip; and a broad
near-black beetle with a red cast, one continuous shell from head through
pronotum to elytra, no waist at all, two luminescent glands on the head as the
only bright thing anywhere on it, and a pair of membranous wings that sweep up
into a V when the fourth chain lifts.

## Defects found and repaired in offline review

Three passes, seven defects, none of which cost a game launch.

1. **The wings were needles.** The blade builder's width axis was given the
   span itself rather than a vector perpendicular to it, so the width was added
   along the spine and the blade collapsed. Both wings read as spears.
2. **Both creatures' legs tapered evenly from body to tip**, which is a
   spider's limb. Six of them read as spikes radiating out of a lump. An
   insect's femur is the thickest part of it and its tibia is a wire, and that
   contrast is the one leg proportion that survives at party-camera distance.
3. **The ant's gaster was a faceted lozenge** rather than an egg, and after the
   first repair a spindle: the waist was held narrow for twice as far as it
   should be, which gave a thin neck with a conical flare at each end. What
   reads at distance is the ratio across the waist, not its length, so it is
   now short and severe - a twentieth of the body's width against a quarter.
4. **The ant's head was the smallest of its three masses**, which is a worker
   rather than a soldier, and it sat too close to the mesosoma to read as a
   separate mass at all.
5. **The ant's antennae went forward and down** with the donor's palps. Each
   ring is now lifted further than the last until the chain arches over the
   head and comes down past the mandible tips.
6. **The beetle read as a spider wearing a box.** This one is a fact about the
   donor rather than a mistake: its leg chains reach 1.74 out from a body 0.35
   wide, so anything built at the ant's girth reads as a spider whatever is on
   its back. Shortening the leg geometry would leave the visible limb hovering
   wherever the animation plants the bone, which is a worse defect than the one
   it fixes, so the body grew to meet the legs instead. A Fire Beetle is Small;
   absolute size is the view-scale entry's job and proportion is the mesh's.
7. **The beetle's head, pronotum and abdomen read as a stack of plates**, being
   three overlapping primitives on a wide body. A beetle is one hard shell, so
   it is now one blade whose per-ring width and height carry the neck pinch,
   the pronotum flare and the elytral taper. A blade rather than a tube because
   a tube's cross-section is circular and a beetle is half again as wide as it
   is tall.

The synthetic stride also had to come down from twenty-six degrees to fourteen,
because the donor's own legs fan only eighteen to thirty-three degrees apart and
twenty-six crossed adjacent legs into two fans. What the donor's real walk cycle
does at its own amplitude is a live-review question.

## What offline review cannot settle

The remaining questions are the guarded live review's, and they are the reason
these two creatures are hidden until the sprint candidate qualifies: whether
the donor's real gait crosses the six legs or reads as a tripod, whether the
wings' stroke reads as a wingbeat at animation speed rather than as a leg
cycle, contact timing, fades, selection and collision, shader and
material-controller behaviour, crowding and cleanup.
