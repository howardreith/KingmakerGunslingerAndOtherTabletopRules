[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param([Parameter(Mandatory = $true)][ValidatePattern('^runtime-[A-Za-z0-9-]+$')][string]$RunId)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'MagicCircleSettingsTransaction.Common.ps1')
Resume-KmgMagicCircleSettingsTransaction -RunId $RunId `
    -StateRoot 'C:/Dev/KingmakerGunslingerLab/compatibility-state' `
    -ExpectedSettingsPath 'C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker/Mods/KingmakerGunslinger/FeatureModules.json' `
    -WhatIf:$WhatIfPreference -Confirm:$false
