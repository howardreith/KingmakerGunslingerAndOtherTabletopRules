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

Reviewed shipped SHA-256 values:

- `dire-rat-mesh.json`: `2ee73bcf0ab4cdf0d275fb64764dea96107669cef07ee3b0c62a6f5228f1633a`
- `dire-rat-albedo.png`: `fb618f5d32ee380c90c872df04de47f65652ac2ae7d803bae8aba67f43689030`
- `hyena-mesh.json`: `1c68a8361149309ca374897761406dc98b81f13873a726e9b7d18d7dc1399d0f`
- `hyena-albedo.png`: `15927eb690a64bdd22e9f6985ddfe29f72345ad0f177c11084187f0c6745ae50`
- `goblin-dog-mesh.json`: `0974e179edf83a61067517a5c343ceeae2753cb67e76352c8bff1dce91473a4a`
- `goblin-dog-albedo.png`: `b7fcc93fd653684dac96268b1626a04590a1e5ccbe74851847c7b9c8d3de626a`
