[CmdletBinding()]
param(
    [string]$KingmakerInstallDir,

    [string]$MSBuildPath,

    [string]$ReleaseNotesPath = 'docs\RELEASE-NOTES-0.0.104.md',

    [string]$ReleaseBranch = 'master',

    [switch]$Publish,

    [switch]$ConfirmReleaseReady,

    [switch]$AllowNonDefaultReleaseBranch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')

function Assert-CommandAvailable {
    param([Parameter(Mandatory = $true)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "$Name is required but was not found on PATH."
    }
}

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter()][string[]]$Arguments = @()
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath failed with exit code $LASTEXITCODE."
    }
}

function Get-NativeCommandOutput {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter()][string[]]$Arguments = @()
    )

    $output = & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath failed with exit code $LASTEXITCODE."
    }

    return (($output | ForEach-Object { [string]$_ }) -join "`n").Trim()
}

function Test-NativeCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter()][string[]]$Arguments = @()
    )

    $priorErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'SilentlyContinue'
        & $FilePath @Arguments *> $null
        return ($LASTEXITCODE -eq 0)
    }
    finally {
        $ErrorActionPreference = $priorErrorActionPreference
    }
}

function Assert-FileExists {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Label was not found: $Path"
    }
}

function Resolve-RepositoryPath {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$Path
    )

    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }

    return [IO.Path]::GetFullPath((Join-Path $RepositoryRoot $Path))
}

function Invoke-QualifiedBuildAndPackage {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [string]$ConfiguredKingmakerInstallDir,
        [string]$ConfiguredMSBuildPath
    )

    $buildArguments = @{
        Configuration = 'Release'
        Clean = $true
    }
    if (-not [string]::IsNullOrWhiteSpace($ConfiguredKingmakerInstallDir)) {
        $buildArguments.KingmakerInstallDir = $ConfiguredKingmakerInstallDir
    }
    if (-not [string]::IsNullOrWhiteSpace($ConfiguredMSBuildPath)) {
        $buildArguments.MSBuildPath = $ConfiguredMSBuildPath
    }

    & (Join-Path $PSScriptRoot 'build.ps1') @buildArguments |
        ForEach-Object { Write-Host ([string]$_) }

    $packageOutput = @(
        & (Join-Path $PSScriptRoot 'package.ps1') -Configuration Release
    )
    $packagePath = [string]($packageOutput | Select-Object -Last 1)
    if ([string]::IsNullOrWhiteSpace($packagePath)) {
        throw 'package.ps1 did not return the generated package path.'
    }

    $packagePath = Resolve-RepositoryPath `
        -RepositoryRoot $RepositoryRoot `
        -Path $packagePath
    Assert-FileExists -Path $packagePath -Label 'Qualified UMM package'

    & (Join-Path $PSScriptRoot 'validate-package.ps1') `
        -PackagePath $packagePath

    $dllPath = Join-Path $RepositoryRoot `
        'artifacts\bin\Release\KingmakerGunslinger\KingmakerGunslinger.dll'
    Assert-FileExists -Path $dllPath -Label 'Qualified release DLL'

    return [pscustomobject]@{
        PackagePath = $packagePath
        PackageSha256 = Get-KmgSha256 -Path $packagePath
        DllPath = $dllPath
        DllSha256 = Get-KmgSha256 -Path $dllPath
    }
}

$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
Assert-CommandAvailable -Name 'git'
Assert-CommandAvailable -Name 'gh'
Assert-CommandAvailable -Name 'python'

