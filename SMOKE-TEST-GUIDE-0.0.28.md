# Kingmaker Gunslinger 0.0.28 smoke-test guide

Build: `0.0.28-s28-player-facing-overhaul`  
Use only a disposable Pathfinder: Kingmaker 2.1.7b campaign.

## Purpose

Sprint 28 exposes the already-qualified same-item recovery transition as a real ability:

```text
exactly one equipped empty/Wrecked Test Musket
+ one Firearm Repair Kit
+ completed full-round Overhaul Test Musket
= the same exact firearm becomes empty/Broken
```

The ability must preserve the item, consume no ammunition, and stop at Broken. Ordinary Broken-to-Normal repair remains separate.

The Test Musket still displays as a Heavy Crossbow. Black Powder Charges, Lead Balls, and Firearm Repair Kits still use placeholder presentation derived from native items.

## 1. Install and initialize

1. Exit Kingmaker completely to the desktop.
2. Install only `KingmakerGunslinger-0.0.28-player-facing-overhaul-smoke-test.zip` through Unity Mod Manager, replacing 0.0.27.
3. Start Kingmaker.
4. Confirm UMM shows **Kingmaker Gunslinger 0.0.28**, a green status indicator, and no bootstrap or Harmony error.
5. Load only a disposable campaign.
6. Select the test character.
7. Click **Grant Firearm Proficiency to selected unit** even if the character already had it. The result must verify that both Reload and Overhaul abilities are present.
8. Add two Test Muskets to shared inventory.
9. Equip exactly one and leave the other in shared inventory.
10. Enable firearm combat tracing.
11. Click **Print visible firearm states (equipment + shared inventory)**.

Required baseline:

```text
visibleFirearms=2
```

Record for each musket:

```text
repositoryIdentity
referenceHash
revision
rounds
condition
```

The two items must have distinct in-process identities.

## 2. Create a fast Wrecked fixture

This setup uses the accepted direct condition diagnostics so the Sprint 28 test does not require replaying the complete two-misfire burst sequence.

1. Keep the intended test musket equipped.
2. Click **Reset first equipped firearm to empty / normal**.
3. Click **Apply misfire damage to first equipped firearm** once. Confirm it becomes empty/Broken.
4. Click **Apply misfire damage to first equipped firearm** a second time. Confirm it becomes empty/Wrecked.
5. Print visible firearm states again.

Required result:

- the intended item is `rounds=0`, `ammunition=<none>`, `condition=Wrecked`;
- the second musket remains Normal;
- `visibleFirearms=2`; and
- no item was removed or replaced.

Record the Wrecked item’s repository identity, reference hash, and revision.

## 3. Verify missing-kit availability

1. Click **Remove all Firearm Repair Kits from shared inventory**.
2. Click **Print Firearm Repair Kit count** and confirm `repairKits=0`.
3. Click **Print Overhaul Test Musket readiness**.

Required result:

```text
available=False
reason=One Firearm Repair Kit is required.
```

The action-bar Overhaul ability should be unavailable. Confirm:

- firearm state and revision are unchanged;
- no ammunition count changed; and
- overhaul runtime counters did not report a completed transaction.

## 4. Verify non-Wrecked rejection

1. Temporarily use the development-only same-item contract control or direct reset so the equipped item is empty/Broken.
2. Add one Firearm Repair Kit.
3. Print overhaul readiness.

Required result:

```text
available=False
```

with a reason stating that only a Wrecked Test Musket can be overhauled and that Broken-to-Normal repair is separate.

4. Return the same item to empty/Wrecked using **Apply misfire damage** once.
5. Confirm the kit is still present. Rejected readiness checks must consume nothing.

## 5. Verify interruption before delivery

1. Confirm the exact equipped item is empty/Wrecked and exactly one Firearm Repair Kit is present.
2. Enter combat or another situation where the full-round action visibly remains in progress long enough to cancel.
3. Start the action-bar **Overhaul Test Musket** ability.
4. Before delivery completes, cancel the current command or issue movement/another command that interrupts it.
5. Reopen the mod panel and print firearm state and repair-kit count.

Required result:

```text
condition=Wrecked
rounds=0
repairKits=1
```

Also required:

- repository revision is unchanged;
- `Overhaul runtime completed` did not increase;
- no fault was recorded; and
- the second musket remains unchanged.

