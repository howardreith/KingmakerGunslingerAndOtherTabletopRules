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
if ($info.Version -ne '0.0.97') { throw "Build-Local supports only active version 0.0.97, observed $($info.Version)." }
$msbuild = Resolve-KmgMsBuild -ExplicitPath $MSBuildPath
Write-Host "MSBuild: $msbuild"
$git = Get-KmgGitState -RepositoryRoot $root
$sourceStateSha256 = Get-KmgSourceStateFingerprint -RepositoryRoot $root

if (-not $ReferenceBundleDir) {
    $cursor = [IO.DirectoryInfo]$root
    for ($depth = 0; $depth -lt 6 -and $cursor; $depth++) {
        $candidate = Join-Path $cursor.FullName 'private\extracted-references\KingmakerGunslinger-private-build-references'
        if (Test-Path -LiteralPath $candidate -PathType Container) {
            $ReferenceBundleDir = $candidate
            break
        }
        $cursor = $cursor.Parent
    }
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

$localRoot = Join-Path $root 'artifacts\local-runtime\0.0.97'
$exactRoot = Join-Path $localRoot 'exact-build'
& $python (Join-Path $root 'tools\build_mod_from_private_references.py') `
    --reference-bundle-dir $ReferenceBundleDir --dotnet $dotnet `
    --csc $csc.FullName --net47-ref-dir $net47 --output-dir $exactRoot `
    --configuration Release --git-commit $git.Commit
if ($LASTEXITCODE -ne 0) { throw "Exact-reference Release build failed with exit code $LASTEXITCODE." }
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $PSScriptRoot 'Test-SupplyItemIcons.ps1') `
    -ReferenceBundleDir $ReferenceBundleDir
if ($LASTEXITCODE -ne 0) { throw 'Focused supply-icon validation failed.' }

$buildOutput = Join-Path $root 'artifacts\bin\Release\KingmakerGunslinger'
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'blueprints') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\icons') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\icons\expanded-summoning') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\bundles') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $exactRoot 'bin\KingmakerGunslinger.dll') -Destination $buildOutput -Force
Copy-Item -LiteralPath (Join-Path $exactRoot 'bin\KingmakerGunslinger.pdb') -Destination $buildOutput -Force
Copy-Item -LiteralPath (Join-Path $root 'Info.json') -Destination $buildOutput -Force
Copy-Item -LiteralPath (Join-Path $root 'blueprints\blueprints.json') -Destination (Join-Path $buildOutput 'blueprints') -Force
Copy-Item -LiteralPath (Join-Path $root 'blueprints\blueprints.schema.json') -Destination (Join-Path $buildOutput 'blueprints') -Force
Copy-Item -Path (Join-Path $root 'assets\game\icons\*.png') -Destination (Join-Path $buildOutput 'assets\icons') -Force
Copy-Item -Path (Join-Path $root 'assets\game\icons\expanded-summoning\*') -Destination (Join-Path $buildOutput 'assets\icons\expanded-summoning') -Force
$bundleManifest = Get-Content -LiteralPath (Join-Path $root 'assets\bundles\asset-bundle-manifest.json') -Raw | ConvertFrom-Json
$bundleSource = 'C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1\Builds\Windows\kingmakergunslinger.firearms'
if (-not (Test-Path -LiteralPath $bundleSource -PathType Leaf)) { throw "Qualified firearm AssetBundle is missing: $bundleSource" }
if ((Get-KmgSha256 -Path $bundleSource) -ne $bundleManifest.sha256) { throw 'Firearm AssetBundle hash does not match the qualified manifest.' }
Copy-Item -LiteralPath $bundleSource -Destination (Join-Path $buildOutput 'assets\bundles\kingmakergunslinger.firearms') -Force
$spearBundle = Join-Path $root 'assets\bundles\kingmakergunslinger.elvenbranchedspear'
$easternBundle = Join-Path $root 'assets\bundles\kingmakergunslinger.easternweapons'
$spearManifest = @($bundleManifest.bundles | Where-Object { $_.name -ceq 'kingmakergunslinger.elvenbranchedspear' })
$easternManifest = @($bundleManifest.bundles | Where-Object { $_.name -ceq 'kingmakergunslinger.easternweapons' })
if ($spearManifest.Count -ne 1 -or $easternManifest.Count -ne 1 -or
    (Get-KmgSha256 -Path $spearBundle) -ne $spearManifest[0].sha256 -or
    (Get-KmgSha256 -Path $easternBundle) -ne $easternManifest[0].sha256) {
    throw 'Original custom-weapon bundle hash does not match the qualified manifest.'
}
Copy-Item -LiteralPath $spearBundle -Destination (Join-Path $buildOutput 'assets\bundles') -Force
Copy-Item -LiteralPath $easternBundle -Destination (Join-Path $buildOutput 'assets\bundles') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\bundles\asset-bundle-manifest.json') -Destination (Join-Path $buildOutput 'assets\bundles') -Force
& (Join-Path $PSScriptRoot 'validate-build-output.ps1') -Configuration Release
& (Join-Path $PSScriptRoot 'package.ps1') -Configuration Release

$packagePath = Join-Path $localRoot "$($info.Id)-$($info.Version)-local-runtime.zip"
New-Item -ItemType Directory -Path $localRoot -Force | Out-Null
$stagedMod = Join-Path $root 'artifacts\staging\install\KingmakerGunslinger'
$hasFirearmSoundBank = Test-Path -LiteralPath (Join-Path $stagedMod 'assets\soundbanks\KMG_Firearms.bnk') -PathType Leaf
$expectedPackageFileCount = if ($hasFirearmSoundBank) { 137 } else { 135 }
& $python (Join-Path $root 'tools\create_deterministic_package.py') --source $stagedMod --output $packagePath --expected-file-count $expectedPackageFileCount
if ($LASTEXITCODE -ne 0) { throw 'Deterministic package creation failed.' }
& (Join-Path $PSScriptRoot 'validate-package.ps1') `
    -PackagePath $packagePath -Configuration Release

$dllPath = Join-Path $buildOutput 'KingmakerGunslinger.dll'
$firearmManifestPath = Join-Path $stagedMod `
    'assets\soundbanks\firearm-soundbank-manifest.json'
$firearmSoundBankPath = Join-Path $stagedMod `
    'assets\soundbanks\KMG_Firearms.bnk'
$manifest = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    generator = 'scripts/Build-Local.ps1'
    repositoryRoot = $root
    commit = $git.Commit
    branch = $git.Branch
    sourceStateSha256 = $sourceStateSha256
    version = $info.Version
    packagePath = $packagePath
    packageSha256 = Get-KmgSha256 -Path $packagePath
    dllSha256 = Get-KmgSha256 -Path $dllPath
    dllMvid = Get-KmgDllMvid -Path $dllPath
    firearmManifestSha256 = Get-KmgSha256 -Path $firearmManifestPath
    firearmSoundBankSha256 = Get-KmgSha256 -Path $firearmSoundBankPath
    validated = $true
}
$manifestPath = Get-KmgPackageManifestPath -PackagePath $packagePath
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
Write-Host "Package: $packagePath"
Write-Host "Version: $($info.Version)"
Write-Host "Package SHA-256: $($manifest.packageSha256)"
Write-Host "DLL SHA-256: $($manifest.dllSha256)"
Write-Host 'No deployment was performed.'
