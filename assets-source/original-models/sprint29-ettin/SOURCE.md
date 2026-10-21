# Original sculpted Ettin second head

`build_ettin_head.py` authors a closed head/collar surface with integrated jaw,
nose, cheeks, brow ridges and ears, plus swept hair, recessed eyes and a mouth
crease. No native geometry, transforms, materials, textures or animation curves
are inputs. The shape follows the original attachment's reference units; native
fit/calibration has not been observed.

```text
blender -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint29-ettin/build_ettin_head.py -- --output <absolute src/KingmakerGunslinger/Summoning/EttinAuthoredHeadMesh.cs> --work-dir <absolute private output>
```

Blender 4.5.10 LTS with `PYTHONHASHSEED=0` produces identical generated C# bytes
in two clean runs. The packed mesh is original project geometry with positions,
normals, the collar-to-skull weight field and four material index groups. It has
2,011 vertices and 3,986 triangles; the packed geometry SHA-256 is
`d93d7fcef34bb1d5127439380be62fdd002d5705631aa44f9c84730604a5f462`.
Generated C# is a normal production source input, not a machine-local build
package. Source `.blend` files and preview images stay local.

`EttinSecondHeadVisual` creates one per-instance skinned mesh with four owned
materials. The collar follows the actual native Head parent; the skull follows
an owned sibling control whose rotation copies the native head while its lateral
position stays fixed relative to the neck. This replaces parenting an offset
primitive puppet under Head. Reattachment after explicit release is supported.
The owned mesh and renderer-created material instances are included in resource
capture/destruction, alongside the root that owns the skull/driver objects.

The optional preview controls and `authoring-poses.json` are **authored probes**,
not a measured native rig or native animation. Inspected turn/pitch/reaction
previews retain the collar connection. Actual native fit, idle/travel/turn/attack
and hit motion, simultaneous instances, teardown and cold reconstruction remain
unverified. Owner visual acceptance remains pending.
