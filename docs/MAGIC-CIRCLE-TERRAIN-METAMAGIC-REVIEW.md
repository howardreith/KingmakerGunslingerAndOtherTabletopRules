# Magic Circle terrain and grouped-metamagic review

Review baseline: `17245f7fd513c9c2731923ab6aae6ae8f4825755`.
Branch: `codex/magic-circle-followup-polish`. Version: `0.0.134`.
No merge, history rewrite, master integration or public release.

[Exact evidence, candidate identities and source correspondence](../reports/magic-circle/TERRAIN-METAMAGIC-REVIEW.json).

## R1: reproduced before editing the renderer

The original flat ring failed on real uneven ground. A bounded guarded request
loaded only `KMG_AUTOMATION_WORKING`, then used native `LoadArea` with
`AutoSaveMode.None` to enter Oleg's Trading Post. The initial mansion search did
not supply suitable stairs; no artificial terrain was substituted. The final
regression uses the exact surveyed native route and validates its real collision
and walkable navigation before casting.

| Site | Native position (metres) | Original flat-ring observation |
| --- | --- | --- |
| Sloped exterior path | `(24.6460419, -6.2679863, 21.3650131)` | 47 of 96 vertices below ground; clearance -0.2270 to +0.0506m |
| Stair approach | `(-21, -4.70963669, 2.95200133)` | Lower end of the measured route |
| Stair base | `(-21, -4.62224674, 6.00000143)` | 58 submerged vertices; minimum clearance -2.6316m |
| Upper stair | `(-23.534317, -1.98248911, 7.69337559)` | 34 submerged and 62 floating vertices; clearance -0.5770 to +2.7120m |

The route rises **2.72714758m**. Area is
`ead426a6c23d39548a670ee515d77df4`, entry
`9092006db68574847ba3c2ac558a1d25`. The original view uses approximately
42.988-degree pitch, 225-degree yaw, 22-degree FOV, 1m near and 400m far planes
at 1280x720. Exact camera positions, floor normals/colliders, sampled ring
geometry and evidence hashes are in the accompanying qualification record.

The decisive unchanged-renderer run was
`20260921T0500003919567Z-disposable-magic-circle-terrain`. Native screenshots
show the ring cutting into uphill terrain and floating across the staircase.
That diagnostic run's overall result was FAIL solely because native camera
reprojection rounded the restored target X by 13 micrometres. It is evidence of
the visual defect, not a claimed qualification PASS. The fixture now restores
the captured camera target exactly; the restoration comparison remains strict.

## Small corrective change

The boundary now uses the game's **ScreenSpaceDecal / GUIDecal AoE projection**,
borrowing the actual native AoE Range's `PF/Decals/GUIDecal` shader and `Sector`
texture with `CIRCLE_ON`. This machinery was inspected in the installed native
assembly and compared in the real scene before adoption. No native assets are
modified, exported into the package or repainted.

One decal and one cloned material belong to each native moving area view.
X/Z scale is twice the actual `Size.Meters`: **6.096m diameter, 3.048m horizontal
radius**. The invisible projection volume is 12m high to retain readable native
middle-gradient opacity over this staircase. This height does not alter the
mechanical aura or create a visible dome. Blue/red/gold/violet colors are
unchanged. The native outline and soft inner fade retain modest visual weight.

The native shader reconstructs the visible surface from camera depth, discards
points outside the local volume and applies its existing slope/height fade.
Native volume depth comparison remains `NotEqual` (7), with front-face culling
(1); it is not forced to `Always`. Walls, stair structure, foliage and hidden
scene regions continue to occlude the ring. Alternate native camera views at
90 and 180 degrees show the downhill arc on exposed floor and stair treads;
the palisade explains the missing arc from the original camera angle.

A collision-projected LineRenderer experiment passed its 60 geometry/lifecycle
checks but was rejected after framebuffer inspection: the collision ramp lies
under visible treads, and connecting sampled vertices creates poor segments
at height discontinuities. That experiment is not the shipped solution.

