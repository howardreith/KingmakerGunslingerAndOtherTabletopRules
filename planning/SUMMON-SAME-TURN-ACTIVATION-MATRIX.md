# Summon Same-Turn Activation Matrix

Status: STANDALONE RUNTIME MATRIX QUALIFIED; OPTIONAL PROFILES AND FINAL VERSION PENDING

| Case | Required evidence | Current disposition |
| --- | --- | --- |
| Ordinary unaccelerated native summon | Existing accepted scheduling and duration unchanged | PASS: Full-Round control retains both native six-second buffs; first lawful opportunity is cast round + 2 |
| Acadamae OFF | Native Full-Round timing; no Acadamae save/consequence | PASS in ordinary control |
| Acadamae ON | Standard action, one slot, one save/consequence, correct current-round summon opportunity | PASS: real prepared spell; one save/publication; exact current-round and next-round opportunities |
| Legitimate Quickened summon | Swift action, caster retains Standard/Move, correct current-round summon opportunity | PASS twice: real path; Swift only; exact cast-round attack and normal next round |
| KMG Expanded Summoning | Real KMG choice follows the same general summon mechanism | PASS: four actual KMG Eagles through the real `1d4+1` spell option |
| `1d3` or `1d4+1` | Every successful spawned unit receives exactly one initial opportunity | PASS: four distinct Eagles, each exactly once in both observed rounds |
| Duplicate callback | Same unit/opportunity correlation becomes a no-op | PASS per spawned unit: `AlreadyEligible`, canonical state unchanged |
| Following round | Native scheduling; no duplicate initiative, command, or activation | PASS: one normal next-round opportunity per accelerated summon |
| Duration | Same-turn opportunity neither shortens nor extends native lifecycle | PASS: accelerated 120s; Acadamae 12s expires exactly at cast round + 2; ordinary native 18s including grace |
| RTwP | Native immediate AI behavior; no artificial turn state | PASS: native two-second appearance clears; native AI acts; no current turn/order/forced turn |
| Cancelled/failed summon | No spawned unit and no activation state | PASS: out-of-range real command preserves slot and produces zero summon rules |
| Non-summon spawn | Completely unaffected | PASS: live combat control has no summon lifecycle/appearance or summon callback |
| Save/load/reset | No stale project-owned activation marker | PASS by construction/source contract: command-end and scene-dispose cleanup; no serialized state |
| Standalone profile | Full guarded behavior | Pending |
| Call of the Wild profile | Exact supported profile passes and restores | Pending |
| Highest-risk combined profile | Exact supported combined profile passes and restores | Pending |

## Standalone guarded evidence

| Scenario | Run ID | Result |
| --- | --- | --- |
| Initial fixed Quickened | `20260826T1534299629829Z-686e0463d5254e4b871d5f7a7fec1827` | PASS |
| Acadamae Standard | `20260826T1631499603377Z-713c7efcecab4c98963f8ca5a72b6650` | PASS |
| KMG `1d4+1` multiple | `20260826T1701469324812Z-78e74018a3c4426194e6ebae8fc9632a` | PASS |
| Ordinary/Acadamae-OFF/negative controls | `20260826T1717124156319Z-44e2710f9b4042e0a7b46c6a9a64c668` | PASS |
| RTwP control | `20260826T1725541089257Z-fb0b13a2dfab4e3a95e33571bf99925c` | PASS |
| Fresh Quickened repeat | `20260826T1729361837486Z-690d0e2c18d9463bbd16232d6d070ab0` | PASS |
| Cleanup-adjusted Quickened repeat | `20260826T1738486014413Z-aa53964345fa417da346a60852014684` | PASS |

These runs qualify the standalone 0.0.103 investigation candidate. The final
0.0.104 source freeze still requires optional-profile qualification and the
mandated fresh-process accelerated repeats.

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
