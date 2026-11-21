Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Loader-compatibility sessions in a disposable full game copy. Adapted from the
# Kingmaker Cutscene Skip lab runtime harness (scripts/runtime, PR #2). The
# owner's installation, saves and game settings are only hashed and restored;
# every loader and mod change happens inside the copy.

$script:LcLabRoot = 'C:\Dev\KingmakerGunslingerLab'
$script:LcCopiesRoot = Join-Path $script:LcLabRoot 'game-copies'
$script:LcEvidenceRoot = Join-Path $script:LcLabRoot 'runtime-evidence\loader-compat'
$script:LcLock = Join-Path $script:LcCopiesRoot 'loader-compat.lock'
$script:LcJournal = Join-Path $script:LcCopiesRoot 'loader-compat.journal.json'
$script:LcComponentCache = Join-Path $script:LcLabRoot 'private\umm-distributions\cache'
$script:LcOwnerInstall = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker'
$script:LcOwnerData = Join-Path $env:USERPROFILE 'AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker'
$script:LcRegistryKey = 'HKCU\Software\Owlcat Games\Pathfinder Kingmaker'
# Locks of sibling labs on this machine; a session never starts while one exists.
$script:LcForeignLocks = @(
    'C:\Dev\KingmakerGunslingerLab\compatibility-state\compatibility.lock',
    'C:\Dev\KingmakerSkipCinematicLab\runtime-state\runtime.lock',
    'C:\Dev\KingmakerBuffPlannerLab\runtime-state\deployment.lock',
    'C:\Dev\KingmakerBuffPlannerLab\runtime-fixture-state\fixture.lock',
    'C:\Dev\KingmakerMemoryLeakResearchLab\runtime-state\kmld-runtime.lock',
    'C:\Dev\KingmakerMountedCombatLab\runtime-state\active-transaction.lock')

# The lab's shared runtime ownership: the same compatibility.lock and runtime-lease
# journal that profile staging, deployment, the generic runtime launcher and the
# settings transactions use (scripts/RuntimeCoordination.Common.ps1).
. (Join-Path $PSScriptRoot '..\..\RuntimeCoordination.Common.ps1')
$script:LcStateRoot = 'C:\Dev\KingmakerGunslingerLab\compatibility-state'
# Every Kingmaker process on the host. A script variable so the offline safety
# checks can substitute a disposable peer without starting a game.
$script:LcGameProcessQuery = { @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) }
function Get-LcGameProcesses { return , @(& $script:LcGameProcessQuery) }

