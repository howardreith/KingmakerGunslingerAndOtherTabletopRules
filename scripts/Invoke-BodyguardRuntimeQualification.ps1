[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.110',
    [ValidateRange(5, 1800)][int]$TimeoutSeconds = 300,
    [Parameter(Mandatory = $true)][string]$DeploymentManifestPath,
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [switch]$AllowDirtyGit,
    [switch]$ConfirmEach
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$invoke = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$settings = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger\FeatureModules.json'
$expectedParent = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger'
$resolvedParent = [IO.Path]::GetFullPath((Split-Path -Parent $settings)).TrimEnd('\')
if ($resolvedParent -cne $expectedParent) {
    throw 'Feature-module settings target changed unexpectedly.'
}
if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
    throw 'Pathfinder: Kingmaker must not be running before Bodyguard qualification.'
}
foreach ($path in @($DeploymentManifestPath, $PackagePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Pinned Bodyguard qualification artifact is missing: $path"
    }
}
if (-not $PSCmdlet.ShouldProcess($settings,
    'run five guarded Bodyguard/Aid Another qualification launches with exact settings restoration')) {
    return
}

$ConfirmPreference = 'None'
$originalExists = Test-Path -LiteralPath $settings -PathType Leaf
$originalBytes = if ($originalExists) {
    [IO.File]::ReadAllBytes($settings)
} else { $null }
$failure = $null

function Set-BodyguardFeatureState([bool]$enabled) {
    $configuration = [ordered]@{
        schemaVersion = 9
        gunslinger = $true
        'acadamae-graduate' = $true
        'shield-other' = $true
        'expanded-summoning' = $true
        'elven-branched-spears' = $true
        'eastern-weapons' = $true
        'brown-fur-transmuter' = $true
        'urban-barbarian' = $true
        'bodyguard-feats' = $enabled
        'protection-from-alignment-control-immunity' = $true
    }
    $temporary = $settings + '.kmg-bodyguard-qualification.tmp'
    [IO.File]::WriteAllText($temporary,
        ($configuration | ConvertTo-Json -Depth 4),
        (New-Object Text.UTF8Encoding($false)))
    Move-Item -LiteralPath $temporary -Destination $settings -Force
}

function Restore-OriginalFeatureState {
    if ($originalExists) {
        $temporary = $settings + '.kmg-bodyguard-qualification-restore.tmp'
        [IO.File]::WriteAllBytes($temporary, $originalBytes)
        Move-Item -LiteralPath $temporary -Destination $settings -Force
    }
    elseif (Test-Path -LiteralPath $settings) {
        Remove-Item -LiteralPath $settings -Force
    }
}

function Wait-ForGuardedKingmakerExit([string]$scenario) {
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and
        [DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 250
    }
    if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
        throw "Kingmaker did not exit within 45 seconds after $scenario."
    }
}

function Invoke-BodyguardScenario([string]$scenario) {
    & $invoke -Scenario $scenario -ExpectedVersion $ExpectedVersion `
        -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
        -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach `
        -ReuseInstalledArtifact `
        -DeploymentManifestPath $DeploymentManifestPath `
        -PackagePath $PackagePath
    if ($LASTEXITCODE -ne 0) {
        throw "Bodyguard guarded scenario failed: $scenario"
    }
    Wait-ForGuardedKingmakerExit $scenario
}

try {
    Set-BodyguardFeatureState $true
    Invoke-BodyguardScenario 'observe-bodyguard-native-contracts'
    Invoke-BodyguardScenario 'observe-aid-another-compatibility-contracts'
    Invoke-BodyguardScenario 'disposable-bodyguard-feats'
    Invoke-BodyguardScenario 'disposable-helpful-bodyguard'
    Set-BodyguardFeatureState $false
    Invoke-BodyguardScenario 'disposable-bodyguard-feats-disabled'
}
catch { $failure = $_ }
finally {
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
Write-Host "Bodyguard guarded qualification PASS; package=$PackagePath; deployment=$DeploymentManifestPath"
