# Craft Magic Items Compatibility Implementation and Qualification Report

## Result

Kingmaker Gunslinger 0.0.98 contains one optional reflection/Harmony adapter
for Craft Magic Items (CMI). It adds no compile-time or packaged CMI dependency,
does not modify CMI, and is inert when UMM ID `CraftMagicItems` is absent or
disabled. An incompatible reflected shape produces one bounded diagnostic,
rolls back bridge-owned graph changes, and disables only the bridge.

The exact externally built CMI authority passed the guarded compatibility
observer with 23/23 assertions and the canonical `KMG_AUTOMATION_WORKING`
smoke with 11/11 assertions. Deterministic source qualification passes 1,238
tests. Human CMI UI acceptance and save/reload of player-crafted items remain
explicitly unclaimed.

## Baseline and external authority

| Field | Exact value |
|---|---|
| Starting KMG commit | `290c63a9d51955ae5e692e51ffbee343e211b208` |
| Working branch | `codex/craft-magic-items-compatibility` |
| KMG release candidate | `0.0.98-craft-magic-items-compatibility` |
| CMI UMM ID / entry / Info version | `CraftMagicItems` / `CraftMagicItems.Main.Load` / `2.1.0` |
| Explanatory source | `bfennema/OwlcatKingmakerModCraftMagicItems` commit `72f87523d0a116f5dfc92c91893d4955fa1eb303` |
| Assembly authority | exact unchanged upstream source-built `CraftMagicItems.dll`; not an official downloaded release binary |
| Assembly/File version | `1.0.0.0` / `1.0.0.0` |
| DLL SHA-256 | `4AE2DA61470350B31BEEF162717A604C9CCD322F66193917944EA4A9596E392D` |
| DLL MVID / bytes | `0044a45b-3bca-439e-86c5-a6aa4d42855e` / `231424` |

The external checkout, build output, installed-mod copy, CMI Data/L10n/Icons,
and CMI DLL are ignored local runtime inputs. None is copied into KMG source or
its package.

## Architecture

The coordinator locates the exact active UMM entry and loaded assembly. A
bounded contract probe resolves CMI's entry, data/recipe shapes, fields,
methods, enum values, and Harmony 2 patch API. All external reflection remains
inside the adapter; the KMG catalog and registration plan use normal project
blueprint types.

The adapter enters at CMI's first non-generic
`AddItemIdForEnchantment` prefix. At that exact seam CMI has assigned
`ItemCraftingData[]` and initialized ordinary recipes, but its equipment and
enchantment indexes have not started. The bridge transactionally appends its
item types, synchronizes the affected `SubCraftingData`, `TypeToItem`, and
Reliable recipe surfaces, and then lets CMI build the rest of its indexes
normally. A late-loaded bridge invokes one complete validated CMI off/on
rebuild. A postfix after CMI feat publication activates the two new magic
categories without duplicating Craft Magic Arms and Armor. Eleven narrowly
scoped Harmony 2 patches are owned by one stable bridge ID and installed once.
Patch installation is itself transactional: the validated Harmony 2
`UnpatchAll(string)` surface removes every patch owned by the bridge if any
member of the 11-patch set fails to install.

The initial generic `ReadJsonFile<T>` approach was rejected during real Mono
qualification: generic code sharing also intercepted CMI localization reads.
The final non-generic index seam removes that cross-instantiation hazard and
was the architecture used by both passing guarded runs.

Repeated main-menu/data initialization, save loads, area transitions, late
attachment, and UMM rendering deduplicate stable item-type identities, exact
AssetGuids, recipes, localization keys, indexes, and patches. Any mutation
failure transactionally restores CMI arrays/caches/indexes and KMG bridge state.

## Registration inventory

### Firearm creation bases

| Weapon | AssetGuid |
|---|---|
| Pistol | `a303d71d244640959827e9464df5a867` |
| Musket | `6c9cdfa2d47e4894847fa85d5319fbd2` |
| Blunderbuss | `236f3e167f5542bcac22bca72046fb1f` |
| Advanced Rifle | `a267e7bbc10e425f8adb87844d572b29` |
| Advanced Revolver | `8ed461fbcc154c51b07e5549211e9f5e` |

These exact canonical bases enter both dedicated CMI Firearms types only while
the Gunslinger module is active and each catalog entry remains
`IsPlayerFireable`. Diagnostic, unavailable, and gated firearm identities are
excluded. Pistol +1 (`d0145d0410a34df08d68a67367c1dfc9`), Musket +1
(`3402fe01de1648b187c192500e370f01`), and Blunderbuss +1
(`1dc7efe0792040f187a18adfdc54c6e0`) are authored exact targets, not additional
from-scratch bases.

### Mundane custom-family bases

| CMI category | Canonical base | AssetGuid |
|---|---|---|
| Exotic Weapons | Wakizashi | `b61ee7e62bc9288004eb0121c8f5d37e` |
| Exotic Weapons | Katana | `aba40a9e8302b31e4daa2acf6ab48a46` |
| Martial Weapons | Nodachi | `35b7082d98ff45ba51dce536a1bc68a1` |
| Exotic Weapons | Elven Branched Spear | `6edc216d68810960f85417237748b042` |

