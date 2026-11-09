# Summoned Creature Manual Playtest Repair — R7 status

**R7 is complete for review (2026-11-09).** ART-01..03 have real model revisions on
final candidate `e4b62528fa2c`. Owner aesthetic approval is separate and pending.
Nothing here was merged, released or permanently installed.

Branch `codex/summon-playtest-r7-models`, stacked on draft #116, which depends on
draft #115 and draft #114.

## What changed

| Issue | Change | Preserved |
| --- | --- | --- |
| ART-01 Glabrezu | The small arms were rooted outside the chest surface and left out of the skin union. They now start inside subpectoral mounds and fuse with the skin. One tapered canine skull replaces the boxed head; a V-shaped torso replaces the banded barrel; the forearms carry chitin vambraces. | Four arms, pincers on the real arm and digit chains, extents within 0.015. Small arms still follow the chest (rig limit). |
| ART-02 Nalfeshnee | Rebuilt on the same joints: a hunched boar-headed ape with scapular hump, ribcage and pectoral shelf over one sagging gut, wedge skull, flat rooting disc, underslung jaw, four tusks, bristled mane. Wings grow from scapular mounds with three fingers and scalloped membranes. | Size category and scale, four tusks, two non-articulated wings. |
| ART-03 Ice Devil | The separate pale beads and black rods are one fused skin: tapered limb sheaths, dark joints, dorsal and ventral plates over a dark slate under-body, one pauldron plate per shoulder, reframed head and mandibles, tail rings on a continuous core. | Silhouette, cold identity, equipped spear, seven-segment tail, four mandibles; extents within 0.02. |

Common to all three: `continuous-bodies/outsider_bake.py` (imported only by these
generators) bakes the albedo from bind-pose position, normal, material ID and
cavity occlusion, using the library's existing unwrap and the existing exporter.

Geometry grew and is disclosed: vertices 5,424 to 10,770 (Glabrezu), 5,919 to 12,402
(Nalfeshnee), 4,546 to 13,741 (Ice Devil); triangles 13,280 / 14,392 / 14,136; albedos
512 to 1024. The Nalfeshnee's tallest point rises 1.875 to 1.990 from ears and mane,
with no scale change; wing tips reach 0.786 against 0.762. Roster icons are unchanged
and still show the earlier Nalfeshnee.

The Ice Devil export names the retained equivalent rig capture, so its loader now
admits the same reviewed rig pair (`IsReviewedRig`) as its sibling outsiders. No
mechanics, attacks, reach, collision, GUIDs or icons changed.

## Prerequisites and evidence inputs

- **Blender:** pinned 4.5.10 LTS at `C:/Dev/.toolchains/blender-4.5.10-windows-x64/`,
  found through prior session records. Rig capture `6443b580…e65d`.
- **Complaint references:** private owner images E08, E09 and E12.
- **Before-state:** pre-R7 exports from commit `70d44c8be7`, stored privately and
  hash-pinned by the observer. They are never packaged. Neither the unversioned
  screenshots nor DATA's installed v0.0.140 was used as a before-state.

## Native route

