# Cleanup 2: Ettin head/material correction after body 5

Source checkpoint, 2026-10-22. Owner appearance rejection is retained. Changed
native stills, actual Ettin neck fit, simultaneous-instance lifecycle and owner
visual acceptance remain **pending**. No build, game, installation, save or Git
operation was performed by this lane.

## Native findings before editing

Inspected `ettin-refine-view-front-left.png`, `-left.png`, `-right.png`,
`-rear-low.png` and `ettin-refine-move-out-016.png` under:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261022T0049060405024Z-disposable-expanded-summoning-cleanup2-native-review`.
The head appears pale, its hair gray and flat, facial features bulging, and the
short rounded base reads as a cuff against the detailed native brown Hill head.

Exact observed candidate source was `acdb5d90ada74dcf781c276367edb13eed328f0e`,
DLL SHA-256 `8b8b2e1911f84056d9183e2f84d0b8784b68fd4ed7e3b15f2340b5fae1f10c1b`,
MVID `870edd15-8797-459f-b8e8-8798ab0e805f`. The actual
`cleanup2-body-readback-ettin-before-refinement` shows the four owned material
instances still have the authored Skin (.55,.43,.39), Shadow (.19,.13,.10),
Hair (.17,.14,.12), Eye (.8,.73,.57) colors. They use **Standard**, with no
textures or `_TintColor`. This does not support a claim that native setup lost
the color; shader/lighting compatibility and untextured geometry are actual
concerns.

Also inspected actual Elephant/Brachiosaurus left and Gorgon front-left frames
in body 5 and baseline directory `20261021T2344278601321Z-disposable-expanded-summoning-cleanup2-native-review`.
Candidate body opacity/continuity and flatter Gorgon plates improve the baseline
holes/segmented limbs and pillow plates. Elephant proportions, Brachiosaurus's
elbow-like neck/body bulges and Gorgon's white quilt-like shading remain visible
review questions. No other creature asset was changed or visually approved.

## Exact material and shader contract

The observed Hill body is renderer/mesh `hill_giant_1`, one material
`hill_giant_01_character_d (Instance)`, shader `PF/StandardDynamic`, Gamma
color space, `_TintColor=(1,1,1,1)`, `_MainTex=hill_giant_01_d`,
`_Parameters=hill_giant_01_m`, `_BumpMap=null`. The outfit is a separate renderer.
Runtime attachment now requires this exact body contract and the existing
Ettin blueprint/prefab gate. Missing/mismatched proof records an attach failure;
there is no generic Standard fallback.

Read-only shader-property receipt from installed Kingmaker 2.1.7b:

| Item | Exact identity / observation |
| --- | --- |
| Container | `Kingmaker_Data/sharedassets1.assets` |
| Container SHA-256 | `cc779caf2fef21d111856a57d40b510677b0c236ad2723d1849564626445f785` |
| Format / Unity / platform | serialized format 17 / `2018.4.10f1` / 19 |
| Shader object | class 48, path ID 3835, offset 24872296, length 4732972; contains exact `PF/StandardDynamic` parsed name |
| `_Parameters` label | `Mask (R=Roughness, G=Emission, B=Metallic)` |
| Defaults | `_MainTex=white`, `_BumpMap=bump`, `_Roughness=1`, `_Metallic=0`, `_Emission=1` |

The local parser reads only the selected object's property metadata, with no
asset export. Format inspection used the primary
[AssetStudio serialized-file](https://github.com/Perfare/AssetStudio/blob/master/AssetStudio/SerializedFile.cs)
and [shader](https://github.com/Perfare/AssetStudio/blob/master/AssetStudio/Classes/Shader.cs)
readers. No native shader code, material, texture or geometry is committed.

Matching installed `Assembly-CSharp.dll` MVID is
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`, SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
Its `CharacterAtlas` normal-channel branch explicitly uses `NORMAL_MAP_ON` and
`_UseNormalMapAtlas`. The owned head disables that branch, clears `_BumpMap2`,
and leaves `_BumpMap` null for the verified flat default plus authored vertex
normals. There is no invented packed-normal encoding. The previously inspected
material controller can instantiate `Renderer.materials` and reset cached
texture/UV/color settings; its contract is recorded in
[palette review notes](CLEANUP2-PALETTE-REVIEW-NOTES.md).

Each of the four materials clones the exact body shader state/keywords, then
uses the original atlas, a new linear RGBA32 parameter map `(230,0,0,255)`, white
tint, roughness scalar 1, metallic/emission scalars 0. Native albedo/normal/
parameter/dissolve/petrification maps are replaced or cleared on these private
clones. No source material/renderer is assigned or modified. The parameter red
value is deliberate matte roughness, with zero emission and metal channels.

## Original geometry/paint correction and ownership

