# Firearm Pistol variant design record

- `Pistol.Duelist` uses a slim octagonal barrel, flared muzzle, swept grip,
  rounded pommel, and narrow swept guard for a visibly lighter dueling profile.
- `Pistol.LastWord` uses a heavier barrel, crowned muzzle, angular receiver and
  grip, coffin pommel, pronounced guard hook, and dorsal sight ridge.
- Both preserve the one-handed family envelope, identity firing grip, +Z muzzle
  axis, `PiercingOneHanded` animation contract, and 0.264 m muzzle point.
- Both author exactly one `KMG_Grip`, `KMG_Support`, `KMG_Butt`, and
  `KMG_Muzzle` marker. The support marker is validation metadata only; no
  support-hand IK target is created for a one-handed firearm.
- No runtime randomness, item state, enhancement tier, or character identity is
  used to select a model.
