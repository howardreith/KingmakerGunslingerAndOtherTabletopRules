[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$root=Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$msbuild=Resolve-KmgMsBuild
$compiler=Join-Path (Split-Path $msbuild -Parent) 'Roslyn/csc.exe'
if(-not(Test-Path -LiteralPath $compiler)){throw 'Existing Roslyn compiler required.'}
$fixture=Join-Path $root ('artifacts/fixtures/s40-factory-'+[Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($fixture)
$paths=[xml](Get-Content -LiteralPath (Join-Path $root 'GamePath.props') -Raw)
$json=Join-Path ([string]$paths.Project.PropertyGroup.KingmakerManagedDir) 'Newtonsoft.Json.dll'
Copy-Item -LiteralPath $json -Destination (Join-Path $fixture 'Newtonsoft.Json.dll')
# The game's JSON build resolves Unity vector types during its own static
# initialization. These local references supply value types, not a Unity host.
foreach($dependency in @('UnityEngine.dll','UnityEngine.CoreModule.dll')) {
 Copy-Item -LiteralPath (Join-Path ([string]$paths.Project.PropertyGroup.KingmakerManagedDir) $dependency) -Destination (Join-Path $fixture $dependency)
}
$executable=Join-Path $fixture 'FactoryBoundary.exe'
$sources=@('tests/fixtures/Sprint40FactoryBoundary.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningBlueprints.Shells.cs',
 'src/KingmakerGunslinger/Blueprints/BlueprintRegistry.cs',
 'src/KingmakerGunslinger/Blueprints/BlueprintManifest.cs',
 'src/KingmakerGunslinger/Blueprints/BlueprintId.cs')|ForEach-Object {Join-Path $root $_}
& $compiler /nologo /target:exe /langversion:latest "/out:$executable" "/reference:$json" $sources
if($LASTEXITCODE -ne 0){throw 'Production factory fixture compilation failed.'}
& $executable $root
if($LASTEXITCODE -ne 0){throw 'Production enchantment factory regression failed.'}
