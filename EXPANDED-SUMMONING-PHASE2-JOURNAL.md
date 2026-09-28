# Expanded Summoning Phase 2 journal

## Sprint 9: flying-animal save/load checkpoint, 2026-09-27

The published-candidate visual-contract, repeated-lifecycle and rules batch
passed guarded Steam runs `20260927T0634285980681Z`,
`20260927T0637208226656Z` and `20260927T0640079411062Z`. The wrapper
restored the 136-file live mod tree exactly in record
`20260927T0643575276053Z`. These checks exercise view callbacks, animation
handles, donor isolation and representative rules; actual doorway travel and
attack impact alignment remain unmeasured.

Added Eagle and Dire Bat to the existing working-save persistence fixture,
with a new exact attached-view/rendered-mesh assertion in prepare, reloaded
cleanup and final absence. Repository validation, 1,923/1,923 domain tests,
clean exact-reference Release and strict 256-file package passed. Precommit
package SHA-256 is
`21c06b3c03c90e4d56ac18f4707e18d1b05b476e1d3e7d527b9d3308eaaa8305`;
DLL SHA-256 is
`9d909f7a2ebb8741f5c6bbfa0abe8168f8b3d42b564df5ae7e34db2220d664c9`.

Guarded `KMG_AUTOMATION_WORKING` prepare
`20260927T0655003623332Z`, fresh-load cleanup
`20260927T0659022955480Z`, and final absence
`20260927T0703025930540Z` each passed 14/14 assertions. Both flying
creatures carried their own private 46-bone mesh/material before the save and
after fresh deserialization. The two writing stages each had exactly one
authorized SaveRoutine; final absence had zero save writes and no Eagle/Bat
fixture. Restoration record `20260927T0705405909889Z` returned the live mod
to `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
This closes the visual save/load and expiry gate, with Sprint 9 motion/contact,
RTWP/turn-based and module-disabled checks still open.

## Sprint 9: preserved Dire Bat publication checkpoint, 2026-09-27

Published the 14 existing Dire Bat logical placements without allocating new
unit or ability identities. Both summon families retain tiers III-IX with one,
1d3 and later 1d4+1 quantities. The published generated total is 813 and the
combined menu total is 842. The focused publication test checks every tier,
quantity and the absence of suppressed placements. The live menu audit now
requires the Dire Bat's own non-null, distinct project sprite.

Repository validation, 1,922 domain tests, exact-reference clean Release and
strict 256-file package passed. Precommit publication candidate package
`05b2558f573575ddbbdb34a87e09a58e677cf45576dc6f302bfd52231c70db92`
and DLL `bdd3ab38fc74896ea51fa1c5918799d5e22c3417325fbdb214b5a994abc8d6dc`
produced guarded Steam PASS runs `20260927T0551063528146Z` (177 casts,
Bat sense/visual 2/2) and `20260927T0554347181180Z` (813/813 native
spellbook logical paths, 29/29 wrapper paths). The batch restoration record
`20260927T0608158839124Z` confirms the prior live tree hash `216A9DC2...5AAF3`.

The first unattended live inventory audit `20260927T0612429996748Z` was FAIL:
its exact natural-fact map reported `DireBatBlindsense` unknown and its allowed
project-reference map counted one prohibited reference. It still passed menu
order, counts, 813 placements and distinct icon sprites. I corrected only the
audit's exact Bat sense GUID/name. The new source-qualified package
`ad2e43bc0354acc25e7af8615000d02e94ed25ef1e80cde83aafba46f366c043`
and DLL `02374e67217b07e5cdd772e07e35bb457d734ccce1c658eb7e7123d55e4e151d`
passed `20260927T0623527812622Z-observe-expanded-summoning-inventory` with
48/48 assertions: exact natural profile, zero prohibited references, 813
published placements, menu reconciliation, order/counts and distinct icons.
Restoration record `20260927T0627357893276Z` confirms the same original tree.
This resolves the inventory audit but does not qualify motion/contact,
RTWP/turn-based, save/load or module-disabled behavior for Sprint 9.

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

## Sprint 9: publication, persistence and open-floor travel, 2026-09-27

The original Bat icon and all 14 preserved Bat placements are published.
Guarded player path passed 813/813 generated roots and 29/29 native wrappers;
inventory passed 48/48 assertions. A working-save prepare/reload/expiry trio
passed 14/14 each with original Eagle and Bat views reattached after load
and removed after native expiry. The state file records exact run IDs and
package hashes for these checkpoints.

The creature review previously labeled captures "moving" without measuring
travel. A focused flight assertion now selects a reachable same-graph point
and records actual unit position and native agent velocity. Early runs
disproved an awake-registration theory. Waiting for the native appearance
buff to clear mattered. A temporary forced-path probe moved the summons but
could not attribute travel to the command, so the final harness removed it.
Guarded result `20260927T0911579435138Z` passed without a forced path: Eagle
moved 7.943 m to within 0.18 m of the target; Dire Bat moved 6.195 m to
within 1.26 m. Both native commands were accepted and start-eligible. The
`IsStarted` flag was not sampled true in this run, and remains diagnostic
output. Repository validation, all 1,924 domain tests, clean Release build,
strict package validation and exact live-tree restoration passed. The state
file records the package hash and restoration ID. This establishes open-floor
travel; doorway, impact, combat modes and module-disabled gates remain open.

## Sprint 9: exact-species combat-mode checkpoint, 2026-09-27

Extended the guarded Quickened summon activation fixture to choose only the
published own-tier Eagle or Dire Bat by request, then require an exact-species
spawn and a native `RuleAttackWithWeapon` on the fixture's exact hostile.
The typed-save launcher, preflight and request serializer were all narrowed
to that one allowlisted parameter. A first Eagle control run returned PASS
while the serializer had dropped the parameter; its on-disk request and Dog
spawn exposed the omission. It was not counted as Eagle evidence.

The corrected working-save runs passed Eagle turn-based (six target weapon
rules), Dire Bat turn-based (two), Dire Bat RTWP (one) and Eagle RTWP (one).
The source, test and exact runtime identifiers, package hashes and live-tree
restoration records are in the state file. The RTWP fixtures also passed
their native appearance/command/no-turn-order controls. This closes a
bounded combat-mode gate for Quickened own-tier casts; doorway traversal,
visual attack-impact alignment and module-disabled behavior remain open.

A Roc donor control was captured in the same corridor. Its native wing also
shows the cyan overlay where it crosses the wall, identifying the overlay as
the game's occlusion presentation rather than a unique Bat material defect.
The Bat's actual behavior at a doorway still needs a dedicated check.

## Sprint 9: disabled publication boundary, 2026-09-27

The broad feature-module-settings compatibility run observed zero Expanded
Summoning parents, placements and native options with the module off, but
returned overall FAIL on unrelated Brown Fur and teleportation-scroll checks.
I added a one-Boolean, read-only summon boundary request and ran it under the
guarded `gunslinger-only` compatibility transaction. The exact result
`20260927T1046174341970Z` passed: zero Expanded Summoning publication on all
eighteen canonical parents, 46 native variants retained, loaded version
`0.0.140`. Transaction `compat-20260927T104207Z-b887ecd1ad31` restored
FeatureModules.json byte-for-byte and the original live mod tree. Repository
validation, 1,926 domain tests, clean Release build and strict package passed.
Save-backed module-off behavior remains untested, along with doorway and
attack-impact alignment.

## Sprint 9: save-backed module-off recovery, 2026-09-27

The first off-load under `gunslinger-only` timed out in native `Player.PostLoad`:
that profile removed Craft Magic Items blueprints referenced by the working
save, before the summoning fixture could run. I replaced that strategy with a
single-scenario setting transition inside the mission's exact-tree snapshot
wrapper. It retains the original installed mod graph, stages only
`expanded-summoning=false`, and restores the full KMG tree afterward. A first
full-mod run loaded and cleaned all 16 summons but failed two visual checks
that still demanded custom assets in the intentionally disabled state. The
native donor views were alive and enabled; I made that the explicit off-state
contract.

The corrected prepare, off-load/cleanup and final-absence sequence passed at
`20260927T1143486900326Z`, `20260927T1151452804617Z`, and
`20260927T1159482067877Z`. The off run passed 14/14, retained the exact saved
Eagle and Bat identities on native 72-bone donor views, reported zero added
publication, released the session hold safely, and wrote the working save
once to clean the fixture. Final enabled-module load found no KMG summons and
wrote nothing. Original live-tree SHA-256 was restored after every run. The
state file carries hashes and restoration IDs. Doorway movement and visual
attack-impact alignment remain open.

## Sprint 9: measured native attack anchors, 2026-09-27

I added read-only weapon-event geometry to the own-tier Quickened fixture and
ran Eagle and Dire Bat separately through the guarded Steam path. Both passed
the exact-hostile combat gate and restored the original live installation.
Eagle's bite jaw and nearest claw-foot bones were about 2 m from the selected
target renderer at their weapon events; Bat's jaw was about 0.44 m away.
The next baked-surface Eagle run `20260927T1240150768739Z` revealed that
the selected renderer was `L_WeaponMarker`, not the hostile body. The geometry
comparison is excluded from contact qualification. The native attack rules
remain valid, and the original installation was restored exactly.
The state file carries both run IDs, package hash and restoration records.
Repository validation, all 1,927 domain cases, clean Release and strict
package passed. I corrected the probe to select a substantial skinned body
renderer before assessing Eagle or Bat visual contact. Doorway traversal also
remains open.

## Sprint 9: hostile-body attack contact, 2026-09-27

I reran the guarded Eagle/Bat own-tier combat fixture after requiring the
hostile's substantial skinned `Character` body renderer. Both runs passed
the native attack rules and exact live-tree restoration. Eagle's authored
beak and talon weighted vertices remained 1.27-1.39 m from the body bounds
at its six weapon events; Bat's beak vertices intersected the bounds at both
bites. This isolates an Eagle visual impact defect. The state file carries
exact run IDs, hashes and restoration records. Repository validation, all
1,927 domain cases, clean Release and strict package passed. I will correct
Eagle's instance-local attack presentation without changing combat reach or
the shared donor, then qualify doorway movement.

## Sprint 9: Eagle contact coordinate calibration, 2026-09-27

I checked the weighted mesh probe against Unity's live renderer bounds. The
first baked Eagle vertex conversion applied its 0.30 view scale again, making
the apparent 1.3 m miss too large; I excluded those distances. The corrected
guarded run `20260927T1326061196128Z` passed six exact-hostile attacks and
restored the original live tree. The displayed bird faced the hostile, but
its beak/head surface stopped 0.739-0.798 m from the body on bites, and its
nearest talon surface stopped 0.864-0.906 m away on claws. That is the
bounded visual-travel defect to fix. All 1,927 domain cases, clean Release
and strict package passed. I will add a bounded instance-local attack lunge,
then rerun the contact and noncombat visual gates.

## Sprint 9: bounded Eagle attack presentation, 2026-09-27

I attached a visual-only lunge to the exact Eagle instance. It translates the
skeleton root at most 0.95 m and returns it after each native attack; no game
entity, movement agent or attack reach changes. Focused tests and all 1,928
domain cases, repository validation, clean Release and strict package passed.
The guarded turn-based run `20260927T1345424022344Z` landed six native
attacks on the exact hostile and measured beak/talon weighted surfaces within
0-0.226 m of its body at impact. The guarded working-save creature review
`20260927T1353291620814Z` passed Eagle and Bat native travel, intact renders
and zero-live-unit cleanup. Both wrappers restored the original 136-file
live mod tree exactly. RTWP result `20260927T1404176347439Z` then passed an
exact-hostile native bite with the head-weighted surface 0.106 m from its
body; the wrapper restored the original live tree. I still need a doorway
route before closing Sprint 9. Human visual approval remains unperformed
and nonblocking.

## Sprint 9: native doorway closeout and internal review, 2026-09-27

The working save's named closed doors separated navmesh areas, so I surveyed
native floor nodes and tried a connected room route. The first endpoint was
too far beyond a blocked boundary: guarded result `20260927T1503558530369Z`
measured 15.57 m of Eagle movement but an 8.688 m final gap, and is excluded.
I narrowed the destination to the adjacent room. Eagle and Dire Bat each
passed separate runs, then the stricter combined fresh-process run
`20260927T1536450257132Z` passed 12/12 assertions. Their native moves
covered 12.357 and 12.360 m across a blocked direct line, finished 0.021
and 0.024 m from destination, and cleaned up with zero survivors. The wrapper
restored the exact pre-run live tree. Repository validation, 1,928 domain
tests, clean Release and strict package passed. I reviewed source safety,
rules/roster fidelity, actual renders and corrected contact measurements,
and restoration evidence. Sprint 9 is internally qualified; owner visual
approval remains pending and nonblocking. Next is Sprint 10 Stirge/Wasp.

## Sprint 10: native flying-vermin intake, 2026-09-27

I extended the read-only guarded donor audit to named Stirge/Wasp/mosquito
units and attach, drain and blood mechanic identities. Repository validation,
all 1,929 domain tests, clean Release and strict package checks passed.
Guarded runtime result `20260927T1601519083199Z` passed and found no named
native flying-vermin unit or attach/blood-drain fact or ability among its
selected blueprints; the sole matching buff was generic ConstitutionDrain.
The wrapper restored the original live mod exactly (`20260927T1603474656325Z`).
The supplied rules materials establish tiers and signature roles but do not
contain exact stat blocks. I am examining existing local mechanics and source
availability before implementing creature profiles or declaring a blocker.

The local audit and source review found no exact Stirge/Wasp numeric rules
and no named native shortcut. I examined the existing vermin/poison builder
and summon grapple lifecycle; they are possible implementation seams but
cannot supply the missing attack, attach, drain and poison values. Because
`AGENTS.md` forbids network access without explicit authorization, I recorded
the smallest source-access decision in the Phase 2 blocker file. No Sprint 10
creature was published and no candidate was left installed.

The owner then authorized network rules lookups for this and future work.
I read Paizo's legacy Pathfinder 1e Stirge and Giant Wasp entries, recorded
their exact signature baselines and URLs in the blocker history, and removed
the source-access stop. This does not itself qualify either creature; next
is implementation and guarded runtime evidence.

I ran a second guarded donor audit (`20260927T1629242113136Z`) to inspect
insect-style rig names and the actual native poison buff fields. No flying
insect unit appeared; the six name matches were Giant Flytrap variants.
The Spider poison buff is a configurable native poison cadence, but its
unmodified Strength/1d6/two-save contract is wrong for Wasp. All 1,929
domain cases, repository validation, clean Release and strict package
checks passed; the wrapper restored the exact live tree
(`20260927T1631336749721Z`). The next implementation path is an original
wasp visual on a bounded flying rig and a Wasp-only poison graph whose
initial and subsequent DC, damage, duration and cure are proven live.

## Sprint 10: Giant Wasp original asset-loader checkpoint, 2026-09-27

I authored an original six-leg/four-wing Wasp mesh and striped translucent
painting, rebuilt the mesh after closing its neck gap, and reviewed top and
side renders. I extended the existing Pteranodon/Eagle/Bat loader and
per-view swap without adding an asset system. The local Blender/FBX products
remain private because they carry the measured donor bind rig; source scripts
and shipped mesh/albedo are committed. Wasp is still unregistered and hidden.

Repository validation, 1,929 domain cases, exact-reference Release and strict
258-file package checks passed. A first guarded audit passed but lacked an
asset-status assertion, so I narrowed the fixture and reran. Guarded result
`20260927T1710239929046Z` passed with
`giant-wasp-original-asset-loader=visual:published`. Restoration record
`20260927T1712231807937Z` returned the exact original 136-file live mod tree,
SHA-256 `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
The pushed checkpoint is `9933c40e354155ca9a66610e30d310c7a2eab96b`.
Next: exact Wasp sting/poison and suppressed identity registration; then Stirge.

