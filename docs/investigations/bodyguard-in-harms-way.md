# Bodyguard and In Harm's Way native-contract investigation

Date: 2026-08-22

## Mission baseline

- Starting local branch: `master`
- Starting local HEAD: `1c78c0e7936745bb82cdb6516298597aabba7ec4`
- Authoritative `origin/master`: `1c78c0e7936745bb82cdb6516298597aabba7ec4`
- Starting divergence: `0 0`
- Working branch: `codex/bodyguard-in-harms-way`, created directly from that
  remote commit
- Starting mod/package version: `0.0.89`
- Starting package:
  `artifacts/packages/KingmakerGunslinger-0.0.89-urban-barbarian.zip`
- Starting package SHA-256:
  `7f8a384a808cec0d570a4f50d634ad2f5114b7686a907b8b140f894287205e2d`
- Pre-change Release domain suite: 1,164 passed, 0 failed

## Installed game contract

The inspected assembly is the private installed Steam Kingmaker reference at
`Kingmaker_Data/Managed/Assembly-CSharp.dll`.

- Supported game: Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b
- Assembly identity:
  `Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null`
- File length: 7,262,208 bytes
- MVID: `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`
- SHA-256:
  `3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB`

`UnityEngine.Application.version` reports `UNKNOWN` in this installation.
The guarded observer therefore treats the exact loaded Assembly-CSharp hash
and MVID above as the authoritative 2.1.7b build contract; it does not weaken
the check to an unknown version string.

The raw disassembly is a machine-local ignored artifact and is not eligible
for commit. This document records only the derived contracts needed by the
implementation.

## Native Combat Reflexes identity

The exact Kingmaker Combat Reflexes `BlueprintFeature` GUID is:

`0f8939ae6f220984e8fb568abbdfba95`

The supported library is required to resolve that exact GUID to the internal
feature `CombatReflexes`. Runtime lookup must use `BlueprintLibraryLookup` and
must fail closed if the GUID, type, internal identity, or required native
mechanics no longer match. Localized-name scanning is not an accepted lookup
path.

## Attack lifecycle

The following exact methods and fields were inspected:

- `Rulebook.TriggerEventInternal<T>(T)`
- `RulebookEventBus.OnEventAboutToTrigger(RulebookEvent)`
- `RulebookEventBus.OnEventDidTrigger(RulebookEvent)`
- `RulebookEventContext.PopEvent(RulebookEvent)`
- `RuleAttackWithWeapon.OnTrigger(RulebookEventContext)`
- `RuleAttackWithWeapon.CreateRuleDealDamage(bool)`
- `RuleAttackWithWeapon.LaunchProjectile(BlueprintProjectile, bool)`
- `RuleAttackWithWeaponResolve.OnTrigger(RulebookEventContext)`
- `RuleAttackRoll.OnTrigger(RulebookEventContext)`
- `RuleCalculateAC.OnTrigger(RulebookEventContext)`
- `AbilityDeliveryTarget.set_AttackRoll(RuleAttackRoll)`
- `AbilityExecutionProcess.ApplyEffect(AbilityExecutionContext,
  AbilityDeliveryTarget, AbilityApplyEffect, AbilitySelectTarget)`
- `ElementsContextData.Dispose()` as invoked from the existing
  `ContextAttackData` finally block in `ApplyEffect`

`Rulebook.TriggerEventInternal` performs the event bus `AboutTo` phase, the
event's `OnTrigger`, then the event bus `Did` phase and `OnDidTrigger`. Each
phase is exception-isolated by the native rulebook.

For a weapon attack, `RuleAttackWithWeapon.OnTrigger` first calculates weapon
stats, constructs and triggers one `RuleAttackRoll` against the parent's
current `Target`, stores the completed roll, and only then constructs damage or
launches the projectile. `CreateRuleDealDamage` preserves the completed
`RuleAttackRoll`, including critical and precision state, on the resulting
`RuleDealDamage`. `RuleAttackWithWeaponResolve` consumes mirror image and
triggers that already-built damage event only when the original roll is a hit.

