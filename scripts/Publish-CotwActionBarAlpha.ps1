# Owner-authorized test alpha, 2026-10-17. Publish only the existing tested bytes;
# no stable-release promotion, rebuild, merge or game installation.
[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param([ValidateSet(2,3)][int]$AlphaVersion=2,[switch]$Publish,[switch]$ConfirmAlphaTestPublication)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
$alphaWhatIf=[bool]$WhatIfPreference;$WhatIfPreference=$false
$root=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
# Explicit immutable profiles only; this is not a generic development-package
# publication path. Alpha 2 remains pinned to its original source and evidence.
$profiles=@{
    2=@{Root='C:/Dev/KingmakerGunslingerLab/worktrees/cotw-controller-actionbar-load-20261017'
        Branch='codex/cotw-controller-actionbar-load-review-20261017';Purpose='cotw-controller-actionbar-load'
        Commit='92b442c06dd8d13f246ca006fdda6382b9eb6236'
        PackageHash='798ff15e5aca7b8860f1635c35e9f06260150dc2a91598081d673e1527b3f5f3'
        DllHash='e78d79cfb334f6d59c2948b6258128ce73334db59e8f7dba16a01860cc946801'
        Mvid='0fa8ccba-349e-47e1-a2ec-8b46d20ed69c';SelectedTests=6;Assertions=39
        Evidence='artifacts/actionbar-inspection/candidate-adapter.log'
        EvidenceHash='7c84b2b231d1cd4974913bd18a96ce5f8333c58fe102d5f4c11b6d23bdd03299'}
    3=@{Root='C:/Dev/KingmakerGunslingerLab/worktrees/cotw-actionbar-installer-20261017'
        Branch='codex/cotw-actionbar-installer-review-20261017';Purpose='cotw-actionbar-installer'
        Commit='39c60fb5e9167f1b2fe35a07b545fa2a2ebaac2e'
        PackageHash='6d4ed736a18cfdcac668eb5fad24e6b19001af635fd7e1fc53006834bd507aed'
        DllHash='d9fea590a604b81b5a14046ac88454a7eb8d4510e94e0b4ae5de082cbc2d606b'
        Mvid='c1a4d16b-efae-4ea8-b3ef-5966c3db9c6b';SelectedTests=33;Assertions=64
        Evidence='artifacts/installer-inspection/candidate-regressions.log'
        EvidenceHash='6ffc65eecf15988779c3088e9b03ea55165b17ab1eaf1ebe5f1ecd5d82567ba4'}
}
$profile=$profiles[$AlphaVersion]
$candidateRoot=$profile.Root
. (Join-Path $PSScriptRoot 'BrownFurAlpha.Common.ps1')
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
. (Join-Path $candidateRoot 'scripts/RuntimeHarness.Common.ps1')
if($Publish -and -not $ConfirmAlphaTestPublication){throw 'Explicit alpha publication confirmation required.'}
$git=Get-KmgGitState $root
if($git.Status.Count -ne 0 -or $git.Branch -cne $profile.Branch) {
    throw 'Publish only from the clean dedicated review branch.'
}
Assert-KmgReleaseOrigin (Read-AlphaNative git @('-C',$root,'remote','get-url','origin'))
$shortCommit=$profile.Commit.Substring(0,12)
$package=Join-Path $candidateRoot "artifacts/development-candidates/$($profile.Purpose)/$shortCommit/KingmakerGunslinger-0.0.157-development-$($profile.Purpose)-$shortCommit.zip"
$receipt=Read-KmgBuildLocalManifest -PackagePath $package -RepositoryRoot $candidateRoot -DevelopmentPurpose $profile.Purpose
Assert-KmgDevelopmentSourceBinding -Receipt $receipt -Git (Get-KmgGitState $candidateRoot) `
    -SourceStateSha256 (Get-KmgSourceStateFingerprint $candidateRoot) -ExpectedVersion 0.0.157
if($receipt.candidateKind -cne 'development' -or $receipt.releaseQualification -ne $false -or
    $receipt.purpose -cne $profile.Purpose -or $receipt.version -cne '0.0.157' -or
    $receipt.commit -cne $profile.Commit -or $receipt.packageSha256 -cne $profile.PackageHash -or
    $receipt.dllSha256 -cne $profile.DllHash -or $receipt.dllMvid -cne $profile.Mvid) {
    throw 'Only the exact owner-authorized controller-load alpha candidate is permitted.'
}
$adapterLog=Join-Path $candidateRoot $profile.Evidence
if((Get-KmgSha256 $adapterLog) -cne $profile.EvidenceHash -or
    (Get-Content -LiteralPath $adapterLog -Tail 1) -cne "Native-contract adapter assertions: PASS=$($profile.Assertions);FAIL=0;saveLoad=NOT_RUN") {
    throw 'Candidate-bound adapter evidence changed.'
}
$repository='howardreith/KingmakerGunslingerAndOtherTabletopRules'
$tag="v0.0.157-controller-load-alpha.$AlphaVersion"
$asset="KingmakerGunslinger-0.0.157-controller-load-alpha.$AlphaVersion.zip"
$notes=Join-Path $root "docs/RELEASE-NOTES-CONTROLLER-LOAD-ALPHA-$AlphaVersion.md"
if(-not(Test-Path -LiteralPath $notes -PathType Leaf)){throw 'Missing alpha notes.'}
$latest=Read-AlphaNative gh @('api',"repos/$repository/releases/latest",'--jq','.tag_name')
if($latest -cne 'v0.0.157'){throw 'Stable Latest changed; review before publishing.'}
$releases=(Read-AlphaNative gh @('release','list','--repo',$repository,'--limit','100','--json','tagName'))|ConvertFrom-Json
if(@($releases|Where-Object tagName -ceq $tag).Count -ne 0){throw 'Alpha release exists; never overwrite it.'}
if(Read-AlphaNative git @('-C',$root,'ls-remote','--tags','origin',"refs/tags/$tag")){throw 'Alpha tag exists; never move it.'}
$directory=Join-Path $root "artifacts/alpha-publication/$tag"
if(Test-Path -LiteralPath $directory){throw 'Alpha staging exists; inspect instead of overwriting it.'}
$WhatIfPreference=$alphaWhatIf
if(-not $Publish -or -not $PSCmdlet.ShouldProcess($repository,"publish exact controller-load alpha $AlphaVersion; prerelease, never Latest")) {
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
    selectedTests=$profile.SelectedTests;registeredTests=2652;adapterAssertions=$profile.Assertions
    adapterEvidenceSha256=(Get-KmgSha256 $adapterLog);nativeSaveLoad='NOT_RUN';nativeControllerInput='NOT_RUN'
    fullSuiteRun=$false;rebuild=$false;brownFurLifecycleRepairRetained=$true
    realInstallerExercised=($AlphaVersion -eq 3);startupReadinessConsumers=$(if($AlphaVersion -eq 3){5}else{0})
    companionUpdates='Update KMG/Tabletop Added Rules with this ZIP. No Buff Planner or CotW update is part of this repair.'
    gaps=@('affected save load','first null on affected character','controller loading and action-bar refresh',
        'nonempty metamagic combinations and storage/restoration fixtures','Buff Planner no-cast entry/exit')
    machineChanges='No game launch, installation, settings or save changes during publication.'} |
    ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
@("$($receipt.packageSha256)  $asset","$(Get-KmgSha256 $manifestPath)  alpha-manifest.json") |
    Set-Content -LiteralPath $checksumsPath -Encoding ASCII
$title="Tabletop Added Rules - Controller load repair (alpha $AlphaVersion)"
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
