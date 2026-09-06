# Blueprint manifest and registration contract

## Sprint 55 status

The append-only contract contains 233 stable identifiers: 232 active and one reserved. Prior wrapper and persisted marker identities remain hidden for compatibility; the latest identities append the native Pistolero/Musket Master archetypes, truthful archetype deed summaries, Steady Aim and Up Close and Deadly action/marker pairs, Twin Shot Knockdown's targeted action, four archetype-aware True Grit choices, and supporting scoped features without changing any established GUID.

The Rare Firearms continuation appended ten collision-free identities and
activated Seeking, Reliable, and all eight item identities. Paper Cartridges
Phase 1 appends the cartridge item plus two item-owned loaded-state tokens. The
manifest contains 1,375 stable identifiers: 1,374 active and one reserved.
Validator contract: 1853 stable identifiers: 1851 active and 2 reserved.

The second reservation, `KMG.ElementalRaces.Diagnostics.ProbeRace`
(`57005fca40ab4775ae2fea5613214054`), is development-only. Ordinary bootstrap
does not register it. The guarded Elemental Races probe temporarily registers
the exact identity without publishing it to `CharacterRaces`, then removes the
owned dictionary/list entries before completion.

Elemental Races owns 215 active manifest identities. The 0.0.114 foundation
contains 24 mechanical race, feature, resource, and SLA blueprints; 16
Human-compatible body-wrapper and visual-preset blueprints; and 28
`EquipmentEntity` body, head, and optional horn recolor proxies. Release A
adds 53 stable heritage identities: four selections, twelve choice markers,
eight affinity providers, eight feature/resource/ability SLA triplets, and
five supporting ability or weapon-enchantment identities. Release B appends 25
stable identities: eleven feat features, nine abilities, four buffs, and one
exact-item weapon enchantment. Release C appends 62 stable replacement-
framework identities: ten slot selections, ten retain-base markers, 21
visible trait markers, and 21 hidden providers. Three blood-trait buffs append
their own fixed identities. Efreeti Magic adds three ability identities and
one shared resource identity. All 187 elemental blueprint identities and all
28 resource proxies register on every startup so saved race, heritage,
provider, resource, feat, trait, active-effect, and doll references continue to
resolve while selector publication is disabled. The resource proxies reuse
native Kingmaker geometry and native ramp textures; the package contains no
extracted game asset.

The blood buffs append to, and do not replace, the 62 framework identities:

| Trait buff | Stable GUID |
| --- | --- |
| Fire in the Blood | `e117e1e0a17a4acec001000000000063` |
| Stone in the Blood | `e117e1e0a17a4acec001000000000064` |
| Storm in the Blood | `e117e1e0a17a4acec001000000000065` |

Efreeti Magic appends four identities; native mechanics pass incrementally,
while save-backed qualification remains pending:

| Symbol suffix under `KMG.ElementalRaces.Traits.Ifrit.EfreetiMagic` | Stable GUID |
| --- | --- |
| `.Resource` | `e117e1e0a17a4acec001000000000066` |
| `.Ability` | `e117e1e0a17a4acec001000000000067` |
| `.EnlargePerson` | `e117e1e0a17a4acec001000000000068` |
| `.ReducePerson` | `e117e1e0a17a4acec001000000000069` |

Each blood-buff symbol is its visible trait marker symbol plus `.FastHealingBuff`.
Daily actual-healing expenditure is saved in `UnitPartElementalBloodCapacity`;
provider removal does not remove that ledger. Focused native checks pass in
KMG-only and highest-risk combined profiles. The six-Insight/blood fresh-process
cycle also preserves all nine active blood buffs and partially spent capacity;
complete lifecycle and other traits' persistence remain separate gates.

Release A runtime observed all 53 appended identities at their exact manifest
GUIDs. The 0.0.114-to-0.0.115 save-backed migration retained every legacy race,
General affinity, General SLA, and General resource identity and introduced no
marker requirement for existing characters. Module-OFF compatibility runs
across all six required installed profiles observed all identities registered
while publishing no Elemental race to the native selector.

Release B runtime observed all 25 appended feat identities at their exact
manifest GUIDs in every module state. With Elemental Races enabled, all eleven
exact feat references publish once to the universal selector and the four
Combat feats publish once to the Fighter selector. With the module disabled,
all identities remain registered and both selectors receive zero project
entries. Replay, foreign-order preservation, exact-GUID conflict refusal, and
reverse rollback passed in all six required installed compatibility profiles.

