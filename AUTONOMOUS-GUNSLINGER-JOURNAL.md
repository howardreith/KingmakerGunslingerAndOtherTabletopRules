# Autonomous Gunslinger journal

## 2026-08-01 initial mission audit

- Branch: `codex/complete-gunslinger`
- Starting HEAD: `5c92012701873421adff1fc0e127b0b3597c352c`
- Qualified runtime foundation: implementation `4f28dcf`; documentation HEAD
  `5c92012`; version `0.0.30`.
- Work completed: read the durable mission, repository instructions, current
  roadmap, Sprint 30 entry/report, architecture, source inventory, test
  inventory, and local Gunslinger/firearm rules headings. Created the required
  initial implementation plan, conservative mandatory coverage matrix, base
  feature fidelity matrix, blocker ledger, journal, and resume handoff.
- Current coverage: 5 of 32 matrix areas are runtime-qualified (15.6%); none of
  the mission's final integration rows is complete.
- Uncertainty: historical runtime evidence proves the Test Musket foundations,
  but does not prove Sprint 30's generic definition-driven adapters in game.
- Current first gate: guarded exact-version 0.0.30 working-save smoke, followed
  by Sprint 30 feature-specific acceptance.
- Next command: `scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario working-save-smoke -ExpectedVersion 0.0.30 -SaveName KMG_AUTOMATION_WORKING -ExitAfterCompletion:$true -Confirm:$false`.
- Source/package evidence: repository validation PASS; exact .NET Framework 4.7
  dependency-free suite PASS, 611/611; exact private-reference Release compile
  PASS; strict standalone eight-file package validation PASS. Local runtime
  package SHA-256 `b253eaed27bccfd7841ca938032373bb13146984e94c021b1994cbd901397dfd`;
  DLL SHA-256 `5ce1b5bf0d3563648e9fcd9629981c4ee41cf2fb59143df7dedf4f94fbe373de`.
- Host note: remove the duplicate uppercase `PATH` environment entry in the
  invoking process before MSBuild; otherwise Roslyn reports duplicate `Path`.
  The portable .NET path also attempted an unauthorized NuGet audit, so the
  offline exact .NET Framework suite was used as the authoritative full suite.

## 2026-08-01 exact-assembly runtime foundation

- Qualified commit: `47fb861a3cc2245998c4c3c64931c05dfb5a8830`.
- `mod-load-smoke` PASS run ID
  `20260801T0435526657821Z-4ba4ea84718947f1a8cfc3de1d6ad76a` at
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260801T0435526405707Z-mod-load-smoke`.
- `working-save-smoke` PASS run ID
  `20260801T0437220565711Z-d8664d7f634542f58d8d95126e90fe51` at
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260801T0437220409268Z-working-save-smoke`.
- Exact deployed DLL SHA-256:
  `b1422ae9a2aed50a0ae8a8d2d3f4ff0defc5d03d31f142d7a19c57f5eb973d7b`.
- Working-save evidence proves exact working/baseline discrimination, exact
  receiver-bound load correlation, stable fingerprint, and no observed
  save-writing API. `KMG_AUTOMATION_BASELINE` was not loaded or mutated.
- First `mod-load-smoke` orchestration shell timed out after the final PASS was
  atomically flushed; the game exited automatically and the structured result
  is authoritative. No second launch was used to manufacture that result.
- Remaining Sprint 30 invariant: execute the existing in-memory two-firearm
  maintenance fixture after guarded working-save load and emit explicit generic
  marker-first action/Heavy Crossbow isolation assertions. No existing scenario
  currently invokes this fixture.
- Next exact action: add a guarded `generic-firearm-actions` scenario by
  extending the qualified working-save flow only after its stable fingerprint;
  invoke the existing one-command maintenance diagnostic without saving, add
  request/orchestrator tests, and source-qualify before live execution.

## 2026-08-01 generic firearm actions source checkpoint

- Added guarded `generic-firearm-actions`, locked to
  `KMG_AUTOMATION_WORKING` and the qualified Steam/receiver-bound load path.
- The scenario runs only after a stable post-load fingerprint, requires the
  existing two-item maintenance evaluator to reach `MaintenanceLoopPassed`,
  proves the concrete native Heavy Crossbow type has zero firearm markers while
  the distinct Test Musket type has exactly one, and retains save-write
  sentinels through final evidence.
- Focused tests PASS: generic-action scenario 10, runtime request 14, scenario
  preflight 31, orchestration initialization 9, runner safety 19.
- Repository validation PASS; exact dependency-free domain suite PASS 611/611;
  exact private-reference Release compile PASS; strict standalone package PASS;
  scenario WhatIf PASS with no deployment or launch.
- Candidate package SHA-256:
  `b6e136110c3039813d0901ca231071a17f845434e58df3fef6ae688f2c53ac5f`.
- Candidate DLL SHA-256:
  `530a96be4133e6b574dab2638da07336ae41a6454584deceea6773486af07531`.
- Next command after committing the source-qualified checkpoint: run
  `mod-load-smoke` for the exact commit, then run `generic-firearm-actions`.

## 2026-08-01 Sprint 30 runtime-qualified

- Exact commit `0052dad0dae299eeefd302e511a0ae4b57dcdbac` passed
  `mod-load-smoke` run
  `20260801T0445314107675Z-8c9b8ceb1bea4cb5ae1a7655a3e98913`.
- Two consecutive fresh-process `generic-firearm-actions` PASS runs:
  `20260801T0446479175229Z-feac50caa3fd439a80b9a09c7a383cc0`
  and `20260801T0448285054152Z-4e5925080ce1422fbcb44c2ee07adcac`.
- Both feature runs deployed DLL SHA-256
  `de9f8507e5180adeb5df8dab4559e901da68022be556ef4fe1ffb874034e3d3f`.
- Both reached `MaintenanceLoopPassed`, observed
  `nativeMarkers=0;markedMarkers=1`, used exact working-save correlation, and
  observed no save-writing API. Baseline was distinguished and never loaded.
- Sprint 30 generic definition-driven actions are RUNTIME-QUALIFIED. Current
  matrix runtime-qualified count is 6 of 32 areas (18.8%); final integration
  remains pending.
- Next checkpoint: production early firearm catalog (pistol, musket,
  blunderbuss) using stable blueprint IDs and generic runtime. Establish
  definition/presentation/acquisition acceptance criteria before source edits.

## 2026-08-01 Sprint 31 entry and pistol-definition checkpoint

- Recorded the authoritative early pistol, musket, and blunderbuss table and
  explicit Sprint 31 acceptance criteria. The source calls blunderbuss range
  `special`; no numeric single-target range has been invented, and scatter
  execution remains assigned to Sprint 32.
- Advanced active development metadata and guarded-runtime exact-version
  contracts to `0.0.31`; added inherited Sprint 31 repository validation and
  updated its dispatch integration coverage.
- Added the canonical fresh-instance early-pistol definition factory with
  exact capacity 1, 20-foot increment, misfire 1, 5-foot burst, standard
  free-hand reload, and non-scatter identity.
- Repository validation PASS; validator-dispatch integration PASS 7/7; clean
  Release domain suite PASS 613/613; exact private-reference Release compile,
  build-output validation, and strict standalone package validation PASS.
- Local package SHA-256:
  `07cee3091fe57307192ef0f7419a7f8d842db775295171dbf31910e3dcc4d04b`.
  DLL SHA-256:
  `74c7837e3041b6b1aee01249dffd698c4dd5e14e9d50252b6aba4ad13d1b7c93`.
- Next action: commit this source-qualified narrow checkpoint, prove exact
  `0.0.31` mod load and working-save smoke, then add production catalog
  blueprint contracts and registrations.

## 2026-08-01 Sprint 31 exact-commit runtime foundation

- Exact commit `67d777980c6eb71649298fceb979978c9243a306` passed
  guarded `mod-load-smoke`, run ID
  `20260801T1309524765214Z-f92ce74df2bc4c6695e6cdd3a6bbeeed`.
- The same commit passed guarded `working-save-smoke`, run ID
  `20260801T1311109692839Z-c700cd91ca9c45e5aa9082adbc3ec263`.
