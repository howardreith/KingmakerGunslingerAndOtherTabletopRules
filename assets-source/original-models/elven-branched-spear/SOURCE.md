# Elven Branched Spear source

All three meshes and the icon render in this directory are original,
project-owned clean-room works. No Owlcat, Paizo, marketplace, downloaded,
traced, or generative-model asset is an input. The repository license applies.

Run `generate_elven_branched_spear.py` with Blender 4.5.10 LTS and
`PYTHONHASHSEED=0`. The script authors metric geometry at an identity root,
keeps the primary grip at the origin, identifies the physical central blade
tip as source +Z, and identifies the thin head-face normal as source +Y. It
exports the classic, thorn, and crown variants before adding source-only render
cameras and lights.

Each FBX carries the source-authored `KMG_Grip`, `KMG_Support`, `KMG_Tip`,
`KMG_Butt`, `KMG_HeadUp`, and `KMG_Back` markers. The generator validates the
markers against evaluated mesh vertices: the central leaf owns the forward
extreme, the butt cap owns the rear extreme, every branch remains behind the
physical tip, grip and support lie inside the shaft, and every mesh scale is
positive identity. The semantic tip and butt are at +/-1.14 m and the measured
support station is +0.593016 m from the grip. Evaluated mesh span is about
2.278557 m because bevelled geometry stops just inside the semantic ends.

The Unity builder derives held and stored transforms independently. It maps
the authored +Z/+Y basis to the measured native Longspear held or BeltModel
basis, then solves translation so the held grip reaches the weapon-bone origin
or the stored renderer center reaches the native stored anchor. The equipment
root remains identity; no fixed legacy Euler correction is source authority.

The current production FBXs and normalized PNGs matched byte-for-byte across
two clean runs on 2026-08-21. Blender's `.blend` container embeds session
metadata and is semantically reproducible but not byte-identical. Exact hashes,
mesh-grounded measurements, markers, and branch coordinates are recorded in
`elven-branched-spear-build-report.json` schema 3.

## Icon-overhaul render path

`tools/icon-art/render_weapon_icon_sources.py` is a render-only consumer of
the classic production FBX. With `PYTHONHASHSEED=0` and Blender 4.5.10 LTS, it
computes the mesh principal axis, places the spear lower-left to upper-right at
42 degrees, fits the complete branching head and pole to a transparent 512 px
canvas, and uses flat material-accurate workbench lighting. It does not alter
the FBX or `.blend` source. Exact hashes and fit measurements are written to
`assets-source/original-icons/icon-overhaul-weapon-render-report.json`.

`tools/icon-art/New-IconOverhaulAssets.ps1 -Mode Items` performs the single
high-quality downsample to the 128 px runtime icon with a 5 px safety margin.
