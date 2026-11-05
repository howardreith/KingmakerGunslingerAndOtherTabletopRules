Set-StrictMode -Version Latest

function Assert-KmgSprint40DiagnosticRecoveryRequest {
 param([string[]]$Scenarios,[string]$Purpose,[string]$SaveName,[bool]$ModuleDisabled,
       [hashtable]$Parameters,[hashtable]$OriginalParameters)
 if($Scenarios.Count -ne 1 -or $Scenarios[0] -cne 'observe-feature-module-settings' -or
   $Purpose -cne 'sprint40-mechanics' -or $SaveName -or $ModuleDisabled -or
   $OriginalParameters.Count -eq 0 -or $Parameters.Count -ne $OriginalParameters.Count){throw 'Sprint40 recovery requires only the original-configuration no-save settings observation.'}
 foreach($key in $OriginalParameters.Keys){
  if(@($Parameters.Keys) -cnotcontains $key -or $Parameters[$key] -isnot [bool] -or
    $Parameters[$key] -ne $OriginalParameters[$key]){throw 'Recovery module parameters differ from actual original configuration.'}
 }
}

# Observed startup writers on HOWARD-LAPTOP. This is a fixed file boundary,
# not an exclusion from the original intake or a caller-supplied path list.
function Get-KmgSprint40StartupFiles {
 @('BagOfTricks\Favourites\Functions.xml','BagOfTricks\Settings.xml',
 'CallOfTheWild\loaded_blueprints.txt','CraftMagicItems\Settings.xml',
 'KingmakerDiceRoller\Settings.xml','KingmakerLastAzlantiPreserver\Settings.xml',
 'RacesUnleashed\Settings.json','TweakOrTreat\loaded_blueprints.txt',
 'ZFavoredClass\loaded_blueprints.txt')
}

function Backup-KmgSprint40StartupFiles {
 param([Parameter(Mandatory=$true)][string]$ModsRoot,
       [Parameter(Mandatory=$true)][string]$EvidenceDirectory)
 $mods=(Resolve-Path -LiteralPath $ModsRoot).Path
 $evidence=[IO.Path]::GetFullPath($EvidenceDirectory)
 if(Test-Path -LiteralPath $evidence){throw 'Startup file evidence must be new.'}
 [void][IO.Directory]::CreateDirectory($evidence)
 $rows=@(foreach($relative in Get-KmgSprint40StartupFiles){
  $source=Join-Path $mods $relative
  $item=Get-Item -LiteralPath $source -Force
  if($item.PSIsContainer -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Startup file is not an ordinary file.'}
  $hash=Get-KmgCompatibilitySha256 $source
  $target=Join-Path $evidence $relative
  [void][IO.Directory]::CreateDirectory((Split-Path $target -Parent))
  Copy-Item -LiteralPath $source -Destination $target
  if((Get-KmgCompatibilitySha256 $target) -cne $hash -or (Get-KmgCompatibilitySha256 $source) -cne $hash){throw 'Startup file changed during byte backup.'}
  [pscustomobject]@{path=$relative;sha256=$hash;length=$item.Length;
   creationUtcTicks=$item.CreationTimeUtc.Ticks;writeUtcTicks=$item.LastWriteTimeUtc.Ticks;attributes=[int]$item.Attributes}
 })
 $receipt=Join-Path $evidence 'startup-peer-snapshot.json'
 Write-KmgCompatibilityJsonAtomic $receipt ([ordered]@{schemaVersion=1;modsRoot=$mods;files=$rows})
 [pscustomobject]@{receiptPath=$receipt;sha256=(Get-KmgCompatibilitySha256 $receipt)}
}

function Restore-KmgSprint40StartupFiles {
 param([Parameter(Mandatory=$true)]$Snapshot,
       [string]$RegeneratedDiagnosticReceipt)
 if((Get-KmgCompatibilitySha256 $Snapshot.receiptPath) -cne $Snapshot.sha256){throw 'Startup snapshot receipt changed.'}
 $saved=Read-KmgCompatibilityJson $Snapshot.receiptPath
 $paths=@(Get-KmgSprint40StartupFiles)
 if($saved.schemaVersion -ne 1 -or @($saved.files).Count -ne $paths.Count -or
   (@($saved.files.path) -join '|') -cne ($paths -join '|')){throw 'Startup snapshot has missing/foreign file identities.'}
 $backupRoot=Split-Path $Snapshot.receiptPath -Parent
 # Validate every backup before the first restoration mutation.
 foreach($row in $saved.files){
  $backup=Join-Path $backupRoot $row.path
  if((Get-KmgCompatibilitySha256 $backup) -cne $row.sha256 -or (Get-Item -LiteralPath $backup).Length -ne $row.length){throw 'Startup backup bytes differ.'}
 }
 $preserve=$null
 if($RegeneratedDiagnosticReceipt){
  $native=Read-KmgCompatibilityJson $RegeneratedDiagnosticReceipt
  $expected=Join-Path $saved.modsRoot 'ZFavoredClass\loaded_blueprints.txt'
  if($native.scenario -cne 'observe-feature-module-settings' -or $native.allNativeRowsExact -ne $true -or
    $native.expectedNativeRows -le 0 -or $native.actualRows -ne $native.expectedNativeRows -or
    -not [IO.Path]::GetFullPath($native.path).Equals([IO.Path]::GetFullPath($expected),[StringComparison]::OrdinalIgnoreCase) -or
    (Get-KmgCompatibilitySha256 $expected) -cne $native.sha256 -or
    (Get-Item -LiteralPath $expected).Length -ne $native.length){throw 'Regenerated diagnostic lacks exact current native producer evidence.'}
  $preserve='ZFavoredClass\loaded_blueprints.txt'
 }
 $restored=@()
 foreach($row in $saved.files){
  if($row.path -ceq $preserve){continue}
  $target=Join-Path $saved.modsRoot $row.path
  $current=Get-Item -LiteralPath $target -Force
  if($current.PSIsContainer -or ($current.Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Startup restoration target is not an ordinary file.'}
  $currentHash=Get-KmgCompatibilitySha256 $target
  if($currentHash -cne $row.sha256){
   Write-KmgRuntimeAtomicBytes -Path $target -Bytes ([IO.File]::ReadAllBytes((Join-Path $backupRoot $row.path))) -ExpectedSha256 $currentHash
  }
  [IO.File]::SetCreationTimeUtc($target,[DateTime]::new([long]$row.creationUtcTicks,[DateTimeKind]::Utc))
  [IO.File]::SetLastWriteTimeUtc($target,[DateTime]::new([long]$row.writeUtcTicks,[DateTimeKind]::Utc))
  [IO.File]::SetAttributes($target,[IO.FileAttributes][int]$row.attributes)
  $after=Get-Item -LiteralPath $target -Force
  if((Get-KmgCompatibilitySha256 $target) -cne $row.sha256 -or $after.Length -ne $row.length -or
   $after.CreationTimeUtc.Ticks -ne $row.creationUtcTicks -or $after.LastWriteTimeUtc.Ticks -ne $row.writeUtcTicks -or
   [int]$after.Attributes -ne $row.attributes){throw 'Startup file exact restoration failed.'}
  $restored+=$row.path
 }
 [pscustomobject]@{restored=$restored;preservedNativeDiagnostic=$preserve;
  disposition=$(if($preserve){'NATIVE_REGENERATED_PENDING_DISPOSITION'}else{'EXACT_PRE_TRANSACTION_BYTES_AND_METADATA'})}
}
