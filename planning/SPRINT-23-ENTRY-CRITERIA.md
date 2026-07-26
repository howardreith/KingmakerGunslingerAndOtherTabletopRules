# Sprint 23 entry criteria — natural-roll misfire detection

Sprint 23 may connect natural attack rolls to firearm misfires only after version 0.0.22.1 (or a later bounded Sprint 22 repair) demonstrates all of the following in Kingmaker:

- the loaded Test Musket remains loaded immediately after quicksave;
- state-token reconciliation reports no unresolved conflict or fault;
- loaded state survives save, complete process exit, restart, and reload;
- a loaded Test Musket makes one ordinary attack through Kingmaker's native weapon pipeline;
- that attack consumes exactly one loaded round from the exact firing item;
- the hit or miss result does not change the one-round consumption rule;
- a second attack while empty is forced to miss;
- an empty-fire attempt does not consume Black Powder Charges or Lead Balls;
- a loaded Broken Test Musket can discharge one round and remains Broken;
- a Wrecked Test Musket is forced to miss;
- duplicate callbacks do not consume two rounds;
- a native Heavy Crossbow remains unaffected; and
- no bootstrap, token-reconciliation, firearm-state, attack-enforcement, or Harmony fault appears in the KMG log.

## Bounded Sprint 23 scope after a pass

Sprint 23 should add the first natural-roll misfire slice:

- read the final natural d20 from the exact firearm `RuleAttackRoll`;
- compare it with the Test Musket's misfire threshold;
- force a misfire result to miss regardless of total attack bonus;
- ensure a misfire still consumes the already-fired loaded round exactly once;
- record deterministic misfire diagnostics;
- provide a development-only method to force the next relevant natural roll for repeatable testing; and
- preserve native Heavy Crossbow behavior.

The first Sprint 23 package may leave firearm condition unchanged so that natural-roll detection can be proven independently. Applying Normal → Broken and Broken → Wrecked transitions belongs to the immediately following bounded sprint unless the exact runtime event ordering is already unambiguous and fully covered.

Sprint 23 must not yet add explosions, area damage, repair gameplay, automatic reloads between iterative attacks, Rapid Reload, pistols, scatter weapons, or the Gunslinger class.
