# Overnight Gunslinger Bug-Fix Matrix

Baseline: d13268d3abe9ffe89c8195b213c1eee194328672, version 0.0.87,
1,150 deterministic tests. Human findings dated 2026-08-19 override conflicting
older automation.

| ID | Human-observed failure | Current diagnosis | Files/systems touched | Focused tests | Full gates | Runtime scenario/run ID | Commit SHA | Remote SHA | Status | Remaining human check | Next action |
|---:|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Acadamae acceleration/save/fatigue not visible/reliable | Prior 0.0.76 structural/runtime evidence is stale; live cast boundary must be re-established | pending | pending | pending | pending | pending | pending | pending | action/save/fatigue/Cord | Inspect current Acadamae implementation and live callback chain |
| 2 | Focused Aim damage appears without Grit loss | Prior fixed-cost claim lacks current player-path proof | pending | pending | pending | pending | pending | pending | pending | visible resource and True Grit | Inspect Focused Aim activation/spend transaction |
| 3 | Firearms may use normal AC; range branch is unclear | Touch-AC service exists but current live resolution/presentation requires requalification | pending | pending | pending | pending | pending | pending | pending | inside/outside feedback | Inspect definitions, event patch, UI seams, and current tests |
| 4 | Acadamae prerequisites displayed twice | Likely manual prose plus native prerequisite renderer | pending | pending | pending | n/a | pending | pending | pending | one prerequisite presentation | Inspect localization and feat blueprint presentation |
| 5 | Oleg lacks maintenance kits | Exact Oleg table is known; current publication roster excludes requested kits | pending | pending | pending | pending | pending | pending | pending | merchant materialization | Inspect existing vendor transaction/publication |
| 6 | Bokken lacks ammunition | Prior graph investigation deferred exact table; current mission reopens it | pending | pending | pending | pending | pending | pending | pending | live inventory if resolvable | Re-run bounded exact-table forensics with current graph |
| 7 | Border Sentinel appears too early at Oleg | Exact item/current references and later fixed target require inventory | pending | pending | pending | pending | pending | pending | pending | new organic location | Audit item and every publication reference |
| 8 | Pistol and possibly other firearm sounds absent | Qualified Wwise path exists; integrated regression boundary unknown | pending | pending | pending | pending | pending | pending | pending | audible five-family/no-crossbow matrix | Audit staging/load/post/package/deployment paths |
| 9 | Firearm feat icons are generic/inconsistent | Current shared generic icon policy and Rapid Reload asset need exact audit | pending | pending | pending | pending | pending | pending | pending | aesthetic UI scale | Inspect selector icon publication and source assets |
| 10 | Elven Branched Spear too long/not held in attacks | Current 2.925m source bounds and accepted prior visual claim conflict with fresh human evidence | pending | pending | pending | pending | pending | pending | pending | full doll/animation matrix | Inspect spear prefab/builder/donor contracts |
| 11 | Musket/Blunderbuss clip and need texture/rig repair | Prior semantic-anchor candidates remain human-failed | pending | pending | pending | pending | pending | pending | pending | full long-gun visual matrix | Inspect current rig specs, bundle inputs, calibration |
| 12 | Project magic items are clustered | Existing acquisition publication needs complete project-wide inventory | pending | pending | pending | pending | pending | pending | pending | every fixed location | Audit all project-owned unique magic acquisition |
