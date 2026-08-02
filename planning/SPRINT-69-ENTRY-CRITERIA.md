# Sprint 69 entry criteria: save-free production critical profiles

## Existing contract

The five registered production weapon types retain native critical fields:
Pistol, Musket, Advanced Rifle, and Advanced Revolver are 20/x4;
Blunderbuss is 20/x2. No separately authorized special-ammunition deliverable
exists in the current roadmap, and none may be invented.

## Required observer

- Extend the existing save-free, read-only vendor/catalog observer with the
  exact five-profile assertion already used by the save-backed catalog gate.
- Read only registered `BlueprintWeaponType` fields.
- Do not invoke a vendor, open a shop, touch inventory, load a save, or mutate
  any blueprint/game state.
- Preserve the existing save-backed assertion for later integrated regression.

## Qualification

Focused source validation, repository validation, the complete domain suite,
clean exact-reference Release build, strict package validation, exact mod load,
and two fresh guarded save-free observations must pass.
