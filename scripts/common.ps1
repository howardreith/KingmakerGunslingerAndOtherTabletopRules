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
        [Parameter(Mandatory = $true)][string]$Version
    )
    if ($Commit -cnotmatch '^[0-9a-f]{40}$' -or
        $Id -cne 'KingmakerGunslinger' -or
        $Version -cnotmatch '^[0-9]+\.[0-9]+\.[0-9]+$') {
        throw 'Candidate layout requires an exact commit, mod ID and version.'
    }
    $root = Join-Path $RepositoryRoot ("artifacts\local-runtime\$Version\" + $Commit.Substring(0, 12))
    return [pscustomobject]@{
        Root = $root
        PackagePath = Join-Path $root "$Id-$Version-local-runtime.zip"
    }
}
