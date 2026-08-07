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

## 2026-08-07T18:10:00Z - Human-screenshot finishing-pass plan

- Branch/commit before work: `codex/firearm-native-weapon-rigs` /
  `8289af3eaa4fa852ad0ec886d1b7be3f56fe4d49` (published, clean).
- Human evidence: regular Pistol is broadly acceptable but reads like Revolver;
  Advanced Revolver exposes garbage/helper-like geometry; all three custom held
  long guns are effectively invisible; Musket back placement remains bad.
- Frozen baseline: preserve every held grip root, Visual transform, long-gun
  support target, muzzle, and animation unless exact binding/renderer evidence
  requires an isolated correction. Do not solve belt placement through held rigs.
- Plan: (1) add deterministic source-to-runtime prefab identity audit; (2) dump
  all approved hierarchy/components/renderers and remove only proven helper
  geometry; (3) trace long-gun renderer/material/LOD/scale/bounds publication and
  repair the narrow failure, validating runtime visible-renderer capability;
  (4) verify native left-hand IK remains present without retuning it; (5) keep
  long-gun belt models hidden unless a separately proven clean calibration exists;
  (6) run exact Unity determinism, full source/package gates, and guarded rig,
  switching, projectile, Wwise, Scatter, and reload regressions.
- Next action: inspect the exact builder import/spec mapping and staged source
  hierarchies before changing production behavior.

## 2026-08-07T19:00:00Z - Exact binding and renderer-cause audit

- Branch/commit before experiment: `8289af3eaa4fa852ad0ec886d1b7be3f56fe4d49`.
- Exact Unity logs: `finishing-pass-audit.log` and
  `finishing-pass-repair-{2,3}.log` under the approved Unity build root.
- Binding proof: Pistol -> Cyril43
  `Assets/ApprovedModels/Pistol/model/model.dae` GUID
  `a8e1a1da7d7120046b38ce3d758c8eed`; Revolver -> Navy Colt
  `Assets/ApprovedModels/Revolver/source/Final2 Sketchfab.fbx` GUID
  `a60608927a6bcfd458cb2d4b03b93fde`. They are distinct families/prefabs.
- Revolver: removed 53 numeric-suffix duplicate preview objects; retained 51
  unsuffixed low-poly renderers.
- Long-gun evidence: Musket bounds were healthy at `0.6529304 m`; Blunderbuss
  was a `0.024450602 m` speck; Rifle was `1.57617009 m` but centered at
  `z=-0.651` behind the grip. Named fore-end/stock transforms confirmed axis.
- Narrow repair: retire failed custom shader; opaque Standard materials plus
  generated reverse-wound backfaces; Blunderbuss Visual scale `0.5 -> 20`;
  Rifle Visual z `-0.651 -> 0.20`. Musket transform, grip roots, IK targets,
  muzzles, animations, and hidden holsters are unchanged.
- Repaired bounds: Musket `0.6529304 m`, Blunderbuss `0.978023946 m`, Rifle
  `1.57617021 m`. Two forced builds matched at AssetBundle SHA-256
  `4A96CD13152A9EF6B48B3758B697659DCC82BC92D46A97AC8FBAAD815E386B2B`.
- Source/package result: **pass** repository validation, 911/911 tests,
  exact-reference Release, build-output, and strict package validation.
  Provisional package/DLL SHA-256:
  `6A9EE18170CC301BBF4F5F8C0FEA666C7F9D17E19D49520A8462AD659893E3E6` /
  `349E2B788BFD1F1DACADB3510DEE3A5F878E9FC4417EBCADF43A9BF567AD2959`.
- Next action: publish, rebuild exact identity, then guarded presentation and
  frozen-contract regressions.

## 2026-08-07T20:00:00Z - Published finishing-pass qualification

