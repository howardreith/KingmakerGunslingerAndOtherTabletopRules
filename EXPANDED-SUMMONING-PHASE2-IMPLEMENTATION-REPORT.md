# Expanded Summoning Phase 2 implementation report

Status: Sprint 9 internally technically qualified; Sprint 10 intake under
way. Eagle and Dire Bat passed guarded casts, player path, live inventory,
open-floor and room-opening movement, exact-hostile native combat in RTWP
and turn-based mode, visual impact contact, save/load/expiry, module-disabled
safety, and installation restoration. Their original bird/bat models, Bat
icon and bounded Bat blindsense are published on the Phase 2A feature branch.
Human visual approval remains pending and nonblocking. Giant Wasp is
registered under suppression with its poison and native sting cadence
qualified; visual contact remains open. Stirge is registered under suppression
with a tested rules boundary and zero-damage carrier, but no live attach
lifecycle or final visual. The Phase 2A draft PR is not ready for review.

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

Sprint 9 doorway qualification used a surveyed connected floor route in the
named disposable working save. An overlong first route
`20260927T1503558530369Z` failed and is excluded. The corrected individual
Eagle/Bat routes passed, and final combined fresh-process result
`20260927T1536450257132Z` passed 12/12 assertions: native `UnitMoveTo`
started and finished through a blocked direct line, Eagle/Bat travelled
12.357/12.360 m to within 0.021/0.024 m of the adjacent-room node, both
renderers remained intact and both cleanup counts were zero. Restoration
record `20260927T1541351948729Z` verified the original 136-file tree hash
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
The final source passed repository validation, 1,928 domain tests, clean
Release and strict package validation (SHA-256
`c329103885331df5e9f81346bc33af8a2fd0d473f42a4744b1eb0deedbd0b56b`).
Engineering, rules, visual and evidence/restoration reviews found no open
technical Sprint 9 gate. The visual review used original meshes, corrected
body-contact measurements, intact party-camera renders and Roc/donor
negative controls; human aesthetic approval is still pending and
nonblocking. The room-opening fixture establishes one map route, not a
general guarantee for every Kingmaker doorway.

Sprint 10 opened with a metadata-only native blueprint audit. Source and
focused test `ed5a3901` passed repository validation, 1,929/1,929 domain
tests, clean Release and strict package checks; guarded runtime result
`20260927T1601519083199Z` passed and restored the original live tree
(`20260927T1603474656325Z`). No named native Stirge, Wasp, mosquito,
attach, or blood-drain unit/fact/ability was present in the selected
blueprints. Generic ConstitutionDrain is not an attach implementation.
This establishes the need for a dedicated bounded Stirge lifecycle; it
does not establish engine infeasibility. The supplied charter/workbook/guide
give tiers and roles but no numeric stat blocks, so rules fidelity remains
open pending an authorized source for exact stats and poison progression.

The owner subsequently authorized network lookup. The primary Pathfinder
1e rules baselines are https://legacy.aonprd.com/bestiary/stirge.html and
https://legacy.aonprd.com/bestiary/wasp.html; the earlier source gate is
resolved. A second guarded live blueprint audit
`20260927T1629242113136Z` found no named insect flight rig beyond Giant
Flytrap variants and inspected the native Spider poison buff's component
fields. Its current Strength/1d6/two-save behavior is not Giant Wasp poison;
a scoped reconfiguration or dedicated component is required. The source
passed repository validation, 1,929/1,929 domain tests, clean Release and
strict package checks. Restoration record `20260927T1631336749721Z`
verified the exact pre-run live installation.

The Giant Wasp original asset-loader checkpoint is pushed as `9933c40e`.
Its project-owned 528-vertex, 376-polygon mesh and 1024-pixel painting load
through the established flying renderer parser and per-instance view swap.
The ship payload carries neither native rig transforms nor a donor texture;
editable source scripts and provenance are retained. Top and side local
renders passed internal silhouette review, while party-camera motion and
impact remain untested. Repository validation, 1,929 domain cases, clean
Release and strict 258-file package validation passed. Guarded native audit
`20260927T1710239929046Z` asserted the packaged Wasp asset loader reported
`visual:published`; restoration `20260927T1712231807937Z` verified the
original 136-file live installation. Candidate package SHA-256 is
`89f75a7c4ec22d3f657e42643f242a459ea31f0f64f18f66908fc2041987a75d`;
DLL SHA-256 is
`b4750c30d0e69b809755040ff8acdde0f2dd32cfc511f45025761eb90d3ba4a0`.
This is an asset-loader checkpoint only: Wasp is not a registered or
published summon and no poison or combat qualification is claimed.

