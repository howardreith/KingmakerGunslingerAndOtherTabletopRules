[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
$root = Join-Path (Split-Path -Parent $PSScriptRoot) ('artifacts\teleportation\save-protection-test-' + [Guid]::NewGuid().ToString('N'))
$saves = Join-Path $root 'saves'
$evidence = Join-Path $root 'evidence'
[void](New-Item -ItemType Directory -Path $saves)
$file = Join-Path $saves 'fixture.zks'
[IO.File]::WriteAllBytes($file, [byte[]]@(1, 2, 3))
$catalog = Open-KmgProtectedSaveCatalog -SaveDirectory $saves -EvidenceDirectory $evidence
$assertions = 0
try {
    foreach ($operation in @('write', 'delete', 'rename')) {
        $denied = $false
        try {
            switch ($operation) {
                'write' { [IO.File]::WriteAllBytes($file, [byte[]]@(4)) }
                'delete' { [IO.File]::Delete($file) }
                'rename' { [IO.File]::Move($file, (Join-Path $saves 'renamed.zks')) }
            }
        } catch [IO.IOException] { $denied = $true }
        if (-not $denied) { throw ('Read lease did not prevent ' + $operation) }
        $assertions++
    }
    $result = Assert-KmgProtectedSaveCatalog -Catalog $catalog
    if (-not $result.passed) { throw 'Fixture preservation failed.' }
    $assertions++
    $new = Join-Path $saves 'owned.zks'
    [IO.File]::WriteAllBytes($new, [byte[]]@(5))
    $rejected = $false
    try { [void](Assert-KmgProtectedSaveCatalog -Catalog $catalog) } catch { $rejected = $true }
    if (-not $rejected) { throw 'Unowned new save was accepted.' }
    $assertions++
    [void](Assert-KmgProtectedSaveCatalog -Catalog $catalog -OwnedPaths @($new))
    $assertions++
    Write-Host "PASS: $assertions save-protection assertions; actual campaign saves were not opened. Evidence: $root"
} finally { Close-KmgProtectedSaveCatalog -Catalog $catalog }
