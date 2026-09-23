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
| A1 | Orchestration: result isolation + recovery bounded tests | DONE - 41 assertions in the repo gate |
| A2 | Restoration wrapper hardened (exit race, stale lock, teardown fault) | DONE |
| B1 | Sprint 0 structural baseline at 0.0.136 | DONE - PASS 38/38 |
| B2 | Sprint 0 mechanical (`disposable-expanded-summoning`) | DONE - PASS 13/13 |
| B3 | Sprint 0 player-path | DONE - PASS 10/10 |
| B4 | Sprint 0 persistence trio | DONE - PASS 9/9 x3 |
| B5 | Sprint 0 visual contracts | DONE - PASS 13/13 |
| B6 | Compatibility / feature-boundary profiles | DONE except two owed mechanical runs |
| C1 | Sprint 1 corrections verified in current code | DONE |
| C2 | Dev-only projected-menu fixture | BUILT; live measurement NOT OBTAINED |
| C3 | Menu rubric + measurements | Rubric written and enforced; live claim narrowed |
| D1 | Rig contract: attached animation ActionSet/clips/events | DONE |
| D2 | Rig contract: per-bone rest transforms + bind matrices | DONE |
| E1 | Deformation proof | DONE - drift 0.00000, control unmoved |
| E2 | Finished Pteranodon mesh + textures | Body done; crest polish, UVs and texture remain |
| E3 | Mesh data + instance-local loader + fallback | Loader DONE; fallback paths not yet exercised live |
| F1-F7 | Seven live acceptance groups | TODO |
| G | Reports, PR update, internal review closure | DONE |

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
- Animation is Playables, not Mecanim: `runtimeAnimatorController` is null and
  `UnitAnimationManager.ActionSet` carries the seven actions
  Bite/Claw/LocoMotion/MicroIdle/Prone/StandUp/Stunned, 7 clips and 7 events.
  View anchors: `ParticlesSnapMap`, `CapsuleCollider` soft / `MeshCollider`
  core, `Locator_HitFX_00` centre torso, corpulence 1.6, view scale 0.82.
- **The bind pose is spread, the live pose is folded.** In renderer space the
  bind rig spans 8.641 across against 2.937 long and mirrors left/right to
  1e-5; the live animated pose spans only 1.638 across against 3.152 long, and
  the two disagree by 3.954 at `L_Feather_1_end`. Author every replacement
  mesh against `rig.measured.json`, which is now built from bind poses. The two
  rejected Blender iterations were authored against the folded pose.
- Wing bones by span: `Arm_Upper` (shoulder) > `Arm_Lower` (elbow) > `Palm`
  (wrist) > `Feather_1..3`; `Feather_4/5` hang off the elbow and `Feather_6`
  off the shoulder. `Feather_1` is the longest and most forward, so it is the
  pterosaur's elongated fourth finger; `Feather_2..6` ends give the membrane's
  trailing-edge control points, sweeping back to the ankle.
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
- **Do not build, render, or run the test suite while a guarded scenario is
  loading.** Doing so starved a run past its 300s timeout; the launcher then
  could not restore because the process was still up, and the six remaining
  matrix steps each failed on that same leftover process. Recovery was manual:
  confirm `saveInteractionOccurred=false` and that the main menu was never
  reached, close the game, then
  `Restore-KingmakerCompatibilityProfile.ps1 -RunId <id>`.
- A batched runtime driver must stop the moment a step leaves Kingmaker running
  or a `Mods.kmg-compat-*` transaction open, and must fingerprint the live tree
  before and after. Carrying on produces a run of identical, uninformative
  failures and leaves the install mutated the whole time.
- Bash heredocs to `python -` in this environment collapse backslash escapes, so
  a literal backslash in a replacement string must be written `chr(92)`. A `\r`
  written the obvious way became a carriage return and split a line in this
  file, twice.
- Python's `Path.write_text` emits CRLF on Windows; `.gitattributes` wants LF for
  everything but `.ps1`. Pass `newline="\n"`, or write bytes.

## Sprint 0 live evidence at 0.0.136 (all PASS, all restored)

| Item | Evidence directory under `C:\Dev\KingmakerGunslingerLab\runtime-evidence` |
|---|---|
| B1 | `20260923T1457480279419Z-observe-expanded-summoning-inventory` 38/38 |
| B2 | `20260923T1638384106636Z-disposable-expanded-summoning` 13/13 |
| B3 | `20260923T1545367199226Z-disposable-expanded-summoning-player-path` 10/10 |
| B4 | `...1622143172370Z-prepare`, `...1625047870309Z-verify-cleanup`, `...1627525088084Z-verify-absent` 9/9 each |
| B5 | `20260923T1606571122538Z-disposable-expanded-summoning-visual-contracts` 13/13 |

## B6 results

Feature boundary, `gunslinger-only`, assertion PASS in both directions:

