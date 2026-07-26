# Loaded-round attack enforcement, token durability, and 0.0.22.1 hook repair

## Scope

Sprint 22 defines the first complete basic-firearm loop for the Test Musket. Version 0.0.22.1 repairs the exact runtime hooks needed to execute it:

1. carry one Black Powder Charge and one Lead Ball;
2. spend a full-round action to reload;
3. store one loaded round on the exact equipped firearm;
4. preserve that loaded state through Kingmaker's native item refresh; and
5. consume the loaded round when the firearm's attack roll begins.

The implementation deliberately retains Kingmaker's ordinary weapon-attack pipeline. It does not replace the shot with a spell or custom damage ability.

## Why quicksave unloaded the Sprint 21 musket

Kingmaker periodically calls `ItemEntity.ApplyEnchantments()` to reconcile a runtime item's enchantments with its blueprint. The installed 2.1.7b method removes a runtime enchantment when:

- its `ParentContext` is null; and
- its blueprint is not one of the item's native blueprint enchantments.

The Sprint 21 state token was a dynamic enchantment with a null parent context. On an equipped item, a quicksave-related refresh could therefore remove the token. The firearm then decoded as canonical empty/Normal state, so the reload ability became available again.

## Sprint 22 durability strategy

New state tokens use `ItemEntity.AddEnchantment(...)` with a parent `MechanicsContext` whenever the item exposes a current wielder or owner. This makes the token an intentional dynamic enchantment rather than an apparently redundant null-context fact.

A Harmony prefix/postfix also surrounds the exact zero-argument `ItemEntity.ApplyEnchantments()` method. Only `ItemEntityWeapon` instances enter firearm-state inspection:

- the prefix records only the known firearm-state token IDs present on the exact item;
- native reconciliation runs normally;
- the postfix compares the token set;
- if exactly one known token existed before and none exists afterward, it restores that exact token and verifies it;
- any changed, duplicate, foreign, or otherwise ambiguous token set is reported as a conflict and is not guessed or normalized.

This fallback is primarily for tokens created by Sprint 21, which may still have a null parent context. The first native refresh may remove and restore such a token; the restored token receives a context when an owner is available.

## Version 0.0.22.1 repair

The installed Kingmaker 2.1.7b rule-event callbacks are `void OnTrigger(RulebookEventContext)`, not zero-argument methods. The repair resolves exactly that signature for `RuleAttackWithWeapon`, `RuleAttackRoll`, and `RuleCalculateAC`. It also rejects non-weapon instances in the `ItemEntity.ApplyEnchantments()` prefix before any firearm-state token inspection, preventing the observed `ItemEntityShield` faults.

## Version 0.0.22 runtime failure and bounded repair

The supplied 0.0.22 result established that quicksave no longer unloaded the Test Musket, but a native attack left it at one loaded round and the attack counters remained at zero. Exact private-assembly inspection showed that all three intended rule-event targets take one `RulebookEventContext`. The same runtime evidence showed `ItemEntityShield` faults because the base `ApplyEnchantments()` prefix inspected non-weapons.

Version 0.0.22.1 changes only those boundaries: exact one-argument rule-event target selection and weapon-only reconciliation inspection. The state machine, item-token carrier, reload transaction, firearm marker, duplicate-event gate, and native Heavy Crossbow exclusion remain unchanged. Sprint 23 misfire work remains blocked pending live acceptance.

## Attack-time discharge

The Harmony patch for the exact installed `RuleAttackRoll.OnTrigger(RulebookEventContext)` callback invokes loaded-round enforcement before Kingmaker calculates the attack result. Version 0.0.22 incorrectly assumed a zero-argument callback and therefore never attached; version 0.0.22.1 repairs that target contract.

Only an exact runtime weapon whose exact weapon type contains exactly one `FirearmDefinitionComponent` is eligible. The borrowed Heavy Crossbow category, icon, model, or name is insufficient. Native Heavy Crossbows are therefore ignored.

For an exact firearm:

| Firearm state | Attack result | State result |
|---|---|---|
| Loaded / Normal | Native attack proceeds | One round consumed |
| Loaded / Broken | Native attack proceeds | One round consumed; Broken retained |
| Empty / Normal | Forced miss | State unchanged |
| Empty / Broken | Forced miss | State unchanged |
| Wrecked | Forced miss | State unchanged |
| State read/write fault | Forced miss | Fault logged |

When forcing a miss, Sprint 22 clears `AutoHit` and sets `AutoMiss`. This prevents an auto-hit effect from bypassing an empty chamber.

A weak reference-identity gate stamps each `RuleAttackRoll` object. If the same Harmony callback is observed twice for the same event instance, the second callback cannot consume another round.

## Inventory boundary

Firing never consumes Black Powder Charges or Lead Balls from shared inventory. Those components were already consumed when reload completed. Attack-time discharge only transitions the item-owned state from loaded to empty.

## Diagnostics

The Unity Mod Manager panel reports two new process-local diagnostic groups.

`Firearm attack enforcement` includes:

- observed attack rolls;
- fired rounds;
- empty rejections;
- wrecked rejections;
- ignored non-firearms;
- duplicate callbacks; and
- faults.

`State-token native reconciliation` includes:

- reconciliation calls;
- calls that observed a state token;
- tokens preserved natively;
- tokens restored after native removal;
- conflicts; and
- faults.

These counters are evidence only. They do not own or reconstruct firearm state.

## Explicitly deferred

Sprint 22 does not add:

- misfire rolls;
- Broken or Wrecked transitions caused by attacks;
- explosions;
- automatic reloads between iterative attacks;
- Rapid Reload;
- attacks of opportunity from reload;
- firearm-specific combat-log localization;
- custom models, sound, projectile, or animation;
- vendors, crafting, or the Gunslinger class.