## Sprint 10: Wasp registered under suppression, 2026-09-27

I appended the Wasp unit, 24 logical/template ability identities and its
dedicated 1d8 sting without changing earlier GUIDs. SM IV and SNA IV each
have six legal quantity placements. All twelve remain suppressed while
poison, strike cadence and live view are unfinished. The menu remains at
813 published choices; 825 are registered. The icon catalog records the
hidden disposition and the live icon observer now checks published
creatures only. The Wasp has no player-visible icon consumer yet.

Full validation, 1,932 domain tests, clean Release and strict package
passed. The first guarded inventory timed out before its late result and
exposed a stale icon-observer assertion; it was not counted as a pass.
After the focused correction, guarded inventory
`20260927T1804369885102Z` passed 48/48 assertions, including exact
registered identities and menu order. Restoration
`20260927T1808371924124Z` returned the original 136-file live tree at
SHA-256 `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Source commit `9f011014e9f0b574a8c10fff14e75088b4569818` is pushed.
Next: Wasp poison, then live combat and view, then Stirge.

## Sprint 10: Wasp poison mechanical checkpoint, 2026-09-27

I cloned the native Spider poison-on-hit and saved poison-buff lifecycle for
the Wasp's dedicated 1d8 sting, changing the effect to Dexterity 1d2,
Fortitude, six total exposures and a one-save cure. The first guarded run
showed DC 16 rather than the Wasp's DC 18. A passive rule-event component
never fired in this context, so I replaced it with a Wasp-only action that
sets the poison context's DC before the native save and buff application.
The next guarded run observed DC 18. The asset attached on the Wasp's
16-bone rig; I corrected an observer expectation inherited from the bird
rig. A failed-save round advanced the poison tick but produced zero integer
stat damage from a rolled 1 under the working save's 0.8 enemy-damage scale;
the final assertion records that scale and checks the native poison rules,
failed-save continuation, and one-save cure without treating a random tick
as guaranteed positive damage.

Final guarded run `20260927T1912579609072Z` passed 21/21 assertions,
including 179/179 casts, Wasp visual 2/2 and the live sting/poison/cure
sequence. Earlier diagnostic failures are excluded. Restoration record
`20260927T1916362556609Z` returned the original 136-file live tree and
SHA-256 `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,933 full domain tests, clean Release and strict
package validation passed. Wasp remains suppressed pending motion, impact,
immunity, lifecycle, quantity, save/load and icon review; Stirge remains
unimplemented. Continue Sprint 10.

