Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'RuntimeCoordination.Common.ps1')
. (Join-Path $PSScriptRoot 'MagicCirclePreparation.Common.ps1')

function ConvertTo-KmgMagicCircleSettingsBytes($Configuration) {
    # FeatureModuleSettingsStore.Save writes this flat schema with Newtonsoft's
    # two-space indentation, CRLF and no BOM/trailing newline. Keep every input
    # key/value; an unfamiliar shape fails closed instead of discarding it.
    if ($Configuration.schemaVersion -ne 12) { throw 'Native settings format requires schema 12.' }
    $rows = foreach ($property in $Configuration.PSObject.Properties) {
        if ($property.Name -cne 'schemaVersion' -and $property.Value -isnot [bool]) {
            throw 'Native settings format requires boolean module values.'
        }
        '  ' + ($property.Name | ConvertTo-Json -Compress) + ': ' + ($property.Value | ConvertTo-Json -Compress)
    }
    return ,([Text.UTF8Encoding]::new($false).GetBytes("{`r`n" + ($rows -join ",`r`n") + "`r`n}"))
}

function Assert-KmgMagicCircleNativeSettingsSave($Lease, [string]$CurrentSha256) {
    # Recovery of older overrides is allowed only for the exact native
    # serialization of the journaled override, with the store's atomic-save
    # predecessor proving the input bytes. Never normalize the current file or
    # accept arbitrary semantically equivalent/foreign edits.
    $settings = $Lease.State.settings
    $previous = $settings.path + '.previous'
    if (-not (Test-Path -LiteralPath $previous -PathType Leaf) -or
        (Get-KmgCompatibilitySha256 $previous) -cne $settings.overrideSha256) {
        throw 'Foreign settings bytes preserved; no exact owned native-save predecessor.'
    }
    $input = [IO.File]::ReadAllBytes($previous)
    if ((Get-KmgMagicCircleBytesHash $input) -cne $settings.overrideSha256) { throw 'Native predecessor changed during verification.' }
    $configuration = ConvertFrom-KmgCircleBindingJson ([Text.Encoding]::UTF8.GetString($input).TrimStart([char]0xFEFF))
    $expected = ConvertTo-KmgMagicCircleSettingsBytes $configuration
    if ((Get-KmgMagicCircleBytesHash $expected) -cne $CurrentSha256) {
        throw 'Foreign settings bytes preserved; they differ from the exact native serialization of the owned override.'
    }
    Assert-KmgRuntimeLease $Lease
    Assert-KmgNotRunning
    $evidence = Join-Path (Split-Path $Lease.StatePath) 'FeatureModules.native-save.bin'
    Write-KmgRuntimeAtomicBytes $evidence $expected
    $proof = [ordered]@{
        predecessorSha256=$settings.overrideSha256; currentSha256=$CurrentSha256; evidencePath=$evidence
        contract='exact owned .previous bytes -> native flat-schema serialization; no value changes'
    }
    if ($settings -is [Collections.IDictionary]) { $settings['nativeReserialization'] = $proof }
    else { $settings | Add-Member -NotePropertyName nativeReserialization -NotePropertyValue $proof -Force }
    Write-KmgRuntimeLeaseState $Lease
}

