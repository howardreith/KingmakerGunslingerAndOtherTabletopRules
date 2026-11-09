## R7 owner-playtest revision (Glabrezu)

The small arms were rooted outside the chest surface and left out of the skin union, which was the visible detachment. They now start inside subpectoral mounds and are united with the skin. The boxed head became one tapered canine skull, the banded barrel a V-shaped torso, and the forearms carry chitin vambraces. The small arms still follow the chest because the rig has only two arm chains.

Generation stays the existing Blender 4.5.10 generator interface with `PYTHONHASHSEED=0` and the local rig capture `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`; two clean runs produced byte-identical exports. Texture coordinates now come from the scoped `continuous-bodies/outsider_bake.py`, so the five atlas tiles are authoring material IDs and the albedo is baked from bind-pose position, normal and cavity. Only project-authored geometry and closed-form colour are inputs. No mechanics, reach, collision, attack, GUID or icon changed. Offline renders and native frames are presentation evidence; owner aesthetic approval is separate.

| Current export | Vertices / triangles (or size) | SHA-256 |
| --- | --- | --- |
| `glabrezu-mesh.json` | 10770 / 13280 | `1e139167b95418fc8ed102bfaa141e7d69c8f5370941031481531247d5706031` |
| `glabrezu-albedo.png` | 1024 x 1024 | `df4286933891a287a65dd78f010b689f5ad75987789429aa5c68cfe21eac0155` |

Pre-R7 exports are retained at commit `70d44c8be7` and are not packaged.

## PR 96 review correction

The larger beveled canine skull and visible swept horns carry its head silhouette. The smaller arms root higher and extend forward and laterally into negative space beside the waist, with larger hands; their chest-driven limitation remains. The pincer arms stay on the actual two native arm/digit chains. Calf/metatarsal forms and broad separated toes replace the rod/disc lower legs.

The scoped `sprint36-demons/anatomy_planes.py` authoring helper supplies original beveled jaw/forefoot masses and elongated muscle forms around the unchanged measured joints. No new driver, native geometry, animation, attack or rule is introduced. Raw capture authority remains `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`.

Regeneration uses the existing generator/arguments and the seam-safe `outsider_surface.py`. All regenerated triangles stay inside a single atlas region; all components are closed, consistently wound and outward. Exact front/side/party renders and 240-pixel party views were inspected. These are offline shape and export observations; native material/motion and owner aesthetic approval remain separate. Reports/previews live in ignored `artifacts/summon-overhaul-review/demons`.

| Current export | Vertices / triangles | SHA-256 |
| --- | --- | --- |
| `glabrezu-mesh.json` | 5424 / 10660 | `2122716bb703ebd1bef27723c2d2d4cb370786bc3930588ebdff4d1f21111ac6` |
| `glabrezu-albedo.png` | - | `e13ab445694f3e1680b6ab3db03fb2d834d5096dbc0ed69817e8c413f2523187` |

## Initial owner visual-overhaul revision (retained record)

The November 2026 owner mission brings the previously deferred cosmetic work into scope. The current exports use opt-in `continuous-bodies/outsider_surface.py` to unite organic skin and blend the existing two-influence weights. Individual teeth, horns, feather vanes and membranes retain intentional ownership. The original rig articulation limits remain. No mechanical profile, attack, route, GUID, icon or save identity changed.

Raw geometry-free input capture SHA-256 is `6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`. Before revision, this locally retained capture regenerated the released Nalfeshnee geometry blob and albedo manifest byte-for-byte. New exports truthfully name this capture; the historical `727645...ad7a4` authority is retained separately. Both are an explicit closed loader allowlist.

Normal regeneration uses the existing generator/arguments below. Blender 4.5.10 and the existing pipeline are retained; private captures, blends, reports and previews stay out of Git/packages. Exact exported front, side and party-angle previews have been inspected offline. Native lighting, motion, measured world-space scale and owner aesthetic approval remain separate and unverified until the [active checkpoint](../../../planning/SUMMON-OUTSIDER-VISUAL-OVERHAUL-STATE.md) records actual observations.

