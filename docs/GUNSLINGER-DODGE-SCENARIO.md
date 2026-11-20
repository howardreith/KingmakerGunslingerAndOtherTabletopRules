# Gunslinger's Dodge: the product scenario runs the real ability (INT-03)

Status: **development change on a review branch. Not merged, not released.** Branch `codex/int03-dodge-scenario`,
stacked on draft PR #170 at its reviewed head `1ceab99e6a` (which is stacked on draft PR #169). The published
v0.0.191 does not contain it. Nothing here is a release qualification.

This is one bounded cut of the Gunslinger remediation epic of 2026-11-15: story S3 T3, inventory record INT-03.
Current status is kept in `planning/GUNSLINGER-REMEDIATION-STATUS.md`. It changes no gameplay: the deed's ability,
buff, cost, duration, stacking, effect links and expiry guard are as they were.

The record was revised after the review of PR #171 at `f9d86391a3`. The three corrections, their tests and the new
run are under "Review corrections" below. The first candidate's run is kept as recorded.

The review of `7c4cb8ad44` accepted the three corrections, which closes INT-03 as a development work item: the
corrected scenario passed 12 of 12 on candidate `15767a89ad2b`, and the recast timing, the outer-abort evidence and
the exit requirement at admission are resolved. It is not merged and not released. NEW-06 is closed for Dodge only.

## The live path

The epic describes Dodge as activating through `GunslingerDodgeProneAbilityLogic.Deliver`. That was not so. The
registered ability (`KMG.Deeds.GunslingerDodgeProneAbility`) has four components:

| Component | What it does |
|---|---|
| `AbilityResourceLogic` | the native grit transaction, custom cost |
| `DodgeGritCostCalculator` | the cost: 1, or 0 with True Grit selected while grit is above 0 |
| `AbilityCasterHasNoFacts` | unavailable while the Dodge buff is already on the caster |
| `AbilityEffectRunAction` with `ContextActionApplyBuff` | applies the timed buff to the caster for one round |

It is a swift action. The buff's `GunslingerDodgeArmorClassBonus` adds a +2 dodge bonus to armor class while the
buff is on, and `GunslingerDodgeExpirationPatch` removes this one buff if it is still present after its own
deadline.

## What was wrong with the product's own scenario

`disposable-gunslinger-dodge` built its gunslinger at the main menu, in a scene of its own with no loaded area, and
ticked the ability command by hand. Kingmaker interrupts an ability command there before it acts, so no execution
process ever exists. The scenario could not reach an assertion; in the disposable game copy it ended in an error on
the published packages and on PR #169's candidate alike (`docs/GUNSLINGER-EFFECT-FREE-BUFF-LINKS.md`). Its text
also still spoke of a "reaction" and printed counters that nothing could move.

## What changed

**The scenario** is now one of the product's working-save scenarios, like its others that need a loaded game.

- The request must name `KMG_AUTOMATION_WORKING`. The product's existing route clicks the real Load Game button,
  checks the save's exact identity, requires the baseline save to be present and distinct, loads read-only and
  watches for any save write. A request without the save name, with the baseline or another save, or with anything
  extra is refused before launch (`scripts/Test-RuntimeScenarioPreflight.ps1`) and by the product's own request
  validation.
- Once the save is loaded the scenario runs one step per frame (`RuntimeTestRunner.GunslingerDodge.cs`), the same
  shape as `disposable-alternate-summoning-natural`. It refuses a fixture that is not a loaded area in the default
  real-time mode with the party out of combat.
- It spawns a disposable gunslinger into the loaded area, with a view, and levels it with the level-up controller.
  The ability it uses is the one that level-up granted. A disposable hostile unit supplies the incoming attack.
  Neither unit acts for itself.
- It issues `UnitUseAbility` through the unit's command queue with time running and waits, frame by frame, until
  the command has finished and its execution process has ended. Nothing ticks the command by hand, adds the buff
  directly or calls a delivery method.
