# Rare Firearms Acquisition Inventory

Guarded read-only baseline run
`20260808T1720275373614Z-observe-vendor-table-contracts` (runtime ID
`20260808T1720275529421Z-c5dbf9e887b64c1b89ac129ba490d2b3`) passed on
`1c570bd4211d69c5c29f6af46a870146adb1645b`, 0.0.73. It enumerated 43
`BlueprintSharedVendorTable` objects and 26 associations without mutation.

| GUID | Runtime type | Exact name / contents | Ownership / references | Disposition |
|---|---|---|---|---|
| afa2c7f292b8e1c4d9c835f0e8047dd3 | BlueprintSharedVendorTable | C11_JhodVendorTable; 61 fixed entries after current ten-entry publication | Base campaign; shared by Capital_Jhod and many priest/Jhod variants | Reject future firearm stock |
| f720440559fc00949900bfa1575196ac | BlueprintSharedVendorTable | `C11_OlegVendorTable`; 97 fixed entries after current publication, including Repair Kit x5 and Overhaul Kit x2 | Base campaign; exact owners `OTP_Oleg` (`5db389e0409ef534d81358555e6ab99d`) and `OTP_Oleg_FirstVisit` (`67db4b8bacc69e643880f0a4ed6dff6f`), one direct reference each | Selected only for bounded early maintenance/eastern mundane stock; reject named magic firearm placement |
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

Issue 5 guarded PASS `20260820T0535269245782Z-7edf2d2c158a4085893931e91b14db1d`
supersedes the pending Oleg ownership note above: the exact live table contained
one Repair Kit row at 5 and one Overhaul Kit row at 2 and only the two exact
direct owners recorded in the inventory. Publication affects the static shared
table for future/unmaterialized inventory. No already-materialized old-save
merchant refresh or player-inventory mutation is claimed.

## Fixed campaign selections from live graph

Guarded PASS `20260808T1734599486274Z-observe-vendor-table-contracts`, runtime
ID `20260808T1734599848444Z-0502d6b37e1a4049a522927f30c23f3a`, enumerated
437 bounded candidates. Each selection is `BlueprintLoot`, fixed `LootEntry[]`,
base-campaign area-owned, and has zero direct references from another registered
blueprint in the exposed graph (the exact area field is the ownership evidence).

| Item | Target GUID / exact name | Area / chapter | Existing fixed contents | References / nature | Disposition |
|---|---|---|---|---|---|
| Duelist's Rebuttal | `1f0bef6b8e540d644962171dc8810459` / `Forest_Container_7_good` | `VarnholdStockade`, Act 4 | Crimson Counselor, antique cyclops boot, food | zero registered direct refs; ordinary fixed stockade loot | Selected |
| The River King's Measure | `b34367a637010f743815aed5875152bd` / `PoorHuman_IrovettiChambers_ChestHuge_Outline (3)` | `IrovettiPalace` (`bf9dbc2998849ee40bbdba9cb40a7d4c`), Pitax | fixed gems/valuables (full record in run) | zero registered direct refs; royal-chambers fixed chest | Selected |
| Irovetti's Ovation | `aeba7802ade083841935daf88d4652d3` / `RichHuman_GoodLoot` | `IrovettiPalaceFW`, Pitax | Calistria Rapier | zero registered direct refs; ordinary fixed First World palace loot | Selected |
| The Last Word (Pistol) | `3bc451b100283774a9e23699dd869f1a` / `FirstWorld_GoodLoot_Locked_2` | `CastleOfKnives`, final act | Greater Empower metamagic rod | zero registered direct refs; independent fixed capstone cache | Selected |
| Watch at the World's End (Musket) | `5a9b9e4b884ae064fa7caa5a13eab065` / `FirstWorld_VeryGoodHiddenLoot02` | `HouseAtTheEdgeOfTime` (`13e7006bce054ce4e82b5064b2f3f8ff`), final act | `ForewarningShieldItem` ×1 | zero registered direct refs; separate deterministic hidden treasure | Selected |

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
original work order's critical required-native-property stop. The 2026-08-08
user authority amendment accepts this absence conclusively, resolves that stop,
forbids further native-Seeking searches, and authorizes project-owned Seeking
`036fc59fd1e24753b98f9d92cdb1e93e`. Acquisition selections are unchanged.

The accepted component record further resolves Thundering's only configurable
field as `Element=Sonic`, with no critical predicate. Its donors include
`DartSonicShockPlus2`, `SlingSonicPlus2`, and named Thundering weapons. After
two independent guarded scatter fixture strategies could not produce a safe
completed native roll for the enchanted blunderbuss, the mission's authorized
fallback was selected: Ovation is +4 Reliable, effective +5, cost 52,300 gp.

Final vendor-reference PASS
`20260808T1739377557378Z-observe-vendor-table-contracts`, runtime ID
`20260808T1739377910467Z-f45ce288a1504610a85c458eb6de1e26`, proved the
selected `SmithVendorTable` is owned only by the two exact capital blacksmith
unit blueprints above. Oleg is rejected as an early trading-post table and the
chapter-generic large tables exposed no registered direct owner. The selected
smith table is the narrowest plausible established capital arms merchant.

## Issue 12 cross-system audit

All five rare-firearm targets are exact and distinct. Duelist's Rebuttal, Irovetti's Ovation, and The Last Word moved to ordinary fixed targets; their retired rows are removed only when they reference project-owned firearms. Production family truth is The Last Word = Pistol and Watch at the World's End = Musket. The complete 30-item inventory is `planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md`.

Guarded runtime `20260820T0855347791407Z-observe-rare-firearm-acquisition` passed with five valid rare-firearm targets and the global 30-item distinct-target assertion.
