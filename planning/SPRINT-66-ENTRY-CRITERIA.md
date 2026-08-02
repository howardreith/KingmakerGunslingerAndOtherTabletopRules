# Sprint 66 entry criteria: production firearm actions

## Existing mechanical contract

Firearm Proficiency grants the stable Reload, Overhaul, and Repair ability
blueprints. Their compatibility adapter types retain historical Test Musket
names, but all three runtime paths resolve exactly one equipped firearm by its
`FirearmDefinitionComponent` and apply definition-driven policy. They already
support production firearms and must retain their stable symbols and GUIDs.

## Required player-facing contract

- Ability names are `Reload Firearm`, `Overhaul Firearm`, and `Repair Firearm`.
- Descriptions, availability reasons, rejection messages, and ordinary runtime
  logs refer to the exact equipped firearm, never Test Musket.
- Reload remains full-round and supports only a firearm whose definition has a
  full-round reload profile; Lightning Reload remains separate.
- Repair remains empty/Broken to empty/Normal and Overhaul remains
  empty/Wrecked to empty/Broken, each consuming one kit atomically.
- Stable blueprint symbols, GUIDs, component types, action economy, targeting,
  ordering, and transaction behavior do not change.

## Qualification

Focused validation must reject any player-facing Test Musket wording in the
three production actions and prove Firearm Proficiency still grants the exact
three stable abilities. The complete domain suite, repository validation,
clean Release build, strict package validation, exact mod load, and two guarded
save-free Gunslinger-presentation observations remain required. Existing
generic-firearm-action runtime evidence is retained because this checkpoint
does not change mechanics.

## Non-goals and failure behavior

This checkpoint does not rename serialized symbols or implementation classes,
change action costs, add rapid reload, select between multiple simultaneously
equipped firearms, or weaken fail-closed ambiguity and rollback behavior.
