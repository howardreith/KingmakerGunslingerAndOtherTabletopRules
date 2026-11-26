# Elemental heritage racial-bonus label repair

Owner mission `ELEMENTAL-HERITAGE-ABILITY-SCORE-REPAIR-MISSION.md` (2026-11-25), developed
in-thread at the owner's direction, without the agent-review controller. Branch
`codex/elemental-heritage-ability-score-repair` is cut from master `35763430ac`, which is
the v0.0.195 release source `ef0e1fe15d` plus its release records. Development checkpoint
only: no version bump, merge or release.

## Report (verbatim)

> "The alternate ability score heritages add all of the penalties to the ability scores.  Tried
> using the Sylph and found myself with +2 dex and Cha but -2 Con, Int, and Wis instead of just
> -2 to one of these."

Unknown: the reporter's mod version, selected heritage, other mods, and whether they read labels
or final scores. The reporter was not contacted.

## Diagnosis

**Display-only in the tested baseline.** In the tested Kingmaker 2.1.7b baseline (unchanged
product code at `adcba2f314` with the installed mod set), actual ability scores matched the
intended values for all twelve heritages. All eight alternate heritages produced incorrect racial
labels through the native PC score-entry and console view-model paths (the save-free method
probes under Regression coverage). The Sylph creator panel also visibly reproduced the defect.
The rendered character sheet does not show a racial bonus at all (its ability entries have no
`RaceBonus` field; observed on candidates, see Verification), so the defect is visible only in
the creator/level-up ability panel and, by the same method, the console stat view. The console
screen itself was not rendered. The reporter's version and mod environment are unknown,
so this classification is bounded to the tested environment.

### Engine contract (Kingmaker 2.1.7b, verified directly)

These findings come from the installed `Assembly-CSharp.dll`: SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`, MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`, decompiled locally with ilspycmd 9.1. No decompilation is
committed.

- `CharSScoreEntry.SetData(UnitDescriptor, Color32, bool)` sets `RaceBonus.text` to
  `UIUtility.AddSign(first Racial modifier in Stat.Modifiers)`, or `-` when there is none.
  `CharBScoresEntry`, the creator and level-up ability panel, calls this base method. The
  character sheet's `CharSScores` also calls it (`showPermanentValue: false`), but the sheet's
  entries have no `RaceBonus` component, so native `SetData` skips the label there: the sheet
  shows totals only. This was observed natively on 2026-11-26, not inferred from the
  decompilation. On the final candidate `a49dffd66c`, every bound sheet entry had a null
  `RaceBonus`: 48 in the eight respec visits, 36 in the six module-OFF sheets and 36 in the six
  module-ON sheets, 120 in total.
- The console `CharInfoStatVM.OnStatUpdated` applies the same first-modifier rule to `RaceBonus`.
- `ModifiableValue.AddModifier` keeps each descriptor's list sorted ascending by value, so the
  label is the most negative Racial modifier.
- `Racial` is in `DefaultStackingDescriptors`, so `ApplyModifiersFiltered` sums every Racial
  modifier. `PermanentValue` and `ModifiedValue` were therefore correct.
- `GetDescriptorBonus(Racial)` is `ApplyModifiersFiltered(0, Racial-only)`. Its result is
  clamped to `MinValueRaw`, which `ModifiableValueAttributeStat` sets to 5 for player-faction
  units outside True Death. A negative racial total therefore reads 5, so the label cannot use
  this method.
- `UIUtility.AddSign` renders negatives with an en dash (U+2013).
- The stat tooltip (`StatTooltipData` / `StatModifiersBreakdown`) lists individual stacking
  modifiers with their sources. It is unchanged.

The Wrath decompilation lead was used only as a pointer; every claim above was checked against
the Kingmaker assembly.

### Why the report matches Stormsoul

Production stores the General modifiers on the base race and puts
`ElementalHeritagePolicy.NetDeltas` (desired minus General) on the selected heritage marker,
both as `Racial`. For Stormsoul:

| Ability | Racial modifiers | Actual | Old label |
| --- | --- | ---: | ---: |
| Str | none | 0 | - |
| Dex | +2 | +2 | +2 |
| Con | -2, +2 | 0 | -2 |
| Int | -2, +2 | 0 | -2 |
| Wis | -2 | -2 | -2 |
| Cha | +2 | +2 | +2 |

