# Expanded Summoning Phase 0 implementation report

Charter: Kingmaker Expanded Summoning Implementation Charter v1.0
(`sha256 73b0a42807499841f987eaf49fd8500498c1289e58e631ed897c007dc9796493`).
Mission scope: Sprints 0, 1 and 2 only. **Sprint 3 remains not started.**

Technical status and owner acceptance are tracked separately throughout. No
human acceptance is declared anywhere in this report.

## 1. Baseline and identifiers

| Item | Value |
|---|---|
| Accepted default branch | `master` |
| Accepted starting SHA | `35555a8d8b4bd8df5a6868a60d3ca1fbb9cd366b` |
| Accepted release | 0.0.136 (`0.0.136-rapid-reload-proficiency-gate`) |
| Mission branch | `codex/expanded-summoning-phase0-sprints0-2` |
| Charter historical baseline | 0.0.114 at `6874dc15a27ded132456dbdd480f47c794543a05` - recorded and superseded, never reset to |

The charter baseline is an ancestor of the accepted master but well behind it.
Local `master` was eleven commits behind `origin/master` and was checked out in
another session's worktree, so this mission branched from `origin/master` and
left every other worktree untouched. `origin/HEAD` in the local checkout still
points at `codex/fifth-playtest-visual-native-feat-repair`; `git remote show
origin` reports `master` as the true default. That stale local symbolic ref is
an unrelated pre-existing condition and was deliberately not repaired here.

### Checkpoint commits

| Sprint | Commit | Subject |
|---|---|---|
| 0 | `54d1afab` | Freeze the Expanded Summoning baseline for charter Sprint 0 |
| 1 | `bd4deb95` | Represent the ideal summoning roster and prove the menus scale |
| 2 (audit) | `1404153b` | Audit the Pteranodon native view contract for charter Sprint 2 |
| intake | `89ac1522` | Reconcile the ideal roster against the supplied workbook and guide |

## 2. Current versus historical baseline counts

The shipped surface at 0.0.136 reproduces the charter's historical figures
exactly, so no discrepancy needed explaining away.

| Figure | Charter (0.0.114) | Observed (0.0.136) |
|---|---|---|
| Unique creatures | 67 | 67 |
| Summon Monster roster / placements | 66 / 361 | 66 / 361 |
| Summon Nature's Ally roster / placements | 57 / 320 | 57 / 320 |
| Registered logical placements | 681 | 681 |
| Suppressed (Dire Bat) | 14 | 14 |
| Native wrappers | 26 | 26 |
| Visible choices | 371 + 322 = 693 | 371 + 322 = 693 |

Borrowed-body proxies: **20**, not the 67 a naive count of non-empty view
policies produces. A view policy naming the creature itself (`boar<Boar`) is an
exact visual; only a differing name is a proxy. Sprint 2 addresses one of the
20.

## 3. Derived ideal manifest

| Figure | Charter | Derived | Workbook |
|---|---|---|---|
| Unique creatures | 145 | 145 | 145 |
| Already complete | 50 | 50 | 50 |
| Requiring work | 95 | 95 | 95 |
| Summon Monster base entries | 120 | 120 | 120 |
| Summon Nature's Ally base entries | 110 | 110 | 110 |
| Core placements | 1,232 | 624 + 608 = 1,232 | 1,232 |

Itemized diff against the charter expectation: **zero on every figure.**

The workbook was absent at intake and supplied by the owner mid-mission. The
manifest, which had been derived from charter Appendix A and B with that
provenance recorded, was then compared against the workbook's deduplicated
Shared Backlog sheet: identical creature set with none present on only one
side, and zero mismatches in Summon Monster tier, Summon Nature's Ally tier,
design priority, effort, or asset package. The five variant elemental families
and the 28 exclusions agree. No manifest change was required.

### Present versus planned coverage

Coverage is **derived** from the union of `ExpandedSummoningCatalog` and
`SummonNativeExpansionCatalog`, not stored as a hand-maintained column.

An earlier revision stored it as literal data filled from the project-owned
catalog alone. That marked the eleven creatures which ship solely as retained
native wrappers - Axiomite, Bogeyman, Frost Giant, Hamadryad, Manticore, Mite,
Movanic Deva, Nereid, Redcap, Soul Eater and Thanadaemon - as though no
identity existed for them. The reuse and publication tests read the same single
catalog, so they encoded the omission rather than detecting it. Deriving
coverage means a catalog change moves the counts and the pinned tests fail.

