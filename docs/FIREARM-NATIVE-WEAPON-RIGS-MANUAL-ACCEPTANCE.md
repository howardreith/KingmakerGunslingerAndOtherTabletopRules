# Firearm native weapon rigs manual acceptance

Human observation record: **Regular Pistol held appearance accepted on
2026-08-07.** This narrow verdict covers the observed held model/pose only; it
does not accept unobserved Pistol lifecycle/body/holster states and does not set
the global readiness to `HumanAccepted`. Its Cyril43 source, equipped transform,
scale, and `PiercingOneHanded` animation are frozen for this pass.

Candidate: version `0.0.71`, implementation commit
`fc53c470c94b08265a8a44ce867d7709d7e1003d`. Package
`artifacts/local-runtime/0.0.71/KingmakerGunslinger-0.0.71-local-runtime.zip`,
SHA-256 `2D7D5A107DF377C1C5BC9D4DCDB693DF5826C390223E14AF789CC03EF34CCE4F`;
DLL `D8D717C21B24CD8EE1702D979132BB5E2123DD513147D39FD804B48728CF4E1D`;
AssetBundle `4A96CD13152A9EF6B48B3758B697659DCC82BC92D46A97AC8FBAAD815E386B2B`;
rig manifest `60D143952974B8B9039E45B7F4E5B14A7D33294BA89FC336ADCC5CDD7A65571D`.
Install to the guarded script-selected Kingmaker UMM mod directory. Use only
`KMG_AUTOMATION_WORKING`; never overwrite `KMG_AUTOMATION_BASELINE`.

Repeat this block separately for Pistol, Musket, Blunderbuss, Rifle, and
Revolver; record the firearm beside every checked result:

Review priority: first confirm held Musket, Blunderbuss, and Advanced Rifle are
visible while idle and firing from front/side/rear/high/low angles. Confirm the
left hand remains present on each support target. Next confirm Advanced Revolver
has no duplicate/wire/helper geometry, then confirm regular Pistol reads as a
flintlock and remains upright. Long-gun belt models are intentionally hidden.

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