The 25 active `KMG.BrownFur.*` identities are owned by the isolated optional
extension. They are registered only when the structural Call of the Wild
contract succeeds and are excluded from the package core's unconditional
registration count. Active identity status does not by itself publish the
Brown-Fur archetype to CotW's selector.
The 73 active `KMG.UrbanBarbarian.*` identities are unconditional native-core
identities. They register with the module on or off so existing owners remain
loadable; the setting controls only publication in the native Barbarian
archetype selector. The nine active Bodyguard/In Harm's Way subsystem identities are also
unconditional native-core identities. They remain registered when
`bodyguard-feats` is disabled so existing feat owners and persistent mode
markers remain loadable; publication and runtime mutation are gated
independently. The unconditional package-core blueprint registration count is
1784; the 28 elemental visual resource proxies are registered separately in
the validated native resource cache.
The historical non-Brown-Fur reservation remains reserved independently.
The 1,155 Expanded Summoning identities freeze the feature foundation: 67 unit
identities, 1,050 abilities, 17 buffs, three AI actions, three brains, nine
weapon identities, two bounded resources, and one hidden KMG extraplanar marker.
They register in every feature-module state; live parent publication remains
independently gated. Bootstrap therefore derives the complete 1,406-blueprint
transaction from the 254 pre-feature identities plus the 1,152 feature-local
identities.

The Elven Branched Spear foundation appends ten active identities: one shared
weapon type, six base-family weapon items, and ordinary child features for
Exotic Weapon Proficiency and Rogue Finesse Training, plus the zero-cost
inherent movement-opportunity accuracy enchantment. These identities remain
registered in every module state; module state gates only new publication.

The Eastern Weapons generic foundation appends fifteen active identities: one
stable weapon type and four generic items for each of Wakizashi, Katana, and
Nodachi. The identities register in every module state. The assigned category
values are independent of blueprint GUIDs and are collision-checked against
every live weapon type before registration.

Paper Cartridges Phase 1 identities are append-only:

| Symbol | GUID | Type | Status | Purpose |
| --- | --- | --- | --- | --- |
| `KMG.Ammunition.PaperCartridge` | `fea7337cfd06417a853546af9d950f77` | `BlueprintItem` | Active | Stackable prepared early-firearm ammunition |
| `KMG.Gunsmithing.CraftPaperCartridges` | `936ffac5400b46b3a72fe503e0947288` | `BlueprintAbility` | Active | Shared-entitlement 20-for-24 gp Paper Cartridge recipe |
| `KMG.Ammunition.PaperLoadedNormalStateToken` | `a6344f33e7344d4aab249485faedf7fd` | `BlueprintWeaponEnchantment` | Active | Inert Normal paper-loaded item state |
| `KMG.Ammunition.PaperBrokenLoadedStateToken` | `fdd814300fff4eea89d9d508663aebc0` | `BlueprintWeaponEnchantment` | Active | Inert Broken paper-loaded item state |
| `KMG.Ammunition.PaperCartridgeModeMarker` | `69a804ea1fd14a5da3ba893c373f481f` | `BlueprintBuff` | Active | Hidden unit-local selected-source marker |
| `KMG.Ammunition.UsePaperCartridges` | `b0f16e90dc4e48929e111a7d56b62e5d` | `BlueprintActivatableAbility` | Active | Visible off-by-default cartridge reload-source mode |
Bootstrap validates and rolls back the complete one-hundred-fifty-eight-blueprint transaction. The historical complete twenty-four-blueprint transaction remains part of the append-only identity record.
Expert Loading adds one feature, one free-action pre-shot ability, and one
unit-owned armed marker. Sprint 50 added Bleeding Wound's thirteen facts.

Sprint 45 adds Targeting Head's level-seven feature, full-round ability, and
one-round mind-affecting Confusion buff. Earlier additions remain append-only:
Startling Shot, Dead Shot, Gun Training, Utility Shot, Pistol-Whip, Initiative,
Nimble, the level-one deeds and grit, the production class, and firearms.

- `KMG.Test.RepairAbility` — full-round personal same-item Broken-to-Normal ability.

The Sprint 28 repair-kit and Overhaul identities remain active:

- `KMG.Test.FirearmRepairKitItem` — stackable inert resource shared by Overhaul and Repair.
- `KMG.Test.OverhaulAbility` — full-round personal same-item Wrecked-to-Broken ability.

