Set-StrictMode -Version Latest

$script:KmgRuntimeEvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
$script:KmgRuntimeScenarioMetadata = [ordered]@{
    'mod-load-smoke' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-manual-save-load' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $true; ReadinessBehavior = 'manual-save-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-save-catalog-and-selection' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $true; ReadinessBehavior = 'catalog-selection'
        TimeoutCategory = 'catalog-selection'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $false
    }
    'observe-save-catalog-provider' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $true; ReadinessBehavior = 'catalog-provider'
        TimeoutCategory = 'catalog'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-load-game-button-action' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $true; ReadinessBehavior = 'load-game-action'
        TimeoutCategory = 'catalog'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'working-save-smoke' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-class-blueprint-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-character-creation-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-descriptor-construction' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-selection' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-preview-application' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-levelup-preview' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-multiclass-preview' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-respec-preview' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-grit-resource' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-grit-rest' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-grit-persistence' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-grit-recovery' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-deadeye' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-dodge' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-quick-clear' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-nimble' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-initiative' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-pistol-whip' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-stop-bleeding' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-bonus-feats' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-gun-training' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-dead-shot' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-startling-shot' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-targeting-head' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-targeting-torso' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-targeting-legs' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-bleeding-wound' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-expert-loading' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-lightning-reload' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-evasive' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-evasive-native-features' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-menacing-shot-native-fear' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-menacing-shot' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-slingers-luck-native-rerolls' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-slingers-luck' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-cheat-death' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-deaths-shot-native-death' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-stunning-shot-native-stunned' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-stunning-shot' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-true-grit' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'generic-firearm-actions' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'production-firearm-catalog' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'advanced-capacity' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'gunslinger-starting-items' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-working-save-entry-action' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $true; ReadinessBehavior = 'human-working-save-entry-action'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-working-save-selection-load-action' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $true; ReadinessBehavior = 'human-working-save-selection-load-action'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-working-save-receiver-bound-action' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $true; ReadinessBehavior = 'human-working-save-receiver-bound-action'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
}
$script:KmgRuntimeScenarios = @($script:KmgRuntimeScenarioMetadata.Keys)
$script:KmgSteamAppId = 640820
$script:KmgSteamExecutable = 'C:\Program Files (x86)\Steam\steam.exe'

