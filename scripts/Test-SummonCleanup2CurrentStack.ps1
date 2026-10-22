# Same-process fixture coverage; process-lifetime recovery is tested separately.
. (Join-Path $PSScriptRoot 'SummonCleanup2CurrentStack.TestFixtures.ps1')

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

foreach($mode in @('child-fail','wrong-receipt','settings-fail','sentinel-foreign','lease-foreign','save-new','backup-foreign','settings-foreign',
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
$tokens=$null;$parseErrors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'SummonCleanup2CurrentStack.Common.ps1'),[ref]$tokens,[ref]$parseErrors)
if($parseErrors.Count){throw 'Wrapper parse failed.'}
$function=$ast.Find({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq 'New-KmgSummonCleanup2Plan'},$true)
$assignment=$function.Body.Find({param($node) $node -is [Management.Automation.Language.AssignmentStatementAst] -and $node.Left -is [Management.Automation.Language.VariableExpressionAst] -and $node.Left.VariablePath.UserPath -ceq 'allowed'},$true)
$advertised=@($assignment.Right.FindAll({param($node) $node -is [Management.Automation.Language.StringConstantExpressionAst]},$true) | ForEach-Object Value)
foreach($key in $advertised){$planArguments=$base.Clone();$planArguments.CreatureKeys=@($key);$plan=New-KmgSummonCleanup2Plan @planArguments;Check ($plan.creatureKeys.Count -eq 1 -and $plan.creatureKeys[0] -ceq $key) ('advertised-'+$key+'-actual-plan-admitted')}
$planArguments=$base.Clone();$planArguments.CreatureKeys=$advertised
Check ((New-KmgSummonCleanup2Plan @planArguments).creatureKeys.Count -eq $advertised.Count) 'entire-advertised-roster-plan-admitted'
foreach($selection in @(@('frost-giant'),@($advertised+@('frost-giant')))) {
    $planArguments=$base.Clone();$planArguments.CreatureKeys=$selection;$readsBefore=$script:fixturePackageReads
    $message=Refusal {New-KmgSummonCleanup2Plan @planArguments}
    Check ($message -like 'frost-giant is a retained native option*reviewPhase=giant-lineup*') 'retained-frost-full-batch-rejected-with-existing-native-path'
    Check ($script:fixturePackageReads -eq $readsBefore -and $case.events.Count -eq 0 -and -not(Test-Path -LiteralPath $case.environment.stateRoot)) 'frost-rejected-before-package-installation-save-or-launch-entry'
}
foreach($mutation in @('purpose','save','unknown-creature','uppercase-creature','duplicate-creature','typed-creature','dirty','commit','version','source','missing-receipt','bad-hash','missing-identity')) {
    $planArguments=$base.Clone();$original=[IO.File]::ReadAllBytes($case.layout.ReceiptPath)
    switch($mutation) {
        purpose {$planArguments.DevelopmentPurpose='other-purpose'}
        save {$planArguments.SaveName='KMG_AUTOMATION_BASELINE'}
        'unknown-creature' {$planArguments.CreatureKeys=@('wolf')}
        'uppercase-creature' {$planArguments.CreatureKeys=@('Elephant')}
        'duplicate-creature' {$planArguments.CreatureKeys=@('elephant','elephant')}
        'typed-creature' {$planArguments.CreatureKeys=@(42)}
        dirty {$script:gitDirty=$true}
        commit {$script:gitCommit='c'*40}
        version {$planArguments.ExpectedVersion='0.0.164'}
        source {$script:sourceHash='c'*64}
        'missing-receipt' {Move-Item -LiteralPath $case.layout.ReceiptPath -Destination ($case.layout.ReceiptPath+'.missing')}
        'bad-hash' {$r=Read-KmgCompatibilityJson $case.layout.ReceiptPath;$r.packageSha256='0'*64;Json $r $case.layout.ReceiptPath}
        'missing-identity' {$r=Read-KmgCompatibilityJson $case.layout.ReceiptPath;$r.PSObject.Properties.Remove('dllMvid');Json $r $case.layout.ReceiptPath}
    }
    Check ($null -ne (Refusal {New-KmgSummonCleanup2Plan @planArguments})) ('candidate-'+$mutation+'-refused')
    $script:gitDirty=$false;$script:sourceHash='b'*64;$script:gitCommit='a'*40
    [IO.File]::WriteAllBytes($case.layout.ReceiptPath,$original)
}
foreach($unknown in @('Parameters','Scenario','CompatibilityProfileId','AllowDirtyGit','AllowForceTerminate','Environment')) {
    $planArguments=@{PackagePath=$case.layout.PackagePath;ExpectedVersion='0.0.163';DevelopmentPurpose='summon-cleanup-iteration2';SaveName='KMG_AUTOMATION_WORKING';CreatureKeys=@('elephant')}
    $planArguments[$unknown]='untrusted'
    Check ($null -ne (Refusal {& (Join-Path $PSScriptRoot 'Invoke-SummonCleanup2CurrentStack.ps1') @planArguments -Confirm:$false})) ('public-unknown-'+$unknown+'-refused')
}
$result=[ordered]@{schemaVersion=1;kind='NoGameOrchestrationTests';checks=$checks;failures=@($failures.ToArray());
    elapsedSeconds=$testClock.Elapsed.TotalSeconds;evidenceDirectory=$testRoot;nativeRuntimeExecuted=$false}
Json $result (Join-Path $testRoot 'test-summary.json')
if($failures.Count){throw ('Cleanup2 orchestration tests failed: '+($failures -join ', '))}
Write-Output ($result | ConvertTo-Json -Depth 5)
