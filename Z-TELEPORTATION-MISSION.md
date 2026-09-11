# Z mission: complete and stabilize Kingmaker teleportation

## Objective and completion contract

You are Z, working in the existing DATA checkout of `howardreith/KingmakerGunslingerAndOtherTabletopRules`.

Complete the existing World-Map Teleportation Spells feature so that:

1. After teleporting, the first normal use of a legal directional movement arrow works without another action to repair movement.
2. Eligible Conjuration specialists can prepare and spend Teleport and Greater Teleport in their real specialist/favorite-school slots.
3. Contextual spell controls fit within the native menu and clearly distinguish settlement teleportation, spellbook casting, and scroll activation.
4. Existing campaigns can buy, learn from, and activate genuine Teleport, Greater Teleport, and Word of Recall scrolls, with correct native costs and persistence.
5. The integrated candidate passes the required checks, is packaged and guarded-installed for the owner, and has a reproducible handoff.

These are the goal's acceptance gates, not optional suggestions. A plan, compiling code, an intermediate milestone, time spent, or partial coverage does not complete the goal. Continue through independently actionable work without asking the owner to type “continue.” A genuine external blocker or quota interruption means PAUSED/BLOCKED, not COMPLETE.

The owner says teleportation is otherwise working well. Preserve its working mechanics. This assignment is a focused completion and repair, not a rewrite of the whole mod.

This is a multi-session mission. Use the durable-state protocol below from the first round. Do not try to fit the whole task into one quota window or sacrifice required testing to finish before a limit.

## 1. Establish the real baseline and preserve other work

Read the current `AGENTS.md` and applicable build, package, runtime, deployment, save-safety, and branch-publication instructions. Distinguish historical versions/checkpoints from current requirements. This mission explicitly assigns the new teleportation work; it does not assign unrelated backlog items.

Inspect the actual checkout, remote, branch/worktree, dirty files, relevant newer commits, active runtime transactions, and installed DLL/package identity. The source investigation for this mission inspected master `9d1072beb9ed100b4b012afa5d194f4621b7d3b1`, following release 0.0.120. That is evidence provenance, not an instruction to check out or reset to it. The installed build and task branch may be newer.

If this teleportation task is already underway, continue its branch/worktree and preserve completed work. Do not create a competing implementation. Keep other work, including firearm-repair changes, intact; do not merge unrelated branches autonomously.

Use a dedicated approved feature branch, never master/main. A `codex/*` branch name is acceptable for Z when the existing guarded publication tooling requires it; changing agents does not require renaming a branch. Do not invent a new prefix that breaks the existing allowlist.

Match the project's C# version, style, architecture, and test conventions. Add no runtime dependency. Reuse native adapters and existing publication, transaction, and testing infrastructure. Do not introduce broad frameworks for this task.

No destructive reset/clean, force-push, history rewrite, unrelated project mutation, or autonomous merge. Do not modify safeguards merely to make the mission run.

## 2. Durable state and multi-session execution

Keep two primary task documents at the active repository root:

- `Z-TELEPORTATION-MISSION.md`: this complete contract, with approved decisions and any explicit subsequent owner amendments.
- `Z-TELEPORTATION-STATE.md`: compact current progress, evidence, blockers, and exact resumption instructions.

When this mission arrives as an attachment, persist its full contents before implementation. If these files already exist, reconcile the newer assignment with existing progress; do not overwrite useful state or silently drop requirements. Maintain pointers to any other records the active repository workflow requires, rather than duplicating long journals.

The state file must include:

- Exact repository/worktree and branch; current HEAD and last pushed commit; dirty/untracked task files.
- Acceptance checklist for the five objective gates, distinguishing TODO, IN PROGRESS, IMPLEMENTED, NATIVE-VERIFIED, and FINAL-CANDIDATE-VERIFIED.
- Current hypothesis, evidence supporting/refuting it, failed approaches, and the next smallest discriminating experiment.
- Exact commands, exit codes, evidence paths/run IDs, source hashes, package/DLL hashes, installed identity, and scope of each passing test.
- Any active or interrupted runtime/deployment transaction: request ID, owned process identity, disposable save identity, original configuration/backup location, cleanup status, and safe recovery command.
- Remaining tests, real blockers, and the next concrete action/command. Do not write only “continue debugging.”

