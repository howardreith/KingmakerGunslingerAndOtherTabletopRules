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

### First live persistence attempt

- Commit `d7ddc34` passed mod load at
  `20260801T1925523230210Z-mod-load-smoke`.
- Run `20260801T1927062718434Z-disposable-gunslinger-grit-persistence`
  proved exact JSON blueprint identity and collection reconstruction, but
  correctly failed its nontrivial value assertions: final maximum was two while
  initial current had been one, so spending one yielded zero.
- Root cause is native level application order: first feature activation fills
  the one-point base before final attribute recalculation. Add an exact global
  unit-level handler filtered to its owner, Gunslinger class, and class level
  exactly one. It restores after recalculation and never refills later levels.
- The repair passes nine focused grit-blueprint checks, inherited validation,
  703/703 tests, clean Release build, and strict packaging. Candidate
  package/DLL SHA-256 are
  `92b0c3133e81c5cac2b1abb0a8c0fb1f8f952bf28b1120350db54dae703e2686` /
  `400a78b2c5950f45978bacb1e0aaeabfff4afa172c27639818a6f39bb292de2a`.

### Persistence lifecycle mode change

- Repair run `20260801T1932396799284Z-disposable-gunslinger-grit-persistence`
  reproduced maximum two/current zero after spend. The global gain-level handler
  did not run on the detached `ApplyLevelup` path.
- Exact `ApplyLevelup` IL shows no gain-level event. It applies queued actions,
  then invokes unit progression `ReapplyFeaturesOnLevelUp`; native
  `AddAbilityResources.RestoreOnLevelUp` uses the corresponding unit-scoped
  subscriber interface.
- Replaced the unused global hook with that exact unit-scoped reapply contract,
  retaining the Gunslinger-class-level-equals-one guard. This is a new
  evidence-supported lifecycle architecture, not a third variation of the
  disproven global event theory.
- Unit-scoped repair passes nine focused blueprint checks, inherited validation,
  703/703 tests, clean Release build, and strict packaging. Candidate
  package/DLL SHA-256 are
  `0766e2c3e36dd8d84c8efad04e0e5293eda92bb1d101c898066e3af1f96ff503` /
  `3b9eccf6770898cc89493fc51a1754feda7a7459d32fbcef6672bda82b52f4d2`.

### One-time initialization guard

- Multiclass analysis found that a class-level-one-only reapply guard would
  still refill grit on later unrelated class levels while Gunslinger remained
  level one. Added stable hidden per-unit initialized marker
  `KMG.Classes.GunslingerGritInitialized`.
- First exact reapply restores and adds the marker; every later reapply sees the
  marker and preserves current grit. The persistence scenario now explicitly
  calls a later exact reapply after spending and requires current to remain one.
- Ten focused blueprint checks, six persistence checks, inherited validation,
  703/703 tests, clean Release build, and strict packaging pass. Candidate
  package/DLL SHA-256 are
  `1bdc5cdfd0e32d170b16037495441883beff4be96f0bcccaa4b74819f3768efc` /
  `c72d2e3c7bbdb3ebce182a8ab29bdd5a468e4e6fec38e489935ebf205898bdee`.

### Native persistence runtime qualification

- Commit `1949d80` passed exact mod load at
  `20260801T1935464087387Z-mod-load-smoke`.
- Two independent fresh-process PASS runs were
  `20260801T1937081415049Z-disposable-gunslinger-grit-persistence` and
  `20260801T1938272020280Z-disposable-gunslinger-grit-persistence`.
- Both observed maximum two/current one, a distinct JSON record retaining the
  exact grit blueprint, one reconstructed replacement record at current one,
  and exact two-entity cleanup. No save API was invoked.
- Exact package/DLL SHA-256 are
  `519c5f99a430808895d1ea787f832088f51b2b861b19974409d6bb2a02715429` /
  `0b37c7748cdca07c0015a730da3626b8a0a6a8d132c96e9ef72f64c325d60866`.
- Continue to explicit multiclass/respec value behavior, then qualifying
  firearm critical/killing-blow grit recovery.

### One-time initialization runtime qualification

- Commit `4e543fd` passed exact mod load at
  `20260801T1945287324618Z-mod-load-smoke`.
- Two independent fresh-process PASS runs were
  `20260801T1946581078281Z-disposable-gunslinger-grit-persistence` and
  `20260801T1948157306155Z-disposable-gunslinger-grit-persistence`.
- Both observed maximum two, spent/reconstructed current one, and current one
  again after an exact later feature reapply. This proves the stable persistent
  initialization marker prevents unrelated later-class levels from refilling
  grit while Gunslinger remains level one.
- Exact package/deployed-DLL SHA-256 are
  `3f32686c52b0c6b21082a5966eb006032e616d15674d6eb73c2d14bf5f078421` /
  `ca3c62f4b48abb2baba8217b78276282474c3ab5deeace7af9704fd42ce319a7`.
- Native respec transfers the same persistent resource-list contract already
  round-tripped here. Proceed immediately to firearm critical/killing-blow
  grit recovery and duplicate protection.

## 2026-08-01 Sprint 35 firearm grit recovery source qualification

- Exact installed metadata exposes final `RuleAttackRoll.IsCriticalConfirmed`,
  correlated `RuleDealDamage.AttackRoll`, target `HPLeft`, native combat state,
  helpless state, and character levels.
- Exact installed `RuleAttackWithWeapon.OnTrigger` IL proves it assigns the
  triggered damage rule to `MeleeDamage`; production killing-blow recovery
  therefore rejects explosion or secondary damage by reference inequality.
- Added separate confirmed-critical and killing-blow recovery paths. Critical
  dedupes once per attack-roll reference; killing blow dedupes once per attack
  and exact target. Both use weak identities and remain distinct clauses.
- Recovery fails closed for non-firearms, noncombat events, noncreatures,
  helpless or unaware targets, and targets below half the Gunslinger's total
  character level.
- Added guarded detached `disposable-gunslinger-grit-recovery`: Wisdom 14 grit
  spends two to zero, critical restores one, killing blow restores one, both
  duplicate calls preserve value, and an unaware-target critical is rejected.
- Eleven focused grit checks, seven scenario checks, 40 preflight checks,
  inherited validation, 710/710 tests, clean Release build, and strict package
  validation pass. Candidate package/DLL SHA-256 are
  `838a3882e5998da9496aaa608a55dfe73ced2433079f8f7ac97aee1b4d25047a` /
  `d589582a7a27d1e146009f7352a62bbd5e0e8c97fd6e96a02fef513dd6122c90`.
- Commit the reconstructable source checkpoint, then require exact mod load and
  two independent fresh-process recovery PASS runs.

### First firearm recovery runtime attempt

- Commit `a968b2e` passed exact mod load at
  `20260801T2005414175062Z-mod-load-smoke`.
- Run `20260801T2006592774427Z-disposable-gunslinger-grit-recovery` failed
  safely with all recovery evaluations ignored and no faults or leaked entity
  references.
- Exact `UnitEntityData.IsInCombat` IL delegates to
  `UnitCombatState.IsInCombat`; constructing that detached state does not set
  its exact `m_InCombat` backing flag. The fixture now sets that flag true on
  both detached states, asserts both native getters, and still clears the
  complete states during cleanup. Production recovery logic is unchanged.
- The repaired fixture passes eight focused checks, 40 isolated preflight
  checks, inherited validation, 710/710 tests, clean Release build, and strict
  packaging. Candidate package/DLL SHA-256 are
  `9b982127b5bebe48d07b7ea4199dcfc0da114a9d9376939a0c402726698f82c2` /
  `36583d9893390380aedb83e3e0e4d952d8303d9a2d60c23159246337fee59b53`.

### Firearm grit recovery runtime qualification

- Repair commit `b4c4874` passed exact mod load at
  `20260801T2010547590872Z-mod-load-smoke`.
- Independent fresh-process PASS runs were
  `20260801T2012145241329Z-disposable-gunslinger-grit-recovery` and
  `20260801T2013336952646Z-disposable-gunslinger-grit-recovery`.
- Both observed `0 -> 1` confirmed-critical recovery and `1 -> 2`
  killing-blow recovery; duplicate callbacks preserved one/two, and an unaware
  target preserved the post-spend value one. Diagnostics were exactly
  critical one, killing blow one, duplicates two, ignored one, faults zero.
- Both runs cleared detached combat states, restored disposable target damage,
  disposed both entities, and preserved party/global-unit reference snapshots.
- Exact package/deployed-DLL SHA-256 are
  `cbeeb299e4e10843cd01079b48ee8c702bd71cf4b469f99c294ac2fd4692bc93` /
  `1596f0b56a60144212e7680aa1dd1421decb90d507b1725c424b3ae3c3759138`.
- Recovery and dedupe are runtime-qualified. Continue immediately to the first
  incomplete level-one deed in fidelity order.

## 2026-08-01 Sprint 36 Deadeye runtime qualification

- Source commit `8ef8854` introduced version 0.0.36 and the first production
  deed: a level-one Deadeye feature, personal free action, and hidden native
  persisted armed fact.
- A successfully discharged exact firearm consumes the fact. Range-scaled grit
  spending is atomic, authorizes touch AC beyond the first increment, preserves
  contextual AC deltas, and leaves native range penalties untouched.
- Repository validation, 719/719 tests, 18 request checks, 40 preflight checks,
  13 dispatch checks, exact-reference Release build, and strict packaging pass.
- Exact mod load passed at `20260801T2039304720861Z-mod-load-smoke`.
  Independent feature PASS runs were `20260801T2040462109967Z` and
  `20260801T2042028613784Z`.
- Both observed initial grit two, second-increment authorization and spend to
  one, no duplicate spend, atomic third-increment insufficient rejection,
  diagnostics applied one/rejected one/faults zero, and exact cleanup.
- Package/DLL SHA-256 are
  `a59e090b2d14911f77f64ba57d26762d2692c1b25ae7e3b6bd38f25b48d7e8f8` /
  `ec58e53a0c9747a34fa55db2290219cf868f13c171978171b1622f9d45ea2426`.
- Deadeye is runtime-qualified. Continue immediately to Gunslinger's Dodge.

## 2026-08-01 Sprint 36 Gunslinger's Dodge drop-prone qualification

- Source commit `f79b4a2` implements the safely representable drop-prone branch:
  a personal free action arms a hidden persisted fact; the next incoming ranged
  weapon attack in light/medium armor at light load spends one grit, applies
  native prone, and adds +4 AC to that attack exactly once.
- Insufficient grit consumes the reaction marker but preserves grit, standing
  state, and native AC. Duplicate callbacks neither spend nor stack AC.
- The 5-foot movement / +2 AC alternative remains explicitly pending safe
  destination selection. Kingmaker has no Immediate command type, so the
  overall adapted deed remains runtime-partial rather than overstated.
- Repository validation, 727/727 tests, 18 request checks, 40 preflight checks,
  13 dispatch checks, clean exact-reference Release build, and strict package
  validation passed. The build helper now uses a response file to stay below
  the Windows process command-line limit.
- Exact mod load passed at `20260801T2059139120474Z-mod-load-smoke`.
  Independent feature PASS runs were `20260801T2102104686115Z` and
  `20260801T2103264904832Z`.
- Both observed grit two to one, native prone, AC 20 to 24, duplicate stability,
  atomic grit-zero rejection at AC 20, marker consumption, applied one,
  rejected one, duplicate one, faults zero, and exact detached cleanup.
- Package/DLL SHA-256 are
  `40d738a160929a4c611aaa0263a53fe0d48ccca6fab2ecc93d8fc400b7dd9b4a` /
  `798e1fe7f96cc083de8493e164e5e640ccad2592481ea97251a4fe6cd5815677`.
- Continue immediately to Quick Clear.

## 2026-08-01 Sprint 36 Quick Clear source qualification

- The authoritative rule is represented by two personal extraordinary actions:
  standard requires at least one current grit and spends none; move spends one
  grit. Both require exactly one equipped firearm whose item-owned state is
  Broken and repair it to Normal without a kit.
- The current architecture writes Broken only through firearm misfire damage,
  so the production adapter treats the exact Broken token as misfire-origin;
  no unrelated source can manufacture that state.
- Availability is fail-closed for zero grit, normal/wrecked state, no firearm,
  or ambiguous equipped firearms. Delivery re-evaluates the same exact item,
  rolls back a move-action grit spend and any partial state write on failure,
  and preserves loaded-round/ammunition fields through the existing repair
  transition.
- Added three stable production blueprints, a guarded save-free detached
  `disposable-gunslinger-quick-clear` scenario, five focused policy cases, and
  production diagnostics. Repository validation, 732/732 tests, 18 request
  checks, 40 preflight checks, exact-reference Release build, and strict package
  validation pass.
- Candidate package/DLL SHA-256 are
  `01fd8fe73c53575c08f957aae99cae21fdb262e333949698f670b89fc732dd28` /
  `34fd2cd6acdc105f378bed9ab276acb3b2771a382fbbde3348b05ca239fb6b41`.
- Next action: commit the reconstructable source checkpoint, require exact mod
  load, and run two independent fresh-process Quick Clear PASS runs.

### Quick Clear runtime qualification

- Source commit `fb4fd51` passed exact mod load at
  `20260801T2119244427175Z-mod-load-smoke`.
- Independent fresh-process PASS runs were
  `20260801T2120422933376Z-disposable-gunslinger-quick-clear` and
  `20260801T2122029596275Z-disposable-gunslinger-quick-clear`.
- Both observed standard grit two unchanged and Broken-to-Normal; move grit two
  to one and Broken-to-Normal; zero-grit rejection remaining Broken; applied
  two, rejected one, faults zero; and exact detached cleanup.
- Exact package/DLL SHA-256 are
  `0443b50c7af34857f5ae6eaf7fa491eaff52bae2b38fc133bec7d36d6f557ce4` /
  `e1c3d9273c73b0e1722922896128c60d21f035558b53cbb9f6fb43e9c792f746`.
- Quick Clear is runtime-qualified. Continue immediately to the next incomplete
  fidelity row; Sprint 36 is not a stopping condition.

## 2026-08-01 Sprint 37 Nimble source qualification

- Advanced the active version to `0.0.37-s37-class-integration` after the
  runtime-qualified Sprint 36 deed bundle.
