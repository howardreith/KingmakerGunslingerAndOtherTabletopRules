# Turn Undead — acceptance and reviewer matrix

This matrix is a finite behavior inventory for the Turn Undead mission. It is not an instruction to run the whole repository suite for each change. Use focused tests for pure rules and regression cases, and group related native observations into a small number of guarded fixtures. Follow the repository's current validation policy and the enrolled workflow's actual budgets.

Statuses: **PASS** (evidence on the stated candidate), **FAIL**, **BLOCKED** (precise unavailable prerequisite), or **NOT RUN**. Do not substitute assumed PASS, a synthetic transcript, or an old package result. “N/A” requires a scope/source explanation. Every native evidence record must identify product commit, DLL/package hash, fixture and observer identity, game/loader version, profile, and runtime transaction/restore receipt as applicable.

## A. Intake and contract checks

| ID | Behavior or invariant | Minimum useful evidence |
| --- | --- | --- |
| TU-01 | Correct repository, current instructions, isolated branch/worktree, independent mission state | Recorded remote/default branch/base SHA; current policy documents; unrelated work untouched |
| TU-02 | Current engine contract is known | Exact supported game/reference assembly identity (expected 2.1.7b, record actual) and targeted inspection of donor, source resources, saving throws, fear state, variants, and load/serialization paths |
| TU-03 | Every implemented channel source has an explicit mapping | Source feature, effective level, DC statistic, resource/cost, restrictions, wrapper, and evidence; unresolved conflicts recorded accurately |
| TU-04 | Mindless/intelligent decisions are supported by data | Reviewed classification inventory; ordinary skeleton/zombie and intelligent variant counterexamples; no authoritative Int/name/brain shortcut |

## B. Feat acquisition and resources

| ID | Scenario | Required observable result |
| --- | --- | --- |
| TU-05 | Positive cleric learns the general feat through normal level-up | One selectable feat; supplied feat icon; one correctly granted source action; feat persists on save/load |
| TU-06 | Negative-only cleric, non-channel class, and positive-permission-only fixture | Feat not selectable through normal acquisition and no usable action solely from alignment, selection permission, generic channel marker, or item; positive neutral cleric qualifies |
| TU-07 | Paladin before/after actual channel grant; Hospitaler; Empyreal | Eligibility follows real source possession; each supported native source binds to the validated mapping; normal restrictions preserved |
| TU-08 | One source activation affecting several undead | Source resource decreases once by its real cost; cleric/Hospitaler/Empyreal one use; ordinary paladin two Lay on Hands uses after confirming actual engine; no per-target debit |
| TU-09 | Resource exhausted or prerequisite unavailable | Correct action disabled/rejected; no negative resource, free effect, or fallback to another class's pool |
| TU-10 | All targets save, or an otherwise valid empty-area attempt | Normal attempt/resource semantics match the verified channel contract; no refund for successful target saves; no double debit |
| TU-11 | Learn/relearn, menu rebuild, reload, module toggle | No newly created daily pool, refill, duplicated charge, or duplicated source action |
| TU-12 | Positive multiclass character with two different channel sources | Explicit source choice; separate level/DC/resource accounting; depleting one does not silently use the other; unrelated levels do not raise the DC |

## C. Core effect and rulebook integration

| ID | Scenario | Required observable result |
| --- | --- | --- |
| TU-13 | Standard-action activation in real time and turn-based combat | Actual standard action spent; no unintended attack of opportunity/spell-slot/metamagic behavior; one activation |
| TU-14 | Mixed group around the 30-foot boundary | Native area delivery hits the intended in-range undead; out-of-range undead and living creatures remain unaffected; environmental delivery semantics match the verified native path |
| TU-15 | Living negative-energy-affinity creature alongside true undead | Only the actual undead qualifies; damage/heal affinity is not reused as the creature-type predicate |
| TU-16 | Allied undead, with and without supported Selective Channeling | Unmodified all-undead behavior is preserved; any selective exclusion matches the documented supported rule and does not become an arbitrary party exemption |
| TU-17 | Initial successful and failed Will saves | Success prevents this application; failure produces the real fleeing state; both intelligent and mindless undead receive the initial save |
| TU-18 | Failed mindless target through multiple rounds | Continues flight for the remaining minute without new saves from this effect; does not gain an intelligent-repeat-save path because its stored Int is 10 |
| TU-19 | Failed intelligent target through later rounds | Exactly the intended native round opportunities; successful recovery ends the owned effect; failed recovery retains it; no new channel debit |
| TU-20 | Effect reaches one minute without successful recovery | Ends at the correct game-time boundary; no per-round duration refresh or timer restart |
| TU-21 | Cleric odd/even levels, different Charisma, extra unrelated levels | Resolved DC agrees with the source contract, and a UI preview agrees where the engine exposes one; for ordinary cleric, floor division and Charisma are correct |
| TU-22 | Channel-resistant target; genuine DC bonus; non-applicable spell bonus | Initial/repeat save modifiers and reason identity are correct; no missing resistance, doubled modifier, or inherited unrelated spell bonus |
| TU-23 | Baseline undead immunity and separately granted protection | Ordinary undead remains turnable; unrelated fear/mind-control immunity remains intact; any specific protection/bypass result has a sourced contract and native evidence |
| TU-24 | Mixed living/undead group with altered hit points | No healing, damage, destruction, or allegiance change is produced by Turn Undead |

## D. Persistence, overlap, and UI

