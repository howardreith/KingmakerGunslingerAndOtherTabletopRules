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