## Sprint 10: suppressed Wasp movement review, 2026-09-27

I extended the guarded working-save review only for the hidden Wasp and
required its native move to reach the connected room through the surveyed
doorway. The final `20260927T1955333075608Z` run passed 12.345 m planar
travel and cleanup. Four camera frames showed the original striped Wasp;
the destination was visually crowded by a bookcase and wall. A diagnostic
fifth frame hid the view's one auxiliary renderer in `try/finally`, but cyan
silhouettes remained in front of the Wasp and at a distant wall. Native
occlusion is plausible; the renderer source was not conclusively identified.
The recorded attack is only an animation handle, not a real strike, so
contact/cadence and clipping remain open. All Wasp placements are still
suppressed. Repository validation, 1,934 domain tests, clean Release and
strict package checks passed; the wrapper restored the original live tree.

## Sprint 10: Wasp private quantity casts, 2026-09-27

I added both quantity modes in both families to the guarded mechanical
cast loop while the Wasp remains hidden from the player menu. The final
`20260927T2015561300256Z` run passed 22/22 assertions and 183/183 native
commands. Four Wasp crowd commands produced legal exact-kind counts, and
all 14 Wasp units had their private original visual. The loop's existing
per-cast snapshot restored after each command; the wrapper then restored
the original 136-file installation exactly. Repository validation,
1,935 domain tests, clean Release and strict package checks passed.
Mind-affecting immunity, real strike cadence/contact, and Stirge remain.

