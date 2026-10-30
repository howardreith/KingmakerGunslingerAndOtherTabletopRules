Set-StrictMode -Version Latest

function Assert-KmgSprint34ProductRoot([string]$Root,[string]$ProductPrNumber) {
    if($ProductPrNumber -cnotmatch '^[1-9][0-9]*$' -or
       [IO.Path]::GetFullPath($Root).TrimEnd('\') -ine 'C:\Dev\KingmakerGunslingerLab\wt\s34') {
        throw 'Sprint34 requires its numeric product PR and assigned product/build/promotion root.'
    }
}
function Assert-KmgSprint34Reuse($Row,$Candidate) {
    if($Row.result -cne 'REUSED_PASS' -or
       $Row.id -cnotin @('profiles-defenses-isolation','barbazu-combat-effects','kyton-actions-defenses','ai-actions-cleanup') -or
       [string]::IsNullOrWhiteSpace($Row.reuseJustification)){throw 'Unselected Sprint34 evidence reuse.'}
    Assert-KmgScopedDevelopmentReuse $Row $Candidate 'C:\Dev\KingmakerGunslingerLab\wt\s34' `
        'codex/expanded-summoning-sprint34-bearded-devil-kyton' `
        @('sprint34-infernal-early','sprint34-infernal-bodies','sprint34-infernal-mechanics')
}
function Get-KmgSprint34Surface {
    $repositoryRoot=Split-Path -Parent $PSScriptRoot
    $program=@'
import json,sys
from pathlib import Path
sys.path.insert(0,str(Path(sys.argv[1])/'tools'))
import expanded_summoning_manifest as m
c=m.parsed_creatures(); hidden=m.suppressed_keys()
scope={'bearded-devil':5,'kyton-chain-devil':5}
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
  routes.append(dict(creature=k,tier=t,identity=registry[symbol],assertion='sprint32-sprint34-player-route-Monster.'+str(t)+'.'+k+'.'+quantity))
print(json.dumps(dict(scopeExact=exact,creatures=sorted(scope),routes=routes,registeredRoots=monster+ally,generatedRoots=monster+ally-suppressed,nativeWrappers=len(m.NATIVE_EXPANDED_OPTIONS),suppressed=suppressed,choices=monster+ally-suppressed+len(m.NATIVE_EXPANDED_OPTIONS))))
'@
    $out=@(& python -B -c $program $repositoryRoot)
    if($LASTEXITCODE -ne 0) { throw 'Cannot derive Sprint34 catalog.' }
    $scope=(($out -join '') | ConvertFrom-Json)
    $surface=Get-KmgSprint29SourceSurface
    foreach($name in @('scopeExact','creatures','routes')){Add-Member -InputObject $surface -MemberType NoteProperty -Name $name -Value $scope.$name}
    Add-Member -InputObject $surface -MemberType NoteProperty -Name scopePublic -Value ($scope.suppressed -eq 0)
    return $surface
}

function Assert-KmgSprint34Qualification($Ledger,$Candidate) {
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint34InfernalOutsiders' -or
       $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or $Ledger.sprint35Started -or
       @($Ledger.baselineExceptions).Count -or @($Ledger.unresolvedFindings).Count){throw 'Sprint34 qualification incomplete.'}
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),@('dllSha256','dllSha256'),@('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))){
        if($Ledger.candidate.($pair[0]) -cne $Candidate.($pair[1])){throw 'Final artifact differs.'}
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or $suite.failures -ne 0 -or
       $suite.executed -le 0 -or $suite.executed -ne $suite.registered -or -not $suite.unfiltered){throw 'Complete gate did not pass.'}
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or -not $Ledger.restoration.workingFixtureClean){throw 'Exact restoration incomplete.'}
    if($Ledger.ownerVisualReview -cne 'NOT_PERFORMED_NONBLOCKING'){throw 'No personal owner aesthetic approval has been supplied.'}
    $surface=Get-KmgSprint34Surface
    foreach($key in @('projectCreatures','monsterCreatures','monsterPlacements','natureAllyCreatures','natureAllyPlacements','generatedRoots','nativeWrappers','choices')){
        if($Ledger.roster.$key -ne $surface.$key){throw 'Source-derived public surface differs.'}
    }
    if(-not $surface.scopeExact -or -not $surface.scopePublic -or -not $Ledger.roster.protectedIdentitySetsEqual){throw 'Exact public scope and preserved identities required.'}
    if($Ledger.gameplayWaiver -or $Ledger.scopeDecisions.dancingChains -cne 'STANDARD_ACTION_OWN_FOUR_NO_ACTIVATION_ATTACK' -or
       $Ledger.scopeDecisions.gaze -cne 'TARGETED_STANDARD_30FT_ELIGIBLE_ALLIES' -or
       $Ledger.scopeDecisions.healSkill -cne 'OWNER_OMITTED_NO_SUBSTITUTE' -or
       @($Ledger.scopeDecisions.otherGameplayOmissions).Count){throw 'Owner decisions changed or broadened.'}
    $required=@('profiles-defenses-isolation','barbazu-combat-effects','kyton-actions-defenses','ai-actions-cleanup',
        'models-icons-ten-routes','mixed-on-persistence','mixed-off-persistence','highest-risk-integration',
        'complete-source-build-package-gate','exact-reproducibility')
    $ids=@($Ledger.coverage|ForEach-Object id)
    if(@($ids|Sort-Object -Unique).Count -ne $ids.Count -or @($required|Where-Object {$ids -cnotcontains $_}).Count){throw 'Coverage incomplete or duplicated.'}
    foreach($row in $Ledger.coverage){
        if($row.result -cnotin @('PASS','REUSED_PASS') -or @($row.proofs).Count -eq 0 -or @($row.evidenceSources).Count -eq 0){throw 'Coverage proof absent.'}
        if($row.result -ceq 'REUSED_PASS'){Assert-KmgSprint34Reuse $row $Candidate}
        else {foreach($source in $row.evidenceSources){foreach($key in @('packageSha256','dllSha256','dllMvid')){if($source.$key -cne $Candidate.$key){throw 'Fresh proof is bound to other bytes.'}};if($source.source -cne $Candidate.commit){throw 'Fresh source differs.'}}}
    }
    foreach($row in $Ledger.coverage|Where-Object {$_.result -ceq 'PASS' -and $_.id -cnotin @('complete-source-build-package-gate','exact-reproducibility')}) {
        foreach($source in $row.evidenceSources) {
            # Existing artifact/profile/installed-stack correlation helper has no campaign selection or controller side effects.
            Assert-KmgSprints3031FreshExecution $source $Candidate $row
            $native=Read-KmgSprints3031Proof $source.nativeProof
            if($native.gitCommit -cne $Candidate.commit -or $native.runId -cne $source.runId -or $native.status -cne 'PASS' -or
               @($native.assertions|Where-Object status -cne 'PASS').Count -or @($source.requiredAssertions).Count -eq 0){throw 'Fresh native proof incomplete.'}
            foreach($name in $source.requiredAssertions){if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1){throw 'Fresh required assertion absent.'}}
            if(@($row.proofs|Where-Object {$_.path -ceq $source.nativeProof.path -and $_.sha256 -ceq $source.nativeProof.sha256}).Count -ne 1){throw 'Fresh raw proof omitted.'}
        }
    }
    $public=@($Ledger.coverage|Where-Object id -ceq 'models-icons-ten-routes')[0]
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
            if($sources.Count -ne 1 -or $stage.rawSha256 -cne $sources[0].nativeProof.sha256 -or
               [string]::IsNullOrWhiteSpace($stage.processIdentity)){throw 'Persistence stage attribution absent.'}
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
function Assert-KmgSprint34PremergeAuthority($Ledger,$Candidate,$PullRequest,$RemoteProductHead,$RemoteMaster,$ExpectedBaseCommit,$ProductPrNumber) {
    Assert-KmgSprint34Qualification $Ledger $Candidate
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or $RemoteMaster -cne $ExpectedBaseCommit -or
       $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or $PullRequest.baseRefName -cne 'master' -or
       $PullRequest.state -cne 'OPEN' -or -not $PullRequest.isDraft -or $PullRequest.mergeable -cne 'MERGEABLE' -or
       $RemoteProductHead -cne $Candidate.commit -or $PullRequest.headRefOid -cne $Candidate.commit -or
       $Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or $Ledger.publication.productMergeCommit -or
       $Ledger.publication.mergeContainsTestedSource -or $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
       [string]$Ledger.publication.productPr -cne $ProductPrNumber) {throw 'Sprint34 premerge source/base/PR authority differs.'}
}