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

## 2026-08-20 native Longspear frame repair

- Reduced the authored overall length from 2.925 m to 2.28 m, matching the
  installed `TH_LongspearKnight1` 2.2825 m longitudinal extent.
- Recentered the primary grip at the midpoint of the shortened haft.
- Moved the support region to +0.37 m and kept every branch above the hand
  region.
- Preserved metric source geometry along +Z; the Unity prefab builder now
  applies the single explicit -90 degree X coordinate-frame conversion that
  maps source +Z to the native Longspear +Y equipment axis.
- Preserved all project-owned materials, three silhouettes, identities, and
  native `PiercingTwoHanded` presentation fields.
