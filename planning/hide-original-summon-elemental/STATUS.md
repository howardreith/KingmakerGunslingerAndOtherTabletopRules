# Hide original Summon Elemental by default (2026-11-12): status

**Released (2026-11-12).** The owner wrote "Approved. Please merge and cut a release."
- PR #133 was merged as `9469105336`.
- Release PR #141 (version-only, owner-merged) became `6d033bc4d9`.
- Stable v0.0.184 is Latest: tag `1b2eaa3bee` → `6d033bc4d9`; package
  `1496ead5dbfd8227943fd0fc068aca881f830b52d498ab8357ecd5d764332c76`; DLL `9251118e…5fee`; MVID
  `7b0d2325-988c-4dd2-8aef-c053b1899904`.
- The native evidence below belongs to development candidate `60d2d1b618`; nothing was re-executed on the release
  build.
- The installed mod is unchanged (0.0.140).

The record below is the pre-release checkpoint.

**Current state (2026-11-12, candidate `60d2d1b618`): ready for final review.** One guarded native run
**PASS, 22/22**. It covers the still-unobserved surfaces:
- the mixed spellbook and the PC metamagic tab, hidden and revealed, with backing lists and rendered rows;
- preparation and undo, and mixed-fixture restoration;
- revealed learning with live-state native replacement and normal cancellation.

Hidden learning and reveal-after-reopen were observed on `0d944a262f` and were not replayed. See
"Targeted verification" at the end. Nothing is merged, released or installed.

**Current behavior.**
- Hidden by default in learning, spellbook/memorize lists and the PC metamagic tab.
- UMM option: "no restart; close and reopen the spell list to apply".
- Sections from "Targeted verification" on are the current record; earlier sections are history.

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

