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

## 0.0.94 human-regression investigation

Date: 2026-08-23

### Source and preserved intake

This investigation continued on `codex/bodyguard-in-harms-way` from the clean,
remote-equal commit
`2683b06fedf1eac4cd4fdb97ad6be14fb9c04698`. The pre-change Release suite
passed 1,211 tests.

The active 0.0.93 human-session log was copied before another Kingmaker launch:

- source evidence directory:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260823T1250159827166Z-human-in-harms-way-regression`
- copied file: `output_log-human-0.0.93.txt`
- size: 676,357 bytes
- modification time: `2026-08-23T12:31:37.6321241Z`
- SHA-256:
  `855CE57C81B5D5A07A1CFAB928C79BE8CED8AEADA54EAC357DDB722FE4A0AEE6`

The relevant confirmed-critical frame is `bodyguard-attack-8`:

- attacker: `007a489e-d797-4555-ab6c-0c27cd6431ee/Kobold`
- original target:
  `533a5084-8aa1-4aa0-a8f6-b8eac959368f/VictimTest`
- protector:
  `5b6aa62a-e6fb-42c3-ba78-9cd3549505c1/HelpfulDefenderTest`
- Aid Another: d20 8 + 9 = 17, success
- canonical grant: halfling Helpful, base 2 + Helpful increment 2 = +4
- AC: native 13, Bodyguard source +4, final 17
- incoming attack: natural 20 + 2 against 17, hit
- terminal KMG record: `stage=immediate-unavailable`, empty arbitration,
  `swiftBefore=NaN`, `swiftAfter=NaN`, no interceptor

This proves that the observed damage was not caused by a committed redirection
being restored too early. The 0.0.93 code rejected the only successful
Bodyguard user before arbitration and never mutated either target. It does not
prove which input caused that rejection: the old implementation collapsed a
missing feat, missing marker, mode off, or unavailable native swift action into
the same empty eligible set and terminal label.

The exact human save was protected as
`Quick_3_HelpfulDefenderTest.protected-intake.zks` with SHA-256
`3414D67CB2E5F8C4F18A952D23247DC6DD9D9F5579066EA64CA7FF29E61B8F01`.
Offline archive inspection found the exact Bodyguard and In Harm's Way facts,
both activatables enabled/running, and both hidden marker identities in the
serialized unit. That establishes saved state, not the temporal live state at
the failing attack. Swift cooldown is not serialized in a form that can prove
the attack-time native budget.

The stable project identities involved are:

- Bodyguard feat: `b2baa3384b4d4328848cc07933b513be`
- Use Bodyguard: `ac31a9d5d34140978b7e778dc8d1e226`
- Bodyguard marker: `a78147a3655f429883ad88e761ff9438`
- In Harm's Way feat: `e481f30c8b6940e1b596e121443aa01e`
- Use In Harm's Way: `ca1e74f0e60747209a8b7cf3737243ea`
- In Harm's Way marker: `57603d0b215e4ac6862bcdf9b5583568`

### Historical and source bisection

Comparisons across the immutable milestones showed:

- 0.0.90 / `1be221ff`: original Bodyguard/In Harm's Way production runtime
- 0.0.91 / `f3608b12`: AC-breakdown attribution only
- 0.0.92 / `a736e25e`: variable canonical Aid Another grants only
- 0.0.93 / `2683b06f`: late Eastern/Favored publication only

The weapon target-redirection and immediate-action implementation did not
change across those milestones. No source milestone can truthfully be named as
the first delivery regression. The human failure exercised a pre-existing
eligibility-observability gap that the earlier fixture had hidden, while the
AC and compatibility changes merely made that older edge visible in a new
human scenario.

### Previous fixture discrepancy

The existing fixture did execute a real `RuleAttackWithWeapon`, real damage,
real rider delivery, and asserted both units' HP. Its delivery evidence was not
synthetic. However, it directly granted the hidden mode marker, set the shared
swift cooldown to zero, and used a broad critical-result control. It therefore
proved the delivery seam only after manufacturing every eligibility gate and
could pass without proving the player-facing activatable/marker lifecycle or
an ordinary native confirmation roll.

The repaired fixture now:

- grants the real feats and switches the real activatables on;
- verifies exact activatable `IsOn`/`IsRunning` and marker agreement;
- observes every pre-filter gate and the native swift cooldown;
- drives a main natural 20 and a separate native confirmation d20 through a
  request-local, exact-`RuleAttackRoll` dice hook;
- observes the actual `RuleAttackWithWeaponResolve` and `RuleDealDamage`
  recipients, both units' HP, native riders, completion count, and restoration;
- contains explicit mode-off and positive-swift-cooldown negative controls.

### Confirmed-critical event contract

Exact installed IL shows that `RuleAttackRoll.OnTrigger` performs the main AC
calculation and d20, then performs the critical AC calculation and confirmation
d20 inside the same `RuleAttackRoll`. Confirmation is not a nested hostile
`RuleAttackRoll`; it must not push another Bodyguard frame or spend another
AoO. The shared postfix runs only after the main hit, concealment, critical
threat, and confirmation state are final, and before the enclosing
`RuleAttackWithWeapon` constructs its damage bundle.

Runtime evidence confirmed this order: one frame and one Bodyguard attempt,
two AC observations on the same roll, one confirmation consumption, one
immediate-action spend, then one native critical delivery. The roll target is
restored at roll pop while the enclosing weapon target remains redirected
through damage and attack-linked `Did` consumers; the weapon target restores
at weapon completion. No confirmation child frame exists to pop or corrupt the
parent.

### Gate repair and player-state synchronization

`InHarmsWayCandidateGate` now receives a complete immutable snapshot before
filtering. Its stable outcomes distinguish module state, hit state, Bodyguard
attempt/success, exact feat ownership, activatable ownership and `IsOn`, marker
presence, activatable/marker divergence, alive/conscious/CanAct, already-used
interception, delivery contract, native `SwiftAction` cooldown,
`HasSwiftAction()`, and policy/redirection failures. Runtime diagnostics also
record turn owner, round, acted state where exposed, party order, all exact
blueprint GUIDs, and target/delivery observations.

The native action contract remains authoritative. In 2.1.7b,
`HasSwiftAction()` is exactly the predicate
`CombatState.Cooldown.SwiftAction <= 0`; successful immediate use adds the
native six-second quantum and verifies the budget became unavailable. A
positive cooldown yields `swift-cooldown-active`; a false native predicate at
zero yields `has-swift-action-false`. No custom round counter was added.

Both reaction activatables now set `DeactivateImmediately = true`. Exact
activatable IL showed that leaving this false can make `IsOn` false while
`IsRunning` and the marker persist until a later turn boundary. Immediate
deactivation keeps player consent and the hidden marker synchronized in RTWP
and turn-based play. It does not change free activation, off-by-default state,
cross-combat persistence, or save serialization. Feat ownership never
substitutes for mode consent.

If an otherwise qualified, mode-on protector has no native immediate action,
one concise combat-log message now explains that fact. Mode-off attacks remain
silent.

### Runtime qualification

All results below used Steam App ID 640820 and the guarded request mechanism.
The source-tested artifact at commit `464ffbe302f348e5b1d2de238bef08fae2d93144`
had package SHA-256
`0623230F8DA32B9BE21B8EF1A7E11BE709EEBF017FD222F64BCC993B758A1565`,
DLL SHA-256
`14CA336A522BA28257564917C8BF23ECA0B3BCECEABB8268FEC17981E799AA42`,
and MVID `730588cc-2b88-4dab-9956-6a3c8f0752c3`.

Core real-delivery runs:

- `disposable-helpful-bodyguard`, run
  `20260823T1416222099584Z-f4cdc36db5d14a348240b78d6756cc34`, PASS,
  evidence directory
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260823T1416221807923Z-disposable-helpful-bodyguard`.
  Halfling Helpful supplied +4. The ordinary hit moved 11 HP damage to the
  protector. The confirmed critical preserved main d20 20 and confirmation
  20/28, moved 24 HP damage to the protector once, left victim HP unchanged,
  advanced swift 0 to 6 once, consumed one AoO, and reported zero faults and
  duplicates.
