# Regional vendor melee withdrawal: status

Current (2026-11-12). **Released in v0.0.185** (see Publication below). Developed on branch
`codex/regional-vendor-melee-withdrawal` from master `dd6a3ade75` (v0.0.184 publication).

## Owner decision

Remove this mod's authored weapon stock from the Dire Narlmarches village trader and the Pitax trader. These
regional merchants are not appropriate sellers for the added weapon families. Nothing is relocated, and no
new purchase route is added.

## Scope

| Merchant | Table | GUID | Retired rows |
|---|---|---|---|
| Dire Narlmarches village trader | `DireNarlmarchesVillageVendorTable` | `f072a8f6889b5f345b7f4e7c74cb3e4c` | 12 Eastern + 6 spear |
| Pitax trader | `PitaxTownVendorTable` | `e5ab1fccf37c55f41a20a80c6ba6a460` | 12 Eastern + 6 spear |

Eastern rows: Wakizashi, Katana and Nodachi, each in ordinary, masterwork, cold iron and +1 (the
`EasternWeaponGenericKind` values Mundane, Masterwork, ColdIron and PlusOne). Spear rows: the six
`ElvenBranchedSpearItemKind` values (ordinary, masterwork, cold iron, masterwork cold iron, +1, +1 cold iron).
All are count one. The total is 36. No firearm publisher supplied either table. Neither table held a named
weapon at the baseline.

## Change

- `EasternWeaponCampaignBlueprints` and `ElvenBranchedSpearCampaignBlueprints`: both regional specs keep their
  exact GUID and name with empty desired kinds. This is the mechanism the capital already uses. Publication
  still validates identity, removes project-owned fixed rows, keeps native and foreign rows in order, rolls back
  transactionally and is idempotent. Bootstrap still gates each publisher on its own module.
- No item blueprint, GUID, enchantment, price, model, compatibility registration, Better Vendors rank or other
  merchant changed. No new reconciliation code was added.

## Existing saves (source-derived)

The decompiled installed 2.1.7b `SharedVendorTables` shows the following. `GetTable` runs once per table per
loaded game:
1. It computes `GetLootDifference(KnownItems, GetFixedItems(Loot))`.
2. For each difference, `UpdateLootItems` adjusts the persisted blueprint/count `Entries`, never below zero.
   A negative difference never adds an entry.
3. It sets `KnownItems` to the current fixed rows.

Null `KnownItems` yields no difference. `PreSave` rebuilds `Entries` from the live collection by blueprint and
count. This matches the Oleg precedent (v0.0.183).

| Case | Result |
|---|---|
| Untouched stocked copy | removed (1 → 0) |
| Purchased copy | nothing to remove; player copy not part of the path |
| Bought, then a matching weapon sold back | the sold copy is removed (1 → 0); extra copies survive (3 → 2) |
| Repeat (reopen trade, reload after saving, republish) | no further subtraction |
| Older save reloaded without saving | the same single subtraction repeats from the saved state |
| Missing known-item history | no difference; copy kept |

Player inventory, equipment, party storage, companions and the stash are never read or written. Ordinary
future sales to either merchant are unchanged.

## Validation

| Check | Command | Duration | Result |
|---|---|---|---|
| Focused gate (first run) | `Invoke-KmgGate.ps1 -Filter 'model-d.,eastern-weapons.campaign-publication,elven-branched-spear.campaign-publication,elven-branched-spear.btsl-publication'` | 20 s | compile PASS; 29/30, with `eastern-weapons.campaign-publication` still pinning the stale `42 + …` observer token |
| Focused gate (after token fix) | `Invoke-KmgGate.ps1 -Filter 'model-d.,eastern-weapons.,elven-branched-spear.,better-vendors.,acquisition.,presentation.'` | 5 s (incremental) | compile PASS; **126/126** |
| Focused gate (final tree, after docs) | same filter plus `feature-modules.` | 3 s (incremental) | compile PASS; **128/128** |
| Built domain exe, all cases (shared `RuntimeTestRunner.cs` edited) | `KingmakerGunslinger.DomainTests.exe` without a filter | 84 s | 2,927/2,950; all 23 failures are a missing gitignored machine-local `artifacts/inspection/bodyguard-native/Assembly-CSharp.il` in the fresh worktree (environmental, unrelated) |

