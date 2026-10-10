# Original Sprint 20 Giant Scorpion

The Giant Scorpion's geometry, UVs and painting are original project-owned
procedural work. No native vertex, triangle, texture, material or animation
is copied. The shipped mesh uses only bone names and weights from the live
Giant Spider renderer; runtime supplies that donor's own bind poses during
the existing instance-local renderer swap.

The geometry-free request-local capture remains outside Git and the package.
Use Blender 4.5.10 LTS. Run the painter first, because the generator reads
the painting's bytes into the shipped manifest:

```
blender --background --factory-startup --python paint_scorpion_albedo.py -- \
    --out assets/sprint20-arachnids/giant-scorpion-albedo.png

blender --background --factory-startup --python generate_scorpion.py -- \
    --capture <private census capture> \
    --albedo assets/sprint20-arachnids/giant-scorpion-albedo.png \
    --mesh-data assets/sprint20-arachnids/giant-scorpion-mesh.json \
    --report <private report.json> --blend-out <private source.blend> \
    --fbx-out <private preview.fbx>
```

Blender and FBX previews, reports, captures and review renders stay
machine-local. The shipped files contain original geometry, painting, UVs,
weights, reviewed bone names and the albedo hash only.

## Why the Giant Spider carries a scorpion

The Sprint 14 census established that Kingmaker has no scorpion and that the
Giant Spider is the only compact many-legged arthropod in the game. Sprint 20
re-measured that rather than inheriting it, because the Sprint 14 capture did
not survive and because a tail authored onto a chain nobody looked at is a
guess. The census passed as
`20261010T1328444995657Z-observe-expanded-summoning-arachnid-census`, and
three independent Giant Spider prefabs agreed on the frame.

Three measurements make this the best fit this project has found for a
borrowed rig:

- **Eight legs for eight legs.** The fourth chain is a complete leg here -
  upper, lower and foot on both sides - where the Sprint 14 ants leave it
  empty and the beetles hang wings on it. This is the first creature in the
  project to weight all four chains a side as real legs, and
  `test_it_walks_on_all_eight_legs` proves it from the built geometry rather
  than from a manifest field.
- **A scorpion's claws ARE its pedipalps**, and the rig has a seven-bone
  pedipalp chain a side that already curls forward and down. The chelae are
  anatomy here, not substitution.
- **The abdomen chain runs backward from the body**, which is where a
  metasoma attaches.

Three guarded transactions were spent before the census passed, and all three
failures were defects in the census rather than in the game: an assertion that
forbade what it should have reported, a discovery term short enough to match
adamANTine and peasANT, and a scan that counted this sprint's own registered
unit as an installed scorpion.

## METASOMA_DRIVEN_BY_A_TWO_BONE_CHAIN

The measured limitation, and the number that would have been guessed wrong.

A scorpion's metasoma is five segments and a telson. This rig's abdomen chain
is `Tail1_M` and `Tail3_M`, with `UpperTorso` between them - three driver
bones, all level at about z 0.7, running backward. Every other tail this
project has authored runs `Tail0_M` through `Tail4_M`, so five was the natural
assumption and it is wrong here.

The tail is therefore authored arching up and forward **over** a level chain
and weighted along it, so the whole metasoma sways as the abdomen sways rather
than articulating segment by segment, and the sting cannot be driven as a
strike of its own. No bone is invented and no rig is built; that would be the
general arbitrary-limb and animation system this sprint must not build.

`test_the_metasoma_rides_the_abdomen_chain` holds the limitation to its
measurement: it asserts the arch is carried by the abdomen chain and by
nothing else, and that `Tail0_M`, `Tail2_M` and `Tail4_M` genuinely do not
exist on this rig. If one ever appears, the fixture fails and the tail is
rebuilt rather than left claiming a constraint it no longer has.

## The chelae extend past their bones, deliberately

The pedipalp chain spans about a fifth of a leg's length, and a scorpion's
chelae are its largest limbs. The first build followed the bones exactly and
the claws rendered as two nubs beside the mouth. Geometry does not have to
stay inside the bones that drive it - the Sprint 14 beetle wings make the same
point - so the arm is carried forward past the chain's end along its own
direction and the hand is built there, still weighted to the pedipalps that
move it. `test_the_chelae_reach_in_front_of_the_body` checks that it extends
the right way rather than merely that it extends.

## Review

`render_scorpion_review.py` delegates to the Sprint 14 reviewer rather than
copying it: one rig family, one reviewer. It adds two poses of its own.
`tail-sway` swings the abdomen chain and must carry the entire arched tail,
the telson and the sting - a render where the body turns and the tail stands
still means the weighting is wrong and the limitation is worse than recorded.
`claws-forward` is the printed routine's opening, two claws seizing one
target.

These are synthetic offline poses. They prove geometry and skinning; they
prove nothing about runtime timing, contact or animation.

## Palette

Deliberately not the Sprint 14 insects'. Those share this rig and a player
chooses between them in one menu: the ants are red-brown and the beetles
near-black, so the scorpion is pale warm sand. The one saturated thing on the
creature is the bead of venom at the sting's tip, because the sting is its
printed attack and it is what a player should see first.
