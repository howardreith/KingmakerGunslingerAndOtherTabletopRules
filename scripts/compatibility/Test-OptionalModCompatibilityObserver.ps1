[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
. (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')

$runtimeProfiles = @(
    'gunslinger-only', 'gunslinger-call-of-the-wild', 'gunslinger-arms-armor',
    'gunslinger-toggle-custom-soundpacks', 'gunslinger-high-risk-combined',
    'gunslinger-all-loadable-local', 'gunslinger-qualified-combined'
)
foreach ($profile in $runtimeProfiles) {
    [void](Assert-KmgRuntimeScenarioPreflight `
        -Scenario 'observe-optional-mod-compatibility' -ExpectedVersion '0.0.76' `
        -TimeoutSeconds 120 -Parameters @{ profileId = $profile })
}

$rejected = @(
    @{ profileId = 'gunslinger-craft-magic-items' },
    @{ profileId = 'gunslinger-call-of-the-wild-craft-magic-items' },
    @{ profileId = 'bag-of-tricks' }, @{ profileId = '../escape' },
    @{ profileId = 'gunslinger-only'; extra = 'not-allowed' }, @{}
)
foreach ($parameters in $rejected) {
    $failedClosed = $false
    try {
        [void](Assert-KmgRuntimeScenarioPreflight `
            -Scenario 'observe-optional-mod-compatibility' -ExpectedVersion '0.0.76' `
            -TimeoutSeconds 120 -Parameters $parameters)
    }
    catch { $failedClosed = $true }
    if (-not $failedClosed) {
        throw "Observer accepted invalid parameters: $($parameters | ConvertTo-Json -Compress)"
    }
}

$observer = Get-Content -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\OptionalModCompatibilityObserver.cs') -Raw
foreach ($contract in @('currentEntry.GetType().DeclaringType', 'GetField("modEntries"', 'GetPatchedMethods()',
    'GetPatchInfo(method)', 'gunslinger-blueprint-registered',
    'gunslinger-root-catalog-published', 'gunslinger-class-selector-input',
    'call-of-the-wild-final-classes', 'CallOfTheWild.Helpers',
    'mysterious-stranger-replacement-rows', 'production-firearm-identities',
    'save-free-observer')) {
    if (-not $observer.Contains($contract)) { throw "Observer contract missing: $contract" }
}
$catalogDiagnostics = Get-Content -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Compatibility\ClassCatalogDiagnostics.cs') -Raw
$catalogDiagnostics += Get-Content -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Bootstrap\BlueprintBootstrap.cs') -Raw
$catalogDiagnostics += Get-Content -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Bootstrap\BlueprintLifecyclePatch.cs') -Raw
foreach ($contract in @('after-registration', 'before-publish', 'after-publish',
    'gunslinger-postfix-return', 'first-idle-update', 'before-chargen-selector',
    'chargen-selector-result', 'Game.Instance.BlueprintRoot',
    'BlueprintRoot.Instance', 'observed.Root', 'GetPatchInfo(target)')) {
    if (-not $catalogDiagnostics.Contains($contract)) {
        throw "Class-catalog diagnostic contract missing: $contract"
    }
}
foreach ($forbidden in @('QuickSave', 'SaveGame', 'LoadGame(', 'StartNewGame')) {
    if ($observer.Contains($forbidden)) {
        throw "Observer contains forbidden save/game mutation token: $forbidden"
    }
}
$wrapper = Get-Content -LiteralPath (Join-Path $root `
    'scripts\compatibility\Invoke-KingmakerCompatibilityProfile.ps1') -Raw
foreach ($contract in @('[ValidateRange(120, 900)]',
    '[int]$RuntimeTimeoutSeconds = 300',
    'TimeoutSeconds = $RuntimeTimeoutSeconds',
    'ObserverStartupTimeoutSeconds = $RuntimeTimeoutSeconds',
    "'observe-feature-module-settings'", '[hashtable]$Parameters = @{}',
    "'Mods\KingmakerGunslinger\FeatureModules.json'",
    '$arguments.Parameters = $Parameters')) {
    if (-not $wrapper.Contains($contract)) {
        throw "Compatibility wrapper timeout contract missing: $contract"
    }
}
$launcher = Get-Content -LiteralPath (Join-Path $root `
    'scripts\Invoke-KingmakerRuntimeTest.ps1') -Raw
if (-not $launcher.Contains("'gunslinger-qualified-combined'")) {
    throw 'Guarded runtime launcher does not allow the committed qualified-combined profile.'
}
$runnerSource = Get-Content -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs') -Raw
foreach ($contract in @('"spent to 0; remains 0"', 'afterUnaware == 0',
    'return exception.ToString();', '"commandResult=" + commandResult')) {
    if (-not $runnerSource.Contains($contract)) {
        throw "Compatibility diagnostic fixture contract missing: $contract"
    }
}
$evasive = Get-Content -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Blueprints\EvasiveBlueprints.cs') -Raw
foreach ($contract in @('PreservesCurrentComponentContract',
    'source.ComponentsArray.Length != clone.ComponentsArray.Length',
    'sourceComponent.GetType() != cloneComponent.GetType()')) {
    if (-not $evasive.Contains($contract)) {
        throw "Evasive optional-mod donor contract missing: $contract"
    }
}
foreach ($forbidden in @('evasion.ComponentsArray.Length != 1',
    'uncanny.ComponentsArray.Length != 2',
    'improved.ComponentsArray.Length != 1', 'CallOfTheWild.')) {
    if ($evasive.Contains($forbidden)) {
        throw "Evasive optional-mod repair retained forbidden coupling: $forbidden"
    }
}
Write-Host 'Optional-mod compatibility observer allowlist and source contracts passed.'
