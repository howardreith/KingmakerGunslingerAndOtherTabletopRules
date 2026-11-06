Set-StrictMode -Version Latest

function Assert-KmgSprint40ProductRoot([string]$Root,[string]$ProductPrNumber) {
    if($ProductPrNumber -cnotmatch '^[1-9][0-9]*$' -or
       [IO.Path]::GetFullPath($Root).TrimEnd('\') -ine 'C:\Dev\KingmakerGunslingerLab\wt\s40') {
        throw 'Sprint40 requires its numeric product PR and assigned product/build/promotion root.'
    }
}

function Get-KmgSprint40Surface {
    $repositoryRoot=Split-Path -Parent $PSScriptRoot
    $program=@'
import json,sys
from pathlib import Path
sys.path.insert(0,str(Path(sys.argv[1])/'tools'))
import expanded_summoning_manifest as m
c=m.parsed_creatures(); hidden=m.suppressed_keys(); scope={'ice-devil':9}
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
  routes.append(dict(creature=k,tier=t,quantity=quantity,identity=registry[symbol],assertion='sprint32-sprint40-public-player-route-Monster.'+str(t)+'.'+k+'.'+quantity))
print(json.dumps(dict(scopeExact=exact,scopePublic='ice-devil' not in hidden,creatures=sorted(scope),routes=routes,
 projectCreatures=len(c),monsterCreatures=sum(x['monster'] is not None for x in c),monsterPlacements=monster,
 natureAllyCreatures=sum(x['ally'] is not None for x in c),natureAllyPlacements=ally,
 registeredRoots=monster+ally,generatedRoots=monster+ally-suppressed,nativeWrappers=len(m.NATIVE_EXPANDED_OPTIONS),
 suppressed=suppressed,choices=monster+ally-suppressed+len(m.NATIVE_EXPANDED_OPTIONS))))
'@
    $out=@(& python -B -c $program $repositoryRoot)
    if($LASTEXITCODE -ne 0) { throw 'Cannot derive Sprint40 catalog.' }
    return (($out -join '') | ConvertFrom-Json)
}

function Assert-KmgSprint40Reuse($Row,$Candidate) {
    throw 'Sprint40 admits no reused coverage row.'
}

function Assert-KmgSprint40FreshExecution($Source,$Candidate,$Row) {
    foreach($proof in @($Source.executionProof,$Source.loadedIdentityProof)) {
        if($null -eq $proof -or @($Row.proofs|Where-Object {$_.path -ceq $proof.path -and $_.sha256 -ceq $proof.sha256}).Count -ne 1) {
            throw 'Fresh Sprint40 execution attribution omitted.'
        }
    }
    $loaded=Read-KmgSprints3031Proof $Source.loadedIdentityProof
    if($loaded.gitCommit -cne $Candidate.commit -or $loaded.moduleVersionId -cne $Candidate.dllMvid -or
       $loaded.loadedModuleSha256 -cne $Candidate.dllSha256 -or $loaded.semanticVersion -cne $Candidate.version) {
        throw 'Fresh Sprint40 execution loaded other bytes.'
    }
    $execution=Read-KmgSprints3031Proof $Source.executionProof
    if($Source.executionKind -ceq 'installed-stack') {
        if($execution.candidateKind -cne 'release-candidate' -or -not $execution.releaseQualification -or
           -not $execution.restoration.verified -or -not $execution.restoration.gameSettingsExact -or
           $execution.restoration.transactionStatus -cne 'Restored' -or
           ($execution.PSObject.Properties['error'] -and $execution.error) -or
           $execution.launchReceipt.runId -cne $Source.runId -or
           [IO.Path]::GetFullPath((Join-Path $execution.launchReceipt.evidenceDirectory 'runtime-result.json')) -ine [IO.Path]::GetFullPath($Source.nativeProof.path)) {
            throw 'Fresh Sprint40 installed-stack attribution or restoration differs.'
        }
        foreach($key in @('commit','packageSha256','dllSha256','dllMvid')) {
            if([string]$execution.candidate.$key -cne [string]$Candidate.$key) { throw 'Fresh Sprint40 installed-stack used a different artifact.' }
        }
    } elseif($Source.executionKind -ceq 'restored-scenario') {
        $leaf=[IO.Path]::GetFileName([IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($Source.nativeProof.path)))
        $runs=@($execution.runs|Where-Object {[IO.Path]::GetFileName(([string]$_.evidenceDirectory).TrimEnd('\')) -ieq $leaf})
        if($execution.restoration -cne 'verified' -or $execution.failures -ne 0 -or
           $execution.expectedVersion -cne $Candidate.version -or
           [string]$execution.liveBefore.sha256 -cne [string]$execution.liveAfterRestore.sha256 -or
           $runs.Count -ne 1 -or $runs[0].outcome -cne 'PASS' -or $runs[0].scenarioStatus -cne 'PASS' -or
           $runs[0].exitCode -ne 0 -or
           [IO.Path]::GetFullPath([string]$runs[0].package) -ine [IO.Path]::GetFullPath([string]$Candidate.packagePath)) {
            throw 'Fresh Sprint40 restored-scenario attribution or restoration differs.'
        }
    } else { throw 'Unsupported fresh Sprint40 execution receipt.' }
}

function Assert-KmgSprint40Qualification($Ledger,$Candidate) {
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint40IceDevil' -or
       $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or $Ledger.sprint41Started -or
       @($Ledger.baselineExceptions).Count -or @($Ledger.unresolvedFindings).Count) {
        throw 'Sprint40 qualification incomplete.'
    }
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),@('dllSha256','dllSha256'),
        @('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))) {
        if($Ledger.candidate.($pair[0]) -cne $Candidate.($pair[1])) { throw 'Final Sprint40 artifact differs.' }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or $suite.failures -ne 0 -or
       $suite.executed -le 0 -or $suite.executed -ne $suite.registered -or -not $suite.unfiltered) {
        throw 'Complete Sprint40 gate did not pass.'
    }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or -not $Ledger.restoration.workingFixtureClean) {
        throw 'Exact Sprint40 restoration incomplete.'
    }
    if($Ledger.ownerVisualReview -cne 'NOT_PERFORMED_NONBLOCKING') { throw 'No personal owner aesthetic approval has been supplied.' }
    $surface=Get-KmgSprint40Surface
    foreach($key in @('projectCreatures','monsterCreatures','monsterPlacements','natureAllyCreatures','natureAllyPlacements','generatedRoots','nativeWrappers','choices')) {
        if($Ledger.roster.$key -ne $surface.$key) { throw 'Source-derived Sprint40 public surface differs.' }
    }
    if(-not $surface.scopeExact -or -not $surface.scopePublic -or -not $Ledger.roster.protectedIdentitySetsEqual -or
       $surface.projectCreatures -ne 134 -or $surface.monsterCreatures -ne 112 -or $surface.monsterPlacements -ne 600 -or
       $surface.natureAllyCreatures -ne 105 -or $surface.natureAllyPlacements -ne 588 -or
       $surface.generatedRoots -ne 1188 -or $surface.nativeWrappers -ne 29 -or $surface.choices -ne 1217) {
        throw 'Exact Sprint40 scope, totals or preserved identities differ.'
    }
    $decisions=$Ledger.scopeDecisions
    if($Ledger.gameplayWaiver -or
       $decisions.persistentImageDisposition -cne 'OWNER_APPROVED_OMISSION_NO_SUBSTITUTE_NO_ENGINE_IMPOSSIBILITY_CLAIM' -or
       $decisions.wallAdaptation -cne 'S35_BOUNDED_SINGLE_GROUNDED_10FT_MOVEMENT_WALL_NO_LOE' -or
       (@($decisions.everySummonExclusions|Sort-Object -CaseSensitive) -join ',') -cne 'Greater Teleport,Nested Summoning' -or
       (@($decisions.measuredDeviations|Sort-Object -CaseSensitive) -join ',') -cne 'Effective movement is the printed 60-foot fly with the separate 40-foot land mode unmodeled' -or
       (@($decisions.sprint39CarryForwards|Sort-Object -CaseSensitive) -join ',') -cne 'NALFESHNEE_OWNED_SLOW_CL12_CLOSE_55_FEET,NIMBUS_PUBLIC_WORDING_DAZE_NOT_BLINDNESS' -or
       @($decisions.otherGameplayOmissions).Count) { throw 'Sprint40 owner decisions changed or broadened.' }
    $required=@('profile-equipment-combat','slow-real-time-and-turn-based','cold-wall-and-ai',
        'fear-regeneration-and-lifetime','presentation-public-route','affected-nalfeshnee-range',
        'mixed-on-persistence','mixed-off-persistence','complete-source-build-package-gate','exact-reproducibility')
    $ids=@($Ledger.coverage|ForEach-Object id)
    if($ids.Count -ne $required.Count -or @($ids|Sort-Object -Unique).Count -ne $ids.Count -or
       @($required|Where-Object {$ids -cnotcontains $_}).Count) { throw 'Sprint40 coverage incomplete, extra or duplicated.' }
    foreach($row in $Ledger.coverage) {
        if($row.result -cne 'PASS' -or @($row.proofs).Count -eq 0) { throw 'Sprint40 coverage proof absent or reused.' }
        if($row.id -cnotin @('complete-source-build-package-gate','exact-reproducibility')) {
            if(@($row.evidenceSources).Count -eq 0) { throw 'Fresh Sprint40 native evidence source absent.' }
            foreach($source in $row.evidenceSources) {
                if($source.source -cne $Candidate.commit -or $source.packageSha256 -cne $Candidate.packageSha256 -or
                   $source.dllSha256 -cne $Candidate.dllSha256 -or $source.dllMvid -cne $Candidate.dllMvid) {
                    throw 'Fresh Sprint40 proof is bound to other bytes.'
                }
                Assert-KmgSprint40FreshExecution $source $Candidate $row
                $native=Read-KmgSprints3031Proof $source.nativeProof
                if($native.gitCommit -cne $Candidate.commit -or $native.runId -cne $source.runId -or
                   $native.status -cne 'PASS' -or @($native.assertions|Where-Object status -cne 'PASS').Count -or
                   @($source.requiredAssertions).Count -eq 0) { throw 'Fresh Sprint40 native proof incomplete.' }
                foreach($name in $source.requiredAssertions) {
                    if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1) {
                        throw 'Sprint40 required assertion absent.'
                    }
                }
            }
        }
    }
    $rows=@{
        'profile-equipment-combat'=@('sprint32-sprint40-profile-live-totals','sprint32-sprint40-native-weapon-contributions',
            'sprint32-sprint40-native-full-routine','sprint32-sprint40-spear-power-attack','sprint32-sprint40-native-disarm-owned-gear',
            'sprint32-sprint40-bite-has-no-slow','sprint32-sprint40-native-haste-disarmed-fallback');
        'slow-real-time-and-turn-based'=@('sprint32-sprint40-slow-zero-final-damage-cold-immune-live-dc',
            'sprint32-sprint40-slow-real-miss-no-attempt','sprint32-sprint40-slow-native-haste-counter',
            'sprint32-sprint40-slow-native-descriptor-immunity','sprint32-sprint40-slow-independent-native-owners',
            'sprint32-sprint40-slow-native-finite-expiry','sprint32-sprint40-slow-turn-native-single-action');
        'cold-wall-and-ai'=@('sprint32-sprint40-cold-native-cast-ConeOfCold','sprint32-sprint40-cold-native-cast-IceStorm',
            'sprint32-sprint40-wall-cast-and-obstruction','sprint32-sprint40-wall-cross-species-live-claims',
            'sprint32-sprint40-wall-cross-species-cleanup-isolated','sprint32-sprint40-wall-all-original-bits-restored',
            'sprint32-sprint40-ai-ConeOfCold-rtwp-native-brain-cast','sprint32-sprint40-ai-IceStorm-turn-native-brain-cast',
            'sprint32-sprint40-ai-WallOfIce-rtwp-source-removal');
        'fear-regeneration-and-lifetime'=@('sprint32-sprint40-regeneration-exact-consumer',
            'sprint32-sprint40-regeneration-qualifier-good','sprint32-sprint40-fear-native-exposure',
            'sprint32-sprint40-fear-live-dc-and-independent-source','sprint32-sprint40-native-expiry-gear-and-resources',
            'sprint32-sprint40-native-death-gear-and-resources');
        'presentation-public-route'=@('sprint32-sprint40-body-ice-devil-0','sprint32-sprint40-private-pair-ice-devil',
            'sprint32-sprint40-body-cleanup-ice-devil','sprint32-sprint40-public-route-scope','sprint32-sprint40-public-icon-ice-devil');
        'affected-nalfeshnee-range'=@('sprint32-sprint40-nalfeshnee-slow-native-range',
            'sprint32-sprint40-nalfeshnee-slow-inside-cast','sprint32-sprint40-nalfeshnee-slow-outside-needs-approach',
            'sprint32-sprint40-nalfeshnee-slow-outside-native-approach')
    }
    foreach($id in $rows.Keys) {
        $row=@($Ledger.coverage|Where-Object id -ceq $id)[0]
        foreach($name in $rows[$id]) {
            if(@($row.evidenceSources|Where-Object {$_.requiredAssertions -ccontains $name}).Count -ne 1) {
                throw 'Sprint40 assertion absent from '+$id+': '+$name
            }
        }
    }
    $public=@($Ledger.coverage|Where-Object id -ceq 'presentation-public-route')[0]
    if(@($public.routes).Count -ne 1 -or @($surface.routes).Count -ne 1) { throw 'Sprint40 public route missing.' }
    $route=$surface.routes[0];$match=$public.routes[0]
    if($match.creature -cne $route.creature -or $match.tier -ne $route.tier -or $match.identity -cne $route.identity -or
       (@($match.observed|Sort-Object -CaseSensitive) -join ',') -cne 'cleanup,duration,eligibility,quantity,slot' -or
       @($public.evidenceSources|Where-Object {$_.runId -ceq $match.runId -and $_.requiredAssertions -ccontains $route.assertion}).Count -ne 1) {
        throw 'Sprint40 public route lacks its exact observed consumers.'
    }
    $processes=@();$runs=@()
    foreach($sequenceName in @('on','off')) {
        $row=@($Ledger.coverage|Where-Object id -ceq ('mixed-'+$sequenceName+'-persistence'))[0]
        $sequence=Read-KmgSprints3031Proof $row.sequenceProof
        if($sequence.source -cne $Candidate.commit -or $sequence.sequence -cne $sequenceName -or $sequence.status -cne 'PASS' -or
           -not $sequence.restoration.exactIntake -or -not $sequence.restoration.gameExited -or
           (@($sequence.stages.stage) -join ',') -cne 'prepare,cleanup,absent' -or @($row.evidenceSources).Count -ne 3 -or
           @($row.proofs|Where-Object {$_.path -ceq $row.sequenceProof.path -and $_.sha256 -ceq $row.sequenceProof.sha256}).Count -ne 1) {
            throw 'Sprint40 mixed persistence envelope incomplete.'
        }
        foreach($stage in $sequence.stages) {
            $sources=@($row.evidenceSources|Where-Object runId -ceq $stage.runId)
            if($sources.Count -ne 1 -or $stage.rawSha256 -cne $sources[0].nativeProof.sha256 -or
               [string]::IsNullOrWhiteSpace($stage.processIdentity)) { throw 'Sprint40 persistence stage attribution absent.' }
            $runs+=$stage.runId;$processes+=$stage.processIdentity
        }
        foreach($name in @('whole-roster-persistence-s40-items-owned','whole-roster-persistence-s40-state-prepared',
            'whole-roster-persistence-s40-state-cold','whole-roster-persistence-s40-source-cleanup',
            'whole-roster-persistence-s40-state-cleanup-absence','whole-roster-persistence-s40-state-fresh-absence')) {
            if(@($row.evidenceSources|Where-Object {$_.requiredAssertions -ccontains $name}).Count -eq 0) {
                throw 'Sprint40 persistence envelope does not name its own saved state: '+$name
            }
        }
    }
    if(@($runs|Sort-Object -Unique).Count -ne 6 -or @($processes|Sort-Object -Unique).Count -ne 6) {
        throw 'Sprint40 mixed ON/OFF requires six fresh processes.'
    }
    $repro=Read-KmgSprints3031Proof $Ledger.candidate.reproducibilityProof
    if($repro.source -cne $Candidate.commit -or $repro.primaryZipSha256 -cne $Candidate.packageSha256 -or
       $repro.rebuiltZipSha256 -cne $Candidate.packageSha256 -or $repro.primaryDllSha256 -cne $Candidate.dllSha256 -or
       $repro.rebuiltDllSha256 -cne $Candidate.dllSha256 -or $repro.exitCode -ne 0) {
        throw 'Exact Sprint40 reproducibility is incomplete.'
    }
    if($Ledger.publication.tag -cne ('v'+$Candidate.version) -or $Ledger.publication.draft -or
       $Ledger.publication.prerelease -or -not $Ledger.publication.latest) { throw 'Stable Sprint40 publication identity differs.' }
}

function Assert-KmgSprint40PremergeAuthority($Ledger,$Candidate,$PullRequest,$RemoteProductHead,$RemoteMaster,$ExpectedBaseCommit,$ProductPrNumber) {
    Assert-KmgSprint40Qualification $Ledger $Candidate
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or $RemoteMaster -cne $ExpectedBaseCommit -or
       $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or $PullRequest.baseRefName -cne 'master' -or
       $PullRequest.state -cne 'OPEN' -or -not $PullRequest.isDraft -or $PullRequest.mergeable -cne 'MERGEABLE' -or
       $RemoteProductHead -cne $Candidate.commit -or $PullRequest.headRefOid -cne $Candidate.commit -or
       [string]$ProductPrNumber -cne '97' -or $Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or
       $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
       [string]$Ledger.publication.productPr -cne [string]$ProductPrNumber) {
        throw 'Sprint40 product merge authority is stale or incomplete.'
    }
}
