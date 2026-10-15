[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param(
    [Parameter(Mandatory=$true)][ValidateSet('working-save-expanded-summoning-prepare','working-save-expanded-summoning-verify-cleanup','working-save-expanded-summoning-verify-absent')][string]$Scenario,
    [Parameter(Mandatory=$true)][ValidateSet('KMG_AUTOMATION_WORKING')][string]$SaveName,
    [Parameter(Mandatory=$true)][string]$PackagePath,
    [Parameter(Mandatory=$true)][string]$ExpectedVersion,
    [ValidateSet('ankylosaurus','armored-dinosaurs','theropods')]
    [string]$PersistenceScope='ankylosaurus',
    [switch]$ExpandedSummoningDisabled,
    [ValidateRange(30,900)][int]$TimeoutSeconds=600
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
$root=Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
if($ExpandedSummoningDisabled -and $Scenario -cne 'working-save-expanded-summoning-verify-cleanup') { throw 'Module OFF is restricted to the exact cleanup stage.' }
Assert-KmgNotRunning
$candidate=Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $root
Assert-KmgExplicitCandidateIdentity -Manifest $candidate -Git (Get-KmgGitState -RepositoryRoot $root) `
    -SourceStateSha256 (Get-KmgSourceStateFingerprint -RepositoryRoot $root) -ExpectedVersion $ExpectedVersion
$install='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker'
$mods=Join-Path $install 'Mods'
$stateRoot='C:/Dev/KingmakerGunslingerLab/compatibility-state'
$runPrefix=if($PersistenceScope -ceq 'theropods'){'sprint24-stack'}elseif($PersistenceScope -ceq 'armored-dinosaurs'){'sprint23-stack'}else{'sprint22-stack'}
$runId=$runPrefix+'-'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,12)
$backup=Join-Path $install ("Mods.kmg-compat-$runId.original")
$runtimeMods=@(Get-ChildItem -LiteralPath $mods -Directory | Where-Object {$_.Name -cne 'KingmakerGunslinger'} | ForEach-Object {
    $infoPath=Join-Path $_.FullName 'Info.json'
    if(-not(Test-Path -LiteralPath $infoPath -PathType Leaf)) { throw 'Installed stack has an unidentified mod directory.' }
    $info=Get-Content -LiteralPath $infoPath -Raw | ConvertFrom-Json
    [pscustomobject]@{destinationName=$_.Name;sourceDirectory=(Join-Path $backup $_.Name);
        sourceManifest=@(Get-KmgCompatibilityDirectoryManifest $_.FullName);id=$info.Id;version=$info.Version}
})
if($runtimeMods.Count -ne 10 -or @($runtimeMods.id | Sort-Object -Unique).Count -ne 10) { throw 'Sprint22 working fixture requires the exact identifiable eleven-mod installed stack.' }
$resolution=[pscustomobject]@{profileId='gunslinger-current-installed-sprint22';runtimeCapable=$true;
    gunslinger=[pscustomobject]@{packagePath=$candidate.packagePath};runtimeMods=$runtimeMods}
if(-not $PSCmdlet.ShouldProcess($mods,'stage the current compatible installed stack for one Sprint22 working-save request and restore exact originals')) { return }
$entered=$false;$lease=$null;$failure=$null;$receipt=$null
$settingsKey='Software\Owlcat Games\Pathfinder Kingmaker'
$gameSettings=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($settingsKey,$false)
if($null -eq $gameSettings) { throw 'Current game settings key is missing; no transaction entered.' }
try {
    $settingsSnapshot=@(Get-KmgFlatGameSettingsValues -Key $gameSettings)
} finally { $gameSettings.Dispose() }
try {
    $transaction=Enter-KmgCompatibilityTransaction -Resolution $resolution -KingmakerInstallDir $install -StateRoot $stateRoot -RunId $runId
    $entered=$true;$lease=Open-KmgCompatibilityRuntimeLease $runId $stateRoot
    $settings=Join-Path $mods 'KingmakerGunslinger/FeatureModules.json'
    $originalSettings=Join-Path $backup 'KingmakerGunslinger/FeatureModules.json'
    if(-not(Test-Path -LiteralPath $originalSettings -PathType Leaf)) { throw 'Actual installed module settings are missing.' }
    $bytes=[IO.File]::ReadAllBytes($originalSettings)
    if($ExpandedSummoningDisabled) { $bytes=ConvertTo-KmgDisabledExpandedSummoningSettingsBytes -OriginalBytes $bytes }
    [IO.File]::WriteAllBytes($settings,$bytes)
    $evidenceRoot=Join-Path 'C:/Dev/KingmakerGunslingerLab/runtime-evidence' $runId
    [void][IO.Directory]::CreateDirectory($evidenceRoot)
    $record=[ordered]@{schemaVersion=1;transactionRunId=$runId;scenario=$Scenario;startedUtc=[DateTime]::UtcNow.ToString('o');
        actualInstalledMods=@($runtimeMods | ForEach-Object { [ordered]@{id=$_.id;version=$_.version;directory=$_.destinationName} });
        moduleDisabled=[bool]$ExpandedSummoningDisabled;persistenceScope=$PersistenceScope;candidate=$candidate;originalManifest=$transaction.originalManifest;gameSettings=$settingsSnapshot;restoration=$null}
    Write-KmgCompatibilityJsonAtomic (Join-Path $evidenceRoot 'installed-stack.json') $record
    $deployment=& (Join-Path $PSScriptRoot 'Deploy-Local.ps1') -PackagePath $candidate.packagePath -RuntimeLease $lease -PassThru -Confirm:$false
    $output=@(& (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario $Scenario -ExpectedVersion $ExpectedVersion `
        -SaveName $SaveName -Parameters @{persistenceScope=$PersistenceScope} -TimeoutSeconds $TimeoutSeconds `
        -ObserverStartupTimeoutSeconds $TimeoutSeconds -CompletionTimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
        -FingerprintTimeoutSeconds $TimeoutSeconds `
        -RuntimeLease $lease -ReuseInstalledArtifact -PackagePath $candidate.packagePath -DeploymentManifestPath $deployment -PassThru -Confirm:$false)
    $receipt=Get-KmgExactRuntimeLaunchReceipt -Output $output
    $record.launchReceipt=$receipt
    $result=Read-KmgCompatibilityJson (Join-Path $receipt.evidenceDirectory 'runtime-result.json')
    if($result.runId -cne $receipt.runId -or $result.scenario -cne $Scenario -or $result.status -cne 'PASS') { throw 'Targeted native installed-stack result is not PASS for this exact run.' }
} catch { $failure=$_;if(Test-Path variable:record){$record.error=$_.Exception.Message} }
finally {
    $exitDeadline=[DateTime]::UtcNow.AddSeconds(180)
    while(@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and [DateTime]::UtcNow -lt $exitDeadline) { Start-Sleep -Milliseconds 500 }
    if($lease) { $lease.Stream.Dispose() }
    if($entered) {
        Assert-KmgNotRunning
        $boundary=Invoke-KmgIndependentBoundaryRestoration -RestoreSettings {
            $gameSettings=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($settingsKey,$true)
            try {
            if($null -eq $gameSettings) { throw 'Native game-settings key disappeared during the owned transaction.' }
            Restore-KmgFlatGameSettingsValues -Key $gameSettings -Snapshot $settingsSnapshot
            } finally { if($gameSettings){$gameSettings.Dispose()} }
        } -RestoreInstallation { Restore-KmgCompatibilityTransaction -RunId $runId -StateRoot $stateRoot }
        $settingsFailure=$boundary.settingsError
        $restoreFailure=$boundary.installationError
        $restored=$boundary.transaction
        if(Test-Path variable:record) {
            $record.restoration=[ordered]@{verified=($null -ne $restored -and $restored.restorationVerified);
                gameSettingsExact=($null -eq $settingsFailure);transactionStatus=if($restored){$restored.status}else{'RESTORATION_FAILED'};
                settingsError=if($settingsFailure){$settingsFailure.Exception.Message}else{$null};
                installationError=if($restoreFailure){$restoreFailure.Exception.Message}else{$null};
                completedUtc=[DateTime]::UtcNow.ToString('o')}
            Write-KmgCompatibilityJsonAtomic (Join-Path $evidenceRoot 'installed-stack.json') $record
        }
        if($restoreFailure) { throw $restoreFailure }
        if($settingsFailure) { throw $settingsFailure }
        if(-not $restored.restorationVerified) { throw 'Exact installed-stack restoration failed.' }
    }
}
if($failure) { throw $failure }
Write-Output $receipt