- The authoritative rule is represented by five cumulative +1 class facts at
  levels 2, 6, 10, 14, and 18. Each adds an exact native Dodge-descriptor AC
  modifier only in light or no armor and refreshes on exact armor-slot or active
  equipment-set changes.
- Installed metadata inspection confirmed `AddStatBonus`, the public
  `ModifiableValue.AddModifier(int, Fact, string, ModifierDescriptor)` contract,
  exact equipment subscriber methods, and `ModifiableValueArmorClass.FlatFooted`.
  Native Dodge semantics therefore remove Nimble whenever Dexterity AC is lost.
- Added a save-free detached `disposable-gunslinger-nimble` scenario proving
  cumulative +5 with no armor, flat-footed exclusion, +5 in light armor, zero
  in medium armor, and exact cleanup.
- Sprint 37/inherited validation, 737/737 tests, exact-reference Release build,
  strict package validation, and 15 validation-dispatch checks pass. Candidate
  package/DLL SHA-256 are
  `3a322ff1b18dcc1e9cf80cbb2d89952b9b01a22ecc4b4a73400446bcd987a1ba` /
  `0188eb4ea037af6e81ed6c15674ee1735eed963302a70b2ccf45e38424bd6e40`.
- Next action: commit this reconstructable source checkpoint, exact mod load,
  and two independent fresh-process Nimble PASS runs.

### Nimble runtime qualification

- Source commit `b82768b` passed exact mod load at
  `20260801T2144324918751Z-mod-load-smoke`.
- Independent fresh-process PASS runs were
  `20260801T2145560578604Z-disposable-gunslinger-nimble` and
  `20260801T2147123020977Z-disposable-gunslinger-nimble`.
- Both observed base/no-armor AC 10/15, base/Nimble flat-footed AC 10/10,
  light armor with/without Nimble 23/18, medium armor with/without Nimble
  20/20, and exact detached cleanup.
- Exact package/DLL SHA-256 are
  `e8244e972dfcd257f163246d1179a467e5a17ed6ab9caaa79c52dfeb18e7807f` /
  `dafbb6bd85f01df1497815d21e3ed97c81ee95cce2797c63b7252798a026272a`.
- Nimble is runtime-qualified. Continue immediately to Gunslinger Initiative;
  Sprint 37 is not a stopping condition.

## 2026-08-01 Sprint 38 Gunslinger Initiative source qualification

- Advanced the active version to `0.0.38-s38-gunslinger-initiative` after the
  runtime-qualified Sprint 37 Nimble checkpoint.
- Exact installed metadata proves `RuleInitiativeRoll.OnTrigger` snapshots the
  native Initiative stat and the combat preparation controller subsequently
  raises `IUnitInitiativeHandler` before consuming the result. The level-three
  unit-owned feature uses that boundary to add exactly +2 only while its native
  Gunslinger grit resource is positive, without spending grit.
- The adapter changes only the exact private modifier backing field, filters by
  exact owning-unit reference, and uses weak rule identity to reject duplicate
  delivery. It does not override or force the d20 and introduces no global
  Harmony patch.
- The tabletop Quick Draw clause remains explicitly incomplete because exact
  native contracts have not yet jointly proven feat identity, free and
  unrestrained hands, visible firearm selection, and action-free weapon-set
  mutation. This checkpoint does not guess or silently claim that behavior.
- Added one stable level-three blueprint, three policy cases, and the save-free
  guarded `disposable-gunslinger-initiative` scenario. Sprint 38/inherited
  validation, 740/740 tests, 18 request checks, 40 preflight checks, 16
  validation-dispatch checks, exact-reference clean Release build, strict
  package validation, and scenario WhatIf pass.
- Candidate package/DLL SHA-256 are
  `1d47befc5495b9d70bda1cf13757813bec9737e528052e20cb8f57bdf799a329` /
  `ce8c55a16c347c873d7265675ae1e6d5c3b59bee6338074a53075ca31ece46f8`.
- Next action: commit the reconstructable source checkpoint, require exact mod
  load, and run two independent fresh-process Initiative PASS runs.

### Initiative first runtime attempt

- Exact source commit `5c6a348` passed mod load at
  `20260801T2206308361819Z-mod-load-smoke`.
- The first feature run
  `20260801T2207477896114Z-disposable-gunslinger-initiative` failed only its
  positive-grit assertion: the detached direct-grant fixture began with one
  grit rather than the asserted two. It nevertheless observed the exact
  intended mechanics: grit `1 -> 1`, native modifier `0 -> 2`, duplicate
  stability at 2, zero-grit modifier `0 -> 0`, applied/rejected/duplicate
  counts `1/1/1`, zero faults, and exact cleanup.
- This is an acceptance-fixture defect, not a production behavior defect. The
  repair asserts the authoritative contract—strictly positive initial grit and
  no spend—without prescribing a maximum for a detached unit with no class
  level. Full source qualification and a new exact commit are required before
  retrying.
- The repaired checkpoint passes Sprint 38/inherited validation, 740/740 tests,
  exact-reference clean Release build, strict package validation, 18 request
  checks, 40 preflight checks, and 16 validation-dispatch checks. Repaired
  candidate package/DLL SHA-256 are
  `c3461173a9c820e4d63a4fee5f7bb078ddf4a29590a560c3928e6f709649ebbb` /
  `cc14f5fd4afa13b5a1e108a86c065ffb983094c1f47258b2532e14f4c30c99b5`.

### Initiative +2 runtime qualification

- Repaired source commit `8ecb3aa` passed exact mod load at
  `20260801T2211000340072Z-mod-load-smoke`.
- Independent fresh-process PASS runs were
  `20260801T2212165760676Z-disposable-gunslinger-initiative` and
  `20260801T2213341079691Z-disposable-gunslinger-initiative`.
- Both observed grit `1 -> 1`, native initiative modifier `0 -> 2`, duplicate
  stability at 2, zero-grit modifier `0 -> 0`, applied/rejected/duplicate counts
  `1/1/1`, zero faults, and exact detached cleanup.
- Exact package/DLL SHA-256 are
  `33e70a386dba3c28cf5ca4fc74cdfa6b291586da24d25269b44a4abf6e499d61` /
  `1e120508f20f13ab74136c244c409f396bc29c97e9ced175398ebf8b71101a26`.
- The +2 slice is runtime-qualified. Continue immediately into the conditional
  Quick Draw contract; the overall feature and Sprint 38 are not stopping
  conditions.

### Initiative conditional-draw disposition

- The supported installed Kingmaker 2.1.7b English localization contains the
  words `Quick Draw` only in a surprise-round rules sentence and has no
  player-facing Quick Draw feat entry. Repository source likewise contains no
  Quick Draw blueprint; bonus-feat implementation remains a separate pending
  coverage row.
- Exact installed IL represents ordinary weapon-set changes through
  `UnitSwitchHandEquipmentSet`, with the turn controller charging the native
  change-weapon action. Mutating the active equipment set inside the initiative
  handler would therefore bypass normal action accounting rather than model a
  proven Quick Draw exemption.
- The conditional draw clause is explicitly
  `OMITTED-NO-MEANINGFUL-INTERACTION` for the current supported game. It may be
  revisited only if later feat work introduces Quick Draw and exact
  free/unrestrained-hand plus visible-firearm contracts. The runtime-qualified
  +2 remains the narrow faithful Gunslinger Initiative adaptation.
- Next action: audit Pistol-Whip's native melee attack, damage, trip, firearm
  handedness, grit, and action-economy contracts. Sprint 38 is not a stopping
  condition.

## 2026-08-01 Sprint 39 Pistol-Whip qualification

- Implemented the level-three standard-action deed with exactly one equipped
  marked firearm, one-grit atomic delivery, Wrecked rejection, and transient
  unowned native melee surrogates. One-handed firearms use 1d6 and two-handed
  firearms use 1d10; both are bludgeoning 20/x2 weapons whose native attack and
  damage enhancement fields copy the equipped firearm's enhancement.
- A successful native `RuleAttackWithWeapon` hit triggers a free native Trip
  combat maneuver. The deed does not discharge ammunition, change firearm
  condition, switch equipment, or persist its surrogate. Exceptional delivery
  restores the spent grit.
- Source commit `abfd426` passed Sprint 39 and inherited validation, 744/744
  domain tests, 18 runtime-request checks, 40 preflight checks, 20 validation
  dispatch checks, exact-reference Release compilation, and strict package
  validation. Exact package/DLL SHA-256 are
  `d89f093a3c4e0f78d8dfe660b49af1b2919c1ce482d774730835b4bb24c538ef` /
  `7a7da98b0770228b531df2f090389c463f5ca6bf2d5cd77b33f68ce0a4f1d564`.
- Exact mod load passed at `20260801T2244554687322Z-mod-load-smoke`.
  Independent fresh-launch feature PASS runs were
  `20260801T2246129650055Z-disposable-gunslinger-pistol-whip` and
  `20260801T2247321424917Z-disposable-gunslinger-pistol-whip`. Both observed
  grit `1 -> 0`, one-handed d6 and two-handed d10 hits with Trip, enhancement
  propagation, unchanged loaded/Broken firearm snapshots, zero-grit
  `InsufficientGrit` rejection, applied/rejected/hit/fault counts `2/1/2/0`,
  and exact external cleanup.
- Sprint 39 is a checkpoint, not a stopping condition. Continue immediately to
  Utility Shot's three branch dispositions.

## 2026-08-01 Sprint 40 Utility Shot source qualification

- Exact installed metadata confirms Kingmaker locks are map interaction,
  Pick Lock, and Disable Device objects rather than firearm-attack targets; it
  also exposes no general movable unattended-object combat target. Blast Lock
  and Scoot Unattended Object are therefore explicitly
  `OMITTED-NO-MEANINGFUL-INTERACTION`, not silently skipped.
- Stop Bleeding is adapted as the tabletop shoot-into-air branch: a standard
  action requiring positive grit without spend, exactly one equipped loaded
  non-Wrecked firearm, and self or an adjacent target with native Bleed. It
  performs no attack roll or damage, consumes exactly one chamber, removes one
  `SpellDescriptor.Bleed` fact, preserves other facts, and restores the prior
  firearm state if delivery faults after discharge.
- Added two stable blueprints, four focused policy cases, and a save-free
  guarded self/adjacent/zero-grit scenario. Sprint 40 and inherited validation,
  748/748 domain tests, 18 request checks, 40 preflight checks, 22 validation
  dispatch checks, exact-reference Release compilation, and strict package
  validation pass. Candidate package/DLL SHA-256 are
  `1387df20e7f43a34b93f0661a6dd193d1b264ea8bdfea8f84f02a1587b21d709` /
  `ebf4c938045e281a1c39910d53ceb8d53487a28689199e01df14d54522ad625e`.
- Next action: create the exact source checkpoint, require mod load, and run two
  independent Stop Bleeding PASS attempts. Sprint 40 is not a stopping
  condition.

### Utility Shot runtime qualification

- Exact source commit `8270ade` passed mod load at
  `20260801T2306012700758Z-mod-load-smoke`.
- Independent fresh-process PASS runs were
  `20260801T2307201904813Z-disposable-gunslinger-stop-bleeding` and
  `20260801T2308408422774Z-disposable-gunslinger-stop-bleeding`.
- Both observed grit `1 -> 1`, self Bleed facts `2 -> 1`, adjacent Bleed facts
  `1 -> 0`, one chamber consumed per successful delivery, zero-grit
  `InsufficientGrit` with its loaded chamber preserved, applied/rejected/fault
  counts `2/1/0`, and exact detached cleanup.
- Exact package/DLL SHA-256 are
  `1387df20e7f43a34b93f0661a6dd193d1b264ea8bdfea8f84f02a1587b21d709` /
  `ebf4c938045e281a1c39910d53ceb8d53487a28689199e01df14d54522ad625e`.
- Utility Shot is disposition-complete for the supported game. Continue
  immediately to level-four Bonus Feats; Sprint 40 is not a stopping condition.

## 2026-08-01 Sprint 41 Bonus Feats source qualification

- The authoritative base feature grants one combat or grit feat at levels
  4/8/12/16/20. Exact installed metadata exposes the native
  `BlueprintFeatureSelection` contract with candidate arrays, feature groups,
  prerequisite filtering, obligatory level-up integration, and UI behavior.
- Reused the exact installed Fighter combat-feat selection GUID
  `41c8486641f7d6d4283ca9dae4147a9f` once at each required Gunslinger level.
  The implementation does not clone the selection, copy candidates, bypass
  prerequisites, or allocate a project-owned stable ID.
- Kingmaker has no native grit-feat category and the repository's deeds are
  class features, not feats; the slice does not mislabel them. Independently
  authorized grit feats can be appended later without changing progression.
- Added three focused cadence/immutability cases and a save-free guarded native
  selection identity scenario. Sprint 41/inherited validation, 751/751 tests,
  18 request checks, 40 preflight checks, 24 dispatch checks, exact-reference
  Release compilation, and strict package validation pass. Candidate
  package/DLL SHA-256 are
  `c58407763dbadb1a59dae7bb47b6e76b1af77720607c23ec70f9221805a1c283` /
  `f5422803cba7ea8af1ad6333a192e5038e79fa1977d4a70503d7a7c16c3f9f19`.
- Next action: source checkpoint, exact mod load, and two independent Bonus
  Feats PASS runs. Sprint 41 is not a stopping condition.

### Bonus Feats runtime qualification

- Source checkpoint `8a8ea7a` passed exact mod load at
  `20260801T2319037969615Z-mod-load-smoke` with version `0.0.41`, runtime
  identity, and core initialization confirmed.
- The first feature run failed closed only because the probe required the
  native selection's direct `Features` array to be populated. Its structured
  evidence already proved the production cadence and showed the installed
  Fighter selection exposes candidates through `AllFeatures`: exact GUID
  `41c8486641f7d6d4283ca9dae4147a9f`, levels `4,8,12,16,20`, five
  occurrences, `Features=0`, `AllFeatures=108`, and
  `IgnorePrerequisites=False`.
- Narrow probe correction `4604717` removed only the invalid direct-array
  requirement. It passed Sprint 41/inherited validation, all 751 tests,
  exact-reference Release compilation, and strict package validation.
