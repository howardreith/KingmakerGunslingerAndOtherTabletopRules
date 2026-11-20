# Up Close and Deadly: damage, grit and consumption observed apart (REP-02 / S4 T2)

Status: **development change on a review branch, awaiting final review of the retention and unarmed-state
corrections asked for by the review of `b4ceae0eae`. Not merged, not released.** Branch `codex/rep02-pistolero-damage-consumption`, stacked on draft PR #178
at its accepted head `2fe1b8af2c`. Nothing here is a release qualification. Gameplay is unchanged: the repair is to the
diagnostic. The review accepted the explanation of the reproduced split; see "Review corrections" for the four
diagnostic corrections and their results.

## The historical split

`disposable-pistolero-deeds` reported `up-close-critical` as FAIL in some runs and PASS in others on identical inputs.
The retained results, under their original identities:

| Run | Package | Result | `damage=` (hit-stage, miss-stage hit-point loss) |
|---|---|---|---|
| `20261111T1816557789702Z` (audit, transaction `compat-20261111T181418Z-9d1b82c04345`) | 0.0.182 candidate | FAIL | not retained in this record |
| `20261111T1821405012055Z` (audit, transaction `compat-20261111T182024Z-636f7b658091`) | same | PASS | not retained in this record |
| `20261115T172457Z` launch 06 | grit UI candidate `4acb7055c9` (package `9d59acd52673…`) | FAIL | `6,3` |
| `20261115T173518Z` launch 01 | same | PASS | `3,3` |
| `20261115T173518Z` launch 02 | same | FAIL | `6,3` |
| `20261115T173211Z` launch 02 | published v0.0.190 (package `1f1f31fa161e…`) | PASS | `3,3` |

Each ran in its own game process (the audit's shared-process explanation was withdrawn as GS-AUD-0038). The
2026-11-15 raw evidence is on the lab machine; it holds only the aggregate string above, not the roll, the deed's
damage event or the armed instance, so it cannot attribute the split by itself. The 2026-11-11 raw logs were not
found on the machine. Attribution below comes from a fresh reproduction, not from the historical evidence.

## What the old assertion measured

The old sequence used one shared target and, for each stage, a standalone `RuleAttackRoll` resolved by the engine,
then the deed's handler invoked by hand:

- The helper's `hit` argument was unused; whether a stage hit was decided by the engine's own roll.
- The forced natural roll of 19 did not reach these standalone rolls: the measured naturals were 3 to 16.
- The native generator was seeded only after the hit stage's roll (to fix the deed's d6), so the hit stage's d20 came
  from the process's own random state while every later roll followed the seeded stream.
- "Damage" was the target's hit-point loss, and `up-close-critical` passed when the critical stage's loss equalled the
  hit stage's and the armed state was gone. Its critical flag was set on the roll after it resolved.

## Measured cause

Session `20261117T053001Z-owner-0.33.0-pistolero-up-close-damage`, launches 1 and 2 (fresh processes, same candidate),
replayed the old order unchanged with every stage measured:

| Stage | Launch 1 | Launch 2 |
|---|---|---|
| Hit stage | natural 16 + 1 = 17 against AC 14: **hit**; deed packet physical 1d6 rolled 6 | natural 9 + 1 = 10 against AC 14: **miss**; deed packet direct 3 (half of 6) |
| Miss stage | natural 3: miss; direct 3 | natural 3: miss; direct 3 |
| Precision immunity stage | natural 3: miss; no packet, no charge, state consumed | the same |
| Critical stage | natural 10: **miss**; direct 3 | natural 10: **miss**; direct 3 |
| Old aggregate | 6 against 3: **FAIL** (`6,3`) | 3 against 3: **PASS** (`3,3`) |

The first candidate's run (`20261117T051533Z`, launches 1 and 2) measured hit-stage naturals 4 and 5 (both misses,
both `3,3` PASS) and the same seeded stages.

So the split is an invalid diagnostic assumption. The "critical" stage is, in this order, always an ordinary miss: the
deed correctly applies half of one roll. The "hit" stage is a real hit only when its unseeded d20 is 13 or more, about
two times in five; it then correctly applies the full packet, and the comparison fails. In every replay the deed's
damage matched each stage's actual hit or miss, and cost and consumption were correct (one grit and the state removed
at each stage). The hit stage's damage amount itself differed between passing and failing replays (6 for a hit, 3 for
a miss), because its actual outcome differed. No production defect was found, and gameplay is unchanged.

