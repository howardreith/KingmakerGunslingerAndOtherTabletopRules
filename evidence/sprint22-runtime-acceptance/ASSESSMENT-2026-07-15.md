# Sprint 22 repair runtime acceptance — 2026-07-15

## Decision

Version 0.0.22.1 satisfies every blocking condition in `planning/SPRINT-23-ENTRY-CRITERIA.md`. Sprint 22 is runtime-accepted and Sprint 23 entry is approved.

## Evidence accepted

The supplied live runs establish all of the following on Pathfinder: Kingmaker 2.1.7b with Unity Mod Manager 0.32.4:

- the item-owned loaded-state token survives quicksave;
- loaded state survives save, complete process exit, restart, and reload;
- one loaded Test Musket shot reaches the exact repaired `RuleAttackRoll` hook and consumes exactly one round from the firing item;
- reload becomes available after discharge and consumes exactly one Black Powder Charge plus one Lead Ball;
- an empty Test Musket attack is forced to miss without consuming inventory ammunition;
- a loaded Broken Test Musket fires one round and remains Broken;
- a Wrecked Test Musket attack is forced to miss, consumes no round, and remains Wrecked;
- duplicate-event, attack-enforcement, armor-class, reload, token-reconciliation, and Harmony fault counters remain clear in the accepted evidence; and
- an actual native Heavy Crossbow reports `equippedFirearms=none detected`, proving that the borrowed Heavy Crossbow category and presentation do not admit it to the exact firearm path.

The final Broken control reports `status=Fired`, `roundsConsumed=1`, `forceMiss=False`, with `condition=Broken` before and after. The final Wrecked control reports `wreckedRejected=1`, `status=Wrecked`, `roundsConsumed=0`, `forceMiss=True`, with `condition=Wrecked` retained.

## Sprint 23 boundary opened

The accepted next slice is limited to final natural-d20 observation, the Test Musket's configured misfire threshold, a forced miss on misfire, deterministic force-next-relevant-roll diagnostics, and native Heavy Crossbow isolation. Automatic Normal → Broken and Broken → Wrecked transitions remain deferred.