- Published implementation: `fc53c470c94b08265a8a44ce867d7709d7e1003d`.
- Exact candidate package/DLL/AssetBundle/rig-manifest SHA-256:
  `2D7D5A107DF377C1C5BC9D4DCDB693DF5826C390223E14AF789CC03EF34CCE4F` /
  `D8D717C21B24CD8EE1702D979132BB5E2123DD513147D39FD804B48728CF4E1D` /
  `4A96CD13152A9EF6B48B3758B697659DCC82BC92D46A97AC8FBAAD815E386B2B` /
  `60D143952974B8B9039E45B7F4E5B14A7D33294BA89FC336ADCC5CDD7A65571D`.
- Full source/package gate: PASS repository validation, 911/911 tests,
  exact-reference Release, build-output, and strict package validation.
- Guarded Steam PASS run/result hashes:
  - rigs `20260807T1339455160535Z-disposable-firearm-visual-rigs` /
    `1DF5B1AC3DC704D121C84368A9EF0CD72E9FDEB4ACA966626100116378E76A02`;
  - switching `20260807T1341293502697Z-disposable-production-firearm-switching` /
    `6317258CC29C1AC9477CA85287734A03FE08DF034E4611783E578A14491A52A4`;
  - Wwise `20260807T1343125131386Z-disposable-firearm-wwise-audio` /
    `AADCA554CAA1E9AA2A47E4BBAA69CA40D181DCE4FDE387886190AFE8C46DD38D`;
  - Scatter `20260807T1344504827205Z-disposable-gunslinger-scatter-shot` /
    `AB019F0E4583A2450B47E5B190124B7505DA1A1585719DBDD80B44866EE2E943`;
  - projectile/damage `20260807T1346289207404Z-disposable-gunslinger-targeting-arms` /
    `B824F56DD8418BD31D797CB11192577F178F3EDFFF9DBABD8F14DB738D243D44`;
  - reload `20260807T1348052311092Z-disposable-reload-autocast` /
    `DF34FEE571B18B09120415E26447FC7F442BF6EC30F751F312ED0338E0379353`.
- One attempted immediate sequential launch was safely refused because the prior
  Kingmaker PID had not yet exited; subsequent process-synchronized launches
  passed. This was harness sequencing, not a product/scenario failure.
- Result: **AutonomousCandidate structural/mechanical pass**. Runtime-loaded
  renderers are active, opaque Standard, and nontrivially bounded; native IK,
  projectile, audio, Scatter, switching, and reload contracts remain intact.
  Human visibility/pose review is still required; nothing is HumanAccepted.
- Next action: supervised checklist, prioritizing three held long guns, then
  Revolver cleanup and Pistol family appearance. Holsters are intentionally hidden.

## 2026-08-07T21:00:00Z - Semantic-anchor calibration source gate

- Branch/commit before work: published
  `1d9a94da43e712c1b4b0a8eb3f69f9f4a6c30f7b`, clean.
- Human verdict: **Regular Pistol held appearance accepted on 2026-08-07**.
  Frozen source/spec: Cyril43 `Assets/ApprovedModels/Pistol/model/model.dae`;
  prefab `Pistol`; Visual position `(0,0,0.1632)`, Euler `(0,180,180)`, scale
  `0.24`; `PiercingOneHanded`; prior bundle
  `4A96CD13152A9EF6B48B3758B697659DCC82BC92D46A97AC8FBAAD815E386B2B`.
  This accepts only the observed held appearance, not unobserved lifecycle/body
  states and not global `HumanAccepted` readiness.
- Question: can source-space Grip/Support/Butt/Muzzle anchors move the visible
  long-gun grip to the identity root, preserve Crossbow animation, place the
  support target outside the mesh, and give Musket semantic length 1.25-1.45 m?
- Implementation: all held long guns declare four normalized source points;
  Visual position is derived as negative transformed/scaled GripPoint; Butt,
  SupportHandTarget, and Muzzle are derived relative to GripPoint. Disabled
  development marker root names the red/green/blue/yellow points. Calibration
  state now begins from actual prefab Visual/Support/Butt/Muzzle values rather
  than zero and can tune/export Butt independently.
