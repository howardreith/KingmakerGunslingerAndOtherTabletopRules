# Installation, updates, removal, and compatibility

## Supported baseline

This build is qualified against Pathfinder: Kingmaker Enhanced Plus Edition
2.1.7b on Windows through Steam, with Unity Mod Manager 0.32.4 or later in the
0.32.x line. Every real qualification launch used Steam App ID 640820.

The package has no bundled gameplay-library dependency. It requires the game,
its supported Unity Mod Manager installation, and the Harmony compatibility
assembly supplied by that environment. Do not copy game, Unity, UMM, Harmony,
or compiler assemblies into this mod folder.

## In Harm's Way immediate-action adaptation

Kingmaker 2.1.7b has a native shared swift-action cooldown but no complete
off-turn immediate-action resource. In turn-based combat, In Harm's Way can
react on another unit's turn when its mode is active, the protector is not
flat-footed, and no earlier immediate action is waiting to be charged. That
off-turn use consumes the protector's next actual turn's swift action and
prevents another immediate action until that turn completes. Delaying does not
clear the debt at the old initiative position. In RTWP, the native six-second
swift cooldown remains the shared swift/immediate budget.

The correlation needed for turn-based play is stored as hidden, save-stable
KMG facts. They are registered even when the Bodyguard module is not published,
so a save made while debt exists remains resolvable. Disabling the module does
not strip saved identities. Combat- and scene-completion cleanup removes only
transient action debt; it does not alter either feat or automation mode.

## Clean installation

1. Back up any saves you intend to keep outside the game's active save folder.
2. Install the standalone
   `KingmakerGunslinger-0.0.113-save-load-hotfix.zip`
   with Unity Mod Manager for Pathfinder: Kingmaker.
3. Do not install a source archive, repository snapshot, private reference
   bundle, compiler package, or framework reference archive.
4. Launch the game through Steam and verify that Unity Mod Manager reports
   Kingmaker Gunslinger version 0.0.113 without a red/broken load indicator.
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
Expanded Summoning, Elven Branched Spears, Eastern Weapons, `Brown-Fur
Transmuter -- requires Call of the Wild`, Urban Barbarian, Bodyguard and In
Harms Way, `Protection from Alignment: control immunity`, and `Elemental Races:
Ifrit, Oread, Sylph, and Undine (preview)` checkboxes. The first ten default ON;
Elemental Races defaults OFF. A change is
saved for the next complete game restart; the panel reports active and saved
next-restart state separately. Older settings migrate to schema 10 while
preserving every explicit prior value. An absent `elemental-races` key migrates
OFF; absent legacy keys retain their established ON defaults.

Brown-Fur is the only module that requires Call of the Wild. Its adjacent
status reports Available, Unavailable, or Blocked independently from saved user
intent, effective publication, and restart-required state. Saved ON intent is
preserved when CotW is absent, but Brown-Fur is not registered or published.
Structurally incompatible CotW installations also fail closed without disabling
the package's ten independent modules. Urban Barbarian, Bodyguard/In Harm's
Way, and Protection from Alignment are native-core features and remain
available when CotW is absent, unknown, or ambiguous.

Turning a module OFF removes its content only from new public choices and
acquisition paths. It does not unregister identities, strip an existing
Gunslinger, remove owned feats/items, or uninstall firearm state support. An
owned Elven Branched Spear and an already selected spear feat therefore remain
loadable and mechanically coherent while that module is OFF. With compatible
CotW still installed, Brown-Fur OFF likewise hides the archetype from new
character creation and respec while retaining its stable identities and the
features and effects of existing Brown-Fur owners. Do not remove the entire mod
from a campaign that has used any module.

The Protection from Alignment module is a startup-only rules publication rather
than new save-bearing content. Turning it OFF and restarting retains Kingmaker's
vanilla protection descriptions and allows new control effects under vanilla
rules; it does not scan for, remove, or otherwise change any control buff that
was already active.

## Removal warning

