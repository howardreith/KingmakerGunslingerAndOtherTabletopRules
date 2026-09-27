# Expanded Summoning Phase 2 implementation report

Status: Sprint 9 in progress. The Dire Bat sense is source/build-qualified and
guarded disposable-cast PASS; visual work and publication remain pending. No
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

Draft tranche PR #25 tracks Phase 2A. All Sprints 10-21 remain planned. Human visual and
gameplay review has not been performed and remains nonblocking for internal
technical acceptance under the owner mission.
