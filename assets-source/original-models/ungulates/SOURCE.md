# Original Sprint 11 ungulates

The Aurochs, Bison, Rhinoceros, and Woolly Rhinoceros geometry, UVs, and
paintings are original project-owned procedural work. They reuse no native
vertices, textures, materials, bind poses, or proprietary game art. The cattle
are authored against the live Horse bone names and measured bind frame; the
rhinoceroses use the live Mastodon frame. Those request-local captures and the
editable Blender/FBX previews contain donor transforms and remain private in
machine-local evidence. Only original geometry, painting, bone *names*, and
weights ship in the schema-2 mesh-data/PNG pair.

Use Blender 4.5.10 LTS. For each key, first run
`paint_ungulate_albedo.py -- --kind <key> --out assets/ungulates/<key>-albedo.png`,
then run `generate_ungulates.py -- --kind <key> --capture <private bind-rig.json>
--albedo assets/ungulates/<key>-albedo.png --mesh-data
assets/ungulates/<key>-mesh.json --report <private report.json> --blend-out
<private source.blend> --fbx-out <private preview.fbx>`. Use the Horse capture
for Aurochs/Bison and the Mastodon capture for both Rhinoceroses. The
generator validates one complete renderer, its unique bone names, and the
expected donor family before writing the original mesh. Its private report
records the exact capture hash. Keep the capture, report, Blender and FBX
outside the package and Git.

The shipped mesh/painting SHA-256 pairs are:

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

The September 29-30 correction replaces the short circular foot-end tubes with
deterministic flattened hoof profiles. Cattle use two compact parallel lobes;
rhinoceroses use one broad rounded pad. Each Rhinoceros leg follows the
Mastodon donor's measured bind-frame centreline as three tapered spindle spans.
No face crosses a donor pivot: each span has one exact donor control, closes
to a point inside the next span, and overlaps it directly. No
separate joint solid is present. The donor's short ankle helper turns the
separately authored foot but does not receive a fourth visible sleeve. This
keeps an upper limb, knee section, lower limb, and foot silhouette without
linear-blend collapse, stretched bridge faces, open holes, broad cap disks,
stacked barrels, bead-like joint covers, or one upper-control column. The
geometry change does not alter collision, mechanical size, donor assets, or
runtime ownership.
