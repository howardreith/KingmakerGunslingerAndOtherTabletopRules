# Kingmaker Gunslinger 0.0.127 player smoke test

Firearm maintenance, full-rest recovery, and misfire interruption. Keep a
disposable save before testing. A Gunslinger with the Gunsmithing feature,
a reusable Gunsmith's Kit in the shared inventory, powder and balls, and at
least one firearm are required. The development console
(`--kmg-dev-console`, if enabled) can force the next natural d20 with
`Force next natural roll` to make misfires deterministic.

1. **Field repair (happy path).** Misfire a Normal firearm until it breaks
   (forced natural roll 1 is reliable). Out of combat, use the full-round
   Repair Firearm action: the exact equipped Broken gun becomes Normal,
   surviving loaded rounds are kept, the Gunsmith's Kit count is unchanged,
   and the item is never replaced (name, enchantments, origin stay).
2. **Combat rejection.** Start a fight and try Repair Firearm, including
   with a character who is not personally engaged while the party is in
   combat. Availability and execution must be rejected with the combat
   explanation, in both real-time-with-pause and turn-based.
3. **Wrecked is rest-only.** Break the gun twice (Broken -> Wrecked). Field
   repair must refuse with the completed-full-rest explanation, in and out
   of combat; the hidden legacy Overhaul slot (if an old save has it) must
   refuse identically. A Wrecked gun also cannot fire or reload.
4. **Full-rest restoration.** With the gunsmith resting and the kit in the
   shared inventory, complete a genuine full rest (camp): carried Broken
   and Wrecked firearms - equipped, in alternate weapon sets, or anywhere
   in the shared inventory - return to Normal in one step, the Wrecked gun
   stays unloaded, exactly one concise summary appears, and nothing is
   consumed. Rest without the kit (or without a gunsmith in the resting
   party): damaged guns stay damaged and one clear explanation appears.
   Cancelling the rest, waiting/skip-time, or an interrupted rest must not
   repair anything; finishing the rest after an interruption repairs once.
5. **Misfire interruption.** In combat, force a misfire that breaks the gun
   during a full attack: the misfiring shot resolves (round spent, burst on
   Broken->Wrecked), but no further iterative shot is fired, no auto-reload
   continues the sequence, and real-time auto-attacking does not immediately
   resume. Clicking a deliberate new attack with the still-Broken gun still
   works and fires under the normal Broken penalties/misfire rules.
6. **Quick Clear unchanged.** In combat with grit, Quick Clear still
   restores a misfire-Broken gun (standard costs nothing but needs one
   grit; move version spends one), rejects Wrecked and non-misfire states,
   and never resumes the interrupted attack order by itself.
7. **Regression spot-checks.** Ammunition crafting still works once per
   rest and resets after resting; native bows/melee and other characters
   are unaffected; save and reload after each step and confirm every
   condition, loaded-round count, and item identity persists.

Report anything that deviates, with the step number, combat mode, and the
combat log lines shown. Automated qualification status and evidence:
`docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`.

## Historical maintenance-loop guide

# Kingmaker Gunslinger 0.0.30 smoke-test guide

The current focused gate is maintained in `SMOKE-TEST-GUIDE-0.0.30.md`.
The remainder of this file preserves the broader 0.0.29 maintenance-loop gate
as historical reference; it is not required to be repeated for Sprint 30.

## Purpose

This guide applies to exact build `0.0.29-s29-complete-maintenance-loop`.

This gate qualifies the complete same-item maintenance loop and the accelerated Sprint 29 regression harness:

```text
empty/Wrecked
  â†’ Overhaul + one Firearm Repair Kit
empty/Broken
  â†’ Repair + one Firearm Repair Kit
empty/Normal
  â†’ Reload + one Black Powder Charge and one Lead Ball
loaded/Normal
```

Use only a disposable campaign. The Test Musket still displays as a Heavy Crossbow, and the consumables still use placeholder native artwork.

## 1. Install and initialize

1. Exit Kingmaker completely to the desktop.
2. Install only `KingmakerGunslinger-0.0.29-complete-maintenance-loop-smoke-test.zip` through Unity Mod Manager, replacing the previous version.
3. Start Kingmaker.
4. Confirm Unity Mod Manager shows:
   - `Kingmaker Gunslinger 0.0.29`;
   - a green status indicator; and
   - no bootstrap or Harmony error.
5. Load a disposable campaign and select the firearm test character.
6. Click **Grant Firearm Proficiency to selected unit**.
7. Confirm the result states that the selected unit has all three abilities:
   - Reload Test Musket;
   - Overhaul Test Musket; and
   - Repair Test Musket.
