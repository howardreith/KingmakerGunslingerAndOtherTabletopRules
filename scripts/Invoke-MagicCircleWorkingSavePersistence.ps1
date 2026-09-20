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
$prepared = $null
if ($PreparedEvidencePath) {
    $resolvedPrepare = (Resolve-Path -LiteralPath $PreparedEvidencePath).Path
    $evidenceRoot = [IO.Path]::GetFullPath($script:KmgRuntimeEvidenceRoot).TrimEnd('\') + '\'
    if (-not $resolvedPrepare.StartsWith($evidenceRoot, [StringComparison]::OrdinalIgnoreCase) -or
        (Split-Path -Leaf $resolvedPrepare) -cne 'magic-circle-persistence.json') {
        throw 'Prepare evidence must be an exact persistence record in the guarded evidence root.'
    }
    $prepareResult = Get-Content -LiteralPath (Join-Path (Split-Path $resolvedPrepare) 'runtime-result.json') -Raw | ConvertFrom-Json
    if ($prepareResult.status -cne 'PASS' -or $prepareResult.scenario -cne 'working-save-magic-circle-prepare') {
        throw 'The referenced prepare run did not pass.'
    }
    $prepared = Get-Content -LiteralPath $resolvedPrepare -Raw | ConvertFrom-Json
}
if (-not $PSCmdlet.ShouldProcess('KMG_AUTOMATION_WORKING', "run guarded Magic Circle $Phase with content=$ContentEnabled/control=$ControlEnhancementEnabled")) { return }
[void](Assert-KmgReusableDeployment -DeploymentManifestPath $DeploymentManifestPath `
    -PackagePath $PackagePath -RepositoryRoot $root -ExpectedVersion $version -AllowDirtyGit:$AllowDirtyGit)
$original = [IO.File]::ReadAllBytes($settings)
$configuration = [Text.Encoding]::UTF8.GetString($original).TrimStart([char]0xFEFF) | ConvertFrom-Json
if ($configuration.schemaVersion -ne 12 -or
    -not ($configuration.PSObject.Properties.Name -contains 'magic-circle-spells') -or
    -not ($configuration.PSObject.Properties.Name -contains 'protection-from-alignment-control-immunity')) {
    throw 'Current schema-12 settings with both existing setting keys are required.'
}
$configuration.'magic-circle-spells' = $ContentEnabled
$configuration.'protection-from-alignment-control-immunity' = $ControlEnhancementEnabled
$scenario = 'working-save-magic-circle-' + $Phase
$before = @(Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory | Where-Object Name -Like "*-$scenario" | ForEach-Object FullName)
$backupDirectory = Join-Path $root 'artifacts/magic-circle/settings'
[void](New-Item -ItemType Directory -Path $backupDirectory -Force)
$settingsBackup = Join-Path $backupDirectory ($Phase + '-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '.json')
[IO.File]::WriteAllBytes($settingsBackup, $original)
$failure = $null
$resultDirectory = $null
try {
    [IO.File]::WriteAllText($settings, ($configuration | ConvertTo-Json -Depth 16), (New-Object Text.UTF8Encoding($false)))
    & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario $scenario `
        -ExpectedVersion $version -SaveName KMG_AUTOMATION_WORKING -TimeoutSeconds 240 `
        -ObserverStartupTimeoutSeconds 600 -CompletionTimeoutSeconds 240 `
        -ExitAfterCompletion:$true -AllowDirtyGit:$AllowDirtyGit -ReuseInstalledArtifact `
        -PackagePath $PackagePath -DeploymentManifestPath $DeploymentManifestPath -Confirm:$false
    if ($LASTEXITCODE -ne 0) { throw "Guarded $scenario failed." }
    $directories = @(Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory |
        Where-Object { $_.Name -like "*-$scenario" -and $_.FullName -notin $before })
    if ($directories.Count -ne 1) { throw 'Ambiguous guarded evidence directory.' }
    $resultDirectory = $directories[0].FullName
    $result = Get-Content -LiteralPath (Join-Path $resultDirectory 'runtime-result.json') -Raw | ConvertFrom-Json
    if ($result.status -cne 'PASS' -or $result.scenario -cne $scenario -or $result.loadedModVersion -cne $version) {
        throw 'Native phase result or loaded version did not qualify.'
    }
    $record = Get-Content -LiteralPath (Join-Path $resultDirectory 'magic-circle-persistence.json') -Raw | ConvertFrom-Json
    if ($Phase -ne 'absent' -and $Phase -ne 'scene' -and
        ($record.settings.magicCircleSpells -ne $ContentEnabled -or $record.settings.sharedControlEnhancement -ne $ControlEnhancementEnabled)) {
        throw 'Observed startup settings do not match the requested profile.'
    }
    if ($prepared) {
        foreach ($key in @('area', 'actors', 'carriers', 'control')) {
            $expected = $prepared.snapshot.$key | ConvertTo-Json -Depth 24 -Compress
            $actual = $record.snapshot.$key | ConvertTo-Json -Depth 24 -Compress
            if ($actual -cne $expected) { throw "Fresh native load changed original persisted $key." }
        }
        [ordered]@{ status = 'PASS'; prepareEvidence = $resolvedPrepare; observedEvidence = $resultDirectory
            exactComparedFields = @('area', 'actors', 'carriers', 'control')
            content = $ContentEnabled; sharedControl = $ControlEnhancementEnabled
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $resultDirectory 'magic-circle-cross-launch-comparison.json') -Encoding UTF8
    }
}
catch { $failure = $_ }
finally {
    $exitDeadline = [DateTime]::UtcNow.AddSeconds(45)
    while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and [DateTime]::UtcNow -lt $exitDeadline) {
        Start-Sleep -Milliseconds 250
    }
    if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
        throw "Game is still running; settings restoration is pending from $settingsBackup. Do not swap builds or kill it."
    }
    [IO.File]::WriteAllBytes($settings, $original)
    if ([Convert]::ToBase64String([IO.File]::ReadAllBytes($settings)) -cne [Convert]::ToBase64String($original)) {
        throw 'Original feature settings bytes were not restored.'
    }
}
if ($failure) { throw $failure }
Write-Output "PASS Magic Circle $Phase; evidence=$resultDirectory; original settings restored exactly."
