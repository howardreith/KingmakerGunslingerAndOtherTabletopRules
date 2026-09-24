<#
.SYNOPSIS
Runs the Expanded Summoning mechanical scenario inside the two required
compatibility transactions against a mod-independent disposable fixture
derived from the protected automation baseline, and restores everything.

.DESCRIPTION
The shared working save is rewritten by every persistence run under whatever
mods the live tree carries. On 2026-09-23 it carried 465 serialized objects
typed by TweakOrTreat and 32 by Call of the Wild. Kingmaker resolves missing
blueprints to null with a warning, but a serialized $type whose assembly is not
loaded aborts the load coroutine, so inside a profile that stages neither mod
the after-load callback never fires and the scenario times out at
load-completion - which is exactly what the two 2026-09-23 runs recorded
(receivers 22<24<26<0<0). The scenario itself is not at fault: it passes on
the same save in the live tree, where those assemblies exist.

The protected KMG_AUTOMATION_BASELINE save predates every foreign mod on this
machine and carries no foreign types at all. This driver never loads or
touches that save. It derives a disposable copy of its bytes with only the
header's Name changed to the working identity, stages the copy under the
working save's own file name for the duration of one compatibility
transaction, runs the mechanical scenario under the profile, and then puts the
original working save back - bytes, last-write time and creation time -
verifying the restored hash. The same lock that makes the profile exclusive
keeps any other guarded run out while the copy is staged.

Everything is recorded: the baseline's hash, the derived fixture's hash and
header, each transaction's outcome and evidence directory, and the restoration
proof, under runtime-evidence\expanded-summoning-compatibility-mechanical.
#>
[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string[]]$ProfileId = @('gunslinger-only', 'gunslinger-high-risk-combined'),
    [ValidateRange(120, 900)][int]$RuntimeTimeoutSeconds = 600,
    [string]$SaveDirectory = (Join-Path (Split-Path -Parent ([Environment]::GetFolderPath('LocalApplicationData'))) 'LocalLow\Owlcat Games\Pathfinder Kingmaker\Saved Games'),
    [string]$RecordRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence\expanded-summoning-compatibility-mechanical',
    [string]$PackagePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$scenario = 'disposable-expanded-summoning'
$workingName = 'KMG_AUTOMATION_WORKING'
$workingFile = 'Manual_299_KMG_AUTOMATION_WORKING.zks'
$baselineFile = 'Manual_298_KMG_AUTOMATION_BASELINE.zks'
$workingPath = Join-Path $SaveDirectory $workingFile
$baselinePath = Join-Path $SaveDirectory $baselineFile
$install = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker'
$liveMod = Join-Path $install 'Mods\KingmakerGunslinger'

function Get-Sha([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }

function Read-SaveHeader([string]$Path) {
    $zip = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entry = $zip.GetEntry('header.json')
        if ($null -eq $entry) { throw "Save has no header.json: $Path" }
        $reader = New-Object IO.StreamReader($entry.Open())
        try { return ($reader.ReadToEnd() | ConvertFrom-Json) } finally { $reader.Dispose() }
    } finally { $zip.Dispose() }
}

function Get-ForeignTypeCounts([string]$Path) {
    # Serialized $type assemblies other than the game's own and the mod under
    # test: the exact thing a staged profile cannot resolve.
    $counts = @{}
    $zip = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        foreach ($entry in $zip.Entries) {
            if (-not $entry.FullName.EndsWith('.json')) { continue }
            $reader = New-Object IO.StreamReader($entry.Open())
            try { $text = $reader.ReadToEnd() } finally { $reader.Dispose() }
            foreach ($match in [regex]::Matches($text, '"\$type"\s*:\s*"([^"]+)"')) {
                $type = $match.Groups[1].Value
                $assembly = if ($type.Contains(',')) { $type.Substring($type.LastIndexOf(',') + 1).Trim() } else { '<none>' }
                if ($assembly -cin @('Assembly-CSharp', 'mscorlib', 'KingmakerGunslinger', 'PublicKeyToken=null', '<none>')) { continue }
                if ($counts.ContainsKey($assembly)) { $counts[$assembly]++ } else { $counts[$assembly] = 1 }
            }
        }
    } finally { $zip.Dispose() }
    return $counts
}

