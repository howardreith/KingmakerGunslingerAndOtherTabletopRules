# Overhaul, summon-menu, and fatigue escalation implementation report

## Scope and baseline

The selected authoritative baseline is
`970aeb7972dcb155d5789a636ba156e68d1c0d0a`, the fetched `origin/master` at
intake and the merge containing the released 0.0.102 starter-firearm, Bokken,
combat-log, and Acadamae-toggle fixes. Work is isolated on
`codex/overhaul-summon-menu-fatigue-escalation`. No blueprint identity,
summon roster, firearm balance, ammunition rule, or accepted Acadamae/Cord
adaptation is redesigned.

## Root causes

### Overhaul Firearm

The production `AbilityCustomLogic` deliberately calculated
`TimeController.GameTime + 60 seconds`, returned `yield null` until that point,
and committed only after the clock reached it. The transaction itself was
already exact-item, atomic, rollback-capable, and idempotent; the opaque delay
was entirely in player-command delivery.

### Expanded Summoning variant menu

Installed Kingmaker 2.1.7b IL identifies the PC surface as
`Kingmaker.UI.ActionBar.ActionBarSpellsGroup.Toggle(UnitEntityData,
IEnumerable<AbilityData>, AbilityData)`. Its native `FillSlots` creates every
`ActionBarSpontaneousConvertedSlot` directly under one layout root. The view
does not measure the completed list against its rendered canvas, select the
better opening direction, clamp to a safe rectangle, or provide an overflow
viewport. Expanded Summoning correctly published larger native variant arrays,
which exposed that unbounded native placement when the owner icon was near a
screen edge.

### Canonical Fatigued escalation

Installed 2.1.7b `BuffCollection.TriggerRuleApplyBuff` publishes
`RuleApplyBuff` and returns null when the application is rejected.
`AddBuffInternal` handles stacking/merging, while `UnitState.AddCondition`
counts and immunizes conditions. None of those installed boundaries upgrades a
second exact canonical Fatigued fact to canonical Exhausted. Acadamae therefore
used a valid canonical Fatigued blueprint but had no general escalation rule.
The earlier Cord interception also substituted canonical buffs before the
native success result was known, making it unsuitable as the shared boundary.

## Implementation

### Prompt exact-item overhaul

`OverhaulTestMusketAbilityLogic` now performs one synchronous delivery step.
It evaluates the exact equipped empty Wrecked firearm and kit at entry,
revalidates the same runtime item at the commit boundary, then invokes the
unchanged `FirearmOverhaulTransactionService`. Cancellation before delivery,
combat, changed equipment, a loaded/replaced firearm, missing inventory, or an
ambiguous pair all fail without mutation. The ability keeps its ordinary
full-round animation, publishes the existing concise native combat-log result,
and creates no timer, polling loop, pending operation, or world-time mutation.

The installed supported safe-use predicate remains the exact caster
`UnitEntityData.IsInCombat` contract already used by this ability. No reliable
broader party-combat predicate was found at the inspected installed boundary,
so this patch does not infer safety from targets, maps, UI, music, or distance.

### Canvas-space variant-menu layout

`SummonVariantMenuLayoutPolicy` is a pure canvas-space service over an anchor
rectangle, desired content size, safe rectangle, margin, and preferred opening
direction. It chooses the fitting or larger side, clamps both axes, bounds an
oversized menu, and reports vertical/horizontal scroll requirements.

The thin Harmony adapter targets only the exact installed PC
`ActionBarSpellsGroup` Toggle/Hide lifecycle and only parents carrying KMG's
exact published Expanded Summoning `AbilityVariants` marker. It forces layout,
measures the actual runtime slots, intersects `Screen.safeArea` with the root
canvas pixel rectangle, converts that rectangle into canvas space, and leaves
already-fitting native menus untouched. Overflow uses one reusable
`ScrollRect`/`RectMask2D` scaffold, reparents the same native slot objects in
their exact order, copies the native layout group, and restores the original
hierarchy before refill or hide. It does not change variants, casting,
selection, alignment filtering, or third-party-added runtime slots.

The guarded supervised observer is read-only. After a human opens the exact
working save and then the largest published menu near the top-left sidebar, it
measures the actual rendered bounds and verifies one bounded viewport plus
first/middle/final reachability. It never sends input, selects a slot, or casts.

### Post-success canonical condition coordination

`CanonicalFatigueApplicationRuntime` patches only the exact private
`BuffCollection.TriggerRuleApplyBuff(BlueprintBuff, MechanicsContext,
TimeSpan?)` overload and matches the installed canonical blueprint references
(`e6f2fc5d73d88064583cb828801212f4` and
`46d1b9cc3d0fd36469a471b047d773a2`). The native rule always runs first. A null
result or missing native condition after immunity is treated as blocked and
cannot escalate or invoke Cord.

After success, a request-local thread scope applies the deterministic state
table, preserves the longest/permanent source duration and context, removes
duplicate canonical facts, and uses a recursion depth only for the related
native Exhausted/Fatigued replacement application. Two synchronous requests
therefore observe live committed state and deterministically reach one
Exhausted fact. Direct Exhausted applications remain idempotent and never
downgrade.

Acadamae's independent failed-save consequence now calls the same adapter and
makes the resolved canonical condition permanent/rest-removable. Its command
eligibility, DC, toggle snapshot, save timing, cancellation, and spell context
remain unchanged. Cord receives the effective incoming condition only after
native success, deals its established direct 1d6 equivalent damage once, keeps
the 1-HP floor, and cannot recursively re-enter the canonical coordinator.

## Regression coverage

The complete dependency-free suite currently passes 1,278/1,278. New focused
coverage includes 14 menu-layout cases, 13 canonical fatigue policy/contract
cases, prompt overhaul delivery and source guards, and extended Acadamae, Cord,
Expanded Summoning, maintenance, and guarded-scenario contracts. Runtime
fixtures exercise the actual production ability, exact action-bar view, exact
canonical blueprint facts, native rule application, serialization/rest, and
the established Cord damage path rather than treating pure policy calls as
mechanical proof.

Qualification evidence, immutable commit identity, runtime run IDs, and final
hashes are maintained in
`docs/OVERHAUL-SUMMON-MENU-FATIGUE-ESCALATION-QUALIFICATION.md`.
