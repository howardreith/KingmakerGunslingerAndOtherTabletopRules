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
- Post-commit comprehensive run:
  `20260804T1248487401307Z-disposable-gunslinger-comprehensive-acceptance` PASS
  with 33 slices and 194 assertions.
- Canonical working-save runs:
  `20260804T1250280019663Z-working-save-smoke` and
  `20260804T1252146835864Z-working-save-smoke` PASS.
- Two same-commit package builds were byte-identical.
- Final functional HEAD: `448e0d13d44574a66ea034632374264cee56e8d1`.
- Package SHA-256:
  `32D6269BAD730F3715DA20DB817268A435027CAE1FF2F7F9DCA80355C685DD7E`.
- DLL SHA-256:
  `D74A42CB9FCD8B2471E8CC169736501D3400AF5E63419DD8ADD49077CA92C931`.
- DLL MVID: `7451652c-4a85-4e0f-8db3-657abcb1f8d7`.
- Firearm bundle SHA-256:
  `1D72CE9067B9B30929CC77F5BC2E1778AA26CA860CF83449A2E2F0EAD7F2CDA3`.
- Live DLL and bundle hashes exactly match the qualified build.
- Git diff, tracked-save, staged-content, credential/private-material, and
  generated-package tracking audits found no release blocker.

The repository-restricted GitHub checkpoint publisher was invoked exactly as
required after the checkpoint commit, but workstation network security rejected
the operation categorically. No raw push or workaround was attempted.

## Remaining authority

Grip, orientation, poses, holster appearance, invisible projectile presentation,
audible layering, and shop UI visibility require the consolidated human pass.