If the game completes the full-round delivery before cancellation can be staged, reload the pre-test save and retry while paused in combat. Do not substitute the immediate diagnostic control for this interruption test.

## 6. Complete the player-facing overhaul

1. Confirm exactly one Test Musket is equipped, it is empty/Wrecked, and exactly one Firearm Repair Kit is present.
2. Record:
   - Wrecked item repository identity;
   - reference hash;
   - revision;
   - repair-kit count;
   - Black Powder Charge count;
   - Lead Ball count; and
   - second musket state.
3. Use the action-bar **Overhaul Test Musket** ability.
4. Allow the full-round action and delivery to finish.
5. Reopen UMM.
6. Print the equipped state, visible firearm states, repair-kit count, and overhaul readiness.

Required transaction result:

```text
status=Overhauled
beforeState=[schema=1; rounds=0; ammunition=<none>; condition=Wrecked]
afterState=[schema=1; rounds=0; ammunition=<none>; condition=Broken]
beforeInventory=[repairKits=1]
afterInventory=[repairKits=0]
repositoryIdentity=<same in-process value>
referenceHash=<same in-process value>
revision=<before>-><before+1>
exactItemPreserved=True
```

Required broader behavior:

- the exact same item remains equipped;
- `visibleFirearms=2`;
- the second musket remains unchanged;
- no weapon item is added or removed;
- Black Powder Charge count is unchanged;
- Lead Ball count is unchanged;
- final state is empty/Broken, not Normal and not loaded;
- `Overhaul runtime attempts +1` and `completed +1`;
- `rejected` and `faults` do not increase.

## 7. Verify repeat-use rejection

1. Leave the overhauled empty/Broken item equipped.
2. Add one Firearm Repair Kit.
3. Print overhaul readiness.
4. Attempt to use Overhaul Test Musket.

Required result:

- readiness is unavailable because the firearm is not Wrecked;
- the kit remains present;
- state and revision remain unchanged;
- the firearm remains empty/Broken; and
- no completed overhaul is recorded.

Remove the extra repair kit afterward if desired.

## 8. Verify recovery-stage separation

1. Keep the exact overhauled empty/Broken firearm equipped.
2. Add one Black Powder Charge and one Lead Ball if needed.
3. Print Reload Test Musket readiness.
4. Confirm reload is available and says the firearm will remain Broken.
5. Do not reload until the persistence evidence below is captured, unless you have a separate save.

This proves Overhaul did not silently perform ordinary repair. The player-facing Broken-to-Normal repair boundary is not part of 0.0.28.

## 9. Verify persistence

1. With the exact overhauled item empty/Broken, quicksave.
2. Print its state and confirm it remains empty/Broken.
3. Make a normal save.
4. Exit Kingmaker completely to the desktop.
5. Restart Kingmaker and load that save.
6. Re-equip or inspect the intended Test Musket.
7. Print visible firearm states and reconciliation diagnostics.

Required persistent result:

```text
rounds=0
ammunition=<none>
condition=Broken
state-token conflicts=0
state-token faults=0
```

Repository identity and reference hash are process-local and may be reassigned after restart. The intended visible item’s token-backed state and the second item’s independent state are the durable evidence.

## 10. Final fault review

Before ending the test, confirm:

```text
Overhaul runtime faults=0
Reload runtime faults=0
Firearm attack enforcement faults=0
Natural-roll misfire faults=0
Second-misfire explosion faults=0
targetFaults=0
Firearm AC faults=0
Combat trace faults=0
State-token reconciliation conflicts=0
State-token reconciliation faults=0
```

All duplicate-application counters must remain zero except harmless native burst-query candidate deduplication already accepted in Sprint 26.

## Blocking failures

Stop and report exact diagnostics if:

- the action is available without a repair kit;
- Normal or Broken firearms are accepted;
- cancelling before delivery consumes a kit or changes state;
- completed delivery consumes anything other than exactly one repair kit;
- the item identity changes or a replacement item appears;
- the revision changes by anything other than one;
- the wrong Test Musket changes;
- the result is Normal, loaded, missing, or still Wrecked;
- powder or Lead Balls change during Overhaul;
- save/restart loses empty/Broken state; or
- any relevant fault, conflict, or duplicate-application counter becomes nonzero.

Sprint 29 remains blocked until this exact 0.0.28 gate passes.
