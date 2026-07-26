# Kingmaker Gunslinger 0.0.23 Sprint 23 natural-roll misfire smoke test

## Purpose

This build tests one bounded rule only: after an exact marked firearm successfully discharges a loaded round, read the final main natural d20 used by `RuleAttackRoll`, compare it with the firearm's configured misfire value, and force a misfire to miss. The Test Musket misfires on natural **1-2**.

Version 0.0.23 intentionally does **not** apply Normal → Broken or Broken → Wrecked condition transitions. A detected misfire must leave the pre-attack firearm condition unchanged. It also does not add explosions, area damage, automatic reloads, Rapid Reload, or additional firearm content.

Use only a disposable campaign.

## Install

1. Exit Kingmaker completely.
2. Install `KingmakerGunslinger-0.0.23-natural-roll-misfire-smoke-test.zip` through Unity Mod Manager over the previous build.
3. Start Kingmaker and confirm Unity Mod Manager shows **Kingmaker Gunslinger 0.0.23** with a green status indicator.
4. Open the mod panel and confirm:
   - `Blueprint state: initialized.`
   - `Natural-roll misfires` shows `faults=0` and `pendingForcedRoll=<none>`.
   - firearm attack enforcement, firearm AC, reload, and token reconciliation show zero faults.
5. Use the mod controls to grant Firearm Proficiency, add/equip one Test Musket, add basic ammunition, and load the Test Musket.

The Test Musket still displays as a Heavy Crossbow and the ammunition still uses placeholder Diamond Dust artwork.

## Diagnostic counters

The `Natural-roll misfires` line reports:

- `eligible`: successfully discharged exact-firearm attacks registered for misfire evaluation;
- `naturalRolls`: eligible attacks that reached the exact main `Roll` assignment;
- `ordinary`: rolls above the configured misfire threshold;
- `misfires`: rolls within the configured misfire threshold;
- `forcedApplied`: queued development rolls actually applied;
- `duplicateAssignments` and `duplicateEvaluations`: duplicate callbacks, expected to remain zero;
- `noNaturalRoll`: eligible attacks that completed before a natural d20 was assigned;
- `faults`: any misfire-hook or evaluation failure, expected to remain zero; and
- `pendingForcedRoll`: the process-local queued value, or `<none>`.

An auto-hit or an attack terminated before the main roll can increment `noNaturalRoll`. That is not itself a failure: no natural roll existed to classify, and the queued forced value must remain pending.

## Test A — forced natural 2 detects a misfire

1. Equip a loaded, Normal Test Musket.
2. Record the firearm attack-enforcement and natural-roll counters.
3. Click **Force next eligible firearm natural d20 to 2**.
4. Confirm `pendingForcedRoll=2`.
5. Make one ordinary Test Musket attack against a valid target.
6. Reopen the mod panel and print the equipped-firearm state diagnostics.

Required result:

- attack enforcement increments `fired` exactly once;
- the exact Test Musket changes from `rounds=1` to `rounds=0`;
- no additional Black Powder Charge or Lead Ball is consumed by firing;
- `eligible`, `naturalRolls`, `misfires`, and `forcedApplied` each increment by exactly one;
- `pendingForcedRoll` returns to `<none>`;
- the last misfire result reports `naturalD20=2`, `misfireRange=1-2`, `misfired=True`, and `finalSuccess=False`;
- Kingmaker resolves the attack as a miss even if its total attack bonus would otherwise hit;
- firearm condition remains Normal; and
- all relevant fault and duplicate counters remain zero.

## Test B — forced natural 3 is not a misfire

1. Reload the exact Test Musket.
2. Click **Force next eligible firearm natural d20 to 3**.
3. Make one ordinary Test Musket attack.
4. Inspect the diagnostics and firearm state.

Required result:

