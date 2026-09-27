# Expanded Summoning Phase 2 implementation report

Status: Sprint 9 in progress. The Dire Bat sense, original Bat/Eagle visuals,
Bat icon and preserved-choice publication are source/build-qualified with
guarded casts, player path and live inventory PASS. Open-floor Eagle/Bat travel
and save/load/expiry are also guarded-runtime qualified. Quickened own-tier
combat is qualified in RTWP and turn-based mode for both creatures.
Doorway traversal and visual attack-contact gates remain. No Phase 2 creature is
yet accepted or ready for owner review.

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

The guarded summon activation fixture now selects each flying creature's
published own-tier Quickened spell and correlates its native weapon rules to
the exact hostile. Corrected Eagle and Dire Bat runs passed in turn-based
mode (six and two target attacks) and RTWP (one each). The first nominal
Eagle control had silently serialized only the save name and actually cast
Dog; it was excluded, and the serializer now preserves an exact allowlisted
`flightCreature`. The state file records four corrected result IDs, package
hashes and restoration records. A Roc donor control also shows the same cyan
wall-occlusion silhouette seen on Bat in the corridor. Visual attack-impact
alignment and doorway behavior remain unqualified.

The module-disabled publication boundary now has a narrow guarded Steam
runtime PASS (`20260927T1046174341970Z`): all eighteen native parents have
zero expanded placements and options while retaining 46 native variants.
The compatibility transaction restored the original module settings and live
mod tree. The earlier broad settings result was overall FAIL on unrelated
Brown Fur and teleportation-scroll checks and is not counted as a pass.
At the publication-only checkpoint, loading an existing Eagle/Bat summon
with Expanded Summoning off was still open. Doorway traversal and
attack-impact alignment remain open.

The save-backed module-off gate is now qualified by the guarded prepare,
disabled fresh-load/cleanup, and enabled final-absence results
`20260927T1143486900326Z`, `20260927T1151452804617Z`, and
`20260927T1159482067877Z`. The disabled run passed all 14 assertions:
16 exact saved summons loaded and cleaned, Eagle/Bat/Pteranodon retained
enabled native donor renderers while custom assets stayed off, zero expanded
placements remained published, and cleanup wrote the working save once.
The final load found zero KMG summons and wrote nothing. All live-mod tree
restorations were exact. The first reduced profile timed out on the known
Craft Magic Items save dependency; it is excluded. A later full-mod run
completed mechanics but failed only stale visual expectations; it also is not
counted as a qualified PASS. Repository validation, 1,927 domain cases,
clean Release and strict package passed. Doorway and impact checks remain.

Draft tranche PR #25 tracks Phase 2A. All Sprints 10-21 remain planned. Human visual and
gameplay review has not been performed and remains nonblocking for internal
technical acceptance under the owner mission.

The latest guarded Eagle and Dire Bat combat runs
`20260927T1219166168032Z` and `20260927T1226258599811Z` both passed native
attack correlation and exact live-tree restoration. New read-only geometry
samples initially showed Eagle's jaw and nearest claw-foot rig bones roughly
2 m away, versus Bat's jaw roughly 0.44 m away. The follow-up baked-surface
run `20260927T1240150768739Z` revealed that the target selector measured the
hostile's `L_WeaponMarker`, not its body. All these geometry distances are
excluded from visual alignment qualification; the native attack correlation
remains valid. Body-renderer contact and doorway traversal remain Sprint 9 gates.
The candidate passed repository validation, 1,927/1,927 domain tests, clean
Release and strict package validation (package SHA-256
`666e32b5c8f3b7bffddce3dead39170c46be6269ebfa663f6a3d35eac990d2e9`).

Corrected guarded body-renderer runs `20260927T1252224741767Z` (Eagle) and
`20260927T1259362389907Z` (Bat) both passed exact-target native combat and
restored the original live tree. The target was the 1,819-bone `Character`
renderer, not a marker. At two Eagle bites and four claws, weighted beak and
talon surface vertices remained 1.27-1.39 m from the body bounds; Bat's
weighted beak intersected those bounds at both bites (0 m). Eagle visual
impact therefore remains a measured Sprint 9 defect. Bat bite contact passed
this bounded fixture. Repository validation, 1,927/1,927 domain tests, clean
Release and strict package passed (SHA-256
`e82b3444df52066691ca9be20ce316d65c92dbfa3f77acd9132cf410acd0c5bc`).

The first baked Eagle distances above applied the 0.30 view scale twice and
are excluded. Guarded calibration `20260927T1314477538739Z` exposed the
implausibly small baked bounds. With the corrected baked-to-world conversion,
guarded Eagle run `20260927T1326061196128Z` passed six native attacks and
measured the actual head/beak surface 0.739-0.798 m from the hostile body at
two bites and nearest talon surface 0.864-0.906 m away at four claws. Facing
dot was 1; the remaining visual travel gap is real. The original installation
was restored exactly. Repository validation, 1,927 domain cases, clean
Release and strict package passed (SHA-256
`d8c66bed8222d7c2fab82cc258a96462c65f029818e3a3f5a1ee524e84d95932`).

The Eagle-only visual skeleton lunge keeps the entity, view root, movement
agent, selection and native attack reach fixed while adding no more than
0.95 m of visual reach during a swing. It returns after impact and on view
disable/destruction. Bat and native donors have no lunge component. Focused
tests and the full 1,928-case domain suite, repository validation, clean
Release and strict package passed (package SHA-256
`2eeb725842c9e3329dce8967b5f22031aab0f1d580cae2e655a7bab0f2796b69`,
DLL SHA-256 `a38afad258f86e4fd59bb0bf9915260f58ee8db210a22ca7a7694a5f6154c4ca`).
Guarded Eagle combat `20260927T1345424022344Z` passed six exact-hostile
native weapon rules; weighted beak and talon surfaces lay within 0-0.226 m
of the hostile body at impact. Guarded working-save creature review
`20260927T1353291620814Z` passed Eagle/Bat native travel, intact renders and
zero surviving reviewed summons after dismissal. Both runtime wrappers
restored the exact pre-run live tree; records `20260927T1348486221199Z` and
`20260927T1358060993442Z`. Guarded RTWP result
`20260927T1404176347439Z` passed an exact-hostile native bite with the
head-weighted surface 0.106 m from the hostile body; restoration record
`20260927T1407214071366Z` returned the original live tree. Actual doorway
traversal remains open. Technical visual evidence does not constitute owner approval;
HumanReview: NOT_PERFORMED_NONBLOCKING.
