[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'gunslinger-only',
        'gunslinger-call-of-the-wild',
        'gunslinger-call-of-the-wild-favored-class',
        'gunslinger-call-of-the-wild-favored-class-traits-disabled',
        'gunslinger-arms-armor',
        'gunslinger-toggle-custom-soundpacks',
        'gunslinger-high-risk-combined',
        'gunslinger-high-risk-combined-favored-class',
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
        'observe-brown-fur-cotw-contract',
        'observe-brown-fur-cotw-absent-isolation',
        'observe-aid-another-compatibility-contracts',
        'disposable-helpful-bodyguard',
        'disposable-bodyguard-feats-disabled',
        'observe-elven-branched-spear-contracts',
        'observe-eastern-weapon-contracts',
        'disposable-elven-branched-spear-combat',
        'disposable-eastern-weapons-combat',
        'working-save-eastern-weapons-prepare',
        'working-save-eastern-weapons-verify-cleanup',
        'working-save-eastern-weapons-verify-absent',
        'observe-expanded-summoning-inventory',
        'disposable-shield-other',
        'disposable-acadamae-graduate',
        'disposable-gunslinger-comprehensive-acceptance')]
    [string[]]$Scenario,
    [hashtable]$Parameters = @{},
    [ValidateRange(120, 900)]
    [int]$RuntimeTimeoutSeconds = 300,
    [ValidateSet('unchanged', 'normal', 'balance-fixes')]
    [string]$CotwProgressionMode = 'unchanged',
    [switch]$AllowDirtyGit,
    [switch]$ReuseInstalledArtifact,
    [string]$DeploymentManifestPath,
    [string]$PackagePath,
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
$cotwSettingsPath = Join-Path $KingmakerInstallDir `
    'Mods\CallOfTheWild\settings.json'
$cotwSettingsOriginalExists = Test-Path -LiteralPath $cotwSettingsPath `
    -PathType Leaf
