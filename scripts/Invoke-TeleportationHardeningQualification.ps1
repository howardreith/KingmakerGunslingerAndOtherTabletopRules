[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [ValidateSet('Coexistence', 'Native', 'Boundary')][string]$Scope = 'Coexistence',
    [string]$ExpectedVersion = '0.0.122',
    [Parameter(Mandatory = $true)][string]$DeploymentManifestPath,
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [switch]$AllowDirtyGit
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
. (Join-Path $PSScriptRoot 'FeatureModuleCatalog.ps1')
if (Get-Process Kingmaker -ErrorAction SilentlyContinue) { throw 'Hardening qualification requires no existing game process.' }
if (-not $PSCmdlet.ShouldProcess('KMG_AUTOMATION_WORKING, read only', 'Run guarded fresh Steam processes with protected saves and exact restoration of the selected qualification settings')) { return }
$ConfirmPreference = 'None'
$evidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
$transactionDirectory = Join-Path $evidenceRoot ('teleportation-hardening-' + $Scope.ToLowerInvariant() + '-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ'))
[void](New-Item -ItemType Directory -Path $transactionDirectory)
$modsRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods'
$settingsPath = Join-Path $modsRoot 'KingmakerGunslinger\FeatureModules.json'
$settingsBytes = [IO.File]::ReadAllBytes($settingsPath)
$item = Get-Item -LiteralPath $settingsPath
$settingsTimes = @($item.CreationTimeUtc, $item.LastWriteTimeUtc, $item.Attributes)
$settingsOriginal = [Text.Encoding]::UTF8.GetString($settingsBytes) | ConvertFrom-Json
if ($settingsOriginal.schemaVersion -ne 11 -or $settingsOriginal.'teleportation-spells' -ne $true) { throw 'The initial settings must be valid with Teleportation ON.' }
$previousPath = $settingsPath + '.previous'
$previousBytes = if (Test-Path -LiteralPath $previousPath -PathType Leaf) { [IO.File]::ReadAllBytes($previousPath) } else { $null }
$previousTimes = if ($null -ne $previousBytes) {
    $old = Get-Item -LiteralPath $previousPath
    @($old.CreationTimeUtc, $old.LastWriteTimeUtc, $old.Attributes)
} else { $null }
function Write-PersistenceEvidence([string]$name, $value) {
    Write-KmgUtf8NoBom -Path (Join-Path $transactionDirectory $name) -Content ($value | ConvertTo-Json -Depth 100)
}
[IO.File]::WriteAllBytes((Join-Path $transactionDirectory 'FeatureModules.original.json'), $settingsBytes)
if ($null -ne $previousBytes) { [IO.File]::WriteAllBytes((Join-Path $transactionDirectory 'FeatureModules.previous.original.bin'), $previousBytes) }
$modsBefore = Get-PersistenceModsInventory
Write-PersistenceEvidence 'mods-before.json' $modsBefore
$dllSha = (Get-FileHash -LiteralPath (Join-Path $modsRoot 'KingmakerGunslinger\KingmakerGunslinger.dll') -Algorithm SHA256).Hash.ToLowerInvariant()
$steps = if ($Scope -ceq 'Coexistence') {
    foreach ($off in @($false, $true)) {
        foreach ($scenario in @('disposable-teleportation-coexistence', 'disposable-teleportation-coexistence-gamepad')) {
            [pscustomobject]@{ scenario = $scenario; off = $off }
        }
    }
} elseif ($Scope -ceq 'Native') {
    foreach ($name in @('casting', 'interaction', 'gamepad', 'familiarity', 'destinations', 'travelers', 'resources', 'spellbook-ui', 'level-up', 'disabled')) {
        [pscustomobject]@{ scenario = ('disposable-teleportation-' + $name); off = ($name -ceq 'disabled') }
    }
    [pscustomobject]@{ scenario = 'working-save-smoke'; off = $false }
} else {
    foreach ($entry in @(Get-KmgFeatureModuleConfigurations -Boundary)) {
        $configuration = [ordered]@{ schemaVersion = 11 }
        $parameters = @{}
        foreach ($module in @(Get-KmgFeatureModuleCatalog)) {
            $value = [bool]$entry.Values[$module.RuntimeParameter]
            $configuration[$module.JsonKey] = $value
            $parameters[$module.RuntimeParameter] = $value
        }
        [pscustomobject]@{ scenario = 'observe-feature-module-settings'; off = (-not $configuration['teleportation-spells'])
            configuration = [pscustomobject]$configuration; parameters = $parameters; name = $entry.Name }
    }
    if (@(Get-KmgFeatureModuleConfigurations -Boundary).Count -ne 26) { throw 'Expected all 26 current module boundaries.' }
}
$runs = New-Object 'System.Collections.Generic.List[object]'
$writtenSettingsStates = New-Object 'System.Collections.Generic.List[object]'
$catalog = $null; $failure = $null; $restorationStartup = $null
try {
    $catalog = Open-KmgProtectedSaveCatalog -EvidenceDirectory $transactionDirectory
    foreach ($step in $steps) {
        Restore-PersistenceSettings
        if ($Scope -ceq 'Boundary') {
            Write-KmgUtf8NoBom -Path $settingsPath -Content ($step.configuration | ConvertTo-Json -Depth 20)
            $writtenSettingsStates.Add($step.configuration)
        } elseif ($step.off) {
            $config = [Text.Encoding]::UTF8.GetString($settingsBytes) | ConvertFrom-Json
            $config.'teleportation-spells' = $false
            Write-KmgUtf8NoBom -Path $settingsPath -Content ($config | ConvertTo-Json -Depth 20)
        }
        $beforeRuns = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory | ForEach-Object FullName)
        $runFailure = $null; $priorCount = $runs.Count
        try {
            $invokeArguments = @{ Scenario = $step.scenario; ExpectedVersion = $ExpectedVersion
                TimeoutSeconds = 420; CompletionTimeoutSeconds = 240; ExitAfterCompletion = $true
                AllowDirtyGit = [bool]$AllowDirtyGit; Confirm = $false; ReuseInstalledArtifact = $true
                DeploymentManifestPath = $DeploymentManifestPath; PackagePath = $PackagePath }
            if ($Scope -ceq 'Boundary') { $invokeArguments.Parameters = $step.parameters }
            else { $invokeArguments.SaveName = 'KMG_AUTOMATION_WORKING' }
            & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') @invokeArguments
            if ($LASTEXITCODE -ne 0) { throw 'Native hardening scenario failed.' }
        } catch { $runFailure = $_ }
        finally {
            Wait-PersistenceExit
            [void](Assert-KmgProtectedSaveCatalog -Catalog $catalog)
        }
        $created = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory | Where-Object {
            $_.Name.EndsWith('-' + $step.scenario, [StringComparison]::Ordinal) -and $beforeRuns -cnotcontains $_.FullName })
        if ($created.Count -eq 1) {
            $path = Join-Path $created[0].FullName 'runtime-result.json'
            if (Test-Path -LiteralPath $path) {
                $result = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
                $runs.Add([ordered]@{ scenario = $step.scenario; moduleOff = $step.off; runId = $result.runId; result = $path
                    status = $result.status; assertions = @($result.assertions).Count
                    configuration = if ($Scope -ceq 'Boundary') { $step.name } else { $null } })
                Write-PersistenceEvidence 'runs.json' @($runs.ToArray())
                if ($result.status -cne 'PASS' -or $result.scenario -cne $step.scenario -or $result.loadedModVersion -cne $ExpectedVersion -or
                    @($result.assertions | Where-Object status -CNE 'PASS').Count -ne 0) { throw 'Native hardening result is not an exact structured PASS.' }
            }
        }
        if ($null -ne $runFailure) { throw $runFailure }
        if ($runs.Count -ne ($priorCount + 1) -or $created.Count -ne 1) { throw 'Hardening evidence identity is ambiguous.' }
    }
} catch { $failure = $_ }
finally {
    Wait-PersistenceExit
    Restore-PersistenceSettings
    if ($Scope -ceq 'Boundary') {
        # Favored Class's native LoadDictionary postfix regenerates its diagnostic
        # loaded_blueprints.txt for the current enabled-module graph. A final
        # guarded startup in the original configuration regenerates the original
        # bytes naturally. No third-party code, settings or diagnostic is edited.
        $beforeRestoration = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory | ForEach-Object FullName)
        try {
            $originalParameters = Get-KmgOriginalModuleRuntimeParameters -Settings $settingsOriginal
            & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario observe-feature-module-settings `
                -ExpectedVersion $ExpectedVersion -Parameters $originalParameters -TimeoutSeconds 420 -ExitAfterCompletion:$true `
                -AllowDirtyGit:$AllowDirtyGit -Confirm:$false -ReuseInstalledArtifact -DeploymentManifestPath $DeploymentManifestPath -PackagePath $PackagePath
            Wait-PersistenceExit
            $created = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory | Where-Object {
                $_.Name.EndsWith('-observe-feature-module-settings', [StringComparison]::Ordinal) -and $beforeRestoration -cnotcontains $_.FullName })
            if ($created.Count -ne 1) { throw 'Original-configuration restoration startup is ambiguous.' }
            $path = Join-Path $created[0].FullName 'runtime-result.json'
            $result = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
            $restorationStartup = [ordered]@{ runId = $result.runId; result = $path; status = $result.status; assertions = @($result.assertions).Count }
            if ($result.status -cne 'PASS' -or $result.loadedModVersion -cne $ExpectedVersion -or
                @($result.assertions | Where-Object status -CNE 'PASS').Count -ne 0) { throw 'Original-configuration restoration startup failed.' }
        } catch { if ($null -eq $failure) { $failure = $_ } }
        finally { Wait-PersistenceExit; Restore-PersistenceSettings }
    }
    $preservation = if ($null -ne $catalog) { Assert-KmgProtectedSaveCatalog -Catalog $catalog } else { $null }
    if ($null -ne $catalog) { Close-KmgProtectedSaveCatalog -Catalog $catalog }
    try {
        $authorizedStates = $writtenSettingsStates.ToArray()
        Restore-PersistenceSidecars -AuthorizedSettingsStates $authorizedStates
    } catch { if ($null -eq $failure) { $failure = $_ } }
    $modsAfter = Get-PersistenceModsInventory
    Write-PersistenceEvidence 'mods-after.json' $modsAfter
    $modsMatch = ($modsBefore | ConvertTo-Json -Depth 10 -Compress) -ceq ($modsAfter | ConvertTo-Json -Depth 10 -Compress)
    Write-PersistenceEvidence 'transaction-result.json' ([ordered]@{ schemaVersion = 1; scope = $Scope; dllSha256 = $dllSha
        passed = ($null -eq $failure -and $runs.Count -eq @($steps).Count -and $modsMatch); runs = @($runs.ToArray())
        restorationStartup = $restorationStartup
        preservedSaves = $preservation; settingsRestored = $true; completeModsTreeRestored = $modsMatch; noGameProcess = $true
        error = if ($null -eq $failure) { $null } else { $failure.ToString() } })
    if (-not $modsMatch) { throw 'Complete Mods inventory differs after hardening qualification.' }
}
if ($null -ne $failure) { throw $failure }
Write-Host "PASS $Scope hardening qualification; evidence=$transactionDirectory"
