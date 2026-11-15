# Tabletop Added Rules v0.0.190

This stable release repairs a startup failure that removed the Gunslinger class, and the rest of the mod's content
for that session, on some installations. It also makes ten Gunslinger status effects readable and visible. Every
identity from v0.0.189 is unchanged and none is appended, so existing saves keep their references. Unity Mod Manager
0.31.1 or newer is still the minimum.

## Gunslinger class missing from character creation

On an affected installation the mod loaded and showed as active, but the Gunslinger was not offered in character
creation and none of the mod's content was available. The log contained
`Player-facing fact has incomplete localization: KMG_BleedingWound_HitPoints_Armed`.

- **Cause.** Eleven Gunslinger status effects had no name or description. The game shows `<null>` for such text, and a
  startup check accepted that. Where the missing text resolved to nothing instead, the check failed and startup
  stopped.
- **Repair.** The effects now have real names and descriptions, and the check no longer accepts stand-in text of any
  kind.

## Armed and used status effects

Ten Gunslinger status effects are now named, described and shown in the party bar:

- Bleeding Wound Armed (Hit Points, Strength, Dexterity, Constitution)
- Expert Loading Armed
- Lightning Reload Used
- Slinger's Luck Armed (Saving Throw, Skill Check)
- Death's Shot Armed
- Stunning Shot Armed

Before this release they appeared on the character sheet as `<null>` and did not appear in the party bar at all. They
keep their existing icons. What they do and what they cost is unchanged.

- **Lightning Reload Used ends on time.** This effect marks that Lightning Reload was used this round. It could stay
  on the character past the end of the round, until some other status effect changed. It now ends at the start of the
  next round.

## Cleanup after a failed start

If startup fails for any reason, the mod now puts back every change it made to the Protection from Alignment spells
and effects exactly, and reports anything it could not restore. Previously this cleanup stopped at its first step
and left most of those changes in place.

## Settings panel

The mod's Unity Mod Manager panel now starts with the loaded version and a line saying whether its content
initialized. It used to show an out-of-date build label and gave no sign when content had failed to load.

## Existing saves

Nothing in this release changes, adds or removes any identity, so saves load exactly as before.

## Disclosed open items

- **Reporting player's setup not reproduced.** One report of the missing class was received, from a heavily modded
  installation running a translation. The failure was reproduced with a controlled test condition, not with that
  player's mods and translation, and what made the difference on that installation is not known. The repair does not
  depend on it.
- **Tested on a development build.** The checks ran in the game on a development build of the same source. The
  released build itself was not run in the game.
- **Not played in combat.** The ten effects were applied to a character and checked in the party bar and on the
  character sheet. Arming them through their abilities as a Gunslinger and spending them on an attack was not played.
- **Other status effects and the party bar.** Some other status effects from this mod are built the way these ten
  were, and probably also work without appearing in the party bar: Deadeye Armed, Gunslinger's Dodge, the Bleeding
  Wound bleed on a target, Startling Shot, Targeting (Head), the Pistolero and Musket Master armed effects, and one
  each from Eastern Weapons, Elven Branched Spears and Expanded Summoning. They are unchanged in this release and
  were not tested.
- **Earlier items.** The disclosures in the v0.0.189 notes still stand, unchanged.
