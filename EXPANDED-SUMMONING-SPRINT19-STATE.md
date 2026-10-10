# Expanded Summoning Sprint 19 state — Girallon and Xill

Controlling state for this branch only. Compact by design: the historical
Expanded Summoning journal is not copied here. The frozen primary-source
contract lives in `planning/EXPANDED-SUMMONING-SPRINT19-CONTRACT.json`.

## Current state, 2026-10-10 — QUALIFIED AND PUBLISHED; RELEASE PENDING

One sprint, one branch, one PR, one release. Girallon and Xill only. Giant
Scorpion, Bebelith, Giant Crab, Sprint 20, Sprint 21 and any broader phase are
out of scope and were not started.

Both creatures were registered with all ten of their roots withheld and stayed
withheld through every guarded runtime review. The complete hidden candidate
passed — mechanics 39/39, bodies 12/12 — publication removed the two
suppression names, and the published surface was then confirmed in game at
39/39. The release remains.

The push policy already allows this branch: the owner authorized adding Sprint
18's branch and the three later ones in one edit during Sprint 18, and nothing
else in that policy was touched.

## Intake, 2026-10-10

| Check | Result |
| --- | --- |
| Latest stable release | `v0.0.147` |
| `origin/master` and `v0.0.147` tag target | `95d610348d7d2322ddc00ef04fa06d2cd352dbdd` |
| Master delta vs expected baseline | NONE |
| Branch base | `95d610348d` (exact released master) |
| Worktree at intake | clean |
| Live source owner | none |
| Kingmaker process | none, at intake and throughout |
| Runtime lease / lock | none |
| Staging transaction | none in flight |
| Installation baseline | tree SHA256 `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` |
| Private reference bundle | `UnityEngine.PhysicsModule.dll` verified against the installed Steam runtime copy and recorded in local build provenance; machine-local build reference only, never committed or packaged |

## Exact candidate

| | |
| --- | --- |
| Version | `0.0.148-expanded-summoning-sprint19` |
| Package | `KingmakerGunslinger-0.0.148-expanded-summoning-sprint19.zip`, 346 members |

The 346 members are Sprint 18's 340 plus this sprint's four body files and two
icons. The count is pinned in the release gate, because the record first
carried Sprint 18's 340 unchanged and nothing would have caught it.

## Gates

| Gate | Result |
| --- | --- |
| Version-aware repository validation wrapper | PASS |
| Sprint 19 corruption fixtures | PASS 22/22 |
| Repository validator regression fixtures | PASS 3/3 |
| Complete unfiltered domain suite | PASS 2529/2529 |
| Clean exact-reference Release build | PASS |
| Build output validation | PASS |
| Strict standalone package validation | PASS, 346 members |
| Expanded Summoning orchestration | PASS, 168 assertions |
| Four-armed body Blender fixtures | PASS 5/5, including driver-chain sharing proved from built geometry |
| Batched Sprint 19 runtime review | PASS 39/39 on the eleventh candidate |
| Party-camera art and crowding review | PASS 12/12 on the second attempt |
| Publication gate | PASS in game — 39/39, both keys removed, 1044 published, 1073 visible |
| Release closure matrix | **PENDING** |

## One defect found at release

The first publication attempt failed, and correctly. `validate-build-output.ps1`
refused the release build because
`assets\sprint19-fourarmed\girallon-mesh.json` was not in it.

The four body files were copied by `Build-Local.ps1` and declared nowhere else,
so the local build, the 346-member package and its strict validation were all
complete while a clean `build.ps1` release build produced none of them. The
hand copy masked the missing declaration. Two things were wrong and both are
fixed: the project file now declares the four files the way it already declared
Sprint 18's, and the release was published through the provenance-checked
`Build-Local.ps1` path with the private reference bundle, which is the path
v0.0.147 used.

The offline gate now carries the catch, generally rather than for this sprint:
every shipped creature body must be required by `validate-build-output.ps1`, so
no build path can omit it silently, and copied by the release builder. A fixture
proves it rejects a body that only the builder knows about.

## Frozen arithmetic, derived from source

| | Before | After registration | After publication |
| --- | ---: | ---: | ---: |
| Unique creatures | 99 | 101 | 101 |
| Summon Monster roster / placements | 90 / 519 | 91 / 524 | 91 / 524 |
| Summon Nature's Ally roster / placements | 88 / 515 | 89 / 520 | 89 / 520 |
| Registered generated placements | 1034 | 1044 | 1044 |
| Withheld | 0 | 10 | 0 |
| Published generated | 1034 | 1034 | 1044 |
| Retained native wrappers | 29 | 29 | 29 |
| Visible choices | 1063 | 1063 | **1073** |
| Ledger entries | 2983 | 3005 | 3005 |

