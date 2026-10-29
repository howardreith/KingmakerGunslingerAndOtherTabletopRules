[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'gunslinger-only',
        'gunslinger-races-unleashed',
        'gunslinger-call-of-the-wild',
        'gunslinger-cowwithhat-suggestion',
        'gunslinger-call-of-the-wild-races-unleashed',
        'gunslinger-tweak-or-treat',
        'gunslinger-call-of-the-wild-favored-class',
        'gunslinger-call-of-the-wild-favored-class-traits-disabled',
        'gunslinger-arms-armor',
        'gunslinger-toggle-custom-soundpacks',
        'gunslinger-high-risk-combined',
        'gunslinger-high-risk-combined-favored-class',
        'gunslinger-all-loadable-local',
        'gunslinger-qualified-combined')]
    [string]$ProfileId,
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'mod-load-smoke',
        'observe-expanded-summoning-sprint22-contracts',
        'observe-expanded-summoning-sprint24-theropod-census',
        'observe-expanded-summoning-sprint26-native-inputs',
        'observe-expanded-summoning-sprint27-native-inputs',
        'observe-expanded-summoning-sprint28-native-inputs',
        'observe-expanded-summoning-sprint29-native-inputs',
        'observe-expanded-summoning-sprints30-31-native-inputs',
        'observe-expanded-summoning-sprint32-native-inputs',
        'observe-expanded-summoning-sprint33-native-inputs',
        'observe-alternate-summoning-sources',
        'observe-alternate-summoning-blocked-family',
        'disposable-alternate-summoning-contracts',
        'disposable-alternate-summoning-player-paths',
        'observe-favored-class-host-state',
        'observe-optional-mod-compatibility',
        'observe-class-blueprint-contracts',
        'observe-gunslinger-presentation',
        'disposable-firearm-dependent-feats',
        'disposable-pistolero-deeds',
        'disposable-archetype-reconciliation',
        'musket-master-mechanics-and-starter',
        'disposable-firearm-visual-rigs',
        'disposable-production-firearm-switching',
        'disposable-gunslinger-targeting-arms',
        'disposable-gunslinger-dodge',
        'disposable-firearm-wwise-audio',
        'disposable-gunslinger-scatter-shot',
        'disposable-reload-autocast',
        'disposable-paper-cartridge-mode-view-lifecycle',
        'observe-rare-firearm-blueprint-contracts',
        'observe-vendor-table-contracts',
        'magic-firearm-native-properties',
        'reliable-firearm-misfire-matrix',
        'blunderbuss-thundering-scatter',
        'disposable-paper-cartridge-comprehensive',
        'observe-feature-module-settings',
        'observe-brown-fur-cotw-contract',
        'observe-brown-fur-cotw-absent-isolation',
        'observe-aid-another-compatibility-contracts',
        'disposable-helpful-bodyguard',
        'elemental-races-races-unleashed-compatibility',
        'disposable-elemental-race-mechanics',
        'disposable-elemental-race-native-identity',
        'observe-elemental-heritage-donors',
        'observe-elemental-feat-native-contracts',
        'disposable-elemental-feat-mechanics',
        'disposable-elemental-ifrit-feats',
        'disposable-elemental-ifrit-advanced-feats',
        'disposable-elemental-sylph-feats',
        'disposable-elemental-undine-feats',
        'observe-elemental-heritage-blueprints',
        'disposable-elemental-trait-turn-costs',
        'observe-elemental-alternate-trait-framework',
        'disposable-elemental-heritage-mechanics',
        'disposable-elemental-heritage-slas',
        'disposable-bodyguard-feats-disabled',
        'working-save-smoke',
        'working-save-expanded-summoning-creature-review',
        'disposable-brown-fur-native-cast',
        'observe-elven-branched-spear-contracts',
        'observe-eastern-weapon-contracts',
        'disposable-elven-branched-spear-combat',
        'disposable-eastern-weapons-combat',
        'observe-kmg-compatibility-asset-attribution',
        'working-save-eastern-weapons-prepare',
        'working-save-eastern-weapons-verify-cleanup',
        'working-save-eastern-weapons-verify-absent',
        'observe-magic-circle-native-contracts',
        'disposable-magic-circle-profile',
        'disposable-magic-circle-evil',
        'disposable-magic-circle-ui', 'disposable-magic-circle-terrain',
        'observe-teleportation-native-contracts',
        'observe-teleportation-world-map',
        'disposable-teleportation-familiarity',
        'disposable-teleportation-resources',
        'disposable-teleportation-context',
        'disposable-teleportation-casting',
        'disposable-teleportation-interaction',
        'disposable-teleportation-travelers',
        'disposable-teleportation-gamepad',
        'disposable-teleportation-spellbook-ui',
        'disposable-teleportation-level-up',
        'disposable-teleportation-destinations',
        'disposable-teleportation-disabled',
        'observe-expanded-summoning-inventory',
        'observe-expanded-summoning-module-boundary',
        'observe-expanded-summoning-variant-menu',
        'disposable-expanded-summoning',
        'disposable-expanded-summoning-native-profile',
        'disposable-expanded-summoning-sprint22-native-review',
        'disposable-expanded-summoning-sprint23-native-review',
        'disposable-expanded-summoning-sprint24-native-review',
        'disposable-expanded-summoning-sprint25-native-review',
        'disposable-expanded-summoning-sprint26-native-review',
        'disposable-expanded-summoning-sprint27-native-review',
        'disposable-expanded-summoning-sprint28-native-review',
        'disposable-expanded-summoning-sprint29-native-review',
        'disposable-expanded-summoning-sprints30-31-native-review',
            'disposable-expanded-summoning-sprint32-native-review',
        'disposable-expanded-summoning-cleanup2-native-review',
        'summon-same-turn-activation',
        'summon-same-turn-acadamae',
        'summon-same-turn-multiple',
        'summon-same-turn-native-control',
        'summon-same-turn-rtwp-control',
        'summon-same-turn-compatibility-quickened',
        'summon-same-turn-compatibility-acadamae',
        'disposable-shield-other',
        'disposable-acadamae-graduate',
        'disposable-gunslinger-comprehensive-acceptance')]
    [string[]]$Scenario,
    [hashtable]$Parameters = @{},
    [ValidateRange(120, 900)]
    [int]$RuntimeTimeoutSeconds = 300,
    [ValidateSet('unchanged', 'normal', 'balance-fixes')]
    [string]$CotwProgressionMode = 'unchanged',
    [switch]$AllowDirtyGit,
    [switch]$ReuseInstalledArtifact,
    [switch]$ExpandedSummoningDisabled,
    [string]$DeploymentManifestPath,
    [string]$PackagePath,
    # The one development purpose of an explicit development candidate. Valid
    # only with -ReuseInstalledArtifact and its exact -PackagePath; the
    # profile then stages, deploys and records it as development, never as a
    # release or qualification candidate.
    [string]$DevelopmentPurpose,
    [string]$ReferenceRoot = 'C:\Dev\KingmakerGunslingerLab\examples',
    [string]$KingmakerInstallDir =
        'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    [string]$StateRoot = 'C:\Dev\KingmakerGunslingerLab\compatibility-state'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
