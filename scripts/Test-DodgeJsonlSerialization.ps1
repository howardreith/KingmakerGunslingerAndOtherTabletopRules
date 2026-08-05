[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ModAssemblyPath,[string]$ReferenceBundleDir='C:\Dev\KingmakerGunslingerLab\private\extracted-references\KingmakerGunslinger-private-build-references',[string]$KingmakerInstallDir='C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker')
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$managed=Join-Path $ReferenceBundleDir 'Managed'
$resolver=[ResolveEventHandler]{param($s,$e)$name=([Reflection.AssemblyName]$e.Name).Name+'.dll';$c=Get-ChildItem $managed -Filter $name -Recurse -File|Select-Object -First 1;if(-not $c){$c=Get-ChildItem (Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed') -Filter $name -Recurse -File|Select-Object -First 1};if($c){return [Reflection.Assembly]::LoadFrom($c.FullName)};$null}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try{
    $newtonsoft=[Reflection.Assembly]::LoadFrom((Join-Path $managed 'Newtonsoft.Json.dll'))
    $mod=[Reflection.Assembly]::LoadFrom((Resolve-Path $ModAssemblyPath).Path)
    $recordType=$mod.GetType('KingmakerGunslinger.Diagnostics.DodgeBuffLifecycleRecord',$true)
    $forensics=$mod.GetType('KingmakerGunslinger.Diagnostics.DodgeBuffLifecycleForensics',$true)
    $settingsField=$forensics.GetField('JsonLineSettings',[Reflection.BindingFlags]'Static,NonPublic')
    $settings=$settingsField.GetValue($null)
    if($settings.PreserveReferencesHandling.ToString() -ne 'None' -or $settings.ReferenceLoopHandling.ToString() -ne 'Error'){throw 'Independent JSONL settings are incorrect.'}
    $json=$newtonsoft.GetType('Newtonsoft.Json.JsonConvert')
    $formatting=[Enum]::Parse($newtonsoft.GetType('Newtonsoft.Json.Formatting'),'None')
    $serialize=$json.GetMethods()|Where-Object{$_.Name-eq'SerializeObject'-and$_.GetParameters().Count-eq3-and$_.GetParameters()[1].ParameterType.Name-eq'Formatting'-and$_.GetParameters()[2].ParameterType.Name-eq'JsonSerializerSettings'}|Select-Object -First 1
    $lines=@()
    foreach($i in 1..3){$r=[Activator]::CreateInstance($recordType,$true);$recordType.GetField('sequence').SetValue($r,[long]$i);$recordType.GetField('eventName').SetValue($r,"sample-$i");$recordType.GetField('utcTimestamp').SetValue($r,'2026-08-05T00:00:00Z');$recordType.GetField('buffRuntimeReferenceId').SetValue($r,'SAME-BUFF');$lines+=$serialize.Invoke($null,@($r,$formatting,$settings))}
    foreach($line in $lines){$o=$line|ConvertFrom-Json;if($null-eq$o.sequence-or$null-eq$o.eventName-or$null-eq$o.utcTimestamp){throw 'Serialized line lacks required identity fields.'};if($line-match '"\$(id|ref|values)"'){throw 'Serialized line contains reference metadata.'}}
    if(($lines|Select-Object -Unique).Count-ne3){throw 'Independent event changes were not preserved.'}
    if($lines[0]-match 'sample-3'){throw 'Later mutation changed an earlier line.'}
    Write-Host 'Production Dodge JSONL serializer passed: 3 independent records, repeated Buff identity, no reference metadata.'
}finally{[AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver)}
