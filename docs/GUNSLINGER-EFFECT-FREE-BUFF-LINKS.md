# Effect links for the remaining deed and named-weapon buffs (DEF-02, first cut)

Status: **development repair on a review branch. Not merged, not released.** Branch
`codex/def02-buff-effect-links`, from master `a7c34718f4`, draft PR #169. The published v0.0.191 does not contain it
and is unchanged. Nothing here is a release qualification.

This is the first bounded cut of the Gunslinger remediation epic of 2026-11-15: inventory record DEF-02 (story
S2 T1), with the bounded inspection of RISK-07 (S2 T3). Current status for these records is kept in
`planning/GUNSLINGER-REMEDIATION-STATUS.md`.

The record was revised after the review of PR #169 at `002ea2f708`. The review's corrections, the runs made for them
and the wording they asked for are marked "review follow-up" below. Earlier verdicts are kept as they were recorded.

**Two different claims, kept apart throughout:**

- *Effect-link repair*: all fourteen buffs have both links in their final blueprint, read in the game.
- *Lifecycle qualification*: what each buff does through its real trigger. This is complete for some identities and
  not for others; the table under "How far each identity is qualified" says which.

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
(`596285cb0d`) so it can be reviewed on its own. Cost, action type, text and the consuming shot are unchanged. With it,
Deadeye does what its description has always said.

### Fixture and observer changes

- `disposable-gunslinger-dodge` drives the ability's queued execution process before reading its effects, and asserts
  `dodge-native-apply-completed`. In the loader-compat copy the scenario still errors; see Dodge below.
- The loader-compat harness (test only, not in the product) gains `kmgBuffCensus` and `kmgDeedLifecycle`, and session
  `effect-free-buff-lifecycles.json`. The timed-buff probe also records Initiative.

### Review follow-up: how a lifecycle probe ends

The review found two faults in the harness. A `kmgDeedLifecycle` step that timed out advanced without ending its
probe, so the next step could resume it and file its result under the wrong label. And the probe's cleanup stopped at
the first exception and could still report success.

- The running probe now lives in a `ProbeSlot` (`tools/LoaderCompatHarness/ProbeLifecycle.cs`) under the identity of
  the step that started it. Only that step can poll it.
- It is terminated exactly once: when it reports a verdict, when it throws, when its step times out, and when a run
  ends with a probe still active. Termination is idempotent. The probe's record and how it ended are written under the
  step that started it.
- Each thing the probe owns is released on its own: the event subscription, the forced-roll queue (checked empty
  afterwards), the pause, every spawned unit and every blueprint clone (checked gone afterwards). One failing release
  does not stop the others.
- The probe's own failure and every cleanup failure are kept together. A cleanup failure fails the step. If isolation
  is not restored, the launch stops there whatever the step says about continuing, and no further probe can start.
- Other screen probes left behind by a step that timed out or threw are dropped, so a later step cannot resume them.
  They are otherwise unchanged.

Domain tests `loader-compat-probe.*` exercise the slot and the cleanup runner themselves, with stand-in probes: a
timed-out probe cannot supply the next probe's result; a probe that throws is ended and released; one failing release
does not stop the others; a cleanup failure cannot pass; and the next probe starts only after the previous one's
isolation is restored. The runner's use of the slot was also exercised in the game by a timeout drill (run 9).

Two checks were made stricter:

- **Dodge's recast case** now requires, before it judges anything: the earlier activation (command succeeded, delivery
  ended, its buff instance, one notice, one grit), the removal of that exact instance (one removed notice, armor class
  back), and the later activation (a new instance, one notice, one grit). If one is missing the case fails as "not
  judged".
- **The two retained shells** (and the hidden Mountain-Sunder marker, applied with them) must leave armor class, speed,
  Reflex and Initiative unchanged *while they are on*, as well as afterwards. The expectation is named by the step and
  is not applied to steps whose buffs grant a bonus.

## How far each identity is qualified

All fourteen: both links present in the final blueprint on the candidate (`empty`, `empty`), absent on the published
package. Beyond that:

