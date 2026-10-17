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

## Dire Rat refinement, 2026-10-16 (VIS-005)

The owner's review found the Dire Rat's head and legs detached-looking, its
legs passing through the body and its colouring odd. The shipped mesh rendered
inside out (see `../continuous-bodies/SOURCE.md`): 12 of its 23 parts faced
inward, 14 had caps wound against their walls, and every normal opposed its
winding. The torso and head tubes also met at `Neck1` with different weights,
so a head turn opened a seam, and the separate leg tubes swung through the
body tube.

The Dire Rat is now built by `build_dire_rat.py` as one continuous skinned
surface on the same Dog frame: an arched trunk hung below the Dog's tall spine
so only short lower legs show, a skull carried below and ahead of the high
head pivot, and a rigid lower jaw, ears, eyes, nose, incisors, whiskers and
naked tail. `generate_sprint12_quadrupeds.py` keeps the Hyena and Goblin Dog
and refuses `--kind dire-rat`.

- Capture: `sprint12-dog-bind-rig.json` from guarded run
  `20261017T0225090480213Z-disposable-expanded-summoning-native-profile` on
  master `2eed3944f6` (SHA-256
  `c651dda7aab132adfe9354d72e4e113613ca04057737b7a30e29bd680ecf4270`).
- `dire-rat-mesh.json`: `cdc262ab43da10301e5f81d01fb94f7c0ee74d621e03fb4c444983341073fbf4`
  (4,899 vertices, 6,452 triangles, 16 closed outward parts, the reviewed 32
  Dog controls).
- `dire-rat-albedo.png`: `746cb3c4b0c417430fa22ffa2f568bd0571080fe9e7ce72b090577af7356f422`
  (1024 x 1024 RGB).
- Superseded: mesh `2ee73bcf…1633a`, albedo `fb618f5d…89030` (listed above).

A read-only visual review of the first rebuild asked for thicker forelegs, a
jaw that follows the head and opens, a tail that does not curl back over the
rump, a darker underjaw and pink only on the naked tail. Its frozen-jaw and
loose-claw findings came from the offline preview, which ignored vertex groups
of non-deform bones; `continuous_body.heat_weights` now restores every bound
control as a deform bone, so the saved source poses with the shipped weights.
The hashes above are the revised build.

## Hyena and Goblin Dog refinement, 2026-10-17 (VIS-010, VIS-009)

Both first exports rendered inside out (16 and 26 inward parts). The owner
reported for the Hyena bulbous proportions and a transparent, open-looking
body; for the Goblin Dog a detached-looking head and legs, coarse proportions
and poor recognition. Both are now built by `build_worg_frame.py`, which also
builds the Sprint 13 Wolverine and Shadow Mastiff: the four ride the Wolf and
Worg frames, which share the same 36 controls, and each binds the same
reviewed 34 (every one but the collar helpers). Each is one continuous
surface: a torso along the animal's own topline, united and relaxed on a
coarser union, a neck to the skull and muzzle, legs along the measured bones
(the frames' bind pose holds the hind legs stretched back), and a rigid lower
jaw, ears, eyes, nose, canines and claws. Rules specific to the frame, each
commented in the builder: the torso follows the spine rather than the buried
upper limb bones; `Head` owns the skull, `jaw_woo_up` the nose,
`jaw_woo_up_add` the upper lip and `withers` the top of the shoulders; the
rigid lower jaw grades from `jaw` to `jaw_woo_down`; a tail held along the
frame ends on `tail_04`, and a short tail hangs from the tailhead weighted
down the chain.

- **Hyena** (Wolf frame): a robust body sloping from high shoulders to low
  hips, a broad heavy head with a blunt muzzle, round ears, a short hanging
  tail with a dark tuft, and a sandy coat with dark spots, a dark mane line,
  a dark muzzle and darker legs.
- **Goblin Dog** (Worg frame): a thin, hunched, hairless body, an oversized
  head with a long muzzle and protruding canines, large splayed ears, bulging
  yellow eyes, a thin rat-like tail and a mottled grey-pink hide with dark
  patches and raw sores.

- Captures: `sprint12-wolf-bind-rig.json` (SHA-256
  `7680509355203adc515dcbf7cf2f3eac1344e0434d51ab70b77982e708bb4c7c`) and
  `sprint12-worg-bind-rig.json` (SHA-256
  `b6c2d2cf97ff05b5b6f80f6d5a14b9a04de92f29d649440ca9ba03e3a9692fa8`), both
  from guarded run `20261017T0225090480213Z-disposable-expanded-summoning-native-profile`
  on master `2eed3944f6`.
- `hyena-mesh.json`: `bb02ff0e93fef0de73100d0dac0769cf36f461fdadf166eada09a40059bb07ef`
  (6,088 vertices, 8,122 triangles, 28 closed outward parts, 34 controls);
  albedo `0b92a99aeb02d3381c923da2a2eff3b093b41706a5f1570b39575face5a7d404`.
- `goblin-dog-mesh.json`: `fa07b94074272d45e2d014573e5d2feeb29f89c1bc58777d0421a9a31049710b`
  (6,052 vertices, 7,644 triangles, 27 closed outward parts, 34 controls);
  albedo `dc8a53f09f0ce21a601bfc8773945050ed4389dfebd51b5109827c38c255789c`.
- Superseded: the Hyena and Goblin Dog rows listed above.
  `generate_sprint12_quadrupeds.py` now refuses all three kinds.
