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
