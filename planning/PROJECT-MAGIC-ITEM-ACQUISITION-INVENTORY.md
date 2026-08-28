# Project Magic Item Acquisition Inventory

The current `0.0.105` audit covers every project-owned named or unique magic item deliberately published to base-campaign fixed loot. Mundane firearms, ordinary +1 merchant firearms, ammunition, maintenance kits, and crafting supplies are out of scope. The older Issue 12 inventory is retained below as superseded history.

## Contract

- Scope: 30 stable item identities: five rare firearms, eighteen named Eastern weapons, six named Elven Branched Spears, and Cord of Stubborn Resolve.
- Result: 30 distinct, deterministic base-campaign `BlueprintLoot` targets across 29 exact areas and zero recurring-vendor rows.
- Transactions remove only stale or duplicate project-owned references, append the intended exact reference once, preserve native and foreign entries and order, validate count one, and restore the exact snapshot if later bootstrap fails.
- The development action `Print all project magic-item location audits` reports item GUID, target GUID, target and area names, current count, all live loot locations, and all live vendor locations without granting or moving items.
- Static blueprint publication affects new campaigns and loot objects not yet materialized from the blueprint. It does not delete, move, or refresh items already instantiated in a save, including owned, stashed, dropped, sold, or previously opened-container items.

## 0.0.105 complete discoverability audit

Every row was checked for exact base-campaign identity, persistent area ownership,
ordinary loot interaction, campaign stage, power fit, target-name obscurity,
clustering, and retired-row cleanup. `Retained` means the prior target survived
the audit; `moved` means the old target is now an explicit cleanup target.

