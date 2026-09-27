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

The initial build has 528 vertices, 376 polygons, and 16 named bone groups,
with at most two influences per vertex. The original painted albedo is
1024 × 1024. The mesh-data SHA-256 is
`1ddaf206e006df6dad21335b57270bd7b1d165588a675c52caca44ba4d0d4432`;
the PNG SHA-256 is
`6a40c0e9a08d532cdfc6ff01d508eb4e6d28e4f8eeb75065a4e77678970a40f6`.
The mesh-data manifest pins the PNG hash and dimensions. The project-owned
scripts and shipped mesh/texture are reviewable; owner visual approval remains
pending after technical runtime qualification.
