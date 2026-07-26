# ADR-0035: Deliver Wrecked-to-Broken recovery as a full-round same-item overhaul

## Status

Accepted for Sprint 28 runtime qualification.

## Context

Sprint 27 established that Kingmaker has no native same-item firearm repair contract suitable for this mod. It also proved that an exact Wrecked Test Musket can transition to empty/Broken while retaining repository identity, runtime reference, and independent state from a second blueprint-identical item.

A player-facing delivery still needed explicit target selection, timing, cost, interruption behavior, and transaction safety.

## Decision

Expose `Overhaul Test Musket` as a personal extraordinary full-round ability granted by Firearm Proficiency.

The ability targets exactly one equipped Test Musket and fails closed if zero or multiple distinct Test Muskets are equipped. It is available only when the exact item is Wrecked and shared inventory contains a Firearm Repair Kit.

On completed delivery, consume exactly one repair kit and transition the same exact item from empty/Wrecked to empty/Broken through one verified cross-resource transaction. Perform no item removal or replacement. Do not create or consume ammunition. Keep ordinary Broken-to-Normal repair separate.

## Consequences

Positive consequences:

- the player receives a real action rather than a development-only state button;
- cancellation before delivery is naturally non-mutating;
- the accepted item-owned state carrier is preserved;
- exact-item ambiguity fails closed;
- state and resource writes are verified and rolled back together when possible; and
- the boundary can later be generalized beyond the Test Musket.

Costs and limitations:

- this first delivery is Test-Musket-specific;
- the repair kit uses placeholder presentation and development distribution;
- no ordinary repair action is included;
- live Kingmaker testing must still prove interruption timing and action-bar availability; and
- a future generic firearm-action layer should replace blueprint-specific duplication.

## Rejected alternatives

### Replace the Wrecked item

Rejected because replacement creates a new runtime item and discards exact-item continuity.

### Delete the firearm and grant a repaired copy

Rejected because it is destructive, changes identity, complicates rollback, and contradicts the accepted recoverable-Wrecked model.

### Consume the kit during availability or command selection

Rejected because cancelled or interrupted commands would lose resources before gameplay delivery.

### Repair directly to Normal

Rejected because Wrecked-to-Broken overhaul and Broken-to-Normal ordinary repair are deliberately separate recovery stages.

### Use `ItemEntityWeapon.UniqueId` as a recovery key

Rejected because the runtime contract previously failed and the item-owned enchantment token is authoritative.
