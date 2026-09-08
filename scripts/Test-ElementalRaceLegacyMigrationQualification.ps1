[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$package = Join-Path $root `
    'artifacts\release\0.0.114\KingmakerGunslinger-0.0.114-elemental-races.zip'
$releasePath = Join-Path $root `
    'artifacts\release\0.0.114\release-manifest.json'
$expectedCommit = '6874dc15a27ded132456dbdd480f47c794543a05'
$expectedPackageSha =
    'b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694'
$expectedDllSha =
    '09af96b95e2abfa39e45f30c8ccb4cb1e8772981dd3be17846f07cbbd2dd8262'
$expectedMvid = 'dcd73856-39d4-40ce-9b05-77bf249103d7'

foreach ($path in @($package, $releasePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required historical qualification input is missing: $path"
    }
}
$release = Get-Content -LiteralPath $releasePath -Raw | ConvertFrom-Json
if ($release.schemaVersion -ne 1 -or
    $release.generator -cne 'scripts/Publish-Release.ps1' -or
    $release.version -cne '0.0.114' -or
    $release.commit -cne $expectedCommit -or
    $release.packageSha256 -cne $expectedPackageSha -or
    $release.dllSha256 -cne $expectedDllSha -or
    $release.packageValidated -ne $true -or
    (Get-KmgSha256 -Path $package) -cne $expectedPackageSha) {
    throw 'Historical 0.0.114 release provenance is not exact.'
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($package)
try {
    $entries = @($archive.Entries)
    $unsafe = @($entries | Where-Object {
        [string]::IsNullOrEmpty($_.Name) -or
            -not $_.FullName.StartsWith('KingmakerGunslinger/',
                [StringComparison]::Ordinal) -or
            $_.FullName.Contains('..') -or $_.FullName.Contains('\')
    })
    $dllEntries = @($entries | Where-Object {
        $_.FullName -ceq 'KingmakerGunslinger/KingmakerGunslinger.dll'
    })
    if ($entries.Count -ne 135 -or $unsafe.Count -ne 0 -or
        $dllEntries.Count -ne 1) {
        throw 'Historical archive catalog is not the exact safe 135-file package.'
    }
    $stream = $dllEntries[0].Open()
    try {
        $memory = New-Object IO.MemoryStream
        try {
            $stream.CopyTo($memory)
            $bytes = $memory.ToArray()
        }
        finally { $memory.Dispose() }
    }
    finally { $stream.Dispose() }
}
finally { $archive.Dispose() }

$sha = [Security.Cryptography.SHA256]::Create()
try {
    $dllSha = ([BitConverter]::ToString($sha.ComputeHash($bytes)) `
        -replace '-', '').ToLowerInvariant()
}
finally { $sha.Dispose() }
$mvid = ([Reflection.Assembly]::Load(
        $bytes).ManifestModule.ModuleVersionId).ToString('D')
if ($dllSha -cne $expectedDllSha -or $mvid -cne $expectedMvid) {
    throw 'Historical package DLL SHA-256 or MVID is not exact.'
}

$scriptPaths = @(
    (Join-Path $PSScriptRoot 'Deploy-QualifiedElementalRaces114.ps1'),
    (Join-Path $PSScriptRoot `
        'Invoke-ElementalRaceLegacyMigrationQualification.ps1'),
    (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'),
    (Join-Path $PSScriptRoot 'Collect-Runtime-Evidence.ps1')
)
foreach ($scriptPath in $scriptPaths) {
    $tokens = $null
    $errors = $null
    [void][Management.Automation.Language.Parser]::ParseFile(
        $scriptPath, [ref]$tokens, [ref]$errors)
    if (@($errors).Count -ne 0) {
        throw "PowerShell parser rejected $scriptPath`: $($errors.Message -join '; ')"
    }
}

$transaction = Get-Content -LiteralPath $scriptPaths[1] -Raw
$old = $transaction.IndexOf(
    '$legacyDeploymentManifestPath = & $deployLegacy',
    [StringComparison]::Ordinal)
$prepare = $transaction.IndexOf(
    "-Scenario 'elemental-race-persistence-prepare'", $old,
    [StringComparison]::Ordinal)
$current = $transaction.IndexOf(
    '$migrationDeploymentManifestPath = & $deployCurrent', $prepare,
    [StringComparison]::Ordinal)
$migration = $transaction.IndexOf(
    "-Scenario 'elemental-race-legacy-migration'", $current,
    [StringComparison]::Ordinal)
$absence = $transaction.IndexOf(
    "-Scenario 'elemental-race-persistence-verify-absent'", $migration,
    [StringComparison]::Ordinal)
$finally = $transaction.IndexOf('finally {', $absence,
    [StringComparison]::Ordinal)
$restore = $transaction.IndexOf('Restore-OriginalFeatureState', $finally,
    [StringComparison]::Ordinal)
$redeploy = $transaction.IndexOf(
    '$restoredDeploymentManifestPath = & $deployCurrent', $restore,
    [StringComparison]::Ordinal)
if ($old -lt 0 -or $prepare -le $old -or $current -le $prepare -or
    $migration -le $current -or $absence -le $migration -or
    $finally -le $absence -or $restore -le $finally -or
    $redeploy -le $restore -or
    $transaction.Contains('KMG_AUTOMATION_BASELINE')) {
    throw 'Legacy migration transaction order, restoration, or protected-save exclusion is not exact.'
}

Write-Host ('Elemental Race legacy migration qualification tests passed: ' +
    'package=0.0.114;entries=135;packageSha={0};dllSha={1};mvid={2};scripts=4' -f
    $expectedPackageSha, $expectedDllSha, $expectedMvid)
