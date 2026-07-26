# Kingmaker Gunslinger 0.0.24 Sprint 24 misfire-condition smoke test

## Purpose

This build combines two evidence layers:

1. recheck the carried-forward Sprint 23 natural-roll and queue-isolation controls; and
2. prove the new Sprint 24 item-owned condition transitions:

```text
Normal -> Broken
Broken -> Wrecked
```

The Test Musket misfires on natural **1-2**. A successfully discharged exact firearm consumes its loaded round first. A detected misfire then forces the attack to miss and changes only that exact item. The resulting state must remain `rounds=0; ammunition=<none>`.

Version 0.0.24 does not add explosion, splash, area, or wielder damage; gameplay repair or Quick Clear; automatic iterative reload; Rapid Reload; additional firearm blueprints; or Gunslinger class behavior.

Use only a disposable campaign.

## Install

1. Exit Kingmaker completely.
2. Install `KingmakerGunslinger-0.0.24-misfire-condition-smoke-test.zip` through Unity Mod Manager over the previous build.
3. Start Kingmaker and confirm Unity Mod Manager shows **Kingmaker Gunslinger 0.0.24** with a green status indicator.
4. Open the mod panel and confirm:
   - `Blueprint state: initialized.`
   - `Natural-roll misfires` shows `faults=0` and `pendingForcedRoll=<none>`.
   - firearm attack enforcement, firearm AC, reload, token reconciliation, and combat tracing show zero faults.
5. Grant Firearm Proficiency, add/equip one Test Musket, add basic ammunition, and load the Test Musket.

The Test Musket still displays as a Heavy Crossbow, and ammunition still uses placeholder Diamond Dust artwork.

## Diagnostic counters

The `Natural-roll misfires` line reports:

- `eligible`: successfully discharged exact-firearm attacks registered for evaluation;
- `naturalRolls`: eligible attacks that reached the exact main `Roll` assignment;
- `ordinary`: rolls above the configured misfire threshold;
- `misfires`: rolls within the configured threshold;
- `normalToBroken`: detected misfires that committed Normal → Broken;
- `brokenToWrecked`: detected misfires that committed Broken → Wrecked;
- `forcedApplied`: queued development rolls actually applied;
- `duplicateAssignments` and `duplicateEvaluations`: expected to remain zero;
- `noNaturalRoll`: eligible attacks that completed before a natural d20 assignment;
- `faults`: any roll, classification, or exact-item mutation failure, expected to remain zero; and
- `pendingForcedRoll`: the process-local queued value, or `<none>`.

The last result includes `conditionTransition`, `conditionBefore`, `conditionAfter`, `stateBefore`, and `stateAfter`.

## Test A — Normal misfire becomes Broken

1. Equip one loaded, Normal Test Musket.
2. Record firearm attack-enforcement, natural-roll, token-repository, and inventory counters.
3. Queue **Force next eligible firearm natural d20 to 2**.
4. Confirm `pendingForcedRoll=2`.
5. Make one ordinary Test Musket attack against a valid target.
6. Reopen the panel and print equipped-firearm state diagnostics.

Required result:

- attack enforcement increments `fired` exactly once;
- the exact firing item changes from `rounds=1; condition=Normal` to `rounds=0; condition=Broken`;
- `eligible`, `naturalRolls`, `misfires`, `normalToBroken`, and `forcedApplied` each increment by exactly one;
- `brokenToWrecked` and `ordinary` do not increment;
- the last result reports `naturalD20=2`, `misfired=True`, `finalSuccess=False`, `conditionTransition=NormalToBroken`, `conditionBefore=Normal`, and `conditionAfter=Broken`;
- `stateBefore` and `stateAfter` are both empty;
- the combat result is a miss even when the native total would hit;
- no additional Black Powder Charge or Lead Ball is consumed by firing;
- `pendingForcedRoll=<none>`; and
- all relevant fault and duplicate counters remain zero.