There is no uninstall cleanup or general uninstall-safe-save claim. Saves may
retain references to the Gunslinger class, progression features, abilities,
resources, firearm/ammunition/repair-kit blueprints, item-owned firearm
state-token enchantments, summon abilities, summon units, Elven Branched Spear
items, the spear category, selected spear features, Eastern Weapon items and
categories, selected Eastern proficiency or chosen-weapon facts,
enchantments, and buffs.
Urban Barbarian archetypes, allocation facts and abilities, and its Rage buff
are likewise stable saved identities and remain registered while its module is
OFF.
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

- This package includes the approved Pistol, Musket, and Blunderbuss models and
  five approved SSE Library CC0 firearm sounds in a Unity 2018.4.10f1 bundle.
  Rifle/Revolver rig and sound entries remain in the bundle only so legacy or
  deliberately Toy Box-spawned items remain mechanically safe; they are not
  official content and have no ordinary selection or acquisition route.
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
- Craft Magic Items 2.1.0 has an optional reflection-only integration described
  below. Eddic Respec and Bag of Tricks remain
  `UNAVAILABLE-LOCAL-REFERENCE`; no claim is made for them.
- The approved asset bundle was built with the locally installed, licensed Unity
  2018.4.10f1 editor. Missing or corrupt bundle data falls back safely and does
  not change firearm rules identity, save schema, or reload action economy.
- Empty firearm requests are rejected during native attack-command construction.
  When the applicable Reload Firearm variant is selected for native automatic use,
  the request produces one normal `UnitUseAbility` reload command instead.

When diagnosing a conflict, reproduce it first on a copied save with only
Unity Mod Manager and Kingmaker Gunslinger enabled. A clean mod load does not
by itself prove campaign or cross-mod compatibility.

## Optional Craft Magic Items integration

When the Unity Mod Manager entry with ID `CraftMagicItems` is installed and
active, Gunslinger 0.0.112 probes `CraftMagicItems.Main` and enables the bridge
only if the required 2.1.0 data-loading, recipe, indexing, crafting, and Harmony
surfaces match. There is no required assembly reference: Gunslinger continues
normally when CMI is absent or disabled, and an incompatible external contract
disables only this bridge. The Gunslinger UMM panel reports `active`, `not
installed`, `installed but disabled`, or `incompatible, see log`; a KMG lower
panel implementation failure is separately reported as `KMG compatibility UI
fault, see log`.

Version 0.0.98 is rejected by its first human ammunition UI test. Its
conditional whole-method prefix could let Layout/input use CMI's renderer and
Repaint use KMG's renderer while a category selection changed, producing a
`SelectionGrid` control-count exception and unbalanced GUILayout/GUIClip
state. Version 0.0.99 removed that prefix and passed its first human ammunition
UI interaction test: the category remained visible and crafting worked.
Version 0.0.100 preserved that selector architecture through final human
acceptance. The 0.0.112 bridge retains CMI's ownership of its top-level Mundane
Crafting and parent/subtype selectors, then augments only KMG-owned data after
CMI has finalized the exact selected data and immediately before its ordinary
equipment-only `NewItemBaseIDs` path. It never drives CMI through a false/true
toggle, mutates CMI's UMM state, or replays CMI's load lifecycle. The ammunition
panel returns to CMI's common Current Money footer.

If KMG's lower panel throws, the exception is fully unwrapped and rethrown.
The original ordinary renderer is not run after partial custom output, and no
compatibility graph mutation occurs inside `OnGUI`. Any bridge disable and
rollback is deferred to the safe update lifecycle.

The tested external authority is CMI 2.1.0 built without source changes from
bfennema/OwlcatKingmakerModCraftMagicItems commit
`72f87523d0a116f5dfc92c91893d4955fa1eb303`. Its `CraftMagicItems.dll` has
SHA-256 `4AE2DA61470350B31BEEF162717A604C9CCD322F66193917944EA4A9596E392D`
and MVID `0044a45b-3bca-439e-86c5-a6aa4d42855e`. This was a reproducible local
build of the exact upstream source, not a downloaded official release binary.
Neither that DLL nor CMI data, localization, icons, or source are included in
the Gunslinger package.

