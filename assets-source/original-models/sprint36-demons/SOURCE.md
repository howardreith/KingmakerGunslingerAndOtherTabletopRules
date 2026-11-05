## Current owner visual-overhaul revision

The November 2026 owner mission brings the previously deferred cosmetic work into scope. The current exports use opt-in `continuous-bodies/outsider_surface.py` to unite organic skin and blend the existing two-influence weights. Individual teeth, horns, feather vanes and membranes retain intentional ownership. The original rig articulation limits remain. No mechanical profile, attack, route, GUID, icon or save identity changed.

Raw geometry-free input capture SHA-256 is `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`. Before revision, this locally retained capture regenerated the released Nalfeshnee geometry blob and albedo manifest byte-for-byte. New exports truthfully name this capture; the historical `727645...ad7a4` authority is retained separately. Both are an explicit closed loader allowlist.

Normal regeneration uses the existing generator/arguments below. Blender 4.5.10 and the existing pipeline are retained; private captures, blends, reports and previews stay out of Git/packages. Exact exported front, side and party-angle previews have been inspected offline. Native lighting, motion, measured world-space scale and owner aesthetic approval remain separate and unverified until the [active checkpoint](../../../planning/SUMMON-OUTSIDER-VISUAL-OVERHAUL-STATE.md) records actual observations.

| Current export | Vertices / triangles | SHA-256 |
| --- | --- | --- |
| `babau-mesh.json` | 4875 / 9690 | `0c25a363b466c46d76610a48358a3b3dd934f77abfa6ffa132c60a5c54d0f5c7` |
| `babau-albedo.png` | - | `cbc8efb3a1488d4a71efbef98f257d810b7b2bf0046d0c5b7824e1ae51611006` |
| `hezrou-mesh.json` | 5018 / 9944 | `833989f1ea873af21e2e7684d5692177a74964d28be2a4f962b99412b651eae3` |
| `hezrou-albedo.png` | - | `f9196f5340b80cd40b1535c1ec15ba36937495cb0174f2a50aaf81be5e107386` |

## Earlier authoring record

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
