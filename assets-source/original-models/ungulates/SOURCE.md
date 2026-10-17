# Original Sprint 11 ungulates

The Aurochs, Bison, Rhinoceros, and Woolly Rhinoceros geometry, UVs, and
paintings are original project-owned procedural work. They reuse no native
vertices, textures, materials, bind poses, or proprietary game art. The cattle
are authored against the live Horse bone names and measured bind frame; the
rhinoceroses use the live Mastodon frame. Those request-local captures and the
editable Blender sources contain donor transforms and remain private in
machine-local evidence. Only original geometry, painting, bone *names*, and
weights ship in the schema-2 mesh-data/PNG pair.

Since the visual refinement all four are built as continuous bodies with the
library in `../continuous-bodies/` (method, determinism hazards and
regeneration commands in its `SOURCE.md`): `build_rhinoceros.py` builds the
Rhinoceros and, with `--kind woolly-rhinoceros`, the Woolly Rhinoceros from
the Mastodon capture; `build_cattle.py --kind aurochs|bison` builds the cattle
from the Horse capture. Each validates one complete renderer, its unique bone
names and the expected donor family through `generate_ungulates.py`'s loader,
and its private report records the exact capture hash. Keep the capture,
report and Blender source outside the package and Git.

`generate_ungulates.py` and `paint_ungulate_albedo.py` built the first export
(table below). The generator now refuses all four kinds and names the new
builders; its spindle-leg and hoof helpers stay for the armored dinosaurs that
import them.

The first export's mesh/painting SHA-256 pairs (all superseded):

| Creature | Mesh data | Albedo |
| --- | --- | --- |
| Aurochs | `477354af79aefb21de544517c498a05b2d420fbb5ebbb1cb675613f66f09196e` | `3aa982572cccc90712dfb0798920f5b6e45827cfbc32568a6d5114bfc1a0269b` |
| Bison | `bd042f973a2a871283dea51614cbce56736f2d9d7dbc65fe6f3874ab4a193d56` | `1ec9fb91dde8f2ab6617f314efd68161b1bdeb56f00b23e8fee0667e2ba9ba7c` |
| Rhinoceros | `aa7e069491034cf8281ea19cfd228ac159e0ad2b778e14f10f1441663014e9ca` | `1568f8678a41a0ce719d307ab68b22c6d7181c24b855ac0d3af92cd2008a6cf1` |
| Woolly Rhinoceros | `45b3003b0184049921982d828faf71e95b90c2babf5acc0827f0095ec41aac52` | `bbd353bc5ecdf135fb2c93e6588d09740caa170835b73b7d4f5b30d09db02f59` |

The game uses the existing per-view renderer swap and donor bind poses. A
rejected asset leaves the donor intact. Offline renders only assess authored
shape and color; live animation, attack contact, targeting, fades, navigation,
and owner visual approval require separate runtime review.

The September 29-30 correction replaced the short circular foot-end tubes of
the first export with deterministic flattened hoof profiles and built each
Rhinoceros leg as three tapered spindle spans along the Mastodon's measured
centreline, one donor control per span. Those helpers remain in the generator.

## Rhinoceros refinement, 2026-10-16 (VIS-024)

The owner's review found a large rear opening, separated overlong legs and a
head with spikes protruding from it. The shipped mesh rendered inside out
(see `../continuous-bodies/SOURCE.md`): 14 of its 22 parts faced inward, the
torso, head, horn and tail tubes had caps wound against their walls, and every
normal opposed its winding, so the rear cap vanished and the background showed
through the body. The rigid spindle legs read as stacked barrels on the
Mastodon's long columns, and the horns rose from the top of a straight head
tube.

The Rhinoceros is now one continuous skinned surface on the same Mastodon
frame: a deep barrel hung low enough to bury the upper legs, a withers hump
and a thick neck narrower than the chest, a long tapering head carried forward
and down with a straight jaw line and a broad square upper lip, painted
nostrils and mouth line, a long front and a shorter rear keratin horn rooted
on the face and shaded dark at the root to pale at the tip, cupped ears
standing clear of the skull, three-nailed round feet and a tufted tail. The
barrel follows the spine rather than the upper-limb bones it buries. The
Mastodon's Neck and Head pivots sit high in the withers, and the heat solve
gives Neck no skin, so Neck and Head take the neck and skull through smooth
ramps along and across the head axis.

A read-only visual review of the first rebuild found the head sinking under a
hood of skin when lowered, spike-like ears, a muzzle that looked hollow from
the front, small feet and nails, and an overall hippo-like read. Close-ups
traced the hood to a hard cut-off in the Head share on the neck sides and to
the barrel's front dome wrapping the base of the head, and the hollow look to
a ball-shaped muzzle and to flat-toned horns seen end-on (culling on and off
render the same, so not a winding fault).

The first native run of the rebuilt body (development candidate
`5641fb79d0cf`, 2026-10-17) showed it solid, lit and intact, but its 0.41
grey hide read as tan under the game's warm torchlight. The hide is now a
darker, slightly cool slate (stored about 0.30) and the horns a darker brown;
the geometry and weights are unchanged.

