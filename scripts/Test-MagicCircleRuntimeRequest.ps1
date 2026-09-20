[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $failures.Add($Name) } catch { }
}

$version = (Get-Content -Raw (Join-Path $PSScriptRoot '../Info.json') | ConvertFrom-Json).Version
$synthetic = Join-Path $script:KmgRuntimeEvidenceRoot 'magic-circle-request-test'
$circleAudit = New-KmgRuntimeRequest -Scenario 'observe-magic-circle-native-contracts' `
    -ExpectedVersion $version -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($circleAudit.parameters.Count -ne 0 -or
    (Get-KmgRuntimeScenarioMetadata -Scenario $circleAudit.scenario).RequiresSaveName) {
    $failures.Add('magic-circle-audit-is-read-only-save-free')
}
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'observe-magic-circle-native-contracts' `
        -ExpectedVersion $version -TimeoutSeconds 30 -ExitAfterCompletion $true `
        -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' }
} 'magic-circle-audit-rejects-save-parameters'
$workingTimeouts = @{
    CatalogTimeoutSeconds = 30; SelectionTimeoutSeconds = 30
    CompletionTimeoutSeconds = 30; MainMenuTimeoutSeconds = 30
    ActionResolutionTimeoutSeconds = 30; ActionInvocationTimeoutSeconds = 30
    DescriptorResolutionTimeoutSeconds = 30; LoadEntryTimeoutSeconds = 30
    FingerprintTimeoutSeconds = 30
}
$native = New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-evil' @workingTimeouts `
    -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' }
if ($native.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    $failures.Add('magic-circle-native-exact-working-save')
}
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-evil' @workingTimeouts `
        -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
        -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' }
} 'magic-circle-native-rejects-baseline'
$native = New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-ui' @workingTimeouts `
    -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' }
if ($native.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    $failures.Add('magic-circle-ui-exact-working-save')
}
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-ui' @workingTimeouts `
        -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
        -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' }
} 'magic-circle-ui-rejects-baseline'
foreach ($phase in @('prepare', 'verify', 'cleanup', 'absent', 'scene')) {
    $request = New-KmgRuntimeRequest -Scenario "working-save-magic-circle-$phase" @workingTimeouts `
        -ExpectedVersion $version -TimeoutSeconds 180 -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' } `
        -EvidenceDirectory $synthetic -ExitAfterCompletion:$true
    if ($request.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
        $failures.Add("magic-circle-persistence-$phase-exact-save")
    }
    Assert-Throws {
        New-KmgRuntimeRequest -Scenario "working-save-magic-circle-$phase" @workingTimeouts `
            -ExpectedVersion $version -TimeoutSeconds 180 -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' } `
            -EvidenceDirectory $synthetic -ExitAfterCompletion:$true
    } "magic-circle-persistence-$phase-rejects-baseline"
}

if ($failures.Count -gt 0) { throw ($failures -join ', ') }
Write-Output 'PASS Magic Circle guarded request tests.'