| Buff | Qualified through its real trigger | Not qualified |
|---|---|---|
| Gunslinger's Dodge | complete for real-time play: grit, armor class against an incoming attack, notice, unaided end, recast isolation | the product's own scenario gives no verdict here |
| Bleeds: hit points, Strength, Dexterity, Constitution | complete for real-time play: application, recurring damage, Stop Bleeding, no later ticks, damage kept | icon |
| Startled, Targeted Head | complete for real-time play: application, condition, grit, unaided end | none found |
| Up Close and Deadly Armed | arming, grit at the right points, consumption by a hit, unaided end | damage; the known intermittent assertion |
| Mountain-Sunder round marker | complete for real-time play: hidden, blocks a repeat in the round, ends, next round applies | none found |
| Falling Petal | critical-hit grant, armor class, unaided end, weapon boundary | **icon check fails** (blank square) |
| Deadeye Armed | arming, grit, consumption by a shot, unaided end (with the duration fix) | **icon check fails**; the touch-AC benefit; re-arm policy is a design question |
| Steady Aim Armed | arming, notice, unaided end | **consumption check fails**: a shot does not consume it (separate defect); the range benefit |
| Wayfarer's Oath and Moonlit Crossing shells | nothing applies them; direct application only: inert, hidden, end cleanly | not loaded from an old save |

## Coverage ledger

Baseline is the published v0.0.191 package. Candidate is the branch build named under Evidence. "Notice" is the game's
own buff-added event, which is what draws the party bar. Times are game seconds in real-time play. "Unavailable"
means the game's own availability check for the ability (`AbilityData.IsAvailable`) said no; **no command was
attempted in those cases**, so they are availability observations and not observed refusals of an attempted use.

| Buff (GUID prefix) | Final blueprint: role | Baseline observed | Change | Real trigger on candidate | Result | Remaining limit |
|---|---|---|---|---|---|---|
| Deadeye Armed (`88ca4220`) | visible armed state, no icon | links absent; real ability spent 1 grit and applied nothing | links; duration completed | Deadeye ability, then a pistol shot; then armed and left | arms, notice 1, 6.0 s left; the ability still reports available while armed; shot consumes it, no further grit; unused state ends at 6.01 to 6.03 s, no refund | no icon (blank square: recorded failure); touch-AC effect not judged; re-arm policy open |
| Gunslinger's Dodge (`bbd7d421`) | visible +2 dodge AC | links absent; works, but no notice, not marked not-dispelable, ended by the expiry guard | links | Dodge ability on a level 1 gunslinger with a hostile attacking | see Dodge below: PASS | product scenario still errors in this environment (fixture) |
| Bleed, hit points (`7fcfa24d`) | visible, permanent until removed, no icon | links absent; on target, no notice, 0 ticks in 15.6 s | links | Bleeding Wound ability, then a pistol hit; Stop Bleeding | notice 1, 1 grit, ticks at 6.0 and 12.0 s for 4 each (Dexterity bonus); Stop Bleeding removed it; 0 ticks in the next 14.2 s; damage 12 before and after | no icon |
| Bleed, Strength (`fe07b116`) | as above | links absent; on target, no notice, 0 ticks in 14.6 s | links | as above | notice 1, 2 grit, ticks at 6.0 and 12.0 s for 1 each; Stop Bleeding removed it; 0 ticks in the next 12.1 s; Strength damage 3 before and after | no icon |
| Bleed, Dexterity (`9252d615`) | as above | links absent; on target, no notice, 0 ticks in 13.6 s | links | as above | notice 1, 2 grit, ticks at 6.0 and 12.0 s for 1 each; Stop Bleeding removed it; 0 ticks in the next 10.0 s; Dexterity damage 3 before and after | no icon |
| Bleed, Constitution (`de789d99`) | as above | links absent; on target, no notice, 0 ticks in 12.6 s | links | as above | notice 1, 2 grit, ticks at 6.0 and 12.0 s for 1 each; Stop Bleeding removed it; 0 ticks in the next 8.0 s; Constitution damage 3 before and after | no icon |
| Startled (`816825ff`) | visible condition on the target | links absent; on target, no notice, did not end in 16 s, target still flat-footed | links | Startling Shot ability on a hostile | notice 1, no grit spent, target loses Dexterity to AC, ends at 6.03 s, condition gone | none found |
| Targeted Head (`1a99dc0d`) | visible condition on the target | links absent; direct application: activation incomplete, not in the party bar | links | Targeting (Head) ability, hit on a hostile | notice 1, 1 grit, target confused, ends at 6.0 s, condition gone | real trigger not run on baseline |
| Up Close and Deadly Armed (`b881cfed`) | visible armed state | links absent; direct application: activation incomplete, not in the party bar | links | Up Close and Deadly ability on a Pistolero, then a pistol hit; then armed and left | notice 1, no grit at arming, ability unavailable while armed; hit consumes it and spends exactly 1; unused state ends at 6.03 s, no charge | damage not judged (intermittent `up-close-critical` assertion not run); real trigger not run on baseline |
| Steady Aim Armed (`f1457546`) | visible armed state | links absent; direct application: activation incomplete, not in the party bar | links | Steady Aim ability on a Musket Master, then a musket hit; then armed and left | arms, notice 1, no grit, ability unavailable while armed; ends at 6.02 to 6.03 s. **The shot does not consume it: recorded failure** | separate defect in the deed's rule hook, not repaired; see Findings |
| Falling Petal (`921d9f64`) | visible +1 dodge AC, no icon | links absent | links | confirmed critical hit with Falling Petal | ordinary hit grants nothing; critical grants it, notice 1, AC 10 to 11, ends at 6.01 s, AC 10; with another weapon in hand AC is 10 while the buff remains | no icon (blank square: recorded failure); real trigger not run on baseline |
| Mountain-Sunder round marker (`4ca1d8e2`) | hidden marker | links absent; first hit applies force damage, marker never ends, next-round hit gets none | links | Power Attack hits with Mountain-Sunder | first hit 1 application, second hit in the round none, marker hidden, ends at 6.0 s, next-round hit applies again | none found |
| Wayfarer's Oath shell (`2fd1feb6`) | hidden, no components; retained identity | links absent | links | none: nothing applies it (the bonus lives on a separate feature) | direct application only: activates, not in the party bar, AC 18, speed 20, Reflex 2 and Initiative 2 before, while on and after; ends at 6.0 s | no old-save fixture carrying the shell was found; not loaded from a save |
| Moonlit Crossing shell (`596a9b06`) | hidden, no components; retained identity | links absent | links | none, as above | as above | as above |
| Suggestion (`6635bf9b`), RISK-07 | provider-bound | provider absent: links present (empty) | none | not run | provider-present blueprint not read | blocked: provider not on the lab machine; see below |

