# Elven Branched Spear modifications

## 2026-08-15 cleanup mission

- Replaced the subtle three-leaf head with unmistakable, physically separated,
  backward-swept prongs around a central leaf blade.
- Added a bounded three-model vocabulary: two-prong classic, three-prong thorn,
  and four-prong crown.
- Preserved the grip origin, +Z axis, -0.915 m butt, +2.01 m point, support-hand
  region, scale, handedness, animation donor, category, and all blueprint GUIDs.
- Added stable SHA-256-derived FBX object IDs and removed the unused,
  nondeterministically packed UV channel.
- Disabled workbench render antialiasing and stripped session-only PNG metadata;
  rendered pixels are unchanged by normalization.
- Rebuilt the exact Unity 2018.4.10f1 bundle with three prefabs and centralized
  deterministic per-item assignment.

No manual binary editing was performed. Regenerate all source artifacts from
the checked-in script.
