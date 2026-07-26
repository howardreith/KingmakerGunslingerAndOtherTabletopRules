# Sprint 2 report — solution and project scaffold

**Version:** `0.0.2-s02-scaffold`
**Prepared:** 2026-07-12
**Result:** Source milestone complete; local compile/runtime gate remains open.

## Goal

Create a buildable repository scaffold that resolves its references from a user-local Pathfinder: Kingmaker installation and cannot redistribute those references accidentally.

## Delivered

### Solution and compiler policy

- `KingmakerGunslinger.sln` with Debug and Release Any CPU configurations.
- Classic C# class-library project targeting .NET Framework 4.7 and C# 7.3.
- `Prefer32Bit=false`, deterministic output, warning level 4, and warnings-as-errors.
- Project-owned output isolated beneath `artifacts/`.

### Local-reference boundary

- `GamePath.props.example` and `new-game-path-props.ps1` establish a local-only `KingmakerInstallDir`.
- The project validates the install directory and ten required game/Unity/UMM assemblies before compilation.
- All non-framework references explicitly set `Private=False`.
- A post-build target rejects accidental copies of known external assemblies.

### UMM package substrate

- `Info.json` declares `KingmakerGunslinger.dll` and `KingmakerGunslinger.Main.Load`.
- `Main.Load` is a harmless loader stub that logs the scaffold version and performs no patches or gameplay changes.
- The package script creates exactly one UMM root folder and copies only project-owned files.
- Separate validators audit build output and expanded install archives against allowlists.

### Stable blueprint data

- All nine Sprint 1 GUID reservations are unchanged.
- `blueprints.schema.json` now accompanies the manifest.
- ADR-0009 records the decision to copy the inspectable manifest and schema into build/install output.

### Reproducibility and diagnostics

- Environment fingerprint script records assembly identity, file version, size, and SHA-256.
- Source-package script excludes local paths, outputs, and binaries.
- Portable Python validation supports structural auditing on non-Windows hosts.
- Original project source is now explicitly MIT licensed; third-party/game content remains outside that grant.
- Sprint 3 entry criteria isolate the UMM/Harmony bootstrap work.

## Acceptance assessment

| Acceptance condition | Result |
|---|---|
| Valid local `GamePath.props` can supply the project references | Implemented; requires Windows/local proof |
| Output contains only project-owned DLL/PDB/JSON | Enforced by project and script allowlists; requires compiled-output proof |
| No proprietary assembly in source or milestone package | Passed |
| `Info.json` points to the future entry method | Passed |
| Blueprint manifest is explicitly deployed | Passed; copied beneath `blueprints/` |
| Package script creates correctly rooted UMM folder | Implemented and statically inspected; PowerShell execution requires Windows/local proof |

## Deliberate limitation

A compiled DLL was not fabricated against homemade stubs. Such a binary could pass superficial packaging checks while failing against the real Mono/UMM assembly identities. The correct next evidence is a build against the owner's installed game, followed by Sprint 3's bootstrap runtime trace.
## Milestone artifacts

The delivered milestone contains a reproducible source archive, a machine-readable file manifest, per-file SHA-256 values, and an outer archive checksum. It deliberately contains no fabricated DLL or incomplete UMM install archive.
