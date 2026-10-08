# Sprint 17 Salamander original human/tail prototype

Status: EXACT6ae91 BOUNDED HUMAN15/15 PASS/RESTORED. FullSprint17 NOT QUALIFIED. Production Salamander
identity/placements/prefab unchanged; spear is now manufactured. A separate original
human/tail mesh is packaged for guarded research only.
Laptop PR26 only; DATA salvage-only, ZERO PORTS.

## New bounded implementation

`generate_human_salamander.py` reuses the previously captured exact human
Guard Captain anatomical frames, not native geometry, pixels or animation
curves. It accepts only blueprint86dc43534645e234eb35431131e3b669,
prefabced3729f4b4abab4da4ef63d8489f857, the1776-slot/177-Transform palette
and the exact combined renderer. Every source slot is accounted for.
The closed26-driver anatomical subset requires finite/invertible frames,
exact parent links and agreement of every recorded duplicate within1e-5.
All17 disagreeing weapon-storage groups, cape and leg branches are excluded.
This is not blind first-slot deduplication or a new donor census.

The original2198-vertex/2216-polygon body has two arms, three independently
weighted digits per hand, a crested reptilian head and a continuous rising
waist/coil. Ten project-owned tail drivers complete the36-driver skin.
There are no visible legs, armor, native body or embedded weapon geometry.
The separate closed lower jaw is not articulated; no bite is claimed.
Original UVs reuse the already project-authored Salamander paint unchanged.

A fixed1.4second original tail sweep has an authored0.60second act point,
a0.55–0.80second extended pose, and full recovery. All ten segment lengths
remain constant in the recipe's141 sampled poses. The curve takes time only:
no target position/distance, reach, attack roll, collision or native curve
is an authoring input. This does not prove contact with a live opponent.

Private output: `artifacts/private-sprint17/human-salamander-v2`.
Rest/strike/recovery independently export the same original mesh/curve JSON:
`2c4b76f0bbf0691ae0f9d9fc2340e77f958b999e27dda2628c82b26642c16670`.
The paint remains
`127e9fe76ad853be6dd7653a27ef9f526d7ffedbcbe9c942c244d2e15cb10aa0`.
Private native metadata remains
`eb0d5eaaf4bec8bf68506d42f299ded5d3d86cb676ff72e53f15d0cfbce9bfd2`;
its coordinates/matrices never enter the export. Only original tail points
and original mesh/UV/weights/curve data are exported. The current binding packages
this exact mesh separately; the original private exports remain preserved.

## Offline checks and honest limits

Nine Blender authoring tests PASS, including exhaustive selected-driver
rejections, complete slot coverage, refusal of ambiguous identities/parents,
exclusion of all17 storage groups, finite closed repeatable geometry,
normalized one/two-bone weights, valid UVs, continuous waist/coil, target-free
length-preserving sweep/recovery and no native-bone movement in the posed preview.
The first timeline test compared a double decimal literal with a float Vector
exactly. Converting its expected operand to the same representation corrected
the fixture; product animation math did not change. The failed log is retained.

The initial v1 rest surface extended0.0243m below its authoring plane. v2 lifts
only the original lower rest shape and evens the waist control-point spacing.
Its minimum authored Y is0.03672357m; no native bone or runtime ground clamp
changed. v1 and all older failed Lizardfolk prototypes remain preserved.
All32 private panels are complete:16 rest clay/silhouette/textured/unlit and
8 each strike/recovery clay/textured. A separate29-pose evaluated-mesh audit
is finite throughout, minimum authored Y0.03672358m. Its log hash is
`d9dd93ac28c5cd040cc1adbcc241e2fc3bae9f588ccf4fe9d5448a802ef7f778`.
The extended tail retains some visible skinning undulation; that and live
upper-body/weapon/ground behavior remain integration review items, not waived.
HumanReview: NOT_PERFORMED_NONBLOCKING.
Neither an offline image nor a bind-compatible skeleton qualifies gameplay.

Earlier private authoring gate PASS:280focused/2082unfiltered81.5s; complete repository/static/
icon/manifest, clean14-reference Release and deterministic strict320 package178.2s.
Source gate log `artifacts/sprint17-human-tail-source-gate.log`. No Kingmaker launch, installation
transaction, save write or production runtime binding occurred for this work.
The last guarded result remains exactfb9 snake smoke11/review82 PASS and
exact restoration05:38:51UTC; its bounded qualification does not transfer.

## Exact native action audit, not adoption

Read-only pinned-assembly inspection found a native Tail special-attack type.
`UnitAnimationActionSpecialAttack` is subclassable; OnStart/OnUpdate are
virtual, while GetDuration reads the selected clip length. UnitAttack selects
the special action through the weapon's native animation type and uses that
duration to set the handle SpeedScale. The handle advances on the native
manager delta; interruption/release callbacks remain available.