- Its verdicts are in `GunslingerDodgeScenarioVerdict`, which has no engine types and is covered by domain tests.
  A command that ends before an execution process exists, a command that does not settle, a missing prerequisite
  and a flat-footed target are each a failure with its reason. A reason beginning "not judged:" is a failure.
- The scenario's own completion timeout ends it as `TIMEOUT` with the stage it was in.
- Every wait records whether its condition held or its real-time limit ran out, and `dodge-waits-reached` fails
  the scenario if any wait ran out (added in the review corrections).
- Cleanup releases each owned thing on its own (pause, notice subscription, level-up controller, both units, the
  blueprint prototypes), keeps every failure, and then compares the area's units and the party with what was there
  before. Any release failure, extra or missing unit, changed party, leftover Dodge modifier or observed save write
  fails `dodge-cleanup`. It runs whether the scenario finished, threw or timed out.

**Retired**, each after its references were traced:

| Removed | Why it was safe |
|---|---|
| `GunslingerDodgeProneAbilityLogic` (the custom delivery, its availability rule and its `RecordDelivery*` calls) | No blueprint builds it. The registered ability's four components are listed above; the builder's own validation requires exactly those four |
| `GunslingerDodgeRuntime` (`BeforeAttackRoll`, `AfterCalculateArmorClass`, `AfterAttackRoll`, the attack-frame stack) | Its three callers were removed from the attack-roll and armor-class patches on 2026-08-04 (`93193c5170`). Nothing has called it since |
| `GunslingerDodgeRuntimeDiagnostics` (applied, rejected, duplicate and fault counters, delivery evidence) | Written only by the two classes above; read only by the old scenario and, optionally, by the lifecycle probe |
| The old main-menu scenario body and its `dodge-native-reaction`, `dodge-no-legacy-reaction` and delivery-evidence output | Replaced by the scenario above |

**Kept:**

- `DodgeGritCostCalculator`, unchanged, now in `Deeds/DodgeGritCostCalculator.cs`. It is live on the registered
  ability.
- Every registered blueprint, including the legacy armed marker `KMG.Deeds.GunslingerDodgeProneArmed`, with its
  GUID. Saves that reference it still resolve.