With both mods active, CMI gains dedicated **Firearms** mundane and magic item
types. Their from-scratch bases are exactly Pistol, Musket, and Blunderbuss.
Legacy Advanced Rifle and Advanced Revolver remain registered, loadable,
firearm-mechanical, Reliable-compatible, indexed for pricing/base recognition,
and eligible for upgrades when already owned, but are not ordinary campaign
creation bases or official support. Wakizashi and Katana enter CMI's Exotic Weapons bases, Nodachi
enters Martial Weapons, and Elven Branched Spear enters Exotic Weapons. There
is no separate Eastern and Elven Weapons magic category: craft the canonical
mundane base through Martial/Exotic, then enchant the owned item through CMI's
ordinary Arms and Armor category. Authored masterwork, material, and +1
variants remain exact owned-item upgrade targets. Named campaign weapons may
be upgraded only when already owned and never become from-scratch bases.

The **Firearm Ammunition** category makes the exact inventory identities used
by Gunslinger in batches of 20. Its shared KMG policy charges 10% of retail,
rounded up with a 1 gp minimum: Black Powder Charge is 200 gp retail/20 gp to
craft, Lead Ball is 20 gp/2 gp, and Paper Cartridge is 240 gp/24 gp. KMG
applies CMI's non-free setting and scale 0.60 only for its own ammunition
operation, restores both settings in every success and exception path, and
leaves non-KMG recipes untouched. Timed and immediate CMI projects charge once
at creation; completion never charges a second time. Existing exact KMG
ammunition projects with target 50 or 60 are normalized once while preserving
progress, `GoldSpent`, result, recipe, crafter, and ordering; cancellation
refunds the original exact spend, and a project already at progress 5 or
greater completes through CMI's normal processing. The recipes do not create
loaded-state markers or change Gunslinger's rest crafting, reload, or Paper
mode rules.

KMG's item-owned firearm state-token and battered-origin enchantments remain
mechanically present and save-stable. The native tooltip filter omits only
enchantments containing the exact `FirearmStateTokenComponent` or
`BatteredFirearmOriginComponent` markers, preventing their null localization
from producing phantom qualities. Real qualities such as Anarchic,
Enhancement +5, and Reliable remain visible, and firearm condition continues
through KMG's dedicated presentation.

CMI's magic **Firearms** category also exposes the existing Gunslinger
`Reliable` enchantment. It remains the exact KMG enchantment with +1 equivalent
bonus and caster level 8 and is restricted at the final creation boundary to a
weapon type containing exactly one canonical firearm-definition marker. That
includes CMI-generated firearm clones and excludes bows, crossbows, Eastern
weapons, Elven Branched Spears, and arbitrary non-firearms. The tabletop
prerequisite is *mending*; Kingmaker 2.1.7b has no usable Mending blueprint, so
the CMI recipe records no spell prerequisite rather than substituting an
invented spell. The ordinary Craft Magic Arms and Armor feat remains required.

New creation follows the owning feature-module state: disabling Gunslinger
suppresses new firearms and ammunition, disabling Eastern Weapons suppresses
its new bases, and disabling Elven Branched Spears suppresses its base.
Registered-but-unavailable firearms are never admitted. Stable already-owned
items remain eligible for CMI's normal upgrade recognition under Gunslinger's
existing save policy.

CMI custom items follow CMI's own generated-blueprint persistence. A campaign
containing a CMI-crafted Gunslinger item may require both mods to remain
installed. Back up the save before crafting, upgrading, removing either mod, or
changing module state.

CMI UI release-regression checklist for one fresh process:

1. Confirm CMI reports Kingmaker Gunslinger 0.0.112.
2. Open Craft Mundane Items.
3. Confirm Firearms offers exactly Pistol, Musket, and Blunderbuss.
4. Confirm Advanced Rifle and Advanced Revolver are absent.
5. Confirm Nodachi appears under Martial.
6. Confirm Wakizashi, Katana, and Elven Branched Spear appear under Exotic.
7. Confirm no separate Eastern and Elven Weapons magic category exists.
8. Craft one 20-unit batch of each ammunition item.
9. Confirm each project estimate is approximately one safe crafting day with
   the same crafter and settings.