The unmodified special action falls back after0.1s if ActiveAnimation is null.
That fallback must not be mistaken for original tail playback. The inactive
Salamander-only bridge below bypasses it in source, but must demonstrate actual
owned-clip evaluation, command-act/contact correspondence, interruption,
pause/speed behavior and cleanup. No human clip may be transplanted to the
Lizardfolk rig or relabeled as Tail. Native spear actions must remain native.

Unity2018.4 permits runtime curve creation only on legacy clips
([SetCurve](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/AnimationClip.SetCurve.html)).
Its [Animation.Sample](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/Animation.Sample.html)
evaluates the current animation state. A creature-owned legacy component can
therefore be investigated on only these original tail drivers, using native
handle time; this still requires runtime proof and is not an adopted or
qualified carrier. No legacy-flag workaround, editor activation, global
animation rewrite or general arbitrary-limb subsystem is authorized.

Private audit script hash:
`dea7e48bae93688c681c065540239176911b66bd1f03be97524cab1e986e2b88`.
Private selected IL capture hash:
`4235d9edfcdf80ad50c7507f468c078ea80a8904dd92946b7f704238495b1791`.
Pinned native assembly hash:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
No native methods were invoked, no action instantiated/adopted, and no native
IL, matrices, clips or proprietary assemblies are redistributed.

## Earlier inactive action bridge — source PASS, no runtime adoption

The source-only slice implements `SalamanderTailAction` and its six
behavior-tested playback/identity cases. It accepts only the named request-local
Salamander clone, exact human prefab/project spear, one owned legacy clip and
ten exact original child drivers. No production hook or fixture calls it yet.
Its inherited native variant entry retains clip-duration selection. Only native
handle time advances the owned legacy sample; a prepared time cannot fire an
event until the exact owned clip/state is acknowledged. Duplicate, invalid,
rewound, interrupted and foreign-handle paths fail closed. Native UnitAttack
still owns all targeting, attack rolls, reach, damage and on-hit riders.

The action restores/stops only its own component on finish/interruption. Failure
interrupts only an exact native UnitAttack referencing this handle, through the
public native command API, then releases that handle. No queue/brain/global
animation mutation and no fabricated null-clip IsActed fallback.

Initial compile attempts exposed two access details: GetVariantsCount is a
final interface implementation despite its reflection IsVirtual flag, and
MarkInterrupted is assembly-internal. The bridge now inherits variant counting
and uses the public exact-command interruption seam. Both failed compile logs
are retained; the third pinned-reference Release compile passes. The six new
domain tests pass. The initial complete gate286focused/2088full81.4s/175.2s
passed and is archived. A follow-up exception-path review added native exact-
command interruption/handle release in finally blocks even if resetting the
owned clip throws. Start/update exceptions fail closed and stop only owned
playback. The corrected complete gate PASS:286focused/2088unfiltered83.7s;
repository/static/icon/manifest, clean14-reference Release and strict320177.9s.
Logs `artifacts/sprint17-salamander-tail-bridge-final-{focused,gate}.log`.
Live clock, playback, contact and cleanup remain NOT QUALIFIED. Merely
assigning IsActed is not a runtime playback/contact assertion.

## Request-local body/action binding — corrected source PASS

Source parent19949d30. The new closed attachment accepts only the named
request-local clone on the exact human prefab and project spear. It checks
all selected duplicate live Transform identities/parents/binds, rejecting
ambiguous anatomy. The native human set is cloned only as an instance-owned
container: all24 native actions and transition/startup references remain
unchanged; exactly one owned Tail is appended. Native equipment remains native,
with no weapon mesh transplant, palm remount or native-bone mutation.

The original ten drivers are direct children of an owned legacy Animation
root. Only project-authored absolute positions/delta quaternions enter its
fixed11-frame clip. Interpolation is explicitly linear, so the earlier141
continuous recipe samples are not a proof of every runtime interpolated pose.
Native handle time alone samples the clip. All own resources are captured for
post-swap rollback and native destruction; no native assets may be destroyed.

The first source gate passed290focused/2092full82.3s/complete179.1s/strict320,
but a final payload audit caught sorted export order versus authoring order.
That undeployed artifact is retained in
artifacts/sprint17-human-binding-before-palette-audit. Binding and independent
contact measurements now map exact names; all36 skin indices are behavior-tested.
The binding-corrected gate passed290focused/2092unfiltered83.0s/178.1s,
but independent ZIP inspection found only320 members: the explicit build/package
lists had omitted the new mesh despite the project wildcard. That artifact is
preserved in artifacts/sprint17-human-binding-corrected-source-pass and was
never deployed. Build/package/strict inventories now explicitly require the
separate mesh and validate its exact hash. The old missing-member ZIP is
rejected by the corrected validator. One inherited current-package count
guard required its corresponding69-to70 addition; immutable historical
Phase1/published metadata are unchanged. The rejected source gate is retained.

