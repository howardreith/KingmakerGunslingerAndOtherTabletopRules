[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
function Assert-True([bool]$Condition, [string]$Name) {
    if (-not $Condition) { $failures.Add($Name) }
}
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $failures.Add($Name) } catch { }
}

$root = Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts\atomic-write-tests'
if (Test-Path -LiteralPath $root) {
    Remove-Item -LiteralPath $root -Recurse -Force
}
try {
    New-Item -ItemType Directory -Path $root | Out-Null
    $newPath = Join-Path $root 'new.json'
    Write-KmgUtf8NoBom -Path $newPath -Content 'new'
    Assert-True ([IO.File]::ReadAllText($newPath) -eq 'new') 'new-file-written'
    Assert-True (-not ([IO.File]::ReadAllBytes($newPath) -join ',').StartsWith('239,187,191')) `
        'utf8-has-no-bom'

    Write-KmgUtf8NoBom -Path $newPath -Content 'replacement'
    Assert-True ([IO.File]::ReadAllText($newPath) -eq 'replacement') `
        'existing-file-replaced'

    Assert-Throws { Write-KmgUtf8NoBom -Path $null -Content 'x' } 'null-rejected'
    Assert-Throws { Write-KmgUtf8NoBom -Path '' -Content 'x' } 'empty-rejected'
    Assert-Throws { Write-KmgUtf8NoBom -Path '   ' -Content 'x' } 'whitespace-rejected'
    Assert-Throws {
        Write-KmgUtf8NoBom -Path @($newPath, $newPath) -Content 'x'
    } 'array-rejected'
    Assert-Throws { Write-KmgUtf8NoBom -Path "`"$newPath`"" -Content 'x' } `
        'quoted-rejected'
    Assert-Throws { Write-KmgUtf8NoBom -Path $root -Content 'x' } 'directory-rejected'
    Assert-Throws {
        Write-KmgUtf8NoBom -Path ($root + '\') -Content 'x'
    } 'missing-filename-rejected'
    Assert-Throws {
        Write-KmgUtf8NoBom -Path (Join-Path $root 'invalid?.json') -Content 'x'
    } 'invalid-character-rejected'

    $lockedPath = Join-Path $root 'locked.json'
    [IO.File]::WriteAllText($lockedPath, 'preserve')
    $unrelated = Join-Path $root '.unrelated.tmp'
    [IO.File]::WriteAllText($unrelated, 'keep')
    $lock = [IO.File]::Open(
        $lockedPath, [IO.FileMode]::Open, [IO.FileAccess]::Read,
        [IO.FileShare]::None)
    try {
        Assert-Throws {
            Write-KmgUtf8NoBom -Path $lockedPath -Content 'must-not-land'
        } 'replacement-failure-reported'
    }
    finally {
        $lock.Dispose()
    }
    Assert-True ([IO.File]::ReadAllText($lockedPath) -eq 'preserve') `
        'failed-replacement-preserves-destination'
    Assert-True (Test-Path -LiteralPath $unrelated -PathType Leaf) `
        'unrelated-temporary-preserved'
    Assert-True (@(Get-ChildItem -LiteralPath $root -Filter '.locked.json.*.tmp').Count -eq 0) `
        'owned-temporary-removed'

    $legacySource = Join-Path $root 'legacy-source.tmp'
    $legacyDestination = Join-Path $root 'legacy-destination.json'
    [IO.File]::WriteAllText($legacySource, 'new')
    [IO.File]::WriteAllText($legacyDestination, 'old')
    $legacyMessage = $null
    try {
        [IO.File]::Replace($legacySource, $legacyDestination, $null)
    }
    catch {
        $legacyMessage = $_.Exception.Message
    }
    Assert-True ($legacyMessage -like '*path is not of a legal form*') `
        'observed-windows-powershell-null-backup-failure-reproduced'
}
finally {
    if (Test-Path -LiteralPath $root) {
        Remove-Item -LiteralPath $root -Recurse -Force
    }
}

if ($failures.Count -ne 0) {
    throw "Atomic-write tests failed: $($failures -join ', ')"
}
Write-Host 'Atomic-write tests passed: 16'
