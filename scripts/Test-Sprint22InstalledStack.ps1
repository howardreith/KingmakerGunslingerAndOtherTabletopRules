[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
. (Join-Path $PSScriptRoot 'compatibility/CompatibilityProfile.Common.ps1')
$checks=0
function Assert-Stack([bool]$Condition,[string]$Predicate) {
    if(-not $Condition) { throw "Sprint22 installed-stack predicate failed: $Predicate" }
    $script:checks++
}
function Assert-StackThrows([scriptblock]$Action,[string]$Pattern) {
    $errorRecord=$null
    try { & $Action | Out-Null } catch { $errorRecord=$_ }
    Assert-Stack ($null -ne $errorRecord -and $errorRecord.Exception.Message -like "*$Pattern*") $Pattern
}
$repo=Get-KmgCompatibilityRepositoryRoot
$fixture=Join-Path $repo ('artifacts/compatibility/tests/sprint22-stack-'+[Guid]::NewGuid().ToString('N'))
$install=Join-Path $fixture 'Pathfinder Kingmaker'
$mods=Join-Path $install 'Mods'
$state=Join-Path $fixture 'state'
$runId='installed-stack-fixture'
$backup=Join-Path $install "Mods.kmg-compat-$runId.original"
[void][IO.Directory]::CreateDirectory((Join-Path $install 'Kingmaker_Data/Managed'))
[IO.File]::WriteAllText((Join-Path $install 'Kingmaker.exe'),'fixture only')
[IO.File]::WriteAllText((Join-Path $install 'Kingmaker_Data/Managed/Assembly-CSharp.dll'),'fixture only')
foreach($name in @('KingmakerGunslinger')+(1..10 | ForEach-Object { 'CurrentMod'+$_ })) {
    $directory=Join-Path $mods $name
    [void][IO.Directory]::CreateDirectory($directory)
    [IO.File]::WriteAllText((Join-Path $directory 'Info.json'),('{"Id":"'+$name+'","Version":"fixture"}'))
    [IO.File]::WriteAllText((Join-Path $directory 'settings.json'),('{"identity":"'+$name+'"}'))
}
$bank=Join-Path $install 'Kingmaker_Data/StreamingAssets/Audio/GeneratedSoundBanks/Windows/KMG_Firearms.bnk'
[void][IO.Directory]::CreateDirectory((Split-Path -Parent $bank))
[IO.File]::WriteAllText($bank,'original fixture bank')
$original=@(Get-KmgCompatibilityDirectoryManifest $mods)
$bankHash=Get-KmgCompatibilitySha256 $bank
$packageSource=Join-Path $fixture 'candidate/KingmakerGunslinger'
[void][IO.Directory]::CreateDirectory($packageSource)
[IO.File]::WriteAllText((Join-Path $packageSource 'Info.json'),'{"Id":"KingmakerGunslinger","Version":"0.0.151"}')
[IO.File]::WriteAllText((Join-Path $packageSource 'KingmakerGunslinger.dll'),'fixture candidate')
$package=Join-Path $fixture 'candidate.zip'
Compress-Archive -LiteralPath $packageSource -DestinationPath $package
$resolution=[pscustomobject]@{profileId='sprint22-installed-fixture';runtimeCapable=$true;
    gunslinger=[pscustomobject]@{packagePath=$package};runtimeMods=@(Get-ChildItem -LiteralPath $mods -Directory |
        Where-Object {$_.Name -cne 'KingmakerGunslinger'} | ForEach-Object {
            [pscustomobject]@{destinationName=$_.Name;sourceDirectory=(Join-Path $backup $_.Name);
                sourceManifest=@(Get-KmgCompatibilityDirectoryManifest $_.FullName)}
        })}
Assert-Stack ($resolution.runtimeMods.Count -eq 10 -and -not(Test-Path -LiteralPath $backup)) 'ten sources point into the not-yet-created owned original backup'
$entered=Enter-KmgCompatibilityTransaction -Resolution $resolution -KingmakerInstallDir $install -StateRoot $state -RunId $runId -FixtureMode -KnownKingmakerProcessIds @()
Assert-Stack ($entered.status -ceq 'Active') 'current stack transaction entered'
foreach($mod in $resolution.runtimeMods) {
    Assert-Stack (Test-KmgCompatibilityManifestEqual $mod.sourceManifest @(Get-KmgCompatibilityDirectoryManifest (Join-Path $mods $mod.destinationName))) ('copied current bytes: '+$mod.destinationName)
}
[IO.File]::WriteAllText($bank,'staged fixture bank')
$boundary=Invoke-KmgIndependentBoundaryRestoration -RestoreSettings { throw 'simulated native settings restoration failure' } -RestoreInstallation {
    Restore-KmgCompatibilityTransaction -RunId $runId -StateRoot $state -FixtureMode -KnownKingmakerProcessIds @()
}
Assert-Stack ($boundary.settingsError.Exception.Message -ceq 'simulated native settings restoration failure') 'settings error retained'
Assert-Stack ($null -eq $boundary.installationError -and $boundary.transaction.restorationVerified) 'installation restoration still attempted and verified after settings error'
Assert-Stack (Test-KmgCompatibilityManifestEqual $original @(Get-KmgCompatibilityDirectoryManifest $mods)) 'all eleven original directories restored exactly'
Assert-Stack ((Get-KmgCompatibilitySha256 $bank) -ceq $bankHash) 'sound bank restored exactly'
$boundary=Invoke-KmgIndependentBoundaryRestoration -RestoreSettings { throw 'settings failed' } -RestoreInstallation { throw 'installation failed' }
Assert-Stack ($boundary.settingsError.Exception.Message -ceq 'settings failed' -and $boundary.installationError.Exception.Message -ceq 'installation failed') 'both independent errors retained'
$boundary=Invoke-KmgIndependentBoundaryRestoration -RestoreSettings {} -RestoreInstallation { [pscustomobject]@{restorationVerified=$true} }
Assert-Stack ($null -eq $boundary.settingsError -and $null -eq $boundary.installationError -and $boundary.transaction.restorationVerified) 'successful restoration result retained'
$exact=[pscustomobject]@{kind='KmgRuntimeLaunchReceipt';runId='exact-run';evidenceDirectory='C:/fixture-evidence/exact-run'}
Assert-Stack ((Get-KmgExactRuntimeLaunchReceipt -Output @('unrelated output',$exact)).runId -ceq 'exact-run') 'exact receipt selected independently of timestamps'
Assert-StackThrows { Get-KmgExactRuntimeLaunchReceipt -Output @() } 'missing or duplicated'
Assert-StackThrows { Get-KmgExactRuntimeLaunchReceipt -Output @($exact,$exact) } 'missing or duplicated'
Assert-StackThrows { Get-KmgExactRuntimeLaunchReceipt -Output @([pscustomobject]@{kind='KmgRuntimeLaunchReceipt';runId='wrong-run';evidenceDirectory='C:/fixture-evidence/exact-run'}) } 'exact RunId directory'
$fakeKey=[pscustomobject]@{values=@{binary=[byte[]]@(1,2,3);text='original';number=7};
    kinds=@{binary=[Microsoft.Win32.RegistryValueKind]::Binary;text=[Microsoft.Win32.RegistryValueKind]::String;number=[Microsoft.Win32.RegistryValueKind]::DWord};subkeys=[string[]]@()}
$fakeKey | Add-Member ScriptMethod GetSubKeyNames { return ,$this.subkeys }
$fakeKey | Add-Member ScriptMethod GetValueNames { return ,([string[]]@($this.values.Keys)) }
$fakeKey | Add-Member ScriptMethod GetValueKind { param($name);return $this.kinds[$name] }
$fakeKey | Add-Member ScriptMethod GetValue { param($name,$default,$options);return $this.values[$name] }
$fakeKey | Add-Member ScriptMethod SetValue { param($name,$value,$kind);$this.values[$name]=$value;$this.kinds[$name]=$kind }
$fakeKey | Add-Member ScriptMethod DeleteValue { param($name,$required);$this.values.Remove($name);$this.kinds.Remove($name) }
$settings=@(Get-KmgFlatGameSettingsValues -Key $fakeKey)
Assert-Stack ($settings.Count -eq 3 -and ($settings | Where-Object name -ceq 'binary').kind -ceq 'Binary') 'typed flat settings snapshot'
$fakeKey.SetValue('text','changed',[Microsoft.Win32.RegistryValueKind]::String)
$fakeKey.SetValue('new','staged-only',[Microsoft.Win32.RegistryValueKind]::String)
Restore-KmgFlatGameSettingsValues -Key $fakeKey -Snapshot $settings
Assert-Stack ($fakeKey.values.text -ceq 'original' -and -not $fakeKey.values.ContainsKey('new') -and $fakeKey.kinds.number -eq [Microsoft.Win32.RegistryValueKind]::DWord -and [Convert]::ToBase64String($fakeKey.values.binary) -ceq 'AQID') 'original kinds/values restored and only new flat values removed'
$fakeKey.subkeys=[string[]]@('unexpected')
Assert-StackThrows { Get-KmgFlatGameSettingsValues -Key $fakeKey } 'Unexpected game-settings subkeys'
Assert-StackThrows { Restore-KmgFlatGameSettingsValues -Key $fakeKey -Snapshot $settings } 'Unexpected game-settings subkeys'
Assert-Stack ($fakeKey.values.Count -eq 3 -and $fakeKey.values.text -ceq 'original') 'unexpected hierarchy refusal leaves values unchanged'
$fakeKey.subkeys=[string[]]@()
Restore-KmgFlatGameSettingsValues -Key $fakeKey -Snapshot @()
Assert-Stack ($fakeKey.values.Count -eq 0) 'original flat value absence restored'
Assert-StackThrows {
    & (Join-Path $PSScriptRoot 'Invoke-Sprint22InstalledStack.ps1') -Scenario working-save-expanded-summoning-prepare -SaveName KMG_AUTOMATION_BASELINE -PackagePath missing -ExpectedVersion 0.0.151 -Confirm:$false
} 'ValidateSet'
Assert-StackThrows {
    & (Join-Path $PSScriptRoot 'Invoke-Sprint22InstalledStack.ps1') -Scenario working-save-expanded-summoning-prepare -SaveName KMG_AUTOMATION_WORKING -PackagePath missing -ExpectedVersion 0.0.151 -ExpandedSummoningDisabled -Confirm:$false
} 'Module OFF is restricted'
Write-Host "Sprint22 installed-stack safety PASS: $checks predicates; fixture filesystem only; no game, native settings or real installation touched."
