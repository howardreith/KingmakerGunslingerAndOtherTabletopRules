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
- geometry: 39 mesh objects, 2,997 triangles
- blade planform: asymmetric curved single edge at local `-X`, blunt spine at
  local `+X`, restrained tip, oval tsuba, elongated wrapped grips
- icon contract: measured 42 degrees above horizontal, tip upper-right, butt
  lower-left, exact 128x128 transparent RGBA production output
- coordinate contract: metric, primary grip at origin, tip along +Z
- lengths: Wakizashi 0.76 m, Katana 1.05 m, Nodachi 1.58 m

SHA-256:

| Output | SHA-256 |
|---|---|
| generator | `A0D3AEEC0BF85EDB458835B99958D6CAC9D9F96567CC41A6ED49EAA40CD2C44D` |
| Blender source | `653CF762EC15AAC848D15A307400BCCFFE6BF42DB249CD122A71FB592B9DFC60` |
| Wakizashi FBX | `73E1A225B833E835550DA50DB55CCBEB842C18E3A80E13BEEAC62FB80F248D08` |
| Katana FBX | `E8116E82F279DB0DAEE4D4B6031BCEECC4685F5E1B4CC19AE319D269641536D6` |
| Nodachi FBX | `FDB2D2C3101CCF0B2368320030D598D9E42A91D02C37D0EE568D5F3B95320FF2` |
| Wakizashi icon | `06734FA45659AD542DA075B03B6A6AD01A763D4943BD96956F394E50295F1CBD` |
| Katana icon | `6DD9D308F3EDEC57084D1BCB06BD4DEB037E8DE311EE77A5AD125A666D3FCF99` |
| Nodachi icon | `CCCBC496712B918620945D078913FA7DA1D939CC0425D3C490FE85BC2F78ED6E` |
| Night Without Moon icon | `C066EB7BFFCAEEB96EC386C2CD58892B864D2CE579B5DB2FEF28084B869E0618` |
| Heaven's Measure icon | `248B0B65DE9FBB95C9B1577D468CA83D2F6F3EE696A24B6465E8658B27DC72E5` |
| World-Tree Severer icon | `658061646032A6E48ADDC6183CB99D5613E6E3EB2185C093AEE6DA0ED488FCC9` |
| Unity bundle | `F58801B7B34514B06577EA9CE36F2F3FC0A79A6F157113EA227251BFE2A15B43` |

The Blender generator deterministically defines geometry, materials, camera,
lighting, transforms, output dimensions, and export options. Blender container
and FBX exporter metadata is captured by the report and pinned by tests. The
production Unity bundle was rebuilt twice consecutively from the recorded
inputs and was byte-identical.

Subjective in-game appearance is not claimed here; it remains human review.