There is no custom update loop, per-frame allocation, vertex raycast, repeated
renderer creation or production global scan. Native decal code updates its
bounds when the transform changes. Existing view-attach, ForceEnd and unload
hooks retain per-area ownership. Native enable/disable registers/unregisters
the decal; destruction frees only its owned material, not the borrowed texture.

## C1: native family metamagic, with no production patch

The test selects the learned family in the real spellbook, opens the native
metamagic mixer, selects the actual Extend feat and invokes the native Write
button. It prepares the resulting level-four custom family through the normal
spellbook UI. The real family popup then selects Evil; the test casts that
selected native child rather than constructing a metamagic child by hand.

The child retains Extend, spell level four, its book and the exact prepared
parent resource. A snapshot of all levels' prepared slots proves that exactly
that level-four slot is spent. The original caster/CL8/Extend context reaches
both carrier and area, with the bearer as area owner. Duration is **9,600
seconds**. After 90 seconds and a recipient exit/re-entry, **9,510 seconds**
remain on the same absolute deadline and context instances; no second slot is
spent. The ordinary known-spell collection is unchanged by native custom-spell
authoring. Production metamagic behavior required no change.

## Qualification and evidence boundaries

**509 native assertions passed across eight final qualification runs.**

| Native scenario | Assertions | Evidence directory (UTC) |
| --- | ---: | --- |
| disposable-magic-circle-terrain | 62 | `20260921T0604076538697Z-disposable-magic-circle-terrain` |
| disposable-magic-circle-evil | 278 | `20260921T0609373636050Z-disposable-magic-circle-evil` |
| disposable-magic-circle-ui | 91 | `20260921T0623547007570Z-disposable-magic-circle-ui` |
| working-save-magic-circle-prepare | 21 | `20260921T0629251149580Z-working-save-magic-circle-prepare` |
| working-save-magic-circle-verify | 13 | `20260921T0632276145832Z-working-save-magic-circle-verify` |
| working-save-magic-circle-scene | 26 | `20260921T0635126941146Z-working-save-magic-circle-scene` |
| working-save-magic-circle-cleanup | 15 | `20260921T0639335625934Z-working-save-magic-circle-cleanup` |
| working-save-magic-circle-absent | 3 | `20260921T0642361815422Z-working-save-magic-circle-absent` |

Terrain and mechanics passed on `native-decal-angles`. The subsequent
`native-decal-final` changes only the disposable UI fixture's camera restoration
and its diagnostic capture; every other changed source/test file has an
identical hash. The affected UI case and persistence sequence run on that final
candidate. The qualification JSON pins each run to its actual DLL/MVID and
records this source correspondence; evidence is not silently transferred
between binaries.

Repository validation, **1,683 domain tests**, clean exact-reference Release
build, strict **235-file package** validation, protected-icon validation,
475 runtime preflight checks, focused request contracts, all 12 settings
ownership cases and the 10 integrated Circle/Word of Recall binding checks
passed. The preflight expected catalog also now includes two already-registered
scenarios omitted from that test's old list; strict equality remains enforced.

The terrain oracle now checks native decal registration/camera submission,
radius mapping, depth/cull/color settings and projection-volume reach against
96 independently sampled real surfaces. Fifty movement positions cover both
directions along the stair route without renderer/material/deadline replacement.
The native UI case separately measures level ground for all four alignments.
Existing lifecycle and eight-overlap reconstruction assertions are retained.

Source inspection establishes how the renderer is configured. Structured
native state proves casting, resources, contexts, ownership and lifecycle.
Terrain raycasts establish volume reach and horizontal mapping; they do not
prove rendered tread coverage. Actual framebuffer captures supply that
separate visual evidence. This is bounded qualification on the observed level
floor, slope and staircase, not a claim about every scene or extreme cliff.

