# Craft Magic Items Compatibility Implementation and Qualification Report

## Result

Kingmaker Gunslinger (KMG) 0.0.101 is the released Craft Magic Items (CMI)
compatibility build promoted from the owner-accepted 0.0.100 refinement. It
preserves the 0.0.99 ammunition renderer repair and addresses the four findings
from that candidate's first human interaction:

- all exact 20-unit KMG ammunition projects now use timed target 5 without
  changing price or item value;
- legacy Advanced Rifle and Advanced Revolver remain loadable/upgradeable but
  are not official support or new campaign crafting bases;
- KMG state and battered-origin policy enchantments remain mechanical but are
  omitted from native player-facing quality text;
- the standalone **Eastern and Elven Weapons** magic category is removed.

Version 0.0.99 passed its first human ammunition interaction: the category
remained available and crafting completed. Its value-derived project durations,
advanced-firearm acquisition, four `<null>` tooltip lines, and extra magic
category were nevertheless rejected as final behavior. Version 0.0.98 remains
rejected for its Unity IMGUI control-tree failure.

The 0.0.100 candidate passed 1,243/1,243 deterministic tests, all source/build/
package gates, a 27/27 real-CMI graph observer, a 13/13 actual ammunition UI
route observer, a two-launch 6/6 + 6/6 save/reload/cleanup qualification, and the
canonical 11/11 working-save smoke. The repository owner then explicitly
accepted the installed 0.0.100 candidate on 2026-08-25 and authorized
finalization, merge, publication, and an incremented release. Version 0.0.101
is the metadata-promoted release and repeats the automated gates below.

## Current targeted bridge and ammunition-economy contract

This archive records the accepted 0.0.100/0.0.101 qualification. The active
0.0.111 source contract supersedes its historical CMI lifecycle and ammunition
price behavior:

- KMG passively detects one compatible, enabled CMI UMM entry. It never calls
  CMI's toggle API, never changes CMI's UMM active/loaded/enabled state, and
  never invokes CMI's general load or feat-publication lifecycle.
- If CMI's graph already exists, KMG augments and finalizes only KMG-owned
  categories, recipes, and indexes by stable identity. Repeated callbacks are
  idempotent. A genuine user disable leaves CMI disabled and makes the KMG
  bridge inactive.
- Every 20-unit KMG ammunition batch uses the same 10%-of-retail policy in the
  native and CMI routes: Black Powder Charge retail 200 gp/craft 20 gp; Lead
  Ball retail 20 gp/craft 2 gp; Paper Cartridge retail 240 gp/craft 24 gp.
  Costs round up and have a 1 gp minimum.
- During only the KMG ammunition control render/commit boundary, the bridge
  temporarily applies CMI's non-free setting and price scale 0.60 so CMI
  calculates that exact policy. Both user settings are restored in the scope's
  dispose/rollback path. No non-KMG recipe or persistent CMI setting changes.

The 34/4/40 figures and historical full-graph rebuild wording below are
archival evidence for the old released candidate, not instructions for the
active bridge.

## Baseline and external authority

| Field | Exact value |
|---|---|
| Original compatibility starting commit | `290c63a9d51955ae5e692e51ffbee343e211b208` |
| Rejected 0.0.98 human-test baseline | `d7178d6ae77b79624917f955658231ae67894c51` |
| Accepted 0.0.100 cleanup starting commit | `2100a881e057d77829a7b60ab85caa973c6ea25b` |
| Working branch | `codex/craft-magic-items-compatibility` |
| Rejected release | `0.0.98-craft-magic-items-compatibility` |
| First crash-repair candidate | `0.0.99-craft-magic-items-ammunition-ui-repair` |
| Accepted candidate | `0.0.100-craft-magic-items-post-human-refinement` |
| Current release | `0.0.101-craft-magic-items-compatibility` |
| CMI UMM ID / entry type / Info version | `CraftMagicItems` / `CraftMagicItems.Main` / `2.1.0` |
| Explanatory source | `bfennema/OwlcatKingmakerModCraftMagicItems` commit `72f87523d0a116f5dfc92c91893d4955fa1eb303` |
| Installed assembly authority | exact unchanged upstream source-built `CraftMagicItems.dll`; not an official downloaded release binary |
| Assembly/File version | `1.0.0.0` / `1.0.0.0` |
| CMI DLL SHA-256 | `4AE2DA61470350B31BEEF162717A604C9CCD322F66193917944EA4A9596E392D` |
| CMI DLL MVID / bytes | `0044a45b-3bca-439e-86c5-a6aa4d42855e` / `231424` |

