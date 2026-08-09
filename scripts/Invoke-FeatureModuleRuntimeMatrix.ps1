[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.75',
    [ValidateRange(5, 1800)][int]$TimeoutSeconds = 300,
    [ValidateSet('all', 'on-off', 'off-on', 'off-off')]
    [string]$Combination = 'all',
    [bool]$ExitAfterCompletion = $true,
    [switch]$ConfirmEach
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$invoke = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$settings = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger\FeatureModules.json'
$expectedParent = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger'
$resolvedParent = [IO.Path]::GetFullPath((Split-Path -Parent $settings)).TrimEnd('\')
if ($resolvedParent -cne $expectedParent) { throw 'Feature-module settings target changed unexpectedly.' }
if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
    throw 'Pathfinder: Kingmaker must not be running before a settings transaction.'
}

$originalExists = Test-Path -LiteralPath $settings -PathType Leaf
$originalBytes = if ($originalExists) { [IO.File]::ReadAllBytes($settings) } else { $null }
$sha = [Security.Cryptography.SHA256]::Create()
try {
    $originalHash = if ($originalExists) {
        ([BitConverter]::ToString($sha.ComputeHash($originalBytes))).Replace('-', '').ToLowerInvariant()
    } else { '<absent>' }
} finally { $sha.Dispose() }

$combinations = [ordered]@{
    'on-on' = [ordered]@{ gunslinger = $true; acadamaeGraduate = $true }
    'on-off' = [ordered]@{ gunslinger = $true; acadamaeGraduate = $false }
    'off-on' = [ordered]@{ gunslinger = $false; acadamaeGraduate = $true }
    'off-off' = [ordered]@{ gunslinger = $false; acadamaeGraduate = $false }
}
if ($Combination -ne 'all') {
    $selected = [ordered]@{}
    $selected[$Combination] = $combinations[$Combination]
    $combinations = $selected
}

$failure = $null
try {
    foreach ($entry in $combinations.GetEnumerator()) {
        $configuration = [ordered]@{
            schemaVersion = 1
            gunslinger = [bool]$entry.Value.gunslinger
            'acadamae-graduate' = [bool]$entry.Value.acadamaeGraduate
        }
        $json = $configuration | ConvertTo-Json -Depth 4
        $temporary = $settings + '.kmg-module-matrix.tmp'
        [IO.File]::WriteAllText($temporary, $json, (New-Object Text.UTF8Encoding($false)))
        Move-Item -LiteralPath $temporary -Destination $settings -Force
        & $invoke -Scenario observe-feature-module-settings `
            -ExpectedVersion $ExpectedVersion -TimeoutSeconds $TimeoutSeconds `
            -Parameters @{ gunslinger = [bool]$entry.Value.gunslinger;
                acadamaeGraduate = [bool]$entry.Value.acadamaeGraduate } `
            -ExitAfterCompletion:$ExitAfterCompletion -Confirm:$ConfirmEach
        if ($LASTEXITCODE -ne 0) {
            throw "Feature-module runtime combination $($entry.Key) failed."
        }
        if ($ExitAfterCompletion) {
            $exitDeadline = [DateTime]::UtcNow.AddSeconds(30)
            while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and
                [DateTime]::UtcNow -lt $exitDeadline) {
                Start-Sleep -Milliseconds 250
            }
            if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
                throw "Kingmaker did not exit within 30 seconds after combination $($entry.Key)."
            }
        }
    }
} catch {
    $failure = $_
} finally {
    if ($originalExists) {
        $temporary = $settings + '.kmg-module-restore.tmp'
        [IO.File]::WriteAllBytes($temporary, $originalBytes)
        Move-Item -LiteralPath $temporary -Destination $settings -Force
    } elseif (Test-Path -LiteralPath $settings) {
        Remove-Item -LiteralPath $settings -Force
    }
    $restoredExists = Test-Path -LiteralPath $settings -PathType Leaf
    if ($restoredExists -ne $originalExists) { throw 'Settings existence restoration failed.' }
    if ($originalExists) {
        $restored = [IO.File]::ReadAllBytes($settings)
        if ($restored.Length -ne $originalBytes.Length -or
            [Convert]::ToBase64String($restored) -cne [Convert]::ToBase64String($originalBytes)) {
            throw 'Settings byte-for-byte restoration failed.'
        }
    }
    Write-Host "Feature-module settings restored exactly; original SHA-256: $originalHash"
}
if ($failure -ne $null) { throw $failure }
Write-Host "Feature-module runtime matrix PASS: $($combinations.Keys -join ', ')"
