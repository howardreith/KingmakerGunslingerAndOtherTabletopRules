# Hide original Summon Elemental by default (2026-11-12): status

**Reviewable development checkpoint after the PR #133 review corrections. Second native checkpoint (candidate
`312bb462e8`) FAIL on an observer-design defect: the owner's reproduced screen is observed hidden by default; reveal,
selection, book and metamagic cells were not reached. STOP.** See "Review corrections" at the end.
Branch `codex/hide-original-summon-elemental` from master `4b10c85768` (v0.0.183 records merge). One draft PR.
Nothing is merged, tagged, released or installed.

## Scope and owner decisions

The mission as handed over asked to remove the "Elementals (n) — show original sources" row from level-up spell
learning and show the original spells as ordinary rows. Mid-mission the owner corrected it:

> By default, Summon Elemental should be a non-existent spell while this mod is installed. No summon elemental
> spell should appear whatsoever since it has been entirely wrapped into summon monster and summon nature's ally.
> I am up for having a toggle in the UMM that will reveal it, and in that case, then it should appear as a
> regular spell, not as this awkward hidden dropdown thing.

Asked about characters that already have the spell (learned earlier, or Druid/Cleric full-list books), the owner
chose **"Hide everywhere"**: the default option also hides it from the spellbook/memorize lists; saves, learned
spells and prepared slots stay intact, and the option shows everything again as ordinary rows.

## Changes (source `b0d235c33a`, observer fix `70ad3b8e6c`)

**New UMM option.** "Show original Summon Elemental spells (applies immediately)", under Expanded Summoning.
- Default off. Applies live (no restart).
- Persisted as the optional `FeatureModules.json` key `show-original-summon-elemental`. There is no schema bump:
  earlier releases ignore the key. A missing key means hidden, and a non-boolean value recovers to defaults.
- Only active while Expanded Summoning is active. With the module off, nothing is hidden.

**Level-up learning (PC and controller).** The existing exact `GetSpells` call-site transpilers
(`StandaloneElementalPcLearningPatch` on `CharBSelectorLayer.FillSpellLevel`,
`StandaloneElementalConsoleLearningPatch` on `CharGenSpellsPhaseVM.GetSpellsCollection`) now:
- with the option off, hide every unselected, unknown original rank (or element child) row on any class list. The
  per-list equivalence proof no longer gates the decision. A pending selection stays visible so it can still be
  changed natively. The existing budget guard still keeps a mandatory learning choice reachable;
- with the option on, return the native list unchanged.

**Spellbook and memorize lists.** New postfixes:
- `OriginalSummonElementalPcBookPatch` on `SpellBookView.GetSpellsForLevel` (84 IL bytes; the page list, page
  count and refresh all read it);
- `OriginalSummonElementalConsoleBookPatch` on `UIUtilityUnit.GetKnownSpellsForLevel` (68 IL bytes; controller
  known list and memorizing panel).

With the option off, both drop original rows (including metamagic copies). A special (domain or other granted)
entry is kept, because a domain slot may have no other choice at that level.

**Withdrawn learning disclosure.**
- PC: `ElementalNativeSourceFold`, `ElementalNativeRowsPresentation`, `ElementalLearningRowsPatch`
  (`SetupSpellSelectorView`) and `ElementalLearningRefreshRowsPatch` (`FillSpellLightUpdate`). The file
  `ElementalNativeRowsPresentationPatch.cs` is deleted.
- Controller: the learning projection/context, the `LearningHeader` VM, `ElementalConsoleBookLifetime`, and the
  intake, virtual-list transpiler, header-text, confirm and tooltip patches.
- Now-unused helpers `ProjectOriginalSources`, `SourcesRemainEligible` and `ElementalRowVisibilityLease`.
- `ElementalConsoleBookPresentation` keeps only `Model`/`Exact`, which observers and the new patches use.

**Unchanged.** Blueprints, GUIDs, spell-list membership, known spells, preparations, slots, resources, casting,
the action-bar elemental chooser (desktop and console), domain/special grants, NPC/item uses and saves. The
historical cleanup2 `Prove` routine is retained only for its old observer.

## Not covered (owner review)

- The PC metamagic ("favorites") tab lists custom spells directly, not through `GetSpellsForLevel`. A metamagic
  copy of an already-known original spell can still appear there.
