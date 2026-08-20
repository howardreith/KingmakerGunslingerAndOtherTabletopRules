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
- Source coordinate contract: metric; grip at origin; point along +Z
- Unity held coordinate contract: source `Visual` rotates +90 degrees around X so
  the equipped point follows the installed native Longspear forward `-Y` axis
- Bounds contract: butt -1.14 m, point +1.14 m, total 2.28 m
- Unity held prefabs: `ElvenBranchedSpear`, `ElvenBranchedSpearThorn`, and
  `ElvenBranchedSpearCrown`
- Unity back prefabs: `ElvenBranchedSpearBack`,
  `ElvenBranchedSpearThornBack`, and `ElvenBranchedSpearCrownBack`

SHA-256:

- generator: `66A5B86B8C3FDAD00C809C8C38A4C77F5504C08D60E2694DCEED2F92084664CD`
- classic FBX: `525B6FB2DAC0106C001BCB0901ED892DBBACA48CA1DF6DDB78702F9B2B72AE01`
- thorn FBX: `03B30979A7EA686AA6CA29D436BA434FD91D15BECDEE8BD2C0377A1B8B470579`
- crown FBX: `C66E6BBAAB853011FBDA3077566A58D516248230FC5DE7CA6554661EEDCE345A`
- 512px source icon: `8C07FE074C8677446DDF5010B8C9B121BDE48BBCB7EB5E59579062E5CB9B1D0C`
- 128px runtime icon: `A4CAA5FED242BEE645AD4F9D1E5F201C372EDE4A066254EE6BD4003A6538AF99`
- Unity bundle: `33EB89C74EC4AE7CDA5A8155224A449233904B74CB59FC453C24AE022EE3CB2A`

Two clean Blender runs produced byte-identical FBXs and normalized PNGs. The
`.blend` binary is deliberately not claimed byte-stable because Blender embeds
session metadata; regenerating it produces the same authored object and
geometry contract. The generator's build report records the current container
hash and this limitation.

The earlier visual repair changed a human-rejected 111,659-byte bundle to a
113,269-byte three-prefab bundle. The current orientation/carry repair leaves
all source FBXs and recorded source hashes unchanged and changes only
project-owned Unity wrappers and manifest data. Two independently restaged
Unity 2018.4.10f1 builds produced the identical current 126,658-byte six-prefab
bundle. It contains the same 45 source mesh objects and 15 material definitions
with no texture asset. The superseded three-prefab bundle SHA-256 was
`F671904DDB492EA194C259889D18BC4916E161E107C5E9F179A375DDF87B5B85`.

Existing item, weapon-type, proficiency, effect, category, placement, and save
identities are unchanged. Runtime maps exact item symbols to approved variants
and retains the classic type-level/native Longspear fallback. Human in-game
acceptance of point direction, attack grip, back silhouette, body-size clipping,
and inventory presentation remains a separate gate.
