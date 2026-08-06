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
  Copy-Item -LiteralPath $input -Destination (Join-Path $Destination $record.processed) -Force
}
Write-Host "Prepared five hash-verified Wwise source WAVs: $Destination"
