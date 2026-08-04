# Installation, updates, removal, and compatibility

## Supported baseline

This build is qualified against Pathfinder: Kingmaker Enhanced Plus Edition
2.1.7b on Windows through Steam, with Unity Mod Manager 0.32.4 or later in the
0.32.x line. Every real qualification launch used Steam App ID 640820.

The package has no bundled gameplay-library dependency. It requires the game,
its supported Unity Mod Manager installation, and the Harmony compatibility
assembly supplied by that environment. Do not copy game, Unity, UMM, Harmony,
or compiler assemblies into this mod folder.

## Clean installation

1. Back up any saves you intend to keep outside the game's active save folder.
2. Install the standalone `KingmakerGunslinger-0.0.64-local-runtime.zip` with Unity Mod
   Manager for Pathfinder: Kingmaker.
3. Do not install a source archive, repository snapshot, private reference
   bundle, compiler package, or framework reference archive.
4. Launch the game through Steam and verify that Unity Mod Manager reports
   Kingmaker Gunslinger version 0.0.64 without a red/broken load indicator.
5. Use a new or disposable save until the build's known limitations are
   acceptable for your campaign.

Do not manually merge individual files into an existing mod directory. The
strict package contains one complete `KingmakerGunslinger` root.

## Updating

1. Exit Kingmaker completely.
2. Back up affected saves and the currently installed mod package/version.
3. Use Unity Mod Manager to replace the complete `KingmakerGunslinger` folder
   with the new standalone package; do not overlay selected files.
4. Launch through Steam and verify the displayed mod version before loading a
   campaign.
5. First validate the update with a copied or disposable save.

Stable published blueprint identities and historical compatibility adapter
types are retained, but arbitrary downgrades are not qualified. Do not load a
save written by a newer mod version after downgrading unless that exact path is
explicitly documented as qualified.

## Removal warning

There is no uninstall cleanup or general uninstall-safe-save claim. Saves may
retain references to the Gunslinger class, progression features, abilities,
resources, firearm/ammunition/repair-kit blueprints, and item-owned firearm
state-token enchantments. Removing the mod while such references remain can
make a save fail to load or leave missing/invalid content.

The safe default is to keep the same or a compatible newer mod version
installed for every campaign that has used Gunslinger content. For a clean
removal, return to a backup made before the mod was introduced or start a new
campaign without it. Deleting visible items or respeccing a character is not
proof that every serialized reference has been removed.

Never test removal against the only copy of a valued save.

## Compatibility boundaries

- This package includes approved Pistol, Musket, Blunderbuss, and Revolver models
  and five approved SSE Library CC0 firearm sounds in a Unity 2018.4.10f1 bundle.
  The quarantined advanced-rifle binary is not packaged; Advanced Rifle retains
  the safe native visual fallback and its approved temporary long-gun sound map.
- The mod patches native attack, armor-class, damage, rest, initiative, save,
  skill, equipment, and level-up flows. Mods changing the same callbacks may
  conflict depending on patch order and behavior.
- The capital-vendor integration appends entries to one exact native vendor
  table. Mods replacing that table or its publication timing may conflict.
- No compatibility is claimed for mods that replace the Gunslinger class
  identity, reuse this project's published blueprint GUIDs, rewrite firearm
  item enchantments, or alter the same saves outside Kingmaker's normal APIs.
- Call of the Wild, Cowboys and Demons, BlueprintCore, Wrath Modification
  Template, and other gameplay mods are neither dependencies nor currently
  qualified compatibility targets.
- The approved asset bundle was built with the locally installed, licensed Unity
  2018.4.10f1 editor. Missing or corrupt bundle data falls back safely and does
  not change firearm rules identity, save schema, or reload action economy.
- Empty firearm requests are rejected during native attack-command construction.
  When the applicable Reload Firearm variant is selected for native automatic use,
  the request produces one normal `UnitUseAbility` reload command instead.

When diagnosing a conflict, reproduce it first on a copied save with only
Unity Mod Manager and Kingmaker Gunslinger enabled. A clean mod load does not
by itself prove campaign or cross-mod compatibility.
