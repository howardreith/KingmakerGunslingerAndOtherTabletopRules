# Elven Branched Spear visual calibration

Deterministic technical acceptance requires an identity equipment root, finite
positive scales, a complete non-collinear semantic frame, opaque Standard
materials, at least one enabled renderer, no camera or light, and renderer-
grounded length from 2.25 through 2.32 m. Runtime diagnostics report either
`custom:validated:<prefab>` or the exact native-fallback reason.

The authored frame is physical tip +Z and head-face normal +Y, with grip at the
origin, butt at -1.14 m, tip at +1.14 m, and support at +0.593016 m. The Unity
builder maps that basis to measured native Longspear frames:

- held donor Euler `(9.712032, 123.546196, 178.825317)`, yielding semantic
  forward `(-0.1292401,-0.9854609,0.1102895)` and head normal
  `(0.5553753,0.0202066,0.8313543)`;
- stored donor Euler `(359.074829,290.676361,267.541138)`, yielding semantic
  forward `(0.3521154,-0.0428966,0.9349731)` and head normal
  `(-0.0302411,-0.9989490,-0.0344429)`;
- held grip at zero; stored renderer-center anchor
  `(-0.0040513,-0.0071580,0.2127583)`.

Held and stored transforms are separately solved from those bases. The runtime
assigns `SupportHandTarget` only through the held prefab's
`EquipmentOffsets.IkTargetLeftHand`; a back prefab cannot drive hand IK.

Human acceptance should use a disposable development character and check:

- inventory icon alpha, framing, and contrast;
- medium male and female models, then a small race;
- idle, walk, run, ordinary thrust, full attack, movement AoO, and critical;
- weapon switching and dropped-item rendering where the game exposes it;
- Enlarge Person and Reduce Person;
- light, medium, and bulky armor silhouettes;
- hand separation, backwards point, floor drag, and body penetration relative
  to the unmodified native Longspear donor;
- missing-purple material, broken trail origin, and severe branch collision.

The custom mesh does not change animation style, socket, two-hand semantics,
attack timing, trail, reach, or sound. Those remain native Longspear contracts.

## Qualified default-medium presentation

The 2026-08-21 exact-package guarded runs accept the three production variants
on the disposable default Medium male fixture:

- static held/stored evidence:
  `20260821T0520508017635Z-weapon-presentation-evidence` (9/9, 56 sheets,
  224 labelled views);
- combat-ready/thrust evidence:
  `20260821T0525081495864Z-weapon-presentation-spear-motion-evidence` (6/6,
  40 sheets, 160 labelled views).

All 40 motion samples lead with the renderer-grounded physical spearhead, as do
all 15 samples in which the native animation reached its acted event. Mean
left-hand-to-support-target distances are Classic `0.130179 m`, Thorn
`0.123882 m`, Crown `0.124882 m`, and native Longspear `0.126062 m`. Direct
front, side, rear, and three-quarter review accepts dominant grip, both hands
on the shaft, branch roll, held idle, combat ready, thrust, and independent
stored orientation without severe persistent clipping in those captured
states.

This does not infer movement, turning, equip/unequip transitions, armor/cloak,
female, Small, or Enlarged acceptance. Those remain final-matrix work.
A visual imperfection is not a mechanics failure: disable or remove only the
dedicated bundle and the native presentation resumes without changing any item
or weapon-category blueprint identity.
