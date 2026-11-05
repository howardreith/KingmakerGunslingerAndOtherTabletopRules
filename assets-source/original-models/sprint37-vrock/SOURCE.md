## PR 96 review correction

The overlapping wing masses are preserved with pointed, unequal-length primaries in interleaving groups instead of equal rounded slabs. The thorax has a keel, thighs taper under overlapping coverts, and hock/metatarsal tendons continue into branched talons. Wings remain chest-driven.

The scoped `sprint36-demons/anatomy_planes.py` authoring helper supplies original beveled jaw/forefoot masses and elongated muscle forms around the unchanged measured joints. No new driver, native geometry, animation, attack or rule is introduced. Raw capture authority remains `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`.

Regeneration uses the existing generator/arguments and the seam-safe `outsider_surface.py`. All regenerated triangles stay inside a single atlas region; all components are closed, consistently wound and outward. Exact front/side/party renders and 240-pixel party views were inspected. These are offline shape and export observations; native material/motion and owner aesthetic approval remain separate. Reports/previews live in ignored `artifacts/summon-overhaul-review/demons`.

| Current export | Vertices / triangles | SHA-256 |
| --- | --- | --- |
| `vrock-mesh.json` | 7070 / 13900 | `0f403569543e6c7d56f42d2e2fa002e48e9734039b676b3917bba330e308c100` |
| `vrock-albedo.png` | - | `fa38da0fe883df8fac27d52c74cc0bca56e0e738054b6ee7b7cfa34ec922f4ff` |

## Initial owner visual-overhaul revision (retained record)

The November 2026 owner mission brings the previously deferred cosmetic work into scope. The current exports use opt-in `continuous-bodies/outsider_surface.py` to unite organic skin and blend the existing two-influence weights. Individual teeth, horns, feather vanes and membranes retain intentional ownership. The original rig articulation limits remain. No mechanical profile, attack, route, GUID, icon or save identity changed.

Raw geometry-free input capture SHA-256 is `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`. Before revision, this locally retained capture regenerated the released Nalfeshnee geometry blob and albedo manifest byte-for-byte. New exports truthfully name this capture; the historical `727645...ad7a4` authority is retained separately. Both are an explicit closed loader allowlist.

Normal regeneration uses the existing generator/arguments below. Blender 4.5.10 and the existing pipeline are retained; private captures, blends, reports and previews stay out of Git/packages. Exact exported front, side and party-angle previews have been inspected offline. Native lighting, motion, measured world-space scale and owner aesthetic approval remain separate and unverified until the [active checkpoint](../../../planning/SUMMON-OUTSIDER-VISUAL-OVERHAUL-STATE.md) records actual observations.

| Current export | Vertices / triangles | SHA-256 |
| --- | --- | --- |
| `vrock-mesh.json` | 7638 / 15068 | `7fb5c8d54f7bda5f25986f92719a0c1b34098137d59988cce5276c1774c4687e` |
| `vrock-albedo.png` | - | `fa38da0fe883df8fac27d52c74cc0bca56e0e738054b6ee7b7cfa34ec922f4ff` |

## Earlier authoring record

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
  `d6db6ba3b2c459a48ba0730beef0bac8` â€” the same measured free-hands rig the
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
