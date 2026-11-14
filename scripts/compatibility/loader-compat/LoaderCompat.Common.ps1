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

function Get-LcSha256([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }

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
    if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) { throw 'Kingmaker is already running; refusing to take over another session.' }
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
        $staged[$name] = [ordered]@{ version = $info.Version; managerVersion = $info.ManagerVersion; dllSha256 = $(if ($dll) { Get-LcSha256 $dll.FullName }) }
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
