# Firearm Maintenance — Behavioral Contract

Status: **ACTIVE DESIGN (approved, locked)** — implemented by mission
`Z-FIREARM-MAINTENANCE`. This document is the lasting developer/player contract
for firearm maintenance, full-rest recovery, and misfire interruption. The
approved behavior comes from `Z-FIREARM-MAINTENANCE-MISSION.md` §4 and does not
weaken to match implementation results; deviations require owner review.

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

### [BASELINE] Current behavior — to be completed by P0 trace

(pending; record current Wrecked-eligibility, combat check, and binding
behavior in the journal and mirror the summary here)

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

### [BASELINE] Current behavior — to be completed by P0/P2 trace

(pending; record the native rest completion route(s) found and current absence
of rest restoration)

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

### [BASELINE] Current behavior — to be completed by P0/P1 trace

(pending; record what currently happens after a mid-sequence break today)

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
