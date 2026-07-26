# Kingmaker Gunslinger 0.0.27 smoke-test guide

Build: `0.0.27-s27-item-lifecycle-recovery-contract`  
Use only a disposable Pathfinder: Kingmaker 2.1.7b campaign.

## Purpose

Sprint 27 does not add player-facing repair. It qualifies one exact-item lifecycle decision:

- a second-misfire firearm remains present as empty/Wrecked;
- a development-only overhaul changes that same item to empty/Broken;
- exact repository and runtime reference identity survive;
- a second blueprint-identical firearm remains unchanged; and
- accidental destructive cleanup now requires an explicit second confirmation.

The Test Musket still displays as a Heavy Crossbow and ammunition still uses placeholder Diamond Dust artwork.

## 1. Install and prepare

1. Exit Kingmaker completely to the desktop.
2. Install only the standalone `KingmakerGunslinger-0.0.27-item-lifecycle-recovery-contract-smoke-test.zip` through Unity Mod Manager, replacing 0.0.26.
3. Start Kingmaker.
4. Confirm UMM shows **Kingmaker Gunslinger 0.0.27** with a green status indicator and no bootstrap or Harmony error.
5. Load only a disposable campaign.
6. Select the firearm test character and grant Firearm Proficiency if needed.
7. Add two Test Muskets to shared inventory.
8. Add at least two Black Powder Charges and two Lead Balls.
9. Enable firearm combat tracing.
10. Equip exactly one Test Musket and leave the other in shared inventory.

Click **Print visible firearm states (equipment + shared inventory)** and capture both items. Required baseline:

```text
visibleFirearms=2
```

Record for each item:

```text
repositoryIdentity
referenceHash
revision
rounds
condition
```

The two items must have distinct runtime references/repository identities.

## 2. Confirm the explosion still retains the Wrecked item

Starting with the equipped firearm empty/Normal:

1. Reload it.
2. Force eligible firearm natural d20 `1` and attack once.
3. Confirm the exact item becomes empty/Broken and the first misfire applies no burst.
4. Reload while Broken.
5. Force eligible firearm natural d20 `1` or `2` and attack again.
6. Allow the native five-foot burst to complete.
7. Print visible firearm states.

Required result:

- the exact firing item is still present;
- it reports `rounds=0`, `ammunition=<none>`, `condition=Wrecked`;
- the second Test Musket is still present and is not Wrecked;
- the two item identities remain distinct;
- no automatic inventory removal or replacement occurred; and
- burst, target, misfire, attack, reload, AC, trace, and token faults are zero.

Record the Wrecked item's exact identity evidence before continuing.

## 3. Run the same-item overhaul probe

1. Keep the exact Wrecked Test Musket equipped.
2. Click **Overhaul first equipped Wrecked firearm to Broken (contract test)** exactly once.
3. Read the complete `Last result` line.
4. Print equipped and visible firearm states again.

Required success record:

```text
repositoryIdentity=<same value as before>
referenceHash=<same value as before>
revision=<before>-><before+1>
stateBefore=[schema=1; rounds=0; ammunition=<none>; condition=Wrecked]
stateAfter=[schema=1; rounds=0; ammunition=<none>; condition=Broken]
```

Required inventory/state behavior:

- visible firearm count remains two;
- the exact runtime reference remains the same;
- no weapon item is removed or added;
- no Black Powder Charge or Lead Ball is consumed or created;
- only the equipped Wrecked item changes;
- the other Test Musket remains unchanged; and
- the result explicitly says the item was not removed, replaced, or silently repaired to Normal.

Any changed repository identity/reference hash, revision jump other than exactly one, missing second item, or final Normal state is a failure.

## 4. Verify rejection on non-Wrecked states

1. With the overhauled item now Broken, click the overhaul control again.
2. The command must fail closed with a reason equivalent to:

```text
Only a wrecked firearm can use the same-item overhaul transition.
```

3. Confirm revision, state, and inventory counts are unchanged.
4. Repeat on an empty/Normal Test Musket when practical.

A Normal or Broken item must never be changed by the Wrecked-only overhaul command.

## 5. Verify Broken reload remains separate from overhaul

1. Keep the overhauled empty/Broken firearm equipped.
2. Click **Print Reload Test Musket readiness**.
3. Confirm reload is available and states that the firearm will remain Broken.
4. Record powder and Lead Ball counts.
5. Use **Reload Test Musket** or the immediate diagnostic reload.
6. Confirm:

```text
rounds=1
condition=Broken
Black Powder Charge: -1
Lead Ball: -1
```

The overhaul itself must not have paid this ammunition cost. Reload remains the only operation in this sequence that consumes the component pair.

The optional development command **Repair first equipped Broken firearm to Normal (diagnostic)** may be used only after the identity/persistence evidence is captured. It represents the separate ordinary repair boundary and is not player-facing gameplay.

## 6. Verify cleanup confirmation safety

Create or retain at least one unequipped Test Musket in shared inventory.

1. Record visible firearm count and repository removal count.
2. Click **Arm removal of ALL unequipped Test Muskets (destructive)**.
3. Confirm the warning and separate confirmation/cancel buttons appear.
4. Do **not** click the confirmation.
5. Click **Cancel Test Musket removal**.
6. Print visible firearm states again.

Required result:

- all Test Muskets remain present;
- repository removal count is unchanged;
- no state revision changes; and
- `Last result` states that no inventory or state mutation was requested.

The explicit **CONFIRM remove ALL unequipped Test Muskets** button is destructive test cleanup. Do not use it during acceptance testing.

## 7. Verify persistence of the overhauled exact item

Return the exact test item to empty/Broken if necessary. Record its current in-process repository identity, runtime reference hash, revision, visible location, and state. The first two values are correlation labels for the current process, not durable item identifiers.

1. Quicksave.
2. Confirm the item remains empty/Broken immediately afterward.
3. Make a normal save.
4. Exit Kingmaker completely to the desktop.
5. Restart Kingmaker and load that save.
6. Re-equip or inspect the same visible Test Musket.
7. Print equipped and visible firearm states.

Required persistent result:

```text
rounds=0
ammunition=<none>
condition=Broken
state-token conflicts=0
state-token faults=0
```

Both the process-local repository identity and runtime reference hash may be reassigned after a full restart. Do not compare either value across processes. The item-owned token state must remain empty/Broken on the intended visible firearm, while the second Test Musket retains its independent state.

## 8. Final regression and fault review

Before ending the test, confirm:

```text
Firearm attack enforcement faults=0
Natural-roll misfire faults=0
Second-misfire explosion faults=0
targetFaults=0
Reload runtime faults=0
Firearm AC faults=0
Combat trace faults=0
State-token reconciliation conflicts=0
State-token reconciliation faults=0
```

Also confirm all duplicate-application counters remain zero except `targetDuplicates`, which may reflect harmless native-query duplicate candidates that were deduplicated before delivery.

## Blocking failures

Stop and report the exact diagnostics if any of these occurs:

- the Wrecked firearm disappears automatically;
- overhaul changes or replaces the wrong Test Musket;
- repository identity or in-process reference changes during overhaul;
- revision does not advance exactly once;
- overhaul produces Normal or a loaded state;
- overhaul consumes or creates ammunition;
- the second Test Musket changes;
- arming or cancelling cleanup removes anything;
- the overhauled state fails to survive save/restart; or
- any relevant fault, conflict, or duplicate-application counter becomes nonzero.

Sprint 28 remains blocked until this exact 0.0.27 gate passes.
