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
- Source coordinate contract: metric; grip at origin; physical point along +Z;
  head-face normal +Y; support station +0.593016 m
- Unity held coordinate contract: full source basis maps to the measured native
  Longspear held basis, with the grip translated to the weapon-bone origin
- Unity stored coordinate contract: full source basis independently maps to the
  measured native BeltModel basis, with renderer center at the donor anchor
- Bounds contract: butt -1.14 m, point +1.14 m, total 2.28 m
- Unity held prefabs: `ElvenBranchedSpear`, `ElvenBranchedSpearThorn`, and
  `ElvenBranchedSpearCrown`
- Unity back prefabs: `ElvenBranchedSpearBack`,
  `ElvenBranchedSpearThornBack`, and `ElvenBranchedSpearCrownBack`

SHA-256:

- generator: `F9977A854176D047D0B0DF4C32C960CF96DD0E99BD4BAE608E1E7E5B3750274F`
- classic FBX: `A7FE4DEE53B18D1778D994F8B24A349B22C000E87660934B882D239A0F807E3A`
- thorn FBX: `3EC09E5A662991944F5B41E01852A8ABCB3A040506481D3789E1E3F94C0F430B`
- crown FBX: `EA5B392F95AADA371185188021A8935C26621C8CE20C485014C215ADE4BA9443`
- 512px source icon: `8C07FE074C8677446DDF5010B8C9B121BDE48BBCB7EB5E59579062E5CB9B1D0C`
- 128px runtime icon: `A4CAA5FED242BEE645AD4F9D1E5F201C372EDE4A066254EE6BD4003A6538AF99`
- Unity bundle: `A59DC61CE246A7F5931F22494C4C52CE39C6E96312F3448FB9138A0AC0D7DC9B`

Two clean 2026-08-21 Blender runs produced byte-identical FBXs and normalized
PNGs and identical schema-3 semantic reports after excluding only the recorded
`.blend` session-container hash. The
`.blend` binary is deliberately not claimed byte-stable because Blender embeds
session metadata; regenerating it produces the same authored object and
geometry contract. The generator's build report records the current container
hash and this limitation.

The current calibration adds source-authored semantic empties without changing
the 45 rendered mesh objects, 2,700 triangles, 15 material definitions, or
icon pixels. Two independently restaged Unity 2018.4.10f1 builds produced the
identical current 127,369-byte six-prefab bundle. Its held/back pairs derive
from measured native donor bases rather than the superseded fixed-Euler bundle
SHA-256 `33EB89C74EC4AE7CDA5A8155224A449233904B74CB59FC453C24AE022EE3CB2A`.

Existing item, weapon-type, proficiency, effect, category, placement, and save
identities are unchanged. Runtime maps exact item symbols to approved variants,
adds held-only left-hand IK, and retains the classic type-level/native
Longspear fallback. Guarded default-Medium-male held, stored, combat-ready, and
thrust acceptance is recorded in
`docs/ELVEN-BRANCHED-SPEAR-VISUAL-CALIBRATION.md`; broader body/movement
coverage remains a separate final gate.