. (Join-Path $PSScriptRoot 'CompatibilityProfile.Common.ps1')
Assert-KmgSuggestionProfileSelection -ProfileId $ProfileId -Scenario $Scenario -PackagePath $PackagePath `
    -ReferenceRoot $ReferenceRoot -RepositoryRoot $root
. (Join-Path (Split-Path $PSScriptRoot) 'RuntimeCoordination.Common.ps1')
. (Join-Path (Split-Path $PSScriptRoot) 'RuntimeHarness.Common.ps1')
. (Join-Path (Split-Path $PSScriptRoot) 'RuntimeAutomation.Common.ps1')
. (Join-Path (Split-Path $PSScriptRoot) 'ExpandedSummoningOrchestration.Common.ps1')
. (Join-Path (Split-Path $PSScriptRoot) 'RuntimeAutomation.Common.ps1')
$workingDispelUi = $null -ne $Scenario -and @($Scenario).Count -eq 1 -and $Scenario[0] -ceq 'working-save-expanded-summoning-creature-review'
if (@($Scenario | Where-Object { $_ -ceq 'working-save-expanded-summoning-creature-review' }).Count -gt 0 -and
    (-not $workingDispelUi -or -not (Test-KmgSprints30DispelUiParameters $Parameters))) {
    throw 'This profile admits only the exact fixed S30/S31 read-only Working UI subcase; no profile entered.'
}
. (Join-Path (Split-Path $PSScriptRoot) 'SummonCleanup2CurrentStack.Common.ps1')
$cleanup2ModuleOff = $Scenario.Count -eq 1 -and
    (Test-KmgCleanup2ModuleOffScope -Scenario $Scenario[0] -Parameters $Parameters)
if ($ExpandedSummoningDisabled -and -not $cleanup2ModuleOff -and
    ($Scenario.Count -ne 1 -or $Scenario[0] -cne 'working-save-expanded-summoning-verify-cleanup' -or
        $Parameters.saveName -cne 'KMG_AUTOMATION_WORKING' -or $Parameters.persistenceScope -cne 'ankylosaurus')) {
    throw 'Module OFF requires exact Ankylosaurus verify/cleanup or a save-free Cleanup2 native menus, giants, learning or controller presentation request.'
}
if (-not [string]::IsNullOrWhiteSpace($DeploymentManifestPath)) {
    throw 'A profile transaction requires a fresh deployment record; an external -DeploymentManifestPath cannot survive restoration/profile changes.'
}
$developmentCandidate = -not [string]::IsNullOrEmpty($DevelopmentPurpose)
if ($developmentCandidate) {
    [void](Assert-KmgDevelopmentPurpose $DevelopmentPurpose)
    if (-not $ReuseInstalledArtifact -or [string]::IsNullOrWhiteSpace($PackagePath)) {
        throw 'A development candidate profile requires -ReuseInstalledArtifact and its exact -PackagePath; no profile was entered.'
    }
}
$liveModDirectory = Join-Path $KingmakerInstallDir 'Mods\KingmakerGunslinger'
$expectedInstall = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker'
if (-not [IO.Path]::GetFullPath($KingmakerInstallDir).TrimEnd('\').Equals(
    [IO.Path]::GetFullPath($expectedInstall).TrimEnd('\'), [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The runtime/deployment guards support only the exact current Kingmaker installation; no profile was entered.'
}
$expectedVersion = [string](Read-KmgCompatibilityJson (Join-Path $root 'Info.json')).Version
$runId = 'compat-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' +
    [Guid]::NewGuid().ToString('N').Substring(0, 12)
$entered = $false
$primaryError = $null
$results = [Collections.Generic.List[object]]::new()
$cotwSettingsPath = Join-Path $KingmakerInstallDir `
    'Mods\CallOfTheWild\settings.json'
$cotwSettingsOriginalExists = Test-Path -LiteralPath $cotwSettingsPath `
    -PathType Leaf
$cotwSettingsOriginalBytes = if ($cotwSettingsOriginalExists) {
    [IO.File]::ReadAllBytes($cotwSettingsPath)
} else { $null }
$cotwSettingsOriginalSha = if ($cotwSettingsOriginalExists) {
    (Get-FileHash -LiteralPath $cotwSettingsPath -Algorithm SHA256).Hash
} else { $null }
$cotwSettingsStagedBeforeSha = $null
$cotwSettingsStagedAfterSha = $null
$favoredSettingsPath = Join-Path $KingmakerInstallDir `
    'Mods\ZFavoredClass\settings.json'
$favoredSettingsStagedBeforeSha = $null
$favoredSettingsStagedAfterSha = $null
$featureSettingsPath = Join-Path $KingmakerInstallDir `
    'Mods\KingmakerGunslinger\FeatureModules.json'
$featureSettingsOriginalExists = Test-Path -LiteralPath $featureSettingsPath `
    -PathType Leaf
$featureSettingsOriginalBytes = if ($featureSettingsOriginalExists) {
    [IO.File]::ReadAllBytes($featureSettingsPath)
} else { $null }
$featureSettingsOriginalSha = if ($featureSettingsOriginalExists) {
    (Get-FileHash -LiteralPath $featureSettingsPath -Algorithm SHA256).Hash
} else { '<absent>' }
$favoredTraitsMode = if ($ProfileId -ceq
    'gunslinger-call-of-the-wild-favored-class-traits-disabled') {
    'disabled'
} elseif ($ProfileId -in @(
    'gunslinger-call-of-the-wild-favored-class',
    'gunslinger-high-risk-combined-favored-class')) { 'enabled' } else {
    'unchanged'
}

