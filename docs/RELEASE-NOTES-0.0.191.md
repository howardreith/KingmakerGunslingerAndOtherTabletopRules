# Tabletop Added Rules v0.0.191

This stable release repairs a second startup failure that removed the Gunslinger class, and the rest of the mod's
content for that session, on some installations. It also repairs the status effects of four named Elven Branched
Spears. Every identity from v0.0.190 is unchanged and none is appended, so existing saves keep their references.
Unity Mod Manager 0.31.1 or newer is still the minimum.

## Gunslinger class still missing after v0.0.190

On an affected installation v0.0.190 got further through startup than v0.0.189 and then failed again. The mod loaded
and showed as active, but the Gunslinger was not offered in character creation and none of the mod's content was
available. The log contained a `NullReferenceException` under `GritAbilityUiIntegration`.

- **Cause.** To decide which Gunslinger abilities show remaining grit on the action bar, the mod read the description
  of nearly every ability it adds, including thousands of summoning abilities that have nothing to do with grit. On
  an installation where one of those descriptions comes back empty in a particular way, the game's own description
  code fails, and startup stopped there.
- **Repair.** The mod now uses a fixed list of the 24 abilities concerned and reads no descriptions for this.

The grit counter appears on the same abilities as before. What deeds cost and when grit is spent is unchanged.

## Named Elven Branched Spear effects

The effects granted by Boughkeeper, Thornstep, Viper's Reach and First Branch are repaired:

- **Shown at once.** Boughkeeper's armor-class bonus now appears beside the portrait on the first attack of
  opportunity that hits. It used to appear only after a second hit.
- **End on time.** These effects now end after one round. Boughkeeper's +1 to Armor Class could stay on the
  character for several rounds.
- **Icons.** Boughkeeper's bonus and the Thornstep, Viper's Reach and First Branch's Reprisal penalties now show the
  Elven Branched Spear icon. They were drawn as blank white squares.

Their names, descriptions, values and triggers are unchanged.

## Existing saves

Nothing in this release changes, adds or removes any identity, so saves load exactly as before.

## Disclosed open items

- **Reporting player's setup not reproduced.** The missing-class report came from one heavily modded installation
  running a translation. The failure was reproduced with a controlled test condition that produces the same error at
  the same place, not with that player's mods and translation. Which ability failed there, and why, is not known. The
  repair removes the step that failed.
- **Three summoning abilities with no description.** Three hidden abilities of summoned devils have no description
  text. The repaired startup no longer reads them. On an affected installation, anything else that asked for their
  description would fail the same way. They are never shown to the player.
- **Tested on a development build.** The checks before release ran in the game on a development build of the same
  source. The release build is checked in the game after publication, and that result is recorded in the repository's
  release record.
- **Grit spending checked for some deeds.** Spending was checked in the game for Deadeye, Dead Shot, Expert Loading,
  both Slinger's Luck uses and the two Pistolero deeds. Gunslinger's Dodge has no passing check: its automated test
  fails before it measures anything, on v0.0.190 as well. One Pistolero damage check gave different results between
  identical runs; its cause is not established, and its grit checks passed every time.
- **Which deeds show the grit counter.** Steady Aim needs grit and spends none, and shows the counter. Quick Clear as a
  standard action, Utility Shot, Startling Shot and Lightning Reload are in the same position and do not. The counter
  also shows remaining grit, whatever a deed costs. This is unchanged from earlier versions.
- **Other status effects and the party bar.** Some other status effects from this mod probably still work without
  appearing beside the portrait: Deadeye Armed, Gunslinger's Dodge, the Bleeding Wound bleed on a target, Startling
  Shot, Targeting (Head), the Pistolero and Musket Master armed effects, and one each from Eastern Weapons and
  Expanded Summoning. They are unchanged in this release and were not tested.
- **Earlier items.** The disclosures in the v0.0.190 notes still stand, unchanged.