Final corrected complete source gate PASS:290focused/2092unfiltered81.9s;
repository/static/icon/manifest, clean14-reference Release, deterministic
strict321 package179.0s. Independent ZIP inspection confirms the270010-byte
human mesh member. Nine Blender authoring tests pass. Earlier binding logs:
artifacts/sprint17-human-binding-corrected-{focused,gate}.log.
Final package-complete logs:
artifacts/sprint17-human-binding-package-corrected-{focused,gate}.log.

The existing closed disposable-expanded-summoning-serpentine-bodies request
now isolates this new hybrid research. It records native appearance/control,
every native renderer, rollback, finite rest/movement, a real full UnitAttack,
native spear clips/two-hand geometry, owned-tail playback/weighted contact,
and exact destruction/borrowed-asset survival. Contact reads the distal half,
not merely the waist. IsActed alone cannot pass. All older Lizardfolk evidence
remains failed and preserved; no snake qualification is rerun or aggregated.

## First human binding runtime — FAIL, exactly restored

Exact bc7a7c9adfa893b6a916dc017c6a7139b2bb3ef6 passed every prelaunch gate:
290focused/2092unfiltered81.7s/complete176.3s/clean14/strict321,
515preflight/168orchestration/17provenance, persistence11/6/3/56,
crowd8/11 and launcher14accepted/13rejected. Its immutable ZIP/DLL/source,
fresh Steam640820 process/request identities and restoration hashes are in
[the curated failed evidence](EXPANDED-SUMMONING-SPRINT17-HUMAN-BINDING-FAIL-EVIDENCE.json).

Working-save smoke11/11 PASS. Hybrid review4/6 FAIL (expected15): native
settlement/control and all26 anatomical duplicate/parent/bind checks passed.
Before allocating or swapping any owned resource, the combined guard rejected
either a non-root human set or a non-null effective Tail action. The original
message cannot distinguish the operands. Zero rollback resources means the
post-swap rollback drill was NOT reached, not that rollback was qualified.
The exception assertion is a consequence of that same rejection.
Body binding, tail playback, movement/contact and destruction remain unqualified.
Environment/fixture cleanup passed. ZERO save writes. The actual snapshot
136files/Info0.0.117/tree216A9DC2...AF3 was exactly restored at
2026-10-07T07:36:19.0667770Z; no game, runtime lease or staging remains.

This is an unresolved production/integration contract, not an owner blocker
or accepted engine limitation. Native IL metadata confirms GetAction(Tail)
has an exact declared-type lookup without a native fallback. The previously
captured detached action names do not prove their AttackType values or the
live actor's effective lookup. No donor census is repeated.

The next narrow diagnostic records both guard operands and raw/effective
Tail identities, plus the patch registries of exactly three relevant getters,
before attachment. It invokes no patch/action and mutates no native set.
A four-case behavior test preserves the original guard truth table and
short-circuit. No guard relaxation, extra wait, new action or production hook.
Complete diagnostic source gate PASS:291focused/2093unfiltered84.6s;
repository/static/icon/manifest, clean14-reference Release, deterministic
strict321 package183.0s. Logs artifacts/sprint17-human-action-guard-source-
focused.log and artifacts/sprint17-human-action-guard-source-gate.log.
This does not diagnose the runtime operand until the exact new artifact runs.

## Guard diagnostic attempt — fixture FAIL, exactly restored

Exact7590ce68319d747eea90f4a9224e4a628861b06a passed291focused/2093full81.5s,
complete175.9s/clean14/strict321 and all existing prelaunch harness checks.
Smoke11/11 PASS; hybrid4/5 FAIL(expected15), before attachment.
Harmony12.Converters.ToHarmony12 threw NullReferenceException from GetPatchInfo.
Because provenance was queried before publishing the boundary object, the
already-read core facts were lost. Neither the target getter nor the original
binding-guard operand is established. This is a fixture defect, not a new
body failure, gameplay qualification or owner-level blocker.
[Exact evidence and restoration](EXPANDED-SUMMONING-SPRINT17-HUMAN-GUARD-DIAGNOSTIC-FAIL-EVIDENCE.json).
ZERO save writes; actual136/0.0.117/tree216A...AF3 restored08:16:10.2330898UTC.
No game/runtime lease/staging remains.

Existing qualified teleportation observers already use Harmony's patched-method
registry before GetPatchInfo to avoid this compatibility converter's null-record
failure. The bounded repair retains membership for only our three exact getters,
never querying an absent target. Registered-method exceptions still fail the
scenario; none is relabeled absent or PASS. Core observations are published
before provenance, so a later metadata failure cannot discard them.
The behavior test verifies zero queries for absence, exactly one registered
query with unchanged result, and propagation of registered failures.
The native human binding guard and all15 runtime assertions remain unchanged.
Registry-bounded source gate PASS:292focused/2094unfiltered82.5s,
complete repository/static/icon/manifest, clean14-reference Release and
deterministic strict321 package177.5s. Logs:
artifacts/sprint17-human-guard-registry-source-{focused,gate}.log.

