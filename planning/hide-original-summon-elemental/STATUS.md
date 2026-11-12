# Hide original Summon Elemental by default (2026-11-12): status

**Reviewable development checkpoint; native checkpoint FAIL (observer defect), corrected but not re-run. STOP.**
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

## Validation ledger (900 s target, 1,800 s ceiling)

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
