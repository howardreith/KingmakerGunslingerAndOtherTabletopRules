# Z-FIREARM-MAINTENANCE — Mission State

**Mission ID:** `Z-FIREARM-MAINTENANCE`

**Last updated:** 2026-09-12 (P0 session 1, trace + baseline complete)

## Mission status

`IN_PROGRESS` — phase **P0 complete (pending commit of trace records); next phase P1**

First incomplete acceptance IDs: all of F01–F06, R01–R10, A01–A08, W01–W02,
C01–C06, Q01–Q03 (`NOT RUN`; see `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`).

## Identity snapshot

| Item | Value |
| --- | --- |
| Host | Windows 10 (10.0.19045 x64), Git Bash |
| Lab root | `C:\Dev\KingmakerGunslingerLab` |
| Worktree | `C:\Dev\KingmakerGunslingerLab\worktrees\firearm-maintenance` (isolated; main checkout left on `master`) |
| Branch | `codex/z-firearm-maintenance-rest-safety` |
| Base SHA | `71af37acc1dc7548fecb067bf753c11d6b90f893` (= `origin/master`, verified in sync after fetch) |
| Current HEAD | mission-records commit `4e74be2d` (records only, no source changes) |
| Last qualified/pushed commit | `4e74be2ddc708d034da139744ee211628fe4ae1e` — guarded push PASS, remote branch verified (`codex/z-firearm-maintenance-rest-safety`) |
| Remote verification | wrapper reported `Guarded push PASS` and re-read the remote head |
| Dirty files | trace updates to STATE/JOURNAL/CONTRACT (committing next); git-ignored machine-local copies: `GamePath.props`, `artifacts/inspection/bodyguard-native/Assembly-CSharp.il` (never commit) |
| Installed mod | `...\Pathfinder Kingmaker\Mods\KingmakerGunslinger` Info.json `0.0.126` (matches master release record) |
| Installed DLL SHA-256 | NOT VERIFIED yet (record during baseline slice) |
| Candidate version | NOT BUILT (expected next version 0.0.127 — confirm no later release before allocating) |
| Build env | dotnet 8.0.424 SDK, Python 3.14.7, MSBuild via vswhere fallback, reference bundle present at lab `private\extracted-references\KingmakerGunslinger-private-build-references`, .NET 4.7 ref assemblies assumed present (verified by Build-Local on first run) |
| Worktree script support | verified: `Get-KmgRepositoryRoot` resolves relative to script dir; push wrapper takes `-RepositoryRoot`; reference bundle walk-up reaches lab `private\` from worktrees\ |

Other work preserved: main checkout clean on `master`; unrelated worktree
`worktrees\share-transmutation-instant` (branch `codex/share-transmutation-instant`,
clean at `636d70bf`) untouched.

## Implementation decisions so far

None in source yet — P0 investigation complete. Verified decisions that bind
later phases:

- **P2 rest boundary candidate**: prefix on `RestController.StopRestProcess()`
  (called exactly once per rest-process termination, before its trailing
  autosave), gated on `Status.RestSucceeded && !NightRandomEncounter &&
  !Status.SkipTime`. Per-unit `ApplyRest` postfixes are unusable (transient
  mid-sleep + non-camping callers: LevelUp, Kingdom, Recruit, Respec,
  Capital). Runtime-verify before committing to it.
- **P1 trigger source**: `FirearmMisfireRuntime.CommitConditionTransition` is
  the only committed exact-item degradation point during attacks; negation
  paths return before it. Interruption hooks needed at:
  `FreeActionFullAttackReloadPatch` (full-attack iterations),
  `EmptyFirearmAttackCommandPatch` (`ResumeAttack` pending continuation and
  `CreateAttackCommand` construction). `UnitCommand.CreatedByPlayer` is NOT a
  usable attack-intent flag (never set on the click attack branch); RTWP
  auto-attacks are Brain-issued. Consent/release signal to be designed in P1.
- **P3 shape**: Broken-only + out-of-combat enforcement goes in the
  field-repair path (policy/runtime/ability layers); the shared
  `FirearmStateMachine.Repair` transition stays combat-usable for Quick Clear.
  Command-start binding of the concrete firearm must be added at actual
  ability-command commencement (today both checks are inside delivery).

## Results

| Gate | Result | Evidence |
| --- | --- | --- |
| Identity verification | PASS | journal 2026-09-12 #1 |
| Repository validation | PASS (in worktree, `scripts/validate-repository.ps1`) | journal #1 |
| Domain suite (pre-change baseline) | **PASS 1581/1581, exit 0** @ base+records commit | journal #2; requires git-ignored `GamePath.props` + bodyguard IL artifact in worktree |
| Build/package/runtime | NOT RUN on any candidate (no source changes yet) | — |

## Active processes / fixtures / cleanup

- None. No runtime launches, no installs touched, no leases held, normal install
  is the untouched 0.0.126 release (backup/restore not yet needed). Main
  checkout and the unrelated `share-transmutation-instant` worktree untouched.

## Blockers

None. Worktree environment note (not a blocker): fresh worktrees need
git-ignored `GamePath.props` and the bodyguard IL artifact copied in for the
domain suite (recorded in journal #2).

## Next concrete actions

1. **P1 design slice**: add a focused domain-testable interruption policy
   (e.g. `Misfires/FirearmSequenceInterruptionPolicy.cs` + a runtime registry
   recording committed degradation per (wielder, weapon, attack roll)) and
   wire: (a) `FirearmMisfireRuntime.CommitConditionTransition` records the
   degradation + cancels the wielder's active `UnitAttack` for that weapon
   via `Commands.Interrupt*`; (b) `FreeActionFullAttackReloadPatch` prefix
   ends remaining iterations when the previous shot of this command committed
   a degradation on the same exact weapon; (c)
   `EmptyFirearmAttackCommandPatch.ResumeAttack` cancels pendings whose
   weapon degraded since capture; (d) `CreateAttackCommand` prefix blocks
   immediate automatic re-issue (Brain path) for a degraded weapon until a
   genuine player-attack consent signal — design the consent signal against
   IL facts in the contract (click bus / ManualTarget), then write focused
   tests for A01–A08 domain layer.
   Prerequisites: baseline green (done); no other in-flight edits.
2. After P1 domain tests pass: run `scripts/test-domain.ps1`, update
   STATE/JOURNAL/ACCEPTANCE (A-row domain cells), commit, push via wrapper.
3. P2 slice: implement `StopRestProcess` prefix coordinator + rest
   restoration service (separate entry point from field repair), starting
   from the contract's rest-boundary notes; runtime-verify the boundary in a
   guarded scenario before extending scope.

Update this file after every coherent slice. Keep it an index; details live in
the journal and acceptance matrix.
