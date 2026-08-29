[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'CompatibilityProfile.Common.ps1')
$repo = Get-KmgCompatibilityRepositoryRoot
$testRoot = Join-Path $repo 'artifacts\compatibility\tests\transaction'
if (Test-Path -LiteralPath $testRoot) {
    $resolved = (Resolve-Path -LiteralPath $testRoot).Path
    $expected = [IO.Path]::GetFullPath($testRoot)
    if (-not $resolved.Equals($expected, [StringComparison]::OrdinalIgnoreCase)) { throw "Unexpected transaction fixture path: $resolved" }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
New-Item -ItemType Directory -Path $testRoot | Out-Null

function Assert-True([bool]$Condition, [string]$Label) { if (-not $Condition) { throw "Assertion failed: $Label" } }
function Assert-Throws([scriptblock]$Action, [string]$Pattern, [string]$Label) {
    try { & $Action; throw "Expected exception was not raised: $Label" }
    catch { if ($_.Exception.Message -notlike "*$Pattern*") { throw "Wrong exception for $Label`: $($_.Exception.Message)" } }
}
function New-Fixture([string]$Name, [bool]$OriginalExists = $true, [bool]$BankExists = $true) {
    $root = Join-Path $testRoot $Name
    $install = Join-Path $root 'Pathfinder Kingmaker'
    New-Item -ItemType Directory -Path (Join-Path $install 'Kingmaker_Data\Managed') -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $install 'Kingmaker.exe') -Value 'fixture-game' -Encoding Ascii
    Set-Content -LiteralPath (Join-Path $install 'Kingmaker_Data\Managed\Assembly-CSharp.dll') -Value 'fixture-assembly' -Encoding Ascii
    if ($OriginalExists) {
        New-Item -ItemType Directory -Path (Join-Path $install 'Mods\Existing Mod\settings') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $install 'Mods\Existing Mod\Info.json') -Value '{"Id":"Existing"}' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $install 'Mods\Existing Mod\settings\state.txt') -Value 'preserve-exactly' -Encoding UTF8
    }
    $bank = Join-Path $install 'Kingmaker_Data\StreamingAssets\Audio\GeneratedSoundBanks\Windows\KMG_Firearms.bnk'
    if ($BankExists) { New-Item -ItemType Directory -Path (Split-Path -Parent $bank) -Force | Out-Null; Set-Content -LiteralPath $bank -Value 'original-bank' -Encoding Ascii }
    $packageSource = Join-Path $root 'package-source\KingmakerGunslinger'
    New-Item -ItemType Directory -Path $packageSource -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $packageSource 'Info.json') -Value '{"Id":"KingmakerGunslinger","Version":"0.0.107"}' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $packageSource 'KingmakerGunslinger.dll') -Value 'gunslinger-fixture' -Encoding Ascii
    $zip = Join-Path $root 'KingmakerGunslinger-0.0.107-local-runtime.zip'
    Compress-Archive -LiteralPath $packageSource -DestinationPath $zip
    $third = Join-Path $root 'references\ThirdMod'
    New-Item -ItemType Directory -Path $third -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $third 'Info.json') -Value '{"Id":"ThirdMod","Version":"1.0","AssemblyName":"ThirdMod.dll","EntryMethod":"ThirdMod.Main.Load"}' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $third 'ThirdMod.dll') -Value 'third-fixture' -Encoding Ascii
    $resolution = [ordered]@{
        profileId = 'gunslinger-fixture'; runtimeCapable = $true
        gunslinger = [ordered]@{ packagePath = $zip }
        runtimeMods = @([ordered]@{ sourceDirectory = $third; destinationName = 'ThirdMod'; sourceManifest = @(Get-KmgCompatibilityDirectoryManifest $third) })
    }
    return [pscustomobject]@{ Root = $root; Install = $install; State = (Join-Path $root 'state'); Resolution = $resolution; Bank = $bank }
}

