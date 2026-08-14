# Eastern Weapons original asset source

All meshes and icon renders in this directory are original project-owned work
created procedurally for this repository. No official game assets, marketplace
assets, downloaded models, traced artwork, or third-party textures are inputs.

Set `PYTHONHASHSEED=0`, then run `generate_eastern_weapons.py` with Blender
4.5 in background mode. The generator fails closed if that process-level
determinism contract is absent. The
script deterministically creates one `.blend`, three FBX exports, six 512px
transparent icon sources, six 128px production icons under `assets/game/icons`, and
the hash-bearing machine-readable build report. Repository licensing applies.

The 2026-08-14 first-playtest repair uses a measured 42-degree render angle
(tip upper-right, butt lower-left), with render-only camera state created only
after FBX export. The designs are conservative curved, single-edged sword silhouettes derived
only from the mission's text. They use metric scale, primary grip at the
origin, and +Z toward the tip. Source-only cameras and lights never enter FBX
exports or Unity prefabs.
