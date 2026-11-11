# Tabletop Added Rules v0.0.183

This stable release applies six Gunslinger playtest repairs: a cleaner spellbook, clearer deed descriptions, and
two acquisition changes for elven branched spears. Every identity from v0.0.182 is unchanged and none is appended,
so existing saves keep their references. The summon roster and its 1,208 public choices are unchanged.

## Spellbook: no elemental disclosure row

The dark "Elementals (n) — show/hide original sources" row is gone from the spellbook, on PC and with a controller.
Every original Summon Elemental source is listed again as an ordinary spell row, and preparing, selecting and
casting them works as before. The action-bar elemental source chooser and the level-up spell-learning list are
unchanged.

## Clearer deed descriptions

Mechanics, costs and ranges are unchanged; only the text was rewritten.

- **Deadeye Armed** (the status shown while armed): "Your next firearm attack targets touch AC even beyond the
  weapon's first range increment. Normal range penalties still apply. This effect ends after 1 round or after that
  firearm attack, even if it misses." It no longer says "You paid 1 Grit". The Deadeye ability and feature use the
  same wording.
- **Gunslinger's Dodge:** "As a swift action, spend 1 grit to gain a +2 dodge bonus to AC for 1 round." The note
  about movement and being knocked prone is gone; the deed never did either.
- **Focused Aim:** "As a swift action, spend 1 grit. Until the end of your turn, add your Charisma modifier
  (minimum +1) to firearm damage rolls. When you use Dead Shot, add this bonus once for each of Dead Shot's attack
  rolls that hits. For example, with Charisma +3 and two Dead Shot hits, Focused Aim adds +6 damage to that shot."

## Branched spears: Oleg and Thornstep

- **Oleg** no longer sells either cold iron branched spear; he keeps the ordinary and masterwork ones. Other
  vendors, their spear stock and anything you carry are unchanged. In an existing save, his unsold cold iron
  spears are removed from his stock when it next updates.
- **Thornstep** is no longer placed in the campaign. It no longer appears in its former Riverine Rise container and
  is no longer offered by weapon recovery. The weapon itself is unchanged, so a Thornstep you already own keeps
  working.
- **Thornstep in older saves.** If the reward is still untouched in its Riverine Rise container, it is removed the
  next time that area loads. A Thornstep you took and later put back in that container stays, as does everything
  else in it.

Boughkeeper and every other named weapon keep their placements.

## Known limitations

- **A spear sold to Oleg.** Vendor stock records only item types and counts. If you bought Oleg's stocked cold iron
  spear and later sold a cold iron spear back to him, that sold copy is removed from his stock when the save
  updates. Spears you carry are never touched.
- **A Thornstep the game added later.** If the Riverine Rise container was generated before Thornstep was placed
  there, a Thornstep the game added to it afterwards is removed like the original reward, because it was also
  never in your party's hands.

## Disclosed open items

- **Not observed in game.** None of the six repairs was observed in game, and no development candidate was built or
  run. That covers:
  - the spellbook rows through refresh and reload, on PC and with a controller;
  - the Deadeye Armed, Gunslinger's Dodge and Focused Aim tooltips as displayed, and a Dodge activation;
  - Oleg's stock in a fresh and an existing save;
  - Thornstep's former container in a fresh and an existing save;
  - legacy owned copies.
- **Evidence.**
  - Focused tests: 153 of 2,948, 0 failures.
  - Harmony target checks against the installed game files, without launching it: spellbook 40/40, Thornstep
    reconciliation 1/1.
  - The existing-save behavior follows the decompiled game code, and offline tests model it.
- **Earlier items.** The disclosures in the v0.0.182 notes still stand.
