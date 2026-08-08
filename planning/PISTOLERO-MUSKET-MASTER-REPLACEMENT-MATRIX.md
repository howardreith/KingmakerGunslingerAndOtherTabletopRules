# Pistolero and Musket Master Replacement Matrix

This matrix is fixed before implementation. Blueprint symbols and exact runtime
references will be appended after inventory, without changing the rules below.

## Pistolero

| Level | Remove exact base feature(s) | Add exact feature(s) | Preserve |
|---|---|---|---|
| 1 | Gunslinger Proficiencies; Deadeye; base level-1 deed summary | Pistolero Proficiencies; Up Close and Deadly; Pistolero level-1 deed summary | Gunsmithing, Grit, Gunslinger's Dodge, Quick Clear, exact Pistol starter |
| 5 | Gun Training selection | Pistol Training rank 1 | Other level-5 entries |
| 7 | Startling Shot; base level-7 deed summary | Existing Deadeye identity; Pistolero level-7 deed summary | Dead Shot and all Targeting modes |
| 9 | Gun Training selection | Pistol Training rank 2 | Other level-9 entries |
| 11 | Bleeding Wound; base level-11 deed summary | Twin Shot Knockdown; Pistolero level-11 deed summary | Expert Loading, Lightning Reload |
| 13 | Gun Training selection | Pistol Training rank 3 | Other level-13 entries |
| 17 | Gun Training selection | Pistol Training rank 4 | Other level-17 entries |

Starter contract: native base starting items remain unchanged; generalized
observer expects exactly the newly granted production Pistol, no new production
Musket, exact powder/ball native grants topped to 20/20, one gunsmith kit, and
owner-bound battered semantics on that exact Pistol.

## Musket Master

| Level | Remove exact base feature(s) | Add exact feature(s) | Preserve |
|---|---|---|---|
| 1 | Gunslinger Proficiencies; Gunslinger's Dodge; base level-1 deed summary | Musket Master Proficiencies; Steady Aim; exact Rapid Reload (Musket); Musket Master level-1 deed summary | Deadeye, Quick Clear, Gunsmithing, Grit |
| 3 | Utility Shot | Fast Musket | Gunslinger Initiative, Pistol-Whip |
| 5 | Gun Training selection | Musket Training rank 1 | Other level-5 entries |
| 9 | Gun Training selection | Musket Training rank 2 | Other level-9 entries |
| 13 | Gun Training selection | Musket Training rank 3 | Other level-13 entries |
| 17 | Gun Training selection | Musket Training rank 4 | Other level-17 entries |

Native archetype contract: `ReplaceStartingEquipment = true`; `StartingItems`
is exactly production Musket, black powder, lead ball, gunsmith kit in that
order. It contains no Pistol, Test Musket, Heavy Crossbow donor, duplicate
firearm, or unrelated item. The observer expects exactly the newly granted
production Musket and no new production Pistol, tops exact powder/ball stacks
to 20/20, and owner-binds that exact Musket without later duplication.

## Catalog and mutual exclusion

Append project archetypes deterministically as Mysterious Stranger, Pistolero,
Musket Master, each exactly once by reference and identity, while preserving
all unrelated current entries. Overlapping replacement rows make the three
project archetypes mutually exclusive; archetype stacking is not implemented.
