# PR 67 review follow-up

Completion milestone in progress, 2026-10-22. PR stays draft; no merge/release or
owner aesthetic approval is claimed. The completion intake was clean
`48a6781392c3d5990c49d9a0e27363e4e3372740` on the existing integration branch.

## Completion implementation (current)

### Review39 results and review40 corrections (current, October 23)

Frozen review39 source `3797bf13b4c1c7633a22bb925cf7551e45a95275`, fingerprint
`acead11b64f7361fe50bf6ed4edf426474c7597a2630510f2693666948ce7ae4`; package
`7fc6e5a14bfdc835228f1287638086ac1cb5b4f091567fb057db585f5b267f05`, DLL
`5238afacc3e29a6a94b379fa6e4482f8fa6330414a50b8749a77ec62c2fcf7ec`, MVID
`b7e69f2e-c4db-440d-a145-9598ccaf0ec4`. One build (183.6s): 23 selected cases,
development gate 9/0 and 36/36 installed targets pass. Cold scripts are byte-
identical (apart from CRLF) to the review37 590/0 fixture qualification.

Window19 began 11:06:47 UTC with the **first live cold trio**:

- Cold transaction `cleanup2-cold-20261023T110658Z-eceec86534a0`,
  **FAILED_RESTORED, 307.6s**: Working, settings and installation restored and
  verified, no recovery debt, no restoration error. Prepare **22/0**, raw
  `20261023T1108141157162Z`. Verify-cleanup **19/2**, raw
  `20261023T1110193647094Z`. The fresh-process reconstruction itself passes:
  Ettin authored-head cold verify (exact two-headed full-body attachment, both
  private chains in the native palette, 14,082 vertices/61 bones, native
  controller adoption and source-cached fade defaults, borrowed textures
  unchanged, no observer attach), Cloud/Storm production render cold verify
  (2.1/1.8 factors, frozen-cape cloth ownership), equipment/resources, readiness
  and profile views. Cleanup fails: units and legacy resources are gone, but four
  rows are `UNRESOLVED`: both palette material instances (destroyed) and both
  `KMG_GiantPalette_*_Albedo` textures (**still alive**). Product leak: the
  view-teardown `Release` keeps the albedo while another component's material
  instance still references it, and nothing retries. `exact-working-save-write`
  fails as a consequence (no save after a failed fixture assertion). Verify-absent
  was correctly not run.
- PC learning **PASS 17/0, 168.5s**, raw `20261023T1117331593427Z`. Native
  preparation and learning folds install (`preparation:native-source-fold;rows=1`,
  `learning:native-source-fold;rows=1`); cancel's producer settled in one frame.
  Root inspected all three PNGs: the preparation header is readable, but the
  **learning header is a 160px right-aligned box in a decorative font, wrapped
  and clipped**. The level-up spellbook preview collapses its single elemental
  row behind "Elementals (1) — show original sources" (owner-review item).
