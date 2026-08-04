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
Clean local candidate package SHA-256: `E4FC0BDCAD9162232C1059C6E3190AE9EED68EB993DCBF7F8C8E2C4A9DF43086`.
Clean DLL SHA-256: `335BED525E1696915466E5582371420720A09217F6D154DD067E5EA284946B2F`.
AssetBundle SHA-256: `D902F279D8E745BC7852ABDEF6F7C03B97128C92F38641101D5DFC140E39FBFD`.

Clean comprehensive PASS pair: `20260804T0354365307904Z` and `20260804T0356104254703Z`.
Canonical working-save PASS pair: `20260804T0357449794135Z` and `20260804T0359247651962Z`.
Human perception remains authoritative for doll pose/scale/orientation, projectile appearance, and audible quality/layering.
