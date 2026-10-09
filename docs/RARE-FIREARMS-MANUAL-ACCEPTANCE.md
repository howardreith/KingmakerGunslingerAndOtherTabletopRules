# Rare Firearms Manual Acceptance

This guide records rare-firearm acceptance history and human checks for
player-facing accessibility and appropriateness. For the current content
followup, use the [2026-10-05 acceptance matrix](#content-followup-acceptance---2026-10-05),
including its existing-fixture and no-save-write boundaries. Earlier acceptance
does not approve Model D or the revised firearm tooltip appearance. Automated
observers remain authoritative for the specific publication contracts they test.

- Inspect Model D stock: Oleg has the three mundane firearms (one each),
  powder and balls (50 each); capital has the three +1 firearms (one each),
  powder/balls/cartridges (200 each) and one Gunsmith's Kit. Bokken has the
  three ammunition rows (100 each), with no kit. No Rifle/Revolver/named unique
  is added by these publications. Existing materialized stock may persist;
  fixed rows can deplete and regeneration is not guaranteed.
- Inspect exact family icons/models and truthful Reliable/native properties.
- Natural-1 Reliable Pistol: native miss and shot spend, no misfire/condition.
- Check one Reliable Musket/Blunderbuss edge in direct and Scatter Shot paths.
- Use Irovetti's Ovation directly and in Scatter; note any extra visual/audio.
- Confirm The Last Word's project-owned Seeking bypasses only a legal attack's
  concealment miss chance; confirm Fey Bane presentation and no obvious family
  regression.

## Current 0.0.143 physical qualification

Static publication is blueprint evidence only. The full route, actual object,
interaction/pickup and revisit/save-reload gates are separate. The
[complete current location table](../planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md)
and [measured qualification report](WEAPON-FINDABILITY-QUALIFICATION.md) are
authoritative; earlier route claims below are not physical PASS evidence.

Old saves may retain already generated inventories. Use the explicitly selected
[controlled recovery](WEAPON-FINDABILITY-RECOVERY.md), which refuses canonical
and supported upgraded copies and records one recovery per save/weapon. It
neither grants on load nor refills whole containers.
## Practical no-full-playthrough acceptance route

Use disposable copies of saves only. Prefer a save made before the first entry
into the target area: a container already instantiated or opened in an older save
may retain its old inventory even though the blueprint publication is correct.
Never use or overwrite `KMG_AUTOMATION_BASELINE`. Do not save after development
item spawning or any external teleport unless intentionally creating a disposable
test state.

Open Unity Mod Manager and use **Rare Firearm Acceptance (DEVELOPMENT ONLY)**:

1. **Print complete rare-firearm catalog audit** reports all eight exact item and
   weapon-type GUIDs, static enchantments, equivalent bonus, price, weight, and
   current shared-inventory count.
2. **Add one copy of all eight test items**, or one item-specific button, adds only
   the selected exact blueprint. It grants no proficiency, ammunition, class
   level, or acquisition state and never runs automatically. There is deliberately
   no remove-by-blueprint cleanup; discard the disposable save instead.
3. **Print acquisition/current-area location audit** reports exact target identity,
   type, area, publication, and current-area match. For exact loaded targets the observer reports persistent scene/entity identity, hierarchy, coordinates, reveal/Perception state, interaction conditions and native inventory. Unloaded or unresolved targets are UNVERIFIED. This observation alone proves neither a walking route nor pickup/persistence.

No authoritative local Bag of Tricks `tp2loc_*` entry point has been proven for
these five exact targets. Do not guess a similarly named command. Use a disposable
pre-entry save or a separately human-validated travel/teleport route.

### Exact physical-location checks

- Capital stock: inspect blacksmith stock backed by `SmithVendorTable`
  (`7de959347266092448d8a72089ef9778`). Its Model D Gunslinger publication has
  Pistol +1, Musket +1 and Blunderbuss +1 (one each); Black Powder Charge,
  Lead Ball and Paper Cartridge (200 each); and one Gunsmith's Kit.
  No mundane firearm, retired Repair/Overhaul Kit, Rifle, Revolver or named
  unique is added by this publication. Mod-owned generic Eastern Weapons and
  Elven Branched Spear rows are also removed from this table. Native and
  unrelated stock remain. Use the current matrix below to distinguish newly
  generated stock from inventory already materialized in an existing save.
- Duelist's Rebuttal: Varnhold Stockade, Agai's chest with Crimson Counselor,
  Owlbear Omelet recipe, Tuskwater Oysters and Fallen Warrior's Boot;
  `1f0bef6b8e540d644962171dc8810459` / `Forest_Container_7_good`.
- The River King's Measure: actual western Pitax Royal Palace chest with a
  Wooden Spoon, Grinding Stone and 16 gold;
  `77ad78d755a49af45abee46d86191b16` /
  `PoorHuman_IrovettiChambers_ChestHuge_Outline (3)#1`.
- Irovetti's Ovation: separate palace conservatory chest with Arbiter's Robe and
  Charoite Wyvern; `c5adf784c614e4b4c8dc220111f64a54` /
  `RichHuman_ConservatoryLoot`.
- The Last Word: House first floor, companion phase, ordinary chest northwest
  of Nyrissa's neglected throne in the Horned Hunter/Linzi room; 1,044 gold,
  gems, scrolls and petals; `b54aad6aa2844fa4c87f46088cde018b` /
  `FirstWorld_PoorLoot01#1`.
- Watch at the World's End: House first floor, companion phase, northern corner
  of the central mirror-hub room southeast of Valerie's room, beside the throne
  corridor; 299 gold, Resist Fire potions, gems, hops, pollen and scrolls;
  `e113fb75d9461924ab64df78c019991a` / `FirstWorld_PoorLoot02#1`.

The House mechanics scene and runtime phase flag are measured rather than
inferred from guide phase numbering. Both +5 locations passed native Phase 1
in `HouseAtTheEdgeOfTime01_Mechanics / Loot_SideA`, with the native lantern
active. Ordinary entrance walking, exactly-one pickup, native treasure,
revisit and disk reload passed before leaving the House. Mirror Memories and
the Third Key objective remained None before and after pickup. The former Vordakai, instrument-puzzle, basement, hidden House
and FinalDungeon3 directions are historical and superseded.

For each location, begin from a pre-entry disposable save, use the panel audit to
confirm the current area, then verify the ordinary player-facing interaction is
accessible and yields exactly one named item. If an old, previously instantiated
container lacks the item, repeat from a pre-entry save before reporting a defect.
Report an inaccessible, duplicated, inappropriate, auto-consumed, or script-filtered
target with the exact target GUID, save timing, area, whether it was previously
opened/instantiated, and the panel audit text.

### Short combat/presentation checks

- Inspect exact family icons/models and tooltips: enhancement, Reliable, Seeking,
  Fey Bane, capacity/range/misfire/condition, price, and flavor must remain visible.
- On a Reliable Pistol, force natural 1: it must miss and expend the shot without
  misfire or condition change. Check a Reliable Musket/Blunderbuss at threshold
  and threshold +1 in direct fire and Scatter Shot.
- Test Irovetti's Ovation directly and with Scatter Shot; confirm one pellet load
  is discharged once and no sonic/Thundering packet occurs. The authorized
  fallback is +4 Reliable because the installed native property is unconditional
  sonic energy rather than a critical-only effect. Confirm ordinary critical
  behavior remains native.
- Confirm The Last Word's Seeking presentation and concealment-only bypass, and
  Watch at the World's End's Fey Bane presentation/target isolation. Confirm no
  Rifle/Revolver appears in ordinary capital or BTSL stock and no obvious visual
  or firearm Wwise audio regression is present.

## Human acceptance â€” 2026-08-08

The user completed manual testing of the installed build from feature commit
`71368cb62ee8a001997d53d77ec22ca67c83a620` and reported that all firearms work
great. The feature is accepted for integration. Some graphical issues were
observed and are intentionally deferred to a separate cleanup effort; they do
not block the accepted firearm mechanics or campaign integration.

## Content followup acceptance - 2026-10-05

**HumanTooltipReview: NOT_PERFORMED. HumanMerchantReview: NOT_PERFORMED.**
The 2026-08-08 acceptance above is historical; it does not approve the new
description appearance, Model D shopping experience or existing-save behavior.

Use the clean, technically qualified **0.0.141** candidate from source commit
`c092a060a5c01f963e5954d6e6f1882bd16b6114`, ZIP SHA256
`2971a4f167f3943e580934ad302ec5b8acb420ee68310e21a50a01c64a67c006`,
or identify and qualify a later candidate separately. The clean build, 2,040
tests, strict package validation and guarded smoke passed; provenance is in the
[root handoff](../CODEX-ALL-NIGHT-GUNSLINGER-FOLLOWUP-HANDOFF-2026-10-04.md#pro-review-followup---clean-artifact-closure-2026-10-05).
The resolved-description and vendor-table observers prove their in-memory
contracts. They do not approve wrapping, merchant row presentation, a campaign's
persisted inventory, or the sufficiency of the early ammunition budget.

### Human procedure and exact expected stock

A human uses the ordinary merchant screens. For each merchant, inspect both
a named disposable fixture where inventory is newly generated and an existing
campaign's already materialized stock in an authorized disposable fixture.
Record how first generation or prior visitation is known; if uncertain, mark
that case **INCONCLUSIVE**, not PASS. Do not invent a stock-regeneration trigger.
No raw-save copying, renaming, deletion, parsing or migration is authorized by
this followup. Use only an already authorized disposable fixture; do not touch
`KMG_AUTOMATION_BASELINE`. If the needed fixture does not exist, leave the case
pending. Do not save changes during this acceptance pass.

Launch through Steam App ID 640820 and use the guarded deployment/restoration
workflow. No automated merchant navigation or inventory rewrite is part of this
checklist. Record the loaded version/commit, module profile, Better Vendors
presence/rank and any other stock-changing mod so its rows are not incorrectly
attributed to Model D. The base expectations below concern these mod-owned
publications; they are not the merchant's entire native/foreign inventory.
Check the standalone Model D profile without Better Vendors before assessing
any separately qualified combined profile.

| Merchant / exact table | Expected newly generated Model D rows | Exclusions from this publication |
|---|---|---|
| Oleg / `C11_OlegVendorTable` / `f720440559fc00949900bfa1575196ac` | Pistol 1; Musket 1; Blunderbuss 1; Black Powder Charge 50; Lead Ball 50 (five Gunslinger rows) | No +1 firearm, Paper Cartridge, Gunsmith's Kit, named firearm or Better Vendors variant; existing regional Eastern/spear rows remain |
| Capital / `SmithVendorTable` / `7de959347266092448d8a72089ef9778` | Pistol +1 1; Musket +1 1; Blunderbuss +1 1; Black Powder Charge 200; Lead Ball 200; Paper Cartridge 200; Gunsmith's Kit 1 (seven Gunslinger rows) | No mundane firearm or mod-owned generic Eastern/spear row; no retired Repair/Overhaul Kit |
| Bokken / `C11_BokkenVendorTable` / `4778ecb5df5d48742b9be5a204ed4657` | Black Powder Charge 100; Lead Ball 100; Paper Cartridge 100 (three rows) | No Gunsmith's Kit or firearm |

For new, unspent stock, check exact quantities, absence of duplicate mod rows,
and retained ordinary native stock. For previously visited merchants, record
the actual rows, prior purchases/sales if known, and differences from Model D.
Old stock or reduced quantities may persist: the implementation normalizes
blueprint tables, **not** merchant inventory already serialized in a campaign.
Purchased equipment, inventory and stash remain untouched. Record whether this
bounded behavior is acceptable to the owner; do not label persistence alone a
blueprint-normalization defect or promise that reopening the shop refreshes it.

Fixed stock can deplete; regeneration is native and not guaranteed. This change
does not create renewable or infinite supply. Crafting remains a separate
existing acquisition path.

### Acceptance record to complete

Operator/date, loaded commit/package hash, fixture name, module profile and
Better Vendors state: **NOT_RECORDED**. For every row, record observed item
quantities/order, how generation timing is known, and **ACCEPT / CONCERN /
INCONCLUSIVE**. All current results below are pending human observation.

| Case | Inventory state | Human result / observations |
|---|---|---|
| Oleg | Newly generated, unspent | NOT_PERFORMED |
| Capital blacksmith | Newly generated, unspent | NOT_PERFORMED |
| Bokken | Newly generated, unspent | NOT_PERFORMED |
| Oleg | Previously materialized campaign stock | NOT_PERFORMED |
| Capital blacksmith | Previously materialized campaign stock | NOT_PERFORMED |
| Bokken | Previously materialized campaign stock | NOT_PERFORMED |

| Presentation / owner decision | Acceptance question | Human result |
|---|---|---|
| Pistol tooltip | Does the complete description wrap legibly and display natural spacing/punctuation? | NOT_PERFORMED |
| Blunderbuss tooltip | Is the distinction between lead-ball attacks and Scatter Shot clear in the rendered tooltip? | NOT_PERFORMED |
| The Last Word tooltip | Are both complete Reliable and Seeking clauses readable, with no visual duplication or clipping? | NOT_PERFORMED |
| Merchant ordering and quantities | Do firearms, ammunition and kit rows look natural and make their quantities clear in all three screens? | NOT_PERFORMED |
| Oleg early supply | Is 50 powder plus 50 lead balls, a budget of 50 ordinary matched shots, sufficient for the intended early discovery/use? Record the owner's judgment; do not change quantities during testing. | NOT_PERFORMED |
| Existing-save limitation | Is retention of already materialized old stock acceptable for existing campaigns, with no promised regeneration or inventory migration? | NOT_PERFORMED |

Record concerns with merchant, fixture timing, module profile and the observed
row/tooltip. Screenshots may support human appearance feedback but do not replace
mechanical assertions. Balance changes or persisted-inventory migration would
require a separate owner decision; this review does not authorize either.
