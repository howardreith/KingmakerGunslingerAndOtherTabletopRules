# Cleanup2 Brachiosaurus tail retarget study

Scratch study only; no production source or assets were changed by the study,
and it launched no native process or build. Local paths below are relative to
`artifacts/cleanup2-tail-prototype/` in the main Cleanup2 worktree. Raw renders,
scripts and measurements remain ignored machine-local evidence. V1, v2 and
failed/less-effective retarget controls remain preserved. The integrator viewed
v4 side/rear-quarter act and rear windup pixels; native activation is not approved.

## Cause established by matched calculations

The authored generator binds Tail2 to the long tail segment starting at renderer Z 3.8, Tail3 at 4.8 and Tail4 at 5.75. Their actual earlier native donor bind pivots are instead Z 2.844, 3.058 and 3.303. Rotating the long authored tail around that compressed native chain creates large off-pivot skinning displacements.

At identical Tail2/3/4 angles `[35,30,20]`, original pivots/original weights give edge-length ratios **0.468–2.265**, with **353** edges outside the existing 0.65–1.35 graybox diagnostic band. Moving only owned pivots to the authored segment starts, with the original weights completely unchanged, gives **0.720–1.250**, **zero** offending edges. At the same `[22,18,10]` pose the ratios improve from 0.620–1.714 / 101 failures to 0.833–1.157 / zero failures. See `retarget-v3/selected/retarget-report.json`, including four matched pivot/weight controls.

Weight smoothing alone with compressed native pivots does not resolve the strong distortion. Retargeted pivots plus linear weights improve edge ratios somewhat but squeeze some triangle areas more than original weights. Thus pivot placement is the dominant demonstrated cause; weight edits cannot substitute for fixing the pivot frame. This is an authored rest-frame skinning experiment, not a statement about measured native clips.

The minimal three-pivot v3 is mechanically promising, but its fixed Tail1 leaves a visually static thick base. The original-weight 85-degree distal bend looks cleaner than v1 yet still resembles a distal flick in profile. Its initial rear-quarter framing cropped the crown; the later v4 uses measured aggregate project-geometry framing and preserves the complete head/neck/body.

## Recommended art proposal: root-pinned v4

Review `root-pinned-v4/selected/synthetic-root-pinned-original-tail.mp4` and its sixteen `synthetic-root-pinned-{rest,windup,act-proposal,recovery-proposal}-{side,rear,front,rear-quarter}.png` images. All sixteen stills were opened and inspected. The MP4 is a generated synthetic 1.5-second curve, not a native attack recording.

V4 keeps all non-tail vertex positions **and weight rows byte-equivalent**. It changes only 594 originally tail-weighted rows, pins the 40 tail-side vertices of the 70 pelvis-junction triangles to source-equivalent Tail0, and grades the remaining tail into Tail1–4 over a 0.70 renderer-unit root transition. It adds an owned Tail1 pivot experiment as well as Tail2/3/4: authored centerline Z `[2.9,3.8,4.8,5.75]`, preserving measured source bind orientations. Native source bones are never edited.

Original authored yaw at the proposed one-second act is `[15,25,25,10]` degrees; pitch is `[6,8,4,2]`. Windup at 0.45 is yaw `[-15,-20,-15,-8]`, pitch `[-6,-8,-4,-2]`; recovery at 1.25 eases through yaw `[5,8,6,3]`, pitch `[2,3,1,0]`, reaching rest at 1.50. Renderer-up and renderer-right supply explicit yaw/pitch bases; no native Slam/Gore or other clip is borrowed.

Forty-six sampled synthetic frames are finite. The sampled edge envelope is **0.753–1.327**, with **zero** offending edges. All junction and non-tail displacement is **exactly zero** in this authored test. Rest-shape maximum numerical error is **0.00000093 renderer units**. Neither topology, UVs nor vertices are altered. The weight/bind remap mathematically preserves rest shape; it is not permission to alter a native mesh.

