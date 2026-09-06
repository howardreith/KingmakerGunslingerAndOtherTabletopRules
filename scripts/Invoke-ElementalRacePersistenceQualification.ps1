[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
# Three guarded fresh-process phases persist race, heritage, Release B feat,
# and the seven-trait blood/Insight plus Efreeti Magic matrix; verify module-OFF,
# restore the module, respec, and clean up the
# fixtures. Run the fresh-load absence phase separately after this restores the
# caller's original settings bytes.
param(
    [string]$ExpectedVersion = '0.0.117',
    [ValidateSet('KMG_AUTOMATION_WORKING')]
    [string]$SaveName = 'KMG_AUTOMATION_WORKING',
    [ValidateRange(120, 1800)][int]$TimeoutSeconds = 900,
    [switch]$AllowDirtyGit,
    [Parameter(Mandatory = $true)][string]$DeploymentManifestPath,
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [switch]$ConfirmEach
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$invoke = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$settings = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger\FeatureModules.json'
$expectedParent = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger'
if ([IO.Path]::GetFullPath((Split-Path -Parent $settings)).TrimEnd('\') -cne $expectedParent) {
    throw 'Feature-module settings target changed unexpectedly.'
}
if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
    throw 'Pathfinder: Kingmaker must not be running before elemental persistence qualification.'
}
foreach ($path in @($DeploymentManifestPath, $PackagePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Pinned Elemental Races persistence artifact is missing: $path"
    }
}
if (-not $PSCmdlet.ShouldProcess($SaveName,
        'run the authorized three-launch 24-fixture Elemental Races heritage and feat persistence sequence')) {
    return
}

$ConfirmPreference = 'None'
$originalExists = Test-Path -LiteralPath $settings -PathType Leaf
$originalBytes = if ($originalExists) {
    [IO.File]::ReadAllBytes($settings)
} else {
    $null
}
$failure = $null
$evidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
$evidenceDirectoriesBefore = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory |
    ForEach-Object FullName)

function Set-ElementalRacesEnabled([bool]$enabled) {
    $configuration = [ordered]@{
        schemaVersion = 10
        gunslinger = $true
        'acadamae-graduate' = $true
        'shield-other' = $true
        'expanded-summoning' = $true
        'elven-branched-spears' = $true
        'eastern-weapons' = $true
        'brown-fur-transmuter' = $true
        'urban-barbarian' = $true
        'bodyguard-feats' = $true
        'protection-from-alignment-control-immunity' = $true
        'elemental-races' = $enabled
    }
    $temporary = $settings + '.kmg-elemental-race-persistence.tmp'
    $json = $configuration | ConvertTo-Json -Depth 4
    [IO.File]::WriteAllText($temporary, $json,
        (New-Object Text.UTF8Encoding($false)))
    Move-Item -LiteralPath $temporary -Destination $settings -Force
}

function Restore-OriginalFeatureState {
    if ($originalExists) {
        $temporary = $settings +
            '.kmg-elemental-race-persistence-restore.tmp'
        [IO.File]::WriteAllBytes($temporary, $originalBytes)
        Move-Item -LiteralPath $temporary -Destination $settings -Force
    }
    elseif (Test-Path -LiteralPath $settings) {
        Remove-Item -LiteralPath $settings -Force
    }
}

function Wait-ForGuardedKingmakerExit([string]$phase) {
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and
        [DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 250
    }
    if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
        throw "Kingmaker did not exit within 45 seconds after elemental persistence $phase."
    }
}

function Preserve-PhaseNativeLog([string]$scenario) {
    $created = @(Get-ChildItem -LiteralPath $evidenceRoot -Directory |
        Where-Object { $_.Name.EndsWith('-' + $scenario,
            [StringComparison]::Ordinal) -and
            $evidenceDirectoriesBefore -notcontains $_.FullName })
    if ($created.Count -ne 1) {
        throw "Expected exactly one new persistence evidence directory for $scenario."
    }
    $resultPath = Join-Path $created[0].FullName 'runtime-result.json'
    $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
    if ($result.scenario -cne $scenario -or $result.status -cne 'PASS' -or
        $result.loadedModVersion -cne $ExpectedVersion -or
        @($result.assertions | Where-Object { $_.status -cne 'PASS' }).Count -ne 0) {
        throw "Persistence phase is not an exact structured PASS: $scenario"
    }
    # Native hydration errors must survive the next launch's log rotation.
    & (Join-Path $PSScriptRoot 'compatibility\Collect-KmgCompatibilityAttributionLog.ps1') `
        -EvidenceDirectory $created[0].FullName `
        -ConfigurationId ('elemental-traits-' + $scenario) | Out-Null
}

try {
    Set-ElementalRacesEnabled $true
    & $invoke -Scenario 'elemental-race-persistence-prepare' `
        -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
        -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
        -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach `
        -ReuseInstalledArtifact `
        -DeploymentManifestPath $DeploymentManifestPath `
        -PackagePath $PackagePath
    if ($LASTEXITCODE -ne 0) {
        throw 'Elemental Races persistence prepare failed.'
    }
    Wait-ForGuardedKingmakerExit 'prepare'
    Preserve-PhaseNativeLog 'elemental-race-persistence-prepare'

    Set-ElementalRacesEnabled $false
    & $invoke -Scenario 'elemental-race-module-disabled-persistence' `
        -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
        -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
        -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach `
        -ReuseInstalledArtifact `
        -DeploymentManifestPath $DeploymentManifestPath `
        -PackagePath $PackagePath
    if ($LASTEXITCODE -ne 0) {
        throw 'Elemental Races module-disabled persistence verification failed.'
    }
    Wait-ForGuardedKingmakerExit 'module-disabled-verify-preserve'
    Preserve-PhaseNativeLog 'elemental-race-module-disabled-persistence'

    Set-ElementalRacesEnabled $true
    & $invoke -Scenario 'elemental-race-module-restored-persistence' `
        -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
        -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
        -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach `
        -ReuseInstalledArtifact `
        -DeploymentManifestPath $DeploymentManifestPath `
        -PackagePath $PackagePath
    if ($LASTEXITCODE -ne 0) {
        throw 'Elemental Races module-restored Respec/cleanup failed.'
    }
    Wait-ForGuardedKingmakerExit 'module-restored-respec-cleanup'
    Preserve-PhaseNativeLog 'elemental-race-module-restored-persistence'
}
catch { $failure = $_ }
finally {
    # A terminal FAIL result can precede Application.Quit completing. Never
    # restore settings while that guarded process is still running.
    Wait-ForGuardedKingmakerExit 'settings restoration'
    Restore-OriginalFeatureState
    $restoredExists = Test-Path -LiteralPath $settings -PathType Leaf
    if ($restoredExists -ne $originalExists) {
        throw 'Feature settings existence was not restored exactly.'
    }
    if ($originalExists) {
        $restored = [IO.File]::ReadAllBytes($settings)
        if ([Convert]::ToBase64String($restored) -cne
            [Convert]::ToBase64String($originalBytes)) {
            throw 'Feature settings bytes were not restored exactly.'
        }
    }
}
if ($failure -ne $null) { throw $failure }
Write-Host "Elemental Races three-launch heritage and feat persistence and cleanup PASS; run elemental-race-persistence-verify-absent next; package=$PackagePath; deployment=$DeploymentManifestPath"
