[CmdletBinding()]
param([switch]$ExpectReviewedFailures)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'MagicCircleSettingsTransaction.Common.ps1')

# Only the external process and launch boundaries are replaced. The production
# orchestration, shared lock, atomic writes, backups and recovery records run on
# disposable files. Deployment checks read actual pinned fixture bytes.
$testRoot = Join-Path (Split-Path $PSScriptRoot) ('artifacts/tests/magic-circle-settings-' + [Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($testRoot)
$script:circleFakeGame = $false
$script:circleOwnerGone = $false
$script:circleRecoveryProbe = $null
function Save-NativeFixtureSettings([string]$Directory) {
    # Invoke the compiled production store used by Kingmaker, not a serializer
    # imitation. Only disposable test files are passed to it.
    $assemblyPath = Join-Path (Split-Path $PSScriptRoot) 'artifacts/tests/Release/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.exe'
    $assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
    $store = $assembly.GetType('KingmakerGunslinger.FeatureModules.FeatureModuleSettingsStore', $true)
    $flags = [Reflection.BindingFlags]'Static,NonPublic'
    $native = $store.GetMethod('Load', $flags).Invoke($null, @($Directory, $null, $null))
    [void]$store.GetMethod('Save', $flags).Invoke($null, @($native))
}
function Get-Process { param([string]$Name, [int]$Id, $ErrorAction)
    if ($Name -eq 'Kingmaker') {
        if ($script:circleRecoveryProbe) { & $script:circleRecoveryProbe }
        if ($script:circleFakeGame) { [pscustomobject]@{ Id=987654 } }; return
    }
    if ($script:circleOwnerGone) { return }
    Microsoft.PowerShell.Management\Get-Process -Id $Id
}
function Assert-KmgNotRunning { if (@(Get-Process -Name Kingmaker).Count) { throw 'Fixture game is running.' } }
function Wait-KmgMagicCircleProcessExit { Assert-KmgNotRunning }
$failures = [Collections.Generic.List[string]]::new(); $passed = [Collections.Generic.List[string]]::new()
function Check([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
foreach ($case in @('busy','game-start-race','foreign-settings','exact-restoration','nested-ownership','launch-exception','running-recovery','recovery-after-exit','what-if','native-exit-save','native-predecessor-recovery','foreign-formatting')) {
    $script:circleFakeGame = $false
    $script:circleOwnerGone = $false
    $directory = Join-Path $testRoot $case; [void][IO.Directory]::CreateDirectory($directory)
    $settings = Join-Path $directory 'FeatureModules.json'; $stateRoot = Join-Path $directory 'coordination'
    $original = [Text.Encoding]::UTF8.GetPreamble() + [Text.Encoding]::UTF8.GetBytes("{`r`n  `"schemaVersion`": 12, `"magic-circle-spells`": true, `"protection-from-alignment-control-immunity`": true, `"unrelated`": false`r`n}`r`n")
    [IO.File]::WriteAllBytes($settings, $original)
    if ($case -in @('native-exit-save','native-predecessor-recovery','foreign-formatting')) {
        Save-NativeFixtureSettings $directory
        $original = [IO.File]::ReadAllBytes($settings)
    }
    $artifact = Join-Path $directory 'deployment-payload.bin'; [IO.File]::WriteAllText($artifact, 'immutable disposable deployment bytes')
    $artifactHash = Get-KmgCompatibilitySha256 $artifact
    $calls = @{ launch=0; guard=0 }
    $verify = {
        Check ((Get-KmgCompatibilitySha256 $artifact) -ceq $artifactHash) 'Deployment payload changed.'
        $calls.guard++
        if ($case -eq 'game-start-race') { $script:circleFakeGame = $true }
        [pscustomobject]@{ path=$artifact; sha256=$artifactHash }
    }
    $run = {
        param($lease)
        $calls.launch++
        if ($case -eq 'native-exit-save') {
            Save-NativeFixtureSettings $directory
            Check ((Get-KmgCompatibilitySha256 $settings) -ceq $lease.State.settings.overrideSha256) 'Override was not native-stable before launch.'
        }
        if ($case -eq 'native-predecessor-recovery') {
            # Model an interrupted pre-canonical transaction using the original
            # production format, then exercise the real native atomic saver.
            $oldOverride = [Text.UTF8Encoding]::new($false).GetBytes(((Get-Content -LiteralPath $settings -Raw | ConvertFrom-Json) | ConvertTo-Json -Depth 16))
            Write-KmgRuntimeAtomicBytes $settings $oldOverride $lease.State.settings.overrideSha256
            $lease.State.settings.overrideSha256 = Get-KmgMagicCircleBytesHash $oldOverride
            Write-KmgRuntimeLeaseState $lease
            Save-NativeFixtureSettings $directory
            $script:circleFakeGame=$true
            throw 'Interrupted after native settings save.'
        }
        if ($case -eq 'foreign-formatting') {
            [IO.File]::WriteAllText($settings, ([IO.File]::ReadAllText($settings) + "`r`n"))
            return
        }
        if ($case -eq 'foreign-settings') { [IO.File]::WriteAllText($settings, '{"foreign":true}'); return }
        if ($case -eq 'launch-exception') { throw 'External launcher fixture exception.' }
        if ($case -in @('running-recovery','recovery-after-exit')) { $script:circleFakeGame=$true; throw 'Interrupted external launch; game still exists.' }
        if ($case -eq 'nested-ownership') {
            $child = Enter-KmgRuntimeLease -ParentLease $lease -StateRoot $stateRoot -Purpose 'nested real launcher/deployment'
            Check (-not $child.Acquired -and [object]::ReferenceEquals($child.Lease, $lease)) 'Nested launcher tried to reacquire ownership.'
            Exit-KmgRuntimeLease $child
            Assert-KmgRuntimeLease $lease
            $blocked = $false
            try { Acquire-KmgCompatibilityLock $stateRoot 'concurrent-deployment' | Out-Null } catch { $blocked=$true }
            Check $blocked 'Nested return released the parent lease.'
            $wrongRootBlocked=$false
            try { Enter-KmgRuntimeLease -ParentLease $lease -StateRoot (Join-Path $directory 'unrelated-root') -Purpose 'wrong shared root' | Out-Null } catch { $wrongRootBlocked=$true }
            Check $wrongRootBlocked 'A separate coordination root was accepted as shared ownership.'
        }
        if ($null -ne $lease) { Assert-KmgRuntimeLease $lease }
    }
    $existing = $null
    if ($case -eq 'busy') { $existing = Acquire-KmgCompatibilityLock $stateRoot 'foreign-profile-owner' }
    $errorObserved = $false
    try {
        Invoke-KmgMagicCircleSettingsTransaction -SettingsPath $settings -VerifyDeployment $verify -Run $run `
            -ContentEnabled $false -ControlEnhancementEnabled $false -StateRoot $stateRoot -WhatIf:($case -eq 'what-if') -Confirm:$false
    } catch { $errorObserved = $true }
    $exact = [Convert]::ToBase64String([IO.File]::ReadAllBytes($settings)) -ceq [Convert]::ToBase64String($original)
    $states = @(if (Test-Path -LiteralPath $stateRoot) { Get-ChildItem -LiteralPath $stateRoot -Filter runtime-lease.json -Recurse -File | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json } })
    try {
        switch ($case) {
            'busy' { Check ($errorObserved -and $calls.launch -eq 0 -and $exact -and (Get-Content -LiteralPath $existing -Raw).Trim() -ceq 'foreign-profile-owner') 'Existing shared owner was ignored.' }
            'game-start-race' { Check ($errorObserved -and $calls.launch -eq 0 -and $exact) 'Settings changed after game-start race.' }
            'foreign-settings' { Check ($errorObserved -and (Get-Content -LiteralPath $settings -Raw) -ceq '{"foreign":true}' -and $states.Count -eq 1 -and $states[0].status -ceq 'RecoveryRequired') 'Foreign settings overwritten or recovery ownership lost.' }
            'foreign-formatting' { Check ($errorObserved -and [IO.File]::ReadAllText($settings).EndsWith("`r`n") -and $states[0].status -ceq 'RecoveryRequired') 'Unproven foreign formatting was silently accepted.' }
            'native-exit-save' { Check (-not $errorObserved -and $exact -and $states[0].status -ceq 'Completed') 'Native exit serialization prevented exact restoration.' }
            'native-predecessor-recovery' {
                Check ($errorObserved -and -not $exact -and $states[0].status -ceq 'RecoveryRequired') 'Native save interruption lost recovery state.'
                $script:circleFakeGame=$false; $script:circleOwnerGone=$true
                Resume-KmgMagicCircleSettingsTransaction -RunId $states[0].runId -StateRoot $stateRoot -ExpectedSettingsPath $settings -Confirm:$false
                Check ([Convert]::ToBase64String([IO.File]::ReadAllBytes($settings)) -ceq [Convert]::ToBase64String($original)) 'Proven native predecessor recovery lost exact original bytes.'
                $recovered=Get-Content -LiteralPath (Join-Path (Join-Path $stateRoot $states[0].runId) 'runtime-lease.json') -Raw | ConvertFrom-Json
                Check ($recovered.settings.nativeReserialization.predecessorSha256 -ceq $states[0].settings.overrideSha256) 'Recovery lacks exact native predecessor evidence.'
            }
            { $_ -in @('exact-restoration','nested-ownership') } { Check (-not $errorObserved -and $calls.launch -eq 1 -and $exact -and $states.Count -eq 1 -and $states[0].status -ceq 'Completed') 'Exact owned restoration did not complete.' }
            'launch-exception' { Check ($errorObserved -and $exact -and $states.Count -eq 1 -and $states[0].status -ceq 'Completed') 'Exception did not restore safely.' }
            'running-recovery' { Check ($errorObserved -and -not $exact -and $states.Count -eq 1 -and $states[0].status -ceq 'RecoveryRequired' -and (Test-Path -LiteralPath $states[0].settings.backupPath)) 'Running/interrupted transaction lost backup/recovery ownership.' }
            'recovery-after-exit' {
                Check ($errorObserved -and -not $exact -and $states.Count -eq 1 -and $states[0].status -ceq 'RecoveryRequired') 'Interruption recovery was not retained.'
                $script:circleFakeGame=$false
                $ownerBlocked=$false
                try { Resume-KmgMagicCircleSettingsTransaction -RunId $states[0].runId -StateRoot $stateRoot -ExpectedSettingsPath $settings -Confirm:$false } catch { $ownerBlocked=$true }
                Check $ownerBlocked 'Recovery stole a living owner lease.'
                $script:circleOwnerGone=$true
                Resume-KmgMagicCircleSettingsTransaction -RunId $states[0].runId -StateRoot $stateRoot -ExpectedSettingsPath $settings -WhatIf -Confirm:$false
                Check ((Get-KmgCompatibilitySha256 $settings) -ceq $states[0].settings.overrideSha256) 'Recovery WhatIf wrote settings.'
                $race = @{ checks=0; attempts=0; preserved=$false }
                $script:circleRecoveryProbe = {
                    # Inject a competing production recovery at the external
                    # game-state boundary after the first caller opened its lock.
                    $race.checks++
                    if ($race.checks -eq 2) {
                        $script:circleRecoveryProbe=$null; $race.attempts++
                        $recoveryBlocked=$false
                        try { Resume-KmgMagicCircleSettingsTransaction -RunId $states[0].runId -StateRoot $stateRoot -ExpectedSettingsPath $settings -Confirm:$false } catch { $recoveryBlocked=$true }
                        $race.preserved=$recoveryBlocked -and (Get-KmgCompatibilitySha256 $settings) -ceq $states[0].settings.overrideSha256
                    }
                }
                try {
                    Resume-KmgMagicCircleSettingsTransaction -RunId $states[0].runId -StateRoot $stateRoot -ExpectedSettingsPath $settings -Confirm:$false
                } finally { $script:circleRecoveryProbe=$null }
                Check ($race.attempts -eq 1 -and $race.preserved) 'Concurrent recovery mutated settings without exclusive recovery ownership.'
                Check ([Convert]::ToBase64String([IO.File]::ReadAllBytes($settings)) -ceq [Convert]::ToBase64String($original)) 'Recovery lost exact backup bytes.'
                Check (-not (Test-Path -LiteralPath (Join-Path $stateRoot 'compatibility.lock'))) 'Recovered transaction did not release ownership.'
            }
            'what-if' { Check (-not $errorObserved -and $calls.launch -eq 0 -and $calls.guard -eq 0 -and $exact -and -not (Test-Path -LiteralPath $stateRoot)) 'WhatIf mutated files or launched.' }
        }
        $passed.Add($case)
    } catch { $failures.Add($case + ': ' + $_.Exception.Message) }
}
$script:circleFakeGame = $false
$script:circleOwnerGone = $false
Write-Output ('Passed: ' + ($passed -join ', '))
Write-Output ('Failed: ' + ($failures -join '; '))
Write-Output "Disposable evidence: $testRoot"
if ($ExpectReviewedFailures) {
    foreach ($name in @('busy','game-start-race','foreign-settings','running-recovery')) {
        if (-not @($failures | Where-Object { $_.StartsWith($name + ':') }).Count) { throw "Expected reviewed regression was not reproduced: $name" }
    }
    Write-Output 'RED confirmed against extracted reviewed production settings orchestration; no live files or launches.'
} elseif ($failures.Count) { throw 'Magic Circle settings transaction regressions failed.' }
