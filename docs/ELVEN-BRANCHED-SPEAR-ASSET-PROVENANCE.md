# Elven Branched Spear asset provenance

The Elven Branched Spear meshes and icon are original project-owned works
created for this repository. They do not use Paizo, Owlcat, marketplace,
downloaded, traced, or generative artwork. The repository license governs every
file in `assets-source/original-models/elven-branched-spear`.

The 2026-08-15 cleanup creates three reusable silhouettes from the mission's
textual design authority: a slender two-handed shaft and central leaf blade with
two, three, or four physically separated backward-swept prongs. The prongs have
real thickness and lateral separation, remain above the 1.47 m shaft-grip
exclusion boundary, and are intentionally readable at isometric scale.

## Reproducible source

- Generator: `assets-source/original-models/elven-branched-spear/generate_elven_branched_spear.py`
- Blender source: `elven-branched-spear.blend`
- FBXs: `elven-branched-spear.fbx`, `elven-branched-spear-thorn.fbx`, and
  `elven-branched-spear-crown.fbx`
- Source render: `elven-branched-spear-icon.png`
- Machine-readable report: `elven-branched-spear-build-report.json`
- Blender version: 4.5.10 LTS
- Geometry: 45 mesh objects and 2,700 triangles across three variants
- Coordinate contract: metric; grip at origin; point along +Z
- Bounds contract: butt -0.915 m, point +2.01 m
- Unity prefabs: `ElvenBranchedSpear`, `ElvenBranchedSpearThorn`, and
  `ElvenBranchedSpearCrown`

SHA-256:

- generator: `301EEA4C1C91D561CC0E77177713DECF3865B0DF3BB0D984DD73E1CF25A5386A`
- classic FBX: `80773756F2C403D8569FE811B049FC3B53AE1399FA83446A70710AF1F69833E5`
- thorn FBX: `2BE981892A5C08E96A018FC5CC9188311128725B5BB0FC545DA12E298205734F`
- crown FBX: `0FAF504CFDD5290E71993A484A77874AEEB2CB01B38174CC7635F716C345D99B`
- 512px source icon: `3F0EC4182B48F6CF8A4C19D70101B84E6B9AAB5FAA4556CD6D8B043897F142F7`
- 128px runtime icon: `9E959D51A39C3F171403975913CD049C2CB2DE2D7D394F8CC53E71717AF2F8BB`
- Unity bundle: `6E9FE86E43072361EEC3357D9C73E17ADD71D22BAF257FB8C7ED6F52931CE777`

Two clean Blender runs produced byte-identical FBXs and normalized PNGs. Two
unchanged-input Unity 2018.4.10f1 builds produced the same 111,659-byte bundle.
The `.blend` binary is deliberately not claimed byte-stable because Blender
embeds session metadata; regenerating it produces the same authored object and
geometry contract. The generator's build report records the current container
hash and this limitation.

The previous single-prefab bundle was 87,627 bytes; the three-prefab bundle is
111,659 bytes, an increase of 24,032 bytes. It contains three prefabs, 45 source
mesh objects, 15 material definitions, and no texture asset. Growth is explained
by the two additional geometry/material variants.

Existing item, weapon-type, proficiency, effect, category, placement, and save
identities are unchanged. Runtime maps exact item symbols to approved variants
and retains the classic type-level/native Longspear fallback. Human in-game
acceptance of branch readability and clipping remains a separate gate.