This ordering permits Bodyguard to run before the nested AC calculation and
permits In Harm's Way arbitration immediately after the nested roll finishes,
without rerolling or cloning the weapon attack.

## Weapon-delivery redirection seam

`RulebookTargetEvent.Target` is an exact public readonly
`UnitEntityData` field. `RuleAttackWithWeapon` also exposes the native
`ReplaceTarget`/`NewTarget` damage-only facility.

`ReplaceTarget` alone is insufficient. Native and project on-hit components
such as weapon debuff/application handlers and Bleeding Wound consume the
parent `RuleAttackWithWeapon.Target` during the parent's `Did` phase rather
than consuming `RuleDealDamage.Target`. A `NewTarget`-only implementation
would therefore redirect HP damage while leaving associated effects on the
original ally.

The selected weapon seam is:

1. Keep the original parent target unchanged through `RuleAttackRoll`, AC,
   concealment, mirror-image selection, critical threat, and confirmation.
2. In the shared `RuleAttackRoll.OnTrigger` postfix, after a successful roll
   and successful native immediate-action expenditure, set the exact parent
   `RulebookTargetEvent.Target` field to the interceptor through a validated
   reflection accessor.
3. Leave the substituted target in place through damage construction,
   projectile resolution, and all global/target/initiator `Did` subscribers
   for the parent attack.
4. Restore the original target from a prefix on
   `RulebookEventContext.PopEvent(RulebookEvent)`. Native
   `Rulebook.TriggerEventInternal<T>` catches faults from `OnTrigger`, the
   event-bus `Did` phase, and `OnDidTrigger` before it calls `PopEvent`, so this
   is the exact exception-safe completion boundary available to Harmony 1.2.

This makes the original attack's single native delivery pipeline choose the
interceptor. It does not create another `RuleAttackWithWeapon`, projectile,
attack command, or attack roll. The accessor must verify the exact declaring
type, public instance field, readonly contract, and `UnitEntityData` field type
before an immediate action can be spent. Contract failure disables
interception and leaves the original target untouched.

Existing `ReplaceTarget` state is preserved and restored exactly. An attack
already redirected before this subsystem reaches arbitration is not eligible
for another interception chain.

## Ability, ray, and touch delivery seam

The installed native `RuleAttackRoll` construction sites outside
`RuleAttackWithWeapon` occur in the attack-roll delivery implementations for
Clashing Rocks, projectile/ray delivery, and touch delivery. These paths
construct an `AbilityDeliveryTarget` for the original target and assign the
completed `RuleAttackRoll` with the exact public setter:

`AbilityDeliveryTarget.set_AttackRoll(RuleAttackRoll)`

`AbilityDeliveryTarget` contains an exact public readonly `TargetWrapper
Target` field. `AbilityExecutionProcess.ApplyEffect` checks the attached
roll's `IsHit` result and then uses this `Target` for the complete
`AbilityApplyEffect` pipeline. Damage, conditions, saves, and other actions in
that effect pipeline therefore share this recipient.

The selected ability seam records the exact completed `RuleAttackRoll` after
successful arbitration and replaces the exact delivery object's `Target`
field in the `set_AttackRoll` postfix. The roll remains against the original
ally; only the delivery wrapper changes. The reflection accessor must validate
the exact setter and readonly field contracts before an immediate action can
be spent. Parentless rolls are eligible only when their exact native reason is
an `AbilityExecutionContext`; arbitrary diagnostic rules are ignored.

The exact private static overload
`AbilityExecutionProcess.ApplyEffect(AbilityExecutionContext,
AbilityDeliveryTarget, AbilityApplyEffect, AbilitySelectTarget)` is the
delivery lifetime boundary for that wrapper. Its existing native IL creates a
`ContextAttackData` and disposes it from a `finally` block after the complete
effect has consumed or captured the redirected recipient. A prefix on the
inherited `ElementsContextData.Dispose()` restores the wrapper on both normal
and exceptional delivery; an `ApplyEffect` postfix covers the early
`applyEffect == null` return. Failure to resolve either exact method is part of
the fail-closed interception contract.

