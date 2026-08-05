[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$collector = Join-Path $root 'artifacts\manual-test\dodge-forensics\Collect-DodgeForensics.ps1'
function Require([bool]$condition,[string]$name){if(-not $condition){throw "FAIL: $name"};Write-Host "PASS: $name"}
function Record([long]$sequence,[string]$event,[long]$game,[Nullable[long]]$end,[string]$identity){
    [ordered]@{sequence=$sequence;eventName=$event;utcTimestamp='2026-08-05T00:00:00Z';threadId=1;isGameThread=$true;gameTimeTicks=$game;gameTimeSeconds=($game/10000000.0);turnBasedState=$false;ownerIdentity='owner-1';buffRuntimeReferenceId=$identity;blueprintGuid=if($identity){'bbd7d42117cc4c23b3e22af3a71621d9'}else{$null};blueprintInternalName=if($identity){'KMG_GunslingerDodge_ArmorClass_Buff'}else{$null};dodgeCollectionCount=if($identity){1}else{0};dodgeInstanceIdentities=@($identity);endTimeTicks=$end;timeLeftTicks=$null;nextTickTimeTicks=$null;nextEventTimeTicks=$end;isPermanent=$false;isActive=$true;isDisposed=$false;rank=1;collectionNextEventRuntimeIdentity=$identity;armorClassModifiedValue=17;gritAmount=2;requestedDurationTicks=$null;exceptionType=$null;exceptionMessage=$null}
}
function Invoke-Validation([string]$path,[bool]$expectPass,[string]$message){
    $priorPreference=$ErrorActionPreference;$ErrorActionPreference='Continue'
    $output=& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $collector -TracePath $path -ValidateOnly 2>&1
    $passed=$LASTEXITCODE -eq 0
    $ErrorActionPreference=$priorPreference
    Require ($passed -eq $expectPass) $message
    return ($output -join "`n")
}
$temp=Join-Path $env:TEMP ('kmg-collector-tests-'+[guid]::NewGuid());New-Item -Path $temp -ItemType Directory|Out-Null
try{
    $failed=Join-Path $root 'tests\fixtures\dodge-forensics-reference-stub.jsonl'
    $failure=Invoke-Validation $failed $false 'collector rejects actual reference-stub shape'
    Require ($failure -match 'reference-metadata line') 'reference-stub rejection is precise'
    $enable=Join-Path $temp 'enable.jsonl'; (Record 1 'forensics-enabled' 0 $null $null)|ConvertTo-Json -Compress|Set-Content $enable
    $failure=Invoke-Validation $enable $false 'collector rejects enable-only trace'
    Require ($failure -match 'only the forensics-enabled record') 'enable-only rejection is precise'
    $valid=Join-Path $temp 'valid.jsonl'; @(
        (Record 1 'forensics-enabled' 0 $null $null),
        (Record 2 'dodge-delivery-entry' 100 $null $null),
        (Record 3 'add-buff-internal-postfix' 110 60000110 'ABC123'),
        (Record 4 'sample' 70000110 60000110 'ABC123')
    )|ForEach-Object{$_|ConvertTo-Json -Compress}|Set-Content $valid
    $summary=Invoke-Validation $valid $true 'collector accepts valid Dodge lifecycle trace'
    Require ($summary -match 'postEndTimeSampleCount\s+: 1') 'collector reports post-EndTime sample'
    $collectorText=Get-Content $collector -Raw
    Require ($collectorText -notmatch 'TotalDefense|Total Defense') 'collector does not require Total Defense'
}finally{if(Test-Path $temp){Remove-Item $temp -Recurse -Force}}
Write-Host 'Dodge forensics collector validation tests passed (7 assertions).'
