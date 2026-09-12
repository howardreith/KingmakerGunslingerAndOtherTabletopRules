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

---

2026-09-12 #3 — P1 implemented: newly-Broken sequence interruption (domain layer green)

New files:
- `src/KingmakerGunslinger/Firing/BrokenSequenceInterruptionPolicy.cs` (pure,
  dependency-free): construction gate (Allow / AllowAndConsume /
  RejectInterrupted), ShouldEndFullAttack, MayResumeCapturedAttack.
- `src/KingmakerGunslinger/Firing/BrokenSequenceSuppressionRuntime.cs`:
  weak-keyed per-(wielder, exact weapon) epoch + suppression registry;
  frame-scoped player-attack marker; ClearForRuntimeTest seam. Process-memory
  only — no persistence, no scene/save leakage.

Wiring (4 points):
1. `Misfires/FirearmMisfireRuntime`: after a verified
   `CommitConditionTransition`, records the degradation on the exact
   RuleAttackRoll (new CWT + `TryGetCommittedDegradation`) and notifies
   `OnCommittedDegradation(wielder, firearmItem)`. Negation paths (Stranger's
   Fortune, Expert Loading) and effective overlays never reach this point
   (unchanged).
2. `Firing/FreeActionFullAttackReloadPatch`: before each iterative shot, if
   the previous completed shot of this command committed a degradation of the
   same exact weapon → `EndRemainingAttacks` (existing helper, native Success
   semantics) BEFORE the loaded/continue or free-reload decisions. The
   misfiring shot itself resolves completely (round, miss, burst via
   FinishAttack).
3. `Firing/EmptyFirearmAttackCommandPatch`: construction gate runs before the
   empty/Wrecked/auto-reload policy (no replacement reload is queued for an
   interrupted sequence); new `RejectInterrupted` disposition + counter +
   player-facing message; pending reload-resume continuations capture the
   degradation epoch and cancel at resume if it advanced; new Harmony prefix
   on `ClickUnitHandler.OnClick` marks the player-attack frame.
4. Harmony 1.2 constraint discovered: **no finalizer support** (4-arg Patch),
   so the player-attack context is frame-scoped (`UnityEngine.Time.frameCount`)
   instead of a depth counter — leak-proof if the click handler faults.
   Residual same-frame edge: an AI construction in the exact frame of a player
   unit-click passes the gate (one-frame window); accepted for P1, to be
   observed in native A02/A04 qualification. Console/radial-menu attack-issuance
   routes may need additional player-intent entry points (native verification
   will tell).

Test-count bookkeeping (learned the established mechanism):
- Active validator `tools/validate_word_of_recall_oracle126.py`
  DETERMINISTIC_TEST_COUNT 1581→1592.
- `validation/static-validation.json`: the active-chain blocks
  {unifiedRepair121, characterVisibility123, teleportPolish124,
  settlementButton125, wordOfRecallOracle126} carry the CURRENT count (they
  are live current-expectation records updated per release — e.g.
  teleportPolish124 block=1581 vs its module literal 1574); deeper historical
  blocks stay frozen. P4's 0.0.127 validator supersedes this interim bump.

Results @ worktree (base 71af37ac + records commits + this slice):
- `scripts/test-domain.ps1 -Configuration Release`: **1592/1592 PASS, exit 0**
  (baseline 1581 + 11 new `broken-sequence.*` cases: 7 pure-policy +
  4 source-contract wiring).
- Main project clean Release Rebuild: OK (compile of the Kingmaker-dependent
  wiring, which the domain suite does not compile).
- `scripts/validate-repository.ps1`: PASS (via test-domain).
- Acceptance A-rows: domain-layer evidence recorded in
  docs/FIREARM-MAINTENANCE-ACCEPTANCE.md as PARTIAL for A01/A02/A04/A05/A07;
  all native cells remain NOT RUN. No runtime launches yet.

Next: P2 rest-restoration slice (StopRestProcess prefix coordinator +
service), then P3 field-repair restriction; native qualification batch at P4.

---

