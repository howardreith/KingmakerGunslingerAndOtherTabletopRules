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
| B4 | Sprint 0 persistence trio | DONE - PASS 9/9 x3 at 0.0.136; closeout PASS 11/11 x3 with the Pteranodon in the fixture and the party-camera review |
| B5 | Sprint 0 visual contracts | DONE - PASS 13/13 |
| B6 | Compatibility / feature-boundary profiles | DONE - the two mechanical runs PASS on the baseline-derived fixture (closeout) |
| C1 | Sprint 1 corrections verified in current code | DONE |
| C2 | Dev-only projected-menu fixture | DONE - live measurement obtained at 120 / 110 entries (closeout) |
| C3 | Menu rubric + measurements | DONE - rubric applied to the live measurement; criterion 8 refined to the widget's own cost (see rubric) |
| D1 | Rig contract: attached animation ActionSet/clips/events | DONE |
| D2 | Rig contract: per-bone rest transforms + bind matrices | DONE |
| E1 | Deformation proof | DONE - drift 0.00000, control unmoved |
| E2 | Finished Pteranodon mesh + textures | DONE - tip-apex crest, atlas UVs, painted albedo; internal visual review PASS |
| E3 | Mesh data + instance-local loader + fallback | DONE - schema 2 loader; both fallback paths exercised live in the fault drill |
| F1-F7 | Seven live acceptance groups | DONE on machine evidence (groups 1-7); HumanReview: PASSED for the Pteranodon candidate (owner statement 2026-09-24, revision unspecified); internal review of the party-camera captures PASS |
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
   fire. Disabled. Lifted in the closeout: the anchor is a group slot the
   game itself gave a party member selected through its own selection path,
   and the live 120/110 measurement was obtained (see the closeout section).

## Owner manual review of the Pteranodon candidate (2026-09-24)

The owner stated on 2026-09-24 that their manual review of the Phase 0
Pteranodon candidate passed, and activated the Phase 0 closeout and Sprints
3-8 order. Recorded exactly as given: no revision, installation, or build was
named by the owner, so the review is not linked to a deployment record; the
last pushed head at the time was `ba37f4b6`, and the live installation rested
at 0.0.117 throughout. The owner's approval closes the personal-review item
for the candidate they saw and nothing else: it waives no compatibility
mechanical run, no live menu measurement, and no other technical item.
HumanReview for the Pteranodon candidate: PASSED (owner statement, revision
unspecified). HumanReview for everything after it: NOT_PERFORMED_NONBLOCKING.

## Sprint 2 finished creature (2026-09-23/24)

- Asset commit `50004a65`: crest iteration 7 (tip apex, 1.05 back / 0.72 up
  from `Head`, base half-width 0.06 to 0.012), five-region atlas UVs with a
  belly-to-back fold, deterministic painter (`paint_pteranodon_albedo.py`,
  1024 x 1024 RGB, same bytes on every run), mesh data schema 2 with the albedo
  pinned by SHA-256, loader publishes mesh + albedo or neither, view patch
  dresses a private copy of the donor material. 1734 domain tests; package
  235/237 files; Build-Local now stages both Pteranodon files itself.
- Rider commits `270545e9` (dev-only
  `disposable-expanded-summoning-pteranodon-fault-drill`; isolation, crowding
  and repeated-lifecycle assertions on the mechanical run; presentation and
  motion-binding assertions on the visual-contracts run), `5ec5890f` (the
  visual swapped onto the donor's own renderer; crowd casts as their own
  list; the launcher waits for the previous game process) and `fafe1e26`
  (the launcher releases its own stale lock between scenarios), and
  `7cc80497` (the rig capture reads the pre-swap donor rig; the lifecycle
  check asks for one view per cast).
- Base runs on `50004a65` (each restored and verified to the 0.0.117 tree):
  `20260923T2348000940057Z-observe-summon-pteranodon-view-contracts` 13/13 (`visual:published`, deformation proof unchanged:
  drift 0.00000, control unmoved, BakeMesh agreement 0.00000);
  `20260923T2353191705789Z-disposable-expanded-summoning` 14/14 (both single Pteranodon casts
  `visual:attached;bones=46;vertices=682;albedo=1024x1024;shader=PF/StandardDynamic`);
  `20260923T2356321457517Z-disposable-expanded-summoning-visual-contracts` 13/13.
- Rider runs on `7cc80497`: `20260924T0112361790237Z-disposable-expanded-summoning-pteranodon-fault-drill` 18/18 PASS;
  `20260924T0115460941403Z-disposable-expanded-summoning` 17/17 PASS; `20260924T0118557039123Z-disposable-expanded-summoning-visual-contracts` 15/15 PASS. Live tree restored
  and verified after each batch; live install 0.0.117 / 136 files.
- Final probe on `523e540b` (the records commit): `20260924T0126332576716Z-observe-summon-pteranodon-view-contracts` 13/13 PASS, `visual:published`, deformation proof
  `bone=L_Feather_3;degrees=25;ownedMoved=2;ownedStill=0;ownedDrifted=0;worstDrift=0.00000;controlMoved=0;controlStill=100;worstControlMotion=0.00000;largestMotion=0.1446;bakeDisagreement=0.00000`; restored and verified.
- Material facts recorded live: the donor's `PF/StandardDynamic` declares
  _BumpMap of the probed map slots; cleared <none>.

## Phase 0 closeout (2026-09-24)

Closeout runs on `02bc5c47` (the integrated tree: `master` @ `996105ed`
merged in `9702d319`), every batch restored and verified to the 0.0.117 tree:

