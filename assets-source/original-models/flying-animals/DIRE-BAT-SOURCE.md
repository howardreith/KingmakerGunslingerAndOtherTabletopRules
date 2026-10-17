# Dire Bat original visual

Original project-owned geometry, weights, UVs and painting on the measured
Giant Eagle bind frame. Only bone names and the bind frame come from the game;
the rig file (`../pteranodon/convert_bind_rig.py` of the guarded Pteranodon
capture, SHA-256
`38fc5f00831a831f9a3885609a8db0d608776ebc9d66af51ca5a10dab8ec75f2`), the
Blender source and the build report stay machine-local.

## Visual refinement, 2026-10-17 (VIS-013)

The owner found the first Dire Bat (`generate_dire_bat.py`) hard to recognise,
with detached-looking claws and toes and very blocky, overlapping body and
wing pieces; it also shipped inside out (26 of 33 parts inward, 29 with caps
opposite their walls, 4 open). `build_dire_bat.py` (library in
`../continuous-bodies/`) now builds it as one continuous body: a compact,
deep, furred body with a thick ruff carrying the head, a rounded skull with a
short blunt snout, a nose leaf, fangs and a lower jaw on `Jaw`, broad pointed
ears, thin legs with short clawed toes, a tail stub, and two membrane wings.
Each wing is a closed thin shell (`continuous_body.sheet`) over a grid from the
arm and first finger to a trailing edge that runs from the ankle through the
finger tips, scalloped inward between them, with four long fingers from the
wrist and a thumb claw. The membrane keeps the first bat's weighting
(`membrane_grid`), so the eagle's wing animation folds it, and every one of
the 46 audited bones carries geometry. `generate_dire_bat.py` now refuses to
build and is kept for its history.

- `dire-bat-mesh.json`: `b04e9dd146b1c9db87de7907f7b6affb4f7910ff8fcfe4cd65288217f17b7cce`
  (9,115 vertices, 12,596 triangles, 40 closed outward parts, 46 bones).
- `dire-bat-albedo.png`: `ffb7ed2bcf2a7d2c5a48e7305b75da2d81d9ee6fb9a946ea90ae9caee76b9a4b`.
- Superseded: mesh `3c488cd4838812c12946410b9bba3283917f9b005904d4b237fedaac508f0819`,
  albedo `c84011da7d1ca446191632ed1330892ed320edf253f1ab6dcdb17e76df21e335`.
