# Firearm native weapon rigs manual acceptance

Human observation record: **Regular Pistol held appearance accepted on
2026-08-07.** This narrow verdict covers the observed held model/pose only; it
does not accept unobserved Pistol lifecycle/body/holster states and does not set
the global readiness to `HumanAccepted`. Its Cyril43 source, equipped transform,
scale, and `PiercingOneHanded` animation are frozen for this pass.

Current micro-calibration candidate: implementation
`5a37f16a176b54a71d18924c42f769caea5c92c2`; package SHA-256
`3296604A13F738DC4E8388F3FD8320AB9BA520BD7C9B6ABC04B16B2C114E6B99`;
DLL `00C19F621AD6184EED6B000ACD76D9C5DC19F5616F8DF91AFA7A1C171A32AF14`;
AssetBundle `EEEBA3292119A4619EE3D391246C55E47FC5D9E0BA625DB19E5AB9BBF124315E`.
Compare only Musket and Blunderbuss against
the prior semantic-anchor package. The candidate moves each complete held rig
approximately `0.020` local units outward; it does not alter length, scale,
rotation, animation, Pistol, Rifle, or holsters. Accept minor residual clipping
when further tuning merely trades one small flaw for another.

Final bounded candidate supersedes that rejected outward-offset package: it
restores the prior human-best Musket/Blunderbuss held anchors and intentionally
shows no Musket, Blunderbuss, or Rifle on the back. Minor residual held clipping
is accepted for now. Confirm absence of firearm, native crossbow sheath, bolt
container, and quiver after switching each long gun inactive.

Final implementation `6b1f5db443c1051ecd949c8987b75ccd3c69c78d`;
package SHA-256
`FA955857DA4DDE83D43107D57A6CE4B1E41F738A4BB18F30269F4A69F067740D`;
DLL `BAFC115F3839B7D31E6DB9BB5C3D6D97FFB7BCCA97416AD440FA2997B0CD4E74`;
AssetBundle `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`.

**REJECTED — DO NOT INSTALL/PROMOTE AS A PASS:** human testing found the held
Musket and Blunderbuss completely invisible. The renderer assertions above did
not prove player-visible hand-slot attachment. The narrow replacement candidate
must first prove Pistol, held Musket, and held Blunderbuss visible before any
full regression or completion verdict.

## Attach-slot experiment A — narrow checkpoint

This candidate restores inherited native attach slots and override behavior
while keeping the failed candidate's sheath patch enabled. Check only:

Commit `6e3aa3782eb6328786b60330ae453fa2d5241f6a`; package
`artifacts/local-runtime/0.0.71/KingmakerGunslinger-0.0.71-local-runtime.zip`;
package SHA-256
`CE0C03BE2AF4D0BA0BBFF6A975C5733D106716B4BE581F69F44ED46140B2F90D`;
DLL `BC1B4C8B67B8CD68A654DD1334361C61A47733A292A78138DC0239874B8387DC`;
AssetBundle `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`.

- [ ] Pistol remains visible and unchanged.
- [ ] Musket is visible while its weapon set is selected.
- [ ] Blunderbuss is visible while its weapon set is selected.
- [ ] Musket is absent from the back after switching away.
- [ ] Blunderbuss is absent from the back after switching away.

If either held long gun remains invisible, report Experiment A FAIL; the next
isolated step is disabling only `FirearmHiddenHolsterPatch`. Do not infer a pass
from automated renderer counts.

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
through the normal authorized UI. Compare Musket, then Blunderbuss in inventory
doll idle/firing, world idle/firing, and set switching. Confirm the rear body is
less buried in the torso and the support hand remains plausible; stop tuning if
the remaining defect is minor or the next adjustment worsens another angle.
