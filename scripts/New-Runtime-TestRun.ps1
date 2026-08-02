[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Scenario,
    [string]$ExpectedVersion = '0.0.56',
    [string]$EvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence',
    [string]$RunName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$requiredEvidenceRoot = [IO.Path]::GetFullPath('C:\Dev\KingmakerGunslingerLab\runtime-evidence').TrimEnd('\')
if (-not [IO.Path]::GetFullPath($EvidenceRoot).TrimEnd('\').Equals($requiredEvidenceRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Evidence root must be exactly: $requiredEvidenceRoot"
}
$git = Get-KmgGitState -RepositoryRoot $root
$safeScenario = ($Scenario -replace '[^A-Za-z0-9._-]+', '-').Trim('-')
if (-not $safeScenario) { throw 'Scenario does not contain a safe directory-name character.' }
if (-not $RunName) {
    $RunName = '{0}-{1}-{2}' -f [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ'), $safeScenario, $git.Commit.Substring(0, 12)
}
$directory = Assert-KmgPathWithin -Path (Join-Path $EvidenceRoot $RunName) -Root $EvidenceRoot
if (Test-Path -LiteralPath $directory) { throw "Runtime test-run collision: $directory" }
New-Item -ItemType Directory -Path $directory | Out-Null
$manifest = [ordered]@{
    schemaVersion = 1
    scenario = $Scenario
    commit = $git.Commit
    branch = $git.Branch
    expectedModVersion = $ExpectedVersion
    startedAtUtc = [DateTime]::UtcNow.ToString('o')
    status = 'CREATED'
}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $directory 'test-run.json') -Encoding UTF8
Write-Output $directory
