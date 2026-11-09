# Alphabetical summon menus: status

**Alphabetization accepted on existing source, focused-test and native menu evidence. Historical
native invocation remains FAIL because of observer setup defects. Observer corrections are
source/focused-check only; no further native run performed.** (2026-11-09)

New, bounded menu-presentation mission, separate from the completed R1-R7 repair history. No R1-R7
result transfers to these binaries. Nothing here is merged, released or permanently installed.

The owner accepted the implemented ordering on the structural census, the six opened native
menus, Druid factory/popup agreement, the two observed canonical selections and the
reference-preserving implementation. That acceptance does not establish a completed reordered
summon cast, alternate-menu selection, a feature-enabled spider popup, actual scroll movement,
console navigation or saved-hotbar behavior.

## Starting source

- The reviewed R5-R7 lineage had merged only into its former parent branches, so it was
  integrated through PR #118 (merge `1efd8a011e`, on the owner's existing merge decision).
  Master's tree equals reviewed R7 `7087e6f907`. No content was added and no history rewritten.
- DATA intake: installed v0.0.140, live DLL `00EF6757...66FD`, 254 files, 16 mods enabled
  (Call of the Wild 1.14.4c-2.1, RacesUnleashed among them), `Params.xml`
  `E44F4495...30BB` (3,077 bytes), 95 saves. Reference bundle manifest `7F1EDE2B...1C04`.

## Ordering rule

Within each menu: the existing displayed family-first creature name, compared with ordinal
case-insensitive comparison; for equal names one, 1d3, then 1d4+1; then stable identity. Rows
not recognized as creature choices stay after them in their relative order. Owned rows use
`SummonChoicePresentation.CreatureName`; native and other-mod rows use the name they show.

Applied by `SummonDisplayOrderPolicy` at the canonical publisher, the alternate-source menu
composer and Druid's flattened conversion rows. Hierarchy, membership, identities, ability
references and quantity roots are unchanged; the registry and catalog are not reordered.
Stirge stays withheld. Surfaces: Summon Monster and Summon Nature's Ally I-IX, higher-tier
quantity choices, retained native choices, Druid spontaneous conversion and prepared Summon
Nature's Ally, and the published alternate families.

## Late RacesUnleashed rows

RacesUnleashed appends eight spider rows in a `LoadDictionary` postfix at `HarmonyPriority(300)`,
after this package publishes. `AlternateSummoningCoordinator.TryReconcile`, on the first Unity
Mod Manager update once publication is present, runs one final pass: `ReorderLateHostRows`. It
writes a new array to this package's own component and changes no other mod's blueprint. There
is no per-frame sorting and no new UI patch. The record of the original publication is left
as it was, so rollback still refuses after a foreign append and sort. This first-update pass
covers the demonstrated installed RacesUnleashed loading sequence, not arbitrary later mutations
by every possible mod.

RacesUnleashed gates those rows on `AbilityShowIfCasterHasFact(SpiderSummonerFeature)`, so a
caster without that feature correctly does not see them in a menu.

## Candidates

| Candidate | Result |
| --- | --- |
| `2f7a1afb6ebd` (package `90a48f34d80217f84e9b8ed5c8d9909d2707a72187f729f36d282a84502e7ef9`, DLL `dbc94e0dff412985aa42fdf1f4999f27f38ad39d3e3f093805e869e446e78e0f`) | **FAIL.** Run `20261109T1527147624250Z-4ed5db58043d4d4c824dcaf657958417`: no menu opened, no click or cast. Census found the 8 late rows unsorted (85 of 93 sorted). Retained. |
| (build stopped on `15a823fb61`) | Stopped by Claude Code under critical memory before a package. Not a candidate. |
| **`c55f40e53f36`** (package `a506a20ef5c10d2c0d416044fd5d8bf4f5ae3fc022a707e717ac157f77a15f06`, DLL `95def0a6245359b76899898440f98e1dae195a18e6fd9552bc7d9764b6c29c49`, MVID `6d2f4980-cfb8-4a17-b201-1a82c5d8bce7`) | **FAIL**: 44 assertions passed, 3 failed. Run `20261109T1612007521888Z-1762ee1cf8fb4cfd91098880b125cbfb`. |

