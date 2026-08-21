# Modifications

- Identify physical source `+X` as butt-to-muzzle and source `+Z` as the
  stock/receiver up axis from the actual meshes, then normalize imported units
  to metres and convert that basis to canonical `+Z` forward / `+Y` up.
- Bake the conversion into copied mesh data and place the measured
  trigger-wrist firing-hand grip at the derivative origin.
- Derive butt and muzzle from renderer-bound endpoint vertices rather than a
  model origin or total-bounds guess.
- Add exact Grip, Support, Butt, Muzzle, Back, WeaponForward, and WeaponUp
  semantic markers. The Musket and Rifle support stations use the measured
  native Heavy Crossbow `0.374 m` forward coordinate; Blunderbuss uses its
  mesh-appropriate `0.360 m` fore-end station.
- Apply a muted period-compatible material tint while retaining licensed source
  geometry and UVs.

The deterministic 2026-08-21 outputs are:

- Musket: `C5E2EA93E903782BF3110E50C1D6677C4E7C109248651495192D8B6063F73A0A`.
- Blunderbuss: `45DD00FD88D7CE1B66690E1A1B6FFE732A343F3C728D84B4FF8956F1F4F4197C`.
- Rifle: `9D9288D04DEED70A6CA7AA321A2107B0F482431A082A1E2EDF4B50CB14742072`.

Two clean Blender processes reproduced those exact hashes. Unity applies the
measured native Heavy Crossbow held basis at the visible-model/semantic-marker
layer while leaving each equipment root at identity, and independently solves
the stored BackMount frame. Guarded default-Medium-male runtime evidence accepts
held idle, combat ready, acted firing, and stored presentation; reload,
locomotion, and the remaining body matrix stay explicitly open.
