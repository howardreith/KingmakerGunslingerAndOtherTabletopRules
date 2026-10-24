<# No-game exact manifest/transaction regression. All mutable files are newly
   created beneath this repository's ignored artifacts directory. Actual Enter,
   Restore, immutable ownership sentinels, bank restoration and lease guards run
   with FixtureMode and explicitly empty native process IDs. No real install,
   save, registry, BASE worktree or game is accessed. #>
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'compatibility/CompatibilityProfile.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeCoordination.Common.ps1')
$clock=[Diagnostics.Stopwatch]::StartNew()
$root=Join-Path (Split-Path -Parent $PSScriptRoot) ('artifacts/compatibility-manifest-roundtrip/'+[Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($root)
$script:checks=0;$script:failures=New-Object 'Collections.Generic.List[string]'
function Check([bool]$Value,[string]$Name) {$script:checks++;if(-not $Value){$script:failures.Add($Name)}}
function Reject([scriptblock]$Action,[string]$Name) {$refused=$false;try{& $Action | Out-Null}catch{$refused=$true};Check $refused $Name}
function Clone($Object) {return ($Object | ConvertTo-Json -Depth 12 | ConvertFrom-Json)}
function Json($Object,[string]$Path) {Write-KmgCompatibilityJsonAtomic $Path $Object}
$time='2026-10-21T12:34:56.1234500Z'
$manifest=@([ordered]@{path='.';kind='directory'},[ordered]@{path='Folder';kind='directory'},
    [ordered]@{path='Folder\a.txt';kind='file';length=[long]3;lastWriteTimeUtc=$time;sha256=('a'*64)},
    [ordered]@{path='z.txt';kind='file';length=[long]5;lastWriteTimeUtc='2026-10-21T12:34:56.0000000Z';sha256=('b'*64)})
$roundtrip=Clone $manifest
Check (Test-KmgCompatibilityManifestEqual $manifest $roundtrip) 'actual-json-roundtrip-exact'
$mixed=Clone $manifest
$mixed[2].lastWriteTimeUtc=[DateTime]::Parse($time,[Globalization.CultureInfo]::InvariantCulture,[Globalization.DateTimeStyles]::RoundtripKind)
$mixed[3].lastWriteTimeUtc='2026-10-21T12:34:56Z'
Check (Test-KmgCompatibilityManifestEqual $manifest $mixed) 'DateTime-and-trimmed-zero-text-exact'
$mixed[2].lastWriteTimeUtc='2026-10-21T12:34:56.12345+00:00'
Check (Test-KmgCompatibilityManifestEqual $manifest $mixed) 'explicit-zero-offset-without-lost-ticks'
$mixed[2].lastWriteTimeUtc=[DateTimeOffset]::Parse($time,[Globalization.CultureInfo]::InvariantCulture)
Check (Test-KmgCompatibilityManifestEqual $manifest $mixed) 'UTC-DateTimeOffset-exact'
Check (Test-KmgCompatibilityManifestEqual $manifest @($roundtrip[3],$roundtrip[1],$roundtrip[0],$roundtrip[2])) 'ordinal-path-set-independent-of-PS-sort-order'
Check (Test-KmgCompatibilityManifestEqual @() @()) 'absent-directory-empty-manifests'
foreach($case in @('tick','hash','length','path-case','kind','extra-file','missing-file','duplicate-path','extra-field','missing-field','wrong-field-case',
    'fraction-overprecision','timezone-missing','timezone-nonzero','invalid-date','unspecified-DateTime','length-string','length-fraction','negative-length','uppercase-hash','bad-root','unsafe-path','null-row')) {
    $trial=Clone $manifest
    switch($case) {
        tick {$trial[2].lastWriteTimeUtc='2026-10-21T12:34:56.1234501Z'}
        hash {$trial[2].sha256='c'*64}
        length {$trial[2].length=4}
        'path-case' {$trial[2].path='Folder\A.txt'}
        kind {$trial[2].kind='File'}
        'extra-file' {$extra=Clone $trial[2];$extra.path='extra';$trial+=@($extra)}
        'missing-file' {$trial=@($trial[0],$trial[1],$trial[2])}
        'duplicate-path' {$trial+=@($trial[2])}
        'extra-field' {$trial[2] | Add-Member NoteProperty extra 'untrusted'}
        'missing-field' {$trial[2].PSObject.Properties.Remove('sha256')}
        'wrong-field-case' {$trial[2].PSObject.Properties.Remove('sha256');$trial[2] | Add-Member NoteProperty SHA256 ('a'*64)}
        'fraction-overprecision' {$trial[2].lastWriteTimeUtc='2026-10-21T12:34:56.12345000Z'}
        'timezone-missing' {$trial[2].lastWriteTimeUtc='2026-10-21T12:34:56.1234500'}
        'timezone-nonzero' {$trial[2].lastWriteTimeUtc='2026-10-21T08:34:56.1234500-04:00'}
        'invalid-date' {$trial[2].lastWriteTimeUtc='2026-02-31T12:34:56Z'}
        'unspecified-DateTime' {$trial[2].lastWriteTimeUtc=[DateTime]::SpecifyKind([DateTime]::Parse($time),[DateTimeKind]::Unspecified)}
        'length-string' {$trial[2].length='3'}
        'length-fraction' {$trial[2].length=3.1}
        'negative-length' {$trial[2].length=-1}
        'uppercase-hash' {$trial[2].sha256='A'*64}
        'bad-root' {$trial[0].path='other-root'}
        'unsafe-path' {$trial[2].path='..\escape'}
        'null-row' {$trial[2]=$null}
    }
    Check (-not(Test-KmgCompatibilityManifestEqual $manifest $trial)) ('reject-'+$case)
    if($case -cin @('kind','duplicate-path','extra-field','missing-field','wrong-field-case','fraction-overprecision','timezone-missing','timezone-nonzero',
        'invalid-date','unspecified-DateTime','length-string','length-fraction','negative-length','uppercase-hash','bad-root','unsafe-path','null-row')) {
        Check (-not(Test-KmgCompatibilityManifestEqual $trial $trial)) ('malformed-never-equals-itself-'+$case)
    }
}

function New-TransactionFixture([string]$Name) {
    $directory=Join-Path $root ($Name+'-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
    $install=Join-Path $directory 'game';$stateRoot=Join-Path $directory 'state';$runId='fixture-'+$Name
    $mods=Join-Path $install 'Mods';$backup=Join-Path $install ("Mods.kmg-compat-$runId.original")
    $managed=Join-Path $install 'Kingmaker_Data/Managed'
    $bankPath=Join-Path $install 'Kingmaker_Data/StreamingAssets/Audio/GeneratedSoundBanks/Windows/KMG_Firearms.bnk'
    foreach($dir in @((Join-Path $mods 'KingmakerGunslinger'),(Join-Path $mods 'Peer'),$managed,(Split-Path -Parent $bankPath),(Join-Path $directory 'payload/KingmakerGunslinger'))) {[void][IO.Directory]::CreateDirectory($dir)}
    [IO.File]::WriteAllText((Join-Path $install 'Kingmaker.exe'),'not executable')
    [IO.File]::WriteAllText((Join-Path $managed 'Assembly-CSharp.dll'),'not an assembly')
    [IO.File]::WriteAllText((Join-Path $mods 'KingmakerGunslinger/Info.json'),'{"Id":"KingmakerGunslinger","Version":"0.0.140"}')
    [IO.File]::WriteAllText((Join-Path $mods 'Peer/settings.json'),'exact current peer')
    [IO.File]::WriteAllText((Join-Path $mods 'root.txt'),'current root file')
    [IO.File]::WriteAllText($bankPath,'current original bank fixture')
    foreach($path in @((Join-Path $mods 'KingmakerGunslinger/Info.json'),(Join-Path $mods 'Peer/settings.json'),(Join-Path $mods 'root.txt'),$bankPath)) {
        (Get-Item -LiteralPath $path).LastWriteTimeUtc=[DateTime]::Parse($time,[Globalization.CultureInfo]::InvariantCulture,[Globalization.DateTimeStyles]::RoundtripKind)
    }
    [IO.File]::WriteAllText((Join-Path $directory 'payload/KingmakerGunslinger/Info.json'),'{"Id":"KingmakerGunslinger","Version":"0.0.163"}')
    $package=Join-Path $directory 'candidate.zip'
    Compress-Archive -LiteralPath (Join-Path $directory 'payload/KingmakerGunslinger') -DestinationPath $package
    $resolution=[pscustomobject]@{profileId='fixture-exact-current-intake';runtimeCapable=$true;gunslinger=[pscustomobject]@{packagePath=$package};
        runtimeMods=@([pscustomobject]@{destinationName='Peer';sourceDirectory=(Join-Path $backup 'Peer');sourceManifest=@(Get-KmgCompatibilityDirectoryManifest (Join-Path $mods 'Peer'))})}
    $original=@(Get-KmgCompatibilityDirectoryManifest $mods)
    $bankTicks=(Get-Item -LiteralPath $bankPath).LastWriteTimeUtc.Ticks
    $state=Enter-KmgCompatibilityTransaction -Resolution $resolution -KingmakerInstallDir $install -StateRoot $stateRoot -RunId $runId -FixtureMode -KnownKingmakerProcessIds @()
    return [pscustomobject]@{directory=$directory;install=$install;stateRoot=$stateRoot;runId=$runId;mods=$mods;backup=$backup;
        quarantine=$state.stagedQuarantinePath;state=$state;statePath=(Join-Path (Join-Path $stateRoot $runId) 'transaction.json');original=$original;bankPath=$bankPath;bankTicks=$bankTicks}
}
function Make-PostMoveFailure($Fixture) {
    Move-Item -LiteralPath $Fixture.mods -Destination $Fixture.quarantine
    Move-Item -LiteralPath $Fixture.backup -Destination $Fixture.mods
    $state=Read-KmgCompatibilityJson $Fixture.statePath
    $state.status='RestorationFailed';$state.restorationVerified=$false
    # Reverse file ordering and serialize precise UTC text. PS5 serializes a
    # DateTime object as millisecond /Date(...)/ and would lose the input ticks.
    # PS7 reparses this ISO text as DateTime; PS5 retains it as string.
    $state.originalManifest=@($state.originalManifest[0])+@($state.originalManifest | Select-Object -Skip 1 | Sort-Object path -Descending)
    foreach($row in $state.originalManifest) {if($row.kind -ceq 'file') {$row.lastWriteTimeUtc=[DateTime]::new((ConvertTo-KmgCompatibilityUtcTicks $row.lastWriteTimeUtc),[DateTimeKind]::Utc).ToString('o')}}
    Json $state $Fixture.statePath
    Copy-Item -LiteralPath $Fixture.statePath -Destination (Join-Path $Fixture.directory 'retained-original-failure-journal.json')
    [IO.File]::WriteAllText($Fixture.bankPath,'staged changed bank fixture')
}
function Restore-Fixture($Fixture) {Restore-KmgCompatibilityTransaction -RunId $Fixture.runId -StateRoot $Fixture.stateRoot -FixtureMode -KnownKingmakerProcessIds @()}
function Check-Restored($Fixture,[string]$Label,$Result) {
    Check $Result.restorationVerified ($Label+'-verified')
    Check (Test-KmgCompatibilityManifestEqual $Fixture.original @(Get-KmgCompatibilityDirectoryManifest $Fixture.mods)) ($Label+'-exact-original-mods')
    Check ([IO.File]::ReadAllText($Fixture.bankPath) -ceq 'current original bank fixture' -and (Get-Item -LiteralPath $Fixture.bankPath).LastWriteTimeUtc.Ticks -eq $Fixture.bankTicks) ($Label+'-bank-bytes-and-exact-ticks')
    Check (-not(Test-Path -LiteralPath $Fixture.quarantine) -and -not(Test-Path -LiteralPath (Join-Path $Fixture.stateRoot 'compatibility.lock'))) ($Label+'-owned-quarantine-and-lock-completed')
}
$fixture=New-TransactionFixture 'normal-roundtrip'
Check-Restored $fixture 'normal' (Restore-Fixture $fixture)
$fixture=New-TransactionFixture 'postmove-resume';Make-PostMoveFailure $fixture
Check-Restored $fixture 'postmove-resume' (Restore-Fixture $fixture)
Check (Test-Path -LiteralPath (Join-Path $fixture.directory 'retained-original-failure-journal.json')) 'failure-journal-retained'
foreach($case in @('foreign-mods','one-tick-mods','foreign-sentinel','sentinel-hash','sentinel-statefile','ownership-fields','missing-quarantine','missing-backup','backup-is-file','foreign-lock','wrong-status','live-lease')) {
    $fixture=New-TransactionFixture $case
    if($case -cnotin @('missing-backup','backup-is-file')) {Make-PostMoveFailure $fixture}
    switch($case) {
        'foreign-mods' {[IO.File]::WriteAllText((Join-Path $fixture.mods 'root.txt'),'foreign mods')}
        'one-tick-mods' {$file=Get-Item -LiteralPath (Join-Path $fixture.mods 'root.txt');$file.LastWriteTimeUtc=$file.LastWriteTimeUtc.AddTicks(1)}
        'foreign-sentinel' {$path=Join-Path $fixture.quarantine '.kmg-compat-sentinel.json';$s=Read-KmgCompatibilityJson $path;$s.runId='foreign';Json $s $path}
        'sentinel-hash' {$path=Join-Path $fixture.quarantine '.kmg-compat-sentinel.json';$s=Read-KmgCompatibilityJson $path;$s.stateSha256='0'*64;Json $s $path}
        'sentinel-statefile' {$path=Join-Path $fixture.quarantine '.kmg-compat-sentinel.json';$s=Read-KmgCompatibilityJson $path;$other=Join-Path $fixture.directory 'foreign-ownership.json';Copy-Item $s.stateFile $other;$s.stateFile=$other;Json $s $path}
        'ownership-fields' {
            $path=Join-Path $fixture.quarantine '.kmg-compat-sentinel.json';$s=Read-KmgCompatibilityJson $path
            $o=Read-KmgCompatibilityJson $s.stateFile;$o.modsPath=Join-Path $fixture.install 'ForeignMods';Json $o $s.stateFile
            $s.stateSha256=Get-KmgCompatibilitySha256 $s.stateFile;Json $s $path
        }
        'missing-quarantine' {Move-Item -LiteralPath $fixture.quarantine -Destination ($fixture.quarantine+'.withheld')}
        'missing-backup' {Move-Item -LiteralPath $fixture.backup -Destination ($fixture.backup+'.withheld')}
        'backup-is-file' {Move-Item -LiteralPath $fixture.backup -Destination ($fixture.backup+'.withheld');[IO.File]::WriteAllText($fixture.backup,'foreign file')}
        'foreign-lock' {[IO.File]::WriteAllText((Join-Path $fixture.stateRoot 'compatibility.lock'),'foreign')}
        'wrong-status' {$s=Read-KmgCompatibilityJson $fixture.statePath;$s.status='Active';Json $s $fixture.statePath}
        'live-lease' {
            $s=Read-KmgCompatibilityJson $fixture.statePath;$s.status='Active';Json $s $fixture.statePath
            $lease=Open-KmgCompatibilityRuntimeLease $fixture.runId $fixture.stateRoot
            $s.status='RestorationFailed';Json $s $fixture.statePath
        }
    }
    $before=@(Get-KmgCompatibilityDirectoryManifest $fixture.mods)
    Reject {Restore-Fixture $fixture} ('resume-refuses-'+$case)
    Check (Test-KmgCompatibilityManifestEqual $before @(Get-KmgCompatibilityDirectoryManifest $fixture.mods)) ($case+'-current-mods-not-overwritten')
    Check (Test-Path -LiteralPath (Join-Path $fixture.stateRoot 'compatibility.lock')) ($case+'-lock-not-stolen-or-removed')
    if($case -ceq 'live-lease') {$lease.Stream.Dispose()}
}
$report=[ordered]@{kind='CompatibilityManifestRoundTripNoGame';powerShellVersion=$PSVersionTable.PSVersion.ToString();
    checks=$checks;failures=@($failures.ToArray());elapsedSeconds=$clock.Elapsed.TotalSeconds;evidenceDirectory=$root;nativeRuntimeExecuted=$false}
Json $report (Join-Path $root 'test-summary.json')
if($failures.Count){throw ('Exact compatibility manifest tests failed: '+($failures -join ', '))}
Write-Output ($report | ConvertTo-Json -Depth 5)
