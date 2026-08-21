# Eastern Weapons original asset source

All 12 meshes and six icon renders in this directory are original,
project-owned clean-room works. No official game asset, marketplace asset,
downloaded model, traced artwork, third-party texture, or generative model is
an input. The repository license applies.

Set `PYTHONHASHSEED=0`, then run `generate_eastern_weapons.py` with Blender
4.5.10 LTS. It creates four bounded variants each for Wakizashi, Katana, and
Nodachi, plus the existing six family/capstone icons. Every source is metric,
has an identity object transform, and uses the same physical frame:

- `KMG_Grip` is the intended dominant-hand station.
- `KMG_Tip` and `KMG_Butt` are verified against the evaluated renderer ends.
- grip-to-tip is source `+Z`.
- `KMG_BladeNormal` is source `+Y` and is orthogonal to forward.
- `KMG_Edge` is source `-X` and identifies the cutting-edge side.
- `KMG_Stored` records an independent source anchor.
- Nodachi also exposes `KMG_Support` on the butt/pommel side of the grip.

The schema-3 build report rejects missing, nonfinite, collinear, reflected,
reversed, or renderer-disconnected semantics and records exact hashes and
geometry parameters. Render-only cameras and lights are added after all
production FBX exports. Blender's `.blend` container embeds session metadata
and is semantically, not byte-for-byte, reproducible.

The calibrated FBXs used by bundle SHA-256
`AE311993F683295D3DD996285D28385A20F593DF16903D909818EB4F25A0096B`
are recorded in `eastern-weapons-build-report.json` and
`assets/bundles/asset-bundle-manifest.json`.
