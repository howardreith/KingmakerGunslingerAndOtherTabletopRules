[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.115',
    [ValidateSet('KMG_AUTOMATION_WORKING')]
    [string]$SaveName = 'KMG_AUTOMATION_WORKING',
    [ValidateRange(120, 1800)][int]$TimeoutSeconds = 900,
    [Parameter(Mandatory = $true)][string]$CurrentDeploymentManifestPath,
    [Parameter(Mandatory = $true)][string]$CurrentPackagePath,
    [string]$LegacyPackagePath,
    [switch]$AllowDirtyGit,
    [switch]$ConfirmEach
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$invoke = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$deployCurrent = Join-Path $PSScriptRoot 'Deploy-Local.ps1'
$deployLegacy = Join-Path $PSScriptRoot `
    'Deploy-QualifiedElementalRaces114.ps1'
$live = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger'
$settings = Join-Path $live 'FeatureModules.json'
$expectedEvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
$legacyExpected = Join-Path $root `
    'artifacts\release\0.0.114\KingmakerGunslinger-0.0.114-elemental-races.zip'
if ([string]::IsNullOrWhiteSpace($LegacyPackagePath)) {
    $LegacyPackagePath = $legacyExpected
}

if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
    throw 'Pathfinder: Kingmaker must not be running before the legacy migration transaction.'
}
foreach ($path in @($CurrentDeploymentManifestPath, $CurrentPackagePath,
        $LegacyPackagePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Legacy migration qualification input is missing: $path"
    }
}
$current = Assert-KmgReusableDeployment `
    -DeploymentManifestPath $CurrentDeploymentManifestPath `
    -PackagePath $CurrentPackagePath -RepositoryRoot $root `
    -ExpectedVersion $ExpectedVersion -AllowDirtyGit:$AllowDirtyGit
$legacy = (Resolve-Path -LiteralPath $LegacyPackagePath).Path
$legacyExact = [IO.Path]::GetFullPath($legacyExpected)
if (-not $legacy.Equals($legacyExact,
        [StringComparison]::OrdinalIgnoreCase) -or
    (Get-KmgSha256 -Path $legacy) -cne
        'b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694') {
    throw 'Legacy migration qualification requires the exact pinned 0.0.114 release package.'
}
if (-not [IO.Path]::GetFullPath($live).Equals(
        [IO.Path]::GetFullPath($current.Deployment.liveModDirectory),
        [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Current deployment does not target the exact live KingmakerGunslinger directory.'
}
if (-not $PSCmdlet.ShouldProcess($SaveName,
        'run the guarded 0.0.114-to-0.0.115 Elemental Race migration and cleanup transaction')) {
    Write-Host 'Source-only/WhatIf validation passed. No deployment, settings, save, or process change occurred.'
    return
}

$ConfirmPreference = 'None'
$WhatIfPreference = $false
$originalSettingsExisted = Test-Path -LiteralPath $settings -PathType Leaf
$originalSettingsBytes = if ($originalSettingsExisted) {
    [IO.File]::ReadAllBytes($settings)
} else { $null }
$originalSettingsSha = if ($originalSettingsExisted) {
    Get-KmgSha256 -Path $settings
} else { '<absent>' }
$transactionId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') +
    '-elemental-race-legacy-migration-transaction'
$transactionDirectory = Join-Path $expectedEvidenceRoot `
    ('transactions\' + $transactionId)
New-Item -ItemType Directory -Path $transactionDirectory -Force | Out-Null
$phases = [Collections.Generic.List[object]]::new()
$legacyDeploymentManifestPath = $null
$migrationDeploymentManifestPath = $null
$restoredDeploymentManifestPath = $null
$failure = $null
$restorationFailure = $null

function Set-ElementalRacesEnabled {
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
        'elemental-races' = $true
    }
    $temporary = $settings + '.kmg-elemental-legacy-enable.tmp'
    [IO.File]::WriteAllText($temporary,
        ($configuration | ConvertTo-Json -Depth 4),
        (New-Object Text.UTF8Encoding($false)))
    Move-Item -LiteralPath $temporary -Destination $settings -Force
}

