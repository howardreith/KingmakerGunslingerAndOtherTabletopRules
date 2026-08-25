# Kingmaker Gunslinger 0.0.98

This release adds optional compatibility with Craft Magic Items 2.1.0 while
preserving standalone Gunslinger behavior. The complete package is
`KingmakerGunslinger-0.0.98-craft-magic-items-compatibility.zip`.

## Craft Magic Items compatibility

- The bridge detects active UMM mod ID `CraftMagicItems`, validates the loaded
  `CraftMagicItems.Main` contract by shape, and integrates at CMI's complete
  data-loading and index-building lifecycle. It works across either mod load
  order and fails closed without affecting ordinary Gunslinger behavior.
- Dedicated mundane and magic **Firearms** categories contain Pistol, Musket,
  Blunderbuss, Advanced Rifle, and Advanced Revolver only when each production
  firearm is currently player-authorized.
- Wakizashi and Katana use Exotic Weapons, Nodachi uses Martial Weapons, and
  Elven Branched Spear uses Exotic Weapons. Only canonical generic bases are
  available from scratch; authored variants are exact upgrade targets and all
  named campaign uniques remain owned-item upgrades only.
- **Firearm Ammunition** recipes create the exact Black Powder Charge, Lead
  Ball, and Paper Cartridge blueprints in batches of 20 using CMI's mundane
  economics.
- The existing KMG `Reliable` enchantment is exposed once with +1 equivalent
  bonus and caster level 8. Its final creation gate requires exactly one
  canonical firearm-definition marker, including on CMI clones, and rejects
  every non-firearm.
- CMI custom upgrades preserve the firearm type, marker, proficiency,
  presentation, reload/capacity components, item-owned loaded state, Eastern
  and spear category mechanics, and the unmodified base blueprint.
- The Gunslinger UMM panel shows a read-only bridge state. Registration is
  idempotent and graph mutations roll back if an external capability fails.

The exact tested authority was CMI 2.1.0 built unchanged from upstream commit
`72f87523d0a116f5dfc92c91893d4955fa1eb303`. Its `CraftMagicItems.dll` SHA-256
is `4AE2DA61470350B31BEEF162717A604C9CCD322F66193917944EA4A9596E392D`
and MVID is `0044a45b-3bca-439e-86c5-a6aa4d42855e`. This was a local build of
the exact source, not a downloaded official release binary. No CMI binary,
source, data, localization, or icon is included in the Gunslinger package.

## Compatibility and persistence

- Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b
- Unity Mod Manager 0.32.4 / supported 0.32.x line
- Harmony 1.2 for Gunslinger; CMI's reflected Harmony 2 surface is validated
- .NET Framework 4.7
- Windows Steam installation used for guarded qualification

The already-qualified firearm SoundBank remains byte-identical at SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

CMI-generated items use CMI's own custom-blueprint persistence and may require
both mods to remain installed. Back up saves before crafting, upgrading, or
removing either mod. The tabletop Reliable prerequisite is *mending*;
Kingmaker 2.1.7b has no usable Mending blueprint, so this integration does not
invent a substitute prerequisite spell.

Install the complete package through Unity Mod Manager. Do not overlay files
onto an older installation, and do not use GitHub's generated source archive as
the mod package.
