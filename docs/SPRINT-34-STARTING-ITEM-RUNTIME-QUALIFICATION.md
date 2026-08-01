# Sprint 34 starting-item runtime qualification

## Scope

This checkpoint qualifies the production Gunslinger starting-item contract on
Kingmaker's native class-item helper. It does not claim that a normal character
creation commit, Gunsmith/battered state, later acquisition, level-up,
multiclass, or respec is complete.

## Qualified behavior

- Exact production array: Early Pistol, black powder, lead ball.
- Exact native receiver: loaded working-save main descriptor.
- Exact native call: `LevelUpHelper.AddStartingItems(UnitDescriptor)`.
- Observed deltas: one Pistol, one powder, one ball.
- Native stacking: one new Pistol instance; ammunition merged into existing
  stacks on the qualified working save.
- Rollback: exact new instances removed, only merged excess quantities removed,
  original inventory references and quantities restored, class identity and
  class starting gold restored, player money unchanged.
- Save safety: unique `KMG_AUTOMATION_WORKING` correlation, distinct baseline,
  stable fingerprint, and no save-writing API.

## Evidence

- Source commit: `cc2f77d0258910c0874fcc08475ed6978b7b0502`.
- Mod-load smoke evidence directory:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260801T1731148707849Z-mod-load-smoke`.
- First feature PASS:
  `20260801T1732334037249Z-ccbfd7861d04442782c358cf7d236dc9`.
- Second feature PASS:
  `20260801T1734133597055Z-4fa5d631d72f4b999751ceeb7d119fd5`.
- Package SHA-256:
  `6a41ed815d910983e0067184fdfb40ca16629b8e70aab83f4d1a24fa4ff57153`.
- DLL SHA-256:
  `2f4d2a0f2772b5923349ce467bbebf7426bb80348c25548d16f0238af23d0fb4`.

Source gates passed: repository validation, runtime request tests 18/18,
scenario preflight tests 40/40, complete domain/reflection suite 691/691,
clean exact private-reference Release compile, build-output validation, and
strict standalone package validation.

## Diagnostic correction retained

The first feature attempt failed because the acceptance code assumed every
class item would create a new object and used an ambiguous inherited Blueprint
property lookup. The structured failure established native stack merging and
was corrected by measuring exact blueprint quantities. No save write occurred,
and the non-ammunition state was restored.

## Remaining boundary

This helper-level proof is necessary but not sufficient for full creation
integration. The next checkpoint must establish the normal creation commit
outcome through an exact reversible boundary, followed by level-up,
multiclass, and respec qualification.
