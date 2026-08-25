# Craft Magic Items Compatibility Implementation and Qualification Report

## Result

Kingmaker Gunslinger 0.0.98 is rejected. Its compatibility graph initialized
correctly, but the first human use of **Firearm Ammunition** exposed a
conditional Unity IMGUI control-tree mismatch. The bridge then rolled back the
whole compatibility graph during `OnGUI`, so the category disappeared.

Version 0.0.99 replaces that renderer architecture with one capability-probed
Harmony 2 inner seam. Craft Magic Items (CMI) always renders and owns the
top-level **Mundane Crafting:** selector. KMG handles only the lower panel when
the already-resolved `selectedCraftingData` is the exact bridge-owned ammunition
object, then branches to CMI's common footer.

The repaired candidate passed 1,241/1,241 deterministic tests, the complete
source/build/package gates, a 12/12 real-CMI patched-renderer observer, a 23/23
real-CMI graph observer, and the canonical 11/11 working-save smoke. Human
visual/interaction acceptance of 0.0.99 remains pending and is not claimed.

## Baseline and external authority

| Field | Exact value |
|---|---|
| Original compatibility starting commit | `290c63a9d51955ae5e692e51ffbee343e211b208` |
| Rejected human-test baseline | `d7178d6ae77b79624917f955658231ae67894c51` |
| Working branch | `codex/craft-magic-items-compatibility` |
| Rejected release | `0.0.98-craft-magic-items-compatibility` |
| Repaired candidate | `0.0.99-craft-magic-items-ammunition-ui-repair` |
| CMI UMM ID / entry type / Info version | `CraftMagicItems` / `CraftMagicItems.Main` / `2.1.0` |
| Explanatory source | `bfennema/OwlcatKingmakerModCraftMagicItems` commit `72f87523d0a116f5dfc92c91893d4955fa1eb303` |
| Installed assembly authority | exact unchanged upstream source-built `CraftMagicItems.dll`; not an official downloaded release binary |
| Assembly/File version | `1.0.0.0` / `1.0.0.0` |
| DLL SHA-256 | `4AE2DA61470350B31BEEF162717A604C9CCD322F66193917944EA4A9596E392D` |
| DLL MVID / bytes | `0044a45b-3bca-439e-86c5-a6aa4d42855e` / `231424` |

The installed DLL is runtime authority. The linked source is explanatory only.
Neither it nor CMI source, Data, L10n, Icons, or installed-mod copies enter the
KMG repository or package. Production KMG has no static CMI type or assembly
reference.

## Rejected 0.0.98 evidence and confirmed root cause

The rejected graph was otherwise exact:

`generation=1; itemTypes=4; firearmBases=5; customWeaponBases=4;
ordinaryWeaponRecipes=46; reliableRecipes=1; ammunitionRecipes=3;
namedCreationBases=0`.

On the first human click of **Firearm Ammunition**, KMG logged
`bridge.incompatible phase=ammunition-ui` with a
`System.Reflection.TargetInvocationException`. CMI then logged
`System.ArgumentException: Getting control 1's position in a group with only 1
controls when doing repaint` through `GUILayout.SelectionGrid`,
`UmmUiRenderer.RenderSelection`,
`Main.DrawSelectionUserInterfaceElements`, and
`Main.RenderCraftMundaneItemsSection`. Unity followed with
`GUILayout: Mismatched LayoutGroup.repaint` and an unbalanced `GUIClip` error.

Source, installed IL, and the supplied live log agree on the sequence:

1. The old `RenderMundanePrefix` conditionally owned CMI's entire mundane
   renderer by reading mutable `SelectedIndex["Mundane Crafting: "]`.
2. Its ammunition route invoked CMI's top-level
   `DrawSelectionUserInterfaceElements` a second time, emitted lower controls,
   and suppressed the original method.
3. A Layout/input pass could run CMI's original tree and mutate the selection,
   while Repaint ran KMG's different tree. Unity therefore saw different
   controls and layout groups between event passes.
4. The catch path synchronously called the bridge incompatibility/rollback
   path inside `OnGUI`, then allowed CMI's original renderer after KMG could
   already have emitted controls. This both compounded the GUI mismatch and
   removed the category during repaint.

