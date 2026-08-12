# Expanded Summoning journal

## 2026-08-12 - immutable-source final runtime qualification

- Pushed immutable runtime source
  `b8e7950680ab4718c18965837a37c5974a8d35bc` contains 1,155 Expanded
  Summoning identities; repository registration is exactly 1,409 active plus
  one reserved in every feature state.
- Final structural run
  `20260812T1149141950160Z-0dfc7143323b4095a01dea690e43c2c0` PASS (30/30).
- Final mechanical run
  `20260812T1303503740041Z-73e7a3e6a825468d91f5a9fd7e970889` PASS: all 153
  production commands, 123 logical one-creature entries, every required
  family/tier quantity case, same-kind counts, templates/alignment, summon
  feats, special combat, and cleanup.
- Final visual run
  `20260812T1151394827201Z-add45a04f5de44c1a39e3251f7ff0778` PASS for 67/67
  unique units and all ten structural visual contracts.
- Enabled persistence runs
  `20260812T1155220523013Z-6d2a18f9b33344d08d3127ffce7e5cb6`,
  `20260812T1158042311459Z-f21e3713df3b4545add5e7ab3436e865`, and
  `20260812T1200508004220Z-02af221881da449f827cf5a048ba6c67` PASS.
- Disabled-publication persistence runs
  `20260812T1203346138814Z-0c81aa138d344d8e867d84d0ec316564`,
  `20260812T1206142978812Z-a085c35bc5174b9d9bdd2633826436d3`, and
  `20260812T1208449380302Z-65c9b7056d97483fb48a4a9b76c22ea6` PASS. Active
  identities loaded safely while new KMG publication was zero.
- All 16 module configurations passed fresh launches with constant 1,409
  registration and isolated publication surfaces.
- Standalone and Call of the Wild passed twice; Arms and Armor and Toggle
  Custom Soundpacks passed once; the highest-risk combined profile passed
  twice. Every profile transaction restored Mods and settings exactly.
- Settings, working save, and protected baseline hashes were restored and
  verified; `KMG_AUTOMATION_BASELINE` was never modified.
- Next: deterministic documentation-bearing package/hash freeze, curated final
  evidence commit/push, draft PR, and requirement-by-requirement audit.
- The documentation-bearing release source was committed and pushed as
  `193d73cc22fe41fda8546f1d2e1750e185ed8288`. Repository validation and the
  complete `1009/1009` domain suite passed before the commit.
- Two clean exact-reference `Build-Local.ps1` executions produced identical
  DLL SHA-256
  `64bc093904ea80514b7811ab73ef488c3c7561ab5af049f7ba08e74d8c177966`
  and deterministic package SHA-256
  `2dde3ce858397cf27e86d01b9f69b68ececb05e0127386cd31d3fd22caa739ce`.
  The canonical 45-file Expanded Summoning ZIP was regenerated with the same
  deterministic writer and strict package validation passed.
- Deterministic `git archive` source ZIP SHA-256 for release source `193d73c`
  is `c698e82d38599e06c58a32a9b243c391c9e9a4cb155b6047dee4d5ef936cf784`.
- Exact next action: commit/push these hashes, open the draft PR, and complete
  remote-equality plus definition-of-done audit.
- Hash evidence was committed and pushed as
  `e5aba193c0eb421a41d4441fc2ae7b9f7bc5358d`.
