# Sprint 16 Dire Crocodile summon-choice icon

The Dire Crocodile source is an original 1254 px square RGBA render produced by
this project's own Blender script,
`tools/render_creature_icon.py --creature dire-crocodile`, the same procedural
route the Sprint 14 and 15 insects used. Everything in the frame is built from
metaballs and mesh primitives with procedural materials, lit with a warm key, a
cool rim and a low fill in front of a radial backdrop inside the roster's dark
bronze ring. No Kingmaker pixels, downloaded model, texture or
generative-model output is an input.

Source `sources/dire-crocodile.png` has SHA-256
`bb5f282d769cdc7e1be047ba7d2161529f52443abc0550d1f8d1d37aacd6ffa2`.
`tools/New-ExpandedSummoningIcons.ps1 -OnlyKeys dire-crocodile` produced the
deterministic 128 px RGBA export
`assets/game/icons/expanded-summoning/dire-crocodile.png`, SHA-256
`77a7395a6fe17e1528b8a8778fa28f77f1c02bb1ad50ab92888688625666116b`.
Technical source inspection and export checks pass. Live menu review and owner
visual approval remain pending, and every Dire Crocodile placement is withheld
anyway until the creature's own guarded review passes.

## Four renders, and what each one was for

The icon is recorded here in the state it reached rather than as a single
result, because three of the four passes existed to fix something that could
only be seen by looking at the render.

1. **Structure.** The tail was four separate metaball elements spaced further
   apart than their own radii, so they never fused: the render showed a row of
   detached lumps trailing out past the bronze ring. The dorsal scutes were
   tall cones, which made the back read as a stegosaurus, and the legs were
   thin sticks that floated clear of the body.
2. **Rebuild.** The tail became one continuous chain that fuses along its
   length and stops inside the ring; the scutes became low flattened plates
   hugging the spine; the legs became short, thick and rooted inside the hull
   with wide feet. The eyes moved from the top of the skull - where two yellow
   dots had read as a frog - to the sides of a raised brow, so in near-profile
   the near eye carries the frame.
3. **Scale.** The animal filled only about two thirds of the ring's width and a
   narrow band of its height, which at 128 pixels is a dark smear rather than a
   creature. The camera came in, the head grew the way a portrait's head does,
   and the tail ended sooner at a radius that still fuses.
4. **Composition.** The tail reached the ring, and the roster's framing keeps
   the subject inside it, so the camera pulled back slightly and the outer rank
   of scutes moved up from mid-flank, where it had read as scattered pebbles,
   to sit beside the inner rank.

## Why profile, and why it matters here

The creature is laid along the view's horizontal with its head to the left,
which is the same decision the Giant Stag Beetle needed and for the same
reason: a feature that projects forward cannot be read down its own length. A
crocodile seen head-on is a wedge; seen across, it is a jaw.

The sprint's order requires that this creature not be a lizard silhouette
disguised by texture, so the things that separate the two are built rather than
painted - a broad flat snout instead of a tapering muzzle, a jaw line long
enough to carry teeth along it, raised brow and nostril bosses, two ranks of
keeled dorsal scutes, a deep keeled tail, and limbs sprawling out to the sides
rather than tucked underneath.

Exact concept record (`prompts/icon-prompts.json`, generator
`blender-procedural`):

> A dire crocodile in strict profile with its head to the left, an enormous
> armored crocodilian with a long low flat snout, heavy toothed jaws held
> slightly open, raised brow and nostril bosses, two ranks of low keeled dorsal
> scutes running from the shoulders down a thick tapering keeled tail, and four
> short thick sprawling legs with wide feet; cool damp green-grey radial
> backdrop, the whole animal inside the bronze ring, read in profile because a
> crocodile seen down its own length is a wedge and seen across it is a jaw;
> visibly a crocodilian rather than a monitor lizard, and visibly far larger
> and heavier than the ordinary Crocodile beside it.