$fixture = New-Fixture 'success'
$before = @(Get-KmgCompatibilityDirectoryManifest (Join-Path $fixture.Install 'Mods'))
$bankHash = Get-KmgCompatibilitySha256 $fixture.Bank
$entered = Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'success' -FixtureMode -KnownKingmakerProcessIds @()
Assert-True ((Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods\KingmakerGunslinger\Info.json'))) 'Gunslinger staged'
Assert-True ((Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods\ThirdMod\Info.json'))) 'third-party mod staged'
$restored = Restore-KmgCompatibilityTransaction -RunId 'success' -StateRoot $fixture.State -FixtureMode -KnownKingmakerProcessIds @()
Assert-True $restored.restorationVerified 'successful restoration verified'
Assert-True (Test-KmgCompatibilityManifestEqual $before @(Get-KmgCompatibilityDirectoryManifest (Join-Path $fixture.Install 'Mods'))) 'original Mods exact'
Assert-True ((Get-KmgCompatibilitySha256 $fixture.Bank) -ceq $bankHash) 'managed SoundBank exact'
$again = Restore-KmgCompatibilityTransaction -RunId 'success' -StateRoot $fixture.State -FixtureMode -KnownKingmakerProcessIds @()
Assert-True $again.restorationVerified 'duplicate restore idempotent'

$fixture = New-Fixture 'profile-failure'
$fixture.Resolution.runtimeMods[0].sourceDirectory = Join-Path $fixture.Root 'missing-source'
Assert-Throws { Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'profile-failure' -FixtureMode -KnownKingmakerProcessIds @() } 'cannot find path' 'copy failure'
Assert-True (Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods\Existing Mod\Info.json')) 'copy failure restored original'

$fixture = New-Fixture 'launch-exception'
try {
    Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'launch-exception' -FixtureMode -KnownKingmakerProcessIds @() | Out-Null
    throw 'simulated launch exception'
} catch { $launchError = $_ } finally {
    Restore-KmgCompatibilityTransaction -RunId 'launch-exception' -StateRoot $fixture.State -FixtureMode -KnownKingmakerProcessIds @() | Out-Null
}
Assert-True ($launchError.Exception.Message -eq 'simulated launch exception') 'launch exception preserved'
Assert-True (Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods\Existing Mod\Info.json')) 'launch exception restoration'

$fixture = New-Fixture 'destination-collision'
$fixture.Resolution.runtimeMods[0].destinationName = 'KingmakerGunslinger'
Assert-Throws { Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'destination-collision' -FixtureMode -KnownKingmakerProcessIds @() } 'Staged destination collision' 'destination collision'
Assert-True (Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods\Existing Mod\Info.json')) 'collision restoration'

$fixture = New-Fixture 'unresolved'
Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'unresolved-one' -FixtureMode -KnownKingmakerProcessIds @() | Out-Null
Assert-Throws { Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'unresolved-two' -FixtureMode -KnownKingmakerProcessIds @() } 'Unresolved compatibility transaction exists' 'unresolved transaction refusal'
Restore-KmgCompatibilityTransaction -RunId 'unresolved-one' -StateRoot $fixture.State -FixtureMode -KnownKingmakerProcessIds @() | Out-Null

$fixture = New-Fixture 'interrupted'
Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'interrupted' -FixtureMode -KnownKingmakerProcessIds @() | Out-Null
$recovered = Restore-KmgCompatibilityTransaction -RunId 'interrupted' -StateRoot $fixture.State -FixtureMode -KnownKingmakerProcessIds @()
Assert-True $recovered.restorationVerified 'interrupted recovery verified'

$fixture = New-Fixture 'staged-mutation'
Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'staged-mutation' -FixtureMode -KnownKingmakerProcessIds @() | Out-Null
Set-Content -LiteralPath (Join-Path $fixture.Install 'Mods\unexpected.txt') -Value 'mutation' -Encoding UTF8
$mutation = Restore-KmgCompatibilityTransaction -RunId 'staged-mutation' -StateRoot $fixture.State -FixtureMode -KnownKingmakerProcessIds @()
Assert-True $mutation.restorationVerified 'staged mutation still restores original'
Assert-True $mutation.stagedMutationObserved 'unexpected staged file recorded'

$fixture = New-Fixture 'hash-mismatch'
Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'hash-mismatch' -FixtureMode -KnownKingmakerProcessIds @() | Out-Null
Set-Content -LiteralPath (Join-Path $fixture.Install 'Mods.kmg-compat-hash-mismatch.original\Existing Mod\settings\state.txt') -Value 'corrupt' -Encoding UTF8
Assert-Throws { Restore-KmgCompatibilityTransaction -RunId 'hash-mismatch' -StateRoot $fixture.State -FixtureMode -KnownKingmakerProcessIds @() } 'manifest/hash mismatch' 'original hash mismatch'
Assert-True (Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods')) 'failed restore preserves restored-original path'
Assert-True (Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods.kmg-compat-hash-mismatch.staged')) 'failed restore preserves staged quarantine'

$fixture = New-Fixture 'original-absent' $false $false
Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'original-absent' -FixtureMode -KnownKingmakerProcessIds @() | Out-Null
$absent = Restore-KmgCompatibilityTransaction -RunId 'original-absent' -StateRoot $fixture.State -FixtureMode -KnownKingmakerProcessIds @()
Assert-True $absent.restorationVerified 'absent original restoration verified'
Assert-True (-not (Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods'))) 'original Mods absence restored'
Assert-True (-not (Test-Path -LiteralPath $fixture.Bank)) 'absent bank restored as absent'

$fixture = New-Fixture 'running-refusal'
Assert-Throws { Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'running-refusal' -FixtureMode -KnownKingmakerProcessIds @(4242) } 'Kingmaker is running' 'Kingmaker-running refusal'
Assert-True (Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods\Existing Mod\Info.json')) 'running refusal made no mutation'

$fixture = New-Fixture 'sentinel-mismatch'
Enter-KmgCompatibilityTransaction -Resolution $fixture.Resolution -KingmakerInstallDir $fixture.Install -StateRoot $fixture.State -RunId 'sentinel-mismatch' -FixtureMode -KnownKingmakerProcessIds @() | Out-Null
$sentinelPath = Join-Path $fixture.Install 'Mods\.kmg-compat-sentinel.json'
$sentinel = Get-Content -LiteralPath $sentinelPath -Raw | ConvertFrom-Json
$sentinel.stateSha256 = ('0' * 64)
$sentinel | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $sentinelPath -Encoding UTF8
Assert-Throws { Restore-KmgCompatibilityTransaction -RunId 'sentinel-mismatch' -StateRoot $fixture.State -FixtureMode -KnownKingmakerProcessIds @() } 'ownership-state hash mismatch' 'sentinel hash mismatch'
Assert-True (Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods')) 'sentinel mismatch preserves staged Mods'
Assert-True (Test-Path -LiteralPath (Join-Path $fixture.Install 'Mods.kmg-compat-sentinel-mismatch.original')) 'sentinel mismatch preserves original backup'

Write-Host 'Kingmaker compatibility transaction filesystem integration tests passed.'
