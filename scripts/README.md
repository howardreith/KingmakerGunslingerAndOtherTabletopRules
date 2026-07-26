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

Do not bypass a failed report. Exact compilation and tests establish candidate quality; `SMOKE-TEST-GUIDE-0.0.29.md` establishes the complete action-bar maintenance loop, interruption safety, exact resource deltas, and persistence. Sprint 30 remains blocked until the 0.0.29 runtime gate passes.