`AbilityDeliveredByWeapon` also copies the enclosing weapon attack's exact
roll into an `AbilityDeliveryTarget`, so this same mapping redirects
attack-linked ability effects after the weapon event has restored its parent
target.

## Attack-family implications

The weapon seam covers ordinary melee and ranged weapons, natural and unarmed
weapon entities, attacks of opportunity, full-attack iterative attacks,
physical/elemental damage bundles, critical damage, precision damage, and
weapon-linked on-hit components that consume the parent target.

The delivery-wrapper seam covers the installed projectile/ray, melee-touch,
ranged-touch, and other native attack-roll ability delivery paths that assign
their roll to `AbilityDeliveryTarget`. Area effects, saving-throw-only spells,
damage-over-time ticks, environmental damage, splash damage without its own
attack roll, and combat maneuvers against CMD are intentionally excluded.

The runtime qualification must include a genuine non-HP rider. A green build
or an HP-only test cannot establish complete In Harm's Way support.

## Attack-of-opportunity economy

The exact native contracts are:

- `UnitCombatState.AttackOfOpportunityCount` public getter/setter
- `UnitCombatState.AttackOfOpportunityPerRound` public getter
- `UnitCombatState.CanAttackOfOpportunity` public getter
- `UnitCombatState.AttackOfOpportunity(UnitEntityData, bool)`

`CanAttackOfOpportunity` requires a positive native remaining count and rejects
the native `DisableAttacksOfOpportunity`, `CannotAttack`, and prohibited-action
conditions. The actual `AttackOfOpportunity` method also enforces
`CanActInCombat`, or native condition value 39
(`AttackOfOpportunityBeforeInitiative`), then `CanAttackOfOpportunity` and
`UnitState.CanAct`. That condition is the native Combat Reflexes flat-footed
authority.

The actual method is rejected as the Bodyguard spend path because it creates a
`UnitAttackOfOpportunity`, raises attack events, starts animation/weapon
delivery, and only then decrements the count. Bodyguard must consume currency
without making that attack.

The selected spend transaction first calls
`UnitCombatState.AttackOfOpportunity(attacker, simulate: true)`. In the exact
2.1.7b IL, that executes the complete native eligibility path—including
`CanActInCombat` or the Combat Reflexes condition, transient AoO suppression,
targetability, motion, threat-hand, memory, and remaining-use checks—but its
`simulate` branch returns before `UnitAttackOfOpportunity`, handler events,
cooldown mutation, or count expenditure. After a successful simulation the
adapter decrements `AttackOfOpportunityCount` synchronously by exactly one and
verifies the committed value. It creates no command or attack. Direct count
decrement is also the resource-consumption pattern used by Kingmaker's own
designer Bodyguard component, although that component is otherwise unsuitable
because it performs no Aid Another roll and uses incomplete eligibility.

The native cooldown controller remains the authority for refresh and for the
Combat Reflexes-derived `AttackOfOpportunityPerRound` value. No custom AoO
counter is introduced.

## Immediate/swift action economy

Kingmaker 2.1.7b has no separate immediate-action counter. Its shared
swift/immediate budget is:

- `UnitEntityData.HasSwiftAction()`:
  `CombatState.Cooldown.SwiftAction <= 0`
- `UnitEntityData.UpdateCooldowns(UnitCommand)`: a native Swift command adds
  exactly `6.0f` to `CombatState.Cooldown.SwiftAction`
- the RTWP action controller and native cooldown controller consume and refresh
  the same field; official turn-based mode also reads it

The selected immediate transaction first calls `HasSwiftAction`, then adds the
native six-second quantum to the exact `SwiftAction` cooldown and verifies
that the shared budget became unavailable. No per-round dictionary or custom
counter is authoritative. Only the first stable candidate whose native spend
commits is selected.

