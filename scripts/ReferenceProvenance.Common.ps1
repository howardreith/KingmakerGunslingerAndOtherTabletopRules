Set-StrictMode -Version Latest

# Game and Unity assemblies: the private bundle must equal the installed Steam
# runtime byte for byte, because the mod binds to exactly that game build.
$script:KmgInstalledReferencePaths = @(
    'Assembly-CSharp.dll',
    'Assembly-CSharp-firstpass.dll',
    'UniRx.dll',
    'Newtonsoft.Json.dll',
    'UnityEngine.dll',
    'UnityEngine.AnimationModule.dll',
    'UnityEngine.AudioModule.dll',
    'UnityEngine.AssetBundleModule.dll',
    'UnityEngine.CoreModule.dll',
    'UnityEngine.ClothModule.dll',
    'UnityEngine.PhysicsModule.dll',
    'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll',
    'UnityEngine.TextRenderingModule.dll'
)

# Loader assemblies: deliberately pinned to the oldest supported Unity Mod
# Manager (scripts/loader-reference-pins.json), never to the local install, so
# a newer installed loader cannot change the API surface the build binds to.
$script:KmgPinnedLoaderReferencePaths = @(
    'UnityModManager\UnityModManager.dll',
    'UnityModManager\0Harmony12.dll'
)

$script:KmgPrivateReferencePaths = @(
    $script:KmgInstalledReferencePaths + $script:KmgPinnedLoaderReferencePaths)

$script:KmgLoaderReferencePinsPath = Join-Path $PSScriptRoot 'loader-reference-pins.json'