- Scroll copying is not blocked. A Wizard can still copy a Summon Elemental scroll; the copied spell is then
  hidden from the book while the option is off.
- Learning contexts the seam does not classify (e.g. a spellbook whose list differs from the selection's list,
  or `ExtraByStat` selections) fall back to the native list. There the original spells appear as ordinary rows,
  never with a disclosure.
- Known spells for spontaneous casters still appear on the action bar through the existing elemental chooser.

## Focused verification

| Check | Result | Seconds |
| --- | --- | ---: |
| `Invoke-KmgGate.ps1 -Filter 'cleanup2-elemental;original-summon-elemental;feature-settings'` (production compile + tests) | 24/2,947, 0 failures | 56.1 |
| `Test-SummonCleanup2ConsolePatchTargets.ps1` on the compiled DLL (installed Harmony12, MVID `07fa1e4d…`) | 49/49 | 0.7 |
| Candidate `Build-Local -DevelopmentPurpose hide-original-summon-elemental` | PASS | 451 |
| Guarded native run (wrapper wall) | **FAIL** (observer) | 121.3 |
| Focused gate after observer fix | 24/2,947, 0 failures | 47.8 |

New tests: `expanded-summoning.original-summon-elemental-book-hidden`,
`expanded-summoning.original-summon-elemental-learning-hidden` and `feature-settings.show-original-summon-elemental`.
Four disclosure-projection tests were retired together with their helpers:
`cleanup2-elemental-source-disclosure`, `-disclosure-rollback`, `-console-projection` and `-console-hidden`.

The patch targets verify both new postfixes, both learning seams (`FillSpellLevel` 511 bytes, `GetSpellsCollection`
298 bytes), and the absence of all 22 withdrawn disclosure types. These are target checks, not rendered results.

## Candidate

| Item | Value |
| --- | --- |
| Source | `b0d235c33a26f740db3a50508404b11c18c01759`, v0.0.183 development, purpose `hide-original-summon-elemental` |
| Package SHA-256 | `4d84746fece0f46fab982d0226fd8f94a6bc9984e0237fe162a959424cdd9861` |
| DLL SHA-256 / MVID | `cdf252fbf4d93697f99f897f7192042ffb364c48e8cd37ca179736acc4b28e2f` / `616ccf20-197c-4c99-8d66-5ce60d64089b` (loaded build verified) |
| Source-state fingerprint | `b24bb68d12351b000f38345cdcb6192d2f22e69dc7be04c9500ebeab500c8b34` |
| Build | 451 s; 24/2,947 tests; strict UMM package validation passed; explicit `-ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references` |

The observer fix `70ad3b8e6c` changes compiled observer code. That candidate's bytes are therefore not the
current source, and nothing was rebuilt.

## Native checkpoint (candidate `b0d235c33a`)

One guarded invocation on DATA through `Invoke-ExpandedSummoningRuntimeScenario.ps1` (Params.xml-protected),
launched from a Windows PowerShell 5.1 child on the combined installation:
- scenario `disposable-expanded-summoning-cleanup2-native-review`, `reviewPhase=elementals-learning`, save-free;
- run `20261112T0536092622277Z-bc998c344a924856999df9b6e90d3c8a`, evidence
  `runtime-evidence/20261112T0536092477142Z-disposable-expanded-summoning-cleanup2-native-review` (private);
- game duration 65.7 s.

A peer session (Kingmaker Mounted Combat) granted a no-Kingmaker window for the build and the run. The window was
released after restoration.

**Result: FAIL, 7 PASS / 3 FAIL.** Mod initialization succeeded and loaded build 0.0.183.
- PASS: human fixture, CL20 spellbook fixture, native Default admission/enter/restore, level-up intake, native
  book selection, book close, and learning-phase restoration (option restored to its in-memory value).
- FAIL `elemental-desktop-preparation-hidden-by-default`, plus the two consequent execution cells.
  - Every substantive predicate held:
    - the original source stayed known, with known objects exact;
    - the native level-2 list held 0 original elemental rows, and 0 were rendered on its 1 page;
    - there was no disclosure;
    - the filter reported `pc-book:hidden=1`.
  - The cell also required `nativeListedRows > 0`, assuming other level-2 spells. The disposable fixture's only
    level-2 known spell is the granted source, so the correctly hidden list was empty (0).
  - **Classification: observer defect.** Reader-only re-evaluation of the same recorded observation without that
    predicate passes this one cell. The original FAIL is retained.
