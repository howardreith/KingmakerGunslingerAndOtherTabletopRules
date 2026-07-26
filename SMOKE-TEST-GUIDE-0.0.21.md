# Kingmaker Gunslinger 0.0.21 smoke test

Use only a disposable campaign and disposable saves.

## Install

1. Exit Kingmaker completely.
2. Install `KingmakerGunslinger-0.0.21-reload-smoke-test.zip` through Unity Mod Manager over version 0.0.20.
3. Start Kingmaker and confirm Unity Mod Manager shows Kingmaker Gunslinger version 0.0.21 with a green status indicator.
4. Load the disposable campaign used for prior firearm tests.

## Restore and verify the ability

1. Select exactly one character who previously received Firearm Proficiency.
2. Open the mod options panel.
3. Click **Grant Firearm Proficiency to selected unit** again.
4. Confirm the result reports a positive proficiency rank and `Reload Test Musket restored=True`.
5. Open that character's abilities. `Reload Test Musket` should be available; it may also auto-fill an action-bar slot.

The Firearm Proficiency feature itself remains intentionally hidden on the character sheet in this technical build.

## Prepare one empty Test Musket

1. Equip exactly one Test Musket—the Heavy Crossbow-looking custom item.
2. Use **Reset first equipped firearm to empty / normal** if this gun came from the earlier A-D fixture.
3. Add 20 Black Powder Charges and 20 Lead Balls.
4. Click **Print Reload Test Musket readiness**.

Expected readiness includes:

```text
hasReloadAbility=True
available=True
state: rounds=0, condition=Normal
blackPowder=20
leadBalls=20
```

## Test the real full-round ability

1. Enter a safe combat or another context where ability actions can execute.
2. Use `Reload Test Musket` on the proficient character.
3. Observe that it occupies a full-round action.
4. After delivery completes, print the equipped firearm state and ammunition counts.

Expected result:

```text
Test Musket: rounds=1; ammunition=kmg.debug.lead-ball; condition=Normal
Black Powder Charge: 19
Lead Ball: 19
Reload runtime: attempts=1; loaded=1; rejected=0; faults=0
```

The exact process-local `kmg-item-*` diagnostic number is not persistent and is irrelevant.

## Diagnostic fallback

When the ability is present but the action cannot be triggered, click:

```text
Reload equipped Test Musket immediately (diagnostic)
```

This bypasses action economy but executes the same cross-resource transaction. A successful diagnostic result indicates that any remaining fault is in ability presentation or command delivery rather than inventory/state logic.

## Rejection tests

### Already loaded

Attempt to use the reload ability again without resetting or firing. It should be unavailable or reject without consuming ammunition. Counts remain 19/19 and the gun remains loaded.

### Missing Lead Ball

1. Reset the gun to empty / normal.
2. Remove all basic ammunition.
3. Add one Black Powder Charge only.
4. Print readiness and attempt reload.

Expected: unavailable/rejected; powder remains 1 and Lead Balls remain 0.

### Missing powder

Repeat with one Lead Ball only. Expected: unavailable/rejected; powder remains 0 and Lead Balls remain 1.

### Broken or wrecked

Apply the existing diagnostic misfire-damage control and print readiness. The reload ability must reject without consuming ammunition.

## Cancellation test

With an empty Normal Test Musket and at least one complete load:

1. Begin `Reload Test Musket`.
2. Cancel or interrupt it before the ability delivery completes.
3. Print state and ammunition counts.

Expected: the gun remains empty and both ammunition counts remain unchanged.

## Save/restart persistence

After one successful reload:

1. Record the loaded state and remaining ammunition counts.
2. Save under a new disposable name.
3. Exit Kingmaker completely.
4. Restart the game and load that save.
5. Print equipped state and ammunition counts again.

Expected: the exact gun remains Loaded / Normal and the counts remain one lower than before the reload.

## Failure evidence

For an ordinary mismatch, provide a screenshot of **Last result** plus the printed state/count diagnostics. For a crash, red UMM status, `ability.failed`, rollback failure, or initialization failure, include the relevant `[KMG]` lines from the Unity Mod Manager Logs tab and `output_log.txt`.
