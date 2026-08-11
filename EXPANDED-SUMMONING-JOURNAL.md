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

## 2026-08-11 - exact donor runtime audit

- Published observer commit `df65c391365ce52367f05b457e6f2bc6a61a3a09` and ran
  guarded save-free scenario `20260811T1741299016346Z-observe-expanded-summoning-inventory`.
- Scenario PASS; all 25 exact donor GUIDs found. Nearly every dedicated summon
  donor retains XP and several retain loot, proving clones must be sanitized.
- Campaign visual donors retain hostile factions and campaign components and
  remain visual-only. Curated elemental and wolf identities were added to the
  inventory; raw runtime diagnostics were not committed.

## 2026-08-11 - native action graph and identity plan

- Guarded save-free run `20260811T1747085341434Z-observe-expanded-summoning-inventory`
  PASS on exact published source `ba6500f16b06cf8adb9c5d32149929137e1d98e2`.
- All 18 canonical parents and 48 direct children confirmed native spawn pool,
  caster-rank-round duration, tier cleanup buffs, alignment conditionals, and
  `ProjectilesCount` quantity semantics.
- Added a deterministic foundation identity plan: 67 units, 681 logical
  placement abilities, 364 celestial/fiendish execution abilities for 182
  templated placements, and four standalone template buffs (1,116 total).
- Complete domain suite: 995/995 PASS. GUIDs remain unallocated until runtime
  registration and the validator-count migration land in the same phase.

## 2026-08-11 - append-only foundation reservations

- Added `tools/expanded_summoning_manifest.py` with allocate-once and repeatable
  validation modes. It derives the frozen roster/quantity plan and detects
  missing symbols, wrong planned types/statuses, malformed GUIDs, collisions,
  and count drift.
- Allocated 1,116 random lowercase GUIDs once and appended them as reserved.
  No existing symbol or GUID changed. Exact ledger: 254 active, 1,117 reserved,
  1,371 total. Runtime registration remains exactly 254 until implementation.
- Allocator check, repository validation, and 995/995 domain tests PASS.

## 2026-08-11 - fourth feature-module domain integration

- Advanced feature settings to schema 3 and added independent, default-enabled,
  restart-bound `expanded-summoning` state without changing active snapshots
  when pending UI values change.
- Schemas 0, 1, and 2 migrate atomically; explicit Gunslinger, Acadamae
  Graduate, and Shield Other values survive while the absent new module defaults
  ON. Malformed recovery still quarantines bytes and defaults all four ON.
- All 16 Boolean configurations round-trip and the pure publication plan gates
  only Expanded Summoning parent publication. Runtime matrix plumbing remains
  the next focused phase.
- Repository validation, 995/995 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `056ff668dd189b50e2d9102cc29223f37fdc1d5673c56d9d67fb259ca5452541`;
  package SHA-256: `2680b476e25bfe668e087525f418152c187e92254043e09bf28ab8db0702f62d`.

## 2026-08-11 - guarded 16-state runtime plumbing

- Replaced the legacy hard-coded eight-state runtime matrix with deterministic
  enumeration from one ordered four-module catalog. Every transaction writes
  schema 3 and still restores the original settings bytes exactly.
- Guarded request creation, request validation, compatibility-profile setup,
  and the native observer now require and report `expandedSummoning` alongside
  the three established module states.
- The observer proves the active restart snapshot and feature-local gate now;
  exact parent publication/count assertions will replace that provisional gate
  assertion when the reserved summon identities are activated.
- Repository validation, 996/996 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `d4c097f6137116eb7e226dd275de996afcfedccaa8fb5992f90d309e9194f8d4`;
  package SHA-256: `8533ae5bca74359485fcff0e780e97529b7d9013690c7efe1f6203f5b6fb767c`.

## 2026-08-11 - constant all-state identity registration

- Activated all 1,116 frozen Expanded Summoning manifest identities without
  changing a GUID: 67 units, 681 logical abilities, 364 aligned execution
  abilities, and four template buffs. Exact ledger and runtime registry contract
  are now 1,370 active plus one historical reservation.
- Added a deterministic registration layer that consumes only frozen symbols and
  uses hidden, unpublished donor-shaped shells. The feature gate does not control
  registration, so disabled-state save deserialization remains possible.
- These shells are intentionally unreachable from live spell parents and are not
  claimed as sanitized or mechanically complete; exact unit builders and spawn
  actions are the immediate next phase before any publication.
- Repository validation, 996/996 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `8646b6614e3bded1e17498dc68655580e36cf5adbad9babfc124eee629a68fcd`;
  package SHA-256: `ee68ba029abff8202c20e0e8e536f97f30051b55a604411fa90f1cc2a3c3352a`.
