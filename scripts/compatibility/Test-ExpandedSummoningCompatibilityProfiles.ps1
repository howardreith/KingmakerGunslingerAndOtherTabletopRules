Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
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
    'disposable-expanded-summoning-sprint23-native-review')) {
    $reviewPhases = @('rtwp-foundation', 'turn-based-control')
    if ($reviewScenario -ceq
        'disposable-expanded-summoning-sprint23-native-review') {
        $reviewPhases += @('charge-rounding-rtwp',
            'charge-rounding-turn-based')
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
    @{ Scenario = @('disposable-expanded-summoning-sprint23-native-review', 'mod-load-smoke'); Parameters = @{ reviewPhase = 'rtwp-foundation' }; Classified = $true }
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

Write-Host "Expanded Summoning compatibility profile tests passed: $($required.Count) profiles; Sprint 22/23 phase boundary passed."