The old labels are exactly the report. Natively, the baseline also showed worse cases: Lavasoul
Dex labeled -4 for an actual -2, and Gemsoul Cha labeled -2 for an actual **+2**.

## Repair

`src/KingmakerGunslinger/ElementalRaces/ElementalRacialBonusDisplay.cs`:

- Harmony postfixes on `CharSScoreEntry.SetData` and `CharInfoStatVM.OnStatUpdated`.
- They act only when the unit's race is one of the four registered elemental races. The races
  are registered whether or not the Elemental Races module publishes them, so an existing
  elemental character keeps a correct label with the module off. Every other race keeps the
  native label.
- The label is the engine's own effective Racial aggregate:
  `ApplyModifiersFiltered(offset, Racial-only) - offset`, invoked by reflection. The large offset
  keeps the `MinValueRaw` clamp from applying. There is no hand-written sum and no heritage
  table.
- A net-zero value renders `-` (console: no value), matching the native "no racial modifier"
  display. Nonzero values use `UIUtility.AddSign`.
- The patch changes labels only: no stat, allocation, fact or save state is written. A failure
  keeps the native label and logs one warning.

Stable GUIDs, keys, NetDeltas, markers and legacy General behavior are untouched, and no save
migration is needed.

## Regression coverage (observer changes)

- `RuntimeTesting/ElementalRacialLabelObservation.cs` is a native-method probe, not a rendered
  screen. For PC it calls the real `CharSScoreEntry.SetData` on an inactive request-local entry
  with no canvas; for console it constructs the real `CharInfoStatVM` and reads its value. It records, per ability, Base/Permanent/Modified values and every Racial
  modifier with its source fact, component, stacking and active state. Expected labels derive
  only from the heritage's final racial modifiers.
- `ElementalHeritageMechanicsScenario` (save-free) checks labels for all 12 heritages. It also
  checks each heritage with an unrelated Charisma Enhancement +2 (labels must not change), and
  at every step of the four transition sequences: legacy General, A, B, General return, A return
  and marker-first. A native DwarfRace control with a second Constitution Racial modifier must
  keep the native first-modifier label, which proves the patch stays inside the elemental
  context.
- The creator regression (`ElementalCharacterCreationRacialLabels.cs`) reads the rendered
  `CharBScoresEntry.RaceBonus` text in the real creator ability panel once allocation is
  complete. That happens on the first pass and on every post-revision pass, requiring all six
  entries bound to the current preview, and compares each rendered total with the live
  `PermanentValue` (the creator panel shows permanent values). At each native final review it
  also runs the native-method probe on the preview; that is a method check, not an observation of
  a rendered final-review control. The final-review check exists because the creator skips the ability
  phase after a revision that leaves skill points unchanged (Smokesoul -> Stormsoul).
- Regression route 0 changes from General -> A -> General to General -> Smokesoul ->
  Stormsoul -> General. The route domain test was updated.
- `RuntimeTesting/NativeSheetRacialLabelProbe.cs` reads the rendered native character sheet
  (`CharacterScreenController.AbilityScores`): six visible entries bound to the owner's live
  stats, each displayed total equal to `ModifiedValue`, and the racial label compared where the
  sheet renders one (it does not; recorded per entry as `raceBonusFieldPresent`). The sheet's
  character, section, window, group, selection and pause are restored and asserted.
- The committed-sheet capture runs in `working-save-elemental-native-respec` (Fighter point-buy,
  a registered native mercenary) on the respec original after every `Player.RespecCompanion`
  commit, and on the rolled regression route.
- `ElementalRacePersistenceScenario` runs the native-method probe inside every fixture
  observation (24 fixtures: prepare after native respec, module-OFF fresh load before/after
  level-up and rest, module-ON fresh load after native respec) and captures the rendered sheet
  for the six Sylph fixtures after each fresh load. A capture failure is recorded on the
  evidence and fails its assertion without aborting the phase.

## Verification

All runs used Windows 10 host DATA, Kingmaker 2.1.7b and Steam app 640820. The live install
enabled the user's existing mod set, including UMM 0.33.0. Runs were guarded: deploy, reuse of
the installed artifact, `KMG_AUTOMATION_WORKING` only for creator runs, then restoration.