The installed DLL is runtime authority and the linked source is explanatory.
Neither enters the KMG repository or package. Production KMG has no static CMI
type or assembly reference and fails closed when the reflected capability shape
is absent or incompatible.

## Historical 0.0.98 failure and preserved 0.0.99 repair

The rejected 0.0.98 graph initialized as
`generation=1;itemTypes=4;firearmBases=5;customWeaponBases=4;
ordinaryWeaponRecipes=46;reliableRecipes=1;ammunitionRecipes=3;
namedCreationBases=0`.

On the first human click of **Firearm Ammunition**, KMG logged
`bridge.incompatible phase=ammunition-ui` with a recursively wrapped
`System.Reflection.TargetInvocationException`. CMI logged
`System.ArgumentException: Getting control 1's position in a group with only 1
controls when doing repaint` through `GUILayout.SelectionGrid`,
`UmmUiRenderer.RenderSelection`, `Main.DrawSelectionUserInterfaceElements`, and
`Main.RenderCraftMundaneItemsSection`. Unity then reported mismatched
`LayoutGroup.repaint` and `GUIClip` state.

Source, installed IL, and the supplied log confirmed that the old conditional
whole-method prefix let Layout/input and Repaint select different control trees,
redrew CMI's selector, and synchronously rolled back the graph inside `OnGUI`.
That prefix, selection-index ownership decision, duplicate selector, partial
fallback, and synchronous GUI rollback remain absent.

CMI still owns the top-level **Mundane Crafting:** selector, subtype selector,
ordinary mundane body, and common money footer. KMG intercepts only the already
resolved exact ammunition `ItemCraftingData` at the capability-probed seam:

`post-selected-crafting-data:ordinary=IL_014d;new-item-bases=IL_0186;
footer=IL_0774;locals=crafter:1,selected:4,recipe:5`.

The Harmony 2 transpiler requires exactly one ordinary-body anchor, one
`NewItemBaseIDs` equipment anchor, one footer target, the expected locals, and
all prebound lower-panel methods. A non-ammunition object follows CMI's original
body unchanged. Exact-reference ammunition selection renders only the lower
panel and branches to the footer. Layout and Repaint therefore take the same
route. No reflected contract discovery occurs after the first custom control.

A rendering fault is logged as a KMG UI fault, not an external-contract
incompatibility. Nested invocation exceptions are unwrapped completely. No
catch runs CMI's original body after partial rendering, and any disable or graph
rollback is deferred to a safe non-GUI lifecycle boundary.

## 0.0.100 refinements

### Ammunition quantity, price, time, and migration

CMI's ordinary mundane price calculation is still authoritative. The previous
timing used `batch value / 4`, producing targets 50, 5, and 60. With the human's
crafter/settings powder appeared as roughly 25 adventuring days or seven safe
days, balls as one safe day, and cartridges as roughly eight safe days. That
calculation was internally consistent but unsuitable for consumable ammunition.

