[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
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
$receiptEvidence=Join-Path $fixture 'exact-receipt'
[void][IO.Directory]::CreateDirectory($receiptEvidence)
@{runId='exact-run';evidenceDirectory=$receiptEvidence}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $receiptEvidence 'runtime-request.json') -Encoding UTF8
@{runId='exact-run';evidenceDirectory=$receiptEvidence}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $receiptEvidence 'runtime-result.json') -Encoding UTF8
$exact=[pscustomobject]@{kind='KmgRuntimeLaunchReceipt';runId='exact-run';evidenceDirectory=$receiptEvidence}
Assert-Stack ((Get-KmgExactRuntimeLaunchReceipt -Output @('unrelated output',$exact)).runId -ceq 'exact-run') 'exact receipt selected independently of timestamps'
Assert-StackThrows { Get-KmgExactRuntimeLaunchReceipt -Output @() } 'missing or duplicated'
Assert-StackThrows { Get-KmgExactRuntimeLaunchReceipt -Output @($exact,$exact) } 'missing or duplicated'
Assert-StackThrows { Get-KmgExactRuntimeLaunchReceipt -Output @([pscustomobject]@{kind='KmgRuntimeLaunchReceipt';runId='wrong-run';evidenceDirectory=$receiptEvidence}) } 'exact RunId request and result'
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
# Actual fixed UI entrypoint refusals precede candidate/installation work.
$uiEntry=Join-Path $PSScriptRoot 'Invoke-Sprint22InstalledStack.ps1'
$uiArguments=@{Scenario='working-save-expanded-summoning-creature-review';SaveName='KMG_AUTOMATION_WORKING';
    PackagePath=(Join-Path $fixture 'missing-ui-development.zip');ExpectedVersion=(Get-KmgModInfo -RepositoryRoot $repo).Version;
    ReviewCase='sprints30-31-dispel-ui';DevelopmentPurpose='sprints30-31-dispel-installed-ui';Confirm=$false}
