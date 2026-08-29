# Complete firearm maintenance loop

## Status

Sprint 29 completes the first player-facing same-item maintenance loop for the Test Musket and adds a process-local qualification harness for rapid regression checks.

The authoritative state carrier remains the exact item's inert `BlueprintWeaponEnchantment` token. The rejected `ItemEntityWeapon.UniqueId` vault is not used.

## Staged gameplay contract

The complete bounded sequence is:

```text
empty/Wrecked
  -- Overhaul Test Musket, full-round, one Firearm Repair Kit -->
empty/Broken
  -- Repair Test Musket, full-round, one Firearm Repair Kit -->
empty/Normal
  -- Reload Test Musket, full-round, one Black Powder Charge + one Lead Ball -->
loaded/Normal
```

Each stage has a different purpose and resource boundary:

- **Overhaul** makes a Wrecked firearm serviceable enough for ordinary repair. It stops at Broken.
- **Repair** restores a Broken firearm to empty Normal. Any rounds loaded in that exact firearm are destroyed; they are not returned to inventory.
- **Reload** places one round in an empty Normal or Broken firearm and consumes the ammunition components.

The actions are intentionally not collapsed into one operation. A Wrecked firearm cannot skip Overhaul, and an ordinary Repair cannot implicitly reload.

## Ordinary Repair availability

`Repair Test Musket` fails closed unless all of these are true:

1. a concrete caster descriptor exists;
2. the Test Musket and Firearm Repair Kit blueprints are initialized;
3. exactly one distinct Test Musket is equipped between the primary and secondary hand slots;
4. that exact runtime item resolves through the item-token repository;
5. its state is Broken; and
6. shared inventory contains at least one Firearm Repair Kit.

The following are rejected without mutation:

- no equipped Test Musket;
- more than one distinct equipped Test Musket;
- empty/Normal;
- empty/Wrecked;
- missing Repair Kit; or
- missing shared inventory.

## Delivery timing

Repair is a personal extraordinary full-round ability. Availability checks are read-only. State and inventory mutation occurs only from ability delivery after the command completes.

Cancelling, interrupting, moving away from the command, or replacing it before delivery therefore starts no repair transaction and must consume no kit.

## Atomic transaction

The dependency-free Repair transaction performs these steps:

1. read the exact firearm state and Repair Kit count;
2. reject without writes unless the state is Broken and at least one kit exists;
3. derive empty/Normal through `FirearmStateMachine.Repair`, discarding every loaded round and its ammunition identity;
4. remove exactly one Repair Kit;
5. verify the expected kit count;
6. replace the exact item's expected-current state;
7. verify the resulting empty/Normal state; and
8. verify that inventory did not change again.

After a mutation-time failure, the transaction attempts to restore both the exact pre-operation firearm state and the exact Repair Kit count. It refuses to overwrite an unexpected concurrent item state. An operation or rollback fault is surfaced and never represented as success.

## Exact-item evidence

A successful runtime result requires:

- unchanged repository identity;
- unchanged in-process runtime reference hash;
- transaction states matching the repository snapshots;
- exactly one repository revision increment;
- Broken before state, which may contain one or more loaded rounds;
- empty/Normal after state; and
- exactly one Repair Kit consumed.

These identities are process-local diagnostics. Save/restart persistence is proved by the intended visible firearm retaining its item-owned empty/Normal or loaded/Normal token state while another blueprint-identical firearm retains its independent state.

## Qualification harness

The Sprint 29 development fixture prepares:

- one exact equipped Test Musket at empty/Wrecked;
- one different visible Test Musket at empty/Normal;
- at least two Firearm Repair Kits;
- at least one Black Powder Charge;
- at least one Lead Ball; and
- a process-local baseline containing exact identities, revisions, resources, completion counters, fault totals, and duplicate totals.

The pure evaluator recognizes four checkpoints:

```text
FixtureReady
OverhaulPassed
RepairPassed
MaintenanceLoopPassed
```

At every checkpoint it verifies:

- exact target identity;
- unchanged visible item count;
- unchanged second-item identity, revision, and state;
- exact target revision delta;
- exact kit, powder, and Lead Ball deltas;
- exact Overhaul, Repair, and Reload completion deltas;
- no new faults; and
- no new duplicate applications.

The matrix is diagnostic-only and process-local. It never controls gameplay or persistence.

## One-command regression runner

The immediate runner prepares the fixture, executes Overhaul, Repair, and Reload through the established immediate runtime adapters, and records a matrix after every stage. It is intended to reduce repetitive regression setup.

Because it bypasses action economy, it cannot qualify:

- actual full-round command timing;
- cancellation or interruption before delivery;
- action-bar availability presentation; or
- animation and combat-log presentation.

Those remain focused manual checks.

## Deferred generalization

Sprint 29 is still Test-Musket-specific. Sprint 30 should extract definition-driven exact-firearm selection and shared maintenance action plumbing before adding a production firearm catalog.