| Exact result | AssetGuid | Count | Batch value | Old value target | Timed target | Gold at scale 1.0 |
|---|---|---:|---:|---:|---:|---:|
| Black Powder Charge | `ea966bf998a647cf97b0ed92f71c4b7d` | 20 | 200 | 50 | 5 | 34 |
| Lead Ball | `55c29771445947d685dba9e1ead46a42` | 20 | 20 | 5 | 5 | 4 |
| Paper Cartridge | `fea7337cfd06417a853546af9d950f77` | 20 | 240 | 60 | 5 | 40 |

One postfix targets the exact supported 13-parameter
`CraftMagicItems.CraftingProjectData` constructor. It changes only `TargetCost`
and only when the project is a new, non-upgrade KMG ammunition project whose
item type is the exact bridge-owned category and whose result blueprint is one
of the three GUIDs above. `GoldSpent`, result identity/count, value, global
crafting rate, other CMI projects, and Crafting Takes No Time behavior are not
changed.

Legacy reconciliation examines existing CMI timer data only at safe project
lifecycle boundaries. It requires exact item-type identity and exact result
GUID, changes target 50 or 60 to 5, preserves `GoldSpent`, `Progress`, result,
crafter, recipe, prerequisites, and ordering, and is idempotent. Progress is not
reset; a project already at target completes through CMI's normal processing.
No project is cancelled, refunded, recharged, or duplicated. The real observer
migrated one target-60 project with progress 7 exactly once.

### Firearm creation versus recognition

`IsPlayerFireable` remains a mechanical capability gate, not the campaign
acquisition policy. The production catalog classifies official ordinary
creation bases separately from legacy recognition identities.

| Weapon | AssetGuid | New mundane/magic Firearms base | Owned-item recognition, upgrade, Reliable, persistence |
|---|---|---|---|
| Pistol | `a303d71d244640959827e9464df5a867` | yes | yes |
| Musket | `6c9cdfa2d47e4894847fa85d5319fbd2` | yes | yes |
| Blunderbuss | `236f3e167f5542bcac22bca72046fb1f` | yes | yes |
| Advanced Rifle | `a267e7bbc10e425f8adb87844d572b29` | no | yes |
| Advanced Revolver | `8ed461fbcc154c51b07e5549211e9f5e` | no | yes |

Legacy advanced firearms remain registered, loadable, mechanically functional,
indexed for price/base recognition, legal existing-item upgrade targets, valid
for Reliable, and valid results for pre-existing 0.0.99 custom blueprints or
projects. They are absent from every future new-item base array and every
ordinary non-CMI acquisition or selector surface.

### Category organization

The finalized graph adds exactly three item types:

- `KMGMagicFirearms`;
- `CraftMundaneKMGFirearms`;
- `KMGFirearmAmmunition`.

`KMGMagicEasternAndElvenWeapons`, its state/localization, and its duplicated
ordinary recipe collection are removed. The exact mundane additions remain:

| CMI category | Canonical base | AssetGuid |
|---|---|---|
| Martial Weapons | Nodachi | `35b7082d98ff45ba51dce536a1bc68a1` |
| Exotic Weapons | Wakizashi | `b61ee7e62bc9288004eb0121c8f5d37e` |
| Exotic Weapons | Katana | `aba40a9e8302b31e4daa2acf6ab48a46` |
| Exotic Weapons | Elven Branched Spear | `6edc216d68810960f85417237748b042` |

The intended workflow is mundane Martial/Exotic creation followed by CMI's
ordinary **Arms and Armor** existing-item upgrade route. No custom family base
was appended to Arms and Armor's from-scratch list. Runtime qualification proved
owned canonical, authored generic, named unique, and 0.0.99-style CMI custom
representatives across all four families remain indexed and upgradeable.

### Internal enchantment tooltip presentation

Runtime enumeration confirmed the two `<null>` qualities on the representative
loaded battered CMI-upgraded Pistol:

| Role | AssetGuid | Internal name | Marker component |
|---|---|---|---|
| Battered origin | `2c01fc0e7f7c4f3bb8f493875cb489a0` | `KMG_BatteredFirearm_Origin` | `BatteredFirearmOriginComponent` |
| Loaded state | `c11a8965dbdd43f08080f4dc51a29113` | `KMG_StateToken_LoadedNormal_LeadBall` | `FirearmStateTokenComponent` |

The same audit confirmed broken-empty (`5513972dd2624c9f86bc29c850dac736`)
and wrecked (`877f65ca3a404f2e98af528b7fb1a2fb`) state variants use the same internal
state-token marker. KMG patches only native `UIUtilityItem.FillWeaponQualities`
and `UIUtilityItem.GetQualities` enumeration. The shared predicate hides an
enchantment only when it contains one of those exact KMG marker component types.
It does not suppress arbitrary null-named or third-party enchantments.

The enchantments and components remain on the item. KMG's dedicated firearm
condition presentation remains authoritative. Runtime tooltip data retained
Anarchic, Enhancement +5, and Reliable while reporting zero `<null>` entries,
zero phantom quality blocks, and no internal marker names.

## Named upgrade-only inventory

These campaign uniques remain recognized for upgrade when already owned and are
absent from every from-scratch list:

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

## Reliable authority and applicability

CMI receives one recipe for the existing KMG `BlueprintWeaponEnchantment`
`ea10817126e14703878d00e84329244e`. No enchantment or misfire mechanic is
cloned. Repository/tabletop authority supplies +1 equivalent, caster level 8,
existing name/description, normal Craft Magic Arms and Armor feat behavior, and
CMI's ordinary cost/index paths. The tabletop prerequisite is *mending*;
Kingmaker 2.1.7b has no usable Mending blueprint, so no substitute spell
blueprint is invented.

The final CMI recipe and custom-GUID boundaries require exactly one canonical
`FirearmDefinitionComponent` on the actual weapon type. The three official and
two legacy-recognized firearms, plus CMI firearm clones, pass. Bows, crossbows, Wakizashi, Katana,
Nodachi, Elven Branched Spears, arbitrary weapons, and ambiguous duplicate
markers fail. The real observer proved +1/Reliable equals plus 2 and exact CMI
rules cost 9,300.

## Module, persistence, and mechanical policy

New firearm/ammunition creation remains off when Gunslinger is off. Eastern and
Elven base additions independently follow their owning modules. Unpublished and
unavailable content remains excluded. CMI installed but disabled keeps the
bridge inactive. Already-owned stable items retain KMG's established policy.

CMI remains the sole owner of generated-blueprint persistence. KMG adds no
second persistence format, so CMI-crafted KMG items may require both mods to
remain installed.

The guarded two-launch fixture used an exact CMI custom Pistol with Anarchic
`57315bc1e1f62a741be0efde688087e9`, Enhancement +5
`bdba267e951851449af552aa9f9e3992`, and Reliable. Before and after a fresh
process/save load it retained exactly one firearm marker, one loaded-normal lead
ball state token, one battered-origin token bound to the same owner, condition
Normal, one loaded round, all three real enchantments, and zero null tooltip
text. Cleanup removed exactly that fixture and saved the disposable working save
clean. No base blueprint was mutated.

## Deterministic, build, profile, and package qualification

Commands run included:

```powershell
python .\tools\validate_repository.py --root .
.\scripts\test-domain.ps1 -Configuration Release
.\scripts\Build-Local.ps1
.\scripts\build.ps1 -Configuration Release -Clean -Package
.\scripts\validate-build-output.ps1 -Configuration Release
.\scripts\Validate-FirearmSoundBank.ps1
.\scripts\validate-package.ps1 `
  -PackagePath .\artifacts\packages\KingmakerGunslinger-0.0.100-craft-magic-items-post-human-refinement.zip `
  -Configuration Release
.\scripts\Test-RuntimeScenarioPreflight.ps1
.\scripts\compatibility\Test-KingmakerCompatibilityProfile.ps1
.\scripts\compatibility\Test-KingmakerCompatibilityProfileResolution.ps1
.\scripts\compatibility\Test-OptionalModReferenceInventory.ps1
.\scripts\compatibility\Test-OptionalModCompatibilityObserver.ps1
.\scripts\compatibility\Test-ExpandedSummoningCompatibilityProfiles.ps1
```

