# Player-facing firearm overhaul

## Status

Sprint 28 turns the runtime-qualified same-item recovery boundary into a player-facing action for the Test Musket.

The authoritative state carrier remains the exact item's inert `BlueprintWeaponEnchantment` token. The rejected `ItemEntityWeapon.UniqueId` vault is not used.

## Gameplay contract

The bounded action is:

```text
exactly one equipped empty/Wrecked Test Musket
+ at least one Firearm Repair Kit in shared inventory
+ completed full-round Overhaul Test Musket delivery
= the same exact firearm becomes empty/Broken
+ exactly one Firearm Repair Kit is consumed
```

Overhaul does not:

- replace or remove the firearm;
- create a new firearm;
- create or load ammunition;
- consume Black Powder Charges or Lead Balls;
- repair Broken to Normal;
- bypass the full-round action; or
- operate on a native Heavy Crossbow.

## Availability

Availability fails closed unless all of these are true:

1. a concrete caster descriptor exists;
2. the Test Musket and Firearm Repair Kit blueprints are initialized;
3. exactly one distinct Test Musket is equipped between the primary and secondary hand slots;
4. that exact runtime item resolves through the accepted item-token repository;
5. its condition is Wrecked; and
6. shared inventory contains at least one Firearm Repair Kit.

A Normal or Broken firearm is rejected. More than one distinct equipped Test Musket is rejected as ambiguous. Readiness checks are read-only and consume nothing.

## Action delivery

`Overhaul Test Musket` is a personal, extraordinary, full-round ability. It uses the same delivery-time mutation boundary already qualified by the full-round reload ability. The state and inventory transaction is not started by an availability query or by command selection. It runs only from ability delivery.

Cancellation or interruption before delivery therefore has no transaction to roll back and must leave the item and repair-kit count unchanged.

## Atomic transaction

The dependency-free transaction performs these steps:

1. read the exact item state and repair-kit count;
2. reject without writes unless state is Wrecked and at least one kit exists;
3. derive empty/Broken through `FirearmStateMachine.OverhaulWrecked`;
4. remove exactly one repair kit;
5. verify the expected inventory count;
6. replace the exact item state using expected-current comparison;
7. verify the exact resulting state; and
8. verify that inventory did not change again.

After a mutation-time failure, the transaction attempts to restore both the exact pre-operation item state and exact repair-kit count. It refuses to overwrite an unexpected concurrent state during rollback. Any operation or rollback fault is reported and never represented as success.

## Exact-item evidence

The runtime result correlates the domain transaction with repository snapshots and requires:

- unchanged repository identity;
- unchanged in-process runtime reference hash;
- transaction states matching the repository states; and
- exactly one revision increment on success.

These identity values are process-local diagnostics. Across a full game restart, persistence is proved by the intended visible item retaining its empty/Broken token state while other blueprint-identical firearms retain their independent states.

## Repair kit

The Firearm Repair Kit is an inert, stackable blueprint cloned from a known native stackable item solely for inventory behavior and placeholder presentation. It carries no gameplay component. One kit is consumed by each completed Overhaul delivery.

The current test values are:

```text
cost: 50 gp
weight: 1.0 lb
```

Distribution through vendors, loot, crafting, or class features is intentionally outside Sprint 28.

## Recovery stages

The recovery model remains explicitly staged:

```text
Wrecked --Overhaul + repair kit--> Broken --ordinary repair, future delivery--> Normal
```

Overhaul is not Quick Clear and is not ordinary repair. This separation prevents a Wrecked firearm from becoming fully functional through one action and preserves a future design boundary for Gunsmithing, class abilities, resource costs, and repair time.

## Diagnostics

Process-local diagnostics report:

```text
attempts
completed
rejected
faults
last result
```

Development controls can add or remove repair kits, print readiness, and execute an immediate diagnostic transaction. Runtime acceptance must use the action-bar full-round ability for the successful and interruption tests; the immediate control bypasses action economy.
