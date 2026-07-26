# Kingmaker Gunslinger 0.0.22 smoke test

Use only a disposable campaign and disposable saves.

## What this build is testing

Version 0.0.22 tests two related changes:

1. the loaded-state token must survive quicksave and Kingmaker's native item-enchantment refresh; and
2. an ordinary Test Musket attack must consume one loaded round, while an empty Test Musket must be forced to miss.

The Test Musket still looks and animates like a Heavy Crossbow. Black Powder Charge and Lead Ball still use temporary Diamond Dust artwork.

## Install

1. Exit Kingmaker completely.
2. Install `KingmakerGunslinger-0.0.22-loaded-round-smoke-test.zip` through Unity Mod Manager over version 0.0.21.
3. Start Kingmaker and confirm Unity Mod Manager shows **Kingmaker Gunslinger 0.0.22** with a green status indicator.
4. Load the disposable campaign used for the reload test.
5. Select exactly one character with Firearm Proficiency. Clicking **Grant Firearm Proficiency to selected unit** again is safe and will also restore the reload ability if necessary.

## Prepare one clean Test Musket

1. Equip exactly one Test Musket—the custom item still displayed as a Heavy Crossbow.
2. Click **Reset first equipped firearm to empty / normal**.
3. Add 20 Black Powder Charges and 20 Lead Balls.
4. Click **Print Reload Test Musket readiness**.

Expected readiness includes:

```text
hasReloadAbility=True
available=True
rounds=0
condition=Normal
blackPowder=20
leadBalls=20
```

## Reload through the real ability

1. Use **Reload Test Musket** in a context where the action can execute.
2. Allow the full-round delivery to finish.
3. Print the equipped-firearm state and basic ammunition counts.

Expected result:

```text
rounds=1
ammunition=kmg.debug.lead-ball
condition=Normal
blackPowder=19
leadBalls=19
```

The reload ability should now be unavailable.

## Critical quicksave regression test

With that same gun still loaded:

1. Quicksave.
2. Do **not** reload or load a save.
3. Immediately print the equipped-firearm state and reload readiness again.

Expected:

- the firearm still has `rounds=1`;
- its condition remains `Normal`;
- the reload ability remains unavailable;
- Black Powder Charge and Lead Ball remain 19/19; and
- `State-token native reconciliation` reports no conflict or fault.

Either of these diagnostic outcomes is acceptable:

- the token was preserved natively; or
- one exact token was restored after native removal.

The important result is that the firearm remains loaded.

## Complete save/restart durability

1. Save under a new disposable name while the gun remains loaded.
2. Exit Kingmaker completely to the desktop.
3. Restart Kingmaker and load that save.
4. Print equipped-firearm state, reload readiness, and ammunition counts.

Expected: the same gun is still Loaded / Normal, reload remains unavailable, and counts remain 19/19.

## Fire one loaded shot

1. Record the current Black Powder Charge and Lead Ball counts.
2. Make one ordinary ranged attack with the loaded Test Musket.
3. Print the firearm state, reload readiness, and ammunition counts.

Expected:

```text
rounds=0
ammunition=<none>
condition=Normal
```

Also expected:

- the ordinary Kingmaker attack proceeds through its normal attack and damage pipeline;
- `Firearm attack enforcement` reports one additional fired round;
- the reload ability becomes available again; and
- inventory powder and Lead Balls remain unchanged at 19/19.

The loaded round was already represented by the item-owned state. Firing must not consume another powder charge or Lead Ball.

## Attempt one empty shot

Without reloading, order another ordinary attack with the same Test Musket.

Expected:

- the attack may still animate and launch the placeholder crossbow presentation;
- the attack roll is forced to miss;
- firearm state remains empty/Normal;
- inventory ammunition remains unchanged; and
- `emptyRejected` increases by one in `Firearm attack enforcement`.

## Native Heavy Crossbow negative control

1. Record the `fired`, `emptyRejected`, and `wreckedRejected` counters.
2. Equip an actual native Heavy Crossbow, not a Test Musket.
3. Make one ordinary attack.

Expected:

- the native Heavy Crossbow attacks normally;
- no loaded-state restriction is applied; and
- the firearm `fired`, `emptyRejected`, and `wreckedRejected` counters do not increase.

The general `ignored` counter may increase because a non-firearm attack-roll object was observed and deliberately rejected from the firearm path.

## Optional Broken-firearm check

A loaded Broken Test Musket is currently allowed to discharge and should remain Broken after its one round is consumed. Misfire and explosion behavior are not part of this build.

## Failure evidence

For an ordinary mismatch, preserve screenshots of:

- **Last result**;
- `Firearm attack enforcement`;
- `State-token native reconciliation`;
- the equipped-firearm state; and
- ammunition counts.

For a crash, red UMM status, initialization fault, token conflict, token-reconciliation fault, or attack-enforcement fault, include the relevant `[KMG]` lines from Unity Mod Manager's **Logs** tab and `output_log.txt` when present.