2026-09-12 #4 — P2 implemented: completed-rest firearm maintenance (domain green)

New files:
- `src/KingmakerGunslinger/Gunsmithing/CompletedRestMaintenancePolicy.cs`
  (pure): EvaluateRest (genuine-completion gate: RestSucceeded &&
  !NightRandomEncounter && !SkipTime, then capable-repairer and kit checks),
  EvaluateItem (Normal skip / Broken restore / Wrecked restore, invalid
  throws), ShouldReportBlocked, DescribeRestored/DescribeBlocked honest
  summaries.
- `src/KingmakerGunslinger/Gunsmithing/CompletedRestMaintenancePatch.cs`:
  Harmony prefix on `RestController.StopRestProcess` (attribute-discovered;
  applied by `harmony.PatchAll` like CraftingRestResetPatch). Once-per-rest
  guard keyed by `ReferenceEquals` on the RestStatus instance; only genuine
  completions proceed. Participants = `Game.Player.AllCharacters`; capability
  = `HasFact(BlueprintBootstrap.GunslingerClass.Gunsmithing)` (real feature
  fact, all grant routes covered, no class-level shortcut); kit =
  `KingmakerRepairKitInventory(Player.Inventory, GunsmithKit).Count() > 0`
  (never consumed). Scope = shared inventory `Items` + participants'
  `Body.AllSlots` (covers current + alternate weapon sets + additional
  limbs), deduplicated by concrete reference. Restoration per item via
  expected-state-guarded `FirearmRuntimeState.Service.Transition` +
  `FirearmStateMachine.Repair` (same transition Quick Clear uses; no fresh
  items; identity/round preservation inherited). Per-item try/catch with
  exact failure logging; coordinator faults never break the native rest.
  One aggregate summary per rest via native combat log + mod log; one
  blocked explanation when damaged guns exist without gunsmith/kit. Runs at
  the termination boundary BEFORE StopRestProcess's trailing autosave, so
  restoration is captured by the post-rest autosave (R10 helper).
  `CraftingRestResetPatch` untouched (independence asserted by test).

Native-verification risks recorded for the runtime lanes:
- Interrupted-then-resumed rest: if the resumed session REUSES the same
  RestStatus with NightRandomEncounter still latched, the completion gate
  would never fire. Fallback plan if observed: move the marker to the
  TickSleepPhase completion branch (RemainingTime<=0 && !encounter). R06.
- Scripted rests (StartScripted/m_ScriptedRest) may terminate without
  StopRestProcess; world-map/inn routes unverified. R08.
- `RestCompletionsSeen`/`MaintenanceRuns` counters exist for runtime
  observation.

Test-count bookkeeping: validator + active-chain blocks 1592→1601.

Results @ worktree (P1 commit 97ff28d6 + this slice):
- `scripts/test-domain.ps1 -Configuration Release`: **1601/1601 PASS, exit 0**
  (+9 `rest-maintenance.*` cases: 6 pure-policy + 3 source-contract wiring).
- Main project clean Release Rebuild: OK.
- Repository validation: PASS (within test-domain run).
- Acceptance R-rows: domain PARTIAL recorded for R01/R04/R05/R06/R07/R09;
  R02/R03/R08/R10 remain fully NOT RUN (need native/scope/persistence
  evidence). No runtime launches yet.

Next: P3 field-repair restriction slice.

---

2026-09-12 #5 — P3 implemented: field repair Broken-only + out-of-combat (domain green)

Source changes:
- `Recovery/FirearmRepairStatus`: +`WreckedRequiresRest = 5`.
- `Recovery/FirearmRepairTransactionService`: field entry now rejects
  Wrecked before every other check (last line of defense; the ordinary
  ability cannot restore Wrecked through the generic transaction). The
  completed-rest route keeps its own guarded-transition entry (P2), and
  Quick Clear keeps `FirearmStateMachine.Repair` (shared low-level
  transition, combat-usable, untouched).
- `Actions/FirearmActionPolicy`: repair ordering is combat → Wrecked →
  Normal → kit, with the mission's reason strings; reload decisions
  unchanged (old 4-arg Evaluate overload delegates with inCombat=false).