## The repair (diagnostic only)

- **One evaluator** (`RuntimeTesting/UpCloseAndDeadlyCaseVerdict.cs`, engine-free) judges each case as four separate
  results: the attack's outcome, the deed's damage packet, the deed's grit charge and the exact armed instance's
  consumption. The aggregate is computed only after all four. Missing or unreadable observations fail. The deed
  packet is read from its own `RuleDealDamage` (no attack roll, precision discovery disabled, one 1d6 physical or one
  fixed direct packet), never from hit-point loss; an unmultiplied critical is judged from that packet's critical
  modifier and dice, never by comparing two rolls. A critical flag set after resolution is not an engine critical.
  The product scenario, the loader-compat probe (by reflection) and the domain tests all call it. Its half-packet,
  consumption, cost and cancellation rules were tightened after review; see "Review corrections".
- **`disposable-pistolero-deeds`** is labelled a component fixture (AddBuff arming, hand-invoked handler, standalone
  rolls, detached units at the main menu). It replays the old order first as recorded evidence with no contract
  verdict (`up-close-original-sequence-measured`), then runs controlled cases, each on a fresh target with 500 extra
  hit points: the outcome is set before the roll resolves (forced natural plus a native d20 seeded to the same
  value; for the critical, the engine's own `AutoCriticalThreat` and `AutoCriticalConfirmation`; for the miss, +500
  deflection, because a first-increment firearm shot uses touch armor class). Each case has `.outcome`, `.damage`,
  `.cost` and `.consumption` assertions; the old names (`up-close-hit`, `up-close-miss`, `up-close-critical`,
  `up-close-precision-immunity`, `up-close-misfire-and-scatter`, `up-close-dead-shot-once`) are now aggregates over
  them, and `up-close-insufficient-grit` is added. Twin Shot is unchanged. Evidence file `pistolero-deeds.json`.
- **The real route**: loader-compat probe `upCloseDamage` (`tools/LoaderCompatHarness/DeedLifecycleProbes.cs`) in the
  mod-only fixture save `KMCS_AFTER_NORMAL_012_MODONLY.zks`, the candidate alone. A Pistolero levelled to 1 by the
  level-up controller (the deed granted by its progression), the registered Up Close and Deadly ability through the
  game's ability command, and each shot through the game's attack command (`UnitAttack`), whose own roll is checked to
  be the one observed. Controlled inputs: the product's forced natural roll plus the native generator seeded as the
  roll starts; base attack bonus 100 so the requested naturals hit and a threat confirms; a hostile with 5,000 extra
  hit points, unable to move, no AI on either unit; +500 deflection for the miss. The target spot was not clear of the
  save's own units.
- `scripts/compatibility/loader-compat/sessions/pistolero-up-close-damage.json` runs the component scenario twice and
  the probe twice, each in a fresh process.

## Results on candidate `a06e989ee1d6` (before the review corrections)

These results were judged by the evaluator as it stood at `a06e989ee1`: the miss packet was checked against the range
of possible halves, not the actual roll; consumption was read from the buff's absence after the shot's waits; the
cancellation case was checked by the probe's own expectations; and recovery read the global last before and after
values. They stay as recorded; the corrected evaluator and probe were not run on these bytes.

Candidate `a06e989ee1d6` (package `9f31014430b89b7f0b99ac6d913614bcf181a29f8859a682df8fe0531c6a6259`, DLL
`fedf6314ed5e6e4c59307b6e8d211997568ad7536b1ce234e023363a4512a4a5`), harness `0278125b0785989e55c8ca7452f10f54b7c8b516c82efd627a3b5d69cba3b5c7`,
UMM 0.33.0. Session `20261117T053001Z`: PASS, 4 of 4 launches. The session reports 95 owner saves unchanged, the
owner installation unchanged and the registry restored. Raw evidence stays on the lab machine.

### Real route (registered ability, attack command), identical in both processes

