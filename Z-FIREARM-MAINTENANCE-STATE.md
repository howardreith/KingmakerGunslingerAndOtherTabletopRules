# Z-FIREARM-MAINTENANCE — Mission State

**Mission ID:** `Z-FIREARM-MAINTENANCE`

**Last updated:** 2026-09-12 (P1 source slice complete, domain green)

## Mission status

`IN_PROGRESS` — phase **P1 source implemented (domain layer); native qualification pending; next P2**

First incomplete acceptance IDs: A01–A08 native cells all `NOT RUN` (A01/02/04/05/06/07
domain PARTIAL); F01–F06, R01–R10, W01–W02, C01–C06, Q01–Q03 `NOT RUN`
(see `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`).

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
| Repository validation | PASS (worktree) | journal #1, #3 |
| Domain suite pre-change baseline | PASS 1581/1581 @ base | journal #2 |
| Domain suite after P1 | **PASS 1592/1592, exit 0** | journal #3 (`scripts/test-domain.ps1 -Configuration Release`) |
| Main project Release build after P1 | PASS (clean Rebuild compiles) | journal #3 |
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

1. **P2 rest-restoration slice**: create
   `Gunsmithing/CompletedRestFirearmMaintenancePolicy.cs` (pure: eligibility,
   scope dedup, once-only semantics) and a runtime coordinator with a Harmony
   prefix on `RestController.StopRestProcess()` gated on
   `Status.RestSucceeded && !NightRandomEncounter && !Status.SkipTime`;
   restore via a dedicated rest-restoration entry point that reuses the
   exact-item transaction protections (NOT the field-repair entry).
   Participants/kit scope: party members with the Gunsmithing repair
   capability + ≥1 reusable kit in shared inventory; carried inventory +
   participant equipment/alternate sets; dedupe concrete items; Broken→Normal
   preserving rounds, Wrecked→Normal stays empty. Keep
   `CraftingRestResetPatch` independent. Add domain tests (R-row policy
   layer), register in csprojs, bump validator count + active-chain blocks,
   run full domain suite + main Release rebuild.
2. Commit + push P2 slice via the approved wrapper; update STATE/JOURNAL and
   R-row domain cells in the acceptance matrix.
3. **P3 field-repair restriction slice** after P2 is green (details in
   contract §1/§5 and decisions above).

Update this file after every coherent slice. Keep it an index; details live in
the journal and acceptance matrix.
