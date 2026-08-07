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