The Sixth Playtest identities are active and append-only:

| Symbol | GUID | Type | Status | Purpose |
| --- | --- | --- | --- | --- |
| `KMG.Deeds.DeadeyeArmedBuff` | `88ca4220e3944b65b2b9fb3afea35b08` | `BlueprintBuff` | Active | Visible one-round Deadeye Armed state |
| `KMG.Gunsmithing.GunsmithKit` | `d52aacb753434691b1ed85a16cc87104` | `BlueprintItem` | Active | Persistent non-consumable Gunsmith's Kit |
| `KMG.Gunsmithing.OverhaulKit` | `77fddc4f10614481a23f7dc8d1188848` | `BlueprintItem` | Active | Consumable Wrecked-to-Broken maintenance kit |
| `KMG.Gunsmithing.CraftBasicAmmunition` | `8d7bb3a3e9444600b636fa58076a219b` | `BlueprintAbility` | Active | Once-per-rest basic ammunition crafting action |
| `KMG.Gunsmithing.CraftedThisRest` | `f14e26a501c3423686d8948e7dd71950` | `BlueprintFeature` | Active | Persisted once-per-rest entitlement marker |
| `KMG.Firearms.Projectile` | `adcd3d85c18b4db694420cb443c4da99` | `BlueprintProjectile` | Active | Clone-derived native-lifecycle firearm projectile |

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

