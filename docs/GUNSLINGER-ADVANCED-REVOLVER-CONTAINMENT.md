# The legacy Advanced Revolver: contained as unsupported content (RISK-01 / S4 T5)

Status: **Accepted and closed for its reviewed development scope** as part of Sprint 4 (principal review, 2026-11-18). Acceptance is not merge or release: not merged, not released. Branch
`codex/risk01-advanced-revolver`, draft PR #182, accepted head `e30b19e8fef03bd4c5e614e3e078ca9e0a467345`, stacked on `codex/pol02-deadeye-cost` (draft PR #181).

## Contract

Epic S4 T5, inventory RISK-01: "The unpublished legacy revolver declares capacity six while its state tokens represent
one round. No normal acquisition route was identified." The owner's direction (2026-11-17): contain it as unsupported
content. No six-round state model and no migration. Preserve identity and safe loading. No deletion, replacement or
silent conversion of owned items. No current acquisition route. No claim that capacity six is supported. Future
registration must not silently accept a capacity the state tokens cannot represent, and any exception stays exact to
this identity.

## Investigation

**Identities.** The revolver has two manifest rows, both unchanged by this story:

- `KMG.Firearms.AdvancedRevolverWeaponType` `a7d3b805c579488eaf91e840896f5d80`, internal name `KMG_AdvancedRevolver_WeaponType`.
- `KMG.Firearms.AdvancedRevolverItem` `8ed461fbcc154c51b07e5549211e9f5e`, internal name `KMG_AdvancedRevolver_Item`.

Both were registered by `a6f09e5056` (2026-08-01, Sprint 33) and appear first in tag `v0.0.95`. The definition is
capacity 6, reload 6 rounds per move action, 1d8/×4, 20 ft, one-handed, 4,000 gp.

**Saved firearm state.** `FirearmRuntimeState.Configure` wires the production repository as
`TokenBackedFirearmStateRepository` over the registered `FirearmStateTokenBlueprints` catalog, which is
`FirearmStateTokenCatalog.CreateCapacityOneDiagnostic()`. Its lead-ball states hold at most one round.
`FirearmStateTokenCatalog.Encode` throws `NotSupportedException` for any state with more. The
`VaultBackedFirearmStateRepository` used by the Sprint 33 six-round domain tests is not wired in production. A revolver
reload asks for up to six rounds. The reload transaction then fails at the token write and restores the weapon state
and the ammunition exactly. The weapon-presentation evidence scenario already records this as a "reload fault" with no
rounds loaded and no ammunition consumed. So a six-round state has never been written to an item: an owned revolver
can only carry an empty or one-round token, which the registered catalog decodes.

**Acquisition, per published tag since `v0.0.95`.** Every consumer of the production firearm catalog was listed in
every tag:

| Route | Finding |
|---|---|
| Vendors (Capital, Beneath the Stolen Lands, Oleg) | The revolver appears only in each table's *owned* list, which removes previously injected rows. It was never in a stocked list. A runtime assertion also bans it from managed vendor tables. |
| Craft Magic Items | The bridge appears from `v0.0.101`, with the revolver recognition-only (upgrade an owned item, never create one). The 0.0.98–0.0.100 notes that listed it as a creation base belong to versions never tagged or published. |
| Progression and magic firearms | The progression catalog excludes revolvers. Magic-firearm validation refuses a revolver family. |
| Starting kit | Recognizes production firearms granted to the Gunslinger; it grants none of its own and never the revolver. |
| Loot | No loot or campaign placement references it. |

So no published release offered an ordinary acquisition route, and no legitimate old save is expected to contain the
item. Only external means (a console, another mod or a development fixture) could have produced one. No such save was
fabricated.

**Equipping.** Before this story the item was player-fireable: equippable, with a tooltip claiming "Capacity 6" and
Craft Magic Items listing it as mechanically supported for upgrades. Native `ItemSlot.InsertItem` (2.1.7b IL) does not
re-check equipment restrictions; only the callers' `CanInsertItem` does. So a restriction blocks new equips but does not
unequip an item already equipped in a save.

