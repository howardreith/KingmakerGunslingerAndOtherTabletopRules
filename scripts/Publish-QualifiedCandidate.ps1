[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$PackagePath,
    [Parameter(Mandatory=$true)][string]$CandidateRepositoryRoot,
    [Parameter(Mandatory=$true)][string]$QualificationLedgerPath,
    [Parameter(Mandatory=$true)][string]$MergedReleaseCommit,
    [Parameter(Mandatory=$true)][string]$ReleaseNotesPath,
    [switch]$Publish,
    [switch]$Sprint22,
    [switch]$Sprint23ChargeRounding,
    [switch]$Sprint24Theropods,
    [string]$ProductPrNumber,
    [switch]$ConfirmReleaseReady
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')
function Get-KmgReleaseCommandText([string]$Command, [string[]]$Arguments) {
    $output = @(& $Command @Arguments)
    if ($LASTEXITCODE -ne 0) { throw "$Command failed with exit code $LASTEXITCODE." }
    return ($output -join "`n").Trim()
}
function Resolve-KmgReleaseInput([string]$Path) {
    if (-not [IO.Path]::IsPathRooted($Path)) { $Path = Join-Path $root $Path }
    return (Resolve-Path -LiteralPath $Path).Path
}
Push-Location $root
try {
    $git = Get-KmgGitState -RepositoryRoot $root
    if(@($Sprint22,$Sprint23ChargeRounding,$Sprint24Theropods | Where-Object {$_}).Count -gt 1) { throw 'Select only one qualified-candidate release contract.' }
    if($Sprint23ChargeRounding -and $ProductPrNumber -cnotmatch '^\d+$') {
        throw 'Sprint 23 charge-rounding publication requires its numeric product PR.'
    }
    if($Sprint24Theropods -and $ProductPrNumber -cnotmatch '^\d+$') {
        throw 'Sprint 24 theropod publication requires its numeric product PR.'
    }
    $selectedBranch=if($Sprint24Theropods){'codex/expanded-summoning-sprint24-deinonychus-tyrannosaurus'}elseif($Sprint23ChargeRounding){'codex/expanded-summoning-sprint23-charge-rounding'}elseif($Sprint22){'codex/expanded-summoning-sprint22-ankylosaurus'}else{'codex/expanded-summoning-sprint21-bebelith-giant-crab'}
    $selectedPr=if($Sprint24Theropods -or $Sprint23ChargeRounding){$ProductPrNumber}elseif($Sprint22){'39'}else{'36'}
    $selectedSuffix=if($Sprint24Theropods){'expanded-summoning-sprint24-theropods'}elseif($Sprint23ChargeRounding){'expanded-summoning-sprint23-charge-rounding'}elseif($Sprint22){'expanded-summoning-sprint22'}else{'expanded-summoning-sprint21'}
    if ($git.Status.Count -ne 0 -or $git.Branch -cne $selectedBranch) {
        throw 'Promotion requires the selected clean reporting branch.'
    }
    $repository = 'howardreith/KingmakerGunslingerAndOtherTabletopRules'
    $origin = Get-KmgReleaseCommandText git @('remote','get-url','origin')
    Assert-KmgReleaseOrigin -Origin $origin
    $repositoryInfo = Get-KmgReleaseCommandText gh @('repo','view','--json','nameWithOwner,defaultBranchRef,isPrivate') | ConvertFrom-Json
    if ($repositoryInfo.nameWithOwner -cne $repository -or $repositoryInfo.isPrivate -or
        $repositoryInfo.defaultBranchRef.name -cne 'master') { throw 'Unexpected release repository metadata.' }
    $null = Get-KmgReleaseCommandText git @('fetch','--tags','origin','master',$git.Branch)
    if ((Get-KmgReleaseCommandText git @('rev-parse',('origin/'+$git.Branch))) -cne $git.Commit -or
        (Get-KmgReleaseCommandText git @('rev-parse','origin/master')) -cne $MergedReleaseCommit) {
        throw 'Reporting or merged release head has moved; review the current lineage.'
    }
    $pr = Get-KmgReleaseCommandText gh @('pr','view',$selectedPr,'--json','state,mergeCommit') | ConvertFrom-Json
    if ($pr.state -cne 'MERGED' -or $pr.mergeCommit.oid -cne $MergedReleaseCommit) {
        throw 'The selected sprint PR has not merged at the selected release commit.'
    }
    $candidateRoot = (Resolve-Path -LiteralPath $CandidateRepositoryRoot).Path
    $candidate = Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $candidateRoot
    $candidateGit = Get-KmgGitState -RepositoryRoot $candidateRoot
    if ($candidateGit.Status.Count -ne 0 -or $candidateGit.Commit -cne $candidate.commit -or
        (Get-KmgSourceStateFingerprint -RepositoryRoot $candidateRoot) -cne $candidate.sourceStateSha256) {
        throw 'Qualified artifact source is no longer frozen.'
    }
    foreach ($ancestor in @($candidate.commit,$git.Commit)) {
        $null = Get-KmgReleaseCommandText git @('merge-base','--is-ancestor',$ancestor,$MergedReleaseCommit)
    }
    # Harness/reporting changes may follow qualification. Product/package inputs
    # must remain byte-equivalent; no reporting-only rebuild is performed.
    $null = Get-KmgReleaseCommandText git @('diff','--exit-code',$candidate.commit,$MergedReleaseCommit,'--',
        'src/KingmakerGunslinger','assets','blueprints','Info.json','Directory.Build.props')
    $info = Get-Content (Join-Path $root 'Info.json') -Raw | ConvertFrom-Json
    if ($info.Id -cne 'KingmakerGunslinger' -or $info.Version -cne $candidate.version -or
        $candidate.version -cnotmatch '^\d+\.\d+\.\d+$') { throw 'Stable version identity differs from the tested artifact.' }
    $ledgerPath = Resolve-KmgReleaseInput $QualificationLedgerPath
    $ledger = Get-Content -LiteralPath $ledgerPath -Raw | ConvertFrom-Json
    if($Sprint24Theropods) { Assert-KmgSprint24TheropodReleaseDecision -Ledger $ledger -Candidate $candidate }
    elseif($Sprint23ChargeRounding) { Assert-KmgSprint23ChargeRoundingReleaseDecision -Ledger $ledger -Candidate $candidate }
    elseif($Sprint22) { Assert-KmgSprint22ReleaseDecision -Ledger $ledger -Candidate $candidate }
    else { Assert-KmgClosureReleaseDecision -Ledger $ledger -Candidate $candidate }
    Assert-KmgClosureProofFiles -Ledger $ledger
    Assert-KmgNotRunning
    if (Test-Path -LiteralPath 'C:/Dev/KingmakerGunslingerLab/compatibility-state/compatibility.lock') {
        throw 'A runtime transaction is still active; publication is refused.'
    }
    $intake = Get-Content -LiteralPath (Join-Path $ledger.evidenceRoot $ledger.restoration.intakeManifest) -Raw | ConvertFrom-Json
    if($Sprint22 -or $Sprint23ChargeRounding -or $Sprint24Theropods) { $intake=@($intake.modsManifest) }
    $currentMods = Get-KmgCompatibilityDirectoryManifest 'C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker/Mods'
    if (-not (Test-KmgCompatibilityManifestEqual $intake $currentMods)) { throw 'Actual human installation has not been restored.' }
    $notesPath = Resolve-KmgReleaseInput $ReleaseNotesPath
    $notes = [IO.File]::ReadAllText($notesPath)
    if (-not $notes.Contains($candidate.version) -or
        $notes -match '(?i)not a public release|publication status:.*(?:local|draft)|release pending|not release-ready') {
        throw 'Stable release notes have not been qualified.'
    }
    foreach ($exception in $ledger.baselineExceptions) {
        if (-not $notes.Contains($exception.id)) { throw 'Release notes omit an inherited failed assertion.' }
    }
    $tag = 'v'+$candidate.version
    if (-not [string]::IsNullOrWhiteSpace((Get-KmgReleaseCommandText git @('ls-remote','--tags','origin',('refs/tags/'+$tag))))) {
        throw 'Stable tag already exists; no tag or asset may be replaced.'
    }
    $releases = Get-KmgReleaseCommandText gh @('api',('repos/'+$repository+'/releases?per_page=100')) | ConvertFrom-Json
    if (@($releases | Where-Object {$_.tag_name -ceq $tag}).Count -gt 0) { throw 'Stable release already exists.' }
    if ($Publish -and -not $ConfirmReleaseReady) { throw 'Publication requires -ConfirmReleaseReady.' }
    $releaseRoot = Join-Path $root ('artifacts/release-promotion/'+$candidate.version+'/'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ'))
    if (Test-Path -LiteralPath $releaseRoot) { throw 'Promotion output already exists.' }
    New-Item -ItemType Directory -Path $releaseRoot | Out-Null
    $assetName = 'KingmakerGunslinger-'+$candidate.version+'-'+$selectedSuffix+'.zip'
    $asset = Join-Path $releaseRoot $assetName
    Copy-Item -LiteralPath $candidate.packagePath -Destination $asset
    if ((Get-KmgSha256 -Path $asset) -cne $candidate.packageSha256) { throw 'Promotion copy differs from qualified bytes.' }
    $checksums = Join-Path $releaseRoot 'SHA256SUMS.txt'
    ($candidate.packageSha256+'  '+$assetName) | Set-Content -LiteralPath $checksums -Encoding ASCII
    $manifest = [ordered]@{
        schemaVersion=2; generator='scripts/Publish-Release.ps1 -ReuseQualifiedCandidate'; version=$candidate.version;
        tag=$tag; commit=$MergedReleaseCommit; mergedReleaseCommit=$MergedReleaseCommit; reportingCommit=$git.Commit;
        testedSource=$candidate.commit; sourceFingerprint=$candidate.sourceStateSha256;
        package=$assetName; packageSha256=$candidate.packageSha256; dllSha256=$candidate.dllSha256; dllMvid=$candidate.dllMvid;
        buildLocalManifestSha256=Get-KmgSha256 -Path ($candidate.packagePath+'.build-local.json');
        fullSuite=$ledger.candidate.fullSuite; harness=$ledger.harness; qualificationStatus=$ledger.status;
        closureLedgerSha256=Get-KmgSha256 -Path $ledgerPath;
        coverage=@(Get-KmgClosurePublicCoverage -Ledger $ledger); baselineExceptions=@($ledger.baselineExceptions);
        roster=$ledger.roster; restoration=$ledger.restoration; ownerVisualReview=$ledger.ownerVisualReview;
        packageValidated=$true; exactArtifactPromotion=$true; rebuiltForPublication=$false; publicationRequested=[bool]$Publish
    }
    $manifestPath = Join-Path $releaseRoot 'release-manifest.json'
    $manifest | ConvertTo-Json -Depth 18 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
    $generatedNotes = Join-Path $releaseRoot 'release-notes.md'
    ($notes+"`n`nValidated ZIP SHA-256: "+$candidate.packageSha256+"`nTested source: "+$candidate.commit+
        "`nMerged release lineage: "+$MergedReleaseCommit+"`nQualification: "+$ledger.status) |
        Set-Content -LiteralPath $generatedNotes -Encoding UTF8
    if ($Publish) {
        $null = Get-KmgReleaseCommandText git @('tag','-a',$tag,'-m',('Tabletop Added Rules '+$tag),$MergedReleaseCommit)
        $null = Get-KmgReleaseCommandText git @('push','origin',('refs/tags/'+$tag))
        $null = Get-KmgReleaseCommandText gh @('release','create',$tag,$asset,$checksums,$manifestPath,'--repo',$repository,
            '--title',('Tabletop Added Rules '+$tag),'--notes-file',$generatedNotes,'--verify-tag','--target',$MergedReleaseCommit,'--latest')
    }
    [pscustomobject]@{tag=$tag;published=[bool]$Publish;mergedReleaseCommit=$MergedReleaseCommit;testedSource=$candidate.commit;
        package=$asset;packageSha256=$candidate.packageSha256;manifest=$manifestPath;checksums=$checksums}
} finally { Pop-Location }