The next pushed source checkpoint, `9f011014e9f0b574a8c10fff14e75088b4569818`,
registers the Wasp unit, its dedicated 1d8 sting and all 24 logical/template
ability identities. It preserves the prior ledger and suppresses all twelve
legal placements. Full repository validation, 1,932 domain tests, clean
Release and strict package validation passed. The guarded fresh-process
inventory `20260927T1804369885102Z` passed all 48 assertions, including
82 registered units, 1,290 abilities, exact menu counts and distinct icons
for published creatures. The initial observer run timed out and exposed an
assertion that expected a Wasp icon despite suppression; it was repaired and
is excluded as a qualification pass. The passing run restored the original
136-file installation exactly (`20260927T1808371924124Z`). Poison, live
Wasp cast/combat/view and Stirge remain pending, so Sprint 10 is not complete.

The next Sprint 10 checkpoint adds two append-only identities for the
Wasp's poison feature and venom buff. The native saved poison lifecycle is
cloned and scoped to the dedicated sting, with Dexterity 1d2, six total
exposures and one successful Fortitude save to cure. A Wasp-only on-hit
action sets the poison context to DC 18, including its +2 racial bonus;
the buff retains that context for later saves. The native Spider poison
icon is a temporary hidden-identity disposition only and must be replaced
or expressly reviewed before any Wasp menu publication.

The final guarded live run `20260927T1912579609072Z` passed all 21
assertions. It cast 179/179 registered placements and directly observed a
Wasp sting hit, DC 18, immediate Dexterity damage, a continued poison
after a failed round save, and removal after a successful save. Both Wasp
views carried the original mesh and material on their private 16-bone
renderer. The working save's enemy-damage scale of 0.8 truncated a rolled
1 to zero on the failed round, so the run does not claim positive damage
on every round. Diagnostic failures `20260927T1841288896478Z`,
`20260927T1853108579217Z` and `20260927T1903352617466Z` drove fixes and
are excluded as qualification passes. Restoration
`20260927T1916362556609Z` verified the exact original 136-file live tree.
The final source passed repository validation, 1,933/1,933 domain tests,
clean Release and strict package checks (clean package SHA-256
`5E7AC1E0DE325AD4F531AB09BCDF8DA8A24E843E6263CE27427B51C3308B5A90`).
This is a poison checkpoint, not full Wasp or Sprint 10 qualification.

The next guarded working-save review admitted the suppressed Wasp to the
private creature-review fixture only. The final result
`20260927T1955333075608Z` passed native 12.345 m movement through a connected
doorway, intact camera frames and cleanup. A fifth frame with the sole
auxiliary view renderer temporarily hidden retained cyan silhouettes; the
native occlusion display is a plausible but unproven explanation. The
Wasp's body and wings were visible, but the destination camera was crowded
by a wall and bookcase. Attack frames exercised an animation handle, not a
target strike; live cadence, body contact and collision remain unqualified.
The 136-file installation was restored exactly. Repository validation,
1,934/1,934 domain tests, clean Release and strict package checks passed
(package SHA-256
`5AE609E8C383588C294DE9CFB9CB00113B6C823AA548CD94C748AE9B9309C01A`).
No full Wasp qualification or publication is claimed.

Private Wasp quantity coverage now adds 1d3 and 1d4+1 casts in both SM
and SNA to the existing disposable fixture. The guarded result
`20260927T2015561300256Z` passed 22/22 assertions: 183/183 commands,
four Wasp quantity variants with legal exact-kind counts and per-cast
cleanup, and 14/14 original Wasp visual attachments. The wrapper restored
the original 136-file installation (`20260927T2019330163598Z`). Repository
validation, 1,935/1,935 domain cases, clean Release and strict package
validation passed (SHA-256
`E30157BF6100441F6D0278B14FCB89F28292AF26C7A935BD71DB982581124BB4`).
Wasp publication remains blocked by unqualified immunity, real strike
cadence/contact, lifecycle and visual clarity; Stirge is pending.

