[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$guide = Get-Content -Raw -LiteralPath (Join-Path $root `
    'INSTALLATION-COMPATIBILITY.md')
$readme = Get-Content -Raw -LiteralPath (Join-Path $root 'README.md')
$package = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\package.ps1')
$validator = Get-Content -Raw -LiteralPath (Join-Path $root `
    'scripts\validate-package.ps1')
$deterministic = Get-Content -Raw -LiteralPath (Join-Path $root `
    'tools\create_deterministic_package.py')

$checks = [ordered]@{
    'install-and-version-verification' =
        $guide.Contains('Unity Mod Manager') -and
        $guide.Contains('verify that Unity Mod Manager reports')
    'update-backup-and-complete-replacement' =
        $guide.Contains('Back up affected saves') -and
        $guide.Contains('replace the complete `KingmakerGunslinger` folder')
    'removal-fails-safe' =
        $guide.Contains('There is no uninstall cleanup') -and
        $guide.Contains('Never test removal against the only copy')
    'compatibility-claims-bounded' =
        $guide.Contains('neither dependencies nor') -and
        $guide.Contains('qualified compatibility targets') -and
        $guide.Contains('Mods changing the same callbacks') -and
        $guide.Contains('conflict depending on patch order')
    'readme-links-warning' =
        $readme.Contains('INSTALLATION-COMPATIBILITY.md') -and
        $readme.Contains('no uninstall-safe-save claim')
    'strict-package-includes-guide' =
        $package.Contains("'INSTALLATION-COMPATIBILITY.md'") -and
        $validator.Contains("'INSTALLATION-COMPATIBILITY.md'") -and
        $validator.Contains('strict nine-file allowlist') -and
        $deterministic.Contains('if len(files) != 9') -and
        $deterministic.Contains('Expected exactly nine staged package files')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Sprint 67 release-documentation tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 67 release-documentation tests passed: $($checks.Count)"
