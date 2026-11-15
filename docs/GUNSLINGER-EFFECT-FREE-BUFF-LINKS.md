# Effect links for the remaining deed and named-weapon buffs (DEF-02, first cut)

Status: **development repair on a review branch. Not merged, not released.** Branch
`codex/def02-buff-effect-links`, from master `a7c34718f4`. The published v0.0.191 does not contain it and is
unchanged. Nothing here is a release qualification.

This is the first bounded cut of the Gunslinger remediation epic of 2026-11-15: inventory record DEF-02 (story
S2 T1), with the bounded inspection of RISK-07 (S2 T3). Current status for these records is kept in
`planning/GUNSLINGER-REMEDIATION-STATUS.md`.

## What was wrong

Fourteen project buffs were built without `FxOnStart` and `FxOnRemove` links. Kingmaker's `Buff.Activate` loads
`FxOnStart` with no null check, after the buff is already attached and its components are on. The rule that applies
the buff swallows the error, so:

- the buff is on the unit and its components work;
- `AddBuff` hands nothing back to its caller;
- the game's "buff added" notice is not raised, so the party bar does not show it;
- the buff collection does not reschedule, so the buff's end and its per-round tick do not happen until some other buff
  on the same unit is added or removed.

The eleven buffs behind the first missing-class report were repaired in v0.0.190 and the eight named Elven Branched
Spear buffs in v0.0.191. These fourteen were left, known only from their source.

## What changed

Each of the eight construction sites now calls the existing `BlueprintUnitFactAccess.UseEmptyBuffEffects`, which fills
an absent link with an empty one and leaves an assigned link alone. An empty link loads nothing and spawns nothing.

| Family | Buffs | Site |
|---|---|---|
| Deadeye | Deadeye Armed | `DeadeyeBlueprints.CreateArmedBuff` |
| Dodge | Gunslinger's Dodge | `GunslingerDodgeBlueprints.CreateArmorClassBuff` |
| Bleeding Wound | the four target bleeds | `BleedingWoundBlueprints.CreateBleed` |
| Startling Shot | Startled | `StartlingShotBlueprints.CreateFlatFootedBuff` |
| Targeting | Targeted Head | `TargetingHeadBlueprints.CreateBuff` |
| Pistolero | Up Close and Deadly Armed | `PistoleroBlueprints.CreateUpCloseArmed` |
| Musket Master | Steady Aim Armed | `MusketMasterBlueprints.CreateSteadyArmed` |
| Eastern Weapons | Wayfarer's Oath shell, Falling Petal, Moonlit Crossing shell, Mountain-Sunder round marker | `EasternWeaponNamedBlueprints.Buff` |

Unchanged: GUIDs, symbols, internal names, displayed text, components, stacking, visibility, durations, targeting,
immunity, costs and when grit is spent. Dodge's expiry guard is kept. `BlueprintBootstrap.cs` and the icon catalog are
untouched. The spear repairs and icon assignments from PRs #164 and #165 and the grit UI recipient contract from
PR #166 are untouched.

One consequence: these buffs now complete activation, so the game announces them and ends them on time. Where a deed's
text says an effect lasts one round, it now does.

### A second defect found on the way: Deadeye never armed

Arming Deadeye through its real ability showed that it spent grit and applied nothing, on the published v0.0.191 as
well. The ability applies Deadeye Armed with the native apply-buff action, and that action's duration named a bonus
but no dice count. Kingmaker evaluates both (`ContextValueHelper.CalculateDiceValue`), so the duration could not be
calculated; the game's action list catches the fault and carries on. Gunslinger's Dodge builds the same action with
the complete value and a comment that says why.

`DeadeyeBlueprints.CreateAbility` now gives the duration its dice type and count. This is a separate commit
(`596285cb0d`) so it can be reviewed, or left out, on its own. Cost, action type, text and the consuming shot are
unchanged. With it, Deadeye does what its description has always said.

### Fixture and observer changes

- `disposable-gunslinger-dodge` drives the ability's queued execution process before reading its effects, and asserts
  `dodge-native-apply-completed`. In the loader-compat copy the scenario still errors; see Dodge below.
- The loader-compat harness (test only, not in the product) gains `kmgBuffCensus` and `kmgDeedLifecycle`, and session
  `effect-free-buff-lifecycles.json`. The timed-buff probe also records Initiative.

