# Blueprint manifest and registration contract

## Sprint 29 status

The append-only ledger contains **15 stable identifiers: 14 active and one reserved**. Sprint 29 activates one new identity without changing any prior GUID:

- `KMG.Test.RepairAbility` — full-round personal same-item Broken-to-Normal ability.

The Sprint 28 repair-kit and Overhaul identities remain active:

- `KMG.Test.FirearmRepairKitItem` — stackable inert resource shared by Overhaul and Repair.
- `KMG.Test.OverhaulAbility` — full-round personal same-item Wrecked-to-Broken ability.

The touch-AC enchantment remains reserved because touch AC is implemented through a rule patch.

## Purpose

Kingmaker identifies blueprints with stable 32-character hexadecimal strings. Once a custom blueprint is referenced by save-bearing content, changing or reusing its ID can break compatibility. `blueprints/blueprints.json` is therefore an append-only identity ledger.

## Manifest invariants

The runtime accepts only:

- schema version `1`;
- namespace `KMG`;
- `runtimeGenerationAllowed: false`;
- `retiredIdsRemainReserved: true`;
- lowercase 32-character hexadecimal IDs;
- symbols matching `KMG.[A-Za-z0-9_.]+`;
- unique symbols and GUIDs;
- statuses `reserved`, `active`, or `retired`; and
- known JSON properties only.

A `reserved` entry allocates an identity but cannot be registered. An `active` entry may be registered only as its exact `plannedType`. A `retired` entry can never be registered or reassigned.

## Deployment

MSBuild and the package pipeline copy both manifest files beside the compiled DLL:

```text
KingmakerGunslinger/
├── KingmakerGunslinger.dll
├── Info.json
└── blueprints/
    ├── blueprints.json
    └── blueprints.schema.json
```

The loader resolves this fixed path relative to the executing assembly. It does not read an arbitrary user path, registry value, or network location.

## Registration transaction

For each active symbol, `BlueprintRegistry.Register<T>` performs:

```text
manifest resolve and exact-type check
        ↓
registry duplicate-symbol check
        ↓
live Kingmaker GUID collision check
        ↓
create or clone Unity ScriptableObject
        ↓
assign private m_AssetGuid from manifest
        ↓
repeat live collision check
        ↓
append to GetAllBlueprints()
        ↓
BlueprintsByAssetId.Add(guid, asset)
        ↓
verify exact object reference
        ↓
record symbol and log success
```

If registration fails after a library mutation, the registry removes only the exact object it created. The current bootstrap rolls the complete fourteen-blueprint transaction back in reverse order. It never assigns through the dictionary indexer and never intentionally replaces an existing game or mod blueprint.

## Current entries

| Symbol | GUID | Type | Status | Role |
|---|---|---|---|---|
| `KMG.Diagnostic.InitializedFeature` | `6294cc6964914ea7bf450d5ef82fadde` | `BlueprintFeature` | Active | Hidden bootstrap diagnostic |
| `KMG.Firearms.FirearmProficiency` | `5148f69223044799800b65732b6cabea` | `BlueprintFeature` | Active | Shared firearm proficiency and action grant |
| `KMG.Test.TestMusketWeaponType` | `6e499550b44c41b3a1ef0693904a46b8` | `BlueprintWeaponType` | Active | Test Musket type |
| `KMG.Test.TestMusketItem` | `09641295ceea4c558400c43df2ddf1f9` | `BlueprintItemWeapon` | Active | Test Musket item |
| `KMG.Test.TouchAcEnchantment` | `6070ed43137d4b7a81d2c112e37b4c0f` | `BlueprintWeaponEnchantment` | Reserved | Unused while touch AC remains rule-patched |
| `KMG.Test.ReloadAbility` | `19e24b74331f437282077ce58e739d0f` | `BlueprintAbility` | Active | Full-round reload |
| `KMG.Test.LoadedStateToken` | `c11a8965dbdd43f08080f4dc51a29113` | `BlueprintWeaponEnchantment` | Active | Loaded/Normal state |
| `KMG.Test.BlackPowderItem` | `ea966bf998a647cf97b0ed92f71c4b7d` | `BlueprintItem` | Active | Black Powder Charge |
| `KMG.Test.LeadBulletItem` | `55c29771445947d685dba9e1ead46a42` | `BlueprintItem` | Active | Lead Ball; stable symbol retained |
| `KMG.Test.BrokenEmptyStateToken` | `5513972dd2624c9f86bc29c850dac736` | `BlueprintWeaponEnchantment` | Active | Empty/Broken state |
| `KMG.Test.BrokenLoadedStateToken` | `f5fa460f93214458b6f59db24b0dfd12` | `BlueprintWeaponEnchantment` | Active | Loaded/Broken state |
| `KMG.Test.WreckedStateToken` | `877f65ca3a404f2e98af528b7fb1a2fb` | `BlueprintWeaponEnchantment` | Active | Empty/Wrecked state |
| `KMG.Test.FirearmRepairKitItem` | `f2b564234b8a4b0d88a7a46128556bef` | `BlueprintItem` | Active | Firearm Repair Kit |
| `KMG.Test.OverhaulAbility` | `8a0ba821382640b58ec9ff168ed778a5` | `BlueprintAbility` | Active | Full-round same-item Overhaul |
| `KMG.Test.RepairAbility` | `c914b3c0786463b7a1e17e47447ee5b1` | `BlueprintAbility` | Active | Full-round same-item ordinary Repair |

The absence of a state-token enchantment represents canonical Empty/Normal state. The item-owned inert state-token carrier remains authoritative.

## Editing policy

- Never change an existing GUID.
- Never assign an existing GUID to a different symbol.
- Never delete a retired entry.
- Do not activate an entry until the sprint that registers it.
- Add manifest change, source registration, migration note, and tests in the same package.
- Runtime code must never call `Guid.NewGuid` for blueprint identity.
