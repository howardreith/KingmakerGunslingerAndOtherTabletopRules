# Sprint 62 scatter completion authority audit

## Existing qualified implementation

ADR-0037 and Sprint 32 already establish the exact native 90-degree cone
geometry, independent per-target attacks at -2, target-specific criticals,
all-roll misfire aggregation, one-chamber discharge, and triple explosion
policy. Thirty-eight focused domain tests remain green within the 831-test
suite. Production Blunderbuss identity is stable and deliberately unavailable.

## First unresolved invariant

The installed `WouldTargetUnitCone` contract requires a caller-supplied numeric
distance. Every authorized local firearm table and rule source available to the
repository describes Blunderbuss range only as `special`; none supplies a cone
length. Native 90-degree geometry does not imply distance.

Choosing 15, 20, 30, or another number would materially change target count,
positioning, misfire exposure, and weapon balance. No existing ADR, qualified
behavior, local tabletop source, or installed blueprint resolves that choice.

## Disposition

Scatter delivery remains temporarily `BLOCKED` on one player-facing balance
decision: the numeric Blunderbuss cone distance. The existing fail-closed
`ScatterConeDistanceService` and unavailable-item restriction remain correct.
No third-party value or inferred crossbow range may substitute for authority.

This bounded blocker does not stop independent mission work. Continue with
production fallback presentation and final lifecycle/compatibility coverage.

## Resolution

The user explicitly directed the implementation to follow PnP. The PnP
Blunderbuss pellet mode is a 15-foot cone, while its bullet mode uses a
10-foot range increment. ADR-0037 now records this project authority. The
domain distance boundary resolves exactly 15 feet to 4.572 native meters and
rejects applying that authority to another firearm. Native delivery remains
the next engineering gate; the production item stays unavailable until then.
