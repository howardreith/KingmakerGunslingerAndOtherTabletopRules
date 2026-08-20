# Overnight Gunslinger Bug-Fix Implementation Report

Status: MISSION CHECKPOINT IN PROGRESS

## Baseline

The mission starts from fetched published origin/master
d13268d3abe9ffe89c8195b213c1eee194328672 on the isolated branch
codex/gunslinger-overnight-bugfixes. Version is 0.0.87 and the unchanged source
passes repository validation, 1,150 deterministic tests, clean exact-reference
Release, build-output, SoundBank, deterministic package, and strict package
validation.

No production behavior has changed at this checkpoint. Fresh human reports
reopen the twelve matrix rows and supersede contradictory historical automated
acceptance. Each subsequent section and commit will remain issue-scoped and
will state implementation, tests, runtime evidence, uncertainty, and any
remaining human gate.

## Issue status

Issues 1 through 12 are pending in the controlling order. The exact current
state is maintained in planning/OVERNIGHT-GUNSLINGER-BUGFIX-MATRIX.md.

## Issue 1 - Acadamae Graduate

Source-qualified candidate in progress. The prior tracker recognized a
completed cast only while its command was the thread-local active `OnAction`
command. The repair attaches the concrete `RuleCastSpell` created by that
eligible command to the tracker entry and consumes the rule exactly once at
the terminal callback, even when that callback is delayed. Failed casts and
canceled commands consume their entries without a save.

The action policy, toggle, prepared-arcane eligibility, exact Summoning marker,
DC, canonical Fatigued blueprint, null MechanicsContext, permanence, and Cord
contract are unchanged. New bounded diagnostics expose the actual d20,
Fortitude modifier/total, DC, outcome, and fatigue disposition for eligible
completed casts only. The guarded scenario now includes actual native command
success and failure paths instead of proving those paths through manual tracker
calls. All source/build/package gates pass; guarded runtime remains pending the
immutable issue commit.

The first immutable runtime attempt exposed a fixture limitation before any
assertion: a detached `ChargenUnit` cannot advance a queued command through its
animation controller in one synchronous tick. The corrected save-free fixture
now invokes the exact protected `UnitUseAbility.OnAction()` boundary. It does
not manually call the production tracker or construct `RuleCastSpell`, so the
native action and all repaired Harmony correlation points remain exercised.

That direct native action also proved unavailable on the detached fixture: an
installed composed `OnAction_Patch3` dereferenced loaded-area state. The final
safe automated mode therefore targets the production defect itself. It creates
the native `RuleCastSpell` while the exact command scope is active, ends that
scope, and only then triggers the rule. This requires the new constructor-time
rule identity retention to work and continues through native Rulebook save and
fatigue handling. It does not claim animation-driven execution or area change;
those remain human-gated.

The first completed delayed-terminal run proved correlation and consequences,
but found the new diagnostic labels were inverted by the old forced-roll
fallback. Installed getter IL proves `BaseRollResult` is d20 plus `StatValue`,
and `RollResult` adds a conditional success bonus. The test postfix no longer
writes `BaseRollResult`; natural rolls are controlled only at native
`RuleRollD20.PreRollDice`. Production diagnostics now report the native d20,
Fortitude `StatValue`, conditional bonus, final total, DC, outcome, and fatigue
disposition separately.

Final automated disposition: guarded run
`20260820T0428503321600Z-d97a49371e1949c89f3de25aac1c6eff` passed all 14
assertions on published commit `f807eb1cc3dabf9dc66acaa2b773c029a72dc942`.
The installed saving-throw path did not call `RuleRollD20.PreRollDice`, so the
guarded-only completion control now sets `BaseRollResult` to the requested
natural plus the actual native `StatValue`. Production diagnostics and
uncontrolled saves remain native. Loaded-area animation execution, area-change
persistence, and visible UI/log presentation remain consolidated human checks.

## Issue 2 - Focused Aim

Source-qualified candidate in progress. The prior ability manually debited the
shared Grit resource before activating its timed damage marker. The candidate
reverses that vulnerable ordering: it first establishes the marker, then
rechecks the live True Grit decision and authoritative shared pool, commits the
effective debit, and verifies the exact post-spend amount. Any unavailable or
inconsistent debit removes the marker, so damage cannot survive a failed spend.

Exact ability-fact ownership is required, armed duplicates remain unavailable,
ordinary uses spend one, and a legally selected True Grit Focused Aim remains a
zero-cost activation requiring positive Grit. The native `AbilityResourceLogic`
UI component still points at the same shared resource and remains presentation
only. A new guarded scenario exercises the real resource collection and native
weapon-stat rule for firearm damage and crossbow isolation. Runtime is pending
publication of the immutable candidate.

## Issue 2 - Focused Aim transactional Grit spend

The verified live defect was not fact-activation reconciliation. Kingmaker materialized the exact Focused Aim buff in the owner's `RawFacts` collection while returning null from `Buffs.AddBuff`. The previous code treated that return value as an activation failure and restored the resource snapshot, but the surviving marker continued to grant the firearm Charisma damage bonus.

The repair resolves only the exact project-owned marker blueprint from `RawFacts` when the native return is null. It then evaluates the established True Grit policy against the live owner, spends the shared Grit resource once, verifies the exact before/after delta, and removes the marker while restoring the snapshot if the commit cannot be proven. Ability ownership, positive-Grit True Grit semantics, firearm-only damage, wrong-owner rejection, duplicate rejection, and feature lifecycle reconciliation are retained.

Commit chain: `24c2bae`, `c8f58e0`, `75d7bc0`, `de46051`, `24ffb78`. Guarded run `20260820T0458550047640Z-b20727d24cc24d3297e0e9d23d385235` passed all seven assertions against `24ffb78ac6821d4aec173213df1f046940be683b`.

## Issue 3 - Firearm Touch AC range and feedback

The existing runtime architecture was retained: the attack-roll frame captures exact firearm identity and per-attack range modifiers, while `RuleCalculateAC` re-reads authoritative participant distance and target AC values. The decision service now applies the repository's completed era policy rather than the Sprint 9 advanced-firearm deferral: one effective increment for early firearms and five for advanced firearms. The contextual ordinary-to-touch delta remains additive, so cover and other native event adjustments are preserved.

Production and magic firearm descriptions plus the dynamic Qualities surface now state exact base penetration distances. Blunderbuss wording limits this to ordinary direct fire and preserves Scatter Shot's cone rules. A new exception-contained player log adapter reuses the existing native warning/battle-log event and publishes one line after a resolvable exact-firearm branch commits. Duplicate callbacks are stamped, and adapter/log failures do not alter native attack processing.

No established safe pre-attack hover seam was found after bounded searches of tooltip, cursor, target-preview, and combat-log adapters. The battle-log fallback is therefore the production candidate; pre-attack hover remains human/UI follow-up rather than a new framework.
