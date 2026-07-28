[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
function Assert-True([bool]$Condition, [string]$Name) {
    if (-not $Condition) { $failures.Add($Name) }
}
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $failures.Add($Name) } catch { }
}

$requestPath = Join-Path $script:KmgRuntimeEvidenceRoot 'source-only-test\runtime request.json'
$arguments = @(Get-KmgSteamLaunchArguments -AppId 640820 -RequestPath $requestPath)
Assert-True ($arguments.Count -eq 4) 'launch-argument-count'
Assert-True ($arguments[0] -eq '-applaunch' -and $arguments[1] -eq '640820') 'steam-app-launch'
Assert-True ($arguments[2] -eq '-kmgRuntimeTestRequest') 'guarded-flag-after-app-id'
Assert-True ($arguments[3] -eq "`"$requestPath`"") 'request-path-safely-quoted'
Assert-Throws { Get-KmgSteamLaunchArguments -AppId 1 -RequestPath $requestPath } 'incorrect-app-id'
Assert-Throws { Get-KmgSteamLaunchArguments -AppId 640820 -RequestPath 'C:\Windows\request.json' } 'request-outside-root'
Assert-Throws { Assert-KmgSteamExecutable -SteamPath 'C:\not-steam.exe' } 'unapproved-steam-executable'

$requested = [DateTime]::UtcNow
$steam = [pscustomobject]@{ Id = 10; ProcessName = 'steam'; StartTime = $requested.ToLocalTime() }
$oldGame = [pscustomobject]@{ Id = 20; ProcessName = 'Kingmaker'; StartTime = $requested.AddMinutes(-2).ToLocalTime() }
$newGame = [pscustomobject]@{ Id = 30; ProcessName = 'Kingmaker'; StartTime = $requested.AddSeconds(1).ToLocalTime() }
$selected = Select-KmgNewKingmakerProcess -Processes @($steam, $oldGame, $newGame) `
    -ExistingProcessIds @(20) -RequestedAtUtc $requested
Assert-True ($selected.Id -eq 30) 'new-game-selected-deterministically'
Assert-True ($selected.Id -ne $steam.Id) 'steam-not-mistaken-for-game'
Assert-Throws {
    Select-KmgNewKingmakerProcess -Processes @(
        $newGame,
        [pscustomobject]@{
            Id = 31
            ProcessName = 'Kingmaker'
            StartTime = $requested.AddSeconds(2).ToLocalTime()
        }) -ExistingProcessIds @() -RequestedAtUtc $requested
} 'multiple-new-games-rejected'

$orchestrator = Get-Content -LiteralPath (
    Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Raw
Assert-True ($orchestrator.Contains('Start-KmgSteamKingmaker')) 'orchestrator-uses-steam-launch'
Assert-True (-not $orchestrator.Contains('Start-Process -FilePath $KingmakerPath')) `
    'direct-game-launch-rejected-by-default'
Assert-True (-not $orchestrator.Contains('Kingmaker.exe')) 'no-implicit-direct-fallback'
Assert-True ($orchestrator.Contains('Source-only/WhatIf validation passed. No deployment or process launch occurred.')) `
    'whatif-launches-no-process'

$common = Get-Content -LiteralPath (
    Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1') -Raw
Assert-True ($common.Contains('Steam executable is missing')) 'missing-steam-fails'
Assert-True ($common.Contains('Steam client process did not become available')) 'steam-timeout-fails'
Assert-True ($common.Contains('direct-executable fallback is disabled')) 'game-start-failure-no-fallback'
Assert-True ($common.Contains('Assert-KmgUnelevated')) 'elevation-rejected'
Assert-True ($common.Contains('Assert-KmgProcessOwner')) 'same-user-verified'
Assert-True ($common.Contains('sanitizedLaunchArguments')) 'sanitized-arguments-recorded'

if ($failures.Count -ne 0) {
    throw "Steam launch source tests failed: $($failures -join ', ')"
}
Write-Host 'Steam launch source tests passed: 19'
