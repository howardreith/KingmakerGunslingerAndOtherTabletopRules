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

# Raw stdout bytes of one git command (no console decoding, no line splitting).
function Invoke-KmgGitBytes {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot, [Parameter(Mandatory = $true)][string[]]$Arguments)
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = 'git'
    $info.Arguments = (@('-C', $RepositoryRoot.TrimEnd('\')) + $Arguments | ForEach-Object { '"' + $_ + '"' }) -join ' '
    $info.UseShellExecute = $false; $info.RedirectStandardOutput = $true; $info.RedirectStandardError = $true; $info.CreateNoWindow = $true
    $process = [Diagnostics.Process]::Start($info)
    try {
        $errors = $process.StandardError.ReadToEndAsync()
        $buffer = New-Object IO.MemoryStream
        $process.StandardOutput.BaseStream.CopyTo($buffer)
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) { throw "git $($Arguments -join ' ') failed ($($process.ExitCode)): $($errors.Result.Trim())" }
        return , $buffer.ToArray()
    }
    finally { $process.Dispose() }
}

# The receipt's producing commit must describe every file the package ships (R1-F4). Each ZIP member under the mod folder
# resolves to the file tracked at that commit that produced it: the file at the same path, or the source of the build's
# copy rule for that packaged path (the project's Content/None items with a Link, so assets/game/icons/*.png ships as
# assets/icons/*.png). Paths match without regard to letter case, as Windows packaging and extraction do, and resolve to
# Git's own spelling; a member that matches more than one tracked file is refused as ambiguous. Each resolved member must
# equal that commit's file as checked out: git archive of the commit writes each file with the conversion a checkout of it
# performs (this repository's attributes and line-ending configuration) and nothing else. Only the compiled assembly is a
# generated member (bound by the receipt's DLL identity); any other member that resolves to no tracked producer file is
# refused, never treated as generated. The package digest the receipt records is measured on the same read handle the
# members are then read from (R7-F1), opened without write or delete sharing, so the archive inspected is the archive the
# receipt records. The working tree is not read. Returns the number of members compared.
$script:KmgPackageGeneratedMembers = @('KingmakerGunslinger.dll')
$script:KmgPackageProjectDirectory = 'src/KingmakerGunslinger'
$script:KmgPackageProjectPath = 'src/KingmakerGunslinger/KingmakerGunslinger.csproj'

function ConvertTo-KmgRepositoryRelativePath {
    param([Parameter(Mandatory = $true)][string]$Path)
    $parts = [Collections.Generic.List[string]]::new()
    foreach ($segment in $Path.Replace('\', '/').Split('/')) {
        if ($segment -eq '' -or $segment -eq '.') { continue }
        if ($segment -eq '..') {
            if ($parts.Count -eq 0) { throw "A package copy rule leaves the repository: $Path" }
            $parts.RemoveAt($parts.Count - 1); continue
        }
        $parts.Add($segment)
    }
    return ($parts -join '/')
}

# The build's copy rules at one commit, read from that commit's project file: each Content or None item with a Link and a
# CopyToOutputDirectory value is copied into the build output at its Link path, which package.ps1 ships at the same path
# under the mod folder. Supported forms are a literal file, or a wildcard file name in one directory whose Link is
# <directory>\%(Filename)%(Extension); any other form is refused rather than guessed.
function Get-KmgPackageCopyRules {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot, [Parameter(Mandatory = $true)][string]$Commit)
    $bytes = Invoke-KmgGitBytes -RepositoryRoot $RepositoryRoot -Arguments @('show', ($Commit + ':' + $script:KmgPackageProjectPath))
    [xml]$project = [Text.Encoding]::UTF8.GetString($bytes).TrimStart([char]0xFEFF)
    $rules = [Collections.Generic.List[object]]::new()
    foreach ($item in @($project.SelectNodes('//*[local-name()="Content" or local-name()="None"]'))) {
        $link = $item.SelectSingleNode('*[local-name()="Link"]')
        $copy = $item.SelectSingleNode('*[local-name()="CopyToOutputDirectory"]')
        if ($null -eq $copy -or $copy.InnerText.Trim() -eq 'Never') { continue }
        $include = [string]$item.GetAttribute('Include')
        if ($null -eq $link -or $include -match '[;$@%]' -or $include.Contains('**')) {
            throw "Unsupported package copy rule in $($script:KmgPackageProjectPath): $include"
        }
        $source = ConvertTo-KmgRepositoryRelativePath ($script:KmgPackageProjectDirectory + '/' + $include)
        $target = ConvertTo-KmgRepositoryRelativePath $link.InnerText.Trim()
        $slash = $source.LastIndexOf('/')
        $sourceDirectory = if ($slash -ge 0) { $source.Substring(0, $slash) } else { '' }
        $sourceName = $source.Substring($slash + 1)
        if ($sourceDirectory -match '[*?]') { throw "Unsupported package copy rule in $($script:KmgPackageProjectPath): $include" }
        if ($sourceName -match '[*?]') {
            $m = [regex]::Match($target, '^(?:(.*)/)?%\(Filename\)%\(Extension\)$')
            if (-not $m.Success -or $m.Groups[1].Value -match '[%*?]') { throw "Unsupported package copy rule in $($script:KmgPackageProjectPath): $include -> $($link.InnerText)" }
            $rules.Add([pscustomobject]@{ Wildcard = $true; SourceDirectory = $sourceDirectory; TargetDirectory = $m.Groups[1].Value
                    Matcher = [Management.Automation.WildcardPattern]::new($sourceName, [Management.Automation.WildcardOptions]::IgnoreCase) })
        }
        else {
            if ($target -match '[%*?]') { throw "Unsupported package copy rule in $($script:KmgPackageProjectPath): $include -> $($link.InnerText)" }
            $rules.Add([pscustomobject]@{ Wildcard = $false; Source = $source; Target = $target })
        }
    }
    if ($rules.Count -eq 0) { throw "No package copy rules were found in $($script:KmgPackageProjectPath) at $($Commit.Substring(0, 10))." }
    return , $rules
}

# The tracked files (Git's spelling) that can have produced one packaged member: its own path, and the source of each copy
# rule whose packaged path it is, each matched without regard to letter case.
function Resolve-KmgPackagedMemberSource {
    param([Parameter(Mandatory = $true)][string]$Relative, [Parameter(Mandatory = $true)]$Rules, [Parameter(Mandatory = $true)]$TrackedByFold)
    $paths = [Collections.Generic.List[string]]::new()
    $paths.Add($Relative)
    $slash = $Relative.LastIndexOf('/')
    $directory = if ($slash -ge 0) { $Relative.Substring(0, $slash) } else { '' }
    $name = $Relative.Substring($slash + 1)
    foreach ($rule in $Rules) {
        if ($rule.Wildcard) {
            if ([string]::Equals($directory, $rule.TargetDirectory, [StringComparison]::OrdinalIgnoreCase) -and $rule.Matcher.IsMatch($name)) {
                $paths.Add($(if ($rule.SourceDirectory) { $rule.SourceDirectory + '/' + $name } else { $name }))
            }
        }
        elseif ([string]::Equals($Relative, $rule.Target, [StringComparison]::OrdinalIgnoreCase)) { $paths.Add($rule.Source) }
    }
    $sources = [Collections.Generic.List[string]]::new()
    foreach ($path in $paths) {
        if (-not $TrackedByFold.ContainsKey($path)) { continue }
        foreach ($tracked in $TrackedByFold[$path]) { if (-not $sources.Contains($tracked)) { $sources.Add($tracked) } }
    }
    return , $sources
}

function Assert-KmgPackagedRepositoryFilesMatchCommit {
    param([Parameter(Mandatory = $true)][string]$PackagePath, [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$Commit, [Parameter(Mandatory = $true)][string]$ExpectedSha256,
        [string]$ModFolder = 'KingmakerGunslinger')
    if ($Commit -cnotmatch '^[0-9a-f]{40}$') { throw 'The candidate receipt does not name a producing commit.' }
    if ($ExpectedSha256 -cnotmatch '^[0-9a-f]{64}$') { throw 'The candidate receipt does not record the package SHA-256.' }
    $short = $Commit.Substring(0, 10)
    $root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $trackedByFold = [Collections.Generic.Dictionary[string, Collections.Generic.List[string]]]::new([StringComparer]::OrdinalIgnoreCase)
    $listing = Invoke-KmgGitBytes -RepositoryRoot $root -Arguments @('ls-tree', '-r', '-z', '--name-only', $Commit)
    foreach ($name in ([Text.Encoding]::UTF8.GetString($listing) -split "`0")) {
        if (-not $name) { continue }
        if (-not $trackedByFold.ContainsKey($name)) { $trackedByFold[$name] = [Collections.Generic.List[string]]::new() }
        $trackedByFold[$name].Add($name)
    }
    $rules = Get-KmgPackageCopyRules -RepositoryRoot $root -Commit $Commit
    Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
    $members = [Collections.Generic.List[object]]::new()
    $unresolved = [Collections.Generic.List[string]]::new()
    $ambiguous = [Collections.Generic.List[string]]::new()
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        $file = [IO.File]::Open((Resolve-Path -LiteralPath $PackagePath).Path, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
        try {
            $actual = [BitConverter]::ToString($sha.ComputeHash($file)).Replace('-', '').ToLowerInvariant()
            if ($actual -cne $ExpectedSha256) {
                throw "The package is not the archive its receipt records: $PackagePath has SHA-256 $actual, the receipt records $ExpectedSha256."
            }
            $file.Position = 0
            $package = New-Object IO.Compression.ZipArchive($file, [IO.Compression.ZipArchiveMode]::Read, $true)
            try {
                foreach ($entry in $package.Entries) {
                    $name = $entry.FullName.Replace('\', '/')
                    if ($name.EndsWith('/')) { continue }
                    if (-not $name.StartsWith($ModFolder + '/', [StringComparison]::OrdinalIgnoreCase)) { $unresolved.Add($name); continue }
                    $relative = $name.Substring($ModFolder.Length + 1)
                    if ($script:KmgPackageGeneratedMembers -ccontains $relative) { continue }
                    $sources = Resolve-KmgPackagedMemberSource -Relative $relative -Rules $rules -TrackedByFold $trackedByFold
                    if ($sources.Count -eq 0) { $unresolved.Add($relative); continue }
                    if ($sources.Count -gt 1) { $ambiguous.Add("$relative ($($sources -join ' or '))"); continue }
                    $stream = $entry.Open()
                    try { $hash = [BitConverter]::ToString($sha.ComputeHash($stream)) } finally { $stream.Dispose() }
                    $members.Add([pscustomobject]@{ Member = $relative; Source = $sources[0]; Sha256 = $hash })
                }
            }
            finally { $package.Dispose() }
        }
        finally { $file.Dispose() }
        if ($unresolved.Count -ne 0) {
            throw "The package ships members that no file tracked at its producing commit $short produces and that are not generated outputs: $($unresolved -join ', ')."
        }
        if ($ambiguous.Count -ne 0) {
            throw "The package ships members that match more than one file tracked at its producing commit ${short}: $($ambiguous -join ', ')."
        }
        if ($members.Count -eq 0) { throw "The package ships no file tracked at its producing commit $short, so that producer cannot be verified." }
        # The producer files as checked out, in batches that keep each git command line short.
        $sourcePaths = [Collections.Generic.List[string]]::new()
        $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($member in $members) { if ($seen.Add($member.Source)) { $sourcePaths.Add($member.Source) } }
        $checkout = [Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
        $batch = [Collections.Generic.List[string]]::new(); $length = 0
        for ($i = 0; $i -le $sourcePaths.Count; $i++) {
            if ($i -lt $sourcePaths.Count -and ($batch.Count -eq 0 -or $length + $sourcePaths[$i].Length -lt 12000)) {
                $batch.Add(':(literal)' + $sourcePaths[$i]); $length += $sourcePaths[$i].Length + 16; continue
            }
            $archiveBytes = Invoke-KmgGitBytes -RepositoryRoot $root -Arguments (@('archive', '--format=zip', '-0', $Commit, '--') + @($batch))
            $commitArchive = New-Object IO.Compression.ZipArchive((New-Object IO.MemoryStream(, $archiveBytes)), [IO.Compression.ZipArchiveMode]::Read)
            try {
                foreach ($entry in $commitArchive.Entries) {
                    if ($entry.FullName.EndsWith('/')) { continue }
                    $stream = $entry.Open()
                    try { $checkout[$entry.FullName] = [BitConverter]::ToString($sha.ComputeHash($stream)) } finally { $stream.Dispose() }
                }
            }
            finally { $commitArchive.Dispose() }
            $batch.Clear(); $length = 0
            if ($i -lt $sourcePaths.Count) { $i-- }
        }
    }
    finally { $sha.Dispose() }
    $differing = @($members | Where-Object { -not $checkout.ContainsKey($_.Source) -or $checkout[$_.Source] -cne $_.Sha256 } | ForEach-Object {
            if ($_.Member -ceq $_.Source) { $_.Member } else { "$($_.Member) (producer file $($_.Source))" } })
    if ($differing.Count -ne 0) {
        throw "The packaged content does not match its producing commit ${short}: $($differing -join ', ') differ from that commit's files as checked out; the package was not produced from that commit's inputs."
    }
    return $members.Count
}
