Set-StrictMode -Version Latest

function Get-KmgSprint29SourceSurface {
    $repositoryRoot=Split-Path -Parent $PSScriptRoot
    $python=Get-Command python -ErrorAction Stop
    $program=@'
import json, sys
from pathlib import Path
sys.path.insert(0, str(Path(sys.argv[1]) / 'tools'))
import expanded_summoning_manifest as m
c=m.parsed_creatures()
s=m.suppressed_keys()
monster=sum(10-x['monster'] for x in c if x['monster'] is not None)
ally=sum(10-x['ally'] for x in c if x['ally'] is not None)
hidden=sum((10-x['monster'] if x['monster'] is not None else 0)+(10-x['ally'] if x['ally'] is not None else 0) for x in c if x['key'] in s)
scope={'ettin':5,'cloud-giant':8,'storm-giant':9}
giants=[x for x in c if x['key'] in scope]
exact=len(giants)==3 and {x['key'] for x in giants}==set(scope) and all(x['monster'] is None and not x['templated'] and x['ally']==scope[x['key']] for x in giants)
roots=sum(10-x['ally'] for x in giants)
wrappers=len(m.NATIVE_EXPANDED_OPTIONS)
print(json.dumps(dict(sprint29RosterExact=exact,sprint29Placements=roots,sprint29SuppressedPlacements=sum(10-x['ally'] for x in giants if x['key'] in s),projectCreatures=len(c),monsterCreatures=sum(x['monster'] is not None for x in c),monsterPlacements=monster,natureAllyCreatures=sum(x['ally'] is not None for x in c),natureAllyPlacements=ally,registeredRoots=monster+ally,generatedRoots=monster+ally-hidden,nativeWrappers=wrappers,choices=monster+ally-hidden+wrappers,suppressedLogicalPlacements=hidden)))
'@
    $output=@(& $python.Source -B -c $program $repositoryRoot)
    if($LASTEXITCODE -ne 0) { throw 'Cannot derive the current Sprint 29 source surface.' }
    return (($output -join '') | ConvertFrom-Json)
}

function Assert-KmgSprint29ScopeDecisions {
    param([Parameter(Mandatory=$true)]$Ledger,
        [Parameter(Mandatory=$true)][string]$Notes)
    $scope=$Ledger.scopeDecisions
    if($null -eq $scope -or $scope.ownerAuthority -cne
        'Sprint 29 mission and Awesome Blow owner decision 2026-10-21' -or
        $scope.skills -cne 'DEFER_NAMED_UNSUPPORTED_CONSUMERS' -or
        $scope.movement -cne 'DEFER_UNSUPPORTED_VERTICAL_AQUATIC' -or
        $scope.weather -cne 'FILTER_CONTROL_WEATHER' -or
        $scope.awesomeBlowWall -cne 'TARGET_COLLISION_1D6_ONLY_STATIC_WALL_NO_HP' -or
        $scope.nativePitUnchanged -ne $true -or
        $scope.rankReallocation -ne $false -or
        $scope.substituteBonuses -ne $false -or
        $scope.signatureCombatMagicRequired -ne $true -or
        @($scope.otherGameplayOmissions).Count -ne 0 -or
        $scope.disclosurePath -cne 'docs/RELEASE-NOTES-0.0.163.md') {
        throw 'Sprint 29 named scope decision is missing, broadened or undisclosed.'
    }
    foreach($phrase in @('Climb','Craft','Perform','Swim','Handle Animal',
        'levitation','water breathing','Control Weather','wall object',
        'victim','1d6')) {
        if(-not $Notes.Contains($phrase)) {
            throw "Sprint 29 release notes omit named scope: $phrase"
        }
    }
}

