# Giant Wasp original visual

The geometry, UV atlas, and painting are original project-owned procedural
work authored for Expanded Summoning Sprint 10. No native model or painting is
copied. The measured Giant Eagle rig is a local build input solely for bind
position and animation alignment; its transforms are not committed or shipped.

Build with Blender 4.5.10 LTS. First run `paint_giant_wasp_albedo.py` with
`--out assets/flying-animals/giant-wasp-albedo.png`. Then run
`generate_giant_wasp.py` with `--rig <local measured rig>`, `--albedo` set to
that PNG, `--mesh-data assets/flying-animals/giant-wasp-mesh.json`, and local
`--report`, `--blend-out`, and `--fbx-out` paths. The scripts retain the editable
source. The Blender and FBX products stay in local evidence because they embed
the measured donor skeleton. Runtime loading uses the established schema-2
mesh-data parser, donor bind poses, and an instance-local renderer swap; there
is no Unity editor import or AssetBundle. The target runtime is Unity 2018.4
in Pathfinder: Kingmaker, loaded by KMG 0.0.140 local-development builds.

The current build has a sword-length Tail-weighted stinger, 528 vertices,
376 polygons, and 16 named bone groups,
with at most two influences per vertex. The original painted albedo is
1024 × 1024. The mesh-data SHA-256 is
`07d84d01695540b6ed295efdf905a0058ec7a0f55bf9726387fea721250a81ac`;
the PNG SHA-256 is
`6a40c0e9a08d532cdfc6ff01d508eb4e6d28e4f8eeb75065a4e77678970a40f6`.
The mesh-data manifest pins the PNG hash and dimensions. The project-owned
scripts and shipped mesh/texture are reviewable; owner visual approval remains
pending after technical runtime qualification.

## Visual refinement, 2026-10-17 (VIS-022)

The owner found that the first wasp's major pieces and appendages did not
form a coherent insect, with the abdomen raised high behind it; it also
shipped inside out (17 of 26 parts inward, 22 with caps opposite their walls).
`build_giant_wasp.py` (library in `../continuous-bodies/`) now builds it on
the same Giant Eagle frame as one connected body: a head capsule with large
compound eyes, antennae and mandibles on a short neck, a deep thorax, a narrow
waist, and a banded gaster carried behind and a little below the thorax that
ends in the sting. Everything behind the waist, sting included, rides `Tail`
alone, so `GiantWaspVisualSting`'s bounded strike pose still curls it under.
Six legs leave the underside of the thorax on the eagle's single leg chain
(fore, mid and hind splayed), and two pairs of narrow amber wings ride the arm
chain, shorter than the eagle's spread. `generate_giant_wasp.py` now refuses
to build and is kept for its history.

- `giant-wasp-mesh.json`: `a32d454cff58ec4827d91888a3d9803178ad74c731882a22e1bd2ebf83b205c5`
  (6,502 vertices, 9,404 triangles, 18 closed outward parts, 17 bones).
- `giant-wasp-albedo.png`: `2d26b8dad8436d26e625c0d16761c9ad35bff0f75fd984eed38198b8825a8943`.
- Superseded: the mesh and painting identified above.