10. Confirm prices are 20/2/24 gp for powder, ball, and paper; CMI remains enabled.
11. Confirm Work in Progress reports target/progress consistently.
12. Enchant one owned Eastern or Elven weapon through Arms and Armor.
13. Inspect a newly crafted magical Pistol.
14. Inspect an upgraded battered starter Pistol while loaded.
15. Confirm Anarchic, Enhancement +5, and Reliable text remains.
16. Confirm no `<null>` text or phantom blank qualities appear.
17. Save, exit, reload through an authorized disposable-save procedure, and
    inspect the representative items again.
18. Confirm firearm state and battered-origin behavior remain intact.
19. Confirm no CMI GUI rendering error.
20. Confirm no KMG bridge fault, layout mismatch, or graph rollback.

The repository owner completed and explicitly accepted the installed 0.0.100
candidate before authorizing the metadata-only 0.0.101 promotion. Retain a
fresh UMM output log when repeating this checklist after installation;
automated evidence does not replace visual and interaction review.

The guarded mechanical qualification is recorded in
`docs/CRAFT-MAGIC-ITEMS-COMPATIBILITY-REPORT.md`. The original acceptance is
human evidence; future regression results must likewise not be inferred from
mechanical logs alone.
# Custom firearm SoundBank

The release audio asset is copied only to
`Kingmaker_Data\StreamingAssets\Audio\GeneratedSoundBanks\Windows\KMG_Firearms.bnk`.
The mod hash-verifies the packaged source and existing destination and never
writes `Init.bnk` or another bank. To uninstall this optional native-audio
artifact, remove only `KMG_Firearms.bnk`; do not remove any vanilla bank.
Missing or rejected custom audio does not disable firearm mechanics.

# 0.0.91 Bodyguard AC-breakdown candidate

Install only the strict standalone
`artifacts\packages\KingmakerGunslinger-0.0.91-bodyguard-in-harms-way.zip`
through Unity Mod Manager. This candidate supersedes, but does not overwrite,
the immutable `0.0.90` artifact. It preserves the qualified combat mechanics
and adds native expanded attack-detail source lines for successful Bodyguard
AC contributions.

Each successful protector contributes one attack-scoped `Bodyguard +2` source;
two successful protectors contribute two such sources and exactly +4 total.
Failed attempts and module-disabled or ineligible attacks produce no source.
No persistent AC modifier or timed buff is introduced.

# 0.0.90 Bodyguard and In Harm's Way candidate

Install only the strict standalone
`artifacts\packages\KingmakerGunslinger-0.0.90-bodyguard-in-harms-way.zip`
through Unity Mod Manager. Do not overlay it onto an older mod directory. The
new project blueprint identities remain registered when the module is disabled
so saves that already own either feat or mode marker remain deserializable.

Bodyguard and In Harm's Way are independent of Call of the Wild. Their shared
module defaults ON, while both per-character automation modes default OFF and
may be enabled together. Disabling the module at the UMM boundary hides both
feats from new selection and makes their runtime hooks inert after restart; it
does not strip facts from existing characters.

# 0.0.89 weapon-presentation candidate

Install only the strict standalone package produced under
`artifacts\packages\KingmakerGunslinger-0.0.89-*.zip` through the existing
Unity Mod Manager workflow. Remove or replace the prior
`Mods\KingmakerGunslinger` directory as directed by that workflow; do not mix
files from `0.0.88` and `0.0.89`. The guarded deployment scripts validate
`Info.json`, assembly version, package shape, AssetBundle manifests, and the
allow-listed `KMG_Firearms.bnk` before replacing a live test install.

The batch adds no required dependency and preserves standalone operation.
Brown-Fur remains the only module with optional Call of the Wild integration.
The presentation calibration changes no item acquisition or save identity.
Static merchant and fixed-loot blueprint behavior remains as qualified in
`0.0.88`; no refresh is promised for an already opened container or already
materialized merchant in an existing save.
