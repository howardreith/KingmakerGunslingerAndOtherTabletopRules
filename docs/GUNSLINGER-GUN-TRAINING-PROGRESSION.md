# Gun Training: three supported types, four opportunities (DEF-04 / S4 T1)

Status: **development change on a review branch, awaiting review. Not merged, not released.** Branch
`codex/def04-gun-training-progression`, stacked on draft PR #177 at its accepted head `dcea649f5e`. Nothing here is a
release qualification. Sprint 4 stays open; its other firearm-policy stories and the Sprint 5 roster decisions are
outside this cut.

## The selected policy

Recorded from the owner's direction of 2026-11-16:

> Retain the existing training opportunities and their compatibility behavior. An opportunity grants one
> still-untrained supported firearm type when one remains. Once all supported types are trained, it grants no further
> category or stacking bonus. Make that limitation explicit in the progression presentation and descriptions.

The supported types are Pistol, Musket and Blunderbuss (`OfficialFirearmSupport.Kinds`). The cut adds no firearm
category, no repeat-training bonus and no compensating benefit, and it does not remove level 17 from the shared
progression.

| Character | Gun Training opportunities | Outcome |
|---|---|---|
| Base Gunslinger | 5, 9, 13, 17 | three distinct types at 5, 9 and 13; the opportunity at 17 is exhausted and grants nothing |
| Mysterious Stranger | 9, 13, 17 | a useful choice each time; all three types trained |
| Pistolero | none (Pistol Training at 5, 9, 13, 17) | four ranks; rank 4 at 17 |
| Musket Master | none (Musket Training at 5, 9, 13, 17) | four ranks; rank 4 at 17 |
| Existing character holding a hidden legacy type | as its class | any opportunity with an official type still untrained stays required and offers that type |

## Finding: gameplay already followed the policy

Nothing in the mechanics was changed. What each part does, from the source and from the engine, checked by
decompiling `Assembly-CSharp.dll` of Kingmaker 2.1.7b (`game-copies/loader-compat`):

- **Each type once.** The three official choices (`GunTrainingBlueprints.CreateChoice`) are rank-one class features.
  `BlueprintFeatureSelection.CanSelect` refuses a rank-one feature the unit already has, so a later opportunity
  offers only untrained types.
- **Required while a type remains; completes when exhausted.** The selection is not obligatory (since `8937ae67fe`,
  defect D1). `LevelUpState.IsComplete` is false while any unselected state `CanSelectAnything`, and
  `FeatureSelectionState.CanSelectAnything` is true for an obligatory selection always, otherwise only while some
  item can be selected. So the pick is required while an official type is untrained, and an opportunity with none left
  completes the level with nothing taken.
- **Exhausted grants nothing.** No component, fact or bonus is attached to the opportunity itself; damage comes only
  from the chosen type's `GunTrainingDamage` through `FirearmTrainingRuntime.Resolve`.
- **Mysterious Stranger.** The archetype removes the level-5 entry (`MysteriousStrangerBlueprints.cs`), leaving 9, 13
  and 17.
- **Ranked archetype training is separate.** Pistolero and Musket Master remove all four Gun Training entries and add
  Pistol Training or Musket Training (four ranks) at the same levels. `FirearmTrainingPolicy.Evaluate` gives Dexterity
  plus rank − 1 to one-handed (Pistol Training) or two-handed (Musket Training: musket and blunderbuss) firearms and
  takes the best of that and exact Gun Training; it never adds them together.
- **Damage and misfire unchanged.** Dexterity to damage once per weapon-stats rule; a Broken trained firearm's misfire
  value rises by 2 instead of 4 (`GunTrainingPolicy`).
- **Legacy types.** Rifle and Revolver choices stay registered with their stable identities but are hidden and not in
  the selection, so they are never offered. A character holding one keeps it, and it does not count toward the three
  official types, so an opportunity with an official type untrained stays required.

So the defect was in presentation: the selection said only "Select one firearm type", named no types and said nothing
about a once-each limit, the fourth opportunity, the Mysterious Stranger's levels or the ranked replacements. The
progression preview lists four Gun Training entries, which is accurate: there are four opportunities.