- `GunslingerDodgeArmorClassBonus` and `GunslingerDodgeExpirationPatch`, unchanged.
- `GunslingerDodgeService` and its request, decision, mode, armor, load and status types: the tabletop reaction
  policy. It has no caller in the product (its only one was the removed runtime's unused field) and it has domain
  tests. It was left alone; whether to keep it is a separate decision.

**About the legacy-marker cleanup.** `GunslingerDodgeRuntime.BeforeAttackRoll` contained code to remove the armed
marker that version 0.0.65 persisted. It has not run since `93193c5170` removed the call, so there was no live
migration to preserve, and this cut removes the unreachable code without changing behavior. A character saved
under 0.0.65 with the marker still has it. The marker is a hidden feature with no components and does nothing.
Nothing was added to remove it.

**Also changed:**

- The save-free aggregate `disposable-gunslinger-comprehensive-acceptance` no longer has a Dodge slice (32 slices,
  was 33). The slice could not execute the ability at the main menu.
- The loader-compat session launcher passes a product launch's `timeouts` into the product's request. The product
  requires them for a scenario that loads a save through its own route.
- The lifecycle probe records the retired delivery diagnostic only on builds that still have it.

## Review corrections

The review of `f9d86391a3` accepted the direction and found three things the scenario could have let through. Each
was a finding in the source; none had occurred in the recorded run.

**1. The recast check did not prove the overlap it claimed.** It waited for a time 6.6 s after the earlier cast and
then read the unit, but the wait could also return because its 60 s real-time limit ran out, and nothing in the
verdict used the times. In the recorded run the wait did reach its time (the unit was read 6.61 s after the earlier
cast). Now:

- the earlier buff's own end time (`Buff.EndTime`) is read before the fixture removes it;
- the verdict requires the removal to be before that deadline, the later activation's buff-added notice to fall
  between the removal and that deadline, and the unit to be read at least 0.5 s of game time after it; the scenario
  waits for 0.6 s after it;
- a wait is a `DodgeWait`, which ends either on its condition or on its limit and keeps which; a wait that ran out
  is never "reached", and the recast is "not judged" unless its deadline wait was reached;
- the later instance must be the one on the unit at that reading, and must then end 6 s after its own start with
  armor class restored;
- the deadline and every measured time are in the assertion's evidence, in seconds of game time.

**2. An outer abort lost the scenario's cleanup evidence.** If the runner completed with a result of its own while
the scenario's steps were still running (a save-write guard failure, an exception in its own observation), ending
the steps ran the cleanup and appended its assertion to the scenario's list, but the result that was written had a
different list. Now the scenario's assertions and running steps live in one `DodgeScenarioSession`. Every result
the runner completes with passes through it:

- if the steps are still running, the abort is recorded with the stage, the steps are ended (which runs each
  release on its own), and an exception from ending them is recorded;
- the result keeps its own status, reason and assertions, and receives after them the scenario's observations so
  far, the abort, the cleanup result with each release failure, and the disposal exception if there was one;
- a result built on the scenario's own list receives nothing again; the steps are disposed once; a second call
  changes nothing;
- a result that was a pass is no longer one if the scenario was aborted.

**3. A request that does not exit was refused too late.** The scenario requires automatic exit, but the check was
in the scenario's first frame, after the save had loaded. Now `GunslingerDodgeScenarioAdmission` refuses a
`disposable-gunslinger-dodge` request with `exitAfterCompletion=false` in the product's request validation
(reason `gunslinger-dodge-exit-required`) and `Assert-KmgRuntimeScenarioPreflight` refuses it before launch. The
rule is Dodge's own; other scenarios' admission is unchanged.

### Tests for the corrections

Eight new domain tests (`dodge-scenario.*`), on the production types the scenario uses:

| Test | What it shows |
|---|---|
| `recast-timely-overlap-passes` | removal at +2, later cast at +2.5, read at +6.6, later end at its own +6.02 passes; the evidence text carries the deadline and each time |
| `recast-after-old-deadline-fails` | every activation succeeds but the later one is applied after the old deadline: "not judged", nothing overlapped. Also a removal after the deadline, and a deadline that was not captured |
| `recast-deadline-wait-must-reach-deadline` | the deadline wait runs out its limit before game time reaches the deadline and the later buff then ends normally: "not judged". Also a reading inside the margin, no wait at all, and the outcome failures once the overlap holds |
| `recast-prerequisites` (existing, updated) | each missing activation or removal alone is still "not judged" |
| `waits-record-why-they-ended` | a wait ends on its condition or on its limit; one that ran out stays unreached even if the condition holds later; any unreached wait fails the scenario |
| `outer-abort-keeps-evidence` | an outer failing result with its own list ends up holding its own failure and reason, the partial Dodge assertions, the abort with its stage and the cleanup result; the steps are disposed once; a second finalization changes nothing |
| `cleanup-failure-beside-abort-reason` | a release that throws and a cleanup that itself throws both appear beside the unchanged outer reason; the releases after the failing one were still attempted |
| `normal-completion-does-not-duplicate` | finalizing a result built on the scenario's own list, or a published copy of it, adds nothing; a scenario that never ran leaves another result alone |
| `admission-requires-automatic-exit` | Dodge without automatic exit is refused with the reason, with it is admitted, and no other scenario is affected |

The abort tests drive a stand-in for the scenario's steps of the same shape: an iterator that records assertions as
it goes and releases through `DodgeReleases` in its `finally`. The preflight script gained six checks (1,205 in
all): the non-exiting request is refused by `New-KmgRuntimeRequest` and by `Assert-KmgRuntimeScenarioPreflight`,
the refusal names its reason, the valid request carries automatic exit, and `working-save-smoke` still accepts a
non-exiting request.

The product's request validator itself cannot be compiled into the domain tests: it reaches two engine-bound
classes. Its refusal was therefore checked in the game (below).

## Focused tests

| Check | First candidate | After the review corrections |
|---|---|---|
| `.\scripts\Invoke-KmgGate.ps1 -Filter 'dodge;true-grit;third-playtest;loader-compat-probe;gunslinger-deed;grit-ui-recipients'` | production compiled; 40 of 3,007 selected, 0 failures (16 s) | production compiled; 48 of 3,015 selected, 0 failures (16 s) |
| `.\scripts\Test-RuntimeScenarioPreflight.ps1` | 1,199 checks passed (18 s) | 1,205 checks passed (21 s) |
| Candidate build's own selection | 66 of 3,007 selected, 0 failures | 74 of 3,015 selected, 0 failures |

Six tests came with the first candidate (`dodge-scenario.*`), on the scenario's verdicts with stand-in observations:

- an accepted activation; and that an interrupted command with no execution process, a command that never
  settles, an unfinished delivery and an unavailable ability are each rejected;
- the effects: one grit (and 0 when the cost is 0), exactly one new announced timed buff, the native action's mark,
  exactly +2;
- a refused attempt changes nothing, and an attempt on an available ability is "not judged" as a refusal;
- the incoming attack reads +2 during and the base value before and after; a flat-footed roll or a missing roll is
  "not judged";
- the unaided end after one round, removed once, bonus on once and off once;
- the recast: each of the three prerequisites alone makes it "not judged"; a later activation that is gone at the
  earlier deadline, is cut short, never ends or leaves its bonus behind fails.

Two existing source checks now read the live files (`third-playtest.dodge-no-prone`,
`true-grit.native-check-cost`). The True Grit cost contract is covered by `true-grit.*`; it was not exercised in the
game.

## Native verdict

Session `scripts/compatibility/loader-compat/sessions/gunslinger-dodge-scenario.json` in the disposable game copy,
loader profile `owner-0.33.0`. The product loaded `KMG_AUTOMATION_WORKING` through its own guarded route.

### After the review corrections: candidate `15767a89ad2b`

`disposable-gunslinger-dodge`: **PASS**, run `20261116T024032Z`, 75 s in the product, 12 of 12 assertions (the
twelfth is `dodge-waits-reached`). The result was published on the scenario's own list, once.

The observations match the first candidate's: grit 5 to 4, armor class 10 to 12, an incoming attack resolved
against 10, 12 and 10, the attempted command refused while active and with no grit with nothing spent or applied,
the unused buff gone after 6.00 s, no release failure, no stray unit, no save write.

Recast timing, in seconds of game time (`dodge-recast-isolation`):

| Event | Game time | Relative to the earlier cast |
|---|---|---|
| Earlier buff added | 44377.96 | 0 |
| Earlier buff's own deadline, read before removal | 44383.96 | +6.00 |
| Earlier instance removed by the fixture (the same instance, 1 notice, armor class 10) | 44379.97 | +2.01 |
| Later buff added (a different instance) | 44380.50 | +2.54, which is 3.46 s before the earlier deadline |
| Deadline wait | reached on its condition after 4.1 s of real time | |
| Unit read: the later instance on the unit, armor class 12 | 44384.58 | +6.62, which is 0.62 s after the earlier deadline |
| Later buff ended, armor class 10 | 44386.52 | 6.02 s after its own start |

The expiry guard removed nothing. All eleven waits of the run ended on their condition: the five commands in 0.02
to 0.05 s, the unused expiry in 5.97 s, the deadline wait in 4.1 s, the later end in 1.93 s.

**Refusal of a non-exiting request**, run `20261116T024238Z`: PASS. The product was started with a request that is
valid except for `exitAfterCompletion=false` (the same scenario, save name and timeouts as the accepted one). Its
log shows `[runtime-test][request.rejected] reason=gunslinger-dodge-exit-required` and no `request.accepted`; no
product evidence was written and no save was loaded. The harness, with no steps, exited from the main menu.

### First candidate `df7f11156a01`, kept as recorded

`disposable-gunslinger-dodge`: **PASS**, run `20261116T012451Z`, 78 s in the product, 11 of 11 assertions. Its
recast reading was taken 6.61 s after the earlier cast, so the overlap did hold in this run; the review finding is
about what the check would have accepted otherwise.

| Assertion | Observed |
|---|---|
| `dodge-fixture-admitted` | default mode, not turn-based, party out of combat, in-area anchor |
| `dodge-registered-ability` | granted by level-up; the four components above; grit 5 |
| `dodge-accepted-activation` | command `Success`, execution process present, delivery ended; grit 5 to 4; Dodge buffs 0 to 1, a new instance; 1 buff-added notice; armor class 10 to 12; marked by the native action; 6 s left |
| `dodge-incoming-attack` | the same incoming attack was resolved against 10 before, **12 during**, 10 after; the target was not flat-footed on any of the three |
| `dodge-refused-while-active` | read unavailable; an actual command ended `Fail` with no execution process; grit 4 to 4; the same single buff; armor class 12 to 12 |
| `dodge-unused-expiry` | ended by itself after 6.02 s of game time; 1 buff-removed notice; armor class back to 10; bonus on once, off once, none left; the expiry guard removed nothing |
| `dodge-recast-isolation` | earlier activation complete (grit 4 to 3, new instance); that exact instance removed at 2.03 s, 1 notice, armor class 10; later activation complete (grit 3 to 2, a different instance); at 6.61 s after the earlier cast the later instance was still on the unit at armor class 12; it ended 6.02 s after its own start, armor class 10; the guard removed nothing |
| `dodge-refused-without-grit` | grit 0: read unavailable; an actual command ended `Fail` with no execution process; grit 0 to 0; no buff; armor class 10 to 10 |
| `dodge-cleanup` | no release failure; no extra or missing unit; party unchanged; no Dodge modifier left; no save write |
| `exact-working-save`, `loaded-mod-version` | the stable expected fingerprint; 0.0.191 |

Queried and attempted, kept apart: the availability reading was taken in three states (available with grit,
unavailable while active, unavailable with no grit). A command was attempted in all three. The two attempts on an
unavailable ability were refused by the game's command at its action step, before any execution process; no
cancellation of a command that had started was attempted.

The lifecycle probe's Dodge step was run once on the same candidate with the adjusted harness
(`20261116T012700Z`): PASS.

## Identities

| Item | Identity |
|---|---|
| Product commits | `c014e5c999` (first candidate), then `15767a89ad` (review corrections); nothing else in the product after `1ceab99e6a` |
| First candidate | built once at `df7f11156a01`, 443 s; package SHA-256 `d2d41e0fb21fcea133cd0fcb0de4f56f97f0b0749c8352f80b377c2a019061c3`, DLL `3b28dcd4a74a12f03d6ba23419f45f8f7a85ccd85ea03345b2ae10b8e8cb3974` |
| Candidate after the review corrections | built once at `15767a89ad2b`, 406 s; package SHA-256 `462696cc2bd30874d89df251a7bdcb289080e6fb7c8eb79c2948a1c9a3e7eedc`, DLL `9519bb86b864480de2d21f3bd0330317d96dc5a879a451da614d3e02ff5fc5a3` |
| Scenario request | `disposable-gunslinger-dodge`, `saveName=KMG_AUTOMATION_WORKING`, completion timeout 240 s; session file SHA-256 `ecf6fcfc907831238b4e89bb9821f2bec5ebf55ea1a74cc33d2fbe0a51bd86c3` |
| Fixture | copies of the owner's two automation saves: working `b29c1ccb5dd0235cd8dddeee5e9d3c23a2d4357b426691a93f4d71ca574755b2`, baseline `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07` |
| Mods staged with the candidate | twelve of the owner's, from the copy's pristine snapshot: BagOfTricks, BetterVendors, CallOfTheWild, CheatMenu, CraftMagicItems, EddicKingmakerRespec, KingmakerBugfixes, ProperFlanking2, RacesUnleashed, SkipIntro, TweakOrTreat, ZFavoredClass |
| Session tooling and harness | commit `df7f11156a`, unchanged by the review corrections; harness DLL `50989f667d8f3fcd4c77b59a3881a5b2d961cae0cfa2f6dc53f337e7aa38cbf5` |
| Refusal drill | a request file and a one-launch session kept outside the repository (they hold a machine path): request SHA-256 `75f766adabbd64eb12ea266bfab260a2f0f85f0439f394a2897f7ebc1739885b`, session `b22d5cffbc873d81231385b37d6097c9f839311a849984ad8c8233819439e969`; no other mods, no fixture save |

All launches for this cut, in order:

| Run | Product | What | Result |
|---|---|---|---|
| `20261116T010215Z` | PR #170's candidate `2ee86e0f6a6e` | route check: the product's `working-save-smoke` in the copy, no other mods | TIMEOUT. The route reached the game's loader, which failed in `Player.PostLoad`: the working save does not load without the mods it was made with |
| `20261116T010718Z` | same | the same with the twelve mods staged | PASS, 69 s. The route works in the copy |
| `20261116T012451Z` | first candidate `df7f11156a01` | `disposable-gunslinger-dodge` | **PASS**, 11 of 11 |
| `20261116T012700Z` | first candidate `df7f11156a01` | lifecycle probe, Dodge step alone, no other mods, fixture `KMCS_AFTER_NORMAL_012_MODONLY.zks`, session SHA-256 `23888bd0...` | PASS |
| `20261116T024032Z` | candidate `15767a89ad2b` | `disposable-gunslinger-dodge`, same session file and fixtures | **PASS**, 12 of 12, 116 s |
| `20261116T024238Z` | candidate `15767a89ad2b` | refusal drill: a non-exiting Dodge request | PASS: refused with `gunslinger-dodge-exit-required`, 74 s |

Every session reported the owner's 95 save files and installation unchanged and the registry restored. The owner's
real installation was not used: the repository's live guarded launcher (`Invoke-KingmakerRuntimeTest.ps1`) was not
run.

Earlier evidence, kept as recorded: the old scenario's errors in the copy and the real ability's behavior on the
published v0.0.191 and PR #169's candidate are in `docs/GUNSLINGER-EFFECT-FREE-BUFF-LINKS.md`.

Validation time for this cut: about 18 minutes (two route checks 269 + 85 s, preflight 18 s, gate 16 s, candidate
build 443 s, two launches 108 + 107 s). That is over the 15-minute planning target and inside the 30-minute
ceiling; the first route check spent most of its time waiting on a load that could not finish.

Validation time for the review corrections: about 11 minutes (gate 16 s, one test-project compile that was
abandoned 15 s, preflight 21 s, candidate build 406 s, two launches 116 + 74 s).

## Limits

- Real-time play only. Turn-based mode, controller, save and load with the buff on, and the selected True Grit cost
  were not run in the game.
- The scenario was run in the disposable game copy, with twelve of the owner's mods staged. It was not run through
  the repository's live guarded launcher on the owner's installation, where the full mod list differs.
- The session needs a fixture folder holding copies of the two automation saves; they are not in the repository.
- No case was produced in the game where the scenario fails; its failure verdicts are covered by the domain tests.
  No cleanup failure, late recast or stalled wait was produced in the game.
- No outer abort was produced in the game. The finalization logic is covered by the domain tests on the session
  type; the runner's own call into it (`StopGunslingerDodge` from `Complete`) and the copy of its diagnostics have
  no test of their own.
- The product's request validator is not compiled into the domain tests. Its Dodge refusal is covered by the
  admission rule's test and by one refusal in the game; the PowerShell preflight has its own checks.
- The scenario's own timeout path was not exercised in the game.
- The disposable gunslinger is a level 1 human with no armor; armor and load are not varied.
- `disposable-gunslinger-comprehensive-acceptance` was not run after losing its Dodge slice.
- The complete domain suite was not run for this cut.
- The other deed scenarios still add their buffs directly and call hooks on units with no view (NEW-06). Only
  Dodge's is repaired here.