The next guarded result `20260927T2115391949167Z` passed 23/23 assertions.
The live Wasp has native VerminType, and a paired native RuleApplyBuff probe
reported Wasp immune/ineligible versus human nonimmune/eligible for a
mind-affecting confusion buff. Neither target received the buff in this
fixture; the claim is limited to the native immunity decision. The cloned
unit still reports donor species marker `EagleGiant`, which needs correction
before publication. Three diagnostic failures are excluded. Exact live-tree
restoration passed (`20260927T2119178826656Z`). Repository validation,
1,936/1,936 domain cases, clean Release and strict package validation passed
(package SHA-256
`CD34E6E571BF39D15F5A848C59E5FA708038DB4FDFE9681CBBAD240200FEFE93`).
Wasp remains suppressed pending species identity, strike cadence/contact,
lifecycle, icon and visual clarity; Stirge is pending.

One owned `BlueprintUnitType` identity now replaces the Eagle donor's
inspectable `EagleGiant` marker on the Wasp only. Its name and Lore (Nature)
category are Wasp-specific; its image remains null while suppressed and
requires an art disposition before publication. The guarded result
`20260927T2152154541742Z` passed 23/23 assertions, including the exact
live species type, native `VerminType` grant of `VerminImmunities`, and the
paired RuleApplyBuff outcome. The direct native component-mask diagnostic
did not match the simple MindAffecting flag, so no component-mask claim is
made. Neither control buff installed in the fixture. The failed diagnostic
`20260927T2140542227252Z` is excluded; the final installation restoration
passed (`20260927T2155531149844Z`). Repository validation, 1,937/1,937
domain tests, clean Release and strict package checks passed. Wasp remains
suppressed pending combat, lifecycle and visual acceptance; Stirge is pending.

The existing guarded flight-combat fixture now accepts only the named,
still-hidden Wasp in addition to its Eagle/Bat controls. The Wasp casts its
own-tier Summon Monster IV variant and must land two native weapon rules on
the exact hostile in each combat mode. Turn-based
`20260927T2217134616132Z` and RTWP `20260927T2220168994981Z` both passed.
The measured weighted Tail/stinger surface was 0.643–0.654 m from the
hostile body in turn-based and 0.743/2.04 m away in RTWP. Mechanical cadence
passes; visual contact does not. The shared batch restored the original
installation exactly (`20260927T2223192246293Z`). Repository validation,
1,938/1,938 domain cases, clean Release and strict package checks passed.
Wasp publication remains closed pending contact, lifecycle and visual review.

Optional real-impact camera captures now supplement the native combat
fixture. The first party-camera set was valid but occluded by the doorway
wall. Final guarded turn-based `20260927T2251029714353Z` and RTWP
`20260927T2254096344270Z` produced two party and two overhead frames each,
with mechanics still passing and one exact installation restoration
(`20260927T2257092555149Z`). The overhead view shows the original Wasp
abdomen and stinger pointing away from the hostile at impact; the first
RTWP frame was still partly dissolved, while the second was intact. This
supports the measured contact defect. A bounded tail-pose correction needs
testing before any Wasp publication. Repository validation, 1,938 domain
cases, clean Release and strict package validation passed.

The request-local Tail aim probe added after those captures is diagnostic
only. Guarded turn-based `20260927T2310527552766Z` and RTWP
`20260927T2314172123373Z` each passed two exact stings. Baked tip gaps
improved to 0/0 m in turn-based and 0.012/1.198 m in RTWP; same-frame
overhead images did not establish a corrected visible pose. The Tail bone
was restored after each sample, and the original installed mod was restored
exactly (`20260927T2317241914821Z`). Repository validation, 1,938 domain
cases, clean Release and strict package validation passed. Visual contact
remains unresolved and Wasp publication remains closed.

The Stirge rules boundary is now implemented as an isolated policy and
covered by a four-turn meal test, including zero actual damage on immune
prey. It has no unit or runtime attachment integration and is not a
Stirge qualification. Repository validation, 1,939 domain cases, clean
Release and strict package validation passed. Guarded working-save smoke
`20260927T2353348639066Z` and exact restoration `20260927T2356206464811Z`
passed. The next work is the dedicated attach/escape/end-turn/drain and
cleanup lifecycle, then a hidden Stirge unit and full runtime review.

