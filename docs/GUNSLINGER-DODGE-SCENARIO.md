# Gunslinger's Dodge: the product scenario runs the real ability (INT-03)

Status: **development change on a review branch. Not merged, not released.** Branch `codex/int03-dodge-scenario`,
stacked on draft PR #170 at its reviewed head `1ceab99e6a` (which is stacked on draft PR #169). The published
v0.0.191 does not contain it. Nothing here is a release qualification.

This is one bounded cut of the Gunslinger remediation epic of 2026-11-15: story S3 T3, inventory record INT-03.
Current status is kept in `planning/GUNSLINGER-REMEDIATION-STATUS.md`. It changes no gameplay: the deed's ability,
buff, cost, duration, stacking, effect links and expiry guard are as they were.

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

## Focused tests

| Check | Result |
|---|---|
| `.\scripts\Invoke-KmgGate.ps1 -Filter 'dodge;true-grit;third-playtest;loader-compat-probe;gunslinger-deed;grit-ui-recipients'` | production compiled; 40 of 3,007 selected, 0 failures (16 s) |
| `.\scripts\Test-RuntimeScenarioPreflight.ps1` | 1,199 checks passed (18 s), including the Dodge scenario's working-save admission and its refusals |
| Candidate build's own selection | 66 of 3,007 selected, 0 failures |

Six tests are new (`dodge-scenario.*`), on the scenario's verdicts with stand-in observations:

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

`disposable-gunslinger-dodge`: **PASS**, run `20261116T012451Z`, 78 s in the product, 11 of 11 assertions.

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
| Product commit | `c014e5c999` (the only product change after `1ceab99e6a`) |
| Candidate | built once at `df7f11156a01`, 443 s; package SHA-256 `d2d41e0fb21fcea133cd0fcb0de4f56f97f0b0749c8352f80b377c2a019061c3`, DLL `3b28dcd4a74a12f03d6ba23419f45f8f7a85ccd85ea03345b2ae10b8e8cb3974` |
| Scenario request | `disposable-gunslinger-dodge`, `saveName=KMG_AUTOMATION_WORKING`, completion timeout 240 s; session file SHA-256 `ecf6fcfc907831238b4e89bb9821f2bec5ebf55ea1a74cc33d2fbe0a51bd86c3` |
| Fixture | copies of the owner's two automation saves: working `b29c1ccb5dd0235cd8dddeee5e9d3c23a2d4357b426691a93f4d71ca574755b2`, baseline `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07` |
| Mods staged with the candidate | twelve of the owner's, from the copy's pristine snapshot: BagOfTricks, BetterVendors, CallOfTheWild, CheatMenu, CraftMagicItems, EddicKingmakerRespec, KingmakerBugfixes, ProperFlanking2, RacesUnleashed, SkipIntro, TweakOrTreat, ZFavoredClass |
| Session tooling and harness | commit `df7f11156a`; harness DLL `50989f667d8f3fcd4c77b59a3881a5b2d961cae0cfa2f6dc53f337e7aa38cbf5` |

All launches for this cut, in order:

| Run | Product | What | Result |
|---|---|---|---|
| `20261116T010215Z` | PR #170's candidate `2ee86e0f6a6e` | route check: the product's `working-save-smoke` in the copy, no other mods | TIMEOUT. The route reached the game's loader, which failed in `Player.PostLoad`: the working save does not load without the mods it was made with |
| `20261116T010718Z` | same | the same with the twelve mods staged | PASS, 69 s. The route works in the copy |
| `20261116T012451Z` | this cut's candidate | `disposable-gunslinger-dodge` | **PASS** |
| `20261116T012700Z` | this cut's candidate | lifecycle probe, Dodge step alone, no other mods, fixture `KMCS_AFTER_NORMAL_012_MODONLY.zks`, session SHA-256 `23888bd0...` | PASS |

Every session reported the owner's 95 save files and installation unchanged and the registry restored. The owner's
real installation was not used: the repository's live guarded launcher (`Invoke-KingmakerRuntimeTest.ps1`) was not
run.

Earlier evidence, kept as recorded: the old scenario's errors in the copy and the real ability's behavior on the
published v0.0.191 and PR #169's candidate are in `docs/GUNSLINGER-EFFECT-FREE-BUFF-LINKS.md`.

Validation time for this cut: about 18 minutes (two route checks 269 + 85 s, preflight 18 s, gate 16 s, candidate
build 443 s, two launches 108 + 107 s). That is over the 15-minute planning target and inside the 30-minute
ceiling; the first route check spent most of its time waiting on a load that could not finish.

## Limits

- Real-time play only. Turn-based mode, controller, save and load with the buff on, and the selected True Grit cost
  were not run in the game.
- The scenario was run in the disposable game copy, with twelve of the owner's mods staged. It was not run through
  the repository's live guarded launcher on the owner's installation, where the full mod list differs.
- The session needs a fixture folder holding copies of the two automation saves; they are not in the repository.
- No case was produced in the game where the scenario fails; its failure verdicts are covered by the domain tests.
  No cleanup failure was produced in the game.
- The scenario's own timeout path was not exercised in the game.
- The disposable gunslinger is a level 1 human with no armor; armor and load are not varied.
- `disposable-gunslinger-comprehensive-acceptance` was not run after losing its Dodge slice.
- The complete domain suite was not run for this cut.
- The other deed scenarios still add their buffs directly and call hooks on units with no view (NEW-06). Only
  Dodge's is repaired here.
