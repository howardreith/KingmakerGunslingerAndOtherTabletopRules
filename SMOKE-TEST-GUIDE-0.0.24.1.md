# Kingmaker Gunslinger 0.0.24.1 Sprint 24 broken-reload repair smoke test

## Purpose

Version `0.0.24.1-s24-broken-reload-repair` makes the smallest repair required by the failed 0.0.24 runtime gate:

```text
empty / Broken -> reload -> loaded / Broken
```

Reloading must consume exactly one Black Powder Charge and one Lead Ball, load one round on the exact equipped Test Musket, and preserve `condition=Broken`. A Wrecked firearm must remain unavailable.

This repair exists so the already-implemented second-misfire transition can be tested naturally:

```text
loaded / Broken -> forced natural 1 or 2 -> empty / Wrecked
```

No repair gameplay, Quick Clear, explosion, splash damage, Rapid Reload, automatic iterative reload, additional firearm type, or Gunslinger class behavior is added.

Use only a disposable campaign.

## Install

1. Exit Kingmaker completely.
2. Install `KingmakerGunslinger-0.0.24.1-broken-reload-repair-smoke-test.zip` through Unity Mod Manager over 0.0.24.
3. Start Kingmaker and load the disposable save containing the empty/Broken Test Musket from the failed 0.0.24 test.
4. Confirm Unity Mod Manager shows **Kingmaker Gunslinger 0.0.24.1** with a green status indicator.
5. Open the mod panel and confirm:
   - `Blueprint state: initialized.`
   - the title says `0.0.24.1 Sprint 24 broken-reload repair smoke test`;
   - reload, attack enforcement, firearm AC, natural-roll misfires, token reconciliation, and combat tracing each show `faults=0`; and
   - `pendingForcedRoll=<none>`.

The Test Musket still displays as a Heavy Crossbow, and ammunition still uses placeholder Diamond Dust artwork.

## Test 1 — Broken reload readiness

Starting state: exactly one equipped Test Musket with `rounds=0; condition=Broken`, plus at least one Black Powder Charge and one Lead Ball in shared inventory.

1. Click **Print selected unit's equipped-firearm state diagnostics**.
2. Confirm the exact equipped item reports `rounds=0; ammunition=<none>; condition=Broken`.
3. Click **Print Reload Test Musket readiness**.

Required result:

```text
available=True
```

The reason must say that the firearm is ready to load and will remain Broken. No inventory count or firearm state may change during this readiness check.

## Test 2 — Reload the exact Broken Test Musket

1. Record the current Black Powder Charge and Lead Ball counts.
2. Use the action-bar **Reload Test Musket** ability. The panel's **Reload equipped Test Musket immediately (diagnostic)** button is an acceptable fallback if action-bar delivery is inconvenient.
3. Wait for delivery to complete.
4. Click **Print selected unit's equipped-firearm state diagnostics**.
5. Inspect `Reload runtime`.

Required result:

- reload reports `status=Loaded`;
- before state is `rounds=0; condition=Broken`;
- after state is `rounds=1; ammunition=kmg.debug.lead-ball; condition=Broken`;
- exactly one Black Powder Charge and one Lead Ball are consumed;
- `attempts` and `loaded` each increment by one;
- `rejected=0` for this operation;
- `faults=0`; and
- the same repository identity remains equipped.

Failure conditions:

- reload is still unavailable;
- the firearm changes to Normal;
- inventory decreases by anything other than one complete component pair;
- more than one item changes; or
- any reload, repository, token, or Harmony fault appears.

## Test 3 — Broken misfire becomes Wrecked

1. Keep the exact loaded/Broken Test Musket equipped.
2. Queue **Force next eligible firearm natural d20 to 2**.
3. Confirm `pendingForcedRoll=2`.
4. Make one ordinary Test Musket attack against a valid target.
5. Reopen the mod panel.
6. Click **Print selected unit's equipped-firearm state diagnostics**.

Required result:

