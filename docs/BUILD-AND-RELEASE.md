# Build, package, and publish

## Supported release baseline

Kingmaker Gunslinger `0.0.119-contextual-world-map-teleportation` targets:

- Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b;
- Unity Mod Manager 0.32.4 API contracts; guarded 0.0.119 runtime testing uses
  UMM 0.33.0.0, with 0.32.4 runtime acceptance deferred to owner testing;
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

## Current 0.0.119 publication authorization

The owner subsequently instructed: "You can forego the remaining tests. Please
wrap things up, commit, push to origin, and cut the release." This supersedes
the remaining native-repeat requirements in the original plan below. Preserve
completed results with their exact artifact identities; do not claim the sealed
release binary passed runtime tests that were waived. The existing publisher's
built-in deterministic build/package/provenance checks remain in use.

## Original 0.0.119 release gate plan

The owner authorized a normal public patch from the dedicated repair branch,
without merging or modifying master. Use the existing provenance-checked
`Publish-Release.ps1` with `-ReleaseBranch codex/teleportation-post-release-hardening`
and `-AllowNonDefaultReleaseBranch` only after the same clean committed artifact
passes all 45 required fresh-process launches and the original-configuration
restoration startup. Build twice with the exact reference bundle and compare
ZIP/DLL bytes; the publisher repeats both builds. Recheck public releases before
publication, preserving all historical tags/assets. Download the public ZIP,
checksums and manifest again, independently compare them with the runtime-tested
artifact, validate the strict package and install that exact public ZIP while
preserving existing settings. Curated final evidence belongs in the hardening
report; raw saves/packages/runtime artifacts remain ignored.

## Historical release authorizations

The public master 0.0.115 Share Transmutation fix is incorporated into this
branch. Its authorized publication and its NOT-RUN save-backed gameplay
record remain distinct from this mission's unpublished release checkpoints.

The owner explicitly authorized finalizing, merging, pushing, and publishing
0.0.116. The accepted content candidate passed the complete domain suite,
24-state module boundary matrix, and guarded shop, firearm, and purchase
save/load scenarios. Release sealing reuses that unchanged-source evidence and
checks the rebuilt release artifact through the guarded runtime workflow.
Exact fingerprints and results belong in the task report. Historical 0.0.114
compatibility evidence stays pinned to that package; 0.0.115's separate
save-backed API qualification is not inferred from these content scenarios.

The owner accepted the installed 0.0.117 stabilization candidate and explicitly
authorized finalization, integration with latest master, merge, push and public
release on 2026-09-07. This supersedes the earlier candidate-only boundary.
The prior acceptance handoff and incremental notes remain historical evidence;
the [public release report](ELEMENTAL-RACES-0.0.117-PUBLIC-RELEASE.md) records the
new authorization and final artifact verification.

Where installed UMM targets a newer framework, pass `-ReferenceBundleDir` to
the publisher to use the existing provenance-checked `Build-Local.ps1` path
for both clean deterministic builds. It runs the same source, full domain,
output, SoundBank, and strict package gates. No installed UMM/Harmony files
are changed. If the legacy external Unity output is absent, Build-Local uses
the tracked bundle only after the existing manifest hash check succeeds.

Release 0.0.117 preserves all existing elemental identities and publishes only
nineteen implemented alternate traits. Treacherous Earth and Nereid Fascination
remain registered but unavailable. The accepted artifact passed 1,458 tests,
259 harness preflight checks and 13,847 assertions across 28 guarded processes.
Release sealing preserves gameplay source and assets, compares the new payload
against that accepted artifact, and verifies the rebuilt DLL's final commit
through focused guarded character-creation, native respec and load checks.
Earlier matrix evidence retains its original artifact attribution. The owner's
acceptance does not fabricate individual full-screen checklist results.

The GitHub repository's release branch is `master`. Before publishing, make
`master` the repository's GitHub default branch. The publisher blocks a default
branch mismatch unless `-AllowNonDefaultReleaseBranch` is supplied deliberately.

After explicit owner authorization, create a draft release for inspection:

```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\Publish-Release.ps1
```

Publish only the owner-approved release:

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
gh release download v0.0.117 `
  --repo howardreith/KingmakerGunslingerAndOtherTabletopRules `
  --pattern 'KingmakerGunslinger-0.0.117-elemental-char-gen-stabilization.zip' `
  --dir "artifacts\release-download\0.0.117"
```

Drag that downloaded ZIP directly into Unity Mod Manager's Mods tab. The
installed layout must contain one `KingmakerGunslinger` directory with
`Info.json`, `KingmakerGunslinger.dll`, the approved assets, blueprints, and
documentation. The package validator rejects foreign game, UMM, Harmony,
Newtonsoft, compiler, or symbol binaries.