## Exact raw/effective boundary — cause identified

Exactd989915120c7380a20ba33e83177c0cd20392dea passed292focused/2094full80.7s,
complete176.4s/clean14/strict321 and every harness prelaunch gate.
Smoke11/11 PASS, hybrid4/6 FAIL(expected15); exactly restored08:41:31.7143709UTC,
ZERO save writes, no game/runtime lease/staging.
[Exact bounded evidence](EXPANDED-SUMMONING-SPRINT17-HUMAN-TAIL-FALLBACK-EVIDENCE.json).

The live manager and root are the same MyAnimationSet instance457560, with
24actions and one transition. Its raw specials are Claw/Bite/Gore/Slam: no Tail.
GetAction(Tail) nevertheless returns its exact raw MyAnimationSet_Slam,
declared Slam. The registered CallOfTheWild.UnitAnimationManager_GetAction
postfix is the effective fallback source. AnimationSet/AttackType getters are
unpatched; their metadata calls are correctly skipped. Both core and provenance
are complete and unchanged after reading. The original pre-allocation guard
therefore conflated an effective fallback with a real existing Tail carrier.
Rollback/body/playback/contact remain unqualified, not waived.

Correction: keep exact human-root identity, reject every raw Tail, and allow
only a null lookup or this identical raw native Slam reference declared Slam.
All other effective results still fail. Append only the original owned Tail,
preserving every native action and clip; the strict post-install lookup must
return that owned action. No native Slam relabeling/playback/adoption, CoTW
patch change/disabling, or global animation rewrite.
The behavior test covers all eight guard states and exact-versus-foreign
fallback reference identity. All15 runtime assertions are retained.
Read-only pinned type metadata also confirms native Transition derives from
UnityEngine.ScriptableObject; no transition cloning/ownership change was made.
Fallback-boundary source gate PASS:292focused/2094unfiltered83.7s,
complete repository/static/icon/manifest, clean14-reference Release and
deterministic strict321 package178.0s. Logs:
artifacts/sprint17-human-native-slam-source-{focused,gate}.log.

## Exact cd709 native crash — preserved, not qualified

Exact cd70979f37af00e26fcc4247ce2f30579bb1f592 passed every prelaunch gate:
292focused/2094full84.0s/complete180.6s/clean14/strict321 plus all harness checks.
Smoke11/11 PASS; human-tail review ended in a native Unity access violation
before a final result. No body/playback/contact/cleanup qualification is claimed.
[Exact crash and restoration](EXPANDED-SUMMONING-SPRINT17-HUMAN-NATIVE-CRASH-EVIDENCE.json).
The log reports a zero-sized D3D buffer immediately before the crash.
Assigning an empty mesh to the native cape is a bounded suspect, not a proven
causal stack or memory-exhaustion diagnosis. A supporting rest image is not
mechanical proof. Smoke audited ZERO save writes; the crashed review's final
write audit is absent/UNKNOWN. No save-file inspection or manual surgery.
Actual136/Info0.0.117/tree216A9DC2...AF3 restored09:08:27.5428737UTC;
no game/runtime lease/staging. Raw logs/dump remain private, hash-preserved.
The d989 raw/effective Tail finding remains valid: exact native human root,
NO raw Tail, CoTW exact native-Slam fallback. cd709 keeps all24 native actions,
rejects raw Tail/foreign lookup, and still requires its owned Tail afterward.
No CoTW mutation, borrowed Slam adoption, production Salamander change or waiver.

The exact native error is UnityPlayer.dll access violation0xc0000005 at
offset0x0000000000c10d4d, PID31096. Native crash diagnostics show13968MiB
physical memory free; this is not evidence of OOM. The original zero-vertex
suppression mesh is the focused next investigation. No unrelated global
renderer/cloth/action changes or repeated unchanged-candidate launch.

## Bounded render correction — source PASS, runtime pending

The bounded null-geometry correction is SOURCE PASS, NOT RUNTIME QUALIFIED:
293focused/2095unfiltered82.8s/complete177.5s/clean14/strict321.
Only the exact zero-bone Cape_Red_M(Clone)/CP_Cape2Sided_M_Any may be suppressed;
sharedMesh=null, native bones/root/renderer flags unchanged, exact mesh restored.
The unused empty mesh is no longer allocated;29 distinct base owned objects
plus controller clones must all be destroyed. All15 runtime assertions remain.
Atomic request-correlated stage records retain observations but explicitly do
not qualify or replace the final save-write audit. One actual native frame must
advance after binding before the supporting camera/pose observation.
The prior supporting image is visibly malformed; finite arithmetic alone is
not visual acceptance. Cause remains unproved; no geometry/shader/cloth rewrite.
Production Salamander and every accepted decision remain unchanged.

