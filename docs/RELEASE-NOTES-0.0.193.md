# Tabletop Added Rules v0.0.193

This stable release repairs a startup failure, reported against v0.0.188 and present through v0.0.192. On an affected
installation it removed the mod's content for that session, left Summon Monster I without its expanded creature
list, and stopped weapon tooltips from opening. Every identity from v0.0.192 is unchanged and none is appended, so
existing saves keep their references. Unity Mod Manager 0.31.1 or newer is still the minimum.

## Startup failure, summoning and weapon tooltips

On an affected installation the mod loaded and showed as active, but:

- the log contained `Elven Branched Spear type profile is invalid` during startup, and the mod's content was not
  available for that session;
- Summon Monster I and the other summoning spells showed only Kingmaker's own creatures, not the expanded list;
- hovering over a weapon in the inventory showed no tooltip, and the log filled with
  `The battered origin token blueprint is unavailable`.

What was wrong, and what changed:

- **Cause.** The Elven Branched Spear was built as a copy of Kingmaker's own Longspear and then checked against the
  values it expected a longspear to have. When the game's Longspear carried different values (for example a
  different reach or weapon group), the check failed and the whole startup stopped. Expanded Summoning is set up
  after the spear, so it never registered.
- **Repair.** The spear's own reach, weapon group, damage and other properties are now set on the spear itself
  instead of being inherited from the game's Longspear. The game's Longspear and other weapons are left untouched.
- **Clearer diagnostics.** If the spear's own profile really is wrong, startup still stops, as before. The log now
  names each failed check, with the expected, actual and Longspear values, and the mod responsible when another mod
  changed the value.
- **Tooltips and prices after a failed startup.** If startup does fail for any reason, ordinary weapons now keep
  their normal prices and tooltips. The log gets a single warning instead of an error on every hover. Battered
  firearms keep their special price whenever the mod started normally.

Spear and firearm values, names, descriptions and prices are unchanged.

## Existing saves

Nothing in this release changes, adds or removes any identity, so saves load exactly as before. Saved spears and
battered firearms load and price as in v0.0.192, including with the Gunslinger option turned off.

## Disclosed open items

- **Reporting player's setup not reproduced.** The report came from an installation with 43 mods, and its full log
  was not available. The failure was reproduced by deliberately changing the game's Longspear in a test copy of the
  game, not with that player's mods. Which value differed there, and which mod or change caused it, is not known.
  Four of the reported mods together did not reproduce it. The repair removes the step that failed.
- **A mod that changes the spear itself still stops startup.** If another mod changes the Elven Branched Spear's own
  reach, group or other checked values, startup still fails, now with a message naming the check and that mod. This
  is deliberate, so that a broken spear is never published silently.
- **Eastern Weapons.** Eastern Weapons still copies some properties from the game's own weapons and checks one of
  them. It was not reported failing and is unchanged.
- **Tested on a development build.** The checks before release ran in a test copy of the game on a development build
  of the same source. The release build is checked in the game after publication, and that result is recorded in the
  repository's release record.
- **Earlier items.** The disclosures in the v0.0.192 notes still stand, unchanged.
