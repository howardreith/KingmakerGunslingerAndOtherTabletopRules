Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot 'ReferenceProvenance.Common.ps1')

<#
A development candidate is a package built for one named development purpose,
such as a guarded native observation of a review branch, from one exact clean
commit that keeps the released version. It is never a release candidate:

- it lives under artifacts\development-candidates\<purpose>\<commit12>\ with a
  file name that says "development", never under artifacts\local-runtime;
- it carries a development receipt, never a Build-Local release manifest;
- every release and qualification reader refuses it, and only a caller that
  names the same purpose explicitly can consume it;
- its deployment record and runtime evidence say candidateKind=development,
  and closure qualification refuses any proof that does.

The receipt binds the purpose, commit, source state, package and DLL hashes,
the exact private reference identity, the development source validation and
the selected domain-test evidence. Nothing here skips a check: the package is
assembled and strictly validated by the same package scripts, and the only
replacement is the version-specific release validator and complete suite.
#>

$script:KmgDevelopmentGenerator = 'scripts/Build-Local.ps1 -DevelopmentPurpose'
$script:KmgDevelopmentCandidateRoot = 'artifacts\development-candidates'
$script:KmgDevelopmentProofPattern = '"candidateKind"\s*:\s*"development"'

function Assert-KmgDevelopmentPurpose {
    param([AllowNull()][AllowEmptyString()][string]$Purpose)
    if ([string]::IsNullOrEmpty($Purpose) -or
        $Purpose -cnotmatch '^[a-z0-9][a-z0-9-]{1,46}[a-z0-9]$' -or
        $Purpose.Contains('--')) {
        throw "Development purpose must be a lower-case hyphenated name of 3-48 characters: '$Purpose'"
    }
    return $Purpose
}

function Get-KmgDevelopmentReceiptPath {
    param([Parameter(Mandatory = $true)][string]$PackagePath)
    return "$([IO.Path]::GetFullPath($PackagePath)).development-receipt.json"
}

function Get-KmgDevelopmentCandidateLayout {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$Purpose,
        [Parameter(Mandatory = $true)][string]$Commit,
        [Parameter(Mandatory = $true)][string]$Id,
        [Parameter(Mandatory = $true)][string]$Version
    )
    [void](Assert-KmgDevelopmentPurpose $Purpose)
    if ($Commit -cnotmatch '^[0-9a-f]{40}$') {
        throw 'A development candidate needs one exact 40-character commit.'
    }
    $short = $Commit.Substring(0, 12)
    $root = [IO.Path]::GetFullPath((Join-Path $RepositoryRoot (
        Join-Path $script:KmgDevelopmentCandidateRoot "$Purpose\$short")))
    $package = Join-Path $root "$Id-$Version-development-$Purpose-$short.zip"
    return [pscustomobject]@{
        Root = $root
        PackagePath = $package
        ReceiptPath = Get-KmgDevelopmentReceiptPath -PackagePath $package
        ExactBuildRoot = Join-Path $root 'exact-build'
        TestLogPath = Join-Path $root 'selected-domain-tests.log'
        ValidationSummaryPath = Join-Path $root 'development-validation.json'
    }
}

