# Expanded Summoning Phase 2 implementation report

Status: Sprint 9 in progress. The Dire Bat sense, original Bat/Eagle visuals,
Bat icon and preserved-choice publication are source/build-qualified with
guarded casts, player path and live inventory PASS. Open-floor Eagle/Bat travel
and save/load/expiry are also guarded-runtime qualified. Doorway/contact,
combat-mode and module-disabled gates remain. No
Phase 2 creature is yet accepted or ready for owner review.

The accepted baseline is `master` at `2ce70e4` (0.0.140), which advanced after
the owner packet was written. The baseline passes 1,918 domain tests, the
repository wrapper, exact-reference clean Release build, and strict standalone
package validation. Its guarded native-donor observer passed with verified
restoration; exact evidence and hashes are in the autonomous state.

Sprint 9 begins with two existing creature identities. Eagle is published as a
Small three-attack flyer on the Giant Eagle donor. Dire Bat is registered as a
Large one-bite flyer with 14 generated placements hidden; its blindsense and
bat visual are missing. Both share the donor with Pteranodon and Roc, so any
visual work must attach to a single view and preserve those controls. The
existing native 60-foot blindsight fact adds stronger senses and immunity than
Dire Bat requires. The new `KMG.Summoning.Natural.DireBat.Blindsense` feature
(`5dcc039bc9674208a51e4babcd8a30ee`) uses only the imprecise native sense
component at 40 feet. Bat placements remain hidden. The guarded disposable
summon test now checks the live unit and donor-sharing bird controls.

The implementation checkpoint passes repository validation, 1,919 domain
tests, exact-reference clean Release compilation and strict package validation.
Its local package and DLL hashes are recorded in the autonomous state; the
guarded game scenario passed on exact candidate `b32c00f0`: 177/177 casts,
`definition=True;bat=2/2;birds=13/13`, exact unit cleanup and live-install
restoration. The state file carries the evidence ID and package/DLL hashes.
This is not a Sprint 9 pass.

The next checkpoint adds an original Bat mesh and albedo, generated on the
measured flying rig and attached instance-locally through the Pteranodon
visual seam. The Pteranodon regenerated mesh is byte-for-byte unchanged.
Repository validation, 1,920/1,920 domain tests, clean Release compilation,
and the strict 253-file package pass. The guarded Steam run
`20260927T0412311065438Z-disposable-expanded-summoning` passes 177 casts,
2/2 Bat visual attachments, 4/4 Eagle/Roc donor controls and the existing
Pteranodon visual checks; restoration is verified by record
`20260927T0416020406571Z-disposable-expanded-summoning.json`. This is
attachment and isolation evidence. Eagle art, Bat icon/publication, movement,
impact, RTWP/turn-based and save/load qualification remain open.

The Eagle now has an original feathered skinned mesh and painted albedo on
the same instance-local flying rig seam. The 255-file package, repository
validation, 1,921 domain tests and clean Release build pass. Guarded cast
evidence `20260927T0443251559393Z` verifies Eagle 2/2, Bat 2/2 and Roc
donor isolation 2/2; visual-contract evidence
`20260927T0446552603378Z` verifies 81/81 selection/navigation, locomotion,
attack animation and hit/death paths. Lifecycle retry
`20260927T0510593955723Z` passed with verified restoration. The player-path
game result passed but exit exceeded the lease window; the exact candidate
tree was manually restored with the guarded script after game exit. A clean
player-path retry, Bat icon/publication, and Sprint 9-specific motion/contact,
RTWP, turn-based and save/load checks were still required at that checkpoint.

The original Bat icon now ships as the 92nd distinct summon concept; all 91
older export hashes are unchanged. Publication exposes the 14 preserved Bat
placements, raising generated visible roots from 799 to 813 and combined
choices from 828 to 842. The guarded cast run
`20260927T0551063528146Z` passed Bat sense and visual attachment 2/2 each.
The player-path run `20260927T0554347181180Z` passed all 813/813 published
logical roots and 29/29 native wrappers; both runs shared a candidate package
and restored the live installation in record `20260927T0608158839124Z`.
The first inventory audit `20260927T0612429996748Z` found its old exact-fact
and allowed-reference maps omitted the new Bat-only sense; it returned FAIL
while placement and icon assertions passed. After adding only the exact sense
GUID/name, `20260927T0623527812622Z` passed all 48 assertions, including
813 published placements, menu order/counts, zero missing icons, distinct
creature sprites and zero prohibited references. Restoration record
`20260927T0627357893276Z` verifies the original live tree. Repository
validation, 1,922/1,922 domain tests, clean Release build and strict 256-file
package pass on this audit-corrected candidate. This is a publication
checkpoint, not final Sprint 9 acceptance or owner visual approval.

The published-candidate visual/rules regression batch passed guarded runs
`20260927T0634285980681Z`, `20260927T0637208226656Z` and
`20260927T0640079411062Z`, with exact wrapper restoration
`20260927T0643575276053Z`. Sprint 9's save/load fixture then added one Eagle
and one Dire Bat to the established disposable working-save trio. Guarded
prepare `20260927T0655003623332Z` passed 14/14 assertions, including both
46-bone original views and one exact working-save write. Fresh-load cleanup
`20260927T0659022955480Z` passed 14/14 with those views reattached, then
expired the fixture and wrote the cleaned save once. Final absence
`20260927T0703025930540Z` passed 14/14, Eagle/Bat zero and no save write.
The original live mod tree was restored in record `20260927T0705405909889Z`.
Repository validation, 1,923/1,923 domain tests, clean Release compilation
and strict package validation passed on this fixture checkpoint. The result
proves save/load and expiry safety for the two visuals, not real doorway travel
or attack contact.

The Sprint 9 creature review now gates Eagle/Bat on native move acceptance,
appearance readiness, measured travel toward a reachable same-graph target,
target proximity and nonzero movement-agent velocity. The final harness does
not force a path. Guarded result `20260927T0911579435138Z` passed: Eagle
traveled 7.943 m to within 0.18 m of its target; Dire Bat traveled 6.195 m
to within 1.26 m. Both commands were accepted and start-eligible. The
`IsStarted` flag was not observed in this run, so exact command lifecycle
remains uncertain. Validation, all 1,924 domain tests, clean Release build,
strict package and exact restoration passed. This is an open-floor movement
checkpoint; doorway travel and actual attack contact remain open.

Draft tranche PR #25 tracks Phase 2A. All Sprints 10-21 remain planned. Human visual and
gameplay review has not been performed and remains nonblocking for internal
technical acceptance under the owner mission.
