[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param(
    [Parameter(Mandatory=$true)][string]$PackagePath,
    [Parameter(Mandatory=$true)][string]$ExpectedVersion,
    [Parameter(Mandatory=$true)][ValidateSet('summon-cleanup-iteration2','summon-cleanup-iteration2-baseline')][string]$DevelopmentPurpose,
    [Parameter(Mandatory=$true)][ValidateSet('KMG_AUTOMATION_WORKING')][string]$SaveName,
    [Parameter(Mandatory=$true)][string[]]$CreatureKeys,
    [ValidateRange(30,900)][int]$TimeoutSeconds=900
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
. (Join-Path $PSScriptRoot 'SummonCleanup2CurrentStack.Common.ps1')
$root=Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$plan=New-KmgSummonCleanup2Plan -RepositoryRoot $root -PackagePath $PackagePath -ExpectedVersion $ExpectedVersion `
    -DevelopmentPurpose $DevelopmentPurpose -SaveName $SaveName -CreatureKeys $CreatureKeys -TimeoutSeconds $TimeoutSeconds
if(-not $PSCmdlet.ShouldProcess('actual current Kingmaker Mods/settings and protected save intake',
    'run one bounded Cleanup 2 creature review and restore exact current intake')) {return}
$environment=[pscustomobject]@{install='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker';
    stateRoot='C:/Dev/KingmakerGunslingerLab/compatibility-state';evidenceRoot='C:/Dev/KingmakerGunslingerLab/runtime-evidence';
    saveDirectory=(Join-Path (Split-Path -Parent ([Environment]::GetFolderPath('LocalApplicationData'))) 'LocalLow/Owlcat Games/Pathfinder Kingmaker/Saved Games');
    settingsKey='Software\Owlcat Games\Pathfinder Kingmaker';scriptsDirectory=$PSScriptRoot;fixtureMode=$false}
Invoke-KmgSummonCleanup2Transaction -Plan $plan -Environment $environment
