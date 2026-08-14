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
        'disposable-firearm-dependent-feats',
        'disposable-pistolero-deeds',
        'disposable-archetype-reconciliation',
        'musket-master-mechanics-and-starter',
        'disposable-firearm-visual-rigs',
        'disposable-production-firearm-switching',
        'disposable-gunslinger-targeting-arms',
        'disposable-gunslinger-dodge',
        'disposable-firearm-wwise-audio',
        'disposable-gunslinger-scatter-shot',
        'disposable-reload-autocast',
        'disposable-paper-cartridge-mode-view-lifecycle',
        'observe-rare-firearm-blueprint-contracts',
        'observe-vendor-table-contracts',
        'magic-firearm-native-properties',
        'reliable-firearm-misfire-matrix',
        'blunderbuss-thundering-scatter',
        'disposable-paper-cartridge-comprehensive',
        'observe-feature-module-settings',
        'observe-elven-branched-spear-contracts',
        'observe-eastern-weapon-contracts',
        'disposable-elven-branched-spear-combat',
        'disposable-eastern-weapons-combat',
        'observe-expanded-summoning-inventory',
        'disposable-shield-other',
        'disposable-acadamae-graduate',
        'disposable-gunslinger-comprehensive-acceptance')]
    [string[]]$Scenario,
    [hashtable]$Parameters = @{},
    [ValidateRange(120, 900)]
    [int]$RuntimeTimeoutSeconds = 300,
    [switch]$AllowDirtyGit,
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

$moduleScenario = @($Scenario | Where-Object { $_ -ceq
    'observe-feature-module-settings' }).Count -gt 0 -or
    (@($Scenario | Where-Object { $_ -ceq
        'observe-vendor-table-contracts' }).Count -gt 0 -and
        $Parameters.Count -gt 0)
if ($moduleScenario) {
    $keys = @($Parameters.Keys | Sort-Object)
    if ($keys.Count -ne 6 -or $keys[0] -cne 'acadamaeGraduate' -or
        $keys[1] -cne 'easternWeapons' -or
        $keys[2] -cne 'elvenBranchedSpears' -or
        $keys[3] -cne 'expandedSummoning' -or $keys[4] -cne 'gunslinger' -or
        $keys[5] -cne 'shieldOther' -or
        $Parameters.gunslinger -isnot [bool] -or
        $Parameters.acadamaeGraduate -isnot [bool] -or
        $Parameters.shieldOther -isnot [bool] -or
        $Parameters.expandedSummoning -isnot [bool] -or
        $Parameters.elvenBranchedSpears -isnot [bool] -or
        $Parameters.easternWeapons -isnot [bool]) {
        throw 'Feature-module profile observation requires exactly six Boolean parameters: gunslinger, acadamaeGraduate, shieldOther, expandedSummoning, elvenBranchedSpears, and easternWeapons.'
    }
} elseif ($Parameters.Count -ne 0) {
    throw 'Compatibility profile parameters are supported only for observe-feature-module-settings or module-state vendor-table observation.'
}

if (-not $PSCmdlet.ShouldProcess((Join-Path $KingmakerInstallDir 'Mods'),
    "run isolated profile $ProfileId and restore exact original state")) { return }
try {
    & (Join-Path $PSScriptRoot 'Enter-KingmakerCompatibilityProfile.ps1') `
        -ProfileId $ProfileId -RunId $runId -KingmakerInstallDir $KingmakerInstallDir `
        -StateRoot $StateRoot -Confirm:$false | Out-Host
    $entered = $true
    if ($moduleScenario) {
        $settingsPath = Join-Path $KingmakerInstallDir `
            'Mods\KingmakerGunslinger\FeatureModules.json'
        $settings = [ordered]@{ schemaVersion = 5
            gunslinger = [bool]$Parameters.gunslinger
            'acadamae-graduate' = [bool]$Parameters.acadamaeGraduate
            'shield-other' = [bool]$Parameters.shieldOther
            'expanded-summoning' = [bool]$Parameters.expandedSummoning
            'elven-branched-spears' = [bool]$Parameters.elvenBranchedSpears
            'eastern-weapons' = [bool]$Parameters.easternWeapons }
        $temporary = $settingsPath + '.kmg-profile.tmp'
        [IO.File]::WriteAllText($temporary,
            ($settings | ConvertTo-Json -Depth 4),
            (New-Object Text.UTF8Encoding($false)))
        Move-Item -LiteralPath $temporary -Destination $settingsPath -Force
    }
    foreach ($name in $Scenario) {
        $before = [DateTime]::UtcNow
        $arguments = @{
            Scenario = $name
            ExpectedVersion = '0.0.80'
            ExitAfterCompletion = $true
            TimeoutSeconds = $RuntimeTimeoutSeconds
            ObserverStartupTimeoutSeconds = $RuntimeTimeoutSeconds
            Confirm = $false
            AllowDirtyGit = [bool]$AllowDirtyGit
        }
        if ($name -ceq 'observe-optional-mod-compatibility') {
            $arguments.CompatibilityProfileId = $ProfileId
        }
        if ($name -ceq 'observe-feature-module-settings') {
            $arguments.Parameters = $Parameters
        }
        if ($name -ceq 'musket-master-mechanics-and-starter') {
            $arguments.SaveName = 'KMG_AUTOMATION_WORKING'
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
