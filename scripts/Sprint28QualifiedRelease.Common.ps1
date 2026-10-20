Set-StrictMode -Version Latest

function Get-KmgSprint28SourceSurface {
    $repositoryRoot=Split-Path -Parent $PSScriptRoot
    $python=Get-Command python -ErrorAction Stop
    $program=@"
import json, sys
from pathlib import Path
sys.path.insert(0, str(Path(sys.argv[1]) / 'tools'))
import expanded_summoning_manifest as m
creatures=m.parsed_creatures()
suppressed=m.suppressed_keys()
monster=sum(10-x['monster'] for x in creatures if x['monster'] is not None)
ally=sum(10-x['ally'] for x in creatures if x['ally'] is not None)
hidden=sum((10-x['monster'] if x['monster'] is not None else 0)+(10-x['ally'] if x['ally'] is not None else 0) for x in creatures if x['key'] in suppressed)
expected={'hill-giant':6,'stone-giant':6,'fire-giant':7}
scoped=[x for x in creatures if x['key'] in expected]
exact=len(scoped)==3 and set(x['key'] for x in scoped)==set(expected) and all(x['monster'] is None and not x['templated'] and x['ally']==expected[x['key']] for x in scoped)
placements=sum(10-x['ally'] for x in scoped)
scoped_hidden=sum(10-x['ally'] for x in scoped if x['key'] in suppressed)
wrappers=len(m.NATIVE_EXPANDED_OPTIONS)
print(json.dumps(dict(sprint28RosterExact=exact,sprint28Placements=placements,sprint28SuppressedPlacements=scoped_hidden,projectCreatures=len(creatures),monsterCreatures=sum(x['monster'] is not None for x in creatures),monsterPlacements=monster,natureAllyCreatures=sum(x['ally'] is not None for x in creatures),natureAllyPlacements=ally,registeredRoots=monster+ally,generatedRoots=monster+ally-hidden,nativeWrappers=wrappers,choices=monster+ally-hidden+wrappers,suppressedLogicalPlacements=hidden)))
"@
    $output=@(& $python.Source -B -c $program $repositoryRoot)
    if($LASTEXITCODE -ne 0) { throw 'Cannot derive the current Sprint 28 source surface.' }
    return (($output -join '') | ConvertFrom-Json)
}

