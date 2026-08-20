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