| Current export | Vertices / triangles | SHA-256 |
| --- | --- | --- |
| `glabrezu-mesh.json` | 5552 / 10916 | `47777aa2c7076d4a0d1c38b09b982d93e77666fc5115827be6e42a42be06db1b` |
| `glabrezu-albedo.png` | - | `e13ab445694f3e1680b6ab3db03fb2d834d5096dbc0ed69817e8c413f2523187` |

## Earlier authoring record

# Sprint 38 original Glabrezu body

The Glabrezu uses distinct original procedural geometry and an original albedo
authored for Sprint 38 by `generate_sprint38_glabrezu.py`. The script reuses
the project's established Blender 4.5 exporter and measured free-hands rig
capture; it does not distribute native geometry, textures, materials or
animation clips. The native rig remains a runtime dependency.

The Sprint 38 input probe found no installed Kingmaker Glabrezu body, so none
is reused. The mechanical donor, the animation donor and the rendered mesh are
three different things and are recorded separately here:

- animation carrier: native donor `a8cbd4cdb0feaf64eab1452257c3c971`, prefab
  `d6db6ba3b2c459a48ba0730beef0bac8` â€” the same measured free-hands rig the
  released Sprint 34, 35, 36 and 37 outsiders ride;
- rendered mesh: this project's own geometry, below;
- mechanics: the Sprint 38 builder, which shares nothing with the rig.

The local measured capture is
`20261014T0546181325834Z-observe-expanded-summoning-sprint24-theropod-census/sprint24-theropod-census.json`,
SHA-256 `727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`,
the same hash-verified capture Sprint 37 recovered from the owned `E:`
evidence archive. No fresh guarded game transaction was spent on a new census.

The Glabrezu has an original broad upright plated trunk, a canine head with a
long muzzle, bared fangs and four swept horns, two very large forward-reaching
pincer arms whose jaws each carry interlocking teeth, two smaller clawed arms
on the chest below them, and heavy digitigrade legs. Four visible arms, two
visible pincers and the canine horned head are what separate this silhouette
from the released Sprint 34-37 outsiders: it is neither the Vrock's feathered
beaked bird-legged trunk nor the Babau's or Hezrou's two-armed shapes, and it
is not a scaled or recoloured copy of any of them. The body creates no
gameplay attack, reach, rend, stun, spell-like or teleport behaviour of its
own; all five printed attacks, the rend gate and Power Word Stun live in the
builder and are observed separately.

Reproduction uses:

`blender.exe -b --factory-startup --python-exit-code 1 --python generate_sprint38_glabrezu.py -- --kind glabrezu --capture <private capture> --albedo <export.png> --mesh-data <export.json> --report <private report.json> --blend-out <private source.blend> --preview <private preview.png>`

Exported bytes:

| File | SHA-256 |
|---|---|
| `assets/sprint38-glabrezu/glabrezu-albedo.png` | `564cc4a54289535f67c296ae44b24111afa6f09ef24394e79d649a9c14d376fd` |
| `assets/sprint38-glabrezu/glabrezu-mesh.json` | `5162a8614cd2d31dc98f0233962f79f035aae7c848cd249a51358bafea1d3b95` |

Exported shape: 3130 vertices, 6000 triangles, 27 weighted bone groups â€” every
bone in the measured set carries geometry, including both digit chains.

Private bind measurements, `.blend` files, reports and rendered previews
remain under ignored local artifacts.

## Recorded visual limitations

These are disclosed rather than claimed as finished, and are cosmetic backlog:

- The rig carries two arm chains and this creature has four arms. The two
  large pincer arms ride the real animated chains â€” and each pincer's two jaws
  ride that arm's `finger1/finger2` and `Bfinger1/Bfinger2` digit chains, so
  the native grip animation opens and closes the claw. The two smaller chest
  arms are geometry weighted to `Torso_Upper`: they move with the chest and do
  not articulate independently. That is the same accepted shape the released
  four-armed Girallon and Xill take on a two-armed rig, and the same one the
  Sprint 37 Vrock's wings take. No four-limb articulation is claimed anywhere.
- The pincer jaws are swept rigid blades with inserted teeth rather than one
  continuous hinged shell, and the hinge itself is a simple joint sphere.
- Hide, plate and chitin are directional procedural regions rather than
  painted scute groups, and the seams between regions are visible at close
  range.
- Fine visual polish is deliberately deferred for this release.

Functional native presentation and visual review remain required and separate.
No owner aesthetic approval is claimed.
