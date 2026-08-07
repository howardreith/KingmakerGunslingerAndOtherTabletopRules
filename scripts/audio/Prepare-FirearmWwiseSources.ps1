[CmdletBinding()]
param([string]$Destination)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$source=Join-Path $root 'assets-source\third-party\audio\sse-library-guns\processed'
$manifest=Get-Content -LiteralPath (Join-Path $root 'assets-source\third-party\audio\sse-library-guns\audio-manifest.json') -Raw | ConvertFrom-Json
if(-not $Destination){$Destination=Join-Path $root 'artifacts\wwise-source-staging'}
$Destination=[IO.Path]::GetFullPath($Destination)
New-Item -ItemType Directory -Path $Destination -Force | Out-Null
foreach($record in $manifest.records){
  $input=Join-Path $source $record.processed
  if(-not(Test-Path -LiteralPath $input -PathType Leaf)){throw "Approved processed source is missing: $input"}
  $hash=(Get-FileHash -LiteralPath $input -Algorithm SHA256).Hash
  if($hash -ne $record.processedSha256){throw "Processed source hash mismatch: $($record.processed)"}
  $output=Join-Path $Destination $record.processed
  if($record.mapping -contains 'blunderbuss-shot'){
    & (Join-Path $PSScriptRoot 'Trim-PcmWave.ps1') -InputPath $input -OutputPath $output -TrimSeconds 2.180
  } else {
    $existingHash = if(Test-Path -LiteralPath $output -PathType Leaf) {
      (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
    } else { $null }
    if($existingHash -ne $hash) {
      Copy-Item -LiteralPath $input -Destination $output -Force
    }
  }
}
Write-Host "Prepared five hash-verified Wwise source WAVs (Blunderbuss deterministically trimmed 2.180 seconds): $Destination"
