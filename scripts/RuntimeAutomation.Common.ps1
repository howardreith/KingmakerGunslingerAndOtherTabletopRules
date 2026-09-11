Set-StrictMode -Version Latest

$script:KmgRuntimeEvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
$script:KmgRuntimeScenarioMetadata = [ordered]@{
    'mod-load-smoke' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-kmg-compatibility-asset-attribution' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'gunslinger-outfit-audit' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-elemental-race-blueprints' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-elemental-heritage-donors' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-elemental-feat-native-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-feat-mechanics' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-ifrit-feats' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-ifrit-advanced-feats' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-sylph-feats' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-undine-feats' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-elemental-heritage-blueprints' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'working-save-elemental-character-creation-regression' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-creator-visual-lifecycle' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-elemental-native-respec' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-elemental-nereid-creation' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-elemental-nereid-respec' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-elemental-character-creation' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-elemental-character-creation-baseline' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-nereid-creation' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-nereid-respec' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-character-creation-case' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-global-traits-kmg-disabled-control' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-elemental-character-creation-routing' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-treacherous-earth' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-nereid' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-trait-turn-costs' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-elemental-alternate-trait-framework' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-heritage-mechanics' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-heritage-slas' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'elemental-race-visual-audit' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'elemental-race-class-clothing' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-race-mechanics' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-spell-affinity' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-race-slas' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-hydraulic-push' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elemental-race-native-identity' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'elemental-races-races-unleashed-compatibility' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-optional-mod-compatibility' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-craft-magic-items-compatibility' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-craft-magic-items-ammunition-ui' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'working-save-craft-magic-items-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-craft-magic-items-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-firearm-wwise-audio' = [pscustomobject]@{
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
    'working-save-midgame-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-midgame-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-midgame-verify-absent' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-unified-repair-alias' = [pscustomobject]@{
        RequiresSaveName = $true
        PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false
        ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'p0-affected-focused-aim-save-load' = [pscustomobject]@{
        RequiresSaveName = $true
        PermittedSaveName = 'KMG_P0_FOCUSED_AIM_AFFECTED_COPY'
        RequiresManualInteraction = $false
        ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-in-harms-way-human-repro' = [pscustomobject]@{
        RequiresSaveName = $true
        PermittedSaveName = 'KMG_IHW_HUMAN_REPRO_COPY'
        RequiresManualInteraction = $false
        ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-in-harms-way-off-turn-economy' = [pscustomobject]@{
        RequiresSaveName = $true
        PermittedSaveName = 'KMG_IHW_HUMAN_REPRO_COPY'
        RequiresManualInteraction = $false
        ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-class-blueprint-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-pistolero-deeds' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'musket-master-mechanics-and-starter' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-gunslinger-presentation' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'icon-overhaul-visual-evidence' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-elven-branched-spear-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-eastern-weapon-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-elven-branched-spear-combat' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-eastern-weapons-combat' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'weapon-presentation-evidence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'weapon-presentation-motion-evidence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'weapon-presentation-handgun-motion-evidence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'weapon-presentation-spear-motion-evidence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'weapon-presentation-eastern-motion-evidence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'weapon-presentation-transition-motion-evidence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'weapon-presentation-reload-evidence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'weapon-presentation-body-matrix-evidence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'gunslinger-outfit-candidate-render' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'gunslinger-outfit-finalist-race-matrix' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'gunslinger-outfit-production-compatibility' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'elemental-race-class-equipment' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'elemental-race-motion' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-elemental-deferred-markers' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'elemental-race-persistence-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'elemental-race-module-disabled-persistence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'elemental-race-module-restored-persistence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'elemental-race-legacy-migration' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'elemental-race-persistence-verify-absent' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'gunslinger-outfit-production-motion' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'gunslinger-outfit-production-persistence-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'gunslinger-outfit-production-persistence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'gunslinger-outfit-production-persistence-verify-absent' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-elven-branched-spear-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-elven-branched-spear-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-elven-branched-spear-verify-absent' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-eastern-weapons-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-eastern-weapons-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-eastern-weapons-verify-absent' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-vendor-table-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-feature-module-settings' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-urban-barbarian-rage-inventory' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-urban-barbarian-focused' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'working-save-urban-barbarian-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-urban-barbarian-off-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-brown-fur-cotw-contract' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-brown-fur-cotw-absent-isolation' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-brown-fur-transmutation-inventory' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-brown-fur-cast-engine-contract' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-brown-fur-bonus-carriers' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-brown-fur-share-targeting' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-brown-fur-transmutation-supremacy' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-brown-fur-reservoir-accounting' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-brown-fur-player-intent' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-brown-fur-cast-execution' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-brown-fur-arcanist-slot' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-teleportation-coexistence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-coexistence-gamepad' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-persistence' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'transaction-owned persistence input'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-familiarity' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-resources' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-context' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-casting' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-interaction' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-travelers' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-gamepad' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-spellbook-ui' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-level-up' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-destinations' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-arrows' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-specialist' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-scrolls' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-teleportation-disabled' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-teleportation-world-map' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-brown-fur-native-cast' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-brown-fur-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-brown-fur-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-brown-fur-off-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-shield-other-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-shield-other-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'observe-shield-other-inventory' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-expanded-summoning-inventory' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-expanded-summoning-variant-menu' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $true
        ReadinessBehavior = 'expanded-summoning-variant-menu'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-expanded-summoning' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-expanded-summoning-player-path' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'summon-same-turn-activation' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'summon-same-turn-acadamae' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'summon-same-turn-multiple' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'summon-same-turn-native-control' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'summon-same-turn-rtwp-control' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'summon-same-turn-compatibility-quickened' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'summon-same-turn-compatibility-acadamae' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-expanded-summoning-visual-contracts' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-expanded-summoning-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-expanded-summoning-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-expanded-summoning-verify-absent' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-shield-other' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-capital-cord-vendor' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-cord-of-stubborn-resolve' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-acadamae-graduate' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-native-fatigue-refresh' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-acadamae-fatigue-escalation' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'working-save-fatigue-prepare' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-fatigue-verify-cleanup' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'working-save-fatigue-verify-absent' = [pscustomobject]@{
        RequiresSaveName = $true; PermittedSaveName = 'KMG_AUTOMATION_WORKING'
        RequiresManualInteraction = $false; ReadinessBehavior = 'autonomous-working-save'
        TimeoutCategory = 'working-save'; UsesCatalogTimeout = $true
        UsesSelectionTimeouts = $true; UsesWorkingStageTimeouts = $true
    }
    'disposable-focused-aim' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-firearm-penetration' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-rare-firearm-acquisition' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-rare-firearm-blueprint-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-teleportation-native-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-midgame-firearms' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'magic-firearm-native-properties' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'reliable-firearm-misfire-matrix' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'blunderbuss-thundering-scatter' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-production-firearm-fallbacks' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-native-firearm-rig-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-firearm-visual-rigs' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-firearm-item-lifecycle-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-production-firearm-switching' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-comprehensive-acceptance' = [pscustomobject]@{
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
    'disposable-gunslinger-levelup-commit' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-creation-commit' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-level-twenty-progression' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-evaluated-chassis' = [pscustomobject]@{
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
    'disposable-gunslinger-multiclass-commit' = [pscustomobject]@{
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
    'disposable-gunslinger-respec-commit' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-broad-respec' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-archetype-reconciliation' = [pscustomobject]@{
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
    'disposable-gunslinger-scatter-shot' = [pscustomobject]@{
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
    'disposable-gunslinger-targeting-arms' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-gunslinger-deaths-shot' = [pscustomobject]@{
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
    'observe-bodyguard-native-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-aid-another-compatibility-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-bodyguard-feats' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-helpful-bodyguard' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-bodyguard-feats-disabled' = [pscustomobject]@{
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
    'disposable-reload-autocast' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-paper-cartridge-reload' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-paper-cartridge-mode-view-lifecycle' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-paper-cartridge-full-attack' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-paper-cartridge-misfire' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-paper-cartridge-scatter' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-paper-cartridge-crafting-vendors' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-paper-cartridge-comprehensive' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-paper-cartridge-lightning-reload' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-overhaul-maintenance' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'observe-native-weapon-feat-contracts' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-firearm-dependent-feats' = [pscustomobject]@{
        RequiresSaveName = $false; PermittedSaveName = $null
        RequiresManualInteraction = $false; ReadinessBehavior = 'mod-load'
        TimeoutCategory = 'basic'; UsesCatalogTimeout = $false
        UsesSelectionTimeouts = $false; UsesWorkingStageTimeouts = $false
    }
    'disposable-empty-firearm-command' = [pscustomobject]@{
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

function Test-KmgNereidPersistenceScope {
    param([string]$Scenario, [hashtable]$Parameters)
    return $null -ne $Parameters -and $Parameters.ContainsKey('qualificationTrait') -and
        $Parameters.qualificationTrait -is [string] -and $Parameters.qualificationTrait -ceq 'NereidFascination' -and
        $Scenario -cin @('elemental-race-persistence-prepare', 'elemental-race-module-disabled-persistence',
            'elemental-race-module-restored-persistence', 'elemental-race-persistence-verify-absent')
}

function Test-KmgTreacherousEffectScope {
    param([string]$Scenario, [hashtable]$Parameters)
    return (Test-KmgNereidPersistenceScope $Scenario $Parameters) -and
        $Parameters.ContainsKey('qualificationEffect') -and $Parameters.qualificationEffect -is [string] -and
        $Parameters.qualificationEffect -ceq 'TreacherousEarth'
}

function Test-KmgElementalOffCreatorScope {
    param([string]$Scenario, [hashtable]$Parameters)
    return $Scenario -ceq 'disposable-elemental-character-creation-baseline' -and $Parameters.Count -eq 1 -and
        $Parameters.ContainsKey('creatorCase') -and $Parameters.creatorCase -is [string] -and $Parameters.creatorCase -ceq 'module-off'
}

function Test-KmgCompletionSceneScope {
    param([string]$Scenario, [hashtable]$Parameters)
    return (Test-KmgTreacherousEffectScope $Scenario $Parameters) -and
        $Scenario -cin @('elemental-race-module-disabled-persistence','elemental-race-module-restored-persistence') -and
        $Parameters.ContainsKey('qualificationOperation') -and $Parameters.qualificationOperation -is [string] -and
        $Parameters.qualificationOperation -ceq 'scene-roundtrip'
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
        [switch]$ManualInteractionRequired,
        [switch]$PermitQualifiedElementalRaces114,
        [switch]$PermitQualifiedElementalRaces117
    )
    $metadata = Get-KmgRuntimeScenarioMetadata -Scenario $Scenario
    $qualifiedElementalRaces114 =
        $PermitQualifiedElementalRaces114 -and
        $Scenario -ceq 'elemental-race-persistence-prepare' -and
        $ExpectedVersion -ceq '0.0.114'
    if ($PermitQualifiedElementalRaces114 -and
        -not $qualifiedElementalRaces114) {
        throw 'The qualified 0.0.114 preflight exception is limited to the Elemental Race legacy persistence producer.'
    }
    $qualifiedElementalRaces117 = $PermitQualifiedElementalRaces117 -and
        -not $PermitQualifiedElementalRaces114 -and
        $Scenario -ceq 'elemental-race-persistence-prepare' -and $ExpectedVersion -ceq '0.0.117'
    if ($PermitQualifiedElementalRaces117 -and (-not $qualifiedElementalRaces117 -or
        $Parameters.Count -ne 1 -or $Parameters.saveName -cne 'KMG_AUTOMATION_WORKING')) {
        throw 'Public 0.0.117 authority permits only its exact disposable persistence producer, without another producer authority.'
    }
    if ($ExpectedVersion -cne '0.0.122' -and
        -not $qualifiedElementalRaces114 -and -not $qualifiedElementalRaces117) {
        throw 'ExpectedVersion must be exactly the active version 0.0.122.'
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
        $creatorRegression = $Scenario -cin @('working-save-elemental-character-creation-regression', 'working-save-elemental-native-respec', 'working-save-elemental-nereid-creation', 'working-save-elemental-nereid-respec')
        $visualLifecycle = $Scenario -ceq 'working-save-creator-visual-lifecycle'
        $persistence = $Scenario -ceq 'disposable-teleportation-persistence'
        if ($persistence) {
            if ($Parameters.Count -ne 3 -or -not $Parameters.ContainsKey('phase') -or -not $Parameters.ContainsKey('planPath') -or
                $Parameters.phase -cnotin @('A', 'B', 'C', 'D') -or $Parameters.planPath -isnot [string] -or
                -not [IO.Path]::IsPathRooted($Parameters.planPath) -or -not (Test-Path -LiteralPath $Parameters.planPath -PathType Leaf)) {
                throw 'Persistence requires an exact phase and an existing guarded plan.'
            }
            $guardedPath = [IO.Path]::GetFullPath($Parameters.planPath)
            if (-not $guardedPath.StartsWith($script:KmgRuntimeEvidenceRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
                throw 'Persistence plan must be inside the guarded evidence root.'
            }
            for ($ancestor = Get-Item -LiteralPath $guardedPath; $null -ne $ancestor; $ancestor = if ($ancestor -is [IO.DirectoryInfo]) { $ancestor.Parent } else { $ancestor.Directory }) {
                if (($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'Persistence plan cannot cross a reparse point.' }
            }
            $plan = Get-Content -LiteralPath $Parameters.planPath -Raw | ConvertFrom-Json
            $txPattern = '[0-9]{8}T[0-9]{13}Z_[a-f0-9]{32}'
            if ($plan.transactionId -cnotmatch ('^' + $txPattern + '$') -or $plan.phase -cne $Parameters.phase -or
                $plan.schemaVersion -ne 1 -or $plan.version -cne $ExpectedVersion -or
                $plan.input.name -cne $Parameters.saveName -or
                $plan.input.sha256 -cne (Get-FileHash -LiteralPath $plan.input.path -Algorithm SHA256).Hash.ToLowerInvariant()) {
                throw 'Persistence input identity or hash differs from its exact plan.'
            }
            $priorPhase = @{ B = 'A'; C = 'B'; D = 'C' }
            $allowedName = if ($Parameters.phase -ceq 'A') { 'KMG_AUTOMATION_WORKING' } else {
                'KMG_TELEPORT_PERSISTENCE_' + $plan.transactionId + '_' + $priorPhase[$Parameters.phase]
            }
            if ($Parameters.saveName -cne $allowedName) { throw 'Persistence input is not owned by this phase transaction.' }
        }
        $requiredParameterCount = if ($persistence) { 3 } elseif ($Scenario -ceq 'working-save-elemental-nereid-respec') { 5 } elseif ($creatorRegression -or $visualLifecycle -or (Test-KmgCompletionSceneScope $Scenario $Parameters)) { 4 } elseif (Test-KmgTreacherousEffectScope $Scenario $Parameters) { 3 } elseif ($Scenario -ceq 'working-save-elemental-deferred-markers' -or (Test-KmgNereidPersistenceScope $Scenario $Parameters)) { 2 } else { 1 }
        if ($Parameters.Count -ne $requiredParameterCount -or
            -not $Parameters.ContainsKey('saveName') -or
            $Parameters.saveName -isnot [string] -or
            (-not $persistence -and $Parameters.saveName -cne $metadata.PermittedSaveName)) {
            throw "$Scenario requires its exact working save and allowlisted parameters."
        }
        if ($Scenario -ceq 'working-save-elemental-deferred-markers' -and
            (-not $Parameters.ContainsKey('fixtureCase') -or $Parameters.fixtureCase -isnot [string] -or
             $Parameters.fixtureCase -cnotin @('public117','deferred117'))) {
            throw 'The deferred-marker probe requires the exact public117 or deferred117 fixture case.'
        }
        if (($creatorRegression -or $visualLifecycle) -and (-not $Parameters.ContainsKey('race') -or
            -not $Parameters.ContainsKey('class') -or -not $Parameters.ContainsKey('allocation') -or
            $Parameters.race -isnot [string] -or $Parameters.class -isnot [string] -or
            $Parameters.allocation -isnot [string] -or
            $Parameters.race -cnotin @('Ifrit', 'Oread', 'Sylph', 'Undine') -or
            $Parameters.class -cnotin @('Fighter', 'Gunslinger') -or
            $Parameters.allocation -cnotin @('point-buy', 'roll'))) {
            throw 'The working creator regression requires exact allowlisted race, class, and allocation parameters.'
        }
        if ($Scenario -cin @('working-save-elemental-native-respec', 'working-save-elemental-nereid-respec') -and
            ($Parameters.class -cne 'Fighter' -or $Parameters.allocation -cne 'point-buy')) {
            throw 'The native respec fixture currently requires Fighter and point-buy.'
        }
        if ($Scenario -cin @('working-save-elemental-nereid-creation', 'working-save-elemental-nereid-respec') -and
            ($Parameters.race -cne 'Undine' -or $Parameters.class -cne 'Fighter' -or $Parameters.allocation -cne 'point-buy')) {
            throw 'Guarded Nereid player qualification requires Undine, Fighter and point-buy.'
        }
        if ($Scenario -ceq 'working-save-elemental-nereid-respec' -and
            (-not $Parameters.ContainsKey('sex') -or $Parameters.sex -isnot [string] -or
             $Parameters.sex -cnotin @('Male','Female'))) {
            throw 'Guarded Nereid respec requires one exact sex per bounded process.'
        }
    }
    elseif ($Scenario -cin @('disposable-elemental-nereid-creation','disposable-elemental-nereid-respec')) {
        $respec = $Scenario -ceq 'disposable-elemental-nereid-respec'
        $count = if ($respec) { 4 } else { 3 }
        if ($Parameters.Count -ne $count -or $Parameters.race -isnot [string] -or
            $Parameters.class -isnot [string] -or $Parameters.allocation -isnot [string] -or
            $Parameters.race -cne 'Undine' -or $Parameters.class -cne 'Fighter' -or
            $Parameters.allocation -cne 'point-buy' -or
            ($respec -and (-not $Parameters.ContainsKey('sex') -or $Parameters.sex -isnot [string] -or
                $Parameters.sex -cnotin @('Male','Female')))) {
            throw 'Native Nereid profile qualification requires exact save-free Undine/Fighter/point-buy parameters and bounded respec sex.'
        }
    }
    elseif ($Scenario -ceq 'disposable-elemental-character-creation-case') {
        if ($Parameters.Count -ne 3 -or -not $Parameters.ContainsKey('race') -or
            -not $Parameters.ContainsKey('class') -or -not $Parameters.ContainsKey('allocation') -or
            $Parameters.race -isnot [string] -or $Parameters.class -isnot [string] -or
            $Parameters.allocation -isnot [string] -or
            $Parameters.race -cnotin @('Ifrit', 'Oread', 'Sylph', 'Undine') -or
            $Parameters.class -cnotin @('Fighter', 'Gunslinger') -or
            $Parameters.allocation -cnotin @('point-buy', 'roll')) {
            throw 'The disposable creator case requires exact allowlisted race, class, and allocation parameters.'
        }
    }
    elseif ($Scenario -ceq 'observe-optional-mod-compatibility') {
        $allowedProfiles = @(
            'gunslinger-only',
            'gunslinger-races-unleashed',
            'gunslinger-call-of-the-wild',
            'gunslinger-call-of-the-wild-races-unleashed',
            'gunslinger-tweak-or-treat',
            'gunslinger-arms-armor',
            'gunslinger-toggle-custom-soundpacks',
            'gunslinger-high-risk-combined',
            'gunslinger-high-risk-combined-favored-class',
            'gunslinger-all-loadable-local',
            'gunslinger-qualified-combined'
        )
        if ($Parameters.Count -ne 1 -or
            -not $Parameters.ContainsKey('profileId') -or
            $Parameters.profileId -isnot [string] -or
            $Parameters.profileId -cnotin $allowedProfiles) {
            throw "$Scenario requires exactly one committed runtime-capable profileId."
        }
    }
    elseif ($Scenario -ceq 'observe-feature-module-settings') {
        if ($Parameters.Count -ne 12 -or
            -not $Parameters.ContainsKey('gunslinger') -or
            $Parameters.gunslinger -isnot [bool] -or
            -not $Parameters.ContainsKey('acadamaeGraduate') -or
            $Parameters.acadamaeGraduate -isnot [bool] -or
            -not $Parameters.ContainsKey('shieldOther') -or
            $Parameters.shieldOther -isnot [bool] -or
            -not $Parameters.ContainsKey('expandedSummoning') -or
            $Parameters.expandedSummoning -isnot [bool] -or
            -not $Parameters.ContainsKey('elvenBranchedSpears') -or
            $Parameters.elvenBranchedSpears -isnot [bool] -or
            -not $Parameters.ContainsKey('easternWeapons') -or
            $Parameters.easternWeapons -isnot [bool] -or
            -not $Parameters.ContainsKey('brownFurTransmuter') -or
            $Parameters.brownFurTransmuter -isnot [bool] -or
            -not $Parameters.ContainsKey('urbanBarbarian') -or
            $Parameters.urbanBarbarian -isnot [bool] -or
            -not $Parameters.ContainsKey('bodyguardFeats') -or
            $Parameters.bodyguardFeats -isnot [bool] -or
            -not $Parameters.ContainsKey(
                'protectionFromAlignmentControlImmunity') -or
            $Parameters.protectionFromAlignmentControlImmunity -isnot [bool] -or
            -not $Parameters.ContainsKey('elementalRaces') -or
            $Parameters.elementalRaces -isnot [bool] -or
            -not $Parameters.ContainsKey('teleportationSpells') -or
            $Parameters.teleportationSpells -isnot [bool]) {
            throw "$Scenario requires exact Boolean gunslinger, acadamaeGraduate, shieldOther, expandedSummoning, elvenBranchedSpears, easternWeapons, brownFurTransmuter, urbanBarbarian, bodyguardFeats, protectionFromAlignmentControlImmunity, elementalRaces, and teleportationSpells parameters."
        }
    }
    elseif ($Scenario -ceq 'observe-kmg-compatibility-asset-attribution') {
        $allowedConfigurations = @('all-suppressed', 'firearms-only',
            'spears-only', 'eastern-only', 'all-enabled')
        if ($Parameters.Count -ne 1 -or
            -not $Parameters.ContainsKey('assetConfiguration') -or
            $Parameters.assetConfiguration -isnot [string] -or
            $Parameters.assetConfiguration -cnotin $allowedConfigurations) {
            throw "$Scenario requires exactly one allowlisted assetConfiguration."
        }
    }
    elseif ($Parameters.Count -ne 0 -and -not (Test-KmgElementalOffCreatorScope $Scenario $Parameters)) {
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
        [hashtable]$Parameters = @{},
        [switch]$PermitQualifiedElementalRaces114,
        [switch]$PermitQualifiedElementalRaces117
    )
    if ($PermitQualifiedElementalRaces117 -and -not $ExitAfterCompletion) {
        throw 'The pinned public 117 fixture producer requires automatic process exit.'
    }
    if (($Scenario -cin @('working-save-elemental-nereid-creation','working-save-elemental-nereid-respec','working-save-elemental-deferred-markers','disposable-elemental-nereid-creation','disposable-elemental-nereid-respec') -or
        (Test-KmgNereidPersistenceScope $Scenario $Parameters) -or
        (Test-KmgElementalOffCreatorScope $Scenario $Parameters)) -and -not $ExitAfterCompletion) {
        throw 'Guarded Nereid player qualification requires automatic process exit.'
    }
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
        -Parameters $Parameters `
        -PermitQualifiedElementalRaces114:$PermitQualifiedElementalRaces114 `
        -PermitQualifiedElementalRaces117:$PermitQualifiedElementalRaces117
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
        parameters = if ($Scenario -ceq 'working-save-elemental-nereid-respec') {
            [ordered]@{ saveName = [string]$Parameters.saveName; race = [string]$Parameters.race
                class = [string]$Parameters.class; allocation = [string]$Parameters.allocation; sex = [string]$Parameters.sex }
        } elseif ($Scenario -cin @('working-save-elemental-character-creation-regression', 'working-save-elemental-native-respec', 'working-save-elemental-nereid-creation', 'working-save-elemental-nereid-respec')) {
            [ordered]@{ saveName = [string]$Parameters.saveName; race = [string]$Parameters.race
                class = [string]$Parameters.class; allocation = [string]$Parameters.allocation }
        } elseif ($Scenario -ceq 'working-save-elemental-deferred-markers') {
            [ordered]@{ saveName = [string]$Parameters.saveName; fixtureCase = [string]$Parameters.fixtureCase }
        } elseif (Test-KmgNereidPersistenceScope $Scenario $Parameters) {
            $scopeArgs = [ordered]@{ saveName = [string]$Parameters.saveName; qualificationTrait = 'NereidFascination' }
            if (Test-KmgTreacherousEffectScope $Scenario $Parameters) { $scopeArgs.qualificationEffect = 'TreacherousEarth' }
            if (Test-KmgCompletionSceneScope $Scenario $Parameters) { $scopeArgs.qualificationOperation = 'scene-roundtrip' }
            $scopeArgs
        } elseif ($Scenario -ceq 'disposable-teleportation-persistence') {
            [ordered]@{ saveName = [string]$Parameters.saveName; phase = [string]$Parameters.phase; planPath = [string]$Parameters.planPath }
        } elseif ($metadata.RequiresSaveName) {
            [ordered]@{ saveName = [string]$Parameters.saveName }
        } elseif ($Scenario -cin @('disposable-elemental-nereid-creation','disposable-elemental-nereid-respec')) {
            $nativeArgs = [ordered]@{ race = [string]$Parameters.race; class = [string]$Parameters.class; allocation = [string]$Parameters.allocation }
            if ($Scenario -ceq 'disposable-elemental-nereid-respec') { $nativeArgs.sex = [string]$Parameters.sex }
            $nativeArgs
        } elseif ($Scenario -ceq 'disposable-elemental-character-creation-case') {
            [ordered]@{
                race = [string]$Parameters.race
                class = [string]$Parameters.class
                allocation = [string]$Parameters.allocation
            }
        } elseif ($Scenario -ceq 'observe-optional-mod-compatibility') {
            [ordered]@{ profileId = [string]$Parameters.profileId }
        } elseif ($Scenario -ceq 'observe-feature-module-settings') {
            [ordered]@{
                gunslinger = [bool]$Parameters.gunslinger
                acadamaeGraduate = [bool]$Parameters.acadamaeGraduate
                shieldOther = [bool]$Parameters.shieldOther
                expandedSummoning = [bool]$Parameters.expandedSummoning
                elvenBranchedSpears = [bool]$Parameters.elvenBranchedSpears
                easternWeapons = [bool]$Parameters.easternWeapons
                brownFurTransmuter = [bool]$Parameters.brownFurTransmuter
                urbanBarbarian = [bool]$Parameters.urbanBarbarian
                bodyguardFeats = [bool]$Parameters.bodyguardFeats
                protectionFromAlignmentControlImmunity =
                    [bool]$Parameters.protectionFromAlignmentControlImmunity
                elementalRaces = [bool]$Parameters.elementalRaces
                teleportationSpells = [bool]$Parameters.teleportationSpells
            }
        } elseif ($Scenario -ceq
            'observe-kmg-compatibility-asset-attribution') {
            [ordered]@{
                assetConfiguration = [string]$Parameters.assetConfiguration
            }
        } elseif (Test-KmgElementalOffCreatorScope $Scenario $Parameters) {
            [ordered]@{ creatorCase = 'module-off' }
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