function New-DerivedFixture([string]$Source, [string]$Destination, [string]$Name) {
    Copy-Item -LiteralPath $Source -Destination $Destination -Force
    $archive = [IO.Compression.ZipFile]::Open($Destination, [IO.Compression.ZipArchiveMode]::Update)
    try {
        $entry = $archive.GetEntry('header.json')
        $reader = New-Object IO.StreamReader($entry.Open())
        try { $text = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $header = $text | ConvertFrom-Json
        if ($header.Name -cne 'KMG_AUTOMATION_BASELINE') { throw "Unexpected baseline header name: $($header.Name)" }
        $updated = [regex]::Replace($text, '"Name"\s*:\s*"KMG_AUTOMATION_BASELINE"', '"Name": "' + $Name + '"', 1)
        if ($updated -ceq $text) { throw 'Header name replacement did not apply.' }
        $entry.Delete()
        $entry = $archive.CreateEntry('header.json', [IO.Compression.CompressionLevel]::Optimal)
        $writer = New-Object IO.StreamWriter($entry.Open(), (New-Object Text.UTF8Encoding($false)))
        try { $writer.Write($updated) } finally { $writer.Dispose() }
    } finally { $archive.Dispose() }
    $check = Read-SaveHeader $Destination
    if ($check.Name -cne $Name) { throw 'Derived fixture header did not take the working identity.' }
    return $check
}

Assert-KmgNotRunning
if (-not (Test-Path -LiteralPath $workingPath -PathType Leaf)) { throw "Working save is missing: $workingPath" }
if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) { throw "Protected baseline is missing: $baselinePath" }
$stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ')
$recordDirectory = Join-Path $RecordRoot $stamp
New-Item -ItemType Directory -Path $recordDirectory -Force | Out-Null

$workingItem = Get-Item -LiteralPath $workingPath
$original = [ordered]@{
    path = $workingPath; sha256 = Get-Sha $workingPath; length = $workingItem.Length
    lastWriteTimeUtc = $workingItem.LastWriteTimeUtc.ToString('o'); creationTimeUtc = $workingItem.CreationTimeUtc.ToString('o')
    header = Read-SaveHeader $workingPath; foreignTypes = Get-ForeignTypeCounts $workingPath
}
$baselineItem = Get-Item -LiteralPath $baselinePath
$baseline = [ordered]@{
    path = $baselinePath; sha256 = Get-Sha $baselinePath; length = $baselineItem.Length
    lastWriteTimeUtc = $baselineItem.LastWriteTimeUtc.ToString('o')
    header = Read-SaveHeader $baselinePath; foreignTypes = Get-ForeignTypeCounts $baselinePath
}
if ($baseline.foreignTypes.Count -ne 0) { throw 'The protected baseline carries foreign serialized types; it cannot seed a mod-independent fixture.' }

$backupPath = Join-Path $recordDirectory 'working-save.original.zks'
Copy-Item -LiteralPath $workingPath -Destination $backupPath
if ((Get-Sha $backupPath) -cne $original.sha256) { throw 'Working save backup hash mismatch.' }
$fixturePath = Join-Path $recordDirectory 'compat-fixture.zks'
$fixtureHeader = New-DerivedFixture -Source $baselinePath -Destination $fixturePath -Name $workingName
$fixture = [ordered]@{
    path = $fixturePath; sha256 = Get-Sha $fixturePath; derivedFrom = $baseline.sha256
    header = $fixtureHeader; foreignTypes = Get-ForeignTypeCounts $fixturePath
}
if ($fixture.foreignTypes.Count -ne 0) { throw 'The derived fixture carries foreign serialized types.' }

$record = [ordered]@{
    schemaVersion = 1; scenario = $scenario; profiles = @($ProfileId); runtimeTimeoutSeconds = $RuntimeTimeoutSeconds
    startedAtUtc = [DateTime]::UtcNow.ToString('o'); recordDirectory = $recordDirectory
    originalWorkingSave = $original; protectedBaseline = $baseline; derivedFixture = $fixture
    liveBefore = (Get-KmgTreeFingerprint -Directory $liveMod); runs = @(); failures = 0
}
$recordPath = Join-Path $recordDirectory 'compatibility-mechanical.json'
$record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $recordPath -Encoding UTF8
Write-Host "Original working save $($original.sha256) backed up; fixture $($fixture.sha256) derived from baseline $($baseline.sha256)."

if (-not $PSCmdlet.ShouldProcess($workingPath, "stage the derived fixture for $($ProfileId -join ', ') and restore")) { return }

