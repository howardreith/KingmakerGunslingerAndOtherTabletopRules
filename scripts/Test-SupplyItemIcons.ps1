[CmdletBinding()]
param(
    [string]$ReferenceBundleDir = 'C:\Dev\KingmakerGunslingerLab\private\extracted-references\KingmakerGunslinger-private-build-references'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$names = @('lead-ball','black-powder','repair-kit','gunsmith-kit','overhaul-kit')
$hashes = @{}
foreach ($name in $names) {
    $path = Join-Path $root "assets\game\icons\$name.png"
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing supply icon: $name" }
    $image = [Drawing.Image]::FromFile($path)
    try {
        if ($image.Width -ne 128 -or $image.Height -ne 128) {
            throw "Supply icon must be exactly 128x128: $name"
        }
        if (($image.PixelFormat -band [Drawing.Imaging.PixelFormat]::Alpha) -eq 0) {
            throw "Supply icon does not decode with alpha: $name"
        }
    }
    finally { $image.Dispose() }
    $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    if ($hashes.ContainsKey($hash)) { throw "Supply icons are byte-identical: $name and $($hashes[$hash])" }
    $hashes[$hash] = $name
}
foreach ($source in @('gunsmith-kit-chroma-source.png','overhaul-kit-chroma-source.png')) {
    $path = Join-Path $root "assets-source\original-icons\supply-icons\$source"
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing source icon: $source" }
}

$icons = Get-Content -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Blueprints\ProjectAssetIcons.cs') -Raw
foreach ($mapping in @(
    'items.SetIcon(ammunition.LeadBall, Require("lead-ball"))',
    'items.SetIcon(ammunition.BlackPowder, Require("black-powder"))',
    'items.SetIcon(repairKit, Require("repair-kit"))',
    'items.SetIcon(supplies.GunsmithKit, Require("gunsmith-kit"))',
    'items.SetIcon(supplies.OverhaulKit, Require("overhaul-kit"))')) {
    if (-not $icons.Contains($mapping)) { throw "Explicit supply mapping is missing: $mapping" }
}
if ($icons.Contains('HarmonyPatch') -or $icons.Contains('DiamondDust')) {
    throw 'Supply presentation introduced a UI patch or heuristic Diamond Dust mutation.'
}
$bootstrap = Get-Content -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Bootstrap\BlueprintBootstrap.cs') -Raw
if ($bootstrap.IndexOf('ProjectAssetIcons.Apply(', [StringComparison]::Ordinal) -gt
    $bootstrap.IndexOf('CapitalVendorBlueprints.Publish(', [StringComparison]::Ordinal)) {
    throw 'Supply icons are not applied before vendor publication.'
}
$vendor = Get-Content -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Blueprints\CapitalVendorBlueprints.cs') -Raw
$craft = Get-Content -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Blueprints\GunsmithingCraftingBlueprints.cs') -Raw
foreach ($token in @('ammunition.BlackPowder','ammunition.LeadBall','repairKit','gunsmithingSupplies.OverhaulKit','gunsmithingSupplies.GunsmithKit')) {
    if (-not $vendor.Contains($token)) { throw "Vendor does not retain authoritative reference: $token" }
}
foreach ($token in @('ammo.BlackPowder','ammo.LeadBall','tool')) {
    if (-not $craft.Contains($token)) { throw "Crafting does not retain authoritative reference: $token" }
}

$managed = Join-Path $ReferenceBundleDir 'Managed'
$resolver = [ResolveEventHandler]{
    param($sender, $eventArgs)
    $name = ([Reflection.AssemblyName]$eventArgs.Name).Name + '.dll'
    $candidate = Get-ChildItem -LiteralPath $managed -Filter $name -Recurse -File | Select-Object -First 1
    if ($candidate) { return [Reflection.Assembly]::LoadFrom($candidate.FullName) }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try {
    $game = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'Assembly-CSharp.dll'))
    $itemType = $game.GetType('Kingmaker.Blueprints.Items.BlueprintItem', $true)
    $spriteType = [Type]::GetType('UnityEngine.Sprite, UnityEngine.CoreModule', $true)
    $field = $itemType.GetField('m_Icon', [Reflection.BindingFlags]'Instance,NonPublic')
    $property = $itemType.GetProperty('Icon', [Reflection.BindingFlags]'Instance,Public')
    if (-not $field -or $field.FieldType -ne $spriteType -or -not $property -or
        $property.PropertyType -ne $spriteType) {
        throw 'Installed BlueprintItem m_Icon/Icon contract did not resolve exactly.'
    }
    $item = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($itemType)
    $sprite = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($spriteType)
    $field.SetValue($item, $sprite)
    if (-not [Object]::ReferenceEquals($property.GetValue($item, $null), $sprite)) {
        throw 'Installed BlueprintItem.Icon did not return the exact assigned Sprite.'
    }
}
finally { [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver) }

Write-Host 'Focused supply-icon and exact-reference BlueprintItem icon tests passed.'