If registration fails after a library mutation, the registry removes only the exact object it created. The current bootstrap rolls the complete one-hundred-twenty-five-blueprint transaction back in reverse order. It never assigns through the dictionary indexer and never intentionally replaces an existing game or mod blueprint.

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
| `KMG.Firearms.AdvancedRifleWeaponType` | `df5e6a66bc494514a740b674ef84c5ba` | `BlueprintWeaponType` | Active legacy | Hidden Rifle type retained for old-save/Toy Box recognition; not published or ordinarily acquired |
| `KMG.Firearms.AdvancedRifleItem` | `a267e7bbc10e425f8adb87844d572b29` | `BlueprintItemWeapon` | Active legacy | Hidden Rifle item retained for old-save/Toy Box recognition; not published or ordinarily acquired |
| `KMG.Firearms.AdvancedRevolverWeaponType` | `a7d3b805c579488eaf91e840896f5d80` | `BlueprintWeaponType` | Active legacy | Hidden Revolver type retained for old-save/Toy Box recognition; not published or ordinarily acquired |
| `KMG.Firearms.AdvancedRevolverItem` | `8ed461fbcc154c51b07e5549211e9f5e` | `BlueprintItemWeapon` | Active legacy | Hidden Revolver item retained for old-save/Toy Box recognition; not published or ordinarily acquired |
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
| `KMG.Classes.GunTrainingRifle` | `144876ce59f847a39031e043fcd939d2` | `BlueprintFeature` | Active legacy | Hidden registered Rifle training retained only for existing owners; absent from the selection |
| `KMG.Classes.GunTrainingRevolver` | `aa7e85bead1044018285c579b9417691` | `BlueprintFeature` | Active legacy | Hidden registered Revolver training retained only for existing owners; absent from the selection |
| `KMG.Deeds.DeadShotFeature` | `4f3a81c6d2754ec6920b7d14aa2e6c39` | `BlueprintFeature` | Active | Level-seven Dead Shot feature |
| `KMG.Deeds.DeadShotAbility` | `c6e2147ab3f84aa9812d37c9650be421` | `BlueprintAbility` | Active | Full-round BAB-iterative Dead Shot delivery |
| `KMG.Deeds.BleedingWoundFeature` | `f8fc9e345ef243dd862bfaa07abccfeb` | `BlueprintFeature` | Active | Level-eleven four-choice deed grant |
| `KMG.Deeds.BleedingWoundHitPointsAbility` | `0beb35e712f749abb660f593e790ab39` | `BlueprintAbility` | Active | HP bleed arming choice |
| `KMG.Deeds.BleedingWoundStrengthAbility` | `9d8b076131364a7394aa225b4306419c` | `BlueprintAbility` | Active | Strength bleed arming choice |
| `KMG.Deeds.TargetingArmsFeature` | `d9403a88ced642d595e6649a0cfafe9f` | `BlueprintFeature` | Active | Level-seven Targeting Arms deed grant |
| `KMG.Deeds.TargetingArmsAbility` | `5d194f9f2c9a46fc9c1dfe5aef7972b8` | `BlueprintAbility` | Active | No-damage firearm attack and one-round main-hand Disarm |
| `KMG.Deeds.BleedingWoundDexterityAbility` | `41fde8c96f7d468d9f9e6ad4d9b60fb0` | `BlueprintAbility` | Active | Dexterity bleed arming choice |
| `KMG.Deeds.BleedingWoundConstitutionAbility` | `a964315c427c4421bdf24c6ccea8700e` | `BlueprintAbility` | Active | Constitution bleed arming choice |
| `KMG.Deeds.BleedingWoundHitPointsArmed` | `e563e8cdec324c59959c4dc8b9449702` | `BlueprintBuff` | Active | Context-carrying armed marker |
| `KMG.Deeds.BleedingWoundStrengthArmed` | `74749a03f92e4fabaec2830bd66f7731` | `BlueprintBuff` | Active | Context-carrying armed marker |
| `KMG.Deeds.BleedingWoundDexterityArmed` | `cd51a3e250094365b54929490a1d64b6` | `BlueprintBuff` | Active | Context-carrying armed marker |
| `KMG.Deeds.BleedingWoundConstitutionArmed` | `479e8f2c4dc7480cac9664ac10232e21` | `BlueprintBuff` | Active | Context-carrying armed marker |
| `KMG.Deeds.BleedingWoundHitPointsBuff` | `7fcfa24d2dcb4044bc2fbe23a1937626` | `BlueprintBuff` | Active | Recurring HP Bleed fact |
| `KMG.Deeds.BleedingWoundStrengthBuff` | `fe07b11676904019b1471905708764eb` | `BlueprintBuff` | Active | Recurring Strength Bleed fact |
| `KMG.Deeds.BleedingWoundDexterityBuff` | `9252d6155b3e4c258cbc7a1e794b8a5a` | `BlueprintBuff` | Active | Recurring Dexterity Bleed fact |
| `KMG.Deeds.BleedingWoundConstitutionBuff` | `de789d99a1a742b5a7ac3da9c666a804` | `BlueprintBuff` | Active | Recurring Constitution Bleed fact |
| `KMG.Deeds.ExpertLoadingFeature` | `38c1d90d421b41f7aa94f06e199d2021` | `BlueprintFeature` | Active | Level-eleven Expert Loading deed grant |
| `KMG.Deeds.ExpertLoadingAbility` | `d981c72e9df94d1989530453669ea3b5` | `BlueprintAbility` | Active | Free-action pre-shot arming ability |
| `KMG.Deeds.ExpertLoadingArmed` | `5a21b3ff5940476f97fe5f7844fa4509` | `BlueprintBuff` | Active | Unit-owned next-firearm marker |
| `KMG.Deeds.LightningReloadFeature` | `7e6a1185097a4e64a1c1f409d006cf22` | `BlueprintFeature` | Active | Level-eleven Lightning Reload deed grant |
| `KMG.Deeds.LightningReloadAbility` | `13626eb6f20248b5934172c3270d167f` | `BlueprintAbility` | Active | Swift-action equipped-firearm reload |
| `KMG.Deeds.LightningReloadUsed` | `b35b22a49a4c4645a1b730698d35837d` | `BlueprintBuff` | Active | Unit-owned once-per-round marker |
| `KMG.Deeds.EvasiveFeature` | `cce09209a18542c887dfb682b53c11a4` | `BlueprintFeature` | Active | Level-fifteen positive-grit controller |
| `KMG.Deeds.EvasiveEvasionBenefit` | `7e71a4a7c40f432f85904280752321a9` | `BlueprintFeature` | Active | Exact native Reflex Evasion mechanics clone |
| `KMG.Deeds.EvasiveUncannyDodgeBenefit` | `ecf97be86d97489d9041dcff2d784570` | `BlueprintFeature` | Active | Exact native Uncanny Dodge mechanics clone |
| `KMG.Deeds.EvasiveImprovedUncannyDodgeBenefit` | `59dcfeea8ce443778ff784138646116c` | `BlueprintFeature` | Active | Exact native CannotBeFlanked mechanics clone |
| `KMG.Deeds.MenacingShotFeature` | `6b08a7d17d564f35a8157044240499ce` | `BlueprintFeature` | Active | Level-fifteen Menacing Shot deed grant |
| `KMG.Deeds.MenacingShotAbility` | `3107264a6fdf4cebb70e20f593327eee` | `BlueprintAbility` | Active | 30-foot exact native Fear-derived delivery |
| `KMG.Deeds.SlingersLuckFeature` | `6b1cda2114444ea09d9aad132c974f5a` | `BlueprintFeature` | Active | Level-fifteen Slinger's Luck deed grant |
| `KMG.Deeds.SlingersLuckSavingThrowAbility` | `f0305fc5463f4068a19d96e6f4e02c67` | `BlueprintAbility` | Active | Fixed two-grit saving-throw reroll arming action |
| `KMG.Deeds.SlingersLuckSkillCheckAbility` | `f759c4b6fba246f4bb9f34b2c30ab7b5` | `BlueprintAbility` | Active | Fixed one-grit skill-check reroll arming action |
| `KMG.Deeds.SlingersLuckSavingThrowArmed` | `841a2ca5e4994da48fadcc4f3a3c3579` | `BlueprintBuff` | Active | Unit-owned next-saving-throw marker |
| `KMG.Deeds.SlingersLuckSkillCheckArmed` | `e553eb5c573b4b178f4b973850f5a0e3` | `BlueprintBuff` | Active | Unit-owned next-skill-check marker |
| `KMG.Deeds.CheatDeathFeature` | `a8a316812d244e3498daf29ecf2be115` | `BlueprintFeature` | Active | Level-19 all-grit Cheat Death handler |
| `KMG.Deeds.StunningShotFeature` | `4e6f09ec942d4b8aa5fac53b35bc2171` | `BlueprintFeature` | Active | Level-19 Stunning Shot feature |
| `KMG.Deeds.StunningShotAbility` | `b1a5e61437714bd19ee84b10bead70a2` | `BlueprintAbility` | Active | Free-action arming ability |
| `KMG.Deeds.StunningShotArmed` | `c2b6f72548824ce2aff95c21cfbe81b3` | `BlueprintBuff` | Active | Unit-owned next-shot marker |
| `KMG.Deeds.StunningShotStunned` | `d3c7073659934df3b00a6d32d0cf92c4` | `BlueprintBuff` | Active | Exact native Stunned clone |
| `KMG.Classes.TrueGritSelection` | `249734aff917457face92dd836a94236` | `BlueprintFeatureSelection` | Active | Level-20 selection granted twice |
| `KMG.Classes.TrueGritDeadeye` | `f66bbe9a3c77448d94672e14ba7621af` | `BlueprintFeature` | Active | Deadeye choice |
| `KMG.Classes.TrueGritGunslingersDodge` | `d5aa091bc3df428188e708777492c48c` | `BlueprintFeature` | Active | Gunslinger's Dodge choice |
| `KMG.Classes.TrueGritQuickClear` | `0437dc9a96fa4ef285df30af04061e86` | `BlueprintFeature` | Active | Quick Clear choice |
| `KMG.Classes.TrueGritGunslingerInitiative` | `cd087a10328646f99de79c580966af4e` | `BlueprintFeature` | Active | Gunslinger Initiative choice |
| `KMG.Classes.TrueGritPistolWhip` | `5735ce8815a4416e86562ca81a3694b2` | `BlueprintFeature` | Active | Pistol-Whip choice |
| `KMG.Classes.TrueGritStopBleeding` | `f854458726034c4f8a0e677a6e335854` | `BlueprintFeature` | Active | Stop Bleeding choice |
| `KMG.Classes.TrueGritDeadShot` | `01cfee9aff4a48dbb68e00897f7ba3a5` | `BlueprintFeature` | Active | Dead Shot choice |
| `KMG.Classes.TrueGritStartlingShot` | `e41c7a2a27f441b0a20e26e49a2bdaf3` | `BlueprintFeature` | Active | Startling Shot choice |
| `KMG.Classes.TrueGritTargetingHead` | `d51dcac901d54b40b90864ea538693e8` | `BlueprintFeature` | Active | Targeting Head choice |
| `KMG.Classes.TrueGritTargetingTorso` | `3e64032419174f54a3fea01957ad886c` | `BlueprintFeature` | Active | Targeting Torso choice |
| `KMG.Classes.TrueGritTargetingLegs` | `90759e6ea511456f9e978e42466329c1` | `BlueprintFeature` | Active | Targeting Legs choice |
| `KMG.Classes.TrueGritTargetingArms` | `863c8bff2aaa43fc8cb98611d1b250d7` | `BlueprintFeature` | Active | Targeting Arms choice |
| `KMG.Classes.TrueGritBleedingWound` | `611626136df74f34a824e1b831948698` | `BlueprintFeature` | Active | Bleeding Wound choice |
| `KMG.Classes.TrueGritExpertLoading` | `bd59e6f11aa3469eb610ddd47b598520` | `BlueprintFeature` | Active | Expert Loading choice |
| `KMG.Classes.TrueGritLightningReload` | `0b2733aed049462c81ae337994ded859` | `BlueprintFeature` | Active | Lightning Reload choice |
| `KMG.Classes.TrueGritEvasive` | `778520ce40ab47d7a2cbfecf2d59fb6c` | `BlueprintFeature` | Active | Evasive choice |
| `KMG.Classes.TrueGritMenacingShot` | `6b31edab150745e69b9b7fb70cad7eab` | `BlueprintFeature` | Active | Menacing Shot choice |
| `KMG.Classes.TrueGritCheatDeath` | `bb9cf569190e458daf720d95264380d4` | `BlueprintFeature` | Active | Cheat Death choice |
| `KMG.Classes.TrueGritStunningShot` | `0d8c89f38b6d4bdcbef25a63d8bf4ef4` | `BlueprintFeature` | Active | Stunning Shot choice |