- Exact anchors/results:
  - Musket source grip/support/butt/muzzle `(0.04,0,0)` /
    `(-0.10,-0.0122,-0.0074)` / `(0.0805,0,0)` / `(-0.242,0,0)`;
    runtime-frame butt/support/muzzle `(0,0,-0.169533)` /
    `(-0.030976,-0.051069,0.586040)` / `(0,0,1.180452)`; length `1.349985`.
  - Blunderbuss source grip/support/butt/muzzle `(0.01,0,-0.00316)` /
    `(-0.0125,-0.00255,-0.00471)` / `(0.01565,0,-0.00316)` /
    `(-0.02675,0,-0.00316)`; runtime-frame butt/support/muzzle
    `(0,0,-0.113)` / `(-0.031,-0.051,0.45)` / `(0,0,0.735)`; length `0.848`.
  - Rifle source grip/support/butt/muzzle `(0.13,0,0)` /
    `(-0.1946,-0.0331,-0.0201)` / `(0.503,0,0)` / `(-0.503,0,0)`;
    runtime-frame butt/support/muzzle `(0,0,-0.574472)` /
    `(-0.030957,-0.050979,0.499929)` / `(0,0,0.974908)`; length `1.549379`.
- Evidence: exact Unity logs `semantic-anchor-pass-{1,2,3}.log`. The first
  build exposed stale bundle-cache reuse; only the verified generated bundle
  output was removed, then two clean builds matched at
  `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`.
- Source/package result: **pass** repository validation, 911/911 tests,
  exact-reference Release, build-output, and strict package validation.
  Provisional package/DLL SHA-256:
  `4FA2B04D9658F0A192CFF781C30F94434ABD7C2C316C5A3B4CB051AE3570721C` /
  `ECD2703CD691304DEEAF7D212B47499888E3CA0239B48EE5A7BA3958B7203688`.
- Avatar renderer isolation: the obsolete whole-avatar renderer scan remains
  deleted; no firearm visual code disables body/arm/hand renderers. Actual hand
  bone distances are unavailable in the detached save-free fixture and remain
  a supervised diagnostic/human gate.
- Next action: commit/push, exact-identity rebuild, guarded visual-rig and frozen
  regressions, then human doll/world/attack review.

## 2026-08-07T22:30:00Z - Published semantic-anchor qualification

- Published implementation: `25a585f79a7c0af232c55636aaaaa77d78a4fdee`.
- Package/DLL/AssetBundle/rig-manifest SHA-256:
  `6858AF28C2DDE865BD2575FDEECF6DA11ADACEB0BC6210B1251DEC54239DBC06` /
  `2757835E9086B35481D9F5E06B03DC691BB317B351794BE1B0EDC20442568EA4` /
  `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B` /
  `35BB38BF142D1F1DB3439F4EC328CE7EBF2CFD149318BCEF714A1254CB5301D1`.
- Exact published rebuild: PASS repository validation, 911/911 tests,
  exact-reference Release, build-output, and strict package validation.
- Guarded PASS run/result hashes: rigs
  `20260807T1419038906650Z-disposable-firearm-visual-rigs` /
  `E939B06DEF436F82CEDE45C8A787EB1F685FFC67555F2E27F6C605A6414A9B7B`;
  switching `20260807T1420432406524Z-disposable-production-firearm-switching` /
  `FE2F225B8C2A4EBB21027E730DFE82E9C4435CF7BDD09925F677D311EC2E76F2`;
  Wwise `20260807T1422088088480Z-disposable-firearm-wwise-audio` /
  `0124DD303BF2E8596607C748E91328F42F30465329FD07C63833890CA75A5495`;
  Scatter `20260807T1423345050167Z-disposable-gunslinger-scatter-shot` /
  `A387285F717A784E240313017F27E5941D8138EA6A6CE84B9DFC5F5C72497DD6`;
  projectile/damage `20260807T1425004772302Z-disposable-gunslinger-targeting-arms` /
  `CF866985A6EAA1E27A603A800C8A0B2FB488CF93B4428D617182CFF11000B5E2`;
  reload `20260807T1426268373129Z-disposable-reload-autocast` /
  `769369149D0516DE8F1CA39CFB480F85D570F6970784957DE14173ACBBAEA59B`.