- **Not observed.** The exception ended the phase before:
  - the revealed book row, preparation and undo;
  - the whole level-up learning screen: hidden default, revealed ordinary row, select, collection round trip,
    native deselection and cancel;
  - every screenshot (no screen was captured).

  The learning-screen behavior is therefore **not natively observed**. Controller learning was not run (it needs
  the separate controller phase). Its evidence is the shared seam, compilation and patch targets only.
- Correction `70ad3b8e6c`: the cell now requires the filter's own non-zero hidden count instead. Focused gate
  passed. Not built, not run (no automatic second build/run).

## Restoration

Wrapper record `runtime-evidence/expanded-summoning-restoration/20261112T0537367040000Z-disposable-expanded-summoning-cleanup2-native-review.json`.
- Live tree 254 files `5C3E4AD4…BFBB` before and after.
- `Params.xml` restored to `E44F4495…30BB` (receipt
  `runtime-backups/umm-params/20261112T0535360674410Z-6629ab8c91f247478908227fc9abcbf6/`).
- Independent Windows PowerShell 5.1 comparison against the intake: everything identical:
  - live v0.0.140, DLL `00EF6757…66FD`;
  - `FeatureModules.json` `6E24B278…85D0`;
  - `Params.xml` byte-identical with the scratch copy;
  - the same 16 enabled mods;
  - 95 saves, none modified since the mission started;
  - `KMG_AUTOMATION_BASELINE` and `KMG_AUTOMATION_WORKING` hashes unchanged.
- No Kingmaker process left and no foreign lock.

## Validation ledger, first checkpoint (900 s target, 1,800 s ceiling)

| Step | Seconds |
| --- | ---: |
| Focused gate 1 | 56.1 |
| Patch-target check | 0.7 |
| Candidate build | 451 |
| Guarded native invocation (wrapper wall, incl. restoration) | 121.3 |
| Focused gate 2 (observer fix) | 47.8 |
| Intake/restoration verification | ~12 |
| **Total** | **~689** |

## Owner retest checklist

1. In UMM, confirm "Show original Summon Elemental spells" is unchecked.
2. Wizard (or Bard) level-up, Spells: no Summon Elemental rows and no "Elementals (n)" row at any level.
3. Open the spellbook of a character that already knows Summon Elemental (or a Druid/Cleric): no Summon Elemental
   rows. Prepared copies still cast from the action bar.
4. Check the option (no restart). Reopen level-up and the spellbook: each Summon Elemental appears once as an
   ordinary row. Select it, switch slot and return; it stays selected. Pick another spell; it deselects.
5. Uncheck the option and confirm the rows disappear again. Summon Monster / Summon Nature's Ally elementals and
   the action-bar Elementals chooser are unchanged.
6. Optional: a Cleric with a Fire, Travel or Weather domain keeps that domain's elemental spell.

---

# Review corrections (PR #133 completion, 2026-11-12)

Authorized: narrow corrections, focused checks, one further candidate and one guarded invocation. Reviewed head
`7f97719f24`. The ledger continued from about 689 s.

## Finding dispositions

1. **PC metamagic/favorites tab bypassed the filter.** Fixed (`312bb462e8`). `SpellBookView.SetUpFavorite`
   (installed, 203 IL bytes) read `Spellbook.GetCustomSpells(i)` directly. `OriginalSummonElementalPcFavoritesPatch`
   copies only that one call site, as the learning seams already do, and routes it through the same decision:
   - option off: original custom/metamagic copies are hidden;
   - option on, or Expanded Summoning inactive: the native enumeration is returned unchanged.

   Unrelated custom spells keep their order. Custom spell data, preparations, hotbar references, costs and casting
   are never written, and the global custom enumeration is unchanged. Patch target added (50/50).
2. **Special/domain keep.** It now matches a special blueprint at any level, so a metamagic copy of a domain
   elemental stored at a higher level stays (book and favorites).
3. **Shared decision.** `OriginalRowsHidden(show, moduleActive)`, with tests for the hidden, revealed and
   module-inactive cases.
4. **UMM wording.** Changed to "no restart; applies when the list refreshes or reopens". The native run below
   shows this is still imprecise for an open level-up list: it updates only when level-up is reopened (see
   "Native finding"). Owner-facing wording correction proposed: "no restart; applies the next time a spell list
   is opened".