The whole-method prefix, selection-index ownership decision, duplicated outer
selector, synchronous GUI rollback, and partial-render fallback have all been
removed.

During repair qualification, one additional same-class hazard was found before
human handoff: KMG's lazy `ImmediateModeGui.Label` resolved its reflected label
method only after evaluating the first argument. Its first Layout could emit no
label while Repaint emitted one. The final route instead capability-probes and
prebinds CMI's own `RenderLabelRow` before any custom control is emitted. This
finding does not alter the original 0.0.98 diagnosis; it prevented a second
mixed-pass control count in the repaired implementation.

## Repaired architecture

The bridge still attaches at CMI's non-generic pre-index data seam, after KMG
blueprints exist and before CMI finalizes its indexes. Registration remains one
transaction per finalized CMI graph, with a complete validated off/on rebuild
for safe late attachment. Stable identities, exact AssetGuids, localization,
recipes, indexes, and Harmony ownership remain idempotent.

For mundane UI, the exact installed method body is probed before patching. The
accepted seam is:

`post-selected-crafting-data:ordinary=IL_014d;new-item-bases=IL_0186;
footer=IL_0774;locals=crafter:1,selected:4,recipe:5`.

The probe requires exactly one ordinary-body anchor, exactly one
`NewItemBaseIDs` equipment-assumption anchor, one safe common-footer target,
the expected crafter/selected/recipe locals, and the exact CMI lower-panel
methods and fields, including `RenderLabelRow`. Missing or ambiguous anchors
reject the contract before UI use; no partially matched transpiler is installed.

The injected helper receives CMI's resolved crafter and
`selectedCraftingData`:

- a non-ammunition object, including an equal-looking but distinct object,
  returns to the untouched ordinary CMI body;
- the exact bridge-owned ammunition object renders only the preflighted lower
  panel and branches to CMI's common **Current Money** footer;
- no KMG route redraws or replaces the outer selector or parent/subtype
  selector;
- the route never consults `SelectedIndex` to decide renderer ownership;
- Layout, input, and Repaint therefore traverse the same control tree for a
  selected object.

All reflection discovery, method binding, recipe-array validation, result-item
validation, and argument-shape validation complete before the first
ammunition-specific control. The lower panel uses CMI's normal item selector,
description row, Knowledge (World) information, and recipe-based craft control.
It preserves CMI project, timer, money, inventory, vendor, and sound behavior.

### Failure and rollback semantics

A UI implementation error is now distinct from an incompatible external
contract. Nested `TargetInvocationException` wrappers are recursively unwrapped;
diagnostics retain every exception type, message, and relevant inner stack. A
failure after the custom route is selected is not swallowed and never runs CMI's
ordinary body after a partial render.

No graph mutation or rollback occurs inside `OnGUI`. A render failure marks the
bridge fault and queues any bridge disable/rollback for the next KMG `OnUpdate`
safe lifecycle boundary. Transactional graph rollback remains available for
initialization, index, patch-installation, and genuine contract failures.
Consequently, an ammunition rendering defect cannot remove CMI item types in
the middle of Layout or Repaint.

## Registration inventory

The finalized graph contains these four stable item types exactly once:

- `KMGMagicFirearms`
- `KMGMagicEasternAndElvenWeapons`
- `CraftMundaneKMGFirearms`
- `KMGFirearmAmmunition`

### Firearm creation bases

| Weapon | AssetGuid |
|---|---|
| Pistol | `a303d71d244640959827e9464df5a867` |
| Musket | `6c9cdfa2d47e4894847fa85d5319fbd2` |
| Blunderbuss | `236f3e167f5542bcac22bca72046fb1f` |
| Advanced Rifle | `a267e7bbc10e425f8adb87844d572b29` |
| Advanced Revolver | `8ed461fbcc154c51b07e5549211e9f5e` |

