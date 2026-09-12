# Firearm Maintenance — Behavioral Contract

Status: **IMPLEMENTED (0.0.127 candidate; domain-qualified 1611/1611, native
qualification in progress)** — implemented by mission `Z-FIREARM-MAINTENANCE`.
This document is the lasting developer/player contract for firearm
maintenance, full-rest recovery, and misfire interruption. The approved
behavior comes from `Z-FIREARM-MAINTENANCE-MISSION.md` §4 and does not
weaken to match implementation results; deviations require owner review.

Sections below keep both the approved `[CONTRACT]` text and the verified
`[BASELINE]` pre-mission behavior. Implementation status (source map of
what changed): §1 field repair — `FirearmActionPolicy` (combat → Wrecked →
Normal → kit ordering), `FirearmRepairTransactionService` (Wrecked rejected
before all else; `WreckedRequiresRest` status), `RepairTestMusketRuntime`
(`Player.IsInCombat` gate), `RepairCommandStartBinding` (Harmony prefix on
`UnitUseAbility.OnStart`, cleared on `OnEnded`; delivery requires the same
concrete reference); §2 rest maintenance — `CompletedRestMaintenancePolicy`
(pure) + `CompletedRestMaintenancePatch` (prefix on
`RestController.StopRestProcess`, once-per-RestStatus, participants =
`Player.AllCharacters` with `HasFact(Gunsmithing)`, kit via shared
inventory, scope = inventory `Items` + `Body.AllSlots` deduplicated);
§3 interruption — `BrokenSequenceInterruptionPolicy` (pure) +
`BrokenSequenceSuppressionRuntime` (weak-keyed epochs + frame-scoped
player-attack marker via `ClickUnitHandler.OnClick` prefix) wired at
`FirearmMisfireRuntime.CommitConditionTransition`,
`FreeActionFullAttackReloadPatch` (end-before-next-shot gate), and
`EmptyFirearmAttackCommandPatch` (construction gate, epoch-checked resume);
§4/§5 — unchanged protections re-verified by tests (`FieldRepairRestrictionTests.QuickClearRouteStaysCombatUsable`,
rest of the 30-case mission suite).

Investigation status markers: `[BASELINE …]` describes current (pre-mission)
verified behavior; `[CONTRACT]` describes the approved target. Native boundary
notes record the verified hooks once traced.

---

## 1. Field repair (Repair Firearm action)

### [CONTRACT] Approved behavior

- Identity, name, usable icon, and **full-round** action economy unchanged.
- No hour-long timer, no campaign-time advancement, no consumable kit, no
  gold/grit charge, no link to ammunition crafting's once-per-rest allowance.
