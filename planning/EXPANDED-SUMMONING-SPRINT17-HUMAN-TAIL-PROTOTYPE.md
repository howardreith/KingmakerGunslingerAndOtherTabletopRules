# Sprint 17 Salamander original human/tail prototype

Status: EXACT bc7 REQUEST-LOCAL BINDING RUNTIME FAIL/RESTORED; Sprint17 NOT QUALIFIED. Production Salamander
identity, placements, profile and prefab remain unchanged. A separate original
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

## Next exact work

Qualify/commit/push this diagnostic, rebuild one exact candidate and run guarded
working-save-smoke plus the unchanged bounded human review. Resolve the observed
operand before changing binding. Preserve native two-handed spear handling
and all native references. Prove the original tail with real commands, then
finish Salamander's printed mechanics. Only a complete same-artifact Sprint17
hidden/publication gate can publish the32 withheld snake roots.
Sprints14–16 remain complete;976published+29wrappers=1005visible.
Then full Phase2B closure and STOP for owner review. Phase2C authorized but
deferred until Phase2B owner acceptance; no Sprints18–22 under this mission.