function Get-LcSha256([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }

# Takes the shared compatibility lock for the whole session (refuses if any other
# entry point holds it) and records a runtime-lease journal for it.
function Enter-LcRuntimeOwnership([string]$SessionId) {
    return Enter-KmgRuntimeLease -ParentLease $null -Purpose ('loader-compat ' + $SessionId) -StateRoot $script:LcStateRoot
}

# Immediately before every fresh launch: no Kingmaker process of any origin, no
# sibling lab lock, and the shared lock still held by this session's lease.
function Assert-LcLaunchAvailable($Lease) {
    $games = Get-LcGameProcesses
    if ($games.Count -gt 0) {
        throw "Launch withheld: Kingmaker is running (pid $((@($games | ForEach-Object { $_.Id })) -join ',')); another session may have taken the host."
    }
    foreach ($lock in $script:LcForeignLocks) {
        if (-not (Test-Path -LiteralPath $lock)) { continue }
        $owned = $null -ne $Lease -and [IO.Path]::GetFullPath($lock).Equals([IO.Path]::GetFullPath($Lease.LockPath), [StringComparison]::OrdinalIgnoreCase) -and
            (Get-Content -LiteralPath $lock -Raw).Trim() -ceq $Lease.RunId
        if (-not $owned) { throw "Launch withheld: another lab holds a runtime lock: $lock" }
    }
    if ($null -ne $Lease) { Assert-KmgRuntimeLease $Lease }
}

function Set-LcLeaseRecoveryRequired($Lease, [string]$Reason) {
    if ($null -eq $Lease) { return }
    $Lease.State.status = 'RecoveryRequired'; $Lease.State.recoveryRequired = $true; $Lease.State.reason = $Reason
    Write-KmgRuntimeLeaseState $Lease
    $Lease.Stream.Dispose()
}

function Complete-LcOwnedLease($Lease, [string]$Reason) {
    Assert-KmgRuntimeLease $Lease
    if ((Get-LcGameProcesses).Count -gt 0) { throw 'Kingmaker is running; the shared lock is not released.' }
    $Lease.State.status = 'Completed'; $Lease.State.recoveryRequired = $false; $Lease.State.reason = $Reason
    Write-KmgRuntimeLeaseState $Lease
    $Lease.Stream.Dispose()
    Remove-KmgCompatibilityOwnedLock $Lease.LockPath $Lease.RunId
}

# The session finalizer. Game settings are restored only at a safe boundary: once
# no Kingmaker process remains (waiting up to WaitSeconds). Otherwise nothing is
# restored and the journal, the loader-compat lock and a RecoveryRequired lease on
# the shared lock are kept for -RecoverOnly. A failed restoration keeps them too.
function Complete-LcSessionOwnership($Journal, $Scope, [scriptblock]$Restore, [int]$WaitSeconds = 60) {
    $lease = if ($null -ne $Scope) { $Scope.Lease } else { $null }
    $deadline = (Get-Date).AddSeconds($WaitSeconds)
    while ((Get-LcGameProcesses).Count -gt 0 -and (Get-Date) -lt $deadline) { Start-Sleep -Seconds 1 }
    $games = Get-LcGameProcesses
    if ($games.Count -gt 0) {
        $reason = "Kingmaker is still running (pid $((@($games | ForEach-Object { $_.Id })) -join ',')); game settings were not restored and ownership is retained. Run Invoke-LoaderCompatSession.ps1 -RecoverOnly after it exits."
        $Journal.recoveryRequired = $true; $Journal.reason = $reason
        Write-LcJson $script:LcJournal $Journal
        Set-LcLeaseRecoveryRequired $lease $reason
        return [pscustomobject]@{ Outcome = 'recovery-required'; Reason = $reason }
    }
    try {
        if ($Journal.registryBackup) { & $Restore $Journal.registryBackup }
    } catch {
        $reason = "Game-settings restoration failed: $($_.Exception.Message). Ownership is retained for -RecoverOnly."
        $Journal.recoveryRequired = $true; $Journal.reason = $reason
        Write-LcJson $script:LcJournal $Journal
        Set-LcLeaseRecoveryRequired $lease $reason
        return [pscustomobject]@{ Outcome = 'recovery-required'; Reason = $reason }
    }
    if ($null -ne $lease) { Complete-LcOwnedLease $lease 'Loader-compat session completed; game settings restored with no Kingmaker process running.' }
    if (Test-Path -LiteralPath $script:LcJournal) { Remove-Item -LiteralPath $script:LcJournal -Force }
    return [pscustomobject]@{ Outcome = 'restored'; Reason = $null }
}

# Completes a session lease left RecoveryRequired, after its original owner and
# every game have exited. Never releases a lock another run owns.
function Complete-LcRecoveredLease([string]$RunId) {
    $statePath = Join-Path (Join-Path $script:LcStateRoot $RunId) 'runtime-lease.json'
    $lockPath = Join-Path $script:LcStateRoot 'compatibility.lock'
    $state = Read-KmgCompatibilityJson $statePath
    if ($state.runId -cne $RunId -or ([string]$state.purpose) -notlike 'loader-compat *' -or $state.status -cnotin @('Active', 'RecoveryRequired')) {
        throw "Recovery covers only this session's own active or RecoveryRequired lease: $statePath"
    }
    if (-not (Test-Path -LiteralPath $lockPath -PathType Leaf) -or (Get-Content -LiteralPath $lockPath -Raw).Trim() -cne $RunId) {
        throw 'The shared compatibility lock is not held by this session; nothing was released.'
    }
    $owner = Get-Process -Id $state.processId -ErrorAction SilentlyContinue
    if (Test-KmgRuntimeProcessIdentity $owner $state.processId $state.processStartedUtc) { throw 'The original session owner is still alive; nothing was released.' }
    $stream = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read)
    try {
        $lease = [pscustomobject]@{ RunId = $RunId; LockPath = $lockPath; Stream = $stream; ProcessId = $PID
            StatePath = $statePath; StateSha256 = (Get-KmgCompatibilitySha256 $statePath); Compatibility = $false; State = $state }
        $state.status = 'Completed'; $state.recoveryRequired = $false
        $state.reason = 'Recovered by Invoke-LoaderCompatSession.ps1 -RecoverOnly after every Kingmaker process exited; game settings restored.'
        Write-KmgRuntimeLeaseState $lease
        $stream.Dispose()
        Remove-KmgCompatibilityOwnedLock $lockPath $RunId
    } finally { $stream.Dispose() }
}

