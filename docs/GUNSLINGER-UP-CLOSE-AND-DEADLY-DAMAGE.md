# Up Close and Deadly: damage, grit and consumption observed apart (REP-02 / S4 T2)

Status: **development change on a review branch, awaiting review. Not merged, not released.** Branch
`codex/rep02-pistolero-damage-consumption`, stacked on draft PR #178 at its accepted head `2fe1b8af2c`. Nothing here is
a release qualification. Gameplay is unchanged: the repair is to the diagnostic.

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
two times in five; it then correctly applies the full packet, and the comparison fails. Both stages consumed the state
and charged one grit each time. Neither damage delivery, grit nor marker removal differed between passing and failing
runs; the actual hit or miss of the hit stage did. No production defect was found, and gameplay is unchanged.

## The repair (diagnostic only)

- **One evaluator** (`RuntimeTesting/UpCloseAndDeadlyCaseVerdict.cs`, engine-free) judges each case as four separate
  results: the attack's outcome, the deed's damage packet, the deed's grit charge and the exact armed instance's
  consumption. The aggregate is computed only after all four. Missing or unreadable observations fail. The deed
  packet is read from its own `RuleDealDamage` (no attack roll, precision discovery disabled, one 1d6 physical or one
  fixed direct packet), never from hit-point loss; an unmultiplied critical is judged from that packet's critical
  modifier and dice, never by comparing two rolls. The charge is `before + restored by recovery − after`, so
  recovery can neither hide a missing charge nor fail a correct one. A critical flag set after resolution is not an
  engine critical. The product scenario, the loader-compat probe (by reflection) and the domain tests all call it.
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

## Results on the final candidate

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
| `scripts/Build-Local.ps1`, final | candidate `a06e989ee1d6` above; development validation passed | 405 s |
| Session on the final candidate, `20261117T053001Z` | PASS, 4 of 4 | 318 s |

The domain tests cover: correct damage with failed consumption fails only consumption; correct consumption with a
multiplied or doubled packet fails only damage; an observed miss expected as a hit fails the outcome and the damage,
and a critical flag set after resolution is not an engine critical; recovery offsetting a correct charge passes,
recovery hiding a missing charge fails, a capped recovery does not fail a correct charge; missing observations fail
every result.

**The first candidate's failure** (`20261117T051533Z`): the controlled miss case in the component fixture and in the
probe raised natural armor, but the shot was resolved against touch armor class (10 and 14), which ignores natural
armor, so the requested miss hit and the full packet was correctly applied. The evaluator reported it as a failed
outcome and failed damage with cost and consumption passing. Fixed in `a06e989ee1` with deflection; fixture setup only.

Total validation time was about 25 minutes, over the 15-minute planning target and under the 30-minute ceiling,
because the first candidate exposed the fixture error and a second build was needed.

## Limits

- **Critical recovery was not exercised.** On the real-route critical, at 3 of 5 grit, no recovery applied, so the
  case shows the deed's own charge only, not a recovery offset. Why the recovery clause declined is not established:
  the diagnostics' ignored counter is global and moved for other units' rolls in the save. The offset logic is covered
  only by the evaluator's domain tests.
- **Precision immunity, insufficient grit, misfire, scatter and duplicate delivery** are component-fixture results
  only (precision immunity is a flag set on the resolved roll), not real-route ones.
- **Cancellation** covers an order interrupted before it started. An interruption after a shot has already resolved
  was not exercised.
- **Controlled inputs.** The naturals were requested (forced roll and seeded generator), the attack bonus was raised to
  100, and the miss used +500 deflection. The deed's own d6 was not controlled.
- **Two matching runs** in fresh processes show the cases are not intermittent on this candidate under these
  prerequisites; they are not statistical proof that no intermittency is possible.
- **Level 1 only** (one die). Higher-level dice counts are covered by the existing policy tests, not observed.
- The real-route target spot was not clear of the save's own units, and the save's party attacked the hostile. Their
  damage is recorded and excluded by initiator. The first candidate's run saw one damage event in its cancellation
  window (`20261117T051533Z` launch 3) without recording its source; a party attack is a possible source, not
  established. In the final run the
  cancellation window held no damage event at all.
- The 2026-11-11 audit runs are not re-attributed from their own evidence; the attribution comes from the reproduction
  of the same sequence.