| Case | Roll | Deed packet | Weapon packet | Grit (before + recovery → after) | Armed instance |
|---|---|---|---|---|---|
| Ordinary hit | natural 15, hit | physical 1d6, rolled 5, applied 5, no critical modifier | physical 1d8 (5; 8) | 5 + 0 → 4 | removed |
| Engine-confirmed critical | natural 20, threat confirmed by the engine | physical 1d6, rolled 4, applied 4, **no critical modifier** | physical 1d8 ×4 (20; 19) | 3 + 0 → 2 | removed |
| Non-misfire miss | natural 10 against 510, miss | direct 2 (half of one 1d6 roll) | none | 2 + 0 → 1 | removed |
| Following unarmed shot | natural 15, hit | none | physical 1d8 (3; 6) | 1 + 0 → 1 | none armed |
| Order cancelled before start | none: the order was taken and interrupted in the same frame, not started, result Interrupt | none in the next second | none | 1 → 1, and 1 after expiry | retained; expired by itself after 6.03 s and 6.00 s |

Every shot resolved exactly one roll, the attack command's own. The target survived throughout (4,926 or more hit
points left). Weapon figures are launch 3; launch 4. The fixture save's own party members (for example Amiri, Harrim
and Valerie) also attacked the hostile once the fight began: 19 damage events on it per launch, of which only the
Pistolero's count. The deed and weapon packets above are attributed by the event's initiator and construction, so the
party's damage does not enter them; hit-point figures include it.

### Component fixture, identical in both processes

| Case | Outcome | Deed packet | Charge | Armed instance |
|---|---|---|---|---|
| Hit | natural 15, hit | physical 1d6, 5 | 1 | removed |
| Miss | natural 10 against 514, miss | direct 1 | 1 | removed |
| Critical (engine `AutoCritical`) | natural 20, confirmed | physical 1d6, 4, no critical modifier | 1 | removed |
| Precision immunity (flag set on the resolved roll) | hit | none | 0 | removed |
| Insufficient grit (0 after arming) | hit | none | 0 | removed |
| Misfire (natural 1) | not a completed discharge | none | 0 | retained |
| Scatter (registered as a scatter volley) | hit | none | 0 | retained |
| Dead Shot delivery, handler invoked twice | hit | one packet, 5 | 1 | removed |

44 of 44 assertions in each launch. No grit recovery applied in any case.

## Validation

