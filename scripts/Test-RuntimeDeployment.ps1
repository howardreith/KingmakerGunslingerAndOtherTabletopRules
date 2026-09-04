[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

$orchestratorPath = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$deploymentPath = Join-Path $PSScriptRoot 'Deploy-Local.ps1'
$legacyDeploymentPath = Join-Path $PSScriptRoot `
    'Deploy-QualifiedElementalRaces114.ps1'
$legacySequencePath = Join-Path $PSScriptRoot `
    'Invoke-ElementalRaceLegacyMigrationQualification.ps1'
$orchestrator = Get-Content -LiteralPath $orchestratorPath -Raw
$deployment = Get-Content -LiteralPath $deploymentPath -Raw
$legacyDeployment = Get-Content -LiteralPath $legacyDeploymentPath -Raw
$legacySequence = Get-Content -LiteralPath $legacySequencePath -Raw

$failures = [Collections.Generic.List[string]]::new()
function Assert-True([bool]$Condition, [string]$Name) {
    if (-not $Condition) { $failures.Add($Name) }
}
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try {
        [void](& $Action)
        $failures.Add($Name)
    }
    catch { }
}

$orchestratorBackupCalls = @(
    [regex]::Matches($orchestrator, 'Backup-Live-Mod\.ps1')
)
$deploymentBackupCalls = @(
    [regex]::Matches($deployment, 'Backup-Live-Mod\.ps1')
)
$shouldProcessIndex = $deployment.IndexOf('$PSCmdlet.ShouldProcess(', [StringComparison]::Ordinal)
$dryReturnIndex = $deployment.IndexOf("Write-Host 'Dry run only;", [StringComparison]::Ordinal)
$backupIndex = $deployment.IndexOf(
    "`$backup = & (Join-Path `$PSScriptRoot 'Backup-Live-Mod.ps1')",
    [StringComparison]::Ordinal)
$manifestIndex = $deployment.IndexOf('Read-KmgBuildLocalManifest', [StringComparison]::Ordinal)
$runningIndex = $deployment.IndexOf('Assert-KmgNotRunning', [StringComparison]::Ordinal)
$liveDirectoryIndex = $deployment.IndexOf(
    'Test-Path -LiteralPath $LiveModDirectory',
    [StringComparison]::Ordinal)
$whatIfCaptureIndex = $deployment.IndexOf(
    '$deploymentWhatIfRequested = [bool]$WhatIfPreference',
    [StringComparison]::Ordinal)
$whatIfDisableIndex = $deployment.IndexOf(
    '$WhatIfPreference = $false',
    $whatIfCaptureIndex,
    [StringComparison]::Ordinal)
$whatIfRestoreIndex = $deployment.IndexOf(
    '$WhatIfPreference = $deploymentWhatIfRequested',
    [StringComparison]::Ordinal)

Assert-True ($orchestratorBackupCalls.Count -eq 0) 'orchestrator-must-not-back-up'
Assert-True ($deploymentBackupCalls.Count -eq 1) 'deployment-must-own-one-backup'
Assert-True ($manifestIndex -ge 0 -and $manifestIndex -lt $shouldProcessIndex) `
    'package-preflight-before-should-process'
Assert-True ($runningIndex -ge 0 -and $runningIndex -lt $shouldProcessIndex) `
    'process-preflight-before-should-process'
Assert-True ($liveDirectoryIndex -ge 0 -and $liveDirectoryIndex -lt $shouldProcessIndex) `
    'live-directory-preflight-before-should-process'
Assert-True ($shouldProcessIndex -ge 0 -and $dryReturnIndex -gt $shouldProcessIndex) `
    'dry-run-return-is-guarded'
Assert-True ($backupIndex -gt $dryReturnIndex) 'backup-only-after-dry-run-return'
Assert-True ($whatIfCaptureIndex -ge 0 -and
    $whatIfDisableIndex -gt $whatIfCaptureIndex -and
    $whatIfDisableIndex -lt $manifestIndex -and
    $whatIfRestoreIndex -gt $liveDirectoryIndex -and
    $whatIfRestoreIndex -lt $shouldProcessIndex) `
    'whatif-is-suppressed-only-for-read-only-preflight'
Assert-True (-not $deployment.Substring(
    $shouldProcessIndex,
    $dryReturnIndex - $shouldProcessIndex).Contains('Backup-Live-Mod.ps1')) `
    'dry-run-does-not-call-backup'
