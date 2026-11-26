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
The character sheet shares the patched PC method, but the rendered sheet and the console screen
were not observed in that baseline pass. The reporter's version and mod environment are unknown,
so this classification is bounded to the tested environment.

### Engine contract (Kingmaker 2.1.7b, verified directly)

These findings come from the installed `Assembly-CSharp.dll`: SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`, MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`, decompiled locally with ilspycmd 9.1. No decompilation is
committed.

- `CharSScoreEntry.SetData(UnitDescriptor, Color32, bool)` sets `RaceBonus.text` to
  `UIUtility.AddSign(first Racial modifier in Stat.Modifiers)`, or `-` when there is none.
  `CharBScoresEntry`, the creator and level-up ability panel, calls this base method, so the
  creator and the character sheet share it.
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
- A committed-sheet capture was added for the rolled (`CustomCompanion`) route. It did not run;
  see Deferred.

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

## Deferred checks and limits

- **Committed character sheet.** The sheet capture admits only the registered `CustomCompanion`
  from the rolled route. That route's fixture pins Dice Roller 0.1.2, but 0.1.6 is installed.
  Updating the pin would mean re-verifying Dice Roller's bridge contract, which is outside this
  mission, so the rendered sheet was not observed. The sheet uses the same
  `CharSScoreEntry.SetData` that the creator panel (through `CharBScoresEntry`) and the
  mechanics probe exercised natively.
- **Existing character after reload; respec.** Not run. The fix writes no state, and an
  existing character goes through the same `SetData`. `working-save-elemental-native-respec`
  would also exercise the new panel check during respec visits.
- **Module disabled.** Not run natively. The patch does not read module state. The races stay
  registered, so existing elemental characters get the corrected label; native races are
  covered by the Dwarf control.
- **Console view rendering.** The console view model was exercised natively, but the console UI
  itself was not rendered in gamepad mode.
- **Other races in the creator.** The Ifrit, Oread and Undine creator regressions were not run.
  All 12 heritage sets are covered by the mechanics scenario through the real UI code.
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