These five canonical bases enter both dedicated CMI Firearms types only while
Gunslinger is active and each production entry remains `IsPlayerFireable`.
Diagnostic, unavailable, and gated identities remain excluded. Pistol +1
(`d0145d0410a34df08d68a67367c1dfc9`), Musket +1
(`3402fe01de1648b187c192500e370f01`), and Blunderbuss +1
(`1dc7efe0792040f187a18adfdc54c6e0`) remain authored targets rather than
additional from-scratch bases.

### Mundane custom-family bases

| CMI category | Canonical base | AssetGuid |
|---|---|---|
| Exotic Weapons | Wakizashi | `b61ee7e62bc9288004eb0121c8f5d37e` |
| Exotic Weapons | Katana | `aba40a9e8302b31e4daa2acf6ab48a46` |
| Martial Weapons | Nodachi | `35b7082d98ff45ba51dce536a1bc68a1` |
| Exotic Weapons | Elven Branched Spear | `6edc216d68810960f85417237748b042` |

Current KMG weapon metadata confirms these classifications. Authored
masterwork/material/+1 forms remain exact index targets. Eastern Weapons and
Elven Branched Spear module state independently gates new creation.

### Ammunition

| Exact result | AssetGuid | Count | Unit value | Batch value | Progress target | Gold at scale 1.0 |
|---|---|---:|---:|---:|---:|---:|
| Black Powder Charge | `ea966bf998a647cf97b0ed92f71c4b7d` | 20 | 10 | 200 | 50 | 34 |
| Lead Ball | `55c29771445947d685dba9e1ead46a42` | 20 | 1 | 20 | 5 | 4 |
| Paper Cartridge | `fea7337cfd06417a853546af9d950f77` | 20 | 12 | 240 | 60 | 40 |

Each recipe returns the existing plain `BlueprintItem`; none is forced into
`NewItemBaseIDs` or an equipment wrapper. The output remains stackable and is
the exact identity consumed by KMG reload/Paper mechanics. The UI observer
proved both Crafting Takes No Time and normal timed-project paths without
changing these already-authorized economics.

### Named upgrade-only exclusions

These campaign uniques are indexed for upgrade when already owned and remain
absent from every from-scratch creation array:

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

CMI receives one recipe for the existing KMG
`BlueprintWeaponEnchantment` `ea10817126e14703878d00e84329244e`.
No enchantment or misfire mechanic is cloned. Repository/tabletop authority
supplies +1 equivalent, caster level 8, existing name/description, normal Craft
Magic Arms and Armor feat behavior, and CMI's ordinary cost/index paths. The
tabletop prerequisite is *mending*; Kingmaker 2.1.7b has no usable Mending
blueprint, so the recipe deliberately has no prerequisite-spell blueprint
rather than inventing one.

The final CMI `RecipeAppliesToBlueprint` boundary and custom-GUID creation
boundary require a `BlueprintItemWeapon` whose actual weapon type has exactly
one canonical `FirearmDefinitionComponent`. This accepts all five current
firearms and CMI clones by marker, while rejecting bows, crossbows, all Eastern
weapons, Elven Branched Spears, arbitrary weapons, and ambiguous duplicate
markers. The recipe participates in every CMI recognition, replacement,
removal, description, plus-equivalent, and pricing index. The real observer
proved a +1/Reliable Pistol at plus-equivalent 2 and exact CMI rules cost 9,300.

## Custom blueprints, state, and module policy

The real CMI observer resolved ordinary +1 and +1/Reliable Pistol clones, a +1
Katana clone, and a +1 Elven Branched Spear clone. Weapon type, exactly one
firearm marker, proficiency, presentation, reload/capacity state, family
category, finesse/reach/grip mechanics, and project-owned zero-cost policy
enchantments remained intact as applicable. Each original base was unchanged.
Owned firearm loaded/condition token identity transferred to the replacement
item exactly once.

New firearm/ammunition creation remains off when Gunslinger is off; Eastern and
spear creation follow their own module gates; unpublished firearms remain
excluded. Already-owned stable items retain the repository's existing policy.
CMI installed but disabled keeps the bridge inactive.

CMI remains the sole owner of generated-blueprint persistence. KMG adds no
second persistence system. CMI-crafted KMG custom items may therefore require
both mods to remain installed.

## Deterministic, build, and package qualification

The repaired source passed:

