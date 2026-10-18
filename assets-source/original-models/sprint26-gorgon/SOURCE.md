# Sprint 26 original Gorgon body

Original modest armored-bull geometry, UVs and matte iron albedo on the native
Horse animation frame. It reuses the project's own procedural cattle primitives
and adds closed overlapping torso, skull and limb plates, two horns, four cloven
feet and living amber eyes. No native vertices, triangles, UVs, textures,
materials, animation curves or proprietary assemblies are redistributed.

The only native inputs are the geometry-free Horse frame captured by the
bounded Sprint 26 input observer, RunId
20261018T0439106684862Z-9ddbdd0aa753493e8171dd4337e8ac49. Its exact unit/prefab
are validated by the generator. Editable Blender/rig inputs remain ignored and
machine-local. The shipped schema-2 file contains original vertices, normals,
UVs, triangles and weights plus native bone names; runtime uses the actual
renderer bind poses through the established attachment transaction.

Generate with installed Blender 4.5 LTS:

```text
blender -b --factory-startup --python-exit-code 1 --python build_gorgon.py -- --capture <private-input.json> --output-dir <assets/ungulates> --work-dir <private-authoring-dir>
```

This is a functional candidate, not polished or owner-approved art. Faceting,
overlap seams, proportions, secondary armor motion and contact choreography
remain a short refinement backlog after actual visibility/command checks.
HumanReview: NOT_PERFORMED_NONBLOCKING. Native visual/lifecycle qualification is
required separately. Bulette reuses the actual installed adult Bulette body;
it does not inherit this skin or the Horse view.

Native prototype review found mixed closed-surface winding, then inward lighting
normals after winding normalization. The Gorgon-only exporter now normalizes
faces to its observed native-visible plate orientation and emits outward normals.
The actual prior frames and verdicts remain machine-local. This is a functional
legibility correction, pending fresh game inspection, without owner art approval.
