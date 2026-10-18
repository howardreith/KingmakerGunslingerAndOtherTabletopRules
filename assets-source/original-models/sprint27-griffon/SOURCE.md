# Original Griffon body

Project-owned geometry, UVs and deterministic matte albedo, authored by
build_griffon.py using established project primitives and schema-2 exporter.
Blender4.5.10 LTS; one2,930-vertex/5,560-triangle feasibility prototype.
Native capture SHA2563a5707417402fec2bddb6c1c4e55ff27f53f7e778c7bb8d2e314bb94533022ad
proved one manticoreMesh/57-bone Manticore renderer, native blueprint
da807351572c540478b703957d863e68, prefab e667b3d46fad6ac44bf36cdd5ab33af8.
No exact native Griffon exists in the captured installed library.

Only original geometry/paint, weights and structural bone names ship. Capture,
bind transforms, preview armature and .blend stay local. No native geometry,
UVs, textures, animation curves or proprietary art is redistributed. Runtime
uses the exact instance binding and native animation/fader/material hooks.

The exporter locally restores outward winding after the legacy serializer's
reversal. Measured-frame positions/normals are unchanged. Real shipped-mesh
renders were inspected with backface culling. Read-only visual review found
recognizable eagle head/beak, wings, foretalons and feline body/paws/tail with
no static functional blocker. HumanReview: NOT_PERFORMED_NONBLOCKING.
Narrow angular wings, coarse feathers, shoulder seams, hind-leg/paw simplification
and pale/tawny transitions remain debt. Native functionality remains unqualified.

Regenerate with Blender --background --factory-startup --python build_griffon.py
and explicit --capture <private sprint27-native-inputs.json>, --output-dir <assets>,
--work-dir <local-only work>. Native transforms must remain local.
