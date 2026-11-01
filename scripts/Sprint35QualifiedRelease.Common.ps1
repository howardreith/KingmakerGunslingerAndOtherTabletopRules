Set-StrictMode -Version Latest

function Assert-KmgSprint35ProductRoot([string]$Root,[string]$ProductPrNumber) {
    if($ProductPrNumber -cnotmatch '^[1-9][0-9]*$' -or
       [IO.Path]::GetFullPath($Root).TrimEnd('\') -ine 'C:\Dev\KingmakerGunslingerLab\wt\s35') {
        throw 'Sprint35 requires its numeric product PR and assigned product/build/promotion root.'
    }
}

function Get-KmgSprint35Surface {
    $repositoryRoot=Split-Path -Parent $PSScriptRoot
    $program=@'
import json,sys
from pathlib import Path
sys.path.insert(0,str(Path(sys.argv[1])/'tools'))
import expanded_summoning_manifest as m
c=m.parsed_creatures(); hidden=m.suppressed_keys()
scope={'barbed-devil':8,'bone-devil':7}
selected=[x for x in c if x['key'] in scope]
exact=len(selected)==2 and all(x['monster']==scope[x['key']] and x['ally'] is None and not x['templated'] for x in selected)
monster=sum(10-x['monster'] for x in c if x['monster'] is not None)
ally=sum(10-x['ally'] for x in c if x['ally'] is not None)
suppressed=sum((10-x['monster'] if x['monster'] is not None else 0)+(10-x['ally'] if x['ally'] is not None else 0) for x in c if x['key'] in hidden)
registry={x['symbol']:x['guid'] for x in json.loads((Path(sys.argv[1])/'blueprints/blueprints.json').read_text(encoding='utf-8-sig'))['entries']}
routes=[]
for k,v in scope.items():
 for t in range(v,10):
  quantity='One' if t==v else 'OneD3' if t==v+1 else 'OneD4PlusOne'
  symbol=f'KMG.Summoning.Ability.SM.Tier{t}.{m.token(k)}.{quantity}'
  routes.append(dict(creature=k,tier=t,identity=registry[symbol],assertion='sprint32-sprint35-public-player-route-Monster.'+str(t)+'.'+k+'.'+quantity))
print(json.dumps(dict(scopeExact=exact,creatures=sorted(scope),routes=routes,registeredRoots=monster+ally,generatedRoots=monster+ally-suppressed,nativeWrappers=len(m.NATIVE_EXPANDED_OPTIONS),suppressed=suppressed,choices=monster+ally-suppressed+len(m.NATIVE_EXPANDED_OPTIONS))))
'@
    $out=@(& python -B -c $program $repositoryRoot)
    if($LASTEXITCODE -ne 0) { throw 'Cannot derive Sprint35 catalog.' }
    $scope=(($out -join '') | ConvertFrom-Json)
    $surface=Get-KmgSprint29SourceSurface
    foreach($name in @('scopeExact','creatures','routes')){Add-Member -InputObject $surface -MemberType NoteProperty -Name $name -Value $scope.$name}
    Add-Member -InputObject $surface -MemberType NoteProperty -Name scopePublic -Value ($scope.suppressed -eq 0)
    return $surface
}

function Get-KmgSprint35EvidenceCandidate($Ledger,$Candidate,$Row,$Source) {
    $exact=$Source.source -ceq $Candidate.commit -and
        $Source.packageSha256 -ceq $Candidate.packageSha256 -and
        $Source.dllSha256 -ceq $Candidate.dllSha256 -and
        $Source.dllMvid -ceq $Candidate.dllMvid
    if($exact){return $Candidate}
    if(-not $Ledger.PSObject.Properties['observerOnlyReuse']){
        throw 'Fresh proof is bound to other bytes.'
    }
    $reuse=$Ledger.observerOnlyReuse
    $prior=[pscustomobject]@{
        commit='f785c3035662c7baeae5e0c5f82e85973e03e489'
        packageSha256='83b8432b04257615d79c66489ac162e4860ffbcdf50db86aaa7a0162fb953b53'
        dllSha256='3c1bd7a69b65b6b7fe0f6f2e9de83f6db2a7201a5b8a3f615eb000ae7efd6bc1'
        dllMvid='e50d50e9-9ada-446f-8f6a-28699fa1735a'
        sourceStateSha256='c2887e734f84a3f5eb1744e7f26cf6e55da27b81b9f4ab0e9605d33ad129651c'
    }
    $allowed=@('early-wall-bootstrap','barbed-behavior','barbed-combat',
        'bone-behavior','public-routes-presentation')
    $compiledObservers=@(
        'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningResourceReceipts.cs',
        'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.InfernalPersistence35.cs')
    if($reuse.reason -cne 'EXACT_ISOLATED_OBSERVER_CORRECTIONS_ONLY' -or
       (@($reuse.compiledObservers|Sort-Object -CaseSensitive) -join ',') -cne
            (@($compiledObservers|Sort-Object -CaseSensitive) -join ',') -or
       (@($reuse.allowedRows|Sort-Object -CaseSensitive) -join ',') -cne
            (@($allowed|Sort-Object -CaseSensitive) -join ',') -or
       $allowed -cnotcontains $Row.id){throw 'Unsupported Sprint35 observer-only evidence reuse.'}
    foreach($key in @('commit','packageSha256','dllSha256','dllMvid','sourceStateSha256')){
        if([string]$reuse.prior.$key -cne [string]$prior.$key){throw 'Observer-only prior tuple differs.'}
    }
    foreach($key in @('source','packageSha256','dllSha256','dllMvid')){
        $priorKey=if($key -ceq 'source'){'commit'}else{$key}
        if([string]$Source.$key -cne [string]$prior.$priorKey){throw 'Reused evidence differs from the exact prior tuple.'}
    }
    $root=Split-Path -Parent $PSScriptRoot
    $changed=@(& git -C $root diff --name-only --no-renames $prior.commit $Candidate.commit -- src/KingmakerGunslinger | Sort-Object -CaseSensitive)
    if($LASTEXITCODE -ne 0 -or ($changed -join ',') -cne
       (@($compiledObservers|Sort-Object -CaseSensitive) -join ',')){
        throw 'Candidate has a compiled change outside the exact observer corrections.'
    }
    return $prior
}

function Assert-KmgSprint35Qualification($Ledger,$Candidate) {
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint35InfernalReach' -or
       $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or $Ledger.sprint36Started -or
       @($Ledger.baselineExceptions).Count -or @($Ledger.unresolvedFindings).Count){throw 'Sprint35 qualification incomplete.'}
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),@('dllSha256','dllSha256'),@('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))){
        if($Ledger.candidate.($pair[0]) -cne $Candidate.($pair[1])){throw 'Final artifact differs.'}
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or $suite.failures -ne 0 -or
       $suite.executed -le 0 -or $suite.executed -ne $suite.registered -or -not $suite.unfiltered){throw 'Complete gate did not pass.'}
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or -not $Ledger.restoration.workingFixtureClean){throw 'Exact restoration incomplete.'}
    if($Ledger.ownerVisualReview -cne 'NOT_PERFORMED_NONBLOCKING'){throw 'No personal owner aesthetic approval has been supplied.'}
    $surface=Get-KmgSprint35Surface
    foreach($key in @('projectCreatures','monsterCreatures','monsterPlacements','natureAllyCreatures','natureAllyPlacements','generatedRoots','nativeWrappers','choices')){
        if($Ledger.roster.$key -ne $surface.$key){throw 'Source-derived public surface differs.'}
    }
    if(-not $surface.scopeExact -or -not $surface.scopePublic -or -not $Ledger.roster.protectedIdentitySetsEqual){throw 'Exact public scope and preserved identities required.'}
    $decisions=$Ledger.scopeDecisions
    if($Ledger.gameplayWaiver -or $decisions.wallOfIce -cne 'ONE_GROUNDED_10FT_GRIDGRAPH_MOVEMENT_ONLY_NO_LOE' -or
       (@($decisions.barbedSlaOmissions|Sort-Object -CaseSensitive) -join ',') -cne 'Major Image,Order''s Wrath,Produce Flame,Pyrotechnics' -or
       (@($decisions.boneSlaOmissions|Sort-Object -CaseSensitive) -join ',') -cne 'Dimensional Anchor,Major Image' -or
       @($decisions.otherGameplayOmissions).Count){throw 'Owner decisions changed or broadened.'}
    $required=@('early-wall-bootstrap','barbed-behavior','barbed-combat','bone-behavior','public-routes-presentation',
        'mixed-on-persistence','mixed-off-persistence','highest-risk-integration','complete-source-build-package-gate','exact-reproducibility')
    $ids=@($Ledger.coverage|ForEach-Object id)
    if($ids.Count -ne $required.Count -or @($ids|Sort-Object -Unique).Count -ne $ids.Count -or @($required|Where-Object {$ids -cnotcontains $_}).Count){throw 'Coverage incomplete, extra or duplicated.'}
    foreach($row in $Ledger.coverage){
        if($row.result -cne 'PASS' -or @($row.proofs).Count -eq 0 -or @($row.evidenceSources).Count -eq 0){throw 'Fresh coverage proof absent.'}
        foreach($source in $row.evidenceSources){
            [void](Get-KmgSprint35EvidenceCandidate $Ledger $Candidate $row $source)
        }
    }
    foreach($row in $Ledger.coverage|Where-Object {$_.id -cnotin @('complete-source-build-package-gate','exact-reproducibility')}) {
        foreach($source in $row.evidenceSources) {
            $evidenceCandidate=Get-KmgSprint35EvidenceCandidate $Ledger $Candidate $row $source
            Assert-KmgSprints3031FreshExecution $source $evidenceCandidate $row
            $native=Read-KmgSprints3031Proof $source.nativeProof
            if($native.gitCommit -cne $evidenceCandidate.commit -or $native.runId -cne $source.runId -or $native.status -cne 'PASS' -or
               @($native.assertions|Where-Object status -cne 'PASS').Count -or @($source.requiredAssertions).Count -eq 0){throw 'Fresh native proof incomplete.'}
            foreach($name in $source.requiredAssertions){if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1){throw 'Fresh required assertion absent.'}}
            if(@($row.proofs|Where-Object {$_.path -ceq $source.nativeProof.path -and $_.sha256 -ceq $source.nativeProof.sha256}).Count -ne 1){throw 'Fresh raw proof omitted.'}
        }
    }
    $public=@($Ledger.coverage|Where-Object id -ceq 'public-routes-presentation')[0]
    if(@($public.routes).Count -ne @($surface.routes).Count){throw 'Public routes missing.'}
    foreach($route in $surface.routes) {
        $match=@($public.routes|Where-Object {$_.creature -ceq $route.creature -and $_.tier -eq $route.tier})
        if($match.Count -ne 1 -or $match[0].identity -cne $route.identity -or
           (@($match[0].observed|Sort-Object -CaseSensitive) -join ',') -cne 'cleanup,duration,eligibility,quantity,slot' -or
           @($public.evidenceSources|Where-Object {$_.runId -ceq $match[0].runId -and $_.requiredAssertions -ccontains $route.assertion}).Count -ne 1){throw 'Public route lacks its exact observed consumers.'}
    }
    $processes=@();$runs=@()
    foreach($sequenceName in @('on','off')) {
        $row=@($Ledger.coverage|Where-Object id -ceq ('mixed-'+$sequenceName+'-persistence'))[0]
        $sequence=Read-KmgSprints3031Proof $row.sequenceProof
        if($sequence.source -cne $Candidate.commit -or $sequence.sequence -cne $sequenceName -or $sequence.status -cne 'PASS' -or
           -not $sequence.restoration.exactIntake -or -not $sequence.restoration.gameExited -or
           (@($sequence.stages.stage) -join ',') -cne 'prepare,cleanup,absent' -or @($row.evidenceSources).Count -ne 3 -or
           @($row.proofs|Where-Object {$_.path -ceq $row.sequenceProof.path -and $_.sha256 -ceq $row.sequenceProof.sha256}).Count -ne 1){throw 'Mixed persistence envelope incomplete.'}
        foreach($stage in $sequence.stages) {
            $sources=@($row.evidenceSources|Where-Object runId -ceq $stage.runId)
            if($sources.Count -ne 1 -or $stage.rawSha256 -cne $sources[0].nativeProof.sha256 -or [string]::IsNullOrWhiteSpace($stage.processIdentity)){throw 'Persistence stage attribution absent.'}
            $runs+= $stage.runId; $processes+= $stage.processIdentity
        }
    }
    if(@($runs|Sort-Object -Unique).Count -ne 6 -or @($processes|Sort-Object -Unique).Count -ne 6){throw 'Mixed ON/OFF requires six fresh processes.'}
    $repro=Read-KmgSprints3031Proof $Ledger.candidate.reproducibilityProof
    if($repro.source -cne $Candidate.commit -or $repro.primaryZipSha256 -cne $Candidate.packageSha256 -or
       $repro.rebuiltZipSha256 -cne $Candidate.packageSha256 -or $repro.primaryDllSha256 -cne $Candidate.dllSha256 -or
       $repro.rebuiltDllSha256 -cne $Candidate.dllSha256 -or $repro.exitCode -ne 0){throw 'Exact reproducibility is incomplete.'}
    if($Ledger.publication.tag -cne ('v'+$Candidate.version) -or $Ledger.publication.draft -or $Ledger.publication.prerelease -or -not $Ledger.publication.latest){throw 'Stable publication identity differs.'}
}

function Assert-KmgSprint35PremergeAuthority($Ledger,$Candidate,$PullRequest,$RemoteProductHead,$RemoteMaster,$ExpectedBaseCommit,$ProductPrNumber) {
    Assert-KmgSprint35Qualification $Ledger $Candidate
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or $RemoteMaster -cne $ExpectedBaseCommit -or
       $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or $PullRequest.baseRefName -cne 'master' -or
       $PullRequest.state -cne 'OPEN' -or -not $PullRequest.isDraft -or $PullRequest.mergeable -cne 'MERGEABLE' -or
       $RemoteProductHead -cne $Candidate.commit -or $PullRequest.headRefOid -cne $Candidate.commit -or
       $Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or $Ledger.publication.productMergeCommit -or
       $Ledger.publication.mergeContainsTestedSource -or $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
       [string]$Ledger.publication.productPr -cne $ProductPrNumber) {throw 'Sprint35 premerge source/base/PR authority differs.'}
}
