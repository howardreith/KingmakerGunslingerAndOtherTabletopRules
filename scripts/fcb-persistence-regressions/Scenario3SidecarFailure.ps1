# Scenario 3: a sidecar restoration failure is recorded without suppressing
# other safe cleanup.
$scenario = New-ScenarioRoot 'f3'
$script:state.txDir = Join-Path $scenario 'tx'
$catalog = New-RealCatalog $scenario
$owned = New-Object 'System.Collections.Generic.List[object]'
$save = New-OwnedSave $scenario 'clean transaction campaign bytes'
$owned.Add($save)
Reset-Stubs -SidecarThrows:$true
$aggregate = $null
try {
    Invoke-FinalizationStages -Owned $owned -Catalog $catalog -TransactionDirectory $script:state.txDir `
        -PrimaryFailure $null -Runs $script:runs -ModsBeforeJson $script:modsJson -Tx 'tx-f3' -ExpectedPhaseCount 2
} catch { $aggregate = $_.ToString() }
$record = Read-FinalRecord $script:state.txDir
$stageStates = Read-StageStates $record
if ($stageStates['sidecar-restoration'] -cne 'failed' -or $stageStates['catalog-disposal'] -cne 'succeeded' -or
    $stageStates['settings-restoration'] -cne 'succeeded' -or $stageStates['owned-save-cleanup'] -cne 'succeeded') {
    throw 'Scenario 3: sidecar failure suppressed or misreported other stages.' }
if ($record.settingsRestored -ne $true -or $record.catalogClosed -ne $true) { throw 'Scenario 3: achieved stages reported inaccurately.' }
if ($aggregate -eq $null -or -not $aggregate.Contains('sidecar restoration failed')) { throw 'Scenario 3: aggregate missing the sidecar cause.' }
Assert-LeasesReleased $catalog 'Scenario 3'
$script:checks++
