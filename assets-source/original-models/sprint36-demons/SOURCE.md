# Sprint 36 original demon bodies

Babau and Hezrou use distinct original procedural geometry and albedos authored
for Sprint 36 by `generate_sprint36_demons.py`. The script reuses the project's
established Blender 4.5 exporter and measured free-hands rig capture; it does
not distribute native geometry, textures, materials, or animation clips. The
native rig remains a runtime dependency.

The local measured capture is
`20261014T0546181325834Z-observe-expanded-summoning-sprint24-theropod-census/sprint24-theropod-census.json`,
SHA-256 `727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`.
The exact animation carrier is native donor
`a8cbd4cdb0feaf64eab1452257c3c971`, prefab
`d6db6ba3b2c459a48ba0730beef0bac8`. Private bind measurements, `.blend`
files, reports, and rendered previews remain under ignored local artifacts.

Babau has an original narrow, long-limbed body, forward skull, swept horns,
long claws, and four rigid slime lobes. Hezrou has an original broad squat
toadlike body, large belly and shoulders, forward muzzle, eye bulbs, jowls,
thick limbs, and seven visible warts. These are different authored silhouettes,
not scaled or recolored copies. Neither body creates gameplay attacks, reach,
slime, stench, or grapple behavior.

Reproduction uses:

`blender.exe -b --factory-startup --python-exit-code 1 --python generate_sprint36_demons.py -- --kind <babau|hezrou> --capture <private capture> --albedo <export.png> --mesh-data <export.json> --report <private report.json> --blend-out <private source.blend> --preview <private preview.png>`

Functional native presentation and visual review remain required. Coarse
textures, seams, and nonessential choreography are cosmetic backlog rather
than claims of finished polish.