Checkpoint after each meaningful experiment or implementation slice and before/after any deployment, save-bearing test, or temporary profile change. Do not rely on a final message at quota exhaustion: the interruption may be abrupt. Use the existing deterministic bounded harness, timeouts, and cleanup/rollback behavior; cleanup must not require another successful model response.

Commit coherent work only when the active required gates pass. Push through the approved workflow. Preserve incomplete uncommitted work and record it accurately rather than committing a known broken checkpoint or discarding it.

On every resumed round/session, read the mission and state first. Verify actual Git, files, installed build, processes, and transaction state against the record before repeating a command. A recorded “test started” is not a pass. Complete/recover an interrupted owned operation safely before starting another. Never rerun merchant grants, deployment, or save writes blindly.

Quota, context, and goal-budget interruptions do not authorize model/provider changes, extra spending, reset-card redemption, schedulers, busy-waiting, or alternate accounts. Checkpoint proactively and pause if resources become unavailable. Do not promise automatic restart after quota refresh. Resume at the next incomplete gate when the owner resumes the goal.

Avoid waste: do not reread the entire repository every round, rerun unchanged expensive native scenarios without reason, retry the same failed experiment without changing the evidence strategy, or launch multiple agents against the same game/fixture. A report is a checkpoint, not a stopping condition. When all independent safe work is exhausted behind a real blocker, record it and pause rather than loop indefinitely.

## 3. Gate 1 — restore directional movement immediately after teleport

### Exact player report

After teleportation, the on-screen arrow buttons around the party token do nothing. Clicking another map dot to initiate road travel, moving briefly, and stopping makes the arrows work again. These are the world-map movement arrows, not merely keyboard arrow keys or camera controls.

### Source-backed leads, not a confirmed diagnosis

Inspect the current equivalents of:

- `src/KingmakerGunslinger/Spells/Teleportation/TeleportationOutcomeWorld.cs`
- `TeleportExplorationGuardPatches.cs`, `TeleportExplorationBoundary.cs`, and `UnitPartTeleportFamiliarity.cs`
- `TeleportContextConfirmationPresenter.cs`, `TeleportationConfirmationSurface.cs`
- `WorldMapPointSpellActionRuntime.cs` and controller counterparts
- `TeleportationWorldSnapshot.cs`
- `docs/TELEPORTATION-NATIVE-FORENSICS.md`

The inspected relocation path marks a magical-arrival boundary, calls `SetCurrentPosition(new MapPosition(destination))`, then calls `UpdatePawnPosition()`. Its forensic notes describe native `TeleportParty` as also clearing/finishing travel and raising pawn events, but additionally revealing outgoing edges.

The exploration guard currently skips the entire native `LocationRevealController.Tick` while a saved magical-arrival boundary applies. Its suppression is released by conditions including actual ordinary walking. This matches the shape of the owner's recovery workaround, but does not prove the guard causes the defect.

Investigate both an omitted movement/direction refresh and suppressed native work needed by directional controls. Also check confirmation cleanup, input ownership, stale selection/route/edge caches, and orphaned UI raycast blockers. Do not assume an event or private field name from these leads; inspect the installed native methods.

### Required investigation

Reproduce the bug through a real contextual spell cast in an authorized disposable native fixture. Compare the same destination reached by ordinary travel, spell teleportation, and native settlement teleportation where available.

Capture structured state at these boundaries:

1. Stationary at the origin before casting, with a working legal arrow.
2. Immediately after relocation and again after native confirmation dismissal/normal UI lifecycle completion.
3. After the first attempted native directional-arrow action, before any workaround.
4. After the owner's click-dot/move/stop workaround, on a separate diagnostic attempt.

Observe canonical point, pawn/controller state, current edge/route/travel data, direction targets and their origin, cached selection, native action bindings, interactability/input ownership, confirmation state, and magical-arrival suppression. Determine what changes in the recovery sequence that was missing after teleport.

Trace the actual arrow callback and its refusal/no-op condition. Inspect the native movement, arrival, pawn-notification, and `LocationRevealController.Tick` paths using approved local references. If suppression affects only exploration, rule that hypothesis out rather than narrowing it without evidence.

Keep diagnostic mutations request-local to the disposable fixture. Do not experiment on the owner's campaign or ship broad diagnostic overrides.

