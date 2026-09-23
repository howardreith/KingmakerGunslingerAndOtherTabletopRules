# Expanded Summoning Phase 0 journal (charter Sprints 0-2)

Mission: implement Phase 0 of the Kingmaker Expanded Summoning Implementation
Charter v1.0 under the owner's explicit batching exception - one worktree, one
branch, one draft PR, with separately reviewable Sprint 0, 1 and 2 checkpoints.
Sprint 3 remains not started.

## Intake

The charter's historical baseline is release 0.0.114 at
`6874dc15a27ded132456dbdd480f47c794543a05`. That commit is an ancestor of the
accepted default branch but is far behind it, so it was recorded and superseded
rather than reset to. The accepted baseline used here is `master` at
`35555a8d8b4bd8df5a6868a60d3ca1fbb9cd366b`, release 0.0.136, which already
contains both the icon-art overhaul and the rapid-reload proficiency gate.

`origin/HEAD` in the local checkout still pointed at
`codex/fifth-playtest-visual-native-feat-repair`, but `git remote show origin`
reports `master` as the real default branch. The stale local symbolic ref was
left alone as an unrelated pre-existing condition rather than repaired inside
this mission.

Local `master` was eleven commits behind and checked out in another session's
worktree, so this mission branched from `origin/master` directly and left every
other worktree untouched.

Two of the three mandated intake documents were absent at the start. The
charter was present; `Kingmaker_Ideal_Summoning_Roster.xlsx` and the summoning
guide were not anywhere in the lab tree, the repository, or the owner's
document folders. Rather than block all three sprints on a missing file, the
roster was rebuilt from charter Appendix A and B with that provenance recorded,
because those appendices carry the controlling fields and reconcile to every
published figure. The owner supplied both documents mid-mission and the
reconciliation is recorded below.

## Sprint 0 - Baseline Freeze and Program Harness

No production behaviour changed. `ExpandedSummoningBaselineInventory` is a
read-only census over the frozen catalogs, emitting deterministic JSON so two
observations can be compared by hash.

The observed surface reproduced the charter's historical figures exactly at
0.0.136: 67 creatures, 66/57 family rosters, 681 registered logical placements
less 14 suppressed, 26 native wrappers, 693 visible choices.

One correction was made to the census while writing it. Listing every creature
with a non-empty view policy as a proxy counted self-named entries such as
`boar<Boar`, which would have overstated the outstanding art work by more than
threefold. Only a view policy naming a *different* creature is a borrowed body.
The real figure is 20, and Sprint 2 addresses exactly one of them.

### Findings classified

- **Setup.** A fresh worktree has no `GamePath.props`; it is gitignored
  machine-local configuration. Copying it per `GamePath.props.example` is the
  documented setup, not a repository change.
- **Unavailable validation.** Two Bodyguard IL-contract tests failed in a fresh
  worktree because `artifacts/inspection/bodyguard-native/Assembly-CSharp.il` is
  gitignored build output that only the primary checkout held. Restoring the
  machine-local file returned the suite to green. Not a code defect, and not
  caused by this mission.
- **Pre-existing coupling.** The version-aware validator pins an exact
  `Case("` count for the active release, and every active feature block in
  `validation/static-validation.json` mirrors it, so adding domain tests forces
  four records to move together. This follows the established feature-commit
  convention. No test was weakened or removed to satisfy it.

## Sprint 1 - Roster Manifest v2 and Menu Scalability Gate

`ExpandedSummoningIdealRosterCatalog` holds all 145 creatures as an inert
planning manifest. It allocates no GUID and publishes nothing; two tests hold
that line, one behavioural and one structural.

The manifest reproduces 145 / 50 / 95 / 120 / 110 / 1,232 with zero delta, and
all 67 shipped creatures are reused in place at unchanged tiers, leaving 78 new
identities outstanding. The only key that needed an alias was `dire-tiger`,
which the charter writes as "Dire Tiger (Smilodon)".

The scalability work corrected the premise that mattered most. The charter's
1,232 is an aggregate across eighteen parent spells; no menu ever holds it. The
worst single menu today is 69 choices and the projected worst is 124 - a factor
of 1.8, not 18.

`SummonVariantMenuLayoutPolicy` already clamps to the canvas-safe rectangle and
scrolls, and its own contract is explicitly option-count agnostic. So the
existing UI was measured rather than replaced, against a rubric fixed before
measuring. It passed at four resolutions, at every option count from 1 to 200,
and at a 154-option stretch probe. The charter permits one bounded presentation
solution when the existing UI is shown to fail; it did not fail, so none was
built.

Live in-game menu measurement was not run and is not claimed.

## Reconciliation against the supplied sources