| Stage | Item | Exact fixed target GUID and name | Exact area | Audit result |
|---|---|---|---|---|
| Late Act I | Border Sentinel | `e72cdc1e01c1eb144b6c29084dd111fb` `Forest_ChestWithMasterworkWeapons` | `StagLordOldCamp` | moved from the crowded Stag Lord fort to an ordinary weapon chest |
| Late Act I | Paper Lantern | `59cb0ac65b4093440ad341b9a2f372cf` `Forest_BarrikadedChest1` | `StagLordFort` | retained; suitable early named weapon |
| Act II | Boughkeeper | `40db074f21260344b95d0e9919c8e682` `Forest_PoorLoot01` | `CapitalRegionLair01` | moved from a treasure stone to an ordinary regional-lair container |
| Act II | Wayfarer's Oath | `020246502ff864f4aab19e2fc00e63ee` `Forest_chest_close` | `TrollLair_Exterior` | retained; visible Troll Trouble loot |
| Act II | Quiet Current | `a2d14c56093720947a6ca4978c6a5985` `Forest_OldDwarfChest` | `TrollLair_SecondLevel` | moved out of a hidden-room chest |
| Act II | Winter Reed | `7208dc79fd87ca849babf696e62d4e93` `Forest_TrollhoundLairLoot02` | `TrollhoundLair` | moved out of a hidden poor box |
| Act II | Cloud-Cleaver | `2bffac36ed3499f4f9a1e6456e96a0f6` `Forest_LockedLoot01` | `CandlemereTower` | retained; ordinary fixed tower chest |
| Act II | Thornstep | `3322c56f38031eb4983b6f87c95081b7` `Forest_GoodLoot01` | `NorthNarlmarchesRegionLair01` | moved out of a generic cache and the Lonely Barrow cluster |
| Act II | Cord of Stubborn Resolve | `9572baf3952095f41abda1fb25055cce` `RichHuman_treasure_chest_04 (1)` | `CapitalTavern_Indoor` | moved from a temporary square variant to the persistent capital inn |
| Act III | Falling Petal | `df9ac89a7d8533a4e999bd267ae52b65` `Forest_UnhiddenLocked01` | `SilverstepGrotto_Cave` | moved out of a hidden chest |
| Act III | Drawn Horizon | `5e302038ce8b06f418a327d4eeadb51d` `Forest_loot_box_02` | `SilverstepLake_Outdoor` | moved out of a temporary First World area |
| Act III | Moonlit Fork | `2aa7aa5c2df96b143bd2fc62a8547c9c` `Forest_TH_GreatclubBarbarianMagic` | `MonsterLairHodag` | moved from the Candlemere cluster to ordinary monster-lair loot |
| Act IV | Storm Over Stone | `2d95232e6fc0b594bb6e13e3d3ea0dc3` `Forest_Loot01` | `Varnhold` | retained; ordinary fixed Varnhold loot |
| Act IV | Duelist's Rebuttal | `1f0bef6b8e540d644962171dc8810459` `Forest_Container_7_good` | `VarnholdStockade` | retained; separate stockade container |
| Act IV | Foxfire Whisper | `a9bb1f714425c564aadee3cc712fb96a` `Forest_CyclopLootRoot` | `DunswardOutdoor` | moved from an obscure Vordakai cache |
| Act IV | Viper's Reach | `8a850f7758cb77b498621a307445bb1e` `Forest_GoodLoot_withWeaponOrArmor` | `LoneCyclopCave` | moved from an obscure Vordakai cache |
| Act IV | Thunder at the Gate | `399410bf927fb3349bad940394fd9abe` `Barbarians_LootRoot` | `ArmagsTomb` | retained; thematic ordinary loot |
| Act IV | Mountain-Sunder | `462bf0e4476e8c7498b2462219d46d25` `Hills_chest_closed` | `BarbarianMainCamp` | moved off the second Armag's Tomb floor |
| Act V | Empty Sleeve | `c0f1626bb1a0b3b47ad452ce75c7f0e2` `RichHuman_GoodLoot_Locked#1` | `PitaxTown` | moved out of the temporary Rushlight Festival camp |
| Act V | Moonlit Crossing | `b4183a776ad4c0b44acbc04837630a2e` `RichHuman_treasure_chest_02` | `Brineheart` | retained; ordinary fixed Brineheart chest |
| Act V | Unfixed Form | `2e5e8c271f5b1ff4ca42dea4f8d8fb37` `Plains_good_loot_1` | `GlenebonPlains` | moved out of a temporary Pitax horde area |
| Act V | Briar-Crowned Spear | `decb6060ab534294eb6d35510e45d317` `RichHuman_NotHiddenLockedGood` | `BlakemoorHideout` | retained; explicitly non-hidden fixed loot |
| Act V | River King's Measure | `b34367a637010f743815aed5875152bd` `PoorHuman_IrovettiChambers_ChestHuge_Outline (3)` | `IrovettiPalace` | retained; ordinary palace chest |
| Act V | Irovetti's Ovation | `c5adf784c614e4b4c8dc220111f64a54` `RichHuman_ConservatoryLoot` | `IrovettiPalace` | moved from a temporary First World palace variant to a separate ordinary palace chest |
| Late | Night Without Moon | `b3344268950f27f4b840f216959f150e` `FirstWorld_GoodLoot_Trapped_1` | `CastleOfKnives` | retained; independent late-game cache |
| Late | Heaven's Measure | `e3703cd9a6de2f24c80c1505e3c9784f` `FirstWorld_2ndFloorGoodLoot05` | `HouseAtTheEdgeOfTime_2ndFloor` | moved out of a hidden locked chest |
| Late | Watch at World's End | `2df91222314044b4da37b7ee83841873` `FirstWorld_GoodLoot02` | `HouseAtTheEdgeOfTime` | moved out of a very-good hidden chest |
| Final | World-Tree Severer | `7e6448d1d8a7e4f4d9cc340b8f15e732` `RichHuman_Loot_1` | `FinalDungeon` | retained; capstone on the first final-dungeon floor |
| Final | Spear of the First Branch | `13e98ebc52714d34eb8e53f1099110fd` `RichHuman_Loot_5_2lvl` | `FinalDungeon2` | retained; capstone on the second final-dungeon floor |
| Final | The Last Word | `559739642f21aaf40847f4ddcbe3db79` `RichHuman_Loot_2_3lvl` | `FinalDungeon3` | moved away from quest-coupled Castle of Knives loot to the third final-dungeon floor |