### Solution requirements

Make completed relocation leave the native movement controller and directional controls synchronized with the ACTUAL arrival point, without pretending that ordinary travel occurred.

Prefer the smallest verified native refresh/notification or owned-state cleanup. If broad suppression of the reveal tick is causal, preserve the non-reveal work that movement requires while narrowly preventing the prohibited revelation side effects. Preserve the no-reveal contract; do not simply remove the guard.

Do not:

- Simulate a tiny walk, secretly click another dot, start/stop a dummy route, or advance time to make controls recover.
- Call the complete `TeleportParty` wrapper without proving and controlling all its reveal/gameplay effects.
- Reveal outgoing roads, set `EdgesOpened`/visited flags, or fake familiarity just to satisfy an arrow condition.
- Temporarily reveal the map and restore fields afterward; irreversible events or discoveries could already have fired.
- Clear all input/modal state or remove listeners belonging to another system.
- Run a permanent per-frame repair loop, repeatedly broadcast arrival events, or rebuild the entire map.
- Hide the defect by allowing only click-dot travel after teleport.

Share the corrected relocation/completion path across prepared, spontaneous, specialist, and scroll sources and all three spells. Refresh for an off-target/similar-location ACTUAL destination, not the originally requested destination.

If native modal/pawn ordering requires a deferred callback, make it one-shot, bounded, tied to the exact request/player/map/pawn/destination, and cancelled on scene/state changes. Do not allow a stale callback from one cast to overwrite a later cast or travel action. Controls must work at the first normal interactive opportunity, without an extra player action.

Preserve current conservative failure/compensation handling. An exception after relocation or a movement refresh is not permission to refund an uncertain partially completed cast.

### Decisive regression tests

After an actual contextual cast and ordinary dismissal, activate the FIRST legal arrow through its actual native bound control/handler and prove native route creation/progress along the intended edge from the new point. Do not “test arrows” by calling a lower-level road-travel helper that bypasses the broken handler. Prove the native control is correctly bound and available, not merely that pathfinding works.

Test stationary waiting frames before that first click, so no hidden recovery walk can occur. Separate pre-input invariants from post-input normal travel: teleport alone must not add time/mileage/revelation/familiarity; deliberate subsequent travel may produce its ordinary authorized effects.

Cover Teleport, Greater Teleport, Recall; repeated casts; ordinary and specialist preparation; spontaneous and scroll sources as implemented; off-target/mishap arrival; settlement and crossroads destinations with different legal directions; recent manual start/stop before casting; cancellation and no-relocation failure; desktop/controller paths; and fresh-process reload of a disposable save made immediately after teleport, before the workaround.

For blocked/unknown edges, retain native refusal. For legal travel, no extra click, reselect, restart, rest, or map re-entry may be required. Confirm native road travel and native settlement teleport still work, and no duplicate resource/arrival events were introduced.

## 4. Gate 2 — real Conjuration specialist/favorite-school slots

Teleport is reported rejected from the Conjuration specialty preparation slot. The inspected spell blueprints already declare Conjuration; a tooltip-only fix is not sufficient.

Inspect actual specialist-slot filtering, any separate school list, derived-list creation order, metadata/caches, compatible mods, and the world-map-only caster checker. Compare a working native Conjuration spell. Determine the demonstrated cause rather than assuming a missing school enum.

An eligible wizard who legitimately knows the spell must prepare Teleport in the fifth-level Conjuration bonus slot and Greater Teleport in the seventh-level bonus slot through the normal spellbook UI in a local area. Normal preparations remain valid. Ordinary rest readies the new preparation. A specialist-only remaining preparation must be counted and spendable from the world map.

Fix the existing canonical identities for already-created characters without forced respec, relearning, or new campaign. Also verify spells newly copied from the new scrolls.

If separate list publication is required, resolve verified identities and publish canonical spells once at correct levels using compatible reversible/idempotent behavior. Preserve foreign entries and handle initialization order/caches narrowly. If a casting check is wrongly blocking preparation, separate those concerns without enabling local-map casting.

Do not globally make school checks return true, manufacture a spellbook, add slots, auto-learn spells, refill resources, or change class-list levels. Keep all three spells' native school metadata correct, but do not publish Word of Recall into Wizard/specialist lists merely because it is Conjuration. Do not enable summoning-only effects for teleportation.

