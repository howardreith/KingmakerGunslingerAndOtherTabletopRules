<# Filter decisions and exact environment restoration. Actual child calls
are checked by Test-DevelopmentGate.ps1; PlanOnly is descriptive, not proof. #>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'KmgGate.Common.ps1')

$savedFilter = $env:KMG_TEST_FILTER
try {
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
Assert (Throws { Resolve-KmgGateFilter -Level 'Focused' -Filter ',; , ' }) `
    'focused-with-only-separators-fails'
Assert ((Resolve-KmgGateFilter -Level 'Focused' -Filter 'sprint14; sprint15') -eq 'sprint14; sprint15') `
    'focused-keeps-multiple-real-terms'
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

$missingInput = Join-Path ([IO.Path]::GetTempPath()) ('kmg-no-reference-input-' + [Guid]::NewGuid().ToString('N'))
$liveBudget=Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts/closeout-triage/CLOSEOUT-BUDGET.json'
$budgetHashBefore=if(Test-Path -LiteralPath $liveBudget){(Get-FileHash -LiteralPath $liveBudget).Hash}else{$null}
$missingMessage=$null
try { & (Join-Path $PSScriptRoot 'Build-Local.ps1') -ReferenceBundleDir $missingInput -KingmakerInstallDir $missingInput }
catch { $missingMessage=$_.Exception.Message }
Assert ($missingMessage -ceq ('Qualified private reference bundle is missing: '+$missingInput)) 'real-build-refuses-missing-references'
$budgetHashAfter=if(Test-Path -LiteralPath $liveBudget){(Get-FileHash -LiteralPath $liveBudget).Hash}else{$null}
Assert ($budgetHashAfter -ceq $budgetHashBefore) 'missing-reference-refusal-preserves-budget'
Assert (-not (Test-Path -LiteralPath $missingInput)) 'refused-build-creates-no-reference-fixture'
if ($failures.Count -ne 0) { throw ('Gate-level contract failures: ' + ($failures -join ', ')) }
Write-Host 'Gate filter/refusal contracts passed.'
& (Join-Path $PSScriptRoot 'Test-DevelopmentGate.ps1')
} finally { $env:KMG_TEST_FILTER = $savedFilter }
