Set-StrictMode -Version Latest

function Get-KmgRepositoryRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ScriptDirectory
    )

    return (Resolve-Path (Join-Path $ScriptDirectory '..')).Path
}

function Get-KmgModInfo {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $infoPath = Join-Path $RepositoryRoot 'Info.json'
    if (-not (Test-Path -LiteralPath $infoPath -PathType Leaf)) {
        throw "Info.json was not found at $infoPath"
    }

    return Get-Content -LiteralPath $infoPath -Raw | ConvertFrom-Json
}

function Resolve-KmgMsBuild {
    param(
        [string]$ExplicitPath
    )

    if ($ExplicitPath) {
        if (-not (Test-Path -LiteralPath $ExplicitPath -PathType Leaf)) {
            throw "MSBuild was not found at the supplied path: $ExplicitPath"
        }

        return (Resolve-Path -LiteralPath $ExplicitPath).Path
    }

    $command = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $programFilesX86 = [Environment]::GetFolderPath('ProgramFilesX86')
    if ($programFilesX86) {
        $vsWhere = Join-Path $programFilesX86 'Microsoft Visual Studio\Installer\vswhere.exe'
        if (Test-Path -LiteralPath $vsWhere -PathType Leaf) {
            $candidate = & $vsWhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
            if ($candidate -and (Test-Path -LiteralPath $candidate -PathType Leaf)) {
                return (Resolve-Path -LiteralPath $candidate).Path
            }
        }
    }

    throw 'MSBuild.exe was not found. Install Visual Studio 2022 Build Tools with the .NET desktop build tools workload, or pass -MSBuildPath.'
}

function Get-KmgSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

# One layout for the builder and its build-then-consume callers. Explicit older
# packages still use their original manifests; this selects only new candidates.
function Get-KmgLocalCandidateLayout {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$Commit,
        [Parameter(Mandatory = $true)][string]$Id,
        [Parameter(Mandatory = $true)][string]$Version,
        [switch]$Reproducibility
    )
    if ($Commit -cnotmatch '^[0-9a-f]{40}$' -or
        $Id -cne 'KingmakerGunslinger' -or
        $Version -cnotmatch '^[0-9]+\.[0-9]+\.[0-9]+$') {
        throw 'Candidate layout requires an exact commit, mod ID and version.'
    }
    $root = Join-Path $RepositoryRoot ("artifacts\local-runtime\$Version\" + $Commit.Substring(0, 12))
    if ($Reproducibility) { $root += '-reproducibility' }
    return [pscustomobject]@{
        Root = $root
        PackagePath = Join-Path $root "$Id-$Version-local-runtime.zip"
        # Identical compiler/PDB paths in both builds; each immutable candidate
        # archives its own copy before the scratch output can be replaced.
        ScratchRoot = Join-Path $RepositoryRoot ("artifacts\obj\release-exact\$Version\" + $Commit.Substring(0, 12))
        ArchivedExactRoot = Join-Path $root 'exact-build'
    }
}

function Get-KmgReleaseOutputRows {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { throw 'Primary archive or scratch is missing.' }
    $prefix = (Resolve-Path -LiteralPath $Path).Path.TrimEnd('\') + '\'
    @(Get-ChildItem -LiteralPath $Path -Recurse -Force | Sort-Object FullName | ForEach-Object {
        if ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Release scratch/archive contains a reparse entry.' }
        $relative = $_.FullName.Substring($prefix.Length)
        if ($_.PSIsContainer) { 'directory|' + $relative }
        else { 'file|' + $relative + '|' + $_.Length + '|' + (Get-KmgSha256 -Path $_.FullName) }
    })
}

# Extracts a mod ZIP into an empty destination after checking every entry's normalized destination: each must lie under
# <Destination>\<ModId>\, with no '.' or '..' segment, no duplicate, and nothing may land at the extraction root. The
# entries checked are exactly the entries extracted. Returns the extracted mod directory.
function Expand-KmgPackageEntries {
    param(
        [Parameter(Mandatory = $true)][string]$ZipPath,
        [Parameter(Mandatory = $true)][string]$Destination,
        [Parameter(Mandatory = $true)][string]$ModId
    )
    Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
    $destinationRoot = [IO.Path]::GetFullPath($Destination).TrimEnd('\') + '\'
    if (-not (Test-Path -LiteralPath $destinationRoot -PathType Container) -or @(Get-ChildItem -LiteralPath $destinationRoot -Force).Count -ne 0) {
        throw "The extraction destination must be an existing empty directory: $destinationRoot"
    }
    $modRoot = $destinationRoot + $ModId + '\'
    $archive = [IO.Compression.ZipFile]::OpenRead($ZipPath)
    try {
        $outside = [Collections.Generic.List[string]]::new()
        $files = [Collections.Generic.List[object]]::new()
        $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach ($entry in $archive.Entries) {
            $name = $entry.FullName
            $normalized = $name.Replace('\', '/')
            $isDirectory = $normalized.EndsWith('/')
            $segments = @($normalized.TrimEnd('/').Split('/'))
            $target = [IO.Path]::GetFullPath([IO.Path]::Combine($destinationRoot, $normalized.Replace('/', '\')))
            $insideMod = $target.StartsWith($modRoot, [StringComparison]::OrdinalIgnoreCase) -or ($isDirectory -and ($target.TrimEnd('\') + '\').Equals($modRoot, [StringComparison]::OrdinalIgnoreCase))
            if ($normalized.StartsWith('/') -or $normalized.Contains(':') -or @($segments | Where-Object { $_ -eq '.' -or $_ -eq '..' -or $_ -eq '' }).Count -ne 0 -or -not $insideMod) {
                $outside.Add($name); continue
            }
            if (-not $isDirectory) {
                if (-not $seen.Add($target)) { throw "Package contains a duplicate entry: $name" }
                $files.Add([pscustomobject]@{ Entry = $entry; Target = $target })
            }
        }
        if ($outside.Count -ne 0) { throw "Package contains entries outside the '$ModId' mod root: $($outside -join ', ')" }
        foreach ($file in $files) {
            [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($file.Target))
            [IO.Compression.ZipFileExtensions]::ExtractToFile($file.Entry, $file.Target, $false)
        }
    }
    finally { $archive.Dispose() }
    $rootFiles = @(Get-ChildItem -LiteralPath $destinationRoot -File -Force)
    $rootDirectories = @(Get-ChildItem -LiteralPath $destinationRoot -Directory -Force)
    if ($rootFiles.Count -ne 0 -or $rootDirectories.Count -ne 1 -or $rootDirectories[0].Name -cne $ModId) {
        throw "Package must contain exactly one root directory named '$ModId' and nothing beside it."
    }
    return $rootDirectories[0].FullName
}
