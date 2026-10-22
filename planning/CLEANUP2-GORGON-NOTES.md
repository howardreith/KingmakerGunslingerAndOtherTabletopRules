# Cleanup 2 Gorgon refinement: reopened ART-10

2026-10-21. The previous no-change disposition in the large-creature notes
applied to the old export and is superseded by this targeted repair. Owner
visual approval and native review of the changed bytes remain **pending**.

## Native defect evidence inspected before authoring

Fresh native baseline directory:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261021T2344278601321Z-disposable-expanded-summoning-cleanup2-native-review`.
The following exact frames were opened and inspected:

| Frame | SHA-256 |
| --- | --- |
| `gorgon-refine-view-front-left.png` | `19b9d6d328e479daf1dbfaa0aa4cfb336fe4065df168a1f9179c0bde8c4a3350` |
| `gorgon-refine-view-left.png` | `ed9d49581a06ca360f33dd602d5a7ec10c22d582d145c3c922cabb300b980f92` |
| `gorgon-refine-view-rear-left.png` | `31482d3bc9c1b2bb67999a0e4b53e9a8c413f8a5d899390c9593d29e5b66446b` |

They show rounded cushion-like plates over a faceted barrel, narrow segmented
limbs, spherical paired hoof ends and coarse muzzle geometry. The horn sweep,
amber eyes and armored bovine concept are retained.

## Actual authored repair and scope

`build_gorgon.py` now reconstructs one closed heavy bovine body with integrated
neck/head, four substantial limbs and tapered tail. Eight flat-soled beveled
hoof halves replace the round toe ends. Twenty-one shallow tapered shells fit
the actual body surface: sixteen staggered flank lames, four dorsal lames and
one forehead shield. Deliberate overlap replaces vertical slots. Thickness is
0.024 body units; millimeter-scale layering separates overlap surfaces.
Creased rim normals, chamfer corners and original burnished-edge paint make
the plates read as sheet metal rather than inflated ellipsoids.

All original horn/eye point sets are checked for exact preservation. All 28
published bone names and order, donor ID, rig hash and renderer-local frame are
retained. Barycentric transfer uses the original project's actual named weights,
with smoothing on the connected skin. Normal/UV seam copies are forced to the
same weights and audited. Each foot retains two rigid terminal-driven hoof
halves. No native mesh, texture pixel, bind transform or animation is committed.

No shared registration/catalog/project/pin, runtime code, transform, collider,
blueprint, mechanic or breath-origin code is edited. The existing mouth-origin
patch still reads the native `Jaw` transform. The new muzzle reaches z=1.8361
versus approximately 1.82 before; no Jaw/Head transform or effect offset moves.
Actual mouth/FX alignment still requires native inspection of these new bytes.

## Authoring and provenance identities

| File | SHA-256 |
| --- | --- |
| `assets-source/original-models/sprint26-gorgon/build_gorgon.py` | `21616e0e16e50ae6214bbba6ef6fcba4fffc21f0a10b84180778e28b74c0f579` |
| Retained original project `cleanup2-gorgon-input.json` | `0f3c8ce10c4c0d50650ecb5a042985d1b242977716c5b7a0286754d370fdc508` |
| Current `assets/ungulates/gorgon-mesh.json` | `ab559e49083e95085056bbc1588d9d2f3515758599a79f6ced4e983f2c5479a3` |
| Current `assets/ungulates/gorgon-albedo.png` | `f6cf501d0f8817ed327e7275736cda78bb5684f3270adee1e7c6448c5a32c9f2` |

The retained project input is the exact previous authored mesh, with no private
bind-frame data. The supplied private Sprint 11 frame identifies Aurochs,
`KMG_aurochs_Original`, 28 bones, rather than the old strict 31-bone native Horse
capture with exact GUID/prefab. It was rejected as native generation admission.
Integration authorized its separate historical offline preview use after
verification. Its real SHA is
`83540b02dcdaeeedb0ec927ec33afa89a1760b1624e0c489c247cb6e3a331ef8`.
All 28 names and 14 authored landmark relationships match, maximum point error
`1.1920928955078125e-7`. This supplies no current native bind calibration or
native animation evidence. Generation without it produces identical assets.
Raw captures and editable `.blend` files remain private/ignored. See the updated
model `SOURCE.md` for original Sprint 26 and current provenance.

## Focused technical and visual checks

Blender 4.5.10 LTS, build `6dc0b208d1b5`, executable
`C:/Dev/.toolchains/blender-4.5.10-windows-x64/blender.exe`.
Commands below ran from the active worktree. `<repo>` denotes its absolute path;
the renderer requires an absolute mesh path for texture loading.

```text
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/sprint26-gorgon/build_gorgon.py -- --output-dir assets/ungulates --work-dir artifacts/cleanup2-gorgon/final --preview-rig C:/Dev/KingmakerGunslingerLab/private/visual-refinement/captures/sprint11-horse-bind-rig.json
```

Final generation: **5.710 s process / 5.047 s authoring**. Export audit **PASS**:
11,783 vertices, 14,748 triangles, 38/38 closed outward components, valid unit
normals, bounded normalized weights, correct albedo hash, identical seam
weights, all 28 drivers on the single connected body, eight cloven hoof halves,
and four preserved horn/eye point sets. Bind front-face normal agreement is
100%. Original hoof contact minima remain front y=.107 and rear y=.072.

The optional pose audit decodes and skins the **actual exported bytes**, writes
private posed JSON, and runs the same geometry/weight auditor on it. Two
historical-frame synthetic cases pass closure/winding/seam/weight checks:

| Synthetic pose | Front-face normal agreement | Posed mesh SHA-256 |
| --- | --- | --- |
| Stride | 99.2338% | `1dd1dcf65b18e0faeb59e0e85d6bad9787aaddf3ab4019e121a031a98803b22f` |
| Head turn / brace | 99.6271% | `0d8e9cc419b7db106c41666124c9510699121729007d1725929df0ce0e46529c` |

These are deliberate synthetic joint probes, not captured native animations or
ground-contact acceptance. Minor local stressed-normal disagreement remains;
the still-inspected shapes are connected and opaque. Native gait, turns,
plate/body intersection, attack/hit and mouth-effect review are still required.

Separate fresh-process regeneration into `artifacts/cleanup2-gorgon/final-repeat`
took **5.095 s process / 4.578 s authoring**, matching both assets and both
posed JSON hashes exactly. Generation without the private preview frame took
**4.724 s process / 4.235 s authoring** and also matched both assets. Thus the six independent
hash comparisons in `artifacts/cleanup2-gorgon/final-determinism.json` pass.
Additional actual refusal probes rejected a byte-different but parseable
original input (**0.564 s, expected exit 1**) and a synthetic foreign preview
identity (**0.516 s, expected exit 1**) before geometry generation.

```text
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/continuous-bodies/render_mesh_data.py -- --mesh <repo>/assets/ungulates/gorgon-mesh.json --out-dir <repo>/artifacts/cleanup2-gorgon/final/renders --name gorgon --up y --forward=+z --views party,front,side,rear --size 1000
```

Final bind renders: **2.779 s**; party/front/side/rear inspected with back-face
culling and shipped normals. The first side/front were also reviewed by
integration, followed by the sharper rim/overlap revision. Final front inspection
then found the foremost lower panel crossing the shoulder hinge; shortening
that panel corrected the crumpled edge before final regeneration and repeat.
Exact posed mesh renders were inspected too: stride party/side (**1.581 s**)
and brace front/rear/side (**1.725 s**). No frame is labelled native.
Ignored evidence lives under `artifacts/cleanup2-gorgon/`; final machine report
is `final/gorgon-report.json`, exact-byte stills are `final/renders`, and
pose stills are `final/pose-renders`. Preliminary candidate reports/renders
remain separate from the final identities above.

Initial authoring attempts correctly failed on a missing helmet ray,
inconsistent newly authored hoof cap winding, and unequal sharp-normal seam
weights. Those defects were corrected before the successful final audit; no
old failure or technical PASS was relabelled as native evidence.

`git diff --check` is clean. No game, production build, actual installation
mutation, Git mutation or frozen candidate edit was performed. Integration owns
any current validator admission/pin update and the later clean candidate batch.
