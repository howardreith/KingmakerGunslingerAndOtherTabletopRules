# Original Lillend body

Project-authored geometry and procedural paint, October25,2026. The winged
elven upper body and serpent lower body follow the printed
[Lillend](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lillend).
No third-party mesh, pixels or native animation curves are embedded. This is
functional-first art; facial detail, robe construction and feather polish remain
cosmetic review debt. No owner aesthetic approval is claimed.

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
| `assets/sprint32-support/lillend-mesh.json` | `2734cb0dc2de27e1eb7c6884d8eab598260bed513f170e4573f83a3d3230afec` |
| `assets/sprint32-support/lillend-albedo.png` | `561609b114d83e8d4ab4c48d66dab920f115c6f2a80eaf8d8d0302f1c8a3b5c4` |

Mesh3932vertices/7664triangles;36drivers; maximum2influences per vertex.
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

Three export checks passed. Final tracked source reproduced both exports
byte-for-byte. Offline party-camera/three-quarter rest views were inspected;
these do not prove native loading, selection, movement, contact or lifecycle.
Native body qualification remains pending in the [active record](../../../planning/SPRINT32-STATUS.md).
Local reproducibility/check receipts: `artifacts/sprint32-body-draft/`.
