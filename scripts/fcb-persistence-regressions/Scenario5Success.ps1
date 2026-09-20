# Scenario 5: successful finalization remains successful and reports no
# failures.
$scenario = New-ScenarioRoot 'f5'
$script:state.txDir = Join-Path $scenario 'tx'
$catalog = New-RealCatalog $scenario
$owned = New-Object 'System.Collections.Generic.List[object]'
$save = New-OwnedSave $scenario 'clean transaction campaign bytes'
$owned.Add($save)
Reset-Stubs
Invoke-FinalizationStages -Owned $owned -Catalog $catalog -TransactionDirectory $script:state.txDir `
    -PrimaryFailure $null -Runs $script:runs -ModsBeforeJson $script:modsJson -Tx 'tx-f5' -ExpectedPhaseCount 2
$record = Read-FinalRecord $script:state.txDir
if ($record.passed -ne $true -or $record.cleanupFailed -ne $false -or $record.noGameProcess -ne $true -or
    $record.settingsRestored -ne $true -or $record.catalogClosed -ne $true) { throw 'Scenario 5: successful finalization misreported.' }
if (@($record.ownedSavesDeleted).Count -ne 1 -or @($record.finalizationFailures).Count -ne 0 -or
    (Test-Path -LiteralPath $save.path -PathType Leaf)) { throw 'Scenario 5: successful cleanup misreported.' }
$script:checks++