The complete-suite run is a supplementary local check, not Sprint qualification.

Coverage:
- New `model-d.RegionalTradersPublishNoAuthoredMelee` pins both regional specs, with exact identity and empty
  stock in both publishers. It also pins Oleg's early Eastern kinds and both Honest Guy tables' complete sets.
- Updated protected hashes: `model-d.RegionalEasternAndNamedLootUnchanged` and
  `model-d.RegionalSpearsAndNamedLootUnchanged`. The spear hash no longer rewrites the capital spec.
- Updated `elven-branched-spear.campaign-publication`: `AllFoundationKinds()` appears 5 → 3 times.
- Updated `eastern-weapons.campaign-publication`: observer token.

Domain tests cannot load Kingmaker types. Registered-table behavior belongs to the native observer below.

## Observer updates (compiled, not run natively)

`observe-model-d-vendors` (`RuntimeTestRunner.ModelDVendors.cs`):
- **Regional and Pitax tables:** zero owned Eastern or spear rows on both, checked by item identity.
- **Oleg's spear expectation:** corrected from four to the current two (ordinary and masterwork).
- **New `ObserveRegionalWithdrawal`, for each regional table:**
  - The real campaign publishers, called again, keep the registered array reference.
  - 18 stale owned rows injected in memory are removed, while native rows and a foreign ×37 row keep
    identity, order and count. A second publication is idempotent, and the exact array is restored in
    `finally`.
  - The native statics `GetFixedItems`/`GetLootDifference`/`UpdateLootItems` are checked on detached entry
    lists for the untouched, purchased, sold-back, missing-history and repeat cases.

`observe-vendor-table-contracts` (`RuntimeTestRunner.cs`): the Eastern row total `42 + 12×HonestGuy` had been
stale since the 0.0.142 capital retirement (it would have been 30). It is now `6 + 12×HonestGuy`.

## Native checks: NOT RUN

- Fresh table publication on the registered tables: **NOT RUN**.
- Generated-stock reconciliation, reopening trade and save/reload: **NOT RUN**. No authorized disposable
  fixture with a visited Dire Narlmarches or Pitax merchant is known. The owner's campaign was not used.

Reason: a development package build plus a guarded run needs an agreed no-Kingmaker window on DATA. Together
they exceed the 30-minute ordinary ceiling.

Exact remaining check, in a no-Kingmaker window on DATA, from this branch's commit:
1. Build a development candidate: `Build-Local.ps1 -DevelopmentPurpose -ReferenceBundleDir <bundle>`.
2. Run `scripts/Invoke-KingmakerRuntimeTest.ps1 -Scenario observe-model-d-vendors -PackagePath <candidate zip>
   -DevelopmentPurpose …`, then `observe-vendor-table-contracts` with the same candidate, through the
   repository's guarded launcher and exact restoration.

A saved-stock check needs a separately authorized disposable fixture.

## Publication

- Owner review of #143 found no blocking defect and one P3: the Eastern manifest's "Current 0.0.143 publication"
  paragraph still listed 12 regional rows. It was repaired as `4455a418c6`, and the historical Pitax named-row label
  was corrected too.
- Owner: "Repair that one issue then merge and cut a release please." PR #143 was merged as `70eeed101a`.
- Release PR #144 (version metadata, release185 validator, notes, record): repository wrapper PASS plus complete
  suite 2,950/2,950 in 421 s. The owner merged it as `c136527a99`.
- `Publish-Release.ps1` from clean master took 1,363 s: complete suite 2,950/2,950, two identical builds, then
  publication. v0.0.185 is Latest:
  - tag `49bd4a6bc8`;
  - package `c48be70e6aef546e23c8414abec3090af77225248e150eef3bad97259eec5ccf`;
  - DLL `5dc21d40a1a3474d3a0f6fc4cbb8012c664fd0848d280ca62a3109fab4dfa694`;
  - MVID `8c11f422-481b-4a0b-8573-b75bb786fa8f`.
  A fresh public download matched.
- The native rows above remain NOT RUN; the release disclosed them.
