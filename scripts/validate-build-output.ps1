[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
. (Join-Path $PSScriptRoot 'IconCatalog.Common.ps1')
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
    'assets\bundles\asset-bundle-manifest.json',
    # The Pteranodon replacement visual ships as mesh data plus its painted
    # albedo rather than an AssetBundle: no bind poses, no material, no
    # editor dependency.
    'assets\pteranodon\pteranodon-mesh.json',
    'assets\pteranodon\pteranodon-albedo.png',
    'assets\flying-animals\dire-bat-mesh.json',
    'assets\flying-animals\dire-bat-albedo.png',
    'assets\flying-animals\eagle-mesh.json',
    'assets\flying-animals\eagle-albedo.png',
    'assets\flying-animals\giant-wasp-mesh.json',
    'assets\flying-animals\giant-wasp-albedo.png',
    'assets\flying-animals\stirge-mesh.json',
    'assets\flying-animals\stirge-albedo.png',
    'assets\ungulates\aurochs-mesh.json',
    'assets\ungulates\aurochs-albedo.png',
    'assets\ungulates\bison-mesh.json',
    'assets\ungulates\bison-albedo.png',
    'assets\ungulates\rhinoceros-mesh.json',
    'assets\ungulates\rhinoceros-albedo.png',
    'assets\ungulates\woolly-rhinoceros-mesh.json',
    'assets\ungulates\woolly-rhinoceros-albedo.png',
    'assets\sprint12-quadrupeds\dire-rat-mesh.json',
    'assets\sprint12-quadrupeds\dire-rat-albedo.png',
    'assets\sprint12-quadrupeds\hyena-mesh.json',
    'assets\sprint12-quadrupeds\hyena-albedo.png',
    'assets\sprint12-quadrupeds\goblin-dog-mesh.json',
    'assets\sprint12-quadrupeds\goblin-dog-albedo.png',
    'assets\sprint13-creatures\wolverine-mesh.json',
    'assets\sprint13-creatures\wolverine-albedo.png',
    'assets\sprint13-creatures\shadow-mastiff-mesh.json',
    'assets\sprint13-creatures\shadow-mastiff-albedo.png',
    'assets\sprint13-creatures\poisonous-frog-mesh.json',
    'assets\sprint13-creatures\poisonous-frog-albedo.png',
    'assets\sprint14-insects\fire-beetle-mesh.json',
    'assets\sprint14-insects\fire-beetle-albedo.png',
    'assets\sprint14-insects\giant-ant-worker-mesh.json',
    'assets\sprint14-insects\giant-ant-worker-albedo.png',
    'assets\sprint14-insects\giant-ant-soldier-mesh.json',
    'assets\sprint14-insects\giant-ant-soldier-albedo.png',
    'assets\sprint14-insects\giant-ant-drone-mesh.json',
    'assets\sprint14-insects\giant-ant-drone-albedo.png',
    'assets\sprint14-insects\giant-stag-beetle-mesh.json',
    'assets\sprint14-insects\giant-stag-beetle-albedo.png',
    'assets\sprint16-crocodilians\crocodile-mesh.json',
    'assets\sprint16-crocodilians\crocodile-albedo.png',
    'assets\sprint16-crocodilians\dire-crocodile-mesh.json',
    'assets\sprint16-crocodilians\dire-crocodile-albedo.png',
    'assets\sprint17-serpents\viper-mesh.json',
    'assets\sprint17-serpents\viper-albedo.png',
    'assets\sprint17-serpents\constrictor-snake-mesh.json',
    'assets\sprint17-serpents\constrictor-snake-albedo.png',
    'assets\sprint17-serpents\salamander-mesh.json',
    'assets\sprint17-serpents\salamander-human-mesh.json',
    'assets\sprint17-serpents\salamander-albedo.png'
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
$integratedIcons = @(Get-KmgIntegratedIconRecords -RepositoryRoot $repositoryRoot)
$requiredIcons = @($requiredIcons + @($integratedIcons | ForEach-Object { $_.Key }) | Select-Object -Unique)
Assert-KmgIntegratedIconFiles -ModDirectory $outputDirectory -Records $integratedIcons
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
if ($summonManifest.count -ne 111 -or @($summonManifest.icons).Count -ne 111) {
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
    'assets\pteranodon\pteranodon-mesh.json' = $true
    'assets\pteranodon\pteranodon-albedo.png' = $true
    'assets\flying-animals\dire-bat-mesh.json' = $true
    'assets\flying-animals\dire-bat-albedo.png' = $true
    'assets\flying-animals\eagle-mesh.json' = $true
    'assets\flying-animals\eagle-albedo.png' = $true
    'assets\flying-animals\giant-wasp-mesh.json' = $true
    'assets\flying-animals\giant-wasp-albedo.png' = $true
    'assets\flying-animals\stirge-mesh.json' = $true
    'assets\flying-animals\stirge-albedo.png' = $true
    'assets\ungulates\aurochs-mesh.json' = $true
    'assets\ungulates\aurochs-albedo.png' = $true
    'assets\ungulates\bison-mesh.json' = $true
    'assets\ungulates\bison-albedo.png' = $true
    'assets\ungulates\rhinoceros-mesh.json' = $true
    'assets\ungulates\rhinoceros-albedo.png' = $true
    'assets\ungulates\woolly-rhinoceros-mesh.json' = $true
    'assets\ungulates\woolly-rhinoceros-albedo.png' = $true
    'assets\sprint12-quadrupeds\dire-rat-mesh.json' = $true
    'assets\sprint12-quadrupeds\dire-rat-albedo.png' = $true
    'assets\sprint12-quadrupeds\hyena-mesh.json' = $true
    'assets\sprint12-quadrupeds\hyena-albedo.png' = $true
    'assets\sprint12-quadrupeds\goblin-dog-mesh.json' = $true
    'assets\sprint12-quadrupeds\goblin-dog-albedo.png' = $true
    'assets\sprint13-creatures\wolverine-mesh.json' = $true
    'assets\sprint13-creatures\wolverine-albedo.png' = $true
    'assets\sprint13-creatures\shadow-mastiff-mesh.json' = $true
    'assets\sprint13-creatures\shadow-mastiff-albedo.png' = $true
    'assets\sprint13-creatures\poisonous-frog-mesh.json' = $true
    'assets\sprint13-creatures\poisonous-frog-albedo.png' = $true
    'assets\sprint14-insects\fire-beetle-mesh.json' = $true
    'assets\sprint14-insects\fire-beetle-albedo.png' = $true
    'assets\sprint14-insects\giant-ant-worker-mesh.json' = $true
    'assets\sprint14-insects\giant-ant-worker-albedo.png' = $true
    'assets\sprint14-insects\giant-ant-soldier-mesh.json' = $true
    'assets\sprint14-insects\giant-ant-soldier-albedo.png' = $true
    'assets\sprint14-insects\giant-ant-drone-mesh.json' = $true
    'assets\sprint14-insects\giant-ant-drone-albedo.png' = $true
    'assets\sprint14-insects\giant-stag-beetle-mesh.json' = $true
    'assets\sprint14-insects\giant-stag-beetle-albedo.png' = $true
    'assets\sprint16-crocodilians\crocodile-mesh.json' = $true
    'assets\sprint16-crocodilians\crocodile-albedo.png' = $true
    'assets\sprint16-crocodilians\dire-crocodile-mesh.json' = $true
    'assets\sprint16-crocodilians\dire-crocodile-albedo.png' = $true
    'assets\sprint17-serpents\viper-mesh.json' = $true
    'assets\sprint17-serpents\viper-albedo.png' = $true
    'assets\sprint17-serpents\constrictor-snake-mesh.json' = $true
    'assets\sprint17-serpents\constrictor-snake-albedo.png' = $true
    'assets\sprint17-serpents\salamander-mesh.json' = $true
    'assets\sprint17-serpents\salamander-human-mesh.json' = $true
    'assets\sprint17-serpents\salamander-albedo.png' = $true
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
