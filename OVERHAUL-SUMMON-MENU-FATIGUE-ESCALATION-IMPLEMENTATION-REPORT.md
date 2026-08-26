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

The complete dependency-free suite currently passes 1,288/1,288. New focused
coverage includes 19 menu-layout cases, 13 canonical fatigue policy/contract
cases, prompt overhaul delivery and source guards, and extended Acadamae, Cord,
Expanded Summoning, maintenance, and guarded-scenario contracts. Runtime
fixtures exercise the actual production ability, exact action-bar view, exact
canonical blueprint facts, native rule application, real guarded save/load,
rest, and
the established Cord damage path rather than treating pure policy calls as
mechanical proof.

Kingmaker's `UnitSerialization` preview copier is explicitly not used as save
evidence because installed 2.1.7b activates `UnitStripConverter`, which strips
`BuffCollection`. The persistence proof instead performs exact native
`Game.SaveGame` writes on the correlated `KMG_AUTOMATION_WORKING` descriptor,
reloads in fresh Steam launches, verifies one permanent canonical Exhausted
fact with its independent rooted context, cleans it, saves, and verifies absence
on a third no-write launch. The fixed subject is `party[1]`; a pre-existing
canonical Fatigued fact on `party[0]` is preserved and never treated as fixture
state.

Expanded Summoning duration qualification also follows the exact installed
contract. Installed `RuleSummonUnit.OnTrigger` applies its requested `Duration`
plus `BonusDuration`, then adds a six-second native lifecycle grace to the
canonical `SummonedUnitBuff`. The guarded fixture captures those rule values
and matches that exact blueprint fact; it does not infer duration from a buff
name or treat the native grace as a KMG mechanics regression. An untouched
0.0.102 baseline run reproduced the former stale 121-second assertion, while
the corrected contract passed on the immutable 0.0.103 candidate.

The immutable code commit passed 19 guarded Steam launches covering the three
changed production surfaces, both save/load persistence loops, working-save
smoke, enabled/disabled module restarts, and Call of the Wild present/absent
profiles. The only remaining boundary is the supervised actual-popup
presentation and navigation check; no autonomous coordinate or input
automation was used.

Qualification evidence, immutable commit identity, runtime run IDs, and final
hashes are maintained in
`docs/OVERHAUL-SUMMON-MENU-FATIGUE-ESCALATION-QUALIFICATION.md`.

## 0.0.103 human-acceptance repair

The first installed-candidate human check retained the successful prompt
overhaul and fatigue behavior but rejected two presentation/fixture surfaces.
At 1600x900, with the expanded summon parent in the first left-sidebar slot,
the popup began several slots lower around mid-sidebar instead of at the top
safe margin. The existing UMM misfire button also stopped reliably producing
the Normal-to-Broken-to-Wrecked fixture sequence.

Installed 2.1.7b contracts and call sites identified the exact menu source as
the private zero-argument `ActionBarGroupSlot.OnToggleGroupClick`, which
synchronously calls the shared `ActionBarSpellsGroup.Toggle` instance through
that slot's `SubGroup`. The first adapter instead rediscovered
`group.GetComponentInParent<ActionBarGroupSlot>()`; that can identify the
shared popup hierarchy rather than the clicked visible slot. It also applied a
canvas-space target by guessing the root pivot position and copied the native
middle/lower child alignment into newly full-height scroll content. Those
three adapter assumptions explain why a mathematically clamped policy result
could still render halfway down the sidebar.

The correction captures and consumes the exact clicked source slot around the
native Toggle call, rejects a missing/stale source instead of guessing, keeps
the native opening direction, and performs nearest-edge clamping. A top
crossing clamps its top edge to the safe top; a bottom crossing clamps its
bottom edge to the safe bottom. Oversized content spans the complete safe
vertical range and begins at its first option. Root placement now translates
the measured rendered bounds after sizing, so arbitrary pivots, anchors,
parent transforms, safe areas, and canvas scale do not change the requested
rectangle. One structured record per open includes source spell/slot identity
and hierarchy, exact and fallback anchor rectangles, native/content/safe/final
and rendered rectangles, viewport and endpoint-slot rectangles, root
pivot/anchors, canvas mode, scrolling state, direction, and clamped edge.

The firearm failure was reproduced in the real UMM output log. The legacy
`DamageFirstEquippedFirearmForDebug` path repeatedly reported that the selected
unit had no exact firearm equipped even while a production firearm was visibly
equipped. That path mixed reflection-based selected-or-main fallback, broad
multi-equipment-set discovery, and a first-candidate choice with an error still
worded specifically for Test Musket. It therefore neither enforced the actual
selected unit/current hands nor provided an unambiguous production-firearm
target.

The replacement UMM controls use `SelectionManager.GetSingleSelectedUnit`,
require current party membership and no active selected-unit/party combat, and
delegate current-hand resolution to the production
`ExactEquippedFirearmResolver`. They fail closed for zero or two supported
firearms. A focused policy permits only canonical Normal-to-Broken and
Broken-to-empty/Wrecked transitions. The bridge revalidates at commit, verifies
the same runtime reference, item ID, blueprint/type, repository identity and
reference hash, revision, loaded-state rule, and static blueprint enchantments,
and restores the prior repository state if post-verification fails. The same
bridge methods now run inside `disposable-overhaul-maintenance`, whose resulting
Wrecked production item then feeds the actual player-facing Overhaul ability.

These corrections retain version 0.0.103 and do not alter summon blueprints,
rosters, spell mechanics, firearm balance, prompt Overhaul delivery, fatigue,
Acadamae, or Cord behavior. Final popup presentation remains intentionally
human-gated after installation; automated geometry and read-only observation
can establish bounds and reachability but cannot accept the visual result.

The final runtime-qualified code commit is
`a1f9e5e26ce6b8c0bc00625ff337181a25e30fe6`. Its complete suite passes
1,288/1,288; exact-reference Release, output, asset, SoundBank, deterministic
reconstruction, and strict package gates pass. Six guarded final launches pass:
the expanded 16-assertion overhaul/development-control fixture, all four
applicable Expanded Summoning inventory/mechanics/player/visual-contract
surfaces, and working-save smoke. The installed package and built DLL are
identified in the qualification report; the installed DLL is byte-equal to the
qualified DLL. Only renewed human popup placement/navigation acceptance remains.

The renewed human check showed native off-screen placement again. The matching
game log conclusively recorded `variant-menu.layout-failed` on every open with
`Rendered popup size must match the target before translation`. The adapter's
three sizing passes had left ordinary fractional rendered-size drift; a strict
0.01-canvas-unit equality guard then threw before translation, and the guarded
failure path correctly restored the native hierarchy. The final correction
constructs the translation target from the measured rendered size, aligns its
top or bottom to the already-selected safe edge, clamps the remaining axis,
and verifies the actual result against the safe rectangle. No blueprint,
roster, spell, firearm, maintenance, or condition code changed.