## Sprint 10: Wasp Vermin immunity checkpoint, 2026-09-27

The live Wasp has the native VerminType feature, including native
BuffDescriptorImmunity and SpellImmunityToSpellDescriptor components. A
paired native RuleApplyBuff probe reported Wasp `CanApply=False` and
`Immunity=True`, while the human control reported `CanApply=True` and
`Immunity=False`. Neither target actually received the confusion buff in
this request-local fixture, so the result establishes the rule's immunity
decision only. The cloned Eagle donor's `EagleGiant` species marker remains
an unresolved classification defect; the functional Vermin fact is present.

Guarded result `20260927T2115391949167Z` passed 23/23 assertions and
restoration `20260927T2119178826656Z` verified the original 136-file live
tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Three earlier diagnostic runs are excluded from qualification. Repository
validation, 1,936 domain cases, clean Release and strict package validation
passed. Wasp remains suppressed pending species identity, live strike
cadence/contact, lifecycle and visual clarity; Stirge remains pending.

## Sprint 10: Wasp species identity checkpoint, 2026-09-27

The Eagle visual donor left an `EagleGiant` inspectable unit-type marker on
the Wasp. I appended one owned `BlueprintUnitType` identity and assigned it
only to the Wasp, with a Giant Wasp name, Lore (Nature) knowledge category,
and no borrowed Eagle image. The type image remains unassigned while the
summon is suppressed and needs its own disposition before publication.

