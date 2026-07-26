# Kingmaker Gunslinger 0.0.22.1 Sprint 22 repair smoke test

Use only a disposable campaign and disposable saves.

## What this build repairs

Version 0.0.22.1 does not add misfires. It repairs two failures observed in 0.0.22:

1. the Kingmaker rule-event hooks were matched as zero-argument methods, but the installed methods take one `RulebookEventContext`, so loaded-round enforcement never ran; and
2. the native item-reconciliation prefix inspected non-weapon items and faulted on `ItemEntityShield`.

The decisive pass condition is no longer merely “the gun attacks.” The exact firing Test Musket must become empty, the attack diagnostics must advance, and reconciliation must remain fault-free.

The Test Musket still displays and animates as a Heavy Crossbow. Black Powder Charge and Lead Ball still use placeholder Diamond Dust artwork.

## Install

1. Exit Kingmaker completely.
2. Install `KingmakerGunslinger-0.0.22.1-attack-hook-repair-smoke-test.zip` through Unity Mod Manager over 0.0.22.
3. Start Kingmaker and confirm Unity Mod Manager shows **Kingmaker Gunslinger 0.0.22.1** with a green status indicator.
4. Load only a disposable campaign.
5. Select exactly one character and click **Grant Firearm Proficiency to selected unit**. Repeating this development command is safe and also restores the reload ability if necessary.

Do not continue if UMM reports a red status, a Harmony patch error, or a KMG bootstrap fault.

## Prepare one clean Test Musket

1. Equip exactly one Test Musket—the custom item still presented as a Heavy Crossbow.
2. Click **Reset first equipped firearm to empty / normal**.
3. Add 20 Black Powder Charges and 20 Lead Balls, or record the exact current counts.
4. Click **Print Reload Test Musket readiness**.

Expected readiness includes:

```text
hasReloadAbility=True
available=True
rounds=0
condition=Normal
```

Record the initial `Firearm attack enforcement` and `State-token native reconciliation` lines.

## Reload through the real ability

1. Use **Reload Test Musket** in a context where a full-round action can finish.
2. Let delivery complete.
3. Print equipped-firearm state, reload readiness, and basic-ammunition counts.

Expected state:

```text
rounds=1
ammunition=kmg.debug.lead-ball
condition=Normal
```

Expected inventory delta: exactly one Black Powder Charge and one Lead Ball consumed. The reload ability must now be unavailable.

## Quicksave regression and shield-fault check

With the same gun still loaded:

1. Quicksave.
2. Do not reload or load another save.
3. Immediately print equipped-firearm state and reload readiness.
4. Inspect `State-token native reconciliation`.

Required result:

- the exact gun still reports `rounds=1`, `condition=Normal`;
- reload remains unavailable;
- inventory counts do not change;
- `conflicts=0` and `faults=0`; and
- no last-fault text mentions `ItemEntityShield` or another non-weapon item.

The reconciliation call count may be much lower than 0.0.22 because non-weapons are intentionally ignored.

## Complete save/restart durability

1. Save under a new disposable name while the gun remains loaded.
2. Exit Kingmaker completely to the desktop.
3. Restart Kingmaker and load that save.
4. Print equipped-firearm state, reload readiness, and ammunition counts.

Required result: the same exact gun is still Loaded/Normal, reload remains unavailable, counts are unchanged from immediately before the save, and reconciliation still reports no conflict or fault.

## Fire one loaded shot — primary repair test

1. Record the current powder and Lead Ball counts.
2. Record all fields on `Firearm attack enforcement`.
3. Make one ordinary ranged attack with the loaded Test Musket. A hit or miss is acceptable.
4. Print equipped-firearm state, reload readiness, and ammunition counts.
5. Re-read `Firearm attack enforcement`.

Required item state:

```text
rounds=0
ammunition=<none>
condition=Normal
```

Required diagnostics and inventory behavior:

- `observed` increases by at least one;
- `fired` increases by exactly one for the shot;
- `faults` remains zero;
- reload becomes available again;
- Black Powder Charge and Lead Ball counts are unchanged by firing; and
- the native Kingmaker attack still completes through the ordinary weapon pipeline.

**Immediate failure:** the item remains at `rounds=1`, `observed` does not increase, `fired` does not increase, or reload remains unavailable.

## Attempt one empty shot

Without reloading, order one more ordinary attack with the same Test Musket.

Required result:

- the attack is forced to miss even if another effect would auto-hit;
- state remains Empty/Normal;
- inventory ammunition remains unchanged;
- `emptyRejected` increases by exactly one; and
- `fired` does not increase.

The placeholder Heavy Crossbow animation or projectile may still play; the rule result and counters are authoritative.

## Loaded Broken and Wrecked checks

These are blocking Sprint 23 controls, not misfire implementation.

### Loaded Broken

1. Reset the equipped Test Musket to Empty/Normal.
2. Load one debug round or reload through the real ability.
3. Click **Apply misfire damage to first equipped firearm** once.
4. Verify `rounds=1`, `condition=Broken`.
5. Make one ordinary attack.

Required result: `fired` increases by one, the item becomes `rounds=0`, and `condition=Broken` is retained.

### Wrecked

1. Prepare a Broken Test Musket, then click **Apply misfire damage to first equipped firearm** again so it becomes Wrecked.
2. Verify it is empty and Wrecked.
3. Make one ordinary attack.

Required result: the attack is forced to miss, `wreckedRejected` increases by one, and the state remains Wrecked.

## Native Heavy Crossbow negative control

1. Record `fired`, `emptyRejected`, and `wreckedRejected`.
2. Equip an actual native Heavy Crossbow—not the custom Test Musket with borrowed presentation.
3. Make one ordinary attack.

Required result:

- the native Heavy Crossbow attacks normally; and
- `fired`, `emptyRejected`, and `wreckedRejected` do not increase.

`observed` and `ignored` may increase because the exact rule event is seen and rejected from the firearm path. That is expected isolation behavior.

## Final blocking conditions

Do not approve Sprint 23 while any of these is true:

- a loaded shot leaves the Test Musket loaded;
- an empty or Wrecked Test Musket can produce a successful attack result;
- powder or Lead Ball counts change when firing;
- native Heavy Crossbows are treated as firearms;
- `duplicateEvents` corresponds to more than one consumed round for one attack;
- any bootstrap, Harmony, firearm-state, reconciliation, or attack-enforcement fault appears; or
- the `ItemEntityShield` exception recurs.

## Failure evidence

Preserve screenshots showing:

- **Last result**;
- the exact equipped-firearm state;
- reload readiness;
- ammunition counts;
- `Firearm attack enforcement`; and
- `State-token native reconciliation`.

For a crash, red UMM status, patch skip, conflict, or fault, also preserve the relevant `[KMG]` lines from Unity Mod Manager's **Logs** tab and `output_log.txt` when present.
