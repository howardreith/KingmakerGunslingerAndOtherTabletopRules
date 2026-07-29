[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$observer = Get-Content -Raw (Join-Path $runtime 'LoadGameNavigationObservation.cs')
$runner = Get-Content -Raw (Join-Path $runtime 'RuntimeTestRunner.cs')
$result = Get-Content -Raw (Join-Path $runtime 'RuntimeTestResult.cs')
$request = Get-Content -Raw (Join-Path $runtime 'RuntimeTestRequest.cs')
$catalog = Get-Content -Raw (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$orchestrator = Get-Content -Raw (
    Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1')
$common = Get-Content -Raw (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$provider = Get-Content -Raw (Join-Path $runtime 'SaveCatalogProviderObservation.cs')
$selection = Get-Content -Raw (Join-Path $runtime 'SaveCatalogSelectionObservation.cs')

$checks = [ordered]@{
    'guarded-allowlist' = $catalog.Contains('ObserveLoadGameNavigation') -and
        $runner.Contains('RuntimeTestRequestParser.TryActivate')
    'supervised-only' = $orchestrator.Contains(
        "'observe-load-game-navigation'") -and
        $orchestrator.Contains('requires -ManualInteractionRequired')
    'exact-catalog-boundary' = $observer.Contains(
        '"Kingmaker.UI.SaveLoadWindow.ListOfSaves"') -and
        $observer.Contains('IsSaveInfoList') -and
        $observer.Contains('method.GetParameters().Length == 2')
    'records-contract' = $result.Contains('declaringType') -and
        $result.Contains('methodSignature') -and
        $result.Contains('receiverType') -and
        $result.Contains('argumentTypes') -and
        $result.Contains('managedThreadId')
    'ordered-transition' = $observer.Contains('navigation-candidate-enter') -and
        $observer.Contains('catalog-initialize-enter') -and
        $observer.Contains('precededBy=')
    'human-click-only' = $result.Contains('ProbeInvokedNavigation') -and
        $observer.Contains('ProbeInvokedNavigation = false')
    'no-reflection-invoke' = -not ($observer -match '\.Invoke\(')
    'no-input' = -not (($observer + $orchestrator) -match
        '(SendKeys|mouse_event|keybd_event|WScript\.Shell|Input\.GetKey)')
    'no-save-descriptor-load' = $observer.Contains(
        'method.GetParameters().Any(p => IsSaveInfo(p.ParameterType))')
    'no-save-mutation' = $observer.Contains('IsSaveMutationOrLoad') -and
        $observer.Contains('name.StartsWith("Save"')
    'game-thread-recorded' = $observer.Contains('_gameThreadId') -and
        $observer.Contains('_wrongThread')
    'atomic-result' = $runner.Contains('CompleteNavigation') -and
        $result.Contains('stream.Flush(true)')
    'pass-ambiguous-timeout' = $runner.Contains(
        'RuntimeTestStatuses.Pass : RuntimeTestStatuses.Ambiguous') -and
        $runner.Contains('CompleteNavigation(RuntimeTestStatuses.Timeout')
    'hooks-removed' = $observer.Contains(
        'Unpatch(method, HarmonyPatchType.All, _context.ModId)')
    'catalog-timeout' = $request.Contains(
        'ObserveLoadGameNavigation') -and $common.Contains(
        "'observe-load-game-navigation'")
    'prior-observers-non-initiating' =
        -not ($provider -match '\.Invoke\(') -and
        -not ($selection -match '\.Invoke\([^;]*(LoadGame|LoadRoutine)')
    'steam-only' = $common.Contains('$script:KmgSteamAppId = 640820') -and
        -not $orchestrator.Contains('Kingmaker.exe')
    'whatif-boundary' = $orchestrator.Contains('$PSCmdlet.ShouldProcess(')
    'images-not-observed' = -not ($observer -match
        '(Thumbnail|Portrait|Screenshot|Sprite|Texture)')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count) {
    throw "Load Game navigation observation tests failed: $($failed -join ', ')"
}
Write-Host "Load Game navigation observation tests passed: $($checks.Count)"