# -RecoverOnly: refuses while any Kingmaker process runs; otherwise restores the
# backed-up game settings, completes the session's shared lease and releases the
# journal and the loader-compat lock.
function Invoke-LcRecovery([scriptblock]$Restore) {
    if (-not (Test-Path -LiteralPath $script:LcJournal)) { return 'No interrupted session journal.' }
    if ((Get-LcGameProcesses).Count -gt 0) { throw 'Kingmaker is running; recover after it exits. Nothing was changed.' }
    $journal = Get-Content -LiteralPath $script:LcJournal -Raw | ConvertFrom-Json
    if ($journal.PSObject.Properties['registryBackup'] -and $journal.registryBackup) { & $Restore $journal.registryBackup }
    if ($journal.PSObject.Properties['leaseRunId'] -and $journal.leaseRunId) { Complete-LcRecoveredLease $journal.leaseRunId }
    Remove-Item -LiteralPath $script:LcJournal -Force
    Exit-LcLock
    return "Recovered interrupted session $($journal.sessionId)."
}

function Write-LcJson([string]$Path, $Value) {
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 12), (New-Object Text.UTF8Encoding($false)))
}

# Refuses anything but a copy under the lab's game-copies root, so a mistyped
# path can never touch the loader of a real installation.
function Assert-LcGameCopy([string]$CopyRoot, [string]$OwnerInstall = $script:LcOwnerInstall) {
    $full = [IO.Path]::GetFullPath($CopyRoot).TrimEnd('\')
    $owner = [IO.Path]::GetFullPath($OwnerInstall).TrimEnd('\')
    $allowed = $script:LcCopiesRoot.TrimEnd('\') + '\'
    if ($full -ieq $owner -or $full.StartsWith($owner + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to use the owner's installation as a game copy: $full"
    }
    if (-not $full.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase)) { throw "A game copy must live under $allowed (got $full)." }
    if (-not (Test-Path -LiteralPath (Join-Path $full 'Kingmaker.exe') -PathType Leaf)) { throw "Game copy lacks Kingmaker.exe: $full" }
    return $full
}

function Get-LcCopyPaths([string]$CopyRoot) {
    $managed = Join-Path $CopyRoot 'Kingmaker_Data\Managed'
    return [pscustomobject]@{
        InstallDir = $CopyRoot; ManagedDir = $managed; UmmDir = Join-Path $managed 'UnityModManager'
        ModsDir = Join-Path $CopyRoot 'Mods'; Pristine = Join-Path $CopyRoot '.lab-pristine'
    }
}

function Assert-LcPreflight {
    if ((Get-LcGameProcesses).Count -gt 0) { throw 'Kingmaker is already running; refusing to take over another session.' }
    foreach ($lock in $script:LcForeignLocks) { if (Test-Path -LiteralPath $lock) { throw "Another lab holds a runtime lock: $lock" } }
    if (Test-Path -LiteralPath $script:LcJournal) { throw "An interrupted session journal exists ($script:LcJournal); run with -RecoverOnly first." }
    $steam = (Get-ItemProperty -LiteralPath 'HKCU:\Software\Valve\Steam').SteamPath
    $cloudLog = Join-Path $steam 'logs\cloud_log.txt'
    $last = @(Select-String -LiteralPath $cloudLog -Pattern '\[AppID 640820\]' | Select-Object -Last 6 | ForEach-Object Line)
    if (-not ($last | Where-Object { $_ -match 'Sync Disabled' })) { throw 'Steam Cloud sync for Kingmaker is not proven disabled (cloud_log.txt); refusing to launch.' }
    if (-not (Get-Process -Name steam -ErrorAction SilentlyContinue)) { throw 'Steam is not running; a direct game-copy launch needs it.' }
}

function Enter-LcLock([string]$SessionId) {
    New-Item -ItemType Directory -Force -Path $script:LcCopiesRoot | Out-Null
    $stream = [IO.File]::Open($script:LcLock, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes("session=$SessionId`npid=$PID`nstartedUtc=$([DateTime]::UtcNow.ToString('o'))`n")
        $stream.Write($bytes, 0, $bytes.Length)
    } finally { $stream.Dispose() }
}

function Exit-LcLock { if (Test-Path -LiteralPath $script:LcLock) { Remove-Item -LiteralPath $script:LcLock -Force } }

function Get-LcFileManifest([string[]]$Paths) {
    return @(& { foreach ($path in $Paths) {
        if (Test-Path -LiteralPath $path -PathType Container) {
            Get-ChildItem -LiteralPath $path -File -Recurse -Force
        } elseif (Test-Path -LiteralPath $path -PathType Leaf) { Get-Item -LiteralPath $path -Force }
    } } | Sort-Object FullName | ForEach-Object { '{0}|{1}|{2}' -f $_.FullName, $_.Length, (Get-LcSha256 $_.FullName) })
}

function Get-LcOwnerSavesManifest { Get-LcFileManifest @((Join-Path $script:LcOwnerData 'Saved Games')) }

# Every file a loader or mod change could affect in the owner's installation.
function Get-LcOwnerInstallManifest {
    $o = $script:LcOwnerInstall
    Get-LcFileManifest @((Join-Path $o 'winhttp.dll'), (Join-Path $o 'doorstop_config.ini'), (Join-Path $o 'Kingmaker.exe'),
        (Join-Path $o 'Kingmaker_Data\Managed\Assembly-CSharp.dll'), (Join-Path $o 'Kingmaker_Data\Managed\UnityModManager'),
        (Join-Path $o 'Mods'))
}

function Compare-LcManifest([string[]]$Before, [string[]]$After) {
    $changes = @()
    $changes += @($Before | Where-Object { $After -notcontains $_ } | ForEach-Object { 'changed/removed: ' + $_.Split('|')[0] })
    $changes += @($After | Where-Object { $Before -notcontains $_ } | ForEach-Object { 'new/changed: ' + $_.Split('|')[0] })
    return ,$changes
}

function Invoke-LcReg([string[]]$Arguments) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try { $output = & reg.exe @Arguments 2>&1 | ForEach-Object { "$_" } } finally { $ErrorActionPreference = $previous }
    return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -join "`n") }
}