8. Enable firearm combat tracing.
9. Add two Test Muskets if fewer than two are visible.
10. Equip exactly one Test Musket and leave the other in shared inventory.

## 2. Run the one-command transaction regression

This first pass deliberately bypasses action economy. It proves the state/resource transactions and the qualification evaluator quickly.

1. Click **Run complete maintenance qualification immediately (diagnostic)** exactly once.
2. Read the complete `Last result` line.

Required final result:

```text
overall=PASS
stage=MaintenanceLoopPassed
```

Every matrix check must report `PASS`, including:

```text
exactItem
visibleItems
secondItem
faults
duplicates
revision
kits
powder
lead
overhaul
repair
reload
```

The one-command result must also state that it bypassed action economy and that action-bar delivery still requires separate testing.

3. Print visible firearm states.
4. Confirm:
   - the exact equipped fixture item is loaded/Normal;
   - the second Test Musket is still empty/Normal;
   - `visibleFirearms=2` or the same larger visible count captured by the fixture;
   - the two items have distinct process-local identities; and
   - all relevant fault and duplicate-application counters remain zero.
5. Click **Clear Sprint 29 qualification baseline (no item mutation)** before the manual pass.

A failed matrix blocks Sprint 30. Preserve the full result rather than trying to infer which stage was intended.

## 3. Prepare the manual action-bar fixture

1. Keep exactly one Test Musket equipped.
2. Click **Prepare Sprint 29 maintenance qualification fixture**.
3. The result must report:
   - one exact target item;
   - one different second item;
   - at least two visible firearms;
   - at least two Firearm Repair Kits;
   - at least one Black Powder Charge;
   - at least one Lead Ball; and
   - a matrix with `overall=PASS; stage=FixtureReady`.
4. Click **Print Sprint 29 maintenance PASS/FAIL matrix**.
5. Capture the target and second-item repository identities, runtime reference hashes, revisions, resource counts, and completion counters.

Expected target state:

```text
rounds=0
ammunition=<none>
condition=Wrecked
```

Expected second-item state:

```text
rounds=0
ammunition=<none>
condition=Normal
```

## 4. Verify Overhaul interruption

1. Start **Overhaul Test Musket** from the action bar.
2. Interrupt it before delivery completes by moving, cancelling, or issuing another command.
3. Print the PASS/FAIL matrix again.

Required result:

```text
overall=PASS
stage=FixtureReady
```

Also confirm:

- the exact item remains empty/Wrecked;
- its revision is unchanged;
- the Repair Kit count is unchanged;
- Overhaul `completed` is unchanged;
- Repair and Reload counters are unchanged; and
- no new fault or duplicate counter appears.

## 5. Complete Overhaul

1. Use **Overhaul Test Musket** again.
2. Allow the full-round delivery to complete.
3. Print the PASS/FAIL matrix.

Required result:

```text
overall=PASS
stage=OverhaulPassed
```

Required transaction evidence:

```text
exact state: empty/Wrecked â†’ empty/Broken
repair kits: -1
target revision: +1
overhaul completed: +1
repair completed: unchanged
reload completed: unchanged
powder: unchanged
Lead Balls: unchanged
```

The repository identity and in-process runtime reference hash must remain unchanged. The second Test Musket must remain empty/Normal with its original revision.

## 6. Verify Repair interruption

1. With the exact target now empty/Broken, start **Repair Test Musket** from the action bar.
2. Interrupt it before delivery completes.
3. Print the matrix again.

Required result:

```text
overall=PASS
stage=OverhaulPassed
```

Confirm:

- the exact item remains empty/Broken;
- its revision is unchanged from the completed-Overhaul checkpoint;
- the remaining Repair Kit is not consumed;
- Repair `completed` is unchanged; and
- no new fault or duplicate counter appears.

## 7. Complete ordinary Repair

1. Use **Repair Test Musket** again.
2. Allow the full-round delivery to complete.
3. Print the matrix.

Required result:

```text
overall=PASS
stage=RepairPassed
```

Required transaction evidence:

```text
exact state: empty/Broken â†’ empty/Normal
repair kits: another -1, total -2 from fixture
target revision: another +1, total +2
repair completed: +1
overhaul completed: still +1
reload completed: unchanged
powder: unchanged
Lead Balls: unchanged
```

The same exact item must remain equipped. No weapon may be added, removed, or replaced. The second Test Musket must remain unchanged.

## 8. Complete Reload

1. Use **Reload Test Musket** from the action bar.
2. Allow the full-round delivery to complete.
3. Print the matrix.

Required final result:

```text
overall=PASS
stage=MaintenanceLoopPassed
```