| Unit identity | Creatures |
|---|---|
| Project-owned (`ExpandedSummoningCatalog`) | 67 |
| Retained native wrapper | 11 |
| None yet | 67 |
| **Represented today** | **78** (77 published somewhere, 1 registered but hidden) |

| Family placement | Summon Monster | Nature's Ally |
|---|---|---|
| Published | 72 | 60 |
| Registered | 1 | 1 |
| Planned | 47 | 49 |
| NotOffered | 25 | 35 |

Unit identity, family placement, future target and acceptance are tracked
separately. **Frost Giant is the regression case**: its unit exists and is
Published at Summon Monster VIII through a retained wrapper, while its Nature's
Ally VII placement is still Planned. It must never be classified as a
nonexistent unit, nor its new family placement as already shipped.

All 78 existing identities are reused in place at unchanged tiers, so **67** new
creature identities remain outstanding. **No GUID was allocated, no creature was
published, and every existing wrapper keeps its exact native unit GUID.**

## 4. What changed in the menus

Nothing. The visible surface is still exactly 693 choices, asserted by test.

The scalability gate corrected the premise. 1,232 is an aggregate across
eighteen parent spells; no menu ever holds it.

| Menu | Today | Projected | Factor |
|---|---|---|---|
| Worst single Summon Monster parent | 69 | 120 | 1.74x |
| Worst single Nature's Ally parent | 58 | 110 | 1.90x |

An earlier revision reported 124 and 112 by adding the tier-9 native wrappers
to the 120 and 110 plan. Those wrapper creatures - Bogeyman, Frost Giant,
Movanic Deva, Thanadaemon, Nereid and Hamadryad - are creatures of the ideal
roster and were already inside the plan, so the addition counted them twice.
One creature is one option whichever catalog supplies its unit. Every tier and
the 624 + 608 = 1,232 aggregate now derive from the same deduplicated plan.

Ordering is proved through the shipped `SummonDisplayOrderPolicy` rather than
asserted about the manifest's alphabetical enumeration: singles, then 1d3, then
1d4+1, with synthetic unrelated third-party children preserved at the end and
the result stable across repeated ordering.

`SummonVariantMenuLayoutPolicy` already clamps to the canvas-safe rectangle and
scrolls, and its contract is explicitly option-count agnostic. It was therefore
measured, not replaced, against a rubric fixed before measuring:

- R1 never escapes the canvas-safe rectangle;
- R2 overflow always offers scrolling;
- R3 the scroll range spans exactly the overflow, no stranded head or tail;
- R4 growth costs scroll extent, never viewport height;
- R5 projected scale behaves in the same shape as baseline scale.

Result: PASS at 1280x720, 1600x900, 1920x1080 and 3440x1440 across the **full**
option-count grid - 4 viewports x 200 counts = 800 evaluations, asserted by
count so the claim cannot outrun the run. An earlier revision looped only the
first viewport while the evidence claimed the whole grid.

A 154-option **stress sample** probes headroom. It is deliberately larger than
the plan and is not a target roster; it authorises no later-sprint content.

The charter permits one bounded presentation solution when the existing UI is
shown to fail. It did not fail, so none was built - and a bookkeeping error
found in review is not a reason to build one.

### Unresolved performance/UX decisions

Live in-game measurement was **not run**: no open/reopen timing, scrolling
feel, memory behaviour, or screenshots at projected scale. The deterministic
gate is geometry proof and does not substitute for it. This is disclosed, not
waived.

## 5. Pteranodon

No Pteranodon visual changed. Its identity, placements, stats, reach,
alignment templating, AI, and persistence are untouched.

Audit: `docs/EXPANDED-SUMMONING-PTERANODON-NATIVE-AUDIT.md`. Load-bearing
findings:

1. The donor is `406c1e1af5400ac4881e330502ccbd9e` `CR3_GiantEagleStandard`,
   **not** a Roc. "Roc" is the view-policy label the roster displays.
2. That donor is **shared by `eagle`, `dire-bat`, `pteranodon` and `roc`**, so
   every visual change must be instance-local and reversible, and those three
   are the sprint's negative controls.
3. `Kingmaker.Visual.CharacterSystem.Skeleton` carries `RaceBoneHierarchyObject`
   and `CloakScaleFudge`, marking it the humanoid Character-doll rig rather than
   a general creature binding seam. Bone-name matching against it is explicitly
   not compatibility evidence.

