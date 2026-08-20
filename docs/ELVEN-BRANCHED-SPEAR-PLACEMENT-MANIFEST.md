# Elven Branched Spear placement manifest

This manifest records the exact base-game blueprint contracts used by the
default-on `elven-branched-spears` module. Publication appends one count-one
fixed entry per listed item after removing only stale entries owned by this
feature. It never replaces unrelated vendor or container contents. Repeated
cache initialization normalizes to one entry, and rollback restores the exact
pre-publication arrays if no foreign mutation intervened.

## Vendor stock

| Chapter | Vendor/table | Exact identity | Items | Quantity and replenishment | Reachability evidence |
|---|---|---|---|---|---|
| Act I | Oleg | `C11_OlegVendorTable` / `f720440559fc00949900bfa1575196ac` | mundane, masterwork, cold iron, masterwork cold iron | one fixed vendor-table entry each; native merchant restock semantics | guarded `observe-vendor-table-contracts` found direct refs from `OTP_Oleg` and `OTP_Oleg_FirstVisit` |
| Act II onward | Capital blacksmith | `SmithVendorTable` / `7de959347266092448d8a72089ef9778` | all six generic versions | one each; native restock semantics | guarded observer found direct refs from `CapitalOwlbearAttack_Blacksmith` and `VerdelBlacksmith` |
| later fallback | Dire Narlmarches village trader | `DireNarlmarchesVillageVendorTable` / `f072a8f6889b5f345b7f4e7c74cb3e4c` | all six generic versions | one each; native restock semantics | guarded observer found direct ref from `DireNarlmarchesVillageTrader` |
| Act V | Pitax specialist trader | `PitaxTownVendorTable` / `e5ab1fccf37c55f41a20a80c6ba6a460` | all six generic versions | one each; native restock semantics | guarded observer found direct ref from `PitaxTown_Trader` |
| BTSL standalone | Honest Guy weapons merchant | `RogueLike_NPCVendorTable` / `a6bae621a7bd96b4fb3c1511cd2f9fac` | all six generic versions | one fixed catalog entry each; native standalone vendor refresh semantics | installed table and `DungeonVendorItemsComponent` verified; guarded observer found six singular spear rows |
| BTSL standalone | Xelliren/Dragon support merchant | `RogueLike_DragonVendorTable` / `08e090bb2038e3d47be56d8752d5dcaf` | none | exact project-owned stale spear cleanup; unrelated rows retained | installed table and `DungeonVendorItemsComponent` verified |
| BTSL campaign | Honest Guy weapons merchant | `DLC3_VendorFirstTable` / `45f027c06962df249b8c014a4b4e95e3` | all six generic versions | one fixed catalog entry each; native campaign vendor refresh semantics | exact installed campaign table verified; guarded observer found six singular spear rows |
| BTSL campaign | Xelliren support merchant | `DLC3_VendorSecondTable` / `420f1da6c2523f64eba810b9b484f60f` | none | exact project-owned stale spear cleanup; unrelated rows retained | exact installed campaign table verified |

The inexpensive weapon is therefore available in Act I, generic cold iron is
available before the main fey escalation, masterwork cold iron is available no
later than the capital, and the +1 cold-iron version has two continuing
purchase paths. Vendor rows are append/normalize operations, not replacements.

The BTSL tables expose fixed catalog rows rather than per-item depth fields, so
publication follows the installed merchant architecture instead of inventing a
custom tier system. The rows use each item blueprint's normal price. No generic
+2 spear exists, and no campaign-unique named spear is added to ordinary BTSL
stock. If the DLC tables are absent, they are skipped without failing the base
campaign; when installed, each table is validated by exact name and GUID.

## Fixed named loot

| Item | Chapter | Area/container | Exact blueprint identity | Quantity | Placement role |
|---|---|---|---|---|---|
| Boughkeeper | late Act I | `StagLordFort` / `Forest_BarrikadedChest1` | `59cb0ac65b4093440ad341b9a2f372cf` | 1 | main-path fort reward; append beside the native +1 glaive |
| Moonlit Fork | Act II | `CandlemereTower` / `Forest_Loot01` | `8a07f25d4083eb84c943bf95684f8e16` | 1 | fixed tower loot beside Full Plate +2 |
| Viper's Reach | Act IV | `VordakaiTombLevel2` / `Forest_cache_1561` | `53d54ca50fccb8c4d9242904eba04d14` | 1 | exact fixed tomb cache; no other project unique in the object |
| Spear of the First Branch | final act | `FinalDungeon` / `RichHuman_Loot_1` | `7e6448d1d8a7e4f4d9cc340b8f15e732` | 1 | early final-dungeon martial cache; append beside the native +5 greatsword |

The fixed rows are non-replenishing count-one additions. Thornstep and
Briar-Crowned are fixed loot, so the named progression is not
entirely missable or concentrated on one vendor.

## Module and save behavior

With the module ON, the four campaign vendor tables, both installed Honest Guy
tables, both Xelliren stale-row cleanup targets, and six containers are normalized.
With the module OFF, no vendor or fixed-loot publication occurs. All item and
effect blueprints remain registered under every module profile, so an existing
owner, selected feat, or saved item identity remains valid. Module settings are
immutable during a process; a change takes effect after restart.

## Evidence

The exact targets came from save-free guarded runtime observers. The expanded
campaign inventory passed at
`runtime-evidence/20260813T2305410957646Z-observe-elven-branched-spear-contracts`.
Vendor identities came from
`runtime-evidence/20260808T1739377557378Z-observe-vendor-table-contracts`.
These observations enumerate installed blueprint references and do not select,
load, or mutate a save.

The first-playtest BTSL publication passed guarded run
`runtime-evidence/20260814T0454174378820Z-observe-vendor-table-contracts`:
all four exact BTSL tables resolved, all 24 expected spear rows were singular,
all native rows remained, and the existing 48 firearm rows remained. Focused
module-ON and module-OFF runs
`20260814T0457017452463Z-observe-feature-module-settings` and
`20260814T0459247497589Z-observe-feature-module-settings` passed and restored
the settings file byte-for-byte (SHA-256
`dc76b429302838c52895d1901ac7488bc58e9d18a01b8e584968497cdb30c50c`).

## Issue 12 distribution supersession

The six named branched spears now use six distinct fixed base-campaign loot targets. Thornstep and Briar-Crowned Spear were removed from recurring merchant stock; generic branched-spear commerce and BTSL publication are unchanged. Old Goblin, Vordakai, and Final Dungeon clustered rows are removed by exact project-owned reference only. The authoritative inventory is `planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md`.

Guarded runtime `20260820T0855347791407Z-observe-rare-firearm-acquisition` passed with all six named spears at one intended target and zero vendor/stale rows.