- Exact built and deployed DLL SHA-256:
  `f133220a212d0cd6b7af21a58fc42af4f04f00c693329e3eb95185593eed6eaa`.
  Local package SHA-256:
  `495ed142e8f2b54723b1bd331365998a4739556d2c43ddb587713842c5a42a39`.
- Working-save evidence proved one exact working descriptor and distinct
  baseline, exact receiver-bound load correlation, stable expected
  fingerprint, automatic exit, and no observed save-writing API.
- Post-commit audit covered 23 explicit text paths and found no generated,
  private, save, binary, credential, or machine-local payloads. The worktree
  was clean and the checkpoint was reconstructable before runtime launch.
- Next action: implement the production early-firearm catalog blueprint
  contracts and registrations; retain fail-closed handling for the unresolved
  numeric blunderbuss range until the existing local contract supports its
  `special` range without an invented balance value.

## 2026-08-01 special-range firearm definition checkpoint

- Extended the immutable firearm definition and serialized marker vocabulary
  to distinguish a fixed numeric increment from an authoritative `special`
  range profile.
- Added canonical early blunderbuss construction with capacity 1, misfire 1-2,
  10-foot burst, full-round free-hand reload, scatter identity, and no numeric
  range value.
- Fixed-range access on the special profile throws; the touch-AC selector
  retains ordinary AC with reason `special-range-not-implemented`, and combat
  tracing records `rangeIncrement=special` without performing range math.
- Added four focused tests. Repository validation PASS; clean Release domain
  suite PASS 617/617; exact private-reference Release compile, build-output
  validation, and strict package validation PASS.
- Candidate package SHA-256:
  `4d4e74081145a272ce1257bd0055da843b0c46ddb47427727fc07a8f92b2b664`.
  Candidate DLL SHA-256:
  `8493d5e3106b58ebbbff3fdc6b83972c865c934cef36c395630743f107844f15`.
- Next action: commit this source-qualified slice, run exact-commit guarded
  mod-load smoke, then implement the production blueprint catalog.

## 2026-08-01 Sprint 31 production catalog runtime-qualified

- Commits `ab9aef1` and `1539ae9` added six stable production blueprints for
  Pistol, Musket, and fail-closed special-range Blunderbuss content plus the
  guarded `production-firearm-catalog` scenario.
- Exact commit `1539ae9` passed `mod-load-smoke`, run ID
  `20260801T1334059331758Z-9736bc0a7d7844bd83bc9d26b5a30676`.
- Two independent fresh-process catalog PASS runs:
  `20260801T1335276981327Z-5145ec8fbc864500889d489fb4c23fad` and
  `20260801T1336546357107Z-1986affde5794aad8eb0710a31932eb0`.
- Both feature runs proved exact runtime item/type mechanics, one marker per
  production type, native Heavy Crossbow isolation, one fail-closed
  Blunderbuss restriction, stable exact working-save fingerprint, and no
  observed save-writing API. The baseline was distinguished and never loaded.
- Clean Release/package gates passed with 624/624 domain tests. Exact package
  SHA-256 was `0ca093bd05eaa19a6dc3e3577b618fea2b3db018b29e61965fc0742815e2c342`;
  DLL SHA-256 was
  `c0e59abe94e89ec478a55c43327c8ce7763851dc1d50f4a141c39e7ad0767473`.
- Next checkpoint: Sprint 32 scatter execution. Establish a contract-backed,
  fail-closed cone/target plan before enabling the production Blunderbuss.

## 2026-08-01 Sprint 32 entry investigation

- Read the roadmap and authorized local scatter rules. Separate per-target
  attacks, -2 rolls, per-target critical confirmation, all-roll misfire, and
  triple scatter-explosion damage are authoritative.
- The available local source labels Blunderbuss range `special` but contains no
  numeric cone length. Sprint 32 criteria therefore prohibit inventing one and
  keep the production Blunderbuss unavailable during exact contract research.
- Next action: inspect installed Kingmaker unit-enumeration, geometry,
  attack-modifier, attack-roll, and damage-delivery contracts; then implement
  the pure deduplicating target-plan slice that does not depend on a guessed
  cone length.

## 2026-08-01 Sprint 32 exact-reference target-plan checkpoint

- Advanced active development metadata to `0.0.32` with an inherited Sprint 32
  validator; sealed Sprint 31 remains fixed at 624 tests.
- Added dependency-free scatter candidates with explicit Inside, Outside, and
  Unknown geometry dispositions. Unknown geometry fails the complete plan
  closed rather than yielding partial targets.
- Added an immutable planner that excludes the exact wielder and outside-cone
  candidates, deduplicates only exact unit references, preserves distinct
  value-equal units, and sorts accepted targets by distance and stable identity.
- Added ten focused tests. Repository/dispatch/request/preflight validation
  passed; clean Release domain suite passed 634/634; exact private-reference
  compile and strict standalone package validation passed.
- Candidate package SHA-256:
  `aedfef956e9b89812a2276eb4e69f1e606dae25742ebdba1b73fec1ec4085341`.
  Candidate DLL SHA-256:
  `3207707a5771162f71e8ea32cc59aff50e9cbbb64d76b5799579ca0a44bf544a`.
- Next action: commit this source-qualified slice, inspect exact directional
  geometry and per-target native attack contracts, and keep Blunderbuss
  availability fail-closed.

## 2026-08-01 Sprint 32 native cone and volley checkpoint

- Exact target-plan commit `31678e2` passed guarded `mod-load-smoke`, run ID
  `20260801T1349406203357Z-1f2d98755c364d8c812fb2a6cbb629cf`.
- Read-only installed 2.1.7b reflection and narrow IL inspection established
  the native 90-degree cone contract: LOS, eye position, target corpulence,
  center range plus radius, and cone-edge intersection. The authoritative
  numeric Blunderbuss distance remains absent and is still fail-closed.
- Recorded ADR-0037 and added one-roll-per-exact-target volley aggregation,
  `-2` penalty authority, target-specific critical counts, non-vacuous all-roll
  misfire, and precision/Vital Strike exclusions. Non-scatter definitions,
  missing rolls, duplicate rolls, and unplanned targets reject explicitly.
- Added ten focused tests. Repository validation and clean Release domain suite
  passed 644/644; exact private-reference compile and strict package validation
  passed.
- Candidate package SHA-256:
  `a1110e94d47cef39a3dfed6f45d33858d9de9bc1757bd91e1a7b51e27d02dee3`.
  Candidate DLL SHA-256:
  `edf19afb4d93a571195b97fb28614529f20b8604c8f7c4a5b41f8c1ead6bc310`.
- Next action: commit this slice and implement the one-discharge scatter action
  transaction boundary while keeping runtime delivery unavailable.

## 2026-08-01 Sprint 32 one-discharge checkpoint

- Added a pure scatter discharge transaction boundary layered on the canonical
  firearm Fire transition. All delivery prerequisites are validated first.
- A loaded action consumes exactly one chamber for zero, one, or many accepted
  targets. Pre-delivery rejection preserves state and consumes nothing; empty
  and Wrecked attempts retain their established forced-miss behavior.
- Non-scatter definitions reject before evaluation. Added seven focused tests;
  full clean Release suite passed 651/651, exact private-reference compile and
  strict package validation passed.
- Candidate package SHA-256:
  `2342ed2a56fe3d8e1c3b776769062eb1d3d7a6207b3eb1063856be619954bfee`.
  Candidate DLL SHA-256:
  `cc264db8df526787eda54e6aea4b38278814aecfd455d8f848927427e8e56de5`.
- Next action: commit this source slice, complete the remaining local authority
  audit for numeric Blunderbuss cone distance, then proceed or establish the
  precise design hard stop without weakening fail-closed availability.

## 2026-08-01 Sprint 32 scatter-explosion checkpoint

- Added a fail-closed scatter explosion damage policy. A nonempty scatter
  volley applies the authoritative triple base damage only when every separate
  attack roll misfires; partial and vacuous volley evidence rejects.
- Preserved ordinary firearm explosion damage at one times base damage and
  rejected scatter evidence supplied for a non-scatter firearm.
- Added six focused tests. Repository validation and the clean Release domain
  suite passed 657/657; exact private-reference compile and strict standalone
  package validation passed.