```powershell
python .\tools\validate_repository.py --root .
.\scripts\test-domain.ps1 -Configuration Release
.\scripts\Build-Local.ps1
.\scripts\validate-build-output.ps1 -Configuration Release
.\scripts\Validate-FirearmSoundBank.ps1
.\scripts\validate-package.ps1 `
  -PackagePath .\artifacts\packages\KingmakerGunslinger-0.0.99-craft-magic-items-ammunition-ui-repair.zip `
  -Configuration Release
.\scripts\Test-RuntimeScenarioPreflight.ps1
.\scripts\compatibility\Test-KingmakerCompatibilityProfile.ps1
.\scripts\compatibility\Test-KingmakerCompatibilityProfileResolution.ps1
.\scripts\compatibility\Test-OptionalModReferenceInventory.ps1
.\scripts\compatibility\Test-OptionalModCompatibilityObserver.ps1
.\scripts\compatibility\Test-ExpandedSummoningCompatibilityProfiles.ps1
```

Results: repository validation PASS; 1,241/1,241 complete domain/reflection
tests PASS; 13/13 focused CMI cases PASS; clean exact-reference Release build
PASS; build-output, firearm SoundBank, deterministic package, and strict
standalone UMM validation PASS; runtime preflight 142/142 PASS; all 12 profile
resolution fixtures PASS; all five Expanded Summoning compatibility profiles
PASS; optional-mod reference and observer contracts PASS.

Focused cases cover absent CMI, accepted/rejected contract shapes, exact and
ambiguous IL anchors, unchanged ordinary routing, exact-reference ownership,
Layout/Repaint route transitions, deferred failure semantics and recursive
exception logging, catalog/module state, Reliable, ammunition economics,
custom graph integrity, load order/idempotence, and package isolation.

| Runtime-qualified package field | Exact value |
|---|---|
| Runtime-deployed source-state SHA-256 | `E045220F6BB9F2D172D56BE12D22FB9940677B659846400722F12BE6F36E434E` |
| KMG DLL SHA-256 | `30FA3C5B93D5611BCC9E19E99859141ADB39E7D323A7A74B69C2F1031BD593A8` |
| KMG DLL MVID / bytes | `de2cf811-b9b2-4a14-94f4-5cec51891e1b` / `4242944` |
| Release/local-runtime package SHA-256 | `80875B5CDC7E18B188CC091C948EE82623B3069B8D86591A86ACA6D6101C1275` |
| Package bytes | `22651866` |

Package path:
`artifacts/packages/KingmakerGunslinger-0.0.99-craft-magic-items-ammunition-ui-repair.zip`.
The package contains no CMI DLL/source/Data/L10n/Icons, game or Unity
assemblies, saves, raw runtime evidence, credentials, or machine-local config.

## Guarded runtime qualification

All launches used the guarded request mechanism and Steam App ID 640820. No
Computer Use, OCR, screenshots, mouse-coordinate automation, or direct
`Kingmaker.exe` launch supplied mechanical evidence.

### Focused ammunition UI observer — PASS 12/12

Command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-craft-magic-items-ammunition-ui `
  -ExpectedVersion 0.0.99 `
  -TimeoutSeconds 240 `
  -ExitAfterCompletion:$true `
  -Confirm:$false `
  -AllowDirtyGit
```

- Evidence directory:
  `runtime-evidence/20260825T0430374441447Z-observe-craft-magic-items-ammunition-ui`
- Run ID:
  `20260825T0430374625241Z-bc2c674153aa42a2a06299923d2f9091`
- Patched target: `CraftMagicItems.Main.RenderCraftMundaneItemsSection`;
  transpiler applications `1`; outer selector owner `CraftMagicItems`.
- Routes: ordinary `4`; ammunition ordinary-body bypass `22`; lower panel
  renders `22`; events `Layout,repaint`; zero GUI failures, UI failures, or
  rollbacks.
- All three exact result GUIDs remained selectable. Invalid crafter and zero
  funds completed balanced Layout/Repaint routes without throwing.
