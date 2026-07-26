# Sprint 21 entry criteria — Test Musket reload action

Sprint 21 may connect ammunition to a firearm reload action only after version 0.0.20 demonstrates all of the following in Kingmaker:

- ten custom blueprints register exactly once;
- Black Powder Charge and Lead Ball appear with their custom names;
- quantities merge and count correctly in shared inventory;
- one successful transaction removes exactly one of each item;
- a missing powder charge or missing lead ball removes neither component;
- 19/19 ammunition stacks survive save, complete process restart, and reload;
- the Sprint 19 A-D firearm-state fixture remains unchanged;
- native Diamond Dust is not mutated;
- no crash, duplicate item, negative count, or unexpected inventory-price anomaly occurs.

## Bounded Sprint 21 scope after a pass

Sprint 21 should add one Test Musket reload ability that:

- targets only the currently equipped exact Test Musket;
- requires an empty, non-wrecked firearm;
- requires one Black Powder Charge and one Lead Ball;
- uses the musket's full-round reload profile;
- consumes inventory only when the action completes successfully;
- commits the Loaded / Normal or Loaded / Broken state to that exact gun;
- verifies both the inventory transaction and item-owned state write;
- restores inventory if the state write fails;
- provides clear combat-log feedback.

It must not yet implement iterative free reloads, Rapid Reload, attack-time consumption, misfires, or the Gunslinger class.

## Failure branch

If item registration, stacking, counting, atomic consumption, persistence, or native-source isolation fails, Sprint 21 remains on ammunition infrastructure and addresses the exact runtime failure before adding a reload ability.
