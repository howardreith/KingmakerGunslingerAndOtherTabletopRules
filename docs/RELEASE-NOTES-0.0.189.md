# Tabletop Added Rules v0.0.189

This stable release repairs the **Elementals** button that Expanded Summoning adds to the action bar's spell popup.
Nothing else changes: every identity from v0.0.188 is unchanged and none is appended, so existing saves keep their
references, and the summon roster and its 1,208 public choices are untouched. Unity Mod Manager 0.31.1 or newer is
still the minimum.

## Elementals button in the spell popup (mouse and keyboard)

Expanded Summoning gathers a caster's original Summon Elemental spells under one Elementals button, which opens a
chooser for the element.

- **No more dead icon.** A spell popup could show an extra Elementals icon that did nothing when clicked. The button
  now appears only when an original Summon Elemental spell has actually been gathered under it. Otherwise the row
  shows the spells exactly as the game does.
- **Works while paused.** The chooser now opens while the game is paused, as well as during normal play. Casting from
  it still follows the game's own targeting rules.
- **Never leaves a spell out of reach.** If the chooser cannot open, for example in another game mode or while the
  keyboard is held by another window, the original Summon Elemental spells return to the row with a short warning.
  You can cast them as usual.
- **Keyboard always comes back.** The chooser blocks keyboard shortcuts only while it is open. It gives them back when
  it closes, including after an error or when the action bar is rebuilt while it is open. It never releases a block
  that belongs to another window.

## Existing saves

Nothing in this release changes, adds or removes any identity, so saves load exactly as before. Prepared and known
spells are not touched.

## Disclosed open items

- **Not tested in the game.** This repair was checked with automated tests and a check that every game method it hooks
  exists in the game. It was not played in the game before release: the paused casting, the warning and spell return,
  keyboard recovery and the action bar being rebuilt with the chooser open have not been observed in play.
- **Controller.** The controller version of the chooser is not part of this repair. It can still offer an Elementals
  entry when no original Summon Elemental spell was gathered.
- **Earlier items.** The disclosures in the v0.0.188 notes still stand, unchanged: the untested Unity Mod Manager
  versions, the hand-maintained inherited summon template list and the two feature-module self-checks that fail on a
  heavily modded installation.
