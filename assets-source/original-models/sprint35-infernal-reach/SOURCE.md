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
