# Human input required: Blunderbuss cone distance

## Exact blocker

Kingmaker's qualified native cone API requires a numeric distance. Every
authorized local firearm table labels the Blunderbuss range only `special` and
supplies no number. Selecting a value changes area, target count, and balance,
so AGENTS.md and the mission prohibit an autonomous choice.

The rest of the scatter contract is resolved in ADR-0037: native 90-degree cone
geometry and line of sight, exact target references, independent native weapon
attacks at -2, one ammunition discharge, per-target critical handling, and
misfire only when every volley roll misfires. Production remains fail-closed
until the distance is authorized.

## Smallest precise question

What numeric cone distance should the production Blunderbuss use?

- **15 feet (recommended):** conservative close-range adaptation limiting
  target multiplication while making the currently unavailable mandatory
  firearm playable.
- **30 feet:** materially larger battlefield area and target multiplication.
- **Another explicit distance:** provide the number of feet to make it project
  authority.

## Exact continuation

Record the authorized distance in ADR-0037 and the fidelity matrix; implement
the narrow native scatter delivery, focused tests, source qualification,
package validation, guarded runtime acceptance, and continue the mission.