function Restore-OriginalFeatureState {
    if ($originalSettingsExisted) {
        $temporary = $settings + '.kmg-elemental-legacy-restore.tmp'
        [IO.File]::WriteAllBytes($temporary, $originalSettingsBytes)
        Move-Item -LiteralPath $temporary -Destination $settings -Force
    }
    elseif (Test-Path -LiteralPath $settings -PathType Leaf) {
        Remove-Item -LiteralPath $settings -Force
    }
}

function Wait-ForGuardedKingmakerExit([string]$Phase) {
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and
        [DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 250
    }
    if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
        throw "Kingmaker did not exit within 45 seconds after legacy migration $Phase."
    }
}

function Invoke-QualifiedPhase {
    param(
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$Version,
        [Parameter(Mandatory = $true)][string]$DeploymentManifest,
        [Parameter(Mandatory = $true)][string]$Package,
        [switch]$UseQualifiedLegacy
    )
    $before = @(Get-ChildItem -LiteralPath $expectedEvidenceRoot -Directory |
        Where-Object { $_.Name.EndsWith('-' + $Scenario,
            [StringComparison]::Ordinal) } |
        ForEach-Object FullName)
    $arguments = @{
        Scenario = $Scenario
        ExpectedVersion = $Version
        SaveName = $SaveName
        TimeoutSeconds = $TimeoutSeconds
        ExitAfterCompletion = $true
        AllowDirtyGit = [bool]$AllowDirtyGit
        Confirm = [bool]$ConfirmEach
        DeploymentManifestPath = $DeploymentManifest
        PackagePath = $Package
    }
    if ($UseQualifiedLegacy) {
        $arguments.ReuseQualifiedElementalRaces114Release = $true
    }
    else {
        $arguments.ReuseInstalledArtifact = $true
    }
    & $invoke @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Guarded legacy migration phase failed: $Scenario"
    }
    Wait-ForGuardedKingmakerExit $Scenario
    $created = @(Get-ChildItem -LiteralPath $expectedEvidenceRoot -Directory |
        Where-Object {
            $_.Name.EndsWith('-' + $Scenario,
                [StringComparison]::Ordinal) -and
            $before -notcontains $_.FullName
        })
    if ($created.Count -ne 1) {
        throw "Expected exactly one new evidence directory for $Scenario; observed $($created.Count)."
    }
    $resultPath = Join-Path $created[0].FullName 'runtime-result.json'
    $evidencePath = Join-Path $created[0].FullName 'runtime-evidence.json'
    if (-not (Test-Path -LiteralPath $resultPath -PathType Leaf) -or
        -not (Test-Path -LiteralPath $evidencePath -PathType Leaf)) {
        throw "Guarded phase evidence is incomplete: $Scenario"
    }
    $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
    $failedAssertions = @($result.assertions | Where-Object {
        $_.status -cne 'PASS'
    })
    if ($result.scenario -cne $Scenario -or $result.status -cne 'PASS' -or
        $result.loadedModVersion -cne $Version -or
        $failedAssertions.Count -ne 0) {
        throw "Guarded phase result is not an exact PASS: $Scenario"
    }
    $record = [ordered]@{
        scenario = $Scenario
        version = $Version
        runId = $result.runId
        status = $result.status
        assertionCount = @($result.assertions).Count
        resultPath = $resultPath
        resultSha256 = Get-KmgSha256 -Path $resultPath
        evidenceManifestPath = $evidencePath
        evidenceManifestSha256 = Get-KmgSha256 -Path $evidencePath
    }
    $phases.Add([pscustomobject]$record)
}