Results: repository validation PASS; 1,243/1,243 complete domain/reflection
tests PASS, including 15 focused CMI cases; clean exact-reference Release build
PASS; build-output, SoundBank, deterministic package, and strict standalone UMM
validation PASS; runtime preflight 143/143 PASS; all 12 compatibility-profile
dry runs PASS; all five Expanded Summoning profile checks PASS; optional-mod
reference inventory and observer contracts PASS.

| Runtime-qualified artifact field | Exact value |
|---|---|
| Runtime-deployed source-state SHA-256 | `2B7C37AF91F18FD8C1A6066EE4DB4E3FE0CB6443D6ADFF5567BB122341CA6D0A` |
| KMG DLL SHA-256 | `08DC2910EE1D5FC4E2775D33786B9D7612C7817409C65F1D5C0AD71D1D6E8669` |
| KMG DLL MVID / bytes | `e3f72b04-ab83-4447-b281-005b30be2e46` / `4286976` |
| Release/local-runtime package SHA-256 | `C8D98E0282AE8EFE310477C9194A43601AE6B10A2A9F9A913C7CEE741A83FA52` |
| Package bytes | `22663146` |

The final clean deterministic rebuild was byte-identical to the package and DLL
used by every 0.0.100 runtime observer listed below.

Final package path:
`artifacts/packages/KingmakerGunslinger-0.0.100-craft-magic-items-post-human-refinement.zip`.
It contains no CMI DLL/source/Data/L10n/Icons, Kingmaker or Unity assemblies,
saves, raw runtime evidence, credentials, or machine-local configuration.

## Guarded runtime qualification

Every launch used the repository guard and Steam App ID 640820. No direct game
executable launch, Computer Use, OCR, screenshots, or coordinate input supplied
mechanical evidence.

### Real CMI graph/refinement observer — PASS 27/27

- Evidence: `runtime-evidence/20260825T1410209452802Z-observe-craft-magic-items-compatibility`
- Run ID: `20260825T1410209704928Z-3ab6b7739b83433d8d19ede3f2d4964b`
- Live CMI 2.1.0 was loaded, active, and capability-compatible.
- Generation 1 and forced generation 2 each reported
  `itemTypes=3;firearmCreation=3;firearmRecognition=5;martial=1;exotic=3;
  customMagicTypes=0;customRecognition=4;ordinaryRecipes=46;reliable=1;
  ammunition=3`.
- It proved advanced firearms recognition-only, zero named creation bases, all
  four custom families through Arms and Armor, target/gold/migration policy,
  Reliable identity/applicability/price, custom clone integrity, state transfer,
  tooltip suppression, base immutability, and the historical full-graph
  idempotence check. Current source uses targeted KMG-only finalization instead.

### Actual ammunition UI observer — PASS 13/13

- Evidence: `runtime-evidence/20260825T1412357504949Z-observe-craft-magic-items-ammunition-ui`
- Run ID: `20260825T1412357754594Z-2f8b8263235048aeb358355328361396`
- Transpiler applications `1`; exact seam as recorded above; outer owner CMI.
- Ordinary routes `4`; ammunition body bypasses/lower renders `24/24`; events
  `Layout,repaint`; zero GUI failures, UI failures, or rollbacks.
- All recipes were selectable. In the historical candidate, immediate paths
  spent 34/4/40 and produced 20 exact units. The active policy replaces those
  archived costs with 20/2/24. A normal Paper Cartridge project used target 5
  and completed via CMI. A powder cancellation refunded exact GoldSpent and
  created no result.