Assert-True ($orchestrator.Contains(
    "`$deploymentManifestPath = & (Join-Path `$PSScriptRoot 'Deploy-Local.ps1')") -and
    $orchestrator.Contains('-PackagePath $package -Confirm:$false -PassThru')) `
    'real-orchestration-cannot-bypass-deployment-boundary'
Assert-True ($orchestrator.Contains('[switch]$ManualInteractionRequired')) `
    'manual-interaction-switch-required'
Assert-True ($orchestrator.Contains(
    'MANUALLY LOAD KMG_AUTOMATION_WORKING NOW')) 'manual-instruction-prominent'
Assert-True (-not ($orchestrator -match
    '(SendKeys|mouse_event|keybd_event|WScript\.Shell)')) 'orchestrator-sends-no-input'
Assert-True ($orchestrator.Contains(
    '-EnforceManualInteraction')) `
    'manual-observation-requires-explicit-switch'
Assert-True ($orchestrator.Contains(
    "'observe-save-catalog-and-selection'")) `
    'catalog-observation-requires-explicit-switch'
Assert-True ($orchestrator.Contains('Start-KmgSteamKingmaker')) `
    'manual-observation-preserves-steam-launch'
Assert-True (-not $orchestrator.Contains('Kingmaker.exe')) `
    'manual-orchestrator-has-no-direct-launch'
Assert-True ($deployment.Contains("Join-Path `$live 'FeatureModules.json'") -and
    $deployment.Contains('[IO.File]::ReadAllBytes($featureSettingsPath)') -and
    $deployment.Contains('[IO.File]::WriteAllBytes($featureSettingsTemporary, $featureSettingsBytes)') -and
    $deployment.Contains('featureModuleSettingsPreserved = $featureSettingsExisted')) `
    'deployment-preserves-feature-settings-bytes-outside-package'
$reuseBranch = $orchestrator.IndexOf('elseif ($ReuseInstalledArtifact) {',
    [StringComparison]::Ordinal)
$reuseVerify = $orchestrator.IndexOf('Assert-KmgReusableDeployment',
    $reuseBranch, [StringComparison]::Ordinal)
$buildBranch = $orchestrator.IndexOf("& (Join-Path `$PSScriptRoot 'Build-Local.ps1')",
    $reuseVerify, [StringComparison]::Ordinal)
Assert-True ($orchestrator.Contains('[switch]$ReuseInstalledArtifact') -and
    $reuseBranch -ge 0 -and $reuseVerify -gt $reuseBranch -and
    $buildBranch -gt $reuseVerify) `
    'reuse-mode-verifies-and-skips-build-deploy'
Assert-True ($deployment.Contains('schemaVersion = 2') -and
    $deployment.Contains('commit = $manifest.commit') -and
    $deployment.Contains('dllMvid = $manifest.dllMvid') -and
    $deployment.Contains('firearmBundleSha256 = Get-KmgSha256') -and
    $deployment.Contains('deployedFirearmManifestSha256 = $deployedFirearmManifestSha256') -and
    $deployment.Contains('deployedFirearmSoundBankSha256 = $deployedFirearmSoundBankSha256')) `
    'deployment-manifest-captures-immutable-artifact-identity'
Assert-True ($deployment.Contains('Packaged and deployed firearm audio files differ.') -and
    $deployment.Contains('$packagedFirearmManifestSha256 -ne $deployedFirearmManifestSha256') -and
    $deployment.Contains('$packagedFirearmSoundBankSha256 -ne $deployedFirearmSoundBankSha256')) `
    'deployment-verifies-packaged-firearm-audio-parity'
Assert-True ($deployment.Contains('[switch]$AllowEmptyFirstInstall') -and
    $deployment.Contains('-AllowEmptySource:$AllowEmptyFirstInstall') -and
    $deployment.Contains('backupWasEmpty = [bool]$backup.EmptySource')) `
    'empty-first-install-is-explicit-and-recorded'