function Assert-KmgSprint29GiantsQualification {
    param([Parameter(Mandatory=$true)]$Ledger,
        [Parameter(Mandatory=$true)]$Candidate,
        [Parameter(Mandatory=$true)][string]$Notes)
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint29Giants' -or
        $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or
        $Ledger.sprint30Started -or @($Ledger.baselineExceptions).Count -ne 0 -or
        @($Ledger.unresolvedFindings).Count -ne 0 -or $Ledger.expandedScope -or
        $Ledger.gameplayWaiver) {
        throw 'Sprint 29 has unresolved gameplay/scope or incomplete release qualification.'
    }
    Assert-KmgSprint29ScopeDecisions -Ledger $Ledger -Notes $Notes
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),
        @('dllSha256','dllSha256'),@('dllMvid','dllMvid'),
        @('sourceFingerprint','sourceStateSha256'))) {
        if([string]$Ledger.candidate.($pair[0]) -cne
            [string]$Candidate.($pair[1])) {
            throw 'Sprint 29 tested source or artifact identity differs.'
        }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or
        $suite.failures -ne 0 -or $suite.executed -le 0 -or
        $suite.executed -ne $suite.registered -or -not $suite.unfiltered) {
        throw 'Sprint 29 candidate did not pass one fresh complete suite.'
    }
    $surface=Get-KmgSprint29SourceSurface
    foreach($key in @('projectCreatures','monsterCreatures','monsterPlacements',
        'natureAllyCreatures','natureAllyPlacements','registeredRoots',
        'generatedRoots','nativeWrappers','choices','suppressedLogicalPlacements',
        'sprint29Placements','sprint29SuppressedPlacements')) {
        if($Ledger.roster.$key -ne $surface.$key) {
            throw "Sprint 29 source-derived publication surface differs: $key"
        }
    }
    if(-not $surface.sprint29RosterExact -or
        -not $Ledger.roster.sprint29RosterExact -or
        $surface.sprint29Placements -ne 8 -or
        $surface.sprint29SuppressedPlacements -ne 0 -or
        $surface.suppressedLogicalPlacements -ne 0 -or
        $Ledger.roster.identityChanges -ne 0 -or
        $Ledger.roster.routeOrderingChanges -ne 0 -or
        $Ledger.roster.protectedIdentitySetsEqual -ne $true) {
        throw 'Sprint 29 public roots or protected identities/routes are not qualified.'
    }
    if($Ledger.ownerVisualReview -cnotin @('NOT_PERFORMED_NONBLOCKING','APPROVED') -or
        ($Ledger.ownerVisualReview -ceq 'APPROVED' -and
            [string]::IsNullOrWhiteSpace([string]$Ledger.ownerVisualReviewEvidence))) {
        throw 'Sprint 29 owner visual-review status lacks provenance.'
    }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or
        -not $Ledger.restoration.workingFixtureClean) {
        throw 'Sprint 29 requires exact protected intake and Working restoration.'
    }
    if($Ledger.publication.tag -cne ('v'+$Candidate.version) -or
        $Ledger.publication.prerelease -or $Ledger.publication.draft -or
        -not $Ledger.publication.latest) {
        throw 'Sprint 29 stable publication identity/state is invalid.'
    }
    $fresh=@('public-eight-native-spellbook-routes',
        'public-inventory-icons-and-body-visibility',
        'affected-giant-melee-ranged-catch-cleave-vital-controls',
        'cloud-fog-storm-lightning-and-ai-lifetimes',
        'ettin-two-weapon-ammunition-owned-equipment',
        'targeted-on-off-cold-load-cleanup-and-absence',
        'standalone-and-high-risk-combined',
        'complete-public-source-build-package-gate',
        'exact-package-release-qualification')
    $mandatory=@('inherited-unaffected-qualification')+$fresh
    $ids=@($Ledger.coverage | ForEach-Object {$_.id})
    if(@($ids | Sort-Object -Unique).Count -ne $ids.Count) {
        throw 'Sprint 29 duplicate coverage identity.'
    }
    foreach($id in $mandatory) {
        $row=@($Ledger.coverage | Where-Object {$_.id -ceq $id})
        if($row.Count -ne 1 -or $row[0].result -cnotin @('PASS','REUSED_PASS') -or
            ($id -cin $fresh -and $row[0].result -cne 'PASS')) {
            throw "Sprint 29 mandatory fresh/reused coverage is incomplete: $id"
        }
    }
    foreach($row in $Ledger.coverage) {
        if($row.result -cnotin @('PASS','REUSED_PASS') -or
            @($row.proofs).Count -eq 0 -or
            @($row.evidenceSources).Count -eq 0) {
            throw "Sprint 29 coverage lacks proof: $($row.id)"
        }
        foreach($source in $row.evidenceSources) {
            if($source.source -cnotmatch '^[0-9a-f]{40}$' -or
                $source.packageSha256 -cnotmatch '^[0-9a-f]{64}$' -or
                $source.dllSha256 -cnotmatch '^[0-9a-f]{64}$' -or
                $source.dllMvid -cnotmatch
                    '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$') {
                throw 'Sprint 29 execution provenance is malformed.'
            }
            if($row.result -ceq 'PASS' -and
                ($source.source -cne $Candidate.commit -or
                 $source.packageSha256 -cne $Candidate.packageSha256 -or
                 $source.dllSha256 -cne $Candidate.dllSha256 -or
                 $source.dllMvid -cne $Candidate.dllMvid)) {
                throw 'Sprint 29 fresh proof is bound to other bytes.'
            }
        }
        if($row.result -ceq 'REUSED_PASS' -and
            ([string]::IsNullOrWhiteSpace([string]$row.reuseJustification) -or
            [string]::IsNullOrWhiteSpace([string]$row.priorSource))) {
            throw 'Sprint 29 reused proof lacks dependency/provenance evidence.'
        }
    }
}