5. **Observer strengthened.**
   - Learning runs first.
   - Cells added: pending selection reachable after hiding returns, and re-hidden after native deselection.
   - The book cell uses a disposable mixed fixture: a known source, an ordinary level-2 control, and Extend copies of
     both. Its checks use exact object identities, independent of `BookOutcome`.
   - The metamagic tab is opened through its real toggle, hidden then revealed, with explicit fixture restoration.
   - The empty-list correction from `70ad3b8e6c` is kept. No blanket non-empty assertion is restored.
6. **Unchanged by instruction.** Scroll copying stays unrestricted. Unclassified learning contexts keep their
   native fallback, and the budget guard stays.

## Focused verification (source `312bb462e8`)

- `Invoke-KmgGate.ps1 -Filter 'cleanup2-elemental;original-summon-elemental;feature-settings'`: 26/2,949, 0
  failures, production compiled, 50.6 s.
  - New tests: `original-summon-elemental-decision` and `original-summon-elemental-mixed-metamagic`.
  - Existing tests: pending/budget (`cleanup2-elemental-learning-budget`, `-learning-copy`, `-learning-refusal`,
    `original-summon-elemental-learning-hidden`) and settings (`feature-settings.*`).
- `Test-SummonCleanup2ConsolePatchTargets.ps1`: 50/50 (0.7 s). This includes the `SetUpFavorite` 203-byte target,
  both book postfixes, both learning seams, and the absence of the 22 withdrawn types. These are target checks, not
  rendered results.

## Candidate `312bb462e8`

| Item | Value |
| --- | --- |
| Source | `312bb462e8ad` (full SHA in the development receipt), v0.0.183 development, purpose `hide-original-summon-elemental` |
| Package SHA-256 | `1f3eb684d37154342e7a98005cb1b842082cad214bdc49d8ed0aded7bebffb8a` |
| DLL SHA-256 / MVID | `40d94cb5cd0b669820c0fc7ce3463fb7fd1b6b3965af1e683106d79e588cd239` / `268ffcd7-0894-4f35-88fa-caf9ce63fab4` (loaded build verified) |
| Build | 438 s; 26/2,949 tests; strict UMM package validation passed; explicit `-ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references` |

The machine crashed and rebooted (08:10 local) after the build and before the run. Nothing was running. A fresh
intake afterwards was identical (see Restoration).

## Native checkpoint (candidate `312bb462e8`)

One guarded invocation on DATA. Same route as before: `Invoke-ExpandedSummoningRuntimeScenario.ps1`
(Params.xml-protected), Windows PowerShell 5.1 child, combined installation, scenario
`disposable-expanded-summoning-cleanup2-native-review`, `reviewPhase=elementals-learning`, save-free.
- Run `20261112T1319565163249Z-ecda47495a47472e975650f162305fd6`; evidence
  `runtime-evidence/20261112T1319564956566Z-disposable-expanded-summoning-cleanup2-native-review` (private).
- Game 64.8 s; wrapper 121.1 s. No peer session was running.

**Result: FAIL, 8 PASS / 2 FAIL.**

**PASS `elemental-desktop-learning-hidden-by-default`: the owner's reproduced surface.** Real desktop Wizard
2→3 level-up, Spells phase, the native "Choose Spells" collection:
- normal Wizard selection: `ExtraMaxLevel` 2, `ExtraByStat` false;
- 82 active native rows, 0 original Summon Elemental rows, the Summon Monster II row present;
- no disclosure object, pending selections unchanged, nothing committed;
- the PC learning seam was installed, and its recorded disposition was `hidden-original-summon-elemental` for
  Small Summon Elemental `970c6db4…` on the Wizard list at level 2.

Screenshot `native-ui-000.png`, inspected: the native Wizard "Choose Spells" panel with an alphabetical level-2
list (Acid Arrow, Aggressive Thundercloud, …) and no "Elementals (n)" row. Only the top of the list is visible; the
structured row count covers the whole list.

**FAIL `elemental-desktop-learning-revealed-ordinary-row`** (plus its execution cell): after the option was
turned on in memory and a native spell-slot round trip, 0 source rows were present.

