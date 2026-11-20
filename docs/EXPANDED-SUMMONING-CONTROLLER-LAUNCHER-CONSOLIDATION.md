# Controller Elementals launcher only after an actual row consolidation (DEF-06 / S5 T2)

Status: **review checkpoint.**
- Every S5 T2 native acceptance observation passed.
- The native session verdict is FAIL, because of one console-learning check that runs after them. That check
  fails identically on the accepted S5 T1 product (a matched baseline), so it predates S5 T2.
- Principal review decides whether that pre-existing failure blocks acceptance.

Branch `codex/def06-controller-launcher-consolidation`, draft PR #187, stacked on PR #186 (accepted S5 T1 records head
`f59e3ed811`). Not merged, not released.

## Defect

`ElementalSourceChooserConsole.Collect` scanned the native controller spell-selector rows and collected elemental
sources, including from Summon Monster and Summon Nature's Ally rows. It then built a synthetic Elementals launcher even
when it had removed no original standalone elemental row. In that case the launcher went at the end of the list
(`first < 0`).

The result was a duplicate, dead entry. The desktop staging already refuses that case.

The historical v0.0.189 disclosure stays as released, under its original release identity
(`docs/RELEASE-NOTES-0.0.189.md`): "The controller version of the chooser is not part of this repair. It can still offer
an Elementals entry when no original Summon Elemental spell was gathered."

## Gameplay change

On a controller, the spell selector shows the Elementals entry only when at least one original standalone Summon
Elemental spell row was actually folded into it. The entry then sits where the first folded row was.

When nothing is folded, the selector is exactly the native one:
- the same rows, in the same order;
- no Elementals entry.

Its native close and return behaviour are unchanged, and nothing is spent.

Desktop behaviour, the Default and Pause support, keyboard ownership and recovery, native-row restoration, module-off
restoration, source validation, pin (execute=false) and native dispatch (execute=true) are unchanged.

## Policy and staging change (`834982a69a`)

**The shared desktop policy is unchanged.** `StageLauncherRows` keeps its end-of-row fallback. That fallback is a real
desktop semantic: the desktop destination is the level of the lowest *source* (`session.MinimumLevel`). That level can be
a Summon Monster or Nature's Ally row's level holding no consolidated tile; the existing test
`cleanup2-elemental-launcher-minimum-level` covers it.

**Controller admission wrapper** (`ElementalConsoleLauncherPolicy.Stage`). The controller selector is one native row
list. A launcher is admitted only when all of these hold:
- the source set is nonempty;
- at least one removed row exists;
- the removed rows are distinct;
- every removed row is an exact, single member of this selector's own list. So an exact first removed position always
  exists.

The staging itself is the shared `StageLauncherRows` over that one row. Because every admitted removed row is in that
row, the desktop fallback can never apply. Any other shape is refused, and the refusal never calls the launcher factory
(so no context, item VM or node is built).

