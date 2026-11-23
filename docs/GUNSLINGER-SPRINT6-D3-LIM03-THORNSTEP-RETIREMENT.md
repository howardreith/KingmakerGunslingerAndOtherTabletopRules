# Sprint 6 D3 / LIM-03: Thornstep retirement at its exact former source

Status: **D3 / LIM-03 accepted by manual review (2026-11-23)** for its demonstrated induced native scope, at head
`f6bcd2f638`, as intentional behavior of the existing Thornstep retirement implementation. Section 9 records the
decision and corrected claims and governs where it differs. Nothing is merged or released.

| | Identity |
|---|---|
| Branch | `codex/s6-d3-lim03-thornstep-retirement`, stacked on D2 acceptance `d3659698e5` (PR #214) |
| Product | **unchanged**: accepted D1 `634a6efdd3`, package `862027f189cf19656aa288264531fd81acbe91f18910336560eb7352da0fdc37`, DLL `65a0b60f43da49e7217414ad8258e7f02e0546916060a1d4b8bdb7ce175c43ef` (staged version 0.0.193) |
| Harness | `a3621eb0d0` (`kmgThornstepRetirement`, condition `thornstepInducedSource`; `tools/LoaderCompatHarness/ThornstepRetirementProbes.cs`), build `ea03eed3316ea910fbc56c63b09ccb53175237b9c16ca4999bb6882ecd9b6dcc` |
| Fixture | `KMCS_AFTER_NORMAL_012_MODONLY.zks` (prologue throne room); test-only constructed bags, see section 3 |
| Native session | `20261123T185954Z-owner-0.33.0-d3-lim03-thornstep` |

## 1. Outcome

**The existing implementation satisfies the selected policy. No deviation was found, and there is no product change.**

**Player-visible result:** in an older save, the former Thornstep source container keeps:

- every copy the party has held and put back;
- every copy when the container has no saved history;
- its other loot.

It loses only never-held copies (persisted item time zero), never more than the history recorded. Fresh containers never generate Thornstep, and later loads remove nothing more.

**Proposed LIM-03 disposition:** close as **accepted intentional behavior**, not a lost-inventory fix. The limits in section 7 stay attached.

## 2. Native contract and runtime scope (installed 2.1.7b, decompiled)

- **Load route.**
  - `MapObjectEntityData.OnViewAttached` calls `EnsureComponentData` for each map-object component.
  - It finds the saved data by component type name and assigns `component.Data`.
  - The `MapObjectComponent.Data` setter calls `OnDataSetted`.
  - `LootComponent.OnDataSetted` calls `LootPersistentData.UpdateLoot(MapObjectLoot)`, where `MapObjectLoot` is the items of the component's `LootTables`.
- **`UpdateLoot`.**
  - It does nothing when `m_KnownItems` is null.
  - Otherwise, for each item whose definition count differs from its history count, `UpdateLootItem` calls `Loot.Add(blueprint, n)` (raise) or `Loot.Remove(blueprint, n)` (lower), then stores the new count.
  - The lowering path removes by blueprint and count, without provenance.
- **Generation.**
  - A container with no saved data gets `CreateInteractionData()`, which is `new LootPersistentData(MapObjectLoot)`.
  - That constructor adds each row through `ItemsCollection.Add(blueprint, count)` and records the counts as history.
- **`ItemEntity.Time`** is persisted. The constructor leaves it at zero.
  - `ItemsCollection.Add` sets it to `Player.GameTime` only when the target collection is the party inventory (`IsPlayerInventory`).
  - `Split` also stamps it (on the new split entity). These are the two native writers found; the time is not
    universally "set only on entering the party inventory".
  - `ItemsCollection.Transfer` is `Remove` then `Add` of the same non-stackable instance.
  - So a party transfer stamps the item and a return to a container keeps that stamp.
  - `ItemEntity` has **no persisted unique ID**.
- **Product scope (as implemented).** `RetiredLootReconciliationPatch.Prefix` acts on any `LootComponent` whose `LootTables` reference the exact `BlueprintLoot` `3322c56f38031eb4983b6f87c95081b7`.
  - There is **no area or map-object identity gate at runtime**.
  - `Forest_GoodLoot01` and `NorthNarlmarchesRegionLair01` are validated **at publication only**: `ElvenBranchedSpearCampaignBlueprints` requires the exact blueprint name, its `Area.name`, the cleanup row and the absence of the Thornstep row.
  - Thornstep was published as a row of that loot blueprint, so the runtime scope equals the set of containers that could have generated it. How many map objects reference this blueprint was not enumerated.
- **Plan.**
  - Removes zero-time, unequipped Thornstep instances by reference, at most the history count, in collection order.
  - Sets the Thornstep history row to 0 before the native diff.
  - Does nothing if the current definition still publishes Thornstep.
- **Guards and fallback.**
  - `Prepare` pins the MVID and the IL sizes of `OnDataSetted` (59) and `UpdateLoot` (219), plus field types. If a pin drifts, the patch is not applied and native blueprint removal returns.
  - On a reconciliation fault, `Protect` sets the history row to 0 when the data and history are reachable. That is **best effort**:
    - removals already made stay made;
    - a fault in the scope lookup falls through to native behavior.
  - No fault campaign was run (no concrete in-scope risk).

## 3. Scenario: test-only construction, induced former source

The fixture area has no loot container. The smallest admitted route that reaches a real component callback:

- **Containers.**
  - The seed launch spawns seven engine **dropped-loot bags** (`BlueprintRoot.Prefabs.DroppedLootBag`).
  - These are real `MapObjectView`s that require a `LootComponent`, are saved in area state and re-attach through `EnsureComponentData` on load.
  - Each bag's saved `LootComponent` data is replaced with constructed data, without firing `OnDataSetted`. No reconciliation of these inputs runs in the seed launch.
- **Historical inputs (test-only):**
  - historical row lists (the definition's items plus Thornstep rows) given to the native `LootPersistentData` constructor;
  - a direct call of the native `UpdateLoot` for the engine-added copy;
  - a null history for the missing-history case;
  - one extra zero-time copy beyond the recorded count.
- **Identity marker (test-only).** Each Thornstep carries a distinct `SellTime` value. This is a harness marker, not a native identity, and the policy does not read it. Within each launch, exact instances are also compared by object reference.
- **Induction.**
  - In the reconcile and reload launches, condition `thornstepInducedSource` installs a first-priority harness prefix on `LootComponent.OnDataSetted`.
  - For the six scoped bags named in the manifest, it sets `LootTables` to the exact former-source reference and snapshots the loaded data. The seventh bag keeps its own empty tables.
  - The product prefix and native `UpdateLoot` then run in the real load-time callback.
  - A last-priority postfix snapshots the result.
- **What this is not:** a Riverine Rise container, an organic old save or an area transition.

**Predeclared expectations.** Before the save, the seed launch wrote the per-bag inputs and expected results (removed and retained markers, decision, history row after, unrelated contents) and the party state to the isolated profile (`kmg-d3-thornstep-manifest.json`). Cross-launch checks compare against that manifest, or against the reconcile launch's verified state (`kmg-d3-thornstep-verified.json`).

## 4. Results

Session `20261123T185954Z-owner-0.33.0-d3-lim03-thornstep`: **PASS 3/3**, 242 s.

- **seed:** 77 s.
- **reconcile:** 77 s, fresh process.
- **reload:** 72 s, fresh process.
- **Restoration:** transaction completed, game settings applied, owner files verified.

Every Thornstep input is listed as marker@time. `S` is a party-inventory stamp: 1021 = 432310330000 ticks, 1032 = 432310750000. Each scoped bag also holds the definition's unrelated loot: Lesser Restoration ×2, Remove Blindness ×3, gold ×43.

| # | Case (bag) | Input (history) | Expected = observed, automatic load | Native-only would (source) | Repeat (direct callback) and fresh reload |
|---|---|---|---|---|---|
| 1 | Fresh publication/generation | Definition: Restoration, Remove Blindness, gold; **0 Thornstep** | The seed's native constructor gives 0 Thornstep. The induced component's own `CreateData()` gives 0 Thornstep, 3 items. | n/a | n/a |
| 2 | Untouched (`untouched`) | 1001@0 (history 1) | 1001 removed; history 1 → 0; `RemovedUntouched` | Remove it (same result) | No change; `NotInHistory` |
| 3 | Engine-added (`engineAdded`) | Generated from current rows, then native `UpdateLoot` with a Thornstep row added one zero-time copy, 1011@0 (history 1) | 1011 removed; history 1 → 0; `RemovedUntouched` | Remove it (same result) | No change; `NotInHistory` |
| 4 | Claimed and returned (`returned`) | 1021@S, after a real transfer to party inventory and back (same instance) (history 1) | 1021 **kept**, same object, stamp unchanged; history 1 → 0; `RetainedAmbiguous` | **Remove 1021** | Kept; `NotInHistory` |
| 5 | Mixed (`mixed`) | 1031@0, 1032@S (history 2) | **1031 removed, 1032 kept**; history 2 → 0; `RemovedUntouchedRetainedAmbiguous` | **Remove 1031 and 1032** (both rows, from the end) | 1032 kept; `NotInHistory` |
| 6 | Missing history (`missingHistory`) | 1051@0 (history null) | 1051 kept; history stays null; `NoHistoryRetained` | Skip (same result) | Kept; `NoHistoryRetained` |
| 7 | Count limit (`overCount`) | 1041@0 generated, 1042@0 extra (history 1) | **Exactly 1041 removed, 1042 kept** (asserted); 1041's position first in the loaded collection is captured; history 1 → 0; `RemovedUntouchedRetainedAmbiguous` | **Remove 1042** (from the end) | 1042 kept; `NotInHistory` |
| 8a | Same item outside the scope (`unscoped`, own empty tables) | 1061@0 plus 1 unrelated item (history: no Thornstep row) | Unchanged; the product did not act (its outcome did not change) | Nothing (same result) | Unchanged |
| 8b | Unrelated loot (all scoped bags) | Definition items | Identical name\|count\|time multiset before and after, in every bag and every launch | — | Identical |
| 8c | Party-held copies | Inventory 2001@stamped (432312010000); stash 2002@0; inventory 8 items, stash 1; main-character equipment (no Thornstep equipped) | Thornstep marker/time arrays in inventory and stash, inventory and stash total counts, and the main character's equipped blueprint-name multiset equal the seed declaration in the reconcile and reload launches | — | Equal |
| 9 | Repeat and fresh-process save/load | — | Direct repeat: every bag unchanged (same object sequence) | — | Reload: each bag's loaded state equals the reconcile launch's verified state; its load reconciliation changed nothing; no duplication or reintroduction |

The "Native-only" column is source inference from the decompiled `UpdateLootItem` (`Loot.Remove(blueprint, n)` takes from the end). It is not a native run. Cases 4, 5 and 7 are where the product's result differs from it.

## 5. Evidence classes

| Claim | Class |
|---|---|
| Definition contains no Thornstep | Captured (seed) |
| Native constructor and the induced component's `CreateData()` generate no Thornstep | **Direct native generation**, asserted (seed; reconcile). Not a fresh area load |
| Engine-added copy created by native `UpdateLoot` at Time 0 with history 1 | **Direct native call** with a test-only historical row list, asserted (seed) |
| Party transfer stamps and the return keeps the exact instance | **Native `ItemsCollection.Transfer`** to and from the party inventory: exact-object and timestamp preservation asserted (seed) |
| Cases 2–8a results and history rows | **Automatic fresh-load reconciliation** (real `EnsureComponentData` → `OnDataSetted` at area load, induced `LootTables`). Asserted against the predeclared manifest (reconcile): Thornstep marker sets, retained loaded references, resulting history row, unrelated contents. First-load timestamp equality is **captured** |
| Exact-instance removal and retention | Asserted within the load launch by object reference (pre/post snapshots around the callback) and by test-only marker |
| Repeat changes nothing | **Direct native callback** (the `Data` setter), asserted (reconcile) |
| No change after a fresh-process save/load | Automatic fresh-load reconciliation (reload): full recorded snapshot equality (items with blueprint, count, time, marker, holding flag; history) against the reconcile launch's verified state, plus the predeclared markers, asserted |
| Party controls preserved | Asserted against the seed declaration (reconcile, reload) for Thornstep marker/time arrays, inventory and stash totals, and the main character's equipped blueprint-name multiset. Not a complete inventory, slot, enchantment or party-wide equipment identity |
| Product decision strings | Parsed diagnostic values from the product's static `LastOutcome` are checked after each callback, but that static is not an independent per-callback attribution witness (two consecutive identical outcomes cannot be told apart). The removal effects are asserted independently |
| Native-only behavior in cases 4, 5, 7 | Source inference (no matched no-product baseline was run, since no deviation appeared) |
| Runtime scope = exact loot-blueprint reference | Source inference (`RetiredLootReconciliationPatch.Prefix`) |
| Item identity across saves | **Not native.** `ItemEntity` has no persisted ID; cross-launch identity uses test-only markers and times |

## 6. Validation and time

- **Product:** unchanged. No focused gate, production compilation or candidate build was needed; the accepted D1 candidate is reused.
- **Harness:** compiled in about 2 s; no compile failure.
- **Native:** one session, 242 s. No other session was attempted.
- **Total:** inside the 15-minute target.

## 7. Limits

- **The route is induced.**
  - Dropped-loot bags carry a test-only former-source reference, and the historical inputs are constructed.
  - It is not a natural Riverine Rise container, an organic old save or a real area transition.
  - No UI looting was executed: the transfers are the native collection methods.
- **The count limit can keep a never-held copy.** If a container holds more zero-time copies than its history records, the extra copy stays (case 7, by design). After the history row is retired, this retirement path removes nothing further.
- **Native-only comparisons are source-inferred**; no no-product baseline was run.
- **Reconciliation-fault protection is best effort** (history row retired when reachable; earlier removals stay; a scope-lookup fault falls through to native). **Pin drift** is different: `Prepare` declines the patch and native behavior remains. Neither was exercised.
- **The scope census was not enumerated:** which map objects reference `Forest_GoodLoot01`.
- **No native item identity exists**; cross-save instance claims rest on test-only markers.

## 8. Recommendation

Accept LIM-03 as qualified intentional behavior on the induced route for the cases above, with the section 7 limits. G5 was not started; no accepted scope was reopened.

## 9. Review decision and corrected claims (2026-11-23)

**D3 / LIM-03 is accepted** for its demonstrated induced native scope, as intentional behavior of the existing
Thornstep retirement implementation. The product remains the accepted D1 candidate (`634a6efdd3`, package
`862027f1…0fdc37`, DLL `65a0b60f…5c43ef`). The qualifying session `20261123T185954Z-owner-0.33.0-d3-lim03-thornstep`,
harness source `a3621eb0d0` and harness build `ea03eed3…6dcc` keep their identities.

Limits preserved: induced dropped-loot components with the exact former-source loot reference; no natural Riverine
Rise container, organic historical save or real transition; native-only comparisons source-inferred; cross-save item
identity by test-only markers; no fault or pin-drift execution; no census of map objects sharing the loot blueprint.

Corrections (applied in place above): party-assertion scope; transfer, first-load and fresh-load assertion classes;
1041/1042 asserted with first position captured; decision strings as checked diagnostics, not attribution; `Split` as a
second time writer; no further removal by this path after history retirement; fault protection separated from pin
drift. No product or harness change and no native rerun.