The audit moved 18 items and retained 12. Exact-target density is one item per
target. Exact-area density is one except for the two separate Irovetti Palace
chests. Normalized final-sequence density is permitted at three only because the
three capstones are split across `FinalDungeon`, `FinalDungeon2`, and
`FinalDungeon3`. No named item was moved to a vendor or centralized in the
capital.

## Superseded 0.0.87 exact acquisition inventory

All targets below are installed Kingmaker 2.1.7b `Kingmaker.Blueprints.Loot.BlueprintLoot` objects in the base campaign. `Fixed/unique` means an exact area-owned target rather than a random table, artisan reward, dialogue grant, DLC target, broad area hook, or shared generic table. Prices are blueprint gold-piece costs; power is the effective enhancement profile used for chapter placement.

| Item | Item GUID | Type / power / price | Previous deliberate source | Final fixed target GUID and name | Act / area | Target evidence and native contents | Status |
|---|---|---|---|---|---|---|---|
| Paper Lantern | `fbb319cb67ae5657820548791a7a3733` | Katana, +1 flaming, 10,370 | Shared Stag Lord chest with Wayfarer's Oath | `59cb0ac65b4093440ad341b9a2f372cf` `Forest_BarrikadedChest1` | Late Act I, Stag Lord Fort | Fixed/unique base-campaign barricaded chest; native/foreign order retained | moved alone |
| Wayfarer's Oath | `9ac64342cca85f72b0fe81cb6b9c53c0` | Nodachi, +1 cold iron/bespoke, 6,400 | Stag Lord Fort cluster | `020246502ff864f4aab19e2fc00e63ee` `Forest_chest_close` | Act II, Troll Lair exterior | Scroll of Fireball, Dart +1, gold | moved to ordinary fixed Troll Trouble loot |
| Border Sentinel | `c1c7a6746916504ebfdcb2b650a7145b` | Naginata, +1 cold iron, 4,420 | Oleg before Issue 7 | `c8b8159fb695be64883b609a7e77e75d` `PoorHuman_treasure_chest_03` | Late Act I, Stag Lord Fort | Rusty Horseshoe, 12 gold; Issue 7 exact target preserved | unchanged from Issue 7 |
| Quiet Current | `be05a24b1b145e1ea008a4bf42b04c32` | Katana, +2 agile, 18,335 | Questionable Capital backpack | `6abcbbc0a161aa54380808655de92197` `Forest_HiddenRoomChest3` | Act II, Troll Lair second level | Shock Light Crossbow +1, gold | moved to exact hidden fixed chest |
| Winter Reed | `060f933d8912594cbc3da731c4dae7a3` | Nodachi, +2 frost, 18,350 | Capital cluster | `27b9b282c32996842bde77e360b72107` `Forest_HiddenPoor_Box` | Act II, Shrine of Lamashtu | Frost Falchion +1, scrolls and potions | moved to thematic fixed Season of Bloom loot |
| Cloud-Cleaver | `bb863dabbf655059af768723cf6226ba` | Naginata, +2 keen, 18,360 | Lonely Barrow pair | `2bffac36ed3499f4f9a1e6456e96a0f6` `Forest_LockedLoot01` | Act II, Candlemere Tower | Noble Hammer | moved to fixed tower loot |
| Falling Petal | `c56dd11c12355a83b1cd9d833b2e5321` | Katana, +2 agile keen plus premium, 36,335 | Goblin cluster | `5b8346d4fc947624e9f8728fe7a12535` `Forest_HiddenLocked02` | Act III, Silverstep Grotto cave | Graceful End | moved to exact hidden fixed chest |
| Drawn Horizon | `d3f2a227bd335087805eb7225721dc83` | Nodachi, +3 keen, 32,350 | Same Goblin cluster | `040bad335c144784798a580e41b5410f` `Forest_Good_GuardedChest` | Act III, Silverstep Grotto First World | Belt of Physical Perfection +2, cold-iron longsword +1, banded armor | moved alone |
| Storm Over Stone | `a7559dde16945f90aada81ecf9adb97a` | Naginata, +3 shock/thundering, 50,360 | Vordakai six-item cluster | `2d95232e6fc0b594bb6e13e3d3ea0dc3` `Forest_Loot01` | Act IV, Varnhold | Frost Scythe +2 and provisions | moved to fixed Varnhold loot |
| Foxfire Whisper | `c7fc72c801e9506bb0c87e84eee8d313` | Katana, +3 agile ghost touch, 50,335 | Vordakai chest shared with two Eastern weapons, Viper's Reach, and Duelist's Rebuttal | `8caed33ddd19e9447b852672e4b795f5` `Forest_cache` | Act IV, Vordakai Tomb Level 1 | Scroll of Raise Dead, 1,212 gold | moved alone |
| Thunder at the Gate | `d5c7922d57a95025a977dd1ee59cb098` | Nodachi, +3 shock/thundering, 50,350 | Vordakai six-item cluster | `399410bf927fb3349bad940394fd9abe` `Barbarians_LootRoot` | Act IV, Armag's Tomb | Longsword +3 | moved to exact barbarian fixed loot |
| Mountain-Sunder | `5867c9be30e15d3a8a22e0f442959d03` | Naginata, +3 keen plus premium, 52,360 | Vordakai six-item cluster | `1946bfd560469984788d4523e0d2786a` `Barbarians_GoodLootRoot` | Act IV, Armag's Tomb level 2 | Human-bane Morningstar +2 | moved to exact barbarian fixed loot |
| Empty Sleeve | `a576839afc71574eb77203bf390fdf30` | Katana, +4 agile keen, 72,335 | Pitax cluster | `3160ffda16f855747ac22738f55a5c67` `RichHuman_Box10` | Act V, Rushlight Festival camp | Daggers +1 and poison wand | moved to exact fixed festival loot |
| Moonlit Crossing | `457e6f3694405f27999cf46047fafa52` | Nodachi, +4 keen holy plus premium, 106,350 | Pitax cluster | `b4183a776ad4c0b44acbc04837630a2e` `RichHuman_treasure_chest_02` | Act V, Brineheart | Lesser Ring of Balance | moved to exact fixed Brineheart loot |
| Unfixed Form | `f4bed29f193e57f6826dc83a684e65db` | Naginata, +4 keen plus premium, 62,360 | Recurring project merchant row | `db0e9ac023132cf46b49cd034dabf283` `RichHuman_GoodLoot_Locked` | Act V, Pitax Horde | Unbound Blade | moved to fixed loot |
| Night Without Moon | `dc660fcebcc855bfb046336fc78a93ae` | Katana, +5 agile keen speed, 200,335 | Final Dungeon chest shared with two Eastern weapons and First Branch | `b3344268950f27f4b840f216959f150e` `FirstWorld_GoodLoot_Trapped_1` | Late game, Castle of Knives | Chainshirt +5 | moved alone |
| Heaven's Measure | `dc086bdf8af25bceb569c8f5c627f560` | Nodachi, +5 keen brilliant energy, 200,350 | Same Final Dungeon cluster | `2252283386d5fb84b9e41d0187ed6dbc` `FirstWorld_2ndFloorGoodHiddenLockedLoot08` | Late game, House at the Edge of Time 2nd Floor | Speed/Ghost Touch Quarterstaff +4 | moved alone |
| World-Tree Severer | `e6e5cf56d3a259debd2f16a300bff115` | Naginata, +5 holy speed, 200,360 | Same Final Dungeon cluster | `7e6448d1d8a7e4f4d9cc340b8f15e732` `RichHuman_Loot_1` | Final act, Final Dungeon | Greatsword +5 | retained alone |
| Boughkeeper | `4a084b0226e077b58d79e33184018002` | Branched spear, +1, 5,320 | Existing distinct fixed loot | `19c1920cf93076249b5c4f29488851f9` `Forest_PriestGhost_TreasureStoneLoot` | Act II, Big Narlmarches | Phylactery of Positive Channeling | retained alone |
| Thornstep | `676faa5f811d851c9f14204bf864e1ec` | Branched spear, +1, 14,320 | Big Narlmarches pair | `364711342543d814eb95aa98a4c65e58` `Forest_cache_1` | Act II, Lonely Barrow | Lesser Gloves of Dueling | moved to exact fixed cache |
| Moonlit Fork | `403d62f6d3bb415c86939430176e55c0` | Branched spear, +2, 18,340 | Lonely Barrow pair | `8a07f25d4083eb84c943bf95684f8e16` `Forest_Loot01` | Act II, Candlemere Tower | Full Plate +2 | moved to exact tower loot |
| Viper's Reach | `1cfe40563a9b816931bb35e69677ac27` | Branched spear, +3, 70,320 | Vordakai cluster | `53d54ca50fccb8c4d9242904eba04d14` `Forest_cache_1561` | Act IV, Vordakai Tomb Level 2 | Gold, agates, potions, Invisibility, Silver Ring, Jade | moved alone |
| Briar-Crowned Spear | `ee580f43f50a0f0afefaedb3ce7133f3` | Branched spear, +4, 72,320 | Weak Irovetti armory mismatch | `decb6060ab534294eb6d35510e45d317` `RichHuman_NotHiddenLockedGood` | Act V, Blakemoor Hideout | Three strong native magic items | moved to power-appropriate fixed loot |
| Spear of the First Branch | `85c18b96ebee3fdc87eb33da93c8fdf6` | Branched spear, +5, 202,340 | Final Dungeon cluster | `13e98ebc52714d34eb8e53f1099110fd` `RichHuman_Loot_5_2lvl` | Final act, Final Dungeon Level 2 | Amulet of Natural Armor +5, fey-bane cold-iron tongi +4 | moved alone |
| Cord of Stubborn Resolve | `c4b804d9ebf941b4842b0a461a2b6b6d` | Belt, fatigue conversion, 15,000 | Capital Smith recurring vendor stock | `e2add2e7254305b40aa1b9ae60ed2be0` `RichHuman_treasure_chest_2` | Act II, Capital Square Village | Belt of Constitution +2; thematic belt cache | moved to fixed loot |
| Duelist's Rebuttal | `bae89c3abc3240578a6bff69044d2c1b` | Pistol, equivalent +3, 19,300 | Vordakai cluster | `1f0bef6b8e540d644962171dc8810459` `Forest_Container_7_good` | Act IV, Varnhold Stockade | Crimson Counselor, antique cyclops boot, food | moved to fixed Varnhold loot |
| River King's Measure | `a27c86b0d87c423d9ba8a05227bbf1e6` | Musket, equivalent +5, 51,800 | Distinct Pitax fixed loot | `b34367a637010f743815aed5875152bd` `PoorHuman_IrovettiChambers_ChestHuge_Outline (3)` | Act V, Irovetti Palace | Gems, medallion, standard weapons, 632 gold | unchanged |
| Irovetti's Ovation | `caf23b7555cd4524a7622eaa25266ea1` | Blunderbuss, equivalent +5, 52,300 | Puzzle-wrapper uncertainty | `aeba7802ade083841935daf88d4652d3` `RichHuman_GoodLoot` | Act V, Irovetti Palace First World | Calistria Rapier | moved to ordinary fixed loot |
| The Last Word | `0d31f794ba294c1e834af44f918f6721` | Pistol, equivalent +7, 99,300 | Coupled to The End quest item | `3bc451b100283774a9e23699dd869f1a` `FirstWorld_GoodLoot_Locked_2` | Late game, Castle of Knives | Greater Empower metamagic rod | moved to independent fixed capstone cache |
| Watch at World's End | `87c7baaaad504b7f8742f2dfcd79d067` | Rifle, equivalent +7, 99,800 | Distinct late fixed loot | `5a9b9e4b884ae064fa7caa5a13eab065` `FirstWorld_VeryGoodHiddenLoot02` | Late game, House at the Edge of Time | Forewarning Shield | unchanged |