- Candidate package SHA-256:
  `85cf357c9fe80387f7afef659b9fad69d194170b7fede30885534e47de472adf`.
  Candidate DLL SHA-256:
  `9a2c79aa55721c88dd12bd61beaa83c0c755a65f26975caa420a8cd66e48ed36`.
- Runtime delivery remains unavailable: the project still contains no
  authoritative numeric Blunderbuss cone distance.
- Next action: commit this source-qualified slice and continue the independent
  native scatter-delivery adapter boundary without inventing that distance.

## 2026-08-01 Sprint 32 cone-distance boundary checkpoint

- Added the explicit boundary between a future project-authorized cone length
  and the native cone API's meter input. Missing authority and non-scatter
  definitions reject; supplied values must be bounded five-foot steps.
- The boundary converts feet to meters deterministically but supplies no
  default, so the production Blunderbuss remains unavailable.
- Added five focused tests. Repository validation and clean Release suite
  passed 662/662; exact compile and strict package validation passed.
- Candidate package SHA-256:
  `e45b9e2253435a1e5926dccc6c9e00a9cadc96ae25af404a2749e0cc6249a639`.
  Candidate DLL SHA-256:
  `33b9cddac997428d945c35e156f04ee004e7109d1a10bb08ed5fa409886c67f7`.
- Next action: commit this boundary, then preserve a clean human-input record
  for the missing player-facing distance and move to the next independent
  mandatory coverage dependency.

## 2026-08-01 Sprint 33 multi-round reload foundation

- Advanced active development metadata to `0.0.33` and added inherited Sprint
  33 validation plus explicit capacity/partial-reload entry criteria.
- Generalized the atomic basic-ammunition transaction to consume an exact
  positive batch and generalized exact-item reload to add the lesser of the
  action batch and remaining capacity.
- Partial top-ups preserve ammunition identity. Full, Wrecked, mixed-ammunition,
  and under-resourced attempts reject before mutation; post-mutation failures
  retain exact state and inventory rollback.
- Removed the generic action policy's capacity-one/full-round restriction, so
  valid partial and advanced move-action definitions can report reload
  availability while full or impossible states remain unavailable.
- Added eight focused cases. Validation dispatch passed 9 checks, runtime
  request tests 16, preflight tests 34, and the complete clean Release suite
  passed 670/670. Exact compile and strict package validation passed.
- Candidate package SHA-256:
  `f04448c1cc8de40b9eae5ad781730a4c911a10cd3d9aec83ce6f560d2710dd55`.
  Candidate DLL SHA-256:
  `75788f9ff56f76a3012754802e0c78d847da11c6c1ecd806abada23e065e2b51`.
- Next action: commit this source foundation, then add exact advanced firearm
  definitions/catalog entries and capacity-aware finite persistence.

## 2026-08-01 Sprint 33 advanced definitions and state semantics

- Added canonical Advanced Rifle and Advanced Revolver factories and production
  catalog specs from the authorized local table. Revolver is the table-backed
  advanced one-handed/pistol-form entry; no unsupported generic pistol stats
  were invented.
- Added a bounded finite-token catalog for every Normal and Broken load count
  through a requested capacity while preserving all four legacy capacity-one
  token IDs and absence-as-empty semantics. Six-round coverage contains 14
  exact states and round-trips each token deterministically.
- Generalized misfire condition evaluation for remaining chambers. A first
  advanced misfire preserves remaining rounds while becoming Broken; repeated
  advanced misfires remain Broken without explosion. Repeated early-firearm
  misfires still empty, Wreck, and schedule burst damage.
- Added twelve focused tests. Repository validation and clean Release suite
  passed 682/682; exact compile and strict package validation passed.
- Candidate package SHA-256:
  `35527b3cba8d7764b3882e8910c58d43bc600741c1e13bc33ba6ff679afde94a`.
  Candidate DLL SHA-256:
  `9ebbfaaf3dd023441c5e424b777fc04b4609ae2d5185ac8f953ff0fb11ec46ac`.
- The save-owned vault remains the authoritative production carrier; expanded
  token definitions are a bounded compatibility vocabulary and are not newly
  registered blueprints.
- Next action: commit this slice and prove save-owned-vault reconstruction,
  repeated discharge, and exact two-item isolation.

## 2026-08-01 Sprint 33 durable capacity isolation

- Proved six-round state survives reconstruction through a new
  `VaultBackedFirearmStateRepository` over the same save-owned store.
- Proved two reference-distinct revolvers retain independent six- and two-round
  records, and two repeated canonical Fire transitions on one reduce only that
  item from six to four chambers while the identical second remains at six.
- Added three focused tests. Repository validation and the clean Release suite
  passed 685/685; exact compile and strict package validation passed.
- Candidate package SHA-256:
  `ea88730ad1a2aa208de081fb4e8e68000dc53e26be17b24403eeda1cd3d4d26e`.
  Candidate DLL SHA-256:
  `5408f50e47c189115c42d3067250ade2491b2030b8f8b90acb60c7b2a42c82ad`.
- Next action: commit this persistence/isolation checkpoint, then build the
  production advanced blueprints and guarded Sprint 33 runtime scenario.

## 2026-08-01 Sprint 33 advanced production blueprints

- Appended four stable manifest identities and registered isolated production
  Advanced Rifle and Advanced Revolver weapon-type/item pairs. Rifle uses the
  Heavy Crossbow presentation source and Revolver the Light Crossbow source;
  source blueprints remain reference-verified unchanged.
- Each custom type carries one exact firearm marker, each item carries one
  Firearm Proficiency restriction, and both are player-fireable. The complete
  production catalog now contains five distinct firearm entries and ten custom
  item/type blueprints; total active registration count is 24.
- Extended the existing guarded production-catalog assertion to cover exact
  advanced specs and marker/native-source isolation.
- Repository validation and clean Release suite passed 685/685; exact compile
  and strict package validation passed.
- Candidate package SHA-256:
  `30e5f6b53efcdef9068e2524945bad49b06a78e77ce745c9c29afc79bf0ad957`.
  Candidate DLL SHA-256:
  `5621847b5197518f97e97912eb680b6a024adfde20448ee40fd8c496fa9deae5`.
- Next action: commit this source slice, pass guarded exact-assembly mod load,
  then add the capacity-specific runtime acceptance scenario.

## 2026-08-01 Sprint 33 advanced catalog runtime qualification

- Exact commit `41f299a` passed guarded `mod-load-smoke`, run ID
  `20260801T1433034839705Z-56c2363396894593961542057943f189`.
- Expanded five-entry `production-firearm-catalog` passed in two consecutive
  independent fresh processes, run IDs
  `20260801T1434411929092Z-b69f2b13cc1f4a03945624f83ff3c5b9` and
  `20260801T1436113008241Z-3c3f36a8807e4ed3869826afd13a5543`.
- Both runs loaded `0.0.33`, retained the exact working-save fingerprint,
  proved advanced spec/marker/native-source isolation, and observed no
  save-writing API.
- Exact deployed package SHA-256:
  `6a8386e782f47726c38be60cda52e5e9b335d943a3650426cf6263c5deb51cf2`.
  Exact deployed DLL SHA-256:
  `ba0f28e9197e1fb6949de9e829a3ccd60b65d865cea2962b6a06528ee87b4a64`.
- Next action: commit curated evidence and implement the guarded capacity
  transaction scenario; catalog proof alone is not live round-count proof.

## 2026-08-01 Sprint 33 guarded capacity scenario source

- Added autonomous `advanced-capacity`, restricted to the exact
  `KMG_AUTOMATION_WORKING` save and the existing guarded Steam launch path.
- After exact save correlation completes, request-local compiled fixtures prove
  two atomic six-round reloads, two-firearm discharge isolation, remaining-round
  preservation, and the advanced repeated-misfire no-explosion policy. The
  scenario deliberately does not claim native inventory or save-vault mutation.
- Added request, metadata, preflight, and ten scenario-source checks. Repository
  validation and the clean Release suite passed 685/685; strict package
  validation passed.
- Candidate package SHA-256:
  `3945f9938d64e3b6b2e263ca216a6a1b8c50942fb1657703e8150c4a132a84db`.
  Candidate DLL SHA-256:
  `38b633bce9ab969c5b39d695cbf80ee7239ab4a9b5523e5398ada779813ae5da`.
