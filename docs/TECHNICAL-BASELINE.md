# Technical Baseline

## Pinned target

| Concern | Sprint 1 baseline | Status |
|---|---|---|
| Game | Pathfinder: Kingmaker Enhanced Plus Edition `2.1.7b` | Target |
| Steam build | `6757524`, published 2021-05-27 | Reference build |
| Primary validation OS | Windows x64 | Required first |
| Input/UI | Keyboard and mouse first | Required first |
| Unity Mod Manager | `0.32.5` validation target | Must be verified in an actual Kingmaker install |
| Mod API | Unity Mod Manager entry method | Selected |
| Harmony source API | Legacy `Harmony12` namespace / `0Harmony12.dll` compatibility surface | Selected provisionally |
| .NET target | .NET Framework `4.7` | Selected |
| C# language | `7.3` | Selected |
| Platform target | `AnyCPU`, `Prefer32Bit=false` | Selected |
| Build output | Class library DLL plus UMM metadata | Sprint 2 |
| Runtime dependencies | None beyond game/UMM assemblies | Policy |

Steam's patch history lists 2.1.7b as the most recent public Kingmaker patch. Storefront-specific assembly parity still has to be checked rather than assumed.

## Why this toolchain

Call of the Wild's Kingmaker project targets .NET Framework 4.7, C# 7.3, AnyCPU, and references the installed game's `0Harmony12.dll`, `Assembly-CSharp.dll`, Newtonsoft.Json, and Unity assemblies without copying them. That is the safest starting point for binary compatibility with Kingmaker's Mono runtime.

Cowboys and Demons is not a toolchain template for this project: it targets Wrath on `net481`, Harmony 2, publicized Wrath/Owlcat assemblies, BlueprintCore, and Wrath-specific asset tooling.

## Initial local reference set

Sprint 2 should resolve these files from a user-local `KingmakerInstallDir`:

```text
Kingmaker_Data/Managed/Assembly-CSharp.dll
Kingmaker_Data/Managed/Assembly-CSharp-firstpass.dll
Kingmaker_Data/Managed/Newtonsoft.Json.dll
Kingmaker_Data/Managed/UnityEngine.dll
Kingmaker_Data/Managed/UnityEngine.CoreModule.dll
Kingmaker_Data/Managed/UnityEngine.AnimationModule.dll
Kingmaker_Data/Managed/UnityEngine.AssetBundleModule.dll
Kingmaker_Data/Managed/UnityEngine.UI.dll
Kingmaker_Data/Managed/UnityModManager/UnityModManager.dll
Kingmaker_Data/Managed/UnityModManager/0Harmony12.dll
```

The exact minimal list will be reduced after the first compile. Every game/Unity/UMM reference must have `Copy Local` disabled.

No game DLL belongs in source control or a package.

## UMM and Harmony compatibility

The current publicly listed Unity Mod Manager version is 0.32.5. Historical UMM work introduced a proxy strategy because Harmony 1.2 and Harmony 2 are not directly compatible. Therefore the source API is pinned to `Harmony12`, but the actual files and assembly identities installed by UMM 0.32.5 must still be inspected and fingerprinted before runtime compatibility is claimed.

Validation must capture:

```text
UnityModManager version
UnityModManager.dll assembly version and SHA-256
0Harmony12.dll existence, assembly identity, and SHA-256
0Harmony.dll / proxy files present
UMM install method selected for Kingmaker
```

If current UMM no longer exposes a working `Harmony12` compatibility assembly for Kingmaker, the decision is revisited in a bounded compatibility sprint. We do not mix Harmony APIs casually.

## Build host

Recommended host:

```text
Windows 10/11 x64
Visual Studio 2022 Build Tools or full Visual Studio
.NET Framework 4.7 Developer Pack
MSBuild
Git
PowerShell 7 optional
```

The project should be buildable from command line after `GamePath.props` is created locally.

## Planned local path convention

A checked-in template:

```xml
<Project>
  <PropertyGroup>
    <KingmakerInstallDir>C:\Games\Pathfinder Kingmaker</KingmakerInstallDir>
  </PropertyGroup>
</Project>
```

will be copied to an ignored `GamePath.props`. The actual path is never committed.

## Required environment fingerprint

Before the first runtime package is called compatible, create `environment.json` containing:

```text
game storefront
game version displayed by UI
executable file version
Assembly-CSharp.dll SHA-256
UnityEngine.dll SHA-256
UnityModManager.dll SHA-256
Harmony compatibility assembly SHA-256
operating system
UMM version
other enabled mods
```

This distinguishes an engine bug from a mismatched installation.

## Compatibility claims deliberately not made

Sprint 1 does not claim:

- macOS or Linux support;
- controller UI support;
- compatibility with older Kingmaker branches;
- compatibility with every UMM release;
- compatibility with Call of the Wild installed simultaneously;
- Steam/GOG/Epic binary identity;
- that a Wrath blueprint GUID exists in Kingmaker.
