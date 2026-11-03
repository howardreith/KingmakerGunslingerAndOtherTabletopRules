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
  `d6db6ba3b2c459a48ba0730beef0bac8` — the same measured free-hands rig the
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

Exported shape: 3130 vertices, 6000 triangles, 27 weighted bone groups — every
bone in the measured set carries geometry, including both digit chains.

Private bind measurements, `.blend` files, reports and rendered previews
remain under ignored local artifacts.

## Recorded visual limitations

These are disclosed rather than claimed as finished, and are cosmetic backlog:

- The rig carries two arm chains and this creature has four arms. The two
  large pincer arms ride the real animated chains — and each pincer's two jaws
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