- Next action: commit this source checkpoint, rebuild the exact commit, then run
  guarded mod-load and two fresh-process capacity acceptances.

## 2026-08-01 Sprint 33 advanced capacity runtime qualification

- Exact commit `f851ae2` passed guarded mod load, run ID
  `20260801T1447365484819Z-10da17eb731c4ba08e76fce5ac216fef`.
- Two independent `advanced-capacity` fresh-process runs passed, run IDs
  `20260801T1448590698485Z-c8d4099979754579b6bc68fde049bbc5` and
  `20260801T1450305002704Z-681c230576604963897eda2927f1c866`.
- Both proved exact six/six reload, four/six post-discharge isolation, advanced
  repeated-misfire no-explosion behavior, stable working-save correlation, and
  no save-writing API. Fixtures were request-local and do not claim native
  inventory or save-vault mutation.
- Exact package/DLL SHA-256:
  `01774ec30fbe71580b3bc3ff7e63337aad0784e7881b868553ed88539d4a284a` /
  `3e610b43025b2771bd9003396ca469cd10e9028dfb7d5fb180f7f821369622d3`.
- Next action: commit curated evidence and begin the next incomplete coverage
  item; Sprint 33 is a checkpoint, not a stopping condition.

## 2026-08-01 Sprint 34 class chassis foundation

- Advanced the active version to `0.0.34` and added inherited Sprint 34
  validation plus exact entry criteria.
- Added an engine-independent canonical 20-level Gunslinger chassis: d10 hit
  die, full BAB, good Fortitude/Reflex, poor Will, and four skill ranks per
  level. Invalid levels fail closed and level rows have deterministic value
  semantics.
- Added six focused tests. Repository validation and the clean Release suite
  passed 691/691; strict package validation passed.
- Candidate package/DLL SHA-256:
  `8be4a23e89208264e0fc1db936f5721d34c1aed76152370ce92b2475a7cf9bc9` /
  `026272d69e9e7044ce525964ad3bc0b9a2a419cfdeed4869ab7fcff37267b5d0`.
- Removed one disposable `artifacts/tmp/KmgValidatorTests-*` fixture left by a
  sandbox-denied validator run; it contained only copied test repository data.
- Next action: commit this foundation, then implement stable production class
  and progression blueprints with exact supported skill/proficiency mappings.

## 2026-08-01 Sprint 34 native class-contract observer

- Added read-only guarded `observe-class-blueprint-contracts`. It records the
  exact native character-class array and each class's stable identity,
  progression, BAB/save progressions, hit die, and supported skill enum values.
- The scenario requires no save, input, UI interaction, registry write, or
  native blueprint mutation and runs only through the guarded Steam harness.
- Seven focused scenario checks, repository validation, 691/691 domain tests,
  clean Release build, and strict package validation passed.
- Candidate package/DLL SHA-256:
  `1a69c5baafc6e9c8d8a14274aa30cf79ed546711a72f633098ab67ff0c491c65` /
  `ffac2365ce5414c4b218d142af5618030c6299c829b1d26c886027f53f290f93`.
- Next action: commit, pass exact mod load, run the observer, curate the exact
  native references, then implement class/progression blueprints.

## 2026-08-01 Sprint 34 level-one feature contract extension

- Exact observer run `20260801T1506494769744Z-6243addc919545bfb797ebaa75c04a26`
  proved 24 complete native classes and identified full BAB
  `b3057560ffff3514299e8b93e7648a9d`, good save
  `ff4662bde9e75f145853417313842751`, and poor save
  `dc0c7c1aba755c54f96c089cdf7d14a3`.
- Extended the read-only observer to record exact level-one feature identities
  so simple/martial weapon and light-armor proficiency features are not guessed.
- Eight focused observer checks, repository validation, 691/691 tests, clean
  Release build, and package validation passed.
- Candidate package/DLL SHA-256:
  `94869c11589b051dcea920acbbe119dc31cb1f2031cb71dbfa7b14a3ae76f4ec` /
  `f483eaa91f6b10597b3bf658993f07a714847b8910a1d7ab72b331dbb0107d94`.
- Next action: commit and run exact mod load plus the extended observer.

## 2026-08-01 Sprint 34 exact proficiency-array observer

- Exact extended observer run
  `20260801T1511119074497Z-e8ca571c082a4e5f9242d68f6af90a36` identified
  aggregate Fighter, Ranger, and Rogue level-one proficiency feature IDs.
- Added read-only `AddProficiencies` component reporting for exact armor-group
  and weapon-category arrays; this distinguishes Gunslinger's required
  simple/martial plus light-armor scope from broader native bundles.
- Nine focused observer checks, 691/691 tests, clean Release build, and strict
  package validation passed. Candidate package/DLL SHA-256:
  `2bbad61365a954c6dce9ecc2f0478352c0680898a1515a781cf10c63f578c4e4` /
  `e33760ed93718f375eae369264b2b04941afe225817edc88b3a6096feb47ab69`.
- Next action: commit, exact mod load, and run the narrowed observer.

## 2026-08-01 Sprint 34 nested proficiency fact observer

- Exact direct-component observer run
  `20260801T1515199250253Z-9108990c8712453aa54aaaff144dcfd4` proved Rogue's
  four direct weapon categories but showed Fighter's aggregate feature delegates
  through nested facts.
- Added exact `AddFacts` identity and nested `AddProficiencies` reporting.
  Ten focused observer checks, 691/691 tests, clean Release build, and package
  validation passed. Candidate package/DLL SHA-256:
  `5f7449215f2424022d05761258584ae57e050ed0adccb7ef346df9a1e1b603b9` /
  `a5a05bf49500904ff2119273fbd3ae116746e0c51597c6716a925b6f4fc66380`.
- Next action: commit and run the final narrowed observation.

## 2026-08-01 Sprint 34 multi-component observer repair

- Exact run at commit `755c617` returned structured ERROR because a nested
  native proficiency fact contains multiple `AddProficiencies` components;
  `SingleOrDefault` rejected that real cardinality. No mutation or save API was
  involved.
- Repaired the read-only formatter to preserve every direct and nested
  proficiency component deterministically. Ten focused checks, 691/691 tests,
  clean Release build, and package validation passed.
- Candidate package/DLL SHA-256:
  `accb96788a21f413d1a9ec2dfdda81b5fa5a59485a35ca58fe1407ac5a266bbd` /
  `31659470103ef2b880bcf6bea1d0304e5f7c68cf31fbf5fad30d58bcd1ca5317`.
- Next action: commit repair, mod load, rerun observation.

## 2026-08-01 Sprint 34 production class blueprint registration

- Repaired observer run
  `20260801T1523097042011Z-c1f35f9724dc43039beaf3158351d326`
  established the exact native Fighter proficiency facts: simple weapon
  `e70ecf1ed95ca2f40b754f1adb22bbdd`, martial weapon
  `203992ef5b35c864390b4e4a1e200629`, and light armor
  `6d3728d4e9c9898458fe5e9532951132`. The whole martial fact is retained because
  it owns multiple native proficiency components.
- Added stable class, progression, and aggregate-proficiency blueprints. The
  class uses d10, full BAB, good Fortitude/Reflex, poor Will, four skill ranks,
  eight supported Kingmaker class skills, and exact levels 1-20. Level one
  grants simple, martial, light armor, and the existing firearm proficiency.
- Seven focused production checks, repository validation, 691/691 domain tests,
  clean Release build, build-output validation, and strict package validation
  passed. Package/DLL SHA-256 are
  `03a932e7a828e632e31832774c3d859b40fb9902b8b60444bf395b73605d6bb8` /
  `d7deac56b4d245b61543ce69c4005597342da36d5f19b1ff77a58f2af5e68921`.
- Guarded Steam `mod-load-smoke` PASS evidence:
  `20260801T1531363590993Z-mod-load-smoke`. No save was required or mutated.
- The class is registered but intentionally not yet appended to the native
  character-class catalog. Next action: commit this qualified registration
  slice, then implement fail-closed catalog publication and a read-only exact
  Gunslinger structure observer.

## 2026-08-01 Sprint 34 character-class catalog publication

