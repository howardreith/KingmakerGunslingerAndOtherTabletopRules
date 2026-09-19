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
if ($failures.Count -gt 0) { throw ($failures -join ', ') }
Write-Output 'PASS Magic Circle guarded request tests.'
