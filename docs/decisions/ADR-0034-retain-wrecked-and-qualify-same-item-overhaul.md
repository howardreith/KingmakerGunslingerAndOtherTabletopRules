# ADR-0034: Retain Wrecked firearms and qualify a same-item overhaul transition

- Status: Accepted for Sprint 27 runtime qualification
- Date: 2026-07-16

## Context

The second early-firearm misfire consequence now leaves the exact firearm empty/Wrecked after resolving its native five-foot burst. Sprint 27 needed to determine whether the remaining item consequence should remove the item, replace it, or retain it for recovery.

Exact Kingmaker 2.1.7b inspection established:

- `ItemsCollection.Remove(ItemEntity)` is the collection/equipment detachment path;
- `ItemEntity.Dispose()` only disposes enchantments and is not item removal;
- blueprint add and `ItemSwitch` replacement create a new `ItemEntity`; and
- no installed same-item repair, mending, or make-whole contract was found.

The current firearm state is an inert enchantment token owned by the exact item. Automatic removal discards that exact state; replacement changes runtime identity.

## Decision

Do not automatically remove or replace an exploded nonmagical firearm.

Retain the exact item as empty/Wrecked and qualify one development-only same-item overhaul transition:

```text
empty/Wrecked → empty/Broken
```

The runtime probe must preserve in-process repository identity and runtime reference hash, advance the repository revision exactly once, manufacture no ammunition, and stop before ordinary Broken-to-Normal repair.

Keep player-facing recovery delivery, cost, time, skill, and availability out of Sprint 27.

## Consequences

### Positive

- Exact-item token state and persistence remain intact.
- The recovery boundary can be tested independently of UI, resource, and class systems.
- The transition is deterministic and reversible through an explicit later ordinary repair step.
- Native item removal remains documented for any future feature that truly needs detachment.
- No guessed native repair API or replacement identity is introduced.

### Negative

- The mod deliberately differs from literal automatic destruction of a nonmagical firearm.
- Wrecked items can remain in inventory until a player-facing recovery or disposal workflow is implemented.
- Sprint 27 does not provide usable gameplay repair.

### Risks

- A future recovery ability could accidentally target the wrong blueprint-identical item. Runtime qualification therefore requires exact repository and reference identity evidence plus a second-item negative control.
- The destructive development cleanup control can still remove Test Muskets when explicitly confirmed. Sprint 27 mitigates accidental activation with a two-step confirmation rather than deleting the diagnostic entirely.

## Rejected alternatives

### Automatically call `ItemsCollection.Remove`

The removal contract is clear, but automatic removal would erase exact-item state and close the recovery path before player-facing design is qualified.

### Call `ItemEntity.Dispose`

Rejected because the exact implementation only disposes enchantments and does not safely detach collection or equipment ownership.

### Replace the Wrecked firearm with a new blueprint item

Rejected because native creation returns a new runtime item and therefore changes exact identity.

### Treat `ItemRestoreValue` as repair

Rejected because it restores blueprint counts by adding items; it does not mutate existing item condition.

### Silently change Wrecked directly to Normal

Rejected because it collapses overhaul and ordinary repair into one uncosted transition and makes later gameplay balancing impossible.
