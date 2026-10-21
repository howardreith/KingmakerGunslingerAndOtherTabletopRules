<# No-game behavior tests. Actual mission orchestration, receipt validation,
   source binding, filesystem transactions, leases, protected save handles and
   independent restoration run in a new bounded artifact directory. Only the
   package-content validator/compiler provenance, Git query, registry open,
   process boundary, deployment and native child are fixture boundaries. The
   production wrapper still delegates strict package validation to the released
   Read-KmgBuildLocalManifest. No install, registry, game or real save is touched. #>
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
. (Join-Path $PSScriptRoot 'SummonCleanup2CurrentStack.Common.ps1')
$testClock=[Diagnostics.Stopwatch]::StartNew()
$testRoot=Join-Path (Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot) ('artifacts/cleanup2-currentstack-tests/'+[Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($testRoot)
$script:checks=0;$script:failures=New-Object 'Collections.Generic.List[string]'
$script:gitCommit='a'*40;$script:sourceHash='b'*64;$script:gitDirty=$false
$script:currentCase=$null
function Check([bool]$Condition,[string]$Name) {
    $script:checks++
    if(-not $Condition){$script:failures.Add($Name)}
}
function Refusal([scriptblock]$Action) {try{& $Action | Out-Null;return $null}catch{return $_.Exception.Message}}
function Json($Value,[string]$Path) {Write-KmgCompatibilityJsonAtomic $Path $Value}
function Get-KmgGitState {param($RepositoryRoot) [pscustomobject]@{Commit=$script:gitCommit;Status=@(if($script:gitDirty){' M changed'})}}
function Get-KmgSourceStateFingerprint {param($RepositoryRoot) $script:sourceHash}
function Read-KmgBuildLocalManifest {
    param($PackagePath,$RepositoryRoot,$DevelopmentPurpose)
    # Full development receipt checks remain real. The existing strict package
    # content validator is independently covered by Test-DevelopmentCandidate;
    # this fixture ZIP deliberately contains no product/proprietary assemblies.
    Read-KmgDevelopmentCandidate -PackagePath $PackagePath -RepositoryRoot $RepositoryRoot -Purpose $DevelopmentPurpose
}
function Assert-KmgNotRunning {if($script:currentCase -and $script:currentCase.running){throw 'Fixture native process is still running.'}}
function Assert-KmgCompatibilityNoKingmakerProcess {param([int[]]$KnownProcessIds) Assert-KmgNotRunning}
function Wait-KmgSummonCleanup2Exit {
    $script:currentCase.events.Add('wait-exit')
    Assert-KmgNotRunning
}
function Open-KmgSummonCleanup2Settings {
    param([string]$KeyPath,[bool]$Writable)
    if($Writable){$script:currentCase.events.Add('restore-settings')}
    if($Writable -and $script:currentCase.failSettings){throw 'Fixture settings restoration failed.'}
    return $script:currentCase.settings
}
function Invoke-KmgSummonCleanup2Deploy {
    param($Plan,$Lease,[string]$ScriptsDirectory)
    Assert-KmgRuntimeLease $Lease
    $script:currentCase.events.Add('deploy')
    $script:currentCase.lease=$Lease
    $script:currentCase.runId=$Lease.RunId
    Check ([IO.File]::ReadAllText((Join-Path $script:currentCase.environment.install 'Mods/root-intake.txt')) -ceq 'current top-level file') 'active-stack-preserves-root-files'
    Check ([IO.File]::ReadAllText((Join-Path $script:currentCase.environment.install 'Mods/KingmakerGunslinger/FeatureModules.json')) -ceq '{"ExpandedSummoning":true,"human":"current"}') 'active-stack-preserves-current-module-settings'
    if($script:currentCase.mode -ceq 'settings-foreign'){$script:currentCase.settings.Values['foreign']='unowned-value'}
    $path=Join-Path $script:currentCase.root 'deployment.json'
    Json @{transactionRunId=$Lease.RunId;packageSha256=$Plan.candidate.packageSha256} $path
    return $path
}
function Invoke-KmgSummonCleanup2Launch {
    param($Plan,$Lease,[string]$Deployment,[string]$ScriptsDirectory)
    Assert-KmgRuntimeLease $Lease
    $script:currentCase.events.Add('launch')
    $script:currentCase.settings.Values['original']='native-changed'
    $script:currentCase.settings.Values['native-new']='new-value'
    $blocked=$false
    try{[IO.File]::WriteAllText($script:currentCase.baselinePath,'forbidden fixture write')}catch [IO.IOException]{$blocked=$true}
    Check $blocked 'baseline-read-lease-blocks-native-write'
    switch($script:currentCase.mode) {
        'child-fail' {throw 'Fixture native child failed after entry.'}
        'settings-fail' {$script:currentCase.failSettings=$true}
        'sentinel-foreign' {
            $sentinel=Join-Path $script:currentCase.environment.install 'Mods/.kmg-compat-sentinel.json'
            $record=Read-KmgCompatibilityJson $sentinel;$record.runId='foreign-owner';Json $record $sentinel
        }
        'lease-foreign' {
            $Lease.Stream.Dispose();[IO.File]::WriteAllText($Lease.LockPath,'foreign-owner')
        }
        'save-new' {[IO.File]::WriteAllText((Join-Path $script:currentCase.environment.saveDirectory 'unowned-new.zks'),'foreign')}
        'backup-foreign' {
            $backup=Join-Path $script:currentCase.environment.install ("Mods.kmg-compat-$($Lease.RunId).original")
            $path=Join-Path $backup 'KingmakerGunslinger/FeatureModules.json'
            $script:currentCase.recoveryFile=[pscustomobject]@{path=$path;bytes=[IO.File]::ReadAllBytes($path);time=(Get-Item $path).LastWriteTimeUtc}
            [IO.File]::WriteAllText($path,'foreign mutation')
        }
        'running' {$script:currentCase.running=$true}
    }
    $runId='fixture-native-'+[Guid]::NewGuid().ToString('N')
    $dir=Join-Path $script:currentCase.root $runId
    $creatures=($Plan.creatureKeys -join ',')
    if($script:currentCase.mode -ceq 'wrong-receipt'){$creatures='gorgon'}
    Json @{runId=$runId;evidenceDirectory=$dir;scenario=$Plan.scenario;expectedModVersion=$Plan.expectedVersion;
        exitAfterCompletion=$true;parameters=@{saveName=$Plan.saveName;creatures=$creatures}} (Join-Path $dir 'runtime-request.json')
    Json @{runId=$runId;evidenceDirectory=$dir;scenario=$Plan.scenario;status='PASS'} (Join-Path $dir 'runtime-result.json')
    return [pscustomobject]@{kind='KmgRuntimeLaunchReceipt';runId=$runId;evidenceDirectory=$dir}
}
function New-SettingsFixture {
    $settings=[pscustomobject]@{Values=[ordered]@{original='human-original'}}
    $settings | Add-Member ScriptMethod GetSubKeyNames {return ,([string[]]@())}
    $settings | Add-Member ScriptMethod GetValueNames {return ,([string[]]@($this.Values.Keys | Sort-Object))}
    $settings | Add-Member ScriptMethod GetValueKind {param($Name) return [Microsoft.Win32.RegistryValueKind]::String}
    $settings | Add-Member ScriptMethod GetValue {param($Name,$Default,$Options) return $this.Values[$Name]}
    $settings | Add-Member ScriptMethod DeleteValue {param($Name,$Throw) $this.Values.Remove($Name)}
    $settings | Add-Member ScriptMethod SetValue {param($Name,$Value,$Kind) $this.Values[$Name]=$Value}
    $settings | Add-Member ScriptMethod Dispose {}
    return $settings
}
function New-Case([string]$Mode) {
    $root=Join-Path $testRoot ($Mode+'-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
    $repository=Join-Path $root 'repository';$install=Join-Path $root 'game';$saves=Join-Path $root 'saves'
    foreach($dir in @($repository,$saves,(Join-Path $install 'Mods'),(Join-Path $install 'Kingmaker_Data/Managed'))) {[void][IO.Directory]::CreateDirectory($dir)}
    [IO.File]::WriteAllText((Join-Path $repository 'Info.json'),'{"Id":"KingmakerGunslinger","Version":"0.0.163"}')
    [IO.File]::WriteAllText((Join-Path $install 'Kingmaker.exe'),'fixture, not executable')
    [IO.File]::WriteAllText((Join-Path $install 'Kingmaker_Data/Managed/Assembly-CSharp.dll'),'fixture, not assembly')
    for($i=0;$i -lt 16;$i++) {
        $id=if($i -eq 0){'KingmakerGunslinger'}else{'FixtureMod'+$i}
        $dir=Join-Path $install ('Mods/'+$id);[void][IO.Directory]::CreateDirectory($dir)
        if(-not($Mode -ceq 'missing-id' -and $i -eq 15)) {Json @{Id=$(if($Mode -ceq 'duplicate-id' -and $i -eq 15){'FixtureMod14'}else{$id});Version='actual-intake-'+$i} (Join-Path $dir 'Info.json')}
        [IO.File]::WriteAllText((Join-Path $dir 'settings.json'),'human-current-'+$i)
    }
    [IO.File]::WriteAllText((Join-Path $install 'Mods/root-intake.txt'),'current top-level file')
    [IO.File]::WriteAllText((Join-Path $install 'Mods/KingmakerGunslinger/FeatureModules.json'),'{"ExpandedSummoning":true,"human":"current"}')
    $working=Join-Path $saves 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    $baseline=Join-Path $saves 'Manual_298_KMG_AUTOMATION_BASELINE.zks'
    if($Mode -cne 'missing-working'){[IO.File]::WriteAllText($working,'unchanged working fixture')}
    if($Mode -cne 'missing-baseline'){[IO.File]::WriteAllText($baseline,'immutable baseline fixture')}
    if($Mode -ceq 'ambiguous-save'){[IO.File]::WriteAllText((Join-Path $saves 'Manual_400_KMG_AUTOMATION_WORKING.zks'),'duplicate')}
    $purpose=if($Mode -ceq 'baseline-positive'){'summon-cleanup-iteration2-baseline'}else{'summon-cleanup-iteration2'}
    $layout=Get-KmgDevelopmentCandidateLayout -RepositoryRoot $repository -Purpose $purpose -Commit ('a'*40) -Id KingmakerGunslinger -Version '0.0.163'
    [void][IO.Directory]::CreateDirectory($layout.Root)
    $payload=Join-Path $root 'payload/KingmakerGunslinger';[void][IO.Directory]::CreateDirectory($payload)
    [IO.File]::WriteAllText((Join-Path $payload 'Info.json'),'{"Id":"KingmakerGunslinger","Version":"0.0.163"}')
    [IO.File]::WriteAllText((Join-Path $payload 'FeatureModules.json'),'candidate settings')
    Compress-Archive -LiteralPath $payload -DestinationPath $layout.PackagePath
    [IO.File]::WriteAllLines($layout.TestLogPath,@('KMG_TEST_FILTER=fixture; selected 1 of 1 registered tests.','PASS fixture.contract','Completed 1 tests; failures=0. Registered 1; elapsed 0.1s.'))
    [IO.File]::WriteAllText($layout.ValidationSummaryPath,'fixture validation summary')
    $receipt=[ordered]@{schemaVersion=1;candidateKind='development';releaseQualification=$false;
        generator='scripts/Build-Local.ps1 -DevelopmentPurpose';purpose=$purpose;commit=('a'*40);branch='codex/fixture';sourceStateSha256=('b'*64);
        version='0.0.163';packagePath=$layout.PackagePath;packageSha256=(Get-KmgSha256 $layout.PackagePath);dllSha256=('c'*64);dllMvid=[Guid]::NewGuid().ToString();
        firearmManifestSha256=('d'*64);firearmSoundBankSha256=('e'*64);
        references=@{files=@($script:KmgPrivateReferencePaths | ForEach-Object {@{relativePath=$_.Replace('\','/');sha256=('f'*64)}})};
        validation=@{validator='tools/validate_development_candidate.py';purpose=$purpose;releaseQualification=$false;baselineVersion='0.0.163';identities=1;summarySha256=(Get-KmgSha256 $layout.ValidationSummaryPath)};
        selectedTests=(Read-KmgDomainTestEvidence -LogPath $layout.TestLogPath -Filter fixture)}
    Json $receipt $layout.ReceiptPath
    $environment=[pscustomobject]@{install=$install;stateRoot=(Join-Path $root 'state');evidenceRoot=(Join-Path $root 'evidence');
        saveDirectory=$saves;settingsKey='fixture-registry-key';scriptsDirectory=$PSScriptRoot;fixtureMode=$true}
    return [pscustomobject]@{root=$root;repository=$repository;environment=$environment;layout=$layout;purpose=$purpose;settings=(New-SettingsFixture);
        mode=$Mode;events=(New-Object 'Collections.Generic.List[string]');failSettings=$false;running=$false;runId=$null;lease=$null;
        baselinePath=$baseline;recoveryFile=$null;originalManifest=@(Get-KmgCompatibilityDirectoryManifest (Join-Path $install 'Mods'))}
}
function Plan($Case) {New-KmgSummonCleanup2Plan -RepositoryRoot $Case.repository -PackagePath $Case.layout.PackagePath -ExpectedVersion '0.0.163' -DevelopmentPurpose $Case.purpose -SaveName KMG_AUTOMATION_WORKING -CreatureKeys @('elephant','griffon')}
function Assert-Restored($Case,[string]$Name) {
    Check (Test-KmgCompatibilityManifestEqual $Case.originalManifest @(Get-KmgCompatibilityDirectoryManifest (Join-Path $Case.environment.install 'Mods'))) ($Name+'-exact-current-mods')
    Check (-not(Test-Path -LiteralPath (Join-Path $Case.environment.stateRoot 'compatibility.lock'))) ($Name+'-owned-lock-released')
}

$script:currentCase=New-Case 'positive'
$case=$script:currentCase;$plan=Plan $case
$receipt=Invoke-KmgSummonCleanup2Transaction $plan $case.environment
Check ($receipt.kind -ceq 'KmgRuntimeLaunchReceipt') 'positive-exact-receipt'
Check (($case.events -join ',') -ceq 'deploy,launch,wait-exit,restore-settings') 'positive-orchestration-order'
Check ($case.settings.Values.Count -eq 1 -and $case.settings.Values.original -ceq 'human-original') 'exact-settings-restored'
Assert-Restored $case 'positive-16-mod-intake'
$record=Get-ChildItem -LiteralPath $case.environment.evidenceRoot -Filter installed-stack.json -Recurse | ForEach-Object {Read-KmgCompatibilityJson $_.FullName}
Check ($record.actualInstalledMods.Count -eq 16 -and $record.restoration.verified -and $record.restoration.settingsAndSavesExact) 'receipt-binds-actual-intake-and-restoration'
$script:currentCase=New-Case 'baseline-positive';$case=$script:currentCase
$receipt=Invoke-KmgSummonCleanup2Transaction (Plan $case) $case.environment
Check ($receipt.kind -ceq 'KmgRuntimeLaunchReceipt') 'positive-baseline-development-purpose'
Assert-Restored $case 'baseline-development'

foreach($mode in @('child-fail','wrong-receipt','settings-fail','sentinel-foreign','lease-foreign','save-new','backup-foreign','running','settings-foreign',
    'missing-id','duplicate-id','missing-working','missing-baseline','ambiguous-save')) {
    $script:currentCase=New-Case $mode;$case=$script:currentCase
    $message=Refusal {Invoke-KmgSummonCleanup2Transaction (Plan $case) $case.environment}
    Check ($null -ne $message) ($mode+'-refused')
    if($mode -cin @('missing-id','duplicate-id','missing-working','missing-baseline','ambiguous-save')) {
        Check ($case.events -cnotcontains 'deploy' -and -not(Test-Path -LiteralPath $case.environment.stateRoot)) ($mode+'-no-entry-or-launch')
        Check (Test-KmgCompatibilityManifestEqual $case.originalManifest @(Get-KmgCompatibilityDirectoryManifest (Join-Path $case.environment.install 'Mods'))) ($mode+'-intake-untouched')
    } elseif($mode -cin @('child-fail','wrong-receipt','settings-fail','save-new')) {
        Assert-Restored $case $mode
        Check ($case.events -contains 'restore-settings') ($mode+'-independent-settings-attempted')
    } elseif($mode -ceq 'settings-foreign') {
        Assert-Restored $case $mode
        Check ($case.events -cnotcontains 'launch' -and $case.events -cnotcontains 'restore-settings') 'foreign-settings-no-launch-or-overwrite'
        Check ($case.settings.Values.foreign -ceq 'unowned-value') 'foreign-settings-retained'
    } elseif($mode -ceq 'running') {
        Check ($case.events -cnotcontains 'restore-settings' -and (Test-Path -LiteralPath (Join-Path $case.environment.stateRoot 'compatibility.lock'))) 'running-no-unsafe-restore'
        $case.running=$false;$case.lease.Stream.Dispose()
        Close-KmgProtectedSaveCatalog $script:KmgCleanup2PendingHandles.catalog
        [void](Restore-KmgCompatibilityTransaction -RunId $case.runId -StateRoot $case.environment.stateRoot -FixtureMode)
    } else {
        Check ($case.events -contains 'restore-settings') ($mode+'-settings-independent-of-installation-refusal')
        Check ($case.settings.Values.Count -eq 1 -and $case.settings.Values.original -ceq 'human-original') ($mode+'-settings-exact-despite-installation-refusal')
        Check (Test-Path -LiteralPath (Join-Path $case.environment.install ("Mods.kmg-compat-$($case.runId).original"))) ($mode+'-original-intake-kept')
        # Recover only the synthetic fault under this exact fixture boundary;
        # the failed orchestration receipt is retained unchanged.
        if($mode -ceq 'sentinel-foreign') {
            $path=Join-Path $case.environment.install 'Mods/.kmg-compat-sentinel.json';$s=Read-KmgCompatibilityJson $path;$s.runId=$case.runId;Json $s $path
        } elseif($mode -ceq 'lease-foreign') {[IO.File]::WriteAllText((Join-Path $case.environment.stateRoot 'compatibility.lock'),$case.runId)}
        elseif($mode -ceq 'backup-foreign') {[IO.File]::WriteAllBytes($case.recoveryFile.path,$case.recoveryFile.bytes);(Get-Item $case.recoveryFile.path).LastWriteTimeUtc=$case.recoveryFile.time}
        [void](Restore-KmgCompatibilityTransaction -RunId $case.runId -StateRoot $case.environment.stateRoot -FixtureMode)
    }
}

$script:currentCase=New-Case 'candidate-refusals';$case=$script:currentCase
$base=@{RepositoryRoot=$case.repository;PackagePath=$case.layout.PackagePath;ExpectedVersion='0.0.163';DevelopmentPurpose='summon-cleanup-iteration2';SaveName='KMG_AUTOMATION_WORKING';CreatureKeys=@('elephant')}
foreach($mutation in @('purpose','save','unknown-creature','uppercase-creature','duplicate-creature','typed-creature','dirty','commit','version','source','missing-receipt','bad-hash','missing-identity')) {
    $args=$base.Clone();$original=[IO.File]::ReadAllBytes($case.layout.ReceiptPath)
    switch($mutation) {
        purpose {$args.DevelopmentPurpose='other-purpose'}
        save {$args.SaveName='KMG_AUTOMATION_BASELINE'}
        'unknown-creature' {$args.CreatureKeys=@('wolf')}
        'uppercase-creature' {$args.CreatureKeys=@('Elephant')}
        'duplicate-creature' {$args.CreatureKeys=@('elephant','elephant')}
        'typed-creature' {$args.CreatureKeys=@(42)}
        dirty {$script:gitDirty=$true}
        commit {$script:gitCommit='c'*40}
        version {$args.ExpectedVersion='0.0.164'}
        source {$script:sourceHash='c'*64}
        'missing-receipt' {Move-Item -LiteralPath $case.layout.ReceiptPath -Destination ($case.layout.ReceiptPath+'.missing')}
        'bad-hash' {$r=Read-KmgCompatibilityJson $case.layout.ReceiptPath;$r.packageSha256='0'*64;Json $r $case.layout.ReceiptPath}
        'missing-identity' {$r=Read-KmgCompatibilityJson $case.layout.ReceiptPath;$r.PSObject.Properties.Remove('dllMvid');Json $r $case.layout.ReceiptPath}
    }
    Check ($null -ne (Refusal {New-KmgSummonCleanup2Plan @args})) ('candidate-'+$mutation+'-refused')
    $script:gitDirty=$false;$script:sourceHash='b'*64;$script:gitCommit='a'*40
    [IO.File]::WriteAllBytes($case.layout.ReceiptPath,$original)
}
foreach($unknown in @('Parameters','Scenario','CompatibilityProfileId','AllowDirtyGit','AllowForceTerminate','Environment')) {
    $args=@{PackagePath=$case.layout.PackagePath;ExpectedVersion='0.0.163';DevelopmentPurpose='summon-cleanup-iteration2';SaveName='KMG_AUTOMATION_WORKING';CreatureKeys=@('elephant')}
    $args[$unknown]='untrusted'
    Check ($null -ne (Refusal {& (Join-Path $PSScriptRoot 'Invoke-SummonCleanup2CurrentStack.ps1') @args -Confirm:$false})) ('public-unknown-'+$unknown+'-refused')
}
$result=[ordered]@{schemaVersion=1;kind='NoGameOrchestrationTests';checks=$checks;failures=@($failures.ToArray());
    elapsedSeconds=$testClock.Elapsed.TotalSeconds;evidenceDirectory=$testRoot;nativeRuntimeExecuted=$false}
Json $result (Join-Path $testRoot 'test-summary.json')
if($failures.Count){throw ('Cleanup2 orchestration tests failed: '+($failures -join ', '))}
Write-Output ($result | ConvertTo-Json -Depth 5)