The final guarded result `20260927T2152154541742Z` passed 23/23 assertions:
the live type is `KMG_Summoning_Natural_GiantWasp_UnitType`, native
`VerminType` grants `VerminImmunities`, and paired RuleApplyBuff decisions
remain Wasp immune/ineligible versus human nonimmune/eligible. The control
buff did not install in this fixture. A diagnostic run with an additional
direct component-mask assertion failed and is excluded from qualification;
the native granted fact and rule outcome are the relevant evidence.
Restoration `20260927T2155531149844Z` returned the original 136-file live
tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,937/1,937 domain cases, clean Release and strict
package validation passed. Wasp remains hidden pending combat contact,
cadence, lifecycle and art; Stirge remains pending.

## Sprint 10: Wasp native strike cadence, 2026-09-27

I admitted only the hidden Wasp to the existing guarded flight-combat
fixture. It cast the own-tier Summon Monster IV variant in both turn-based
and RTWP sessions, correlated native RuleAttackWithWeapon events to the
exact disposable hostile, and required two Wasp stings per mode. The
turn-based run `20260927T2217134616132Z` passed with two exact strikes;
RTWP `20260927T2220168994981Z` also passed with two after 145 wait frames.
At both turn-based impacts the closest weighted Tail/stinger surface was
0.643–0.654 m from the hostile's rendered body. RTWP samples were 0.743 m
and 2.04 m. Native combat cadence is qualified in this bounded fixture;
visible sting contact is not. No global attack behavior was changed and
the Wasp remains suppressed. The shared guarded batch restored the exact
original 136-file live tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
restoration `20260927T2223192246293Z`. Repository validation, 1,938
domain cases, clean Release and strict package validation passed.