- Immediate crafts created 20 exact units and spent 34/4/40 gold. Timed Paper
  crafting created target 60 / gold 40 / result count 20, completed through
  CMI's normal project lifecycle, and restored state exactly.
- KMG loose reload consumed crafted powder and ball; Paper mode consumed the
  exact crafted cartridge. Inventory/money cleanup was exact.
- Before/after graph remained
  `generation=1;itemTypes=4;firearmBases=5;customWeaponBases=4;
  ordinaryWeaponRecipes=46;reliableRecipes=1;ammunitionRecipes=3`.

This is mechanical evidence from the actual patched CMI renderer hosted under
Unity `OnGUI`; it is not a claim about visual presentation.

### Real CMI graph observer — PASS 23/23

Command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-craft-magic-items-compatibility `
  -ExpectedVersion 0.0.99 `
  -TimeoutSeconds 240 `
  -ExitAfterCompletion:$true `
  -Confirm:$false `
  -AllowDirtyGit
```

- Evidence directory:
  `runtime-evidence/20260825T0433473521309Z-observe-craft-magic-items-compatibility`
- Run ID:
  `20260825T0433473724454Z-d7415250d8a041eebca7a65e33d54352`
- Live CMI `2.1.0` entry was loaded, active, and contract-compatible.
- Generation 1 and forced generation 2 each contained exactly four item types,
  five firearm bases, four custom-family bases, 46 ordinary recipes, one
  Reliable recipe, three ammunition recipes, and zero named creation bases.
- Exact category placement, indexes, Reliable authority/applicability/cost,
  representative clones, base immutability, owned-state transfer, and complete
  rebuild idempotence passed.

### Canonical working-save smoke — PASS 11/11

Command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario working-save-smoke `
  -ExpectedVersion 0.0.99 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true `
  -Confirm:$false `
  -AllowDirtyGit
```

- Evidence directory:
  `runtime-evidence/20260825T0436539145682Z-working-save-smoke`
- Run ID:
  `20260825T0436539425049Z-4c42f59e054f4bf98e5381056bfe9d6f`
- Complete catalog count `111`; unique working and baseline descriptors;
  exact receiver/descriptor correlation and ordered slot/window/load/callback/
  fingerprint sequence; loaded KMG `0.0.99`; stable three-party fingerprint;
  no save-writing API; status PASS.

Failed development runs remain rejected evidence. They exposed the lowercase
Unity `repaint` spelling, a request-harness phase boundary, the lazy label
control mismatch, an unskilled disposable crafter that made external CMI
progress/day zero, and two request-local Pistols returned to inventory during
observer cleanup. Each was fixed and rerun; only the three PASS runs above are
qualification claims.

## Mandatory human checklist for 0.0.99

Perform all steps in one fresh process:

1. Open Craft Magic Items -> Craft Mundane Items.
2. Begin on an ordinary category.
3. Click Firearm Ammunition.
4. Verify the category stays visible.
5. Select Black Powder Charge.
6. Select Lead Ball.
7. Select Paper Cartridge.
8. Switch back to an ordinary category.
9. Switch to Firearm Ammunition again.
10. Close and reopen the UMM window.
11. Switch to another mod tab and return.
12. Craft one 20-unit batch of each item.
13. Confirm money/count/project behavior.
14. Confirm no KMG `bridge.incompatible` line.
15. Confirm no CMI `Error rendering GUI` line.
16. Confirm no `GUILayout`, `LayoutGroup`, `SelectionGrid`, or `GUIClip` error.
17. Save and reload only through an authorized disposable-save procedure.

Human acceptance status: **pending**. The first human UI test rejected 0.0.98;
no human has yet accepted the repaired 0.0.99 interface.

## Remaining uncertainty

The structured observer proves the actual patched renderer route and craft
mechanics, but not visual layout, legibility, or a person's complete UMM
interaction. The checklist above remains required. Save/reload of a
player-crafted CMI/KMG item also remains a human acceptance item. No official
downloaded CMI 2.1.0 binary was available; compatibility is claimed only for
the exact installed authority above and compatible capability shapes. A changed
shape fails closed. The runs used the installed mod stack, including Call of
the Wild, but were not a separately restored profile-isolation runtime test.