Source of the final candidate: `c55f40e53f36a0a0ce62cfd8327b89dda3972131`, v0.0.179 development,
purpose `summon-alphabetical-menus`.

## Focused results

`Invoke-KmgGate.ps1 -Level Focused` with a named filter: 48 of 2,892 selected, all passing,
production compiled. The candidate build ran the same 48. Includes
`expanded-summoning.alphabetical-late-sort-rollback` and
`expanded-summoning.alphabetical-phase-capture-dispatch`. Not full qualification.

## Native results on `c55f40e53f36`

Save-free phase `summon-alphabetical-menus`, one invocation, 154 s including restoration.

**Structural coverage (not opened menus).** Census of published arrays: 93 checked, 93 passed, 0 failed, 0 skipped
(18 canonical, 75 alternate, 5,235 rows), no unclassified row, no Stirge row, all seven
alternate families `Published`. The late pass changed exactly the 8 previously affected menus:
Summon Monster I, IV, V, VII and Summon Nature's Ally I, IV, V, VII. That change shows the rows
were present before the sort.

**Rollback (native).** On a transient unregistered parent holding real rows, not on a live
published parent: append, sort, repeat (no change), real `ExpandedSummoningPublication.Rollback`.
It refused, all choices were kept, and the record was not refreshed. The domain regression
covers the same sequence.

**Six menus opened in game**, labels and ability identities recorded from the UI's row objects, all
sorted, following the published parent order, with no duplicates:

| Menu | Rows | First | Last |
| --- | ---: | --- | --- |
| Summon Monster IV (known) | 61 | Ant, Giant (Drone) | Wolverine (1d3) |
| Summon Nature's Ally IX (known) | 107 | Ankylosaurus (1d4+1) | Worm, Purple (1d3) |
| Druid conversion, level 5 | 73 | Ankylosaurus | Wolverine (1d4+1) |
| Druid prepared SNA V | 73 | Ankylosaurus | Wolverine (1d4+1) |
| Monster Tactician SM V single | 14 | Ankylosaurus | Xill |
| Master Summoner SM V | 75 | Ankylosaurus | Xill |

Summon Monster IV shows all ten mephits, family-first and in order. The Druid conversion factory
matched the popup in order (73 rows). All eight screenshots were inspected and are readable. An
incidental hover tooltip covers part of the grid in five of them. The Summon Nature's Ally IX
first-row and last-row captures are identical, consistent with no scrolling being needed.

**Selection routing.** Clicking two available canonical rows selected each row's own ability
object, with the matching quantity and source context: Wolf, Dire (single) in Summon Monster IV,
and Worm, Purple (1d3) in Summon Nature's Ally IX. Selection only.

**Qualification of two passing assertions.** `alpha-click-alternate-MonsterTacticianSummonVSingle`
and `alpha-click-alternate-MasterSummonerSummon5Base` passed the old predicate
`(!available || selected) && quantity && context` while their rows were unavailable. Those two
PASS results did not establish selection; they established only that the rows' configured
quantity and source context match. They remain counted in the original 44 PASS. The raw receipt
is unchanged.

### The three failures, classified but not converted

1. `alpha-late-row-consumed`: observer expectation error. The check required a visible spider
   row, but RacesUnleashed correctly hides it for a caster without its feature. The rows' sorted
   position is established structurally; a visible row in a real popup was not observed.
2. `cleanup2-native-hud-stationary-alpha-druid-cast`: observer defect. The cast moved after the
   alternate menus, which left the Spell group closed. **The routing cast did not happen.**
3. `sprint32-execution`: the resulting exception. Every earlier cell had completed.

### Not established

- A completed native cast of a reordered choice. No reordered summon cast completed; only
  selection routing was observed.
- Selection in an alternate-source menu: those rows were out of resources for the fixture
  caster (the ordering and ability identities of those menus were observed).
