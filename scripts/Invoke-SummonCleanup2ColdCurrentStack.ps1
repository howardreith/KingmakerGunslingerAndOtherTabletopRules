[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param(
    [Parameter(Mandatory=$true)][string]$PackagePath,
    [Parameter(Mandatory=$true)][ValidateSet('0.0.163')][string]$ExpectedVersion,
    [Parameter(Mandatory=$true)][ValidateSet('summon-cleanup-iteration2')][string]$DevelopmentPurpose,
    [Parameter(Mandatory=$true)][ValidateSet('KMG_AUTOMATION_WORKING')][string]$SaveName,
    [Parameter(Mandatory=$true)][ValidateSet('giant-humanoids-ii')][string]$PersistenceScope,
    [ValidateRange(30,900)][int]$TimeoutSeconds=600
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
. (Join-Path $PSScriptRoot 'SummonCleanup2CurrentStack.Common.ps1')
. (Join-Path $PSScriptRoot 'SummonCleanup2ColdCurrentStack.Common.ps1')
$plan=New-KmgCleanup2ColdCurrentStackPlan -RepositoryRoot (Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot) `
    -PackagePath $PackagePath -ExpectedVersion $ExpectedVersion -DevelopmentPurpose $DevelopmentPurpose -SaveName $SaveName `
    -PersistenceScope $PersistenceScope -TimeoutSeconds $TimeoutSeconds
if(-not $PSCmdlet.ShouldProcess('exact current identifiable mod intake and original KMG_AUTOMATION_WORKING',
    'retain one transaction across three fresh native cold phases, then restore original Working bytes/metadata and current installation/settings')) {return}
Invoke-KmgCleanup2ColdCurrentStackTransaction $plan (Get-KmgCleanup2ColdEnvironment)
