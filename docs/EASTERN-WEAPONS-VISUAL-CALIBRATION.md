# Eastern Weapons visual calibration

## Calibrated presentation contract (2026-08-21)

The production meshes use a complete, renderer-grounded source frame rather
than an unexplained Euler correction. Physical grip-to-tip is source `+Z`,
blade normal is `+Y`, and the distinct cutting-edge side is `-X`. Unity's FBX
reflection is accounted for when reconstructing the right-handed imported
basis. Equipment roots remain identity transformed; only the visible child is
basis-converted and translated.

| Family | Physical length | Native donor | Held grip | Held support |
|---|---:|---|---|---|
| Wakizashi | 0.76 m | Scimitar / `OH_ScimitarBandits` | measured Scimitar grip `(-0.008433,-0.020029,0.001054)` | none; compact one hand, valid main/offhand |
| Katana | 1.05 m | Bastard Sword / `OH_SwordBastardArmy` | donor weapon-bone origin | donor versatile/two-hand behavior; no custom IK |
| Nodachi | 1.58 m | Greatsword / `TH_GreatswordBarbarian` | donor weapon-bone origin | held-only IK at the native `-0.169 m` butt-side station |

Each donor contributes its measured target forward `+Y`, blade normal `+X`,
and cutting-edge side `-Z`. Translation then solves the held grip or independent
stored renderer-center anchor. The serialized visible-child transform is an
output of that basis solve. It is not shared between held and stored roles.

All 12 production variants have exact held and `Stored` prefabs. Runtime clones
the donor `WeaponVisualParameters` and changes only `m_WeaponModel` and
`m_WeaponBeltModel`; animation style, trails, sounds, slots, sheath, timing, and
all other fields are preserved. The native donor blueprints are never mutated.
The custom models retain separate palettes, guards, wraps, and silhouettes.

## Validation and artifact identity

The Unity 2018.4.10f1 builder and runtime both reject missing semantics,
degenerate/collinear axes, negative scale, reversed tip/butt, endpoints outside
renderer bounds, nonidentity roots, incompatible held/stored roles, wrong
cardinality, or unexpected cameras/lights. Runtime publication is transactional:
all 12 held/stored pairs must pass before any custom model is exposed.

Two consecutive forced builds produced the same 365,592-byte bundle:

`AE311993F683295D3DD996285D28385A20F593DF16903D909818EB4F25A0096B`

The clean published checkpoint is
`8aeef5e7fb2ef976e7ca5cbe82ba44d50b01401b`. Its clean Release runtime package
and DLL SHA-256 values are
`0AC692C8D3F5EFC8D7A15968BBA8B791C6F4885D8A17156B8F8AFF2695927A5B`
and `CCF8F81C0025762CD52835A6949848652C255F45EC7B895B083ABA4AD368B8FB`;
DLL MVID is `3e3d7594-5eab-4c58-b739-0e9e04e5326f`.

## Runtime visual acceptance

Guarded Steam App ID 640820 evidence from the clean checkpoint passes:

- `20260821T0655066469058Z-weapon-presentation-evidence`: all 22 production
  variants and six native controls, 56 held/stored PNG/JSON pairs, 224 labelled
  views, 9/9 assertions, no blank or low-density sheets, exact cleanup.
- `20260821T0657502514655Z-weapon-presentation-eastern-motion-evidence`: all 12
  Eastern variants and Scimitar/Bastard Sword/Greatsword controls in
  combat-ready plus nine attack samples, 150 PNG/JSON pairs, 600 views, 6/6
  assertions, all 15 commands acted, exact cleanup.
- `20260821T0701587480686Z-disposable-eastern-weapons-combat`: 21/21 live
  identity, presentation-donor, item-resolution, combat-rule, and cleanup
  assertions across all 30 Eastern items.

Front, side, rear, and three-quarter review accepts grip, tip polarity, blade
plane, and held/stored independence for all 12 variants on the captured default
Medium male. Katana follows the Bastard Sword attack frame; Nodachi follows the
Greatsword frame with both hands plausibly on its handle; Wakizashi retains its
valid light-blade attack pose without sideways roll. No severe persistent
clipping is visible in captured held-idle, stored, combat-ready, or acted
states. This acceptance does not infer locomotion, equip/unequip transitions,
armor/cloak, female, Small, or Enlarged results; those remain mission-matrix
work.
