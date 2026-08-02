# Sprint 61 later equipment acquisition qualification

Sprint 61 publishes the player-fireable Gunslinger equipment catalog through
the exact installed capital Jhod shared-vendor table
`afa2c7f292b8e1c4d9c835f0e8047dd3`. The route requires no shop invocation,
save load, or dependency on another gameplay mod.

## Published stock

- Pistol, Musket, Advanced Rifle, and Advanced Revolver: one each.
- Black Powder Charge, Lead Ball, and Firearm Repair Kit: 99 each.
- Blunderbuss: deliberately excluded until scatter delivery is qualified.

The quantities follow observed Kingmaker 2.1.7b vendor conventions. Across all
43 shared tables, count one is the dominant non-stackable quantity (498 native
entries). The target capital table uses count 99 for each of its 50 ordinary
stackable entries; Diamond Dust alone uses 2000. Existing item costs, weights,
stackability, firearm identity, and native table order are unchanged.

## Safety and rollback

`CapitalVendorBlueprints` appends seven native `LootItemsPackFixed` components
after all 51 native components. Publication fails closed on a null contract,
duplicate project item, or partial prior publication. Re-entry with the complete
seven-item publication is idempotent. Bootstrap failure rolls back only when
the complete published reference sequence is still exact; intervening mutation
causes rollback refusal rather than destructive replacement.

The reusable reference-identity transaction has four focused tests covering
append order, idempotence, ambiguous duplicate/partial state, and single-use
rollback. The complete dependency-free suite passes 831/831, along with
repository validation, clean exact-reference Release build, strict package
validation, 38 runtime-request checks, and 80 preflight checks.

## Runtime evidence

Exact production commit `c2fd27b` passed guarded mod load at
`20260802T0931384431105Z-mod-load-smoke`. Two independent fresh-process,
save-free observations passed at:

- `20260802T0932570144693Z-observe-vendor-table-contracts`
- `20260802T0934221001134Z-observe-vendor-table-contracts`

Both observed 58 exact table components, seven project entries, zero invalid
counts, zero Blunderbuss entries, and the installed fixed-entry/reference
contract. No shop, inventory, vendor refresh, save, or save-writing API was
invoked. Package SHA-256 is
`6bbd63bf662197b540684b09cd4a84eebe275eb210ad86804fdc736a6d9f0819`;
DLL SHA-256 is
`e31962312cc583eb83cdb5e27645816f90c1acf409360243adfa687654288169`.

Sprint 61 is a checkpoint, not a mission stopping condition.
