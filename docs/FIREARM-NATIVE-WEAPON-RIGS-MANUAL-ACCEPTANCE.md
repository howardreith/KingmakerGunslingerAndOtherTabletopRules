# Firearm native weapon rigs manual acceptance

Candidate: version `0.0.71`, implementation commit
`d7b6bc1756ae89f5e043c5b3362a46e8fe614e8f`. Package
`artifacts/local-runtime/0.0.71/KingmakerGunslinger-0.0.71-local-runtime.zip`,
SHA-256 `6B3E85517C945B7CB6096E83C2946706749B91C142FA5C7412044EBDD5A03D81`;
DLL `B1C181740DF76179B145D5C9A03B420DADDB71E6AA938445FDBAA5351660CE5F`;
AssetBundle `62BAB35C9DEB94AE98B61CD8B56CA523CC946A740248C06B63E8E41A94AE7CDD`;
rig manifest `429A4E7A30553C016EFEEA95951598164D6F7A4930218A64977EA7DEBD2C2B7F`.
Install to the guarded script-selected Kingmaker UMM mod directory. Use only
`KMG_AUTOMATION_WORKING`; never overwrite `KMG_AUTOMATION_BASELINE`.

Repeat this block separately for Pistol, Musket, Blunderbuss, Rifle, and
Revolver; record the firearm beside every checked result:

Review priority: first confirm held Musket and Blunderbuss remain fully visible
from front, side, rear, high, and low camera angles without changing their good
grip. Next confirm Pistol is upright and its muzzle still points outward. Treat
long-gun back placement last; the current candidate intentionally hides those
belt models.

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
