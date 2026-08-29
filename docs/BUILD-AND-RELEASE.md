# Build, package, and publish

## Supported release baseline

Kingmaker Gunslinger `0.0.108` targets:

- Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b;
- Unity Mod Manager 0.32.4 in the supported 0.32.x line;
- Harmony 1.2 through `0Harmony12.dll`;
- .NET Framework 4.7;
- C# 7.3;
- Windows Release/AnyCPU.

Game, Unity, Unity Mod Manager, Harmony, Newtonsoft, compiler, save, and local
configuration files are build inputs only. They must not be committed or bundled
as extra binaries in the UMM package.

## Machine prerequisites

Install Git, GitHub CLI, Python 3, Visual Studio 2022 Build Tools with MSBuild
and the .NET desktop build workload, and the .NET Framework 4.7 targeting pack.

Authenticate GitHub CLI once:

```powershell
gh auth login
gh auth status
```

Copy the ignored game-path example:

```powershell
Copy-Item .\GamePath.props.example .\GamePath.props
```

The checked-in example already points to the usual Steam installation:

```text
C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker
```

Unity Mod Manager must already be installed for Kingmaker, including:

```text
Kingmaker_Data\Managed\UnityModManager\UnityModManager.dll
Kingmaker_Data\Managed\UnityModManager\0Harmony12.dll
```

On a fresh Windows installation, remove Mark-of-the-Web from the locally
installed UMM files before .NET Framework contract tests:

```powershell
$umm = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Kingmaker_Data\Managed\UnityModManager'
Get-ChildItem $umm -File -Recurse | Unblock-File
```

## Ordinary release build

From a clean checkout:

```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\build.ps1 `
  -Configuration Release `
  -Clean `
  -Package
```

The build runs version-aware repository validation, the complete
dependency-free test suite, production compilation, strict build-output
validation, SoundBank validation, deterministic ZIP creation, and strict UMM
package validation.

## Guarded GitHub release publisher

The 0.0.108 icon-art polish Round 2 completed source, package, and guarded
runtime qualification and is owner-approved for publication through this
workflow.

The GitHub repository's release branch is `master`. Before publishing, make
`master` the repository's GitHub default branch. The publisher blocks a default
branch mismatch unless `-AllowNonDefaultReleaseBranch` is supplied deliberately.

Create a draft release for inspection:

```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\Publish-Release.ps1
```

Publish the owner-approved release:

```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\Publish-Release.ps1 `
  -Publish `
  -ConfirmReleaseReady
```

The publisher:

1. requires a clean, fully pushed `master`;
2. verifies GitHub CLI authentication, the remote repository, visibility, and
   version metadata;
3. runs the complete build and package pipeline twice;
4. requires byte-identical package and DLL hashes across both clean builds;
5. validates the final release copy;
6. creates `SHA256SUMS.txt` and `release-manifest.json`;
7. creates and pushes annotated tag `v<Info.json Version>`;
8. creates or refreshes a GitHub release, remaining draft unless `-Publish`
   is supplied; and
9. uploads the actual UMM ZIP, checksum, and manifest.

A published release is immutable project history. The script refuses to replace
one. Any later code change requires a new version, validator entry, changelog
section, and release asset.

## End-user download test

After publication, download the named file under **Assets**. Do not download
GitHub's automatic **Source code (zip)** archive.

```powershell
gh release download v0.0.108 `
  --repo howardreith/KingmakerGunslingerAndOtherTabletopRules `
  --pattern 'KingmakerGunslinger-0.0.108-icon-art-polish-round-2.zip' `
  --dir "$env:USERPROFILE\Downloads\KingmakerGunslinger-0.0.108"
```

Drag that downloaded ZIP directly into Unity Mod Manager's Mods tab. The
installed layout must contain one `KingmakerGunslinger` directory with
`Info.json`, `KingmakerGunslinger.dll`, the approved assets, blueprints, and
documentation. The package validator rejects foreign game, UMM, Harmony,
Newtonsoft, compiler, or symbol binaries.
