# Expanded Summoning Sprint 18 state — Ape and Dire Ape

Controlling state for this branch only. Compact by design: the historical
Expanded Summoning journal is not copied here. The frozen primary-source
contract lives in `planning/EXPANDED-SUMMONING-SPRINT18-CONTRACT.json` and the
source-gate evidence in
`planning/EXPANDED-SUMMONING-SPRINT18-SOURCE-EVIDENCE.json`.

## Current state, 2026-10-09 — SOURCE QUALIFIED; RUNTIME NOT QUALIFIED; BLOCKED

One sprint, one branch, one PR, one release. Ape (Gorilla) and Dire Ape
(Gigantopithecus) only. Girallon, Xill, Giant Scorpion, Bebelith, Giant Crab,
Sprint 19 and any broader phase are out of scope and were not started.

**Exact candidate `bd21f2447b3e68fbb95759292a1991320529f4e0`.** The complete
source, build and package gate passed once on that clean committed candidate.
Three gates remain unrun and one is an owner decision; see Blockers.

## Intake, 2026-10-09

| Check | Result |
| --- | --- |
| Latest stable release | `v0.0.146` |
| `origin/master` and `v0.0.146` tag target | `da782d2297d6cf361ac30a5e0ea07348d3b7f7d7` |
| Master delta vs expected baseline | NONE |
| Branch base | `da782d22` (exact released master) |
| Worktree at intake | clean |
| Live source owner | none |
| Kingmaker process | none, at intake and throughout |
| Runtime lease / lock | none (`compatibility-state/compatibility.lock` absent) |
| Staging transaction | none in flight |
| Installation baseline | 136 files, `Info.json` 0.0.117, tree SHA256 `2043B4587E5D25197AE873BBB69A90C367F836D86499F25892D225016F93C593` |
| Save baseline | 118 saves; `KMG_AUTOMATION_BASELINE` and `KMG_AUTOMATION_WORKING` present |

## Exact candidate

| | |
| --- | --- |
| Commit | `bd21f2447b3e68fbb95759292a1991320529f4e0` |
| Source state SHA256 | `b3c25e810d540c1507efa968c33c6157d254d8ba6469799eaece538259221f6f` |
| Version | `0.0.147-expanded-summoning-sprint18` |
| Package | `KingmakerGunslinger-0.0.147-expanded-summoning-sprint18.zip`, 336 members |
| Package SHA256 | `205cd6585f477f5d33769115cd942b4a9aa1d91fe1a4ccd3ea8935203e378ebd` |
| DLL SHA256 | `9b64c911e67d87bd58c5bfa644c0b11502424f25d2341da5ec3c112fbcd8c2ec` |
| DLL MVID | `ff9911e3-7061-4fc4-8d5a-f7fa1b234955` |

## Gates

| Gate | Result |
| --- | --- |
| Version-aware repository validation wrapper | PASS |
| Sprint 18 corruption fixtures | PASS 13/13 |
| Repository validator regression fixtures | PASS 3/3 |
| Icon catalog fixtures | PASS 29/29 |
| Complete unfiltered domain suite | PASS 2508/2508, including 10 new Sprint 18 cases |
| Clean exact-reference Release build | PASS, 14 exact private references |
| Build output validation | PASS |
| Strict standalone package validation | PASS, 336 members |
| Expanded Summoning orchestration | PASS, 168 assertions |
| Guarded primate donor census | **NOT RUN** |
| Batched Sprint 18 runtime review | **NOT RUN** |
| Publication gate | **NOT RUN** |
| Release closure matrix | **NOT RUN** |

## Frozen arithmetic, derived from source

| | Before | After registration | After publication |
| --- | ---: | ---: | ---: |
| Unique creatures | 97 | 99 | 99 |
| Summon Monster roster / placements | 88 / 506 | 90 / 519 | 90 / 519 |
| Summon Nature's Ally roster / placements | 86 / 502 | 88 / 515 | 88 / 515 |
| Registered generated placements | 1008 | 1034 | 1034 |
| Suppressed | 0 | 26 | 0 |
| Published generated placements | 1008 | 1008 | 1034 |
| Retained native wrappers | 29 | 29 | 29 |
| Visible choices | 1037 | 1037 | 1063 |
| Ledger entries | 2922 | 2980 | 2980 |
| Project icon concepts | 109 | 111 | 111 |

Ape: 7 Summon Monster + 7 Nature's Ally = 14 roots (parent tiers 3-9).
Dire Ape: 6 + 6 = 12 roots (parent tiers 4-9). Sprint total 26.

## What is implemented

- **Ape**: exactly two primary slams on a project-owned 1d6 slam weapon, each at
  the printed bonus with the whole Strength modifier. No bite, no claw, no rend,
  no invented fear or roar. Large, Space 10 / Reach 10, so no reduced-reach
  carrier; nothing printed grants a bonus against trip, so no trip-defence
  carrier.
