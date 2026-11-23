# Tabletop Added Rules v0.0.194

This stable release makes the mod compatible with Bag of Tricks' **No Item Weight** setting. On v0.0.193 and earlier,
turning that setting on stopped the mod's startup: its content was missing for that session, and Summon Monster I had
no creature choices. Every identity from v0.0.193 is unchanged and none is appended, so existing saves keep their
references. Unity Mod Manager 0.31.1 or newer is still the minimum.

## Bag of Tricks No Item Weight

With Bag of Tricks' Items & Equipment setting that makes every item weigh nothing, the mod loaded and showed as active,
but:

- the log contained `Elven Branched Spear type profile is invalid` with `weight [effective-getter]` and
  `getterPatches=BagOfTricks`, and the mod's content was not available for that session;
- Summon Monster I and the other summoning spells had no expanded creature choices.

What was wrong, and what changed:

- **Cause.** During startup the mod checked its items' weights by asking the game for each item's current weight.
  With No Item Weight on, Bag of Tricks makes the game answer 0 for every item, which is exactly what that setting is
  for. The mod read that 0 as broken item data and stopped startup.
- **Repair.** The startup checks now look at the weight stored in the mod's own item data, which is still 10 lb for
  the spear and so on, and no longer treat Bag of Tricks' runtime 0 as an error. Wrong stored data still stops
  startup, as before.
- **Items made while the setting is on.** A few items (magic and named firearms, Magic Circle scrolls and Teleportation
  scrolls) used to copy their weight from another item when they were built. With No Item Weight on, they would have
  copied 0 and stayed weightless even after the setting was turned off. They now take their intended weight from the
  mod's own data or from the source item's stored weight.
- **No new dependency.** The mod does not read or change Bag of Tricks' settings and does not need Bag of Tricks.
  Bag of Tricks' own weight change already covers every item the mod adds.

With the setting on, every item from this mod weighs 0 in tooltips, stacks and the party's carried weight, like the
game's own items. With it off, or without Bag of Tricks, every item has its normal weight, including items picked up
or made while the setting was on. Switching the setting updates the same items without a restart. Items that
naturally weigh nothing, such as Paper Cartridges, stay weightless.

Item values, names, descriptions and prices are unchanged.

## Existing saves

Nothing in this release changes, adds or removes any identity, so saves load exactly as before. A save made with
No Item Weight on loads with each item's normal weight when the setting is off, and at 0 when it is on.

## Disclosed open items

- **Tested with a small set of mods.** The checks ran in a test copy of the game with Bag of Tricks 1.16.4 and Call
  of the Wild, with Bag of Tricks alone, and with no other mods. The reporting player's other mods were not
  installed, and their full log was not available.
- **Tooltip display.** Item weights were checked through the game's tooltip text function and the inventory's carried
  weight refresh. The rendered tooltip on screen was not inspected.
- **Test-lab restoration.** The test copy of the game was not shown to be restored to its exact earlier setup after
  testing. The player's own game, saves and settings were verified unchanged. This affects only the test lab.
- **Tested on a development build.** The checks before release ran in a test copy of the game on a development build
  of the same source. The release build is checked in the game after publication, and that result is recorded in the
  repository's release record.
- **Earlier items.** The disclosures in the v0.0.193 notes still stand, unchanged.