function Get-KmgFileSha256Lower {
    param([Parameter(Mandatory = $true)][string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-KmgLoaderReferencePins {
    param([string]$PinsPath = $script:KmgLoaderReferencePinsPath)
    if (-not (Test-Path -LiteralPath $PinsPath -PathType Leaf)) {
        throw "Loader reference pins are missing: $PinsPath"
    }
    $document = Get-Content -LiteralPath $PinsPath -Raw | ConvertFrom-Json
    if ($document.schemaVersion -ne 1 -or
        [string]$document.minimumManagerVersion -cnotmatch '^[0-9]+\.[0-9]+\.[0-9]+$') {
        throw 'Loader reference pins have an invalid schema or minimum manager version.'
    }
    $pins = @($document.references)
    $expected = @($script:KmgPinnedLoaderReferencePaths | ForEach-Object { $_.Replace('\', '/') })
    $observed = @($pins | ForEach-Object { [string]$_.relativePath })
    if (($observed -join "`n") -cne ($expected -join "`n")) {
        throw "Loader reference pins must name exactly: $($expected -join ', ')"
    }
    foreach ($pin in $pins) {
        if ([string]$pin.sha256 -cnotmatch '^[0-9a-f]{64}$' -or
            [string]$pin.url -cnotmatch '^https://' -or
            [string]::IsNullOrWhiteSpace([string]$pin.assemblyVersion) -or
            ($null -ne $pin.archiveEntry -and
                [string]$pin.archiveSha256 -cnotmatch '^[0-9a-f]{64}$')) {
            throw "Loader reference pin is malformed: $($pin.relativePath)"
        }
    }
    return [pscustomobject]@{
        Path = (Resolve-Path -LiteralPath $PinsPath).Path
        Sha256 = Get-KmgFileSha256Lower -Path $PinsPath
        MinimumManagerVersion = [string]$document.minimumManagerVersion
        References = $pins
    }
}

<#
Places every pinned loader reference in $LoaderReferenceDir, downloading only
the files that are absent there, and verifies every one against its pin. A file
that is present with different bytes is refused rather than replaced: a
tampered or substituted reference is an error, not a cache miss. There is no
fallback to an installed or bundled loader.
#>
function Initialize-KmgPinnedLoaderReferences {
    param(
        [Parameter(Mandatory = $true)][string]$LoaderReferenceDir,
        [string]$PinsPath = $script:KmgLoaderReferencePinsPath
    )
    $pins = Get-KmgLoaderReferencePins -PinsPath $PinsPath
    New-Item -ItemType Directory -Path $LoaderReferenceDir -Force | Out-Null
    foreach ($pin in $pins.References) {
        $target = Join-Path $LoaderReferenceDir ([string]$pin.relativePath).Replace('/', '\')
        if (-not (Test-Path -LiteralPath $target -PathType Leaf)) {
            New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
            $download = "$target.download-$([Guid]::NewGuid().ToString('N'))"
            try {
                Invoke-WebRequest -Uri $pin.url -OutFile $download -UseBasicParsing
                if ($null -ne $pin.archiveEntry) {
                    if ((Get-KmgFileSha256Lower -Path $download) -cne $pin.archiveSha256) {
                        throw "Pinned loader archive hash differs: $($pin.url)"
                    }
                    Add-Type -AssemblyName System.IO.Compression
                    Add-Type -AssemblyName System.IO.Compression.FileSystem
                    $archive = [IO.Compression.ZipFile]::OpenRead($download)
                    try {
                        $entry = $archive.GetEntry([string]$pin.archiveEntry)
                        if ($null -eq $entry) { throw "Pinned archive lacks $($pin.archiveEntry)" }
                        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, "$download.entry", $true)
                    }
                    finally { $archive.Dispose() }
                    Remove-Item -LiteralPath $download -Force
                    Move-Item -LiteralPath "$download.entry" -Destination $download
                }
                if ((Get-KmgFileSha256Lower -Path $download) -cne $pin.sha256) {
                    throw "Downloaded loader reference differs from its pin: $($pin.relativePath)"
                }
                Move-Item -LiteralPath $download -Destination $target
            }
            finally {
                foreach ($leftover in @($download, "$download.entry")) {
                    if (Test-Path -LiteralPath $leftover) { Remove-Item -LiteralPath $leftover -Force }
                }
            }
        }
    }
    return @(Assert-KmgPinnedLoaderReferences -LoaderReferenceDir $LoaderReferenceDir -PinsPath $PinsPath)
}

function Assert-KmgPinnedLoaderReferences {
    param(
        [Parameter(Mandatory = $true)][string]$LoaderReferenceDir,
        [string]$PinsPath = $script:KmgLoaderReferencePinsPath
    )
    $pins = Get-KmgLoaderReferencePins -PinsPath $PinsPath
    $result = [Collections.Generic.List[object]]::new()
    foreach ($pin in $pins.References) {
        $path = Join-Path $LoaderReferenceDir ([string]$pin.relativePath).Replace('/', '\')
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "Pinned loader reference is missing: $path"
        }
        $hash = Get-KmgFileSha256Lower -Path $path
        if ($hash -cne $pin.sha256) {
            throw "Pinned loader reference differs from its pin: " +
                "$($pin.relativePath); pinnedSha256=$($pin.sha256); actualSha256=$hash"
        }
        $result.Add([ordered]@{
            relativePath = [string]$pin.relativePath
            sha256 = $hash
            role = 'pinned-loader'
            assemblyVersion = [string]$pin.assemblyVersion
            source = [string]$pin.url
            pinsSha256 = $pins.Sha256
        })
    }
    return @($result)
}

function Assert-KmgReferenceBundleMatchesInstall {
    param(
        [Parameter(Mandatory = $true)][string]$ReferenceBundleDir,
        [Parameter(Mandatory = $true)][string]$KingmakerInstallDir,
        [Parameter(Mandatory = $true)][string]$LoaderReferenceDir,
        [string]$PinsPath = $script:KmgLoaderReferencePinsPath
    )

    $bundleManaged = Join-Path $ReferenceBundleDir 'Managed'
    $installedManaged = Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed'
    $result = [Collections.Generic.List[object]]::new()
    foreach ($relativePath in $script:KmgInstalledReferencePaths) {
        $bundlePath = Join-Path $bundleManaged $relativePath
        $installedPath = Join-Path $installedManaged $relativePath
        if (-not (Test-Path -LiteralPath $bundlePath -PathType Leaf)) {
            throw "Private reference is missing: $bundlePath"
        }
        if (-not (Test-Path -LiteralPath $installedPath -PathType Leaf)) {
            throw "Installed runtime reference is missing: $installedPath"
        }
        $bundleHash = Get-KmgFileSha256Lower -Path $bundlePath
        $installedHash = Get-KmgFileSha256Lower -Path $installedPath
        if ($bundleHash -ne $installedHash) {
            throw "Private reference differs from the installed Steam runtime: " +
                "$relativePath; privateSha256=$bundleHash; installedSha256=$installedHash"
        }
        $result.Add([ordered]@{
            relativePath = $relativePath.Replace('\', '/')
            sha256 = $bundleHash
            role = 'installed-game'
        })
    }
    foreach ($loader in @(Assert-KmgPinnedLoaderReferences `
            -LoaderReferenceDir $LoaderReferenceDir -PinsPath $PinsPath)) {
        $result.Add($loader)
    }
    return @($result)
}

<#
A development or release receipt names its loader references by pin, not by
whatever happened to be installed. Refuses a receipt whose loader rows are
absent or differ from the current pins.
#>
function Assert-KmgReceiptLoaderReferencesMatchPins {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$ReferenceFiles,
        [string]$PinsPath = $script:KmgLoaderReferencePinsPath
    )
    $pins = Get-KmgLoaderReferencePins -PinsPath $PinsPath
    foreach ($pin in $pins.References) {
        $rows = @($ReferenceFiles | Where-Object {
            $null -ne $_ -and $null -ne $_.PSObject.Properties['relativePath'] -and
                [string]$_.relativePath -ceq [string]$pin.relativePath })
        if ($rows.Count -ne 1) {
            throw "Receipt lacks exactly one pinned loader row: $($pin.relativePath)"
        }
        $row = $rows[0]
        foreach ($name in @('sha256', 'role', 'assemblyVersion', 'pinsSha256')) {
            if ($null -eq $row.PSObject.Properties[$name]) {
                throw "Receipt loader row lacks ${name}: $($pin.relativePath)"
            }
        }
        if ([string]$row.sha256 -cne [string]$pin.sha256 -or
            [string]$row.role -cne 'pinned-loader' -or
            [string]$row.assemblyVersion -cne [string]$pin.assemblyVersion -or
            [string]$row.pinsSha256 -cne $pins.Sha256) {
            throw "Receipt loader reference does not match the current pin: $($pin.relativePath)"
        }
    }
}

<#
The package's declared loader floor is the build's pinned floor. Also refuses
any runtime dependency copied into the mod: the package carries exactly one
binary, the mod itself, and never a loader, Harmony, game or Unity assembly.
#>
<#
Identity of a mod DLL read in an isolated Windows PowerShell process (reflection-only, so nothing is loaded into the
caller): SHA-256, MVID and every referenced assembly's full binding identity. An unreadable DLL fails.
#>
function Get-KmgPackagedDllIdentity {
    param([Parameter(Mandatory = $true)][string]$DllPath)
    $escaped = $DllPath.Replace("'", "''")
    $script = "`$ErrorActionPreference='Stop'; `$bytes=[IO.File]::ReadAllBytes('$escaped'); `$a=[Reflection.Assembly]::ReflectionOnlyLoad(`$bytes); " +
        "`$sha=[Security.Cryptography.SHA256]::Create(); [ordered]@{ name=`$a.GetName().Name; sha256=([BitConverter]::ToString(`$sha.ComputeHash(`$bytes))).Replace('-','').ToLowerInvariant(); " +
        "mvid=`$a.ManifestModule.ModuleVersionId.ToString(); references=@(`$a.GetReferencedAssemblies() | ForEach-Object { `$_.FullName }); " +
        "gitCommit=(@([Reflection.CustomAttributeData]::GetCustomAttributes(`$a) | Where-Object { `$_.AttributeType.FullName -ceq 'System.Reflection.AssemblyMetadataAttribute' -and [string]`$_.ConstructorArguments[0].Value -ceq 'GitCommit' } | ForEach-Object { [string]`$_.ConstructorArguments[1].Value }) | Select-Object -First 1) } | ConvertTo-Json -Compress"
    $previous = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    try { $output = & powershell.exe -NoLogo -NoProfile -NonInteractive -Command $script 2>&1 | ForEach-Object { "$_" } }
    finally { $ErrorActionPreference = $previous }
    if ($LASTEXITCODE -ne 0) { throw "The packaged mod DLL could not be read: $($output -join ' ')" }
    return (($output -join '') | ConvertFrom-Json)
}

function Assert-KmgPackagedLoaderContract {
    param(
        [Parameter(Mandatory = $true)][string]$ModDirectory,
        [string]$PinsPath = $script:KmgLoaderReferencePinsPath
    )
    $pins = Get-KmgLoaderReferencePins -PinsPath $PinsPath
    $infoPath = Join-Path $ModDirectory 'Info.json'
    if (-not (Test-Path -LiteralPath $infoPath -PathType Leaf)) {
        throw 'Package lacks Info.json.'
    }
    $info = Get-Content -LiteralPath $infoPath -Raw | ConvertFrom-Json
    if ($null -eq $info.PSObject.Properties['ManagerVersion'] -or -not ($info.ManagerVersion -is [string]) -or
        $info.ManagerVersion -cne $pins.MinimumManagerVersion) {
        throw "Packaged ManagerVersion must equal the pinned loader floor $($pins.MinimumManagerVersion)."
    }
    $binaryFiles = @(
        Get-ChildItem -LiteralPath $ModDirectory -Recurse -File |
            Where-Object { $_.Extension -in @('.dll', '.exe', '.pdb', '.mdb') }
    )
    if ($binaryFiles.Count -ne 1 -or $binaryFiles[0].Name -ne 'KingmakerGunslinger.dll') {
        throw 'The standalone UMM package must contain exactly one binary: KingmakerGunslinger.dll.'
    }
    $forbiddenNames = @(
        '0Harmony.dll',
        '0Harmony12.dll',
        '0Harmony-1.2.dll',
        'dnlib.dll',
        'Assembly-CSharp.dll',
        'Assembly-CSharp-firstpass.dll',
        'Newtonsoft.Json.dll',
        'UnityEngine.dll',
        'UnityModManager.dll'
    )
    foreach ($name in $forbiddenNames) {
        if (Get-ChildItem -LiteralPath $ModDirectory -Recurse -File -Filter $name -ErrorAction SilentlyContinue) {
            throw "Install package contains a private or foreign runtime assembly: $name"
        }
    }
    # The packaged DLL itself: within the UnityModManager and Harmony families it must reference exactly the pinned
    # loader assemblies, by full binding identity (a DLL compiled against a newer loader is refused).
    $identity = Get-KmgPackagedDllIdentity -DllPath $binaryFiles[0].FullName
    if ([string]$identity.name -cne 'KingmakerGunslinger') { throw "The packaged DLL defines assembly '$($identity.name)', not KingmakerGunslinger." }
    $expected = @($pins.References | ForEach-Object {
        '{0}, Version={1}, Culture=neutral, PublicKeyToken=null' -f [IO.Path]::GetFileNameWithoutExtension([string]$_.relativePath), [string]$_.assemblyVersion
    } | Sort-Object)
    $observed = @(@($identity.references) | Where-Object { ([string]$_ -split ',')[0] -match '(?i)^(unitymodmanager|0?harmony)' } | Sort-Object)
    if (($observed -join "`n") -cne ($expected -join "`n")) {
        throw "The packaged mod DLL references [$($observed -join '; ')], not exactly the pinned loader assemblies [$($expected -join '; ')]."
    }
    return $identity
}

# Material inputs of a candidate: the product tree, its manifest, the loader pins and every helper that selects or
# verifies what the compiler receives. A candidate is current only while none of them differs from its producing commit.
# Every file outside docs/ is a material input of a candidate: product sources, shipped files (Info.json, README,
# CHANGELOG, licences and guides), assets, blueprints, project files and every build, packaging and validation helper.
# Only documentation under docs/ (review records and evidence) can change without making a candidate stale.
$script:KmgCandidateMaterialPaths = @('.', ':(exclude)docs')

function Assert-KmgCandidateMaterialInputsCurrent {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot, [Parameter(Mandatory = $true)][string]$Commit)
    if ($Commit -cnotmatch '^[0-9a-f]{40}$') { throw 'The candidate receipt does not name a producing commit.' }
    $previous = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    try {
        $null = & git -C $RepositoryRoot cat-file -e ($Commit + '^{commit}') 2>&1
        if ($LASTEXITCODE -ne 0) { throw "The candidate's producing commit $Commit is not in this repository." }
        $changed = @(& git -C $RepositoryRoot diff --name-only $Commit -- @($script:KmgCandidateMaterialPaths) 2>&1 | ForEach-Object { "$_" })
        if ($LASTEXITCODE -ne 0) { throw 'Unable to compare the candidate material inputs.' }
        $untracked = @(& git -C $RepositoryRoot ls-files --others --exclude-standard -- @($script:KmgCandidateMaterialPaths) 2>&1 | ForEach-Object { "$_" })
        # Documents package.ps1 ships are material wherever they live, docs/ included (the packaging inventory is
        # scripts/package-documents.json, itself a material input); other documentation under docs/ is not.
        $shipped = @((Get-Content -LiteralPath (Join-Path $RepositoryRoot 'scripts\package-documents.json') -Raw | ConvertFrom-Json).documents | ForEach-Object { [string]$_ })
        if ($shipped.Count -eq 0) { throw 'The shipped document list is empty.' }
        $changed += @(& git -C $RepositoryRoot diff --name-only $Commit -- @($shipped) 2>&1 | ForEach-Object { "$_" })
        if ($LASTEXITCODE -ne 0) { throw 'Unable to compare the shipped documents.' }
    }
    finally { $ErrorActionPreference = $previous }
    $all = @($changed + $untracked | Where-Object { $_ })
    if ($all.Count -ne 0) {
        throw "The candidate is stale: material inputs changed since its producing commit $($Commit.Substring(0, 10)): $($all -join ', '). Build a new candidate."
    }
}
