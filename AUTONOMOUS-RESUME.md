# Autonomous Gunslinger resume handoff

## Active Expanded Summoning mission (2026-08-11)

- Branch: `codex/expanded-summoning`; selected baseline `origin/master` at
  `2894d9fcce250708e354894ffd8e1be9c7493b9b`, containing required ancestor
  `e4d560f8dd2909518614e3a20e77ba4d70dadeb8`; release baseline 0.0.77.
- Durable contract/state: `planning/EXPANDED-SUMMONING-MISSION.md` and
  `EXPANDED-SUMMONING-STATE.json`.
- Completed: Shield Other range repair; frozen 67-creature/681-placement logical
  catalog; guarded final-live parent/donor/action-graph inventory; additive merge
  and rollback policies; sanitizer contract; schema-3 fourth-module state with
  all 16 logical combinations; 1,132 Expanded Summoning identities and exact
  aggregate registry count 1,386; all 681 abilities and additive base-parent
  placements; HD-banded celestial/fiendish templates, spawn-local alignment,
  and bounded smite; all 55 selected donor graphs.
- Current immutable mechanical source is
  `b0deb04ff9b387b375202c5304a6741c9549ef0a`. The complete domain suite is
  1,006/1,006 PASS; clean Release and strict package validation PASS.
- Native elementals and mephits plus the reconstructed Lantern Archon are
  structurally qualified by fresh Steam run
  `20260811T2138366091237Z-observe-expanded-summoning-inventory`: all 19
  assertions PASS, including exact Lantern mechanics, zero sanitizer failures,
  and zero native action contamination. No save was accessed.
- Exact native invisibility, incorporeal, energy-drain, tail, grab, domination,
  and vampiric-touch graphs are captured by guarded PASS run
  `20260811T2156134219365Z-observe-expanded-summoning-inventory`. Stock energy
  drain was rejected because it can become permanent.
- Invisible Stalker and Shadow Demon are reconstructed and exact-structure
  qualified by guarded PASS `20260811T2207541420526Z`.
- Salamander and Succubus are reconstructed and exact-structure qualified by
  guarded PASS `20260811T2238575798728Z`: all 24 assertions passed, including
  67 units, 1,047 abilities, registry 1,386, all 681 placements, exact special
  mechanics, and zero sanitizer/isolation failures. Exact next action is the
  bounded Bebelith and Pixie group.
- Active blocker: none. SSH publication is working. GitHub CLI authentication is
  invalid and will be rechecked after local qualification for draft-PR creation.


## Shield Other casting/UI regression repair (2026-08-11)

- Repository root: `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`.
- Branch: `codex/shield-other-spell`.
- Current commit: the commit containing this final handoff (resolve with
  `git rev-parse HEAD`). Runtime-qualified implementation commit:
  `9af3b9193c6d9be4ec267e211beeb2ab14a137d2`.
- Current hypothesis is proven by the user's retained `output_log.txt`: owned
  Shield Other assigned `MaterialComponent = null`, while Kingmaker/CotW
  availability code unconditionally dereferences `MaterialComponent.Item`.
  The repeated spontaneous-slot update exceptions caused the inert cast icon
  and spellbook/sidebar corruption.
- Repair: non-null empty material data; distinct packaged 128x128 project icon;
  obsolete focus text removed; live disposable scenario now requires native
  `AbilityData` availability and a startable `UnitUseAbility` command.
- Last completed command: eight-state
  `.\scripts\Invoke-FeatureModuleRuntimeMatrix.ps1` on the exact published
  package.
- Last verified result: 981/981 deterministic, exact-reference Release/package,
  CotW, standalone, highest-risk, and all eight module states PASS. Native
  availability, no material requirement, and `UnitUseAbility.CanStart` are true;
  raw CotW log has zero matching failures. Package/DLL SHA-256 are
  `8ca10ff0ffa543f2befdd2d9e36a176443d756994773dd95ee329a9e84b4f4ca` /
  `28b0b6a555feff7e0f77aa858e66d1f052bceb29c940c18b8ad9893fbfc09e4e`.
- Exact next executable action: no autonomous engineering gate remains. The
  documented human-only visual check may be performed on a disposable Oracle
  save without changing source.
- Remaining completion gates: none automated. Final audit before this handoff
  proved a clean feature branch, local/origin equality, zero Kingmaker
  processes, zero merge commits, and zero final-log material failures. Visual
  rest/spellbook/sidebar confirmation remains explicitly human-only.
- Real blocker: none.

## Shield Other mission — final evidence publication (2026-08-10)

- Current phase: Phase 11 final evidence commit/publication and completion audit.
  Branch `codex/shield-other-spell`; frozen base
  `7ba84439caa1fc92b8c8148ce95ea79fd59bdc57`.
- Last completed command/check: second consecutive highest-risk combined
  `disposable-shield-other` PASS in transaction
  `compat-20260810T165726Z-dbba50e40dcd`, with `restorationVerified: true`.
- Exact current commit: release-source
  `80d468ee22835d6c258041cd63f2cd288ef0401e`; final curated evidence is
  intentionally uncommitted.
- Exact next action: run strict validation on the final package without a
  rebuild, commit/push evidence through the approved helper, verify clean tree
  and exact local/origin equality, then close the mission.
- Remaining gates: final package no-rebuild validator, final evidence commit,
  approved push, remote equality, clean tree, no-process audit, and
  requirement-by-requirement completion audit.
- Active blocker: none. Matrix, standalone x2, CotW x2, highest-risk x2,
  persistence, deterministic, Release, package, restoration, and protected-save
  gates all pass.

## Active Shield Other mission — 0.0.77 release qualification (2026-08-10)

- Current phase: Phases 10–11, final release commit and exact-commit runtime
  repetition. Branch is `codex/shield-other-spell`; frozen base is
  `7ba84439caa1fc92b8c8148ce95ea79fd59bdc57`.
- Last completed command/check: version-aware validation, 981/981 deterministic
  tests, clean exact-reference Release build, SoundBank validation, and strict
  standalone package validation PASS for 0.0.77.
- Exact current commit is `46509db2d1e685eb4cdbc5b4e4a4338701e1ddf3`;
  intentional release/evidence changes are uncommitted until the release-source
  checkpoint.
- Exact next action: finish the evidence diff, commit and publish it through the
  approved helper, verify origin equality, then run the all-eight feature matrix
  and consecutive standalone, CotW, and highest-risk combined gates on the exact
  clean commit.
- Remaining gates: release-source commit/push; final matrix; standalone x2;
  CotW x2; highest-risk combined x2; restoration audit; final evidence
  commit/push; clean tree and remote equality.
- Active blocker: none. Persistence PASS directories are
  `20260810T1610288094375Z-working-save-shield-other-prepare` and
  `20260810T1613120391758Z-working-save-shield-other-verify-cleanup`; protected
  baseline SHA-256 remains `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`.

## Active Shield Other mission — real save/load persistence (2026-08-10)

- Current phase: Phase 9, guarded two-stage `KMG_AUTOMATION_WORKING`
  save/load persistence qualification. Branch is `codex/shield-other-spell`;
  frozen base is `7ba84439caa1fc92b8c8148ce95ea79fd59bdc57`.
- Last completed command/check: after observing native worker-thread
  `SaveStashedArea`, focused persistence contracts 13/13, runtime
  preflight 86/86, complete deterministic suite 981/981, exact-reference
  Release compile, and strict 0.0.76 checkpoint package validation all pass.
- Exact current commit before this final native-clone guard checkpoint is
  `47af0dafecf7d3284b61827700a8bdd71cf0bdf4`; the numbered-file guard
  are intentionally uncommitted until this evidence update is committed.
- Exact next action: commit and publish the numbered-file guard, rerun
  `working-save-shield-other-prepare` for structured PASS, then fresh-launch
  `working-save-shield-other-verify-cleanup` for structured PASS and final clean
  working-save proof.
  run `working-save-shield-other-prepare` followed by a fresh-launch
  `working-save-shield-other-verify-cleanup`, both naming only
  `KMG_AUTOMATION_WORKING` and exiting after the exact save callback.
- Remaining gates: prove fresh-load caster/subject/CL context and 3 HP -> 1/2
  damage, prove cleanup save, audit baseline/working-save evidence, advance all
  release pins to 0.0.77, final docs/hashes/package, final repeated runtime
  profiles, clean tree, and local/remote SHA equality.
- Active blocker: none. `KMG_AUTOMATION_BASELINE` remains protected and the new
  save sentinel authorizes exactly one `SaveRoutine` call only when its
  `SaveInfo` argument is the captured working descriptor by object reference.

## 2026-08-10 Acadamae playtest repair

- Branch `codex/feature-modules-acadamae-graduate`; 0.0.76 mode, permanent-fatigue, and project Cord icon repairs are implemented and targeted runtime-qualified.
- Deterministic total is 970; active identities are 252 (ledger 253).
- Standalone, four module combinations, exact CotW/Arms & Armor/Toggle, qualified combined, consecutive high-risk combined, and two working-save smokes pass with transaction restoration.
- Exact next action: commit/publish final docs, rebuild and run final-commit repetition gates, deploy through `Deploy-Local.ps1`, verify installed hashes/settings preservation, finalize clean remote proof. Do not merge.

## Current resume — Feature Modules, Acadamae Graduate, and Cord (2026-08-09)

- Branch `codex/feature-modules-acadamae-graduate`; base `7a99ce5ac6d6976212310f997bd39ddfe4a57935`; latest published pre-versioning checkpoint `08443098e73efc80a9a3e82db530c0b68513077c`.
- Settings/identity/publication architecture, Acadamae, Cord, capital vendor, four standalone module configurations, consecutive standalone integration, and exact targeted optional profiles are implemented and runtime-proven. Deterministic total is 967; registered identities are 250.
- Transactional 0.0.75 / `0.0.75-feature-modules-acadamae-graduate` pins and release documentation are in progress in the working tree.
- Exact next action: finish 0.0.75 documentation/profile validation, run full clean build/package, commit/publish, then execute final consecutive standalone and high-risk runtime gates, eligible ON/ON working-save smoke, restoration audit, final hashes, clean tree, and remote equality. Do not merge.

## Current resume — Paper mode area-transition/save-load crash repair

- Clean branch/local/remote intake SHA:
  `82de97fdc3e5c00d063b24d06b3d387075430d40` on
  `codex/paper-cartridges-auto-reload`; version remains 0.0.74.
- Prior full qualification is suspended. User reproduced a modal null reference
  in composed `Buff.SpawnParticleEffect_Patch1` across four quicksaves and dungeon
  area/level transitions. Saves are potentially recoverable and must not be edited.
- High-confidence hypothesis pending exact proof: the from-scratch persistent
  Paper mode marker leaves `FxOnStart`/`FxOnRemove` null, so live view reconstruction
  reaches an invalid no-FX lifecycle shape. CotW is visible in composition but is
  not yet classified as causal.
- Exact next action: bounded installed IL/signature/field inspection plus four-way
  runtime blueprint comparison; record rejected theories, then make only the
  minimal unchanged-GUID save-compatible repair and qualify it on a real view-backed
  party unit in standalone, CotW, qualified combined, and reachable combined-CotW.

## Current resume — Paper Cartridges Phase 7 (2026-08-09)

- Branch `codex/paper-cartridges-auto-reload`; baseline
  `759685077da0aed6d7ed1fda2cd43e5ad12d0bdb`; latest published source checkpoint
  `2da2f60948c8e70b964e88e54675e069c042a457`.
- Phases 1–6 mechanics and guarded vertical scenarios pass. Complete deterministic
  total is 954. Registry is 248 active; append-only ledger is 249 identities with
  one reserved. Bokken alone is safely deferred by exact installed graph evidence.
- Phase 6 PASS IDs: enhanced reload
  `20260809T0038463040427Z-disposable-paper-cartridge-reload`, item lifecycle
  `20260809T0042467907112Z-observe-firearm-item-lifecycle-contracts`, and switching
  `20260809T0046598688957Z-disposable-production-firearm-switching`.
- No save write was performed. Do not claim a fresh Paper-token restart; retain one
  narrow human checklist while relying on deterministic Paper round trips and the
  inherited qualified item-token carrier lifecycle.
- Exact next action: commit/push Phase 6 evidence, transactionally advance all
  actual 0.0.73 pins to 0.0.74 / `0.0.74-paper-cartridges-auto-reload`, implement
  the comprehensive guarded scenario, then run final standalone/compatibility,
  two fresh comprehensive PASSes, eligible working-save smokes, hashes, audits,
  reports, clean-tree and remote-equality proof. Do not merge.
- Mission mechanics and autonomous runtime/compatibility qualification are now
  complete at version 0.0.74. Final comprehensive runs are
  `20260809T0223531574928Z` and `20260809T0228043712566Z`; working-save runs are
  `20260809T0230156578884Z` and `20260809T0232431480084Z`. Five compatibility
  transactions restored exactly. Exact next action is the final documentation
  commit, complete audit, approved-helper publication, and remote-equality proof.

## Current resume — Rare Firearms project-owned Seeking authority (2026-08-08)

- Exact branch/checkpoint verified clean and published:
  `codex/rare-firearms-campaign-integration` at
  `69fd9a0b9e5889082f15f0e536eb940e79be138b`; baseline `1c570bd` is an ancestor.
- Native Seeking absence is accepted historical evidence. Do not search for it
  again. The prior stop is resolved by new user authority.
- Register exactly ten new blueprints. New project Seeking symbol/GUID:
  `KMG.Enchantments.Seeking` / `036fc59fd1e24753b98f9d92cdb1e93e`.
- Exact next action after publishing this amendment checkpoint: inspect installed
  Kingmaker 2.1.7b concealment attack IL/signatures, record the narrow exact-item
  attack-scoped seam, then implement the stable manifest identity and focused
  policies/tests. Continue every remaining original mission phase through
  0.0.74, complete runtime/compatibility/package qualification and final remote
  verification; do not stop after Seeking.
- Resumed source checkpoint completed exact IL inspection and implemented the
  fail-closed exact-check `RuleConcealmentCheck.Success` adapter plus one active
  Seeking blueprint. Source gates pass 935/935 and clean Release/package. Exact
  next action after commit/publication is guarded save-free bootstrap/blueprint
  observation on the clean SHA, followed by the remaining nine-blueprint magic
  catalog and Reliable shared threshold service.
- Guarded save-free observer PASS on exact `2ba3866`: evidence
  `20260808T1839549682548Z-observe-vendor-table-contracts`, runtime ID
  `20260808T1839549954807Z-f994425a76d245288d0c0bec7b29e2b6`. Next implement
  Reliable plus all eight items and the shared 0..20 threshold service.

---

## Active rare-firearms mission — 2026-08-08

- Branch `codex/rare-firearms-campaign-integration`; exact baseline
  `1c570bd4211d69c5c29f6af46a870146adb1645b`, version 0.0.73.
- Durable records exist; guarded read-only vendor run
  `20260808T1720275373614Z-observe-vendor-table-contracts` passed.
- Observer source is qualified (validation, 930/930, clean Release/SoundBank/
  strict package). The first live graph-rescan strategy timed out without a
  result and is replaced by a bounded single-pass direct-reference index. Next:
  commit/publish the repair, run `observe-vendor-table-contracts`
  on the clean commit, curate exact enchantment/merchant/loot evidence, and
  publish the completed inventory before production registration.
- The repaired run passed at `20260808T1734599486274Z`; five fixed targets and
  native enhancement/Fey Bane/Thundering contracts are recorded. Current next
  action is to qualify/commit/publish the display-name/vendor-reference observer
  refinement, resolve Seeking and the exact capital merchant, then publish the
  completed inventory before production registration.
- CRITICAL STOP: final component-based run
  `20260808T1744000629586Z-observe-vendor-table-contracts` found no native
  Seeking enchantment, confirming two earlier name/display scans and installed-
  assembly inspection. Await new authority changing the nonoptional property or
  an authoritative asset supplying it. Do not implement a custom substitute or
  begin production registration.
- Continuation audit reopened one narrow evidence gap: generic enchantment
  components may encode concealment via scalar/enum values. A deeper observer is
  qualified and awaiting commit/publish/runtime execution; its result controls
  whether the provisional Seeking stop remains.
- That deeper run passed at `20260808T1748440170104Z` and found no generic
  concealment value. The Seeking hard stop is confirmed; no independent safe
  forensics strategy remains. Await changed authority or an authoritative asset.
- Preserve inherited Dodge/Targeting Torso and CotW classifications. Use only
  the approved helper and verify exact remote SHA after every commit.

---

## Pistolero and Musket Master mission resume point (2026-08-07)

Revision 2 begins from exact merged master
`10b792735db5d685b46749dc08ea819f31fa8052`, version 0.0.72, on dedicated branch
`codex/pistolero-musket-master-archetypes`. Unchanged repository validation,
911/911 domain tests, exact Release/build-output/SoundBank/strict packaging, and
guarded mod-load/class-contract/presentation observers pass. Exact evidence and
hashes are in `PISTOLERO-MUSKET-MASTER-JOURNAL.md`.

Durable mission documents are locally committed at `c962e33`. The exact
approved push helper refused the branch because its external allowlist has not
been updated for this mission. Do not raw-push, edit/bypass the helper, or reuse
the obsolete compatibility branch. Human action required: allowlist
`codex/pistolero-musket-master-archetypes`, rerun the approved helper, and verify
origin points to `c962e33` (or the immediately following blocker-record commit).

After publication, the exact next engineering action is the mandatory
pre-implementation inventory of starting-item, proficiency, training, reload,
range, deed, True Grit, presentation, bootstrap, compatibility, observer, test,
and installed Kingmaker contracts. Do not write archetype features before that
inventory and replacement-symbol annotation are complete.

Publication is now restored: branch/local/origin were verified at `8ade461` and
the approved helper accepts the branch. The mandatory inventory is complete in
`planning/PISTOLERO-MUSKET-MASTER-INVENTORY.md`, including exact installed IL
for archetype starter selection. Next: commit/push that inventory, then implement
canonical handedness and scoped proficiency foundations with focused tests.

## Optional-mod compatibility mission resume point (2026-08-07)

New human authority: exact Call of the Wild 1.14.4c-2.1 reaches character
creation with its added classes visible, but Gunslinger absent. This is
`CONFLICT-CONFIRMED`, superseding timeout-only classification for the
player-facing catalog. Keep it independent from detached Dodge. Exact compiled
IL now proves CotW writes `Main.library.Root` while exact game chargen reads
`Game.Instance.BlueprintRoot`; Gunslinger publishes through
`BlueprintRoot.Instance`. Diagnostic-only lifecycle/root/catalog snapshots are
source-qualified with 911/911 tests and strict packaging. Instrumented run
`20260807T2132214591875Z-9fc724aa137b43b7b92d9907706743f6`
proved all roots identical and case A: Evasive's vanilla-only native component
count assertion rejected CotW-mutated donors and rolled back 146 registrations.
The narrow current-donor ordered-type validation repair is implemented without
a CotW dependency and is published at `b00e7e8`. Fresh CotW load
`20260807T2146571019519Z-mod-load-smoke` and strengthened observer
`20260807T2149121927539Z-a37fb450a1164ec9b664812be3073704` pass with exact
restoration. The observer proves Gunslinger exactly once in the final root and
chargen input while retaining all 46 compiled CotW helper classes. Candidate
package/DLL SHA-256 are `37B4C25A...AC0DBE5` /
`B8799B41...3A69E8B`. Human chargen confirmation remains pending. Next: build
the separate guarded view-backed Dodge qualification on the exact
`KMG_AUTOMATION_WORKING` receiver-bound load path; do not change production
Dodge unless that real player path fails.

