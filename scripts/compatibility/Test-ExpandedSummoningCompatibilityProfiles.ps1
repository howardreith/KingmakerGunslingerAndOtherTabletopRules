Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
. (Join-Path $root 'scripts/ExpandedSummoningOrchestration.Common.ps1')
$invoke = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot `
    'Invoke-KingmakerCompatibilityProfile.ps1')
$tokens = $null
$parseErrors = $null
$invokePath = Join-Path $PSScriptRoot 'Invoke-KingmakerCompatibilityProfile.ps1'
$invokeAst = [Management.Automation.Language.Parser]::ParseFile(
    $invokePath, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -ne 0) {
    throw 'Expanded Summoning compatibility runner has parse errors.'
}
$profiles = Get-Content -Raw -LiteralPath (Join-Path $root `
    'compatibility\profiles.json') | ConvertFrom-Json

$required = @(
    'gunslinger-only',
    'gunslinger-call-of-the-wild',
    'gunslinger-arms-armor',
    'gunslinger-toggle-custom-soundpacks',
    'gunslinger-high-risk-combined'
)
$ids = @($profiles.profiles | ForEach-Object id)
$missing = @($required | Where-Object { $_ -cnotin $ids })
if ($missing.Count -ne 0) {
    throw "Expanded Summoning compatibility profiles are missing: $($missing -join ', ')"
}
if (-not $invoke.Contains("'observe-expanded-summoning-inventory'")) {
    throw 'Expanded Summoning structural runtime scenario is not allowlisted for compatibility profiles.'
}
if (-not $invoke.Contains('restorationVerified') -or
    -not $invoke.Contains('Restore-KingmakerCompatibilityProfile.ps1')) {
    throw 'Compatibility invocation no longer requires exact transaction restoration.'
}

$phaseAssignments = @($invokeAst.FindAll({ param($node)
    $node -is [Management.Automation.Language.AssignmentStatementAst] -and
    $node.Left.Extent.Text -ceq '$sprint22ReviewScenario'
}, $true))
if ($phaseAssignments.Count -ne 1) {
    throw 'Sprint 22 compatibility scenario classifier changed.'
}
$classifyPhase = [scriptblock]::Create(
    $phaseAssignments[0].Extent.Text + "`nreturn `$sprint22ReviewScenario")
$phaseGuards = @($invokeAst.FindAll({ param($node)
    $node -is [Management.Automation.Language.IfStatementAst] -and
    @($node.Clauses | Where-Object {
        $_.Item1.Extent.Text -ceq '$sprint22ReviewScenario'
    }).Count -eq 1
}, $true))
if ($phaseGuards.Count -ne 1) {
    throw 'Sprint 22 compatibility parameter guard changed.'
}
$phaseClause = @($phaseGuards[0].Clauses | Where-Object {
    $_.Item1.Extent.Text -ceq '$sprint22ReviewScenario'
})[0]
$validatePhase = [scriptblock]::Create(($phaseClause.Item2.Statements |
    ForEach-Object { $_.Extent.Text }) -join "`n")
