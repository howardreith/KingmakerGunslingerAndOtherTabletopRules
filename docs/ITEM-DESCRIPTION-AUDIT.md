# Project Item Description Audit

Scope: every project-registered `BlueprintItem` and `BlueprintItemWeapon`. Player-facing localizations are reviewed below; internal-only shells and surrogate items with no project-authored localization are separately inventoried at the end. Native Owlcat and third-party descriptions were not changed. This audit treats the normal item card as authoritative for enhancement, material, weapon category, handedness, damage, critical profile, and ordinary enchantments; those traits are no longer repeated in item prose.

`—` means the item deliberately has no extra description because its card already communicates every player-relevant property. Flavor text remains separate. Keys below are the authoritative source keys (some are generated from the listed symbol in the named/generic builders).

## Ammunition and supplies

| Item | Description key | Old text | New text | Retained information |
| --- | --- | --- | --- | --- |
| Black Powder Charge | `KMG.Item.BlackPowderCharge.Description` | A measured charge of black powder used with a projectile to load an early firearm. | Use with a lead ball to load early firearms. | Required loose-ammunition pairing. |
| Lead Ball | `KMG.Item.LeadBall.Description` | A cast lead projectile sized for an early firearm. Loading one also requires a black powder charge. | Use with a black powder charge to load early firearms. | Required loose-ammunition pairing. |
| Paper Cartridge | `KMG.Item.PaperCartridge.Description` | A prepared paper or cloth bundle…compatible with early pistols, muskets, and blunderbusses. | Combines powder and shot for early firearms. Reduces reload time by one step and increases misfire by 1. | The non-obvious reload and misfire tradeoff. |
| Gunsmith's Kit | `KMG.Item.GunsmithKit.Description` | A durable, non-consumable set of firearm-cleaning and ammunition-casting tools required to craft basic firearm ammunition. | Required to craft basic firearm ammunition. | Crafting prerequisite. |
| Firearm Overhaul Kit | `KMG.Item.OverhaulKit.Description` | A consumable set of fitted replacement parts used by Overhaul Firearm to restore one Wrecked firearm to Broken condition. | Consume with Overhaul Firearm to change one wrecked firearm to broken. | Exact use and state change. |
| Firearm Repair Kit | `KMG.Item.FirearmRepairKit.Description` | A compact set of replacement springs, pins, tools, and fitted parts…ordinary repair is still required afterward. | Consume with Overhaul Firearm to change a wrecked Test Musket to an empty broken firearm. Repair it afterward. | Test-fixture limitation and follow-up repair. |

## Ordinary firearms

The previous ordinary-firearm generator began with `An early/advanced firearm with a black-powder mechanism` (or its blunderbuss variant), repeated the firearm identity, then appended the penetration rule. Every entry now uses the concise common text below plus its existing penetration sentence.

| Item | Description key | Old text before shared penetration | New text before shared penetration | Retained information |
| --- | --- | --- | --- | --- |
| Pistol | `KMG.Item.Pistol.Description` | An early firearm with a black-powder mechanism. It uses powder and lead shot, can misfire, and must be reloaded as its capacity is spent. | Uses black powder and lead balls. It can misfire and must be reloaded. | Ammunition, misfire, reload, and Touch-AC rule remain useful. |
| Musket | `KMG.Item.Musket.Description` | An early firearm with a black-powder mechanism. It uses powder and lead shot, can misfire, and must be reloaded as its capacity is spent. | Uses black powder and lead balls. It can misfire and must be reloaded. | Ammunition, misfire, reload, and Touch-AC rule remain useful. |
| Blunderbuss | `KMG.Item.Blunderbuss.Description` | An early firearm that can fire an ordinary lead ball at a 10-foot range increment or use Scatter Shot to fire pellets in a 15-foot cone. It uses black powder and lead balls, can misfire, and must be reloaded after firing. | Uses black powder and lead balls. It can fire a lead ball or use Scatter Shot to fire pellets in a 15-foot cone. It can misfire and must be reloaded. | Scatter exception, ammunition, misfire, reload, and direct-fire Touch-AC rule. |
| Advanced Rifle | `KMG.Item.AdvancedRifle.Description` | An advanced firearm with a black-powder mechanism. It uses powder and lead shot, can misfire, and must be reloaded as its capacity is spent. | Uses black powder and lead balls. It can misfire and must be reloaded. | Ammunition, misfire, reload, and advanced penetration rule. |
| Advanced Revolver | `KMG.Item.AdvancedRevolver.Description` | An advanced firearm with a black-powder mechanism. It uses powder and lead shot, can misfire, and must be reloaded as its capacity is spent. | Uses black powder and lead balls. It can misfire and must be reloaded. | Ammunition, misfire, reload, and advanced penetration rule. |