## Qualification evidence

### Current 0.0.105 qualification

- Immutable guarded-runtime artifact: source-state SHA-256
  `250ED285247113C33B39855609F6125C68652C7C744F06B967DD0EC7CD0981E7`,
  package SHA-256
  `6B6A85BD7642715841A4820B6DB9A443A69C4D9EB578E3C56A6FBC5912BCE8CE`,
  DLL SHA-256
  `2A06D93880E3716E29B30153A7E5B48FC53F6C363D1C1D53A0CB29819FBB457C`,
  MVID `61589b34-b11d-43a2-9d06-f9fac46fcdf3`.
- Complete campaign audit: evidence directory
  `20260828T0237409495792Z-observe-rare-firearm-acquisition`, run ID
  `20260828T0237409652002Z-da91ee23bd11461cb090f401c03dcde3`, PASS,
  25/25 assertions. The live graph contains 30 items on 30 distinct targets
  across 29 exact areas, one active row per item, no vendor rows, and no stale
  copies. The shared discoverability policy returned PASS.
- Focused Cord audit: evidence directory
  `20260828T0240171423403Z-observe-capital-cord-vendor`, run ID
  `20260828T0240171580108Z-c3515522e0db41bfbb6dfe7a482d8271`, PASS,
  4/4 assertions. It proves the exact persistent capital-inn target, count one,
  zero vendor rows, and zero retired Capital Square Village rows.