The issue is an exhausted additional training opportunity. Level 17 is not empty (it carries the class's other
level-17 features), and level-up is not blocked there; both are shown below.

## What changed

Text only, in `src/KingmakerGunslinger/Blueprints`:

| Description | Now says |
|---|---|
| Gun Training selection (`KMG.GunTraining.Selection.Description`) | "Select one supported firearm type: Pistol, Musket or Blunderbuss. You add your Dexterity modifier to damage when firing that type, and its Broken-state misfire increase is reduced from 4 to 2. Each type can be trained only once. A Gunslinger gets this choice at levels 5, 9, 13 and 17; once all three types are trained, a remaining opportunity grants nothing more: no further type and no additional bonus. A Mysterious Stranger gets it at levels 9, 13 and 17 instead. A Pistolero or Musket Master never gets it: Pistol Training or Musket Training replaces it and gains a rank at each of those levels." |
| Mysterious Stranger | adds "Gun Training comes at levels 9, 13 and 17 instead of 5, 9, 13 and 17, so each of the three choices trains a firearm type." |
| Pistolero | adds "Pistol Training replaces Gun Training at levels 5, 9, 13 and 17 and gains a rank each time." |
| Musket Master | adds "Musket Training replaces Gun Training at levels 5, 9, 13 and 17 and gains a rank each time." |

Blueprint identities, localization keys, ranks, progression entries and serialized facts are unchanged. The text has
no translations in the repository. `scripts/Test-IconPolishRound2Assets.ps1` pins `GunTrainingBlueprints.cs` by hash
to protect its selector icon assignments; the pin moves to the new bytes with a comment, as the D1 fix did.

Tests and observers:

- `gun-training.presentation-states-the-policy` (domain): the four descriptions carry the statements above and the old
  selection text is gone; the official types are exactly Pistol, Musket, Blunderbuss.
- `disposable-gunslinger-gun-training` keeps its direct-fact controls (a Pistol fact added directly: +4 damage at
  Dexterity 18, Broken-state misfire) and now also runs `RuntimeTestRunner.GunTrainingLevelUps.cs`: detached chargen
  units levelled through real `LevelUpController` visits (the `FavoredClassLevelUpHarness` helpers; the
  `LevelUpState.IsComplete` gate, then `ApplyLevelup`), with Gun Training chosen explicitly and every other choice
  filled deterministically. Each group runs under its own guard and must leave its own evidence. It does not need the
  Favored Class mod.
- `scripts/compatibility/loader-compat/sessions/gunslinger-gun-training-levelups.json` runs that scenario with the
  candidate alone, at the main menu, in a fresh process. No save is loaded or written.

## Native level-up transactions

Session `20261117T033517Z-owner-0.33.0-gunslinger-gun-training-levelups`: PASS, 12 of 12 assertions. Candidate
`0c0f4f7b7708` (package `39a759e6cb53cbe38bdad69a26a4dabebdd8dd9c85365f727578dcbe4047e62b`, DLL
`16abc5e7317dd100fab448e43ceddf70e1764882243a6cb16fb45d523788e8db`), UMM 0.33.0, Harmony 2.3.6, only the candidate and
the loader harness (`41e4300c58a6…`) active. Raw evidence stays on the lab machine under
`runtime-evidence/loader-compat/`; `gun-training-levelups.json` SHA-256 `00ee6d8a9b6c…`, `runtime-result.json`
`b00330fa1564…`. The session reports 95 owner saves unchanged, the owner installation unchanged and the registry
restored.

Offered types are listed as the selection offered them (only selectable items).

