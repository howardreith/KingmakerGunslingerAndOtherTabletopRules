# Firearm native weapon rigs manual acceptance

Candidate identity is pending. Before use, fill exact version, commit, package
path/SHA-256, DLL SHA-256, AssetBundle SHA-256, rig-manifest SHA-256,
installation path, and authorized save (`KMG_AUTOMATION_WORKING`). Never use or
overwrite `KMG_AUTOMATION_BASELINE`.

For each Pistol, Musket, Blunderbuss, Rifle, and Revolver:

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

## Failure report

Record firearm, body/race, state, action, camera, symptom, screenshot/video if
useful, and whether the development native/custom toggle changes the symptom.
Screenshots are supporting visual evidence only, never mechanical proof.

## Next supervised action

After candidate identities are filled, load only `KMG_AUTOMATION_WORKING`
through the normal authorized UI, open the inventory doll, and evaluate the
Musket model/rig checklist first using the native/custom calibration toggle.

