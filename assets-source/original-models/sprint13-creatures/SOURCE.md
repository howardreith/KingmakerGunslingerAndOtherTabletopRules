# Original Sprint 13 creatures

Wolverine, Shadow Mastiff and Poison Frog geometry, UVs and paintings are
original project-owned procedural work. No native vertices, triangles, texture,
material or animation is copied. The three original meshes use only bone names
and weights from the respective live Worg and Giant Poisonous Frog renderer;
runtime supplies that individual donor's own bind poses during the existing
instance-local renderer swap.

The Wolverine and the Shadow Mastiff ride the Worg rig, whose bind frame Sprint
12 already captured for the Goblin Dog, so both read their bones from that
Sprint 12 capture and Sprint 13 needed only one new capture, for the Giant
Poisonous Frog.

The geometry-free request-local captures remain outside Git and the package.
Use Blender 4.5.10 LTS. For each key, run
`paint_sprint13_albedo.py -- --kind <key> --out
assets/sprint13-creatures/<key>-albedo.png` under Blender's bundled Python,
which is where numpy lives, then run
`generate_sprint13_creatures.py -- --kind <key> --capture <private capture>
--albedo assets/sprint13-creatures/<key>-albedo.png --mesh-data
assets/sprint13-creatures/<key>-mesh.json --report <private report.json>
--blend-out <private source.blend> --fbx-out <private preview.fbx>`.

Wolverine and Shadow Mastiff use the Worg capture; Poison Frog uses the Giant
Poisonous Frog capture. The generator rejects an incomplete, repeated,
multi-renderer or wrong-family capture, and names the sprint whose capture each
creature is entitled to, so a Sprint 12 file cannot silently stand in for a
Sprint 13 one or the reverse. Private reports record the exact capture hash.
Blender/FBX previews, reports, captures and review renders remain machine-local.
The shipped files contain original geometry, painting, UVs, weights, reviewed
bone names and the albedo hash only.

The expected visual reads are: a low, front-heavy mustelid with a broad blunt
skull, small round ears, a pale band running hip to shoulder over a near-black
coat, heavy short legs and a short bushy tail; a large shadow-black hound with
a deep chest, a drawn-in loin, ears carried high, a long heavy tail and two
pale eyes that are the only light thing on it; and a small round frog with a
head nearly as wide as its body, eyes that break the outline at the top and
sides, and aposematic warning colour broken by dark blotches, which is what
separates it from the drab Giant Poisonous Frog it shares a rig with.

Offline rest-pose renders are an authoring check. Live idle, movement, attack,
hit, death, quantity, party-camera, clipping, targeting, fallback and cleanup
review remain required before publication.

## First rest-pose review and repair, 2026-10-02

The first internal rest-pose review found none of the three creatures reading
as its species. Seven defects were reproducible in every view, and none of them
was a matter of taste.

1. **All three heads were cones.** Each head tube took the body tube's own
   radius at the neck and then only ever narrowed, so neck, skull and muzzle
   formed one unbroken taper with no skull in it. The Wolverine read as an
   anteater and the Shadow Mastiff as an otter. Each neck ring now pinches
   below the shoulder and each skull ring swells back out above it, and both
   muzzles pull back - the Wolverine's to 0.155 forward, the Mastiff's from
   0.330 to 0.290.

2. **The Wolverine's ears did not exist.** They sat 0.105 above a skull of
   radius 0.215 and stood 0.056 tall, so both were entirely inside the head.
   Their bases now meet the skull surface and their caps clear it, while
   staying short and round; a canid's are tall triangles carried high, and that
   difference is one of the few head cues that survives the party camera.

3. **The Wolverine's tail was a beaver's paddle.** It held 0.13 of girth as far
   out as the third bone and ran the whole donor tail chain, reaching sixty per
   cent of the body length behind an animal whose own tail is nearer a quarter
   of it. The girth now falls away by the third bone and the rest of the chain
   carries a wisp.

