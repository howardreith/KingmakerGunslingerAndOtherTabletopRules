[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$outputDirectory = Join-Path $repositoryRoot "artifacts\bin\$Configuration\KingmakerGunslinger"

$requiredFiles = @(
    'KingmakerGunslinger.dll',
    'Info.json',
    'blueprints\blueprints.json',
    'blueprints\blueprints.schema.json'
)
$requiredIcons = @('gunslinger-class','firearm-proficiency','gunsmithing','grit',
    'deeds','deadeye','gunslingers-dodge','quick-clear','reload-firearm',
    'repair-firearm','overhaul-firearm','early-pistol','musket','blunderbuss',
    'rifle','revolver','lead-ball','black-powder','repair-kit')
foreach ($name in $requiredIcons) {
    $requiredFiles += "assets\icons\$name.png"
}
foreach ($relativePath in $requiredFiles) {
    $path = Join-Path $outputDirectory $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required build output is missing: $path"
    }
}

$allowedRelativePaths = @{
    'KingmakerGunslinger.dll' = $true
    'KingmakerGunslinger.pdb' = $true
    'Info.json' = $true
    'blueprints\blueprints.json' = $true
    'blueprints\blueprints.schema.json' = $true
}

$unexpected = @()
foreach ($file in Get-ChildItem -LiteralPath $outputDirectory -Recurse -File) {
    $relativePath = $file.FullName.Substring($outputDirectory.Length).TrimStart('\', '/')
    if (-not $allowedRelativePaths.ContainsKey($relativePath) -and
        $relativePath -notlike 'assets\icons\*.png') {
        $unexpected += $relativePath
    }
}
if ($unexpected.Count -gt 0) {
    throw "Unexpected files exist in build output:`n$($unexpected -join [Environment]::NewLine)"
}

$forbiddenNames = @(
    '0Harmony.dll',
    '0Harmony12.dll',
    'Assembly-CSharp.dll',
    'Assembly-CSharp-firstpass.dll',
    'Newtonsoft.Json.dll',
    'UnityEngine.dll',
    'UnityModManager.dll'
)
foreach ($name in $forbiddenNames) {
    if (Get-ChildItem -LiteralPath $outputDirectory -Recurse -File -Filter $name -ErrorAction SilentlyContinue) {
        throw "A non-project assembly was copied into build output: $name"
    }
}

Write-Host "Build output validation passed: $outputDirectory"