The bleed figures are from run 9 (review follow-up), where all four cures completed. In run 6 the cures for Strength
and Constitution did not start; that run's figures for the other two (0 ticks in 58 s and 32 s after the cure) agree.
The hit-point figure includes 4 points from the pistol shot itself; the bleed inflicted 8.

Direct application on the fixture save's main character (candidate, run 6): all ten applied buffs activated (`AddBuff`
handed the buff back), the seven visible ones were in the party bar with their blueprint's icon or none, the three
hidden ones were not, and all ended at 6.0 s with armor class, speed, Reflex and Initiative back at their earlier
values. On the baseline the five that were tried activated incompletely and none was in the party bar; they ended at
6.0 s there only because Dodge's expiry guard removed Dodge, which reschedules every buff on that unit.

## Gunslinger's Dodge

**The real activation does not go through `GunslingerDodgeProneAbilityLogic.Deliver`.** The epic and the first prompt
for this cut assumed it did; that was mistaken. The registered ability's components are `AbilityResourceLogic`,
`DodgeGritCostCalculator`, `AbilityCasterHasNoFacts` and `AbilityEffectRunAction` (read from the final blueprint in the
game), and a domain test pins that the builder does not create the custom logic. A real activation leaves the
product's delivery diagnostic at `deliverEntered=false`. The buff is applied by the native apply-buff action. The
dead delivery component was not activated. Its class file also holds `DodgeGritCostCalculator`, which is live: it is
the cost calculator on the registered ability. Any later removal of the dead delivery and its `RecordDelivery*`
diagnostics (INT-03) must keep that calculator.

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
| Availability check while active | unavailable; grit unchanged | unavailable; grit unchanged |
| Availability check with no grit | unavailable | unavailable |

The last two rows are the game's availability check only. A use was not attempted while the buff was active or with
no grit, so what an attempted command would do in those states was not observed.

**Recast before an earlier deadline** (candidate, run 10, review follow-up, with the prerequisites now required):

| Step | Observed |
|---|---|
| Earlier activation | command succeeded, delivery ended, buff on the gunslinger, 1 notice, 1 grit |
| Early removal (by the fixture, standing in for an early end) | at 2.06 s; the instance on the unit was the earlier one; it is gone; 1 removed notice; AC 14 |
| Later activation | at 2.56 s; command succeeded, delivery ended; a new instance; 1 notice; 1 grit; 6.0 s left |
| When the earlier deadline has passed (6.62 s) | the later instance is still on the gunslinger, the same instance, 1.97 s left, AC 16 |
| End | the later instance ended 6.03 s after its own start; AC 14; no bonus modifier left; guard removals 0 |

