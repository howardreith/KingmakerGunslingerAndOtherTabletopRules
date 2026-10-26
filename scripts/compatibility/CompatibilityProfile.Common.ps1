Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot '..\common.ps1')

function Get-KmgCompatibilityRepositoryRoot {
    return (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
}

function Get-KmgCompatibilitySha256([string]$Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

# The new optional provider is authorized only for its declared save-free
# scenarios. Read that existing profile declaration rather than copying its
# scenario list into another admission layer. Other profiles keep their scope.
function Assert-KmgSuggestionProfileSelection {
    param([string]$ProfileId, [string[]]$Scenario, [string]$PackagePath,
        [string]$ReferenceRoot, [string]$RepositoryRoot)
    if ($ProfileId -cne 'gunslinger-cowwithhat-suggestion') { return }
    $profiles = Read-KmgCompatibilityJson (Join-Path $RepositoryRoot 'compatibility/profiles.json')
    $matches = @($profiles.profiles | Where-Object id -ceq $ProfileId)
    if ($matches.Count -ne 1 -or $matches[0].requiredGunslingerPackage -cne 'explicit-validated-candidate' -or
        $matches[0].workingSaveSmokePermitted -or @($Scenario).Count -eq 0 -or
        @($Scenario | Where-Object { $_ -cnotin @($matches[0].scenarios) }).Count -gt 0) {
        throw 'Optional Suggestion profile requires its declared save-free scenario selection; no profile entered.'
    }
    if ([string]::IsNullOrWhiteSpace($PackagePath) -or [string]::IsNullOrWhiteSpace($ReferenceRoot)) {
        throw 'Optional Suggestion profile requires explicit candidate and isolated reference root; no profile entered.'
    }
    Assert-KmgSuggestionReferenceRoot -ReferenceRoot $ReferenceRoot -RepositoryRoot $RepositoryRoot
}

function Assert-KmgSuggestionReferenceRoot {
    param([string]$ReferenceRoot,[string]$RepositoryRoot)
    if([string]::IsNullOrWhiteSpace($ReferenceRoot)){throw 'Optional Suggestion requires an explicit isolated reference root.'}
    $profiles = Read-KmgCompatibilityJson (Join-Path $RepositoryRoot 'compatibility/profiles.json')
    $matches = @($profiles.profiles | Where-Object id -ceq 'gunslinger-cowwithhat-suggestion')
    if($matches.Count -ne 1){throw 'Optional Suggestion reference profile is ambiguous.'}
    $references = [IO.Path]::GetFullPath($ReferenceRoot).TrimEnd('\')
    $isolated = [IO.Path]::GetFullPath((Join-Path $RepositoryRoot 'artifacts')).TrimEnd('\') + '\'
    if (-not $references.StartsWith($isolated, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Optional Suggestion reference root must be isolated under this worktree artifacts; no profile entered.'
    }
    for ($parent = $references; $parent.Length -ge $isolated.TrimEnd('\').Length; $parent = Split-Path -Parent $parent) {
        if ((Test-Path -LiteralPath $parent) -and
            ((Get-Item -LiteralPath $parent -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw 'Optional Suggestion reference root contains a reparse point; no profile entered.'
        }
    }
    $catalog = Read-KmgCompatibilityJson (Join-Path $RepositoryRoot 'compatibility/reference-catalog.json')
    foreach ($key in $matches[0].modKeys) {
        $entries = @($catalog.references | Where-Object key -ceq $key)
        if ($entries.Count -ne 1) { throw 'Optional Suggestion reference key is ambiguous; no profile entered.' }
        foreach ($alias in $entries[0].folderAliases) {
            $source = Assert-KmgCompatibilityChildPath (Join-Path $references $alias) $references
            if (-not (Test-Path -LiteralPath $source)) { continue } # resolver reports missing references
            $pending = [Collections.Generic.Queue[IO.FileSystemInfo]]::new()
            $pending.Enqueue((Get-Item -LiteralPath $source -Force))
            while ($pending.Count -gt 0) {
                $item = $pending.Dequeue()
                if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                    throw 'Optional Suggestion selected reference tree contains a reparse point; no profile entered.'
                }
                # Check a directory before descending: never enumerate a
                # linked tree even when this is only an admission read.
                if ($item.PSIsContainer) {
                    foreach ($child in @(Get-ChildItem -LiteralPath $item.FullName -Force)) { $pending.Enqueue($child) }
                }
            }
        }
    }
}

# Fixed optional overlay on the full current intake. This resolver only reads;
# existing Enter/Restore-KmgCompatibilityTransaction owns every mutation.
function Assert-KmgSprint32SuggestionReferenceIdentity {
    param($Resolution,[string]$RepositoryRoot)
    $profiles=Read-KmgCompatibilityJson (Join-Path $RepositoryRoot 'compatibility/profiles.json')
    $profile=@($profiles.profiles|Where-Object id -ceq 'gunslinger-cowwithhat-suggestion')
    if($profile.Count -ne 1 -or $Resolution.profileId -cne $profile[0].id) {throw 'Sprint32 provider reference profile differs.'}
    $expected=@($profile[0].sprint32WorkingOverlayReferences)
    if($expected.Count -ne 2 -or @($Resolution.runtimeMods).Count -ne $expected.Count) {throw 'Sprint32 investigated reference count differs.'}
    foreach($identity in $expected) {
        $actual=@($Resolution.runtimeMods|Where-Object ummId -ceq $identity.ummId)
        if($actual.Count -ne 1 -or $actual[0].version -cne $identity.version -or
           $actual[0].assembly.assemblyName -cne $identity.assemblyName -or $actual[0].assembly.sha256 -cne $identity.sha256) {
            throw 'Sprint32 provider/dependency differs from the investigated reference identity; no transaction entered.'
        }
    }
}

function Resolve-KmgSprint32SuggestionOverlay {
    param([Parameter(Mandatory=$true)]$InstalledRuntimeMods,
        [Parameter(Mandatory=$true)][string]$SuggestionTransition,
        [Parameter(Mandatory=$true)][bool]$ProviderPresent,
        [Parameter(Mandatory=$true)][string]$SuggestionReferenceRoot,
        [Parameter(Mandatory=$true)][string]$PackagePath,
        [Parameter(Mandatory=$true)][string]$KingmakerInstallDir,
        [Parameter(Mandatory=$true)][string]$RepositoryRoot)
    $mods=Join-Path $KingmakerInstallDir 'Mods'
    if(@($InstalledRuntimeMods).Count -ne 10 -or @($InstalledRuntimeMods.id | Sort-Object -Unique).Count -ne 10) {
        throw 'Provider overlay requires the intact identifiable ten other intake mods.'
    }
    # Resolve the provider/reference identities read-only; never dispatch the
    # three-mod save-free profile against Working. Preserve every intake mod.
    Assert-KmgSuggestionReferenceRoot -ReferenceRoot $SuggestionReferenceRoot -RepositoryRoot $RepositoryRoot
    $providerProfile=Resolve-KmgCompatibilityProfile -ProfileId 'gunslinger-cowwithhat-suggestion' -ReferenceRoot $SuggestionReferenceRoot -PackagePath $PackagePath -KingmakerInstallDir $KingmakerInstallDir -RepositoryRoot $RepositoryRoot
    Assert-KmgSprint32SuggestionReferenceIdentity -Resolution $providerProfile -RepositoryRoot $RepositoryRoot
    $provider=@($providerProfile.runtimeMods | Where-Object ummId -ceq 'CowWithHatsCustomSpellsMod')
    $dependency=@($providerProfile.runtimeMods | Where-Object ummId -ceq 'CallOfTheWild')
    $installedDependency=@($installedRuntimeMods | Where-Object id -ceq 'CallOfTheWild')
    if(-not $providerProfile.runtimeCapable -or $providerProfile.runtimeMods.Count -ne 2 -or $provider.Count -ne 1 -or $dependency.Count -ne 1 -or $installedDependency.Count -ne 1 -or
       @($installedRuntimeMods | Where-Object { $_.id -ieq 'CowWithHatsCustomSpellsMod' -or $_.destinationName -ieq $provider[0].destinationName }).Count -ne 0 -or
       $provider[0].assembly.assemblyName -cne 'CowWithHatsCustomSpellsMod' -or $dependency[0].assembly.assemblyName -cne 'CallOfTheWild') {
        throw 'Optional provider overlay identity or intact installed dependency boundary differs; no transaction entered.'
    }
    $providerInfo=Read-KmgCompatibilityJson (Join-Path $provider[0].sourceDirectory 'Info.json')
    if(@($providerInfo.Requirements).Count -ne 1 -or $providerInfo.Requirements[0] -cne 'CallOfTheWild') { throw 'Provider dependency contract differs; no transaction entered.' }
    $installedInfo=Read-KmgCompatibilityJson (Join-Path (Join-Path $mods $installedDependency[0].destinationName) 'Info.json')
    if(-not $installedInfo.PSObject.Properties['Id'] -or $installedInfo.Id -cne 'CallOfTheWild' -or
       -not $installedInfo.PSObject.Properties['AssemblyName'] -or -not $installedInfo.PSObject.Properties['Version'] -or
       $installedInfo.AssemblyName -cne $dependency[0].assembly.fileName -or $installedInfo.Version -cne $dependency[0].version -or
       (Get-KmgCompatibilitySha256 (Join-Path (Join-Path $mods $installedDependency[0].destinationName) $installedInfo.AssemblyName)) -cne $dependency[0].assembly.sha256) {
        throw 'Installed CotW differs from the investigated provider dependency; no transaction entered.'
    }
    return [ordered]@{transition=$SuggestionTransition;present=$ProviderPresent;provider=$provider[0];dependency=$dependency[0].assembly;intakeModsPreserved=$installedRuntimeMods.Count}
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

function ConvertTo-KmgCompatibilityUtcTicks {
    param([Parameter(Mandatory=$true)]$Value)
    if($Value -is [DateTime]) {
        if($Value.Kind -ne [DateTimeKind]::Utc) { throw 'Manifest DateTime must identify UTC explicitly.' }
        return $Value.Ticks
    }
    if($Value -is [DateTimeOffset]) {
        if($Value.Offset -ne [TimeSpan]::Zero) { throw 'Manifest DateTimeOffset must identify UTC explicitly.' }
        return $Value.UtcDateTime.Ticks
    }
    if($Value -isnot [string] -or $Value -cnotmatch '^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?(?:Z|\+00:00)$') {
        throw 'Manifest timestamp must be explicit UTC ISO text with at most seven fractional digits.'
    }
    $parsed=[DateTimeOffset]::MinValue
    $formats=[string[]]@("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'","yyyy-MM-dd'T'HH:mm:ss'Z'",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz","yyyy-MM-dd'T'HH:mm:sszzz")
    $styles=[Globalization.DateTimeStyles]::AssumeUniversal -bor [Globalization.DateTimeStyles]::AdjustToUniversal
    if(-not [DateTimeOffset]::TryParseExact($Value,$formats,[Globalization.CultureInfo]::InvariantCulture,$styles,[ref]$parsed)) {
        throw 'Manifest timestamp is not a valid exact UTC instant.'
    }
    return $parsed.UtcDateTime.Ticks
}

function ConvertTo-KmgCompatibilityCanonicalManifest {
    param([AllowNull()]$Manifest)
    if($null -eq $Manifest) { throw 'A manifest cannot be null.' }
    $result=New-Object 'Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    foreach($row in @($Manifest)) {
        if($null -eq $row) { throw 'A manifest row cannot be null.' }
        $names=if($row -is [Collections.IDictionary]) {@($row.Keys)}else{@($row.PSObject.Properties.Name)}
        if($row.path -isnot [string] -or [string]::IsNullOrWhiteSpace($row.path) -or
            $row.kind -isnot [string] -or $row.kind -cnotin @('file','directory')) { throw 'Manifest row identity is malformed.' }
        $allowed=if($row.kind -ceq 'file') {@('path','kind','length','lastWriteTimeUtc','sha256')}else{@('path','kind')}
        if($names.Count -ne $allowed.Count -or @($names | Where-Object {$_ -cnotin $allowed}).Count -ne 0) {
            throw 'Manifest row has missing, differently cased or unknown fields.'
        }
        if([IO.Path]::IsPathRooted($row.path) -or
            ($row.path -cne '.' -and @($row.path -split '[\\/]' | Where-Object {$_ -cin @('','.','..')}).Count -ne 0) -or
            ($row.path -ceq '.' -and $row.kind -cne 'directory') -or $result.ContainsKey($row.path)) {
            throw 'Manifest has an invalid or duplicate exact relative path.'
        }
        $entry=[ordered]@{path=$row.path;kind=$row.kind}
        if($row.kind -ceq 'file') {
            if($null -eq $row.length -or $row.length.GetType().FullName -notin @('System.Byte','System.Int16','System.Int32','System.Int64','System.UInt16','System.UInt32','System.UInt64') -or
                $row.length -lt 0 -or $row.length -gt [long]::MaxValue -or
                $row.sha256 -isnot [string] -or $row.sha256 -cnotmatch '^[0-9a-f]{64}$') { throw 'Manifest file length or SHA-256 is malformed.' }
            $entry.length=[long]$row.length
            $entry.utcTicks=ConvertTo-KmgCompatibilityUtcTicks $row.lastWriteTimeUtc
            $entry.sha256=$row.sha256
        }
        $result.Add($row.path,$entry)
    }
    if($result.Count -gt 0 -and (-not $result.ContainsKey('.') -or $result['.'].kind -cne 'directory')) { throw 'A nonempty directory manifest needs its exact root record.' }
    return ,$result
}

function Test-KmgCompatibilityManifestEqual($Expected, $Actual) {
    # JSON parsers differ in DateTime handling and file collation. Identity is
    # an ordinal path-keyed set; timestamp identity is the exact UTC tick count.
    try {
        $left=ConvertTo-KmgCompatibilityCanonicalManifest $Expected
        $right=ConvertTo-KmgCompatibilityCanonicalManifest $Actual
        if($left.Count -ne $right.Count) {return $false}
        foreach($path in $left.Keys) {
            if(-not $right.ContainsKey($path)) {return $false}
            $a=$left[$path];$b=$right[$path]
            if($a.kind -cne $b.kind) {return $false}
            if($a.kind -ceq 'file' -and ($a.length -ne $b.length -or $a.utcTicks -ne $b.utcTicks -or $a.sha256 -cne $b.sha256)) {return $false}
        }
        return $true
    } catch {return $false}
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

function Get-KmgCompatibilityPackageVersion([string]$PackagePath) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead([IO.Path]::GetFullPath($PackagePath))
    try {
        $entries = @($archive.Entries | Where-Object { $_.FullName.Replace('\', '/') -ceq 'KingmakerGunslinger/Info.json' })
        if ($entries.Count -ne 1) { throw 'Compatibility package must contain exactly one KingmakerGunslinger/Info.json.' }
        $reader = New-Object IO.StreamReader($entries[0].Open())
        try { $info = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
        if ($info.Id -cne 'KingmakerGunslinger' -or $info.Version -cnotmatch '^0\.0\.[0-9]+$') {
            throw 'Compatibility package has an invalid mod identity or version.'
        }
        return [string]$info.Version
    } finally { $archive.Dispose() }
}

function Get-KmgCompatibilityDefaultPackage([string]$RepositoryRoot) {
    $version = [string](Read-KmgCompatibilityJson (Join-Path $RepositoryRoot 'Info.json')).Version
    return (Get-KmgLocalCandidateLayout -RepositoryRoot $RepositoryRoot `
        -Commit ((& git -C $RepositoryRoot rev-parse HEAD).Trim()) `
        -Id 'KingmakerGunslinger' -Version $version).PackagePath
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
    $reference = if (@($profile.modKeys).Count -eq 0) { [IO.Path]::GetFullPath($ReferenceRoot) } else { (Resolve-Path -LiteralPath $ReferenceRoot).Path }
    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    $packageHash = Get-KmgCompatibilitySha256 $package
    $packageVersion = Get-KmgCompatibilityPackageVersion $package
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
        gunslinger = [ordered]@{ packagePath = $package; version = $packageVersion; packageSha256 = $packageHash; ummId = 'KingmakerGunslinger' }
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
    try {
        if ($PSBoundParameters.ContainsKey('KnownKingmakerProcessIds')) { Assert-KmgCompatibilityNoKingmakerProcess -KnownProcessIds $KnownKingmakerProcessIds }
        else { Assert-KmgCompatibilityNoKingmakerProcess }
    } catch { Remove-KmgCompatibilityOwnedLock $lockPath $RunId; throw }
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

function Assert-KmgCompatibilityResumeQuarantine {
    param($State,[string]$StateRoot)
    $quarantine=[string]$State.stagedQuarantinePath
    if(-not(Test-Path -LiteralPath $quarantine -PathType Container)) {throw 'Post-move restoration resume requires its owned staged quarantine.'}
    $entries=@(Get-Item -LiteralPath $quarantine -Force)+@(Get-ChildItem -LiteralPath $quarantine -Force -Recurse)
    if(@($entries | Where-Object {($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0}).Count) {throw 'Resume quarantine contains a reparse point.'}
    $sentinel=Read-KmgCompatibilityJson (Join-Path $quarantine '.kmg-compat-sentinel.json')
    $sentinelFields=@($sentinel.PSObject.Properties.Name)
    if($sentinelFields.Count -ne 4 -or @($sentinelFields | Where-Object {$_ -cnotin @('schemaVersion','runId','stateFile','stateSha256')}).Count -or
        $sentinel.schemaVersion -ne 1 -or $sentinel.runId -cne $State.runId -or
        $sentinel.stateFile -isnot [string] -or $sentinel.stateSha256 -isnot [string] -or $sentinel.stateSha256 -cnotmatch '^[0-9a-f]{64}$') {throw 'Resume quarantine sentinel has foreign or malformed ownership.'}
    $ownershipPath=Join-Path (Join-Path ([IO.Path]::GetFullPath($StateRoot)) $State.runId) 'ownership-state.json'
    if(-not [IO.Path]::GetFullPath($sentinel.stateFile).Equals([IO.Path]::GetFullPath($ownershipPath),[StringComparison]::OrdinalIgnoreCase) -or
        -not(Test-Path -LiteralPath $ownershipPath -PathType Leaf) -or
        (Get-KmgCompatibilitySha256 $ownershipPath) -cne $sentinel.stateSha256) {throw 'Resume quarantine does not bind the exact immutable ownership-state bytes.'}
    $ownership=Read-KmgCompatibilityJson $ownershipPath
    $fields=@('schemaVersion','runId','profileId','installRoot','modsPath','originalBackupPath','stagedQuarantinePath')
    if(@($ownership.PSObject.Properties).Count -ne $fields.Count -or @($ownership.PSObject.Properties.Name | Where-Object {$_ -cnotin $fields}).Count) {throw 'Resume immutable ownership state has missing or unknown fields.'}
    foreach($field in $fields) {
        if($field -ceq 'schemaVersion') {if($ownership.$field -ne 1) {throw 'Resume ownership schema mismatch.'}}
        elseif($ownership.$field -isnot [string] -or $ownership.$field -cne $State.$field) {throw "Resume immutable ownership mismatch: $field"}
    }
}

function Restore-KmgCompatibilityTransaction {
    param(
        [Parameter(Mandatory = $true)][ValidatePattern('^[A-Za-z0-9._-]{1,100}$')][string]$RunId,
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
    $expectedPaths=@{modsPath=(Join-Path $install 'Mods');originalBackupPath=(Join-Path $install "Mods.kmg-compat-$RunId.original");
        stagedQuarantinePath=(Join-Path $install "Mods.kmg-compat-$RunId.staged");lockPath=(Join-Path ([IO.Path]::GetFullPath($StateRoot)) 'compatibility.lock')}
    foreach($field in $expectedPaths.Keys) {
        if(-not [IO.Path]::GetFullPath([string]$state.$field).Equals([IO.Path]::GetFullPath($expectedPaths[$field]),[StringComparison]::OrdinalIgnoreCase)) {throw "Restoration ownership path mismatch: $field"}
    }
    if ((Get-Content -LiteralPath $state.lockPath -Raw).Trim() -cne $RunId) { throw 'Compatibility restoration does not own the shared lock.' }
    # Refuse before the first mutation while a runtime child holds this lease.
    $restoreLease = [IO.File]::Open($state.lockPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    try {
    if ($PSBoundParameters.ContainsKey('KnownKingmakerProcessIds')) { Assert-KmgCompatibilityNoKingmakerProcess -KnownProcessIds $KnownKingmakerProcessIds }
    else { Assert-KmgCompatibilityNoKingmakerProcess }
    $priorStatus=$state.status
    $state.status = 'Restoring'; Write-KmgCompatibilityJsonAtomic $statePath $state
    try {
        $resumePostMove=$false
        if(Test-Path -LiteralPath $backup -PathType Leaf) {throw 'Original Mods backup path is a file instead of the owned directory.'}
        if($state.originalExisted -and -not(Test-Path -LiteralPath $backup)) {
            if($priorStatus -cne 'RestorationFailed' -or -not(Test-Path -LiteralPath $mods -PathType Container)) {throw 'Original Mods backup is missing outside the exact post-move failure state.'}
            $currentEntries=@(Get-Item -LiteralPath $mods -Force)+@(Get-ChildItem -LiteralPath $mods -Force -Recurse)
            if(@($currentEntries | Where-Object {($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0}).Count) {throw 'Restored Mods resume intake contains a reparse point.'}
            if(-not(Test-KmgCompatibilityManifestEqual @($state.originalManifest) @(Get-KmgCompatibilityDirectoryManifest $mods))) {throw 'Post-move resume current Mods are not the exact original intake.'}
            Assert-KmgCompatibilityResumeQuarantine -State $state -StateRoot $StateRoot
            $resumePostMove=$true
        }
        if (-not $resumePostMove -and (Test-Path -LiteralPath $mods -PathType Container)) {
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
        if ($state.originalExisted -and -not $resumePostMove) {
            if (-not (Test-Path -LiteralPath $backup -PathType Container)) { throw 'Original Mods backup is missing.' }
            Move-Item -LiteralPath $backup -Destination $mods
            $after = @(Get-KmgCompatibilityDirectoryManifest $mods)
            if (-not (Test-KmgCompatibilityManifestEqual @($state.originalManifest) $after)) { throw 'Restored Mods manifest/hash mismatch.' }
        } elseif (-not $state.originalExisted -and (Test-Path -LiteralPath $mods)) { throw 'Mods must remain absent because it was absent before the transaction.' }
        $bank = $state.managedSoundBank
        if ($bank.existed) {
            if (-not (Test-Path -LiteralPath $bank.backupPath -PathType Leaf)) { throw 'Managed SoundBank backup is missing.' }
            New-Item -ItemType Directory -Path (Split-Path -Parent $bank.path) -Force | Out-Null
            Copy-Item -LiteralPath $bank.backupPath -Destination $bank.path -Force
            $bankTicks=ConvertTo-KmgCompatibilityUtcTicks $bank.lastWriteTimeUtc
            (Get-Item -LiteralPath $bank.path).LastWriteTimeUtc = [DateTime]::new($bankTicks,[DateTimeKind]::Utc)
            $bankAfter=Get-Item -LiteralPath $bank.path
            if ((Get-KmgCompatibilitySha256 $bank.path) -cne $bank.sha256 -or $bankAfter.Length -ne $bank.length -or $bankAfter.LastWriteTimeUtc.Ticks -ne $bankTicks) { throw 'Managed SoundBank restoration hash/length/exact timestamp mismatch.' }
        } elseif (Test-Path -LiteralPath $bank.path -PathType Leaf) { Remove-Item -LiteralPath $bank.path -Force }
        if (Test-Path -LiteralPath $quarantine -PathType Container) {
            $quarantineSentinel = Read-KmgCompatibilityJson (Join-Path $quarantine '.kmg-compat-sentinel.json')
            if ($quarantineSentinel.runId -cne $RunId) { throw 'Quarantined staged directory sentinel mismatch.' }
            if ((Get-KmgCompatibilitySha256 $quarantineSentinel.stateFile) -cne $quarantineSentinel.stateSha256) { throw 'Quarantined sentinel ownership-state hash mismatch.' }
            Remove-Item -LiteralPath $quarantine -Recurse -Force
        }
        $state.status = 'Restored'; $state.restorationVerified = $true; $state.restoredAtUtc = [DateTime]::UtcNow.ToString('o')
        Write-KmgCompatibilityJsonAtomic $statePath $state
        $restoreLease.Dispose()
        Remove-KmgCompatibilityOwnedLock $state.lockPath $RunId
        return $state
    } catch {
        $state.status = 'RestorationFailed'; $state.restorationVerified = $false
        Write-KmgCompatibilityJsonAtomic $statePath $state
        throw "Compatibility restoration failed closed. $($state.recoveryInstructions) Cause: $($_.Exception.Message)"
    }
    } finally { $restoreLease.Dispose() }
}
