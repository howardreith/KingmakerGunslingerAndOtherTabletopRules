# Eastern Weapons blueprint identities

This ledger records the first production Eastern Weapons identities. All are
active in `blueprints/blueprints.json` and register in every feature-module
state. Module state controls publication and custom presentation, not saved
identity availability.

The stable category values are not blueprint GUIDs:

| Family | Stable category value |
| --- | ---: |
| Wakizashi | `0x004B4D48` (`4934984`) |
| Katana | `0x004B4D49` (`4934985`) |
| Nodachi | `0x004B4D4A` (`4934986`) |

The guarded installed-game inventory proved all three values unoccupied across
136 loaded weapon types before implementation. Runtime registration repeats
that complete-graph collision check and fails closed on any foreign owner.

| Symbol | GUID | Type |
| --- | --- | --- |
| `KMG.EasternWeapons.Wakizashi.WeaponType` | `86bd3d7faf1aec1c527fb9c0d87a395c` | `BlueprintWeaponType` |
| `KMG.EasternWeapons.Katana.WeaponType` | `85d96c1dd1eb02b381c2b3f8ad345952` | `BlueprintWeaponType` |
| `KMG.EasternWeapons.Nodachi.WeaponType` | `41c269cc820ee734f437ab3fa20de198` | `BlueprintWeaponType` |
| `KMG.EasternWeapons.Wakizashi.BaseItem` | `b61ee7e62bc9288004eb0121c8f5d37e` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Wakizashi.MasterworkItem` | `58fd0f272f4523458016dc3656b778c3` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Wakizashi.ColdIronItem` | `1aa01a0528eb595b5cbf19ac7c71a64e` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Wakizashi.Plus1Item` | `83a507873a518b54793d0da632def246` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.BaseItem` | `aba40a9e8302b31e4daa2acf6ab48a46` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.MasterworkItem` | `37e8c76b2fc196e9f82e1196e918263c` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.ColdIronItem` | `599f8f45f325911ffcbdbd6544ba114f` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.Plus1Item` | `87b3d851726a4a9abd0baec6beca957c` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.BaseItem` | `35b7082d98ff45ba51dce536a1bc68a1` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.MasterworkItem` | `df5a3b333eab59c04028d88084d7ada9` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.ColdIronItem` | `0db026048052031be931b9701b3859ef` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.Plus1Item` | `38e31ba5cdbdc668f8dcd8985070c0b7` | `BlueprintItemWeapon` |
| KMG.EasternWeapons.ProficiencyPolicyEnchantment | 1264cc0bf069541d8c2191d05fc40c5d | BlueprintWeaponEnchantment |
| KMG.EasternWeapons.Wakizashi.ExoticWeaponProficiency | b14f7d9b2b665801a9d5b916c6be4ea9 | BlueprintFeature |
| KMG.EasternWeapons.Katana.ExoticWeaponProficiency | 93ef81404f085e2a8b261bdab15d5a08 | BlueprintFeature |
| KMG.EasternWeapons.Wakizashi.FinesseTraining | dfdc2a631ebb5980934181c86e1c43fd | BlueprintFeature |
| `KMG.EasternWeapons.Wakizashi.PaperLantern` | `fbb319cb67ae5657820548791a7a3733` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Wakizashi.QuietCurrent` | `be05a24b1b145e1ea008a4bf42b04c32` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Wakizashi.FallingPetal` | `c56dd11c12355a83b1cd9d833b2e5321` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Wakizashi.FoxfireWhisper` | `c7fc72c801e9506bb0c87e84eee8d313` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Wakizashi.EmptySleeve` | `a576839afc71574eb77203bf390fdf30` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Wakizashi.NightWithoutMoon` | `dc660fcebcc855bfb046336fc78a93ae` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.WayfarersOath` | `9ac64342cca85f72b0fe81cb6b9c53c0` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.WinterReed` | `060f933d8912594cbc3da731c4dae7a3` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.DrawnHorizon` | `d3f2a227bd335087805eb7225721dc83` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.ThunderAtTheGate` | `d5c7922d57a95025a977dd1ee59cb098` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.MoonlitCrossing` | `457e6f3694405f27999cf46047fafa52` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.HeavensMeasure` | `dc086bdf8af25bceb569c8f5c627f560` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.BorderSentinel` | `c1c7a6746916504ebfdcb2b650a7145b` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.CloudCleaver` | `bb863dabbf655059af768723cf6226ba` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.StormOverStone` | `a7559dde16945f90aada81ecf9adb97a` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.MountainSunder` | `5867c9be30e15d3a8a22e0f442959d03` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.UnfixedForm` | `f4bed29f193e57f6826dc83a684e65db` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Nodachi.WorldTreeSeverer` | `e6e5cf56d3a259debd2f16a300bff115` | `BlueprintItemWeapon` |
| `KMG.EasternWeapons.Katana.WayfarersOath.EquippedFact` | `2fd1feb61e7b53af8c8aee70eb85d5b8` | `BlueprintBuff` |
| `KMG.EasternWeapons.Wakizashi.FallingPetal.EffectEnchantment` | `1008f83705ce543f99b361a288797769` | `BlueprintWeaponEnchantment` |
| `KMG.EasternWeapons.Wakizashi.FallingPetal.ArmorClassBuff` | `921d9f64271d507c9ae00c74a405b937` | `BlueprintBuff` |
| `KMG.EasternWeapons.Katana.MoonlitCrossing.EquippedFact` | `596a9b06c478572d8d82414ad8d78935` | `BlueprintBuff` |
| `KMG.EasternWeapons.Nodachi.MountainSunder.EffectEnchantment` | `3c62b9e8b6655a7abf76327f244c337a` | `BlueprintWeaponEnchantment` |
| `KMG.EasternWeapons.Nodachi.MountainSunder.RoundMarker` | `4ca1d8e286dd5c82a2ee8ce369146d57` | `BlueprintBuff` |
| `KMG.EasternWeapons.Nodachi.UnfixedForm.EffectEnchantment` | `f5051c4567ee595995cd67692672bc81` | `BlueprintWeaponEnchantment` |

The identities use the repository's deterministic SHA-256 symbol derivation
and append-only manifest contract. Each family's four generic items share its
single weapon type and category; material and enhancement tiers never create
new categories. The four proficiency identities and eighteen named-item
identities use UUIDv5 with namespace
497608d5-6817-5d68-8df4-b462cc3a6c13, itself derived from the upstream
repository URL, and the exact manifest symbol as the name. This makes the
allocation independently reproducible while the checked-in ledger remains the
runtime authority.
