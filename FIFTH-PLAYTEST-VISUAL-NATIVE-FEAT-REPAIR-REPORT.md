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

Pre-commit local candidate package SHA-256: `0393ADE441798B8FFC8A328845B15ED4DADFED009962FB3E8DA314FF3B96CD17`.
Pre-commit DLL SHA-256: `92954F5FCA705B523A5CC3A81FF6DADD9A9D8E3C1BA81A81340AAF8037820C30`.
AssetBundle SHA-256: `D902F279D8E745BC7852ABDEF6F7C03B97128C92F38641101D5DFC140E39FBFD`.

Final clean-commit hashes and qualification IDs are appended after the clean build. Human perception remains authoritative for doll pose/scale/orientation and audible quality/layering.