Branch `codex/postbase-archetypes-compatibility` starts from clean integrated
`master` commit `d03dfe9eae65f5cd1395df7337f21dfdb4357661`, version `0.0.71`.
Unchanged repository validation, 911/911 deterministic tests, exact-reference
Release build, build-output, SoundBank, package creation, and strict package
validation pass. Baseline package/DLL SHA-256 are
`1815C6A37C935A61223D026E03A8E6D50A0D949066CD41F9D2A17479D9197CC2` /
`F879904D51DDAA0B226375048EF0C7983F44158B8441EC1EC4616C00CB204BEB`.
Durable compatibility mission documents and the read-only inventory/catalog
checkpoint are established at version `0.0.72`. Its full source/build/package
gate passes with package/DLL SHA-256
`43D48259B890F7F600DF6E2FFC1B5D142ED0948FF1A1BD4FE1F0181E9779B006` /
`BD1BD66C690A4689A2125CE4D6CC8ED3CFC36962ADB222A131F1BDA856FA0339`.
Static audit and the eight-profile manifest are source-qualified. Next command
after the external publication policy is resolved:
`.\scripts\compatibility\Test-KingmakerCompatibilityProfile.ps1`, after
implementing the exact profile resolver and transaction framework entirely
against temporary fixtures. Do not touch the real Mods directory before all
transaction safety fixtures pass.
Checkpoint `1062676` is published and verified on the identically named origin
branch. Profile resolution and transaction fixtures pass. The guarded
`observe-optional-mod-compatibility` source is exact-reference qualified with
911/911 tests and strict package validation. Next action after publishing this
observer checkpoint: the compatibility profile wrapper now enters the
transaction, invokes the existing Steam/App ID 640820 harness with the typed
profile ID, and restores in `finally`. Publish/rebuild this runner checkpoint,
then rerun `gunslinger-only` mod-load and observer fresh processes after the
bounded inter-scenario exit-wait repair. The first real mod-load run
`20260807T1912134251961Z-379f7fd088d945fca5a7e663ed6c1262` passed and
transaction `compat-20260807T191144Z-9ce245d1f232` restored exactly; the
observer was not launched because the prior process was still exiting.
The next repaired run reached the observer but failed at its first UMM field
lookup; transaction `compat-20260807T191438Z-3adf77ada3af` restored exactly.
The observer now resolves `modEntries` from the live ModEntry declaring type and
is fully source/build/package qualified. Commit/publish/rebuild, then rerun the
gunslinger-only observer before advancing.
The live-type rerun identified the final exact mismatch: UMM 0.32.4 declares
`modEntries` as `Public, Static, InitOnly`, not private. Binding flags now cover
the proven public field. Transaction `compat-20260807T192017Z-17a2340a5202`
restored exactly. Source-qualify/publish/rebuild and rerun the observer.
Run `20260807T1924016022942Z-fce5fc0272f9417a968fbbb87d3fd868`
then passed every product/UMM assertion and exposed only an overload-collapsing
Harmony diagnostic false positive. Target identities now include parameter
types. Source-qualify/publish/rebuild and rerun gunslinger-only observer.
Gunslinger-only observer now passes twice with exact restoration. Call of the
Wild's first process exceeded the default 120-second result bound and then
exited safely; transaction `compat-20260807T192918Z-c9ce6b83ef83` restored
exactly. The profile wrapper now uses 300-second guarded startup/result bounds.
Commit/publish, then rerun Call of the Wild mod-load plus observer.
Call of the Wild again reached structured 300-second `TIMEOUT`; restoration
verified. Continue independent profiles. Arms & Armor passed every substantive
observer assertion; only expected-ID array order differed from actual UMM order
`ArmsArmor,KingmakerGunslinger`. Exact membership comparison now ignores order
while retaining actual load order evidence. Qualify/publish, then rerun Arms &
Armor observer and continue Toggle Custom Soundpacks.
Arms & Armor now passes its exact observer, visual-rig, and production-switching
matrix; Toggle Custom Soundpacks passes exact load, observer, and Wwise runs.
All associated transactions restored Mods and `KMG_Firearms.bnk` exactly.
Call of the Wild remains `CONFLICT-OBSERVED` at the exact 300-second readiness
timeout. Next command is the guarded `gunslinger-high-risk-combined`
`mod-load-smoke` attempt; restore and classify before attempting the separately
named all-loadable profile. Do not rerun the unchanged individual CotW timeout.
Both combined profile names reproduced the 300-second `request-accepted`
timeout and restored exactly. The wrapper now permits a bounded explicit
timeout. Source-qualify and publish it, then run exactly one Call of the Wild
`mod-load-smoke` with `-RuntimeTimeoutSeconds 600`. If readiness still does not
occur, preserve `CONFLICT-OBSERVED` and do not retry unchanged.
The 600-second attempt also timed out at `request-accepted`; transaction
`compat-20260807T201206Z-e36dbdadd645` restored exactly. Do not retry Call of
the Wild or either current combined profile unchanged. Continue gunslinger-only
class/presentation and working-save qualification, then define the maximum
passing combination from Arms & Armor plus Toggle Custom Soundpacks if the
committed schema permits an evidence-backed extension profile.
The extension profile `gunslinger-qualified-combined` now exists with only
Arms & Armor and Toggle Custom Soundpacks; all nine dry runs, 911/911 tests,
and package gates pass. Commit/publish, then run its load, compatibility
observer, and full high-risk scenario matrix in fresh processes. It is the
maximum passing candidate, not evidence for Call of the Wild.
Its first `mod-load-smoke` passed under transaction
`compat-20260807T202946Z-28927c42d2fd`; observer launch then exposed a remaining
typed ValidateSet in `Invoke-KingmakerRuntimeTest.ps1`, and restoration verified.
The allowlist is repaired. Source-qualify/publish, then rerun the combined
matrix beginning with the observer (load smoke is already evidenced).
The qualified combination passed observer plus all targeted high-risk scenarios
but comprehensive acceptance failed on two fixtures; standalone reproduced
both. The Grit fixture's impossible post-spend expectation is corrected. Full
inner Dodge exceptions are now preserved and the dedicated Dodge scenario is
allowed by the profile wrapper. Source-qualify/publish, run standalone Dodge,
then make only an evidence-backed fixture or Gunslinger repair and rerun
standalone before the combined comprehensive repetition.
Dedicated Dodge run `20260807T2050596684740Z-855d97503f97488086afa4b2c7268038`
still lacked its inner exception because the top-level summary truncated it.
The central summary now preserves the full exception chain. Qualify/publish and
run exactly one more standalone Dodge diagnostic before choosing a repair.
The full exception proves a finished detached command reports `Interrupt`.
The fixture now records the enum and evaluates the actual strict Dodge effects.
Qualify/publish and rerun standalone Dodge once; its effect assertions will
decide whether further repair is required.
The effect-based run proved no timed buff was applied. Preserve
`GUNSLINGER-REPAIR-REQUIRED`; do not weaken the assertions or run working-save
smoke until standalone comprehensive acceptance is repaired. Finalize exact
reports/hashes and publish the blocker checkpoint. No merge occurred.

The user-authoritative accepted native-rig history is: attach-slot Experiment A
still left held Musket and Blunderbuss invisible; the later isolated holster-
patch experiment restored visibility. Minor residual clipping is accepted and
firearm visual calibration is frozen for this mission. The older active summary
below is retained as historical provenance but is superseded where it says
Experiment A passed or Experiment B was unnecessary.

## Superseded firearm native-rig handoff

Branch `codex/firearm-native-weapon-rigs` has published isolated attach-slot
experiment A `6e3aa3782eb6328786b60330ae453fa2d5241f6a`, version `0.0.71`. Human testing
now **passes** experiment A: held Musket and Blunderbuss are visible again after
restoring inherited attach slots. The preceding `6b1f5db` empty-slot candidate
remains a recorded hard failure. The user accepts remaining torso clipping for
now and authorized merge to `master`. The restored human-best anchors remain;
the rejected `-0.020` offset remains absent. Remove only Hidden's empty attach
slots/forced override, preserve inherited attachment values, belt/sheath null,
equipped models, current sheath patch, Pistol, transforms, scales and animations.
Exact package/DLL hashes are
`CE0C03BE2AF4D0BA0BBFF6A975C5733D106716B4BE581F69F44ED46140B2F90D` /
`BC1B4C8B67B8CD68A654DD1334361C61A47733A292A78138DC0239874B8387DC`.
Repository validation, 911/911, exact-reference Release, and strict package
gates pass. Experiment B is unnecessary. Next action: commit/push this human
verdict, verify the master worktree is clean and unchanged at the qualified
source baseline, then perform the explicitly authorized normal merge and push.

Historical candidate context: the rejected `-0.020` micro-offset was replaced by the
last human-best Musket/Blunderbuss anchor values; no speculative rotation was
retained. Two deterministic Unity builds match at
`F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`;
repository validation, 911/911 tests, exact-reference Release, build-output and
strict package validation pass. Explicit long-gun `Hidden` holster policy now
clears belt, sheath, prototype fallback attach slots and exact firearm
`ReattachSheath` output; active held models remain configured. Pistol held
presentation remains frozen. Provisional package/DLL hashes are
`C6E416A87212BA244F3A71EB0FAC78466B8C43413DA8B7FA3B027C8561BC598A` /
`78E93E479D2B58E9466F86A5F4A357C28755AC997766A83427809445EE856132`.
Exact package/DLL hashes are
`FA955857DA4DDE83D43107D57A6CE4B1E41F738A4BB18F30269F4A69F067740D` /
`BAFC115F3839B7D31E6DB9BB5C3D6D97FFB7BCCA97416AD440FA2997B0CD4E74`.
Guarded visual rigs, switching, Targeting Arms, Wwise, Scatter and reload all
PASS. Next action: confirm restored Musket/Blunderbuss held placement and verify
Musket/Blunderbuss/Rifle show nothing on the back. Minor residual clipping is
explicitly accepted for now; do not begin another transform architecture pass.
Pistol is proven bound to Cyril43
`model.dae`, distinct from Revolver's Navy Colt source. Revolver's 53 duplicate
preview objects are removed. Blunderbuss's proven `0.024 m` unit collapse and
Rifle's proven behind-grip offset are corrected. Opaque Standard materials and generated reverse-wound backfaces
replace the failed custom shader. Two cache-cleared exact Unity builds match at
`F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`.
Repository validation, 911/911 tests, exact-reference Release, build-output,
strict package validation, and guarded rig/switching/Wwise/Scatter/projectile/
reload scenarios pass. Exact package/DLL SHA-256 are
`6858AF28C2DDE865BD2575FDEECF6DA11ADACEB0BC6210B1251DEC54239DBC06` /
`2757835E9086B35481D9F5E06B03DC691BB317B351794BE1B0EDC20442568EA4`.
Pistol is frozen after narrow human held-appearance acceptance. Musket,
Blunderbuss, and Rifle now use explicit source Grip/Support/Butt/Muzzle anchors;
semantic lengths are `1.349985`, `0.848`, and `1.549379 m`. Two clean exact
Unity bundles match at
`F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`.
Next action: supervised semantic-anchor checklist across inventory doll and
world idle/fire/recovery/switching states. No weapon is globally HumanAccepted. The
session-isolated per-kind calibration model/UI is source-qualified by repository
validation, 910/910 tests, runtime preflight 86, exact-reference Release build, build-output and strict
package validation. Candidate package/DLL SHA-256 are
`9F905766214BEB2AC23E2519525826B14970FA7CDE32D305BD8D4E9D2452DF2D` /
`479244B41883256831396E60FFCC9CFD06E6F40544AF6E8185D0785831D5000C`.
All five firearms are `AutonomousCandidate`; none is `HumanAccepted`;
no visual acceptance is claimed. Published commit `8f2ba17aeb9da6b2f9ae1786475a5b8d96b69b97`
passed guarded smoke and `disposable-firearm-visual-rigs`; exact evidence is in
the journal. Next command: commit/push long-gun source, then run guarded visual
rig scenario and frozen regressions passed on the scan-retirement commit. Next
Final 0.0.71 qualification passes two consecutive rig runs, Wwise, Targeting
Arms and working-save smoke. Next action: commit/push the final evidence handoff;
remaining implementation gaps are inventory-doll refresh, markers/import
promotion and live rendered-unit parenting diagnostics before human review.
Durable contract: `planning/FIREARM-NATIVE-WEAPON-RIGS-MISSION.md`.
Declarative eight-prefab rig specifications now build deterministically under
exact Unity 2018.4.10f1; two clean staged bundle hashes match at
`88DF971967ECF4879BAA93FE79A734D46ABA2A754AEBD193FAE01AB756DCFD91`.
Models remain disabled pending runtime capability/IK preparation and visual
review. Source qualification passes repository validation, 898/898 tests,
exact-reference Release build, and strict packaging. Current structural package
SHA-256 is
`4BD296BEC867BC6B44496A07826224DEBF9F61EB3F41CA6F2028BABC9ECAC7A3`;
DLL SHA-256 is
`6BBA65B3C22332B6DB8B2F2420E7412195C562B72F45AC7A39A2F4F548A905AB`.
Runtime rig validation/EquipmentOffsets preparation and guarded donor observation
are source-qualified: repository validation, 901/901 tests, runtime preflight 86,
exact-reference Release, and strict packaging pass. Package/DLL hashes are
`569B283BD8B40EF57232C5DB05C9EA37DBA4C08217B57425A15E2BE79A43C36B` /
`EB5AE4153CC0ACD34FEE894F885940CCFB1934FED856E6FB4062F4246923DBCD`.
All production profiles remain `NativeFallback`. Next action after commit/push:
exact-commit mod load and `observe-native-firearm-rig-contracts` passed on
published `8bdb40b`; run IDs/hashes are recorded in the native-rig journal and
forensics report. Exact Heavy Crossbow left-hand target is
`(-0.031,-0.051,0.374)`. Active phase is the development calibration lab,
followed by the Musket loaded-unit proof.

## Sixth-playtest resume point (2026-08-04)

Version 0.0.66 is functionally qualified at `448e0d1`; package SHA-256 begins
`32D6269B`. Automated work is complete. Resume only from concrete findings in
`SIXTH-PLAYTEST-VISUAL-AUDIO-CHECKLIST.md`, or retry the exact policy publisher
after the external workstation network restriction changes.

## Current sixth-playtest state

- Branch `codex/sixth-playtest-btsl-animation-grit-crafting`, version 0.0.66.
- Latest crafting-inclusive comprehensive PASS:
  `20260804T1240496957011Z-disposable-gunslinger-comprehensive-acceptance`.
- Next: commit documentation, run the second comprehensive acceptance and two
  working-save smokes, deterministic package/audits, append hashes, and hand
  off the consolidated human checklist.

## Durable objective

Execute `AUTONOMOUS-GUNSLINGER-MISSION.md` continuously until its complete
definition of done or a listed genuine human-input hard stop.

## Repository state

- Active fourth-playtest mission: branch
  `codex/fourth-playtest-runtime-ux-repair`, starting from qualified `0.0.63`
  commit `6a0117d`. Fresh exact mod-load
  `20260803T2225523236661Z-mod-load-smoke` proved the reported UX failures are
  real integration defects, not stale deployment. Baseline package/DLL hashes
  are recorded in `FOURTH-PLAYTEST-REPAIR-REPORT.md`.
- First repair checkpoint merges the firearm rows alphabetically into all five
  native parametrized feat menus and uses exact `Rifle` naming. Repository
  validation, 878 tests, Release build, strict package validation, and guarded
  save-free runtime run
  `20260803T2230567143233Z-disposable-firearm-dependent-feats` pass. Final live
  UI observation remains required with the coherent `0.0.64` package.
- The native Grit counter repair passes repository validation, 878 tests,
  Release/package validation, and guarded save-free runtime evidence
  `20260803T2240193812834Z-disposable-gunslinger-grit-resource`. The real
  granted ability fact reported count `1->0`, availability `True->False`, and
  no UI-side double spend. Package/DLL hashes are `cefc9fa9...b1a9` /
  `68ee5be2...16f1`; final player-visible observation remains Phase 12 work.
- Next exact action: commit the qualified Grit checkpoint, then audit and repair
  the native reload auto-use/action-bar integration from the fourth-playtest
  evidence without weakening the existing firearm transaction gates.
- RTwP auto-use continuation is now source/runtime-qualified: guarded run
  `20260803T2250479651771Z-disposable-reload-autocast` used the real granted
  ability, loaded exactly once, and retained the original target in the native
  command collection. Package/DLL hashes are `03ccbba8...82fce` /
  `edc45bb7...74a0a`. Commit this coherent checkpoint, then add exact
  weapon-switch/interruption/Wrecked cancellation and turn-based action-state
  coverage before closing Phase 7.
- Follow-up guarded run `20260803T2257100760678Z-disposable-reload-autocast`
  passed weapon-switch and interruption cancellation, Wrecked rejection, and
  the RTwP/turn-based action-availability policy. Package/DLL hashes are
  `af2b9b88...ed93d` / `75792762...dcc27`. Commit this extension, then proceed
  to Phase 8 condition and maintenance player UX; integrated all-kind and
  player-visible right-click checks remain final acceptance work.

- Mission complete. Exact `eda0202` passed final mod load
  `20260802T2020407693601Z` and consecutive fresh-process 32-slice acceptance
  runs `20260802T2017342856278Z` / `20260802T2019030300247Z`. All 854 tests,
  Release build, and strict package validation pass. The complete report is
  `docs/COMPLETE-GUNSLINGER-QUALIFICATION-REPORT.md`; no mandatory work remains.

- Death's Shot is runtime-qualified on exact `612105f` with fresh guarded PASS
  pair `20260802T2009410114711Z` / `20260802T2011048524145Z`. Installed
  `ContextActionKillTarget` IL proved its terminal effect is
  `UnitState.MarkedForDeath`; the production deed now uses that exact terminal
  transition after its native Fortitude failure. Run the expanded 32-slice
  comprehensive acceptance twice, then perform the final definition-of-done
  audit; do not stop at this deed checkpoint.

- Targeting Arms is runtime-qualified on exact `98a2a59`: mod load
  `20260802T1932466249834Z` and fresh PASS pair
  `20260802T1934063083002Z` / `20260802T1935297321491Z` proved one chamber,
  one grit, no damage, exact six-second native main-hand Disarm, and cleanup.
  Implement and qualify Death's Shot next; do not stop at this checkpoint.

- Branch: `codex/complete-gunslinger`
- Audited source HEAD: `6b1e413`. Sprint 93 item-owned battered origin repair
  passed exact mod load `20260802T1440278862789Z` and two fresh guarded
  `gunslinger-starting-items` runs `20260802T1441506873456Z` /
  `20260802T1445126858809Z`. Native 1/1/1 grant, exact owner, bound/ordinary
  values 22/1000, exact rollback, no save write, and version 0.0.60 passed.
- Gunsmith/starting equipment is runtime-qualified. Commit this curated
  evidence, re-audit mandatory coverage, and continue the highest-priority
  independently actionable gap; do not stop at Sprint 93.
