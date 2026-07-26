# Firearm item lifecycle and recovery contract

## Status

Sprint 27 establishes the exact Kingmaker 2.1.7b item-removal and replacement boundaries and introduces one development-only same-item recovery probe. It does not add player-facing repair gameplay.

The authoritative firearm state remains an inert item-owned `BlueprintWeaponEnchantment` token. The rejected `ItemEntityWeapon.UniqueId` vault remains prohibited.

## Exact native removal boundary

The demonstrated full-item detachment path is:

```text
ItemEntity.Collection.Remove(item)
  → ItemsCollection.Remove(item, item.Count)
  → ItemsCollection.Extract(item)
  → clear item.Collection
  → item.HoldingSlot?.RemoveItem(true)
  → update slot index
  → collection OnItemsRemoved bookkeeping
```

This path safely handles both collection ownership and an equipped slot. `ItemEntity.Dispose()` is not an alternative: its exact body only disposes the item's enchantment collection.

Sprint 27 records this contract for possible future destruction mechanics but does not call it automatically after a firearm explosion.

## Replacement boundary

Kingmaker's native blueprint-based creation paths call `ItemsEntityFactory.CreateEntity` and therefore return a new runtime `ItemEntity`:

```text
ItemsCollection.Add(BlueprintItem)
ItemSwitch.RunAction()
```

`ItemSwitch` may remove the replaced item after creating and equipping the new item. That workflow changes exact runtime identity and is unsuitable for preserving the existing per-item firearm token and lifecycle evidence.

## Repair-contract search

Exact metadata inspection found no installed item-condition `Repair`, `Mending`, `MakeWhole`, or `Make Whole` contract. `ItemRestoreValue` restores a missing count of a blueprint by adding new item entities; it does not repair an existing item and does not preserve exact runtime identity for a replacement.

Therefore Sprint 27 does not pretend that a native item-repair API exists.

## Gameplay decision

The mod retains an exploded nonmagical firearm as the exact empty/Wrecked item rather than removing it automatically.

Reasons:

- the current state architecture is explicitly per exact item;
- removal would irreversibly discard the token-backed state and block a recovery path;
- replacement would create a different runtime item;
- no native same-item repair contract exists; and
- keeping Wrecked is deterministic, inspectable, persistent, and fail-closed.

This is a deliberate mod gameplay model rather than a claim that the tabletop destruction sentence is being implemented literally.

## Same-item overhaul probe

Sprint 27 adds the pure transition:

```text
empty/Wrecked → empty/Broken
```

The development control requires the exact equipped firearm to be Wrecked. It then:

1. captures the repository identity, runtime reference hash, revision, and state;
2. commits `FirearmStateMachine.OverhaulWrecked` through the accepted item-token repository;
3. verifies repository identity did not change;
4. verifies the in-process runtime reference hash did not change;
5. verifies revision increased by exactly one; and
6. verifies the final state is empty/Broken.

The probe grants no ammunition, consumes no inventory resource, removes no item, creates no replacement, and does not silently complete ordinary Broken-to-Normal repair.

## Staged recovery model

The bounded candidate recovery model is intentionally two-stage:

```text
Wrecked --overhaul--> Broken --ordinary repair--> Normal
```

Sprint 27 qualifies only the exact-item overhaul boundary. A future sprint must separately choose and qualify player-facing delivery, cost, time, skill, and availability. The existing development-only ordinary repair command is not a release mechanic.

## Destructive diagnostic safety

The old one-click cleanup control could remove every unequipped Test Musket and was easy to confuse with nearby diagnostic buttons. Sprint 27 replaces it with an arm/confirm/cancel flow. Arming or cancelling performs no inventory or state mutation.

## Runtime acceptance and player-facing delivery

Sprint 27 proved the exact-item overhaul boundary in Kingmaker. Sprint 28 exposes that accepted transition through a full-round Overhaul Test Musket ability that consumes one Firearm Repair Kit only during completed delivery. The player-facing implementation and atomic rollback contract are documented in `FIREARM-PLAYER-FACING-OVERHAUL.md`.