Pixel findings: the larger base-to-tip sweep and lift/downstroke are readable from the side, rear and rear quarter; the caudal base stays attached and the neck/body/feet stay unchanged. The bend has a continuous curved silhouette rather than the sharp v1 hook. A strict front view still hides most of the tail behind the torso; only a tip sliver appears at extremes. This is a plausible original tail-whip art candidate for native testing, **not** exact enemy contact, 20-foot visual reach, whole-body counter-rotation or owner acceptance. The existing native forward-facing victim/contact problem is not solved by inventing a target or moving the root.

V4 report SHA256 `ed6eb858e00c2b94f86263de893e0c8bfa7081408d74ddaa4400b409a2173d96`.
MP4 SHA256 `dc3331ef0528f798b6c0ba5a8e9233a6cb912ca59d5286742abde99ea1364f0e`.
Current input mesh remains SHA256 `9bd237aad9b1a99f25162e4a109814b5f72fb9da6167d2f000e88f333e24a974`.

## Minimal coherent runtime integration for a later checkpoint

1. Bake the tail-only weight repair and authored pivot positions into the reproducible project generator/payload. Preserve every non-tail row, all topology/UVs, IDs, stats and collision. The production body must continue to originate from the ordinary registered Brachiosaurus route, not a debug replacement.
2. After successful exact private body attachment/material adoption, create an ownership-checked Transform-only tail proxy chain. Preserve source Tail0 and all native skeleton/Animator/colliders. Only the **private project mesh instance** receives four new owned bind-pose entries and corresponding Tail1–4 palette transforms. Shared cached project meshes and every native mesh/material/action remain unchanged. Restore only exact owned palette/bind entries and dispose owned transforms on rollback, module disable, view refresh and destruction.
3. Derive the runtime baseline from the actual current native source transforms
   and exact private bind matrices. A proposed local retarget formula is not a
   proof of current-pose equivalence. At zero gesture offset, the owned palette
   and private bind-pose product must reproduce the original current skin
   matrices, including parent motion; check that algebra and finite baked poses
   before using an authored offset. Require supported TRS/shear/scale and the
   exact parent chain. The older scalar capture certifies neither the current
   native pose nor a native retarget implementation.
4. Use a **read-only** postfix at the actual native null-handle `ScheduleAct(1)`/`TryStartNextAttack` seam. Require exact registered Brachiosaurus, exact live summon source/caster, actual Tail weapon/hand, exact body/material receipt, no native/foreign Tail animation handle, and native deadline unchanged. Drive only owned visual transforms against native command time. Observe native act/end events; permit a bounded visual recovery after a real act, and abort on cancellation. Do not return an animation handle, assign `IsActed`, alter the fallback, invoke attack rules or supply a hit.
5. Extend the exact attachment/material-controller receipt narrowly for Brachiosaurus; the current read-only helper is Crab-only and the giant-body material collector's prefix filter is insufficient as an activation proof. A foreign refresh must cause refusal without stale palette overwrite.
6. Run a bounded actual canonical single/full ordinary Tail command observer, with native post-placement enemy/range/LOS proof. Compare actual fallback deadline/act events and costs before/after the render driver, finite baked current-pose geometry, junction continuity, actual PNGs and same-frame native source/collider/root invariants. Retain independent native Mastodon and released Elephant/Triceratops controls. Missing native Hit remains unsupported.

The remaining blocker is **native runtime ownership/current-TRS/timing validation**, plus integrator visual approval of this concrete v4 proposal. The synthetic art now has a stronger viable deformation envelope; no production activation or timing-neutrality claim has been made.

## Reproduce

Use the confirmed Blender 4.5.10 executable in background mode with `--python-exit-code 1` and these scratch scripts: `retarget-probe.py` (initial controls), `retarget-grid.py` (bounded placement/weight comparison), `retarget-render.py` (v3) and `whole-tail-render.py` (v4). Inputs are the unchanged project mesh/albedo and existing `captures/sprint11-mastodon-bind-rig.json` scalar metadata. That capture is an earlier Mastodon-donor bind frame on a 26-palette WoollyRhinoceros view, not the missing full40-row Sprint25 census or a new current native actor. No native geometry, texture or animation curves are exported.