- Independent fresh-process PASS runs were
  `20260801T2322304096701Z-disposable-gunslinger-bonus-feats` and
  `20260801T2324134236411Z-disposable-gunslinger-bonus-feats`. Both reproduced
  the exact GUID, required five levels/occurrences, 108 aggregate candidates,
  prerequisite enforcement, version `0.0.41`, and automatic exit without save
  access or unit mutation.
- Final package/DLL SHA-256 are
  `ceb4c03d1c229e95eee278f366492f726683da007094be1829c3bed24c106e95` /
  `3b948b7cdaec3820678e94d4498700af3f30f0bee6a9f991b27409fc9746e71f`.
- Sprint 41 is complete. Continue immediately to Sprint 42 Gun Training; this
  checkpoint is not a stopping condition.

## 2026-08-01 Sprint 42 Gun Training source qualification

- The authoritative feature grants cumulative specific-firearm selections at
  levels 5/9/13/17, Dexterity to damage for selected types, and a +2 rather
  than +4 Broken-state misfire increase. Exact installed IL confirmed
  `RuleCalculateWeaponStats.AddBonusDamage(int)` feeds the primary native weapon
  damage descriptor without replacing the ordinary damage stat.
- Added one stable obligatory `BlueprintFeatureSelection` and five rank-one
  Pistol/Musket/Blunderbuss/Rifle/Revolver choices keyed only to immutable
  `FirearmDefinition.Kind`. Native selection rank checks prevent duplicate
  choices; no borrowed weapon category or custom UI is used.
- Added the missing ordinary Broken misfire rule: effective threshold is base
  +4, reduced to base +2 for a matching trained kind, clamped to 20. Wrecked
  firearms remain rejected. Matching facts add the exact positive, zero, or
  negative Dexterity modifier once through native weapon-stat calculation;
  unmarked, ambiguous, and different-kind weapons fail closed.
- Added five focused cases; all 756 domain tests pass. Sprint 42 and inherited
  source validation, 18 request checks, 40 preflight checks, 26 dispatch
  checks, exact-reference Release compilation, and strict package validation
  pass. The save-free guarded scenario inspects production cadence and choices,
  compares untrained/trained native pistol damage, and forces natural 5 against
  Broken state to distinguish untrained Wrecked from trained Broken.
- Candidate package/DLL SHA-256 are
  `60754bbf1ea7e1e91f6847c7aed9283b12ffb535d2cecb41fc6eaff8e706a6c0` /
  `adbeee6dc2dbc496f0b9714a58a1925c041ddbc70b5085ef8aa52e7e3e202fdb`.
  Runtime qualification remains required and Sprint 42 is not a stopping
  condition.

### Gun Training runtime qualification

- Source checkpoint `72ab329` passed initial mod load. Its first feature run
  proved production progression and native Dexterity damage but failed closed
  because the fixture's `AutoHit` flag suppresses Kingmaker's natural-roll
  setter; neither Broken firearm entered misfire evaluation.
- Narrow correction `76ae9f9` removed only that fixture bypass. It re-passed
  Sprint 42/inherited validation, all 756 tests, exact-reference Release
  compilation, and strict package validation.
- Exact correction-assembly mod load passed at
  `20260801T2353104199349Z-mod-load-smoke`. Independent post-gate fresh-process
  PASS runs were
  `20260801T2354325601237Z-disposable-gunslinger-gun-training` and
  `20260801T2355488749660Z-disposable-gunslinger-gun-training`.
- Both PASS runs observed levels `5,9,13,17`, four occurrences, five distinct
  rank-one choices, untrained/trained pistol native bonus damage `0/4`, forced
  natural 5 changing untrained Broken to Wrecked while the trained pistol
  remained Broken, version `0.0.42`, and exact detached cleanup.
- Final package/DLL SHA-256 are
  `703b611c52cc8a4d55b84052e2a54386af203afb22f46f3617f9ec9d4a28bcaa` /
  `593477e79f303c8207574ae511961d7bd003db01ca433e92d1d0b43c9cc5c5db`.
- Sprint 42 is complete. Continue immediately to the next incomplete base-class
  or deed row; this checkpoint is not a stopping condition.

## 2026-08-01 Sprint 43 Dead Shot source qualification

- The authoritative level-seven deed spends 1 grit and one chamber for a
  full-round shot, rolls the BAB iteratives against one target, adds only one
  base-dice packet per hit after the first, confirms at most one critical at
  highest BAB -5 adjusted by extra threats, and misfires only when every roll
  misfires.
- Added two stable production blueprints and the level-seven progression grant.
  Short-lived weak contexts distinguish native probe rolls from the one
  item-owned discharge, preventing multiple chamber consumption, multiple
  condition transitions, or probe critical grit recovery. The final native
  weapon delivery overrides only `WeaponDamageDiceOverride`; ordinary damage
  modifiers, target handling, and critical confirmation remain native.
- Added six deterministic policy/outcome cases. All 762 tests pass. Sprint 43
  plus inherited validation, 18 request checks, 40 preflight checks, 28
  validation-dispatch checks, clean exact-reference Release compilation, and
  strict package validation pass. Source-only scenario WhatIf also passes.
- The guarded save-free scenario observes the level-seven full-round blueprint,
  forces BAB 11 rolls `19,1,19` to prove two hits/two base-dice packets with no
  aggregate misfire, then forces `1,1,1` to prove exactly one chamber/grit cost
  and one Normal-to-Broken transition. Detached state is cleaned without saving.
- Candidate package/DLL SHA-256 are
  `85c574e881c58f7e2f15a49a855cfc40d85efb3ce51174021d1558b5ba61b27d` /
  `c477bb009ecd47cc880e4525ad7a6f1425cebbef1074f75420810854091f7870`.
- Exact mod load passed for `2cc0f33`. Its first feature run failed closed only
  because the mixed fixture used natural 2 as its intended misfire even though
  the production Early Pistol's Normal threshold is exactly 1. The structured
  result proved progression, all-1 aggregate misfire, costs, and cleanup; the
  narrow correction changes only the mixed forced sequence to `19,1,19`.
- Next action: commit the source checkpoint, require exact mod load, and run two
  independent Dead Shot PASS processes. Sprint 43 is not a stopping condition.

## 2026-08-01 Sprint 43 Dead Shot runtime qualification

- Corrected checkpoint `fdd5d7c2f3423fd85112b80e046bec042f7be8b0` rebuilt
  cleanly with all 762 tests and strict package validation passing. Final
  package/DLL SHA-256 are
  `191be3b74ca79c1d0128f0c02c377a9118ea02cfc4dd802030bbd3a0bcd16b4b` /
  `d235c2bcfb74bb25b320f7904a8e72bb9120f01a398d254c0a5dce369bb6a4f4`.
- Exact-version mod load PASS run
  `20260802T0032018715336Z-6864954149b440dc9f10abfac6449d6e`
  loaded version `0.0.43` from the corrected commit with ready Harmony context.
- Independent fresh-process feature PASS runs
  `20260802T0033299551403Z-5d701f8560b7443b9a4433072e419c69` and
  `20260802T0034468834084Z-e1508a1ec47246ab91a5ba4f863bef43`
  identically observed three BAB-11 probes at `+11,+6,+1`, forced natural
  rolls `19,1,19`, two hits and two base-dice packets, Normal condition after
  the mixed volley, all-roll misfire changing Normal to Broken, grit
  `4->3->3`, and exact detached-state cleanup.
- Sprint 43 is runtime-qualified. Continue immediately to level-seven
  Startling Shot; the checkpoint is not a stopping condition.

## 2026-08-01 Sprint 44 Startling Shot source qualification

- Authority requires at least one grit without expenditure, a standard-action
  intentional firearm miss, no damage, and flat-footed state until the start
  of the target's next turn. The recorded Kingmaker adaptation uses native
  weapon-range enemy targeting and a one-round `LoseDexterityToAC` buff; the
  deliberate miss creates no attack or damage rule event.
- Added stable level-seven feature, ability, and buff blueprints. Delivery
  consumes exactly one item-owned chamber and zero grit. Failure after
  discharge removes the new buff and restores the exact prior firearm state.
- Added three focused policy cases; all 765 domain/reflection tests pass.
  Sprint 44 plus inherited repository validation, exact-reference Release
  compilation, build-output checks, and strict standalone package validation
  pass. Runtime request tests pass 19 checks and preflight passes 42 checks.
- The guarded save-free scenario checks progression/action/targeting, one
  chamber, unchanged grit and HP, native `RuleCheckTargetFlatFooted`, one-round
  duration, diagnostics, and exact detached cleanup without save APIs.
- Current source candidate package/DLL SHA-256 are
  `6492b8f7bbc7dd27189ac663c4d7065f0f88839ae8950575c603af7b907e7d8c` /
  `c2e8ad3afd24a1dc2c62405a04a6384c0ae967931c5a840a7b871a36859315a9`.
- Next action: finish dispatcher/parser/WhatIf gates, commit the source
  checkpoint, require exact mod load, and run two independent Startling Shot
  PASS processes. Sprint 44 is not a stopping condition.
- Exact mod load passed on source commit `acf909a`. The first feature run
  `20260802T0100005433275Z-1a5b38ac671a4750883ac0e6af4b87d5` failed closed
  at the intentional-miss delivery because the detached fixture passed a null
  mechanics context to `BuffCollection.AddBuff`; production ability delivery
  always supplies one. The narrow correction constructs the exact native
  disposable caster/target context and leaves production behavior unchanged.
- Corrected commit `9cade83` passed exact mod load, but feature run
  `20260802T0104131211548Z-2030d6c86378401bb4cabcde78d535d8` proved a generic
  `MechanicsContext` is still not a sufficient native buff-delivery fixture:
  `AddBuff` returned null. Exact installed constructor inspection established
  the production-shaped `AbilityData`, `AbilityParams`, and
  `AbilityExecutionContext` path now used by the detached scenario.
- Production-shaped context commit `e487a99` passed guarded Steam mod load in
  run `20260802T0109149299896Z-f71377fa0cc54c3babc042a9eee22c95`, package
  SHA-256 `071eddd0ceece27bc07bda729311ca6398d23e58815ee7fc25af8093217f8779`
  and DLL SHA-256
  `64e29ac1e2eab40d2c2374c419a10fba006b36f46a307c7d8e8cb9ce37980049`.
- Guarded feature run
  `20260802T0111206331676Z-d7699f4644d14bee8bae8268671a4350` still returned
  null from native `AddBuff` with that production-shaped context. Per the
  two-repair rule, fixture variation stopped. Repository-wide blueprint
  comparison then isolated a production contract mismatch: the temporary
  hostile buff was marked `IsClassFeature`, while every other true use is a
  permanent feature. The narrow correction classifies the timed debuff as a
  non-class-feature buff and validates that invariant.
- Exact commit `1a818d0` passed mod load, but feature run
  `20260802T0116128177162Z-97dc5c3ac58f43618bbfc3d01feafaf7` falsified buff
  classification as the null-return cause. Retained exact-runtime IL then
  identified the decisive `RuleApplyBuff` branch: a `RemoveOnResurrect` buff
  is rejected while the target has `UnitState.Immortality`. The scenario's
  target was cloned from `DefaultPlayerCharacter` and therefore immortal,
  unlike the enemy contract it represents. The disposable fixture now clears
  that flag before delivery; production buff behavior is not weakened.
- Exact lifecycle-fixture commit `e34e40c` passed guarded mod load at evidence
  directory `20260802T0119594190278Z-mod-load-smoke`, but guarded feature run
  `20260802T0121202660495Z-19a9d9d2d2304733a70145d5dd1b79b5` still returned
  null from `RuleApplyBuff`. The immortality hypothesis was necessary but not
  sufficient, so additional feature launches stop pending a narrower handler
  observation. Startling Shot remains source-qualified; Targeting is the next
  independent completion item.

## 2026-08-01 Sprint 45 Targeting Head implementation

- Selected the independently exact Head row rather than inventing the unresolved
  Arms replacement debuff. Entry criteria are recorded in
  `planning/SPRINT-45-ENTRY-CRITERIA.md`.
- Version `0.0.45` adds a level-seven full-round weapon-range ability using one
  ordinary native `RuleAttackWithWeapon`. Eligibility requires one exact loaded
  non-Wrecked firearm, a valid target, and 1 grit; delivery spends exactly 1
  grit and leaves native chamber, misfire, damage, critical, concealment, and
  immunity behavior authoritative.
- A hit applies a one-round native Confusion buff only when the attack's native
  `ImmuneToSneakAttack` result is false. The buff carries the MindAffecting
  descriptor so native effect immunities remain authoritative.
- Five focused policy/rider cases bring the complete suite to 770/770 PASS.
  Initial clean Release/package validation passed with candidate package
  SHA-256 `4497b9a660fc77302d23d4e9d41870116b9eb278d21c353e2c60cea9f82a3bd2`
  and DLL SHA-256
  `56e021f47057a71ccf6f8c4f9470f43b839cc69f3dad60f499a3c8e8c0b4b160`.
  Updated harness gates pass: runtime request 20, scenario preflight 44,
  runtime result 2, runner 19, deployment 16, Steam launch 47, validation
  dispatcher 29, PowerShell parser 74, and guarded WhatIf. The guarded
  save-free scenario is implemented. Latest qualified dirty-tree candidate
  package SHA-256 is
  `805bc0b0af5c8bb4c240ba219d5ab90f618c84447e3ffc2e5b0150df349f7ea3`
  and DLL SHA-256 is
  `53c0fd5cd0e5b6fa6353da7dab24ec44471e9ee8d005dcf9f6edb16cb30e3584`.

## 2026-08-01 Sprint 45 Targeting Head runtime boundary

- Source checkpoint `e543356` and verifier checkpoint `6b29536` remain clean,
  reconstructable commits. The exact latter build passed 770/770 tests and
  strict package validation with package SHA-256
  `eaa5a2001d8ac6fde6c46eb62551e493f687312c1ded95f39e2ccadff02fc112`
  and DLL SHA-256
  `8e9789a2ce7cd26f2923699019e1ee00b5b975a2d04305ff877d183caddbc06b`.
- Exact mod load passed in evidence directory
  `20260802T0144092249996Z-mod-load-smoke`.
