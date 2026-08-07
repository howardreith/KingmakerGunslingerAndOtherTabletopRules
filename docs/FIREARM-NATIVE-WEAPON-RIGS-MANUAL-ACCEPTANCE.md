# Firearm native weapon rigs manual acceptance

Human observation record: **Regular Pistol held appearance accepted on
2026-08-07.** This narrow verdict covers the observed held model/pose only; it
does not accept unobserved Pistol lifecycle/body/holster states and does not set
the global readiness to `HumanAccepted`. Its Cyril43 source, equipped transform,
scale, and `PiercingOneHanded` animation are frozen for this pass.

Candidate: version `0.0.71`, semantic-anchor implementation commit
`25a585f79a7c0af232c55636aaaaa77d78a4fdee`. Package
`artifacts/local-runtime/0.0.71/KingmakerGunslinger-0.0.71-local-runtime.zip`,
SHA-256 `6858AF28C2DDE865BD2575FDEECF6DA11ADACEB0BC6210B1251DEC54239DBC06`;
DLL `2757835E9086B35481D9F5E06B03DC691BB317B351794BE1B0EDC20442568EA4`;
AssetBundle `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`;
rig manifest `35BB38BF142D1F1DB3439F4EC328CE7EBF2CFD149318BCEF714A1254CB5301D1`.
Install to the guarded script-selected Kingmaker UMM mod directory. Use only
`KMG_AUTOMATION_WORKING`; never overwrite `KMG_AUTOMATION_BASELINE`.

Repeat this block separately for Pistol, Musket, Blunderbuss, Rifle, and
Revolver; record the firearm beside every checked result:

Review priority: first confirm held Musket, Blunderbuss, and Advanced Rifle are
visible while idle and firing from front/side/rear/high/low angles. Confirm the
left hand remains present on each support target. Next confirm Advanced Revolver
has no duplicate/wire/helper geometry, then confirm regular Pistol reads as a
flintlock and remains upright. Long-gun belt models are intentionally hidden.

For each long gun verify the firing hand is at the trigger/wrist grip, the butt
is visibly behind it, the muzzle is forward, and the support hand sits outside
or below the fore-stock rather than inside it. Musket must read longer than
Blunderbuss. Check inventory doll, world peaceful/combat idle, firing, recovery,
set switching, and unequipped state separately; doll success alone is incomplete.

- [ ] Inventory doll and world models are visible; intended firing-hand grip,
  outward muzzle, credible scale, acceptable peaceful/combat idle, draw, and
  switching; no duplicate/stale model, torso penetration, barrel-tip pivot, or
  huge stock arc; long-gun support hand contacts wooden fore-end.
- [ ] Normal hit/miss/critical/misfire, full attack, reload, RTwP, and turn-based
  animation are acceptable; Pistol/Revolver do not primarily read as throwing a
  bullet or sword-stabbing; body translation and recovery are acceptable.
- [ ] No crossbow limbs/string/stock, bolt/arrow/trail/quiver, or unrelated
  Scatter projectile; holster is acceptable or intentionally hidden; muzzle
  aligns to barrel; exactly one firearm sound and no crossbow sound; impact/FX
  do not alter damage.
- [ ] Save/reload, area transition, inventory open/close, human male/female,
  dwarf, small race, half-orc, enlarged/reduced, and practical death/revival or
  equipment-disable lifecycle are acceptable; native crossbows remain normal.

Completion rows:

- [ ] Pistol block complete; `PiercingOneHanded` does not read as throwing or stabbing.
- [ ] Musket block complete; support hand remains on the fore-end.
- [ ] Blunderbuss block complete; support hand is on wood, not the flared muzzle; Scatter is clean.
- [ ] Rifle block complete; stock, fore-end and muzzle alignment are credible.
- [ ] Revolver block complete; scale is credible and independently calibrated from Pistol.

## Failure report

Record firearm, body/race, state, action, camera, symptom, screenshot/video if
useful, and whether the development native/custom toggle changes the symptom.
Screenshots are supporting visual evidence only, never mechanical proof.

## Next supervised action

After candidate identities are filled, load only `KMG_AUTOMATION_WORKING`
through the normal authorized UI, open the inventory doll, and evaluate the
Musket model/rig checklist first using the native/custom calibration toggle.