Two save-free phases under the guarded, `Params.xml`-protected wrapper:
`summon-playtest-r7-models` (Oleg, bright outdoor) and
`summon-playtest-r7-models-interior` (Jamandi's throne room, dim). These are ordinary
`summon-playtest` phases; focused tests confirm they are not Sprint40 phases, and no
restricted routine was renamed or recreated. Each creature is cast through its normal
published summon, then the pre-R7 export is shown on the same live unit, so before
and after share camera, zoom, light and rig.

## Candidates

| Candidate | Purpose | Result |
| --- | --- | --- |
| `c5da8351497c` (package `696ba38a…9c60`, DLL `2b30b9e9…3b67`) | First | Bright phase PASS. Superseded; dim not run. Evidence retained in `runtime-evidence/20261109T0537135879411Z-…`. |
| **`e4b62528fa2c`** | Final | Bright and dim PASS. |

**Final identities.** Source `e4b62528fa2c432e4f797fd03c5781345d1dd6aa`, v0.0.179
development `summon-playtest-r7`. Package
`e57ca0d2971d735c8b22795d459a6303bedabe1522943029d59dd94190ed3687`. DLL
`cd72c112320ed14dc30d2adaa2e42c62fc40ff06970aa9f106da2375a8a3bc36`, MVID
`6e795521-81f0-49fa-b8e8-1b05e10b38fc`, loaded in both runs. The observer is compiled
into that DLL.

| Export | SHA-256 |
| --- | --- |
| `glabrezu-mesh.json` | `fe091f17eef9af988ff86a32f90dba544dd1a3243bf8160c0809e85ae46b13d3` |
| `glabrezu-albedo.png` | `4c2f15474aad86188320aed40463ccd6b891a03c0c9bc1ec8495d65a419ef34f` |
| `nalfeshnee-mesh.json` | `d34e6c194d5b6c8c798a1ead77b9248e48e44721fccb2491eccee79aa1199aaf` |
| `nalfeshnee-albedo.png` | `d9ab401ba01429ff64c8b05cdfaeac67b29b2d078a0f0c5d35e5002179868e7b` |
| `ice-devil-mesh.json` | `ac3468c597fe97752f5544d9da975c1c6dae0ca5e0d07d3a9ed9f3ca0ee9d700` |
| `ice-devil-albedo.png` | `a7e25c51737fcc5679cc29ed194248fd2eeb81eac3c682f34dfcec7df86e8351` |

Each generator reproduced its exports byte for byte on a second run. All meshes are
closed and consistently wound on welded positions.

## Results

- **Focused:** `summon-playtest;sprint32-native-admission`, 42 of 2,884 selected,
  passing, production compiled. New cases cover the Glabrezu small arms being one
  skin with the chest, the three loader contracts, and phase admission. The candidate
  build ran 41 of 2,884 (`summon-playtest`) and strict package validation passed.
- **Native, bright** (`20261109T0555428992269Z`): PASS. `r7-glabrezu-bright`,
  `r7-nalfeshnee-bright`, `r7-ice-devil-bright`, `r7-owned-state-restored` true. Both
  exports moved about 3 m and attacked; the Ice Devil kept its equipped spear.
- **Native, dim** (`20261109T0600334528504Z`): PASS, the same four checks in `-dim`
  form, with move and stills only.

## What I inspected

Actual game frames from both runs, as contact sheets and full frames: gameplay-zoom
party stills, close details, rear views, and move and attack frames. Motion was
**inspected through extracted frames, not watched continuously.** Offline renders
were used only during authoring.

In these frames the small arms stay attached in idle, move and attack; the Nalfeshnee
reads as a boar-headed winged creature at gameplay zoom; the Ice Devil keeps its
two-tone read and its spear.

## Limits and corrections

- **Bright flecks in "after" stills.** The first stills of each new export show small
  bright flecks. They are the end of the game's summon fade-in: the capture recorded
  `dissolve=0.059` on all three, 0 by the later "before" stills, and later frames of
  the same units are clean. The stills were simply taken first. **Use the idle pairs
  for comparison.** Retiming them needs another build and run, which would exceed the
  ceiling, so it was not done.
- **Two wrong explanations along the way.** I first blamed loose end-cap geometry and
  then self-shadow leaks. A code change and a test for the first were removed, since
  the exports did not change and the test flagged slender claws. A one-frame diagnostic
  disproved the second. A small crease-shading and flesh-value change made during that
  work stays in the final export; it was not needed to remove the flecks.
- **Dim scene.** The hall is populated. The dim Glabrezu idle view is partly blocked by
  a foreground beam and candle; the gameplay-zoom dim still is clear. No dim attack.
- All motion was commanded with brains inactive; autonomous AI was not observed.
- **Integration scope.** R7 changed the three bodies, the Ice Devil rig admission,
  package pins, observer phases and R6's clip helper. Native casts of the published
  summons exercised publication and the loaders for these creatures. Turn enrollment
  and optional-provider links were not changed and not re-run. No historical suite,
  compatibility matrix or R1–R6 replay ran. Earlier evidence stays with its candidates.

## Restoration

Intake v0.0.140, live tree `5C3E4AD4…BFBB` (254 files), DLL `00EF6757…66FD`, 16 mods,
`Params.xml` `E44F4495…30BB`, 95 saves. All three guarded runs restored it: tree,
`Params.xml`, settings and protected saves verified, no game left running. No campaign
save was written.

## Ledger (separate R7 ledger: 900 s target, 1,800 s ceiling)

| Step | Seconds |
| --- | ---: |
| Focused gates (25, 17 for the withdrawn test's control, 2, 18) | 62 |
| Candidate builds (413, 389) | 802 |
| Guarded native runs (199 superseded, 188 bright, 155 dim) | 542 |
| **Total** | **1,406** |

Over the 900 s target by 506 s, because of the second candidate; within the ceiling.
Blender authoring and offline renders are not counted. Earlier ledgers are unchanged:
R5 about 1,389 s, cumulative R6 about 2,796 s.

## Owner review

Private set: `runtime-evidence/summon-playtest-r7/review-e4b62528fa2c` (see `INDEX.md`).

1. Glabrezu: are the small arms convincingly attached, and do the head and torso work?
2. Nalfeshnee: does it now read as a credible boar-ape demon, and do the wings work?
3. Ice Devil: is the pale and dark integration better, without losing what you liked?
4. Are all three readable in both bright and dim scenes?
5. Do the changed proportions and the unchanged roster icons sit well together?

R6's viewing and listening decisions also remain open.
