# Sixth-playtest repair report

## Baseline proof

The exact 0.0.65 live installation matched the qualified package: baseline
commit `5c6be16e1dc2f243b31d465abf1c9adc9c45898c`, package SHA-256
`CE69D364895E7CC5A60450E3D7127D0E07FD2702B5DBA734FC9D30F6CE523D06`,
live DLL SHA-256 `2C91D81A0B5838EEEEE83CF40B75496A9141DC6E72ACE5040A43DA1620F20084`,
bundle SHA-256 `D902F279D8E745BC7852ABDEF6F7C03B97128C92F38641101D5DFC140E39FBFD`,
and MVID `1d2371e3-4e3c-4139-bb1c-c54d43a146f7`. Unity is 2018.4.10f1.

The supplied screenshots confirm the Pistol was barrel-gripped/backward and
used a shoulder crossbow holster. The qualified Pistol mapping was prefab
`Pistol`, clip `gunantq_flintlock fire_cs_usc.wav`, and native projectile
`ArrowCrossBow00`.

## 0.0.66 functional changes

- Exact four-table standalone/campaign BTSL publication with optional-DLC,
  idempotence, partial-publication rejection, and rollback.
- Gunsmith's Kit, Overhaul Kit, and atomic once-per-rest 20/20 crafting.
- Immediate Dodge and Deadeye expenditure with visible one-round buffs.
- Immediate critical/zero-crossing Grit restoration diagnostics.
- Explicit five-family presentation profiles; corrected short-firearm wrapper
  scale/yaw/grip pivot; hidden short-firearm holster; no stale item/type visual
  prototype; no whole-unit renderer scan.
- Stable clone-derived projectile blueprint retaining native callbacks while
  suppressing only that projectile's inherited renderers.
- Spatial persistent firearm emitter. Final absence of layered native sound is
  still an audible human claim, not an automated claim.

## Qualification evidence

- Repository validation and clean Release/package pipeline PASS.
- Complete deterministic suite: 882/882 PASS.
- Mod load: `20260804T1230532823354Z-mod-load-smoke` PASS.
- Exact four-table vendor observer:
  `20260804T1231558243754Z-observe-vendor-table-contracts` PASS.
- Corrected presentation observer:
  `20260804T1235585047424Z-observe-production-firearm-fallbacks` PASS.
- Comprehensive pre-crafting run:
  `20260804T1237039801982Z-disposable-gunslinger-comprehensive-acceptance` PASS.
- Comprehensive crafting-inclusive run:
  `20260804T1240496957011Z-disposable-gunslinger-comprehensive-acceptance` PASS.

Final hashes, deterministic repeat runs, working-save runs, and audit results
are appended after the final clean package qualification.

## Remaining authority

Grip, orientation, poses, holster appearance, invisible projectile presentation,
audible layering, and shop UI visibility require the consolidated human pass.
