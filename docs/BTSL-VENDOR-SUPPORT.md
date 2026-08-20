# Beneath the Stolen Lands vendor support

Version 0.0.66 publishes native `LootItemsPackFixed` entries to four exact,
runtime-observed tables. Missing DLC tables are optional and log
`SKIPPED_OPTIONAL_TABLE_ABSENT` without failing base-game bootstrap.

| Path | Vendor table | GUID |
|---|---|---|
| Standalone | `RogueLike_NPCVendorTable` | `a6bae621a7bd96b4fb3c1511cd2f9fac` |
| Standalone | `RogueLike_DragonVendorTable` | `08e090bb2038e3d47be56d8752d5dcaf` |
| Campaign | `DLC3_VendorFirstTable` | `45f027c06962df249b8c014a4b4e95e3` |
| Campaign | `DLC3_VendorSecondTable` | `420f1da6c2523f64eba810b9b484f60f` |

Each Honest Guy table receives one mundane and one +1 Pistol, Musket, and
Blunderbuss plus module-enabled permanent Eastern weapons and Elven Branched
Spears. Each Xelliren table receives 200 Black Powder, 200 Lead Balls, 200 Paper
Cartridges, 10 Repair Kits, 5 Overhaul Kits, and one Gunsmith's Kit. Wrong-owner
project rows are removed by exact item reference. Native and foreign rows retain
their relative order while project rows are inserted by stable item type/name;
publication is idempotent and rollback restores the exact original arrays.

The split applies to static blueprint tables and therefore to new or
not-yet-materialized merchant stock. Already materialized old-save inventories
are not broadly mutated or claimed to refresh.

Guarded table observation establishes blueprint publication only. Actual shop
visibility and purchase remain on the consolidated human checklist.
