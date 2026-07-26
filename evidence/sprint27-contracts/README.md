# Sprint 27 exact item-lifecycle contracts

Inspected input: exact private Kingmaker 2.1.7b `Assembly-CSharp.dll`.

Assembly SHA-256:

```text
3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb
```

## Findings

### Collection removal is the native detachment boundary

`ItemsCollection.Remove(ItemEntity)` delegates to the count-aware overload. Full-item removal calls `Extract`, which removes the exact object from the collection, clears its `Collection`, removes it from an equipped `HoldingSlot` through `ItemSlot.RemoveItem(true)`, updates slot indexing, and then raises collection-removal bookkeeping.

### `ItemEntity.Dispose()` is not destruction/removal

The exact method only disposes the item's enchantment collection. Calling it as an inventory-destruction substitute would leave collection and equipment ownership outside the demonstrated removal path.

### Replacement creates a new runtime item

`ItemsCollection.Add(BlueprintItem)` calls `ItemsEntityFactory.CreateEntity`. Native `ItemSwitch.RunAction()` also creates a new item, adds it, swaps equipment, and optionally removes the old item. This is a replacement flow, not exact-item condition recovery.

### No native item-condition repair contract was found

Name inspection found no `Repair`, `MakeWhole`, `Make Whole`, or `Mending` type/method contract. `ItemRestoreValue.RunAction()` compares blueprint counts and adds missing blueprint items; it does not mutate an existing item's condition and does not preserve exact runtime identity for a replacement.

## Sprint 27 decision

Automatic removal is technically possible through `item.Collection.Remove(item)`, but it is deliberately not used for firearm explosion consequences. The mod retains the exact item as empty/Wrecked and qualifies a development-only, same-item token transition to empty/Broken. Player-facing cost, delivery, timing, and ordinary Broken-to-Normal repair remain separate future work.