$legacyShouldProcess = $legacyDeployment.IndexOf(
    '$PSCmdlet.ShouldProcess(', [StringComparison]::Ordinal)
$legacyBackup = $legacyDeployment.IndexOf(
    "`$backup = & (Join-Path `$PSScriptRoot 'Backup-Live-Mod.ps1')",
    [StringComparison]::Ordinal)
Assert-True ($legacyDeployment.Contains(
        'b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694') -and
    $legacyDeployment.Contains(
        '09af96b95e2abfa39e45f30c8ccb4cb1e8772981dd3be17846f07cbbd2dd8262') -and
    $legacyDeployment.Contains(
        'dcd73856-39d4-40ce-9b05-77bf249103d7') -and
    $legacyDeployment.Contains('$expectedEntryCount = 135')) `
    'legacy-deployment-pins-complete-artifact-identity'
Assert-True ($legacyShouldProcess -ge 0 -and
    $legacyBackup -gt $legacyShouldProcess) `
    'legacy-deployment-backs-up-only-after-authorization'
Assert-True ($legacyDeployment.Contains(
        '$deploymentWhatIfRequested = [bool]$WhatIfPreference') -and
    $legacyDeployment.Contains(
        'Remove-Item -LiteralPath $temporary -Recurse -Force')) `
    'legacy-deployment-dry-run-cleans-private-staging'
Assert-True ($orchestrator.Contains(
        '[switch]$ReuseQualifiedElementalRaces114Release') -and
    $orchestrator.Contains(
        'Current-source and qualified-legacy artifact reuse are mutually exclusive.') -and
    $orchestrator.Contains(
        'Qualified 0.0.114 reuse permits only elemental-race-persistence-prepare')) `
    'legacy-reuse-is-mutually-exclusive-and-scenario-locked'
Assert-True ($legacySequence.Contains(
        '$legacyDeploymentManifestPath = & $deployLegacy') -and
    $legacySequence.Contains(
        '$migrationDeploymentManifestPath = & $deployCurrent') -and
    $legacySequence.Contains(
        '$restoredDeploymentManifestPath = & $deployCurrent') -and
    $legacySequence.Contains('Restore-OriginalFeatureState')) `
    'legacy-transaction-restores-settings-and-current-artifact'
Assert-True (-not $legacySequence.Contains('KMG_AUTOMATION_BASELINE')) `
    'legacy-transaction-excludes-protected-baseline'

$overlayVerifier = Get-Command Assert-KmgQualifiedLegacyRuntimeOverlay `
    -ErrorAction SilentlyContinue
Assert-True ($null -ne $overlayVerifier) `
    'legacy-runtime-overlay-verifier-exists'
