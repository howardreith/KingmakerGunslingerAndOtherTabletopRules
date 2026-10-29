# Original Lillend body

Project-authored geometry and procedural paint, revised October29,2026. The winged
elven upper body and serpent lower body follow the printed
[Lillend](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lillend).
No third-party mesh, pixels or native animation curves are embedded. The
original draft is retained in the v0.0.169 release. The repair replaces its
visible surfaces with a continuous anatomical sculpt, inset adult face, attached
five-finger hands, shaped auburn locks, a real teal/ivory garment shell with
folded hems and gold seams, and layered curved feather vanes. The existing
`outsider-celestial/sdf_sculpt.py` and `continuous-bodies/continuous_body.py`
authoring tools are reused; no new runtime art system is introduced.
No owner aesthetic approval is claimed.

The exact existing human bind-only observation is the private source input:
SHA256 `eb0d5eaaf4bec8bf68506d42f299ded5d3d86cb676ff72e53f15d0cfbce9bfd2`.
Only its26 reviewed anatomical coordinate frames guide authoring. Original
mesh export contains original vertices/weights/UVs, named drivers, provenance
hash and fixed authored tail points; it contains no captured native geometry,
pixels, bind matrices or animation curves. Private capture and Blender scene
remain machine-local and are not package inputs.

Ten original tail drivers reuse the project's authored Salamander rest/sweep
recipe,1.4seconds with a0.60second action event. Earlier Salamander contact
observations do not establish Lillend contact. Wings follow the existing torso
animation. Ground30-foot movement and the accepted Airborne adaptation remain;
there is no new flight/navigation system. Native sword equipment and native
upper-body animation remain owned by the engine.

Exact exports:

| File | SHA256 |
|---|---|
| `assets/sprint32-support/lillend-mesh.json` | `11ad83a0858d95c72627f63c131a713de26c1e9bc83c3c79a84000b9816a8dd7` |
| `assets/sprint32-support/lillend-albedo.png` | `5148b79042d0e09010269155936c984613c4992b28302999f6caba7779a962d7` |

Mesh23116vertices/45408triangles;36drivers; maximum2influences per vertex.
Paint1024x1024RGB. Native adapter admits only exact Lillend unit, native human
carrier and owned sword; each summon owns its mesh/material/tail action. The
accepted Salamander gait/playback/settlement helpers are reused unchanged;
released Salamander code/assets and DATA's work are not modified.

Authoring uses Blender4.5.10LTS with its bundled NumPy. `paint_lillend.py --output`
accepts an explicit destination; `generate_lillend.py` requires `--capture`,
`--albedo`, `--mesh-data`, `--report`, `--blend-out`. Run through Blender
`--background --factory-startup --python-exit-code 1 --python <script> -- <args>`.
The generator imports the existing sibling `sprint17-serpents` export primitives.
`test_lillend_export.py --capture <private-capture>` checks finite nondegenerate
geometry, normalized legal weights/UVs, closed identity and tail segment lengths.
No external downloads or personal files are needed for authoring.

Three focused export checks pass. The released tail rest centreline, all action
frames and timing remain unchanged; progressively tapered cross-sections open
the visible coil without moving the tail drivers/contact recipe. Offline front,
side, three-quarter and party-camera views are authoring evidence. Native
presentation, exact artifact identities and owner-review status belong to
[the repair record](../../../planning/SATYR-LILLEND-PRESENTATION-REPAIR.md).
Editable authoring instructions are `generate_lillend.py`, `lillend_surfaces.py`
and `paint_lillend.py`; the native-frame-containing `.blend` stays ignored in
`artifacts/repair-packet-0.0.169/lillend-authored.blend`.

## Native Satyr blade mapping

Satyr retains the native archer body, blueprint
`6114adae73a1338498412b3c63c946f8`, prefab
`22dfc39c941704743a5b3fd9769327a0`. Its missing one-handed blade setting is
borrowed at runtime from the native melee Satyr, blueprint
`a6dceccaf84af1a42843e58412265388`, prefab
`36a3ffb15f1331c489ac1ab666aa9d26`. Native128 read-only metadata
SHA256 `65ecf1b59021dc67049a9ebf8207d83c873b6f06acf68cc3edb8a156abd6f7af`
found identical mesh/bind metadata: satyr01,4677vertices,55bone frames, no
startup action or transitions. Runtime guards additionally compare complete
bone paths and bind matrices before mapping.

Each exact summon owns only its cloned animation set, main-hand action and
settings/variant arrays. The native `attackWithSword_01`/`02` clips stay borrowed;
Bow, Gore and every other native action are preserved. No proprietary clips,
geometry or curves are exported, modified or packaged. This is an adapter over
the shipped owned-set mechanism, with no rig retargeting or creature-rule change.
Native playback/contact, sibling isolation and cleanup remain required in the
active record; matching metadata alone is not visual qualification.
