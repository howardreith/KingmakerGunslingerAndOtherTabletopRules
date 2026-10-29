# Sprint 33 original bodies

Original project-authored geometry and procedural RGB albedos. No proprietary mesh,
texture or animation bytes are included. Both use the existing measured Lizardfolk
free-hands rig and original-mesh exporter; this is distinct from their unchanged
Air Elemental and Soul Eater mechanical source donors.

Rig donor: a8cbd4cdb0feaf64eab1452257c3c971; installed prefab:
d6db6ba3b2c459a48ba0730beef0bac8. Private bind capture SHA256:
727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4.
Capture: 20261014T0546181325834Z-observe-expanded-summoning-sprint24-theropod-census/sprint24-theropod-census.json.
It contains measured bind metadata; it is not redistributed.

Generate with Blender 4.5.10 in background mode, passing this Python source and
`-- --kind invisible-stalker` (or `shadow-demon`), `--capture <private capture>`,
`--albedo <kind>-albedo.png`, `--mesh-data <kind>-mesh.json`, `--report <private json>`,
`--blend-out <private blend>`, `--preview <private png>`. The source resolves the
existing serpent/theropod exporter relative to this directory. Keep the exact
albedo basename because it is embedded in the mesh payload. Scenes and native
captures stay outside the package; only four runtime exports are shipped.

The Stalker has a head, torso, two arms and trailing wind ribbons. Shadow Demon
adds original horns, claws and scalloped wings. Native wind/shadow materials are
borrowed by installed reference and cloned privately at runtime. Offline previews
show geometry and procedural colors, not final native transparency or deformation.

Offline validation: finite vertices/normals/UVs, nondegenerate indexed triangles,
unit normals, normalized weights (at most two influences), schema size and albedo
hash binding PASS. Native attachment, idle/locomotion/attack, selection, repeated-owner
isolation, unchanged donor control and private teardown PASS in RunId
20261029T0514180832814Z-disposable-expanded-summoning-sprint32-native-review,
source126cdbc7ce390ae03461f3837035b17daf35bbcf (development artifact).
Cold-load/module-OFF persistence remains NOT_RUN. This is original-artifact evidence,
not a claim of final-release execution. The bounded native availability run
20261029T0336574203591Z-observe-expanded-summoning-sprint33-native-inputs identified
no suitable genuine Kingmaker Shadow Demon body; this deliverable uses original geometry.
Fine shoulder seams, simple fingers, flat wing transitions and basic texturing
remain visual polish; no owner aesthetic approval is asserted.

| Export | SHA256 |
|---|---|
| invisible-stalker-mesh.json | 778a3943afad71acc94fd76b8ef157caa85e954110f01c1e440d0707254092b3 |
| invisible-stalker-albedo.png | d113b4d7a011c04d9741cdf166f73a8456f5c3132b8c5e66393d2fb9184dcd51 |
| shadow-demon-mesh.json | 6085202d2e50968e57ab43dece08626756f5f44028b0b39cca4a22143704c507 |
| shadow-demon-albedo.png | a78f6d42b435e4cd99a1a3ecdf73e2b25cfb77fbbe048258f0a9957303ccbb3c |
