<#
.SYNOPSIS
Behaviour regression for the version-specific child dispatch in
scripts/validate-repository.ps1.

.DESCRIPTION
A nested `if` once made the 0.0.157 block a child of the 0.0.156 block. The
wrapper still exited 0, so a passing run did not establish that the Sprint 25
corruption fixtures had executed at all. Exit status alone cannot catch that:
the test has to observe which children were actually selected.

This does not hand-copy the dispatch into a second switch and does not look for
a filename in the source text. It parses the real script, takes the actual
`if` statements whose condition reads $activeVersion, and executes those exact
source extents with a controlled child shim standing in for the Python
interpreter. Nesting is therefore reproduced faithfully: a block nested inside
a non-matching parent simply never runs.

Nothing here builds, deploys, launches the game, or invokes the expensive
validation campaign - only the dispatch statements run, and their children are
recorded rather than executed.

Run: powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RepositoryValidatorDispatch.ps1
#>
[CmdletBinding()]
param([string]$ValidatorPath)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $ValidatorPath) {
    $ValidatorPath = Join-Path $PSScriptRoot 'validate-repository.ps1'
}
$validatorFull = [IO.Path]::GetFullPath($ValidatorPath)
if (-not (Test-Path -LiteralPath $validatorFull)) {
    throw "Repository validator is missing: $validatorFull"
}

$errors = $null
$tokens = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile(
    $validatorFull, [ref]$tokens, [ref]$errors)
if (@($errors).Count -ne 0) {
    throw "Repository validator does not parse: $(@($errors).Count) error(s)."
}

# The real top-level version-dispatch statements, in source order. Taking only
# top-level statements is deliberate: a block nested inside another is not a
# dispatch decision of its own and must not be lifted out of its parent here,
# because that is exactly the defect under test.
$dispatchStatements = @(
    $ast.EndBlock.Statements |
        Where-Object {
            $_ -is [System.Management.Automation.Language.IfStatementAst] -and
            $_.Clauses[0].Item1.Extent.Text -match '\$activeVersion'
        })
if ($dispatchStatements.Count -lt 2) {
    throw ("Expected several top-level version-dispatch statements; found " +
        "$($dispatchStatements.Count). The dispatch may have been nested again.")
}

