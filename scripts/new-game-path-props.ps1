[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$KingmakerInstallDir,

    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$outputPath = Join-Path $repositoryRoot 'GamePath.props'

if (-not (Test-Path -LiteralPath $KingmakerInstallDir -PathType Container)) {
    throw "The supplied Kingmaker installation directory does not exist: $KingmakerInstallDir"
}

$KingmakerInstallDir = (Resolve-Path -LiteralPath $KingmakerInstallDir).Path
$assemblyPath = Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed\Assembly-CSharp.dll'
if (-not (Test-Path -LiteralPath $assemblyPath -PathType Leaf)) {
    throw "Assembly-CSharp.dll was not found beneath the supplied directory: $assemblyPath"
}

if ((Test-Path -LiteralPath $outputPath) -and -not $Force) {
    throw "GamePath.props already exists. Pass -Force to replace it: $outputPath"
}

$escapedPath = [Security.SecurityElement]::Escape($KingmakerInstallDir)
$content = @"
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <KingmakerInstallDir>$escapedPath</KingmakerInstallDir>
  </PropertyGroup>
</Project>
"@

Set-Content -LiteralPath $outputPath -Value $content -Encoding UTF8
Write-Host "Created local build configuration: $outputPath"
