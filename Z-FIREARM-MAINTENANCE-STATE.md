# Z-FIREARM-MAINTENANCE — Mission State

**Mission ID:** `Z-FIREARM-MAINTENANCE`

**Last updated:** 2026-09-12 (P4c complete; next P4d native qualification)

## Mission status

`IN_PROGRESS` — phase **P4d: guarded native runtime qualification on the immutable 0.0.127 candidate**

First incomplete acceptance IDs: ALL native cells `NOT RUN` (A/F/R domain
PARTIAL where noted); W01–W02, C01–C06, Q01 partially (build/validation done,
native pending); R03/R08 fully `NOT RUN`
(see `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`).

## Candidate identity (immutable for all remaining gates)

| Item | Value |
| --- | --- |
| Version | 0.0.127 (informational 0.0.127-firearm-maintenance) |
| Source commit | `62285749fb7bd679fa452bf4ad4d6537aea762f5` (src/ identical at HEAD `0991f6411f1ed4786d23b48112c258c62899a355`; the later commit adds only tests/journal) |
| Package | `artifacts/local-runtime/0.0.127/KingmakerGunslinger-0.0.127-local-runtime.zip` SHA-256 `97e4039f47a2153572ac938ab8225a8ac8b2abdf15521a36c69eb68badf2b72f` (strict UMM validation PASS; also `artifacts/packages/KingmakerGunslinger-0.0.127-firearm-maintenance.zip`) |
| DLL SHA-256 | `bbceda4498092b2c0ff9aefe38c43382b8454d7be8c8bcc4776d52c8b995f835` |
| DLL MVID | `204d1c6d-6f02-4650-8711-e82239dce9cb` |
| Installed | NOT DEPLOYED yet (live install still 0.0.126 release); backup-first reversible deployment authorized for qualification only |

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
| Identity verification / repository validation | PASS each slice | journal #1–#6 |
| Domain baseline @ base | PASS 1581/1581 | journal #2 |
| Domain after P1/P2/P3 | PASS 1592/1601/1611 | journal #3/#4/#5 |
| 0.0.127 source validation (full chain) | PASS | journal #6 |
| Full Build-Local pipeline | **PASS exit 0** (validation + 1611/1611 + exact-reference build + strict package validation x2) | journal #6 |
| Native runtime | NOT RUN | — |

## Next concrete actions

1. **P4d — guarded native qualification** (on the immutable candidate above):
   a. Write the guarded runtime scenario(s) under
      `src/KingmakerGunslinger/RuntimeTesting/` following the existing
      RuntimeTestRunner partial-class pattern (see
      RuntimeTestRunner.TeleportationScrolls.cs for naming/assertions);
      cover: (i) misfire-interruption (forced natural roll via the guarded
      mechanism, full attack, verify no next shot/reload + counters
      SequenceInterruptionRejections / full-attack.ended-after-committed-break
      log), (ii) field-repair rejection (combat + Wrecked + legacy alias),
      (iii) completed-rest restoration (camp; verify conditions → Normal,
      Wrecked unloaded, once-only) and cancelled-rest no-op,
      (iv) save/load persistence round trip with a named disposable fixture.
   b. Register scenarios in the runner; rebuild candidate ONLY via
      Build-Local if source changed (new DLL hash ⇒ rerun all native lanes
      on the new identity).
   c. Backup-first deploy via scripts/Deploy-Local.ps1 (records backup
      identity); run `scripts/Invoke-KingmakerRuntimeTest.ps1` per lane
      x2 fresh runs (Steam App ID 640820 enforced by the harness);
      evidence under C:\Dev\KingmakerGunslingerLabuntime-evidence\.
   d. Restore the pre-mission 0.0.126 install (scripts/Restore-Live-Mod.ps1)
      unless owner separately authorizes leaving the candidate.
   e. Update acceptance matrix native cells + release notes PENDING gates.
2. P5: reconcile matrix, cleanup verification, final report
   (Z-FIREARM-MAINTENANCE-REPORT.md), owner smoke test pointer.