- Sprint 93 evidence is committed as `95d6fb3`. The full remaining-row audit
  found no routine source slice. The next narrow dependency is renewed runtime
  permission for the existing save-free `disposable-gunslinger-startling-shot`
  and `disposable-gunslinger-targeting-head` commands. See
  `HUMAN-INPUT-REQUIRED.md`; do not run either until authorized.
- Authorization received. Exact `8609ebd` passed Startling Shot pair
  `20260802T1451324004693Z` / `20260802T1452540172188Z` and Targeting Head pair
  `20260802T1454137455208Z` / `20260802T1455368086818Z`. Both are now
  runtime-qualified. Commit the curated records and continue the remaining-row
  audit; the permission file is resolved and removed.
- Repaired-deed evidence is committed as `8c77e8d`. The next highest-dependency
  blocker is the missing numeric production Blunderbuss cone distance. Exact
  choices and the recommended 15-foot conservative adaptation are recorded in
  `HUMAN-INPUT-REQUIRED.md`. Do not implement a distance until authorized.
- User directed PnP fidelity, resolving pellet mode to a 15-foot cone and
  bullet mode to a 10-foot range increment. Sprint 94 encodes the exact
  distance boundary; qualify/commit it, then continue into native delivery.
- Sprint 94 is committed as `b9a2b6a`. Sprint 95 adds the exact native cone
  target resolver while keeping production locked. Qualify/commit it, then
  continue into the marked volley transaction.
- Sprint 95 is committed as `9e56731`. Sprint 96 implements the request-scoped
  native volley marker and hook integration. Qualify/commit it, then compose
  the complete ability transaction; production remains locked.
- Sprint 96 is committed as `284b3e2`. Sprint 97 composes the atomic native
  Scatter Shot runtime transaction including one discharge, independent -2
  attacks with precision suppression, aggregate misfire, triple burst, and
  item-state rollback. Focused contract, repository validation, 849 tests,
  clean Release build, and strict package pass with package/DLL hashes
  `b4d21431...` / `af48d199...`. Commit it before registering the guarded
  ability/runtime acceptance or unlocking player content.
- Sprint 97 is committed as `c392935`. Sprint 98 registers and grants the
  guarded Scatter Shot ability while retaining the item/vendor lock. Source,
  849 tests, Release build, and strict package pass with package/DLL hashes
  `f8c31f26...` / `3278619b...`. Commit it, then implement and run the save-free
  guarded scatter acceptance before unlocking the Blunderbuss.
- Sprint 98 is committed as `1a9d1fb`. Sprint 99 adds the guarded save-free
  `disposable-gunslinger-scatter-shot` transaction scenario; preflight 84,
  repository validation, 849 tests, Release build, and strict package pass.
  Commit it, then run exact mod load plus two fresh-process PASS runs before
  changing the production Blunderbuss restriction or vendor publication.
- Exact `07407d4` mod load passed (`20260802T1557206558978Z`); first scatter run
  `20260802T1558563329150Z` failed closed because the native cone predicate is
  an instance method. The repair now invokes it on an inert exact component.
  Source-qualify and commit, then use only one materially different attempt.
- Repaired exact `ac5c520` mod load passed (`20260802T1602457684816Z`), but the
  final allowed scatter run `20260802T1604090773661Z` proved detached fixtures
  are absent from the native live-area query. Stop under the two-attempt rule.
  Production remains locked. `HUMAN-INPUT-REQUIRED.md` requests one renewed,
  save-free attempt using exact reversible live-area fixture registration.
- Standing continuous authorization supersedes every historical attempt-limit
  stop. Mission state is `continue`; `HUMAN-INPUT-REQUIRED.md` is resolved and
  removed. Implement reversible request-local live-area registration with
  guaranteed cleanup and re-run Scatter Shot until qualified or a genuine
  mission hard stop is proven.
- Live-area fixture registration is implemented with per-target flags,
  immortality retention, `finally` unregistration/disposal, exact `+2` count,
  and reference-snapshot restoration. Preflight 84, 849 tests, Release build,
  and strict package pass. Commit and run exact mod load plus Scatter Shot.
- Exact `253e825` mod load passed; runtime proved `LoadedAreaState` is null at
  main menu. Installed `GetTargetsAround` IL enumerates `State.Units` directly.
  The current repair uses owned `Units.All` additions/removals; qualify, commit,
  and retry without changing the production lock.
- Exact `41e8690` proved the complete scatter transaction and cleanup; only the
  registration counter read the external collection instead of `State.Units`.
  Repair uses authoritative pool count plus external snapshot restoration.
- Scatter runtime-qualified on exact `d47fa60`: mod load plus PASS pair
  `20260802T1631497040259Z` / `20260802T1633240163716Z`. Current qualified
  source unlocks/publishes Blunderbuss; 849 tests/build/package pass. Commit and
  run updated vendor observer twice, then continue remaining mandatory rows.
- Published Blunderbuss runtime-qualified on exact `ea1faaa`: mod load plus
  vendor PASS pair `20260802T1639246998795Z` / `20260802T1640487811206Z`.
  Commit curated evidence, re-audit mandatory rows, and continue immediately.
- Current active checkpoint is Sprint 100 native creation commit. The save-free
  observer and rollback contract pass all 849 tests/build/package gates. Commit
  the source checkpoint, run exact mod load and two fresh scenario passes, then
  continue the mandatory matrix without pausing.
- First exact creation run exposed and narrowed a detached-CharGen zero-grant
  contract. The repaired ownership observer no-ops only for zero observed
  pistols and remains fail-closed for multiple pistols; all source gates pass.
  Commit the repair and resume exact runtime qualification.
- Native creation commit is runtime-qualified on exact `75563e8`: mod load
  `20260802T1653068423255Z` and PASS pair `20260802T1654290642019Z` /
  `20260802T1655490400235Z`. Commit curated evidence and continue the remaining
  broad native player replacement callback.
- Sprint 101 source-qualifies expanded metadata-only observation of exact
  installed `Player.RespecCompanion` signatures/call graph. Commit and run it,
  then implement the reversible broad callback fixture from observed evidence.
- Exact handler contract is observed and the reversible broad callback fixture
  passes all source/build/package gates. Commit it, run exact mod load and fresh
  broad-respec qualification, repair from structured evidence as needed, and
  continue the mandatory matrix.
- Broad native player replacement is runtime-qualified on exact `61ed182` with
  mod load `20260802T1727191713556Z` and PASS pair
  `20260802T1728404473198Z` / `20260802T1730049919619Z`. Commit curated
  evidence and continue to the save-backed item lifecycle row.
- Sprint 102 live inventory transfer passes all source/build/package gates.
  Commit it and run exact mod load plus two guarded `gunslinger-starting-items`
  transactions against only `KMG_AUTOMATION_WORKING`, then continue stash/vendor.
- Live inventory/party transfer is qualified on exact `7b26575`: mod load plus
  guarded PASS pair `20260802T1736103455384Z` / `20260802T1737524072305Z`.
  Commit curated evidence and implement shared-stash/vendor lifecycle next.
- Qualified baseline contained: `4f28dcf` runtime implementation and `5c92012`
  documentation.
- Current checkpoint: Sprint 89 carries battered effective condition through
  shared equipped-firearm resolution, native discharge, and reload without
  mutating persisted actual condition. Focused contracts, repository
  validation, 846 tests, clean Release build, and strict packaging pass.
  Exact `0410d21` mod load passed as `20260802T1402557908977Z`; continue into
  misfire and deed propagation. Sprint 84 runtime evidence remains
  `20260802T1312212200554Z`, `20260802T1313522632011Z`, and
  `20260802T1315168298512Z`. Historical Sprint 70 evidence follows.
- Historical checkpoint: Sprint 70 exact mod load and the corrected final guarded
  level-up commit observation passed on `d0b15f6`, proving detached Gunslinger
  level `0->1->2`, native success callback, and unchanged external collections.
  Only one PASS is available because the first of two allowed attempts exposed
  the corrected argument-order defect. Do not run a third. Select the next
  independent incomplete coverage row and continue.
- Current-build integration: exact `d0b15f6` passed canonical working-save
  smoke runs `20260802T1108448693251Z` and `20260802T1110221239004Z` on
  `KMG_AUTOMATION_WORKING`. This is a save/load regression checkpoint, not the
  still-pending comprehensive mechanical acceptance scenario.
- Latest class progression: exact `ddea6cc` passed mod load
  `20260802T1117198249088Z` and save-free level-20 PASS runs
  `20260802T1118455028165Z` / `20260802T1120048623643Z`, proving BAB 20,
  saves `12/12/6`, 29/29 direct facts, and isolation.
- External boundary: `production-firearm-catalog` was rejected because
  feature-specific save-backed scenarios are not explicitly permitted. Do not
  retry or circumvent it; save-free scenarios and canonical working-save smoke
  remain available.
- Latest multiclass evidence: exact `7fbdfae` passed mod load
  `20260802T1126008241032Z` and PASS runs `20260802T1127226945921Z` /
  `20260802T1128499610464Z`, proving Fighter 1/Gunslinger 1, native callback,
  proficiency/grit facts, and expanded external isolation.
- Latest respec evidence: initial `163320d` run exposed starting-inventory
  isolation; repair `2eb0090` passed mod load `20260802T1140330341524Z` and
  final run `20260802T1141509870468Z`, proving source `1/0`, replacement `0/1`,
  callback/facts, exact rollback, and isolation. Do not run a third attempt.
- Latest chassis evidence: exact `e35de17` package/DLL hashes are
  `86c3ad51eb2ad54bdaebc6f77a1a3b592893ee578d14b61d9386bbd04ff797b7` /
  `c92ee42f8662459bc4d588a093ad87d31dff3e1c760bb9f31fd4225e1dade908`.
  Mod load `20260802T1149510654742Z` and fresh save-free runs
  `20260802T1151111616673Z` / `20260802T1152385768115Z` passed d10, native HP
  `0/11/18`, evaluated skill points `4/4` at Intelligence 10, level 2, and
  isolation. Combined with Sprint 72, the class-chassis row is runtime-qualified.
  Commit this curated evidence, then select the next incomplete independent
  integration or final-acceptance item; do not stop at Sprint 75.
- Latest bounded repair: `3a26059` reconciles Startling Shot's native null
  `AddBuff` return against exactly one installed requested-blueprint fact while
  retaining fail-closed atomic rollback. All 831 tests and source/build/package
  gates pass. Do not launch a third Startling Shot attempt without new runtime
  authority; continue to an independent incomplete item.
- Latest observer correction: `4485dba` measures Targeting Head's separately
  dispatched native damage by authoritative target delta. Retained final runtime
  evidence proved `0 -> 5` damage plus hit, grit/chamber, Confusion, and cleanup.
  All source/build/package gates pass. Do not launch a third Targeting Head
  attempt without new authority; continue independently.
- Latest lifecycle evidence: exact `731ff07` package/DLL hashes are
  `d9a7081ea6458ee3e0167a2d5fd60a6ac6623c5fcee04d7f7f5c7d1c270e199c` /
  `746c369f8211216595c68d0a4f848e574b15a5b3b32d75c7c545dc8901754e1c`.
  Mod load `20260802T1209365380684Z` and final run
  `20260802T1210579132106Z` passed reconstruction, removal, new-item isolation,
  duplicate rejection/preservation `2->2`, and no inventory/save mutation.
  Do not run a third attempt; continue independently.
- Latest switching evidence: exact `7ab1a58` package/DLL hashes are
  `414fefdf4536ede54b8f71693d4c198b5f4799b2f581a10f42c2cf7807c0bca0` /
  `624e9cdde3e8df4b019de8fe7f736e3ef0885b084667a2b7479cfd4acdc904ed`.
  Mod load `20260802T1217444779507Z` and PASS runs
  `20260802T1219064706820Z` / `20260802T1220329632177Z` reproduced exact
  identical-item selection, independent loaded/Normal versus empty/Broken state,
  dual-equipped ambiguity rejection, and isolation. Continue independently.
- Version: `0.0.60`.
- User-supplied worktree inputs `AGENTS.md` and
  `AUTONOMOUS-GUNSLINGER-MISSION.md` must be preserved.

## Last runtime evidence

- Production firearm actions exact source `087e11a` passed mod load at
  `20260802T1020144242399Z-mod-load-smoke`. Independent PASS runs
  `20260802T1021336705231Z-observe-gunslinger-presentation` and
  `20260802T1022571370114Z-observe-gunslinger-presentation` observed 20 levels,
  75 visible facts, zero incomplete visible facts, and exact production names
  `Reload Firearm,Overhaul Firearm,Repair Firearm` with no Test Musket
  descriptions. Package/DLL SHA-256 are
  `cf8ff7859a5b5fe9373524af4310c05c66d2ea96714aace83189e17c99d352d0` /
  `b79daa5d57d411a0bdcd190b8f58b88dd54dc6636c37ddcaf339e30535bb0490`.

- Acquisition exact source `c2fd27b` passed mod load at
  `20260802T0931384431105Z-mod-load-smoke`. Independent PASS runs
  `20260802T0932570144693Z-observe-vendor-table-contracts` and
  `20260802T0934221001134Z-observe-vendor-table-contracts` both observed 58
  capital table entries, seven exact Gunslinger entries, correct native-derived
  quantities, zero Blunderbuss entries, and no save/shop interaction.
  Package/DLL SHA-256 are
  `6bbd63bf662197b540684b09cd4a84eebe275eb210ad86804fdc736a6d9f0819` /
  `e31962312cc583eb83cdb5e27645816f90c1acf409360243adfa687654288169`.

- Presentation exact source `adcb030` passed mod load at
  `20260802T0838152303512Z-mod-load-smoke`. Independent PASS runs
  `20260802T0839341612816Z-observe-gunslinger-presentation` and
  `20260802T0840534291165Z-observe-gunslinger-presentation` both observed 20
  levels, 75 visible facts, one excluded hidden fact, zero incomplete facts,
  six UI groups/21 grouped features, and readable class/progression metadata.
  Package/DLL SHA-256 are
  `94e3c83c32600abf3f12a18da55f693a91b7174866735add5aff3cefb7a52d0d` /
  `9989c449b4859c24a59990bb9e46732e38b009e25e9332461f1439e08a827a53`.

- Stunning Shot exact commit `f5dc6bb` passed mod load at
  `20260802T0726286857532Z-mod-load-smoke`. Independent PASS runs
  `20260802T0727464837674Z-disposable-gunslinger-stunning-shot` and
  `20260802T0729102463349Z-disposable-gunslinger-stunning-shot` both observed
  chamber `1->0`, native damage `0->3`, grit `4->2->0->2`, natural-1 and
  natural-20 Fortitude branches, exact six-second Stunned, critical-immunity
  rejection, marker consumption, and detached cleanup. Package/DLL SHA-256 are
  `dd0b46951c3d8c963b5705c1a6ad999b379dbe75f81ee8467cc796cc7d7ef777` /
  `58377a40be8084968bf1e7471875b80d47f1abfe8af41a79c8a1044f78ca045b`.

- Death's Shot observer commit `c1305fc` passed mod load at
  `20260802T0642181684393Z-mod-load-smoke`. Observer runs
  `20260802T0643380649281Z-observe-deaths-shot-native-death` and
  `20260802T0646499769488Z-observe-deaths-shot-native-death` failed closed:
  Destruction is damage-only, while the full catalog exposes three distinct
  Fortitude/kill authorities. See the blocker ledger; continue Stunning Shot.

- Cheat Death exact commit `10a4274` passed mod load at
  `20260802T0626527728449Z-mod-load-smoke`. Independent PASS runs
  `20260802T0629237809884Z-disposable-gunslinger-cheat-death` and
  `20260802T0630482940232Z-disposable-gunslinger-cheat-death` both observed
  maximum HP 137, grit `1->0`, final HP 1, zero-grit control HP -10, level-19
  progression, and cleanup. Package/DLL SHA-256 are `3877860c6d2f955afd991740b4b38ad2cf86bc8d1f3521b204bff87624c7c161` /
  `42f5122584447f195f3a66dbe2fbce5737a7d432acae35049f28a207503064b9`.

- Slinger's Luck exact commit `a67a930` passed mod load at
  `20260802T0603271690563Z-mod-load-smoke`. Independent PASS runs
  `20260802T0604472262192Z-disposable-gunslinger-slingers-luck` and
  `20260802T0606065893610Z-disposable-gunslinger-slingers-luck` both observed
  saving and skill native rolls `17->10`, fixed grit `4->2->1`, both markers
  consumed, other-unit grit `4->4`, and exact cleanup. Package/DLL SHA-256 are
  `a39c776bb026280bb92594a34b8c6e72b790dabbe0cfa5bcb01de217b837aa9b` /
  `219d52d8e45aa0372e0300406a4a3b4e24b97b0c55a72d15ad4231c301746eae`.

- Dead Shot exact mod load passed on `fdd5d7c` with run
  `20260802T0032018715336Z-6864954149b440dc9f10abfac6449d6e`.
  Independent PASS runs
  `20260802T0033299551403Z-5d701f8560b7443b9a4433072e419c69` and
  `20260802T0034468834084Z-e1508a1ec47246ab91a5ba4f863bef43`
  both observed BAB-11 probes `+11,+6,+1`, rolls `19,1,19`, two hits/two
  base-dice packets, no mixed-volley condition change, all-roll misfire
  `Normal -> Broken`, grit `4->3->3`, and exact cleanup. Package/DLL SHA-256
  are `191be3b74ca79c1d0128f0c02c377a9118ea02cfc4dd802030bbd3a0bcd16b4b` /
  `d235c2bcfb74bb25b320f7904a8e72bb9120f01a398d254c0a5dce369bb6a4f4`.

- Gun Training source `72ab329` plus natural-roll fixture correction `76ae9f9`
  passed exact mod load at `20260801T2353104199349Z-mod-load-smoke`.
  Independent post-gate PASS runs
  `20260801T2354325601237Z-disposable-gunslinger-gun-training` and
  `20260801T2355488749660Z-disposable-gunslinger-gun-training` both observed
  levels `5,9,13,17`, four selection occurrences, five choices, untrained versus
  trained damage `0 -> 4`, forced natural 5 changing untrained Broken to
  Wrecked while trained remained Broken, and exact cleanup. Package/DLL
  SHA-256 are
  `703b611c52cc8a4d55b84052e2a54386af203afb22f46f3617f9ec9d4a28bcaa` /
  `593477e79f303c8207574ae511961d7bd003db01ca433e92d1d0b43c9cc5c5db`.

- Bonus Feats source commits `8a8ea7a` and probe correction `4604717` passed
  mod load at `20260801T2319037969615Z-mod-load-smoke`. Independent PASS runs
  `20260801T2322304096701Z-disposable-gunslinger-bonus-feats` and
  `20260801T2324134236411Z-disposable-gunslinger-bonus-feats` both observed
  exact GUID `41c8486641f7d6d4283ca9dae4147a9f`, levels `4,8,12,16,20`, five
  occurrences, 108 aggregate candidates, and `IgnorePrerequisites=False`.
  Package/DLL SHA-256 are
  `ceb4c03d1c229e95eee278f366492f726683da007094be1829c3bed24c106e95` /
  `3b948b7cdaec3820678e94d4498700af3f30f0bee6a9f991b27409fc9746e71f`.

