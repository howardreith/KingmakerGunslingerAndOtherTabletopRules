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

The section below records the audit stage as it was written. The finished
creature - crest, texture coordinates, painting, loader and live acceptance -
is described under "Sprint 2 - the finished creature" further down. Identity,
placements, stats, reach, alignment templating, AI and persistence are
untouched throughout.

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
domain tests moves four records together. This mission moved them 1702 -> 1717, and the continuation below moved
them further to 1733.
No test was weakened or removed.

`.gitattributes` mandates LF for `.cs`, `.csproj`, `.json`, `.py` and `.md`.
Source-scanning tests assert exact multi-line tokens containing LF, so any tool
that rewrites those files must preserve LF endings.


---

# Continuation, 2026-09-23: autonomous completion pass

Everything above stands as written. This section records the work done under
the owner's autonomous-completion order and supersedes nothing without saying
so. Where an earlier claim here turned out to be wrong - the loader's bind-pose
strategy in particular - the correction is stated explicitly rather than the
old text being quietly edited.

Branch `codex/expanded-summoning-phase0-sprints0-2`, draft PR #21. Accepted
baseline `master` @ `35555a8d`, release 0.0.136. Not merged, not released, no
candidate left installed, Sprint 3 not started.

## Acceptance labels

- `OwnerDelegationGranted` - the autonomous-completion order delegates
  intermediate technical, menu/usability and visual acceptance to internal
  review.
- `InternalAcceptance` - recorded per item below.
- `HumanReview: NOT_PERFORMED_NONBLOCKING` - throughout. Nothing in this report
  is `OwnerAccepted` or `HumanVisualReviewPassed`.

## What is complete

### Sprint 0 - baseline freeze and program harness

All live evidence at 0.0.136, each with restoration verified and the live tree
returned to 0.0.117 / 136 files /
`FD2FC61C250B13857D81ACC197A896450F5B242EE392FA7192B01201E908F35F`.

| Item | Result |
|---|---|
| Structural inventory | PASS 38/38 |
| Mechanical | PASS 13/13 |
| Player path | PASS 10/10 |
| Persistence trio | PASS 9/9 each |
| Visual contracts | PASS 13/13 |
| Compatibility matrix | see below |
| Feature boundary | see below |

### The feature-module gate was a tautology

It compared the active flag against the settings the launcher had just written -
both sides came from the same object - so it would have passed unchanged if the
module had published its entire roster with the module off, or nothing with it
on. It now censuses the eighteen native summon parents.

| module | publishedParents | placements | nativeOptions | preservation | unclassified | placementsExact | nativeVariants |
|---|---|---|---|---|---|---|---|
| enabled | 18 | 667 | 26 | 0 | 0 | True | 0 |
| disabled | 0 | 0 | 0 | 0 | 0 | True | 46 |

The cross-run reading is the fact no single run can establish: Expanded
Summoning **substitutes for** the native summon menu rather than adding to it.
All 46 native variants are replaced by 667 project placements plus 26
native-option wrappers, and with the module off the native 46 are untouched.
`placementsExact` is the per-variant check across all 681 generated placements,
true in both directions.

Both runs' overall status is FAIL, for two reasons that are not this mission's
and are not glossed: brown fur cannot pass in a profile without Call of the Wild
in either module state, and three `teleportation-scroll-template-*` assertions
fail identically on master's own lineage at 0.0.132. This branch changes no
teleportation file.

### Compatibility

Every scenario that ran passed, with the mod set confirmed loaded rather than
assumed, and the structural inventory 38/38 under each.

| Profile | mods confirmed loaded |
|---|---|
| `gunslinger-only` | none |
| `gunslinger-call-of-the-wild` | CallOfTheWild 1.14.4c-2.1 |
| `gunslinger-arms-armor` | ArmsArmor |
| `gunslinger-toggle-custom-soundpacks` | ToggleCustomSoundpacks |
| `gunslinger-high-risk-combined` | ArmsArmor + CallOfTheWild + ToggleCustomSoundpacks |

Call of the Wild was the profile worth running - it rewrites summon spells and
shares all eighteen parents - and the surface is unchanged under it.

`gunslinger-high-risk-combined` carried `CONFLICT-OBSERVED` for historically
timing out before readiness. With a 600-second budget it reached readiness and
passed. That is evidence the timeout was a budget problem, not that the older
conflict never existed.

### Sprint 2 - the Pteranodon

The donor is `CR3_GiantEagleStandard`, shared by eagle, dire bat, pteranodon and
roc, so every change is instance-local and those three are the controls.

**The bind pose overturned a conclusion recorded earlier in this mission.** The
capture had recorded live local transforms; those are whatever frame the
animation system is on, and they showed folded wings - 1.638 across against
3.152 long. The bind poses describe a spread pose 8.641 across against 2.937
long, mirrored to 1e-5, disagreeing by 3.954 at the wingtip.