- Automated gates: repository validation PASS; 1,315/1,315 dependency-free
  domain/reflection tests PASS; clean Release build PASS; build-output,
  SoundBank, and strict standalone package validation PASS.

### Superseded 0.0.88 qualification history

- Pre-change read-only catalog run: `20260820T0827266290275Z-observe-rare-firearm-acquisition`, PASS. It enumerated 436 fixed candidates and established exact area/type/reference evidence before mutation.
- First repaired-candidate run: `20260820T0851451113499Z-5a2a08d9fdc6420d9c8dcef6ed5978eb`, FAIL only because a legacy Smith-table assertion retained its pre-distribution total. Its new `projectMagicDistribution` evidence already showed 30 items, 30 targets, one target row each, and zero vendor rows.
- Qualified campaign audit: evidence directory `20260820T0855347791407Z-observe-rare-firearm-acquisition`, PASS, 25/25 assertions. Exact package SHA-256 `72B2E2CDBA163DB127A1ED9B6F6728EBD7AE9DFE5FCA9E674CBF0BFE7D4A81FD`; DLL SHA-256 `3A76F00E2752DB6A7682BC406DEF59FD4E8CDB65410DE3A81AA84831897B6DD9`.
- Qualified Cord audit: evidence directory `20260820T0859287762363Z-observe-capital-cord-vendor`, PASS. It proves the exact Capital Square Village target, count one, and zero Smith/vendor publication.
- Automated gates: repository validation PASS; 1,160/1,160 dependency-free domain/reflection tests PASS; clean Release build PASS; build-output validation PASS; SoundBank validation PASS; strict standalone package validation PASS.

The runtime graph proves blueprint publication and exact target ownership. Final organic pacing and discoverability remain a consolidated human acceptance judgment; no claim is made that an already materialized merchant or opened container refreshes in an existing save.
