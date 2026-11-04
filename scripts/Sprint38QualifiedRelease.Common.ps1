Set-StrictMode -Version Latest

function Assert-KmgSprint38ProductRoot([string]$Root,[string]$ProductPrNumber) {
    if($ProductPrNumber -cnotmatch '^[1-9][0-9]*$' -or
       [IO.Path]::GetFullPath($Root).TrimEnd('\') -ine 'C:\Dev\KingmakerGunslingerLab\wt\s38') {
        throw 'Sprint38 requires its numeric product PR and assigned product/build/promotion root.'
    }
}

function Get-KmgSprint38Surface {
    $repositoryRoot=Split-Path -Parent $PSScriptRoot
    $program=@'
import json,sys
from pathlib import Path
sys.path.insert(0,str(Path(sys.argv[1])/'tools'))
import expanded_summoning_manifest as m
c=m.parsed_creatures(); hidden=m.suppressed_keys()
scope={'glabrezu':9}
selected=[x for x in c if x['key'] in scope]
exact=len(selected)==1 and all(x['monster']==scope[x['key']] and x['ally'] is None and not x['templated'] for x in selected)
monster=sum(10-x['monster'] for x in c if x['monster'] is not None)
ally=sum(10-x['ally'] for x in c if x['ally'] is not None)
suppressed=sum((10-x['monster'] if x['monster'] is not None else 0)+(10-x['ally'] if x['ally'] is not None else 0) for x in c if x['key'] in hidden)
registry={x['symbol']:x['guid'] for x in json.loads((Path(sys.argv[1])/'blueprints/blueprints.json').read_text(encoding='utf-8-sig'))['entries']}
routes=[]
for k,v in scope.items():
 for t in range(v,10):
  quantity='One' if t==v else 'OneD3' if t==v+1 else 'OneD4PlusOne'
  symbol=f'KMG.Summoning.Ability.SM.Tier{t}.{m.token(k)}.{quantity}'
  routes.append(dict(creature=k,tier=t,quantity=quantity,identity=registry[symbol],assertion='sprint32-sprint38-public-player-route-Monster.'+str(t)+'.'+k+'.'+quantity))
print(json.dumps(dict(scopeExact=exact,creatures=sorted(scope),routes=routes,registeredRoots=monster+ally,generatedRoots=monster+ally-suppressed,nativeWrappers=len(m.NATIVE_EXPANDED_OPTIONS),suppressed=suppressed,choices=monster+ally-suppressed+len(m.NATIVE_EXPANDED_OPTIONS))))
'@
    $out=@(& python -B -c $program $repositoryRoot)
    if($LASTEXITCODE -ne 0) { throw 'Cannot derive Sprint38 catalog.' }
    $scope=(($out -join '') | ConvertFrom-Json)
    $surface=Get-KmgSprint29SourceSurface
    foreach($name in @('scopeExact','creatures','routes')){Add-Member -InputObject $surface -MemberType NoteProperty -Name $name -Value $scope.$name}
    Add-Member -InputObject $surface -MemberType NoteProperty -Name scopePublic -Value ($scope.suppressed -eq 0)
    return $surface
}

function Assert-KmgSprint38Reuse($Row,$Candidate) {
    # Sprint 38 reuses nothing: the Glabrezu is a new creature and every one of
    # its mechanics proofs is fresh against this candidate's exact bytes.
    throw 'Sprint38 admits no reused coverage row.'
}

function Assert-KmgSprint38FreshExecution($Source,$Candidate,$Row) {
    # The same contract Sprint 37 settled, reused unchanged: a row binds to the
    # bytes the game actually loaded, not merely to a package path on disk. The
    # engine writes its own loaded-build receipt at startup carrying the commit,
    # the DLL digest and the module version id of the assembly in the process,
    # so that receipt is required for every source and is what the candidate is
    # compared against.
    foreach($proof in @($Source.executionProof,$Source.loadedIdentityProof)) {
        if($null -eq $proof -or @($Row.proofs|Where-Object {$_.path -ceq $proof.path -and $_.sha256 -ceq $proof.sha256}).Count -ne 1) {
            throw 'Fresh Sprint38 execution attribution omitted.'
        }
    }
    $loaded=Read-KmgSprints3031Proof $Source.loadedIdentityProof
    if($loaded.gitCommit -cne $Candidate.commit -or $loaded.moduleVersionId -cne $Candidate.dllMvid -or
       $loaded.loadedModuleSha256 -cne $Candidate.dllSha256 -or $loaded.semanticVersion -cne $Candidate.version) {
        throw 'Fresh Sprint38 execution loaded other bytes.'
    }
    $execution=Read-KmgSprints3031Proof $Source.executionProof
    if($Source.executionKind -ceq 'installed-stack') {
        if($execution.candidateKind -cne 'release-candidate' -or -not $execution.releaseQualification -or
            -not $execution.restoration.verified -or -not $execution.restoration.gameSettingsExact -or
            $execution.restoration.transactionStatus -cne 'Restored' -or
            ($execution.PSObject.Properties['error'] -and $execution.error) -or
            $execution.launchReceipt.runId -cne $Source.runId -or
            [IO.Path]::GetFullPath((Join-Path $execution.launchReceipt.evidenceDirectory 'runtime-result.json')) -ine [IO.Path]::GetFullPath($Source.nativeProof.path)) {
            throw 'Fresh Sprint38 installed-stack attribution or restoration differs.'
        }
        foreach($key in @('commit','packageSha256','dllSha256','dllMvid')) {
            if([string]$execution.candidate.$key -cne [string]$Candidate.$key) { throw 'Fresh Sprint38 installed-stack used a different artifact.' }
        }
    } elseif($Source.executionKind -ceq 'restored-scenario') {
        # The disposable mechanics review runs under the snapshot-and-restore
        # wrapper rather than a compatibility profile, so the wrapper's own
        # verified restoration record is the execution receipt. Nothing here is
        # authored: these are its recorded facts, including that the live tree
        # it mutated came back to the exact hash it started from.
        $leaf=[IO.Path]::GetFileName([IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($Source.nativeProof.path)))
        $runs=@($execution.runs|Where-Object {[IO.Path]::GetFileName(([string]$_.evidenceDirectory).TrimEnd('\')) -ieq $leaf})
        if($execution.restoration -cne 'verified' -or $execution.failures -ne 0 -or
            $execution.expectedVersion -cne $Candidate.version -or
            [string]$execution.liveBefore.sha256 -cne [string]$execution.liveAfterRestore.sha256 -or
            $runs.Count -ne 1 -or $runs[0].outcome -cne 'PASS' -or $runs[0].scenarioStatus -cne 'PASS' -or
            $runs[0].exitCode -ne 0 -or
            [IO.Path]::GetFullPath([string]$runs[0].package) -ine [IO.Path]::GetFullPath([string]$Candidate.packagePath)) {
            throw 'Fresh Sprint38 restored-scenario attribution or restoration differs.'
        }
    } else { throw 'Unsupported fresh Sprint38 execution receipt.' }
}

function Assert-KmgSprint38Qualification($Ledger,$Candidate) {
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint38Glabrezu' -or
       $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or $Ledger.sprint39Started -or
       @($Ledger.baselineExceptions).Count -or @($Ledger.unresolvedFindings).Count) {
        throw 'Sprint38 qualification incomplete.'
    }
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),@('dllSha256','dllSha256'),@('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))) {
        if($Ledger.candidate.($pair[0]) -cne $Candidate.($pair[1])) { throw 'Final Sprint38 artifact differs.' }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or $suite.failures -ne 0 -or
       $suite.executed -le 0 -or $suite.executed -ne $suite.registered -or -not $suite.unfiltered) {
        throw 'Complete Sprint38 gate did not pass.'
    }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or -not $Ledger.restoration.workingFixtureClean) {
        throw 'Exact Sprint38 restoration incomplete.'
    }
    if($Ledger.ownerVisualReview -cne 'NOT_PERFORMED_NONBLOCKING') { throw 'No personal owner aesthetic approval has been supplied.' }
    $surface=Get-KmgSprint38Surface
    foreach($key in @('projectCreatures','monsterCreatures','monsterPlacements','natureAllyCreatures','natureAllyPlacements','generatedRoots','nativeWrappers','choices')) {
        if($Ledger.roster.$key -ne $surface.$key) { throw 'Source-derived Sprint38 public surface differs.' }
    }
    if(-not $surface.scopeExact -or -not $surface.scopePublic -or -not $Ledger.roster.protectedIdentitySetsEqual -or
       $surface.projectCreatures -ne 132 -or $surface.monsterCreatures -ne 110 -or $surface.monsterPlacements -ne 598 -or
       $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or
       $surface.generatedRoots -ne 1186 -or $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1215) {
        throw 'Exact Sprint38 scope, totals or preserved identities differ.'
    }
    # The owner-facing scope decisions, named rather than implied. Each is a
    # disclosed omission or a measured deviation, not a waiver: changing or
    # broadening one refuses.
    $decisions=$Ledger.scopeDecisions
    if($Ledger.gameplayWaiver -or
       (@($decisions.glabrezuSlaOmissions|Sort-Object -CaseSensitive) -join ',') -cne 'Reverse Gravity,Veil' -or
       (@($decisions.everySummonExclusions|Sort-Object -CaseSensitive) -join ',') -cne 'Greater Teleport,Nested Summoning,Wish' -or
       (@($decisions.measuredDeviations|Sort-Object -CaseSensitive) -join ',') -cne 'Live Perception 28 from the installed Persuasive feat' -or
       (@($decisions.unrepresentedSkills|Sort-Object -CaseSensitive) -join ',') -cne 'Bluff,darkvision,Diplomacy,Intimidate,Knowledge,Sense Motive,telepathy,Use Magic Device' -or
       @($decisions.otherGameplayOmissions).Count) { throw 'Sprint38 owner decisions changed or broadened.' }
    # Sprint 38 coverage. Every mechanics row is fresh; nothing is reused.
    $required=@('core-profile-and-body','rend-round-contract','power-word-stun',
        'at-will-spells-and-bursts','observed-ai-target-binding',
        'public-routes-presentation','mixed-on-persistence','mixed-off-persistence',
        'highest-risk-integration','complete-source-build-package-gate','exact-reproducibility')
    $ids=@($Ledger.coverage|ForEach-Object id)
    if($ids.Count -ne $required.Count -or @($ids|Sort-Object -Unique).Count -ne $ids.Count -or
       @($required|Where-Object {$ids -cnotcontains $_}).Count) { throw 'Sprint38 coverage incomplete, extra or duplicated.' }
    foreach($row in $Ledger.coverage) {
        if($row.result -cne 'PASS' -or @($row.proofs).Count -eq 0) { throw 'Sprint38 coverage proof absent or reused.' }
        if($row.id -cnotin @('complete-source-build-package-gate','exact-reproducibility')) {
            if(@($row.evidenceSources).Count -eq 0) { throw 'Fresh Sprint38 native evidence source absent.' }
            foreach($source in $row.evidenceSources) {
                if($source.source -cne $Candidate.commit -or $source.packageSha256 -cne $Candidate.packageSha256 -or
                   $source.dllSha256 -cne $Candidate.dllSha256 -or $source.dllMvid -cne $Candidate.dllMvid) {
                    throw 'Fresh Sprint38 proof is bound to other bytes.'
                }
                Assert-KmgSprint38FreshExecution $source $Candidate $row
                $native=Read-KmgSprints3031Proof $source.nativeProof
                if($native.gitCommit -cne $Candidate.commit -or $native.runId -cne $source.runId -or
                   $native.status -cne 'PASS' -or @($native.assertions|Where-Object status -cne 'PASS').Count -or
                   @($source.requiredAssertions).Count -eq 0) { throw 'Fresh Sprint38 native proof incomplete.' }
                foreach($name in $source.requiredAssertions) {
                    if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1) {
                        throw 'Sprint38 required assertion absent.'
                    }
                }
            }
        }
    }
    # Each printed fact this sprint is about must be named by the row that
    # owns it, so a passing phase cannot stand in for a phase that silently
    # lost a case. The runner prefixes every assertion of this scenario with
    # sprint32-, as the Sprint 36 and 37 contracts also recorded.
    $rows=@{
        'core-profile-and-body'=@('sprint32-sprint38-core-bootstrap',
            'sprint32-sprint38-core-five-primary-attacks',
            'sprint32-sprint38-core-defenses',
            'sprint32-sprint38-core-feats-and-constant-sense',
            'sprint32-sprint38-core-attack-fallback');
        'rend-round-contract'=@('sprint32-sprint38-rend-limbs',
            'sprint32-sprint38-rend-round-clock-is-live',
            'sprint32-sprint38-rend-live-attack-command',
            'sprint32-sprint38-rend-pair-rends-once',
            'sprint32-sprint38-rend-other-limbs-never',
            'sprint32-sprint38-rend-history-is-per-victim',
            'sprint32-sprint38-rend-round-not-command');
        'power-word-stun'=@('sprint32-sprint38-stun-range-is-printed-close',
            'sprint32-sprint38-stun-buckets-follow-current-hit-points',
            'sprint32-sprint38-stun-refuses-an-immune-recipient',
            'sprint32-sprint38-stun-resistance-uses-printed-caster-level',
            'sprint32-sprint38-vrock-screech-numerical-resistance-unchanged');
        'at-will-spells-and-bursts'=@('sprint32-sprint38-spells-at-will-carry-no-resource',
            'sprint32-sprint38-spells-at-will-repeat',
            'sprint32-sprint38-spells-bursts-at-printed-difficulty');
        'observed-ai-target-binding'=@('sprint32-sprint38-ai-second-enemy-is-real',
            'sprint32-sprint38-ai-candidates-differ',
            'sprint32-sprint38-ai-stun-picks-the-affectable-candidate')
    }
    foreach($id in $rows.Keys) {
        $row=@($Ledger.coverage|Where-Object id -ceq $id)[0]
        foreach($name in $rows[$id]) {
            if(@($row.evidenceSources|Where-Object {$_.requiredAssertions -ccontains $name}).Count -ne 1) {
                throw 'Sprint38 assertion absent from '+$id+': '+$name
            }
        }
    }
    $public=@($Ledger.coverage|Where-Object id -ceq 'public-routes-presentation')[0]
    if(@($public.routes).Count -ne @($surface.routes).Count) { throw 'Sprint38 public routes missing.' }
    foreach($route in $surface.routes) {
        $match=@($public.routes|Where-Object {$_.creature -ceq $route.creature -and $_.tier -eq $route.tier})
        if($match.Count -ne 1 -or $match[0].identity -cne $route.identity -or
           (@($match[0].observed|Sort-Object -CaseSensitive) -join ',') -cne 'cleanup,duration,eligibility,quantity,slot' -or
           @($public.evidenceSources|Where-Object {$_.runId -ceq $match[0].runId -and $_.requiredAssertions -ccontains $route.assertion}).Count -ne 1) {
            throw 'Sprint38 public route lacks its exact observed consumers.'
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
            throw 'Sprint38 mixed persistence envelope incomplete.'
        }
        foreach($stage in $sequence.stages) {
            $sources=@($row.evidenceSources|Where-Object runId -ceq $stage.runId)
            if($sources.Count -ne 1 -or $stage.rawSha256 -cne $sources[0].nativeProof.sha256 -or
               [string]::IsNullOrWhiteSpace($stage.processIdentity)) { throw 'Sprint38 persistence stage attribution absent.' }
            $runs+=$stage.runId;$processes+=$stage.processIdentity
        }
    }
    if(@($runs|Sort-Object -Unique).Count -ne 6 -or @($processes|Sort-Object -Unique).Count -ne 6) {
        throw 'Sprint38 mixed ON/OFF requires six fresh processes.'
    }
    # The saved state this creature actually carries has to be named by the
    # envelope that proves it, in both module states.
    foreach($sequenceName in @('on','off')) {
        $row=@($Ledger.coverage|Where-Object id -ceq ('mixed-'+$sequenceName+'-persistence'))[0]
        # The roster-persistence runner prefixes every assertion of these
        # scenarios with whole-roster-persistence-, just as the mechanics
        # scenario prefixes its own with sprint32-. Naming the bare cell here
        # would have matched nothing at all, which is a refusal that says
        # "absent" when the truth is "named wrongly".
        foreach($name in @('whole-roster-persistence-s38-items-owned',
            'whole-roster-persistence-s38-stun-stood-then-withdrawn')) {
            if(@($row.evidenceSources|Where-Object {$_.requiredAssertions -ccontains $name}).Count -eq 0) {
                throw 'Sprint38 persistence envelope does not name its own saved state: '+$name
            }
        }
    }
    $repro=Read-KmgSprints3031Proof $Ledger.candidate.reproducibilityProof
    if($repro.source -cne $Candidate.commit -or $repro.primaryZipSha256 -cne $Candidate.packageSha256 -or
       $repro.rebuiltZipSha256 -cne $Candidate.packageSha256 -or $repro.primaryDllSha256 -cne $Candidate.dllSha256 -or
       $repro.rebuiltDllSha256 -cne $Candidate.dllSha256 -or $repro.exitCode -ne 0) {
        throw 'Exact Sprint38 reproducibility is incomplete.'
    }
    if($Ledger.publication.tag -cne ('v'+$Candidate.version) -or $Ledger.publication.draft -or
       $Ledger.publication.prerelease -or -not $Ledger.publication.latest) { throw 'Stable Sprint38 publication identity differs.' }
}

function Assert-KmgSprint38PremergeAuthority($Ledger,$Candidate,$PullRequest,$RemoteProductHead,$RemoteMaster,$ExpectedBaseCommit,$ProductPrNumber) {
    Assert-KmgSprint38Qualification $Ledger $Candidate
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or $RemoteMaster -cne $ExpectedBaseCommit -or
       $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or $PullRequest.baseRefName -cne 'master' -or
       $PullRequest.state -cne 'OPEN' -or -not $PullRequest.isDraft -or $PullRequest.mergeable -cne 'MERGEABLE' -or
       $RemoteProductHead -cne $Candidate.commit -or $PullRequest.headRefOid -cne $Candidate.commit -or
       $Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or $Ledger.publication.productMergeCommit -or
       $Ledger.publication.mergeContainsTestedSource -or $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
       [string]$Ledger.publication.productPr -cne $ProductPrNumber) { throw 'Sprint38 premerge source/base/PR authority differs.' }
}
