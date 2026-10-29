Set-StrictMode -Version Latest

function Get-KmgSprints3031Surface {
    $repositoryRoot=Split-Path -Parent $PSScriptRoot
    $program=@'
import json,sys
from pathlib import Path
sys.path.insert(0,str(Path(sys.argv[1])/'tools'))
import expanded_summoning_manifest as m
c=m.parsed_creatures(); hidden=m.suppressed_keys()
scope={'lemure':2,'dretch':3,'hound-archon':4,'lantern-archon':3,'astral-deva':9,'trumpet-archon':9}
selected=[x for x in c if x['key'] in scope]
exact=len(selected)==6 and all(x['monster']==scope[x['key']] and x['ally'] is None and not x['templated'] for x in selected)
monster=sum(10-x['monster'] for x in c if x['monster'] is not None)
ally=sum(10-x['ally'] for x in c if x['ally'] is not None)
suppressed=sum((10-x['monster'] if x['monster'] is not None else 0)+(10-x['ally'] if x['ally'] is not None else 0) for x in c if x['key'] in hidden)
registry={x['symbol']:x['guid'] for x in json.loads((Path(sys.argv[1])/'blueprints/blueprints.json').read_text(encoding='utf-8-sig'))['entries']}
routes=[]
for k,v in scope.items():
 for t in range(v,10):
  quantity='One' if t==v else 'OneD3' if t==v+1 else 'OneD4PlusOne'
  symbol=f'KMG.Summoning.Ability.SM.Tier{t}.{m.token(k)}.{quantity}'
  routes.append(dict(creature=k,tier=t,identity=registry[symbol],assertion='sprints30-31-player-route-Monster.'+str(t)+'.'+k+'.'+quantity))
print(json.dumps(dict(scopeExact=exact,creatures=sorted(scope),routes=routes,registeredRoots=monster+ally,generatedRoots=monster+ally-suppressed,nativeWrappers=len(m.NATIVE_EXPANDED_OPTIONS),suppressed=suppressed,choices=monster+ally-suppressed+len(m.NATIVE_EXPANDED_OPTIONS))))
'@
    $out=@(& python -B -c $program $repositoryRoot)
    if($LASTEXITCODE -ne 0) { throw 'Cannot derive S30-31 catalog.' }
    return (($out -join '') | ConvertFrom-Json)
}

