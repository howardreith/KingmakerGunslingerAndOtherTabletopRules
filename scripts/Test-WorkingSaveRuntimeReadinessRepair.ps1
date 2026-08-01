[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$runner = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs')
$scenario = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'WorkingSaveSmokeScenario.cs')
$request = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRequest.cs')
$result = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestResult.cs')
$orchestrator = Get-Content -Raw -LiteralPath (
    Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1')
$deploy = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\Deploy-Local.ps1')
$identity = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'RuntimeBuildIdentity.cs')
$main = Get-Content -Raw -LiteralPath (
    Join-Path $root 'src\KingmakerGunslinger\Main.cs')
$fixture = Get-Content -Raw -LiteralPath (
    Join-Path $root 'tests\fixtures\working-save-pre-readiness-timeout.json') |
    ConvertFrom-Json

$checks = [ordered]@{
    'normal-launch-no-working-hooks' =
        $runner.IndexOf('if (!decision.Accepted)', [StringComparison]::Ordinal) -lt
        $runner.IndexOf('new RuntimeTestRunner', [StringComparison]::Ordinal)
    'invalid-request-no-hooks' = $request.Contains('return "save-name-required"') -and
        $runner.Contains('if (!decision.Accepted)')
    'accepted-before-hooks' =
        $runner.IndexOf('WriteLifecycleStage("request-accepted")',
            [StringComparison]::Ordinal) -lt
        $runner.IndexOf('_workingSaveSmoke.Install()',
            [StringComparison]::Ordinal)
    'malformed-save-name-before-ui' = $request.Contains('save-name-required') -and
        $request.Contains('save-name-not-allowed')
    'hook-install-error-contained' = $scenario.Contains('RemoveHooks();') -and
        $runner.Contains('CompleteStartupError(')
    'load-game-original-preserved' = $scenario.Contains(
        'private static void Prefix(') -and
        -not $scenario.Contains('private static bool Prefix(')
    'hook-exception-contained' = $scenario.Contains(
        'A diagnostic failure must never escape into the game handler.')
    'startup-hooks-removed' = $scenario.Contains('catch') -and
        $scenario.Contains('RemoveHooks();')
    'ready-after-hooks-and-action' =
        $runner.IndexOf('hooks-install-complete', [StringComparison]::Ordinal) -lt
        $runner.IndexOf('_trace.WriteReady(', [StringComparison]::Ordinal) -and
        $scenario.Contains('_stage == "action-invocation"')
    'no-action-before-ready' =
        $runner.Contains('if (!_workingReadyWritten)') -and
        $scenario.Contains('if (_stage == "action-invocation")')
    'stage-specific-missing-ready' = $runner.Contains(
        'startupTimeout.Diagnostics.Add("timeoutStage=" + _workingStartupStage)')
    'repeated-update-error-terminal' = $runner.Contains(
        'CompleteStartupError(_workingSaveSmoke != null &&') -and
        $runner.Contains(': _workingStartupStage, exception);')
    'umm-visibility-neutral' = $runner.Contains(
        'overlay nonblocking-or-absent') -and
        -not $runner.Contains('ShowOnStart')
    'startup-exception-error' = $runner.Contains('"startup-error"') -and
        $runner.Contains('RuntimeTestStatuses.Error')
    'new-game-unpatched' = -not $scenario.Contains('OnButtonNewGame')
    'steam-app-id' = $orchestrator.Contains('[int]$SteamAppId = 640820')
    'single-deployment-backup' = $deploy.Contains('backup') -and
        $orchestrator.Contains("'Deploy-Local.ps1'")
    'baseline-forbidden' = $request.Contains('baseline-save-forbidden')
    'no-save-write-call' = -not ($scenario -match
        '\.(Save|AutoSave|QuickSave)\s*\(')
    'atomic-evidence' = $result.Contains('stream.Flush(true)') -and
        $result.Contains('File.Replace(temporary, path, null)')
    'failed-stage-regression-fixture' =
        $fixture.firstMissingStage -eq 'main-menu-ready' -and
        $fixture.uiActionOccurred -eq $false -and
        $fixture.saveActionOccurred -eq $false
    'identity-before-request-parsing' =
        $main.IndexOf('RecordEarlyIdentity(context)',
            [StringComparison]::Ordinal) -lt
        $main.IndexOf('TryAttach(context)', [StringComparison]::Ordinal)
    'identity-has-path-hash-mvid-commit' =
        $identity.Contains('LoadedModulePath') -and
        $identity.Contains('LoadedModuleSha256') -and
        $identity.Contains('ModuleVersionId') -and
        $identity.Contains('GitCommit')
    'identity-hash-does-not-use-version' =
        $identity.Contains('SHA256.Create()') -and
        $identity.Contains('assembly.ManifestModule.ModuleVersionId')
    'package-live-cache-mismatch-observable' =
        $identity.Contains('loadedModuleSha256') -and
        $identity.Contains('loadedModulePath')
    'rejection-structured-error' =
        $runner.Contains('WriteRejectedRequest(context, decision)') -and
        $runner.Contains('Status = RuntimeTestStatuses.Error') -and
        $runner.Contains('hookInstalled=false') -and
        $runner.Contains('uiActionOccurred=false') -and
        $runner.Contains('saveActionOccurred=false')
    'unknown-scenario-stage-error' =
        $request.Contains('return "scenario-not-allowed"') -and
        $request.Contains('return "scenario-allowlisted"')
    'invalid-save-stage-error' =
        $request.Contains('return "save-name-valid"')
    'proven-main-menu-lifecycle' =
        $scenario.Contains('Assembly.GetType(OwnerType, true)') -and
        $scenario.Contains('component.gameObject.activeInHierarchy')
    'brittle-main-menu-readiness-removed' =
        -not $scenario.Contains(
            'Assembly.GetType(MainMenuType, true);' + [Environment]::NewLine +
            '            UnityEngine.Object[] candidates')
    'no-cache-deletion-added-without-proof' =
        -not $deploy.Contains('KingmakerGunslinger.dll.*.cache')
    'build-commit-embedded' =
        $identity.Contains('AssemblyMetadataAttribute') -and
        (Get-Content -Raw -LiteralPath (
            Join-Path $root 'tools\build_mod_from_private_references.py')).Contains(
            'AssemblyMetadata("GitCommit"')
}

$requiredStages = @(
    'request-argument-observed', 'request-file-opened', 'request-schema-valid',
    'request-accepted', 'runner-created', 'onupdate-entered',
    'runner-onupdate-entered',
    'scenario-selected', 'hooks-install-start', 'hooks-install-complete',
    'main-menu-search-start', 'main-menu-ready', 'load-game-action-resolved',
    'working-save-ready', 'startup-error'
)
foreach ($stage in $requiredStages) {
    $checks["stage-$stage"] = $runner.Contains('"' + $stage + '"')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Working-save readiness repair tests failed: $($failed -join ', ')"
}
Write-Host "Working-save readiness repair tests passed: $($checks.Count)"
