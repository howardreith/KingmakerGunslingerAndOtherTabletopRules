# Druid duplicate rows and giant sizes after v0.0.165

Development checkpoint, 2026-10-24, on
`codex/expanded-summoning-druid-duplicates-giant-sizes` from master
`00ef53dd82` (released v0.0.165 plus its record). The owner then wrote "Could
you go ahead and push, merge, and do a new release of this?"; see
[the v0.0.166 release candidate](#v0166-release-candidate).

The owner's reports (2026-10-24):

> Hey so - the fixes look good, but now we're doubling up on icons for summon
> nature's ally 1. Pony, dire rate, etc., they're all listed twice under summon
> nature's ally, and for all future iterations like 1d3 dire rates in SNA2 it's
> doubled up there too. Could you please fix that?

> Another thing - so when I look at the rules, a storm giant is a HUGE giant
> humanoid. As is a cloud giant. Fire a frost giants are LARGE. But they're all
> the same size in the implementation. Could you please make the huge giants
> properly sized?

Asked what they saw, the owner answered: "I would not say the giants look
identical - they all look distinct. But their heights are all the same and the
space they take up in game is identical." Their in-game screenshot (lab file
`giant_illustration.png`, not committed) shows Hill, Stone, Fire, Frost, Cloud
and Storm summons at one height, beside an elf of ordinary size.

## Changes

| Item | Cause | Change | Commit |
| --- | --- | --- | --- |
| Doubled SNA creatures | Native `ActionBarSpellsGroup.Fill` (0600451C, 113 IL bytes) lists the slot's Conversion rows, then appends the prepared spell's own `Variants`. Since v0.0.165 linked the tier-I children to their parent, a Druid who prepares SNA I itself had its own SNA I conversion row expanded into the ten creatures as well; SNA II's Single and d3 quantity rows did the same against SNA II's own variants. | When the prepared spell is the quantity row's own published parent and every projected child is already listed by that spell, the row is removed (`DruidSummonConversionPolicy.Project` removals) in the popup and in the factory list, where native and Call of the Wild append the variants themselves. Conversions from every other prepared spell project as before. | `e25fcc8ac7` |
| Giant heights | The family heights (Hill 1.0, Stone 1.2, Fire 1.4, Frost 1.5, Cloud 1.8, Storm 2.1) were drawn by a copied-model owner that refused, and returned the giant to its donor body, whenever the view's renderer or collider cache changed. Native `UnitMarkManager.AddMark/RemoveMark`, `UnitViewHandSlotData.MatchVisuals` and `UpdateBeltPrefabs` schedule exactly that rebuild (`MarkRenderersAndCollidersAreUpdated`, then `UnitEntityView.LateUpdate`), so in ordinary play every giant reverted. The v0.0.165 giant lineup kept its giants idle and never triggered it. | A prefix on `UnitEntityView.OnDataAttached` multiplies the view's own `m_OriginalScale` by the family factor before native attach writes `localScale = m_OriginalScale * m_Scale`. The view keeps it through its own refreshes and later size effects. | `c3db8b312a` |
| Huge space | All six giants kept the donors' Large corpulence (1.1) | The same prefix multiplies `m_Corpulence` by 15/10 for Cloud and Storm, the Huge families, before native attach builds the selection colliders and movement agent. Native `get_Corpulence` (max 0.5, size scale times base) then feeds the selection ring, colliders, movement and reach from the space edge. | `c3db8b312a` |
| Lineup survey | The longer row of Huge giants moved the survey's first acceptable spot 0.6 m, where a pillar stood between the Human (the fixture's only fog-of-war revealer) and the Hill Giant | The survey refuses a candidate when a scenery collider crosses the row at 1.2 or 2.5 m | `9e754dd2a9` |

Unit `Size` was already Huge for Cloud and Storm and Large for the others; it,
statistics, weapon reach (the 15-foot Cloud/Storm melee patch) and blueprints
are unchanged. A summoned Frost still waits for its exact provenance and is
scaled when its summoned buff appears; native Frost enemies are untouched.
Explicit release restores the two bases only while they still hold this patch's
values. Root scaling is already the production route for 18 other creatures
(`SummonViewScaleCatalog`, e.g. Dire Crocodile and Tyrannosaurus at 2.0); the
cleanup2 giant lane's prohibition on it was specific to that lane, which kept
gameplay space native. The owner's report asks for the space to differ.

Because reach is measured from the space edge, the larger Huge space adds about
0.55 m (1.8 ft) to Cloud and Storm melee reach compared with v0.0.165. That is
the direction of the rules (a Huge creature threatens 15 ft from a 15-foot
space), but it is a gameplay change the owner should know about.

`Cleanup2GiantRenderScalePrototype` keeps its request-owned scale-prototype
review use; production no longer calls it.

## Offline checks

- Development candidate `9e754dd2a9` (`Build-Local -DevelopmentPurpose
  summon-cleanup-iteration2`, cleanup2 reference bundle, 175.6 s): filtered
  domain selection 24/24, development-candidate validation against the v0.0.165
  baseline (7,140 identities, 0 appended), build-output, SoundBank and strict
  package validation. Package
  `e791ff58b07756dd9f80961998db660510cbf17510650433e8e1dc300b24d570`, DLL
  `f2586b39957cedb38602b8b1f6a94f95cb76f9aa1eeff5c535c7264ed0bc54c2`.
- New domain cases `expanded-summoning.cleanup4-druid-prepared-parent-rows` and
  `expanded-summoning.cleanup4-giant-native-size`; with the related Druid and
  giant cases, 14/14.
- `scripts/Test-SummonCleanup2ConsolePatchTargets.ps1` on that DLL: 40/40,
  including the new `ExactVariantFill` contract (Fill 0x0600451C, 113 bytes).

