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

- Candidate: development package
  `KingmakerGunslinger-0.0.193-development-bag-of-tricks-no-item-weight-f48d60ceaf33.zip`.
  - Built from commit `f48d60ceaf33` (product source unchanged since `0194ef0c51`
    apart from removing an unused helper).
  - Package SHA-256 `9e95d45d71910afc15f692ce946972d798844f347a6719a5ad52ed23e741bdfa`.
  - DLL SHA-256 `e5ebf25ed36bce41d8e14fdf2c690991d6f6572913fae329d6f1131db6e74b1e`
    (recorded as `productSha256` in every launch).
- Harness: `tools/LoaderCompatHarness` at `c43232e6c8`, test only.
- Baseline: published `KingmakerGunslinger-0.0.193-spear-bootstrap-tooltip-repair.zip`.

**Restoration:** every session completed with "transaction completed, game
settings applied, owner files verified". Each session's staging, including Bag
of Tricks' `Settings.xml`, is removed at completion. The owner's installation
was never touched.

| Session (evidence under `runtime-evidence/loader-compat/`) | Mods | Result | Time |
| --- | --- | --- | --- |
| `20261122T143947Z-umm-0.31.1-bag-of-tricks-no-item-weight-baseline` (published 0.0.193) | BagOfTricks | PASS (failure reproduced) | 64 s + 56 s |
| `20261122T144208Z-umm-0.31.1-bag-of-tricks-no-item-weight` (first run) | BagOfTricks, CallOfTheWild | FAIL: observer defect only, see below | ~10.5 min |
| `20261122T145742Z-umm-0.31.1-bag-of-tricks-no-item-weight` (rerun, corrected observer) | BagOfTricks, CallOfTheWild | PASS, 6/6 launches | 101+110+104+79+81+109 s |
| `20261122T150751Z-umm-0.31.1-bag-of-tricks-no-item-weight-absent` | none | PASS, 2/2 launches | 75+65 s |

The first main run failed only in the cross-launch census comparison. The
harness compared a float token with the same value re-read as a double, so
`0.1 -> 0.1` was reported as a difference. Every other step in launches 2, 3
and 6 passed. The retained raw census records show identical values. The
comparison was corrected in `c43232e6c8`, and the whole session was rerun on
the unchanged candidate. Both runs are retained.

The sessions are committed under `scripts/compatibility/loader-compat/sessions/`
as `bag-of-tricks-no-item-weight{,-absent,-baseline}.json`. The new harness ops
are `botWeightSetting`, `kmgWeightCensus` and `kmgWeightItems`
(`tools/LoaderCompatHarness/WeightProbes.cs`). `botWeightSetting` sets
`toggleItemsWeighZero` on Bag of Tricks' own `Main.settings`, which is the same
object its patches read (`sameSettingsObject: true`). It persists the value
with Bag of Tricks' own `ModSettings.Save`, so "on at startup" is a genuine
fresh-process start with the setting saved.

## Acceptance ledger

Unless a session is named, results are from the rerun `20261122T145742Z`
on candidate `f48d60ceaf33`.