function Read-KmgSprints3031Proof {
    param([Parameter(Mandatory=$true)]$Proof)
    if($Proof.sha256 -cnotmatch '^[0-9a-f]{64}$' -or -not (Test-Path -LiteralPath $Proof.path -PathType Leaf) -or
        (Get-FileHash -LiteralPath $Proof.path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $Proof.sha256) { throw 'S30-31 proof is absent or changed.' }
    return (Get-Content -LiteralPath $Proof.path -Raw | ConvertFrom-Json)
}
function Get-KmgSprints3031Blob([string]$Commit,[string]$Path) {
    if($Commit -cnotmatch '^[0-9a-f]{40}$' -or $Path -match '(^/|\.\.|:|\\)' -or
        $Path -cnotmatch '^(src|assets|blueprints|validation|scripts|tools)/') { throw 'Invalid dependency identity.' }
    $out=@(& git -C (Split-Path -Parent $PSScriptRoot) rev-parse ($Commit+':'+$Path) 2>$null)
    if($LASTEXITCODE -ne 0 -or $out.Count -ne 1 -or $out[0] -cnotmatch '^[0-9a-f]{40}$') { throw 'Dependency blob missing.' }
    return [string]$out[0]
}

function Assert-KmgSprints3031ReusedMechanics {
    param([Parameter(Mandatory=$true)]$Row,[Parameter(Mandatory=$true)]$Candidate)
    if($Row.result -cne 'REUSED_PASS' -or $Row.kind -cne 'mechanics' -or
        $Row.acceptance -cne 'OWNER_S3031_SCOPED_DEVELOPMENT_MECHANICS_2026_10_24' -or
        [string]::IsNullOrWhiteSpace([string]$Row.reuseJustification) -or @($Row.evidenceSources).Count -eq 0) { throw 'Unsupported S30-31 reuse.' }
    foreach($source in $Row.evidenceSources) {
        # Original development-marked receipts remain mandatory, not bypassed.
        $receipt=Read-KmgSprints3031Proof $source.receiptProof
        $profile=Read-KmgSprints3031Proof $source.profileProof
        $native=Read-KmgSprints3031Proof $source.nativeProof
        if($receipt.candidateKind -cne 'development' -or $receipt.releaseQualification -ne $false -or
            $profile.candidateKind -cne 'development' -or $profile.releaseQualification -ne $false -or
            $receipt.repositoryRoot -ine 'C:\Dev\KingmakerGunslingerLab\wt\s30-31' -or
            $receipt.branch -cne 'codex/expanded-summoning-sprints30-31-outsider-celestial-pilot') { throw 'Original development attribution lost.' }
        foreach($pair in @(@('source','commit'),@('sourceFingerprint','sourceStateSha256'),@('packageSha256','packageSha256'),@('dllSha256','dllSha256'),@('dllMvid','dllMvid'))) {
            if([string]$source.($pair[0]) -cne [string]$receipt.($pair[1])) { throw 'Reused tuple differs from original receipt.' }
        }
        if($native.gitCommit -cne $source.source -or $native.runId -cne $source.runId -or $native.status -cne 'PASS' -or
            $source.originalVerdict -cne 'PASS' -or @($native.assertions | Where-Object {$_.status -cne 'PASS'}).Count -gt 0 -or
            $profile.profileId -cne $source.profile -or $profile.packageSha256 -cne $source.packageSha256 -or
            -not $profile.restorationVerified -or $native.gameVersion -cne $source.gameVersion) { throw 'Reused native/profile/restoration proof incomplete.' }
        $run=@($profile.runtimeResults | Where-Object {$_.runId -ceq $source.runId})
        if($run.Count -ne 1 -or $run[0].status -cne 'PASS' -or $run[0].runtimeResultSha256.ToLowerInvariant() -cne $source.nativeProof.sha256) { throw 'Profile does not bind the raw run.' }
        foreach($proof in @($source.receiptProof,$source.profileProof,$source.nativeProof)) {
            if(@($Row.proofs | Where-Object {$_.path -ceq $proof.path -and $_.sha256 -ceq $proof.sha256}).Count -ne 1) { throw 'Original marked proof omitted.' }
        }
        if(@($source.requiredAssertions).Count -eq 0 -or [string]::IsNullOrWhiteSpace([string]$source.observedOutcome) -or
            [string]::IsNullOrWhiteSpace([string]$source.observerMeaning) -or [string]::IsNullOrWhiteSpace([string]$source.environmentComparison)) { throw 'Reuse lacks observed outcome/meaning/environment.' }
        foreach($name in $source.requiredAssertions) {
            if(@($native.assertions | Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1) { throw 'Required native observation missing.' }
        }
        $oldRefs=@($receipt.references.files | ForEach-Object {$_.relativePath+':'+$_.sha256.ToLowerInvariant()} | Sort-Object -CaseSensitive)
        $newRefs=@($Candidate.references.files | ForEach-Object {$_.relativePath+':'+$_.sha256.ToLowerInvariant()} | Sort-Object -CaseSensitive)
        if($oldRefs.Count -eq 0 -or @($oldRefs|Where-Object {$newRefs -cnotcontains $_}).Count -gt 0) { throw 'Relevant installed references changed.' }
        $addedRefs=@($newRefs|Where-Object {$oldRefs -cnotcontains $_})
        if($addedRefs.Count -gt 0) {
            # The integrated cleanup code explicitly references two assemblies
            # already supplied by the same installed2.1.7b runtime. Never permit
            # replacement/loss of any original reference or arbitrary additions.
            $allowed=@('UniRx.dll:bfee7eca22156d8b95c149ba1383f065124b7b44a62ed4eafc32a5e67f6bc68a',
                'UnityEngine.ClothModule.dll:390e7edea7c242f764c4645c8582cf02bfdb3d2d86f51d4ea8d8b91ac5e57e91')|Sort-Object -CaseSensitive
            $review=Read-KmgSprints3031Proof $source.dependencyReviewProof
            if(($addedRefs -join '|') -cne ($allowed -join '|') -or
                $review.mission -cne 'Sprints3031OutsiderCelestialPilot' -or
                $review.integratedMaster -cne '60175837909f75e7507200bc1e22a6add04ddd51' -or
                (@($review.referenceAdditions.identities|Sort-Object -CaseSensitive) -join '|') -cne ($allowed -join '|') -or
                [string]::IsNullOrWhiteSpace([string]$review.referenceAdditions.unchangedConsumerReason)) { throw 'Unreviewed explicit reference additions.' }
        }
        foreach($category in @('production','configuration','asset','shared-helper','observer')) {
            if(@($source.dependencies | Where-Object {$_.category -ceq $category}).Count -eq 0) { throw "Missing dependency category: $category" }
        }
        foreach($dep in $source.dependencies) {
            $old=Get-KmgSprints3031Blob $source.source $dep.path
            $new=Get-KmgSprints3031Blob $Candidate.commit $dep.path
            if($old -cne $dep.originalBlob -or $new -cne $dep.candidateBlob) { throw 'Recorded dependency identity differs from actual source.' }
            if($old -ceq $new) {
                if($dep.comparison -cne 'IDENTICAL_RELEVANT_BYTES') { throw 'Incorrect identical dependency disposition.' }
            } else {
                if($dep.comparison -cne 'REVIEWED_UNCHANGED_OBSERVED_PATH') { throw ('Changed relevant dependency needs fresh observation: '+$dep.path) }
                $review=Read-KmgSprints3031Proof $source.dependencyReviewProof
                if($review.mission -cne 'Sprints3031OutsiderCelestialPilot' -or $review.integratedMaster -cne '60175837909f75e7507200bc1e22a6add04ddd51') { throw 'Named post-integration review is missing.' }
                $decision=@($review.decisions|Where-Object {$_.path -ceq $dep.path -and $_.originalBlob -ceq $old -and $_.candidateBlob -ceq $new -and $_.outcome -ceq $source.observedOutcome})
                if($decision.Count -ne 1 -or $decision[0].disposition -cne 'UNCHANGED_OBSERVED_PATH' -or
                    [string]::IsNullOrWhiteSpace([string]$decision[0].changedCode) -or [string]::IsNullOrWhiteSpace([string]$decision[0].unchangedConsumerReason)) { throw 'Changed dependency has no exact scoped review; fresh observation required.' }
            }
        }
    }
}

function Get-KmgSprints3031MechanicsRequirements {
    # Fixed owner charter/triage outcomes, not a mode-by-profile product.
    return @{
        'lemure'=@('profile-defenses','natural-attacks','native-ai','turn-cancellation','ownership-lifecycle')
        'dretch'=@('profile-defenses','natural-attacks','fear-cloud','safe-ai','held-escape','turn-cancellation','ownership-lifecycle')
        'hound-archon'=@('profile-defenses','armed-natural-attacks','aid','menace','circle','disarm','native-ai','turn-cancellation','ownership-lifecycle')
        'lantern-archon'=@('profile-defenses','light-rays','orb','aid','menace','ai-turn-cancellation','ownership-lifecycle')
        'astral-deva'=@('profile-defenses','hammer-slam','ordinary-cleave','protective-aura','heal','blade-barrier','limited-support','at-will-support','holy-smite','dispel-evil','ai-turn-cancellation','disarm','ownership-lifecycle')
        'trumpet-archon'=@('profile-defenses','prepared-book','cantrips','prepared-effects','invisibility-purge','aid-banishment-ai','trumpet-blast','instrument','named-skills','blind-fight','sword-attacks','ordinary-cleave','held-escape','circle-menace','turn-cancellation','dispel-evil','ownership-lifecycle')
    }
}

function Assert-KmgSprints3031FreshExecution {
    param($Source,$Candidate,$Row)
    $execution=Read-KmgSprints3031Proof $Source.executionProof
    if(@($Row.proofs|Where-Object {$_.path -ceq $Source.executionProof.path -and $_.sha256 -ceq $Source.executionProof.sha256}).Count -ne 1) { throw 'Fresh execution attribution omitted.' }
    if($Source.executionKind -ceq 'profile') {
        if($execution.candidateKind -cne 'release' -or -not $execution.restorationVerified -or $execution.profileId -cne $Source.profile) { throw 'Fresh profile is not restored final-artifact execution.' }
        $tuple=$execution
        $run=@($execution.runtimeResults|Where-Object {$_.runId -ceq $Source.runId})
        if($run.Count -ne 1 -or $run[0].status -cne 'PASS' -or $run[0].runtimeResultSha256.ToLowerInvariant() -cne $Source.nativeProof.sha256) { throw 'Fresh profile does not bind the raw result.' }
    } elseif($Source.executionKind -ceq 'installed-stack') {
        if($execution.candidateKind -cne 'release-candidate' -or -not $execution.releaseQualification -or
            -not $execution.restoration.verified -or -not $execution.restoration.gameSettingsExact -or
            $execution.restoration.transactionStatus -cne 'Restored' -or ($execution.PSObject.Properties['error'] -and $execution.error) -or
            $execution.launchReceipt.runId -cne $Source.runId -or
            [IO.Path]::GetFullPath((Join-Path $execution.launchReceipt.evidenceDirectory 'runtime-result.json')) -ine [IO.Path]::GetFullPath($Source.nativeProof.path)) { throw 'Fresh installed-stack attribution/restoration differs.' }
        $tuple=$execution.candidate
    } else { throw 'Unsupported fresh execution receipt.' }
    foreach($key in @('commit','packageSha256','dllSha256','dllMvid')) {
        if([string]$tuple.$key -cne [string]$Candidate.$key) { throw 'Actual fresh execution used a different artifact.' }
    }
}

function Assert-KmgSprints3031Qualification {
    param([Parameter(Mandatory=$true)]$Ledger,[Parameter(Mandatory=$true)]$Candidate,[Parameter(Mandatory=$true)][string]$Notes)
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprints3031OutsiderCelestialPilot' -or
        $Ledger.status -cne 'QUALIFIED' -or $Ledger.releaseEligible -ne $true -or $Ledger.sprint32Started -ne $false -or
        @($Ledger.baselineExceptions).Count -ne 0 -or @($Ledger.unresolvedFindings).Count -ne 0 -or
        $Ledger.expandedScope -ne $false -or $Ledger.gameplayWaiver -ne $false) { throw 'S30-31 not completely qualified.' }
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),@('dllSha256','dllSha256'),@('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))) {
        if([string]$Ledger.candidate.($pair[0]) -cne [string]$Candidate.($pair[1])) { throw 'Final artifact identity differs.' }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or $suite.failures -ne 0 -or $suite.executed -le 0 -or
        $suite.executed -ne $suite.registered -or $suite.unfiltered -ne $true) { throw 'Complete final gate missing.' }
    $surface=Get-KmgSprints3031Surface
    if(-not $surface.scopeExact -or $surface.suppressed -ne 0 -or $Ledger.roster.protectedIdentitySetsEqual -ne $true -or
        $Ledger.roster.lanternOrderingAndReferencesPreserved -ne $true) { throw 'Final public scope/identity differs.' }
    foreach($key in @('registeredRoots','generatedRoots','nativeWrappers','choices')) {
        if($Ledger.roster.$key -ne $surface.$key) { throw 'Source-derived roster differs.' }
    }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or $Ledger.restoration.workingFixtureClean -ne $true) { throw 'Final restoration incomplete.' }
    foreach($pair in @(@('houndAttackRoles','GENERAL_PF1'),@('trumpetBlastDc','GENERAL_PF1'),@('offensiveBanishment','RETAIN_CASTER_TRAVEL_EXCLUDED'),@('magicCircle','CANONICAL_PUBLISHED_ADAPTATION'),@('visualStandard','FUNCTIONAL_FIRST'))) {
        if($Ledger.scopeDecisions.($pair[0]) -cne $pair[1]) { throw 'Closed owner decision changed.' }
    }
    foreach($phrase in @('Magic Circle','printed','change shape','gestalt','visual')) {
        if($Notes -notmatch [regex]::Escape($phrase)) { throw 'Release disclosure omits scope.' }
    }
    if($Ledger.ownerVisualReview -cnotin @('NOT_PERFORMED_NONBLOCKING','APPROVED') -or
        ($Ledger.ownerVisualReview -ceq 'APPROVED' -and [string]::IsNullOrWhiteSpace([string]$Ledger.ownerVisualReviewEvidence))) { throw 'Visual approval provenance missing.' }
    if($Ledger.publication.tag -cne ('v'+$Candidate.version) -or $Ledger.publication.prerelease -or $Ledger.publication.draft -or -not $Ledger.publication.latest) { throw 'Stable release identity invalid.' }
    $fresh=@('public-thirty-routes','public-inventory-icons-bodies','mixed-persistence-six-fresh-processes','standalone-and-high-risk-integration','complete-final-source-build-package-gate','exact-package-release-qualification')
    $ids=@($Ledger.coverage | ForEach-Object {$_.id})
    if(@($ids|Sort-Object -Unique).Count -ne $ids.Count) { throw 'Duplicate coverage ID.' }
    foreach($id in $fresh) {
        $row=@($Ledger.coverage|Where-Object {$_.id -ceq $id})
        if($row.Count -ne 1 -or $row[0].result -cne 'PASS' -or $row[0].kind -cne 'final') { throw "Mandatory fresh row absent: $id" }
    }
    $requiredMechanics=Get-KmgSprints3031MechanicsRequirements; $claims=@()
    foreach($row in $Ledger.coverage) {
        if($row.result -cnotin @('PASS','REUSED_PASS') -or @($row.proofs).Count -eq 0 -or @($row.evidenceSources).Count -eq 0) { throw 'Missing/failed mandatory coverage.' }
        if($row.result -ceq 'REUSED_PASS') { Assert-KmgSprints3031ReusedMechanics $row $Candidate }
        elseif($row.kind -cnotin @('mechanics','final')) { throw 'Unsupported fresh coverage kind.' }
        foreach($source in $row.evidenceSources) {
            if($source.source -cnotmatch '^[0-9a-f]{40}$' -or $source.packageSha256 -cnotmatch '^[0-9a-f]{64}$' -or $source.dllSha256 -cnotmatch '^[0-9a-f]{64}$' -or $source.dllMvid -cnotmatch '^[0-9a-f-]{36}$') { throw 'Malformed execution identity.' }
            if($row.result -ceq 'PASS' -and ($source.source -cne $Candidate.commit -or $source.packageSha256 -cne $Candidate.packageSha256 -or $source.dllSha256 -cne $Candidate.dllSha256 -or $source.dllMvid -cne $Candidate.dllMvid -or $source.candidateKind -cne 'release')) { throw 'False fresh artifact attribution.' }
            if($row.result -ceq 'PASS' -and $row.id -cnotin @('complete-final-source-build-package-gate','exact-package-release-qualification')) {
                Assert-KmgSprints3031FreshExecution $source $Candidate $row
                $native=Read-KmgSprints3031Proof $source.nativeProof
                if($native.gitCommit -cne $Candidate.commit -or $native.runId -cne $source.runId -or $native.status -cne 'PASS' -or
                    @($native.assertions|Where-Object {$_.status -cne 'PASS'}).Count -gt 0 -or @($source.requiredAssertions).Count -eq 0) { throw 'Fresh native case missing or failed.' }
                foreach($name in $source.requiredAssertions) {
                    if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1) { throw 'Fresh native required observation absent.' }
                }
                if(@($row.proofs|Where-Object {$_.path -ceq $source.nativeProof.path -and $_.sha256 -ceq $source.nativeProof.sha256}).Count -ne 1) { throw 'Fresh native raw proof omitted.' }
            }
        }
        if($row.kind -ceq 'mechanics') { foreach($claim in $row.claims) {
            if($claim.creature -cnotin $surface.creatures -or $claim.requirement -cnotin $requiredMechanics[$claim.creature]) { throw 'Unsupported mechanics claim.' }
            $bound=@($row.evidenceSources|Where-Object {$_.runId -ceq $claim.runId})
            if($bound.Count -ne 1 -or @($claim.assertions).Count -eq 0) { throw 'Mechanics claim has no exact observed run.' }
            foreach($assertion in $claim.assertions) {
                if($bound[0].requiredAssertions -cnotcontains $assertion) { throw 'Mechanics claim has no corresponding observation.' }
            }
            $claims+=($claim.creature+':'+$claim.requirement)
        }}
    }
    foreach($creature in $surface.creatures) { foreach($group in $requiredMechanics[$creature]) {
        if($claims -cnotcontains ($creature+':'+$group)) { throw "Missing creature requirement: $creature/$group" }
    }}
    $routes=@(@($Ledger.coverage|Where-Object {$_.id -ceq 'public-thirty-routes'})[0].routes)
    if($routes.Count -ne $surface.routes.Count) { throw 'Missing affected public routes.' }
    foreach($route in $surface.routes) {
        $match=@($routes|Where-Object {$_.creature -ceq $route.creature -and $_.tier -eq $route.tier})
        if($match.Count -ne 1 -or $match[0].identity -cne $route.identity -or
            (@($match[0].observed | Sort-Object -CaseSensitive) -join ',') -cne 'cleanup,duration,eligibility,quantity,slot') { throw 'Incomplete route identity or consumers.' }
        $routeRow=@($Ledger.coverage|Where-Object {$_.id -ceq 'public-thirty-routes'})[0]
        $routeSources=@($routeRow.evidenceSources|Where-Object {$_.runId -ceq $match[0].runId -and $_.requiredAssertions -ccontains $route.assertion})
        if($routeSources.Count -ne 1) { throw 'Public route is not bound to its exact native observation.' }
    }
    $presentation=@($Ledger.coverage|Where-Object {$_.id -ceq 'public-inventory-icons-bodies'})[0]
    $presented=@($presentation.evidenceSources|ForEach-Object {$_.requiredAssertions})
    foreach($name in @('sprints30-31-public-inventory-integration')+@($surface.creatures|ForEach-Object {'sprints30-31-public-body-icon-'+$_;'sprints30-31-public-body-reset-'+$_})) {
        if($presented -cnotcontains $name) { throw 'Final inventory/body/icon observation missing.' }
    }
    $stages=@(@($Ledger.coverage|Where-Object {$_.id -ceq 'mixed-persistence-six-fresh-processes'})[0].stages)
    if($stages.Count -ne 6 -or @($stages.runId|Sort-Object -Unique).Count -ne 6 -or @($stages.processIdentity|Sort-Object -Unique).Count -ne 6) { throw 'Persistence requires six fresh processes.' }
    foreach($id in @('on-prepare','on-cleanup-save','on-absence','off-prepare','off-cleanup-save','off-on-absence')) {
        $stage=@($stages|Where-Object {$_.id -ceq $id})
        if($stage.Count -ne 1 -or $stage[0].result -cne 'PASS' -or $stage[0].freshProcess -ne $true -or
            $stage[0].originalWorkingRestored -ne $true -or (@($stage[0].creatures|Sort-Object -CaseSensitive) -join ',') -cne ($surface.creatures -join ',')) { throw 'Mixed persistence stage incomplete.' }
        $persistenceRow=@($Ledger.coverage|Where-Object {$_.id -ceq 'mixed-persistence-six-fresh-processes'})[0]
        if(@($persistenceRow.evidenceSources|Where-Object {$_.runId -ceq $stage[0].runId}).Count -ne 1) { throw 'Fresh persistence stage has no matching native proof.' }
        $source=@($persistenceRow.evidenceSources|Where-Object {$_.runId -ceq $stage[0].runId})[0]
        if($source.executionKind -cne 'installed-stack') { throw 'Persistence stage requires the actual guarded installed-stack receipt.' }
        $execution=Read-KmgSprints3031Proof $source.executionProof
        $raw=Read-KmgSprints3031Proof $source.nativeProof
        $process=Read-KmgSprints3031Proof $stage[0].processProof
        if(@($persistenceRow.proofs|Where-Object {$_.path -ceq $stage[0].processProof.path -and $_.sha256 -ceq $stage[0].processProof.sha256}).Count -ne 1 -or
            $process.runId -cne $source.runId -or -not $process.launchBegan -or -not $process.guardedRequestAccepted -or
            @($process.preLaunchKingmakerProcesses).Count -ne 0 -or $process.steamAppId -ne 640820 -or
            $process.kingmakerProcessId -le 0 -or [string]::IsNullOrWhiteSpace([string]$process.kingmakerStartedAtUtc) -or
            $stage[0].processIdentity -cne ([string]$process.kingmakerProcessId+'@'+$process.kingmakerStartedAtUtc)) { throw 'Fresh process identity lacks its original launch proof.' }
        $scenario=if($id.EndsWith('prepare')){'working-save-expanded-summoning-prepare'}elseif($id.EndsWith('cleanup-save')){'working-save-expanded-summoning-verify-cleanup'}else{'working-save-expanded-summoning-verify-absent'}
        if($raw.scenario -cne $scenario -or $execution.scenario -cne $scenario -or $execution.persistenceScope -cne 'outsider-celestial' -or
            [bool]$execution.moduleDisabled -ne ($id -ceq 'off-cleanup-save')) { throw 'Persistence stage/module/scope differs from execution.' }
        $required=if($id.EndsWith('absence')){@('whole-roster-persistence-s3031-fresh-absence')}else{
            @($surface.creatures|ForEach-Object {'whole-roster-persistence-s3031-items-resources-effects-'+$_;'whole-roster-persistence-s3031-aura-source-'+$_})+
            @($(if($id.EndsWith('prepare')){'whole-roster-persistence-s3031-prepare-aura-consumers';'whole-roster-persistence-s3031-prepared-slot-setup'}else{'whole-roster-persistence-s3031-cold-aura-consumers';'whole-roster-persistence-s3031-cleanup-owned-state-cleanup';'whole-roster-persistence-s3031-cleanup-absence'}))}
        foreach($name in $required) { if($source.requiredAssertions -cnotcontains $name) { throw 'Required native persistence state was not observed.' } }

    }
    $integration=@($Ledger.coverage|Where-Object {$_.id -ceq 'standalone-and-high-risk-integration'})[0]
    if((@($integration.profiles|Sort-Object -CaseSensitive) -join ',') -cne 'gunslinger-high-risk-combined,gunslinger-only' -or
        $integration.affectedOldControlsPassed -ne $true -or $integration.sharedSourcesAndLifecyclePassed -ne $true) { throw 'Final integration controls missing.' }
    foreach($profile in @('gunslinger-only','gunslinger-high-risk-combined')) {
        $runs=@($integration.evidenceSources|Where-Object {$_.profile -ceq $profile -and $_.executionKind -ceq 'profile'})
        if($runs.Count -ne 1) { throw 'Required integration profile lacks its actual execution receipt.' }
        foreach($case in @('shared-circles','source-lifecycle','celestial-cleave')) {
            if($runs[0].requiredAssertions -cnotcontains ('sprints30-31-final-integration-reset-'+$case)) { throw 'Fixed integration case/reset observation missing.' }
        }
    }
}

