<# Offline development-candidate contract. Exercises the actual receipt,
   reader, deployment, qualification and Build-Local code against bounded
   temporary fixtures: success, stale source/receipt, mismatched package/DLL,
   missing content, wrong purpose/location, development evidence offered as
   release qualification, argument refusal and the executed nested calls of
   the development build up to its exact-reference compiler boundary. It
   writes nothing outside its temporary boundary, deploys nothing and never
   launches the game. Identity collisions are fixtures of
   tools/test_development_candidate_validator.py. #>
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')

$boundary = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) (
    'kmg-development-candidate-' + [Guid]::NewGuid().ToString('N'))))
$oldFilter = $env:KMG_TEST_FILTER
$hadFilter = Test-Path Env:\KMG_TEST_FILTER
$oldTrace = $env:KMG_DEVELOPMENT_FIXTURE_TRACE
$failures = New-Object Collections.Generic.List[string]
$checks = 0

function Assert-That([bool]$Condition, [string]$Name) {
    $script:checks++
    if (-not $Condition) { $script:failures.Add($Name) }
}

function Get-Refusal([scriptblock]$Body) {
    try { & $Body | Out-Null } catch { return $_.Exception.Message }
    return $null
}

function Write-Json($Value, [string]$Path) {
    $Value | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Write-RunnerLog([string]$Path, [string]$Filter, [int]$Selected,
    [int]$Registered, [int]$Failed = 0) {
    $lines = @('Kingmaker Gunslinger domain, firearm-state, and combat-rule tests',
        "KMG_TEST_FILTER=$Filter; selected $Selected of $Registered registered tests. This is a focused run and is not a gate.")
    for ($index = 0; $index -lt $Selected - $Failed; $index++) { $lines += "PASS fixture.case-$index" }
    $lines += "Completed $Selected tests; failures=$Failed. Registered $Registered; elapsed 0.1s."
    Set-Content -LiteralPath $Path -Value $lines -Encoding UTF8
}

try {
    foreach ($version in @('0.0.156', '0.0.167', '0.0.200')) {
        $local = Get-KmgLocalCandidateLayout -RepositoryRoot $boundary -Commit ('a' * 40) -Id 'KingmakerGunslinger' -Version $version
        Assert-That ($local.PackagePath -ceq (Join-Path $boundary "artifacts\local-runtime\$version\aaaaaaaaaaaa\KingmakerGunslinger-$version-local-runtime.zip")) "local-candidate-layout-$version"
    }
    foreach ($bad in @(@('short','KingmakerGunslinger','0.0.167'), @(('a'*40),'../escape','0.0.167'), @(('a'*40),'KingmakerGunslinger','../escape'))) {
        Assert-That ((Get-Refusal { Get-KmgLocalCandidateLayout -RepositoryRoot $boundary -Commit $bad[0] -Id $bad[1] -Version $bad[2] }) -like '*exact commit*') 'invalid-candidate-layout-refused'
    }
    $repository = Join-Path $boundary 'repository'
    New-Item -ItemType Directory -Path $repository -Force | Out-Null
    '{"Id":"KingmakerGunslinger","Version":"0.0.156"}' |
        Set-Content -LiteralPath (Join-Path $repository 'Info.json') -Encoding UTF8
    $purpose = 'fixture-native-observation'
    $commit = 'a' * 40
    $layout = Get-KmgDevelopmentCandidateLayout -RepositoryRoot $repository -Purpose $purpose `
        -Commit $commit -Id 'KingmakerGunslinger' -Version '0.0.156'
    New-Item -ItemType Directory -Path $layout.Root -Force | Out-Null
    'fixture development package bytes' | Set-Content -LiteralPath $layout.PackagePath
    Write-RunnerLog $layout.TestLogPath 'alternate-summoning' 3 10
    '{"validator":"tools/validate_development_candidate.py"}' |
        Set-Content -LiteralPath $layout.ValidationSummaryPath
    $tests = Read-KmgDomainTestEvidence -LogPath $layout.TestLogPath -Filter 'alternate-summoning'
    $receipt = [ordered]@{
        schemaVersion = 1; candidateKind = 'development'; releaseQualification = $false
        generator = 'scripts/Build-Local.ps1 -DevelopmentPurpose'; purpose = $purpose
        commit = $commit; branch = 'codex/fixture'; sourceStateSha256 = ('b' * 64)
        version = '0.0.156'; packagePath = $layout.PackagePath
        packageSha256 = Get-KmgSha256 -Path $layout.PackagePath
        dllSha256 = ('c' * 64); dllMvid = '0f0e0d0c-0b0a-4908-8706-050403020100'
        firearmManifestSha256 = ('d' * 64); firearmSoundBankSha256 = ('e' * 64)
        references = [ordered]@{ files = @($script:KmgPrivateReferencePaths | ForEach-Object {
            [ordered]@{ relativePath = $_.Replace('\', '/'); sha256 = ('f' * 64) } }) }
        validation = [ordered]@{ validator = 'tools/validate_development_candidate.py'
            purpose = $purpose; releaseQualification = $false; baselineVersion = '0.0.156'
            identities = 10; summarySha256 = Get-KmgSha256 -Path $layout.ValidationSummaryPath }
        selectedTests = $tests
    }
    Write-Json $receipt $layout.ReceiptPath
    function Get-Receipt { Get-Content -LiteralPath $layout.ReceiptPath -Raw | ConvertFrom-Json }
    function Test-ReceiptRefusal([scriptblock]$Mutate, [string]$Name, [string]$Expected) {
        $trial = Get-Receipt
        & $Mutate $trial
        $message = Get-Refusal { Assert-KmgDevelopmentReceipt -Receipt $trial `
            -PackagePath $layout.PackagePath -RepositoryRoot $repository -Purpose $purpose }
        Assert-That ($null -ne $message -and $message -like $Expected) $Name
    }

    # Success path: the exact receipt, through the consumer's own reader.
    $read = Read-KmgDevelopmentCandidate -PackagePath $layout.PackagePath `
        -RepositoryRoot $repository -Purpose $purpose
    Assert-That ($read.packageSha256 -ceq $receipt.packageSha256 -and $read.purpose -ceq $purpose) 'valid-receipt-accepted'
    Assert-That ($tests.selected -eq 3 -and $tests.registered -eq 10 -and $tests.failures -eq 0) 'runner-evidence-parsed'
    Assert-That ([IO.Path]::GetFileName($layout.PackagePath) -ceq
        "KingmakerGunslinger-0.0.156-development-$purpose-aaaaaaaaaaaa.zip" -and
        $layout.Root -like '*\artifacts\development-candidates\*') 'development-only-location-and-name'

    # Wrong purpose, missing content, malformed identities and stale bindings.
    $message = Get-Refusal { Read-KmgDevelopmentCandidate -PackagePath $layout.PackagePath `
        -RepositoryRoot $repository -Purpose 'other-purpose' }
    Assert-That ($null -ne $message) 'other-purpose-refused'
    foreach ($field in @('selectedTests', 'references', 'validation', 'dllMvid', 'sourceStateSha256')) {
        Test-ReceiptRefusal { param($r) $r.PSObject.Properties.Remove($field) } "missing-$field-refused" '*lacks required content*'
    }
    Test-ReceiptRefusal { param($r) $r.candidateKind = 'release' } 'release-kind-refused' '*schema, kind or generator*'
    Test-ReceiptRefusal { param($r) $r.releaseQualification = $true } 'release-qualification-claim-refused' '*schema, kind or generator*'
    Test-ReceiptRefusal { param($r) $r.generator = 'scripts/Build-Local.ps1' } 'release-generator-refused' '*schema, kind or generator*'
    Test-ReceiptRefusal { param($r) $r.version = '0.0.157' } 'version-bump-refused' '*not the repository version*'
    Test-ReceiptRefusal { param($r) $r.dllSha256 = 'C' * 64 } 'malformed-dll-hash-refused' '*malformed dllSha256*'
    Test-ReceiptRefusal { param($r) $r.dllMvid = [Guid]::Empty.ToString() } 'empty-mvid-refused' '*malformed DLL MVID*'
    Test-ReceiptRefusal { param($r) $r.packageSha256 = '0' * 64 } 'mismatched-package-hash-refused' '*package hash*'
    Test-ReceiptRefusal { param($r) $r.commit = 'b' * 40 } 'other-commit-location-refused' '*development-only location*'
    Test-ReceiptRefusal { param($r) $r.references.files = @($r.references.files | Select-Object -Skip 1) } 'missing-reference-identity-refused' '*private reference identity*'
    Test-ReceiptRefusal { param($r) $r.validation.purpose = 'other-purpose' } 'validation-purpose-refused' '*development source validation*'
    Test-ReceiptRefusal { param($r) $r.selectedTests.selected = 2 } 'test-count-drift-refused' '*test counts differ*'
    Test-ReceiptRefusal { param($r) $r.selectedTests.failures = 1 } 'failed-tests-refused' '*selected domain-test evidence*'

    $originalPackage = [IO.File]::ReadAllBytes($layout.PackagePath)
    'tampered package' | Set-Content -LiteralPath $layout.PackagePath
    $message = Get-Refusal { Read-KmgDevelopmentCandidate -PackagePath $layout.PackagePath `
        -RepositoryRoot $repository -Purpose $purpose }
    Assert-That ($message -like '*package hash*') 'changed-package-bytes-refused'
    [IO.File]::WriteAllBytes($layout.PackagePath, $originalPackage)

    $originalLog = [IO.File]::ReadAllBytes($layout.TestLogPath)
    Add-Content -LiteralPath $layout.TestLogPath -Value 'PASS fixture.injected'
    $message = Get-Refusal { Read-KmgDevelopmentCandidate -PackagePath $layout.PackagePath `
        -RepositoryRoot $repository -Purpose $purpose }
    Assert-That ($message -like '*selected domain-test evidence*') 'changed-test-log-refused'
    [IO.File]::WriteAllBytes($layout.TestLogPath, $originalLog)

    # A complete receipt is still development-only and binds the actual full
    # runner log. Exercise the consumer, including typed metadata refusals.
    @('PASS fixture.one','PASS fixture.two','Completed 2 tests; failures=0. Registered 2; elapsed 0.1s.') | Set-Content $layout.TestLogPath
    $fullReceipt = Get-Receipt
    $fullReceipt.selectedTests = [pscustomobject](Read-KmgDomainTestEvidence -LogPath $layout.TestLogPath -CompleteSuite)
    Assert-KmgDevelopmentReceipt -Receipt $fullReceipt -PackagePath $layout.PackagePath -RepositoryRoot $repository -Purpose $purpose
    $checks++
    foreach ($badFlag in @('true', $false, 1)) {
        $fullReceipt.selectedTests.completeSuite = $badFlag
        Assert-That ($null -ne (Get-Refusal { Assert-KmgDevelopmentReceipt -Receipt $fullReceipt -PackagePath $layout.PackagePath -RepositoryRoot $repository -Purpose $purpose })) 'complete-receipt-invalid-flag-refused'
    }
    $fullReceipt.selectedTests.completeSuite = $true
    $fullReceipt.selectedTests.filter = 'all'
    Assert-That ($null -ne (Get-Refusal { Assert-KmgDevelopmentReceipt -Receipt $fullReceipt -PackagePath $layout.PackagePath -RepositoryRoot $repository -Purpose $purpose })) 'complete-receipt-filter-conflict-refused'
    [IO.File]::WriteAllBytes($layout.TestLogPath, $originalLog)

    $evidenceLog = Join-Path $boundary 'runner.log'
    foreach ($case in @(
        @{ Name = 'zero-selection'; Selected = 0; Registered = 10; Failed = 0 },
        @{ Name = 'failed-selection'; Selected = 3; Registered = 10; Failed = 1 },
        @{ Name = 'over-selection'; Selected = 11; Registered = 10; Failed = 0 })) {
        Write-RunnerLog $evidenceLog 'alternate-summoning' $case.Selected $case.Registered $case.Failed
        Assert-That ($null -ne (Get-Refusal { Read-KmgDomainTestEvidence -LogPath $evidenceLog -Filter 'alternate-summoning' })) "runner-$($case.Name)-refused"
    }
    Write-RunnerLog $evidenceLog 'other-filter' 3 10
    Assert-That ($null -ne (Get-Refusal { Read-KmgDomainTestEvidence -LogPath $evidenceLog -Filter 'alternate-summoning' })) 'runner-other-filter-refused'
    Assert-That ($null -ne (Get-Refusal { Read-KmgDomainTestEvidence -LogPath (Join-Path $boundary 'absent.log') -Filter 'x' })) 'runner-missing-log-refused'
    @('PASS fixture.one','PASS fixture.two','Completed 2 tests; failures=0. Registered 2; elapsed 0.1s.') | Set-Content $evidenceLog
    $completeEvidence = Read-KmgDomainTestEvidence -LogPath $evidenceLog -CompleteSuite
    Assert-That ($completeEvidence.completeSuite -eq $true -and $completeEvidence.selected -eq 2 -and
        $completeEvidence.registered -eq 2 -and [string]::IsNullOrEmpty($completeEvidence.filter)) 'complete-runner-evidence'
    foreach ($case in @(
        @{ Name='filtered-masquerade'; Lines=@('KMG_TEST_FILTER=all; selected 2 of 2 registered tests.','PASS a','PASS b','Completed 2 tests; failures=0. Registered 2; elapsed 0.1s.') },
        @{ Name='incomplete'; Lines=@('PASS a','Completed 1 tests; failures=0. Registered 2; elapsed 0.1s.') },
        @{ Name='zero'; Lines=@('Completed 0 tests; failures=0. Registered 0; elapsed 0.1s.') },
        @{ Name='duplicates'; Lines=@('PASS a','PASS a','Completed 2 tests; failures=0. Registered 2; elapsed 0.1s.') },
        @{ Name='failed'; Lines=@('PASS a','FAIL b','Completed 2 tests; failures=1. Registered 2; elapsed 0.1s.') },
        @{ Name='repeated-completion'; Lines=@('PASS a','Completed 1 tests; failures=0. Registered 1; elapsed 0.1s.','Completed 1 tests; failures=0. Registered 1; elapsed 0.1s.') })) {
        $case.Lines | Set-Content $evidenceLog
        Assert-That ($null -ne (Get-Refusal { Read-KmgDomainTestEvidence -LogPath $evidenceLog -CompleteSuite })) "complete-$($case.Name)-refused"
    }

    $git = [pscustomobject]@{ Commit = $commit; Status = @() }
    Assert-KmgDevelopmentSourceBinding -Receipt (Get-Receipt) -Git $git -SourceStateSha256 ('b' * 64) -ExpectedVersion '0.0.156'
    $checks++
    foreach ($case in @('commit', 'source', 'dirty', 'version')) {
        $trialGit = [pscustomobject]@{ Commit = $commit; Status = @() }
        $source = 'b' * 64; $version = '0.0.156'
        switch ($case) {
            commit { $trialGit.Commit = 'c' * 40 }
            source { $source = 'c' * 64 }
            dirty { $trialGit.Status = @(' M src/changed.cs') }
            version { $version = '0.0.157' }
        }
        Assert-That ((Get-Refusal { Assert-KmgDevelopmentSourceBinding -Receipt (Get-Receipt) -Git $trialGit `
            -SourceStateSha256 $source -ExpectedVersion $version }) -like '*stale*') "stale-$case-refused"
    }

    # Deployment records: the development deployment of this receipt only.
    $deployment = [pscustomobject]@{ candidateKind = 'development'; releaseQualification = $false
        developmentPurpose = $purpose; developmentReceiptSha256 = Get-KmgSha256 -Path $layout.ReceiptPath }
    Assert-KmgDevelopmentDeploymentRecord -Deployment $deployment -ReceiptPath $layout.ReceiptPath -Purpose $purpose
    $checks++
    foreach ($case in @('kind', 'purpose', 'receipt', 'release-record')) {
        $trial = [pscustomobject]@{ candidateKind = 'development'; releaseQualification = $false
            developmentPurpose = $purpose; developmentReceiptSha256 = Get-KmgSha256 -Path $layout.ReceiptPath }
        switch ($case) {
            kind { $trial.candidateKind = 'release' }
            purpose { $trial.developmentPurpose = 'other-purpose' }
            receipt { $trial.developmentReceiptSha256 = '0' * 64 }
            'release-record' { $trial = [pscustomobject]@{ schemaVersion = 2 } }
        }
        Assert-That ($null -ne (Get-Refusal { Assert-KmgDevelopmentDeploymentRecord -Deployment $trial `
            -ReceiptPath $layout.ReceiptPath -Purpose $purpose })) "deployment-$case-refused"
    }
    Assert-KmgReleaseDeploymentRecord -Deployment ([pscustomobject]@{ schemaVersion = 2 })
    $checks++
    Assert-That ((Get-Refusal { Assert-KmgReleaseDeploymentRecord -Deployment $deployment }) -like
        '*cannot be reused by a release*') 'development-deployment-refused-by-release-path'

    # Release readers refuse the development candidate; the development reader
    # refuses a release candidate.
    $message = Get-Refusal { Read-KmgBuildLocalManifest -PackagePath $layout.PackagePath -RepositoryRoot $repository }
    Assert-That ($message -like '*cannot be consumed as a release*') 'release-reader-refuses-development-candidate'
    $copied = Join-Path $repository 'artifacts\local-runtime\0.0.156\KingmakerGunslinger-0.0.156-development-copy.zip'
    New-Item -ItemType Directory -Path (Split-Path $copied) -Force | Out-Null
    Copy-Item -LiteralPath $layout.PackagePath -Destination $copied
    $message = Get-Refusal { Read-KmgBuildLocalManifest -PackagePath $copied -RepositoryRoot $repository }
    Assert-That ($message -like '*cannot be consumed as a release*') 'renamed-development-copy-refused'
    $releasePackage = Join-Path $repository 'artifacts\local-runtime\0.0.156\KingmakerGunslinger-0.0.156-local-runtime.zip'
    'fixture release package' | Set-Content -LiteralPath $releasePackage
    '{"generator":"scripts/Build-Local.ps1"}' | Set-Content -LiteralPath (Get-KmgPackageManifestPath -PackagePath $releasePackage)
    $message = Get-Refusal { Read-KmgBuildLocalManifest -PackagePath $releasePackage -RepositoryRoot $repository -DevelopmentPurpose $purpose }
    Assert-That ($message -like '*release Build-Local candidate cannot be consumed as a development*') 'development-reader-refuses-release-candidate'
    $moved = Join-Path $repository 'artifacts\elsewhere\KingmakerGunslinger-0.0.156-x.zip'
    New-Item -ItemType Directory -Path (Split-Path $moved) -Force | Out-Null
    Copy-Item -LiteralPath $layout.PackagePath -Destination $moved
    Copy-Item -LiteralPath $layout.ReceiptPath -Destination (Get-KmgDevelopmentReceiptPath -PackagePath $moved)
    $message = Get-Refusal { Read-KmgDevelopmentCandidate -PackagePath $moved -RepositoryRoot $repository -Purpose $purpose }
    Assert-That ($message -like '*development-only location*') 'moved-development-candidate-refused'
    Assert-That ((Get-Refusal { Read-KmgDevelopmentCandidate -PackagePath $moved -RepositoryRoot $repository -Purpose 'Bad Purpose' }) -like '*Development purpose*') 'malformed-purpose-refused'

    # Development evidence offered as release qualification.
    $proofRoot = Join-Path $boundary 'proofs'
    New-Item -ItemType Directory -Path $proofRoot | Out-Null
    $releaseProof = Join-Path $proofRoot 'release-runtime-evidence.json'
    '{"schemaVersion":1,"packageSha256":"x"}' | Set-Content -LiteralPath $releaseProof
    $developmentProof = Join-Path $proofRoot 'development-runtime-evidence.json'
    Write-Json ([ordered]@{ schemaVersion = 1; candidateKind = 'development'; releaseQualification = $false }) $developmentProof
    function New-Ledger([string[]]$Paths) {
        [pscustomobject]@{ coverage = @([pscustomobject]@{ proofs = @($Paths | ForEach-Object {
            [pscustomobject]@{ path = $_; sha256 = Get-KmgSha256 -Path $_ } }) }) }
    }
    Assert-KmgClosureProofFiles -Ledger (New-Ledger @($releaseProof))
    $checks++
    Assert-That ((Get-Refusal { Assert-KmgClosureProofFiles -Ledger (New-Ledger @($releaseProof, $developmentProof)) }) -like
        '*cannot qualify a release*') 'development-evidence-refused-as-qualification'

    # Build-Local argument refusals, before any machine or child is touched.
    $buildLocal = Join-Path $PSScriptRoot 'Build-Local.ps1'
    foreach ($case in @(
        @{ Name = 'purpose-without-filter'; Arguments = @{ DevelopmentPurpose = $purpose; PlanOnly = $true } },
        @{ Name = 'filter-without-purpose'; Arguments = @{ DevelopmentTestFilter = 'x'; PlanOnly = $true } },
        @{ Name = 'blank-filter'; Arguments = @{ DevelopmentPurpose = $purpose; DevelopmentTestFilter = ' ; '; PlanOnly = $true } },
        @{ Name = 'malformed-purpose'; Arguments = @{ DevelopmentPurpose = 'Release'; DevelopmentTestFilter = 'x'; PlanOnly = $true } },
        @{ Name = 'skip-domain-tests'; Arguments = @{ DevelopmentPurpose = $purpose; DevelopmentTestFilter = 'x'; SkipDomainTests = $true; PlanOnly = $true } })) {
        $arguments = $case.Arguments
        Assert-That ($null -ne (Get-Refusal { & $buildLocal @arguments })) "build-local-$($case.Name)-refused"
    }
    $developmentPlan = @(& $buildLocal -PlanOnly -DevelopmentPurpose $purpose -DevelopmentTestFilter 'alternate-summoning')
    $releasePlan = @(& $buildLocal -PlanOnly)
    $completePlan = @(& $buildLocal -PlanOnly -DevelopmentPurpose $purpose -DevelopmentCompleteSuite)
    Assert-That (($completePlan -join ',') -ceq 'development-repository-wrapper,complete-domain-suite,exact-reference-release-build,package-assembly,strict-package-validation,development-receipt') 'complete-development-plan'
    foreach ($arguments in @(
        @{DevelopmentCompleteSuite=$true;PlanOnly=$true},
        @{DevelopmentPurpose=$purpose;DevelopmentCompleteSuite=$true;DevelopmentTestFilter='x';PlanOnly=$true},
        @{DevelopmentPurpose=$purpose;DevelopmentCompleteSuite=$true;DevelopmentTestFilter='';PlanOnly=$true},
        @{DevelopmentPurpose=$purpose;DevelopmentCompleteSuite=$true;SkipDomainTests=$true;PlanOnly=$true})) {
        Assert-That ($null -ne (Get-Refusal { & $buildLocal @arguments })) 'complete-argument-conflict-refused'
    }
    Assert-That (($developmentPlan -join ',') -ceq 'development-repository-wrapper,selected-domain-tests,exact-reference-release-build,package-assembly,strict-package-validation,development-receipt') 'development-plan'
    Assert-That (($releasePlan -join ',') -ceq 'repository-wrapper,complete-domain-suite,exact-reference-release-build,package-assembly,strict-package-validation') 'release-plan-unchanged'

    # The executed nested calls of the actual development build, in a bounded
    # Git fixture whose children trace their arguments. The exact-reference
    # compiler is the boundary: it fails on purpose, so no package is built.
    $fixture = Join-Path $boundary 'build-fixture'
    $fixtureScripts = Join-Path $fixture 'scripts'
    New-Item -ItemType Directory -Path $fixtureScripts, (Join-Path $fixture 'tools') -Force | Out-Null
    foreach ($name in @('Build-Local.ps1', 'RuntimeHarness.Common.ps1', 'common.ps1',
        'RuntimeCoordination.Common.ps1', 'DevelopmentCandidate.Common.ps1',
        'ReferenceProvenance.Common.ps1', 'KmgGate.Common.ps1')) {
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $fixtureScripts
    }
    New-Item -ItemType Directory -Path (Join-Path $fixtureScripts 'compatibility') | Out-Null
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'compatibility\CompatibilityProfile.Common.ps1') `
        -Destination (Join-Path $fixtureScripts 'compatibility')
    # The fixture carries a released version outside the concluded campaign pair.
    $fixtureVersion = '0.0.156'
    ([ordered]@{ Id = 'KingmakerGunslinger'; Version = $fixtureVersion } | ConvertTo-Json -Compress) |
        Set-Content (Join-Path $fixture 'Info.json')
    "artifacts/`n" | Set-Content (Join-Path $fixture '.gitignore')
    $env:KMG_DEVELOPMENT_FIXTURE_TRACE = Join-Path $boundary 'build-trace.jsonl'
    @'
