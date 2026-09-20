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
    Wait-PersistenceExit
    Restore-PersistenceSettings
    $cleanupFailures = New-Object 'System.Collections.Generic.List[object]'
    foreach ($save in $owned) {
        $save.TransactionDirectory = $transactionDirectory
        [void](Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $cleanupFailures)
    }
    if ($cleanupFailures.Count -ne 0 -and $null -eq $failure) {
        $failure = New-Object InvalidOperationException (
            'Owned-save cleanup preserved files instead of deleting: ' +
            (($cleanupFailures | ForEach-Object { $_.path + ' (' + $_.reason + ')' }) -join '; '))
    }
    $preservation = if ($null -ne $catalog) { Assert-KmgProtectedSaveCatalog -Catalog $catalog } else { $null }
    if ($null -ne $catalog) { Close-KmgProtectedSaveCatalog -Catalog $catalog }
    try { Restore-PersistenceSidecars } catch { if ($null -eq $failure) { $failure = $_ } }
    $modsAfter = Get-PersistenceModsInventory
    Write-PersistenceEvidence 'mods-after.json' $modsAfter
    $modsMatch = ($modsBefore | ConvertTo-Json -Depth 10 -Compress) -ceq ($modsAfter | ConvertTo-Json -Depth 10 -Compress)
    Write-PersistenceEvidence 'transaction-result.json' ([ordered]@{ schemaVersion = 1; transactionId = $tx
        passed = ($null -eq $failure -and $runs.Count -eq 2 -and $modsMatch); phases = @($runs.ToArray())
        preservedSaves = $preservation; settingsRestored = $true; completeModsTreeRestored = $modsMatch
        ownedSavesDeleted = @($owned | Where-Object { -not ($cleanupFailures | Where-Object path -CEQ $_.path) } | ForEach-Object path)
        preservedOwnedSaves = @($cleanupFailures.ToArray())
        cleanupFailed = ($cleanupFailures.Count -ne 0); noGameProcess = $true
        error = if ($null -eq $failure) { $null } else { $failure.ToString() } })
    if (-not $modsMatch) { throw 'Complete Mods inventory differs after the persistence transaction.' }
}
if ($null -ne $failure) { throw $failure }
Write-Host "PASS fresh-process Favored Class persistence prepare/verify; evidence=$transactionDirectory"
