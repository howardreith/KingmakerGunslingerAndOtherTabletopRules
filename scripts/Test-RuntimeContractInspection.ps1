[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeContractInspection.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
function Assert-True([bool]$Condition, [string]$Name) {
    if (-not $Condition) { $failures.Add($Name) }
}
function Assert-ThrowsLike([scriptblock]$Action, [string]$Pattern, [string]$Name) {
    try {
        & $Action
        $failures.Add($Name)
    }
    catch {
        if ($_.Exception.Message -notmatch $Pattern) {
            $failures.Add("$Name ($($_.Exception.Message))")
        }
    }
}

$unrelatedParameterCalls = 0
$unrelated = [pscustomobject]@{
    Name = 'System.Collections.Generic.ICollection<T>.Add'
    DeclaringType = $null
    MetadataToken = 2
}
$unrelated | Add-Member -MemberType ScriptMethod -Name GetParameters -Value {
    $script:unrelatedParameterCalls++
    throw [TypeLoadException]::new(
        "Method 'System.Collections.Generic.HashSet``1<T>." +
        "System.Collections.Generic.ICollection<T>.Add(!0)' is security transparent, " +
        'but is a member of a security critical type.')
}
$required = [pscustomobject]@{
    Name = 'Get'
    DeclaringType = $null
    MetadataToken = 1
}
$required | Add-Member -MemberType ScriptMethod -Name GetParameters -Value {
    return @([pscustomobject]@{ ParameterType = [string] })
}

$selected = @(Select-KmgNamedMethodCandidates -Methods @($unrelated, $required) `
    -Names @('Get', 'Ensure'))
Assert-True ($selected.Count -eq 1 -and $selected[0].Name -eq 'Get') `
    'stable-name-filter-selects-only-explicit-contract-candidates'
Assert-True ($unrelatedParameterCalls -eq 0) `
    'unrelated-hashset-add-parameter-metadata-not-inspected'
$parameters = @(Get-KmgRequiredMethodParameters -Method $selected[0] `
    -ContractName 'fixture-required-get')
Assert-True ($parameters.Count -eq 1) 'independent-required-contract-validates'

$requiredFailure = [pscustomobject]@{
    Name = 'Ensure'
    DeclaringType = $null
    MetadataToken = 3
}
$requiredFailure | Add-Member -MemberType ScriptMethod -Name GetParameters -Value {
    throw [TypeLoadException]::new('synthetic required-contract loader failure')
}
Assert-ThrowsLike {
    Get-KmgRequiredMethodParameters -Method $requiredFailure `
        -ContractName 'fixture-required-ensure'
} 'fixture-required-ensure.*synthetic required-contract loader failure' `
    'required-loader-failure-remains-fatal-and-contextual'

$inspector = Get-Content -LiteralPath (
    Join-Path $PSScriptRoot 'inspect-runtime-contracts.ps1') -Raw
Assert-True ($inspector.Contains(
    "Select-KmgNamedMethodCandidates ``") -and
    $inspector.Contains("-Names @('Get', 'Ensure')")) `
    'broad-extension-enumeration-filters-by-name-first'
Assert-True ($inspector.Contains(
    "Get-KmgRequiredMethodParameters -Method `$method")) `
    'candidate-parameter-loader-is-strict'
Assert-True ($inspector.Contains('$unitPartGetContractPassed') -and
    $inspector.Contains('$unitPartEnsureContractPassed') -and
    $inspector.Contains('$contractPassed = (')) `
    'existing-required-contract-gates-retained'
Assert-True ($inspector.Contains('toleratedLoaderFailures = @()')) `
    'tolerated-unrelated-loader-events-explicitly-recorded'
Assert-True ($inspector.Contains(
    'unrelatedStaticMethodsExcludedBeforeParameterInspection')) `
    'excluded-unrelated-metadata-count-recorded'
Assert-True ($inspector.Contains(
    '$declaredFlags = $BindingFlags -bor [Reflection.BindingFlags]::DeclaredOnly') -and
    $inspector.Contains('$current = $current.BaseType')) `
    'hidden-property-lookup-is-declared-and-hierarchy-scoped'

$namesA = @((Select-KmgNamedMethodCandidates -Methods @($unrelated, $required) `
    -Names @('Get', 'Ensure')) | ForEach-Object Name)
$namesB = @((Select-KmgNamedMethodCandidates -Methods @($required, $unrelated) `
    -Names @('Get', 'Ensure')) | ForEach-Object Name)
Assert-True (($namesA -join ',') -eq ($namesB -join ',')) `
    'candidate-order-deterministic'
Assert-True (@(Select-KmgNamedMethodCandidates -Methods @() `
    -Names @('Get', 'Ensure')).Count -eq 0) 'missing-required-member-not-invented'
Assert-True (-not $inspector.Contains('catch [TypeLoadException] { continue }')) `
    'no-blanket-loader-suppression'

if ($failures.Count -ne 0) {
    throw "Runtime-contract inspector regression tests failed: $($failures -join ', ')"
}
Write-Host 'Runtime-contract inspector regression tests passed: 11'
