[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'CompatibilityProfile.Common.ps1')
$root = Get-KmgCompatibilityRepositoryRoot
$base = @{ ProfileId = 'gunslinger-cowwithhat-suggestion'; RepositoryRoot = $root
    PackagePath = (Join-Path $root 'artifacts/selection-fixture.zip')
    ReferenceRoot = (Join-Path $root 'artifacts/selection-fixture-references') }
$profile = (Read-KmgCompatibilityJson (Join-Path $root 'compatibility/profiles.json')).profiles |
    Where-Object id -ceq $base.ProfileId
$passed = 0
foreach ($scenario in $profile.scenarios) {
    Assert-KmgSuggestionProfileSelection @base -Scenario @($scenario)
    $passed++
}
function Refuses([hashtable]$Arguments, [string]$Message) {
    try { Assert-KmgSuggestionProfileSelection @Arguments }
    catch {
        if ($_.Exception.Message -notlike ('*' + $Message + '*')) { throw }
        $script:passed++; return
    }
    throw 'Expected profile admission refusal was absent.'
}
foreach ($scenario in @('working-save-expanded-summoning-prepare',
    'disposable-expanded-summoning-sprint29-native-review', 'unknown', '')) {
    $case = $base.Clone(); $case.Scenario = @($scenario)
    Refuses $case 'declared save-free'
}
$case = $base.Clone(); $case.Scenario = @('mod-load-smoke', 'working-save-smoke')
Refuses $case 'declared save-free'
$case = $base.Clone(); $case.Scenario = @('mod-load-smoke'); $case.PackagePath = ''
Refuses $case 'explicit candidate'
$case = $base.Clone(); $case.Scenario = @('mod-load-smoke'); $case.ReferenceRoot = ''
Refuses $case 'explicit candidate'
$case = $base.Clone(); $case.Scenario = @('mod-load-smoke')
$case.ReferenceRoot = 'C:/Dev/KingmakerGunslingerLab/examples'
Refuses $case 'isolated under'
# A lexical child path cannot authorize a linked source tree. Use an empty
# workspace-owned junction; remove the link itself, never its target contents.
$fixture = Join-Path $root ('artifacts/suggestion-profile-links-' + [Guid]::NewGuid().ToString('N'))
$references = Join-Path $fixture 'references'
$outside = Join-Path $fixture 'outside'
$alias = Join-Path $references 'CowWithHatsCustomSpellsMod'
$link = Join-Path $alias 'linked-child'
try {
    [void][IO.Directory]::CreateDirectory($alias)
    [void][IO.Directory]::CreateDirectory($outside)
    New-Item -ItemType Junction -Path $link -Target $outside | Out-Null
    $case = $base.Clone(); $case.Scenario = @('mod-load-smoke'); $case.ReferenceRoot = $references
    Refuses $case 'selected reference tree contains a reparse point'
} finally {
    if (Test-Path -LiteralPath $link) {
        if (-not [IO.Path]::GetFullPath($link).StartsWith([IO.Path]::GetFullPath($fixture) + '\', [StringComparison]::OrdinalIgnoreCase) -or
            ((Get-Item -LiteralPath $link -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -eq 0) {
            throw 'Fixture junction cleanup boundary changed; preserve it for inspection.'
        }
        [IO.Directory]::Delete($link)
    }
    foreach ($directory in @($alias, $references, $outside, $fixture)) {
        if (Test-Path -LiteralPath $directory) { [IO.Directory]::Delete($directory) }
    }
}
$case = $base.Clone(); $case.Scenario = @('mod-load-smoke')
$case.ReferenceRoot = Join-Path $root 'artifacts/../../outside'
Refuses $case 'isolated under'
# Exercise the actual entry point's early refusal. WhatIf alone is not the
# predicate: this unsupported, otherwise known scenario must throw before any
# profile, deployment or package selection can begin.
try {
    & (Join-Path $PSScriptRoot 'Invoke-KingmakerCompatibilityProfile.ps1') `
        -ProfileId $base.ProfileId -Scenario 'disposable-expanded-summoning-sprint29-native-review' `
        -ReferenceRoot $base.ReferenceRoot -PackagePath $base.PackagePath -WhatIf
} catch {
    if ($_.Exception.Message -notlike '*declared save-free*') { throw }
    $passed++
    Write-Host "Suggestion profile admission PASS=$passed; declared scenarios, unsafe selections, absent candidate and reference escape tested; no staging or game launch."
    return
}
throw 'Actual profile entry point did not refuse the undeclared scenario.'