function Assert-KmgSprint29GiantsReleaseDecision {
    param([Parameter(Mandatory=$true)]$Ledger,
        [Parameter(Mandatory=$true)]$Candidate,
        [Parameter(Mandatory=$true)][string]$Notes)
    Assert-KmgSprint29GiantsQualification -Ledger $Ledger -Candidate $Candidate -Notes $Notes
    if($Ledger.publication.productMergeCommit -cnotmatch '^[0-9a-f]{40}$' -or
        $Ledger.publication.mergeContainsTestedSource -ne $true) {
        throw 'Sprint 29 publication requires the actual verified product merge.'
    }
}

function Assert-KmgSprint29PremergeAuthority {
    param([Parameter(Mandatory=$true)]$Ledger,
        [Parameter(Mandatory=$true)]$Candidate,
        [Parameter(Mandatory=$true)][string]$Notes,
        [Parameter(Mandatory=$true)]$PullRequest,
        [Parameter(Mandatory=$true)][string]$RemoteProductHead,
        [Parameter(Mandatory=$true)][string]$RemoteMaster,
        [Parameter(Mandatory=$true)][string]$ExpectedBaseCommit)
    Assert-KmgSprint29GiantsQualification -Ledger $Ledger -Candidate $Candidate -Notes $Notes
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or
        $RemoteMaster -cne $ExpectedBaseCommit -or
        $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or
        $PullRequest.baseRefName -cne 'master' -or
        $PullRequest.state -cne 'OPEN' -or
        $PullRequest.isDraft -ne $true -or
        $PullRequest.mergeable -cne 'MERGEABLE' -or
        $RemoteProductHead -cne $Candidate.commit -or
        $PullRequest.headRefOid -cne $Candidate.commit) {
        throw 'Sprint 29 premerge authority moved or is not the exact open draft candidate/base.'
    }
    if($Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or
        -not [string]::IsNullOrEmpty([string]$Ledger.publication.productMergeCommit) -or
        $Ledger.publication.mergeContainsTestedSource -ne $false -or
        $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
        [string]$Ledger.publication.productPr -cne '64') {
        throw 'Sprint 29 premerge proof must record its pending merge and pinned base/PR.'
    }
}