## Magic firearms

All eight entries retain the shared penetration sentence. The old text redundantly began with the enhancement, firearm type, and ordinary enchantment name; the normal item card already displays those values.

| Item | Description key | Old unique text | New unique text | Retained information |
| --- | --- | --- | --- | --- |
| Pistol +1 | `KMG.Firearms.PistolPlus1Item.Description` | This masterwork pistol bears a +1 enhancement bonus. | — | The card displays masterwork and +1; penetration remains appended. |
| Musket +1 | `KMG.Firearms.MusketPlus1Item.Description` | This masterwork musket bears a +1 enhancement bonus. | — | The card displays masterwork and +1; penetration remains appended. |
| Blunderbuss +1 | `KMG.Firearms.BlunderbussPlus1Item.Description` | This masterwork blunderbuss bears a +1 enhancement bonus. | — | The card displays masterwork and +1; penetration remains appended. |
| Duelist's Rebuttal | `KMG.Firearms.DuelistsRebuttalItem.Description` | +2 Reliable pistol. Reliable reduces…a natural 1 remains a miss. | Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0. A natural 1 still misses. | Reliable's non-obvious exception. |
| The River King's Measure | `KMG.Firearms.RiverKingsMeasureItem.Description` | +4 Reliable musket. Reliable reduces…a natural 1 remains a miss. | Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0. A natural 1 still misses. | Reliable's non-obvious exception. |
| Irovetti's Ovation | `KMG.Firearms.IrovettisOvationItem.Description` | +4 Reliable blunderbuss. Reliable reduces…a natural 1 remains a miss. | Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0. A natural 1 still misses. | Reliable's non-obvious exception. |
| The Last Word | `KMG.Firearms.TheLastWordItem.Description` | +5 Reliable Seeking pistol. Reliable reduces…Seeking ignores concealment miss chances… | Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0. A natural 1 still misses. Seeking ignores concealment miss chances without revealing unseen creatures or bypassing other defenses. | Reliable and Seeking exceptions. |
| Watch at the World's End | `KMG.Firearms.WatchAtWorldsEndItem.Description` | +5 Reliable Fey Bane musket. Reliable reduces…Fey Bane is especially effective against Fey creatures. | Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0. A natural 1 still misses. | Reliable exception; Fey Bane remains on the normal enchantment card. |

## Eastern generic weapons

The old generator is expanded below so every stable item and localization key has an individual review row.

| Item | Description key | Old text | New text | Retained information |
| --- | --- | --- | --- | --- |
| Wakizashi | `KMG.EasternWeapons.Wakizashi.BaseItem.Description` | This is a wakizashi. | Usable with Weapon Finesse. | Finesse is the family-specific rule. |
| Masterwork Wakizashi | `KMG.EasternWeapons.Wakizashi.MasterworkItem.Description` | This is a masterwork wakizashi. | Usable with Weapon Finesse. | Finesse is the family-specific rule. |
| Cold Iron Wakizashi | `KMG.EasternWeapons.Wakizashi.ColdIronItem.Description` | This is a cold iron wakizashi. | Usable with Weapon Finesse. | Finesse is the family-specific rule. |
| +1 Wakizashi | `KMG.EasternWeapons.Wakizashi.Plus1Item.Description` | This is a +1 magic wakizashi. | Usable with Weapon Finesse. | Finesse is the family-specific rule. |
| Katana | `KMG.EasternWeapons.Katana.BaseItem.Description` | This is a katana. | Martial Weapon Proficiency is sufficient when wielded two-handed. | Grip-dependent proficiency exception. |
| Masterwork Katana | `KMG.EasternWeapons.Katana.MasterworkItem.Description` | This is a masterwork katana. | Martial Weapon Proficiency is sufficient when wielded two-handed. | Grip-dependent proficiency exception. |
| Cold Iron Katana | `KMG.EasternWeapons.Katana.ColdIronItem.Description` | This is a cold iron katana. | Martial Weapon Proficiency is sufficient when wielded two-handed. | Grip-dependent proficiency exception. |
| +1 Katana | `KMG.EasternWeapons.Katana.Plus1Item.Description` | This is a +1 magic katana. | Martial Weapon Proficiency is sufficient when wielded two-handed. | Grip-dependent proficiency exception. |
| Nodachi | `KMG.EasternWeapons.Nodachi.BaseItem.Description` | This is a nodachi. | A long cavalry blade designed for sweeping cuts. | Concise flavor prevents inherited donor Brace text from leaking into the item card. |
| Masterwork Nodachi | `KMG.EasternWeapons.Nodachi.MasterworkItem.Description` | This is a masterwork nodachi. | A long cavalry blade designed for sweeping cuts. | Concise flavor prevents inherited donor Brace text from leaking into the item card. |
| Cold Iron Nodachi | `KMG.EasternWeapons.Nodachi.ColdIronItem.Description` | This is a cold iron nodachi. | A long cavalry blade designed for sweeping cuts. | Concise flavor prevents inherited donor Brace text from leaking into the item card. |
| +1 Nodachi | `KMG.EasternWeapons.Nodachi.Plus1Item.Description` | This is a +1 magic nodachi. | A long cavalry blade designed for sweeping cuts. | Concise flavor prevents inherited donor Brace text from leaking into the item card. |

