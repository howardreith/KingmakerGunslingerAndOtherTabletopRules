[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = if ($RepositoryRoot) {
    (Resolve-Path -LiteralPath $RepositoryRoot).Path
} else {
    Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
}
$validator = Join-Path $repositoryRoot 'tools\validate_repository.py'
if (-not (Test-Path -LiteralPath $validator -PathType Leaf)) {
    throw "Version-aware repository validator is missing: $validator"
}

$python = Get-Command python -ErrorAction SilentlyContinue
if ($null -eq $python) {
    $python = Get-Command python3 -ErrorAction SilentlyContinue
}
if ($null -eq $python) {
    throw 'Python 3 is required to run tools\validate_repository.py.'
}

& $python.Source (Join-Path $repositoryRoot 'tools\test_repository_validator.py')
if ($LASTEXITCODE -ne 0) { throw 'Repository validator regression fixtures failed.' }
& $python.Source $validator --root $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    throw "Repository validation failed with exit code $LASTEXITCODE."
}

if ((Get-Content -Raw -LiteralPath (Join-Path $repositoryRoot 'Info.json') | ConvertFrom-Json).Version -eq '0.0.146') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_checkpoint146.py')
    if ($LASTEXITCODE -ne 0) { throw 'Checkpoint integration corruption fixtures failed.' }
}

& (Join-Path $PSScriptRoot 'Test-IconOverhaulAssets.ps1') `
    -RepositoryRoot $repositoryRoot
& (Join-Path $PSScriptRoot 'Test-IconPolishRound2Assets.ps1') `
    -RepositoryRoot $repositoryRoot

& $python.Source (Join-Path $repositoryRoot 'tools\validate_icon_catalog.py') --root $repositoryRoot
if ($LASTEXITCODE -ne 0) { throw 'Icon authoring catalog validation failed.' }
& $python.Source (Join-Path $repositoryRoot 'tools\test_icon_catalog.py')
if ($LASTEXITCODE -ne 0) { throw 'Icon catalog corruption fixtures failed.' }
& $python.Source (Join-Path $repositoryRoot 'tools\test_icon_runtime_evidence.py')
if ($LASTEXITCODE -ne 0) { throw 'Icon runtime evidence corruption fixtures failed.' }
& $python.Source (Join-Path $repositoryRoot 'tools\test_native_icon_screens.py')
if ($LASTEXITCODE -ne 0) { throw 'Native icon screenshot corruption fixtures failed.' }
& (Join-Path $PSScriptRoot 'Test-IconCensusControlRequest.ps1')

# The gate level's own contract: a qualification cannot be narrowed by an
# inherited KMG_TEST_FILTER, a focused run restores the caller's
# environment exactly, and the candidate pipeline performs each expensive
# operation once. Offline and cheap; it runs no gate and builds nothing.
& (Join-Path $PSScriptRoot 'Test-KmgGate.ps1')

# Orchestration decisions for the Expanded Summoning runtime batches:
# current-run result selection, stale-result rejection, parse failure,
# scenario failure, restoration failure, interrupted operation, and a
# successful transaction. It launches nothing and writes nothing to the
# installation, so it belongs in the gate that runs every build rather
# than in a script nobody executes.
& (Join-Path $PSScriptRoot 'Test-ExpandedSummoningRuntimeOrchestration.ps1') -ScriptRoot $PSScriptRoot

# Parse the guarded entry points with the installed PowerShell parser. This
# catches encoding and parameter-block faults before deployment or save access.
foreach ($scriptName in @('Invoke-KingmakerRuntimeTest.ps1',
    'RuntimeAutomation.Common.ps1', 'WeaponFindabilityPersistence.Common.ps1',
    'Invoke-WeaponFindabilityPersistenceQualification.ps1')) {
    $tokens = $null
    $parseErrors = $null
    [void][Management.Automation.Language.Parser]::ParseFile(
        (Join-Path $PSScriptRoot $scriptName), [ref]$tokens, [ref]$parseErrors)
    if (@($parseErrors).Count -ne 0) {
        throw "Guarded runtime script does not parse: $scriptName ($($parseErrors.Message -join '; '))"
    }
}

Write-Host 'Version-aware repository validation passed.'
