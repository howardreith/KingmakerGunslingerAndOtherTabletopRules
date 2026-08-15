# Elven Branched Spear source

All three meshes and the icon render in this directory are original,
project-owned clean-room works. No Owlcat, Paizo, marketplace, downloaded,
traced, or generative-model asset is an input. The repository license applies.

Run `generate_elven_branched_spear.py` with Blender 4.5.10 LTS and
`PYTHONHASHSEED=0`. The script authors metric geometry at an identity root,
keeps the primary grip at the origin, points the central blade along +Z, and
exports the classic, thorn, and crown variants before adding source-only render
cameras and lights.

The production FBXs and normalized PNGs matched byte-for-byte across two clean
runs on 2026-08-15. Blender's `.blend` container embeds session metadata and is
semantically reproducible but not byte-identical. Exact hashes and branch
coordinates are recorded in `elven-branched-spear-build-report.json`.
