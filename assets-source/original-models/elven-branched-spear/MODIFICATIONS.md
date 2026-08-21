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

## 2026-08-21 semantic-frame presentation calibration

- Added mesh-grounded source markers for grip, support hand, physical tip,
  physical butt, head-face normal, and renderer center to every FBX.
- Added evaluated-vertex validation proving the central leaf is the physical
  +Z end, the butt cap is the physical -Z end, branches remain behind the tip,
  and grip/support markers remain inside the shaft.
- Moved the support station from +0.37 m to the measured native Longspear
  grip-to-left-hand interval, +0.593016 m.
- Replaced unexplained held/back Euler guesses with a full-basis conversion
  from source +Z/+Y to measured native Longspear held and stored frames.
- Kept held grip at the weapon-bone origin and aligned the independent stored
  renderer center to the donor BeltModel anchor.
- Added held-only `EquipmentOffsets.IkTargetLeftHand`; stored prefabs do not
  drive hand IK.
- Regenerated all three FBXs, the Blender source, schema-3 build report, and the
  six-prefab Unity bundle. The project-owned visual silhouettes, icons,
  blueprint identities, donor animations, trails, sounds, slots, timing, and
  mechanics remain unchanged.
