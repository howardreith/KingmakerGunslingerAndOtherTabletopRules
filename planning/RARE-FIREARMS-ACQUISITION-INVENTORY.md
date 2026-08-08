# Rare Firearms Acquisition Inventory

Guarded read-only baseline run
`20260808T1720275373614Z-observe-vendor-table-contracts` (runtime ID
`20260808T1720275529421Z-c5dbf9e887b64c1b89ac129ba490d2b3`) passed on
`1c570bd4211d69c5c29f6af46a870146adb1645b`, 0.0.73. It enumerated 43
`BlueprintSharedVendorTable` objects and 26 associations without mutation.

| GUID | Runtime type | Exact name / contents | Ownership / references | Disposition |
|---|---|---|---|---|
| afa2c7f292b8e1c4d9c835f0e8047dd3 | BlueprintSharedVendorTable | C11_JhodVendorTable; 61 fixed entries after current ten-entry publication | Base campaign; shared by Capital_Jhod and many priest/Jhod variants | Reject future firearm stock |
| f720440559fc00949900bfa1575196ac | BlueprintSharedVendorTable | C11_OlegVendorTable; 84 fixed entries | Base campaign; exact ownership pending | General-merchant candidate |
| 7de959347266092448d8a72089ef9778 | BlueprintSharedVendorTable | SmithVendorTable; 16 fixed entries | Exact campaign ownership pending | Preferred blacksmith candidate |
| 03139ca71b2f2a34bae0a8a11a342fe4 | BlueprintSharedVendorTable | C2_VendorTableLarge; 52 fixed entries | Exact ownership pending | Candidate |
| b3bc1bb9f4a59f3438edc505e0f3b407 | BlueprintSharedVendorTable | C3_VendorTableLarge; 50 fixed entries | Exact ownership pending | Candidate |
| fc01b45fee3606749a21d9612c5629a6 | BlueprintSharedVendorTable | C4_VendorTableLarge; 50 fixed entries | Exact ownership pending | Candidate |
| a6bae621a7bd96b4fb3c1511cd2f9fac | BlueprintSharedVendorTable | RogueLike_NPCVendorTable; 173 fixed + dungeon component | BTSL standalone Honest Guy | Selected optional BTSL target |
| 08e090bb2038e3d47be56d8752d5dcaf | BlueprintSharedVendorTable | RogueLike_DragonVendorTable; 346 fixed + dungeon component | BTSL standalone Xelliren variants | Selected optional BTSL target |
| 45f027c06962df249b8c014a4b4e95e3 | BlueprintSharedVendorTable | DLC3_VendorFirstTable; 29 fixed | BTSL campaign HonestGuy_Story | Selected optional BTSL target |
| 420f1da6c2523f64eba810b9b484f60f | BlueprintSharedVendorTable | DLC3_VendorSecondTable; 29 fixed | BTSL campaign SilverDragon_Story | Selected optional BTSL target |

Installed fixed-entry contract: `LootItemsPackFixed.m_Item : LootItem`, public
`LootItem.Item : BlueprintItem`, and `m_Count : Int32`. Exact native/foreign
order, identity and rollback remain mandatory.

No fixed loot target is selected yet. The next observer must inventory exact
`BlueprintLoot`/`BlueprintUnitLoot` candidates, contents, owners, areas, DLC,
fixed/random contract and reference counts for Act 3/4, Pitax and final act.
Static shared publication is proven; save-owned legacy vendor cleanup is not.
