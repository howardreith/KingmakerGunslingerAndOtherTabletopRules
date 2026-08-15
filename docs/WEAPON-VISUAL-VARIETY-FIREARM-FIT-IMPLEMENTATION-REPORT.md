# Weapon Visual Variety, Firearm Fit, and Proficiency Implementation Report

Status: automated pre-review qualification passed; human Musket/pistol/spear/blade
visual comparison is the active gate. This is not a completion claim.

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
mesh. The audit was regenerated after production Pistol bindings were added.
Its current exact identities are:

- JSON SHA-256:
  `449EEA20FDD1D486A31F28D81F399702EDB9BAD12BA68FBA83842BE5F213D5DB`;
- Markdown SHA-256:
  `02FE494041BF1C08F9DB161E2552B9858F0FA219621C3CB98DF3956A6BAF1983`.

Focused audit tests and the complete dependency-free suite pass 1,064/1,064.
No game process was launched for the documentation/audit generation itself.

## Workstream C2 - exact firearm item variants

`WeaponVisualVariantCatalog` now covers all 14 equipped firearm item symbols.
The approved pre-review vocabulary is bounded to seven exact variants: five
family Service variants plus `Pistol.Duelist` and `Pistol.LastWord`. Long-gun
items intentionally remain on their accepted Service prefabs until the Musket
fit gate is resolved; no diagnostic Musket candidate is production-bound.

The only non-Service firearm assignments are:

- `KMG.Firearms.DuelistsRebuttalItem` -> `Pistol.Duelist` -> prefab
  `PistolDuelist`;
- `KMG.Firearms.TheLastWordItem` -> `Pistol.LastWord` -> prefab
  `PistolLastWord`.

All other exact symbol-to-variant assignments, item/type GUIDs, effective
prefabs, and sources are the firearm rows in
`docs/weapon-visual-mapping-audit.json`. No blueprint identity, weapon type,
mechanic, animation donor, inherited attachment slot, or save-state contract
changed. The item-level binding clones the inherited private visual recursively,
replaces only its model, and retains the type-level native/custom family visual
as the no-bundle fallback.

The project-owned clean-room source is
`assets-source/original-models/firearm-pistol-variants/firearm-pistol-variants.blend`,
generated by `generate_firearm_pistol_variants.py`. Production FBXs and exact
hashes are:

- `pistol-duelist.fbx`:
  `D39F645A949CC8F42386FE852C632A360B50F7E19C13BEDDEDA9714F01B8BBE3`;
- `pistol-last-word.fbx`:
  `BB8CCB51034D2EE66C293E7E5D7BEEC3F0F17340CF7660DD21CB220091AFFDEB`.

Both author grip `(0,0,0)`, support `(0,-0.020,0.145)`, butt
`(0,0,-0.075)`, and muzzle `(0,0,0.264)` on the +Z firing axis. The Unity
importer requires exactly one of each marker even for these one-handed authored
assets, while runtime intentionally creates no support-hand IK target. Two clean
Blender 4.5.10 LTS runs matched both FBXs and normalized source renders exactly;
the `.blend` container remains semantic-only reproducible because it embeds
session metadata.

Two unchanged-input Unity 2018.4.10f1 builds matched at:

- bundle SHA-256:
  `CC9DA6B2FB43FD2932971E3CCE015610497E4C2DB657F62DBA675A31DE327B20`;
- bundle size: 17,992,788 bytes;
- prior bundle size: 17,960,137 bytes;
- growth: 32,651 bytes;
- prefab count: 11 to 13;
- added source meshes: 17 (8 Duelist, 9 Last Word);
- added source materials: 6;
- added textures: zero.

The fail-closed runtime firearm observer now checks all 14 exact item visual
identities, three distinct Pistol prefab references, seven variant instances,
renderability, the authored marker frame, absence of one-handed support IK, and
exact cleanup. Repository validation, all 1,064 dependency-free tests, the clean
Release build, build-output validation, deterministic packaging, and strict
package validation pass. The source-space renders are
`renders/pistol-duelist-source.png` and `renders/pistol-last-word-source.png`;
Kingmaker-scale readability and hand fit remain a human visual acceptance item.

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

## Immutable pre-review candidate and coherent commits

