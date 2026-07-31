[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$observer = Get-Content -Raw (
    Join-Path $runtime 'LoadGameButtonActionObservation.cs')
$runner = Get-Content -Raw (Join-Path $runtime 'RuntimeTestRunner.cs')
$result = Get-Content -Raw (Join-Path $runtime 'RuntimeTestResult.cs')
$request = Get-Content -Raw (Join-Path $runtime 'RuntimeTestRequest.cs')
$catalog = Get-Content -Raw (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$orchestrator = Get-Content -Raw (
    Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1')
$common = Get-Content -Raw (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$provider = Get-Content -Raw (
    Join-Path $runtime 'SaveCatalogProviderObservation.cs')
$selection = Get-Content -Raw (
    Join-Path $runtime 'SaveCatalogSelectionObservation.cs')

$checks = [ordered]@{
    'guarded-allowlist' = $catalog.Contains('ObserveLoadGameButtonAction') -and
        $catalog.Contains('"observe-load-game-button-action"') -and
        $runner.Contains('RuntimeTestRequestParser.TryActivate')
    'supervised-only' = $common.Contains(
        "'observe-load-game-button-action' = [pscustomobject]@{") -and
        $common.Contains('RequiresManualInteraction = $true') -and
        $orchestrator.Contains('-EnforceManualInteraction')
    'exact-handler' = $observer.Contains(
        '"Kingmaker.UI.MainMenuUI.MainMenuButtons"') -and
        $observer.Contains('"OnButtonLoadGame"') -and
        $observer.Contains('method.GetParameters().Length == 0') -and
        $observer.Contains('method.ReturnType == typeof(void)')
    'exact-catalog-boundary' = $observer.Contains(
        '"Kingmaker.UI.SaveLoadWindow.ListOfSaves"') -and
        $observer.Contains('IsSaveInfoList') -and
        $observer.Contains('method.GetParameters().Length == 2')
    'only-two-hooks' = $observer.Contains(
        'internal bool Ready { get { return _patched.Count == 2; } }') -and
        -not $observer.Contains('assembly.GetTypes()')
    'unique-active-interactable' = $observer.Contains(
        '_candidates.Count == 1') -and
        $observer.Contains('ActiveInHierarchy') -and
        $observer.Contains('Interactable')
    'component-and-hierarchy' = $result.Contains('componentType') -and
        $result.Contains('gameObjectPath') -and
        $result.Contains('mainMenuRootPath') -and
        $result.Contains('siblingIndex') -and
        $result.Contains('componentIdentities')
    'listener-and-label-identity' = $observer.Contains(
        'GetPersistentMethodName') -and $observer.Contains('m_RuntimeCalls') -and
        $result.Contains('safeLabelIdentities') -and $result.Contains('listeners')
    'ordered-transition' = $observer.Contains(
        'load-game-handler-enter') -and
        $observer.Contains('catalog-initialize-enter') -and
        $observer.Contains('_catalogSequence > _handlerSequence')
    'human-click-only' = $result.Contains('ProbeInvokedAction') -and
        $observer.Contains('ProbeInvokedAction = false')
    'no-reflection-invoke' = -not ($observer -match '\.Invoke\(')
    'no-input' = -not (($observer + $orchestrator) -match
        '(SendKeys|mouse_event|keybd_event|WScript\.Shell|Input\.GetKey|PointerEventData)')
    'no-save-descriptor-load' = -not ($observer -match
        '(LoadGameFromMainMenu|LoadRoutine|SaveInfo __)')
    'game-thread-required' = $observer.Contains('_gameThreadId') -and
        $observer.Contains('_wrongThread') -and
        $observer.Contains('!_wrongThread')
    'originals-preserved' = $observer.Contains('new HarmonyMethod(prefix)') -and
        $observer.Contains('new HarmonyMethod(postfix)') -and
        -not ($observer -match '(__runOriginal|__result|Prefix\([^)]*IEnumerator)')
    'atomic-result' = $runner.Contains('CompleteButtonAction') -and
        $result.Contains('stream.Flush(true)')
    'hooks-removed' = $observer.Contains(
        'Unpatch(method, HarmonyPatchType.All, _context.ModId)')
    'images-not-required' = -not ($observer -match
        '(Thumbnail|Portrait|Screenshot|Sprite|Texture)')
    'prior-observers-non-initiating' =
        -not ($provider -match '\.Invoke\(') -and
        -not ($selection -match '\.Invoke\([^;]*(LoadGame|LoadRoutine)')
    'steam-only' = $common.Contains('$script:KmgSteamAppId = 640820') -and
        -not $orchestrator.Contains('Kingmaker.exe')
    'whatif-boundary' = $orchestrator.Contains('$PSCmdlet.ShouldProcess(')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count) {
    throw "Load Game button-action tests failed: $($failed -join ', ')"
}
Write-Host "Load Game button-action tests passed: $($checks.Count)"
