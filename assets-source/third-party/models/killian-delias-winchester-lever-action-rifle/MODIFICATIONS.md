# Modification record

Original source files are preserved unchanged.

Record every derivative operation below before release. At minimum document:

- import tool and version;
- Unity editor version;
- coordinate-system conversion;
- scale;
- rotation/orientation;
- pivot/origin changes;
- hand-socket attachment;
- material/shader conversion;
- texture conversion or compression;
- mesh optimization, decimation, or topology changes;
- collider changes;
- animation or rig changes;
- AssetBundle name and hash;
- processed output hashes;
- assignment in game.

Intended game mapping: **Advanced Rifle only**.

Do not use this lever-action model as the early Musket.

## Kingmaker derivative

- Import/build tool: Unity 2018.4.10f1, Windows 64-bit target.
- Original FBX and five textures: preserved byte-for-byte; no topology,
  decimation, collider, animation, or rig changes.
- Prefab: wrapped as `Rifle`, normalized to a 1.55-unit target length, centered,
  rotated onto the shared firearm forward axis, and given a muzzle marker.
- Material: Unity Standard shader with the preserved BaseColor texture;
  conservative runtime texture import/compression defaults.
- Attachment: mapped only to `FirearmKind.Rifle` / Advanced Rifle and attached
  by the shared right-hand firearm visual handler.
- Bundle: deterministic Windows bundle `kingmakergunslinger.firearms`; final
  bundle SHA-256
  `D902F279D8E745BC7852ABDEF6F7C03B97128C92F38641101D5DFC140E39FBFD`,
  reproduced by two consecutive builds. Packaged output hashes are recorded in
  the release report.

## 2026-08-07 held visibility correction

Exact hierarchy/bounds evidence showed the `1.57617009 m` rifle centered at rig
z `-0.651`, placing almost all geometry behind the grip. Visual z is corrected
to `0.20`; opaque Standard materials and generated reverse-wound backfaces are
used. Grip root, support target, muzzle, animation, and original source are unchanged.

The semantic-anchor pass declares source Grip `(0.13,0,0)`, Support
`(-0.1946,-0.0331,-0.0201)`, Butt `(0.503,0,0)`, and Muzzle `(-0.503,0,0)`,
yielding `1.549379 m` butt-to-muzzle length in the grip frame.

## 2026-08-21 canonical Advanced Rifle derivative

The earlier wrapper calibration above is superseded for production by
`../firearm-long-gun-derivatives/rifle-normalized.fbx`. Blender 4.5 derives it
deterministically from the preserved, cleared Advanced Rifle FBX with SHA-256
`74D60FCC6D9A6E89C20EBB8C2D35471417E9F6C23A840FA65967C20251540A1D`.
The actual source frame is `+X` butt-to-muzzle / `+Z` receiver-up; the grip is
the stock wrist behind `gachette` and inside the rear `levier` span. The
renderer-bound endpoints, full WeaponForward/WeaponUp frame, and native Heavy
Crossbow `0.374 m` support station are recorded in the generation report.
Derivative SHA-256 is
`9D9288D04DEED70A6CA7AA321A2107B0F482431A082A1E2EDF4B50CB14742072`.
No original FBX, texture, topology, UV, animation, or rig was overwritten.