$root = Join-Path ([IO.Path]::GetTempPath()) ('kmg-dispatch-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
$childLog = Join-Path $root 'children.log'

# The child shim: records the requested child and returns a scripted code.
$shim = Join-Path $root 'child-shim.ps1'
@'
$requested = if ($args.Count -gt 0) { [string]$args[0] } else { '' }
$root = Split-Path -Parent $PSCommandPath
Add-Content -LiteralPath (Join-Path $root 'children.log') -Value $requested
$failMarker = Join-Path $root 'fail-on.txt'
if (Test-Path -LiteralPath $failMarker) {
    $target = (Get-Content -Raw -LiteralPath $failMarker).Trim()
    if ($target -and $requested -like ('*' + $target)) { exit 3 }
}
exit 0
'@ | Set-Content -LiteralPath $shim -Encoding UTF8

function Invoke-Dispatch {
    param([string]$Version, [string]$FailOnChild)
    Set-Content -LiteralPath $script:childLog -Value $null
    $failMarker = Join-Path $script:root 'fail-on.txt'
    if ($FailOnChild) { Set-Content -LiteralPath $failMarker -Value $FailOnChild -Encoding UTF8 }
    elseif (Test-Path -LiteralPath $failMarker) { Remove-Item -LiteralPath $failMarker -Force }

    # The variables the real statements close over.
    $activeVersion = $Version
    $repositoryRoot = 'C:\kmg-dispatch-fixture'
    $python = [pscustomobject]@{ Source = $script:shim }
    $global:LASTEXITCODE = 0
    $thrown = $null
    foreach ($statement in $script:dispatchStatements) {
        try {
            # The actual source extent of the real statement, executed as-is.
            $block = [ScriptBlock]::Create($statement.Extent.Text)
            & $block
        }
        catch {
            $thrown = $_.Exception.Message
            break
        }
    }
    $selected = @()
    if (Test-Path -LiteralPath $script:childLog) {
        $selected = @(Get-Content -LiteralPath $script:childLog |
            Where-Object { $_ -ne '' } |
            ForEach-Object { Split-Path -Leaf $_ })
    }
    return [pscustomobject]@{ Selected = $selected; Thrown = $thrown }
}

$results = New-Object System.Collections.ArrayList
$failures = 0
function Check {
    param([string]$Name, [bool]$Ok, [string]$Detail)
    if (-not $Ok) { $script:failures++ }
    [void]$script:results.Add([pscustomobject]@{
        Check = $Name
        Result = if ($Ok) { 'PASS' } else { 'FAIL' }
        Detail = $Detail })
}

try {
    $firearm = 'test_firearm_continuation_release156.py'
    $sprint25 = 'test_expanded_summoning_sprint25157.py'
    $sprint23 = 'test_expanded_summoning_sprint23152.py'
    $sprint24 = 'test_expanded_summoning_sprint24154.py'

    $r156 = Invoke-Dispatch -Version '0.0.156'
    Check '0.0.156 selects its firearm child exactly once' `
        (@($r156.Selected | Where-Object { $_ -eq $firearm }).Count -eq 1) `
        ($r156.Selected -join ', ')
    Check '0.0.156 does not select the Sprint 25 child' `
        (@($r156.Selected | Where-Object { $_ -eq $sprint25 }).Count -eq 0) `
        ($r156.Selected -join ', ')

    $r157 = Invoke-Dispatch -Version '0.0.157'
    # This is the regression: before the repair the Sprint 25 block was nested
    # inside the 0.0.156 block, so at 0.0.157 it was never reached and the
    # wrapper still exited 0.
    Check '0.0.157 selects its Sprint 25 child exactly once' `
        (@($r157.Selected | Where-Object { $_ -eq $sprint25 }).Count -eq 1) `
        ($r157.Selected -join ', ')
    Check '0.0.157 does not select the 0.0.156-only firearm child' `
        (@($r157.Selected | Where-Object { $_ -eq $firearm }).Count -eq 0) `
        ($r157.Selected -join ', ')

    # Shared checks stay selected for both versions.
    foreach ($version in @('0.0.156', '0.0.157')) {
        $shared = Invoke-Dispatch -Version $version
        Check "$version keeps the shared Sprint 23 child" `
            ($shared.Selected -contains $sprint23) ($shared.Selected -join ', ')
        Check "$version keeps the shared Sprint 24 child" `
            ($shared.Selected -contains $sprint24) ($shared.Selected -join ', ')
    }

    # An unrelated version selects neither version-specific child.
    $r155 = Invoke-Dispatch -Version '0.0.155'
    Check '0.0.155 selects neither version-specific child' `
        ((@($r155.Selected | Where-Object { $_ -eq $firearm }).Count -eq 0) -and
         (@($r155.Selected | Where-Object { $_ -eq $sprint25 }).Count -eq 0)) `
        ($r155.Selected -join ', ')

    # A failing selected child must stop the parent, before anything is built
    # or deployed.
    $failing = Invoke-Dispatch -Version '0.0.157' -FailOnChild $sprint25
    Check 'a failing Sprint 25 child throws rather than continuing' `
        ($null -ne $failing.Thrown -and $failing.Thrown -match 'Sprint 25 corruption fixtures failed') `
        ([string]$failing.Thrown)
    $failingFirearm = Invoke-Dispatch -Version '0.0.156' -FailOnChild $firearm
    Check 'a failing firearm child throws rather than continuing' `
        ($null -ne $failingFirearm.Thrown -and
            $failingFirearm.Thrown -match 'Firearm continuation patch corruption fixtures failed') `
        ([string]$failingFirearm.Thrown)

    # No dispatch statement may be nested inside another: that is the defect.
    $nested = @()
    foreach ($statement in $dispatchStatements) {
        $inner = @($statement.FindAll({
                param($n) $n -is [System.Management.Automation.Language.IfStatementAst] -and
                $n.Clauses[0].Item1.Extent.Text -match '\$activeVersion' }, $true) |
            Where-Object { $_.Extent.StartOffset -ne $statement.Extent.StartOffset })
        if ($inner.Count -gt 0) { $nested += $statement.Clauses[0].Item1.Extent.Text }
    }
    Check 'no version dispatch is nested inside another' ($nested.Count -eq 0) `
        ($nested -join ' | ')
}
finally {
    try { Remove-Item -Recurse -Force -LiteralPath $root -ErrorAction Stop } catch { }
}

Write-Host ''
Write-Host "Validator          : $validatorFull"
Write-Host "Dispatch statements: $($dispatchStatements.Count) top-level, executed as real source extents"
Write-Host ''
$results | Format-Table -AutoSize | Out-String -Width 200 | Write-Host
if ($failures -ne 0) {
    throw "Repository validator dispatch regression FAILED: $failures of $($results.Count) checks."
}
Write-Host ("Repository validator dispatch regression passed: $($results.Count) checks; " +
    'real dispatch statements with a recorded child shim; nothing built, deployed or launched.')
