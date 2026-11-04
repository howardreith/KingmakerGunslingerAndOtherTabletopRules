Set-StrictMode -Version Latest

function Assert-KmgSprint39ProductRoot([string]$Root,[string]$ProductPrNumber) {
    if($ProductPrNumber -cnotmatch '^[1-9][0-9]*$' -or
       [IO.Path]::GetFullPath($Root).TrimEnd('\') -ine 'C:\Dev\KingmakerGunslingerLab\wt\s39') {
        throw 'Sprint39 requires its numeric product PR and assigned product/build/promotion root.'
    }
}

function Get-KmgSprint39Surface {
    $repositoryRoot=Split-Path -Parent $PSScriptRoot
    $program=@'
import json,sys
from pathlib import Path
sys.path.insert(0,str(Path(sys.argv[1])/'tools'))
import expanded_summoning_manifest as m
c=m.parsed_creatures(); hidden=m.suppressed_keys()
scope={'nalfeshnee':9}
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
  routes.append(dict(creature=k,tier=t,quantity=quantity,identity=registry[symbol],assertion='sprint32-sprint39-public-player-route-Monster.'+str(t)+'.'+k+'.'+quantity))
print(json.dumps(dict(scopeExact=exact,creatures=sorted(scope),routes=routes,registeredRoots=monster+ally,generatedRoots=monster+ally-suppressed,nativeWrappers=len(m.NATIVE_EXPANDED_OPTIONS),suppressed=suppressed,choices=monster+ally-suppressed+len(m.NATIVE_EXPANDED_OPTIONS))))
'@
    $out=@(& python -B -c $program $repositoryRoot)
    if($LASTEXITCODE -ne 0) { throw 'Cannot derive Sprint39 catalog.' }
    $scope=(($out -join '') | ConvertFrom-Json)
    $surface=Get-KmgSprint29SourceSurface
    foreach($name in @('scopeExact','creatures','routes')){Add-Member -InputObject $surface -MemberType NoteProperty -Name $name -Value $scope.$name}
    Add-Member -InputObject $surface -MemberType NoteProperty -Name scopePublic -Value ($scope.suppressed -eq 0)
    return $surface
}

function Assert-KmgSprint39Reuse($Row,$Candidate) {
    # Sprint 39 reuses nothing: the Nalfeshnee is a new creature and every one
    # of its mechanics proofs is fresh against this candidate's exact bytes.
    throw 'Sprint39 admits no reused coverage row.'
}

function Assert-KmgSprint39FreshExecution($Source,$Candidate,$Row) {
    # The same contract Sprints 37 and 38 settled, reused unchanged: a row
    # binds to the bytes the game actually loaded, not merely to a package path
    # on disk. The engine writes its own loaded-build receipt at startup
    # carrying the commit, the DLL digest and the module version id of the
    # assembly in the process, so that receipt is required for every source and
    # is what the candidate is compared against.
    foreach($proof in @($Source.executionProof,$Source.loadedIdentityProof)) {
        if($null -eq $proof -or @($Row.proofs|Where-Object {$_.path -ceq $proof.path -and $_.sha256 -ceq $proof.sha256}).Count -ne 1) {
            throw 'Fresh Sprint39 execution attribution omitted.'
        }
    }
    $loaded=Read-KmgSprints3031Proof $Source.loadedIdentityProof
    if($loaded.gitCommit -cne $Candidate.commit -or $loaded.moduleVersionId -cne $Candidate.dllMvid -or
       $loaded.loadedModuleSha256 -cne $Candidate.dllSha256 -or $loaded.semanticVersion -cne $Candidate.version) {
        throw 'Fresh Sprint39 execution loaded other bytes.'
    }
    $execution=Read-KmgSprints3031Proof $Source.executionProof
    if($Source.executionKind -ceq 'installed-stack') {
        if($execution.candidateKind -cne 'release-candidate' -or -not $execution.releaseQualification -or
            -not $execution.restoration.verified -or -not $execution.restoration.gameSettingsExact -or
            $execution.restoration.transactionStatus -cne 'Restored' -or
            ($execution.PSObject.Properties['error'] -and $execution.error) -or
            $execution.launchReceipt.runId -cne $Source.runId -or
            [IO.Path]::GetFullPath((Join-Path $execution.launchReceipt.evidenceDirectory 'runtime-result.json')) -ine [IO.Path]::GetFullPath($Source.nativeProof.path)) {
            throw 'Fresh Sprint39 installed-stack attribution or restoration differs.'
        }
        foreach($key in @('commit','packageSha256','dllSha256','dllMvid')) {
            if([string]$execution.candidate.$key -cne [string]$Candidate.$key) { throw 'Fresh Sprint39 installed-stack used a different artifact.' }
        }
    } elseif($Source.executionKind -ceq 'restored-scenario') {
        # The disposable mechanics reviews run under the snapshot-and-restore
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
            throw 'Fresh Sprint39 restored-scenario attribution or restoration differs.'
        }
    } else { throw 'Unsupported fresh Sprint39 execution receipt.' }
}

