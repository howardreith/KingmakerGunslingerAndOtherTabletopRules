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
2. Install the standalone `KingmakerGunslinger-0.0.82-brown-fur-human-review-repair.zip`
   with Unity Mod Manager for Pathfinder: Kingmaker.
3. Do not install a source archive, repository snapshot, private reference
   bundle, compiler package, or framework reference archive.
4. Launch the game through Steam and verify that Unity Mod Manager reports
   Kingmaker Gunslinger version 0.0.82 without a red/broken load indicator.
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

With Call of the Wild installed, Focused Weapon exposes each KMG custom weapon
category only when the level-up unit owns exact Weapon Focus for that category.
Elven Branched Spear, Wakizashi, Katana, and Nodachi each passed singular
eligibility and actual native damage-die effect checks; absence of matching
Weapon Focus remains an exact negative control.

## Feature-module settings

The UMM panel contains independent Gunslinger, Acadamae Graduate, Shield Other,
Expanded Summoning, Elven Branched Spears, Eastern Weapons, and `Brown-Fur
Transmuter  requires Call of the Wild` checkboxes. All default ON. A change is
saved for the next complete game restart; the panel reports active and saved
next-restart state separately. Older settings migrate to schema 6 while
preserving every explicit prior value and defaulting the absent Brown-Fur key
ON.

Brown-Fur is the only module that requires Call of the Wild. Its adjacent
status reports Available, Unavailable, or Blocked independently from saved user
intent, effective publication, and restart-required state. Saved ON intent is
preserved when CotW is absent, but Brown-Fur is not registered or published.
Structurally incompatible CotW installations also fail closed without disabling
the package's six independent modules.

Turning a module OFF removes its content only from new public choices and
acquisition paths. It does not unregister identities, strip an existing
Gunslinger, remove owned feats/items, or uninstall firearm state support. An
owned Elven Branched Spear and an already selected spear feat therefore remain
loadable and mechanically coherent while that module is OFF. With compatible
CotW still installed, Brown-Fur OFF likewise hides the archetype from new
character creation and respec while retaining its stable identities and the
features and effects of existing Brown-Fur owners. Do not remove the entire mod
from a campaign that has used any module.

## Removal warning

There is no uninstall cleanup or general uninstall-safe-save claim. Saves may
retain references to the Gunslinger class, progression features, abilities,
resources, firearm/ammunition/repair-kit blueprints, item-owned firearm
state-token enchantments, summon abilities, summon units, Elven Branched Spear
items, the spear category, selected spear features, Eastern Weapon items and
categories, selected Eastern proficiency or chosen-weapon facts,
enchantments, and buffs.
Removing the mod while such references remain can
make a save fail to load or leave missing/invalid content.

The safe default is to keep the same or a compatible newer mod version
installed for every campaign that has used Gunslinger content. For a clean
removal, return to a backup made before the mod was introduced or start a new
campaign without it. Deleting visible items or respeccing a character is not
proof that every serialized reference has been removed.

Never test removal against the only copy of a valued save.

Call of the Wild owns the parent Arcanist class. Removing CotW from a save that
contains a CotW Arcanist or Brown-Fur character is unsupported even if the
Brown-Fur module is subsequently disabled.

## Compatibility boundaries

Expanded Summoning 0.0.78 was requalified in isolated standalone, Call of the
Wild, Arms and Armor, Toggle Custom Soundpacks, and highest-risk combined
profiles. Its 77-icon creature catalog is project-owned and has no base-game
or optional-mod icon dependency; optional-mod
summon parents are discovered structurally and are skipped rather than guessed
when ambiguous. Every profile transaction restored the prior Mods directory.

The Elven Branched Spear release candidate passed all 32 five-module states and
an isolated Call of the Wild 1.14.4c-2.1 combat profile. That exact profile
proved the native spear selectors and Dexterity routes, optional Fighter's
Finesse and Trained Grace behavior, deliberate Dervish Dance exclusion, all
six named effects, and restoration of the prior Mods tree. The custom model is
project-owned; missing or rejected spear bundle data retains the native
Longspear presentation without changing mechanics or save identity.

