# Blueprint manifest and registration contract

## Sprint 29 status

Sprint 47 extends the append-only contract to **72 stable identifiers: 71 active
and one reserved** with Targeting Head's feature, ability, and Confusion buff.
The ledger has 72 stable identifiers: 71 active and one reserved. Gunslinger
class registration remains a complete twenty-four-blueprint transaction.

Sprint 45 adds Targeting Head's level-seven feature, full-round ability, and
one-round mind-affecting Confusion buff. Earlier additions remain append-only:
Startling Shot, Dead Shot, Gun Training, Utility Shot, Pistol-Whip, Initiative,
Nimble, the level-one deeds and grit, the production class, and firearms.

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

If registration fails after a library mutation, the registry removes only the exact object it created. The current bootstrap rolls the complete sixty-four-blueprint transaction back in reverse order. It never assigns through the dictionary indexer and never intentionally replaces an existing game or mod blueprint.

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
| `KMG.Firearms.AdvancedRifleWeaponType` | `df5e6a66bc494514a740b674ef84c5ba` | `BlueprintWeaponType` | Active | Advanced Rifle type |
| `KMG.Firearms.AdvancedRifleItem` | `a267e7bbc10e425f8adb87844d572b29` | `BlueprintItemWeapon` | Active | Advanced Rifle item |
| `KMG.Firearms.AdvancedRevolverWeaponType` | `a7d3b805c579488eaf91e840896f5d80` | `BlueprintWeaponType` | Active | Six-chamber Advanced Revolver type |
| `KMG.Firearms.AdvancedRevolverItem` | `8ed461fbcc154c51b07e5549211e9f5e` | `BlueprintItemWeapon` | Active | Advanced Revolver item |
| `KMG.Deeds.PistolWhipFeature` | `aa94b21dcaa64fcaa0483c9774a5ee75` | `BlueprintFeature` | Active | Level-three deed grant |
| `KMG.Deeds.PistolWhipAbility` | `9c011fb0a9d34a78b93f6bec673f8210` | `BlueprintAbility` | Active | Standard-action melee attack and Trip |
| `KMG.Deeds.PistolWhipOneHandedType` | `b49cca444edd4c0998f2c9d744b15cfa` | `BlueprintWeaponType` | Active | Hidden 1d6 melee surrogate type |
| `KMG.Deeds.PistolWhipOneHandedItem` | `eeed7234a3ee49db995f1b668d021822` | `BlueprintItemWeapon` | Active | Unowned one-handed rule-event item |
| `KMG.Deeds.PistolWhipTwoHandedType` | `a6006de207d54de19e9c0c60f341840b` | `BlueprintWeaponType` | Active | Hidden 1d10 melee surrogate type |
| `KMG.Deeds.PistolWhipTwoHandedItem` | `77b668da744842bcb9183ef22982ba6d` | `BlueprintItemWeapon` | Active | Unowned two-handed rule-event item |
| `KMG.Classes.GunTrainingSelection` | `3f6c5b8a19d447df9d6af862bc4f83a1` | `BlueprintFeatureSelection` | Active | Cumulative levels 5/9/13/17 firearm-kind selection |
| `KMG.Classes.GunTrainingPistol` | `58b90ab430a249a8a482eaf27b65d874` | `BlueprintFeature` | Active | Pistol Gun Training |
| `KMG.Classes.GunTrainingMusket` | `970bdf4a11ea4f758f9e6ac3fd2ee716` | `BlueprintFeature` | Active | Musket Gun Training |
| `KMG.Classes.GunTrainingBlunderbuss` | `dc258ec601b04745a5b613d2fc095894` | `BlueprintFeature` | Active | Blunderbuss Gun Training |
| `KMG.Classes.GunTrainingRifle` | `144876ce59f847a39031e043fcd939d2` | `BlueprintFeature` | Active | Rifle Gun Training |
| `KMG.Classes.GunTrainingRevolver` | `aa7e85bead1044018285c579b9417691` | `BlueprintFeature` | Active | Revolver Gun Training |
| `KMG.Deeds.DeadShotFeature` | `4f3a81c6d2754ec6920b7d14aa2e6c39` | `BlueprintFeature` | Active | Level-seven Dead Shot feature |
| `KMG.Deeds.DeadShotAbility` | `c6e2147ab3f84aa9812d37c9650be421` | `BlueprintAbility` | Active | Full-round BAB-iterative Dead Shot delivery |

The absence of a state-token enchantment represents canonical Empty/Normal state. The item-owned inert state-token carrier remains authoritative.

## Editing policy

- Never change an existing GUID.
- Never assign an existing GUID to a different symbol.
- Never delete a retired entry.
- Do not activate an entry until the sprint that registers it.
- Add manifest change, source registration, migration note, and tests in the same package.
- Runtime code must never call `Guid.NewGuid` for blueprint identity.