- Added fail-closed native catalog publication. It rejects unavailable or null
  catalogs and duplicate reference/GUID identities, verifies the exact assigned
  array, and restores only the exact array it published if later initialization
  fails; concurrent catalog changes cause rollback refusal rather than overwrite.
- Eight focused production checks, repository validation, 691/691 domain tests,
  clean Release build, build-output validation, and strict package validation
  passed. Candidate package/DLL SHA-256 are
  `668befdf0fe76ea4af1698105050821a579f21f8776da7dbe99a0a1d18fd4216` /
  `896cd312df98c9373512a188bbd1766e29168313e2385c248b723468b085a40d`.
- Fresh guarded Steam mod load passed at
  `20260801T1535128902157Z-mod-load-smoke`.
- Fresh read-only catalog observation passed at
  `20260801T1536273988367Z-observe-class-blueprint-contracts`. It observed 25
  complete class entries and exactly one Gunslinger class
  `abca4797366d4df0831a418eee39069a`, progression
  `c3f6b41e0f4c4526af4bbd2a3b54aba2`, d10/full BAB/good Fortitude and Reflex/
  poor Will/eight skills, plus level-one aggregate
  `b9b6769f8a654a58a6bd55e10801ea22` containing the three exact native facts
  and Firearm Proficiency `5148f69223044799800b65732b6cabea`.
- Neither scenario required a save or invoked save or input APIs. Next action:
  checkpoint publication, then inspect the creation contract and add starting
  equipment before disposable-character qualification.

## 2026-08-01 Sprint 34 starting-item contract observer

- Extended the existing read-only class observer to record each native class's
  exact starting-item names and blueprint GUIDs. Eleven focused observer checks,
  repository validation, 691/691 tests, clean Release build, and strict package
  validation passed. Candidate package/DLL SHA-256 are
  `617aa64cc2d91f7c4d0be8bd1c970c132920a82b7aa00a6480f19e704c402b82` /
  `73c64a1080bae3f656accc157593b56722bc6218257d790069fe215f63a8b6df`.
- Guarded save-free Steam observation PASS evidence:
  `20260801T1539170231770Z-observe-class-blueprint-contracts`.
- Next action: commit the observer extension, curate the exact native item
  contracts, and wire the authorized production firearm/ammunition kit.

## 2026-08-01 Sprint 34 production starting kit

- Native observation established that `BlueprintCharacterClass.StartingItems`
  is an ordered item-reference array and repeated references express quantity.
  No source authority specifies an invented ammunition quantity, so the exact
  minimum functional package is one Early Pistol, one black-powder charge, and
  one lead ball. The separate battered-state/Gunsmith adaptation remains open.
- Wired the class to existing production identities and added exact null,
  cardinality, order, and reference checks. Nine focused production checks,
  repository validation, 691/691 domain tests, clean Release build, output
  validation, and strict packaging passed. Candidate package/DLL SHA-256 are
  `a846b35e2bfb2d0616f02520770285963f12444220f8af894e965b45d38c7efb` /
  `a3a3d0eb4454b1629842b373a4d1b637da836f45fb5bfea1b4fbcf88dd5e975b`.
- Fresh guarded Steam mod load passed at
  `20260801T1542218801301Z-mod-load-smoke`. Fresh save-free class observation
  passed at `20260801T1543407360853Z-observe-class-blueprint-contracts` and
  proved exactly Early Pistol `a303d71d244640959827e9464df5a867`, black
  powder `ea966bf998a647cf97b0ed92f71c4b7d`, and lead ball
  `55c29771445947d685dba9e1ead46a42` on the Gunslinger class.
- Next action: commit this starting-kit contract, then establish a guarded,
  disposable in-memory character-creation observer before claiming item grant.

## 2026-08-01 Sprint 34 character-creation contract observer

- Added save-free guarded `observe-character-creation-contracts`. It performs
  metadata-only reflection over eight exact Assembly-CSharp types and invokes
  no constructor, method, UI, input, save, registry, or game-state mutation.
- Five focused scenario checks, repository validation, 691/691 domain tests,
  clean Release build, output validation, and strict package validation passed.
  Candidate package/DLL SHA-256 are
  `b8594aa6f40d98afcbd7a0b7492597d3890c8dc5fbb63c4bd1f6e89fc4f6ba1e` /
  `da04e3e67b7a5834ef5ce1bbfc1bf710126ca9ed2776fd72eb4090837f7da624`.
- Guarded Steam PASS evidence:
  `20260801T1548242006681Z-observe-character-creation-contracts`.
- Exact contracts include `ChargenUnit(BlueprintUnit)`,
  `LevelUpController.StartWithoutAssigningStaticInstance`, `SelectClass`,
  `ApplyClassMechanics`, `Commit`, `Cancel`, `UnitDescriptor.AddStartingInventory`,
  and descriptor/entity disposal. Next action: commit this observer, identify an
  exact disposable `BlueprintUnit` source and prove construction/cleanup without
  attaching the unit to player, scene, roster, or save state.

## 2026-08-01 Sprint 34 rooted chargen-unit contract extension

- Extended the metadata observer with exact `BlueprintRoot`, `CharGenRoot`, and
  `BlueprintUnit` contracts plus direct, read-only rooted unit identities. It
  does not call `GetPregens`, `PreparePregensCoroutine`, or any constructor.
- Six focused checks, repository validation, 691/691 tests, clean Release build,
  and strict package validation passed. Candidate package/DLL SHA-256 are
  `0432ba6526a2f0a3a10a146d222a8f0c676b90b6aa0634b79f47e17ca14145b5` /
  `402300ec6e125f93c6c7b409097c150219a40ee5eacfa241ee1a81736387e40e`.
- Guarded Steam PASS evidence:
  `20260801T1553095129386Z-observe-character-creation-contracts`. Exact default
  player source is `StartGame_Player_Unit@4391e8b9afbb0cf43aeba700c089f56d`;
  five complete pregen sources were also observed without initializing them.
- Next action: commit the extension, then construct only a disposable
  `ChargenUnit` from the exact default-player source, prove it is absent from
  player/scene/roster state, and dispose it before attempting class selection.

## 2026-08-01 Sprint 34 disposable descriptor construction boundary

- The first exact `ChargenUnit(BlueprintUnit)` experiment failed closed because
  native construction attached a preview view; cleanup succeeded with unchanged
  empty party/global-unit snapshots. The materially different direct descriptor
  boundary uses the observed non-public `UnitDescriptor(BlueprintUnit)` contract.
- Six focused checks, repository validation, 691/691 domain tests, clean
  exact-reference Release build, output validation, and strict packaging pass.
  Candidate package/DLL SHA-256 are
  `22323d1f29763d35f5da6ff813858a58794e990180f9f5d5ff6ae2840aa8ad40` /
  `a529443ea74a6c1b847e76c187399a331c4cb7590aaef10d5df74b982d045099`.
- Two independent fresh-process guarded Steam runs passed:
  `20260801T1600264550680Z-34603ecdf85e45e69d11e30fb98b1bc1` and
  `20260801T1601407816389Z-cd6a9e332a0442ae8b755355cee55e4e`.
  Both constructed the exact default-player descriptor with no entity, party,
  or `Game.State.AllUnits` attachment and proved unchanged snapshots after
  `Dispose`. Neither scenario loaded or wrote a save.
- Next action: checkpoint the qualified boundary, then use it to observe exact
  isolated Gunslinger class selection and cancellation before any commit or
  starting-inventory invocation.

## 2026-08-01 Sprint 34 isolated Gunslinger selection boundary

- Exact metadata observation resolved nested
  `LevelUpState.CharBuildMode` values and progression/class-data contracts. A
  bare descriptor could not satisfy native `RequestPreview`; the native
  `ChargenUnit` preview owner did, while remaining externally isolated.
- `SelectClass` returned true, `State.SelectedClass` was the exact production
  Gunslinger, and the controller queued exact `SelectClass` and
  `ApplyClassMechanics` actions. The source descriptor correctly remained level
  zero because actions were not applied. `Cancel` preserved level zero and
  entity disposal left party/global-unit snapshots unchanged.
- Six focused selection checks, repository validation, 691/691 tests, clean
  Release, and strict packaging passed. Candidate package/DLL SHA-256 are
  `20f2855f7ddee207866eb9ed7f6870e62e81e3b75711749f0015be98e21e852a` /
  `f95378fe0b32a261e9934ec5d8642c9972c665d9e261322647a1489a96019fd8`.
