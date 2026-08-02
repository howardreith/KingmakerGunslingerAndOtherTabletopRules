# Sprint 52 entry criteria: Lightning Reload

## Authority and adaptation

The mission-authorized local Gunslinger rules grant Lightning Reload at level
11. While the gunslinger has at least 1 grit, she may reload one barrel of an
equipped one- or two-handed firearm as a swift action once per round without
provoking attacks of opportunity. Rapid Reload or an alchemical cartridge
changes that action to a free action, still only once per round.

Kingmaker has native swift and free command types but the current mod contains
neither a qualified Rapid Reload feat nor alchemical-cartridge ammunition.
Sprint 52 therefore implements the reachable swift-action deed exactly and
keeps the once-per-round gate independent of action type. The free-action route
must remain fail-closed until either qualifying prerequisite is implemented;
it must not infer Rapid Reload from names or treat ordinary powder and ball as
an alchemical cartridge.

## Observable contract

- The feature and one personal extraordinary swift-action ability appear
  exactly once at Gunslinger level 11.
- Availability requires positive current grit but spends no grit.
- The caster must have exactly one resolvable equipped, non-Wrecked production
  firearm with an unloaded chamber and the matching basic ammunition in shared
  inventory.
- Delivery loads exactly one chamber through the existing atomic firearm-state
  and ammunition transaction and preserves Broken condition.
- A successful delivery marks only that unit as having used Lightning Reload
  for the current round. A second delivery in the same round is unavailable.
- The unit-local use marker clears on that unit's next native round boundary.
- Rejection or transactional failure does not mark the deed used and does not
  consume grit, ammunition, or firearm state.
- The ability provokes no attack of opportunity and does not repair, fire, or
  otherwise mutate the weapon.
- Another unit's use and ordinary reload actions do not affect this unit's
  once-per-round gate.

## Deterministic qualification

- Pure tests cover positive-grit gating, one-round use, independent units,
  empty/loaded/Broken/Wrecked firearm states, ammunition prerequisites, and
  failure atomicity.
- Repository validation, the complete domain suite, clean Release build, and
  strict package validation must pass.
- A guarded save-free scenario must prove level-11 progression, swift action,
  positive-grit/no-spend behavior, one-chamber inventory-backed reload,
  same-round rejection, next-round reset, Broken preservation, cleanup, and
  isolation.
- The exact assembly requires mod-load PASS and two independent fresh-process
  feature PASS runs.

## Non-goals

Sprint 52 does not add Rapid Reload, alchemical cartridges, iterative automatic
reloads, multi-barrel batch loading, firearm repair, attacks, save interaction,
or any level-15-or-later deed. The free-action branch remains an explicit
future extension of the same once-per-round contract when a prerequisite has
its own authority and qualification.