param([string]$RepositoryRoot, [string]$DevelopmentPurpose, [string]$DevelopmentSummaryPath)
@{ event = 'repository-wrapper'; purpose = $DevelopmentPurpose; summary = $DevelopmentSummaryPath } |
    ConvertTo-Json -Compress | Add-Content $env:KMG_DEVELOPMENT_FIXTURE_TRACE
if ($DevelopmentSummaryPath) { '{"validator":"tools/validate_development_candidate.py"}' | Set-Content $DevelopmentSummaryPath }
# Exercise the same nested missing-reference probe against this isolated copy.
$missing=Join-Path (Split-Path -Parent $PSScriptRoot) 'absent-references'
$message=$null
try { & (Join-Path $PSScriptRoot 'Build-Local.ps1') -ReferenceBundleDir $missing -KingmakerInstallDir $missing }
catch { $message=$_.Exception.Message }
if($message -cne ('Qualified private reference bundle is missing: '+$missing)){throw 'Nested offline reference refusal failed for the wrong reason.'}

'@ | Set-Content (Join-Path $fixtureScripts 'validate-repository.ps1')
    @'
param([string]$Configuration, [string]$MSBuildPath, [switch]$Clean, [switch]$SkipRepositoryValidation, [string]$RunnerLogPath)
@{ event = 'test-domain'; configuration = $Configuration; clean = [bool]$Clean; skipWrapper = [bool]$SkipRepositoryValidation
   filter = $env:KMG_TEST_FILTER; runnerLog = $RunnerLogPath } | ConvertTo-Json -Compress | Add-Content $env:KMG_DEVELOPMENT_FIXTURE_TRACE
