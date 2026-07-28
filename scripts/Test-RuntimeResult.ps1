[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$runtimeRoot = Join-Path (Split-Path $PSScriptRoot -Parent) `
    'src\KingmakerGunslinger\RuntimeTesting'
$source = (Get-Content -LiteralPath (Join-Path $runtimeRoot 'RuntimeTestResult.cs') -Raw) +
    (Get-Content -LiteralPath (Join-Path $runtimeRoot 'RuntimeTestRunner.cs') -Raw)
$required = @(
    'PASS', 'FAIL', 'AMBIGUOUS', 'ERROR', 'TIMEOUT',
    'runtime-result.json', 'runtime-summary.txt',
    'FileMode.CreateNew', 'Flush(true)', 'File.Move'
)
$missing = @($required | Where-Object { $source.IndexOf($_, [StringComparison]::Ordinal) -lt 0 })
if ($missing.Count -ne 0) {
    throw "Runtime result contract tokens are missing: $($missing -join ', ')"
}

$terminalFailure = @('FAIL', 'AMBIGUOUS', 'ERROR', 'TIMEOUT')
if (@($terminalFailure | Where-Object { $_ -eq 'PASS' }).Count -ne 0) {
    throw 'PASS was incorrectly classified as a failure.'
}
Write-Host 'Runtime result source tests passed: 2'