Source parent3dfb24171ef93c71eac04a520c5de46e0045bb8c. Logs:
artifacts/sprint17-human-null-geometry-frame-source-{focused,gate}.log.
The earlier pre-frame-observation source pass293/2095/177.7s is separately
archived; it was never launched. New exact-head runtime qualification is required.

## Exact487 runtime — bounded progress, three mandatory failures

Exact487309ff2e750b5b44f120929e802c977c01a0b6 completed without a native crash.
All prelaunch PASS:293focused/2095full82.4s/complete175.7s/clean14/strict321,
plus515preflight/168orchestration/17provenance and persistence/crowd/launcher gates.
Smoke11/11; full human review12/15 FAIL, NOT QUALIFIED.
[Exact result and restoration](EXPANDED-SUMMONING-SPRINT17-HUMAN-NULL-GEOMETRY-EVIDENCE.json).
Rollback/body binding/post-native-frame rest/movement/original-tail playback/
native-reference isolation/destruction/environment/fixture cleanup pass.
Both rollback and destruction reclaim30 owned objects;158 borrowed objects
survive.17 atomic stages are request-correlated, explicitly non-qualifying.
The audited cape has null geometry, unchanged native bones/root/renderer flags.
Actual136/Info0.0.117/tree216A9DC2...AF3 restored09:58:17.2992027UTC;
ZERO audited save writes, no game/runtime lease/staging. Earlier crash retained.
The real full attack produced ONE native spear hit plus ONE original tail hit,
not two spear iteratives. ConfigureSummonWeaponType marks the spear natural;
correct only Salamander's manufactured-weapon classification, not native clips.
Spear target gap0.03135m passes; wrist-origin gaps0.10288/0.10368m and distal-tail
gap0.46630m fail. Audit grip surfaces and the actual tail path separately; no
threshold waiver. The tail follows unit heading; do not reparent it on that theory.
Optional post-native-frame art shows the original body, not the earlier shredded
frame; neither that image nor finite arithmetic qualifies full visual/contact work.
Salamander's production prefab/profile/identity remain unchanged at this boundary.

## Historical spear/contact correction — source PASS before exact807

Source parent1ef236cb0bfeadf08fd291f01be8edd50fb9c15a.
[Exact source/offline hashes and failure dispositions](EXPANDED-SUMMONING-SPRINT17-HUMAN-CLOSE-CONTACT-SOURCE-EVIDENCE.json).
294focused/2096unfiltered83.4s; complete180.7s, clean14-referenceRelease,
deterministic strict321. No runtime launch or transfer of prior12/15 evidence.

Native read-only attack-count inspection proves the manufactured-weapon branch
retains BAB iteratives. Only the Salamander spear is changed from natural to
manufactured. No added attack, native animation change, or Pixie-bow change.
The real full-attack assertion now also requires live BAB8 and non-natural
spear type, alongside two native spear events and one owned tail event.

The original wrist-origin measurement is retained. Grip additionally uses only
the exact hand/three-finger skin vertices (seven drivers per side, influence
at least0.5, minimum8 vertices). No forearm, opposite hand or unknown driver.
The8cm grip and25cm striking limits remain unchanged. Target bounds, owner
heading and closest striking/target points are recorded at the actual event.

The target-free1.4second/.6act original tail sweep now keeps its proximal coil
and brings the distal half through close-melee space. No target/reach/damage
input or native bone changes. Three exports matchb5524a69...1c0f19; only
originalTailSlap differs from the prior JSON, not geometry/UV/paint/binds.
The first revised quaternion wind-up dipped6.13cm below the authoring plane;
privatev3 is preserved and rejected. Fixed yaw/elevation authoring instead
passes10 tests and29 actual evaluated-mesh poses, minimumY0.03672358m.
The new permanent authoring test covers ground clearance and native isolation
through wind-up/recovery.32rest/strike/recovery panels are retained; the
offline coil is continuous and finite, with angular bends at extreme poses.
These panels do not establish live interpolation/contact or owner art approval.

The first complete source gate correctly rejected the stale packaged-export
checksum. Both failed ZIPs/log remain archived. The pin now names this reviewed
export, and the full gate passed again; no check disabled or failed result waived.

## Exact807 level-override failure — restored, narrow fixture correction source PASS

