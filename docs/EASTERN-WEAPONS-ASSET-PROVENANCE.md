# Eastern Weapons asset provenance

The Wakizashi, Katana, and Nodachi meshes and all six Eastern Weapons icons are
original project-owned works generated for this repository. They use no Paizo,
Owlcat, extracted-game, marketplace, mod, downloaded, traced, third-party, or
generative source. Repository licensing applies.

The silhouettes derive only from mission-authorized visual language. The four
variants per family combine visibly different guard/pommel geometry with
bounded blade width and curvature. Family scale remains fixed: Wakizashi 0.76
m, Katana 1.05 m, and two-handed Nodachi 1.58 m. No scabbard or secondary
carried model is included.

## Reproducible sources

- generator: `assets-source/original-models/eastern-weapons/generate_eastern_weapons.py`
- Blender source: `eastern-weapons.blend`
- FBXs: the 12 files enumerated by `eastern-weapons-build-report.json` and
  `assets/bundles/asset-bundle-manifest.json`
- icons: six 512px source renders and six 128px runtime PNGs
- Blender: 4.5.10 LTS
- geometry: 190 mesh objects and 12,252 triangles across 12 variants
- materials: four definitions per prefab; no textures
- coordinate contract: metric, primary grip at origin, tip along +Z

Stable identities:

- generator SHA-256:
  `9A55B6D41FDDABEB6A9D08005E3C93064840B9E02CB57273E50AB71F2D5EF283`
- Unity bundle: 310,375 bytes, SHA-256
  `079AA2E44E313291C144BD830D302782310274B11375204F9CE8FF6481EF3041`

The exact 12 FBX hashes are locked in the bundle manifest and domain tests.
Two clean Blender runs produced byte-identical FBXs and normalized PNGs. Two
unchanged-input Unity 2018.4.10f1 builds produced the exact bundle hash above.
The `.blend` container is not falsely claimed byte-stable because Blender
embeds session metadata; it regenerates the same authored object contract.

The previous three-prefab bundle was 147,724 bytes. The 12-prefab result is
310,375 bytes, an increase of 162,651 bytes attributable to nine additional
geometry/material prefabs. Blueprint/type GUIDs, family categories, mechanics,
donor animations, grips, campaign placement, persistence, and optional-mod
contracts are unchanged. Human in-game acceptance remains separate.
