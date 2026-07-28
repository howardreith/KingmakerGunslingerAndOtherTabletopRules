[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$orchestratorPath = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$deploymentPath = Join-Path $PSScriptRoot 'Deploy-Local.ps1'
$orchestrator = Get-Content -LiteralPath $orchestratorPath -Raw
$deployment = Get-Content -LiteralPath $deploymentPath -Raw

$failures = [Collections.Generic.List[string]]::new()
function Assert-True([bool]$Condition, [string]$Name) {
    if (-not $Condition) { $failures.Add($Name) }
}

$orchestratorBackupCalls = @(
    [regex]::Matches($orchestrator, 'Backup-Live-Mod\.ps1')
)
$deploymentBackupCalls = @(
    [regex]::Matches($deployment, 'Backup-Live-Mod\.ps1')
)
$shouldProcessIndex = $deployment.IndexOf('$PSCmdlet.ShouldProcess(', [StringComparison]::Ordinal)
$dryReturnIndex = $deployment.IndexOf("Write-Host 'Dry run only;", [StringComparison]::Ordinal)
$backupIndex = $deployment.IndexOf(
    "`$backup = & (Join-Path `$PSScriptRoot 'Backup-Live-Mod.ps1')",
    [StringComparison]::Ordinal)
$manifestIndex = $deployment.IndexOf('Read-KmgBuildLocalManifest', [StringComparison]::Ordinal)
$runningIndex = $deployment.IndexOf('Assert-KmgNotRunning', [StringComparison]::Ordinal)
$liveDirectoryIndex = $deployment.IndexOf(
    'Test-Path -LiteralPath $LiveModDirectory',
    [StringComparison]::Ordinal)

Assert-True ($orchestratorBackupCalls.Count -eq 0) 'orchestrator-must-not-back-up'
Assert-True ($deploymentBackupCalls.Count -eq 1) 'deployment-must-own-one-backup'
Assert-True ($manifestIndex -ge 0 -and $manifestIndex -lt $shouldProcessIndex) `
    'package-preflight-before-should-process'
Assert-True ($runningIndex -ge 0 -and $runningIndex -lt $shouldProcessIndex) `
    'process-preflight-before-should-process'
Assert-True ($liveDirectoryIndex -ge 0 -and $liveDirectoryIndex -lt $shouldProcessIndex) `
    'live-directory-preflight-before-should-process'
Assert-True ($shouldProcessIndex -ge 0 -and $dryReturnIndex -gt $shouldProcessIndex) `
    'dry-run-return-is-guarded'
Assert-True ($backupIndex -gt $dryReturnIndex) 'backup-only-after-dry-run-return'
Assert-True (-not $deployment.Substring(
    $shouldProcessIndex,
    $dryReturnIndex - $shouldProcessIndex).Contains('Backup-Live-Mod.ps1')) `
    'dry-run-does-not-call-backup'
Assert-True ($orchestrator.Contains(
    "& (Join-Path `$PSScriptRoot 'Deploy-Local.ps1') -PackagePath `$package -Confirm:`$false")) `
    'real-orchestration-cannot-bypass-deployment-boundary'

if ($failures.Count -ne 0) {
    throw "Runtime deployment safety tests failed: $($failures -join ', ')"
}
Write-Host 'Runtime deployment safety tests passed: 9'
