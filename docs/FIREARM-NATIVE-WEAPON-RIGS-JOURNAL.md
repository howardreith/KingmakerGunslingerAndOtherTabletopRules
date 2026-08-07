# Firearm native weapon rigs journal

Entries are append-only in spirit.

## 2026-08-07T03:18:53.3765828Z - Baseline isolation

- Branch/commit before experiment: `master` at
  `2d9d95c8b0f919fb5f129c783522608bc47e2029`.
- Question: can the qualified Wwise source baseline be established without
  disturbing unrelated work, and can the required mission branch be isolated?
- Inspected: repository `AGENTS.md`, Git status/branches/log/ancestry/worktrees,
  required autonomous handoffs and reports, Wwise mission/journal/report,
  firearm asset/presentation/projectile/equipment sources, production/test
  blueprint presentation, development UI/controls, runtime runner/catalog,
  Unity builder and preparation/build/package validators, bundle manifest, and
  source-model modification/provenance records.
- Files changed: the five initial native-rig durable documents and the leading
  mission state in `AUTONOMOUS-RESUME.md`.
- Commands: `git status --short --branch`; `git branch --show-current`;
  `git rev-parse HEAD`; `git log -12 --oneline --decorate`;
  `git merge-base --is-ancestor 2d9d95c... HEAD`; `git worktree list
  --porcelain`; `git worktree add -b codex/firearm-native-weapon-rigs
  .worktrees/firearm-native-weapon-rigs 2d9d95c...`; targeted `Get-Content` and
  `rg` inspection.
- Evidence: ancestry exit `0`; clean original checkout; isolated worktree on
  `codex/firearm-native-weapon-rigs`; baseline rig-manifest SHA-256
  `326E3B59A0FF869D8BA570F2A01C5D6137F828CC3FAA652CC9191309779B219D`.
- Result: **pass**. Exact expected source SHA is available and clean. Initial
  non-escalated branch creation failed with a Git ref-lock permission denial;
  the approved Git operation then succeeded without modifying source history.
- Meaning: mission can proceed safely from the qualified Wwise checkpoint.
- Next action: validate and commit the durable checkpoint, publish with the
  policy script, then inspect exact installed Kingmaker rig contracts.

## 2026-08-07T03:24:00Z - Initial checkpoint validation

- Branch/commit before experiment: `codex/firearm-native-weapon-rigs` at
  `2d9d95c8b0f919fb5f129c783522608bc47e2029`.
- Question: do the untouched qualified source and new durable mission documents
  pass every pre-commit source, domain, exact-reference Release, and package
  gate?
- Inspected: repository validators, all dependency-free domain/reflection tests,
  qualified private-reference build, output/package validators, produced DLL and
  local-runtime package.
- Files changed: journal and resume evidence only after validation.
- Commands: `.\scripts\validate-repository.ps1`;
  `.\scripts\test-domain.ps1 -Configuration Release -Clean` (sandbox attempt
  and approved rerun); `.\scripts\Build-Local.ps1`.
- Evidence: repository validation PASS; initial domain run passed 897 tests but
  `audio.staging-lifecycle` received `UnauthorizedAccessException` at atomic
  `File.Replace`; unchanged approved rerun PASS 898/898; clean Build-Local PASS,
  including repeated repository/domain validation, exact-reference compile,
  build-output validation, SoundBank validation, strict package validation;
  local-runtime package SHA-256
  `86192DD3383C51CEB60A5D00A49194CAA33FB29BC31FC6AF29CFEFFED2BE6B98`;
  DLL SHA-256
  `D664EF718C90C7582644E764039BEBD092AFE406C8513F4947A4349D77B6F1AF`.
- Result: **pass**. The first failure was sandbox-only and disappeared on the
  identical authorized rerun; no code workaround was introduced.
- Meaning: the durable mission checkpoint is safe to commit. This build proves
  only the unchanged baseline and does not qualify any new firearm rig.
- Next action: commit/push, then inspect exact private-reference signatures and
  native donor contracts.

## 2026-08-07T03:30:00Z - Publication routing and exact metadata forensics

- Branch/commit before experiment: `codex/firearm-native-weapon-rigs` at
  `da916fbc74a3b48dc0d5e8cc43c5dad867b633d0`.
