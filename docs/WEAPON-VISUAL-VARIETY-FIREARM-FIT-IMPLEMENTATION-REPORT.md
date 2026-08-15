# Weapon Visual Variety, Firearm Fit, and Proficiency Implementation Report

Status: in progress; this is the durable mission record, not a completion claim.

## Baseline

- Source branch: `codex/eastern-weapons` (same accepted commit as its merged
  successor `master` at mission start).
- Base commit: `d846d8abed5281d1979cf33900ebef77e72e0a8b`, `Seal first
  playtest repair qualification`.
- Mission branch: `codex/weapon-visual-variety-firearm-fit-cleanup`.
- Initial working tree: clean.
- Version and package identity: `0.0.80`, `KingmakerGunslinger`, package
  `KingmakerGunslinger-0.0.80-eastern-weapons.zip`.
- Firearm bundle: `kingmakergunslinger.firearms`, baseline SHA-256
  `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`.
- Eastern bundle: `kingmakergunslinger.easternweapons`, 147,724 bytes,
  baseline SHA-256
  `F58801B7B34514B06577EA9CE36F2F3FC0A79A6F157113EA227251BFE2A15B43`.
- Elven Branched Spear bundle: `kingmakergunslinger.elvenbranchedspear`,
  87,627 bytes, baseline SHA-256
  `3AB56092F363AA96C627287095E2CA549EEA7ED50D39C73BCD943646BFBE0EBE`.

The accepted Eastern runtime qualification was sealed for functional source
commit `5e99d4d7555d9d96efd7bd79714161003e314013` and subsequently documented
through base commit `d846d8a`. It passed 1,048 dependency-free tests, the exact
all-30 item observer, combat and spear scenarios, enabled/disabled profiles,
current optional-mod profiles, persistence, working-save smoke, and the then
required 64-state matrix. The last subjective Eastern second-review gate was
still recorded separately; no prior visual acceptance is being fabricated here.

## Qualification policy for this mission

The user-supplied repair policy supersedes routine exhaustive-matrix practice.
The firearm feat publication change is a single-module selector-publication
change and therefore requires focused mechanics, relevant optional-mod and
persistence checks, plus the 14-state boundary matrix. Model/bundle changes
require focused family visual contracts, Eastern Weapons ON/OFF, all modules ON,
and the highest-risk combined compatibility profile. A final exhaustive release
seal will not run during human visual iteration.

## Workstream A — Gunslinger-only firearm proficiency

Implemented source behavior:

- `KMG.Firearms.FirearmProficiency` remains the sole hidden full-proficiency
  fact with GUID `5148f69223044799800b65732b6cabea`.
- Scoped facts remain GUIDs `1c6f66f734f64535a7de50030023a0dd` and
  `cbf3d4e79b6144b9b76480d8b242d37c`.
- The legacy wrapper remains registered at GUID
  `b1a58cfdbf004f04ade7765373484c29`, remains readable on existing character
  sheets, and retains its exact one-fact full-proficiency grant.
- The wrapper has no feat groups and is removed by exact identity from every
  discovered feature selection's `Features` and `AllFeatures` arrays.
- Rapid Reload alone is appended exactly once to the basic and Fighter catalogs.
- Publication rollback restores the exact original array references for every
  touched selector.
- Guarded runtime coverage now checks a real detached ordinary owner, a real
  detached legacy owner with `AddFact` propagation, normal level-one Gunslinger
  class grants, and exact Pistolero, Musket Master, and Mysterious Stranger
  proficiency scopes.

Automated evidence at the uncommitted workstream candidate:

- `scripts/test-domain.ps1 -Configuration Release -Clean`: PASS, 1,050/1,050.
- `scripts/build.ps1 -Configuration Release -Clean -Package`: PASS.
- Strict standalone package validation: PASS.
- Candidate package SHA-256:
  `E07F60313C48CAC907441D761DE14F664F562BC76CCD9C840FF389779B739B1A`.
- Candidate DLL SHA-256:
  `B0EEF900F85C193B70A03AE1CF2F3730DE4CD1366F19AFE0025B284F2DF8257E`.
