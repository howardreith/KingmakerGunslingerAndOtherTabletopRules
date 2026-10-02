<#
.SYNOPSIS
Runs one of three named validation levels and says plainly which gates it ran
and which it deferred by policy.

.DESCRIPTION
The owner's 2026-10-02 workflow amendment for Expanded Summoning Sprints 14-21
reads the repository's "validate after a source change" instruction at a
coherent candidate boundary rather than at every edit. The quality bar is
unchanged; what changes is how often the expensive sequence runs.

  Focused  - the inner development loop. The smallest relevant tests and
             validator, and an incremental compile. Never a gate.
  Sprint   - the candidate boundary. The complete domain suite, the repository
             wrapper and one clean exact-reference Release build, so a
             deployment can follow against that exact artifact.
  Tranche  - the charter closure. Everything Sprint runs, plus the package and
             its strict validation.

This script invokes the existing tools rather than reimplementing them, and
appends one timing record per gate so the next sprint can be planned from
measurements instead of impressions. Runtime scenarios, the inventory and
player-path census, the persistence trio and the compatibility matrix stay
where they are, driven by their own guarded orchestrators: this script decides
nothing about the live machine and never launches the game.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Focused', 'Sprint', 'Tranche')]
    [string]$Level,
    # Focused only. Semicolon- or comma-separated substrings of test names.
    [string]$Filter,
    [switch]$SkipBuild,
    [string]$ReferenceBundleDir = 'C:\Dev\KingmakerGunslingerLab\private\extracted-references',
    [string]$TimingPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
if (-not $TimingPath) { $TimingPath = Join-Path $root 'artifacts\gate-timings.jsonl' }
New-Item -ItemType Directory -Path (Split-Path -Parent $TimingPath) -Force | Out-Null

$script:ran = @()
$script:deferred = @()
$commit = (git -C $root rev-parse HEAD)

function Add-KmgDeferred([string]$Gate, [string]$Why) {
    $script:deferred += [pscustomobject]@{ gate = $Gate; reason = $Why }
}

function Invoke-KmgGateStep([string]$Gate, [scriptblock]$Body) {
    Write-Host "=== $Gate ==="
    $clock = [System.Diagnostics.Stopwatch]::StartNew()
    $failed = $null
    try { & $Body } catch { $failed = $_.Exception.Message; throw }
    finally {
        $clock.Stop()
        $record = [ordered]@{
            schemaVersion = 1
            timestampUtc = [DateTime]::UtcNow.ToString('o')
            level = $Level
            gate = $Gate
            commit = $commit
            seconds = [Math]::Round($clock.Elapsed.TotalSeconds, 1)
            failed = [bool]$failed
        }
        if ($Filter) { $record.filter = $Filter }
        Add-Content -LiteralPath $TimingPath -Value ($record | ConvertTo-Json -Compress) -Encoding utf8
        $script:ran += [pscustomobject]@{ gate = $Gate; seconds = $record.seconds }
    }
}

$python = (Get-Command python).Source

if ($Level -eq 'Focused') {
    Invoke-KmgGateStep 'domain-tests (focused)' {
        if ($Filter) { $env:KMG_TEST_FILTER = $Filter }
        try { & (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release -SkipRepositoryValidation }
        finally { Remove-Item Env:\KMG_TEST_FILTER -ErrorAction SilentlyContinue }
    }
    Add-KmgDeferred 'repository wrapper' 'Focused level; runs at the sprint candidate boundary'
    Add-KmgDeferred 'exact-reference Release build' 'Focused level; runs at the sprint candidate boundary'
    Add-KmgDeferred 'package validation' 'Focused level; runs at the tranche boundary'
} else {
    Invoke-KmgGateStep 'repository wrapper and complete domain suite' {
        & (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release
    }
    if ($SkipBuild) {
        Add-KmgDeferred 'exact-reference Release build' 'Explicitly skipped by -SkipBuild'
    } else {
        Invoke-KmgGateStep 'exact-reference Release build' {
            $dotnet = (Get-Command dotnet).Source
            $csc = Get-ChildItem -Path (Split-Path $dotnet) -Recurse -Filter 'csc.dll' -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -like '*Roslyn*' } | Sort-Object FullName -Descending | Select-Object -First 1
            if (-not $csc) { throw 'Roslyn csc.dll was not found beneath the installed dotnet SDK.' }
            & $python (Join-Path $root 'tools\build_mod_from_private_references.py') `
                --reference-bundle-dir $ReferenceBundleDir --dotnet $dotnet --csc $csc.FullName `
                --net47-ref-dir 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7' `
                --output-dir (Join-Path $root 'artifacts\gate-exact-build') `
                --configuration Release --git-commit $commit
            if ($LASTEXITCODE -ne 0) { throw "Exact-reference Release build failed with exit code $LASTEXITCODE." }
        }
    }
    if ($Level -eq 'Sprint') {
        Add-KmgDeferred 'package and strict package validation' 'Sprint level; the guarded orchestrator packages its own candidate'
        Add-KmgDeferred 'inventory and player-path census' 'Sprint level; samples the new creatures only, full census at tranche close'
        Add-KmgDeferred 'five-profile compatibility matrix' 'Sprint level; standalone plus the touched seam only'
    } else {
        Invoke-KmgGateStep 'package and strict package validation' {
            & (Join-Path $PSScriptRoot 'Build-Local.ps1') -ReferenceBundleDir $ReferenceBundleDir
            if ($LASTEXITCODE -ne 0) { throw "Build-Local failed with exit code $LASTEXITCODE." }
        }
        Add-KmgDeferred 'runtime scenarios, census, persistence, compatibility' 'Driven by their own guarded orchestrators against the deployed candidate'
    }
}

Write-Host ''
Write-Host "Gate level: $Level  (commit $commit)"
Write-Host 'Ran:'
foreach ($entry in $script:ran) { Write-Host ("  {0}  {1}s" -f $entry.gate, $entry.seconds) }
if ($script:deferred.Count -eq 0) {
    Write-Host 'Deferred by policy: nothing.'
} else {
    Write-Host 'Deferred by policy:'
    foreach ($entry in $script:deferred) { Write-Host ("  {0}  - {1}" -f $entry.gate, $entry.reason) }
}
Write-Host "Timings appended to $TimingPath"
if ($Level -eq 'Focused') {
    Write-Host 'A focused run is not a qualification. The sprint candidate gate is the authority.'
}
