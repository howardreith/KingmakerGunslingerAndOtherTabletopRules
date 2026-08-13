# Elven Branched Spear asset provenance

The Elven Branched Spear mesh and icon are original project-owned works created
for this repository on 2026-08-13. They do not use Paizo, Owlcat, marketplace,
or other third-party artwork. No donor archive was present under the optional
`asset-sources/elven-branched-spear` path, so no third-party license or
attribution applies. The repository license governs these files.

The design comes only from the mission's textual silhouette: a slender
two-handed shaft, long central leaf point, three staggered forward-raked leaf
branches, restrained blue-metal inlay, and proportions suitable for an
isometric game. It does not claim to be an official Pathfinder or Owlcat asset.

## Reproducible source

- Generator: `assets-source/original-models/elven-branched-spear/generate_elven_branched_spear.py`
- Blender source: `elven-branched-spear.blend`
- Export: `elven-branched-spear.fbx`
- Source render: `elven-branched-spear-icon.png`
- Machine-readable report: `elven-branched-spear-build-report.json`
- Blender version: 4.5.10 LTS
- Geometry: 15 mesh objects, 900 triangles
- Coordinate contract: metric; grip at origin; point along +Z
- Bounds contract: butt -0.915 m, point +2.01 m, maximum width 0.26 m

SHA-256:

- generator: `47A4069B86799CBEBC844909FF82CFE38D22E70B1993CBAE686C19BF57F73B75`
- Blender source: `20CEEE2571D2515432E53AD3E6D9976C798C6AD53CEC5E220B5FFD4CA60A6D33`
- FBX: `8A79B5FE83285BA8D95B4111008A9C2E330DC61BFE4BA7CC2212D0C7CB25474B`
- 512px source icon: `7133B7536A78BD6A1712DBEA02FFAD49901957F2F73D05C083EE2F9F8FFA652A`
- 128px runtime icon: `2F3CF65793CCE8A1F79F6E907887FDC42698188150844B1A7D7B75C79C433186`
- Unity bundle: `3AB56092F363AA96C627287095E2CA549EEA7ED50D39C73BCD943646BFBE0EBE`

No manual mesh modifications were made after generation. The only derivative
operation outside Blender was deterministic high-quality downscaling of the
transparent 512px render to the project's native 128px inventory-icon size.
