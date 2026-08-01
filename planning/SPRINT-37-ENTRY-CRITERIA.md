# Sprint 37 entry criteria — class integration

Sprint 37 continues the shippable base-class progression and the roadmap's
feat/reload integration. It begins with the highest-dependency incomplete
progression row, Nimble, before feat selections build on later class levels.

## Nimble acceptance

- Add cumulative +1 dodge facts at levels 2, 6, 10, 14, and 18.
- The total must be +1 through +5 and never exceed +5.
- Apply only while wearing light or no armor.
- Use Kingmaker's native Dodge modifier descriptor so flat-footed or any other
  loss of Dexterity AC also removes Nimble.
- Equipment changes must update the bonus without save mutation or shared state.
- Prove no-armor, light-armor, medium-armor, flat-footed, and cleanup behavior
  in a guarded detached runtime scenario.

## Non-goals for this slice

Nimble does not alter armor proficiency, Dexterity limits, touch AC, firearm
rules, or unrelated classes. Feat selections and reload economy follow as
separate narrow slices within Sprint 37.

Sprint 37 is a checkpoint, not a stopping condition. After qualification,
continue immediately to the next incomplete class-integration row.