The classification is verified from current KMG weapon metadata. Their
masterwork/material/+1 generic forms remain exact CMI index targets. Eastern
and spear module state independently controls new creation.

### Ammunition

| Result item | AssetGuid | Count | Unit value | Batch value | Progress | Gold at scale 1.0 |
|---|---|---:|---:|---:|---:|---:|
| Black Powder Charge | `ea966bf998a647cf97b0ed92f71c4b7d` | 20 | 10 | 200 | 50 | 34 |
| Lead Ball | `55c29771445947d685dba9e1ead46a42` | 20 | 1 | 20 | 5 | 4 |
| Paper Cartridge | `fea7337cfd06417a853546af9d950f77` | 20 | 12 | 240 | 60 | 40 |

The recipes return the exact existing plain `BlueprintItem` identity and do
not enter `NewItemBaseIDs`. CMI's mundane formula supplies progress and cost.
The output remains stackable and consumable by existing KMG ammunition code;
Paper Cartridge compatibility remains controlled by its existing firearm
profile.

### Named upgrade-only exclusions

The following campaign uniques are indexed for upgrade when already owned but
are deliberately absent from every creation-base array:

- Firearms: Duelist's Rebuttal, The River King's Measure, Irovetti's Ovation,
  The Last Word, Watch at the World's End.
- Wakizashi: Paper Lantern, Quiet Current, Falling Petal, Foxfire Whisper,
  Empty Sleeve, Night Without Moon.
- Katana: Wayfarer's Oath, Winter Reed, Drawn Horizon, Thunder at the Gate,
  Moonlit Crossing, Heaven's Measure.
- Nodachi: Border Sentinel, Cloud-Cleaver, Storm Over Stone, Mountain-Sunder,
  Unfixed Form, World-Tree Severer.
- Elven Branched Spear: Boughkeeper, Thornstep, Moonlit Fork, Viper's Reach,
  Briar-Crowned Spear, Spear of the First Branch.

## Reliable authority and boundary

CMI receives exactly one recipe for KMG's existing
`BlueprintWeaponEnchantment` AssetGuid
`ea10817126e14703878d00e84329244e`. KMG does not clone the enchantment or add a
second misfire implementation. Repository/tabletop authority supplies +1
equivalent bonus, caster level 8, the existing display name/description, normal
Craft Magic Arms and Armor feat behavior, and CMI's ordinary cost/index paths.
The tabletop spell prerequisite is *mending*. Because Kingmaker 2.1.7b has no
usable Mending blueprint, the reflected recipe uses an empty spell array rather
than inventing another spell; this is an explicit adaptation limitation.

The final `RecipeAppliesToBlueprint` result is conjunctively restricted, and custom
GUID creation is independently guarded, to a `BlueprintItemWeapon` whose actual
weapon type contains exactly one `FirearmDefinitionComponent`. This recognizes
CMI clones without an item-GUID allowlist and rejects bows, crossbows,
Wakizashi, Katana, Nodachi, Elven Branched Spear, arbitrary weapons, and
ambiguous duplicate markers. Reliable is inserted into CMI's ordinary
enchantment-to-recipe and plus-equivalent/cost indexes for recognition,
replacement, removal, description, and pricing.

## Custom blueprint and item-state integrity

Qualification resolves representative CMI-generated firearm, Eastern, and
spear clones through CMI's real custom blueprint machinery. It compares the
original before/after and requires the clone to retain weapon type, exactly one
firearm marker, proficiency, presentation, reload/capacity mechanics, family
category, finesse/reach/grip mechanics, and project-owned zero-cost policy
enchantments as applicable. The base blueprint must remain unchanged. Before a
CMI upgrade replaces a firearm item entity, KMG transfers missing item-owned
loaded/condition token identities and battered-origin ownership to the result;
Reliable remains an ordinary exact item enchantment.

CMI remains the owner of generated-blueprint persistence. KMG does not create
a parallel persistence system. Consequently, CMI-crafted KMG items may require
both mods to remain installed.

## Deterministic and package qualification

The runtime-qualified candidate passed these commands:

```powershell
python .\tools\validate_repository.py
.\scripts\test-domain.ps1 -Configuration Release
.\scripts\Build-Local.ps1
.\scripts\validate-package.ps1 `
  -PackagePath .\artifacts\packages\KingmakerGunslinger-0.0.98-craft-magic-items-compatibility.zip `
  -Configuration Release
```

Results: repository validation PASS; 1,238/1,238 domain/reflection tests PASS;
exact-reference clean Release build PASS; build-output, firearm SoundBank,
deterministic packaging, and strict standalone UMM package validation PASS.
Ten focused CMI cases cover absence, accepted/rejected contract shape, catalog
construction, idempotence, module states, Reliable applicability, ammunition
economics, custom graph integrity, lifecycle/load order, and package isolation.