## Threat and adjacency

The exact native threat methods are:

- `UnitEngagementExtension.GetThreatHand(UnitEntityData)`
- `UnitEngagementExtension.IsMeleeWeapon(WeaponSlot, UnitEntityData)`
- `UnitEngagementExtension.IsReach(UnitEntityData, UnitEntityData, WeaponSlot)`
- `UnitEngagementExtension.IsEngage(UnitEntityData, UnitEntityData)`

`IsMeleeWeapon` requires a present melee weapon and rejects native unarmed
weapons unless `UnitMechanicsFeatures.ImprovedUnarmedStrike` is active.
`GetThreatHand` uses primary hand, then a legal secondary hand, then additional
limbs. `IsReach` compares native `DistanceTo` with both unit corpulence values
plus the selected weapon's `AttackRange`, and requires native line of sight.
`IsEngage` additionally requires `UnitState.CanAct` and rejects total
concealment.

Bodyguard must consider all currently valid primary, secondary, and additional
limb slots, apply the same native melee/reach rules to each, calculate each
target-aware attack bonus, and select the highest bonus with stable slot order
as the tie-break. This preserves reach weapons, size/corpulence changes,
natural attacks, polymorph, disarm, and native unusual unarmed threat.

Ally adjacency is a distinct five-foot edge-distance test. It uses native
center distance minus both full corpulence values, with the repository's
existing small floating-point tolerance. Adjacency to the ally never replaces
the separate native threat test against the attacker.

## Aid Another attack calculation

The selected safe path is:

1. Trigger native `RuleCalculateAttackBonus(protector, attacker,
   selectedMeleeWeapon, 0)` for each qualifying threat candidate.
2. Select the highest current target-aware `Result` before any d20 is rolled.
3. Trigger one native `RuleRollD20(protector)` for the selected attack.
4. Apply attack-roll semantics against AC 10: natural 1 fails, natural 20
   succeeds, otherwise `d20 + attack bonus >= 10` succeeds.

`RuleCalculateAttackBonus` wraps the native no-target attack calculation and
adds target-aware concealment, flanking, and other target relations.
`RuleRollD20` uses native RNG and the native d20 reroll event pipeline.

The guarded qualification fixture uses inherited
`RuleRollDice.Override(int)` before triggering its request-local
`RuleRollD20`. This exact native override remains authoritative when an
installed dice-control mod replaces `PreRollDice` or the ordinary incoming
roll assignment. The override queue cannot be armed by player content and is
always cleared with the disposable fixture; production Bodyguard rolls remain
native random rolls.

This is narrower and safer than constructing a synthetic `RuleAttackRoll`.
It creates no `RuleAttackWithWeapon`, `UnitAttack`, `UnitCommand`, projectile,
damage bundle, ammunition discharge, reload, misfire, grit event, critical
pipeline, animation, or on-hit action. A short-lived synthetic-Aid guard still
marks these nested calculation events so this subsystem cannot recurse.

## Bodyguard AC seam

`RuleCalculateAC` is nested inside the original `RuleAttackRoll` before hit
classification. The shared project AC postfix first completes firearm
touch-AC selection, then Bodyguard adds one attack-scoped aggregate of two per
successful distinct protector to the exact AC event. This order prevents the
firearm touch-AC replacement from erasing Bodyguard's contribution.

The installed presentation contract is exact:

- the `RuleCalculateAC` constructor creates its public
  `List<BonusSource> BonusSources`;
- `RuleCalculateAC.AddBonus(int, Fact)` increments the private calculation
  bonus and appends the same value/fact pair to `BonusSources`;
- `RuleCalculateAC.OnTrigger` consumes that private calculation bonus while it
  computes `TargetAC`;
- `AttackLogMessage.AppendArmorClassBreakdown(StringBuilder,
  RuleCalculateAC)` prints the already-final `TargetAC`, then passes the
  event's `BonusSources` to `StatModifiersBreakdown.AddBonusSources`;
