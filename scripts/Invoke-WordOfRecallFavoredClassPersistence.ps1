[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.133',
    [Parameter(Mandatory = $true)][string]$DeploymentManifestPath,
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [switch]$AllowDirtyGit
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
$evidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
$modsRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods'
$settingsPath = Join-Path $modsRoot 'KingmakerGunslinger\FeatureModules.json'
$scenario = 'disposable-word-of-recall-favored-class-persistence'
$receiptName = 'word-of-recall-favored-class-persistence.json'
if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) { throw 'A Favored Class persistence transaction requires no existing game process.' }
if (-not $PSCmdlet.ShouldProcess('KMG_AUTOMATION_WORKING and one transaction-owned save', 'Run two fresh Steam processes with protected pre-existing saves and exact settings restoration')) { return }
$ConfirmPreference = 'None'
$tx = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '_' + [Guid]::NewGuid().ToString('N')
$transactionDirectory = Join-Path $evidenceRoot ('word-of-recall-fcb-persistence-' + $tx)
[void](New-Item -ItemType Directory -Path $transactionDirectory)
$owned = New-Object 'System.Collections.Generic.List[object]'
$runs = New-Object 'System.Collections.Generic.List[object]'
$catalog = $null
$failure = $null
$settingsBytes = [IO.File]::ReadAllBytes($settingsPath)
$settingsItem = Get-Item -LiteralPath $settingsPath
$settingsTimes = @($settingsItem.CreationTimeUtc, $settingsItem.LastWriteTimeUtc, $settingsItem.Attributes)
$previousPath = $settingsPath + '.previous'
$previousBytes = if (Test-Path -LiteralPath $previousPath -PathType Leaf) { [IO.File]::ReadAllBytes($previousPath) } else { $null }
$previousTimes = if ($null -ne $previousBytes) {
    $previousItem = Get-Item -LiteralPath $previousPath
    @($previousItem.CreationTimeUtc, $previousItem.LastWriteTimeUtc, $previousItem.Attributes)
} else { $null }
if ($null -ne $previousBytes) { [IO.File]::WriteAllBytes((Join-Path $transactionDirectory 'FeatureModules.previous.original.bin'), $previousBytes) }
$settingsOriginal = [Text.Encoding]::UTF8.GetString($settingsBytes) | ConvertFrom-Json
if ($settingsOriginal.schemaVersion -ne 11 -or $settingsOriginal.'teleportation-spells' -ne $true) {
    throw 'Favored Class persistence qualification requires the current valid settings with Teleportation enabled.'
}
[IO.File]::WriteAllBytes((Join-Path $transactionDirectory 'FeatureModules.original.json'), $settingsBytes)
function Write-PersistenceEvidence([string]$name, $value) {
    Write-KmgUtf8NoBom -Path (Join-Path $transactionDirectory $name) -Content (ConvertTo-Json -InputObject $value -Depth 100)
}
function Register-PersistenceOwnedSave([string]$runDirectory) {
    $receiptPath = Join-Path $runDirectory 'word-of-recall-fcb-persistence-owned-save.json'
    if (-not (Test-Path -LiteralPath $receiptPath -PathType Leaf)) { return }
    $entry = Get-Content -LiteralPath $receiptPath -Raw | ConvertFrom-Json
    $name = 'KMG_FCB_PERSISTENCE_' + $tx + '_prepare'
    $path = [IO.Path]::GetFullPath($entry.path)
    if ($entry.transactionId -cne $tx -or $entry.phase -cne 'prepare' -or $entry.name -cne $name -or
        $entry.existedBeforePreparation -ne $false -or $entry.lifecycle -cne 'native-prepared-before-write' -or
        [IO.Path]::GetFileName($path) -cnotmatch ('^Manual_[0-9]+_' + [Regex]::Escape($name) + '\.zks$') -or
        [IO.Path]::GetDirectoryName($path) -cne $catalog.Directory -or @($catalog.Files | Where-Object path -CEQ $path).Count -ne 0) {
        throw 'Disposable save ownership is ambiguous; no deletion is authorized.'
    }
    if (@($owned | Where-Object path -CEQ $path).Count -ne 0) { throw 'Duplicate save lifecycle receipt.' }
    $completedPath = Join-Path $runDirectory 'word-of-recall-favored-class-persistence.json'
    if (-not (Test-Path -LiteralPath $completedPath -PathType Leaf)) { throw 'Owned save lacks its completed-save receipt.' }
    $completed = Get-Content -LiteralPath $completedPath -Raw | ConvertFrom-Json
    if ($completed.savedInfo -eq $null -or [string]::IsNullOrWhiteSpace([string]$completed.savedInfo.sha256) -or
        [string]$completed.savedInfo.name -cne $name -or
        [IO.Path]::GetFullPath([string]$completed.savedInfo.path) -cne $path) {
        throw 'Completed-save receipt does not prove this owned save identity.'
    }
    $owned.Add([ordered]@{ name = $name; path = $path; phase = 'prepare'; runId = $entry.runId; receipt = $receiptPath
        completedReceiptPath = $completedPath; completedSha256 = [string]$completed.savedInfo.sha256
        createdSha256 = if (Test-Path -LiteralPath $path) { (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() } else { $null } })
    Write-PersistenceEvidence 'owned-saves.json' @($owned.ToArray())
}
function Remove-PersistenceOwnedSave {
    # Deletes one transaction-owned save only when the current file is
    # revalidated against its authoritative completed-save identity: exact
    # safe path inside the catalog directory, never part of the protected
    # inventory, current hash equal to both the completed receipt hash and
    # the creation hash. On replacement, mismatch, missing proof or any
    # error the file is PRESERVED and the failure recorded.
    param($Save, $Catalog, [System.Collections.Generic.List[object]]$Failures)
    $reason = $null
    $deleted = $false
    try {
        if ($null -eq $Save -or [string]::IsNullOrWhiteSpace([string]$Save.completedSha256) -or
            [string]::IsNullOrWhiteSpace([string]$Save.createdSha256)) {
            $reason = 'missing authoritative owned-save proof'
        } else {
            $safePath = [IO.Path]::GetFullPath([string]$Save.path)
            if ([IO.Path]::GetDirectoryName($safePath) -cne $Catalog.Directory -or
                @($Catalog.Files | Where-Object path -CEQ $safePath).Count -ne 0) {
                $reason = 'cleanup target escaped its proven transaction'
            } elseif (-not (Test-Path -LiteralPath $safePath -PathType Leaf)) {
                $reason = 'owned save file is absent'
            } else {
                $current = (Get-FileHash -LiteralPath $safePath -Algorithm SHA256).Hash.ToLowerInvariant()
                if ($current -cne $Save.completedSha256) { $reason = 'current file differs from its completed-save receipt (changed or replaced output)' }
                elseif ($current -cne $Save.createdSha256) { $reason = 'current file differs from its creation hash' }
                else {
                    $copy = Join-Path $Save.TransactionDirectory ([IO.Path]::GetFileName($safePath))
                    if (-not (Test-Path -LiteralPath $copy)) { Copy-Item -LiteralPath $safePath -Destination $copy }
                    Remove-Item -LiteralPath $safePath -Force
                    $deleted = $true
                }
            }
        }
    } catch { $reason = 'cleanup error: ' + $_.ToString() }
    if (-not $deleted) {
        $Failures.Add([ordered]@{ path = [string]$Save.path; reason = $reason; preserved = $true })
    }
    return $deleted
}

function Invoke-FinalizationStages {
    # Protected finalization: every remaining stage runs independently, a
    # stage failure never skips later stages, catalog handles are disposed
    # through their own protected step, and the final record is attempted
    # before the aggregate failure propagates. Save deletion and settings or
    # sidecar restoration remain prohibited while the game process is alive.
    # A preserved owned save is intentionally NOT excused in the final catalog
    # assertion: it appears as an unowned new save, that assertion failure is
    # recorded, and the preserved output stays on disk.
    param(
        [System.Collections.Generic.List[object]]$Owned,
        $Catalog,
        [string]$TransactionDirectory,
        $PrimaryFailure,
        [System.Collections.Generic.List[object]]$Runs,
        [string]$ModsBeforeJson,
        [string]$Tx,
        [int]$ExpectedPhaseCount
    )
    $stageFailures = New-Object 'System.Collections.Generic.List[object]'
    $stageOutcomes = New-Object 'System.Collections.Generic.List[object]'
    function Add-Outcome([string]$stage, [string]$state, [string]$detail) {
        $stageOutcomes.Add([ordered]@{ stage = $stage; state = $state; detail = $detail }) }
    function Add-Failure([string]$stage, [string]$detail) {
        $stageFailures.Add([ordered]@{ stage = $stage; detail = $detail }) }
    $ownedDeleted = New-Object 'System.Collections.Generic.List[string]'
    $preserved = New-Object 'System.Collections.Generic.List[object]'
    $settingsRestored = $false
    $noGameProcess = $false
    $catalogClosed = ($null -eq $Catalog)
    $preservation = $null
    $modsMatch = $null

    # Stage 1: guarded process exit. A live game process prohibits every
    # mutating stage below but never prohibits disposal or reporting.
    $mutationsProhibited = $false
    try {
        Wait-PersistenceExit
        $noGameProcess = $true
        Add-Outcome 'process-exit' 'succeeded' $null
    } catch {
        $mutationsProhibited = $true
        Add-Failure 'process-exit' $_.ToString()
        Add-Outcome 'process-exit' 'failed' $_.ToString()
    }

    # Stage 2: settings restoration (mutating).
    if ($mutationsProhibited) {
        Add-Outcome 'settings-restoration' 'skipped' 'prohibited while the game process is alive'
    } else {
        try {
            Restore-PersistenceSettings
            $settingsRestored = $true
            Add-Outcome 'settings-restoration' 'succeeded' $null
        } catch {
            Add-Failure 'settings-restoration' $_.ToString()
            Add-Outcome 'settings-restoration' 'failed' $_.ToString()
        }
    }

    # Stage 3: owned-save destructive cleanup (mutating). Preserved output is
    # never weakened or deleted to satisfy a later stage.
    if ($mutationsProhibited) {
        foreach ($save in $Owned) {
            $preserved.Add([ordered]@{ path = [string]$save.path
                reason = 'skipped: game process alive'; preserved = $true })
        }
        Add-Outcome 'owned-save-cleanup' 'skipped' 'prohibited while the game process is alive'
    } else {
        $cleanupFailures = New-Object 'System.Collections.Generic.List[object]'
        foreach ($save in $Owned) {
            $save.TransactionDirectory = $TransactionDirectory
            if (Remove-PersistenceOwnedSave -Save $save -Catalog $Catalog -Failures $cleanupFailures) {
                $ownedDeleted.Add([string]$save.path)
            }
        }
        foreach ($item in $cleanupFailures) {
            $preserved.Add($item)
            Add-Failure 'owned-save-cleanup' ('preserved ' + $item.path + ' (' + $item.reason + ')')
        }
        Add-Outcome 'owned-save-cleanup' ($(if ($cleanupFailures.Count -eq 0) { 'succeeded' } else { 'failed' })) (
            $(if ($cleanupFailures.Count -eq 0) { 'all proven owned saves deleted' }
              else { ($cleanupFailures | ForEach-Object { $_.path + ': ' + $_.reason }) -join '; ' }))
    }

    # Stage 4: protected-save catalog assertion (read-only). Preserved owned
    # output intentionally remains visible to this assertion.
    if ($null -ne $Catalog) {
        try {
            $preservation = Assert-KmgProtectedSaveCatalog -Catalog $Catalog
            Add-Outcome 'catalog-assertion' 'succeeded' $null
        } catch {
            Add-Failure 'catalog-assertion' $_.ToString()
            Add-Outcome 'catalog-assertion' 'failed' $_.ToString()
        }
    } else {
        Add-Outcome 'catalog-assertion' 'skipped' 'no protected catalog was opened'
    }

    # Stage 5: catalog handle disposal (non-mutating; always attempted).
    if ($null -ne $Catalog) {
        try {
            Close-KmgProtectedSaveCatalog -Catalog $Catalog
            $catalogClosed = $true
            Add-Outcome 'catalog-disposal' 'succeeded' $null
        } catch {
            Add-Failure 'catalog-disposal' $_.ToString()
            Add-Outcome 'catalog-disposal' 'failed' $_.ToString()
        }
    } else {
        Add-Outcome 'catalog-disposal' 'skipped' 'no protected catalog was opened'
    }

    # Stage 6: sidecar restoration (mutating).
    if ($mutationsProhibited) {
        Add-Outcome 'sidecar-restoration' 'skipped' 'prohibited while the game process is alive'
    } else {
        try {
            Restore-PersistenceSidecars
            Add-Outcome 'sidecar-restoration' 'succeeded' $null
        } catch {
            Add-Failure 'sidecar-restoration' $_.ToString()
            Add-Outcome 'sidecar-restoration' 'failed' $_.ToString()
        }
    }

    # Stage 7: complete Mods inventory comparison (read-only).
    try {
        $modsAfter = Get-PersistenceModsInventory
        $modsMatch = ($ModsBeforeJson -ceq ($modsAfter | ConvertTo-Json -Depth 10 -Compress))
        Write-PersistenceEvidence 'mods-after.json' $modsAfter
        Add-Outcome 'mods-inventory' ($(if ($modsMatch) { 'succeeded' } else { 'failed' })) $null
        if (-not $modsMatch) { Add-Failure 'mods-inventory' 'complete Mods inventory differs after the transaction' }
    } catch {
        Add-Failure 'mods-inventory' $_.ToString()
        Add-Outcome 'mods-inventory' 'failed' $_.ToString()
    }

    $passed = ($null -eq $PrimaryFailure) -and $stageFailures.Count -eq 0 -and
        $noGameProcess -and $settingsRestored -and $catalogClosed -and
        ($modsMatch -eq $true) -and $Runs.Count -eq $ExpectedPhaseCount

    $aggregateParts = New-Object 'System.Collections.Generic.List[string]'
    if ($null -ne $PrimaryFailure) { $aggregateParts.Add('primary: ' + $PrimaryFailure.ToString()) }
    foreach ($item in $stageFailures) { $aggregateParts.Add($item.stage + ': ' + $item.detail) }

    # Stage 8: the final record is attempted BEFORE any failure propagates.
    try {
        Write-PersistenceEvidence 'transaction-result.json' ([ordered]@{ schemaVersion = 1; transactionId = $Tx
            passed = $passed; phases = @($Runs.ToArray()); stageOutcomes = @($stageOutcomes.ToArray())
            finalizationFailures = @($stageFailures.ToArray())
            preservedSaves = $preservation; catalogClosed = $catalogClosed
            settingsRestored = $settingsRestored; completeModsTreeRestored = $modsMatch
            ownedSavesDeleted = @($ownedDeleted.ToArray())
            preservedOwnedSaves = @($preserved.ToArray())
            cleanupFailed = @($stageFailures | Where-Object stage -CEQ 'owned-save-cleanup').Count -ne 0
            noGameProcess = $noGameProcess
            primaryError = if ($null -eq $PrimaryFailure) { $null } else { $PrimaryFailure.ToString() }
            error = if ($aggregateParts.Count -eq 0) { $null } else { ($aggregateParts -join ' | ') } })
    } catch {
        # The evidence destination may be unwritable; the aggregate then
        # carries the reporting failure alongside every earlier cause.
        $aggregateParts.Add('result-write: ' + $_.ToString())
        throw ('Persistence finalization failed before its record could be written: ' + ($aggregateParts -join ' | '))
    }
    if (-not $passed) {
        throw ('Persistence finalization failed: ' + ($aggregateParts -join ' | '))
    }
}

$dllSha = (Get-FileHash -LiteralPath (Join-Path $modsRoot 'KingmakerGunslinger\KingmakerGunslinger.dll') -Algorithm SHA256).Hash.ToLowerInvariant()
$modsBefore = Get-PersistenceModsInventory
Write-PersistenceEvidence 'mods-before.json' $modsBefore
try {
    $catalog = Open-KmgProtectedSaveCatalog -EvidenceDirectory $transactionDirectory
    $workingPath = Join-Path $catalog.Directory 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($workingPath)
    try {
        $entry = $zip.GetEntry('header.json')
        if ($null -eq $entry) { throw 'Working save has no exact native header.' }
        $reader = New-Object IO.StreamReader($entry.Open())
        try { $header = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
    } finally { $zip.Dispose() }
    if ($header.Name -cne 'KMG_AUTOMATION_WORKING' -or $header.GameId -cne 'dce769e0-229c-4bfd-b8ea-e2d572bf8472' -or
        $header.GameName -cne 'Hedwirg' -or $header.Area -cne '2849fdde28fe50f4d935bf2cf3405051' -or @($header.PartyPortraits).Count -ne 3) {
        throw 'Original working save identity differs; no campaign may be loaded.'
    }
    $inputSave = [ordered]@{ name = $header.Name; file = [IO.Path]::GetFileName($workingPath); path = $workingPath
        sha256 = (Get-FileHash -LiteralPath $workingPath -Algorithm SHA256).Hash.ToLowerInvariant()
        gameId = $header.GameId; gameName = $header.GameName; areaName = 'JamandisMansion'; partyCount = 3 }
    $expected = $null; $previousResultPath = $null
    foreach ($phase in @('prepare', 'verify')) {
        $outputName = if ($phase -ceq 'verify') { $null } else { 'KMG_FCB_PERSISTENCE_' + $tx + '_prepare' }
        $planPath = Join-Path $transactionDirectory ('phase-' + $phase + '-plan.json')
        Write-PersistenceEvidence ('phase-' + $phase + '-plan.json') ([ordered]@{
            schemaVersion = 1; transactionId = $tx; phase = $phase; version = $ExpectedVersion; dllSha256 = $dllSha
            input = $inputSave; expected = $expected; outputSaveName = $outputName; previousResultPath = $previousResultPath })
        $beforeRuns = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory | ForEach-Object FullName)
        $phaseFailure = $null; $runDirectory = $null
        try {
            & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario $scenario -ExpectedVersion $ExpectedVersion `
                -SaveName $inputSave.name -Parameters @{ phase = $phase; planPath = $planPath } -TimeoutSeconds 900 `
                -CompletionTimeoutSeconds 600 -ExitAfterCompletion:$true -AllowDirtyGit:$AllowDirtyGit -Confirm:$false `
                -ReuseInstalledArtifact -DeploymentManifestPath $DeploymentManifestPath -PackagePath $PackagePath
            if ($LASTEXITCODE -ne 0) { throw "Native Favored Class persistence phase $phase failed." }
        } catch { $phaseFailure = $_ }
        finally {
            Wait-PersistenceExit
            $created = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory | Where-Object {
                $_.Name.EndsWith('-' + $scenario, [StringComparison]::Ordinal) -and $beforeRuns -cnotcontains $_.FullName })
            if ($created.Count -eq 1) {
                $runDirectory = $created[0].FullName
                Register-PersistenceOwnedSave $runDirectory
            }
            [void](Assert-KmgProtectedSaveCatalog -Catalog $catalog -OwnedPaths @($owned | ForEach-Object path))
        }
        if ($null -ne $phaseFailure) { throw $phaseFailure }
        if ($null -eq $runDirectory) { throw 'Favored Class persistence evidence directory is ambiguous.' }
        $previousResultPath = Join-Path $runDirectory 'runtime-result.json'
        $result = Get-Content -LiteralPath $previousResultPath -Raw | ConvertFrom-Json
        $receipt = Get-Content -LiteralPath (Join-Path $runDirectory $receiptName) -Raw | ConvertFrom-Json
        if ($result.status -cne 'PASS' -or $result.scenario -cne $scenario -or $result.loadedModVersion -cne $ExpectedVersion -or
            @($result.assertions | Where-Object status -CNE 'PASS').Count -ne 0 -or $receipt.runId -cne $result.runId -or
            $receipt.transactionId -cne $tx -or $receipt.phase -cne $phase -or $receipt.dllSha256 -cne $dllSha -or
            @($runs | Where-Object processId -EQ $receipt.processId).Count -ne 0) { throw 'Favored Class persistence phase is not an exact fresh-process structured PASS.' }
        $runs.Add([ordered]@{ phase = $phase; runId = $result.runId; processId = $receipt.processId; result = $previousResultPath; assertions = @($result.assertions).Count })
        Write-PersistenceEvidence 'phases.json' @($runs.ToArray())
        if ($phase -ceq 'prepare') {
            $expected = $receipt.expected; $inputSave = $receipt.savedInfo
            if ($inputSave.name -cne $outputName -or $inputSave.sha256 -cne (Get-FileHash -LiteralPath $inputSave.path -Algorithm SHA256).Hash.ToLowerInvariant()) {
                throw 'Native output no longer matches its exact completed save receipt.'
            }
            Copy-Item -LiteralPath $inputSave.path -Destination (Join-Path $transactionDirectory ([IO.Path]::GetFileName($inputSave.path)))
        }
    }
} catch { $failure = $_ }
finally {
    Invoke-FinalizationStages -Owned $owned -Catalog $catalog `
        -TransactionDirectory $transactionDirectory -PrimaryFailure $failure -Runs $runs `
        -ModsBeforeJson ($modsBefore | ConvertTo-Json -Depth 10 -Compress) -Tx $tx -ExpectedPhaseCount 2
}
Write-Host "PASS fresh-process Favored Class persistence prepare/verify; evidence=$transactionDirectory"
