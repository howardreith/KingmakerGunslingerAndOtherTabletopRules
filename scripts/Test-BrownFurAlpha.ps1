[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'BrownFurAlpha.Common.ps1')
. (Join-Path $PSScriptRoot 'DevelopmentCandidate.Common.ps1')
$receipt=[pscustomobject]@{candidateKind='development';releaseQualification=$false
    purpose='brown-fur-startup-order';version='0.0.157'
    commit='913dfd6e89386ff233849f06f902ca8ea2de239e'
    packageSha256='d7cf7aa3cbae1a3ff76a73d01ad302448e907be6da92fbe5b0a94569ca91f33e'
    dllSha256='f878c8c3e36264b9f34d0441443a26c50da5ea939490d03e264f30164731afbb'
    dllMvid='88974741-ddbe-4fcb-8f1c-145305534eaa'}
$checks=0
function Check([bool]$value) {if(-not $value){throw 'Brown-Fur alpha predicate failed.'};$script:checks++}
function Reject([scriptblock]$body) {$refused=$false;try{& $body|Out-Null}catch{$refused=$true};Check $refused}
$plan=New-KmgBrownFurAlphaPlan $receipt
Check ($plan.prerelease -and -not $plan.latest -and -not $plan.releaseQualification)
$arguments=@(Get-KmgBrownFurAlphaCreateArguments $plan $PSScriptRoot 'notes.md')
Check ($arguments -contains '--prerelease' -and $arguments -contains '--latest=false' -and $arguments -contains '--draft')
Check ($arguments -contains '--verify-tag' -and $arguments -notcontains '--clobber')
Reject {New-KmgBrownFurAlphaPlan $receipt -Tag v0.0.157}
foreach($name in @('candidateKind','releaseQualification','purpose','version','commit','packageSha256','dllSha256','dllMvid')) {
    $corrupt=$receipt | ConvertTo-Json | ConvertFrom-Json
    $corrupt.$name=if($name -ceq 'releaseQualification'){$true}else{'wrong'}
    Reject {New-KmgBrownFurAlphaPlan $corrupt}
}
foreach($name in @('tag','repository','productCommit','prerelease','latest','releaseQualification','asset','candidateKind','publicationChannel')) {
    $corrupt=$plan | ConvertTo-Json | ConvertFrom-Json
    $corrupt.$name=if($name -ceq 'prerelease'){$false}elseif($name -in @('latest','releaseQualification')){$true}else{'wrong'}
    Reject {Get-KmgBrownFurAlphaCreateArguments $corrupt $PSScriptRoot 'notes.md'}
}
$proofDirectory=Join-Path (Split-Path $PSScriptRoot) 'artifacts/alpha-publication-tests'
[void][IO.Directory]::CreateDirectory($proofDirectory)
$proof=Join-Path $proofDirectory ([Guid]::NewGuid().ToString('N')+'.json')
[IO.File]::WriteAllText($proof,($plan|ConvertTo-Json))
Reject {Assert-KmgNoDevelopmentProof $proof}
$resume=@{RemoteCommit=$receipt.commit;PackageHash=$receipt.packageSha256
    ManifestHash='1f53cec66ce1c15e0ea97a943ad7dbe0e41af30f4c028cc51e9395a76dea58c5'
    ChecksumsHash='c289761347b5d86d772d04f0c8660cb2bf49d5ba10f7a762e0a6da72405c5b98'}
Assert-KmgBrownFurAlphaTagOnlyResume @resume
Check $true
foreach($key in @('RemoteCommit','PackageHash','ManifestHash','ChecksumsHash')) {
    $bad=$resume.Clone();$bad[$key]='wrong'
    Reject {Assert-KmgBrownFurAlphaTagOnlyResume @bad}
}
$nativeFixture=Join-Path $proofDirectory ([Guid]::NewGuid().ToString('N')+'.ps1')
[IO.File]::WriteAllText($nativeFixture,"[Console]::Error.WriteLine('native-progress'); [Console]::Out.WriteLine('native-output'); exit 0")
$nativeOutput=Read-AlphaNative powershell.exe @('-NoProfile','-ExecutionPolicy','Bypass','-File',$nativeFixture)
Check ($nativeOutput.Contains('native-output') -and $nativeOutput.Contains('native-progress'))
Check ($ErrorActionPreference -ceq 'Stop')
[IO.File]::WriteAllText($nativeFixture,'exit 7')
Reject {Read-AlphaNative powershell.exe @('-NoProfile','-ExecutionPolicy','Bypass','-File',$nativeFixture)}
Write-Host "Brown-Fur alpha publication safety PASS: $checks assertions; no remote writes or build."
