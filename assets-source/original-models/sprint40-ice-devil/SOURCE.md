## R7 owner-playtest revision (Ice Devil)

The separate pale beads and black rods are now one fused skin. Limb sheaths taper into dark joints, dorsal and ventral plates overlap a dark slate under-body, the shoulders carry one pauldron plate, the head is a narrower wedge under a heavy brow with framed eyes and four mandibles, and tail rings wrap a continuous core. The silhouette and equipped-spear contract are preserved; extents stay within 0.02 of the prior export. The export names the retained equivalent capture, so the loader now admits the same reviewed rig pair as its sibling outsiders.

Generation stays the existing Blender 4.5.10 generator interface with `PYTHONHASHSEED=0` and the local rig capture `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`; two clean runs produced byte-identical exports. Texture coordinates now come from the scoped `continuous-bodies/outsider_bake.py`, so the five atlas tiles are authoring material IDs and the albedo is baked from bind-pose position, normal and cavity. Only project-authored geometry and closed-form colour are inputs. No mechanics, reach, collision, attack, GUID or icon changed. Offline renders and native frames are presentation evidence; owner aesthetic approval is separate.

| Current export | Vertices / triangles (or size) | SHA-256 |
| --- | --- | --- |
| `ice-devil-mesh.json` | 13741 / 14136 | `ac3468c597fe97752f5544d9da975c1c6dae0ca5e0d07d3a9ed9f3ca0ee9d700` |
| `ice-devil-albedo.png` | 1024 x 1024 | `a7e25c51737fcc5679cc29ed194248fd2eeb81eac3c682f34dfcec7df86e8351` |

Pre-R7 exports are retained at commit `70d44c8be7` and are not packaged.

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
