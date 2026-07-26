# Kingmaker Gunslinger 0.0.26 smoke-test guide

Candidate: `0.0.26-s26-misfire-burst`  
Target: Pathfinder: Kingmaker 2.1.7b / UMM 0.32.4 / Harmony 1.2.0.1

Use only a disposable campaign. The Test Musket still displays as a Heavy Crossbow and ammunition still uses placeholder Diamond Dust artwork.

## What this package must prove

A second misfire from a loaded/Broken Test Musket must leave the exact item empty/Wrecked and resolve one native Reflex DC 12 save plus one fresh native base weapon-damage event for every unique living, targetable unit in the Test Musket's native 5-foot line-of-sight burst. The exact wielder must be included once. A first misfire still causes no burst.

## 1. Install the exact candidate

1. Exit Kingmaker completely to the desktop.
2. Install only `KingmakerGunslinger-0.0.26-misfire-burst-smoke-test.zip` through Unity Mod Manager.
3. Start Kingmaker.
4. Confirm UMM shows **Kingmaker Gunslinger 0.0.26** with a green status indicator.
5. Confirm there is no bootstrap or Harmony error.
6. Load a disposable save.

## 2. Arrange a controlled spatial fixture

1. Select a healthy firearm wielder who can survive a musket damage roll.
2. Grant Firearm Proficiency if needed and equip exactly one Test Musket.
3. Add at least two Black Powder Charges and two Lead Balls.
4. Keep one valid hostile target alive for the firearm attack.
5. Position at least one other living unit very close to the wielder—clearly within 5 feet and with unobstructed line of sight. An ally is easiest because it will not move while paused.
6. Position another living unit clearly outside 5 feet, or behind a solid wall that blocks line of sight.
7. Remove temporary HP, invulnerability, immunity, or unusual damage reduction from the expected in-burst units when practical.
8. Record the names and HP of the wielder, the intended nearby unit, and the intended outside/blocked unit.
9. Open the mod panel and enable firearm combat tracing.
10. Reset the first equipped firearm to empty/Normal, reload it, and confirm:

```text
rounds=1
condition=Normal
misfireBurst=5ft
```

11. Record the complete `Second-misfire explosion` line. A fresh process should begin at zero.

Native qualification uses mechanics distance, occupied-space corpulence, targetability, and line of sight. Do not rely only on visual center-to-center distance at the five-foot edge.

## 3. First misfire: no query and no burst

1. Queue forced firearm natural d20 `1`.
2. Make one valid Test Musket attack.
3. Pause immediately and reopen the panel.

Required state:

```text
rounds=0
condition=Broken
conditionTransition=NormalToBroken
normalToBroken +1
```

Required explosion changes:

```text
notRequired +1
scheduled unchanged
attempts unchanged
queries unchanged
plannedTargets unchanged
targetApplied unchanged
faults=0
targetFaults=0
```

No unit may receive explosion save/damage from the first misfire.

## 4. Reload while Broken

1. Record powder and Lead Ball counts.
2. Confirm reload readiness is available and says the firearm will remain Broken.
3. Reload and wait for the full-round delivery.
4. Confirm:

```text
rounds=1
condition=Broken
Black Powder Charge: -1
Lead Ball: -1
reload faults=0
```

## 5. Second misfire: native multi-target burst

1. Record HP for every fixture unit immediately before the attack.
2. Record all explosion counters.
3. Queue forced firearm natural d20 `2`.
4. Make one valid Test Musket attack.
5. Pause immediately after all Reflex and damage events resolve.
6. Reopen the panel and capture the complete explosion line.

Required firearm result:

```text
naturalD20=2
misfired=True
finalSuccess=False
conditionTransition=BrokenToWrecked
rounds=0
ammunition=<none>
condition=Wrecked
```

Required burst-level changes:

```text
scheduled +1
attempts +1
queries +1
applied +1
rejected unchanged
duplicates unchanged
faults=0
```

Let `N` be the number of unique qualified units: every native-qualified nearby unit plus the exact wielder once. Required target changes:

```text
plannedTargets +N
targetAttempts +N
targetApplied +N
targetRejected unchanged
targetFaults=0
```