- First feature run `20260802T0145575387773Z-98fdf5c587d942ffb26d9f7d736a8e20`
  proved grit `3->2`, chamber `1->0`, hit, non-immunity, and native Confusion,
  but falsified detached HP delta and `TimeLeft` as valid fixture assertions.
- The narrowed verifier used the native damage rule and `Buff.IsPermanent`.
  Second fresh run
  `20260802T0150467055365Z-ad41b1a17c7b4ecaa4003d7eafc0403d`
  again proved the attack roll and Confusion state, but exposed
  `MeleeDamage=null` and `IsPermanent=true`. Directly triggering
  `RuleAttackWithWeapon` therefore resolves the roll without the subsequent
  native damage dispatch, while this detached context does not retain the
  supplied timed duration.
- Per the two-material-attempt rule, do not make a third speculative Targeting
  Head repair. Change mode to exact damage-dispatch and timed-buff contract
  inspection. Continue independently with Targeting Torso, whose deed-local
  threat-range behavior does not depend on a timed rider.

## 2026-08-01 Sprint 46 Targeting Torso source qualification

- Local authority defines Torso exactly: the level-seven full-round, one-grit
  Targeting firearm attack threatens a critical on natural 19–20, while
  creatures immune to sneak attacks are immune to the effect.
- Version `0.0.46` adds stable feature/ability IDs, level-seven progression,
  and a reference-scoped weak marker. The marked attack adjusts only its
  calculated `RuleCalculateWeaponStats.CriticalEdgeBonus`; no weapon blueprint
  or persisted item state is mutated. Native confirmation and multiplier remain
  authoritative.
- Successful delivery explicitly follows the installed native contract by
  triggering `RuleAttackWithWeapon.CreateRuleDealDamage(false)` after the hit.
  This avoids the direct-roll/no-damage gap exposed by Sprint 45 without
  broadening any global patch.
- Three focused policy cases bring the complete suite to 773/773 PASS.
  Repository validation, exact private-reference Release compilation, strict
  package validation, request 21, preflight 46, result 2, runner 19,
  deployment 16, Steam launch 47, validator dispatch 29, all 74 PowerShell
  parsers, and guarded WhatIf pass.
- Latest dirty-tree package/DLL SHA-256 are
  `10836a02ce37457b2d539799fd059e0a291526f6a880070fcae377e7c5ffcf6d` /
  `3ffa48ca1860659e60cc3e567e306e84194072630c46842a812c60c2ba18423c`.
  Commit the reconstructable source checkpoint, rebuild the exact clean commit,
  require mod-load PASS, then run two independent Torso qualifications.

## 2026-08-01 Sprint 46 Targeting Torso runtime qualification

- Exact clean commit `cc629a5` rebuilt with package SHA-256
  `dd867f0d8dd8ac89b02596e5a627e48c1a81272d7906dccfab4f32b7ff3dbfe7`
  and DLL SHA-256
  `546ab30b3186136a09d2aded32e9e815336a15299f3f96292aa791f0e86014c7`.
- Guarded Steam mod load passed in run
  `20260802T0209076152067Z-14117474bdfb443a988299a617157502`.
- Independent feature runs
  `20260802T0210250762420Z-13a22fcb19dc4c328a56adbca72e666f`
  and `20260802T0211456711945Z-392586b512d546f2a37db9da41a8568c`
  both passed identically: grit `3->1`, both chambers `1->0`, natural 18 hit
  without threatening and dealt 8 damage, natural 19 hit/threatened and dealt
  6 damage, and detached party/global-unit cleanup passed. No save was named or
  loaded and the save-free scenario exposed no save-writing path.
- Targeting Torso is runtime-qualified. Continue immediately to Targeting Legs,
  whose normal-damage/prone mapping is independently authoritative.

## 2026-08-01 Sprint 47 Targeting Legs implementation in progress

- Entry criteria record the authoritative automatic knockdown plus a narrow
  Kingmaker adaptation: one native Trip event receives guaranteed attack
  strength after an eligible firearm hit, while native maneuver immunity
  remains authoritative and anatomy is never guessed from names.
- Added rider policy, stable feature/ability blueprints, level-seven
  progression, ordinary firearm attack and explicit native damage dispatch,
  sneak/trip-immunity gates, and the native Trip rider.
- Three focused cases bring the directly compiled suite to 776/776 PASS.
  Version `0.0.47` now passes inherited validators through Sprint 47, all
  776 domain/reflection tests, clean Release compilation, strict build-output
  and standalone-package validation, request 22, preflight 48, result 2,
  runner 19, deployment 16, Steam launch 47, validator dispatch 29, and the
  guarded dirty-tree WhatIf.
- The reconstructable dirty-tree package/DLL SHA-256 values are
  `132ee6ae9688784cea89dae2b03794b7d11774e8152645d17a8f6cd8a694a91d` /
  `f9d77ae0350ccae84722ee84b6749d1aaf5b83e0eac105a3271fcee6c4995792`.
  Commit the source-qualified checkpoint, rebuild the exact clean commit,
  require mod-load PASS, then run two independent Legs qualifications.

## 2026-08-01 Sprint 47 first runtime observation

- Exact source checkpoint `3a7738f` rebuilt with package/DLL SHA-256
  `c2724fcb316bc785fdd159933bfb910a7a56363481f6d7a43db9663bd0e5d2d3` /
  `2a4afd1344f15427dd16c0c61dd71b81c50f5eea306236d2aba94ebbb157cacf`;
  mod-load run `20260802T0227021781653Z-mod-load-smoke` passed.
- Feature run `20260802T0228235373097Z-dcee01225e0e438f80c0aab6f6b70060`
  failed one exact assertion. Both forced hits dealt positive native damage,
  grit changed `3->1`, both chambers changed `1->0`, native maneuver immunity
  suppressed the second Trip, and cleanup passed. The eligible native Trip
  reported success but the detached target remained non-prone.
- One narrow repair preserves the authoritative native Trip and immunity gate,
  then ensures its successful result owns the same native `UnitCondition.Prone`
  state when detached event aftermath did not install it. Requalify all source
  gates and checkpoint this evidence-driven repair before another runtime run.

## 2026-08-01 Sprint 47 Targeting Legs runtime qualification

- Exact repair commit `1a2f29c` rebuilt with package/DLL SHA-256
  `598313f1b3e38201646ee039e87d8614ef8a87427596d39d72e55cec1e81df89` /
  `cda2c4e181c205b1676a6b81eb6575a345c566c9ed4d018c33cb274c4b282c25`.
  Mod-load run `20260802T0231549132685Z-mod-load-smoke` passed.
- Independent feature runs
  `20260802T0233121188720Z-7ef51e849a88459591847b21e4d7466e` and
  `20260802T0234333804832Z-c0c8adb9ca9648f8bbbea1c7117546c9`
  both passed: grit `3->1`, both chambers `1->0`, eligible damage was positive,
  native Trip succeeded and prone changed `False->True`, the immune target took
  positive damage without Trip or prone, and cleanup/external isolation passed.
- Targeting Legs is runtime-qualified. Continue immediately with the remaining
  Targeting dispositions, beginning with exact Wings interaction support.

## 2026-08-01 Sprint 48 Targeting Wings disposition

- Read-only exact-word IL metadata inspection of installed Kingmaker 2.1.7b
  found only the visual `UnitAnimationSpecialAttackType.Wing` value and the
  `ActivatableAbilityGroup.Wings` grouping after controller-library names were
  excluded.
- No general flying, airborne, grounded, altitude, falling, or flight-loss rule
  contract exists. Targeting Wings is therefore confirmed
  `OMITTED-NO-MEANINGFUL-INTERACTION`; anatomy/name guesses and prone substitution
  remain forbidden.
- Continue immediately to Targeting Arms and resolve its missing item-drop
  interaction without inventing an autonomous balance replacement.

## 2026-08-01 Sprint 49 Targeting Arms contract audit

- Private rules authority requires no hit damage and a gunslinger-chosen carried
  item drop. Installed Kingmaker does contain `CombatManeuver.Disarm`, but exact
  `RuleCombatManeuver.OnTrigger` IL applies margin-scaled main/off-hand disarm
  buffs rather than dropping or unequipping a chosen item.
- Automatic-strength Disarm would arbitrarily enlarge duration. No local source
  authorizes a fixed duration, item-selection policy, or replacement debuff, so
  Arms is temporarily `BLOCKED` on that narrow player-facing balance choice.
- Continue independently to level-11 Bleeding Wound. Do not let the Arms row
  stop progress on separately authoritative deeds.

## 2026-08-01 Sprint 50 Bleeding Wound source qualification

- Exact installed-contract inspection confirmed per-round fact callbacks through
  `ITickEachRound`, native ability damage through `RuleDealStatDamage`, and no
  built-in Bleed delivery component beyond `SpellDescriptor.Bleed`.
- Entry criteria cover all four pre-shot choices, marker consumption, one/two
  grit costs, living/sneak-immunity gates, ordinary hit damage, persistent native
  Bleed facts, native HP/stat damage, removal, isolation, and qualification.
- Added the pure four-choice policy, four mutually exclusive free-action marker
  abilities, exact next-firearm-attack correlation, post-hit grit spending,
  persistent Bleed facts, native recurring HP/stat damage, level-11 progression,
  and a save-free guarded runtime scenario.
- Source qualification passes the full Sprint 29-50 invariant chain, 29
  validation-dispatch checks, 23 request checks, 50 preflight checks, all
  runtime safety suites, and 780/780 domain/reflection tests. The clean Release
  build and strict standalone package validation pass.
- Candidate package SHA-256:
  `9a2aa52a121b4a02dc2f77ce735f7778aaa147dbb96b57a8296e0b0ac4e5575e`.
  Candidate DLL SHA-256:
  `3e420639a04cd967d42732f0d18747a426648722a97fb5015887473e73c42967`.
- Next action: audit and commit the reconstructable source checkpoint, rebuild
  the exact commit, require guarded mod load, then run two independent
  Bleeding Wound PASS processes. Sprint 50 is not a stopping condition.

## 2026-08-01 Sprint 50 Bleeding Wound runtime qualification

- Source checkpoint `628fd58` was followed by two bounded native-contract
  repairs: `e86da67` moved delivery before exact firearm correlation teardown,
  and `5a939d7` verified the attached exact fact rather than trusting
  `BuffCollection.AddBuff`'s null return in the detached fixture.
- Exact final package SHA-256:
  `412fa07d8831d5d07a5ac009278ad845460f7a5327ee421df2a26e5ea6cb620e`.
  Exact DLL SHA-256:
  `05859c0ef58adc52e67bbb05eea3faebed389c016c8ceb4f6c203c3f83f72622`.
- Guarded exact-commit mod load passed in
  `20260802T0321317830272Z-mod-load-smoke`. Independent feature PASS runs
  `20260802T0322528218163Z-disposable-gunslinger-bleeding-wound` and
  `20260802T0324126984990Z-disposable-gunslinger-bleeding-wound` proved level-11
  progression, four free choices, ordinary damage, Dexterity-modifier 4 HP
  bleed, 1 Strength bleed, exact 1+2 grit spend, miss/chamber behavior, removal,
  cleanup, and no save-backed interaction.
- Continue immediately to Expert Loading. Sprint 50 is a checkpoint, not a
  stopping condition.

## 2026-08-01 Sprint 51 Expert Loading entry audit

- The authoritative level-11 rule spends 1 grit after a Broken-gun misfire to
  prevent its explosion while retaining Broken condition.
- The existing exact pipeline commits Broken-to-Wrecked and schedules its burst
  inside the first `IsSuccessRoll` evaluation, before `FinishAttack` delivers
  the burst. Sprint 51 will insert a narrow deed decision before that mutation;
  it will not broaden Harmony targets or change ordinary misfire behavior.
- Because Kingmaker cannot pause after the roll, the documented adaptation is a
  personal free-action pre-shot marker consumed by the next exact firearm
  attack, spending only when it suppresses the exact Broken-to-Wrecked event.
- Entry criteria are recorded in `planning/SPRINT-51-ENTRY-CRITERIA.md`. Next:
  implement the pure suppression decision and focused tests before touching the
  native adapter.

## 2026-08-01 Sprint 51 Expert Loading source qualification

- Added the pure suppression policy, four focused tests, one level-11 feature,
  one personal free-action ability, one unit-owned armed marker, and the exact
  `ExpertLoadingBrokenRemainsBroken` condition result.
- The existing misfire adapter invokes Expert Loading only during the first
  exact eligible natural-roll evaluation, before item mutation and explosion
  scheduling. Marker consumption is unit-local; grit is verified exactly; any
  spend/verification fault restores grit when possible and returns the ordinary
  Broken-to-Wrecked result.
- Added a guarded save-free scenario proving progression, one-grit suppression,
  empty/Broken retention, no burst scheduling, no-grit Wrecked/burst behavior,
  chamber consumption, cleanup, and external isolation.
- Source gates pass: full Sprint 29-51 invariant chain, 29 dispatch checks, 24
  request checks, 52 preflight checks, all runtime safety suites, 784/784 domain
  tests, clean Release compilation, strict package validation, and scenario
  WhatIf with no deployment or launch.
- Candidate package SHA-256:
  `4e2f6f566bbfb356bf1058e810487046e72546ce3629afd05e553a4eef8e1516`.
  Candidate DLL SHA-256:
  `8e02b7dafc71d892ea07d3453329543f88ca009f24a89f63bb40dd760fe6e77f`.
- Removed the exact generated `artifacts/tmp/KmgValidatorTests-4ct3zhh3`
  fixture left by an earlier sandbox-denied validator run; it was not tracked
  and is not recoverable.
- Next: commit the reconstructable source checkpoint, rebuild the exact commit,
  require mod-load PASS, and run two independent Expert Loading PASS processes.

## 2026-08-01 Sprint 51 Expert Loading runtime qualification

- The reconstructable source checkpoint is commit `79731d1`. Its exact package
  SHA-256 is `81a00010bf9a38bf4f7be4d22409159dd828172113e71f48e33296dd172d09bd`;
  its DLL SHA-256 is
  `3e34b55954328cbbfdc33ed6b8745e83addea2735887ea948287db7a62e15f88`.