foreach ($reviewScenario in @(
    'disposable-expanded-summoning-sprint22-native-review',
    'disposable-expanded-summoning-sprint23-native-review',
    'disposable-expanded-summoning-sprint24-native-review')) {
    $reviewPhases = @('rtwp-foundation', 'turn-based-control')
    if ($reviewScenario -ceq
        'disposable-expanded-summoning-sprint23-native-review') {
        $reviewPhases += @('charge-rounding-rtwp',
            'charge-rounding-turn-based')
    }
    if ($reviewScenario -ceq
        'disposable-expanded-summoning-sprint24-native-review') {
        $reviewPhases = @('rtwp-foundation', 'rtwp-deinonychus',
            'turn-based-deinonychus', 'rtwp-tyrannosaurus',
            'turn-based-tyrannosaurus', 'visual-lifecycle',
            'shared-controls')
    }
    foreach ($phase in $reviewPhases) {
        $Scenario = @($reviewScenario)
        $Parameters = @{ reviewPhase = $phase }
        if (-not (& $classifyPhase)) {
            throw "Expanded Summoning compatibility classifier rejected $reviewScenario/$phase."
        }
        & $validatePhase
    }
}
$invalidPhaseCases = @(
    @{ Scenario = @('mod-load-smoke'); Parameters = @{}; Classified = $false },
    @{ Scenario = @('disposable-expanded-summoning-sprint22-native-review'); Parameters = @{}; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint22-native-review'); Parameters = @{ reviewPhase = 'all' }; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint22-native-review'); Parameters = @{ reviewPhase = 'charge-rounding-rtwp' }; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint22-native-review'); Parameters = @{ reviewPhase = 'RTWP-foundation' }; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint22-native-review'); Parameters = @{ reviewPhase = 'rtwp-foundation'; saveName = 'KMG_AUTOMATION_WORKING' }; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint22-native-review', 'mod-load-smoke'); Parameters = @{ reviewPhase = 'rtwp-foundation' }; Classified = $true }
    @{ Scenario = @('disposable-expanded-summoning-sprint23-native-review'); Parameters = @{}; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint23-native-review'); Parameters = @{ reviewPhase = 'all' }; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint23-native-review', 'mod-load-smoke'); Parameters = @{ reviewPhase = 'rtwp-foundation' }; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint24-native-review'); Parameters = @{}; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint24-native-review'); Parameters = @{ reviewPhase = 'all' }; Classified = $true },
    @{ Scenario = @('disposable-expanded-summoning-sprint24-native-review', 'mod-load-smoke'); Parameters = @{ reviewPhase = 'rtwp-foundation' }; Classified = $true }
)
foreach ($case in $invalidPhaseCases) {
    $Scenario = $case.Scenario
    $Parameters = $case.Parameters
    $classified = & $classifyPhase
    if ($classified -ne $case.Classified) {
        throw 'Sprint 22 compatibility scenario classification was not exact.'
    }
    if ($classified) {
        $rejected = $false
        try { & $validatePhase } catch { $rejected = $true }
        if (-not $rejected) {
            throw 'Invalid Sprint 22 compatibility phase parameters were accepted.'
        }
    }
}

# Execute the actual two route assignments: inventory must take both the
# independent settings rollback and exact launcher-receipt path, not recency.
foreach ($assignment in @('$sprint22Boundary', '$exactSprint22')) {
    $matches=@($invokeAst.FindAll({param($node)
        $node -is [Management.Automation.Language.AssignmentStatementAst] -and
        $node.Left.Extent.Text -ceq $assignment
    },$true))
    if($matches.Count -ne 1){throw 'Exact settings/receipt assignment is ambiguous.'}
    $classify=[scriptblock]::Create($matches[0].Extent.Text+"`nreturn "+$assignment)
    foreach($scenarioName in @('observe-expanded-summoning-inventory',
        'disposable-expanded-summoning-sprint22-native-review',
        'disposable-expanded-summoning-sprint23-native-review',
        'disposable-expanded-summoning-sprint24-native-review',
        'disposable-expanded-summoning-sprint25-native-review')) {
        $Scenario=@($scenarioName);$name=$scenarioName
        if(-not (& $classify)){throw 'Inventory/review escaped exact settings/receipt protection.'}
    }
    foreach($scenarioName in @('', 'mod-load-smoke',
        'OBSERVE-EXPANDED-SUMMONING-INVENTORY', 'observe-expanded-summoning-inventory-extra')) {
        $Scenario=@($scenarioName);$name=$scenarioName
        if(& $classify){throw 'Unrelated or malformed scenario entered this exact boundary.'}
    }
}
Write-Host "Expanded Summoning compatibility profile tests passed: $($required.Count) profiles; phase guards and18 exact settings/receipt classifier checks."

