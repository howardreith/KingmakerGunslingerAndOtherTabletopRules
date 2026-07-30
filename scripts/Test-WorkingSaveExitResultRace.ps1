[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$passed = 0
$failures = [Collections.Generic.List[string]]::new()
function Assert-True([bool]$condition, [string]$name) {
    if ($condition) { $script:passed++; return }
    $script:failures.Add($name)
}
function Assert-Throws([scriptblock]$action, [string]$name) {
    try { & $action; $script:failures.Add($name) }
    catch { $script:passed++ }
}

$root = Split-Path -Parent $PSScriptRoot
$testRoot = Join-Path $root 'artifacts\working-save-exit-result-race-test'
if (Test-Path -LiteralPath $testRoot) {
    Remove-Item -LiteralPath $testRoot -Recurse -Force
}
[void](New-Item -ItemType Directory -Path $testRoot)
$writtenUtc = [DateTime]::UtcNow.AddSeconds(-1)
$runId = 'current-run'
$scenario = 'working-save-smoke'
$version = '0.0.30'
$resultPath = Join-Path $testRoot 'runtime-result.json'

function Write-TestResult([string]$status, [string]$id = $runId,
    [string]$resultScenario = $scenario) {
    [ordered]@{
        schemaVersion = 1
        runId = $id
        scenario = $resultScenario
        status = $status
        loadedModVersion = $version
        evidenceDirectory = $testRoot
        evidenceFiles = @($resultPath)
    } | ConvertTo-Json -Depth 4 |
        Set-Content -LiteralPath $resultPath -Encoding UTF8
}

foreach ($status in @('PASS', 'FAIL', 'ERROR')) {
    Write-TestResult $status
    $actual = Get-KmgCurrentRuntimeResult -ResultPath $resultPath `
        -EvidenceDirectory $testRoot -RunId $runId -Scenario $scenario `
        -ExpectedVersion $version -RequestWrittenUtc $writtenUtc
    Assert-True ($actual.status -ceq $status) "post-exit-$($status.ToLower())-preserved"
}

Remove-Item -LiteralPath $resultPath -Force
$clock = [Diagnostics.Stopwatch]::StartNew()
$none = Wait-KmgRuntimeResultFlushGrace -ResultPath $resultPath `
    -EvidenceDirectory $testRoot -RunId $runId -Scenario $scenario `
    -ExpectedVersion $version -RequestWrittenUtc $writtenUtc `
    -GraceMilliseconds 150 -PollMilliseconds 25
$clock.Stop()
Assert-True ($null -eq $none -and $clock.ElapsedMilliseconds -ge 100) `
    'exit-no-result-final-bounded-rescan'

$job = Start-Job -ScriptBlock {
    param($path, $directory)
    Start-Sleep -Milliseconds 100
    [ordered]@{
        schemaVersion=1; runId='current-run'; scenario='working-save-smoke'
        status='PASS'; loadedModVersion='0.0.30'; evidenceDirectory=$directory
        evidenceFiles=@($path)
    } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $path -Encoding UTF8
} -ArgumentList $resultPath,$testRoot
$duringGrace = Wait-KmgRuntimeResultFlushGrace -ResultPath $resultPath `
    -EvidenceDirectory $testRoot -RunId $runId -Scenario $scenario `
    -ExpectedVersion $version -RequestWrittenUtc $writtenUtc `
    -GraceMilliseconds 1000 -PollMilliseconds 25
Wait-Job $job | Remove-Job
Assert-True ($duringGrace.status -ceq 'PASS') 'result-collected-during-flush-grace'

(Get-Item -LiteralPath $resultPath).LastWriteTimeUtc = $writtenUtc.AddSeconds(-1)
Assert-Throws {
    Get-KmgCurrentRuntimeResult -ResultPath $resultPath `
        -EvidenceDirectory $testRoot -RunId $runId -Scenario $scenario `
        -ExpectedVersion $version -RequestWrittenUtc $writtenUtc
} 'stale-result-rejected'
Write-TestResult 'PASS' 'wrong-run'
Assert-Throws {
    Get-KmgCurrentRuntimeResult -ResultPath $resultPath `
        -EvidenceDirectory $testRoot -RunId $runId -Scenario $scenario `
        -ExpectedVersion $version -RequestWrittenUtc $writtenUtc
} 'mismatched-run-rejected'
Write-TestResult 'PASS' $runId 'mod-load-smoke'
Assert-Throws {
    Get-KmgCurrentRuntimeResult -ResultPath $resultPath `
        -EvidenceDirectory $testRoot -RunId $runId -Scenario $scenario `
        -ExpectedVersion $version -RequestWrittenUtc $writtenUtc
} 'mismatched-scenario-rejected'

$runner = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$resultWriter = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestResult.cs')
$orchestrator = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot `
    'Invoke-KingmakerRuntimeTest.ps1')
$scenarioSource = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\WorkingSaveSmokeScenario.cs')
Assert-True ($orchestrator.IndexOf('Get-KmgCurrentRuntimeResult') -lt
    $orchestrator.IndexOf('$process.Refresh()')) 'result-priority-before-process-refresh'
Assert-True ($orchestrator.Contains('readinessObservationMissed') -and
    $orchestrator.Contains('final-result-received')) 'missed-readiness-is-warning'
Assert-True (-not $resultWriter.Contains('File.Delete(readyPath)')) `
    'readiness-persists-after-result'
Assert-True ($runner.IndexOf('WriteLifecycleStage("final-result-flushed")') -lt
    $runner.IndexOf('Application.Quit();')) 'quit-only-after-flushed-marker'
Assert-True ($runner.Contains('File.ReadAllText(flushedPath)')) `
    'flushed-marker-visibility-verified'
Assert-True ($runner.Contains('TryWriteEvidenceFailure(exception)') -and
    $runner.Contains('RuntimeTestStatuses.Error')) 'write-failure-structured-error'
Assert-True ($runner.Contains('load-completion-and-fingerprint')) `
    'load-without-fingerprint-cannot-pass'
Assert-True ($runner.IndexOf('fingerprint-complete') -lt
    $runner.IndexOf('Application.Quit();')) 'fingerprint-without-flush-cannot-exit'
Assert-True ($orchestrator.Contains('after final rescan and bounded flush grace')) `
    'true-early-exit-remains-error'
Assert-True (-not ($scenarioSource -match '\.(Save|AutoSave|QuickSave)\s*\(')) `
    'no-save-writing-introduced'
Assert-True ($orchestrator.Contains('[int]$SteamAppId = 640820')) `
    'steam-app-id-remains-mandatory'
Assert-True ($orchestrator.Contains("'Deploy-Local.ps1'") -and
    $orchestrator.Contains('No deployment or process launch occurred.')) `
    'deployment-and-whatif-contracts-retained'
Assert-True ($resultWriter.Contains('stream.Flush(true)') -and
    $resultWriter.Contains('File.Replace(temporary, path, null)')) `
    'atomic-evidence-retained'

Remove-Item -LiteralPath $testRoot -Recurse -Force
if ($failures.Count -ne 0) {
    throw "Working-save exit/result race tests failed: $($failures -join ', ')"
}
Write-Host "Working-save exit/result race tests passed: $passed"