Prove mixed ordinary/specialist preparations count accurately and native spending consumes one legitimate use, not both. Preserve linked opposition-school costs and native spending order. Cancel/stale requests cost nothing.

Negative controls: other school slots reject the spells, universalists gain no bonus slot, unknown/wrong-level spells remain unavailable, unrelated Conjuration spells work, and Conjuration-opposed wizards retain native restrictions. Test ready and spent preparations across rest, fresh-process reload, relevant preview/cancellation, and module OFF/ON without free restoration.

## 5. Gate 3 — compact UI and native settlement coexistence

The owner's screenshots show the appended “TELEPORT LEINNA (2 PREPARED)” control extending beyond the parchment's inner margins. Another screenshot has both the native settlement “TELEPORT” button and “TELEPORT LEINNA (1 PREPARED),” making their meanings unclear.

Use native-styled compact controls within the padded content area. Preferred layout:

```text
[Destination]
[Road travel duration]

[Settlement Teleport]              only when natively available

[Cast Teleport]
  Leinna · 2 prepared

[Use Teleport Scroll]
  Leinna · 3 shared scrolls

[Travel]              [Cancel]
```

This is a presentation guide, not fixed pixel geometry. Show only relevant methods. A two-line title/detail row may be one focusable control. Use full spell names and distinguish costs. Include spellbook identity only when needed for ambiguity. No new submenu is required.

Measure the real native inner content rectangle, layout groups, padding, anchors/pivots, canvas scaling, preferred/minimum sizes, clipping, and actual hitboxes. Match native sizing; do not stretch the whole menu to preserve a verbose label, shrink the whole panel, or make text tiny. Handle long names/localization readably.

Keep spell rows above the Travel/Cancel footer where safely possible. Bound and clip long spell-source lists with scrolling while keeping native settlement actions and the footer reachable. Fit supported screen sizes and panel anchors, including compact 16:10 geometry comparable to the screenshots.

When both forms coexist, label the existing native action “Settlement Teleport” or an equivalent localized distinction. Preserve its callback, ownership, eligibility, prerequisites, native costs, and subsequent UI. Do not substitute the spell engine or charge a spell/scroll for settlement teleport. Do not claim native cost/time characteristics without verification.

Scope any native label/layout modification to this presentation and restore it safely. Never mutate a shared localization asset globally. A narrow caption is acceptable if safe relabeling is unavailable.

Travel stays the native default/initial confirm action. Preserve Cancel, Escape, Enter, controller focus/navigation/scrolling, resource/Enter actions, and foreign controls. Clarify that native journey-duration text describes road travel when magical options coexist; do not recalculate its value.

When the panel closes/changes or no augmentation applies, leave no extra rows, invisible blockers, stale callbacks, changed labels, layout offsets, or owned input locks. Test this jointly with Gate 1.

Require measured rendered bounds and native before/after images where supported. Images establish appearance, not mechanical correctness. Native structured control/callback evidence must establish behavior under the repository's guarded test rules.

## 6. Gate 4 — scroll items, acquisition, learning, and casting

### Standard items

Create real native scroll items pointing to the existing canonical spell abilities, not duplicate scroll-only spells.

| Scroll | Standard item level/type | Caster level | Base price |
|---|---|---:|---:|
| Teleport | 5, arcane | 9 | 1,125 gp |
| Greater Teleport | 7, arcane | 13 | 2,275 gp |
| Word of Recall | 6, divine | 11 | 1,650 gp |

These values are approved design choices. Retain native merchant price adjustments, identification, stacking, sale/buyback, and presentation. Register stable project-owned item identities in the existing manifest. No new art dependency is required.

Sell one standard cleric-created Recall scroll. Keep the spell's Cleric 6 / Druid 8 list levels; verify native activation eligibility where the same spell has different class-list levels.

### Vendors and finite stock

Verify exact native catalogs, progression, and shared references; do not guess GUIDs from names.

- Zarcie: finite batch of 5 Teleport scrolls at her verified fifth-level-capable tier, provisionally Arcane I.
- Zarcie: finite batch of 3 Greater Teleport scrolls at her verified seventh-level-capable tier, provisionally Arcane III.
- Arsinoe/applicable capital-era priest family: finite batch of 5 Recall scrolls. Include Jhod/settlement priests where they actually use that appropriate stock family.

