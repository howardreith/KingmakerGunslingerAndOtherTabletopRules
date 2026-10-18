[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$PackagePath,
    [Parameter(Mandatory=$true)][string]$CandidateRepositoryRoot,
    [Parameter(Mandatory=$true)][string]$QualificationLedgerPath,
    [string]$MergedReleaseCommit,
    [Parameter(Mandatory=$true)][string]$ReleaseNotesPath,
    [switch]$Publish,
    [switch]$Sprint22,
    [switch]$Sprint23ChargeRounding,
    [switch]$Sprint24Theropods,
    [switch]$Sprint25GiantBodies,
    [switch]$PreflightOnly,
    [string]$ExpectedBaseCommit,
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
    Assert-KmgQualifiedCandidateInvocationMode -PreflightOnly:$PreflightOnly -Sprint25GiantBodies:$Sprint25GiantBodies `
        -Publish:$Publish -ConfirmReleaseReady:$ConfirmReleaseReady -MergedReleaseCommit $MergedReleaseCommit `
        -ExpectedBaseCommit $ExpectedBaseCommit
    $git = Get-KmgGitState -RepositoryRoot $root
    if(@($Sprint22,$Sprint23ChargeRounding,$Sprint24Theropods,$Sprint25GiantBodies | Where-Object {$_}).Count -gt 1) { throw 'Select only one qualified-candidate release contract.' }
    if($Sprint25GiantBodies -and $ProductPrNumber -cne '49') {
        throw 'Sprint 25 publication is authorized only for product PR 49.'
    }
    if($Sprint25GiantBodies -and [IO.Path]::GetFullPath($root).TrimEnd('\') -ine
        'C:\Dev\KingmakerGunslingerLab\wt\s25') { throw 'Sprint 25 publisher must run from the assigned wt/s25 root.' }
    if($Sprint23ChargeRounding -and $ProductPrNumber -cnotmatch '^\d+$') {
        throw 'Sprint 23 charge-rounding publication requires its numeric product PR.'
    }
    if($Sprint24Theropods -and $ProductPrNumber -cnotmatch '^\d+$') {
        throw 'Sprint 24 theropod publication requires its numeric product PR.'
    }
    $selectedBranch=if($Sprint25GiantBodies){'codex/expanded-summoning-sprint25-brachiosaurus-elephant'}elseif($Sprint24Theropods){'codex/expanded-summoning-sprint24-deinonychus-tyrannosaurus'}elseif($Sprint23ChargeRounding){'codex/expanded-summoning-sprint23-charge-rounding'}elseif($Sprint22){'codex/expanded-summoning-sprint22-ankylosaurus'}else{'codex/expanded-summoning-sprint21-bebelith-giant-crab'}
    $selectedPr=if($Sprint25GiantBodies -or $Sprint24Theropods -or $Sprint23ChargeRounding){$ProductPrNumber}elseif($Sprint22){'39'}else{'36'}
    $selectedSuffix=if($Sprint25GiantBodies){'expanded-summoning-sprint25-giant-bodies'}elseif($Sprint24Theropods){'expanded-summoning-sprint24-theropods'}elseif($Sprint23ChargeRounding){'expanded-summoning-sprint23-charge-rounding'}elseif($Sprint22){'expanded-summoning-sprint22-ankylosaurus'}else{'expanded-summoning-sprint21'}
    if ($git.Status.Count -ne 0 -or $git.Branch -cne $selectedBranch) {
        throw 'Promotion requires the selected clean reporting branch.'
    }
    $repository = 'howardreith/KingmakerGunslingerAndOtherTabletopRules'
    $origin = Get-KmgReleaseCommandText git @('remote','get-url','origin')
    Assert-KmgReleaseOrigin -Origin $origin
    $repositoryInfo = Get-KmgReleaseCommandText gh @('repo','view','--json','nameWithOwner,defaultBranchRef,isPrivate') | ConvertFrom-Json
    if ($repositoryInfo.nameWithOwner -cne $repository -or $repositoryInfo.isPrivate -or
        $repositoryInfo.defaultBranchRef.name -cne 'master') { throw 'Unexpected release repository metadata.' }
    # Fetch only the two refs this promotion compares. A --tags fetch fails
    # outright on this repository because upstream rewrote its lineage, so
    # local v0.0.136-v0.0.153 tags point at pre-rewrite commits and git
    # refuses to clobber them. Those historical tags and their published
    # assets are deliberately left untouched. Nothing is weakened: stable
    # tag existence is already proven against the remote with ls-remote and
    # the GitHub releases API below, both stronger than a local tag read.
    $null = Get-KmgReleaseCommandText git @('fetch','--no-tags','origin','master',$git.Branch)
    $remoteMaster=Get-KmgReleaseCommandText git @('rev-parse','origin/master')
    $remoteProduct=Get-KmgReleaseCommandText git @('rev-parse',('origin/'+$git.Branch))
    if ($remoteProduct -cne $git.Commit -or
        (-not $PreflightOnly -and $remoteMaster -cne $MergedReleaseCommit)) {
        throw 'Reporting or merged release head has moved; review the current lineage.'
    }
    $pr = Get-KmgReleaseCommandText gh @('pr','view',$selectedPr,'--json','state,mergeCommit,headRefOid,baseRefOid,baseRefName,isDraft,mergeable') | ConvertFrom-Json
    if (-not $PreflightOnly -and ($pr.state -cne 'MERGED' -or $pr.mergeCommit.oid -cne $MergedReleaseCommit)) {
        throw 'The selected sprint PR has not merged at the selected release commit.'
    }
    # The head that was actually merged, taken from verified metadata and then
    # cross-checked against the product merge's own parents. Two independent
    # sources, so no ledger claim can nominate a different head.
    $mergedPrHead = $null
    if(-not $PreflightOnly) {
    $mergedPrHead = [string]$pr.headRefOid
    $mergeParents = @((Get-KmgReleaseCommandText git @('log','--format=%P','-1',$MergedReleaseCommit)) -split '\s+' |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($mergedPrHead -cnotmatch '^[0-9a-f]{40}$' -or $mergeParents -cnotcontains $mergedPrHead) {
        throw 'The merged pull-request head is not one of the product merge parents.'
    }
    }
    $candidateRoot = (Resolve-Path -LiteralPath $CandidateRepositoryRoot).Path
    if($Sprint25GiantBodies -and [IO.Path]::GetFullPath($candidateRoot).TrimEnd('\') -ine
        'C:\Dev\KingmakerGunslingerLab\wt\s25') {
        throw 'Sprint 25 promotion must use the product root selected before building: wt/s25.'
    }
    $candidate = Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $candidateRoot
    $candidateGit = Get-KmgGitState -RepositoryRoot $candidateRoot
    if ($candidateGit.Status.Count -ne 0 -or $candidateGit.Commit -cne $candidate.commit -or
        (Get-KmgSourceStateFingerprint -RepositoryRoot $candidateRoot) -cne $candidate.sourceStateSha256) {
        throw 'Qualified artifact source is no longer frozen.'
    }
    # Publication unconditionally requires tested source inside the actual
    # merge. Preflight proves only the pinned base ancestry, never a future merge.
    if($PreflightOnly) {
        if($candidate.commit -cne $git.Commit) { throw 'Premerge source and artifact head differ.' }
        $null = Get-KmgReleaseCommandText git @('merge-base','--is-ancestor',$ExpectedBaseCommit,$candidate.commit)
    } else {
        $null = Get-KmgReleaseCommandText git @('merge-base','--is-ancestor',$candidate.commit,$MergedReleaseCommit)
    }
    # Reporting ancestry is checked separately. A reporting branch legitimately
    # carries the records describing its own merge, so it can be a reviewed
    # descendant of the merged head rather than an ancestor of the merge.
    $allowedReportingPaths = @(
        'AUTONOMOUS-RESUME.md',
        'docs/SPRINT24-COVERAGE.md',
        'docs/RELEASE-NOTES-0.0.155.md',
        'planning/SPRINT24-THEROPOD-DINOSAURS-LEDGER.json',
        'scripts/Publish-QualifiedCandidate.ps1',
        'scripts/QualifiedRelease.Common.ps1',
        'scripts/Test-Sprint24Release.ps1',
        # The focused publisher refusal suite added for this repair, and the
        # validation wrapper that runs it. Neither is compiled into or included
        # in the candidate package: the archive carries no scripts/ entry.
        'scripts/Test-Sprint24ReportingLineage.ps1',
        'scripts/validate-repository.ps1')
    if ($Sprint25GiantBodies) {
        $allowedReportingPaths=@('AUTONOMOUS-RESUME.md','docs/SPRINT25-COVERAGE.md',
            'docs/RELEASE-NOTES-0.0.159.md','planning/SPRINT25-GIANT-BODIES-LEDGER.json',
            'planning/SPRINT25-QUALIFICATION-LEDGER.json','scripts/Publish-QualifiedCandidate.ps1',
            'scripts/Publish-Release.ps1','scripts/QualifiedRelease.Common.ps1',
            'scripts/Test-Sprint25Release.ps1','scripts/validate-repository.ps1')
    }
    elseif (-not $Sprint24Theropods) {
        $allowedReportingPaths = @()
        $null = Get-KmgReleaseCommandText git @('merge-base','--is-ancestor',$git.Commit,$MergedReleaseCommit)
    }
    $reportingLineage=$null
    if(-not $PreflightOnly) {
    $reportingLineage = Assert-KmgReportingLineage -RepositoryRoot $root `
        -ReportingCommit $git.Commit -MergedReleaseCommit $MergedReleaseCommit `
        -MergedPrHead $mergedPrHead -AllowedReportingPaths $allowedReportingPaths
    # Harness/reporting changes may follow qualification. Product/package inputs
    # must remain byte-equivalent; no reporting-only rebuild is performed.
    $null = Get-KmgReleaseCommandText git @('diff','--exit-code',$candidate.commit,$MergedReleaseCommit,'--',
        'src/KingmakerGunslinger','assets','blueprints','Info.json','Directory.Build.props')
    }
    $info = Get-Content (Join-Path $root 'Info.json') -Raw | ConvertFrom-Json
    if ($info.Id -cne 'KingmakerGunslinger' -or $info.Version -cne $candidate.version -or
        $candidate.version -cnotmatch '^\d+\.\d+\.\d+$') { throw 'Stable version identity differs from the tested artifact.' }
    $ledgerPath = Resolve-KmgReleaseInput $QualificationLedgerPath
    $ledger = Get-Content -LiteralPath $ledgerPath -Raw | ConvertFrom-Json
    if($PreflightOnly) {
        Assert-KmgSprint25PremergeAuthority -Ledger $ledger -Candidate $candidate -PullRequest $pr `
            -RemoteProductHead $remoteProduct -RemoteMaster $remoteMaster -ExpectedBaseCommit $ExpectedBaseCommit
    }
    elseif($Sprint25GiantBodies) {
        Assert-KmgSprint25GiantBodyReleaseDecision -Ledger $ledger -Candidate $candidate
        if($ledger.publication.productMergeCommit -cne $MergedReleaseCommit) { throw 'Ledger product merge differs from verified GitHub/Git lineage.' }
    }
    elseif($Sprint24Theropods) { Assert-KmgSprint24TheropodReleaseDecision -Ledger $ledger -Candidate $candidate }
    elseif($Sprint23ChargeRounding) { Assert-KmgSprint23ChargeRoundingReleaseDecision -Ledger $ledger -Candidate $candidate }
    elseif($Sprint22) { Assert-KmgSprint22ReleaseDecision -Ledger $ledger -Candidate $candidate }
    else { Assert-KmgClosureReleaseDecision -Ledger $ledger -Candidate $candidate }
    Assert-KmgClosureProofFiles -Ledger $ledger
    Assert-KmgNotRunning
    if (Test-Path -LiteralPath 'C:/Dev/KingmakerGunslingerLab/compatibility-state/compatibility.lock') {
        throw 'A runtime transaction is still active; publication is refused.'
    }
    $intake = Get-Content -LiteralPath (Join-Path $ledger.evidenceRoot $ledger.restoration.intakeManifest) -Raw | ConvertFrom-Json
    if($Sprint22 -or $Sprint23ChargeRounding -or $Sprint24Theropods -or $Sprint25GiantBodies) { $intake=@($intake.modsManifest) }
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
    if($PreflightOnly) {
        # No output directory, tag, push or release is created by this path.
        # A real product merge must still pass the unchanged publication path.
        [pscustomobject]@{kind='KmgSprint25PremergePreflight';status='PASS';published=$false;
            checkedUtc=[DateTime]::UtcNow.ToString('o');requiresProductMerge=$true;productPr=49;
            expectedBaseCommit=$ExpectedBaseCommit;testedSource=$candidate.commit;tag=$tag;
            candidateRoot=$candidateRoot;packageSha256=$candidate.packageSha256;
            dllSha256=$candidate.dllSha256;dllMvid=$candidate.dllMvid;
            sourceFingerprint=$candidate.sourceStateSha256;qualificationLedger=$ledgerPath;
            qualificationLedgerSha256=Get-KmgSha256 $ledgerPath;promotionDirectoryCreated=$false}
        return
    }
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
        mergedPrHead=$mergedPrHead; reportingLineageMode=$reportingLineage.mode;
        reportingDeltaPaths=@($reportingLineage.reportingDeltaPaths);
        publisherScriptSha256=Get-KmgSha256 -Path (Join-Path $PSScriptRoot 'Publish-QualifiedCandidate.ps1');
        publisherCommonSha256=Get-KmgSha256 -Path (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1');
        publisherTestSha256=Get-KmgSha256 -Path (Join-Path $PSScriptRoot $(if($Sprint25GiantBodies){'Test-Sprint25Release.ps1'}else{'Test-Sprint24Release.ps1'}));
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