Eastern Weapons passed all 64 six-module states and exact standalone, Call of
the Wild, Arms and Armor, Toggle Custom Soundpacks negative-control, and
maximum combined profiles. Katana grip-dependent proficiency, native fighter
training, Wakizashi finesse, all five bespoke effects, all eighteen native
enchantment arrays, Speed attack planning, Brilliant Energy exclusions,
commerce, fixed loot, and module-disabled persistence were exercised in live
guarded scenarios. Arms and Armor required one narrow, reflection-only bridge
for its hard-coded versatile-weapon classifier; the bridge recognizes only the
exact KMG Katana type and changes no foreign identity or blueprint. The exact
local Arms and Armor build contains no Katana, Wakizashi, or Nodachi provider,
so no duplicate-name or cross-proficiency bridge is needed. Every compatibility
transaction restored the complete Mods tree exactly.

The superseded `0.0.81` Brown-Fur Transmuter candidate was qualified against
the exact local Call of the Wild
1.14.4c-2.1 DLL SHA-256
`4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`
and MVID `8caab254-aacf-4811-8093-44b9184e6e53`. Structural resolution, rather
than the binary hash alone, gates publication. Both resolved normal exploit
replacements (levels 3/9) and balance-fixes replacements (levels 4/10) passed,
as did CotW absence, Brown-Fur OFF with an existing owner, the highest-risk
CotW plus Arms and Armor plus Toggle Custom Soundpacks profile, and all 16
seven-module boundary states. Unknown or ambiguous future CotW structures are
reported as Blocked and leave Brown-Fur unpublished. Human review rejected that
candidate for unclear toggle/resource presentation and immediate Personal-spell
self-casting. Version `0.0.82` repairs those paths. Its 1,138 domain tests,
40 focused runtime launches with 435 passing assertions, normal and balance
progressions, CotW absence, module-OFF persistence, highest-risk profile, and
all 16 boundary states pass on one immutable installed artifact. Human
presentation and play review accepted that exact artifact on 2026-08-16. The
revised runtime policy treats the 16-state boundary as authoritative and does
not require an exhaustive game-launch release matrix.

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
- Exact local Arms & Armor 1.0.10 (DLL SHA-256
  `CEC7C177819F8F68ADAC4CB24DF5834C862D0930D77305655AC3195097E33733`)
  passed isolated identity, Mysterious Stranger, visual-rig, and switching
  observations with Gunslinger 0.0.72. This claim does not cover other builds.
- Exact local Toggle Custom Soundpacks 1.0.1 (DLL SHA-256
  `A2582533DFDFF82D1ECE3EC51D931D72D7C8AAC9A1302C219FCD8FCA070C9434`)
  passed isolated identity and Wwise coexistence observations with Gunslinger
  0.0.72. This claim does not cover other builds.
- Exact local Call of the Wild 1.14.4c-2.1 (DLL SHA-256
  `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`)
  retains historical `CONFLICT-CONFIRMED` final-selector evidence: earlier human
  testing reached character creation with Call of the Wild classes present but
  Gunslinger absent. The 0.0.75 settings-aware reconciler subsequently passed
  exact ON/ON and Gunslinger-OFF/Acadamae-ON catalog publication plus full
  Acadamae/Cord mechanics. This is targeted exact compatibility evidence, not a
  claim that every older comprehensive or human selector scenario was rerun.
- Craft Magic Items is `STATIC-AUDITED-ONLY`; no compiled local root was
  supplied. Eddic Respec and Bag of Tricks are
  `UNAVAILABLE-LOCAL-REFERENCE`. No claim is made for them.
- The approved asset bundle was built with the locally installed, licensed Unity
  2018.4.10f1 editor. Missing or corrupt bundle data falls back safely and does
  not change firearm rules identity, save schema, or reload action economy.
- Empty firearm requests are rejected during native attack-command construction.
  When the applicable Reload Firearm variant is selected for native automatic use,
  the request produces one normal `UnitUseAbility` reload command instead.

When diagnosing a conflict, reproduce it first on a copied save with only
Unity Mod Manager and Kingmaker Gunslinger enabled. A clean mod load does not
by itself prove campaign or cross-mod compatibility.
# Custom firearm SoundBank

The release audio asset is copied only to
`Kingmaker_Data\StreamingAssets\Audio\GeneratedSoundBanks\Windows\KMG_Firearms.bnk`.
The mod hash-verifies the packaged source and existing destination and never
writes `Init.bnk` or another bank. To uninstall this optional native-audio
artifact, remove only `KMG_Firearms.bnk`; do not remove any vanilla bank.
Missing or rejected custom audio does not disable firearm mechanics.
