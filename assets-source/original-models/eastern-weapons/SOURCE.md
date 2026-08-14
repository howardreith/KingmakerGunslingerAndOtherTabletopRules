# Eastern Weapons original asset source

All meshes and icon renders in this directory are original project-owned work
created procedurally for this repository. No official game assets, marketplace
assets, downloaded models, traced artwork, or third-party textures are inputs.

Run `generate_eastern_weapons.py` with Blender 4.5 in background mode. The
script deterministically creates one `.blend`, three FBX exports, six 512px
transparent icon sources, six 128px production icons under `assets/game/icons`, and
the hash-bearing machine-readable build report. Repository licensing applies.

The designs are conservative curved, single-edged sword silhouettes derived
only from the mission's text. They use metric scale, primary grip at the
origin, and +Z toward the tip. Source-only cameras and lights never enter FBX
exports or Unity prefabs.
