# Alphabetical summon menus: status

**Draft checkpoint, not complete (2026-11-09).** New, bounded menu-presentation mission,
separate from the completed R1-R7 repair history. No R1-R7 result transfers to these
binaries. Nothing here is merged, released or permanently installed.

## Starting source

- The reviewed R5-R7 lineage had merged only into its former parent branches, so it was
  integrated through PR #118 (merge `1efd8a011e`, on the owner's existing merge decision).
  Master's tree equals reviewed R7 `7087e6f907`. No content was added and no history rewritten.
- Branch `codex/summon-alphabetical-menus` starts from that master.
- DATA intake: installed v0.0.140, live DLL `00EF6757...66FD`, 254 files, 16 mods enabled
  (Call of the Wild 1.14.4c-2.1, RacesUnleashed among them), `Params.xml`
  `E44F4495...30BB` (3,077 bytes), 95 saves. Reference bundle manifest `7F1EDE2B...1C04`.

## Ordering rule

Within each menu: the existing displayed family-first creature name, compared with ordinal
case-insensitive comparison; for equal names one, 1d3, then 1d4+1; then stable identity.
Rows not recognized as creature choices stay after them in their relative order. The name
comes from `SummonChoicePresentation.CreatureName`, the presentation metadata the labels
already use; native and other-mod rows use the name they show.

Applied at `SummonDisplayOrderPolicy` through the canonical publisher, the alternate-source
menu composer and the Druid flattened conversion rows. Hierarchy, membership, identities and
ability references are unchanged. The registry and catalog are not reordered.

## Native attempt on the first candidate: FAILED

Candidate `2f7a1afb6ebd` (package `90a48f34d80217f84e9b8ed5c8d9909d2707a72187f729f36d282a84502e7ef9`,
DLL `dbc94e0dff412985aa42fdf1f4999f27f38ad39d3e3f093805e869e446e78e0f`). Run
`20261109T1527147624250Z-4ed5db58043d4d4c824dcaf657958417`. Install, `Params.xml`, settings
and saves restored exactly.

- The phase stopped at `sprint32-execution`: the screenshot helper refused the new phase.
  **No real menu was opened, and no click or cast happened.**
- The structural census ran first: 93 published menus (18 canonical, 75 alternate), 85 sorted,
  0 unclassified, 0 Stirge rows. The 8 failures are Summon Monster I, IV, V, VII and Summon
  Nature's Ally I, IV, V, VII, each ending in a RacesUnleashed spider row.
- Cause: RacesUnleashed appends those rows in a `LoadDictionary` postfix at `HarmonyPriority(300)`,
  after this package publishes.

## Correction, not yet observed natively

`15a823fb61` reorders the published menus once after loading finishes and admits the phase to
screen capture. Focused gate: 46 of 2,890 selected, all passing. **The corrected observer
and late sort have not run in game.**

The next candidate build was stopped by Claude Code under critical system memory, before it
produced a package. Its directory holds no package and is not a candidate.

## Validation ledger (900 s target; ceiling raised by the owner from 1,800 to 2,400 s)

| Step | Seconds |
| --- | ---: |
| Focused gates (25, 21, 18) | 64 |
| Failed compile attempts (one timed at 16; the rest estimated) | about 40 |
| First candidate build | 577 |
| Guarded native attempt | 137 |
| Stopped build | up to 413 |
| **Planning total** | **1,231** |