| Check | Result | Time |
|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'up-close;pistolero;archetype'` | production compilation passed; 34 of 34 (six new `up-close-verdict.*` tests); run after each change | 17 s |
| Harness build (`Build-LoaderCompatHarness.ps1`, game-copy references) | built; first `cdcecf3ea2e8…`, final `0278125b0785…` | under 10 s |
| `scripts/Build-Local.ps1 -DevelopmentPurpose pistolero-damage-consumption`, first | candidate `27578ecf2172` (package `b79f42b6dc11…`, DLL `b554c986b3e9…`); development validation passed | 420 s |
| Session on the first candidate, `20261117T051533Z` | FAIL, 4 of 4 launches: the controlled miss hit in both fixtures (see below); every other case passed | 332 s |
| `scripts/Build-Local.ps1`, second | candidate `a06e989ee1d6` above; development validation passed | 405 s |
| Session on the second candidate, `20261117T053001Z` | PASS, 4 of 4 | 318 s |

The domain tests cover: correct damage with failed consumption fails only consumption; correct consumption with a
multiplied or doubled packet fails only damage; an observed miss expected as a hit fails the outcome and the damage,
and a critical flag set after resolution is not an engine critical; recovery offsetting a correct charge passes,
recovery hiding a missing charge fails, a capped recovery does not fail a correct charge; missing observations fail
every result.

**The first candidate's failure** (`20261117T051533Z`): the controlled miss case in the component fixture and in the
probe raised natural armor, but the shot was resolved against touch armor class (10 and 14), which ignores natural
armor, so the requested miss hit and the full packet was correctly applied. The evaluator reported it as a failed
outcome and failed damage with cost and consumption passing. Fixed in `a06e989ee1` with deflection; fixture setup only.

Total validation time for that pass was about 25 minutes, over the 15-minute planning target and under the 30-minute
ceiling, because the first candidate exposed the fixture error and a second build was needed.

## Review corrections (review of `3d6b4b0c19`)

The review accepted the explanation of the reproduced split and asked for four diagnostic corrections, made in
`e63f6e43b2`. Gameplay is unchanged.

1. **Exact half.** The handler now keeps the d6 results it already draws for a miss, in the same order and with no
   extra draw, in a weak record keyed by the exact attack and its owner
   (`Archetypes/UpCloseAndDeadlyDeliveryRecords.cs`). The evaluator sums that record and requires the fixed packet to
   equal `floor(total / 2)`; a missing, unreadable or wrong-length record fails. The total is never derived from the
   packet.
2. **Consumption inside the delivery.** The exact instance and its deadline are captured before the shot. Every
   observed event is numbered in order: the shot's roll, the exact instance's removal (the game's buff-removed notice
   for that reference), the deed's damage event. Consumption passes only when the removal falls after the roll and
   before the deed's damage event (the handler removes the state before dealing it), or, with no damage event, inside
   the hand-invoked delivery, and before the deadline. The buff's absence at the final read is recorded but never
   decides consumption.
3. **Admitted, interrupted cancellation.** `EvaluateCancellation` requires the exact new order to be admitted to the
   actor's commands, not started, not acted, and ended `Interrupt`; a rejected or never-admitted order fails as an
   unmet prerequisite even with no roll, damage or grit change. The probe now calls it.
4. **Recovery attribution.** No recovery applied anywhere is a valid zero (`none-applied`). Any applied recovery is
   `unattributed`, because the recovery diagnostics are global and do not name the unit they restored, and the cost
   is then unknown. The evaluator accepts a nonzero amount only when attributed to the actor; the current readers
   never claim that.

Domain controls, through the same `Evaluate` and `EvaluateCancellation` the consumers call (37 of 37 focused tests):

| Control | Result |
|---|---|
| Full roll 5, emitted 2 | damage passes |
| Full roll 5, emitted 0; full roll 5, emitted 3 | damage fails; cost and consumption still pass |
| Full roll 1, emitted 0 | damage passes |
| Full roll missing; unreadable; two dice for a one-die deed | damage fails |
| Exact instance removed after the roll, before the deed's damage, before its deadline | consumption passes |
| Retained through delivery, gone only at its deadline (a final read finds no buff) | consumption fails |
| Removed after the deed's damage event; another instance removed; identity, start, time or deadline missing; no bound on the window | consumption fails |
| Admitted, not started, not acted, `Interrupt` | cancellation passes |
| Never admitted, with no roll, no damage and no grit change | prerequisite fails (the quiet outcome is recorded apart); started, acted or a `Success` result also fail |
| Another unit's recovery (unattributed) beside a missing charge | cost fails |
| Attributed recovery offsetting a correct charge; capped recovery; no recovery | cost passes |
| Attributed recovery hiding a missing charge; `none-applied` with a nonzero amount | cost fails |

### Results on the corrected candidate

Candidate `e63f6e43b214` (package `a83fc922c6d74b9c224dc8f628ec8e254a406a2fbfc026f662b147699a4c1598`, DLL
`f382e7997960c8ee4c123b9321fad1febe603cc0481624e7c3022edf803fb53a`), harness
`836770fb5cd5c463a798e6270b22dfe90aed74b7746ce410eaa524fdc5ec1755`, the same four-launch session file. Session
`20261117T163622Z-owner-0.33.0-pistolero-up-close-damage`: PASS, 4 of 4 fresh launches; the probe reported no
findings in either process and the component fixture 44 of 44 assertions in each. The session reports 95 owner saves
unchanged, the owner installation unchanged and the registry restored.

Real route, both processes:

| Case | Deed packet | Full roll → half | Exact instance: removal order and time | Grit |
|---|---|---|---|---|
| Ordinary hit (natural 15) | physical 1d6, 5 | — | removed after the roll, before the deed's damage event; absent at that event; 43,232.17 s against a deadline of 43,237.37 s (launch 3) | 5 → 4, no recovery applied anywhere |
| Engine-confirmed critical (natural 20; weapon ×4) | physical 1d6, 4, no critical modifier | — | the same order; 43,235.46 s against 43,240.82 s | 3 → 2, no recovery applied anywhere |
| Miss (natural 10 against 510) | direct 2 | 5 → 2 | the same order; 43,238.70 s against 43,244.07 s | 2 → 1, no recovery applied anywhere |
| Following unarmed shot | none | — | none armed | 1 → 1 |
| Cancellation | none | — | order admitted, not started, not acted, `Interrupt`; no roll; instance retained; expired by itself after 6.03 s and 6.02 s | 1 → 1, 1 after expiry |

Launch 4 matched in every verdict (removal at 43,232.20, 43,235.49 and 43,238.73 s against deadlines 5.2 to 5.4 s
later). Every shot's roll was the attack command's own and the target survived throughout.

Component fixture, both processes: hit, critical and the twice-invoked Dead Shot delivery each one packet, one grit,
the exact instance removed inside the delivery before the deed's damage event; the miss a direct 1 from a recorded
roll of 2; precision immunity and insufficient grit removed inside the delivery with no damage event and no charge;
misfire and scatter retained with no charge. The replayed original order measured hit-stage naturals 13 and 14 (both
real hits, 6 against the critical stage's 3, old aggregate FAIL in both); its miss and critical stages recorded the
roll 6 and emitted 3.

**Critical recovery inputs** (read when the confirmed critical's roll resolved, both processes): actor character level
1, target character level 0 (the hostile is an unlevelled clone), both in combat, target not helpless, exact firearm,
outcome qualifying. The product's recovery policy, re-evaluated on those inputs, decides `InsignificantTarget`
(`targetHitDice * 2 < characterLevel`). That reading is consistent with no recovery having applied; it is the policy
evaluated on observed inputs, not the runtime's own record of its decision.

| Check | Result | Time |
|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'up-close;pistolero;archetype'` | production compilation passed (14 s); 37 of 37 | 17 s |
| Harness build | `836770fb5cd5…` | under 10 s |
| `scripts/Build-Local.ps1 -DevelopmentPurpose pistolero-damage-consumption` | candidate `e63f6e43b214`; development validation passed | 435 s |
| Session `20261117T163622Z` | PASS, 4 of 4 | 343 s |

