[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.121',
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
$scenario = 'disposable-teleportation-persistence'
if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) { throw 'A persistence transaction requires no existing game process.' }
if (-not $PSCmdlet.ShouldProcess('KMG_AUTOMATION_WORKING and unique transaction-owned saves', 'Run four fresh Steam processes with protected pre-existing saves and exact settings restoration')) { return }
$ConfirmPreference = 'None'
$tx = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '_' + [Guid]::NewGuid().ToString('N')
$transactionDirectory = Join-Path $evidenceRoot ('teleportation-persistence-' + $tx)
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
    throw 'Persistence qualification requires the current valid settings with Teleportation enabled.'
}
[IO.File]::WriteAllBytes((Join-Path $transactionDirectory 'FeatureModules.original.json'), $settingsBytes)
function Write-PersistenceEvidence([string]$name, $value) {
    Write-KmgUtf8NoBom -Path (Join-Path $transactionDirectory $name) -Content ($value | ConvertTo-Json -Depth 100)
}
function Register-PersistenceOwnedSave([string]$runDirectory) {
    $receiptPath = Join-Path $runDirectory 'teleportation-persistence-owned-save.json'
    if (-not (Test-Path -LiteralPath $receiptPath -PathType Leaf)) { return }
    $entry = Get-Content -LiteralPath $receiptPath -Raw | ConvertFrom-Json
    $name = 'KMG_TELEPORT_PERSISTENCE_' + $tx + '_' + $entry.phase
    $path = [IO.Path]::GetFullPath($entry.path)
    if ($entry.transactionId -cne $tx -or $entry.phase -cnotin @('A', 'B', 'C') -or $entry.name -cne $name -or
        $entry.existedBeforePreparation -ne $false -or $entry.lifecycle -cne 'native-prepared-before-write' -or
        [IO.Path]::GetFileName($path) -cnotmatch ('^Manual_[0-9]+_' + [Regex]::Escape($name) + '\.zks$') -or
        [IO.Path]::GetDirectoryName($path) -cne $catalog.Directory -or @($catalog.Files | Where-Object path -CEQ $path).Count -ne 0) {
        throw 'Disposable save ownership is ambiguous; no deletion is authorized.'
    }
    if (@($owned | Where-Object path -CEQ $path).Count -ne 0) { throw 'Duplicate save lifecycle receipt.' }
    $owned.Add([ordered]@{ name = $name; path = $path; phase = $entry.phase; runId = $entry.runId; receipt = $receiptPath
        createdSha256 = if (Test-Path -LiteralPath $path) { (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() } else { $null } })
    Write-PersistenceEvidence 'owned-saves.json' @($owned.ToArray())
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
    $dllSha = (Get-FileHash -LiteralPath (Join-Path $modsRoot 'KingmakerGunslinger\KingmakerGunslinger.dll') -Algorithm SHA256).Hash.ToLowerInvariant()
    foreach ($phase in @('A', 'B', 'C', 'D')) {
        if ($phase -ceq 'C') {
            $off = [Text.Encoding]::UTF8.GetString($settingsBytes) | ConvertFrom-Json
            $off.'teleportation-spells' = $false
            Write-KmgUtf8NoBom -Path $settingsPath -Content ($off | ConvertTo-Json -Depth 20)
        }
        if ($phase -ceq 'D') { Restore-PersistenceSettings }
        $outputName = if ($phase -ceq 'D') { $null } else { 'KMG_TELEPORT_PERSISTENCE_' + $tx + '_' + $phase }
        $planPath = Join-Path $transactionDirectory ('phase-' + $phase + '-plan.json')
        Write-PersistenceEvidence ('phase-' + $phase + '-plan.json') ([ordered]@{
            schemaVersion = 1; transactionId = $tx; phase = $phase; version = $ExpectedVersion; dllSha256 = $dllSha
            input = $inputSave; expected = $expected; outputSaveName = $outputName; previousResultPath = $previousResultPath })
        $beforeRuns = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory | ForEach-Object FullName)
        $phaseFailure = $null; $runDirectory = $null
        try {
            & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario $scenario -ExpectedVersion $ExpectedVersion `
                -SaveName $inputSave.name -Parameters @{ phase = $phase; planPath = $planPath } -TimeoutSeconds 420 `
                -CompletionTimeoutSeconds 240 -ExitAfterCompletion:$true -AllowDirtyGit:$AllowDirtyGit -Confirm:$false `
                -ReuseInstalledArtifact -DeploymentManifestPath $DeploymentManifestPath -PackagePath $PackagePath
            if ($LASTEXITCODE -ne 0) { throw "Native persistence phase $phase failed." }
        } catch { $phaseFailure = $_ }
        finally {
            Wait-PersistenceExit
            $created = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory | Where-Object {
                $_.Name.EndsWith('-' + $scenario, [StringComparison]::Ordinal) -and $beforeRuns -cnotcontains $_.FullName })
            if ($created.Count -eq 1) {
                $runDirectory = $created[0].FullName
                Register-PersistenceOwnedSave $runDirectory
                & (Join-Path $PSScriptRoot 'compatibility\Collect-KmgCompatibilityAttributionLog.ps1') `
                    -EvidenceDirectory $runDirectory -ConfigurationId ('teleportation-persistence-' + $phase) | Out-Null
            }
            [void](Assert-KmgProtectedSaveCatalog -Catalog $catalog -OwnedPaths @($owned | ForEach-Object path))
        }
        if ($null -ne $phaseFailure) { throw $phaseFailure }
        if ($null -eq $runDirectory) { throw 'Persistence phase evidence directory is ambiguous.' }
        $previousResultPath = Join-Path $runDirectory 'runtime-result.json'
        $result = Get-Content -LiteralPath $previousResultPath -Raw | ConvertFrom-Json
        $receipt = Get-Content -LiteralPath (Join-Path $runDirectory 'teleportation-persistence.json') -Raw | ConvertFrom-Json
        if ($result.status -cne 'PASS' -or $result.scenario -cne $scenario -or $result.loadedModVersion -cne $ExpectedVersion -or
            @($result.assertions | Where-Object status -CNE 'PASS').Count -ne 0 -or $receipt.runId -cne $result.runId -or
            $receipt.transactionId -cne $tx -or $receipt.phase -cne $phase -or $receipt.dllSha256 -cne $dllSha -or
            @($runs | Where-Object processId -EQ $receipt.processId).Count -ne 0) { throw 'Persistence phase is not an exact fresh-process structured PASS.' }
        $runs.Add([ordered]@{ phase = $phase; runId = $result.runId; processId = $receipt.processId; result = $previousResultPath; assertions = @($result.assertions).Count })
        Write-PersistenceEvidence 'phases.json' @($runs.ToArray())
        $expected = $receipt.finalSnapshot; $inputSave = $receipt.savedInfo
        if ($phase -cne 'D') {
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
    foreach ($save in $owned) {
        # Each path was proved absent in the initial inventory and recorded by
        # native PrepareSave before its first write. Never use prefix deletion.
        if (Test-Path -LiteralPath $save.path -PathType Leaf) {
            $safePath = [IO.Path]::GetFullPath($save.path)
            if ([IO.Path]::GetDirectoryName($safePath) -cne $catalog.Directory -or
                @($catalog.Files | Where-Object path -CEQ $safePath).Count -ne 0) { throw 'Owned-save cleanup target escaped its proven transaction.' }
            $copy = Join-Path $transactionDirectory ([IO.Path]::GetFileName($safePath))
            if (-not (Test-Path -LiteralPath $copy)) { Copy-Item -LiteralPath $safePath -Destination $copy }
            Remove-Item -LiteralPath $safePath -Force
        }
    }
    $preservation = if ($null -ne $catalog) { Assert-KmgProtectedSaveCatalog -Catalog $catalog } else { $null }
    if ($null -ne $catalog) { Close-KmgProtectedSaveCatalog -Catalog $catalog }
    try { Restore-PersistenceSidecars } catch { if ($null -eq $failure) { $failure = $_ } }
    $modsAfter = Get-PersistenceModsInventory
    Write-PersistenceEvidence 'mods-after.json' $modsAfter
    $modsMatch = ($modsBefore | ConvertTo-Json -Depth 10 -Compress) -ceq ($modsAfter | ConvertTo-Json -Depth 10 -Compress)
    Write-PersistenceEvidence 'transaction-result.json' ([ordered]@{ schemaVersion = 1; transactionId = $tx
        passed = ($null -eq $failure -and $runs.Count -eq 4 -and $modsMatch); phases = @($runs.ToArray())
        preservedSaves = $preservation; settingsRestored = $true; completeModsTreeRestored = $modsMatch
        ownedSavesDeleted = @($owned | ForEach-Object path); noGameProcess = $true
        error = if ($null -eq $failure) { $null } else { $failure.ToString() } })
    if (-not $modsMatch) { throw 'Complete Mods inventory differs after the persistence transaction.' }
}
if ($null -ne $failure) { throw $failure }
Write-Host "PASS fresh-process teleportation persistence A/B/C/D; evidence=$transactionDirectory"
