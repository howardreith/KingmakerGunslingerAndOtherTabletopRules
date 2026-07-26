# Sprint 25 entry criteria — second-misfire explosion and damage consequence

Sprint 25 may add the consequence of a detected misfire from a loaded Broken firearm only after version 0.0.24.1 (or a later bounded Sprint 24 repair) demonstrates all of the following in Kingmaker:

- the exact private `RuleAttackRoll.Roll` setter and public `IsSuccessRoll(int)` patches attach without Harmony faults;
- forced natural 1 and 2 each force a miss and consume exactly one round from the exact firing item;
- forced natural 3 and 20 remain ordinary and preserve Kingmaker's native result;
- Normal misfire commits exactly one empty Normal → empty Broken transition;
- an empty Broken exact firearm reloads to loaded/Broken, consumes exactly one Black Powder Charge and one Lead Ball, and is not silently repaired;
- Broken misfire commits exactly one empty Broken → empty Wrecked transition;
- one attack-roll object cannot apply condition damage twice;
- a second blueprint-identical Test Musket remains unchanged;
- no attack-time Black Powder Charge or Lead Ball consumption occurs;
- a genuine native Heavy Crossbow, an empty firearm, and a Wrecked firearm do not consume a pending forced roll or enter condition-transition diagnostics;
- an observed eligible `noNaturalRoll` completion preserves the pending forced roll and applies no condition damage;
- empty/Broken and empty/Wrecked states survive quicksave and complete save/exit/restart/load;
- token reconciliation reports no conflict or fault; and
- no bootstrap, attack-enforcement, AC, reload, misfire, repository, or Harmony fault appears.

The 0.0.24 result does not satisfy this gate because its player-facing reload path rejected the empty/Broken state. Version 0.0.24.1 is the bounded repair candidate for that failure.

## Bounded Sprint 25 scope after a pass

Sprint 25 should add only the immediate explosion/damage consequence of the second misfire while the firearm was Broken at discharge.

Before implementation, record the exact Pathfinder rule source and the exact Kingmaker damage-event contracts. The bounded implementation must:

- trigger only from the already-proven Broken → Wrecked misfire decision;
- retain the exact firing item and attack-roll correlation;
- apply at most once per attack-roll object;
- keep the firearm Wrecked and empty;
- direct any required damage only to the correct wielder through a native Kingmaker damage event;
- preserve the ordinary Normal → Broken misfire with no explosion;
- leave native Heavy Crossbows untouched;
- remain deterministic under the force-next-roll diagnostic; and
- expose explicit explosion-attempt, applied, duplicate, rejected, and fault diagnostics.

Sprint 25 must not yet add area or splash damage unless the verified rule explicitly requires it, repair gameplay, Quick Clear, automatic iterative reloads, Rapid Reload, pistols, scatter weapons, additional firearm blueprints, magical firearms, or the Gunslinger class.

If the exact Kingmaker damage-event contract or Pathfinder consequence is ambiguous, Sprint 25 must remain a research/contract sprint rather than guessing or patching broad damage pipelines.