Focused gate (`scripts/Invoke-KmgGate.ps1`): production compilation passed. Filter
`elemental-heritages;elemental-chargen` ran 20 tests with 0 failures; the wider filter
`elemental;runtime;harmony;csproj;source` ran 308 with 0 failures, before the fix commit.

Development candidates, built with `Build-Local.ps1 -DevelopmentPurpose ... -DevelopmentTestFilter
'elemental-heritages;elemental-chargen'`:

| Purpose | Commit | Package SHA-256 | DLL SHA-256 | Build |
| --- | --- | --- | --- | ---: |
| elemental-label-baseline | `adcba2f314` (observers, no fix) | `6d358d7544f90d21898908a06d01e139eb42547e3616078d52cde4f057a7b5bf` | `009ab2cdf75ef22a105e10e9050caabdff5791fb009d30a9013602f3142739f0` | 465 s |
| elemental-label-candidate | `1650c97019` (fix) | `cf8c0dbb82398a2de43d21ce80bb17097c01a18333d64354b769eac0afc1ae48` | `46678922c33cbf4efd9243f3bbbebe9566002fbea2a2f8fa8f3a8dc5b08887a4` | 469 s |
| elemental-label-candidate2 | `fb8759a709` (fix + observer corrections) | `522b498b987e8f85f44f1f52044bc99df6eeac124f8607148cc7f5c06a3ddd16` | `a2680dd2ab0b05e2bc6f65748d8b7b290fdcbd8839959b930da9357920ee3da4` | 470 s |

Native results:

| Run (UTC 2026-11-26) | Build | Scenario | Result |
| --- | --- | --- | --- |
| 03:57 | baseline | heritage mechanics | FAIL, as expected. See note 1. |
| 03:59 | baseline | Sylph creator (point-buy) | Not run. The launch stalled on Unity's "Pathfinder Kingmaker Configuration" dialog. See Incidents. |
| 04:56 | candidate | heritage mechanics | PASS, 81/81 |
| 04:58 | candidate | Sylph creator (point-buy) | FAIL in the new sheet hook (point-buy unit is not a registered mercenary), corrected in `fb8759a709`. Creator panel labels and graphs were correct. |
| 05:12 | candidate2 | heritage mechanics | PASS, 81/81 (in-game 47.8 s) |
| 05:14 | candidate2 | Sylph creator (roll) | Refused: fixture requires Dice Roller 0.1.2; 0.1.6 is installed. See Deferred. |
| 05:23 | baseline | Sylph creator (point-buy) | FAIL, as expected. Rendered Smokesoul INT **-2** for an actual 0. |
| 05:27 | candidate2 | Sylph creator (point-buy) | **PASS, 12/12, 3 characters** |

1. Baseline mechanics observations. The baseline observer expected an ASCII minus, so it also
   failed correct General labels. That reader bug was corrected in `a9a512ca50`. Re-evaluated
   offline against the retained raw observations, with the en-dash expectation and both verdicts
   kept: 4/4 General heritages correct, and 8/8 alternates wrong on both PC and console, with
   correct actual racial values throughout.

Candidate2 creator run (`20261126T0527302441164Z`), routes G->Smoke->Storm->G, Smoke->Storm
and Storm->G->Smoke:

- All 7 rendered panel observations were bound to the current preview and correct.
- All 9 final-review native-method probes were correct, including Stormsoul at every visit.
  These are method probes, not rendered final-review controls.
- All 59 racial graphs were exact: facts, markers, providers, resources, stat overlays and
  ranks.
- The allocation baseline was preserved across every revisit after allocation.
- All three characters committed.

Restoration after every pass:

- The live mod was restored byte-exact from its pre-deploy backup (254 files).
- At each launch UMM pruned a stale `KingmakerMountedCombat` entry (no folder in `Mods`) from
  `Params.xml` and updated `LastUpdateCheck`. After every pass the intake bytes were put back
  (SHA-256 `18c2d3c1d4fbbaeafb82914a72f226018dd9c90bc08c42e85f2407cb3506c88c`).
- No protected save was touched: creator runs block native save writes.

Review stills (lab originals in the run directories; committed copies are byte-identical):