$cotwSettingsOriginalBytes = if ($cotwSettingsOriginalExists) {
    [IO.File]::ReadAllBytes($cotwSettingsPath)
} else { $null }
$cotwSettingsOriginalSha = if ($cotwSettingsOriginalExists) {
    (Get-FileHash -LiteralPath $cotwSettingsPath -Algorithm SHA256).Hash
} else { $null }
$cotwSettingsStagedBeforeSha = $null
$cotwSettingsStagedAfterSha = $null
$favoredSettingsPath = Join-Path $KingmakerInstallDir `
    'Mods\ZFavoredClass\settings.json'
$favoredSettingsStagedBeforeSha = $null
$favoredSettingsStagedAfterSha = $null
$favoredTraitsMode = if ($ProfileId -ceq
    'gunslinger-call-of-the-wild-favored-class-traits-disabled') {
    'disabled'
} elseif ($ProfileId -in @(
    'gunslinger-call-of-the-wild-favored-class',
    'gunslinger-high-risk-combined-favored-class')) { 'enabled' } else {
    'unchanged'
}

$cotwProfileIds = @('gunslinger-call-of-the-wild',
    'gunslinger-call-of-the-wild-favored-class',
    'gunslinger-call-of-the-wild-favored-class-traits-disabled',
    'gunslinger-high-risk-combined-favored-class',
    'gunslinger-high-risk-combined', 'gunslinger-all-loadable-local')
if ($CotwProgressionMode -cne 'unchanged' -and
    $ProfileId -notin $cotwProfileIds) {
    throw "CotW progression mode requires a profile containing Call of the Wild."
}
if ($CotwProgressionMode -cne 'unchanged' -and
    -not $cotwSettingsOriginalExists) {
    throw "CotW progression mode requires an existing original settings file: $cotwSettingsPath"
}

$moduleScenario = @($Scenario | Where-Object { $_ -ceq
    'observe-feature-module-settings' }).Count -gt 0 -or
    (@($Scenario | Where-Object { $_ -ceq
        'observe-vendor-table-contracts' }).Count -gt 0 -and
        $Parameters.Count -gt 0)
if ($moduleScenario) {
    $keys = @($Parameters.Keys | Sort-Object)
    if ($keys.Count -ne 9 -or $keys[0] -cne 'acadamaeGraduate' -or
        $keys[1] -cne 'bodyguardFeats' -or
        $keys[2] -cne 'brownFurTransmuter' -or
        $keys[3] -cne 'easternWeapons' -or
        $keys[4] -cne 'elvenBranchedSpears' -or
        $keys[5] -cne 'expandedSummoning' -or $keys[6] -cne 'gunslinger' -or
        $keys[7] -cne 'shieldOther' -or $keys[8] -cne 'urbanBarbarian' -or
        $Parameters.gunslinger -isnot [bool] -or
        $Parameters.acadamaeGraduate -isnot [bool] -or
        $Parameters.shieldOther -isnot [bool] -or
        $Parameters.expandedSummoning -isnot [bool] -or
        $Parameters.elvenBranchedSpears -isnot [bool] -or
        $Parameters.easternWeapons -isnot [bool] -or
        $Parameters.brownFurTransmuter -isnot [bool] -or
        $Parameters.urbanBarbarian -isnot [bool] -or
        $Parameters.bodyguardFeats -isnot [bool]) {
        throw 'Feature-module profile observation requires exactly nine Boolean parameters: gunslinger, acadamaeGraduate, shieldOther, expandedSummoning, elvenBranchedSpears, easternWeapons, brownFurTransmuter, urbanBarbarian, and bodyguardFeats.'
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
    if ($CotwProgressionMode -cne 'unchanged') {
        if (-not (Test-Path -LiteralPath $cotwSettingsPath -PathType Leaf)) {
            throw "Staged CotW settings file is missing: $cotwSettingsPath"
        }
        $cotwSettingsStagedBeforeSha = (Get-FileHash -LiteralPath `
            $cotwSettingsPath -Algorithm SHA256).Hash
        $settingsText = [IO.File]::ReadAllText($cotwSettingsPath)
        $balancePattern = '("balance_fixes"\s*:\s*)(true|false)'
        $balanceMatches = [regex]::Matches($settingsText, $balancePattern,
            [Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($balanceMatches.Count -ne 1) {
            throw "Staged CotW settings must contain exactly one balance_fixes Boolean."
        }
        $desiredBalance = if ($CotwProgressionMode -ceq 'balance-fixes') {
            'true'
        } else { 'false' }
        $match = $balanceMatches[0]
        $replacement = $match.Groups[1].Value + $desiredBalance
        $updated = $settingsText.Substring(0, $match.Index) + $replacement +
            $settingsText.Substring($match.Index + $match.Length)
        $temporaryCotw = $cotwSettingsPath + '.kmg-progression.tmp'
        [IO.File]::WriteAllText($temporaryCotw, $updated,
            (New-Object Text.UTF8Encoding($false)))
        Move-Item -LiteralPath $temporaryCotw -Destination $cotwSettingsPath -Force
        $resolvedSettings = Get-Content -Raw -LiteralPath $cotwSettingsPath |
            ConvertFrom-Json
        $expectedBalance = $CotwProgressionMode -ceq 'balance-fixes'
        if ($resolvedSettings.balance_fixes -isnot [bool] -or
            [bool]$resolvedSettings.balance_fixes -ne $expectedBalance) {
            throw "Staged CotW balance_fixes did not resolve to the requested mode."
        }
        $cotwSettingsStagedAfterSha = (Get-FileHash -LiteralPath `
            $cotwSettingsPath -Algorithm SHA256).Hash
    }
    if ($favoredTraitsMode -cne 'unchanged') {
        if (-not (Test-Path -LiteralPath $favoredSettingsPath -PathType Leaf)) {
            throw "Staged Favored Class settings file is missing: $favoredSettingsPath"
        }
        $favoredSettingsStagedBeforeSha = (Get-FileHash -LiteralPath `
            $favoredSettingsPath -Algorithm SHA256).Hash
        $settingsText = [IO.File]::ReadAllText($favoredSettingsPath)
        $traitsPattern = '("enable_traits"\s*:\s*)(true|false)'
        $traitsMatches = [regex]::Matches($settingsText, $traitsPattern,
            [Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($traitsMatches.Count -ne 1) {
            throw 'Staged Favored Class settings must contain exactly one enable_traits Boolean.'
        }
        $desiredTraits = if ($favoredTraitsMode -ceq 'enabled') {
            'true'
        } else { 'false' }
        $match = $traitsMatches[0]
        $replacement = $match.Groups[1].Value + $desiredTraits
        $updated = $settingsText.Substring(0, $match.Index) + $replacement +
            $settingsText.Substring($match.Index + $match.Length)
        $temporaryFavored = $favoredSettingsPath + '.kmg-traits.tmp'
        [IO.File]::WriteAllText($temporaryFavored, $updated,
            (New-Object Text.UTF8Encoding($false)))
        Move-Item -LiteralPath $temporaryFavored -Destination `
            $favoredSettingsPath -Force
        $resolvedSettings = Get-Content -Raw -LiteralPath `
            $favoredSettingsPath | ConvertFrom-Json
        $expectedTraits = $favoredTraitsMode -ceq 'enabled'
        if ($resolvedSettings.enable_traits -isnot [bool] -or
            [bool]$resolvedSettings.enable_traits -ne $expectedTraits) {
            throw 'Staged Favored Class enable_traits did not resolve to the requested mode.'
        }
        $favoredSettingsStagedAfterSha = (Get-FileHash -LiteralPath `
            $favoredSettingsPath -Algorithm SHA256).Hash
    }
    if ($moduleScenario) {
        $settingsPath = Join-Path $KingmakerInstallDir `
            'Mods\KingmakerGunslinger\FeatureModules.json'
        $settings = [ordered]@{ schemaVersion = 8
            gunslinger = [bool]$Parameters.gunslinger
            'acadamae-graduate' = [bool]$Parameters.acadamaeGraduate
            'shield-other' = [bool]$Parameters.shieldOther
            'expanded-summoning' = [bool]$Parameters.expandedSummoning
            'elven-branched-spears' = [bool]$Parameters.elvenBranchedSpears
            'eastern-weapons' = [bool]$Parameters.easternWeapons
            'brown-fur-transmuter' = [bool]$Parameters.brownFurTransmuter
            'urban-barbarian' = [bool]$Parameters.urbanBarbarian
            'bodyguard-feats' = [bool]$Parameters.bodyguardFeats }
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
            ExpectedVersion = '0.0.93'
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
        if ($ReuseInstalledArtifact) {
            if ([string]::IsNullOrWhiteSpace($DeploymentManifestPath) -or
                [string]::IsNullOrWhiteSpace($PackagePath)) {
                throw '-ReuseInstalledArtifact requires deployment and package paths.'
            }
            $arguments.ReuseInstalledArtifact = $true
            $arguments.DeploymentManifestPath = $DeploymentManifestPath
            $arguments.PackagePath = $PackagePath
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
$cotwSettingsRestoredExists = Test-Path -LiteralPath $cotwSettingsPath `
    -PathType Leaf
$cotwSettingsRestoredSha = if ($cotwSettingsRestoredExists) {
    (Get-FileHash -LiteralPath $cotwSettingsPath -Algorithm SHA256).Hash
} else { $null }
$cotwSettingsBytesRestored =
    $cotwSettingsOriginalExists -eq $cotwSettingsRestoredExists
if ($cotwSettingsBytesRestored -and $cotwSettingsOriginalExists) {
    $restoredBytes = [IO.File]::ReadAllBytes($cotwSettingsPath)
    $cotwSettingsBytesRestored =
        $restoredBytes.Length -eq $cotwSettingsOriginalBytes.Length
    if ($cotwSettingsBytesRestored) {
        for ($index = 0; $index -lt $restoredBytes.Length; $index++) {
            if ($restoredBytes[$index] -ne $cotwSettingsOriginalBytes[$index]) {
                $cotwSettingsBytesRestored = $false
                break
            }
        }
    }
}
$cotwSettingsEvidencePath = Join-Path $StateRoot `
    "$runId\cotw-settings-profile.json"
[ordered]@{
    schemaVersion = 1
    progressionMode = $CotwProgressionMode
    originalExisted = $cotwSettingsOriginalExists
    originalSha256 = $cotwSettingsOriginalSha
    stagedBeforeSha256 = $cotwSettingsStagedBeforeSha
    stagedAfterSha256 = $cotwSettingsStagedAfterSha
    restoredExisted = $cotwSettingsRestoredExists
    restoredSha256 = $cotwSettingsRestoredSha
    exactBytesRestored = $cotwSettingsBytesRestored
} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath `
    $cotwSettingsEvidencePath -Encoding UTF8
if (-not $cotwSettingsBytesRestored) {
    throw "CotW settings bytes were not restored exactly: $runId"
}
$favoredSettingsEvidencePath = Join-Path $StateRoot `
    "$runId\favored-settings-profile.json"
[ordered]@{
    schemaVersion = 1
    traitsMode = $favoredTraitsMode
    stagedBeforeSha256 = $favoredSettingsStagedBeforeSha
    stagedAfterSha256 = $favoredSettingsStagedAfterSha
    completeModsRestorationVerified = [bool]$state.restorationVerified
} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath `
    $favoredSettingsEvidencePath -Encoding UTF8
[pscustomobject][ordered]@{
    profileId = $ProfileId
    transactionRunId = $runId
    restorationVerified = $true
    stagedMutationObserved = [bool]$state.stagedMutationObserved
    cotwProgressionMode = $CotwProgressionMode
    cotwSettingsStagedSha256 = $cotwSettingsStagedAfterSha
    cotwSettingsRestoredSha256 = $cotwSettingsRestoredSha
    cotwSettingsBytesRestored = $cotwSettingsBytesRestored
    cotwSettingsEvidencePath = $cotwSettingsEvidencePath
    favoredTraitsMode = $favoredTraitsMode
    favoredSettingsStagedBeforeSha256 = $favoredSettingsStagedBeforeSha
    favoredSettingsStagedAfterSha256 = $favoredSettingsStagedAfterSha
    favoredSettingsEvidencePath = $favoredSettingsEvidencePath
    runtimeResults = @($results)
}
