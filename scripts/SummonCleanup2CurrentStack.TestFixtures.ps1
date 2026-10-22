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
$script:fixturePackageReads=0
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
    $script:fixturePackageReads++
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