- Independent fresh-process PASS run IDs are
  `20260801T1614399128650Z-853ebae7a0054627aab7b589eba025b9` and
  `20260801T1615556388755Z-efbaedd302864c00b5e8b7e6b6601b73`.
  Neither run loaded or wrote a save.
- Next action: checkpoint exact selection/cancel, then observe a safe native
  action-application or preview-result boundary before inventory grant.

## 2026-08-01 Sprint 34 controller-owned preview mechanics

- The controller automatically refreshes its owned preview when level-up
  actions are added. An explicit second `ApplyLevelup(Preview)` correctly
  exposed duplicate application (preview level two) and was removed rather
  than normalized into acceptance.
- The qualified path starts at Gunslinger level zero on distinct source and
  preview descriptors. `SelectClass` advances only the preview to level one;
  `ApplyClassMechanics` leaves it at one, the queue contains exactly two
  actions, and the source remains level zero. Cancel and entity disposal leave
  party/global-unit snapshots unchanged.
- Five focused preview checks, repository validation, 691/691 tests, clean
  exact-reference Release, and strict packaging passed. Candidate package/DLL
  SHA-256 are
  `d4979fefda3ba457565809c51aa559aaed3d54d041cc4beba767bfda32e4f30d` /
  `9a8d8d8b8cdf085c6acbd07101678bb990541cd0e380024b9b8ada5f517244c7`.
- Independent fresh-process PASS run IDs are
  `20260801T1626016361171Z-cf3feca2f0f84ca8b058b0663604f4d2` and
  `20260801T1627170968339Z-57f79cac173340bfb71c82dd45b8b296`.
  Neither run loaded or wrote a save.
- Next action: checkpoint preview mechanics, then observe exact preview facts
  and stats before invoking starting inventory on the disposable preview.

## 2026-08-01 Sprint 34 exact preview class data

- Extended the qualified controller-preview boundary to exact native
  `ClassData`. Both runs proved level one and production full-BAB
  `b3057560ffff3514299e8b93e7648a9d`, good Fortitude/Reflex
  `ff4662bde9e75f145853417313842751`, and poor Will
  `dc0c7c1aba755c54f96c089cdf7d14a3` identities.
- The first broader assertion also observed that `Progression.Features` is
  still empty at this stage. The proficiency aggregate is therefore not
  claimed as materialized; it remains a separate controller-refresh/commit
  contract investigation.
- Six focused preview checks, repository validation, 691/691 tests, clean
  Release, and strict packaging passed. Candidate package/DLL SHA-256 are
  `d92acf8e2d114da1885273cae04f04b7695fbb3c6a53de4e816c413d0eed3fc5` /
  `d1a7118126e3c428719c8aa08109647852bfebf542555ac62335d63f08a385d9`.
- Independent fresh-process PASS run IDs are
  `20260801T1632531458628Z-092ddb04a7df4a86b677183ec448970e` and
  `20260801T1634085276178Z-30f01289d10d4e96be16f1bc82de5824`.
- Next action: checkpoint exact class data, then observe the native preview
  refresh/commit boundary required to materialize feature facts before adding
  starting inventory.

## 2026-08-01 Sprint 34 exact preview proficiency facts

- Two application theories appeared to show an empty feature store. Narrow
  metadata inspection established that `Progression.Features` is a native
  `FeatureCollection`; contents are exposed by its declared `Enumerable`
  property rather than by directly enumerating the collection.
- Exact declared-property observation proved one production aggregate
  `b9b6769f8a654a58a6bd55e10801ea22` and one each of simple weapons
  `e70ecf1ed95ca2f40b754f1adb22bbdd`, martial weapons
  `203992ef5b35c864390b4e4a1e200629`, light armor
  `6d3728d4e9c9898458fe5e9532951132`, and firearm proficiency
  `5148f69223044799800b65732b6cabea` on the disposable level-one preview.
- Eight focused preview checks, repository validation, 691/691 tests, clean
  Release, and strict packaging passed. Candidate package/DLL SHA-256 are
  `8cc336312e067eb125673cc840992318142f562b821498cf604ebba1c21047e7` /
  `c27521a85b2f233da33b14aef83bd52c5b26ae0497bf772c6c1a999db7e99190`.
- Fresh-process PASS run IDs are
  `20260801T1646395750915Z-05d1dda2cd1d4306aa07f19f0ff40e12` and
  `20260801T1647528331254Z-26e8105b5534435282cfc6c83b9411ed`.
- Next action: checkpoint the exact facts, then invoke `AddStartingInventory`
  only on the disposable refreshed preview and observe exact item identities.

## 2026-08-01 Sprint 34 starting-inventory contract correction

- A guarded disposable-preview call to `UnitDescriptor.AddStartingInventory`
  did not grant class items. It added two instances of default-player blueprint
  item `850c86ad8b953c74bb85f9bb1d1148bc`, proving that method consumes
  `BlueprintUnit.StartingInventory`, not `BlueprintCharacterClass.StartingItems`.
- Structured FAIL evidence is
  `20260801T1653067738684Z-31c75c40cc2642038ec2771b3c2325fd`.
  Class selection, class data, all exact proficiency facts, cancel, disposal,
  and external isolation still passed; no save was loaded or written.
- The incorrect call and assertion were removed, restoring a clean tree at
  `a8674ca`. Next action is read-only native commit/setup contract inspection,
  then a separately guarded disposable creation-commit scenario if supported.

## 2026-08-01 Sprint 34 native class-item grant call graph

- Added metadata-only IL call-graph resolution for exact creation methods.
  `Commit` calls `ApplyLevelup` and first-level `SetupNewCharacher`; setup can
  attach the entity to player state and calls default-unit inventory, so it is
  not an isolated receiver.
- The exact setup delegate resolves to
  `LevelUpHelper.AddStartingItems(UnitDescriptor)`. Its call graph reads the
  maximum class/class data, adds class items to the descriptor inventory,
  equips compatible items, applies `AddStartingEquipment` components, and may
  call `Player.GainMoney`.
- Seven focused observer checks, repository validation, 691/691 tests, clean
  Release, and strict packaging passed. Candidate package/DLL SHA-256 are
  `339b65d5722dc85ec51e78be3ccf6da490450eaecf2b1bc1abfb43220b508561` /
  `7deadb7d7af294a74449444058e428becdc21147e7e44206ee55552328df9e6a`.
- Save-free guarded metadata PASS run
  `20260801T1704113460224Z-67a8c8fb00194ac28840c0b3d3012d39` invoked none
  of the inspected methods.
- Next action: checkpoint the call graph, then invoke only the exact helper on
  the disposable preview with `StartingGold` temporarily zeroed/restored and
  require unchanged player money and exact class-item deltas.

## 2026-08-01 Sprint 34 class-item helper receiver gate

- Two materially different save-free receivers were tested against exact
  `LevelUpHelper.AddStartingItems`: the controller preview and the disposable
  source after exact two-action application. Both returned safely with no item
  additions because neither unit is the real main character or a custom
  companion. Player money, class gold, party/global-unit snapshots, and cleanup
  remained correct.
- Structured FAIL evidence is
  `20260801T1708158441657Z-f718c0f1dd60494cae934f9512e572eb` and
  `20260801T1710226633438Z-aa873b9035364e4eabd74d27b5c3a0d6`.
- The failed helper mutations were removed, restoring the qualified call-graph
  source at `8e391b2`. Per the two-attempt rule, no third save-free receiver
  variation is authorized. Next mode is a guarded `KMG_AUTOMATION_WORKING`
  scenario using the actual main character with exact item/money snapshots,
  explicit rollback, no save, and unchanged fingerprint evidence.

## 2026-08-01 Sprint 34 native starting-item runtime qualification

- Added guarded `gunslinger-starting-items` acceptance on the already-qualified
  receiver-bound `KMG_AUTOMATION_WORKING` lifecycle. It temporarily substitutes
  only the real main receiver's readonly maximum-class identity, zeros/restores
  production class gold, calls exact `LevelUpHelper.AddStartingItems`, and
  restores exact inventory quantities/references and all altered values.
