# Private Sprint 17 original serpentine and hybrid prototypes

## Current private comparison, October 6 UTC

**NOT RUNTIME QUALIFIED; NOT PACKAGED.** The corrected a173da3a research
capture supports a second snake prototype on the native Purple Worm's
continuous chain and a separate original Salamander hybrid body. This is
authoring work only. No native geometry, texture pixels, animation curves or
proprietary assets are inputs or outputs. Private preview armatures use the
measured bind metadata; only original mesh data and bone names are exported.

The earlier water prototype remains below and in history. The shared exporter
now accepts the measured authoring family explicitly; water's geometry/weight
checks still pass. No existing runtime renderer or published creature changes.

### Continuous-chain Viper and Constrictor

`generate_worm_snakes.py` accepts only the captured native blueprint/prefab,
two exact skins and complete 40-bone frame. Original geometry uses 16 drivers:
`Hips_Joints`, `Body02` through `Body014`, `Head`, `Jaw_Down`. It never binds
to upper/side radial jaw petals, horn or stone joints. Both snakes have a
continuous tapered body, independent conventional lower jaw and original
paint; Viper has fangs/thinner body and Constrictor has smaller gripping teeth
and a heavier body. Vertex counts: 1436 and 1562, at most two influences.

Private output: `artifacts/private-sprint17/worm-prototype-v1`.
The initial review clipped the long body and overlit textured panels. The
corrected `review-framed` contains 48 panels: clay, silhouette, textured,
unlit, two body bend extremes, jaw opening and close-up head/jaw views.
Camera framing uses evaluated vertices, with a regression for long narrow
silhouettes. These synthetic poses do not prove native motion or contacts.
The original unframed panels remain preserved.

### Separate Salamander hybrid body

`generate_salamander.py` accepts only the existing exact Lizardfolk view's
two-skin/39-bone frame. It builds an original humanoid chest/head, two arms,
four digits per hand, a rising lower coil, four-joint tail and swept crests.
27 drivers carry geometry; legs, feet, native armor, club and shield do not.
No weapon is included. **Weapon handling is not solved or qualified here.**

Private final authoring iteration: `artifacts/private-sprint17/salamander-prototype-v3`,
2592 vertices, at most two influences, 26 review panels. v1 exposed detached
digits and a waist seam. v2 connected finger roots and made one continuous
tail/waist/neck skin. Its transported texture orientation put belly paint on
top of the coil; v3 explicitly maps the dorsal direction between tail and
upright chest. Connectivity and dorsal-paint regressions guard both repairs.
All earlier iterations remain private and preserved, not silently replaced.
Paint is original seeded ember scales; no native texture input.

### Current authoring checks and private hashes

Four water, four continuous-chain and six hybrid behavior tests PASS under
Blender 4.5.10 LTS: exact provenance/rejections, closed finite geometry,
determinism, normalized weights, excluded donor branches, anatomy differences,
framing, waist/finger connectivity and dorsal paint. Focused domain 219/219,
complete repository wrapper and unfiltered domain 2021/2021 (83.2 seconds)
PASS. No production C# or shipping asset changed; a new exact build/package
and runtime gate remain mandatory at the next executable candidate boundary.

Both new captures come from
`20261006T0242403834515Z-disposable-expanded-summoning-serpentine-survey`:
worm capture SHA `f44405a3d8b2fa314832fe14e7f1f778aa1f8d7c98d350fa35b4f7515e80f22c`;
Salamander capture SHA `bf6035021f916ccf650057fac964f47c9797072545138cf2546ec3d59ad0384b`.

| Private output | SHA-256 |
| --- | --- |
| Continuous-chain Viper mesh | `4fc12f4e870d0af38f0314112f90a9c7fb5ad935ac78e0ef7a75c43e38201dcf` |
| Viper albedo | `3bc6e6946c4be56cef5656325d97d22cdeddbb0e79f16612a7a98b6a7c5f1e72` |
| Continuous-chain Constrictor mesh | `7cddb3a96f9ad00a5a857df7b2797dc34b6ec477f64cf5b8dafa9b9027c99894` |
| Constrictor albedo | `cea2b015b0930f3b8a4f5b718339688b77c9643860cf1f9700406b2c074360cc` |
| Salamander v3 mesh | `80de7840d183666c7e7553c41d96d5bc3f532f4ab2556f62147f867a26371fb6` |
| Salamander albedo | `127e9fe76ad853be6dd7653a27ef9f526d7ffedbcbe9c942c244d2e15cb10aa0` |

Reproduce the worm family with the commands below, substituting
`generate_worm_snakes.py`, `test_worm_snake_prototype.py` and the worm capture.
For Salamander, use `paint_salamander_albedo.py`, `generate_salamander.py` and
`test_salamander_prototype.py`, its own capture, and omit `--kind` (identity
is fixed). `render_snake_review.py --suite` selects the family from the private
armature; hybrid adds synthetic tail and arm poses. Always use
`--python-exit-code 1` and keep outputs outside shipping `assets/`.