About 13 minutes in total for this pass.

## Second review correction (review of `b4ceae0eae`)

The review accepted the split's attribution, the exact-half verdict, the positive consumption checks, the admitted
cancellation, the conservative recovery attribution and candidate `e63f6e43b214`'s hit, critical, miss and
cancellation results. It found one false-pass path in the non-consumption branches, corrected in `d72746b81c`:

- **Retention needed positive evidence.** A retained (armed, not consumed) case passed whenever no removal event was
  seen, even with no delivery bounds, deadline or presence read. It now needs the exact instance identified before the
  delivery, delivery start and end both observed, the deadline observed, no removal of that instance inside the
  delivery, and the instance on the actor at an exact-reference read taken immediately after the delivery
  (`armedInstance.presentAtDeliveryEnd`, before any fixture cleanup). The final read after cleanup stays supplementary.
- **The unarmed state is read, not inferred.** The probe assigned an instance only when it armed, so the following
  unarmed shot reported "not identified" by construction. It now reads the actor's armed state before every shot
  (`armedInstance.presentBeforeShot`, and the count of armed instances); an expected-unarmed case passes only on an
  explicit `presentBeforeShot == false`, and an armed case only when exactly one instance is present and it is the one
  the ability created.

Controls through the same evaluator (39 of 39 focused tests); run against `b4ceae0eae`'s evaluator, the two new
groups fail, at "identified only" retention and at an instance present before an unarmed shot:

