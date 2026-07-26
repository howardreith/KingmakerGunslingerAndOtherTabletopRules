# Sprint 8 entry criteria — combat-pipeline instrumentation

## Goal

Observe, without changing, the Kingmaker attack and AC-calculation events required for firearm rules. A Test Musket shot should produce one coherent diagnostic trace containing enough data to implement range-limited touch AC and natural-roll misfire logic in later sprints.

## Runtime gate

Sprint 8 should ideally begin after a compiled Sprint 7 UMM package demonstrates:

- four custom blueprints register exactly once;
- the UMM development panel renders;
- Test Musket insertion succeeds;
- nonproficient equip is denied;
- granting Firearm Proficiency permits the intended positive-path equip;
- equipped-firearm diagnostics find exactly one marker;
- native Heavy Crossbows remain unchanged;
- new-game and save/load smoke tests do not crash.

Without that evidence, Sprint 8 may produce source and contract-inspection work, but it must not be labeled runtime-complete or READY FOR KINGMAKER.

## Bounded implementation

Sprint 8 may add:

- read-only rulebook subscribers or narrowly targeted Harmony adapters;
- a firearm-marker lookup service for the weapon participating in an event;
- a correlation identifier joining AC, attack-roll, weapon-attack, and damage observations;
- structured logging for attacker, target, weapon, natural d20, modifiers, result, distance, range increment, ordinary AC, touch AC, reason/source, and attack-command shape;
- a UMM switch or explicit diagnostic command controlling verbose traces;
- runtime-contract inspection for every event interface/type/member used.

Sprint 8 must not:

- change AC or attack results;
- force touch AC;
- create or consume ammunition;
- block empty attacks;
- implement misfires, broken state, grit, class content, vendors, or assets;
- log every non-firearm attack when instrumentation is disabled.

## Design constraints

- Instrument only attacks whose exact participating weapon type carries `FirearmDefinitionComponent`.
- Preserve the ordinary weapon pipeline.
- Do not identify firearms solely by Heavy Crossbow category.
- Do not retain unit, target, or rule-event objects beyond the event/correlation lifetime.
- Logging failures must never alter combat.
- A missing optional datum should be logged as unavailable rather than guessed.
- The trace must distinguish standard attacks, full attacks, bonus attacks, and ability-generated weapon attacks where the engine exposes that information.

## Acceptance

For one Test Musket attack, the log should make it possible to answer:

1. Which concrete weapon and weapon type participated?
2. Was the firearm marker found exactly once?
3. What natural d20 was rolled?
4. What total attack result and hit/miss outcome occurred?
5. What were the target's ordinary AC and touch AC at the relevant calculation point?
6. What was attacker-to-target distance and how many firearm range increments did it represent?
7. What command or ability generated the attack?
8. Did one attack produce duplicate callbacks that later logic must de-duplicate?

A native Heavy Crossbow attack should not be labeled a firearm and should not generate the full firearm trace.

## Deliverables

- Source milestone and change report.
- Updated runtime-contract script.
- Trace schema/documentation.
- Deterministic tests for marker lookup and event-correlation logic that do not require Kingmaker where practical.
- A UMM install ZIP only if the target assemblies are available and a real compile succeeds.