## Sprint 10: Wasp actual-impact visual diagnosis, 2026-09-27

The first guarded image batch `20260927T2235521854887Z` (turn-based) and
`20260927T2238573277718Z` (RTWP) captured two party-camera frames at real
native sting rules in each mode. They were valid, lit renders, but the
doorway wall occluded the Wasp and hostile. I added a request-local overhead
camera render that restores its exact camera pose and render targets before
returning. Final guarded runs `20260927T2251029714353Z` and
`20260927T2254096344270Z` both passed mechanics and produced two overhead
plus two party-camera PNGs per mode. The overhead views show the original
Wasp mesh but its abdomen and stinger extend away from the hostile during
the native attack. The first RTWP frame remains partly dissolved; the
second is intact and displays the misaligned pose clearly. A whole-body
forward lunge alone would not correct the backwards stinger. This is a
visual contact defect, not a native attack-cadence failure. All Wasp
placements remain suppressed. Restoration
`20260927T2257092555149Z-summon-same-turn-activation.json` returned the
original 136-file installation at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,938 domain cases, clean Release and strict package
validation passed. Next: reversible target-directed tail-pose probe.

## Sprint 10: Wasp temporary Tail aim probe, 2026-09-27

I added a bounded, request-local Tail-bone rotation at the first two native
sting events, with baked stinger-tip distance, overhead capture, and exact
rotation restoration. Turn-based `20260927T2310527552766Z` and RTWP
`20260927T2314172123373Z` both passed two exact stings. Baked turn-based
tips entered target bounds; RTWP improved but the second gap remained
1.198 m. The images do not prove the altered pose rendered on that same
frame, and the test pose has not been promoted to gameplay. The Wasp stays
hidden. Restoration `20260927T2317241914821Z` returned the original
136-file installation exactly. Repository validation, 1,938 domain tests,
clean Release and strict package validation passed. Next I will test a
frame-persistent, bounded visual pose and continue the remaining Sprint 10
requirements.

