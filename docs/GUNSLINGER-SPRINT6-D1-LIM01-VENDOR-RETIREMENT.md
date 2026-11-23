# Sprint 6 checkpoint D1 (LIM-01): vendor retirement and ambiguous player-sold copies

Status: **D1 / LIM-01 accepted by manual review (2026-11-23)** for its demonstrated development scope, at head
`61f18c908c` (product `634a6efdd3`, package `862027f1…0fdc37`, DLL `65a0b60f…5c43ef`). Section 11 records the decision
and corrected claims and governs where it differs. Nothing is merged or released.

- Branch `codex/s6-d1-lim01-vendor-retirement`, stacked on the G1-R R1 acceptance-records head `e16b46eb76`
  (`codex/s6-ver02-g1r-lethal-riders`, draft PR #212). The stack is not rebased.
- **Product change:** commit `634a6efdd3`. Candidate
  `KingmakerGunslinger-0.0.193-development-d1-lim01-vendor-retirement-634a6efdd346.zip`:
  - package SHA-256 `862027f189cf19656aa288264531fd81acbe91f18910336560eb7352da0fdc37`;
  - DLL SHA-256 `65a0b60f43da49e7217414ad8258e7f02e0546916060a1d4b8bdb7ce175c43ef`.
- **Baseline product:** accepted `9e902dfa80`, package `704cb0b4…79c9`, DLL `431f13da…5676`.
- **Harness:** commit `97eb1e6496` (`tools/LoaderCompatHarness/VendorRetirementProbes.cs`, step `kmgVendorRetirement`),
  build `4f858cb88221cb63c253ab3ebdf4a0a9f5114411c0b669fc219375d3c8b5887f`. The same build was used for the baseline and
  the candidate.
- **Fixture:** `KMCS_AFTER_NORMAL_012_MODONLY.zks`; loader profile `owner-0.33.0`; product alone.

## 1. Policy (new owner decision of 2026-11-23)

This policy was selected in this review; it was not accepted earlier.

- **Retire** the intended authored vendor publication rows.
- **Remove** already materialized stock only when surviving evidence proves that the removable quantity is authored
  stock covered by the retirement.
- **Preserve** ambiguous materialized copies, including possible player-sold copies.
- **Leave untouched** party inventory, equipment, stash and unrelated vendor stock.
- **Accept** that some older authored vendor stock may remain, because its provenance can no longer be distinguished.

## 2. Scope (exact LIM-01 withdrawals)

| Table | GUID | Retired rows (git history) |
|---|---|---|
| Oleg, `C11_OlegVendorTable` | `f720440559fc00949900bfa1575196ac` | Cold iron and masterwork cold iron branched spears (withdrawn in v0.0.183). His mundane and masterwork branched spear rows and his Eastern rows stay active |
| Dire Narlmarches village, `DireNarlmarchesVillageVendorTable` | `f072a8f6889b5f345b7f4e7c74cb3e4c` | Every authored branched spear row (all foundation kinds) and Eastern weapon row (all generic kinds) (v0.0.185) |
| Pitax, `PitaxTownVendorTable` | `e5ab1fccf37c55f41a20a80c6ba6a460` | Every authored branched spear row (all foundation kinds plus Briar-Crowned Spear) and Eastern row (all generic kinds plus Empty Sleeve, Moonlit Crossing, Unfixed Form) (v0.0.185) |

The protected item set for the two regional tables is the **current protected spear and Eastern families**: every
project-owned branched spear and Eastern weapon item, 12 spear and 30 Eastern, 42 in all. (Withdrawn: an earlier
sentence here claimed that a never-published item cannot appear in a table's history; see section 11.)

**Out of scope:**

- the capital table's "Model D retired generic stock" (same native mechanism; not LIM-01);
- `Acquisition/RetiredVendorStockPolicy.cs` (maintenance kits, a different consumer);
- LIM-02, LIM-03/Thornstep and any other vendor or acquisition redesign.

## 3. Native boundary (installed 2.1.7b `Assembly-CSharp`, MVID `07fa1e4d-…`, read with ilspycmd)

`Kingmaker.UnitLogic.Parts.SharedVendorTables` (`Player.SharedVendorTables`), with persisted
`m_PersistentTables: List<TableData>`.

- **`TableData` holds:**
  - `Table`;
  - `Entries: List<LootEntry>` (blueprint and count only);
  - `Loot: List<BlueprintUnitLoot>`;
  - `KnownItems: Dictionary<BlueprintItem,int>` (the fixed-row history).
- **`GetTable(table)`** (398 IL bytes), on the **first access per loaded game** (a miss in `m_Dictionary`):
  - with no `TableData`, it generates `Entries` from the current blueprint (the fresh path);
  - otherwise it runs `GetLootDifference(KnownItems, GetFixedItems(Loot))`. That returns nothing when `KnownItems` is
    null. Each negative difference goes through `UpdateLootItems`, which subtracts from matching `Entries` with a floor
    of 0;
  - **then `KnownItems` is replaced** by the current fixed rows, and the live collection is built from the positive
    entries;
  - this subtraction is **where the destructive change happens**.
- **`PreSave`** rewrites `Entries` only for tables accessed (cached) in this process.
- **Evidence available at the boundary:** only the history count and the stacked blueprint/count. There is no item
  instance, sale time or transaction history. An untouched original with history 1 and count 1, and an original
  bought and then followed by one matching sale, have the **same** representation. More copies than history (a
  count above the recorded number) can only be partly attributed. **No surviving field proves that any materialized
  copy is authored stock**, so under the policy every materialized copy at a retired pair is ambiguous and preserved.

## 4. Implementation (commit `634a6efdd3`)

- **`Acquisition/RetiredVendorHistoryPolicy.cs`** (pure, unit-tested):
  - for each exact retired item whose history count exceeds its current published count, it plans to lower the
    history to that count (zero for a fully retired row);
  - absent history gives `NoHistory`, and nothing changes;
  - `Apply` removes a zero row from the history, or stores the published count, and verifies the write. A failed
    write throws, so no completion is recorded that could hide a failed mutation.
- **`Acquisition/RetiredVendorHistoryPatch.cs`** (Harmony prefix on `SharedVendorTables.GetTable`):
  - it acts only for the three exact tables, only on a cache miss, and only when `TableData` exists;
  - it reads the native `Loot` and computes the published rows with the native `GetFixedItems`, then applies the
    policy before the native diff;
  - the native code then makes no decrement for those items, and replaces `KnownItems` with the current rows;
  - **`Prepare` pins** the MVID, the 398-byte `GetTable`, and the field types of `m_Dictionary`, `TableData.Table`,
    `Entries`, `Loot` and `KnownItems`. On any drift the patch is not applied;
  - **on a fault**, best effort when the history and item identities are available: the retired rows are removed
    from that table's history, and the failure is logged (section 11 states the limits of this fallback);
  - **diagnostics:** one Info line per protected table per load when rows are lowered.
- **Why the history update stays correct:**
  - the lowered row only reaches what the native code writes anyway, `KnownItems = current fixed rows`;
  - the next first access, in the same session after a save or in a fresh load, therefore sees no difference, so
    there's no further subtraction or duplication;
  - the retired publication rows are already absent, so they can never be regenerated;
  - no other history row is touched.
- Campaign comments that said older unsold copies are removed by native reconciliation were corrected
  (`ElvenBranchedSpearCampaignBlueprints.cs`, `EasternWeaponCampaignBlueprints.cs`).

## 5. Offline evidence

- **New behavior tests** (`tests/KingmakerGunslinger.DomainTests/RetiredVendorHistoryTests.cs`). They model the
  decompiled `GetTable`/`GetLootDifference`/`UpdateLootItems` path, compare the unprotected baseline with the protected
  outcome, and cover:
  - a fresh table;
  - an ambiguous single copy (untouched versus sold-back, identical outcome);
  - a purchased copy not replenished, including after a reload;
  - multiple copies stable across repeats;
  - missing history;
  - native reconciliation outside the pairs (an unrelated item at the affected table, and the same item at an
    unrelated table);
  - partial publication (only the excess history lowered).
- **Focused gate:**
  - `scripts/Invoke-KmgGate.ps1 -Filter 'retired-vendor-history;retired-loot;retired-vendor-stock;elven-branched-spear;eastern-weapon'`:
    production compilation 17.3 s, 70 of 3185 tests, 0 failures, 21 s;
  - after the comment edits, `…;acquisition.`: production compilation 14.5 s, 81 of 3185 tests, 0 failures, 16 s.
- **Candidate:** `scripts/Build-Local.ps1 -DevelopmentPurpose d1-lim01-vendor-retirement -DevelopmentTestFilter
  '<same filter>' -ReferenceBundleDir …/KingmakerGunslinger-private-build-references -LoaderReferenceDir …`, run on a
  detached `634a6efdd3`, 17:27:33Z–17:35:02Z (7 m 29 s), 81/81 tests. No deployment.

## 6. Native evidence

**Scenario (labelled as it is):**

- **Constructed legacy-shaped vendor data** passed through native serialization and reconciliation. It is not a
  recovered organic save, and no UI purchase or sale was executed.
- **Seed launch (writes the data):**
  - native `TableData` for the four tables, through the native fields; none of them had persisted data in the fixture;
  - active stock from the native `GenerateItems()`, plus the case rows;
  - history from the native `GetFixedItems` plus the legacy retired rows;
  - one cold iron spear and one Paper Lantern added to party inventory, and one cold iron spear to the shared stash;
  - then a native save.
- **First-access reconciliation:** a fresh process loads that save, so the engine's own first `GetTable` access
  reconciles each table. Each table is asserted **uncached** before that access.
- **Oracle:**
  - every table's counts are compared with a native-diff oracle computed from the loaded data (protected pairs exempt
    in the protected mode);
  - the case rows have exact counts;
  - the reopened collection must have identical counts (its reference identity is captured);
  - no history row may survive that the publication no longer has;
  - party controls compared before and after the group of vendor accesses: the selected retired-item counts in
    inventory and stash, total inventory and stash counts, and the main character's equipped blueprint-name multiset.

### 6.1 Sessions

| Session | Product | Launches | Result |
|---|---|---|---|
| `20261123T172326Z-owner-0.33.0-d1-lim01-baseline` (2 m 38 s) | accepted `9e902dfa80` (`704cb0b4…`) | seed 72 s; reconcile:native 72 s | **PASS 2/2**, the matched baseline reproduction |
| `20261123T173520Z-owner-0.33.0-d1-lim01-protected` (4 m 16 s) | candidate `634a6efdd3` (`862027f1…`) | seed 78 s; reconcile:protected (plus save) 88 s; reload:protected (fresh process) 75 s | **PASS 3/3** |

### 6.2 Case table (counts at each table's first native access)

| Case | Representation (history; stocked) | Baseline, accepted product | Candidate first access | Candidate fresh reload |
|---|---|---|---|---|
| 2 and 4, Oleg cold iron spear (untouched original **or** bought plus one matching sale) | 1; 1 | **1 → 0** (loss of an ambiguous copy whose representation is also consistent with a sold-back copy) | 1 → 1 | 1 → 1 |
| 3, Oleg masterwork cold iron, bought out | 1; 0 | 0 → 0 | 0 → 0 (not replenished) | 0 → 0 |
| 5, Pitax branched spear, three stocked (multiple matching) | 1; 3 | 3 → 2 | 3 → 3 | 3 → 3 |
| 5, Pitax masterwork branched spear, two stocked (mixed) | 1; 2 | 2 → 1 | 2 → 2 | 2 → 2 |
| 2 and 4, Pitax named spear (Boughkeeper), one stocked | 1; 1 | 1 → 0 | 1 → 1 | 1 → 1 |
| 2 and 4, Pitax Eastern (Wakizashi), one stocked | 1; 1 | 1 → 0 | 1 → 1 | 1 → 1 |
| 3, Pitax Eastern (masterwork Wakizashi), bought out | 1; 0 | 0 → 0 | 0 → 0 | 0 → 0 |
| 5, Pitax Eastern named (Paper Lantern), two stocked | 1; 2 | 2 → 1 | 2 → 2 | 2 → 2 |
| 6, Dire Narlmarches branched spear, missing history | none; 2 | 2 → 2 | 2 → 2 (`NoHistory`) | 2 → 2 |
| 6, Dire Narlmarches Eastern (Wakizashi), missing history | none; 1 | 1 → 1 | 1 → 1 | 1 → 1 |
| **Control:** a native row at Pitax (`Biography_lies`) lowered by one | 5 (fixed 4); 5 | 5 → 4 | 5 → 4 (native kept) | 4 → 4 |
| **Control:** the same cold iron spear at an unrelated table (`C2_VendorTableLarge`, `03139ca7…`) | 1; 1 | 1 → 0 | 1 → 0 (native kept; not protected) | 0 → 0 |

**Asserted in the reconciliation and reload launches** (not the seed launch):

- all four tables uncached before the first access;
- zero oracle mismatches;
- the reopen returned the same counts (the same collection reference was captured, not gated);
- the history afterwards holds no key the current fixed rows lack (this gate rejects obsolete keys in non-null
  history; it does not establish full history equality or matching counts).

**Fresh reload:** each reload's oracle derives its expectations from that launch's loaded data, and reconciliation
did not change them (asserted). That the reload's loaded counts equal the previous launch's post-access counts is a
**captured comparison** of the two launches' recorded outputs (the case table), not an independent automated
cross-save check. It shows no duplication, delayed subtraction or reintroduction in this run. The history no longer holds a retired row, and the product reports
`NoRetiredRows`.

**Product diagnostics captured** in `productOutcome`:

- `Neutralized` with the lowered rows at Oleg and Pitax, plus their still-stocked counts;
- `NoHistory` at Dire Narlmarches on first access.

The control table's `productOutcome` field shows the previous target table's value. The probe reads the patch's
last-outcome static, which the patch does not write for non-target tables. It is a harness reporting artifact, not a
product result. The control's counts and the identity check (`controlProtected: false`) are the evidence.

**Fresh publication** (native `GenerateItems` and the fixed rows, which the fresh path uses):

- **Oleg:** 97 fixed rows, 0 retired. He generates his mundane and masterwork branched spears plus Katana, Nodachi and
  Wakizashi (each mundane and masterwork).
- **Dire Narlmarches:** 15 fixed rows, 0 project items.
- **Pitax:** 1 fixed row, 0 project items.

This is a generator and blueprint check. Because no in-scope table had persisted data in the fixture, every table's
data was constructed, so a separate live fresh-table access was not taken.

**Identity coverage, asserted on the candidate:**

- Oleg's retired set is exactly the two cold iron spears;
- the Dire Narlmarches and Pitax sets equal every spear (12) and Eastern (30) item;
- the control table is not protected.

**Party controls:**

- **Asserted** within each reconciliation and reload launch, before and after the group of vendor accesses:
  - the selected item counts (cold iron spear 1 and Paper Lantern 1 in inventory, cold iron spear 1 in stash);
  - the total inventory (9) and stash (1) counts;
  - the main character's equipped blueprint-name multiset.
- **Captured comparison across launches:** the same values were recorded at seed, reconciliation and reload.
- No complete item-instance or cross-save identity check was made.

## 7. What provenance is and is not available

- **Available:** the per-table fixed-row history count, the stacked blueprint/count, and the current publication.
- **Not available:** item instances, sale timestamps, buyer/seller identity and transaction history. None is
  inferred or reconstructed.
- **Consequence:** no materialized copy at a retired pair can be proven authored, so all are preserved.
- **Retained:** old authored stock that was never sold stays purchasable at those three traders in older saves. This is
  the deliberate cost of not deleting a possible player-sold copy.
- **No compensation:** losses that already happened under earlier versions, where a native first access already
  subtracted a copy, cannot be reconstructed, and nothing is fabricated to replace them.

## 8. Limits

- No organic legacy save was available; the fixture is constructed and labelled as such. No UI trade was executed.
- RTWP, product alone, the one fixture area.
- The pins tie the protection to the installed 2.1.7b assembly. On any drift it does not apply, and native behavior
  (subtraction) returns.
- Out-of-scope withdrawals keep native behavior: for example the capital "Model D" generic stock.

## 9. Validation totals and restoration

- **Gates:** 21 s and 16 s. **Candidate build:** 7 m 29 s. **Native:** 2 m 38 s (baseline) plus 4 m 16 s
  (candidate). In total about 15 minutes, within the planning target.
- **Restoration:** both sessions' transactions completed, with game settings applied and owner files verified. No
  Kingmaker process was present before either session.

## 10. Recommendation

**Accept LIM-01 under the selected policy for its demonstrated development scope.** The candidate retires the exact
publication rows and preserves every ambiguous materialized copy at those pairs. It does not replenish purchased
stock, keeps native reconciliation elsewhere, and leaves party state untouched. It stays stable across reopen, save
and fresh reload.

Retained ambiguous old stock is the disclosed consequence of the policy. The other migration stories (LIM-02,
LIM-03) were not started.

## 11. Review decision and corrected claims (2026-11-23)

**D1 / LIM-01 is accepted** for its demonstrated development scope under the policy selected in this review. The
baseline and candidate identities, the verdicts, and the distinction between model tests and native execution are
unchanged. Corrections (applied in place above):

1. **Cross-save stock equality** is a comparison of captured outputs. Each reload's oracle derives its expectations
   from that launch's loaded data and does not compare against a previous-launch manifest.
2. **Reopen.** Count stability is asserted; collection reference identity is captured. The history gate rejects
   obsolete keys in non-null history; it does not establish full history equality or matching counts.
3. **First-access and reopen checks** run in the reconciliation and reload launches, not the seed launch.
4. **Party preservation** assertions cover selected item counts, total inventory and stash counts, and the main
   character's equipped blueprint-name multiset, compared before and after the group of vendor accesses. There is no
   complete item-instance or cross-save identity check.
5. The baseline's Oleg result is the **loss of an ambiguous copy whose representation is also consistent with a
   sold-back copy**; "attributable sold-copy decrement" is withdrawn.
6. **The regional 42-item set** is the current protected spear and Eastern families. The synthetic Boughkeeper and
   Paper Lantern cases exercise the protection on members of those families. They do **not** represent the
   historically listed named Pitax publication (Briar-Crowned Spear; Empty Sleeve, Moonlit Crossing, Unfixed Form).
   The claim that never-published items cannot appear in history is withdrawn: other writers, such as the native
   `AddLoot`, can add history rows.
7. **Fault retention is best effort**, when the history and item identities are available.
   - The fallback removes retired rows from history. That is not qualified where another publisher introduces a
     positive protected publication: removing the row could make the native diff add stock.
   - The unsupported-engine limitation stands (pin drift → the patch is not applied → native subtraction).
   - The protection's scope is the product's own publication.
   - The fault-path comment in `RetiredVendorHistoryPatch.cs` was corrected, comment only. The qualifying binary is
     still built from `634a6efdd3`.

This is a records-only correction, with no product build or native run.