- Question: did policy publication reach this branch, and which exact installed
  native rig members exist?
- Inspected: exact policy script; installed `Assembly-CSharp.dll` through
  read-only reflection metadata; existing source presentation/builder/runtime
  surfaces.
- Files changed: forensics, journal, structured Unity builder, and current
  version source validator.
- Commands: exact mandated policy publisher; read-only `Get-Content` of policy;
  narrow assembly string search; broad reflection attempt; narrow
  reflection-only known-type inspection.
- Evidence: policy output targeted
  `.worktrees/firearm-wwise-audio`, branch `codex/firearm-wwise-audio`, commit
  `2d9d95c...`, leaving `da916fb` local-only. Policy source hard-codes that
  worktree. Broad reflection failed with `StackOverflowException`; narrow
  reflection-only inspection returned the exact members curated in
  `FIREARM-NATIVE-RIG-FORENSICS.md`.
- Result: **ambiguous publication / pass forensics**. The mandated command did
  not publish this mission branch; arbitrary push was not substituted. Exact
  rig metadata is sufficient to proceed safely with structured authoring.
- Meaning: local work may continue, but every checkpoint must continue invoking
  and truthfully recording the policy mismatch until policy routing is updated.
- Next action: validate the declarative rig-specification refactor, then inspect
  native donor objects through the guarded runtime scenario.

## 2026-08-07T03:36:00Z - Declarative rig builder checkpoint

- Branch/commit before experiment: `codex/firearm-native-weapon-rigs` at
  `da916fbc74a3b48dc0d5e8cc43c5dad867b633d0`.
- Question: can all eight current bundle prefabs be built from reviewable
  per-prefab specifications with identity roots, explicit Muzzle points, exact
  long-gun support-target hierarchy, and deterministic bytes?
- Inspected: preserved five source-model families, exact Unity editor version,
  staged prefab/material hierarchy, generated AssetBundle and build logs.
- Files changed: `tools/unity/BuildFirearmBundles.cs`, current source validator,
  bundle manifest, forensics/journal/resume documents.
- Commands: repository validation; `Prepare-UnityAssets.ps1`; exact Unity
  2018.4.10f1 `BuildFirearmBundles.BuildBatch` runs using hidden batch process;
  SHA-256 comparison after independent restaging.
- Evidence paths (machine-local, not committed):
  `C:\Dev\KingmakerGunslingerLab\unity-asset-build\native-rigs-build.log`,
  `native-rigs-build-2.log`, `native-rigs-build-3.log`. Exact editor reported
  `2018.4.10f1 (a0470569e97b)`. Two independently restaged outputs matched at
  SHA-256
  `88DF971967ECF4879BAA93FE79A734D46ABA2A754AEBD193FAE01AB756DCFD91`.
- Result: **pass** for deterministic structural authoring. An initial shell
  wrapper returned before Unity exited; the process completed successfully and
  its log was retained. Synchronous hidden `Start-Process -Wait` then established
  the matching hash.
- Meaning: builder structure is reproducible. The existing transform values are
  explicitly labeled calibration/source-unit work, and no production profile is
  enabled; this does not prove EquipmentOffsets assignment or visual quality.
- Next action: run source/domain/Release/package gates with the new bundle, then
  implement runtime preparation/capability and native donor observation.

## 2026-08-07T03:41:00Z - Structured bundle source qualification

- Branch/commit before experiment: `codex/firearm-native-weapon-rigs` at
  `da916fbc74a3b48dc0d5e8cc43c5dad867b633d0` plus the declarative builder diff.
- Question: does the deterministic structural bundle pass every repository,
  domain, exact-reference Release, build-output, and strict package gate?
- Inspected: full 898-test catalogue, exact-reference DLL, packaged bundle and
  manifest, SoundBank, final local-runtime archive.
- Files changed: evidence in journal/resume only after validation.
- Commands: `.\scripts\test-domain.ps1 -Configuration Release -Clean` and
  `.\scripts\Build-Local.ps1`.