## Sprint 10: Wasp delayed Tail pose diagnosis, 2026-09-27

I held the aimed Tail through two rendered frames in a guarded request-local
component and restored it on completion/teardown. Turn-based
`20260927T2331283868029Z` and RTWP `20260927T2334367843649Z` passed two
native exact-target stings each. The baked tip reached target bounds in both
turn-based samples and the first RTWP sample; second RTWP remained 1.193 m
short. Delayed images show a changed pose, but the doorway clips the Wasp
body and wings, while impact effects obscure contact. No visual or
production animation PASS is claimed. Wasp remains hidden. The guarded
batch restored the exact original 136-file live tree
(`20260927T2337441522077Z`); validation, 1,938 domain cases, clean
Release and strict package checks passed. Next is Stirge attach and blood
drain, then a revised Wasp visual rig/open-space contact fixture.

## Sprint 10: Stirge attachment rules boundary, 2026-09-27

I separated the verified Stirge rules from the existing summon grab path:
Stirge attaches on a touch hit without the extra grab check, drains one
actual Constitution point per attached end turn, and detaches after four
points or prey death. The policy records the printed +8 maintain bonus and
10% disease chance; it does not yet install gameplay. Its domain sequence
tests include hit/miss, duplicate link, four-turn meal, death, escape and
immune prey. Repository validation, 1,939 domain tests, clean Release and
strict package checks passed. Guarded working-save smoke
`20260927T2353348639066Z` passed and exact installation restoration
`20260927T2356206464811Z` passed. Stirge remains unregistered and hidden;
the next step is a dedicated runtime lifecycle and live scenario.

## Sprint 10: hidden Stirge registration, 2026-09-28

I appended a Stirge unit, nine SNA logical placements and a zero-damage
carrier weapon behind publication suppression. The profile is Tiny,
one-HD magical beast with the printed physical scores and airborne speed;
the Eagle donor is temporary rigging. The original 2,490-entry ledger
prefix and published 813-choice menu remain exact. Focused tests pin the
new tail, tiers, profile, carrier and suppression. The generated weapon
audit now includes the hidden carrier. Repository validation, 1,940 domain
tests, clean Release and strict package checks passed.

