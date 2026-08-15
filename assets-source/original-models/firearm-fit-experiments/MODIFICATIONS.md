# Musket fit-experiment modifications

## 2026-08-15 geometry proof

- Pass-through: imported and re-exported the existing licensed Musket geometry
  without intentional mesh edits, then added the four semantic marker empties.
- Minimal control: retained a firing-hand grip, barrel, muzzle, lock reference,
  physical support-hand fore-end, furniture, and ramrod while reducing the rear
  stock to a short diagnostic stub.
- Clearance stock: retained the same fixed grip, support, butt, and muzzle frame,
  narrowed torso-facing stock/receiver depth, and built a dropped segmented
  stock around that pose.
- Added exactly one each of `KMG_Grip`, `KMG_Support`, `KMG_Butt`, and
  `KMG_Muzzle`. No attach-slot, animation, weapon mechanic, item identity, weapon
  type identity, or production item-to-prefab mapping changed.

These are deliberately unpolished diagnostic candidates. Human comparison, not
the source render, decides whether geometry materially improves live fit.

Two clean Blender 4.5.10 LTS runs produced byte-identical FBXs and normalized
PNGs. The `.blend` file is semantically reproducible but embeds Blender session
metadata and is not falsely claimed byte-identical. Two unchanged-input Unity
2018.4.10f1 builds produced the same 17,960,137-byte bundle at SHA-256
`BD78F647966271D826C16D5FD93BD481EA1953E48CE66D9E9313ABBFED15B152`.