$cotwProfileIds = @('gunslinger-call-of-the-wild',
    'gunslinger-cowwithhat-suggestion',
    'gunslinger-call-of-the-wild-races-unleashed',
    'gunslinger-tweak-or-treat',
    'gunslinger-call-of-the-wild-favored-class',
    'gunslinger-call-of-the-wild-favored-class-traits-disabled',
    'gunslinger-high-risk-combined-favored-class',
    'gunslinger-high-risk-combined', 'gunslinger-all-loadable-local')
if ($CotwProgressionMode -cne 'unchanged' -and
    $ProfileId -notin $cotwProfileIds) {
    throw "CotW progression mode requires a profile containing Call of the Wild."
}
if ($CotwProgressionMode -cne 'unchanged' -and
    -not $cotwSettingsOriginalExists) {
    throw "CotW progression mode requires an existing original settings file: $cotwSettingsPath"
}

$moduleScenario = @($Scenario | Where-Object { $_ -in @(
    'observe-feature-module-settings',
    'observe-expanded-summoning-module-boundary',
    'elemental-races-races-unleashed-compatibility') }).Count -gt 0 -or
    (@($Scenario | Where-Object { $_ -ceq
        'observe-vendor-table-contracts' }).Count -gt 0 -and
        $Parameters.Count -gt 0)
$assetAttributionScenario = @($Scenario | Where-Object { $_ -ceq
    'observe-kmg-compatibility-asset-attribution' }).Count -gt 0
$sprint22ReviewScenario = @($Scenario | Where-Object { $_ -cin @(
    'disposable-expanded-summoning-sprint22-native-review',
    'disposable-expanded-summoning-sprint23-native-review',
    'disposable-expanded-summoning-sprint24-native-review',
    'disposable-expanded-summoning-sprint25-native-review',
    'disposable-expanded-summoning-sprint26-native-review',
    'disposable-expanded-summoning-sprint27-native-review',
    'disposable-expanded-summoning-sprint28-native-review',
    'disposable-expanded-summoning-sprint29-native-review',
    'disposable-expanded-summoning-sprints30-31-native-review',
            'disposable-expanded-summoning-sprint32-native-review',
    'disposable-expanded-summoning-cleanup2-native-review') }).Count -gt 0
if ($assetAttributionScenario -and $Scenario.Count -ne 1) {
    throw 'Asset attribution must run as one fresh-process profile scenario.'
}
if ($moduleScenario) {
    $keys = @($Parameters.Keys | Sort-Object)
    if ($keys.Count -ne 13 -or $keys[0] -cne 'acadamaeGraduate' -or
        $keys[1] -cne 'bodyguardFeats' -or
        $keys[2] -cne 'brownFurTransmuter' -or
        $keys[3] -cne 'easternWeapons' -or
        $keys[4] -cne 'elementalRaces' -or
        $keys[5] -cne 'elvenBranchedSpears' -or
        $keys[6] -cne 'expandedSummoning' -or $keys[7] -cne 'gunslinger' -or
        $keys[8] -cne 'magicCircleSpells' -or
        $keys[9] -cne 'protectionFromAlignmentControlImmunity' -or
        $keys[10] -cne 'shieldOther' -or $keys[11] -cne 'teleportationSpells' -or
        $keys[12] -cne 'urbanBarbarian' -or
        $Parameters.magicCircleSpells -isnot [bool] -or
        $Parameters.gunslinger -isnot [bool] -or
        $Parameters.acadamaeGraduate -isnot [bool] -or
        $Parameters.shieldOther -isnot [bool] -or
        $Parameters.expandedSummoning -isnot [bool] -or
        $Parameters.elvenBranchedSpears -isnot [bool] -or
        $Parameters.easternWeapons -isnot [bool] -or
        $Parameters.brownFurTransmuter -isnot [bool] -or
        $Parameters.urbanBarbarian -isnot [bool] -or
        $Parameters.bodyguardFeats -isnot [bool] -or
        $Parameters.protectionFromAlignmentControlImmunity -isnot [bool] -or
        $Parameters.elementalRaces -isnot [bool] -or
        $Parameters.teleportationSpells -isnot [bool]) {
        throw 'Feature-module profile observation requires exactly thirteen Boolean parameters: gunslinger, acadamaeGraduate, shieldOther, expandedSummoning, elvenBranchedSpears, easternWeapons, brownFurTransmuter, urbanBarbarian, bodyguardFeats, protectionFromAlignmentControlImmunity, elementalRaces, teleportationSpells, and magicCircleSpells.'
    }
} elseif ($sprint22ReviewScenario) {
    $keys = @($Parameters.Keys)
    if ($Scenario.Count -ne 1 -or $keys.Count -ne 1 -or
        $keys[0] -cne 'reviewPhase' -or
        $Parameters.reviewPhase -isnot [string] -or
        ($Scenario[0] -ceq
            'disposable-expanded-summoning-sprint22-native-review' -and
            $Parameters.reviewPhase -cnotin @('rtwp-foundation',
                'turn-based-control')) -or
        ($Scenario[0] -ceq
            'disposable-expanded-summoning-sprint23-native-review' -and
            $Parameters.reviewPhase -cnotin @('rtwp-foundation',
                'turn-based-control', 'charge-rounding-rtwp',
                'charge-rounding-turn-based')) -or
        ($Scenario[0] -ceq
            'disposable-expanded-summoning-sprint24-native-review' -and
            $Parameters.reviewPhase -cnotin @('rtwp-foundation',
                'rtwp-deinonychus', 'turn-based-deinonychus',
                'rtwp-tyrannosaurus', 'turn-based-tyrannosaurus',
                'visual-lifecycle', 'shared-controls')) -or
        ($Scenario[0] -ceq 'disposable-expanded-summoning-sprint25-native-review' -and
            $Parameters.reviewPhase -cnotin @('rtwp-foundation', 'turn-based-commands',
                'rtwp-ai', 'routes', 'visual-lifecycle', 'overrun', 'great-cleave')) -or
        ($Scenario[0] -ceq 'disposable-expanded-summoning-sprint27-native-review' -and
            $Parameters.reviewPhase -cnotin @('rtwp-foundation','pounce-rtwp','pounce-turn-based','turn-based-abort','turn-based-target-loss','rtwp-ai','charge-boundaries','visual-lifecycle','routes','outdoor-locomotion','shared-controls','expiry-boundary','qualification-combat','qualification-presentation')) -or
        ($Scenario[0] -ceq 'disposable-expanded-summoning-sprint28-native-review' -and $Parameters.reviewPhase -cnotin @('profiles','modes','commands','turns','ownership','disarm','catch','ai','ai-turns','cleave','routes')) -or
        ($Scenario[0] -ceq 'disposable-expanded-summoning-sprint29-native-review' -and $Parameters.reviewPhase -cnotin @('profiles','modes','commands','turns','cloud-magic','storm-magic','ai','ai-turns','items','disarm','awesome','awesome-turn','catch','cleave','vital','vital-turn','routes')) -or
        ($Scenario[0] -ceq 'disposable-expanded-summoning-sprints30-31-native-review' -and $Parameters.reviewPhase -cnotin @('bootstrap','lemure-core','lemure-full','lemure-ai','dretch-cloud','dretch-ai','dretch-ai-ally','archon-core','archon-hound','hound-armed','hound-natural','lantern-rays','lantern-body','astral-core','astral-hammer','astral-blade','astral-support','astral-atwill','astral-smite','astral-dispel','trumpet-dispel','trumpet-blast','trumpet-blast-ai','trumpet-blast-ai-safe','trumpet-book','trumpet-book-turn','trumpet-caster','outsider-bodies','source-lifecycle','defense-consumers','dretch-melee','dretch-melee-ai','hound-single','hound-ai','hound-turn','hound-disarm','shared-circles','archon-boundaries','public-routes','final-integration','boundary-lemure','boundary-dretch','boundary-hound-archon','boundary-lantern-archon','boundary-astral-deva','boundary-trumpet-archon','celestial-cleave')) -or
        ($Scenario[0] -ceq 'disposable-expanded-summoning-cleanup2-native-review' -and
            -not (Test-KmgCleanup2ReviewPhaseScope -Scenario $Scenario[0] -Parameters $Parameters)) -or
        ($Scenario[0] -ceq 'disposable-expanded-summoning-sprint32-native-review' -and
            -not (Test-KmgSprint32ReviewPhaseScope -Scenario $Scenario[0] -Parameters $Parameters)) -or
        ($Scenario[0] -ceq 'disposable-expanded-summoning-sprint26-native-review' -and
            $Parameters.reviewPhase -cnotin @('rtwp-foundation', 'turn-based-commands',
                'gorgon-breath-rtwp', 'gorgon-breath-turn-based',
                'bulette-leap-rtwp', 'bulette-leap-turn-based',
                'rtwp-ai', 'routes', 'visual-lifecycle'))) {
        throw 'Sprint 22/23/24/25 native review requires one fresh scenario and exactly one allowlisted reviewPhase parameter.'
    }
} elseif ($assetAttributionScenario) {
    $keys = @($Parameters.Keys)
    if ($keys.Count -ne 1 -or $keys[0] -cne 'assetConfiguration' -or
        $Parameters.assetConfiguration -isnot [string] -or
        $Parameters.assetConfiguration -cnotin @('all-suppressed',
            'firearms-only', 'spears-only', 'eastern-only', 'all-enabled')) {
        throw 'Asset attribution requires exactly one allowlisted assetConfiguration parameter.'
    }
} elseif ($Parameters.Count -ne 0 -and -not $workingDispelUi) {
    throw 'Compatibility profile parameters are supported only for feature-module, module-state vendor, or asset-attribution observations.'
}

