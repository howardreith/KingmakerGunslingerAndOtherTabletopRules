[CmdletBinding()]
param([Parameter(Mandatory = $true)][string]$ModDllPath)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop') {
    throw 'Run this isolated .NET Framework check with powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-FirearmInputControlFlow.ps1 -ModDllPath <candidate DLL>.'
}
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
$hotfixRepository = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$hotfixDll = Assert-KmgPathWithin -Path $ModDllPath -Root (Join-Path $hotfixRepository 'artifacts')
if (-not (Test-Path -LiteralPath $hotfixDll -PathType Leaf) -or [IO.Path]::GetFileName($hotfixDll) -cne 'KingmakerGunslinger.dll') {
    throw 'An existing candidate KingmakerGunslinger.dll under repository artifacts is required.'
}
$hotfixManaged = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Kingmaker_Data\Managed'
$hotfixOutput = Split-Path -Parent $hotfixDll
$hotfixResolve = [ResolveEventHandler] {
    param($sender, $event)
    $name = ([Reflection.AssemblyName]$event.Name).Name + '.dll'
    foreach ($directory in @($hotfixManaged, (Join-Path $hotfixManaged 'UnityModManager'), $hotfixOutput)) {
        $path = Join-Path $directory $name
        if (Test-Path -LiteralPath $path) { return [Reflection.Assembly]::LoadFrom($path) }
    }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($hotfixResolve)
try {
    $hotfixHarmony = Join-Path $hotfixManaged 'UnityModManager\0Harmony12.dll'
    [void][Reflection.Assembly]::LoadFrom($hotfixHarmony)
    Add-Type -Path (Join-Path $hotfixRepository 'tests\KingmakerGunslinger.NativeContractTests\FirearmInputControlFlowTests.cs') -ReferencedAssemblies @($hotfixHarmony, 'System.Core.dll')
    $hotfixAssembly = [Reflection.Assembly]::LoadFrom($hotfixDll)
    Write-Output ('Candidate DLL version: ' + $hotfixAssembly.GetName().Version)
    Write-Output ('Candidate DLL SHA-256: ' + (Get-FileHash -LiteralPath $hotfixDll -Algorithm SHA256).Hash.ToLowerInvariant())
    [FirearmInputControlFlowTests]::Run($hotfixAssembly)
} finally {
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($hotfixResolve)
}