## Coverage ledger

Baseline is the published v0.0.191 package. Candidate is the branch build named under Evidence. "Notice" is the game's
own buff-added event, which is what draws the party bar. Times are game seconds in real-time play.

| Buff (GUID prefix) | Final blueprint: role | Baseline observed | Change | Real trigger on candidate | Result | Remaining limit |
|---|---|---|---|---|---|---|
| Deadeye Armed (`88ca4220`) | visible armed state, no icon | links absent; real ability spent 1 grit and applied nothing | links; duration completed | Deadeye ability, then a pistol shot; then armed and left | arms, notice 1, 6.0 s left; shot consumes it, no further grit; unused state ends at 6.03 s, no refund | no icon (blank square); can be re-armed while armed for another grit; touch-AC effect not judged here |
| Gunslinger's Dodge (`bbd7d421`) | visible +2 dodge AC | links absent; works, but no notice, not marked not-dispelable, ended by the expiry guard | links | Dodge ability on a level 1 gunslinger with a hostile attacking | see Dodge below: PASS | product scenario still errors in this environment (fixture) |
| Bleed, hit points (`7fcfa24d`) | visible, permanent until removed, no icon | links absent; on target, no notice, 0 ticks in 15.6 s | links | Bleeding Wound ability, then a pistol hit; Stop Bleeding | notice 1, 1 grit, ticks at 6.0 and 12.0 s for 4 each (Dexterity bonus); Stop Bleeding removed it, 0 ticks in the next 58 s, damage 16 before and after | no icon |
| Bleed, Strength (`fe07b116`) | as above | links absent; on target, no notice, 0 ticks in 14.6 s | links | as above | notice 1, 2 grit, ticks at 6.0 and 12.0 s for 1 each | **cure not exercised**: the Stop Bleeding command did not start (fixture); no icon |
| Bleed, Dexterity (`9252d615`) | as above | links absent; on target, no notice, 0 ticks in 13.6 s | links | as above | notice 1, 2 grit, ticks at 6.0 and 12.0 s for 1 each; Stop Bleeding removed it, 0 ticks in the next 32 s, damage 6 before and after | no icon |
| Bleed, Constitution (`de789d99`) | as above | links absent; on target, no notice, 0 ticks in 12.6 s | links | as above | notice 1, 2 grit, ticks at 6.0 and 12.0 s for 1 each | **cure not exercised**, as Strength; no icon |
| Startled (`816825ff`) | visible condition on the target | links absent; on target, no notice, did not end in 16 s, target still flat-footed | links | Startling Shot ability on a hostile | notice 1, no grit spent, target loses Dexterity to AC, ends at 6.03 s, condition gone | none found |
| Targeted Head (`1a99dc0d`) | visible condition on the target | links absent; direct application: activation incomplete, not in the party bar | links | Targeting (Head) ability, hit on a hostile | notice 1, 1 grit, target confused, ends at 6.0 s, condition gone | real trigger not run on baseline |
| Up Close and Deadly Armed (`b881cfed`) | visible armed state | links absent; direct application: activation incomplete, not in the party bar | links | Up Close and Deadly ability on a Pistolero, then a pistol hit; then armed and left | notice 1, no grit at arming, refused while armed; hit consumes it and spends exactly 1; unused state ends at 6.03 s, no charge | damage not judged (intermittent `up-close-critical` assertion not run); real trigger not run on baseline |
| Steady Aim Armed (`f1457546`) | visible armed state | links absent; direct application: activation incomplete, not in the party bar | links | Steady Aim ability on a Musket Master, then a musket hit; then armed and left | arms, notice 1, no grit, refused while armed; ends at 6.02 to 6.03 s. **The shot does not consume it** | separate defect in the deed's rule hook, not repaired; see Findings |
| Falling Petal (`921d9f64`) | visible +1 dodge AC, no icon | links absent | links | confirmed critical hit with Falling Petal | ordinary hit grants nothing; critical grants it, notice 1, AC 10 to 11, ends at 6.01 s, AC 10; with another weapon in hand AC is 10 while the buff remains | no icon; real trigger not run on baseline |
| Mountain-Sunder round marker (`4ca1d8e2`) | hidden marker | links absent; first hit applies force damage, marker never ends, next-round hit gets none | links | Power Attack hits with Mountain-Sunder | first hit 1 application, second hit in the round none, marker hidden, ends at 6.0 s, next-round hit applies again | none found |
| Wayfarer's Oath shell (`2fd1feb6`) | hidden, no components; retained identity | links absent | links | none: nothing applies it (the bonus lives on a separate feature) | direct application only: activates, not in the party bar, AC, speed, Reflex and Initiative unchanged while on, ends at 6.0 s | no old-save fixture carrying the shell was found; not loaded from a save |
| Moonlit Crossing shell (`596a9b06`) | hidden, no components; retained identity | links absent | links | none, as above | as above | as above |
| Suggestion (`6635bf9b`), RISK-07 | provider-bound | provider absent: links present (empty) | none | not run | provider-present blueprint not read | blocked: provider not on the lab machine; see below |