function Backup-LcRegistry([string]$Path) {
    $r = Invoke-LcReg @('export', $script:LcRegistryKey, $Path, '/y')
    if ($r.ExitCode -ne 0) { throw "Registry export failed: $($r.Output)" }
}

# Exact restoration: remove the key the runs may have changed, then import the
# backup taken before the session.
function Restore-LcRegistry([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Registry backup is missing: $Path" }
    $d = Invoke-LcReg @('delete', $script:LcRegistryKey, '/f')
    if ($d.ExitCode -ne 0 -and $d.Output -notmatch 'unable to find') { throw "Registry delete failed: $($d.Output)" }
    $i = Invoke-LcReg @('import', $Path)
    if ($i.ExitCode -ne 0) { throw "Registry import failed: $($i.Output)" }
}

# One-time capture of the copy's as-copied loader and mod state (a byte copy of
# the owner's installation), so the owner's 0.33.0 set and mods are re-applied exactly.
function Initialize-LcGameCopy($Paths) {
    if (-not (Test-Path -LiteralPath $Paths.Pristine)) {
        $building = "$($Paths.Pristine).building"
        if (Test-Path -LiteralPath $building) { Remove-Item -LiteralPath $building -Recurse -Force }
        New-Item -ItemType Directory -Path $building | Out-Null
        Copy-Item -LiteralPath $Paths.UmmDir -Destination (Join-Path $building 'UnityModManager') -Recurse
        Copy-Item -LiteralPath $Paths.ModsDir -Destination (Join-Path $building 'Mods') -Recurse
        foreach ($name in @('winhttp.dll', 'doorstop_config.ini')) { Copy-Item -LiteralPath (Join-Path $Paths.InstallDir $name) -Destination $building }
        Move-Item -LiteralPath $building -Destination $Paths.Pristine
    }
    Set-Content -LiteralPath (Join-Path $Paths.InstallDir 'steam_appid.txt') -Value '640820' -Encoding ASCII -NoNewline
}

function Resolve-LcComponent($File) {
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    New-Item -ItemType Directory -Force -Path $script:LcComponentCache | Out-Null
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $urlKey = -join ($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($File.url)) | ForEach-Object { $_.ToString('x2') }) } finally { $sha.Dispose() }
    $download = Join-Path $script:LcComponentCache $urlKey
    if (-not (Test-Path -LiteralPath $download)) { Invoke-WebRequest -Uri $File.url -OutFile $download -UseBasicParsing }
    $entry = $File.PSObject.Properties['archiveEntry']
    if ($null -eq $entry -or -not $entry.Value) { return $download }
    if ((Get-LcSha256 $download) -ne $File.archiveSha256) { throw "Archive hash mismatch: $($File.url)" }
    $extracted = Join-Path $script:LcComponentCache ($File.sha256 + '.bin')
    if (-not (Test-Path -LiteralPath $extracted)) {
        $archive = [IO.Compression.ZipFile]::OpenRead($download)
        try { [IO.Compression.ZipFileExtensions]::ExtractToFile($archive.GetEntry($entry.Value), $extracted, $true) } finally { $archive.Dispose() }
    }
    return $extracted
}

