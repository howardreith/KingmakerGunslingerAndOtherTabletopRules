# Original Gorgon body: Cleanup 2 refinement

Current authoring is `build_gorgon.py`, using the retained exact original
project-authored `cleanup2-gorgon-input.json`. Its SHA-256 is
`0f3c8ce10c4c0d50650ecb5a042985d1b242977716c5b7a0286754d370fdc508`.
This input contains original geometry, UVs, painting identity and named weights;
it contains no native vertices, textures, bind transforms or animation curves.

## Current hard-panel recovery

The native review-14 front-left and left frames still showed convex quilt-like
plates and matte shading on the preceding gunmetal repaint. The current export
replaces that armor geometry with **25 rigid planar steel panels**: clipped
corners, narrow physical bevels, split crease normals and overlapping lips.
Paired roof facets follow the curved back instead of a wide floating tangent
slab. Each panel follows its dominant original Chest/UpperTorso/LowerTorso
driver; the forehead shield follows Head. No soft skin blend bends the panels.

The underlying 9,500 body triangles retain exactly the preceding exported
positions, normals and named weights, independent of new UV seam copies. The
28-name palette/rig identity, original horn and eye point sets, eight hoof
halves and their contact minima are retained. Blueprint, unit/view scale,
colliders, native donor, animator, summon rules and breath origin are unchanged.

`gorgon-parameters.png` is a third original export, baked through the **same UV
and region rasterizer** as the albedo. Geometry regions 10..34 are armor:
R=.23 roughness, G=0 emission, B=.94 metallic. All other authored regions use
R=.82, G=0, B=0. No color threshold, inferred UV rectangle or native texture mask
identifies armor. Albedo changes are original steel paint; UV packing changes
because the geometry changed, so this revision makes no old-texel equality claim.

Only the exact Gorgon runtime material specialization binds this map. The
existing PNG/hash/dimension loader checks its bytes; CPU channels are copied
verbatim into a private `Texture2D(..., linear:true)` so packed inputs are not
gamma-decoded. The map is included in that attachment's resource receipt,
rollback and teardown. The native PF shader's declared channel label establishes
R=Roughness/G=Emission/B=Metallic. Current scalars are `_Metallic=0.4`,
`_Roughness=1`, `_Emission=0`; the zero body B mask prevents full-body metal.
The completion section below records the exact shader response and native
black-plate correction. Changed appearance still requires native observation.

Current generated hashes:

| Export | SHA-256 |
| --- | --- |
| `gorgon-mesh.json` | `3deacf1f00f43c305cf0748ff52c109e947d4b7aa78e7146e94fd3e98e798063` |
| `gorgon-albedo.png` | `f1421542521f466f75de4cc30ac680b3bedf8ebe710dec188b87af58e873fe34` |
| `gorgon-parameters.png` | `16f8c28cfbe334097b8013bb1bedc108022b580b384f11a59101f747a4483f61` |

Two independent Blender processes reproduce all three files byte for byte.
The actual-byte auditor passes 9,311 vertices, 11,816 triangles, 42 closed
outward components, normalized weights and identical coincident seam weights.
Bind normal agreement is 100%; two historical-frame synthetic poses pass the
same topology/weight contracts. These are not native motion observations.

The first broad planar-slab preview was rejected for floating dorsal plates;
the second was corrected for skin protrusion. Current front/side/party views
of exact exported arrays were inspected, including a 360px party view and a
Blender PBR art probe using the exact original packed map. Bevels and separate
flat plates remain readable at that distance. PBR preview response is an art
target, not proof of the installed PF shader. Local reports/renders and retained
rejected iterations remain ignored under `artifacts/cleanup2-gorgon/`.
Generation took 7.063s with the optional pose audit; a fresh no-rig repeat took
5.328s; exact old/new body and repeat checks took .874s. No game, build, package,
or native visual check was run in this lane. Owner visual approval is pending.

## Previous continuous-body repair

The current native baseline showed pillow-like ellipsoid plates, disconnected
looking thin limbs, round toe ends and a coarse muzzle. Cleanup 2 now authors
one closed heavy bovine skin with integrated neck/head/limbs/tail, eight flat
beveled cloven hoof halves, and 21 thin overlapping tapered metal shells fitted
to its surface. Creased rim normals and original burnished-edge paint distinguish
metal lames from cushions. Horn and amber-eye point sets are retained exactly.
All 28 published bone names/order and the original rig identity are retained;
nearest-triangle transfer and smoothing preserve their named skinning contract.
Sharp normal/UV seam copies receive identical weights so they cannot tear.

No unit/view scale, collider, native mesh/material/texture, blueprint, mechanic,
animation or breath-origin code is changed. The runtime still binds against the
actual native renderer. All new geometry, normals, UVs and albedo are original
project work. No proprietary pixels or geometry are committed.

## Reproducible generation

Installed Blender 4.5.10 LTS, from the repository root:

```text
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/sprint26-gorgon/build_gorgon.py -- --output-dir assets/ungulates --work-dir <ignored-private-authoring-dir>
```

The generator refuses any input differing from the retained original project
hash. It uses the existing continuous-body library and actual export geometry/
weight auditor, without changing those shared tools. No private native capture
is needed to regenerate the two shipped files.