$failed = if ($env:KMG_DEVELOPMENT_FIXTURE_FAIL) { 1 } else { 0 }
$registered = if ($env:KMG_TEST_FILTER) { 9 } else { 2 }
$lines = @('PASS fixture.one', 'PASS fixture.two', "Completed 2 tests; failures=$failed. Registered $registered; elapsed 0.1s.")
if ($env:KMG_TEST_FILTER) { $lines = @('KMG_TEST_FILTER=' + $env:KMG_TEST_FILTER + '; selected 2 of 9 registered tests. This is a focused run and is not a gate.') + $lines }
$lines | Set-Content $RunnerLogPath
'' | Set-Content -NoNewline "$RunnerLogPath.stderr"
'@ | Set-Content (Join-Path $fixtureScripts 'test-domain.ps1')
    @'
import json, os, sys
with open(os.environ["KMG_DEVELOPMENT_FIXTURE_TRACE"], "a", encoding="utf-8") as trace:
    trace.write(json.dumps({"event": "exact-build", "arguments": sys.argv[1:]}) + "\n")
sys.exit(3)
'@ | Set-Content (Join-Path $fixture 'tools\build_mod_from_private_references.py')
    $bundle = Join-Path $boundary 'references'
    $install = Join-Path $boundary 'install'
    foreach ($relative in $script:KmgPrivateReferencePaths) {
        foreach ($path in @((Join-Path $bundle "Managed\$relative"), (Join-Path $install "Kingmaker_Data\Managed\$relative"))) {
            New-Item -ItemType Directory -Path (Split-Path $path) -Force | Out-Null
            "reference $relative" | Set-Content -LiteralPath $path
        }
    }
    $msbuildFixture = Join-Path $boundary 'msbuild-fixture.exe'
    'not executed' | Set-Content $msbuildFixture
    & git -C $fixture init --quiet
    & git -C $fixture add -A
    & git -C $fixture -c user.name=DevelopmentFixture -c user.email=fixture@example.invalid commit --quiet -m fixture
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the bounded Git fixture.' }
    $fixtureBuild = Join-Path $fixtureScripts 'Build-Local.ps1'
    $buildArguments = @{ DevelopmentPurpose = $purpose; DevelopmentTestFilter = 'selected-behavior'
        ReferenceBundleDir = $bundle; KingmakerInstallDir = $install; MSBuildPath = $msbuildFixture
        ExpectedVersion = $fixtureVersion }
    $env:KMG_TEST_FILTER = 'prior'
    $buildArguments.ExpectedVersion = '0.0.157'
    $message = Get-Refusal { & $fixtureBuild @buildArguments }
    Assert-That ($message -like '*Candidate version differs*' -and
        -not (Test-Path -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE)) 'wrong-selected-version-refused-before-children'
    $buildArguments.ExpectedVersion = $fixtureVersion
    $message = Get-Refusal { & $fixtureBuild @buildArguments }
    if (-not (Test-Path -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE)) {
        throw "The fixture development build stopped before its children: $message"
    }
    $trace = @(Get-Content -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE | ForEach-Object { $_ | ConvertFrom-Json })
    $fixtureCommit = (& git -C $fixture rev-parse HEAD).Trim()
    $fixtureLayout = Get-KmgDevelopmentCandidateLayout -RepositoryRoot $fixture -Purpose $purpose `
        -Commit $fixtureCommit -Id 'KingmakerGunslinger' -Version $fixtureVersion
    $wrappers = @($trace | Where-Object event -eq 'repository-wrapper')
    $runs = @($trace | Where-Object event -eq 'test-domain')
    $builds = @($trace | Where-Object event -eq 'exact-build')
    Assert-That ($message -like '*Exact-reference Release build failed with exit code 3*') "build-reached-compiler-boundary ($message)"
    Assert-That ($wrappers.Count -eq 1 -and $wrappers[0].purpose -ceq $purpose -and
        $wrappers[0].summary -ceq $fixtureLayout.ValidationSummaryPath) 'build-ran-development-wrapper-once'
    Assert-That ($runs.Count -eq 1 -and $runs[0].filter -ceq 'selected-behavior' -and $runs[0].clean -and
        $runs[0].skipWrapper -and $runs[0].configuration -ceq 'Release' -and
        $runs[0].runnerLog -ceq $fixtureLayout.TestLogPath) 'build-ran-selected-tests-once'
    $outputIndex = if ($builds.Count -eq 1) { [Array]::IndexOf([string[]]$builds[0].arguments, '--output-dir') } else { -1 }
    Assert-That ($builds.Count -eq 1 -and $outputIndex -ge 0 -and
        $builds[0].arguments[$outputIndex + 1] -ceq $fixtureLayout.ExactBuildRoot) 'build-used-development-output'
    Assert-That (-not (Test-Path (Join-Path $fixture 'artifacts\local-runtime'))) 'build-left-release-location-untouched'
    Assert-That ($env:KMG_TEST_FILTER -ceq 'prior') 'build-restored-filter'

    $firstAttemptLogHash = (Get-FileHash -LiteralPath $fixtureLayout.TestLogPath).Hash
    Remove-Item -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE
    $env:KMG_DEVELOPMENT_FIXTURE_FAIL = '1'
    $message = Get-Refusal { & $fixtureBuild @buildArguments }
    Remove-Item Env:\KMG_DEVELOPMENT_FIXTURE_FAIL
    $retainedAttempts = @(Get-ChildItem -LiteralPath (Split-Path $fixtureLayout.Root) -Directory |
        Where-Object Name -like ($fixtureCommit.Substring(0, 12) + '.failed-*'))
    Assert-That ($retainedAttempts.Count -eq 1 -and
        (Get-FileHash -LiteralPath (Join-Path $retainedAttempts[0].FullName 'selected-domain-tests.log')).Hash -ceq $firstAttemptLogHash) 'retry-retains-original-failed-attempt-log'
    $trace = @(Get-Content -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE | ForEach-Object { $_ | ConvertFrom-Json })
    Assert-That ($message -like '*empty, failed or inconsistent*' -and
        @($trace | Where-Object event -eq 'exact-build').Count -eq 0) 'failed-selection-stops-before-build'

    Remove-Item -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE
    $buildArguments.Remove('DevelopmentTestFilter')
    $buildArguments.DevelopmentCompleteSuite = $true
    $message = Get-Refusal { & $fixtureBuild @buildArguments }
    $trace = @(Get-Content -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE | ForEach-Object { $_ | ConvertFrom-Json })
    $runs = @($trace | Where-Object event -eq 'test-domain')
    Assert-That ($message -like '*Exact-reference Release build failed with exit code 3*' -and
        $runs.Count -eq 1 -and [string]::IsNullOrEmpty($runs[0].filter) -and
        @($trace | Where-Object event -eq 'repository-wrapper').Count -eq 1 -and
        @($trace | Where-Object event -eq 'exact-build').Count -eq 1 -and $env:KMG_TEST_FILTER -ceq 'prior') 'complete-build-clears-and-restores-filter-once'
    $buildArguments.Remove('DevelopmentCompleteSuite')
    $buildArguments.DevelopmentTestFilter = 'selected-behavior'

    Remove-Item -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE
    'dirty' | Set-Content (Join-Path $fixture 'untracked-change.txt')
    $message = Get-Refusal { & $fixtureBuild @buildArguments }
    Assert-That ($message -like '*exactly clean commit*' -and
        -not (Test-Path -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE)) 'dirty-source-refused-before-children'
    Remove-Item -LiteralPath (Join-Path $fixture 'untracked-change.txt')

    New-Item -ItemType Directory -Path $fixtureLayout.Root -Force | Out-Null
    '{}' | Set-Content -LiteralPath $fixtureLayout.ReceiptPath
    $message = Get-Refusal { & $fixtureBuild @buildArguments }
    Assert-That ($message -like '*already exists and is immutable*' -and
        -not (Test-Path -LiteralPath $env:KMG_DEVELOPMENT_FIXTURE_TRACE)) 'existing-candidate-not-overwritten'

    if ($failures.Count -ne 0) { throw ('Development-candidate contract failures: ' + ($failures -join ', ')) }
    Write-Host "Development candidate contract: PASS=$checks FAIL=0. Offline fixtures only; no deployment, lease or game launch."
} finally {
    if ($hadFilter) { $env:KMG_TEST_FILTER = $oldFilter } else { Remove-Item Env:\KMG_TEST_FILTER -ErrorAction SilentlyContinue }
    $env:KMG_DEVELOPMENT_FIXTURE_TRACE = $oldTrace
    Remove-Item Env:\KMG_DEVELOPMENT_FIXTURE_FAIL -ErrorAction SilentlyContinue
    $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
    if (-not $boundary.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup path.' }
    if (Test-Path -LiteralPath $boundary) { Remove-Item -LiteralPath $boundary -Recurse -Force }
}
