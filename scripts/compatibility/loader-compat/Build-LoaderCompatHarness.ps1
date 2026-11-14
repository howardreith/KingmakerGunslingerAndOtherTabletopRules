param(
    [Parameter(Mandatory = $true)][string]$SourceDir,
    [Parameter(Mandatory = $true)][string]$OutputDir,
    [Parameter(Mandatory = $true)][string]$LoaderReferenceDir,
    [string]$ManagedDir = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Kingmaker_Data\Managed'
)
$ErrorActionPreference = 'Stop'
$dotnet = (Get-Command dotnet).Source
$csc = Get-ChildItem (Join-Path (Split-Path $dotnet) 'sdk') -Filter csc.dll -Recurse -File |
    Where-Object { $_.FullName -like '*\Roslyn\bincore\csc.dll' } | Sort-Object FullName -Descending | Select-Object -First 1
$net47 = 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7'
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$refs = @("$net47\mscorlib.dll", "$net47\System.dll", "$net47\System.Core.dll") +
    @('Assembly-CSharp', 'Assembly-CSharp-firstpass', 'Newtonsoft.Json', 'UnityEngine', 'UnityEngine.CoreModule', 'UnityEngine.IMGUIModule', 'UniRx') |
    ForEach-Object { if ($_ -like '*\*') { $_ } else { Join-Path $ManagedDir "$_.dll" } }
$refs += @((Join-Path $LoaderReferenceDir 'UnityModManager\UnityModManager.dll'), (Join-Path $LoaderReferenceDir 'UnityModManager\0Harmony12.dll'))
$args = @('/noconfig', '/nostdlib+', '/langversion:7.3', '/target:library', '/deterministic+', '/optimize+', '/nowarn:CS1701,CS1702',
    "/out:$(Join-Path $OutputDir 'KmgLoaderCompatHarness.dll')") + ($refs | ForEach-Object { "/reference:$_" }) +
    @(Get-ChildItem $SourceDir -Filter *.cs | ForEach-Object FullName)
$env:DOTNET_ROLL_FORWARD = 'Major'
& $dotnet $csc.FullName @args
if ($LASTEXITCODE -ne 0) { throw "harness compile failed: $LASTEXITCODE" }