- Evidence: repository validation PASS; complete suite PASS 898/898 (twice,
  including Build-Local); exact-reference compile, output validation, SoundBank
  validation, strict standalone and local-runtime package validation PASS;
  package SHA-256
  `4BD296BEC867BC6B44496A07826224DEBF9F61EB3F41CA6F2028BABC9ECAC7A3`;
  DLL SHA-256
  `6BBA65B3C22332B6DB8B2F2420E7412195C562B72F45AC7A39A2F4F548A905AB`;
  bundle SHA-256
  `88DF971967ECF4879BAA93FE79A734D46ABA2A754AEBD193FAE01AB756DCFD91`.
- Result: **pass**. No deployment or Kingmaker launch occurred.
- Meaning: structured rig authoring is source-qualified, but equipped readiness
  remains native fallback and native IK/runtime preparation remains pending.
- Next action: commit this checkpoint, invoke policy publisher, then implement
  fail-closed runtime rig validation/EquipmentOffsets preparation and donor
  observation.

## 2026-08-07T04:05:00Z - Runtime rig preparation and donor observer

- Branch/commit before experiment: `codex/firearm-native-weapon-rigs` at
  `1b2da2687aefe4487cafa49403aaf1cfd67ee732`.
- Question: can bundle rigs be validated and long-gun left-hand IK assigned
  transactionally before publication, while a guarded save-free observer records
  exact native Light/Heavy Crossbow rig structure without enabling custom art?
- Inspected: installed `EquipmentOffsets` contract; runtime bundle loader;
  presentation profile; runtime scenario catalog/dispatch/preflight; native donor
  blueprint identities and transient model lifecycle.
- Files changed: asset runtime, presentation readiness, runtime catalog/runner,
  automation metadata/preflight, focused domain tests, inherited/current
  validators, and authoritative static test count.
- Commands: repeated `.\scripts\test-domain.ps1 -Configuration Release -Clean`;
  `.\scripts\Test-RuntimeScenarioPreflight.ps1`; `.\scripts\Build-Local.ps1`.
- Evidence: first source run failed before compilation because inherited
  validation expected 877 declared tests; the validator was parameterized while
  retaining 877 as its historical default. Second run exposed the matching
  authoritative static count and it was updated. Final repository validation
  PASS; runtime preflight PASS 86; complete suite PASS 901/901; exact-reference
  compile/output/SoundBank/strict packages PASS; package SHA-256
  `569B283BD8B40EF57232C5DB05C9EA37DBA4C08217B57425A15E2BE79A43C36B`;
  DLL SHA-256
  `EB5AE4153CC0ACD34FEE894F885940CCFB1934FED856E6FB4062F4246923DBCD`;
  bundle remains
  `88DF971967ECF4879BAA93FE79A734D46ABA2A754AEBD193FAE01AB756DCFD91`.
- Result: **pass source qualification**. No game launch occurred yet.
- Meaning: each equipped prefab now fails independently, capability publication
  is transactional, long guns receive exact native `IkTargetLeftHand`, and all
  production profiles remain `NativeFallback`. Live donor/capability evidence is
  the next required gate.
- Next action: commit/push, run exact-commit `mod-load-smoke`, then run
  `observe-native-firearm-rig-contracts` and use its structured evidence to
  refine the Musket proof.

## 2026-08-07T04:06:00Z - Guarded donor and capability qualification

- Branch/commit before experiment: published
  `8bdb40b65c24b271f60361b492e559523e595e17`.
- Question: does the exact deployed commit load through Steam, expose the native
  donor rig contract, prepare all five custom capabilities, preserve fallback,
  and clean up without save mutation?
- Inspected: exact deployed package/DLL/bundle identity, UMM version/commit,
  native Light/Heavy Crossbow transient models, custom runtime capabilities.
- Files changed: curated forensics/journal/resume/report only after runs.
- Commands: guarded `mod-load-smoke`; guarded
  `observe-native-firearm-rig-contracts`, both expected version `0.0.70`,
  automatic exit, no confirmation UI.
- Evidence: mod-load run
  `20260807T0403244410281Z-832f57f82d554f5ca257b31bf88b4012`, result SHA-256
  `CFBE0EC292B6589E9F3AD5496B744A77F51240C01741813F8AFB65C9B8892F78`,
  PASS; donor run
  `20260807T0405003890595Z-8623a3dc21f84769bde130d434879a37`, result SHA-256
  `6CC73E253B48ADDBB0DE6D59867A7AEF82083914469D8281D978759BDA995BDE`,
  PASS. Exact observations are in `FIREARM-NATIVE-RIG-FORENSICS.md`.
