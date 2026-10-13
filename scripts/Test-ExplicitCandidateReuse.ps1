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