function Assert-KmgSprints3031PremergeAuthority {
    param($Ledger,$Candidate,[string]$Notes,$PullRequest,[string]$RemoteProductHead,[string]$RemoteMaster,[string]$ExpectedBaseCommit)
    Assert-KmgSprints3031Qualification $Ledger $Candidate $Notes
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or $RemoteMaster -cne $ExpectedBaseCommit -or
        $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or $PullRequest.baseRefName -cne 'master' -or $PullRequest.state -cne 'OPEN' -or
        $PullRequest.isDraft -ne $true -or $PullRequest.mergeable -cne 'MERGEABLE' -or $RemoteProductHead -cne $Candidate.commit -or
        $PullRequest.headRefOid -cne $Candidate.commit -or $Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or
        $Ledger.publication.productPr -ne 66 -or $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
        $Ledger.publication.productMergeCommit -or $Ledger.publication.mergeContainsTestedSource -ne $false) { throw 'Exact draft PR66/base/candidate authority differs.' }
}
function Assert-KmgSprints3031ReleaseDecision {
    param($Ledger,$Candidate,[string]$Notes)
    Assert-KmgSprints3031Qualification $Ledger $Candidate $Notes
    if($Ledger.publication.productPr -ne 66 -or $Ledger.publication.productMergeCommit -cnotmatch '^[0-9a-f]{40}$' -or
        $Ledger.publication.mergeContainsTestedSource -ne $true) { throw 'Actual verified product merge missing.' }
}