| Control | Result |
|---|---|
| Identified, bounded, deadline, no removal inside, present at delivery end | retention passes |
| Identified only; any one of start, end, deadline, end presence or identity missing | retention fails |
| No removal event, but the instance absent at delivery end | retention fails |
| Removed before the delivery, then absent | retention fails |
| Removed inside the delivery | retention fails |
| Removed after the delivery ended (its later expiry) | retention still passes |
| Positive consumption (removal after the roll, before the deed's damage, before the deadline) | still passes |
| Actual pre-shot absence | unarmed state passes |
| An instance present before an "unarmed" shot | state fails; no packet and no charge still pass apart |
| `identified=false` alone; an unreadable pre-shot read | state fails |

Candidate `d72746b81c5d` (package `0a43cd8f3e3f7460539e2ff1241047541fd895ce53490befc77f6f657b85cb85`, DLL
`99a19f16210d8a74097632885b97d90d367626fe88ef5dd151ba881199c12398`), harness
`593a26d438f8cd68e5c0e9c6b5b7a387c583fb0e0c723a81af82857da5db1077`, session
`20261117T183252Z-owner-0.33.0-pistolero-up-close-damage`: PASS, 4 of 4 fresh launches, identical verdicts in each pair;
44 of 44 component assertions per launch and no probe findings. The session reports 95 owner saves unchanged, the
owner installation unchanged and the registry restored.

| Observation | Both processes |
|---|---|
| Component misfire | exact instance present before and at delivery end; no removal inside the delivery (sequence 1 to 2); deadline 6.0 s; no packet; grit 5 → 5 |
| Component scatter | the same |
| Real-route following unarmed shot | no armed instance on the actor before the order (count 0); no packet; grit 1 → 1 |
| Real-route hit, critical, miss | exactly one armed instance before each shot, the ability's own; removed after the roll and before the deed's damage event, absent at it, 5.2 to 5.5 s before the deadline |
| Exact half | real-route miss: recorded roll 5, packet 2; component miss: recorded roll 2, packet 1 |
| Grit | hit 5 → 4, critical 3 → 2, miss 2 → 1; no recovery applied anywhere |
| Cancellation | admitted, not started, not acted, `Interrupt`; no roll; retained; expired after 6.01 s; no charge |

The earlier candidates' results are unchanged and were judged by the evaluators of their own commits. The DEF-01 row
of the status register no longer calls the assertion unattributed.

| Check | Result | Time |
|---|---|---|
| Focused gate | production compilation passed (15 s); 39 of 39 | 18 s |
| The new controls against `b4ceae0eae`'s evaluator | 2 of 11 `up-close-verdict` tests fail, as intended; corrected evaluator restored and the gate rerun, 39 of 39 | 18 s each |
| Harness build | `593a26d438f8…` | under 10 s |
| `Build-Local.ps1` | candidate `d72746b81c5d`; development validation passed | 441 s |
| Session `20261117T183252Z` | PASS, 4 of 4 | 339 s |

About 14 minutes for this pass.

## Limits

- **No native recovery offset.** On the real-route critical, at 3 of 5 grit, no recovery applied, so the case shows
  the deed's own charge only. The observed inputs (target character level 0 against actor level 1) make the policy
  decide `InsignificantTarget`; that is a reading of the policy, not a confirmed runtime decision, and not a gameplay
  defect. A native case with a significant target is a disclosed follow-up. Any applied recovery would now read as
  unattributed and fail the cost, because the diagnostics cannot name the unit.
- **Precision immunity, insufficient grit, misfire, scatter and duplicate delivery** are component-fixture results
  only (precision immunity is a flag set on the resolved roll), not real-route ones.
- **Cancellation** covers an order interrupted before it started. An interruption after a shot has already resolved
  was not exercised.
- **Controlled inputs.** The naturals were requested (forced roll and seeded generator), the attack bonus was raised to
  100, and the miss used +500 deflection. The deed's own d6 was not controlled; the miss's roll is read from the
  handler's own record of what it drew, a diagnostic inside the product, not an observation outside it.
- **Consumption bound.** In the real route the delivery window is bounded by the deed's damage event; the end of the
  outer attack rule cannot be observed from a global handler, which runs before the deed's initiator handler.
- **Two matching runs** in fresh processes per candidate show the cases are not intermittent on those bytes under
  these prerequisites; they are not statistical proof that no intermittency is possible.
- **Level 1 only** (one die). Higher-level dice counts are covered by the existing policy tests, not observed.
- The real-route target spot was not clear of the save's own units, and the save's party attacked the hostile. Their
  damage is recorded and excluded by initiator. The first candidate's run saw one damage event in its cancellation
  window (`20261117T051533Z` launch 3) without recording its source; a party attack is a possible source, not
  established. In the final run the
  cancellation window held no damage event at all.
- The 2026-11-11 audit runs are not re-attributed from their own evidence; the attribution comes from the reproduction
  of the same sequence.
