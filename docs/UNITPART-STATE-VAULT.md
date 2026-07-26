# Save-owned UnitPart firearm-state vault

## Purpose

Sprint 13 needs a carrier capable of representing arbitrary future firearm states while preserving exact per-item identity through Kingmaker saves. The candidate is a custom `UnitPart` attached to the main-character entity and serialized as part of the player's save graph.

The main character is not the firearm-state owner. It is only the stable save host. Every vault record contains a **direct ItemEntityWeapon reference** to the exact firearm.

## Data shape

```text
UnitPartFirearmStateVault
  records[]
    recordSchemaVersion: 1
    item: direct ItemEntityWeapon reference
    state:
      schemaVersion
      loadedRounds
      loadedAmmunitionId
      condition
```

`FirearmState` remains immutable. `FirearmStateData` is the primitive serializer DTO. The vault always clones DTOs on read and write so callers cannot mutate persisted data by retaining a reference.

## Identity contract

The only durable key is the direct item reference stored by the save serializer.

The vault does not use:

- weapon blueprint GUID;
- weapon-type GUID;
- display name;
- equipment slot;
- wielder identity;
- inventory index;
- `GetHashCode()`;
- an observed runtime ID;
- a generated external ID.

All record lookups compare with `ReferenceEquals`. Two item objects that compare equal by value still receive independent records.

## Canonical state

An exact firearm with no vault record is interpreted as:

```text
schemaVersion:       1
loadedRounds:        0
loadedAmmunitionId:  none
condition:           Normal
```

The repository removes a record when a gun returns to this canonical state. This keeps ordinary unmodified firearms out of the save payload.

## Replacement transaction

The store contract uses expected-current replacement:

```text
Replace(item, expectedData, targetData)
```

- `expectedData = null` requires no existing record.
- `targetData = null` removes a matching record.
- a mismatch fails without accepting the caller's write;
- the in-memory vault snapshots its record list before mutation;
- the target is read back and compared after mutation;
- any failure restores the pre-operation record list.

The repository performs a second decode-and-compare verification after the store returns.

## Null and duplicate handling

A deserialized record with a null item reference is pruned. This handles deletion or a serializer that deliberately resolves a dead object reference to null.

Two records referring to the same exact item are treated as corruption. The vault fails closed rather than selecting one record.

Null or malformed state data is also rejected. The repository's strict codec is the final authority for schema, ammunition, capacity, and condition validation.

## Legacy migration

The four stable Sprint 12 item-enchantment tokens remain registered so an older save can resolve them. They are a read-only legacy source for normal runtime operations.

Migration is ordered:

1. Read and strictly decode the exact item's legacy token set.
2. Read the exact item's vault state.
3. If no vault state exists, write and verify the decoded state.
4. Clear the token set.
5. Re-read and verify that no legacy token remains.
6. If cleanup fails after a new write, attempt to remove the vault record.

A vault/token conflict preserves both carriers and raises a diagnostic failure. Unknown or duplicate tokens are likewise preserved.

## Save-growth tradeoff

A direct vault reference may keep a sold or otherwise unreachable firearm in the save graph. That behavior is intentional for the persistence experiment because sale and repurchase must not lose state. The lifecycle matrix measures whether this produces unacceptable growth or prevents item deletion.

A future accepted implementation may add conservative pruning only when Kingmaker provides reliable evidence that an item is permanently destroyed. It must not prune merely because the item is outside party inventory.

## Unproven assumptions

The following remain unproven until a compiled build is run:

- custom UnitParts are serialized from a mod assembly in this Kingmaker build;
- Json.NET restores the concrete UnitPart type after restart;
- a direct item reference is restored to the reconstructed inventory or vendor item rather than to a duplicate object;
- merchant inventory remains in the same object graph across save/load;
- respec and companion/main-character reconstruction retain the vault;
- uninstalling the mod leaves saves recoverable.

The carrier is therefore **provisional and unproven**, and the persistence decision remains NO-GO.
