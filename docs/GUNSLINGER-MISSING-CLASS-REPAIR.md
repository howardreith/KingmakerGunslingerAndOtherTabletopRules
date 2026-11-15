# Missing Gunslinger class repair (report against v0.0.189)

Status: **development repair, reviewable, not released.** This record covers one review branch and one development
candidate. Nothing here is a release qualification, and v0.0.189 stays published and unchanged.

## The report

A player on Pathfinder: Kingmaker 2.1.7b, Unity Mod Manager 0.33.3.0 and mod 0.0.189 (DLL SHA-256
`f963c6158bbd8b58dda7baba29644889d53adc055757b08fb266879f5aa12e1d`, the published DLL) had no Gunslinger class in
character creation. Their `output_log.txt` shows, in order:

1. `[blueprints][initialize.root-cause]` `Player-facing fact has incomplete localization:
   KMG_BleedingWound_HitPoints_Armed`, raised from `PlayerFacingPresentation.Visit` under
   `GunslingerClassBlueprints.Register`.
2. `[blueprints][protection-from-alignment.rollback-failed]` `The unit-fact description assignment did not verify.`
3. 3426 registrations rolled back, `initialize.failed`, and a class catalog without the Gunslinger.

The log reports the locale as `zhCN` while the screenshots show Italian text, and lists 51 mods.

## What was wrong

### 1. Eleven buffs had no name or description

`PlayerFacingPresentation` walks the Gunslinger progression and archetypes through level entries, selection lists and
public component fields, and requires every visible project fact (`KMG_` name, not hidden) to have a name and a
description. Eleven buffs reached that way were created with neither:

| Blueprint | Released GUID | Repair |
|---|---|---|
| `KMG_BleedingWound_HitPoints_Armed` | `e563e8cdec324c59959c4dc8b9449702` | named and described |
| `KMG_BleedingWound_Strength_Armed` | `74749a03f92e4fabaec2830bd66f7731` | named and described |
| `KMG_BleedingWound_Dexterity_Armed` | `cd51a3e250094365b54929490a1d64b6` | named and described |
| `KMG_BleedingWound_Constitution_Armed` | `479e8f2c4dc7480cac9664ac10232e21` | named and described |
| `KMG_ExpertLoading_Armed` | `5a21b3ff5940476f97fe5f7844fa4509` | named and described |
| `KMG_LightningReload_Used` | `b35b22a49a4c4645a1b730698d35837d` | named and described |
| `KMG_SlingersLuck_SavingThrow_Armed` | `841a2ca5e4994da48fadcc4f3a3c3579` | named and described |
| `KMG_SlingersLuck_SkillCheck_Armed` | `e553eb5c573b4b178f4b973850f5a0e3` | named and described |
| `KMG_DeathsShot_Armed` | `dfc8df775a53441292502d05f13334e4` | named and described |
| `KMG_StunningShot_Armed` | `c2b6f72548824ce2aff95c21cfbe81b3` | named and described |
| `KMG_DeathsShot_DeathEffect` | `b3ed104593874cdc9a0f5bbf99d26cd2` | hidden with Kingmaker's `HiddenInUi` buff flag |

Only the first was named in the report. The other ten were found by a census of the same traversal in the running
game (below); repairing only the reported one would have failed on the next.

The ten visible buffs are real states shown on the gunslinger until they are used up, so they now carry text, in the
same form as the existing `Deadeye Armed` buff. `KMG_DeathsShot_DeathEffect` is different: Death's Shot resolves its
Fortitude save and marks the target itself, and no code path applies this buff to a unit. It is a retained identity,
so it is hidden through the engine's own flag instead of being given text nobody would read. No buff is treated as
hidden because of its name.

### 2. The check accepted an engine placeholder

The check was `string.IsNullOrWhiteSpace(fact.Name)`. In Kingmaker 2.1.7b the conversion of a `LocalizedString` that
was never stored returns the literal text `<null>` (decompiled `LocalizedString.op_Implicit`), which is not empty. So
on a stock game the eleven buffs passed with `<null>` as their name and description. The reported installation resolved
the same unstored string to an empty value, and the first of the eleven failed the check.

The check now asks whether text was *assigned*: a stored string whose key resolves to real text. An unstored string,
a blank result and the engine's `[unknown key: ...]` placeholder are each refused, on every installation. A buff's
native `HiddenInUi` flag is honoured next to a feature's `HideInUI` and an ability's `Hidden`.