Use the nearest verified native equivalent if provisional tier names are inaccurate, and document the actual unlock. Do not add a character-level purchasing requirement or leak capital-era stock into early Jhod stock through a shared reference.

When Zarcie's content is genuinely unavailable, use Hassuf for equivalent finite arcane batches at appropriate verified progression. Check kingdom auto-management; use its normal working acquisition path, or equivalent finite fallback if manual rank requirements would otherwise make acquisition impossible.

Do not infer content absence from Zarcie being unloaded, not yet unlocked, or outside the area, or from a broken Steam/DLC launch. Avoid simultaneous duplicate grants.

Do not add stock to Oleg, Bokken, the Skeletal Salesman, general loot, or other merchants in this mission.

Handle fresh AND already-generated merchant inventories. Reuse applicable existing publication/reconciliation mechanisms, but prove saved-stock migration works for these families. Each grant occurs once using native persisted tracking or narrow stable grant identities. Distinct eligible merchants can receive their batch; aliased catalog paths must not double-grant it.

Never refill because quantity fell below the original amount. Bought-out stock stays bought out after reload, stock progression, and module OFF/ON unless a separately specified intentional native grant applies. Preserve buyback items, purchases, foreign additions, and other inventory; never reset a merchant inventory. No custom periodic restocking.

### Learning

Eligible wizards must buy and copy Teleport/Greater Teleport using native learning eligibility, checks, costs, consumption, and native handling of spells above current casting level. Update only the correct book. Do not grant spontaneous known spells, preparations, slots, or inappropriate spell access.

Prove the integrated purchase -> copy -> specialist preparation -> rest -> cast -> first directional movement flow. Test already-known characters separately. Learning and spent state must survive fresh-process reload.

### Inventory-backed sources

Extend the common destination/source/execution model with a scroll adapter; do not merely remove item exclusions from the spellbook adapter. Preserve prepared, spontaneous, specialist, and scroll ownership/accounting without key collisions.

Enumerate accessible traveling-party scrolls, including equipped items without requiring belt placement. Deduplicate the same actual item across APIs. Exclude stash, merchant/inaccessible inventory, and items only on inactive companions.

Recognize verified scroll type and canonical spell association, not display name. Do not enable wands, potions, unlimited items, arbitrary abilities, or unrelated teleport spells. Support compatible crafted scrolls and their actual relevant metadata. Aggregate equivalent items; distinguish materially different activation variants.

Identify active, available readers who can attempt native activation, including legitimate Use Magic Device. Do not require learned/prepared spells, an available slot, or a spellbook for UMD. Allow legitimate uncertain attempts; do not grant unqualified readers automatic success. Multiple readers share one stock, not duplicated counts.

At an otherwise eligible destination, show useful disabled feedback when relevant carried scrolls exist but nobody can attempt activation.

Keep prepared and scroll actions explicit. Never substitute resources silently. Confirmation identifies reader, destination, variant when relevant, one-scroll cost, remaining shared count, no spell-slot cost, native activation-check information, and conditional Teleport outcome odds. Do not present conditional arrival odds as unconditional trip success.

Opening, refreshing, navigating, and cancelling must not roll checks, alter retry state, consume items, or mutate the campaign. Revalidate origin, reader, destination, world state, and actual accessible item on confirmation. Copied/sold/moved/consumed items invalidate the selection. Bind an equivalent current item deterministically for an aggregated choice; do not silently switch material variants/readers.

### Native activation and transactions

Trace the installed native scroll path and relevant compatible hooks: eligibility, checks, rulebook events, retry restrictions, and consumption on success/failure. Use actual Kingmaker behavior, not a parallel tabletop rules engine.

Separate revalidation, one native activation attempt, observation of its native result/cost, and teleport resolution after success. Identify the native boundary that activates one item and executes one strategic effect; do not execute an inert local spell and activate the item again.

| Case | Required behavior |
|---|---|
| Cancel/invalid selection before activation | No roll, cost, or movement |
| Successful activation | Exactly one scroll, no spell slot, existing teleport resolver |
| Native activation failure | No teleport; preserve native consumption/retry/failure effects |
| Off-target/mishap result after activation | Scroll remains spent; no further item cost |
| Technical failure | Conservative attributable handling; no duplicated item or free partial effect |

