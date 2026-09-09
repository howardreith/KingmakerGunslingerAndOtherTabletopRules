[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',
    [string]$BackupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod',
    [string]$EvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence',
    [switch]$PassThru,
    [ValidateSet('0.0.114','0.0.117')][string]$ProducerVersion = '0.0.114'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$deploymentWhatIfRequested = [bool]$WhatIfPreference
$WhatIfPreference = $false
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

# The established script path/default preserves the qualified 114 caller.
# Version 117 requires the explicit pinned-producer selection.
$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$identity = Get-KmgQualifiedElementalProducerIdentity -Version $ProducerVersion -RepositoryRoot $root
$expectedVersion = $identity.Version
$expectedCommit = $identity.Commit
$expectedPackageSha = $identity.PackageSha256
$expectedDllSha = $identity.DllSha256
$expectedDllMvid = $identity.DllMvid
$expectedEntryCount = 135
$expectedPackage = $identity.PackagePath
$package = (Resolve-Path -LiteralPath $PackagePath).Path
if (-not $package.Equals($expectedPackage,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "The qualified legacy producer must be the exact repository release artifact: $expectedPackage"
}
$releaseManifestPath = $identity.ReleaseManifestPath
$release = Get-Content -LiteralPath $releaseManifestPath -Raw |
    ConvertFrom-Json
if ($release.schemaVersion -ne 1 -or
    $release.generator -cne 'scripts/Publish-Release.ps1' -or
    $release.version -cne $expectedVersion -or
    $release.commit -cne $expectedCommit -or
    $release.package -cne [IO.Path]::GetFileName($expectedPackage) -or
    $release.packageSha256 -cne $expectedPackageSha -or
    $release.dllSha256 -cne $expectedDllSha -or
    $release.packageValidated -ne $true) {
    throw 'The authoritative pinned public release manifest does not match the pinned migration producer.'
}
if ((Get-KmgSha256 -Path $package) -cne $expectedPackageSha) {
    throw 'The authoritative pinned public package SHA-256 is not exact.'
}

Assert-KmgNotRunning
$live = (Resolve-Path -LiteralPath $LiveModDirectory).Path
$expectedLive = [IO.Path]::GetFullPath(
    'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger')
if (-not $live.Equals($expectedLive,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing legacy deployment outside the exact live mod directory: $live"
}
$requiredEvidenceRoot = [IO.Path]::GetFullPath(
    'C:\Dev\KingmakerGunslingerLab\runtime-evidence').TrimEnd('\')
if (-not [IO.Path]::GetFullPath($EvidenceRoot).TrimEnd('\').Equals(
        $requiredEvidenceRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Evidence root must be exactly: $requiredEvidenceRoot"
}

$temporary = Join-Path ([IO.Path]::GetTempPath()) (
    'KmgQualifiedElementalRaces114-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporary | Out-Null
try {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $entries = @($archive.Entries)
        $unsafeEntries = @($entries | Where-Object {
            [string]::IsNullOrEmpty($_.Name) -or
                -not $_.FullName.StartsWith('KingmakerGunslinger/',
                    [StringComparison]::Ordinal) -or
                $_.FullName.Contains('..') -or
                $_.FullName.Contains('\')
        })
        if ($entries.Count -ne $expectedEntryCount -or
            $unsafeEntries.Count -ne 0) {
            throw 'The pinned public archive entry catalog is unsafe or incomplete.'
        }
        $archiveRelativeFiles = @($entries | ForEach-Object {
            $_.FullName.Substring('KingmakerGunslinger/'.Length)
        } | Sort-Object)
    }
    finally { $archive.Dispose() }

    Expand-Archive -LiteralPath $package -DestinationPath $temporary
    $source = Join-Path $temporary 'KingmakerGunslinger'
    $info = Get-Content -LiteralPath (Join-Path $source 'Info.json') -Raw |
        ConvertFrom-Json
    $dll = Join-Path $source 'KingmakerGunslinger.dll'
    $actualSourceFiles = @(Get-ChildItem -LiteralPath $source -Recurse -File |
        ForEach-Object {
            $_.FullName.Substring($source.Length).TrimStart('\')
        } | Sort-Object)
    if ($info.Id -cne 'KingmakerGunslinger' -or
        $info.Version -cne $expectedVersion -or
        (Get-KmgSha256 -Path $dll) -cne $expectedDllSha -or
        (Get-KmgDllMvid -Path $dll) -cne $expectedDllMvid -or
        ($actualSourceFiles -join "`n") -cne
            (($archiveRelativeFiles -replace '/', '\') -join "`n")) {
        throw 'The extracted pinned public package identity or file catalog is not exact.'
    }

    $WhatIfPreference = $deploymentWhatIfRequested
    if (-not $PSCmdlet.ShouldProcess($live,
            'back up the live mod and deploy the explicitly pinned public elemental producer')) {
        $WhatIfPreference = $false
        Write-Host 'Dry run only; the pinned package was validated and no deployment occurred.'
        return
    }
    $WhatIfPreference = $false
    $ConfirmPreference = 'None'
    $backup = & (Join-Path $PSScriptRoot 'Backup-Live-Mod.ps1') `
        -LiveModDirectory $live -BackupRoot $BackupRoot -Confirm:$false
    $settings = Join-Path $live 'FeatureModules.json'
    $settingsExisted = Test-Path -LiteralPath $settings -PathType Leaf
    $settingsBytes = if ($settingsExisted) {
        [IO.File]::ReadAllBytes($settings)
    } else { $null }
    try {
        foreach ($child in Get-ChildItem -LiteralPath $live -Force) {
            $target = Assert-KmgPathWithin -Path $child.FullName -Root $live
            Remove-Item -LiteralPath $target -Recurse -Force
        }
        Copy-Item -Path (Join-Path $source '*') -Destination $live `
            -Recurse -Force
    }
    finally {
        if ($settingsExisted) {
            $settingsTemporary = $settings + '.kmg-qualified-114-restore.tmp'
            [IO.File]::WriteAllBytes($settingsTemporary, $settingsBytes)
            Move-Item -LiteralPath $settingsTemporary -Destination $settings `
                -Force
        }
        elseif (Test-Path -LiteralPath $settings -PathType Leaf) {
            Remove-Item -LiteralPath $settings -Force
        }
    }

    $deployedInfo = Get-Content -LiteralPath (Join-Path $live 'Info.json') `
        -Raw | ConvertFrom-Json
    $deployedDll = Join-Path $live 'KingmakerGunslinger.dll'
    $actualLiveFiles = @(Get-ChildItem -LiteralPath $live -Recurse -File |
        Where-Object { $_.FullName -ne $settings } |
        ForEach-Object {
            $_.FullName.Substring($live.Length).TrimStart('\')
        } | Sort-Object)
    $expectedLiveFiles = @($actualSourceFiles | Where-Object {
        $_ -cne 'FeatureModules.json'
    })
    if ($deployedInfo.Version -cne $expectedVersion -or
        (Get-KmgSha256 -Path $deployedDll) -cne $expectedDllSha -or
        (Get-KmgDllMvid -Path $deployedDll) -cne $expectedDllMvid -or
        ($actualLiveFiles -join "`n") -cne ($expectedLiveFiles -join "`n")) {
        throw 'Pinned public deployment verification failed; use the recorded explicit backup.'
    }

    $deploymentDirectory = Join-Path $EvidenceRoot (
        'deployments\' + [DateTime]::UtcNow.ToString(
            'yyyyMMddTHHmmssfffffffZ'))
    New-Item -ItemType Directory -Path $deploymentDirectory | Out-Null
    $deployment = [ordered]@{
        schemaVersion = 1
        authority = $identity.Authority
        deployedAtUtc = [DateTime]::UtcNow.ToString('o')
        packagePath = $package
        packageSha256 = $expectedPackageSha
        releaseManifestPath = (Resolve-Path -LiteralPath `
            $releaseManifestPath).Path
        releaseManifestSha256 = Get-KmgSha256 -Path $releaseManifestPath
        commit = $expectedCommit
        version = $expectedVersion
        archiveEntryCount = $expectedEntryCount
        dllSha256 = $expectedDllSha
        dllMvid = $expectedDllMvid
        deployedDllSha256 = Get-KmgSha256 -Path $deployedDll
        featureModuleSettingsPreserved = $settingsExisted
        featureModuleSettingsSha256 = if ($settingsExisted) {
            Get-KmgSha256 -Path $settings
        } else { '<absent>' }
        liveModDirectory = $live
        backupDirectory = $backup.Destination
        files = $actualLiveFiles
    }
    $deploymentPath = Join-Path $deploymentDirectory `
        ("qualified-elemental-races-$expectedVersion-deployment.json")
    $deployment | ConvertTo-Json -Depth 5 | Set-Content `
        -LiteralPath $deploymentPath -Encoding UTF8
    Write-Host "Pinned public $expectedVersion deployment verified; manifest: $deploymentPath"
    if ($PassThru) { Write-Output $deploymentPath }
}
finally {
    if (Test-Path -LiteralPath $temporary) {
        [void](Assert-KmgPathWithin -Path $temporary -Root ([IO.Path]::GetTempPath()))
        Remove-Item -LiteralPath $temporary -Recurse -Force
    }
}