# Exercise the actual entry point in its own script scope. A deliberately wrong
# package version ends valid requests at the candidate boundary, before any
# ShouldProcess/deployment/lease/launch; this also detects missing helper loads
# that evaluating an extracted guard in this test's scope could conceal.
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
$dispatchRoot = Join-Path $root ('artifacts/fixtures/sprint26-profile-dispatch-' + [Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($dispatchRoot)
$dispatchPackage = Join-Path $dispatchRoot 'wrong-version.zip'
$zip = [IO.Compression.ZipFile]::Open($dispatchPackage, [IO.Compression.ZipArchiveMode]::Create)
try {
    $entry = $zip.CreateEntry('KingmakerGunslinger/Info.json')
    $writer = [IO.StreamWriter]::new($entry.Open())
    try { $writer.Write('{"Id":"KingmakerGunslinger","Version":"0.0.0"}') } finally { $writer.Dispose() }
} finally { $zip.Dispose() }
$dispatchScenario = 'disposable-expanded-summoning-sprint26-native-review'
$dispatchChecks = 0
foreach ($phase in @('rtwp-foundation', 'turn-based-commands',
    'gorgon-breath-rtwp', 'gorgon-breath-turn-based',
    'bulette-leap-rtwp', 'bulette-leap-turn-based', 'rtwp-ai', 'routes', 'visual-lifecycle')) {
    $caught = $null
    try { & $invokePath -ProfileId gunslinger-only -Scenario $dispatchScenario -Parameters @{reviewPhase=$phase} -ReuseInstalledArtifact -PackagePath $dispatchPackage -WhatIf -Confirm:$false }
    catch { $caught = $_.Exception.Message }
    if ($caught -cne 'Compatibility package version does not match the current repository; no profile was entered.') {
        throw "Actual Sprint26 profile dispatch failed before candidate boundary for ${phase}: $caught"
    }
    $dispatchChecks++
}
foreach ($parameters in @(@{}, @{reviewPhase='RTWP-FOUNDATION'}, @{reviewPhase='overrun'},
    @{reviewPhase=@('rtwp-foundation')}, @{reviewPhase='rtwp-foundation';extra='no'})) {
    $caught = $null
    try { & $invokePath -ProfileId gunslinger-only -Scenario $dispatchScenario -Parameters $parameters -ReuseInstalledArtifact -PackagePath $dispatchPackage -WhatIf -Confirm:$false }
    catch { $caught = $_.Exception.Message }
    if ($caught -cne 'Sprint 22/23/24/25 native review requires one fresh scenario and exactly one allowlisted reviewPhase parameter.') {
        throw "Actual Sprint26 profile dispatch did not reject invalid parameters: $caught"
    }
    $dispatchChecks++
}
Write-Host "PASS actual Sprint26 profile dispatch: $dispatchChecks candidate-boundary/refusal checks; no profile entered."
$dispatchScenario = 'disposable-expanded-summoning-sprint28-native-review'
$sprint28DispatchChecks = 0
foreach ($phase in @('profiles', 'modes', 'commands', 'ownership', 'catch')) {
    $caught = $null
    try { & $invokePath -ProfileId gunslinger-only -Scenario $dispatchScenario -Parameters @{reviewPhase=$phase} -ReuseInstalledArtifact -PackagePath $dispatchPackage -WhatIf -Confirm:$false }
    catch { $caught = $_.Exception.Message }
    if ($caught -cne 'Compatibility package version does not match the current repository; no profile was entered.') {
        throw "Actual Sprint28 profile dispatch failed before candidate boundary for ${phase}: $caught"
    }
    $sprint28DispatchChecks++
}
foreach ($parameters in @(@{}, @{reviewPhase='COMMANDS'}, @{reviewPhase='all'},
    @{reviewPhase=@('commands')}, @{reviewPhase='commands';saveName='KMG_AUTOMATION_WORKING'})) {
    $caught = $null
    try { & $invokePath -ProfileId gunslinger-only -Scenario $dispatchScenario -Parameters $parameters -ReuseInstalledArtifact -PackagePath $dispatchPackage -WhatIf -Confirm:$false }
    catch { $caught = $_.Exception.Message }
    if ($caught -cne 'Sprint 22/23/24/25 native review requires one fresh scenario and exactly one allowlisted reviewPhase parameter.') {
        throw "Actual Sprint28 profile dispatch did not reject invalid parameters: $caught"
    }
    $sprint28DispatchChecks++
}
Write-Host "PASS actual Sprint28 profile dispatch: $sprint28DispatchChecks candidate-boundary/refusal checks; no profile entered."
