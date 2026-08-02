# Sprint 94 PnP Blunderbuss distance authority

## Rule and adaptation

Per explicit user direction, follow PnP: pellet mode uses a 15-foot cone and
single-bullet mode uses a 10-foot range increment. Preserve the immutable
`special` definition because the two firing modes do not share one ordinary
range profile. Kingmaker's qualified native 90-degree cone receives exactly
4.572 meters for pellet delivery.

## Acceptance

- One canonical constant owns the 15-foot pellet cone.
- Resolution accepts only an exact scatter Blunderbuss.
- Missing/general distance APIs retain fail-closed validation.
- Production remains unavailable until native delivery is complete.
- Focused tests, repository validation, complete domain suite, clean Release
  build, strict package validation, and staged safety audits pass.

## Non-goals

This checkpoint does not unlock the production item, add an ability, dispatch
attacks, consume ammunition, or claim runtime qualification.
