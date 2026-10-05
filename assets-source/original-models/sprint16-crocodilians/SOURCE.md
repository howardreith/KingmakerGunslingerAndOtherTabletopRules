# Original Crocodile and Dire Crocodile

Sprint 16 source/art checkpoint, 2026-10-05. **Runtime NOT QUALIFIED.**

Both are original geometry, topology, UVs and deterministic paintings authored
by the checked-in generators. No native vertices, polygons, texture pixels,
material data or animation clips are inputs or redistribution assets.
The existing skinned-mesh JSON pipeline supplies each live instance's actual
native bind matrices at attach time. A missing, malformed or mismatched asset
keeps the native donor visible and must not partially attach.

## Private frame and anatomy

The guarded seven-assertion survey at source
`8250ac8f8369699af7825459bc860682e3e5eae5` captured one Monitor Lizard
renderer, 41 bind frames, root `cent_spine1_jnt`. Private capture SHA-256:
`d7c91e3e45384e37d3f906bd48e2f318acbfe0ccb30607c8970665d27a08c4ea`.
The file is in the machine-local evidence run
`20261005T1254571775971Z-disposable-expanded-summoning-crocodilians`,
named `sprint16-monitor-lizard-bind-rig.json`. It and the Blender previews
are private, not package/source-distribution content.

The original skin uses 27 reviewed bone names: the torso/neck/head, separate
lower jaw, all seven tail joints and three joints on each of four legs.
No tongue, unrelated foot or extra limb chain carries geometry.
Both meshes have 2,342 vertices, at most two influences per vertex, normalized
weights, inset UVs, original 1,024-square RGB albedo and complete triangle data.

Crocodile is warm olive with a flattened broad torso and blunt U-shaped snout.
Dire is a distinct heavier slate-olive body with larger osteoderms, not just
an enlarged texture copy. A continuous torso-to-head-and-tail skin removes
the first iteration's visible primitive collars. UVs are inset while their
region identity is still known; inferring a shared atlas boundary had created
pale limb-end bands, which the corrected exports remove.

Dire's exact identity gets a **2.0x view-only** multiplier on the shared Large
frame to represent its Gargantuan body. Crocodile and native Monitor Lizard
are not scaled. The old 0.20-1.25 bounds remain for every existing scale entry;
this is an explicit new-creature exception, not a general safety-bound change.
Large/reduced-reach and Gargantuan/15-foot reach are independent mechanical
contracts and remain live qualification gates.

## Deterministic assets

| Asset | SHA-256 |
| --- | --- |
| Crocodile mesh | `770fa7c3c87fb74f3335358069cefde77529868e30184e090bf3c7c840065fd7` |
| Crocodile albedo | `66864117b75bf34d30cc9c487ff988d8306940694d288a55d74aea3e772e2afd` |
| Dire mesh | `eb7a9182fbe5f66dc8b819efd9641a62da611230923bf4bd95ca1383d4e7173a` |
| Dire albedo | `aef6c8fe388635293938f5b0c0c46fa4757256326d6adbec88d351266bce253c` |

Only the four files under `assets/sprint16-crocodilians` enter the mod.
The strict package adds exactly four files: 309 with the soundbank, 307 without.
Neither private bind captures, previews nor source scripts enter the mod.

## Reproduction and review

Use installed Blender 4.5.10 LTS in background mode. For each of
`crocodile` and `dire-crocodile`, invoke:

1. `paint_crocodilian_albedo.py -- --kind KEY --out OUTPUT/KEY-albedo.png`
2. `generate_crocodilians.py -- --kind KEY --capture PRIVATE_CAPTURE
   --albedo OUTPUT/KEY-albedo.png --mesh-data OUTPUT/KEY-mesh.json
   --report PRIVATE/KEY-report.json --blend-out PRIVATE/KEY.blend`
3. `render_crocodilian_review.py -- --blend PRIVATE/crocodile.blend
   --out-dir PRIVATE/review --suite --resolution 640`

The suite renders 44 panels: clay/silhouette/textured/unlit in four views for
each creature; jaw-open side and three-quarter; both tail extremes and stride;
and ordinary/Dire side-by-side at the exact proposed 2x scale. The initial clay
lights clipped highlights, so clay is rerendered with 15% of textured energy.
These are synthetic poses: jaw -32 degrees, seven tail joints at +/-13 degrees
each, and bounded fore/hind limb swings. They are not exported native clips.

Offline attack-contact feasibility: upper/lower tooth rows use head/jaw
respectively; the jaw pivots at the captured hinge without an extra floating
mouth; tail geometry is carried continuously through every native tail joint.
Ordinary geometry reaches -1.2341 along forward Z and +2.1856 at the tail tip;
Dire reaches -1.2741/+2.1856 before the 2x view step. These measurements locate
the surfaces to inspect for bite/Death Roll/Swallow/tail contact; they do not
prove native animation timing, target spacing or hit events.

Live review remains mandatory: no visible donor body, all attack contacts,
idle/movement/turn/hit/death/fade/despawn, direct/quantity crowding, fallback,
per-view resource cleanup and native Monitor Lizard negative control.

`HumanReview: NOT_PERFORMED_NONBLOCKING`.
