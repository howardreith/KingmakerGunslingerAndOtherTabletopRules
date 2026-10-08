# Kingmaker Gunslinger 0.0.143: Weapon findability fixes

Candidate informational version: 0.0.143-weapon-findability-fixes.

This candidate starts at audited 0.0.142 commit
4ba8d4aca087391144abf401f526189f59b26535 and preserves its unrelated work.

The Last Word and Watch at the World's End move to ordinary first-floor House
at the Edge of Time chests. Unfixed Form moves to Whiterose Abbey; Moonlit
Crossing to Littletown's piers barrel; Winter Reed to Empty Skull Rock's eastern
cave bone pile; Drawn Horizon to Lake Silverstep Village's southeastern shore
travel pack; and Thunder at the Gate to Armag's Tomb's outside supply crate.
Canonical item identities, properties, crafting identities and module ownership
are retained. Publication removes only the affected mod-owned rows from retired
loot definitions and preserves the destination's native treasure.

Installed scene checks also exposed two definitions with no physical source.
Paper Lantern now uses the fort's actual Glaive +1 chest with the literal `#2`
suffix. The River King's Measure now uses the actual palace chest with the
literal `#1` suffix, containing a Wooden Spoon, Grinding Stone and 16 gold.
Their walking and pickup qualification is tracked separately from scene presence.

The complete inventory contains 29 named world-loot weapons and Cord of
Stubborn Resolve: 30 distinct containers in 28 exact areas. Three named weapons
share the normalized House group, with the two firearms on its first floor.

Blueprint publication, scene presence, an ordinary route, normal interaction and
pickup, revisiting, and save/reload are separate qualification gates. Missing
scene or route evidence is UNVERIFIED. The native 2.1.4 reference is a comparison
source; installed 2.1.7b runtime evidence is required for physical qualification.

Changing definitions does not guarantee repair of previously generated loot.
The development panel provides explicitly invoked, one-weapon recovery for
eligible relocated weapons. It inspects known ownership and destination progress,
refuses known canonical or supported upgraded copies, and records recovery on
the main character's persisted unit part. Unknown historical ownership remains
unknown; the ledger prevents repeated recovery grants, not all historical copies.

Installed 2.1.7b qualification passed 29/29 blueprint/source and ordered
native-treasure checks, 29/29 active persistent scene checks, and
28/29 complete native route/pickup/revisit/disk-reload checks. The seven
requested replacements passed all those gates. Recovery passed native
412-assertion preparation and 33-assertion fresh-process verification, including
canonical/upgraded-copy refusal and all 28 ledgers. See
[exact per-placement evidence and remaining limits](WEAPON-FINDABILITY-QUALIFICATION.md).
Optional authentic Better Vendors kingdom-stage acceptance is NOT RUN because
its required save is unavailable. No merge, tag or GitHub release is performed.

Candidate archive: `KingmakerGunslinger-0.0.143-weapon-findability-fixes.zip`.
The inherited firearm SoundBank remains byte-identical, SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Existing `CraftMagicItems.dll` integration is preserved; recovery recognizes its
supported named-item upgrade identity without changing crafting distribution.

Historical inherited gates preserve the earlier 1,288-test content checkpoint
and the 1,325-test fatigue-authority checkpoint. These counts do not describe
this candidate's domain suite or requalify earlier runtime artifacts.
