# ADR-0027: Enforce loaded rounds in RuleAttackRoll and guard native token reconciliation

## Status

Accepted for the Sprint 22 runtime smoke test.

## Context

Sprint 21 proved that a full-round ability could consume real inventory components and write a loaded-state enchantment to the exact equipped Test Musket. Runtime testing then showed that quicksave made the gun appear empty again.

Inspection of the exact Kingmaker 2.1.7b `ItemEntity.ApplyEnchantments()` IL showed that native reconciliation removes dynamic enchantments whose `ParentContext` is null and whose blueprints are not part of the item's built-in enchantment list.

The project also needed to connect loaded state to ordinary weapon attacks without replacing Kingmaker's attack, critical, feat, concealment, and damage pipeline.

## Decision

1. New item-owned state tokens receive a parent `MechanicsContext` when the item exposes a wielder or owner.
2. A narrow Harmony prefix/postfix guards the exact zero-argument `ItemEntity.ApplyEnchantments()` method.
3. The guard restores only the unambiguous case of one known token before and no token afterward.
4. Loaded-round enforcement runs at the beginning of the exact `RuleAttackRoll` for a marked firearm.
5. A loaded firearm commits the canonical `FirearmStateMachine.Fire` transition.
6. Empty, Wrecked, or untrustworthy marked firearms are forced to miss by clearing `AutoHit` and setting `AutoMiss`.
7. A weak reference-identity gate prevents duplicate callback consumption.
8. Native Heavy Crossbows are identified as non-firearms and are left unchanged.
9. Firing does not touch inventory ammunition because components were consumed during reload.

## Consequences

The Test Musket now participates in Kingmaker's ordinary attack pipeline while observing its own chamber state. Older null-context state tokens can self-heal on the first native reconciliation pass.

The Harmony postfix is intentionally conservative. A duplicate, foreign, changed, or ambiguous token set remains visible as a conflict rather than being silently repaired.

A parent context can retain an owner association on an inert token. Broader transfer, merchant, deletion, and save-growth behavior remains part of later lifecycle qualification.

## Rejected alternatives

### Character buff for loaded state

Rejected because two identical guns could not retain independent state.

### Spell-like “Fire Gun” ability

Rejected because it would bypass or duplicate normal weapon-attack behavior and feat integration.

### Ignore quicksave state loss

Rejected because it destroys ammunition economy and makes saved combat state untrustworthy.

### Re-add every missing token without a before snapshot

Rejected because it could hide corruption, copy state to the wrong item, or invent state after an unrelated mod changed enchantments.

### Identify guns by Heavy Crossbow category

Rejected because native Heavy Crossbows must remain unaffected.
