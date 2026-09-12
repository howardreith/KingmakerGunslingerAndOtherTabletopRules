# Firearm Maintenance — Acceptance Matrix

Stable gate IDs from `Z-FIREARM-MAINTENANCE-MISSION.md` §8. Do not delete or
rename rows. Update observed results/evidence as gates run; a gate is PASS only
with the required evidence layer recorded for the **exact final candidate**.
Domain (D) and native runtime (N) results are tracked separately when both
apply.

Legend: `NOT RUN` (default), `PASS`, `FAIL`, `BLOCKED`, `AMBIGUOUS`, `N/A`
(evidence-backed only).

## Field maintenance (repair action)

| ID | Scenario / required outcome | Domain | Native | Evidence / notes |
| --- | --- | --- | --- | --- |
| F01 | Broken gun, legitimate repair capability, outside combat, reusable kit: full-round repair succeeds on the same exact item; rounds/ammunition/enchantments/origin survive; kit/gold/grit/crafting allowance unchanged. | NOT RUN | NOT RUN | |
| F02 | Normal gun, missing kit, only a retired consumable kit, missing capability, incapable character, or ambiguous/no equipped target: rejected without state/resource mutation. Multiple reusable kits still satisfy at-least-one. | NOT RUN | NOT RUN | |
| F03 | Active combat in RTWP and turn-based, including a party encounter where the caster is not personally engaged: availability and execution reject. | NOT RUN | NOT RUN | |
| F04 | Eligible start, then combat begins / kit lost / capability lost / caster unable to act / command cancelled / original weapon or context changes before delivery: original and replacement guns remain unrepaired. | NOT RUN | NOT RUN | |
| F05 | Wrecked gun via ordinary field repair, direct ability delivery, and legacy Overhaul fact/slot: rejected in and out of combat with full-rest explanation. | NOT RUN | NOT RUN | |
| F06 | Ordinary success and injected state-write/verification failure: exact-item transaction/rollback correct; no inventory/resource side effects; no false success notification. | NOT RUN | NOT RUN | |

## Full-rest maintenance

| ID | Scenario / required outcome | Domain | Native | Evidence / notes |
| --- | --- | --- | --- | --- |
| R01 | Genuine complete rest with eligible participating gunsmith + kit restores Broken and Wrecked to Normal; Wrecked stays unloaded; no separate click or resource expenditure. | PARTIAL 2026-09-12: completion gate + item decisions + kit-never-consumed domain-tested (rest-maintenance.genuine-completion-qualifies, item-decisions-actual-condition, wiring tests). | NOT RUN | |
| R02 | Shared carried guns, participant-equipped guns, alternate sets, two identical blueprints with distinct damage, duplicate references: correct concrete targets repaired once; no substitution. | PARTIAL 2026-09-12: scope-collection wiring asserted (inventory Items + Body.AllSlots + reference dedup, wiring-scope-capability-evidence); concrete-target behavior needs native run. | NOT RUN | |
| R03 | Remote stash, vendor, ground loot, nonparticipant equipment unchanged; absent roster gunsmith does not qualify the rest. | NOT RUN | NOT RUN | |
| R04 | Missing kit/capability or no eligible participating repairer: no repair. Multiple participants don't multiply work. Provider routes confirmed without a class-only shortcut. | PARTIAL 2026-09-12: missing-prereq policy + capability-via-feature-fact wiring asserted (missing-prerequisites-no-repair, wiring-scope-capability-evidence). | NOT RUN | |
| R05 | Open/start/cancel rest, partial/interrupted non-completion, waiting/travel/time advancement, fatigue removal: no repair. | PARTIAL 2026-09-12: non-completion gate domain-tested for encounter/skip-time/unsuccessful (non-completion-never-qualifies); cancel/open/partial UI routes need native runs. | NOT RUN | |
| R06 | Interrupted rest subsequently genuinely completes: repairs once, after completion. | PARTIAL 2026-09-12: once-per-RestStatus guard wired (wiring-completion-boundary-once); resumed-rest flag-latch risk recorded in journal #4 (fallback: TickSleepPhase completion branch). | NOT RUN | |
| R07 | Repeated completion notifications, per-unit callbacks, re-entrancy: no duplicate effects/messages; early ineligible participant cannot preclude later eligible party context. | PARTIAL 2026-09-12: single-prefix design (not per-unit ApplyRest) + once-only guard asserted; duplicate-notification behavior needs native run. | NOT RUN | |
| R08 | Local camping, world-map, settlement/inn full-rest routes present in this build: verified boundary and scope; absent routes need evidence-backed N/A. | NOT RUN | NOT RUN | |
| R09 | Restoration failure on one item doesn't corrupt other items or native rest; same-item rollback; honest partial-result reporting; crafting/rest reset independent. | PARTIAL 2026-09-12: per-item isolation + honest summaries + crafting independence domain-tested (wiring-failures-isolated, summaries-honest). | NOT RUN | |
| R10 | Save/load before completion preserves damage; after completed restoration preserves Normal + exact rounds/identities; no repair on load or stale completion-marker replay. | PARTIAL 2026-09-12: maintenance commits before the post-rest autosave (design + wiring); persistence needs native round trip. | NOT RUN | |

