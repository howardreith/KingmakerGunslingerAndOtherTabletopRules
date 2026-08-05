[CmdletBinding()]
param([string]$OutputPath)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path $PSScriptRoot -Parent
$defaultOutputPath = Join-Path $repositoryRoot `
    'artifacts\manual-test\dodge-forensics\Install-DodgeForensics.ps1'
if ([string]::IsNullOrWhiteSpace($OutputPath)) { $OutputPath = $defaultOutputPath }
$canonicalPath = Join-Path $repositoryRoot `
    'artifacts\manual-test\dodge-forensics\Install-DodgeForensics.ps1'
if (-not (Test-Path -LiteralPath $canonicalPath -PathType Leaf)) {
    throw "Canonical Dodge-forensics installer is missing: $canonicalPath"
}
$content = Get-Content -LiteralPath $canonicalPath -Raw
if ($content.Contains('-Single')) {
    throw 'Canonical Dodge-forensics installer contains the invalid -Single token.'
}
$directory = Split-Path $OutputPath -Parent
if (-not (Test-Path -LiteralPath $directory)) {
    New-Item -LiteralPath $directory -ItemType Directory -Force | Out-Null
}
[IO.File]::WriteAllText($OutputPath, $content, [Text.UTF8Encoding]::new($false))
Write-Host "Generated Dodge-forensics installer: $OutputPath"