### 3. Rollback validated the text it was putting back

`ProtectionFromAlignmentPublication` replaces the description of fifteen native facts and restores them if
initialization fails. It restored through `BlueprintUnitFactAccess.SetDescription`, which also requires the result to
display as non-empty text. The four native Protection from Evil/Good/Law/Chaos buffs store an empty description, so
their own original state could not be put back: the setter wrote the value, then threw, and every entry after it,
and every component array, stayed modified. This happens on a stock English game after any initialization failure;
it does not depend on the reporter's setup.

Rollback now writes back the exact captured object with no display-text condition, each entry independently. A slot
is written only while it still holds the object this publication stored there, so a later change by another mod is
left alone and reported. The outcome is logged as `[protection-from-alignment][rollback.complete]` only when nothing
is left, and as `[rollback.incomplete]` with each unresolved entry otherwise. Publishing new text still goes through
the validating setter.

### 4. Misleading status text

The settings panel showed a hard-coded `0.0.154` build label. It now shows the loaded mod's own name and version from
its loader entry, followed by one line for the content state: initialized, not initialized yet, or initialization
failed (with what is unavailable and where the details are). Object names and stack traces stay in the log.

### 5. The buffs never finished activating (found while closing the review)

The review of this branch asked for the ten visible buffs to be applied in the game and inspected in the buff UI.
Doing that showed a further defect in the same eleven buffs. They were built without `FxOnStart` and `FxOnRemove`
links. Kingmaker's `Buff.Activate` loads `FxOnStart` without a null check, after the buff is already active. The
null reference is swallowed by the rule that applies the buff, so:

- the buff is on the character and its mechanics work;
- `BuffCollection.OnFactAdded` never runs, so the "buff added" notification is not raised and the buff **never
  appears in the party bar**;
- expiry scheduling is not updated, so a buff that ends itself at the next round can linger. On the published
  v0.0.189, `Lightning Reload Used` was still on the character 25 s after play resumed.

The eleven buffs now get the empty effect links the project's other effect-free buffs already use
(`BlueprintUnitFactAccess.UseEmptyBuffEffects`). An empty link loads nothing and spawns nothing. With them,
activation completes, all ten visible buffs appear in the party bar with their icons, and `Lightning Reload Used`
ends 6.0 s after play resumes, which is one round.

Other project buffs are built the same way and are **not changed here**: Deadeye Armed, Gunslinger's Dodge, the four
Bleeding Wound bleeds applied to a target, Startling Shot, Targeting (Head), the Pistolero and Musket Master armed
buffs, and one buff each in the Eastern Weapons, Elven Branched Spear and Expanded Summoning (Suggestion) builders.
That inference comes from their source, which omits the links; none of them was applied or observed. Gunslinger's
Dodge in particular has its own expiry guard built around the existing behaviour, so it needs its own change and
test.

## Review follow-ups

The review of PR #161 at `42d8230b4c` found no blocking defect and listed these, all done on this branch:

- **Journal before write.** `ProtectionFromAlignmentPublication` changed a description and only then recorded the
  change for rollback. `SetDescription` stores the field before it verifies it, so a failure inside it left a changed
  description that rollback was never told about. The publication now records the intended mutation first, for
  descriptions and component arrays. A recorded entry that was never written is seen by rollback as still original and
  left alone. This ordering predates the branch. The new `protection-description-write-failure` launch throws after
  the field is stored and before the setter returns: the pre-change candidate `3ed36db218f0` left that description
  changed, and the new candidate restores all fifteen facts exactly while the rest of initialization continues.
- **Capture completeness.** The rollback-residue check now fails if any fact a session asked to have recorded was not
  recorded, instead of silently judging fewer entries.
- **Wording.** The Bleeding Wound armed descriptions say "ongoing bleed damage to hit points" (or to Strength,
  Dexterity, Constitution) in place of "recurring Hit Points bleed".
- **Class screen.** See the evidence below.
- **Buff UI.** See section 5 and the evidence below.

## What did not change

Blueprint GUIDs, symbols and internal names; grants; grit costs; stacking; durations; when a marker is consumed. The
buffs keep the icons the existing name-based assignment already gave them (`deeds`, and `reload-firearm` for Lightning
Reload); no art was added or changed. `BlueprintBootstrap.cs` is unchanged.

