# Eastern Weapons placement manifest

## Live qualification

Historical pre-Issue-7 guarded read-only vendor/loot run
`20260814T1531432806171Z-d6638ced8af7472fabeb9b65f2c233c7`
resolved all eight installed vendor tables by exact GUID/name and observed 97
singular Eastern rows: 49 base-campaign merchant rows plus 48 generic BTSL
rows. The four BTSL tables contained no named Eastern item and retained all
expected firearm and Elven Branched Spear rows. Four exact fixed-loot targets
contained eleven count-one named rows; together with seven named merchant rows,
all eighteen named weapons were placed exactly once. The fresh human finding
that Oleg was too early superseded only Border Sentinel's placement; the old
run remains historical evidence rather than current acceptance.

Fresh-process run
`20260814T1534024888613Z-29a2ce31f1db4aa7bedec9c2c14e6047`
with only Eastern Weapons disabled observed zero Eastern merchant, BTSL, and
fixed-loot rows while preserving the other module rows. Both compatibility
transactions restored their exact pre-run Mods trees; neither accessed a save.

This manifest records the exact installed campaign contracts selected for the
default-on `eastern-weapons` module. Every addition is count one, additive,
idempotent, and rollback-owned. Publication removes only stale rows owned by
this feature, preserves all native, firearm, spear, and foreign-mod rows, and
restores the exact pre-publication arrays if no later foreign mutation occurs.

## Generic and dependable merchant stock

| Band | Vendor/table | Exact identity | Eastern stock | Lifecycle and reachability evidence |
|---|---|---|---|---|
| Act I | Oleg | `C11_OlegVendorTable` / `f720440559fc00949900bfa1575196ac` | mundane and masterwork Wakizashi, Katana, and Nodachi | one fixed row each under native restock semantics; direct refs from `OTP_Oleg` and `OTP_Oleg_FirstVisit`; no named Eastern weapon remains in this table |
| Act II onward | Capital blacksmith | `SmithVendorTable` / `7de959347266092448d8a72089ef9778` | all 12 generic items; Quiet Current, Winter Reed, Cloud-Cleaver | one fixed row each under the recurring capital merchant lifecycle; direct refs from `CapitalOwlbearAttack_Blacksmith` and `VerdelBlacksmith` |
| later replacement | Dire Narlmarches village trader | `DireNarlmarchesVillageVendorTable` / `f072a8f6889b5f345b7f4e7c74cb3e4c` | all 12 generic items | one fixed row each under native restock semantics; direct ref from `DireNarlmarchesVillageTrader` |
| Act V | Pitax specialist | `PitaxTownVendorTable` / `e5ab1fccf37c55f41a20a80c6ba6a460` | all 12 generic items; Empty Sleeve, Moonlit Crossing, Unfixed Form | one fixed row each under native restock semantics; direct ref from `PitaxTown_Trader` |

The four required tables contain 48 feature-owned rows when the module is on:
42 generic rows and six named rows. Tier-two and tier-five progression is
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
| late Act I | Paper Lantern | Wayfarer's Oath | Border Sentinel | first two append to `Forest_BarrikadedChest1` / `59cb0ac65b4093440ad341b9a2f372cf`; Border Sentinel appends separately to fixed `PoorHuman_treasure_chest_03` / `c8b8159fb695be64883b609a7e77e75d`; both exact targets are in `StagLordFort` |
| Act II | Quiet Current | Winter Reed | Cloud-Cleaver | recurring capital blacksmith stock |
| Act III | Falling Petal | Drawn Horizon | none | distinct Silverstep Grotto fixed chests `5b8346d4fc947624e9f8728fe7a12535` and `040bad335c144784798a580e41b5410f` |
| Act IV | Storm Over Stone / Duelist's Rebuttal | Foxfire Whisper / Viper's Reach | Thunder at the Gate / Mountain-Sunder | paired fixed targets across Varnhold, Vordakai's Tomb, and Armag's Tomb; see `docs/GUNSLINGER-ACQUISITION-REBALANCE.md` |
| Act V | Empty Sleeve | Moonlit Crossing | Unfixed Form | recurring Pitax specialist stock |
| late game | Night Without Moon | Heaven's Measure | World-Tree Severer | `RichHuman_Loot_1` / `7e6448d1d8a7e4f4d9cc340b8f15e732` in `FinalDungeon` |

The five fixed containers receive 12 named count-one rows. They are important
main-path fort, tomb, and final-dungeon caches already accepted by the spear
qualification. Nothing is hidden behind a new Perception-only route, and every
family has its own uninterrupted six-item named path.

Border Sentinel is the stable `KMG.EasternWeapons.Nodachi.BorderSentinel`
blueprint (`c1c7a6746916504ebfdcb2b650a7145b`), a +1 cold-iron nodachi priced at
4,420 gp. The selected chest is a fixed base-campaign `BlueprintLoot` with zero
registered direct references, native horseshoe/gold contents, and exact
`StagLordFort` area ownership. Publication preserves those entries and appends
Border Sentinel once. It removes the project item only from future static Oleg
publication; it never removes a copy already owned, sold, dropped, stashed, or
materialized in an existing save. New campaigns and not-yet-instantiated static
loot/vendor state receive the changed blueprint contract; no old-save merchant
or opened-container refresh is claimed.

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
passed with all Eastern commerce and loot publication absent while all 46
persistent identities remained registered. Original settings bytes restored
exactly to SHA-256
`2e53fa0a09c56662434f6ea548ff5ebcf91f5aaf293d668248221239a1308655`.

The dedicated vendor/loot observer and merchant reopen/refresh lifecycle
exercise remain part of final runtime qualification; this checkpoint proves
exact blueprint-array publication, module gating, and transaction restoration.

## Issue 7 current qualification

Save-free guarded run
`20260820T0613577259438Z-865e96f7deb44004bda7cef62f0511bb`
passed 24/24 assertions on immutable commit
`3e52dfdac3d86eddabc2e8fa94024b96ced0241b`. The live installed graph reported
zero Border Sentinel rows in Oleg, zero rows across all shared vendors, and one
row across all registered `BlueprintLoot` objects. That row was count one in
exact target `PoorHuman_treasure_chest_03` /
`c8b8159fb695be64883b609a7e77e75d`, exact area `StagLordFort`. The complete
Eastern publication was 48 base-campaign merchant rows, 48 BTSL rows, and 12
fixed-loot rows across five targets, with zero unexpected Eastern rows.

The development panel's `Print Border Sentinel location audit` action reports
the exact item/target/area identities, count-one match, current-area match, and
complete target contents without opening, moving, granting, teleporting, or
saving anything. Physical chest materialization and discoverability remain a
human check on an unopened/new-campaign target.

## Issue 12 distribution supersession

All eighteen named Eastern weapons now use eighteen distinct fixed base-campaign loot targets. Named Eastern merchant rows were removed; generic and masterwork Eastern merchant stock is unchanged. Old Stag Lord, Goblin, Vordakai, and Final Dungeon clusters are normalized by exact project-owned reference only. The authoritative item GUID, target, act, native contents, and route are in `planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md`.

Guarded runtime `20260820T0855347791407Z-observe-rare-firearm-acquisition` passed with every named Eastern item at count one on its intended target and zero vendor/stale rows.
