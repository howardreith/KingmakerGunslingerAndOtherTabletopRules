Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot 'common.ps1')

function Assert-KmgNotRunning {
    $processes = @(Get-Process -Name 'Kingmaker' -ErrorAction SilentlyContinue)
    if ($processes.Count -gt 0) {
        throw "Pathfinder: Kingmaker is running (PID(s): $($processes.Id -join ', '))."
    }
}

function Assert-KmgPathWithin {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Root,
        [switch]$AllowRoot
    )
    $fullPath = [IO.Path]::GetFullPath($Path).TrimEnd('\')
    $fullRoot = [IO.Path]::GetFullPath($Root).TrimEnd('\')
    if ((-not $AllowRoot -and $fullPath -eq $fullRoot) -or
        (-not $fullPath.StartsWith($fullRoot + '\', [StringComparison]::OrdinalIgnoreCase))) {
        throw "Path escapes required root '$fullRoot': $fullPath"
    }
    return $fullPath
}

function Get-KmgPackageManifestPath {
    param([Parameter(Mandatory = $true)][string]$PackagePath)
    return "$([IO.Path]::GetFullPath($PackagePath)).build-local.json"
}

function Get-KmgDllMvid {
    param([Parameter(Mandatory = $true)][string]$Path)
    $resolved = (Resolve-Path -LiteralPath $Path).Path
    $assembly = [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($resolved))
    return $assembly.ManifestModule.ModuleVersionId.ToString('D')
}

function Assert-KmgReusableDeployment {
    param(
        [Parameter(Mandatory = $true)][string]$DeploymentManifestPath,
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion
    )
    $deploymentPath = (Resolve-Path -LiteralPath $DeploymentManifestPath).Path
    $requiredRoot = [IO.Path]::GetFullPath(
        'C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments')
    [void](Assert-KmgPathWithin -Path $deploymentPath -Root $requiredRoot)
    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    $buildPath = Get-KmgPackageManifestPath -PackagePath $package
    if (-not (Test-Path -LiteralPath $buildPath -PathType Leaf)) {
        throw "Reusable deployment build manifest is missing: $buildPath"
    }
    $build = Get-Content -LiteralPath $buildPath -Raw | ConvertFrom-Json
    $deployment = Get-Content -LiteralPath $deploymentPath -Raw | ConvertFrom-Json
    $git = Get-KmgGitState -RepositoryRoot $RepositoryRoot
    if ($git.Status.Count -ne 0) {
        throw 'Reusable runtime execution requires an exactly clean Git state.'
    }
    if ($build.schemaVersion -ne 1 -or $build.validated -ne $true -or
        $build.generator -ne 'scripts/Build-Local.ps1' -or
        $build.packagePath -ne $package -or $build.commit -ne $git.Commit -or
        $build.version -ne $ExpectedVersion) {
        throw 'Reusable build identity does not match the clean current commit/version.'
    }
    if ((Get-KmgSha256 -Path $package) -ne $build.packageSha256) {
        throw 'Reusable package SHA-256 no longer matches its immutable build manifest.'
    }
    if ($deployment.schemaVersion -ne 2 -or
        $deployment.packagePath -ne $package -or
        $deployment.packageSha256 -ne $build.packageSha256 -or
        $deployment.commit -ne $build.commit -or
        $deployment.version -ne $ExpectedVersion -or
        $deployment.dllSha256 -ne $build.dllSha256 -or
        $deployment.dllMvid -ne $build.dllMvid) {
        throw 'Reusable deployment identity does not match its immutable package/build identity.'
    }
    $live = [IO.Path]::GetFullPath($deployment.liveModDirectory)
    $expectedLive = [IO.Path]::GetFullPath(
        'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger')
    if (-not $live.Equals($expectedLive, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Reusable deployment points outside the exact live Gunslinger mod directory.'
    }
    $dll = Join-Path $live 'KingmakerGunslinger.dll'
    $bundle = Join-Path $live 'assets\bundles\kingmakergunslinger.firearms'
    $info = Get-Content -LiteralPath (Join-Path $live 'Info.json') -Raw |
        ConvertFrom-Json
    if ($info.Version -ne $ExpectedVersion -or
        (Get-KmgSha256 -Path $dll) -ne $build.dllSha256 -or
        (Get-KmgDllMvid -Path $dll) -ne $build.dllMvid -or
        (Get-KmgSha256 -Path $bundle) -ne $deployment.firearmBundleSha256) {
        throw 'Installed version, DLL SHA/MVID, or firearm bundle differs from the reusable deployment.'
    }
    $settings = Join-Path $live 'FeatureModules.json'
    $settingsExists = Test-Path -LiteralPath $settings -PathType Leaf
    $settingsHash = if ($settingsExists) { Get-KmgSha256 -Path $settings } else { '<absent>' }
    Write-Host ('Reusable artifact verified: commit={0};version={1};package={2};dll={3};mvid={4};installedDll={5};bundle={6};settings={7}' -f
        $git.Commit, $ExpectedVersion, $build.packageSha256, $build.dllSha256,
        $build.dllMvid, (Get-KmgSha256 -Path $dll),
        $deployment.firearmBundleSha256, $settingsHash)
    return [pscustomobject]@{
        Build = $build
        Deployment = $deployment
        PackagePath = $package
        DeploymentManifestPath = $deploymentPath
        SettingsExists = $settingsExists
        SettingsSha256 = $settingsHash
    }
}

function Read-KmgBuildLocalManifest {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot
    )
    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    $manifestPath = Get-KmgPackageManifestPath -PackagePath $package
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "Build-Local manifest is missing: $manifestPath"
    }
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $info = Get-KmgModInfo -RepositoryRoot $RepositoryRoot
    if ($manifest.schemaVersion -ne 1 -or $manifest.packagePath -ne $package -or
        $manifest.generator -ne 'scripts/Build-Local.ps1' -or
        $manifest.validated -ne $true -or
        [string]::IsNullOrWhiteSpace([string]$manifest.dllMvid)) {
        throw 'Build-Local manifest has an invalid schema or package path.'
    }
    $allowedPackageRoot = Join-Path $RepositoryRoot 'artifacts\local-runtime'
    [void](Assert-KmgPathWithin -Path $package -Root $allowedPackageRoot)
    if ($manifest.version -ne $info.Version) {
        throw "Build-Local package version does not match repository Info.json: $($manifest.version)"
    }
    $actualHash = Get-KmgSha256 -Path $package
    if ($manifest.packageSha256 -ne $actualHash) {
        throw 'Build-Local package hash does not match its manifest.'
    }
    & (Join-Path $PSScriptRoot 'validate-package.ps1') -PackagePath $package
    return $manifest
}

function Get-KmgGitState {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot)
    $commit = (& git -C $RepositoryRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Unable to read Git commit.' }
    $branch = (& git -C $RepositoryRoot branch --show-current).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Unable to read Git branch.' }
    $status = @(& git -C $RepositoryRoot status --porcelain)
    if ($LASTEXITCODE -ne 0) { throw 'Unable to read Git status.' }
    return [ordered]@{ Commit = $commit; Branch = $branch; Status = @($status) }
}

function Test-KmgDirectoryWritableByAcl {
    param([Parameter(Mandatory = $true)][string]$Path)
    $acl = Get-Acl -LiteralPath $Path
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principals = @($identity.User.Value, $identity.Name) + @($identity.Groups | ForEach-Object Value)
    $writeRights = [Security.AccessControl.FileSystemRights]::WriteData -bor
        [Security.AccessControl.FileSystemRights]::CreateFiles -bor
        [Security.AccessControl.FileSystemRights]::Modify -bor
        [Security.AccessControl.FileSystemRights]::FullControl
    $allow = $false
    foreach ($rule in $acl.Access) {
        $ruleIdentity = $rule.IdentityReference.Value
        try {
            $ruleIdentity = $rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value
        } catch [Security.Principal.IdentityNotMappedException] {
            # Keep the account-form identity and compare it without guessing.
        }
        if ($principals -notcontains $ruleIdentity -and $identity.Name -ne $rule.IdentityReference.Value) {
            continue
        }
        if (($rule.FileSystemRights -band $writeRights) -eq 0) { continue }
        if ($rule.AccessControlType -eq [Security.AccessControl.AccessControlType]::Deny) {
            return $false
        }
        $allow = $true
    }
    return $allow
}