- Guarded exact-commit mod load passed in
  `20260802T0343348846193Z-mod-load-smoke`. Independent fresh-process feature
  runs `20260802T0344552821282Z-disposable-gunslinger-expert-loading` and
  `20260802T0347194832064Z-disposable-gunslinger-expert-loading` both passed.
- Each feature run proved the level-11 feature and ability, exact one-grit
  suppression, empty/Broken retention, consumed marker, zero scheduled burst,
  and a no-grit control that became Wrecked and scheduled exactly one burst.
  Cleanup and external-isolation assertions passed; no save was loaded or
  mutated.
- Continue immediately to Lightning Reload. Sprint 51 is a checkpoint, not a
  stopping condition.

## 2026-08-01 Sprint 52 Lightning Reload entry audit

- The authoritative level-11 deed requires positive grit without spending it,
  reloads one barrel as a non-provoking swift action once per round, and makes
  that action free only with Rapid Reload or an alchemical cartridge.
- The existing generic transaction already loads one chamber atomically and
  preserves Broken condition. The only player-facing reload blueprint remains
  the diagnostic full-round Test Musket ability, so Sprint 52 requires a new
  production equipped-firearm deed rather than altering that diagnostic.
- No qualified Rapid Reload feature or alchemical-cartridge ammunition exists
  in the current catalog. The reachable swift route will be implemented now;
  the free route stays fail-closed until one of its prerequisites is separately
  implemented and qualified. Entry criteria are recorded in
  `planning/SPRINT-52-ENTRY-CRITERIA.md`.
- Next: add the pure availability/use policy and focused tests before binding
  native per-unit round reset or the production ability.

## 2026-08-01 Sprint 52 Lightning Reload source qualification

- Added a pure availability decision with six focused cases covering positive
  grit/no-spend, same-round gating, unit isolation, Broken eligibility,
  firearm/state/ammunition rejection, and invalid input.
- Added one level-11 feature, a personal extraordinary swift-action production
  ability, and a unit-local use marker whose bound `ITickEachRound` component
  removes it at the next native round callback.
- Delivery uses the existing exact equipped-firearm resolver and atomic shared-
  inventory reload transaction. It loads one chamber, preserves condition, and
  removes the use marker on any exception or late transaction rejection.
- Added guarded save-free scenario
  `disposable-gunslinger-lightning-reload`, proving progression, no grit spend,
  one-chamber loading, same-round rejection, bound round reset, Broken
  preservation, zero-grit rejection, exact inventory restoration, and external
  isolation.
- Repository validation, runtime request/preflight/runner/result/deployment/
  Steam safety suites, 790 domain tests, exact-reference Release build, strict
  package validation, and dirty-source WhatIf pass. The generated validator
  fixture `artifacts/tmp/KmgValidatorTests-otrmpjdp` left by a sandbox-denied
  run was removed by exact verified path; the elevated rerun passed 29 checks.
- Next: finish the reconstructable source audit and commit, rebuild the exact
  commit, require mod-load PASS, and run two fresh feature PASS processes.

## 2026-08-01 Sprint 52 Lightning Reload runtime qualification

- The reconstructable source checkpoint is `df60f59`. Its exact package
  SHA-256 is `dd90bd94ef0d6efc72a1a1f0ffecdec1f55f824e3d6f765a97ba2abe8f43d471`;
  its DLL SHA-256 is
  `902f32bc4b4c0ee0dd522f553e3cb799a6855c7a9c2af7f26d5137bebfd612fc`.
- Guarded exact-commit mod load passed in
  `20260802T0409040682597Z-mod-load-smoke`. Independent feature runs
  `20260802T0410232157000Z-disposable-gunslinger-lightning-reload` and
  `20260802T0411425931495Z-disposable-gunslinger-lightning-reload` passed.
- Both runs observed grit `3->3`, one Normal and one Broken chamber loaded,
  Broken retained, same-round rejection, bound next-round reset, zero-grit
  rejection, exact shared-inventory restoration, and disposable isolation.
- Continue immediately to Evasive. Sprint 52 is a checkpoint, not a stopping
  condition.

## 2026-08-01 Sprint 53 Evasive entry audit

- The authoritative level-15 feature conditionally grants native Evasion,
  Uncanny Dodge, and Improved Uncanny Dodge while grit is positive, substituting
  Gunslinger level for Rogue level in the improved comparison.
- Exact installed candidate GUIDs are available, but their Kingmaker 2.1.7b
  components and class-level fields have not yet been observed in this runtime.
  A narrow save-free observer will resolve only those three identities and
  record their concrete components before implementation.
- Entry criteria are recorded in `planning/SPRINT-53-ENTRY-CRITERIA.md`. Next:
  commit and run the exact native-feature observer; do not infer scaling from
  display names or another Owlcat game.

## 2026-08-01 Sprint 53 Evasive source qualification

- Exact observer `20260802T0418217104942Z-observe-evasive-native-features`
  confirmed native Evasion uses the Reflex Evasion component, Uncanny Dodge
  uses FlatFootedIgnore plus its pre-initiative condition, and Improved
  Uncanny Dodge uses CannotBeFlanked with no attacker-level comparison.
- Added a level-fifteen wrapper and three hidden project-owned native clones.
  Positive grit installs all three benefits; exact grit Spend/Restore postfixes
  refresh only the owning unit and exact grit resource. Zero grit removes only
  the project-owned clones, preserving facts granted by other classes.
- Added five focused policy cases and guarded save-free scenario
  `disposable-gunslinger-evasive`. Repository validation, focused runtime
  request/preflight/runner checks, all 795 domain tests, Release build, and
  strict standalone package validation pass.
- Next: complete the reconstructable source audit and commit, rebuild the exact
  commit, require mod-load PASS, and run two independent feature PASS launches.

## 2026-08-01 Sprint 53 Evasive runtime qualification

- Source checkpoint `a79f4b2` exposed one exact Harmony binding fault during
  guarded mod load: native Restore names its first parameter `blueprint`, not
  `resource`. The mod failed closed before initialization and no save interaction
  occurred. Correction `28b8b83` binds the installed name and pins it in the
  Sprint validator.
- Exact corrected package SHA-256 is
  `803895664337cb5716d3de931856b45e50bdef78a2c1b992befb19f92ca25e0b`;
  DLL SHA-256 is
  `a3c9b64f85e0c803e4c335249d3e859affa8d9ba00e9addabd613bb733a3a523`.
- Corrected mod load passed in `20260802T0436563082877Z-mod-load-smoke`.
  Independent feature runs
  `20260802T0438185768148Z-disposable-gunslinger-evasive` and
  `20260802T0439372742098Z-disposable-gunslinger-evasive` both passed.
- Both runs observed level 15, grit `4->0->1`, all benefits
  `True,False,True`, other-unit isolation, exact disposal, and no save use.
- Continue immediately to Menacing Shot; Sprint 53 is a checkpoint only.

## 2026-08-01 Sprint 54 Menacing Shot entry audit

- The authoritative rule requires level 15, exactly 1 grit, a firearm shot into
  the air, all living creatures in a 30-foot-radius burst, native fear-spell
  treatment, and DC `10 + floor(Gunslinger level / 2) + Wisdom modifier`.
- The shot has no target attack roll or damage. The established no-roll
  discharge boundary can consume one loaded chamber while preserving firearm
  condition, but grit and chamber rollback must be one atomic delivery.
- No current checkpoint qualifies Kingmaker's exact Fear spell components,
  immunity handling, duration, living filter, ally/self inclusion, or a native
  self-centered burst. Sprint 54 therefore begins with a save-free exact
  contract observer; no behavior will be inferred from display names.
- Entry criteria are recorded in `planning/SPRINT-54-ENTRY-CRITERIA.md`.

## 2026-08-02 Sprint 54 Menacing Shot source qualification

- The native Fear observer identified ability
  `d2aeac47450c76347aebbc02e4f463e0`: Will save with MindAffecting, Fear, and
  Emotion descriptors. Failure applies native Frightened
  `f08a7239aa961f34c8301518e71d4cdf` for caster-level rounds; success applies
  native Shaken `25ec6cb6ab1845c48a95f9c20b034220` for one round.
- Added a level-fifteen feature and self-activated standard-action ability. Its
  custom delivery selects living creatures within 30 feet, including self and
  allies, and atomically consumes one grit plus one loaded firearm chamber.
  The retained native Fear action applies the exact observed save outcomes at
  DC `10 + floor(Gunslinger level / 2) + Wisdom modifier`.
- Added six focused policy cases and guarded save-free scenario
  `disposable-gunslinger-menacing-shot`. Repository/Sprint validation, request,
  preflight and runner checks, all 801 domain tests, clean Release build, output
  validation, and strict package validation pass.
- Compile-candidate package SHA-256 is
  `7f68b580c249d6202c1d9a501876fd9d9871fdb92ba445bc60b5904fcddf6a1a`;
  DLL SHA-256 is
  `01bf2d015c7aa1642407cb64fdd9596bba98f6b6f805afd7051ac3364f697205`.
- Next: commit this reconstructable source checkpoint, rebuild the exact commit,
  require exact mod-load PASS, and run two independent feature launches.

## 2026-08-02 Sprint 54 Menacing Shot runtime qualification

- Exact source commit is `24a735e2e9b6b2261cf249515300ae1528311d06`.
  Exact package SHA-256 is
  `ee2c66a181d7b0b60d186da892c27ada2f23f863e7ebe26dfbc689a84bc0b05a`;
  DLL SHA-256 is
  `d94d2465fae2974370a2b803ecb057c168420550340e53dd4e2c7e5ebb2fcb27`.
- Guarded exact mod load passed at
  `20260802T0510264214475Z-mod-load-smoke`.
- Independent feature runs
  `20260802T0511440259050Z-disposable-gunslinger-menacing-shot` and
  `20260802T0513015518728Z-disposable-gunslinger-menacing-shot` both passed.
- Each run observed two level-fifteen/Wisdom-18 disposable Gunslingers with
  DC 21 and caster level 15. Each delivery included self, changed grit `4->3`
  and its loaded chamber `1->0`, applied native Frightened on the forced failed
  Will save and native Shaken on the forced successful save, then disposed both
  units without save or external-unit interaction.
- Continue immediately to Slinger's Luck; Sprint 54 is a checkpoint only.

## 2026-08-02 Sprint 55 Slinger's Luck entry audit

- Authority requires a saving-throw reroll for fixed cost 2 or a skill-check
  reroll for fixed cost 1, always retaining the second result even when lower;
  no effect may reduce either cost.
- Kingmaker has no safe generic post-roll player interruption. The established
  project adaptation is two explicit pre-roll arming choices whose next matching
  unit-owned rule event consumes one marker and requests one native reroll.
- No current exact observation proves the installed saving-throw and skill-check
  rule types, result members, or safe replacement phase. Sprint 55 therefore
  begins with a save-free contract observer before gameplay implementation.
- Entry criteria are recorded in `planning/SPRINT-55-ENTRY-CRITERIA.md`.

## 2026-08-02 Sprint 55 native reroll observation and source implementation

- Exact observer commit `fbd03ae` passed mod load at
  `20260802T0520533425683Z-mod-load-smoke` and save-free observation at
  `20260802T0522131721778Z-observe-slingers-luck-native-rerolls`.
- Installed `RuleSavingThrow` and `RuleSkillCheck` expose read-only computed
  `RollResult` and exact non-public `D20` setters. Post-trigger replacement
  changed completed native rolls `9->1` and `14->1`; native `Dice.D20` is a
  readable `RulebookEvent.RollEntry`. The observer disposed its detached unit.
- Added separate fixed-cost arming abilities and unit-owned markers. Their exact
  initiator rule handlers draw one native second d20, replace the completed rule
  result, spend exactly 2 or 1 grit, always keep the second roll, and roll back
  both result and grit on failure. Six policy cases cover costs and fail-closed
  gates.
- Next: complete source/build/package gates, commit the reconstructable
  checkpoint, then require mod load and two independent feature PASS launches.

## 2026-08-02 Sprint 55 Slinger's Luck runtime qualification

- Initial exact-commit runs failed closed without spending grit or consuming
  markers. The first verifier also incorrectly compared the modifier-inclusive
  `BaseRollResult` with a natural d20.
- Qualified native IL proved every runtime `Fact` clones and binds its blueprint
  `GameLogicComponent` instances and exposes them through `CallComponents<T>`.
  It also confirmed `RuleInitiatorLogicComponent<T>` uses the initiator-rulebook
  subscriber contract rather than the ordinary unit-subscriber contract.
- Corrected production handlers validate `rule.D20.Value`, preventing ordinary
  skill/save modifiers from being rejected as invalid natural rolls. The
  detached scenario invokes the actual bound marker component through native
  `Fact.CallComponents<T>` and asserts the retained `D20.Value`.
- Exact commit `a67a9300829602f8e17d904273a063f8266ae9b0` passed all Sprint
  validators, 807/807 domain tests, clean exact-reference Release compilation,
  strict packaging, and mod load run
  `20260802T0603271690563Z-mod-load-smoke`.
- Two independent fresh-launch feature PASS runs are
  `20260802T0604472262192Z-disposable-gunslinger-slingers-luck` and
  `20260802T0606065893610Z-disposable-gunslinger-slingers-luck`. Both observed
  saving `17->10`, saving grit `4->2`, skill `17->10`, skill grit `2->1`, both
  markers consumed, other-unit grit `4->4`, and detached cleanup.
- Exact package SHA-256 is
  `a39c776bb026280bb92594a34b8c6e72b790dabbe0cfa5bcb01de217b837aa9b`;
  exact built/deployed DLL SHA-256 is
  `219d52d8e45aa0372e0300406a4a3b4e24b97b0c55a72d15ad4231c301746eae`.
- Sprint 55 is complete as a checkpoint. Continue immediately to Cheat Death.

## 2026-08-02 Sprint 56 Cheat Death runtime qualification

- Entry criteria fixed the exact post-damage contract: level 19, completed
  owned `RuleDealDamage`, final HP at or below zero, and at least one grit.
- The production target handler rejects zero-damage observations, spends all
  remaining grit atomically, and changes native damage to leave exactly 1 HP.
- Commit `10a42747ad500d1bf7bf9593a12c31b9ed828c97` passed the full validator
  chain, 813/813 tests, clean exact-reference Release build, and strict package
  validation.