- firearm attack enforcement increments `fired` exactly once;
- the exact firing item ends at `rounds=0; ammunition=<none>; condition=Wrecked`;
- natural-roll diagnostics increment `eligible`, `naturalRolls`, `misfires`, `brokenToWrecked`, `forcedApplied` by one;
- `normalToBroken` does not increment for this attack;
- the last result reports `naturalD20=2`, `misfired=True`, `finalSuccess=False`, `conditionTransition=BrokenToWrecked`, `conditionBefore=Broken`, and `conditionAfter=Wrecked`;
- `pendingForcedRoll=<none>`;
- the attack is a miss;
- no extra Black Powder Charge or Lead Ball is consumed when firing;
- no explosion or extra wielder damage is applied by the mod in this version; and
- duplicate and fault counters remain zero.

## Test 4 — Wrecked reload remains blocked

1. Record inventory counts with the empty/Wrecked Test Musket equipped.
2. Click **Print Reload Test Musket readiness**.
3. Attempt to activate Reload Test Musket once.
4. Print readiness and item state again.

Required result:

- readiness reports `available=False` with a Wrecked reason;
- state remains `rounds=0; ammunition=<none>; condition=Wrecked`;
- Black Powder Charge and Lead Ball counts do not change;
- reload `loaded` does not increment;
- a rejected attempt may increment `rejected`, but `faults` must remain zero; and
- no second item changes.

## Test 5 — Wrecked attack and forced-roll queue isolation

1. Queue natural 2 while the Wrecked Test Musket remains equipped.
2. Confirm `pendingForcedRoll=2`.
3. Attempt one attack.
4. Inspect diagnostics.

Required result:

- attack enforcement increments `wreckedRejected` and forces a miss;
- the Wrecked item remains unchanged;
- `pendingForcedRoll` remains `2`;
- no natural-roll, misfire, or condition-transition counter increments; and
- faults remain zero.

Click **Cancel pending forced firearm natural roll** before continuing.

## Test 6 — Normal reload regression

1. Equip a separate empty/Normal Test Musket or reset a disposable Test Musket to empty/Normal.
2. Record inventory counts.
3. Reload it once.
4. Print state and reload diagnostics.

Required result:

- it becomes `rounds=1; condition=Normal`;
- exactly one Black Powder Charge and one Lead Ball are consumed;
- the repair does not change the previously working Normal reload path; and
- faults remain zero.

## Test 7 — Exact-item isolation

1. Keep two Test Muskets visible: one equipped and one in shared inventory or another equipment slot.
2. Print visible firearm states and record both repository identities.
3. Reload or misfire only the equipped item.
4. Print visible firearm states again.

Required result:

- only the exact acted-on repository identity changes;
- the other Test Musket remains unchanged; and
- repository conflicts and faults remain zero.

## Test 8 — Save and restart persistence

1. Produce an empty/Broken item, quicksave, and print its state.
2. Save normally, exit Kingmaker completely to desktop, restart, load, and print the same item state.
3. Produce an empty/Wrecked item through the repaired Broken reload plus a forced misfire.
4. Repeat quicksave and complete save/exit/restart/load.

Required result:

- Broken remains empty/Broken after quicksave and full restart;
- Wrecked remains empty/Wrecked after quicksave and full restart;
- Wrecked reload remains unavailable after restart;
- token reconciliation reports `conflicts=0; faults=0`;
- no duplicate state entry appears; and
- the process-local forced-roll queue returns to `<none>` after restart.

## Evidence to capture

Please capture four screenshots or equivalent copied diagnostics:

1. empty/Broken readiness showing `available=True`;
2. successful loaded/Broken state and reload inventory delta;
3. post-attack empty/Wrecked state with `brokenToWrecked +1`; and
4. post-restart Wrecked state with reload unavailable and zero relevant faults.

## Pass gate

Version 0.0.24.1 passes the repair gate only when:

- an empty Broken Test Musket reloads successfully;
- reload preserves Broken condition and consumes exactly one component pair;
- a forced natural 1 or 2 from loaded/Broken consumes exactly one round and produces empty/Wrecked;
- Wrecked reload and attack remain blocked without consuming inventory or a queued forced roll;
- Normal reload remains unchanged;
- exact-item isolation holds;
- Broken and Wrecked persist through quicksave and complete restart; and
- all relevant duplicate, conflict, and fault counters remain zero.

Sprint 25 remains blocked until this exact package passes the repair gate together with the carried-forward Sprint 24 controls.
