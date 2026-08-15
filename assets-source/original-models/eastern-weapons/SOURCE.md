# Eastern Weapons original asset source

All 12 meshes and six icon renders in this directory are original,
project-owned clean-room works. No official game asset, marketplace asset,
downloaded model, traced artwork, third-party texture, or generative model is
an input. The repository license applies.

Set `PYTHONHASHSEED=0`, then run `generate_eastern_weapons.py` with Blender
4.5.10 LTS. It creates four bounded variants each for Wakizashi, Katana, and
Nodachi, plus the existing six family/capstone icons. Every source is metric,
has an identity root and primary grip at the origin, and points its blade along
+Z. Render-only cameras and lights are added after every production FBX export.

Two clean runs on 2026-08-15 produced byte-identical 12 FBXs and 12 normalized
PNGs. Blender's `.blend` container embeds session metadata and is semantically,
not byte-for-byte, reproducible. Exact hashes and geometry parameters are in
`eastern-weapons-build-report.json`.
