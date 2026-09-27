# Expanded Summoning Phase 2 journal

## Sprint 9: Dire Bat icon checkpoint, 2026-09-27

Added an original 1254x1254 Dire Bat source painting and exported one 128x128
RGBA creature-choice icon. The 92-row provenance manifest maps its unit and
28 generated ability/template symbols to that one creature concept. All 91
earlier production hashes remain byte-identical. The icon catalog now includes
Bat while the 14 registered placements remain hidden pending publication.
The icon authoring catalog records the new consumer disposition and pending
owner visual approval.

Repository and icon-catalog validation passed; all 1,921 domain cases passed;
clean exact-reference Release build and strict 256-file package passed. Local
precommit package SHA-256 is
`4b6629632342fd5b3b64e14108634bfaa6f9719787d5c36154e90b14955b6dab`;
DLL SHA-256 is
`6628733c4381cf85fe39bf49afe1f759154dad71652e8380efd194d04ad095f8`.
The guarded Steam `disposable-expanded-summoning` run
`20260927T0532168732741Z-disposable-expanded-summoning` returned PASS on
0.0.140: 177 casts, Bat/Eagle attachment 2/2 each, Bat sense 2/2, and donor
isolation 2/2. The wrapper restored the live mod tree to its pre-run
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`
hash in record
`20260927T0535489979402Z-disposable-expanded-summoning.json`. This checks
runtime loading but does not prove the hidden Bat icon appears in the UI;
that requires the later publication/player-path scenario.

## Intake: 2026-09-26/27

The owner authorized Sprints 9-21 in three stacked tranches. The packet named
`2943a02d` as expected master. A remote read showed `master` had advanced to
the merged Favored Class PR #24 at `2ce70e4`; Phase 2A was fast-forwarded to
that accepted commit before tracked edits. The worktree is isolated from the
Phase 1 and unrelated worktrees.

Copied the two documented, ignored local build prerequisites into the new
worktree: `GamePath.props` and the Bodyguard IL inspection artifact. The clean
master build passed 1,918 domain cases, repository validation, exact-reference
Release compilation, and strict package validation. The guarded native-donor
observer passed and restored the live installation exactly; see the state file
for hashes and evidence IDs.

Sprint 9 reconciliation found Eagle and Dire Bat already registered on the
Giant Eagle donor. Dire Bat's identity is preserved but its 14 placements are
hidden. The native 60-foot blindsight fact grants more than the bat's required
blindsense, so it cannot be reused unchanged. The Pteranodon custom mesh and
view ownership path is the accepted implementation seam to inspect next.

An initial sandboxed observer call stopped during backup creation before
deployment because the runtime backup root was outside the workspace write
root. A guarded retry with filesystem access passed. No Kingmaker process or
runtime lock remained after either call; the successful run's restoration
record proves the pre-run live tree was restored.

## Sprint 9: Dire Bat sense checkpoint, 2026-09-27

Inspected the native sense audit and the Nereid live test of the native
`Blindsense` component. Added a new Dire Bat-only feature instead of cloning
the native `Blindsight` feature, which grants blindness immunity and a wider
precise range. Preserved all existing Bat unit and placement GUIDs, kept its
14 placements hidden, and added one feature GUID. The focused domain test
failed before source implementation, then passed. The guarded disposable
summon scenario now checks the feature on live bats and its absence on the
three donor-sharing birds; that scenario is pending the clean candidate run.

Repository validation and all 1,919 domain tests passed. Exact-reference
clean Release compilation and strict standalone package validation passed;
the state file records the hashes. The 0.0.140 Favored Class, Mostly Human,
Bodyguard, Better Vendors, and Phase 1 ledger tests were updated to preserve
their accepted blocks while allowing exactly this appended Phase 2 identity.
The icon catalog registry hash was advanced for the single append. A generated
original Bat icon remains unintegrated under ignored local artifacts.

The clean-tree candidate `b32c00f0` was pushed and run through the guarded
Steam disposable summoning scenario. The game reported PASS for 177/177 casts
and the new Dire Bat sense assertion (`2/2` Bat units, `13/13` bird controls).
The wrapper restored the same 136-file live tree hash it snapshotted. Exact
runtime IDs and hashes are in the state file. The sense checkpoint is closed;
Sprint 9 remains open for models, icon, publication, and full player paths.

## Sprint 9: Dire Bat original visual checkpoint, 2026-09-27

Extended the existing Pteranodon mesh pipeline with an original scalloped
Bat mesh and procedural painting. The accepted Pteranodon output remained
byte-for-byte identical in a regeneration control. The Bat source uses the
private measured rig; only original geometry, weights and painted albedo ship.
The loader validates both files, and the view patch swaps a private mesh and
material on one summoned Bat's donor renderer. Eagle/Roc remain controls.

A focused asset test failed before the files were staged and then passed.
Repository validation, 1,920 domain tests, clean Release compilation and the
253-file package passed. The guarded Steam disposable scenario passed 177
casts, Bat visual attachment 2/2, Bat sense 2/2, Eagle/Roc donor isolation
4/4, and Pteranodon visual regression. The wrapper restored the exact
136-file live tree hash. Evidence IDs and hashes are in the state file.
Bat is still hidden; icon, Eagle, motion/impact, player path and persistence
remain Sprint 9 work.

## Sprint 9: Eagle original visual checkpoint, 2026-09-27

Built a feathered Eagle and procedural albedo on the same measured flying rig,
retaining the accepted Small scale and three-attack mechanics. The new source
and shipped mesh/albedo add no donor transforms. A focused test failed before
the mesh was staged, then passed. Repository validation, 1,921 domain tests,
exact-reference Release and 255-file package all passed. The first guarded
cast test found a test-accounting error: a later mechanical check summoned one
more Eagle, but the Pteranodon lifecycle count covered only the roster loop.
Narrowed the count to that loop; the rerun passed Eagle/Bat attachments and
Roc/Pteranodon controls.

The visual-contract scenario passed 81/81 movement, selection, attack and
hit/death checks, including Eagle-versus-Medium bounds. The lifecycle result
first arrived after a short launcher timeout; retry at 600 seconds passed and
restored the baseline tree. Player-path's game result passed, but its very
long shutdown outlasted the guarded lease and prevented wrapper restoration.
After the game exited naturally, verified the exact deployed hash and used
the guarded restore tool; the 136-file baseline hash was recovered. Player
path needs a clean rerun after Bat icon/publication. Sprint 9 is still open.
