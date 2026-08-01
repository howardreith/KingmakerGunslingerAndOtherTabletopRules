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
        $manifest.generator -ne 'scripts/Build-Local.ps1' -or $manifest.validated -ne $true) {
        throw 'Build-Local manifest has an invalid schema or package path.'
    }
    $allowedPackageRoot = Join-Path $RepositoryRoot 'artifacts\local-runtime'
    [void](Assert-KmgPathWithin -Path $package -Root $allowedPackageRoot)
    if ($manifest.version -ne $info.Version -or $manifest.version -ne '0.0.37') {
        throw "Build-Local package version is not the required 0.0.37: $($manifest.version)"
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