- **Dire Ape**: one primary bite and two primary claws in the layout this
  project already qualified for the grizzly bear, dire bear and owlbear, so all
  three are at the printed bonus with full Strength.
- **Rend**: the engine's own `RendFeature` deals 1d4 plus one and a half times
  the live Strength modifier as a single damage event, which is the printed
  1d4+6 unmodified. A bounded Dire-Ape-owned gate decides only whether an attack
  is that rend, keyed on the live attack command, and holds no serializable
  state. It is unreachable by any other creature.
- **Skills**: exact profile-controlled ranks (Ape one Mobility, one Perception;
  Dire Ape one each of Mobility, Perception, Stealth). The generic priority-list
  allocation is switched off for both.
- **Icons**: two original project-owned 128x128 paintings rendered by the
  project's own procedural Blender creature tool. Visibly distinct in colour,
  silhouette, muzzle and hands.

## Honest omissions carried by this sprint

- `ORDINARY_MAP_LAND_USE_SCOPE` — the 30-foot climb speed and the printed Climb
  skill (+14 Ape, +16 Dire Ape, including the +8 racial bonus). The printed rank
  that bought Climb is not reallocated; Mobility is not inflated and Athletics is
  not raised to simulate the racial bonus.
- `PASSIVE_CREATURE_SENSES_UNMODELED` — low-light vision and scent on both apes,
  with no substitute sense and no vision-range override.
- **Rend animation variant**: the engine selects it from its command-level rend
  gate, which this sprint does not use, so the variant is not played. The rend is
  still a visible damage event. Presentation deviation, nothing invented.
- **Original bodies NOT authored**: both apes ride the Owlbear donor rig and are
  recorded as borrowed-body visual proxies.

## Blockers

1. **`PUSH_POLICY_ALLOWLIST` — owner decision.**
   `codex-policy/Push-KingmakerGunslinger.ps1` refuses
   `codex/expanded-summoning-sprint18-ape-dire-ape` as non-allowlisted. The
   policy was run exactly as `AGENTS.md` requires and was not changed or
   bypassed. This is the same boundary the v0.0.143 release hit and the owner
   resolved by editing the allowlist. It blocks the push, the draft PR, the
   merge, the tag and the release.
   *Smallest decision:* add that branch name to `AllowedBranches`.
2. **`PRIMATE_DONOR_CENSUS_NOT_RUN`.** The Sprint 16/17 mesh generators take a
   request-local donor bind-pose capture as their only game-sourced input, so the
   original bodies cannot be authored before the guarded census runs.
3. **`SPRINT18_RUNTIME_REVIEW_NOT_RUN`.** No live profile, combat, rend, visual,
   crowding, module-disabled or persistence evidence exists.

Neither ape can release independently: both share the same unauthored visual
seam, the same unrun runtime review and the same push boundary.

## Machine state

Installation is byte-identical to intake: 136 files, tree SHA256
`2043B458…93C593`, `Info.json` 0.0.117. Saves unchanged at 118. No Kingmaker
process, no deployment (`Build-Local` reported "No deployment was performed"),
no runtime lease, no staging transaction, no save write.

One build input was repaired: `UnityEngine.PhysicsModule.dll` was missing from
the local private reference bundle and was copied read-only from the installed
Steam runtime, which is the same source the project's own export script uses.
Its bytes now hash identically to the installed file; the other 13 references
were already identical and none changed. Nothing in the repository, the
installed mod or the game was modified.

## Progress

| Mission section | State |
| --- | --- |
| 1 Intake and branch | DONE except the draft PR, which the push policy blocks |
| 2 Freeze primary-source contract | DONE |
| 3 Registration, identity, publication | REGISTERED AND WITHHELD; publication NOT RUN |
| 4 Donor and rig audit | Offline pre-census DONE; guarded census NOT RUN |
| 5 Original visuals | NOT STARTED (blocked on section 4) |
| 6 Mechanics | IMPLEMENTED; source-proved; runtime NOT PROVED |
| 7 Skills, senses, omitted movement | IMPLEMENTED; source-proved; live totals NOT PROVED |
| 8 Tests and cadence | DONE — 10 Sprint 18 cases, 13 validator fixtures |
| 9 Hidden candidate qualification | Source half DONE; runtime half NOT RUN |
| 10 Publication | NOT RUN |
| 11 Release closure | NOT RUN |
| 12 Merge and release | BLOCKED on the push policy |

## Candidate budget

| Budget | Maximum | Used |
| --- | ---: | ---: |
| Hidden candidate | 1 | 0 |
| Product-correction candidate | 1 | 0 |
| Observation-only correction | 1 | 0 |

## Human review

`HumanReview: NOT_PERFORMED_NONBLOCKING`. Technical PASS is not owner visual
approval. Sprint 19 was not started.