- The first exact-commit diagnostic run
  `20260801T1727050485069Z-f37fe13f57be4cbfa5eceeebd34f5898`
  proved native ammunition stack merging: the pistol was a new instance while
  powder and ball merged into existing stacks. It also exposed an ambiguous
  reflective Blueprint property read. Structured FAIL was preserved; exact
  working-save correlation, class/gold/inventory-reference restoration, and no
  save write passed.
- Repair commit `cc2f77d` measures native total quantities, removes the exact
  new instance plus only merged excess quantities, restores class/gold first in
  `finally`, and verifies final money directly. Repository validation, request
  tests 18/18, preflight tests 40/40, 691/691 domain tests, exact private-
  reference Release compile, output validation, and strict package validation
  passed.
- Exact-assembly mod-load PASS:
  `20260801T1731148707849Z-mod-load-smoke`. Two independent fresh-process
  starting-item PASS run IDs are
  `20260801T1732334037249Z-ccbfd7861d04442782c358cf7d236dc9` and
  `20260801T1734133597055Z-4fa5d631d72f4b999751ceeb7d119fd5`.
  Both observed `added=1;pistol=1;powder=1;ball=1`, complete inventory/class/
  gold/money rollback, stable exact save fingerprint, and no save-writing API.
- Exact package/DLL SHA-256 are
  `6a41ed815d910983e0067184fdfb40ca16629b8e70aab83f4d1a24fa4ff57153` /
  `2f4d2a0f2772b5923349ce467bbebf7426bb80348c25548d16f0238af23d0fb4`.
- Next action: inspect and qualify the normal creation commit outcome without
  persisting a character, then proceed to level-up, multiclass, and respec.

## 2026-08-01 Sprint 34 same-class level-up preview

- Exact creation `Commit` remains too broad for the current rollback proof: its
  first-level setup performs global rest, entity registration, remote-companion
  mutation, body/default inventory initialization, view attachment, and dynamic
  root parenting. This is an engineering boundary, not a human-input blocker.
- Added save-free `disposable-gunslinger-levelup-preview`. It applies the exact
  two level-one actions to an isolated source, cancels that controller, starts
  native `LevelUp` mode, selects Gunslinger again, and observes the isolated
  preview at Gunslinger 2 while the source remains Gunslinger 1.
- Six focused checks, repository validation, 691/691 tests, exact private-
  reference Release build, output validation, and strict packaging passed.
  Exact commit is `84bb692`; mod-load PASS evidence directory is
  `20260801T1741332784385Z-mod-load-smoke`.
- Fresh-process PASS run IDs are
  `20260801T1742575116740Z-8a6cca94fc1c4d97bda6a25e01dad80a`
  and `20260801T1744173560342Z-aae78c49ec4849d19bedbde1f12446fb`.
  Both observed `initial=0;seeded=1;previewBefore=1;selected=True;previewAfter=2;sourceAfter=1;queued=2`
  and exact external cleanup; no save was loaded.
- Package/DLL SHA-256 are
  `a1a2e199df996427eef7ca7f123fbdac9da37709c51f7ada5d2960a08455d63e` /
  `386772a6ab12125a40d7647e1d1049ed64a5c2bead7f81ade123d17b735e2472`.
- Next action: reuse the isolated level-one seed to qualify Fighter 1 ->
  Gunslinger 1 multiclass preview, then inspect respec mode contracts.

## 2026-08-01 Sprint 34 multiclass preview

- Added save-free `disposable-gunslinger-multiclass-preview` using the exact
  native Fighter blueprint as a disposable level-one seed, then exact
  `LevelUp` mode with Gunslinger selection.
- Six focused checks, repository validation, 691/691 tests, exact Release
  compilation, output validation, and strict packaging passed on `c1eb9b7`.
  Mod-load PASS evidence is `20260801T1748568385594Z-mod-load-smoke`.
- Independent PASS run IDs are
  `20260801T1750132878481Z-0665958b379b4f8ca6067083a9ee9708` and
  `20260801T1751280423920Z-703b0d97c03843a28fedffe8c4392214`.
  Both proved source Fighter 1/Gunslinger 0, preview Fighter 1/Gunslinger 1,
  two exact queued actions, controller cancellation, and external isolation.
- Package/DLL SHA-256 are
  `14e5a1746638f1f1d48c4a9ccd79c92cd6307e1d2e96680355a7f8f873e9eedf` /
  `66265d13b598477701674ec05cf50dec009bf2809bca0a3cbd63533ec9cffd86`.
- Next action: inspect exact respec build modes and reset contracts, then add a
  save-free reversible respec preview if the native boundary supports one.

## 2026-08-01 Sprint 34 disposable respec hard stop

- Metadata-only commit `dd85431` added exact call graphs for both native
  `PrepareRespec` entry points. Mod load passed at
  `20260801T1757540871121Z-mod-load-smoke`; independent observation PASS runs
  were `20260801T1759106312890Z-observe-character-creation-contracts` and
  `20260801T1800273782467Z-observe-character-creation-contracts`.
- The observed direct calls were `UnitEntityData.get_Descriptor` plus
  `UnitDescriptor.PrepareRespec`, and `UnitDescriptor.set_Body`, respectively.
- Added save-free native Respec-mode preview commit `25d2da1`. All focused
  checks, repository validation, 691/691 tests, clean Release build, and strict
  package validation passed. Package/DLL SHA-256 were
  `d1a3d1facb21ae3e69e70b8ac2f6470060e22302af87618e9c72f43239d02f60` /
  `6328d59dfb8953fafc2170e2c4bf4b32cba65645f5d5763bdd1af0818cf8f59a`.
  Exact mod load passed at `20260801T1805094993616Z-mod-load-smoke`.
- First attempt `20260801T1806257874513Z-disposable-gunslinger-respec-preview`
  failed closed with `NullReferenceException` and no save load.
- Added deterministic stage labeling in `f2fbcc5`; all source gates passed,
  package/DLL SHA-256 were
  `8d2573a4befee67041e2641426fc8336ea6820b5b9a76d3c19b4de8c386410bb` /
  `df6a944204e8bd06a004914abfe6b354746069e5d831611fceff5815dc3ed14b`,
  and mod load passed at `20260801T1809469616185Z-mod-load-smoke`.
- The materially different run
  `20260801T1811040970058Z-disposable-gunslinger-respec-preview` also failed
  closed with `NullReferenceException`; cleanup masked the labeled inner
  exception. Per the two-attempt runtime stopping rule, no third launch was
  made. The hard stop is recorded in `AUTONOMOUS-BLOCKERS.md`.

## 2026-08-01 Sprint 34 respec cleanup mode change

- Re-auditing the mission corrected the prior hard-stop interpretation: two
  failed repairs require a mode change while safe evidence acquisition remains,
  not human input.
- Source-qualified metadata observer commit `07dd111` passed 691/691 tests,
  clean Release build, and strict package validation. Package/DLL SHA-256 were
  `57ae1bacf8722bcce22e13a3a605d1bd8dee5f554de1b437a3b8151fa003c679` /
  `854936d851c772f86f2390e8a5baf7d2de60c1f344514185171cf198e15a4c28`.
- Exact mod load passed at `20260801T1815425699140Z-mod-load-smoke` and the
  non-initiating observation passed at
  `20260801T1817013647054Z-observe-character-creation-contracts`.
- Exact metadata proves the `Body` setter has no nested managed calls;
  `UnitEntityData.Dispose` calls `UnitDescriptor.Dispose`, which calls
  `UnitBody.Dispose`. This explains why cleanup masked the prior stage error
  after `PrepareRespec` replaced or cleared the disposable body.
- Implemented an evidence-backed rollback that retains the original body and
  restores it through the exact nonpublic setter before entity disposal. Seven
  focused checks, repository validation, 691/691 tests, exact Release build,
  output validation, and strict packaging pass. Candidate package/DLL SHA-256
  are `7272b90e171d9d214120bd160c128af0fd5840ecab75171a03231840741c3247` /
  `893c59c2ca5ef697da0ebd83dc63ac18f05af0d2b62f3bbcb3f1f9c70c40dcde`.

## 2026-08-01 Sprint 34 detached-replacement respec qualification

