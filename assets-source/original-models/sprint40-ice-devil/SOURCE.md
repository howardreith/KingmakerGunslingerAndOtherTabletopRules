# Sprint 40 original Ice Devil body

`generate_ice_devil.py` authors an original insectile Gelugon on the established
measured free-hands rig. The single bounded Sprint40 installed-library capture
found no Ice Devil/Gelugon unit. No native creature geometry, texture or clip is
exported or distributed here. Skeleton/animation reuse and visible geometry are
separate: the body is original, while native donor
`a8cbd4cdb0feaf64eab1452257c3c971`, prefab
`d6db6ba3b2c459a48ba0730beef0bac8`, remains the animation carrier.

The measured capture is `sprint24-theropod-census.json`, SHA-256
`727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`.
The existing owned E: archive copy was hash-verified before authoring. The
established serpent/theropod exporter supplies rig binding, winding, original
mesh serialization and albedo writing; no new export/rig framework is added.

Distinct geometry includes two large compound eyes with lens relief, two
antennae, four mandible prongs, plated thorax over a narrow waist, two arm and
two leg chains, finger-chain claws, and a seven-segment curling tail. There
are no wings, human skull, poisonous sting or baked-in weapon. The actual
equipped spear must supply its native attachment and disappear on disarm.

The tail follows the pelvis and does not articulate independently; coarse
joint transitions, simple materials and fine choreography remain cosmetic
backlog. The first private preview hid the tail behind the trunk and was
retained; the corrected authoring geometry moves it into the visible silhouette.
Private `.blend`, capture, report and preview files remain under ignored
`artifacts/sprint40-body`; original distributable mesh/albedo are under
`assets/sprint40-ice-devil`.

Reproduction uses Blender 4.5 with absolute paths:

```text
blender.exe -b --factory-startup --python-exit-code 1 --python generate_ice_devil.py -- --capture <verified private capture> --albedo <ice-devil-albedo.png> --mesh-data <ice-devil-mesh.json> --report <private report.json> --blend-out <private source.blend> --preview <private preview.png>
```

Source/export pixels and private preview were inspected. Native attachment,
movement, attacks, cast visibility, death, selection and cleanup are still
required. No owner aesthetic approval or runtime qualification is claimed.
