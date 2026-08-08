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
| 7de959347266092448d8a72089ef9778 | BlueprintSharedVendorTable | SmithVendorTable; 16 fixed entries | Base capital; exact owners `CapitalOwlbearAttack_Blacksmith` (`ba7a7a2842d072046be55b3f9034d04e`) and `VerdelBlacksmith` (`478862ab88b8ef24385cb386c1644dc2`), one direct reference each | Selected capital blacksmith table |
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

## Fixed campaign selections from live graph

Guarded PASS `20260808T1734599486274Z-observe-vendor-table-contracts`, runtime
ID `20260808T1734599848444Z-0502d6b37e1a4049a522927f30c23f3a`, enumerated
437 bounded candidates. Each selection is `BlueprintLoot`, fixed `LootEntry[]`,
base-campaign area-owned, and has zero direct references from another registered
blueprint in the exposed graph (the exact area field is the ownership evidence).

| Item | Target GUID / exact name | Area / chapter | Existing fixed contents | References / nature | Disposition |
|---|---|---|---|---|---|
| Duelist's Rebuttal | `193b1222846a0114197e716cb35d3ce8` / `Forest_cache` | `VordakaiTombLevel2` (`b4789ad28d5ae9340bf4ea2ed747a8b0`), Act 4 | BreastplateStandartPlus3 ×1 | zero registered direct refs; area-owned fixed martial cache | Selected |
| The River King's Measure | `b34367a637010f743815aed5875152bd` / `PoorHuman_IrovettiChambers_ChestHuge_Outline (3)` | `IrovettiPalace` (`bf9dbc2998849ee40bbdba9cb40a7d4c`), Pitax | fixed gems/valuables (full record in run) | zero registered direct refs; royal-chambers fixed chest | Selected |
| Irovetti's Ovation | `485300a2036a763499aa77ebac1f83c6` / `Forest_PoorLoot_PuzzleItem3_Instrument` | `IrovettiPalace`, Pitax | `PuzzleItem3_Instrument` ×1 | zero registered direct refs; distinct performance-instrument cache | Selected |
| The Last Word | `36d315a81b36980438e2ef1a866791d1` / `FirstWorld_BasementGoodLoot01` | `HouseAtTheEdgeOfTime_Basement` (`859897014d874bb4a9d8ad1a94d266bb`), final act | `TheEndItem` ×1 | zero registered direct refs; non-hidden good-loot capstone | Selected |
| Watch at the World's End | `5a9b9e4b884ae064fa7caa5a13eab065` / `FirstWorld_VeryGoodHiddenLoot02` | `HouseAtTheEdgeOfTime` (`13e7006bce054ce4e82b5064b2f3f8ff`), final act | `ForewarningShieldItem` ×1 | zero registered direct refs; separate deterministic hidden treasure | Selected |

Rejected examples include repeated generic `Forest_FatLoot`/`PoorLoot` palace
containers, empty/quest-book placeholders, and DLC/other-area armories. The two
Pitax and two final targets are distinct exact blueprints.

## Native enchantment contracts

The same run resolved exact installed authorities: Enhancement +1
`d42fc23b92c640846ac137dc26e000d4`, +2
`eb2faccc4c9487d43b3575d7e77ff3f5`, +4
`783d7d496da6ac44f9511011fc5f1979`, and +5
`bdba267e951851449af552aa9f9e3992`, each using exactly one native
`WeaponEnhancementBonus` and matching `EnchantmentCost`; Fey Bane
`b6948040cdb601242884744a543050d4`, cost 1, uses one
`WeaponConditionalDamageDice` and one `WeaponConditionalEnhancementBonus`;
Thundering `690e762f7704e1f4aa1ac69ef0ce6a96`, cost 1, uses one
`WeaponEnergyDamageDice` and has multiple native weapon donors. The exact
Seeking is absent. Three materially distinct guarded graph strategies found no
internal name, display name, donor, or concealment-component weapon enchantment;
final run `20260808T1744000629586Z-observe-vendor-table-contracts`, runtime ID
`20260808T1744000942305Z-9576e406512a450bbe5766283bc57d5b`. This triggers the
work order's critical required-native-property stop.

Final vendor-reference PASS
`20260808T1739377557378Z-observe-vendor-table-contracts`, runtime ID
`20260808T1739377910467Z-f45ce288a1504610a85c458eb6de1e26`, proved the
selected `SmithVendorTable` is owned only by the two exact capital blacksmith
unit blueprints above. Oleg is rejected as an early trading-post table and the
chapter-generic large tables exposed no registered direct owner. The selected
smith table is the narrowest plausible established capital arms merchant.