The guide corroborates the shipped profile: it values the ten-foot reach and
the single main attack everything stacks on, which Large size and `Bite2d6`
already provide. That sharpens acceptance emphasis - the bite must land at the
right moment and point at Large reach - without changing a statistic.

Toolchain confirmed installed: Unity **2018.4.10f1** exactly matching the game
runtime, and Blender **4.5.10 LTS**, with three shipped deterministic bundles
as precedent. All three precedents are static props; a skinned creature mesh
bound to an existing skeleton is new ground for this repository.

No mesh was authored, because the charter forbids authoring against a guessed
skeleton. Instead the autonomous guarded scenario
`observe-summon-pteranodon-view-contracts` was implemented to report the real
donor contract from the running game.

## 6. Runtime evidence and machine restoration

The guarded scenario `observe-summon-pteranodon-view-contracts` was run twice
against the installed game at 0.0.136 through Steam App ID 640820.

| Run | Evidence directory | Result |
|---|---|---|
| 1 | `20260923T1250267753124Z-observe-summon-pteranodon-view-contracts` | FAIL 1/8 - the assertion was wrong, not the game |
| 2 | `20260923T1258015475577Z-observe-summon-pteranodon-view-contracts` | **PASS 8/8** |

Run 1 demanded an Animator on a detached donor prefab. Animation binds at
attach time, so none exists to find; the assertion now records the binding
instead of requiring it. The committed extract is
`reports/expanded-summoning-phase0/sprint2-pteranodon-donor-observation.json`.

Between the runs the harness refused to launch on a dirty working tree, which
is the intended guard; the correction was committed before re-running.

### Machine state

- Kingmaker exited cleanly after both runs; no process was left running.
- The live mod directory was backed up before deployment to
  `runtime-backups/live-mod/20260923T1250204338674Z`, and deployment was
  verified against a manifest at
  `runtime-evidence/deployments/20260923T1250266804870Z/deployment.json`.
- **The installed mod was restored.** The first runs left this mission's
  0.0.136 branch build installed over the pre-run 0.0.117 installation. The
  mission required restoration and did not authorize a permanent installation,
  and a shared version string does not make a branch artifact an accepted
  release, so the deploy-and-leave default was not treated as permission.

  Before restoring, ownership was established rather than assumed: the live
  DLL hash matched this mission's exact build byte for byte, no save had been
  written after the deployment (the newest save predates it), and the only
  files newer than the deployed DLL were `FeatureModules.json` written by this
  mission's own runs. Nothing else had touched the installation, so a blind
  downgrade was not a risk.

  | State | Version | DLL SHA-256 | Files |
  |---|---|---|---|
  | Pre-run backup | 0.0.117 | `FD2FC61C250B1385…` | 136 |
  | After this mission's runs | 0.0.136 | `254295C295EA77FF…` | 238 |
  | After restoration | 0.0.117 | `FD2FC61C250B1385…` | 136 |

  The restored hash equals the pre-run backup hash exactly. The backup is
  retained.

- **Subsequent runs restore themselves.**
  `scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1` fingerprints the live
  tree, snapshots it, runs the scenario, and restores its own snapshot in a
  `finally` - on success, on failure and on interruption - recording
  before/after fingerprints under
  `runtime-evidence/expanded-summoning-restoration`. It refuses to restore over
  a tree that changed underneath it rather than destroying someone else's work.
- No save was read, written, or selected. `KMG_AUTOMATION_BASELINE` was never
  touched, and the scenario requires no save at all.
- Other worktrees, including the rapid-reload and magic-circle checkouts, were
  left at their original commits throughout.

## 7. Environment notes

Each fresh worktree needs two gitignored machine-local inputs that are not in
source control:

- `GamePath.props`, copied per `GamePath.props.example`;
- `artifacts/inspection/bodyguard-native/Assembly-CSharp.il` (+ `.res`),
  required by two Bodyguard IL-contract tests.

Without them the suite reports failures that are environment gaps, not defects.

The repository pins an exact `Case("` count per active release and mirrors it
in every active feature block of `validation/static-validation.json`, so adding
domain tests moves four records together. This mission moved them 1702 -> 1717.
No test was weakened or removed.

`.gitattributes` mandates LF for `.cs`, `.csproj`, `.json`, `.py` and `.md`.
Source-scanning tests assert exact multi-line tokens containing LF, so any tool
that rewrites those files must preserve LF endings.