- Result: **pass**. All five custom capabilities prepared; all five production
  profiles stayed `NativeFallback`; transient donors cleaned; no save used.
- Meaning: the native IK architecture is proven present and compatible with the
  custom hierarchy. Human pose quality remains wholly unproven.
- Next action: implement the development-only session calibration model/UI and
  native hands-equipment refresh, then enable Musket only after its candidate
  loaded-unit scenario passes.

## 2026-08-07T05:05:00Z - Session calibration model and live instance adapter

- Branch/commit before experiment: `codex/firearm-native-weapon-rigs` at
  `c7fda25e62ec3a231127492a23430dd32713e9c8`.
- Question: can calibration be isolated per firearm, applied only to a unique
  selected exact-firearm candidate instance, exported deterministically, and
  reset without save or shared-native mutation?
- Inspected: existing reflection-only UMM GUI, exact firearm resolver, validated
  rig capabilities, selected-unit API, instantiated `EquipmentOffsets` contract.
- Files changed: new calibration state/runtime/UI, development panel/project
  registration, focused source tests, current test-count validator surfaces.
- Commands: `test-domain.ps1 -Configuration Release` (sandbox-only temp replace
  denial after 900 passes); `Build-Local.ps1` (same denial); escalated
  `Build-Local.ps1` (901 tests passed, compile rejected one unused field);
  corrected field; final escalated `Build-Local.ps1`.
- Evidence: repository validation PASS; complete suite PASS 904/904;
  exact-reference Release, build-output, SoundBank, strict package PASS; package
  SHA-256 `4A2977ACA90AA5DAE2A27C55F82249374CD41EA81A2875D4363A236D6AFC2C7A`;
  DLL SHA-256 `BA16B74BF575E6A3D641DDF95A05596EB0C378CC790A7E15EFC484FBD1B01425`.
- Result: **pass source qualification**. No runtime launch and no profile
  readiness change occurred.
- Meaning: deterministic finite session calibration, coarse/fine transform
  controls, exact active-instance filtering, native IK verification, resets,
  allowlisted animations, and `humanAccepted=false` export now exist. Native
  equipment refresh/toggle, doll refresh, markers, and belt/projectile controls
  remain incomplete and are not represented as passing.
- Next action: commit/push, then add exact native hands-equipment refresh and
  candidate toggle before the Musket disposable loaded-unit proof.

## 2026-08-07T05:20:00Z - Reversible native hands-equipment refresh

- Branch/commit before experiment: published `1e98aadc97ee3481b3893dfabca319642b8066a2`.
- Question: can the selected project-owned firearm switch between candidate and
  its preserved non-null native model through the exact world-view lifecycle?
- Files changed: calibration runtime/UI, focused test, validator counts.
- Command: escalated `Build-Local.ps1` for disposable Windows temp access.
- Evidence: repository validation PASS; suite PASS 905/905; exact-reference
  Release and strict package PASS; package/DLL SHA-256
  `5AD68BE85A3AEAA2B53EF299C0AE4A7D2B4F4C74F8F1EC6883CFBACE2539458D` /
  `BD24338674025E3B9ED4CE5AEBA1B5B597AFFCAC91A4EE9F93C93A7A42026466`.
- Result: **pass source qualification**. World refresh uses
  `UnitViewHandsEquipment.UpdateAll`; the original model is retained per kind
  and restored explicitly. Doll refresh remains unavailable/fail-closed.
- Next action: create and qualify the disposable Musket visual-rig scenario.

## 2026-08-07T05:40:00Z - Musket autonomous-candidate source gate

- Branch/commit before experiment: published `f9a799283564c6d0b96d1f78e66abc1c7fa4b924`.
- Question: can Musket alone advance to `AutonomousCandidate` with a guarded
  save-free structural/IK/projectile/cleanup scenario and explicit visual gate?
- Files changed: Musket readiness, scenario catalog/runner/automation/preflight,
  focused test and validator counts.
- Commands: runtime preflight (first fingerprint assertion transiently changed,
  immediate isolated rerun PASS 86); `Build-Local.ps1` (first compile found
  missing Assets namespace after all 906 tests passed); corrected import; final
  `Build-Local.ps1`.
