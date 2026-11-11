# Gunslinger playtest repairs (2026-11-11): status

**Reviewable development checkpoint. Focused/compile evidence only. No candidate package was built, no game
was launched, and no native acceptance row has been observed.**

Branch `codex/gunslinger-playtest-repairs` from master `aa91f46571eb239df36e0233082153f2cba01899` (v0.0.182
records merge). Source commit `bfd39dda00c7da7d8c8e8a304cd47556f5d2384f`; this record and the acquisition docs
follow in a documentation commit. Draft PR #129. Nothing is merged, tagged, released or installed.

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

## Existing-save handling (engine contract, inspected; not natively observed)

The installed 2.1.7b `Assembly-CSharp` (MVID `07fa1e4d-…`) was decompiled with ilspycmd 9.1. No mod-side save
migration was added, because the engine already does the exact, idempotent work.

- **Oleg (`Kingmaker.UnitLogic.Parts.SharedVendorTables.GetTable`).**
  - On the first access to a table per loaded game, the persisted `KnownItems` is diffed with the current
    `LootItemsPackFixed` rows (`GetLootDifference`). `UpdateLootItems` subtracts the removed counts from the
    persisted `Entries`, clamped at 0, and then `KnownItems` is replaced.
  - An unsold stocked copy is removed. A purchased copy is never replenished.
  - The decrement is one per removed row, by blueprint. So a cold iron spear the player sold to Oleg stays.
    If the stocked copy is still unsold, the stacked count drops from 2 to 1.
  - Other tables, other items and player inventories are never touched. `PreSave` persists the result, so
    reloads don't reintroduce it.
- **Thornstep (`LootComponent.OnDataSetted` → `LootPersistentData.UpdateLoot`, on area load).**
  - The persisted `m_KnownItems` is compared with the container's current `LootTables` items. A dropped
    Thornstep row calls `Loot.Remove(Thornstep, 1)` on that container's own collection and sets its known count
    to 0.
  - Native and foreign loot rows are unchanged.
  - A Thornstep already taken is unaffected: it is not in the container, and the engine logs a harmless
    "Can't remove". A copy owned anywhere else is never searched.
  - **Fresh source:** never generated with Thornstep.
  - **Generated, unlooted source in an older save:** removed when Riverine Rise is next loaded.
  - **Limitations:**
    - A save whose container has no persisted known-item history (`m_KnownItems == null`) keeps its copy.
    - If a player took the original and later deposited a Thornstep back into that same container, the engine
      cannot tell the two apart and removes one.
    - An unloaded area keeps the copy in its saved state until that area is loaded.

## Validation

| Step | Command | Wall time | Result |
| --- | --- | ---: | --- |
| Focused gate, run 1 (working tree) | `scripts/Invoke-KmgGate.ps1 -Filter 'elven-branched-spear;acquisition.;presentation.;model-d.;mysterious-stranger;deadeye;dodge;cleanup2-elemental;eastern-weapon;retired-vendor-stock;third-playtest;ac.deadeye;weapon-findab'` | 23 s | Compile PASS; 147 of 2,942: **2 failures** from the new test's slicing (the `Retired` comment sat inside the active-loot slice). Fixed by placing `Retired` before `Loot` |
| Focused gate, run 2 (working tree) | same | 21 s | Compile PASS (16.6 s); 147 of 2,942, 0 failures |
| Focused gate, run 3 (commit `bfd39dda00`) | same | 3 s (incremental) | Compile PASS; 147 of 2,942, 0 failures |
| Harmony target preparation (read-only, no game launch) | `scripts/Test-SummonCleanup2ConsolePatchTargets.ps1 -ModAssemblyPath artifacts/bin/Release/KingmakerGunslinger/KingmakerGunslinger.dll` | 1 s | 40/40 PASS: all remaining patch targets resolve, and the 10 withdrawn spellbook patches are absent |
| Historical validator | `python -I -B` → `validate_mysterious_stranger.validate` | <1 s | PASS |
| Engine-contract inspection | ilspycmd on `LootComponent`, `ItemsCollection`, `SharedVendorTables`, `VendorLogic`, `UnitPartVendor`, `UnitEntityData` | about 6 min of tooling | Contract read, not a test |

- **Compile output** (development, not a candidate; same source bytes as `bfd39dda00`):
  `artifacts/bin/Release/KingmakerGunslinger/KingmakerGunslinger.dll`, SHA-256
  `6d15ed3ba08ed9eb51a0f9d72560bed37c9a038bb5cbd4938b801489c8bfccfc`. It was not packaged, staged or deployed.
- **Ledger:** about 50 s of validation inside the 15-minute target.
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
| Oleg | Fresh and previously saved stock cleaned; scope excludes other vendors and player inventory | **NOT RUN** (native) | Publication spec plus contract test (`elven-branched-spear.campaign-publication`); engine reconciliation inspected only |
| Thornstep | Removed from active publication and recovery; fresh and previously generated sources checked separately | Publication/recovery: **PASS** (offline contract). Fresh and generated sources: **NOT RUN** (native) | `elven-branched-spear.campaign-publication`, `acquisition.distributed-campaign`, `acquisition.source-target-contracts` and the registry pin compile |
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
   - Build `LootPersistentData` from the former six-placement entries, then call `UpdateLoot(current
     MapObjectLoot)`. Expect Thornstep removed and native rows kept; a repeat call changes nothing.
   - A real Riverine Rise reload would be stronger, but needs a save that has generated that container.
6. **Legacy copies.** Grant Thornstep and both cold iron spears into inventory, equipment, a companion and the
   stash of the disposable working save. Save, cold-load and reload: all present, the Thornstep enchantment
   still works, and Boughkeeper's loot row is present.

## Owner review

- **Level-up learning disclosure.** The same "Elementals (n) — show original sources" control still exists in
  level-up spell learning (wizard-type selection, PC and controller). It was kept because this mission scoped the
  removal to the spellbook. Should it also be withdrawn?
- **Cold iron availability.** Generic cold iron branched spears are no longer purchasable in Act I. Their first
  vendor is now the Dire Narlmarches village trader, Pitax or BTSL.
- **Visual acceptance** of the repaired book and the tooltips remains the owner's.
