# Owner-authorized test alpha, 2026-10-17. Publish only the existing tested bytes;
# no stable-release promotion, rebuild, merge or game installation.
[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param([switch]$Publish,[switch]$ConfirmAlphaTestPublication)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
$alphaWhatIf=[bool]$WhatIfPreference;$WhatIfPreference=$false
$root=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$candidateRoot='C:/Dev/KingmakerGunslingerLab/worktrees/cotw-controller-actionbar-load-20261017'
. (Join-Path $PSScriptRoot 'BrownFurAlpha.Common.ps1')
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $candidateRoot 'scripts/RuntimeHarness.Common.ps1')
if($Publish -and -not $ConfirmAlphaTestPublication){throw 'Explicit alpha publication confirmation required.'}
$git=Get-KmgGitState $root
if($git.Status.Count -ne 0 -or $git.Branch -cne 'codex/cotw-controller-actionbar-load-review-20261017') {
    throw 'Publish only from the clean dedicated review branch.'
}
Assert-KmgReleaseOrigin (Read-AlphaNative git @('-C',$root,'remote','get-url','origin'))
$package=Join-Path $candidateRoot 'artifacts/development-candidates/cotw-controller-actionbar-load/92b442c06dd8/KingmakerGunslinger-0.0.157-development-cotw-controller-actionbar-load-92b442c06dd8.zip'
$receipt=Read-KmgBuildLocalManifest -PackagePath $package -RepositoryRoot $candidateRoot -DevelopmentPurpose cotw-controller-actionbar-load
Assert-KmgDevelopmentSourceBinding -Receipt $receipt -Git (Get-KmgGitState $candidateRoot) `
    -SourceStateSha256 (Get-KmgSourceStateFingerprint $candidateRoot) -ExpectedVersion 0.0.157
if($receipt.candidateKind -cne 'development' -or $receipt.releaseQualification -ne $false -or
    $receipt.purpose -cne 'cotw-controller-actionbar-load' -or $receipt.version -cne '0.0.157' -or
    $receipt.commit -cne '92b442c06dd8d13f246ca006fdda6382b9eb6236' -or
    $receipt.packageSha256 -cne '798ff15e5aca7b8860f1635c35e9f06260150dc2a91598081d673e1527b3f5f3' -or
    $receipt.dllSha256 -cne 'e78d79cfb334f6d59c2948b6258128ce73334db59e8f7dba16a01860cc946801' -or
    $receipt.dllMvid -cne '0fa8ccba-349e-47e1-a2ec-8b46d20ed69c') {
    throw 'Only the exact owner-authorized controller-load alpha candidate is permitted.'
}
$adapterLog=Join-Path $candidateRoot 'artifacts/actionbar-inspection/candidate-adapter.log'
if((Get-KmgSha256 $adapterLog) -cne '7c84b2b231d1cd4974913bd18a96ce5f8333c58fe102d5f4c11b6d23bdd03299' -or
    (Get-Content -LiteralPath $adapterLog -Tail 1) -cne 'Native-contract adapter assertions: PASS=39;FAIL=0;saveLoad=NOT_RUN') {
    throw 'Candidate-bound adapter evidence changed.'
}
$repository='howardreith/KingmakerGunslingerAndOtherTabletopRules'
$tag='v0.0.157-controller-load-alpha.2'
$asset='KingmakerGunslinger-0.0.157-controller-load-alpha.2.zip'
$notes=Join-Path $root 'docs/RELEASE-NOTES-CONTROLLER-LOAD-ALPHA-2.md'
if(-not(Test-Path -LiteralPath $notes -PathType Leaf)){throw 'Missing alpha notes.'}
$latest=Read-AlphaNative gh @('api',"repos/$repository/releases/latest",'--jq','.tag_name')
if($latest -cne 'v0.0.157'){throw 'Stable Latest changed; review before publishing.'}
$releases=(Read-AlphaNative gh @('release','list','--repo',$repository,'--limit','100','--json','tagName'))|ConvertFrom-Json
if(@($releases|Where-Object tagName -ceq $tag).Count -ne 0){throw 'Alpha release exists; never overwrite it.'}
if(Read-AlphaNative git @('-C',$root,'ls-remote','--tags','origin',"refs/tags/$tag")){throw 'Alpha tag exists; never move it.'}
$directory=Join-Path $root "artifacts/alpha-publication/$tag"
if(Test-Path -LiteralPath $directory){throw 'Alpha staging exists; inspect instead of overwriting it.'}
$WhatIfPreference=$alphaWhatIf
if(-not $Publish -or -not $PSCmdlet.ShouldProcess($repository,'publish exact controller-load alpha 2; prerelease, never Latest')) {
    Write-Host "Alpha preflight PASS: $tag; product $($receipt.commit); native save load NOT_RUN; no remote writes."
    return
}
$WhatIfPreference=$false
$remoteHead=Read-AlphaNative git @('-C',$root,'ls-remote','--heads','origin',"refs/heads/$($git.Branch)")
if(($remoteHead -split '\s+')[0] -cne $git.Commit){throw 'Guarded-push the current clean review branch first.'}
$null=Read-AlphaNative git @('-C',$root,'merge-base','--is-ancestor',$receipt.commit,$git.Commit)
[void][IO.Directory]::CreateDirectory($directory)
$publicPackage=Join-Path $directory $asset
Copy-Item -LiteralPath $package -Destination $publicPackage
if((Get-KmgSha256 $publicPackage) -cne $receipt.packageSha256){throw 'Publication copy changed tested bytes.'}
$manifestPath=Join-Path $directory 'alpha-manifest.json'
$checksumsPath=Join-Path $directory 'SHA256SUMS.txt'
[ordered]@{schemaVersion=1;candidateKind='development';publicationChannel='alpha-test';releaseQualification=$false
    tag=$tag;ummVersion=$receipt.version;productCommit=$receipt.commit;documentationCommit=$git.Commit
    package=$asset;packageSha256=$receipt.packageSha256;dllSha256=$receipt.dllSha256;dllMvid=$receipt.dllMvid
    selectedTests=6;registeredTests=2652;adapterAssertions=39
    adapterEvidenceSha256=(Get-KmgSha256 $adapterLog);nativeSaveLoad='NOT_RUN';nativeControllerInput='NOT_RUN'
    fullSuiteRun=$false;rebuild=$false;brownFurLifecycleRepairRetained=$true
    companionUpdates='Update KMG/Tabletop Added Rules with this ZIP. No Buff Planner or CotW update is part of this repair.'
    gaps=@('affected save load','first null on affected character','controller loading and action-bar refresh',
        'nonempty metamagic combinations and storage/restoration fixtures','Buff Planner no-cast entry/exit')
    machineChanges='No game launch, installation, settings or save changes during publication.'} |
    ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
@("$($receipt.packageSha256)  $asset","$(Get-KmgSha256 $manifestPath)  alpha-manifest.json") |
    Set-Content -LiteralPath $checksumsPath -Encoding ASCII
$title='Tabletop Added Rules - Controller load repair (alpha 2)'
$null=Read-AlphaNative git @('-C',$root,'tag','-a',$tag,$receipt.commit,'-m',$title)
$null=Read-AlphaNative git @('-C',$root,'push','origin',"refs/tags/$tag")
$peeled=Read-AlphaNative git @('-C',$root,'ls-remote','--tags','origin',"refs/tags/$tag^{}")
if(($peeled -split '\s+')[0] -cne $receipt.commit){throw 'Remote tag product mismatch.'}
$null=Read-AlphaNative gh @('release','create',$tag,$publicPackage,$checksumsPath,$manifestPath,
    '--repo',$repository,'--title',$title,'--notes-file',$notes,'--verify-tag','--target',$receipt.commit,
    '--prerelease','--latest=false','--draft')
$uploaded=(Read-AlphaNative gh @('release','view',$tag,'--repo',$repository,'--json','isDraft,isPrerelease,assets'))|ConvertFrom-Json
if(-not $uploaded.isDraft -or -not $uploaded.isPrerelease -or @($uploaded.assets).Count -ne 3){throw 'Unexpected draft state; left unpublished.'}
foreach($path in @($publicPackage,$manifestPath,$checksumsPath)) {
    $uploadedAsset=@($uploaded.assets|Where-Object name -ceq ([IO.Path]::GetFileName($path)))
    if($uploadedAsset.Count -ne 1 -or $uploadedAsset[0].digest -cne ('sha256:'+(Get-KmgSha256 $path))) {
        throw 'Uploaded asset digest mismatch; left draft.'
    }
}
$null=Read-AlphaNative gh @('release','edit',$tag,'--repo',$repository,'--draft=false','--prerelease','--latest=false')
$published=(Read-AlphaNative gh @('release','view',$tag,'--repo',$repository,'--json','url,isDraft,isPrerelease,tagName'))|ConvertFrom-Json
if($published.isDraft -or -not $published.isPrerelease -or $published.tagName -cne $tag){throw 'Published alpha metadata mismatch.'}
if((Read-AlphaNative gh @('api',"repos/$repository/releases/latest",'--jq','.tag_name')) -cne $latest){throw 'Stable Latest changed unexpectedly.'}
$download=Join-Path $directory 'download-verification';[void][IO.Directory]::CreateDirectory($download)
$null=Read-AlphaNative gh @('release','download',$tag,'--repo',$repository,'--pattern',$asset,'--dir',$download)
if((Get-KmgSha256 (Join-Path $download $asset)) -cne $receipt.packageSha256){throw 'Downloaded alpha differs from tested ZIP.'}
Write-Host "Published alpha: $($published.url)"
Write-Host "ZIP SHA-256: $($receipt.packageSha256)"
