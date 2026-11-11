# Gunslinger playtest repairs (2026-11-11): status

**Released (2026-11-11).** The owner accepted the corrected review: "This is acceptable. Please merge this and
cut a new release." PR #129 was merged as `57d8640323`, and the work shipped in stable v0.0.183 (release commit
`6365ddb261`, package `018b41f12b83091e603ef59b3816c687abaa4ec0371a78d157babaf64d8c6288`). Release PR #130 also
corrected one stale test expectation (`feature-modules.runtime-matrix`, spear loot rows 6 → 5) that the focused
filter below missed. The native rows in the acceptance matrix stay NOT RUN; nothing was re-executed on the
release build. The installed mod is not permanently changed.

The record below is the pre-release checkpoint.

**Review correction applied (2026-11-11); awaiting re-review.** The review of `5e7ffa13fd` found one P2:
Thornstep's retirement could delete a previously claimed copy that had been returned to its former container.
It is corrected in `e04609c2f1` (see "Existing-save handling"). The status stays a reviewable development
checkpoint. Evidence is focused and compile only: no candidate package was built, no game was launched, and no
native acceptance row has been observed.

Branch `codex/gunslinger-playtest-repairs` from master `aa91f46571eb239df36e0233082153f2cba01899` (v0.0.182
records merge). Draft PR #129. Nothing is merged, tagged, released or installed.
- Source commits: `bfd39dda00c7da7d8c8e8a304cd47556f5d2384f` (six repairs) and
  `e04609c2f1e46d2be29cda76501ed3095efa3cb2` (review correction).
- Documentation commits follow each.

## Changes

### 1. Spellbook elemental disclosure withdrawn

**Removed:**
- **PC:** `ElementalPreparationRowsPatch` (`SpellBookView.RefreshSpellsView`) and
  `ElementalNativeRowsPresentation.Preparation`. These produced the dark "Elementals (n) — show/hide original
  sources" row and hid the original-source rows behind it.
- **Controller:** the console book projection and its synthetic "Elementals / Show sources" header VM. That
  covers the intake, the virtual-list transpiler and the confirm, hint, selection, memorize, context, metamagic
  and tooltip guards.

**Result.** The native book again lists every original elemental source as an ordinary row. Native pagination,
selection, preparation and casting are unchanged, and nothing remains that could hide a row, leave a blank or
invisible tile, or bring a fold back after refresh.

**Unchanged:**
- spell availability rules;
- Summon Elemental blueprints, learned spells, prepared slots, special/domain grants and saved action-bar
  references;
- the action-bar elemental source chooser (desktop and console);
- the level-up spell-learning disclosure, which shares `ElementalNativeSourceFold`,
  `ElementalRowVisibilityLease` and `ProjectOriginalSources`.

**Observers updated, not run.** These now require no owned disclosure and a visible original row:
- desktop `elemental-desktop-preparation-no-disclosure`;
- console `elemental-console-preparation-no-disclosure`;
- `elemental-console-native-preparation-closed-no-disclosure`;
- the PC metamagic/source book-row helper.

`scripts/Test-SummonCleanup2ConsolePatchTargets.ps1` now asserts that the 10 withdrawn patch types are absent.

### 2. Deadeye Armed status

Copy only; mechanics, costs and ranges are unchanged.
- **Status (`KMG.Deadeye.Armed.Description`):** "Your next firearm attack targets touch AC even beyond the
  weapon's first range increment. Normal range penalties still apply. This effect ends after 1 round or after
  that firearm attack, even if it misses." The unconditional "You paid 1 Grit" is gone.
- **Ability:** "As a free action, spend 1 grit to gain Deadeye Armed for 1 round. Your next firearm attack
  targets touch AC even beyond the weapon's first range increment. Normal range penalties still apply. The
  effect ends after that firearm attack, even if it misses."
- **Feature:** "As a free action, spend 1 grit. For 1 round, your next firearm attack targets touch AC even
  beyond the weapon's first range increment. Normal range penalties still apply. The effect ends after that
  firearm attack, even if it misses."

The text was checked against `DeadeyeRuntime`/`DeadeyeService`:
- increment ≤ 1 already uses touch AC;
- any exact eligible firearm attack consumes the buff, hit or miss;
- non-firearm actions leave it armed;
- range penalties are not altered.

