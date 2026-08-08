# Paper Cartridges Implementation Report

Status: implementation not yet started. This report will be updated after every
qualified phase and finalized only after the complete work-order definition of
done is met.

## Baseline

- Base: `759685077da0aed6d7ed1fda2cd43e5ad12d0bdb`.
- Branch: `codex/paper-cartridges-auto-reload`.
- Starting version: `0.0.73` / `0.0.73-pistolero-musket-master`.
- Target version: `0.0.74` / `0.0.74-paper-cartridges-auto-reload`.

## Planned implementation

1. Immutable ammunition profiles and definition-family compatibility.
2. Paper item and two append-only loaded-state token blueprints.
3. One reload plan and one generic atomic source transaction.
4. Per-unit native Paper mode plus proficiency grants.
5. Shared manual, native auto-use, full-attack, and Lightning Reload integration.
6. One misfire policy for ordinary fire, Dead Shot, and Scatter.
7. Shared-marker crafting, zero resale, and bounded Smith/BTSL/Bokken acquisition.
8. Persistence/static-enchantment regression, guarded runtime, compatibility,
   version/package qualification, and final documentation.

## Evidence

### Phase 1 — ammunition and state foundation

- Added immutable loose and Paper Cartridge profiles. The Paper profile owns
  loaded ID `kmg.ammunition.paper-cartridge`, one-step reload reduction, +1
  misfire modifier, and definition-driven Early Pistol/Musket/Blunderbuss
  compatibility; advanced definitions reject it.
- Added stackable inert zero-weight 12-gp Paper Cartridge item
  `KMG.Ammunition.PaperCartridge` / `fea7337cfd06417a853546af9d950f77`.
- Added project-owned 128x128 Paper Cartridge art and source/provenance record.
- Appended Normal/Broken paper-loaded token identities
  `a6344f33e7344d4aab249485faedf7fd` and
  `fdd814300fff4eea89d9d508663aebc0`. All four old token IDs remain exact.
- Blueprint registration is 245 active; ledger is 246 stable identities (one
  reserved). No production combat/reload path selects paper yet.
- Repository validation, 941/941 tests, clean exact-reference Release,
  build-output, SoundBank, package creation, and strict validation pass.
- Intermediate package SHA-256:
  `9c019ff426484b8d3ddc65f1d4b1164288efe4f594e10c37cfbb17fe68ac0139`.
  DLL SHA-256:
  `69bc766e65fc13f0b239c6805ef5bb07bd0e990747bd1d230b9c2b2d9c381168`.
