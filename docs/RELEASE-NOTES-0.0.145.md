# Kingmaker Gunslinger 0.0.145: Heirloom Nodachi icon

This owner-authorized release corrects one cosmetic defect on top of the
0.0.144 weapon findability release.

The **Heirloom Weapon: Nodachi** trait option and its three choices
(Proficiency, AOO Bonus and CMB Bonus) now show the mod's Nodachi artwork,
the same icon the Nodachi items use. Previously they captured the native donor
sword sprite before the project icon was applied, so the trait menu showed a
falchion-like blade. When Eastern Weapon presentation is disabled, the trait
keeps matching the item's native fallback icon as before.

Blueprint identities, weapon mechanics, proficiencies, trait benefits and the
selection structure are unchanged. All other 0.0.144 content, including the
weapon findability moves and per-weapon recovery, is carried forward unchanged.

## Qualification and remaining limits

At the owner's explicit request, no test suites, repository validation or
runtime scenarios were run for 0.0.145. The merged source was compiled with the
exact-reference Release build and packaged. The trait icon has **not** been
inspected in-game. The inherited 0.0.143 qualification and its recorded limits
are unchanged; see the
[0.0.144 release notes](RELEASE-NOTES-0.0.144.md) in the repository and the
[qualification report](WEAPON-FINDABILITY-QUALIFICATION.md).

## Installation

Download **KingmakerGunslinger-0.0.145-heirloom-nodachi-icon.zip** from the
release's Assets and install it through Unity Mod Manager's Mods tab. Confirm
Gunslinger **0.0.145** is enabled. The GitHub Source code archives are not the
installable mod package. The release includes SHA256SUMS.txt and an artifact
manifest for the exact published archive.