- `disposable-bodyguard-feats`, run
  `20260823T1419130297700Z-28227dbdfd944f9f993dafe9f42bcd74`, PASS,
  evidence directory
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260823T1419130035149Z-disposable-bodyguard-feats`.
  A native confirmed critical preserved main d20 20 and confirmation 20/23,
  redirected 24 HP plus physical, fire, save, and condition delivery once,
  and restored both targets. The unavailable-action control retained swift 6,
  damaged only the original target, recorded `swift-cooldown-active`, and
  emitted the explanatory message. Mode-off damaged only the original target,
  spent no swift action, recorded `in-harms-way-mode-off`, and emitted no
  unavailable-action message. Zero-damage riders and both Shield Other orderings
  passed.
- `observe-bodyguard-native-contracts`, run
  `20260823T1421551498137Z-0468330966c04fdca58c287bc53f95dc`, PASS.

Compatibility profiles all restored their prior Mods/settings state exactly:

- standalone transaction `compat-20260823T142618Z-a6ab27db75d3`, PASS
- Call of the Wild transaction `compat-20260823T142756Z-57085ab24e38`, PASS
- CotW + Favored Class, traits enabled:
  `compat-20260823T143019Z-d049108650f6`, observer and combat PASS
- CotW + Favored Class, traits disabled:
  `compat-20260823T143433Z-638feca41a71`, observer and combat PASS
- CotW + Favored Class + Tweak or Treat + Races Unleashed:
  `compat-20260823T143857Z-8f350a96bc95`, observer and combat PASS
- Eastern Weapons disabled with Favored Class:
  `compat-20260823T144344Z-0e0f05f14ff4`, module observer and combat PASS
- Bodyguard module disabled with Favored Class:
  `compat-20260823T144833Z-7dc439bbf710`, module observer and inert combat PASS

The canonical `working-save-smoke` run
`20260823T1454518705298Z-be7c00b628624552949ad9b7c4ef8551` passed at
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260823T1454518578721Z-working-save-smoke`.
No save-writing API was observed and `KMG_AUTOMATION_BASELINE` was not used as
the working save.