- A visible RacesUnleashed row in a real popup, which needs a caster with `SpiderSummonerFeature`.
- Actual scrolling. At 1920x1080 the 107-row menu fits, so its first and last rows were both
  in view without any scroll movement.
- Console navigation, every tier as an opened popup, natural characters and saved hotbars.
  Cold-save hydration was not exercised.

## Observer corrections after the final run (source and focused checks only)

Commit `fbd117d4a3aa0491b443a15b9592226cff1a0ba7`. **None of this has been executed natively.** No
package was built and Kingmaker was not launched. The verdicts above stay with `c55f40e53f36`; this
source has no native result.

- **Selection classification.** `AlphabeticalMenuObservationVerdict.ClassifySelection` separates
  three outcomes. An available row passes only when the exact row object is selected with the
  expected quantity and context; an available row that fails to select fails. An unavailable row
  is not clicked, emits no `alpha-click` assertion, and is listed as not exercised in the
  `alpha-surface-limits` evidence. A separate `alpha-row-configuration` assertion carries the
  quantity and context evidence under an accurate name.
- **Spider visibility.** The check reads the installed row's own `AbilityShowIfCasterHasFact` and
  the caster's fact. A caster without it must not see the row (`alpha-late-row-expected-absent`,
  which states it does not demonstrate a visible sorted popup); an eligible caster must see it in
  a sorted menu. The product's gating is unchanged. The feature-enabled popup remains unobserved.
- **Druid cast.** Before the existing cast, the observer re-opens the Spell group, refills it,
  selects the level, waits for the UI, and finds the current slot again by the exact prepared
  ability, confirming its `SpellSlot`. The cast, debit and restoration checks are unchanged.

Focused gate (`expanded-summoning.alphabetical;alternate-summoning.alphabetical;expanded-summoning.sprint32-native-admission`):
11 of 2,894 selected, all passing, production compiled, 20.3 s. The behavioral checks cover the
selection outcomes and the late-row visibility outcomes through the observer's own classifier. The
Druid correction has compile coverage only. The gate ran compilation and the named tests only: no
package, compatibility matrix, historical suite or native scenario.

The production ordering code is unchanged by this closeout: no file under `Summoning/` or
`Blueprints/` differs from `ffbea3343d`.

## Restoration

Both native invocations restored exactly: live v0.0.140, DLL `00EF6757...66FD`, 254 files,
`Params.xml` `E44F4495...30BB` (receipt state `restored`), game settings, 95 protected saves with
byte and metadata comparison. `KMG_AUTOMATION_BASELINE` was not touched. No campaign save was
written, no game left running, no pending receipt. Raw evidence and screenshots stay private
under `runtime-evidence`. This closeout performed no native installation transaction.

## Memory

| Reading | Available | Commit headroom |
| --- | ---: | ---: |
| Before the build | 5.8 GB | 10.5 GB of 52.9 GB |
| During the build (low) | 1.15 GB | 5.5 GB |
| Launch go/no-go, held across samples | 8.9 GB | 11.2 GB |
| During the native run (low) | 3.9 GB | 6.2 GB |

The critical-memory stop did not recur. One warning fired during the build; samples swung between
1.2 and 9 GB and recovered. The compiler server left from the earlier build had exited on its own.
The next one, from this mission's gate, was idle and was left to exit; no process was stopped. The
repository exposes no separate build-concurrency option. The largest other process is a hung
`gcloud auth print-access-token` (about 5.9 GB, since 2026-11-06), left alone as unrelated.

## Validation ledger (900 s target; owner ceiling raised from 1,800 to 2,400 s)

| Step | Seconds |
| --- | ---: |
| Earlier spend (planning figure, including the stopped build at its upper bound) | 1,231 |
| Focused gate | 22 |
| Candidate build `c55f40e53f36` | 554 |
| Guarded native invocation | 154 |
| Subtotal at the end of the native continuation | about 1,961 |
| Closeout focused gate | 20 |
| **Cumulative** | **about 1,981** |

The native continuation spent about 730 s; this closeout spent 20 s. About 419 s of the ceiling
remain unused.