The generator now authors a smaller, shallower face with narrow recessed eyes,
smaller integrated ears/nose/cheeks, a receding hairline, tapered dark nape locks
and a longer oblique neck/shoulder root. A new original 512-square, four-region
RGBA atlas supplies brown skin weathering, subtle creases, dark hair strands,
shadow and amber eyes. It is generated from authored UV functions; no donor
pixels, UVs, vertices, animation curves or body surfaces are copied.

Stable unit/prefab GUIDs, `KMG_Ettin_AuthoredHead`, four submeshes/material names
and two driver bones are preserved. The fixed neck-relative skull offset changes
to authored `(.52,.02,0)`; it still does not rotate its lateral position around
the first head. Existing release/reattach gating is retained. Capture/release
now includes both new textures, the mesh, root and proven material instances.
An assigned foreign material without the owned name/shader/map provenance is
not destroyed. The existing native lifecycle observer derives its resource
array dynamically; its four-material/two-driver contract still applies.

## Shared donor posed-frame plausibility, not Ettin calibration

Used the authorized Hill pre-survey in
`20261022T0056098360099Z-disposable-expanded-summoning-cleanup2-native-review`,
run `20261022T0056098570079Z-7ff8d9129a024d08b226b1081c369b73`, observation
SHA-256 `5a645f0a9fcf934e7d2732ea204e583a61e394629e06a02c19c6853454fd6278`.
The prefab exactly matches `635d6dee1c7831b41aa987bdbeff14bc`. Reconstructed
captured local TRS chains; values below are relative to its captured UpperTorso:

| Point | XYZ |
| --- | --- |
| Native Head / Neck | `(.0360,.9752,-.1479)` / `(0,.7705,.0218)` |
| Native clavicles | `(+/-.2362,.4562,.0518)` |
| Native upper-arm origins | `(.6450,.3798,-.0005)` / `(-.6323,.3237,.0867)` |
| Authored root centre | `(.2341,.2219,.0734)` |
| Authored neck midpoint / upper neck | `(.3696,.4850,.1034)` / `(.4984,.8785,.1175)` |
| Authored skull pivot | `(.4901,1.0487,.0955)` |

The root centre lies below both clavicles and between upper-arm origins, making
chest burial plausible rather than treating the entire isolated neck as exposed.
These are shared-donor scalar checks, **not** an actual Ettin instance, sampled
body-surface intersection, native bind calibration or fit PASS. Root has been
asked to include exact actual Ettin frame points in candidate-6 lifecycle review.

## Offline checks and reproducibility

All commands use `C:/Dev/.toolchains/blender-4.5.10-windows-x64/blender.exe`
4.5.10 LTS, hash `6dc0b208d1b5`, from this worktree:

```powershell
blender.exe -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint29-ettin/build_ettin_head.py -- --output src/KingmakerGunslinger/Summoning/EttinAuthoredHeadMesh.cs --work-dir artifacts/cleanup2-ettin/final
blender.exe -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint29-ettin/build_ettin_head.py -- --output artifacts/cleanup2-ettin/repeat/EttinAuthoredHeadMesh.cs --work-dir artifacts/cleanup2-ettin/repeat
```

Final generation exit 0, **2.7258 s**; independent repeat exit 0, **2.4240 s**,
generated C# SHA-256 identical. Local exact-payload decoder/audit exit 0,
**0.4859 s**: 2,840 vertices, 5,636 triangles (3200/800/1300/336), closed
two-manifold skin, positive signed skin volume 0.1307843, zero degenerate
triangles, 5,636/5,636 outward averaged normals, finite unit normals and bounded
weights, exact UV/material quadrants, exact geometry/paint hashes and no trailing
payload bytes. It reconstructs the actual committed packed data for preview,
rather than rendering an independent approximation.

The shared reviewer command uses that `ettin-exact-packed.blend`, `--up y
--forward=+z`, the existing `authoring-poses.json`, absolute `--source`/`--out`
paths, `--views front,side,rear,low --size 720`. Exit 0, **1.8564 s**, ten images:
four bind views and party/side views for turn, pitch and combined neck reaction.
Bind front/side/rear/low and turn/pitch/reaction frames were opened and inspected;
the closed neck connection remains intact. Workbench studio shading is an
offline geometry/UV check, not PF/native lighting or aesthetic acceptance.
The first relative-output preview attempt failed its output path; a local audit
helper initially failed recreating Blender groups after mesh replacement. Both
local helper issues were corrected before the successful exact-payload checks.

Packed geometry SHA-256:
`85c6e7ce05a9db979f98027f7fe4c8f89f318a81851fb93bae0f8ca134f4e025`.
Original paint PNG SHA-256:
`736b53f8365befd0ded146316323a21aafe6c01ccefdb628b05ee7d751f6f34c`.
Generated C# SHA-256:
`ccf84c93242a4a8c9a7e1b02bd6e5ca667f818b9f2b03a984b4930c278dbd33c`.
Binary editor caches, PNGs, local readers, reconstructed `.blend` and native
evidence remain ignored machine-local artifacts. Root owns production compile,
candidate packaging, guarded native still/motion/lifecycle checks and acceptance.
