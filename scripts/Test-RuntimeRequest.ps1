[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $failures.Add($Name) } catch { }
}

$synthetic = Join-Path $script:KmgRuntimeEvidenceRoot 'source-only-request-test'
$request = New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.30' `
    -TimeoutSeconds 30 -ExitAfterCompletion $true -EvidenceDirectory $synthetic
if ($request.schemaVersion -ne 1 -or -not $request.enabled -or
    $request.scenario -ne 'mod-load-smoke' -or $request.parameters.Count -ne 0 -or
    $request.startupTimeoutSeconds -ne 180) {
    $failures.Add('valid-request-schema')
}
Assert-Throws { New-KmgRuntimeRequest -Scenario 'unknown' -ExpectedVersion '0.0.30' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic } 'unknown-scenario'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic } 'empty-version'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.30' `
    -TimeoutSeconds 4 -ExitAfterCompletion $false -EvidenceDirectory $synthetic } 'short-timeout'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.30' `
    -TimeoutSeconds 30 -StartupTimeoutSeconds 4 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic } 'short-startup-timeout'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.30' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory 'C:\Windows\Temp' } 'outside-root'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.30' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic `
    -Parameters @{ unexpected = $true } } 'unknown-parameter'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'working-save-smoke' -ExpectedVersion '0.0.30' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic `
    -Parameters @{} } 'working-save-name-missing'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'working-save-smoke' -ExpectedVersion '0.0.30' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic `
    -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' } } 'baseline-forbidden'

if ($failures.Count -ne 0) { throw "Runtime request tests failed: $($failures -join ', ')" }
Write-Host 'Runtime request source tests passed: 9'