## What changed

- `Firearms/FirearmCapacityContainment.cs` (new, engine-free):
  - `IsRepresentable` (capacity and rounds per action within `FirearmStateTokenCatalog.DurableLeadBallCapacity = 1`).
  - `IsContainedLegacy`: exactly key `advanced-revolver`, capacity 6, not player-fireable, recognition-only.
  - `Validate`, which throws for any other unrepresentable spec.
- `ProductionFirearmBlueprints` calls `Validate` before registering the five firearms and again when validating the
  registered catalog. A future firearm with an unrepresentable capacity, or the revolver made fireable or published,
  now fails registration instead of being accepted.
- `ProductionFirearmCatalog.CreateAdvancedRevolver` is no longer player-fireable. Through the existing mechanism this
  adds the native `UnavailableProductionFirearmRestriction`, so the item cannot be equipped, and gives Craft Magic Items
  the `Unavailable` role: not created, not offered for upgrade, not recognized as supported. The bridge's
  "advanced-firearms-recognition-only" qualification now expects the Advanced Rifle alone.
- The item description gains one sentence: "Unsupported legacy firearm: this mod cannot keep track of a six-round
  cylinder, so it cannot be equipped." The declared capacity, identities, names, cost and the localization key are
  unchanged. Owned items are not deleted, replaced or converted.

## Tests

| Command | Result | Time |
|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'firearm-capacity;advanced;craft-magic-items;firearm-descriptions;presentation;sprint31;sprint33;production-firearm'` | production compiled; 89 of 89 (the first run found the two failures below) | 26 s, then 4 s |

New `firearm-capacity.*` tests:

- The durable capacity equals the largest round count in the registered tokens, and a two-round state cannot be
  encoded.
- The production catalog passes, and only the revolver exceeds the capacity: unequippable, recognition-only, capacity
  still 6.
- Registration refuses each variant: the revolver made fireable, the revolver published as a crafting base, the same
  capacity under another key, and another capacity under the legacy key.

Updated tests:

- `advanced.catalog-revolver-exact`: the revolver is not player-fireable.
- `craft-magic-items.catalog` and `.feature-matrix`: four recognized firearms, the revolver `Unavailable`. The first run
  failed `.feature-matrix` (expected 5) before this update, and a fixture defect in the new test (an Early Pistol must
  reload as a standard action) was corrected.

`tools/validate_icon_catalog.py` passed, so no pinned source changed.

## Native evidence and limits

- Registration runs at every load of the final Sprint 4 candidate `16708bd5e237` (package `e017a5f3d8ec…`, DLL `cbca54912642…`, MVID `90ce7e7d-9c5a-46ec-9c95-c0734d2faaec`). All three Sprint 4 native sessions on it
  (`20261118T014552Z`, `20261118T014708Z`, `20261118T014826Z`) loaded the mod and used the registered Gunslinger
  class, which would not happen if the new validation threw.
- The final candidate build first stopped at `Test-IconPolishRound2Assets.ps1`, which pins
  `ProductionFirearmCatalog.cs`. The pin was refreshed with a note in `e30b19e8fe` (cherry-picked to the closure
  branch), as DEF-04 had done for `GunTrainingBlueprints.cs`. No dedicated revolver
  session was run: there is no legitimate save or acquisition route to exercise, and the equipment gate is the
  existing mechanism already used for unsupported production firearms.
- The Craft Magic Items qualification runs only with Craft Magic Items installed. It is not exercised by this story's
  sessions.
- A revolver already equipped in a save made by external means stays equipped (the native slot does not re-check
  restrictions). Its reloads keep failing closed with exact restoration, as before.
- Six-round support remains unimplemented by owner direction. Choosing it later means extending the durable
  representation and migrating one-round tokens (the epic's alternative remedy).
