# Explicit owner authorization on 2026-10-17: publish one unqualified alpha.
# Leaves stable release/qualification guards, product bytes and installation alone.
[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param(
    [string]$CandidateRepositoryRoot='C:/Dev/KingmakerGunslingerLab/worktrees/brown-fur-startup-order-20261017',
    [switch]$Publish,
    [switch]$ConfirmAlphaTestPublication
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
$alphaWhatIf=[bool]$WhatIfPreference;$WhatIfPreference=$false
$root=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
. (Join-Path $PSScriptRoot 'BrownFurAlpha.Common.ps1')
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $CandidateRepositoryRoot 'scripts/RuntimeHarness.Common.ps1')
function Read-AlphaNative([string]$Command,[string[]]$Arguments) {
    $output=@(& $Command @Arguments)
    if($LASTEXITCODE -ne 0){throw "Alpha command failed: $Command $($Arguments[0])"}
    return (($output | ForEach-Object {[string]$_}) -join "`n").Trim()
}
if($Publish -and -not $ConfirmAlphaTestPublication){throw 'Public alpha publication requires explicit -ConfirmAlphaTestPublication.'}
$git=Get-KmgGitState $root
if($git.Status.Count -ne 0 -or $git.Branch -cne 'codex/brown-fur-startup-order-20261017'){throw 'Alpha publisher requires the clean dedicated review branch.'}
Assert-KmgReleaseOrigin (Read-AlphaNative git @('-C',$root,'remote','get-url','origin'))
$package=Join-Path $CandidateRepositoryRoot 'artifacts/development-candidates/brown-fur-startup-order/913dfd6e8938/KingmakerGunslinger-0.0.157-development-brown-fur-startup-order-913dfd6e8938.zip'
$receipt=Read-KmgBuildLocalManifest -PackagePath $package -RepositoryRoot $CandidateRepositoryRoot -DevelopmentPurpose brown-fur-startup-order
Assert-KmgDevelopmentSourceBinding -Receipt $receipt -Git (Get-KmgGitState $CandidateRepositoryRoot) `
    -SourceStateSha256 (Get-KmgSourceStateFingerprint $CandidateRepositoryRoot) -ExpectedVersion 0.0.157
$plan=New-KmgBrownFurAlphaPlan $receipt
$notes=Join-Path $root 'docs/RELEASE-NOTES-BROWN-FUR-ALPHA-1.md'
if(-not(Test-Path -LiteralPath $notes -PathType Leaf)){throw 'Alpha notes are missing.'}
$nativePath='C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261017T2101466987536Z-disposable-brown-fur-native-cast/runtime-result.json'
if((Get-KmgSha256 $nativePath) -cne '2c9a1ddfced4d70ac538eac3c826eca6b37e1259d19017cb195209871af2ec4c'){throw 'Native alpha evidence identity changed.'}
$native=Get-Content -LiteralPath $nativePath -Raw|ConvertFrom-Json
if($native.gitCommit -cne $plan.productCommit -or $native.status -cne 'PASS' -or
    @($native.assertions).Count -ne 10 -or @($native.assertions|Where-Object status -cne 'PASS').Count -ne 0){throw 'Native alpha evidence does not match the product.'}
$repository=$plan.repository
$latest=Read-AlphaNative gh @('api',"repos/$repository/releases/latest",'--jq','.tag_name')
if($latest -cne 'v0.0.157'){throw 'Stable Latest changed; review before alpha publication.'}
$releases=(Read-AlphaNative gh @('release','list','--repo',$repository,'--limit','100','--json','tagName')) | ConvertFrom-Json
if(@($releases|Where-Object tagName -ceq $plan.tag).Count -ne 0){throw 'Alpha release already exists; never overwrite it.'}
$remoteTag=Read-AlphaNative git @('-C',$root,'ls-remote','--tags','origin',"refs/tags/$($plan.tag)")
if($remoteTag){throw 'Alpha tag already exists remotely; no ref will be replaced.'}
$directory=Join-Path $root 'artifacts/alpha-publication/v0.0.157-brown-fur-alpha.1'
$createArguments=@(Get-KmgBrownFurAlphaCreateArguments $plan $directory $notes)
$WhatIfPreference=$alphaWhatIf
if(-not $Publish -or -not $PSCmdlet.ShouldProcess($repository,'publish exact Brown-Fur alpha 1; prerelease, never Latest')) {
    Write-Host "Alpha preflight PASS: $($plan.tag); unchanged product $($plan.productCommit); no remote writes."
    return
}
$WhatIfPreference=$false
$remoteHead=Read-AlphaNative git @('-C',$root,'ls-remote','--heads','origin',"refs/heads/$($git.Branch)")
if(($remoteHead -split '\s+')[0] -cne $git.Commit){throw 'Push the clean review branch through the project guard before publication.'}
$null=Read-AlphaNative git @('-C',$root,'merge-base','--is-ancestor',$plan.productCommit,$git.Commit)
if(Test-Path -LiteralPath $directory){throw 'Alpha output already exists; inspect the prior attempt instead of overwriting it.'}
[void][IO.Directory]::CreateDirectory($directory)
$publicPackage=Join-Path $directory $plan.asset
Copy-Item -LiteralPath $package -Destination $publicPackage
if((Get-KmgSha256 $publicPackage) -cne $receipt.packageSha256){throw 'Alpha publication copy changed the tested package.'}
$manifest=[ordered]@{schemaVersion=1;candidateKind=$plan.candidateKind;publicationChannel=$plan.publicationChannel;releaseQualification=$false
    tag=$plan.tag;ummVersion=$receipt.version;productCommit=$plan.productCommit;documentationCommit=$git.Commit
    package=$plan.asset;packageSha256=$receipt.packageSha256;dllSha256=$receipt.dllSha256;dllMvid=$receipt.dllMvid
    selectedTests=83;registeredTests=2652;nativeAssertions=10;nativeEvidenceSha256=(Get-KmgSha256 $nativePath)
    nativeInputMode='mouse-keyboard';fullSuiteRun=$false;rebuild=$false
    gaps=@('saved Brown-Fur owner cold reconstruction','affected character save load','controller-mode loading','physical controller input')
    restoration='Original installation/settings restored; 95 protected saves unchanged.'}
$manifestPath=Join-Path $directory 'alpha-manifest.json'
$manifest|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $manifestPath -Encoding UTF8
@("$($receipt.packageSha256)  $($plan.asset)","$(Get-KmgSha256 $manifestPath)  alpha-manifest.json") |
    Set-Content -LiteralPath (Join-Path $directory 'SHA256SUMS.txt') -Encoding ASCII
$null=Read-AlphaNative git @('-C',$root,'tag','-a',$plan.tag,$plan.productCommit,'-m',$plan.title)
$null=Read-AlphaNative git @('-C',$root,'push','origin',"refs/tags/$($plan.tag)")
$peeled=Read-AlphaNative git @('-C',$root,'ls-remote','--tags','origin',"refs/tags/$($plan.tag)^{}")
if(($peeled -split '\s+')[0] -cne $plan.productCommit){throw 'Remote annotated tag does not resolve to the tested product.'}
$null=Read-AlphaNative gh $createArguments
# Keep the draft private until all three uploaded asset digests match.
$uploaded=(Read-AlphaNative gh @('release','view',$plan.tag,'--repo',$repository,'--json','isDraft,isPrerelease,assets'))|ConvertFrom-Json
if(-not $uploaded.isDraft -or -not $uploaded.isPrerelease -or @($uploaded.assets).Count -ne 3){throw 'Unexpected draft alpha state; left unpublished.'}
foreach($path in @($publicPackage,$manifestPath,(Join-Path $directory 'SHA256SUMS.txt'))) {
    $asset=@($uploaded.assets|Where-Object name -ceq ([IO.Path]::GetFileName($path)))
    if($asset.Count -ne 1 -or $asset[0].digest -cne ('sha256:'+(Get-KmgSha256 $path))){throw 'Uploaded alpha asset digest mismatch; left draft.'}
}
$null=Read-AlphaNative gh @('release','edit',$plan.tag,'--repo',$repository,'--draft=false','--prerelease','--latest=false')
$published=(Read-AlphaNative gh @('release','view',$plan.tag,'--repo',$repository,'--json','url,isDraft,isPrerelease,tagName'))|ConvertFrom-Json
if($published.isDraft -or -not $published.isPrerelease -or $published.tagName -cne $plan.tag){throw 'Public alpha metadata verification failed.'}
if((Read-AlphaNative gh @('api',"repos/$repository/releases/latest",'--jq','.tag_name')) -cne $latest){throw 'Stable Latest identity changed unexpectedly.'}
$download=Join-Path $directory 'download-verification';[void][IO.Directory]::CreateDirectory($download)
$null=Read-AlphaNative gh @('release','download',$plan.tag,'--repo',$repository,'--pattern',$plan.asset,'--dir',$download)
if((Get-KmgSha256 (Join-Path $download $plan.asset)) -cne $receipt.packageSha256){throw 'Downloaded public alpha ZIP differs from tested bytes.'}
Write-Host "Published alpha: $($published.url)"
Write-Host "Exact tested ZIP SHA-256: $($receipt.packageSha256)"