- Projected menu, live: `20260924T1647492722669Z-disposable-expanded-summoning-projected-menu`. PASS 3/3 on `02bc5c47`: Summon Monster at 120 entries and Nature's Ally at 110 both render every entry (120 and 110 slots), first, middle and last reachable, the popup inside the safe area (550 x 550 and 550 x 500 canvas units against a 1910 x 1070 safe rectangle, so no scrolling is needed at this size and none is installed), the first entry visible on open, no slot growth across three cycles; warm toggle 119-120 ms and 103-110 ms, the cold first open 3334 ms (slot instantiation, reported, not gated), this host at 425-1500 ms per frame under the harness. The subject is a
  party member selected through the game's own selection path (Hedwirg, 42
  active group slots); nothing installs an action-bar slot. Criterion 8 is
  scored on the synchronous toggle, with the run's first open reported as
  the cold open; the rubric records why.
- Persistence trio with the Pteranodon: prepare `20260924T1625111025628Z-working-save-expanded-summoning-prepare`
  PASS 11/11, verify-cleanup `20260924T1628426382345Z-working-save-expanded-summoning-verify-cleanup` PASS 11/11, verify-absent
  `20260924T1632136753152Z-working-save-expanded-summoning-verify-absent` PASS 11/11. The reloaded Pteranodon carries the attached
  visual on the freshly deserialized unit; the game's fader instantiates the
  renderer's material (Unity's " (Instance)" suffix) once a unit lives across
  frames, and the instance keeps the painting.
- Party-camera review (internal): eight party-camera renders on `00195475` (four per writing stage: idle, twice moving, attacking), each with the creature in frame, the screen lit, the renderer enabled and the material intact (dissolve 0.008 at idle, 0 after); the reloaded creature renders as a pterosaur - brown leather wings spread, red crest, long beak - beside the party at Large scale, moving across the room and biting, with the summoned Wolf and Small Air Elemental in the same frames. Internal review of the images: PASS.
- Two defects the closeout found and repaired, each bounded to the seam that
  failed (`b7a0b6da`): the Pteranodon's material clone was taken in the
  donor's dissolve state and never adopted by the view's material controller,
  so the game's fades and tints passed it by and a clone taken fully
  dissolved stayed invisible in the party camera - the review images on
  `6dec9738` showed no creature while every mechanical observer was
  satisfied; and the variant menu's scrolling installer measured the popup's
  preferred size in the root's local units against canvas-unit rectangles,
  so a 550-unit popup asked for 1100 and 120 entries engaged a viewport they
  did not need - a viewport that added three layout groups to one object
  where Unity allows one, faulted on the null second and, once past that,
  drew a panel four times its grid. The clone now starts intact and the
  controller re-reads the renderer's materials; the content carries one
  layout group of the native type and the sizes convert to canvas units
  (`02bc5c47`).
- HumanReview: the owner's passed review (2026-09-24, revision unspecified)
  stands as recorded; it predates the repair above, whose build has been
  reviewed internally from the party camera only - HumanReview for it:
  NOT_PERFORMED_NONBLOCKING.
- Compatibility mechanical on the baseline-derived fixture (record
  `20260924T1203525961325Z`): `gunslinger-only` `20260924T1208085331010Z-disposable-expanded-summoning` PASS 17/17;
  `gunslinger-high-risk-combined` `20260924T1213545977986Z-disposable-expanded-summoning` PASS 17/17.
  Fixture `269a9beda2b736f5` derived from the protected baseline `cc7cbb0d08581873`
  with only its header name changed and no foreign serialized types; the
  working save `d832bcfcbbd31ac2` restored byte for byte with its timestamps after
  each transaction. The earlier timeouts were the working save's 465
  TweakOrTreat and 32 Call of the Wild `$type` entries, unresolvable inside a
  profile that stages neither mod.
- Regression on the integrated tree: mechanical `20260924T1616435094695Z-disposable-expanded-summoning (17/17 on 00195475; every Pteranodon `adopted=true`; 17/17 again inside each compatibility transaction on fd31a0e5)`, visual contracts
  `20260924T1619405601959Z-disposable-expanded-summoning-visual-contracts (15/15 on 00195475)`, fault drill `20260924T1622134839883Z-disposable-expanded-summoning-pteranodon-fault-drill (18/18 on 00195475)`, player path `20260924T1305336453485Z-disposable-expanded-summoning-player-path`
  PASS 10/10 (6dec9738; lease released cleanly with the scaled exit wait), inventory `20260924T1219459315268Z-observe-expanded-summoning-inventory (38/38 on fd31a0e5)`, probe `20260924T1637399702201Z-observe-summon-pteranodon-view-contracts (13/13 on 00195475)`,
  same-turn family `20260924T0505196230195Z` activation 10/10, `20260924T0511162702784Z` acadamae 11/11, `20260924T0517130971040Z` multiple 10/10, `20260924T0523309103824Z` native-control 7/7, `20260924T0529318624492Z` rtwp-control 5/5 (22b0d72c), and `20260924T1236387538486Z-disposable-acadamae-graduate` 20/20, `20260924T1238286248628Z-disposable-shield-other` 23/23, `20260924T1240184174861Z-disposable-elemental-race-mechanics` 27/27 (fd31a0e5).
- Platform: qualified on Windows 10 Pro 10.0.19045 with the installed Steam
  build only. Linux/Proton was not available and is not claimed.
- Recorded separately: HumanReview PASSED for the Pteranodon candidate (owner
  statement, revision unspecified) and NOT_PERFORMED_NONBLOCKING for the
  closeout work; InternalAcceptance per item above; technical qualification
  is the runtime evidence listed here.

## Next executable action

Phase 0 is finalized: merge PR #21 at its reviewed head, verify the integrated
master build and package, then open the Sprints 3-8 worktree, branch and draft
PR under the 2026-09-24 order. Sprint 3 starts there, not here.
