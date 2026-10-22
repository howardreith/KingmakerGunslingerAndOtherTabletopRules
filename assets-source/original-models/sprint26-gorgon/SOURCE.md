# Original Gorgon body: Cleanup 2 refinement

Current authoring is `build_gorgon.py`, using the retained exact original
project-authored `cleanup2-gorgon-input.json`. Its SHA-256 is
`0f3c8ce10c4c0d50650ecb5a042985d1b242977716c5b7a0286754d370fdc508`.
This input contains original geometry, UVs, painting identity and named weights;
it contains no native vertices, textures, bind transforms or animation curves.

## Reopened ART-10 repair

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

## Plate-only gunmetal paint follow-up

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
