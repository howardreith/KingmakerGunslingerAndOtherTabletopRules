# Private runtime-reference handoff

## Purpose

The remaining compile boundary is the exact Kingmaker and Unity Mod Manager assembly set from the installation that will run the mod. Sprint 17 includes a narrow exporter so those references can be supplied privately without copying the game executable, saves, account data, settings, screenshots, or logs.

The resulting archive is private build input. It must not be published or included in a mod release.

## Export command

Run from the Sprint 17 repository root in Windows PowerShell:

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\scripts\export-private-build-references.ps1 `
  -KingmakerInstallDir 'C:\Path\To\Pathfinder Kingmaker' `
  -Storefront Steam `
  -DisplayedGameVersion 2.1.7b
```

The default output is:

```text
artifacts\private-references\KingmakerGunslinger-private-build-references.zip
artifacts\private-references\KingmakerGunslinger-private-build-references.zip.sha256
```

## Included files

The archive contains only the ten managed assemblies required by the current project:

```text
Managed/Assembly-CSharp.dll
Managed/Assembly-CSharp-firstpass.dll
Managed/Newtonsoft.Json.dll
Managed/UnityEngine.dll
Managed/UnityEngine.AnimationModule.dll
Managed/UnityEngine.AssetBundleModule.dll
Managed/UnityEngine.CoreModule.dll
Managed/UnityEngine.UI.dll
Managed/UnityModManager/UnityModManager.dll
Managed/UnityModManager/0Harmony12.dll
```

It also contains:

```text
reference-manifest.json
PRIVATE-NOT-FOR-REDISTRIBUTION.txt
```

The manifest records assembly identities, file versions, sizes, and SHA-256 hashes. It records a hash and file version for `Kingmaker.exe`, but does not copy the executable or disclose the installation path.

## What the handoff enables

With that private archive, the source can be compiled against the exact game/UMM contracts rather than substitutes. Sprint 17 includes `tools/build_mod_from_private_references.py` for a cross-platform Roslyn compile and package path.

A successful compile produces a **compile candidate**, not a persistence pass. The candidate still needs to be installed through UMM and exercised inside Kingmaker before any runtime or save-persistence claim is made.

## Privacy and redistribution boundary

The private reference archive:

- Must remain private.
- Must not be attached to a public GitHub release or Nexus upload.
- Must never be nested inside the UMM mod ZIP.
- Must be deleted from public build staging after compilation.
- Contains no user saves or account credentials by design.
