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