## Test B — ordinary natural 3 does not change condition

1. Reset or repair the Test Musket to empty/Normal, then reload it.
2. Queue natural 3 and attack.
3. Inspect diagnostics and item state.

Required result:

- one loaded round is consumed exactly once;
- `eligible`, `naturalRolls`, `ordinary`, and `forcedApplied` increment by one;
- `misfires`, `normalToBroken`, and `brokenToWrecked` do not increment;
- the last result reports `naturalD20=3`, `misfired=False`, `conditionTransition=None`, and Normal before/after;
- Kingmaker's native success result remains authoritative; and
- faults and duplicates remain zero.

A natural 3 can still miss if its native total does not reach the target AC.

## Test C — natural 1 boundary from Normal

1. Reset to empty/Normal and reload.
2. Queue natural 1 and attack.

Required result:

- one new misfire and one new `normalToBroken` transition;
- one exact round consumed;
- final state empty/Broken;
- final attack result miss; and
- zero faults and duplicates.

Kingmaker may label natural 1 as a critical miss. The authoritative condition evidence is the UMM diagnostic line.

## Test D — Broken misfire becomes Wrecked

1. Begin with the empty/Broken Test Musket produced by Test A or C.
2. Reload that exact Broken Test Musket. It must become loaded/Broken.
3. Record counters and queue natural 2.
4. Attack once.
5. Print equipped-firearm state diagnostics.

Required result:

- the exact item changes from `rounds=1; condition=Broken` to `rounds=0; condition=Wrecked`;
- `fired`, `eligible`, `naturalRolls`, `misfires`, `brokenToWrecked`, and `forcedApplied` each increment once;
- `normalToBroken` does not increment for this attack;
- the last result reports `conditionTransition=BrokenToWrecked` and empty state before/after;
- the attack is a miss;
- no explosion, splash, area, or extra wielder damage is added by the mod in this sprint;
- no attack-time inventory ammunition is consumed; and
- faults and duplicates remain zero.

Afterward, reload must be unavailable and a Wrecked attack attempt must be forced to miss.

## Test E — natural 20 remains ordinary

1. Use a separate or reset empty/Normal Test Musket and reload it.
2. Queue natural 20 and attack.

Required result:

- `ordinary` increments once;
- `misfires`, `normalToBroken`, and `brokenToWrecked` do not increment;
- condition remains Normal;
- one round is consumed exactly once;
- Kingmaker retains its native natural-20 and critical behavior; and
- faults and duplicates remain zero.

## Test F — exact-item isolation with two Test Muskets

1. Put two Test Muskets on the selected character or keep one equipped and one in shared inventory.
2. Reset both to empty/Normal.
3. Load only the equipped Test Musket.
4. Record the repository identities and states of both items.
5. Queue natural 2 and attack with the equipped item.
6. Print visible firearm states.

Required result:

- only the firing repository identity changes to empty/Broken;
- the second Test Musket remains empty/Normal;
- no state is selected by display name, Heavy Crossbow category, inventory position, or owner; and
- repository conflicts and faults remain zero.

## Test G — native Heavy Crossbow cannot consume the queue

1. Queue natural 2 and confirm `pendingForcedRoll=2`.
2. Unequip every Test Musket and equip a genuine native Heavy Crossbow.
3. Confirm selected-unit diagnostics report `equippedFirearms=none detected`.
4. Make one native Heavy Crossbow attack.
5. Inspect counters before re-equipping a Test Musket.

Required result:

- `pendingForcedRoll` remains `2`;
- firearm `eligible`, `naturalRolls`, `ordinary`, `misfires`, `normalToBroken`, `brokenToWrecked`, and `forcedApplied` do not change because of the native attack;
- the native attack resolves normally; and
- no firearm or Harmony fault appears.

Then re-equip and load a Normal Test Musket, attack once, and confirm the still-pending 2 is consumed and causes Normal → Broken.

## Test H — empty and Wrecked attempts cannot consume the queue

