# Expanded Summoning Sprint 18 state — Ape and Dire Ape

Controlling state for this branch only. Compact by design: the historical
Expanded Summoning journal is not copied here. The frozen primary-source
contract lives in `planning/EXPANDED-SUMMONING-SPRINT18-CONTRACT.json`.

## Charter

One sprint, one branch, one PR, one release. Ape (Gorilla) and Dire Ape
(Gigantopithecus) only. Girallon, Xill, Giant Scorpion, Bebelith, Giant Crab,
Sprint 19 and any broader phase are out of scope. Stop for owner review when
the release is published.

## Intake, 2026-10-09

| Check | Result |
| --- | --- |
| Latest stable release | `v0.0.146` |
| `origin/master` | `da782d2297d6cf361ac30a5e0ea07348d3b7f7d7` |
| `v0.0.146` tag target | `da782d2297d6cf361ac30a5e0ea07348d3b7f7d7` |
| Master delta vs expected baseline | NONE |
| Branch base | `da782d22` (exact released master) |
| Worktree | clean |
| Live source owner | none (no `artifacts/**/source-ownership.json`, no laptop owner receipt) |
| Kingmaker process | none |
| Runtime lease / lock | none (`compatibility-state/compatibility.lock` absent) |
| Staging transaction | none in flight; `artifacts/deploy-staging/` holds a stale 2026-09-13 copy and no journal |
| Installation baseline | 136 files, `Info.json` 0.0.117, tree SHA256 `2043B4587E5D25197AE873BBB69A90C367F836D86499F25892D225016F93C593`, Info SHA256 `1040695B0AE4339B2A935BE3FE06828BE80A451B4EA86BAE11A7C16D6DCE5CFF`, DLL SHA256 `FD2FC61C250B13857D81ACC197A896450F5B242EE392FA7192B01201E908F35F` |
| Save baseline | 118 saves; `KMG_AUTOMATION_BASELINE` (Manual_298) and `KMG_AUTOMATION_WORKING` (Manual_299) present |

## Frozen arithmetic, derived from source

`ExpandedSummoningCatalog` quantity semantics evaluated for parent tiers 1-9:

| | Before | After |
| --- | ---: | ---: |
| Unique creatures | 97 | 99 |
| Summon Monster roster | 88 | 90 |
| Summon Monster placements | 506 | 519 |
| Summon Nature's Ally roster | 86 | 88 |
| Summon Nature's Ally placements | 502 | 515 |
| Registered generated placements | 1008 | 1034 |
| Templated placements | 287 | 300 |
| Retained native wrappers | 29 | 29 |
| Visible choices | 1037 | 1063 |

Ape: 7 Summon Monster + 7 Summon Nature's Ally = 14 roots (parent tiers 3-9).
Dire Ape: 6 + 6 = 12 roots (parent tiers 4-9). Sprint total 26.

## Native rend audit result

Read-only Mono.Cecil IL inspection of the private compiler-input reference
assembly. No game process, blueprint library or game data was touched.

- `Kingmaker.Designers.Mechanics.Facts.RendFeature` is an exact native rend
  **damage** carrier: on `IsRend && AttackRoll.IsHit` it deals
  `RendDamage` dice plus `(int)(Strength.Bonus * 1.5)` as one `RuleDealDamage`.
  At Strength 19 and 1d4 dice that is the printed 1d4+6 exactly, and it follows
  live Strength. **Adopted unchanged.**
- `UnitAttack.IsRend(AttackHandInfo)` is the native **gate**: it requires the
  rending attack to come from `Body.SecondaryHand` and the immediately
  preceding executed attack to have hit from `Body.PrimaryHand`. It identifies
  a rend by hand slot rather than by limb, so it cannot express a bite plus two
  equal primary claws, it would rend from a bite-then-claw pair, and it never
  checks that both hits landed on the same target. **Not adopted.**
- The same gate also selects `UnitAnimationActionHandle.IsRendAttack`, so the
  native rend animation variant is not selected. Recorded as an honest
  presentation deviation; nothing is invented to replace it.

Adopted design: a bounded Dire-Ape-owned component replaces exactly one
decision — whether this attack is a rend — and the native `RendFeature`
resolves the damage. No global patch, no multi-hit framework.

## Donor and rig

Offline pre-census from the retained
`20261002T1440160968919Z-observe-expanded-summoning-native-donors` audit: the
installed library's 106 unit types contain **no** primate type, and no audited
fact carries a `RendFeature` component. There is no native primate donor and no
native rend creature to clone. Candidate families ranked in the contract record;
`Trolls` (50 units) leads on silhouette and contacts. The bounded read-only
primate donor census with bind-rig capture has **NOT RUN**, and the original
mesh generators cannot be authored before it: the Sprint 16/17 generators read a
request-local donor bind-pose capture as their only game-sourced input.

## Progress

| Mission section | State |
| --- | --- |
| 1 Intake and branch | DONE (branch created; draft PR pending first push) |
| 2 Freeze primary-source contract | DONE — `planning/EXPANDED-SUMMONING-SPRINT18-CONTRACT.json` |
| 3 Registration, identity, publication | NOT STARTED |
| 4 Donor and rig audit | PRE-CENSUS DONE OFFLINE; GUARDED CENSUS NOT RUN |
| 5 Original visuals | BLOCKED ON SECTION 4 |
| 6 Mechanics | DESIGN FROZEN; NOT IMPLEMENTED |
| 7 Skills, senses, omitted movement | DESIGN FROZEN; NOT IMPLEMENTED |
| 8 Tests and cadence | NOT STARTED |
| 9 Hidden candidate qualification | NOT STARTED |
| 10 Publication | NOT STARTED |
| 11 Release closure | NOT STARTED |
| 12 Merge and release | NOT STARTED |

## Candidate budget

| Budget | Maximum | Used |
| --- | ---: | ---: |
| Hidden candidate | 1 | 0 |
| Product-correction candidate | 1 | 0 |
| Observation-only correction | 1 | 0 |

## Version surface

Expected `0.0.147`, tag `v0.0.147`, package
`KingmakerGunslinger-0.0.147-expanded-summoning-sprint18.zip`. The 0.0.146
validator freezes `src/KingmakerGunslinger/Summoning/`, the Expanded Summoning
builders and the shipped creature asset directories against exact commits and
asserts `sprint18Started == false`, so the version surface must advance with
this sprint and a new `tools/validate_expanded_summoning_sprint18147.py` must
inherit the chain.

## Accepted engine limitations carried by this sprint

- `PASSIVE_CREATURE_SENSES_UNMODELED` — low-light vision and scent on both apes.
- `ORDINARY_MAP_LAND_USE_SCOPE` — the 30-foot climb speed and the printed Climb
  skill on both apes. Mobility is never substituted for Climb.

## Human review

`HumanReview: NOT_PERFORMED_NONBLOCKING`.