function Assert-KmgRuntimeEvidenceDirectory {
    param([Parameter(Mandatory = $true)][string]$Path)
    $root = [IO.Path]::GetFullPath($script:KmgRuntimeEvidenceRoot).TrimEnd('\')
    $full = [IO.Path]::GetFullPath($Path).TrimEnd('\')
    if (-not $full.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Runtime evidence directory must be beneath $root"
    }
    return $full
}

function Get-KmgRuntimeScenarioMetadata {
    param([Parameter(Mandatory = $true)][string]$Scenario)
    if (-not $script:KmgRuntimeScenarioMetadata.Contains($Scenario)) {
        throw "Scenario is not allowlisted: $Scenario"
    }
    return $script:KmgRuntimeScenarioMetadata[$Scenario]
}

function Test-KmgSupervisedWorkingSaveEntryReadinessBehavior {
    param([Parameter(Mandatory = $true)][string]$ReadinessBehavior)
    return $ReadinessBehavior -cin @(
        'human-working-save-entry-action',
        'human-working-save-selection-load-action',
        'human-working-save-receiver-bound-action'
    )
}

function Assert-KmgRuntimeScenarioPreflight {
    param(
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds,
        [int]$StartupTimeoutSeconds = 180,
        [int]$CatalogTimeoutSeconds = 0,
        [int]$SelectionTimeoutSeconds = 0,
        [int]$CompletionTimeoutSeconds = 0,
        [int]$MainMenuTimeoutSeconds = 0,
        [int]$ActionResolutionTimeoutSeconds = 0,
        [int]$ActionInvocationTimeoutSeconds = 0,
        [int]$DescriptorResolutionTimeoutSeconds = 0,
        [int]$LoadEntryTimeoutSeconds = 0,
        [int]$FingerprintTimeoutSeconds = 0,
        [hashtable]$Parameters = @{},
        [switch]$EnforceManualInteraction,
        [switch]$ManualInteractionRequired
    )
    $metadata = Get-KmgRuntimeScenarioMetadata -Scenario $Scenario
    if ($ExpectedVersion -cne '0.0.60') {
        throw 'ExpectedVersion must be exactly the active version 0.0.60.'
    }
    if ($TimeoutSeconds -lt 5 -or $TimeoutSeconds -gt 1800) {
        throw 'TimeoutSeconds must be from 5 through 1800.'
    }
    if ($StartupTimeoutSeconds -lt 5 -or $StartupTimeoutSeconds -gt 600) {
        throw 'StartupTimeoutSeconds must be from 5 through 600.'
    }
    if ($EnforceManualInteraction) {
        if ($metadata.RequiresManualInteraction -and -not $ManualInteractionRequired) {
            throw "$Scenario requires -ManualInteractionRequired."
        }
        if (-not $metadata.RequiresManualInteraction -and $ManualInteractionRequired) {
            throw '-ManualInteractionRequired is valid only for supervised observations.'
        }
    }
    if ($metadata.RequiresSaveName) {
        if ($Parameters.Count -ne 1 -or
            -not $Parameters.ContainsKey('saveName') -or
            $Parameters.saveName -isnot [string] -or
            $Parameters.saveName -cne $metadata.PermittedSaveName) {
            throw "$Scenario requires exactly saveName=$($metadata.PermittedSaveName)."
        }
    }
    elseif ($Parameters.Count -ne 0) {
        throw "Scenario '$Scenario' does not accept parameters."
    }
    if ($metadata.UsesCatalogTimeout -and
        ($CatalogTimeoutSeconds -lt 5 -or $CatalogTimeoutSeconds -gt 1800)) {
        throw 'Catalog scenario timeout must be from 5 through 1800.'
    }
    if (-not $metadata.UsesCatalogTimeout -and $CatalogTimeoutSeconds -ne 0) {
        throw 'Catalog timeout is valid only for a catalog scenario.'
    }
    if ($metadata.UsesSelectionTimeouts -and
        ($SelectionTimeoutSeconds -lt 5 -or $SelectionTimeoutSeconds -gt 1800 -or
         $CompletionTimeoutSeconds -lt 5 -or $CompletionTimeoutSeconds -gt 1800)) {
        throw 'Catalog selection stage timeouts must be from 5 through 1800.'
    }
    if (-not $metadata.UsesSelectionTimeouts -and
        ($SelectionTimeoutSeconds -ne 0 -or $CompletionTimeoutSeconds -ne 0)) {
        throw 'Selection and completion timeouts are valid only for the selection scenario.'
    }
    if ($metadata.UsesWorkingStageTimeouts) {
        foreach ($stageTimeout in @($MainMenuTimeoutSeconds,
            $ActionResolutionTimeoutSeconds, $ActionInvocationTimeoutSeconds,
            $DescriptorResolutionTimeoutSeconds, $LoadEntryTimeoutSeconds,
            $FingerprintTimeoutSeconds)) {
            if ($stageTimeout -lt 5 -or $stageTimeout -gt 1800) {
                throw 'Working-save stage timeouts must be from 5 through 1800.'
            }
        }
    }
    elseif (@(@($MainMenuTimeoutSeconds, $ActionResolutionTimeoutSeconds,
        $ActionInvocationTimeoutSeconds, $DescriptorResolutionTimeoutSeconds,
        $LoadEntryTimeoutSeconds, $FingerprintTimeoutSeconds) |
        Where-Object { $_ -ne 0 }).Count -ne 0) {
        throw 'Working-save stage timeouts are valid only for working-save scenarios.'
    }
    return $metadata
}

function New-KmgRuntimeRequest {
    param(
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds,
        [int]$StartupTimeoutSeconds = 180,
        [int]$CatalogTimeoutSeconds = 0,
        [int]$SelectionTimeoutSeconds = 0,
        [int]$CompletionTimeoutSeconds = 0,
        [int]$MainMenuTimeoutSeconds = 0,
        [int]$ActionResolutionTimeoutSeconds = 0,
        [int]$ActionInvocationTimeoutSeconds = 0,
        [int]$DescriptorResolutionTimeoutSeconds = 0,
        [int]$LoadEntryTimeoutSeconds = 0,
        [int]$FingerprintTimeoutSeconds = 0,
        [Parameter(Mandatory = $true)][bool]$ExitAfterCompletion,
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [hashtable]$Parameters = @{}
    )
    $metadata = Assert-KmgRuntimeScenarioPreflight -Scenario $Scenario `
        -ExpectedVersion $ExpectedVersion -TimeoutSeconds $TimeoutSeconds `
        -StartupTimeoutSeconds $StartupTimeoutSeconds `
        -CatalogTimeoutSeconds $CatalogTimeoutSeconds `
        -SelectionTimeoutSeconds $SelectionTimeoutSeconds `
        -CompletionTimeoutSeconds $CompletionTimeoutSeconds `
        -MainMenuTimeoutSeconds $MainMenuTimeoutSeconds `
        -ActionResolutionTimeoutSeconds $ActionResolutionTimeoutSeconds `
        -ActionInvocationTimeoutSeconds $ActionInvocationTimeoutSeconds `
        -DescriptorResolutionTimeoutSeconds $DescriptorResolutionTimeoutSeconds `
        -LoadEntryTimeoutSeconds $LoadEntryTimeoutSeconds `
        -FingerprintTimeoutSeconds $FingerprintTimeoutSeconds `
        -Parameters $Parameters
    $evidence = Assert-KmgRuntimeEvidenceDirectory -Path $EvidenceDirectory
    $runId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '-' +
        [Guid]::NewGuid().ToString('N')
    return [ordered]@{
        schemaVersion = 1
        enabled = $true
        runId = $runId
        scenario = $Scenario
        expectedModVersion = $ExpectedVersion
        evidenceDirectory = $evidence
        timeoutSeconds = $TimeoutSeconds
        startupTimeoutSeconds = $StartupTimeoutSeconds
        catalogTimeoutSeconds = $CatalogTimeoutSeconds
        selectionTimeoutSeconds = $SelectionTimeoutSeconds
        completionTimeoutSeconds = $CompletionTimeoutSeconds
        exitAfterCompletion = $ExitAfterCompletion
        mainMenuTimeoutSeconds = $MainMenuTimeoutSeconds
        actionResolutionTimeoutSeconds = $ActionResolutionTimeoutSeconds
        actionInvocationTimeoutSeconds = $ActionInvocationTimeoutSeconds
        descriptorResolutionTimeoutSeconds = $DescriptorResolutionTimeoutSeconds
        loadEntryTimeoutSeconds = $LoadEntryTimeoutSeconds
        fingerprintTimeoutSeconds = $FingerprintTimeoutSeconds
        parameters = if ($metadata.RequiresSaveName) {
            [ordered]@{ saveName = [string]$Parameters.saveName }
        } else { [ordered]@{} }
    }
}

function Write-KmgUtf8NoBom {
    param(
        [Parameter(Mandatory = $true)][AllowNull()][object]$Path,
        [Parameter(Mandatory = $true)][string]$Content
    )
    $stage = 'validate-path'
    $destination = $null
    $temporary = $null
    try {
        if ($Path -isnot [string]) {
            throw 'Path must be exactly one scalar string.'
        }
        if ([string]::IsNullOrWhiteSpace($Path)) {
            throw 'Path must not be null, empty, or whitespace.'
        }
        if ($Path.Length -ge 2 -and $Path[0] -eq '"' -and
            $Path[$Path.Length - 1] -eq '"') {
            throw 'Path must not contain literal surrounding quotes.'
        }
        if ($Path.IndexOfAny([IO.Path]::GetInvalidPathChars()) -ge 0) {
            throw 'Path contains an invalid path character.'
        }
        if ($Path -match '^[A-Za-z][A-Za-z0-9+.-]*://' -or
            $Path -match '^[A-Za-z]+::') {
            throw 'Path must be a filesystem path, not a URI or provider path.'
        }

        $destination = [IO.Path]::GetFullPath($Path)
        $fileName = [IO.Path]::GetFileName($destination)
        if ([string]::IsNullOrWhiteSpace($fileName)) {
            throw 'Path must include a filename.'
        }
        if ($fileName.IndexOfAny([IO.Path]::GetInvalidFileNameChars()) -ge 0) {
            throw 'Filename contains an invalid character.'
        }
        if (Test-Path -LiteralPath $destination -PathType Container) {
            throw 'Path resolves to a directory.'
        }

        $directory = [IO.Path]::GetDirectoryName($destination)
        if ([string]::IsNullOrWhiteSpace($directory)) {
            throw 'Path must have a valid parent directory.'
        }
        if (Test-Path -LiteralPath $directory) {
            if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
                throw 'The destination parent exists but is not a directory.'
            }
        }
        else {
            $stage = 'create-parent'
            [void][IO.Directory]::CreateDirectory($directory)
        }

        $temporary = Join-Path $directory (
            ".$fileName.$([Guid]::NewGuid().ToString('N')).tmp")
        $stage = 'write-temporary'
        $bytes = [Text.UTF8Encoding]::new($false).GetBytes($Content)
        $stream = [IO.File]::Open(
            $temporary,
            [IO.FileMode]::CreateNew,
            [IO.FileAccess]::Write,
            [IO.FileShare]::None)
        try {
            $stream.Write($bytes, 0, $bytes.Length)
            $stream.Flush($true)
        }
        finally {
            $stream.Dispose()
        }

        if (Test-Path -LiteralPath $destination) {
            if (-not (Test-Path -LiteralPath $destination -PathType Leaf)) {
                throw 'Destination is not a regular file.'
            }
            $stage = 'replace-destination'
            # Windows PowerShell 5.1 coerces a direct $null argument to an
            # empty string for File.Replace. NullString.Value preserves an
            # actual null through PowerShell's .NET method binder.
            [IO.File]::Replace(
                $temporary,
                $destination,
                [Management.Automation.Language.NullString]::Value)
        }
        else {
            $stage = 'move-new-destination'
            [IO.File]::Move($temporary, $destination)
        }
    }
    catch {
        $safeDestination = if ($destination) {
            ($destination -replace '[\x00-\x1f\x7f]', '?')
        }
        elseif ($Path -is [string]) {
            ($Path -replace '[\x00-\x1f\x7f]', '?')
        }
        else {
            '<non-scalar>'
        }
        throw "Atomic write failed at stage '$stage' for destination '$safeDestination': $($_.Exception.Message)"
    }
    finally {
        if ($temporary -and (Test-Path -LiteralPath $temporary -PathType Leaf)) {
            Remove-Item -LiteralPath $temporary -Force
        }
    }
}

function Test-KmgRuntimeStageMarker {
    param(
        [Parameter(Mandatory = $true)][object]$Marker,
        [Parameter(Mandatory = $true)][string]$RunId,
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$Stage,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][int]$ProcessId,
        [Parameter(Mandatory = $true)][DateTime]$RequestWrittenUtc
    )
    try {
        $utc = [DateTime]::Parse($Marker.timestampUtc,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
        return $Marker.schemaVersion -eq 1 -and
            $Marker.runId -ceq $RunId -and $Marker.scenario -ceq $Scenario -and
            $Marker.stage -ceq $Stage -and
            $Marker.loadedModVersion -ceq $ExpectedVersion -and
            $Marker.processId -eq $ProcessId -and
            $utc -ge $RequestWrittenUtc.ToUniversalTime()
    }
    catch { return $false }
}

function Test-KmgRuntimeReadyMarker {
    param(
        [Parameter(Mandatory = $true)][object]$Marker,
        [Parameter(Mandatory = $true)][string]$RunId,
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][int]$ProcessId,
        [Parameter(Mandatory = $true)][DateTime]$RequestWrittenUtc,
        [ref]$FailedPredicates
    )
    $failures = [Collections.Generic.List[string]]::new()
    try {
        $readyUtc = [DateTime]::Parse(
            $Marker.readinessTimestampUtc,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
        if ($Marker.schemaVersion -ne 1) { $failures.Add('schemaVersion') }
        if ($Marker.runId -cne $RunId) { $failures.Add('runId') }
        if ($Marker.scenario -cne $Scenario) { $failures.Add('scenario') }
        if ($Marker.loadedModVersion -cne $ExpectedVersion) {
            $failures.Add('loadedModVersion')
        }
        if ($Marker.processId -ne $ProcessId) { $failures.Add('processId') }
        if ($readyUtc -lt $RequestWrittenUtc.ToUniversalTime()) {
            $failures.Add('freshness')
        }
        if (@($Marker.installedObservationHookIdentifiers).Count -le 0) {
            $failures.Add('installedObservationHookIdentifiers')
        }
        $metadata = Get-KmgRuntimeScenarioMetadata -Scenario $Scenario
        if (-not $metadata.UsesWorkingStageTimeouts) {
            if ($PSBoundParameters.ContainsKey('FailedPredicates')) {
                $FailedPredicates.Value = @($failures)
            }
            return $failures.Count -eq 0
        }
        $isSupervisedWorkingEntry =
            Test-KmgSupervisedWorkingSaveEntryReadinessBehavior `
                -ReadinessBehavior $metadata.ReadinessBehavior
        $receiverBound = $metadata.ReadinessBehavior -ceq
            'human-working-save-receiver-bound-action'
        $expectedStage = if ($receiverBound) {
            'working-receiver-bound-action-ready'
        } elseif ($isSupervisedWorkingEntry) {
            'working-entry-ready'
        } else {
            'load-game-action-resolved'
        }
        if ($isSupervisedWorkingEntry -and
            $Marker.saveName -cne 'KMG_AUTOMATION_WORKING') {
            $failures.Add('saveName')
        }
        if ($Marker.runtimeRunnerActive -ne $true) {
            $failures.Add('runtimeRunnerActive')
        }
        if ($Marker.updateCallbackCount -lt 2) { $failures.Add('updateCallbackCount') }
        if ($Marker.mainMenuLifecycleReady -ne $true) {
            $failures.Add('mainMenuLifecycleReady')
        }
        if ($Marker.ummStartupState -cne
            'initialized; overlay nonblocking-or-absent') {
            $failures.Add('ummStartupState')
        }
        if ($Marker.readinessStage -cne $expectedStage) {
            $failures.Add('readinessStage')
        }
        if ($receiverBound) {
            if ([string]::IsNullOrWhiteSpace([string]$Marker.exactSlotIdentity)) {
                $failures.Add('exactSlotIdentity')
            }
            if ([string]::IsNullOrWhiteSpace([string]$Marker.exactWindowIdentity)) {
                $failures.Add('exactWindowIdentity')
            }
            $requiredHooks = @(
                'Kingmaker.UI.SaveLoadWindow.SaveSlot.OnButtonSaveLoad():System.Void',
                'Kingmaker.UI.SaveLoadWindow.SaveLoadWindow.HandleHardcodeMainMenuSaveLoad(Kingmaker.EntitySystem.Persistence.SaveInfo):System.Void',
                'Kingmaker.MainMenu.LoadGame(Kingmaker.EntitySystem.Persistence.SaveInfo):System.Void'
            )
            foreach ($requiredHook in $requiredHooks) {
                if ($requiredHook -cnotin @($Marker.installedObservationHookIdentifiers)) {
                    $failures.Add("installedExactHook:$requiredHook")
                }
            }
        }
        if ($PSBoundParameters.ContainsKey('FailedPredicates')) {
            $FailedPredicates.Value = @($failures)
        }
        return $failures.Count -eq 0
    }
    catch {
        $failures.Add('markerSchemaOrTimestamp')
        if ($PSBoundParameters.ContainsKey('FailedPredicates')) {
            $FailedPredicates.Value = @($failures)
        }
        return $false
    }
}

function Get-KmgCurrentRuntimeResult {
    param(
        [Parameter(Mandatory = $true)][string]$ResultPath,
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [Parameter(Mandatory = $true)][string]$RunId,
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][DateTime]$RequestWrittenUtc
    )
    if (-not (Test-Path -LiteralPath $ResultPath -PathType Leaf)) { return $null }
    $item = Get-Item -LiteralPath $ResultPath
    if ($item.LastWriteTimeUtc -lt $RequestWrittenUtc.ToUniversalTime()) {
        throw 'The final runtime result is stale.'
    }
    try { $result = Get-Content -LiteralPath $ResultPath -Raw | ConvertFrom-Json }
    catch { throw "The final runtime result schema is unreadable: $($_.Exception.Message)" }
    $validStatuses = @('PASS', 'FAIL', 'AMBIGUOUS', 'ERROR', 'TIMEOUT')
    if ($result.schemaVersion -ne 1 -or $result.runId -cne $RunId -or
        $result.scenario -cne $Scenario -or
        $result.loadedModVersion -cne $ExpectedVersion -or
        $result.status -cnotin $validStatuses) {
        throw 'The final runtime result schema, run identity, scenario, version, or status is invalid.'
    }
    $expectedDirectory = [IO.Path]::GetFullPath($EvidenceDirectory).TrimEnd(
        [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    $actualPath = [IO.Path]::GetFullPath($ResultPath)
    if (-not $actualPath.StartsWith($expectedDirectory,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The final runtime result is outside the current evidence directory.'
    }
    if (-not ($result.PSObject.Properties.Name -contains 'evidenceDirectory') -or
        [string]::IsNullOrWhiteSpace([string]$result.evidenceDirectory)) {
        throw 'The final runtime result does not name its evidence directory.'
    }
    $namedDirectory = [IO.Path]::GetFullPath(
        [string]$result.evidenceDirectory).TrimEnd(
            [IO.Path]::DirectorySeparatorChar)
    if (-not $namedDirectory.Equals(
        $expectedDirectory.TrimEnd([IO.Path]::DirectorySeparatorChar),
        [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The final runtime result names a different evidence directory.'
    }
    return $result
}

function Wait-KmgRuntimeResultFlushGrace {
    param(
        [Parameter(Mandatory = $true)][string]$ResultPath,
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [Parameter(Mandatory = $true)][string]$RunId,
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][DateTime]$RequestWrittenUtc,
        [ValidateRange(100, 5000)][int]$GraceMilliseconds = 1000,
        [ValidateRange(10, 250)][int]$PollMilliseconds = 50
    )
    # The first read is the mandatory final rescan. Further reads are bounded
    # solely to allow an already-committed atomic rename to become visible.
    $deadline = [DateTime]::UtcNow.AddMilliseconds($GraceMilliseconds)
    do {
        $result = Get-KmgCurrentRuntimeResult -ResultPath $ResultPath `
            -EvidenceDirectory $EvidenceDirectory -RunId $RunId `
            -Scenario $Scenario -ExpectedVersion $ExpectedVersion `
            -RequestWrittenUtc $RequestWrittenUtc
        if ($null -ne $result) { return $result }
        if ([DateTime]::UtcNow -ge $deadline) { return $null }
        Start-Sleep -Milliseconds $PollMilliseconds
    } while ($true)
}

function Initialize-KmgRuntimeTestEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Request,
        [Parameter(Mandatory = $true)][AllowNull()][object]$DeploymentManifestPath
    )
    $requestPath = Join-Path $EvidenceDirectory 'runtime-request.json'
    $resultPath = Join-Path $EvidenceDirectory 'runtime-result.json'
    $orchestration = [ordered]@{
        schemaVersion = 3
        runId = $Request.runId
        status = 'PREPARING'
        startedAtUtc = [DateTime]::UtcNow.ToString('o')
        requestPath = $requestPath
        resultPath = $resultPath
        deploymentCompleted = $true
        deploymentManifestPath = '<unavailable>'
        launchBegan = $false
        saveInteractionOccurred = $false
        guardedRequestAccepted = $false
        preLaunchKingmakerProcesses = @()
    }
    try {
        if ($DeploymentManifestPath -isnot [string] -or
            [string]::IsNullOrWhiteSpace($DeploymentManifestPath)) {
            throw 'Deployment manifest path must be exactly one scalar string.'
        }
        $deploymentPath = [IO.Path]::GetFullPath($DeploymentManifestPath)
        if (-not (Test-Path -LiteralPath $deploymentPath -PathType Leaf)) {
            throw 'The completed deployment manifest is missing.'
        }
        $orchestration.deploymentManifestPath = $deploymentPath
        if (Test-Path -LiteralPath $resultPath) {
            throw 'A runtime result already exists before request creation.'
        }
        Write-KmgUtf8NoBom -Path $requestPath `
            -Content (($Request | ConvertTo-Json -Depth 8) + [Environment]::NewLine)
        $orchestration.status = 'ACTIVE'
        [void](Write-KmgOrchestrationEvidence `
            -EvidenceDirectory $EvidenceDirectory -Record $orchestration)
        return [ordered]@{
            requestPath = $requestPath
            resultPath = $resultPath
            orchestration = $orchestration
        }
    }
    catch {
        $failure = $_
        $orchestration.status = 'ERROR'
        $orchestration.completedAtUtc = [DateTime]::UtcNow.ToString('o')
        $orchestration.failingOperation = 'pre-launch-request-and-evidence-write'
        $orchestration.exception = [ordered]@{
            type = $failure.Exception.GetType().FullName
            message = $failure.Exception.Message
        }
        try {
            [void](Write-KmgOrchestrationEvidence `
                -EvidenceDirectory $EvidenceDirectory -Record $orchestration)
        }
        catch {
            throw "Pre-launch request creation failed and ERROR evidence could not be written: $($failure.Exception.Message)"
        }
        throw $failure
    }
}

function Assert-KmgSteamAppId {
    param([Parameter(Mandatory = $true)][int]$AppId)
    if ($AppId -ne $script:KmgSteamAppId) {
        throw "Steam App ID must be exactly $($script:KmgSteamAppId); received $AppId."
    }
}

function Assert-KmgSteamExecutable {
    param([Parameter(Mandatory = $true)][string]$SteamPath)
    $expected = [IO.Path]::GetFullPath($script:KmgSteamExecutable)
    $actual = [IO.Path]::GetFullPath($SteamPath)
    if (-not $actual.Equals($expected, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Steam executable must be exactly: $expected"
    }
    if (-not (Test-Path -LiteralPath $actual -PathType Leaf)) {
        throw "Steam executable is missing: $actual"
    }
    return $actual
}

function Assert-KmgUnelevated {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    if ($principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Runtime tests must run without administrator elevation.'
    }
}

function Get-KmgProcessOwner {
    param([Parameter(Mandatory = $true)][int]$ProcessId)
    $instances = @(Get-CimInstance Win32_Process -Filter "ProcessId = $ProcessId")
    if ($instances.Count -ne 1) {
        throw "Process disappeared or was ambiguous before its owner could be verified: PID=$ProcessId"
    }
    # Invoke-CimMethod writes its result to the success stream. Capture it
    # completely so a CimMethodResult can never become part of a caller's
    # launch-result pipeline.
    $ownerResults = @(Invoke-CimMethod -InputObject $instances[0] -MethodName GetOwner)
    if ($ownerResults.Count -ne 1) {
        throw "Windows owner lookup returned $($ownerResults.Count) results for PID=$ProcessId."
    }
    $owner = $ownerResults[0]
    if ($owner.ReturnValue -ne 0 -or [string]::IsNullOrWhiteSpace([string]$owner.User)) {
        throw "Unable to verify the Windows user for PID=$ProcessId."
    }
    return [string]::Concat([string]$owner.Domain, '\', [string]$owner.User)
}

function Assert-KmgProcessOwner {
    param(
        [Parameter(Mandatory = $true)][int]$ProcessId,
        [Parameter(Mandatory = $true)][string]$ExpectedOwner,
        [Parameter(Mandatory = $true)][string]$Label
    )
    $actual = Get-KmgProcessOwner -ProcessId $ProcessId
    if (-not $actual.Equals($ExpectedOwner, [StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label is running as a different Windows user."
    }
    # Assertions are deliberately success-stream silent.
    return
}

function Get-KmgSteamLaunchArguments {
    param(
        [Parameter(Mandatory = $true)][int]$AppId,
        [string]$RequestPath
    )
    Assert-KmgSteamAppId -AppId $AppId
    $arguments = @('-applaunch', $AppId.ToString([Globalization.CultureInfo]::InvariantCulture))
    if (-not [string]::IsNullOrWhiteSpace($RequestPath)) {
        $safePath = Assert-KmgPathWithin -Path $RequestPath -Root $script:KmgRuntimeEvidenceRoot
        if (-not [IO.Path]::IsPathRooted($safePath) -or $safePath.Contains('"')) {
            throw 'The runtime request path cannot be quoted safely.'
        }
        $arguments += @('-kmgRuntimeTestRequest', "`"$safePath`"")
    }
    return $arguments
}

function Wait-KmgSteamProcess {
    param(
        [Parameter(Mandatory = $true)][string]$SteamPath,
        [ValidateRange(1, 300)][int]$TimeoutSeconds = 60
    )
    $SteamPath = Assert-KmgSteamExecutable -SteamPath $SteamPath
    $steam = @(Get-Process -Name steam -ErrorAction SilentlyContinue |
        Sort-Object StartTime, Id | Select-Object -First 1)
    if ($steam.Count -eq 0) {
        [void](Start-Process -FilePath $SteamPath -PassThru)
    }
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        $steam = @(Get-Process -Name steam -ErrorAction SilentlyContinue |
            Sort-Object StartTime, Id | Select-Object -First 1)
        if ($steam.Count -eq 1) { return $steam[0] }
        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "Steam client process did not become available within $TimeoutSeconds seconds."
}

function Select-KmgNewKingmakerProcess {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$Processes,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$ExistingProcesses,
        [Parameter(Mandatory = $true)][DateTime]$RequestedAtUtc
    )
    $existingIdentities = @($ExistingProcesses | ForEach-Object {
        '{0}:{1}' -f $_.Id, $_.StartTime.ToUniversalTime().Ticks
    })
    $matches = @($Processes | Where-Object {
        $identity = '{0}:{1}' -f $_.Id, $_.StartTime.ToUniversalTime().Ticks
        $_.ProcessName -eq 'Kingmaker' -and
        $identity -notin $existingIdentities -and
        $_.StartTime.ToUniversalTime() -ge $RequestedAtUtc.AddSeconds(-2)
    } | Sort-Object StartTime, Id)
    if ($matches.Count -gt 1) {
        throw "More than one newly launched Kingmaker process was found: $($matches.Id -join ', ')."
    }
    if ($matches.Count -eq 1) { return $matches[0] }
    return $null
}

