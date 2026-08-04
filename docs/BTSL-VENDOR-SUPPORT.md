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

Each installed table receives exactly one of each firearm, 200 powder, 200 lead
balls, 10 Repair Kits, 5 Overhaul Kits, and one Gunsmith's Kit. Native entries
and ordering are retained; complete publication is idempotent and partial or
duplicate publication fails closed.

Guarded table observation establishes blueprint publication only. Actual shop
visibility and purchase remain on the consolidated human checklist.