- Result: **AutonomousCandidate structural/mechanical pass**. Pistol freeze,
  semantic ordering/relative length, renderers, IK, animation, switching,
  projectile, audio, Scatter, and reload pass. No protected save was overwritten.
  Doll/world clipping and hand contact remain human-perception gates.
- Next action: supervised inventory doll, peaceful/combat idle, firing/recovery,
  switching, and unequipped review.

## 2026-08-07T23:00:00Z - Held long-gun clipping micro-calibration plan

- Branch/commit before experiment: `codex/firearm-native-weapon-rigs` at
  published, clean `39935dc38d1ec9a7a411eac155b6cb1e4f1989e0`.
- Human baseline: Regular Pistol remains frozen; Musket length `1.349985` m,
  scale `4.186`, Euler `(0,90,0)`, and Crossbow animation are accepted for this
  pass. Blunderbuss scale `20`, Euler `(0,90,0)`, and Crossbow animation are
  also frozen. Only residual held torso clipping is in scope.
- Exact before values:
  - Musket source grip `(0.0400,0,0)`, Visual position approximately
    `(0,0,0.16744)`, SupportHandTarget
    `(-0.030976,-0.051069,0.586040)`, Muzzle `(0,0,1.180452)`.
  - Blunderbuss source grip `(0.0100,0,-0.00316)`, Visual position
    `(0.0632,0,0.2)`, SupportHandTarget `(-0.031,-0.051,0.45)`, Muzzle
    `(0,0,0.735)`.
- Question: does a single `-0.020` held-rig local-X clearance, expressed by a
  source-grip Z micro-adjustment so semantic grip construction remains intact,
  reduce torso clipping while preserving length, scale, rotation, visibility,
  animation, and relative Butt/Support/Muzzle geometry?
- Planned isolated change: Musket source grip Z `0 -> 0.00478`; Blunderbuss
  source grip Z `-0.00316 -> -0.00216`. This changes only one source-space
  coordinate per weapon and should shift the complete derived rig approximately
  `-0.020` local X. No Pistol, Rifle, belt, scale, rotation, or animation value
  will change.
- Result: **structural pass, visual verdict pending**. Two cache-cleared exact
  Unity builds matched at
  `EEEBA3292119A4619EE3D391246C55E47FC5D9E0BA625DB19E5AB9BBF124315E`.
  Derived Musket Visual/Support/Butt/Muzzle X values are
  `-0.020009/-0.050986/-0.020009/-0.020009`; Blunderbuss values are
  `0.043200/-0.051000/-0.020000/-0.020000`. Lengths remain exactly
  `1.349985/0.848 m`; scales, rotations, animation strings, Pistol, Rifle, and
  all belt specs are unchanged. Exact logs:
  `artifacts/micro-calibration-unity-pass-{1,2}.log` (uncommitted evidence).
- Commands/checks: `test-domain.ps1 -Configuration Release -Clean` passed
  911/911 outside the sandbox after two sandbox-only `File.Replace` denials in
  the unrelated audio staging lifecycle test; `Build-Local.ps1` passed full
  repository validation, 911/911, exact-reference Release, build-output,
  SoundBank, package creation, and strict package validation.
- Provisional package/DLL SHA-256:
  `F130C8F063556EBEA674F9FCA194052E708F88B699E7164067FBCC3580E01388` /
  `869D88032F711839D999186C1325C200A18B407ADB4300519342F35CA7B331E5`.
- Meaning: automated evidence proves a coherent one-axis offset without rig or
  mechanical regression; it cannot prove reduced clipping. Next action:
  commit/push, rebuild exact published identity, run guarded rig/switching and
  frozen regressions, then request the narrow Musket/Blunderbuss visual check.

## 2026-08-07T23:30:00Z - Published clipping candidate qualification

- Implementation commit: published
  `5a37f16a176b54a71d18924c42f769caea5c92c2`.