- `evidence/elemental-heritage-label-repair/baseline-adcba2f-smokesoul-creator-panel.png` (SHA-256
  `646082101454c84f080e220eb2a6b08e627e2c21a119ea8c9c29e7114ed00114`) shows Smokesoul after
  allocation with Racial Bonus INT -2 beside a score of 10.
- `evidence/elemental-heritage-label-repair/candidate-fb8759a-stormsoul-creator-panel.png`
  (SHA-256 `128778983cf77c6aaeb7a84322ccd9f85765062d6cce56cddf570ee43a5beb53`) shows Stormsoul:
  +2 Dex, - Con, - Int, -2 Wis, +2 Cha.

Screenshots support presentation only; the mechanics claims rest on the structured observations.

### Review follow-up (sheet, totals, respec, reload, module OFF)

The review of `2a69d3aac7` asked for the rendered sheet, an existing character after reload, a
native respec commit, the module-OFF state and an explicit displayed-total check. The observer
changes above were built as three more development candidates (same build command, filter
`elemental-heritages;elemental-chargen;elemental-race`, 35 tests, 0 failures each):

| Purpose | Commit | Package SHA-256 | DLL SHA-256 | Build |
| --- | --- | --- | --- | ---: |
| elemental-label-candidate3 | `c4c35847ed` | `2f53ed3e8d7c3f3bfe8567e0afa286c7c6ce5f4898ede288a91fdf1c12ce93e1` | `b68c620ac49f98a91fbab3dbbce5d63fa283bcf7ba60edb0aaab6c0983790f5a` | 460 s |
| elemental-label-candidate4 | `925181dddb` | `7ae20947e653596ca81f0d736282c743f85a659f121a5663ec87e10a8f1ed87b` | `1b689230e4ddd4c5a53e48e734d25f4b8b87f83842386a812f6c252b7194b600` | 436 s |
| elemental-label-candidate5 | `a49dffd66c` | `33c916bd6e690016f4e6cbfef7843c4b43a00526c0f151ba391cac671af274af` | `e0379d4b08e4e0e4a17adede34f9038fc682012f29be24f6c217df943d03a782` | 491 s |

The production patch is unchanged from `1650c97019` in every candidate; only observers changed.

| Run (UTC 2026-11-26) | Build | Scenario | Result |
| --- | --- | --- | --- |
| 12:20 | candidate3 | heritage mechanics | PASS, 81/81 |
| 12:22 | candidate3 | Sylph native respec | FAIL at the first commit: the new sheet hook built the screenshot helper, which admits only its own scenarios. Observer fixed in `3d24b903c9`. |
| 12:25 | candidate3 | persistence prepare | PASS |
| 12:27 | candidate3 | module OFF, fresh load | 174/175. Native label probes 96/96 exact; sheet captures never started because the capture method clears `_currentUnit` (observer fixed in `3d24b903c9`). |
| 12:40 | candidate4 | heritage mechanics | PASS, 81/81 |
| 12:42 | candidate4 | Sylph native respec | FAIL: every visit committed and passed except the sheet check, which expected a racial label the sheet does not render (`RaceBonus` null). Fixed in `a49dffd66c`. |
| 12:53 | candidate4 | persistence prepare / module OFF | PASS / 174/175 (same sheet expectation). |
| 13:09 | candidate5 | heritage mechanics | **PASS, 81/81** |
| 13:10 | candidate5 | Sylph native respec | **PASS, 12/12** |
| 13:23 | candidate5 | persistence prepare | **PASS, 66/66** |
| 13:26 | candidate5 | module OFF, fresh load | **PASS, 175/175** |
| 13:28 | candidate5 | module ON, fresh load + native respec + cleanup | 254/255. All label assertions pass; one pre-existing failure (note 2). |

Candidate5 results (`a49dffd66c`):

- **Native respec** (`20261126T1310450512574Z`): eight creator visits, General, General,
  Smokesoul, Stormsoul, General, Stormsoul, Smokesoul, General. The first commits the
  registered mercenary; the other seven are real `Player.RespecCompanion` commits with one
  success callback each and the original identity kept. In every visit:
  - the rendered creator panel labels and displayed totals (`PermanentValue`) were exact;
  - the committed sheet's six entries were bound and visible, and each displayed total equaled
    the live `ModifiedValue`;
  - exactly one owned heritage marker matched the visit.
  The sheet renders no racial label (`raceBonusFieldPresent` false on all 48 entries).
