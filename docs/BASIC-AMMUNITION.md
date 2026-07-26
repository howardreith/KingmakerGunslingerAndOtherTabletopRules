# Basic ammunition subsystem

## Sprint 20 scope

Sprint 20 adds two inert, stackable inventory items and a verified inventory transaction:

- **Black Powder Charge**
- **Lead Ball**

One complete early-firearm load requires exactly one of each item. The transaction is deliberately separate from the loaded state on a particular firearm.

This sprint does **not** add a reload ability, does not place ammunition into a gun, does not prevent empty attacks, and does not consume ammunition during an attack. Those integrations remain later milestones.

## Blueprint construction

The two custom `BlueprintItem` instances use the GUIDs reserved before the persistence gate:

| Symbol | GUID | Display name |
|---|---|---|
| `KMG.Test.BlackPowderItem` | `ea966bf998a647cf97b0ed92f71c4b7d` | Black Powder Charge |
| `KMG.Test.LeadBulletItem` | `55c29771445947d685dba9e1ead46a42` | Lead Ball |

The stable symbol retains the earlier `LeadBullet` spelling; changing an existing symbol or GUID for cosmetic consistency would create unnecessary migration risk.

Both items are isolated clones of Kingmaker's mundane Diamond Dust `BlueprintItem` (`92752bbbf04dfa1439af186f48aee0e9`). Cloning supplies known stack/inventory presentation infrastructure while the custom clones replace their name, description, flavor, cost, weight, stackability, miscellaneous classification, and component array. The source blueprint is snapshotted before cloning and verified afterward to ensure it was not mutated.

Current provisional values are:

| Item | Cost | Weight |
|---|---:|---:|
| Black Powder Charge | 10 gp | 0.1 lb |
| Lead Ball | 1 gp | 0.1 lb |

The inherited Diamond Dust icon and inventory sounds are placeholders. The custom items carry no gameplay components.

## Localization

Each item uses stable custom localization keys. Registration is repeated during blueprint bootstrap because Kingmaker recreates its active localization pack during process startup. Identical existing values are accepted; a conflicting value under one of the mod's keys fails blueprint initialization rather than replacing another value silently.

## Inventory boundary

`IBasicAmmunitionInventory` is the engine-independent boundary:

```text
Count(component)
Add(component, amount)
Remove(component, amount)
```

`KingmakerBasicAmmunitionInventory` adapts that boundary to the shared `ItemsCollection` using Kingmaker's typed `Count`, `Add`, and `Remove` methods. It does not enumerate UI slots or infer quantities from stack objects.

## Atomic consumption contract

`BasicAmmunitionTransactionService.TryConsumeOneLoad` follows this sequence:

1. Capture exact powder and ball counts.
2. Reject without any write unless both counts are at least one.
3. Remove one Black Powder Charge.
4. Remove one Lead Ball.
5. Re-read both counts and require an exact decrement of one each.
6. On any mutation or verification failure, restore both components to the exact pre-transaction counts.
7. Verify rollback and report rollback failure separately from the original mutation failure.

A successful result therefore means exactly one pair was consumed. An insufficient-components result means no inventory method capable of mutation was called. A failed mutation is never reported as success.

This provides application-level transaction semantics over an inventory API that does not expose a native multi-item database transaction. The rollback is best-effort under catastrophic engine failure, and the resulting exception preserves both the mutation and rollback exceptions for diagnosis.

## Separation from loaded firearm state

Inventory components and gun state are intentionally different concepts:

```text
Shared inventory
    Black Powder Charge stack
    Lead Ball stack

Exact Test Musket item
    loaded rounds
    loaded ammunition identity
    condition
```

A future reload action will consume one inventory pair only after all prerequisites pass, then commit `Loaded / Normal` or `Loaded / Broken` state to the exact firearm through the already proven item-owned token carrier. Firing will consume the loaded state, not the shared inventory pair again.

## Runtime controls

Version 0.0.20 exposes disposable-save controls to:

- add 20 of each item;
- add one powder charge;
- add one lead ball;
- print current counts;
- consume one pair atomically;
- remove all basic ammunition.

These controls are diagnostic. They do not reload a gun.

## Runtime acceptance

The smoke test must establish that:

- both items register without breaking the existing ten-blueprint transaction;
- their localized names and descriptions appear;
- twenty identical copies merge into ordinary stackable inventory quantities;
- exact count diagnostics agree with the inventory UI;
- one successful transaction subtracts one from each stack;
- missing powder or missing lead ball consumes neither item;
- stacks survive save, exit to desktop, restart, and reload;
- the Sprint 19 A-D firearm-state fixture remains intact;
- native Diamond Dust remains unchanged.
