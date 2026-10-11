# Build and qualification scripts

The current [validation policy](../docs/VALIDATION-POLICY.md) makes
`Invoke-KmgGate.ps1 -Filter <affected names>` the ordinary development entry.
Sprint/Tranche, packages, purity, native campaigns and release reproducibility
are explicit broad boundaries. Preserve the actual current installation and
every live transaction guard. Original Sprint 29/30 guidance is retained in
[history](../docs/history/scripts-README-before-workflow-reset-2026-10-11.md).

The PowerShell scripts target a local Windows installation of Pathfinder: Kingmaker 2.1.7b.

`Invoke-KingmakerRuntimeTest.ps1 -Scenario working-save-smoke` requires
`-SaveName KMG_AUTOMATION_WORKING`, rejects manual-interaction mode, and launches
only through Steam App ID 640820. See `docs\WORKING-SAVE-SMOKE.md`.

- `qualify-runtime-candidate.ps1` is a historical broad qualification path. The 0.0.29 record used 599 tests three times and two Release compiles; those counts/repeats are not current development requirements.
- `build.ps1` compiles the mod against the exact installed private Kingmaker, Unity, UMM, Harmony, and Newtonsoft references.
- `package.ps1` produces the strict standalone UMM ZIP using the current validated inventory, including
  the installation/update/removal/compatibility guide.
- `inspect-runtime-contracts.ps1` records the retained attack, natural-roll, save, damage, item, inventory, and state contracts from the supplied installation.
- `export-private-build-references.ps1` creates the narrow private compiler-input archive. Never include that archive or its DLLs in a UMM package.
- `package-source.ps1` creates the source archive without loose binaries or private references.
- `validate-repository.ps1`, `validate-build-output.ps1`, and `validate-package.ps1` enforce source, build, and package boundaries.

The tools in `tools/` provide exact-reference compilation and source/package
validation. Older 599-test/Sprint 29 records describe their original inputs.

## Local runtime harness

`Build-Local.ps1` is the non-deploying explicit broad candidate entry. It uses the
version-aware validator, complete domain suite, preserved qualified references,
existing package staging/validation, and deterministic ZIP writer. The
Backup/Deploy/Restore scripts are exact-directory constrained and support
`-WhatIf`. Launching and all live writes require a separate explicit task.

The harness does not need `.worktreeinclude`: it derives the isolated reference
location from the lab workspace and never copies ignored machine configuration
into a worktree.

`Invoke-KingmakerRuntimeTest.ps1` is the guarded Windows 10 orchestration entry
point. It includes guarded smoke tests and explicitly supervised observation
scenarios; use `-WhatIf` for source-only qualification.
`observe-save-catalog-provider` requires `-ManualInteractionRequired`, waits
for hook readiness, and permits only the human opening the Load Game screen.
`Test-RuntimeRequest.ps1`,
`Test-RuntimeResult.ps1`, and `Test-RuntimeRunner.ps1` validate the guarded
protocol without launching Kingmaker. `Capture-WindowEvidence.ps1` optionally
captures one explicit process-owned window with GDI and returns warning-only
failure; it is never a correctness source.

Do not bypass a failed report. Exact compilation and selected tests establish a
development checkpoint within their scope; native behavior needs its relevant
guarded evidence. `SMOKE-TEST-GUIDE-0.0.29.md` retains that version's maintenance,
interruption, resource and persistence requirements. Its old Sprint 30 restriction
is historical and cannot select an old campaign for current workflow work.


`Test-FirearmInputControlFlow.ps1 -ModDllPath <candidate DLL under artifacts>`
runs ten isolated CLR/Harmony behavior checks against the compiled firearm input
wrapper: desktop Boolean returns, controller void returns, ordinary exits,
exceptions and nested native finally blocks. Run it with Windows PowerShell 5.1
after `Build-Local.ps1`. It reads the installed private references, launches no
game, and reports the exact tested DLL hash. These synthetic control-flow checks
supplement guarded runtime input proof; they do not qualify firearm mechanics.

## Firearm postrelease native qualification

`Test-FirearmInputControlFlow.ps1 -ModDllPath <candidate DLL under artifacts>`
runs ten synthetic managed return/exception/finally checks on the compiled native
input wrapper. Use Windows PowerShell 5.1; this is separate from game proof.

The guarded `disposable-firearm-break-interruption` scenario at version 0.0.128
executes the real native desktop/controller order adapters, reload/action economy,
weapon rules, recovery and stale-order negatives in request-owned save-free
fixtures. Its 145 assertions and precise pause/rest boundaries are documented in
`docs/FIREARM-POSTRELEASE-HOTFIX-QUALIFICATION.md`. It never uses authorization
bridges, direct suppression resets, save selection or save mutation.