function Assert-KmgSprint39Qualification($Ledger,$Candidate) {
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint39Nalfeshnee' -or
       $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or $Ledger.sprint40Started -or
       @($Ledger.baselineExceptions).Count -or @($Ledger.unresolvedFindings).Count) {
        throw 'Sprint39 qualification incomplete.'
    }
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),@('dllSha256','dllSha256'),@('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))) {
        if($Ledger.candidate.($pair[0]) -cne $Candidate.($pair[1])) { throw 'Final Sprint39 artifact differs.' }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or $suite.failures -ne 0 -or
       $suite.executed -le 0 -or $suite.executed -ne $suite.registered -or -not $suite.unfiltered) {
        throw 'Complete Sprint39 gate did not pass.'
    }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or -not $Ledger.restoration.workingFixtureClean) {
        throw 'Exact Sprint39 restoration incomplete.'
    }
    if($Ledger.ownerVisualReview -cne 'NOT_PERFORMED_NONBLOCKING') { throw 'No personal owner aesthetic approval has been supplied.' }
    $surface=Get-KmgSprint39Surface
    foreach($key in @('projectCreatures','monsterCreatures','monsterPlacements','natureAllyCreatures','natureAllyPlacements','generatedRoots','nativeWrappers','choices')) {
        if($Ledger.roster.$key -ne $surface.$key) { throw 'Source-derived Sprint39 public surface differs.' }
    }
    if(-not $surface.scopeExact -or -not $surface.scopePublic -or -not $Ledger.roster.protectedIdentitySetsEqual -or
       $surface.projectCreatures -ne 133 -or $surface.monsterCreatures -ne 111 -or $surface.monsterPlacements -ne 599 -or
       $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or
       $surface.generatedRoots -ne 1187 -or $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1216) {
        throw 'Exact Sprint39 scope, totals or preserved identities differ.'
    }
    # The owner-facing scope decisions, named rather than implied. Each is a
    # disclosed omission, a settled narrowing or a measured deviation, not a
    # waiver: changing or broadening one refuses.
    $decisions=$Ledger.scopeDecisions
    if($Ledger.gameplayWaiver -or
       (@($decisions.nalfeshneeSlaOmissions|Sort-Object -CaseSensitive) -join ',') -cne 'Summon demon reinforcements' -or
       (@($decisions.everySummonExclusions|Sort-Object -CaseSensitive) -join ',') -cne 'Greater Teleport,Nested Summoning,Wish' -or
       (@($decisions.settledNarrowings|Sort-Object -CaseSensitive) -join ',') -cne 'At most one pending Unholy Nimbus per creature,Constant Unholy Aura is an exact-source self effect with no ally grant,Feeblemind forbids spellcasting and not magic items,Greater Dispel Magic carries one decisive check over its ordered effects' -or
       (@($decisions.measuredDeviations|Sort-Object -CaseSensitive) -join ',') -cne 'Effective movement is the printed 40-foot fly with the 30-foot land mode omitted,Slow carries the project-recorded medium reach rather than the printed close derivation' -or
       (@($decisions.unrepresentedSkills|Sort-Object -CaseSensitive) -join ',') -cne 'Bluff,darkvision,Diplomacy,Intimidate,Knowledge,Sense Motive,telepathy,Use Magic Device' -or
       @($decisions.otherGameplayOmissions).Count) { throw 'Sprint39 owner decisions changed or broadened.' }
    # Sprint 39 coverage. Every mechanics row is fresh; nothing is reused. The
    # two Unholy Nimbus rows are separate because the two combat modes are
    # separate phases - game time only advances with turns in one of them - and
    # a single row could otherwise be satisfied by one mode alone.
    $required=@('core-profile-and-body','unholy-nimbus-real-time','unholy-nimbus-turn-based',
        'spells-aura-feeblemind-and-dispel',
        'public-routes-presentation','mixed-on-persistence','mixed-off-persistence',
        'highest-risk-integration','complete-source-build-package-gate','exact-reproducibility')
    $ids=@($Ledger.coverage|ForEach-Object id)
    if($ids.Count -ne $required.Count -or @($ids|Sort-Object -Unique).Count -ne $ids.Count -or
       @($required|Where-Object {$ids -cnotcontains $_}).Count) { throw 'Sprint39 coverage incomplete, extra or duplicated.' }
    foreach($row in $Ledger.coverage) {
        if($row.result -cne 'PASS' -or @($row.proofs).Count -eq 0) { throw 'Sprint39 coverage proof absent or reused.' }
        if($row.id -cnotin @('complete-source-build-package-gate','exact-reproducibility')) {
            if(@($row.evidenceSources).Count -eq 0) { throw 'Fresh Sprint39 native evidence source absent.' }
            foreach($source in $row.evidenceSources) {
                if($source.source -cne $Candidate.commit -or $source.packageSha256 -cne $Candidate.packageSha256 -or
                   $source.dllSha256 -cne $Candidate.dllSha256 -or $source.dllMvid -cne $Candidate.dllMvid) {
                    throw 'Fresh Sprint39 proof is bound to other bytes.'
                }
                Assert-KmgSprint39FreshExecution $source $Candidate $row
                $native=Read-KmgSprints3031Proof $source.nativeProof
                if($native.gitCommit -cne $Candidate.commit -or $native.runId -cne $source.runId -or
                   $native.status -cne 'PASS' -or @($native.assertions|Where-Object status -cne 'PASS').Count -or
                   @($source.requiredAssertions).Count -eq 0) { throw 'Fresh Sprint39 native proof incomplete.' }
                foreach($name in $source.requiredAssertions) {
                    if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1) {
                        throw 'Sprint39 required assertion absent.'
                    }
                }
            }
        }
    }
    # Each printed fact this sprint is about must be named by the row that owns
    # it, so a passing phase cannot stand in for a phase that silently lost a
    # case. The runner prefixes every assertion of this scenario with
    # sprint32-, as the Sprint 36 to 38 contracts also recorded.
    $rows=@{
        'core-profile-and-body'=@('sprint32-sprint39-nalfeshnee-identities',
            'sprint32-sprint39-nalfeshnee-profile',
            'sprint32-sprint39-nalfeshnee-aura-not-double-counted',
            'sprint32-sprint39-nalfeshnee-feats',
            'sprint32-sprint39-nalfeshnee-authored-body');
        'unholy-nimbus-real-time'=@('sprint32-sprint39-nalfeshnee-nimbus-interface',
            'sprint32-sprint39-nalfeshnee-nimbus-activation',
            'sprint32-sprint39-nalfeshnee-nimbus-one-pending',
            'sprint32-sprint39-nalfeshnee-nimbus-delayed-burst',
            'sprint32-sprint39-nalfeshnee-nimbus-rolled-duration',
            'sprint32-sprint39-nalfeshnee-nimbus-recipients-at-burst',
            'sprint32-sprint39-nalfeshnee-nimbus-uses-exhausted',
            'sprint32-sprint39-nalfeshnee-nimbus-owners-independent',
            'sprint32-sprint39-nalfeshnee-nimbus-second-owner-resolves',
            'sprint32-sprint39-nalfeshnee-nimbus-destroyed-source');
        'unholy-nimbus-turn-based'=@('sprint32-sprint39-nalfeshnee-nimbus-turn-admission-turnbased',
            'sprint32-sprint39-nalfeshnee-nimbus-activation-turnbased',
            'sprint32-sprint39-nalfeshnee-nimbus-one-pending-turnbased',
            'sprint32-sprint39-nalfeshnee-nimbus-delayed-burst-turnbased',
            'sprint32-sprint39-nalfeshnee-nimbus-rolled-duration-turnbased',
            'sprint32-sprint39-nalfeshnee-nimbus-recipients-at-burst-turnbased');
        'spells-aura-feeblemind-and-dispel'=@('sprint32-sprint39-nalfeshnee-spells-at-will-declarations',
            'sprint32-sprint39-nalfeshnee-spells-general-resistance',
            'sprint32-sprint39-nalfeshnee-aura-self-only',
            'sprint32-sprint39-nalfeshnee-feeblemind-disability',
            'sprint32-sprint39-nalfeshnee-feeblemind-items-permitted',
            'sprint32-sprint39-nalfeshnee-feeblemind-lasting',
            'sprint32-sprint39-nalfeshnee-feeblemind-cure',
            'sprint32-sprint39-nalfeshnee-dispel-one-check-cap-and-no-retry',
            'sprint32-sprint39-nalfeshnee-spells-at-will-repeat')
    }
    foreach($id in $rows.Keys) {
        $row=@($Ledger.coverage|Where-Object id -ceq $id)[0]
        foreach($name in $rows[$id]) {
            if(@($row.evidenceSources|Where-Object {$_.requiredAssertions -ccontains $name}).Count -ne 1) {
                throw 'Sprint39 assertion absent from '+$id+': '+$name
            }
        }
    }
    $public=@($Ledger.coverage|Where-Object id -ceq 'public-routes-presentation')[0]
    if(@($public.routes).Count -ne @($surface.routes).Count) { throw 'Sprint39 public routes missing.' }
    foreach($route in $surface.routes) {
        $match=@($public.routes|Where-Object {$_.creature -ceq $route.creature -and $_.tier -eq $route.tier})
        if($match.Count -ne 1 -or $match[0].identity -cne $route.identity -or
           (@($match[0].observed|Sort-Object -CaseSensitive) -join ',') -cne 'cleanup,duration,eligibility,quantity,slot' -or
           @($public.evidenceSources|Where-Object {$_.runId -ceq $match[0].runId -and $_.requiredAssertions -ccontains $route.assertion}).Count -ne 1) {
            throw 'Sprint39 public route lacks its exact observed consumers.'
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
            throw 'Sprint39 mixed persistence envelope incomplete.'
        }
        foreach($stage in $sequence.stages) {
            $sources=@($row.evidenceSources|Where-Object runId -ceq $stage.runId)
            if($sources.Count -ne 1 -or $stage.rawSha256 -cne $sources[0].nativeProof.sha256 -or
               [string]::IsNullOrWhiteSpace($stage.processIdentity)) { throw 'Sprint39 persistence stage attribution absent.' }
            $runs+=$stage.runId;$processes+=$stage.processIdentity
        }
    }
    if(@($runs|Sort-Object -Unique).Count -ne 6 -or @($processes|Sort-Object -Unique).Count -ne 6) {
        throw 'Sprint39 mixed ON/OFF requires six fresh processes.'
    }
    # The saved state this creature actually carries has to be named by the
    # envelope that proves it, in both module states. The roster-persistence
    # runner prefixes every assertion of these scenarios with
    # whole-roster-persistence-, just as the mechanics scenario prefixes its
    # own with sprint32-; naming the bare cell would match nothing at all,
    # which is a refusal that says "absent" when the truth is "named wrongly".
    foreach($sequenceName in @('on','off')) {
        $row=@($Ledger.coverage|Where-Object id -ceq ('mixed-'+$sequenceName+'-persistence'))[0]
        foreach($name in @('whole-roster-persistence-s39-items-owned',
            'whole-roster-persistence-s39-nimbus-pending-armed')) {
            if(@($row.evidenceSources|Where-Object {$_.requiredAssertions -ccontains $name}).Count -eq 0) {
                throw 'Sprint39 persistence envelope does not name its own saved state: '+$name
            }
        }
    }
    $repro=Read-KmgSprints3031Proof $Ledger.candidate.reproducibilityProof
    if($repro.source -cne $Candidate.commit -or $repro.primaryZipSha256 -cne $Candidate.packageSha256 -or
       $repro.rebuiltZipSha256 -cne $Candidate.packageSha256 -or $repro.primaryDllSha256 -cne $Candidate.dllSha256 -or
       $repro.rebuiltDllSha256 -cne $Candidate.dllSha256 -or $repro.exitCode -ne 0) {
        throw 'Exact Sprint39 reproducibility is incomplete.'
    }
    if($Ledger.publication.tag -cne ('v'+$Candidate.version) -or $Ledger.publication.draft -or
       $Ledger.publication.prerelease -or -not $Ledger.publication.latest) { throw 'Stable Sprint39 publication identity differs.' }
}

function Assert-KmgSprint39PremergeAuthority($Ledger,$Candidate,$PullRequest,$RemoteProductHead,$RemoteMaster,$ExpectedBaseCommit,$ProductPrNumber) {
    Assert-KmgSprint39Qualification $Ledger $Candidate
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or $RemoteMaster -cne $ExpectedBaseCommit -or
       $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or $PullRequest.baseRefName -cne 'master' -or
       $PullRequest.state -cne 'OPEN' -or -not $PullRequest.isDraft -or $PullRequest.mergeable -cne 'MERGEABLE' -or
       $RemoteProductHead -cne $Candidate.commit -or $PullRequest.headRefOid -cne $Candidate.commit -or
       $Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or $Ledger.publication.productMergeCommit -or
       $Ledger.publication.mergeContainsTestedSource -or $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
       [string]$Ledger.publication.productPr -cne $ProductPrNumber) { throw 'Sprint39 premerge source/base/PR authority differs.' }
}
