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

The identities use the repository's deterministic SHA-256 symbol derivation
and append-only manifest contract. Each family's four generic items share its
single weapon type and category; material and enhancement tiers never create
new categories.

