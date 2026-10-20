[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    # Validates a development candidate for this one named purpose instead of
    # the active version's release. It replaces only the release validator
    # dispatch below; every fixture and offline contract still runs, and the
    # result never qualifies a release.
    [string]$DevelopmentPurpose,
    [string]$DevelopmentSummaryPath
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
& $python.Source (Join-Path $repositoryRoot 'tools\test_development_candidate_validator.py')
if ($LASTEXITCODE -ne 0) { throw 'Development-candidate validator corruption fixtures failed.' }
if ([string]::IsNullOrEmpty($DevelopmentPurpose)) {
    if ($DevelopmentSummaryPath) { throw '-DevelopmentSummaryPath requires -DevelopmentPurpose.' }
    & $python.Source $validator --root $repositoryRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Repository validation failed with exit code $LASTEXITCODE."
    }
} else {
    $developmentArguments = @(
        (Join-Path $repositoryRoot 'tools\validate_development_candidate.py'),
        '--root', $repositoryRoot, '--purpose', $DevelopmentPurpose)
    if ($DevelopmentSummaryPath) { $developmentArguments += @('--summary', $DevelopmentSummaryPath) }
    & $python.Source @developmentArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Development-candidate source validation failed with exit code $LASTEXITCODE."
    }
}

$activeVersion = (Get-Content -Raw -LiteralPath (Join-Path $repositoryRoot 'Info.json') | ConvertFrom-Json).Version
if ($activeVersion -eq '0.0.146') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_checkpoint146.py')
    if ($LASTEXITCODE -ne 0) { throw 'Checkpoint integration corruption fixtures failed.' }
}
if ($activeVersion -eq '0.0.147') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint18147.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 18 corruption fixtures failed.' }
}
if ($activeVersion -eq '0.0.148') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint19148.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 19 corruption fixtures failed.' }
}
if ($activeVersion -eq '0.0.149') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint20149.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 20 corruption fixtures failed.' }
}
if ($activeVersion -eq '0.0.150') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint21150.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 21 corruption fixtures failed.' }
}
if ($activeVersion -eq '0.0.151') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint22151.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 22 corruption fixtures failed.' }
}
if ($activeVersion -in @('0.0.152', '0.0.153', '0.0.154', '0.0.155', '0.0.156', '0.0.157', '0.0.158', '0.0.159', '0.0.160', '0.0.161')) {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint23152.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 23 corruption fixtures failed.' }
}
if ($activeVersion -in @('0.0.153', '0.0.154', '0.0.155', '0.0.156', '0.0.157', '0.0.158', '0.0.159', '0.0.160', '0.0.161')) {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint24154.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 24 corruption fixtures failed.' }
}
if ($activeVersion -eq '0.0.156') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_firearm_continuation_release156.py')
    if ($LASTEXITCODE -ne 0) { throw 'Firearm continuation patch corruption fixtures failed.' }
}
if ($activeVersion -eq '0.0.157') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_alternate_summoning_release157.py')
    if ($LASTEXITCODE -ne 0) { throw 'Alternate summoning release corruption fixtures failed.' }
}
if ($activeVersion -eq '0.0.158') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_visual_refinement_release158.py')
    if ($LASTEXITCODE -ne 0) { throw 'Visual refinement release corruption fixtures failed.' }
}

if ($activeVersion -eq '0.0.159') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint25158.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 25 corruption fixtures failed.' }
}

if ($activeVersion -eq '0.0.160') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint26160.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 26 corruption fixtures failed.' }
}

if ($activeVersion -eq '0.0.161') {
    & $python.Source (Join-Path $repositoryRoot 'tools\test_expanded_summoning_sprint27161.py')
    if ($LASTEXITCODE -ne 0) { throw 'Sprint 27 corruption fixtures failed.' }
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
& (Join-Path $PSScriptRoot 'Test-ExplicitCandidateReuse.ps1')
& (Join-Path $PSScriptRoot 'Test-DevelopmentCandidate.ps1')
& (Join-Path $PSScriptRoot 'Test-Sprint22Release.ps1')
& (Join-Path $PSScriptRoot 'Test-Sprint24Release.ps1')
& (Join-Path $PSScriptRoot 'Test-Sprint25Release.ps1')
& (Join-Path $PSScriptRoot 'Test-Sprint26Release.ps1')
& (Join-Path $PSScriptRoot 'Test-Sprint27Release.ps1')
& (Join-Path $PSScriptRoot 'Test-GriffonDevelopmentPersistence.ps1')
& (Join-Path $PSScriptRoot 'Test-Sprint28DevelopmentPersistence.ps1')
& (Join-Path $PSScriptRoot 'Test-Sprint24ReportingLineage.ps1')
# The push wrapper lives outside this repository and had no behaviour test
# until the Sprint 25 audit. It reports SKIP when the wrapper is absent, so a
# clean clone elsewhere still validates.
& (Join-Path $PSScriptRoot 'Test-PushPolicyWrapper.ps1')
# The version-specific child dispatch above once nested the 0.0.157 block
# inside the 0.0.156 block, so a passing run did not establish that the
# Sprint 25 fixtures had run at all. This executes the real dispatch
# statements against a recorded child shim; it builds and launches nothing.
& (Join-Path $PSScriptRoot 'Test-RepositoryValidatorDispatch.ps1')
# The ported preflight has to be proved through the real gate dispatch, not
# by a filename search: the gate must reach the child exactly once, a failing
# child must stop everything downstream, and a scenario registered in only
# one registry must be refused before any installation or save is touched.
& (Join-Path $PSScriptRoot 'Test-GatePreflightIntegration.ps1')
& (Join-Path $PSScriptRoot 'Test-Sprint22InstalledStack.ps1')

# Parse the guarded entry points with the installed PowerShell parser. This
# catches encoding and parameter-block faults before deployment or save access.
foreach ($scriptName in @('Invoke-KingmakerRuntimeTest.ps1',
    'RuntimeAutomation.Common.ps1', 'WeaponFindabilityPersistence.Common.ps1',
    'Invoke-WeaponFindabilityPersistenceQualification.ps1',
    'Invoke-Sprint22InstalledStack.ps1', 'Invoke-ExpandedSummoningRuntimeScenario.ps1')) {
    $tokens = $null
    $parseErrors = $null
    [void][Management.Automation.Language.Parser]::ParseFile(
        (Join-Path $PSScriptRoot $scriptName), [ref]$tokens, [ref]$parseErrors)
    if (@($parseErrors).Count -ne 0) {
        throw "Guarded runtime script does not parse: $scriptName ($($parseErrors.Message -join '; '))"
    }
}

if ([string]::IsNullOrEmpty($DevelopmentPurpose)) {
    Write-Host 'Version-aware repository validation passed.'
} else {
    Write-Host "Development-candidate repository validation passed for '$DevelopmentPurpose'. This is not a release qualification."
}
