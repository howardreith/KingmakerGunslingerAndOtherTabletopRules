# Engine-issued item-identity vault

## Purpose

Firearm state must belong to one concrete gun and survive Kingmaker reconstructing runtime item objects during save/load. Object reference identity is excellent within one process but cannot by itself prove continuity across reconstruction.

Sprint 14 therefore tests Kingmaker's own item identity as the durable key.

## Accepted identity contract

The runtime adapter requires:

```text
Type:   Kingmaker.Items.ItemEntityWeapon or subclass
Member: UniqueId, including an inherited member
Value:  System.Guid or System.String
Format: nonempty GUID in D form
Stored: lowercase canonical D-form string
```

The source accepts no alternative member. In particular, it does not use:

- `Id`, `m_Id`, `EntityId`, or `m_UniqueId`;
- item or weapon blueprint GUID;
- display name;
- owner, slot, or inventory position;
- `RuntimeHelpers.GetHashCode`;
- a mod-generated GUID.

Those values may remain useful diagnostics, but they are not persistence keys.

## Record model

```text
UnitPartFirearmStateVault
    _identityRecords[]
        RecordSchemaVersion = 1
        ItemId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        State
            SchemaVersion
            LoadedRounds
            LoadedAmmunitionId
            Condition
```

Canonical empty/Normal state remains represented by the absence of a record.

## Runtime flow

```text
FirearmItemStateService
    │ exact firearm marker validation
    ▼
VaultBackedFirearmStateRepository
    │ immutable state transitions
    ▼
IdentityBackedFirearmStateVaultStore
    │ RequireIdentity(runtime item)
    ▼
KingmakerFirearmItemIdentityProvider
    │ read UniqueId; never mutate it
    ▼
KingmakerFirearmStateVaultStore
    │ primitive identity key
    ▼
UnitPartFirearmStateVault._identityRecords
```

Missing identity throws before a record read or write. This is intentional: treating an unknown identity as empty could overwrite or transfer state.

## Sprint 13 one-way migration

The old serialized field `_records` remains readable and contains direct `ItemEntityWeapon` references. It is a legacy source only.

For one migration transaction:

| Situation | Result |
|---|---|
| Resolvable legacy item; no identity record | Create identity record, remove legacy record |
| Resolvable legacy item; equivalent existing identity state | Remove redundant legacy record |
| Two legacy records resolve to one identity | Preserve all records and throw; never merge live items |
| Resolvable legacy item; conflicting identity state | Preserve all records and throw |
| Null legacy record or item reference | Preserve as unresolved |
| Item lacks valid UniqueId | Preserve as unresolved |
| Malformed schema or state | Preserve pre-migration collections and throw |

The migration constructs working copies and commits both collections together. A conflict does not partially migrate unrelated records in that transaction.

## Sprint 12 token migration

The older enchantment-token path remains outside the UnitPart migration. Once the identity-backed repository is available, a supported token is decoded, written and verified in the identity record, then removed. A conflict preserves both carriers.

## Why this remains provisional

Public diagnostics from Owlcat-engine mods show item entities printed with GUID-like suffixes, and other Owlcat mod code uses entity `UniqueId` values in save-related workflows. Those precedents justify inspecting the contract; they do not prove Kingmaker's item lifecycle behavior.

Only the installed-assembly inspection and full in-game lifecycle matrix can establish whether the identity is stable and semantically correct for firearms.
