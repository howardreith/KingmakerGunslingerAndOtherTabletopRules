# Tabletop Added Rules v0.0.196

This stable release fixes the **Racial Bonus** shown next to each ability score for the alternate elemental
heritages (Ifrit, Oread, Sylph and Undine). Ability scores themselves were already correct; only the label was wrong.
Every identity from v0.0.195 is unchanged and none is appended, so existing saves keep their references. Unity Mod
Manager 0.31.1 or newer is still the minimum.

## Elemental heritage racial bonus labels

A player reported: "Tried using the Sylph and found myself with +2 dex and Cha but -2 Con, Int, and Wis instead of
just -2 to one of these."

- **What was wrong.** For the alternate heritages (Lavasoul, Sunsoul, Gemsoul, Ironsoul, Smokesoul, Stormsoul,
  Mistsoul and Rimesoul), the Racial Bonus column in character creation and level-up showed a penalty that was not
  applied. A Stormsoul Sylph showed +2 Dex, +2 Cha and -2 Con, Int and Wis, while the character actually had +2 Dex,
  +2 Cha and -2 Wis. Some labels were worse: Gemsoul showed Cha -2 for an actual +2. The console (controller) stat
  view showed the same wrong value.
- **Cause.** An alternate heritage is stored as the race's General adjustments plus the difference to the chosen
  heritage, for example Intelligence +2 and -2 for Stormsoul. The game adds these up correctly for the ability score,
  but its Racial Bonus label shows only the first, most negative entry.
- **Repair.** For the four elemental races, the label now shows the game's own combined racial value for each ability.
  No racial bonus shows "-". Ability scores, point-buy, heritage choices and saves are not changed, and other races
  keep the game's usual label.

The General heritages were labelled correctly before and are unchanged. The character sheet shows ability totals but
no Racial Bonus column, so it was not affected.

## Existing saves

Nothing in this release changes, adds or removes any identity, and the repair changes only what a label displays.
Existing elemental characters, including after a respec, show the corrected labels with no save migration. This also
applies with the Elemental Races module switched off, as long as the races stay registered for existing saves.

## Disclosed open items

- **Tested with the developer's own mod set.** The checks ran on the developer's installation with its usual mods.
  The reporting player's version and other mods are unknown.
- **Console screen.** The console (controller) value was checked through the game's own stat model; the console
  screen itself was not opened in controller mode.
- **Routes tested in the game.** Character creation and native respec were run in the game for the Sylph. All twelve
  heritages were checked through the game's own label code, and saved characters of every race, heritage and sex were
  reloaded with the module on and off.
- **Tested on a development build.** The checks before release ran on a development build of the same source. The
  release build is checked in the game after publication, and that result is recorded in the repository's release
  record.
- **Earlier items.** The disclosures in the v0.0.195 notes still stand, unchanged.
