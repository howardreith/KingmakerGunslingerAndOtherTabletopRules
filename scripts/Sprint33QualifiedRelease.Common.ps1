Set-StrictMode -Version Latest

# Sprint 33 uses the existing qualification ledger and exact-artifact publisher.
function Read-KmgSprint33Proof($Proof) {
    if([string]$Proof.sha256 -cnotmatch '^[0-9a-f]{64}$' -or
       -not(Test-Path -LiteralPath $Proof.path -PathType Leaf) -or
       (Get-FileHash -LiteralPath $Proof.path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $Proof.sha256) { throw 'Sprint33 proof missing or changed.' }
    Get-Content -LiteralPath $Proof.path -Raw | ConvertFrom-Json
}
function Assert-KmgSprint33Reuse($Row,$Candidate) {
    if($Row.result -cne 'REUSED_PASS' -or $Row.id -cnotin @('twelve-preserved-routes','dedicated-models-motion-ownership','natural-invisibility-purge','succubus-preservation-stalker-turn') -or
       [string]::IsNullOrWhiteSpace($Row.reuseJustification)) { throw 'Unselected Sprint33 evidence reuse.' }
    foreach($source in $Row.evidenceSources) {
        $receipt=Read-KmgSprint33Proof $source.receiptProof
        $native=Read-KmgSprint33Proof $source.nativeProof
        $transaction=Read-KmgSprint33Proof $source.transactionProof
        $loaded=Read-KmgSprint33Proof $source.loadedProof
        $orchestration=Read-KmgSprint33Proof $source.orchestrationProof
        $deployment=Read-KmgSprint33Proof $source.deploymentProof
        $protected=Read-KmgSprint33Proof $source.protectedProof
        if($receipt.candidateKind -cne 'development' -or $receipt.releaseQualification -ne $false -or
           $receipt.purpose -cne 'sprint33-shadow-outsiders' -or
           $receipt.repositoryRoot -ine 'C:\Dev\KingmakerGunslingerLab\wt\s33' -or
           $receipt.branch -cne 'codex/expanded-summoning-sprint33-shadow-outsiders') { throw 'Original Sprint33 development attribution lost.' }
        foreach($key in @('packageSha256','dllSha256','dllMvid')) { if($source.$key -cne $receipt.$key){throw 'Reused artifact identity differs.'} }
        if($source.source -cne $receipt.commit -or $native.gitCommit -cne $receipt.commit -or
           $native.runId -cne $source.runId -or $native.status -cne 'PASS' -or
           @($native.assertions|Where-Object status -cne 'PASS').Count -or
           $deployment.packageSha256 -cne $receipt.packageSha256 -or -not $transaction.restorationVerified) { throw 'Original native proof/restoration differs.' }
        foreach($key in @('commit','packageSha256','dllSha256','dllMvid')){if($deployment.$key -cne $receipt.$key){throw 'Original deployment differs.'}}
        if($deployment.developmentReceiptSha256 -cne $source.receiptProof.sha256 -or $loaded.gitCommit -cne $receipt.commit -or
           $loaded.loadedModuleSha256 -cne $receipt.dllSha256 -or $loaded.moduleVersionId -cne $receipt.dllMvid -or
           $orchestration.status -cne 'PASS' -or $orchestration.runId -cne $source.runId -or
           [IO.Path]::GetFullPath($orchestration.resultPath) -ine [IO.Path]::GetFullPath($source.nativeProof.path) -or
           [IO.Path]::GetFullPath($orchestration.deploymentManifestPath) -ine [IO.Path]::GetFullPath($source.deploymentProof.path) -or
           $orchestration.kingmakerProcessId -ne $loaded.processId -or $transaction.status -cne 'Restored' -or
           $transaction.profileId -cne $source.profile -or [DateTime]$transaction.createdAtUtc -gt [DateTime]$orchestration.startedAtUtc -or
           [DateTime]$transaction.restoredAtUtc -lt [DateTime]$orchestration.completedAtUtc -or
           $protected.status -cne 'RESTORED' -or -not $protected.restoration.allOriginalSavesExact -or -not $protected.restoration.gameExitConfirmed){throw 'Original launch/load/restoration correlation differs.'}
        foreach($proof in @($source.receiptProof,$source.nativeProof,$source.transactionProof,$source.loadedProof,$source.orchestrationProof,$source.deploymentProof,$source.protectedProof)) {
            if(@($Row.proofs|Where-Object {$_.path -ceq $proof.path -and $_.sha256 -ceq $proof.sha256}).Count -ne 1){throw 'Original proof omitted.'}
        }
        if(@($source.requiredAssertions).Count -eq 0 -or @($source.dependencies).Count -eq 0 -or
           [string]::IsNullOrWhiteSpace($source.environmentComparison) -or [string]::IsNullOrWhiteSpace($source.observerMeaning)) { throw 'Reuse comparison absent.' }
        foreach($name in $source.requiredAssertions) {
            if(@($native.assertions|Where-Object {$_.name -ceq $name -and $_.status -ceq 'PASS'}).Count -ne 1){throw 'Required reused observation absent.'}
        }
        $old=@($receipt.references.files|ForEach-Object {$_.relativePath+':'+$_.sha256}|Sort-Object -CaseSensitive)
        $new=@($Candidate.references.files|ForEach-Object {$_.relativePath+':'+$_.sha256}|Sort-Object -CaseSensitive)
        if(($old -join '|') -cne ($new -join '|')){throw 'Installed references changed.'}
        foreach($dep in $source.dependencies) {
            $oldBlob=(& git rev-parse ($source.source+':'+$dep.path)).Trim()
            if($LASTEXITCODE -ne 0){throw 'Original dependency unavailable.'}
            $newBlob=(& git rev-parse ($Candidate.commit+':'+$dep.path)).Trim()
            if($LASTEXITCODE -ne 0 -or $oldBlob -cne $dep.originalBlob -or $newBlob -cne $dep.candidateBlob){throw 'Dependency attribution differs.'}
            if($oldBlob -cne $newBlob -and ($dep.disposition -cne 'REVIEWED_UNCHANGED_OBSERVED_PATH' -or [string]::IsNullOrWhiteSpace($dep.unchangedConsumerReason))){throw 'Changed dependency requires fresh proof or explicit affected-path review.'}
        }
    }
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