function Restore-WorkingSave {
    # Bytes, then the timestamps the catalog checks, then the proof.
    Copy-Item -LiteralPath $backupPath -Destination $workingPath -Force
    $item = Get-Item -LiteralPath $workingPath
    $item.LastWriteTimeUtc = [DateTime]::Parse($original.lastWriteTimeUtc, $null, [Globalization.DateTimeStyles]::RoundtripKind)
    $item.CreationTimeUtc = [DateTime]::Parse($original.creationTimeUtc, $null, [Globalization.DateTimeStyles]::RoundtripKind)
    $restoredSha = Get-Sha $workingPath
    $item = Get-Item -LiteralPath $workingPath
    return [ordered]@{
        sha256 = $restoredSha; verified = ($restoredSha -ceq $original.sha256)
        lastWriteTimeUtc = $item.LastWriteTimeUtc.ToString('o'); creationTimeUtc = $item.CreationTimeUtc.ToString('o')
        timestampsRestored = ($item.LastWriteTimeUtc.ToString('o') -ceq $original.lastWriteTimeUtc -and $item.CreationTimeUtc.ToString('o') -ceq $original.creationTimeUtc)
    }
}

foreach ($profile in $ProfileId) {
    $run = [ordered]@{ profileId = $profile; startedAtUtc = [DateTime]::UtcNow.ToString('o') }
    try {
        Assert-KmgNotRunning
        Copy-Item -LiteralPath $fixturePath -Destination $workingPath -Force
        $run.stagedSha256 = Get-Sha $workingPath
        if ($run.stagedSha256 -cne $fixture.sha256) { throw 'Staged fixture hash mismatch.' }
        Write-Host "=== $scenario under $profile on the derived fixture ==="
        $arguments = @{ ProfileId = $profile; Scenario = @($scenario); RuntimeTimeoutSeconds = $RuntimeTimeoutSeconds; Confirm = $false }
        if ($PackagePath) { $arguments.PackagePath = $PackagePath }
        $before = [DateTime]::UtcNow
        try {
            & (Join-Path $PSScriptRoot 'compatibility\Invoke-KingmakerCompatibilityProfile.ps1') @arguments
            $run.launcherOutcome = 'Clean'
        } catch {
            $run.launcherOutcome = 'Unclean'
            $run.error = $_.Exception.Message
            Write-Warning "Profile $profile errored: $($_.Exception.Message)"
        }
        $evidence = Get-ChildItem -LiteralPath 'C:\Dev\KingmakerGunslingerLab\runtime-evidence' -Directory |
            Where-Object { $_.Name -like ('*-' + $scenario) -and $_.CreationTimeUtc -ge $before.AddSeconds(-2) } |
            Sort-Object Name -Descending | Select-Object -First 1
        if ($evidence) {
            $run.evidenceDirectory = $evidence.Name
            $resultPath = Join-Path $evidence.FullName 'runtime-result.json'
            if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
                $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
                $run.scenarioStatus = [string]$result.status
                $run.assertions = @($result.assertions).Count
                $run.assertionsPassed = @($result.assertions | Where-Object { $_.status -ceq 'PASS' }).Count
                $run.gitCommit = [string]$result.gitCommit
            } else { $run.scenarioStatus = 'NO-RESULT' }
        } else { $run.scenarioStatus = 'NO-EVIDENCE-FROM-THIS-RUN' }
        $run.outcome = if ($run.launcherOutcome -ceq 'Clean' -and $run.scenarioStatus -ceq 'PASS') { 'PASS' } else { 'FAIL' }
    } finally {
        $exitDeadline = [DateTime]::UtcNow.AddSeconds(180)
        while (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and [DateTime]::UtcNow -lt $exitDeadline) { Start-Sleep -Milliseconds 500 }
        $run.restoration = Restore-WorkingSave
        $run.completedAtUtc = [DateTime]::UtcNow.ToString('o')
        if ($run.restoration.verified) { Write-Host "Working save restored: $($run.restoration.sha256)" }
        else { Write-Warning "Working save restoration did not verify. Backup retained: $backupPath" }
    }
    if ($run.outcome -cne 'PASS' -or -not $run.restoration.verified) { $record.failures++ }
    $record.runs += $run
    $record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $recordPath -Encoding UTF8
}

$record.liveAfter = Get-KmgTreeFingerprint -Directory $liveMod
$record.liveTreeUnchanged = ($record.liveAfter.Sha256 -ceq $record.liveBefore.Sha256)
$record.completedAtUtc = [DateTime]::UtcNow.ToString('o')
$record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $recordPath -Encoding UTF8
Write-Host "Record: $recordPath"
Write-Host ("Failures: {0}; live tree unchanged: {1}" -f $record.failures, $record.liveTreeUnchanged)
if ($record.failures -ne 0) { exit 1 }