The current technical candidate is source commit
`041e934dc85d14512d9353f479d006a39065bd42` on
`codex/weapon-visual-variety-firearm-fit-cleanup`. The coherent commit sequence
from base `d846d8abed5281d1979cf33900ebef77e72e0a8b` is:

1. `3540ebd` - Make firearm proficiency Gunslinger-only.
2. `0c3d42b` - Audit every custom weapon visual mapping.
3. `1d08cbf` - Add deterministic branched spear variants.
4. `07c2a27` - Add deterministic Eastern weapon variants.
5. `37a937b` - Add Musket fit graybox experiment.
6. `5b8d18f` - Add immutable artifact repair qualification.
7. `041e934` - Validate exact Eastern item variants at runtime.

The final commit remains intentionally unset until the human gate and any
evidence-directed Musket/Blunderbuss iteration are complete. No merge occurred.

The immutable installed artifact used for every accepted pre-review runtime run
has these exact identities:

- version: `0.0.80`;
- package:
  `artifacts/local-runtime/0.0.80/KingmakerGunslinger-0.0.80-local-runtime.zip`;
- package SHA-256:
  `847604574E84BA98BF55E77489FD8CF9276AFE07473B676AF31B72B2AC1D8797`;
- DLL SHA-256:
  `27BCD2F725B369DB4C46F0227AE03D91E4DEC92B941CE5B351EC01E4C2FC0A9D`;
- DLL MVID: `24a38b51-b2f8-4a2e-b42b-edff7e32b1a8`;
- installed DLL SHA-256: identical to the packaged DLL;
- firearm bundle SHA-256:
  `BD78F647966271D826C16D5FD93BD481EA1953E48CE66D9E9313ABBFED15B152`;
- all-ON settings SHA-256:
  `2E53FA0A09C56662434F6EA548FF5EBCF91F5AAF293D668248221239A1308655`;
- deployment manifest:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260815T0529185947906Z/deployment.json`;
- pre-deployment backup:
  `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260815T0529146394604Z`.

`scripts/Build-Local.ps1` built, tested, packaged, and strictly validated this
source commit once. `scripts/Deploy-Local.ps1` backed up and deployed it once.
Every later launch used `-ReuseInstalledArtifact`, which reverified commit,
version, package hash, DLL hash, MVID, installed DLL hash, firearm bundle hash,
and the request-local/original settings identity before Steam launch.

## Final-candidate automated qualification to the human gate

The following commands and results are accepted for commit `041e934`:

- `scripts/test-domain.ps1 -Configuration Release`: PASS, 1,061/1,061.
- `scripts/build.ps1 -Configuration Release -Clean -Package`: PASS, including
  repository validation, complete domain/reflection suite, clean Release build,
  build-output validation, deterministic packaging, and strict package
  validation.
- `scripts/Build-Local.ps1`: PASS once for the immutable commit, including the
  exact private-reference Release build and focused supply-icon test.
- `scripts/Test-RuntimeDeployment.ps1`: PASS, 19/19, for immutable reuse and
  deployment-manifest behavior.
- Blender generators were run twice from clean inputs: all production FBXs and
  normalized renders matched; `.blend` session metadata remains the documented
  non-byte-stable exception.
- exact Unity 2018.4.10f1 bundle builds were run twice from unchanged inputs:
  firearm, Eastern, and spear bundle hashes matched their manifests.

Guarded Steam App ID 640820 runtime PASS evidence:

| Scenario | Run ID | Evidence directory |
|---|---|---|
| all-ON module load | `20260815T0529384705552Z-3b8227ba790a4abdbbac402136238a7e` | `20260815T0529384337593Z-mod-load-smoke` |
| firearm production/diagnostic visual rigs, markers, IK, cleanup | `20260815T0531581755461Z-db3d4b910d2b46c6a5ed3c921bef23ef` | `20260815T0531581440963Z-disposable-firearm-visual-rigs` |
| exact all-30 Eastern item observer | `20260815T0534097873347Z-91c2116f10124d97a30d6c737147cbd3` | `20260815T0534097525070Z-observe-eastern-weapon-contracts` |
| Eastern disposable combat/cleanup | `20260815T0537205906818Z-8a9bf8e85e924ebcb45a3c74531ab93b` | `20260815T0537205608781Z-disposable-eastern-weapons-combat` |
| exact all-12 branched-spear observer | `20260815T0539303961608Z-78c31f3df4dd431899c2d36d0a3d7c8b` | `20260815T0539303715105Z-observe-elven-branched-spear-contracts` |
| branched-spear disposable combat/cleanup | `20260815T0541426733415Z-f0ff47492bb14b5c87386f57c9ebded6` | `20260815T0541426471390Z-disposable-elven-branched-spear-combat` |
| canonical `KMG_AUTOMATION_WORKING` catalog/load/fingerprint, with no save-writing API | `20260815T0543492604693Z-6ab479731480431a8e7ec3fb060f8030` | `20260815T0543491837527Z-working-save-smoke` |
| isolated highest-risk optional-mod profile | `20260815T0549310463372Z-aeea181ff79e4cfe968dd10c3d1263bb` | `20260815T0549310271669Z-observe-optional-mod-compatibility` |
| Eastern OFF, every other module ON, highest-risk profile | `20260815T0552181340583Z-1e7629dac21e4d238c3b78498b1898f3` | `20260815T0552181084011Z-observe-feature-module-settings` |

All evidence directories are beneath
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/`. The isolated compatibility
profile was exactly Gunslinger + Call of the Wild + Arms & Armor + Toggle
Custom Soundpacks. Both compatibility transactions restored the original mod
set; the Eastern-OFF transaction also restored the exact original all-ON
settings hash. A preceding direct observer request correctly failed because the
ambient local set was Gunslinger + Call of the Wild + Bag of Tricks rather than
the requested profile; it is retained as negative setup evidence at
`20260815T0546260048618Z-observe-optional-mod-compatibility` and is not counted
as product qualification.

