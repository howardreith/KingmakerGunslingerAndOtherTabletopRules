# Original Sprint 12 quadrupeds

Dire Rat, Hyena and Goblin Dog geometry, UVs and paintings are original
project-owned procedural work. No native vertices, triangles, texture,
material or animation is copied. Dog deliberately retains its native Dog
visual. The three original meshes use only bone names and weights from the
respective live Dog, Wolf and Worg renderer; runtime supplies that individual
donor's own bind poses during the existing instance-local renderer swap.

The geometry-free request-local captures remain outside Git and the package.
Use Blender 4.5.10 LTS. For each key, run
`paint_sprint12_quadruped_albedo.py -- --kind <key> --out
assets/sprint12-quadrupeds/<key>-albedo.png`, then run
`generate_sprint12_quadrupeds.py -- --kind <key> --capture <private capture>
--albedo assets/sprint12-quadrupeds/<key>-albedo.png --mesh-data
assets/sprint12-quadrupeds/<key>-mesh.json --report <private report.json>
--blend-out <private source.blend> --fbx-out <private preview.fbx>`.

Dire Rat uses the Dog capture; Hyena uses Wolf; Goblin Dog uses Worg. The
generator rejects an incomplete, repeated, multi-renderer or wrong-family
capture. Private reports record the exact capture hash. Blender/FBX previews,
reports, captures and review renders remain machine-local. The shipped files
contain original geometry, painting, UVs, weights, reviewed bone names and the
albedo hash only.

The expected visual reads are: a compact coarse-furred rat with long naked
tail, pointed muzzle, round ears and whiskers; a spotted, high-shouldered hyena
with a dark mane, powerful forequarters and blunt muzzle; and a starved,
long-legged hairless rodent with a flat rat face, beady eyes, protruding teeth,
mange and a whip tail. Offline rest-pose renders are an authoring check. Live
idle, movement, attack, hit, death, quantity, party-camera, clipping, targeting,
fallback and cleanup review remain required before publication.

## Hyena repair, 2026-10-01

The first internal rest-pose review of the shipped meshes found the Dire Rat
and Goblin Dog reading as their species and the Hyena not reading as one. Three
geometry defects, all reproducible in every view and none of them a matter of
taste:

1. The head was as wide as the torso carrying it - a 0.275 skull against a 0.36
   body - and reached 0.34 forward, so the silhouette was head-dominated. The
   skull is now 0.235 and the muzzle 0.29 forward, with the jaw, nose, ears and
   eye spread brought in to match. The shoulders are the widest point again,
   which is what a hyena's profile depends on.
2. The mane ran at a fixed height that ignored the body radius beneath it, so
   it was buried at the spine, exactly bisected at the upper torso and floating
   at the neck. It rendered as a flat dark sliver lying on the back. Each
   station now clears its own local radius by a small margin with the bulk
   seated inside the body, and the ridge peaks over the withers and tapers both
   ways.
3. The tail was thicker in the middle than at its root and was painted from the
   body region, so it read as a broad spotted paddle. It now tapers from root
   to tip, sits in the limb region, and extends past the last bone so the tip
   is not a flat cut face. It keeps more volume than the Goblin Dog's whip tail
   because a hyena's tail is short and brushy.

After the repair the species reads: spotted coat, dark dorsal mane, large
rounded ears, blunt heavy muzzle, shoulders higher than hips.

One rest-pose observation is deliberately not treated as a geometry defect. The
Hyena's hind limbs stack at sharp angles in the bind pose and one hind paw sits
off the floor. The same `canine_leg` code produces ordinary legs on the Dog and
Worg donors, so this is the Wolf donor's own bind pose rather than project
geometry, and a bind pose is not a play pose. Live idle, walk and attack review
has to settle it; a still cannot.

Current review frames are under `artifacts/authoring/sprint12-quadrupeds/review-20261001/`
(machine-local). The earlier `renders/` sheet is stale: ten of its twelve views
predate the meshes that actually shipped, so it must not be read as evidence.

Reviewed shipped SHA-256 values:

- `dire-rat-mesh.json`: `2ee73bcf0ab4cdf0d275fb64764dea96107669cef07ee3b0c62a6f5228f1633a`
- `dire-rat-albedo.png`: `fb618f5d32ee380c90c872df04de47f65652ac2ae7d803bae8aba67f43689030`
- `hyena-mesh.json`: `0217f8d599480e5aaed6f1b6df9736b64609298664f3732271bb39c08596ac48`
- `hyena-albedo.png`: `15927eb690a64bdd22e9f6985ddfe29f72345ad0f177c11084187f0c6745ae50`
- `goblin-dog-mesh.json`: `0974e179edf83a61067517a5c343ceeae2753cb67e76352c8bff1dce91473a4a`
- `goblin-dog-albedo.png`: `b7fcc93fd653684dac96268b1626a04590a1e5ccbe74851847c7b9c8d3de626a`