A rules-level activation failure is not a technical exception. Extend the existing transaction outcome model where needed instead of forcing failures into a spellbook-only “exactly one” assumption. One component owns consumption; never combine native spending with another manual removal.

Handle stacks, final item removal, equipped items, native events, and repeated confirmation callbacks. Compensate only a proven technical debit before material effects when exact safe restoration can be verified. Do not refund bad rules outcomes, uncertain expenditure, relocation, damage, or partial effects.

Prevent ordinary inventory/action-bar Use from consuming these scrolls without a strategic destination. Explain: “Use this scroll by selecting a destination on the world map.” Preserve legitimate copying/preparation and unrelated scrolls. No local targeting, combat escape, automatic area exit, or new hotkey.

Check Craft Magic Items' normal scroll creation and resulting items without a hard dependency, duplicate spells, free crafting, or bypassed prerequisites. Test relevant installed school/spellbook/scroll/vendor interactions, not an unrelated mod overhaul.

## 7. Shared invariants and module lifecycle

All successful sources use the existing destination policy, outcome resolver, protected-world behavior, and corrected post-relocation movement completion.

Preserve persisted visited-location requirements for Teleport and Greater Teleport, familiarity and mishap rules, forbidden destinations/states, and the existing whole-party/associated-companion transport adaptation. Do not introduce scroll-only passenger limits or strand companions.

Recall chooses the party's current Oleg-before-capital / capital-after-establishment sanctuary at activation, with existing handling of ambiguity. It does not use the merchant's, creator's, or purchase-time location.

Buying/copying/teleporting does not fabricate ordinary arrival counts. Teleport does not reveal new routes, enter a local area, advance road-travel time/mileage, or process ordinary fatigue/encounters. Once the player deliberately resumes normal travel, native movement and its legitimate effects must work normally; the no-reveal guard must not suppress them indefinitely.

Keep everything under World-Map Teleportation Spells, with no new toggle. OFF prevents new stock publication/reconciliation, new learning through these scrolls, and teleport activation, while preserving identities, owned items, learned spell data, preparations/spent state, and grant/purchase history. Re-enable without duplicating stock or refilling resources. Restore scoped UI/input changes. Preserve original user settings.

## 8. Evidence, validation, and staging

Use goal checkpoints in this order where practical: reproduce/fix arrows; fix specialist preparation; correct UI; implement/integrate scrolls; qualify/install the combined candidate. Keep independently verified fixes even if another gate is blocked.

For each source slice, follow the active required workflow: inspect, narrow change, focused tests, repository validation, complete current domain suite, clean Release build, package validation, authorized native scenario, evidence, then coherent commit/publication. Do not hard-code old expected test counts/version numbers.

Use realistic behavioral tests and production paths. Request-local deterministic rolls can reproduce outcomes, not replace activation/teleport logic. A spawned item does not prove merchant purchase; direct spellbook mutation does not prove copying or specialist-slot acceptance; a lower-level movement helper does not prove arrows work.

Maintain an acceptance matrix covering all gates, with final integrated end-to-end cases and negative controls. Include:

- Legal first-arrow movement immediately after all successful teleport families/source types; failures/cancellation and ordinary movement regressions; saved magical-arrival reload.
- Already-known/newly copied specialist preparations, native rest/spend, opposition and other-school controls, spent-state persistence.
- Both screenshot scenarios; settlement-only, spell-only, scroll and mixed sources; many/long rows; edge anchors; relevant scaling/aspect ratios; desktop/controller; repeated open/close/destination changes; default Travel; no invisible raycast blockers.
- Old/new merchant inventories, real purchase/copy/buyback, finite stock/fallback/aliasing and saved grant state.
- Normal/UMD readers and native failure consequences, cleric/druid Recall, equipped/shared/final/crafted items, stale requests and double input.
- On-target/off-target/mishap outcomes, actual arrival destination, Recall transition, protected world state, module OFF/ON and fresh-process persistence.

Use the required generic module-boundary checks plus focused relevant installed-mod profiles. Do not invent exhaustive external-mod power sets or reopen unrelated races/firearms qualification.

