[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
function Require([bool]$Condition, [string]$Label) {
    if (-not $Condition) { throw "Sprint 84 contract failed: $Label" }
}

$proficiency = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Blueprints\FirearmProficiencyBlueprints.cs') -Raw
$gunsmithing = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Blueprints\GunsmithingBlueprints.cs') -Raw
$class = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Blueprints\GunslingerClassBlueprints.cs') -Raw
$bootstrap = Get-Content (Join-Path $root 'src\KingmakerGunslinger\Bootstrap\BlueprintBootstrap.cs') -Raw
$manifest = Get-Content (Join-Path $root 'blueprints\blueprints.json') -Raw | ConvertFrom-Json

Require ($proficiency.Contains('AttachReload') -and
    $proficiency.Contains('grant.Facts.Length != 3') -and
    $proficiency.Contains('!ReferenceEquals(grant.Facts[0], reloadAbility)') -and
    $proficiency.Contains('!ReferenceEquals(grant.Facts[1], scatterShotAbility)') -and
    $proficiency.Contains('!ReferenceEquals(grant.Facts[2], paperCartridgeMode)')) 'proficiency-current-action-grant'
Require ($gunsmithing.Contains('grant.Facts = new BlueprintUnitFact[]') -and
    $gunsmithing.Contains('{ overhaulAbility, repairAbility, craftingAbility,') -and
    $gunsmithing.Contains('paperCraftingAbility }')) 'gunsmithing-maintenance-and-ammunition-grants'
Require ($gunsmithing.Contains('feature.HideInUI = false') -and $gunsmithing.Contains('feature.IsClassFeature = true')) 'visible-class-feature'
Require ($class.Contains('{ proficiencies, gunsmithing, grit, deadeye, dodge, quickClear }')) 'level-one-placement'
Require ($bootstrap.Contains('GunsmithingCraftingBlueprints.Register') -and
    $bootstrap.IndexOf('GunsmithingCraftingBlueprints.Register') -lt $bootstrap.IndexOf('FirearmProficiencyBlueprints.AttachReload') -and
    $bootstrap.IndexOf('FirearmProficiencyBlueprints.AttachReload') -lt $bootstrap.IndexOf('GunsmithingBlueprints.Register')) 'bootstrap-current-order'
$entry = @($manifest.entries | Where-Object symbol -eq 'KMG.Classes.Gunsmithing')
Require ($entry.Count -eq 1 -and $entry[0].plannedType -eq 'BlueprintFeature' -and $entry[0].status -eq 'active') 'manifest-identity'
$craftBasic = @($manifest.entries | Where-Object symbol -eq 'KMG.Gunsmithing.CraftBasicAmmunition')
$craftPaper = @($manifest.entries | Where-Object symbol -eq 'KMG.Gunsmithing.CraftPaperCartridges')
Require ($craftBasic.Count -eq 1 -and $craftBasic[0].status -eq 'active') 'basic-ammunition-manifest-identity'
Require ($craftPaper.Count -eq 1 -and $craftPaper[0].status -eq 'active') 'paper-cartridge-manifest-identity'
'Sprint 84 Gunsmithing source contract passed.'
