# Original sculpted Ettin second head

`build_ettin_head.py` authors a closed head/shoulder-root neck surface with
integrated jaw, nose/cheeks, brow ridges and small ears, plus a receding
hairline, uneven tapered nape locks, carved eye/mouth recesses, lips and teeth. It also
paints one original four-region atlas procedurally. No native geometry, pixel
data, material files or animation curves are authoring inputs. The current
refinement responds to rejected native body-6 images and the actual Hill facial
anchor directions: Head-local **-Z is forward, +Y is up**. The previous +Z
sculpt faced backward. Changed native fit/appearance remains pending.

```text
blender -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint29-ettin/build_ettin_head.py -- --output <absolute src/KingmakerGunslinger/Summoning/EttinAuthoredHeadMesh.cs> --work-dir <absolute private output>
```

Two clean Blender 4.5.10 LTS runs produced identical generated C# bytes.
The packed mesh is original project geometry with positions,
normals, the neck-to-skull weight field, original UVs and four material index
groups. It has 5,024 vertices and 9,992 triangles; packed geometry SHA-256 is
`747aec079b36e424b13af5e4d4cf002a9810cda13da5a2e81b5d3e2175183eb8`.
The packed 512x512 original RGBA PNG SHA-256 is
`4d6d02da2b349dc35082df3d8296d8ae80b00f25ac36022632d3eecc561dcc60`.
Generated C# is a normal production source input, not a machine-local build
package. Source `.blend` files and preview images stay local.

`EttinSecondHeadVisual` creates one per-instance skinned mesh with four owned
materials using the exactly identified native Hill PF shader state with all
surface maps replaced/cleared. The verified parameter channels receive original
matte/nonemissive/nonmetal values. No native pixels are sampled into the paint.
The neck root follows the actual native Head parent; the skull follows
an owned sibling control whose rotation copies the native head while its lateral
position stays fixed relative to the neck, with the revised Head-local offset
`(.44,-.035,-.02)`. This replaces parenting an offset
primitive puppet under Head. Reattachment after explicit release is supported.
The owned mesh, original atlas/parameter textures and proven renderer-created
material instances are included in resource capture/destruction, alongside the
root that owns the skull/driver objects. Four material slots, two bone drivers,
mesh naming and stable blueprint identities are retained.

The optional preview controls are **authored probes**, not native animation.
The exact packed bytes were also previewed with the runtime two-bone bind
formula in the complete captured shared-Hill-donor posed TRS, plus synthetic
30-degree turn and 20-degree pitch. Actual native6 Neck recovery is a separate
scalar check: its missing Head quaternion was not invented. Changed native
neck burial, facing, proportions, PF lighting, motion and lifecycle remain
unverified. The prior native6 lifecycle passed resource isolation/release but
failed two missing off-hand pose-handle assertions; those results concern old
bytes. Owner visual acceptance and cold reconstruction remain pending.

See [current forward repair notes](../../../planning/CLEANUP2-ETTIN-FORWARD-REPAIR-NOTES.md)
for exact native failure/readback identity, shader metadata receipt, shared-donor
frame limits, commands and offline checks. The preceding material-refinement
notes remain an old-byte checkpoint. No source `.blend`, raw native data,
generated package or machine-local evidence is committed.
