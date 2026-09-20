# Scenario 1: changed output stays intact through the actual finalization
# path; catalog handles are released; the final record reports the failure
# and the preserved path.
$scenario = New-ScenarioRoot 'f1'
$script:state.txDir = Join-Path $scenario 'tx'
$catalog = New-RealCatalog $scenario
$owned = New-Object 'System.Collections.Generic.List[object]'
$save = New-OwnedSave $scenario 'transaction campaign bytes'
[IO.File]::WriteAllText($save.path, 'tampered transaction campaign bytes')
$owned.Add($save)
Reset-Stubs
$aggregate = $null
try {
    Invoke-FinalizationStages -Owned $owned -Catalog $catalog -TransactionDirectory $script:state.txDir `
        -PrimaryFailure $null -Runs $script:runs -ModsBeforeJson $script:modsJson -Tx 'tx-f1' -ExpectedPhaseCount 2
} catch { $aggregate = $_.ToString() }
$resultPath = Join-Path $script:state.txDir 'transaction-result.json'
if (-not (Test-Path -LiteralPath $resultPath -PathType Leaf)) { throw 'Scenario 1: the final record was not written.' }
$record = Read-FinalRecord $script:state.txDir
if ($record.passed -ne $false -or $record.cleanupFailed -ne $true) { throw 'Scenario 1: failure not reported in the final record.' }
if (@($record.preservedOwnedSaves | Where-Object { ([string]$_.path) -ceq [string]$save.path }).Count -ne 1) { throw 'Scenario 1: the preserved path is not reported.' }
if (-not (Test-Path -LiteralPath $save.path -PathType Leaf)) { throw 'Scenario 1: preserved output was deleted.' }
$stageStates = Read-StageStates $record
if ($stageStates['catalog-assertion'] -cne 'failed' -or $stageStates['catalog-disposal'] -cne 'succeeded' -or
    $stageStates['sidecar-restoration'] -cne 'succeeded' -or $stageStates['settings-restoration'] -cne 'succeeded') {
    throw 'Scenario 1: stage outcomes do not show protected finalization.' }
if ($aggregate -eq $null -or -not $aggregate.Contains('unowned-new-save')) { throw 'Scenario 1: aggregate failure missing the catalog assertion cause.' }
Assert-LeasesReleased $catalog 'Scenario 1'
if ($script:state.settings -ne 1 -or $script:state.sidecars -ne 1) { throw 'Scenario 1: safe cleanup stages were skipped.' }
$script:checks++
