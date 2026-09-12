# Firearm Maintenance, Full-Rest Recovery, and Misfire Interruption

## Durable mission for Z

**Mission ID:** `Z-FIREARM-MAINTENANCE`

**Repository:** `howardreith/KingmakerGunslingerAndOtherTabletopRules`

**Expected lab:** `C:\Dev\KingmakerGunslingerLab`

**Expected repository:** `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`

**Suggested branch:** `codex/z-firearm-maintenance-rest-safety`

**Owner:** Howie

**Planning reference:** source review at `71af37acc1dc7548fecb067bf753c11d6b90f893`, associated with the 0.0.126 release record. This is a reference for investigation, NOT an instruction to reset to that revision. Inspect and preserve the actual current repository and installed build.

---

## 1. Assignment and completion contract

Implement and qualify the following approved adaptation:

1. **Repair Firearm is an out-of-combat, Broken-only maintenance action.** Keep its existing full-round action economy and reusable shared-inventory Gunsmith's Kit requirement. It restores the same firearm to Normal without consuming the kit or other resources.
2. **Wrecked firearms are restored through successful full resting, not the field-repair action.** A legitimate participating gunsmith with access to the reusable kit automatically maintains the resting party's carried Broken and Wrecked firearms. Restore them directly to Normal. A Wrecked gun remains unloaded.
3. **A newly committed firearm break stops the remainder of the current attack sequence and its automatic continuations.** Do not let auto-reload or real-time auto-attacking immediately resume the interrupted sequence. A later genuinely player-issued attack with a Broken gun remains possible, subject to ordinary damaged-firearm rules and native action availability.
4. **Wrecked firearms cannot fire or reload.** Preserve and qualify the existing command, deed, and state-level protections.
5. **Quick Clear remains a combat recovery option.** Preserve its eligibility, resources, action economy, and existing interactions; do not put the field-repair combat restriction into a shared low-level repair operation that Quick Clear also uses.

This is a multi-session engineering mission, not a one-session prompt. A session ending, context compaction, a quota warning, or a weekly allowance limit does not change the completion criteria. Save resumable progress and report the correct paused state. Do not label the mission complete because source edits or domain tests are complete while required runtime gates remain unverified.

Continue through the next incomplete, safely executable task without seeking routine confirmation. Stop only for completion, an actual capacity limit, an owner pause, or a genuine safety/access/design blocker. A reproducible code failure is work to investigate, not by itself a reason to abandon the mission.

**Engineering completion means:** the approved behavior is implemented; required source, build, package, native runtime, persistence, and compatibility gates pass on the exact final candidate; durable documentation and evidence are complete; qualified checkpoints are published through the approved branch workflow; and the candidate is ready for owner review. Human visual/play acceptance must remain separately marked pending until Howie supplies it. No merge or public release is authorized.

## 2. Safety, authority, and scope

### Read before changing anything

Read root `AGENTS.md`, any applicable nested instructions, this mission, and any existing mission state/journal before work. Read the current runtime, build, package, save-lease, and publication procedures before using them. Historical mission files are context only: do not reactivate unrelated backlog work, teleportation, races, icons, statistics rolling, or other projects.

Verify the host, OS, repository root, remote, branch, HEAD, worktree status, installed game, installed mod, and build environment. Expected technology is the project's existing Kingmaker/C#/Harmony stack; verify actual settings rather than imposing remembered versions. Preserve the project's existing style, language version, explicit project-file registration, tests, and architecture. Add no dependency and perform no unrelated refactor.

Inspect existing scripts before executing them. A build or packaging command must not unexpectedly merge, tag, release, or permanently deploy.

### Git and parallel work

Use a dedicated feature branch. Reuse an existing branch/worktree for this exact mission on resumption; do not create a new branch every session. When another agent or the owner is using the main checkout, prefer an isolated worktree built from the appropriate current base. First verify that its build/deployment/publication scripts support that worktree.

Never discard, reset, clean, auto-stash, overwrite, or rebase over unrelated work. Never commit to `master`/`main`, merge autonomously, force-push, rewrite history, change permissions, create a tag/public release, or open a PR. Leave a reviewable branch and final report.

