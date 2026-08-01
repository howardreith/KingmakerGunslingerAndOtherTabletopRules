[CmdletBinding()]
param(
    [string]$MSBuildPath,
    [string]$ReferenceBundleDir,
    [string]$KingmakerInstallDir = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'ReferenceProvenance.Common.ps1')

$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$info = Get-KmgModInfo -RepositoryRoot $root
if ($info.Version -ne '0.0.35') { throw "Build-Local supports only active version 0.0.35, observed $($info.Version)." }
$msbuild = Resolve-KmgMsBuild -ExplicitPath $MSBuildPath
Write-Host "MSBuild: $msbuild"
$git = Get-KmgGitState -RepositoryRoot $root

if (-not $ReferenceBundleDir) {
    $labRoot = [IO.Path]::GetFullPath((Join-Path $root '..\..'))
    $ReferenceBundleDir = Join-Path $labRoot 'private\extracted-references\KingmakerGunslinger-private-build-references'
}
if (-not (Test-Path -LiteralPath $ReferenceBundleDir -PathType Container)) {
    throw "Qualified private reference bundle is missing: $ReferenceBundleDir"
}
[void](Assert-KmgReferenceBundleMatchesInstall `
    -ReferenceBundleDir $ReferenceBundleDir `
    -KingmakerInstallDir $KingmakerInstallDir)

$python = (Get-Command python -ErrorAction Stop).Source
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$dotnetRoot = Split-Path $dotnet
$csc = Get-ChildItem -LiteralPath (Join-Path $dotnetRoot 'sdk') -Filter csc.dll -Recurse -File |
    Where-Object { $_.FullName -like '*\Roslyn\bincore\csc.dll' } |
    Sort-Object FullName -Descending | Select-Object -First 1
if (-not $csc) { throw 'Roslyn csc.dll was not found beneath the installed dotnet SDK.' }
$net47 = 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7'
if (-not (Test-Path -LiteralPath (Join-Path $net47 'mscorlib.dll') -PathType Leaf)) {
    throw ".NET Framework 4.7 reference assemblies are missing: $net47"
}

& (Join-Path $PSScriptRoot 'validate-repository.ps1')
& (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release -Clean -MSBuildPath $msbuild

$localRoot = Join-Path $root 'artifacts\local-runtime\0.0.35'
$exactRoot = Join-Path $localRoot 'exact-build'
& $python (Join-Path $root 'tools\build_mod_from_private_references.py') `
    --reference-bundle-dir $ReferenceBundleDir --dotnet $dotnet `
    --csc $csc.FullName --net47-ref-dir $net47 --output-dir $exactRoot `
    --configuration Release --git-commit $git.Commit
if ($LASTEXITCODE -ne 0) { throw "Exact-reference Release build failed with exit code $LASTEXITCODE." }

$buildOutput = Join-Path $root 'artifacts\bin\Release\KingmakerGunslinger'
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'blueprints') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $exactRoot 'bin\KingmakerGunslinger.dll') -Destination $buildOutput -Force
Copy-Item -LiteralPath (Join-Path $exactRoot 'bin\KingmakerGunslinger.pdb') -Destination $buildOutput -Force
Copy-Item -LiteralPath (Join-Path $root 'Info.json') -Destination $buildOutput -Force
Copy-Item -LiteralPath (Join-Path $root 'blueprints\blueprints.json') -Destination (Join-Path $buildOutput 'blueprints') -Force
Copy-Item -LiteralPath (Join-Path $root 'blueprints\blueprints.schema.json') -Destination (Join-Path $buildOutput 'blueprints') -Force
& (Join-Path $PSScriptRoot 'validate-build-output.ps1') -Configuration Release
& (Join-Path $PSScriptRoot 'package.ps1') -Configuration Release

$packagePath = Join-Path $localRoot "$($info.Id)-$($info.Version)-local-runtime.zip"
New-Item -ItemType Directory -Path $localRoot -Force | Out-Null
$stagedMod = Join-Path $root 'artifacts\staging\install\KingmakerGunslinger'
& $python (Join-Path $root 'tools\create_deterministic_package.py') --source $stagedMod --output $packagePath
if ($LASTEXITCODE -ne 0) { throw 'Deterministic package creation failed.' }
& (Join-Path $PSScriptRoot 'validate-package.ps1') -PackagePath $packagePath

$dllPath = Join-Path $buildOutput 'KingmakerGunslinger.dll'
$manifest = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    generator = 'scripts/Build-Local.ps1'
    repositoryRoot = $root
    commit = $git.Commit
    branch = $git.Branch
    version = $info.Version
    packagePath = $packagePath
    packageSha256 = Get-KmgSha256 -Path $packagePath
    dllSha256 = Get-KmgSha256 -Path $dllPath
    validated = $true
}
$manifestPath = Get-KmgPackageManifestPath -PackagePath $packagePath
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
Write-Host "Package: $packagePath"
Write-Host "Version: $($info.Version)"
Write-Host "Package SHA-256: $($manifest.packageSha256)"
Write-Host "DLL SHA-256: $($manifest.dllSha256)"
Write-Host 'No deployment was performed.'
