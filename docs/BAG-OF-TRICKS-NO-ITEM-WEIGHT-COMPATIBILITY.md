# Bag of Tricks No Item Weight compatibility

Mission: [planning/BAG-OF-TRICKS-NO-ITEM-WEIGHT-MISSION.md](../planning/BAG-OF-TRICKS-NO-ITEM-WEIGHT-MISSION.md),
branch `codex/bag-of-tricks-no-item-weight`, draft PR #199. Development work only:
no merge, tag, release, version change or installation into the owner's game.

## Player-visible result

- **No Item Weight on (at startup, or switched on later):** Tabletop Rules Expansion
  initializes completely. Expanded Summoning and every other enabled module
  publish. Every product item reads 0 lb in its tooltip, in stacks and in the
  party's carried weight, exactly like native items.
- **No Item Weight off, Bag of Tricks present or absent:** every item has its
  intended weight. That includes items created while the setting was on, and
  items in saves made while it was on. Naturally weightless items (Paper
  Cartridge, natural weapons) stay weightless.
- Switching the setting changes the same item instances immediately, through the
  game's own getters. There is no restart, re-registration or item replacement.

## Root cause

Bag of Tricks 1.16.4 implements No Item Weight with Harmony postfixes that
return 0 from six Kingmaker 2.1.7b getters: `BlueprintItem.Weight`,
`BlueprintItemWeapon.Weight`, `BlueprintItemArmor.Weight`,
`BlueprintItemShield.Weight`, `BlueprintWeaponType.Weight` and
`BlueprintArmorType.Weight`. Each postfix reads `Settings.toggleItemsWeighZero`
through `StringUtils.ToToggleBool` on every call. That function throws for a
string that is neither the on nor the off value.

