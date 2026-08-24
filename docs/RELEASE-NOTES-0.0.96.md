# Kingmaker Gunslinger 0.0.96

This patch release restores Kingmaker Gunslinger's project-owned firearm sound
effects on Pathfinder: Kingmaker 2.1.7b while preserving the existing firearm
mechanics, event catalog, and SoundBank bytes.

## Firearm audio restoration

- The production manifest loader now parses the canonical schema-1 JSON through
  an exact private contract. Another loaded component's process-global
  `JsonConvert.DefaultSettings` can no longer rename or suppress the required
  manifest properties.
- Manifest parsing remains strict: missing, null, duplicate, mistyped,
  malformed, unknown, and unsupported schema data fails closed without rolling
  back or changing a committed firearm attack.
- Startup logs now identify manifest read and validation, bank staging/hash
  parity, Wwise readiness, and the single process-lifetime bank load attempt.
- Pistol, Musket, Blunderbuss, Revolver, and Rifle retain their five canonical
  Wwise Event mappings. Ordinary committed hits and misses report once;
  misfires, Empty/Wrecked, canceled, and otherwise uncommitted attacks remain
  silent; Scatter reports once per committed volley.
- `KMG_Firearms.bnk` is preserved byte-for-byte at SHA-256
  `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
  The package contains no `Init.bnk`, additional bank, or external `.wem` file.

The guarded save-free audio qualification passed every focused routing and
attack-boundary assertion. The repository owner then listened to the installed
qualified implementation, reported that the sound effect was working, and
approved this release. Wwise playing IDs alone were not treated as audible
evidence.

## Compatibility

- Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b
- Unity Mod Manager 0.32.4 / supported 0.32.x line
- Harmony 1.2 through `0Harmony12.dll`
- .NET Framework 4.7
- Windows Steam installation used for qualification

The mod remains standalone. Call of the Wild is required only for the optional
Brown-Fur Transmuter module. This release does not claim compatibility with
every version of every third-party mod.

## Updating

Close Kingmaker, back up affected saves and the installed mod, then install the
complete `KingmakerGunslinger-0.0.96-firearm-audio-restoration.zip` through
Unity Mod Manager. Do not overlay individual files onto an older installation,
and do not download GitHub's automatically generated source archives as the mod
package.

## Save warning

Kingmaker Gunslinger publishes save-owned classes, archetypes, feats, spells,
buffs, items, weapon categories, summons, enchantments, resources, and firearm
state identities. Keep this version or a compatible newer version installed for
campaigns that use its content. Uninstalling the complete mod from such a
campaign is not generally safe.
