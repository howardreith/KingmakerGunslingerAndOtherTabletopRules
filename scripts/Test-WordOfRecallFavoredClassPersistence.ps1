[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$tokens = $null; $errors = $null
$scriptPath = Join-Path $PSScriptRoot 'Invoke-WordOfRecallFavoredClassPersistence.ps1'
$ast = [Management.Automation.Language.Parser]::ParseInput(([IO.File]::ReadAllText($scriptPath)), [ref]$tokens, [ref]$errors)
if ($errors.Count -ne 0) { throw 'Favored Class persistence orchestrator has syntax errors.' }
foreach ($name in @('Remove-PersistenceOwnedSave')) {
    $function = @($ast.FindAll({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name }, $true))
    if ($function.Count -ne 1) { throw 'Expected the exact hardened cleanup seam.' }
    . ([scriptblock]::Create($function[0].Extent.Text))
}
$testRoot = Join-Path (Split-Path -Parent $PSScriptRoot) ('artifacts\tests\fcb-persistence-' + [Guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $testRoot)
$catalog = [pscustomobject]@{ Directory = $testRoot; Files = @([pscustomobject]@{ path = (Join-Path $testRoot 'Manual_299_KMG_AUTOMATION_WORKING.zks') }) }
function New-OwnedFile([string]$content) {
    $name = 'KMG_FCB_PERSISTENCE_20260920T0401079546815Z_c87ca63ff44b44e69bdbe11fdde271f6_prepare'
    $path = Join-Path $testRoot ('Manual_77_' + $name + '.zks')
    [IO.File]::WriteAllText($path, $content)
    return [ordered]@{
        name = $name; path = $path; phase = 'prepare'; runId = 'exact-test-run'
        receipt = (Join-Path $testRoot 'owned.json')
        completedSha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        createdSha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        TransactionDirectory = $testRoot }
}
$checks = 0
try {
    # 1. Changed output (modified after completion) is preserved.
    $save = New-OwnedFile 'original campaign bytes'
    [IO.File]::WriteAllText($save.path, 'modified campaign bytes')
    $failures = New-Object 'System.Collections.Generic.List[object]'
    $deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
    if ($deleted -or -not (Test-Path -LiteralPath $save.path -PathType Leaf) -or $failures.Count -ne 1 -or
        -not ([string]$failures[0].reason).Contains('completed-save receipt')) { throw 'Changed owned-save output was deleted or not reported.' }
    $checks++

    # 2. Replaced output (the file removed and a different one swapped into
    #    the proven path) is preserved: the identity still names the original
    #    bytes while the path now holds different content.
    $save = New-OwnedFile 'first campaign'
    Remove-Item -LiteralPath $save.path -Force
    [IO.File]::WriteAllText($save.path, 'replaced campaign with different length bytes')
    $failures = New-Object 'System.Collections.Generic.List[object]'
    $deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
    if ($deleted -or -not (Test-Path -LiteralPath $save.path -PathType Leaf) -or $failures.Count -ne 1 -or
        -not ([string]$failures[0].reason).Contains('completed-save receipt')) { throw 'Replaced owned-save output was deleted or not reported.' }
    $checks++

    # 3. Missing authoritative owned-save proof (no completed hash) preserves the file.
    $save = New-OwnedFile 'unproven campaign'
    $save.completedSha256 = $null
    $failures = New-Object 'System.Collections.Generic.List[object]'
    $deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
    if ($deleted -or -not (Test-Path -LiteralPath $save.path -PathType Leaf) -or
        -not ([string]$failures[0].reason).Contains('missing authoritative owned-save proof')) { throw 'Unproven owned-save output was deleted or not reported.' }
    $checks++

    # 4. An absent file is reported, never treated as deleted.
    $save = New-OwnedFile 'vanished campaign'
    Remove-Item -LiteralPath $save.path -Force
    $failures = New-Object 'System.Collections.Generic.List[object]'
    $deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
    if ($deleted -or $failures.Count -ne 1 -or
        -not ([string]$failures[0].reason).Contains('owned save file is absent')) { throw 'Absent owned-save file was not reported.' }
    $checks++

    # 5. A path outside the catalog directory is refused.
    $save = New-OwnedFile 'escaped campaign'
    $save.path = Join-Path (Join-Path $testRoot 'outside') 'Manual_77_escape.zks'
    $failures = New-Object 'System.Collections.Generic.List[object]'
    $deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
    if ($deleted -or -not ([string]$failures[0].reason).Contains('escaped its proven transaction')) { throw 'Escaping cleanup target was deleted or not refused.' }
    $checks++

    # 6. An unchanged, fully proven owned save IS deleted.
    $save = New-OwnedFile 'exact campaign bytes'
    $failures = New-Object 'System.Collections.Generic.List[object]'
    $deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
    if (-not $deleted -or (Test-Path -LiteralPath $save.path -PathType Leaf) -or $failures.Count -ne 0) { throw 'A fully proven unchanged owned save was not deleted.' }
    $checks++

    # 7. A protected-catalog file is never a deletion target.
    $save = New-OwnedFile 'protected bytes'
    $save.path = $catalog.Files[0].path
    [IO.File]::WriteAllText($save.path, 'protected')
    $failures = New-Object 'System.Collections.Generic.List[object]'
    $deleted = Remove-PersistenceOwnedSave -Save $save -Catalog $catalog -Failures $failures
    if ($deleted -or -not (Test-Path -LiteralPath $save.path -PathType Leaf)) { throw 'A protected-catalog file was deleted.' }
    $checks++
}
finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "PASS Favored Class owned-save cleanup filesystem regressions; checks=$checks"
