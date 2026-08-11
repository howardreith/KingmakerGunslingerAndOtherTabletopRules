# Expanded Summoning journal

## 2026-08-11 - baseline and mission intake

- Intended repository confirmed at `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger` with remote `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Fetch completed. `origin/master` at `2894d9fcce250708e354894ffd8e1be9c7493b9b` is the newest qualified non-experimental descendant of required baseline `e4d560f8dd2909518614e3a20e77ba4d70dadeb8`.
- Created `codex/expanded-summoning` from that merged 0.0.77 baseline.
- Mandatory inspection proved `ShieldOtherLinkValidityPolicy` still removes an established bond outside close range. Separate prerequisite repair is next.
- GitHub CLI exists but its stored token is invalid. Work and SSH publication continue; draft-PR credential state will be rechecked after all local qualification.

## 2026-08-11 - Shield Other prerequisite qualified

- Removed all post-cast distance dependence from `ShieldOtherLinkValidityPolicy`.
  Close range remains on `ShieldOtherBlueprints` for initial targeting.
- Preserved missing/dead endpoint, missing caster-level, and area-separation
  termination. Existing removal/dispel and duration behavior is unchanged.
- Added focused extreme-distance and unavailable-distance regression cases plus
  a source contract forbidding reintroduction of established-link distance use.
- `git diff --check`: PASS.
- Repository validation: PASS for 0.0.77.
- Complete domain suite: 981/981 PASS.
- Clean Release build and strict standalone package: PASS.
- DLL SHA-256: `6cc7d0186f7b5d57b58644bffb2fc23c71feb898816bdea0da2acf63954f29b0`.
- Package SHA-256: `6d097f33e70cfce3364a015d9e59c541d14444cbaf55f082314d47e026f0d431`.

## 2026-08-11 - frozen logical catalog

- Added immutable family, multiplicity, template-policy, creature, and variant
  specifications without allocating or guessing runtime blueprint identities.
- Catalog self-validation proves 67 unique creature keys, 66 SM entries, 57 SNA
  entries, 361 SM placements, 320 SNA placements, and 681 total placements.
- Variant generation maps current tier to one, immediately prior tier to 1d3,
  and all lower eligible tiers to 1d4+1 while retaining one creature key.
- SNA always uses caster-alignment policy; SM template policy is explicit per row.
- Complete suite: 985/985 PASS. Repository validation, clean Release build, and
  strict standalone package validation PASS.

## 2026-08-11 - guarded final-live inventory observer

- Added save-free `observe-expanded-summoning-inventory` to the managed and
  PowerShell guarded allowlists.
- Observer records final-live summon-family abilities and component fields,
  roster-matched unit donors and view/body/fact references, and native summon
  feat/template/pool candidates. It performs no blueprint or save mutation.
- Runtime preflight: 86 checks PASS. Repository validation, 985/985 domain tests,
  clean Release build, and strict standalone package validation PASS.
- Exact next step is a clean-commit Steam App ID 640820 observer run.

## 2026-08-11 - final-live parent inventory and publication policies

- Guarded run `20260811T1727529145302Z-observe-expanded-summoning-inventory`
  PASS on a fresh Steam process. No save was loaded or written.
- Curated all 18 canonical SM/SNA parent GUIDs in
  `planning/EXPANDED-SUMMONING-INVENTORY.md`; broad name matching is explicitly
  rejected because the CotW-composed graph contained 523 summon-named abilities.
- Added pure additive merge and multi-parent transaction policies. Tests prove
  reference/order preservation, GUID/reference singularization, idempotence,
  exact rollback, setter-mutate-then-throw recovery, and refusal after unrelated
  later mutation.
- The first rollback fixture exposed missing journal-before-write handling; the
  transaction was corrected and the complete suite now passes 989/989.
- Exact-name narrowing identified nineteen dedicated summon-unit donor candidates
  and six fallback visual donors. Broad substring false positives (notably Roc)
  are rejected; the next phase audits exact component/fact/view structures.

## 2026-08-11 - summon-unit sanitizer contract

- Added a pure, explicit sanitizer policy covering XP, loot, inventory,
  interaction/dialogue, story, companion/pet, persistence, teleport/planar
  travel, nested summoning/conjuration, expensive material components, and
  persistent corpses.
- Unsafe donor members are removed. If such a member represented a required
  combat mechanic, the plan records an explicit safe-replacement obligation
  rather than silently dropping or retaining it.
- The policy preserves safe donor-member references and rejects null or
  duplicate inventory identities. Complete suite: 992/992 PASS.
- Repository validation, clean Release build, and strict package validation
  pass. DLL SHA-256: `d871074d841ebf918a864518c1b4f7b6419936d9b3dbca4b03a7a75d391b7dc1`;
  package SHA-256: `52339044928028e2c049154303145c915f324db668623c9b7e8fefbc83e3b5d6`.
