# Sprint 24 entry criteria — firearm condition transitions on detected misfire

Sprint 24 may connect a detected natural-roll misfire to item-owned firearm condition only after version 0.0.23 (or a later bounded Sprint 23 repair) demonstrates all of the following in Kingmaker:

- the exact private `RuleAttackRoll.Roll` setter and public `IsSuccessRoll(int)` patches attach without Harmony faults;
- forced natural 1 and 2 each increment the misfire counter and force the attack to miss;
- forced natural 3 and 20 increment the ordinary counter and preserve Kingmaker's native success result;
- every eligible attack consumes exactly one loaded round from the exact firing item before classification;
- no attack-time Black Powder Charge or Lead Ball consumption occurs;
- a Sprint 23 misfire leaves Normal condition Normal and Broken condition Broken;
- a genuine native Heavy Crossbow does not consume a pending forced roll or enter firearm misfire diagnostics;
- an empty firearm does not consume a pending forced roll;
- a Wrecked firearm does not consume a pending forced roll;
- an eligible attack that completes before main-roll assignment increments `noNaturalRoll` and preserves the pending forced roll;
- duplicate assignment and evaluation counters remain zero;
- save, complete process exit, restart, and reload preserve the item-owned post-shot state; and
- no bootstrap, attack-enforcement, AC, reload, token-reconciliation, misfire, or Harmony fault appears.

## Bounded Sprint 24 scope after a pass

Sprint 24 should add only the item-owned condition transitions caused by a detected misfire:

```text
Normal -> Broken
Broken -> Wrecked
```

The transition must:

- occur on the exact firearm whose already-loaded round was discharged;
- happen at most once for one attack-roll object;
- retain `rounds=0` after the shot;
- preserve ammunition identity rules;
- use the existing item-owned inert enchantment-token repository;
- survive quicksave and complete save/exit/restart/load;
- leave native Heavy Crossbows untouched; and
- remain deterministic under the existing force-next-roll diagnostic.

Sprint 24 must not yet add explosions, area damage, splash damage, repair gameplay, automatic iterative reloads, Rapid Reload, pistols, scatter weapons, additional firearm blueprints, or the Gunslinger class.