function Wait-KmgMagicCircleProcessExit {
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    while (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and [DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 250
    }
    Assert-KmgNotRunning
}

function Assert-KmgMagicCircleDeploymentFiles($Files) {
    if (@($Files).Count -eq 0) { throw 'Transaction has no verified deployment files.' }
    foreach ($file in $Files) {
        if (-not (Test-Path -LiteralPath $file.path -PathType Leaf) -or
            (Get-KmgCompatibilitySha256 $file.path) -cne $file.sha256) { throw 'Owned deployment identity changed.' }
    }
}

function Restore-KmgMagicCircleOwnedSettings($Lease) {
    Assert-KmgRuntimeLease $Lease
    Assert-KmgNotRunning
    $settings = $Lease.State.settings
    if ($null -eq $settings) {
        # Interruption before the journaled override performed no settings write.
        $Lease.State.recoveryRequired = $false
        $Lease.State.reason = $null
        Write-KmgRuntimeLeaseState $Lease
        return
    }
    Assert-KmgMagicCircleDeploymentFiles $settings.deploymentFiles
    if ((Get-KmgCompatibilitySha256 $settings.backupPath) -cne $settings.originalSha256) { throw 'Exact original settings backup is damaged.' }
    $current = Get-KmgCompatibilitySha256 $settings.path
    if ($current -cne $settings.overrideSha256 -and $current -cne $settings.originalSha256) {
        Assert-KmgMagicCircleNativeSettingsSave $Lease $current
    }
    Assert-KmgRuntimeLease $Lease
    Assert-KmgNotRunning
    if ($current -cne $settings.originalSha256) {
        Write-KmgRuntimeAtomicBytes $settings.path ([IO.File]::ReadAllBytes($settings.backupPath)) $current
    }
    if ((Get-KmgCompatibilitySha256 $settings.path) -cne $settings.originalSha256) { throw 'Exact settings restoration verification failed.' }
    $Lease.State.recoveryRequired = $false
    $Lease.State.reason = $null
    $Lease.State.status = 'SettingsRestored'
    Write-KmgRuntimeLeaseState $Lease
}

function Resume-KmgMagicCircleSettingsTransaction {
    [CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
    param([string]$RunId, [string]$StateRoot, [string]$ExpectedSettingsPath)
    $statePath = Join-Path (Join-Path $StateRoot $RunId) 'runtime-lease.json'
    $state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
    if ($state.schemaVersion -ne 1 -or $state.runId -cne $RunId -or $state.status -ceq 'Completed' -or
        $state.purpose -cne 'Magic Circle persistence settings') { throw 'Not an unfinished Magic Circle settings transaction.' }
    if ($null -ne $state.settings -and (
        -not [IO.Path]::GetFullPath($state.settings.path).Equals([IO.Path]::GetFullPath($ExpectedSettingsPath), [StringComparison]::OrdinalIgnoreCase) -or
        -not [IO.Path]::GetFullPath($state.settings.backupPath).Equals((Join-Path (Split-Path $statePath) 'FeatureModules.original.bin'), [StringComparison]::OrdinalIgnoreCase))) {
        throw 'Recovery paths differ from the exact owned settings/backup.'
    }
    $owner = Get-Process -Id $state.processId -ErrorAction SilentlyContinue
    if ($owner -and $owner.StartTime.ToUniversalTime().ToString('o') -ceq $state.processStartedUtc) { throw 'Original owner process is still alive; recovery cannot steal its lease.' }
    Assert-KmgNotRunning
    if (-not $PSCmdlet.ShouldProcess($ExpectedSettingsPath, "recover exact settings transaction $RunId")) { return }
    $lock = Join-Path $StateRoot 'compatibility.lock'
    if ((Get-Content -LiteralPath $lock -Raw).Trim() -cne $RunId) { throw 'Recovery does not own the current shared lock.' }
    # A recovery caller must exclude another recovery writer as well as normal
    # deployment owners. Shared reads still allow the exact ownership checks.
    $stream = [IO.File]::Open($lock, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read)
    $lease = [pscustomobject]@{ RunId=$RunId; LockPath=$lock; Stream=$stream; ProcessId=$PID; StatePath=$statePath; Compatibility=$false; State=$state }
    $scope = [pscustomobject]@{ Lease=$lease; Acquired=$true }
    try { Restore-KmgMagicCircleOwnedSettings $lease }
    catch {
        $state.recoveryRequired = $true; $state.reason = $_.Exception.Message
        $state.status = 'RecoveryRequired'; Write-KmgRuntimeLeaseState $lease
        throw
    } finally { Exit-KmgRuntimeLease $scope }
    Write-Output "PASS exact settings recovery for $RunId; backup retained."
}

function Invoke-KmgMagicCircleSettingsTransaction {
    [CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
    param([Parameter(Mandatory = $true)][string]$SettingsPath,
        [Parameter(Mandatory = $true)][scriptblock]$VerifyDeployment,
        [Parameter(Mandatory = $true)][scriptblock]$Run,
        [bool]$ContentEnabled, [bool]$ControlEnhancementEnabled,
        [string]$StateRoot = 'C:/Dev/KingmakerGunslingerLab/compatibility-state')
    if (-not $PSCmdlet.ShouldProcess($SettingsPath, 'override Magic Circle startup settings for one guarded phase')) { return }
    $scope = Enter-KmgRuntimeLease -Purpose 'Magic Circle persistence settings' -StateRoot $StateRoot
    $lease = $scope.Lease
    $failure = $null
    try {
        Assert-KmgRuntimeLease $lease
        Assert-KmgNotRunning
        $files = @(& $VerifyDeployment)
        Assert-KmgMagicCircleDeploymentFiles $files
        # Recheck after the potentially slow artifact preflight, while still
        # holding the shared lease, before capture and before every live write.
        Assert-KmgNotRunning
        $original = [IO.File]::ReadAllBytes($SettingsPath)
        $configuration = ConvertFrom-KmgCircleBindingJson ([Text.Encoding]::UTF8.GetString($original).TrimStart([char]0xFEFF))
        if ($configuration.schemaVersion -ne 12 -or
            -not ($configuration.PSObject.Properties.Name -contains 'magic-circle-spells') -or
            -not ($configuration.PSObject.Properties.Name -contains 'protection-from-alignment-control-immunity')) {
            throw 'Existing schema-12 settings with both Circle/Protection keys are required.'
        }
        $configuration.'magic-circle-spells' = $ContentEnabled
        $configuration.'protection-from-alignment-control-immunity' = $ControlEnhancementEnabled
        $override = ConvertTo-KmgMagicCircleSettingsBytes $configuration
        $backup = Join-Path (Split-Path $lease.StatePath) 'FeatureModules.original.bin'
        Write-KmgRuntimeAtomicBytes $backup $original
        $lease.State.settings = [ordered]@{ path = [IO.Path]::GetFullPath($SettingsPath); backupPath = $backup
            originalSha256 = Get-KmgCompatibilitySha256 $backup; overrideSha256 = Get-KmgMagicCircleBytesHash $override
            deploymentFiles = $files
            recoveryCommand = "scripts/Restore-MagicCircleSettingsTransaction.ps1 -RunId $($lease.RunId) -Confirm:`$false" }
        $lease.State.status = 'OverridePrepared'; $lease.State.recoveryRequired = $true
        $lease.State.reason = 'Settings transaction unfinished. After the owning process and game exit, run scripts/Restore-MagicCircleSettingsTransaction.ps1 with this transaction RunId. Never overwrite foreign settings.'
        Write-KmgRuntimeLeaseState $lease
        Assert-KmgRuntimeLease $lease
        Assert-KmgMagicCircleDeploymentFiles $files
        Assert-KmgNotRunning
        Write-KmgRuntimeAtomicBytes $SettingsPath $override $lease.State.settings.originalSha256
        $lease.State.status = 'OverrideActive'; Write-KmgRuntimeLeaseState $lease
        & $Run $lease
    } catch { $failure = $_ }
    finally {
        try {
            Wait-KmgMagicCircleProcessExit
            Restore-KmgMagicCircleOwnedSettings $lease
        } catch {
            $lease.State.recoveryRequired = $true; $lease.State.reason = $_.Exception.Message
            $lease.State.status = 'RecoveryRequired'; Write-KmgRuntimeLeaseState $lease
            $failure = $_
        }
        finally { Exit-KmgRuntimeLease $scope }
    }
    if ($failure) { throw $failure }
}
