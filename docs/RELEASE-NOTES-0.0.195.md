# Tabletop Added Rules v0.0.195

This stable release collects the save, lifecycle-cleanup and migration repairs reviewed since v0.0.194 (remediation
Sprint 6). Every identity from v0.0.194 is unchanged and none is appended, so existing saves keep their references.
Unity Mod Manager 0.31.1 or newer is still the minimum.

## Saves with damaged mod data

These two repairs matter only for a save whose mod data is malformed. No released or ordinary build has been shown to
write such data. Ordinary saves restore exactly as before.

- **Urban Barbarian Controlled Rage.** Malformed Controlled Rage data used to throw an error that could abort the save
  or that character's load, break every tier's turn-on, or break the action-bar name and icon. Now the save loads, and
  every tier whose saved allocation is clear keeps it exactly. A tier whose saved entry is invalid or conflicting is
  blocked: raging grants no Controlled Rage bonus for it, and no default is made up, until you choose an allocation
  with that tier's selector. The rejected text stays in the save, and the log warns about it.
- **Brown-Fur Transmuter Powerful Change.** One bad Powerful Change record could make the game skip the whole Brown-Fur
  record for that character at load, losing every valid increase with it, or abort the save. Now a bad record restores
  nothing and leaves the spell's own bonus alone, while valid records keep their exact increase, unless a rejected
  record in the same save could refer to them, in which case they are held back. The rejected record stays in the
  save, the log warns about it, and casting that spell again with Powerful Change creates fresh state.

## Brown-Fur Powerful Change after the caster leaves

If a Brown-Fur Transmuter companion cast a Powerful Change spell on another character and was then dismissed while
the spell still lasted, the spell kept running but lost its Powerful Change increase after the next save and load
(in the tested case, Bull's Strength dropped from +6 to +4). It now keeps its exact increase through loading, and
removing the buff forgets its record.

## Summon grapples and swallows

- **Saving keeps ongoing holds.** Saving the game, when the game allows it (out of combat), used to release a summon's
  ongoing grab, and a Purple Worm's, Giant Flytrap's, Dire Crocodile's or Tyrannosaurus's swallow, in the game you
  were playing. Those holds now continue after the save. Loading a save still releases holds safely instead of
  rebuilding them, as before.
- **Swallowed victims that die are released.** A creature that died inside a summoned Purple Worm or Giant Flytrap
  stayed swallowed, and the summon could not bite or swallow anything else. The dead creature is now released, as the
  Dire Crocodile and Tyrannosaurus already did.

## Retired vendor stock and player-sold copies

When the mod retired its older Elven Branched Spear and Eastern weapon listings at Oleg's (cold iron spears), the
Dire Narlmarches village vendor and Pitax, it could remove a copy that you had sold there yourself, because the game cannot tell that copy apart from the
vendor's own stock. Retired listings stay retired, but any copy whose origin cannot be proven is now kept. As a
result, some older vendor stock of these items may stay for sale; that is intended. Copies lost on earlier versions
are not restored.

## Existing saves

Nothing in this release changes, adds or removes any identity, so saves load as before.

- Earth Elementals summoned on v0.0.101 to v0.0.181 keep their old target preference, and Griffons summoned on v0.0.161
  to v0.0.179 keep their duplicate Charge, until the summon ends. The Bebelith's leftover marker effect from before
  v0.0.182 is kept but changes no armor class. Newly summoned creatures are not affected.

## Disclosed open items

- **Two old incidents remain unexplained.** A stall while loading certain old saves into a Tenebrous area, and a
  November 11 error while restoring buffs after loading, could not be traced because the original saves and log are
  not available. Neither is claimed fixed.
- **Tested on development builds.** Each change was checked in a disposable copy of the game on its own development
  build, with Unity Mod Manager 0.33.0 and the mod alone (with Call of the Wild for the Brown-Fur persistence check).
  Earlier changes were not re-checked on the final combined build, and the builds did not include v0.0.194's Bag of
  Tricks change. The release build is checked in the game after publication, and that result is recorded in the
  repository's release record.
- **Not checked.** The save change for the Dire Crocodile's and Tyrannosaurus's swallows (they share the repaired
  code but were not run), turn-based mode, other Unity Mod Manager versions, moving between areas with an ongoing hold,
  holds with three or four links, successful Dismissal, downgrading to an older version, and loading a save made on
  a pre-fix version for the Earth Elemental, Griffon and Bebelith cases.
- **Damaged-data limits.** Some damage is lost by the game before the mod sees it: a repeated property keeps only one
  value, some numbers and dates can lose digits or text, and a number too large to read can stop the game loading that
  character's mod data at all. This release does not make every damaged save recoverable.
- **Vendor and Thornstep checks** used constructed save data, not real old saves.
- **Earlier items.** The disclosures in the v0.0.194 notes still stand, unchanged.
