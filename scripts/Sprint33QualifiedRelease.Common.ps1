Set-StrictMode -Version Latest

# Sprint 33 uses the existing qualification ledger and exact-artifact publisher.
function Read-KmgSprint33Proof($Proof) {
    if([string]$Proof.sha256 -cnotmatch '^[0-9a-f]{64}$' -or
       -not(Test-Path -LiteralPath $Proof.path -PathType Leaf) -or
       (Get-FileHash -LiteralPath $Proof.path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $Proof.sha256) { throw 'Sprint33 proof missing or changed.' }
    Get-Content -LiteralPath $Proof.path -Raw | ConvertFrom-Json
}
# The one OFF-only observer correction does not invalidate the completed ON envelope.
# Verify the entire product delta, all three original executions and exact restoration.
function Assert-KmgSprint33OnReuse($Row,$Candidate) {
    if($Row.id -cne 'mixed-on-persistence' -or $Row.result -cne 'REUSED_PASS' -or
       [string]::IsNullOrWhiteSpace($Row.reuseJustification)){throw 'Unselected persistence reuse.'}
    $old=Read-KmgSprint33Proof $Row.originalCandidateProof
    $sequence=Read-KmgSprint33Proof $Row.sequenceProof
    if($old.commit -cne '3c03687566b184db07cd778a55c8d78e9d9a1290' -or $old.version -cne $Candidate.version -or
       $sequence.source -cne $old.commit -or $sequence.sequence -cne 'on' -or $sequence.status -cne 'PASS' -or
       -not $sequence.restoration.exactIntake -or -not $sequence.restoration.gameExited -or
       (@($sequence.stages.stage) -join ',') -cne 'prepare,cleanup,absent' -or
       @($sequence.stages.runId|Sort-Object -Unique).Count -ne 3 -or @($Row.evidenceSources).Count -ne 3){throw 'Completed original ON envelope required.'}
    $path='src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs'
    $changed=@(& git diff --name-only $old.commit $Candidate.commit -- src/KingmakerGunslinger assets assets-source blueprints)
    if($LASTEXITCODE -or $changed.Count -ne 1 -or $changed[0] -cne $path){throw 'ON dependencies changed beyond the reviewed OFF expectation.'}
    $before=(& git show ($old.commit+':'+$path)) -join "`n"
    if($LASTEXITCODE){throw 'Original observer unavailable.'}
    $after=(& git show ($Candidate.commit+':'+$path)) -join "`n"
    $needle='Sprint28PersistenceScope || Sprint29PersistenceScope || Sprints3031PersistenceScope || Sprint32PersistenceScope) && verify;'
    if($LASTEXITCODE -or $before -cnotmatch [regex]::Escape($needle) -or
       $after -cne $before.Replace($needle,$needle.Replace('Sprint32PersistenceScope)','Sprint32PersistenceScope || Sprint33PersistenceScope)'))){throw 'ON observer path changed.'}
    if((@($old.references.files|ForEach-Object {$_.relativePath+':'+$_.sha256}|Sort-Object) -join '|') -cne
       (@($Candidate.references.files|ForEach-Object {$_.relativePath+':'+$_.sha256}|Sort-Object) -join '|')){throw 'ON installed references changed.'}
    foreach($proof in @($Row.originalCandidateProof,$Row.sequenceProof)){
        if(@($Row.proofs|Where-Object {$_.path -ceq $proof.path -and $_.sha256 -ceq $proof.sha256}).Count -ne 1){throw 'ON provenance omitted.'}
    }
    foreach($stage in $sequence.stages){
        $sources=@($Row.evidenceSources|Where-Object runId -ceq $stage.runId)
        if($sources.Count -ne 1){throw 'ON stage attribution missing or duplicated.'}
        $source=$sources[0]
        foreach($key in @('packageSha256','dllSha256','dllMvid')){if($source.$key -cne $old.$key){throw 'ON original artifact differs.'}}
        if($source.source -cne $old.commit -or $source.executionKind -cne 'installed-stack'){throw 'ON source differs.'}
        Assert-KmgSprints3031FreshExecution $source $old $Row
        $native=Read-KmgSprint33Proof $source.nativeProof
        if($native.gitCommit -cne $old.commit -or $native.runId -cne $stage.runId -or $native.status -cne 'PASS' -or
           @($native.assertions|Where-Object status -cne 'PASS').Count -or $stage.rawSha256 -cne $source.nativeProof.sha256 -or
           @($stage.requiredAssertions).Count -eq 0){throw 'ON native evidence incomplete.'}
        foreach($name in $stage.requiredAssertions){if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1){throw 'ON required outcome absent.'}}
        if(@($Row.proofs|Where-Object {$_.path -ceq $source.nativeProof.path -and $_.sha256 -ceq $source.nativeProof.sha256}).Count -ne 1){throw 'ON raw proof omitted.'}
    }
}
function Assert-KmgSprint33Reuse($Row,$Candidate) {
    if($Row.id -ceq 'mixed-on-persistence'){Assert-KmgSprint33OnReuse $Row $Candidate;return}
    if($Row.result -cne 'REUSED_PASS' -or $Row.id -cnotin @('twelve-preserved-routes','dedicated-models-motion-ownership','natural-invisibility-purge','succubus-preservation-stalker-turn') -or
       [string]::IsNullOrWhiteSpace($Row.reuseJustification)) { throw 'Unselected Sprint33 evidence reuse.' }
    Assert-KmgScopedDevelopmentReuse $Row $Candidate 'C:\Dev\KingmakerGunslingerLab\wt\s33' 'codex/expanded-summoning-sprint33-shadow-outsiders' @('sprint33-shadow-outsiders')
}
function Assert-KmgSprint33Qualification($Ledger,$Candidate) {
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint33DedicatedShadowOutsiders' -or
       $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or $Ledger.sprint34Started -or
       @($Ledger.baselineExceptions).Count -or @($Ledger.unresolvedFindings).Count){throw 'Sprint33 qualification incomplete.'}
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),@('dllSha256','dllSha256'),@('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))){
        if($Ledger.candidate.($pair[0]) -cne $Candidate.($pair[1])){throw 'Final artifact differs.'}
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or $suite.failures -ne 0 -or
       $suite.executed -le 0 -or $suite.executed -ne $suite.registered -or -not $suite.unfiltered){throw 'Complete gate did not pass.'}
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or -not $Ledger.restoration.workingFixtureClean){throw 'Exact restoration incomplete.'}
    if($Ledger.ownerVisualReview -cne 'NOT_PERFORMED_NONBLOCKING'){throw 'No personal owner aesthetic approval has been supplied.'}
    $surface=Get-KmgSprint29SourceSurface
    foreach($key in @('projectCreatures','monsterCreatures','monsterPlacements','natureAllyCreatures','natureAllyPlacements','generatedRoots','nativeWrappers','choices')){
        if($Ledger.roster.$key -ne $surface.$key){throw 'Source-derived public surface differs.'}
    }
    if($Ledger.roster.preservedRoutes -ne 12 -or -not $Ledger.roster.protectedIdentitySetsEqual -or
       $Ledger.models.invisibleStalker -cne 'original-invisible-stalker' -or $Ledger.models.shadowDemon -cne 'original-shadow-demon' -or
       -not $Ledger.models.succubusPreserved){throw 'Selected identities/models are incomplete.'}
    $required=@('twelve-preserved-routes','dedicated-models-motion-ownership','natural-invisibility-purge','succubus-preservation-stalker-turn','mixed-on-persistence','mixed-off-persistence','highest-risk-integration','complete-source-build-package-gate','exact-reproducibility')
    $ids=@($Ledger.coverage|ForEach-Object id)
    if(@($ids|Sort-Object -Unique).Count -ne $ids.Count -or @($required|Where-Object {$ids -cnotcontains $_}).Count){throw 'Coverage incomplete or duplicated.'}
    foreach($row in $Ledger.coverage){
        if($row.result -cnotin @('PASS','REUSED_PASS') -or @($row.proofs).Count -eq 0 -or @($row.evidenceSources).Count -eq 0){throw 'Coverage proof absent.'}
        if($row.result -ceq 'REUSED_PASS'){Assert-KmgSprint33Reuse $row $Candidate}
        else {foreach($source in $row.evidenceSources){foreach($key in @('packageSha256','dllSha256','dllMvid')){if($source.$key -cne $Candidate.$key){throw 'Fresh proof is bound to other bytes.'}};if($source.source -cne $Candidate.commit){throw 'Fresh source differs.'}}}
    }
    foreach($row in $Ledger.coverage|Where-Object {$_.result -ceq 'PASS' -and $_.id -cnotin @('complete-source-build-package-gate','exact-reproducibility')}) {
        foreach($source in $row.evidenceSources) {
            # Existing artifact/profile/installed-stack correlation helper has no campaign selection or controller side effects.
            Assert-KmgSprints3031FreshExecution $source $Candidate $row
            $native=Read-KmgSprint33Proof $source.nativeProof
            if($native.gitCommit -cne $Candidate.commit -or $native.runId -cne $source.runId -or $native.status -cne 'PASS' -or
               @($native.assertions|Where-Object status -cne 'PASS').Count -or @($source.requiredAssertions).Count -eq 0){throw 'Fresh native proof incomplete.'}
            foreach($name in $source.requiredAssertions){if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1){throw 'Fresh required assertion absent.'}}
            if(@($row.proofs|Where-Object {$_.path -ceq $source.nativeProof.path -and $_.sha256 -ceq $source.nativeProof.sha256}).Count -ne 1){throw 'Fresh raw proof omitted.'}
        }
    }
    $repro=Read-KmgSprint33Proof $Ledger.candidate.reproducibilityProof
    if($repro.source -cne $Candidate.commit -or $repro.primaryZipSha256 -cne $Candidate.packageSha256 -or
       $repro.rebuiltZipSha256 -cne $Candidate.packageSha256 -or $repro.primaryDllSha256 -cne $Candidate.dllSha256 -or
       $repro.rebuiltDllSha256 -cne $Candidate.dllSha256 -or $repro.exitCode -ne 0){throw 'Exact reproducibility is incomplete.'}
    if($Ledger.publication.tag -cne ('v'+$Candidate.version) -or $Ledger.publication.draft -or $Ledger.publication.prerelease -or -not $Ledger.publication.latest){throw 'Stable publication identity differs.'}
}
function Assert-KmgSprint33PremergeAuthority($Ledger,$Candidate,$PullRequest,$RemoteProductHead,$RemoteMaster,$ExpectedBaseCommit,$ProductPrNumber) {
    Assert-KmgSprint33Qualification $Ledger $Candidate
    if($ExpectedBaseCommit -cnotmatch '^[0-9a-f]{40}$' -or $RemoteMaster -cne $ExpectedBaseCommit -or
       $PullRequest.baseRefOid -cne $ExpectedBaseCommit -or $PullRequest.baseRefName -cne 'master' -or
       $PullRequest.state -cne 'OPEN' -or -not $PullRequest.isDraft -or $PullRequest.mergeable -cne 'MERGEABLE' -or
       $RemoteProductHead -cne $Candidate.commit -or $PullRequest.headRefOid -cne $Candidate.commit -or
       $Ledger.publication.status -cne 'PENDING_PRODUCT_MERGE' -or $Ledger.publication.productMergeCommit -or
       $Ledger.publication.mergeContainsTestedSource -or $Ledger.publication.expectedBaseCommit -cne $ExpectedBaseCommit -or
       [string]$Ledger.publication.productPr -cne $ProductPrNumber) {throw 'Sprint33 premerge source/base/PR authority differs.'}
}