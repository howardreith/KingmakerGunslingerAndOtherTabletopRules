# Kingmaker Gunslinger 0.0.144: Weapon findability fixes

This owner-authorized release promotes the qualified 0.0.143 implementation
with a version increment. Gameplay mechanics, canonical item identities,
crafting identity, prices, enchantments, module ownership and assets are unchanged.

The Last Word and Watch at the World's End now appear in ordinary first-floor
House at the Edge of Time chests before the final chapter. Unfixed Form moves to
Whiterose Abbey; Moonlit Crossing to Littletown's piers barrel; Winter Reed to
Empty Skull Rock's eastern cave bone pile; Drawn Horizon to Lake Silverstep
Village's southeastern shore travel pack; and Thunder at the Gate to Armag's
Tomb's outside supply crate. Original treasure is preserved.

Installed scene inspection also corrected Paper Lantern's fort chest and The
River King's Measure's palace chest: their former tables had no scene objects.
The [complete inventory](../planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md)
describes all 29 named world-loot weapons, native treasure and access conditions.
Roadwarden and Dead Reckoning remain merchant-only.

Old campaigns are not automatically repaired. The Gunslinger development panel
provides **Missed relocated campaign weapon recovery**: select a weapon, inspect
known ownership and destination progress, acknowledge unknown historical
ownership, choose **Recover selected weapon once**, then save normally. Known
canonical or supported upgraded copies refuse recovery without inventory changes.
A per-save, per-weapon ledger prevents repeated grants. See the
[supported invocation and limitations](WEAPON-FINDABILITY-RECOVERY.md).

## Existing qualification and remaining limits

The 0.0.143 implementation passed repository validation, 2,301 domain tests,
clean Release compilation, package checks and guarded Steam-based Kingmaker
2.1.7b fixtures. All 29 weapons passed blueprint/native-treasure/active-scene
checks; 28 passed normal approach, pickup, revisit and fresh-process save/reload.
All seven requested moves and both additional corrections passed those gates.
Recovery passed 412 preparation and 33 reload assertions, including canonical
and actual Craft Magic Items upgraded-copy refusal. Native Roadwarden and Dead
Reckoning purchase and persistence passed.

**Spear of the First Branch's route, pickup and persistence remain UNVERIFIED.**
The native fixture stopped at the navigation-region connection in the Ravaged
Capital's Central Passage. Its placement is unchanged. Better Vendors 2.0.8 is
installed and Ready, but authentic kingdom-stage acceptance is **NOT RUN** because
the required authentic save is unavailable. The
[qualification report](WEAPON-FINDABILITY-QUALIFICATION.md) and curated evidence
retain their actual sampled versions and provenance.

At the owner's explicit request, no additional test suites, repository validation
or runtime scenarios were run for 0.0.144. This release compiles and packages the
merged source; it does not claim a new 0.0.144 runtime qualification.

## Installation

Download **KingmakerGunslinger-0.0.144-weapon-findability-fixes.zip** from the
release's Assets and install it through Unity Mod Manager's Mods tab. Confirm
Gunslinger **0.0.144** is enabled. The GitHub Source code archives are not the
installable mod package. The release includes SHA256SUMS.txt and an artifact
manifest for the exact published archive.
