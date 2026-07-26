# ADR-0030: Apply misfire condition damage to the exact discharged item

- **Status:** Accepted for Sprint 24
- **Date:** 2026-07-16

## Context

Sprint 23 proved in live Kingmaker evidence that forced natural 1 and 2 are classified as firearm misfires and forced to miss, while natural 3 and 20 retain ordinary Kingmaker behavior. The existing discharge pipeline already consumes one loaded round from the exact marked firearm before natural-roll classification.

The next bounded rule is item condition damage:

```text
Normal -> Broken
Broken -> Wrecked
```

The project already has an immutable state machine and an accepted item-owned inert enchantment-token repository. Re-identifying the firearm by display name, blueprint category, slot, owner, or process hash would weaken isolation. Applying condition damage before the loaded-round commit would create ambiguous ordering and rollback behavior. Applying it on every possible `IsSuccessRoll` invocation could damage the item more than once.

Several formal Sprint 23 queue and persistence controls were not separately captured, but the user explicitly approved carrying them into the combined Sprint 24 smoke test. This is an evidence-policy exception, not a claim that those observations already occurred.

## Decision

Retain the exact runtime item object and repository identity in the short-lived `RuleAttackRoll` context created only after a verified `Fired` transition.

At the first exact `IsSuccessRoll(int)` evaluation:

1. evaluate the existing pure natural-roll decision;
2. force a detected misfire to miss;
3. derive one pure condition decision from the already-empty post-discharge state;
4. for a misfire only, commit the expected state through `FirearmRuntimeState.Service.Transition(exactItem, ...)`;
5. reject an intervening state mismatch;
6. verify the resulting state and repository identity; and
7. record the exact transition in diagnostics.

Only the first evaluation for one `RuleAttackRoll` may mutate condition. Duplicate evaluations retain miss enforcement but perform no second state mutation.

Use the existing token-backed repository. Do not revive the rejected `ItemEntityWeapon.UniqueId` vault or create a second persistence carrier.

## Rejected alternatives

### Apply damage before discharge

Rejected because the loaded-round transaction is already authoritative and runtime-proven. Condition damage must operate on the verified empty post-shot state so a Wrecked result cannot retain ammunition.

### Re-resolve by blueprint or Heavy Crossbow category

Rejected because the Test Musket intentionally borrows native Heavy Crossbow presentation. Category-based matching would risk damaging native weapons.

### Store only a weak display or slot identity in the attack context

Rejected because equipment and inventory position can change and are not item identity.

### Call the transition on every `IsSuccessRoll` callback

Rejected because Kingmaker may evaluate success more than once. The condition mutation requires an atomic per-attack gate.

### Add explosion or wielder damage now

Rejected as a separate rules and presentation concern. Sprint 24 is limited to item-owned condition state so persistence and exact-item ordering can be proven independently.

### Treat user-approved carry-forward as observed evidence

Rejected. Documentation must distinguish the observed forced-roll boundary from the unobserved controls that remain in the combined 0.0.24 gate.

## Consequences

- A Normal misfire produces an empty/Broken state on the exact firing item.
- A Broken misfire produces an empty/Wrecked state on the exact firing item.
- A native Heavy Crossbow remains outside the firearm repository and cannot receive condition damage.
- One attack-roll object cannot apply two condition transitions.
- State-token save durability remains the sole persistence mechanism under test.
- The 0.0.24 smoke guide must include both Sprint 24 transitions and all carried-forward Sprint 23 isolation/persistence controls.
- Sprint 25 remains blocked until this combined gate passes. Its prospective scope is the second-misfire consequence layer, beginning with explosion/damage behavior only after exact-item Wrecked ordering is proven.
