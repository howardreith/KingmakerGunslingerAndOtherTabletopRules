# Sprint 17 Salamander original human/tail prototype

Status: PRIVATE AUTHORING ONLY; Sprint17 NOT QUALIFIED. Production Salamander
identity, placements, profile, prefab and packaged assets remain unchanged.
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
and original mesh/UV/weights/curve data are exported. No prototype is packaged.

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

Full source gate PASS:280focused/2082unfiltered81.5s; complete repository/static/
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
That fallback must not be mistaken for original tail playback. A future
Salamander-only action must bypass that fallback with real owned-clip
evaluation and demonstrate command-act/contact correspondence, interruption,
pause/speed behavior and cleanup. No human clip may be transplanted to the
Lizardfolk rig or relabeled as Tail. Native spear actions must remain native.

Unity2018.4 permits runtime curve creation only on legacy clips
([SetCurve](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/AnimationClip.SetCurve.html)).
Its [Animation.Sample](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/Animation.Sample.html)
evaluates the current animation state. A creature-owned legacy component can
therefore be investigated on only these original tail drivers, using native
handle time; this is an inference requiring runtime proof, not an implemented
or qualified carrier. No legacy-flag workaround, editor activation, global
animation rewrite or general arbitrary-limb subsystem is authorized.

Private audit script hash:
`dea7e48bae93688c681c065540239176911b66bd1f03be97524cab1e986e2b88`.
Private selected IL capture hash:
`4235d9edfcdf80ad50c7507f468c078ea80a8904dd92946b7f704238495b1791`.
Pinned native assembly hash:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
No native methods were invoked, no action instantiated/adopted, and no native
IL, matrices, clips or proprietary assemblies are redistributed.

## Next exact work

Implement the bounded request-local original body/owned-tail binding on the
exact human prefab, preserving native two-handed spear handling and all
native references. Prove the original tail with real native commands, then
finish Salamander's printed mechanics. Only a complete same-artifact Sprint17
hidden/publication gate can publish the32 withheld snake roots.
Sprints14–16 remain complete;976published+29wrappers=1005visible.
Then full Phase2B closure and STOP for owner review. Phase2C authorized but
deferred until Phase2B owner acceptance; no Sprints18–22 under this mission.
