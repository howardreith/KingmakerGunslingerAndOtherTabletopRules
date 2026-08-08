# Rare Firearms Manual Acceptance

This checklist will be expanded with the development-only Rare Firearm
Acceptance panel and a no-full-playthrough location-validation workflow. It
will require disposable save copies, prohibit `KMG_AUTOMATION_BASELINE`, explain
old instantiated-container limits, list exact target/area identities, and name
only authoritative locally proven Bag of Tricks entry points. The automated
observer remains authoritative for publication wiring; human checks cover only
player-facing accessibility and appropriateness.

- Inspect selected capital stock: three mundane and three +1 early firearms,
  exact supplies, no Rifle/Revolver/named unique.
- Inspect exact family icons/models and truthful Reliable/native properties.
- Natural-1 Reliable Pistol: native miss and shot spend, no misfire/condition.
- Check one Reliable Musket/Blunderbuss edge in direct and Scatter Shot paths.
- Use Irovetti's Ovation directly and in Scatter; note any extra visual/audio.
- Confirm The Last Word's project-owned Seeking bypasses only a legal attack's
  concealment miss chance; confirm Fey Bane presentation and no obvious family
  regression.

The full campaign need not be played; static loot is mechanically qualified.
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
   type, area, publication, and current-area match. The installed contracts have
   not yet proven a safe live-container highlight or teleport, so the panel
   truthfully reports live entity, coordinates, and distance as unavailable and
   never guesses, moves the party, opens loot, changes Perception, or writes a save.

No authoritative local Bag of Tricks `tp2loc_*` entry point has been proven for
these five exact targets. Do not guess a similarly named command. Use a disposable
pre-entry save or a separately human-validated travel/teleport route.

### Exact physical-location checks

- Capital stock: inspect blacksmith stock backed by `SmithVendorTable`
  (`7de959347266092448d8a72089ef9778`). It must contain mundane Pistol, Musket,
  and Blunderbuss; their three +1 variants; 200 Black Powder; 200 Lead Balls;
  10 Repair Kits; 5 Overhaul Kits; and one Gunsmith Kit. It must not contain a
  Rifle, Revolver, or named unique.
- Representative midgame: `Forest_cache` in `VordakaiTombLevel2` contains
  **Duelist's Rebuttal**.
- Pitax: `PoorHuman_IrovettiChambers_ChestHuge_Outline (3)` and
  `Forest_PoorLoot_PuzzleItem3_Instrument`, both in `IrovettiPalace`, contain
  **The River King's Measure** and **Irovetti's Ovation** respectively.
- Final act: `FirstWorld_BasementGoodLoot01` in
  `HouseAtTheEdgeOfTime_Basement` contains **The Last Word**;
  `FirstWorld_VeryGoodHiddenLoot02` in `HouseAtTheEdgeOfTime` contains
  **Watch at the World's End**.

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

## Human acceptance — 2026-08-08

The user completed manual testing of the installed build from feature commit
`71368cb62ee8a001997d53d77ec22ca67c83a620` and reported that all firearms work
great. The feature is accepted for integration. Some graphical issues were
observed and are intentionally deferred to a separate cleanup effort; they do
not block the accepted firearm mechanics or campaign integration.