## Eastern named weapons

The old generated text was `{enhancement}, {native properties}, {material} {family}.` followed only by the handful of bespoke effects. Standard properties are now omitted. Each row remains one stable item and localization key.

| Item | Description key | New text | Retained information |
| --- | --- | --- | --- |
| Paper Lantern | `KMG.EasternWeapons.Wakizashi.PaperLantern.Description` | A warm shimmer travels along its polished edge. | Native card covers properties; flavor blocks donor copy. |
| Quiet Current | `KMG.EasternWeapons.Wakizashi.QuietCurrent.Description` | Its polished edge moves like still water. | Native card covers properties; flavor blocks donor copy. |
| Falling Petal | `KMG.EasternWeapons.Wakizashi.FallingPetal.Description` | A critical hit grants a +1 dodge bonus to AC for 1 round, ending early if you stop wielding it. | Bespoke critical-hit effect. |
| Foxfire Whisper | `KMG.EasternWeapons.Wakizashi.FoxfireWhisper.Description` | A pale glimmer clings to the blade. | Native card covers properties; flavor blocks donor copy. |
| Empty Sleeve | `KMG.EasternWeapons.Wakizashi.EmptySleeve.Description` | Its unadorned guard conceals careful craftsmanship. | Native card covers properties; flavor blocks donor copy. |
| Night Without Moon | `KMG.EasternWeapons.Wakizashi.NightWithoutMoon.Description` | Its blackened steel drinks in the light. | Native card covers properties; flavor blocks donor copy. |
| Wayfarer's Oath | `KMG.EasternWeapons.Katana.WayfarersOath.Description` | Grants a +2 competence bonus on Initiative while equipped. | Bespoke equipped bonus. |
| Winter Reed | `KMG.EasternWeapons.Katana.WinterReed.Description` | A cold hush follows each drawn cut. | Native card covers properties; flavor blocks donor copy. |
| Drawn Horizon | `KMG.EasternWeapons.Katana.DrawnHorizon.Description` | Its edge reflects a distant, clear horizon. | Native card covers properties; flavor blocks donor copy. |
| Thunder at the Gate | `KMG.EasternWeapons.Katana.ThunderAtTheGate.Description` | A storm-dark pattern runs along the steel. | Native card covers properties; flavor blocks donor copy. |
| Moonlit Crossing | `KMG.EasternWeapons.Katana.MoonlitCrossing.Description` | One-handed use grants a +1 dodge bonus to AC. Two-handed use grants +2 weapon damage. | Grip-dependent bespoke effects. |
| Heaven's Measure | `KMG.EasternWeapons.Katana.HeavensMeasure.Description` | Its measured curve is bright as noon. | Native card covers properties; flavor blocks donor copy. |
| Border Sentinel | `KMG.EasternWeapons.Nodachi.BorderSentinel.Description` | A border warden's blade, worn smooth by long patrols. | Native card covers properties; flavor blocks donor copy. |
| Cloud-Cleaver | `KMG.EasternWeapons.Nodachi.CloudCleaver.Description` | Its broad edge is polished like open sky. | Native card covers properties; flavor blocks donor copy. |
| Storm Over Stone | `KMG.EasternWeapons.Nodachi.StormOverStone.Description` | Its dark steel recalls rain on stone. | Native card covers properties; flavor blocks donor copy. |
| Mountain-Sunder | `KMG.EasternWeapons.Nodachi.MountainSunder.Description` | Mighty Cleaving allows one additional Cleave attack. While Power Attack is active, your first hit each round deals 1d6 force damage. | Both bespoke effects. |
| Unfixed Form | `KMG.EasternWeapons.Nodachi.UnfixedForm.Description` | While polymorphed or not your natural size, deals base damage as if one size category larger. | Bespoke size condition. |
| World-Tree Severer | `KMG.EasternWeapons.Nodachi.WorldTreeSeverer.Description` | Its blade bears the shape of an ancient bough. | Native card covers properties; flavor blocks donor copy. |

## Elven Branched Spears

The old generic generator recited cold iron, masterwork, enhancement, two-handed/reach, and weapon identity. The old named generator added the same profile and the repetitive `It remains…` tail. The card now owns standard traits; every spear retains the nonstandard Finesse and movement-provoked attack-of-opportunity rule.

