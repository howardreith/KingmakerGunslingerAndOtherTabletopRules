<#
.SYNOPSIS
Proves the gate level's filter hygiene and that its pipeline runs each
expensive operation once.

.DESCRIPTION
Cheap and offline. It never runs a gate, builds anything or launches the game;
it exercises the two filter decisions directly and reads Build-Local's own
plan. The point is that a qualification cannot be narrowed by an inherited
KMG_TEST_FILTER, and that nobody has to run a five-minute pipeline to find out
whether it validates the repository twice.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'KmgGate.Common.ps1')

$failures = @()
function Assert([bool]$Condition, [string]$Message) {
    if ($Condition) { Write-Host "PASS gate.$Message" }
    else { $script:failures += $Message; Write-Error "FAIL gate.$Message" -ErrorAction Continue }
}
function Throws([scriptblock]$Body) {
    try { & $Body; return $false } catch { return $true }
}

# --- the level decides the filter, and refuses a disagreement ---------------
Assert ((Resolve-KmgGateFilter -Level 'Focused' -Filter 'sprint14') -eq 'sprint14') `
    'focused-keeps-its-filter'
Assert ((Resolve-KmgGateFilter -Level 'Focused' -Filter '  sprint14  ') -eq 'sprint14') `
    'focused-trims-its-filter'
Assert (Throws { Resolve-KmgGateFilter -Level 'Focused' -Filter '' }) `
    'focused-without-a-filter-fails'
Assert (Throws { Resolve-KmgGateFilter -Level 'Focused' -Filter '   ' }) `
    'focused-with-a-blank-filter-fails'
Assert (Throws { Resolve-KmgGateFilter -Level 'Focused' -Filter $null }) `
    'focused-with-a-null-filter-fails'
Assert ((Resolve-KmgGateFilter -Level 'Sprint' -Filter '') -eq '') `
    'sprint-clears-the-filter'
Assert ((Resolve-KmgGateFilter -Level 'Tranche' -Filter $null) -eq '') `
    'tranche-clears-the-filter'
# Refused rather than ignored: a dropped filter would teach the caller it had
# been honoured.
Assert (Throws { Resolve-KmgGateFilter -Level 'Sprint' -Filter 'sprint14' }) `
    'sprint-refuses-a-filter'
Assert (Throws { Resolve-KmgGateFilter -Level 'Tranche' -Filter 'sprint14' }) `
    'tranche-refuses-a-filter'

# --- an inherited filter cannot narrow a qualification ----------------------
$env:KMG_TEST_FILTER = 'stale-from-an-earlier-focused-run'
$seenInsideFullGate = 'unset'
Invoke-KmgWithTestFilter -Filter '' -Body {
    $script:seenInsideFullGate = if (Test-Path Env:\KMG_TEST_FILTER) { $env:KMG_TEST_FILTER } else { '<absent>' }
}
Assert ($seenInsideFullGate -eq '<absent>') 'inherited-filter-cannot-narrow-a-full-gate'
Assert ((Test-Path Env:\KMG_TEST_FILTER) -and
    $env:KMG_TEST_FILTER -eq 'stale-from-an-earlier-focused-run') `
    'full-gate-restores-the-callers-filter'

# --- a focused run restores the previous environment exactly ----------------
$seenInsideFocused = 'unset'
Invoke-KmgWithTestFilter -Filter 'sprint14' -Body {
    $script:seenInsideFocused = $env:KMG_TEST_FILTER
}
Assert ($seenInsideFocused -eq 'sprint14') 'focused-applies-its-filter'
Assert ($env:KMG_TEST_FILTER -eq 'stale-from-an-earlier-focused-run') `
    'focused-restores-the-callers-filter'

# A variable that did not exist must not exist afterwards, which is a different
# thing from one restored to the empty string.
Remove-Item Env:\KMG_TEST_FILTER
Invoke-KmgWithTestFilter -Filter 'sprint14' -Body { }
Assert (-not (Test-Path Env:\KMG_TEST_FILTER)) 'focused-leaves-an-unset-variable-unset'

# A body that throws must still restore the environment.
$env:KMG_TEST_FILTER = 'prior'
try { Invoke-KmgWithTestFilter -Filter 'x' -Body { throw 'boom' } } catch { }
Assert ($env:KMG_TEST_FILTER -eq 'prior') 'a-failing-body-still-restores-the-filter'
Remove-Item Env:\KMG_TEST_FILTER -ErrorAction SilentlyContinue

# --- each expensive operation appears exactly once --------------------------
$plan = @(@(& (Join-Path $PSScriptRoot 'Build-Local.ps1') -PlanOnly) |
    ForEach-Object { $_.ToString().Trim() } |
    Where-Object { $_ })
foreach ($step in @('repository-wrapper', 'complete-domain-suite',
        'exact-reference-release-build', 'package-assembly',
        'strict-package-validation')) {
    Assert (@($plan | Where-Object { $_ -eq $step }).Count -eq 1) "plan-runs-$step-once"
}
Assert ($plan.Count -eq 5) 'plan-contains-exactly-the-five-operations'

# The duplication the amendment named: Build-Local validates the repository
# itself and must therefore tell test-domain.ps1 not to do it again.
$buildLocal = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Build-Local.ps1') -Raw
Assert ($buildLocal -match 'test-domain\.ps1.*(\r?\n\s*)?-MSBuildPath \$msbuild -SkipRepositoryValidation') `
    'build-local-validates-the-repository-once'

# A qualification level must not be able to defer its own build.
$gate = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Invoke-KmgGate.ps1') -Raw
Assert ($gate -notmatch '\$SkipBuild') 'no-skip-build-on-a-qualification-level'

if ($failures.Count -ne 0) {
    throw ("Gate-level contract failures: " + ($failures -join ', '))
}
Write-Host ("Gate-level contract validation passed: " +
    "filter hygiene, environment restoration and a single-pass pipeline.")
