Set-StrictMode -Version Latest

function Assert-KmgReleaseOrigin {
    param([Parameter(Mandatory=$true)][string]$Origin)
    $allowed = @('git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git',
        'https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules.git',
        'https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules')
    if ($Origin -cnotin $allowed) { throw 'Unexpected release repository origin.' }
}

function Assert-KmgClosureReleaseDecision {
    param([Parameter(Mandatory=$true)]$Ledger, [Parameter(Mandatory=$true)]$Candidate)
    $required = @('boundary-on','boundary-off','matched149-control','gunslinger-only',
        'call-of-the-wild','arms-armor','toggle-custom-soundpacks','high-risk-combined',
        'required-module-states','whole-roster-enabled-persistence',
        'native-cleanup-save-and-fresh-absence','module-disabled-deserialization-cleanup',
        'sprint18-21-regressions','1063-roots-and-29-wrappers',
        'resource-lifecycle-template-initialization','exact-package-release-qualification')
    if ($Ledger.schemaVersion -ne 1 -or -not $Ledger.releaseEligible -or
        $Ledger.status -cnotin @('QUALIFIED','QUALIFIED_WITH_DOCUMENTED_BASELINE_EXCEPTIONS')) {
        throw 'Closure is not qualified for release.'
    }
    foreach ($pair in @(@('source','commit'),@('zipSha256','packageSha256'),
            @('dllSha256','dllSha256'),@('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))) {
        if ([string]$Ledger.candidate.($pair[0]) -cne [string]$Candidate.($pair[1])) {
            throw "Closure candidate identity differs: $($pair[0])."
        }
    }
    if ($Ledger.candidate.pipelineExitCode -ne 0 -or $Ledger.candidate.fullSuite.exitCode -ne 0 -or
        $Ledger.candidate.fullSuite.failures -ne 0 -or $Ledger.candidate.fullSuite.executed -le 0 -or
        $Ledger.candidate.fullSuite.executed -ne $Ledger.candidate.fullSuite.registered -or
        -not $Ledger.candidate.fullSuite.unfiltered) { throw 'Fresh complete candidate suite did not pass.' }
    if ($Ledger.roster.generatedRoots -ne 1063 -or $Ledger.roster.nativeWrappers -ne 29 -or
        $Ledger.roster.choices -ne 1092) { throw 'Release roster identity is not qualified.' }
    if ($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or
        -not $Ledger.restoration.workingFixtureClean) { throw 'Restoration or fixture cleanup is incomplete.' }
    $ids = @($Ledger.coverage | ForEach-Object { $_.id })
    if (@($ids | Sort-Object -Unique).Count -ne $ids.Count) { throw 'Duplicate closure coverage identity.' }
    foreach ($id in $required) {
        $row = @($Ledger.coverage | Where-Object { $_.id -ceq $id })
        if ($row.Count -ne 1 -or $row[0].result -cnotin @('PASS','REUSED_PASS','BASELINE_EXCEPTION')) {
            throw "Mandatory closure coverage is incomplete: $id."
        }
    }
    foreach ($row in $Ledger.coverage) {
        if ($row.result -cnotin @('PASS','REUSED_PASS','BASELINE_EXCEPTION') -or
            @($row.proofs).Count -eq 0) { throw "Unaccounted coverage or absent proof: $($row.id)." }
        if ($row.result -ceq 'REUSED_PASS' -and
            ([string]::IsNullOrWhiteSpace([string]$row.reuseJustification) -or
             [string]::IsNullOrWhiteSpace([string]$row.priorSource))) { throw "Missing reuse provenance: $($row.id)." }
    }
    $allowed = @('teleportation-scroll-template-2c283c993b233df53b487fc7eeac2ba3',
        'teleportation-scroll-template-2a2b0185be1d2dba5aa1b5a24774e17d',
        'teleportation-scroll-template-42d3daeb7d503687df8953b47727372b',
        'feature-module-gunslinger-publication','feature-module-eastern-weapons-publication-gate',
        'feature-module-elven-branched-spears-publication-gate')
    foreach ($exception in $Ledger.baselineExceptions) {
        if ($exception.id -cnotin $allowed -or $exception.rawStatus -cne 'FAIL' -or
            $exception.baselineVersion -cne '0.0.149' -or
            -not $exception.matchedClauseOrRows -or -not $exception.dependenciesUnchanged -or
            -not $exception.outsideSprint21AndSafe -or -not $exception.downstreamCoverageComplete -or
            [string]::IsNullOrWhiteSpace([string]$exception.knownIssue) -or
            [string]::IsNullOrWhiteSpace([string]$exception.followUp)) {
            throw 'Baseline exception does not meet the owner mission conditions.'
        }
    }
    $exceptions = @($Ledger.baselineExceptions).Count
    if (($exceptions -gt 0) -ne ($Ledger.status -ceq 'QUALIFIED_WITH_DOCUMENTED_BASELINE_EXCEPTIONS') -or
        (@($Ledger.coverage | Where-Object {$_.result -ceq 'BASELINE_EXCEPTION'}).Count -gt 0 -and
         $exceptions -eq 0)) { throw 'Raw exception disposition and release status disagree.' }
    foreach ($finding in $Ledger.findings) {
        if ($finding.classification -cnotin @('FIXTURE_DEFECT','DEMONSTRATED_BASELINE_DEFECT') -or
            ($finding.classification -ceq 'FIXTURE_DEFECT' -and $finding.native -cne 'VERIFIED')) {
            throw "Boundary finding is not dispositioned: $($finding.id)."
        }
    }
}

# Release-tooling lineage check. Immutable merged PRODUCT ancestry and reviewed
# post-merge REPORTING ancestry are different things, and conflating them made
# promotion impossible once the reporting branch carried the records that
# describe its own merge. This distinguishes them without removing any check.
#
# Returns a provenance object; throws before any external mutation otherwise.
function Assert-KmgReportingLineage {
    param(
        [Parameter(Mandatory=$true)][string]$RepositoryRoot,
        [Parameter(Mandatory=$true)][string]$ReportingCommit,
        [Parameter(Mandatory=$true)][string]$MergedReleaseCommit,
        [Parameter(Mandatory=$true)][string]$MergedPrHead,
        [Parameter(Mandatory=$true)][string[]]$AllowedReportingPaths
    )
    function Git-Lines([string[]]$Arguments) {
        $out = @(& git -C $RepositoryRoot @Arguments 2>$null)
        if ($LASTEXITCODE -ne 0) { throw "git $($Arguments -join ' ') failed with exit code $LASTEXITCODE." }
        return $out
    }
    function Is-Ancestor([string]$Ancestor, [string]$Descendant) {
        $null = & git -C $RepositoryRoot merge-base --is-ancestor $Ancestor $Descendant 2>$null
        return ($LASTEXITCODE -eq 0)
    }
    foreach ($sha in @($ReportingCommit, $MergedReleaseCommit, $MergedPrHead)) {
        if ([string]$sha -cnotmatch '^[0-9a-f]{40}$') {
            throw 'Reporting lineage requires full forty-character commit identities.'
        }
    }
    # Accepted path that predates this repair: the reporting commit is already
    # part of the product merge, so nothing outside it is being released.
    if (Is-Ancestor $ReportingCommit $MergedReleaseCommit) {
        return [pscustomobject]@{ mode='contained-in-product-merge'
            reportingCommit=$ReportingCommit; mergedPrHead=$MergedPrHead
            mergedReleaseCommit=$MergedReleaseCommit; reportingDeltaPaths=@() }
    }
    # Reviewed post-merge reporting path. Every one of these must hold.
    if (-not (Is-Ancestor $MergedPrHead $MergedReleaseCommit)) {
        throw 'The merged pull-request head is not an ancestor of the product merge.'
    }
    if (-not (Is-Ancestor $MergedPrHead $ReportingCommit)) {
        throw 'The reporting commit does not descend from the head that was merged.'
    }
    $base = (Git-Lines @('merge-base',$ReportingCommit,$MergedReleaseCommit) | Select-Object -First 1)
    if ([string]$base -cne $MergedPrHead) {
        throw 'Reporting and product lineages diverge somewhere other than the merged head.'
    }
    $changed = @(Git-Lines @('diff','--name-only',$MergedPrHead,$ReportingCommit) |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($changed.Count -eq 0) {
        throw 'A post-merge reporting commit must actually change a reviewed reporting path.'
    }
    $unexpected = @($changed | Where-Object { $AllowedReportingPaths -cnotcontains $_ })
    if ($unexpected.Count -ne 0) {
        throw ('Post-merge reporting changes outside the reviewed paths: ' + ($unexpected -join ', '))
    }
    # Belt and braces: never accept a compiled or packaged input here, whatever
    # the allow-list happens to say. A records change cannot hide product work.
    $frozen = @('src/','assets/','blueprints/','validation/','tools/','Info.json',
        'Directory.Build.props','scripts/package.ps1','scripts/Build-Local.ps1',
        'scripts/validate-package.ps1','scripts/validate-build-output.ps1')
    foreach ($path in $changed) {
        foreach ($root in $frozen) {
            if ($path -ceq $root -or $path.StartsWith($root, [StringComparison]::Ordinal)) {
                throw ('A frozen executable or package input changed after the merge: ' + $path)
            }
        }
    }
    return [pscustomobject]@{ mode='reviewed-post-merge-reporting'
        reportingCommit=$ReportingCommit; mergedPrHead=$MergedPrHead
        mergedReleaseCommit=$MergedReleaseCommit; reportingDeltaPaths=@($changed) }
}

function Assert-KmgClosureProofFiles {
    param([Parameter(Mandatory=$true)]$Ledger)
    foreach ($proof in @($Ledger.coverage | ForEach-Object { $_.proofs })) {
        if ([string]$proof.sha256 -cnotmatch '^[0-9a-f]{64}$' -or
            -not (Test-Path -LiteralPath $proof.path -PathType Leaf) -or
            (Get-FileHash -LiteralPath $proof.path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $proof.sha256) {
            throw 'Closure evidence is missing or changed after qualification.'
        }
        # Deployment records, runtime evidence and profile receipts produced
        # under a development candidate say so; none of them qualifies a release.
        if ([IO.File]::ReadAllText($proof.path) -match '"candidateKind"\s*:\s*"development"') {
            throw 'Development-candidate evidence cannot qualify a release.'
        }
    }
}

function Assert-KmgSprint22ReleaseDecision {
    param([Parameter(Mandatory=$true)]$Ledger,[Parameter(Mandatory=$true)]$Candidate)
    if($Ledger.schemaVersion -ne 1 -or $Ledger.mission -cne 'Sprint22Ankylosaurus' -or
        $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or $Ledger.sprint23Started -or
        @($Ledger.baselineExceptions).Count -ne 0 -or @($Ledger.unresolvedFindings).Count -ne 0) {
        throw 'Sprint22 is not fully qualified; no new exception or unfinished requirement is authorized.'
    }
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),@('dllSha256','dllSha256'),
        @('dllMvid','dllMvid'),@('sourceFingerprint','sourceStateSha256'))) {
        if([string]$Ledger.candidate.($pair[0]) -cne [string]$Candidate.($pair[1])) { throw 'Sprint22 tested artifact identity differs.' }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or $suite.failures -ne 0 -or
        $suite.executed -le 0 -or $suite.executed -ne $suite.registered -or -not $suite.unfiltered) { throw 'Sprint22 candidate gate did not execute and pass its full suite.' }
    if($Ledger.roster.generatedRoots -ne 1063 -or $Ledger.roster.nativeWrappers -ne 29 -or $Ledger.roster.choices -ne 1092 -or
        $Ledger.roster.preservedAnkylosaurusRoots -ne 10 -or $Ledger.roster.protectedIdentitySetsEqual -ne $true) { throw 'Sprint22 published identities or menu surface changed.' }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or -not $Ledger.restoration.workingFixtureClean) { throw 'Sprint22 restoration and clean fixture are required.' }
    $mandatory=@('ankylosaurus-profile-carriers-and-ten-routes','tail-rider-rtwp-and-turn-based','original-body-contact-huge-crowd',
        'private-resource-teardown-and-fallback','targeted-enabled-cold-load-cleanup-absence','module-disabled-cold-load-reenable',
        'gunslinger-only','high-risk-combined','exact-package-release-qualification')
    $ids=@($Ledger.coverage | ForEach-Object {$_.id})
    if(@($ids | Sort-Object -Unique).Count -ne $ids.Count) { throw 'Sprint22 duplicate coverage identity.' }
    foreach($id in $mandatory) {
        $row=@($Ledger.coverage | Where-Object {$_.id -ceq $id})
        if($row.Count -ne 1 -or $row[0].result -cne 'PASS') { throw "Sprint22 fresh mandatory coverage missing: $id" }
    }
    foreach($row in $Ledger.coverage) {
        if($row.result -cnotin @('PASS','REUSED_PASS') -or @($row.proofs).Count -eq 0 -or @($row.evidenceSources).Count -eq 0) { throw 'Sprint22 coverage lacks evidence.' }
        if($row.result -ceq 'REUSED_PASS' -and ([string]::IsNullOrWhiteSpace([string]$row.reuseJustification) -or
            [string]::IsNullOrWhiteSpace([string]$row.priorSource))) { throw 'Sprint22 inherited proof lacks dependency comparison/provenance.' }
        if($row.result -ceq 'PASS') {
            foreach($source in $row.evidenceSources) {
                if($source.source -cne $Candidate.commit -or $source.packageSha256 -cne $Candidate.packageSha256 -or
                    $source.dllSha256 -cne $Candidate.dllSha256 -or $source.dllMvid -cne $Candidate.dllMvid) { throw 'Sprint22 native coverage is bound to other candidate bytes.' }
            }
        }
    }
}