| Item | Description key | New text | Retained information |
| --- | --- | --- | --- |
| Elven Branched Spear | `KMG.ElvenBranchedSpear.BaseItem.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. | Finesse and unique opportunity bonus. |
| Masterwork Elven Branched Spear | `KMG.ElvenBranchedSpear.MasterworkItem.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. | Finesse and unique opportunity bonus. |
| Cold Iron Elven Branched Spear | `KMG.ElvenBranchedSpear.ColdIronItem.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. | Finesse and unique opportunity bonus. |
| Masterwork Cold Iron Elven Branched Spear | `KMG.ElvenBranchedSpear.MasterworkColdIronItem.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. | Finesse and unique opportunity bonus. |
| +1 Elven Branched Spear | `KMG.ElvenBranchedSpear.Plus1Item.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. | Finesse and unique opportunity bonus. |
| +1 Cold Iron Elven Branched Spear | `KMG.ElvenBranchedSpear.Plus1ColdIronItem.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. | Finesse and unique opportunity bonus. |
| Boughkeeper | `KMG.ElvenBranchedSpear.Boughkeeper.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. An attack of opportunity hit grants +1 dodge bonus to AC until the start of your next turn. | Boughkeeper trigger. |
| Thornstep | `KMG.ElvenBranchedSpear.Thornstep.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. Once per round, an attack of opportunity triggered by enemy movement that hits reduces the target's speed by 10 feet for 1 round. | Thornstep trigger and limit. |
| Moonlit Fork | `KMG.ElvenBranchedSpear.MoonlitFork.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. | Finesse and unique opportunity bonus; no standard-trait repetition. |
| Viper's Reach | `KMG.ElvenBranchedSpear.VipersReach.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. Once per round, a damaging sneak attack imposes a -2 penalty on Reflex saves for 1 round. | Viper's Reach trigger and limit. |
| Briar-Crowned Spear | `KMG.ElvenBranchedSpear.BriarCrownedSpear.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. Once per round after an attack of opportunity hit, expend another available attack of opportunity to attack that target at -5. | Extra-attack trigger and cost. |
| Spear of the First Branch | `KMG.ElvenBranchedSpear.SpearOfTheFirstBranch.Description` | Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement. Once per round after an attack of opportunity hit or damaging sneak attack, the target makes a Fortitude save. Failure entangles it for 1 round; success reduces its speed by 10 feet for 1 round. | Save, outcomes, and limit. |

## Other project-created items

| Item / role | Description key | Old text | New text | Reason |
| --- | --- | --- | --- | --- |
| Test Musket | Native Standard Heavy Crossbow description retained by clone | Native donor text | Unchanged | Diagnostic clone intentionally inherits Owlcat text; changing it would rewrite a native description. |
| Pistol-Whip one-handed surrogate | none | Native donor text | Unchanged | Runtime-only attack surrogate; no project-authored item localization or inventory presentation. |
| Pistol-Whip two-handed surrogate | none | Native donor text | Unchanged | Runtime-only attack surrogate; no project-authored item localization or inventory presentation. |
| Summoned Salamander Spear | none (`KMG.Summoning.Special.Salamander.Spear`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |
| Summoned Salamander Tail | none (`KMG.Summoning.Special.Salamander.Tail`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |
| Summoned Bebelith Claw | none (`KMG.Summoning.Special.Bebelith.Claw`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |
| Summoned Pixie Sleep Bow | none (`KMG.Summoning.Special.Pixie.SleepBow`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |
| Summoned Bite 1d4 | none (`KMG.Summoning.Natural.Bite1d4`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |
| Summoned Bite 1d3 | none (`KMG.Summoning.Natural.Bite1d3`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |
| Summoned Tail 1d12 | none (`KMG.Summoning.Natural.Tail1d12`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |
| Summoned Tail 3d6 | none (`KMG.Summoning.Natural.Tail3d6`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |
| Summoned Bite 2d8 | none (`KMG.Summoning.Natural.Bite2d8`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |
| Summoned Talon 2d6 | none (`KMG.Summoning.Natural.Talon2d6`) | Empty shell | Empty shell | Internal summoned-unit equipment, never a player inventory item. |

The source of truth for the changed copy is `BasicAmmunitionBlueprints`, `GunsmithingSupplyBlueprints`, `FirearmRepairKitBlueprints`, `CordOfStubbornResolveBlueprints`, `ProductionFirearmBlueprints`, `MagicFirearmBlueprints`, `EasternWeaponBlueprints`, `EasternWeaponNamedBlueprints`, `ElvenBranchedSpearBlueprints`, and `ElvenBranchedSpearNamedBlueprints`.
