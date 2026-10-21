<#
.SYNOPSIS
Runs one of three named validation levels and says plainly which gates it ran
and which it deferred by policy.

.DESCRIPTION
The owner's 2026-10-11 validation policy makes Focused the ordinary development
checkpoint. Sprint/Tranche are explicitly selected broad candidate boundaries;
neither is a prerequisite for a narrow review handoff.

  Focused  - the default development/review entry: production compilation and a
             named domain subset. Requires a filter; no full/native or release
             qualification is claimed. See docs/VALIDATION-POLICY.md.
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
    [ValidateSet('Focused', 'Sprint', 'Tranche')]
    [string]$Level = 'Focused',
    # Focused only, and required there. Semicolon- or comma-separated
    # substrings of test names. Sprint and Tranche refuse it.
    [string]$Filter,
    [string]$ReferenceBundleDir,
    [string]$KingmakerInstallDir,
    [string]$MSBuildPath,
    [string]$TimingPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'KmgGate.Common.ps1')

$root = Split-Path -Parent $PSScriptRoot
if (-not $TimingPath) { $TimingPath = Join-Path $root 'artifacts\gate-timings.jsonl' }

# Resolved before anything runs, so a level/filter disagreement is refused
# rather than discovered after a hundred seconds of testing.
$effectiveFilter = Resolve-KmgGateFilter -Level $Level -Filter $Filter
if ($Level -eq 'Focused' -and $ReferenceBundleDir) {
    throw 'Focused compilation uses GamePath.props or -KingmakerInstallDir. Select Sprint/Tranche for the private-reference candidate builder.'
}
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
        if ($effectiveFilter) { $record.filter = $effectiveFilter }
        Add-Content -LiteralPath $TimingPath -Value ($record | ConvertTo-Json -Compress) -Encoding utf8
        $script:ran += [pscustomobject]@{ gate = $Gate; seconds = $record.seconds }
    }
}

if ($Level -eq 'Focused') {
    Invoke-KmgGateStep 'production compilation' {
        $compileArguments = @{ Configuration = 'Release'; CompileOnly = $true }
        if ($KingmakerInstallDir) { $compileArguments.KingmakerInstallDir = $KingmakerInstallDir }
        if ($MSBuildPath) { $compileArguments.MSBuildPath = $MSBuildPath }
        & (Join-Path $PSScriptRoot 'build.ps1') @compileArguments
    }
    Invoke-KmgWithTestFilter -Filter $effectiveFilter -Body {
        Invoke-KmgGateStep 'domain-tests (focused)' {
            & (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release -SkipRepositoryValidation -MSBuildPath $MSBuildPath
        }
    }
    Add-KmgDeferred 'repository wrapper' 'Focused level; runs at the sprint candidate boundary'
    Add-KmgDeferred 'complete domain suite' 'Focused level; runs at the sprint candidate boundary'
    Add-KmgDeferred 'exact-reference frozen Release candidate' 'Focused compiled production; immutable packaging is an explicit candidate boundary'
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
            if ($KingmakerInstallDir) { $arguments.KingmakerInstallDir = $KingmakerInstallDir }
            if ($MSBuildPath) { $arguments.MSBuildPath = $MSBuildPath }
            & (Join-Path $PSScriptRoot 'Build-Local.ps1') @arguments
            if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) {
                throw "Build-Local failed with exit code $LASTEXITCODE."
            }
            # A nested PowerShell packaging error can otherwise leave a zero
            # process exit while the suite/build output exists but no package
            # was validated. The gate's own success requires the exact receipt.
            $info = Get-Content -LiteralPath (Join-Path $root 'Info.json') -Raw |
                ConvertFrom-Json
            $package = Join-Path $root (
                'artifacts\local-runtime\' + $info.Version + '\' +
                $info.Id + '-' + $info.Version + '-local-runtime.zip')
            $manifestPath = $package + '.build-local.json'
            if (-not (Test-Path -LiteralPath $package -PathType Leaf) -or
                -not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
                throw 'Full gate did not produce a validated candidate ZIP and manifest.'
            }
            $manifest = Get-Content -LiteralPath $manifestPath -Raw |
                ConvertFrom-Json
            if ($manifest.commit -cne $commit -or
                $manifest.version -cne $info.Version -or
                $manifest.validated -ne $true -or
                $manifest.packagePath -cne $package -or
                $manifest.packageSha256 -cne
                    (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash.ToLowerInvariant()) {
                throw 'Full gate candidate receipt does not match the current source and ZIP.'
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
    Write-Host "FOCUSED DEVELOPMENT CHECKPOINT. Production compiled; filter '$effectiveFilter' selected related tests. Full/native/release qualification deferred."
}
