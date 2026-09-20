[CmdletBinding()]
param([switch]$ExpectReviewedFailure)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

# Execute the actual generic script, including preflight, reusable artifact
# validation, request/result/evidence handling and coordination. Debugger hooks
# redirect only fixed machine paths into this disposable fixture and replace
# the external Steam/process boundary. No lease or file operation is mocked.
$root = Split-Path $PSScriptRoot
$testRoot = Join-Path $root ('artifacts/local-runtime/lease-tests-' + [Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($testRoot)
$package = Join-Path $testRoot 'candidate.zip'
$version = (Get-KmgModInfo $root).Version
$candidate = Join-Path $root "artifacts/local-runtime/$version/KingmakerGunslinger-$version-local-runtime.zip"
Copy-Item -LiteralPath $candidate -Destination $package
Expand-Archive -LiteralPath $package -DestinationPath (Join-Path $testRoot 'installed')
$live = Join-Path $testRoot 'installed/KingmakerGunslinger'
$git = Get-KmgGitState $root
$fingerprint = Get-KmgSourceStateFingerprint $root
$build = Get-Content -LiteralPath ($candidate + '.build-local.json') -Raw | ConvertFrom-Json
$build.packagePath = $package; $build.commit = $git.Commit
$build.sourceStateSha256 = $fingerprint
$build | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath ($package + '.build-local.json') -Encoding UTF8
$deployment = Join-Path $testRoot 'deployment.json'
[ordered]@{schemaVersion=2;packagePath=$package;packageSha256=$build.packageSha256
    commit=$git.Commit;sourceStateSha256=$fingerprint;version=$version
    dllSha256=$build.dllSha256;dllMvid=$build.dllMvid;liveModDirectory=$live
    firearmManifestSha256=$build.firearmManifestSha256;firearmSoundBankSha256=$build.firearmSoundBankSha256
    deployedFirearmManifestSha256=$build.firearmManifestSha256;deployedFirearmSoundBankSha256=$build.firearmSoundBankSha256
    firearmBundleSha256=Get-KmgSha256 (Join-Path $live 'assets/bundles/kingmakergunslinger.firearms')
} | ConvertTo-Json | Set-Content -LiteralPath $deployment -Encoding UTF8

$global:kmgLeaseTest = @{root=$testRoot;live=$live;running=$false;ownerGone=$false;launches=0;case='';stateRoot='';evidenceRoot='';interrupt=$false;probe=$null;ownerReused=$false}
$global:kmgLeaseTest.process = [pscustomobject]@{Id=900001;ProcessName='Kingmaker';StartTime=[DateTime]::Now;HasExited=$false;ExitCode=0}
$global:kmgLeaseTest.process | Add-Member ScriptMethod Refresh {
    if ($global:kmgLeaseTest.interrupt) { throw 'Controlled external process observation interruption.' }
    $this.HasExited = -not $global:kmgLeaseTest.running
}
function global:Get-Process {
    [CmdletBinding()]param([string]$Name,[int]$Id)
    if ($Name -ceq 'Kingmaker') {
        if ($global:kmgLeaseTest.probe) { & $global:kmgLeaseTest.probe }
        if ($global:kmgLeaseTest.running) { $global:kmgLeaseTest.process }; return
    }
    if ($Id -eq 900001) { if ($global:kmgLeaseTest.running) { $global:kmgLeaseTest.process }; return }
    if ($Id -eq $PID -and $global:kmgLeaseTest.ownerGone) {
        if ($global:kmgLeaseTest.ownerReused) { [pscustomobject]@{Id=$PID;StartTime=[DateTime]::Now.AddDays(1)} }; return
    }
    Microsoft.PowerShell.Management\Get-Process @PSBoundParameters
}
$launchBoundary = {
    param($SteamPath,$AppId,$RequestPath,$PreLaunchProcesses,$SteamStartupTimeoutSeconds,$GameStartupTimeoutSeconds)
    $global:kmgLeaseTest.launches++
    $request = Get-Content -LiteralPath $RequestPath -Raw | ConvertFrom-Json
    $global:kmgLeaseTest.running = -not $request.exitAfterCompletion
    if ($global:kmgLeaseTest.case -eq 'interruption') { $global:kmgLeaseTest.interrupt=$true; $global:kmgLeaseTest.running=$true }
    elseif ($global:kmgLeaseTest.case -eq 'timeout') { $global:kmgLeaseTest.running=$true }
    else {
        $result = [ordered]@{schemaVersion=1;runId=$request.runId;scenario=$request.scenario
            loadedModVersion=$request.expectedModVersion;status='PASS';evidenceDirectory=$request.evidenceDirectory}
        Write-KmgUtf8NoBom -Path (Join-Path $request.evidenceDirectory 'runtime-result.json') -Content ($result | ConvertTo-Json)
    }
    $result = [pscustomobject]@{steamExecutable=$SteamPath;steamAppId=$AppId;steamProcessId=900002
        kingmakerProcess=$global:kmgLeaseTest.process;kingmakerProcessId=900001
        kingmakerStartedAtUtc=$global:kmgLeaseTest.process.StartTime.ToUniversalTime();sanitizedLaunchArguments=@('-applaunch','640820','-kmgRuntimeTestRequest',$RequestPath)}
    $result.PSObject.TypeNames.Insert(0,'KingmakerGunslinger.RuntimeLaunchResult')
    return $result
}
$global:kmgLeaseTest.launchBoundary = $launchBoundary
function Add-FixtureHook([string]$File,[string]$Line,[scriptblock]$Action) {
    $path = Join-Path $PSScriptRoot $File
    $lines = [IO.File]::ReadAllLines($path)
    $indices = @(0..($lines.Length-1) | Where-Object { $lines[$_] -ceq $Line })
    if ($indices.Count -lt 1) { throw "Missing fixture-path hook: $File : $Line" }
    Set-PSBreakpoint -Script $path -Line ($indices[0]+1) -Action $Action
}
$hooks = @(
    Add-FixtureHook 'Invoke-KingmakerRuntimeTest.ps1' '$scenarioMetadata = Get-KmgRuntimeScenarioMetadata -Scenario $Scenario' {
        Set-Variable KmgRuntimeEvidenceRoot $global:kmgLeaseTest.evidenceRoot -Scope 1 -WhatIf:$false
        Set-Item Function:Start-KmgSteamKingmaker -Value $global:kmgLeaseTest.launchBoundary -WhatIf:$false
    }
    Add-FixtureHook 'RuntimeCoordination.Common.ps1' '    if ($null -ne $ParentLease) {' {
        Set-Variable StateRoot $global:kmgLeaseTest.stateRoot -Scope 1
    }
    Add-FixtureHook 'RuntimeHarness.Common.ps1' '    [void](Assert-KmgPathWithin -Path $deploymentPath -Root $requiredRoot)' {
        Set-Variable requiredRoot $global:kmgLeaseTest.root -Scope 1
    }
    Add-FixtureHook 'RuntimeHarness.Common.ps1' '    if (-not $live.Equals($expectedLive, [StringComparison]::OrdinalIgnoreCase)) {' {
        Set-Variable expectedLive $global:kmgLeaseTest.live -Scope 1
    }
    Add-FixtureHook 'Collect-Runtime-Evidence.ps1' 'if (-not [IO.Path]::GetFullPath($EvidenceRoot).TrimEnd(''\'').Equals($requiredEvidenceRoot, [StringComparison]::OrdinalIgnoreCase)) {' {
        Set-Variable requiredEvidenceRoot $global:kmgLeaseTest.evidenceRoot -Scope 1
        Set-Variable EvidenceRoot $global:kmgLeaseTest.evidenceRoot -Scope 1
        Set-Variable LiveModDirectory $global:kmgLeaseTest.live -Scope 1
        Set-Variable GameDirectory $global:kmgLeaseTest.root -Scope 1
    }
)
$completionPath=Join-Path $PSScriptRoot 'Complete-KingmakerRuntimeLease.ps1'
if (Test-Path -LiteralPath $completionPath) {
    $hooks += Add-FixtureHook 'RuntimeCoordination.Common.ps1' "    `$statePath=Join-Path (Join-Path `$StateRoot `$RunId) 'runtime-lease.json'" {
        Set-Variable StateRoot $global:kmgLeaseTest.stateRoot -Scope 1 -WhatIf:$false
    }
}
$checks=[Collections.Generic.List[string]]::new()
function Check([bool]$Condition,[string]$Name) {
    if (-not $Condition) { throw "FAIL $Name" }
    $checks.Add($Name)
}
function Reject([scriptblock]$Action,[string]$Pattern,[string]$Name) {
    $errorObserved=$null
    try { & $Action } catch { $errorObserved=$_.Exception.Message }
    Check ($null -ne $errorObserved -and $errorObserved -like $Pattern) $Name
}
function Fixture-Snapshot([string]$Directory) {
    return (@(Get-ChildItem -LiteralPath $Directory -Recurse -File | Sort-Object FullName | ForEach-Object {
        $_.FullName + ':' + (Get-KmgCompatibilitySha256 $_.FullName)
    }) -join "`n")
}
$results = @()
try {
    $cases=if ($ExpectReviewedFailure) { @('leave-open','automatic-exit','interruption','what-if') }
        else { @('leave-open','automatic-exit','interruption','timeout','inherited','busy','what-if') }
    foreach ($case in $cases) {
        $global:kmgLeaseTest.case=$case; $global:kmgLeaseTest.running=$false; $global:kmgLeaseTest.interrupt=$false
        $global:kmgLeaseTest.ownerGone=$false; $global:kmgLeaseTest.ownerReused=$false
        $global:kmgLeaseTest.stateRoot=Join-Path $testRoot ($case + '/coordination')
        $global:kmgLeaseTest.evidenceRoot=Join-Path $testRoot ($case + '/evidence')
        $parent=$null; $lease=$null
        if ($case -eq 'inherited') {
            $parent=Enter-KmgRuntimeLease -Purpose 'Magic Circle persistence settings' -StateRoot $global:kmgLeaseTest.stateRoot
            $lease=$parent.Lease
            $lease.State.settings=@{backupPath='disposable parent obligation'}
            $lease.State.recoveryRequired=$true
            Write-KmgRuntimeLeaseState $lease
        }
        if ($case -eq 'busy') { [void](Acquire-KmgCompatibilityLock $global:kmgLeaseTest.stateRoot 'foreign-owner') }
        $before=Fixture-Snapshot $testRoot; $launchCount=$global:kmgLeaseTest.launches
        $errorText=$null
        try {
            & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario mod-load-smoke -ExpectedVersion $version `
                -ExitAfterCompletion:($case -eq 'automatic-exit') -ReuseInstalledArtifact -DeploymentManifestPath $deployment `
                -PackagePath $package -AllowDirtyGit -Confirm:$false -TimeoutSeconds 5 -WhatIf:($case -eq 'what-if') -RuntimeLease $lease
        } catch { $errorText=$_.Exception.Message }
        $states=@(if (Test-Path -LiteralPath $global:kmgLeaseTest.stateRoot) { Get-ChildItem -LiteralPath $global:kmgLeaseTest.stateRoot -Recurse -Filter runtime-lease.json | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json } })
        $lock=Test-Path -LiteralPath (Join-Path $global:kmgLeaseTest.stateRoot 'compatibility.lock')
        $status=if ($states.Count) { $states[0].status } else { '<none>' }
        $results += [pscustomobject]@{case=$case;error=$errorText;status=$status;lock=$lock}
        if ($ExpectReviewedFailure) { continue }
        switch ($case) {
            'what-if' {
                Check ($null -eq $errorText -and $global:kmgLeaseTest.launches -eq $launchCount -and -not $lock -and
                    (Fixture-Snapshot $testRoot) -ceq $before) 'generic-WhatIf-no-file-mutation-or-launch'
            }
            'busy' {
                Check ($errorText -like '*lock already exists*' -and $global:kmgLeaseTest.launches -eq $launchCount -and
                    (Fixture-Snapshot $testRoot) -ceq $before) 'generic-rejects-foreign-owner-before-launch'
            }
            'automatic-exit' { Check ($null -eq $errorText -and $status -ceq 'Completed' -and -not $lock) 'automatic-exit-PASS-releases-ordinary-lease' }
            'inherited' {
                Check ($null -eq $errorText -and $lock -and $status -ceq 'Active' -and $states[0].recoveryRequired -and
                    $null -ne $states[0].settings -and (Get-KmgCompatibilitySha256 $lease.StatePath) -ceq $lease.StateSha256) 'generic-inherited-ownership-leaves-parent-restoration-pending'
                Assert-KmgRuntimeLease $lease
                $global:kmgLeaseTest.running=$false; $lease.Stream.Dispose(); $global:kmgLeaseTest.ownerGone=$true
                Reject { & $completionPath -RunId $lease.RunId -Confirm:$false } '*without settings/profile restoration obligations*' 'generic-completion-rejects-settings-parent'
                # This disposable obligation is intentionally left for its own
                # parent recovery test suite; generic completion may not erase it.
            }
            default {
                $state=$states[0]; $runId=$state.runId
                $statePath=Join-Path (Join-Path $global:kmgLeaseTest.stateRoot $runId) 'runtime-lease.json'
                $lockPath=Join-Path $global:kmgLeaseTest.stateRoot 'compatibility.lock'
                if ($case -eq 'leave-open') {
                    Check ($null -eq $errorText -and $status -ceq 'CompletionPending' -and -not $state.recoveryRequired -and $lock) 'leave-open-native-PASS-is-successful-owned-handoff'
                    Check ($state.runtime.game.processId -eq 900001 -and $state.runtime.outcome.status -ceq 'PASS') 'handoff-pins-exact-process-and-native-outcome'
                } else {
                    Check ($null -ne $errorText -and $status -ceq 'RecoveryRequired' -and $lock) ($case + '-retains-generic-recovery-record')
                    $orchestrationPath=Join-Path (Split-Path $state.runtime.request.path) 'orchestration.json'
                    $orchestration=Read-KmgCompatibilityJson $orchestrationPath
                    $expectedError=if ($case -eq 'timeout') { 'Runtime result timed out;*' } else { '*Controlled external process observation interruption.*' }
                    Check ($orchestration.status -ceq 'ERROR' -and $orchestration.exception.message -like $expectedError) ($case + '-actual-generic-terminal-error-recorded')
                }
                $before=Fixture-Snapshot $testRoot
                Reject { & $completionPath -RunId $runId -Confirm:$false } '*original runtime owner is still alive*' ($case + '-live-owner-rejected')
                $global:kmgLeaseTest.ownerGone=$true
                Reject { & $completionPath -RunId $runId -Confirm:$false } '*recorded Kingmaker process is still alive*' ($case + '-recorded-game-rejected')
                $global:kmgLeaseTest.process.StartTime=$global:kmgLeaseTest.process.StartTime.AddSeconds(1)
                Reject { & $completionPath -RunId $runId -Confirm:$false } '*Kingmaker is running*' ($case + '-foreign-game-or-reused-game-PID-rejected')
                $global:kmgLeaseTest.running=$false; $global:kmgLeaseTest.interrupt=$false
                Check ((Fixture-Snapshot $testRoot) -ceq $before) ($case + '-rejections-preserve-files')
                & $completionPath -RunId $runId -WhatIf -Confirm:$false
                Check ((Fixture-Snapshot $testRoot) -ceq $before) ($case + '-completion-WhatIf-mutation-free')
                if ($case -eq 'leave-open') {
                    $ownedLock=[IO.File]::ReadAllBytes($lockPath)
                    [IO.File]::WriteAllText($lockPath,'foreign-owner')
                    Reject { & $completionPath -RunId $runId -Confirm:$false } '*foreign run*' 'completion-rejects-foreign-lock'
                    Check ((Get-Content -LiteralPath $lockPath -Raw) -ceq 'foreign-owner') 'completion-preserves-foreign-lock'
                    [IO.File]::WriteAllBytes($lockPath,$ownedLock)
                    $requestPath=$state.runtime.request.path; $requestBytes=[IO.File]::ReadAllBytes($requestPath)
                    [IO.File]::AppendAllText($requestPath,"`r`n")
                    $foreignBefore=Fixture-Snapshot $testRoot
                    Reject { & $completionPath -RunId $runId -Confirm:$false } '*binding changed*' 'completion-rejects-foreign-request'
                    Check ((Fixture-Snapshot $testRoot) -ceq $foreignBefore) 'completion-preserves-foreign-request'
                    [IO.File]::WriteAllBytes($requestPath,$requestBytes)
                    $raceCheck=@{count=0}
                    $global:kmgLeaseTest.probe={
                        $raceCheck.count++
                        if ($raceCheck.count -eq 2) { $global:kmgLeaseTest.running=$true; $global:kmgLeaseTest.probe=$null }
                    }
                    $raceBefore=Fixture-Snapshot $testRoot
                    Reject { & $completionPath -RunId $runId -Confirm:$false } '*Kingmaker is running*' 'completion-rechecks-game-after-acquiring-ownership'
                    $global:kmgLeaseTest.running=$false
                    Check ((Fixture-Snapshot $testRoot) -ceq $raceBefore) 'completion-game-start-race-is-mutation-free'
                    $held=[IO.File]::Open($lockPath,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite,[IO.FileShare]::Read)
                    try { Reject { & $completionPath -RunId $runId -Confirm:$false } '*another process*' 'competing-completion-handle-rejected' }
                    finally { $held.Dispose() }
                    $journalBytes=[IO.File]::ReadAllBytes($statePath)
                    $global:kmgLeaseTest.probe={
                        $global:kmgLeaseTest.probe=$null
                        [IO.File]::AppendAllText($statePath,"`r`n")
                    }
                    Reject { & $completionPath -RunId $runId -Confirm:$false } '*journal changed*' 'completion-rejects-journal-change-during-owner-check'
                    Check ([IO.File]::ReadAllText($statePath).EndsWith("`r`n")) 'completion-preserves-foreign-journal'
                    [IO.File]::WriteAllBytes($statePath,$journalBytes)
                }
                # Exercise a real competing recovery while the first completion
                # holds its exclusive handle, through the same entry point.
                $race=@{checks=0;rejected=$false}
                $global:kmgLeaseTest.probe={
                    $race.checks++
                    if ($race.checks -eq 2) {
                        $global:kmgLeaseTest.probe=$null
                        try { & $completionPath -RunId $runId -Confirm:$false } catch { $race.rejected=$_.Exception.Message -like '*another process*' }
                    }
                }
                $global:kmgLeaseTest.ownerReused=$true
                & $completionPath -RunId $runId -Confirm:$false
                $global:kmgLeaseTest.probe=$null
                $completed=Read-KmgCompatibilityJson $statePath
                Check ($race.rejected -and $completed.status -ceq 'Completed' -and -not $completed.recoveryRequired -and
                    -not (Test-Path -LiteralPath $lockPath)) ($case + '-normal-exit-guarded-completion-with-PID-reuse-and-competing-recovery')
                if ($null -ne $state.runtime.outcome) { Check ((Get-KmgCompatibilitySha256 $state.runtime.outcome.path) -ceq $state.runtime.outcome.sha256) 'completion-does-not-rewrite-native-PASS' }
            }
        }
    }
} finally {
    $hooks | Remove-PSBreakpoint
    Remove-Item Function:global:Get-Process
    $PSDefaultParameterValues.Remove('Enter-KmgRuntimeLease:StateRoot')
    Remove-Variable kmgLeaseTest -Scope Global
}
$results | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $testRoot 'results.json') -Encoding UTF8
$checks | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $testRoot 'assertions.json') -Encoding UTF8
$results | Format-Table -AutoSize | Out-Host
Write-Output "Disposable evidence: $testRoot"
if ($ExpectReviewedFailure) {
    $leave=@($results | Where-Object case -eq 'leave-open')[0]
    if (-not ($leave.error -like 'Runtime ownership retained for recovery:*' -and $leave.status -ceq 'RecoveryRequired' -and $leave.lock)) { throw 'Reviewed generic leave-open defect was not reproduced.' }
    Write-Output 'RED: actual generic launcher accepted PASS then threw and retained its lease on intentional leave-open.'
} else { Write-Output "PASS $($checks.Count) generic launcher/lease lifecycle assertions." }
