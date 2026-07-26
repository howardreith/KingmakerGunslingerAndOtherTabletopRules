# Sprint 2 Entry Criteria and Scope

## Goal

Create a buildable repository scaffold that references a local Kingmaker install without redistributing game binaries.

## Inputs to capture on the development machine

- Kingmaker install path.
- Storefront and displayed version.
- `Assembly-CSharp.dll` SHA-256.
- UMM version and install method.
- `UnityModManager.dll` assembly identity/SHA-256.
- Harmony/proxy files in the UMM managed directory.
- Operating system.
- Enabled-mod list for the clean baseline.

## Bounded Sprint 2 deliverables

```text
KingmakerGunslinger.sln
src/KingmakerGunslinger/KingmakerGunslinger.csproj
GamePath.props.example
.gitignore
Directory.Build.props (only if useful)
Info.json
Main.cs placeholder
Properties/AssemblyInfo.cs or SDK equivalent
scripts/build.ps1
scripts/package.ps1
README build section
```

The project will:

- target .NET Framework 4.7;
- use C# 7.3;
- target AnyCPU with Prefer32Bit false;
- resolve references from `KingmakerInstallDir`;
- set game/Unity/UMM references to non-copying;
- copy only project-owned output and metadata;
- contain Debug and Release configurations;
- fail with a clear message when the game path is absent.

## Acceptance

1. A valid local `GamePath.props` allows the solution to restore/build.
2. The output contains only project-owned DLL/PDB/JSON files.
3. No proprietary assembly appears in source control or the package.
4. `Info.json` points to the future entry method.
5. The blueprint manifest is copied into the development output or embedded by an explicit decision.
6. The package script produces a correctly rooted UMM folder, even though the mod has no functional bootstrap until Sprint 3.

## Explicit exclusions

Sprint 2 does not install Harmony patches, register blueprints, create the class, or create a firearm.