Optional `--preview-rig <private-historical-Aurochs-frame.json>` enables only
offline synthetic pose checks and the local editable rig preview. The admitted
historical frame is `request-local summoned horse donor view`, blueprint
`KMG_Summoning_Unit_Aurochs`, mesh `KMG_aurochs_Original`, renderer `horse_01`,
28 named bones. Its real SHA-256 is
`83540b02dcdaeeedb0ec927ec33afa89a1760b1624e0c489c247cb6e3a331ef8`.
All 28 names and 14 authored landmark relationships are checked before use.
It is **not** the strict original 31-bone native Horse capture and supplies no
current native bind calibration or native motion evidence. The runtime does
not read this preview. Generation with/without it produces identical exports.
Raw captures and `.blend` files remain machine-local and ignored.

Current exported identities and commands/durations are recorded in
`planning/CLEANUP2-GORGON-NOTES.md`. Technical checks and synthetic renders do
not grant owner visual approval. Native locomotion, plate/hoof contact and
mouth-effect presentation on these changed bytes remain unverified.

## Historical plate-only gunmetal paint follow-up

The native body-5 screenshots retained a pale quilt-like plate/rim appearance.
The generator now darkens only known authored plate regions 10 through 30:
gunmetal centers, a narrower/reduced painted rim, restrained directional
brushing and overlap-edge shading. No shader branch, parameter texture,
material helper, geometry or skinning change is part of this follow-up.

`original_colour` retains the preceding color field exactly. Generation bakes
that old paint, the new proposal and a plate-ownership mask through the same
UV rasterizer/padding. It composites only exact plate-owned pixels; every
nonplate texel and unowned background texel stays byte-identical. The old
albedo must reproduce SHA-256
`f6cf501d0f8817ed327e7275736cda78bb5684f3270adee1e7c6448c5a32c9f2`.
The full decoded mesh blob must retain SHA-256
`eb5d3c7a59695312ecc954c076edd6c5947374eaf782b6ba71cb6382d03a4dcf`,
covering positions, normals, UVs, indices and every weight. JSON outputs are
explicit UTF-8/LF. The albedo metadata and `paintRevision` identify the update;
all other mesh metadata stays unchanged.

Final pair hashes, independent fresh-process equality, pixel checks and local
front/side render inspection are recorded in
`planning/CLEANUP2-GORGON-PAINT-NOTES.md`. These renders are synthetic art probes;
changed native presentation and owner visual approval remain pending.

## Original Sprint 26 provenance

The retained project input was documented as an original modest armored-bull
skin on the Horse frame, with original geometry/UVs/albedo and native bone names.
Its original generator admitted exact Horse unit
`5bb9579fdb2b26b48bb10d61c81cfdfb`, prefab
`6032f7a5751c1104aa59647b176634c8`, renderer `horse_01`, 31 captured bones.
The documented geometry-free input observer RunId was
`20261018T0439106684862Z-9ddbdd0aa753493e8171dd4337e8ac49`.
That private capture was not replayed or replaced during Cleanup 2. The supplied
28-bone Aurochs file is used only under the separate historical-preview checks
above, with no invented unit/prefab metadata.

Earlier winding/lighting corrections and native visibility records remain
historical evidence of the old exports. The old functional candidate was not
owner-approved art. Bulette retains its unrelated installed adult Bulette body.

## Completion material correction, October 22

The installed `PF/StandardDynamic` shader (sharedassets1, path ID 3835,
Unity 2018.4.10f1) was read locally and selected D3D11 programs disassembled
with the installed D3D compiler. The extracted programs remain private ignored
artifacts. Fragment 1221, SHA-256
`301cb6818fdc544e2956341e21ef206e32f74e7dcb09e34ddda401a345365788`,
uses `SPECULAR_ON` and `METALLNESS_ON`: parameter-map R is multiplied by
`_Roughness` (PFCommonBuffer byte80); B by `_Metallic` ($Globals byte136).
Fragment1152 omits both responses; 1213 enables nonmetal specular only.
The previous `_Metallic=0` therefore erased the authored plate metal mask.

The first completion candidate used multiplier1 and those two **local**
keywords, retaining the linear packed map, R roughness, B plate mask and zero
emission. Native review19 (`20261022T2145486312880Z-e40afda3d3194e9b97d3143844f4d31e`)
then showed nearly black plates in front, left, top and attack024 views.
The hard plate geometry remained useful and is preserved.

The pass name-index table resolves fragment1221 texture t3/sampler0 exactly to
`unity_NHxRoughness`. That lookup is used for direct-light specular. The
inspected fragment binds albedo, decals, parameters, this BRDF lookup and the
cluster-light index texture; it has no environment-reflection sampler. Its
diffuse multiplier is `(1-metallic)*0.779084`. The authored B mask of 0.94 at
scalar1 therefore retained only 6% of the nonmetal diffuse response outside
specular highlights. This explains a black-plate failure without assuming that
Unity failed to initialize the built-in lookup. The recorded false global
SPECULAR/METALLNESS flags do not prove that either: these material keywords are
local, and native `StandardMaterialController.Awake` caches their local state.
Installed `RenderingManager.ApplySettings` has no specular/metallic quality
toggle; `Init` enables only GLOBAL_REFLECTIONS_ON and GLOBAL_SHADOWS_ON.

The owned Gorgon material now uses `_Metallic=0.4`: effective plate metal is
`0.94*0.4=0.376`, retaining 62.4% of the nonmetal diffuse response while keeping
the genuine native albedo-colored specular path. This is a weathered-steel
mixture for the installed direct-light shader, not a pure-metal or environment
reflection claim. Roughness remains 0.23 on plates, body B remains zero and
emission stays zero. The receipt records the mask, scalar, effective metal and
diffuse retention separately. No shader/global/donor mutation, repaint, geometry,
weight or export change accompanies this correction. Lookup identity and the
changed appearance require the next bounded native review; source/disassembly
is not aesthetic acceptance.