| ID | Scenario | Required observable result |
| --- | --- | --- |
| TU-25 | Mid-effect save/load of intelligent and mindless targets | Remaining duration, effect ownership, classification, and recovery context survive; no extra initial save, refreshed duration, charge grant, or missing-fact error |
| TU-26 | Two casters or unrelated existing fear | Successful new save does not clear existing fear; expiry/removal touches only owned effect(s); same-effect overlap follows the documented native policy |
| TU-27 | Existing positive-channel submenu in selected mod profile | New action appears once only for an eligible feat owner, uses the action artwork, and executes the correct source; all preexisting variants remain usable |
| TU-28 | Vanilla profile without that submenu | Discoverable normal Turn Undead action with correct source/icon; no global channel reparenting required to simulate an absent menu |
| TU-29 | Pin execution action, save/load, inspect empty-resource feedback | Hotbar binding survives; correct tooltip, cost, DC where exposed, and disabled state; feat artwork and action artwork have distinct intended consumers |
| TU-30 | Module disable/reenable and supported respec/loss of source | No unresolved saved blueprint or component identity; no stale usable action without eligibility; publication and active-owner behavior follow the documented module policy |
| TU-31 | Existing wizard necromancer | Native ability/resource/DC and action entries preserve baseline behavior; qualifying multiclass cleric use stays distinct |

## E. Compatibility and packaging

| ID | Scenario | Required observable result |
| --- | --- | --- |
| TU-32 | Vanilla profile, then exact supported CotW profile | No hard dependency; menu integration, source qualification, resistance/DC registration, and supported source coverage work in the selected installed versions |
| TU-33 | CotW channel extensions are present | Adding Turn does not manufacture unrequested quick/swift/smite/line/cone variants or change existing heal/harm actions; negative-energy-affinity rewrite does not leak into Turn targeting |
| TU-34 | Repeated blueprint publication and relevant load-order initialization | Stable identities, singular selection/menu entries, preservation of third-party entries, no repeated modifier append or variant collision |
| TU-35 | Supplied icon integration | Two PNG roles match final manifest, actual sprites load at intended dimensions, neither art is substituted with a placeholder, packaged paths and provenance are correct |
| TU-36 | Final owner-review candidate | Focused checks and required native evidence refer to exact candidate; draft PR handoff and state are accurate; staging is restored; no automatic merge/release/human-install change |

## Suggested fixture grouping

Use the existing test/harness infrastructure. These are fixture goals, not invented command names or authority to launch a campaign.

1. **Source and acquisition:** ordinary positive/negative clerics, supported native alternate sources, multiclass source selection, depletion, feat acquisition, and menu/icon binding. Most source-policy edge cases belong in fast production-path tests; use native observations to verify the actual source boundary and normal UI route.
2. **Mixed undead encounter:** a verified mindless skeleton/zombie, an intelligent undead, a channel-resistant undead, an allied undead, a living negative-affinity creature if the profile provides one, and an out-of-range control. Obtain both successful and failed saves through a supported deterministic fixture or recorded actual outcomes; do not replace the product's save logic merely to force a green result.
3. **Round and persistence:** real-time/turn-based execution, intelligent recovery, mindless persistence, minute expiration, and a controlled mid-effect save/load using only the authorized disposable fixture. Exercise overlap/source ownership in the smallest suitable scenario.
4. **Optional provider:** selected CotW menu/source/modifier interaction, then the meaningful native baseline comparison. Avoid an entire unrelated compatibility matrix.

Several matrix rows can share one real observation when its evidence genuinely proves each claim. Pure-policy test results and native observations should remain distinct. An unavailable native machine permits useful implementation and review work but leaves the corresponding native rows BLOCKED/NOT RUN; it does not create permission to assert completion or to widen machine access.

## Reviewer checklist

- Review exact product code, not only the mission ledger or self-reported status.
- Check that the new feat consumes an existing positive-channel resource and has no second damage/heal branch.
- Trace the actual DC/level calculation for every advertised source, including Empyreal and delayed channel progressions.
- Inspect both prerequisite/visibility checks and execution checks; a hidden invalid ability can still be invoked through saved data or scripts.
- Verify classification against the shipped records and real counterexamples; reject Int/name/brain heuristics presented as facts.
- Inspect saved component names, ownership, round timing, native event duplication, and source-specific modifier context.
- Inspect the loaded-menu integration design for existing wrapper preservation and source `Parent` collisions.
- Look for broad CotW registration side effects and global immunity or wizard edits outside scope.
- Verify the two actual artwork consumers and runtime-size UI, not just the existence of files in an art directory.
- Check that reported native evidence belongs to the exact artifact and that required restoration completed.
- Identify each unresolved rule or environment question precisely; do not turn unrelated historical failures into new feature acceptance gates.

## Final evidence index template

```text
Feature: Turn Undead
Product repository / branch / code SHA:
Records SHA, if separate:
DLL SHA-256 / package SHA-256:
Game version / inspected assembly identity:
Build loader/reference profile:
Native runtime loader and mod profile:
Source-contract table:
Classification inventory / supported-content boundary:
Focused checks: commands, nonzero selection, results, durations:
Native fixtures: request/transaction IDs, observer identity, key results:
Action-menu and artwork evidence:
Save/load evidence:
Restoration receipt:
Matrix statuses and evidence locations:
Deferred work / exact reason / effect on completion claim:
Draft PR and independent review outcomes:
Controller state and finalization result:
```

The template is an index, not a replacement for the underlying evidence or the development process's required report schema.