- Crafted powder/ball were consumed by loose reload and the exact cartridge by
  Paper mode. Request-local inventory and money cleanup was exact.

### Two-launch custom-firearm persistence — PASS 6/6 + 6/6

- Prepare evidence: `runtime-evidence/20260825T1404297559483Z-working-save-craft-magic-items-prepare`
- Prepare run ID: `20260825T1404297828378Z-bf9ac5cfeabe443a9f62ad48dfb7591c`
- Verify/cleanup evidence: `runtime-evidence/20260825T1406592978062Z-working-save-craft-magic-items-verify-cleanup`
- Verify run ID: `20260825T1406592978062Z-ab48f8c916b44a12bc7cfc71366facd7`
- Prepare observed `before=0;observed=1;after=1`; fresh-load cleanup observed
  `before=1;observed=1;after=0` for the exact custom GUID.
- Each launch authorized one exact captured `SaveRoutine`, observed two native
  numbered working-save `SaveStashedArea` clones, and rejected no write.

An earlier cleanup-only PASS (`20260825T1401357448503Z-f915056098604cd1b7089f88e3c894fe`)
removed the fixture left by the initial sentinel-development run. That run had
correctly saved the item but rejected Kingmaker's new numbered native save clone;
the guard was narrowed to the exact `Manual_<digits>_KMG_AUTOMATION_WORKING.zks`
identity while still requiring the one armed exact-reference `SaveRoutine`.

### Canonical working-save smoke — PASS 11/11

- Evidence: `runtime-evidence/20260825T1414437489624Z-working-save-smoke`
- Run ID: `20260825T1414437755365Z-42963c31170848c9848c4a9898688247`
- Complete catalog count 111; unique working/baseline descriptors; exact ordered
  receiver-bound action, load callback, and stable three-party fingerprint;
  loaded KMG 0.0.100; no save-writing API.

## 0.0.101 release-promotion qualification

The metadata-promoted 0.0.101 source state repeated every release-critical
mechanical check before its release commit. The guarded reusable-artifact
identity was
`sourceState=a099311ac5f2af4d65971837aedd140ddd12ecb3e48ecb4dda402f6d46948ea5`;
its pre-commit Git base was `02f717dc3dd14243387581e4e9b6f45e0d01ec7f`.
The standalone and local-runtime ZIPs were byte-identical.

| 0.0.101 promotion artifact field | Exact value |
|---|---|
| Package | `artifacts/packages/KingmakerGunslinger-0.0.101-craft-magic-items-compatibility.zip` |
| Package SHA-256 / bytes | `950F393B51DBDA313D2185D43F651FCF2B07FDCCAE4107D2CE206F2D8D03E756` / `22663364` |
| DLL SHA-256 / bytes | `C576C53B9D9195EB5A1FDC267DBE09A973AAC9CA42D3472C7E2BA11966D4548E` / `4286976` |
| DLL MVID | `7b6b215a-d530-45f4-89b6-d82b4e92b9bd` |

- Repository validation and all 1,243 domain/reflection tests passed, including
  all 15 focused CMI cases. Clean Release compilation, build-output,
  SoundBank, deterministic package, and strict standalone package validation
  passed. Runtime preflight passed 143/143; all 12 compatibility-profile dry
  runs and all five Expanded Summoning profile checks passed.
- Real CMI graph/refinement observer: PASS 27/27; evidence
  `runtime-evidence/20260825T1558087916302Z-observe-craft-magic-items-compatibility`;
  run ID `20260825T1558088217294Z-bd05754a23fc4f05adefb9f21a66422b`.
- Actual ammunition UI observer: PASS 13/13; evidence
  `runtime-evidence/20260825T1600409567792Z-observe-craft-magic-items-ammunition-ui`;
  run ID `20260825T1600409869241Z-79c2787125c64dbfa341c4fc953b494d`.
  It retained one transpiler application, the exact inner seam, CMI outer
  ownership, Layout/Repaint route stability, 24/24 lower-panel renders, zero
  GUI faults, and zero rollbacks.
