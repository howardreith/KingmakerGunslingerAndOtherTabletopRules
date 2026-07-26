# Sprint 26 entry criteria — explosion burst targeting

Sprint 26 may begin only after the exact 0.0.25 standalone package proves in Kingmaker that:

- a first Normal-to-Broken misfire applies no explosion damage;
- an empty Broken firearm reloads while remaining Broken;
- a second Broken-to-Wrecked misfire triggers exactly one native Reflex DC 12 save and one native damage event against the exact current wielder;
- the applied damage is full on a failed save or half on a passed save as reported by Kingmaker;
- the firearm remains empty/Wrecked before and after the damage event;
- the damage event is correlated to the exact firing item and attack roll;
- duplicate success evaluation cannot repeat the save or damage;
- a second blueprint-identical Test Musket is unchanged;
- ordinary firearm rolls, native Heavy Crossbows, empty firearms, and Wrecked firearms do not enter explosion diagnostics;
- quicksave and complete save/exit/restart/load retain the Wrecked item state;
- explosion, misfire, attack, reload, AC, token-reconciliation, bootstrap, repository, and Harmony faults remain zero; and
- the tester captures the explosion diagnostic line and the relevant combat-log/HP evidence.

## Bounded Sprint 26 scope after a pass

Sprint 26 should add only the tabletop burst portion of the already-verified early-firearm explosion rule:

- inspect and record exact Kingmaker 2.1.7b spatial-query, unit-position, and distance contracts before implementation;
- choose a deterministic origin compatible with the wielder's occupied space;
- use the firearm definition's verified misfire-burst radius;
- enumerate only valid living units in that radius, including the exact wielder once;
- trigger one native Reflex DC 12 save and one native weapon-damage event per affected unit;
- preserve at-most-once behavior per attack and per affected unit;
- keep the exact firearm empty/Wrecked;
- expose target-count, applied, duplicate, rejected, and fault diagnostics; and
- retain native Heavy Crossbow isolation.

If exact spatial contracts or Pathfinder burst geometry cannot be established unambiguously, Sprint 26 must remain a contract/research sprint.

Sprint 26 must not add scatter triple damage, item destruction, repair gameplay, Quick Clear, Gunsmithing, make whole, Rapid Reload, iterative automatic reloads, new firearm blueprints, magical firearms, or Gunslinger class progression.