Run 6 made the same observations without requiring the prerequisites, and run 7 on the published package showed the
later instance surviving the earlier deadline too, with the guard removing each activation at that activation's own
deadline. The guard is stateless (it compares the current buff's own end time), so it has nothing owed to an earlier
instance. It is kept: with the links it no longer acts, and no conflict was observed.

## Findings beyond effect links

Recorded in `KNOWN-ISSUES.md`. None is repaired here except the first.

1. **Deadeye never armed** (published v0.0.191). Repaired on this branch, as above.
2. **Steady Aim is never consumed by a shot and never grants its 10 feet.** `SteadyAimAttackHandler` reads
   `RuleAttackWithWeapon.AttackRoll` in `OnEventAboutToTrigger`; Kingmaker creates the roll inside `OnTrigger`, so it
   is null there and the handler returns. Moving the hook is a firearm-rules change with its own risks (probe rolls,
   scatter volleys) and is left for its own cut.
   Later repaired as its own cut on a branch stacked on this one: `docs/GUNSLINGER-STEADY-AIM-RANGE.md`. The
   verdicts recorded here are for this cut's candidate and stand as recorded.
3. **Deadeye reports itself available while Deadeye Armed is up.** That is the observation (availability check, twice,
   runs 6 and 9). What a second use would do was not run. From source: the registered ability has no "not already
   armed" restriction, its resource logic spends grit on every use, and the armed buff's stacking is Replace; so a
   second use is expected to spend another grit and replace the state. Design question; not changed.
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

Product packages:

| | Baseline | First candidate | Candidate |
|---|---|---|---|
| Source | tag `v0.0.191` (`3e13965d9d`) | `5e4bbd3838ac` | `96dbf78fac0c` |
| Contains | published release | links, first Dodge scenario change | plus the Deadeye duration and the corrected Dodge assertion |
| Package SHA-256 | `c09ca6255feefe6c547d051ab3cf3ead8bc918402d7488f077efa0ac5d2e7fbb` | `119bc70ce503386ffc0f4ce1fb71cef53c9fd521519a828dd17591f2fd35d9ef` | `3484df4947e0dff0ddc6a525ef2a3b52a256c3772a3182bb8df920cd2452596b` |
| DLL SHA-256 (as loaded) | `7c3b0324f40e32b0a476c24e9e66a63a2ecefd8908dc3902bed8ec81a5d87287` | `4acc7503a2c55d8ec70ad05f20ddbefe44447a1dd778fec0b592e5435011c27e` | `ed64e2a4de9264eab9911e1a9212fd89c9ca7fe3304c527eb918cb6f863757fa` |

No file under `src/`, `assets/`, `assets-source/` or `blueprints/` has changed since `96dbf78fac0c`, so the candidate
package was reused for the review follow-up and was not rebuilt.

Harness builds, which are separate from the product and differ between runs:

| Harness | Source | DLL SHA-256 | Used by |
|---|---|---|---|
| Exploratory | working tree before `96dbf78fac0c`; never committed in this form | not recorded | run 3 only |
| First committed | `96dbf78fac0c` | `7f34297db582d7a4a498d4fe156c6fe5cc482e3b022c1e698e50c398ae62bd5e` | run 6 |
| Re-arm rule named per step, cure optional | `edcbeff516` (run from the working tree, then committed unchanged) | `3cae88f168c8a4b6e44eec405ae2a64fa76e6a0b46d51b48a3a2b54442536465` | run 7 |
| Review follow-up: termination, cleanup, stricter checks | `ccb7912097` | `2b3b87e0294b0063e0e493f2fd2984682e1c839be711ec65710e0ba556faa49e` | run 9 |
| Review follow-up: notice counting in the Dodge recast check | `7319d293b4` | `4ff4d9205d6ddf79db43405cef286fab3e4aa9f454595e0c9ef53a2eb7a77852` | run 10 |

The exploratory harness of run 3 had three observer faults that were corrected before the first committed harness
(durations timed from the command instead of from the buff's own notices; a forced firearm roll that a native 1 could
still turn into a miss; bleed damage that included the shot's own). Run 3's verdicts are not relied on. It is what
showed the Deadeye and Steady Aim defects and the mistaken Dodge delivery assumption, each of which was then observed
again with a committed harness.

Commands, in order, with wall-clock time:

| # | Command | Time | Result |
|---|---|---|---|
| 1 | `Invoke-KmgGate.ps1 -Filter 'dodge;deadeye;bleeding-wound;startling;targeting;pistolero;musket-master;eastern-weapons;armed-presentation;grit-ui-recipients;gunslinger-deed'` | 18 s | production compiled; 71 of 2,993 selected, 0 failures |
| 2 | `Build-Local.ps1 -DevelopmentPurpose def02-buff-effect-links -DevelopmentCompleteSuite` at `5e4bbd3838ac` | 470 s | complete suite 2,993 of 2,993, 0 failures; development validation and strict package validation PASS |
| 3 | Session `deed-lifecycle-shakeout` (scratch; every script in one launch; exploratory harness) on the first candidate, `20261115T195057Z` | 258 s | exploratory; see above |
| 4 | Gate as in 1, plus `loader-compat`, after the corrections | 16 s | production compiled; 71 selected, 0 failures |
| 5 | `Build-Local.ps1 -DevelopmentPurpose def02-buff-effect-links -DevelopmentTestFilter <as 4>` at `96dbf78fac0c` | 421 s | 71 selected, 0 failures; development validation and strict package validation PASS |
| 6 | Session `effect-free-buff-lifecycles` on the candidate, `20261115T201300Z` | 331 s | 13 harness steps: 9 PASS, 4 FAIL (below); product Dodge scenario ERROR (fixture) |
| 7 | Session `effect-free-buff-lifecycles-published-baseline` (first launch trimmed to seven steps) on the published package, `20261115T202009Z` | 157 s | FAIL by design, as in the ledger |
| 8 | Review follow-up: `Invoke-KmgGate.ps1 -Filter 'loader-compat-probe;dodge;deadeye;bleeding-wound;eastern-weapons;grit-ui-recipients;gunslinger-deed'` | 6 s | production compiled; 46 of 2,998 selected, 0 failures, including the five new `loader-compat-probe.*` tests. A first run of the five alone failed one of them on an over-strict assertion in the test itself (a late call for an ended step); the test was corrected |
| 9 | Review follow-up: session `effect-free-buff-lifecycles-review-followup` (file SHA-256 `f5f56697ca3c807a0e56845bf1eb2b5695dfec71775fea648abd1139b1235849`) on the candidate, `20261115T210158Z` | 154 s | 5 harness steps: 3 PASS, 2 FAIL (the drill, by design; Dodge, on a fault in the new check) |
| 10 | Review follow-up: the Dodge step of that session alone (scratch copy, SHA-256 `23888bd0cdf0549c5f8cc2581891b9a0d22a647819354c1a06f0a61af0f1a7ad`) on the candidate, `20261115T210524Z` | 101 s | PASS |

Runs 1 to 7 took 1,671 s (27.9 min) and stopped at the ordinary ceiling. The review follow-up, runs 8 to 10, took
261 s (4.4 min). The complete suite was run once, at the first candidate, because many domain tests read the changed
builder files as text; it was not repeated. `tools/validate_icon_catalog.py` PASS.

### Run 6: the four failing steps

The verdicts stand as recorded. What each step also observed correctly is listed so that it is not lost.

| Step | Recorded failure | Reading | Observed correctly in the same step |
|---|---|---|---|
| `direct-visible-without-icons` | **FAIL**: Deadeye Armed and Falling Petal draw a blank square | finding 4; still fails | both activated, both were in the party bar, both ended at 6.0 s, stats unchanged afterwards |
| `steady-aim` | **FAIL**: not consumed by the eligible shot | finding 2; still fails | armed through the ability, 1 notice, no grit at arming or at the shot, unavailable while armed, both armed states ended after 6.02 to 6.03 s, a shot afterwards cost nothing |
| `deadeye` | **FAIL**: "the ability can be used again while armed" | the probe asserted a rule the ability does not have (finding 3). The original FAIL is kept | armed, 1 notice, grit 5 to 4, shot consumed it with no further grit, unused state ended at 6.03 s with no refund |
| `bleeding-wound` | **FAIL**: Stop Bleeding did not start for the Strength and Constitution bleeds | fixture: the next cure was issued while the previous command was still finishing | application and ticking for all four; cure, no later ticks and unchanged damage for hit points and Dexterity |

### Review follow-up runs

Run 9, on the same candidate package, with the harness at `ccb7912097`:

| Step | Verdict | What it shows |
|---|---|---|
| `timeout-drill` (the Mountain-Sunder script given four seconds) | **FAIL by design**: "step timeout after 4s" | the probe was cut off with its two units in the game. It was ended under its own step (`0|kmgDeedLifecycle_timeout-drill`), with its partial record; all seven releases were attempted (event subscription, forced roll, pause, two units, two blueprint clones), none failed, isolation restored. The four steps after it ran under their own identities with their own records |
| `direct-hidden` | PASS | the two shells and the Mountain-Sunder marker: AC 18, speed 20, Reflex 2, Initiative 2 before, while on and after; not in the party bar; ended at 6.0 s |
| `dodge` | **FAIL** | the new early-removal prerequisite counted removed notices "since a time" and took in the previous activation's end notice (2 instead of 1). The case was, as designed, not judged. Corrected at `7319d293b4`; see run 10 |
| `deadeye` | PASS | a new execution with the expectation that names the ability's re-arm rule: arms, 1 notice, grit 5 to 4, the ability reports available while armed (recorded, not asserted), shot consumes it, unused state ends at 6.01 s, no refund |
| `bleeding-wound` | PASS | all four bleeds: applied by an armed pistol hit (1 notice each; 1, 2, 2, 2 grit), ticks at 6.0 and 12.0 s, each Stop Bleeding command succeeded with its delivery ended and removed its bleed in 1.65 s with no grit, 0 ticks afterwards, damage unchanged (12, 3, 3, 3) |

Every probe in runs 9 and 10 ended with every release attempted and none failed.

Deadeye's corrected expectation, both ways: re-read against the corrected expectation, run 6's recorded observations
contain every asserted point and satisfy each of them (a re-evaluation of recorded observations, not a new run; run
6's FAIL is kept). Run 9 is a new execution and passed.

Run 10, the Dodge step alone with the harness at `7319d293b4`: PASS, as in the Dodge section.

### How the real triggers were driven, and what stands in for play

- The gunslingers are disposable copies of the default player character, spawned beside the party and levelled through
  Kingmaker's own level-up controller (level 1; 7 for the conditions; 11 for Bleeding Wound; Pistolero and Musket
  Master archetypes added through the controller). They were not created through the character screens.
- Abilities were used with the game's ability command (`UnitUseAbility`) and ran through the game's command and
  ability controllers with time unpaused.
- Weapon attacks are the game's `RuleAttackWithWeapon` with the equipped weapon, triggered directly, not ordered
  through the attack command. Firearms were given one loaded round through the product's item-state service and a
  natural 15 through the product's forced-roll queue with the native generator seeded to match; Eastern weapons rolled
  from a seeded native generator. Attack bonus was raised to 100 so that a hit does not depend on armor class.
- Bleed targets were friendly disposable units standing within touch range. Between cures the fixture clears the
  gunslinger's action cooldowns; each cure's command and delivery are complete before the next is issued.
- The fixture removed one Dodge activation early, as described.

## Limits and remaining checks

Not run:

1. Real triggers on the published baseline for Up Close and Deadly, Steady Aim, Targeting (Head) and Falling Petal.
   Their baseline rests on the absent links, on direct application for three of them and on the shared mechanism.
2. Turn-based mode, a controller, save and load with any of these buffs on a unit, and any mod stack other than this
   mod alone on Unity Mod Manager 0.33.0.
3. The two retained shells in an old save. No admitted fixture that carries them was found; none was made.
4. Suggestion with its provider.
5. `disposable-gunslinger-dodge` to a verdict. It needs a loaded area, or a different way to run the command.
6. Deadeye's touch-AC benefit and range behaviour through the real ability, and what a second use while armed
   actually does. The repair makes the state arm and be consumed; what the consumed state does to the shot was not
   judged here.
7. Damage for Up Close and Deadly. The intermittent `up-close-critical` assertion was not investigated or run.
8. What an attempted Dodge command does while the buff is active or with no grit (only availability was read).
9. The harness runner's timeout path was exercised in the game once (run 9) and its rules by the domain tests; a
   cleanup that actually fails in the game was not produced.

Explicit follow-ups, not part of this cut: Steady Aim's rule hook; the six missing icons; Suggestion with its
provider; the broader compatibility and save-and-load work; Deadeye's re-arm policy.

Still open and unchanged from the grit UI recipient record: the three hidden summoning abilities without a stored
description, and the uneven grit counter on deeds that need grit and spend none.

This is a development candidate built against the released version number. A release needs its own version, release
validator pins and the normal release process on the release build.