- Candidate DLL MVID: `c31d53fa-a337-44b9-9374-5e2035bbb84a`.

Runtime evidence is pending an immutable commit and exact-artifact deployment.
The visual mapping, asset variants, semantic markers, fit experiments, bundle
measurements, screenshots, residual clipping table, final commit list, and human
acceptance status will be appended in their respective coherent workstreams.

## Workstream B — complete pre-authoring visual audit

The deterministic generator `scripts/generate_weapon_visual_audit.py` produces:

- `docs/WEAPON-VISUAL-MAPPING-AUDIT.md`;
- `docs/weapon-visual-mapping-audit.json`.

Coverage is the complete active manifest set: 68/68 `BlueprintItemWeapon`
identities and GUIDs. This consists of 56 equipped player/development weapons,
two unowned Pistol-Whip rule-event items, and ten Expanded Summoning
creature-weapon identities. The latter twelve are explicit preserve-only
exclusions rather than silently omitted cosmetic targets.

The audit records every required identity, family/type, inherited item and type
visual, effective prefab, source FBX/archive, Blender source and generator,
animation donor/style, grip contract, material, bundle, provenance/license,
many-to-one group, proposed variant, clipping concern, and tier. Weapon-type
symbols and GUIDs are exact for project types; runtime native donor types are
identified explicitly where the project intentionally owns no replacement type.

The proposed vocabulary is exact by blueprint identity and bounded to five or
fewer variants per family. Enhancement increments alone do not force a distinct
mesh. Two consecutive clean generations matched:

- JSON SHA-256:
  `494E345981E5056B36ACAFDB38BFAE974E63D45F03DD71DC6958332A858059CF`;
- Markdown SHA-256:
  `79AD63DAE60B49C6B0D27DCF60EDD8B29DA336044CFB3821ADE2954D6DF11997`.

Focused audit tests and the complete dependency-free suite pass 1,053/1,053.
No game process was launched for this documentation/audit-only workstream.

## Workstream D — unmistakable Elven Branched Spear geometry

The project-owned Blender generator now produces a bounded three-variant visual
vocabulary without changing any item, type, category, proficiency, effect, or
save identity:

- `ElvenBranchedSpear.ClassicBranch`: two separated backward-swept prongs;
- `ElvenBranchedSpear.ThornBranch`: three staggered prongs;
- `ElvenBranchedSpear.CrownBranch`: four balanced prongs.

All branches have physical thickness, their tips are laterally separated from
the central blade, and every branch begins above the 1.47 m shaft-grip exclusion
boundary. The grip remains at the origin, support target at +0.48 m, butt at
-0.915 m, and central tip at +2.01 m on the preserved +Z weapon axis.

Exact deterministic FBX identities:

- classic: `80773756F2C403D8569FE811B049FC3B53AE1399FA83446A70710AF1F69833E5`;
- thorn: `2BE981892A5C08E96A018FC5CC9188311128725B5BB0FC545DA12E298205734F`;
- crown: `0FAF504CFDD5290E71993A484A77874AEEB2CB01B38174CC7635F716C345D99B`.

Two independent Blender 4.5.10 LTS runs produced byte-identical FBXs and
normalized source/runtime PNGs. Stable exporter UUIDs are SHA-256-derived; the
unused nondeterministic UV pack was removed. Blender's `.blend` container is
semantically regenerated but not falsely claimed byte-stable because Blender
embeds session metadata.

The exact Unity 2018.4.10f1 builder produced three prefabs:
`ElvenBranchedSpear`, `ElvenBranchedSpearThorn`, and
`ElvenBranchedSpearCrown`. Two unchanged-input builds matched exactly at 111,659
bytes and SHA-256
`6E9FE86E43072361EEC3357D9C73E17ADD71D22BAF257FB8C7ED6F52931CE777`.
The baseline was 87,627 bytes, so the bounded variants add 24,032 bytes. The
source set contains 45 mesh objects, 2,700 triangles, 15 material definitions,
and no texture asset.

