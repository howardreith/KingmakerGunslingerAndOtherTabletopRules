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
