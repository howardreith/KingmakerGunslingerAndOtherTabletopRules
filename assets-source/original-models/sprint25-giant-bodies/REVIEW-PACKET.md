# Sprint 25 review packet — weighted prototype, round 3

One packet, one revision, correctly identified views. It replaces the earlier
graybox packets; `runtime-evidence/sprint25-graybox/` and `…-graybox-r2/` are
superseded.

**What changed since round 2**, which returned CHANGES_REQUIRED with ten
defects. Six were acted on and re-measured; the rest are listed as open with
their measurements. This round also carries one new experiment that the owner
asked for: the Brachiosaurus neck's pivot problem, attacked directly instead
of by redistributing weights.

The settled seam comparisons (`pin`, `blend`, `graded`, `chained`) are
**measured but no longer rendered**. Their numbers are in the report and have
not changed in kind; re-rendering four whole bodies of failed comparisons each
round is churn, not review.

## Source revisions

| what | revision |
| --- | --- |
| branch | `codex/expanded-summoning-sprint25-brachiosaurus-elephant` |
| `prototype_giant_bodies.py` | SHA-256 `62b45cc0e25d9938…` |
| `graybox_giant_bodies.py` (bodies and influences) | SHA-256 `11f227e3399629f8…` |
| `neck_retarget_experiment.py` | SHA-256 `dd30e960a4a3179a…` |
| donor census | `20261016T1351412649785Z-…-donor-census/sprint25-donor-census.json`, SHA-256 `e2ce2a776dc06fdb…` |
| donor | Mastodon prefab `fe9f25fe62e0f3b49a13910a996470b2`, 40 joints |
| renders | `C:/Dev/KingmakerGunslingerLab/runtime-evidence/sprint25-weighted-prototype/` |
| the experiment | `…/sprint25-weighted-prototype/neck-retarget/` |
| measurements | `prototype-report.json` and `neck-retarget/neck-retarget-report.json` |

## How to read these images

- **Camera.** The creature faces **+Y**. `side` stands at +X (its **left**),
  so it faces image-right. `front` at +Y, `rear` at −Y, `party` is the
  three-quarter. `closeup` frames the head, shoulder and neck base.
- **Ground.** Drawn at the donor's own sole plane, `z = 0.0352`, which every
  sole ring is flattened onto. "Planted" means a measured 0.000 above it.
- **Faceting is expected.** 624 and 808 vertices. Judge joins, deformation,
  proportion and ground contact.
- **Every pose is an authored probe** named after a donor clip. The census
  captured clip names and event times, never curves. `walk-swing` is named for
  what it does: joints only, no root motion and no IK, so stance feet do not
  stay down.
- The **rigid neck is a control, not a proposed design.** Please do not read
  its stability under the chain probe as quality: the probe moves joints that
  variant is not weighted to.

## A. The neck pivot experiment — the one new question

1. `neck-retarget/rigid-control-chain-probe-16-side.png`
2. `neck-retarget/retargeted-chain-probe-16-side.png`
3. `neck-retarget/retargeted-chain-probe-08-side.png`
4. `neck-retarget/rigid-control-shoulder-swing-side.png`
5. `neck-retarget/retargeted-shoulder-swing-side.png`

Judge: 1 against 2 is the experiment. Does the retargeted neck bend like a
neck, or merely differently? 3 is the same mapping at the amplitude the skin
holds within tolerance. 4 and 5 are a motion both variants answer to, included
so neither can pass by ignoring a probe — they should look the same.

## B. Head, shoulder and the join, close

6. `brachiosaurus-authored-upright-rigid-closeup-side.png`
7. `elephant-bind-faithful-closeup-side.png`
8. `elephant-bind-faithful-closeup-party.png`

Judge: the shoulder mass that now bridges the neck and the body; whether the
neck's crease reads as a defect; whether the Brachiosaurus head reads as a
head at all; whether the Elephant's ears read as ears; tusk placement.

## C. Deformation that was previously absent

9. `brachiosaurus-authored-upright-rigid-knee-flex-side.png`
10. `elephant-bind-faithful-knee-flex-side.png`
11. `elephant-bind-faithful-slam-foreleg-side.png`

Judge: the shin and carpus now deform — rings exist on `frontKnee_*` and
`frontAnkle_*`, where before only the foot block moved. Is the bend plausible
for a Huge quadruped, and does the slam read as a stomp yet?

## D. Bodies and silhouette

12. `brachiosaurus-authored-upright-rigid-bind-side.png`
13. `brachiosaurus-authored-upright-rigid-bind-rear.png`
14. `brachiosaurus-authored-upright-rigid-bind-party.png`
15. `elephant-bind-faithful-bind-side.png`
16. `elephant-bind-faithful-bind-rear.png`
17. `elephant-bind-faithful-bind-party.png`

Judge: species identity, proportion, the tail, and the rear views — which no
previous packet provided, and which are where the tail/hip join and the dorsal
UV pole can be seen.

## E. Motion and texture

18. `elephant-bind-faithful-trunk-curl-8-side.png` — in tolerance (1.18)
19. `elephant-bind-faithful-trunk-curl-side.png` — 16°/joint, 1.37, which the
    generator scores out of tolerance
20. `brachiosaurus-authored-upright-rigid-gore-strike-side.png`
21. `elephant-bind-faithful-textured-side.png`
22. `brachiosaurus-authored-upright-rigid-textured-side.png`

## Known open, with measurements — do not spend the review re-finding these

- the trunk still reads as a fifth leg and passes within 0.060 units of the
  foreleg; its reaching near the ground is faithful to the donor, so the fix
  is shape, not length;
- the Brachiosaurus neck creases where the donor's 44.9° rest turn is carried
  through the re-aim, and the head is a terminal cap, not authored skull;
- the shoulder mass holds the join but these are still placed primitives, not
  one unioned surface;
- the tail is about a quarter of the neck's length;
- a UV pole whorl on the dorsal midline and a texel-density step at the
  shoulder;
- the ears are flat plates;
- the gore pose tucks the trunk between the forelegs;
- `walk-swing` does not hold stance contact, by construction.

## Not provided

No native screenshots, no in-game frames, no animation curves, no death or
interruption phases, no turn, no icon-scale render, no textured render of a
posed state, no top-down view of the contact poses. The product candidate is
not valid while the painted icon is missing, and native animation
qualification has not been performed.
