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
  `7969E03980FEE42CA000AA561F07C72401AD24AC786C1134C7FF86EB6FF51AA4`;
- Markdown SHA-256:
  `6D7FD2CAA36E672BDD4514928C46475D43F1CD5E54E434412CCE453CC53390F2`.

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