- `AddBonusSources` renders each entry through its source fact's
  `IUIDataProvider.Name`.

The original implementation wrote `TargetAC` only in the shared postfix.
That made the correct final AC participate in hit resolution but supplied no
line item to `AppendArmorClassBreakdown`, which explains the human-observed
15 total with only base 10 and armor +3 displayed.

The repair retains that proven post-firearm aggregate write and appends one
display-only BonusSource of +2 for each successful protector, using that
protector's actual project-owned Bodyguard feature fact. The Bodyguard feat's
localized `IUIDataProvider.Name` is `Bodyguard`, so the native expanded attack
details render a truthful named line. Multiple protectors produce multiple
native +2 entries; Kingmaker may render them as separate same-name lines.

The display-only source append occurs after native `OnTrigger`, so it does not
change TargetAC. The existing aggregate write changes `TargetAC` exactly once;
the two operations therefore cannot double-count. Calling `AddBonus` in the
postfix was rejected because it would mutate the already-consumed private
calculation bonus without applying it automatically. A target AC temporary
modifier was also rejected: the source fact belongs to another unit, the
protector, and a target-stat mutation would introduce unnecessary cross-unit
lifetime and rollback risk.

Both `TargetAC` and newly appended source entries are transactionally restored
if attribution fails. The aggregate and sources are stamped per exact AC event
and owned only by the enclosing attack frame. No stat modifier or timed/round
buff is created, and duplicate callbacks cannot add either AC or source lines
twice.

## Rejected approaches

- `RuleDealDamage-only` cancellation/transfer: too late for poison, bleed,
  conditions, saving-throw riders, enchantment actions, and target subscribers.
- Shield Other's finalized-HP architecture: intentionally operates after
  recipient defenses and cannot represent attack-recipient substitution.
- `RuleAttackWithWeapon.ReplaceTarget` alone: redirects the damage event but
  not components reading the parent attack's `Target`.
- Cancelling and replaying or cloning the attack: rerolls result state and can
  duplicate animation, projectiles, ammunition, firearm discharge, misfire,
  grit, critical, precision, and on-hit processing.
- A real `UnitAttackOfOpportunity`: performs the forbidden attack and weapon
  pipeline instead of only spending AoO currency.
- A custom AoO or immediate-action counter: would diverge from Combat Reflexes
  and Kingmaker's shared swift cooldown.
- Center-to-center distance or weapon-name tables: loses corpulence, size,
  native reach, natural attacks, polymorph, unarmed-state, and LOS semantics.
- A full synthetic Aid `RuleAttackRoll`: exposes an unnecessary hostile attack
  event surface and requires broad suppression of firearm/on-hit systems when
  native attack-bonus plus native d20 rules provide the needed calculation.

## Harmony targets and fail-closed contracts

The implementation extends the repository's shared patches for:

- `RuleAttackRoll.OnTrigger(RulebookEventContext)` prefix/postfix
- `RuleCalculateAC.OnTrigger(RulebookEventContext)` postfix

It adds narrowly scoped patches for:

- `AbilityDeliveryTarget.set_AttackRoll(RuleAttackRoll)` postfix
- `AbilityExecutionProcess.ApplyEffect(AbilityExecutionContext,
  AbilityDeliveryTarget, AbilityApplyEffect, AbilitySelectTarget)` postfix
- `ElementsContextData.Dispose()` prefix, restricted at runtime to
  `ContextAttackData`, for ability-target restoration from native `finally`
- `RulebookEventContext.PopEvent(RulebookEvent)` prefix for rule-event target
  restoration after all `Did`/`OnDidTrigger` consumers
- `SceneEntitiesState.Dispose()` prefix for final scene-transition cleanup

No transpiler is selected. Consequently there is no IL rewrite assumption.
Reflection/assembly contract tests must validate every readonly target field,
method signature, event-parent relationship, and delivery setter. Any missing
contract disables the interception path and emits one diagnostic rather than
shipping a mechanically partial fallback.