- Mod load passed at `20260802T0626527728449Z-mod-load-smoke`. Independent
  feature runs `20260802T0629237809884Z-disposable-gunslinger-cheat-death` and
  `20260802T0630482940232Z-disposable-gunslinger-cheat-death` both observed
  `maxHp=137`, grit `1->0`, final HP 1, zero-grit control HP -10, level-19
  progression, and detached cleanup.
- Exact package/DLL SHA-256 are `3877860c6d2f955afd991740b4b38ad2cf86bc8d1f3521b204bff87624c7c161` /
  `42f5122584447f195f3a66dbe2fbce5737a7d432acae35049f28a207503064b9`.
- Sprint 56 is complete as a checkpoint. Continue immediately to the next
  incomplete coverage item.

## 2026-08-02 Sprint 57 Death's Shot native-contract investigation

- Entry criteria commit `b975f49` requires normal firearm damage, exact
  Fortitude DC, native Death immunity, and suppression of both grit-recovery
  routes; synthetic HP/state death is forbidden.
- Observer commit `c1305fc` passed mod load at
  `20260802T0642181684393Z-mod-load-smoke`.
- First observer `20260802T0643380649281Z-observe-deaths-shot-native-death`
  failed because exact Destruction GUID `3b646e1db3403b940bf620e01d2ce0c7`
  uses divine damage on both save branches and contains no kill action.
- A broader catalog observer on `362a17b` found 16 Death-descriptor abilities
  and three Fortitude/kill candidates: Scaled Fist Quivering Palm
  `749e77f7014cb4e4487400e508e70a59`, Monk Quivering Palm
  `4de518e69f9b8094fb996b1599d00314`, and conditional Death Clutch
  `c3d2294a6740bc147870fff652f3ced5`. Run
  `20260802T0646499769488Z-observe-deaths-shot-native-death` failed uniqueness.
- The two-attempt fail-closed limit is reached. Death's Shot is temporarily
  blocked pending authority to select a native graph; continue independently
  to Stunning Shot.

## 2026-08-02 Sprint 58 Stunning Shot runtime qualification

- Source checkpoint `9ddb210` added the level-19 free-action marker, exact
  native Stunned clone, 2-grit policy, native critical-immunity gate, and six
  focused domain cases; the complete suite is 819/819 PASS.
- Native Stunned observer passed at
  `20260802T0658067764449Z-observe-stunning-shot-native-stunned` after proving
  GUID `09d39b38bb7c6014394b6daced9bacd3` and `AddCondition(Stunned)`.
- Runtime harness checkpoint `0e00284` was narrowed after exact evidence: the
  installed hit result is read-only, and detached attacks require the existing
  native `CreateRuleDealDamage(false)` pattern to materialize damage.
- Final exact commit `f5dc6bbc8f2443c54b1294b1815c000288b4ccea`
  passed mod load at `20260802T0726286857532Z-mod-load-smoke`.
- Independent PASS runs
  `20260802T0727464837674Z-disposable-gunslinger-stunning-shot` and
  `20260802T0729102463349Z-disposable-gunslinger-stunning-shot` both observed
  availability at level 19, chamber `1->0`, native damage `0->3`, grit
  `4->2->0->2`, deterministic natural-1/native Stunned and natural-20/no-buff
  Fortitude branches, critical-immunity no-spend, all markers consumed, and
  exact detached cleanup.
- Exact package/DLL SHA-256 are
  `dd0b46951c3d8c963b5705c1a6ad999b379dbe75f81ee8467cc796cc7d7ef777` /
  `58377a40be8084968bf1e7471875b80d47f1abfe8af41a79c8a1044f78ca045b`.
- Sprint 58 is complete as a checkpoint. Continue immediately with True Grit.

## 2026-08-02 Sprint 59 True Grit runtime qualification

- Entry criteria fixed 18 eligible choices, two distinct selections, a
  one-grit reduction to minimum zero, the remaining positive-grit floor for a
  positive cost reduced to zero, removal of pre-existing no-spend gates, and
  Slinger's Luck's fixed exclusion.
- Commits `188f345`, `649d3c1`, and `ec9e9c0` added the central policy,
  selection blueprints/stable IDs, and adapters for every offered deed.
- Runtime harness commit `1d7c5b6fac06c9342ffd88895551e62af991eee9`
  passed 827/827 tests, 76 preflight checks, 36 request checks, clean Release
  build, and strict package validation.
- Mod load passed at `20260802T0801211781568Z-mod-load-smoke`. Independent
  PASS runs `20260802T0802418511488Z-disposable-gunslinger-true-grit` and
  `20260802T0804057434013Z-disposable-gunslinger-true-grit` both observed
  grit `4->3->2->4`, one chamber, native damage, deterministic native save
  branches, two selections/18 choices, zero-grit policy edges, variable cost,
  fixed exclusion, and detached cleanup.
- Exact package/DLL SHA-256 are
  `4135fcfe0df73990c9a3cdac246462d64124638f7550cdf5939e87f51b84a144` /
  `ad0beec416f6a4f79b06203a7a664c566e04a3d34bb2e6814bb535a6bc41ec90`.
- Sprint 59 is complete as a checkpoint. Continue immediately to the next
  incomplete independent coverage item.

## 2026-08-02 Sprint 60 player-facing presentation qualification

- Entry criteria and implementation added stable localized presentation,
  recursive progression/selection/AddFacts traversal, hidden-fact exclusion,
  icon preservation/fallback, and native progression UI groups.
- Initial guarded launches failed closed after 125 registrations because the
  installed Fighter icon was null. Root-cause diagnostic commit `c9b4790`
  exposed the exact exception; `42041ef` then proved the Fighter progression
  icon was also null. ADR-0007 inspection changed the architecture to the
  inherited production Early Pistol/Light Crossbow icon on `adcb030`.
- Exact source `adcb0300b67de86c2a4b7ea80cf573778fb43e36` passed the complete
  validator chain, 827/827 tests, 37 request checks, 78 preflight checks, clean
  exact-reference Release build, strict packaging, and mod load run
  `20260802T0838152303512Z-mod-load-smoke`.
- Independent fresh-process PASS runs
  `20260802T0839341612816Z-observe-gunslinger-presentation` and
  `20260802T0840534291165Z-observe-gunslinger-presentation` both observed 20
  levels, 75 visible project facts, one hidden fact, zero incomplete facts,
  six UI groups with 21 grouped features, and readable class/progression
  presentation. No save was loaded or written.
- Exact package/DLL SHA-256 are
  `94e3c83c32600abf3f12a18da55f693a91b7174866735add5aff3cefb7a52d0d` /
  `9989c449b4859c24a59990bb9e46732e38b009e25e9332461f1439e08a827a53`.
- Sprint 60 is complete as a checkpoint. Continue immediately with Sprint 61
  later equipment acquisition and economy.

## 2026-08-02 Sprint 61 acquisition entry audit

- Mission 4.5, the Sprint 38 roadmap, and ADR-0035 require a normal later
  acquisition route, with vendor or loot distribution explicitly assigned.
- Existing firearm, ammunition, and repair-kit item costs provide local economy
  authority; no new price or balance decision is necessary.
- `docs/research/blueprint-candidates.csv` still marks Oleg and capital vendor
  table IDs unresolved. Production mutation therefore cannot be selected from
  narrative names alone.
- `planning/SPRINT-61-ENTRY-CRITERIA.md` requires a guarded, save-free,
  non-initiating installed-contract observer to prove exact table identities,
  content shape, duplicate/restock behavior, and rollback before implementation.
- Next action: implement and source-qualify that exact vendor-table observer.

## 2026-08-02 Sprint 61 later equipment acquisition qualification

- Read-only observer checkpoints mapped 43 shared vendor tables, 26 direct
  associations, and the exact capital Jhod route. After two incorrect item
  resolver theories failed closed, metadata-only commit `04eec65` proved
  `LootItemsPackFixed.m_Item` is a native `LootItem` wrapper with read-only
  `Item`; `f80480d` then resolved exact item identities safely.
- Stock-pattern commit `f3b113d` observed installed quantity authority:
  non-stackable count one occurs 498 times; the target capital table uses 99
  for 50 ordinary stackables and 2000 only for Diamond Dust.
- Transaction checkpoint `7242f75` added append-order, idempotence,
  ambiguity, and rollback tests. Production commit `c2fd27b` publishes four
  player-fireable firearms at one each and three consumables at 99 each,
  retaining 51 native entries and excluding Blunderbuss.
- Complete validation passes 831/831 tests, clean exact-reference Release
  build, strict package validation, 38 request checks, and 80 preflight checks.
- Exact mod load `20260802T0931384431105Z-mod-load-smoke` passed. Independent
  PASS runs `20260802T0932570144693Z-observe-vendor-table-contracts` and
  `20260802T0934221001134Z-observe-vendor-table-contracts` observed 58 table
  entries, seven exact project entries, zero invalid counts, and zero
  Blunderbuss entries without shop, inventory, or save interaction.
- Package/DLL SHA-256 are
  `6bbd63bf662197b540684b09cd4a84eebe275eb210ad86804fdc736a6d9f0819` /
  `e31962312cc583eb83cdb5e27645816f90c1acf409360243adfa687654288169`.
- Sprint 61 is runtime-qualified. Next action: re-audit the coverage matrix and
  select the highest-dependency incomplete independent row.

## 2026-08-02 Sprint 62 scatter completion authority audit

- Reconciled ADR-0037, Sprint 32 criteria, current scatter source/tests,
  production Blunderbuss definition, installed 90-degree cone API, and the
  authorized local firearm table/rules.
- The native API requires a numeric distance; all local authority retains only
  `special`. Any autonomous distance would materially change balance.
- Scatter remains fail-closed and temporarily `BLOCKED` pending that one
  player-facing choice. Independent next action: production fallback visuals,
  animation, sound, and projectile qualification.

## 2026-08-02 Sprint 63 fallback presentation entry audit

- ADR-0007 authorizes crossbow-compatible core-mod presentation and makes
  custom models, sounds, icons, projectiles, and animations independently
  replaceable. Installed metadata localizes the exact contract in
  `WeaponVisualParameters`, item equipment/model fields, item icon, and
  inventory sound fields.
- `planning/SPRINT-63-ENTRY-CRITERIA.md` requires exact Light Crossbow
  inheritance for Pistol/Revolver and Heavy Crossbow inheritance for
  Musket/Blunderbuss/Rifle, with no mechanical or packaged-asset change.
- Next action: implement the save-free exact fallback observer.

## 2026-08-02 Sprint 63 fallback presentation qualification

- Observer checkpoint `02d26a4` added exact item/type/nested visual comparison.
  The first launch failed closed at scenario allowlisting and left one exact
  harness-owned Kingmaker process; PID 37780 was verified by path and stopped
  without save interaction. Repair `82264b4` added the missing C# allowlist and
  timeout exemption and strengthened static validation.
- Exact mod load `20260802T0946237925580Z-mod-load-smoke` passed. Independent
  PASS runs `20260802T0947439357619Z-observe-production-firearm-fallbacks` and
  `20260802T0949087984758Z-observe-production-firearm-fallbacks` proved exact
  Light/Heavy source mapping for all five firearms, one projectile each, and
  non-null icons with no blueprint/gameplay/save mutation.
- Complete validation passes 831/831 tests, 38 request checks, 82 preflight
  checks, clean Release build, and strict packaging. Package/DLL hashes are
  `99dc424e24ecf853a1cc24ec87f938a11ff30cc433d02bf49089bc2391d79908` /
  `ab541c2b01dff4d272cde383f3f13175e35294a64e90b40c73bf84a4ab710598`.
- The fallback presentation row is `ADAPTED-ACCEPTED`. Continue immediately to
  the next incomplete independent coverage item.

## 2026-08-02 Sprint 64 production critical-profile checkpoint

- Roadmap audit found no special-ammunition deliverable. Alchemical cartridges
  appear only as an explicitly absent Lightning Reload prerequisite requiring
  separate authority; none were invented.
- Source commit `fdf54ec` adds an explicit live assertion over all five
  registered native weapon-type critical fields. Repository validation,
  831/831 tests, 38 request checks, 82 preflight checks, clean exact-reference
  Release build, and strict package validation pass.
- Package/DLL SHA-256 are
  `129000a03208443c33245a72bc44b4b63bbfffb2e8a433da760b8627f7ce2a14` /
  `1bff1d974c06ce58ea0bdb231402b0c18e62da787b08bbbb8faf7dc7e335d356`.
- Exact mod load `20260802T0957016959514Z-mod-load-smoke` passed. The first
  save-backed catalog request was externally rejected before launch; no save
  or game process was touched. Continue the independent lifecycle audit.

## 2026-08-02 Sprint 65 save-free firearm lifecycle Stage A

- Source checkpoint `1e206d1` adds a save-free detached-item observer for the
  exact native collection/factory surfaces, same-item token reconstruction,
  same-blueprint new-item isolation, and repository removal isolation.
- Complete gates pass: 831/831 tests, 38 request checks, 84 preflight checks,
  seven focused checks, clean exact-reference Release build, and strict
  packaging. Package/DLL SHA-256 are
  `528d22fff0516528afdb9112c0775a837304761bc9055659bfcb72fadb68ffdd` /
  `c6322627269e8503f57d3c2c2fcaf03ceab503eecbdd198cd7b3fdd056942fea`.
- Exact mod load `20260802T1005415884560Z-mod-load-smoke` and independent
  lifecycle PASS runs `20260802T1007011584387Z` / `20260802T1008259836323Z`
  observed native contracts true, source tokens `1->1->0`, created tokens zero,
  exact loaded reconstruction, and canonical empty created state.
- Live inventory/stash/vendor and durable sale/restart paths remain pending;
  Stage A is not overstated as full runtime qualification. Continue to the next
  independent source-qualified coverage row.

## 2026-08-02 Sprint 66 production firearm actions

- Audit proved Reload, Repair, and Overhaul mechanics were already generic and
  definition-driven, while their player-visible names and diagnostics still
  exposed the development-only Test Musket label.
- Source checkpoint `087e11a` changes only presentation and diagnostics while
  retaining stable symbols, GUIDs, compatibility types, action economy,
  targeting, and transaction behavior.