- Exact package/DLL/AssetBundle/rig-manifest SHA-256:
  `3296604A13F738DC4E8388F3FD8320AB9BA520BD7C9B6ABC04B16B2C114E6B99` /
  `00C19F621AD6184EED6B000ACD76D9C5DC19F5616F8DF91AFA7A1C171A32AF14` /
  `EEEBA3292119A4619EE3D391246C55E47FC5D9E0BA625DB19E5AB9BBF124315E` /
  `15A1B3D6E821A96C1DF64FBF80752254AA3C498CE2871ADC2BB434EE5502B3FC`.
- Exact published rebuild: PASS repository validation, 911/911 tests,
  exact-reference Release, build-output, SoundBank, and strict package gates.
- Guarded PASS run/result hashes: visual rigs
  `20260807T1453008879992Z-disposable-firearm-visual-rigs` /
  `A14D875713AE859907367B4BA3D8F831F8578123EA9112BEA155A0F9367ADDDE`;
  switching `20260807T1454571052269Z-disposable-production-firearm-switching` /
  `5411EDCACDE80BC5F1B74341B4162681E5FFEE9E39C7B78C45E89784C4006321`;
  Targeting Arms `20260807T1456435671775Z-disposable-gunslinger-targeting-arms` /
  `4186D6D5F52209F7521E466C88F4E1ADE43B3801063A97FCC4FD9461C2F09BDF`;
  Wwise `20260807T1458093478502Z-disposable-firearm-wwise-audio` /
  `23D813195452E8CF878981E1D64F78919E785E8415419F6E703114FD5E7A60BB`;
  Scatter `20260807T1459345133932Z-disposable-gunslinger-scatter-shot` /
  `23C8BD0EB4767B293E9DE8F4B73EEEF62485635C9DE75CE7B85BA8E7C6A353DC`;
  reload `20260807T1500590643378Z-disposable-reload-autocast` /
  `9E39C73152180EEDA22757A4AD9DD7166B215EE72C53BFE772D71C1EF92F4E51`.
- A first overlap attempt was refused before launch while Kingmaker was still
  exiting; explicit process-exit gates resolved it. The final wrapper inherited
  a nonzero native exit state after all structured results were PASS; the six
  independently hashed JSON results above are authoritative.
- Result: **AutonomousCandidate mechanical/structural pass**. No save-writing
  API was observed and `KMG_AUTOMATION_BASELINE` was not overwritten. Whether
  the 0.020 clearance visibly reduces torso clipping remains a human gate.

## 2026-08-08T00:15:00Z - Final bounded finishing-pass plan

- Branch/commit before work: clean, published
  `3fab59c65c4767bd6231f4482673a76cf77872b7`.
- Human verdict: reject the immediately preceding `-0.020` local-X clearance;
  preserve accepted Pistol and Crossbow animations; restore the last human-best
  semantic-anchor held values; render no Musket, Blunderbuss, or Rifle on back.
- Exact history evidence: `git diff 39935dc..5a37f16 --
  tools/unity/BuildFirearmBundles.cs` proves the rejected experiment changed
  only Musket source-grip Z `0 -> 0.00478` and Blunderbuss source-grip Z
  `-0.00316 -> -0.00216`.
- Restored fallback values: Musket source grip `(0.0400,0,0)`, derived Visual
  approximately `(0,0,0.16744)`, SupportHandTarget
  `(-0.030976,-0.051069,0.586040)`, Muzzle `(0,0,1.180452)`, length
  `1.349985 m`; Blunderbuss source grip `(0.0100,0,-0.00316)`, derived Visual
  `(0.0632,0,0.2)`, SupportHandTarget `(-0.031,-0.051,0.45)`, Muzzle
  `(0,0,0.735)`, length `0.848 m`.
- Bounded rotation decision: no rotation is selected without a live rendered
  torso/hand fixture that establishes the outward sign. The last unobserved
  translation made appearance worse; speculative 4/7/10-degree rotation could
  likewise trade torso clipping for barrel/support-hand error. The restored
  human-best values remain the mandated safe fallback and minor clipping is
  explicitly accepted.
