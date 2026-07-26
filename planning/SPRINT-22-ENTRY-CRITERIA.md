# Sprint 22 entry criteria — loaded-round attack enforcement

Sprint 22 may connect loaded state to attacks only after version 0.0.21 demonstrates all of the following in Kingmaker:

- eleven custom blueprints register exactly once;
- Firearm Proficiency grants or restores `Reload Test Musket` on an existing disposable character;
- the ability is visible in the character's abilities/action-bar surface;
- it is treated as a full-round action;
- an empty, Normal Test Musket with one powder charge and one Lead Ball reloads successfully;
- exactly one of each component is consumed;
- the exact equipped Test Musket becomes Loaded / Normal;
- another blueprint-identical Test Musket remains unchanged;
- an already-loaded, broken, wrecked, unequipped, or ammunition-starved Test Musket rejects reload without mutation;
- cancelling the action before delivery consumes nothing;
- loaded state and remaining inventory counts survive save, complete process restart, and reload;
- no bootstrap, action-delivery, state-token, inventory, or rollback fault appears in the KMG log.

## Bounded Sprint 22 scope after a pass

Sprint 22 should:

- prevent an empty Test Musket from completing a damaging ordinary weapon attack;
- allow a loaded Test Musket to attack through the normal ranged-weapon pipeline;
- consume exactly one loaded round from the exact firing item;
- leave inventory ammunition unchanged when firing;
- avoid double consumption from nested rule events;
- preserve ordinary damage, critical, feat, concealment, and touch-AC behavior;
- add diagnostic counters and deterministic attack-state tests.

It must not yet implement misfires, iterative automatic reloads, Rapid Reload, pistols, scatter weapons, or the Gunslinger class.