That changed the loader too. It had been computing bind poses at attach time
from the donor's live bones, on the reasoning that both sides of the product
move together. They do, but that makes the mesh render as authored *in whatever
frame the unit is on at attach*, and attach time is arbitrary. The loader now
reuses the donor's own `sharedMesh.bindposes`, matched per bone name.

**Deformation proof**, on the detached never-activated probe:

    bone=L_Feather_3;degrees=25;ownedMoved=2;ownedStill=0;ownedDrifted=0;
    worstDrift=0.00000;controlMoved=0;controlStill=100;
    worstControlMotion=0.00000;largestMotion=0.1446;bakeDisagreement=0.00000

Vertices the rotated bone owns moved and held station in its frame exactly; 100
vertices on the opposite wing did not move; Unity's own `BakeMesh` agrees with
the evaluated skinning to zero. `ownedMoved=2` is a thin population - the
membrane blends across bones and few vertices exceed the 0.8 dominance threshold
- and that is stated rather than rounded up.

### The Unity licence, and why the asset ships as data

The 2018.4.10f1 editor that built every previous AssetBundle in this repository
stopped accepting its licence between 2026-08-21 and 2026-09-23. Same install,
same project, same batch command: success then, and now `BatchMode: Unity has
not been activated with a valid License` / `Missing or bad username and
password`. Only 2018.4.10f1 and a Hub-installed 6000.5.6f1 are present, and a
6000.x bundle will not load in a 2018.4 game.

Rather than stop, the Pteranodon ships as ~80 KB of mesh data the runtime builds
a `Mesh` from, beside a 1024 x 1024 painted albedo the mesh data pins by hash.
It carries strictly less than a bundle would, has no editor dependency, and is
less code on both sides. The bundle builder is retained but is not on the
shipping path. Re-activating the licence needs Unity account credentials;
nothing in this delivery depends on it.

### Sprint 2 - the finished creature

**The crest took seven iterations, and the lesson is that the tip must be the
apex.** Iterations 5 and 6 both peaked directly above the skull and trailed
down behind it, and both read as a wedge sitting on the head - at any length.
The accepted crest is a spike whose upper edge rises in one line from the brow
to a tip 1.05 back and 0.72 up from the `Head` bone, about 34 degrees above the
beak line and about the beak's own length, with a 0.06 half-width at the skull
thinning to 0.012 at the tip so it survives the party camera's high angle. It
was judged from clay, silhouette, textured, unlit and thumbnail renders in
profile, from above, and from both game-camera angles.

**Texture coordinates and painting.** The generator writes a five-region atlas:
wings across the top half, and body, crest, beak and limbs in the four quarters
below. Tubes map their length along u and a belly-to-back fold along v, so the
two flanks share texels, the body has no seam anywhere and countershading is a
plain gradient - the cost is mirrored flanks, invisible on a symmetrical
animal. A deterministic painter under Blender's Python lays down the albedo
from closed-form and seeded-noise functions of the atlas coordinates: leather
wings with actinofibrils running across the chord, a countershaded pelt with a
pale throat and a flushed head, a muted display red on the crest, horn-gradient
beaks, scaled limbs. Two runs give identical bytes; the palette is recorded in
`body-plan.md`.

**Loader and material.** Mesh data schema 2 adds the coordinates to the
payload and names the albedo by bare file name, SHA-256 and header dimensions.
The loader publishes the visual only when the mesh validates *and* the bytes
beside it hash to the recorded value; anything else is a named fallback,
because a mesh without its painting is not the reviewed creature. The view
patch puts the albedo in a private copy of the donor's `PF/StandardDynamic`
material, clears whichever of a probed list of map slots the copy declares,
and records what it did: on the live donor the shader declares _BumpMap of
the probed slots and <none> were cleared. Unity 2018 cannot enumerate a
shader's properties, so the probe list is stated rather than pretended to be
exhaustive.

**The swap rides the donor's own renderer.** The first live isolation check
found every freshly summoned eagle, dire bat and roc with its donor renderer
disabled - the game's `EntityFader` hides a summon until it fades in, and
`UnitFxVisibilityManager` and the occlusion highlighter cache the renderer
list by reference. A renderer added beside the donor's, which is what the
first design did, sits outside all of that: visible through fog, opaque during
the fade, untouched by a hit flash or the death dissolve. The visual now swaps
the mesh, the 46-bone array and the material onto the donor's own
`SkinnedMeshRenderer` component, on that one instance; root bone, bounds,
shadow modes and the enabled state stay whatever the game sets, and the
rollback puts the original references back on the same component.

