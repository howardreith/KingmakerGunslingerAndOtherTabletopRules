# Private Sprint 17 snake prototypes

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
A fixed native Purple Worm comparison is the next bounded research gate.

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
