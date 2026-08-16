[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.83',
    [string]$SaveName = 'KMG_AUTOMATION_WORKING',
    [ValidateRange(120, 900)][int]$TimeoutSeconds = 300,
    [ValidateSet('prepare', 'cleanup', 'absent')]
    [string]$StartPhase = 'prepare',
    [switch]$AllowDirtyGit,
    [switch]$ConfirmEach
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($SaveName -cne 'KMG_AUTOMATION_WORKING') {
    throw 'Eastern Weapons persistence qualification permits only KMG_AUTOMATION_WORKING.'
}
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$invoke = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$settings = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger\FeatureModules.json'
$expectedParent = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger'
if ([IO.Path]::GetFullPath((Split-Path -Parent $settings)).TrimEnd('\') -cne
    $expectedParent) { throw 'Feature-module settings target changed unexpectedly.' }
if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
    throw 'Pathfinder: Kingmaker must not be running before persistence qualification.'
}
if (-not $PSCmdlet.ShouldProcess($SaveName,
    'run the authorized three-phase Eastern Weapons persistence sequence')) {
    return
}

$originalExists = Test-Path -LiteralPath $settings -PathType Leaf
$originalBytes = if ($originalExists) { [IO.File]::ReadAllBytes($settings) } else { $null }
$failure = $null
function Set-EasternFeatureState([bool]$enabled) {
    $configuration = [ordered]@{
        schemaVersion = 7
        gunslinger = $true
        'acadamae-graduate' = $true
        'shield-other' = $true
        'expanded-summoning' = $true
        'elven-branched-spears' = $true
        'eastern-weapons' = $enabled
        'brown-fur-transmuter' = $true
        'urban-barbarian' = $true
    }
    $temporary = $settings + '.kmg-eastern-persistence.tmp'
    [IO.File]::WriteAllText($temporary,
        ($configuration | ConvertTo-Json -Depth 4),
        (New-Object Text.UTF8Encoding($false)))
    Move-Item -LiteralPath $temporary -Destination $settings -Force
}
function Restore-OriginalFeatureState {
    if ($originalExists) {
        $temporary = $settings + '.kmg-eastern-persistence-restore.tmp'
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
        throw "Kingmaker did not exit within 45 seconds after Eastern persistence $phase."
    }
}
try {
    if ($StartPhase -ceq 'prepare') {
        Set-EasternFeatureState $true
        & $invoke -Scenario 'working-save-eastern-weapons-prepare' `
            -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
            -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
            -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach
        if ($LASTEXITCODE -ne 0) { throw 'Eastern persistence prepare failed.' }
        Wait-ForGuardedKingmakerExit 'prepare'
    }

    if ($StartPhase -ne 'absent') {
        Set-EasternFeatureState $false
        & $invoke -Scenario 'working-save-eastern-weapons-verify-cleanup' `
            -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
            -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
            -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach
        if ($LASTEXITCODE -ne 0) { throw 'Eastern persistence verify/cleanup failed.' }
        Wait-ForGuardedKingmakerExit 'verify-cleanup'
    }

    Restore-OriginalFeatureState
    & $invoke -Scenario 'working-save-eastern-weapons-verify-absent' `
        -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
        -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
        -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach
    if ($LASTEXITCODE -ne 0) { throw 'Eastern persistence final absence failed.' }
    Wait-ForGuardedKingmakerExit 'verify-absent'
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
Write-Host 'Eastern Weapons three-phase working-save persistence PASS.'
