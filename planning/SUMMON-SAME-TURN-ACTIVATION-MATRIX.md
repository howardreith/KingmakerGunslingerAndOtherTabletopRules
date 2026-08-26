# Summon Same-Turn Activation Matrix

Status: CORE REPAIR QUALIFIED; COMPLETE MATRIX PENDING

| Case | Required evidence | Current disposition |
| --- | --- | --- |
| Ordinary unaccelerated native summon | Existing accepted scheduling and duration unchanged | Pending control |
| Acadamae OFF | Native Full-Round timing; no Acadamae save/consequence | Pending control |
| Acadamae ON | Standard action, one slot, one save/consequence, correct current-round summon opportunity | Pending reproduction |
| Legitimate Quickened summon | Swift action, caster retains Standard/Move, correct current-round summon opportunity | Initial fixed PASS: real path; Swift only; exact cast-round attack and normal next round |
| KMG Expanded Summoning | Real KMG choice follows the same general summon mechanism | Initial fixed PASS used actual KMG native-choice spell graph; non-native creature control pending |
| `1d3` or `1d4+1` | Every successful spawned unit receives exactly one initial opportunity | Pending |
| Duplicate callback | Same unit/opportunity correlation becomes a no-op | Initial fixed PASS: `AlreadyEligible`, exact buff state unchanged |
| Following round | Native scheduling; no duplicate initiative, command, or activation | Initial fixed PASS: one lawful turn, one native command sequence |
| Duration | Same-turn opportunity neither shortens nor extends native lifecycle | Initial fixed PASS: exact 120s; expiration/dismissal controls pending |
| RTwP | Native immediate AI behavior; no artificial turn state | Pending control |
| Cancelled/failed summon | No spawned unit and no activation state | Pending control |
| Non-summon spawn | Completely unaffected | Pending control |
| Save/load/reset | No stale project-owned activation marker | Pending |
| Standalone profile | Full guarded behavior | Pending |
| Call of the Wild profile | Exact supported profile passes and restores | Pending |
| Highest-risk combined profile | Exact supported combined profile passes and restores | Pending |

Initial fixed evidence is guarded run
`20260826T1534299629829Z-686e0463d5254e4b871d5f7a7fec1827` in
`20260826T1534299473030Z-summon-same-turn-activation`. Rows marked initial
remain subject to the frozen final-candidate fresh-process repetitions.

## Required lifecycle fields

Each relevant cast must correlate combat/cycle identity, current actor, caster
identity and initiative, action resources, spell/slot/metamagic/Acadamae mode,
command action type, spell completion, summon rule and pool, spawned unit
identity, entity-tick survival, live/combat registration, initiative and turn
membership, acted state, summon resources/controller/AI, first issued command,
first-action round, caster resources after spawn, and next-round scheduling.

Exact installed equivalents replace guessed field names. A row cannot pass on
visible behavior alone when native mechanical state is available.

## Proven pre-fix boundary

Guarded run `20260826T1445424590411Z-3b96e766c8b144449164781c019dcc51`
used a real prepared Quickened Summon Monster I and exact invocation-context
identity. Cast-round `TurnController.Prepare` observed
`CanActInCombat=true`, all three action resources available,
`SummonedUnitAppearBuff=true`, and `IsAbleToAct=false`. The same unit's next
round `Prepare` observed the appearance lock absent and `IsAbleToAct=true`.
Thus spawn, context, enrollment, initiative, and resource initialization all
succeed; the blueprint-derived appearance lock is the first failing state.

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