This mission authorizes scoped reads/fetches from the assigned repository and checkpoint publication through its existing approved policy. For a `codex/*` branch, follow `AGENTS.md` and use the approved wrapper:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```

Verify that it publishes the intended worktree, branch, and commit. Do not bypass the wrapper, edit its guardrails, or claim a push succeeded without confirming the remote branch. If publication cannot safely proceed, record the exact local checkpoint and publication blocker; continue independent authorized work.

Do not install software, change the OS, inspect credentials, open unrelated accounts/files, access other repositories, or alter unrelated game/mod configuration. Use already-installed build tools and dependencies. Network work is limited to the assigned repository and directly necessary technical documentation; do not re-litigate the owner-approved balance decisions by searching for alternative tabletop rules.

### Runtime and installation

Use only the repository's guarded `-kmgRuntimeTestRequest` runtime mechanism and Steam App ID **640820**. Do not launch `Kingmaker.exe` directly, bypass save identity verification, automate coordinates/OCR, elevate privileges, or kill an existing owner game session. Never treat screenshots alone as mechanical proof.

`KMG_AUTOMATION_BASELINE` is immutable: do not select, load, modify, overwrite, rename, or delete it. Load `KMG_AUTOMATION_WORKING` only through its exact authorized receiver-bound contract. In-memory request-local fixtures are preferred. Any persistence round trip requiring a save write must use an explicitly named disposable fixture and an already-proven guarded lease/sentinel mechanism permitted by the repository. Do not broaden save permissions yourself. If no authorized write route exists, finish safe work and report that persistence lane BLOCKED rather than writing to a campaign save.

Inspect and preserve existing logs before launching tests. Use unique run IDs and evidence directories under the approved lab evidence root. Preserve foreign mods/settings and restore temporary changes. Do not manipulate Steam credentials, entitlement, purchases, updates, or cloud conflicts. `DLC Required` across known-good saves is an environment failure, not permission to edit saves.

This assignment authorizes the existing backup-first, reversible candidate deployment **solely for guarded local qualification**, provided the current repository/host permissions allow it. Never overwrite a running install or another agent's newer artifact. Record backup and restore identities before deployment. Restore the pre-mission install after testing unless Howie has separately authorized leaving the candidate installed for playtesting. Do not leave an unqualified or partially copied candidate active. A pre-existing stricter deployment restriction remains binding; record a blocker instead of bypassing it.

## 3. Durable records and allowance-safe operation

Create these records at the start, before substantial implementation. Preserve existing records on resume.

| File | Purpose |
| --- | --- |
| `Z-FIREARM-MAINTENANCE-MISSION.md` | This stable assignment and approved behavior. Do not weaken it to match results. |
| `Z-FIREARM-MAINTENANCE-STATE.md` | Current status, exact checkout/candidate identities, acceptance summary, blockers, and immediately executable resumption steps. |
| `Z-FIREARM-MAINTENANCE-JOURNAL.md` | Concise append-only checkpoints, failures, evidence, and changes of strategy. |
| `docs/FIREARM-MAINTENANCE-CONTRACT.md` | Lasting developer/player contract: repair routes, rest boundary, interruption semantics, compatibility invariants, source map. |
| `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md` | Stable acceptance IDs from section 8, observed results, evidence provenance, and invalidated/unrun gates. |
| `Z-FIREARM-MAINTENANCE-REPORT.md` | Review handoff, written when completing or handing off an incomplete mission; never imply missing gates passed. |

Keep the state concise; link to the matrix and journal instead of copying all logs. If repository instructions require updating a shared resume/coverage ledger, add a narrowly scoped entry without overwriting other agents' records.

### Required state contents

Record:

- Mission status: `IN_PROGRESS`, `PAUSED_CAPACITY`, `PAUSED_OWNER`, `BLOCKED`, or `COMPLETE_ENGINEERING`.
- Current phase and the first incomplete acceptance IDs.
- Host, repository/worktree, branch, base SHA, current HEAD, last qualified/pushed commit, remote verification, and current dirty files with their purpose.
- The implementation decisions that matter to resumption, especially verified native command/rest hooks and rejected approaches.
- Candidate version, source/build revision, package path/hash, DLL SHA-256/MVID, installed DLL hash, and backup/restore status. Use `NOT BUILT`/`NOT VERIFIED` rather than made-up values.
- Domain/build/package/runtime results with exact command, exit/result status, evidence path, run ID, artifact identity, mode/profile, and applicability to the current candidate.
- Any active process/test, owned fixture/lease, outstanding cleanup, modified settings, and whether the normal install is restored.
- Blockers with evidence and safe alternatives already tried.
- **The next one to three concrete actions**, including exact file/symbol or safely runnable command and prerequisites. “Continue testing” is not an adequate resumption step.

Update state and journal after every coherent work slice, after every test result that changes a conclusion, before a long runtime sweep, before compaction/hand-off, and on a credible allowance warning. Write records atomically where practical. Do not wait until the final few tokens to document work. Keep source edits small enough that a forced interruption does not strand a large, unexplained diff.

Observe the repository's commit gates. Do not commit or call a build qualified with failed/unknown required checks just to make a checkpoint. When interrupted mid-change, preserve the working tree and record exactly what is uncommitted, which checks have not run, and how to resume. Qualified commits should be pushed using the approved wrapper. Never push local saves, proprietary assemblies, machine-local evidence, credentials, generated packages, or backups; only curated evidence summaries and source-controlled tests/docs belong in Git.

On quota exhaustion or impending cutoff, reach a safe local boundary, flush state, and report `PAUSED_CAPACITY`. Do not circumvent limits, repeatedly retry unavailable models, start uncontrolled jobs, or promise automatic resumption. Work resumes when a subsequent authorized session is started. The next session must reconcile real disk/process/Git state with the saved record; the record is an index, not proof that a half-finished operation succeeded.

Reuse previously valid evidence only when source/candidate identity, relevant settings/profile, fixture provenance, and gate requirements still match. A rebuilt DLL with a new hash cannot inherit runtime PASS from an earlier artifact. Do not discard useful older evidence; mark its scope or supersession.

## 4. Approved behavioral contract

### 4.1 Field repair

Retain the existing Repair Firearm identity, name, usable icon, and full-round action economy. Do not add an hour-long live timer, advance campaign time manually, add a consumable kit, charge gold/grit, or tie maintenance to ammunition crafting's once-per-rest allowance.

Field repair is allowed only when a legitimate character with the existing Gunsmithing repair capability can act, is outside active combat, has access to **at least one reusable Gunsmith's Kit in shared inventory**, and has the one exact equipped repair target required by the current resolver. The target's **actual persisted condition** must be Broken. Normal and Wrecked are ineligible.

Resolve capability from real existing feature/provider identities and supported grant routes; do not hard-code “Gunslinger class level > 0” or confuse a retired alias fact with proof of entitlement. Preserve approved class/archetype/feat access. Avoid unrelated publication changes.

Use native combat authority that disallows repairing during an active party encounter even if the individual character has not personally entered combat. Follow existing repository conventions and prove the selected combat check in both combat modes; do not rely solely on whether this caster has an attack queued.

Check availability in the UI, at actual command commencement, and immediately before mutation. Bind the intended **concrete firearm** at command start; recheck it at delivery. Two checks executed back-to-back inside delivery do not prove the weapon stayed the same during the preceding full-round command. Combat starting, cancellation, inability to act, loss of required kit/capability, or target/context change before delivery must prevent repair and change no firearm state/resources. Resolve expected ineligibility as a normal rejection, not an exception-log flood.

Repair must preserve item reference/identity, existing enchantments, origin/ownership metadata, loaded-ammunition identity, and surviving round count. Never refund a misfired round or replace a magical gun with a fresh basic instance. Do not consume the kit or charge anything on success or failure.

Provide specific understandable reasons, including “Cannot repair firearms during combat,” “This firearm is Wrecked and requires a completed full rest,” and a missing-kit explanation. Availability inspection must not mutate state.

### 4.2 Rest maintenance

Automatic maintenance occurs only after a **genuine successful full rest**. A participating, legitimately capable gunsmith with shared-kit access is required. Use native rest participation and post-rest capability, not all roster members and not a transient sleeping flag during a callback.

The party inventory scope is locked for this mission: restore eligible firearms in shared carried inventory and in the participating party's equipment/alternate weapon sets. Deduplicate the same concrete item referenced by multiple collections. Do not require a Wrecked gun to remain actively equipped. One qualifying gunsmith and one reusable kit suffice for the party's eligible carried firearms; do not invent one-kit-per-gun limits, gold costs, maintenance slots, camp-role assignment, extra rest hours, or crafting charges.

Exclude remote stashes, merchant stock, ground loot, unrelated units, and equipment of nonparticipants. Use actual ownership/container/participant evidence instead of scanning every loaded item or every roster member. Prove the scope in ordinary camping, world-map resting, and settlement/inn resting routes that exist in the installed game.

Broken and Wrecked become Normal directly. Existing loaded rounds survive a Broken repair; Wrecked remains empty. Normal items are untouched. Preserve the same identity and transaction protections as field repair.

Opening the rest UI, starting a rest, cancelling it, waiting, travel/time advancement, fatigue removal, partial rest, or an interruption **before** successful completion does not qualify. If a rest is interrupted and then resumes to a genuine successful completion, maintenance applies once. Do not infer completion from elapsed time alone.

A repeated callback, multiple participant callbacks, or re-entrancy must not run party-wide maintenance repeatedly, duplicate messages, or consume anything. Verify an actual full-rest completion boundary and session identity/once-only semantics. An arbitrary per-unit restoration method is insufficient without caller/context proof. Do not mark a rest “processed” before eligibility/scope are established in a way that makes an early ineligible participant block a later valid gunsmith.

Use the existing exact-item transaction machinery. Per-item atomic restoration is sufficient; do not roll back a completed native rest or already-repaired unrelated guns because one target fails. Report successful, skipped, and failed targets honestly, log the exact failure, and ensure duplicate/recovery calls are safe. A failure must not break the game's rest completion.

Show one concise maintenance summary per completed rest with actual changes. Provide a clear explanation when damaged carried guns cannot be restored because the required gunsmith/kit is absent, without repeating notifications per participant. Do not report repairs that were not committed.

### 4.3 Newly Broken stops the current attack sequence

Trigger interruption from a **verified committed degradation of the exact firearm during the current attack sequence**, not merely from observing an effective Broken condition.

Let the misfiring shot finish resolving under existing rules: round expenditure, miss, condition change, any appropriate burst, and existing diagnostics. Prevent the **next real shot** and any associated automatic reload before they start. Do not refund completed attacks/resources or grant fresh native action economy.

Tie the interruption to the concrete command/order and firearm context. Cover remaining iterative/Haste/extra attacks, pending reload-resume work, real-time repeated auto-attack orders, and target replacement/retarget continuation of the interrupted order. Do not globally clear every command belonging to the character or party. Leave unrelated movement, abilities, other characters, and ordinary weapons alone.

A new player-issued attack with the Broken gun is allowed when native action economy permits. That intentional later sequence can fire under the existing Broken penalties/misfire rules; **do not turn Broken into a permanently unusable state or cancel every iteration solely because it was already Broken when the new sequence began**. A deliberate attack order is not equivalent to an internal call to `CreateAttackCommand`: trace and verify the native player-intent route separately from automatic command recreation.

Neither automatic target changes nor reload completion count as renewed player consent. Reloading or Quick Clear may restore readiness but must not resurrect the cancelled old attack order. Weapon switching, explicit later orders, death, area change, command cleanup, and save/load must not leak suppression to unrelated future commands or revive stale automatic continuations. Prefer native cancellation semantics and narrowly scoped command state; do not add a fourth persistent firearm condition or replace the existing firearm-token schema.

Preserve the distinction between **actual stored Broken** and a battered-firearm ownership-based **effective Broken overlay**. A pre-existing effective overlay alone is not a new physical malfunction and is not cured by maintenance. Misfire-negation features that prevent a break must not spuriously interrupt the sequence. Keep existing Stranger's Fortune/Expert Loading and other relevant feature behavior intact; qualify the applicable paths discovered in source.

Treat composite firing deeds according to their existing discharge model. Multiple component attack rolls representing a single shot are not automatically multiple shots to cancel. Inspect Dead Shot/scatter and other custom attack paths; do not silently redesign their misfire rules. The invariant is no subsequent real discharge or automatic resumption after the applicable committed break.

### 4.4 Wrecked is unusable for firearm firing

Preserve layered rejection at command construction, queued/ongoing delivery, auto-reload, individual discharge, state transitions, and firearm-firing deeds. A Wrecked firearm must not initiate a shot, consume ammunition/grit for an unavailable firing action, emit an invalid projectile, or deal firing damage. Low-level forced miss is a safety fallback, not sufficient user-facing behavior.

If a command was legal when queued but its gun becomes Wrecked before its next shot, reject before that shot's effects/resource commitment. Preserve native rules for already-spent action economy; do not invent broad refunds. A Broken-to-Wrecked misfire still resolves its one legitimate consequence/burst exactly once. No further firing follows it.

Do not globally disable unrelated character abilities or redesign firearm-as-improvised-melee rules. Cover every supported path that actually fires the firearm. Rest is the new Wrecked maintenance route; field repair and the saved legacy Overhaul alias must not bypass it. Preserve other already-approved explicitly implemented recovery mechanics rather than inventing or removing magical recovery as part of this task.

### 4.5 Compatibility and non-goals

Keep Quick Clear's existing combat availability, standard/move versions, grit rules, True Grit interactions, actual-condition restrictions, and Wrecked rejection. Do not send it through field-repair eligibility.

Keep ammunition crafting/grant/reset behavior, vendor cleanup, retired consumable items, blueprint IDs, save-token IDs, enchantments, and battered origin unchanged. Keep exactly one visible maintenance action. The legacy Overhaul blueprint remains registered, hidden/autofill-ignored, and routed through the same **Broken-only out-of-combat** field checks if an old saved fact/slot invokes it. It must not become a second action or a rest-restoration shortcut.

This mission changes neither misfire chances, Broken penalties, burst balance, free/Lightning Reload costs, proficiency, firearm prices, nor damage. Do not rename all old `TestMusket` types, replace persistence architecture, add inventory UI, introduce new systems/settings, or touch unrelated modules as cleanup.

## 5. Investigate current source before implementing

The earlier review identified the following source map. Reconfirm actual paths/callers on your working base; list renamed/new counterparts in the contract. Do not assume these files are the entire call graph.

| Area | Starting points under `src/KingmakerGunslinger/` | Investigation |
| --- | --- | --- |
| Field repair | `Actions/FirearmActionPolicy.cs`, `Recovery/RepairTestMusketRuntime.cs`, `Recovery/RepairTestMusketAbilityLogic.cs` | Capability, combat authority, command-start binding, delivery checks, display reasons. |
| Exact-item transaction | `Recovery/FirearmRepairTransactionService.cs`, `Recovery/FirearmItemRepairStateStore.cs`, `Firearms/FirearmStateMachine.cs` | Field versus rest authorization, rollback, preserved rounds, direct shared callers. |
| Rest | `Gunsmithing/CraftingRestResetPatch.cs`; native `RestController.ApplyRest` callers | This is a candidate integration point, not proof of full-rest completion. Trace local/world-map/settlement routes and per-unit versus per-party semantics. |
| Misfire | `Misfires/FirearmMisfireRuntime.cs`, condition services, negation/consequence paths | Committed exact-item degradation, attack context, aggregate/deed handling, existing deduplication. |
| Full attacks | `Firing/FreeActionFullAttackReloadPatch.cs`, `Reloading/FullAttackAutoReloadPolicy.cs` | Existing `UnitAttack.OnAction` boundary, native completion, later deliberate Broken orders. |
| Auto-continuation | `Firing/EmptyFirearmAttackCommandPatch.cs` | Pending reload callbacks, saved targets, real-time command recreation and genuine input intent. |
| Firing safety | `Firing/FirearmDischargeRuntime.cs`, discharge service, reload policy, custom deeds/scatter | Command gate and defense-in-depth; paths bypassing ordinary discharge. |
| Recovery exceptions | `Deeds/QuickClearRuntime.cs`, `Deeds/QuickClearService.cs` and related feature providers | Shared repair transition must remain usable in combat via Quick Clear. |
| Identity/ownership | `Actions/ExactEquippedFirearmResolver.cs`, `Firearms/FirearmRuntimeState.cs`, `Gunsmithing/` battered-origin/use policies | Concrete item references and persisted condition versus effective overlay. |
| Publication/UI | Locate Repair/Overhaul/Gunsmithing blueprint builders, ability providers, condition/tooltips, kit descriptions | Every promise of full-round Wrecked repair must be removed from active user-facing text. |

Read the relevant existing domain/runtime tests and reflection/patch contracts before adding equivalents. Locate the current unified-maintenance and legacy-alias scenarios, then extend or replace their obsolete expectations explicitly. Do not leave active acceptance checks asserting the old “Wrecked field repair succeeds” contract.

Inspect installed native assemblies only within the authorized development scope. Verify actual method signatures, sequencing, and interaction with the owner mod profile. Do not invent a convenient `OnFullRestCompleted` API or install multiple competing prefixes where extending the existing integration is safer. Record patch order and exact target assertions when compatibility depends on them.

Keep business/policy logic in focused services/policies, adapters responsible for native context, and patches thin. Use separate clearly named field-repair and completed-rest restoration entry points, or an equivalently explicit validated context. They may share atomic state mutation. The ordinary ability must not gain access to Wrecked restoration merely by calling an unconstrained generic transaction.

## 6. Implementation phases

### P0 — Baseline and durable setup

Verify safety/checkout/installed identities; create records and acceptance IDs; trace the current implementation and native boundaries. Record baseline checks. Establish the pre-change behavior using focused deterministic tests or guarded disposable scenarios where feasible. Record inherited failures separately; do not erase them, claim new tests passed on baseline, or spend the mission repairing unrelated issues.

**Exit:** verified source map, current behavior, safe workflow, rest/command investigation findings, and exact next implementation slice recorded.

### P1 — Stop accidental continuation after a break

Implement command-scoped interruption and auto-continuation protection first. Preserve already-Broken deliberate attacks, negated misfires, ownership overlays, native action budgets, and unrelated commands. Extend the production harness as needed without weakening isolation.

**Exit:** focused behavioral tests and native RTWP/turn-based scenarios demonstrate interruption before another shot/reload and intentional later use. Any remaining qualification is explicitly tracked, not assumed.

### P2 — Add verified completed-rest restoration

Introduce the completed-rest coordinator and explicit restoration service route. Implement qualification of participants, kit/capability, inventory scope, item deduplication, one-time completion behavior, rollback/failure reporting, and preserved identity/ammunition. Keep the crafting-rest reset independent.

**Exit:** full completion restores eligible carried damage; incomplete/cancelled rests do not; duplicate callbacks are harmless; rest routes and save persistence are covered or honestly blocked.

### P3 — Restrict field repair and preserve legacy/Quick Clear paths

Make field repair Broken-only and out of combat at all relevant boundaries. Bind the exact target at real command start. Update legacy alias behavior, capability checks, and reasons. Leave Quick Clear's combat route intact. Perform a full caller audit for bypasses.

**Exit:** invalid field/alias invocations cannot change damage state; valid field repair remains resource-free and same-item; Quick Clear and crafting regressions pass.

P1 precedes the harsher recovery restriction to avoid increasing the consequences of accidental automation. Do not distribute any intermediate candidate as a finished feature.

### P4 — Documentation, integration, and immutable final qualification

Update active descriptions, condition explanations, player smoke test, `KNOWN-ISSUES.md`, changelog, appropriate coverage/fidelity records, and developer contract. Historical release notes remain history; do not rewrite old evidence as though it tested this new design. Close the old Wrecked-in-combat design question with the new approved behavior and actual qualification status.

Perform repository validation, the complete domain suite, clean Release build, and strict package validation using current scripts. Allocate candidate version/metadata using the established local workflow without publishing a release or guessing an already-used version. Build/deploy an immutable candidate; record source SHA, hashes/MVID, versions, profile/settings, fixture, and package identity.

Run the required native acceptance/regression lanes and current repository-mandated compatibility/module boundary matrix. Use focused optional-mod profiles plus required generic boundaries, not an invented exhaustive `2^N` game-launch sweep. Do not invent an OFF toggle for core functionality that has none; mark genuinely inapplicable lanes N/A with evidence and retain applicable combined-profile tests. Reuse the same qualified artifact across runs. Any changed shipped DLL/assets/configuration require appropriate final-artifact reruns; code changes cannot inherit prior runtime results. Documentation-only follow-up commits may cite the unchanged tested binary and its exact build commit, explicitly.

### P5 — Review handoff

Reconcile all acceptance IDs and artifacts. Verify remote checkpoint publication and temporary-install/settings/fixture cleanup. Produce the final report and an owner smoke test. Mark engineering complete only under section 9. Do not merge, tag, release, open a PR, or claim human acceptance.

## 7. Test and evidence rules

Match the existing test conventions and dependencies. Prefer real policy/service/state-store integration and native objects over broad mocks. Use targeted fixtures only for engine construction or deliberate fault injection; never duplicate the production behavior in a test helper and call its agreement proof. Assert observable item state, resource counts, native commands, attacks/projectiles, and completion, not merely which helper was called.

Use deterministic misfire controls only inside the guarded test request. No force-roll state, new suppression, rest flag, fixture fact, or test callback may leak into ordinary play. Clean up scoped test instrumentation and owned transient objects. Verify ordinary launch does not activate test mode.

For native proof, execute the real production command lifecycle/rest flow where required. Calling `ApplyForRuntimeTest`, a rest postfix, a transaction helper, or a policy with fabricated booleans can prove that layer, but cannot by itself qualify native scheduling, actual rest completion, or genuine input-versus-auto-resume behavior. Use validated receiver-bound native routes and record what actually ran. Add thin observation hooks when necessary; do not fake “player intent” by setting the very flag the test expects the implementation to set.

Minimum evidence for each gate: expected and observed result, actual route exercised, PASS/FAIL/BLOCKED/NOT RUN/N/A, artifact hash/build revision, run ID, evidence path, mode/profile, and fixture identity. A missing/assertion-free/contradictory result is not PASS. Preserve older runs and explain supersession. Report counts from actual executions only.

Each mandatory high-risk native lane—misfire interruption, field-repair rejection, completed-rest restoration, and no-op cancelled rest—must pass on the final artifact in **two independent guarded runs/fresh-process executions as applicable**; the persistence lane must include a fresh-process load of an authorized disposable round trip. Avoid running the entire full matrix twice when only these repetitions are needed, unless repository policy requires more.

## 8. Stable acceptance matrix

Create one row per ID below. Split rows into subcases when needed; do not silently delete or rename gates to hide omissions. Domain and native evidence should have separate result cells when both are relevant. Mark runtime applicability explicitly rather than presenting a domain PASS as a combined PASS.

### Field maintenance

| ID | Scenario and required outcome |
| --- | --- |
| F01 | Broken gun, legitimate repair capability, outside combat, reusable kit: full-round repair succeeds on the same exact item; rounds/ammunition/enchantments/origin survive; kit/gold/grit/crafting allowance unchanged. |
| F02 | Normal gun, missing kit, only a retired consumable kit, missing capability, incapable character, or ambiguous/no equipped target: rejected without state/resource mutation. Multiple reusable kits still satisfy the at-least-one requirement. |
| F03 | Active combat in RTWP and turn-based, including a party encounter where this caster is not personally engaged: availability and execution reject. |
| F04 | Start outside combat, then combat begins, kit/capability is lost, caster becomes unable to act, command is cancelled, or the original weapon/context changes before delivery: original and replacement guns remain unrepaired. |
| F05 | Wrecked gun through ordinary field repair, direct ability delivery, and legacy Overhaul fact/slot: rejected in and out of combat with a full-rest explanation. |
| F06 | Ordinary success and injected state-write/verification failure: exact-item transaction/rollback is correct; no inventory/resource side effects; no false success notification. |

### Full-rest maintenance

| ID | Scenario and required outcome |
| --- | --- |
| R01 | Genuine complete rest with eligible participating gunsmith and kit restores Broken and Wrecked to Normal; Wrecked stays unloaded; no separate click or resource expenditure. |
| R02 | Shared carried guns, participant-equipped guns, alternate sets, two identical blueprints with distinct damage, and duplicate references: correct concrete targets repaired once; no substitution. |
| R03 | Remote stash, vendor, ground loot, and nonparticipant equipment remain unchanged; only absent roster gunsmith does not qualify the rest. |
| R04 | Missing kit/capability or no eligible participating repairer: no repair. Multiple participants/capable repairers do not multiply work. Confirm supported provider routes without a class-only shortcut. |
| R05 | Open/start/cancel rest, partial/interrupted non-completion, waiting/travel/time advancement, and fatigue removal: no repair. |
| R06 | Interrupted rest subsequently genuinely completes: repairs once, after completion, not during the interruption. |
| R07 | Repeated native completion notifications, per-unit callbacks, and re-entrancy: no duplicate effects/messages; early ineligible participant cannot preclude a later eligible party context. |
| R08 | Local camping, world-map, and settlement/inn full-rest routes present in this build: verified boundary and correct scope. Truly absent routes require evidence-backed N/A, not a guessed PASS. |
| R09 | Restoration failure on one item does not corrupt other items or native rest; same-item rollback and honest partial-result reporting; ammunition crafting/rest reset remains independent. |
| R10 | Save/load before completion preserves damage; save/load after actual completed restoration preserves Normal and exact rounds/identities. No repair merely on load or replay of a stale completion marker. |

### Attack interruption and deliberate later firing

| ID | Scenario and required outcome |
| --- | --- |
| A01 | Deterministic newly Broken on first/middle shot of a native full attack in each combat mode: no remaining iterative/Haste/extra shot, projectile, ammo consumption, or associated auto-reload. First legitimate misfire expenditure/consequence remains. |
| A02 | Free reload, Lightning Reload, ordinary reload-resume, pending callbacks, and real-time repeated auto-attack orders cannot resume the interrupted order. No resume on the next update. |
| A03 | Automatic retarget/target death does not evade interruption. Native last-shot break and single-attack break do not create an immediate automatic new attack. |
| A04 | Later genuine player attack order with an already-Broken gun is allowed if native actions permit; existing penalties/misfire chance remain; later deliberate sequence is not cancelled solely for starting Broken. No free extra turn/action. |
| A05 | Reload/Quick Clear, switching weapons, order cleanup, death, scene transition, and save/load neither resurrect the stopped order nor lock unrelated future movement/attacks/abilities. |
| A06 | Pre-existing battered ownership effective-Broken overlay alone does not count as a newly committed physical break. Maintenance does not erase origin/ownership or pretend to cure the overlay. |
| A07 | Existing misfire-negation and applicable Expert Loading/other feature paths retain their intended outcomes; a prevented break does not trigger new-break interruption. |
| A08 | Real firearm-firing deeds/scatter/composite attacks use their actual discharge semantics. No extra real shot follows a committed qualifying break; no invented per-component cancellation of a single discharge. |

### Wrecked restrictions and regressions

| ID | Scenario and required outcome |
| --- | --- |
| W01 | Wrecked ordinary/new/queued attacks and reload/auto-reload/deed paths cannot fire or spend firing resources; unavailable action is rejected before invalid projectile/effect generation. |
| W02 | A gun becomes Wrecked after a command is queued: next shot cannot occur. The actual Broken-to-Wrecked misfire's approved burst resolves exactly once, with no duplicate consequence. |
| C01 | Quick Clear standard/move, relevant True Grit handling, kit-free combat use, actual-condition eligibility, and Wrecked rejection remain as before. Recovery cannot resume the cancelled old order. |
| C02 | Basic/paper ammunition crafting, its costs/once-per-rest reset, legitimate grants, free/Lightning Reload economics, and retired-kit vendor cleanup are unchanged. |
| C03 | Old saved Repair/Overhaul facts and action bars load without missing blueprints, extra visible maintenance buttons, or bypasses. Retired kit items remain inert/save-compatible. |
| C04 | Named/magic/battered guns, enchantment tokens, surviving ammunition IDs/counts, multiple identical guns, equipment transfer/switching, and save/reload preserve correct per-item state. |
| C05 | Native bow/crossbow/melee attacks, other characters, unrelated abilities/commands, and relevant optional-mod profiles remain unaffected. No ordinary-play test instrumentation leak. |
| C06 | Active Repair/Gunsmithing/kit/condition/legacy text consistently explains the new rules; precise unavailable reasons and truthful rest summary appear; human visual judgment remains separately pending. |
| Q01 | Repository/source validation, all domain tests, clean Release build, strict installable package validation, and required patch/blueprint identity checks pass with recorded outputs. |
| Q02 | Current repository-required runtime/compatibility matrix and high-risk repeated final-artifact runs pass with matching DLL/package/source/profile evidence; all stale/unrun lanes explicitly identified. |
| Q03 | Protected-save/fixture/settings/install safeguards verified; final package available; state/journal/contract/matrix/report complete; allowed checkpoint push verified. No merge/tag/PR/release. |

## 9. Stopping, recovery, and reporting

### Genuine blockers

A protected-save risk, unidentified live process/artifact, missing required permission/tool/game assembly, unsupported deployment or save-write path, mandatory credential/cloud interaction, unreconcilable concurrent edits, or a behavior decision genuinely outside section 4 is a blocker. Preserve evidence and complete independent safe lanes before ending where possible. Never cross the boundary to make the matrix green.

When a test fails, inspect structured evidence, narrow the fixture, trace native control flow, or improve observation. Record what changed between attempts. Do not repeatedly launch the same failing scenario without a new hypothesis. A run limit is not a design excuse, but actual allowance exhaustion is a valid pause. Missing runtime permission is not permission to replace native proof with a fake test.

On resume, read this mission, state, acceptance matrix, and recent journal entries; inspect Git/files/processes/install identities; verify cleanup; then resume the earliest incomplete dependency. Do not restart all investigation or rerun valid matrix lanes simply because the conversation context is new. Conversely, do not trust stale PASS after changed source, DLL, fixtures, or settings.

### Completion audit

Before `COMPLETE_ENGINEERING`, verify that every mandatory applicable row is PASS with the required evidence layer on the final candidate; no mechanical BLOCKED/NOT RUN/AMBIGUOUS lane is hidden; documentation accurately matches implementation; exact candidate/package identity is recorded; authorized publication/cleanup succeeded; and the owner smoke test is ready. Evidence-backed N/A is permitted only when a feature/route genuinely does not exist, not when it was difficult to test. Human visual/play approval can remain PENDING without falsely recording it as an automated PASS.

The final report must include:

1. Status: complete engineering, paused capacity/owner, or incomplete with blockers; list the outstanding acceptance IDs first when incomplete.
2. Verified baseline behavior and implemented causal changes, including the **actual** rest boundary and player-intent/automatic-continuation boundary used.
3. Scope/compatibility decisions and any remaining risks or deviations requiring owner review.
4. Changed source areas and durable documentation.
5. Exact base/head/remote branch, candidate version, build revision, DLL hash/MVID, package path/hash, installed/backup/restore state.
6. Tests by evidence layer, exact final-artifact runtime results/profile coverage, repetitions, persistence proof, and honest NOT RUN/blocked lanes. Do not report historical assertions as current-candidate proof.
7. A short owner playtest covering a fresh break, deliberate later Broken shot, combat/field repair rejection, full-rest restoration, and Quick Clear.
8. For any pause/incomplete handoff: precise next steps, retained dirty work, active process/lease/cleanup status, and evidence locations.

### End-state reminder

The goal is a reliable, save-compatible gameplay change with verified recovery and command behavior—not merely renamed descriptions, a green policy suite, or a hypothetical implementation. Weekly allowance determines when to checkpoint; it does not authorize reduced scope or a false completion claim.