| ID | Result | Evidence |
| --- | --- | --- |
| BW01 | PASS | Published 0.0.193 with the real Bag of Tricks 1.16.4, setting on at startup: `kmgInitialization` failed. The log holds `Elven Branched Spear type profile is invalid`, `weight [effective-getter]` and `getterPatches=BagOfTricks`, matching the report. Reproduced once. |
| BW02 | PASS | Launch 2 (on at startup, reporter modules with `gunslinger=false`): `blueprintsInitialized=true`, `blueprintsFailed=false`, empty failure chain. The log has `[blueprints][initialize.complete]` and `[elven-branched-spear][foundation.ready]`, and has no `initialize.failed`, `package-blueprint-lifecycle-failed` or profile-invalid line. |
| BW03 | PASS | Census of every manifest identity: 7,935 entries; 213 planned item entries; 213 resolve to items (194 BlueprintItemWeapon, 7 BlueprintItemEquipmentUsable, 6 BlueprintItem, 5 BlueprintItemArmor, 1 BlueprintItemEquipmentBelt); 0 unresolved. All are registered, including Teleportation scrolls (publication off), firearms (Gunslinger off) and legacy `KMG.Test.*` ammunition and repair-kit identities. 75 have an authored weight of 0 (natural/body items and Paper Cartridge). Setting on: every live item and weapon-type getter reads 0. Live off: every getter equals the unpatched value computed from fields. Harmony owners of all six getters: `BagOfTricks`. |
| BW04 | PASS | 19 real instances on the fixture save cover spear base/named/+3, Eastern masterwork/named/+2, production pistol (battered-bound), magic +1 pistol, named magic pistol, reliable +2 musket, Lead Ball ×10, Black Powder ×10, Paper Cartridge ×5, Gunsmith's Kit, Cord, Magic Circle scroll ×2, Teleport scroll and a native Longsword. With the setting on: `Blueprint.Weight` 0, `TotalWeight` 0, tooltip `0 lbs` / `0 (0) lbs`, party carried weight 0.0 (against an unpatched sum of 123.1). Items added after enabling (magic +1 musket, Magic Circle Law ×3, Word of Recall scroll) also read 0. |
| BW05 | PASS | Launch 2 went on → off → on and launch 6 went off → on → off, on the same tracked instances. Each step verified the same objects are still held, the counts are unchanged and stored `m_Weight` is unchanged. Live-off values: spear 10, katana 6, CloudCleaver 8, pistol 4, Lead Ball stack 0.1 (1 total); carried weight 123.1 = unpatched sum. On again: 0. Census comparison after toggling: 215 identities compared, 0 differences. |
| BW06 | PASS | Items constructed at startup while on, then read live off: magic +1 pistol 4, Duelist's Rebuttal 4, reliable +2 musket 9, Magic Circle Evil scroll 0.2, Teleport/Greater Teleport/Word of Recall scrolls 0.2. Launch 6 (constructed with the setting off) against launch 2 (constructed with it on): 215 identities, 0 stored or unpatched differences, natives included. |
| BW07 | PASS | Saved while on (fixture `7b46dea4…`). Launch 3 (fresh process, on): every recorded identity was found with equal count, enchantment names, battered owner and stored weight, and every live weight was 0. Launch 6 (fresh process, off): the same identities, with live weights equal to the authored ones. The fixture was not overwritten after launch 2. |
| BW08 | PASS | Absent session: Bag of Tricks not installed (`present=false`); initialization completed; all 213 census items live equal unpatched; instances and carried weight 88.1 = unpatched sum; native prices and tooltips correct. Present-and-off: launch 6 above. Unreadable-runtime-weight launch: with the spear type `Weight` getter throwing inside validation, startup still completed and the diagnostic recorded it. No optional integration discovery exists to fail. |
| BW09 | PASS | Launch 2, reporter configuration with `gunslinger=false` and the setting on. A new level-1 sorcerer knows `SummonMonsterISingle` with 7 variants (Fire Beetle, Dog, Eagle, Poisonous Frog, Pony, Dire Rat, Viper). The real creature menu showed 7 visible entries. Selecting Beetle, Fire and clicking the ground returned command result `Success` and summoned `KMG_Summoning_Unit_FireBeetle` (714b0410…). Residue check passed. |
| BW10 | PASS | `kmgItemPricing` (launch 6 and the absent session): native Longspear, Longsword and two ordinary weapons gave native sell prices and non-throwing tooltip fills; a battered pistol gave the fixed scrap price with the main character as owner; no `battered-origin.inspection-unavailable`. Battered ownership also survived save and reload (BW07). |
| BW11 | PASS | Natively, with the setting on: launch 4 wrote `m_Weight=0` onto the owned spear during validation, and startup failed with `weight [owned-field]: expected=10 actual=0`. Launch 5 added +1 ft to the owned spear's live reach, and startup failed with `attack-range [effective-getter]`. Domain tests `bag-of-tricks-no-item-weight.*` cover the policy: stored 0 or 9 with both getters at 0 fails; every non-weight predicate keeps its fatal getter check. |
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
- **Native time:** about 25 min across the four sessions. That is over the
  15-minute planning target, mainly because of the observer-correction rerun.
- **Deferred by policy:** the complete domain suite, the Sprint/Tranche gates,
  the full compatibility matrix and the historical 43-mod stack.

## Limitations and deferred items

- The tested mod subsets were Bag of Tricks + Call of the Wild, Bag of Tricks
  alone, and none. About 40 of the reporter's other mods were absent.
- The inventory header redraw by Bag of Tricks' own
  `Inventory_UpdatePlayerMoneyAndInventoryWeight` postfix was not captured
  visually. Carried weight is observed through `EncumbranceHelper`, and the
  tooltip text through `UIUtilityTexts.GetItemWeight`. The native comparison
  Longsword follows the setting at the same moments as product items, because
  both read the same stateless getters.
- The legacy Firearm Repair Kit and the obsolete Overhaul Kit are covered by the
  census only. They were not used as inventory instances because retired-kit
  cleanup may remove them from saves.
- Product-side automated tests are policy-level domain tests. Real construction
  and validation are proved by the native sessions.