- Evidence: repository validation PASS; suite PASS 906/906; exact-reference and
  strict package PASS; package/DLL SHA-256
  `2DEBECF19A3D44D246FA52E94632881BE229102BB0A0B32E39005D405248D2F6` /
  `3CD68BBC38A55DE491000CBBE648B858958472D02927379832C54C0E27807E37`.
- Result: **pass source qualification**; runtime result pending published commit.
- Meaning: Musket is structurally eligible for live candidate review, never
  `HumanAccepted`. Grip, clipping, pose, scale and animation remain human gates.
- Next action: commit/push, then guarded smoke and visual-rig scenarios.

## 2026-08-07T06:25:00Z - Published Musket runtime qualification

- Branch/commit before experiment: published
  `8f2ba17aeb9da6b2f9ae1786475a5b8d96b69b97`.
- Question: does the exact deployed commit load and instantiate the Musket
  candidate with identity hierarchy, renderer, exact native left-hand IK, one
  logical projectile, explicit human gate, and deterministic cleanup?
- Commands: guarded Steam `mod-load-smoke`; guarded Steam
  `disposable-firearm-visual-rigs`; both version `0.0.70`, save-free, automatic
  exit.
- Evidence: smoke run
  `20260807T0421190993099Z-53e862198b0343ddb4327a2b79d481b2`, result SHA-256
  `BBE9AB631A7D2BE2118875E69ADA4E03515A72E52A0F937FEC2FED2567C7D611`;
  rig run `20260807T0422526272729Z-a25328bd2f3345079a5589ae59a48c6b`,
  result SHA-256
  `97321FDD49D6D361F854FECA174DF7120A7DBEC963F38E4E322296B723C6413D`.
- Result: **pass** for both.
- Meaning: Musket is structurally/mechanically qualified as
  `AutonomousCandidate`. Human grip, clipping, scale, pose, animation timing and
  support-hand quality remain untested and must not be called accepted.
- Next action: extend the same independently calibrated architecture and guarded
  assertions to Blunderbuss and Rifle, preserving Scatter and other mechanics.

## 2026-08-07T06:45:00Z - Remaining long-gun source qualification

- Branch/commit before experiment: published `2513c464dfff09d96c14afc0f0d3e11782de5105`.
- Question: do Blunderbuss and Rifle independently satisfy the same exact
  long-gun hierarchy, native IK, projectile and cleanup gates as Musket?
- Files changed: two readiness profiles, generalized per-kind runtime assertions,
  focused test and validator counts.
- Commands: `Build-Local.ps1` three times: first found a stale Musket source-test
  anchor after 907 tests; second found exact catalog member is `AdvancedRifle`;
  final PASS.
- Evidence: repository validation PASS; suite PASS 907/907; exact-reference and
  strict package PASS; package/DLL SHA-256
  `AE968B489B2E344C536C0046136A158462FE7F8717AAA291EF871E28AA4DB7AE` /
  `FE093C41175FC42F1BD0657107476F1BAD67F8F34484A95934C4612E1B77BDF1`.
- Result: **pass source qualification**; guarded runtime pending published commit.
- Next action: commit/push, then rerun `disposable-firearm-visual-rigs`.

## 2026-08-07T07:00:00Z - Pistol and Revolver source qualification

- Branch/commit before experiment: published `8794e668daa5a50c639157beab57c7ef0b1e7147`.
- Question: do independently authored Pistol/Revolver rigs satisfy identity,
  muzzle, renderability, no-support, projectile, cleanup and allowlisted
  `PiercingOneHanded` candidate requirements?
- Files changed: short-gun readiness/animation, generalized runtime assertions,
  focused test and validator counts.
- Commands: `Build-Local.ps1` (first compile found missing exact animation
  namespace after 908 tests); corrected import; final `Build-Local.ps1` PASS.
- Evidence: repository validation PASS; suite PASS 908/908; exact-reference and
  strict package PASS; package/DLL SHA-256
  `9B56A87F4122D72DD699B7AD4A874F83D33DFE4084F3AD7CE6FA1C7135FF3F8B` /
  `CC20FB243D19C35208B746E388B55D6491F949CA3A0B6144C8BAA38D25E2667E`.
