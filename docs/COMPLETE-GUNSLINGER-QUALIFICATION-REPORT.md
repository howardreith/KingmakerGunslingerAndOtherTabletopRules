# Complete Gunslinger qualification report

## Qualified baseline

- Starting runtime baseline: `4f28dcf`
- Final qualified implementation: `eda0202223cda378aa2a87b7ec84d92fd30d988e`
- Branch: `codex/complete-gunslinger`
- Mod version: `0.0.60`

## Implemented scope

The base Gunslinger is playable from character creation through level 20. The
qualified scope includes the class chassis and presentation; proficiency,
Gunsmithing, grit, every meaningful base deed, Nimble, bonus feats, Gun
Training, and True Grit; early and advanced item-owned firearms; ammunition,
capacity, reload, native attacks and touch AC, misfire and condition changes,
scatter, repair and overhaul; persistence, migration, transfer, vendor, and
diagnostic behavior; and player-facing equipment acquisition and actions.

Every mandatory coverage row is `RUNTIME-QUALIFIED` or otherwise has an
accepted mission disposition. Every base feature is classified in the fidelity
matrix. No mandatory row remains blocked or incomplete.

## Excluded and adapted scope

Archetypes, third-party deeds, exhaustive published firearm content, custom 3D
models, custom animation controllers, and unrelated systems remain outside the
mission. Targeting Wings and the two noncombat Utility Shot interactions are
omitted because installed Kingmaker exposes no meaningful general interaction.
All other adaptations, including Kingmaker action-economy translations and
Death's Shot's exact native terminal death state, are recorded in
`planning/GUNSLINGER-FIDELITY-MATRIX.md`.

## Source and package qualification

- Repository validation: PASS
- Complete dependency-free suite: 854/854 PASS
- Clean exact-reference Release build: PASS
- Strict standalone package validation: PASS
- Package SHA-256: `55ce7a9b5de1f35acac378b68f6fac3cea4bd745ba2986932b8e9b4cd7bc95e7`
- DLL SHA-256: `390c773924d52ddd9afaade21f911cb30c7c313e94e75ce41dec3bd60e858853`
- Installable artifact: `artifacts/local-runtime/0.0.60/KingmakerGunslinger-0.0.60-local-runtime.zip`

## Final runtime qualification

Exact `eda0202` passed final mod load:

- `20260802T2020407693601Z-mod-load-smoke`

It then passed the complete 32-slice autonomous acceptance twice from fresh,
independent Steam-launched Kingmaker processes:

- `20260802T2017342856278Z-disposable-gunslinger-comprehensive-acceptance`
- `20260802T2019030300247Z-disposable-gunslinger-comprehensive-acceptance`

Both comprehensive runs completed all slice-owned cleanup. The guarded harness
observed no unexpected save-writing API. All final scenarios were save-free;
`KMG_AUTOMATION_BASELINE` was neither selected nor accessed.

## Known limitations and operations

The documented fidelity omissions and approved adaptations are the known
limitations. Installation, removal warnings, version compatibility, fallback
visuals, and acquisition paths are documented in the packaged README,
installation compatibility guide, smoke-test guide, and fidelity matrix. No
known critical or major defect remains.

Recommended integration command (human-operated; autonomous merge is
forbidden):

```text
git switch <integration-branch>
git merge --no-ff codex/complete-gunslinger
```

COMPLETE BASE GUNSLINGER QUALIFIED