- Utility Shot source commit `8270ade` passed mod load at
  `20260801T2306012700758Z-mod-load-smoke`. Independent Stop Bleeding PASS runs
  `20260801T2307201904813Z-disposable-gunslinger-stop-bleeding` and
  `20260801T2308408422774Z-disposable-gunslinger-stop-bleeding` both observed
  grit `1 -> 1`, self bleeds `2 -> 1`, adjacent bleeds `1 -> 0`, discharged
  rounds `0/0`, zero-grit `InsufficientGrit` with the loaded chamber preserved,
  applied/rejected/fault counts `2/1/0`, and exact cleanup. Package/DLL SHA-256
  are `1387df20e7f43a34b93f0661a6dd193d1b264ea8bdfea8f84f02a1587b21d709` /
  `ebf4c938045e281a1c39910d53ceb8d53487a28689199e01df14d54522ad625e`.

- Pistol-Whip source commit `abfd426` passed mod load at
  `20260801T2244554687322Z-mod-load-smoke`. Independent PASS runs
  `20260801T2246129650055Z-disposable-gunslinger-pistol-whip` and
  `20260801T2247321424917Z-disposable-gunslinger-pistol-whip` proved one grit
  spent per delivered attack, native 1d6/1d10 melee surrogates, native Trip on
  each forced hit, enhancement propagation, zero-grit atomic rejection,
  unchanged firearm state, zero faults, and exact cleanup. Package/DLL SHA-256
  are `d89f093a3c4e0f78d8dfe660b49af1b2919c1ce482d774730835b4bb24c538ef` /
  `7a7da98b0770228b531df2f090389c463f5ca6bf2d5cd77b33f68ce0a4f1d564`.

- Gunslinger Initiative repair commit `8ecb3aa` passed mod load at
  `20260801T2211000340072Z-mod-load-smoke`. Independent PASS runs
  `20260801T2212165760676Z-disposable-gunslinger-initiative` and
  `20260801T2213341079691Z-disposable-gunslinger-initiative` proved grit
  `1 -> 1`, native modifier `0 -> 2`, duplicate stability, zero-grit rejection,
  zero faults, and exact cleanup. Package/DLL SHA-256 are
  `33e70a386dba3c28cf5ca4fc74cdfa6b291586da24d25269b44a4abf6e499d61` /
  `1e120508f20f13ab74136c244c409f396bc29c97e9ced175398ebf8b71101a26`.

- Nimble commit `b82768b` passed mod load at
  `20260801T2144324918751Z-mod-load-smoke`. Independent PASS runs
  `20260801T2145560578604Z-disposable-gunslinger-nimble` and
  `20260801T2147123020977Z-disposable-gunslinger-nimble` proved +5 ordinary AC
  in light/no armor, zero in medium armor, native flat-footed exclusion, and
  exact cleanup. Package/DLL SHA-256 are
  `e8244e972dfcd257f163246d1179a467e5a17ed6ab9caaa79c52dfeb18e7807f` /
  `dafbb6bd85f01df1497815d21e3ed97c81ee95cce2797c63b7252798a026272a`.

- Quick Clear commit `fb4fd51` passed mod load at
  `20260801T2119244427175Z-mod-load-smoke`. Independent PASS runs
  `20260801T2120422933376Z-disposable-gunslinger-quick-clear` and
  `20260801T2122029596275Z-disposable-gunslinger-quick-clear` proved standard
  no-spend repair, move one-grit repair, zero-grit atomic rejection, zero
  faults, and exact cleanup. Package/DLL SHA-256 are
  `0443b50c7af34857f5ae6eaf7fa491eaff52bae2b38fc133bec7d36d6f557ce4` /
  `e1c3d9273c73b0e1722922896128c60d21f035558b53cbb9f6fb43e9c792f746`.

- Gunslinger's Dodge source commit `f79b4a2` passed mod load at
  `20260801T2059139120474Z-mod-load-smoke`. Independent PASS runs
  `20260801T2102104686115Z-disposable-gunslinger-dodge` and
  `20260801T2103264904832Z-disposable-gunslinger-dodge` proved grit two to one,
  native prone, AC 20 to 24, duplicate stability, atomic insufficient rejection,
  zero faults, and exact cleanup. The movement alternative remains pending.
  Package/DLL SHA-256 are
  `40d738a160929a4c611aaa0263a53fe0d48ccca6fab2ecc93d8fc400b7dd9b4a` /
  `798e1fe7f96cc083de8493e164e5e640ccad2592481ea97251a4fe6cd5815677`.

- Deadeye commit `8ef8854` passed mod load at
  `20260801T2039304720861Z-mod-load-smoke`. Independent PASS runs
  `20260801T2040462109967Z-disposable-gunslinger-deadeye` and
  `20260801T2042028613784Z-disposable-gunslinger-deadeye` proved native armed
  fact consumption, grit two to one at the second increment, duplicate spend
  protection, atomic insufficient rejection, zero faults, and exact cleanup.
  Package/DLL SHA-256 are
  `a59e090b2d14911f77f64ba57d26762d2692c1b25ae7e3b6bd38f25b48d7e8f8` /
  `ec58e53a0c9747a34fa55db2290219cf868f13c171978171b1622f9d45ea2426`.

- Firearm grit recovery repair commit `b4c4874` passed mod load at
  `20260801T2010547590872Z-mod-load-smoke`. Independent PASS runs
  `20260801T2012145241329Z-disposable-gunslinger-grit-recovery` and
  `20260801T2013336952646Z-disposable-gunslinger-grit-recovery` proved exact
  critical `0 -> 1`, killing blow `1 -> 2`, duplicate stability, unaware-target
  rejection, zero faults, and exact cleanup. Package/deployed-DLL SHA-256 are
  `cbeeb299e4e10843cd01079b48ee8c702bd71cf4b469f99c294ac2fd4692bc93` /
  `1596f0b56a60144212E7680AA1DD1421DECB90D507B1725C424B3AE3C3759138`.

- One-time native grit initialization commit `4e543fd` passed mod load at
  `20260801T1945287324618Z-mod-load-smoke`. Independent persistence PASS runs
  `20260801T1946581078281Z-disposable-gunslinger-grit-persistence` and
  `20260801T1948157306155Z-disposable-gunslinger-grit-persistence` observed
  maximum two, spent/reconstructed current one, and current one after a later
  exact feature reapply. Package/deployed-DLL SHA-256 are
  `3f32686c52b0c6b21082a5966eb006032e616d15674d6eb73c2d14bf5f078421` /
  `ca3c62f4b48abb2baba8217b78276282474c3ab5deeace7af9704fd42ce319a7`.

- Native grit persistence repair commit `1949d80` passed mod load at
  `20260801T1935464087387Z-mod-load-smoke`. Independent PASS runs
  `20260801T1937081415049Z-disposable-gunslinger-grit-persistence` and
  `20260801T1938272020280Z-disposable-gunslinger-grit-persistence` proved
  maximum two/current one, distinct exact-blueprint JSON reconstruction at
  current one, and exact cleanup without save APIs. Package/DLL SHA-256 are
  `519c5f99a430808895d1ea787f832088f51b2b861b19974409d6bb2a02715429` /
  `0b37c7748cdca07c0015a730da3626b8a0a6a8d132c96e9ef72f64c325d60866`.

- Native daily grit rest commit `b0ca3f3` passed mod load at
  `20260801T1916580082273Z-mod-load-smoke`. Independent save-free PASS runs
  `20260801T1918185563653Z-disposable-gunslinger-grit-rest` and
  `20260801T1919385521658Z-disposable-gunslinger-grit-rest` proved
  `maximum=1;initial=1;spent=0;rested=1` with exact cleanup. Package/DLL
  SHA-256 are
  `9211a4cd8dfb0e9b2dc9c2092673f9b4fb947cf3849aa176685e13d5a4608694` /
  `997ad369b55856321a3c1ce8593dc219864a0a38857df086e46c5a8902f8e8d6`.

- Native grit repair commit `cd22f3d` passed mod load at
  `20260801T1907337714075Z-mod-load-smoke`. Independent save-free PASS runs
  `20260801T1908491510715Z-disposable-gunslinger-grit-resource` and
  `20260801T1910149815825Z-disposable-gunslinger-grit-resource` proved initial
  1/1, spend to zero, no level-up refill at Gunslinger 2, capped restore to one,
  and exact cleanup. Package/DLL SHA-256 are
  `4ddea7d37d08cd1255562cc8d21678ea686c01d1dc3a48ecaade6630d18c8fbd` /
  `da0d1e20a51dc288daa3383fbd0fff628b79b76194b7946c1e60a774d6d1543b`.

- Exact respec preview commit `3d4ba8f` passed mod load at
  `20260801T1836154433116Z-mod-load-smoke`, then two independent save-free PASS
  runs `20260801T1837314150470Z-disposable-gunslinger-respec-preview` and
  `20260801T1838472989503Z-disposable-gunslinger-respec-preview`. Both proved a
  fresh detached replacement at Fighter 0/Gunslinger 0 reached Gunslinger 1,
  while the original disposable source remained Fighter 1/Gunslinger 0; both
  detached entities were cleaned up. Package/DLL SHA-256 are
  `fffd41f772c7b8b3668c7b8c4d2e8364a16a19cb118dccba112a67d826721e05` /
  `5616dfd5da3eb32431c28501fa53289d6866364e818fa37a9568f235a9e6f36e`.

- Exact multiclass preview commit `c1eb9b7` passed mod-load at
  `20260801T1748568385594Z-mod-load-smoke`, then two save-free PASS runs:
  `20260801T1750132878481Z-0665958b379b4f8ca6067083a9ee9708` and
  `20260801T1751280423920Z-703b0d97c03843a28fedffe8c4392214`.
  Both proved source Fighter 1/Gunslinger 0 and preview Fighter 1/Gunslinger 1,
  with two actions and exact cleanup. Package/DLL SHA-256 are
  `14e5a1746638f1f1d48c4a9ccd79c92cd6307e1d2e96680355a7f8f873e9eedf` /
  `66265d13b598477701674ec05cf50dec009bf2809bca0a3cbd63533ec9cffd86`.

- Exact Sprint 34 level-up preview commit `84bb692` passed mod-load at
  `20260801T1741332784385Z-mod-load-smoke`, then two independent save-free
  `disposable-gunslinger-levelup-preview` runs:
  `20260801T1742575116740Z-8a6cca94fc1c4d97bda6a25e01dad80a` and
  `20260801T1744173560342Z-aae78c49ec4849d19bedbde1f12446fb`.
  Both proved isolated Gunslinger 1 -> 2 preview, unchanged source at level 1,
  two exact actions, and external cleanup. Package/DLL SHA-256 are
  `a1a2e199df996427eef7ca7f123fbdac9da37709c51f7ada5d2960a08455d63e` /
  `386772a6ab12125a40d7647e1d1049ed64a5c2bead7f81ade123d17b735e2472`.

- Exact Sprint 34 starting-item commit `cc2f77d` passed `mod-load-smoke` at
  `20260801T1731148707849Z-mod-load-smoke`, then two independent fresh-process
  `gunslinger-starting-items` runs:
  `20260801T1732334037249Z-ccbfd7861d04442782c358cf7d236dc9` and
  `20260801T1734133597055Z-4fa5d631d72f4b999751ceeb7d119fd5`.
  Both proved one Pistol, one powder, one ball, exact instance/quantity rollback,
  restored class identity/gold/money, stable working-save identity, and no save
  write. Package/DLL SHA-256 are
  `6a41ed815d910983e0067184fdfb40ca16629b8e70aab83f4d1a24fa4ff57153` /
  `2f4d2a0f2772b5923349ce467bbebf7426bb80348c25548d16f0238af23d0fb4`.

- Exact Sprint 31 catalog commit `1539ae9` passed `mod-load-smoke`, run ID
  `20260801T1334059331758Z-9736bc0a7d7844bd83bc9d26b5a30676`.
- Two fresh-process `production-firearm-catalog` PASS runs:
  `20260801T1335276981327Z-5145ec8fbc864500889d489fb4c23fad` and
  `20260801T1336546357107Z-1986affde5794aad8eb0710a31932eb0`.
- Exact deployed package SHA-256:
  `0ca093bd05eaa19a6dc3e3577b618fea2b3db018b29e61965fc0742815e2c342`;
  DLL SHA-256:
  `c0e59abe94e89ec478a55c43327c8ce7763851dc1d50f4a141c39e7ad0767473`.
  Both feature runs proved the three concrete catalog entries, marker/native
  isolation, special-range fail-closed behavior, stable working-save identity,
  and no save write.

- Exact Sprint 31 entry commit `67d7779` passed `mod-load-smoke`, run ID
  `20260801T1309524765214Z-f92ce74df2bc4c6695e6cdd3a6bbeeed`, and canonical
  `working-save-smoke`, run ID
  `20260801T1311109692839Z-c700cd91ca9c45e5aa9082adbc3ec263`.
- Exact deployed DLL SHA-256:
  `f133220a212d0cd6b7af21a58fc42af4f04f00c693329e3eb95185593eed6eaa`.
  The working save was uniquely correlated, the baseline was distinct and not
  loaded, the fingerprint was stable, and no save-writing API was observed.
- Exact commit `47fb861` passed `mod-load-smoke`, run ID
  `20260801T0435526657821Z-4ba4ea84718947f1a8cfc3de1d6ad76a`.
- Exact commit `47fb861` passed canonical `working-save-smoke`, run ID
  `20260801T0437220565711Z-d8664d7f634542f58d8d95126e90fe51`.