# Fast argument checks above precede candidate work; no profile is active yet.
if (-not $ReuseInstalledArtifact -and -not $WhatIfPreference) {
    & (Join-Path (Split-Path $PSScriptRoot) 'Build-Local.ps1') | Out-Host
}
if (-not $PackagePath) { $PackagePath = Get-KmgCompatibilityDefaultPackage $root }
if ((Get-KmgCompatibilityPackageVersion $PackagePath) -cne $expectedVersion) {
    throw 'Compatibility package version does not match the current repository; no profile was entered.'
}
$candidateReadArguments = @{ PackagePath = $PackagePath; RepositoryRoot = $root }
if ($developmentCandidate) { $candidateReadArguments.DevelopmentPurpose = $DevelopmentPurpose }
$profileCandidate = Read-KmgBuildLocalManifest @candidateReadArguments
$candidateGit = Get-KmgGitState -RepositoryRoot $root
if ($developmentCandidate) {
    Assert-KmgDevelopmentSourceBinding -Receipt $profileCandidate -Git $candidateGit `
        -SourceStateSha256 (Get-KmgSourceStateFingerprint -RepositoryRoot $root) `
        -ExpectedVersion $expectedVersion
}
if ($profileCandidate.commit -cne $candidateGit.Commit -or
    $profileCandidate.sourceStateSha256 -cne (Get-KmgSourceStateFingerprint -RepositoryRoot $root)) {
    throw 'Compatibility candidate must match the unchanged product worktree before profile entry.'
}
$compatRuntimeLease = $null
if (-not $PSCmdlet.ShouldProcess((Join-Path $KingmakerInstallDir 'Mods'),
    "run isolated profile $ProfileId and restore exact original state")) { return }
$sprint22Boundary=@($Scenario | Where-Object {
    Test-KmgExactSummoningRuntimeBoundary $_ }).Count -gt 0 -or $workingDispelUi
