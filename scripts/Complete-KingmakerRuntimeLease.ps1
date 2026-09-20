[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param([Parameter(Mandatory=$true)][ValidatePattern('^runtime-[A-Za-z0-9-]+$')][string]$RunId)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeCoordination.Common.ps1')
Complete-KmgGenericRuntimeLease -RunId $RunId -WhatIf:$WhatIfPreference
