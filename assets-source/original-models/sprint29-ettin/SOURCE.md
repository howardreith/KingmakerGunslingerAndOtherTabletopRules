# Original sculpted Ettin second head

`build_ettin_head.py` authors a closed head/shoulder-root neck surface with
integrated jaw, shallow nose/cheeks, brow ridges and small ears, plus a receding
hairline, tapered nape locks, recessed narrow eyes and a mouth crease. It also
paints one original four-region atlas procedurally. No native geometry, pixel
data, material files or animation curves are authoring inputs. The current
refinement responds to actual rejected native body-5 images and a scalar
shared-Hill-donor frame check; changed native fit/appearance remains pending.

```text
blender -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint29-ettin/build_ettin_head.py -- --output <absolute src/KingmakerGunslinger/Summoning/EttinAuthoredHeadMesh.cs> --work-dir <absolute private output>
```

Two clean Blender 4.5.10 LTS runs produced identical generated C# bytes.
The packed mesh is original project geometry with positions,
normals, the neck-to-skull weight field, original UVs and four material index
groups. It has 2,840 vertices and 5,636 triangles; packed geometry SHA-256 is
`85c6e7ce05a9db979f98027f7fe4c8f89f318a81851fb93bae0f8ca134f4e025`.
The packed 512x512 original RGBA PNG SHA-256 is
`736b53f8365befd0ded146316323a21aafe6c01ccefdb628b05ee7d751f6f34c`.
Generated C# is a normal production source input, not a machine-local build
package. Source `.blend` files and preview images stay local.

`EttinSecondHeadVisual` creates one per-instance skinned mesh with four owned
materials using the exactly identified native Hill PF shader state with all
surface maps replaced/cleared. The verified parameter channels receive original
matte/nonemissive/nonmetal values. No native pixels are sampled into the paint.
The neck root follows the actual native Head parent; the skull follows
an owned sibling control whose rotation copies the native head while its lateral
position stays fixed relative to the neck. This replaces parenting an offset
primitive puppet under Head. Reattachment after explicit release is supported.
The owned mesh, original atlas/parameter textures and proven renderer-created
material instances are included in resource capture/destruction, alongside the
root that owns the skull/driver objects. Four material slots, two bone drivers,
mesh naming and stable blueprint identities are retained.

The optional preview controls and `authoring-poses.json` are **authored probes**,
not a measured native rig or native animation. Inspected turn/pitch/reaction
previews retain the neck connection. Actual changed native fit, idle/travel/turn/attack
and hit motion, simultaneous instances, teardown and cold reconstruction remain
unverified. Owner visual acceptance remains pending.

See [current refinement notes](../../../planning/CLEANUP2-ETTIN-MATERIAL-REFINEMENT-NOTES.md)
for exact native failure/readback identity, shader metadata receipt, shared-donor
frame limits, commands and offline checks. No source `.blend`, raw native data,
generated package or machine-local evidence is committed.