Next required: bounded live donor motion/ground/jaw acceptance, exact-owned
multi-renderer fallback/lifecycle, and Salamander's actual spear carrier and
tail contact. The archived native donor audit contains a separate Lizardfolk
greatclub/two-hand candidate. Exact c55 research subsequently confirms its
39-bone body and 19-bone armor frames match the current view's bind positions,
rotations, parents and indices exactly. Its attached grip/animation remain
**unqualified**. Native clip names also expose water's slam actions and only
idle in the worm locomotion list; a reared sampled worm pose is not proof of
snake movement. Refine/prove the supporting coil/body in a bounded owned-view
slice, without a global animation/limb rewrite or movement waiver. Do not infer
two-hand spear handling from the old club/shield view or force all three
creatures onto one rig. Printed profiles/signatures,
icons, hidden candidate and publication remain open. HumanReview:
NOT_PERFORMED_NONBLOCKING.

## Preserved first water-rig prototype

October 6, 2026. **PROTOTYPE ONLY; NOT RUNTIME QUALIFIED; NOT PACKAGED.**
These are original code-authored geometry, UVs, skin weights and seeded paint.
No native geometry, texture pixels or animation data are inputs. The private
f507 research capture supplies measured bind positions for offline armatures;
exports contain original mesh data and bone names, never native bind matrices.

The native Water Elemental body is a bounded first donor hypothesis, not an
accepted snake rig. Seven native skins include a 43-bone body. These prototypes
weight only LowerTorso, SpineA_M, Spine1_M, SpineB_M, UpperTorso, Neck_M, Head
and Jaw_M. No geometry rides the arm, hand or water-effect branches. The low
tail coil is rigidly weighted to LowerTorso: **there is no continuous tail
chain here**, and neither locomotion nor bite animation/contact is proved.
The later exact native Purple Worm comparison passed as recorded above.

## Offline findings

Viper is the contracted Medium venomous snake: thin body, broad triangular
head, fangs and tawny diamond pattern. Constrictor is a thicker olive snake
with a narrower head, smaller gripping teeth and dark saddle markings.
Both have independent lower-jaw geometry and at most two normalized weights
per vertex. No leg geometry is present.

The first clay pass found accumulated cross-section roll turning the head
onto its edge. Transporting the section from the muzzle toward the tail
corrected that error in v2. The sharply bent coil-to-neck join and jaw hinge
silhouette still need final refinement if this donor survives comparison.
The textured preview's light response does not establish a usable game shader.
The native water skins/materials must not leak into any eventual owned swap.

Blender 4.5.10 LTS generated 40 private panels: each creature in clay,
silhouette, textured and unlit shading in four views, jaw-open side and
three-quarter, and both synthetic neck extremes. Poses are invented offline
stress tests, **not native clips or runtime timing/contact evidence**.
Four behavior tests pass: exact capture/rejection cases, closed finite
geometry and complete deterministic weights, isolated mouth/tooth atlas
regions, deterministic distinct paint. Viper has 2052 vertices; Constrictor
2178. Only these generators/tests/documentation are checked in.

Private v2 output: `artifacts/private-sprint17/water-prototype-v2`.
Source capture: research run
`20261006T0142363709333Z-disposable-expanded-summoning-serpentine-survey`,
`sprint17-medium-water-elemental-rig-survey.json`, SHA-256
`cd7bda3fb879146702193aef2807fc85b678c634a9001a9f93a558acd0e2d061`.

| Private output | SHA-256 |
| --- | --- |
| Viper mesh | `a8738aa9eeb6d28fcbfb97240aafd16ac2189103f530b84191eb5e00b6de0235` |
| Viper albedo | `3bc6e6946c4be56cef5656325d97d22cdeddbb0e79f16612a7a98b6a7c5f1e72` |
| Constrictor mesh | `3054b295cb89f838a4d345bb128b1105036c9c8e1074d7326b1befb2d8982ca7` |
| Constrictor albedo | `cea2b015b0930f3b8a4f5b718339688b77c9643860cf1f9700406b2c074360cc` |

## Reproduction

Use the installed Blender in background mode with `--python-exit-code 1`.
For each key `viper` and `constrictor-snake`:

1. `paint_snake_albedo.py -- --kind KEY --out PRIVATE/KEY-albedo.png`
2. `generate_snakes.py -- --kind KEY --capture PRIVATE_CAPTURE
   --albedo PRIVATE/KEY-albedo.png --mesh-data PRIVATE/KEY-mesh.json
   --report PRIVATE/KEY-report.json --blend-out PRIVATE/KEY.blend`
3. `render_snake_review.py -- --blend PRIVATE/KEY.blend
   --out-dir PRIVATE/review --suite --resolution 480`
4. `test_snake_prototype.py -- --capture PRIVATE_CAPTURE`

Salamander is a separate humanoid/weapon seam, not this snake mesh. Its existing
identity/placements are untouched. No asset publication, new creature choice,
icon, package entry, mechanic or save state changes at this checkpoint.
`HumanReview: NOT_PERFORMED_NONBLOCKING`.
