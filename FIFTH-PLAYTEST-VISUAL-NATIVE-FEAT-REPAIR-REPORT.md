# Fifth playtest visual and native-feat repair report

## Outcome

Version 0.0.65 is functionally repaired and awaits the final supervised visual/audio verdict. Firearm parameters now flow through both native parametrized-feature enumeration routes. Equipped item visuals, as well as weapon-type visuals, receive approved firearm prefabs. Native crossbow sounds are suppressed, and successful discharge invokes the mapped firearm clip exactly once. The inherited projectile sequence remains because removing it caused a demonstrated Targeting Arms damage-suppression regression; its live appearance remains an explicit human-review item.

## Runtime evidence

- Native menus: `20260804T0324161708239Z-observe-native-weapon-feat-contracts`.
- Unit-aware parameter menus: `20260804T0327479465160Z-disposable-firearm-dependent-feats` (an updated commit-level run follows final build).
- Five repaired item/type visuals: `20260804T0338269806180Z-observe-production-firearm-fallbacks`.
- Native damage, chamber consumption, and exact flintlock invocation: `20260804T0339517895460Z-disposable-gunslinger-stunning-shot`.
- Deterministic tests: 882/882 PASS.

## Candidate identities

Functional commit: `b8ef562` on `codex/fifth-playtest-visual-native-feat-repair`.
Qualified local candidate package SHA-256: `CE69D364895E7CC5A60450E3D7127D0E07FD2702B5DBA734FC9D30F6CE523D06`.
Qualified DLL SHA-256: `2C91D81A0B5838EEEEE83CF40B75496A9141DC6E72ACE5040A43DA1620F20084`.
AssetBundle SHA-256: `D902F279D8E745BC7852ABDEF6F7C03B97128C92F38641101D5DFC140E39FBFD`.

Clean comprehensive PASS pair: `20260804T0354365307904Z` and `20260804T0356104254703Z`.
Canonical working-save PASS pair: `20260804T0357449794135Z` and `20260804T0359247651962Z`.
Human perception remains authoritative for doll pose/scale/orientation, projectile appearance, and audible quality/layering.