`WeaponVisualVariantCatalog` is the centralized exact-symbol authority. All 12
spear item blueprints receive an inherited private `m_VisualParameters`
override through recursive field access; the weapon type retains the classic
custom/native Longspear fallback. The runtime loader rejects a missing,
duplicate, partial, nonrenderable, or implausible three-prefab set
transactionally. The combat observer now requires all 12 exact item mappings,
instantiates all three prefabs, validates anchors, and cleans every instance.

Automated evidence:

- repository/source validation: PASS;
- complete dependency-free suite: PASS, 1,054/1,054;
- clean exact-reference Release build and output validation: PASS;
- deterministic package creation and strict standalone validation: PASS;
- package SHA-256:
  `0856BFFE80EC513C1B69059BD4D9584E490E606F8951BB18E62CE1EFDB1DC13D`;
- DLL SHA-256:
  `DD2FF53B35F52C81CA84C8E5779419F52072D35914E669C257EBA39605300483`;
- DLL MVID: `cc6b28dc-492d-46c6-a464-85dd90e0342b`.

Runtime combat/idle/switch/save-load regression and human visual acceptance are
pending the mission's final immutable candidate. No in-game screenshot or
subjective clipping/readability PASS is asserted by automation.

## Workstream C — bounded Eastern blade variety

The project-owned generator now emits four reusable variants for each qualified
family:

- Wakizashi: `Classic`, `Petal`, `Moon`, `Capstone`;
- Katana: `Classic`, `Reed`, `Regal`, `Capstone`;
- Nodachi: `Classic`, `Cleaver`, `Titan`, `Capstone`.

Disc, petal, bar, wing, and crown guards; round, cap, spike, and crown pommels;
and bounded blade-width/curvature differences provide silhouette changes beyond
tint. The 30-item assignment is exact by blueprint symbol in
`WeaponVisualVariantCatalog` and duplicated verbatim in the machine-readable
audit. Cold-iron items share the first craftsmanship variant rather than
manufacturing a fifth mesh solely for material tier. Enhancement increments may
share `Classic`; each capstone uses its family's bespoke `Capstone` geometry.

All qualified family contracts remain fixed: Wakizashi 0.76 m, Katana 1.05 m,
Nodachi 1.58 m; grip origin; +Z blade axis; support target; handedness; category;
native animation donor; enchantment overlay behavior; and every item/type GUID.
Each item receives a recursive inherited private `m_VisualParameters` override,
while each weapon type retains its classic prefab and native donor fallback.

Source/build identities:

- generator: `assets-source/original-models/eastern-weapons/generate_eastern_weapons.py`;
- Blender source: `assets-source/original-models/eastern-weapons/eastern-weapons.blend`;
- 12 FBXs and hashes: `assets/bundles/asset-bundle-manifest.json`;
- 12 Unity prefab names: the same bundle manifest;
- bundle: `kingmakergunslinger.easternweapons`, 310,375 bytes, SHA-256
  `079AA2E44E313291C144BD830D302782310274B11375204F9CE8FF6481EF3041`.

The baseline three-prefab bundle was 147,724 bytes; nine additional
geometry/material prefabs add 162,651 bytes. The source contains 190 mesh
objects, 12,252 triangles, four material definitions per prefab, and no texture
asset. Two clean Blender runs matched across all 12 FBXs and 12 normalized PNGs;
two unchanged-input Unity 2018.4.10f1 builds produced the same bundle hash.

The existing all-30 observer is stricter: it resolves the inherited item field,
requires the exact symbol-approved prefab, proves the family/type relation and
all non-model native donor fields, verifies CuttingEdge material and enchantment
overlays, instantiates every exact prefab, and checks cleanup. Automated source
validation and the complete dependency-free suite pass 1,055/1,055; the clean
exact-reference Release build and output validation pass.

The coherent candidate package and strict standalone validation also pass:

- package SHA-256:
  `9C3E331F39BBF50AB35055C636B2C681228A6B19A3DD334DE0BD55386F8D102F`;
- DLL SHA-256:
  `36CDACFF566491BF9C293A532BECD44D5212C3A67285DB9E395861ADEC73CEFF`;
- DLL MVID: `6729b750-ea59-4679-b7bd-a66dc7d83e1a`.