- Allowed only when all hold:
  1. a legitimate character with the existing Gunsmithing repair capability can
     act (capability resolved from real feature/provider identities and
     supported grant routes — not `Gunslinger class level > 0` and not a
     retired alias fact);
  2. the character is **outside active combat** (native combat authority that
     disallows repair during an active party encounter even if the character
     personally hasn't entered combat — proven in both RTWP and turn-based);
  3. **at least one reusable Gunsmith's Kit** is in shared inventory
     (at-least-one; no per-gun limit; retired consumable kits do not satisfy);
  4. the one exact equipped repair target required by the current resolver is
     bound, and its **actual persisted condition is Broken** (Normal and
     Wrecked are ineligible).
- Availability is checked in the UI, at actual command commencement, and
  immediately before mutation. The concrete firearm is bound at command start
  and rechecked at delivery (two back-to-back checks inside delivery do not
  prove identity across the full-round interval). Combat start, cancellation,
  inability to act, kit/capability loss, or target/context change before
  delivery must prevent repair and change no state. Expected ineligibility
  resolves as a normal rejection, not an exception-log flood.
- Repair preserves item reference/identity, enchantments, origin/ownership
  metadata, loaded-ammunition identity, and surviving round count. Never refund
  a misfired round; never substitute a fresh basic instance for a magic gun.
  Consumes nothing on success or failure.
- User-facing reasons include at least: "Cannot repair firearms during
  combat," "This firearm is Wrecked and requires a completed full rest," and a
  missing-kit explanation. Availability inspection must not mutate state.

### [BASELINE] Current behavior (verified P0 trace, base 71af37ac)

- `Actions/FirearmActionPolicy.EvaluateRepair` accepts **Broken and Wrecked**
  ("Only a Broken or Wrecked firearm can be repaired.").
- `Recovery/FirearmRepairTransactionService.GetRejection` likewise permits
  Broken and Wrecked at the transaction layer; kit snapshot equality and
  same-state rollback already present.
- `Recovery/RepairTestMusketRuntime` has **no combat check at all**; it
  resolves the one exact equipped firearm (persisted condition via
  `FirearmRuntimeState.Service`, not the battered effective overlay), requires
  ≥1 kit in `Game.Instance.Player.Inventory`, and executes the transaction.
- `Recovery/RepairTestMusketAbilityLogic` is the full-round Standard ability
  (`SetIsFullRoundAction(true)`). `IsAvailableFor` gates the UI; `Deliver`
  runs `TryPrepare` (evaluate at delivery start) then `Complete`
  (re-evaluate + `ReferenceEquals` weapon match) then `Execute`. **Both checks
  are inside delivery** — nothing binds the concrete firearm at actual command
  issuance, so a weapon/context change across the full-round interval is only
  caught by the two back-to-back delivery checks (P3 gap per mission §4.1).
- Quick Clear (`Deeds/QuickClearRuntime` + `QuickClearService`) is already
  Broken-only, actual-condition based, uses `FirearmStateMachine.Repair`
  directly (shared low-level transition, combat-usable), spends grit via
  TrueGrit. Untouched by this mission's field-repair restriction.
- Legacy Overhaul alias (`Blueprints/OverhaulTestMusketAbilityBlueprints`) is
  hidden, `ActionBarAutoFillIgnored`, and delegates to
  `RepairTestMusketAbilityLogic.Create` — it inherits whatever field checks the
  unified ability has.
- User-facing text still promises unified Broken-or-Wrecked full-round repair
  in `GunsmithingBlueprints`, `GunsmithingSupplyBlueprints`,
  `RepairTestMusketAbilityBlueprints`, the availability reason strings, and
  `blueprints/blueprints.json` manifest notes (P4 updates these; the
  `UnifiedFirearmRepairTests` source-contract assertions are updated with
  them).

## 2. Rest maintenance (the Wrecked/Broken recovery route)

### [CONTRACT] Approved behavior

- Fires only after a **genuine successful full rest** (native rest completion
  boundary; interrupted-then-resumed completes ⇒ maintenance applies once).
  Opening rest UI, starting, cancelling, waiting, travel/time advancement,
  fatigue removal, partial rest, or pre-completion interruption never qualify.
- Requires a participating, legitimately capable gunsmith with shared-kit
  access (native rest participation and post-rest capability — not the whole
  roster, not a transient sleeping flag).
- Scope: eligible firearms in shared carried inventory plus the participating
  party's equipment/alternate weapon sets. Deduplicate the same concrete item
  across collections. A Wrecked gun need not stay equipped. One gunsmith + one
  reusable kit cover the party's eligible carried firearms.
- Excluded: remote stashes, merchant stock, ground loot, unrelated units,
  nonparticipant equipment (use actual ownership/container/participant
  evidence, not global item scans).
- Broken and Wrecked become **Normal directly**. Loaded rounds survive a
  Broken repair; Wrecked stays empty. Normal items untouched. Same identity and
  transaction protections as field repair (exact-item, atomic per item).
- Repeat callbacks / multiple participant callbacks / re-entrancy run
  party-wide maintenance once per completed rest, with no duplicate messages or
  consumption; once-only semantics must not let an early ineligible
  participant block a later valid gunsmith.
- Per-item atomic restoration; one failing target never rolls back the native
  rest or already-repaired guns; exact failure logged; recovery calls safe;
  rest completion never breaks.
- One concise maintenance summary per completed rest with actual changes; a
  clear single explanation when damaged carried guns can't be restored for
  missing gunsmith/kit (not per participant); never report uncommitted repairs.

### [BASELINE] Current behavior and native rest boundaries (verified P0 trace)

- No rest-based firearm restoration exists today. The only rest hook is
  `Gunsmithing/CraftingRestResetPatch`: a per-unit postfix on
  `RestController.ApplyRest(UnitDescriptor)` that removes the ammunition
  crafting once-per-rest marker — appropriate for its purpose, **not** a
  full-rest completion boundary.
- Native rest state machine (IL dump at
  `private\charvis-native-il\Assembly-CSharp.il`, game 2.1.7b):
  - `RestPhase`: Manage=0, Camp=1, Sleep=2, Finished=3, SkipTime=4.
    `RestResult` flags: Unknown=1, Success=2, HuntingRandomEncounter=4,
    NightRandomEncounter=8, SkipTime=16.
  - `RestController.Tick()` dispatches per phase: Camp → `CampCoroutine` +
    `TickCamp` (hunting/camping checks happen here); Sleep → first tick
    `StartSleepPhase`, then `TickSleepPhase`; SkipTime → `SkipTime()` then
    phase=Finished + `StopRestProcess`.
  - `TickSleepPhase()`: if `Status.NightRandomEncounter` → phase=Finished +
    `StopRestProcess` (interrupted). Otherwise sets `Status.RestSucceeded =
    true` each tick; when `RemainingTime <= 0` → phase=Finished +
    `StopRestProcess` (**genuine completion**). Otherwise advances game time
    by the interval and — while sleeping — calls `ApplyRestInterval()` only
    if `Status.ApplyRest` (incremental per-interval healing; a **transient
    mid-sleep** callback, not a completion boundary). Supports
    `RestUntilHealed` extension.
  - `ApplyRestInterval()` is party-wide: iterates `Player.AllCharacters` +
    `ExCompanions`, calls `HealAndApplyRest(unit, status)` which heals and
    then calls `ApplyRest(unit.Descriptor)` per unit.
  - `ApplyRest` is **also called natively outside camping** by
    `LevelUpController`, `KingdomTimelineManager`, `KingdomTask`, `Recruit`,
    `RespecCompanion`, and `CapitalCompanionLogic` — so any per-unit
    `ApplyRest` postfix is not proof of a rest, let alone completion.
  - `StopRestProcess()` (iterator `<StopRestProcess>d__76`) is the common
    termination coroutine started at the completion boundary via
    `LoadingProcess.StartLoadingProcess`: it checks
    `RestSucceeded`/`SkipTime`, ticks entity creation/destruction, preloads
    unit resources, and **autosaves at the end**
    (`SaveManager.GetNextAutoslot` → `SaveGame`).
  - **P2 integration point (candidate, to be runtime-verified):** a prefix on
    `RestController.StopRestProcess()` — invoked exactly once per rest-process
    termination — gated on `Status.RestSucceeded && !Status.NightRandomEncounter
    && !Status.SkipTime`. It runs before preload/autosave, so committed
    maintenance is captured by the post-rest autosave. An interrupted rest
    that later resumes completes through the same path exactly once (R06).
  - `StartScripted(bool immediate)` is a scripted/instant rest route that
    also calls `ApplyRestInterval` (m_ScriptedRest). World-map and
    settlement/inn routes to be confirmed at runtime for R08.

## 3. Newly Broken stops the current attack sequence

### [CONTRACT] Approved behavior

- Interruption triggers on a **verified committed degradation of the exact
  firearm during the current attack sequence**, not merely an effective Broken
  condition (battered ownership overlays don't count; misfire negation that
  prevents a break must not interrupt).
- The misfiring shot finishes resolving under existing rules (round
  expenditure, miss, condition change, burst, diagnostics). The **next real
  shot** and any associated automatic reload are prevented before they start.
  No refunds of completed attacks/resources; no fresh native action economy.
- Scope: remaining iterative/Haste/extra attacks, pending reload-resume work,
  real-time repeated auto-attack orders, and target-replacement continuation of
  the interrupted order. Not a global clear of the character's/party's
  commands; unrelated movement, abilities, other characters, and ordinary
  weapons untouched.
- A later **genuine player-issued** attack with the Broken gun remains possible
  under native action economy and existing Broken penalties/misfire rules.
  Broken must not become permanently unusable, and a deliberate sequence is not
  cancelled merely for starting Broken. The native player-intent route must be
  traced separately from automatic command recreation
  (`CreateAttackCommand`-style internal calls are not consent).
- Automatic target changes and reload completion are not renewed consent;
  reload/Quick Clear may restore readiness but never resurrect the cancelled
  order. Weapon switching, explicit later orders, death, area change, command
  cleanup, and save/load must not leak suppression to unrelated future
  commands or revive stale continuations. Prefer native cancellation semantics
  and narrowly scoped command state; no fourth persistent firearm condition;
  no replacement of the firearm-token schema.
- Composite firing deeds (Dead Shot/scatter and other custom attack paths)
  keep their existing discharge model: multiple component rolls of one shot are
  not automatically multiple shots to cancel. Invariant: no subsequent real
  discharge or automatic resumption after the applicable committed break.

### [BASELINE] Current behavior and native command boundaries (verified P0 trace)

- Nothing today interrupts an attack sequence after a committed break:
  - Full attacks: `Firing/FreeActionFullAttackReloadPatch` prefixes
    `UnitAttack.OnAction` (instance, per-iterative-shot boundary where
    `LastAttackRule` identifies the previous completed shot and
    `PlannedAttack` the next). `Reloading/FullAttackAutoReloadPolicy` returns
    `ContinueLoaded` whenever rounds remain — **a gun that just broke
    mid-sequence keeps firing its remaining iteratives** (Broken preserves
    surviving rounds after the misfired round was consumed). If empty and the
    effective reload is Free (or free Lightning Reload), it **reloads and
    continues the sequence**. Only Wrecked-effective or unavailable reload
    ends the attack.
  - Command construction: `Firing/EmptyFirearmAttackCommandPatch` prefixes
    static `UnitAttack.CreateAttackCommand(executor, target)` and rejects
    empty/Wrecked/ambiguous at construction; a loaded Broken gun constructs
    freely (deliberate Broken attacks are already possible today and remain
    so under the contract).
  - Reload-resume: when an empty-gun attack is replaced by the auto-reload
    ability, a `PendingAttack` is stored keyed by the reload
    `UnitUseAbility`; `ReloadEndedPostfix` on `UnitUseAbility.OnEnded(bool)`
    schedules `ResumeAttack`, which re-checks same-weapon, loaded, non-Wrecked
    effective, paper-mode, and turn-based standard-action availability —
    **a Broken gun resumes the interrupted order after reload today**.
- Committed degradation point: `Misfires/FirearmMisfireRuntime.CommitConditionTransition`
  (invoked from `AfterIsSuccessRoll` on the eligible `RuleAttackRoll`) runs an
  expected-state-guarded `FirearmRuntimeState.Service.Transition` with
  repository-identity verification and publishes the condition notification.
  This is the only place Normal→Broken / Broken→Wrecked commits during an
  attack — the natural, verified trigger for P1 interruption. Misfire
  negation (Stranger's Fortune `TryIgnoreMisfire`, Expert Loading) returns
  before it, so a prevented break never reaches the trigger (A07 baseline
  already safe).
- Discharge gating: `Firing/FirearmDischargeRuntime.BeforeAttackRoll`
  consumes the round via the state machine (`Fire`), registers the eligible
  attack, and forces a miss for empty/Wrecked/fault; `DeadShotRuntime` and
  `ScatterVolleyRuntime` bypass ordinary discharge for their composite paths.
- Player intent vs automation (IL evidence):
  - Player attack clicks: `ClickUnitHandler.OnClick` → (auto-use ability
    path or) `UnitAttack.CreateAttackCommand` → `UnitCommands.Run(command)` +
    `CombatState.ManualTarget = target`. **The attack branch does not set
    `UnitCommand.CreatedByPlayer`** — that flag is only set for
    movement/interact/auto-use-ability commands (ClickGroundHandler,
    ClickMapObjectHandler, ClickUnitHandler other branches, console
    InGameInputLayer, AreaTransition). `CreatedByPlayer` therefore cannot
    discriminate deliberate vs automatic attack commands.
  - RTWP repeated auto-attacks are issued by the Brain:
    `BlueprintAiAttack` builds attack commands, and
    `DecisionContext`/`TargetInfo`/`HasManualTargetConsideration`/
    `ManualTargetConsideration` read `ManualTarget`. `UnitConfusionController`
    and `StalkerUnitController` also create attack commands.
  - Consequence for P1 design: suppression must be scoped to the interrupted
    command/order (cancel remaining iterations, cancel pending resume, block
    immediate automatic re-issue) while a genuinely new player-issued order
    passes; candidate consent signals are the native click event bus and
    `ManualTarget` semantics, to be settled in P1 with focused tests.
  - `UnitCommands` API available for native cancellation: `Run`,
    `AddToQueue(First)`, `AddToQueueOrRun`, `InterruptAll`,
    `InterruptAiCommands`, `InterruptAndRemoveCommand`, `InterruptMove`,
    `InterruptGroupCommand`, `IsRunning`, `UpdateCombatTarget`.
- Battered overlay (`Gunsmithing/BatteredFirearmRuntimeUseResolver` +
  `BatteredFirearmUsePolicy`): computes an **effective** Broken overlay from
  battered origin ownership; kept separate from the persisted actual
  condition everywhere above (repair eligibility reads the actual condition).

## 4. Wrecked is unusable for firing

### [CONTRACT] Approved behavior

- Layered rejection at command construction, queued/ongoing delivery,
  auto-reload, individual discharge, state transitions, and firearm-firing
  deeds. A Wrecked firearm never initiates a shot, consumes firing
  ammunition/grit, emits an invalid projectile, or deals firing damage.
  Low-level forced miss is a fallback only, not user-facing behavior.
- A command legal when queued whose gun becomes Wrecked before its next shot is
  rejected before that shot's effects/resource commitment; native rules for
  already-spent action economy stand (no invented refunds). A Broken→Wrecked
  misfire resolves its one legitimate consequence/burst exactly once; no
  further firing.
- Rest is the Wrecked maintenance route; field repair and the legacy saved
  Overhaul alias must not bypass it. No global disabling of unrelated
  abilities; no redesign of firearm-as-improvised-melee rules. Other
  explicitly implemented approved recovery mechanics preserved as-is.

## 5. Compatibility invariants

- **Quick Clear** keeps its combat availability, standard/move versions, grit
  rules, True Grit interactions, actual-condition restrictions, and Wrecked
  rejection. It never goes through field-repair eligibility; any shared
  low-level repair transition must remain combat-usable by Quick Clear.
- Ammunition crafting/grant/reset behavior, vendor cleanup, retired consumable
  items, blueprint IDs, save-token IDs, enchantments, and battered origin
  unchanged.
- Exactly one visible maintenance action. The legacy Overhaul blueprint stays
  registered, hidden/autofill-ignored, and routed through the same
  **Broken-only out-of-combat** field checks if a saved fact/slot invokes it.
- Unchanged by this mission: misfire chances, Broken penalties, burst balance,
  free/Lightning Reload costs, proficiency, prices, damage. No renaming of
  `TestMusket` types, no persistence-architecture replacement, no inventory
  UI, no new systems/settings, no unrelated cleanup.

## 6. Source map (verified against worktree @ 71af37ac)

| Area | Files under `src/KingmakerGunslinger/` | Role / current notes |
| --- | --- | --- |
| Field repair | `Actions/FirearmActionPolicy.cs`, `Recovery/RepairTestMusketRuntime.cs`, `Recovery/RepairTestMusketAbilityLogic.cs` | Capability, combat authority, command-start binding, delivery checks, display reasons. |
| Exact-item transaction | `Recovery/FirearmRepairTransactionService.cs`, `Recovery/FirearmItemRepairStateStore.cs`, `Firearms/FirearmStateMachine.cs` | Field vs rest authorization (rest route to be added), rollback, preserved rounds. |
| Rest | `Gunsmithing/CraftingRestResetPatch.cs`; native rest completion callers | Candidate integration point only; full-rest completion boundary to be traced. |
| Misfire | `Misfires/FirearmMisfireRuntime.cs`, condition services, negation/consequence paths | Committed exact-item degradation, attack context, deduplication. |
| Full attacks | `Firing/FreeActionFullAttackReloadPatch.cs`, `Reloading/FullAttackAutoReloadPolicy.cs` | `UnitAttack.OnAction` boundary, native completion, later deliberate Broken orders. |
| Auto-continuation | `Firing/EmptyFirearmAttackCommandPatch.cs` | Pending reload callbacks, saved targets, real-time command recreation, genuine input intent. |
| Firing safety | `Firing/FirearmDischargeRuntime.cs`, discharge service, reload policy, custom deeds/scatter | Command gate and defense-in-depth. |
| Recovery exceptions | `Deeds/QuickClearRuntime.cs`, `Deeds/QuickClearService.cs`, feature providers | Shared repair transition must stay combat-usable via Quick Clear. |
| Identity/ownership | `Actions/ExactEquippedFirearmResolver.cs`, `Firearms/FirearmRuntimeState.cs`, `Gunsmithing/` battered-origin/use policies | Concrete item references; persisted condition vs effective overlay. |
| Publication/UI | Repair/Overhaul/Gunsmithing blueprint builders, ability providers, condition tooltips, kit descriptions | Remove every active promise of full-round Wrecked repair. |

This map is the starting point, not the full call graph; the P0 trace extends
it with actual callers and native boundaries.

## 7. Change history

- 2026-09-12: initial contract from mission §4; source map verified. Baseline
  sections intentionally pending P0 trace.
- 2026-09-12 (P0 trace complete): filled all three [BASELINE] sections with
  verified current behavior and native boundaries (rest state machine,
  StopRestProcess completion coroutine, ApplyRest non-camping callers,
  ClickUnitHandler/Brain attack-issuance routes, CreatedByPlayer coverage,
  UnitCommands cancel API). Domain baseline 1581/1581 PASS @ 71af37ac in the
  mission worktree.
- 2026-09-12 (P1–P3 implemented): status header now maps each contract
  section to its implementing sources (see above). Domain suite
  1611/1611 PASS. Active texts, manifest notes, KNOWN-ISSUES design
  question, changelog, and the player smoke test updated to the new
  contract. Native qualification tracked in
  `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`.
