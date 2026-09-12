# Z-FIREARM-MAINTENANCE — Mission State

**Mission ID:** `Z-FIREARM-MAINTENANCE`

**Last updated:** 2026-09-12 (session 1 end; P4d scenario 1/4 done)

## Mission status

`IN_PROGRESS` — phase **P4d: guarded native qualification (scenario 1 of 4 written; remaining scenarios, rebuild, deployment, and runs pending)**

First incomplete acceptance IDs: ALL native cells `NOT RUN` (A/F/R domain
PARTIAL where noted); W01–W02, C01–C06 pending native; R03/R08 fully
`NOT RUN` (see `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`).

Branch HEAD `f4060c6739df145f85f8a704be8e28630e2e3a21` (all work pushed;
remote verified after every checkpoint).

## Candidate identity

**SUPERSEDED — source changed after journal #6's build** (scenario code
added in commits 0991f641..f4060c67; src/ deltas exist). Historical:
0.0.127 build @ 6228574, package SHA-256 97e4039f...f72f, DLL SHA-256
bbceda44...f835, MVID 204d1c6d-6f02-4650-8711-e82239dce9cb (strict UMM
validation PASS; journal #6). Before any native run: rerun
`scripts/Build-Local.ps1` and record the NEW package/DLL SHA-256/MVID;
all native evidence must reference that identity.

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

1. **P4d remaining scenarios** (follow `RuntimeTestRunner.FirearmMaintenance.cs`
   + journal #7/#8 patterns; register catalog const + Allowed set + dispatch
   branch + csproj + RuntimeAutomation.Common.ps1 metadata each time):
   a. `disposable-field-repair-rejection`: fixture with Gunsmithing + kit +
      Broken gun → RepairTestMusketRuntime.Evaluate available out of combat;
      seed combat via Player.IsInCombat? (combat state on detached fixture —
      verify a safe native way, e.g. Game.Instance.Player.IsInCombat
      backing field or a real encounter; do NOT fake booleans) → rejected
      with combat reason; Wrecked → rejected with full-rest reason (direct
      Evaluate + ability delivery via AbilityExecutionContext like
      RunDisposableGunsmithingCrafting + transaction status
      WreckedRequiresRest).
   b. `disposable-completed-rest-restoration`: verify which native route
      reaches StopRestProcess from a guarded fixture (StartScripted vs camp
      flow; R06/R08 flags); assert CompletedRestMaintenancePatch counters +
      conditions → Normal (Wrecked unloaded) + once-only + summary; include
      a non-termination case (skip-time/encounter flag) asserting no run.
   c. persistence lane: request-local fixture + named disposable save round
      trip IF an authorized write route exists (mission §2; otherwise mark
      BLOCKED and report).
2. **Rebuild** `scripts/Build-Local.ps1`; record new artifact identity here.
3. **Deploy** via `scripts/Deploy-Local.ps1 -PackagePath <new package>`
   (backup-first; record backup identity); run lanes x2 fresh launches via
   `scripts/Invoke-KingmakerRuntimeTest.ps1 -Scenario <id>
   -ExpectedVersion 0.0.127 ...`; evidence under
   `C:\Dev\KingmakerGunslingerLabuntime-evidence\`.
4. Restore live install (`scripts/Restore-Live-Mod.ps1` + recorded backup).
5. P5: acceptance matrix native cells, RELEASE-NOTES-0.0.127 PENDING gates,
   Z-FIREARM-MAINTENANCE-REPORT.md, final commit/push.

No active processes, fixtures, or leases. Live install untouched (0.0.126
release). Worktree has no dirty files between checkpoints.