Record immutable source/build/loaded identities for evidence. Reuse one candidate across applicable runs. Changes invalidate affected tests, not every unrelated historic result. Do not call development-artifact results final-package proof. Perform required final-candidate validation and fresh-process checks without relabeling waived/not-run cases as passing.

Structured native evidence proves mechanics. UI bounds/callbacks establish controls; screenshots, when available, support visual polish only. Follow `AGENTS.md`: no mouse-coordinate automation or screenshot/OCR claims of mechanical correctness.

## 9. Authority, runtime safety, installation, and publication

This mission authorizes implementation, builds, packaging, task-relevant repository/primary-documentation access, guarded native testing, and guarded installation of a validated local candidate. It does not authorize unrelated access, software installation, purchases, credentials, model/provider changes, or account actions.

Use the approved Windows 10 `-kmgRuntimeTestRequest` mechanism and Steam App ID 640820 launch workflow. A broken Steam/DLC environment is not permission to alter saves or disable entitlement checks.

Use positively identified approved automation/disposable fixtures. Preserve `KMG_AUTOMATION_BASELINE` and `KMG_AUTOMATION_WORKING` on disk; authorized working-save loading may seed disposable tests. All new persistence writes must target explicitly transaction-owned disposable saves through approved mechanisms. Never load or modify ordinary owner campaign saves.

You may implement narrowly scoped guarded scenarios for this mission, not a general arbitrary-save/command facility. Do not compete with another agent or owner for the game, installation, Steam session, or mutable fixture. Do not terminate an owner-controlled/unrelated process.

Before mutation, persist the existing workflow's verified backup/transaction record and restoration instructions. Use bounded owned tests and deterministic cleanup. On resumption, inspect interrupted operations rather than blindly restarting them.

Credentials, purchases, cloud conflicts, unidentified saves, enforced access denial, or unexpected Steam dialogs stop the affected operation. Continue safe independent work. Repeated engineering failures alone are not a permission boundary: inspect evidence, narrow the fixture, and change approach while a safe reversible path remains. No unbounded retry loops.

You are explicitly authorized to guarded-install the validated test/final candidate with verified rollback protection. After qualification, leave the final validated build installed for owner testing, preserving FeatureModules.json, UMM settings, original enablement choices, all other mods, and all pre-existing saves. Restore temporary test profiles. Do not stop at “ZIP built” solely because an older prompt did not authorize installation.

This does not override enforced sandbox/allowlist rules. Use normal narrow approval mechanisms where necessary; do not weaken guards or evade denial. Record an exact blocked operation and recovery requirement if access prevents completion.

Use the current approved Git publication mechanism. The inspected `AGENTS.md` requires the following wrapper for coherent `codex/*` branch checkpoints and before pausing/handoff; verify the applicable local contract and use it rather than bypassing it:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```

Do not commit saves, raw/private runtime artifacts, generated packages, proprietary assemblies, credentials, or machine-local configuration. Use curated evidence and local pointers as the active workflow permits. Commit only qualified coherent changes; record partial work in state.

Do not merge to master/main, tag a release, or publish a public release. Prior release permission is not new release permission. Runtime/build/version examples are historical and must be reconciled with the actual current candidate.

## 10. Final handoff and explicit stop rule

The goal is complete only when every objective gate is satisfied and evidenced on the appropriate final candidate, or the owner explicitly amends the contract. A blocked/not-run required case leaves the goal incomplete; after independent work is exhausted, pause with the concrete blocker instead of declaring success or spinning.

The final handoff contains:

- Demonstrated causes of the arrow and specialist-slot bugs, what changed, and evidence for the fixes.
- Actual vendor/location/unlock/quantity/base-price/fallback table and finite-stock migration behavior.
- Player instructions for copying, specialty preparation, scroll activation, and the compact settlement/spell/scroll UI.
- Before/after UI evidence where supported, structured first-arrow acceptance, and integrated purchase-to-movement tests.
- Exact test results, final artifact qualification, unavailable profiles/not-run cases, and remaining limitations.
- Branch/source commit, publication status, version, package/DLL hashes and local paths, installed identity, verified rollback location, and restoration status.
- Updated player documentation and removal/qualification of stale “scrolls excluded,” school, UI, and post-teleport movement claims.

A new session should be able to understand and continue this mission from the two durable files without relying on hidden chat context. A player should be able to teleport, then immediately choose a legal direction and keep playing.
