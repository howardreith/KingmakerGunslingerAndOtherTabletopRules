# Sprint 34 original bodies

Original procedural geometry and albedos authored for this mission using
`generate_infernal_outsiders.py`, Blender 4.5, the existing S17 exporter and S24
measured rig importer. No extracted game geometry, texture, animation clip or
private assembly is distributed. The native rig remains a runtime dependency.

The local capture is `20261014T0546181325834Z-observe-expanded-summoning-sprint24-theropod-census/sprint24-theropod-census.json`
under the lab runtime-evidence directory, SHA256
`727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`.
Donor `a8cbd4cdb0feaf64eab1452257c3c971`, prefab
`d6db6ba3b2c459a48ba0730beef0bac8`, provide the free-hand animation rig.
Private bind measurements, blend files and rendered previews remain local.
Exports contain original vertices/UVs/weights and 27 required bone names.

Barbazu has a green devil body, horns, beard and a separately bound native glaive
visual. Kyton has a pale chain-wrapped body and four weighted barbed chains.
These are distinct original meshes. Native attachment/contact, controlled-chain
extension and motion acceptance are pending; offline appearance is not owner
approval. Rough seams and simple beard/chain animation are deferred cosmetics.

Reproduction uses `blender.exe -b --python generate_infernal_outsiders.py --`
with `--kind`, the exact private `--capture`, and absolute `--albedo`,
`--mesh-data`, `--report`, `--blend-out`, `--preview` paths. Keep the last three
under ignored local artifacts. The runtime JSON and albedo go under
`assets/sprint34-infernal-outsiders/`.

Offline decoder, finite vertex/normal/UV data, outward winding, chain indices
and matching albedo hashes passed. Native motion/attachment remain NOT_RUN.

Mesh SHA256:
- bearded-devil: `e636ee491240107f1c1db79c4747934e69ad9b4cda539cc0619b5dc47c664ad5`
- kyton-chain-devil: `f76800cef1783dd901839a8df4935556c95dec3cb6b1b72ac76cfa995d8f0428`