- Capture: `sprint11-mastodon-bind-rig.json` from guarded run
  `20261017T0228570223946Z-disposable-expanded-summoning-rules` on master
  `2eed3944f6` (SHA-256
  `ce02946c67360d0b2053402e0ada584ddc368b255df66f3c8852272c04f9e414`). That
  scenario records the attached Woolly Rhinoceros renderer, so it holds the 26
  controls the bodies bind (the Mastodon's ankle helpers are absent; the
  review armature bridges them so offline poses move each foot with its knee).
- `rhinoceros-mesh.json`: `3bcd3bcfa0a73ce05a7876bcac988850ab6a62795996f2a9235d53d4222b5224`
  (5,948 vertices, 7,728 triangles, 20 closed outward parts, 26 controls).
- `rhinoceros-albedo.png`: `e8d172fdb09e68d5910dfc70623ed634696cd8347d2249b5744b49cc12770bbf`
  (1024 x 1024 RGB).
- Superseded by the recolour: mesh `c6c1c923…ae969`, albedo `91042f17…1c584`.

## Woolly Rhinoceros, Aurochs and Bison refinement, 2026-10-17 (VIS-003, VIS-017, VIS-023)

The owner approved the pilot's continuous-body style for every remaining
update. The first export of all three rendered inside out like the Rhinoceros
(82, 11 and 37 inward parts). The owner reported for the Woolly Rhinoceros the
same open rear, separated long legs and incoherent head and horns; for the
Aurochs a detached-looking tail, horns clipping into the body, open hoof
joins and a very dark, indistinct form; for the Bison missing-looking body
sections, open limb and hoof joins, a clunky spiky silhouette and weak
recognition.

- **Woolly Rhinoceros** (`build_rhinoceros.py --kind woolly-rhinoceros`): the
  Rhinoceros body hung lower on the same frame, with a high shoulder hump, a
  fuller throat, a long side-flattened front horn sweeping forward, a shorter
  rear horn, smaller ears and a red-brown coat painted with long vertical hair
  streaks over a darker underside. Two coat experiments were dropped: separate
  hanging locks read as bubbles and a belly coat mass as a ridge.
- **Aurochs** (`build_cattle.py --kind aurochs`): one deep barrel with chest,
  withers and rump masses hung low under the Horse's spine, haunches and
  shoulders that the legs grow out of, a short thick neck, a cattle skull
  carried below and ahead of the Horse's high head pivot with the chin on
  `Jaw`, long pale lyre-shaped horns with dark tips, ears standing out
  sideways, cloven hooves with a cleft, and a tail hanging from the tailhead
  with a dark switch. Dark brown coat with a pale stripe down the spine and a
  pale muzzle ring.
- **Bison** (`build_cattle.py --kind bison`): a high shoulder hump sloping to
  small hindquarters, built as one capsule chain along the back and narrowed
  after the union so it reads deeper than wide; the head hung low with a
  woolly forecap, beard and short black horns; long hair on the forelegs; a
  dark cape over a paler rear. Scattered hair clumps were tried first and
  piled up as bubbles on the back.

Rules specific to these bodies, each commented in its builder: the cattle
barrel follows the spine rather than the buried upper limb bones; `Neck2`
takes a smooth share of the neck where the body buries it; `Head` owns the
skull and `Jaw` the chin below the line of the mouth; the hanging tail is a
rooted strand weighted down the five tail controls by height. The torso masses
of the cattle are united and relaxed on a coarser union before the limbs and
head join them (`Volume.shell`, `relax`), so masses meeting over tenths of a
metre blend instead of leaving creases.

- Capture (cattle): `sprint11-horse-bind-rig.json` from the same guarded run
  `20261017T0228570223946Z-disposable-expanded-summoning-rules` (SHA-256
  `83540b02dcdaeeedb0ec927ec33afa89a1760b1624e0c489c247cb6e3a331ef8`), the 28
  Horse controls the attached Aurochs body binds.
- `woolly-rhinoceros-mesh.json`: `a53793a0283f263acf57fb4b56f20d8696ab9df6200bf547ac40f45d84da1265`
  (5,951 vertices, 8,128 triangles, 20 closed outward parts, 26 controls);
  albedo `fcdfb0c4e0926c829a62da847ba1f14802b137dd3162f2fff2fa7ac8a104e2c6`.
- `aurochs-mesh.json`: `e1cd846250eb18d3a9bb5e852814cd38069c19581d32bd8bc60f135c562cc3e9`
  (6,365 vertices, 9,280 triangles, 17 closed outward parts, the exact 28
  Horse controls); albedo
  `78d99e63a792aa2b40105167dd874c47ff52807931030e50f28bbf95c5793e01`.
- `bison-mesh.json`: `7195063b5a13117adad7edf6d3cf5c2b38ee4008c14345b5c9f51609394f3641`
  (7,226 vertices, 10,480 triangles, 17 closed outward parts, the exact 28
  Horse controls); albedo
  `1d7b9e28ba2836eb64bae3fb5c48c738d82fa3fd375edfb06a7397ba24130441`.
- Superseded: the first-export rows of the table above.
