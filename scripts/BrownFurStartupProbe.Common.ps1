Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'compatibility/CompatibilityProfile.Common.ps1')

function New-KmgBrownFurProbeResolution {
    param([string]$ModsRoot, [string]$BackupRoot, [string]$PackagePath)
    # This mission's actual DATA intake. Never drop an unfamiliar mod to get a
    # green test, and never replace a dependency with an example-directory copy.
    $expected = @('BagOfTricks','BetterVendors','CallOfTheWild','CheatMenu',
        'CraftMagicItems','EddicKingmakerRespec','KingmakerBuffPlanner',
        'KingmakerBugfixes','KingmakerDiceRoller','KingmakerGunslinger',
        'KingmakerLastAzlantiPreserver','ProperFlanking2','RacesUnleashed',
        'SkipIntro','TweakOrTreat','ZFavoredClass')
    $directories = @(Get-ChildItem -LiteralPath $ModsRoot -Directory -Force)
    if ((($directories.Name | Sort-Object) -join '|') -cne (($expected | Sort-Object) -join '|')) {
        throw 'Brown-Fur probe requires the exact sixteen-mod DATA intake; no mod may be removed.'
    }
    $mods = @($directories | ForEach-Object {
        $info = Read-KmgCompatibilityJson (Join-Path $_.FullName 'Info.json')
        if ($info.Id -cne $_.Name -or [string]::IsNullOrWhiteSpace($info.Version)) {
            throw 'Installed mod directory/UMM identity is ambiguous.'
        }
        if ($info.Id -cne 'KingmakerGunslinger') {
            [pscustomobject]@{ destinationName = $_.Name
                sourceDirectory = Join-Path $BackupRoot $_.Name
                sourceManifest = @(Get-KmgCompatibilityDirectoryManifest $_.FullName)
                id = $info.Id; version = $info.Version }
        }
    })
    if (($mods | Where-Object id -ceq 'CallOfTheWild').version -cne '1.14.4c-2.1') {
        throw 'Brown-Fur probe requires the attributed CotW 1.14.4c-2.1 contract.'
    }
    return [pscustomobject]@{ profileId='brown-fur-startup-actual-installed-stack'
        runtimeCapable=$true; gunslinger=[pscustomobject]@{packagePath=$PackagePath}
        runtimeMods=$mods }
}
