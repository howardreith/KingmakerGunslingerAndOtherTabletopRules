# Z-FIREARM-MAINTENANCE — Journal (append-only)

Format: `YYYY-MM-DD #N — topic`. Concise entries; evidence paths under
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\` when runtime work starts.

---

2026-09-12 #1 — Mission start, identity verification, worktree setup (P0)

- Read mission (owner download copy, 339 lines; copied verbatim to
  `Z-FIREARM-MAINTENANCE-MISSION.md` in the worktree), root `AGENTS.md`.
- Main checkout `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`:
  clean on `master` @ `71af37ac`; `git fetch` confirmed `origin/master` ==
  `71af37acc1dc7548fecb067bf753c11d6b90f893` (no drift).
- Existing unrelated worktree `worktrees\share-transmutation-instant`
  (branch `codex/share-transmutation-instant`, clean @ `636d70bf`) — preserved,
  untouched.
- Created isolated worktree `worktrees\firearm-maintenance` with new branch
  `codex/z-firearm-maintenance-rest-safety` from `master` @ `71af37ac`
  (mission-suggested branch name).
- Verified worktree compatibility of the toolchain up front:
  - `scripts/common.ps1` `Get-KmgRepositoryRoot` resolves relative to the
    script directory → worktree root. ✔
  - `codex-policy/Push-KingmakerGunslinger.ps1` takes `-RepositoryRoot`, runs
    `git -C`, requires `codex/*` branch + clean tree + approved origin URL +
    fast-forward. No merge/tag/release behavior. ✔
  - `scripts/Build-Local.ps1` walks up ≤6 parents from repo root to find
    `private\extracted-references\KingmakerGunslinger-private-build-references`;
    lab root has it → reachable from `worktrees\firearm-maintenance`. ✔
    NOTE: Build-Local currently pins active version `0.0.126`; candidate
    version allocation later must update that pin as part of the established
    workflow (check how prior missions did it, e.g. 0.0.125→0.0.126).
  - `scripts/Deploy-Local.ps1` / `Backup-Live-Mod.ps1` hard-pin the exact live
    mod dir and backup root; deployment is backup-first and reversible. ✔
- Installed mod: `Info.json` version `0.0.126`, matches master release record.
  Game install at `C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker`.
- Build tools present: dotnet SDK 8.0.424, Python 3.14.7; MSBuild resolved via
  vswhere fallback at build time (same as prior missions).
- Mission source map verification: all 16 files listed in mission §5 exist at
  the stated paths under `src/KingmakerGunslinger/`. No renames needed in the
  initial contract source map.
- Created durable records: MISSION (copy), STATE, JOURNAL (this file),
  `docs/FIREARM-MAINTENANCE-CONTRACT.md` (initial),
  `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md` (all IDs, NOT RUN).
- No source changes, no builds, no runtime launches this slice.

Strategy note: P1 (interruption) precedes P3 (repair restriction) per mission
§6 ordering, to avoid shipping an intermediate candidate where accidental
automation has harsher consequences.

---

2026-09-12 #2 — P0 source/native trace complete; pre-change domain baseline established

- Read all 16 mission §5 source-map files in full. Baseline findings mirrored
  into `docs/FIREARM-MAINTENANCE-CONTRACT.md` [BASELINE] sections:
  - Field repair today: Broken **and** Wrecked eligible at policy
    (`FirearmActionPolicy.EvaluateRepair`) and transaction
    (`FirearmRepairTransactionService.GetRejection`); **no combat check**;
    weapon bound only by two back-to-back checks inside ability delivery
    (`RepairTestMusketAbilityLogic.TryPrepare`→`Complete`), not at command
    issuance. Quick Clear already Broken-only via
    `FirearmStateMachine.Repair` (shared low-level transition; must stay
    combat-usable). Legacy Overhaul alias delegates to the same ability logic.
  - Rest: only `CraftingRestResetPatch` (per-unit `ApplyRest` postfix).
    Native rest state machine traced from
    `private\charvis-native-il\Assembly-CSharp.il` (2.1.7b): phases
    Manage/Camp/Sleep/Finished/SkipTime; `TickSleepPhase` sets
    `RestSucceeded` each tick, on `RemainingTime<=0` → Finished +
    `StopRestProcess` (genuine completion); `NightRandomEncounter` → Finished
    + `StopRestProcess` (interrupted); mid-sleep `ApplyRestInterval` (per
    unit via `HealAndApplyRest`) is transient, and `ApplyRest` is also called
    by LevelUp/KingdomTimeline/KingdomTask/Recruit/Respec/CapitalCompanion
    logic — per-unit ApplyRest is NOT a rest boundary.
    `StopRestProcess` (started once at any termination via
    LoadingProcess) checks RestSucceeded/SkipTime, preloads, and autosaves at
    the end → P2 candidate: prefix on `StopRestProcess` gated on
    `RestSucceeded && !NightRandomEncounter && !SkipTime`, before the
    autosave. Scripted rest (`StartScripted`) also applies intervals;
    world-map/inn routes deferred to runtime verification (R08).
  - Attack continuation today: mid-sequence misfire does NOT stop the
    sequence — `FullAttackAutoReloadPolicy.ContinueLoaded` keeps firing a
    just-broken gun with surviving rounds; empty+Free reload reloads and
    continues; `EmptyFirearmAttackCommandPatch.ResumeAttack` resumes pending
    attacks for a Broken gun after reload. Committed degradation point =
    `FirearmMisfireRuntime.CommitConditionTransition` (expected-state-guarded
    transition + repository identity check); negation paths (Stranger's
    Fortune, Expert Loading) return before it.
  - Player intent (IL): `ClickUnitHandler.OnClick` →
    `CreateAttackCommand` → `Commands.Run` + `CombatState.ManualTarget`.
    Attack branch does NOT set `CreatedByPlayer` (only movement/interact/
    auto-use branches do) → CreatedByPlayer cannot discriminate attack
    intent. RTWP auto-attacks are Brain-issued (`BlueprintAiAttack`,
    ManualTarget considerations). P1 suppression must be command/order-scoped;
    consent signal candidates: click event bus / ManualTarget semantics.
    `UnitCommands` exposes Run/AddToQueue/InterruptAll/
    InterruptAiCommands/InterruptAndRemoveCommand/InterruptGroupCommand.
- Test conventions: domain suite = dependency-free exe, pure policy/service
  tests + source-contract string assertions
  (`UnifiedFirearmRepairTests` asserts current "Broken or Wrecked" unified
  text; to be updated explicitly in P3/P4).
- Worktree environment fixes (machine-local, git-ignored, never committed):
  copied `GamePath.props` and
  `artifacts/inspection/bodyguard-native/Assembly-CSharp.il` from the main
  checkout; without them the domain build loses Newtonsoft.Json reference
  (2 bodyguard-runtime FAILs).
- **Pre-change baseline: `scripts/test-domain.ps1 -Configuration Release` →
  1581/1581 PASS, exit 0** @ 71af37ac (+ mission-records commit), log
  `/tmp/domain-baseline2.log` (session-local). Inherited-failure note: the two
  bodyguard-runtime IL-dump tests are environment-dependent, not source
  failures; fixed by copying the generated artifact.
- No source changes yet. Next slice: P1 design + implementation of
  command-scoped interruption (see STATE next-actions).
