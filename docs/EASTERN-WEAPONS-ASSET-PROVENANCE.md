# Eastern Weapons asset provenance

The Wakizashi, Katana, and Nodachi meshes and all six Eastern Weapons icons are
original project-owned works generated for this repository on 2026-08-14.
They use no Paizo, Owlcat, extracted-game, marketplace, mod, downloaded,
traced, or other third-party model, texture, or artwork. Repository licensing
applies.

The silhouettes derive only from the mission text: conservative curved
single-edged blades, Wakizashi visibly shorter than Katana, and a long but
non-exaggerated two-handed Nodachi. No scabbard or secondary carried model is
included.

## Reproducible sources

- generator: `assets-source/original-models/eastern-weapons/generate_eastern_weapons.py`
- Blender source: `eastern-weapons.blend`
- FBXs: `wakizashi.fbx`, `katana.fbx`, `nodachi.fbx`
- six 512px transparent source renders: the three family names plus the three
  capstone names with `-icon-source.png`
- machine-readable report: `eastern-weapons-build-report.json`
- Blender: 4.5.10 LTS
- geometry: 39 mesh objects, 3,522 triangles
- coordinate contract: metric, primary grip at origin, tip along +Z
- lengths: Wakizashi 0.76 m, Katana 1.05 m, Nodachi 1.58 m

SHA-256:

| Output | SHA-256 |
|---|---|
| generator | `8C1723A70ECFBF628D16EC7AFF824B6C9DE1D91B686AC6FED3B51F553C062FD6` |
| Blender source | `B0436EC6AA06F231C064DAC0D09A338A0209A65D8A3A01CAE260D92ADFEF08A9` |
| Wakizashi FBX | `C1FC338B67D9A3ABF6FD13507D3645C046EEE46F61F696F746890D3D858023BA` |
| Katana FBX | `7C608292339B86DDAEDF0926E2701841282C72963E227442917FD303CDD064C6` |
| Nodachi FBX | `0CA8CA8A71AB7893E0FEC3ECEB538A236F1C0D763F31BEB8E0230B436C8987FB` |
| Wakizashi icon | `37B63E5558527ABFFC97FD8B9619B3643C3CEEEB4CB4A36D16097DCF6AADB8F2` |
| Katana icon | `00F09CF266B9D1BC6CAAE411FE6CB43E4E844CEA469A3D0552D34C82CA86B65C` |
| Nodachi icon | `002B53FBB1C9F1678DC3DD7B3097AE913AF17C6193C1395B6BB555D450FFD215` |
| Night Without Moon icon | `A5D0D46E6130F70A9B5831AD704B9BE0FF43FB3C7D81466D39F7343731C3A41A` |
| Heaven's Measure icon | `60126EA454F449DF68F4A650D7AF0275B08632EDB416C8E560E20A9881F3EDA2` |
| World-Tree Severer icon | `A49F8C0A4C52E351A898834B861AB8138752193D85BCBACD0E232D6A54E2C4B5` |
| Unity bundle | `39884FF681EE553DE957E36E01B350AB926A452F994C4E8D33015D57D4EAD1EC` |

The Blender generator deterministically defines geometry, materials, camera,
lighting, transforms, output dimensions, and export options. Blender container
and FBX exporter metadata is captured by the report and pinned by tests. The
production Unity bundle was rebuilt twice consecutively from the recorded
inputs and was byte-identical.

Subjective in-game appearance is not claimed here; it remains human review.
