[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,
    [switch]$AllowMissingFirearmSoundBank
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$info = Get-KmgModInfo -RepositoryRoot $repositoryRoot

if (-not (Test-Path -LiteralPath $PackagePath -PathType Leaf)) {
    throw "Package does not exist: $PackagePath"
}

$tempDirectory = Join-Path ([IO.Path]::GetTempPath()) ("KmgPackageValidation-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempDirectory -Force | Out-Null

try {
    Expand-Archive -LiteralPath $PackagePath -DestinationPath $tempDirectory -Force
    $roots = @(Get-ChildItem -LiteralPath $tempDirectory -Directory)
    if ($roots.Count -ne 1 -or $roots[0].Name -ne $info.Id) {
        throw "Package must contain exactly one root directory named '$($info.Id)'."
    }

    $modDirectory = $roots[0].FullName
    $expected = @(
        'CHANGELOG.md',
        'Info.json',
        'INSTALLATION-COMPATIBILITY.md',
        'KingmakerGunslinger.dll',
        'LICENSE',
        'README.md',
        'SMOKE-TEST-GUIDE.md',
        'THIRD-PARTY-ASSETS.md',
        'assets\bundles\kingmakergunslinger.firearms',
        'assets\bundles\asset-bundle-manifest.json',
        'blueprints\blueprints.json',
        'blueprints\blueprints.schema.json'
    )
    $iconNames = @('gunslinger-class','firearm-proficiency','gunsmithing','grit',
        'deeds','nimble','bonus-feat','gun-training','true-grit','rapid-reload',
        'weapon-focus-firearm','deadeye','gunslingers-dodge','quick-clear','reload-firearm',
        'repair-firearm','overhaul-firearm','early-pistol','musket','blunderbuss',
        'rifle','revolver','lead-ball','black-powder','repair-kit',
        'gunsmith-kit','overhaul-kit','focused-aim')
    $expected += @($iconNames | ForEach-Object { "assets\icons\$_.png" })
    $packagedBank=Join-Path $modDirectory 'assets\soundbanks\KMG_Firearms.bnk'
    if(Test-Path -LiteralPath $packagedBank -PathType Leaf){
        $expected += @('assets\soundbanks\KMG_Firearms.bnk','assets\soundbanks\firearm-soundbank-manifest.json')
        $manifest=Get-Content -LiteralPath (Join-Path $modDirectory 'assets\soundbanks\firearm-soundbank-manifest.json') -Raw | ConvertFrom-Json
        if((Get-KmgSha256 -Path $packagedBank) -cne $manifest.sha256){throw 'Packaged firearm SoundBank hash mismatch.'}
    } elseif(-not $AllowMissingFirearmSoundBank){throw 'Release package is missing authentic KMG_Firearms.bnk.'}
    $actual = @(
        Get-ChildItem -LiteralPath $modDirectory -Recurse -File |
            ForEach-Object { $_.FullName.Substring($modDirectory.Length).TrimStart('\', '/') } |
            Sort-Object
    )
    $expectedSorted = @($expected | Sort-Object)
    if (($actual -join "`n") -ne ($expectedSorted -join "`n")) {
        throw "Package entries do not match the strict release allowlist.`nExpected:`n$($expectedSorted -join [Environment]::NewLine)`nActual:`n$($actual -join [Environment]::NewLine)"
    }

    $packagedInfo = Get-Content -LiteralPath (Join-Path $modDirectory 'Info.json') -Raw | ConvertFrom-Json
    if ($packagedInfo.Id -ne $info.Id -or $packagedInfo.Version -ne $info.Version) {
        throw 'Packaged Info.json does not match the repository mod ID and version.'
    }
    if ($packagedInfo.AssemblyName -ne 'KingmakerGunslinger.dll') {
        throw 'Packaged Info.json names an unexpected mod assembly.'
    }

    $binaryFiles = @(
        Get-ChildItem -LiteralPath $modDirectory -Recurse -File |
            Where-Object { $_.Extension -in @('.dll', '.exe', '.pdb', '.mdb') }
    )
    if ($binaryFiles.Count -ne 1 -or $binaryFiles[0].Name -ne 'KingmakerGunslinger.dll') {
        throw 'The standalone UMM package must contain exactly one binary: KingmakerGunslinger.dll.'
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
        if (Get-ChildItem -LiteralPath $modDirectory -Recurse -File -Filter $name -ErrorAction SilentlyContinue) {
            throw "Install package contains a private or foreign runtime assembly: $name"
        }
    }
    $audioArtifacts=@(Get-ChildItem -LiteralPath $modDirectory -Recurse -File | Where-Object {$_.Extension -in @('.bnk','.wem')})
    if($audioArtifacts.Count -gt 1 -or ($audioArtifacts.Count -eq 1 -and $audioArtifacts[0].Name -cne 'KMG_Firearms.bnk')){throw 'Package contains forbidden or unexpected Wwise artifacts.'}
    if(Get-ChildItem -LiteralPath $modDirectory -Recurse -File | Where-Object {$_.Name -ieq 'Init.bnk'}){throw 'Package contains forbidden Init.bnk.'}
}
finally {
    if (Test-Path -LiteralPath $tempDirectory) {
        Remove-Item -LiteralPath $tempDirectory -Recurse -Force
    }
}

Write-Host "Strict standalone UMM package validation passed: $PackagePath"