- GitHub app publication opened draft PR
  [#2](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/2)
  against `master`; the initial PR head exactly matched `e5aba193c` and the PR
  contains all 99 feature-branch commits then present.
- Exact next action: publish this final PR record and prove clean-tree,
  local/origin, and PR-head equality before closing the mission.
- Completion audit found that all generated abilities inherited native spell
  fields, but the final runtime assertions did not explicitly aggregate that
  evidence. Added one 1,045-node invariant spanning 681 logical roots and 364
  template executions: exact parent mapping, Conjuration/Summoning/full-round/
  close native contract, metamagic equality, non-null material data, and exact
  root-versus-execution action-bar behavior. The invariant links every new
  choice to Acadamae's actual classifier while the existing Acadamae runtime
  scenario proves eligible/ineligible, save, fatigue, cancellation, and
  exactly-once behavior.
- Repository validation, `1009/1009` domain tests, and clean Release compile
  pass. Exact next action: commit/push and execute the strengthened structural
  observer on its immutable source.

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

## 2026-08-11 - registration performance repair

- Fresh-process structural run `20260811T1813067760804Z-observe-expanded-summoning-inventory`
  failed closed at 300 seconds with `timeoutStage=request-accepted` and zero
  assertions. No save was selected, loaded, or written.
- Instrumentation isolated the cost to cloning 1,045 complete native ability
  graphs during identity-only registration. Replaced those unreachable shells
  with lightweight hidden `BlueprintAbility` instances carrying non-null empty
  native data structures; frozen symbols, GUIDs, planned types, and registry
  count are unchanged.
- Repository validation, 996/996 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `b8a26e3b8e0b2e42efcd1bbea24d86f2003d2d46de95de1c0d1313bae5e42934`;
  package SHA-256: `0542f022c0e3408f5534ab2921b9fe82cd9543c3f06d62fc73d5595b2e273cdf`.

## 2026-08-11 - exact donor-mapped unit foundation

- Lightweight registry repair passed guarded fresh-process scenario
  `20260811T1820275250965Z-observe-expanded-summoning-inventory` on published
  source `fde2c66638e68d9c466946e9a90fd91ac709cca3`; duration 85.6 seconds,
  status PASS, no save selected/loaded/written.
- Frozen one exact donor GUID decision for each of 67 creature keys. Dedicated
  summon donors are distinguished from campaign/proxy donors so the latter
  cannot silently bypass stricter normalization.
- Unit identities now clone their creature-specific donor, copy the component
  array before filtering, remove explicit XP/loot/inventory/dialogue/story/
  companion/persistence surfaces, and force the proven native Summoned faction.
  No unit is published yet; fact-level travel/summoning removal and mechanical
  reconstruction remain required before publication.
- Repository validation, 997/997 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `9ae16529e873ceb7b1c111f383102940141a80a0499a208c94cf7eb3efe2c7e3`;
  package SHA-256: `9a3b39656ffead6aa01a47273a33f7aaf059e3c97938ee0c172137644edf2765`.

## 2026-08-11 - bounded structural unit cloning

- Exact-donor runs `20260811T1827302141068Z` (180 seconds) and
  `20260811T1831132044960Z` (300 seconds) both failed closed at
  `timeoutStage=request-accepted`; no assertions and no save access. Deep Unity
  cloning of 67 complete unit graphs is therefore rejected as an architecture.
- Replaced it with fresh `BlueprintUnit` creation plus structural copying of
  non-Unity blueprint fields. Every array is defensively cloned, native cached
  pointers and KMG GUIDs are never copied, and component filtering/faction
  normalization occurs only on the new unit.
- Repository validation, 997/997 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `80be16ab0d14224d2f0427573c18501c025356cfd3f4ff77da0c70dfc3f665c5`;
  package SHA-256: `a916354b594550e521e493baae6d835fbd2c5693e5c512d42ec49d8577074047`.

## 2026-08-11 - faction field runtime repair

- Published structural-clone run `20260811T1838286338373Z` timed out, but the
  authoritative game log exposed the immediate owned root cause: initialization
  rolled back after `MissingFieldException` for `BlueprintUnit.m_Faction`.
- The installed 2.1.7b assembly exposes the field as `Faction`. Resolution now
  searches the exact two supported names across the hierarchy and still fails
  closed on absence or ambiguity.
- Repository validation, 997/997 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `259c904aec1492b9b83fa36be0bf1573e17572703db5f8e242f4c571390abae0`;
  package SHA-256: `1ae6af5c30617807422d188fb53e53fb5c8e0829ad4443034122f9b8dda247df`.

## 2026-08-11 - native variant action construction

- Exact-donor structural registration passed guarded fresh-process run
  `20260811T1843447348611Z-observe-expanded-summoning-inventory` on published
  source `787a970d019efdc731c667a50ce7816776e46e1d`; status PASS, no save access.
- Configured all 681 logical abilities and 364 aligned execution abilities from
  the exact native family/tier/multiplicity child contract. Component/action
  graphs are independently cloned without copying Unity cached pointers; each
  graph must contain exactly one spawn action, retargeted to its KMG unit.
- Native range, casting time, duration rank, summon pool, tier cleanup buff,
  descriptors, metamagic, and quantity/Superior Summoning rank semantics are
  inherited from the exact canonical child. Parent arrays remain untouched.
- Repository validation, 998/998 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `b5c5f5ee16394c2c5149c739cc32f01fd7d5463612c06f44cac4812688de7cec`;
  package SHA-256: `ffce07d9c936a0fd421b140757a96f99533df9651b72aa27cc3e7d985d4ce89f`.

## 2026-08-11 - conditional multi-branch spawn repair

- Guarded run `20260811T1851191704530Z` failed closed. The game log proved
  initialization completed all identity registrations, then rolled back because
  native SM V Large Air Elemental contains multiple conditional spawn actions.
- Updated the invariant from exactly one action to at least one action, and
  retarget every branch to the same KMG unit. Conditional alignment/routing is
  preserved and no branch can create a mixed-kind pack.
- Repository validation, 998/998 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `277e364708c26d5a1fc19d01f8127c722517a478f8679ebe9e814a8b53f298c8`;
  package SHA-256: `7800aea247364c952a27e45a39b1098e05d83e324d27e958ebc41278c916a7b4`.

## 2026-08-11 - direct tier-I template repair

- Guarded run `20260811T1858359771129Z` failed closed after registration because
  direct parent `SummonNaturesAllyI` has no `AbilityVariants` component and no
  `Single` name token. Direct parents now supply only the one-creature template;
  quantity requests against a direct parent fail closed.
- Repository validation, 998/998 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256: `56b964c0c4d6c3b9c459d1aa9943b1773b2f6dd1a56ad987b4f1fcf927fa04ae`;
  package SHA-256: `db84ce95e431fa502b6aa6736fd8308350951963f32516663102e5c061b8511f`.

## 2026-08-11 - native action graph and transactional publication

- Guarded fresh-process run
  `20260811T1905591662821Z-observe-expanded-summoning-inventory` passed on
  source `344c9590dc02c086da9e4d9dcd03917e6ebb741a`: all 67 units, 1,045
  abilities, 1,370 registered identities, and every configured native spawn
  action graph validated without save access.
- Added additive publication for all 18 canonical SM/SNA parents. The merge
  preserves original references and order, appends the 681 KMG logical
  placements deterministically, rejects duplicate identities, and records the
  exact component collection for rollback. Rollback refuses an unrelated later
  mutation rather than overwriting it.
- Repository validation, 999/999 domain tests, clean Release build, and strict
  package validation PASS. DLL SHA-256:
  `ea5e5c6cd7eee93c727ea655617a279128cc33bad4ab89914b0ce882babe91e0`;
  package SHA-256:
  `7d1a8860253fb28c9dd0f470c0e40a7b3a4146eec66c75c3697dbfd1d9b55310`.

## 2026-08-11 - live parent publication PASS

- Guarded fresh-process run
  `20260811T1912513494823Z-observe-expanded-summoning-inventory` passed on
  committed source `7feb6cb10049ad88c6bf4ddad148e3c5c7bd9eb0`.
- Exact assertions passed: 67 KMG units, 1,045 KMG abilities, 1,370 total
  registered identities, and 681 KMG logical placements across the 18
  canonical final-live parent surfaces. No save was accessed.
- The loaded library contained 523 summon-family ability candidates, including
  optional-mod clones. Those remain inventory evidence only; KMG mutated only
  the 18 exact base parents at this checkpoint.

## 2026-08-11 - donor component reference isolation

- Retained donor components are now recursively cloned per KMG unit. Blueprint
  references and Unity assets remain shared read-only inputs, while component
  instances, arrays, and mutable nested value/action graphs are isolated.
- This removes the possibility that runtime component ownership or application
  state on a KMG summon mutates the native donor blueprint's component graph.
- Repository validation, 1,000/1,000 domain tests, clean Release build, and
  strict package validation PASS. DLL SHA-256:
  `c267f3aadb27291326eaaeab2d72f594a18cdd0bbe685a733f60adc56af4277d`;
  package SHA-256:
  `489843a5eeda59e562334fafaf07c579a501632456a49151305b5cdaba7e3f24`.

## 2026-08-11 - runtime fact and spell deny contract

- Added a fail-closed runtime member classifier for creature summoning,
  conjuration, teleportation, planar travel, permanent profane gifts, and
  campaign/companion/loot surfaces.
- KMG units now filter prohibited direct facts and component-granted blueprint
  arrays, clear all donor class-level memorized/selected spells, and always
  receive a non-null empty starting-inventory array. Approved combat members
  such as natural armor, tripping bite, and hell-hound breath remain eligible.
- Repository validation, 1,001/1,001 domain tests, clean Release build, and
  strict package validation PASS. DLL SHA-256:
  `84463f21c0f373b04b914a49ac7387363e2da67ec01bc0f354d138ed41454208`;
  package SHA-256:
  `b96b07e3ed21dfa6feacce9f5c1152d2b66065502f72d6e432345ce1ea4915ba`.

## 2026-08-11 - authoritative sanitizer observer

- The guarded structural observer now compares every KMG unit with its frozen
  donor and fails on shared component instances, prohibited fact/component
  references, inherited class spell arrays, or nonempty starting inventory.
- Source validation, 1,001/1,001 tests, clean Release, and strict package PASS.
  DLL SHA-256: `5d77c1932f48b8ad3c2cba010e65918f9fcfd92a5d11e16ca0d404f227c2c2ec`;
  package SHA-256: `f61a7e8154db157979b2423abdf410fd2726b580c63e0415c972b7decd46f551`.

## 2026-08-11 - sanitizer native runtime PASS

- Guarded save-free run
  `20260811T1924349269417Z-observe-expanded-summoning-inventory` passed on
  source `9b064ac2896d38b69a87777976825fbed795f6a2`.
- Exact native assertions: shared donor components `0`; prohibited fact or
  component references `0`; inherited class spell arrays `0`; nonempty
  starting inventories `0`. Registry remained 1,370 and live placements 681.

## 2026-08-11 - standalone celestial/fiendish template foundation

- Configured the four frozen KMG template buffs using only native Owlcat
  components. Celestial grants acid/cold/electricity resistance and DR/evil;
  fiendish grants cold/fire resistance and DR/good. Low values are 5 and high
  values 10; high also grants CR+5 spell resistance.
- Each of the 182 templated logical SM placements now routes to its frozen
  celestial and fiendish executions. Non-evil casters may use celestial,
  non-good casters may use fiendish, so neutral casters retain both choices.
  Execution descriptors are Good/Evil and every native spawn branch receives
  the selected buff as a permanent, non-dispellable child for bounded cleanup.
- Unfinished fidelity: exact SR for creatures with 5–10 HD and once-per-day
  smite are not yet implemented; low-tier omission is conservative, not claimed
  as final fidelity.
- Repository validation, 1,002/1,002 tests, clean Release, and strict package
  PASS. DLL SHA-256: `dbd047731508c0395900f48ab86e6bd862ef348fa36bf4de3933c2baaa43cecc`;
  package SHA-256: `f5c42e5160147944420028d91ab80761502c1436e7d60bc56ffe8dcefb86c8db`.

## 2026-08-11 - template localization collision repair

- Guarded run `20260811T1932316147945Z-observe-expanded-summoning-inventory`
  timed out after bootstrap rolled back. The authoritative log identified a
  localization collision: celestial/fiendish executions used unique name keys
  but shared the logical placement description key.
- Execution description keys now include the Celestial/Fiendish suffix. The
  exact harness-launched Kingmaker PID 15760 was terminated after the timeout;
  the scenario performed no save access.
- Repository validation, 1,002/1,002 tests, clean Release, and strict package
  validation PASS after the repair.

## 2026-08-11 - localization repair PASS and exact template observer

- Guarded fresh-process run
  `20260811T1937046769749Z-observe-expanded-summoning-inventory` passed on
  repaired source `61a3c40e3dd5ebecd07a6ed5d19a0b16d4af51d5`.
  The loaded library contained 67 KMG units, 1,045 KMG abilities, 1,370 total
  identities, and 681 live parent placements; all sanitizer assertions passed
  and no save was accessed.
- Added fail-closed final-live assertions for exactly 182 nested logical
  template choices, 182 celestial executions, 182 fiendish executions, and
  four template buffs. Execution checks cover alignment masks, Good/Evil
  descriptors, lack of direct logical effects, and permanent non-dispellable
  child-buff application on every spawn branch. Buff checks cover DR bypass,
  exact elemental-resistance component counts, and high-tier CR+5 SR.
- Repository validation, 1,002/1,002 domain/reflection tests, clean Release,
  and strict standalone package validation PASS. DLL SHA-256:
  `17a6c5a3fb889f700d1a3db362ee7c111b52d87b68d26ad74019b7c5d85d3eb2`;
  package SHA-256:
  `5f1a75d3b29102d7e77742fed0476005a78db98cd85b5cb6473cff8fc10a6f43`.

## 2026-08-11 - exact template graph native PASS

- Guarded save-free run
  `20260811T1944046651609Z-observe-expanded-summoning-inventory` passed on
  committed source `d76f5c6010ca3612ba2b1c24e076b7601fed9227`.
- Exact final-live assertions passed: 182 logical template choices, 182
  celestial executions, 182 fiendish executions, and four native-component
  template buffs. Registry count remained 1,370 and canonical placements 681;
  donor isolation and all sanitizer invariants remained zero-failure.
- The scenario did not access a save. The guarded harness completed its normal
  deployment transaction and process exit without requiring UI evidence.
- Committed-source clean Release DLL SHA-256:
  `d45d94e9cfab1e032b597b7b42de7ca49990702ef8e8d165fcbbdd04acd7aa84`;
  package SHA-256:
  `f356e94fca20e335fbba8ef70d5241903527aecc2ef2593b93f631564f798dd0`.

## 2026-08-11 - exact 5-10 HD template spell resistance

- Allocated and activated two append-only buff identities:
  `KMG.Summoning.Template.Celestial.Mid`
  (`c3c53de0ca9440e5af263dfb16922188`) and
  `KMG.Summoning.Template.Fiendish.Mid`
  (`031bc1b958324023bf3f4c33b976185d`). Collision and format validation
  passed against the complete manifest.
- Extracted a pure HD-band policy with exact boundaries: low 0-4, mid 5-10,
  high 11+. Low and mid use resistance/DR 5; high uses 10. Mid and high grant
  native CR+5 spell resistance while low omits it below the tabletop threshold.
- Expanded Summoning now contributes 1,118 registered identities. The complete
  append-only ledger is 1,373 IDs: 1,372 active and one reserved. Bootstrap
  derives its aggregate registration expectation from the feature-local count.
- Repository validation, manifest validation, 1,003/1,003 domain/reflection
  tests, clean Release, and strict package validation PASS. DLL SHA-256:
  `48175b82c756f080424dcf577ac994798bfbb23daa96999d60708cd88210808b`;
  package SHA-256:
  `d6c8ac7ae2fb742e7027f956d01c26ec0c9372679b443cec72e1ee66d08a8898`.

## 2026-08-11 - exact template spell resistance native PASS

- Guarded save-free run
  `20260811T1954362756414Z-observe-expanded-summoning-inventory` passed on
  committed source `d384ba06cf76896543a6b23ed480d3f6715bbba2`.
- Exact final-live counts passed: 1,372 registered identities, six template
  buffs, 182 logical choices, 182 celestial executions, 182 fiendish
  executions, and 681 canonical parent placements. The observer additionally
  proved low/mid/high resistance values and SR presence, donor isolation, the
  prohibited-reference deny contract, empty inherited spell arrays, and empty
  starting inventory.
- The scenario performed no save access. Committed-source DLL SHA-256:
  `379c59dfd462a39ac1ec953d71ce9e658f9a7b494d7bef52447eeb231c2cc2f8`;
  package SHA-256:
  `39afcc9c9dbb0a90651e4a3837e8e4563dc1522f4e6da47556268502e016bff4`.

## 2026-08-11 - exact native/optional template-mechanic observer

- Added a nine-GUID final-live inventory for the Call of the Wild celestial
  and fiendish template features plus their referenced Smite Evil, Smite Good,
  spell-resistance, energy-resistance, and alignment-DR facts. The observer is
  structural and uses only base blueprint types; KMG retains no optional-mod
  compile-time reference.
- Repository validation, 1,003/1,003 tests, clean Release, and strict package
  validation PASS. DLL SHA-256:
  `df9060c481b80cf7dc78302cc5d680652e1a25ad1cee4780335e009ae96b9f1e`;
  package SHA-256:
  `bec91cc818b9007eb8cf8fc919dee0da23e9c6f23a1b8815830da0bb85b46484`.

## 2026-08-11 - template-mechanic inventory PASS and smite follow-up

- Guarded save-free run
  `20260811T2000068867927Z-observe-expanded-summoning-inventory` passed on
  committed source `63d77276ea97468f6bb31768b3f4e7125a390250`; all nine exact
  final-live template features/facts were found.
- The optional template features themselves use only base-game `AddFacts`.
  Their smite facts use base `AddFacts` and `AddAbilityResources`, but delegate
  to Smite Evil/Good abilities `f009c072167c4b53a37c1071a2251c3f` and
  `320b92730bd54842b9707931a5dbab18` plus shared resource
  `b4274c5bb0bf2ad4190eb7c44859048b`. Those exact dependencies are now included
  in a deeper fields/action-graph observer.
- Repository validation, 1,003/1,003 tests, clean Release, and strict package
  validation PASS. DLL SHA-256:
  `dbec700adf841b346350c10f74c7c6bcaa46529242ece270635a421f09bff060`;
  package SHA-256:
  `265760f09f186fab4f67f819d8c2896f8b9ac1811bb0be4166d77929a22cc3a5`.

## 2026-08-11 - smite dependency graph PASS and deep trace

- Guarded save-free run
  `20260811T2004013138253Z-observe-expanded-summoning-inventory` passed on
  committed source `f5587d327e67a202adcca89b7961f2683c2b2a0f`.
- Both optional template smites are swift supernatural targeted abilities,
  consume one unit-local shared resource, use Charisma and character-level
  ranks, check target alignment/current smite state, and conditionally apply a
  target buff. Direct reuse is not yet safe because resource maximum and
  applied-buff identities were nested below the prior graph depth.
- Increased the exact graph depth and added explicit resource-amount expansion.
  Repository validation, 1,003/1,003 tests, clean Release, and strict package
  validation PASS. DLL SHA-256:
  `6ffc86d6f1e2b4db76f94ca43d9ea4ea191f0c282a312e203b0fe526eaa3074e`;
  package SHA-256:
  `4589be1fdb76e920eef68735daa5376aec193b91a03f0d45bd7b75c81ef064ae`.

## 2026-08-11 - bounded celestial/fiendish smite source qualification

- Guarded save-free run
  `20260811T2008011506353Z-observe-expanded-summoning-inventory` passed on
  committed source `15c3b7f1dc2d91269f44a0480c4ba036f83ea15b` and completed the
  optional implementation trace. Its shared resource has exact base maximum
  one, while both smite abilities apply the same permanent non-child target
  buff. KMG therefore does not reuse that unsafe external-state graph.
- Added two frozen KMG marker buffs and a summon-local combat handler. The
  celestial marker recognizes evil and the fiendish marker recognizes good;
  it grants a nonnegative Charisma attack bonus and HD damage, consumes itself
  after the first eligible successful hit, and creates no target buff or other
  state that can outlive the summon. This is the approved conservative
  adaptation of the swift target-selection mechanic.
- Every one of 182 celestial and 182 fiendish execution abilities applies its
  matching marker as a permanent, non-dispellable summon-child buff. The
  guarded structural observer now checks those 364 applications and exactly
  two configured marker identities.
- Expanded Summoning contributes 1,120 identities. The append-only ledger is
  1,375 IDs: 1,374 active and one reserved. Repository validation, manifest
  validation, 1,004/1,004 tests, clean Release, and strict package validation
  PASS. DLL SHA-256:
  `f59d4092f169471fc71499da2be5966323f0d50ed38f41d834ef33155decf707`;
  package SHA-256:
  `07e451168183fe27553daa440887278a0377c20a3d804513887f5d9789577464`.

## 2026-08-11 - bounded template smite native structural PASS

- A first guarded fresh launch on committed source
  `a0d8e7752281d4ae1e51ce094c1226f6a30faf16` accepted the exact request
  and loaded the expected DLL, then exited during Steam/platform initialization
  before final-live blueprint inspection. The structured failure is
  `20260811T2020185475961Z-observe-expanded-summoning-inventory`; no save was
  accessed and no feature assertion failed.
- The subsequent fresh Steam launch passed as
  `20260811T2021406071785Z-observe-expanded-summoning-inventory`. It proved
  exact registry 1,374; 67 units; 1,045 abilities; 681 placements; 182
  celestial and 182 fiendish executions; six HD-banded template buffs; two
  summon-local smite markers; and zero donor-sharing, prohibited-reference,
  inherited-spell, or starting-inventory violations. No save was accessed.
- Committed-source clean Release DLL SHA-256:
  `bf51c14793b878636502bf43cc2f2119b3c0e5a12208972e2d36b44d22b724b0`;
  package SHA-256:
  `40516139bebc06bcea6ef29e67d93485a072a6f6605e6587d935931b65d5e425`.

## 2026-08-11 - spawn-local summon alignment source qualification

- Inspected the exact 2.1.7b assembly alignment surface. `UnitDescriptor`
  owns a `UnitAlignment`; `UnitAlignment.Set(Alignment)` is public; the exact
  enum is the nine Pathfinder alignments encoded as moral and law/chaos bits.
- Added a pure fail-closed resolver plus a native post-spawn context action.
  Celestial and fiendish resolution preserves law/chaos and replaces good/evil;
  Nature's Ally copies the actual caster's exact alignment from
  `Context.MaybeCaster`. The action mutates only the spawned unit descriptor,
  so donor and KMG blueprint objects remain unchanged and the engine serializes
  the resolved alignment as normal unit state.
- The structural observer now requires family-correct alignment actions across
  all 182 celestial executions, 182 fiendish executions, and 320 Nature's Ally
  placements, with no cross-family modes. Repository validation, 1,005/1,005
  tests, clean Release, and strict package validation PASS. DLL SHA-256:
  `b8b0903f254579d71d5f23473e3e9865c47d60978f4d13201587ff0242f69173`;
  package SHA-256:
  `1e28773cd7a34fdb3906e964dbbeea1c9c830c2091ed9437d591ff6750768518`.

## 2026-08-11 - alignment observer failure and ActionList isolation repair

- Guarded run `20260811T2031332882968Z-observe-expanded-summoning-inventory`
  completed normally on committed source
  `6fd61f2e0e600f689efe7ef6e88495dfe7cd0f37`. Registry 1,374, all 681
  placements, all template counts, and all sanitizer assertions passed, but
  the new celestial, fiendish, and Nature's Ally alignment assertions failed.
  No save was accessed.
- Retained final-live action graphs showed one through four caster-alignment
  actions accumulating on abilities that reused a native quantity template.
  The initial graph comparison implicated the `ActionList` container and its
  `GameAction[]`; a subsequent run narrowed the remaining alias to the action
  ScriptableObjects themselves.
- Added an explicit recursive `ActionList` clone. Tightened the observer to
  require exactly one alignment action and one
  intended template/smite application per execution, plus zero KMG post-spawn
  actions or buffs on any non-KMG ability. Repository validation, 1,005/1,005
  tests, clean Release, and strict package validation PASS. DLL SHA-256:
  `436b15364607b1fd9b98b4fcbec3c1553d2708183d1f111218f0cd0bd02cd67d`;
  package SHA-256:
  `d607d3f3272d72bfbc6f1003618e399bdc0af1d59b644310988b5ab01aeea7da`.

## 2026-08-11 - GameAction ScriptableObject isolation follow-up

- The committed ActionList-only repair still failed guarded run
  `20260811T2037058339804Z-observe-expanded-summoning-inventory` on
  `ed7d6a2b41b951166ed0a243fb30e7531b5dd6d0`. Exact counts showed 286
  non-KMG abilities containing KMG post-spawn actions or buffs. No save was
  accessed.
- Assembly metadata and retained graphs identified the deeper boundary:
  `GameAction` inherits `SerializedScriptableObject`, so the generic Unity
  object guard returned each native action by reference. The explicit
  `ActionList` clone consequently cloned an array whose action elements were
  still shared.
- The clone now treats `GameAction` like `BlueprintComponent`: it creates a
  distinct ScriptableObject of the exact runtime type and recursively copies
  its fields, while continuing to preserve referenced blueprints, prefabs,
  icons, and other immutable Unity assets. Repository validation, 1,005/1,005
  tests, clean Release, and strict package validation PASS. DLL SHA-256:
  `b515558f54db01c1694a25507df298415c6c7ca0c2053e21f81db63a67c37507`;
  package SHA-256:
  `597637b7698117bd5e86cb9f074581a3020419392032e923612b96baf962d729`.

## 2026-08-11 - GameAction isolation proved; branch-cardinality correction

- Guarded run `20260811T2043115519772Z-observe-expanded-summoning-inventory`
  completed on committed source
  `0759783da38db3843905f368c62a964217870545`. The new native-action-isolation
  assertion passed at zero, proving the 286 contaminated non-KMG graphs were
  repaired. Representative KMG graphs contained one independent caster action.
  No save was accessed.
- Celestial, fiendish, and aggregate alignment assertions still failed because
  they required exactly one added action per ability. Native summon quantity
  graphs can legitimately contain multiple spawn nodes. Reframed the invariant
  to require a nonzero spawn count and exact equality between spawn-node count
  and family-correct alignment/template/smite action counts.
- Repository validation, 1,005/1,005 tests, clean Release, and strict package
  validation PASS. DLL SHA-256:
  `a675b1a4eb7e01c418fd71e842e2d0dedb8c0b6435e7d0e815389ee4d9ab78f8`;
  package SHA-256:
  `7755cdeca8ee554f2ec0456bcac67e72d91ce4a007787e31d31e403e845ff85d`.

## 2026-08-11 - spawn-local alignment and action isolation native PASS

- Guarded save-free run
  `20260811T2048003275107Z-observe-expanded-summoning-inventory` passed on
  committed source `b88e99cffb7464d7354416fba82d1da313e17ae2`.
- Exact final-live alignment checks passed across all 182 celestial, 182
  fiendish, and 320 Nature's Ally executions. Every native spawn branch had one
  family-correct alignment action; templated branches also had one template and
  one bounded-smite application per spawn node. Non-KMG abilities containing
  any KMG post-spawn action or buff: zero.
- All earlier invariants remained green: registry 1,374; 67 units; 1,045
  abilities; 681 placements; six HD-banded buffs; two smite markers; zero
  donor-component sharing, prohibited references, class spells, or starting
  inventory. No save was accessed. Committed-source DLL SHA-256:
  `b8cd73a138056eb4e138ec58da8e41a521764d3fadb907e061cd10a83e7a0d00`;
  package SHA-256:
  `601417e301e25ef4a6a64eec16b165d8867c2e3dc194b61ebfca2d7472f9ed60`.

## 2026-08-11 - complete frozen donor graph observer

- Replaced the historical 25-GUID donor sample with the exact 54 distinct
  donor GUIDs derived from `ExpandedSummoningDonorCatalog`. The guarded
  observer now fails on any missing chosen donor and records bounded component,
  body, and view graphs alongside every donor's core BlueprintUnit fields.
- This is forensic instrumentation only; no donor or KMG creature mechanic was
  changed. Repository validation, 1,005/1,005 tests, clean Release, and strict
  package validation PASS. DLL SHA-256:
  `73b57960c90c1838f640359f66ad0794975485023219b0a74296bda0a637b9c1`;
  package SHA-256:
  `043f279437749279c6c5a69355637a753ba6b28bf45bd82c6f31fb80940eaa00`.

## 2026-08-11 - complete frozen donor graph native PASS

- Guarded run `20260811T2055502086857Z-observe-expanded-summoning-inventory`
  passed on committed source
  `9e1d851e75cf413f5d0a576484a9f5a8538b2a2b`. All 54 distinct catalog donor
  GUIDs were present; no selected donor was missing.
- The observer retained core unit fields and bounded component, body, and view
  graphs for every donor. All 17 assertions passed: registry 1,374; 67 units;
  1,045 abilities; 681 placements; exact template/alignment/smite execution;
  zero shared components, prohibited references, inherited class spells,
  starting inventory, or native action contamination.
- The host wrapper timed out at 120 seconds only after the game wrote the PASS
  result and exited; structured scenario duration was 103,225 ms. This is PASS
  evidence, not a runtime failure. No save was accessed.

## 2026-08-11 - Lantern Archon native-candidate observer

- The official Pathfinder 1e stat block establishes the reconstruction target:
  CR 2, 2 outsider HD, Small lawful good, two 30-foot ranged-touch light rays,
  aura of menace, and archon defenses. Greater teleport and gestalt are outside
  the safe summon contract and will not be retained.
- The current Ghaele donor is 13 HD, Medium, chaotic good, armed, spellcasting,
  and visually unsuitable. Added a bounded final-live candidate scan for exact
  Will-o'-Wisp, archon, light-ray, and aura identities; no gameplay changes.
- Repository validation, 1,005/1,005 tests, clean Release, and strict package
  validation PASS. DLL SHA-256:
  `1d462421aabfb59f29825b9d69724526e44f8bd859a10bdb9b7b66c4decabdbd`;
  package SHA-256:
  `9f40a951c380446dccf77c829aa0b4f4ce309f077f9f258b4539cc44bb309960`.

## 2026-08-11 - elementals, mephits, and Lantern Archon source qualification

- The focused candidate run passed and established exact final-live donors:
  Will-o'-Wisp prefab for the Lantern view, Ghaele's two-projectile ray graph,
  its ray AI targeting considerations, and the native Archon-subdomain Aura of
  Menace carrier. No donor blueprint is mutated.
- Added an immutable exact-reuse profile for 24 dedicated elemental summon
  units and four dedicated mephits. Their cloned KMG units retain native combat
  mechanics while the existing sanitizer removes XP/loot/campaign surfaces.
- Reconstructed Lantern as 2 outsider HD, Small lawful good, official ability
  scores, 60-foot airborne movement, dual 1d6 direct ranged-touch rays limited
  to 30 feet, a ray-only brain, DR 10/evil, electricity immunity, natural armor
  +4, poison and evil defenses, lawful/good/extraplanar facts, and native Aura
  of Menace. Wisp attacks/invisibility/immunity and all Ghaele spells/weapons
  are absent. Teleport and gestalt are intentionally omitted.
- Frozen special identities: ray `d4c2ce6c90094fdfb0fd908312372d72`,
  AI `3579bfa7c4b040c4812286f4ade47146`, brain
  `427b496a05db48aa94997415f1a74c39`, defenses
  `4c55af41c90443c18267a806c740ce16`. Ledger is 1,379 total:
  1,378 active and one reserved; feature foundation 1,124.
- Repository validation, 1,006/1,006 tests, clean Release, and strict package
  validation PASS. DLL SHA-256:
  `646f2b131aa23d63c0531526d481a5a9bffb8d94b4d01fe4bdff1b6239cc6236`;
  package SHA-256:
  `c792287814bd894a95f980cbe6bbcad475db22b9c3c0cf4e338e8616bee7a155`.

## 2026-08-11 - elementals, mephits, and Lantern Archon native PASS

- The first guarded launch exposed two fail-closed construction contracts:
  special AI/brain shells must be named before registry validation, and native
  Lantern facts must be requested by their exact installed concrete types.
  Both repairs were narrow, source-qualified, committed, and pushed.
- A subsequent complete structural run reached all assertions and found one
  observer-only false positive: the generic forbidden-token policy matched the
  `KMG_Summoning` namespace on the bounded Light Ray and Archon Defenses facts.
  The observer now permits only those two exact owned identities; every other
  unit-granted fact and ability remains subject to the full forbidden policy.
- Guarded fresh-process run
  `20260811T2138366091237Z-observe-expanded-summoning-inventory` passed on
  committed source `fa91900ea6e64206986a9db2deeccbe866f75190`.
  All 19 assertions passed: 67 units, 1,046 abilities, registry 1,378, 681
  placements, 55 exact donors, exact Lantern stats/ray/aura/defenses, zero
  shared donor components, zero prohibited references, zero inherited spells
  or inventories, and exact template/alignment/smite structure. No save was
  accessed. DLL SHA-256:
  `6231443bf123bfaeee7d67df5ea5a20a487307f310132fed4e3a1ad00b43189e`;
  package SHA-256:
  `0e0d09dcad78e33d074ebc5355a8eef90c12e0a257555530f412628b389d8c2a`.

## 2026-08-11 - bounded special-creature mechanic inventory

- Two save-free guarded fresh-process runs captured the exact final-live graphs
  needed for Salamander, Invisible Stalker, Shadow Demon, and Succubus:
  `20260811T2150184037199Z-observe-expanded-summoning-inventory` on
  `a5a40796803b503895e955d12149c3457d556c46` and
  `20260811T2156134219365Z-observe-expanded-summoning-inventory` on
  `37e28a9e64c0b8ed1c2bb8ac0aff5b30cfef4556`. Both passed all structural
  assertions; neither accessed a save.
- Native Natural Invisibility `94b2838e8a492c44ebf89e7fe7a75a62`
  explicitly keeps invisibility after offensive actions. Native Incorporeal
  `c4a7f98d743bc784c9d4cf2105852c39` supplies the damage divisor,
  critical/precision immunity, airborne, and trip-immunity bundle.
- Native Spectre/Devourer energy-drain facts are unsafe for direct reuse: they
  apply a one-day negative level that can become permanent. A KMG derivative
  must use `EnergyDrainType.Temporary` and a summon-bounded duration.
- The base-game Drake tail fact adds exact `Tail1d6`; the Shambling Mound grab
  graph is bounded to a hit-confirmed grapple and exposes its target/caster
  cleanup buffs and constrict damage for safe reconstruction. Native Dominate
  Person applies `DominatePersonBuff` for a round-based rank duration, while
  the inspected Vampiric Touch graph applies only temporary hit points to the
  caster. Donor spell-list and optional-mod components will not be copied.
- Repository validation, 1,006/1,006 tests, clean Release, and strict package
  validation passed. DLL SHA-256:
  `773f57f084555af269ee0b4462e95024e2ca9791042f29a6cbdc60df3ed4ca99`;
  package SHA-256:
  `d0950ac29aa163fc1d2f595515b0e5958b7856359def5f3891d54a93d0f6e5f5`.

## 2026-08-11 - Invisible Stalker and Shadow Demon structural PASS

- Reconstructed Invisible Stalker as a 7-HD Medium neutral air/elemental
  outsider with official ability scores, two 2d6 slams, 30-foot airborne
  movement, natural armor +6, relevant combat feats, and the qualified native
  attack-safe Natural Invisibility buff. Huge-elemental stats, whirlwind, air
  mastery, class spells, and donor brain are absent.
- Reconstructed Shadow Demon as a 7-HD Medium chaotic-evil incorporeal outsider
  with claw/claw/bite, a hit-confirmed 1d6 cold rider, DR 10/cold iron or good,
  acid/fire resistance 10, cold/electricity/poison immunity, and SR 17. The
  Soul Eater's Wisdom damage, DR/magic, scaling, and campaign surfaces are
  absent. Possession, shadow blend, and sprint remain documented conservative
  omissions; teleportation and summoning are intentionally absent.
- Frozen `KMG.Summoning.Special.ShadowDemon.CombatTraits` at
  `f81993d391054678a138227b91141eae`. Ledger is now 1,380 total: 1,379 active,
  one reserved; Expanded Summoning foundation is 1,125 and aggregate runtime
  registration is 1,379 in every feature-module state.
- Guarded save-free run
  `20260811T2207541420526Z-observe-expanded-summoning-inventory` passed on
  `b4a3fc86804a8a950808457b4f9c38bddeb8152c`: both new exact assertions,
  Lantern, 67 units, 1,046 abilities, registry 1,379, all 681 placements, and
  every sanitizer/isolation assertion passed. DLL SHA-256:
  `c15c777d8818cf2c9b076d0bf26187763ec324dee182448cc0ddf9abe5fd1cb0`;
  package SHA-256:
  `9ad5f4487eadf510133ceb62248104bdf83cf6452e6353029a424979b64f6684`.

## 2026-08-11 - Salamander and Succubus structural PASS

- Reconstructed Salamander as an 8-HD Medium fire outsider with official
  ability scores, spear and 2d6 tail routine, hit-confirmed heat, bounded
  grab/constrict, natural armor +7, and DR 10/magic. The Lizardfolk donor is
  visual only; its equipment, progression, inventory, and campaign behavior
  are absent.
- Reconstructed Succubus as an 8-HD Medium chaotic-evil outsider with two
  claws, bounded three-round humanoid domination, a one-round temporary
  first-hit energy drain, DR 10/cold iron or good, energy defenses, and SR 18.
  The native permanent-capable energy drain, profane gift, teleportation, and
  summoning were not retained.
- Two fail-closed runtime type mismatches exposed concrete native
  `BlueprintFeature` types for DR and creature-type facts. Both were repaired
  without weakening exact GUID/type validation. A subsequent run passed every
  mechanical assertion and isolated one observer-only omission of the new
  Succubus special ability; catalog-driven accounting replaced the special-case
  count.
- Guarded save-free run
  `20260811T2238575798728Z-observe-expanded-summoning-inventory` passed on
  committed source `b0deb04ff9b387b375202c5304a6741c9549ef0a`.
  All 24 assertions passed: 67 units, 1,047 abilities, registry 1,386, all 681
  placements, exact Salamander and Succubus structures, exact template and
  alignment executions, and zero shared donor components, prohibited
  references, inherited spells, or starting inventory. No save was accessed.
- Repository validation, `1006/1006` domain tests, clean Release, and strict
  package validation passed. DLL SHA-256:
  `8a109ca92f13f3ce69b867cc0da08c182be84ce927a36f4d47a8c12d9f10dfca`;
  package SHA-256:
  `87c793d7be7040c3df4a0fc7c6a710ab24860420b9327015f439c37cefa2f7e6`.

## 2026-08-11 - Bebelith and Pixie structural PASS

- Reconstructed Bebelith as a 12-HD Huge chaotic-evil outsider with two 2d4
  claws, a 2d6 bite, DR 10/good, and a unit-local +2 attack/damage bonus
  against chaotic-evil outsiders. Its second same-target claw hit each round
  makes the target's equipped armor eligible for a DC 25 Reflex save; failure
  applies a bounded one-round -2 AC state without mutating the item. The
  Doomspider donor supplies the view only; poison and web behavior are absent.
- Reconstructed Pixie as a 4-HD Small neutral-good fey with a Nixie view,
  airborne movement, attack-safe natural invisibility, DR 10/cold iron, SR 15,
  sixteen no-damage sleep arrows with Will DC 15 and bounded five-minute sleep,
  and one CL 8 Irresistible Dance use using the native touch/dance state.
- Bebelith rot and climb are conservative omissions. Armor destruction is
  adapted to a short AC penalty to avoid permanent item mutation; demon hunting
  uses the exact chaotic-evil outsider surface available in Kingmaker. Pixie
  arrows are body-mounted blueprint weapons and never enter inventory.
- Guarded save-free run
  `20260811T2310424930290Z-observe-expanded-summoning-inventory` passed on
  committed source `f058f4b5060e7eae4de4c7621cbdcbd06cbf08a7`.
  All 25 assertions passed: 67 units, 1,048 abilities, registry 1,396, all 681
  placements, exact Bebelith/Pixie structures, and zero donor aliases,
  prohibited references, inherited class spells, starting inventory, or
  native-action contamination. The live Call of the Wild summon surfaces were
  preserved. No save was accessed.
- Repository validation, `1006/1006` domain tests, clean Release, and strict
  package validation passed. DLL SHA-256:
  `bac9447730454c6b46da2bdef17b7634e154e49c308c1423b00c8b38a63cdd56`;
  package SHA-256:
  `0a295fa0f05a38da920dbd07c019e96e563830d3bec1b17887efda727f94e631`.

## 2026-08-12 - Low-tier natural reconstruction structural PASS

- Reconstructed Dog, Eagle, Poisonous Frog, Giant Centipede, Giant Spider,
  Goblin Dog, and Hyena from explicit tabletop profiles. Donors now supply
  views/rigs only where the native summon chassis was mechanically wrong.
- Added frozen KMG 1d4 and 1d3 bite identities and rebuilt HD, size, ability
  scores, speed, natural armor, attacks, feats, trip behavior, and the exact
  native poison graphs selected by the forensic inventory.
- Bootstrap evidence proved CotW's extraplanar subtype feature is not available
  during standalone KMG registration. Added hidden frozen identity
  `KMG.Summoning.Subtype.Extraplanar` and applied it exactly once to all 67 KMG
  units; final-live optional native-marker reconciliation remains pending.
- Guarded save-free run
  `20260812T0010300046437Z-observe-expanded-summoning-inventory` passed on
  committed source `c2bee19c6598f559436e5f09af5029dc1da746de`.
  All 27 assertions passed: 67 units, 1,048 abilities, registry 1,399, all 681
  placements, 67 exact extraplanar markers, exact low-tier and special
  structures, and zero donor aliases, prohibited references, inherited class
  spells, starting inventory, or native-action contamination. Call of the Wild
  summon parents were preserved. No save was accessed.
- Repository validation, `1007/1007` domain tests, clean Release, and strict
  package validation passed. DLL SHA-256:
  `49e19b17c15e7f0419b9caf01be223804161c2963151dd039d30c679b367fbba`;
  package SHA-256:
  `4a875b25bcad49996242755a37c1abef1d46b92b870a7b54ed4db19ed05649e6`.

## 2026-08-12 - Tier III-IV natural/proxy structural PASS

- Added immutable profiles for Boar, Leopard, Monitor Lizard, Cheetah,
  Crocodile, Dire Bat, Wolverine, Dire Boar, Dire Wolf, Grizzly Bear, Lion, and
  Pteranodon. Each KMG unit now owns exact animal HD, ability scores, size,
  speed, armor, natural attacks, and proven native facts; donors supply views
  and compatible rigs only.
- Added and activated frozen `KMG.Summoning.Natural.Tail1d12`
  (`d7ec01bae32a4d9086214f156ce52ecd`) as Crocodile's secondary attack. The
  manifest is 1,400 active plus one reserved; feature-local/registry counts are
  1,146/1,400 in every module state.
- Reused exact native pounce, ferocity, monitor-lizard poison, trip, airborne,
  armor, feat, bite, claw, and gore contracts. Recorded bounded omissions for
  movement modes, grab/death roll, sprint, blindsense, wolverine rage, and
  unproven feat identities rather than importing unrelated campaign or
  Shambling Mound target state.
- Guarded run `20260812T0026276683838Z` retained a deterministic bootstrap
  failure: native Dog trip defense is concretely `BlueprintFeature`, not the
  base `BlueprintUnitFact` type. Owned registrations rolled back and no save
  was accessed. The repair keeps exact per-fact concrete-type validation.
- Fresh guarded save-free run
  `20260812T0031212209441Z-observe-expanded-summoning-inventory` passed on
  committed source `2534f57199cec7a8cd5ef3b5715cdd4ad30d0ac6` in 106,947 ms.
  All 28 assertions passed: 67 units, 1,048 abilities, registry 1,400, all 681
  placements, exact tier I-IV natural and special structures, and zero donor
  aliases, prohibited references, inherited spells, starting inventory, or
  native-action contamination. Call of the Wild parents were preserved. No
  save was accessed.
- Repository validation, `1008/1008` domain tests, clean Release, and strict
  package validation passed. Runtime DLL SHA-256:
  `f09a132d50cd9058c16f609f9910181c30e04e630dc19166233f441891241794`;
  strict package SHA-256:
  `b89370cc94f4ef125e407c9e7e9fa8164db6e5d2ddf049d9cb7f6ba1d3713725`;
  local-runtime package SHA-256:
  `994ab69e830668d50147901bdf7d410c50aa732f0ba36ace1903f85f99875eb0`.

## 2026-08-12 - Tier V-VII natural/proxy structural PASS

- Added immutable tabletop profiles for Dire Lion, Ankylosaurus, Dire Bear,
  Dire Tiger/Smilodon, Elephant, Mastodon, and Roc. Donors contribute only
  their selected view/rig; exact animal HD, ability scores, size, speed,
  natural armor, weapons, feats, brain, inventory, and alignment are KMG-owned.
- Added frozen weapons `KMG.Summoning.Natural.Tail3d6`
  (`15394605e1664a51bce4b50f38a7603a`), `Bite2d8`
  (`c19d1025fe2b47769c93a3b76d0c052c`), and `Talon2d6`
  (`8a3741a7598147baa08de552565635ad`). The ledger is 1,403 active plus one
  reserved; feature-local/registry counts are 1,149/1,403 in every module state.
- Reused exact native Smilodon critical/focus/pounce structures and Mastodon
  gore/slam weapons. Recorded conservative omissions for ankylosaurus stun,
  elephant/mastodon trample, grab, separate movement modes, and unproven feat
  identities; none adds damage, control, or persistent state.
- Repository validation, `1009/1009` domain tests, clean Release, and strict
  package validation passed. Runtime DLL SHA-256:
  `af525f7db07df088a4f71e674365644500fd9a2982da9e92f0d99ecd648fd199`;
  strict package SHA-256:
  `3431fefac0163119b889f40908f92bb12f8a9731bf8fe01bc342fbdf5159cc3c`;
  local-runtime package SHA-256:
  `d05baab244a7841171cc43648144b3c4cdc176624b76682fa8bf77bb6a64f908`.
- Guarded fresh-process run
  `20260812T0045336396930Z-observe-expanded-summoning-inventory` passed on
  committed source `3c2c5fef82a7d9b032f7da906385013a5699cc8c` in 107,891 ms.
  All 29 assertions passed: 67 units, 1,048 abilities, registry 1,403, all 681
  placements, exact tier I-VII natural/proxy and special structures, and zero
  donor aliases, prohibited references, inherited spells, starting inventory,
  or native-action contamination. Call of the Wild final-live summon surfaces
  were preserved. No save was accessed; the harness restored the Mods/settings
  transaction.

## 2026-08-12 - Complete native cast and quantity matrix PASS

- Added the guarded `disposable-expanded-summoning` scenario on the exact
  `KMG_AUTOMATION_WORKING` load workflow. It drives production `AbilityData`,
  `UnitUseAbility`, `RuleCastSpell`, and the native execution process rather
  than invoking summon factories directly.
- The scenario casts every approved one-creature SM/SNA logical entry and one
  quantity option for every eligible family/tier: 123 one-creature casts, 16
  `1d3` casts, and 14 `1d4+1` casts, for 153 commands total. A request-local
  `RuleSummonUnit` observer proves legal counts and exact same-kind unit
  identity, then exact snapshot restoration proves cleanup.
- Loaded-area diagnosis found that custom summon buffs had null particle-link
  shells. Native unit-view attachment dereferenced them while spawning Lantern
  Archon. All sixteen KMG summon buff identities now preserve native-safe
  non-null empty `FxOnStart` and `FxOnRemove` objects; focused domain coverage
  prevents regression.
- Clean source gates passed: repository validation, `1009/1009` domain tests,
  clean Release, and strict package validation. DLL SHA-256:
  `4ebc621994d9f408554ad2a7902d79fcb41d8eda057fb157288bba7a12821a25`;
  package SHA-256:
  `e3e7c5a9f153cd7dba6a96e0fb72f9f213fcb14919e65e76778698c421f816cf`.
- Fresh Steam run
  `20260812T0235012741461Z-b7419c9642a445ac9edf4bfc8a2ad825`
  passed on committed source
  `8647ceff29ae45c416f948a979fd25098422910d`: 153/153 casts, all 205 spawned
  units same-kind, exact cleanup, no save-writing API observation, and all
  request-local hooks removed. `KMG_AUTOMATION_BASELINE` was never selected or
  modified.

## 2026-08-12 - Complete visual-contract matrix PASS

- Added guarded scenario `disposable-expanded-summoning-visual-contracts` on
  the exact `KMG_AUTOMATION_WORKING` load workflow. It instantiates every 67
  unique one-creature unit through its production ability and native summon
  path, then exercises the attached view, renderers, bounded scale and world
  bounds, colliders, selection/navigation, locomotion events, attack events,
  native damage/hit and death handling, ranged projectile origins, and exact
  disposal/view detachment.
- The Will-o'-Wisp rig used by Lantern Archon has no cast or attack animation.
  The production ray now uses Kingmaker's native `Immediate` cast-animation
  path, allowing its two projectiles to originate at `CenterTorso` without
  waiting for a nonexistent inherited Ghaele animation event. A focused source
  contract protects the fallback.
- Clean gates passed: repository validation, `1009/1009` domain tests, clean
  Release, and strict package validation. DLL SHA-256:
  `b2a2c448e1f29907823298778fc9cc8ff81e42ab4eb21003b11ef2758a4b7203`;
  package SHA-256:
  `23a3562191597c6ff992db56e17fd9beb8587a4a2f42306ed6302b1f6cc4b191`.
- Fresh Steam run
  `20260812T0316269056830Z-77c365156f0b47f5bc6a6c1e8501a6c7`
  passed on committed source
  `ee8a5886fdd817e659fe2afdf3f1019501aac064`. All ten assertions passed:
  67/67 live views, renderable geometry, bounded footprints,
  selection/navigation contracts, locomotion, attack paths, hit/death paths,
  valid ranged origins, and exact cleanup. No save-writing API was observed,
  hooks were removed, and the Mods/settings transaction was restored.

## 2026-08-12 - Active-summon persistence PASS, enabled and disabled

- Added three guarded working-save scenarios: prepare, verify cleanup, and
  verify absent. The prepare phase casts the actual SM Small Air Elemental and
  SNA Wolf production abilities, completes Owlcat's queued entity-creation
  boundary, proves exact loaded-area attachment, and performs one authorized
  save of `KMG_AUTOMATION_WORKING`.
- The enabled transaction passed across three fresh Steam processes. Both
  frozen unit GUIDs were present in serialized `party.json`; restart restored
  exact blueprint identity, caster context, 120-second native lifecycle,
  commands, views, faction, and 681 published parent references. Native
  lifecycle dismissal plus the entity-destruction controller reached zero live
  units, and the second restart found zero deserialized KMG summons with no
  save write.
- The disabled transaction also passed. A save containing both active summons
  loaded safely after Expanded Summoning was disabled because all identities
  remained registered, while required parent publication was exactly zero.
  Cleanup and the final zero-unit/no-write restart both passed.
- The exact pre-test working save and feature settings were restored from
  verified backups. Restored SHA-256 values are
  `96fd2ecc57793d6d5462744c3d0e298a7d4258b92d381e1e8e45c7c14fb651ad`
  and `424da4573acb5dc9e3c7ca3546da688a1405702858fb3b28aea5cbae28c4ba3e`.
  `KMG_AUTOMATION_BASELINE` remained unchanged at
  `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`.

## 2026-08-12 - Sixteen-state fresh-launch module matrix PASS

- Ran all 16 combinations of Gunslinger, Acadamae Graduate, Shield Other, and
  Expanded Summoning on immutable pushed source
  `5e25656d0ed869973d97ed11191ed3175330f4ac`.
- All 16 fresh Steam launches passed with zero failed assertions. Every process
  observed the exact requested active snapshot and the constant 1,403
  registered identities. Each preexisting feature controlled only its own
  publication surface; Expanded Summoning published all 681 required-base
  references when enabled and exactly zero when disabled.
- The outer transaction restored `FeatureModules.json` byte-for-byte to
  SHA-256 `424da4573acb5dc9e3c7ca3546da688a1405702858fb3b28aea5cbae28c4ba3e`.
  Independent post-run hashes also confirmed the working save and protected
  baseline remained at their original hashes.

## 2026-08-12 - Standalone repair and compatibility profiles PASS

- Standalone inventory qualification first exposed two optional-mod-only
  dependencies. Lantern Archon now reuses the exact Call of the Wild Aura of
  Menace carrier only when present; vanilla safely omits that aura. Pixie's
  dance no longer clones Call of the Wild delivery/state blueprints: frozen
  KMG state `aa8b4284e12e49f0b37f327f665638d1` provides bounded `CantAct`,
  -4 AC, and -10 Reflex mechanics at touch range.
- The append-only ledger is now 1,150 Expanded Summoning identities, 1,404
  active repository identities, one reserved identity, and 1,405 total. Static
  validation, all 1,009 domain tests, clean Release, and strict packaging pass.
- On immutable pushed source `5bce781d25ba6f3efadf693dafef2267fd2003fe`,
  every required compatibility transaction passed: standalone 2/2, Call of
  the Wild 2/2, Arms and Armor 1/1, Toggle Custom Soundpacks 1/1, and
  highest-risk combined 2/2. Every run proved 67 units, 1,048 abilities, all
  681 placements, constant 1,404 registration, zero sanitizer/donor mutation
  failures, and exact transaction restoration.
- Transaction IDs: `compat-20260812T054815Z-c06c6c6b86df`,
  `compat-20260812T055014Z-1e10b82c5c5b`,
  `compat-20260812T055206Z-1a0eb7ab4a94`,
  `compat-20260812T055451Z-0b6f132431c1`,
  `compat-20260812T055735Z-3c351a890b59`,
  `compat-20260812T055928Z-f3f395ae342e`,
  `compat-20260812T060121Z-d337b4c24bd8`, and
  `compat-20260812T060411Z-37a29b64137f`.

## 2026-08-12 - Strengthened complete mechanical runtime PASS

- Expanded the disposable mechanical scenario from summon creation alone to
  exact caster-level duration, native close-range approach, good/neutral/evil
  celestial and fiendish choices, SNA caster alignment, Augment Summoning,
  Superior Summoning, and representative natural, proxy, elemental,
  outsider, incorporeal, invisible, breath, projectile, Bebelith, Succubus,
  and Pixie mechanics.
- Added summon-local buff application because Owlcat's generic nested permanent
  buff action did not survive the native spawn graph. The feature action first
  removes the four exact Owlcat celestial/fiendish template buffs before
  applying one selected HD-banded KMG template and bounded smite marker. This
  prevents neutral fiendish choices from retaining Owlcat's default celestial
  defenses.
- Added a bounded native-save Succubus domination action and project-owned,
  nontransferable Salamander/Pixie weapons. The synchronous harness uses the
  already-started native Fire Mephit command and its exact execution context to
  deliver the native effect graph when cone world-time cannot advance inside
  the post-load callback.
- Repository validation, the complete `1009/1009` domain suite, clean Release,
  and strict package construction passed. Guarded Steam run
  `20260812T1143070098993Z-bffb856b44d34334be86fa89c15bb6db` passed all
  assertions: 153/153 commands, 123/123 single entries, 16/16 `1d3`, 14/14
  `1d4+1`, same-kind identity, 153/153 duration/placement, exact cleanup, zero
  residual native templates, Fire Mephit damage `0->18`, and all listed
  representative special contracts. `KMG_AUTOMATION_BASELINE` was neither
  selected nor modified.

## 2026-08-12 - Completion-audit native contracts and final profiles PASS

- The completion audit added one aggregate final-live assertion over all 681
  published roots and 364 celestial/fiendish execution children. Guarded
  save-free run `20260812T1327062696968Z-bd09acfba08942df8f7c42e5c70252f4`
  passed all 31 assertions on pushed source
  `5205805eab3fe0115d6888c53bce73c80474d1b7`: 681 roots, 1,045 total nodes,
  and exact parent mapping, native spell contract, Acadamae classification,
  non-null material data, metamagic equality, and action-bar state for every
  node.
- The complete native mechanical matrix was repeated on the same source as run
  `20260812T1330147883834Z-ec8896f1d65b43e0913a6bea7cba4405`; all 12
  aggregate assertions and 153/153 production commands passed.
- Final-source compatibility repetitions all passed and restored the original
  Mods tree: standalone `compat-20260812T133252Z-8ab70bfdbf75` and
  `compat-20260812T133451Z-f7096e45017c`; Call of the Wild
  `compat-20260812T133727Z-74e8798f7849` and
  `compat-20260812T134033Z-433e1ea1a746`; highest-risk combined
  `compat-20260812T134340Z-adb4d15f893d` and
  `compat-20260812T134649Z-1b83742188f4`.
- The only changes after the already-passing visual, persistence, and 16-state
  evidence are documentation and guarded test instrumentation; no production
  summoning, publication, settings, or persistence code changed.
- Two final clean exact-reference builds from artifact source
  `a6bdfccc4c814ca26769d71816db2f1069702f48` passed repository validation,
  `1009/1009` domain tests, warnings-as-errors Release compilation, and strict
  package validation. Both emitted identical DLL and package bytes.
- Final SHA-256 values: DLL
  `e26d025cd8642a4bc30c0170e88110ba65e5b44a91df8a6337b360b08d683d45`;
  canonical deterministic 45-file package
  `4167473340865ad188b8f6fe0e434c18c614375e6ad5fe243183edea0b7edbec`;
  deterministic source archive
  `b640dc899f4450f4018be774eda5c42ef18818bd5d788f13d1431f578e75afed`.

## 2026-08-12 - Definition-of-done audit reopened

- A literal completion audit found that the durable mission file summarized,
  rather than preserved, the full 22-section authorization and stopping
  contract. It now contains the complete baseline, roster, architecture,
  safety, qualification, publication, and final-response requirements.
- The deterministic roster generator and generated ledger now cite final native
  qualification source `5205805eab3fe0115d6888c53bce73c80474d1b7` and the
  final structural, mechanical, visual, persistence, and compatibility runs.
- The fidelity header now cites the same final evidence. The inventory now
  labels early donor-candidate text as historical, records the missing exact
  Medium/Greater Air Elemental GUIDs, and points to the superseding 55-donor
  frozen audit. No runtime or production blueprint behavior changed.
- Completion remains intentionally open while the final requirement-to-evidence
  pass confirms explicit optional-feat and lifecycle-edge coverage. This audit
  corrects the earlier status estimate without discarding any passing evidence.

## 2026-08-12 - Requirement-to-evidence audit complete

- Added `planning/EXPANDED-SUMMONING-COMPLETION-AUDIT.md`, mapping all 22
  mission sections to source, domain, native runtime, persistence, module,
  profile, build, publication, and restoration evidence.
- The final-live observer's normalized fact search proves no Sacred Summons
  structure exists in the installed supported graph. This satisfies the
  authorized optional-surface fail-closed path; Augment and Superior remain
  directly exercised.
- Lifecycle review confirmed 153/153 exact native `SummonedUnitBuff` instances,
  CL20 duration, and enabled/disabled fresh-process context/control/expiration
  cleanup. No KMG-specific rest, area-transition, caster-death, RTwP, or
  turn-based lifecycle path exists, so those boundaries retain exact Owlcat
  semantics.
- No implementation correction was required. Remaining automated work is the
  final deterministic artifact/hash freeze, evidence update, PR refresh, and
  clean local/origin verification.

## 2026-08-12 - Deterministic final artifact freeze

- The first audit freeze exposed nondeterministic metadata in the canonical
  `Compress-Archive` ZIP even though the DLL and local-runtime ZIP were stable.
  Commit `8e08834da9a4fb9c31ade7e7ad1cea94b6a44edd` routes the canonical package
  through the existing fixed-timestamp writer and adds a domain source contract.
- Two clean exact-reference builds from `8e08834` each passed repository
  validation, all `1009/1009` domain tests, warnings-as-errors Release compile,
  focused icon checks, build-output validation, and strict package validation.
- Both builds emitted identical DLLs and identical canonical/local-runtime
  45-file packages. DLL SHA-256:
  `729f9d1d833747ee2a3cc9978db68ce2a1ed5732ee52908ffd418bb380a3ac9c`.
  Package SHA-256:
  `165d4a47d0b5933fbd04da00b28f1f5362ec5408a96bce122ee836bbef222d53`.
- Two independent `git archive` runs over the artifact source matched at
  `608629da01e41a4f8524e764806f4c161a4eb9f449c1e1d8b8dfdf9cb31c32c5`.
  Generated packages remain ignored and uncommitted.

## 2026-08-12 - Human first-playtest failure reopens qualification

- Human acceptance through the actual spellbook/UI route found that most
  templated natural/proxy SM choices no-op without spending a slot, while
  native summons, elementals, mephits and most special KMG summons work.
- The failure partition makes the nested KMG logical-root ->
  Celestial/Fiendish execution-child path the first hypothesis. The prior
  153-command suite granted execution children directly, so it is now labeled
  lower-layer evidence rather than end-to-end acceptance.
- Erinyes is tracked as an independent campaign-donor failure. Presentation
  repair also covers exact native duplicate reconciliation, single-before-
  quantity ordering, immutable icon selection and a Medium Air Elemental view
  for Invisible Stalker.
- Repair baseline is clean pushed head
  `e9f251c584607dd45a45a2414e2aaffabff4c44b`; branch and draft PR #2 are
  retained. Exact next action is a discriminating real-spellbook reproduction.

## 2026-08-12 - Nested template wrapper reproduced and repaired

- Authoritative pre-repair run
  `20260812T1729340453662Z-0ebb1d302b9045389454b13fbe812a06`
  reproduced the player failure: native SM I Dog, direct Celestial/Fiendish
  Dog children, SNA I Dog, and Small Earth Elemental all executed, while the
  KMG SM Dog and Giant Spider logical roots threw `Can't cast variational
  ability` before `RuleCastSpell`; their slots remained unspent.
- SNA I Dog used the same KMG Dog unit and remained non-destroyed in the loaded
  scene after `EntityCreator.Tick`, proving that the shared natural unit was
  viable and the nested player-facing `AbilityVariants` was the primary root
  cause. Erinyes separately executed and created a bound live view, leaving
  its human-visible presentation failure as a distinct chassis/render task.
- All 182 templated SM logical placements now clone the proven native spawn
  graph directly. A single post-spawn action chooses Celestial for good,
  Fiendish for evil, and an off-by-default persistent per-character mode for
  morally neutral casters (Celestial off, Fiendish on). The 364 old execution
  identities remain frozen and registered but are no longer nested beneath
  player-facing roots.
- Three append-only identities were added for the neutral mode, bringing the
  feature foundation to 1,158, repository active identities to 1,412, and the
  total ledger to 1,413 including one reserved identity.
- The first repaired runtime run showed every case succeeding and spending
  exactly one slot, but exposed a harness-only error: it required membership
  in `Game.State.AllUnits`, which the native Dog control also lacked. Exact
  loaded-scene membership, non-destroyed/in-state status, holding state,
  descriptor, and bound view are the authoritative native-matched contract;
  `AllUnits` remains diagnostic.
- Corrected fresh-process run
  `20260812T1747088695607Z-de3328d9413644bfab2a460382b70e9c`
  passed 7/7 assertions. Native Dog, KMG Dog, SNA Dog, Giant Spider, Small
  Earth Elemental, Erinyes, and both direct-child controls each fired the
  expected rules; all real-parent cases spent exactly one prepared slot and
  cleanup passed. Static validation, 1,010/1,010 domain tests, exact-reference
  Release build, deterministic package construction, and strict package
  validation also passed.

## 2026-08-12 - Player-path, presentation, and Erinyes repair checkpoint

- The production catalog now suppresses 48 frozen GUID-mapped semantic native
  duplicates without mutating or unregistering their blueprints. Display order
  is current-tier singles, unique native singles, 1d3 choices, then 1d4+1
  choices; unknown third-party entries retain stable relative order. All 67
  creature keys use an immutable donor/category icon policy.
- Invisible Stalker now uses the exact Medium Air Elemental view rather than
  the Huge Air Elemental view. The true Huge Air Elemental unit is unchanged.
- Guarded actual-parent run
  `20260812T1820149527504Z-1fd92e30891847a6970a5b4d34c470d7`
  passed the expanded 25-case human path, including native Dog, representative
  natural/proxy/special summons, neutral template modes, quantities, SNA,
  Movanic Deva, and Frost Giant with exact one-slot expenditure and cleanup.
- Guarded actual-parent run
  `20260812T1824547130714Z-668cd85d823f48dea9f4bc9b7fb6667d`
  then passed every one of the 681 approved logical placements through its
  actual native spellbook parent, proving exact kind, legal quantity, template
  and alignment, live post-tick renderer/state, one-slot spend, and cleanup.
- Erinyes' distinct human presentation failure was traced to the non-dedicated
  campaign donor's inherited `BuffOnEntityCreated` / `AppearFromFog` component.
  Its KMG-owned profile now retains the proven ranged body, brain, facts, and
  view while replacing the campaign component graph with a fresh 9-HD outsider
  chassis and explicit Medium lawful-evil tabletop statistics.
- Save-free structural run
  `20260812T1838020780354Z-93eb2a40a02c4d77a9386354d9a5d46a`
  passed all exact contracts: 681 roots, 1,412 registered identities, 182
  direct template roots, and the fog-free Erinyes profile. Visual run
  `20260812T1842003981829Z-32445fd9cdab4e28a3af586148dbc9d4`
  passed all 67 views, bounded footprints, locomotion, attacks, hits, deaths,
  projectile origins, and cleanup. Repository validation, 1,013 domain tests,
  and the Release build pass. No protected save was selected or modified.

## 2026-08-12 - Complete final-live menu audit PASS

- The structural observer now derives a reference-exact expected display
  sequence for each of the 18 canonical parents from the frozen 681-placement
  KMG catalog and 48-GUID native catalog. It proves every KMG root is singular,
  mapped semantic native duplicates are absent, unique native choices are
  singular, and every suppressed native identity remains registered.
- It also proves current-tier KMG singles precede unique native singles, then
  1d3, then 1d4+1 choices; any unclassified third-party entries form a stable
  tail. All KMG icons are non-null, required animal/element/mephit/outsider
  categories are reference-distinct, and quantity names carry their dice.
- Guarded run
  `20260812T1850060235984Z-f8fb2f115ddb41b28d8720a4ca342a50`
  passed every audit. It flushed 27 seconds after the orchestration script's
  conservative 120-second result window, so later repetitions use the existing
  `-TimeoutSeconds 240` parameter; the runtime result itself is complete PASS.
- Final standalone menu totals are: SM I-IX =
  `3,13,21,35,44,56,64,68,69`; SNA I-IX =
  `5,14,21,33,40,47,53,57,59`. SM VIII retains the exact native Movanic
  Deva/Frost Giant conditional option and SM IX retains its exact unique native
  option. Zero unclassified third-party entries were present in this profile.

## 2026-08-12 - Cancelled-cast and standalone-elemental contracts PASS

- The actual-parent player-path harness now records the native cancellation
  boundary. Kingmaker accepts a distant summon click so the controller can
  approach; the test initializes that real command, proves it is not yet in
  cast range, cancels before `Start`, and observes zero `RuleCastSpell`, spawn
  actions, `RuleSummonUnit`, or prepared-slot expenditure (`7 -> 7`).
- Guarded run
  `20260812T1912292727051Z-1bb26c2ee8c648df91fbb021fba1fe37`
  passed all nine assertions, including the cancellation contract and a fresh
  `681/681` actual-native-parent repetition. Every successful cast spent
  exactly one slot and completed exact-kind, quantity, post-tick live-world,
  renderer, template/alignment, and cleanup contracts.
- The structural observer now freezes Owlcat's six standalone Summon Elemental
  roots by exact GUID. Guarded run
  `20260812T1920367972975Z-41fb84e591cc47eb8cacecf3ca2c1ad4`
  passed 37/37 assertions: every root retains exactly four original non-KMG
  one-spawn children, with zero KMG template/alignment action contamination.
  The standalone spell is unchanged. Repository validation, all 1,013 domain
  tests, the exact-reference Release build, and `git diff --check` pass.

## 2026-08-12 - Repair persistence, module, compatibility, and regressions

- Repeated the guarded enabled persistence sequence on pushed `9cb5f54`:
  prepare `20260812T1929470099833Z-bb3292f38b3644b6923940b52fc491e4`,
  cleanup `20260812T1932384938123Z-38502fdc53534e7bab8b9ff1f0c9737b`,
  and absence `20260812T1935299471647Z-9e6533fcb51d421ea31e545fdd28aa51`.
  Repeated disabled load safety with prepare
  `20260812T1938060304326Z-4f4f1d75920e45fdb86513c0bb48c61d`,
  cleanup `20260812T1941047028065Z-e853f261c5e64fe9bd172692d7538997`,
  and absence `20260812T1943438977236Z-003950a75a8d4c448b8865c651413c33`.
  All six stages passed.
- All 16 fresh feature-module configurations passed with constant 1,412
  registrations, independent publication, and settings restored to SHA-256
  `424da4573acb5dc9e3c7ca3546da688a1405702858fb3b28aea5cbae28c4ba3e`.
- Required repaired-source compatibility passed with exact restoration:
  standalone `compat-20260812T201850Z-c5a07cfc955e` and
  `compat-20260812T202109Z-c1bd197ce6ca`; Call of the Wild
  `compat-20260812T202324Z-10ac0552d9eb` and
  `compat-20260812T202715Z-eee0aa7926aa`; Arms and Armor
  `compat-20260812T203105Z-c11cd33c2df9`; Toggle Custom Soundpacks
  `compat-20260812T203318Z-e4887a3c64fe`; highest-risk combined
  `compat-20260812T203535Z-72786ce6ba3a` and
  `compat-20260812T203926Z-3da6a8617ea8`.
- The Acadamae fixture initially guessed the KMG Dog internal name. It now
  derives the exact published logical root from the frozen summon catalog.
  Fresh run `20260812T2058347680701Z-8cdc06e4886a41cba7ac7592462864b5`
  passed 13/13 through the native Summon Monster I parent and retained Cord
  behavior. Shield Other passed 23/23 in
  `20260812T2101121022865Z-89f2bb1c40ea4d43abba943f1ce05b8e`.
  Paper cartridge/firearm/vendor comprehensive passed 6/6 in high-risk
  transaction `compat-20260812T212522Z-f2d2def65434`.
- The broad Gunslinger comprehensive run passed 184 assertions except its
  known detached Gunslinger's Dodge fixture. Instrumented attempts proved its
  save-free unit has no Swift-action controller: the command interrupts before
  `Start`, with no resource spend, execution process, or production effect
  entry. All diagnostic source edits were removed. This is an inherited
  fixture limitation; no summoning or Dodge gameplay source changed.
- Assembly reflection proved spell descriptor state exists only on the shared
  `BlueprintAbility`; no per-invocation `AbilityData` descriptor exists.
  Dynamic shared mutation was rejected as nondeterministic and cache-unsafe.
  Direct roots keep `Summoning`; spawned alignment, Celestial/Fiendish buff,
  and smite remain caster-correct. Supported profiles expose no Sacred Summons
  surface, so the optional path fails closed.
- Bounded high-tier review retained the frozen roster. No dedicated safe
  Astral Deva, Trumpet Archon, or high-tier fiend addition was proven; those
  candidates remain deferred instead of using weak campaign proxies.