## Native evidence

All on development candidate `9e754dd2a9` except the first lineup attempt.

| Run | Phase / profile | Result | Notes |
| --- | --- | --- | --- |
| `20261024T1345297142650Z` | giant-lineup, gunslinger-only, candidate `c3db8b312a` | FAIL 52/7 | Heights already measured at Stone 4.20, Fire 4.91, Frost 5.26, Cloud 6.30, Storm 7.37 world units and all six release checks passed; the Hill Giant's fader was hidden by a pillar between it and the Human, so its height, the ratios and the picture failed. Fixed by `9e754dd2a9`. |
| `20261024T1355234337693Z` | giant-lineup, gunslinger-only | **PASS 74/0**, 156.7 s | Height over Hill (3.501): Stone 1.200, Fire 1.403, Frost 1.503, Cloud 1.800, Storm 2.105. View corpulence 1.1 for the Large families and the SM VIII Frost, 1.65 for Cloud and Storm (mechanical Size Huge). Each giant kept its exact size through native hover, `MarkRenderersAndCollidersAreUpdated` and its LateUpdate rebuild, native hide/show, an unrelated child and a weapon pose; release restored the exact bases and the lifetime re-acquired them. Picture `cleanup2-six-giants-human-lineup.png` (`66ba2406…`). Result `1fae9ccf…`, observations `819f855a…`. |
| `20261024T1400134458725Z` | menu-adapters, gunslinger-call-of-the-wild | FAIL 147/3 (game side) | All 12 Druid checks pass. Prepared SNA I: 10 creatures, 10 popup rows, 0 duplicates, 0 paw/quantity rows (`removed=1`). Prepared SNA II: 21 creatures, 21 rows, 0 duplicates, 0 quantity rows (`removed=2`); the factory list matches. The three failures are the known Feral Hunter "Alternate native hover closure failed" stop, the same exception as `20261024T0602279134237Z`. Result `a4a962d8…`, observations `a31d2f59…`. |

The menu-adapters host was stopped by the Claude Code session at about
14:00Z because the system ran critically low on memory; the game finished the
scenario and exited by itself at 14:04:04Z. Recovery by hand, in order:
`Restore-KingmakerCompatibilityProfile.ps1 -RunId
compat-20261024T135904Z-8a5f757e27b0` (restoration verified, lock released,
live mod back to the owner's 0.0.140, 254 files, no `Mods.kmg-compat-*`
folders); the four game-settings values the run had changed (screen resolution
height and value, Unity session count and id) written back from the
transaction's `game-settings-intake.json`, after which all 66 values match;
and all 95 saves compared against the wrapper's `preexisting-saves.json`
catalog (no change, no new file). The wrapper's own `protected-native.json`
still reads `ACTIVE` because it never reached its finally block. The
menu-adapters phase was not rerun.

## v0.0.166 release candidate

Release preparation `5d2760c091`: version
`0.0.166-expanded-summoning-druid-giant-sizes`;
`tools/validate_expanded_summoning_druid_giant_sizes_release166.py` admits,
relative to master `00ef53dd82`, no identity change, the eight pinned
production files and the version metadata, the unchanged project file, no
asset change and its own static record (21 corruption fixtures); the package
gains the v0.0.166 release notes (402 files with the SoundBank).

Gates in the frozen worktree `artifacts/worktrees/summon-cleanup4-release166`
at `5d2760c091`:

| Gate | Result |
| --- | --- |
| `Invoke-KmgGate.ps1 -Level Sprint -ReferenceBundleDir <cleanup2 bundle>` | PASS in 216.9 s: repository wrapper with the v0.0.166 validator, complete domain suite 2,742 / 2,742, exact-reference build and strict package validation. Package `71cce0f22488739e572b927f98d00ded6ce8d700b583b3b67944b4a2dc2293b8`, DLL `c671a3f0acdf56da0b1b888ab4e58eed5c384e78e5dd8b63121800cfe6d317bf`. The first attempt failed 23 tests only because the fresh worktree lacked the local `artifacts/inspection/bodyguard-native/Assembly-CSharp.il` dump; it was copied in unchanged (`33b4ad38…`), as for v0.0.165. |
| `disposable-expanded-summoning-visual-contracts` on that package | PASS 15/15 in 335.5 s, run `20261024T1524430216031Z` (result `77f24b96…`): all 117 creatures render within bounds and move, attack, take hits and die. |
| `working-save-expanded-summoning-creature-review`, creatures giant-crab, girallon, giant-stag-beetle, griffon, elephant, stegosaurus, brachiosaurus, gorgon, fire-giant, storm-giant, cloud-giant | PASS 38/38 in 616.5 s, run `20261024T1530391995990Z` (result `8ca29258…`). |

Both guarded runs restored the live mod exactly (`5C3E4AD4…BFBB`, the owner's
0.0.140, 254 files); 95 saves remain. The temporary reference junction under
`artifacts/worktrees/private/extracted-references` was created for the
harness's release build and removed afterwards. The Druid and giant
observations stay bound to development candidate `9e754dd2a9`, whose source
differs from the release only in its version metadata, release validator and
documents.

## Deferred

- The `menus` and `giants-off` phases were not rerun; neither path changed
  except that `giants-off` now reads the native size observation.
- Native save/reload of the Cloud and Storm Giants. The cold persistence flow
  (`Invoke-SummonCleanup2ColdCurrentStack.ps1`) is pinned to 0.0.163 in six
  scripts and their fixture tests. A reloaded view attaches through the same
  prefix the lineup exercises, and the persistence observer was updated to read
  the native size, but it has not run.
- Owner visual acceptance of the heights and space.
