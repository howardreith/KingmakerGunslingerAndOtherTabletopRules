# Sprint 11 entry criteria — runtime per-item state association

## Goal

Associate one independent `FirearmState` with each concrete Test Musket item instance during a running Kingmaker process, without yet claiming save persistence.

## Required evidence before implementation

- Sprint 10 pure-state files remain free of Kingmaker and Unity types.
- The installed Kingmaker contract inspection identifies the concrete runtime weapon-item type and stable object-reference behavior available during inventory/equipment operations.
- The repository still registers exactly four active custom blueprints and no ammunition blueprint.

## Allowed work

- Introduce `IFirearmStateRepository` as the only runtime access boundary.
- Build a process-local repository keyed by the exact firearm item instance, preferably weakly so discarded items are not retained.
- Initialize an unseen firearm to `FirearmState.CreateEmpty()`.
- Resolve only exact firearms carrying one `FirearmDefinitionComponent`.
- Add diagnostics that print an item's runtime identity and current state.
- Add development-only controls that set or transition state without consuming inventory ammunition.
- Prove that two identical Test Muskets can hold different state in one running process.
- Add dependency-free repository tests using plain object keys where possible.

## Forbidden work

- Character buffs as firearm state.
- Save serialization or a claim that state survives process restart.
- Inventory ammunition consumption.
- Reload action economy.
- Empty-fire interception.
- Misfire-roll interception or explosion damage.
- Vendors, crafting, grit, or class progression.

## Acceptance

1. Two identical firearm item instances receive different states without leakage.
2. Equip/unequip and weapon-set switching do not merge state during the same process.
3. Transferring an item between units retains the state associated with that exact object instance.
4. Native Heavy Crossbows never receive firearm state.
5. Repository diagnostics identify both the item instance and immutable state.
6. The milestone states plainly that save/load and process restart are unproven and belong to Sprint 12.
