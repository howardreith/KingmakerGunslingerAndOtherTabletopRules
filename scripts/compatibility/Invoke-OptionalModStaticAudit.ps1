[CmdletBinding()]
param(
    [string]$ReferenceRoot = 'C:\Dev\KingmakerGunslingerLab\examples',
    [string]$OutputPath
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$python = (Get-Command python -ErrorAction Stop).Source
$args = @((Join-Path $root 'tools\compatibility\scan_optional_mod_sources.py'), '--root', $root, '--reference-root', $ReferenceRoot)
if ($OutputPath) { $args += @('--output', $OutputPath) }
& $python @args
if ($LASTEXITCODE -ne 0) { throw "Optional-mod static audit failed with exit code $LASTEXITCODE." }
