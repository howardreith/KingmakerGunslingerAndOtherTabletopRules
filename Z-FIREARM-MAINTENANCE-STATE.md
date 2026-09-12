# Z-FIREARM-MAINTENANCE — Mission State

**Mission ID:** `Z-FIREARM-MAINTENANCE`

**Last updated:** 2026-09-12 (P3 source slice complete, domain green)

## Mission status

`IN_PROGRESS` — phase **P1–P3 source implemented (domain layer); next P4 (docs/version/pipeline/native)**

First incomplete acceptance IDs: ALL native cells `NOT RUN` (A/F/R domain
PARTIAL where noted); W01–W02, C01–C06, Q01–Q03 `NOT RUN`; R03/R08 fully
`NOT RUN` (see `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`).

## Identity snapshot

| Item | Value |
| --- | --- |
| Host | Windows 10 (10.0.19045 x64), Git Bash |
| Lab root | `C:\Dev\KingmakerGunslingerLab` |
| Worktree | `C:\Dev\KingmakerGunslingerLab\worktrees\firearm-maintenance` (isolated; main checkout left on `master`) |
| Branch | `codex/z-firearm-maintenance-rest-safety` |
| Base SHA | `71af37acc1dc7548fecb067bf753c11d6b90f893` (= `origin/master`, verified in sync after fetch) |
| Current HEAD | P1 slice commit (see git log; base 71af37ac + records + P1) |
| Last qualified/pushed commit | see journal latest entry; verify with `git log origin/codex/z-firearm-maintenance-rest-safety -1` |
| Remote verification | wrapper `Guarded push PASS` after each checkpoint commit |
| Dirty files | none between checkpoints (git-ignored machine-local: `GamePath.props`, `artifacts/` incl. bodyguard IL dump — never commit) |
| Installed mod | `...\Pathfinder Kingmaker\Mods\KingmakerGunslinger` Info.json `0.0.126` (matches master release record) |
| Installed DLL SHA-256 | NOT VERIFIED yet (record during baseline slice) |
| Candidate version | NOT BUILT (expected next version 0.0.127 — confirm no later release before allocating) |
| Build env | dotnet 8.0.424 SDK, Python 3.14.7, MSBuild via vswhere fallback, reference bundle present at lab `private\extracted-references\KingmakerGunslinger-private-build-references`, .NET 4.7 ref assemblies assumed present (verified by Build-Local on first run) |
| Worktree script support | verified: `Get-KmgRepositoryRoot` resolves relative to script dir; push wrapper takes `-RepositoryRoot`; reference bundle walk-up reaches lab `private\` from worktrees\ |

Other work preserved: main checkout clean on `master`; unrelated worktree
`worktrees\share-transmutation-instant` (branch `codex/share-transmutation-instant`,
clean at `636d70bf`) untouched.

## Implementation decisions so far

**P1 (implemented this session, journal #3):** committed-degradation record in
`FirearmMisfireRuntime` (per-RuleAttackRoll CWT + suppression registry keyed
wielder/weapon); full-attack iterations end at the next `OnAction` prefix when
the previous shot of the same command committed the break (before the
loaded/free-reload decisions); `CreateAttackCommand` construction gate rejects
automatic recreation while suppressed (never queues a replacement reload);
pending reload-resume captures/rechecks a degradation epoch; player intent =
frame-scoped marker set by a Harmony prefix on `ClickUnitHandler.OnClick`
(Harmony 1.2 has NO finalizer → frame scoping, not a depth counter); repaired
weapons never gated; suppression is process-memory/weak-keyed. Known edges for
native qualification: same-frame AI construction during a player unit-click;
console/radial-menu attack routes; RTWP brain re-issue; Dead Shot/scatter
composite paths (A08).

**P0 binding decisions** (details in contract + journal #2):
- **P2 rest boundary candidate**: prefix on `RestController.StopRestProcess()`
  (called exactly once per rest-process termination, before its trailing
  autosave), gated on `Status.RestSucceeded && !NightRandomEncounter &&
  !Status.SkipTime`. Per-unit `ApplyRest` postfixes are unusable (transient
  mid-sleep + non-camping callers: LevelUp, Kingdom, Recruit, Respec,
  Capital). Runtime-verify before committing to it.
- **P3 shape**: Broken-only + out-of-combat enforcement goes in the
  field-repair path (policy/runtime/ability layers); the shared
  `FirearmStateMachine.Repair` transition stays combat-usable for Quick Clear.
  Command-start binding of the concrete firearm must be added at actual
  ability-command commencement (today both checks are inside delivery).

**Test-count bookkeeping mechanism (learned, journal #3)**: adding domain
tests requires bumping `DETERMINISTIC_TEST_COUNT` in the ACTIVE validator and
the active-chain blocks in `validation/static-validation.json`
({121,123,124,125,126} — live current-expectation records; deeper blocks
frozen). P4's 0.0.127 validator supersedes the interim bump.

## Results

| Gate | Result | Evidence |
| --- | --- | --- |
| Identity verification | PASS | journal #1 |
| Repository validation | PASS (worktree) | journal #1–#5 |
| Domain suite pre-change baseline | PASS 1581/1581 @ base | journal #2 |
| Domain suite after P1 | PASS 1592/1592 | journal #3 |
| Domain suite after P2 | PASS 1601/1601 | journal #4 |
| Domain suite after P3 | **PASS 1611/1611, exit 0** | journal #5 |
| Main project Release Rebuild (P1/P2/P3) | PASS each slice | journal #3–#5 |
| Build/package (Build-Local pipeline) | NOT RUN (P4) | — |
| Native runtime | NOT RUN | — |

## Active processes / fixtures / cleanup

- None. No runtime launches, no installs touched, no leases held, normal install
  is the untouched 0.0.126 release (backup/restore not yet needed). Main
  checkout and the unrelated `share-transmutation-instant` worktree untouched.

## Blockers

None. Worktree environment note (not a blocker): fresh worktrees need
git-ignored `GamePath.props` and the bodyguard IL artifact copied in for the
domain suite (recorded in journal #2).

## Next concrete actions

1. **P4a — docs + contract closeout**: update
   `docs/FIREARM-MAINTENANCE-CONTRACT.md` (implemented status per section),
   `KNOWN-ISSUES.md`, `CHANGELOG.md`, player smoke-test guidance, and the
   coverage/fidelity records if required by the repo ledger; close the old
   Wrecked-in-combat design question with the new behavior.
2. **P4b — version allocation + validator chain**: allocate 0.0.127
   (verify no later release exists), create
   `tools/validate_firearm_maintenance127.py` (following the 126 pattern:
   baseline chain + new mission contracts + count 1611+docs tests),
   wire `tools/validate_repository.py` dispatch, update `Info.json`,
   `scripts/Build-Local.ps1` version pin, static-validation.json new block.
3. **P4c — immutable candidate**: run full
   `scripts/Build-Local.ps1` (validate + domain -Clean + exact build +
   package) → record version/SHA-256/MVID/package hash; strict package
   validation.
4. **P4d — guarded native qualification** via
   `scripts/Invoke-KingmakerRuntimeTest.ps1` with a new/reused guarded
   scenario covering: misfire interruption (RTWP+TB), field-repair
   rejection in combat + Wrecked, completed-rest restoration + cancelled
   rest no-op, save round trip. Mandatory lanes ×2 fresh runs per mission
   §7. Backup-first reversible deployment per mission §2; restore after
   testing.
5. P5: reconcile matrix, cleanup verification, final report + owner smoke
   test.

Update this file after every coherent slice. Keep it an index; details live in
the journal and acceptance matrix.