- Holster question: can a tri-state exact-firearm policy clear belt, sheath,
  prototype-backed attach slots, and private `ReattachSheath` output for long
  guns while leaving active held models and native crossbows unchanged?
- Next action: restore the two grip values, implement explicit `Hidden` policy
  plus exact slot lifecycle cleanup, add focused/runtime assertions, then run
  full deterministic and guarded qualification.

## 2026-08-08T01:00:00Z - Restored rigs and hidden-holster source gate

- Files changed: restored two source grip coordinates; introduced explicit
  `NativeFallback/Custom/Hidden` holster state; long guns set Hidden with typed
  empty attach slots and override; exact `ReattachSheath` postfix destroys only
  an exact resolved long-gun firearm's sheath model; strengthened source/runtime
  assertions. Pistol held source/spec/animation and short-gun attach-slot
  behavior are unchanged.
- Exact final held values equal the last human-best baseline recorded above.
  No 4/7/10-degree rotation or further translation was retained.
- Deterministic evidence: `final-bounded-unity-pass1.log` and `pass2.log`;
  both AssetBundles SHA-256
  `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`.
  Musket length remains `1.34998477 m`; Blunderbuss `0.848 m`.
- Source/package checks: PASS repository validation, 911/911 tests,
  exact-reference Release compilation (including the private lifecycle patch),
  build-output, SoundBank, and strict package validation. Provisional
  package/DLL SHA-256:
  `C6E416A87212BA244F3A71EB0FAC78466B8C43413DA8B7FA3B027C8561BC598A` /
  `78E93E479D2B58E9466F86A5F4A357C28755AC997766A83427809445EE856132`.
- Result: **source/structural pass**. Runtime must still prove Harmony patch
  application, empty effective long-gun attach slots, held models, native donor
  isolation, switching cleanup, and frozen mechanics on a published commit.

## 2026-08-08T02:00:00Z - Final bounded published qualification

- Published implementation: `6b1f5db443c1051ecd949c8987b75ccd3c69c78d`.
- Exact candidate package/DLL/AssetBundle/rig-manifest SHA-256:
  `FA955857DA4DDE83D43107D57A6CE4B1E41F738A4BB18F30269F4A69F067740D` /
  `BAFC115F3839B7D31E6DB9BB5C3D6D97FFB7BCCA97416AD440FA2997B0CD4E74` /
  `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B` /
  `35BB38BF142D1F1DB3439F4EC328CE7EBF2CFD149318BCEF714A1254CB5301D1`.
- Exact published rebuild: PASS repository validation, 911/911 tests,
  exact-reference Release, build-output, SoundBank, and strict package gates.
- Guarded PASS run/result hashes: visual rigs
  `20260807T1531219665001Z-disposable-firearm-visual-rigs` /
  `F352FAF9247FC4CC51137DB73B1404B91F70CB66B93CA15A185A0AF934C04D63`;
  switching `20260807T1533292743869Z-disposable-production-firearm-switching` /
  `A13D189B2B4FF6B87B702A46226FF0EFEAD2E22A0ED3797EC767ADAEB8DED3D9`;
  Targeting Arms `20260807T1534557411006Z-disposable-gunslinger-targeting-arms` /
  `34C1CE88D321819B4B538B15C7BB20DB6F4FDDED1DF9052A26E57AD592C30695`;
  Wwise `20260807T1536223446254Z-disposable-firearm-wwise-audio` /
  `4B45AE697E686B27EC9FD16DE5955C4B2D5A19F978D52BCE8450C9F262FCC299`;
  Scatter `20260807T1537478186289Z-disposable-gunslinger-scatter-shot` /
  `AA8C9F38CB83CCCB4847BB9F1E8B8D28B9B05EF6339D3526A45144E9BD679F65`;
  reload `20260807T1539127986702Z-disposable-reload-autocast` /
  `92567B093A657AFCCE332F48482AC9280B46AF51B5F192D1D5319209CDECF68C`.
