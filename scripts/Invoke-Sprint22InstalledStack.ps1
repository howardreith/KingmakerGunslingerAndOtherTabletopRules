[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param(
    [Parameter(Mandatory=$true)][ValidateSet('working-save-expanded-summoning-prepare','working-save-expanded-summoning-verify-cleanup','working-save-expanded-summoning-verify-absent','working-save-expanded-summoning-creature-review')][string]$Scenario,
    [Parameter(Mandatory=$true)][ValidateSet('KMG_AUTOMATION_WORKING')][string]$SaveName,
    [Parameter(Mandatory=$true)][string]$PackagePath,
    [Parameter(Mandatory=$true)][string]$ExpectedVersion,
    [ValidateSet('ankylosaurus','armored-dinosaurs','theropods','giant-bodies','magical-beasts','griffon','giant-humanoids','giant-humanoids-ii')]
    [string]$PersistenceScope='ankylosaurus',
    [ValidateSet('sprints30-31-dispel-ui')][string]$ReviewCase,
    [string]$DevelopmentPurpose,
    [switch]$ExpandedSummoningDisabled,
    [ValidateRange(30,900)][int]$TimeoutSeconds=600
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
$root=Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$fixedDispelUi=$Scenario -ceq 'working-save-expanded-summoning-creature-review'
if($fixedDispelUi) {
    $uiParameters=@{creatures='trumpet-archon,astral-deva';reviewCase=$ReviewCase}
    if($SaveName -cne 'KMG_AUTOMATION_WORKING' -or
        -not(Test-KmgSprints30DispelUiParameters $uiParameters) -or
        $PSBoundParameters.ContainsKey('PersistenceScope') -or $ExpandedSummoningDisabled -or
        $TimeoutSeconds -ne 600) {
        throw 'Installed-stack UI requires only the exact read-only S30/S31 case, Working save, module ON and fixed600-second process budget; no transaction entered.'
    }
} elseif($PSBoundParameters.ContainsKey('ReviewCase') -or
    $Scenario -ieq 'working-save-expanded-summoning-creature-review') {
    throw 'Installed-stack reviewCase is restricted to the exact S30/S31 UI scenario; no transaction entered.'
}
if($ExpandedSummoningDisabled -and $Scenario -cne 'working-save-expanded-summoning-verify-cleanup') { throw 'Module OFF is restricted to the exact cleanup stage.' }
$development=if($fixedDispelUi) {
    if([string]::IsNullOrEmpty($DevelopmentPurpose)) { $false }
    elseif($DevelopmentPurpose -ceq 'sprints30-31-dispel-installed-ui') {
        [void](Assert-KmgDevelopmentPurpose $DevelopmentPurpose)
        $true
    } else { throw 'Installed-stack fixed UI development requires its exact S30/S31 purpose; no transaction entered.' }
} elseif($PersistenceScope -ceq 'giant-humanoids-ii') {
    Test-KmgSprint29DevelopmentPersistence -Scope $PersistenceScope -Purpose $DevelopmentPurpose
} elseif($PersistenceScope -ceq 'giant-humanoids') {
    Test-KmgSprint28DevelopmentPersistence -Scope $PersistenceScope -Purpose $DevelopmentPurpose
} else {
    Test-KmgGriffonDevelopmentPersistence -Scope $PersistenceScope -Purpose $DevelopmentPurpose
}
Assert-KmgNotRunning
$candidate=Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $root -DevelopmentPurpose $DevelopmentPurpose
if($development) {
    Assert-KmgDevelopmentSourceBinding -Receipt $candidate -Git (Get-KmgGitState -RepositoryRoot $root) `
        -SourceStateSha256 (Get-KmgSourceStateFingerprint -RepositoryRoot $root) -ExpectedVersion $ExpectedVersion
} else {
    Assert-KmgExplicitCandidateIdentity -Manifest $candidate -Git (Get-KmgGitState -RepositoryRoot $root) `
        -SourceStateSha256 (Get-KmgSourceStateFingerprint -RepositoryRoot $root) -ExpectedVersion $ExpectedVersion
}
$install='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker'
$mods=Join-Path $install 'Mods'
$stateRoot='C:/Dev/KingmakerGunslingerLab/compatibility-state'
$runPrefix=if($fixedDispelUi){'sprints30-31-ui-stack'}elseif($PersistenceScope -ceq 'giant-humanoids-ii'){'sprint29-stack'}elseif($PersistenceScope -ceq 'giant-humanoids'){'sprint28-stack'}elseif($PersistenceScope -ceq 'griffon'){'sprint27-stack'}elseif($PersistenceScope -ceq 'magical-beasts'){'sprint26-stack'}elseif($PersistenceScope -ceq 'giant-bodies'){'sprint25-stack'}elseif($PersistenceScope -ceq 'theropods'){'sprint24-stack'}elseif($PersistenceScope -ceq 'armored-dinosaurs'){'sprint23-stack'}else{'sprint22-stack'}
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
$resolution=[pscustomobject]@{profileId=$(if($fixedDispelUi){'gunslinger-current-installed-s30-31-ui'}else{'gunslinger-current-installed-sprint22'});runtimeCapable=$true;
    gunslinger=[pscustomobject]@{packagePath=$candidate.packagePath};runtimeMods=$runtimeMods}
$operation=if($fixedDispelUi){'stage the exact current installed stack for one fixed read-only S30/S31 Working UI request and restore exact originals'}else{'stage the current compatible installed stack for one Sprint22 working-save request and restore exact originals'}
if (-not $PSCmdlet.ShouldProcess($mods,$operation)) { return }
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
        candidateKind=$(if($development){'development'}else{'release-candidate'});releaseQualification=(-not $development);developmentPurpose=$DevelopmentPurpose;
        moduleDisabled=[bool]$ExpandedSummoningDisabled;persistenceScope=$(if($fixedDispelUi){$null}else{$PersistenceScope});reviewCase=$ReviewCase;candidate=$candidate;originalManifest=$transaction.originalManifest;gameSettings=$settingsSnapshot;restoration=$null}
    Write-KmgCompatibilityJsonAtomic (Join-Path $evidenceRoot 'installed-stack.json') $record
    $deployment=& (Join-Path $PSScriptRoot 'Deploy-Local.ps1') -PackagePath $candidate.packagePath -DevelopmentPurpose $DevelopmentPurpose -RuntimeLease $lease -PassThru -Confirm:$false
    $requestParameters=if($fixedDispelUi){$uiParameters}else{@{persistenceScope=$PersistenceScope}}
    $completionBudget=if($fixedDispelUi){180}else{$TimeoutSeconds}
    $arguments=@{Scenario=$Scenario;ExpectedVersion=$ExpectedVersion;SaveName=$SaveName;
        Parameters=$requestParameters;TimeoutSeconds=$TimeoutSeconds;
        ObserverStartupTimeoutSeconds=$TimeoutSeconds;CompletionTimeoutSeconds=$completionBudget;
        ExitAfterCompletion=$true;FingerprintTimeoutSeconds=$TimeoutSeconds;
        RuntimeLease=$lease;ReuseInstalledArtifact=$true;DevelopmentPurpose=$DevelopmentPurpose;
        PackagePath=$candidate.packagePath;DeploymentManifestPath=$deployment;PassThru=$true;Confirm=$false}
    $output=@(& (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') @arguments)
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
if($fixedDispelUi) {
    Add-Member -InputObject $receipt -NotePropertyName transactionRunId -NotePropertyValue $runId
    Add-Member -InputObject $receipt -NotePropertyName installedStackRecordPath -NotePropertyValue (Join-Path $evidenceRoot 'installed-stack.json')
}
Write-Output $receipt