- `Recovery/RepairTestMusketRuntime`: combat gate via
  `Game.Instance.Player.IsInCombat` (native party-level authority — an
  active party encounter rejects repair even if the caster is not
  personally engaged); availability text now Broken-only.
- `Recovery/RepairCommandStartBinding.cs` (new): Harmony prefix on
  `UnitUseAbility.OnStart` binds the exact equipped firearm + start
  eligibility (non-combat, kit, Broken) when the repair ability command
  begins; postfix on `UnitUseAbility.OnEnded` clears it.
  `RepairTestMusketAbilityLogic.TryPrepare` now requires
  `ReferenceEquals(boundAtCommandStart, start.Weapon)` — the concrete
  target is anchored at genuine command commencement, closing the
  back-to-back-delivery-checks gap (F04).
- Active user-facing texts updated to the new contract: ability, feature,
  kit, legacy-alias descriptions; availability reasons; GetReason;
  DevelopmentUi; Main bootstrap log; blueprints.json manifest notes.
  Legacy Overhaul alias delegates to the same logic (inherits all checks).
- Dev accelerated fixture (`KingmakerDevelopmentBridge.Sprint29` +
  `MaintenanceQualificationService`/`Baseline`): seeds an empty
  misfire-BROKEN target instead of Wrecked; stage classification updated
  (Broken=FixtureReady, Wrecked=Failed/rest-only).

Validator-chain maintenance (established patterns, each recorded):
- tokens in `validate_sprint29` (runtime text, dev-harness seed) and
  `validate_sprint30` (policy reason) updated to the new contract strings.
- `blueprints.json` repair-entry notes sit inside hash-pinned manifest
  prefixes (entries[:1869], entries[:1872] in validate_teleportation118/119);
  the same notes were deliberately changed by the 0.0.121 unified-repair
  release (pins were re-pinned then). Re-pinned both digests for the new
  notes: 1869=34ccf639d25e981b5263bd4de855c3fa38659b7d0206c2a0448487d01b4ade0f,
  1872=d63a4cdd831276e7173ab53aac6fd135d5ccbb1bbf6ab0c56a14eb2daaf8138e.
- DETERMINISTIC_TEST_COUNT + active-chain static blocks → 1611.
- Stray UTF-8 BOM introduced into blueprints.json by an edit script —
  detected by validator ("Unexpected UTF-8 BOM"), stripped.

Obsolete expectations explicitly replaced:
- Sprint29 `repair.transaction.wrecked-to-normal` now asserts
  WreckedRequiresRest + zero mutation; repeated-cycles test now misfires
  from Normal each cycle (Broken→Wrecked is rest-only);
  `MaintenanceRemovedOverhaulStageRejected` now asserts Wrecked is not a
  loop stage; fixture/baseline tests use empty-Broken.
- Sprint30 `generic.repair.wrecked-available` now asserts rejection.
- `FourthPlaytestOverhaulMaintenance` asserts equipped-Broken runtime text
  + party combat authority + Wrecked mention in the blueprint text.
- `UnifiedFirearmRepairTests` text/manifest assertions updated.
- New `FieldRepairRestrictionTests` (10 cases): combat beats all
  eligibility, Wrecked rest-only, Normal/kit rejection, reload unaffected,
  transaction defense, party-combat authority wiring, command-start
  binding wiring, legacy alias, no active Wrecked-field text anywhere
  (incl. manifest), Quick Clear route intact.

Results @ worktree (P2 commit 72388f66 + this slice):
- `scripts/test-domain.ps1 -Configuration Release`: **1611/1611 PASS, exit 0**
- Main project clean Release Rebuild: OK.
- Acceptance F-rows: domain PARTIAL recorded (F01/F02/F05 partially;
  F03/F04 policy+wiring only) — native cells all NOT RUN. No runtime
  launches yet.

Next: P4 (docs/version 0.0.127 validator chain, full Build-Local pipeline,
package validation, guarded native lanes ×2) then P5.