- Two-launch CMI persistence: PASS 6/6 prepare plus 6/6 verify/cleanup; evidence
  `runtime-evidence/20260825T1602570715700Z-working-save-craft-magic-items-prepare`
  and
  `runtime-evidence/20260825T1605292417607Z-working-save-craft-magic-items-verify-cleanup`;
  run IDs `20260825T1602570995398Z-51520668bfd24971bebf7c6ed15acef8`
  and `20260825T1605292417607Z-915e55444b314104ba3784c71a5765a5`.
  The exact fixture persisted across a fresh process and cleanup left the
  disposable save with zero fixtures.
- Canonical working-save smoke: PASS 11/11; evidence
  `runtime-evidence/20260825T1608057120336Z-working-save-smoke`; run ID
  `20260825T1608057420923Z-4aa684664b6342a8b58803cd0803829a`.

All real launches used the guarded mechanism and Steam App ID 640820. The
release publisher performs two further clean deterministic builds from the
final release commit and records the final public hashes in
`release-manifest.json` and `SHA256SUMS.txt`.

## Completed human acceptance and 0.0.101 regression checklist

The accepted 0.0.100 checklist is retained as the post-install regression
checklist for 0.0.101. Perform all steps in one fresh process and retain a fresh
UMM output log:


This is an archival 0.0.101 checklist. For current KMG, retain its UI and
identity checks but use the targeted-bridge contract and 20/2/24 ammunition
costs above rather than its historical lifecycle and pricing rows.
1. Confirm CMI reports KMG 0.0.101.
2. Open **Craft Mundane Items**.
3. Confirm **Firearms** offers exactly Pistol, Musket, and Blunderbuss.
4. Confirm Advanced Rifle and Advanced Revolver are absent.
5. Confirm Nodachi appears under Martial Weapons.
6. Confirm Wakizashi, Katana, and Elven Branched Spear appear under Exotic Weapons.
7. Confirm no separate **Eastern and Elven Weapons** magic category exists.
8. Craft one 20-unit batch of each ammunition item.
9. Confirm each project estimate is approximately one safe crafting day with the same crafter/settings.
10. For archival 0.0.101 verification only, confirm prices remain 34, 4, and 40 gold at price scale 1.0.
11. Confirm Work in Progress reports target/progress consistently.
12. Enchant one owned Eastern or Elven weapon through Arms and Armor.
13. Inspect a newly crafted magical Pistol.
14. Inspect an upgraded battered starter Pistol while loaded.
15. Confirm Anarchic, Enhancement +5, and Reliable text remains.
16. Confirm no `<null>` text or phantom blank qualities appear.
17. Save, exit, reload, and inspect the representative items again using only an authorized disposable save.
18. Confirm firearm state and battered-origin behavior remain intact.
19. Confirm no CMI GUI rendering error.
20. Confirm no KMG bridge fault, layout mismatch, or graph rollback.

Human acceptance status: **accepted**. The repository owner explicitly accepted the installed 0.0.100 candidate
on 2026-08-25 and directed its
finalization, merge, push, and publication under an incremented version. The
0.0.101 promotion changes release/version metadata, not the accepted gameplay
or UI architecture.

## Remaining uncertainty

Structured runtime evidence proves the actual patched renderer route, project
data, inventory identities, native tooltip data, and a guarded fresh-process
save/reload. Human acceptance supplies the distinct visual/interaction
evidence for the accepted 0.0.100 behavior; the checklist remains useful for
installation-specific regression review. No official downloaded CMI 2.1.0
binary was available; support is qualified for the exact installed authority
above and compatible capability shapes. A changed shape fails closed.
CMI-crafted KMG custom items retain CMI's ordinary both-mods-required
persistence limitation.