- Working-save evidence directory:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260801T0437220409268Z-working-save-smoke`.
- Deployed DLL SHA-256:
  `b1422ae9a2aed50a0ae8a8d2d3f4ff0defc5d03d31f142d7a19c57f5eb973d7b`.
- First active Sprint 33 invariant: exact multi-round state and inventory
  deltas remain atomic across partial top-up, persistence, repeated discharge,
  misfire, and two otherwise identical firearms.

## Current source evidence

- Nimble has five cumulative +1 native Dodge facts at levels 2/6/10/14/18,
  exact light/no-armor gating, equipment refresh, and native flat-footed
  exclusion. The guarded detached scenario covers no/light/medium armor and
  cleanup. Sprint 37 validation, 737/737 tests, exact-reference Release build,
  strict packaging, and 15 dispatch checks pass. Candidate package/DLL SHA-256:
  `3a322ff1b18dcc1e9cf80cbb2d89952b9b01a22ecc4b4a73400446bcd987a1ba` /
  `0188eb4ea037af6e81ed6c15674ee1735eed963302a70b2ccf45e38424bd6e40`.

- Quick Clear now has exact standard and move actions over one equipped
  item-owned misfire-broken firearm. Standard requires positive grit without
  spending; move spends one. Both repair without a kit and fail atomically.
  The guarded detached scenario covers both successes, zero-grit rejection,
  diagnostics, and cleanup. Validation, 732/732 tests, exact-reference Release
  build, and strict packaging pass. Candidate package/DLL SHA-256 are
  `01fd8fe73c53575c08f957aae99cae21fdb262e333949698f670b89fc732dd28` /
  `34fd2cd6acdc105f378bed9ab276acb3b2771a382fbbde3348b05ca239fb6b41`.

- First recovery run `20260801T2006592774427Z` failed safely because a newly
  constructed detached `UnitCombatState` retains `m_InCombat=false`. Exact
  `UnitEntityData.IsInCombat` IL delegates to that flag. The repaired fixture
  sets both exact detached flags and asserts their native getters before event
  evaluation; production recovery code is unchanged. Rebuild and commit before
  the repaired runtime attempt.
  The rebuild now passes 710/710 and strict packaging; candidate package/DLL
  SHA-256 are
  `9b982127b5bebe48d07b7ea4199dcfc0da114a9d9376939a0c402726698f82c2` /
  `36583d9893390380aedb83e3e0e4d952d8303d9a2d60c23159246337fee59b53`.

- Firearm grit recovery is source-qualified. Exact critical and exact native
  weapon-damage zero-crossing paths restore independently, weakly dedupe by
  attack/target reference, and fail closed for invalid combat/target contexts.
  A guarded detached runtime scenario covers both restores, duplicate calls,
  unaware-target rejection, and exact cleanup. Complete suite is 710/710;
  candidate package/DLL SHA-256 are
  `838a3882e5998da9496aaa608a55dfe73ced2433079f8f7ac97aee1b4d25047a` /
  `d589582a7a27d1e146009f7352a62bbd5e0e8c97fd6e96a02fef513dd6122c90`.

- Persistence run `20260801T1927062718434Z` passed JSON identity/reconstruction
  but exposed initial-fill ordering at Wisdom 14: maximum two, current one
  before spend. The active repair adds an owner/class-filtered
  `IUnitGainLevelHandler` restoring only when Gunslinger class level equals one;
  later levels remain non-refilling. Rebuild, commit, and retry next.
  The repair now passes 703/703 tests and strict packaging; candidate
  package/DLL SHA-256 are
  `92b0c3133e81c5cac2b1abb0a8c0fb1f8f952bf28b1120350db54dae703e2686` /
  `400a78b2c5950f45978bacb1e0aaeabfff4afa172c27639818a6f39bb292de2a`.
- Run `20260801T1932396799284Z` reproduced the initial-fill failure. Exact
  `ApplyLevelup` IL proves it does not raise the global gain-level event; it
  explicitly invokes unit-scoped feature reapply after queued actions. The
  repair now uses `IUnitReapplyFeaturesOnLevelUpHandler` with the same exact
  class-level-one guard. It passes 703/703 tests and strict packaging;
  candidate package/DLL SHA-256 are
  `0766e2c3e36dd8d84c8efad04e0e5293eda92bb1d101c898066e3af1f96ff503` /
  `3b9eccf6770898cc89493fc51a1754feda7a7459d32fbcef6672bda82b52f4d2`.
- Multiclass audit found class-level-one alone would refill on later unrelated
  levels. A new stable hidden per-unit initialization marker makes the exact
  reapply restoration one-time; the persistence scenario now requires a later
  reapply to preserve current one. All 703 tests and strict packaging pass;
  candidate package/DLL SHA-256 are
  `1bdc5cdfd0e32d170b16037495441883beff4be96f0bcccaa4b74819f3768efc` /
  `c72d2e3c7bbdb3ebce182a8ab29bdd5a468e4e6fec38e489935ebf205898bdee`.

- Native grit persistence round trip is source-qualified: a non-maximum current
  value uses Kingmaker `DefaultJsonSettings`, deserializes to a distinct record
  with the exact grit blueprint, and rebuilds a fresh detached resource map.
  Complete suite remains 703/703; candidate package/DLL SHA-256 are
  `ccf5facff8d846c8b6ac3598115fe9cebb57df9c6570b1a7aceb7f62b8cf3e2f` /
  `4d519cd6d807d01da2499402599101e5e7c599f951b9e978bb17a6b89ad67c61`.

- Exact IL shows native `RestController.ApplyRest(UnitDescriptor)` restores all
  registered unit resources and the supported-build eligibility helper always
  permits resource restore. A guarded detached `disposable-gunslinger-grit-rest`
  scenario is source-qualified with 703/703 tests and strict packaging.
  Candidate package/DLL SHA-256 are
  `5e4711b5fdfdd4c7ed478fa77e76ad2afddb124c691b2cf498cb5a69b00cd1a9` /
  `c77b482cbe7b9579cd69f88ececc3e551fdc43631e97a4466f4a01b04849f199`.

- First live grit attempt
  `20260801T1904500891309Z-disposable-gunslinger-grit-resource` failed safely
  in native `BlueprintAbilityResource.GetMaxAmount` because its runtime-created
  `Amount.Class` array was null. Commit `cd22f3d` resolved this by initializing
  all four native class/archetype arrays; the two subsequent runs passed.

- The native grit integration is source-qualified: stable resource and owner
  feature blueprints, exact Wisdom-floor subscriber, first-grant restore,
  no level-up refill, and a guarded detached-unit acceptance scenario. Full
  clean validation passes 703/703 tests; candidate package/DLL SHA-256 are
  `6ba5a00008d8d72b14a3db1439e519a02d5b347bdcca6c9db4916954d17ab5bd` /
  `8de23371914342fa66ae5a379c5d156ad874e4e233eaab081c094b068d6d6306`.

- Sprint 32 exact-reference target planning is implemented with 10 focused
  cases. Native cone/volley aggregation adds 10, one-discharge transactions add
  seven, triple-explosion policy adds six, and the fail-closed cone-distance
  boundary adds five; the complete domain suite is 662/662 PASS.
- Package candidate SHA-256:
  `e45b9e2253435a1e5926dccc6c9e00a9cadc96ae25af404a2749e0cc6249a639`;
  DLL candidate SHA-256:
  `33b9cddac997428d945c35e156f04ee004e7109d1a10bb08ed5fa409886c67f7`.
- This slice is source-qualified only. It does not establish cone length,
  native attack delivery or feature runtime acceptance. Native 90-degree
  directional geometry is contract-proven; numeric cone distance is not.
- Sprint 33 exact batch reload and partial top-up transactions add eight
  focused cases. Complete suite is 670/670 PASS. Package candidate SHA-256 is
  `f04448c1cc8de40b9eae5ad781730a4c911a10cd3d9aec83ce6f560d2710dd55`;
  DLL candidate SHA-256 is
  `75788f9ff56f76a3012754802e0c78d847da11c6c1ecd806abada23e065e2b51`.
- Advanced Rifle/Revolver definitions, six-round finite token semantics, and
  capacity-aware early/advanced misfire handling add twelve more focused cases;
  complete suite is 682/682 PASS. Latest package SHA-256 is
  `35527b3cba8d7764b3882e8910c58d43bc600741c1e13bc33ba6ff679afde94a`;
  DLL SHA-256 is
  `9ebbfaaf3dd023441c5e424b777fc04b4609ae2d5185ac8f953ff0fb11ec46ac`.
- Save-owned-vault reconstruction, two-item count isolation, and repeated
  canonical discharge add three cases; complete suite is 685/685 PASS. Latest
  package SHA-256 is
  `ea88730ad1a2aa208de081fb4e8e68000dc53e26be17b24403eeda1cd3d4d26e`;
  DLL SHA-256 is
  `5408f50e47c189115c42d3067250ade2491b2030b8f8b90acb60c7b2a42c82ad`.
- Four stable advanced blueprint IDs now produce exact Rifle/Revolver item/type
  pairs and extend guarded catalog acceptance. Complete suite remains 685/685
  PASS. Latest package SHA-256 is
  `30e5f6b53efcdef9068e2524945bad49b06a78e77ce745c9c29afc79bf0ad957`;
  DLL SHA-256 is
  `5621847b5197518f97e97912eb680b6a024adfde20448ee40fd8c496fa9deae5`.
- Exact commit `41f299a` passed `mod-load-smoke` run
  `20260801T1433034839705Z-56c2363396894593961542057943f189` and two expanded
  catalog runs `20260801T1434411929092Z-b69f2b13cc1f4a03945624f83ff3c5b9`
  and `20260801T1436113008241Z-3c3f36a8807e4ed3869826afd13a5543`.
  Both feature runs observed no save-writing API. Exact deployed package/DLL
  SHA-256 are `6a8386e782f47726c38be60cda52e5e9b335d943a3650426cf6263c5deb51cf2`
  and `ba0f28e9197e1fb6949de9e829a3ccd60b65d865cea2962b6a06528ee87b4a64`.

## Commands already run

- Read mission, roadmap, Sprint 30 report/entry criteria, architecture, source
  and test inventories, version files, and local class/firearm rule headings.
- Verified `codex/complete-gunslinger` descends from the qualified baseline.
- Repository validation passed. Exact Release domain suite passed 611/611.
  Exact private-reference Release build and strict package validation passed.
  Runtime package SHA-256 is
  `b253eaed27bccfd7841ca938032373bb13146984e94c021b1994cbd901397dfd`;
  DLL SHA-256 is
  `5ce1b5bf0d3563648e9fcd9629981c4ee41cf2fb59143df7dedf4f94fbe373de`.

## Sprint 30 closure

Commit `0052dad` passed exact mod load and two fresh-process feature runs. Latest
run ID is `20260801T0448285054152Z-4e5925080ce1422fbcb44c2ee07adcac`;
deployed DLL SHA-256 is
`de9f8507e5180adeb5df8dab4559e901da68022be556ef4fe1ffb874034e3d3f`.
Both feature runs reached `MaintenanceLoopPassed`, proved native Heavy Crossbow
isolation, and observed no save write.

## Next action

Sprint 83 is paused at a genuine player-facing design gate documented in
`planning/SPRINT-83-GUNSMITH-BATTERED-CONTRACT-AUDIT.md` and
`HUMAN-INPUT-REQUIRED.md`. Await authorization for persistent owner-bound,
class-bound, or maintenance-only Gunsmith/battered behavior. The recommended
choice is persistent originating-owner behavior. After authority is supplied,
implement it without invoking broad first-level creation Commit.

Sprint 82 native weapon-pipeline preservation is runtime-qualified on exact
commit `a3243c9`. Package/DLL hashes are
`7e873edd2bd0f54da09de789e7f15dd0200bb2e095484578804df0b2855b2388` /
`510ea8061af01a696b0f8b44dd186a798a65a6a3e9b94f08e41ac65dc6775696`.
Mod load `20260802T1245266307096Z-mod-load-smoke` and comprehensive runs
`20260802T1247085787494Z` / `20260802T1248346969150Z` passed. Commit this
curated evidence and audit the remaining character-creation/Gunsmith boundary
without invoking the explicitly unsafe broad first-level commit path.

Sprint 81 comprehensive acceptance is runtime-qualified on exact commit
`58baf84`. Exact mod load `20260802T1234452818737Z-mod-load-smoke` and fresh
comprehensive runs `20260802T1236067944023Z` and `20260802T1237355810113Z`
passed all 30 composed slices. Exact package/DLL hashes are
`2fd4229ceadaa2f2b59133bccff053dd2e05589b01cfd65eefa630cb0cc6a8b7` /
`27d42233c2b464431f858c8d36b695fdeefab2c2f156de9b822450e5624b5dbc`.
Commit this curated evidence, then audit every remaining non-final matrix row
against the mission hard-stop contract. Do not rerun bounded Startling Shot,
Targeting Head, detached respec commit, or lifecycle corruption attempts.
Do not stop merely at the comprehensive checkpoint.

Sprint 47 Targeting Legs is runtime-qualified on exact repair commit `1a2f29c`.
Mod-load run `20260802T0231549132685Z-mod-load-smoke` and independent feature
runs `20260802T0233121188720Z-7ef51e849a88459591847b21e4d7466e` and
`20260802T0234333804832Z-c0c8adb9ca9648f8bbbea1c7117546c9` passed, proving
positive normal damage, successful native Trip with prone aftermath, native
maneuver-immunity suppression, exact grit/chambers, and cleanup. Commit this
curated evidence and inspect exact installed support for Targeting Wings next;
do not stop at the Sprint 47 boundary.

Targeting Wings is now disposition-complete as
`OMITTED-NO-MEANINGFUL-INTERACTION`. Exact installed 2.1.7b metadata exposes
only a visual Wing animation selector and a Wings activatable-ability grouping,
not a general flight state or flight-loss rule. Commit the curated disposition
and proceed directly to Targeting Arms; do not substitute prone or infer wings
from creature names.

Targeting Arms is temporarily `BLOCKED`: native Kingmaker Disarm applies
margin-scaled main/off-hand buffs and cannot perform the tabletop chosen-item
drop. Automatic success would arbitrarily alter duration, and no local authority
selects a replacement. Preserve the audit in
`planning/SPRINT-49-TARGETING-ARMS-CONTRACT-AUDIT.md` and continue immediately
to Bleeding Wound; do not stop the overall mission on this independent design
question.

Sprint 50 Bleeding Wound is runtime-qualified on exact repair commit `5a939d7`.
Mod-load run `20260802T0321317830272Z-mod-load-smoke` and independent feature
runs `20260802T0322528218163Z-disposable-gunslinger-bleeding-wound` and
`20260802T0324126984990Z-disposable-gunslinger-bleeding-wound` passed all seven
mechanical/isolation assertions. The exact package/DLL hashes are
`412fa07d8831d5d07a5ac009278ad845460f7a5327ee421df2a26e5ea6cb620e` /
`05859c0ef58adc52e67bbb05eea3faebed389c016c8ceb4f6c203c3f83f72622`.
Curated Sprint 50 evidence is committed as `b84063f`. Sprint 51 Expert Loading
is runtime-qualified on exact source commit `79731d1` at version `0.0.51`.
Exact mod load `20260802T0343348846193Z-mod-load-smoke` and independent feature
runs `20260802T0344552821282Z-disposable-gunslinger-expert-loading` and
`20260802T0347194832064Z-disposable-gunslinger-expert-loading` passed. The
exact package/DLL hashes are
`81a00010bf9a38bf4f7be4d22409159dd828172113e71f48e33296dd172d09bd` /
`3e34b55954328cbbfdc33ed6b8745e83addea2735887ea948287db7a62e15f88`.
Commit the curated Sprint 51 evidence and continue immediately to Lightning
Reload.

Sprint 51 evidence is committed as `8d1de74`. Sprint 52 Lightning Reload is
runtime-qualified on exact source commit `df60f59` at version `0.0.52`.
Exact mod load `20260802T0409040682597Z-mod-load-smoke` and feature runs
`20260802T0410232157000Z-disposable-gunslinger-lightning-reload` and
`20260802T0411425931495Z-disposable-gunslinger-lightning-reload` passed. The
exact package/DLL hashes are
`dd90bd94ef0d6efc72a1a1f0ffecdec1f55f824e3d6f765a97ba2abe8f43d471` /
`902f32bc4b4c0ee0dd522f553e3cb799a6855c7a9c2af7f26d5137bebfd612fc`.
Commit the curated Sprint 52 evidence and continue immediately to Evasive.

Sprint 46 Targeting Torso is runtime-qualified on exact commit `cc629a5`.
Mod-load run `20260802T0209076152067Z-14117474bdfb443a988299a617157502`
and independent feature runs
`20260802T0210250762420Z-13a22fcb19dc4c328a56adbca72e666f` and
`20260802T0211456711945Z-392586b512d546f2a37db9da41a8568c` passed. Both
feature runs proved natural-18 isolation, natural-19 threat, native damage, two
grit/chambers, cleanup, and no save-backed interaction. Commit this curated
evidence and begin Targeting Legs immediately.

Sprint 45 Targeting Head is source-qualified through clean commits `e543356`
and `6b29536` at version `0.0.45`. Exact mod load passed, but fresh feature runs
`20260802T0145575387773Z-98fdf5c587d942ffb26d9f7d736a8e20` and
`20260802T0150467055365Z-ad41b1a17c7b4ecaa4003d7eafc0403d` exposed two exact
runtime gaps: direct `RuleAttackWithWeapon` dispatch resolves the hit without a
native damage rule, and the detached context applies the requested timed buff
as permanent. Do not attempt a third speculative repair. Inspect exact native
damage/timed-buff contracts later; begin the independent Targeting Torso slice
now.
Sprint 44 source and successive evidence-driven fixture checkpoints are
committed through `e34e40c`. Exact mod load passes, but the disposable
`DefaultPlayerCharacter` target retains an unidentified `RuleApplyBuff` veto
after the exact immortality rejection branch was removed. Do not launch another
Startling Shot variation until a narrower handler observation identifies the
veto. Advance independently to Targeting and return to the bounded fixture gate
before declaring Startling Shot runtime-qualified. Preserve the Dodge movement
alternative as a documented pending adaptation until deterministic destination
selection is safe. Broad
first-level `Commit` and native replacement callbacks remain deferred until
their global mutations have complete rollback proof; do not invoke them
speculatively.
Full first-level `Commit` remains
deferred until its global rest/entity/remote-companion/view mutations have a
complete rollback proof; do not invoke it speculatively.

Sprint 59 True Grit is runtime-qualified on exact commit `1d7c5b6` and version
`0.0.59`. Mod load `20260802T0801211781568Z-mod-load-smoke` and independent
feature runs `20260802T0802418511488Z-disposable-gunslinger-true-grit` and
`20260802T0804057434013Z-disposable-gunslinger-true-grit` passed. The exact
package/DLL hashes are
`4135fcfe0df73990c9a3cdac246462d64124638f7550cdf5939e87f51b84a144` /
`ad0beec416f6a4f79b06203a7a664c566e04a3d34bb2e6814bb535a6bc41ec90`.
Commit the curated Sprint 59 evidence and continue immediately to the next
incomplete independent coverage item; Sprint 59 is not a stopping boundary.

## Safety boundaries

Fourth-playtest checkpoint: timed Overhaul maintenance is source- and
runtime-qualified by guarded run
`20260803T2312021633458Z-disposable-overhaul-maintenance`. Package/DLL hashes
are `7d5b83ba05b27eb5aaf657f7016cd02395629147cd6bc60408f58f24629404e3` /
`438ab26030c6bea5ecd15dbd70ccde2e3c890f557c038d421e95f4268f9543db`.
Continue Phase 8 at condition tooltip/combat-log visibility and Quick Clear
presentation; do not treat the timed action checkpoint as full UX acceptance.

Native firearm condition tooltip source and bootstrap are qualified through
fresh mod-load `20260803T2317264754582Z-mod-load-smoke`; package/DLL hashes are
`4a3f9d55eae4ec59210804286bb25dcc964239e64b9a4d2ef552d13758b011be` /
`91828430d78309a5eeffd278be45f9ce048f255ca15d8760bb62a4819c0167b6`.
Continue Phase 8 with condition transition combat-log feedback and Quick Clear
presentation. Preserve final supervised tooltip layout/refresh verification.

Quick Clear is runtime-qualified, including native granted facts, semantic
icons, readable unavailable guidance, availability gates, costs, exact repair,
and cleanup, at `20260803T2323347859647Z-disposable-gunslinger-quick-clear`.
Package/DLL hashes are `dbfecb4500a9a8cf65f1456d4678e0a4b0d5e0c03bbf6911916248685749f8dd` /
`bf35cd354c344c4780dc4f9f4259b2a7ba6260e9ec4e895be6b3ffdf67452f12`.
Continue to the next incomplete independent Phase 9 visual/audio item while
preserving final supervised condition/Quick Clear UI acceptance.

The five-prefab bundle, including the provenance-cleared Killian Delias
Winchester mapped only to Advanced Rifle, is deterministic and runtime-resolved.
Bundle SHA-256 is `D902F279D8E745BC7852ABDEF6F7C03B97128C92F38641101D5DFC140E39FBFD`;
guarded evidence is `20260803T2330553131263Z-observe-production-firearm-fallbacks`.
Continue Phase 6/9 with actual doll socket/crossbow suppression and live audio
layering evidence; do not treat prefab instantiation as visual completion.

Production weapon visual parameters now drive the native equipment lifecycle
with exact custom firearm prefabs and cleared inherited crossbow sound/model
fallback. Guarded evidence
`20260803T2343075121479Z-observe-production-firearm-fallbacks` passes all five
entries; package/DLL hashes are `af3c5aeb...90a23` / `e2c13102...fe006`.
Next, prove the loaded doll hierarchy and enabled-renderer state, then perform
final supervised socket/scale/projectile and audible exactly-once acceptance.
Do not promote this structural checkpoint to completed visual/audio UX.

Rapid Reload and the semantic progression icon family are runtime-qualified at
`20260803T2353092670187Z-observe-gunslinger-presentation`. The preceding
`20260803T2350599488388Z` failure exposed missing progression-level traversal;
the final run proves eleven distinct core/deed/training/capstone/maintenance
icons and one top-level/five kind-isolated Rapid Reload choices. Package/DLL
hashes are `efae7436...8c2dd` / `8bc39668...04f1a2`. Preserve the generated
source and deterministic exporter; continue with loaded doll/audio, integrated
auto-use visibility, and final Grit/condition UI acceptance.

Firearm descriptions and native item qualities are runtime-qualified at
`20260804T0009378520857Z-observe-production-firearm-fallbacks`, following fresh
bootstrap PASS `20260804T0008070526912Z-mod-load-smoke`. Two rejected global
tooltip patch strategies timed out at `20260803T2359504427346Z` (PID 15468) and
`20260804T0004138320330Z` (PID 19340); both were explicitly terminated. The
scoped item-qualities patch reports semantic firearm metadata and live
condition without `<null>` text. Package/DLL hashes are
`c6a6b206...bbb2b4` / `b43890a9...5395f`. Continue loaded doll/audio and
integrated final UI acceptance; rendered tooltip wrapping and refresh remain
unproven.

The coherent `0.0.64` candidate is functionally qualified. Validator and
880/880 tests pass; package/DLL/bundle hashes are `dff6891d...9a02a` /
`f804c5c1...10b6e` / `D902F279...FBFD`. Final PASS evidence comprises mod load
`20260804T0017030833654Z`, presentation `20260804T0018308614483Z`, firearm
assets/qualities `20260804T0019578736107Z`, comprehensive pair
`20260804T0021240668392Z` / `20260804T0022573382219Z`, and canonical
working-save pair `20260804T0024344255556Z` / `20260804T0026123359766Z`.
No save-writing API was observed. Next and only remaining action is the single
supervised perception session in `FOURTH-PLAYTEST-VISUAL-ACCEPTANCE-CHECKLIST.md`;
do not split it into piecemeal review requests.

Superseding functional source checkpoint: `f3f3ab0` adds native player-facing combat-log
notifications for all firearm condition transitions. The 880/880 suite and two
deterministic Release builds pass. Final clean-commit evidence is mod load
`20260804T0122437045614Z`, presentation `20260804T0124121027946Z`, firearm
assets/qualities `20260804T0125380338184Z`, 190-assertion comprehensive pair
`20260804T0127055727006Z` / `20260804T0128458736571Z`, and canonical working-save
pair `20260804T0130335785001Z` / `20260804T0132153365435Z`. The working-save
fingerprint is identical and no save-writing API was observed. Only the one
consolidated human perception session remains.

The external `FOURTH-PLAYTEST-RUNTIME-UX-REPAIR-REPORT.md` is authoritative for
the clean post-handoff HEAD, package/DLL/MVID identity, and definitive runtime
IDs. Do not infer the current final Git identity from this committed resume file.

Current checkpoint: exact `d7fe62a` native vendor staging is runtime-qualified
by fresh working-save PASS pair `20260802T1820527823172Z` /
`20260802T1822316079053Z`. The player-sale direction is `AddForSell` /
`ItemsForSell` / `RemoveFromSell` against detached exact `Capital_Jhod`, with
complete rollback and no save writes. Commit this curated evidence, then
implement and qualify the durable `Deal`, repurchase, and restart lifecycle.
Do not stop at the pre-deal boundary.

Latest checkpoint: exact `4956175` actual sale/purchase transaction passed fresh
working-save runs `20260802T1912168233390Z` and `20260802T1913544926139Z`.
Production now patches exact `GetItemSellPrice`; the battered pistol credits 22
gp, the acquired catalog pistol is ordinary, all temporary funding and
inventory mutations roll back, and no save API occurs. Commit curated evidence
and proceed directly to Targeting Arms, then Death's Shot.

Launch only through Steam App ID 640820 and the guarded request mechanism. Use
only `KMG_AUTOMATION_WORKING`; never load or mutate
`KMG_AUTOMATION_BASELINE`. Never save, quicksave, send UI input, or infer a save
from Continue/newest ordering. Stop on ambiguous identity, entitlement, UI,
prerequisite, save-write, or result evidence.

Pistolero/Musket Master active checkpoint: exact `e178ec6` passed the combined
Musket Master mechanics/starter scenario with exact Musket/no-Pistol, 20/20,
kit, battered ownership, repeat stability, scoped proficiency, reload, range,
training, rollback, and no save writes. Commit the curated evidence, then close
the remaining expanded Pistolero deed branches and Twin Shot cleanup/turn
boundaries before persistence, compatibility, version, and final gates.

The expanded Pistolero and combined Musket Master fixtures are guarded-runtime
qualified. Generic archetype presentation and class observers passed on exact
`1cd4b2785052acbe5be03a7426c064ba14c2ee46` in directories
`20260808T1412495842884Z-observe-gunslinger-presentation` and
`20260808T1414251995558Z-observe-class-blueprint-contracts`. Next implement
and qualify archetype persistence/reconciliation transitions and starter
identity invariants, then continue to the bounded base-starter investigation,
version/profile transaction, compatibility profiles, and final gates.

Reconciliation is now runtime-qualified on exact `e47bf17` in
`20260808T1425513568150Z-disposable-archetype-reconciliation`; the five native
respec transitions all passed with exact external rollback. The next action is
the bounded installed-code base-starter-choice investigation, followed by the
version/profile transaction and final qualification matrix.

The optional base starter was safely deferred after exact installed-code
inspection. The 0.0.73 version/profile transaction now passes the full 930-test
build/package gate and all profile fixture/restoration tests; its local-runtime
package/DLL hashes are `506CBF11...57F6F8` / `7DE2A590...2564B`. Commit and
publish this transaction, then execute the required exact-profile matrices in
the order standalone, Arms & Armor, Toggle Custom Soundpacks, qualified
combined, and one bounded Call of the Wild sequence.

Standalone 0.0.73 profile PASS is complete under sentinel transaction
`compat-20260808T144718Z-253cb669aa6d`, with all eight committed scenarios PASS
and exact restoration verified. Commit/publish this evidence, then continue
immediately with Arms & Armor, Toggle Custom Soundpacks, qualified combined,
and the bounded Call of the Wild profile.

Arms & Armor profile is now also fully PASS with exact restoration under
`compat-20260808T150037Z-9db324f97b45`. Publish this evidence and proceed with
Toggle Custom Soundpacks next.

Toggle Custom Soundpacks also passed its complete eight-scenario matrix with
exact Mods/SoundBank restoration under `compat-20260808T151456Z-9d71de2d9c6e`.
Publish this evidence, then run qualified combined followed by the single
bounded Call of the Wild sequence.

Qualified combined core and high-risk transactions passed and restored exactly.
Its comprehensive run retains inherited Dodge and newly reproduces the
unchanged-source Torso cached-threat defect documented in the journal/blockers;
all archetype-specific slices remain PASS. Next run exactly one bounded Call of
the Wild sequence, then proceed to final fresh archetype passes and reports.

The single bounded Call of the Wild repaired-candidate sequence passed all
seven authorized scenarios under `compat-20260808T160604Z-d19d00ad690b` and
restored exactly. Preserve the public `CONFLICT-CONFIRMED` classification until
human chargen confirmation. Next rebuild the committed package, run two final
fresh-process passes of all complete archetype slices, and run two eligible
canonical working-save smokes before final reports and hashes.

Pistolero and Musket Master are independently qualified at the final 0.0.73
candidate. Exact `b705dab64f319cb009ef62f14242468b9f974efe` passed the clean
930-test/package gate, two Pistolero processes, two exact Musket Master starter/
mechanics processes, and two canonical working-save smokes. Final package/DLL
hashes are `CFFD4404...692868` / `66DC6C2D...536683`. The full Gunslinger
aggregate remains blocked only by the recorded inherited Dodge and unchanged-
source Torso defects. After the final documentation commit is published and
local/origin equality is verified, no autonomous archetype mission action
remains; do not merge. Call of the Wild still requires the short human chargen
catalog check before its public classification may change.
## Current resume point — Reliable and eight-item source catalog

The amended ten-blueprint catalog and shared 0..20 effective-misfire service pass
all source, domain, exact-reference Release, SoundBank, and strict package gates.
The exact next action is to commit and publish this coherent source checkpoint,
run a guarded save-free bootstrap observer from the clean SHA, record its exact
run ID, then implement the accepted capital/BTSL vendor and five-target fixed-loot
publication transactions. Do not revisit native Seeking forensics.
## Current resume point — acquisition publication

The bounded capital/BTSL vendor normalization and five-target fixed-loot
transactions pass source/build/package gates. Commit and publish this checkpoint,
then run the guarded save-free live graph observer from its clean SHA. After live
bootstrap proof, implement the dedicated acquisition observer and development-only
Rare Firearm Acceptance catalog/spawn/location-audit panel.
## Current resume point — dedicated acquisition observer

The inherited Jhod-era observer expectation was repaired after exact live failure
evidence, and the typed `observe-rare-firearm-acquisition` scenario now covers the
Smith/BTSL roster, exclusions, and five exact fixed-loot relationships. Commit and
publish this passing repair, then run that scenario from the clean SHA. Continue
with the development Rare Firearm Acceptance panel after the observer passes.
## Current resume point — combat and lifecycle qualification

The acquisition observer passes and the development-only catalog/spawn/location
audit surface plus practical manual checklist pass source/build/package gates.
Publish this checkpoint. Next implement and run focused magic-firearm blueprint,
Reliable zero-threshold/direct/scatter, Seeking concealment, static-enchantment
lifecycle, native enhancement/Fey Bane, and Thundering scatter qualification.
## Current resume point — blueprint/state runtime observer

The typed rare-firearm blueprint/state observer passes all source/build/package
gates. Commit and publish it, run it from the clean SHA, then implement the live
Reliable direct/scatter matrix and Seeking/native-property combat scenarios.

## Current resume point — combat-property runtime fixtures

The guarded `observe-rare-firearm-blueprint-contracts` run
`20260808T1915428002243Z-99c2bd30714c4647a7d91e40b494a6b0` passed on exact
published source `2fb5391ba27edfe67d032812ea94129ce6dc6086`. It proved all eight
live item identities/families/costs/weights/static enchantments, exact-item
Reliable and Seeking resolution, Reliable thresholds 0 and 2, and preservation
of The Last Word's three static enchantments across Loaded and Broken token
replacement. Next implement the deterministic live Reliable direct/scatter and
Seeking/native enhancement/Fey Bane/Thundering combat-property scenarios.

## Current resume point — Reliable and remaining native-property matrices

Project Seeking concealment behavior passed guarded runtime run
`20260808T1943391642314Z-5bde100e871d487cbd1c5357ebeb9bbe` on exact
published source `2ed81a147e709d13c450dc793b2082a633ea159a`. The same exact
native Blur buff and forced percentile 1 produced a control Concealment miss and
a The Last Word hit while both stored Partial concealment; the Seeking attack
used natural 19, attack bonus 135, AC 14, one discharge, threshold none, and no
misfire/state leak. The run also qualified the exact `IsSuccessRoll(d20)`
compatibility fallback when another installed patch bypasses the Roll setter.
Next implement and run the Reliable direct/scatter threshold matrix, followed by
static lifecycle and native enhancement/Fey Bane/Thundering qualification.

## Current resume point — scatter and native-property qualification

The guarded direct-fire `reliable-firearm-misfire-matrix` passed on exact
published source `4b0bcc6ea8ff0fa2c50ebac70f4483d559122e81`, run
`20260808T1948114670922Z-dddc0f3024424ac7bb628fdd7e02e080`. Reliable
Pistol natural 1 was a native miss with Normal condition; Reliable Musket roll
1 misfired/Broke and roll 2 hit normally; mundane Musket roll 2 misfired/Broke;
Broken thresholds were exactly Pistol trained/untrained 2/4 and Musket 3/5.
Next qualify Reliable through Scatter Shot and the native Thundering per-target
critical matrix, then native enhancement/Fey Bane and full static lifecycle.

## Current resume point — authorized Ovation fallback

Native Thundering's exact installed component is unconditional Sonic energy,
not critical-only. Two materially distinct guarded live scatter strategies
failed closed at native roll completion. The authorized fallback is selected:
Ovation is now +4 Reliable, effective +5, cost 52,300. Next run the full source,
build, package, and guarded scatter gates for the fallback; then complete static
enchantment lifecycle and remaining native enhancement/Fey Bane qualification.

The final Ovation fallback scatter gate passed as runtime ID
`20260808T2025312832560Z-6f915d9e74bc4165963f48798ce00149` on published source
`9d9e4c88201e40e06030f26674d0b7c1969c0495`. Next implement and qualify the
complete magic-firearm static-enchantment/state-token lifecycle, then close
native enhancement/Fey Bane damage evidence and integrated regression gates.

The Last Word maintenance lifecycle passed in run
`20260808T2040179460252Z-5438cbb8e26c4cdeb513b3a8d8ba03ba` on source
`ae4b309d4574eea28f67ea3254177ac4db1cebf2`: exact static count 3/3/3 across
Overhaul and Repair, plus all timing, kit, log, and cleanup contracts. Next
qualify Quick Clear and reconciliation/transfer lifecycle coverage, then finish
native damage properties, version 0.0.74, compatibility, and final regressions.

Quick Clear static lifecycle passed in run
`20260808T2044205593443Z-1587853acc1a49959806b534bffdd6cb` on source
`a329a1260beb73bde362b506e8ab1c96ab7de1ba`. Next complete reconciliation and
inventory-transfer isolation, then native enhancement/Fey Bane damage evidence,
0.0.74 versioning, compatibility profiles, and final regression passes.
# Active work order: Paper Cartridges

Branch: `codex/paper-cartridges-auto-reload`

Baseline: `759685077da0aed6d7ed1fda2cd43e5ad12d0bdb`

Durable contract: `planning/PAPER-CARTRIDGES-AUTO-RELOAD-MISSION.md`

Current checkpoint: Phase 1 foundation is source-qualified at 941/941 with 245
active registrations and 246 ledger identities. Commit/publish Phase 1 and
verify remote equality, then begin Phase 2's one reload plan, generic atomic
source transaction, native Paper mode/grants, and manual reload runtime path.

## Current resume point — Paper persistent-mode crash repair

The published 0.0.74 candidate at `82de97fdc3e5c00d063b24d06b3d387075430d40`
is rejected pending repair qualification. Exact installed IL proves the marker's
null `FxOnStart` is dereferenced by original view-backed
`Buff.SpawnParticleEffect`; `FxOnRemove` has the same unsafe removal contract.
The installed Call of the Wild prefix only checks `Fact.Active`, so it exposes
the composed stack name but is not the null cause. The unchanged-GUID repair
initializes both empty FX links and both marker/mode empty resource-ID arrays,
with startup validation. Next: deterministic/build gates, then an exact live
party-view lifecycle observer under standalone and required compatibility profiles.

Repair qualification is complete. On 2026-08-09 the user successfully loaded an
affected game and traveled between dungeon floors, closing the final human recovery
gate. The unchanged marker identity, autonomous lifecycle/compatibility evidence,
and human area-transition result jointly qualify the repaired 0.0.74 candidate.

## Current resume point — feature modules release complete

The Feature Modules, Acadamae Graduate, and Cord of Stubborn Resolve work order is complete on `codex/feature-modules-acadamae-graduate`. Exact release source `8fea913d7baba880496012576878ac95eaadc74d` passed 967/967 deterministic tests, clean Release/package qualification, all four standalone module settings, consecutive standalone and maximum-risk combined integration runs, and two guarded working-save smokes. Final evidence is in `docs/FEATURE-MODULES-ACADAMAE-GRADUATE-QUALIFICATION.md`. Next retain the branch unmerged for user review.
# Shield Other active mission (2026-08-09)

- Current phase: Phase 4 stable blueprint construction and base publication.
- Last completed command/check: guarded final-live inventory PASS at
  `20260810T0408000666226Z-observe-shield-other-inventory`; 104,644 assets and
  zero Shield Other candidates.
- Exact current commit: `ef8b80fd43b0e52e1e804c0399a3a0a3d94e9ef9`
  (curated runtime evidence changes are uncommitted).
- Exact next action: commit/publish curated inventory evidence, then register
  stable ability/target-buff identities and transactional level-2 base lists.
- Remaining gates: final duplicate scan; module/settings; identities;
  lifecycle; exact split; base/CotW publication; complete deterministic suite;
  Release build/package; guarded runtime profiles twice each; documentation,
  hashes, commits, push-helper publication, and remote-SHA verification.
- Active blocker: none.

## Shield Other current resume - stable identities checkpoint

- Current phase: Phase 7 transactional spell-list publication.
- Last completed command/check: `Build-Local.ps1` PASS with 975/975 tests,
  exact-reference Release build, and strict package validation.
- Exact current commit: `49725186571660560ff693414cdf6ac21ae75705` with the
  stable-blueprint checkpoint ready to commit.
- Exact next action: commit/publish stable identities, verify remote equality,
  then implement the pure-tested transactional five-list level-2 publisher.
- Remaining gates: base/CotW publication; lifecycle component; exact damage
  transpiler/transfer; runtime scenarios; 0.0.77 version/docs/package; required
  consecutive standalone/CotW/combined profile passes; final hashes and audit.
- Active blocker: none.

## Shield Other current resume - base publication checkpoint

- Current phase: Phase 7 optional final-live CotW reconciliation.
- Last completed command/check: `Build-Local.ps1` PASS with 977/977 tests,
  exact-reference Release build, and strict package validation.
- Exact current commit: `c4091a8cca908fd0997eb9813d62e3dc7230388f` with the
  base-publication checkpoint ready to commit.
- Exact next action: commit/publish base publication, verify remote equality,
  then add first-idle structural CotW discovery/reconciliation and second pass.
- Remaining gates: CotW publication; lifecycle component; exact damage
  transpiler/transfer; runtime scenarios; 0.0.77 release; consecutive runtime
  profile gates; final package hashes, restoration/protected-save audit.
- Active blocker: none.

## Shield Other current resume - optional publication checkpoint

- Current phase: Phase 9 guarded blueprint/publication runtime observation.
- Last completed command/check: `Build-Local.ps1` PASS with 978/978 tests,
  exact-reference Release build, and strict package validation.
- Exact current commit: `cd44575bcd44d52798b4f68204259e3b49fc6cd3` with
  first-idle CotW reconciliation ready to commit.
- Exact next action: commit/publish, verify remote equality, then implement and
  run the guarded save-free Shield Other blueprint/list observer on standalone
  and exact CotW profiles before lifecycle/damage runtime code.
- Remaining gates: live publication proof; lifecycle; exact damage transfer;
  disposable behavior scenarios; 0.0.77 release; consecutive standalone/CotW/
  combined gates; hashes, restoration, protected-save audit, final report.
- Active blocker: none.

## Shield Other current resume - publication observer ready

- Current phase: Phase 9 guarded blueprint/publication runtime observation.
- Last completed command/check: `Build-Local.ps1` PASS with 978/978 tests and
  exact-reference Release/package qualification.
- Exact current commit: `a5769addf2709d9ea72268b767ef45c0a38f3769` with
  strengthened observer changes ready to commit.
- Exact next action: commit/publish, rebuild the clean SHA, then run
  `observe-shield-other-inventory` through the guarded Steam launcher and
  inspect exact eight-list and casting-model diagnostics.
- Remaining gates: runtime publication proof; lifecycle; exact split;
  disposable scenarios; version 0.0.77; compatibility repetitions; final audit.
- Active blocker: none.

## Shield Other current resume - first-idle timing repair

- Current phase: guarded publication observer retry after evidence-backed repair.
- Last completed check: repaired `Build-Local.ps1` PASS, 978/978 tests and exact
  Release/package. Failed run evidence is
  `20260810T1235457719557Z-observe-shield-other-inventory`.
- Exact current commit: `2e2f6d042bc5470bb19c8fa62812cdb6fad48dfe` with the
  first-idle timing repair ready to commit.
- Exact next action: commit/publish repair, rebuild clean SHA, rerun guarded
  `observe-shield-other-inventory`, then inspect structured membership/model data.
- Remaining gates: live publication; lifecycle/split; behavior/profile runtime;
  0.0.77 release and final audit. Active blocker: none.

## Shield Other current resume - observer self-filter

- Current phase: final publication observer PASS capture.
- Last completed runtime result:
  `20260810T1239058867514Z-observe-shield-other-inventory`; eight-list
  publication PASS, duplicate assertion false-positive on the project GUID.
- Exact current commit: `d2a76d896fb9d00507a1c7a306c67303997f35ca`; observer
  self-filter change is uncommitted.
- Exact next action: build, commit/publish, rerun guarded observer once, curate
  PASS evidence, then begin link lifecycle/runtime component implementation.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - compatibility repetitions PASS

- Current phase: Phase 9 guarded real-save persistence, then release conversion.
- Last completed check: two consecutive CotW and two consecutive highest-risk
  combined expanded runtime PASS runs on exact commit
  `76e5ce35a807388053ed4f911ea4f1c892611870`; all four restorations `True`.
- Exact current commit: `76e5ce35a807388053ed4f911ea4f1c892611870`;
  compatibility evidence curation is uncommitted.
- Exact next action: implement the guarded two-stage working-save persistence
  scenario using only `KMG_AUTOMATION_WORKING`.
- Remaining gates: real save/load, 0.0.77 docs/version/package/hashes and final
  release-source audit/repetitions. Active blocker: none.

## Shield Other current resume - disposable restored / real-save next

- Current phase: Phase 9 guarded real-save persistence implementation.
- Last completed check: `20260810T1517266807175Z-disposable-shield-other`
  passed on exact commit `0c6a3c7d6ebd9110c78a88bb00beaa625f1444f8`;
  transaction restoration was exact.
- Exact current commit: `0c6a3c7d6ebd9110c78a88bb00beaa625f1444f8`;
  evidence curation is uncommitted.
- Exact next action: inspect and extend the guarded working-save state machine for
  a two-launch Shield Other persistence check using only `KMG_AUTOMATION_WORKING`.
- Remaining gates: real save/load; repeated standalone/CotW/high-risk profiles;
  0.0.77 release/docs/package/hashes. Active blocker: none.

## Shield Other current resume - real-save persistence strategy

- Current phase: Phase 9 guarded real-save persistence implementation.
- Last runtime proved paired partial JSON is also not the save boundary;
  transaction `compat-20260810T151320Z-66eaf4e97106` restored exactly.
- Exact current commit: `de1be27fceaf80ef19b4647ab28c7d7283764e35`;
  synthetic persistence fixture removal and evidence records are uncommitted.
- Exact next action: build/commit/publish, confirm disposable scenario PASS, then
  extend guarded working-save stages for Shield Other using only
  `KMG_AUTOMATION_WORKING`.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - two-unit native save graph

- Current phase: Phase 9 save-context full graph qualification.
- Last runtime proved the descriptor utility strips live buffs (`rawFacts=0`);
  all gameplay assertions and restoration passed.
- Exact current commit: `e7d2e6dc7bb41536d463ed23468f6b77b4dadf5b`;
  paired caster/subject `UnitEntityData` native JSON graph is uncommitted.
- Exact next action: build, commit/publish, and run guarded standalone.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - stable-GUID post-load lookup

- Current phase: Phase 9 save-context cross-load identity qualification.
- Last runtime completed the exact loader lifecycle but the fixture incorrectly
  required blueprint CLR-reference identity across load; restoration was exact.
- Exact current commit: `16e78d32400c37e44fc0e418fd94ea3860942781`;
  stable-GUID lookup and raw-fact instrumentation are uncommitted.
- Exact next action: build, commit/publish, and rerun guarded standalone.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - exact JSON unit loader fixture

- Current phase: Phase 9 save-context loader qualification.
- Last runtime failed because the view constructor was not the save path;
  transaction restoration was exact.
- Exact current commit: `83e31a7585a834dcd8728f06ed6b4c7ce7793fbd`;
  reflected `JsonConstructorMark` + private `Initialize` loader sequence is
  uncommitted.
- Exact next action: build, commit/publish, and rerun guarded standalone.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - native unit PostLoad persistence fixture

- Current phase: Phase 9 save-context post-load qualification.
- Last runtime proved descriptor reconstruction but no materialized `RawFacts`
  before unit post-load; all gameplay checks passed and restoration was exact.
- Exact current commit: `45549c369d8d115c28dcb07fbe63cddfcf997700`;
  detached `UnitEntityData.PostLoad` fixture is uncommitted.
- Exact next action: build, commit/publish, and rerun guarded standalone.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - save graph field instrumentation

- Current phase: Phase 9 save-context reconstruction diagnosis.
- Last runtime `20260810T1455123546800Z-disposable-shield-other` serialized a
  20,465-byte unit graph and passed every gameplay assertion, but the combined
  reconstructed-context check was false; restoration was exact.
- Exact current commit: `57181a87a426e3be67f66eb312289cf26c112d9c`;
  field-by-field reconstruction instrumentation is uncommitted.
- Exact next action: build, commit/publish, rerun, and repair only the exact
  unresolved native reference boundary.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - exact unit-save graph repair

- Current phase: Phase 9 save-context runtime repair.
- Last runtime `20260810T1450578085280Z-disposable-shield-other` failed because
  top-level `MechanicsContext` serialization is not a supported save boundary;
  transaction `compat-20260810T145017Z-507e7c5d18d3` restored exactly.
- Exact current commit: `85f94fbcd24f88471dd037aa34ddbffbbfc01f60`;
  replacement with `UnitSerialization.Serialize(UnitDescriptor)` is uncommitted.
- Exact next action: run `Build-Local.ps1`, commit/publish, and rerun standalone.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - native save-context fixture source-qualified

- Current phase: Phase 9 save-context runtime qualification.
- Last completed command/check: `Build-Local.ps1` passed repository validation,
  981/981 tests, exact Release, and package validation for the exact native
  `MechanicsContext` `DefaultJsonSettings` round trip.
- Exact current commit: `825dd49f3c6f668a218108cb6abdc5eeef2c382d`;
  save-context fixture and evidence records are uncommitted.
- Exact next action: commit/publish and run guarded standalone
  `disposable-shield-other`.
- Remaining gates: repeated standalone/CotW/high-risk profiles; 0.0.77
  release/docs/package/hashes. Active blocker: none.

## Shield Other current resume - lethal-state lifecycle repair

- Current phase: Phase 9 lifecycle repair qualification.
- Last runtime: `20260810T1442147693813Z-disposable-shield-other` failed only
  immediate post-lethal removal; area unload and all other assertions passed.
  Transaction `compat-20260810T144135Z-dcad894ec3af` restored exactly.
- Exact current commit: `454652a71fdb048638cb11428037332aacfd3f31`;
  full lethal-state validity repair and focused source test are uncommitted.
- Exact next action: run `Build-Local.ps1`, commit/publish, and rerun standalone.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - area/death lifecycle runtime PASS

- Current phase: Phase 9 save/load persistence qualification.
- Last completed check: `20260810T1446127542431Z-disposable-shield-other`
  passed on exact commit `79552bd9974907bf001ca645d0974cb05e72dfd8`;
  area unload and full lethal-state link removal passed, restoration `True`.
- Exact current commit: `79552bd9974907bf001ca645d0974cb05e72dfd8`;
  this evidence curation is uncommitted.
- Exact next action: inspect Kingmaker's exact save serializer and add a
  request-local buff/context serialization round trip or, if only a real save
  can prove it, use the authorized working-save guarded mechanism.
- Remaining gates: save/load; repeated standalone/CotW/high-risk profiles;
  0.0.77 release/docs/package/hashes. Active blocker: none.

## Shield Other current resume - mitigation/immunity runtime PASS

- Current phase: Phase 9 remaining lifecycle and save/load qualification.
- Last completed check: `20260810T1436273826468Z-disposable-shield-other`
  passed on exact commit `1c152496abb5af17f016d2e1a6c7d03532d64666`;
  target DR/resistance applied once, caster defenses did not reapply, target
  immunity produced `0/0`, all regressions passed, restoration `True`.
- Exact current commit: `1c152496abb5af17f016d2e1a6c7d03532d64666`;
  this evidence curation is uncommitted.
- Exact next action: inventory and implement request-local area-transition and
  serialization round-trip observations without protected-save mutation.
- Remaining gates: area/save-load; two-pass standalone, CotW, highest-risk
  combined; 0.0.77 release/docs/package/hashes. Active blocker: none.

## Shield Other current resume - area/death fixture source-qualified

- Current phase: Phase 9 lifecycle runtime qualification.
- Last completed command/check: `Build-Local.ps1` passed repository validation,
  981/981 tests, exact Release, and package validation after adding caster-unload
  and post-lethal round-tick removal assertions.
- Exact current commit: `46683bfaa9e4cbf7570729d52000344d63f1d72c`;
  lifecycle fixture and evidence records are uncommitted.
- Exact next action: commit/publish and run guarded standalone
  `disposable-shield-other`.
- Remaining gates: real save/load; repeated standalone/CotW/high-risk profiles;
  0.0.77 release/docs/package/hashes. Active blocker: none.

## Shield Other current resume - eight-way standalone module matrix PASS

- Current phase: Phase 9 typed-damage and remaining lifecycle runtime qualification.
- Last completed check: all eight `observe-feature-module-settings` combinations
  passed on exact commit `c90c0e43710e4801694b770de5e63121e00d2fd3` with
  254 identities, independent publication surfaces, and exact transactional
  restoration. Final evidence: `20260810T1424139686495Z-observe-feature-module-settings`.
- Exact current commit: `c90c0e43710e4801694b770de5e63121e00d2fd3`;
  this evidence curation is uncommitted.
- Exact next action: extend/run `disposable-shield-other` for typed physical,
  energy, immunity/mitigation, and remaining area/death/save-load assertions.
- Remaining gates: expanded runtime damage/lifecycle coverage; two-pass standalone,
  CotW, and highest-risk combined profiles; 0.0.77 release/docs/package/hashes.
- Active blocker: none.

## Shield Other current resume - typed packet fixture source-qualified

- Current phase: Phase 9 typed-damage runtime qualification.
- Last completed command/check: `Build-Local.ps1` passed repository validation,
  981/981 deterministic tests, exact-reference Release build, and strict package
  validation after adding native piercing/fire fixture cases.
- Exact current commit: `033860b42c258363a8327e5ab20750de81d903ea`;
  typed fixture and evidence records are uncommitted.
- Exact next action: commit/publish, then run guarded standalone
  `disposable-shield-other` and inspect typed target/caster HP deltas.
- Remaining gates: mitigation/immunity, area/death/save-load; two-pass standalone,
  CotW, highest-risk combined; 0.0.77 release/docs/package/hashes.
- Active blocker: none.

## Shield Other current resume - typed physical/energy runtime PASS

- Current phase: Phase 9 mitigation/immunity runtime qualification.
- Last completed command/check: guarded standalone
  `20260810T1430187656299Z-disposable-shield-other` passed on exact commit
  `ee0b4fa5f4d30b94dcb10c141eabac139fe6594b`; piercing and fire 4 each split
  `2/2`, all prior assertions passed, and transaction restoration was `True`.
- Exact current commit: `ee0b4fa5f4d30b94dcb10c141eabac139fe6594b`;
  this evidence curation is uncommitted.
- Exact next action: inventory native fixture-safe DR/resistance/immunity facts,
  add focused mitigation cases, then source- and runtime-qualify them.
- Remaining gates: mitigation/immunity, area/save-load; two-pass standalone,
  CotW, highest-risk combined; 0.0.77 release/docs/package/hashes.
- Active blocker: none.

## Shield Other current resume - mitigation fixture source-qualified

- Current phase: Phase 9 mitigation/immunity runtime qualification.
- Last completed command/check: `Build-Local.ps1` passed repository validation,
  981/981 tests, exact Release, and strict package validation for request-local
  DR 2, fire resistance 2, caster DR 100, and fire-immunity cases.
- Exact current commit: `582fef4a4457830283abeee6d7c6291f445ae0b7`;
  mitigation fixture and evidence records are uncommitted.
- Exact next action: commit/publish and run guarded standalone
  `disposable-shield-other` to prove once-only native defense processing.
- Remaining gates: area/save-load; two-pass standalone, CotW, highest-risk
  combined; 0.0.77 release/docs/package/hashes. Active blocker: none.

## Shield Other current resume - publication runtime qualified

- Current phase: Phase 5 link lifecycle runtime implementation.
- Last completed check: guarded PASS
  `20260810T1243101482251Z-observe-shield-other-inventory` on exact
  `823195c683d633fabbe4916119e847e65bb61013`; all eight lists exactly once.
- Exact current commit: `823195c683d633fabbe4916119e847e65bb61013` with curated
  publication evidence ready to commit.
- Exact next action: commit/publish evidence, then implement the save-persistent
  caster-linked target-buff component, lifecycle/range revalidation, and tests.
- Remaining gates: lifecycle; exact damage transfer; disposable behavior;
  version 0.0.77; required profile repetitions; final artifacts/audits/report.
- Active blocker: none.

## Shield Other current resume - link lifecycle source-qualified

- Current phase: Phase 6 exact finalized HP damage splitting.
- Last completed command/check: `Build-Local.ps1` PASS with 979/979 tests,
  exact-reference Release build, strict package validation.
- Exact current commit: `48d4433341fc92fce89759c721b45ae263be77db` with the
  lifecycle checkpoint ready to commit.
- Exact next action: commit/publish, then implement the selected post-difficulty/
  pre-LastHandledDamage transpiler callback, immediate link revalidation, and
  exception-safe transferred-event guard.
- Remaining gates: split runtime; disposable behavior; 0.0.77/profile/release
  qualification and final audits. Active blocker: none.

## Shield Other current resume - damage runtime source-qualified

- Current phase: Phase 9 disposable Shield Other behavior/runtime qualification.
- Last completed command/check: approved unrestricted `Build-Local.ps1` PASS
  with 980/980 tests, exact-reference Release build, and strict package validation.
- Exact current commit: `9a00d9662994d14f537ef382ce7d5771e4448807`
  with the finalized-damage runtime checkpoint ready to commit.
- Exact next action: commit/publish and verify remote equality, then implement a
  guarded disposable scenario proving casting, bonuses, lifecycle, even/odd
  transfer, defenses/temp HP, death, exclusion, and recursion contracts.
- Remaining gates: disposable runtime behavior; module/profile repetitions;
  0.0.77 version/docs/package; final hashes, restoration/protected-save audit,
  implementation report, clean tree, and remote equality.
- Active blocker: none.

## Shield Other current resume - disposable split scenario ready

- Current phase: Phase 9 first guarded Shield Other mechanics run.
- Last completed command/check: `Build-Local.ps1` PASS with 980/980 tests,
  exact-reference Release build, and strict package validation; runtime preflight
  separately passes 86 checks.
- Exact current commit: `d5891b5233c1b3e5fc281003332f6a1291f989fc`
  with the disposable scenario changes ready to commit.
- Exact next action: commit/publish, verify remote equality, rebuild the clean
  SHA, run guarded `disposable-shield-other`, and inspect structured evidence.
- Remaining gates: expand behavior/damage/exclusion lifecycle matrix; disabled
  module and profile repetitions; 0.0.77 release/docs/package; hashes/audits.
- Active blocker: none.

## Shield Other current resume - switch to isolated standalone profile

- Current phase: Phase 9 guarded standalone mechanics qualification.
- Last completed runtime attempt: installed-profile run
  `20260810T1305018393571Z-disposable-shield-other` reached exact build/request
  ownership but no consumption before the established 600-second CotW bound;
  no save was named or touched. The unattended process was then terminated.
- Exact current commit: `0a2698a9fcd902526101acd98473b739b47cf9b9`;
  compatibility-runner allowlist change and timeout journal are uncommitted.
- Exact next action: qualify/commit/publish this strategy change, then invoke
  `gunslinger-only` transactionally for `disposable-shield-other` and verify
  exact Mods restoration plus structured mechanics evidence.
- Remaining gates: expanded mechanics matrix; CotW and combined repetitions;
  disabled module; 0.0.77 docs/package/hashes/audits/final report.
- Active blocker: none; installed broad-profile startup strategy is retired.

## Shield Other current resume - exact transpiler type repair

- Current phase: repair qualification before standalone mechanics retry.
- Last completed check: failed isolated evidence
  `20260810T1320348526120Z-disposable-shield-other` identified the exact
  `LastHandledDamage` declaring-type mismatch; compatibility transaction
  `compat-20260810T131953Z-8ac491185118` restoration verified `True`.
- Exact current commit: `d7978be23729303cd22510ad1cfc9bddabee741d`;
  one-line production type repair plus focused test/docs are uncommitted.
- Exact next action: run complete validation, commit/publish, rebuild clean SHA,
  and rerun transactional `gunslinger-only` `disposable-shield-other`.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - first standalone mechanics PASS

- Current phase: Phase 9 expanded disposable behavior/damage matrix.
- Last completed runtime check: transactional standalone PASS
  `20260810T1330351072971Z-disposable-shield-other` on exact published source
  `5e81423e9002234b1cf0a77de7b8357aef67d77e`; bonuses 1/1/1/1, odd split 1/2,
  even split 2/2, two logs, cleanup true. Transaction
  `compat-20260810T132959Z-c342ce67b0f1` restoration verified `True`.
- Exact current commit: `5e81423e9002234b1cf0a77de7b8357aef67d77e`;
  curated PASS documentation is uncommitted.
- Exact next action: commit/publish evidence, then expand the scenario for link
  replacement/multiple subjects, range/death/dispel, defenses/temp HP/lethal,
  exclusions, and reciprocal recursion before profile repetitions.
- Remaining gates: expanded matrix; module/profile repetitions; 0.0.77 release,
  final hashes/audits/report. Active blocker: none.

## Shield Other current resume - expanded lifecycle scenario ready

- Current phase: Phase 9 expanded link lifecycle runtime qualification.
- Last completed check: `Build-Local.ps1` PASS with 980/980 tests, exact Release,
  and strict package validation after adding replacement, multiple-subject,
  out-of-range, and dispel-equivalent coverage.
- Exact current commit: `a72765b800d26d686955e6a883a1d1bfc1e7aa9a`;
  scenario and durable progress changes are uncommitted.
- Exact next action: commit/publish, rebuild clean SHA, and run transactional
  standalone `disposable-shield-other` to inspect expanded structured evidence.
- Remaining gates: death/area/save-load; defenses/temp HP/lethal/exclusions/
  recursion; module/profile repetitions; 0.0.77 final release. Blocker: none.

## Shield Other current resume - expanded lifecycle runtime PASS

- Current phase: Phase 9 damage defenses, temporary HP, lethal, exclusions, and
  recursion runtime matrix.
- Last completed runtime check: standalone PASS
  `20260810T1337175179075Z-disposable-shield-other` on exact
  `9afdc95caa124523f6778f1fd9bb75b60d949cad`; replacement, two subjects,
  range termination, and dispel-equivalent removal all passed. Transaction
  `compat-20260810T133642Z-7b898f99d298` restoration verified `True`.
- Exact current commit: `9afdc95caa124523f6778f1fd9bb75b60d949cad`;
  curated PASS documentation is uncommitted.
- Exact next action: commit/publish evidence, then expand live damage coverage
  for target mitigation, both units' temporary HP, caster lethal damage,
  non-HP exclusions, and reciprocal recursion guard.
- Remaining gates: area/death/save-load and full damage matrix; module/profile
  repetitions; 0.0.77 release/audits/report. Active blocker: none.

## Shield Other current resume - exclusions/recursion/lethal scenario ready

- Current phase: Phase 9 expanded damage runtime qualification.
- Last completed check: `Build-Local.ps1` PASS with 980/980 tests, exact Release,
  and strict package validation after adding stat-damage exclusions, reciprocal
  recursion suppression, and lethal caster transfer.
- Exact current commit: `9b12bfbcb39b0e68bf97db8118de3919a3e45ae9`;
  expanded scenario and progress docs are uncommitted.
- Exact next action: commit/publish, rebuild clean SHA, run transactional
  standalone `disposable-shield-other`, and inspect exact structured outcomes.
- Remaining gates: typed mitigation/temp HP/immunity/area/death/save-load;
  module/profile repetitions; 0.0.77 release. Active blocker: none.

## Shield Other current resume - exclusions/recursion/lethal runtime PASS

- Current phase: Phase 9 native temporary-HP and mitigation runtime coverage.
- Last completed runtime check: standalone PASS
  `20260810T1343395800417Z-disposable-shield-other` on exact
  `30098c73a7aa1820f5f889c140a55d4d376e7859`; stat exclusions, reciprocal guard,
  lethal caster, six logs, and cleanup passed. Transaction
  `compat-20260810T134303Z-8c43ad78f3c0` restoration verified `True`.
- Exact current commit: `30098c73a7aa1820f5f889c140a55d4d376e7859`;
  curated evidence docs are uncommitted.
- Exact next action: commit/publish evidence, then add native temporary-HP
  modifiers for both subject and caster and qualify their consumption semantics.
- Remaining gates: typed DR/resistance/immunity, area/death/save-load; profiles,
  modules, and 0.0.77 release. Active blocker: none.

## Shield Other current resume - native temporary-HP scenario ready

- Current phase: Phase 9 temporary-HP runtime qualification.
- Last completed check: `Build-Local.ps1` PASS with 980/980 tests, exact Release,
  and strict package after adding separate subject/caster native temporary-HP
  consumption cases.
- Exact current commit: `bbb87c2a2ef6eb1208589be6f601899f575e398e`;
  temporary-HP scenario and progress docs are uncommitted.
- Exact next action: commit/publish, rebuild clean SHA, and run transactional
  standalone `disposable-shield-other` to prove native temporary-HP semantics.
- Remaining gates: typed mitigation/immunity, area/death/save-load; profile and
  module repetitions; 0.0.77 release. Active blocker: none.

## Shield Other current resume - temporary-HP observation repair

- Current phase: qualify cached-value refresh before temporary-HP retry.
- Last runtime result: `20260810T1349330912897Z-disposable-shield-other` failed
  only stale consumption reads; HP outcomes were correct (`0/3` and `3/0`).
  Transaction `compat-20260810T134851Z-f1299dd53841` restored exactly.
- Exact current commit: `881d0c2660b549a032b5b9a62b751c80a360808c`;
  native `UpdateValue()` observation repair and evidence docs are uncommitted.
- Exact next action: validate, commit/publish, rebuild, and rerun standalone once.
- Remaining gates unchanged. Active blocker: none.

## Shield Other current resume - temporary HP PASS / module matrix repair

- Current phase: Phase 3/9 eight-combination runtime qualification.
- Last runtime PASS: `20260810T1354174801203Z-disposable-shield-other` on exact
  `5c2c5445fc64ab06149d630d6f16b8bcea83048a`; both native temporary-HP cases
  consumed 3 and transaction restoration verified.
- Exact current commit: `5c2c5445fc64ab06149d630d6f16b8bcea83048a`;
  three-setting observer/profile writer and eight-way direct matrix are uncommitted.
- Exact next action: validate/commit/publish, then run all eight module settings
  in isolated standalone profiles and capture constant identity/publication proof.
- Remaining gates: typed mitigation/immunity, area/death/save-load; profile
  repetitions and 0.0.77 release. Active blocker: none.

## Shield Other current resume - module request serializer repair

- Current phase: source qualification before eight-combination runtime matrix.
- Last attempt stopped in pure preflight because `RuntimeAutomation.Common.ps1`
  still accepted only two module parameters; transaction
  `compat-20260810T135922Z-4b9aa824e344` restored exactly and no game launched.
- Exact current commit: `486a38502b12529e53bf54ac83ca882749de5ae8`;
  authoritative validator/serializer repair and evidence docs are uncommitted.
- Exact next action: validate/commit/publish, rebuild clean SHA, retry the eight
  isolated standalone combinations. Remaining gates unchanged; blocker none.

## Shield Other current resume - runtime-side module request repair

- Current phase: final acceptance plumbing before eight-combination runtime.
- Last run `20260810T1403193768024Z-observe-feature-module-settings` was rejected
  with `module-states-required`; no observer/save action. Explicit recovery of
  transaction `compat-20260810T140244Z-f0f80ecf5cc6` verified `True`.
- Exact current commit: `77273bf7eead256bcdc24460dbe91d2e3ac9b709`;
  runtime validator and focused source test plus evidence docs are uncommitted.
- Exact next action: validate/commit/publish/rebuild and retry all-enabled.
- Remaining gates unchanged. Active blocker: none.
## Expanded Summoning current resume - transactional publication ready

- Working branch: `codex/expanded-summoning`; baseline
  `2894d9fcce250708e354894ffd8e1be9c7493b9b`.
- Latest guarded PASS:
  `20260811T1905591662821Z-observe-expanded-summoning-inventory` on
  `344c9590dc02c086da9e4d9dcd03917e6ebb741a`; 67 units, 1,045 abilities,
  1,370 registered identities, and all native spawn graphs validated.
- Additive transactional publication for the 18 canonical summon parents is
  source-qualified with 999/999 tests, clean Release, and strict package PASS.
- Exact next action: commit/push this checkpoint, rebuild from the committed
  SHA, then run `observe-expanded-summoning-inventory` to prove 681 live KMG
  placements and exact preservation of all preexisting parent variants.
- Remaining work includes alignment/template mechanics, fact-level unit
  normalization, special adaptations, optional-parent reconciliation, guarded
  mechanical/visual/persistence scenarios, profiles, release metadata, and PR.

## Expanded Summoning current resume - live publication PASS

- Pushed source `7feb6cb10049ad88c6bf4ddad148e3c5c7bd9eb0` passed guarded run
  `20260811T1912513494823Z-observe-expanded-summoning-inventory`.
- Exact live assertions: 67 units, 1,045 abilities, 1,370 registry identities,
  and 681 placements on 18 canonical parents; no save access.
- Exact next action: implement KMG-owned celestial/fiendish template mechanics
  and deterministic good/neutral/evil execution routing without optional-mod
  compile-time references.

## Expanded Summoning current resume - donor component isolation

- Every retained KMG unit component is now an independent deep clone; native
  donor component instances and mutable arrays are no longer shared.
- Full source validation, 1,000/1,000 tests, clean Release, and strict package
  validation pass.
- Exact next action: commit/push, then remove forbidden facts and spell grants
  from proxy/custom donor graphs before implementing template routing.

## Expanded Summoning current resume - fact/spell deny contract

- Prohibited summon/conjuration/teleport/planar/campaign facts and component
  grants are filtered, class spell selections are cleared, and inventory is
  forced empty on all KMG units.
- Source validation, 1,001/1,001 tests, clean Release, and strict package pass.
- Exact next action: commit/push and add authoritative final-live observer
  assertions for donor isolation and prohibited-reference absence.

## Expanded Summoning current resume - sanitizer native PASS

- Guarded run `20260811T1924349269417Z-observe-expanded-summoning-inventory`
  passed on `9b064ac2896d38b69a87777976825fbed795f6a2`.
- Exact assertions were zero shared donor components, zero prohibited
  references, zero inherited class spell arrays, and zero nonempty inventories.
- Exact next action: implement celestial/fiendish buffs and alignment routing.

## Expanded Summoning current resume - template foundation

- Four native-component KMG template buffs and 182 nested celestial/fiendish
  logical choices are source-qualified with 1,002/1,002 tests, clean Release,
  and strict package PASS.
- Exact next action: commit/push and run the guarded structural observer, then
  add exact template graph assertions and finish SR/smite/alignment fidelity.

## Expanded Summoning current resume - exact template graph observer

- The localization repair passed guarded fresh-process run
  `20260811T1937046769749Z-observe-expanded-summoning-inventory` on source
  `61a3c40e3dd5ebecd07a6ed5d19a0b16d4af51d5`; 67 units, 1,045 abilities,
  1,370 identities, 681 placements, and all sanitizer checks passed with no
  save access.
- The structural observer now requires exactly 182 logical template choices,
  182 celestial executions, 182 fiendish executions, and four native-component
  template buffs, including alignment gates, descriptors, child application,
  resistance/DR structure, and high-tier spell resistance.
- Repository validation, 1,002/1,002 tests, clean Release, and strict package
  validation pass. Exact next action: commit/push, rerun the observer on that
  immutable SHA, then finish template SR/smite/alignment fidelity.

## Expanded Summoning current resume - template graph native PASS

- Guarded fresh-process run
  `20260811T1944046651609Z-observe-expanded-summoning-inventory` passed on
  committed source `d76f5c6010ca3612ba2b1c24e076b7601fed9227`.
- Exact final-live counts passed: 182 logical template choices, 182 celestial
  executions, 182 fiendish executions, four template buffs, 681 parent
  placements, and 1,370 registered identities. No save was accessed.
- Exact next action: add frozen mid-tier celestial/fiendish buffs so creatures
  with 5-10 HD receive CR+5 SR without over-granting SR below 5 HD; then qualify
  the resulting six-buff template graph.

## Expanded Summoning current resume - exact template SR source-qualified

- Added frozen `Celestial.Mid` (`c3c53de0ca9440e5af263dfb16922188`)
  and `Fiendish.Mid` (`031bc1b958324023bf3f4c33b976185d`) buff
  identities. The append-only ledger is now 1,373 IDs: 1,372 active and one
  reserved; Expanded Summoning contributes 1,118 identities.
- The pure HD policy assigns low below 5 HD, mid from 5 through 10 HD, and high
  above 10 HD. Mid grants resistance/DR 5 plus CR+5 SR; high grants value 10
  plus CR+5 SR; low conservatively omits SR exactly below the tabletop gate.
- The aggregate bootstrap count is derived from the feature-local foundation
  count. Repository validation, 1,003/1,003 tests, clean Release, and strict
  package validation pass. Exact next action: commit/push and run the guarded
  observer against the immutable SHA.

## Expanded Summoning current resume - exact template SR native PASS

- Guarded save-free run
  `20260811T1954362756414Z-observe-expanded-summoning-inventory` passed on
  committed source `d384ba06cf76896543a6b23ed480d3f6715bbba2`.
- Exact final-live assertions passed: registry 1,372; six HD-banded buffs; 182
  logical choices; 182 celestial and 182 fiendish executions; 681 canonical
  placements; zero sanitizer failures. No save was accessed.
- Exact next action: inventory native celestial/fiendish smite and alignment
  machinery, then implement a bounded adaptation or document why it is unsafe.

## Expanded Summoning current resume - exact template-mechanic observer

- The save-free inventory now records the two exact Call of the Wild template
  features and their seven referenced smite/SR/resistance/DR facts by GUID,
  without any optional-mod compile-time dependency.
- Repository validation, 1,003/1,003 tests, clean Release, and strict package
  validation pass. Exact next action: commit/push and run the observer in the
  composed profile to capture the smite component internals.

## Expanded Summoning current resume - exact smite dependency follow-up

- Guarded run `20260811T2000068867927Z-observe-expanded-summoning-inventory`
  passed on `63d77276ea97468f6bb31768b3f4e7125a390250`; all nine exact
  Call of the Wild template-mechanic GUIDs were present and no save was used.
- Smite Evil/Good template features use base `AddFacts` and
  `AddAbilityResources`, pointing to exact abilities `f009c072...` / `320b9273...`
  and shared resource `b4274c5b...`. The observer now follows those three
  identities with full fields/action graphs. Exact next action: commit/push and
  execute that focused observation.

## Expanded Summoning current resume - deep smite graph observer

- Guarded run `20260811T2004013138253Z-observe-expanded-summoning-inventory`
  passed on `f5587d327e67a202adcca89b7961f2683c2b2a0f` and confirmed both
  optional template smites are swift, one-resource targeted abilities using
  Charisma and character-level ranks plus conditional target buffs.
- Reusing that graph directly remains unapproved because the shared resource
  maximum and applied-buff identities were still nested. The observer now emits
  depth-12 action graphs and the exact resource amount structure. Exact next
  action: commit/push and run that final dependency observation.

## Expanded Summoning current resume - bounded template smite source-qualified

- Guarded run `20260811T2008011506353Z-observe-expanded-summoning-inventory`
  passed on `15c3b7f1dc2d91269f44a0480c4ba036f83ea15b`. It proved the optional
  shared smite resource has fixed maximum one, but both abilities apply the
  same permanent non-child target buff. Direct reuse was rejected because that
  external state could survive its summoned source.
- KMG now registers two summon-local smite markers. Every templated execution
  receives the appropriate marker as a permanent, non-dispellable child of the
  summoned unit. The marker grants nonnegative Charisma to attack and HD to
  damage against the opposed alignment, then consumes itself after the first
  successful eligible hit; it never creates target state.
- The deterministic ledger is now 1,375 IDs: 1,374 active and one reserved;
  Expanded Summoning contributes 1,120 identities. Repository validation,
  1,004/1,004 tests, clean Release, and strict package validation pass. Exact
  next action: commit/push, rebuild the immutable SHA, and run the guarded
  final-live structural observer.

## Expanded Summoning current resume - bounded template smite native PASS

- The first guarded launch on `a0d8e7752281d4ae1e51ce094c1226f6a30faf16`
  accepted the exact request but exited during platform initialization before
  blueprint inspection; it accessed no save. The next fresh Steam launch
  passed as run `20260811T2021406071785Z-observe-expanded-summoning-inventory`.
- Exact final-live assertions passed: registry 1,374; 67 units; 1,045
  abilities; 681 placements; 182 celestial and 182 fiendish executions; six
  template buffs; two bounded smite markers; zero sanitizer violations. No
  save was accessed.
- Exact next action: implement save-safe summon-child alignment assignment for
  celestial, fiendish, and Nature's Ally units, then source- and runtime-qualify
  it before special creature mechanics.

## Expanded Summoning current resume - spawn-local alignment source-qualified

- Added a native post-spawn context action that mutates only the new unit
  descriptor, never its shared KMG unit blueprint. Celestial and fiendish
  summons preserve the unit's law/chaos axis and replace its moral axis;
  Nature's Ally summons copy the caster's exact alignment and fail closed if
  the caster context is absent or malformed.
- All 182 celestial executions, 182 fiendish executions, and 320 Nature's Ally
  placements carry the family-correct action. Because `UnitAlignment.Set`
  writes the unit descriptor, the assigned value uses the engine's ordinary
  serialized state and summon cleanup requires no external marker or target
  state.
- Repository validation, 1,005/1,005 tests, clean Release, and strict package
  validation pass. Exact next action: commit/push, rebuild the immutable SHA,
  and run the guarded final-live structural observer.

## Expanded Summoning current resume - ActionList isolation repair

- Guarded run `20260811T2031332882968Z-observe-expanded-summoning-inventory`
  on `6fd61f2e0e600f689efe7ef6e88495dfe7cd0f37` failed the new exact
  alignment assertions while all roster, registry, placement, and sanitizer
  assertions passed. Retained graphs showed 1-4 duplicated alignment actions
  across abilities sharing a native quantity template; no save was accessed.
- Initial evidence implicated the `ActionList` container and its `GameAction[]`;
  an explicit recursive list clone was added. The next run narrowed the deeper
  alias to the ScriptableObject action elements themselves.
- The observer now requires exactly one family-correct action on every KMG
  execution and zero KMG actions or buffs on every non-KMG ability. Repository
  validation, 1,005/1,005 tests, clean Release, and strict package pass. Exact
  next action: commit/push and rerun the guarded observer on the immutable SHA.

## Expanded Summoning current resume - GameAction isolation follow-up

- Guarded run `20260811T2037058339804Z-observe-expanded-summoning-inventory`
  on `ed7d6a2b41b951166ed0a243fb30e7531b5dd6d0` retained 286 contaminated
  non-KMG abilities, proving the first `ActionList`-only repair was incomplete.
  No save was accessed.
- Exact assembly inspection showed `GameAction` derives from
  `SerializedScriptableObject`. The clone's Unity-object guard therefore
  returned each spawn action by reference before reaching its newly cloned
  `ActionList`. The repair now creates independent `GameAction` ScriptableObjects
  and recursively clones their fields while preserving unrelated Unity assets.
- Repository validation, 1,005/1,005 tests, clean Release, and strict package
  validation pass. Exact next action: commit/push and rerun the exact alignment
  and zero-native-contamination observer.

## Expanded Summoning current resume - per-spawn-branch observer correction

- Guarded run `20260811T2043115519772Z-observe-expanded-summoning-inventory`
  on `0759783da38db3843905f368c62a964217870545` proved the GameAction repair:
  native/third-party contamination fell from 286 to zero, and representative
  KMG graphs held one independent alignment action. No save was accessed.
- The remaining failures were observer overconstraint. Some native quantity
  templates legitimately contain multiple `ContextActionSpawnMonster` nodes;
  requiring exactly one added action per ability rejects those valid graphs.
  The observer now counts spawn nodes and requires exactly one alignment,
  template, and smite action per corresponding spawn branch.
- Repository validation, 1,005/1,005 tests, clean Release, and strict package
  validation pass. Exact next action: commit/push and rerun the guarded observer.

## Expanded Summoning current resume - spawn-local alignment native PASS

- Guarded save-free run
  `20260811T2048003275107Z-observe-expanded-summoning-inventory` passed on
  committed source `b88e99cffb7464d7354416fba82d1da313e17ae2`.
- Every spawn branch across 182 celestial executions, 182 fiendish executions,
  and 320 Nature's Ally placements had exactly its required alignment action;
  each templated branch also had exactly one template and smite application.
  Non-KMG action contamination was exactly zero. Registry 1,374, 681 parent
  placements, and all sanitizer invariants remained exact.
- Exact next action: inventory and implement required special creature
  adaptations in bounded groups, starting with Lantern Archon, mephits, and
  elementals against final-live native components.

## Expanded Summoning current resume - complete frozen donor observer

- Expanded the prior 25-GUID sample to all 54 distinct chosen donor GUIDs
  derived directly from the frozen donor catalog. The observer now records
  shallow unit fields plus bounded component, body, and view object graphs and
  fails if any chosen donor is absent.
- Repository validation, 1,005/1,005 tests, clean Release, and strict package
  validation pass. Exact next action: commit/push, run the guarded save-free
  observer, and use its exact final-live graphs to partition native reuse from
  required proxy/special reconstruction.

## Expanded Summoning current resume - complete donor graph native PASS

- Guarded save-free run
  `20260811T2055502086857Z-observe-expanded-summoning-inventory` passed on
  committed source `9e1d851e75cf413f5d0a576484a9f5a8538b2a2b`.
- All 54 distinct frozen donor GUIDs were present. Their bounded component,
  body, and view graphs were captured; all 17 assertions passed with zero
  warnings or exception. Registry 1,374 and all 681 placements remained exact.
- The wrapper reached its 120-second host timeout after the game had already
  flushed a PASS result and exited; the scenario itself completed in 103,225
  ms. No save was selected, loaded, or written.
- Exact next action: classify native reuse and implement the first bounded
  special-mechanic group: elementals, mephits, and Lantern Archon.

## Expanded Summoning current resume - Lantern candidate observer

- The primary Lantern Archon stat block requires a CR 2 Small lawful-good
  outsider with two light rays and archon defenses. The selected Ghaele summon
  donor is not an acceptable mechanical or visual implementation.
- Added a bounded final-live probe for Will-o'-Wisp, archon, light-ray, and aura
  candidates. It changes no gameplay and will freeze exact native identities
  before the Lantern unit is reconstructed.
- Repository validation, 1,005/1,005 tests, clean Release, and strict package
  validation pass. Exact next action: commit/push and run the guarded save-free
  candidate observation.

## Expanded Summoning current resume - first special mechanics source-qualified

- Candidate run `20260811T2102583998517Z-observe-expanded-summoning-inventory`
  passed on `8c576bc2ff741f726493ce347de16f02c7ee02de` and selected the
  exact Will-o'-Wisp view, two-projectile native ray, and Aura of Menace carrier.
- Added immutable native-reuse profiles for all 24 elementals and four mephits.
  Reconstructed Lantern Archon as 2-HD Small lawful-good outsider with the
  official ability scores, bounded dual 1d6/30-foot touch rays, single-ray AI,
  archon defenses, and native aura. Teleport and gestalt are absent.
- Four new active identities are frozen. Ledger: 1,124 feature identities;
  1,378 active plus one reserved; constant registry count 1,378.
- Repository validation, 1,006/1,006 tests, clean Release, and strict package
  validation pass. Exact next action: commit/push, rebuild the immutable SHA,
  and run the exact guarded Lantern structural observer.