- Complete qualification passed: five focused checks, 38 request checks, 84
  preflight checks, repository validation, 831/831 domain/reflection tests,
  clean exact-reference Release build, and strict package validation.
- Exact mod load `20260802T1020144242399Z-mod-load-smoke` passed. Independent
  presentation runs `20260802T1021336705231Z` and `20260802T1022571370114Z`
  passed with 20 levels, 75 visible facts, zero incomplete facts, and exact
  actions `Reload Firearm,Overhaul Firearm,Repair Firearm` with no Test Musket
  descriptions.
- Runtime package/DLL SHA-256 are
  `cf8ff7859a5b5fe9373524af4310c05c66d2ea96714aace83189e17c99d352d0` /
  `b79daa5d57d411a0bdcd190b8f58b88dd54dc6636c37ddcaf339e30535bb0490`.
- Player actions and diagnostics are runtime-qualified. Continue immediately
  to the next independent incomplete final-integration row.

## 2026-08-02 Sprint 67 release documentation and package contract

- Replaced the disposable Sprint-era installation paragraph with a packaged
  installation/update/removal/compatibility guide. It requires backups and
  complete-folder replacement, warns that custom serialized references make
  uninstall safety unqualified, and bounds runtime/dependency/mod-compatibility
  claims.
- Source checkpoint `cb6ffd7` advances both ordinary and deterministic package
  enforcement to an exact nine-file allowlist containing the guide. Historical
  version-specific eight-file archive evidence remains historical.
- Six focused checks, 38 request checks, 84 preflight checks, inherited source
  validation, and the complete 831/831 suite pass. Clean exact-reference
  Release build and both strict package paths pass before and after commit.
- Exact committed-tree package/DLL SHA-256 are
  `1a350b6deab0855abeaaa967c4e3cd09dbac55d0ec7bb097413ca5253f86ed3a` /
  `902fda0ecbb0853f3217f1bf8065d6304fc5e07fe4a4c1464ede49aebb85de09`.
- This mandatory documentation row is COMPLETE. No runtime launch was needed
  because no assembly source, blueprint, or runtime input changed. Continue to
  the next incomplete mandatory row.

## 2026-08-02 Sprint 68 deed runtime requalification audit

- Exact mod load `20260802T1031471777161Z-mod-load-smoke` passed.
- Startling Shot runs `20260802T1033081927669Z` and
  `20260802T1038097680536Z` failed closed at native timed-buff return. Detached
  liveness repair `b7eeaa9` passed 831/831 tests and packaging but did not alter
  the result, so the two-attempt rule ended further Startling probing.
- Targeting Head run `20260802T1039503873162Z` proved native hit, grit `3->2`,
  chamber `1->0`, and active Confusion while `AddBuff` returned null. Exact
  repair `1928bba` reconciles the installed fact and explicitly dispatches the
  detached hit's damage rule; complete 831-test/build/package gates pass.
- Repaired run `20260802T1042345480789Z` proved damage `0->5`, grit `3->2`,
  chamber `1->0`, exact six-second nonpermanent Confusion, and cleanup. Its only
  FAIL is the observer reading `Attack.MeleeDamage`, which remains null because
  the explicitly triggered native damage rule is separate. The two-attempt
  limit ends further assertion-only retries.
- Exact `1928bba` package/DLL SHA-256 are
  `566063eea17b61b6b5e04e594113a471815a27db180a418f95b085d2eeb34b8c` /
  `cb0c5abd017c0d6d8894d62a1b9c4a8481185dbae19daa8cefa8bd20e1cae9d4`.
- Both are bounded engineering evidence gates, not overall mission stops.
  Continue the next independent coverage item.

## 2026-08-02 Sprint 69 save-free production critical profiles

- Source checkpoint `7f3a721` extends the existing read-only vendor/catalog
  observer with the exact five registered native critical profiles, avoiding
  the externally denied save-backed catalog path.
- Four focused checks, 38 request checks, 84 preflight checks, inherited source
  validation, 831/831 tests, clean exact-reference Release build, and strict
  nine-file packaging pass.
- Exact mod load `20260802T1049001530168Z-mod-load-smoke` passed. Independent
  save-free observations `20260802T1050199707554Z` and
  `20260802T1051400747562Z` both reported
  `pistol=20/x4;musket=20/x4;blunderbuss=20/x2;rifle=20/x4;revolver=20/x4`
  while all vendor-publication and observation-only assertions remained PASS.
- Package/DLL SHA-256 are
  `2ebec0739a9d04e994a7e2ae1ee82b26fac98ae4306fbd0fe34aecdaab69ac35` /
  `383e9fd6c92da6c243df6ddd7f7a9f8957c574b9231bcf4d2aa4e8c6fa81c77e`.
- The critical-profile/special-ammunition row is runtime-qualified; no absent
  special-ammunition feature was invented. Continue immediately.

## 2026-08-02 Sprint 70 safely reversible level-up commit

- Installed IL proved first-level native commit is unsafe for a detached
  autonomous fixture because it can publish custom companions into cross-scene
  state and remote companions. The observer therefore commits only level two
  in exact `LevelUp` mode on a disposable native unit.
- Source `73e4b20` and callback-order correction `d0b15f6` pass six focused
  checks, 84 preflight checks, repository validation, 831/831 tests, clean
  exact-reference Release build, and strict nine-file package validation.
- Exact mod load `20260802T1100483093512Z-mod-load-smoke` passed. First commit
  run `20260802T1102097358773Z` failed before commit on a reflected argument-
  order defect. The corrected second/final materially different run
  `20260802T1105339180432Z` passed Gunslinger `0->1->2`, preview level two,
  native success callback, and exact external-state isolation.
- Exact `d0b15f6` package/DLL SHA-256 are
  `50526cb1c6d5b4c82661717a9e6cac9ca7d9a4e9c644ba3e1beb9fba673941da` /
  `5bf4bd967aab6131fdbbcceb22d0f4dc0ddc0dd8608371de304ab71733e9e012`.
- This closes the safely reversible level-up commit gap with strong single-run
  evidence. It is not overstated as two-run reproduction. Continue immediately
  to the next independent final-integration gap.

## 2026-08-02 Sprint 71 current-build working-save regression

- The exact `d0b15f6` package passed the canonical guarded Steam
  `working-save-smoke` twice on named disposable save
  `KMG_AUTOMATION_WORKING`: `20260802T1108448693251Z` and
  `20260802T1110221239004Z`.
- Both fresh launches passed readiness, exact save identity, load correlation,
  completion, fingerprint, and save-write evidence without touching the
  protected baseline save.
- This advances final acceptance to runtime-partial but is not comprehensive
  mechanical acceptance. Remaining design-authority and integrated feature
  gates are preserved; continue to an independent actionable item.

## 2026-08-02 Sprint 72 save-free level-twenty progression

- The external reviewer rejected the existing save-backed
  `production-firearm-catalog` launch because feature-specific save-backed
  scenarios lack explicit boundary permission. No workaround was attempted;
  the next safe independent class-chassis gap was selected.
- Source `ddea6cc` applies exact native `CharGen` level one and nineteen exact
  native `LevelUp` actions to one detached unit without `Commit` or save load.
  Seven focused checks, 84 preflight checks, repository validation, 831/831
  tests, clean Release build, and strict package validation pass.
- Exact mod load `20260802T1117198249088Z` and fresh save-free progression runs
  `20260802T1118455028165Z` / `20260802T1120048623643Z` all passed. Both
  progression runs observed level 20, BAB 20, saves `12/12/6`, 29/29 direct
  progression facts, and external isolation.
- Exact package/DLL SHA-256 are
  `f258eb67a4e1ba36226daf944ee974b0878d01c30dc91fadd5d1238ee8988752` /
  `653fdc9bf9778bd5306001b71a2dd18aa691e612d5a239022022cc116e3976bb`.
- Continue to another save-free integration gap; do not retry denied
  save-backed feature scenarios without explicit permission.

## 2026-08-02 Sprint 73 save-free multiclass commit

- Source `7fbdfae` adds a detached Fighter 1 -> Fighter 1/Gunslinger 1 native
  `LevelUp` commit with success callback, exact proficiency/grit fact checks,
  and party/global/cross-scene/remote-companion/inventory isolation.
- Five focused checks, 84 preflight checks, repository validation, 831/831
  tests, clean exact-reference Release build, and strict packaging passed.
- Exact mod load `20260802T1126008241032Z` and fresh multiclass runs
  `20260802T1127226945921Z` / `20260802T1128499610464Z` all passed.
- Exact package/DLL SHA-256 are
  `7b6680e6006fa9746efdf2a52fbf4183f45548619507148023c803b6e039d5d7` /
  `bd45166466a4146463892081527486648251f1af9d539f2f90a901094c7c1928`.
- Multiclass commit is qualified. Continue to the next safe class-integration
  gap; creation commit and broad respec replacement remain separate.

## 2026-08-02 Sprint 74 detached respec replacement commit

- Source `163320d` committed a detached `Respec` replacement candidate without
  invoking the globally mutating `Player.RespecCompanion` callback.
- First run `20260802T1136179100890Z` passed source/replacement/fact assertions
  but exposed native first-level starting-inventory leakage in isolation.
- Repair `2eb0090` adds exact reference/quantity rollback plus starting-gold and
  shared-money stability. Seven focused checks, 84 preflight checks, repository
  validation, 831/831 tests, clean Release build, and packaging pass.
- Exact mod load `20260802T1140330341524Z` and corrected final run
  `20260802T1141509870468Z` passed source Fighter `1/0`, replacement `0/1`,
  callback, proficiency/grit facts, inventory/gold rollback, and isolation.
- Exact package/DLL SHA-256 are
  `412c258fc4af0bb86041a3b525a074f6abdedf0842b587cf8f4acde3340fec59` /
  `e3c7f7eccc23b82c8e23c54e41a481152fcd7aa7f1327d75451345216845f281`.
- Do not run a third respec replacement attempt. Continue to an independent
  save-free gap; broad player replacement remains separate.

## 2026-08-02 Sprint 75 evaluated hit die and skill points

- Installed native level-up IL established the exact d10 player base-class HP
  contract `0 -> 11 -> 18` and class-plus-Intelligence skill-point evaluation.
- Source `e35de17` adds a detached, save-free exact `CharGen` then `LevelUp`
  observer with Intelligence fixed at 10 and no commit or shared inventory.
- Seven focused checks, 26 runner checks, 84 preflight checks, repository
  validation, 831/831 tests, clean Release build, and strict package validation
  passed. The preflight's known artifact-fingerprint race failed once and
  passed all 84 immediately when rerun in isolation; no guard was weakened.
- Exact mod load `20260802T1149510654742Z-mod-load-smoke` and fresh observations
  `20260802T1151111616673Z` / `20260802T1152385768115Z` passed d10, HP
  `0/11/18`, skill base 4, evaluated points `4/4`, class level 2, and external
  isolation.
- Exact package/DLL SHA-256 are
  `86c3ad51eb2ad54bdaebc6f77a1a3b592893ee578d14b61d9386bbd04ff797b7` /
  `c92ee42f8662459bc4d588a093ad87d31dff3e1c760bb9f31fd4225e1dade908`.
- The class-chassis row is runtime-qualified when combined with Sprint 72's
  reproduced level-20 BAB/saves/facts. Continue immediately to the next
  independent integration or acceptance gap.

## 2026-08-02 Sprint 76 Startling Shot applied-fact reconciliation

- Re-auditing normal first-level creation commit reconfirmed Sprint 70's exact
  safety exclusion: `SetupNewCharacher` may publish a detached unit into
  cross-scene and remote-companion collections. It was not invoked.
- Prior guarded Startling Shot evidence and the independently qualified
  Targeting Head contract prove native `AddBuff` can return null after installing
  the exact requested timed fact. Source `3a26059` now reconciles exactly one
  matching `BuffCollection.RawFacts` entry before treating delivery as failed.
- Existing ambiguity remains fail-closed through `SingleOrDefault`; absence or
  duplicates retain atomic firearm-state rollback. Action, grit, duration,
  targeting, and condition policy are unchanged.
- Inherited source validation, 831/831 tests, 26 runner checks, 84 preflight
  checks, clean Release build, and strict packaging passed. The uncommitted
  candidate package/DLL hashes were
  `ade103b85c2a9bfda0dd6fb856ff2b9898ab00ce748074972decb5f8ee606fc9` /
  `1f069a4370db57c2a940574d0bd70582b282b0eea57d9f9ebd0c9593afa55c0d`.
- The two-attempt limit prohibits a third Startling Shot runtime launch. Keep
  the deed source-qualified pending newly authorized runtime evidence and
  continue immediately to an independent coverage item.

## 2026-08-02 Sprint 77 Targeting Head native-damage evidence correction

- Retained final guarded evidence already proved a hit, grit `3 -> 2`, chamber
  `1 -> 0`, target damage `0 -> 5`, exact six-second Confusion, and cleanup.
  Its sole failed assertion consulted `Attack.MeleeDamage`, although the adapter
  intentionally dispatches the native `RuleDealDamage` as a separate rule.
- Source `4485dba` corrects only the observer to use the authoritative target
  damage delta. Production deed mechanics and native damage delivery are
  unchanged.
- Inherited validation, 831/831 tests, 26 runner checks, 84 preflight checks,
  clean Release build, and strict packaging passed. The source candidate
  package/DLL SHA-256 were
  `bc943da3b9f271439f36d8c54134bced34cb6e34cf0a16daf340b11808134cee` /
  `8d5f0ec51a41d869729df815d60664ef20171e7ece74f80f23f6656f7743c2f2`.
- The two-attempt limit still prohibits a third Targeting Head launch. Preserve
  the strong retained runtime evidence, keep the corrected observer
  source-qualified, and continue to an independent coverage item.

## 2026-08-02 Sprint 78 save-free corrupt token preservation

- Source `2505613` extended the detached production-firearm lifecycle observer
  with two distinct registered native token enchantments on one Early Pistol.
- Eight focused checks, inherited validation, 831/831 tests, 26 runner checks,
  84 preflight checks, clean Release, and strict packaging passed.
- Mod load `20260802T1205137472949Z` passed. First run
  `20260802T1206373328455Z` exposed that the correct rejection is
  `InvalidDataException`, not the observer's expected exception.
