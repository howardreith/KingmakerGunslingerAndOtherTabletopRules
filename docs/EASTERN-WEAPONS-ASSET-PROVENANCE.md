# Eastern Weapons asset provenance

The Wakizashi, Katana, and Nodachi meshes and all six Eastern Weapons icons are
original project-owned works generated for this repository. They use no Paizo,
Owlcat, extracted-game, marketplace, mod, downloaded, traced, third-party, or
generative source. Repository licensing applies.

The silhouettes derive only from mission-authorized visual language. The four
variants per family combine visibly different guard/pommel geometry with
bounded blade width and curvature. Family scale remains fixed: Wakizashi 0.76
m, Katana 1.05 m, and two-handed Nodachi 1.58 m. No project-owned scabbard mesh
is included. The bundle does include independently transformed, complete
stored prefabs of each project-owned weapon mesh. Those custom presentation
clones clear the redundant donor sheath field; the native donor blueprints and
their sheaths remain unchanged.

## Reproducible sources

- generator: `assets-source/original-models/eastern-weapons/generate_eastern_weapons.py`
- Blender source: `eastern-weapons.blend`
- FBXs: the 12 files enumerated by `eastern-weapons-build-report.json` and
  `assets/bundles/asset-bundle-manifest.json`
- icons: six 512px source renders and six 128px runtime PNGs
- Blender: 4.5.10 LTS
- geometry: 190 mesh objects and 12,252 triangles across 12 variants
- materials: four definitions per prefab; no textures
- coordinate contract: metric; physical source forward `+Z`; blade normal
  `+Y`; cutting-edge side `-X`; renderer-grounded grip/tip/butt/stored markers

Stable identities:

- generator SHA-256:
  `758D015B44372C427FDD58397662A44244CD93F3389D86E5945A0AA592B373E5`
- Unity bundle: 365,592 bytes, SHA-256
  `AE311993F683295D3DD996285D28385A20F593DF16903D909818EB4F25A0096B`

The exact 12 FBX hashes are locked in the bundle manifest and domain tests.
The schema-3 report records the evaluated semantic/renderer relationship. Two
unchanged-input Unity 2018.4.10f1 builds produced the exact bundle hash above.
The `.blend` container is not falsely claimed byte-stable because Blender
embeds session metadata; it regenerates the same authored object contract.

The previous 12-held-prefab bundle was 311,289 bytes. The calibrated result has
12 held plus 12 independently stored prefabs. Blueprint/type GUIDs, family
categories, mechanics, campaign placement, persistence, and optional-mod
contracts are unchanged. Runtime clones the native presentation donor, replaces
its held and belt model fields, and clears its sheath field only on the custom
clone because the complete custom stored prefab owns that role. Animations,
trails, sounds, slots, timing, and every other donor field remain exact.
The clone-only replacement is published at
`754ae076de0c02b5dd1e62691ba5905aa363432c` and clean-qualified without
changing the deterministic bundle. Guarded visual acceptance is recorded in
`docs/EASTERN-WEAPONS-VISUAL-CALIBRATION.md`.
