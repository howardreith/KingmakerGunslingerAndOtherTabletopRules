[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$info = Get-KmgModInfo -RepositoryRoot $repositoryRoot
$packagesDirectory = Join-Path $repositoryRoot 'artifacts\packages'
$stagingDirectory = Join-Path $repositoryRoot 'artifacts\staging\source'
$sourceRoot = Join-Path $stagingDirectory 'KingmakerGunslinger'
$packagePath = Join-Path $packagesDirectory "KingmakerGunslinger-source-$($info.Version).zip"

& (Join-Path $PSScriptRoot 'validate-repository.ps1')

if (Test-Path -LiteralPath $stagingDirectory) {
    Remove-Item -LiteralPath $stagingDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path $sourceRoot -Force | Out-Null
New-Item -ItemType Directory -Path $packagesDirectory -Force | Out-Null

$excludedRootNames = @('.git', '.vs', '.idea', 'artifacts', '__pycache__')
$excludedFileNames = @('GamePath.props', 'environment.json', 'runtime-contracts.json', 'source.zip', 'SHA256SUMS.txt', 'manifest.json', 'Assembly-CSharp.dll', 'Assembly-CSharp-firstpass.dll', 'UnityModManager.dll', '0Harmony12.dll', 'Newtonsoft.Json.dll')

foreach ($file in Get-ChildItem -LiteralPath $repositoryRoot -Recurse -File) {
    $relativePath = $file.FullName.Substring($repositoryRoot.Length).TrimStart('\', '/')
    $segments = $relativePath -split '[\\/]'
    if ($segments[0] -in $excludedRootNames) {
        continue
    }
    if ($file.Name -in $excludedFileNames) {
        continue
    }
    if ($file.Extension -in @('.dll', '.exe', '.pdb', '.mdb', '.pyc')) {
        continue
    }

    $destination = Join-Path $sourceRoot $relativePath
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $destination
}

if (Test-Path -LiteralPath $packagePath) {
    Remove-Item -LiteralPath $packagePath -Force
}
Compress-Archive -LiteralPath $sourceRoot -DestinationPath $packagePath -CompressionLevel Optimal
Write-Host "Created source package: $packagePath"