**Build gap closed.** `Build-Local.ps1` stages the Release tree by explicit
copies and had never copied the Pteranodon files; the earlier mesh copy came
from a prior MSBuild output. It now stages mesh and albedo itself, and the
package count moves 234/236 to 235/237 with the source-contract tokens that
pin it. The batch launcher, too, started a following scenario while the
previous game process was still exiting and recorded ERROR for everything
after the first; it now waits, bounded, as restoration already did.

**Live acceptance.** On the asset commit, each restored and verified:
`20260923T2348000940057Z-observe-summon-pteranodon-view-contracts` 13/13 with `visual:published` and the deformation proof
unchanged; `20260923T2353191705789Z-disposable-expanded-summoning` 14/14, every cast Pteranodon reporting
`visual:attached;bones=46;vertices=682;albedo=1024x1024`; `20260923T2356321457517Z-disposable-expanded-summoning-visual-contracts`
13/13. On `7cc80497`, after the swap and the launcher fixes: `20260924T0112361790237Z-disposable-expanded-summoning-pteranodon-fault-drill` 18/18 PASS -
the first Pteranodon cast with the visual withdrawn came up on the donor's own
mesh and material, the second with a fault injected after the swap was rolled
back to them on the same component in the same frame, and every later cast
attached; `20260924T0115460941403Z-disposable-expanded-summoning` 17/17 PASS -
eagle, dire bat and roc on the shared donor untouched, a 1d4+1 Pteranodon cast
attached on every unit at once, 4 Pteranodon casts in one lifecycle
each attached exactly once and cleaned to the exact snapshot; `20260924T0118557039123Z-disposable-expanded-summoning-visual-contracts`
15/15 PASS - the Pteranodon mesh and material on the donor's own renderer,
its 46 bones all on the view's skeleton with the root bone kept, the shader
carrying the albedo at the catalog view scale, and the same state after the
native locomotion, attack, hit and death paths had run.

## Closeout, 2026-09-24

The three items below were completed after the owner's manual review of the
Pteranodon candidate passed; nothing was waived.

- **Projected menu, live** (`20260924T1647492722669Z-disposable-expanded-summoning-projected-menu`): PASS 3/3 on `02bc5c47`: Summon Monster at 120 entries and Nature's Ally at 110 both render every entry (120 and 110 slots), first, middle and last reachable, the popup inside the safe area (550 x 550 and 550 x 500 canvas units against a 1910 x 1070 safe rectangle, so no scrolling is needed at this size and none is installed), the first entry visible on open, no slot growth across three cycles; warm toggle 119-120 ms and 103-110 ms, the cold first open 3334 ms (slot instantiation, reported, not gated), this host at 425-1500 ms per frame under the harness. The earlier
  fixture installed an action-bar slot by hand and hung the UI rebuild; the
  measurement now selects a party member through the game's own selection
  path and anchors to the group slots its action bar already has. The first
  live attempt found two things the fixture had hidden: the Monster cycles ran
  while the loading screen was still up and every one faulted, and the settle
  frames measured this host's frame pacing (400-1000 ms per frame under the
  harness), not the widget. Criterion 8 is therefore scored on the synchronous
  toggle - native fill plus the layout applied inside it - and the run's first
  open, which instantiates the slot widgets, is reported as the cold open.
- **Compatibility mechanical** (record `20260924T1203525961325Z`): `gunslinger-only`
  `20260924T1208085331010Z-disposable-expanded-summoning` PASS 17/17, `gunslinger-high-risk-combined`
  `20260924T1213545977986Z-disposable-expanded-summoning` PASS 17/17. The timeouts were the working
  save's foreign serialized types; the driver stages a fixture derived from the
  protected baseline under the working name and restores the working save byte
  for byte afterwards.
- **In-game review images**: the persistence stages now render the party
  camera to file on the Pteranodon - idle, moving, attacking - after the load
  and creature fades complete (eight party-camera renders on `00195475` (four per writing stage: idle, twice moving, attacking), each with the creature in frame, the screen lit, the renderer enabled and the material intact (dissolve 0.008 at idle, 0 after); the reloaded creature renders as a pterosaur - brown leather wings spread, red crest, long beak - beside the party at Large scale, moving across the room and biting, with the summoned Wolf and Small Air Elemental in the same frames. Internal review of the images: PASS.). The reloaded creature
  carries the attached visual (`20260924T1628426382345Z-working-save-expanded-summoning-verify-cleanup`).
- **Two defects found by that evidence and repaired** (`b7a0b6da`), each
  bounded to its seam: the swapped material had been cloned in the donor's
  dissolve state and was never adopted by the view's material controller, so
  every game-driven fade and tint passed it by and a clone taken fully
  dissolved stayed invisible - the first review images showed the party and
  no creature while every mechanical observer was satisfied; and the menu's
  scrolling installer measured the popup's preferred size in local units
  against canvas units, so 120 entries at 1280 x 720 engaged a viewport they
  did not need, and that viewport put three layout groups on one object
  where Unity allows one, faulted, and once past that drew a panel four
  times its grid; both halves of that one repair are in `b7a0b6da` and
  `02bc5c47`. The order's visual
  approval is recorded against an unspecified revision; the repaired build
  has internal review only.