The absence of a state-token enchantment represents canonical Empty/Normal state. The item-owned inert state-token carrier remains authoritative.

| `KMG.Deeds.DeathsShotFeature` | `c90022e8f715409bb3b2898f30a6a42f` | `BlueprintFeature` | Active | Level-19 Death's Shot grant |
| `KMG.Deeds.DeathsShotAbility` | `effaa5da710e486c87e6e3637c00e1ed` | `BlueprintAbility` | Active | Free-action arming ability |
| `KMG.Deeds.DeathsShotArmed` | `dfc8df775a53441292502d05f13334e4` | `BlueprintBuff` | Active | Next-firearm marker |
| `KMG.Deeds.DeathsShotDeathEffect` | `b3ed104593874cdc9a0f5bbf99d26cd2` | `BlueprintBuff` | Active | Native death carrier |
| `KMG.Classes.TrueGritDeathsShot` | `0e96be083f894aa19ae9a8c9eeeff4fc` | `BlueprintFeature` | Active | Death's Shot choice |
| `KMG.Feats.AcadamaeGraduate` | `7939ff087cb843729448589ba2de19f1` | `BlueprintFeature` | Active | General Acadamae Graduate feat identity |
| `KMG.Items.CordOfStubbornResolve` | `c4b804d9ebf941b4842b0a461a2b6b6d` | `BlueprintItemEquipmentBelt` | Active | Belt-slot Cord identity |