function Assert-KmgSprint28GiantsQualification {
    param([Parameter(Mandatory=$true)]$Ledger,[Parameter(Mandatory=$true)]$Candidate)
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint28Giants' -or
        $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or
        $Ledger.sprint29Started -or @($Ledger.baselineExceptions).Count -ne 0 -or
        @($Ledger.unresolvedFindings).Count -ne 0 -or $Ledger.expandedScope -or
        $Ledger.gameplayWaiver) {
        throw 'Sprint 28 has unresolved scope, failed qualification or a new gameplay waiver.'
    }
    $skill=$Ledger.skillDecision
    if($skill.ownerAuthorized -ne $true -or
        $skill.authority -cne 'Owner Sprint 28 closeout directive 2026-10-20' -or
        $skill.disposition -cne 'DEFER_NAMED_CLIMB_CRAFT_SPRINT28' -or
        (@($skill.deferredSkillNames) -join '|') -cne 'Climb|Craft' -or
        (@($skill.affectedCreatures) -join '|') -cne
            'hill-giant|stone-giant|fire-giant' -or
        (@($skill.printedValues) -join '|') -cne
            'Hill Climb +10|Stone Climb +12|Fire Climb +14|Fire Craft (any one) +8' -or
        $skill.nativePitBehavior -cne 'UNCHANGED' -or
        $skill.newStats -ne $false -or $skill.rankReallocation -ne $false -or
        $skill.substituteBonuses -ne $false -or
        @($skill.otherGameplayOmissions).Count -ne 0 -or
        $skill.disclosurePath -cne 'docs/RELEASE-NOTES-0.0.162.md') {
        throw 'Sprint 28 named Climb/Craft decision or exact disclosure is missing or broadened.'
    }
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),
        @('dllSha256','dllSha256'),@('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))) {
        if([string]$Ledger.candidate.($pair[0]) -cne [string]$Candidate.($pair[1])) {
            throw 'Sprint 28 tested source or artifact identity differs.'
        }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or
        $suite.failures -ne 0 -or $suite.executed -le 0 -or
        $suite.executed -ne $suite.registered -or -not $suite.unfiltered) {
        throw 'Sprint 28 candidate did not pass a fresh complete nonempty suite.'
    }
    $surface=Get-KmgSprint28SourceSurface
    foreach($key in @('projectCreatures','monsterCreatures','monsterPlacements',
        'natureAllyCreatures','natureAllyPlacements','generatedRoots','nativeWrappers',
        'choices','suppressedLogicalPlacements')) {
        if($Ledger.roster.$key -ne $surface.$key) {
            throw "Sprint 28 source-derived publication surface differs: $key"
        }
    }
    if(-not $surface.sprint28RosterExact -or -not $Ledger.roster.sprint28RosterExact -or
        $surface.sprint28Placements -ne 11 -or $Ledger.roster.sprint28Placements -ne 11 -or
        $surface.sprint28SuppressedPlacements -ne 0 -or
        $Ledger.roster.sprint28SuppressedPlacements -ne 0 -or
        $surface.suppressedLogicalPlacements -ne 0 -or
        $Ledger.roster.identityChanges -ne 0 -or $Ledger.roster.routeOrderingChanges -ne 0 -or
        $Ledger.roster.protectedIdentitySetsEqual -ne $true) {
        throw 'Sprint 28 public roots or protected identities/routes are not qualified.'
    }
    if($Ledger.ownerVisualReview -cnotin @('NOT_PERFORMED_NONBLOCKING','APPROVED') -or
        ($Ledger.ownerVisualReview -ceq 'APPROVED' -and
            [string]::IsNullOrWhiteSpace([string]$Ledger.ownerVisualReviewEvidence))) {
        throw 'Sprint 28 owner visual-review status lacks its required provenance.'
    }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or
        -not $Ledger.restoration.workingFixtureClean) {
        throw 'Sprint 28 requires exact human intake restoration and a clean working fixture.'
    }
    if($Ledger.publication.tag -cne ('v'+$Candidate.version) -or
        $Ledger.publication.prerelease -or $Ledger.publication.draft -or
        -not $Ledger.publication.latest) {
        throw 'Sprint 28 stable publication lineage/state is invalid.'
    }
    $fresh=@('composed-adult-profiles-melee-rock-and-feats',
        'native-rock-range-catching-fire-damage-and-controls',
        'rtwp-and-turn-based-native-commands-and-ai',
        'owned-equipment-corpse-loot-and-functional-bodies-icons',
        'targeted-persistence-on-off-cleanup-fresh-absence',
        'eleven-public-roots-quantities-and-protected-identity',
        'standalone-and-high-risk-shared-controls',
        'complete-public-source-build-package-gate',
        'public-inventory-icon-eleven-roots','exact-package-release-qualification')
    $mandatory=@('inherited-unaffected-qualification')+$fresh
    $ids=@($Ledger.coverage | ForEach-Object {$_.id})
    if(@($ids | Sort-Object -Unique).Count -ne $ids.Count) {
        throw 'Sprint 28 duplicate coverage identity.'
    }
    foreach($id in $mandatory) {
        $row=@($Ledger.coverage | Where-Object {$_.id -ceq $id})
        if($row.Count -ne 1 -or $row[0].result -cnotin @('PASS','REUSED_PASS') -or
            ($id -cin $fresh -and $row[0].result -cne 'PASS')) {
            throw "Sprint 28 mandatory fresh/reused coverage is incomplete: $id"
        }
    }
    foreach($row in $Ledger.coverage) {
        if($row.result -cnotin @('PASS','REUSED_PASS') -or @($row.proofs).Count -eq 0 -or
            @($row.evidenceSources).Count -eq 0) {
            throw "Sprint 28 coverage lacks proof: $($row.id)"
        }
        foreach($source in $row.evidenceSources) {
            if($source.source -cnotmatch '^[0-9a-f]{40}$' -or
                $source.packageSha256 -cnotmatch '^[0-9a-f]{64}$' -or
                $source.dllSha256 -cnotmatch '^[0-9a-f]{64}$' -or
                $source.dllMvid -cnotmatch '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$') {
                throw 'Sprint 28 execution provenance is malformed.'
            }
            if($row.result -ceq 'PASS' -and ($source.source -cne $Candidate.commit -or
                $source.packageSha256 -cne $Candidate.packageSha256 -or
                $source.dllSha256 -cne $Candidate.dllSha256 -or
                $source.dllMvid -cne $Candidate.dllMvid)) {
                throw 'Sprint 28 fresh proof is bound to other bytes.'
            }
        }
        if($row.result -ceq 'REUSED_PASS' -and
            ([string]::IsNullOrWhiteSpace([string]$row.reuseJustification) -or
            [string]::IsNullOrWhiteSpace([string]$row.priorSource))) {
            throw 'Sprint 28 reuse lacks dependency/provenance evidence.'
        }
    }
}

function Assert-KmgSprint28GiantsReleaseDecision {
    param([Parameter(Mandatory=$true)]$Ledger,[Parameter(Mandatory=$true)]$Candidate)
    Assert-KmgSprint28GiantsQualification -Ledger $Ledger -Candidate $Candidate
    if($Ledger.publication.productMergeCommit -cnotmatch '^[0-9a-f]{40}$' -or
        $Ledger.publication.mergeContainsTestedSource -ne $true) {
        throw 'Sprint 28 publication requires the actual verified product merge.'
    }
}

function Assert-KmgSprint28PremergeAuthority {
    param([Parameter(Mandatory=$true)]$Ledger,[Parameter(Mandatory=$true)]$Candidate,
        [Parameter(Mandatory=$true)]$PullRequest,
        [Parameter(Mandatory=$true)][string]$RemoteProductHead,
        [Parameter(Mandatory=$true)][string]$RemoteMaster,
        [Parameter(Mandatory=$true)][string]$ExpectedBaseCommit)
    Assert-KmgSprint28GiantsQualification -Ledger $Ledger -Candidate $Candidate
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or
        $RemoteMaster -cne $ExpectedBaseCommit -or
        $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or
        $PullRequest.baseRefName -cne 'master' -or $PullRequest.state -cne 'OPEN' -or
        $PullRequest.isDraft -ne $true -or $PullRequest.mergeable -cne 'MERGEABLE' -or
        $RemoteProductHead -cne $Candidate.commit -or
        $PullRequest.headRefOid -cne $Candidate.commit) {
        throw 'Sprint 28 premerge authority moved or is not the exact open draft candidate/base.'
    }
    if($Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or
        -not [string]::IsNullOrEmpty([string]$Ledger.publication.productMergeCommit) -or
        $Ledger.publication.mergeContainsTestedSource -ne $false -or
        $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
        [string]$Ledger.publication.productPr -cne '62') {
        throw 'Sprint 28 premerge proof must record its pending merge and exact pinned base/PR.'
    }
}
