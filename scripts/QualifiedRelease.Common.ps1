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

function Assert-KmgClosureProofFiles {
    param([Parameter(Mandatory=$true)]$Ledger)
    foreach ($proof in @($Ledger.coverage | ForEach-Object { $_.proofs })) {
        if ([string]$proof.sha256 -cnotmatch '^[0-9a-f]{64}$' -or
            -not (Test-Path -LiteralPath $proof.path -PathType Leaf) -or
            (Get-FileHash -LiteralPath $proof.path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $proof.sha256) {
            throw 'Closure evidence is missing or changed after qualification.'
        }
    }
}