- Runtime meaning: all three long guns have active held renderers, exact native
  IK, null belt/sheath, empty effective attach slots, and Hidden policy; native
  donors remain unchanged. Switching cleanup, projectile/damage, Wwise,
  Scatter, and reload contracts pass. The multi-run wrapper's inherited native
  exit state was nonzero after all six JSON results were already PASS; it does
  not represent a scenario failure.
- Final verdict: automated work for this bounded pass is complete. Regular
  Pistol held appearance retains its prior narrow human acceptance. Long-gun
  held rigs remain `AutonomousCandidate`; minor residual torso clipping is
  explicitly accepted for now. No long gun is rendered on the back.

## 2026-08-08T03:00:00Z - Human hard FAIL and attach-slot A/B plan

- Failed implementation: `6b1f5db443c1051ecd949c8987b75ccd3c69c78d`;
  evidence handoff `07b2a8c035a057fc3664fbab281fe328ac86b51a`.
- Human verdict: **FAIL**. Held Musket is completely invisible. Held
  Blunderbuss is completely invisible. Characters enter the correct weapon
  pose, but no held model is player-visible. This supersedes detached prefab
  and active-renderer assertions; those did not prove attachment/rendering in
  the actual player inventory/world hand hierarchy.
- Promotion status: this candidate must not be promoted or merged. Its result
  is not "minor residual clipping" and the bounded pass is not complete.
- Exact regression hypothesis: long-gun Hidden changed inherited attach slots
  to an empty collection and forced `m_OverrideAttachSlots=true`. That
  blueprint-level contract may prevent active held attachment.
- A/B experiment A (this checkpoint only): retain custom equipped model,
  restored human-best anchors, null belt/sheath, severed Prototype, and current
  `ReattachSheath` patch. Remove empty `m_PossibleAttachSlots`, remove forced
  override, and restore the inherited native attach-slot/override values copied
  from the visible semantic-anchor candidate. No model, transform, scale,
  anchor, animation, projectile, or mechanical change is authorized.
- Decision gate: build/package this isolated A/B candidate and stop for the
  five-item narrow human check. If held models remain invisible, disable only
  `FirearmHiddenHolsterPatch` in experiment B. Full regressions wait for human
  confirmation of actual held visibility.

### Experiment A source/package result

- Exact change: long-gun `OverrideAttachSlots` restored to `false`; the Hidden
  block no longer writes `m_PossibleAttachSlots` or
  `m_OverrideAttachSlots`. Materialization continues to copy both inherited
  native values before Prototype is severed. Belt/sheath remain null and the
  sheath postfix remains compiled. No AssetBundle input changed; bundle remains
  the human-best `F52CBC5B...71A0B` identity.
- Checks: PASS repository validation, 911/911 domain/reflection tests,
  exact-reference Release, build-output, SoundBank, package creation and strict
  package validation.
- Provisional package/DLL SHA-256:
  `7383AFF87023575D138439C1BFB57A28B18D4C101174374E63FDB079382FFB25` /
  `C2259009B5806A4F9C982C6097C4EC74D013F19672E8E22B602FD2BEF2DDB999`.
- Result: **automated source pass; human visibility unknown**. This is not
  evidence that the held models are visible. Publish/rebuild exact identity,
  then stop for the required narrow human A/B check without running the full
  regression matrix.

- Published experiment A: `6e3aa3782eb6328786b60330ae453fa2d5241f6a`.
  Exact published package/DLL/AssetBundle/rig-manifest SHA-256:
  `CE0C03BE2AF4D0BA0BBFF6A975C5733D106716B4BE581F69F44ED46140B2F90D` /
  `BC1B4C8B67B8CD68A654DD1334361C61A47733A292A78138DC0239874B8387DC` /
  `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B` /
  `35BB38BF142D1F1DB3439F4EC328CE7EBF2CFD149318BCEF714A1254CB5301D1`.
- Exact published rebuild again passed repository validation, 911/911 tests,
  exact-reference Release, build-output, SoundBank, and strict package gates.
  Per explicit human-checkpoint policy, no detached guarded runtime or full
  regression scenarios were run. Next evidence must be actual player-visible
  Pistol/Musket/Blunderbuss and inactive-back observation.