function Assert-KmgSprint23ChargeRoundingReleaseDecision {
    param([Parameter(Mandatory=$true)]$Ledger,[Parameter(Mandatory=$true)]$Candidate)
    if($Ledger.schemaVersion -ne 1 -or
        $Ledger.mission -cne 'Sprint23TriceratopsPowerfulChargeRounding' -or
        $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or
        $Ledger.sprint24Started -or @($Ledger.baselineExceptions).Count -ne 0 -or
        @($Ledger.unresolvedFindings).Count -ne 0) {
        throw 'Sprint 23 charge-rounding patch is not fully qualified.'
    }
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),
        @('dllSha256','dllSha256'),@('dllMvid','dllMvid'),
        @('sourceFingerprint','sourceStateSha256'))) {
        if([string]$Ledger.candidate.($pair[0]) -cne
            [string]$Candidate.($pair[1])) {
            throw 'Charge-rounding tested artifact identity differs.'
        }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or
        $suite.failures -ne 0 -or $suite.executed -le 0 -or
        $suite.executed -ne $suite.registered -or -not $suite.unfiltered) {
        throw 'Charge-rounding candidate gate did not pass its full suite.'
    }
    if($Ledger.roster.generatedRoots -ne 1075 -or
        $Ledger.roster.nativeWrappers -ne 29 -or
        $Ledger.roster.choices -ne 1104 -or
        $Ledger.roster.identityChanges -ne 0 -or
        $Ledger.roster.routeChanges -ne 0 -or
        $Ledger.roster.assetChanges -ne 0 -or
        $Ledger.roster.serializationChanges -ne 0) {
        throw 'The arithmetic patch changed the released content surface.'
    }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or
        -not $Ledger.restoration.workingFixtureClean) {
        throw 'Charge-rounding restoration and fixture cleanup are incomplete.'
    }
    $mandatory=@('source-reproduction-and-focused-correction',
        'complete-source-build-package-gate','triceratops-charge-rtwp',
        'triceratops-charge-turn-based','high-risk-combined-charge',
        'ordinary-power-attack-critical','released-rhino-fixed-increment',
        'v0152-unchanged-sprint23-evidence','exact-package-release-qualification')
    $ids=@($Ledger.coverage | ForEach-Object {$_.id})
    if(@($ids | Sort-Object -Unique).Count -ne $ids.Count) {
        throw 'Charge-rounding coverage contains duplicate identities.'
    }
    foreach($id in $mandatory) {
        $row=@($Ledger.coverage | Where-Object {$_.id -ceq $id})
        if($row.Count -ne 1 -or $row[0].result -cnotin @('PASS','REUSED_PASS')) {
            throw "Charge-rounding mandatory coverage is incomplete: $id"
        }
    }
    foreach($row in $Ledger.coverage) {
        if($row.result -cnotin @('PASS','REUSED_PASS') -or
            @($row.proofs).Count -eq 0 -or
            @($row.evidenceSources).Count -eq 0) {
            throw "Charge-rounding coverage lacks evidence: $($row.id)"
        }
        if($row.result -ceq 'REUSED_PASS') {
            if([string]::IsNullOrWhiteSpace([string]$row.reuseJustification) -or
                [string]::IsNullOrWhiteSpace([string]$row.priorSource)) {
                throw 'Charge-rounding reused proof lacks provenance.'
            }
        } else {
            foreach($source in $row.evidenceSources) {
                if($source.source -cne $Candidate.commit -or
                    $source.packageSha256 -cne $Candidate.packageSha256 -or
                    $source.dllSha256 -cne $Candidate.dllSha256 -or
                    $source.dllMvid -cne $Candidate.dllMvid) {
                    throw 'Fresh charge-rounding proof is bound to other candidate bytes.'
                }
            }
        }
    }
}