The first pre-fix immutable launch also failed closed because the new exact item
visual had exposed a stale validator which still required family-level visual
reference equality. Commit `041e934` replaced that obsolete condition with the
exact symbol-approved variant contract while preserving the native fallback
when the bundle is unavailable. The full suite and the exact all-30 runtime
observer independently prove the correction. Negative evidence remains at
`20260815T0521306651467Z-mod-load-smoke`.

No runtime test selected or wrote `KMG_AUTOMATION_BASELINE`. Only the explicitly
authorized `KMG_AUTOMATION_WORKING` save entered the save-backed run. Disposable
fixtures reported exact cleanup. Existing firearm, Eastern, and spear item and
weapon-type identities were not replaced; the mapping audit is the complete
symbol/GUID-to-visual appendix:
`docs/weapon-visual-mapping-audit.json` and
`docs/WEAPON-VISUAL-MAPPING-AUDIT.md`.

## Remaining human gate and deferred final seal

Automated structural, mechanical, identity, persistence, asset-loading, and
compatibility qualification is green. Human visual acceptance is **PENDING**
for these subjective facts:

- whether Minimal Control clears the torso when Pass-Through clips;
- whether Clearance Stock materially improves torso/upper-arm clearance while
  retaining physical primary/support-hand contact through idle, attack,
  recovery, switching, inventory-doll refresh, save/load, and restart;
- whether `PistolDuelist` and `PistolLastWord` remain distinct and readable at
  ordinary gameplay zoom while preserving primary-hand contact and muzzle axis;
- whether the Classic/Thorn/Crown spear branches remain unmistakable and
  elegant at ordinary gameplay zoom without hand/arm intersection;
- whether the bounded Eastern blade variants read as distinct within their
  preserved Wakizashi/Katana/Nodachi silhouettes.

The exact comparison procedure and acceptance matrix are in
`docs/FIREARM-FIT-GRAYBOX-QUALIFICATION.md`. Source-space aids are committed at
`assets-source/original-models/firearm-fit-experiments/renders/`; required
matching in-game screenshots are still absent from the local ignored path
`evidence/screenshots/weapon-fit/0.0.80/` and are not fabricated here. Residual
clipping by pose/body type and the final long-gun clearance envelope cannot be
truthfully completed before that comparison.

Blunderbuss E5 remains gated on the Musket result. If Minimal clears, the
clearance-stock method proceeds to a distinct Blunderbuss graybox and envelope;
if Minimal clips substantially like Pass-Through, the frozen pose/animation is
the dominant limitation and animation escalation requires explicit approval.

Per the repair policy, the 14-state boundary matrix for the single-module feat
publication change and any final release seal are deferred until the visual
candidate receives human acceptance. This prevents paying the final matrix cost
for a candidate that human review may send back for geometry iteration.
