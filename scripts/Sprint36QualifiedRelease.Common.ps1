Set-StrictMode -Version Latest

function Assert-KmgSprint36ProductRoot([string]$Root,[string]$ProductPrNumber) {
    if($ProductPrNumber -cnotmatch '^[1-9][0-9]*$' -or
       [IO.Path]::GetFullPath($Root).TrimEnd('\') -ine 'C:\Dev\KingmakerGunslingerLab\wt\s36') {
        throw 'Sprint36 requires its numeric product PR and assigned product/build/promotion root.'
    }
}

function Get-KmgSprint36Surface {
    $repositoryRoot=Split-Path -Parent $PSScriptRoot
    $program=@'
import json,sys
from pathlib import Path
sys.path.insert(0,str(Path(sys.argv[1])/'tools'))
import expanded_summoning_manifest as m
c=m.parsed_creatures(); hidden=m.suppressed_keys()
scope={'babau':5,'hezrou':8}
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
  routes.append(dict(creature=k,tier=t,quantity=quantity,identity=registry[symbol],assertion='sprint32-sprint36-public-player-route-Monster.'+str(t)+'.'+k+'.'+quantity))
print(json.dumps(dict(scopeExact=exact,creatures=sorted(scope),routes=routes,registeredRoots=monster+ally,generatedRoots=monster+ally-suppressed,nativeWrappers=len(m.NATIVE_EXPANDED_OPTIONS),suppressed=suppressed,choices=monster+ally-suppressed+len(m.NATIVE_EXPANDED_OPTIONS))))
'@
    $out=@(& python -B -c $program $repositoryRoot)
    if($LASTEXITCODE -ne 0) { throw 'Cannot derive Sprint36 catalog.' }
    $scope=(($out -join '') | ConvertFrom-Json)
    $surface=Get-KmgSprint29SourceSurface
    foreach($name in @('scopeExact','creatures','routes')){Add-Member -InputObject $surface -MemberType NoteProperty -Name $name -Value $scope.$name}
    Add-Member -InputObject $surface -MemberType NoteProperty -Name scopePublic -Value ($scope.suppressed -eq 0)
    return $surface
}

function Assert-KmgSprint36Reuse($Row,$Candidate) {
    $allowed=@('profiles-registration','babau-combat-acid','hezrou-combat-control','both-modes-lifetime')
    if($Row.result -cne 'REUSED_PASS' -or $allowed -cnotcontains $Row.id -or
       $Row.priorSource -cnotmatch '^[0-9a-f]{40}$' -or
       $Row.reuseJustification -cne 'UNCHANGED_PRODUCTION_DEPENDENCIES_WITH_EXACT_ARTIFACT_ATTRIBUTION' -or
       @($Row.dependencyPaths).Count -eq 0 -or @($Row.evidenceSources).Count -eq 0) {
        throw 'Sprint36 reuse authority or provenance differs.'
    }
    foreach($path in $Row.dependencyPaths) {
        if($path -cnotmatch '^(src/KingmakerGunslinger|assets|assets-source|blueprints)/' -or
           $path -match '/RuntimeTesting/') { throw 'Sprint36 reuse dependency path is not a production input.' }
    }
    $root=Split-Path -Parent $PSScriptRoot
    & git -C $root diff --quiet $Row.priorSource $Candidate.commit -- @($Row.dependencyPaths)
    if($LASTEXITCODE -ne 0) { throw 'A production dependency changed after the reused Sprint36 proof.' }
    foreach($source in $Row.evidenceSources) {
        if($source.source -cne $Row.priorSource -or
           $source.packageSha256 -cnotmatch '^[0-9a-f]{64}$' -or
           $source.dllSha256 -cnotmatch '^[0-9a-f]{64}$' -or
           $source.dllMvid -cnotmatch '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$' -or
           @($source.requiredAssertions).Count -eq 0) { throw 'Reused Sprint36 source tuple is incomplete.' }
        $native=Read-KmgSprints3031Proof $source.nativeProof
        if($native.gitCommit -cne $source.source -or $native.runId -cne $source.runId -or
           $native.status -cne 'PASS' -or @($native.assertions|Where-Object status -cne 'PASS').Count) {
            throw 'Reused Sprint36 native proof differs from its exact execution.'
        }
        foreach($name in $source.requiredAssertions) {
            if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1) {
                throw 'Reused Sprint36 required assertion is absent.'
            }
        }
    }
}

