[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'gunslinger-only',
        'gunslinger-call-of-the-wild',
        'gunslinger-arms-armor',
        'gunslinger-toggle-custom-soundpacks',
        'gunslinger-high-risk-combined',
        'gunslinger-all-loadable-local',
        'gunslinger-qualified-combined')]
    [string]$ProfileId,
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'mod-load-smoke',
        'observe-optional-mod-compatibility',
        'observe-class-blueprint-contracts',
        'observe-gunslinger-presentation',
        'disposable-firearm-visual-rigs',
        'disposable-production-firearm-switching',
        'disposable-gunslinger-targeting-arms',
        'disposable-gunslinger-dodge',
        'disposable-firearm-wwise-audio',
        'disposable-gunslinger-scatter-shot',
        'disposable-reload-autocast',
        'disposable-gunslinger-comprehensive-acceptance')]
    [string[]]$Scenario,
    [ValidateRange(120, 900)]
    [int]$RuntimeTimeoutSeconds = 300,
    [string]$KingmakerInstallDir =
        'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    [string]$StateRoot = 'C:\Dev\KingmakerGunslingerLab\compatibility-state'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$runId = 'compat-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' +
    [Guid]::NewGuid().ToString('N').Substring(0, 12)
$entered = $false
$primaryError = $null
$results = [Collections.Generic.List[object]]::new()

if (-not $PSCmdlet.ShouldProcess((Join-Path $KingmakerInstallDir 'Mods'),
    "run isolated profile $ProfileId and restore exact original state")) { return }
try {
    & (Join-Path $PSScriptRoot 'Enter-KingmakerCompatibilityProfile.ps1') `
        -ProfileId $ProfileId -RunId $runId -KingmakerInstallDir $KingmakerInstallDir `
        -StateRoot $StateRoot -Confirm:$false | Out-Host
    $entered = $true
    foreach ($name in $Scenario) {
        $before = [DateTime]::UtcNow
        $arguments = @{
            Scenario = $name
            ExpectedVersion = '0.0.72'
            ExitAfterCompletion = $true
            TimeoutSeconds = $RuntimeTimeoutSeconds
            ObserverStartupTimeoutSeconds = $RuntimeTimeoutSeconds
            Confirm = $false
        }
        if ($name -ceq 'observe-optional-mod-compatibility') {
            $arguments.CompatibilityProfileId = $ProfileId
        }
        & (Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1') @arguments
        $evidence = Get-ChildItem -LiteralPath 'C:\Dev\KingmakerGunslingerLab\runtime-evidence' `
            -Directory | Where-Object { $_.LastWriteTimeUtc -ge $before.AddSeconds(-2) } |
            Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
        if ($null -eq $evidence -or -not (Test-Path -LiteralPath `
            (Join-Path $evidence.FullName 'runtime-result.json') -PathType Leaf)) {
            throw "Runtime scenario result directory was not resolved: $name"
        }
        $result = Get-Content -LiteralPath (Join-Path $evidence.FullName `
            'runtime-result.json') -Raw | ConvertFrom-Json
        if ($result.scenario -cne $name -or $result.status -cne 'PASS') {
            throw "Runtime scenario result mismatch: expected $name PASS."
        }
        $results.Add([ordered]@{ scenario = $name; runId = $result.runId
            status = $result.status; evidenceDirectory = $evidence.FullName })
        $exitDeadline = [DateTime]::UtcNow.AddSeconds(60)
        while (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and
            [DateTime]::UtcNow -lt $exitDeadline) { Start-Sleep -Milliseconds 500 }
        if (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0) {
            throw "Kingmaker did not complete guarded automatic exit after scenario: $name"
        }
    }
}
catch {
    $primaryError = $_
}
finally {
    if ($entered) {
        $deadline = [DateTime]::UtcNow.AddSeconds(60)
        while (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and
            [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 500 }
        try {
            & (Join-Path $PSScriptRoot 'Restore-KingmakerCompatibilityProfile.ps1') `
                -RunId $runId -StateRoot $StateRoot -Confirm:$false | Out-Host
        }
        catch {
            if ($null -ne $primaryError) {
                throw "Profile failed: $($primaryError.Exception.Message) Restoration also failed: $($_.Exception.Message)"
            }
            throw
        }
    }
}
if ($null -ne $primaryError) { throw $primaryError }
$state = Get-Content -LiteralPath (Join-Path $StateRoot "$runId\transaction.json") `
    -Raw | ConvertFrom-Json
if (-not $state.restorationVerified -or $state.status -cne 'Restored') {
    throw "Profile completed but exact restoration was not verified: $runId"
}
[pscustomobject][ordered]@{
    profileId = $ProfileId
    transactionRunId = $runId
    restorationVerified = $true
    stagedMutationObserved = [bool]$state.stagedMutationObserved
    runtimeResults = @($results)
}