function Test-KmgDevelopmentCandidatePackage {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot
    )
    $package = [IO.Path]::GetFullPath($PackagePath)
    $developmentRoot = [IO.Path]::GetFullPath((Join-Path $RepositoryRoot `
        $script:KmgDevelopmentCandidateRoot)).TrimEnd('\') + '\'
    return (Test-Path -LiteralPath (Get-KmgDevelopmentReceiptPath -PackagePath $package)) -or
        $package.StartsWith($developmentRoot, [StringComparison]::OrdinalIgnoreCase) -or
        ([IO.Path]::GetFileName($package) -like '*-development-*')
}

function Assert-KmgNotDevelopmentCandidate {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot
    )
    if (Test-KmgDevelopmentCandidatePackage -PackagePath $PackagePath -RepositoryRoot $RepositoryRoot) {
        throw 'A development candidate cannot be consumed as a release or qualification candidate.'
    }
}

function Get-KmgOptionalProperty {
    param([AllowNull()]$Object, [Parameter(Mandatory = $true)][string]$Name)
    if ($null -eq $Object) { return $null }
    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) { return $null }
    return $property.Value
}

function Get-KmgUtf8Sha256 {
    param([AllowEmptyString()][string]$Text)
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString($sha.ComputeHash(
            [Text.Encoding]::UTF8.GetBytes($Text))) -replace '-', '').ToLowerInvariant()
    } finally { $sha.Dispose() }
}

function Read-KmgDomainTestEvidence {
    <# Reads the domain runner's own lines from one selected run. Absent,
       zero, failed, repeated or inconsistent evidence is refused. #>
    param(
        [Parameter(Mandatory = $true)][string]$LogPath,
        [Parameter(Mandatory = $true)][string]$Filter
    )
    if (-not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
        throw "Selected domain-test evidence is missing: $LogPath"
    }
    $lines = @(Get-Content -LiteralPath $LogPath)
    $selectionPattern = '^KMG_TEST_FILTER=(.*); selected (\d+) of (\d+) registered tests\.'
    $completionPattern = '^Completed (\d+) tests; failures=(\d+)\. Registered (\d+); elapsed'
    $selection = @($lines | Where-Object { $_ -cmatch $selectionPattern })
    $completion = @($lines | Where-Object { $_ -cmatch $completionPattern })
    if ($selection.Count -ne 1 -or $completion.Count -ne 1) {
        throw 'Selected domain-test evidence lacks exactly one selection and one completion line.'
    }
    [void]($selection[0] -cmatch $selectionPattern)
    $observedFilter = $Matches[1]
    $selected = [int]$Matches[2]
    $registered = [int]$Matches[3]
    [void]($completion[0] -cmatch $completionPattern)
    $completed = [int]$Matches[1]
    $failures = [int]$Matches[2]
    $completedRegistered = [int]$Matches[3]
    $passed = [string[]]@($lines | Where-Object { $_ -cmatch '^PASS \S' } |
        ForEach-Object { $_.Substring(5) })
    $failed = @($lines | Where-Object { $_ -cmatch '^FAIL ' })
    $unique = New-Object 'Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
    foreach ($name in $passed) { [void]$unique.Add($name) }
    if ($observedFilter -cne $Filter) {
        throw "Selected domain-test evidence ran filter '$observedFilter', not '$Filter'."
    }
    if ($selected -lt 1 -or $selected -gt $registered -or $completed -ne $selected -or
        $completedRegistered -ne $registered -or $failures -ne 0 -or $failed.Count -ne 0 -or
        $passed.Count -ne $selected -or $unique.Count -ne $selected) {
        throw 'Selected domain-test evidence is empty, failed or inconsistent.'
    }
    [Array]::Sort($passed, [StringComparer]::Ordinal)
    return [ordered]@{
        filter = $Filter
        selected = $selected
        registered = $registered
        failures = 0
        passedNamesSha256 = Get-KmgUtf8Sha256 ($passed -join "`n")
        logSha256 = Get-KmgSha256 -Path $LogPath
    }
}

