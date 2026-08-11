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