function Assert-KmgSprint24TheropodReleaseDecision {
    param([Parameter(Mandatory=$true)]$Ledger,[Parameter(Mandatory=$true)]$Candidate)
    if($Ledger.schemaVersion -ne 1 -or
        $Ledger.mission -cne 'Sprint24DeinonychusTyrannosaurus' -or
        $Ledger.status -cne 'QUALIFIED' -or -not $Ledger.releaseEligible -or
        $Ledger.sprint25Started -or @($Ledger.baselineExceptions).Count -ne 0 -or
        @($Ledger.unresolvedFindings).Count -ne 0) {
        throw 'Sprint 24 is not fully qualified; no new exception or unfinished requirement is authorized.'
    }
    foreach($pair in @(@('source','commit'),@('zipSha256','packageSha256'),
        @('dllSha256','dllSha256'),@('dllMvid','dllMvid'),
        @('sourceFingerprint','sourceStateSha256'))) {
        if([string]$Ledger.candidate.($pair[0]) -cne
            [string]$Candidate.($pair[1])) {
            throw 'Sprint 24 tested artifact identity differs.'
        }
    }
    $suite=$Ledger.candidate.fullSuite
    if($Ledger.candidate.pipelineExitCode -ne 0 -or $suite.exitCode -ne 0 -or
        $suite.failures -ne 0 -or $suite.executed -le 0 -or
        $suite.executed -ne $suite.registered -or -not $suite.unfiltered) {
        throw 'Sprint 24 candidate gate did not execute and pass its full suite.'
    }
    # Publication removes exactly the two suppression keys and moves nothing.
    if($Ledger.roster.projectCreatures -ne 107 -or
        $Ledger.roster.monsterCreatures -ne 95 -or
        $Ledger.roster.monsterPlacements -ne 543 -or
        $Ledger.roster.natureAllyCreatures -ne 95 -or
        $Ledger.roster.natureAllyPlacements -ne 550 -or
        $Ledger.roster.generatedRoots -ne 1093 -or
        $Ledger.roster.nativeWrappers -ne 29 -or
        $Ledger.roster.choices -ne 1122 -or
        $Ledger.roster.suppressedLogicalPlacements -ne 0 -or
        $Ledger.roster.identityChanges -ne 0 -or
        $Ledger.roster.routeOrderingChanges -ne 0 -or
        $Ledger.roster.protectedIdentitySetsEqual -ne $true) {
        throw 'Sprint 24 published identities or menu surface changed.'
    }
    # The four accepted limitations, exactly; no new waiver.
    $limits=@('ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD',
        'SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED',
        'PASSIVE_CREATURE_SENSES_UNMODELED',
        'RUN_NATIVE_SUMMON_AI_DISPOSITION')
    $declared=@($Ledger.acceptedLimitations)
    if($declared.Count -ne $limits.Count -or
        @(Compare-Object -ReferenceObject ($limits | Sort-Object) `
            -DifferenceObject ($declared | Sort-Object)).Count -ne 0) {
        throw 'Sprint 24 accepted limitations are not exactly the four recorded labels.'
    }
    if($Ledger.swallowedStatePersistenceExemption) {
        throw 'The clean-reset contract forbids a swallowed-state persistence exemption.'
    }
    if($Ledger.restoration.current -cne 'EXACT_INTAKE_VERIFIED' -or
        -not $Ledger.restoration.workingFixtureClean) {
        throw 'Sprint 24 restoration and clean fixture are required.'
    }
    # The release must promote the merge that actually carries this source.
    if([string]::IsNullOrWhiteSpace([string]$Ledger.publication.productMergeCommit) -or
        $Ledger.publication.mergeContainsTestedSource -ne $true -or
        $Ledger.publication.tag -cne 'v0.0.155' -or
        $Ledger.publication.prerelease -or $Ledger.publication.draft -or
        -not $Ledger.publication.latest) {
        throw 'Sprint 24 publication state or merge lineage is invalid.'
    }
    $mandatory=@('shared-controls-released-consumers-and-exact-ai',
        'targeted-enabled-cold-load-cleanup-absence',
        'module-disabled-cold-load-reenable',
        'complete-public-source-build-package-gate',
        'public-inventory-high-risk-combined',
        'public-eighteen-roots-real-parent-paths',
        'exact-package-release-qualification')
    $ids=@($Ledger.coverage | ForEach-Object {$_.id})
    if(@($ids | Sort-Object -Unique).Count -ne $ids.Count) {
        throw 'Sprint 24 duplicate coverage identity.'
    }
    foreach($id in $mandatory) {
        $row=@($Ledger.coverage | Where-Object {$_.id -ceq $id})
        if($row.Count -ne 1 -or $row[0].result -cnotin @('PASS','REUSED_PASS')) {
            throw "Sprint 24 mandatory coverage is incomplete: $id"
        }
    }
    foreach($row in $Ledger.coverage) {
        if($row.result -cnotin @('PASS','REUSED_PASS') -or
            @($row.proofs).Count -eq 0 -or
            @($row.evidenceSources).Count -eq 0) {
            throw "Sprint 24 coverage lacks evidence: $($row.id)"
        }
        if($row.result -ceq 'REUSED_PASS') {
            if([string]::IsNullOrWhiteSpace([string]$row.reuseJustification) -or
                [string]::IsNullOrWhiteSpace([string]$row.priorSource)) {
                throw 'Sprint 24 inherited proof lacks dependency comparison/provenance.'
            }
        } else {
            foreach($source in $row.evidenceSources) {
                if($source.source -cne $Candidate.commit -or
                    $source.packageSha256 -cne $Candidate.packageSha256 -or
                    $source.dllSha256 -cne $Candidate.dllSha256 -or
                    $source.dllMvid -cne $Candidate.dllMvid) {
                    throw 'Sprint 24 native coverage is bound to other candidate bytes.'
                }
            }
        }
    }
}

function Get-KmgClosurePublicCoverage {
    param([Parameter(Mandatory=$true)]$Ledger)
    foreach ($row in $Ledger.coverage) {
        if ($null -eq $row.PSObject.Properties['evidenceSources'] -or
            @($row.evidenceSources).Count -eq 0) {
            throw "Public release coverage lacks execution provenance: $($row.id)."
        }
        [ordered]@{
            id=$row.id; result=$row.result; evidenceSources=@($row.evidenceSources)
            reuseJustification=if ($row.PSObject.Properties['reuseJustification']) { $row.reuseJustification } else { $null }
            proofs=@($row.proofs | ForEach-Object {
                [ordered]@{
                    evidenceFile=[IO.Path]::GetFileName($_.path)
                    evidenceRun=[IO.Path]::GetFileName((Split-Path $_.path -Parent))
                    sha256=$_.sha256
                }
            })
        }
    }
}