- Result: **pass source qualification**. The animation is a mechanically testable
  candidate, not a human visual verdict.
- Next action: commit/push and run the all-five guarded structural scenario.

## 2026-08-07T07:20:00Z - Retire whole-character renderer scan

- Branch/commit before experiment: published `c36474d202b69f5d40dd303c0750321a11a0bb4f`.
- Question: can the obsolete name-based whole-character quiver/crossbow renderer
  scan be removed completely without breaking source/package qualification?
- Files changed: deleted handler, project include removed, historical validator
  converted to a retirement guard, focused test/counts.
- Command: `Build-Local.ps1`.
- Evidence: repository validation PASS; suite PASS 909/909; exact-reference and
  strict package PASS; package/DLL SHA-256
  `655B4C2A59DF09A689A5C49A70B818650DFBD1276BB5A99A2982D1D3331B94AB` /
  `395278EA216126828FC361C126FBBD1C0AB87FB6459323509910DF3A69112D2D`.
- Result: **pass**. The dangerous renderer scan is absent and cannot be compiled.
- Next action: commit/push, rerun all-five rig runtime, then frozen regressions.

## 2026-08-07T08:45:00Z - All-five published runtime and frozen regressions

- Branch/commit before experiment: published `54eeeea460844e66d1fff286b0b494ceeb27e6a2`.
- Question: do all five candidate rigs and frozen Wwise/projectile/Scatter/reload/
  switching contracts pass on consecutive fresh guarded Steam launches?
- Commands: `disposable-firearm-visual-rigs`, `disposable-firearm-wwise-audio`,
  `disposable-gunslinger-scatter-shot`, `disposable-gunslinger-targeting-arms`,
  `disposable-reload-autocast`, `disposable-production-firearm-switching`.
- Evidence: all PASS. Run/result SHA-256 pairs:
  - rigs `20260807T0434551954973Z-4133c90579c64263a335b8c204cf324c` /
    `83ABC30BAC60F8A7421A57F4BFD4D0997F5DE4A53420E1782A5218483C9ADED2`;
  - Wwise `20260807T0436296097402Z-6e9bf6f1c99d40178aa87dcf83503ce0` /
    `39B0A0C04686F89BCB2F87E78E6D0C4EBA6E7DCA07436BE1EB46BF224F5CB048`;
  - Scatter `20260807T0438026145108Z-9ebe4496e4564c6abcddc40009259765` /
    `5B7AA2628DB301EA9662612631357D70F76D599A1675852F2FA77EF44F126AE9`;
  - Targeting Arms `20260807T0439386747600Z-42a04e5453d541ce9ad2384333ee26f7` /
    `D3F2AC530EEDEACF4A1B143E3B8FC5F472C439CAF0527C2AB037A29D21749167`;
  - reload `20260807T0441122052582Z-911155a36df8478fae5987f4f7f7fc54` /
    `B8DAE899E2AC41F4C06FB246BF379F1B828097B9909CA86E2AF51D8FCD41B5AA`;
  - switching `20260807T0442452610862Z-0f5acf42894c424aafbdb68db55d1c3f` /
    `9AA564F9B3AB1208E5D8BEF065FB547931AD54AC9CA2550A423913FF4A792A60`.
- Result: **pass**. No save was used or written. These are structural/mechanical
  results only; appearance remains unaccepted.
- Next action: exact slot-scoped sheath/quiver lifecycle and doll refresh.

## 2026-08-07T09:05:00Z - Exact hidden-holster policy

- Branch/commit before experiment: published `7c6f31b07493118c6be3a255968b5f5d97aa8b8e`.
- Question: can inherited crossbow belt/sheath presentation be suppressed on
  exact project-owned candidate visual parameters without renderer scans or
  native donor mutation?
- Files changed: readiness holster policy, weapon presentation, runtime
  assertions, focused test/counts.
- Command: `Build-Local.ps1`.
- Evidence: repository validation PASS; suite PASS 910/910; exact-reference and
  strict package PASS; package/DLL SHA-256
  `0756F8F78111FAB171F1FD8B77BBFE1A68DB9D71CEEDA89D785C5E2BAF56DEC7` /
  `B0B775467597C716662FCC916B179A96A118AA530870ABAAD5617830F2B6BEBC`.