Platform: Windows 10 Pro 10.0.19045 only. Linux/Proton was not available and is
not claimed.

## What was not complete before the closeout, and why

### The live projected-menu measurement (C2/C3)

Not obtained, after seven guarded runs. Fully recorded in
`docs/EXPANDED-SUMMONING-PROJECTED-MENU-RUBRIC.md`. The short version: the
shipped layout anchors the popup to the slot a player clicked, and no
`ActionBarGroupSlot` is active in `KMG_AUTOMATION_WORKING`'s hierarchy. The
layout policy is measured exhaustively at domain level instead (four viewports x
200 option counts), the real menu remains covered by the supervised observation,
and no live measurement at 120/110 entries is claimed anywhere.

### Human review of the finished creature

Every claim about the finished creature is machine evidence or the
implementation thread's own look at renders. Nobody has seen it under the
game's lighting. The owner review is checklist section 2; until it is done the
creature is `InternalAcceptance` only, `HumanReview: NOT_PERFORMED_NONBLOCKING`.

### Two compatibility mechanical runs

`disposable-expanded-summoning` under `gunslinger-only` and
`gunslinger-high-risk-combined` remain owed, for the recorded save-load timeout
inside the compatibility transaction.

## Defects found and fixed in this mission's own tooling

1. **A scenario that recorded FAIL could be counted as a pass.** The wrapper only
   moved an outcome away from PASS when evidence was absent, so a FAIL with a
   zero exit code did not increment the failure count.
2. **The unattended matrix scheduled a supervised scenario**, aborting every
   profile before its mechanical scenario.
3. **A batch would not release its own compatibility lock**, because ownership
   compared two stamps taken a second apart as substrings. Restoration failed
   with the test build still live.
4. **A new scenario was absent from the runner's working-save chains**, so it
   launched and sat idle until the harness timed out.

Each now has a test that fails on the defect and was mutation-checked.

## Process incidents

- A Release build run concurrently with a loading game starved it past its
  300-second timeout; the leftover process then blocked restoration and the next
  six matrix steps. Recovered by hand after confirming
  `saveInteractionOccurred=false`; live tree verified back to 0.0.117.
- The live installation ended every session at 0.0.117 / 136 files /
  `FD2FC61C...`, verified after each run.


## Additions after the draft above was written

**The Unity licence was refreshed twice and still refuses.** Both 2018.4.10f1
installs behave identically - the original standalone editor and a fresh
Hub-installed 2018.4.10f1. Each reads the refreshed `Unity_lic.ulf` (both report
a next-check date matching the file's own) and each then reports `BatchMode:
Unity has not been activated with a valid License` followed by `Failed to
activate/update license. Missing or bad username and password.` No editor
version was changed and 6000.5.6f1 was never invoked. Logs are in
`unity-asset-build/pteranodon-verify/`. The suspected cause - that Hub 3.20
grants the seat through its licensing client while 2018.4 reads only the legacy
ULF path - is a hypothesis, not a finding. It blocks nothing: the asset ships as
mesh data.

**The action-bar fixture for the projected menu hung the game.** Installing a
`MechanicActionBarSlotSpontaneusSpell` and marking the UI settings dirty stopped
frame production after the save loaded: no error, no result, the process
resident and unresponsive until closed. Because a frame-budget guard cannot fire
when frames stop, the scenario could not report its own failure and had to be
diagnosed from the absent post-readiness stage. The mutation is disabled and the
code retained with that reason recorded. The live 120/110 measurement therefore
remains **not run**, and the prerequisite is now stated exactly: the disposable
working save presents no anchorable action-bar group slot, and the direct way to
create one destabilises the UI rebuild.

**Both compatibility mechanical runs timed out in save load.** Nine of eleven
assertions passed in each of `gunslinger-only` and
`gunslinger-high-risk-combined`; the two failures are the save-load completion
pair (`callback=False`, load reached step 26, after-load callback never fired).
The same scenario passes 13/13 on this candidate outside a compatibility
transaction, so this is recorded as a harness/environment characteristic of
loading a save inside that transaction, not as a summoning defect and not as a
pass.

**Machine state.** Verified after every run in this pass: live installation
0.0.117, 136 files, DLL
`FD2FC61C250B13857D81ACC197A896450F5B242EE392FA7192B01201E908F35F`, no leftover
process, no compatibility lock, no `Mods.kmg-compat-*` sidecar.