0.0.193 used those public getters both as construction inputs and as fatal
validation inputs. With the setting on, the spear type profile saw stored 10
but a live value of 0 and aborted the whole blueprint transaction (the
reporter's line 828). Construction also copied `source.Weight` (magic firearms)
and `donor.Weight` (Magic Circle and Teleportation scrolls) into new items, so
items first built while the setting was on would have kept zero as their
authored weight.

## Engine contracts

These come from the decompiled 2.1.7b `Assembly-CSharp`, read-only, in the lab's
private references.

- `BlueprintItem.Weight => m_Weight` (virtual).
- `BlueprintItemWeapon.Weight` and `BlueprintItemArmor.Weight` return their own
  `m_Weight` when it is above 0, and otherwise their type's `Weight`.
- `BlueprintItemShield.Weight` returns its armor component's weight.
- Every consumer of item weight reads `Blueprint.Weight`:
  - `ItemEntity.TotalWeight => Count * Blueprint.Weight`, used by
    `EncumbranceHelper.GetPartyCarryingCapacity`, `UnitBody` and the loot
    collector;
  - `UIUtilityTexts.GetItemWeight` (tooltips);
  - vendor deal weight;
  - inventory sorting.
- Product items use only native item classes. The census types are
  BlueprintItem, BlueprintItemWeapon, BlueprintItemArmor, BlueprintItemEquipmentBelt
  and BlueprintItemEquipmentUsable. Bag of Tricks' postfixes therefore already
  cover every product item path.

**Decision:** Bag of Tricks' existing getters are sufficient. The product adds
no integration, no dependency, no weight override and no settings access.
Native Harmony readback shows all six getters owned by `BagOfTricks`
(see BW03). The static IL listing used during investigation is not bound by
hash to the installed DLL; the native readback is the evidence for the installed
binary.

## Design: authored weight versus effective weight

- **Authored weight** is the serialized `m_Weight`, read through the new
  `Blueprints/AuthoredItemWeight.cs` (`Stored`, `StoredEquals`). Construction
  writes it from the catalogs, or copies a donor's *stored field* for items that
  deliberately inherit the donor's weight. Validation judges it. Wrong authored
  data still fails startup.
- **Effective weight** is the live getter value. It is a legitimate runtime
  setting, so it is never a construction input and never a fatal check. Startup
  reads it in exactly one place: a contained, nonfatal diagnostic logged once
  (`[elven-branched-spear][weight.runtime-override] … authored weight=10
  effective=0 … getterPatches=BagOfTricks`).

### Audited sites (current source)

| Site | Before | After |
| --- | --- | --- |
| `ElvenBranchedSpearBlueprints.cs` `SpearWeaponTypeAccess` weight predicate | live `Weight` getter compared to the engine contract (fatal) | owned `m_Weight` against the catalog only; getter not read; donor field as context |
| `ElvenBranchedSpear/ElvenBranchedSpearProfilePolicy.cs` | every effective mismatch fatal | `IsRuntimeModifiable("weight")`: weight is judged on the owned field only; every other predicate keeps its fatal effective-getter check |
| `SpearItemAccess.Validate` / `ValidateNamed` | `item.Weight == 10` | stored `m_Weight == ElvenBranchedSpearCatalog.WeightPounds` |
| `EasternWeaponBlueprints.cs` (type, generic item, named item) | live getters against the catalog | stored fields against the catalog |
| `WeaponTypeMechanicalAccess.cs` (firearm type) | live type getter | stored type `m_Weight` against the spec |
| `ProductionFirearmBlueprints.cs` (firearm item) | live item getter | stored `m_Weight` against the spec |
| `MagicFirearmBlueprints.cs` construction | `source.Weight` (0 while on) | `family.Spec.WeightPounds` (catalog) |
| `MagicFirearmBlueprints.cs` validation | live item getter vs live family getter (0 == 0 hides defects) | both stored weights against the catalog |
| `ProgressionWeaponBlueprints.cs` | live item vs live canonical item | stored vs stored (canonical validated against its catalog) and > 0 |
| `BasicAmmunitionBlueprints.cs` | live getters in validation and in the ready log | stored fields (Paper Cartridge stays an authored 0) |
| `FirearmRepairKitBlueprints.cs` (legacy identity) | live getter in validation and log | stored field; identity unchanged |
| `MagicCircleScrollBlueprints.cs` construction | `donor.Weight` | donor's stored `m_Weight` |
| `TeleportationScrollBlueprints.cs` construction (identities registered even when publication is off) | `donor.Weight` | donor's stored `m_Weight` |
| `BlueprintItemAccess.cs` | `Configure*` writes `m_Weight`; `Capture` reads the field | unchanged (already field-based) |
| `Bootstrap/BlueprintBootstrap.cs` | full transaction and rollback | unchanged (and unedited); no `MarkFailed` or rollback path added |
| `Gunsmithing/BatteredFirearmOriginRuntime.cs` and pricing | 0.0.193 unavailable-state safeguard | unchanged; covered by BW10 |
| Expanded Summoning natural/body items, Cord, Gunsmith/Overhaul kits | constant `m_Weight` writes | unchanged |

Remaining `.Weight` reads in `RuntimeTesting/` and `Development/` are
diagnostics outside startup and were left as they are.

## Native evidence

**Environment:**

- Isolated loader-compat lab `game-copies/loader-compat`, UMM 0.31.1.0,
  Kingmaker 2.1.7b.
- Bag of Tricks 1.16.4, `BagOfTricks.dll` SHA-256
  `d03626594ece0f339aeef03ec7259684af7ddeb8b2edf41936f144848059a0e6`. This is the
  same hash as the owner's installation. Byte identity with the reporter's copy
  is not established.
- The reporter's raw Log.txt was not available on this machine. The work started
  from the excerpts quoted in the mission.

**Artifacts:**

- Current candidate: development package
  `KingmakerGunslinger-0.0.193-development-bag-of-tricks-no-item-weight-c4e1259457e0.zip`.
  - Built from commit `c4e1259457e0`, which includes the round-1 truthful
    weight diagnostic. Later commits change only sessions and documentation.
  - Package SHA-256 `c2f6a95d371c4ae60f982b37b9d69eed01cbbeb379af91bfaf611353948fca52`.
  - DLL SHA-256 `d31be89b7614521862ffadfc4a86944b284eac7a90abe22166ab6f85d6f242c2`.
- Earlier candidate: `f48d60ceaf33` (package `9e95d45d…`, DLL `e5ebf25e…`),
  used by the round-0 sessions listed below.
- Harness: `tools/LoaderCompatHarness` at `c4e1259457`, test only.
- Baseline: published `KingmakerGunslinger-0.0.193-spear-bootstrap-tooltip-repair.zip`.

**Restoration and the disposable lab.** Every session completed with
"transaction completed, game settings applied, owner files verified". That
verdict covers the owner's installation, saves and registry; none of them was
touched.

The lab copy itself works as follows:

- Its configuration source is `game-copies/loader-compat/.lab-pristine`. That
  folder holds the loader, the mods and Bag of Tricks' `Settings.xml`, and no
  session writes it.
- Every session rebuilds the copy's live `UnityModManager`, proxy and `Mods`
  from that source (`Install-LcLoaderProfile`, `Set-LcGameCopyMods` in
  `LoaderCompat.Common.ps1`), and stages its candidate and harness on top.
- The next session discards that staging.

Digests recorded by `botWeightSetting` in session `20261122T161435Z`:

- Pristine `Mods/BagOfTricks/Settings.xml` SHA-256
  `37349d1117a70855e8e02663c82d7ca7ba4c3114d205531f78ce8cc1428bdaba`, with No
  Item Weight off. The same digest was read at the first step of launch 1 and at
  the final step of launch 6.
- Pristine `BagOfTricks.dll` SHA-256 `d036…a0e6`.
- The session's staged `Settings.xml` read `37349d11…` (off) before launch 1
  changed it. After launch 5 turned the setting off again through Bag of Tricks'
  own save, the staged file is byte-identical to the pristine file again.

A peer session (`risk03`, owner-0.33.0 loader) used the same lab immediately
before this mission's first session (`20261122T143026Z`) and after its round-0
sessions (`20261122T152012Z`). The live staging therefore belongs to whichever
session ran last. Restoring it to a pre-mission snapshot would overwrite a
peer's work, and every session rebuilds it from the unchanged pristine source
anyway. No lab file was edited by hand.

| Session (evidence under `runtime-evidence/loader-compat/`) | Candidate | Mods | Result | Time |
| --- | --- | --- | --- | --- |
| `20261122T143947Z-…-baseline` | published 0.0.193 | BagOfTricks | PASS (failure reproduced) | 64 s + 56 s |
| `20261122T144208Z-…-no-item-weight` | f48d60ceaf33 | BagOfTricks, CallOfTheWild | FAIL: census float-comparison observer defect only | ~10.5 min |
| `20261122T145742Z-…-no-item-weight` | f48d60ceaf33 | BagOfTricks, CallOfTheWild | PASS 6/6 | ~10 min |
| `20261122T150751Z-…-absent` | f48d60ceaf33 | none | PASS 2/2 | 75+65 s |
| `20261122T155547Z-…-absent` | c4e1259457e0 | none | PASS 3/3 | 112+62+62 s |
| `20261122T160004Z-…-no-item-weight` | c4e1259457e0 | BagOfTricks, CallOfTheWild | FAIL: launch 3's census step lacked its contract table (session authoring); its other checks passed | ~13.5 min |
| `20261122T161435Z-…-no-item-weight` | c4e1259457e0 | BagOfTricks, CallOfTheWild | **PASS 6/6** | 114+199+148+80+80+165 s |

Both failed runs are retained, and neither was a product failure:

- `20261122T144208Z`: the harness compared a float token with the same value
  re-read as a double (`0.1 -> 0.1`). Fixed in `c43232e6c8`, then the whole
  session was rerun.
- `20261122T160004Z`: launch 3's census step had no contract table, so all 213
  items reported "no construction contract". Its live-zero check and its
  comparison with the startup census passed (213 resolved, 215 compared,
  0 differences). Fixed in `35662110d7`, then the whole session was rerun.

Sessions are committed under `scripts/compatibility/loader-compat/sessions/` as
`bag-of-tricks-no-item-weight{,-absent,-baseline}.json`. The harness ops live in
`tools/LoaderCompatHarness/WeightProbes.cs`:

- `botWeightSetting` sets `toggleItemsWeighZero` on Bag of Tricks' own
  `Main.settings`, the same object its patches read (`sameSettingsObject: true`).
  It saves through Bag of Tricks' own `ModSettings.Save`, and it records the
  pristine and staged settings digests.
- `kmgWeightCensus` checks every manifest identity: live weights, the stored
  weight against its construction contract, and missing or mistyped planned
  identities.
- `kmgWeightItems` checks real inventory instances: acquisition quantity,
  per-entry persistence tuples, `Blueprint.Weight`, `TotalWeight`, tooltip text
  and carried weight.
- `kmgInventoryWeightView` observes the open inventory window across its native
  refresh after a live toggle.
- `kmgPublicationStatus` checks the bootstrap's published-module state.
- `kmgWeightProbeSelfTest` runs synthetic cases of the probes' own verdict
  functions.

## Acceptance ledger

Unless a session is named, results are from `20261122T161435Z` on candidate
`c4e1259457e0`. The round-0 results on `f48d60ceaf33` (`20261122T145742Z`,
`20261122T150751Z`) agree with them.

| ID | Result | Evidence |
| --- | --- | --- |
| BW01 | PASS | Published 0.0.193 with the real Bag of Tricks 1.16.4, setting on at startup: `kmgInitialization` failed. The log holds `Elven Branched Spear type profile is invalid`, `weight [effective-getter]` and `getterPatches=BagOfTricks`, matching the report. Reproduced once. |
| BW02 | PASS | Launch 2, on at startup with the reporter modules (`gunslinger=false`): `blueprintsInitialized=true`, `blueprintsFailed=false`, empty failure chain. The log has `[blueprints][initialize.complete]` and `[elven-branched-spear][foundation.ready]`, and has no `initialize.failed`, `][publication.failed]` (any contained module publication failure), `package-blueprint-lifecycle-failed` or profile-invalid line. `kmgPublicationStatus`: `MagicCircles`, `MagicCirclePublication` and `TeleportationScrolls` are set. The check catches a partial publication: absent session `20261122T155547Z` launch 3 forced the Magic Circle spell-list publication to throw; the bootstrap still completed and logged `[magic-circle][publication.failed]`, and the status check failed with `MagicCirclePublication is null, expected set` (asserted through `expectFailure`). |
| BW03 | PASS | Census of every manifest identity: 7,935 entries; 213 planned item entries; 213 resolve to their planned item types (194 BlueprintItemWeapon, 7 BlueprintItemEquipmentUsable, 6 BlueprintItem, 5 BlueprintItemArmor, 1 BlueprintItemEquipmentBelt). This includes Teleportation scrolls (publication off), firearms (Gunslinger off) and legacy `KMG.Test.*` identities. A missing or mistyped planned identity now fails the census. Absent session launch 1 added a synthetic required identity and the census failed with `planned item KMG.Synthetic.MissingWeightItem (…beef) does not resolve`; the real full-set census passed in the same launch. **Stored weights:** 115 positive, 98 zero. Of the 98, 23 inherit a positive weight from their weapon or armor type (for example the legacy Test Musket at 8 lb and the giant armors) and 75 are normally weightless (natural/body items and Paper Cartridge). **Construction contracts:** every one of the 213 stored weights matches its contract. Catalog values: spear 10; wakizashi/katana/nodachi 2/6/8; Cord 1; Gunsmith's Kit 2; Overhaul Kit 1; Black Powder and Lead Ball 0.1; Paper Cartridge 0; repair kit 1; summon body items 0. Donor fields: Magic Circle scrolls against donor `c2cadc1c…`; Teleport, Greater Teleport and Word of Recall against their donors in the constructor's fallback order. Weapon type: firearm and pistol-whip items, where the unpatched weight equals the weapon type's stored catalog weight. An item without a contract fails. Setting on: every live item and weapon-type getter reads 0. Live off: every getter equals the unpatched value computed from fields. Harmony owners of all six getters: `BagOfTricks`. |
| BW04 | PASS | 21 initial real instances on the fixture save. **Weapons:** spear base, named and +3; Eastern masterwork, named and +2; production pistol (battered-bound); magic +1 pistol; named magic pistol; reliable +2 musket; legacy Test Musket (stored 0, takes its type's 8 lb). **Stacks and supplies:** Lead Ball ×10, Black Powder ×10, Paper Cartridge ×5, legacy Firearm Repair Kit ×3, Gunsmith's Kit. **Other:** Cord; product armor `HillHideArmor` (stored 0, takes its armor type's 25 lb); Magic Circle scroll ×2; Teleport scroll; native Longsword. Each acquisition verified the held-count change and one stack per stackable request. With the setting on: `Blueprint.Weight` 0, `TotalWeight` 0, tooltip `0 lbs` / `0 (0) lbs`, party carried weight 0.0 (against an unpatched sum of 159.1). Items added after enabling (magic +1 musket, Magic Circle Law ×3, Word of Recall scroll) also read 0. |
| BW05 | PASS | **Same instances:** launch 2 went on → off → on and launch 6 went off → on → off. Each step verified the same objects are still held, the counts are unchanged and stored `m_Weight` is unchanged. Live-off values: spear 10, katana 6, CloudCleaver 8, pistol 4, Test Musket 8, hide armor 25, Lead Ball stack 0.1 (1 total). **Normal inventory refresh** (`kmgInventoryWeightView`): each toggle ran with the real inventory window open. In the toggle snapshot the view still held its previous carried weight. Kingmaker's own `Inventory.Update` capacity comparison then called `StashEncumbrance.Update` within the same frame (the stash uses the encumbrance bar; there is no text label) and bound the new value. Observed values: launch 2, off: 0 → 159.1 = unpatched sum; launch 2, on: 165.3 → 0; launch 6, on then off: same pattern. At the refresh, the tooltip texts of the spear, Lead Ball stack, hide armor, Test Musket or repair kit, and the native Longsword control all matched the new state. Census comparison after toggling: 215 identities, 0 differences. |
| BW06 | PASS | Items constructed at startup while on, then read live off: magic +1 pistol 4, Duelist's Rebuttal 4, reliable +2 musket 9, Magic Circle scrolls 0.2, Teleport/Greater Teleport/Word of Recall scrolls 0.2. Each also matches its construction contract (BW03). Launch 6 (constructed with the setting off) against launch 2 (constructed with it on): 215 identities, 0 stored or unpatched differences, natives included. |
| BW07 | PASS | Saved while on (fixture `81740820…`). The fixture records one tuple per inventory entry: quantity, enchantments and battered owner. Launch 3 (fresh process, on): every recorded identity was found with the same entry count, the same per-entry tuple multiset and the same stored weight, and every live weight was 0. Launch 6 (fresh process, off): the same, with live weights equal to the authored ones. The fixture was not overwritten after launch 2. The probe's own self-test (launch 1) shows that a stack of ten split into singletons, or an enchantment or owner moved to another entry, fails verification, while an identical multiset passes. |
| BW08 | PASS | Absent session `20261122T155547Z`: Bag of Tricks not installed (`present=false`); initialization completed with Magic Circle published; all 213 census items live equal unpatched, with every contract met; instances (armor, Test Musket and repair kits included) and carried weight 124.1 = unpatched sum; native prices and tooltips correct. Present-and-off: launch 6 above. Unreadable-runtime-weight launch: with the spear type `Weight` getter throwing inside validation, startup still completed and the diagnostic recorded it. No optional integration discovery exists to fail. |
| BW09 | PASS | Launch 2, reporter configuration with `gunslinger=false` and the setting on. A new level-1 sorcerer knows `SummonMonsterISingle` with 7 variants (Fire Beetle, Dog, Eagle, Poisonous Frog, Pony, Dire Rat, Viper). The real creature menu showed 7 visible entries. Selecting Beetle, Fire and clicking the ground returned command result `Success` and summoned `KMG_Summoning_Unit_FireBeetle` (714b0410…). Residue check passed. |
| BW10 | PASS | `kmgItemPricing` (launch 6 and the absent session): native Longspear, Longsword and two ordinary weapons gave native sell prices and non-throwing tooltip fills; a battered pistol gave the fixed scrap price with the main character as owner; no `battered-origin.inspection-unavailable`. Battered ownership also survived save and reload (BW07). |
| BW11 | PASS | Natively, with the setting on, launch 4 wrote `m_Weight=0` onto the owned spear during validation. Startup failed with `weight [owned-field]: expected=10 actual=0 donor=9; getter not read (runtime setting); fields only`. The stored fields are labelled as fields, and no live getter value is reported because none was read. Launch 5 added +1 ft to the owned spear's live reach, and startup failed with `attack-range [effective-getter]`. Domain tests `bag-of-tricks-no-item-weight.*` cover the policy: a stored 0 or 9 fails and prints no getter value; every non-weight predicate keeps its fatal getter check. |
| BW12 | PASS | The native Longspear type is unchanged from its pre-product capture (`m_Weight` 9). Native comparison items (Standard Longspear, Standard Longsword) keep their stored and unpatched weights across on, off and absent, and their live weight follows Bag of Tricks (0 while on). No GUID, manifest row, released key or module semantics changed: development validation shows identities=7935, appended=0. |
| BW13 | PASS | Override active before construction: launch 2. Override activated after construction: launch 6 off → on. Bag of Tricks setting metadata is never read by the product. Not varied: the UMM load order of Bag of Tricks relative to this mod (the patches are stateless postfixes evaluated per call). |
| BW14 | see Validation | Production compilation, focused selection, candidate identity and restoration are recorded below. Independent review is the controller's next step. |

## Validation

- **Focused gate, controller receipt:** `verify --gate focused -- -Filter
  bag-of-tricks-no-item-weight` on the committed head compiles production and
  runs the 3 `bag-of-tricks-no-item-weight.*` cases. The controller refuses a
  multi-term filter argument.
- **Wider focused selection, local runs:** `Invoke-KmgGate.ps1` and the
  candidate build were run with `bag-of-tricks-no-item-weight` plus
  `elven-branched-spear.bootstrap-repair`: 18 of 3,166 tests, 0 failures, which
  adds the 15 existing spear bootstrap-repair cases. These were on product
  source identical to the head.
- This is a focused pass, not full qualification.
- **Development candidate:** `Build-Local.ps1 -DevelopmentPurpose
  bag-of-tricks-no-item-weight` took about 6 min. It ran the repository wrapper
  validators, development-candidate validation against v0.0.193 (appended=0),
  the focused tests, the exact build and strict package validation.
- **Native time:** about 25 min in round 0. Round 1 took about 31 min: the
  absent session, the main session whose authoring omission failed launch 3,
  and its rerun. Both rounds exceed the 15-minute planning target because of the
  reruns. Development builds took about 6 min each round.
- **Deferred by policy:** the complete domain suite, the Sprint/Tranche gates,
  the full compatibility matrix and the historical 43-mod stack.

## Limitations and deferred items

- The tested mod subsets were Bag of Tricks + Call of the Wild, Bag of Tricks
  alone, and none. About 40 of the reporter's other mods were absent.
- The inventory refresh is observed through the values bound to the native view
  (the `PartyCapacity` field and the `StashEncumbrance.Update` call) and the
  tooltip text function. No screenshot of the encumbrance bar was taken.
- The obsolete Overhaul Kit is covered by the census and its contract only. The
  legacy Firearm Repair Kit is also used as a real instance and survived save
  and reload.
- Product-side automated tests are policy-level domain tests. Real construction
  and validation are proved by the native sessions.