Direct application on the fixture save's main character (candidate): all ten applied buffs activated (`AddBuff` handed
the buff back), the seven visible ones were in the party bar with their blueprint's icon or none, the three hidden
ones were not, and all ended at 6.0 s with armor class, speed, Reflex and Initiative back at their earlier values. On
the baseline the five that were tried activated incompletely and none was in the party bar; they ended at 6.0 s there
only because Dodge's expiry guard removed Dodge, which reschedules every buff on that unit.

## Gunslinger's Dodge

**The real activation does not go through `GunslingerDodgeProneAbilityLogic.Deliver`.** The registered ability's
components are `AbilityResourceLogic`, `DodgeGritCostCalculator`, `AbilityCasterHasNoFacts` and
`AbilityEffectRunAction` (read from the final blueprint in the game), and a domain test pins that the builder does not
create the custom logic. A real activation leaves the product's delivery diagnostic at `deliverEntered=false`. The
buff is applied by the native apply-buff action. The class file still holds the live cost calculator; its custom
delivery and `RecordDelivery*` diagnostics are dead code, like the pre-AC reaction diagnostic (INT-03). Nothing was
wired up to satisfy either.

**The earlier failure is the fixture's.** `disposable-gunslinger-dodge` runs from the main menu on a unit that is in
no loaded area. Kingmaker interrupts the ability command there before anything is cast, so there is no buff to read.
With the scenario now driving the execution process, it stops at that cause: "Native Dodge command produced no
execution process; result=Interrupt". It still errors in the loader-compat copy, and its new assertion has not been
exercised. The product behaves correctly in a loaded game, on both builds:

| Observation (level 1 gunslinger, real ability, hostile attacker) | Published v0.0.191 | Candidate |
|---|---|---|
| Grit | 5 to 4 | 5 to 4 |
| Armor class | 14 to 16 | 14 to 16 |
| Incoming attack's target AC before, during, after | 14, 16, 14 (not flat-footed) | 14, 16, 14 (not flat-footed) |
| "Buff added" notice | **0** | 1 |
| Marked not-dispelable by the native action | **no** | yes |
| Ended after | 6.07 s, **removed by the expiry guard** | 6.0 s, by the game; guard removals 0 |
| Armor class afterwards; bonus turned on and off | 14; once each | 14; once each |
| Second use while active | refused, no grit spent | refused, no grit spent |
| With no grit | refused | refused |

Recast before an earlier deadline: the first activation was removed by the fixture after 2.0 s (standing in for an
early end), and the ability was used again 2.5 s after the first. When the first activation's deadline passed (6.6 s)
the second was still on the gunslinger, the same instance, with 1.97 s left and AC 16. It ended 6.03 s after its own
start and AC returned to 14. On the candidate the guard removed nothing in the whole sequence; on the published
package it removed each activation at that activation's own deadline. The guard is stateless (it compares the current
buff's own end time), so it has nothing owed to an earlier instance. It is kept: with the links it no longer acts, and
no conflict was observed.

## Findings beyond effect links

Recorded in `KNOWN-ISSUES.md`. None is repaired here except the first.

1. **Deadeye never armed** (published v0.0.191). Repaired on this branch, as above.
2. **Steady Aim is never consumed by a shot and never grants its 10 feet.** `SteadyAimAttackHandler` reads
   `RuleAttackWithWeapon.AttackRoll` in `OnEventAboutToTrigger`; Kingmaker creates the roll inside `OnTrigger`, so it
   is null there and the handler returns. Moving the hook is a firearm-rules change with its own risks (probe rolls,
   scatter volleys) and is left for its own cut.
