# Musket geometry-proof qualification

Status: **automated structural PASS; human side-by-side verdict required**.

This is the bounded Phase E2/E3 decision gate. The production Musket remains
bound to `Musket 01.fbx` / prefab `Musket`. The three new prefabs are diagnostic
only and cannot be selected by an item blueprint. No attach slot, animation,
weapon statistic, GUID, ammunition state, or save identity changed.

## Immutable candidate identity

- version: `0.0.80`;
- source commit: `041e934dc85d14512d9353f479d006a39065bd42`;
- package SHA-256:
  `847604574E84BA98BF55E77489FD8CF9276AFE07473B676AF31B72B2AC1D8797`;
- DLL SHA-256:
  `27BCD2F725B369DB4C46F0227AE03D91E4DEC92B941CE5B351EC01E4C2FC0A9D`;
- DLL MVID: `24a38b51-b2f8-4a2e-b42b-edff7e32b1a8`;
- firearm bundle: `kingmakergunslinger.firearms`;
- bundle size: 17,960,137 bytes;
- bundle SHA-256:
  `BD78F647966271D826C16D5FD93BD481EA1953E48CE66D9E9313ABBFED15B152`;
- exact Unity builder: 2018.4.10f1, Windows x64;
- source report:
  `assets-source/original-models/firearm-fit-experiments/musket-fit-candidates-build-report.json`.

The installed hash matched the packaged DLL. Exact runtime evidence IDs are in
`docs/WEAPON-VISUAL-VARIETY-FIREARM-FIT-IMPLEMENTATION-REPORT.md`.

## Candidate set

| Candidate | Purpose | FBX SHA-256 | Production-bound |
|---|---|---|---|
| `MusketPassThrough` | Existing Mesh Masters geometry re-exported through Blender; pipeline control | `16E918E4E6F32CF053C9C875951496BA40DDDEAF51D63C6D6008C7D272B17755` | no |
| `MusketMinimalControl` | Barrel, muzzle, lock, real fore-end, furniture, ramrod, and almost no rear stock; geometry-causation control | `33C4D7F097F745F501F79F4DAED3E42E366E6D4F10974BB6A385F3F1E86E6B01` | no |
| `MusketClearanceStock` | Complete narrow, dropped, segmented stock designed around the fixed pose | `069B5D1DFC912D781065FFC3CF84A786B6C381C9CFEBD4779580BD89B6DBE006` | no |

All three resolve at runtime to the same qualified frame:

- grip: `(0,0,0)`;
- support: `(-0.030976,-0.051069,0.586040)`;
- butt: `(0,0,-0.169533)`;
- muzzle: `(0,0,1.180452)`;
- semantic length: `1.349985 m`;
- animation: inherited `Crossbow`;
- native attachment slots: unchanged.

The Unity importer accepts source-authored markers only when exactly one each of
`KMG_Grip`, `KMG_Support`, `KMG_Butt`, and `KMG_Muzzle` exists. A partial or
duplicate set, non-finite point, backward/off-axis muzzle, implausible length,
out-of-envelope support, non-positive scale, or empty rendered hierarchy fails
the build. Existing markerless production sources retain their exact hardcoded
legacy fallback.

## Source-space evidence

The following committed renders are geometry/provenance aids, not proof of
Kingmaker fit:

- `assets-source/original-models/firearm-fit-experiments/renders/musket-pass-through-source.png`;
- `assets-source/original-models/firearm-fit-experiments/renders/musket-minimal-control-source.png`;
- `assets-source/original-models/firearm-fit-experiments/renders/musket-clearance-stock-source.png`.

In-game before/after captures belong under the repository-ignored local path
`evidence/screenshots/weapon-fit/0.0.80/`. Their absence is the explicit human
gate, not an automated PASS. Use these exact names:

- `musket-pass-through-inventory-{front,left,right,side}.png`;
- `musket-minimal-inventory-{front,left,right,side}.png`;
- `musket-clearance-inventory-{front,left,right,side}.png`;
- corresponding `world-idle`, `combat-idle`, `attack`, `recovery`, and
  `switching` captures.

## Human comparison procedure

Use only `KMG_AUTOMATION_WORKING`; never overwrite
`KMG_AUTOMATION_BASELINE`. Launch through Steam App ID 640820. A human must load
the working save through the normal UI. Do not use automated UI navigation,
screenshots, OCR, or coordinates as mechanical evidence.

1. Verify the immutable commit/version/package/bundle identities from the final
   report and select exactly one unit with an equipped Musket.
2. Open the development firearm calibration lab. Capture production/baseline
   first, then use `Show pass-through Musket`, `Show minimal-control Musket`, and
   `Show clearance-stock Musket` in that order.
3. After each change, close and reopen inventory for a clean doll rebuild. The
   tool refreshes world `HandsEquipment`; it deliberately does not claim an
   automatic inventory-doll refresh.
4. Use identical character, ancestry, sex, body type, armor, camera, zoom,
   lighting, and weapon set for the three candidates.
5. Inspect male Human, female Human, a shorter ancestry, and a broad-bodied
   ancestry in light clothing, light armor, and bulky torso armor. Record doll
   front/left/right/side, world peaceful/combat idle, attack, firing, recovery,
   switching, unequip/reequip, save/load, and restart.
6. Finish with `Restore production Musket`; verify the ordinary Musket model is
   restored before saving or ending the session.

For every row record primary grip, support contact, floating/embedded hand,
stock/receiver/butt penetration, muzzle direction, inversion, visibility,
fallback, switching, and inventory/world/save consistency.

## Decision rule

- If Minimal clears the torso while PassThrough clips, geometry is a meaningful
  lever. Compare ClearanceStock against Minimal and select/revise a complete
  stock inside the provisional envelope below.
- If Minimal clips substantially like PassThrough, pose/animation is dominant.
  Stop mesh polishing and request explicit approval before any animation work.
- If idle improves but attack/recovery becomes unacceptable, animation is the
  remaining limitation.
- Reject any candidate that floats/embeds either hand, reverses/disappears,
  worsens ordinary world idle or attack, or trades torso clipping for a larger
  arm/hand defect.

No final Musket is selected until this comparison is recorded. Blunderbuss E5
must not start until the Musket result identifies the winning diagnostic method.

## Provisional clearance envelope

This envelope describes the candidate to review; it is not called final until a
human selects the geometry:

- fixed grip origin `(0,0,0)` and +Z muzzle axis;
- fixed support target and physical fore-end surface at z `0.586040`;
- muzzle z `1.180452`, butt z `-0.169533`;
- receiver/lock region: z `-0.02..0.19`, X half-width at most `0.039 m`,
  Y half-depth at most `0.026 m`;
- stock torso-facing thickness at most `0.032 m`;
- dropped-stock X envelope `0..0.061 m` behind the grip;
- fore-end bounding width/depth `0.068 x 0.104 m`, preserving support contact;
- no geometry outside the fixed grip/support hand-clearance regions;
- total visible candidate bounds approximately
  `0.100 x 0.104 x 1.360 m` for ClearanceStock.

Human result: **PENDING**.

Blunderbuss result: **NOT STARTED BY DESIGN; gated on the Musket conclusion**.
