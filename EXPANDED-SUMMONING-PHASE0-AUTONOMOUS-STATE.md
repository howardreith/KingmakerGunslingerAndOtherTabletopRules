# Phase 0 autonomous completion state

Durable working state for the owner's standing autonomous-completion order.
Read this first after any context change and continue from `Next executable
action`. Do not restart the project or rerun completed work.

## Authority

- Standing owner order: autonomous completion of Sprints 0-2.
- Intermediate technical, menu/usability and visual acceptance are **delegated
  to internal review**. Record `OwnerDelegationGranted`, `InternalAcceptance`
  and `HumanReview: NOT_PERFORMED_NONBLOCKING` separately. Never record
  `OwnerAccepted` or `HumanVisualReviewPassed`.
- Hard boundaries unchanged: no merge, no release, no permanent deployment, no
  Sprint 3, no protected-baseline writes, no force-push.

## Branch and candidate

- Branch `codex/expanded-summoning-phase0-sprints0-2`, draft PR #21.
- Accepted baseline: `master` @ `35555a8d`, release 0.0.136.
- Live install must end at 0.0.117 / `FD2FC61C250B1385...` / 136 files.

## Acceptance items

| ID | Item | State |
|---|---|---|
| A1 | Orchestration: result isolation + recovery bounded tests | TODO |
| A2 | Restoration wrapper hardened (exit race, stale lock, teardown fault) | DONE |
| B1 | Sprint 0 structural baseline at 0.0.136 | DONE - PASS 38/38 |
| B2 | Sprint 0 mechanical (`disposable-expanded-summoning`) | TODO |
| B3 | Sprint 0 player-path | TODO |
| B4 | Sprint 0 persistence trio | TODO |
| B5 | Sprint 0 visual contracts | TODO |
| B6 | Compatibility / feature-boundary profiles | TODO |
| C1 | Sprint 1 corrections verified in current code | DONE |
| C2 | Dev-only projected-menu fixture, real UI, unattended | TODO |
| C3 | Menu measurements vs baseline + rubric | TODO |
| D1 | Rig contract: attached animator controller/clips/events | TODO |
| D2 | Rig contract: per-bone rest transforms + bind matrices | TODO |
| E1 | Membrane test mesh deformation proof | TODO |
| E2 | Finished Pteranodon mesh + textures | TODO |
| E3 | Bundle + instance-local loader + fallback | TODO |
| F1-F7 | Seven live acceptance groups | TODO |
| G | Reports, PR update, internal review closure | TODO |

## Verified facts (do not re-derive)

- Coverage: 67 project-owned + 11 retained-native = **78 represented**, 77
  published somewhere, Dire Bat registered-hidden, **67** still to allocate.
- Frost Giant: unit exists via native wrapper, SM VIII Published, SNA VII
  Planned.
- Ideal core: 145 creatures, 120 SM / 110 SNA base entries, 624 + 608 = **1,232**
  placements. SM IX = 120, SNA IX = 110 (no second wrapper addition).
- Donor: `406c1e1af5400ac4881e330502ccbd9e` `CR3_GiantEagleStandard`, prefab
  `GiantEagle`, shared by eagle / dire-bat / pteranodon / roc.
- Rig: one `SkinnedMeshRenderer` `eagle_boss1`, root bone `LowerTorso`, 72 bones
  / 72 bind poses, one `PF/StandardDynamic` material, Animator on child
  `GiantEagleBoss_Body_RIG_02` with a generic Avatar. Controller and clips bind
  at attach time, not on the detached prefab.
- Wing chain is feathered: `Arm_Upper > Arm_Lower > Palm > Feather_1..6` per
  side. A pterosaur membrane must be authored onto those feather bones.
- `Test-RuntimeScenarioPreflight.ps1` has 0.0.134 hardcoded in 25 places and
  fails at 0.0.136. Pre-existing, out of scope, recorded only.

## Operating rules learned the hard way

- `.gitattributes` mandates LF for `.cs/.csproj/.json/.py/.md` and CRLF for
  `.ps1`. Source-scanning tests assert exact multi-line LF tokens.
- Adding domain tests moves the pinned `Case("` count in two validators and two
  `static-validation.json` blocks together.
- A fresh worktree needs machine-local `GamePath.props` and
  `artifacts/inspection/bodyguard-native/Assembly-CSharp.il`.
- `UnityEngine.PhysicsModule` types are outside the qualified reference bundle.
- Never trust a green signal without checking provenance: evidence directory
  dates, restoration result, and the scenario's own recorded status are separate
  facts from the wrapper's exit code.

## Next executable action

Run the save-backed Sprint 0 baselines (B2, B3) through
`scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1` with
`-SaveName KMG_AUTOMATION_WORKING`, now that the attached-view scenario is
allowed its timeouts.