3. **Deadeye can be re-armed while armed**, for another grit. Design question.
4. **Six visible buffs have no icon**: Deadeye Armed, the four bleeds, Falling Petal. Icon assignments need an art
   decision and an authorized catalog change.
5. **The product's deed scenarios do not use the real abilities.** They add the armed buffs directly and call the
   runtime hooks, on units with no view. That is why none of the above was seen before.

## Suggestion (RISK-07)

- The owned buff is created by `CreateBuffShell`, which assigns both links. Read in the game with the provider absent:
  start `empty`, remove `empty`, one component (`Sprint32SuggestionPresence`). The ability is unavailable there, as
  before; nothing was changed.
- With the provider present, `BindSuggestionProvider32` copies every field of the provider's buff into a staged buff
  and then into the owned one, so the final links are the provider's. The builder already requires the provider's
  components to equal the native Rainbow Pattern buff's; that native buff, read in the game, has a start effect
  (`396af91a...`) and an empty remove link.
- **Blocked:** CowWithHat Custom Spells 1.4.5 is not installed on the lab machine, is not in the disposable copy's
  mod snapshot and has no isolated reference root on disk, so the provider-present final blueprint was not read and
  the buff was not applied. No product patch was made. The suspicion is neither confirmed nor disproved.

## Evidence

