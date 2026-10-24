# Cleanup 2: Ettin facial direction and sculpt repair after native 6

Reviewable source checkpoint, 2026-10-22. **Native appearance acceptance remains
pending.** No build, package, game, installation, save or Git operation was
performed by this lane. Only the Ettin generator/provenance, its generated C#,
`EttinSecondHeadVisual.cs` and this note change. Earlier material-refinement
notes remain evidence for their earlier bytes.

## Rejected native evidence and concrete diagnosis

Read actual native6 images under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261022T0136351111593Z-disposable-expanded-summoning-cleanup2-native-review`:
`cleanup2-ettin-bea95e37-34fe-42cc-b4ec-60ae2c4df210-idle-12-heads--40.png`,
`-idle-12-body-75.png`, and `-main-hand-24-heads--105.png`.
The added face is dark and smooth compared with the angular native first head;
the exposed neck is long/vertical, and the added face looks backward/upward.
The owner rejected this appearance. This was not diagnosed as lost material
color: exact native readback retains the expected owned materials and maps.

| Native6 identity | Value |
| --- | --- |
| Run | `20261022T0136351421581Z-36c7a69a3b5f4956b34c975c82bf71bf` |
| Source | `cfe39dc1a1c7c16b3228ca5801a4ab356257349b` |
| DLL SHA-256 | `59c51bb540e3426fbd3290b53a8a89776d5dae59cddec1600cdfb4781d8962c3` |
| MVID | `9a25bb0d-03ab-446f-83e0-c6d70674ca09` |
| Observation JSON SHA-256 | `d0dd0b72df696558a4cc5042215069cbf98fe5823a0311d1aae57ce0ce387580` |

Root reports 97.038 s, **72 PASS / 2 FAIL**, both failures missing the native
off-hand pose handle. Isolation, recast, private-resource release and exact
restoration passed for those old bytes. There is no off-hand motion, new-byte
lifecycle, save or cold-hydration PASS.

The prior sculpt used **Head-local +Z as forward**, and runtime correctly
transformed it by `Root.worldToLocalMatrix * Head.localToWorldMatrix` without
correcting this authoring error. The complete authorized shared-Hill hierarchy
has these exact facial anchors relative to Head:

| Anchor | Head-local XYZ |
| --- | --- |
| `Jaw` | `(0,-.004439232,-.07620681)` |
| `Locator_HeadCenterFX_00` | `(0,.133,-.113)` |

Both place the native face on **negative Z**. In plan view the new authored
forward `(0,0,-1)` has dot **+1** with the normalized actual Jaw direction;
the previous +Z direction has dot **-1**. The opposite-facing native pixels
independently agree. No donor vertices, UVs, texture pixels or animation curves
are authoring inputs.

## Focused correction and preserved contracts

The new original sculpt directly uses **-Z forward / +Y up**. It has a shorter,
oblique neck widening into a broad shoulder fan, a broad jaw/chin, integrated
nose/cheeks and smaller ears, a restrained brow, real carved orbital/mouth
cavities, recessed narrow eyes, lips and four uneven teeth. Nape hair is placed
behind the head on +Z with uneven tapered locks and a bald forehead. The
original atlas supplies gray-tan skin, forehead creases, eye/beard weathering,
ash hair and muted ivory eyes/teeth. These are project-authored choices for
native review, not a claim of matching the donor's texture.

The fixed Head-local skull offset changes from `(.52,.02,0)` to
`(.44,-.035,-.02)`. Runtime still derives the offset once in the native neck
frame, copies Head rotation without orbiting the lateral position, and leaves
the native head/rig/mechanics unchanged. Unit GUID
`2481bc73abd64ef0ae581b48438b5404`, prefab
`635d6dee1c7831b41aa987bdbeff14bc`, mesh `KMG_Ettin_AuthoredHead`, two drivers
and the four Skin/Shadow/Hair/Eye material slots remain exact.

The proven material/lifecycle code from the prior checkpoint remains: require
exact `hill_giant_1` body/mesh, `hill_giant_01_character_d` material identity,
`PF/StandardDynamic`, Gamma, exact native albedo/parameter names, white Tint
and null BumpMap; clone its shader state and replace/clear all native maps on
the owned clones. Original 512-square albedo plus a linear RGBA32 parameter
map `(230,0,0,255)` use the verified **R=Roughness, G=Emission, B=Metallic**
contract, roughness scalar 1, emission/metal 0. The normal-map branch is disabled
and uses authored vertex normals/verified flat default. No Standard fallback.

The shader receipt remains installed `sharedassets1.assets` SHA-256
`cc779caf2fef21d111856a57d40b510677b0c236ad2723d1849564626445f785`, format 17,
Unity 2018.4.10f1, platform 19, class 48/path ID 3835/offset 24872296/length
4732972. Exact `_Parameters` property label and CharacterAtlas normal-keyword
inspection are detailed in [the preceding material notes](CLEANUP2-ETTIN-MATERIAL-REFINEMENT-NOTES.md).
No native shader/property blob is committed. Existing capture/destruction still
owns the root, mesh, both original textures and only proven owned material
instances. No new resource type or observer registration is needed.

## Captured-frame offline check and its limits

Full posed TRS chains come from the exact shared Hill prefab in
`20261022T0056098360099Z-disposable-expanded-summoning-cleanup2-native-review`,
predicate `cleanup2-pre-survey-actor-hill-giant`, run
`20261022T0056098570079Z-7ff8d9129a024d08b226b1081c369b73`, observation SHA-256
`5a645f0a9fcf934e7d2732ea204e583a61e394629e06a02c19c6853454fd6278`.
This is a complete shared-donor **captured posed frame**, not native6's Head
rotation or a native bind-pose capture. Native6 has paired upper-body
world/Neck-local points, allowing separate Neck recovery (determinant
`.9999958873`, maximum orthogonality error `.0000140667`), but **no Head
quaternion**. It was not fabricated or inferred from that Neck recovery.

Let `H` be captured Head-local-to-Neck, `N` Neck-to-rig, `K` the initial owned
skull transform, `Kp` its synthetic pose, and `w` the exact packed skull weight.
The preview executes the runtime bind equation on each exact packed position:
`N * ((1-w)*(H*p) + w*(Kp*K^-1*H*p))`. Normals use the corresponding inverse
transposes. A proper +90-degree X display rotation maps Unity Y-up to Blender
Z-up without reflection. Geometry-free native anchor markers remain separate.

The new shoulder-fan centre is rig `(-.20850,2.74157,-.22573)`; captured clavicle
origins have Y `2.59833/2.66778`, upper-arm origins Y `2.46192/2.59551`.
Its lower surface reaches that shoulder region. This supports plausible
placement only: no native body-surface intersection/burial measurement exists.
Native review must decide neck exposure, lateral proportions and skin match.

Nine 900-square renders cover captured-donor, synthetic 30-degree turn and
synthetic 20-degree pitch, each front/side/rear. Opened final front/side/rear,
turn-front and pitch-side. The face now follows the anatomical front, real
socket/mouth contours are legible, the connected collar deforms, and the skull
driver's translation delta is exactly zero in both synthetic poses. Workbench
lighting looks darker/glossier than the PF native scene and grants no native
shader or owner visual acceptance. An initial preview used the wrong display
up-axis; it was corrected before final inspection. An early cavity reduction
had microscopic sliver faces; an original-surface voxel remesh removed them
before final packed-byte admission.

## Reproduction, measured checks and identities

Blender `C:/Dev/.toolchains/blender-4.5.10-windows-x64/blender.exe`, 4.5.10 LTS,
build hash `6dc0b208d1b5`. From this worktree (absolute output paths resolved
before invoking Blender):

```powershell
$root = (Get-Location).Path
$out = Join-Path $root 'artifacts/cleanup2-ettin'
$src = Join-Path $root 'src/KingmakerGunslinger/Summoning/EttinAuthoredHeadMesh.cs'
blender.exe -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint29-ettin/build_ettin_head.py -- --output $src --work-dir "$out/forward-final"
blender.exe -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint29-ettin/build_ettin_head.py -- --output "$out/forward-repeat.cs" --work-dir "$out/forward-repeat"
blender.exe -b --factory-startup --python-exit-code 1 -P artifacts/cleanup2-ettin/audit_packed_head.py -- forward-final
blender.exe -b --factory-startup --python-exit-code 1 -P artifacts/cleanup2-ettin/preview_native_frame.py
```

Final generation **5.6568 s**, independent generation **5.6384 s**, both exit 0
and identical generated C# SHA-256. Exact packed-byte audit **0.6115 s**, exit 0:
5,024 vertices; 9,992 triangles (5,600/2,088/1,600/704); closed two-manifold
skin, signed volume `.11552049`; zero degenerate triangles; 9,992/9,992 outward
averaged normals; finite unit normals/positions/UVs, bounded normalized weights,
exact four UV quadrants/index bounds/hashes and no trailing bytes. The audit
reconstructs the decoded committed payload for preview. Captured-frame plus
synthetic-pose preview/check **2.6192 s**, exit 0.

| Current identity | SHA-256 |
| --- | --- |
| Packed original geometry | `747aec079b36e424b13af5e4d4cf002a9810cda13da5a2e81b5d3e2175183eb8` |
| Packed original albedo | `4d6d02da2b349dc35082df3d8296d8ae80b00f25ac36022632d3eecc561dcc60` |
| Generated C# | `0ab82b733871627bc7b22f45265261a3f9fef7ebf2490a8f4bc10b4d9f130b3e` |
| Generator | `8a7ef8fabdfe6355afc7d57a94669a845e27dc29fa501f5e5c9ffe687aa9ff39` |
| Runtime attachment source | `bbda8b396620550d4e4394a677f1f0e086aabd4b49d229bfb0fdb702aa0acd82` |

Local QA helpers, logs, JSON, textures and `.blend` remain ignored under
`artifacts/cleanup2-ettin`; no proprietary bytes or raw evidence are committed.
Helper SHA-256: packed audit `2c28112d4ca00436042894d01b5e504677eebcdb95504368e40eeb71dc7fe5e0`,
frame preview `ba88c9e86081b07b155d79a22c923786657e567876706412aafc6466b320f5c9`.
Final PNGs are in `forward-final/native-frame-renders/`.

Root owns production compilation and the next guarded native reproduction:
same-direction facial review with explicit current Head/Neck quaternions,
neck burial and proportions beside the original head, PF lighting/texture
readback, available real motion poses and independent-instance teardown/recast.
Those checks are required on the new artifact; this source checkpoint does not
turn the retained native6 failure or old-byte lifecycle results into a new PASS.
