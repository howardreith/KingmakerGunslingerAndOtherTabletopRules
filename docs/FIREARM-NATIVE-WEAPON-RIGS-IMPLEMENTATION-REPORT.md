# Firearm native weapon rigs implementation report

## Outcome

All five equipped firearm rigs are enabled as `AutonomousCandidate` after
deterministic bundle, runtime capability, exact native IK, projectile and guarded
save-free regression qualification. Belt/back models remain independently
disabled, preserving native fallback behavior. No firearm is `HumanAccepted`;
grip, clipping, scale, pose and animation quality still require a person.

## Starting identity

- Source: `codex/firearm-wwise-audio` / `2d9d95c8b0f919fb5f129c783522608bc47e2029` / `0.0.70`.
- Feature: `codex/firearm-native-weapon-rigs` in isolated repository-local worktree.
- Baseline rig-manifest SHA-256: `326E3B59A0FF869D8BA570F2A01C5D6137F828CC3FAA652CC9191309779B219D`.

## Exact Kingmaker findings

Exact installed 2.1.7b findings are curated in
`FIREARM-NATIVE-RIG-FORENSICS.md`. `WeaponVisualParameters` supplies model,
belt/sheath, animation, attach slots, projectile and prototype fallback.
`UnitViewHandSlotData` owns attachment/model/sheath lifecycle and owner scale;
`UnitViewHandsEquipment` exposes `UpdateAll`, visibility, belt and active-set
lifecycle. `EquipmentOffsets.IkTargetLeftHand` is the native support-hand target.
Native Light/Heavy Crossbow donors use `Crossbow`, `Shield` attach slot, one
projectile, exact quiver/sheath prefabs, and calibrated IK children near z=.36m.

## Root causes

Starting evidence identifies non-semantic pivots/grips, inconsistent source
units, absent native support-hand IK, and coupled animation/projectile/holster
lifecycle as the likely causes. Exact findings remain pending.

## Implementation

The Unity builder now uses explicit per-prefab rig specifications and produces
identity grip roots with `Visual`, `Muzzle`, and long-gun `SupportHandTarget`.
Runtime loading validates each equipped model independently before publication.
It rejects nonidentity roots, absent/invalid hierarchy, nonfinite transforms,
bad +Z muzzle/support ordering, cameras/lights, and invalid materials. For every
validated long gun it attaches the exact installed `EquipmentOffsets` component
when absent and assigns `IkTargetLeftHand` to the authored support target.
Capabilities publish transactionally beside the prefab dictionaries.

Presentation readiness is explicit (`NativeFallback`, `AutonomousCandidate`,
`HumanAccepted`) and custom selection requires a validated runtime capability.
All five equipped models are autonomous candidates. Pistol/Revolver use the
allowlisted `PiercingOneHanded` candidate; long guns retain `Crossbow`. No belt
candidate or human status is assigned.

The development UI now provides per-kind session calibration for visual,
support and muzzle position/rotation, uniform scale, coarse/fine increments,
the short-gun animation allowlist, deterministic `humanAccepted=false` JSON
export, resets, and reversible native/custom world refresh through exact
`UnitViewHandsEquipment.UpdateAll`. It fails closed when selection, exact
firearm, view, hierarchy or IK is ambiguous. Exact inventory-doll refresh,
markers, belt controls and projectile diagnostic toggle remain incomplete.

The obsolete whole-character renderer-name scan was deleted and removed from
the project. A validator now rejects its return. Holster/quiver behavior remains
native fallback because an exact per-slot replacement has not yet been safely
qualified.

The new allowlisted `observe-native-firearm-rig-contracts` scenario is save-free
and records exact native Light/Heavy Crossbow visual parameters, hierarchy,
renderers, attach slots, projectile count, belt/sheath identity, EquipmentOffsets,
left-hand IK target, all custom capability diagnostics, fallback readiness, and
transient cleanup.

## Rejected experiments

The first broad reflection probe overflowed the installed type graph and was
replaced with exact-name inspection. Build iterations also found stale source
test anchors, an exact `AdvancedRifle` catalog name, and missing namespace
imports; each was corrected before publication. Sandbox-only temp/ref-lock
denials were rerun with scoped approval and did not alter implementation.

## Automated evidence

Repository validation passes; 909/909 domain/reflection tests pass; exact-reference
Release build, build-output validation, SoundBank validation and strict package
validation pass. Exact Unity 2018.4.10f1 produced identical clean bundle hashes
`88DF971967ECF4879BAA93FE79A734D46ABA2A754AEBD193FAE01AB756DCFD91`.
Guarded all-five structural run
`20260807T0434551954973Z-4133c90579c64263a335b8c204cf324c` PASS, result
SHA-256 `83ABC30BAC60F8A7421A57F4BFD4D0997F5DE4A53420E1782A5218483C9ADED2`.

## Regression evidence

Fresh published-commit PASS runs: Wwise
`20260807T0436296097402Z-6e9bf6f1c99d40178aa87dcf83503ce0`; Scatter
`20260807T0438026145108Z-9ebe4496e4564c6abcddc40009259765`; Targeting Arms
`20260807T0439386747600Z-42a04e5453d541ce9ad2384333ee26f7`; reload
`20260807T0441122052582Z-911155a36df8478fae5987f4f7f7fc54`; switching
`20260807T0442452610862Z-0f5acf42894c424aafbdb68db55d1c3f`.

## Candidate identities

- Commit: `54eeeea460844e66d1fff286b0b494ceeb27e6a2`.
- Version: `0.0.70` (candidate identity has not yet been bumped to 0.0.71).
- Package: `artifacts/local-runtime/0.0.70/KingmakerGunslinger-0.0.70-local-runtime.zip`, SHA-256 `655B4C2A59DF09A689A5C49A70B818650DFBD1276BB5A99A2982D1D3331B94AB`.
- DLL SHA-256: `395278EA216126828FC361C126FBBD1C0AB87FB6459323509910DF3A69112D2D`.
- AssetBundle SHA-256: `88DF971967ECF4879BAA93FE79A734D46ABA2A754AEBD193FAE01AB756DCFD91`.
- Rig manifest SHA-256: `2DD5D5F69C99925B8D390292B1FC3045BC7775CBB04B3D136FF0938D04BF9CA6`.

## Human acceptance required

See `docs/FIREARM-NATIVE-WEAPON-RIGS-MANUAL-ACCEPTANCE.md`. All checks remain
pending.

## Known limitations

Inventory-doll refresh, exact per-slot quiver/sheath suppression, belt/back
candidate qualification, calibration debug markers/import promotion, real-unit
hand-distance instrumentation, two consecutive final candidate runs, version
0.0.71 identity, working-save/full acceptance, and all human visual judgments
remain. The structural scenario instantiates validated rigs but does not yet
prove native live hand-slot parenting on a rendered disposable unit.

## Next action

Implement exact slot-scoped sheath/quiver lifecycle and inventory-doll refresh,
then bump the coherent final candidate to 0.0.71 and rerun the full acceptance
set twice before the supervised Musket-first visual session.
