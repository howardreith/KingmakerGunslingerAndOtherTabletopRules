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
