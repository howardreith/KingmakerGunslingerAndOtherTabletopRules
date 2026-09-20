# Scenario 4: a process-exit failure skips every prohibited mutation but
# still disposes catalog handles and reports the blocked cleanup state.
$scenario = New-ScenarioRoot 'f4'
$script:state.txDir = Join-Path $scenario 'tx'
$catalog = New-RealCatalog $scenario
$owned = New-Object 'System.Collections.Generic.List[object]'
$save = New-OwnedSave $scenario 'clean transaction campaign bytes'
$owned.Add($save)
Reset-Stubs -WaitExitThrows:$true
$aggregate = $null
try {
    Invoke-FinalizationStages -Owned $owned -Catalog $catalog -TransactionDirectory $script:state.txDir `
        -PrimaryFailure $null -Runs $script:runs -ModsBeforeJson $script:modsJson -Tx 'tx-f4' -ExpectedPhaseCount 2
} catch { $aggregate = $_.ToString() }
$record = Read-FinalRecord $script:state.txDir
if ($record.noGameProcess -ne $false -or $record.settingsRestored -ne $false) { throw 'Scenario 4: blocked state hard-coded as successful.' }
if (-not (Test-Path -LiteralPath $save.path -PathType Leaf)) { throw 'Scenario 4: deletion was attempted despite the live process.' }
if ($script:state.settings -ne 0 -or $script:state.sidecars -ne 0) { throw 'Scenario 4: prohibited mutations ran.' }
$stageStates = Read-StageStates $record
if ($stageStates['settings-restoration'] -cne 'skipped' -or $stageStates['owned-save-cleanup'] -cne 'skipped' -or
    $stageStates['sidecar-restoration'] -cne 'skipped' -or $stageStates['catalog-disposal'] -cne 'succeeded') {
    throw 'Scenario 4: skipped prohibited stages or disposal misreported.' }
if (@($record.preservedOwnedSaves | Where-Object { ([string]$_.path) -ceq [string]$save.path }).Count -ne 1) { throw 'Scenario 4: blocked cleanup not reported as preserved.' }
if ($aggregate -eq $null -or -not $aggregate.Contains('did not terminate')) { throw 'Scenario 4: aggregate missing the process-exit cause.' }
Assert-LeasesReleased $catalog 'Scenario 4'
$script:checks++
