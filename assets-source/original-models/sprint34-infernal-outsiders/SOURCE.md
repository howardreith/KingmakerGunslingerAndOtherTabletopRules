## Owner visual overhaul, November 2026 (development draft)

The current Barbazu and Kyton exports supersede the original surface treatment
described below. They remain original authored geometry; native meshes, clips,
materials and textures remain runtime references and are not copied.

Both bodies now opt into `continuous-bodies/outsider_surface.py`: a voxel union
of the selected organic surfaces, with transferred and smoothed two-influence
skin weights. Eyes, horns, beard, clothing and chains stay intentional separate
solids. The measured `Torso_Upper` joint is at the abdomen, below the shoulder
joints; new pectoral and clavicle masses therefore sit above that joint. The
neck explicitly connects to the skull. Revised brows, cheeks, hips and fitted
ragged leather replace the unbroken mannequin surface.

Barbazu retains its original native glaive dependency, grip transform and weapon
ownership. Its beard is a shorter arrangement of curved locks rooted through
the jawline, leaving the mouth, chest and glaive lane clear. Beard geometry
follows Head; no independent beard simulation or new attack exists.

Kyton's previous first chain centre was approximately 0.121 renderer units
from its palm centre before posing. Each new branch starts approximately 0.032
units from that same centre, within the hand/manacle volume. Root manacles,
fitted torso loops and crossed shoulder chains establish credible attachments;
the four attack chains drape down from the wrists at rest. Their four disjoint
vertex receipts are regenerated after the body union, and every attack-chain
vertex remains rigidly weighted to its declared palm. The runtime's actual
`bone.localToWorldMatrix * bindpose` path and private mesh cloning are preserved;
this revision does not invent a coordinate-space fix. Native attack identities,
contact timing, extension and follow-through remain unchanged.

The available geometry-free native capture has SHA-256
`6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`, rather
than the historical raw capture `727645...` below. A no-edit Nalfeshnee
regeneration from this available capture reproduced the released base64 mesh
payload and albedo exactly. The capture has the same measured donor geometry,
but is recorded under its real raw-file hash. The runtime permits only the
two reviewed capture identities; no old hash is stamped onto new input bytes.

Regenerate with Blender 4.5.10, the existing CLI below, this reviewed capture,
and the checked-in opt-in helper. No other creature generator is selected by
these commands. Put reports, `.blend` files and previews under ignored artifacts.

| Export | SHA-256 | Vertices / triangles |
| --- | --- | --- |
| bearded-devil mesh | `058b0b40f6a09949faca459e917896210d1dd922da0704be387754efb2f97bec` | 4,389 / 8,678 |
| bearded-devil albedo | `fd61649a0caf17e9ccd8d66e55092d2c1d98df88509295de98958c5b78d4c888` | 512 x 512 |
| kyton-chain-devil mesh | `fcb2b0082bcdabeee453d3aa1128acb1c939f2394940db023d769dcdb4bbdc80` | 17,904 / 34,952 |
| kyton-chain-devil albedo | `06bdc66deefd1d573953d8422f4b71573e8a4d8d9171f1394537dd33ac12941d` | 512 x 512 |

Exact-export front, party-camera and side previews were inspected offline.
The orientation audit found 25 closed outward Barbazu parts and 214 closed
outward Kyton parts, with no inconsistent shared edge or inward component.
These facts do not prove in-game lighting, glaive contact, second-instance
chain isolation, motion, view reconstruction or removal. Those native checks
and owner aesthetic approval remain separate and unclaimed. The older native
PASS records below belong to their original bytes. Kyton has more triangles
than the released body, so mixed-group native rendering cost remains a check.


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
These are distinct original meshes. Body05 establishes native attachment,
contact, controlled-chain extension and motion. Technical and agent image review
are separate from personal owner aesthetic approval. Rough seams and simple beard/chain animation are deferred cosmetics.

Reproduction uses `blender.exe -b --python generate_infernal_outsiders.py --`
with `--kind`, the exact private `--capture`, and absolute `--albedo`,
`--mesh-data`, `--report`, `--blend-out`, `--preview` paths. Keep the last three
under ignored local artifacts. The runtime JSON and albedo go under
`assets/sprint34-infernal-outsiders/`.

Offline decoder, finite vertex/normal/UV data, outward winding, chain indices
and matching albedo hashes passed. Body03/source124c370b observed both live
bodies, independent resources, native movement and actual attack clips; its raw
29/29 result remains unchanged. Agent image review leaves attack presentation
open: chain images do not clearly reach the target despite manual vertex gaps,
and glaive contact alignment is unproved. Body04/source3f5fffa9 native BakeMesh agrees with actual contact vertices,
but ordinary next frames show the chain reset after UnitAttack advanced its
planned limb. The correction retains the contacted limb for a0.35s visual
follow-through. The glaive's measured native localY axis supplies a fixed grip
correction. Body05/source6c20b2f7, RunId
`20261030T0014352465588Z-d0ed1ae39feb4339919786bd7dc387a6`, passed32/32.
Actual glaive blade contact and nonresting chain follow-through were observed;
BakeMesh/manual contact gaps were below0.000006m. Agent-reviewed ordinary frames
`contact-0-after-1` and Kyton `contact-5-after-1` visibly connect at normal and
controlled reach. This closes functional presentation on that development
artifact; final package presentation remains required. Rough seams, simple
motion and minor clipping remain cosmetic backlog.
No owner aesthetic approval is claimed.
The private glaive presentation selects the retained native main-hand mapping
without changing weapon category, damage, reach or native action consumers.

Mesh SHA256:
- bearded-devil: `e636ee491240107f1c1db79c4747934e69ad9b4cda539cc0619b5dc47c664ad5`
- kyton-chain-devil: `f76800cef1783dd901839a8df4935556c95dec3cb6b1b72ac76cfa995d8f0428`