Required final deltas from the fixture baseline:

```text
target state: loaded/Normal
target revision: +3
repair kits: -2
Black Powder Charges: -1
Lead Balls: -1
overhaul completed: +1
repair completed: +1
reload completed: +1
second item: unchanged
faults: unchanged
duplicates: unchanged
```

The exact final firearm state must be:

```text
rounds=1
ammunition=kmg.debug.lead-ball
condition=Normal
```

## 9. Verify Repair rejection boundaries

Use fresh disposable fixtures as needed. Each check must leave item state, revision, and Repair Kit count unchanged.

### Normal firearm

1. With the exact firearm empty/Normal, add at least one Repair Kit.
2. Click **Print Repair Test Musket readiness**.
3. Required: `available=False` with a reason that only a Broken firearm can use ordinary Repair.

### Wrecked firearm

1. Set the exact item to empty/Wrecked through the fixture or diagnostic state controls.
2. Print Repair readiness.
3. Required: unavailable with a reason that the firearm must be Overhauled first.

### Loaded Broken firearm

1. Prepare the fixture.
2. Complete Overhaul to reach empty/Broken.
3. Reload while Broken.
4. Print Repair readiness.
5. Required: unavailable with a reason that Repair requires an empty firearm.

### Missing Repair Kit

1. Return the item to empty/Broken.
2. Remove all Firearm Repair Kits.
3. Print Repair readiness.
4. Required: unavailable with `One Firearm Repair Kit is required.`

Do not use the immediate Repair diagnostic as evidence for player-facing action timing. It is acceptable only for fixture recovery between negative cases.

## 10. Verify persistence

1. End with the intended exact Test Musket in loaded/Normal state after a passing manual maintenance loop.
2. Record the second Test Musket's independent state.
3. Quicksave.
4. Confirm the exact item remains loaded/Normal immediately after the quicksave.
5. Make a normal save.
6. Exit Kingmaker completely to the desktop.
7. Restart Kingmaker and load the save.
8. Print equipped and visible firearm states.

Required persistent result:

```text
intended firearm: rounds=1, condition=Normal
second firearm: independent prior state retained
state-token conflicts=0
state-token faults=0
```

Process-local repository labels, runtime reference hashes, counters, and the maintenance qualification baseline reset after restart. The matrix should correctly report that no active baseline exists until a new fixture is prepared.

## 11. Final diagnostic review

Before ending the test, confirm:

```text
Overhaul runtime faults=0
Repair runtime faults=0
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

All duplicate-application counters included by the maintenance matrix must remain at their baseline values. Harmless native spatial-query candidate deduplication may still appear in `targetDuplicates`; it is not included in the maintenance duplicate-application total.

Any consumed kit on interruption, wrong item mutation, changed second item, incorrect revision/resource delta, failed PASS/FAIL checkpoint, state loss after restart, nonzero relevant fault, or new duplicate application blocks Sprint 30.

## Teleportation completion smoke test (2026-09-10 candidate)

Applies on top of the 0.0.119 smoke steps above, using a save kept for testing.

1. **Specialist preparation.** On a Conjuration specialist wizard, open the
   spellbook, level-5 page, and prepare Teleport into the star-marked
   favorite slot; rest. The slot accepts it (and rejects non-Conjuration
   spells). Greater Teleport uses the seventh-level favorite slot. Other
   schools' favorite slots and universalist books are unchanged.
2. **Scroll purchase.** Buy a Teleport scroll from Zarcie (or Word of Recall
   from Arsinoe/Jhod). Stock is finite; buying out leaves it empty after
   reload and after toggling the module OFF/ON.
3. **Copy.** With a wizard who does not yet know Teleport, use the scroll's
   copy action in inventory: the spell enters the book and the scroll is
   consumed. Then prepare it (favorite or ordinary slot) and rest.
4. **Scroll activation.** On the world map, select a visited destination:
   a "Use Teleport Scroll / reader · n shared scrolls" action appears for
   any reader (a caster who knows the spell, or anyone with trained Use
   Magic Device). Confirm: exactly one scroll is consumed, no spell slot,
   and the party arrives instantly. Cancelling consumes nothing. Using the
   scroll from the inventory outside this flow prompts you to select a
   destination on the world map instead.
5. **Post-teleport arrows.** After any teleport (spell, scroll, or off-target
   result), the direction arrows around the party token respond on the very
   first click — also after reloading a save made right after teleporting.
6. **Compact menu.** At a settlement with a native Teleport button plus spell
   rows, the native button reads "Settlement Teleport"; long spell lists
   scroll inside the panel; the label and layout restore after closing.
