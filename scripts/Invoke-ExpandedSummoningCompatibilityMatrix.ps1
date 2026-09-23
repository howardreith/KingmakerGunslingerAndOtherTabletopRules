Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Sprint 0 item B6: the Expanded Summoning compatibility and feature-boundary
# matrix.
#
# Each step is its own profile invocation. The compatibility launcher throws on
# the first scenario that does not record PASS and skips the rest of its list,
# so batching a scenario expected to fail for unrelated reasons behind the ones
# being measured would silently drop them.
#
# The first attempt at this matrix cost a manual recovery. One step's game was
# starved past its timeout, the launcher could not restore because the process
# was still running, and the remaining six steps then each failed on that same
# leftover process - six pointless failures whose message said nothing about the
# real problem, and an open transaction left on the machine the whole time.
# Hence the guards below: a step that leaves Kingmaker running or a transaction
# open stops the matrix immediately, and the live tree is fingerprinted before
# and after so a restoration failure is caught here rather than in a log.
#
# Brown-Fur Transmuter is off in every module state: it requires Call of the
# Wild, which gunslinger-only deliberately does not stage.

$install = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker'
$liveMod = Join-Path $install 'Mods\KingmakerGunslinger'
$runtimeTimeoutSeconds = 600

function Get-KmgLiveFingerprint {
    if (-not (Test-Path -LiteralPath $liveMod -PathType Container)) {
        return [pscustomobject]@{ Files = 0; Version = '<absent>'; Dll = '<absent>' }
    }
    $files = @(Get-ChildItem -LiteralPath $liveMod -Recurse -File)
    $info = Get-Content -Raw -LiteralPath (Join-Path $liveMod 'Info.json') |
        ConvertFrom-Json
    return [pscustomobject]@{
        Files = $files.Count
        Version = [string]$info.Version
        Dll = (Get-FileHash -LiteralPath (Join-Path $liveMod `
            'KingmakerGunslinger.dll') -Algorithm SHA256).Hash
    }
}

<#
.SYNOPSIS
Every scenario this driver schedules must be one the harness will run
unattended.

.DESCRIPTION
observe-expanded-summoning-variant-menu is supervised: its harness metadata
sets RequiresManualInteraction and it waits for a human to open a menu.
Scheduling it aborted every profile in the first run of this matrix, before
the mechanical scenario could execute, because the compatibility launcher
stops a profile's list at the first scenario that does not record PASS.
#>
function Assert-KmgScenariosUnattended([string[]]$Names) {
    $harness = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot `
        'RuntimeAutomation.Common.ps1')
    foreach ($name in ($Names | Sort-Object -Unique)) {
        $pattern = "'" + [regex]::Escape($name) +
            '''\s*=\s*\[pscustomobject\]@\{(?<body>[\s\S]*?)\r?\n    \}'
        $block = [regex]::Match($harness, $pattern)
        if (-not $block.Success) {
            throw "Scenario is not declared in the harness: $name"
        }
        if ($block.Groups['body'].Value -match
            'RequiresManualInteraction\s*=\s*\$true') {
            throw ("Refusing to schedule a supervised scenario in an " +
                "unattended matrix: $name")
        }
    }
}

function Assert-KmgMachineClean([string]$Phase) {
    $running = @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue)
    if ($running.Count -ne 0) {
        throw ("$Phase - Kingmaker is still running (PID(s): " +
            ($running.Id -join ', ') + "). Stopping the matrix: every " +
            'remaining step would fail on this same process, and a compatibility ' +
            'transaction cannot restore while it holds the install.')
    }
    $open = @(Get-ChildItem -LiteralPath $install -Directory `
        -Filter 'Mods.kmg-compat-*' -ErrorAction SilentlyContinue)
    if ($open.Count -ne 0) {
        throw ("$Phase - a compatibility transaction is still open: " +
            ($open.Name -join ', ') + ". Stopping the matrix; restore it with " +
            'scripts\compatibility\Restore-KingmakerCompatibilityProfile.ps1.')
    }
}

$modulesOn = [ordered]@{
    gunslinger                             = $true
    acadamaeGraduate                       = $true
    shieldOther                            = $true
    expandedSummoning                      = $true
    elvenBranchedSpears                    = $true
    easternWeapons                         = $true
    brownFurTransmuter                     = $false
    urbanBarbarian                         = $true
    bodyguardFeats                         = $true
    protectionFromAlignmentControlImmunity = $true
    elementalRaces                         = $true
    teleportationSpells                    = $true
    magicCircleSpells                      = $true
}
$modulesSummoningOff = [ordered]@{}
foreach ($key in $modulesOn.Keys) { $modulesSummoningOff[$key] = $modulesOn[$key] }
$modulesSummoningOff['expandedSummoning'] = $false

# The variant menu is observed supervised, not here: see
# Assert-KmgScenariosUnattended. The projected-menu fixture is the
# unattended measurement of that surface.
$observe = @('observe-expanded-summoning-inventory')
$profileScenarios = @('mod-load-smoke', 'observe-optional-mod-compatibility') + $observe

$steps = @(
    [pscustomobject]@{ Name = 'boundary-summoning-on'
        ProfileId = 'gunslinger-only'
        Scenario = @('observe-feature-module-settings')
        Parameters = $modulesOn }
    [pscustomobject]@{ Name = 'boundary-summoning-off'
        ProfileId = 'gunslinger-only'
        Scenario = @('observe-feature-module-settings')
        Parameters = $modulesSummoningOff }
    [pscustomobject]@{ Name = 'profile-gunslinger-only'
        ProfileId = 'gunslinger-only'
        Scenario = $profileScenarios + @('disposable-expanded-summoning')
        Parameters = @{} }
    [pscustomobject]@{ Name = 'profile-call-of-the-wild'
        ProfileId = 'gunslinger-call-of-the-wild'
        Scenario = $profileScenarios
        Parameters = @{} }
    [pscustomobject]@{ Name = 'profile-arms-armor'
        ProfileId = 'gunslinger-arms-armor'
        Scenario = $profileScenarios
        Parameters = @{} }
    [pscustomobject]@{ Name = 'profile-toggle-custom-soundpacks'
        ProfileId = 'gunslinger-toggle-custom-soundpacks'
        Scenario = $profileScenarios
        Parameters = @{} }
    [pscustomobject]@{ Name = 'profile-high-risk-combined'
        ProfileId = 'gunslinger-high-risk-combined'
        Scenario = $profileScenarios + @('disposable-expanded-summoning')
        Parameters = @{} }
)

$reportPath = Join-Path $PSScriptRoot 'b6-matrix-report.json'
$before = Get-KmgLiveFingerprint
$record = [ordered]@{
    schemaVersion = 2
    startedAtUtc  = [DateTime]::UtcNow.ToString('o')
    expectedVersion = '0.0.136'
    runtimeTimeoutSeconds = $runtimeTimeoutSeconds
    liveBefore = [ordered]@{ files = $before.Files; version = $before.Version
        dll = $before.Dll }
    steps = @()
}
Write-Host ("Live tree before: version={0} files={1}" -f $before.Version, $before.Files)
Assert-KmgScenariosUnattended (@($steps | ForEach-Object { $_.Scenario }) |
    ForEach-Object { $_ })
Assert-KmgMachineClean 'before the matrix'

$halted = $null
foreach ($step in $steps) {
    Write-Host ''
    Write-Host ('=== B6 step: {0} ({1}) ===' -f $step.Name, $step.ProfileId)
    $entry = [ordered]@{
        name = $step.Name
        profileId = $step.ProfileId
        scenarios = @($step.Scenario)
        startedAtUtc = [DateTime]::UtcNow.ToString('o')
    }
    $arguments = @{
        ProfileId = $step.ProfileId
        Scenario  = @($step.Scenario)
        RuntimeTimeoutSeconds = $runtimeTimeoutSeconds
        Confirm   = $false
    }
    if ($step.Parameters.Count -gt 0) { $arguments.Parameters = $step.Parameters }
    try {
        & '.\scripts\compatibility\Invoke-KingmakerCompatibilityProfile.ps1' @arguments
        $entry.outcome = 'COMPLETED'
    }
    catch {
        # A step may fail on its own assertions and the matrix should carry on;
        # what it must not do is carry on over a dirty machine.
        $entry.outcome = 'FAILED'
        $entry.error = $_.Exception.Message
        Write-Warning ('B6 step {0} failed: {1}' -f $step.Name, $_.Exception.Message)
    }
    $entry.completedAtUtc = [DateTime]::UtcNow.ToString('o')
    try {
        Assert-KmgMachineClean ('after step ' + $step.Name)
        $entry.machineClean = $true
    }
    catch {
        $entry.machineClean = $false
        $entry.haltReason = $_.Exception.Message
        $halted = $_.Exception.Message
    }
    $record.steps += $entry
    $record | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportPath -Encoding UTF8
    if ($halted) { break }
}

$after = Get-KmgLiveFingerprint
$record.liveAfter = [ordered]@{ files = $after.Files; version = $after.Version
    dll = $after.Dll }
$record.liveTreeRestored = ($after.Files -eq $before.Files -and
    $after.Version -ceq $before.Version -and $after.Dll -ceq $before.Dll)
$record.halted = $halted
$record.completedAtUtc = [DateTime]::UtcNow.ToString('o')
$record | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportPath -Encoding UTF8

Write-Host ''
Write-Host ('B6 matrix finished; report: {0}' -f $reportPath)
foreach ($entry in $record.steps) {
    Write-Host ('  {0,-34} {1}' -f $entry.name, $entry.outcome)
}
Write-Host ("Live tree after: version={0} files={1} restored={2}" -f `
    $after.Version, $after.Files, $record.liveTreeRestored)
if ($halted) { Write-Warning ("Matrix halted: {0}" -f $halted) }
if (-not $record.liveTreeRestored) {
    throw 'The live mod tree did not return to its pre-matrix state.'
}