**Transactional commit** (`ElementalConsoleLauncherPolicy.Commit`).
1. It revalidates immediately before the write. `CollectSpells` runs inside the selector's constructor, so no view is
   bound yet. The revalidated facts are therefore:
   - module and controller still active;
   - the exact selector (the context's `CurrentVm`);
   - its holder (owner);
   - the unit, alive and directly controllable;
   - the spellbook sequence;
   - the exact original `ItemVms` reference sequence.
2. On refusal, only the owned launcher is disposed.
3. On a write failure, the original list is restored only while this transaction still owns the partial state. A
   foreign mutation is never overwritten, and native rows are never disposed.
4. After a complete commit, each removed native row is disposed once.

**Result decision** (`ElementalConsoleResultPolicy.Route`): the controller Result decision moved, unchanged, into an
engine-free helper:
- a stale or foreign node is refused;
- module off restores the native selector;
- an invalid source, or Use while another targeting owner is active, is refused with no spend;
- otherwise pin, Use or navigate.

## Diagnostics

- `ElementalSourceChooserConsole.LastCollection` records every controller spell-selector collection decision. It is
  either `preserve-native-console-rows:<reason>;sources=N;rows=M` or the commit outcome.
- `Outcome` keeps its existing values.

## Fixtures and tests

**Engine-free false-pass controls** (`Cleanup2ElementalConsoleLauncherTests`, seven cases), using the production helpers:

| Case | What it proves |
|---|---|
| no consolidation | Sources but no removed row: no launcher, the factory is never called, exact references and order, no context, no disposal, and a refused staging can never commit |
| no sources | No launcher for any row shape when the source set is empty |
| foreign removed rows | A same-blueprint row from another selector, a mix of exact and foreign rows, a duplicate removal, and a row present twice are all refused with nothing built, mutated or disposed |
| first consolidated position | One row gives a launcher in place; a mixed menu gives one launcher at the first removed position, with Summon Monster/Nature's Ally, foreign and unrelated rows exact |
| stale before commit | A changed selector, owner, unit, spellbooks or rows: no write, launcher disposed once, native rows not disposed |
| partial commit failure | Owned partial state is rolled back exactly; a foreign mutation is not overwritten; removed native rows are not disposed |
| commit disposal and Result routes | Removed rows are disposed once each, only after the write, with the launcher current; stale, foreign-owner, exhausted and foreign-targeting cases are refused; pin, use and navigate route correctly; cancel, stale and repeated submit dispatch exactly once in total |

**Native observer** (`RuntimeTestRunner.SummonCleanup2ConsoleConsolidation.cs`, wired into the existing
`elementals-console` review in `779ee6620e` and corrected in `f91a801447`):
- A first-priority `CollectSpells` postfix snapshots the exact native rows before the product's own postfix runs, so the
  published list is compared with the list it came from.
- Lane A runs before any original standalone spell is prepared.
- Lane B adds the one-launcher placement, the stale-node and exhausted-source refusals and a no-leak check to the
  existing navigation, cancel, pin and cast flow.
- The ownership snapshot covers focus (`CurrentSlot`), `Active`, pause, selected unit, selector, input layers, targeting
  and the last dispatched source.

**Session:** `scripts/compatibility/loader-compat/sessions/def06-console.json`.
- Candidate only, save-free, UMM 0.33.0, controller mode selected natively before the world exists.
- It was renamed from `elementals-console-consolidation.json` in `32dd7ce74d` / `243c49e0cf`, because the long name broke
  MAX_PATH.

**Focused checks (final source `f91a801447`):**

| Check | Result | Time |
|---|---|---|
| `Invoke-KmgGate -Filter 'expanded-summoning.cleanup2-elemental'`: staging, console collection, lifecycle, source selection/debit, pause and keyboard | 30 of 30 | 15–17 s |
| `-Filter 'expanded-summoning.cleanup2;expanded-summoning.native-profile;loader-compat'` | 68 of 68 (3,135 registered) | 17 s |
| Scenario preflight | 1,205 passed | 17 s |
| Development-candidate validation (`def06-console-launcher`) | PASS | 5 s |
| Icon catalog | PASS | — |
| `git diff --check` | clean | — |

## Candidates and native sessions

All runs used Kingmaker 2.1.7b, UMM 0.33.0 and harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`,
with only the candidate staged. Steam Cloud was "Sync Disabled".

| Candidate | Identity | Session | Result |
|---|---|---|---|
| `779ee6620ed7` | package `66feecad08c64a76adcc1bd7471d408d8ab8566e053c74b47bc69b2ff68cfef5`, DLL `496bc871b38795bea9c6c004f2d28f7a5c848a237662c4db4b3d3e3e6e2f4932`, MVID `068ec248-534a-4cf5-a0bb-5f4e86302686` | `20261119T212359Z-owner-0.33.0-elementals-console-consolidation` | **FAIL**, fixture: evidence paths of 261–271 characters exceeded MAX_PATH before controller setup; no lane ran |
| `779ee6620ed7` (same bytes) | as above | `20261119T212635Z-owner-0.33.0-def06-console` | **FAIL**, observer: the stale node was refused correctly in game, but its ownership verdict was computed over a row already holding two correctly-false fields, which aborted the phase before pin, cast, exhaustion and leaks |
| `f91a80144707` | package `6e23d2eb5776d97d5f5bf14aaacf5cda567f309b1dfbf77cfaced36454045481`, DLL `76e0aaad6ef362358ff571bf42a302212eb0faa9aa205f23bd7325af0f931307`, MVID `23a5bfc6-76d3-4e57-85fc-436397b97684` (in game) | `20261119T213730Z-owner-0.33.0-def06-console` | **FAIL**: all 18 S5 T2 observations PASS; one pre-existing console-learning check FAIL (below) |
| Accepted S5 T1 product `8ceb08ccef29` (matched baseline) | package `3ef1bce7…7ccb`, MVID `3806eb49-…0659` (in game) | `20261119T213956Z-owner-0.33.0-def06-console` | **FAIL** on the same learning check with identical values; every other check passes |

Candidate `f91a80144707` was built 21:30:41–21:37:14 UTC (68 of 68 selected), after a memory preflight showing 32.8 of
63.2 GB free. Its session ran 21:37:29–21:39:14 UTC (90 s, 82.8 s in game).

## Native observations (session `20261119T213730Z`, candidate `f91a80144707`)

**Lane A: no consolidation.**
- **Menu:** the actual console selector for the level-20 Wizard (Summon Monster II, Summon Nature's Ally II, Magic
  Missile and nine cantrips: 12 rows). Collection outcome
  `preserve-native-console-rows:no-original-row-consolidated;sources=4;rows=12`: four elemental sources, all through
  Summon Monster II.
- **Rows:** the published rows are the exact native objects in order. There is no synthetic Elementals row, no console
  context, no disposed row and no standalone elemental row.
- **Close:** native decline closes it, and the selector is disposed by that native close. Focus slot, action-bar active
  state, pause (unpaused before and after), selected unit, selector and input layers are exact; targeting is free; there
  is no new dispatch; source budgets and pins are unchanged.

**Lane B: consolidation.**
- **Placement:** six original standalone rows were removed, the first at position 0, and one launcher sits at
  position 0. Outcome `native-console-elementals-ready;removed=6;position=0`.
- **Retained rows:** Summon Monster II, Summon Nature's Ally II, Magic Missile and the nine cantrips keep their exact
  objects and order.
- **Disposal:** the six removed rows are disposed once each; no retained row is disposed.
- **Source set:** 28 sources (24 standalone and 4 through Summon Monster II), equal to the shared policy re-run over the
  exact native rows.
- **Bindings:** all 24 rank/element bindings use the original native source, child, spellbook and preparation.
- **Navigation and cancel:** rank, then element, then source, then native decline: nothing spent, input layers exact.
- **Stale node:** a leaf of the declined selector reaches `ActionBarVM.Result(node, execute=true)`.
  - Outcome `refuse-stale-console-node-no-spend`.
  - Nothing is spent, no command is queued and nothing is dispatched.
  - Every owner is exact.
- **Pin:** native Confirm (`Result(execute=false)`) stores `MechanicActionBarSlotSpontaneusConvertedSpell` with the exact
  retained child in slot 10. No transient node is serialized and nothing is spent; the pin is then removed and
  everything restored.
- **Use:** native Use dispatches the exact original Small Fire Elemental child once:
  - one native command;
  - one rule cast and one spawn of the expected unit `46cede83…`;
  - level-2 available slots 7 → 6, only the original preparation spent.
- **Exhausted source:** after the menu is reopened, Use on the spent original source is refused with
  `refuse-console-source-no-spend:Native preparation debit changed or exhausted; open Elementals again.`
  - No command, dispatch or debit (level-2 slots stay at 6).
  - The Summon Monster II source stays offered and valid, and the selector stays owned.
  - Native decline then closes it with every owner exact.
- **No leaks:** all 17 selectors the phase opened are disposed with no item VMs left, and none of the 4 console contexts
  is current.
- **UI restoration:** party, selection, current unit, budgets, pins and input layers are exact.

**Restoration receipt (all four sessions):** saves (95), installation and registry unchanged or restored. The native
profile's empty world was restored in every session that reached it (not in the MAX_PATH run, which failed during
setup).

## Pre-existing failure outside S5 T2

`cleanup2-elemental-console-real-learning-revealed-ordinary-row`, in the console learning sub-phase that runs after the
launcher lanes, fails because the seam-level default-hidden learning list (`OriginalSummonElementalVisibility.Show =
false`) still contains one original Summon Elemental (`seamHiddenOriginalElementals: 1`). The revealed list is correct
(one ordinary row, no owned header).

The matched baseline on the accepted S5 T1 product (session `20261119T213956Z`) reproduces it with identical values.
S5 T2 changes neither `StandaloneElementalDiscovery` nor `OriginalSummonElementalVisibility`.

Its mechanism is not traced. It may be a defect in the hidden learning seam, or a check left behind by the owner's
2026-11-11 and 2026-11-12 withdrawals of the learning/spellbook disclosure. It is recorded as a known issue, not repaired
here.

## Limitations

- **Foreign rows:** in the native lanes these are native rows the chooser must leave alone: Summon Monster II (a source
  provider), Summon Nature's Ally II (not on the Wizard list), Magic Missile and the cantrips. No other mod was loaded;
  rows from another mod were exercised only in the engine-free tests.
- **Pause:** the lanes ran unpaused in native Default mode. Pause is observed as preserved, not exercised as paused. The
  paused desktop path keeps its existing lifecycle tests.
- **Stale node entry:** the stale node was delivered through the native `ActionBarVM.Result` entry, as delayed input,
  rather than through a physical controller press after the close.
- **Session verdict:** the native session verdict is FAIL because of the pre-existing learning check. No session ran with
  that sub-phase excluded.
- **Release obligation carried from S5 T1:** the next release validator must replace its obsolete creature-list
  expectation.
