# Tabletop Added Rules v0.0.187

This stable release renames the mod in Unity Mod Manager. Nothing in play changes: every identity from
v0.0.186 is unchanged and none is appended, so existing saves keep their references, and the summon
roster and its 1,208 public choices are untouched.

## New name in Unity Mod Manager

Unity Mod Manager now lists the mod as **Mr. Bard's Tabletop Rules Expansion** instead of
**Kingmaker Gunslinger**. Its settings panel, with the feature-module checkboxes, is under the new name.

Only the displayed name changed. The mod's internal ID is still `KingmakerGunslinger`, and so are its
folder, DLL and settings file. To update, install over the existing mod as usual. Unity Mod Manager
recognizes it as the same mod, and your module settings carry over.

## Existing saves

Nothing in this release changes, adds or removes any identity, so saves load exactly as before.

## Disclosed open items

- **Not observed in game.** The new name was checked in the mod's `Info.json`, which Unity Mod Manager
  reads, but not looked at in the Unity Mod Manager window itself.
- **Earlier items.** The disclosures in the v0.0.186 notes still stand, unchanged: the hand-maintained
  inherited summon template list and the two feature-module self-checks that fail on a heavily modded
  installation.