- A reduced single-unit Respec run completed safely but proved native Respec
  mode alone retains Fighter 1. Exact installed `Player.RespecCompanion` IL
  established that Kingmaker instead creates a fresh same-blueprint unit and
  initiates respec on that level-zero replacement candidate.
- Commit `3d4ba8f` mirrors that reset boundary with two detached `ChargenUnit`
  instances and invokes no global creation, UI, replacement callback,
  `PrepareRespec`, or `Commit`.
- Eight focused checks, repository validation, 691/691 tests, exact Release
  build, output validation, and strict packaging passed. Package/DLL SHA-256
  are `fffd41f772c7b8b3668c7b8c4d2e8364a16a19cb118dccba112a67d826721e05` /
  `5616dfd5da3eb32431c28501fa53289d6866364e818fa37a9568f235a9e6f36e`.
- Exact mod load passed at `20260801T1836154433116Z-mod-load-smoke`.
  Independent PASS runs were
  `20260801T1837314150470Z-disposable-gunslinger-respec-preview` and
  `20260801T1838472989503Z-disposable-gunslinger-respec-preview`.
- Both proved replacement Fighter 0/Gunslinger 0 -> Gunslinger 1, unchanged
  source Fighter 1/Gunslinger 0, two queued actions, intact body, exact cleanup,
  and no save load/write.

## 2026-08-01 Sprint 35 grit domain entry

- Advanced the active repository/build/runtime version to `0.0.35` with an
  inherited Sprint 35 validator; validation-dispatch fixtures pass 11 checks.
- Recorded exact grit acceptance criteria from the authorized rules source:
  Wisdom modifier minimum 1, daily refill to maximum, bounded current value,
  explicit bonus maximum, atomic spend/restore, and per-unit operation dedupe.
- Added dependency-free grit state, service, transaction result/status, and
  operation gate plus 12 focused cases. Complete suite is 703/703 PASS.
- Clean exact-reference Release build, output validation, and strict package
  validation pass. Package/DLL SHA-256 are
  `527c076df6278f769159cb9e29a5faf3f3472f91f071ce76ec4508fad55351ce` /
  `d3d59745b713386502aa9ef23eefdf2651ae2059130486324352604d93b64a44`.
- This checkpoint is source-only. Next bind the exact Kingmaker per-unit
  resource and qualify persistence/rest before adding firearm recovery.

## 2026-08-01 Sprint 35 native grit resource source qualification

- Exact installed IL established that `BlueprintAbilityResource.GetMaxAmount`
  computes and caps its base before raising `IResourceAmountBonusHandler`, and
  that native `IncreaseResourceAmount` is an active owned-unit subscriber.
- Added two stable blueprint identities: a one-point-base native grit resource
  and its level-one owner feature. The feature restores on first activation but
  not level-up; its exact resource-filtered subscriber adds only positive
  `Wisdom modifier - 1`, yielding the rules floor without affecting other pools.
- Added guarded `disposable-gunslinger-grit-resource` acceptance for native
  grant, spend, second-level non-refill, capped restore, reference-snapshot
  isolation, controller cancellation, and entity disposal.
- Focused grit blueprint/runtime checks, inherited repository validation,
  703/703 domain/reflection tests, clean Release build, output validation, and
  strict package validation pass. Candidate package/DLL SHA-256 are
  `6ba5a00008d8d72b14a3db1439e519a02d5b347bdcca6c9db4916954d17ab5bd` /
  `8de23371914342fa66ae5a379c5d156ad874e4e233eaab081c094b068d6d6306`.
- Runtime launch correctly refused the dirty tree. Commit this source-qualified
  slice, then run exact mod load and two fresh-process feature passes.

### First live grit attempt

- Commit `aede630` passed mod load at
  `20260801T1903334246022Z-mod-load-smoke`.
- Run `20260801T1904500891309Z-disposable-gunslinger-grit-resource` failed
  safely before assertions at `grant-level-one-grit`: exact stack evidence
  located a null dereference inside native `BlueprintAbilityResource.GetMaxAmount`.
  No save was loaded or written.
- Root cause: runtime construction of native `Amount` does not receive Unity's
  serialized empty-array defaults. Narrow repair initializes `Class`,
  `Archetypes`, `ClassDiv`, and `ArchetypesDiv` to correctly typed empty arrays.

### Native grit resource runtime qualification

- Repair commit `cd22f3d` passed exact mod load at
  `20260801T1907337714075Z-mod-load-smoke`.
- Two independent fresh-process PASS runs were
  `20260801T1908491510715Z-disposable-gunslinger-grit-resource` and
  `20260801T1910149815825Z-disposable-gunslinger-grit-resource`.
- Both observed maximum/current 1/1 after level-one grant, current zero after
  spend, current still zero at Gunslinger level two, restored current one, and
  exact detached-entity cleanup with unchanged external reference snapshots.
- Exact repaired package/DLL SHA-256 are
  `4ddea7d37d08cd1255562cc8d21678ea686c01d1dc3a48ecaade6630d18c8fbd` /
  `da0d1e20a51dc288daa3383fbd0fff628b79b76194b7946c1e60a774d6d1543b`.
- Next checkpoint: exact daily-rest refill and persistence/respec behavior;
  native grit recovery from qualifying firearm events follows.

## 2026-08-01 Sprint 35 native daily-rest entry

- Exact installed IL proves public static `RestController.ApplyRest` enumerates
  every registered unit resource and invokes full native `Restore`; the exact
  supported-build `CanRestoreResourcesOnRest` helper returns true.
- Added a guarded detached-unit daily-rest scenario: grant Gunslinger 1, spend
  grit from one to zero, call the exact native unit rest method, require refill
  to maximum, cancel the controller, dispose the entity, and verify unchanged
  party/global-unit snapshots.
- Five focused rest checks, 40 runtime-preflight checks, inherited repository
  validation, 703/703 tests, clean Release build, and strict packaging pass.
  Candidate package/DLL SHA-256 are
  `5e4711b5fdfdd4c7ed478fa77e76ad2afddb124c691b2cf498cb5a69b00cd1a9` /
  `c77b482cbe7b9579cd69f88ececc3e551fdc43631e97a4466f4a01b04849f199`.
- Commit this source-qualified scenario, then require exact mod load and two
  independent fresh-process daily-rest PASS runs.

### Native daily-rest runtime qualification

- Commit `b0ca3f3` passed exact mod load at
  `20260801T1916580082273Z-mod-load-smoke`.
- Two independent fresh-process PASS runs were
  `20260801T1918185563653Z-disposable-gunslinger-grit-rest` and
  `20260801T1919385521658Z-disposable-gunslinger-grit-rest`.
- Both observed `maximum=1;initial=1;spent=0;rested=1` and exact detached
  cleanup with unchanged party/global-unit reference snapshots.
- Exact package/DLL SHA-256 are
  `9211a4cd8dfb0e9b2dc9c2092673f9b4fb947cf3849aa176685e13d5a4608694` /
  `997ad369b55856321a3c1ce8593dc219864a0a38857df086e46c5a8902f8e8d6`.
- Daily grit reset is runtime-qualified. Continue to exact save/load and
  multiclass/respec persistence; do not stop at this checkpoint.

## 2026-08-01 Sprint 35 native grit persistence entry

- Exact installed contracts mark `UnitAbilityResource`'s blueprint and amount
  as JSON properties with a JSON constructor. The owning collection's
  serialized `PersistantResources` setter rebuilds its blueprint-keyed map;
  native respec transfers this same property to the replacement unit.
- Added a guarded no-file round trip using Kingmaker's exact
  `DefaultJsonSettings`: detached Wisdom 14 Gunslinger grit reaches maximum two,
  spends to one, serializes to JSON, deserializes into a distinct exact-blueprint
  record, and reconstructs current one on a second detached descriptor.
- Five focused persistence checks, 40 runtime-preflight checks, inherited
  validation, 703/703 tests, clean Release build, and strict packaging pass.
  Candidate package/DLL SHA-256 are
  `ccf5facff8d846c8b6ac3598115fe9cebb57df9c6570b1a7aceb7f62b8cf3e2f` /
  `4d519cd6d807d01da2499402599101e5e7c599f951b9e978bb17a6b89ad67c61`.
- Commit, then run exact mod load and two independent fresh-process persistence
  PASS runs. This invokes no save API and writes no save file.
