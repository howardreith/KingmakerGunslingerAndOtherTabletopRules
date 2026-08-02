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

Require ($proficiency.Contains('AttachReload') -and $proficiency.Contains('grant.Facts.Length != 1')) 'proficiency-reload-only'
Require ($gunsmithing.Contains('grant.Facts = new BlueprintUnitFact[] { overhaulAbility, repairAbility }')) 'gunsmithing-exact-maintenance-grants'
Require ($gunsmithing.Contains('feature.HideInUI = false') -and $gunsmithing.Contains('feature.IsClassFeature = true')) 'visible-class-feature'
Require ($class.Contains('{ proficiencies, gunsmithing, grit, deadeye, dodge, quickClear }')) 'level-one-placement'
Require ($bootstrap.Contains('ExpectedRegisteredBlueprintCount = 127') -and $bootstrap.IndexOf('AttachReload') -lt $bootstrap.IndexOf('GunsmithingBlueprints.Register')) 'bootstrap-count-and-order'
$entry = @($manifest.entries | Where-Object symbol -eq 'KMG.Classes.Gunsmithing')
Require ($entry.Count -eq 1 -and $entry[0].plannedType -eq 'BlueprintFeature' -and $entry[0].status -eq 'active') 'manifest-identity'
Require (@($manifest.entries).Count -eq 127) 'ledger-count'
Require (@($manifest.entries | Where-Object status -eq 'active').Count -eq 127) 'active-count'
'Sprint 84 Gunsmithing source contract passed.'
