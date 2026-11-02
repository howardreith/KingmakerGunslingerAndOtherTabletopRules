# Sprint 37 original Vrock body

The Vrock uses distinct original procedural geometry and an original albedo
authored for Sprint 37 by `generate_sprint37_vrock.py`. The script reuses the
project's established Blender 4.5 exporter and measured free-hands rig
capture; it does not distribute native geometry, textures, materials or
animation clips. The native rig remains a runtime dependency.

No installed Kingmaker Vrock body exists. The mechanical donor, the animation
donor and the rendered mesh are three different things and are recorded
separately here:

- animation carrier: native donor `a8cbd4cdb0feaf64eab1452257c3c971`, prefab
  `d6db6ba3b2c459a48ba0730beef0bac8` — the same measured free-hands rig the
  released Sprint 34, 35 and 36 outsiders ride;
- rendered mesh: this project's own geometry, below;
- mechanics: the Sprint 37 builder, which shares nothing with the rig.

The local measured capture is
`20261014T0546181325834Z-observe-expanded-summoning-sprint24-theropod-census/sprint24-theropod-census.json`,
SHA-256 `727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`.
That capture had aged out of the working lab and was recovered, hash-verified
against this exact value, from the owned `E:` evidence archive rather than by
spending a fresh guarded game transaction on a new census.

The Vrock has an original upright feathered trunk, a long bare neck, a small
bald skull with a long hooked beak and a downward hook spur, a feather ruff at
the neck base, two mantled wings each carrying seven tapering primaries, bare
clawed hands, and bare bird legs with three forward talons and a back spur on
each foot. That silhouette is neither the Babau's lean horned skirmisher nor
the Hezrou's broad squat toad; it is not a scaled or recoloured copy of
either. The body creates no gameplay attack, reach, stun, spore or flight
behaviour of its own.

Reproduction uses:

`blender.exe -b --factory-startup --python-exit-code 1 --python generate_sprint37_vrock.py -- --kind vrock --capture <private capture> --albedo <export.png> --mesh-data <export.json> --report <private report.json> --blend-out <private source.blend> --preview <private preview.png>`

Exported bytes:

| File | SHA-256 |
|---|---|
| `assets/sprint37-vrock/vrock-albedo.png` | `66a8f91bb08ac7e1e429434042b5822f849bd967b28a950748c34ac0cb4a93bf` |
| `assets/sprint37-vrock/vrock-mesh.json` | `559dc4c33f34b21c3bc65b07ad5a563719b0611f8ae19c59295f0d18c3db203c` |

Private bind measurements, `.blend` files, reports and rendered previews
remain under ignored local artifacts.

## Recorded visual limitations

These are disclosed rather than claimed as finished, and are cosmetic backlog:

- The rig carries no wing bones, so both wings are geometry weighted to
  `Torso_Upper`. They move with the chest and do not articulate; no flight
  animation, selectable altitude or wall traversal is claimed anywhere.
- Each wing's primaries are separate rigid blades rather than one continuous
  overlapping vane, and the leading edge is a simple swept tube.
- Plumage is a directional procedural streak rather than painted feather
  groups, and the seams between regions are visible at close range.

Functional native presentation and visual review remain required and separate.
No owner aesthetic approval is claimed.
