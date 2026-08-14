# Eastern Weapons placement manifest

This manifest records the exact installed campaign contracts selected for the
default-on `eastern-weapons` module. Every addition is count one, additive,
idempotent, and rollback-owned. Publication removes only stale rows owned by
this feature, preserves all native, firearm, spear, and foreign-mod rows, and
restores the exact pre-publication arrays if no later foreign mutation occurs.

## Generic and dependable merchant stock

| Band | Vendor/table | Exact identity | Eastern stock | Lifecycle and reachability evidence |
|---|---|---|---|---|
| Act I | Oleg | `C11_OlegVendorTable` / `f720440559fc00949900bfa1575196ac` | mundane and masterwork Wakizashi, Katana, and Nodachi; Border Sentinel | one fixed row each under native restock semantics; direct refs from `OTP_Oleg` and `OTP_Oleg_FirstVisit`; Border Sentinel is therefore guaranteed no later than Act I |
| Act II onward | Capital blacksmith | `SmithVendorTable` / `7de959347266092448d8a72089ef9778` | all 12 generic items; Quiet Current, Winter Reed, Cloud-Cleaver | one fixed row each under the recurring capital merchant lifecycle; direct refs from `CapitalOwlbearAttack_Blacksmith` and `VerdelBlacksmith` |
| later replacement | Dire Narlmarches village trader | `DireNarlmarchesVillageVendorTable` / `f072a8f6889b5f345b7f4e7c74cb3e4c` | all 12 generic items | one fixed row each under native restock semantics; direct ref from `DireNarlmarchesVillageTrader` |
| Act V | Pitax specialist | `PitaxTownVendorTable` / `e5ab1fccf37c55f41a20a80c6ba6a460` | all 12 generic items; Empty Sleeve, Moonlit Crossing, Unfixed Form | one fixed row each under native restock semantics; direct ref from `PitaxTown_Trader` |

The four required tables contain 49 feature-owned rows when the module is on:
42 generic rows and seven named rows. Tier-two and tier-five progression is
dependable for every family. Ordinary item pricing remains authoritative, so
publication does not grant or discount an item outside the documented balance
formula.

## Beneath the Stolen Lands

| Mode | Table | Exact GUID | Rows |
|---|---|---|---|
| standalone | `RogueLike_NPCVendorTable` | `a6bae621a7bd96b4fb3c1511cd2f9fac` | all 12 generic items, once each |
| standalone | `RogueLike_DragonVendorTable` | `08e090bb2038e3d47be56d8752d5dcaf` | all 12 generic items, once each |
| campaign | `DLC3_VendorFirstTable` | `45f027c06962df249b8c014a4b4e95e3` | all 12 generic items, once each |
| campaign | `DLC3_VendorSecondTable` | `420f1da6c2523f64eba810b9b484f60f` | all 12 generic items, once each |

When all installed tables are present this is exactly 48 singular rows. DLC
absence is a safe skip. Installed tables are validated by exact GUID and name.
No named Eastern weapon is placed in ordinary BTSL stock.

## Named progression

| Band | Wakizashi | Katana | Nodachi | Source |
|---|---|---|---|---|
| late Act I | Paper Lantern | Wayfarer's Oath | Border Sentinel | first two append to `Forest_BarrikadedChest1` / `59cb0ac65b4093440ad341b9a2f372cf` in `StagLordFort`; Border Sentinel is guaranteed Oleg stock |
| Act II | Quiet Current | Winter Reed | Cloud-Cleaver | recurring capital blacksmith stock |
| Act III | Falling Petal | Drawn Horizon | Storm Over Stone | `Forest_LootBoxGood2` / `70c4615a8d667dc4cb740c22ee7b5eed` in `GoblinKingFort` |
| Act IV | Foxfire Whisper | Thunder at the Gate | Mountain-Sunder | `Forest_cache` / `193b1222846a0114197e716cb35d3ce8` in `VordakaiTombLevel2` |
| Act V | Empty Sleeve | Moonlit Crossing | Unfixed Form | recurring Pitax specialist stock |
| late game | Night Without Moon | Heaven's Measure | World-Tree Severer | `RichHuman_Loot_1` / `7e6448d1d8a7e4f4d9cc340b8f15e732` in `FinalDungeon` |

The four fixed containers receive 11 named count-one rows. They are important
main-path fort, tomb, and final-dungeon caches already accepted by the spear
qualification. Nothing is hidden behind a new Perception-only route, and every
family has its own uninterrupted six-item named path.

## Module, lifecycle, and rollback behavior

All 30 item blueprints and three stable categories register in every module
state. Module OFF suppresses all Eastern vendor and loot rows while leaving
existing saved items coherent. Module ON normalizes exact count-one rows on
initial blueprint publication; reopening or refreshing a merchant consumes
the same shared vendor table and cannot multiply the rows. Repeated cache
initialization recognizes an already exact catalog. Rollback refuses if a
foreign writer changes an array after publication.

## Source evidence and qualification state

The save-free guarded `observe-eastern-weapon-contracts` run
`20260814T1119161920060Z-d07fac81ae644db0ac092e1fa3cfa3fe`
recorded the installed vendor GUID/name pairs, four accepted fixed-loot
GUID/name/area triples, table shapes, and campaign reachability. The spear
placement observer and qualification established the direct merchant owners
and main-path semantics quoted above. No save was selected or written.

Production publication and source/runtime cardinality assertions are now
implemented. The module-enabled fresh-process observation
`20260814T1343180894067Z-9c6e5326e6fa4ee8a6f0761a7cd2af78` passed with
exactly 49 base-campaign rows, 48 rows across the four installed BTSL tables,
seven named merchant rows, and 11 fixed-loot rows. The transactional
module-disabled observation
`20260814T1349013224092Z-26cb873bd080433ebe1bd5f3658f3061`
passed with all Eastern commerce and loot publication absent while all 44
persistent identities remained registered. Original settings bytes restored
exactly to SHA-256
`2e53fa0a09c56662434f6ea548ff5ebcf91f5aaf293d668248221239a1308655`.

The dedicated vendor/loot observer and merchant reopen/refresh lifecycle
exercise remain part of final runtime qualification; this checkpoint proves
exact blueprint-array publication, module gating, and transaction restoration.