Push-Location $root
try {
    $status = Get-NativeCommandOutput -FilePath 'git' -Arguments @(
        'status', '--porcelain'
    )
    if (-not [string]::IsNullOrWhiteSpace($status)) {
        throw 'Release publishing requires a clean working tree.'
    }

    Invoke-NativeCommand -FilePath 'gh' -Arguments @(
        'auth', 'status', '--hostname', 'github.com'
    )

    $repositoryJson = Get-NativeCommandOutput -FilePath 'gh' -Arguments @(
        'repo', 'view',
        '--json', 'nameWithOwner,defaultBranchRef,isPrivate'
    )
    $repositoryInfo = $repositoryJson | ConvertFrom-Json
    $repository = [string]$repositoryInfo.nameWithOwner
    $defaultBranch = [string]$repositoryInfo.defaultBranchRef.name
    $isPrivate = [bool]$repositoryInfo.isPrivate
    if ([string]::IsNullOrWhiteSpace($repository) -or
        [string]::IsNullOrWhiteSpace($defaultBranch)) {
        throw 'GitHub CLI did not return the repository or its default branch.'
    }
    if ($isPrivate) {
        throw 'Kingmaker Gunslinger is expected to be a public repository before publication.'
    }
    if ($defaultBranch -ne $ReleaseBranch -and
        -not $AllowNonDefaultReleaseBranch) {
        throw "GitHub's default branch is '$defaultBranch', not '$ReleaseBranch'. Change the repository default branch or pass -AllowNonDefaultReleaseBranch deliberately."
    }

    $currentBranch = Get-NativeCommandOutput -FilePath 'git' -Arguments @(
        'rev-parse', '--abbrev-ref', 'HEAD'
    )
    if ($currentBranch -ne $ReleaseBranch) {
        throw "Release publishing must run from '$ReleaseBranch'; current branch is '$currentBranch'."
    }

    Invoke-NativeCommand -FilePath 'git' -Arguments @(
        'fetch', '--prune', '--tags', 'origin', $ReleaseBranch
    )

    $head = Get-NativeCommandOutput -FilePath 'git' -Arguments @(
        'rev-parse', 'HEAD'
    )
    $remoteHead = Get-NativeCommandOutput -FilePath 'git' -Arguments @(
        'rev-parse', "origin/$ReleaseBranch"
    )
    if ($head -ne $remoteHead) {
        throw "HEAD ($head) must exactly match origin/$ReleaseBranch ($remoteHead) before publishing."
    }

    $origin = Get-NativeCommandOutput -FilePath 'git' -Arguments @(
        'remote', 'get-url', 'origin'
    )
    if ($origin -notmatch [Regex]::Escape($repository)) {
        throw "Origin '$origin' does not match GitHub repository '$repository'."
    }

    $info = Get-KmgModInfo -RepositoryRoot $root
    $id = [string]$info.Id
    $version = [string]$info.Version
    $displayName = [string]$info.DisplayName
    if ($id -ne 'KingmakerGunslinger') {
        throw "Unexpected UMM ID: $id"
    }
    if ([string]::IsNullOrWhiteSpace($version) -or
        $version -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$') {
        throw "Info.json does not contain valid semantic version text: $version"
    }
    if ([string]::IsNullOrWhiteSpace($displayName)) {
        $displayName = $id
    }

    [xml]$versionProps = Get-Content -LiteralPath `
        (Join-Path $root 'Directory.Build.props') -Raw
    $projectVersion = [string]$versionProps.Project.PropertyGroup.KmgVersion
    $informationalVersion = [string]$versionProps.Project.PropertyGroup.KmgInformationalVersion
    if ($projectVersion -ne $version -or
        -not $informationalVersion.StartsWith($version, [StringComparison]::Ordinal)) {
        throw 'Info.json and Directory.Build.props version metadata are inconsistent.'
    }

    $resolvedNotesPath = Resolve-RepositoryPath `
        -RepositoryRoot $root `
        -Path $ReleaseNotesPath
    Assert-FileExists -Path $resolvedNotesPath -Label 'Release notes file'
    $customNotes = (Get-Content -LiteralPath $resolvedNotesPath -Raw).Trim()
    if ([string]::IsNullOrWhiteSpace($customNotes)) {
        throw 'Release notes are empty.'
    }
    if (-not $customNotes.Contains($version)) {
        throw "Release notes do not mention current version $version."
    }
    if ($Publish -and -not $ConfirmReleaseReady) {
        throw 'Publication requires -ConfirmReleaseReady.'
    }
    if ($Publish -and
        $customNotes -match '(?i)not a public release|publication status:.*(?:local|draft)|release pending|not release-ready') {
        throw 'Release notes still identify the candidate as unpublished or not release-ready.'
    }

    $tag = "v$version"
    $title = "$displayName $tag"
    $existingRelease = $null
    if (Test-NativeCommand -FilePath 'gh' -Arguments @(
        'release', 'view', $tag, '--repo', $repository
    )) {
        $existingReleaseJson = Get-NativeCommandOutput -FilePath 'gh' -Arguments @(
            'release', 'view', $tag,
            '--repo', $repository,
            '--json', 'isDraft,isImmutable,url'
        )
        $existingRelease = $existingReleaseJson | ConvertFrom-Json
        if (-not [bool]$existingRelease.isDraft) {
            throw "Published GitHub release '$tag' already exists. Advance the version instead of replacing it."
        }
        if ([bool]$existingRelease.isImmutable) {
            throw "GitHub release '$tag' is immutable and cannot be refreshed."
        }
    }

    $first = Invoke-QualifiedBuildAndPackage `
        -RepositoryRoot $root `
        -ConfiguredKingmakerInstallDir $KingmakerInstallDir `
        -ConfiguredMSBuildPath $MSBuildPath
    $firstPackageCopy = Join-Path $root `
        "artifacts\release-work\$version\first\$([IO.Path]::GetFileName($first.PackagePath))"
    New-Item -ItemType Directory -Path (Split-Path -Parent $firstPackageCopy) -Force | Out-Null
    Copy-Item -LiteralPath $first.PackagePath -Destination $firstPackageCopy -Force

    $second = Invoke-QualifiedBuildAndPackage `
        -RepositoryRoot $root `
        -ConfiguredKingmakerInstallDir $KingmakerInstallDir `
        -ConfiguredMSBuildPath $MSBuildPath
    if ($first.PackageSha256 -cne $second.PackageSha256 -or
        $first.DllSha256 -cne $second.DllSha256) {
        throw "Deterministic release build failed: package $($first.PackageSha256)/$($second.PackageSha256), DLL $($first.DllSha256)/$($second.DllSha256)."
    }

    $statusAfterBuild = Get-NativeCommandOutput -FilePath 'git' -Arguments @(
        'status', '--porcelain'
    )
    if (-not [string]::IsNullOrWhiteSpace($statusAfterBuild)) {
        throw "Qualification modified tracked or unignored files:$([Environment]::NewLine)$statusAfterBuild"
    }

    $releaseDirectory = Join-Path $root "artifacts\release\$version"
    if (Test-Path -LiteralPath $releaseDirectory) {
        Remove-Item -LiteralPath $releaseDirectory -Recurse -Force
    }
    New-Item -ItemType Directory -Path $releaseDirectory -Force | Out-Null

    $assetName = [IO.Path]::GetFileName($second.PackagePath)
    $releasePackage = Join-Path $releaseDirectory $assetName
    Copy-Item -LiteralPath $second.PackagePath -Destination $releasePackage -Force
    & (Join-Path $PSScriptRoot 'validate-package.ps1') `
        -PackagePath $releasePackage

    $packageHash = Get-KmgSha256 -Path $releasePackage
    $checksumsPath = Join-Path $releaseDirectory 'SHA256SUMS.txt'
    "$packageHash  $assetName" |
        Set-Content -LiteralPath $checksumsPath -Encoding ASCII

    $releaseManifest = [ordered]@{
        schemaVersion = 1
        generator = 'scripts/Publish-Release.ps1'
        version = $version
        tag = $tag
        branch = $ReleaseBranch
        commit = $head
        package = $assetName
        packageSha256 = $packageHash
        dllSha256 = $second.DllSha256
        deterministicBuilds = 2
        packageValidated = $true
        publicationRequested = [bool]$Publish
    }
    $releaseManifestPath = Join-Path $releaseDirectory 'release-manifest.json'
    $releaseManifest | ConvertTo-Json -Depth 5 |
        Set-Content -LiteralPath $releaseManifestPath -Encoding UTF8

    if (Test-NativeCommand -FilePath 'git' -Arguments @(
        'show-ref', '--verify', '--quiet', "refs/tags/$tag"
    )) {
        $tagCommit = Get-NativeCommandOutput -FilePath 'git' -Arguments @(
            'rev-list', '-n', '1', $tag
        )
        if ($tagCommit -ne $head) {
            throw "Existing tag '$tag' resolves to $tagCommit, not release commit $head."
        }
    }
    else {
        Invoke-NativeCommand -FilePath 'git' -Arguments @(
            'tag', '-a', $tag, '-m', $title, $head
        )
    }

    if (-not (Test-NativeCommand -FilePath 'git' -Arguments @(
        'ls-remote', '--exit-code', '--tags', 'origin', "refs/tags/$tag"
    ))) {
        Invoke-NativeCommand -FilePath 'git' -Arguments @(
            'push', 'origin', "refs/tags/$tag"
        )
    }

    $generatedNotesLines = @(
        '## Installation',
        '',
        "1. Download **$assetName** from **Assets** below.",
        '2. In Unity Mod Manager, select Pathfinder: Kingmaker and drag the ZIP into the Mods tab.',
        "3. Launch the game and confirm **$displayName $version** is enabled.",
        '',
        "Do not download GitHub's automatically generated **Source code** archives; they are not the Unity Mod Manager package.",
        '',
        '## Verification',
        '',
        "SHA-256: $packageHash",
        '',
        "Release commit: $head",
        '',
        'The asset passed version-aware source validation, the complete dependency-free test suite, two clean deterministic Release builds, strict build-output validation, SoundBank validation, and strict standalone UMM package validation.'
    )
    $notes = $customNotes + [Environment]::NewLine +
        [Environment]::NewLine +
        ($generatedNotesLines -join [Environment]::NewLine)
    $generatedNotesPath = Join-Path $releaseDirectory "release-notes-$version.md"
    $notes | Set-Content -LiteralPath $generatedNotesPath -Encoding UTF8

    if ($null -eq $existingRelease) {
        $releaseArguments = @(
            'release', 'create', $tag,
            $releasePackage,
            $checksumsPath,
            $releaseManifestPath,
            '--repo', $repository,
            '--title', $title,
            '--notes-file', $generatedNotesPath,
            '--verify-tag',
            '--target', $head
        )
        if ($version.Contains('-')) {
            $releaseArguments += '--prerelease'
            $releaseArguments += '--latest=false'
        }
        elseif ($Publish) {
            $releaseArguments += '--latest'
        }
        if (-not $Publish) {
            $releaseArguments += '--draft'
        }
        Invoke-NativeCommand -FilePath 'gh' -Arguments $releaseArguments
    }
    else {
        Invoke-NativeCommand -FilePath 'gh' -Arguments @(
            'release', 'upload', $tag,
            $releasePackage,
            $checksumsPath,
            $releaseManifestPath,
            '--repo', $repository,
            '--clobber'
        )

        $editArguments = @(
            'release', 'edit', $tag,
            '--repo', $repository,
            '--title', $title,
            '--notes-file', $generatedNotesPath,
            '--verify-tag',
            '--target', $head
        )
        if ($Publish) {
            $editArguments += '--draft=false'
        }
        else {
            $editArguments += '--draft'
        }
        if ($version.Contains('-')) {
            $editArguments += '--prerelease'
            if ($Publish) {
                $editArguments += '--latest=false'
            }
        }
        else {
            $editArguments += '--prerelease=false'
            if ($Publish) {
                $editArguments += '--latest'
            }
        }
        Invoke-NativeCommand -FilePath 'gh' -Arguments $editArguments
    }

    $releaseUrl = Get-NativeCommandOutput -FilePath 'gh' -Arguments @(
        'release', 'view', $tag,
        '--repo', $repository,
        '--json', 'url',
        '--jq', '.url'
    )

    Write-Host "Release: $releaseUrl"
    Write-Host "State: $(if ($Publish) { 'published' } else { 'draft' })"
    Write-Host "Asset: $releasePackage"
    Write-Host "SHA-256: $packageHash"
}
finally {
    Pop-Location
}
