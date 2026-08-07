[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$script = Join-Path $PSScriptRoot 'Inspect-OptionalModReferences.ps1'
$repo = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$fixture = Join-Path $repo 'artifacts\compatibility\tests\inventory-fixture'
if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
New-Item -ItemType Directory -Path (Join-Path $fixture 'SourceOnly') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $fixture 'Invalid') -Force | Out-Null
Set-Content -LiteralPath (Join-Path $fixture 'SourceOnly\Main.cs') -Value 'class Main {}' -Encoding UTF8
Set-Content -LiteralPath (Join-Path $fixture 'Invalid\Info.json') -Value '{"Id":"Broken","DisplayName":"Broken","Version":"1.0","ManagerVersion":"0.1","Requirements":[],"EntryMethod":"Broken.Main.Load","AssemblyName":"missing.dll"}' -Encoding UTF8
$out = Join-Path $repo 'artifacts\compatibility\tests\inventory-output'
if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Recurse -Force }
$path = & $script -ReferenceRoot $fixture -ApprovedRoot $fixture -OutputRoot $out | Select-Object -Last 1
$data = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
if ($data.records.Count -ne 2) { throw 'Fixture inventory did not return exactly two children.' }
if (($data.records | Where-Object folderName -eq 'SourceOnly').classification -ne 'SOURCE-REFERENCE-ONLY') { throw 'Source-only fixture classification failed.' }
if (($data.records | Where-Object folderName -eq 'Invalid').classification -ne 'INVALID-LOADABLE-REFERENCE') { throw 'Invalid loadable fixture classification failed.' }
$escaped = $false
try { & $script -ReferenceRoot (Split-Path -Parent $fixture) -ApprovedRoot $fixture -OutputRoot $out | Out-Null } catch { $escaped = $_.Exception.Message -like '*escapes the approved root*' }
if (-not $escaped) { throw 'Approved-root escape was not rejected.' }
Write-Host 'Optional-mod reference inventory fixtures passed.'
