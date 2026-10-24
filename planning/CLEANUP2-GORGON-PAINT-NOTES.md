# Gorgon plate-only gunmetal paint correction

2026-10-22 development checkpoint. Native presentation on these changed bytes
and owner visual approval are **pending**. This is a diffuse paint correction;
no metallic/specular shader behavior is claimed or enabled.

## Evidence and exact scope

Opened actual native body-5 `gorgon-refine-view-front-left.png` and
`gorgon-refine-view-left.png` under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261022T0049060405024Z-disposable-expanded-summoning-cleanup2-native-review`.
Broad pale plate centers and bright painted borders read as white quilting.
The exact before-refinement material observation retained the correct owned
1024 atlas, white `_TintColor`, `PF/StandardDynamic` in Gamma, null `_BumpMap`
and `_Parameters`, and inherited Horse keywords without metallic/specular
branches. Actual roughness/metallic/emission scalars were not captured there;
shader defaults must not be substituted for observations.

Only authored plate regions 10..30 changed. Center color field changed from
(.48,.55,.57) to (.285,.330,.345), with restrained directional brushing. Bright
rim width changed from .025 to .008 authored units and rim addition from .18
to (.055,.060,.062), with a narrow dark overlap-edge falloff. These are original
albedo marks. Body, horn, hoof, eye, nostril pixels and all mesh data remain
unchanged. No shader branch, parameter texture, generic material helper, native
asset, gameplay or attack gesture file was changed.

## Reproduction and preservation

`assets-source/original-models/sprint26-gorgon/build_gorgon.py` retains the prior
`original_colour` function and deterministically regenerates its exact old atlas.
The same UV rasterizer/padding generates an explicit known plate-region mask.
The new atlas composites only mask-owned plate pixels, preserving all other
texels including unowned atlas background. Generation refuses unless the old
paint exactly reproduces its recorded SHA and the full decoded exported mesh
blob equals the preceding geometry/normal/UV/index/weight hash. JSON uses
explicit UTF-8/LF. `SOURCE.md` documents this workflow.

| Check | Result |
| --- | --- |
| Old albedo reproduced byte-identically | Yes, `f6cf501d0f8817ed327e7275736cda78bb5684f3270adee1e7c6448c5a32c9f2` |
| Changed plate/padding pixels | 508310 |
| Changed nonplate pixels | **0** |
| Full decoded mesh blob byte-identical | Yes, `eb5d3c7a59695312ecc954c076edd6c5947374eaf782b6ba71cb6382d03a4dcf` |
| Protected metadata identical | Yes; only albedo metadata and new paintRevision differ |
| Fresh independent export pair | Byte-identical JSON and PNG |
| Final generator guard rerun | Same exact pair |
| Export JSON line endings | LF only |
| Authored directory Python cache | None found |

Final shipped pair:

- `assets/ungulates/gorgon-mesh.json`: **`2a920dc65854367b282b38d7b19f4b536b27a2a56ae314a7787f4e6934061068`**.
- `assets/ungulates/gorgon-albedo.png`: **`cf66f22224e040cbd65fcec717ab41f9786ec0cefd02dd4534d322eec82721a4`**.

Prior mesh JSON `ab559e49083e95085056bbc1588d9d2f3515758599a79f6ced4e983f2c5479a3`
differs only in albedo metadata/new paint revision, not decoded model bytes.
Stable unit, prefab, bone palette, rig identity and published keys are retained.

## Commands and local inspection

Used installed Blender 4.5.10 LTS and its embedded Python. From the worktree:

```text
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/sprint26-gorgon/build_gorgon.py -- --output-dir artifacts/cleanup2-gorgon/paint-candidate --work-dir artifacts/cleanup2-gorgon/paint-candidate/work
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/sprint26-gorgon/build_gorgon.py -- --output-dir artifacts/cleanup2-gorgon/paint-repeat --work-dir artifacts/cleanup2-gorgon/paint-repeat/work
python artifacts/cleanup2-gorgon/verify-paint-only.py
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/continuous-bodies/render_mesh_data.py -- --mesh <absolute-worktree>/artifacts/cleanup2-gorgon/paint-candidate/gorgon-mesh.json --out-dir <absolute-worktree>/artifacts/cleanup2-gorgon/paint-candidate/renders --name gorgon-gunmetal --up y --forward=+z --views front,side --size 1000
```

Initial fresh generation **11.344 s**; independent repeat **14.500 s**; final
guarded generator run **15.547 s**, same pair. Local front/side render invocation
**5.498 s**. The first render invocation used relative input paths, which the
Blender image loader could not resolve; it failed before output. Correcting to
absolute paths produced the inspected images. The failure is not presentation
evidence. Equality checker completed successfully (~1 s invocation).

Opened and inspected both `gorgon-gunmetal-shipped-front.png` and `-side.png`:
the broad white painted piping is removed and plates read dark with restrained
edge/brushing marks. Existing rounded underlying forms and local Blender
lighting remain visible. These are **synthetic bind-pose art probes**, not
native shader response, locomotion, metalness, breath, collision or owner
acceptance evidence. No geometry adjustment is hidden in this correction.

Ignored local evidence is under `artifacts/cleanup2-gorgon/paint-before`,
`paint-candidate`, `paint-repeat`, plus `paint-equality-report.json` and its
small verifier. No native assets, game/save writes, build, C# test execution,
Git mutation or commit occurred in this lane. Root owns the exact new
`gorgon-presentation` full outdoor packet, actual scalar/keyword observations,
guarded native staging and current package pin update. Missing native Hit
support and other retained failures are not waived by this paint correction.
