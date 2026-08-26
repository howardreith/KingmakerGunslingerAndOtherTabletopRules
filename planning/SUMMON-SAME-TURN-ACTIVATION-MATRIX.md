# Summon Same-Turn Activation Matrix

Status: NOT YET QUALIFIED

| Case | Required evidence | Current disposition |
| --- | --- | --- |
| Ordinary unaccelerated native summon | Existing accepted scheduling and duration unchanged | Pending control |
| Acadamae OFF | Native Full-Round timing; no Acadamae save/consequence | Pending control |
| Acadamae ON | Standard action, one slot, one save/consequence, correct current-round summon opportunity | Pending reproduction |
| Legitimate Quickened summon | Swift action, caster retains Standard/Move, correct current-round summon opportunity | Pending fixture proof |
| KMG Expanded Summoning | Real KMG choice follows the same general summon mechanism | Pending |
| `1d3` or `1d4+1` | Every successful spawned unit receives exactly one initial opportunity | Pending |
| Duplicate callback | Same unit/opportunity correlation becomes a no-op | Pending |
| Following round | Native scheduling; no duplicate initiative, command, or activation | Pending |
| Duration | Same-turn opportunity neither shortens nor extends native lifecycle | Pending |
| RTwP | Native immediate AI behavior; no artificial turn state | Pending control |
| Cancelled/failed summon | No spawned unit and no activation state | Pending control |
| Non-summon spawn | Completely unaffected | Pending control |
| Save/load/reset | No stale project-owned activation marker | Pending |
| Standalone profile | Full guarded behavior | Pending |
| Call of the Wild profile | Exact supported profile passes and restores | Pending |
| Highest-risk combined profile | Exact supported combined profile passes and restores | Pending |

## Required lifecycle fields

Each relevant cast must correlate combat/cycle identity, current actor, caster
identity and initiative, action resources, spell/slot/metamagic/Acadamae mode,
command action type, spell completion, summon rule and pool, spawned unit
identity, entity-tick survival, live/combat registration, initiative and turn
membership, acted state, summon resources/controller/AI, first issued command,
first-action round, caster resources after spawn, and next-round scheduling.

Exact installed equivalents replace guessed field names. A row cannot pass on
visible behavior alone when native mechanical state is available.

## Pure policy cases

- not in combat;
- RTwP;
- not a summon;
- summon outside the caster's active turn;
- already eligible summon;
- missing opportunity;
- already acted;
- duplicate callback;
- distinct units from one multi-summon cast;
- next-round token mismatch.
