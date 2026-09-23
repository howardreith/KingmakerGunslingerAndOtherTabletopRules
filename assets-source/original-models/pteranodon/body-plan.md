# Pteranodon body plan, in the donor's bind frame

Axes are the donor renderer's: **+X** to the creature's left, **+Y** up, **-Z**
forward. Every coordinate below is a bind-pose bone head from
`rig.measured.json`, or a point derived from one.

## What the rig gives us

| Bone | Bind head | Role in the pterosaur |
|---|---|---|
| `LowerTorso` | (0.000, 1.189, 0.272) | hips, mesh root |
| `UpperTorso` | (0.000, 1.574, -0.263) | chest |
| `Neck` | (0.000, 1.822, -0.717) | neck |
| `Head` | (-0.005, 1.943, -1.089) | skull |
| `Jaw` -> `Jaw_end` | (0.000, 1.840, -1.156) -> (0.000, 1.694, -1.412) | lower beak |
| `Tail` -> `Tail_end` | (-0.001, 0.998, 0.620) -> (-0.002, 0.013, 1.524) | eagle tail fan |
| `Tail_L`/`Tail_R` | (±0.150, 0.977, 0.620) | outer tail feathers |
| `L/R_Leg0_Upper` | (±0.267, 0.993, 0.080) | thigh |
| `L/R_Leg0_Lower` | (±0.272, 0.487, 0.090) | shank |
| `L/R_Foot0` | (±0.288, 0.128, -0.089) | ankle, membrane anchor |
| `L/R_Finger_1..4` | around the foot | toes |
| wing chain | see the membrane generator | |

## Where the donor's anatomy and a pterosaur's disagree

Three places, and each needs a deliberate decision rather than a shrug.

**The beak.** The donor's head ends at `Jaw_end`, z = -1.412. A Pteranodon's
toothless beak is roughly as long again as its skull. The mesh therefore
extends forward of the last bone, to about z = -2.25, with the upper beak
weighted to `Head` and the lower to `Jaw`. That is deliberate: when the bite
animation opens the jaw, a long beak *should* swing, and weighting it anywhere
else would make the bite read as a head twitch.

**The crest.** There is no bone for it. A Pteranodon's crest sweeps back and up
from the skull, so it is authored as part of the head and weighted entirely to
`Head`. It will move exactly with the skull, which is correct - a crest is bone,
not soft tissue.

**The tail.** The donor's tail is an eagle's: `Tail` runs down and back 1.17
units to `Tail_end`, with `Tail_L`/`Tail_R` fanning a feather spread nearly 1.5
units wide. A Pteranodon has a short stub. The mesh takes only the first ~35 % of
the `Tail` bone and leaves `Tail_end`, `Tail_L` and `Tail_R` with no geometry at
all. Those bones will still animate; nothing will be attached to them, which is
exactly what is wanted. This is the one place where the donor skeleton is
carrying equipment the creature does not have, and the honest answer is to not
build it rather than to build a small fan and hope it reads as a tail.

## Proportions that follow

- Wingspan 8.641 (fixed by the rig).
- Nose to tail: beak tip z = -2.25 to tail stub z ≈ +1.03, so ≈ 3.28.
- Span-to-length ratio ≈ 2.63. A real Pteranodon is roughly 2.4-2.8 depending on
  how much beak is counted, so the donor rig lands in the right envelope without
  being stretched.
- The view multiplier `SummonViewScaleCatalog` applies to this creature is 0.82
  and is unchanged, so the replacement is authored to the donor's own envelope
  and scaled by the same factor the donor body was.

## Build order

1. torso tube: `LowerTorso` -> `UpperTorso` -> `Neck`, radius 0.30 -> 0.26 -> 0.15
2. neck tube: `Neck` -> `Head`, radius 0.15 -> 0.13
3. skull: a short box-ish hull around `Head`
4. upper beak: `Head` -> tip, tapering to a point, weighted to `Head`
5. lower beak: `Jaw` -> tip, slightly shorter, weighted to `Jaw`
6. crest: a swept blade from the back of the skull, weighted to `Head`
7. legs: `Leg0_Upper` -> `Leg0_Lower` -> `Foot0`, radius 0.10 -> 0.07 -> 0.05
8. toes: short tubes along each `Finger_n_1` -> `Finger_n_2` -> `_end`
9. tail stub: `Tail` for 35 % of its length, radius 0.12 -> 0.05
10. membranes: unchanged from the membrane prototype, both wings

## Weights

Every vertex is weighted to the bone whose segment it was generated from, with a
blend to the next bone across the last 25 % of each segment so joints do not
crease. Maximum three influences, inside Unity's four.
