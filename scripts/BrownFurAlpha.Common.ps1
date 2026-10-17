Set-StrictMode -Version Latest

function Read-AlphaNative([string]$Command,[string[]]$Arguments) {
    # Windows PowerShell can promote normal native stderr (Git progress) to a
    # terminating NativeCommandError. Preserve the real process exit decision.
    $previous=$ErrorActionPreference
    try {
        $ErrorActionPreference='Continue'
        $output=@(& $Command @Arguments 2>&1)
        $nativeExit=$LASTEXITCODE
    } finally { $ErrorActionPreference=$previous }
    if($nativeExit -ne 0){throw "Alpha command failed: $Command $($Arguments[0]); exit=$nativeExit"}
    return (($output | ForEach-Object {[string]$_}) -join "`n").Trim()
}

function Assert-KmgBrownFurAlphaTagOnlyResume {
    param([string]$RemoteCommit,[string]$PackageHash,[string]$ManifestHash,[string]$ChecksumsHash)
    # Exact immutable files from the interrupted alpha attempt. No tag write,
    # asset replacement, arbitrary existing draft, or changed manifest allowed.
    if($RemoteCommit -cne '913dfd6e89386ff233849f06f902ca8ea2de239e' -or
        $PackageHash -cne 'd7cf7aa3cbae1a3ff76a73d01ad302448e907be6da92fbe5b0a94569ca91f33e' -or
        $ManifestHash -cne '1f53cec66ce1c15e0ea97a943ad7dbe0e41af30f4c028cc51e9395a76dea58c5' -or
        $ChecksumsHash -cne 'c289761347b5d86d772d04f0c8660cb2bf49d5ba10f7a762e0a6da72405c5b98') {
        throw 'Interrupted alpha does not match its exact owned tag-only checkpoint.'
    }
}

function New-KmgBrownFurAlphaPlan {
    param([Parameter(Mandatory=$true)]$Receipt,
        [string]$Tag='v0.0.157-brown-fur-alpha.1')
    # One owner-authorized public preview of the exact tested bytes. This is
    # not a generic development-to-release promotion or a qualification waiver.
    if ($Tag -cne 'v0.0.157-brown-fur-alpha.1' -or
        $Receipt.candidateKind -cne 'development' -or $Receipt.releaseQualification -ne $false -or
        $Receipt.purpose -cne 'brown-fur-startup-order' -or $Receipt.version -cne '0.0.157' -or
        $Receipt.commit -cne '913dfd6e89386ff233849f06f902ca8ea2de239e' -or
        $Receipt.packageSha256 -cne 'd7cf7aa3cbae1a3ff76a73d01ad302448e907be6da92fbe5b0a94569ca91f33e' -or
        $Receipt.dllSha256 -cne 'f878c8c3e36264b9f34d0441443a26c50da5ea939490d03e264f30164731afbb' -or
        $Receipt.dllMvid -cne '88974741-ddbe-4fcb-8f1c-145305534eaa') {
        throw 'Only the explicitly authorized, unchanged Brown-Fur alpha candidate may be published.'
    }
    return [pscustomobject]@{tag=$Tag;repository='howardreith/KingmakerGunslingerAndOtherTabletopRules'
        title='Tabletop Added Rules - Brown-Fur startup repair (alpha 1)'
        asset='KingmakerGunslinger-0.0.157-brown-fur-alpha.1.zip'
        productCommit=$Receipt.commit;prerelease=$true;latest=$false;releaseQualification=$false
        candidateKind='development';publicationChannel='alpha-test'}
}

function Get-KmgBrownFurAlphaCreateArguments {
    param([Parameter(Mandatory=$true)]$Plan,[string]$Directory,[string]$NotesPath)
    if ($Plan.tag -cne 'v0.0.157-brown-fur-alpha.1' -or
        $Plan.repository -cne 'howardreith/KingmakerGunslingerAndOtherTabletopRules' -or
        $Plan.productCommit -cne '913dfd6e89386ff233849f06f902ca8ea2de239e' -or
        $Plan.prerelease -ne $true -or $Plan.latest -ne $false -or $Plan.releaseQualification -ne $false -or
        $Plan.candidateKind -cne 'development' -or $Plan.publicationChannel -cne 'alpha-test' -or
        $Plan.asset -cne 'KingmakerGunslinger-0.0.157-brown-fur-alpha.1.zip') {
        throw 'Alpha publication cannot select stable, Latest, another product or repository.'
    }
    return @('release','create',$Plan.tag,(Join-Path $Directory $Plan.asset),
        (Join-Path $Directory 'SHA256SUMS.txt'),(Join-Path $Directory 'alpha-manifest.json'),
        '--repo',$Plan.repository,'--title',$Plan.title,'--notes-file',$NotesPath,
        '--verify-tag','--target',$Plan.productCommit,'--prerelease','--latest=false','--draft')
}