The UI repeat passed after its fixture restored the exact captured camera
target; the first UI run of the terrain-qualified candidate failed only that restoration
assertion. New diagnostic evidence records the native reprojection delta.
No production camera, observer or metamagic behavior was patched. The existing
matched native Protection popup control exceptions remain separately recorded;
there were zero Circle exceptions.

The original installation/settings restoration and exact candidate identities
are recorded below and in the accompanying JSON. Raw packages, saves, private
assemblies, native textures and machine-local runtime logs are not committed.

Only two Working writes occurred: preparation and exact bound cleanup. Fresh
absence passed. The original **0.0.117** installation was restored from
backup `20260921T0623473756899Z`. All **136 file names and
contents** match that backup, including settings SHA-256
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`. Kingmaker exited; no baseline save was written.


## Selected native visual evidence

These unmodified captures were individually inspected. Structural occlusion is
intentional; use the alternate stair view to see the exposed downhill arc.

| View | Before | After |
| --- | --- | --- |
| Slope | [Flat ring](../reports/magic-circle/terrain-review/before-slope.png) | [Projected boundary](../reports/magic-circle/terrain-review/after-slope.png) |
| Upper stair | [Floating ring](../reports/magic-circle/terrain-review/before-stairs.png) | [Original camera](../reports/magic-circle/terrain-review/after-stairs-original-view.png), [side camera](../reports/magic-circle/terrain-review/after-stairs-side-view.png) |
| Movement | | [Ascending midpoint](../reports/magic-circle/terrain-review/moving-upstairs.png) |

Level-ground colors: [Evil](../reports/magic-circle/terrain-review/level-evil.png),
[Good](../reports/magic-circle/terrain-review/level-good.png),
[Chaos](../reports/magic-circle/terrain-review/level-chaos.png),
[Law](../reports/magic-circle/terrain-review/level-law.png).

## Exact tested candidate

| Identity | Final candidate |
| --- | --- |
| Source state SHA-256 | `5ce0922599fa2705f88f86891c9f1f6373ccd51030c16c1a97f8a1a51610eef2` |
| ZIP SHA-256 | `844f7f2463e7dc12c88fab552f8fe75f5a750276bd89831536891d67de8d3fc6` |
| DLL SHA-256 | `b814e19a1887a9cc95127e0941ff65346d6358dbdd4cf7129ceb14d23499b117` |
| DLL MVID | `023277cd-0876-4664-814c-bf054f7a9056` |

Local installable package:
`artifacts/local-runtime/0.0.134/KingmakerGunslinger-0.0.134-local-runtime.zip`.

The terrain/mechanics candidate was `native-decal-angles`: ZIP
`cd1865fae5e7cef7d56ae36df5d212d7217c49b917412633d0fd8c9d9cca162a`,
DLL `e0cc0821be310c5894dd4c9357edbd3845626a5d1ec9cdb02f1ffae15752f41f`,
MVID `e41dad18-5e9b-4928-8b85-e34db6f6701a`.

The adjacent `.build-local.json` pins the package's provenance. Its embedded
commit is the review baseline, with the recorded dirty source fingerprint.
The changed source/test file hashes establish correspondence to the committed
implementation; subsequent changes are curated evidence and documentation.
All eleven installable diagnostic/final candidate identities, six compile-only
scratch candidates and failed attempts are retained in the qualification record rather than relabeled as final PASS.

## Preserved scope

The general family still exposes four variants; Paladin exposes Evil/Chaos and
Antipaladin Good/Law through their existing restricted parents. Oracle selection
and known accounting, stable legacy identities/known choices, scroll composition,
finite acquisition, scribing and shared Protection mechanics are unchanged.
No icons or icon assignments changed. **Existing new artwork still awaits owner
approval**; native visual qualification is not owner art approval.

No Buff Planner changes, deferred tabletop mechanics, unrelated UI/gamepad work,
reduced-profile qualification, uninstall work or accepted runtime/save safety
redesign. No new save against existing control, control suppression/resumption,
summoned-creature barrier, inward/trap circle or blanket descriptor immunity.
