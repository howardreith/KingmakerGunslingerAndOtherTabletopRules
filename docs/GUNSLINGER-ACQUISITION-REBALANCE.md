# Gunslinger Acquisition Rebalance

## Superseded conclusion

The `0.0.88` graph proved 30 project items on 30 exact loot blueprints. It did
not prove normal loot interaction, independent accessibility, theme, power fit,
or organic campaign pacing. The former pacing acceptance is superseded.

## Intake density requiring correction

| Named area or sequence | Project items |
|---|---:|
| Stag Lord Fort | 3 |
| Capital complex | 3 |
| Lonely Barrow | 2 |
| Big Narlmarches | 2 |
| Vordakai's Tomb | 6 |
| Pitax Town/Horde | 3 |
| Irovetti Palace | 3 |
| Castle of Knives/House/final sequence | 6 |

Default target density is at most two project uniques per named major area and
preferably one, with at most one per floor/subarea absent strong theme evidence.
Every route must be fixed, deterministic, base-campaign, normally lootable, and
independent of random tables, artisan rewards, DLC, broad hooks, dialogue-only
grants, quest consumption, or unproven puzzle wrappers.

## Mandatory rechecks

Reprove Quiet Current backpack uniqueness/accessibility; replace the weak
Briar-Crowned Spear rusty-horseshoe cache absent compelling evidence; reconsider
Duelist's Rebuttal and the Vordakai cluster; prove Irovetti's Ovation independent
lootability; replace The Last Word target if coupled to `The End`; and reduce the
Stag Lord, Vordakai, and Irovetti concentrations. Preserve strong thematic
placements only when exact evidence supports them.

Production firearm-family truth is `The Last Word = Pistol` and
`Watch at the World's End = Musket`. Stale Revolver/Rifle labels in inventory,
observers, fixtures, reports, and guides must be corrected without changing the
production items or adding deliberate advanced-firearm acquisition.

## Implemented 2026-08-20 correction

The expanded read-only census identified 1,177 exact candidate loot blueprints.
Every selected replacement is base-campaign fixed `BlueprintLoot`, has zero
registered direct references, has no quest/puzzle component, and exposes
ordinary native contents. Publication preserves native and foreign rows by
reference/order, adds the desired row once at count one, removes project-owned
rows only from explicitly retired targets, and restores exact arrays on
rollback. Materialized/opened containers and player-owned items are untouched.

| Act | Item | Exact target | Area |
|---|---|---|---|
| I | Paper Lantern | `59cb0ac65b4093440ad341b9a2f372cf` | Stag Lord Fort |
| I | Border Sentinel | `c8b8159fb695be64883b609a7e77e75d` | Stag Lord Fort |
| II | Wayfarer's Oath | `020246502ff864f4aab19e2fc00e63ee` | Troll Lair exterior |
| II | Quiet Current | `6abcbbc0a161aa54380808655de92197` | Troll Lair second level |
| II | Winter Reed | `27b9b282c32996842bde77e360b72107` | Shrine of Lamashtu |
| II | Cloud-Cleaver | `2bffac36ed3499f4f9a1e6456e96a0f6` | Candlemere Tower |
| II | Boughkeeper | `19c1920cf93076249b5c4f29488851f9` | Big Narlmarches |
| II | Thornstep | `364711342543d814eb95aa98a4c65e58` | Lonely Barrow |
| II | Cord of Stubborn Resolve | `e2add2e7254305b40aa1b9ae60ed2be0` | Capital Square Village |
| II | Moonlit Fork | `8a07f25d4083eb84c943bf95684f8e16` | Candlemere Tower |
| III | Falling Petal | `5b8346d4fc947624e9f8728fe7a12535` | Silverstep Grotto cave |
| III | Drawn Horizon | `040bad335c144784798a580e41b5410f` | Silverstep Grotto First World |
| IV | Storm Over Stone | `2d95232e6fc0b594bb6e13e3d3ea0dc3` | Varnhold |
| IV | Duelist's Rebuttal | `1f0bef6b8e540d644962171dc8810459` | Varnhold Stockade |
| IV | Foxfire Whisper | `8caed33ddd19e9447b852672e4b795f5` | Vordakai Tomb level 1 |
| IV | Viper's Reach | `53d54ca50fccb8c4d9242904eba04d14` | Vordakai Tomb level 2 |
| IV | Thunder at the Gate | `399410bf927fb3349bad940394fd9abe` | Armag's Tomb |
| IV | Mountain-Sunder | `1946bfd560469984788d4523e0d2786a` | Armag's Tomb level 2 |
| V | Empty Sleeve | `3160ffda16f855747ac22738f55a5c67` | Rushlight Festival camp |
| V | Moonlit Crossing | `b4183a776ad4c0b44acbc04837630a2e` | Brineheart |
| V | Unfixed Form | `db0e9ac023132cf46b49cd034dabf283` | Pitax Horde |
| V | Briar-Crowned Spear | `decb6060ab534294eb6d35510e45d317` | Blakemoor Hideout |
| V | River King's Measure | `b34367a637010f743815aed5875152bd` | Irovetti Palace |
| V | Irovetti's Ovation | `aeba7802ade083841935daf88d4652d3` | Irovetti Palace First World |
| Late | Night Without Moon | `b3344268950f27f4b840f216959f150e` | Castle of Knives |
| Late | The Last Word | `3bc451b100283774a9e23699dd869f1a` | Castle of Knives |
| Late | Heaven's Measure | `2252283386d5fb84b9e41d0187ed6dbc` | House at the Edge of Time floor 2 |
| Late | Watch at the World's End | `5a9b9e4b884ae064fa7caa5a13eab065` | House at the Edge of Time |
| Final | World-Tree Severer | `7e6448d1d8a7e4f4d9cc340b8f15e732` | Final Dungeon |
| Final | Spear of the First Branch | `13e98ebc52714d34eb8e53f1099110fd` | Final Dungeon level 2 |

Old density included Stag Lord Fort 3, Capital complex 3, Vordakai's Tomb 6,
Pitax Town/Horde 3, and Irovetti Palace 3. Corrected normalized named-area
density is at most 2: Vordakai is 2, Stag Lord Fort is 2, Capital is 1, and
the Pitax cluster is spread across Rushlight, Brineheart, Pitax Horde,
Blakemoor, and two separate palace phases.

## Per-item evidence contract

Record item/GUID, family, power, price, exact target GUID/name/type, area/chapter,
native contents, reference count, fixed/random/shared status, normal interaction,
quest/dialog/puzzle risks, density, theme, power, accessibility, rejected and
selected candidates, runtime result, human-only result, and old-save materialization.
Transactions preserve native/foreign rows and exact rollback, and never move an
already instantiated or player-owned item.