| module | publishedParents | placements | nativeOptions | preservation | unclassified | placementsExact | nativeVariants |
|---|---|---|---|---|---|---|---|
| enabled | 18 | 667 | 26 | 0 | 0 | True | 0 |
| disabled | 0 | 0 | 0 | 0 | 0 | True | 46 |

The module substitutes for the native summon menu rather than adding to it: all
46 native variants across the eighteen parents are replaced, and turning it off
leaves them untouched. Both runs' overall status is FAIL for two reasons that
are not this mission's - brown fur cannot pass in a profile without Call of the
Wild in either module state, and three teleportation-scroll assertions fail
identically on master's lineage at 0.0.132.

Compatibility, every scenario that ran PASS, inventory 38/38 in each:

| Profile | mods confirmed loaded |
|---|---|
| `gunslinger-only` | none |
| `gunslinger-call-of-the-wild` | CallOfTheWild 1.14.4c-2.1 |
| `gunslinger-arms-armor` | ArmsArmor |
| `gunslinger-toggle-custom-soundpacks` | ToggleCustomSoundpacks |
| `gunslinger-high-risk-combined` | ArmsArmor + CallOfTheWild + ToggleCustomSoundpacks |

`gunslinger-high-risk-combined` carried `CONFLICT-OBSERVED` for historically
timing out before readiness; with a 600s budget it reached readiness and passed.
That is evidence the timeout was a budget problem, not that the older conflict
never existed.

Live tree identical before and after the whole matrix, every step machine-clean.

Still owed: `disposable-expanded-summoning` under `gunslinger-only` and
`gunslinger-high-risk-combined`. The driver scheduled the supervised
`observe-expanded-summoning-variant-menu`, which aborted each profile before the
mechanical scenario; the driver now refuses supervised scenarios and a domain
test enforces it.

## The Unity licence is unusable; the asset ships as mesh data instead

The 2018.4.10f1 editor that built every previous AssetBundle here stopped
accepting its licence between 2026-08-21 and 2026-09-23. Same install, same
project, same batch command: success then, and now `BatchMode: Unity has not
been activated with a valid License` / `Missing or bad username and password`.
Only 2018.4.10f1 and a Hub 6000.5.6f1 are installed, and a 6000.x bundle will
not load in a 2018.4 game.

Rather than stop, the Pteranodon ships as ~70 KB of mesh data the runtime builds
a `Mesh` from. It carries strictly less than a bundle would - our geometry plus
the donor's bone names, no bind poses, no material, no import settings - has no
editor dependency, and is less code. The bundle builder is retained but is not
on the shipping path. **Re-activating the licence remains the owner's to do if a
bundle is ever wanted again; nothing here depends on it.**

## Deformation proof

`20260923T1951220917229Z-observe-summon-pteranodon-view-contracts`, 12/12 PASS:

    bone=L_Feather_3;degrees=25;ownedMoved=2;ownedStill=0;ownedDrifted=0;
    worstDrift=0.00000;controlMoved=0;controlStill=100;
    worstControlMotion=0.00000;largestMotion=0.1446;bakeDisagreement=0.00000

Vertices the rotated bone owns moved and held station in its frame exactly; 100
vertices on the opposite wing did not move; Unity's own `BakeMesh` agrees with
the evaluated skinning to zero. `ownedMoved=2` is small because the membrane
blends across bones and few vertices exceed the 0.8 dominance threshold - the
proof holds, but the owned population is thin and worth widening if the mesh
changes.

## C2/C3: the live projected-menu measurement was not obtained

Seven guarded runs. Two real defects were found and fixed along the way - the
runner's working-save chains, and the compatibility-lock ownership rule - and
both now have tests. The measurement itself is blocked by the fixture: no
`ActionBarGroupSlot` is active in `KMG_AUTOMATION_WORKING`'s hierarchy, and the
shipped layout anchors the popup to a clicked slot, so there is nothing to
measure against. Creating a save whose caster has a summon parent on the action
bar is a change to a shared disposable fixture and is recorded rather than done.

The claim is narrowed accordingly and stated in
`docs/EXPANDED-SUMMONING-PROJECTED-MENU-RUBRIC.md`: the layout policy is
measured exhaustively at domain level (four viewports x 200 counts), the real
menu is measured supervised, and no live measurement at 120/110 entries is
claimed.

## Two hard limits reached and reported

1. **Unity licence.** Two refreshes and a fresh Hub install of the same pinned
   2018.4.10f1 did not change the batchmode refusal. Both installs read the
   refreshed ULF and both then report not-activated. Blocks nothing: the asset
   ships as mesh data.
2. **Projected-menu anchor.** Installing an action-bar slot to anchor the popup
   hung the game - frames stopped, so even the frame-budget guard could not
   fire. Disabled. The live 120/110 measurement is marked not run.

## Next executable action

Finish E2: a crest that reads at silhouette scale, then UVs and a texture.
Acceptance groups 1 and 2 depend on it; groups 4, 6 and 7 need new scenarios.
