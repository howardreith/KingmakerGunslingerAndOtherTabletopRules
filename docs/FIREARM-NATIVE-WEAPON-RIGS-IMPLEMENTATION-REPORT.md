# Firearm native weapon rigs implementation report

## Outcome

Final semantic-anchor identity is implementation
`25a585f79a7c0af232c55636aaaaa77d78a4fdee`: package
`6858AF28C2DDE865BD2575FDEECF6DA11ADACEB0BC6210B1251DEC54239DBC06`,
DLL `2757835E9086B35481D9F5E06B03DC691BB317B351794BE1B0EDC20442568EA4`,
AssetBundle `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`,
manifest `35BB38BF142D1F1DB3439F4EC328CE7EBF2CFD149318BCEF714A1254CB5301D1`.
All source/package and guarded rig, switching, Wwise, Scatter, Targeting Arms,
and reload gates pass. Human review must still establish abdominal clearance,
grip contact, and support-hand surface contact on doll/world animations.

Semantic-anchor candidate: Regular Pistol held appearance was human-accepted on
2026-08-07 and its source, equipped transform, scale, and animation are frozen;
this is not blanket `HumanAccepted` status. Musket, Blunderbuss, and Rifle now
declare source-space Grip/Support/Butt/Muzzle points. Their identity root is the
firing-hand grip, Visual translation is derived from GripPoint, and every other
anchor is derived relative to it. Musket semantic length is `1.349985 m`, versus
Blunderbuss `0.848 m`; support targets start from the Heavy Crossbow lateral/palm
offset near `(-0.031,-0.051,...)`. Crossbow animation and hidden holsters are unchanged.

Finishing-pass audit: Pistol is deterministically bound to the intended Cyril43
flintlock `model.dae`, distinct from Revolver's Navy Colt source. Revolver
authoring removes 53 numeric-suffix duplicate preview objects. The failed custom
long-gun shader is retired in favor of opaque Standard materials and generated
reverse-wound backfaces. Exact bounds corrected Blunderbuss source-unit collapse
and Rifle placement behind the grip; Musket's good held transform was preserved.
Long-gun holsters remain hidden, so the awkward backpack pose is not shipped.

All five equipped firearm rigs are enabled as `AutonomousCandidate` after
deterministic bundle, runtime capability, exact native IK, projectile and guarded
save-free regression qualification. Belt/back models remain independently
disabled, preserving native fallback behavior. No firearm is `HumanAccepted`;
grip, clipping, scale, pose and animation quality still require a person.

The current review build preserves the established Musket and Blunderbuss grip,
support-hand, muzzle, and Crossbow animation calibrations. Their held materials
use a project-owned double-sided diffuse shader after exact diagnostics found no
LODGroup, no negative/mirrored scale, enabled renderers, and complete normal
arrays. This is a narrow response to view-dependent partial disappearance, not
a claim of visual acceptance. Pistol's equipped Visual child alone has a
180-degree roll correction; its root, muzzle, animation, and belt prefab remain
unchanged. Long-gun belt models remain intentionally hidden rather than expose
the awkward native back-slot pose.

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

- Commit: `3ae6b5d903720dbd450a2bb3fa82ed32d0b14c4d`.
- Version: `0.0.71`.
- Package: `artifacts/local-runtime/0.0.71/KingmakerGunslinger-0.0.71-local-runtime.zip`, SHA-256 `9F905766214BEB2AC23E2519525826B14970FA7CDE32D305BD8D4E9D2452DF2D`.
- DLL SHA-256: `479244B41883256831396E60FFCC9CFD06E6F40544AF6E8185D0785831D5000C`.
- AssetBundle SHA-256: `88DF971967ECF4879BAA93FE79A734D46ABA2A754AEBD193FAE01AB756DCFD91`.
- Rig manifest SHA-256: `2DD5D5F69C99925B8D390292B1FC3045BC7775CBB04B3D136FF0938D04BF9CA6`.

## Human acceptance required

The exact candidate checklist is
`docs/FIREARM-NATIVE-WEAPON-RIGS-MANUAL-ACCEPTANCE.md`. Automated evidence does
not establish camera-angle visibility, upright visual reading, clipping, scale,
or animation quality. First inspect held Musket and Blunderbuss from multiple
camera angles, then Pistol orientation/muzzle, and only then holster policy.

## Targeted repair candidate identity

- Implementation commit: `d7b6bc1756ae89f5e043c5b3362a46e8fe614e8f`.
- Version: `0.0.71`.
- Package SHA-256: `6B3E85517C945B7CB6096E83C2946706749B91C142FA5C7412044EBDD5A03D81`.
- DLL SHA-256: `B1C181740DF76179B145D5C9A03B420DADDB71E6AA938445FDBAA5351660CE5F`.
- AssetBundle SHA-256: `62BAB35C9DEB94AE98B61CD8B56CA523CC946A740248C06B63E8E41A94AE7CDD`.
- Rig manifest SHA-256: `429A4E7A30553C016EFEEA95951598164D6F7A4930218A64977EA7DEBD2C2B7F`.

## Finishing-pass candidate identity

