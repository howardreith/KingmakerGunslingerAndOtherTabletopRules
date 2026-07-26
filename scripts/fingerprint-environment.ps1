[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$KingmakerInstallDir,

    [Parameter(Mandatory = $true)]
    [string]$Storefront,

    [Parameter(Mandatory = $true)]
    [string]$DisplayedGameVersion,

    [string]$UnityModManagerVersion = '0.32.5',

    [string[]]$EnabledMods = @(),

    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
if (-not $OutputPath) {
    $OutputPath = Join-Path $repositoryRoot 'environment.json'
}

if (-not (Test-Path -LiteralPath $KingmakerInstallDir -PathType Container)) {
    throw "KingmakerInstallDir does not exist: $KingmakerInstallDir"
}
$KingmakerInstallDir = (Resolve-Path -LiteralPath $KingmakerInstallDir).Path
$managedDirectory = Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed'
$executablePath = Join-Path $KingmakerInstallDir 'Kingmaker.exe'

$relativeAssemblyPaths = @(
    'Assembly-CSharp.dll',
    'Assembly-CSharp-firstpass.dll',
    'Newtonsoft.Json.dll',
    'UnityEngine.dll',
    'UnityEngine.AnimationModule.dll',
    'UnityEngine.AssetBundleModule.dll',
    'UnityEngine.CoreModule.dll',
    'UnityEngine.UI.dll',
    'UnityModManager\UnityModManager.dll',
    'UnityModManager\0Harmony12.dll',
    'UnityModManager\0Harmony.dll'
)

$assemblies = @()
foreach ($relativePath in $relativeAssemblyPaths) {
    $path = Join-Path $managedDirectory $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        continue
    }

    $assemblyIdentity = $null
    try {
        $assemblyIdentity = [Reflection.AssemblyName]::GetAssemblyName($path).FullName
    }
    catch {
        $assemblyIdentity = $null
    }

    $fileVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($path).FileVersion
    $assemblies += [ordered]@{
        relativePath = $relativePath.Replace('\', '/')
        assemblyIdentity = $assemblyIdentity
        fileVersion = $fileVersion
        sizeBytes = (Get-Item -LiteralPath $path).Length
        sha256 = Get-KmgSha256 -Path $path
    }
}

$executableVersion = $null
if (Test-Path -LiteralPath $executablePath -PathType Leaf) {
    $executableVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($executablePath).FileVersion
}

$fingerprint = [ordered]@{
    capturedAtUtc = [DateTime]::UtcNow.ToString('o')
    game = [ordered]@{
        installDirectory = $KingmakerInstallDir
        storefront = $Storefront
        displayedVersion = $DisplayedGameVersion
        executableFileVersion = $executableVersion
        unityModManagerVersion = $UnityModManagerVersion
    }
    host = [ordered]@{
        operatingSystem = [Environment]::OSVersion.VersionString
        processArchitecture = $env:PROCESSOR_ARCHITECTURE
        powerShellVersion = $PSVersionTable.PSVersion.ToString()
    }
    assemblies = $assemblies
    enabledMods = @($EnabledMods)
}

$fingerprint | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
Write-Host "Wrote environment fingerprint: $OutputPath"
