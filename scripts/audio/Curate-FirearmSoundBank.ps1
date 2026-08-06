[CmdletBinding()]
param(
    [string]$GeneratedWindowsDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '..\common.ps1')

$root = Get-KmgRepositoryRoot -ScriptDirectory (Split-Path -Parent $PSScriptRoot)
$projectRoot = Join-Path $root 'assets-source\wwise\KingmakerGunslingerFirearms'
if(-not $GeneratedWindowsDirectory) {
    $GeneratedWindowsDirectory = Join-Path $projectRoot 'GeneratedSoundBanks\Windows'
}
$generatedRoot = [IO.Path]::GetFullPath($GeneratedWindowsDirectory)
$bankSource = Join-Path $generatedRoot 'KMG_Firearms.bnk'
$bankTextPath = Join-Path $generatedRoot 'KMG_Firearms.txt'
$infoPath = Join-Path $generatedRoot 'SoundbanksInfo.xml'
$projectPath = Join-Path $projectRoot 'KingmakerGunslingerFirearms.wproj'
$sourceMapPath = Join-Path $projectRoot 'source-map.json'
$provenancePath = Join-Path $root 'assets-source\third-party\audio\sse-library-guns\audio-manifest.json'

foreach($required in @($bankSource, $bankTextPath, $infoPath, $projectPath, $sourceMapPath, $provenancePath)) {
    if(-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Required authentic authoring output is missing: $required"
    }
}

[xml]$project = Get-Content -LiteralPath $projectPath -Raw
if($project.WwiseDocument.WwiseVersion -ne 'v2016.2.6' -or $project.WwiseDocument.WwiseBuild -ne '6153') {
    throw 'The bank source project is not Wwise 2016.2.6 build 6153.'
}
[xml]$info = Get-Content -LiteralPath $infoPath -Raw
if($info.SoundBanksInfo.Platform -ne 'Windows' -or $info.SoundBanksInfo.SoundbankVersion -ne '120') {
    throw 'Unexpected generated SoundBank platform or Wwise SoundBank version.'
}
$banks = @($info.SoundBanksInfo.SoundBanks.SoundBank | Where-Object { $_.ShortName -eq 'KMG_Firearms' })
if($banks.Count -ne 1 -or $banks[0].Path -ne 'KMG_Firearms.bnk') {
    throw 'Generated metadata must contain exactly one KMG_Firearms bank.'
}
$bank = $banks[0]
$expectedEvents = @(
    'KMG_Firearm_Pistol_Shot',
    'KMG_Firearm_Musket_Shot',
    'KMG_Firearm_Blunderbuss_Shot',
    'KMG_Firearm_Revolver_Shot',
    'KMG_Firearm_Rifle_Shot'
)
$actualEvents = @($bank.IncludedEvents.Event | ForEach-Object { [string]$_.Name })
if($actualEvents.Count -ne 5 -or (Compare-Object $expectedEvents $actualEvents)) {
    throw 'KMG_Firearms does not contain exactly the five canonical events.'
}
$memoryFiles = @($bank.IncludedMemoryFiles.File)
if($memoryFiles.Count -ne 5) { throw 'KMG_Firearms must contain exactly five in-memory media files.' }
if(@($info.SelectNodes('/SoundBanksInfo/StreamedFiles/*')).Count -ne 0) {
    throw 'External streamed media is forbidden.'
}
if(@($info.SelectNodes('/SoundBanksInfo/MediaFilesNotInAnyBank/*')).Count -ne 0) {
    throw 'Generated metadata reports media outside a SoundBank.'
}
$externalWem = @(Get-ChildItem -LiteralPath $generatedRoot -Recurse -Filter '*.wem' -File)
if($externalWem.Count -ne 0) { throw 'External .wem files are forbidden for KMG_Firearms.' }

$sourceMap = Get-Content -LiteralPath $sourceMapPath -Raw | ConvertFrom-Json
$expectedSources = @($sourceMap.events | ForEach-Object { [string]$_.source })
$actualSources = @($memoryFiles | ForEach-Object { [string]$_.ShortName })
if($actualSources.Count -ne 5 -or (Compare-Object $expectedSources $actualSources)) {
    throw 'Embedded media names do not match the approved source map.'
}
$provenance = Get-Content -LiteralPath $provenancePath -Raw | ConvertFrom-Json
foreach($sourceName in $expectedSources) {
    $records = @($provenance.records | Where-Object { $_.processed -ceq $sourceName })
    if($records.Count -ne 1) { throw "Approved provenance record is missing or ambiguous: $sourceName" }
    $originalPath = Join-Path $projectRoot (Join-Path 'Originals\SFX' $sourceName)
    if(-not (Test-Path -LiteralPath $originalPath -PathType Leaf)) { throw "Wwise original is missing: $sourceName" }
    $originalHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $originalPath).Hash.ToUpperInvariant()
    if($originalHash -cne [string]$records[0].processedSha256) {
        throw "Wwise original does not match its approved processed SHA-256: $sourceName"
    }
}
$bankText = Get-Content -LiteralPath $bankTextPath -Raw
foreach($eventName in $expectedEvents) {
    if($bankText -notmatch [regex]::Escape($eventName)) { throw "Bank text metadata omits $eventName." }
}
if(([regex]::Matches($bankText, '\\Actor-Mixer Hierarchy\\Default Work Unit\\KMG_Firearms_SFX\\')).Count -ne 5) {
    throw 'Bank text metadata must list exactly five in-memory KMG_Firearms_SFX media objects.'
}
if((Get-Item -LiteralPath $bankSource).Length -lt 100000) { throw 'Generated KMG_Firearms.bnk is unexpectedly small.' }

$destination = Join-Path $root 'assets\soundbanks'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$bankDestination = Join-Path $destination 'KMG_Firearms.bnk'
Copy-Item -LiteralPath $bankSource -Destination $bankDestination -Force
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $bankDestination).Hash.ToUpperInvariant()

$manifest = [ordered]@{
    schemaVersion = 1
    bankName = 'KMG_Firearms'
    bankFileName = 'KMG_Firearms.bnk'
    platform = 'Windows'
    wwiseVersion = '2016.2.6.6153'
    sha256 = $hash
    mediaEmbedded = $true
    events = [ordered]@{
        Pistol = 'KMG_Firearm_Pistol_Shot'
        Musket = 'KMG_Firearm_Musket_Shot'
        Blunderbuss = 'KMG_Firearm_Blunderbuss_Shot'
        Revolver = 'KMG_Firearm_Revolver_Shot'
        Rifle = 'KMG_Firearm_Rifle_Shot'
    }
}
$manifestPath = Join-Path $destination 'firearm-soundbank-manifest.json'
$json = $manifest | ConvertTo-Json -Depth 4
[IO.File]::WriteAllText($manifestPath, $json + "`r`n", [Text.UTF8Encoding]::new($false))

& (Join-Path $root 'scripts\Validate-FirearmSoundBank.ps1')
Write-Output "Curated authentic KMG_Firearms.bnk ($((Get-Item $bankDestination).Length) bytes, SHA-256 $hash)."
