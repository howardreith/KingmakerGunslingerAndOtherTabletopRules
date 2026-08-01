# Sprint 31 entry criteria — early production firearm catalog

## Entry gate

Sprint 30 is runtime-qualified on commit `0052dad` by exact mod-load PASS and
two consecutive fresh-process `generic-firearm-actions` PASS runs. No save API
was observed and `KMG_AUTOMATION_BASELINE` was not loaded.

## Authoritative rules

The local `private/rules/FIREARMS.md` table specifies:

| Weapon | Hands | Medium damage | Critical | Range | Misfire | Burst | Capacity | Reload |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| Pistol | one | 1d8 | x4 | 20 ft. | 1 | 5 ft. | 1 | standard |
| Musket | two | 1d12 | x4 | 40 ft. | 1-2 | 5 ft. | 1 | full-round |
| Blunderbuss | two | 1d8 | x2 | special | 1-2 | 10 ft. | 1 | full-round |

Early firearms use touch AC only within their first increment, have a maximum
of five increments, require a free hand to reload, and use one dose of black
powder plus compatible bullet/pellet ammunition.

## Kingmaker adaptation

- Use ordinary native weapon attacks and crossbow-compatible placeholder
  presentation under ADR-0005, ADR-0007, and ADR-0013.
- Firearm identity, rules, and action selection come only from the exact custom
  marker; inherited crossbow categories are adapters.
- Production pistol and musket statistics must match the table rather than
  inherit native crossbow damage, critical, range, or handedness silently.
- Blunderbuss scatter execution remains Sprint 32. The table gives its range as
  `special`; no numeric single-target range adaptation is authorized yet.
  Implement content and all non-range definition fields without inventing that
  balance value, and continue local contract/rules investigation before making
  it player-fireable.

## Observable behavior

- Three distinct production item/type blueprint pairs register under stable,
  manifest-owned IDs without mutating native sources.
- Pistol, musket, and blunderbuss each carry exactly one immutable definition
  marker and one firearm-proficiency restriction.
- Pistol and musket expose their exact tabletop damage, critical multiplier,
  range, capacity, misfire, hands, and reload action through concrete runtime
  blueprints and definitions.
- Generic Reload, Overhaul, and Repair resolve each production item by marker,
  preserve per-item state, and reject unmarked native crossbows.
- Production names/descriptions identify placeholder visuals and do not call
  any item a Test Musket.

## Deterministic tests

- Canonical definition factories and invalid/special-range boundaries.
- Stable manifest IDs, active registration count, and collision/rollback.
- Native-source snapshot preservation and exact one-marker/one-restriction
  validation for each production pair.
- Concrete damage, critical, range, handedness, presentation, and item/type
  wiring tests where dependency-free representation is possible.
- Generic action policy and independent-state regression for all catalog
  definitions.

## Runtime evidence

Add a guarded working-save scenario that grants disposable production items in
memory, proves exact blueprint/definition data, operates generic maintenance on
at least pistol and musket, proves independent item-owned state, verifies native
crossbow isolation, and observes no save-writing API. Require mod-load PASS and
two fresh-process feature PASS runs for the final catalog checkpoint.

## Non-goals

Scatter cone attacks, advanced firearms, capacity greater than one, class
progression, grit, deeds, vendors, campaign economy, custom models, and enemy AI
remain outside this checkpoint.

## Failure and rollback

Blueprint registration remains transactional and collision-safe. Any missing
source, ambiguous exact contract, wrong statistic, marker/restriction count,
resource rollback failure, state sharing, or runtime ambiguity fails closed.
Runtime fixture mutations are never saved and are discarded on automatic exit.
