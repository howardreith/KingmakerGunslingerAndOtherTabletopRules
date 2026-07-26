# Sprint 14 completion report — engine-issued item-identity vault

```text
Milestone:                 0.0.14-s14-item-identity-vault
Source implementation:     COMPLETE
Portable validation:       PASSED
Compiled Kingmaker build:  NOT PRODUCED
UMM install package:       NOT PRODUCED
Persistence gate decision: NO-GO
Ammunition/reload work:    BLOCKED
```

## Why Sprint 14 remained a persistence sprint

Sprint 13 introduced a save-owned custom `UnitPart`, but each record still held a direct `ItemEntityWeapon` reference. Without a compiled lifecycle run, the project could not prove that Kingmaker would restore that direct reference to the correct reconstructed firearm.

Sprint 14 keeps the UnitPart as the save host but replaces new record keys with Kingmaker's own item identity candidate:

```text
ItemEntityWeapon.UniqueId
        │
        ├─ must be readable
        ├─ must be Guid or string
        ├─ must parse as a nonempty GUID
        └─ is canonicalized to lowercase D form
                 │
                 ▼
FirearmStateIdentityVaultRecord
    record schema + item ID string + FirearmStateData
```

The source never generates, assigns, mutates, or guesses the ID.

## Delivered source

### Canonical identity domain

`FirearmItemId` is an immutable value object with:

- strict standard GUID D-form parsing;
- empty-GUID rejection;
- lowercase canonical output;
- value equality and deterministic ordering;
- no display-name, blueprint, inventory, or runtime-hash fallback.

### Strict runtime identity provider

`KingmakerFirearmItemIdentityProvider` accepts only a concrete `ItemEntityWeapon` and reads the inherited member named exactly `UniqueId`. Runtime values must be `System.Guid` or `System.String`.

An unavailable, null, malformed, or unsupported value rejects all persistence access. It is never interpreted as an empty firearm.

### Primitive identity records

`UnitPartFirearmStateVault` now serializes:

```text
_identityRecords:
    RecordSchemaVersion
    ItemId
    State
```

The primary collection contains no runtime item object. Compare-and-replace remains transactional and verifies every result.

### One-way Sprint 13 migration

The original `_records` direct-reference list remains in the serialized type solely for compatibility. Migration:

1. validates each legacy payload;
2. resolves the referenced item's engine ID;
3. creates a new identity record when none exists;
4. removes an equivalent redundant legacy record;
5. preserves null or unresolvable legacy records;
6. rejects conflicting states and restores the entire pre-migration carrier set.

New normal writes never create direct-reference records. A development-only fixture can create one for lifecycle testing.

### Existing Sprint 12 migration retained

The four no-op state-token blueprints remain stable. The existing migration repository now writes decoded token state into the identity-backed vault. Reference migration and token migration expose separate diagnostics.

### Reconstruction-oriented repository behavior

`IdentityBackedFirearmStateVaultStore` converts a runtime object into its verified engine identity before delegating to primitive records. Two different runtime objects with the same identity therefore reconstruct the same state, while different identities remain independent even when objects or blueprints compare equal.

## Stable blueprint ledger

Sprint 14 adds no blueprint ID and changes none:

```text
Manifest entries: 12
Active entries:    8
Reserved entries:  4
New Sprint 14 IDs: 0
```

## New dependency-free cases

Sprint 14 adds 35 named C# cases:

- 14 identity parsing/equality/order cases;
- 15 object-to-identity vault cases;
- 4 repository reconstruction/isolation cases;
- 2 migration-snapshot cases.

The complete harness contains 327 declared cases. Final execution still requires a Windows .NET Framework build host.

## Runtime gate

A compiled local package must prove all of the following before the carrier is accepted:

- exactly one readable inherited `UniqueId` exists;
- its installed value type is Guid or string;
- an item's ID remains stable across save/load and process restart;
- movement between equipment, party members, and stash preserves ID and state;
- sale/repurchase preserves the same item ID and state;
- duplication creates a different ID and no state transfer;
- deleted items do not resurrect or transfer state;
- the custom UnitPart and primitive records serialize and restore;
- Sprint 13 reference migration and Sprint 12 token migration both work;
- malformed, missing, duplicate, and conflicting data fail closed.

Until that matrix produces a real GO result, Sprint 15 cannot add ammunition.

## Published artifact status

This milestone contains no compiled DLL and no UMM install archive. It is not ready for Kingmaker under the user's stated definition.