Exact80779026b3718083aff76819bbbb90237f759335: smoke11/11, human12/15 FAIL.
[Exact runtime, restoration and level-override correction](EXPANDED-SUMMONING-SPRINT17-HUMAN-LEVEL-OVERRIDE-EVIDENCE.json).
All exact prelaunch PASS294/2096/177.4s/clean14/strict321 and harness gates.
The manufactured spear now produces native iteratives, but the fixture spawned
BAB20 from ruleLevel20 instead of printed8: FOUR spear events plus ONE owned-tail event.
All four spear clips are native. The three failed predicates still require2+1;
none is waived. The native command ended Interrupt after the five events,
not claimed Success or whole-Sprint qualification.

All rule-frame spear target gaps~0.03135m; weighted grips0.00557–0.01778m
left/0.006416–0.006417m right; tail337points gap0 at rule and paired frame.
Original tail moves3.5892m with one owned act event. One native follow-through
paired left grip is0.08171118m; do not claim sustained8cm grip. Predicate timing
remains the actual rule event, not a cherry-picked peak or widened threshold.
Rollback and native destruction reclaim30owned objects;158borrowed survive.
Actual136/Info0.0.117/tree216A9DC2...AF3 restored11:03:17.7293829UTC;
ZERO save writes, no game/runtime lease/deployment staging;17correlated stages.

Pinned native AddClassLevels.GetLevels proves positive RuleSummonUnit.Level
overrides blueprint levels. The fixture's20 was NOT caster level. Correction:
pass0 so the blueprint supplies8; record authored/actual levels and require
those plus base/modifiedBAB8 before accepting2spear+1tail. No stat overwrite,
production/asset/threshold change or native animation modification.
This request-local correction is SOURCE PASS294focused/2096full81.8s,
complete177.1s/clean14/strict321; corrected exact-head runtime is still PENDING.
All earlier failed artifacts/diagnostics preserved. Sprints14–16 COMPLETE;
Sprint17 NOT QUALIFIED;32roots hidden;976published+29wrappers=1005visible.
HumanReview: NOT_PERFORMED_NONBLOCKING.

## Exact801 printed levels and attacks pass; grip remains open

Exact801ed1f3638e608df082db6a8e70b28bacef927b: smoke11/11, human14/15 FAIL.
[Exact printed-level runtime and restoration](EXPANDED-SUMMONING-SPRINT17-HUMAN-PRINTED-LEVEL-RUNTIME-EVIDENCE.json).
All exact prelaunch PASS294/2096full82.5s/complete178.8s/clean14/strict321,
515preflight/168orchestration/17provenance and all persistence/crowd/launcher gates.
The zero summon-level override now preserves authored/character/baseBAB/modifiedBAB8.
The native command succeeds with exactly TWO spear events plus ONE original tail.

Only attack-contact fails: second left weighted hand-surface gap0.0851590857m
exceeds the unchanged0.08m limit; first is0.07913925m. Both native spear clips
are Human_2H_spear_attack_02, already TransitioningOut at weight0.9393939.
Same-frame rendered gaps grow to0.1625474/0.196584031m. This suggests a timing
or original-geometry issue, not a proved cause. No peak cherry-picking, limit
change, forced native pose, favorable-clip rerun or accepted limitation.
Right grips~0.006417m, spear target gaps~0.03135m, original337point tail gap0;
one owned tail event/movement3.5892m. Other14 checks PASS, not aggregate qualification.

Rollback/native destruction reclaim30owned objects;158borrowed survive.
Actual136/Info0.0.117/tree216A9DC2...AF3 restored11:38:07.0136916UTC;
ZERO save writes; no game, completed/released lease, no deployment staging.
Seventeen correlated stages and all earlier failures/artifacts are preserved.
Sprints14–16 COMPLETE; Sprint17 NOT QUALIFIED;32roots hidden;
976published+29wrappers=1005visible. HumanReview: NOT_PERFORMED_NONBLOCKING.

Read-only same-pose diagnostic now SOURCE PASS296focused/2098full82.7s,
complete180.0s/clean14/strict321. It retains the borrowed native hand's
weighted surface privately and compares it with the original hand against the
same native spear on the same actor/frame. LateUpdate/end-of-frame samples
are capped512; exact native cached event times/delegate identities are read,
never invoked. No native geometry export, pose/weapon write or global hook.
Two weight-selection behavior tests and an independent original-distance
agreement check are added; all15 runtime checks and8cm/25cm limits remain.
Two prelaunch test-count metadata rejections are preserved and synchronized
to2098. No runtime for this diagnostic yet; exact clean-head gate next.

## Exactd5bc diagnostic CPU-readability failure — no attack result

Exactd5bc656519300da5753248a62e602da7f1aed525: smoke11/11 PASS; human
diagnostic FAIL after5 assertions (4PASS), not the required15-check review.
[Exact native-readability failure and restoration](EXPANDED-SUMMONING-SPRINT17-HUMAN-GRIP-READABILITY-EVIDENCE.json).
All exact prelaunch PASS296/2098full82.8s/complete179.0s/clean14/strict321,
515preflight/168orchestration/17provenance and all persistence/crowd/launcher gates.

