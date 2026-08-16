# Brown-Fur Transmuter fidelity matrix

Status: the listed mechanical and compatibility rows passed on immutable source
commit `2ef6e933ff521dff2330a948336a38083e741082`, but its human review failed.
Toggle state/resource presentation and pre-command Share target acquisition are
blocking fidelity gaps under repair. The final 128-state release seal has not
run.

| Tabletop or product contract | Kingmaker implementation | Result |
| --- | --- | --- |
| Powerful Change at level 3 | One-shot six-stat selection; exactly one qualifying Arcanist-slot Transmutation receives +2, or +4 at level 20 | PASS |
| Original bonus semantics | Execution-scoped modifier adjustment preserves Enhancement, Polymorph, and Size descriptors, source spell, duration, dispel, expiration, persistence, and recast behavior | PASS |
| Multiple-stat choice | Only the selected qualifying ability is enhanced; an invalid stat rejects before reservoir or slot expenditure | PASS |
| Powerful Change source boundary | Actual CotW Arcanist spellbook and prestige advancement qualify; items, SLAs, supernatural abilities, and unrelated spellbooks do not | PASS |
| Share Transmutation pre-command targeting | Armed eligible Personal spells must enter unit-target selection before command construction | FAIL in `0.0.81`; repair pending |
| Share Transmutation at level 9 | Genuine Personal Transmutation spells convert per execution to Touch for a willing creature without mutating shared blueprints | Mechanical delivery PASS; player acquisition repair pending |
| Share Transmutation at level 20 | Exact 30-foot boundary; over-30-foot targets reject | PASS |
| Willing creature policy | Self, party, controlled companions, pets, summons, and proven friendly non-attackable allies qualify; enemies, objects, dead targets, and ambiguous factions reject | PASS |
| Share source boundary | Genuine spells from another actual spellbook qualify; item, SLA, and supernatural activation do not | PASS |
| Combined use | Powerful Change and Share form one immutable transaction, cost exactly two reservoir points, apply once, and reject before slot spend when insufficient | PASS |
| Transmutation Supremacy | Level-20 genuine Transmutation casts gain execution-scoped Extend without higher slot or longer cast time | PASS |
| Extend exclusions | Already Extended spells do not extend twice; instantaneous, permanent, absent-duration, and ineligible selector structures remain unchanged | PASS |
| CotW metamagic interoperability | Prepared metamagic, Metamixing, variant, and converted spell paths retain their native behavior | PASS |
| Transaction cleanup | Cancellation, interruption, exception, combat/load cleanup, concurrent scopes, and one-shot selection cleanup retain no global cast state | PASS |
| Archetype progression | Adds Powerful Change 3, Share Transmutation 9, Supremacy 20; replaces resolved exploit opportunities 3/9 or 4/10; removes Magical Supremacy | PASS |
| Publication transaction | Appends one stable Brown-Fur archetype while preserving six existing CotW archetypes and order; rollback is isolated and idempotent | PASS |
| Module OFF | Registers stable identities and preserves existing owners while hiding Brown-Fur from new selection | PASS |
| CotW absent/incompatible | Brown-Fur remains unpublished; absent CotW registers no dependent identities; all six independent modules continue | PASS |
| Installed spell inventory | 86 roots and 177 canonical/variant/ConvertedFrom rows: 174 generic adapters, 3 named adapters, 0 unexplained | PASS |
| Native Powerful Change presentation | Six exclusive activatable toggles, selected overlay, distinct icons, and live shared reservoir count | FAIL in `0.0.81`; repair pending |
| Seven-module boundary | All ON, all OFF, seven ON-alone, and seven OFF-with-others-ON states | Prior candidate PASS 16/16; rerun required |

## Authorized adaptations and boundaries

- Kingmaker willingness is determined from native party, control, pet, summon,
  faction, friendliness, and attackability surfaces and fails closed when those
  surfaces disagree.
- Initial scope uses Kingmaker's normal one-archetype selection model; no new
  generalized archetype stacking infrastructure is introduced.
- Compatibility is structurally gated. A future CotW build with an unknown or
  ambiguous Arcanist graph is Blocked rather than guessed.
- Removing CotW from a save containing its Arcanist or Brown-Fur is unsupported
  because the parent class belongs to CotW.

There are no unexplained installed spells or known silent bonus-mechanics
approximations in the qualified inventory. The early Share target-acquisition
failure is a real player-facing mechanics defect, not merely a presentation
issue.
