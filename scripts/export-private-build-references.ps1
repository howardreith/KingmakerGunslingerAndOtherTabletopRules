[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$KingmakerInstallDir,

    [string]$Storefront = 'Unspecified',

    [string]$DisplayedGameVersion = 'Unspecified',

    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot

if (-not (Test-Path -LiteralPath $KingmakerInstallDir -PathType Container)) {
    throw "KingmakerInstallDir does not exist: $KingmakerInstallDir"
}
$KingmakerInstallDir = (Resolve-Path -LiteralPath $KingmakerInstallDir).Path
$managedDirectory = Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed'
$executablePath = Join-Path $KingmakerInstallDir 'Kingmaker.exe'
if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
    throw "Kingmaker.exe was not found under the supplied installation: $KingmakerInstallDir"
}

if (-not $OutputPath) {
    $OutputPath = Join-Path $repositoryRoot 'artifacts\private-references\KingmakerGunslinger-private-build-references.zip'
}
$OutputPath = [IO.Path]::GetFullPath($OutputPath)
$outputDirectory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

$required = @(
    'Assembly-CSharp.dll',
    'Assembly-CSharp-firstpass.dll',
    'Newtonsoft.Json.dll',
    'UnityEngine.dll',
    'UnityEngine.AnimationModule.dll',
    'UnityEngine.AssetBundleModule.dll',
    'UnityEngine.CoreModule.dll',
    'UnityEngine.UI.dll',
    'UnityModManager\UnityModManager.dll',
    'UnityModManager\0Harmony12.dll'
)

$missing = @()
foreach ($relativePath in $required) {
    $sourcePath = Join-Path $managedDirectory $relativePath
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        $missing += $relativePath
    }
}
if ($missing.Count -gt 0) {
    throw "Required private build references are missing: $($missing -join ', ')"
}

$stagingParent = Join-Path ([IO.Path]::GetTempPath()) ('kmg-private-references-' + [Guid]::NewGuid().ToString('N'))
$bundleRoot = Join-Path $stagingParent 'KingmakerGunslinger-private-build-references'
$managedRoot = Join-Path $bundleRoot 'Managed'
New-Item -ItemType Directory -Path $managedRoot -Force | Out-Null

try {
    $records = @()
    foreach ($relativePath in $required) {
        $sourcePath = Join-Path $managedDirectory $relativePath
        $destinationPath = Join-Path $managedRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $destinationPath) -Force | Out-Null
        Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force

        $assemblyIdentity = $null
        try {
            $assemblyIdentity = [Reflection.AssemblyName]::GetAssemblyName($sourcePath).FullName
        }
        catch {
            $assemblyIdentity = $null
        }

        $records += [ordered]@{
            relativePath = ('Managed/' + $relativePath.Replace('\', '/'))
            sizeBytes = (Get-Item -LiteralPath $sourcePath).Length
            sha256 = Get-KmgSha256 -Path $sourcePath
            assemblyIdentity = $assemblyIdentity
            fileVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($sourcePath).FileVersion
        }
    }

    $manifest = [ordered]@{
        schemaVersion = 1
        createdAtUtc = [DateTime]::UtcNow.ToString('o')
        purpose = 'Private compile-time references for Kingmaker Gunslinger. Do not redistribute.'
        containsGameExecutable = $false
        containsSavesOrUserData = $false
        installPathIncluded = $false
        game = [ordered]@{
            storefront = $Storefront
            displayedVersion = $DisplayedGameVersion
            executableFileVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($executablePath).FileVersion
            executableSha256 = Get-KmgSha256 -Path $executablePath
        }
        files = $records
    }
    $manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $bundleRoot 'reference-manifest.json') -Encoding UTF8

    @'
PRIVATE BUILD REFERENCES — DO NOT REDISTRIBUTE

This archive contains managed assemblies from a locally installed copy of Pathfinder: Kingmaker
and Unity Mod Manager. It exists only to compile and inspect Kingmaker Gunslinger against the exact
runtime selected by the owner of that installation.

It contains no saves, configuration files, account credentials, screenshots, logs, or game executable.
Do not publish this archive or include its DLLs in the mod's source or release packages.
'@ | Set-Content -LiteralPath (Join-Path $bundleRoot 'PRIVATE-NOT-FOR-REDISTRIBUTION.txt') -Encoding UTF8

    if (Test-Path -LiteralPath $OutputPath) {
        Remove-Item -LiteralPath $OutputPath -Force
    }
    Compress-Archive -LiteralPath $bundleRoot -DestinationPath $OutputPath -CompressionLevel Optimal

    $checksumPath = "$OutputPath.sha256"
    Set-Content -LiteralPath $checksumPath `
        -Value "$(Get-KmgSha256 -Path $OutputPath)  $([IO.Path]::GetFileName($OutputPath))" `
        -Encoding ASCII

    Write-Host 'Created private reference bundle.'
    Write-Host "Archive:  $OutputPath"
    Write-Host "Checksum: $checksumPath"
    Write-Host 'This archive is private input for compilation and must not be redistributed.'
}
finally {
    if (Test-Path -LiteralPath $stagingParent) {
        Remove-Item -LiteralPath $stagingParent -Recurse -Force
    }
}
