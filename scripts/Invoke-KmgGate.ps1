<#
.SYNOPSIS
Runs one of three named validation levels and says plainly which gates it ran
and which it deferred by policy.

.DESCRIPTION
The owner's 2026-10-02 workflow amendment for Expanded Summoning Sprints 14-21
reads the repository's "validate after a source change" instruction at a
coherent candidate boundary rather than at every edit. The quality bar is
unchanged; what changes is how often the expensive sequence runs.

  Focused  - the inner development loop. A named subset of the domain tests and
             nothing else. Requires an explicit filter, and is never a gate.
  Sprint   - the candidate boundary. One repository-wrapper pass, one complete
             domain suite, one clean exact-reference Release build, one package
             assembly and one strict package validation, producing the single
             immutable candidate the sprint's runtime batch then deploys.
  Tranche  - the charter closure. The same single pipeline; what differs is
             that the tranche's exhaustive census, persistence matrix and
             five-profile compatibility matrix run after it through their own
             guarded orchestrators.

Sprint and Tranche delegate the whole pipeline to Build-Local.ps1, which
performs each expensive operation exactly once and records one candidate
package and DLL hash. This script decides nothing about the live machine and
never launches the game.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Focused', 'Sprint', 'Tranche')]
    [string]$Level,
    # Focused only, and required there. Semicolon- or comma-separated
    # substrings of test names. Sprint and Tranche refuse it.
    [string]$Filter,
    [string]$ReferenceBundleDir,
    [string]$TimingPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'KmgGate.Common.ps1')

$root = Split-Path -Parent $PSScriptRoot
if (-not $TimingPath) { $TimingPath = Join-Path $root 'artifacts\gate-timings.jsonl' }
New-Item -ItemType Directory -Path (Split-Path -Parent $TimingPath) -Force | Out-Null

# Resolved before anything runs, so a level/filter disagreement is refused
# rather than discovered after a hundred seconds of testing.
$effectiveFilter = Resolve-KmgGateFilter -Level $Level -Filter $Filter

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
        if ($effectiveFilter) { $record.filter = $effectiveFilter }
        Add-Content -LiteralPath $TimingPath -Value ($record | ConvertTo-Json -Compress) -Encoding utf8
        $script:ran += [pscustomobject]@{ gate = $Gate; seconds = $record.seconds }
    }
}

if ($Level -eq 'Focused') {
    Invoke-KmgWithTestFilter -Filter $effectiveFilter -Body {
        Invoke-KmgGateStep 'domain-tests (focused)' {
            & (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release -SkipRepositoryValidation
        }
    }
    Add-KmgDeferred 'repository wrapper' 'Focused level; runs at the sprint candidate boundary'
    Add-KmgDeferred 'complete domain suite' 'Focused level; runs at the sprint candidate boundary'
    Add-KmgDeferred 'exact-reference Release build' 'Focused level; runs at the sprint candidate boundary'
    Add-KmgDeferred 'package assembly and strict validation' 'Focused level; runs at the sprint candidate boundary'
} else {
    # One pipeline, one pass of each expensive operation. Build-Local.ps1 owns
    # the whole sequence - repository wrapper, complete domain suite, exact
    # build, package, strict validation - so nothing here can run any of them a
    # second time. The filter is cleared for the duration, which is what stops
    # an inherited KMG_TEST_FILTER narrowing a qualification.
    Invoke-KmgWithTestFilter -Filter '' -Body {
        Invoke-KmgGateStep 'repository wrapper, complete suite, exact build, package and strict validation' {
            $arguments = @{}
            if ($ReferenceBundleDir) { $arguments.ReferenceBundleDir = $ReferenceBundleDir }
            & (Join-Path $PSScriptRoot 'Build-Local.ps1') @arguments
            if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) {
                throw "Build-Local failed with exit code $LASTEXITCODE."
            }
        }
    }
    if ($Level -eq 'Sprint') {
        Add-KmgDeferred 'inventory and player-path census' 'Sprint level; samples the new creatures only, full census at tranche close'
        Add-KmgDeferred 'five-profile compatibility matrix' 'Sprint level; standalone plus the touched seam only'
        Add-KmgDeferred 'whole-roster persistence and module-disabled matrix' 'Sprint level; targeted persistence only, and only for new serialized state'
    }
    Add-KmgDeferred 'runtime scenarios' 'Driven by their own guarded orchestrators against the candidate this gate produced'
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
    Write-Host "FOCUSED RUN - NOT A QUALIFICATION. Filter '$effectiveFilter' selected a subset; the sprint candidate gate is the authority."
}