Ten roots are ten identities, not thirty: neither creature is templated. The
Girallon is a magical beast with no Summon Monster entry at all, so the
celestial and fiendish templates never reach it, and the Xill is already an
evil outsider.

## Guarded runtime transactions

Thirteen spent. Three passed and are the release's evidence.

| # | Outcome | What it found |
| ---: | --- | --- |
| 1 | FAIL | The scenario name was refused by a fourth allowlist inside the mod itself, which only answers after a deploy and a launch. Now covered offline by `TheReviewScenarioIsWiredAtEveryGate`. |
| 2 | FAIL | Mod init threw on an unknown natural weapon key and rolled back 2,376 registrations: the Girallon's two weapons were never added to the builder's key resolver. Now covered offline by `EveryProfileWeaponKeyResolvesInTheBuilder`. |
| 3 | STALL | My harness mistake — the run's timeouts were left at their defaults rather than the values a review this size needs. |
| 4 | 29/39 | Four invented placeholder body GUIDs, so neither body was attempted; the Girallon kept the donor's unit type; its hit points and two skills were wrong because the exact-rank component was gated to primates; the Xill had no shield bonus, no spell resistance and no paralysis trigger. |
| 5 | 31/39 | The Xill's configuration overwrote its donor prefab with a human one, copied from the method it was modelled on; the paralysis cases measured the condition left over from the case before. |
| 6 | CRASH | `NullReferenceException` in `UnitCommand.OnRun`. |
| 7 | CRASH | `NullReferenceException` in `UnitAttack.GetApproachRadius`. Same cause: a round-waiter I had added idled real-time combat for a full six-second round, through which the engine ends the fight and despawns the target's view. |
| 8 | 38/39 | Persuasion and Knowledge (arcana) each exactly three short: they are outsider class skills and the donor's class list does not carry them. |
| 9 | 38/39 | Retry accumulation. The rend tracker is scoped to the attack command, not to the review's idea of an attempt, so claws from abandoned attempts still counted. |
| 10 | 38/39 | `UnitAttack.TryMergeInto` folds a second attack on one target into a live command, so a "second sequence" could be the same command as its first. |
| 11 | **39/39 PASS** | `20261010T0649575611737Z-disposable-expanded-summoning-sprint19-review`, 608 s. |
| 12a | 11/12 | The Girallon captured mid-dissolve: the crowd review's native-fade wait was gated to three earlier creature families and did not cover a donor-rig body. |
| 12b | **12/12 PASS** | `20261010T0744422373426Z-working-save-expanded-summoning-creature-review`, all eight captures `renderer=true dissolve=0 intact=true`. |
| 13 | **39/39 PASS** | `20261010T0840306592691Z-disposable-expanded-summoning-sprint19-review`, the publication gate: ten roots live and published, nothing withheld, registered 1044 = published 1044. |

Every defect was this project's own. The live tree was restored to
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` after every
transaction, and no process, lease or lock was left outstanding.

## Accepted limitations

`FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS`, `MULTIWEAPON_ARMED_ROUTINE_UNREPRESENTED`,
`IMPLANT_UNREPRESENTED`, `ETHEREAL_TRAVEL_UNMODELED`,
`ORDINARY_MAP_LAND_USE_SCOPE`, `PASSIVE_CREATURE_SENSES_UNMODELED`, and the
paralysis duration bounded to the summon's lifetime. Each is recorded in
`docs/RELEASE-NOTES-0.0.148.md` with what the engine cannot carry and what was
deliberately not substituted for it.

## Released-history note

The v0.0.147 record in `validation/static-validation.json` and the 0.0.147
`CHANGELOG.md` entry are left exactly as they shipped: a released record is
immutable and this sprint's own gate enforces it. Both carry wording written
during Sprint 18's candidate pass and never amended at release — 58 appended
identities and a published surface frozen at 1008, where the release actually
appended 61, took the ledger from 2922 to 2983 and published 1034 generated
placements for 1063 visible choices. `docs/RELEASE-NOTES-0.0.147.md` records
that correctly. The discrepancy is identified in this sprint's record and
changelog entry rather than edited into history.

`HumanReview: NOT_PERFORMED_NONBLOCKING.`