- Repair `731ff07`, mod load `20260802T1209365380684Z`, and the second/final run
  `20260802T1210579132106Z` passed reconstruction `1->1`, removal `1->0`, new
  item isolation, corrupt rejection with tokens preserved `2->2`, and cleanup
  without inventory or save mutation.
- Exact repaired package/DLL SHA-256 are
  `d9a7081ea6458ee3e0167a2d5fd60a6ac6623c5fcee04d7f7f5c7d1c270e199c` /
  `746c369f8211216595c68d0a4f848e574b15a5b3b32d75c7c545dc8901754e1c`.
- Do not run a third attempt. This is strong single-run evidence after one exact
  observer repair, not two-run reproduction. Continue independently.

## 2026-08-02 Sprint 79 production identical-firearm switching

- Source `7ab1a58` adds a detached save-free observer with two distinct
  production Early Pistol instances. The first is loaded/Normal and the second
  empty/Broken through their item-owned token repositories.
- Eight focused checks, 84 preflight checks, inherited validation, 831/831
  tests, clean Release, and strict packaging passed. One initial compile found
  only a missing existing resolver namespace import; the corrected full gate
  passed before commit.
- Exact mod load `20260802T1217444779507Z` and fresh switching runs
  `20260802T1219064706820Z` / `20260802T1220329632177Z` passed. Both selected
  each exact item after native primary-hand switching, preserved independent
  states, rejected two simultaneously equipped distinct firearms as ambiguous,
  and cleaned up without inventory or save mutation.
- Exact package/DLL SHA-256 are
  `414fefdf4536ede54b8f71693d4c198b5f4799b2f581a10f42c2cf7807c0bca0` /
  `624e9cdde3e8df4b019de8fe7f736e3ef0885b084667a2b7479cfd4acdc904ed`.
- Production equipment switching and identical-item isolation are reproduced.
  Continue immediately to another incomplete integration or acceptance gap.

## 2026-08-02 Sprint 80/81 comprehensive acceptance qualification

- Sprint 80 source commit `b3bd907` composed 30 already-qualified synchronous
  save-free slices under one guarded comprehensive scenario. Focused checks,
  runner safety 26/26, preflight 84/84, the complete 831-test suite, clean
  Release build, and strict package validation passed.
- Exact `b3bd907` mod load `20260802T1227366658010Z-mod-load-smoke` passed. The
  first comprehensive run `20260802T1228585887731Z` passed; the independent
  run `20260802T1230268400100Z` failed only Menacing Shot's forced-failure
  branch because its uncontrolled native d20 produced a natural 20. The other
  169 assertions passed. No third launch of that artifact was attempted.
- Sprint 81 commit `58baf84` made only the observer deterministic by selecting
  already-qualified native d20 seeds immediately before the native Fear saves:
  natural 1 for failure and natural 20 for success. Production Menacing Shot
  logic and balance are unchanged.
- Exact committed package SHA-256 is
  `2fd4229ceadaa2f2b59133bccff053dd2e05589b01cfd65eefa630cb0cc6a8b7`;
  DLL SHA-256 is
  `27d42233c2b464431f858c8d36b695fdeefab2c2f156de9b822450e5624b5dbc`.
  Mod load `20260802T1234452818737Z-mod-load-smoke` and independent
  comprehensive runs `20260802T1236067944023Z` and
  `20260802T1237355810113Z` all passed.
- Continue immediately with the remaining matrix audit. Comprehensive
  acceptance is a checkpoint, not mission completion; creation/respec,
  scatter, blocked deed decisions, and save-backed lifecycle remain separate.

## 2026-08-02 Sprint 82 native weapon-pipeline qualification

- Audited the mandatory native concealment, mirror-image, cover, LOS, range,
  critical, and damage row against ADR-0005 and current implementation. These
  mechanics remain Kingmaker-owned; the mod must prove preservation rather
  than duplicate them.
- Added seven focused compositional invariants proving production weapon-event
  bracketing, postfix-only misfire narrowing, contextual post-native AC delta,
  cover/flat-footed domain regressions, native damage/critical runtime paths,
  and comprehensive composition. No shipped gameplay source changed.
- Focused checks 7/7, runner safety 26/26, isolated preflight 84/84 after the
  known first-run race, repository validation, 831/831 tests, clean Release
  build, and strict package validation passed.
- Exact source commit is `a3243c9`. Package SHA-256 is
  `7e873edd2bd0f54da09de789e7f15dd0200bb2e095484578804df0b2855b2388`;
  DLL SHA-256 is
  `510ea8061af01a696b0f8b44dd186a798a65a6a3e9b94f08e41ac65dc6775696`.
- Exact mod load `20260802T1245266307096Z-mod-load-smoke` and comprehensive
  runs `20260802T1247085787494Z` and `20260802T1248346969150Z` passed.
  Native pipeline preservation is runtime-qualified; continue to the remaining
  creation/respec, Gunsmith, deed-design, scatter, and lifecycle gates.

## 2026-08-02 Sprint 83 Gunsmith/battered contract audit

- Read the authorized local base-class and Gunsmithing feat rules. The starting
  firearm is origin-bound, has nonowner Broken/unusable behavior, is scrap-only
  at 4d10 gp, and can be upgraded by its Gunslinger; Gunsmithing also covers
  crafting and rest-period restoration.
- Current architecture has exact starting-item grants, persistent item-owned
  condition, and atomic kit-based Repair/Overhaul, but no accepted originating
  owner, per-item sale value, crafting, or rest-work contract.
- Persistent owner binding, a class-bound Battered Pistol, and a
  maintenance-only adaptation are all feasible but materially differ in
  transfer, equipment, economy, save schema, and feat value. No local decision
  selects one, so autonomous implementation would violate the design-authority
  boundary.
- Recorded the full audit in
  `planning/SPRINT-83-GUNSMITH-BATTERED-CONTRACT-AUDIT.md` and the smallest
  precise decision in `HUMAN-INPUT-REQUIRED.md`. Persistent origin binding is
  recommended for fidelity. Broad first-level creation Commit remains
  prohibited independently.

## 2026-08-02 Sprint 84 Gunsmithing source qualification

- The user authorized Sprint 83 option 1: persistent originating owner, exact
  owner-bound effective condition, fixed 22 gp value, and a real automatically
  granted Gunsmithing feature gating Repair/Overhaul.
- Added the visible localized `KMG.Classes.Gunsmithing` blueprint at Gunslinger
  level one. Firearm Proficiency now grants Reload only; Gunsmithing exclusively
  grants Overhaul and Repair through native `AddFacts` ownership.
- Extended the append-only ledger to 127 identities (126 active, one reserved)
  and updated inherited validators to assert the new authority split.
- Focused contract, repository validation, 838/838 domain tests, clean Release
  build, output validation, and strict package validation passed. Package/DLL
  hashes are `b6c60c1a...3918` / `2d24750a...b05`.
- Next: commit the source-qualified tree, run exact mod load and two fresh
  save-free presentation observations, then implement origin persistence.
- Source commit `8f320a3` passed exact mod load
  `20260802T1312212200554Z` and fresh presentation PASS runs
  `20260802T1313522632011Z` / `20260802T1315168298512Z`. Both observed 20
  levels, 76 visible reachable facts, one hidden, zero incomplete, six groups,
  22 grouped facts, and Reload/Overhaul/Repair. Gunsmithing is runtime-qualified;
  continue exact-item origin persistence immediately.

## 2026-08-02 Sprint 85 battered origin ledger core

- Added a dependency-free immutable exact-item to exact-origin-unit ledger.
  Same-owner reconstruction is idempotent; different-owner rebinding fails
  without mutation; multiple items remain isolated; snapshots are defensive.
- Added six focused ownership cases. Repository validation and all 844 tests
  passed, followed by clean Release build and strict package validation.
- Package/DLL hashes are `5a55e919...9839a` / `4521c532...04fff`.
- Next: commit this core, host it in a save-owned UnitPart with primitive JSON
  records, then bind only the exact newly granted starting firearm.

## 2026-08-02 Sprint 86 battered ownership persistence carrier

- Added `UnitPartBatteredFirearmOwnership` on the same player-owned save graph
  as firearm state. Its versioned records serialize only primitive item GUID
  and originating-unit identity strings; no runtime item or unit references.
- The carrier validates every read/write, rejects unsupported/null/duplicate
  records and immutable-owner conflicts, and atomically publishes new records.
- The provider reuses the exact established main-character resolver and creates
  the part only for writes. Focused contract, repository validation, 844 tests,
  clean Release build, and strict packaging passed.
- Package/DLL hashes: `8608b007...d22e7` / `ef2ba51d...f5c61`.
- Next: commit, then patch exact `LevelUpHelper.AddStartingItems` with before/
  after item-reference correlation and bind only one newly granted Early Pistol.

## 2026-08-02 Sprint 87 exact starting-firearm binder

- Added an exact Harmony prefix/postfix for native
  `LevelUpHelper.AddStartingItems(UnitDescriptor)`. It activates only for the
  registered Gunslinger maximum class, snapshots inventory references, and
  requires exactly one newly created exact production Early Pistol.
- The postfix reads the engine-issued item GUID and attached descriptor unit ID,
  then writes the immutable save-owned binding. It never uses the wielder,
  inventory order, value equality, or a pre-existing pistol.
- Added exact-owner removal for item lifecycle/test rollback. Wrong-owner
  removal fails without mutation; the domain suite is now 845 tests.
- Focused contracts, repository validation, clean Release build, and strict
  package validation passed. Package/DLL hashes are `690967f5...fbdb61` /
  `2bf60b7d...b15068`.
- Next: commit, exact mod-load qualify the patch target, then integrate the
  effective-condition overlay into firearm action/attack resolution.
- Exact `ff2f8c5` mod-load PASS `20260802T1349030296324Z` proves the installed
  `AddStartingItems` Harmony target resolves and all patches install.

## 2026-08-02 Sprint 88 runtime-use resolver

- Added the read-only runtime adapter from exact item GUID plus exact user unit
  ID to the qualified battered-use policy. Unbound items remain ordinary;
  bound items use only persisted origin ownership.
- The adapter does not create a UnitPart during evaluation and fails closed on
  missing exact item/user identity. Next: qualify and commit, then inject its
  effective condition into firing/action boundaries without mutating actual
  item condition.
- Focused contract, repository validation, 845 tests, clean Release build, and
  strict package validation passed. Package/DLL hashes are
  `b3ecebf5...ef3a3` / `743e9ba2...618c4`.

## 2026-08-02 Sprint 89 battered use boundaries

- Shared equipped-firearm resolution now carries the battered effective
  condition separately from actual item state. Reload evaluates a transient
  effective copy; maintenance continues to use actual persisted condition.
- Native discharge resolves exact item/user ownership and rejects effective
  Wrecked use without consuming a chamber or mutating actual state.
- Extended discharge results with explicit effective condition and added a
  regression for actual Broken/effective Wrecked preservation.
- Repository validation, 846 tests, clean Release build, and strict packaging
  passed. Package/DLL hashes: `71261278...38eb0` / `e50d6a39...604b4`.
- Next: commit/mod-load, then propagate effective Broken into misfire threshold
  and remaining deed use gates without changing maintenance semantics.
- Exact `0410d21` guarded Steam mod-load PASS is
  `20260802T1402557908977Z-mod-load-smoke`; rebuilt package/DLL hashes are
  `51f9a32e...b39ca` / `1c9359b3...32a1c`. Sprint 89 is runtime-qualified.

## 2026-08-02 Sprint 90 battered misfire overlay

- Propagated effective battered condition from exact discharge into the
  short-lived misfire context. Gun Training threshold calculation now sees the
  effective condition rather than silently using persisted actual condition.
- Extended the immutable condition decision to prove actual and effective
  inputs separately. Early actual-Normal/effective-Broken misfires use the
  existing destructive `BrokenToWrecked` and burst contract; advanced firearms
  preserve actual state as established for advanced Broken use.
- Focused cases plus repository validation, all 848 domain/reflection tests,
  clean Release build, and strict package validation pass. Package/DLL hashes:
  `bd216cb3...f73a8` / `448824f7...624f5`.
- Next: commit/mod-load, then convert remaining deed use gates from actual to
  effective condition while preserving maintenance-only actual semantics.
- Exact `b0fa6ad` guarded Steam mod-load PASS is
  `20260802T1408301519777Z-mod-load-smoke`; rebuilt package/DLL hashes are
  `5b59b009...2d5de` / `de62f372...118967`. Sprint 90 is runtime-qualified.

## 2026-08-02 Sprint 91 battered deed use gates

- Audited every deed path using the shared exact equipped-firearm resolver.
  Pistol-Whip, Lightning Reload, and all four Dead Shot condition boundaries
  now use the shared effective battered condition.
- Preserved actual repository state for all atomic commits. Quick Clear remains
  actual-condition maintenance by design and cannot repair an ownership-only
  effective Broken overlay.
- Focused contract, parser validation, repository validation, all 848 tests,
  clean Release build, and strict package validation pass. Package/DLL hashes:
  `4b8e94f4...415f6` / `2e10d064...03938`.
- Next: commit/mod-load, re-audit all battered/economy coverage, and continue
  the highest-priority incomplete mandatory row.
- Exact `8d2e5b6` guarded Steam mod-load PASS is
  `20260802T1412373661267Z-mod-load-smoke`; rebuilt package/DLL hashes are
  `7c416c64...54588` / `6ac5c281...caadd`. Sprint 91 is runtime-qualified.

## 2026-08-02 Sprint 92 battered sale value

- Re-audited installed item/vendor metadata and selected exact
  `VendorLogic.GetItemBuyPrice(ItemEntity)`, the player-to-vendor instance sale
  calculation. Vendor-to-player and blueprint prices remain native.
- Added a per-item postfix that returns the authorized fixed 22 gp only for an
  exact GUID present in the persisted battered ownership carrier. Missing or
  ordinary identity preserves the native result.
- Focused contract/parser checks, repository validation, all 848 tests, clean
  Release build, and strict package validation pass. Package/DLL hashes:
  `a07775c5...457fe` / `4f15f985...1ed04`.
- Next: commit/mod-load, then update the Gunsmith/battered matrix row and audit
  the next mission gap.