All game launches ran in `C:\Dev\KingmakerGunslingerLab\game-copies\loader-compat` through
`scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1`, loader profile `owner-0.33.0` (Unity Mod Manager
0.33.0), Kingmaker 2.1.7b, no other mods, real-time mode, fixture save `KMCS_AFTER_NORMAL_012_MODONLY.zks`, saves and
area cache redirected to a per-session profile. Every session reported the owner's saves (95 files) and installation
unchanged and the registry settings restored. Evidence root:
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\loader-compat\`.

| | Baseline | First candidate | Candidate |
|---|---|---|---|
| Source | tag `v0.0.191` (`3e13965d9d`) | `5e4bbd3838ac` | `96dbf78fac0c` |
| Contains | published release | links, first Dodge scenario change | plus the Deadeye duration and the corrected Dodge assertion |
| Package SHA-256 | `c09ca6255feefe6c547d051ab3cf3ead8bc918402d7488f077efa0ac5d2e7fbb` | `119bc70ce503386ffc0f4ce1fb71cef53c9fd521519a828dd17591f2fd35d9ef` | `3484df4947e0dff0ddc6a525ef2a3b52a256c3772a3182bb8df920cd2452596b` |
| DLL SHA-256 (as loaded) | `7c3b0324f40e32b0a476c24e9e66a63a2ecefd8908dc3902bed8ec81a5d87287` | `4acc7503a2c55d8ec70ad05f20ddbefe44447a1dd778fec0b592e5435011c27e` | `ed64e2a4de9264eab9911e1a9212fd89c9ca7fe3304c527eb918cb6f863757fa` |

Commands, in order, with wall-clock time:

| # | Command | Time | Result |
|---|---|---|---|
| 1 | `Invoke-KmgGate.ps1 -Filter 'dodge;deadeye;bleeding-wound;startling;targeting;pistolero;musket-master;eastern-weapons;armed-presentation;grit-ui-recipients;gunslinger-deed'` | 18 s | production compiled; 71 of 2,993 selected, 0 failures |
| 2 | `Build-Local.ps1 -DevelopmentPurpose def02-buff-effect-links -DevelopmentCompleteSuite` at `5e4bbd3838ac` | 470 s | complete suite 2,993 of 2,993, 0 failures; development validation and strict package validation PASS |
| 3 | Session `deed-lifecycle-shakeout` (scratch; every script in one launch) on the first candidate, `20261115T195057Z` | 258 s | observer shake-out. Showed the Deadeye and Steady Aim defects, the stale Dodge delivery assumption and three observer faults |
| 4 | Gate as in 1, plus `loader-compat`, after the corrections | 16 s | production compiled; 71 selected, 0 failures |
| 5 | `Build-Local.ps1 -DevelopmentPurpose def02-buff-effect-links -DevelopmentTestFilter <as 4>` at `96dbf78fac0c` | 421 s | 71 selected, 0 failures; development validation and strict package validation PASS |
| 6 | Session `effect-free-buff-lifecycles` on the candidate, `20261115T201300Z` | 331 s | 13 harness steps: 9 PASS, 4 FAIL (below); product Dodge scenario ERROR (fixture) |
| 7 | Session `effect-free-buff-lifecycles-published-baseline` (first launch trimmed to seven steps) on the published package, `20261115T202009Z` | 157 s | FAIL by design, as in the ledger |

Total 1,671 s (27.9 min). The complete suite was run once, at the first candidate, because many domain tests read the
changed builder files as text. `tools/validate_icon_catalog.py` PASS.

The four failing steps of run 6:

| Step | Failure | Reading |
|---|---|---|
| `direct-visible-without-icons` | Deadeye Armed and Falling Petal draw a blank square | finding 4; everything else in the step is correct |
| `deadeye` | "the ability can be used again while armed" | the probe assumed a rule the ability does not have (finding 3). Every other observation in the step is correct. The probe now asserts this only where the step says the ability refuses; this is a reader-side correction of recorded observations, and the step was not run again |
| `steady-aim` | not consumed by the eligible shot | finding 2 |
| `bleeding-wound` | Stop Bleeding did not start for the Strength and Constitution bleeds | fixture: a second standard-action command issued while the previous one was finishing. Applying and ticking passed for all four; the cure passed for hit points and Dexterity. The probe now waits for the command and clears cooldowns; not run again |

Harness identity: runs 3 and 6 used the harness as committed at `96dbf78fac0c` (run 3 before its corrections; DLL for
run 6 `7f34297db582d7a4a498d4fe156c6fe5cc482e3b022c1e698e50c398ae62bd5e`). Run 7 used the harness with the two probe
changes just described (DLL `3cae88f168c8a4b6e44eec405ae2a64fa76e6a0b46d51b48a3a2b54442536465`), which is the branch
head's harness. No file under `src/`, `assets/`, `assets-source/` or `blueprints/` changed after `96dbf78fac0c`, so
the candidate's product bytes are the branch's product source.

How the real triggers were driven, and what stands in for play:

- The gunslingers are disposable copies of the default player character, spawned beside the party and levelled through
  Kingmaker's own level-up controller (level 1; 7 for the conditions; 11 for Bleeding Wound; Pistolero and Musket
  Master archetypes added through the controller). They were not created through the character screens.
- Abilities were used with the game's ability command (`UnitUseAbility`) and ran through the game's command and
  ability controllers with time unpaused.
- Weapon attacks are the game's `RuleAttackWithWeapon` with the equipped weapon, triggered directly, not ordered
  through the attack command. Firearms were given one loaded round through the product's item-state service and a
  natural 15 through the product's forced-roll queue with the native generator seeded to match; Eastern weapons rolled
  from a seeded native generator. Attack bonus was raised to 100 so that a hit does not depend on armor class.
- Bleed targets were friendly disposable units standing within touch range.
- The fixture removed one Dodge activation early, as described.

## Limits and remaining checks

Stopped at the ordinary validation ceiling. Not run:

1. **Stop Bleeding on the Strength and Constitution bleeds**, and the bleed step as a whole with the corrected probe:
   one launch of the `bleeding-wound` step of `effect-free-buff-lifecycles.json` on the candidate (about 2 minutes).
2. The `deadeye` step again with the corrected expectation (its observations are already complete).
3. Real triggers on the published baseline for Up Close and Deadly, Steady Aim, Targeting (Head) and Falling Petal.
   Their baseline rests on the absent links, on direct application for three of them and on the shared mechanism.
4. Turn-based mode, a controller, save and load with any of these buffs on a unit, and any mod stack other than this
   mod alone on Unity Mod Manager 0.33.0.
5. The two retained shells in an old save. No admitted fixture that carries them was found; none was made.
6. Suggestion with its provider.
7. `disposable-gunslinger-dodge` to a verdict. It needs a loaded area, or a different way to run the command.
8. Deadeye's touch-AC benefit and range behaviour through the real ability. The repair makes the state arm and be
   consumed; what the consumed state does to the shot was not judged here.
9. Damage for Up Close and Deadly. The intermittent `up-close-critical` assertion was not investigated or run.

Still open and unchanged from the grit UI recipient record: the three hidden summoning abilities without a stored
description, and the uneven grit counter on deeds that need grit and spend none.

This is a development candidate built against the released version number. A release needs its own version, release
validator pins and the normal release process on the release build.