- Result: **pass source qualification**. All candidate firearms explicitly use
  hidden belt/sheath models; native crossbow objects are unchanged.
- Next action: commit/push and rerun all-five guarded rig scenario.

## 2026-08-07T09:25:00Z - Candidate version 0.0.71

- Branch/commit before experiment: published `602e48998171cf10c3a474648dff8e57ae97ec07`.
- Question: can every authoritative version/build/validator surface advance
  exactly one patch to the independent native-rig candidate identity?
- Files changed: version properties, UMM metadata, active build/runtime guards,
  0.0.71 validator/dispatch, UI label, changelog.
- Commands: repository validation (first found retained historical Dodge label
  token); corrected additive label; runtime preflight; `Build-Local.ps1`.
- Evidence: preflight PASS 86; repository validation PASS; suite PASS 910/910;
  exact-reference and strict 0.0.71 package PASS; provisional package/DLL
  SHA-256 `525C5CE6567D7E987FBBD89D388C33D7B6435DA03A8120D0D996E165A1B483FF` /
  `D9E0C48263B800DCD75D53FB1BEB60E65AB5C1324DA1B6C3357FA822930FAE80`.
- Result: **pass source qualification**. Exact published-commit rebuild/runtime
  remains pending.
- Next action: commit/push, rebuild, then critical scenarios twice.

## 2026-08-07T10:05:00Z - Final published 0.0.71 qualification

- Branch/commit before experiment: published `3ae6b5d903720dbd450a2bb3fa82ed32d0b14c4d`.
- Question: does the exact independent candidate pass repeat structural, Wwise,
  projectile/damage and authorized working-save gates without save writes?
- Commands: exact `Build-Local.ps1`; two consecutive
  `disposable-firearm-visual-rigs`; `disposable-firearm-wwise-audio`;
  `disposable-gunslinger-targeting-arms`; `working-save-smoke` naming only
  `KMG_AUTOMATION_WORKING`.
- Evidence: package/DLL SHA-256
  `9F905766214BEB2AC23E2519525826B14970FA7CDE32D305BD8D4E9D2452DF2D` /
  `479244B41883256831396E60FFCC9CFD06E6F40544AF6E8185D0785831D5000C`.
  Runtime PASS run/result hashes:
  - rig 1 `20260807T0452453368618Z-108bd4df764c4c948b1baf7c72619537` /
    `872B3D0310C6A1EC214EC03AC94400B2DC3FFFD4D544A4E1635D7EE918806933`;
  - rig 2 `20260807T0454196627467Z-0f6629e8e8fd4f2f924c1d4da64cc130` /
    `8D726E15965092F15DE782FA11E497394605C97995D6E7D35BA6ED869438478A`;
  - Wwise `20260807T0455538086836Z-aa2b2e869bd9435c9510a2c64e19b4ee` /
    `FB1FEA2D073B373541AD599AF46C72D13DB56F06BA45882A7345D303936B59D4`;
  - Targeting Arms `20260807T0457299033010Z-53ece3c653eb43e1b71ad1913c0661e9` /
    `492D9B02C09F39A22BD53205656AC12C15515A571EF5968934EECE78E29DF0EE`;
  - working save `20260807T0459208536053Z-7621757d095c4a6a89273a06c4585d69` /
    `B223E4046BDE847D912630FA0AC1F33A6991AD7AA217B00433A9DD68C9152E6B`.
- Result: **pass**. Protected baseline was not selected or overwritten; no
  save-writing API was observed. Human appearance remains entirely unaccepted.
- Next action: supervised Musket-first checklist after remaining lab diagnostics
  are completed, or use current candidate strictly as structural review build.

## 2026-08-07T16:30:00Z - Targeted visibility and pistol orientation repair

- Branch/commit before experiment: `codex/firearm-native-weapon-rigs` /
  `cf03ba8fd4834153fe03d8008a66cd7592c44950` (published).
- Question: can partial held Musket/Blunderbuss disappearance and inverted
  Pistol presentation be corrected without changing the working grip, support
  hand, muzzle, animation, or belt calibration?
- Inspected assets: exact Unity 2018.4.10f1 staged Musket, Blunderbuss, and
  Pistol models/prefabs; every renderer, mesh, normal count, material, shader,
  enabled state, and hierarchy scale.
