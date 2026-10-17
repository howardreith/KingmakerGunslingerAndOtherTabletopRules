# Read-only native load plus the existing disposable Brown-Fur cast scenario.
# This does NOT qualify a cold load containing previously serialized Brown-Fur.
[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param(
    [Parameter(Mandatory=$true)][string]$PackagePath,
    [Parameter(Mandatory=$true)][ValidateSet('brown-fur-startup-order')][string]$DevelopmentPurpose,
    [Parameter(Mandatory=$true)][ValidateSet('KMG_AUTOMATION_WORKING')][string]$SaveName,
    [ValidateRange(120,600)][int]$TimeoutSeconds=300
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
. (Join-Path $PSScriptRoot 'BrownFurStartupProbe.Common.ps1')
$root=Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
Assert-KmgNotRunning
Assert-KmgUnelevated
Assert-KmgSteamAppId 640820
$null=Assert-KmgSteamExecutable 'C:/Program Files (x86)/Steam/steam.exe'
$candidate=Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $root -DevelopmentPurpose $DevelopmentPurpose
Assert-KmgDevelopmentSourceBinding -Receipt $candidate -Git (Get-KmgGitState $root) `
    -SourceStateSha256 (Get-KmgSourceStateFingerprint $root) -ExpectedVersion '0.0.157'
$install='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker'
$stateRoot='C:/Dev/KingmakerGunslingerLab/compatibility-state'
$runId='brown-fur-startup-'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,12)
$resolution=New-KmgBrownFurProbeResolution -ModsRoot (Join-Path $install 'Mods') `
    -BackupRoot (Join-Path $install "Mods.kmg-compat-$runId.original") -PackagePath $candidate.packagePath
$scenario='disposable-brown-fur-native-cast'
$null=Assert-KmgRuntimeScenarioPreflight -Scenario $scenario -ExpectedVersion '0.0.157' `
    -TimeoutSeconds $TimeoutSeconds -Parameters @{saveName=$SaveName}
if (-not $PSCmdlet.ShouldProcess($SaveName,'read-only native Load Game, disposable Brown-Fur cast, exact installed-stack/settings restoration')) { return }
$entered=$false;$lease=$null;$catalog=$null;$failure=$null;$receipt=$null
$evidence=Join-Path 'C:/Dev/KingmakerGunslingerLab/runtime-evidence' $runId
[void][IO.Directory]::CreateDirectory($evidence)
$settingsKey='Software\Owlcat Games\Pathfinder Kingmaker'
$settings=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($settingsKey,$false)
if ($null -eq $settings) { throw 'Current native game-settings key is missing.' }
try { $settingsSnapshot=@(Get-KmgFlatGameSettingsValues $settings) } finally { $settings.Dispose() }
$record=[ordered]@{schemaVersion=1;candidateKind='development';releaseQualification=$false
    candidate=$candidate;transactionRunId=$runId;scenario=$scenario;saveName=$SaveName
    action='native main-menu Load Game button -> exact working SaveSlot -> MainMenu.LoadGame'
    actualOtherMods=@($resolution.runtimeMods | Select-Object id,version)
    settingsSnapshot=$settingsSnapshot;launchReceipt=$null;restoration=$null}
Write-KmgCompatibilityJsonAtomic (Join-Path $evidence 'probe.json') $record
try {
    # All existing files, including WORKING, are OS read-leased. This probe
    # admits no save writes, new fixture names, edits or migration.
    $catalog=Open-KmgProtectedSaveCatalog -EvidenceDirectory $evidence
    $transaction=Enter-KmgCompatibilityTransaction -Resolution $resolution -KingmakerInstallDir $install -StateRoot $stateRoot -RunId $runId
    $entered=$true;$lease=Open-KmgCompatibilityRuntimeLease $runId $stateRoot
    $featurePath=Join-Path $install 'Mods/KingmakerGunslinger/FeatureModules.json'
    $originalFeatures=Join-Path $transaction.originalBackupPath 'KingmakerGunslinger/FeatureModules.json'
    $featureBytes=[IO.File]::ReadAllBytes($originalFeatures)
    $features=[Text.Encoding]::UTF8.GetString($featureBytes) | ConvertFrom-Json
    if ($features.schemaVersion -ne 12 -or $features.'brown-fur-transmuter' -ne $true) { throw 'Current Brown-Fur settings must already be enabled; no automatic setup.' }
    [IO.File]::WriteAllBytes($featurePath,$featureBytes)
    $deployArgs=@{PackagePath=$candidate.packagePath;DevelopmentPurpose=$DevelopmentPurpose;RuntimeLease=$lease;Confirm=$false}
    & (Join-Path $PSScriptRoot 'Deploy-Local.ps1') @deployArgs -WhatIf
    $deployment=& (Join-Path $PSScriptRoot 'Deploy-Local.ps1') @deployArgs -PassThru
    $launchArgs=@{Scenario=$scenario;ExpectedVersion='0.0.157';SaveName=$SaveName
        TimeoutSeconds=$TimeoutSeconds;ObserverStartupTimeoutSeconds=$TimeoutSeconds;CompletionTimeoutSeconds=$TimeoutSeconds
        ExitAfterCompletion=$true;RuntimeLease=$lease;ReuseInstalledArtifact=$true;PackagePath=$candidate.packagePath
        DeploymentManifestPath=$deployment;DevelopmentPurpose=$DevelopmentPurpose;PassThru=$true;Confirm=$false}
    & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') @launchArgs -WhatIf | Out-Host
    $output=@(& (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') @launchArgs)
    $receipt=Get-KmgExactRuntimeLaunchReceipt $output
    $record.launchReceipt=$receipt
    $result=Read-KmgCompatibilityJson (Join-Path $receipt.evidenceDirectory 'runtime-result.json')
    if ($result.status -cne 'PASS' -or $result.scenario -cne $scenario) { throw 'Exact Brown-Fur probe result is not PASS.' }
} catch { $failure=$_;$record.error=$_.Exception.Message }
finally {
    $deadline=[DateTime]::UtcNow.AddSeconds(120)
    while (@(Get-Process Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 250 }
    Assert-KmgNotRunning
    if ($lease) { $lease.Stream.Dispose() }
    if ($entered) {
        $boundary=Invoke-KmgIndependentBoundaryRestoration -RestoreSettings {
            $key=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($settingsKey,$true)
            if ($null -eq $key) { throw 'Native settings key disappeared.' }
            try { Restore-KmgFlatGameSettingsValues -Key $key -Snapshot $settingsSnapshot } finally { $key.Dispose() }
        } -RestoreInstallation { Restore-KmgCompatibilityTransaction -RunId $runId -StateRoot $stateRoot }
        $record.restoration=[ordered]@{settingsExact=($null -eq $boundary.settingsError)
            installationExact=($null -eq $boundary.installationError -and $null -ne $boundary.transaction -and $boundary.transaction.restorationVerified)}
        Write-KmgCompatibilityJsonAtomic (Join-Path $evidence 'probe.json') $record
        if ($boundary.installationError) { throw $boundary.installationError }
        if ($boundary.settingsError) { throw $boundary.settingsError }
    }
    if ($catalog) {
        try { $null=Assert-KmgProtectedSaveCatalog $catalog } finally { Close-KmgProtectedSaveCatalog $catalog }
    }
    Write-KmgCompatibilityJsonAtomic (Join-Path $evidence 'probe.json') $record
}
if ($failure) { throw $failure }
Write-Host "Brown-Fur read-only startup probe complete: $evidence; dependent-state cold persistence is still unqualified."
