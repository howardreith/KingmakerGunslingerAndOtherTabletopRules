[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('prepare', 'verify', 'scene', 'cleanup', 'absent')][string]$Phase,
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [Parameter(Mandatory = $true)][string]$DeploymentManifestPath,
    [string]$PreparedEvidencePath,
    [bool]$ContentEnabled = $true,
    [bool]$ControlEnhancementEnabled = $true,
    [switch]$AllowDirtyGit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$version = (Get-Content -LiteralPath (Join-Path $root 'Info.json') -Raw | ConvertFrom-Json).Version
$settings = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger\FeatureModules.json'
if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) { throw 'Kingmaker must have exited before this phase.' }
if (-not (Test-Path -LiteralPath $settings -PathType Leaf)) { throw 'Existing qualified feature settings are required.' }
if ($Phase -eq 'prepare' -and -not $ContentEnabled) { throw 'Prepare requires Magic Circle content enabled.' }
if ($Phase -notin @('prepare', 'absent') -and [string]::IsNullOrWhiteSpace($PreparedEvidencePath)) {
    throw 'Verification requires the exact successful prepare evidence path.'
}
. (Join-Path $PSScriptRoot 'MagicCircleSettingsTransaction.Common.ps1')
if (-not $PSCmdlet.ShouldProcess('KMG_AUTOMATION_WORKING', "run guarded Magic Circle $Phase with content=$ContentEnabled/control=$ControlEnhancementEnabled")) { return }
$scenario = 'working-save-magic-circle-' + $Phase
$state = @{ Reuse = $null; Binding = $null; Prepared = $null; PreparedPath = $null; Evidence = $null }
$verify = {
    $state.Reuse = Assert-KmgReusableDeployment -DeploymentManifestPath $DeploymentManifestPath `
        -PackagePath $PackagePath -RepositoryRoot $root -ExpectedVersion $version -AllowDirtyGit:$AllowDirtyGit
    if ($PreparedEvidencePath -and $null -eq $state.Binding) {
        $state.Binding = New-KmgMagicCirclePreparationBinding $PreparedEvidencePath $version $state.Reuse.Build
        $state.Prepared = Read-KmgMagicCirclePreparationBinding $state.Binding $version
        $state.PreparedPath = (Resolve-Path -LiteralPath $PreparedEvidencePath).Path
    }
    foreach ($entry in @(
        @{ path=(Join-Path $state.Reuse.Deployment.liveModDirectory 'KingmakerGunslinger.dll'); sha256=$state.Reuse.Build.dllSha256 },
        @{ path=$state.Reuse.PackagePath; sha256=$state.Reuse.Build.packageSha256 },
        @{ path=$state.Reuse.DeploymentManifestPath; sha256=(Get-KmgCompatibilitySha256 $state.Reuse.DeploymentManifestPath) })) {
        [pscustomobject]$entry
    }
}
$run = {
    param($lease)
    $prepared = $state.Prepared; $resolvedPrepare = $state.PreparedPath
    $before = @(Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory | Where-Object Name -Like "*-$scenario" | ForEach-Object FullName)
    $parameters = @{}
    if ($Phase -in @('verify','scene','cleanup')) { $parameters.preparationBinding = $state.Binding }
    & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario $scenario `
        -ExpectedVersion $version -SaveName KMG_AUTOMATION_WORKING -Parameters $parameters -TimeoutSeconds 240 `
        -ObserverStartupTimeoutSeconds 600 -CompletionTimeoutSeconds 240 `
        -ExitAfterCompletion:$true -AllowDirtyGit:$AllowDirtyGit -ReuseInstalledArtifact `
        -PackagePath $PackagePath -DeploymentManifestPath $DeploymentManifestPath -RuntimeLease $lease -Confirm:$false
    if ($LASTEXITCODE -ne 0) { throw "Guarded $scenario failed." }
    $directories = @(Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory |
        Where-Object { $_.Name -like "*-$scenario" -and $_.FullName -notin $before })
    if ($directories.Count -ne 1) { throw 'Ambiguous guarded evidence directory.' }
    $resultDirectory = $directories[0].FullName
    $state.Evidence = $resultDirectory
    $result = Get-Content -LiteralPath (Join-Path $resultDirectory 'runtime-result.json') -Raw | ConvertFrom-Json
    if ($result.status -cne 'PASS' -or $result.scenario -cne $scenario -or $result.loadedModVersion -cne $version) {
        throw 'Native phase result or loaded version did not qualify.'
    }
    $record = Get-Content -LiteralPath (Join-Path $resultDirectory 'magic-circle-persistence.json') -Raw | ConvertFrom-Json
    if ($Phase -ne 'absent' -and $Phase -ne 'scene' -and
        ($record.settings.magicCircleSpells -ne $ContentEnabled -or $record.settings.sharedControlEnhancement -ne $ControlEnhancementEnabled)) {
        throw 'Observed startup settings do not match the requested profile.'
    }
    if ($prepared -and $Phase -ne 'absent') {
        foreach ($key in @('area', 'actors', 'carriers', 'control', 'market')) {
            $expected = $prepared.snapshot.$key | ConvertTo-Json -Depth 24 -Compress
            $actual = $record.snapshot.$key | ConvertTo-Json -Depth 24 -Compress
            if ($actual -cne $expected) { throw "Fresh native load changed original persisted $key." }
        }
        [ordered]@{ status = 'PASS'; prepareEvidence = $resolvedPrepare; observedEvidence = $resultDirectory
            exactComparedFields = @('area', 'actors', 'carriers', 'control', 'market')
            content = $ContentEnabled; sharedControl = $ControlEnhancementEnabled
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $resultDirectory 'magic-circle-cross-launch-comparison.json') -Encoding UTF8
    }
    if ($prepared -and $Phase -eq 'absent') {
        $originalMarket = $prepared.snapshot.market
        $absence = $record.marketAbsence
        if (($absence.inventory | ConvertTo-Json -Compress) -cne ($originalMarket.originalInventory | ConvertTo-Json -Compress) -or
            $absence.gold -ne $originalMarket.originalGold -or $originalMarket.table -in $absence.cachedTables -or
            $originalMarket.table -in $absence.savedTables -or $originalMarket.table -in $absence.grants) {
            throw 'Fresh absence did not restore exact pre-fixture inventory, gold, supplier and grant state.'
        }
        [ordered]@{ status = 'PASS'; prepareEvidence = $resolvedPrepare; observedEvidence = $resultDirectory
            exactComparedFields = @('originalInventory', 'originalGold', 'ownedTableAbsent', 'ownedGrantAbsent')
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $resultDirectory 'magic-circle-cross-launch-comparison.json') -Encoding UTF8
    }
}
Invoke-KmgMagicCircleSettingsTransaction -SettingsPath $settings -VerifyDeployment $verify -Run $run `
    -ContentEnabled $ContentEnabled -ControlEnhancementEnabled $ControlEnhancementEnabled -Confirm:$false
Write-Output "PASS Magic Circle $Phase; evidence=$($state.Evidence); original settings restored exactly."
