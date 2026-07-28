# Build and qualification scripts

The PowerShell scripts target a local Windows installation of Pathfinder: Kingmaker 2.1.7b.

- `qualify-runtime-candidate.ps1` is the Windows source-to-candidate path. For 0.0.29 it requires 599 tests three times, two same-output-path Release compiles, and strict package validation.
- `build.ps1` compiles the mod against the exact installed private Kingmaker, Unity, UMM, Harmony, and Newtonsoft references.
- `package.ps1` produces the strict standalone eight-file UMM ZIP.
- `inspect-runtime-contracts.ps1` records the retained attack, natural-roll, save, damage, item, inventory, and state contracts from the supplied installation.
- `export-private-build-references.ps1` creates the narrow private compiler-input archive. Never include that archive or its DLLs in a UMM package.
- `package-source.ps1` creates the source archive without loose binaries or private references.
- `validate-repository.ps1`, `validate-build-output.ps1`, and `validate-package.ps1` enforce source, build, and package boundaries.

The cross-platform evidence tools in `tools/` provide the equivalent exact-reference compile, 599-test three-run execution, Sprint 29 source validation, and standalone-package validation used for the current artifacts.

## Local runtime harness

`Build-Local.ps1` is the non-deploying Sprint 30 build entry point. It uses the
version-aware validator, complete domain suite, preserved qualified references,
existing package staging/validation, and deterministic ZIP writer. The
Backup/Deploy/Restore scripts are exact-directory constrained and support
`-WhatIf`. Launching and all live writes require a separate explicit task.

The harness does not need `.worktreeinclude`: it derives the isolated reference
location from the lab workspace and never copies ignored machine configuration
into a worktree.

`Invoke-KingmakerRuntimeTest.ps1` is the guarded Windows 10 orchestration entry
point. Its only current scenario is `mod-load-smoke`; use `-WhatIf` for
source-only qualification. `Test-RuntimeRequest.ps1`,
`Test-RuntimeResult.ps1`, and `Test-RuntimeRunner.ps1` validate the guarded
protocol without launching Kingmaker. `Capture-WindowEvidence.ps1` optionally
captures one explicit process-owned window with GDI and returns warning-only
failure; it is never a correctness source.

Do not bypass a failed report. Exact compilation and tests establish candidate quality; `SMOKE-TEST-GUIDE-0.0.29.md` establishes the complete action-bar maintenance loop, interruption safety, exact resource deltas, and persistence. Sprint 30 remains blocked until the 0.0.29 runtime gate passes.