Once the owner supplied the workbook, the derived manifest was compared against
its deduplicated Shared Backlog sheet. The result was equivalence on every
compared field: the same 145 creatures with none present on only one side, and
zero mismatches in either family tier, design priority, effort, or asset
package. The workbook independently yields 120 and 110 base entries and
624 / 608 / 1,232 placements. The five variant elemental families and the 28
exclusions agree. No manifest change was required.

The guide corroborates the Pteranodon profile rather than altering it: it
values the ten-foot reach and the way everything stacks on one main attack, and
Large size with `Bite2d6` already carries both. That sharpened Sprint 2's
acceptance emphasis without touching a statistic.

## Sprint 2 - Skinned-Creature Pipeline and Pteranodon Vertical Slice

The audit is recorded in
`docs/EXPANDED-SUMMONING-PTERANODON-NATIVE-AUDIT.md`. Its three load-bearing
findings:

1. The donor is `CR3_GiantEagleStandard`, not a Roc. "Roc" is the view-policy
   label the roster displays, which is why the proxy reads that way.
2. That donor is shared by `eagle`, `dire-bat`, `pteranodon` and `roc`. Every
   visual change must therefore be instance-local and reversible, and those
   three become the sprint's negative controls.
3. `Kingmaker.Visual.CharacterSystem.Skeleton` is the humanoid Character-doll
   rig, not a general creature binding seam. Recording that now prevents a
   later sprint mistaking bone-name matching against it for compatibility
   evidence.

The toolchain is exact and installed: Unity 2018.4.10f1 matching the game
runtime, plus Blender 4.5.10 LTS, with three shipped deterministic bundles as
precedent. All three precedents are static props, so a skinned creature mesh
bound to an existing skeleton is genuinely new ground here.

Because the charter forbids treating a Roc-to-pterosaur adaptation as proven,
no mesh was authored against a guessed skeleton. Instead an autonomous guarded
observation scenario, `observe-summon-pteranodon-view-contracts`, was
implemented to report the real donor contract from the running game: whether
`CharacterAvatar` is absent, the skinned renderer set with its root bone, bone
count and bind poses, the animator and its clips, the collider and bounds
surfaces, and the exact bone names an original mesh would have to bind to. The
observation instantiates the donor prefab inactive and far from play, measures
it, and destroys it in a `finally` block; it mutates no blueprint, unit,
inventory or save.


## 2026-09-23 - autonomous completion pass

Sprint 0's remaining gate was the compatibility and feature-boundary matrix. The
feature-module gate turned out to be a tautology: it compared the active flag
against the settings object the launcher had just written, so it would have
passed unchanged had the module published its whole roster with the module off.
It now censuses the eighteen native summon parents, and running it in both
directions established something no single run could - the module substitutes
for the native summon menu rather than adding to it. All 46 native variants
across those parents are replaced by 667 project placements plus 26
native-option wrappers, and with the module off the 46 are untouched.

Five compatibility profiles passed, with the mod set confirmed loaded in each
rather than assumed. Call of the Wild was the one worth running, since it
rewrites summon spells and shares all eighteen parents; the surface is unchanged
under it. `gunslinger-high-risk-combined`, historically `CONFLICT-OBSERVED` for
timing out before readiness, reached readiness with a 600-second budget and
passed - evidence that the historical timeout was a budget problem, not that the
old conflict never existed.

The Pteranodon work turned on a measurement that contradicted an earlier
conclusion in this journal. The rig capture had been recording live local
transforms, which are whatever frame the animation system is on; they showed
folded wings. The bind poses show a spread pose disagreeing by 3.954 units at
the wingtip. That invalidated the loader design too: computing bind poses at
attach time makes the mesh render as authored in whatever frame the unit happens
to be on, and attach time is arbitrary. The loader now reuses the donor's own
bind poses. The deformation proof that followed is exact - vertices the probed
bone owns move and hold station in its frame to zero drift, the opposite wing
does not move, and Unity's own BakeMesh agrees with the evaluated skinning to
zero.

The Unity editor's licence stopped working between 2026-08-21 and today, on the
same install that had built every previous AssetBundle here. Rather than stop,
the asset ships as mesh data the runtime builds a Mesh from; it carries strictly
less than a bundle would and has no editor dependency. Two licence refreshes and
a fresh Hub install of the same pinned 2018.4.10f1 did not change the batchmode
refusal.

Four defects in this mission's own tooling were found and fixed, each with a
test that fails on the defect: a scenario that recorded FAIL could be counted as
a pass; the unattended matrix scheduled a supervised scenario; a batch would not
release its own compatibility lock; and a new scenario was absent from the
runner's working-save chains. A fifth problem - a fixture that hung the game -
was diagnosed and disabled rather than shipped.
