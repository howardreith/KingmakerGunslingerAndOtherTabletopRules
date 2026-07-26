# Reload Test Musket ability

## Current scope

`Reload Test Musket` is a personal extraordinary ability with standard-action command type and Kingmaker's full-round-action flag enabled. Firearm Proficiency grants the ability, and the disposable-save development controls can verify or restore it.

Version `0.0.24.1-s24-broken-reload-repair` corrects an integration mismatch exposed by the Sprint 24 runtime test: the pure firearm state machine had always allowed a Broken firearm to load and fire, but the player-facing availability adapter and cross-resource transaction still carried Sprint 21's temporary Normal-only restriction.

The repaired ability permits an **empty Normal or empty Broken** Test Musket to reload. Reloading a Broken firearm does not repair it. A Wrecked firearm remains permanently ineligible for this ordinary reload action.

## Availability

The action fails closed unless all of the following are true:

1. A concrete caster exists.
2. Exactly one distinct equipped weapon is the exact Test Musket item blueprint.
3. The item resolves through the exact firearm marker and item-token state repository.
4. Its condition is `Normal` or `Broken`; `Wrecked` is rejected.
5. It is empty.
6. Shared inventory contains at least one Black Powder Charge.
7. Shared inventory contains at least one Lead Ball.

For an empty Broken firearm, readiness explicitly reports that loading will preserve the Broken condition.

## Transaction order

The delivery phase performs the cross-resource transaction:

1. Read the exact firearm state and shared-inventory counts.
2. Recheck every eligibility condition before mutation.
3. Calculate a one-round loaded state while preserving `Normal` or `Broken` condition.
4. Consume exactly one Black Powder Charge and one Lead Ball through the verified inventory transaction.
5. Replace the state of the exact equipped firearm.
6. Re-read and verify the exact loaded state and unchanged condition.
7. Re-read and verify shared-inventory counts.

Mutation occurs inside `AbilityCustomLogic.Deliver`, not when the action is merely selected. Cancellation before delivery consumes nothing.

## Rollback

After the first possible write, any failure attempts to restore both resources to their exact before-state:

- the exact firearm state is restored only when the current state is either the original state or the transaction's attempted loaded state;
- shared inventory is restored to the exact Black Powder Charge and Lead Ball counts;
- state and inventory rollback failures are retained separately in `FirearmReloadTransactionException`; and
- a rollback failure is never reported as a successful reload.

## Condition guarantees

The successful one-round results are exactly:

```text
empty / Normal -> loaded / Normal
empty / Broken -> loaded / Broken
```

The following remains rejected:

```text
empty / Wrecked -> no change
```

This repair adds no firearm repair, Quick Clear, condition reduction, explosion, splash damage, Rapid Reload, automatic iterative reload, or new firearm content.