| Runtime-qualified candidate field | Exact value |
|---|---|
| Runtime-deployed source-state SHA-256 | `624975EB47335D587601B5C6358D6F67A188F11747A5293F3DE3F6BDB6E94B5B` |
| KMG DLL SHA-256 | `99D3A115FCD2AC78299A5F94705E478FC44D07002A9AC7F2185EB7C877767A3D` |
| KMG DLL MVID | `d7301a45-0bb7-4d3d-bd27-b19cf568aca1` |
| Local-runtime/release package SHA-256 | `DE71DF335EBB192CAAA5A2529B2AC121DC8D2B1859329C86E08DEFDFE54D83A4` |
| Package bytes | `22623746` |

The package inventory contains no `CraftMagicItems.dll`, CMI source, CMI
Data/L10n/Icons, game/Unity assemblies, saves, credentials, raw runtime
evidence, or machine-local configuration.

## Guarded runtime qualification

The dedicated real-assembly command was:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-craft-magic-items-compatibility `
  -ExpectedVersion 0.0.98 `
  -TimeoutSeconds 240 `
  -ExitAfterCompletion:$true `
  -Confirm:$false `
  -AllowDirtyGit
```

The observer accepts only the live active `CraftMagicItems` entry, checks the
complete registered graph and exact counts, validates canonical bases and named
exclusions, proves Reliable identity/applicability/cost indexing, builds and
resolves representative CMI clones, checks component and base immutability,
and forces a complete repeated CMI initialization boundary to prove
idempotence. It is a save-free disposable boundary and does not claim visual
UI acceptance.

Exact result:

- Evidence directory:
  `runtime-evidence/20260824T2255264229186Z-observe-craft-magic-items-compatibility`
- Run ID:
  `20260824T2255264464788Z-013a170e700c49968b4c6a34b613b212`
- Steam App ID `640820`; scenario started `2026-08-24T22:57:15.3120357Z`;
  completed `2026-08-24T22:57:16.9363536Z`; guarded launch-to-result
  duration `102749` ms; status PASS.
- 23/23 assertions passed. The initial and forced rebuilt graphs were exactly
  `itemTypes=4; firearmBases=5; customBases=4; ordinaryRecipes=46;
  reliable=1; ammunition=3`, at generations 1 and 2 respectively.
- Real CMI clones resolved for an ordinary +1 Pistol, +1/Reliable Pistol,
  +1 Katana, and +1 Elven Branched Spear. The observer also proved exact
  firearm state-token transfer and `RulesRecipeItemCost=9300` for the tested
  +1/Reliable combination.

The canonical save-backed follow-up was:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario working-save-smoke `
  -ExpectedVersion 0.0.98 `
  -SaveName KMG_AUTOMATION_WORKING `
  -TimeoutSeconds 300 `
  -ExitAfterCompletion:$true `
  -Confirm:$false `
  -AllowDirtyGit
```

- Evidence directory:
  `runtime-evidence/20260824T2258238378003Z-working-save-smoke`
- Run ID:
  `20260824T2258238632353Z-d7d7dea72e6c456c9df48701e8277678`
- Steam App ID `640820`; started `2026-08-24T22:58:31.3830304Z`;
  completed `2026-08-24T23:00:31.7759726Z`; guarded duration
  `120384` ms; status PASS.
- 11/11 assertions passed: complete 111-save catalog, unique working/baseline
  identities, descriptor correlation, load completion/fingerprint, exact
  receiver-bound order, loaded KMG `0.0.98`, and no save-writing API.

Six earlier guarded candidates failed or timed out closed while the adapter
was narrowed: premature KMG catalog construction, Harmony's direct dynamic
method rejection, factory-signature rejection, Mono dynamic-parameter
metadata rejection, and finally generic `ReadJsonFile<T>` sharing into CMI's
localization loader. Each failure caused an engineering strategy change; none
was treated as runtime permission. The two final-candidate runs above supersede
the earlier passing pre-rollback candidate evidence and are the current
qualification claims.

## Human acceptance checklist

- [ ] Firearms appears once in mundane and magic weapon crafting; Firearm
  Ammunition appears once; labels are localized and categories are sensible.
- [ ] Craft one base firearm and upgrade one already-owned custom or named
  weapon.
- [ ] Apply Reliable to a firearm and verify it is absent/rejected for a
  representative non-firearm.
- [ ] Craft one exact 20-unit batch of each ammunition item.
- [ ] Craft or upgrade one Katana or Wakizashi and one Elven Branched Spear.
- [ ] Save and reload representative crafted items through an authorized
  disposable-save procedure.

## Remaining uncertainty

The CMI category layout and checklist above require human interaction and are
not claimed from structured logs. The observer created request-local item
entities and CMI custom blueprints, not inventory projects; save/reload of a
player-crafted CMI/KMG item therefore remains human acceptance work. No
official downloaded CMI 2.1.0 binary was available, so compatibility with a
binary distribution that differs from the pinned unchanged source build is
not claimed; the shape probe fails closed if its contract differs. The passing
runs used the user's all-installed stack, including Call of the Wild, but were
not a profile-controlled Call of the Wild plus CMI isolation/restoration test.
