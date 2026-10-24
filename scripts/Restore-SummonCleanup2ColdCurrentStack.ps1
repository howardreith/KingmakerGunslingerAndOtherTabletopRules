[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param([Parameter(Mandatory=$true)][string]$RunId,[Parameter(Mandatory=$true)][string]$ExpectedOwnershipSha256)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
. (Join-Path $PSScriptRoot 'SummonCleanup2CurrentStack.Common.ps1')
. (Join-Path $PSScriptRoot 'SummonCleanup2ColdCurrentStack.Common.ps1')
Assert-KmgSummonCleanup2NativeExit
if(-not $PSCmdlet.ShouldProcess($RunId,'verify exact cold ownership and restore original Working/current intake after confirmed game and original-owner exit')){return}
Restore-KmgCleanup2ColdCurrentStackTransaction $RunId $ExpectedOwnershipSha256 (Get-KmgCleanup2ColdEnvironment)