- **Existing characters after reload, module OFF** (`20261126T1326001904784Z`): 24 saved
  fixtures (4 races x 3 heritages x 2 sexes) loaded in a fresh process with Elemental Races
  off. All 96 native-method label observations were exact (load, level-up, rest, re-spend). The
  six Sylph fixtures (General, Smokesoul, Stormsoul; both sexes) were opened on the rendered
  native sheet: totals exact, state restored exactly.
- **Module ON after reload and native respec** (`20261126T1328558287358Z`): 120/120 native-method
  label observations exact. The six Sylph replacements' rendered sheets after native respec into
  their restored heritages were exact. Fixture cleanup passed (`elemental-race-persistence-cleanup`).
- The Dwarf control, the Enhancement case and all 12 heritage sets passed again in mechanics.

2. Pre-existing persistence capture check. `elemental-race-persistence-restored-captures` fails
   only through `PhysicalLifecycleEvidenceExact`. It requires the lifecycle rows to cover every
   published alternate trait. Since v0.0.120 that is 21 traits. `NereidFascination`
   was published in `d5aa78b615`; `TreacherousEarth` became published in the merge
   `c5fc5ab3b8`, the commit the v0.0.120 tag references. The ordinary 24-fixture matrix exercises
   19; those two belong to the separate `qualificationTrait`/`qualificationEffect` scopes. All
   168 lifecycle rows were present and exact; only the trait-set comparison fails. This was
   classified by inspecting that deterministic contract, not by rerunning unchanged code; it is
   independent of labels. It is left unchanged for separate review.
3. The 4-launch chain stopped after the module-ON phase, so `elemental-race-persistence-verify-absent`
   did not run on candidate5. The module-ON cleanup assertion passed, and the working save was
   restored byte-exact to its intake bytes and mtime afterwards.
4. Reader-only re-evaluations, with original verdicts kept in the run directories:
   - The candidate2 creator run's recorded panel totals equal the live `PermanentValue` 42/42.
     That run predates the in-game total assertion.
   - Candidate4's respec visits pass 8/8 and its module-OFF sheets pass 6/6 under the corrected
     sheet reader. Candidate5 then executed the corrected reader natively.

Restoration after every pass: the live mod was restored byte-exact from its pre-deploy backup.
`Params.xml` went back to its intake bytes. `FeatureModules.json` was restored after the OFF
phase. All 95 saves were hash-checked against intake, including `KMG_AUTOMATION_WORKING` with
its original bytes and mtime (2026-10-05T20:09:11Z). No other save changed.

## Deferred checks and limits

- **Console view rendering.** The console view model was exercised natively, but the console UI
  itself was not rendered in gamepad mode.
- **Other races in the creator.** The Ifrit, Oread and Undine creator and respec routes were not
  run. All 12 heritage sets are covered through the native label methods in the mechanics
  scenario, and all 24 race/sex/heritage fixtures through the persistence phases.
- **Fresh-load absence phase.** `elemental-race-persistence-verify-absent` did not run on the
  final candidate (note 3).
- **Rolled creator route.** The rolled regression still pins Dice Roller 0.1.2, but 0.1.6 is
  installed. The committed sheet is covered by the respec route instead.
- **Persistence capture check.** The trait-set comparison that fails since 0.0.120 is
  pre-existing and left for separate review (note 2).
- **Wider validation.** No complete domain suite, Sprint gate or compatibility profiles were
  run.

## Incidents

- **03:59Z stall.** The second launch of the baseline pass started about 3 s after the first
  game exited. It sat on Unity's configuration/resolution dialog and never accepted the request
  or touched a save. The orchestrator timed out and retained the lease as RecoveryRequired.
  With owner approval, exactly that PID was terminated. Then
  `Complete-KingmakerRuntimeLease.ps1` ran, the live mod was restored byte-exact, and
  `Params.xml` was restored to intake. Cause unknown; a held modifier key at launch is one
  possibility. Later passes waited 20 s between launches and had no recurrence.