The native combined body reports2268vertices/1776palette slots, but its CPU
vertex getter rejects access because isReadable=false (preserved game log).
The new read-only control incorrectly assumed direct vertices were available;
it fails before original attachment, rollback or attacks. Classification:
FIXTURE_OBSERVATION, not a proved gameplay regression. No grip-control result.
The last full review remains exact801 human14/15, with the0.085159m left
grip failure against the unchanged0.08m limit. That failure is NOT resolved.

Actual136/Info0.0.117/tree216A9DC2...AF3 restored12:19:30.7905394UTC;
ZERO native save writes; fixture/environment cleanup PASS; no game,
completed/released lease/recoveryfalse, no deployment staging.
Four atomic stages, the native log and all earlier artifacts are preserved.
Sprints14–16 COMPLETE; Sprint17 NOT QUALIFIED;32roots hidden;
976published+29wrappers=1005visible. HumanReview: NOT_PERFORMED_NONBLOCKING.

Owned BakeMesh control repair is now SOURCE PASS297focused/2099full82.6s,
complete176.9s/clean14/strict321. It replaces the invalid direct native vertex
read with two disabled project-owned renderers outside the live view.
Each retains the exact borrowed mesh/bones and fills only an owned snapshot;
original/native import flags, renderers, clips, bones and weapon are untouched.
The owned control must match every finite world-matrix entry within1e-5;
original baked grip must agree with the independent weighted-world measurement.
All eight bake resources plus both frame probes are captured for exact cleanup;
native asset references must survive. No geometry export or8cm/25cm waiver.
A new finite-frame rejection test and both count pins are synchronized to2099.
The exact2dace attempt below failed before BakeMesh; last full grip remains801 FAIL.

## Exact2dace hand-selector failure — restored

Exact2daceb68fa5ee50ee2c1205e876d47dc9d5eadea: smoke11/11 PASS; human
diagnostic4/5partial FAIL, before the required15-check attack review.
[Exact hand-selection failure and restoration](EXPANDED-SUMMONING-SPRINT17-HUMAN-GRIP-BAKE-RUNTIME-EVIDENCE.json).
All exact prelaunch PASS297/2099full83.1s/complete177.1s/clean14/strict321,
515preflight/168orchestration/17provenance and all persistence/crowd/launcher gates.

Native2268vertex/1776palette weight/bind shape checks pass, but one hand
selection is unavailable. Side/count/rejection distribution were not recorded.
The control fails before allocation, BakeMesh, attachment or attacks; no new
hand comparison or gameplay result. Classification: FIXTURE_OBSERVATION.
Last full801 remains14/15: left-grip0.085159m exceeds unchanged0.08m.
Neither diagnostic failure resolves that defect or qualifies the BakeMesh seam.

Actual136/Info0.0.117/tree216A9DC2...AF3 restored13:00:45.4757100UTC;
ZERO native save writes; fixture/environment cleanup PASS; no game,
completed/released lease/recoveryfalse, no deployment staging.
Four atomic stages and every failed artifact are preserved.
Sprints14–16 COMPLETE; Sprint17 NOT QUALIFIED;32roots hidden;
976published+29wrappers=1005visible. HumanReview: NOT_PERFORMED_NONBLOCKING.

Bounded weight-census/failure-isolation change is now SOURCE PASS:
298focused/2100full83.5s/complete180.4s/clean14/strict321.
It records both hand selectors' rejection reasons, weight sums and positive
driver aggregates before failure. Original/native control failures stay sticky
and fail qualification, but no longer erase native attack/event-timing evidence.
No normalization, native pose/import writes, art/gameplay or8cm/25cm changes.
Every actually allocated control/probe is tracked; cleanup is not confused
with whether an unavailable control was ever created. One new behavior test
preserves selection while distinguishing rejection causes; count pins2100.
Exacta5a1 runtime below now retains15 checks but remains FAIL.

## Exacta5a1 full attack retained; contact and serializer remain open

Exacta5a1e962e688c80a5acd21f64ab394eae1165314: smoke11/11 PASS;
full human review13/15 FAIL, with complete native2spear+1tail command Success.
[Exact full review, failures and restoration](EXPANDED-SUMMONING-SPRINT17-HUMAN-WEIGHT-CENSUS-RUNTIME-EVIDENCE.json).
All exact prelaunch PASS298/2100full85.5s/complete182.6s/clean14/strict321,
515preflight/168orchestration/17provenance and all persistence/crowd/launcher gates.

Native01 left grip0.005556m passes; native02 left0.08940738m exceeds0.08m
(end frame0.200967208m). No asset/pose change, threshold waiver or qualification.
The second failure is diagnostic serialization: the active runtime serializer
turns counter dictionaries into arrays, so JObject.FromObject rejects them.
Both controls stop before BakeMesh. Native2268 weight sums are finite/normalized;
selector counts remain unknown. Failure isolation now retains all15 checks,
144bounded timing rows and both cached native act events rather than aborting.
Native02 starts transitioning out before its0.734528542s act event; actual
command resolves next frame. Timing is proved, native-hand fidelity is NOT.

