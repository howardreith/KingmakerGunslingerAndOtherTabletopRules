# Sprint 25 review packet — weighted motion prototype

One packet, one revision, correctly identified views. It replaces the earlier
graybox packets; `runtime-evidence/sprint25-graybox/` and `…-graybox-r2/` are
superseded and should not be mixed with these renders.

**Round 2.** Round 1 of this packet was reviewed independently and returned
CHANGES_REQUIRED with ten defects. Three were corrections to the record and
three were authoring defects; all six were acted on and re-measured, and
`WEIGHTED-PROTOTYPE.md` records what changed, what was measured after, and
what was handed to the final geometry pass instead. These renders are the
corrected set.

## Source revisions

| what | revision |
| --- | --- |
| branch / parent commit | `codex/expanded-summoning-sprint25-brachiosaurus-elephant` @ `4171847f` |
| `prototype_giant_bodies.py` | SHA-256 `d940c34a958c7e50…` |
| `graybox_giant_bodies.py` (bodies and influences) | SHA-256 `302b5aaab3636d07…` |
| donor census | `20261016T1351412649785Z-observe-expanded-summoning-sprint25-donor-census/sprint25-donor-census.json`, SHA-256 `e2ce2a776dc06fdb…` |
| donor | Mastodon prefab `fe9f25fe62e0f3b49a13910a996470b2`, 40 joints |
| albedos | `assets/sprint25-giant-bodies/{brachiosaurus,elephant}-albedo.png` |
| renders | `C:/Dev/KingmakerGunslingerLab/runtime-evidence/sprint25-weighted-prototype/` |
| measurements | `prototype-report.json` in that directory |

Three guarded census runs exist for this donor and their bind frames are
identical — same 40 joints, same bind-position hash `0ac4268cd15e52f2`. The
one named above is the capture of record in the sprint ledger.

## How to read these images

- **Camera.** The conversion leaves the creature facing **+Y**. `side` stands
  at +X (the creature's **left**) and the creature faces **image-right**.
  `front` stands at +Y looking back along −Y. `party` is the three-quarter
  view.
- **Ground.** The plane is drawn at the donor's own sole plane, `z = 0.0352`,
  which is where every sole ring is flattened to. "Planted" means a measured
  0.000 above that plane.
- **Faceting is expected.** 486 (Brachiosaurus) and 670 (Elephant) vertices.
  Judge joins, deformation and proportion, not polygon count.
- **Every pose is an authored probe**, named after a donor clip. The census
  captured clip names and event times, never curves. These are **not** the
  native clips. `walk-swing` is named for what it does: it rotates joints
  only, with no root motion and no IK, so stance feet do not stay down.
- **Weight renders** colour each vertex by its dominant bone; the colour per
  bone name is stable across every image here.

## Assigned images

### A. Does the skin belong to the right bones

1. `elephant-bind-faithful-weights-side.png`
2. `brachiosaurus-authored-upright-rigid-weights-side.png`

Judge: whether each region is driven by a plausible bone, and whether the
Brachiosaurus neck being a **single flat colour** (one rigid piece on `Head`)
is acceptable, or will read as a stiff neck in play.

### B. Brachiosaurus — the neck decision

3. `brachiosaurus-bind-faithful-bind-side.png` — the neck authored on the
   donor chain's own bind path. Does this read as a sauropod at all?
4. `brachiosaurus-authored-upright-rigid-bind-side.png` — the shipped
   silhouette.
5. `brachiosaurus-authored-upright-chain-curl-probe-side.png` — the same
   upright body with full donor-chain influence under a 16°/joint curl.
   Measured 3.46 visible stretch, 10.51 units of displacement.
6. `brachiosaurus-authored-upright-rigid-chain-curl-probe-side.png` — the same
   probe with the chain influence removed. Measured 1.01 and 1.24.

Judge: 5 against 6 is the decision. Also judge the **wedge at the shoulder**
in 4 and 6 — the protruding anchor cap round 1 found there is gone (the ring
is now fully inside the hull in every pose), so what is left is the join
itself.

### C. Brachiosaurus — motion

7. `brachiosaurus-authored-upright-rigid-gore-strike-side.png` — Neck −24°,
   Head −16°; skull travels 3.24 units.
8. `brachiosaurus-authored-upright-rigid-walk-swing-side.png` — judge the limb
   swing, not contact; the feet leave the plane by −0.04 to +0.20 and the
   record says so.
9. `brachiosaurus-authored-upright-rigid-knee-flex-side.png` — bends
   `frontKnee_L` and `frontAnkle_L`, two donor joints that **no vertex
   weights**. Judge the shin-to-foot seam specifically.

### D. Elephant — trunk, tusks, contacts

10. `elephant-bind-faithful-bind-side.png`
11. `elephant-bind-faithful-trunk-curl-8-side.png` — 8°/joint, **within the
    skin tolerance** (1.18). This is the passing frame of the defining motion.
12. `elephant-bind-faithful-trunk-curl-side.png` — 16°/joint, 1.37, which the
    generator scores **out** of tolerance. Judge 11 and 12 together: how much
    curl does a trunk need, and does the authored skin reach it?
13. `elephant-bind-faithful-gore-strike-side.png`
14. `elephant-bind-faithful-slam-foreleg-side.png` — the raised forefoot is
    0.741 above the sole plane; the other three measure 0.000.
15. `elephant-bind-faithful-bind-front.png` — lateral symmetry, ear placement,
    and whether the ears read as ears.

### E. Texture on the real UVs

16. `elephant-bind-faithful-textured-side.png`
17. `brachiosaurus-authored-upright-rigid-textured-side.png`

Judge: region placement, stretch, seams, and whether the surface scale suits a
Huge creature at party-camera distance.

### F. Party-camera readability

18. `brachiosaurus-authored-upright-rigid-bind-party.png`
19. `elephant-bind-faithful-bind-party.png`

## Known open items — do not spend the review re-finding these

Recorded with measurements in `WEIGHTED-PROTOTYPE.md`, and all of them belong
to the final geometry pass:

- the trunk still reads as a fifth leg and passes within 0.06 units of the
  foreleg — tapering by arc length helped and did not settle it;
- the Brachiosaurus neck is a hinged boom with a capped knob for a head;
- the shoulder and neck-base wedge, which survived all five seam weightings;
- the mid-limb joints drive 16 vertices, so there is no carpal bend;
- the tail is about a quarter of the neck's length;
- a UV pole whorl on the dorsal midline and a texel-density step at the
  shoulder;
- the ears are flat plates and do not yet read as ears;
- the gore pose tucks the trunk between the forelegs;
- placed primitives rather than a unioned surface.

New defects, and any of the above that these images show to be **worse or
different** than described, are what a further review is for.

## Not provided

No native screenshots, no in-game frames, no animation curves, no death or
interruption phases, no turn, no rear views, no close-ups, no icon-scale
render, no textured render of a posed state, and no top-down view of the
contact poses. The product candidate is not valid while the painted icon is
missing, and native animation qualification has not been performed.
