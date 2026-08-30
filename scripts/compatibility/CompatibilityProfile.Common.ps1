Set-StrictMode -Version Latest

function Get-KmgCompatibilityRepositoryRoot {
    return (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
}

function Get-KmgCompatibilitySha256([string]$Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-KmgCompatibilityRelativePath([string]$Root, [string]$Path) {
    $rootUri = [Uri]([IO.Path]::GetFullPath($Root).TrimEnd('\') + '\')
    return [Uri]::UnescapeDataString($rootUri.MakeRelativeUri([Uri][IO.Path]::GetFullPath($Path)).ToString()).Replace('/', '\')
}

function Get-KmgCompatibilityDirectoryManifest([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { throw "Directory does not exist: $Path" }
    $root = (Resolve-Path -LiteralPath $Path).Path
    $directories = @(Get-ChildItem -LiteralPath $root -Directory -Recurse -Force | Sort-Object FullName | ForEach-Object {
        [ordered]@{ path = Get-KmgCompatibilityRelativePath $root $_.FullName; kind = 'directory' }
    })
    $files = @(Get-ChildItem -LiteralPath $root -File -Recurse -Force | Sort-Object FullName | ForEach-Object {
        [ordered]@{
            path = Get-KmgCompatibilityRelativePath $root $_.FullName
            kind = 'file'
            length = $_.Length
            lastWriteTimeUtc = $_.LastWriteTimeUtc.ToString('o')
            sha256 = Get-KmgCompatibilitySha256 $_.FullName
        }
    })
    return @([ordered]@{ path = '.'; kind = 'directory' }) + $directories + $files
}

function Test-KmgCompatibilityManifestEqual($Expected, $Actual) {
    return (($Expected | ConvertTo-Json -Depth 6 -Compress) -ceq
        ($Actual | ConvertTo-Json -Depth 6 -Compress))
}

function Assert-KmgCompatibilityChildPath([string]$Path, [string]$Root) {
    $fullRoot = [IO.Path]::GetFullPath($Root).TrimEnd('\')
    $fullPath = [IO.Path]::GetFullPath($Path).TrimEnd('\')
    if (-not $fullPath.StartsWith($fullRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path escapes approved root '$fullRoot': $fullPath"
    }
    return $fullPath
}

function Read-KmgCompatibilityJson([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "JSON file is missing: $Path" }
    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Get-KmgCompatibilityAssemblyRecord([string]$Path) {
    $file = Get-Item -LiteralPath $Path
    $name = [Reflection.AssemblyName]::GetAssemblyName($file.FullName)
    $assembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($file.FullName)
    return [ordered]@{
        fileName = $file.Name
        assemblyName = $name.Name
        assemblyVersion = $name.Version.ToString()
        fullIdentity = $name.FullName
        mvid = $assembly.ManifestModule.ModuleVersionId.ToString('D')
        length = $file.Length
        sha256 = Get-KmgCompatibilitySha256 $file.FullName
    }
}

function Resolve-KmgCompatibilityProfile {
    param(
        [Parameter(Mandatory = $true)][string]$ProfileId,
        [Parameter(Mandatory = $true)][string]$ReferenceRoot,
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$KingmakerInstallDir,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot
    )
    $catalog = Read-KmgCompatibilityJson (Join-Path $RepositoryRoot 'compatibility\reference-catalog.json')
    $profiles = Read-KmgCompatibilityJson (Join-Path $RepositoryRoot 'compatibility\profiles.json')
    $profileMatches = @($profiles.profiles | Where-Object id -ceq $ProfileId)
    if ($profileMatches.Count -ne 1) { throw "Profile ID must resolve exactly once: $ProfileId" }
    $profile = $profileMatches[0]
    $reference = (Resolve-Path -LiteralPath $ReferenceRoot).Path
    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    $packageHash = Get-KmgCompatibilitySha256 $package
    $runtimeMods = @()
    $staticOnly = @()
    $unavailable = @()
    $warnings = @()
    foreach ($key in @($profile.modKeys)) {
        $catalogMatches = @($catalog.references | Where-Object key -ceq $key)
        if ($catalogMatches.Count -ne 1) { throw "Logical mod key must resolve exactly once: $key" }
        $entry = $catalogMatches[0]
        if ($entry.availabilityDisposition -ceq 'UNAVAILABLE-LOCAL-REFERENCE') {
            $unavailable += $key
            continue
        }
        $aliases = @($entry.folderAliases)
        $existing = @($aliases | ForEach-Object {
            $candidate = Assert-KmgCompatibilityChildPath (Join-Path $reference $_) $reference
            if (Test-Path -LiteralPath $candidate -PathType Container) { $candidate }
        })
        if (-not $entry.runtimeStagingAllowed) {
            $staticOnly += [ordered]@{ key = $key; paths = $existing; disposition = if ($entry.role -eq 'future-extension') { 'UNAVAILABLE-LOCAL-REFERENCE' } else { 'STATIC-AUDITED-ONLY' } }
            continue
        }
        if ($existing.Count -ne $aliases.Count -or $existing.Count -eq 0) {
            throw "Runtime key '$key' did not resolve every exact alias. Expected $($aliases.Count), found $($existing.Count)."
        }
        foreach ($source in $existing) {
            $infoPath = Join-Path $source 'Info.json'
            if (-not (Test-Path -LiteralPath $infoPath -PathType Leaf)) {
                $lowerInfo = Join-Path $source 'info.json'
                if (-not (Test-Path -LiteralPath $lowerInfo -PathType Leaf)) { throw "Runtime mod Info.json is missing: $source" }
                $infoPath = $lowerInfo
            }
            $info = Read-KmgCompatibilityJson $infoPath
            $dlls = @(Get-ChildItem -LiteralPath $source -File -Filter '*.dll')
            $declared = if ($info.PSObject.Properties['AssemblyName']) { [string]$info.AssemblyName } else { $null }
            $assemblyFile = @(if ($declared) { $dlls | Where-Object Name -ceq $declared } else { $dlls })
            if ($assemblyFile.Count -ne 1) { throw "Runtime mod assembly is missing or ambiguous: $source" }
            $runtimeMods += [ordered]@{
                key = $key
                sourceDirectory = $source
                destinationName = Split-Path -Leaf $source
                ummId = [string]$info.Id
                displayName = [string]$info.DisplayName
                version = [string]$info.Version
                managerVersion = [string]$info.ManagerVersion
                entryMethod = [string]$info.EntryMethod
                assembly = Get-KmgCompatibilityAssemblyRecord $assemblyFile[0].FullName
                sourceManifest = Get-KmgCompatibilityDirectoryManifest $source
            }
        }
    }
    $observedIds = @('KingmakerGunslinger') + @($runtimeMods | ForEach-Object ummId)
    if (@($observedIds | Sort-Object -Unique).Count -ne $observedIds.Count) { throw 'Resolved profile contains duplicate UMM IDs.' }
    $expected = @($profile.expectedUmmIds)
    if ($profile.runtimeLoadableRequired -and (Compare-Object -ReferenceObject $expected -DifferenceObject $observedIds)) {
        throw "Resolved UMM IDs do not match the committed profile: $ProfileId"
    }
    return [ordered]@{
        schemaVersion = 1
        profileId = $ProfileId
        description = $profile.description
        runtimeCapable = [bool]$profile.runtimeLoadableRequired -and $unavailable.Count -eq 0 -and $staticOnly.Count -eq 0
        gunslinger = [ordered]@{ packagePath = $package; version = '0.0.110'; packageSha256 = $packageHash; ummId = 'KingmakerGunslinger' }
        runtimeMods = $runtimeMods
        staticOnlyReferences = $staticOnly
        unavailableReferences = $unavailable
        expectedUmmIds = $expected
        expectedLoadOrder = $observedIds
        intendedModsDirectory = Join-Path ([IO.Path]::GetFullPath($KingmakerInstallDir).TrimEnd('\')) 'Mods'
        scenarios = @($profile.scenarios)
        conflicts = @()
        warnings = $warnings
        workingSaveSmokePermitted = [bool]$profile.workingSaveSmokePermitted
    }
}

function Assert-KmgCompatibilityInstallRoot {
    param([Parameter(Mandatory = $true)][string]$KingmakerInstallDir, [switch]$FixtureMode)
    if (-not (Test-Path -LiteralPath $KingmakerInstallDir -PathType Container)) { throw "Kingmaker install root is missing: $KingmakerInstallDir" }
    $root = (Resolve-Path -LiteralPath $KingmakerInstallDir).Path.TrimEnd('\')
    if (-not $FixtureMode) {
        $expected = [IO.Path]::GetFullPath('C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker').TrimEnd('\')
        if (-not $root.Equals($expected, [StringComparison]::OrdinalIgnoreCase)) { throw "Unsupported Kingmaker install root: $root" }
    }
    foreach ($relative in @('Kingmaker.exe', 'Kingmaker_Data\Managed\Assembly-CSharp.dll')) {
        if (-not (Test-Path -LiteralPath (Join-Path $root $relative) -PathType Leaf)) { throw "Kingmaker install identity file is missing: $relative" }
    }
    return $root
}

function Assert-KmgCompatibilityNoKingmakerProcess([int[]]$KnownProcessIds) {
    $ids = @(if ($PSBoundParameters.ContainsKey('KnownProcessIds')) { $KnownProcessIds } else { Get-Process -Name Kingmaker -ErrorAction SilentlyContinue | ForEach-Object Id })
    if ($ids.Count -gt 0) { throw "Pathfinder: Kingmaker is running (PID(s): $($ids -join ', '))." }
}

function Write-KmgCompatibilityJsonAtomic([string]$Path, $Value) {
    $directory = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
    $temporary = Join-Path $directory ('.' + [IO.Path]::GetFileName($Path) + '.' + [Guid]::NewGuid().ToString('N') + '.tmp')
    $Value | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $temporary -Encoding UTF8
    Move-Item -LiteralPath $temporary -Destination $Path -Force
}

function Get-KmgCompatibilityActiveTransactions([string]$StateRoot) {
    if (-not (Test-Path -LiteralPath $StateRoot -PathType Container)) { return @() }
    return @(Get-ChildItem -LiteralPath $StateRoot -Directory -Force | ForEach-Object {
        $statePath = Join-Path $_.FullName 'transaction.json'
        if (Test-Path -LiteralPath $statePath -PathType Leaf) {
            $state = Read-KmgCompatibilityJson $statePath
            if ($state.status -in @('Preparing', 'Active', 'Restoring', 'RestorationFailed')) { $state }
        }
    })
}

function Acquire-KmgCompatibilityLock([string]$StateRoot, [string]$RunId) {
    New-Item -ItemType Directory -Path $StateRoot -Force | Out-Null
    $lockPath = Join-Path $StateRoot 'compatibility.lock'
    try {
        $stream = [IO.File]::Open($lockPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
        try {
            $bytes = [Text.Encoding]::UTF8.GetBytes($RunId + "`r`n")
            $stream.Write($bytes, 0, $bytes.Length)
            $stream.Flush($true)
        } finally { $stream.Dispose() }
    } catch [IO.IOException] { throw "A compatibility transaction lock already exists: $lockPath" }
    return $lockPath
}

function Remove-KmgCompatibilityOwnedLock([string]$LockPath, [string]$RunId) {
    if (-not (Test-Path -LiteralPath $LockPath -PathType Leaf)) { return }
    $owner = (Get-Content -LiteralPath $LockPath -Raw).Trim()
    if ($owner -cne $RunId) { throw "Compatibility lock is not owned by run $RunId." }
    Remove-Item -LiteralPath $LockPath -Force
}

function Expand-KmgCompatibilityGunslingerPackage([string]$PackagePath, [string]$Destination) {
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    Expand-Archive -LiteralPath $PackagePath -DestinationPath $Destination -Force
    $roots = @(Get-ChildItem -LiteralPath $Destination -Directory)
    if ($roots.Count -ne 1 -or $roots[0].Name -cne 'KingmakerGunslinger') { throw 'Gunslinger package must contain exactly one KingmakerGunslinger root.' }
    return $roots[0].FullName
}

function Enter-KmgCompatibilityTransaction {
    param(
        [Parameter(Mandatory = $true)]$Resolution,
        [Parameter(Mandatory = $true)][string]$KingmakerInstallDir,
        [Parameter(Mandatory = $true)][string]$StateRoot,
        [Parameter(Mandatory = $true)][ValidatePattern('^[A-Za-z0-9._-]{1,100}$')][string]$RunId,
        [switch]$FixtureMode,
        [int[]]$KnownKingmakerProcessIds
    )
    if (-not $Resolution.runtimeCapable) { throw "Profile is not runtime-capable: $($Resolution.profileId)" }
    if ($PSBoundParameters.ContainsKey('KnownKingmakerProcessIds')) { Assert-KmgCompatibilityNoKingmakerProcess -KnownProcessIds $KnownKingmakerProcessIds } else { Assert-KmgCompatibilityNoKingmakerProcess }
    $install = Assert-KmgCompatibilityInstallRoot -KingmakerInstallDir $KingmakerInstallDir -FixtureMode:$FixtureMode
    $stateRootFull = [IO.Path]::GetFullPath($StateRoot).TrimEnd('\')
    $active = @(Get-KmgCompatibilityActiveTransactions $stateRootFull)
    if ($active.Count -gt 0) { throw "Unresolved compatibility transaction exists: $($active[0].runId)" }
    $runRoot = Join-Path $stateRootFull $RunId
    if (Test-Path -LiteralPath $runRoot) { throw "Transaction run directory already exists: $runRoot" }
    $lockPath = Acquire-KmgCompatibilityLock $stateRootFull $RunId
    $mods = Join-Path $install 'Mods'
    $backup = Join-Path $install ("Mods.kmg-compat-$RunId.original")
    $quarantine = Join-Path $install ("Mods.kmg-compat-$RunId.staged")
    if ((Test-Path -LiteralPath $backup) -or (Test-Path -LiteralPath $quarantine)) {
        Remove-KmgCompatibilityOwnedLock $lockPath $RunId
        throw 'Transaction destination collision; no filesystem mutation was performed.'
    }
    New-Item -ItemType Directory -Path $runRoot | Out-Null
    $statePath = Join-Path $runRoot 'transaction.json'
    $originalExisted = Test-Path -LiteralPath $mods -PathType Container
    $before = if ($originalExisted) { @(Get-KmgCompatibilityDirectoryManifest $mods) } else { @() }
    $bankPath = Join-Path $install 'Kingmaker_Data\StreamingAssets\Audio\GeneratedSoundBanks\Windows\KMG_Firearms.bnk'
    $bankExisted = Test-Path -LiteralPath $bankPath -PathType Leaf
    $bankBackup = Join-Path $runRoot 'KMG_Firearms.bnk.original'
    $bank = [ordered]@{ path = $bankPath; existed = $bankExisted; sha256 = $null; length = $null; lastWriteTimeUtc = $null; backupPath = $bankBackup }
    if ($bankExisted) {
        $bankFile = Get-Item -LiteralPath $bankPath
        $bank.sha256 = Get-KmgCompatibilitySha256 $bankPath
        $bank.length = $bankFile.Length
        $bank.lastWriteTimeUtc = $bankFile.LastWriteTimeUtc.ToString('o')
        Copy-Item -LiteralPath $bankPath -Destination $bankBackup
    }
    $state = [ordered]@{
        schemaVersion = 1; runId = $RunId; profileId = $Resolution.profileId; status = 'Preparing'
        installRoot = $install; modsPath = $mods; originalExisted = $originalExisted
        originalBackupPath = $backup; stagedQuarantinePath = $quarantine; lockPath = $lockPath
        originalManifest = $before; stagedManifest = @(); stagedMutationObserved = $false
        managedSoundBank = $bank; createdAtUtc = [DateTime]::UtcNow.ToString('o')
        restoredAtUtc = $null; restorationVerified = $false; recoveryInstructions = "Run Restore-KingmakerCompatibilityProfile.ps1 -RunId $RunId -StateRoot `"$stateRootFull`"."
    }
    Write-KmgCompatibilityJsonAtomic $statePath $state
    $ownershipPath = Join-Path $runRoot 'ownership-state.json'
    $ownership = [ordered]@{ schemaVersion = 1; runId = $RunId; profileId = $Resolution.profileId; installRoot = $install; modsPath = $mods; originalBackupPath = $backup; stagedQuarantinePath = $quarantine }
    Write-KmgCompatibilityJsonAtomic $ownershipPath $ownership
    $ownershipHash = Get-KmgCompatibilitySha256 $ownershipPath
    try {
        if ($originalExisted) { Move-Item -LiteralPath $mods -Destination $backup }
        New-Item -ItemType Directory -Path $mods | Out-Null
        $preparationSentinel = [ordered]@{ schemaVersion = 1; runId = $RunId; stateFile = $ownershipPath; stateSha256 = $ownershipHash }
        Write-KmgCompatibilityJsonAtomic (Join-Path $mods '.kmg-compat-sentinel.json') $preparationSentinel
        $packageScratch = Join-Path $runRoot 'package'
        $gunslingerSource = Expand-KmgCompatibilityGunslingerPackage $Resolution.gunslinger.packagePath $packageScratch
        Copy-Item -LiteralPath $gunslingerSource -Destination (Join-Path $mods 'KingmakerGunslinger') -Recurse
        foreach ($mod in @($Resolution.runtimeMods)) {
            $source = [IO.Path]::GetFullPath([string]$mod.sourceDirectory)
            $destinationName = [string]$mod.destinationName
            if ([string]::IsNullOrWhiteSpace($destinationName) -or $destinationName.IndexOfAny([IO.Path]::GetInvalidFileNameChars()) -ge 0) { throw "Invalid staged destination name: $destinationName" }
            $destination = Join-Path $mods $destinationName
            if (Test-Path -LiteralPath $destination) { throw "Staged destination collision: $destinationName" }
            Copy-Item -LiteralPath $source -Destination $destination -Recurse
            $copied = @(Get-KmgCompatibilityDirectoryManifest $destination)
            if (-not (Test-KmgCompatibilityManifestEqual @($mod.sourceManifest) $copied)) { throw "Copied mod hash manifest mismatch: $destinationName" }
        }
        $state.status = 'Active'
        Write-KmgCompatibilityJsonAtomic $statePath $state
        $sentinel = [ordered]@{ schemaVersion = 1; runId = $RunId; stateFile = $ownershipPath; stateSha256 = $ownershipHash }
        Write-KmgCompatibilityJsonAtomic (Join-Path $mods '.kmg-compat-sentinel.json') $sentinel
        $state.stagedManifest = @(Get-KmgCompatibilityDirectoryManifest $mods)
        Write-KmgCompatibilityJsonAtomic $statePath $state
        return $state
    } catch {
        $enterError = $_
        try { Restore-KmgCompatibilityTransaction -RunId $RunId -StateRoot $stateRootFull -FixtureMode:$FixtureMode | Out-Null } catch { throw "Profile entry failed: $($enterError.Exception.Message) Restoration also failed: $($_.Exception.Message)" }
        throw $enterError
    }
}

function Restore-KmgCompatibilityTransaction {
    param(
        [Parameter(Mandatory = $true)][string]$RunId,
        [Parameter(Mandatory = $true)][string]$StateRoot,
        [switch]$FixtureMode,
        [int[]]$KnownKingmakerProcessIds
    )
    if ($PSBoundParameters.ContainsKey('KnownKingmakerProcessIds')) { Assert-KmgCompatibilityNoKingmakerProcess -KnownProcessIds $KnownKingmakerProcessIds } else { Assert-KmgCompatibilityNoKingmakerProcess }
    $statePath = Join-Path ([IO.Path]::GetFullPath($StateRoot).TrimEnd('\')) "$RunId\transaction.json"
    $state = Read-KmgCompatibilityJson $statePath
    if ($state.runId -cne $RunId) { throw 'Transaction state RunId mismatch.' }
    if ($state.status -ceq 'Restored' -and $state.restorationVerified) { return $state }
    $install = Assert-KmgCompatibilityInstallRoot -KingmakerInstallDir $state.installRoot -FixtureMode:$FixtureMode
    $mods = [string]$state.modsPath; $backup = [string]$state.originalBackupPath; $quarantine = [string]$state.stagedQuarantinePath
    foreach ($path in @($mods, $backup, $quarantine)) {
        $parent = Split-Path -Parent $path
        if (-not $parent.Equals($install, [StringComparison]::OrdinalIgnoreCase)) { throw "Transaction path escaped install root: $path" }
    }
    $state.status = 'Restoring'; Write-KmgCompatibilityJsonAtomic $statePath $state
    try {
        if (Test-Path -LiteralPath $mods -PathType Container) {
            $sentinelPath = Join-Path $mods '.kmg-compat-sentinel.json'
            if (-not (Test-Path -LiteralPath $sentinelPath -PathType Leaf)) { throw 'Current Mods directory lacks the active transaction sentinel.' }
            $sentinel = Read-KmgCompatibilityJson $sentinelPath
            if ($sentinel.runId -cne $RunId) { throw 'Current Mods sentinel does not belong to this transaction.' }
            if (-not (Test-Path -LiteralPath $sentinel.stateFile -PathType Leaf) -or
                (Get-KmgCompatibilitySha256 $sentinel.stateFile) -cne $sentinel.stateSha256) {
                throw 'Current Mods sentinel ownership-state hash mismatch.'
            }
            if (Test-Path -LiteralPath $quarantine) { throw 'Staged quarantine destination already exists.' }
            $currentStaged = @(Get-KmgCompatibilityDirectoryManifest $mods)
            $state.stagedMutationObserved = -not (Test-KmgCompatibilityManifestEqual @($state.stagedManifest) $currentStaged)
            Move-Item -LiteralPath $mods -Destination $quarantine
        }
        if ($state.originalExisted) {
            if (-not (Test-Path -LiteralPath $backup -PathType Container)) { throw 'Original Mods backup is missing.' }
            Move-Item -LiteralPath $backup -Destination $mods
            $after = @(Get-KmgCompatibilityDirectoryManifest $mods)
            if (-not (Test-KmgCompatibilityManifestEqual @($state.originalManifest) $after)) { throw 'Restored Mods manifest/hash mismatch.' }
        } elseif (Test-Path -LiteralPath $mods) { throw 'Mods must remain absent because it was absent before the transaction.' }
        $bank = $state.managedSoundBank
        if ($bank.existed) {
            if (-not (Test-Path -LiteralPath $bank.backupPath -PathType Leaf)) { throw 'Managed SoundBank backup is missing.' }
            New-Item -ItemType Directory -Path (Split-Path -Parent $bank.path) -Force | Out-Null
            Copy-Item -LiteralPath $bank.backupPath -Destination $bank.path -Force
            (Get-Item -LiteralPath $bank.path).LastWriteTimeUtc = [DateTime]::Parse($bank.lastWriteTimeUtc).ToUniversalTime()
            if ((Get-KmgCompatibilitySha256 $bank.path) -cne $bank.sha256) { throw 'Managed SoundBank restoration hash mismatch.' }
        } elseif (Test-Path -LiteralPath $bank.path -PathType Leaf) { Remove-Item -LiteralPath $bank.path -Force }
        if (Test-Path -LiteralPath $quarantine -PathType Container) {
            $quarantineSentinel = Read-KmgCompatibilityJson (Join-Path $quarantine '.kmg-compat-sentinel.json')
            if ($quarantineSentinel.runId -cne $RunId) { throw 'Quarantined staged directory sentinel mismatch.' }
            if ((Get-KmgCompatibilitySha256 $quarantineSentinel.stateFile) -cne $quarantineSentinel.stateSha256) { throw 'Quarantined sentinel ownership-state hash mismatch.' }
            Remove-Item -LiteralPath $quarantine -Recurse -Force
        }
        $state.status = 'Restored'; $state.restorationVerified = $true; $state.restoredAtUtc = [DateTime]::UtcNow.ToString('o')
        Write-KmgCompatibilityJsonAtomic $statePath $state
        Remove-KmgCompatibilityOwnedLock $state.lockPath $RunId
        return $state
    } catch {
        $state.status = 'RestorationFailed'; $state.restorationVerified = $false
        Write-KmgCompatibilityJsonAtomic $statePath $state
        throw "Compatibility restoration failed closed. $($state.recoveryInstructions) Cause: $($_.Exception.Message)"
    }
}