- Implementation commit: `fc53c470c94b08265a8a44ce867d7709d7e1003d`.
- Version: `0.0.71`.
- Package SHA-256: `2D7D5A107DF377C1C5BC9D4DCDB693DF5826C390223E14AF789CC03EF34CCE4F`.
- DLL SHA-256: `D8D717C21B24CD8EE1702D979132BB5E2123DD513147D39FD804B48728CF4E1D`.
- AssetBundle SHA-256: `4A96CD13152A9EF6B48B3758B697659DCC82BC92D46A97AC8FBAAD815E386B2B`.
- Rig manifest SHA-256: `60D143952974B8B9039E45B7F4E5B14A7D33294BA89FC336ADCC5CDD7A65571D`.
- Automated gates: repository/package plus guarded rig, switching, Wwise,
  Scatter, Targeting Arms projectile/damage, and reload scenarios all PASS.
- Remaining risk: only human rendering can prove that the three long guns are
  actually visible in the inventory/world cameras, Revolver cleanup looks
  coherent, and Pistol visually reads as the intended flintlock. Holsters are
  intentionally hidden. No weapon is HumanAccepted.

See `docs/FIREARM-NATIVE-WEAPON-RIGS-MANUAL-ACCEPTANCE.md`. All checks remain
pending.

## Known limitations

Inventory-doll refresh, belt/back candidate qualification, calibration debug markers/import promotion, real-unit
hand-distance instrumentation, two consecutive final candidate runs, version
0.0.71 identity, working-save/full acceptance, and all human visual judgments
remain. The structural scenario instantiates validated rigs but does not yet
prove native live hand-slot parenting on a rendered disposable unit.

## Next action

Implement exact slot-scoped sheath/quiver lifecycle and inventory-doll refresh,
then perform the supervised Musket-first visual session. If calibration changes
are required, export them with `humanAccepted=false` and promote only reviewed
values into the authoritative rig specification.

Final 0.0.71 evidence: two consecutive all-five rig PASS runs
`20260807T0452453368618Z-108bd4df764c4c948b1baf7c72619537` and
`20260807T0454196627467Z-0f6629e8e8fd4f2f924c1d4da64cc130`; Wwise PASS
`20260807T0455538086836Z-aa2b2e869bd9435c9510a2c64e19b4ee`;
Targeting Arms PASS
`20260807T0457299033010Z-53ece3c653eb43e1b71ad1913c0661e9`;
working-save smoke PASS
`20260807T0459208536053Z-7621757d095c4a6a89273a06c4585d69`.

## Held clipping micro-calibration candidate

The 2026-08-07 finishing pass leaves the human-accepted Pistol, Rifle, belt
policy, every scale and rotation, and both Crossbow animations unchanged. It
changes only the semantic source-grip Z for Musket (`0` to `0.00478`) and
Blunderbuss (`-0.00316` to `-0.00216`). This derives an approximately `-0.020`
local-X clearance for each complete held rig, including support, butt, and
muzzle anchors, without changing semantic lengths (`1.349985` and `0.848 m`).
Two exact Unity builds are byte-identical at
`EEEBA3292119A4619EE3D391246C55E47FC5D9E0BA625DB19E5AB9BBF124315E`;
repository validation, 911/911 tests, Release build, build-output, and strict
package validation pass. Automated checks do not establish that torso clipping
looks improved; that remains the next narrow human comparison.

Published implementation commit is
`5a37f16a176b54a71d18924c42f769caea5c92c2`. Candidate package
`artifacts/local-runtime/0.0.71/KingmakerGunslinger-0.0.71-local-runtime.zip`
has SHA-256
`3296604A13F738DC4E8388F3FD8320AB9BA520BD7C9B6ABC04B16B2C114E6B99`;
DLL `00C19F621AD6184EED6B000ACD76D9C5DC19F5616F8DF91AFA7A1C171A32AF14`;
AssetBundle `EEEBA3292119A4619EE3D391246C55E47FC5D9E0BA625DB19E5AB9BBF124315E`;
rig manifest `15A1B3D6E821A96C1DF64FBF80752254AA3C498CE2871ADC2BB434EE5502B3FC`.
Guarded visual rigs, switching, Targeting Arms/projectile, Wwise, Scatter, and
reload scenarios all PASS with exact run IDs/hashes in the journal. The only
remaining uncertainty for this pass is the human-perceived clipping delta.

## Final bounded finishing candidate

The rejected `-0.020` local-X experiment is reverted by reapplying only the
last human-best Musket and Blunderbuss source grip values. Musket remains
`1.349985 m`; scale, rotation, semantic points, model, materials and Crossbow
animation are unchanged. Blunderbuss likewise returns to its prior anchors and
`0.848 m` length. No unobserved rotation was selected because the save-free
fixture cannot establish a trustworthy torso-outward sign; minor residual
clipping is accepted rather than risking another regression.

Holsters now use an explicit tri-state policy. Musket, Blunderbuss and Rifle
are `Hidden`: effective belt/sheath are null, the exact installed attach-slot
collection is empty with override enabled, prototype fallback is severed, and
an exact long-gun-only `ReattachSheath` postfix removes recreated sheath/quiver
presentation. It resolves production firearms through their marker and never
scans avatar renderers or touches native crossbows. Pistol's accepted held
source, transform, scale and animation remain frozen.