$bag=$uiArguments.Clone();$bag.Remove('ReviewCase');Assert-StackThrows { & $uiEntry @bag } 'Installed-stack UI requires only'
foreach($change in @(@{ReviewCase='SPRINTS30-31-DISPEL-UI'},
    @{PersistenceScope='ankylosaurus'},@{ExpandedSummoningDisabled=$true},@{TimeoutSeconds=300},
    @{SaveName='kmg_automation_working'})) {
    $bag=$uiArguments.Clone();foreach($key in $change.Keys){$bag[$key]=$change[$key]}
    Assert-StackThrows { & $uiEntry @bag } 'Installed-stack UI requires only'
}
$bag=$uiArguments.Clone();$bag.DevelopmentPurpose='sprint29-hidden-working'
Assert-StackThrows { & $uiEntry @bag } 'requires its exact S30/S31 purpose'
$bag=$uiArguments.Clone();$bag.Scenario='working-save-expanded-summoning-prepare'
Assert-StackThrows { & $uiEntry @bag } 'reviewCase is restricted'
$bag=$uiArguments.Clone();$bag.Scenario='Working-Save-Expanded-Summoning-Creature-Review'
Assert-StackThrows { & $uiEntry @bag } 'reviewCase is restricted'
Assert-StackThrows { & $uiEntry @uiArguments } 'missing-ui-development.zip'
# Execute the shipped argument-construction expressions with fixture values.
# This observes actual forwarding, not a duplicated request implementation.
$parseTokens=$null;$parseErrors=$null
$entryAst=[Management.Automation.Language.Parser]::ParseFile($uiEntry,[ref]$parseTokens,[ref]$parseErrors)
Assert-Stack ($parseErrors.Count -eq 0) 'installed-stack entrypoint parses'
$construction=@()
foreach($variable in @('$requestParameters','$completionBudget','$arguments')) {
    $nodes=@($entryAst.FindAll({param($node) $node -is [Management.Automation.Language.AssignmentStatementAst] -and $node.Left.Extent.Text -ceq $variable},$true))
    if($nodes.Count -ne 1){throw 'Installed-stack shipped request construction is absent or ambiguous.'}
    $construction += [scriptblock]::Create($nodes[0].Extent.Text)
}
$fixedDispelUi=$true;$uiParameters=@{creatures='trumpet-archon,astral-deva';reviewCase='sprints30-31-dispel-ui'}
$Scenario='working-save-expanded-summoning-creature-review';$ExpectedVersion='fixture-version'
$SaveName='KMG_AUTOMATION_WORKING';$TimeoutSeconds=600;$PersistenceScope='ankylosaurus'
$lease=[pscustomobject]@{fixture=$true};$candidate=[pscustomobject]@{packagePath='fixture-package'}
$deployment='fixture-deployment';$DevelopmentPurpose='sprints30-31-dispel-installed-ui'
foreach($expression in $construction){. $expression}
Assert-Stack ($arguments.Parameters.Count -eq 2 -and $arguments.Parameters.reviewCase -ceq 'sprints30-31-dispel-ui' -and $arguments.Parameters.creatures -ceq 'trumpet-archon,astral-deva' -and -not $arguments.Parameters.ContainsKey('persistenceScope')) 'fixed UI actual forwarding retains exactly its two external parameters'
Assert-Stack ($arguments.SaveName -ceq $SaveName -and $arguments.ExitAfterCompletion -and $arguments.CompletionTimeoutSeconds -eq 180 -and $arguments.TimeoutSeconds -eq 600 -and $arguments.RuntimeLease -eq $lease -and $arguments.ReuseInstalledArtifact) 'fixed UI actual forwarding retains save exit timing lease and artifact guards'
$fixedDispelUi=$false;$PersistenceScope='giant-humanoids-ii'
foreach($expression in $construction){. $expression}
Assert-Stack ($arguments.Parameters.Count -eq 1 -and $arguments.Parameters.persistenceScope -ceq $PersistenceScope -and $arguments.CompletionTimeoutSeconds -eq 600) 'existing persistence forwarding and native budget remain unchanged'
$fixedDispelUi=$false;$PersistenceScope='outsider-celestial';$DevelopmentPurpose='sprints30-31-persistence'
foreach($expression in $construction){. $expression}
Assert-Stack ($arguments.Parameters.Count -eq 1 -and $arguments.Parameters.persistenceScope -ceq $PersistenceScope -and $arguments.SaveName -ceq 'KMG_AUTOMATION_WORKING' -and $arguments.ExitAfterCompletion -and $arguments.CompletionTimeoutSeconds -eq 600 -and $arguments.RuntimeLease -eq $lease -and $arguments.ReuseInstalledArtifact -and $arguments.DevelopmentPurpose -ceq $DevelopmentPurpose) 'pilot persistence actual request keeps fixed scope, native budget, purpose, lease, artifact and Working guards'
$receiptNodes=@($entryAst.FindAll({param($node) $node -is [Management.Automation.Language.IfStatementAst] -and $node.Extent.Text.Contains('NotePropertyName installedStackRecordPath')},$true))
if($receiptNodes.Count -ne 1){throw 'Fixed UI installed-stack receipt binding is absent or ambiguous.'}
$receiptBinding=[scriptblock]::Create($receiptNodes[0].Extent.Text)
$receipt=[pscustomobject]@{kind='KmgRuntimeLaunchReceipt';runId='fixture-native-run'}
$fixedDispelUi=$true;$runId='fixture-stack-run';$evidenceRoot=Join-Path $fixture 'stack-record'
. $receiptBinding
Assert-Stack ($receipt.transactionRunId -ceq $runId -and $receipt.installedStackRecordPath -ceq (Join-Path $evidenceRoot 'installed-stack.json') -and $receipt.runId -ceq 'fixture-native-run') 'fixed UI returned receipt binds native and installed-stack run identities'
$receipt=[pscustomobject]@{kind='KmgRuntimeLaunchReceipt';runId='fixture-native-run'};$fixedDispelUi=$false
. $receiptBinding
Assert-Stack ($null -eq $receipt.PSObject.Properties['transactionRunId'] -and $null -eq $receipt.PSObject.Properties['installedStackRecordPath']) 'existing persistence receipt shape remains unchanged'
Write-Host "Sprint22 installed-stack safety PASS: $checks predicates; fixture filesystem only; no game, native settings or real installation touched."
