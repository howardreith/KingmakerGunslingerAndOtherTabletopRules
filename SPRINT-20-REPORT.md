# Sprint 20 report — basic ammunition and atomic inventory transactions

## Gate consumed

Sprint 19 runtime testing established a conditional persistence GO for item-owned firearm-state enchantment tokens. Four blueprint-identical Test Muskets retained Loaded/Normal, Empty/Broken, Loaded/Broken, and token-free Empty/Normal states through save, complete process exit, restart, and reload. Sprint 20 therefore resumes the feature plan without changing the accepted carrier.

The four core Sprint 19 carrier files are byte-for-byte unchanged and enforced by the Sprint 20 source validator.

## Delivered

### Two stackable inventory blueprints

Sprint 20 activates the two previously reserved IDs:

- `KMG.Test.BlackPowderItem` — Black Powder Charge.
- `KMG.Test.LeadBulletItem` — Lead Ball.

Both are component-free `BlueprintItem` clones with custom names, descriptions, flavor text, cost, weight, and stackability. They currently inherit a placeholder Diamond Dust icon and inventory presentation.

Blueprint initialization now transactionally registers ten custom blueprints instead of eight. A failure registering either ammunition item rolls back the complete owned registration set.

### Engine-independent transaction model

The new ammunition domain contains immutable count snapshots, a two-component inventory abstraction, typed transaction results, and a failure type that preserves both mutation and rollback exceptions.

`TryConsumeOneLoad`:

- performs no writes when either component is missing;
- removes exactly one powder charge and one lead ball when both are available;
- verifies exact post-consumption counts;
- restores the exact pre-transaction counts after a partial failure;
- verifies rollback and reports rollback failure distinctly.

### Typed Kingmaker inventory adapter

The runtime adapter uses the exact `ItemsCollection.Count(BlueprintItem)`, `Add(BlueprintItem, int)`, and `Remove(BlueprintItem, int)` APIs from the supplied Kingmaker 2.1.7b assemblies.

### Development controls

The UMM panel can add, count, consume, and remove basic ammunition on a disposable save. It explicitly states that these controls do not load or fire a weapon.

## Explicitly not delivered

Sprint 20 does not add:

- a reload ability;
- a reload action cost;
- free-hand checks;
- attacks of opportunity from reloading;
- inventory consumption linked to a gun;
- empty-fire prevention;
- firing-time loaded-round consumption;
- misfires or repair gameplay;
- merchants, loot, crafting, models, or the Gunslinger class.

## Test coverage

The dependency-free suite grows from 373 to 398 cases. Twenty-five new cases cover:

- count snapshots and validation;
- success with one or several available loads;
- missing-powder, missing-ball, and empty rejection without mutation;
- null and invalid inputs;
- failure on first and second removal;
- failure after a mutation;
- post-mutation verification mismatch;
- successful rollback;
- rollback-failure reporting;
- result invariants and deterministic formatting.

## Runtime status

The exact-reference build is intended for a Kingmaker 2.1.7b smoke test. Runtime acceptance focuses on blueprint registration, item localization and stacking, count accuracy, atomic pair consumption, insufficient-component behavior, save/restart persistence, and regression of the firearm-state token carrier.
