# Paper Cartridges Implementation Report

Status: prior 0.0.74 candidate rejected for a release-blocking persistent-mode
view-reconstruction crash; repair investigation active on the same unmerged branch.

Exact installed IL identifies the marker's null `FxOnStart` dereference in native
view reconstruction; `FxOnRemove` is independently null-sensitive. The unchanged-
GUID repair supplies empty no-FX links and empty marker/ability resource arrays,
with fail-closed startup validation. Call of the Wild's prefix only checks
`Fact.Active`; it is not the null cause. Deterministic/build qualification passes;
live view-backed and human failing-save acceptance remain pending.

### Repair completion

- Production repair: non-null empty `FxOnStart`/`FxOnRemove` and empty marker/mode
  resource-ID arrays; no Harmony fallback patch and no GUID migration.
- Root-cause classification: Kingmaker native original body was causal; the
  from-scratch project marker was malformed; Call of the Wild was contributory
  only by composing the visible wrapper name and did not dereference the null.
- The dedicated attached-view lifecycle passes standalone, exact CotW, qualified
  Arms & Armor + Toggle, and the bounded all-three-mod profile. Two independent
  comprehensive processes and canonical working-save smoke also pass.
- Existing saves should recover because serialized facts still resolve marker
  GUID `69a804ea1fd14a5da3ba893c373f481f` to the corrected blueprint definition.
  Loading one of the user's already-failing saves remains the decisive human-only
  recovery acceptance item.

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

### Phases 2–6 — mechanics and integrated lifecycle

- One immutable reload plan now binds exact unit, equipped item, canonical
  definition, condition, selected profile/source, loadable rounds, action, and
  Lightning legality across presentation, delivery, native auto-use, and full
  attacks. Loose and Paper inventory sources use one rollback transaction.
- A native per-unit activatable mode is off by default, remains selected at zero
  stock, grants through full and scoped proficiency, and never rewrites a loaded
  chamber or falls back to loose ammunition.
- Lightning Reload dynamically executes Swift/Free, consumes one selected source,
  keeps one use per round, and is used at most once as a genuinely Free inline
  fallback. Normal Free reloads do not spend it.
- Ordinary, Dead Shot, and Scatter use one effective misfire policy: condition
  and training, Paper +1, exact-item Reliable, then one 0..20 clamp. The exact
  pre-discharge ammunition identity survives until evaluation.
- Crafting creates 20 Paper Cartridges for 120 gp using the basic recipe's shared
  rest marker. Paper has zero resale. Smith and every installed BTSL table receive
  one normalized 200-count entry; Jhod and the five rare-firearm loot targets are
  unchanged. Bokken alone is deferred by the exact bounded graph evidence in
  `planning/PAPER-CARTRIDGES-BOKKEN-INVENTORY.md`.
- Deterministic token/codec/repository tests plus guarded reload/lifecycle/switching
  evidence prove Paper Normal/Broken identity, two-item isolation, reconciliation,
  and static-enchantment preservation. No autonomous save write was performed.

### Phase 7 — release and final qualification

- Version is `0.0.74`; informational version is
  `0.0.74-paper-cartridges-auto-reload`. Registry is 248 active blueprints; the
  append-only ledger is 249 identities including one reserved (six additions).
- A final compatibility run exposed an installed direct-field roll composition in
  the ordinary forced-roll diagnostic hook. The exact eligible context now consumes
  and evaluates its authoritative queued roll; production native rolls are unchanged.
  The repaired Wwise regression passed as
  `20260809T0134477575049Z-disposable-firearm-wwise-audio`.
- Complete deterministic suite: 954 PASS, 0 failures. Clean Release, build-output,
  SoundBank, strict standalone package, scenario preflight, and request tests pass.
- Exact final comprehensive PASSes:
  `20260809T0223531574928Z-disposable-paper-cartridge-comprehensive` and
  `20260809T0228043712566Z-disposable-paper-cartridge-comprehensive`.
- Compatibility transactions (all exact restoration `True`): standalone
  `compat-20260809T013637Z-4660ecf4446e`; Arms & Armor
  `compat-20260809T015702Z-08d5c0a31965`; Toggle Custom Soundpacks
  `compat-20260809T020420Z-0100b5e97026`; qualified combined
  `compat-20260809T021134Z-a8ec10bef81e`; bounded Call of the Wild smoke
  `compat-20260809T021852Z-bf16e6df813d` (PASS, public human-gate conflict
  classification unchanged).
- Canonical working-save smokes PASS:
  `20260809T0230156578884Z-working-save-smoke` and
  `20260809T0232431480084Z-working-save-smoke`.
- Final DLL SHA-256:
  `24C06ABAADB0F6CD9BD9BDE1153766C5F343933D93D1CE3F5FD6B94750A1B928`.
  Local-runtime package SHA-256:
  `19AE04841664CF5C54C02D70140D932ED30315DA5026184874F1E20D8B16CE94`.
  Strict release package SHA-256:
  `86C701611008EC9DDD11072130E0B45CF06768444F80C51E3429A877AAC93B4F`.