## Attack interruption and deliberate later firing

| ID | Scenario / required outcome | Domain | Native | Evidence / notes |
| --- | --- | --- | --- | --- |
| A01 | Deterministic newly-Broken on first/middle shot of a native full attack in each combat mode: no remaining iterative/Haste/extra shot, projectile, ammo consumption, or associated auto-reload; first misfire expenditure/consequence remains. | PARTIAL 2026-09-12: full-attack end-on-break gate domain-tested (`broken-sequence.full-attack-ends-after-break` + wiring test); gate runs before free-reload decision. | NOT RUN | |
| A02 | Free reload, Lightning Reload, ordinary reload-resume, pending callbacks, real-time repeated auto-attack orders cannot resume the interrupted order; no resume on next update. | PARTIAL 2026-09-12: construction rejection + epoch-checked resume + no-replacement-reload domain-tested (`broken-sequence.construction-automatic-rejected`, `resume-blocked-by-later-degradation` + wiring tests). | NOT RUN | |
| A03 | Automatic retarget / target death does not evade interruption; native last-shot break and single-attack break create no immediate automatic new attack. | NOT RUN | NOT RUN | |
| A04 | Later genuine player attack order with already-Broken gun allowed when native actions permit; penalties/misfire remain; not cancelled solely for starting Broken; no free extra action. | PARTIAL 2026-09-12: player-issued order consumes suppression; unsuppressed Broken construction always allowed (`broken-sequence.construction-player-order-consumes`, `construction-unsuppressed-allows`). | NOT RUN | |
| A05 | Reload/Quick Clear, weapon switching, order cleanup, death, scene transition, save/load neither resurrect the stopped order nor lock unrelated future commands. | PARTIAL 2026-09-12: repaired weapon never locked (`broken-sequence.construction-repaired-never-locked`); suppression weak-keyed/transient (`broken-sequence.wiring-suppression-transient`). | NOT RUN | |
| A06 | Pre-existing battered ownership effective-Broken overlay alone is not a newly committed physical break; maintenance does not erase origin/ownership or pretend to cure the overlay. | PARTIAL 2026-09-12: suppression is fed only from `CommitConditionTransition` (persisted actual condition), never an overlay (wiring test). | NOT RUN | |
| A07 | Misfire-negation and Expert Loading/feature paths keep intended outcomes; a prevented break triggers no interruption. | PARTIAL 2026-09-12: negation paths return before the commit-record point (wiring test). | NOT RUN | |
| A08 | Real firearm-firing deeds/scatter/composite attacks use actual discharge semantics; no extra real shot after committed qualifying break; no invented per-component cancellation of a single discharge. | NOT RUN | NOT RUN | |

## Wrecked restrictions and regressions

| ID | Scenario / required outcome | Domain | Native | Evidence / notes |
| --- | --- | --- | --- | --- |
| W01 | Wrecked ordinary/new/queued attacks and reload/auto-reload/deed paths cannot fire or spend firing resources; rejected before invalid projectile/effect generation. | NOT RUN | NOT RUN | |
| W02 | Gun becomes Wrecked after command queued: next shot cannot occur; Broken→Wrecked misfire's approved burst resolves exactly once, no duplicate consequence. | NOT RUN | NOT RUN | |
| C01 | Quick Clear standard/move, True Grit handling, kit-free combat use, actual-condition eligibility, Wrecked rejection unchanged; recovery cannot resume cancelled old order. | NOT RUN | NOT RUN | |
| C02 | Ammunition crafting costs/once-per-rest reset, grants, free/Lightning Reload economics, retired-kit vendor cleanup unchanged. | NOT RUN | NOT RUN | |
| C03 | Old saved Repair/Overhaul facts and action bars load: no missing blueprints, no extra visible maintenance buttons, no bypasses; retired kit items inert/save-compatible. | NOT RUN | NOT RUN | |
| C04 | Named/magic/battered guns, enchantment tokens, ammunition IDs/counts, multiple identical guns, equipment transfer/switching, save/reload: correct per-item state. | NOT RUN | NOT RUN | |
| C05 | Native bow/crossbow/melee attacks, other characters, unrelated abilities/commands, optional-mod profiles unaffected; no ordinary-play instrumentation leak. | NOT RUN | NOT RUN | |
| C06 | Active Repair/Gunsmithing/kit/condition/legacy text explains the new rules; precise unavailable reasons; truthful rest summary; human visual judgment separately PENDING. | NOT RUN | NOT RUN | |
| Q01 | Repository/source validation, all domain tests, clean Release build, strict package validation, patch/blueprint identity checks pass with recorded outputs. | NOT RUN | N/A | |
| Q02 | Required runtime/compatibility matrix and high-risk repeated final-artifact runs pass with matching DLL/package/source/profile evidence; stale/unrun lanes identified. | N/A | NOT RUN | |
| Q03 | Protected-save/fixture/settings/install safeguards verified; final package available; state/journal/contract/matrix/report complete; checkpoint push verified; no merge/tag/PR/release. | NOT RUN | NOT RUN | |

## Human acceptance

Owner visual/play acceptance is tracked **separately** as PENDING and is never
recorded as an automated PASS (mission §9).
