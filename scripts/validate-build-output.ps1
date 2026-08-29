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
    'blueprints\blueprints.schema.json',
    'assets\bundles\kingmakergunslinger.firearms',
    'assets\bundles\kingmakergunslinger.elvenbranchedspear',
    'assets\bundles\kingmakergunslinger.easternweapons',
    'assets\bundles\asset-bundle-manifest.json'
)
$requiredIcons = @('gunslinger-class','firearm-proficiency','gunsmithing','grit',
    'deeds','nimble','bonus-feat','gun-training','true-grit','rapid-reload',
    'weapon-focus-firearm','deadeye','gunslingers-dodge','quick-clear','reload-firearm',
    'firearm-monogram-pistol','firearm-monogram-musket',
    'firearm-monogram-blunderbuss',
    'repair-firearm','overhaul-firearm','early-pistol','musket','blunderbuss',
    'rifle','revolver','lead-ball','black-powder','repair-kit',
    'gunsmith-kit','overhaul-kit','wakizashi','katana','nodachi',
    'night-without-moon','heavens-measure','world-tree-severer')
foreach ($name in $requiredIcons) {
    $requiredFiles += "assets\icons\$name.png"
}
foreach ($name in @('firearm-monogram-rifle','firearm-monogram-revolver')) {
    $retiredPath = Join-Path $outputDirectory "assets\icons\$name.png"
    if (Test-Path -LiteralPath $retiredPath) {
        throw "Retired player-facing selector exists in build output: $retiredPath"
    }
}
$summonManifest = Get-Content -LiteralPath (Join-Path $repositoryRoot `
    'assets\game\icons\expanded-summoning\icon-manifest.json') -Raw | ConvertFrom-Json
if ($summonManifest.count -ne 77 -or @($summonManifest.icons).Count -ne 77) {
    throw 'Expanded Summoning runtime icon manifest is malformed.'
}
$requiredFiles += 'assets\icons\expanded-summoning\icon-manifest.json'
foreach ($icon in $summonManifest.icons) {
    $requiredFiles += "assets\icons\expanded-summoning\$($icon.file)"
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
    'assets\bundles\kingmakergunslinger.firearms' = $true
    'assets\bundles\kingmakergunslinger.elvenbranchedspear' = $true
    'assets\bundles\kingmakergunslinger.easternweapons' = $true
    'assets\bundles\asset-bundle-manifest.json' = $true
}

$unexpected = @()
foreach ($file in Get-ChildItem -LiteralPath $outputDirectory -Recurse -File) {
    $relativePath = $file.FullName.Substring($outputDirectory.Length).TrimStart('\', '/')
    if (-not $allowedRelativePaths.ContainsKey($relativePath) -and
        $relativePath -notlike 'assets\icons\*.png' -and
        $relativePath -notlike 'assets\icons\expanded-summoning\*.png' -and
        $relativePath -ne 'assets\icons\expanded-summoning\icon-manifest.json') {
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
