# Sprint 34 entry criteria: Gunslinger class chassis

## Authoritative rule

The base Gunslinger uses d10 hit dice, full base attack bonus, good Fortitude
and Reflex saves, poor Will saves, four skill ranks per level, any alignment,
simple and martial weapon proficiency, firearm proficiency, and light armor
proficiency. Its class progression spans levels 1 through 20.

## Kingmaker adaptation

Only skills with meaningful Kingmaker counterparts will be placed on the class
blueprint. Any omitted tabletop-only skill mapping must be recorded explicitly
before blueprint registration. Starting wealth is not reproduced as a separate
economy system; the class receives the authorized starting firearm/ammunition
package through normal starting-equipment integration.

## Observable behavior

- a canonical domain chassis exposes all 20 exact BAB and base-save rows;
- invalid or missing levels fail closed;
- class blueprint/progression identities are stable manifest entries;
- the class is selectable at character creation and advances through level 20;
- multiclass, level-up, respec, proficiencies, and starting equipment use native
  Kingmaker flows without development controls.

## Deterministic tests

- exact level 1, 5, 10, 15, and 20 rows;
- complete monotonic 20-level progression;
- full-BAB iterative attack derivation;
- good/poor save formulas and invalid-level rejection;
- exact blueprint structure, progression entries, and stable identities;
- starting-equipment and proficiency isolation from unrelated classes.

## Runtime evidence

Require exact-assembly mod load, guarded class catalog/structure acceptance,
then disposable in-memory character creation, level-up, multiclass, and respec
observations when safe exact runtime contracts are established. Risky acceptance
requires two independent fresh-process PASS runs and no save-writing API.

## Non-goals

Grit, deeds, Nimble, Gun Training, bonus-feat selections, True Grit, archetypes,
custom models, and balance inventions are outside this chassis checkpoint.

## Failure behavior

Missing vanilla references, ambiguous class-array insertion, duplicate stable
identities, incomplete progression rows, or unsafe starting-item grants fail
closed and leave native blueprints unchanged.