### 3. Gunslinger's Dodge

Copy only. The ability (`KMG.Dodge.Reaction.Description`) and feature (`KMG.Dodge.Feature.AdaptedDescription`)
now read: "As a swift action, spend 1 grit to gain a +2 dodge bonus to AC for 1 round." The caveat "This
adaptation causes no movement and does not make you prone." and "You remain standing and may act normally." are
removed.

`GunslingerDodgeProneAbilityLogic.Deliver` adds only the one-round AC buff and applies no prone condition. The
`Prone` internal names are legacy identities and stay.

### 4. Focused Aim and Dead Shot

Copy only.
- **Ability:** "As a swift action, spend 1 grit. Until the end of your turn, add your Charisma modifier
  (minimum +1) to firearm damage rolls. When you use Dead Shot, add this bonus once for each of Dead Shot's
  attack rolls that hits. For example, with Charisma +3 and two Dead Shot hits, Focused Aim adds +6 damage to
  that shot."
- **Feature:** the same text without the example.
- **Buff:** "Until the end of your turn, add your Charisma modifier (minimum +1) to firearm damage rolls. When
  you use Dead Shot, add this bonus once for each of Dead Shot's attack rolls that hits. For example, with
  Charisma +3 and two Dead Shot hits, Focused Aim adds +6 damage to that shot."

The calculation is unchanged: `FocusedAimBonus = max(1, Cha) × hits`. The hit count comes from
`DeadShotRuntime.FocusedAimMultiplier`, and an ordinary shot counts as 1. The existing test
`mysterious-stranger.focused-aim` now also asserts the tooltip example `(3, 2) → 6` and `(3, 1) → 3`. The
historical `tools/validate_mysterious_stranger.py` token "not attack rolls" was replaced by the new Dead Shot
wording.

### 5. Oleg: no cold iron branched spears

The `C11_OlegVendorTable` spec now publishes only `Mundane` and `Masterwork`. Publication already drops
mod-owned rows that are not desired, so both cold iron rows leave Oleg's table. That table is the one shared by
`OTP_Oleg` and `OTP_Oleg_FirstVisit`, according to the existing guarded vendor-table observation in the placement
manifest.

**Unchanged:**
- the Dire Narlmarches village, Pitax and both Honest Guy tables, which keep all six generic spears;
- the item blueprints, and player-owned copies;
- Oleg's unrelated stock.

### 6. Thornstep retired from campaign acquisition

**Removed.** The `LootSpec(Thornstep, 3322c56f…, Forest_GoodLoot01, NorthNarlmarchesRegionLair01)` is gone, so
Thornstep leaves:
- the campaign weapon registry and the recovery selection;
- the discoverability audit.

**Added:**
- `Retired = { NamedSpearKind.Thornstep }`;
- an exact `CleanupSpec` for its former source;
- `RetiredLootSources`, the exact former source. Publication validates its identity, its cleanup row and the
  absence of the retired row, and `RetiredLootReconciliationPatch` uses it for older saves (below);
- publication validation requiring exactly one mutation per active spec, and no retired kind in loot or vendor
  specs. This replaces `_loot.Count < 6`.

**Active expectations:**
- registry 29 → 28, still with distinct-key, item and target checks plus a retired-key check;
- discoverability items 30 → 29 and exact areas 28 → 27 (Riverine Rise held only Thornstep);
- runtime module-gate spear loot rows 6 → 5;
- the read-only distribution observer now also requires zero loot and vendor rows for retired Thornstep.

**Preserved:** item `676faa5f…`, enchantment, speed-penalty buff, round marker, `NamedSpearKind.Thornstep = 1`,
effects and visuals. Boughkeeper's Old Mesa placement is unchanged, and the test pins it. The
development-only "Add one Thornstep" grant (disposable saves) stays as a legacy-copy fixture.

## Existing-save handling (engine contract inspected; correction tested offline; not natively observed)

The installed 2.1.7b `Assembly-CSharp` (MVID `07fa1e4d-...`) was decompiled with ilspycmd 9.1: first by type,
then as a full project for the review correction.