- Druid menus **FAIL 61/2, 219.1s**, raw `20261023T1122460179498Z`. Under CotW the
  native-construction projection now works (`quantities=2;children=21`, exact
  child -> quantity -> preparation chains, native membership, Fact/metamagic),
  but the popup rows stayed parent-only: native `OnToggleGroupClick` fills the
  popup from the slot's `Conversion` list, built by `SetToggleAdditionalSpells`
  (or CotW's replacing prefix), never from `GetConvertedAbilityData`.
- Controller **FAIL 26/2, 184.3s**, raw `20261023T1129466055804Z`. Shared screen
  records fix the capture collision; native preparation, prepared-slot focus
  and native unprepare pass. Root inspected the controller book: "Elementals —
  HIDE SOURCES" header, original source row and native tooltip are readable.
  The disposed-preparation check then ran after a fixed 8 frames (0.22s here)
  while the native close had not yet unbound its pooled rows.

Window19 closed 11:31:27 UTC (24m40s); every transaction restored.

Review40 corrections:

- Giant palette: view teardown retires a still-referenced private albedo through
  a janitor that destroys it only once no live material binds it (bounded 10s;
  a texture still bound after that is left alone). New read-only
  `GiantSummonPaletteRuntime.CaptureOwnedResources` seam, used by the roster
  cleanup observer beside the Ettin/primate/serpent seams.
- PC learning header copies the exact native row's anchors/width and uses the
  row title's font.
- Druid: the same projection now also applies in place to the PC popup's own
  `Conversion` list (`ActionBarGroupSlot.SetToggleAdditionalSpells` 060044B3,
  468 bytes, after native or CotW construction). The expected child set is cached
  per registry because this runs on every spell-tab fill. Installed targets 37/37.
- Controller disposed-preparation check polls the same observations for the
  native unbind (bounded 600 frames/8s); nothing is released by the observer.

### Review38 results and review39 corrections (October 23)

Frozen review38 source `7d5569ac306740d9164e4d28eb2f33ffcc228569`, fingerprint
`c10cbb963a34028c22edfabefe22939936b5686fb8811917251a8b917eee3ad8`; package
`cdea670107812431ac7bf33172ec5f4f70a8adfaeb71b2f469ee7a0ad51a74a7`, DLL
`bceb1c6da64937cd7843ac848ec7dee80b30228db2ca27a0c9e5bf16479c079f`, MVID
`92108a2e-4bcd-4199-9f8d-9d20bde34f4c`. One build (239.0s): 23 selected cases,
development gate 9/0 and 37/37 installed targets pass.

Window18 ran 10:39:30 to 11:00:39 UTC (21m09s); all four transactions restored.

- PC learning **FAIL 16/3, 192.8s**, raw `20261023T1040445689037Z`. The corrected
  pin installs the native PC preparation fold: disclosure, original-source
  preparation, undo, native close, learning disclosure and native allocation
  all **PASS**. Cancel commits nothing and keeps Wizard2, known objects and
  resources exact, but the intake receipt refused `nativeProducerReadLockAcquired`:
  the native `LevelUpPreviewThread` still held its own lock in the same frame
  as the owned `Cancel()`, finishing that backend's last queued preview.
- Menu adapters with alternate sources **FAIL 61/2, 216.6s**, raw
  `20261023T1045186727113Z`. The HUD settle works: all twelve SM/SNA Horse
  quantity routes and the native pin pass. Druid book lookup now passes. The
  projection then built nothing: Call of the Wild's prefix 06000B31 replaces the
  spontaneous factory body (returns false), so the transpiled native body never
  ran (`reads=0;created=0`). With CotW (the owner's actual stack) the factory
  route cannot reach native construction.
- Controller **FAIL 25/1, 174.6s**, raw `20261023T1053392097967Z`. Real native
  preparation and the formerly crashing prepared-slot focus pass. A second
  capture session in the same request reused `native-ui-000.png`.
- Creature commands **PASS 32/0, 205.5s**, raw `20261023T1058365721589Z`. The
  ranked camera selected candidate 9 (pitch 60, side -1). Root inspected all six
  Brachiosaurus PNGs: whole body, neck, legs, tail, contact and both targets are
  visible from contact through recovery; the tail tip passes behind the poplar
  (native blue silhouette) only in ready/approach. Triangle sight survey still
  reports UNKNOWN (`environmentSightProven=false`); the pictures are the evidence.
  Owner-review note: the torso still reads as a broad rounded dome and the tail
  is short for a sauropod; the neck/chest transition is no longer abrupt.

Review39 corrections:

- Druid projection makes each child with the same two native calls as the
  installed factory body (native constructor 06002B3A, then the unchanged native
  `ConvertedFrom` setter 06002B27 on the retained quantity conversion), with the
  scoped `HasVariant` membership validating and inheriting Fact/metamagic. The
  spontaneous-factory transpiler is removed; no field is hand-assigned.
- PC and controller cancel checks poll the same non-blocking producer reads for
  natural completion (bounded 600 frames/8s); the producer is never stopped.
- `NativeIconScreenEvidence` sessions in one request share one record list, so
  file indices stay unique and `native-ui-screens.json` keeps every capture.

### Review37 results and review38 corrections (October 23)

Frozen review37 source `12af3f45a44b7a74deb46c700c715e5cec3ee03f`, fingerprint
`43893147b1e9b0ec0c2829c2739fdc69a9315a226152ac591bbdbbde7c7ec8ab`; package
`84c1e9d66af38d201550990c81cb1fd5a0145ac643026d3abb69e98a448a6346`, DLL
`98ea5c35b0133f758e81252799e3cfd8bcee12e05ba52e67d64df40de59cda6f`, MVID
`bcdb878d-da40-41ef-a9da-b11ae59ed5dd`. One build (184.4s): compilation, package,
23 selected cases and 24/24 installed targets pass. The unchanged cold transaction
fixtures were rerun on these exact CRLF bytes: **590/0, 70.4s**,
`artifacts/cleanup2-currentstack-tests/c4ea5f7e636446cea5cbd25a13d1f0d0` (no game).

Window17 ran 10:15:52 to 10:28:47 UTC (12m55s); both transactions restored.

- Menu adapters with alternate sources **FAIL 11/2, 240.1s**, raw
  `20261023T1017349307368Z`. It stopped at the first canonical SM II Horse popup,
  before any Druid route. Cause is harness timing, not product: this session
  now renders 1920x1200 (review36: 1920x1080) at about 36 fps, so the fixed
  16-frame settle lasted 0.44s instead of 1.01s. The native HUD `UpperPart`
  `MoveAnimator` (unscaled DOTween) was still sliding; the popup snapshot source
  rect was 46.7px below its live rect and admission correctly refused.
- PC learning **FAIL 11/2, 187.0s**, raw `20261023T1027154797158Z`. The review37
  singleton-book and native close-tween corrections pass natively
  (`native-book-selection`, `native-book-close`). The disclosure then reports
  `not-used`. **Product defect:** `ElementalPreparationRowsPatch.Prepare` pinned
  `SpellBookView.RefreshSpellsView` at 730 IL bytes; installed 06004838 is 731,
  so Harmony never installed the PC preparation fold. No earlier native run ever
  recorded a fold outcome. The installed-target checker did not cover it.

Review38 corrections:

- Pin corrected to 731. The installed-target checker now also covers the three
  row-fold patches, the desktop chooser, Ettin, Pteranodon/Wasp, giant and
  gesture patches. Against the review37 DLL it fails exactly
  `ElementalPreparationRowsPatch` (36/37); against the corrected DLL 37/37.
- Every Cleanup2 action-bar popup opening (menus, tooltips, adapters, Druid,
  alternate, OFF, Favorite source) first waits for the actual source rect to stay
  unchanged for six frames and 0.3s unscaled (bounded 600 frames/4s), failing
  closed. The settle record is attached to each popup admission. No UI state is
  assigned and the snapshot/live admission predicate is unchanged.

Controller preparation, Brachiosaurus camera and cold trio were deliberately not
spent on review37 bytes; they run on review38.

### Review36 recovery state (October 23)
Owner requested a Claude handoff after session errors. See
[CLEANUP2-CLAUDE-HANDOFF-2026-10-23.md](CLEANUP2-CLAUDE-HANDOFF-2026-10-23.md)
for the frozen/dirty source boundary and exact next actions. Last review36
native giants-off is **11/0 PASS,70.612s**, raw20261023T0940428372896Z.
Cloud/Storm OFF views and release pass. Exact restoration completed
09:42:09.5796206UTC; window16 is closed at12m59.580s. No game remains running.
The final90-minute window has not started. Current next-batch C# changes remain
uncompiled/uncommitted; this handoff is not mission completion.

Frozen source `ebe1bce4160e06972a930d8f679aaae136cfefe7`; package
`af9722f2304bd6ceba9b28694e375723bdbab1d2ff2ed4c717538fc3a90cceb1`, DLL
`beec19a1863265ad405217bf8bc43e0a9974bf8f908ad154aef5dcaee557ab28`, MVID
`c42a6cbe-7436-4b08-8320-f76cfc746923`. Compilation/package,23 selected cases
(1.6s) and23 installed patch targets pass. PR remains draft.

- Creature commands **32/0 PASS,121.685s**, raw `20261023T0544037260256Z`.
  Both current bodies reach actual native approach/attack/recovery with unchanged
  rules/costs and exact native tick deadlines. Root inspected Crab motion
  positively. Brachiosaurus images are tree-occluded: mechanical PASS does not
  qualify its appearance. Existing camera positions are being reprioritized.
- Druid menus **61/2 FAIL,120.464s**, raw `20261023T0549228037890Z`.
  Exact native quantity arrays now match. A class-data lookup rejects the owned
  demanded book before projection; native variant validation also needs exact
  per-invocation membership. The correction preserves native constructed source
  objects and setter inheritance, without reparenting shared blueprints.
- Cold preflight refuses before game/install/save-byte mutation: actual Working
  attributes8224 include NotContentIndexed8192. Narrow support preserves exact
  attributes; disposable three-phase/restoration fixtures now **590/0,49.900s**,
  `artifacts/cleanup2-currentstack-tests/8fccf5b41631421eb2815e5fc27ffd5f`.
- PC learning fails at an active class-tab assumption. Native singleton books
  hide that strip. Cleanup must await the native0.4s close tween before testing
  book-hidden. Raw `20261023T0930489547115Z`,75.104s; exact exception retained.
- Controller **24/1 FAIL,106.863s**, raw `20261023T0934278557553Z`.
  Native source preparation succeeds, then an unbound pooled view causes the
  observer lookup to throw. Exact-slot lookup now skips null view models.

All completed native transactions above confirm exit and restore fresh95-save
inventories and exact installation/settings. Window15 ended05:51:42.7803730UTC
(20m28.780s from conservative05:31:14UTC); request/session interruptions did not
leave a game running. Resumed window16 starts09:29:10UTC, ceiling30minutes;
final90 has not started. Current gallery has100 verified originals; new native
results are not relabelled as earlier candidates. Current integration source
corrections require coherent compilation/freeze before their affected runs.


### Review35 complete body packets and exact remaining observer failures

Frozen source `fc2b00676129460a2635356ce0a3cd39fb6f1c4a`, package
`df34d09c8763e27accaf8267b2282b561e5c9adc2c919cb5f0a31b10c73c1cef`, DLL
`b7c5ee470c55c752f072f7e25828f25af910f607e1d644b1fbf6100e031efcc7`.
Results below retain these exact bytes; review36 changes are not native PASS.

| Phase / raw evidence prefix | Result | Verified boundary |
| --- | --- | --- |
| Creature commands `20261023T0509118423366Z` | 16/2 FAIL;90.456s | Crab native command/rules/cost,2.999315m travel, gesture samples and recovered source state pass. Forty native recovery frames record1.5999999999985448 actual/rendered seconds, just below the strict1.6 requirement through subtraction cancellation. Frame-wait wall8.0978145s also exceeds8s. No relaxed recovery PASS; Brachiosaurus is unreached. |
| Menu adapters `20261023T0512231430715Z` | 58/2 FAIL;111.660s | Prepared/known SM/SNA Horse quantity routes and original pin/UI restoration pass. Actual Druid feature `b296531ffe013c8499ad712f8ae97f6b` has three exact native Single/d3/d4plus1 conversion arrays: only rankI uses a canonical parent, ranksII-IX use native quantity leaves. Parent-array detection finds zero candidates and stops before any grant/cast. |
| Girallon/Stegosaurus `20261023T0516203937056Z` | **31/0 PASS;125.230s** | Both ordinary native attacks and full117-frame packets pass (9 stills+108 tracking each). Travel is4.13248873m Girallon and5.96945429m Stegosaurus. Root inspected six new Girallon originals; current local gallery now contains98 originals. Packet completeness does not claim every frame unobstructed or owner art approval. |
| PC learning `20261023T0520484720745Z` | 8/2 FAIL;66.926s | Intake, original party/selection/sources/pins and retained completed-controller fields are recorded exact. A restoration finalizer throws and masks the primary failure; its cause remains unknown. No learning allocation/preparation qualification is inferred. |
| Fresh OFF,05:23UTC | Pre-native refusal | The staged `FeatureModules.json` file is absent; no native fixture ran. The original human FeatureModules file existed (435bytes, schema12, allON, Oct15 modification time) and was restored. Missing-defaults transaction handling is staged; disposable fixtures also cover original absence, which was not this live intake. |

All selected review35 transactions restored95 original saves and exact current
installation/settings. Last verified restoration is
**2026-10-23T05:23:58.1234634Z**. Window14 is closed at **19m45.123s** from
05:04:13UTC; final90-minute qualification has not started. Raw native directories
use the table prefixes under `C:/Dev/KingmakerGunslingerLab/runtime-evidence/`
with suffix `-disposable-expanded-summoning-cleanup2-native-review`.

Staged review36 is bounded to the demonstrated failures and remaining surfaces:

- Compare the exact native recovery tick deadline, retaining actual/rendered
  recovery and wall/frame bounds; no command timing or production rules change.
- Match the exact Druid feature/class/quantity arrays and published provenance.
  The installed factory produces child -> original quantity conversion -> actual
  preparation links through an exact instance-scoped variant read; no source
  fields, shared assets or `AbilityData` are authored. Unknown rows remain native.
- Refresh stale duplicate-slot debit tracking and retain the PC learning primary
  exception with individual restoration diagnostics.
- Use the actual native Wizard-extra controller learning setup; qualify the
  existing native navigation/callbacks rather than a headless substitute.
- Handle absent staged OFF defaults transactionally, with focused disposable
  positive/negative fixtures for both existing and absent original settings.
- Observe legitimate existing tier4 controller Horse-quantity, Elemental and
  Mephit labels through native converted-row hovers. Giant controller coverage
  stays explicit: the level8 fixture does not own the required tier6 access.

Known-spontaneous source, PC/controller allocation/preparation, alternate menus,
actual OFF, Brachiosaurus visible contact and protected cold execution remain
open. Review34 giant76/0 and pin23/0 retain their original identity; no unchanged
campaign or owner approval is implied.

### Review34 production giants, visible Crab motion and original source pins

The then-next review35 batch changed four bounded observations: recovery wait
excludes its own PNG I/O while retaining native/rendered timing; Girallon may
use a shorter supported retrace with the same actor/graph guards; controller
preparation follows actual native root/branch/leaf navigation; Druid admission
records its exact current direct progression components before any grant.
No production model, timing or source rule is changed by this batch.

Frozen `2ddff743264a0abd6a3f5267951c48e96e6963db`, source fingerprint
`811fbeee0ae2d222b05f72e4807b5f1a269b63417ec75991344fcabdc579bc5d`, package
`c706c3f65906884c1080804ec76e3e281faedbe07ac8d5a577837efa504827ba`, DLL
`d6be217f354926828ec14527f845ca9783dc7270df14ede58c22bc8f305e240b`, MVID
`e160f181-07e0-4b82-aae3-786cc4b37012`. Exact compilation, strict package,
20 selected cases(1.6s),22 installed Harmony targets and26 OFF-scope cases pass.

| Phase / raw evidence prefix | Result | Actual inspection and remaining boundary |
| --- | --- | --- |
| Creature commands `20261023T0432343826891Z` | 16/2 FAIL;90.378s | Root inspected all six full-body Crab originals: native approach, both claws, target, extension and folded recovery are visible. Rules/cost, travel, owned gesture and recovery state pass. PNG I/O exhausted the8s observer wait before its required1.6s rendered recovery sample. The correction excludes its own I/O from that wait while preserving exact native/rendered time,20s total and finite frame bounds. Brachiosaurus remains unreached. |
| Giant lineup `20261023T0435348017652Z` | **76/0 PASS;161.026s** | Whole production six-family lineup inspected: Storm largest/Hill smallest, distinct palettes. In-view cape motion, weapons/clothing, exact native root/collider/cache ownership, highlighting/visibility, disjoint instances and release all pass. Fresh OFF and cold remain unrun; owner review pending. |
| Menu adapters `20261023T0439460340729Z` | 58/1 FAIL;109.519s | Twelve prepared/known SM/SNA Horse quantity routes, native pin and all UI restorations pass. Root inspected000/003: the approved brown full-body painting is present in both families. The earlier beige-bust identification was a different tile, not a demonstrated Horse art defect; CPU coordinate mapping remains an observer limitation. Druid stops before grant at exact conversion-feature lookup; bounded actual progression/component intake is added. |
| Focused pin `20261023T0443432873140Z` | **23/0 PASS;81.528s** | Native original pin rebuild, owning-graph serialization/hydration, source-chain retention, actual pinned-child cast/debit and exact restoration pass. This is native in-memory graph hydration, not a cold Working-save claim. |
| Girallon/Stegosaurus `20261023T0446341568618Z` | 28/3 FAIL;113.663s | Both ordinary attacks/rules/costs pass. Stegosaurus completes5.946m/full117-frame packet. Root inspected joined hands, side bodies and motion samples. Some later Stegosaurus frames contain tree occlusion; no all-frame visibility claim. Girallon travels3.671m, then its return passes.033m too close to a moving party actor. The strict guard is retained; bounded shorter native-supported retraces are staged. Optional action flags were unreached, not proof of absent actions. |
| Controller `20261023T0450249389618Z` | 21/1 FAIL;83.563s | Explicit SOURCE Confirm stores the original native converted child, restores its pin and spends nothing; normal Use/cast/debit and UI restoration pass. Learning admission now succeeds. Preparation refuses its original-row focus assertion; exact native navigation diagnosis is pending. |

All six transactions restored fresh95-save inventories and exact installation/
settings after confirmed exit. Compatibility receipts, in table order:
`compat-20261023T043210Z-7caf73ba7406`, `compat-20261023T043512Z-42cf7d9f274c`,
`compat-20261023T043912Z-dfb7ea56a45d`, `compat-20261023T044319Z-1416781e8f12`,
`compat-20261023T044611Z-6db9359445c7`, `compat-20261023T045001Z-fd5ec01701db`.
Last verification **04:52:02.0962708UTC**; development window13 is closed at
**26m14.096s** from04:25:48UTC. Final90 has not started. A read-only cold wrapper
`-WhatIf` attempt stopped in inherited hash preflight before any transaction;
it is not cold execution or evidence of a save/process failure. The qualified
cold transaction itself remains unrun. Existing failures retain their identities.

### Review33 actual production giant lineup and remaining corrections

Frozen `4c10688600cd4aab198d2603bc641cb812a3cc43`, package
`b7add19581526b3460b39fa9ca56b94dd7bed87906dfcb2359b9f03f20b841d4`, DLL
`64c015292bfec661f9ce69e2d355394fdc5c9e7f60846339b14e239809fd6588`, MVID
`4ab869c4-f328-422b-8378-26c91527359e`. Exact production compilation,
20 focused cases (1.6s) and22 installed Harmony targets pass.

The integrator inspected the production six-family lineup: all heads, bodies
and weapons fit; Storm is largest and Hill smallest. Palette, private scale
ownership, native roots/colliders, visibility and highlight checks pass. The
existing gallery contains79 verified originals, with that lineup and the
explicitly FAILED Crab diagnostic first. No owner acceptance is inferred.

| Phase / raw evidence prefix | Result | Demonstrated boundary and correction |
| --- | --- | --- |
| Creature commands `20261023T0350574821093Z` | 14 PASS/3 FAIL;62.989s | Whole Crab and target visible; no attack issued. Bounded rays identify low claw/leg points below the same sloping ground supporting the actor. New observation records measured submerged contact UNKNOWN while retaining full framing and all above-ground blockers. Motion/contact still requires inspection. |
| Menu adapters `20261023T0358217054900Z` | 13 PASS/4 FAIL;98.544s | Three SM Horse quantity rows retain exact source/label/approved CPU texture pixels. Actual PNG still shows beige bust; bounded layer/capture-frame diagnosis added. Native incidental hover retains ContextTooltipData; teardown now closes only an exact owned standard trigger/source, preserving foreign refusal. Druid/alternate routes unreached. |
| Giant lineup `20261023T0401277325850Z` | 73 PASS/2 FAIL;102.927s | Clear production scale lineup inspected. Storm bones move but cape displacement is zero before camera positioning; move observation under the existing held camera with actual visibility/clock/driver evidence. Scaled idle ranges normalize to.0288-.0393 within the unchanged.05 native envelope; require exact owned factor and retain raw measurements. |
| Focused pin `20261023T0404475780691Z` | 17 PASS/3 FAIL;75.020s | Favorite exception and original pinned-child rebuild pass. Hydration rejects a retained AutoCommit controller whose Preview aliases its destroyed Unit. Native constructor explicitly uses this alias; corrected admission requires its exact destroyed/viewless/world-absent state and unchanged context/idle producer. Console/learning repetition of this known guard was skipped. |
| Girallon/Stegosaurus `20261023T0408562371083Z` | 26 PASS/5 FAIL;87.482s | Both ordinary native attacks/rules/costs pass. Art placement omitted its separate caster whenever a native party existed; actual stills show overlap and all walking lanes correctly refuse. Placement now always includes the exact caster. No path/footprint guard is weakened; overlapped stills are not art acceptance. |

All five games exited and restored their fresh95-original save inventories,
installation and settings exactly. Compatibility receipts in table order:
`compat-20261023T035033Z-b9ef36a33123`,
`compat-20261023T035753Z-3fccbf0438b9`,
`compat-20261023T040101Z-07324a7aea78`,
`compat-20261023T040421Z-63a02dd23bf8`,
`compat-20261023T040833Z-e737c51fe7c7`. Last verification
**04:10:37.4010020UTC**, window12 **25m37.401s** from03:45:00UTC.
Final90-minute completion qualification has not started. The next coherent
input also adds a real known-spontaneous Elemental cast/debit case, exact ON
source-node native pinning, and reuses existing learning/controller phases under
fresh OFF. Coherent exact-reference compilation passes (04:25:48-04:25:59UTC).
Twenty-six focused OFF scope and early-wrapper refusal cases pass without any
package/game/save access. The initial direct shell attempt stopped at execution
policy; the explicit process Bypass rerun passes. Native execution is pending;
all original FAILs remain. Development window13 starts04:25:48UTC, ceiling30m.

### Review32 readable console sources and narrowed native failures

Frozen `e5b35198e8797ff12f32536c0c828b6e7e2c7de0`, package
`2b2e9682ad379b71eb32a03bfe8bd6316af55176840ec8a5cf2b5a2064830468`, DLL
`e3c5ef3f4c46f35011b99249f4e6328a1335fd9664d855005eca22c2efb97ac1`, MVID
`9e6c007c-b308-43a4-8fab-1e556373fb0c`. Exact compilation,20 focused cases
(1.6s),22 installed Harmony targets and four exact pin-phase admission/refusal
cases pass. Initial direct PowerShell scope-check invocation hit execution
policy before tests; the repository-standard Bypass invocation passes.

The integrator inspected both actual console PNGs: book/level/preparation and
`Hide sources` are now readable. Original source Use/cast/debit, native book
field clearance and complete owned UI restoration pass. The local existing
gallery now contains77 verified originals, including these two and their two
review31 predecessors; candidate identities and failed-run status are retained.

| Phase / raw evidence prefix | Result | Precise remaining cause / correction |
| --- | --- | --- |
| Giant lineup `20261023T0326408457665Z` | 20 PASS/4 FAIL;96.019s | Appearance succeeds after139 normal frames. SNA/SM Frost acquired production scale only one frame after spawn; first ownership refusal is native hierarchy/model refresh with cache changes, source materials preserved. Defer first acquisition until exact native appearance/material/cache readiness, without combat-permission gates or clock/opacity writes; retain first mismatch identities. Observer now stops before touching released Cloth instead of producing a secondary null exception. No production lineup image or height qualification. |
| Creature commands `20261023T0329485513657Z` | 15 PASS/2 FAIL;78.080s | Actual flat Oleg lane/path/placement succeeds. The same native scenery collider blocks20–22 of116 unique Crab surface points in every tried angle; target rays are clear. No command or image reached. Next run retains one explicitly FAILED diagnostic frame and bounded project-body hit points, then aborts before attack; no guessed new scene or obstruction exemption. |
| Focused pin `20261023T0333100979116Z` | 11 PASS/3 FAIL;77.924s | Rank2-only setup removes its last ordinary source when exercising Favorite. Production correctly preserves only the explicit special-source row; expecting an ordinary grouping launcher is wrong. Preserve the original six-rank fixture setup while still skipping24 casts, extra book/SNA/Extend. Hydration remains unreached. |
| Controller `20261023T0336409655040Z` | 13 PASS/1 FAIL;81.646s | Visible source labels and prior native closure/restoration defect are fixed. Learning then refuses its old global-level-up-null assumption. Preserve exact completed intake and positively owned console backend/service rather than clearing retained native state. Learning remains unqualified. |

Each game exited and its actual fresh95 original saves, installation and settings
restored exactly. Compatibility receipts in table order:
`compat-20261023T032612Z-7f24fff40f68`,
`compat-20261023T032920Z-d968bd855f89`,
`compat-20261023T033244Z-30f40efbb23e`,
`compat-20261023T033617Z-5eff8c8f0284`. Final verification
**03:38:18.5496847UTC** closes window11 at17m18.550s from03:21:00UTC.
Final90-minute completion qualification has not started.

The planned menu-adapter32 run was not launched: bounded source review found the
same observer-owned post-lease Pause in PC learning, PC/controller alternate
menus and OFF. Those four requests are removed with strict foreign-mode guards
retained. Their corrected execution, effective Horse pixels, learning, OFF,
production giant lifecycle and exact live cold sequence remain open.

### Review31 exact failures and review32 corrections

Frozen `69643385199d89c2d4225bd16a26e6040f345140`, package
`b1354053998e40bd01f2c8d218668e5bcd03a28df780b4736e3f5923da313029`, DLL
`fd2ab3a5b1ea658c027487772a4ba913783edf7d1b35124346534a74be6592ea`, MVID
`ad6e1f1f-47ec-4a15-a345-7f4907c25c73`. Exact compilation,20 focused cases
and22 installed Harmony targets pass. Original FAIL records remain unchanged.

| Phase / raw evidence prefix | Actual result | Demonstrated cause and next bounded correction |
| --- | --- | --- |
| Giant lineup `20261023T0251584889370Z` | 13 PASS/2 FAIL;82.641s | Native three-second appearance advances1.289 Unity seconds in12.364 wall seconds; current controller owns its progressing materials. Preserve opacity predicate with60s wall allowance. Installed `MarkRenderersAndCollidersAreUpdated` also schedules full collider/material census, unlike the previous visual-only assumption; use its exact highlighter invalidation without that flag. Retain destroyed-owner references and first refusal instead of retrying through Unity fake-null. Production height remains unqualified. |
| Sources `20261023T0255349549599Z` | 81 PASS/3 FAIL;266.443s | All24 actual casts/spawns/debits, two books/exhaustion, SNAII, Extend and Favorite special slot pass. Hydration stops on a retained global level-up controller. Native Commit/Cancel do not clear that field: require exact unchanged intake plus positively disposed detached preview and idle producer, without clearing/canceling it. New `elementals-pin` reproduces only Favorite/pin/hydration/original-pin-cast; it does not relabel earlier24 casts. |
| Creature commands `20261023T0302167858388Z` | 10 PASS/2 FAIL;70.179s | Mansion has real `c0_before` interchapter92fb2c819ba16a24eaa73351e9708523, active canvas and14 visible renderers. Preserve refusal; use the actual review28 Oleg westward approach and creature-specific recorded perspective, with current full-body/path/ground/obstruction checks. No creature attack was reached. |
| Controller `20261023T0305550571638Z` | 11 PASS/3 FAIL;81.292s | Actual24 binding/cancel, original-source Use/cast/debit and preparation disclosure run. Native `SpellBookVM.Back` disposes but leaves the service field populated, causing local UI restoration refusal. Use exact owned `CharcaterServiceContext.HideSpellbook` and require field cleared. Inspected two real PNGs expose clipped source/disclosure labels; production rows now lead with book/preparation and shorten disclosure controls. Learning remains unreached. |
| Menu adapters `20261023T0309422535995Z` | 6 PASS/3 FAIL;95.140s | Observer itself requested Pause after strict Default acquisition. Remove that request; preserve foreign-mode refusal. Extra-book cleanup wrongly compared intentionally exercised fixture Wizard contents with intake; verify exact retained book references, released extras/stats and separately unchanged original party resources/pins. Horse effective-pixel observation remains unreached. |

All five games exit; each actual fresh95-original-save inventory, installation
and settings restores exactly. Compatibility receipts, in table order:
`compat-20261023T025134Z-fdc90bbcdffe`,
`compat-20261023T025512Z-9bc11501bab4`,
`compat-20261023T030153Z-de79f97ae92a`,
`compat-20261023T030532Z-ec92e9d6ee60`,
`compat-20261023T030910Z-508c30853617`. Last verification
**03:11:37.3148940UTC** closes window10 at24m37.315s from02:47:00UTC.
Final90-minute completion qualification has not started.

The exact cold trio observer now waits for its ordinary Cloud/Storm production
owners before pausing; general save-readiness alone does not promise that native
appearance/palette lifecycle has finished. This is a bounded read-only readiness
check, not a rebuild or opacity write. Cold scripts retain their qualified
process/backup/restoration identity; actual native cold execution remains open.

### Review30 production height and attack-view refusals

Frozen `6f4c204ff3c7a52ca26bcf40904a2bb9a210cbb5`, package
`2efad753871c5aaf1cf93796bfb283b92a2bdbad5ce5d0f2d7b5e1e049785808`, DLL
`9311b16be7cce1e99bb955ed77be2412a02714d0546ce04aff7e2c46c4a4b5f3`, MVID
`4fa59045-f89c-464e-b968-7cf21e1cf613`. Exact compilation,20 focused cases and22
installed Harmony targets pass. This candidate includes the Brachiosaurus
lower-neck/support repair, production giant height, current source/controller
observer corrections and qualified cold transaction; those additions are not
native passes merely because the package builds.

- Giant lineup `20261023T0230218284944Z`: **13 PASS/2 FAIL**,83.174s.
  All seven actual canonical casts pass. SM Frost appearance stops at12s/35frames
  with sampled dissolve0.9534376. The observer samples the first skin, including
  disabled originals, and records no Unity dissolve clock or controller/material
  identities. Cause is unknown; entry/exit native clock, visible-material,
  controller and production-owner snapshots are added without opacity writes or
  relaxing the predicate. No production-height picture or qualification results.
- Creature commands `20261023T0233284025458Z`: **15 PASS/2 FAIL**,76.394s.
  Two legal lane/camera candidates reach actual scenery obstruction or the5s
  survey bound; neither creature attacks. The next review uses the already
  observed clear mansion stage instead of Oleg, retaining all native graph,
  ground, path, protected-actor, range and actual-surface visibility requirements.
  New Crab/Brachiosaurus geometry remains awaiting native contact/recovery.

Both games exit and fresh95-original-save inventories, installation and settings
restore exactly: `compat-20261023T022954Z-15ff46f5d5cc` at02:32:01.7486003UTC,
`compat-20261023T023301Z-1aea79425470` at02:34:59.8359902UTC. Window9 closes at
28m14.836s from02:06:45UTC. Final90-minute qualification has not started.

Historical Horse row-name checks do not prove the painted image rendered:
review7/9 pixels show a beige bust, while their actual packages already contain
approved painted Horse PNG SHA256
`3bf6ac8d8c7b08869bf262c4018937d5aa6b72cf3e0b809efec916c6e0cf67c1`.
Installed Unity Image may render overrideSprite while the old observer checks
only sprite. This is a demonstrated observation gap, not a proven override cause.
The existing adapter review now observes the effective sprite/texture pixels;
approved art and foreign UI state remain unchanged.

### Review29 cloth contact and Ettin ordinary commands

Frozen `ed83d7dc47c02b2a662f27312d7e3e54326bdbb9`, package
`0f2902b0aeb2fc2a1afd9e13b529e6ca29126211992deb63e2eb651d3923132e`, DLL
`31a4cc3d4643b478ceb2f2026aebeb464f40583a930d0bd66370f596dd57f4fd`, MVID
`c678a6a0-f9ff-49bc-a0bc-6e52897248e1`. Exact compilation,20 focused cases and22
installed Harmony targets PASS. Initial frozen build lacked ignored current
GamePath.props; copying the unchanged integration configuration resolved that
build prerequisite. The failed log remains; no failed-build package was used.

- Scale `20261023T0148568542755Z`: **93 PASS/0**,132.847s. Exact active
  coefficients/pins and copied native cape configuration/poses pass. The paired
  authored grids now positively contact the disabled capsule:64 valid samples,
  2.0417 Unity seconds,2.4492 motion,2.0547 paired difference,0.2403 penetration
  without collision versus0 with collision. Private controls release and source
  materials remain alive. This supports opening the exact production cape gate;
  production height/selection/lifecycle/OFF still require their own execution.
- Creature commands `20261023T0153220984616Z`: **15 PASS/1 FAIL**,71.008s.
  Every camera candidate hit scenery before any command. Padded empty bounds
  were incorrectly used as sight targets and the selected lane rose over1m.
  The correction keeps full-body/feet framing while checking actual surface
  landmarks and a bounded set of flatter native-supported lanes. No obstruction
  name is exempted, no scenery is hidden and unknown survey results stay unknown.
  New folded Crab geometry is not qualified by this failed run.
- Ettin `20261023T0157213050053Z`: **19 PASS/0**,86.832s. Exact owned setup
  and join-combat Pause both settle normally. Native Default full attack produces
  two main and two off-hand weapon packets with one native cost. The installed
  donor selects OffHandAttack but supplies no separate action; ordinary command
  resolution succeeds without a substituted clip/handle. The narrow turn-admitted
  path also resolves under the established fixture clock; this is not a full
  player turn-based UI campaign. Cold reconstruction remains unexecuted.
  All six images were inspected: the body remains coherent, but first-frame
  summon FX and mostly ready poses do not prove a distinct off-hand swing or
  visual weapon contact. Mechanical resolution does not grant that visual claim.

All three games exited; each fresh95-original-save inventory, installation and
settings restored exactly. Compatibility receipts respectively
`compat-20261023T014832Z-c40e0e7264cc`,
`compat-20261023T015256Z-529cc572025f`,
`compat-20261023T015655Z-cd90be69fb03`. Final save verification
**01:59:02.5490496UTC** closes window8 at19m21.549s from01:39:41UTC.
Final90-minute qualification has not started. Window9 began02:06:45UTC.

Review29 sources `20261023T0207158091873Z`: **79 PASS/3 FAIL**,266.062s.
All24 cast/spawn/debit cases, two books, Extend and the actual native special
slot now pass. In-memory pin hydration refuses before serialization because
three new guards incorrectly require `fixture.Created.Contains(fixture.Caster)`.
The native fixture factory stores its caster separately; its exact teardown
explicitly includes Caster/Hostile alongside Created. The correction uses that
actual positive ownership contract and records every admission predicate.
Original party/resources/slots/pins stay exact; failed local extra-book cleanup
is independently retained, then the shared fixture teardown completes. Game
exit and all95 original saves/install/settings restore exactly at
**02:12:07.3099215UTC**, receipt `compat-20261023T020653Z-e6953608da9c`.
The failed record is not converted to PASS.

Controller29 `20261023T0213324985572Z`: **4 PASS/2 FAIL**,66.455s. Actual
Default/Gamepad is present, but admission fails before UI actions: the PC
narrative manager is absent in controller mode and the native console UI
readiness aggregate is false. The existing controller lifecycle contracts and
individual readiness predicates require inspection; absent PC objects alone
cannot prove absence of a controller narrative. No chooser behavior is
qualified. Exit and all95 original saves/install/settings restore exactly at
**02:14:54.8070501UTC**, receipt `compat-20261023T021304Z-7303ac65bbbc`.
Installed IL identifies two observer errors: controller narrative state lives
in DialogUiContext's three VM references/configured native views, and the
InGameInputLayer.inGameUiContext field is never assigned. The corrected input
proof follows the actual bound InGameInputLayerView/VM and the exact layer
created/pushed by Bind06005D20. Native hidden/unbound narrative state and each
readiness predicate are recorded; no UI state is forced to earn admission.

Combined production code compiles. Focused OFF admission10/10 PASS. The older
whole preflight runner stopped at an unrelated private-tools ACL enumeration;
its failed log is retained and it is not a pass. Final cold protection fixtures
PASS546/546,48.479s and real-process393/393,37.733s. See the existing cold plan
for exact directories and late restored-Working protection. Native cold remains
unexecuted. A source review found no concrete giant cross-instance/native-asset
mutation defect; actual production attach/visibility/reconstruction remain required.

Subsequent Brachiosaurus mesh `73f3689c5abdda09fb68f9e08182cf671c6a753966bedaf21ebb16877c04c57c`
blends the lower neck/chest and hip joins, narrows lower supports and removes
flared foot rims. Inspected front/side/gameplay offline views are improved; exact
tail/upper-neck/head, skin weights, UVs, paint and gesture remain unchanged.
It awaits native gait/contact inspection. Fresh-OFF Cloud/Storm compatibility
view checks and production giant ownership checks are staged for the next candidate.

### Review28 ordinary attacks and full-rank source observations

Frozen `53fda8c3f62790c2bc28f0b944368396cb457c01`, package
`ed22f52a75665263502efda21dd07a471548c0cdbac1c99ce09c6429af48c69b`, DLL
`44ba275911e6ece5c4371f9bebee9f4b1284467481dfcd178f980f5d2906e4e8`, MVID
`31a065de-659d-4f64-9227-5e1e2afefdae`. Exact compilation,17 focused cases and21
installed Harmony targets pass. These results do not qualify subsequent bytes.

- Creature commands `20261023T0118396309129Z`: **28 PASS/0**,83.617s. Both Crab
  and Brachiosaurus actually approach and attack in native Default mode; native
  weapon/rule/cost, Crab's attributed Constrict rider, private gesture/recovery
  and source/collider invariants pass. Root inspected originals. Brachiosaurus
  is opaque with the smoother neck, but tree occlusion and synchronous capture
  limit visible contact/gait judgement. The new folded Crab arms and normal-frame
  fixed camera are subsequent changes, not qualified by this run.
- Scale `20261023T0113506000167Z`: **92 PASS/1 FAIL**,144.671s. Root inspected
  both newly clear six-family lineup originals. Production colors are distinct;
  experimental relative height is readable. The separate authored contact
  control now has exact active coefficients and stable pins but never reaches
  the capsule. Its zero backstop opposes the stimulus. The correction uses the
  installed native cape's disabled-backstop sentinel and records actual Unity
  elapsed time. Positive collision proof and production copied Cloth stay open.
- Ettin `20261023T0122426972123Z`: **6 PASS/2 FAIL**,69.498s. Exact admission
  diagnostics isolate native mode `Pause`; all other ownership/input predicates
  pass. Installed native unpause ignores requests while Pause is pending;
  owned setup and join-combat auto-pause are the two possible sources. The
  bounded caller correction records and settles only its exact fixture Pause,
  restoring the original stack/counts before unchanged strict Default admission.
- Sources `20261023T0125539596463Z`: **71 PASS/2 FAIL**,257.527s. All24 original
  elemental choices actually cast/spawn/debit. Two native books, exhaustion
  isolation, SNA II identity and native Extend creation/cast/duration pass.
  Fixture cleanup fails `ownedFeaturesAndBooksReleased` and masks an earlier
  exception after Extend; special-slot and pinned-button checks were not reached.
  Original party/resources/slots/pins are exact. This remains a failed run.

All four games exited and each actual95-original-save inventory, installation
and settings restored exactly. Compatibility receipts respectively:
`compat-20261023T011809Z-7935ade4c5f2`,
`compat-20261023T011322Z-695671df5ca6`,
`compat-20261023T012217Z-4a6e3b2dce6f`,
`compat-20261023T012531Z-ebfc7f1c3017`. Final save verification
**01:30:26.4500841UTC** closes window7 at26m18.874s from01:04:07.576UTC.
No console run was added. Final90-minute qualification has not started.
The existing local comparison now contains73 original images, including the
clear lineup pair, with identities and visible pixels checked. Owner review is
still separate from these technical and visual observations.

### Review27 completed vertical slice and remaining reproductions

Frozen `a3706701502c05f8f52da42059ecd45cb5bb00b8`, package
`a2f9f83a9f1e5dd799266f5cca9595b02880ced6f06b5c59fa5dbdf54531bb52`, DLL
`81c3b851cba680b0a5a0df3210a9a3700b695480a8e37511c216bbda025aa8a1`, MVID
`587b3de7-4154-4eaf-ab63-b5bdd9de58d1`. Compilation,17 focused cases and21
installed Harmony targets pass. Guarded push and PR draft status verified.

- Desktop `20261023T0049016760290Z`: **21 PASS/0**,90.098s. Root inspected the
  readable two-source chooser. Actual standalone and SM Small Fire casting,
  spawn, separate preparation debit, exhausted-source refusal, cancel and native
  refill settlement pass.24 bindings are checked; this is not24 executed casts.
  Other source contexts/controller/learning/adapter qualification remains open.
- Scale `20261023T0044130254786Z`: **92 PASS/1 FAIL**,126.738s. Native palette
  settlement, initialized private cape configuration and all six poses pass.
  The separate authored contact-control grids were initialized while inactive;
  their pinned particles cease proving a stable frame. The correction requires
  exact active-solver coefficient readback before motion. Production copied
  Cloth stays gated. The lineup remains blurred/partly obstructed; the next
  owned capture widens the lens, lowers the angle and restores native depth of
  field exactly after disabling it for the relocated camera.
- Creature commands `20261023T0052157974263Z`: **14 PASS/2 FAIL**,74.191s.
  Crab really approaches and executes one native attack/cost. Its legitimate
  Constrict rider produces a second damage event, violating the observer's
  one-damage assumption. Future checks distinguish exact primary damage and
  positively attributed native Grapple/Constrict; old evidence stays FAIL.
  Root inspected full-body contact: visible claw reach needs further diagnosis.
  Brachiosaurus was not reached.
- Ettin `20261023T0056029449549Z`: **6 PASS/1 FAIL**,67.476s. Native Default
  disposal refuses before combat assertions and masks the original setup
  boundary; raw evidence does not identify which ownership predicate changed.
  The correction limits the native lease to commands after synchronous fixture
  casting/placement, retains original setup exceptions and records each exact
  refusal predicate. No guard is relaxed or ordinary RT/TB PASS claimed.

All four games exited, compatibility transactions restored and verified exact
installation/settings, and each fresh95-original-save inventory remained exact.
Receipts: `compat-20261023T004345Z-7c38d88a8e6c`,
`compat-20261023T004834Z-b7f6a991f576`,
`compat-20261023T005151Z-91b79ac9f82c`,
`compat-20261023T005540Z-a846928edf42`. Final save receipt
**00:57:25.4194686UTC** closes window6 at20m43.419s from00:36:42UTC.
Final90-minute qualification has not started. The local comparison contains71
originals with identities/visible pixels verified, including the successful PC
view and selected retained Griffon/Beetle/Elephant views. No owner approval.

### Review26 exact cause and restoration

Frozen `4f07fd6f5fc741358be137aad960dbd4fe81d692`, package
`2a6a2327919773152123377b9a6f84c261a63cc8539959042653064a7b204b0f`, DLL
`8be3f78bc8c74a2800f25fb412969327a578b9a495ceb20716964f0a61c39125`, MVID
`e3c34432-c763-4b61-ab22-fad4e89c1fe1`. Compilation, strict packaging,17 focused
cases and21 installed Harmony targets pass. Scale run
`20261023T0028331805091Z` records **61 PASS/3 FAIL**,87.177s.

- The first pre-commit identity check requires an enabled solver even though
  the preparation state deliberately disables it. **The revised configuration
  round-trip was not reached.** The correction distinguishes disabled preparation
  from initialized live state, retaining the positive initialized/finite gate.
- Fire's exact native material intake has an unfinished cutout animation and
  DISSOLVE_ON while its cached default is false. Native controller reinitialization
  resets the detached original to that default. Palette acquisition must await
  actual native animation/default settlement; full protected-state comparison
  remains required. No shared donor state is overwritten to satisfy the check.
- Root inspected the PNG: all heads fit, but the chandelier still covers a
  middle torso. The22-degree native lens puts the camera42m away;20/25-degree
  views hit room walls. The next capture uses a temporary50-degree lens to fit
  the same coplanar row from inside the room and restores the exact original FOV.
  Actual image inspection remains required; no visibility PASS is inferred.

Transaction `compat-20261023T002806Z-e8c73578330c` is Restored and verified,
installation/settings exact, game exit confirmed and all95 fresh original saves
exact. Final preservation receipt **00:30:18.0629683UTC** closes development
window5 at27m40.063s from00:02:38. PC/creature26 were not launched because the
remaining window could not accommodate their successful full paths. Final
90-minute completion qualification has not begun. Original FAIL records remain.

### Review25 exact causes and next bounded correction

Frozen `f8b507cb2c7c46af12903c5594f56cfe69eadba0`, package
`bfb0c32d732cfa09c2428a3884c36a6882381270859f90d4ad90b82a99ad1108`, DLL
`0890a0dbaeb570dd6c7153409489f4e396a01a17f7e75069562b0e898e41a61f`.
Compilation/package and17 selected cases pass;21 installed Harmony targets pass.
The wrapper's16 exact alternate-profile pre-entry refusals pass without a game.

- Scale `20261023T0008406501314Z`: **61 PASS/3 FAIL**,87.663s. Actual images
  fit all heads, but a chandelier still crosses the middle bodies; the next
  camera prefers lower downward views. The owned Frost cape has292 expected
  coefficients and **zero readback while inactive in hierarchy**, with every
  scalar/collider/particle comparison exact. The correction initializes the
  hidden owned solver before source commit and retains the strict comparison.
  Storm separately fails protected material queue/keyword state; exact intake/
  current controller diagnostics are added, with no inferred admission.
- Desktop `20261023T0012154533263Z`: **14 PASS/2 FAIL**,76.364s. Root inspected
  the corrected, readable actual chooser. Original Small Fire cast/spawn and
  preparation debit pass again. The retained session becomes stale after
  preparation changes: installed native HandleMemorizedSpell queues Update's
  refill, which replaces the session and selects the highest nonempty level.
  The observer now waits for that native refill and three stable frames, then
  selects the current session's level through native OnClick. No source or
  resource contract changes; two-source/exhaustion execution remains pending.
- Creature commands `20261023T0015287498017Z`: **13 PASS/1 FAIL**,70.457s.
  Before any attack, the game's serializer turns the route-count dictionary
  into an array, so JObject.FromObject throws. Explicit JProperty projection
  fixes that receipt only; no movement, path or combat assertion is relaxed.

All three original FAIL records remain. Their exact compatibility transactions
`compat-20261023T000816Z-3c812a9c77e9`, `compat-20261023T001153Z-de784223bb7e`
and `compat-20261023T001503Z-7b98f7ba624c` report Restored and verified exact
installation/settings. Each fresh95-save inventory is exact; final third-run
guard receipt is00:16:56.4650279UTC. Window5 began00:02:38UTC and closed after
review26 as recorded above. Final90-minute completion qualification has not begun.

### Review24 implementation and evidence

Review23 (`eea1d1a8`) compiled and passed its 17 focused cases, then strict
packaging refused three stale Gorgon export pins. No installation or native
process was started. The current correction pins the inspected forged-scale
exports exactly; the existing positive/stale/missing/cross-wired export check
passes 129 assertions in 2.432s. The failed build remains in review23 artifacts.

- Girallon distal fingers and hand paint, Stegosaurus stance/ankles/material,
  and Brachiosaurus neck/chest/limb treatment are implemented and reproduced
  exactly. Root inspected full offline front/side/gameplay previews and rejected
  an earlier Brachiosaurus neck fold. The final continuous neck arc preserves
  the exact tail/gesture weights. Native inspection of these changed bytes is pending.
- MENU-02 now follows the authorized source-preserving Option B implementation:
  a transient desktop Elementals chooser, original source/child dispatch, native
  controller casting navigation, and ordinary PC learning/preparation disclosure.
  Mixed SM/SNA rows and original definitions/resources remain intact. Desktop
  and console learning/preparation disclosure are implemented. Native observers
  now cover allocation/cancel, preparation/undo, all24 originals, two books,
  Extend, a favorite-slot exception, SNA/SM contexts, pins and a fresh module-OFF
  process. Their actual native execution remains open.
- Actual native Default progression is scoped through the native mode factory
  and the existing fixture clock gate. Manual pulses are refused while that
  exact native mode owns progression. The ordinary body/Ettin command checks
  retain weapon, rule, action-cost and source-state evidence; optional Hit and
  OffHandAttack handles are not manufactured.
- The exact three-phase cold transaction is implemented. Disposable Working/
  backup/metadata/current-intake tests pass458/458; separate guard-host/process
  lifetime tests pass316/316. Current inventory has no fixed mod-count gate.
  See the current section of `CLEANUP2-ETTIN-COLD-PLAN.md`; native cold remains pending.
- Relative render-only giant ownership and a stable visual sole pivot are wired.
  Copied native Cloth remains closed in production until the bounded native
  contact/solver probe establishes its contract. No gameplay root is scaled.

### Review24 current native observations

Frozen source `d91487c274cdc6e0ff26f45a4604dcbbab99bcc6`, package
`46ba070947b8438935aa75ef50139f1454ddeda24af77fe60ed661b833b249b0`, DLL
`7d820cb95bd8286e54fc26f525885eb087c40d5580a74f6a6945511c65aa7ae2`,
MVID `7010835f-c8a5-49d6-8bf6-4ccd4404a737`. All results below retain this
identity; later source repairs are not execution on these bytes.

- Scale `20261022T2341070702515Z`: **60 PASS/4 FAIL**, 86.609s. The actual Frost
  Cloth has 292 coefficients, all with exact `float.MaxValue` disabled collision
  distance. Sentinel preservation now passes and the probe reaches the owned
  copy, then refuses its strict Cloth configuration round-trip. The exact
  differing field was not captured; the bounded field diagnostic awaits native
  execution. Production Cloth remains closed. Camera acquisition/settlement
  is separate from visible-environment proof: CPU-unreadable scene triangles
  prevented qualification, and this run has no qualified lineup PNG.
- PC Elementals `20261022T2345454148399Z`: **14 PASS/1 FAIL**, 76.563s. The actual
  standalone Small Fire native cast and source debit pass. The next launcher
  lookup finds no active unique chooser after casting. The actual first PNG
  also exposes overlapping labels from narrow text rectangles. Layout and
  native reopen/fresh-session observer corrections are implemented afterward,
  but are not native-qualified here. All 24 casts, book/metamagic/special-slot
  coverage and fresh module-OFF remain open.
- Bodies `20261022T2350096990938Z`: **38 PASS/5 FAIL**, 121.773s. Gorgon completes
  its 117-original packet and 5.9618144m travel; its ordinary native command
  proves weapon/rule events, costs and recovery. Root inspected the forged
  plates, walk and attack/recovery positively; owner art approval remains open.
  Girallon and Stegosaurus ordinary native attacks also pass. Their walk packets
  refuse before tracking because no complete six-meter footprint-clear route
  avoids the caster/other actors and route clamps. This is fixture placement,
  not a newly demonstrated product defect; the travel requirement is retained.

The existing small local comparison is
`artifacts/cleanup2-replacement-current/index.html`: **63 originals**, adding
nine native24 Gorgon views and one explicitly failed **before-layout-fix** PC
chooser image. Exact source/run/DLL/package/file identities accompany every new
image. Original pixels and the historical 188-image portable packet are unchanged.
The unchanged exporter completed in 1.248s with 63 visible-pixel checks PASS.

All three hosts exited normally. Transactions
`compat-20261022T234041Z-e4789daca88c`,
`compat-20261022T234522Z-ea74a626a732` and
`compat-20261022T234946Z-bacab5ac9588` each record exact installation/settings
restoration and all 95 original saves preserved. Their protected-native receipts
remain under frozen review24 `artifacts/cleanup2-protected-native/`. The last
save-preservation receipt completed **2026-10-22T23:52:28.4444403Z**; window 4
ran 23m59.444s from 23:28:29UTC. Final 90-minute qualification has **not started**.
All original FAIL records remain unchanged.

### Review22 observations (preserved)

Frozen source `0781beba6196f086ad43abdaf50685b87786fa57`, package
`ea1d4bc56d99276f1eff59dfbde2647c562ce00dda550c8211e268edb5081e7f`, DLL
`4bf3c0a7dcac6487cfd5c5fae5fcc2c3e1d6363aeb7c6863a6dd06660f2cb7d7`.
Exact compilation/package and17 selected cases passed; all18 actual Harmony
target-resolution cases passed, including the original constructor negative.

- Bodies `20261022T2307092060103Z`: **38 PASS/5 FAIL**,159.001s.
  All three actual native Default single attacks passed rules/events/costs.
  Girallon completed its packet and6.014m travel. Root and art reviewer inspected
  ordinary and refined front/side/attack views: attached, separated far fingers
  improve readability; exact side occlusion does not establish a new tear.
  Stegosaurus is elongated with improved ankle/foot contact; its smooth finish
  remains for owner judgement. Gorgon's native specular highlight is visible,
  but the broad rectangular armor was rejected and replaced with smaller forged
  scales. That new Gorgon export is offline-inspected, not native-qualified.
- Gorgon/Stegosaurus travel failed at .068/.066m. Their post-attack facing chose
  unsurveyed six-meter routes; the old raw packet does not identify the actual
  path/command refusal. The scoped correction surveys the complete native route
  and records command/path/velocity state without lowering travel requirements.
  Action captures now sample while the command runs, not mainly after it ends.
- Scale `20261022T2311417992329Z`: **59 PASS/2 FAIL**,86.432s. The camera reaches
  all seven actors, but chandelier/occlusion silhouettes prevent final palette
  acceptance. Frost's pre-mutation Cloth coefficient multiplication overflows;
  the old packet omitted the exact input. The corrected arithmetic preserves
  only Unity's exact `float.MaxValue` disabled-constraint sentinel, refuses real
  enabled overflow, and records coefficient extrema before admission. Production
  Cloth remains closed pending the actual positive contact/solver probe.
- Source review found native console preparation disposal clears its selection
  property. Owned header retirement now avoids disposed selection authority;
  the existing console observer checks actual disposal/unbinding/resource release.
  Druid conversion is being projected through the existing native variant factory
  so children retain the original conversion/preparation source chain.

Both native hosts exited normally. `compat-20261022T230642Z-a960be0e4a65` and
`compat-20261022T231115Z-655b3a7ccbe6` report exact installation/settings restore;
both fresh save inventories are preserved exactly. Window3 closed23:14:17UTC,
26m02s after22:48:15, inside30minutes. Final90minute qualification has not begun.
All original FAIL records and PNGs remain unchanged.

### Review21 startup failure (preserved; no creature result)

Frozen source `3def23cf2f1b534d95b9d1dbfd2f392f09afc78f`, package
`4b8f49d2eca2fcdf19a2e5a888a66209cfbe9cd663b64bc8b9d6d6a49682bbe5`,
DLL `7144581afe4c5b39bc148626395b6e52cc64db35a360fff46eb60935d1951ffb`,
MVID `25a83b3b-b97b-43d7-ae48-3f37dd5e8530`. Exact packaging and16 selected
cases passed, but recovery-bodies `20261022T2248408770827Z` never activated:
Harmony12 could not resolve the new console preparation constructor because its
attribute omitted the native arguments. Mod bootstrap failed before fixture/save
interaction. This is an introduced integration defect, not an art result.

The fix uses the exact installed three-argument constructor; the new console
targets passed actual Harmony target resolution (17 positive targets and the
original missing-constructor negative fixture,18/18). Corrected production
compilation passed12.7s and9 focused cases passed2.5s. The exact PowerShell scope
predicate passed52 positive/refusal cases across13 advertised phases.
The original UMM log is retained in review21 artifacts. The exact owned process
was closed normally (no force termination); its guard remained alive throughout.
`compat-20261022T224818Z-c52079c11d9b` records confirmed exit, exact installation/
settings restoration and all95 original saves preserved. No review21 PNG exists.

The prior development validation window closed22:42:50UTC (28m16s). The next
focused increment started22:48:15UTC; final completion qualification is not started.

### Review20 (preserved)

Frozen source `824874b96ec3d43c40e0ead5f51c235aa9f46386`, package
`ce94e9e5ac53007cad45616464d67b5f48187c6a8e20e6c20e03f361cb66882b`,
DLL `7a616cc07fc1d2a897ceb60e75b23e272fda4ca8e036d007b8b8f9caccd834c6`.
The selected 15 cases and exact compilation/package checks passed. Native
results remain FAIL and do not qualify the new art or chooser:

- `elementals-presentation` refused before staging: the compatibility wrapper
  retained a second obsolete phase list. It now uses the existing central exact
  scope predicate; original refusal remains recorded.
- Scale run `20261022T2221041829008Z` passed standing eligibility (51 PASS / 1 FAIL)
  but stopped before Cloth on an invisible native selection capsule. Camera
  admission now excludes only exact native view-owned selection collider fields
  without any rendered geometry; walls and unknown obstacles remain checked.
  The production-colors original has real visible actors but is a rear view;
  it is not final palette/height acceptance. Fixture actors now face the selected
  camera, with exact original orientation restoration.
- Body run `20261022T2225018851552Z` stopped before capture because the observer
  required Dialog although the recorded fixture was already native Default.
  The corrected lease borrows that actual mode, retaining exact stack/counters,
  factory pointer and public-controller identities, or uses the native factory
  push from Dialog. Manual fixture ticks remain forbidden during the lease.
- Both launched transactions restored the installation/settings exactly after
  confirmed exit; all 95 current original saves match their fresh intake. Receipts
  `compat-20261022T222036Z-f99a6f8437bc` and
  `compat-20261022T222437Z-720770d909ba`. The preflight refusal also preserved all95.

Console book/learning presentation and focused source/adaptor observers are now
implemented; exact-reference compilation and 8 affected behavior cases passed.
Real native execution remains required. No module-OFF, cold or final90-minute
completion qualification is claimed by this increment.

### Review19 (preserved)

Frozen source `276fd667ccd6cbb060ee3e7b6c672625f8673356`, package
`be44e6129b03275f3e28c12ac064f13338742506c33255c9bcbf5f7c3dba9308`,
DLL `e6548c89cf91fc62e74b4a0ae88f7be4120951a964da54e7e0455974664a33fa`,
MVID `19507ffc-c653-46d8-956e-ea6c817cda3c`.

- Scale probe `20261022T2138470733174Z-disposable-expanded-summoning-cleanup2-native-review`:
  **51 PASS / 3 FAIL**, 93.508s. Four finite body measurements per actor were
  stable, but all actors had the same native visual/navmesh offset; the old
  exact-ground admission refused before Cloth. The first wider PNG was inspected
  and rejected because a castle wall obscured the actors. New camera obstruction
  checks/downward angles and relative fixed-sole comparison are unexecuted corrections.
- Gorgon `20261022T2145486092881Z-disposable-expanded-summoning-cleanup2-native-review`:
  **12 PASS / 2 FAIL**, 94.080s; only the old optional-Hit packet expectation
  failed. Root inspected front/side/action originals: hard plates were excessively
  black. Installed D3D11 shader inspection proves a direct-light metallic path
  without environment reflection: effective metal.94 removes94% of diffuse.
  The new owned scalar.4 retains62.4% diffuse plus actual native tinted specular.
  Geometry/paint are unchanged; this correction awaits native inspection.
- Both transactions confirmed game exit, exact preservation of all **95 saves
  in their fresh intake**, and exact installation/settings restoration. Receipts:
  `compat-20261022T213810Z-08b741dcf755` and
  `compat-20261022T214526Z-5217b5993e10` under the lab compatibility-state directory.
  Original FAIL records remain unchanged.

Development validation window21:33:26–22:02:54UTC stayed within30minutes.
Affected production compilation passed;14 selected source/visibility rollback/
geometry/giant-policy cases passed in1.4s. Compile failures exposed and corrected
missing native abstract/reference/type contracts before native use. The final
90-minute completion qualification has **not** begun.

## Ettin replacement continuation, 2026-10-22 (previous checkpoint)

Intake verified clean integration HEAD `e3cd9471600f9a376c060faf5a480ab0cd086662`
and the same open draft PR. This continuation supersedes the rejected appended
head design; earlier native failures and the 25+8-original gallery remain intact.

- **Ettin:** one private complete body replaces the visible donor body. Two
  detailed reshaped faces, necks and shared shoulders use installed donor topology
  and unmodified skin/hair maps. No game geometry/pixels are distributed. The
  actual production array builder was used for offline front/side/rear/three-quarter,
  gameplay-distance and synthetic neck/head/jaw previews. The initial ridged
  shoulder join was rejected and rebuilt. Two private native-pose chains, one
  owned material and exact renderer suppression replace obsolete extra-head
  assumptions. Current cache comparison, source preservation and private release
  checks remain. Expiry now observes the actual native dismember fact/FX path and
  positive entity/view disposal; off-hand selection records its exact native
  action lookup. Cold load remains unavailable through a qualified Cleanup2 path.
- **Gorgon:** 25 hard beveled plates replace rounded pads; a separate linear
  authored parameter map confines metal/roughness changes to plate UV regions.
  Continuous bull body, bones, horns, eyes and hooves remain exact. Inspected
  offline geometry/PBR previews and native panels; the native metal finish remains matte.
- **Girallon:** lower hands are compact, broader and separated outboard/forward;
  joined neck, body and articulated upper hands remain exact. Inspected ordinary
  and synthetic pose previews; native far-hand fingers remain dark/partly occluded.
- **Stegosaurus:** slimmer leg shafts and more compact feet retain the useful
  elongated trunk, tail, plates and measured donor joints. Synthetic stride and
  tail poses inspected. Native crouch/material presentation still needs finish.

### Current native inspection and demonstrated corrections

The replacement Ettin is now inspected in-game: matching detailed adult faces,
continuous neck/shoulder anatomy, normal opaque skin and exactly two visible
heads per actor. Whole-body, close front/side and available native main-hand pose
samples were inspected. Owner acceptance remains pending. The latest local page is:

`C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger\artifacts\worktrees\summon-cleanup-iteration2\artifacts\cleanup2-replacement-current\index.html`

The page selects48 unique originals:29 current native,7 historical and12 offline.
Original paths, candidate/run identities and SHA-256s accompany it; every selected
PNG was checked for visible varying pixels. Offline previews are explicitly
labeled. The previous 25+8 and 188-original galleries
are preserved; none is relabeled as a new candidate result. No gallery upload.

- Review16 Ettin failed before captures: native visibility re-enabled the donor
  body, and 31 small-face normals were zeroed by Unity's `Vector3.normalized`
  epsilon. The exact body now uses the existing per-renderer suppression owner;
  double-length normalization preserves those valid tiny surface directions.
  Private production-array reproduction now has zero invalid-length normals.
  Protected outfit/weapon visibility also changes natively during appearance;
  that mutable enabled flag remains recorded, while exact renderer/mesh/material
  identities, source-body suppression and full material-cache checks stay required.
- Review17 confirms attachment, valid skinning, actual head/surface following,
  current cached material agreement, independent expiry, recast and private
  resource release. Positive expiry evidence is the native dismember/FX fact
  followed by entity/view/holding disappearance, with borrowed resources intact.
  No positive SMC dissolve ramp is claimed or forced. The two remaining failures
  identify the actual off-hand flail selecting `OffHandAttack` with
  `SlashingOneHanded`, absent from the native action set. No combat command was
  issued by this pose observer; this absence does not establish broken combat.
- Review16 attack reproduction isolated `mechanicsExact`: all other 19 named
  rule/event/cost/state predicates passed. Installed native getter `060018D5`
  returns movement-agent corpulence `.5` outside CombatMode and the view's `1.1`
  in combat. The old baseline preceded that ordinary transition. Review18 takes
  one stable native combat baseline before either matched command, retains it
  across all four modes, and uses deep-cloned JSON to avoid float roundtrip
  differences. **Crab and Brachiosaurus both pass** single/full render-OFF/ON
  scheduling, rules, costs, private gesture motion and original rig/collider
  preservation. Close ready/act/recovery captures were inspected; framing limits
  contact appraisal, especially the Brachiosaurus tail. This is not final attack
  art acceptance or natural Default-mode/turn-based timing qualification.
- Review16's three body packets contain 351 originals, all packets complete:
  Girallon lower hands are more separated, but far fingers remain dark/occluded;
  Gorgon panels have hard boundaries, but native response remains matte;
  Stegosaurus feet are slimmer, while its crouch and soft finish remain unfinished.
  Front/side/rear and sampled attack/movement frames were inspected. All three
  native Hit actions are absent, preserving the packet FAILs. Gorgon's owned
  parameter map is positively bound; scalar/map composition and global
  `SPECULAR_ON` state are not established, so no speculative shader fix is claimed.

### Frozen candidates and focused results

`Build-Local.ps1 -DevelopmentPurpose summon-cleanup-iteration2` used the existing
qualified `cloth-probe-current` reference bundle. Reviews16/17 selected ten cases:
`cleanup2-native-admission`, `cleanup2-production-gesture`, `repaired-first-exports`,
`sprint23-original-art`, `cleanup2-giant-palette` (all under `expanded-summoning`).
Review18 selected the four attack/admission cases after its observer-only change.
All selected cases, installed-reference compilation and strict packages passed.
Captured build-log spans were186.9s,200.7s and191.8s respectively; selected
runner execution was about1.9s. These are focused development gates, not a full suite.
Review15's stale 398-file packaging limit failed and remains recorded; the added
Gorgon parameter map makes the exact count399. No release qualification.

| Candidate | Source commit | Package SHA-256 | DLL MVID |
| --- | --- | --- | --- |
| Review16 | `809c39c2ab69f1da369772ce2add37918dc7fb09` | `05c1f8a3c5507ad63f6591e0852f8d8e74050dafe5e8b25c4a7eebc702d4b78b` | `32f5373d-d1b4-4e6b-83a7-19e85d476493` |
| Review17 | `6bba8d99edbe33c232e54c72be007fa0251a9abd` | `2ecdceb756fcb62b5b8b365a9babe24cc53c85b8c0949f2c44f25022666e4f6a` | `8eb48796-06c4-4b01-84dc-09d3eb90d678` |
| Review18 | `2955e0add984c73f925063afd66dfe3f86c38573` | `e533f0aa75cc02e195dea2f54710a4ffb319ad9800d4389f4739113eb82be99e` | `8f4153af-6244-46b9-ac72-8eb95c716778` |

Each frozen checkout is `artifacts/worktrees/summon-cleanup2-review<N>` under the
main checkout, with `artifacts/cleanup2-review<N>-build.log` and its immutable
development receipt containing DLL/source-state/reference hashes. Runtime paths
below are under `C:/Dev/KingmakerGunslingerLab/runtime-evidence/` and have the suffix
`-disposable-expanded-summoning-cleanup2-native-review`.

| Candidate / phase | Evidence directory prefix | Result |
| --- | --- | --- |
| 16 / Ettin initial | `20261022T1914314629192Z` | FAIL8/2,73.779s; demonstrated visibility/normals defects, no PNGs |
| 16 / attacks reproduction | `20261022T1917207146701Z` | FAIL11/2,74.529s; exact native combat-radius baseline cause |
| 16 / three changed bodies | `20261022T1920283881216Z` | FAIL24/4,146.483s; complete351-image packets, unsupported Hit actions |
| 17 / Ettin replacement | `20261022T1924206397557Z` | FAIL77/2,112.302s;48 PNGs; lifecycle/material/skinning success, two absent off-hand actions |
| 18 / Crab and Brachio attacks | `20261022T1927229897433Z` | PASS55/0,123.228s; both creatures and all matched command modes reached |
| 18 / giant palettes/scale observation | `20261022T1931020259616Z` | FAIL28/1,73.989s; all six ordinary palette ownership checks pass; lineup camera clips/empties its frame and refuses before PNG/scale prototype |

Giant scale remains unimplemented; this camera failure does not establish an
engine scaling prohibition. The next bounded step is to fix the existing lineup
framing, inspect production colors and qualify relative render-only height with
native clothing/equipment/collision/selection/effects. Exact feet are unnecessary.

### Restoration and remaining boundary

The focused validation window began19:04UTC; the final native transaction restored
by19:32:30UTC, within the ordinary30-minute ceiling. No further compile/native
check was selected. All six original guard hosts retained their save handles
through confirmed game exit. Every fresh intake contained95 original saves;
all six preservation receipts report exact originals and confirmed exit. All six
compatibility transactions report `Restored`, `restorationVerified=true` and exact
game settings/current-installation restoration. Native run failures remain FAIL.

Transaction IDs under `C:/Dev/KingmakerGunslingerLab/compatibility-state/`:
`compat-20261022T191355Z-a2c634fd0012`, `compat-20261022T191656Z-e8d57b46a2ae`,
`compat-20261022T192002Z-39c95dcc6816`, `compat-20261022T192356Z-fa7f2df49515`,
`compat-20261022T192659Z-0e5961c143ae`, `compat-20261022T193041Z-c1d9884b4c34`.
Each frozen worktree's `artifacts/cleanup2-protected-native/` retains its fresh
catalog, protection/preservation receipts and original failure status.

Cold reconstruction remains unqualified: the installed-stack wrapper rejects
Cleanup2 development purpose before mutation, and the local cold-load draft has
not qualified acquisition/recovery/process boundaries for the current16-mod
stack. It was not invoked. No real save, permanent installation, release, PR66
work or standalone elemental invocation was changed.

Unchanged menus, names, Horse, Mastodon/Triceratops and other successful evidence
retain their original provenance. This is a substantive visual development
checkpoint, not completion of giant height, Brachiosaurus finish, cold load,
owner acceptance or the mission. Elemental consolidation stays deferred.

## Previous recovery pass, 2026-10-22

Intake verified the clean integration worktree at
`bca1735515293568647498cd54c9de5825c8ef18`, on
`codex/expanded-summoning-cleanup-iteration2`. PR67 was open/draft; the main
checkout and PR66 work were left untouched. The historical gallery below is
review5-9 evidence, not evidence of these repairs. The four transparent uploads
were not used to judge appearance.

### Repairs and demonstrated causes

- **Ettin:** the review12 native reproduction exposed only two changed cached
  fields: render queue `2000 -> 2450`, and RenderType `Opaque -> TransparentCutout`.
  All four head caches matched the exact current donor cache; other attach-time
  fade defaults and texture identities matched. Installed Assembly-CSharp
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`,
  `DissolveAnimationController.Update` token `060011A3`, IL `00D0-00E6`,
  performs this transition. The observer now permits only that paired native
  transition when dissolve/cutout were enabled, retains the original strict
  attach comparison in evidence, and requires the complete current donor/head
  cache comparison. Production dissolve is not forced or reset.
- **Ettin art:** planar cheeks/jaw, thinner angled brow, eyelid rims and facial
  furrows replace the smooth mask face. The neck fan, two-driver rig and warmed
  original paint remain. Deterministic export: 5,026 vertices, 9,992 outward
  triangles; closed skin and normalized weights. Captured-frame offline renders
  are supplemental and are not native animation.
- **Girallon:** two terminal trunk rows close the visible neck/skull gap, with
  a Neck-to-Head weight blend and the final row buried inside the skull. All
  existing arm, palm and digit positions/normals/weights remain byte-equivalent.
  Export: 8,325 vertices, 16,244 triangles, 44 closed outward components. This
  does not claim to solve far-hand readability.
- **Giants:** ordinary summon hooks now own exact body-atlas palettes for Stone
  (gray), Fire (warm charcoal), Cloud (pale blue), and Storm (bronze). Hill and
  Frost retain native colors. Separate equipment/cloth surfaces stay protected.
  Height remains inactive: scaling the native root would also scale gameplay
  colliders; the render-only proxy still needs its clothing/collision proof.
  Exact feet and a new anatomy measurement system are not acceptance conditions.
- **Attacks:** review12 passes the corrected immediate native-act scheduling
  predicate but fails the independent aggregate rules/cost predicate. Existing
  raw data cannot identify every conjunct; the observer now records each one
  and bounded changed fields, without relaxing any requirement. Also corrected
  two image-receipt comparisons (`png=<name>;bytes=...`, not a bare filename).
  The diagnostic Claw-to-Bite alias remains excluded from production.

### Frozen inputs and focused checks

Review12 source `cd06cebc9f5e77d554f815bc13cb69cfff49fa85`, ZIP
`5434e6a8e153566a426b09cdb59597715468cceb78302cf90c44e931e6b2025c`,
DLL `c1d7e61f0b83a6a0ef473516c6c050448e681c352c5ee6fd6d1b3e30233afc1b`,
MVID `2904ec38-7686-4e37-8ab6-2ec298b16d47`: exact compile/package and 27/27
selected Cleanup2 cases passed. Native Ettin FAIL8/2 (73.774s), evidence directory
`20261022T1720034941943Z-disposable-expanded-summoning-cleanup2-native-review`;
attacks FAIL11/2 (78.379s), directory
`20261022T1722592197869Z-disposable-expanded-summoning-cleanup2-native-review`.
Both guards confirmed exit, exact current installation/settings restoration,
and all **95 currently inventoried** original saves unchanged.

Review13 `b033c10497` stopped at the stale Girallon hash in the repaired-body
test (36/37); its failed log is retained. The corrected pin is frozen at
review14 `70e575b8150cd3183c0281ba30ea9fd3cf0d51e3`. The added `recovery-bodies`
selection reuses existing Girallon/Gorgon capture and preservation checks;
phase preflight passed 832 checks. No existing runtime guard was weakened.

Review14 immutable package completed 17:38:02 UTC with **37/37** selected cases
(1.9s runner), exact installed-reference compilation and strict package PASS.
Command: `Build-Local.ps1 -DevelopmentPurpose summon-cleanup-iteration2
-DevelopmentTestFilter 'expanded-summoning.cleanup2;expanded-summoning.native-profile;expanded-summoning.repaired-first-exports'
-ReferenceBundleDir <existing qualified cloth-probe-current bundle>`.
Source-state `c24090fc7b70e1ced2a36f70334d11cffe7d4aca2451a114a05ae0102fb7d716`;
ZIP `224dd5e1d9e5a6cda7da0b19ef5634fca2195bb0579f0beaa27af186c84a801f`;
DLL `5e5ae3f27bff13617866454d043a50b7443f186ef2b2db99b3df4db5fecdb043`;
MVID `f44123c5-16c6-4059-bc07-61e038194bdc`.
Frozen worktree: `artifacts/worktrees/summon-cleanup2-review14`; log
`artifacts/cleanup2-review14-build.log` in that checkout. It is development only.

### Current native inspection and local comparison

Open the local file (not uploaded):
`C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger\artifacts\worktrees\summon-cleanup-iteration2\artifacts\cleanup2-recovery-current\index.html`.
It contains 25 current original PNGs and 8 historical originals, paired body/head
views and sampled Girallon attack/Gorgon movement playback. All current images
contain nontransparent varying pixels; image hashes, original paths, candidate,
and run identities are in the two adjacent identity JSONs. Complete originals
remain in runtime-evidence. This is a small hand-selected page, not a new exporter.

| Review14 phase | Run | Result and inspection |
| --- | --- | --- |
| Ettin lifecycle | `20261022T1738458879562Z-766cf84fa6af4eb8aef040a5291658df` | FAIL75/4,108.848s;48 original images. Attachment/current cache, independent heads, expiry, recast and exact private-resource release passed. Two off-hand handles absent; two positive-changing-dissolve assertions failed. Body/head/side and main-hand frames inspected: warmer face and jaw/brow detail, but still a stylized mask beside the native face. **Art remains unfinished.** |
| Girallon and Gorgon | `20261022T1742162026645Z-a85127499a304bf88f409f25e17d53b4` | FAIL18/3,116.095s;234 originals. Both117-image packets complete, native travel5.974234/5.971801m and attack handles present; both Hit handles absent, causing both packet failures and aggregate failure. Side/front and sampled attack/move frames inspected. Girallon neck connection improved; far hands remain dark/overlapping. Gorgon is darker but plates remain padded and matte. **Do not approve the metal appearance.** |

The remaining Ettin fade result is now precise. Older actors recorded111 and110
expiry samples with source/head dissolve range `[0,0]`, zero active samples,
zero material-control failures and maximum head/source error zero. Controller
release was `reinitialized`, ownership `released`, assigned materials zero.
The recast actor had two active samples (maximum0.00552976131), which may be
remaining summon-in fade; the packet does not identify direction/animation.
Installed `SummonedUnitBuff.OnRemoved` token `06002A64` delegates expiry to
`Blueprint.Visual.BloodPuddleFx`, `DismemberUnitFX` and `DismemberedUnitBuff`;
it does not request StandardMaterialController dissolve. Therefore the failed
requirement that every expiry produce changing `_Dissolve` is not established
by that native contract. Both original FAIL verdicts remain. The exact native
FX/downstream component path must be understood before changing the assertion
or claiming positive fade-out proof. No production lifecycle defect is inferred
from these zero synchronized values.

Both review14 transactions are Restored with exact settings and installation
verification, confirmed game exit, and all95 currently inventoried saves exact.
Their compatibility records are `compat-20261022T173821Z-fa233903dec6` and
`compat-20261022T174151Z-4263bdd4e288`. Last native exit/restoration was confirmed
by17:44:34 UTC. No further build/native run was selected after that point;
remaining work was evidence inspection, local gallery assembly and handoff.

**Still unqualified:** review14 production attack named-predicate reproduction;
Crab/Brachiosaurus render-ON contact/recovery; actual giant production palette
capture and height integration; Ettin cold reconstruction; remaining art above.
The unchanged source10 Mastodon/Triceratops preservation result is reused, not
rerun. Palette implementation is production code but is not native-qualified.

The validation window began 17:15:14 UTC, with the ordinary 30-minute ceiling
at 17:45:14 UTC. Safe transaction completion/restoration remains mandatory.
The cold path remains unavailable: the preserved local draft lacks qualified
process-boundary acquisition/recovery coverage, and public legacy entrypoints
refuse this current-stack mission. It was not promoted or invoked.

Unchanged SNA menus, family-first labels, painted Horse and native
Mastodon/Triceratops preservation retain their original evidence below.
Standalone elemental consolidation is **deferred**, not completed; every
rules-distinct invocation remains available. No further elemental design work
belongs to this recovery pass.

## Confirmed tooling defects

The original PowerShell wrapper now owns the live save handles and transaction
lease until a precise process query confirms Kingmaker has exited. Evidence
write or process-enumeration errors cannot unwind that owner. Restoration runs
after confirmed exit; the original failure still exits nonzero.

Disposable process-boundary tests actually start the standalone wrapper and a
separate stand-in process. They prove write/delete/rename denial and retained
transaction ownership before exit, then exact restoration and original wrapper
exit. The three cases cover ordinary waiting, waiting-evidence IO failure and
process-query failure continuing beyond stand-in exit until the query recovers.
Final results: 70/70 PASS on Windows PowerShell 5.1 (13.065s) and PowerShell 7
(12.330s). No real installation, registry, game or save is used.

Frost is rejected anywhere in a requested batch before package/install/save
entry, with the existing retained-native giant-lineup route identified. The
14 advertised generated targets use the actual shared catalog resolver.
CurrentStack behavior: 124/124 on PS5.1 (15.227s), 124/124 on PS7 (11.900s).
Named resolver cases: 2/2; exact production compile 16.1s. Safety commit:
7fb9ac54a251621e3e7ae8d4ba88754d7ee8d45e.

## Review10 artifact and native evidence

Source: 3f782d4cb8b69b9d43247c0ac972c3454fa60d6a.
Source-state SHA256: 5e72ccc6b8dc785b687cec63e4ca4626876cd7019efb985bf14eed57acda1b95.
Package SHA256: d67912b70ad96715809da3ee523497d5a74d339d56691c13a2ccd7e3a0bce981.
DLL SHA256: 5118f52b819bfff1886f4fa50e1c06ca76c5c26e5639adc1e509981da263f45e.
MVID: 3ff81984-b0a0-4422-b783-446f76bcf5e4.
Purpose/version: summon-cleanup-iteration2 / 0.0.163, development only.
Exact production compile, strict package and 83/83 selected domain cases PASS
(4.3s runner). Final phase preflight 828/828 PASS.

| Phase | Run | Result |
| --- | --- | --- |
| Production attacks | 20261022T1231203057818Z-6abe5596ec7c4720a1acdeaf9a1b7bb2 | FAIL, 11 PASS / 2 FAIL, 109.411s |
| Mastodon and canonical Triceratops shared controls | 20261022T1236053125583Z-1ac4b7c8506648b4a02a4471ea4c3d6f | PASS, 24/24, 74.828s |
| Ettin lifecycle | 20261022T1241398095815Z-0d0db554918c445dacedb457a4ef01dc | FAIL, 8 PASS / 2 FAIL, 89.776s at initial head/material assertion |

Raw evidence remains in the correspondingly named save-free Cleanup2 directories
under C:/Dev/KingmakerGunslingerLab/runtime-evidence. All three guarded
transactions completed restoration, confirmed native exit and verified all
95 original saves unchanged. No Working save was loaded or written.

The attack failure exposed an incorrect minimum-delay assumption in the new
driver/observer. Kingmaker's HasAnimation=false branch legitimately acts on its
first eligible Tick despite storing ScheduleAct(1). Exact source/collider and
zero-offset skin proofs passed, but the old association gate refused the real
act. Brachiosaurus was not reached. The failed record is retained.
Correction 532e9391ee43ff02ef385c604b634fc67c0f3d4d reads the native branch,
handled transition and exact first-tick/delta association without delaying or
forcing combat. It captures ready-before-command, actual act and recovery.
Its new native reproduction is still required; no production animation PASS.

Source11 focused qualification completed: exact installed-reference production
compilation PASS (11.8s), named filter expanded-summoning.cleanup2, 27/27 cases
PASS (domain build/run3.0s; runner0.5s). DLL SHA256
157738fd6ce53c71d0fef00dae15818f6d507b8540bb044d4ee22a2e9434489f,
MVID30f36765-6678-4150-8119-454e47911d05. Frozen worktree:
artifacts/worktrees/summon-cleanup2-review11. Log:
artifacts/cleanup2-review11-focused.log in that checkout.
No source11 package or native run exists. Its attempted candidate pipeline
passed repository validation, then failed domain compilation because this new
checkout lacked ignored GamePath.props (Newtonsoft reference unresolved).
After copying the existing local configuration, the focused gate above passed.
The failed candidate log is retained as cleanup2-review11-build.log. Do not
describe this as a successful source11 immutable candidate build.

The shared controls now pass their exact source/view/material/mesh/action/scale
preservation predicates. This closes the prior failed-before-spawn technical
gap for Mastodon and Triceratops. Native pixels still need owner review.

Ettin's exact two-driver/four-material attachment and original texture ownership
were present. The live dissolve matched the donor at the initial sample
(0.008244276, maximum error zero), but the full cached fade-baseline predicate
was false. Execution stopped there. Neither subsequent changing expiry fades
nor cold reconstruction is qualified. Do not turn this partial match into PASS.
All four cached dissolve values were also zero, matching the captured native
baseline. The differing field is not recorded by this run. The next narrow
observer change must expose field-level cached baseline differences and texture
instance IDs, including attach-time versus current identities; forcing fades or
dropping the full comparison is not justified.

The Human pre-upload CPU reference contract reader initially failed on its
expected-negative exception wrapper. Reader-only correction retained the same
candidate DLL and passed 34/34 in 0.297s. This proves the no-game contract,
not native giant measurement. Giant calibration remains empty and palette
application remains an experiment.

## Visible work and remaining checks

Review10 adds warmer Ettin skin paint, controlled material adoption, original
render-only Crab/Brachiosaurus gestures, weights-only Brachiosaurus tail repair,
and darker Gorgon plate paint. Geometry/resource identities and deterministic
generation are documented in the dedicated material, attack and Gorgon notes.
No new native Gorgon paint or giant scale-prototype run was selected before the
bounded checkpoint ended. Ettin material baseline needs a narrow diagnosis and
correction before retry. Cold harness work remains a local unpublished draft;
legacy cold entrypoints still refuse this current-stack mission.

Owner acceptance remains required for Griffon intersections, Stag Beetle contact,
Girallon far fingers, Ettin face/neck, Elephant/Stegosaurus proportions/materials,
Gorgon appearance and surrounding-icon comparison. No blanket art approval.

## Local gallery and elemental presentation

Owner requested local review only; no upload or remote reviewer is needed.
Open artifacts/cleanup2-portable-review-gallery-v2/index.html in the integration
worktree, or extract artifacts/cleanup2-portable-review-gallery-v2.zip.
188 original PNGs, seven identity/result groups, 182,743,270 bytes; ZIP SHA256
eb112333d06becfc9662a8766f2071dbefe29b3d5735f4817a4ebb3d577a17c4.
This packet covers the recorded review5–9 evidence, not the new review10 work.

[Option B screen design](CLEANUP2-OPTION-B-SCREEN-DESIGN.md) specifies one
desktop Spells-tab elemental chooser with explicit original casting-source
and resource choices. Original abilities, spellbooks, preparations, items and
metamagic contexts execute. Domain/item/legacy exceptions retain access.
This is a design proposal; all six legacy ranks remain visible.

## Validation boundary

Ordinary focused window: 12:23:34–12:53:34 UTC, 2026-10-22, following
docs/VALIDATION-POLICY.md. No new live task is selected at its ceiling.
The final focused gate was started before the ceiling; its recorded completion
was12:53:39.662 UTC, six seconds beyond the planned ceiling. No further checks
or native task were selected. Restoration always completes. Release, broad current-stack gameplay, alternate
callers, old-save surfaces and final art acceptance are deferred explicitly.
