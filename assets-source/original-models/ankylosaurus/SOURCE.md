# Ankylosaurus original body

The mesh, UVs and albedo are original deterministic project work. The editable
sources are `generate_ankylosaurus.py` and `paint_ankylosaurus.py`; the generator
also writes an editable Blender scene to the requested private output path.
No native mesh, texture, animation curve or proprietary assembly is included.

The geometry-free native capture is the Sprint 22 three-donor survey, RunId
`20261012T1037196397495Z-observe-expanded-summoning-sprint22-contracts`, SHA-256
`df5dcae9a746a3863d3496bff3759996f5f635f1782dcbd7c6b6f256cafb3893`.
Its Hodag renderer supplies 24 reviewed drivers, including six tail joints and
four leg chains. The native `Hodag_attack_tail` action is 1.3 seconds; the donor
has no native Hit animation. Captured native rest metadata and the resulting
editable rig remain private. Runtime binds the original geometry to each
instance's actual native transforms through the released attachment system.

Regenerate with Blender 4.5 and Python 3, using `--python-exit-code 1`:

```text
python paint_ankylosaurus.py --out <exports>/ankylosaurus-albedo.png
blender -b --python-exit-code 1 --python generate_ankylosaurus.py -- --capture <private-survey.json> --albedo <exports>/ankylosaurus-albedo.png --mesh-data <exports>/ankylosaurus-mesh.json --report <private-report.json> --blend-out <private-source.blend>
blender -b --python-exit-code 1 --python review_ankylosaurus.py -- --source <private-source.blend> --out <private-review-directory>
```

The offline review covers clay, silhouette, textured, unlit, party camera and
tail extremes. Those poses do not prove native strike contact. The existing
approved Ankylosaurus icon depicts an armored club-tail animal and is retained.
Owner aesthetic approval remains NOT_PERFORMED_NONBLOCKING.

## Refinement, 2026-10-17 (VIS-028)

The owner found the basic armored shape a useful start but the face confused,
the tail-club lobes oversized and bulbous, and the club faces bright cyan; the
first export rendered inside out (79 inward parts). `build_ankylosaurus.py`
(library in `../continuous-bodies/`) now builds it on the same Hodag survey as
one continuous body with the same 24 reviewed drivers: a very broad, low torso
relaxed as one surface, short sprawled legs on the measured bones (the hind
leg runs from the body straight to the knee, since the Hodag's upper-leg joint
sits below it), a small beaked head with a rigid lower jaw on `Jaw_M` and
short horns at the back corners of the skull, rows of domed osteoderms across
the back with lateral spikes, smaller spikes down the tail, and a moderate
two-lobed club on `Tail5_M`. Armor, spikes and club are bone-tan over an
olive-brown hide. `generate_ankylosaurus.py` now refuses to build and keeps
its loader, `DONOR` and `DRIVERS`.

- Capture: `sprint22-ankylosaurus-contracts.json` from guarded run
  `20261017T1417501790103Z-observe-expanded-summoning-sprint22-contracts` on
  development candidate `c6d53f636527` (SHA-256
  `33ce3ad77a73d752d53aeae7de9effdc3b3b20a94fc8b2949d511d697f72a1c1`).
- `ankylosaurus-mesh.json`: `3eb7585e6f410711c7bf84bf4e23ff0df2c04fbe06ec226a0552a715c9fa9ca9`
  (11,520 vertices, 14,560 triangles, 75 closed outward parts).
- `ankylosaurus-albedo.png`: `2cdd8bc7e9139e748a9a88ec52e8a360245b6ca5ecb7829eec78a57d5a598bac`.
- Superseded: mesh `02f995bae67f36b591e67442ac967734c3f29fd3d2eebdb12761100733997c65`,
  albedo `f9255a77008fbcd001350bdeeb408af6f34adb39f55869337c224f8899602050`.
