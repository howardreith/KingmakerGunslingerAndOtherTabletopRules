<# Executes the installed game/CotW methods and the compiled production
transpiler headlessly. Does NOT prove a saved campaign or native UI load. #>
[CmdletBinding()]
param(
    [string]$KingmakerInstallDir='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker',
    [string]$ModAssemblyPath
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if(-not $ModAssemblyPath){$ModAssemblyPath=Join-Path $root 'artifacts/bin/Release/KingmakerGunslinger/KingmakerGunslinger.dll'}
$managed=Join-Path $KingmakerInstallDir 'Kingmaker_Data/Managed'
$cotwPath=Join-Path $KingmakerInstallDir 'Mods/CallOfTheWild/CallOfTheWild.dll'
$resolver=[ResolveEventHandler]{param($sender,$eventArgs)
    $file=([Reflection.AssemblyName]$eventArgs.Name).Name+'.dll'
    foreach($directory in @($managed,(Join-Path $managed 'UnityModManager'),(Split-Path -Parent $cotwPath))) {
        $path=Join-Path $directory $file
        if(Test-Path -LiteralPath $path){return [Reflection.Assembly]::LoadFrom($path)}
    }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try {
    $refs=@((Join-Path $managed 'Assembly-CSharp.dll'),(Join-Path $managed 'UnityEngine.CoreModule.dll'),(Join-Path $managed 'UnityEngine.UI.dll'),
        (Join-Path $managed 'UnityModManager/0Harmony12.dll'),$cotwPath)
    foreach($path in $refs){[void][Reflection.Assembly]::LoadFrom($path)}
    $umm=[Reflection.Assembly]::LoadFrom((Join-Path $managed 'UnityModManager/UnityModManager.dll'))
    # CotW's static settings constructor only reads settings.json. Supply the
    # normal installed path in this isolated process; never write that file.
    $umm.GetType('UnityModManagerNet.UnityModManager').GetField('<modsPath>k__BackingField',
        [Reflection.BindingFlags]'Static,NonPublic').SetValue($null,(Join-Path $KingmakerInstallDir 'Mods'))
    $mod=[Reflection.Assembly]::LoadFrom($ModAssemblyPath)
    Add-Type -Path (Join-Path $root 'tests/CotwSpontaneousSlotProbe.cs') -ReferencedAssemblies ($refs+@('System.Core'))
    try {[CotwSpontaneousSlotProbe]::Run($mod)} catch {Write-Output $_.Exception.ToString(); throw}
} finally {[AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver)}
