# Kingmaker Gunslinger 0.0.25 smoke-test guide

Candidate: `0.0.25-s25-second-misfire-explosion`  
Target: Pathfinder: Kingmaker 2.1.7b / UMM 0.32.4 / Harmony 1.2.0.1

Use only a disposable campaign. The Test Musket still displays as a Heavy Crossbow, and ammunition still uses placeholder Diamond Dust artwork.

## What this package must prove

The new bounded sequence is:

```text
loaded/Normal
  -> first misfire
empty/Broken                    no explosion damage
  -> full-round reload
loaded/Broken
  -> second misfire
empty/Wrecked
  -> exact current wielder makes one Reflex DC 12 save
  -> exactly one native non-critical, non-precision base weapon-damage event
```

A passed Reflex save uses Kingmaker's native half-damage handling. Nearby creatures are intentionally outside this sprint.

## 1. Install the exact candidate

1. Exit Kingmaker completely to the desktop.
2. Install only `KingmakerGunslinger-0.0.25-second-misfire-explosion-smoke-test.zip` through Unity Mod Manager.
3. Start Kingmaker.
4. Confirm UMM shows **Kingmaker Gunslinger 0.0.25** with a green status indicator.
5. Confirm no bootstrap or Harmony error appears.
6. Load a disposable save.

## 2. Prepare a controlled fixture

1. Select a healthy character who can survive a musket damage roll. Hedwig is acceptable.
2. Remove temporary hit points, invulnerability, damage immunity, or unusual damage reduction when practical. Those effects may make HP evidence inconclusive even when the native event runs.
3. Grant Firearm Proficiency through the mod panel if needed.
4. Equip exactly one Test Musket.
5. Add at least two Black Powder Charges and two Lead Balls.
6. Keep one low-threat enemy alive so valid weapon attacks can be issued.
7. In the mod panel, click **Reset first equipped firearm to empty / normal**.
8. Use **Reload Test Musket** and wait for the full-round delivery to finish.
9. Click **Print selected unit's equipped-firearm state diagnostics**.
10. Confirm the starting state is:

```text
rounds=1
condition=Normal
```

11. Record the character's HP and these complete process-local diagnostic lines:

```text
Reload runtime
Firearm attack enforcement
Natural-roll misfires
Second-misfire explosion
State-token native reconciliation
```

A fresh process should begin with explosion counters at zero.

## 3. First misfire: Normal becomes Broken and does not explode

1. Click **Force next eligible firearm natural d20 to 1**.
2. Confirm the natural-roll line shows `pendingForcedRoll=1`.
3. Make one ordinary Test Musket attack against the valid enemy.
4. Pause immediately after the attack resolves.
5. Reopen the mod panel.
6. Click **Print selected unit's equipped-firearm state diagnostics**.

Required firearm and misfire result:

```text
rounds=0
ammunition=<none>
condition=Broken
naturalD20=1
misfired=True
finalSuccess=False
conditionTransition=NormalToBroken
normalToBroken=1
```

Required explosion diagnostics after this first misfire:

```text
scheduled=0
attempts=0
applied=0
notRequired=1
rejected=0
duplicates=0
faults=0
last=NOT REQUIRED: conditionTransition=NormalToBroken ...
```

The character must not lose HP from the firearm consequence. Enemy attacks and attacks of opportunity are unrelated; pause promptly and compare the combat log when needed.

Failure conditions include any Reflex save, any explosion damage, `scheduled>0`, `attempts>0`, `applied>0`, `rejected>0`, or a fault.

## 4. Reload the exact Broken firearm without repairing it

1. Record Black Powder Charge and Lead Ball counts.
2. Click **Print Reload Test Musket readiness**.
3. Confirm `available=True` and that the reason says the firearm will remain Broken.
4. Use the action-bar **Reload Test Musket** ability.
5. Wait for the full-round delivery to finish.
6. Print the equipped-firearm state again.

Required result:

```text
rounds=1
ammunition=kmg.debug.lead-ball
condition=Broken
Black Powder Charge: -1
Lead Ball: -1
```

Reload diagnostics must show one new successful load, no new rejection, `faults=0`, and Broken both before and after. Reload must not repair the item.

## 5. Second misfire: Broken becomes Wrecked and damages the exact wielder once

1. Record the exact wielder's current HP immediately before the attack.
2. Record the complete explosion diagnostic line.
3. Click **Force next eligible firearm natural d20 to 2**.
4. Confirm `pendingForcedRoll=2`.
5. Make one ordinary Test Musket attack.
6. Pause immediately after resolution.
7. Reopen the mod panel and print the equipped-firearm state.

Required firearm and misfire result:

```text
naturalD20=2
misfired=True
finalSuccess=False
conditionTransition=BrokenToWrecked
conditionBefore=Broken
conditionAfter=Wrecked
rounds=0
ammunition=<none>
brokenToWrecked=1
```

Required cumulative explosion counters for the first-plus-second-misfire sequence:

```text
scheduled=1
attempts=1
applied=1
notRequired=1
rejected=0
duplicates=0
faults=0
last=APPLIED: ...
```

The final `APPLIED` record must identify:

```text
exactWielder=<the firing character>
attackRoll=<reference identity>
repositoryIdentity=<exact item repository identity>
weaponDamage=<runtime base dice formula>
reflexDC=12
reflexNaturalD20=<1..20>
reflexTotal=<integer>
reflexPassed=True or False
halfBecauseSavingThrow=True or False
damageBeforeDifficulty=<nonnegative integer>
damageWithoutReduction=<nonnegative integer>
appliedDamage=<positive for this fixture>
hpBefore=<integer>
hpAfter=<integer>
hpLoss=<integer>
finalState=empty/Wrecked
```

Verify all of the following:

1. The exact firing character, not the enemy or another party member, took the explosion damage.
2. Exactly one Reflex save and exactly one base weapon-damage event appear for this attack.
3. The forced firearm d20 did not force the Reflex roll.
4. `hpLoss` agrees with the character's observed HP change, allowing for temporary-HP presentation when applicable.
5. `halfBecauseSavingThrow` exactly matches `reflexPassed`; `True` invokes Kingmaker's native half-damage handling and `False` leaves full damage.
6. The `attackRoll` and `repositoryIdentity` fields are present, proving the event remained correlated to the exact attack and item.
7. No Black Powder Charge or Lead Ball was consumed at attack time.
8. The firearm was already empty/Wrecked when damage resolved and remained empty/Wrecked afterward.

Any `applied` increase greater than one, any `duplicates>0`, any wrong target, or any fault is a failure.

## 6. Verify exact-item and nearby-unit isolation

1. Keep or add a second blueprint-identical Test Musket in shared inventory.
2. Click **Print visible firearm states (equipment + shared inventory)**.
3. Confirm only the exact firing item became Wrecked.
4. Confirm the second Test Musket did not change state.
5. Confirm no nearby ally or enemy received explosion damage from this bounded build.

Nearby-unit burst damage is deliberately deferred; any such damage in 0.0.25 is a failure.

## 7. Verify Wrecked reload and attack rejection

1. Click **Print Reload Test Musket readiness**.
2. Confirm `available=False` with an explicit Wrecked reason.
3. Attempt reload once.
4. Confirm no powder or Lead Ball is consumed and the item remains empty/Wrecked.
5. Click **Force next eligible firearm natural d20 to 1**.
6. Attempt one attack with the Wrecked Test Musket.
7. Confirm:

```text
wreckedRejected increases by 1
naturalRolls unchanged
misfires unchanged
explosion scheduled unchanged
explosion attempts unchanged
explosion applied unchanged
explosion notRequired unchanged
explosion rejected unchanged
pendingForcedRoll=1
```

8. Click **Cancel pending forced firearm natural d20**.

## 8. Verify empty-firearm queue isolation

1. Reset the Test Musket to empty/Normal.
2. Queue forced natural 1.
3. Attempt one empty-firearm attack.
4. Confirm `emptyRejected` increases, while the forced roll remains pending and all explosion counters remain unchanged.
5. Cancel the queued roll.

## 9. Verify ordinary firearm rolls never enter explosion handling

1. Reset the Test Musket to empty/Normal and reload it.
2. Record the explosion counters.
3. Queue forced natural 3.
4. Make one valid Test Musket attack.
5. Confirm Kingmaker's ordinary hit/miss result remains authoritative, condition stays Normal, and every explosion counter is unchanged.
6. Reload, repeat with forced natural 20, and confirm native critical behavior remains authoritative with explosion counters unchanged.

## 10. Verify native Heavy Crossbow isolation

1. Equip a genuine native Heavy Crossbow, not the Test Musket.
2. Queue forced natural 1.
3. Make one native Heavy Crossbow attack.
4. Confirm the queued firearm roll remains pending.
5. Confirm firearm misfire and explosion counters remain unchanged.
6. Cancel the queued roll.

The display name `Heavy Crossbow` is not sufficient to identify a firearm; the exact custom marker must be present.

## 11. Persistence regression

1. Return to the exact empty/Wrecked Test Musket from the second-misfire test.
2. Quicksave.
3. Print the equipped item state and confirm it remains empty/Wrecked.
4. Make a normal save.
5. Exit Kingmaker completely to the desktop.
6. Restart Kingmaker and load that save.
7. Print equipped and visible firearm states.

Required persistent state:

```text
rounds=0
ammunition=<none>
condition=Wrecked
conflicts=0
state-token faults=0
```

Process-local attack, misfire, and explosion counters reset after restart. The item-owned Wrecked state must not.

## 12. Final evidence to capture

Capture screenshots showing:

1. the first-misfire empty/Broken state and explosion counters;
2. the loaded/Broken state after reload;
3. the second-misfire empty/Wrecked state;
4. the complete `Second-misfire explosion` `APPLIED` line;
5. the character HP/combat-log evidence for the Reflex save and damage;
6. Wrecked reload unavailability;
7. Wrecked attack rejection with the forced roll preserved;
8. native Heavy Crossbow isolation; and
9. the post-restart empty/Wrecked state with zero reconciliation conflicts/faults.

## Pass gate

Sprint 25 passes only when the exact 0.0.25 standalone package proves:

- first misfire: no explosion damage;
- Broken reload: condition preserved and exactly one powder/ball pair consumed;
- second misfire: exactly one Reflex DC 12 save and one native base weapon-damage event against the exact wielder;
- full or half native damage matches the save result;
- exact item remains empty/Wrecked;
- duplicates and relevant faults remain zero;
- ordinary rolls, native Heavy Crossbows, empty firearms, Wrecked firearms, and a second Test Musket remain isolated; and
- save/restart persistence remains intact.

Sprint 26 remains blocked until that live gate passes.