function Assert-KmgSprint36Qualification($Ledger,$Candidate) {
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint36BabauHezrou' -or
       $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or $Ledger.sprint37Started -or
       @($Ledger.baselineExceptions).Count -or @($Ledger.unresolvedFindings).Count) {
        throw 'Sprint36 qualification incomplete.'
    }
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),@('dllSha256','dllSha256'),@('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))) {
        if($Ledger.candidate.($pair[0]) -cne $Candidate.($pair[1])) { throw 'Final Sprint36 artifact differs.' }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or $suite.failures -ne 0 -or
       $suite.executed -le 0 -or $suite.executed -ne $suite.registered -or -not $suite.unfiltered) {
        throw 'Complete Sprint36 gate did not pass.'
    }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or -not $Ledger.restoration.workingFixtureClean) {
        throw 'Exact Sprint36 restoration incomplete.'
    }
    if($Ledger.ownerVisualReview -cne 'NOT_PERFORMED_NONBLOCKING') { throw 'No personal owner aesthetic approval has been supplied.' }
    $surface=Get-KmgSprint36Surface
    foreach($key in @('projectCreatures','monsterCreatures','monsterPlacements','natureAllyCreatures','natureAllyPlacements','generatedRoots','nativeWrappers','choices')) {
        if($Ledger.roster.$key -ne $surface.$key) { throw 'Source-derived Sprint36 public surface differs.' }
    }
    if(-not $surface.scopeExact -or -not $surface.scopePublic -or -not $Ledger.roster.protectedIdentitySetsEqual -or
       $surface.projectCreatures -ne 130 -or $surface.monsterCreatures -ne 108 -or $surface.monsterPlacements -ne 594 -or
       $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or
       $surface.generatedRoots -ne 1182 -or $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1211) {
        throw 'Exact Sprint36 scope, totals or preserved identities differ.'
    }
    $decisions=$Ledger.scopeDecisions
    if($Ledger.gameplayWaiver -or
       (@($decisions.protectiveSlimeOmissions|Sort-Object -CaseSensitive) -join ',') -cne 'Ammunition branch,Manufactured-weapon branch' -or
       (@($decisions.babauSlaOmissions|Sort-Object -CaseSensitive) -join ',') -cne 'Darkness' -or
       (@($decisions.hezrouSlaOmissions|Sort-Object -CaseSensitive) -join ',') -cne 'Gaseous Form' -or
       @($decisions.otherGameplayOmissions).Count) { throw 'Sprint36 owner decisions changed or broadened.' }
    $required=@('profiles-registration','babau-combat-acid','hezrou-combat-control','both-modes-lifetime',
        'public-routes-presentation','mixed-on-persistence','mixed-off-persistence','highest-risk-integration',
        'complete-source-build-package-gate','exact-reproducibility')
    $ids=@($Ledger.coverage|ForEach-Object id)
    if($ids.Count -ne $required.Count -or @($ids|Sort-Object -Unique).Count -ne $ids.Count -or
       @($required|Where-Object {$ids -cnotcontains $_}).Count) { throw 'Sprint36 coverage incomplete, extra or duplicated.' }
    foreach($row in $Ledger.coverage) {
        if($row.result -cnotin @('PASS','REUSED_PASS') -or @($row.proofs).Count -eq 0) { throw 'Sprint36 coverage proof absent.' }
        if($row.result -ceq 'REUSED_PASS') { Assert-KmgSprint36Reuse $row $Candidate }
        elseif($row.id -cnotin @('complete-source-build-package-gate','exact-reproducibility')) {
            if(@($row.evidenceSources).Count -eq 0) { throw 'Fresh Sprint36 native evidence source absent.' }
            foreach($source in $row.evidenceSources) {
                if($source.source -cne $Candidate.commit -or $source.packageSha256 -cne $Candidate.packageSha256 -or
                   $source.dllSha256 -cne $Candidate.dllSha256 -or $source.dllMvid -cne $Candidate.dllMvid) {
                    throw 'Fresh Sprint36 proof is bound to other bytes.'
                }
                Assert-KmgSprints3031FreshExecution $source $Candidate $row
                $native=Read-KmgSprints3031Proof $source.nativeProof
                if($native.gitCommit -cne $Candidate.commit -or $native.runId -cne $source.runId -or
                   $native.status -cne 'PASS' -or @($native.assertions|Where-Object status -cne 'PASS').Count -or
                   @($source.requiredAssertions).Count -eq 0) { throw 'Fresh Sprint36 native proof incomplete.' }
                foreach($name in $source.requiredAssertions) {
                    if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1) {
                        throw 'Fresh Sprint36 required assertion absent.'
                    }
                }
            }
        }
    }
    $public=@($Ledger.coverage|Where-Object id -ceq 'public-routes-presentation')[0]
    if(@($public.routes).Count -ne @($surface.routes).Count) { throw 'Sprint36 public routes missing.' }
    foreach($route in $surface.routes) {
        $match=@($public.routes|Where-Object {$_.creature -ceq $route.creature -and $_.tier -eq $route.tier})
        if($match.Count -ne 1 -or $match[0].identity -cne $route.identity -or
           (@($match[0].observed|Sort-Object -CaseSensitive) -join ',') -cne 'cleanup,duration,eligibility,quantity,slot' -or
           @($public.evidenceSources|Where-Object {$_.runId -ceq $match[0].runId -and $_.requiredAssertions -ccontains $route.assertion}).Count -ne 1) {
            throw 'Sprint36 public route lacks its exact observed consumers.'
        }
    }
    $processes=@();$runs=@()
    foreach($sequenceName in @('on','off')) {
        $row=@($Ledger.coverage|Where-Object id -ceq ('mixed-'+$sequenceName+'-persistence'))[0]
        $sequence=Read-KmgSprints3031Proof $row.sequenceProof
        if($sequence.source -cne $Candidate.commit -or $sequence.sequence -cne $sequenceName -or $sequence.status -cne 'PASS' -or
           -not $sequence.restoration.exactIntake -or -not $sequence.restoration.gameExited -or
           (@($sequence.stages.stage) -join ',') -cne 'prepare,cleanup,absent' -or @($row.evidenceSources).Count -ne 3 -or
           @($row.proofs|Where-Object {$_.path -ceq $row.sequenceProof.path -and $_.sha256 -ceq $row.sequenceProof.sha256}).Count -ne 1) {
            throw 'Sprint36 mixed persistence envelope incomplete.'
        }
        foreach($stage in $sequence.stages) {
            $sources=@($row.evidenceSources|Where-Object runId -ceq $stage.runId)
            if($sources.Count -ne 1 -or $stage.rawSha256 -cne $sources[0].nativeProof.sha256 -or
               [string]::IsNullOrWhiteSpace($stage.processIdentity)) { throw 'Sprint36 persistence stage attribution absent.' }
            $runs+=$stage.runId;$processes+=$stage.processIdentity
        }
    }
    if(@($runs|Sort-Object -Unique).Count -ne 6 -or @($processes|Sort-Object -Unique).Count -ne 6) {
        throw 'Sprint36 mixed ON/OFF requires six fresh processes.'
    }
    $repro=Read-KmgSprints3031Proof $Ledger.candidate.reproducibilityProof
    if($repro.source -cne $Candidate.commit -or $repro.primaryZipSha256 -cne $Candidate.packageSha256 -or
       $repro.rebuiltZipSha256 -cne $Candidate.packageSha256 -or $repro.primaryDllSha256 -cne $Candidate.dllSha256 -or
       $repro.rebuiltDllSha256 -cne $Candidate.dllSha256 -or $repro.exitCode -ne 0) {
        throw 'Exact Sprint36 reproducibility is incomplete.'
    }
    if($Ledger.publication.tag -cne ('v'+$Candidate.version) -or $Ledger.publication.draft -or
       $Ledger.publication.prerelease -or -not $Ledger.publication.latest) { throw 'Stable Sprint36 publication identity differs.' }
}

function Assert-KmgSprint36PremergeAuthority($Ledger,$Candidate,$PullRequest,$RemoteProductHead,$RemoteMaster,$ExpectedBaseCommit,$ProductPrNumber) {
    Assert-KmgSprint36Qualification $Ledger $Candidate
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or $RemoteMaster -cne $ExpectedBaseCommit -or
       $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or $PullRequest.baseRefName -cne 'master' -or
       $PullRequest.state -cne 'OPEN' -or -not $PullRequest.isDraft -or $PullRequest.mergeable -cne 'MERGEABLE' -or
       $RemoteProductHead -cne $Candidate.commit -or $PullRequest.headRefOid -cne $Candidate.commit -or
       $Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or $Ledger.publication.productMergeCommit -or
       $Ledger.publication.mergeContainsTestedSource -or $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
       [string]$Ledger.publication.productPr -cne $ProductPrNumber) { throw 'Sprint36 premerge source/base/PR authority differs.' }
}
