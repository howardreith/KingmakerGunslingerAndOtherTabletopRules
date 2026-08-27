# Summon Same-Turn Activation Mission

Status: ACTIVE - standalone matrix qualified; compatibility and final version pending

## Identity

- Starting qualified master: `cf1ca7aedf34ee76690f8864daedc9319a8e21a6`
- Starting version: `0.0.103`
- Working branch: `codex/summon-same-turn-activation`
- Target candidate after complete qualification: `0.0.104`

The starting SHA is both the fetched `origin/master` and local clean `master`.
It is tagged `v0.0.103` and is the merge named **Merge overhaul, summon menu,
and fatigue escalation release**.

## Narrow objective

Identify the exact Kingmaker 2.1.7b combat/turn boundary that deprives a
genuine summon created during its caster's active turn of its first lawful
action opportunity. Repair that boundary for every successfully spawned unit
from an actual accelerated summoning spell, exactly once, without changing the
caster's remaining actions or normal later-round scheduling.

The repair must be owned by authoritative summon provenance plus live
turn-based state. It must not be owned by Acadamae Graduate, spell names, unit
names, arbitrary entity creation, or a new spell action type.

## Investigation order

1. Reproduce the defect through a real spellbook, `AbilityData`,
   `UnitUseAbility`, `RuleCastSpell`, summon graph, and live turn-based
   controller.
2. Compare ordinary native, Acadamae Standard-action, and legitimate Quickened
   summon lifecycle state at the first meaningful divergence.
3. Inspect the exact installed 2.1.7b assemblies and bounded IL for summon
   creation, combat enrollment, initiative, current-round membership, acted
   state, AI/controller state, and later-round cleanup.
4. Implement the smallest native-compatible, summon-scoped, idempotent repair.
5. Qualify focused/domain, assembly-backed, full repository, build/package,
   runtime, compatibility, regression, duration, and save/load gates.

## Frozen accepted behavior

The 0.0.103 summon popup, Expanded Summoning player path/roster/quantity/
templates/duration, Acadamae eligibility/toggle/Standard action/save/fatigue/
exhaustion/Cord behavior, ordinary summon timing, and all unrelated systems are
regression constraints. They are not redesign targets.

## Hard evidence boundary

No build, domain test, direct unit creation, screenshot, UI appearance, or
successful spawn alone qualifies the repair. Acceptance requires guarded real
Kingmaker processes launched through Steam App ID 640820 and the actual player
spell path. Ambiguous evidence is failure.

Only `KMG_AUTOMATION_WORKING` may be used for authorized save-backed automation.
`KMG_AUTOMATION_BASELINE` must never be selected or modified.

## Completion contract

Completion requires the entire matrix in
`planning/SUMMON-SAME-TURN-ACTIVATION-MATRIX.md`, two fresh-process repeats of
the principal accelerated cases, exact artifact/profile restoration, version
advancement once, clean-tree and remote equality, and a truthful distinction
between source qualification, automated runtime qualification, and later
human acceptance. The branch must be pushed but never merged autonomously.
