param(
    [switch]$RequireAuthoredObjects
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$projectRoot = Join-Path $root 'assets-source\wwise\KingmakerGunslingerFirearms'
$projectPath = Join-Path $projectRoot 'KingmakerGunslingerFirearms.wproj'
$mixerPath = Join-Path $projectRoot 'Master-Mixer Hierarchy\Default Work Unit.wwu'
$sourceMapPath = Join-Path $projectRoot 'source-map.json'

foreach($required in @($projectPath, $mixerPath, $sourceMapPath)) {
    if(-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Missing required Wwise authoring file: $required"
    }
}

[xml]$project = Get-Content -LiteralPath $projectPath -Raw
if($project.WwiseDocument.WwiseVersion -ne 'v2016.2.6' -or $project.WwiseDocument.WwiseBuild -ne '6153') {
    throw 'The authoring project is not Wwise 2016.2.6 build 6153.'
}
$projectNode = $project.WwiseDocument.ProjectInfo.Project
if($projectNode.Name -ne 'KingmakerGunslingerFirearms') {
    throw 'Unexpected Wwise project name.'
}
$windows = @($projectNode.Platforms.Platform | Where-Object { $_.Name -eq 'Windows' })
if($windows.Count -ne 1) { throw 'The authoring project must contain exactly one Windows platform.' }

[xml]$mixer = Get-Content -LiteralPath $mixerPath -Raw
$weaponBuses = @(Select-Xml -Xml $mixer -XPath "//Bus[@Name='WEAPONS' and @ID='{90EB9CC7-BB9C-42E0-9B57-62AC34459906}']")
if($weaponBuses.Count -ne 1) { throw 'The exact Owlcat Kingmaker WEAPONS bus identity is missing.' }

$forbidden = @(Get-ChildItem -LiteralPath $projectRoot -Recurse -File | Where-Object {
    $_.Name -ieq 'Init.bnk' -or $_.Extension -ieq '.wem' -or $_.Extension -ieq '.bnk'
})
if($forbidden.Count -ne 0) {
    throw ('Generated or forbidden Wwise artifacts are present: ' + (($forbidden.FullName) -join ', '))
}

$map = Get-Content -LiteralPath $sourceMapPath -Raw | ConvertFrom-Json
$expectedEvents = @(
    'KMG_Firearm_Pistol_Shot',
    'KMG_Firearm_Musket_Shot',
    'KMG_Firearm_Blunderbuss_Shot',
    'KMG_Firearm_Revolver_Shot',
    'KMG_Firearm_Rifle_Shot'
)
$actualEvents = @($map.events | ForEach-Object { $_.event })
if($actualEvents.Count -ne 5 -or (Compare-Object $expectedEvents $actualEvents)) {
    throw 'source-map.json does not contain the exact five canonical events.'
}
if($map.bank -ne 'KMG_Firearms' -or $map.platform -ne 'Windows' -or $map.streaming -ne $false -or $map.mediaEmbedded -ne $true) {
    throw 'source-map.json does not match the release authoring contract.'
}

if($RequireAuthoredObjects) {
    $eventText = (Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Events') -Filter '*.wwu' -File | Get-Content -Raw) -join "`n"
    foreach($eventName in $expectedEvents) {
        if($eventText -notmatch [regex]::Escape('Name="' + $eventName + '"')) {
            throw "Missing authored Wwise event: $eventName"
        }
    }
    $bankText = (Get-ChildItem -LiteralPath (Join-Path $projectRoot 'SoundBanks') -Filter '*.wwu' -File | Get-Content -Raw) -join "`n"
    if($bankText -notmatch [regex]::Escape('Name="KMG_Firearms"')) {
        throw 'Missing authored KMG_Firearms SoundBank.'
    }
}

Write-Output ('PASS: Wwise 2016.2.6 authoring project scaffold validated. Authored objects required: ' + [bool]$RequireAuthoredObjects)