`queryCandidates` may be at least `N-1`, and `targetDuplicates` may increase when Kingmaker's query also returns the exact wielder or another duplicate reference. That is acceptable only when `targetApplied` still increases exactly once per unique planned unit and the burst-level `duplicates` counter remains unchanged.

The final `APPLIED` record must include one bracketed target result for every planned unit. For each result verify:

```text
target=<name>
unitId=<stable identity>
distanceMeters=<native mechanics distance>
exactWielder=True or False
reflexNaturalD20=<1..20>
reflexTotal=<integer>
reflexPassed=True or False
halfBecauseSavingThrow exactly matches reflexPassed
appliedDamage=<nonnegative integer>
hpBefore=<integer>
hpAfter=<integer>
```

The exact wielder result must appear once and last. The forced firearm d20 must not force any Reflex roll.

## 6. Verify inclusion and exclusion

1. Confirm the exact wielder received one Reflex save and one damage event.
2. Confirm each clearly in-range, unobstructed living unit received one Reflex save and one damage event.
3. Confirm the clearly outside-range or line-of-sight-blocked unit received neither.
4. Confirm no unit received two saves or two damage events from the same explosion.
5. Confirm each HP change agrees with its own native damage record, allowing for temporary-HP or reduction presentation if those were not removed.
6. Confirm the exact firearm remained empty/Wrecked throughout target delivery.

Any wrong target, missing intended nearby target, affected outside/blocked target, repeated target, or target fault is a failure.

## 7. Wrecked attack and reload must not repeat the burst

1. Confirm reload readiness is unavailable with an explicit Wrecked reason.
2. Attempt reload and confirm no ammunition is consumed.
3. Record explosion counters.
4. Queue forced firearm natural d20 `1`.
5. Attempt one attack with the Wrecked Test Musket.
6. Confirm:

```text
wreckedRejected +1
naturalRolls unchanged
scheduled unchanged
attempts unchanged
queries unchanged
targetApplied unchanged
pendingForcedRoll=1
```

7. Cancel the queued roll.

## 8. Exact-item and ordinary-roll isolation

1. Add or inspect a second blueprint-identical Test Musket and confirm only the exact firing item is Wrecked.
2. Reset and reload the test firearm, force natural `3`, and attack.
3. Confirm ordinary resolution remains authoritative, condition stays Normal, and all explosion counters remain unchanged.
4. Repeat with natural `20` when practical.
5. Equip a genuine native Heavy Crossbow, queue forced natural `1`, and attack.
6. Confirm the queued roll remains pending and misfire/explosion counters remain unchanged.
7. Cancel the queued roll.

## 9. Persistence regression

1. Return to the exact empty/Wrecked Test Musket created by the second misfire.
2. Quicksave and confirm it remains empty/Wrecked.
3. Make a normal save.
4. Exit Kingmaker completely to the desktop.
5. Restart and load that save.
6. Print equipped and visible firearm states.

Required persistent state:

```text
rounds=0
ammunition=<none>
condition=Wrecked
state-token conflicts=0
state-token faults=0
```

Process-local burst counters reset after restart; item state must persist.

## 10. Final evidence to capture

Capture screenshots showing:

1. the arranged nearby and outside/blocked fixture;
2. first-misfire empty/Broken state with no query/burst;
3. loaded/Broken state after reload;
4. second-misfire empty/Wrecked state;
5. the complete multi-target `APPLIED` diagnostic line;
6. combat-log or HP evidence for every affected unit's Reflex save and damage;
7. evidence that the outside/blocked unit was untouched;
8. Wrecked reload and attack rejection without another query;
9. native Heavy Crossbow isolation; and
10. post-restart empty/Wrecked persistence with zero reconciliation conflicts/faults.

## Pass gate

Sprint 26 passes only when this exact package proves one definition-sized native query, exactly one save/damage pair per unique qualified unit, exact-wielder inclusion once and last, correct outside/line-of-sight exclusion, no repeated burst from a Wrecked attack, exact-item and native-crossbow isolation, persistent empty/Wrecked state, and zero relevant burst, target, misfire, attack, reload, AC, trace, token, bootstrap, or Harmony faults.

Sprint 27 remains blocked until this live gate passes.