# Clean install: the loader directory, proxy and proxy configuration are removed
# and rebuilt from one declared file set, never a mixture of old and new files.
function Install-LcLoaderProfile($Paths, [string]$ProfileName) {
    $document = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'loader-profiles.json') -Raw | ConvertFrom-Json
    $installed = [ordered]@{ profile = $ProfileName; files = [ordered]@{} }
    Remove-Item -LiteralPath $Paths.UmmDir -Recurse -Force
    foreach ($name in @('winhttp.dll', 'doorstop_config.ini')) { Remove-Item -LiteralPath (Join-Path $Paths.InstallDir $name) -Force }
    if ($ProfileName -eq 'owner-0.33.0') {
        $installed.method = 'byte copy of the owner installation (UMM 0.33.0 Nexus distribution, Harmony 2.3.6)'
        Copy-Item -LiteralPath (Join-Path $Paths.Pristine 'UnityModManager') -Destination $Paths.UmmDir -Recurse
        foreach ($name in @('winhttp.dll', 'doorstop_config.ini')) { Copy-Item -LiteralPath (Join-Path $Paths.Pristine $name) -Destination $Paths.InstallDir }
        foreach ($name in @('Log.txt', 'Params.xml')) { Remove-Item -LiteralPath (Join-Path $Paths.UmmDir $name) -Force -ErrorAction SilentlyContinue }
        foreach ($p in $document.owner033.expected.PSObject.Properties) {
            if ((Get-LcSha256 (Join-Path $Paths.UmmDir $p.Name)) -ne $p.Value) { throw "owner-0.33.0 file differs from its recorded identity: $($p.Name)" }
        }
    } else {
        $property = $document.profiles.PSObject.Properties[$ProfileName]
        if (-not $property) { throw "Unknown loader profile: $ProfileName" }
        $definition = $property.Value
        $installed.method = 'component-assembled from upstream NuGet packages and the UMM repository; not the Nexus installer ZIP'
        $installed.ummVersion = $definition.ummVersion
        $installed.harmonyVersion = $definition.harmonyVersion
        New-Item -ItemType Directory -Path $Paths.UmmDir | Out-Null
        Copy-Item -LiteralPath (Join-Path $Paths.Pristine 'UnityModManager\Config.xml') -Destination $Paths.UmmDir
        foreach ($entry in $definition.files.PSObject.Properties) {
            $source = Resolve-LcComponent $entry.Value
            if ((Get-LcSha256 $source) -ne $entry.Value.sha256) { throw "Component hash mismatch for $($entry.Name)" }
            Copy-Item -LiteralPath $source -Destination (Join-Path $Paths.InstallDir ($entry.Name -replace '/', '\')) -Force
        }
        [IO.File]::WriteAllText((Join-Path $Paths.InstallDir 'doorstop_config.ini'), $document.doorstopConfig, [Text.Encoding]::ASCII)
    }
    foreach ($file in @(Get-ChildItem -LiteralPath $Paths.UmmDir -File)) { $installed.files[$file.Name] = Get-LcSha256 $file.FullName }
    foreach ($name in @('winhttp.dll', 'doorstop_config.ini')) { $installed.files[$name] = Get-LcSha256 (Join-Path $Paths.InstallDir $name) }
    return $installed
}

# Rebuilds the copy's Mods folder: the named owner mods from the pristine set,
# the candidate exactly as a user installs it (the ZIP's single mod folder),
# and the test-only harness.
function Set-LcGameCopyMods($Paths, [string[]]$OwnerMods, [string]$CandidateZip, [string]$HarnessDir) {
    if (Test-Path -LiteralPath $Paths.ModsDir) { Remove-Item -LiteralPath $Paths.ModsDir -Recurse -Force }
    New-Item -ItemType Directory -Path $Paths.ModsDir | Out-Null
    $staged = [ordered]@{}
    foreach ($name in @($OwnerMods | Where-Object { $_ })) {
        if ($name -eq 'KingmakerGunslinger') { throw 'The product comes from the candidate ZIP, never the owner set.' }
        $source = Join-Path $Paths.Pristine "Mods\$name"
        if (-not (Test-Path -LiteralPath $source)) { throw "Mod not found in the pristine set: $name" }
        Copy-Item -LiteralPath $source -Destination $Paths.ModsDir -Recurse
        $info = Get-Content -LiteralPath (Join-Path $source 'Info.json') -Raw | ConvertFrom-Json
        $dll = Get-ChildItem -LiteralPath $source -Filter '*.dll' -File | Where-Object { $_.Name -eq $(if ($info.PSObject.Properties['AssemblyName'] -and $info.AssemblyName) { $info.AssemblyName } else { "$name.dll" }) }
        # ManagerVersion is optional in a mod's Info.json.
        $staged[$name] = [ordered]@{ version = $info.Version
            managerVersion = $(if ($info.PSObject.Properties['ManagerVersion']) { $info.ManagerVersion })
            dllSha256 = $(if ($dll) { Get-LcSha256 $dll.FullName }) }
    }
    Expand-Archive -LiteralPath $CandidateZip -DestinationPath $Paths.ModsDir
    $staged['KingmakerGunslinger'] = [ordered]@{ zipSha256 = Get-LcSha256 $CandidateZip
        dllSha256 = Get-LcSha256 (Join-Path $Paths.ModsDir 'KingmakerGunslinger\KingmakerGunslinger.dll')
        managerVersion = (Get-Content -LiteralPath (Join-Path $Paths.ModsDir 'KingmakerGunslinger\Info.json') -Raw | ConvertFrom-Json).ManagerVersion }
    $harnessTarget = Join-Path $Paths.ModsDir 'KmgLoaderCompatHarness'
    New-Item -ItemType Directory -Path $harnessTarget | Out-Null
    Copy-Item -LiteralPath (Join-Path $HarnessDir 'KmgLoaderCompatHarness.dll') -Destination $harnessTarget
    Copy-Item -LiteralPath (Join-Path $HarnessDir 'Info.json') -Destination $harnessTarget
    $staged['KmgLoaderCompatHarness'] = [ordered]@{ dllSha256 = Get-LcSha256 (Join-Path $harnessTarget 'KmgLoaderCompatHarness.dll') }
    return $staged
}

function Start-LcKingmakerCopy($Paths, [string[]]$Arguments) {
    $exe = Join-Path $Paths.InstallDir 'Kingmaker.exe'
    $quoted = @('-popupwindow', '-screen-width', '1280', '-screen-height', '720') + ($Arguments | ForEach-Object { if ($_ -match '\s') { '"' + $_ + '"' } else { $_ } })
    return Start-Process -FilePath $exe -WorkingDirectory $Paths.InstallDir -ArgumentList ($quoted -join ' ') -PassThru
}

# Stops only the process this session started, and only if it runs from the copy.
function Stop-LcOwnedProcess($Process, $Paths) {
    if ($Process.HasExited) { return 'already-exited' }
    $path = $null
    try { $path = $Process.MainModule.FileName } catch { }
    if (-not $path -or -not $path.StartsWith($Paths.InstallDir, [StringComparison]::OrdinalIgnoreCase)) { throw "Refusing to stop process $($Process.Id): not the game copy ($path)." }
    $Process.CloseMainWindow() | Out-Null
    if (-not $Process.WaitForExit(20000)) { $Process.Refresh(); if (-not $Process.HasExited) { $Process.Kill(); $Process.WaitForExit(20000) | Out-Null } }
    return 'stopped-after-timeout'
}
