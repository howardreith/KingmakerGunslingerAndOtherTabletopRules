# Outsider/Celestial original bodies

Lemure, Dretch and Hound Archon are project-authored geometry with procedurally
baked original 1024px albedos. Each builder reads only geometry-free native bone
interfaces from the strict Lizardfolk Free Hands census; no game mesh, texture,
material, animation or raw private rig capture is included. The existing
project-owned paintings under `assets-source/original-icons/expanded-summoning/sources/`
are the visual references; their pixels are not copied.

## October 25 refinement

The first exports unioned closed ellipsoids and capsules and read as rounded
toys at the party camera. The refinement sculpts each visible surface as a
signed-distance field instead (`sdf_sculpt.py`): primitives placed on the
measured joints, smooth unions/subtractions with stated blend widths, folds and
wrinkles by displacement, painted material ids, and an ambient cavity term from
the same field. OpenVDB (bundled with Blender 4.5) extracts the surface, which
is put into canonical element order so repeated builds export identical bytes.
`continuous_body` then relaxes, reduces, weights, unwraps, bakes and exports it
with the existing closed-outward and one-continuous-body checks.

- **Lemure**: a pear-shaped mound with a few tilted, asymmetric sagging creases,
  an inset suffering face under a hood of flesh (brow, recessed eyes, sagging
  nose, open mouth, dripping lower lip) and heavy arms with continuous finger
  roots and black claws. The pooled base follows the pelvis; low-hanging hands
  still follow their arms.
- **Dretch**: a hunched shoulder girdle that swallows the neck, a swollen belly
  carried by a compressed chest and pelvis (folds and navel), cartilage bat ears
  grown from the skull, a heavy brow, snub nose and lips over a native-jawed
  underbite with tusks, and large connected hands and three-toed feet.
- **Hound Archon**: a mastiff skull (brow, stop, short deep muzzle, flews, lower
  jaw on the native jaw controls), heavy shoulders and arms, connected fingers
  and thumbs, digitigrade legs in boots, and real cloth geometry: a sleeveless
  tunic with neckline, belt, folded split hem and a mantle over the right
  shoulder. The garment is its own closed shell weighted from the nearest body
  vertices; the skin under it is inset so it cannot show through in the bind
  pose. No cloth simulation is used.

Jaw weights are bounded to the jaw volume, so throats, chests and bellies never
follow the jaw controls. Offline posed checks (stride, attack, bite, turn) are
authoring checks only, not native animation evidence.

Each builder keeps its exact released unit identity, measured bone set (Lemure
17, Dretch 29, Hound Archon 29), donor/prefab/renderer contract and loader
flags. The existing attachment/material controller, accessory suppression,
expiry-visual link, rollback and teardown are unchanged.

## Rig capture

The released v0.0.167 exports were built from the October 14 census
(`727645bb…`), which is not on the lab machine. The three released builders,
rerun unchanged with the October 17 recapture of the same donor
(`6443b580…`, the capture the refined theropods already use), reproduced the
released mesh payloads and albedos byte for byte except the `rigSha256` field.
The refined exports are built from that recapture, and the three visual
policies pin its hash.

## Regenerate

With installed Blender (private outputs stay outside the repository):

```
blender -b --factory-startup --python-exit-code 1 --python build_hound_archon.py -- --capture <private census.json> --albedo <hound-archon-albedo.png> --mesh-data <hound-archon-mesh.json> --report <private generation.json> --blend-out <private source.blend> --preview <private preview.png>
```

`build_lemure.py` and `build_dretch.py` take the same six parameters. Two
consecutive builds of each produce identical mesh and albedo bytes. The source
records pin the generated hashes, the builder and sculpt-module hashes, the
painting reference, the capture and the superseded v0.0.167 identities.

## Hound sword

The Free Hands donor has no native Character avatar, so the native hand-equipment
controller cannot render a carried item on it. Hound therefore instantiates its
existing native greatsword model under the measured right palm on the exact
private unit only. This runtime instance is presentation only, follows actual
item ownership/equipped mode, hides when natural/disarmed, and is destroyed on
rollback/view teardown. The native prefab/material is never modified and no
weapon asset is redistributed.

## Limits

Offline previews are not native camera-scale evidence or owner aesthetic
acceptance. Native presentation, the held-sword grip with the new hand, attack
and lifecycle views remain separate observations.
