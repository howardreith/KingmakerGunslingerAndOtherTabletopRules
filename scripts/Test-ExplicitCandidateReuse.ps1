[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
$manifest=[pscustomobject]@{ version='0.0.151';commit=('a'*40);sourceStateSha256=('b'*64) }
$git=[pscustomobject]@{ Commit=('a'*40);Status=@() }
$checks=0
Assert-KmgExplicitCandidateIdentity -Manifest $manifest -Git $git -SourceStateSha256 ('b'*64) -ExpectedVersion '0.0.151'
$checks++
foreach($case in @('version','commit','source','dirty')) {
    $trialManifest=[pscustomobject]@{ version='0.0.151';commit=('a'*40);sourceStateSha256=('b'*64) }
    $trialGit=[pscustomobject]@{ Commit=('a'*40);Status=@() }
    switch($case) {
        version { $trialManifest.version='0.0.150' }
        commit { $trialGit.Commit=('c'*40) }
        source { $trialManifest.sourceStateSha256=('c'*64) }
        dirty { $trialGit.Status=@('modified product') }
    }
    $rejected=$false
    try { Assert-KmgExplicitCandidateIdentity -Manifest $trialManifest -Git $trialGit -SourceStateSha256 ('b'*64) -ExpectedVersion '0.0.151' }
    catch { $rejected=$true }
    if(-not $rejected) { throw "Invalid candidate accepted: $case" }
    $checks++
}
$git.Status=@('modified product')
Assert-KmgExplicitCandidateIdentity -Manifest $manifest -Git $git -SourceStateSha256 ('b'*64) -ExpectedVersion '0.0.151' -AllowDirtyGit
$checks++
$manifest.commit=('c'*40)
$rejected=$false
try { Assert-KmgExplicitCandidateIdentity -Manifest $manifest -Git $git -SourceStateSha256 ('b'*64) -ExpectedVersion '0.0.151' -AllowDirtyGit }
catch { $rejected=$true }
if(-not $rejected) { throw 'AllowDirtyGit bypassed candidate identity.' }
$checks++
Write-Host "Explicit candidate reuse safety passed: $checks checks. No lease, deployment or game launch."

function Assert-ReproRefused([scriptblock]$Action, [string]$Pattern) {
    $message = $null
    try { & $Action } catch { $message = $_.Exception.Message }
    if (-not $message -or $message -notlike $Pattern) { throw "Expected refusal '$Pattern', got '$message'." }
    $script:checks++
}
$references = @([pscustomobject]@{relativePath='Assembly-CSharp.dll';sha256=('c'*64)})
$primary = [pscustomobject]@{version='0.0.169';commit=('a'*40);sourceStateSha256=('b'*64)
    completeDomainSuite=$true;reproducibilityBuild=$false;references=[pscustomobject]@{files=$references}}
$git = [pscustomobject]@{Commit=('a'*40);Status=@()}
$identityArgs = @{Manifest=$primary;Git=$git;SourceStateSha256=('b'*64);ExpectedVersion='0.0.169';References=$references}
Assert-KmgReleaseReproducibilityIdentity @identityArgs
$checks++
$primary.completeDomainSuite=$false
Assert-ReproRefused { Assert-KmgReleaseReproducibilityIdentity @identityArgs } '*complete-domain primary*'
$primary.completeDomainSuite=$true;$primary.reproducibilityBuild=$true
Assert-ReproRefused { Assert-KmgReleaseReproducibilityIdentity @identityArgs } '*complete-domain primary*'
$primary.reproducibilityBuild=$false
$identityArgs.References=@([pscustomobject]@{relativePath='Assembly-CSharp.dll';sha256=('d'*64)})
Assert-ReproRefused { Assert-KmgReleaseReproducibilityIdentity @identityArgs } '*reference identities differ*'
$identityArgs.References=$references;$git.Status=@('changed source')
Assert-ReproRefused { Assert-KmgReleaseReproducibilityIdentity @identityArgs } '*exact version, current source state*'
$git.Status=@();$primary.PSObject.Properties.Remove('completeDomainSuite')
Assert-ReproRefused { Assert-KmgReleaseReproducibilityIdentity @identityArgs } '*complete-domain primary*'

$builder = Join-Path $PSScriptRoot 'Build-Local.ps1'
Assert-ReproRefused { & $builder -PlanOnly -SkipDomainTests } '*explicit non-development reproducibility*'
Assert-ReproRefused { & $builder -PlanOnly -ReproducibilityBuild } '*explicit non-development reproducibility*'
Assert-ReproRefused { & $builder -PlanOnly -SkipDomainTests -ReproducibilityBuild -DevelopmentPurpose fixture -DevelopmentCompleteSuite } '*explicit non-development reproducibility*'
$plan = @(& $builder -PlanOnly -SkipDomainTests -ReproducibilityBuild)
if ($plan -notcontains 'reuse-primary-domain-suite' -or $plan -contains 'complete-domain-suite') { throw 'Reproducibility plan misstates domain selection.' }
$checks++

$boundary = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) ('kmg-release-repro-' + [Guid]::NewGuid().ToString('N'))))
$junction=$null
try {
    New-Item -ItemType Directory -Path $boundary | Out-Null
    $layoutArgs=@{RepositoryRoot=$boundary;Commit=('a'*40);Id='KingmakerGunslinger';Version='0.0.169'}
    $first=Get-KmgLocalCandidateLayout @layoutArgs
    $second=Get-KmgLocalCandidateLayout @layoutArgs -Reproducibility
    if ($first.Root -ceq $second.Root -or $first.ScratchRoot -cne $second.ScratchRoot) { throw 'Immutable outputs must differ while compiler paths remain identical.' }
    $checks++
    Assert-KmgReleaseScratch -RepositoryRoot $boundary -Layout $first
    $checks++
    New-Item -ItemType Directory -Path $first.ScratchRoot -Force | Out-Null
    'original compiler output' | Set-Content -LiteralPath (Join-Path $first.ScratchRoot 'output.dll')
    Assert-ReproRefused { Assert-KmgReleaseScratch -RepositoryRoot $boundary -Layout $first } '*incomplete attempt*'
    New-Item -ItemType Directory -Path $first.Root -Force | Out-Null
    Copy-Item -LiteralPath $first.ScratchRoot -Destination $first.ArchivedExactRoot -Recurse
    'immutable first package' | Set-Content -LiteralPath $first.PackagePath
    $firstHash=Get-KmgSha256 $first.PackagePath
    Assert-KmgReleaseScratch -RepositoryRoot $boundary -Layout $second -PrimaryArchive $first.ArchivedExactRoot -Reproducibility
    $checks++
    Assert-ReproRefused { Assert-KmgReleaseScratch -RepositoryRoot $boundary -Layout $first } '*final candidate output already exists*'
    'changed scratch' | Set-Content -LiteralPath (Join-Path $first.ScratchRoot 'output.dll')
    Assert-ReproRefused { Assert-KmgReleaseScratch -RepositoryRoot $boundary -Layout $second -PrimaryArchive $first.ArchivedExactRoot -Reproducibility } '*Scratch differs*'
    Copy-Item -LiteralPath (Join-Path $first.ArchivedExactRoot 'output.dll') -Destination $first.ScratchRoot -Force
    $escape=[pscustomobject]@{Root=$second.Root;ScratchRoot=(Join-Path (Split-Path $boundary) 'foreign-scratch')}
    Assert-ReproRefused { Assert-KmgReleaseScratch -RepositoryRoot $boundary -Layout $escape } '*escapes required root*'
    $target=Join-Path $boundary 'junction-target'
    New-Item -ItemType Directory -Path $target | Out-Null
    $junction=Join-Path (Split-Path $first.ScratchRoot) 'junction'
    New-Item -ItemType Junction -Path $junction -Target $target | Out-Null
    $linked=[pscustomobject]@{Root=$second.Root;ScratchRoot=(Join-Path $junction 'scratch')}
    Assert-ReproRefused { Assert-KmgReleaseScratch -RepositoryRoot $boundary -Layout $linked } '*reparse point*'
    [IO.Directory]::Delete($junction);$junction=$null
    New-Item -ItemType Directory -Path $second.Root | Out-Null
    Assert-ReproRefused { Assert-KmgReleaseScratch -RepositoryRoot $boundary -Layout $second -PrimaryArchive $first.ArchivedExactRoot -Reproducibility } '*final candidate output already exists*'
    if ((Get-KmgSha256 $first.PackagePath) -cne $firstHash) { throw 'First package changed during admission checks.' }
    $checks++
} finally {
    if ($junction -and (Test-Path -LiteralPath $junction)) { [IO.Directory]::Delete($junction) }
    $resolved=(Resolve-Path -LiteralPath $boundary).Path
    if ($resolved -ine $boundary -or -not $resolved.StartsWith([IO.Path]::GetTempPath(),[StringComparison]::OrdinalIgnoreCase)) { throw 'Synthetic cleanup boundary differs.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
Write-Host "Candidate/reproducibility safety PASS=$checks; bounded fixtures only, no compilation, deployment or game launch."