The protected human-save copy was attempted twice, most recently as run
`20260823T1459343495247Z-778bafaa29ea4afbaa5b1c7661092801` at
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260823T1459343282896Z-disposable-in-harms-way-human-repro`.
It found exactly one named save among 109 descriptors, correlated the exact
descriptor through the native load entry, and observed the after-load callback,
but timed out at `post-load-fingerprint` before any attack scenario ran. It
observed no save-writing API. The wrapper removed its named copy and sidecars
and reverified the original SHA-256 unchanged. This is an exact-save runtime
limitation, not positive or negative In Harm's Way mechanics evidence.

### Honest conclusion

The original 0.0.93 damage recipient is fully explained at the event level:
In Harm's Way never passed candidate filtering, spent no immediate action, and
never redirected the attack. The legacy evidence cannot honestly distinguish
whether the attack-time blocker was feat/mode/marker state or native swift
availability. The exact save cannot currently reach a stable post-load
fingerprint to recover that temporal fact.

The repaired player path no longer hides that ambiguity: every gate has one
exact reason, activatable and marker opt-out state cannot lag, and a genuinely
available +4 Helpful confirmed-critical attack is proven to deliver all damage
and riders to the protector once. A genuine unavailable native immediate
action remains a non-interception, now with an explicit player explanation.
