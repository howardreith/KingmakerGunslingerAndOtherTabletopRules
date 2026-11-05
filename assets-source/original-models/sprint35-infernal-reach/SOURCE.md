## PR 96 review correction: original devil surfaces

Bone retains the recognizable ivory skull and exposed ribs. Its revision replaces round joint knobs with flatter flared condyles, tapered/paired shafts, swept iliac crests and clawed feet. Two closed insectile membrane sheets with veins replace the shoulder spike fans. The tail uses the original six landmarks and tip/ownership contract with a smooth sweep and shallow plates. Barbed retains its 34 rooted thorns, with tapered calves/hocks and substantial clawed feet. No mechanical profile, attack timing, stable identity or donor animation is changed.

The shared reviewed skin helper preserves atlas seams per face. All four final meshes have zero triangles spanning atlas regions in `tools/check_original_mesh_atlas.py`. Link regression tests pass 3/3; combined link+four-asset atlas check wall duration 0.462 seconds. Front and elevated views of all four, plus Bone side, were inspected as offline shape evidence. Native material/motion, selection/contact/clearance and owner aesthetic approval remain separate and pending. Raw capture provenance remains the previously reviewed `.6443...e65d` geometry-free input.

| Current export | Vertices / triangles | SHA-256 |
| --- | --- | --- |
| `barbed-devil-mesh.json` | 6861 / 13410 | `93aa5740103861bc692714363c738d57fd1fc1818e58141af2c5f4cc257abda3` |
| `barbed-devil-albedo.png` | - | `ee6d695aa052562c72a2fa2018546f1a6da0fef2ea9cea85d8416f536da1bc79` |
| `bone-devil-mesh.json` | 5911 / 11440 | `286d75dbd83d9299fe52094d1ed8d30c02fdc362b55a0c10b62d8c58670e141a` |
| `bone-devil-albedo.png` | - | `908e53eb79b80ea1fb817c1ae9d87fabf032304d0174767fa1b91d51a2e636df` |

## Earlier visual-overhaul record

## Owner visual overhaul, November 2026 (development draft)

The current Barbed and Bone Devil exports supersede the original coarse bodies
described below. Their authored sources opt into the existing continuous-body
voxel union through `continuous-bodies/outsider_surface.py`, selecting organic
skin and skeletal scaffold regions and transferring two-influence weights.
Deliberate eyes, thorns, exoskeletal plates, wing vanes and the separately
controlled stinger remain rooted discrete solids. No native geometry, texture,
material or clip is copied.

Barbed Devil receives a connected muscular chest and limb surface, face planes,
rooted shoulder/back/limb thorn groups and a short visual barbed tail. The
shoulder thorns are located against the measured shoulder joints so that their
tips clear the body at gameplay distance; a first draft buried them in the
lower torso and was rejected during offline review. There are 34 authored
thorns, with the exact anatomy guard updated accordingly. The new tail is only
appearance: no attack, reach or mechanical tail consumer is added.

Bone Devil now has an ivory skull with recessed sockets and mandibular hooks,
an explicit neck-to-skull bridge, an exposed ribcage and sternum at actual chest
height, pelvic plates, long fingers with separate dark claw tips and rooted
overlapping insectile wing vanes. The darker recessed scaffold and lighter
plates distinguish it from Babau. The existing pelvis-owned six-segment tail
retains its real native sting timing and ownership; its root remains embedded
in the pelvis, with revised curvature and stinger proportion. The exported
tail vertex receipt is regenerated after the body union. The wing vanes move
with the chest, without independent wing articulation.

The actual available geometry-free rig capture is SHA-256
`6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`. The
historical `727645...` capture is retained below as provenance for the released
body. An unchanged Nalfeshnee regeneration demonstrated that the available
capture produces exactly the released mesh payload and albedo. New exports
record the real available input hash, accepted by the closed two-capture
runtime guard; they never mislabel it as the historical raw file.

Regenerate with Blender 4.5.10, the existing CLI below, the reviewed available
capture and the checked-in helper. Reports, `.blend` files and previews are
ignored local artifacts. Only the two named kinds are selected by this source.

| Export | SHA-256 | Vertices / triangles |
| --- | --- | --- |
| barbed-devil mesh | `2d4e45ad81659ebdfb6f5a299360e2f2910f8cd4214bbb6edf776d52c602e393` | 7,070 / 13,844 |
| barbed-devil albedo | `ee6d695aa052562c72a2fa2018546f1a6da0fef2ea9cea85d8416f536da1bc79` | 512 x 512 |
| bone-devil mesh | `008f22be80cfc7a93202d09ede66398e213550ddfb0da9f0290967056dbcae62` | 7,272 / 14,300 |
| bone-devil albedo | `0e66a075029f433f5f0dec35aa0c5950b878d86f579e3150f449dd1e1f3e62d8` | 512 x 512 |

Exact-export front, party-camera and side previews were reviewed offline. The
orientation audit found 71 closed outward Barbed parts and 65 closed outward
Bone parts, with no inconsistent shared edge or inward component. This does
not establish ordinary indoor/outdoor game lighting, tail contact, movement,
selection, view reconstruction or owner aesthetic approval. Increased mesh
cost also needs the selected mixed-group native observation. Wall of Ice is
an independent gameplay investigation and is not closed by these art exports.


# Sprint 35 original bodies

The Barbed Devil and Bone Devil use original procedural geometry and albedos
authored for Sprint 35 by `generate_infernal_reach.py`. The script reuses the
project's established Blender 4.5 exporter and the measured free-hands rig
capture; it does not distribute native geometry, textures, materials, or
animation clips. The native rig remains a runtime dependency.

The local measured capture is
`20261014T0546181325834Z-observe-expanded-summoning-sprint24-theropod-census/sprint24-theropod-census.json`,
SHA-256 `727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`.
The exact animation carrier is native donor
`a8cbd4cdb0feaf64eab1452257c3c971`, prefab
`d6db6ba3b2c459a48ba0730beef0bac8`. Private bind measurements, `.blend`
files, reports, and rendered previews remain under ignored local artifacts.

Barbed Devil has an original broad red body, horns, claws, and twenty visible
rigid thorns. Bone Devil has an original narrow exoskeletal body, horned skull,
joint shells, two simple ribbed wings, and a six-segment stinging tail. The
tail's exported vertex receipt lets a private visual component extend only the
tail to the native sting target at contact; it does not create attacks, reach,
or damage.

Reproduction uses:

`blender.exe -b --factory-startup --python-exit-code 1 --python generate_infernal_reach.py -- --kind <barbed-devil|bone-devil> --capture <private capture> --albedo <export.png> --mesh-data <export.json> --report <private report.json> --blend-out <private source.blend> --preview <private preview.png>`

Functional native presentation and visual review remain required. Simplified
wings, coarse textures, seams, and nonessential choreography are cosmetic
backlog, not claims of finished polish.