try {
    Set-ElementalRacesEnabled
    $legacyDeploymentManifestPath = & $deployLegacy `
        -PackagePath $legacy -Confirm:$false -PassThru
    if ([string]::IsNullOrWhiteSpace($legacyDeploymentManifestPath)) {
        throw 'Pinned 0.0.114 deployment did not return its evidence manifest.'
    }
    Invoke-QualifiedPhase `
        -Scenario 'elemental-race-persistence-prepare' -Version '0.0.114' `
        -DeploymentManifest $legacyDeploymentManifestPath -Package $legacy `
        -UseQualifiedLegacy

    $migrationDeploymentManifestPath = & $deployCurrent `
        -PackagePath $current.PackagePath -Confirm:$false -PassThru
    if ([string]::IsNullOrWhiteSpace($migrationDeploymentManifestPath)) {
        throw 'Current migration build deployment did not return its evidence manifest.'
    }
    Invoke-QualifiedPhase `
        -Scenario 'elemental-race-legacy-migration' `
        -Version $ExpectedVersion `
        -DeploymentManifest $migrationDeploymentManifestPath `
        -Package $current.PackagePath
    Invoke-QualifiedPhase `
        -Scenario 'elemental-race-persistence-verify-absent' `
        -Version $ExpectedVersion `
        -DeploymentManifest $migrationDeploymentManifestPath `
        -Package $current.PackagePath
}
catch { $failure = $_ }
finally {
    try {
        Wait-ForGuardedKingmakerExit 'final restoration'
        Restore-OriginalFeatureState
        $restoredDeploymentManifestPath = & $deployCurrent `
            -PackagePath $current.PackagePath -Confirm:$false -PassThru
        if ([string]::IsNullOrWhiteSpace($restoredDeploymentManifestPath)) {
            throw 'Final current-build restoration did not return a deployment manifest.'
        }
        [void](Assert-KmgReusableDeployment `
            -DeploymentManifestPath $restoredDeploymentManifestPath `
            -PackagePath $current.PackagePath -RepositoryRoot $root `
            -ExpectedVersion $ExpectedVersion `
            -AllowDirtyGit:$AllowDirtyGit)
        $restoredExists = Test-Path -LiteralPath $settings -PathType Leaf
        $restoredSha = if ($restoredExists) {
            Get-KmgSha256 -Path $settings
        } else { '<absent>' }
        if ($restoredExists -ne $originalSettingsExisted -or
            $restoredSha -cne $originalSettingsSha) {
            throw 'Feature-module settings existence or bytes were not restored exactly.'
        }
    }
    catch { $restorationFailure = $_ }

    $transactionStatus = if ($failure -eq $null -and
        $restorationFailure -eq $null -and $phases.Count -eq 3) {
        'PASS'
    } else { 'FAIL' }
    $transaction = [ordered]@{
        schemaVersion = 1
        transactionId = $transactionId
        status = $transactionStatus
        saveName = $SaveName
        protectedBaselineExcluded = $true
        legacyProducerCommit =
            '6874dc15a27ded132456dbdd480f47c794543a05'
        legacyPackagePath = $legacy
        legacyPackageSha256 = Get-KmgSha256 -Path $legacy
        currentPackagePath = $current.PackagePath
        currentPackageSha256 = Get-KmgSha256 -Path $current.PackagePath
        originalSettingsExisted = $originalSettingsExisted
        originalSettingsSha256 = $originalSettingsSha
        restoredSettingsSha256 = if (Test-Path -LiteralPath $settings `
                -PathType Leaf) {
            Get-KmgSha256 -Path $settings
        } else { '<absent>' }
        legacyDeploymentManifestPath = $legacyDeploymentManifestPath
        migrationDeploymentManifestPath = $migrationDeploymentManifestPath
        restoredDeploymentManifestPath = $restoredDeploymentManifestPath
        phases = @($phases)
        failure = if ($failure) { $failure.Exception.Message } else { '' }
        restorationFailure = if ($restorationFailure) {
            $restorationFailure.Exception.Message
        } else { '' }
        completedAtUtc = [DateTime]::UtcNow.ToString('o')
    }
    $transactionPath = Join-Path $transactionDirectory `
        'elemental-race-legacy-migration-transaction.json'
    $transaction | ConvertTo-Json -Depth 7 | Set-Content `
        -LiteralPath $transactionPath -Encoding UTF8
    Write-Host "Legacy migration transaction evidence: $transactionPath"
}

if ($restorationFailure -ne $null) { throw $restorationFailure }
if ($failure -ne $null) { throw $failure }
Write-Host ('Elemental Race 0.0.114-to-{0} migration PASS; phases={1}; ' +
    'current artifact and exact feature settings restored.' -f
    $ExpectedVersion, $phases.Count)
