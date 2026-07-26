# Sprint 30 entry criteria — generic definition-driven firearm actions

Sprint 30 may begin only after the exact 0.0.29 standalone package proves in Kingmaker that:

- Firearm Proficiency grants Reload Test Musket, Overhaul Test Musket, and Repair Test Musket;
- the automated fixture prepares two distinct exact Test Muskets and emits a passing `FixtureReady` matrix;
- the one-command immediate runner reaches `MaintenanceLoopPassed` with every matrix check passing;
- action-bar Overhaul changes only the exact empty/Wrecked item to empty/Broken and consumes one Repair Kit on completed delivery;
- action-bar Repair changes only the same exact empty/Broken item to empty/Normal and consumes one additional Repair Kit on completed delivery;
- action-bar Reload changes only that exact empty/Normal item to loaded/Normal and consumes one Black Powder Charge plus one Lead Ball;
- interrupting Overhaul and Repair before delivery consumes no kit and changes no item state or revision;
- Repair rejects Normal, Wrecked, loaded Broken, missing-kit, and ambiguous-target cases without mutation;
- the independent second Test Musket remains unchanged through the complete loop;
- exact in-process repository identity and runtime reference remain unchanged, with one revision increment per completed stage;
- the resulting item-owned state survives quicksave and full save/exit/restart/load; and
- overhaul, repair, reload, attack, misfire, explosion, AC, trace, token-reconciliation, bootstrap, and Harmony faults and duplicate-application counters remain zero.

## Bounded Sprint 30 scope after a pass

Sprint 30 should replace Test-Musket-specific Reload, Overhaul, and Repair selection/runtime duplication with definition-driven actions that operate on any exact marked firearm.

The generic layer must preserve:

- exact equipped-item selection and ambiguity rejection;
- definition-specific capacity and reload configuration;
- accepted item-token state persistence;
- delivery-time resource mutation;
- cross-resource verification and rollback;
- native Heavy Crossbow isolation;
- same-item identity and revision evidence; and
- the existing Test Musket as the regression fixture.

Sprint 30 may add shared exact-firearm action context, common availability/result types, and migration adapters that keep the three existing action blueprints behaviorally compatible. It must not yet add the early firearm catalog, scatter attacks, advanced capacities, Gunslinger class progression, grit, deeds, vendors, production assets, or enemy firearm AI.
