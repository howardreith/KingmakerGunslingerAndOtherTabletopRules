# Mod item/vendor availability audit and redistribution options

**Mission:** Z weekend 2026-10-03, slice 7 — **research only**.
**Absolute restriction honored:** no vendor inventory, injection code,
merchant table, loot table, reward, starting equipment, crafting rule, stock
count, restock behavior, price, chapter/quest gate, DLC/BTSL distribution,
Better Vendors compatibility behavior or item balance was changed. The only
artifacts of this slice are this report and the machine-readable table
[`MOD-ITEM-AVAILABILITY-AUDIT.csv`](MOD-ITEM-AVAILABILITY-AUDIT.csv)
(62 rows).

## Scope and method

Every mod-added item with a campaign/BTSL/vendor acquisition path was traced
from its registration source and publication code (not filenames): the
Gunslinger module's firearms/ammunition/kits, the Skeletal Salesman stock
catalog, the five fixed named-firearm loot containers, the Better Vendors
progression catalog, the Eastern Weapons campaign module, the Elven Branched
Spear campaign module, and the Cord of Stubborn Resolve fixed loot. The
mod's vendor/inventory code was cross-checked against
`docs/BETTER-VENDORS-COMPATIBILITY.md` and `docs/BTSL-VENDOR-SUPPORT.md`;
both documents matched the current code (no contradictions found).
Chapter/level gates that the engine cannot encode (fixed blueprint table
rows carry no chapter predicate) are marked `not provable` in the CSV rather
than guessed.

## Evidence-backed patterns

1. **The capital blacksmith is the concentration point.** One vanilla table
   (`SmithVendorTable`) carries 10 Gunslinger rows (3 mundane + 3 +1
   firearms, 3 ammunition at 200 each, 1 kit) plus, when those modules are
   on, up to 12 Eastern generic rows and 4 spear generic rows — up to ~26
   mod rows on a single otherwise-vanilla merchant. On a sparse native list
   this is the clearest "overrepresentation" surface in the mod.
2. **Broad merchant spread beyond the smith.** Mod stock currently appears
   on Oleg's table (eastern/spear early generics), the Dire Narlmarches
   village and Pitax town smiths (all generics), Bokken the alchemist
   (ammunition + kit — a potion vendor carrying powder), four BTSL tables
   (Honest Guy: full firearms; Xelliren: ammunition + kit), and the Skeletal
   Salesman's C3/C4 generated stock (two ~33 kgp firearm uniques on a
   scroll/consumable-themed traveling merchant).
3. **Firearms are 60% of the Better Vendors weapon catalog** (30 of 50
   rows; the rest are eastern/spear melee). This is rank-gated correctly
   (Military I/III/V/VII/IX mirroring the verified BV 2.0.8 schedule) — the
   strongest-gated acquisition path the mod has.
4. **Fixed rows never restock.** All campaign vendor rows are
   `LootItemsPackFixed` appends; bought stock depletes until the native
   table regenerates. There is no flood risk, but there is also no
   resupply: a party that burns the smith's 200 cartridges in late game has
   only Bokken's 100 and BTSL as recurring powder sources.
5. **Gating quality is uneven.** Named firearm loot is area-gated sensibly
   (Varnhold Stockade → Pitax → finale); the salesman is kingdom-day-gated
   (491+); Better Vendors is rank-gated; but the fixed vendor rows carry no
   chapter predicate at all — the engine's fixed table rows cannot express
   one, so "+1 firearms from first availability" is a structural property,
   not a bug.
6. **No duplicate-path injection was found.** Every path is append-only
   with an owned-set cleanup and exact-count/idempotency validation; the
   Better Vendors ledger additionally models native fixed-row reconciliation
   with write-ahead claims. Retired items (repair/overhaul kits) are removed
   from previously generated tables on load — no dead injections found.
7. **Intentional non-acquisition items.** Advanced Rifle/Revolver are
   registered but never published (compatibility-only identities with an
   availability restriction) — documented behavior, listed in the CSV as
   `NONE` rather than an anomaly.
8. **Non-Gunslinger parties** encounter firearm rows on every merchant
   above plus 60% of the BV catalog; eastern/spear weapons are usable by
   anyone, so the eastern/spear generics (not the firearms) are the rows
   most visible to parties with no firearm user.

## Redistribution options (no model chosen or implemented)

### Model A — Specialist quartermaster (consolidation)

- **Organizing principle:** one new firearm-focused merchant (or a dedicated
  repurposed table on an existing capital NPC) owns all recurring firearm
  commerce, with tiered stock that unlocks by chapter-equivalent events.
- **Item classes:** all mundane/+1 firearms, ammunition and kits move off
  the capital smith, Bokken and BTSL onto the quartermaster; eastern/spear
  generics stay on the smith/regional tables; named loot, the salesman and
  the Better Vendors catalog are unchanged.