4. **The Shadow Mastiff was a long low tube with a hump.** It ran nearly the
   same girth from rump to shoulder and lifted the withers ring by 0.055. The
   loin now draws in well below the chest, which is the single proportion that
   reads as a deep-chested running dog, and its tail keeps its reach - the
   printed tail slap needs that - while taking a hound's taper instead of a
   paddle's constant mass.

5. **The Poison Frog's body dived in the middle.** Its widest ring was placed
   at the donor's stomach bone, which hangs 0.40 below the spine it belongs to,
   so the body fell away between its two ends and rendered as a creased wedge.
   The ring now sits on the spine, where the body actually is, and keeps its
   weight on the stomach bone so the belly still deforms with the donor's own
   belly joint.

6. **The Poison Frog's eyes hung in the air.** They were placed at the donor's
   eyelid joints, which sit where the Giant Poisonous Frog's eyes are rather
   than where this smaller head is, so both floated clear of the skull and one
   was plainly detached in a review frame. The skull is now an explicit
   flattened dome - a frog's head is far wider than it is tall, and a circular
   tube wide enough to carry the eyes would have been a ball - and the eyes sit
   half buried in it, breaking its outline at the top and the sides. They keep
   their eyelid-joint weights, so the donor's blink still drives them. The
   Wolverine's and the Mastiff's eyes had the same fault from the shared eye
   helper, which measures its spread from the skull's origin and so leaves the
   beads inside any head wider than the creatures it was written for; all three
   are now placed against their own skull.

7. **The Shadow Mastiff had a glowing blue ring around its neck.** The atlas
   regions tile edge to edge and a tube's first and last rings land exactly on
   its region's u edges, so a bilinear sample there reads half its colour from
   the neighbouring region. The body region sits directly beside the crest
   region, and this is the first creature whose crest is deliberately bright: a
   pale blue, so that its eyes are the one light feature on a shadow-black
   coat. The bleed showed as a ring at the neck and a smear across the muzzle,
   and the nose pad - which was also painted from the crest - rendered white.
   Every Sprint 13 coordinate is now inset two per cent inside its own region,
   which removes the bleed for good at the cost of a border of texels nothing
   was sampling, and the nose pad moves to the jaw region, which is as dark as
   the rest of the head.

One rest-pose observation is deliberately not treated as a geometry defect, on
the same reasoning Sprint 12 recorded for the Hyena. The Poison Frog's hind
limbs sprawl backwards at full extension and the body rides clear of the floor
behind the forelimbs. That is the Giant Poisonous Frog donor's own bind pose -
a frog rig is bound with the legs extended - and skinning requires the mesh to
follow the bind pose it is bound to, so authoring the legs folded would make
them swing wildly the moment the donor animates. A bind pose is not a play
pose; live idle, hop and attack review has to settle it, and a still cannot.

Review frames from this pass are machine-local under the session scratchpad.

Reviewed shipped SHA-256 values:

- `wolverine-mesh.json`: `e1e97db7ab271c3705472a78262fc3306a1515ef174c39e7d6899c28bad54fc6`
- `wolverine-albedo.png`: `7a0ef9388bab87c4995dbaf7b8bf6ae83436c91c9652bb0c375da273b6035fe2`
- `shadow-mastiff-mesh.json`: `61755b4c1853942b73e0f2666ac246bf5d77ef9b7c6800ca30ea2f7e56a13bd3`
- `shadow-mastiff-albedo.png`: `a29c2bdce8e4c640eef8d04cb2ec45569816c8192a0da29e7674111e5cc785b4`
- `poisonous-frog-mesh.json`: `2b37e38a4e6fb7613a06ab69f161273221a05c50105691a9dcdfd58899f58ed6`
- `poisonous-frog-albedo.png`: `89ac9f05d76582d86b7a32f16f9afada165e1f37cd4565290313d001b25f8a21`