| Character | Level | Required | Offered | Taken | Training facts | Completed |
|---|---|---|---|---|---|---|
| Base | 5 | yes | Blunderbuss, Musket, Pistol | Blunderbuss | 0 → 1 | yes |
| Base | 9 | yes | Musket, Pistol | Pistol | 1 → 2 | yes |
| Base | 13 | yes | Musket | Musket | 2 → 3 | yes |
| Base | 17 | no | none | none | 3 → 3 | yes |
| Mysterious Stranger | 5 | Gun Training not opened | | | | yes |
| Mysterious Stranger | 9 | yes | Blunderbuss, Musket, Pistol | Musket | 0 → 1 | yes |
| Mysterious Stranger | 13 | yes | Blunderbuss, Pistol | Blunderbuss | 1 → 2 | yes |
| Mysterious Stranger | 17 | yes | Pistol | Pistol | 2 → 3 | yes |
| Legacy, Rifle before 5 | 5, 9, 13 | yes | the untrained official types (3, 2, 1) | Blunderbuss, Pistol, Musket | 1 → 4 | yes |
| Legacy, Rifle before 5 | 17 | no | none | none | 4 → 4 | yes |
| Legacy, Rifle replacing Blunderbuss at 16 | 17 | yes | Blunderbuss | Blunderbuss | 3 → 4 | yes |

Every level from 1 to 17 completed for every character; the table lists only visits where Gun Training opened, plus
the Mysterious Stranger's level 5. At each required visit the offered set equalled exactly the untrained official
types, and Rifle or Revolver was never offered.

- **Base, after 17:** each official type resolves to Dexterity (+6 with this unit's Dexterity, raised by its
  deterministic ability increases), and the native weapon-stats bonus damage of each production firearm is +6 at 16
  and +6 at 17: the exhausted opportunity added nothing.
- **Cancel:** a cancelled level-9 visit with Pistol selected held it in the preview only (preview rank 1); afterwards
  the character was level 8 with no Pistol fact, and the real level 9 then took Pistol.
- **Legacy states** are constructed: a hidden Rifle fact was added directly (before level 5, or in place of
  Blunderbuss at 16). They are compatibility controls, not an old save. The Rifle fact was kept exactly once in both.
- **Pistolero:** Gun Training never opened; Pistol Training ranks 1, 2, 3, 4 at 5, 9, 13, 17; at 17 Pistol
  Dexterity + 3 = 9 (entitlement and weapon bonus damage), Musket and Blunderbuss untrained.
- **Musket Master:** the same with Musket Training; Musket and Blunderbuss 9, Pistol untrained. No Gun Training fact
  on either.
- **Presentation:** the published blueprints' runtime descriptions carry the text above.
- **Isolation:** six detached units, party and global unit lists unchanged.

## Validation

| Check | Result | Time |
|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'gun-training;firearm-feat-icons;mysterious-stranger;pistolero;musket-master'` | production compilation passed; 22 of 22 focused tests (including `Sprint36Tests` Gun Training cases and the D1 `GunslingerDeedFixTests` case) | 16 s |
| `scripts/Test-IconPolishRound2Assets.ps1`; `tools/validate_icon_catalog.py` | passed (29 protected files); passed | under 5 s |
| `scripts/Build-Local.ps1 -DevelopmentPurpose gun-training-progression` | development candidate above; development-candidate repository validation passed, including the roster coverage suite and its freshness check | 455 s |
| Complete domain suite (`KMG_TEST_FILTER` unset) | 3,071 of 3,071 | 75 s |
| Loader-compat session above | PASS, 12 of 12 | 102 s (scenario 64 s) |

## Limits and deferred checks

- **Respec not run.** The native respec (`Player.RespecCompanion`) needs a companion in a loaded game and the doll
  room, which the save-free main-menu scenario does not have. From the decompiled method: it creates a fresh unit and
  restores only features that are not from a progression, class, race or item and are not class features. All Gun
  Training choices, official and legacy, are class features, so respec discards them and the rebuilt character takes
  its opportunities again through the same level-up selection shown above. A legacy Rifle or Revolver fact would
  therefore not survive a respec; the rebuilt character trains the three official types. This is derived from code,
  not observed, and other respec mods were not examined.
- **No actual historical save.** The legacy states are constructed facts on fresh units, not a character saved by an
  older version.
- **No UI screen observation.** The level-up screens and progression preview were not looked at; the text was read
  from the runtime blueprints.
- **One level-up path per character.** A single deterministic pick order per character; with three rank-one choices
  the order does not change the rule.
- The scenario's existing direct-fact misfire and damage controls are unchanged and still pass; they are not
  level-up evidence.