**New UMM option.** "Show original Summon Elemental spells", under Expanded Summoning. (Historical label "applies
immediately", superseded: current label "no restart; close and reopen the spell list to apply".)
- Default off. No restart; close and reopen the spell list to apply.
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

---

# Verification continuation (PR #133, 2026-11-12, candidate `0d944a262f`)

The owner authorized narrow observer and wording corrections, focused checks, one build and one guarded run, and
raised the cumulative ceiling to 2,400 s (target 900 s). The reviewed head was `f500a1cda6`. Feature implementation
frozen.

## Changes (source `0d944a262f`)

- **Wording.** The UMM label and comments now say "no restart; close and reopen the spell list to apply". This was
  verified in the installed code: opening level-up and `SetClass` both call `ClearAllSelectors`, which clears the
  layer's cached `CurrentSpellSelectionData`, so a reopened list rebuilds through the filter. A same-list slot switch
  only runs `FillSpellLightUpdate`.
- **Learning observer.** One disposable Wizard2 actor and two separate native level-up sessions:
  - **Session A:** option off before opening; hidden check (at least 10 ordinary rows, Summon Monster II once,
    0 originals, no disclosure, allocation unchanged); owned native cancel, with preview disposal checked.
  - **Session B:** option on before reopening; all references reacquired; ordinary row; native selection; the slot
    switch is described as a native selection refresh; native replacement; cancel.

  Each session owns its controller cleanup, and the actor is destroyed once. The old in-session "hidden pending
  reachable" and "re-hidden after deselect" cells are removed: they assumed a rebuild that never happens.
  Pending-choice filtering safety rests on the focused tests (`cleanup2-elemental-learning-budget`,
  `original-summon-elemental-learning-hidden`).
- **Book/metamagic verdicts strengthened.**
  - The level-3 metamagic-copy list is checked when hidden.
  - A revealed book cell page-walks the rendered identities: source and control once each.
  - Metamagic tab: the hidden verdict requires the rendered control copy; the revealed verdict requires both
    rendered copies.
  - The ordinary preparation/undo and the explicit mixed-fixture cleanup are kept.

## Focused verification

- Focused gate (filter `cleanup2-elemental;original-summon-elemental;feature-settings`): 26/2,949, 0 failures,
  production compiled, 66.2 s.
- Patch targets: 50/50, 1.3 s.

## Candidate `0d944a262f`

| Item | Value |
| --- | --- |
| Source | `0d944a262f4fc78541ef396753e0513f1cb5f30c`, v0.0.183 development, purpose `hide-original-summon-elemental` |
| Source-state fingerprint | `dc153be10ae1c057a71b45801499d132df9e917df2988648ec10652a5e9c14b3` |
| Package SHA-256 | `41043db7dee2289ea8f96623e02d18b9c5f01a719b19614a0492daf99139f0c4` |
| DLL SHA-256 / MVID | `84c2917a5ffbf932c8f2dfb62b3b19f2c420635fd695fce249562a2e1b5f67b9` / `f34ca190-0bbd-4707-a880-74a700947a9e` (loaded module) |
| Build | 733 s (a peer session's offline MSBuild ran concurrently); 26/2,949 tests; strict UMM package validation passed; explicit `-ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references` |

## Native checkpoint

One guarded invocation on DATA. Same route as before: `Invoke-ExpandedSummoningRuntimeScenario.ps1`
(Params.xml-protected), Windows PowerShell 5.1 child, combined installation,
`disposable-expanded-summoning-cleanup2-native-review`, `reviewPhase=elementals-learning`, save-free.
- Run evidence `runtime-evidence/20261112T1413144022391Z-disposable-expanded-summoning-cleanup2-native-review`
  (private). Wrapper 117.8 s.
- The peer session granted a no-Kingmaker window; it was released after restoration.

**Result: FAIL, 12 PASS / 1 FAIL.** The failure is the execution cell.

| Cell | Result | Observed |
| --- | --- | --- |
| Session A `learning-hidden-by-default` | PASS | Real Wizard 2→3 "Choose Spells"; 82 active rows, 0 original elementals, Summon Monster II once, no disclosure, pending `[–, –]` (2 free), seam installed |
| Session A `native-learning-cancel-hidden-session` | PASS | Owned Esc and confirm Yes; 0 commits; Wizard2; known spells exact; preview disposed; original controller field restored |
| Session B `learning-revealed-ordinary-row` | PASS | Option on before reopening; 83 active rows; `970c6db4…` (Summon Small Elemental) exactly once, interactable, unselected; no disclosure or duplicate |
| Session B `native-learning-allocation` | PASS | Native row toggle → `SetSpell`; pending `[–, –]` → `[970c6db4…, –]`; free slots 2 → 1; preview knows it; actor does not; 0 commits |
| Session B `learning-selection-refresh-retains-choice` | PASS | Second slot then the original slot (native selection refresh, not a rebuild); the row is still once and selected; pending exact |
| Session B native deselection by replacement | **FAIL** (`ui-execution-learning`) | After toggling another ordinary unselected level-2 row, the wait for "replacement pending and source not pending" did not settle within its bound. The pending state at timeout was not recorded. |
| `learning-restored` and native Default restore | PASS | Session B's controller force-closed by its own cleanup; creator closed; party/selection exact; in-memory option restored |

**Screenshots inspected (private).**
- `native-ui-000`: session A, the native list with no "Elementals" row.
- `native-ui-001`: session B. The list shows "Summon Small Elemental" as an ordinary row between Summon Monster II and
  Telekinetic Strikes, in the same Stone Call…Vine Strike region as the owner's bug screenshot.
- `native-ui-002`: after selection, the first slot holds Summon Small Elemental, the second slot is empty, and the
  preview book lists it at level 2.

**Classification: observer/native-interaction unknown, not a product defect** (product presentation was correct
in every observed cell). The replacement click either did not route through `SetSpell` for the original slot,
or routed into the other slot. One candidate explanation is that the native current slot differs from the toggled
one. It was not established because the timeout recorded no state. **Not observed:** native deselection by
replacement, session B's cancel cell, the mixed book (hidden/revealed), the metamagic tab (hidden/revealed),
book preparation/undo and the mixed-fixture cleanup cell. The mixed fixture was never created.

**Controller.** No rendered observation. Coverage is the shared learning seam, the controller book postfix target,
compile and pure tests.

**Exceptions unchanged.** Domain/special grants are kept (any level). Scroll copying is unrestricted. Unclassified
learning contexts fall back to native rows. The budget guard is kept.

Earlier candidates keep their own results: `b0d235c33a` FAIL 7/3 and `312bb462e8` FAIL 8/2 (its hidden-default
learning PASS). Correction `70ad3b8e6c` stays focused/compile-only. No result transfers to `0d944a262f`.

## Restoration

Wrapper record `runtime-evidence/expanded-summoning-restoration/20261112T1414383464502Z-disposable-expanded-summoning-cleanup2-native-review.json`.
- Live tree 254 files `5C3E4AD4…BFBB` before and after.
- `Params.xml` restored to `E44F4495…30BB` (receipt
  `runtime-backups/umm-params/20261112T1412413018455Z-78540c992d9f4e96a0b18efc75a6b720/`).
- An independent Windows PowerShell 5.1 comparison against the intake was identical:
  - live v0.0.140, DLL `00EF6757…66FD`;
  - `FeatureModules.json` `6E24B278…85D0`;
  - `Params.xml` byte-identical;
  - the same 16 enabled mods;
  - 95 saves, none modified since 13:30 UTC;
  - baseline and working save hashes unchanged.
- No Kingmaker process and no lock left. The disposable actor was destroyed, the in-memory option restored and the
  creator closed.

## Ledger (cumulative; ceiling 2,400 s by owner extension, target 900 s)

| Step | Seconds |
| --- | ---: |
| Carried | ~1,314 |
| Focused gate | 66.2 |
| Patch targets | 1.3 |
| Candidate build `0d944a262f` | 733 |
| Guarded invocation (wrapper wall, incl. restoration) | 117.8 |
| Intake and restoration verification | ~15 |
| **Cumulative after the run** | **~2,247** |
| Focused gate (deselection observer fix) | 51.6 |
| **Cumulative** | **~2,299** |

About 101 s remain, which is not enough for another build or run.

## Deselection diagnosis and observer fix (`HEAD`, focused/compile-only)

Offline reading of installed 2.1.7b (no validation time):
- `LevelUpController.UnselectSpell` calls `RemoveAction`, which recalculates the preview. `UpdatePreview` →
  `ApplyLevelup` creates a **new `LevelUpState`**, so every `SpellSelectionData` object is replaced.
- The earlier selection went through incremental `AddAction` (`Check` + `Apply` on the same state). That is why the
  captured object showed the first allocation correctly.
- The deselection wait polled the captured, now stale, selection object, which still read `[source, –]`. It could
  never settle even if the native replacement had succeeded.
- The console observer already documents this native rebuild (`UnselectSpell06002852`).

**Classification.** Observer defect: a stale reference after a native state rebuild. Native replacement was not
observed either way.

**Fix.** Committed after the record of candidate `0d944a262f`; not built, not run.
- After the replacement click, pending is read from the live `backend.State.SpellSelections` entry for the Wizard
  book/list.
- The wait is bounded and records state instead of throwing blind: slot index before and after, whether the selection
  object was rebuilt, the stale captured pending, whether the preview knows the replacement or source, and the wait
  frames and seconds.

Focused gate: 26/2,949, 0 failures, production compiled, 51.6 s.

Remaining native observations for a future candidate:
- native deselection by replacement;
- session B cancel;
- mixed book hidden and revealed;
- metamagic tab hidden and revealed;
- book preparation and undo;
- mixed-fixture cleanup.

One build and one run need a new allowance (about 600 s, plus a margin if a peer build runs concurrently). The
ledger is about 2,299 of 2,400 s.

## Owner retest checklist

1. Option off: Wizard level-up "Choose Spells" shows no Summon Elemental and no "Elementals (n)" row. Observed
   natively.
2. Option on, then **close and reopen** level-up: "Summon Small Elemental" appears once as an ordinary row and can be
   selected. Observed natively. Then pick a different spell for that slot and confirm it replaces Summon Small
   Elemental (not yet observed natively).
3. Spellbook (level pages and the star metamagic tab), option off then on with the book reopened: original
   elemental rows and their metamagic copies are hidden, then shown once each; other spells and favorites
   unchanged. Not yet observed natively.
4. Summon Monster / Summon Nature's Ally elementals, prepared copies, domain elementals and the action-bar
   Elementals chooser are unchanged.

---

# Targeted verification (PR #133, 2026-11-12, candidate `60d2d1b618`)

The owner authorized observer safeguards and an itinerary change, focused checks, one build and one guarded run, and
raised the cumulative ceiling to 3,600 s. Earlier extensions are preserved: 1,800 s, then 2,400 s, with the target
900 s. Reviewed head `389e3c4112`. Feature code frozen and unchanged.

## Observer changes (source `60d2d1b618`)

- **Stale-selection diagnosis kept separate.** `7b7d5fa102` established an observer defect: native `UnselectSpell`
  rebuilds `LevelUpState`. It did not establish that replacement succeeded on `0d944a262f`. That remains not
  observed.
- **Live-state replacement.**
  - The intended slot is taken from the live allocation, where the source landed.
  - Before input: the native current slot must be the intended one. If not, the slot's own switch toggle (off→on)
    is used. The live selection must hold the source there and the live preview must know it. An ordinary,
    interactable, unselected replacement row is reacquired from the live layer.
  - Each observation is one coherent snapshot of the live `LevelUpState` selection (exactly one match required),
    the current preview, the current slot, and reacquired source/replacement rows.
  - The verdict requires:
    - intended slot source → replacement;
    - the other slot and the allowance unchanged;
    - the preview knows the replacement and not the source;
    - the source still one ordinary unselected interactable row while revealed;
    - committed known spells unchanged, 0 commits.
  - Bounded waits record before/after state. Toggles are set only off→on.
- **Itinerary.**
  1. The mixed book and metamagic cell first: hidden, revealed, preparation/undo, then fixture restoration. A failed
     restoration gates learning.
  2. Then one revealed level-up session: ordinary row, native allocation, replacement, normal cancellation.

  The hidden session and slot round trip are not replayed.
- **Fallback cleanup** is recorded as its own failing cell, never as normal cancellation.
- No verdict logic testable outside the game changed, so no new pure tests.

## Focused verification

- Focused gate (filter `cleanup2-elemental;original-summon-elemental;feature-settings`): 26/2,949, 0 failures,
  production compiled, 50.5 s.
- Patch targets: 50/50, 1.3 s.

These are compile and target evidence, not native verification of replacement.

## Candidate

| Item | Value |
| --- | --- |
| Source | `60d2d1b618` (full SHA in the development receipt), v0.0.183 development, purpose `hide-original-summon-elemental` |
| Package SHA-256 | `8353c5b64218b8a9f2c404899700037b86f5d8d448dbff6e0d43d6f77b1a0c3c` |
| DLL SHA-256 / MVID | `cd62c823a262bfca430e393231caa37d9e59f956d2ba6c13570c68566607278a` / `dd2fbe1c-84a8-49f1-bddf-9b1770814748` |
| Build | 627 s; no peer session or competing compiler (only this session's idle VBCSCompiler); 26/2,949 tests; strict UMM package validation passed; explicit `-ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references` |

## Native checkpoint: PASS 22/22

One guarded invocation on DATA. Same route as before: `Invoke-ExpandedSummoningRuntimeScenario.ps1`
(Params.xml-protected), Windows PowerShell 5.1 child, combined installation,
`disposable-expanded-summoning-cleanup2-native-review`, `reviewPhase=elementals-learning`, save-free.
- Evidence `runtime-evidence/20261112T1528377487196Z-disposable-expanded-summoning-cleanup2-native-review` (private).
- Game 64.5 s, wrapper 115.1 s. No fallback cleanup cell was recorded.

**Mixed spellbook and metamagic.** The disposable CL20 Wizard book holds:
- the known source `970c6db4…` (Summon Small Elemental);
- an ordinary level-2 control `08cb5f4c…` (Scare);
- Extend copies of both at level 3.

| Cell | Observed |
| --- | --- |
| `book-mixed-hidden` | Level-2 list: source object absent, 0 original elementals, control once; rendered (1 page): 0 originals, control once. Level-3 list: Extend source absent, Extend control once. Source and control still known; known objects exact; no disclosure |
| `favorites-mixed-hidden` | Native star toggle → `SetUpFavorite` (favorites seam installed). Backing list `[Scare:Extend]`; rendered control copy once, 0 originals; Extend source still in `GetCustomSpells(3)` |
| `favorites-revealed` | Rebuilt via level tab then star: `[SummonSmallElemental:Extend, Scare:Extend]`, each listed and rendered once |
| `book-mixed-revealed` | Level 2: source and control listed and rendered once each. Level 3: both Extend copies listed once. No disclosure |
| `preparation-no-disclosure`, `native-source-preparation`, `native-preparation-undo` | Native double-click memorized the exact source `AbilityData` in common slot 0; native double-click forgot it; known spells exact |
| `mixed-fixture-restored` | Control unknown again; both custom copies gone; known objects equal the pre-cell snapshot; no errors |
| `native-book-close` | Owned native close completed |

**Revealed learning, replacement and cancellation.** A fresh disposable Wizard2→3 level-up, with the option on
before opening.

| Cell | Observed |
| --- | --- |
| `learning-revealed-ordinary-row` | 83 active rows; source once, interactable, unselected; Summon Monster II once; no disclosure |
| `native-learning-allocation` | Native row toggle → `SetSpell`. Live selection (1 match) pending `[–,–]` → `[source,–]`; free 2 → 1; intended slot 0; the preview knows the source; the actor does not; 0 commits. **The native UI then auto-advanced the current slot to 1** |
| `learning-replacement-preconditions` | Slot 0 re-selected through its native switch; current slot 0; live pending `[source,–]`; the preview knows the source, not the replacement; source row on; replacement `9a46dfd3…` (Acid Arrow) row unselected and interactable |
| `learning-native-deselect` | Live after: pending `[Acid Arrow, –]`; free still 1; the selection object **and** preview were rebuilt (identities changed). The preview knows Acid Arrow and not the source. The source row is still once, unselected, interactable; the Acid Arrow row is on. Captured stale selection still read `[source, –]`, confirming the earlier diagnosis. Committed known spells exact; 0 commits; settled at the first check (0.08 s) |
| `native-learning-cancel` | **Normal** cancellation: owned Esc and confirm Yes; 0 commits; Wizard2; known spells exact; preview disposed; controller field restored. No fallback |
| `learning-restored` | Party, selection, creator, sources, pins and in-memory option exact; no rejected predicates |

**Screenshots inspected (private).**
- `000`: level-2 book shows only Scare.
- `001`: metamagic tab shows only Scare (Extend).
- `002`: metamagic tab shows Summon Small Elemental and Scare, each once.
- `003`: level-2 book shows Summon Small Elemental and Scare.
- `005`: level-up after replacement, with slot 1 holding Acid Arrow and the preview book showing Acid Arrow.

`004` (prepared slot) is not re-inspected; its structured cell passed.

**Not covered (unchanged limits).**
- Controller: no rendered observation. Coverage is the shared learning seam, the controller book postfix target,
  compile and pure tests.
- Domain/special grants are kept by policy and pure tests; not observed natively.
- Scroll copying is unrestricted.
- Unclassified learning contexts use the native fallback.
- Pending-choice filtering under a rebuilt hidden list is covered by focused tests only (a rebuild needs a reopen,
  which discards pending choices).

Earlier results stay with their candidates:
- `b0d235c33a`: FAIL 7/3.
- `312bb462e8`: FAIL 8/2.
- `0d944a262f`: FAIL 12/1, including its valid hidden and reveal-after-reopen learning observations.
- `7b7d5fa102`: focused/compile-only. This run is on `60d2d1b618`.

## Restoration

Wrapper record `runtime-evidence/expanded-summoning-restoration/20261112T1529594218486Z-disposable-expanded-summoning-cleanup2-native-review.json`.
- Live tree 254 files `5C3E4AD4…BFBB` before and after.
- `Params.xml` restored to `E44F4495…30BB` (receipt
  `runtime-backups/umm-params/20261112T1528049374232Z-144658a0fe534d96b8c65590b36f5a1c/`).
- An independent Windows PowerShell 5.1 comparison against the intake was identical:
  - live v0.0.140, DLL `00EF6757…66FD`;
  - `FeatureModules.json` `6E24B278…85D0`;
  - `Params.xml` byte-identical;
  - the same 16 enabled mods;
  - 95 saves, none modified since 15:00 UTC;
  - baseline and working save hashes unchanged.
- No Kingmaker process and no lock left. The mixed fixture was removed (verified cell), the disposable learning
  actor destroyed, and the in-memory option restored.

## Ledger (cumulative; ceiling 3,600 s by owner extension; history 900 → 1,800 → 2,400 → 3,600)

| Step | Seconds |
| --- | ---: |
| Carried | ~2,299 |
| Focused gate | 50.5 |
| Patch targets | 1.3 |
| Candidate build `60d2d1b618` | 627 |
| Guarded invocation (wrapper wall, incl. restoration) | 115.1 |
| Intake and restoration verification | ~15 |
| **Cumulative** | **~3,108** |

## Owner retest checklist

1. Option off: Wizard level-up "Choose Spells" shows no Summon Elemental and no "Elementals (n)" row.
2. Spellbook level page and star (metamagic) tab: no Summon Elemental and no Summon Elemental metamagic copy;
   other spells and favorites are unchanged.
3. Option on, then close and reopen level-up and the spellbook:
   - Summon Small Elemental appears once as an ordinary row, and its metamagic copy once;
   - select it, then pick another spell for the same slot, and it is replaced;
   - cancel the level-up, and nothing is learned.
4. Summon Monster / Summon Nature's Ally elementals, prepared copies, domain elementals and the action-bar
   Elementals chooser are unchanged.