if($sprint22Boundary) {
    . (Join-Path $root 'scripts\ExpandedSummoningOrchestration.Common.ps1')
    $sprint22SettingsKey='Software\Owlcat Games\Pathfinder Kingmaker'
    $nativeKey=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($sprint22SettingsKey,$false)
    if($null -eq $nativeKey) { throw 'Current game settings are missing; no profile entered.' }
    try { $sprint22SettingsSnapshot=@(Get-KmgFlatGameSettingsValues -Key $nativeKey) }
    finally { $nativeKey.Dispose() }
}
try {
    & (Join-Path $PSScriptRoot 'Enter-KingmakerCompatibilityProfile.ps1') `
        -ProfileId $ProfileId -RunId $runId -KingmakerInstallDir $KingmakerInstallDir `
        -StateRoot $StateRoot -ReferenceRoot $ReferenceRoot -PackagePath $PackagePath -Confirm:$false | Out-Host
    $entered = $true
    if($sprint22Boundary) {
        Write-KmgCompatibilityJsonAtomic (Join-Path $StateRoot "$runId\game-settings-intake.json") $sprint22SettingsSnapshot
    }
    $compatRuntimeLease = Open-KmgCompatibilityRuntimeLease $runId $StateRoot
    if ($CotwProgressionMode -cne 'unchanged') {
        if (-not (Test-Path -LiteralPath $cotwSettingsPath -PathType Leaf)) {
            throw "Staged CotW settings file is missing: $cotwSettingsPath"
        }
        $cotwSettingsStagedBeforeSha = (Get-FileHash -LiteralPath `
            $cotwSettingsPath -Algorithm SHA256).Hash
        $settingsText = [IO.File]::ReadAllText($cotwSettingsPath)
        $balancePattern = '("balance_fixes"\s*:\s*)(true|false)'
        $balanceMatches = [regex]::Matches($settingsText, $balancePattern,
            [Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($balanceMatches.Count -ne 1) {
            throw "Staged CotW settings must contain exactly one balance_fixes Boolean."
        }
        $desiredBalance = if ($CotwProgressionMode -ceq 'balance-fixes') {
            'true'
        } else { 'false' }
        $match = $balanceMatches[0]
        $replacement = $match.Groups[1].Value + $desiredBalance
        $updated = $settingsText.Substring(0, $match.Index) + $replacement +
            $settingsText.Substring($match.Index + $match.Length)
        $temporaryCotw = $cotwSettingsPath + '.kmg-progression.tmp'
        [IO.File]::WriteAllText($temporaryCotw, $updated,
            (New-Object Text.UTF8Encoding($false)))
        Move-Item -LiteralPath $temporaryCotw -Destination $cotwSettingsPath -Force
        $resolvedSettings = Get-Content -Raw -LiteralPath $cotwSettingsPath |
            ConvertFrom-Json
        $expectedBalance = $CotwProgressionMode -ceq 'balance-fixes'
        if ($resolvedSettings.balance_fixes -isnot [bool] -or
            [bool]$resolvedSettings.balance_fixes -ne $expectedBalance) {
            throw "Staged CotW balance_fixes did not resolve to the requested mode."
        }
        $cotwSettingsStagedAfterSha = (Get-FileHash -LiteralPath `
            $cotwSettingsPath -Algorithm SHA256).Hash
    }
    if ($favoredTraitsMode -cne 'unchanged') {
        if (-not (Test-Path -LiteralPath $favoredSettingsPath -PathType Leaf)) {
            throw "Staged Favored Class settings file is missing: $favoredSettingsPath"
        }
        $favoredSettingsStagedBeforeSha = (Get-FileHash -LiteralPath `
            $favoredSettingsPath -Algorithm SHA256).Hash
        $settingsText = [IO.File]::ReadAllText($favoredSettingsPath)
        $traitsPattern = '("enable_traits"\s*:\s*)(true|false)'
        $traitsMatches = [regex]::Matches($settingsText, $traitsPattern,
            [Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($traitsMatches.Count -ne 1) {
            throw 'Staged Favored Class settings must contain exactly one enable_traits Boolean.'
        }
        $desiredTraits = if ($favoredTraitsMode -ceq 'enabled') {
            'true'
        } else { 'false' }
        $match = $traitsMatches[0]
        $replacement = $match.Groups[1].Value + $desiredTraits
        $updated = $settingsText.Substring(0, $match.Index) + $replacement +
            $settingsText.Substring($match.Index + $match.Length)
        $temporaryFavored = $favoredSettingsPath + '.kmg-traits.tmp'
        [IO.File]::WriteAllText($temporaryFavored, $updated,
            (New-Object Text.UTF8Encoding($false)))
        Move-Item -LiteralPath $temporaryFavored -Destination `
            $favoredSettingsPath -Force
        $resolvedSettings = Get-Content -Raw -LiteralPath `
            $favoredSettingsPath | ConvertFrom-Json
        $expectedTraits = $favoredTraitsMode -ceq 'enabled'
        if ($resolvedSettings.enable_traits -isnot [bool] -or
            [bool]$resolvedSettings.enable_traits -ne $expectedTraits) {
            throw 'Staged Favored Class enable_traits did not resolve to the requested mode.'
        }
        $favoredSettingsStagedAfterSha = (Get-FileHash -LiteralPath `
            $favoredSettingsPath -Algorithm SHA256).Hash
    }
    if ($ExpandedSummoningDisabled) {
        $settingsPath = Join-Path $KingmakerInstallDir 'Mods\KingmakerGunslinger\FeatureModules.json'
        $settingsOff = Get-KmgExpandedSummoningOffSettingsBytes -SettingsPath $settingsPath `
            -AllowMissingDefaults:$cleanup2ModuleOff
        [IO.File]::WriteAllBytes($settingsPath, $settingsOff)
    }
    if ($moduleScenario) {
        $settingsPath = Join-Path $KingmakerInstallDir `
            'Mods\KingmakerGunslinger\FeatureModules.json'
        $settings = [ordered]@{ schemaVersion = 12
            gunslinger = [bool]$Parameters.gunslinger
            'acadamae-graduate' = [bool]$Parameters.acadamaeGraduate
            'shield-other' = [bool]$Parameters.shieldOther
            'expanded-summoning' = [bool]$Parameters.expandedSummoning
            'elven-branched-spears' = [bool]$Parameters.elvenBranchedSpears
            'eastern-weapons' = [bool]$Parameters.easternWeapons
            'brown-fur-transmuter' = [bool]$Parameters.brownFurTransmuter
            'urban-barbarian' = [bool]$Parameters.urbanBarbarian
            'bodyguard-feats' = [bool]$Parameters.bodyguardFeats
            'protection-from-alignment-control-immunity' =
                [bool]$Parameters.protectionFromAlignmentControlImmunity
            'elemental-races' = [bool]$Parameters.elementalRaces
            'teleportation-spells' = [bool]$Parameters.teleportationSpells
            'magic-circle-spells' = [bool]$Parameters.magicCircleSpells }
        $temporary = $settingsPath + '.kmg-profile.tmp'
        [IO.File]::WriteAllText($temporary,
            ($settings | ConvertTo-Json -Depth 4),
            (New-Object Text.UTF8Encoding($false)))
        Move-Item -LiteralPath $temporary -Destination $settingsPath -Force
    }
    # Enter staged this candidate in a fresh profile. Create its own validated
    # deployment receipt under the transaction lease, after profile settings.
    # The outer finally still restores the exact original profile intake.
    $deployArguments = @{ PackagePath = $PackagePath; LiveModDirectory = $liveModDirectory
        RuntimeLease = $compatRuntimeLease }
    if ($developmentCandidate) { $deployArguments.DevelopmentPurpose = $DevelopmentPurpose }
    & (Join-Path $root 'scripts\Deploy-Local.ps1') @deployArguments -WhatIf -Confirm:$false
    $profileDeploymentPath = & (Join-Path $root 'scripts\Deploy-Local.ps1') `
        @deployArguments -PassThru -Confirm:$false
    foreach ($name in $Scenario) {
        $before = [DateTime]::UtcNow
        $arguments = @{
            Scenario = $name
            ExpectedVersion = $expectedVersion
            ExitAfterCompletion = $true
            TimeoutSeconds = $RuntimeTimeoutSeconds
            ObserverStartupTimeoutSeconds = [Math]::Min($RuntimeTimeoutSeconds,600)
            # Leave room inside the whole-process budget for a native
            # load timeout to flush evidence and quit before restoration.
            CompletionTimeoutSeconds = if ($workingDispelUi) { 180 } else { $RuntimeTimeoutSeconds }
            Confirm = $false
            AllowDirtyGit = [bool]$AllowDirtyGit
        }
        if ($name -ceq 'observe-optional-mod-compatibility') {
            $arguments.CompatibilityProfileId = $ProfileId
        }
        if ($name -ceq 'observe-feature-module-settings') {
            $arguments.Parameters = $Parameters
        }
        if ($name -ceq 'observe-expanded-summoning-module-boundary') {
            $arguments.Parameters = @{
                expandedSummoning = [bool]$Parameters.expandedSummoning
            }
        }
        if ($name -cin @(
            'disposable-expanded-summoning-sprint22-native-review',
            'disposable-expanded-summoning-sprint23-native-review',
            'disposable-expanded-summoning-sprint24-native-review',
            'disposable-expanded-summoning-sprint25-native-review',
            'disposable-expanded-summoning-sprint26-native-review',
            'disposable-expanded-summoning-sprint27-native-review',
            'disposable-expanded-summoning-sprint28-native-review',
            'disposable-expanded-summoning-sprint29-native-review',
            'disposable-expanded-summoning-sprints30-31-native-review',
            'disposable-expanded-summoning-sprint32-native-review',
            'disposable-expanded-summoning-cleanup2-native-review',
            'working-save-expanded-summoning-creature-review')) {
            $arguments.Parameters = $Parameters
        }
        if ($name -ceq 'observe-kmg-compatibility-asset-attribution') {
            $arguments.Parameters = $Parameters
        }
        $arguments.ReuseInstalledArtifact = $true
        $arguments.DeploymentManifestPath = $profileDeploymentPath
        $arguments.PackagePath = $PackagePath
        if ($developmentCandidate) { $arguments.DevelopmentPurpose = $DevelopmentPurpose }
        if ($name -in @('musket-master-mechanics-and-starter',
            'disposable-expanded-summoning',
            'working-save-smoke', 'working-save-expanded-summoning-creature-review', 'disposable-brown-fur-native-cast',
            'disposable-magic-circle-evil', 'disposable-magic-circle-ui', 'disposable-magic-circle-terrain',
            'observe-teleportation-world-map', 'disposable-teleportation-familiarity',
            'disposable-teleportation-resources',
            'disposable-teleportation-context',
            'disposable-teleportation-casting',
            'disposable-teleportation-interaction',
            'disposable-teleportation-travelers',
            'disposable-teleportation-gamepad',
            'disposable-teleportation-spellbook-ui',
            'disposable-teleportation-level-up',
            'disposable-teleportation-destinations',
            'disposable-teleportation-disabled',
            'summon-same-turn-activation', 'summon-same-turn-acadamae',
            'summon-same-turn-multiple', 'summon-same-turn-native-control',
            'summon-same-turn-rtwp-control')) {
            $arguments.SaveName = 'KMG_AUTOMATION_WORKING'
        }
        $arguments.RuntimeLease = $compatRuntimeLease
        $exactSprint22=(Test-KmgExactSummoningRuntimeBoundary $name) -or $workingDispelUi
        if($exactSprint22) {
            . (Join-Path $root 'scripts\ExpandedSummoningOrchestration.Common.ps1')
            $arguments.PassThru=$true
            $launchOutput=@(& (Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1') @arguments)
            $launchReceipt=Get-KmgExactRuntimeLaunchReceipt -Output $launchOutput
            $evidence=Get-Item -LiteralPath $launchReceipt.evidenceDirectory
        } else {
            & (Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1') @arguments
            $evidence = Get-ChildItem -LiteralPath 'C:\Dev\KingmakerGunslingerLab\runtime-evidence' `
            -Directory | Where-Object { $_.LastWriteTimeUtc -ge $before.AddSeconds(-2) } |
            Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
        }
        if ($null -eq $evidence -or -not (Test-Path -LiteralPath `
            (Join-Path $evidence.FullName 'runtime-result.json') -PathType Leaf)) {
            throw "Runtime scenario result directory was not resolved: $name"
        }
        $result = Get-Content -LiteralPath (Join-Path $evidence.FullName `
            'runtime-result.json') -Raw | ConvertFrom-Json
        if ($result.scenario -cne $name -or $result.status -cne 'PASS' -or
            ($exactSprint22 -and $result.runId -cne $launchReceipt.runId)) {
            throw "Runtime scenario result mismatch: expected $name PASS."
        }
        $exitDeadline = [DateTime]::UtcNow.AddSeconds(60)
        while (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and
            [DateTime]::UtcNow -lt $exitDeadline) { Start-Sleep -Milliseconds 500 }
        if (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0) {
            throw "Kingmaker did not complete guarded automatic exit after scenario: $name"
        }
        $configurationId = $ProfileId + '-' + $name
        if ($name -cin @(
            'disposable-expanded-summoning-sprint22-native-review',
            'disposable-expanded-summoning-sprint23-native-review',
            'disposable-expanded-summoning-sprint24-native-review',
            'disposable-expanded-summoning-sprint25-native-review',
            'disposable-expanded-summoning-sprint26-native-review',
            'disposable-expanded-summoning-sprint27-native-review',
            'disposable-expanded-summoning-sprint28-native-review',
            'disposable-expanded-summoning-sprint29-native-review',
            'disposable-expanded-summoning-sprints30-31-native-review',
            'disposable-expanded-summoning-sprint32-native-review',
    'disposable-expanded-summoning-cleanup2-native-review')) {
            $configurationId += '-' + [string]$Parameters.reviewPhase
        }
        if ($name -ceq 'observe-kmg-compatibility-asset-attribution') {
            $configurationId += '-' + [string]$Parameters.assetConfiguration
        }
        if ($name -ceq 'disposable-expanded-summoning-sprint28-native-review') {
            $configurationId = 's28-' + $ProfileId + '-' + [string]$Parameters.reviewPhase
        }
        if ($name -ceq 'disposable-expanded-summoning-sprint29-native-review') {
            $configurationId = 's29-' + $ProfileId + '-' + [string]$Parameters.reviewPhase
        }
        if ($name -ceq 'disposable-expanded-summoning-sprint32-native-review') {
            $configurationId = 's32-' + $ProfileId + '-' + [string]$Parameters.reviewPhase
        }
        if ($name -ceq 'disposable-expanded-summoning-sprints30-31-native-review') {
            $configurationId = 's30-31-' + $ProfileId + '-' + [string]$Parameters.reviewPhase
        }
        elseif ($name -ceq 'disposable-expanded-summoning-cleanup2-native-review') {
            $configurationId = 'cleanup2-' + $ProfileId + '-' + [string]$Parameters.reviewPhase
        }
        if ($name -ceq 'disposable-expanded-summoning-sprint27-native-review' -and
            $configurationId.Length -gt 120) {
            # Only the evidence basename is shortened; collector validation and
            # the original native request/profile identity remain unchanged.
            $configurationId = 's27-' + $ProfileId + '-' + [string]$Parameters.reviewPhase
        }
        $collection = & (Join-Path $PSScriptRoot `
            'Collect-KmgCompatibilityAttributionLog.ps1') `
            -EvidenceDirectory $evidence.FullName `
            -ConfigurationId $configurationId `
            -KingmakerInstallDir $KingmakerInstallDir
        $results.Add([ordered]@{ scenario = $name; runId = $result.runId
            status = $result.status; evidenceDirectory = $evidence.FullName
            runtimeResultSha256 = (Get-FileHash -LiteralPath (Join-Path `
                $evidence.FullName 'runtime-result.json') -Algorithm SHA256).Hash
            outputLogSha256 = $collection.rawLogSha256
            logSummaryPath = $collection.summaryPath
            logSummarySha256 = $collection.summarySha256 })
    }
}
catch {
    $primaryError = $_
}
finally {
    # Cleanup 2 save-free reviews also run under an outer protected-save
    # catalog in this same process. Keep its OS handles AND this lease alive
    # if native result writing suppresses automatic exit. A throw here would
    # otherwise unwind the original standalone host and release those guards.
    if ($entered -and $Scenario.Count -eq 1 -and
        $Scenario[0] -ceq 'disposable-expanded-summoning-cleanup2-native-review') {
        try { Wait-KmgSummonCleanup2Exit }
        catch {
            $exitFailure = $_
            if ($null -eq $primaryError) { $primaryError = $exitFailure }
            $exitRecord = [ordered]@{ schemaVersion=1; transactionRunId=$runId;
                scenario=$Scenario[0]; status='FAILED'; restoration=$null }
            Wait-KmgSummonCleanup2ProtectedExit -Record $exitRecord `
                -RecordPath (Join-Path $StateRoot "$runId/cleanup2-protected-exit.json") `
                -ExitFailure $exitFailure
        }
    }
    if ($null -ne $compatRuntimeLease) { $compatRuntimeLease.Stream.Dispose() }
    if ($entered) {
        $deadline = [DateTime]::UtcNow.AddSeconds(60)
        while (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and
            [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 500 }
        try {
            if($sprint22Boundary) {
                Assert-KmgNotRunning
                $boundary=Invoke-KmgIndependentBoundaryRestoration -RestoreSettings {
                    $nativeKey=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($sprint22SettingsKey,$true)
                    if($null -eq $nativeKey) { throw 'Native settings key disappeared during the owned profile.' }
                    try { Restore-KmgFlatGameSettingsValues -Key $nativeKey -Snapshot $sprint22SettingsSnapshot }
                    finally { $nativeKey.Dispose() }
                } -RestoreInstallation { Restore-KmgCompatibilityTransaction -RunId $runId -StateRoot $StateRoot }
                Write-KmgCompatibilityJsonAtomic (Join-Path $StateRoot "$runId\game-settings-restoration.json") ([ordered]@{
                    exact=($null -eq $boundary.settingsError);installationVerified=($null -ne $boundary.transaction -and $boundary.transaction.restorationVerified);
                    settingsError=if($boundary.settingsError){$boundary.settingsError.Exception.Message}else{$null};
                    installationError=if($boundary.installationError){$boundary.installationError.Exception.Message}else{$null}})
                if($boundary.installationError) { throw $boundary.installationError }
                if($boundary.settingsError) { throw $boundary.settingsError }
            } else {
                & (Join-Path $PSScriptRoot 'Restore-KingmakerCompatibilityProfile.ps1') `
                    -RunId $runId -StateRoot $StateRoot -Confirm:$false | Out-Host
            }
        }
        catch {
            if ($null -ne $primaryError) {
                throw "Profile failed: $($primaryError.Exception.Message) Restoration also failed: $($_.Exception.Message)"
            }
            throw
        }
    }
}
if ($null -ne $primaryError) { throw $primaryError }
$state = Get-Content -LiteralPath (Join-Path $StateRoot "$runId\transaction.json") `
    -Raw | ConvertFrom-Json
if (-not $state.restorationVerified -or $state.status -cne 'Restored') {
    throw "Profile completed but exact restoration was not verified: $runId"
}
$featureSettingsRestoredExists = Test-Path -LiteralPath $featureSettingsPath `
    -PathType Leaf
$featureSettingsRestoredSha = if ($featureSettingsRestoredExists) {
    (Get-FileHash -LiteralPath $featureSettingsPath -Algorithm SHA256).Hash
} else { '<absent>' }
$featureSettingsBytesRestored =
    $featureSettingsRestoredExists -eq $featureSettingsOriginalExists
if ($featureSettingsBytesRestored -and $featureSettingsOriginalExists) {
    $featureSettingsRestoredBytes = [IO.File]::ReadAllBytes($featureSettingsPath)
    $featureSettingsBytesRestored =
        [Convert]::ToBase64String($featureSettingsRestoredBytes) -ceq
        [Convert]::ToBase64String($featureSettingsOriginalBytes)
}
if (-not $featureSettingsBytesRestored) {
    throw "FeatureModules.json bytes were not restored exactly: $runId"
}
$featureSettingsEvidencePath = Join-Path $StateRoot `
    "$runId\feature-modules-restoration.json"
[ordered]@{
    schemaVersion = 1
    originalExists = $featureSettingsOriginalExists
    originalSha256 = $featureSettingsOriginalSha
    restoredExists = $featureSettingsRestoredExists
    restoredSha256 = $featureSettingsRestoredSha
    exactBytesRestored = $featureSettingsBytesRestored
} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath `
    $featureSettingsEvidencePath -Encoding UTF8
$cotwSettingsRestoredExists = Test-Path -LiteralPath $cotwSettingsPath `
    -PathType Leaf
$cotwSettingsRestoredSha = if ($cotwSettingsRestoredExists) {
    (Get-FileHash -LiteralPath $cotwSettingsPath -Algorithm SHA256).Hash
} else { $null }
$cotwSettingsBytesRestored =
    $cotwSettingsOriginalExists -eq $cotwSettingsRestoredExists
if ($cotwSettingsBytesRestored -and $cotwSettingsOriginalExists) {
    $restoredBytes = [IO.File]::ReadAllBytes($cotwSettingsPath)
    $cotwSettingsBytesRestored =
        $restoredBytes.Length -eq $cotwSettingsOriginalBytes.Length
    if ($cotwSettingsBytesRestored) {
        for ($index = 0; $index -lt $restoredBytes.Length; $index++) {
            if ($restoredBytes[$index] -ne $cotwSettingsOriginalBytes[$index]) {
                $cotwSettingsBytesRestored = $false
                break
            }
        }
    }
}
$cotwSettingsEvidencePath = Join-Path $StateRoot `
    "$runId\cotw-settings-profile.json"
[ordered]@{
    schemaVersion = 1
    progressionMode = $CotwProgressionMode
    originalExisted = $cotwSettingsOriginalExists
    originalSha256 = $cotwSettingsOriginalSha
    stagedBeforeSha256 = $cotwSettingsStagedBeforeSha
    stagedAfterSha256 = $cotwSettingsStagedAfterSha
    restoredExisted = $cotwSettingsRestoredExists
    restoredSha256 = $cotwSettingsRestoredSha
    exactBytesRestored = $cotwSettingsBytesRestored
} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath `
    $cotwSettingsEvidencePath -Encoding UTF8
if (-not $cotwSettingsBytesRestored) {
    throw "CotW settings bytes were not restored exactly: $runId"
}
$favoredSettingsEvidencePath = Join-Path $StateRoot `
    "$runId\favored-settings-profile.json"
[ordered]@{
    schemaVersion = 1
    traitsMode = $favoredTraitsMode
    stagedBeforeSha256 = $favoredSettingsStagedBeforeSha
    stagedAfterSha256 = $favoredSettingsStagedAfterSha
    completeModsRestorationVerified = [bool]$state.restorationVerified
} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath `
    $favoredSettingsEvidencePath -Encoding UTF8
$profileReceipt = [ordered]@{
    profileId = $ProfileId
    transactionRunId = $runId
    restorationVerified = $true
    stagedMutationObserved = [bool]$state.stagedMutationObserved
    cotwProgressionMode = $CotwProgressionMode
    cotwSettingsStagedSha256 = $cotwSettingsStagedAfterSha
    cotwSettingsRestoredSha256 = $cotwSettingsRestoredSha
    cotwSettingsBytesRestored = $cotwSettingsBytesRestored
    cotwSettingsEvidencePath = $cotwSettingsEvidencePath
    favoredTraitsMode = $favoredTraitsMode
    favoredSettingsStagedBeforeSha256 = $favoredSettingsStagedBeforeSha
    favoredSettingsStagedAfterSha256 = $favoredSettingsStagedAfterSha
    favoredSettingsEvidencePath = $favoredSettingsEvidencePath
    featureSettingsOriginalSha256 = $featureSettingsOriginalSha
    featureSettingsRestoredSha256 = $featureSettingsRestoredSha
    featureSettingsExactBytesRestored = $featureSettingsBytesRestored
    featureSettingsEvidencePath = $featureSettingsEvidencePath
    runtimeResults = @($results)
}
if ($developmentCandidate) {
    $profileReceipt.candidateKind = 'development'
    $profileReceipt.releaseQualification = $false
    $profileReceipt.developmentPurpose = $DevelopmentPurpose
    $profileReceipt.packageSha256 = $profileCandidate.packageSha256
}
if (-not $developmentCandidate -and [IO.Path]::GetFullPath($root).TrimEnd('\') -iin @('C:\Dev\KingmakerGunslingerLab\wt\s30-31','C:\Dev\KingmakerGunslingerLab\wt\s33')) {
    # Actual validated final artifact bound to this restored profile and its
    # immutable runtime hashes. This is execution attribution, not qualification.
    $profileReceipt.candidateKind = 'release'
    $profileReceipt.commit = $profileCandidate.commit
    $profileReceipt.packageSha256 = $profileCandidate.packageSha256
    $profileReceipt.dllSha256 = $profileCandidate.dllSha256
    $profileReceipt.dllMvid = $profileCandidate.dllMvid
}
[pscustomobject]$profileReceipt