One behaviour does change as a consequence of section 5: the eleven buffs now complete activation, so the game
raises its normal "buff added" notification for them and schedules their expiry. Nothing else about when they are
applied, consumed or removed was altered.

## Why working installations did not fail

Established, on the published v0.0.189 DLL (`f963c615…`) in the lab's disposable game copy:

| Condition | Unstored text resolves to | Initialization | Gunslinger in class catalog |
|---|---|---|---|
| Stock game, `enGB` | `<null>` | completes | 1 |
| Stock game, `zhCN` (the game's own Chinese pack) | `<null>` | completes | 1 |
| Twelve of the reported mods at the reported versions, `enGB` | not recorded (harness did not load) | completes | not recorded |
| Controlled: unstored text resolves to `""` | `""` | fails with the reported exception | 0 |

So the omission was tolerated because the engine's placeholder is not empty, and the locale identifier alone does not
remove that placeholder. The controlled row reproduces the reported exception text, the missing class and the
reported rollback error exactly; it does so with a test-only Harmony postfix on `LocalizedString.op_Implicit`.

Not established: what made unstored text resolve blank on the reporter's installation. The log does not show it.
The twelve shared mods are Bag of Tricks 1.16.4, Better Vendors 2.0.8, Call of the Wild 1.14.4c-2.1, Cheat Menu 1.2.3,
Craft Magic Items 2.1.0, EddicKingmakerRespec 1.0, Kingmaker Bugfixes 1.3.3, Proper Flanking 2 1.2.3-2.1, Races
Unleashed 1.0.11, Skip Intro 1.3.1, Tweak or Treat 1.1.0 and Z Favored Class 1.3.1. The remaining candidates are the
other thirty-eight reported mods, the Italian translation (which may replace game files), and Unity Mod Manager
0.33.3 against the lab's 0.33.0. None of these was available here, and the reporter's setup was not reproduced.
The repair does not depend on the answer: no visible project fact in the traversal is left without stored text, and
the check no longer treats any stand-in as text.

## Evidence

All game launches ran in `C:\Dev\KingmakerGunslingerLab\game-copies\loader-compat` through
`scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1`, loader profile `owner-0.33.0`, with saves and
the area cache redirected to a per-session profile. Every session reported the owner's saves and installation
unchanged and the game's registry settings restored. Evidence root:
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\loader-compat\`.

Artifacts:

| | Baseline | Repaired development candidate |
|---|---|---|
| Source | tag `v0.0.189` (`90bbfbb08d`) | commit `cb3d383178c32e7a4f6f6fbb0d7c79efbd3250dc` |
| Package SHA-256 | `23eccbe657548dca0decbf335eaed9785d6238e846987e1f1bb20a2e1ac8013d` | `c5d3913ad9e68219f2c65ca600ff65302ffbd6719f220214bb6f3bc2a0c7febc` |
| DLL SHA-256 (as loaded in process) | `f963c6158bbd8b58dda7baba29644889d53adc055757b08fb266879f5aa12e1d` | `39a6ca1d0d314dd6a3cc1af6124c1f4730ccf3355f2f96959b8f7875a7c8d77f` |

An earlier candidate, `3ed36db218f0` (DLL `ee0504aa…`), passed the first two sessions before the review. It is
superseded by `cb3d383178c3`, which adds the review follow-ups and section 5; every session below was run again on the
new candidate.

Session `gunslinger-missing-class.json`, same launches on both:

| Launch | Baseline `20261115T040915Z` | Repaired `20261115T062258Z` |
|---|---|---|
| `stock-startup` | FAIL: 11 visible project buffs with placeholder text; product validator accepted 5 incomplete controls; panel shows `0.0.154` | PASS: 0 of 147 visible project facts unassigned; 10 controls correct; panel shows `Mr. Bard's Tabletop Rules Expansion 0.0.189` and `Content: initialized.` |
| `unassigned-text-blank` | FAIL: `initialize.root-cause` = the reported exception; class published 0 times; `rollback-failed` logged | PASS: `initialize.complete`; class published once; census and controls clean |
| `unassigned-text-blank-creation` (`disposable-gunslinger-creation-commit`) | FAIL | PASS: native creation selected and committed a Gunslinger (5 of 5 assertions) |
| `rollback-after-injected-failure` | FAIL: 16 owned mutations left behind; `rollback-failed` | PASS: 18 of 20 restored exactly, including the blank original; both foreign changes preserved; `rollback.incomplete` names the two; panel shows `Content: initialization failed.` |
| `rollback-native-state` | FAIL: 18 owned mutations left behind on an otherwise stock game | PASS: 20 of 20 restored exactly; `rollback.complete` |
| `locale-zhCN` | FAIL: same 11 buffs (initialization itself completes) | PASS |

In the baseline `unassigned-text-blank` launch the validator contract step could not run, because the harness borrowed
a class icon that is absent after a failed start. That harness defect was fixed before the repaired runs. The baseline
session was not repeated for it.

Session `gunslinger-missing-class-screens.json`:

| Launch | Baseline `20261115T063118Z` | Repaired `20261115T061857Z` |
|---|---|---|
| `chargen-class-screen` (unassigned text resolves blank) | FAIL: initialization fails with the reported exception; the class screen lists the Gunslinger 0 times | PASS: the New Character class screen lists 24 classes, the Gunslinger once, labelled `Gunslinger`; its own toggle selects it; Next moves on to the Heritage screen with it selected; the product logs `chargen-selector-result count=25,byReference=1,byGuid=1` |
| `buff-ui` | FAIL: activation of all ten incomplete; 0 buff icons in the party bar; the character sheet draws every name and tooltip as `<null>`; `Lightning Reload Used` still present after 25 s | PASS: activation complete; all ten in the party bar with their icons; the character sheet draws the ten names, tooltip text and icons; `Lightning Reload Used` ends after 6.0 s |
| `protection-description-write-failure` | FAIL: `ProtectionFromLaw` description left changed | PASS: all fifteen facts restored exactly; initialization completes; `publication.failed` logged |

The class screen is opened the way the New Game window opens it for a custom character
(`IUIStartChargen.HandleStartChargen(null)`), and the verdicts come from the game's own view objects: the selector
items the screen built, the party bar's buff widgets and the character sheet's buff rows. Each launch also writes
screenshots beside its result (`chargen-class-list.png`, `chargen-class-selected.png`, `chargen-after-class.png`,
`buffs-party-bar.png`, `buffs-character-sheet.png`, `buffs-character-sheet-end.png`). I looked at the class-selected,
party-bar and character-sheet pictures from the repaired runs: they show the Gunslinger selected with its description
and progression, the buff icons beside the portrait, and the ten named rows under Effects and Conditions.

Session `gunslinger-missing-class-shared-mods.json` (twelve of the reported mods at the reported versions), repaired
candidate, `20261115T062915Z`: both launches PASS; 2686 facts visited, 151 visible project facts, none unassigned,
with and without the blank condition. On the published package the harness of the day could not load beside
Kingmaker Bugfixes (`20261115T044745Z`, launch timeouts), so that run has no census; its loader log shows
`initialize.complete`, which is the source of the third row in the table under "Why working installations did not
fail". The harness now reads its request without process-wide JSON defaults.

Mechanics recorded in the census for the eleven buffs are identical on both packages: buff type, `IsClassFeature`,
`Stacking`, component count and icon.

Offline, at `cb3d383178c3`:

| Check | Result |
|---|---|
| `Invoke-KmgGate.ps1 -Filter 'armed-presentation;protection-alignment'` | production compiled; 21 of 2991 selected, 0 failures; 21 s |
| Complete domain suite (`test-domain.ps1`) | 2991 of 2991, 0 failures; 66 s |
| `Build-Local.ps1 -DevelopmentPurpose gunslinger-missing-class-repair` | development validation PASS, 67 selected tests, strict package validation PASS |
| `tools/validate_icon_catalog.py` | PASS |

The engine-free domain tests (`armed-presentation.*`) cover the text-assignment decision, the exact-restore rollback
and the status lines. They cannot fail on the old builders, which need the engine; the pre-fix failure of the builders
is the baseline column above.

## Limits and remaining checks

- The reporter's installation, translation and full mod list were not reproduced. The blank-resolution condition is a
  controlled stand-in.
- The lab loader is Unity Mod Manager 0.33.0; the report used 0.33.3.
- The ten buffs were applied directly to a level 1 Fighter in a fixture save. They were not armed through their
  abilities by a Gunslinger or spent on an attack, so the interaction of the completed activation with real play was
  not observed beyond `Lightning Reload Used` ending at the next round.
- The other buffs listed in section 5 are unchanged and untested.
- This is a development candidate built against the released version number. A release needs its own version, release
  validator pins and the normal release process, including the complete suite and package qualification on the
  release build.