- **Oleg (`Kingmaker.UnitLogic.Parts.SharedVendorTables.GetTable`).** No mod-side migration.
  - On the first access to a table per loaded game, the persisted `KnownItems` is diffed with the current
    `LootItemsPackFixed` rows (`GetLootDifference`). `UpdateLootItems` subtracts one per removed row from the
    persisted blueprint/count `Entries`, clamped at 0, and then `KnownItems` is replaced.
  - An unsold stocked copy is removed. A purchased copy is never replenished.
  - Other tables, other items and player inventories are never touched. `PreSave` persists the result, so
    reloads don't reintroduce it.
  - **Correction of the earlier record.** It said a cold iron spear the player sold to Oleg always stays; that
    was wrong.
    - A shared table persists only blueprint/count entries, with no per-item state such as `SellTime`, so a
      sold copy and the stocked copy cannot be told apart.
    - If the stocked copy was bought and the player later sold a cold iron spear to Oleg, the one-count
      decrement removes that sold copy from Oleg's stock.
    - If the stocked copy is unsold, the stacked count drops from 2 to 1.
    - The sold item is vendor stock, not player inventory. There is no safer provenance to use, so this is
      recorded as the precise Oleg migration limitation rather than changed.
- **Thornstep (`RetiredLootReconciliationPatch`, a prefix on the exact source's `LootComponent.OnDataSetted`).**
  - **The native behavior the review identified.** `LootPersistentData.UpdateLoot` would call
    `Loot.Remove(Thornstep, 1)` by blueprint, from the end of the collection. That deletes a copy the player
    claimed and put back, and with two copies it removes the last one.
  - **Provenance used.** `ItemEntity.Time` is persisted (`[JsonProperty]`). The entity constructor never sets it,
    and `ItemsCollection.Add` stamps `Player.GameTime` only when an item enters the party inventory
    (`IsPlayerInventory`); `Split` also stamps it. Adding to a container does not.
    - A generated reward in its container therefore keeps `Time == 0` until the party holds it.
    - Every claimed-and-returned or deposited copy carries a nonzero time.
    - The zone-exit mass-loot window can take items from unviewed bodies, so the container's `IsViewed` flag is
      not used as proof.
  - **Plan (`RetiredLootReconciliationPolicy`).** Only containers whose `LootTables` reference
    `3322c56f38031eb4983b6f87c95081b7` are affected.
    - **No persisted history** (`m_KnownItems == null`): nothing changes, and the engine also skips
      reconciliation, so every copy is retained.
    - **History without Thornstep** (fresh source, or already reconciled): nothing changes.
    - **History records Thornstep:** zero-time, unequipped instances, at most the recorded count, are removed
      by exact reference. Every other Thornstep is retained. The history row is set to 0 before the native
      diff, so the engine never decrements by blueprint, and repeat loads are no-ops.
    - **Faults:** on any fault the history row is still retired, so the copy is retained.
    - **Scope:** native rows, deposits and other containers are never touched.
  - **Guards.** `Prepare` pins the MVID, `OnDataSetted` 59 IL bytes (`0x06001A28`), `UpdateLoot` 219 IL bytes
    (`0x0600A6C8`), the `Data` property type and the `m_KnownItems` field type. The adapter also does nothing if
    the current definition still publishes the item.
  - **Results:**
    - **Fresh source:** never generated with Thornstep.
    - **Generated, untouched source in an older save:** removed when Riverine Rise is next loaded.
    - **Claimed and returned:** retained.
    - **Missing history:** retained.
  - **Remaining limitation.** A Thornstep added by the engine to a container generated before the placement
    existed also has zero time and is removed. That copy has likewise never been held.

## Validation

| Step | Command | Wall time | Result |
| --- | --- | ---: | --- |
| Focused gate, run 1 (working tree) | `scripts/Invoke-KmgGate.ps1 -Filter 'elven-branched-spear;acquisition.;presentation.;model-d.;mysterious-stranger;deadeye;dodge;cleanup2-elemental;eastern-weapon;retired-vendor-stock;third-playtest;ac.deadeye;weapon-findab'` | 23 s | Compile PASS; 147 of 2,942: **2 failures** from the new test's slicing (the `Retired` comment sat inside the active-loot slice). Fixed by placing `Retired` before `Loot` |
| Focused gate, run 2 (working tree) | same | 21 s | Compile PASS (16.6 s); 147 of 2,942, 0 failures |
| Focused gate, run 3 (commit `bfd39dda00`) | same | 3 s (incremental) | Compile PASS; 147 of 2,942, 0 failures |
| Harmony target preparation (read-only, no game launch) | `scripts/Test-SummonCleanup2ConsolePatchTargets.ps1 -ModAssemblyPath artifacts/bin/Release/KingmakerGunslinger/KingmakerGunslinger.dll` | 1 s | 40/40 PASS: all remaining patch targets resolve, and the 10 withdrawn spellbook patches are absent |
| Historical validator | `python -I -B` → `validate_mysterious_stranger.validate` | <1 s | PASS |
| Engine-contract inspection | ilspycmd on `LootComponent`, `ItemsCollection`, `SharedVendorTables`, `VendorLogic`, `UnitPartVendor`, `UnitEntityData` | about 6 min of tooling | Contract read, not a test |
| **Review correction** | | | |
| Full decompile for provenance | `ilspycmd -p` of `Assembly-CSharp` (`ItemEntity`, `ItemsCollection.Add`, `MassLootHelper`, `LootCollector`) | about 5 min of tooling | Contract read, not a test |
| Focused gate, correction run 1 | filter above plus `retired-loot` | 16 s | **Compile FAIL** (missing `Kingmaker.Blueprints.DirectSerialization` using). Fixed |
| Focused gate, correction run 2 | same | 24 s | 153 of 2,948: the 6 `retired-loot.*` cases PASS; **2 failures** (`model-d.SalesmanNamedLootAndStarterUnchanged`, `acquisition.weapon-active-cleanup-disjoint`) because `RetiredSources` sat before `CleanupLoot`, where the disjointness test treats GUIDs as active. Moved after `CleanupLoot` |
| Focused gate, correction run 3 | same | 21 s | Compile PASS; 153 of 2,948, 0 failures |
| Focused gate on commit `e04609c2f1` | same | about 3 s (incremental) | 153 of 2,948, 0 failures |
| New patch target preparation (read-only) | a scratch copy of the target checker scoped to `KingmakerGunslinger.Acquisition.RetiredLootReconciliationPatch` | 1 s | 1/1 PASS: the installed Harmony12 shim ran `Prepare` (all pins) and resolved `LootComponent::OnDataSetted` |
| Spellbook target preparation on the new DLL | `scripts/Test-SummonCleanup2ConsolePatchTargets.ps1` | 1 s | 40/40 PASS |

- **Compile output** (development, not a candidate). Neither was packaged, staged or deployed:
  - at `bfd39dda00`: SHA-256 `6d15ed3ba08ed9eb51a0f9d72560bed37c9a038bb5cbd4938b801489c8bfccfc`;
  - at `e04609c2f1`: SHA-256 `f5ba9fc4a1f6f87f1efd3621d9545b321725adc711469f4c2dfbb4a4a5f696fc`.
- **New behavior tests (`retired-loot.*`).** These run the production policy, then a model of the native
  `UpdateLoot`/`UpdateLootItem`/`Remove(blueprint, count)` diff transcribed from the decompiled method:
  - untouched generated reward removed once, with repeat loads idempotent;
  - claimed-and-returned copy retained, with an unprotected control showing the reviewed deletion;
  - missing history retained at zero and nonzero time;
  - unrelated items preserved: native rows, a partially looted row and a deposited dagger;
  - mixed copies: the exact untouched instance is removed and the deposited one retained, with a control
    showing native would remove the deposited copy;
  - claimed and fresh boundaries.

  The native method itself was not executed.
- **Ledger:** about 50 s of validation for the six repairs and about 90 s for the correction, inside the
  15-minute target.
- **Not run:** a broad suite, Sprint/Tranche, a package or native work. The full acceptance matrix needs native
  fixtures that do not exist yet (see "Remaining native checks"). Building and qualifying them would exceed the
  30-minute ordinary ceiling, so it was not launched (VALIDATION-POLICY §7).

## Acceptance matrix

| Area | Required observation | Result | Evidence |
| --- | --- | --- | --- |
| Spellbook | No disclosure or blank tile; correct spells remain usable through refresh and reload | **NOT RUN** (native) | Source removal; Harmony target check 40/40 with withdrawn patches absent; observers updated but not executed. No 1920×1200 capture |
| Deadeye Armed | Actual active-status tooltip explains benefit, range penalties and expiration | **NOT RUN** (native hover) | Source text above; `deadeye.*` and `ac.deadeye-*` behavior tests PASS (mechanics unchanged) |
| Gunslinger's Dodge | Clean descriptions; representative activation stays standing with its expected AC effect | Descriptions: source updated. Activation: **NOT RUN** (native) | `dodge.*`, `third-playtest.dodge-no-prone` and `third-playtest.dodge-one-round-two-ac` PASS; delivery code applies no prone. Copy-only, with no behavior fix needed |
| Focused Aim | Consistent descriptions and a correct Dead Shot example; relevant existing behavior checks pass | **PASS** (offline) / tooltip display NOT RUN | `mysterious-stranger.focused-aim` asserts the example (3, 2) → 6, minimum +1 and the ordinary ×1; 7 `mysterious-stranger.*` PASS |
| Oleg | Fresh and previously saved stock cleaned; scope excludes other vendors and player inventory | **NOT RUN** (native) | Publication spec plus contract test (`elven-branched-spear.campaign-publication`); engine reconciliation inspected only. Limitation: a copy sold to Oleg after the stocked copy was bought is decremented |
| Thornstep | Removed from active publication and recovery; fresh and previously generated sources checked separately | Publication/recovery: **PASS** (offline contract). Existing-save plan: **PASS** (offline policy plus a native-diff model). Fresh and generated sources in game: **NOT RUN** | `elven-branched-spear.campaign-publication`, `acquisition.*` and the six `retired-loot.*` cases; patch target 1/1 |
| Compatibility | Legacy owned weapons load; Boughkeeper and unrelated loot survive; no duplicate additions on repeat load | **NOT RUN** (native) | Identities unchanged; Boughkeeper spec pinned; cleanup removes only project-owned rows |
| Runtime restoration | The pre-test installation, Mods state and protected saves are preserved or restored | **N/A (untouched)** | No staging, deployment, lease or game launch; the installed mod and saves were not accessed |

## Remaining native checks (for an explicitly selected candidate run)

1. **Spellbook, 1920×1200.**
   - Run the PC cleric book at spell level 5 and the controller book.
   - Confirm that no "Elementals" row or blank tile appears, and that original Summon Elemental rows are visible
     and preparable.
   - Change character and level, close and reopen, then save and reload.
   - Cast and confirm the correct source's slot is debited.
   - The updated cleanup2 desktop and console phases cover most of this at level 2; their Summon Elemental
     fixture lives at levels 2 and 3.
2. **Deadeye.** Activate it, then hover the real active-status icon. Check the text is readable and has no
   unresolved key.
3. **Dodge.** One activation in turn-based mode and one in real time: the character stays standing with +2 AC
   for one round.
4. **Oleg, induced old-save state, labelled induced.**
   - Set `TableData` `KnownItems`/`Entries` with both cold iron spears at count 1 on a fresh `SharedVendorTables`,
     then call `GetTable`. Expect both removed, the two other spears and unrelated stock retained, and the player
     inventory unchanged.
   - Repeat after `PreSave` with a new instance: no reintroduction.
   - Also check a fresh table: no cold iron.
5. **Thornstep, induced generated container, labelled induced.**
   - On the exact former-source `LootComponent`, invoke the real `OnDataSetted` path (prefix plus native) for
     each case, and check that a repeat changes nothing:
     - untouched (`Time` 0): removed, native rows kept;
     - returned copy (`Time` > 0): retained;
     - null history: retained;
     - mixed copies: the exact untouched copy is removed.
   - A real Riverine Rise reload would be stronger, but needs a save that has generated that container.
6. **Legacy copies.** Grant Thornstep and both cold iron spears into inventory, equipment, a companion and the
   stash of the disposable working save. Save, cold-load and reload: all present, the Thornstep enchantment
   still works, and Boughkeeper's loot row is present.

## Owner review

The reviewer confirmed the following are within the mission scope, not questions:
- leaving the level-up learning disclosure;
- the loss of the Act I cold iron purchase.

- **Visual acceptance** of the repaired book and the tooltips remains the owner's.