Rollback reclaims30objects; native destruction32actual objects/158borrowed alive.
Actual136/Info0.0.117/tree216A9DC2...AF3 restored13:33:09.9424841UTC;
ZERO native save writes; no game/runtime/compatibility lock or staging;
lease Completed/recoveryfalse. All17stages/failed artifacts retained.
Sprints14–16 COMPLETE; Sprint17 NOT QUALIFIED;32roots hidden;
976published+29wrappers=1005visible. HumanReview: NOT_PERFORMED_NONBLOCKING.

Counter JSON repair is now SOURCE PASS299focused/2101full83.0s,
complete179.0s/clean14/strict321. Explicit ordered scalar JProperty construction
replaces only dictionary FromObject calls. The new behavior test reproduces
the old array-converter failure, preserves exact/zero/empty counters without
calling the converter, and restores process defaults afterward. Production
does not change global settings. No model, pose, selector or threshold change.
Exacted738 below verifies the JSON repair, not the missing native hand control.

## Exact6ae91 bounded human/original-tail seam PASS

Salamander profile/heat/tail-grab source checkpoint based on65bc0536:
311focused/2113unfiltered PASS, repository/icon/manifest PASS, clean14-reference
Release and strict321-member package PASS (181.4s). This development artifact
is NOT a clean exact-head runtime candidate and was NOT deployed.
[Source evidence](EXPANDED-SUMMONING-SPRINT17-SALAMANDER-PROFILE-SOURCE-EVIDENCE.json)
records source fingerprint, DLL/MVID/ZIP, private engine-audit hashes and exact scope.

Existing Salamander unit/weapons/traits GUIDs and five published roots remain.
Only TailType and UnitType append:2911stable IDs/2909active/2reserved.
The source now supplies printed racial HP/save/rank contributions, native feats,
Medium2d6 ten-foot tail type, owner-scoped1d6fire weapon heat, and tail-only
project grab/live physical-plus-fire constrict. Per-rule heat and per-round
maintain guards reject duplicates. Native modifiers and borrowed facts remain live.
All these new mechanics, defenses/skills breakdowns, real commands/AI, new UI
consumers and persistence are NOT RUNTIME QUALIFIED. Original human/tail
production adoption is still open; no prefab, geometry, pose or clip change here.

Earlier exact6ae91f266b203a98a0f297b24f3a70c785458502 remains bounded prototype
PASS: fresh smoke11/11 +human15/15, both native spear variants,144paired grip
rows, original tail, movement/rollback/destruction; legacy both-hands-at-rule
predicate remains FALSE. [Exact bounded evidence](EXPANDED-SUMMONING-SPRINT17-HUMAN-NATIVE-GRIP-PASS-EVIDENCE.json).
It does not qualify this changed production source. Earlier failures remain FAIL,
not open ownership/design blockers. No new runtime transaction or save write.
Last actual restoration15:14:52UTC:136files/Info0.0.117/tree216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3.

Source-lock heartbeat expired15:53:01UTC. Edits paused; no competitor, game,
runtime lock/staging or remote motion found. Old receipt preserved; same owner
PID31796 reacquired exclusivity with keeper37540/start15:54:48.8399249UTC.
Sprints14–16 COMPLETE/PUBLISHED. Sprint17 NOT QUALIFIED;32snake roots hidden.
976published+29wrappers=1005visible. HumanReview: NOT_PERFORMED_NONBLOCKING.

## Next exact work

Next: adopt the qualified human/original-tail seam ONLY for the exact production
Salamander, with native settlement/fallback/resource cleanup and preserved GUIDs/
five roots. Complete guarded mechanics/command/AI/UI/routes/persistence observers,
freeze a clean committed candidate, rerun all exact prelaunch gates and one full
same-artifact Sprint17 hidden batch. Publish32snake roots only after all gates,
exercise affected Salamander routes, then fullPhase2B closure and STOP owner review.
Phase2C remains authorized but deferred; no18–22 in this mission.
Preserve all native two-handed weapon handling and native references.
HumanReview: NOT_PERFORMED_NONBLOCKING.
Preserve native two-handed spear handling
and all native references. Prove the original tail with real commands, then
finish Salamander's printed mechanics. Only a complete same-artifact Sprint17
hidden/publication gate can publish the32 withheld snake roots.
Sprints14–16 remain complete;976published+29wrappers=1005visible.
Then full Phase2B closure and STOP for owner review. Phase2C authorized but
deferred until Phase2B owner acceptance; no Sprints18–22 under this mission.
