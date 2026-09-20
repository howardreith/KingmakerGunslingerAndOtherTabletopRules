# Scenario 2: a catalog assertion failure (foreign new save) with a
# successful deletion still permits disposal and final reporting.
$scenario = New-ScenarioRoot 'f2'
$script:state.txDir = Join-Path $scenario 'tx'
$catalog = New-RealCatalog $scenario
$owned = New-Object 'System.Collections.Generic.List[object]'
$save = New-OwnedSave $scenario 'clean transaction campaign bytes'
$owned.Add($save)
Reset-Stubs
[IO.File]::WriteAllText((Join-Path $scenario 'saves\Manual_99_FOREIGN.zks'), 'foreign')
$aggregate = $null
try {
    Invoke-FinalizationStages -Owned $owned -Catalog $catalog -TransactionDirectory $script:state.txDir `
        -PrimaryFailure $null -Runs $script:runs -ModsBeforeJson $script:modsJson -Tx 'tx-f2' -ExpectedPhaseCount 2
} catch { $aggregate = $_.ToString() }
$record = Read-FinalRecord $script:state.txDir
if ($record.passed -ne $false) { throw 'Scenario 2: catalog assertion failure did not fail the record.' }
if (@($record.ownedSavesDeleted).Count -ne 1 -or (Test-Path -LiteralPath $save.path -PathType Leaf)) { throw 'Scenario 2: proven owned save was not deleted.' }
$stageStates = Read-StageStates $record
if ($stageStates['owned-save-cleanup'] -cne 'succeeded' -or $stageStates['catalog-disposal'] -cne 'succeeded') { throw 'Scenario 2: successful stages were misreported.' }
if ($aggregate -eq $null -or -not $aggregate.Contains('FOREIGN')) { throw 'Scenario 2: aggregate missing the foreign-save cause.' }
Assert-LeasesReleased $catalog 'Scenario 2'
$script:checks++