**Native finding (observer-design defect, not a product defect).** Installed
`CharBFeatureSelector.FillDataAllSpells` calls only `CharBSelectorLayer.FillSpellLightUpdate` when the new slot
uses the same spell list as the current layer. Two slots of one Wizard selection therefore never rebuild the list,
so the transpiled `FillSpellLevel` seam never re-ran after the option changed. The native list regenerates only when
level-up is reopened (or a different list is shown). Consequences:
- A UMM toggle made while a level-up screen is open applies the next time level-up is opened. Hence the proposed
  wording above.
- A pending choice cannot be hidden by a mid-session toggle; the seam also keeps selected rows (pure tests).
- The observer's reveal, "hiding returns while pending" and "re-hidden after deselect" cells need two level-up
  sessions (hidden, then revealed) rather than a slot round trip.

**Not observed on this candidate.**
- The revealed learning row, native selection, round trip, hidden-pending, native deselection and re-hide cells.
- The mixed book cell, the metamagic tab hidden and revealed, the revealed book preparation and undo, and
  mixed-fixture restoration. These are in the later preparation part, which did not run.
- Controller: nothing rendered. Coverage is the shared learning seam, the controller book postfix target, compile
  and pure tests.

Candidate `b0d235c33a` keeps its own result (7 PASS / 3 FAIL), and correction `70ad3b8e6c` remains
focused/compile-only. Neither is relabelled by this run.

## Restoration (second run)

Wrapper record `runtime-evidence/expanded-summoning-restoration/20261112T1321227955159Z-disposable-expanded-summoning-cleanup2-native-review.json`.
- Live tree 254 files `5C3E4AD4…BFBB` before and after.
- `Params.xml` restored to `E44F4495…30BB` (receipt
  `runtime-backups/umm-params/20261112T1319226459532Z-d410e1a2368b4e3c8cf161a47ca01b54/`).
- An independent Windows PowerShell 5.1 comparison against the post-crash intake was identical:
  - live v0.0.140, DLL `00EF6757…66FD`;
  - `FeatureModules.json` `6E24B278…85D0`;
  - `Params.xml` byte-identical;
  - the same 16 enabled mods;
  - 95 saves, none modified since 13:00 UTC;
  - baseline and working save hashes unchanged.
- No Kingmaker process and no lock left. Fixture-owned state: the disposable level-up actor was cancelled and
  destroyed (`learning-restored` PASS, option restored); the mixed book fixture was never created.

## Ledger (continued; ceiling 1,800 s)

| Step | Seconds |
| --- | ---: |
| Carried from the first checkpoint | ~689 |
| Focused gate | 50.6 |
| Patch-target check | 0.7 |
| Candidate build `312bb462e8` | 438 |
| Guarded native invocation (wrapper wall, incl. restoration) | 121.1 |
| Intake and restoration verification | ~15 |
| **Cumulative** | **~1,314** |

About 486 s remain under the ceiling. That is not enough for another build plus run (about 560 s), so a further
attempt needs a new owner allowance.

## Proposed next correction (not implemented, not authorized)

- Observer: split learning into two native level-up sessions on the same disposable actor.
  1. Option off: open level-up, check hidden, cancel.
  2. Option on: reopen, check the ordinary row, select, round trip, deselect by native replacement, cancel.

  Drop the in-session "hiding returns while pending" and "re-hidden" cells, or replace them with a third reopen
  after a cancel. That reopen cannot keep a pending choice, so the pure tests remain the evidence for pending
  safety. Keep the book/metamagic cell as written: its level/tab changes do rebuild through `GetSpellsForLevel` and
  `SetUpFavorite`.
- Product wording: "no restart; applies the next time a spell list is opened".

## Owner retest checklist (updated)

1. Option unchecked: at a Wizard level-up, no Summon Elemental and no "Elementals (n)" row (observed natively on
   `312bb462e8`).
2. Spellbook of a character who knows Summon Elemental, including the metamagic (star) tab: no original Summon
   Elemental rows or metamagic copies. Other metamagic favorites remain.
3. Check the option, then **close and reopen** level-up and the spellbook. Summon Elemental appears once as an
   ordinary row, and its metamagic copy reappears. Select it, switch slot and back (it stays selected), then pick
   another spell (it deselects).
4. Uncheck the option, reopen, and the rows are gone. Summon Monster / Summon Nature's Ally elementals, prepared
   copies and the action-bar Elementals chooser are unchanged.
5. Optional: a Cleric with a Fire, Travel or Weather domain keeps its domain elemental, including a metamagic copy.