- **Progression logic:** tier unlocks keyed to the same kingdom-rank
  machinery the BV integration already models (mundane at I, +1 at III,
  paper cartridges at a mid tier).
- **Compatibility:** BTSL support becomes "quartermaster appears in BTSL" or
  drops firearms there; the BV contract is untouched. The smith returns to
  native-only content, eliminating pattern 1.
- **Discoverability:** best for firearm users (one obvious shop), worst for
  accidental discovery; a non-Gunslinger party may never find the shop.
- **Sparse-store risk:** eliminated on the smith; concentrated on one new
  NPC that must carry enough stock to not look empty itself.
- **Implementation surface:** largest — a new vendor blueprint/table set,
  moving three existing publications behind it, new domain/runtime tests,
  new icon/presentation decisions for the NPC's stock entries.
- **Migration/save concerns:** the existing owned-set cleanup already
  removes retired rows from generated tables on load; saves keep purchased
  items. Low risk but broad blast radius.
- **Tests required:** per-merchant exact-count/idempotency for the new
  table; chapter/rank gate policy tests; BTSL profile regression; save-load
  cleanup verification for the three retired publications.

### Model B — Restrained thematic distribution (no new content)

- **Organizing principle:** keep only merchants with a thematic link to
  firearms; delete the rest of the injections.
- **Item classes:** capital smith keeps the current 10 firearm rows as the
  single recurring firearm source; Bokken keeps ammunition only (drop the
  kit); BTSL keeps ammunition + kit on Xelliren and drops the six firearm
  rows from Honest Guy; regional smiths and Oleg keep eastern/spear
  generics but the smith's 12 eastern + 4 spear generic rows move to the
  regional tables only (Dire Narlmarches/Pitax/Oleg).
- **Progression logic:** regional spread supplies implicit pacing (Oleg =
  Act I early kinds; Dire Narlmarches/Pitax = later); no new gates.
- **Compatibility:** BTSL firearm access is removed by design (owner
  decision); Better Vendors unchanged and becomes the main enhancement
  path.
- **Discoverability:** unchanged for firearms (smith remains the obvious
  source); eastern/spear slightly harder early.
- **Sparse-store risk:** materially reduced on the smith (26 → 10 mod rows)
  and Bokken; BTSL loses its only firearms.
- **Implementation surface:** smallest — delete rows from three existing
  publication lists; every existing safety mechanism already covers
  removal.
- **Migration/save concerns:** identical to the retired-kit precedent
  (owned-set cleanup on load).
- **Tests required:** update the existing exact-count vendor contracts
  (capital/Bokken/BTSL/eastern/spear publication tests) — no new harness.

### Model C — Mixed progression (starter access + sparse merchants + crafting)

- **Organizing principle:** each acquisition channel does one job: Oleg =
  Act I starter access (mundane firearms + limited powder), capital smith =
  consumable resupply only (ammunition/kit, generous counts), enhancements =
  Better Vendors + Gunsmithing crafting, uniques = fixed loot + salesman.
- **Item classes:** mundane firearms move from the smith to Oleg (early
  kinds, matching the eastern precedent); +1 firearms leave fixed vendors
  entirely (BV rank I becomes the first +1 source); the smith keeps 3
  ammunition rows + kit at current counts.
- **Progression logic:** explicit — Act I mundane, rank-gated +N, loot-gated
  uniques; paper cartridges could gate behind a mid rank to preserve the
  early reload economy.
- **Compatibility:** BV unchanged and more important; BTSL reduced to
  Xelliren supplies; Oleg gains its first firearm rows (early-kind
  convention already established by eastern/spear).
- **Discoverability:** excellent for a starting Gunslinger (Oleg is the
  first merchant); non-Gunslinger parties see only ammunition rows on
  recurring merchants.
- **Sparse-store risk:** lowest overall — no merchant carries more than a
  handful of mod rows, and resupply remains unlimited-in-practice through
  the smith's consumable rows.
- **Implementation surface:** moderate — move lists between existing
  publications (no new blueprints), one new early-kind policy for firearms,
  chapter-gate verification for cartridges if adopted.
- **Migration/save concerns:** same owned-set cleanup story; moving +1 rows
  off the smith only removes future supply, never purchased items.
- **Tests required:** vendor contract updates plus a new Oleg early-stock
  contract; BV tier-I catalog unchanged; salesman/loot untouched.

**Preliminary technical recommendation (for owner review, not chosen or
implemented):** Model C best matches the stated problem — it directly
removes the overrepresentation on sparse vanilla stores while preserving
starter access and strengthening the already well-gated progression paths,
with moderate implementation surface and no new content requirements.

## Explicit no-change confirmation

No production vendor, loot, price, stock, gate or compatibility behavior
was modified by this slice. `git diff` for this slice contains the two
research artifacts only.
