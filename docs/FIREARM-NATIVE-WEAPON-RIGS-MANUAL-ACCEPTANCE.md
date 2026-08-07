# Firearm native weapon rigs manual acceptance

Candidate: version `0.0.71`, commit
`3ae6b5d903720dbd450a2bb3fa82ed32d0b14c4d`. Package
`artifacts/local-runtime/0.0.71/KingmakerGunslinger-0.0.71-local-runtime.zip`,
SHA-256 `9F905766214BEB2AC23E2519525826B14970FA7CDE32D305BD8D4E9D2452DF2D`;
DLL `479244B41883256831396E60FFCC9CFD06E6F40544AF6E8185D0785831D5000C`;
AssetBundle `88DF971967ECF4879BAA93FE79A734D46ABA2A754AEBD193FAE01AB756DCFD91`;
rig manifest `2DD5D5F69C99925B8D390292B1FC3045BC7775CBB04B3D136FF0938D04BF9CA6`.
Install to the guarded script-selected Kingmaker UMM mod directory. Use only
`KMG_AUTOMATION_WORKING`; never overwrite `KMG_AUTOMATION_BASELINE`.

Repeat this block separately for Pistol, Musket, Blunderbuss, Rifle, and
Revolver; record the firearm beside every checked result:

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
