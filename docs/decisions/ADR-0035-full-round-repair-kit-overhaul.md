# ADR-0035 — Full-round repair-kit delivery for same-item overhaul

## Status

Accepted for Sprint 28 runtime qualification.

## Context

Sprint 27 proved that a Wrecked firearm can remain the same exact runtime item while its item-owned state token changes to empty/Broken. That probe intentionally had no player-facing timing or cost.

A release mechanic needs an unambiguous item target, action cost, inventory cost, interruption boundary, and rollback policy without introducing item replacement or relying on a nonexistent native repair API.

## Decision

Use a personal extraordinary full-round ability granted by Firearm Proficiency.

The ability targets exactly one equipped Wrecked Test Musket and consumes one stackable Firearm Repair Kit only when delivery completes. Delivery invokes an atomic transaction over the exact item-owned state and the shared repair-kit count.

Completed delivery performs only:

```text
empty/Wrecked + one repair kit → empty/Broken
```

It does not perform Broken-to-Normal repair.

## Rationale

- Full-round `AbilityCustomLogic.Deliver` is already runtime-proven by Reload Test Musket.
- Equipment-based selection is deterministic and fails closed when more than one distinct matching item is equipped.
- A dedicated inert stackable item gives a visible, auditable cost without a new crafting subsystem.
- Mutation at delivery makes pre-delivery cancellation free of state/resource writes.
- Expected-current state replacement and exact count verification protect against stale or partial writes.
- Keeping the same item preserves the accepted token carrier and persistence model.

## Consequences

- Existing characters with Firearm Proficiency must receive both Reload and Overhaul through one `AddFacts` component with missing-fact restoration enabled.
- A Wrecked firearm cannot be overhauled without a repair kit.
- A Normal or Broken firearm cannot consume a kit through this ability.
- A successful overhaul still leaves the firearm Broken and empty; reload and ordinary repair remain separate.
- The current repair kit is development-accessible rather than economically distributed. Vendor, loot, and crafting integration are deferred.

## Rejected alternatives

### Automatic post-explosion recovery

Rejected because it removes meaningful lifecycle cost and bypasses player action.

### Replace the Wrecked item with a new Broken item

Rejected because native blueprint creation changes runtime identity and would abandon the exact item-owned state history.

### Call `ItemEntity.Dispose`

Rejected because exact contract inspection proved it disposes enchantments rather than safely removing or repairing an item.

### Consume powder or Lead Ball as the repair cost

Rejected because ammunition and mechanical repair are separate resources and conflating them makes transaction evidence ambiguous.

### Immediate UI button as the release mechanic

Rejected. The immediate command remains a diagnostic only and bypasses action economy.
