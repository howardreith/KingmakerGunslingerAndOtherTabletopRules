[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'QualifiedRelease.Common.ps1')

# Disposable git fixtures only. Nothing here touches the real repository, the
# network, the installation or any release surface.
$allowed=@(
    'AUTONOMOUS-RESUME.md',
    'docs/SPRINT24-COVERAGE.md',
    'docs/RELEASE-NOTES-0.0.155.md',
    'planning/SPRINT24-THEROPOD-DINOSAURS-LEDGER.json',
    'scripts/Publish-QualifiedCandidate.ps1',
    'scripts/QualifiedRelease.Common.ps1',
    'scripts/Test-Sprint24Release.ps1',
    'scripts/Test-Sprint24ReportingLineage.ps1',
    'scripts/validate-repository.ps1')

function New-Fixture {
    $root=Join-Path ([IO.Path]::GetTempPath()) ('kmg-lineage-'+[Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $root | Out-Null
    Push-Location $root
    try {
        $null = & git init --initial-branch=master 2>$null
        $null = & git config user.email 'fixture@example.invalid' 2>$null
        $null = & git config user.name 'Fixture' 2>$null
        $null = & git config commit.gpgsign false 2>$null
        function Commit([string]$relative,[string]$text,[string]$message) {
            $full=Join-Path $root $relative
            New-Item -ItemType Directory -Force -Path (Split-Path $full -Parent) | Out-Null
            Set-Content -LiteralPath $full -Value $text -Encoding UTF8
            $null = & git add -A 2>$null
            $null = & git commit -q -m $message 2>$null
        }
        # Base, then a product branch whose head is what gets merged.
        Commit 'src/Product.cs' 'base' 'base'
        $base = (& git rev-parse HEAD).Trim()
        $null = & git checkout -q -b product 2>$null
        Commit 'src/Product.cs' 'product work' 'product work'
        $prHead = (& git rev-parse HEAD).Trim()
        # An unrelated master commit, then the product merge.
        $null = & git checkout -q master 2>$null
        Commit 'docs/Other.md' 'other' 'unrelated master work'
        $null = & git merge -q --no-ff -m 'Merge product' product 2>$null
        $merge = (& git rev-parse HEAD).Trim()
        # Reporting continues on the product branch after the merge.
        $null = & git checkout -q product 2>$null
        Commit 'AUTONOMOUS-RESUME.md' 'records' 'record the merge'
        Commit 'scripts/Publish-QualifiedCandidate.ps1' 'tooling' 'repair the publisher'
        $reporting = (& git rev-parse HEAD).Trim()
        # A branch off the base that is neither contained in the merge nor a
        # descendant of the merged head: the genuine unrelated-lineage shape.
        $null = & git checkout -q -b rogue $base 2>$null
        Commit 'AUTONOMOUS-RESUME.md' 'rogue records' 'rogue records'
        $rogue = (& git rev-parse HEAD).Trim()
        $null = & git checkout -q product 2>$null
        return [pscustomobject]@{ Root=$root; Base=$base; PrHead=$prHead; Merge=$merge; Reporting=$reporting; Rogue=$rogue }
    } finally { Pop-Location }
}

function Remove-Fixture($f) {
    try { Remove-Item -Recurse -Force -LiteralPath $f.Root -ErrorAction Stop } catch { }
}

function Expect-Accept($f,$name,[hashtable]$over) {
    $a=@{ RepositoryRoot=$f.Root; ReportingCommit=$f.Reporting; MergedReleaseCommit=$f.Merge
          MergedPrHead=$f.PrHead; AllowedReportingPaths=$allowed }
    foreach ($k in $over.Keys) { $a[$k]=$over[$k] }
    $r = Assert-KmgReportingLineage @a
    if ($null -eq $r -or [string]::IsNullOrWhiteSpace([string]$r.mode)) { throw "Accepted case returned no provenance: $name" }
    return $r
}

function Expect-Refuse($f,$name,[hashtable]$over) {
    $a=@{ RepositoryRoot=$f.Root; ReportingCommit=$f.Reporting; MergedReleaseCommit=$f.Merge
          MergedPrHead=$f.PrHead; AllowedReportingPaths=$allowed }
    foreach ($k in $over.Keys) { $a[$k]=$over[$k] }
    $refused=$false
    try { $null = Assert-KmgReportingLineage @a } catch { $refused=$true }
    if (-not $refused) { throw "Unsafe reporting lineage was accepted: $name" }
}

$checks=0
$f=New-Fixture
try {
    # 1. The exact safe post-merge reporting shape.
    $r=Expect-Accept $f 'reviewed post-merge reporting' @{}
    if ($r.mode -cne 'reviewed-post-merge-reporting') { throw 'Wrong mode for the post-merge shape.' }
    if (@($r.reportingDeltaPaths).Count -ne 2) { throw 'Reporting delta was not reported exactly.' }
    $checks++

    # 2. Existing accepted path: reporting already contained in the merge.
    $r=Expect-Accept $f 'contained in product merge' @{ ReportingCommit=$f.PrHead }
    if ($r.mode -cne 'contained-in-product-merge') { throw 'Wrong mode for the contained path.' }
    $checks++

    # 3. Wrong PR merge head.
    Expect-Refuse $f 'merged head not in merge' @{ MergedPrHead=$f.Base }; $checks++

    # 4. Reporting that does not descend from the merged head and is not
    #    contained in the merge either.
    Expect-Refuse $f 'unrelated reporting lineage' @{ ReportingCommit=$f.Rogue }; $checks++

    # 5. Moved master: a product merge that the reviewed head did not merge
    #    into, checked from a reporting commit outside that merge.
    Expect-Refuse $f 'moved product merge' @{ MergedReleaseCommit=$f.Base; ReportingCommit=$f.Rogue }; $checks++

    # 6. Short or malformed identities. An empty string is deliberately not
    #    tested through the mandatory parameter: PowerShell would prompt for it
    #    rather than call the function, which hangs a non-interactive run.
    foreach ($bad in @('deadbeef', ($f.Reporting.Substring(0,39)), ('z'*40))) {
        Expect-Refuse $f 'malformed identity' @{ ReportingCommit=$bad }; $checks++
    }

    # 7. An empty allow-list cannot accept a post-merge reporting delta.
    Expect-Refuse $f 'empty allow-list' @{ AllowedReportingPaths=@() }; $checks++

    # 8. Wildcards are not paths: scripts/* must not satisfy the allow-list.
    Expect-Refuse $f 'wildcard allow-list' @{ AllowedReportingPaths=@('AUTONOMOUS-RESUME.md','scripts/*') }; $checks++
} finally { Remove-Fixture $f }

# 9. Product source changed after the merge, disguised as a records commit.
$f=New-Fixture
try {
    Push-Location $f.Root
    try {
        Set-Content -LiteralPath (Join-Path $f.Root 'src/Product.cs') -Value 'sneaked in' -Encoding UTF8
        $null = & git add -A 2>$null; $null = & git commit -q -m 'record the merge' 2>$null
        $sneak=(& git rev-parse HEAD).Trim()
    } finally { Pop-Location }
    Expect-Refuse $f 'product source disguised as records' @{ ReportingCommit=$sneak
        AllowedReportingPaths=@($allowed + 'src/Product.cs') }
    $checks++
} finally { Remove-Fixture $f }

# 10. A build/package input changed after the merge, even if allow-listed.
foreach ($input in @('Info.json','Directory.Build.props','scripts/package.ps1','tools/validate_repository.py','assets/x.png','validation/static-validation.json')) {
    $f=New-Fixture
    try {
        Push-Location $f.Root
        try {
            $full=Join-Path $f.Root $input
            New-Item -ItemType Directory -Force -Path (Split-Path $full -Parent) | Out-Null
            Set-Content -LiteralPath $full -Value 'changed' -Encoding UTF8
            $null = & git add -A 2>$null; $null = & git commit -q -m 'records' 2>$null
            $c=(& git rev-parse HEAD).Trim()
        } finally { Pop-Location }
        Expect-Refuse $f "frozen input $input" @{ ReportingCommit=$c; AllowedReportingPaths=@($allowed + $input) }
        $checks++
    } finally { Remove-Fixture $f }
}

# 11. A post-merge reporting commit that changes nothing is not a reason to publish.
$f=New-Fixture
try {
    Push-Location $f.Root
    try {
        # Branch from the merged head itself so the delta really is empty.
        $null = & git checkout -q -b recordless $f.PrHead 2>$null
        $null = & git commit -q --allow-empty -m 'empty' 2>$null
        $e=(& git rev-parse HEAD).Trim()
    } finally { Pop-Location }
    Expect-Refuse $f 'empty reporting delta' @{ ReportingCommit=$e }
    $checks++
} finally { Remove-Fixture $f }

Write-Host "Sprint24 reporting-lineage safety passed: $checks checks; no tag, push or release mutation."