function Start-KmgSteamKingmaker {
    param(
        [Parameter(Mandatory = $true)][string]$SteamPath,
        [Parameter(Mandatory = $true)][int]$AppId,
        [string]$RequestPath,
        [AllowEmptyCollection()]
        [Diagnostics.Process[]]$PreLaunchProcesses = @(),
        [ValidateRange(1, 300)][int]$SteamStartupTimeoutSeconds = 60,
        [ValidateRange(1, 300)][int]$GameStartupTimeoutSeconds = 60
    )
    Assert-KmgSteamAppId -AppId $AppId
    Assert-KmgUnelevated
    Assert-KmgNotRunning
    $PreLaunchProcesses = @($PreLaunchProcesses)
    $preExistingKingmaker = @($PreLaunchProcesses | Where-Object {
        $_.ProcessName -eq 'Kingmaker'
    })
    if ($preExistingKingmaker.Count -ne 0) {
        throw "Kingmaker was already running before Steam launch: PID=$($preExistingKingmaker.Id -join ', ')."
    }
    $currentOwner = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    $steam = Wait-KmgSteamProcess -SteamPath $SteamPath -TimeoutSeconds $SteamStartupTimeoutSeconds
    if ($steam.Path -and
        -not $steam.Path.Equals(
            [IO.Path]::GetFullPath($script:KmgSteamExecutable),
            [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The available Steam process does not use the approved Steam executable.'
    }
    Assert-KmgProcessOwner -ProcessId $steam.Id -ExpectedOwner $currentOwner -Label 'Steam'
    $arguments = @(Get-KmgSteamLaunchArguments -AppId $AppId -RequestPath $RequestPath)
    $requestedAt = [DateTime]::UtcNow
    [void](Start-Process -FilePath $SteamPath -ArgumentList $arguments -PassThru)
    $deadline = $requestedAt.AddSeconds($GameStartupTimeoutSeconds)
    do {
        $game = Select-KmgNewKingmakerProcess `
            -Processes @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) `
            -ExistingProcesses $PreLaunchProcesses -RequestedAtUtc $requestedAt
        if ($game) {
            Assert-KmgProcessOwner -ProcessId $game.Id -ExpectedOwner $currentOwner -Label 'Kingmaker'
            $launchResult = [pscustomobject][ordered]@{
                PSTypeName = 'KingmakerGunslinger.RuntimeLaunchResult'
                steamExecutable = (Resolve-Path -LiteralPath $SteamPath).Path
                steamAppId = $AppId
                sanitizedLaunchArguments = if ($RequestPath) {
                    '-applaunch 640820 -kmgRuntimeTestRequest "<approved-evidence-path>"'
                } else {
                    '-applaunch 640820'
                }
                steamProcessId = $steam.Id
                kingmakerProcess = $game
                kingmakerProcessId = $game.Id
                kingmakerStartedAtUtc = $game.StartTime.ToUniversalTime()
            }
            Write-Output -NoEnumerate $launchResult
            return
        }
        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "Kingmaker did not start through Steam App ID $AppId within $GameStartupTimeoutSeconds seconds; direct-executable fallback is disabled."
}

function Assert-KmgRuntimeLaunchResult {
    param([Parameter(Mandatory = $true)][AllowNull()][object]$LaunchResult)
    if ($null -eq $LaunchResult) {
        throw 'Steam launch returned no launch result.'
    }
    if ($LaunchResult -is [array]) {
        throw "Steam launch returned an array-valued result with $($LaunchResult.Count) entries."
    }
    if ($LaunchResult.PSObject.TypeNames -notcontains
        'KingmakerGunslinger.RuntimeLaunchResult') {
        throw 'Steam launch returned a malformed or untyped launch result.'
    }
    foreach ($property in @('steamExecutable', 'steamAppId', 'steamProcessId',
        'kingmakerProcess', 'kingmakerProcessId', 'kingmakerStartedAtUtc')) {
        if ($null -eq $LaunchResult.PSObject.Properties[$property] -or
            $null -eq $LaunchResult.$property) {
            throw "Steam launch result is missing required property '$property'."
        }
    }
    if ($LaunchResult.steamAppId -ne $script:KmgSteamAppId -or
        $LaunchResult.kingmakerProcess.ProcessName -ne 'Kingmaker' -or
        $LaunchResult.kingmakerProcessId -ne $LaunchResult.kingmakerProcess.Id) {
        throw 'Steam launch result does not identify the required Kingmaker process.'
    }
    return
}

function Write-KmgOrchestrationEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Record
    )
    $path = Join-Path $EvidenceDirectory 'orchestration.json'
    Write-KmgUtf8NoBom -Path $path `
        -Content (($Record | ConvertTo-Json -Depth 6) + [Environment]::NewLine)
    return $path
}
