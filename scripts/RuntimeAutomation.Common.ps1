Set-StrictMode -Version Latest

$script:KmgRuntimeEvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
$script:KmgRuntimeScenarios = @('mod-load-smoke', 'observe-manual-save-load')

function Assert-KmgRuntimeEvidenceDirectory {
    param([Parameter(Mandatory = $true)][string]$Path)
    $root = [IO.Path]::GetFullPath($script:KmgRuntimeEvidenceRoot).TrimEnd('\')
    $full = [IO.Path]::GetFullPath($Path).TrimEnd('\')
    if (-not $full.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Runtime evidence directory must be beneath $root"
    }
    return $full
}

function New-KmgRuntimeRequest {
    param(
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds,
        [Parameter(Mandatory = $true)][bool]$ExitAfterCompletion,
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [hashtable]$Parameters = @{}
    )
    if ($Scenario -notin $script:KmgRuntimeScenarios) {
        throw "Scenario is not allowlisted: $Scenario"
    }
    if ([string]::IsNullOrWhiteSpace($ExpectedVersion)) {
        throw 'ExpectedVersion is required.'
    }
    if ($TimeoutSeconds -lt 5 -or $TimeoutSeconds -gt 1800) {
        throw 'TimeoutSeconds must be from 5 through 1800.'
    }
    if ($Parameters.Count -ne 0) {
        throw "Scenario '$Scenario' does not accept parameters."
    }
    $evidence = Assert-KmgRuntimeEvidenceDirectory -Path $EvidenceDirectory
    $runId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '-' +
        [Guid]::NewGuid().ToString('N')
    return [ordered]@{
        schemaVersion = 1
        enabled = $true
        runId = $runId
        scenario = $Scenario
        expectedModVersion = $ExpectedVersion
        evidenceDirectory = $evidence
        timeoutSeconds = $TimeoutSeconds
        exitAfterCompletion = $ExitAfterCompletion
        parameters = [ordered]@{}
    }
}

function Write-KmgUtf8NoBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Content
    )
    [IO.File]::WriteAllText($Path, $Content, [Text.UTF8Encoding]::new($false))
}