function Assert-KmgDevelopmentReceipt {
    <# The receipt's own consistency and its binding to the package bytes,
       the development-only location, the selected test log and the source
       validation summary. Callers separately bind it to the current source. #>
    param(
        [Parameter(Mandatory = $true)]$Receipt,
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$Purpose
    )
    [void](Assert-KmgDevelopmentPurpose $Purpose)
    foreach ($name in @('schemaVersion', 'candidateKind', 'releaseQualification',
        'generator', 'purpose', 'commit', 'branch', 'sourceStateSha256', 'version',
        'packagePath', 'packageSha256', 'dllSha256', 'dllMvid',
        'firearmManifestSha256', 'firearmSoundBankSha256', 'references',
        'validation', 'selectedTests')) {
        if ($null -eq $Receipt.PSObject.Properties[$name]) {
            throw "Development receipt lacks required content: $name"
        }
    }
    if ($Receipt.schemaVersion -ne 1 -or $Receipt.candidateKind -cne 'development' -or
        $Receipt.releaseQualification -ne $false -or
        $Receipt.generator -cne $script:KmgDevelopmentGenerator) {
        throw 'Development receipt has an invalid schema, kind or generator.'
    }
    if ($Receipt.purpose -cne $Purpose) {
        throw "Development candidate was built for purpose '$($Receipt.purpose)', not '$Purpose'."
    }
    $info = Get-KmgModInfo -RepositoryRoot $RepositoryRoot
    if ($Receipt.version -cne $info.Version) {
        throw "Development candidate version $($Receipt.version) is not the repository version $($info.Version)."
    }
    $layout = Get-KmgDevelopmentCandidateLayout -RepositoryRoot $RepositoryRoot `
        -Purpose $Purpose -Commit ([string]$Receipt.commit) -Id $info.Id -Version $info.Version
    $package = [IO.Path]::GetFullPath($PackagePath)
    if (-not $layout.PackagePath.Equals($package, [StringComparison]::OrdinalIgnoreCase) -or
        $Receipt.packagePath -cne $layout.PackagePath) {
        throw 'Development package is not at its exact development-only location and name.'
    }
    foreach ($name in @('sourceStateSha256', 'packageSha256', 'dllSha256',
        'firearmManifestSha256', 'firearmSoundBankSha256')) {
        if ([string]$Receipt.$name -cnotmatch '^[0-9a-f]{64}$') {
            throw "Development receipt has a malformed $name."
        }
    }
    $mvid = [Guid]::Empty
    if (-not [Guid]::TryParse([string]$Receipt.dllMvid, [ref]$mvid) -or $mvid -eq [Guid]::Empty) {
        throw 'Development receipt has a malformed DLL MVID.'
    }
    if (-not (Test-Path -LiteralPath $package -PathType Leaf) -or
        (Get-KmgSha256 -Path $package) -cne $Receipt.packageSha256) {
        throw 'Development package hash does not match its receipt.'
    }
    $referenceFiles = @(Get-KmgOptionalProperty $Receipt.references 'files')
    $expectedReferences = @($script:KmgPrivateReferencePaths | ForEach-Object { $_.Replace('\', '/') })
    $observedReferences = @($referenceFiles | ForEach-Object { [string](Get-KmgOptionalProperty $_ 'relativePath') })
    if ($referenceFiles.Count -ne $expectedReferences.Count -or
        (($observedReferences -join "`n") -cne ($expectedReferences -join "`n")) -or
        @($referenceFiles | Where-Object { [string](Get-KmgOptionalProperty $_ 'sha256') -cnotmatch '^[0-9a-f]{64}$' }).Count -ne 0) {
        throw 'Development receipt lacks the exact private reference identity.'
    }
    $validation = $Receipt.validation
    if ((Get-KmgOptionalProperty $validation 'validator') -cne 'tools/validate_development_candidate.py' -or
        (Get-KmgOptionalProperty $validation 'purpose') -cne $Purpose -or
        (Get-KmgOptionalProperty $validation 'releaseQualification') -ne $false -or
        (Get-KmgOptionalProperty $validation 'baselineVersion') -cne $info.Version -or
        [int](Get-KmgOptionalProperty $validation 'identities') -le 0 -or
        -not (Test-Path -LiteralPath $layout.ValidationSummaryPath -PathType Leaf) -or
        (Get-KmgSha256 -Path $layout.ValidationSummaryPath) -cne
            (Get-KmgOptionalProperty $validation 'summarySha256')) {
        throw 'Development receipt lacks its exact development source validation.'
    }
    $tests = $Receipt.selectedTests
    if ([string]::IsNullOrWhiteSpace([string](Get-KmgOptionalProperty $tests 'filter')) -or
        [int](Get-KmgOptionalProperty $tests 'selected') -lt 1 -or
        [int](Get-KmgOptionalProperty $tests 'selected') -gt [int](Get-KmgOptionalProperty $tests 'registered') -or
        [int](Get-KmgOptionalProperty $tests 'failures') -ne 0 -or
        [string](Get-KmgOptionalProperty $tests 'passedNamesSha256') -cnotmatch '^[0-9a-f]{64}$' -or
        -not (Test-Path -LiteralPath $layout.TestLogPath -PathType Leaf) -or
        (Get-KmgSha256 -Path $layout.TestLogPath) -cne (Get-KmgOptionalProperty $tests 'logSha256')) {
        throw 'Development receipt lacks passing selected domain-test evidence.'
    }
    $observedTests = Read-KmgDomainTestEvidence -LogPath $layout.TestLogPath -Filter ([string]$tests.filter)
    if ($observedTests.selected -ne [int]$tests.selected -or
        $observedTests.registered -ne [int]$tests.registered -or
        $observedTests.passedNamesSha256 -cne [string]$tests.passedNamesSha256) {
        throw 'Development receipt test counts differ from its selected test log.'
    }
}

function Assert-KmgDevelopmentSourceBinding {
    <# A development candidate describes one exact clean commit; anything else
       is a stale candidate or receipt. #>
    param(
        [Parameter(Mandatory = $true)]$Receipt,
        [Parameter(Mandatory = $true)]$Git,
        [Parameter(Mandatory = $true)][string]$SourceStateSha256,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion
    )
    if (@($Git.Status).Count -ne 0 -or $Receipt.commit -cne $Git.Commit -or
        $Receipt.version -cne $ExpectedVersion -or
        $Receipt.sourceStateSha256 -cne $SourceStateSha256) {
        throw 'Development candidate is stale: it does not describe the exact current clean source state, commit and version.'
    }
}

function Assert-KmgDevelopmentDeploymentRecord {
    param(
        [Parameter(Mandatory = $true)]$Deployment,
        [Parameter(Mandatory = $true)][string]$ReceiptPath,
        [Parameter(Mandatory = $true)][string]$Purpose
    )
    if ((Get-KmgOptionalProperty $Deployment 'candidateKind') -cne 'development' -or
        (Get-KmgOptionalProperty $Deployment 'releaseQualification') -ne $false -or
        (Get-KmgOptionalProperty $Deployment 'developmentPurpose') -cne $Purpose -or
        (Get-KmgOptionalProperty $Deployment 'developmentReceiptSha256') -cne
            (Get-KmgSha256 -Path $ReceiptPath)) {
        throw 'Reusable deployment is not this development candidate''s deployment.'
    }
}

function Assert-KmgReleaseDeploymentRecord {
    param([Parameter(Mandatory = $true)]$Deployment)
    if ($null -ne (Get-KmgOptionalProperty $Deployment 'candidateKind')) {
        throw 'A development deployment cannot be reused by a release or qualification path.'
    }
}

function Read-KmgDevelopmentCandidate {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$Purpose
    )
    [void](Assert-KmgDevelopmentPurpose $Purpose)
    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    if (Test-Path -LiteralPath (Get-KmgPackageManifestPath -PackagePath $package)) {
        throw 'A release Build-Local candidate cannot be consumed as a development candidate.'
    }
    $receiptPath = Get-KmgDevelopmentReceiptPath -PackagePath $package
    if (-not (Test-Path -LiteralPath $receiptPath -PathType Leaf)) {
        throw "Development receipt is missing: $receiptPath"
    }
    $receipt = Get-Content -LiteralPath $receiptPath -Raw | ConvertFrom-Json
    Assert-KmgDevelopmentReceipt -Receipt $receipt -PackagePath $package `
        -RepositoryRoot $RepositoryRoot -Purpose $Purpose
    return $receipt
}

function Assert-KmgNoDevelopmentProof {
    <# Qualification refuses any proof produced under a development candidate. #>
    param([Parameter(Mandatory = $true)][string]$Path)
    if ([IO.File]::ReadAllText($Path) -match $script:KmgDevelopmentProofPattern) {
        throw "Development-candidate evidence cannot qualify a release: $Path"
    }
}