if ($null -ne $overlayVerifier) {
    $overlayRoot = Join-Path (Join-Path $PSScriptRoot '..\artifacts\test-temp') `
        ('qualified-legacy-overlay-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $overlayRoot -Force | Out-Null
    try {
        $dll = Join-Path $overlayRoot 'KingmakerGunslinger.dll'
        $settings = Join-Path $overlayRoot 'FeatureModules.json'
        $settingsPrevious = $settings + '.previous'
        $cache = Join-Path $overlayRoot 'KingmakerGunslinger.dll.12345.cache'
        $extra = Join-Path $overlayRoot 'unexpected.bin'
        Copy-Item -LiteralPath ([Management.Automation.PSObject].Assembly.Location) `
            -Destination $dll
        $settingsSource = '{"schemaVersion":10,"elemental-races":true}'
        [IO.File]::WriteAllText($settings, $settingsSource,
            (New-Object Text.UTF8Encoding($false)))
        $expectedFiles = @('KingmakerGunslinger.dll')
        $settingsSha = Get-KmgSha256 -Path $settings
        $dllSha = Get-KmgSha256 -Path $dll
        $dllMvid = Get-KmgDllMvid -Path $dll

        $exact = Assert-KmgQualifiedLegacyRuntimeOverlay `
            -LiveDirectory $overlayRoot -ExpectedFiles $expectedFiles `
            -ExpectedSettingsSha256 $settingsSha `
            -ExpectedDllSha256 $dllSha -ExpectedDllMvid $dllMvid
        Assert-True ($exact.SettingsMode -ceq 'exact' -and
            $exact.RuntimeGeneratedFiles.Count -eq 0) `
            'legacy-runtime-overlay-accepts-pristine-deployment'

        Copy-Item -LiteralPath $settings -Destination $settingsPrevious
        [IO.File]::WriteAllText($settings,
            '{ "schemaVersion" : 10, "elemental-races" : true }',
            (New-Object Text.UTF8Encoding($false)))
        Copy-Item -LiteralPath $dll -Destination $cache
        $normalized = Assert-KmgQualifiedLegacyRuntimeOverlay `
            -LiveDirectory $overlayRoot -ExpectedFiles $expectedFiles `
            -ExpectedSettingsSha256 $settingsSha `
            -ExpectedDllSha256 $dllSha -ExpectedDllMvid $dllMvid
        Assert-True ($normalized.SettingsMode -ceq 'normalized-with-exact-backup' -and
            $normalized.RuntimeGeneratedFiles.Count -eq 2) `
            'legacy-runtime-overlay-accepts-only-observed-normalization-and-cache'

        Add-Content -LiteralPath $cache -Value 'tamper'
        Assert-Throws {
            Assert-KmgQualifiedLegacyRuntimeOverlay `
                -LiveDirectory $overlayRoot -ExpectedFiles $expectedFiles `
                -ExpectedSettingsSha256 $settingsSha `
                -ExpectedDllSha256 $dllSha -ExpectedDllMvid $dllMvid
        } 'legacy-runtime-overlay-rejects-cache-tamper'
        Copy-Item -LiteralPath $dll -Destination $cache -Force

        [IO.File]::WriteAllText($settings,
            '{"schemaVersion":10,"elemental-races":false}',
            (New-Object Text.UTF8Encoding($false)))
        Assert-Throws {
            Assert-KmgQualifiedLegacyRuntimeOverlay `
                -LiveDirectory $overlayRoot -ExpectedFiles $expectedFiles `
                -ExpectedSettingsSha256 $settingsSha `
                -ExpectedDllSha256 $dllSha -ExpectedDllMvid $dllMvid
        } 'legacy-runtime-overlay-rejects-semantic-settings-drift'
        [IO.File]::WriteAllText($settings,
            '{ "schemaVersion" : 10, "elemental-races" : true }',
            (New-Object Text.UTF8Encoding($false)))

        Copy-Item -LiteralPath $dll -Destination `
            (Join-Path $overlayRoot 'KingmakerGunslinger.dll.54321.cache')
        Assert-Throws {
            Assert-KmgQualifiedLegacyRuntimeOverlay `
                -LiveDirectory $overlayRoot -ExpectedFiles $expectedFiles `
                -ExpectedSettingsSha256 $settingsSha `
                -ExpectedDllSha256 $dllSha -ExpectedDllMvid $dllMvid
        } 'legacy-runtime-overlay-rejects-multiple-caches'
        Remove-Item -LiteralPath `
            (Join-Path $overlayRoot 'KingmakerGunslinger.dll.54321.cache') -Force

        [IO.File]::WriteAllText($extra, 'foreign')
        Assert-Throws {
            Assert-KmgQualifiedLegacyRuntimeOverlay `
                -LiveDirectory $overlayRoot -ExpectedFiles $expectedFiles `
                -ExpectedSettingsSha256 $settingsSha `
                -ExpectedDllSha256 $dllSha -ExpectedDllMvid $dllMvid
        } 'legacy-runtime-overlay-rejects-arbitrary-extra-file'
    }
    finally {
        if (Test-Path -LiteralPath $overlayRoot) {
            Remove-Item -LiteralPath $overlayRoot -Recurse -Force
        }
    }
}

if ($failures.Count -ne 0) {
    throw "Runtime deployment safety tests failed: $($failures -join ', ')"
}
Write-Host 'Runtime deployment safety tests passed: 35'