- Evidence: `native-rigs-visibility-repair.log`,
  `native-rigs-double-sided-repair.log`, and
  `native-rigs-double-sided-repair-2.log` under the approved Unity build root.
  Musket had zero LODGroups, two enabled MeshRenderers with normals
  21526/21526 and 4717/4717; Blunderbuss had zero LODGroups and one enabled
  MeshRenderer with normals 1444/1444. No zero, negative, mirrored, or nonfinite
  hierarchy scale was found. Standard shader exposed no usable `_Cull`
  property, so that experiment was rejected as ineffective.
- Change: retain/remove LOD metadata defensively, fail closed on bad scales,
  emit exhaustive renderer diagnostics, use held-long-gun-only
  `KingmakerGunslinger/DoubleSidedDiffuse` (`Cull Off`), and roll only Pistol's
  Visual child by 180 degrees. Held rig and belt values are otherwise unchanged.
- Result: **pass deterministic structural build**. Two clean exact-Unity builds
  produced AssetBundle SHA-256
  `62BAB35C9DEB94AE98B61CD8B56CA523CC946A740248C06B63E8E41A94AE7CDD`.
  View dependence plus the eliminated alternatives makes backface culling or
  source normal orientation the supported cause. Human camera-angle review is
  still required.
- Next action: full repository/package qualification, publish, then guarded rig
  and frozen-contract runtime regressions.

## 2026-08-07T16:50:00Z - Visibility repair source/package gate

- Branch/commit before experiment: `cf03ba8fd4834153fe03d8008a66cd7592c44950`.
- Command: `scripts/Build-Local.ps1`.
- Result: **pass** repository validation, 911/911 domain/reflection tests,
  exact-reference Release build, build-output validation, and strict package
  validation. Provisional package/DLL SHA-256:
  `AA54406BEFF23A872FB3B234EA86097A4A5090E33AC6CEBAD73BCC346F8B6C91` /
  `FF5822BF94F93EC7D24ACBCAB6ED22EC5FD9AED9ECAB61B9639C5CE0D11BCA6D`.
  Rig manifest SHA-256:
  `429A4E7A30553C016EFEEA95951598164D6F7A4930218A64977EA7DEBD2C2B7F`.
- Meaning: source and package structure are safe; a published-commit rebuild
  and guarded runtime qualification remain required. Appearance is unaccepted.
- Next action: commit, push through policy helper, rebuild exact published
  identity, then run the visual-rig and frozen-contract scenarios.

## 2026-08-07T17:30:00Z - Published targeted-repair runtime qualification

- Published implementation: `d7b6bc1756ae89f5e043c5b3362a46e8fe614e8f`.
- Exact package/DLL SHA-256:
  `6B3E85517C945B7CB6096E83C2946706749B91C142FA5C7412044EBDD5A03D81` /
  `B1C181740DF76179B145D5C9A03B420DADDB71E6AA938445FDBAA5351660CE5F`.
- Commands: guarded Steam launches for `disposable-firearm-visual-rigs`,
  `disposable-firearm-wwise-audio`, and
  `disposable-gunslinger-targeting-arms`, all version `0.0.71`.
- PASS run/result SHA-256:
  - `20260807T1221537641919Z-disposable-firearm-visual-rigs` /
    `87FF14C30A7D890F3F75FBB33E20E7E58B3498C08CE38E30827EF68A04A44A9E`;
  - `20260807T1223275298998Z-disposable-firearm-wwise-audio` /
    `0CA12AB9D5CAFD2312EB82D80CDC9F34AB34923D79486605B34E1E276A662FC4`;
  - `20260807T1225036369925Z-disposable-gunslinger-targeting-arms` /
    `BF1F490C2334D5BDD05533E6807404810FD1EAD66F71A1574A4012DB39A444BC`.
- Result: **pass structural/mechanical qualification**. Native attachment/IK,
  custom capability, Wwise, logical projectile, and damage delivery remain
  intact. No save-writing API was observed and the protected baseline was not
  selected or overwritten.
- Remaining uncertainty: human review must determine whether Cull Off resolves
  all camera-angle disappearance and whether Pistol is upright with a visually
  correct muzzle. No weapon is HumanAccepted.
- Next action: follow the priority order in the manual acceptance document.