Version 0.0.75 registers 250 active project identities in every module configuration. The two identities above are the only identities appended by this mission; disabled modules change publication only, never registry membership.

Version 0.0.76 appends `KMG.Feats.AcadamaeGraduateModeMarker`
(`b5fc52ec666640318f8921d5fa60ec39`) and
`KMG.Feats.UseAcadamaeGraduate`
(`a780ab99b76849ed825729808e2bbf29`). Shield Other appends
`KMG.Spells.ShieldOther.Ability` (`6a8c4c1d2fbe4d6a9a724988c1348401`)
and `KMG.Spells.ShieldOther.TargetBuff`
(`7bd92e3c44ad42e7b523ee8ed7afc602`). It registers 254 active identities in
every module configuration; the ledger contains those identities plus one
historical reserved entry. Established GUIDs remain unchanged.

## Editing policy

- Never change an existing GUID.
- Never assign an existing GUID to a different symbol.
- Never delete a retired entry.
- Do not activate an entry until the sprint that registers it.
- Add manifest change, source registration, migration note, and tests in the same package.
- Runtime code must never call `Guid.NewGuid` for blueprint identity.

## 0.0.89 identity statement

Weapon Presentation Calibration adds no blueprint identity and changes no
existing GUID. Its changes are confined to project-owned presentation assets,
clone-local visual parameters, request-gated evidence infrastructure, and
release metadata. Native donor blueprints remain unmodified.

## 0.0.88 identity statement

The overnight bug-fix batch adds no blueprint identity and changes no existing
GUID. It changes only acquisition references, runtime policy/adapters, and
project-owned presentation assets. The 30-item acquisition inventory in
`planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md` maps existing stable item
GUIDs to exact installed base-campaign loot GUIDs; target GUIDs are references
to native Kingmaker blueprints and are not project-owned manifest entries.