- one loaded round is consumed exactly once;
- `eligible`, `naturalRolls`, `ordinary`, and `forcedApplied` each increment by one;
- `misfires` does not increment;
- the last result reports `naturalD20=3`, `misfired=False`, and leaves Kingmaker's native success result unchanged;
- `pendingForcedRoll=<none>`;
- condition remains unchanged; and
- faults and duplicates remain zero.

The attack can still miss if natural 3 plus the native attack total does not reach the target AC. This test proves non-misfire classification, not a guaranteed hit.

## Test C — forced natural 1 and 20 boundary controls

Repeat with a freshly loaded Test Musket:

1. Force natural 1 and attack. Required: one new misfire, forced miss, one round consumed, condition unchanged.
2. Reload, force natural 20, and attack. Required: one new ordinary result, no new misfire, one round consumed, condition unchanged. Kingmaker remains responsible for its ordinary natural-20 and critical behavior.

## Test D — native Heavy Crossbow cannot consume the queue

1. Queue forced natural 2 and confirm `pendingForcedRoll=2`.
2. Unequip the Test Musket and equip a genuine native Heavy Crossbow.
3. Make one native Heavy Crossbow attack.
4. Inspect the mod panel before re-equipping the Test Musket.

Required result:

- selected-unit diagnostics report `equippedFirearms=none detected` while the native Heavy Crossbow is equipped;
- `pendingForcedRoll` remains `2`;
- firearm `eligible`, `naturalRolls`, `ordinary`, `misfires`, and `forcedApplied` do not change from the native attack;
- the native attack resolves normally; and
- no firearm or Harmony fault appears.

Then re-equip and load the Test Musket, make one eligible attack, and confirm that the still-pending 2 is consumed by that firearm roll.

## Test E — empty and Wrecked attempts cannot consume the queue

### Empty control

1. Leave the Test Musket empty.
2. Queue forced natural 2.
3. Attempt one Test Musket attack.

Required result:

- attack enforcement increments `emptyRejected` and forces the attack to miss;
- `pendingForcedRoll` remains `2`;
- no misfire counter changes; and
- no inventory ammunition is consumed.

Reload and fire once to confirm the pending value is then applied to the eligible roll.

### Wrecked control

1. Use the development-only misfire-damage control to make an empty Test Musket Wrecked.
2. Queue forced natural 2.
3. Attempt one Test Musket attack.

Required result:

- attack enforcement increments `wreckedRejected` and forces a miss;
- the firearm remains Wrecked;
- `pendingForcedRoll` remains `2`;
- no misfire counter changes; and
- faults remain zero.

Cancel the queued value or replace the disposable Test Musket before continuing.

## Test F — save/restart regression

1. Return to an ordinary Test Musket state, load it, and quicksave.
2. Confirm the loaded token remains present and token reconciliation reports `conflicts=0; faults=0`.
3. Make one forced-2 misfire and confirm the item is empty with its original condition unchanged.
4. Save, exit completely to desktop, restart Kingmaker, and load the save.
5. Confirm the item-owned state is still empty with the same condition and no token-reconciliation fault.

The forced-roll queue is intentionally process-local and must not survive a complete process restart.

## Pass gate for Sprint 24

Version 0.0.23 passes only when all of the following are observed:

- forced 1 and 2 are classified as misfires and forced to miss;
- forced 3 and 20 are classified as ordinary and preserve Kingmaker's native result;
- each eligible attack consumes exactly one already-loaded round and no extra inventory ammunition;
- firearm condition is unchanged by every Sprint 23 misfire;
- native Heavy Crossbows, empty firearms, and Wrecked firearms do not consume a pending forced roll;
- attacks that complete without a natural d20 preserve the pending forced roll and are reported through `noNaturalRoll`;
- save/restart behavior from Sprint 22 remains intact;
- duplicate counters remain zero; and
- misfire, attack-enforcement, AC, reload, token-reconciliation, bootstrap, and Harmony faults remain zero.

Do not begin automatic condition transitions unless this complete live gate passes.
