# Original Griffon body

## Cleanup iteration 2 (2026-10-21)

`refine_griffon.py` supersedes the feasibility generator for the current export.
It reads `cleanup2-griffon-input.json`, the exact original project .163 geometry
export, and the shared continuous-body library. No native bind transforms,
geometry, textures or animation curves are supplied or reconstructed.

The baseline's 75 components were already outward/closed. Exact-byte renders
instead showed needle-like feather rows, exposed primitive shoulder joins,
long thin foretalons and a straight rod tail with a ball tip. The current mesh
unions the lion body/eagle neck/forelimb skin, broadens the scapular bridges under
closed feather coverts, replaces needles with overlapping tapered vanes over
closed fan webs, shortens/root-connects the foretalons and authors a curved
muscular tail with a tapered swept tuft. Original native bone names/weights
transfer over closest triangles and are smoothed; no animation/mechanics change.
A new painted 1024px albedo blends the eagle/lion regions over the new UVs.

```text
blender -b --factory-startup --python-exit-code 1 -P assets-source/original-models/sprint27-griffon/refine_griffon.py -- --output-dir <absolute assets/sprint27-griffon> --work-dir <absolute private output>
```

Use Blender 4.5.10 LTS and `PYTHONHASHSEED=0`. Two clean runs match byte for byte.
Exact-byte culling renders were inspected front/side/rear/party/below; these are
synthetic stills. Native travel, turns, attacks, and actual implemented transition
states still require review. Owner acceptance is pending. Exact asset identities
and known limits are in `planning/CLEANUP2-LARGE-CREATURES-NOTES.md`.

## Retained Sprint 27 baseline record

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
