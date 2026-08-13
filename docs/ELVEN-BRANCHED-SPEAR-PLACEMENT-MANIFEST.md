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
| Act II onward | Capital blacksmith | `SmithVendorTable` / `7de959347266092448d8a72089ef9778` | all six generic versions; Thornstep | one each; native restock semantics | guarded observer found direct refs from `CapitalOwlbearAttack_Blacksmith` and `VerdelBlacksmith` |
| later fallback | Dire Narlmarches village trader | `DireNarlmarchesVillageVendorTable` / `f072a8f6889b5f345b7f4e7c74cb3e4c` | all six generic versions | one each; native restock semantics | guarded observer found direct ref from `DireNarlmarchesVillageTrader` |
| Act V | Pitax specialist trader | `PitaxTownVendorTable` / `e5ab1fccf37c55f41a20a80c6ba6a460` | all six generic versions; Briar-Crowned Spear | one each; native restock semantics | guarded observer found direct ref from `PitaxTown_Trader` |

The inexpensive weapon is therefore available in Act I, generic cold iron is
available before the main fey escalation, masterwork cold iron is available no
later than the capital, and the +1 cold-iron version has two continuing
purchase paths. Vendor rows are append/normalize operations, not replacements.

## Fixed named loot

| Item | Chapter | Area/container | Exact blueprint identity | Quantity | Placement role |
|---|---|---|---|---|---|
| Boughkeeper | late Act I | `StagLordFort` / `Forest_BarrikadedChest1` | `59cb0ac65b4093440ad341b9a2f372cf` | 1 | main-path fort reward; append beside the native +1 glaive |
| Moonlit Fork | Act III | `GoblinKingFort` / `Forest_LootBoxGood2` | `70c4615a8d667dc4cb740c22ee7b5eed` | 1 | Season of Bloom main-path fort treasure; append beside Bracers of Armor +4 |
| Viper's Reach | Act IV | `VordakaiTombLevel2` / `Forest_cache` | `193b1222846a0114197e716cb35d3ce8` | 1 | main-path tomb treasure; append without replacing the existing rare-firearm placement |
| Spear of the First Branch | final act | `FinalDungeon` / `RichHuman_Loot_1` | `7e6448d1d8a7e4f4d9cc340b8f15e732` | 1 | early final-dungeon martial cache; append beside the native +5 greatsword |

The fixed rows are non-replenishing count-one additions. Thornstep and
Briar-Crowned are dependable merchant stock, so the named progression is not
entirely missable or concentrated on one vendor.

## Module and save behavior

With the module ON, the four vendor tables and four containers are normalized.
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