A two-frame request-local Tail pose diagnostic then ran in guarded
turn-based `20260927T2331283868029Z` and RTWP `20260927T2334367843649Z`.
Both passed native two-sting cadence. The delayed baked tip reached target
bounds in both turn-based samples and the first RTWP sample; the second
RTWP gap remained 1.193 m. The delayed overhead renders show the changed
pose but body/wing doorway clipping and obscured impact contact. It is not
a production animation or visual acceptance. Wasp stays unpublished.
Exact installation restoration `20260927T2337441522077Z`, repository
validation, 1,938 domain cases, clean Release and strict package checks
passed.

The next bounded Sprint 10 checkpoint registers Stirge as a suppressed
SNA I-IX unit with a zero-damage touch carrier. It appends 11 stable
identities after the preserved Wasp block. The live inventory reports
83 units, 834 executable roots, 813 visible parents and zero prohibited
references (48/48 assertions, `20260928T0052069434655Z`). Guarded
working-save smoke also passed. The candidate package passed repository
validation, 1,940 domain tests, clean Release and strict package checks;
the installation was restored exactly. The touch-AC and attach/drain
lifecycle, original Stirge visual and icon remain unimplemented, and all
Stirge choices stay hidden.

The hidden Stirge primary weapon now clones the native held-touch weapon
from Shocking Grasp delivery. The builder requires `AttackType.Touch`;
live inventory on the final candidate observed `type=Touch;dice=0;primary=True`
and passed all 49 assertions (`20260928T0125544188038Z`). Repository
validation, 1,940 domain cases, clean Release and strict package validation
passed, with exact installation restoration. This is attack-type routing,
not a direct hit or attach/drain qualification. Publication remains closed.

The next guarded disposable combat run (`20260928T0142219533559Z`) summoned
the hidden own-tier Stirge and resolved its primary attack with the native
rulebook. It hit touch AC 6 against ordinary melee AC 14 and did no HP damage
(0 to 0). Repository validation, 1,940 domain cases, clean Release and strict
package checks passed; the wrapper restored the original installation
exactly (`20260928T0146022240699Z`). This qualifies direct touch delivery,
not attachment, drain or publication.

The hidden Stirge now owns two more stable buff identities: a touch-hit
attachment trait and a hold state. It uses reciprocal native grapple parts
without an extra grab roll, removes its Dexterity bonus to AC while attached,
and tracks actual Constitution loss toward four points. The guarded
disposable run `20260928T0232376506044Z` passed attachment, an exact first
Constitution drain (0 to 1) with the link retained, and explicit cleanup.
Repository validation, 1,940 domain cases, clean Release and strict package
checks passed; exact installation restoration was recorded at
`20260928T0236160392850Z`. Four-point detach, independent escape/death/
dismissal/transition cleanup, disease, persistence and visuals remain open.

The four-point meal then passed guarded combat `20260928T0247164550495Z`:
each of four ticks dealt exactly one actual Constitution damage, the native
link persisted through the third, and the fourth automatically removed both
parts, both buffs and the holder's lost-Dexterity state. Repository validation,
1,940 domain cases, clean Release and strict package checks passed; exact
installation restoration is `20260928T0250550544603Z`. Interruption paths,
disease, persistence and visual publication remain open.

The subsequent interruption run `20260928T0304163413890Z` passed native
victim `TryBreakFree` followed by controller-equivalent target-part removal,
with no further drain and clean holder release. A separate reattachment was
released by the area-leave safeguard with exactly one target swept and no
residual parts or buffs. Repository validation, 1,940 domain cases, clean
Release and strict package checks passed; restoration was exact
(`20260928T0307556510922Z`). The fixture does not prove the controller's
actual tick scheduling. Death, dismissal, expiry, disease and visual gates
remain open.

The attached-summon teardown run `20260928T0332419433992Z` passed once the
fixture advanced Kingmaker's queued entity destruction: the summon was
destroyed, the victim was free, and no extra HP or Constitution damage was
dealt. The earlier immediate check `20260928T0320014996249Z` failed before
the destruction queue advanced and is excluded. Repository validation,
1,940 domain cases, clean Release and strict package checks passed; exact
restoration is `20260928T0336376497673Z`. Actual timer expiry, prey death,
disease, persistence and visuals remain open.