### Empty control

1. Leave a Normal or Broken Test Musket empty.
2. Queue natural 2.
3. Attempt one Test Musket attack.

Required result:

- attack enforcement increments `emptyRejected` and forces a miss;
- `pendingForcedRoll` remains `2`;
- no natural-roll or condition-transition counter changes; and
- no inventory ammunition is consumed.

Reload that same non-Wrecked item and attack once. The pending 2 must then be applied to the eligible roll.

### Wrecked control

1. Use the Wrecked Test Musket from Test D.
2. Queue natural 2.
3. Attempt one attack.

Required result:

- attack enforcement increments `wreckedRejected` and forces a miss;
- the item remains empty/Wrecked;
- `pendingForcedRoll` remains `2`;
- no natural-roll or condition-transition counter changes; and
- faults remain zero.

Cancel the queued value before continuing.

## Test I — eligible completion without a natural d20

This path occurs only when Kingmaker completes a successfully discharged eligible attack before assigning the main `Roll`, such as an earlier concealment/miss-chance termination or a native auto-hit path.

1. Queue natural 2.
2. Produce one eligible loaded Test Musket attack that ends before main-roll assignment.
3. Inspect diagnostics.

Required result when the path is reached:

- attack enforcement reports one fired round;
- `noNaturalRoll` increments by one;
- `naturalRolls`, `misfires`, `ordinary`, `normalToBroken`, and `brokenToWrecked` do not increment for that attack;
- `pendingForcedRoll` remains `2`; and
- no condition damage occurs.

Then make a normal eligible Test Musket attack and confirm the preserved 2 is consumed. If the chosen encounter cannot produce this native path, preserve the save and capture that limitation rather than treating an ordinary natural roll as proof.

## Test J — quicksave and full restart persistence

### Broken persistence

1. Produce an empty/Broken item through a forced misfire.
2. Quicksave without loading a save.
3. Print the item state.
4. Save normally, exit Kingmaker completely to desktop, restart, and load.
5. Print the same item state and repository identity.

Required result:

- the item remains empty/Broken after quicksave and after full restart;
- token reconciliation reports `conflicts=0; faults=0`;
- the Broken token is preserved or unambiguously restored by the existing weapon-only reconciliation path; and
- no duplicate item-state entry appears.

### Wrecked persistence

1. Produce an empty/Wrecked item through a second forced misfire while Broken.
2. Repeat quicksave and full save/exit/restart/load.

Required result:

- the exact item remains empty/Wrecked;
- reload remains unavailable;
- attack enforcement still rejects it as Wrecked;
- token reconciliation reports zero conflicts and faults; and
- no bootstrap, Harmony, or repository fault appears.

The forced-roll queue is process-local and must return to `<none>` after a complete process restart.

## Combined pass gate for Sprint 25

Version 0.0.24 passes only when all of the following are observed:

- forced 1 and 2 are misfires and force a miss;
- forced 3 and 20 are ordinary and preserve Kingmaker's native result;
- Normal misfire commits exactly one Normal → Broken transition on the exact firing item;
- Broken misfire commits exactly one Broken → Wrecked transition on the exact firing item;
- every eligible attack consumes exactly one loaded round before condition evaluation and no attack-time inventory ammunition;
- all post-shot and post-transition states remain empty;
- a second identical Test Musket remains unchanged when another item misfires;
- native Heavy Crossbows, empty firearms, and Wrecked firearms do not consume a pending forced roll or enter condition diagnostics;
- an observed `noNaturalRoll` completion preserves the pending forced roll and applies no condition damage;
- empty/Broken and empty/Wrecked states survive quicksave and complete save/exit/restart/load;
- duplicate assignment and evaluation counters remain zero; and
- bootstrap, attack-enforcement, AC, reload, token-reconciliation, misfire, repository, and Harmony faults remain zero.

Do not begin explosion/damage behavior for a second misfire while Broken until this complete combined gate passes.
