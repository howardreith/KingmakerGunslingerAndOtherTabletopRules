# Sprint 29 entry criteria — automated qualification and complete maintenance loop

Sprint 29 may begin only after the exact 0.0.28 standalone package proves in Kingmaker that:

- Firearm Proficiency grants both Reload Test Musket and Overhaul Test Musket;
- Overhaul is unavailable without a Firearm Repair Kit;
- Normal and Broken firearms are rejected without resource or state mutation;
- cancelling or interrupting the full-round action before delivery consumes no kit and leaves the exact item Wrecked;
- completed delivery consumes exactly one kit and no powder or Lead Ball;
- only the exact equipped Wrecked Test Musket changes to empty/Broken;
- repository identity and in-process runtime reference remain unchanged;
- repository revision advances exactly once;
- a second blueprint-identical Test Musket remains unchanged;
- the empty/Broken state survives quicksave and full save/exit/restart/load; and
- overhaul, reload, attack, misfire, explosion, AC, trace, token-reconciliation, bootstrap, and Harmony faults remain zero.

## Bounded Sprint 29 scope after a pass

Sprint 29 is the first accelerated vertical slice. It should combine two related outcomes:

1. a one-command deterministic runtime qualification harness that prepares common firearm fixtures and emits a concise PASS/FAIL report for already-qualified contracts; and
2. one player-facing ordinary Broken-to-Normal maintenance path, kept distinct from Wrecked-to-Broken Overhaul.

The maintenance path must define its action timing, resource or skill cost, exact-item target selection, interruption behavior, rollback behavior, and save persistence. It must reuse the accepted item-token repository and must not replace the item.

Sprint 29 may also extract shared selection and transaction primitives needed by Reload, Overhaul, and ordinary repair. It must not add Gunslinger class progression, grit, deeds, new firearm categories, scatter attacks, advanced capacities, custom assets, vendors, or enemy firearm AI.
