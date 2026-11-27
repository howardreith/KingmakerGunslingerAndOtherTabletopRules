[CmdletBinding()]
param([string]$ReportPath)

# Selection semantics of loader.production-dll-references-pinned-loader on this checkout (it builds the domain runner
# and the production DLL into this checkout's artifacts; nothing is installed or launched):
#   - the wrapper (test-domain.ps1) with a bound DLL passes;
#   - the direct runner selecting the case by a KMG_TEST_FILTER substring, or by --test-prefix, without a binding fails;
#   - the direct runner unfiltered without a binding reports the case NOT APPLICABLE, never PASS, and exits 0;
#   - a Build-Local deferral reports NOT APPLICABLE;
#   - the wrapper given a production DLL that was removed during preparation refuses before the runner starts;
#   - a digest refusal inside the wrapper restores the caller's binding variables and leaves no private copy;
#   - the fixture itself restores its caller's filter and four binding variables on every exit.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$case = 'loader.production-dll-references-pinned-loader'
$runner = Join-Path $root 'artifacts\tests\Release\KingmakerGunslinger.DomainTests\KingmakerGunslinger.DomainTests.exe'
$productionDll = Join-Path $root 'artifacts\bin\Release\KingmakerGunslinger\KingmakerGunslinger.dll'
$cases = [Collections.Generic.List[object]]::new()
function Add-Case([string]$Name, [bool]$Passed, [string]$Detail) {
    $cases.Add([ordered]@{ name = $Name; passed = $Passed; detail = $Detail })
    Write-Host ("{0} {1} :: {2}" -f $(if ($Passed) { 'PASS' } else { 'FAIL' }), $Name, $Detail)
}
$bindingNames = @('KMG_TEST_FILTER', 'KMG_PRODUCTION_DLL', 'KMG_PRODUCTION_DLL_SHA256', 'KMG_EXPECTED_LOADER_REFERENCES', 'KMG_LOADER_CASE_DEFERRED')
# The caller's values of all five variables are saved before the fixture changes any of them and restored on every exit
# (success, a failed check or an exception), so running the fixture in an existing session leaves its filter and its
# production-DLL binding as they were.
$callerBindings = @{}; foreach ($name in $bindingNames) { $callerBindings[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
try {
function Invoke-Runner([hashtable]$Environment, [string[]]$Arguments) {
    $saved = @{}; foreach ($name in $bindingNames) { $saved[$name] = [Environment]::GetEnvironmentVariable($name, 'Process'); [Environment]::SetEnvironmentVariable($name, $null, 'Process') }
    $previous = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    try {
        foreach ($key in $Environment.Keys) { [Environment]::SetEnvironmentVariable($key, $Environment[$key], 'Process') }
        $output = @(& $runner @Arguments 2>&1 | ForEach-Object { "$_" })
        return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Lines = $output }
    }
    finally {
        $ErrorActionPreference = $previous
        foreach ($name in $bindingNames) { [Environment]::SetEnvironmentVariable($name, $saved[$name], 'Process') }
    }
}
function Get-CaseLine($Lines) { @($Lines | Where-Object { $_ -match ('^(PASS|FAIL|NOT APPLICABLE) ' + [regex]::Escape($case)) } | ForEach-Object { if ($_.Length -gt 220) { $_.Substring(0, 220) } else { $_ } }) -join ' / ' }

# 1. The wrapper with a bound DLL (it compiles production first): PASS.
$env:KMG_TEST_FILTER = $case
try { $wrapped = @(& (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release -SkipRepositoryValidation 6>&1 2>&1 | ForEach-Object { "$_" }); $wrappedOk = $true }
catch { $wrapped = @($_.Exception.Message); $wrappedOk = $false }
finally { Remove-Item Env:KMG_TEST_FILTER -ErrorAction SilentlyContinue }
Add-Case 'wrapper-bound-dll-passes' ($wrappedOk -and (@($wrapped | Where-Object { $_ -ceq "PASS $case" }).Count -eq 1)) (Get-CaseLine $wrapped)

# 2. Direct runner, substring selection, no binding: FAIL.
$r = Invoke-Runner @{ KMG_TEST_FILTER = 'production-dll' } @()
Add-Case 'direct-substring-selection-without-a-dll-fails' ($r.ExitCode -ne 0 -and (Get-CaseLine $r.Lines) -match '^FAIL ') ("exit $($r.ExitCode); " + (Get-CaseLine $r.Lines))
# 3. Direct runner, --test-prefix selection, no binding: FAIL.
$r = Invoke-Runner @{} @('--test-prefix', 'loader.')
Add-Case 'direct-test-prefix-selection-without-a-dll-fails' ($r.ExitCode -ne 0 -and (Get-CaseLine $r.Lines) -match '^FAIL ') ("exit $($r.ExitCode); " + (Get-CaseLine $r.Lines))
# 4. Direct runner unfiltered, no binding: NOT APPLICABLE (not PASS), run green.
$r = Invoke-Runner @{} @()
$summary = @($r.Lines | Where-Object { $_ -match '^Completed \d+ tests; failures=|^Not applicable' }) -join ' / '
Add-Case 'direct-unfiltered-run-reports-the-case-not-applicable' ($r.ExitCode -eq 0 -and (Get-CaseLine $r.Lines) -match '^NOT APPLICABLE ' -and @($r.Lines | Where-Object { $_ -ceq "PASS $case" }).Count -eq 0) ("exit $($r.ExitCode); " + (Get-CaseLine $r.Lines) + ' / ' + $summary)
# 5. A Build-Local deferral: NOT APPLICABLE.
$r = Invoke-Runner @{ KMG_TEST_FILTER = $case; KMG_LOADER_CASE_DEFERRED = '1' } @()
Add-Case 'deferred-case-reports-not-applicable' ($r.ExitCode -eq 0 -and (Get-CaseLine $r.Lines) -match '^NOT APPLICABLE .*deferred') (Get-CaseLine $r.Lines)

# 6. The wrapper given a production DLL removed during preparation: refused before the runner.
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('kmg-loader-selection-' + [Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($scratch)
try {
    $removed = Join-Path $scratch 'KingmakerGunslinger.dll'
    Copy-Item -LiteralPath $productionDll -Destination $removed
    $removedSha = (Get-FileHash -LiteralPath $removed -Algorithm SHA256).Hash.ToLowerInvariant()
    Remove-Item -LiteralPath $removed
    $env:KMG_TEST_FILTER = $case
    try { & (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release -SkipRepositoryValidation -ProductionDll $removed -ProductionDllSha256 $removedSha | Out-Null; $refusal = '' }
    catch { $refusal = $_.Exception.Message }
    finally { Remove-Item Env:KMG_TEST_FILTER -ErrorAction SilentlyContinue }
    Add-Case 'wrapper-removed-dll-refused' ($refusal -match 'is missing .*; the case was not run') $refusal

    # 7. A digest refusal in the same process restores the caller's binding variables and removes the private copy.
    $seed = @{ KMG_PRODUCTION_DLL = 'seed-dll'; KMG_PRODUCTION_DLL_SHA256 = 'seed-sha'; KMG_EXPECTED_LOADER_REFERENCES = 'seed-refs'; KMG_LOADER_CASE_DEFERRED = 'seed-deferred' }
    foreach ($key in $seed.Keys) { [Environment]::SetEnvironmentVariable($key, $seed[$key], 'Process') }
    $env:KMG_TEST_FILTER = $case
    $privateBefore = @(Get-ChildItem -LiteralPath ([IO.Path]::GetTempPath()) -Directory -Filter 'kmg-production-dll-*' -ErrorAction SilentlyContinue).Count
    try { & (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release -SkipRepositoryValidation -ProductionDll $productionDll -ProductionDllSha256 ('1' * 64) | Out-Null; $refusal = '' }
    catch { $refusal = $_.Exception.Message }
    finally { Remove-Item Env:KMG_TEST_FILTER -ErrorAction SilentlyContinue }
    $privateAfter = @(Get-ChildItem -LiteralPath ([IO.Path]::GetTempPath()) -Directory -Filter 'kmg-production-dll-*' -ErrorAction SilentlyContinue).Count
    $restored = @($seed.Keys | Where-Object { [Environment]::GetEnvironmentVariable($_, 'Process') -cne $seed[$_] }).Count -eq 0
    foreach ($key in $seed.Keys) { [Environment]::SetEnvironmentVariable($key, $null, 'Process') }
    Add-Case 'digest-refusal-restores-bindings-and-removes-the-private-copy' ($refusal -match 'changed after its compile' -and $restored -and $privateAfter -eq $privateBefore) "refusal: $refusal; bindings restored: $restored; private copies before $privateBefore, after $privateAfter"
}
finally { Remove-Item -LiteralPath $scratch -Recurse -Force -ErrorAction SilentlyContinue }

$failed = @($cases | Where-Object { -not $_.passed })
$report = [ordered]@{ schema = 'kmg.loader-case-selection-tests.v1'; commit = (& git -C $root rev-parse HEAD).Trim(); passed = $cases.Count - $failed.Count; total = $cases.Count; cases = @($cases) }
if ($ReportPath) { $report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $ReportPath -Encoding UTF8 }
Write-Host ("RESULT {0}/{1} loader-case selection checks passed" -f $report.passed, $report.total)
if ($failed.Count -gt 0) { throw "$($failed.Count) loader-case selection check(s) failed." }
}
finally { foreach ($name in $bindingNames) { [Environment]::SetEnvironmentVariable($name, $callerBindings[$name], 'Process') } }