Guarded working-save smoke `20260928T0029440999424Z` passed. The first
inventory run `20260928T0037477029671Z` was a diagnostic failure: it
found an existing Wasp poison fact missing from the exact safety-audit
allowlist. After an exact-name exception, repeat inventory
`20260928T0052069434655Z` passed 48/48 assertions, including 83 units,
834 executable roots, 813 visible placements and zero prohibited
references. All runs restored the original installed mod bytes; final
restoration is `20260928T0055527257092Z`. Stirge mechanics and final
visual identity remain unqualified; the next work is touch attack and
attachment, not publication.

## Sprint 10: native Stirge touch routing, 2026-09-28

I replaced the hidden Stirge carrier's bite donor with the exact native
held-touch weapon from Shocking Grasp delivery, gated on `AttackType.Touch`.
The final live inventory `20260928T0125544188038Z` passed 49/49 and
observed `type=Touch;dice=0;primary=True`; the preceding run
`20260928T0113159269094Z` passed the same assertion. The final source
also passed repository validation, 1,940 domain tests, clean Release and
strict package checks. The wrapper restored the original installation
exactly (`20260928T0129418890710Z`). The result does not yet prove a
real attack hit, absence of HP damage, attachment, blood drain or cleanup.

## Sprint 10: direct native Stirge touch hit, 2026-09-28

I added a guarded combat assertion using the own-tier Stirge summon and an
armored disposable hostile. Its native primary attack hit at touch AC 6,
while the ordinary melee control was AC 14, and dealt zero HP damage.
`20260928T0142219533559Z` passed the full disposable scenario. Repository
validation, 1,940 domain tests, clean Release and strict package checks
passed. Restoration `20260928T0146022240699Z` returned the exact original
installed tree. Attachment and drain remain the next mechanical work.

## Sprint 10: hidden Stirge attach and first drain, 2026-09-28

I added two append-only Stirge buff identities and a dedicated native
attachment lifecycle. Its zero-HP touch hit establishes the native holder
and target parts directly, without the grab maneuver used by other summons.
The holder loses Dexterity to AC; each attached round requests one
Constitution damage and advances the four-point meal by actual damage.
The guarded fixture `20260928T0232376506044Z` passed the touch, link,
first drain and explicit cleanup assertions. It observed actual Constitution
damage 0 to 1, meal 1 and the link still active. The earlier attach-only
run `20260928T0220384662621Z` also passed. Both restored the installed
tree exactly; final restoration was `20260928T0236160392850Z`. Repository
validation, 1,940 domain tests, clean Release and strict package checks
passed. Next are full meal detachment, escapes, interruption cleanup and
disease, followed by Stirge and Wasp visual publication gates.

## Sprint 10: four-point Stirge meal, 2026-09-28

I extended the disposable combat fixture through all four blood-drain ticks.
`20260928T0247164550495Z` passed: each tick caused one actual Constitution
damage; the link remained for points one through three; the fourth point
automatically removed holder and target parts, both buffs and the lost-Dexterity
condition. Repository validation, 1,940 domain tests, clean Release and strict
package checks passed. The wrapper restored the original installation exactly
(`20260928T0250550544603Z`). Next are interrupted holds and disease, then
visual and icon qualification. Stirge remains hidden.

## Sprint 10: Stirge break-free and area transition, 2026-09-28

I reattached Stirge after the four-point meal and ran the victim's native
`TryBreakFree` rule. It succeeded; removing the target part as the native
controller does, then ticking the holder, cleared the source without another
drain. A second reattachment was released by the existing area-leave sweep
with exactly one target swept. `20260928T0304163413890Z` passed both cases,
all prior Stirge assertions and exact installation restoration
(`20260928T0307556510922Z`). Repository validation, 1,940 domain tests,
clean Release and strict package checks passed. Controller scheduling,
death/dismissal/expiry, disease, persistence and visual gates remain open.