Runtime Eastern ON/OFF, all-ON, highest-risk optional-mod profile, all-30 visual
observer, combat regression, save/load consistency, and human visual acceptance
remain pending the final immutable mission candidate.

## Workstream E - Musket geometry proof and semantic markers

The production Musket remains deliberately unchanged and continues to use
`Musket 01.fbx`, prefab `Musket`, inherited native attachment slots, Crossbow
animation, and the already-qualified identity grip frame. Three additional
bundle prefabs are diagnostic-only and are held in a separate runtime cache;
none can be selected by a firearm item or `FirearmPresentationProfile`:

- `MusketPassThrough`: licensed current geometry imported and re-exported
  through the deterministic Blender workflow;
- `MusketMinimalControl`: project-owned 256-triangle control with a real
  fore-end and almost no rear stock;
- `MusketClearanceStock`: project-owned 280-triangle complete graybox with a
  narrow, dropped, segmented rear stock.

Source paths, FBX hashes, mesh/material counts, metric bounds, and source-space
renders are exact in
`assets-source/original-models/firearm-fit-experiments/musket-fit-candidates-build-report.json`.
The generator is
`assets-source/original-models/firearm-fit-experiments/generate_musket_fit_candidates.py`;
the Blender source is `musket-fit-candidates.blend`. The pass-through retains
Mesh Masters attribution and CC-BY-4.0; the two grayboxes are clean-room project
work. No original licensed source was overwritten.

All three candidates author exactly one each of `KMG_Grip`, `KMG_Support`,
`KMG_Butt`, and `KMG_Muzzle`. The Unity importer reads these recursively and
fails closed on partial/duplicate markers, missing markers for an authored
asset, non-finite values, wrong muzzle axis, implausible length/scale, support
outside the weapon envelope, or an empty visible hierarchy. Markerless legacy
sources retain the existing hardcoded fallback. The runtime result for every
candidate is the exact fixed Musket frame: grip `(0,0,0)`, support
`(-0.030976,-0.051069,0.586040)`, butt `(0,0,-0.169533)`, muzzle
`(0,0,1.180452)`, length `1.349985 m`. Runtime preparation retains the native
left-hand IK binding to `SupportHandTarget`.

Two clean Blender runs matched across all three FBXs and three normalized PNGs.
Blender `.blend` session metadata is explicitly not claimed byte-stable. Two
unchanged-input exact Unity 2018.4.10f1 builds matched:

- bundle SHA-256:
  `BD78F647966271D826C16D5FD93BD481EA1953E48CE66D9E9313ABBFED15B152`;
- bundle size: 17,960,137 bytes;
- baseline size: 16,184,635 bytes;
- growth: 1,775,502 bytes;
- prefab count: 8 to 11;
- candidate source mesh counts: 2 pass-through, 8 minimal, 10 clearance;
- candidate source material counts: 2, 3, and 3;
- textures added: zero.

Growth is principally the pass-through candidate's 24,651-vertex licensed mesh
plus Unity-generated reverse-wound two-sided meshes for all 20 candidate mesh
renderers. The two project grayboxes themselves are intentionally small.

The development calibration lab can select each diagnostic on an exact equipped
Musket through the native world equipment refresh and can restore the production
Musket. It states, rather than conceals, that inventory-doll acceptance requires
closing and reopening inventory for a clean rebuild. The standardized manual
matrix, source-render paths, screenshot naming, decision rules, and provisional
clearance envelope are in `docs/FIREARM-FIT-GRAYBOX-QUALIFICATION.md`.

Automated source/reflection evidence currently passes 1,059/1,059 tests. The
Unity build itself is the integration test for authored marker transforms and
its log proves all three resolve to the exact fixed frame. Final repository,
build, package, and guarded runtime results will be recorded after the coherent
immutable commit.

Human Musket geometry verdict: **PENDING**. Automation does not claim reduced
clipping. The source renders are design aids, not Kingmaker screenshots.

Blunderbuss E5: **NOT STARTED BY DESIGN**. The mission explicitly gates it on
the Musket experiment's live conclusion, so building a speculative Blunderbuss
before that result would violate the evidence rule.
