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
| Aurochs | `c699bf2f310faad1b27f5a8526d5ec62edd9d89de2d1b271e40c3e1dc5144857` | `3aa982572cccc90712dfb0798920f5b6e45827cfbc32568a6d5114bfc1a0269b` |
| Bison | `ded381caaad9bf5f350867d2f13391d90b5ad52b92c27360b4f1051ef2416dc7` | `1ec9fb91dde8f2ab6617f314efd68161b1bdeb56f00b23e8fee0667e2ba9ba7c` |
| Rhinoceros | `fc4196030a46cc7c71d9a2e7492b5c89a08543d8f08d3728bbf187ffe16c0723` | `1568f8678a41a0ce719d307ab68b22c6d7181c24b855ac0d3af92cd2008a6cf1` |
| Woolly Rhinoceros | `2a10b55256a9bdc578ea8eed02e8535b3348522de9b5905b48ab7a80efb5fe36` | `bbd353bc5ecdf135fb2c93e6588d09740caa170835b73b7d4f5b30d09db02f59` |

The game uses the existing per-view renderer swap and donor bind poses. A
rejected asset leaves the donor intact. Offline renders only assess authored
shape and color; live animation, attack contact, targeting, fades, navigation,
and owner visual approval require separate runtime review.
