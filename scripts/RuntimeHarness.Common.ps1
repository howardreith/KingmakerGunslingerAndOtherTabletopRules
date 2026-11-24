Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot 'common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeCoordination.Common.ps1')
. (Join-Path $PSScriptRoot 'DevelopmentCandidate.Common.ps1')

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
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [switch]$AllowDirtyGit,
        # Names the one development purpose this caller may consume. Without
        # it, a development candidate or its deployment is refused.
        [string]$DevelopmentPurpose
    )
    $deploymentPath = (Resolve-Path -LiteralPath $DeploymentManifestPath).Path
    $requiredRoot = [IO.Path]::GetFullPath(
        'C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments')
    [void](Assert-KmgPathWithin -Path $deploymentPath -Root $requiredRoot)
    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    $development = -not [string]::IsNullOrEmpty($DevelopmentPurpose)
    if ($development) {
        [void](Assert-KmgDevelopmentPurpose $DevelopmentPurpose)
        $buildPath = Get-KmgDevelopmentReceiptPath -PackagePath $package
    } else {
        Assert-KmgNotDevelopmentCandidate -PackagePath $package -RepositoryRoot $RepositoryRoot
        $buildPath = Get-KmgPackageManifestPath -PackagePath $package
    }
    if (-not (Test-Path -LiteralPath $buildPath -PathType Leaf)) {
        throw "Reusable deployment build manifest is missing: $buildPath"
    }
    $build = Get-Content -LiteralPath $buildPath -Raw | ConvertFrom-Json
    $deployment = Get-Content -LiteralPath $deploymentPath -Raw | ConvertFrom-Json
    $git = Get-KmgGitState -RepositoryRoot $RepositoryRoot
    if ($git.Status.Count -ne 0 -and -not $AllowDirtyGit) {
        throw 'Reusable runtime execution requires an exactly clean Git state.'
    }
    $sourceStateSha256 = Get-KmgSourceStateFingerprint `
        -RepositoryRoot $RepositoryRoot
    if ($development) {
        Assert-KmgDevelopmentReceipt -Receipt $build -PackagePath $package `
            -RepositoryRoot $RepositoryRoot -Purpose $DevelopmentPurpose
        Assert-KmgDevelopmentSourceBinding -Receipt $build -Git $git `
            -SourceStateSha256 $sourceStateSha256 -ExpectedVersion $ExpectedVersion
        Assert-KmgDevelopmentDeploymentRecord -Deployment $deployment `
            -ReceiptPath $buildPath -Purpose $DevelopmentPurpose
    } else {
        if ($build.schemaVersion -ne 1 -or $build.validated -ne $true -or
            $build.generator -ne 'scripts/Build-Local.ps1' -or
            $build.packagePath -ne $package -or $build.commit -ne $git.Commit -or
            $build.version -ne $ExpectedVersion -or
            $build.sourceStateSha256 -cnotmatch '^[0-9a-f]{64}$' -or
            $build.sourceStateSha256 -cne $sourceStateSha256 -or
            [string]::IsNullOrWhiteSpace([string]$build.firearmManifestSha256) -or
            [string]::IsNullOrWhiteSpace([string]$build.firearmSoundBankSha256)) {
            throw 'Reusable build identity does not match the exact current source state/commit/version.'
        }
        Assert-KmgReleaseDeploymentRecord -Deployment $deployment
    }
    if ((Get-KmgSha256 -Path $package) -ne $build.packageSha256) {
        throw 'Reusable package SHA-256 no longer matches its immutable build manifest.'
    }
    if ($deployment.schemaVersion -ne 2 -or
        $deployment.packagePath -ne $package -or
        $deployment.packageSha256 -ne $build.packageSha256 -or
        $deployment.commit -ne $build.commit -or
        $deployment.sourceStateSha256 -ne $build.sourceStateSha256 -or
        $deployment.version -ne $ExpectedVersion -or
        $deployment.dllSha256 -ne $build.dllSha256 -or
        $deployment.dllMvid -ne $build.dllMvid -or
        $deployment.firearmManifestSha256 -ne $build.firearmManifestSha256 -or
        $deployment.firearmSoundBankSha256 -ne $build.firearmSoundBankSha256 -or
        $deployment.deployedFirearmManifestSha256 -ne $build.firearmManifestSha256 -or
        $deployment.deployedFirearmSoundBankSha256 -ne $build.firearmSoundBankSha256) {
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
    $firearmManifest = Join-Path $live `
        'assets\soundbanks\firearm-soundbank-manifest.json'
    $firearmSoundBank = Join-Path $live `
        'assets\soundbanks\KMG_Firearms.bnk'
    $info = Get-Content -LiteralPath (Join-Path $live 'Info.json') -Raw |
        ConvertFrom-Json
    if ($info.Version -ne $ExpectedVersion -or
        (Get-KmgSha256 -Path $dll) -ne $build.dllSha256 -or
        (Get-KmgDllMvid -Path $dll) -ne $build.dllMvid -or
        (Get-KmgSha256 -Path $bundle) -ne $deployment.firearmBundleSha256 -or
        (Get-KmgSha256 -Path $firearmManifest) -ne $build.firearmManifestSha256 -or
        (Get-KmgSha256 -Path $firearmSoundBank) -ne $build.firearmSoundBankSha256) {
        throw 'Installed version, DLL SHA/MVID, firearm bundle, or firearm audio differs from the reusable deployment.'
    }
    $settings = Join-Path $live 'FeatureModules.json'
    $settingsExists = Test-Path -LiteralPath $settings -PathType Leaf
    $settingsHash = if ($settingsExists) { Get-KmgSha256 -Path $settings } else { '<absent>' }
    Write-Host ('Reusable artifact verified: commit={0};sourceState={1};version={2};package={3};dll={4};mvid={5};installedDll={6};bundle={7};manifest={8};bank={9};settings={10}' -f
        $git.Commit, $sourceStateSha256, $ExpectedVersion,
        $build.packageSha256, $build.dllSha256,
        $build.dllMvid, (Get-KmgSha256 -Path $dll),
        $deployment.firearmBundleSha256, $build.firearmManifestSha256,
        $build.firearmSoundBankSha256, $settingsHash)
    return [pscustomobject]@{
        Build = $build
        Deployment = $deployment
        PackagePath = $package
        DeploymentManifestPath = $deploymentPath
        SettingsExists = $settingsExists
        SettingsSha256 = $settingsHash
    }
}

function Test-KmgFlatJsonEquivalent {
    param(
        [Parameter(Mandatory = $true)][string]$LeftPath,
        [Parameter(Mandatory = $true)][string]$RightPath
    )
    try {
        $left = Get-Content -LiteralPath $LeftPath -Raw | ConvertFrom-Json
        $right = Get-Content -LiteralPath $RightPath -Raw | ConvertFrom-Json
    }
    catch { return $false }
    if ($null -eq $left -or $null -eq $right) { return $false }
    [string[]]$leftNames = @($left.PSObject.Properties | ForEach-Object Name)
    [string[]]$rightNames = @($right.PSObject.Properties | ForEach-Object Name)
    [Array]::Sort($leftNames, [StringComparer]::Ordinal)
    [Array]::Sort($rightNames, [StringComparer]::Ordinal)
    if (($leftNames -join "`n") -cne ($rightNames -join "`n")) {
        return $false
    }
    foreach ($name in $leftNames) {
        $leftValue = $left.PSObject.Properties[$name].Value
        $rightValue = $right.PSObject.Properties[$name].Value
        if ($leftValue -is [Management.Automation.PSCustomObject] -or
            $rightValue -is [Management.Automation.PSCustomObject] -or
            ($leftValue -is [Collections.IEnumerable] -and
                $leftValue -isnot [string]) -or
            ($rightValue -is [Collections.IEnumerable] -and
                $rightValue -isnot [string])) {
            return $false
        }
        $leftJson = ConvertTo-Json -InputObject $leftValue -Compress
        $rightJson = ConvertTo-Json -InputObject $rightValue -Compress
        if ($leftJson -cne $rightJson) { return $false }
    }
    return $true
}

function Assert-KmgQualifiedLegacyRuntimeOverlay {
    param(
        [Parameter(Mandatory = $true)][string]$LiveDirectory,
        [Parameter(Mandatory = $true)][string[]]$ExpectedFiles,
        [Parameter(Mandatory = $true)][string]$ExpectedSettingsSha256,
        [Parameter(Mandatory = $true)][string]$ExpectedDllSha256,
        [Parameter(Mandatory = $true)][string]$ExpectedDllMvid
    )
    $live = (Resolve-Path -LiteralPath $LiveDirectory).Path.TrimEnd('\')
    $settings = Join-Path $live 'FeatureModules.json'
    $settingsPrevious = $settings + '.previous'
    $ordinaryFiles = [Collections.Generic.List[string]]::new()
    $runtimeGenerated = [Collections.Generic.List[string]]::new()
    $cacheFiles = [Collections.Generic.List[string]]::new()
    foreach ($file in @(Get-ChildItem -LiteralPath $live -Recurse -File)) {
        $relative = $file.FullName.Substring($live.Length).TrimStart('\')
        if ($relative -ceq 'FeatureModules.json') { continue }
        if ($relative -ceq 'FeatureModules.json.previous') {
            $runtimeGenerated.Add($relative)
            continue
        }
        if ($relative -cmatch
            '^KingmakerGunslinger\.dll\.[1-9][0-9]*\.cache$') {
            $runtimeGenerated.Add($relative)
            $cacheFiles.Add($file.FullName)
            continue
        }
        $ordinaryFiles.Add($relative)
    }
    $ordinary = @($ordinaryFiles | Sort-Object)
    $expected = @($ExpectedFiles | Sort-Object)
    if (($ordinary -join "`n") -cne ($expected -join "`n")) {
        throw 'Installed 0.0.114 file catalog contains a missing, changed-name, or unapproved extra file.'
    }
    if ($cacheFiles.Count -gt 1) {
        throw 'Installed 0.0.114 contains more than one runtime-generated DLL cache.'
    }
    foreach ($cacheFile in $cacheFiles) {
        if ((Get-KmgSha256 -Path $cacheFile) -cne $ExpectedDllSha256 -or
            (Get-KmgDllMvid -Path $cacheFile) -cne $ExpectedDllMvid) {
            throw 'Installed 0.0.114 runtime-generated DLL cache differs from the exact qualified DLL.'
        }
    }

    $settingsExists = Test-Path -LiteralPath $settings -PathType Leaf
    $previousExists = Test-Path -LiteralPath $settingsPrevious -PathType Leaf
    if ($ExpectedSettingsSha256 -ceq '<absent>') {
        if ($settingsExists -or $previousExists) {
            throw 'Installed 0.0.114 unexpectedly created feature settings.'
        }
        $settingsSha = '<absent>'
        $settingsMode = 'absent'
    }
    else {
        if (-not $settingsExists) {
            throw 'Installed 0.0.114 feature settings are unexpectedly absent.'
        }
        $settingsSha = Get-KmgSha256 -Path $settings
        if ($previousExists -and
            (Get-KmgSha256 -Path $settingsPrevious) -cne
                $ExpectedSettingsSha256) {
            throw 'Installed 0.0.114 feature-settings backup differs from the exact deployed bytes.'
        }
        if ($settingsSha -ceq $ExpectedSettingsSha256) {
            $settingsMode = 'exact'
        }
        elseif (-not $previousExists -or
            -not (Test-KmgFlatJsonEquivalent -LeftPath $settings `
                -RightPath $settingsPrevious)) {
            throw 'Installed 0.0.114 feature settings changed semantically or without an exact backup.'
        }
        else {
            $settingsMode = 'normalized-with-exact-backup'
        }
    }
    return [pscustomobject]@{
        SettingsExists = $settingsExists
        SettingsSha256 = $settingsSha
        SettingsMode = $settingsMode
        RuntimeGeneratedFiles = @($runtimeGenerated | Sort-Object)
    }
}

function Get-KmgQualifiedElementalProducerIdentity {
    param(
        [Parameter(Mandatory = $true)][ValidateSet('0.0.114','0.0.117')][string]$Version,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot
    )
    # Only these two immutable public producers are authorized by the current
    # save-compatibility mission. No caller-supplied hash or arbitrary version.
    if ($Version -ceq '0.0.114') {
        $commit = '6874dc15a27ded132456dbdd480f47c794543a05'
        $packageName = 'KingmakerGunslinger-0.0.114-elemental-races.zip'
        $packageSha = 'b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694'
        $dllSha = '09af96b95e2abfa39e45f30c8ccb4cb1e8772981dd3be17846f07cbbd2dd8262'
        $mvid = 'dcd73856-39d4-40ce-9b05-77bf249103d7'
    } else {
        $commit = 'f8a2fd996752afb0e361a53bec175328ace5435a'
        $packageName = 'KingmakerGunslinger-0.0.117-elemental-char-gen-stabilization.zip'
        $packageSha = '9368c1ff2c82b76574bab5ed75868d7eb633e759f925e82a1c0da7e861f62f6f'
        $dllSha = 'fd2fc61c250b13857d81acc197a896450f5b242ee392fa7192b01201e908f35f'
        $mvid = '18f5eaaa-3021-4836-8763-9ba22965b958'
    }
    return [pscustomobject]@{
        Version=$Version; Commit=$commit; PackageSha256=$packageSha; DllSha256=$dllSha; DllMvid=$mvid
        PackagePath=[IO.Path]::GetFullPath((Join-Path $RepositoryRoot ("artifacts/release/$Version/$packageName")))
        ReleaseManifestPath=Join-Path $RepositoryRoot ("artifacts/release/$Version/release-manifest.json")
        Authority="qualified-elemental-races-$Version-release"
    }
}

function Assert-KmgQualifiedElementalRaces114Deployment {
    param(
        [Parameter(Mandatory = $true)][string]$DeploymentManifestPath,
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [switch]$AllowDirtyGit,
        [ValidateSet('0.0.114','0.0.117')][string]$ProducerVersion = '0.0.114'
    )
    $identity = Get-KmgQualifiedElementalProducerIdentity -Version $ProducerVersion -RepositoryRoot $RepositoryRoot
    $expectedVersion = $identity.Version
    $expectedCommit = $identity.Commit
    $expectedPackageSha = $identity.PackageSha256
    $expectedDllSha = $identity.DllSha256
    $expectedDllMvid = $identity.DllMvid
    $expectedPackage = $identity.PackagePath
    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    if (-not $package.Equals($expectedPackage,
            [StringComparison]::OrdinalIgnoreCase) -or
        (Get-KmgSha256 -Path $package) -cne $expectedPackageSha) {
        throw 'Qualified legacy reuse requires the exact pinned public elemental release package.'
    }
    $deploymentPath = (Resolve-Path -LiteralPath `
        $DeploymentManifestPath).Path
    $requiredRoot = [IO.Path]::GetFullPath(
        'C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments')
    [void](Assert-KmgPathWithin -Path $deploymentPath -Root $requiredRoot)
    $deployment = Get-Content -LiteralPath $deploymentPath -Raw |
        ConvertFrom-Json
    $git = Get-KmgGitState -RepositoryRoot $RepositoryRoot
    if ($git.Status.Count -ne 0 -and -not $AllowDirtyGit) {
        throw 'Qualified legacy runtime execution requires an exactly clean Git state.'
    }
    if ($deployment.schemaVersion -ne 1 -or
        $deployment.authority -cne
            $identity.Authority -or
        $deployment.packagePath -cne $package -or
        $deployment.packageSha256 -cne $expectedPackageSha -or
        $deployment.commit -cne $expectedCommit -or
        $deployment.version -cne $expectedVersion -or
        $deployment.archiveEntryCount -ne 135 -or
        $deployment.dllSha256 -cne $expectedDllSha -or
        $deployment.dllMvid -cne $expectedDllMvid -or
        $deployment.deployedDllSha256 -cne $expectedDllSha) {
        throw 'Qualified legacy deployment manifest identity is not exact.'
    }
    $live = [IO.Path]::GetFullPath($deployment.liveModDirectory)
    $expectedLive = [IO.Path]::GetFullPath(
        'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger')
    if (-not $live.Equals($expectedLive,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Qualified legacy deployment points outside the exact live mod directory.'
    }
    $info = Get-Content -LiteralPath (Join-Path $live 'Info.json') -Raw |
        ConvertFrom-Json
    $dll = Join-Path $live 'KingmakerGunslinger.dll'
    if ($info.Version -cne $expectedVersion -or
        (Get-KmgSha256 -Path $dll) -cne $expectedDllSha -or
        (Get-KmgDllMvid -Path $dll) -cne $expectedDllMvid) {
        throw 'Installed pinned public version or DLL identity differs from its qualified deployment.'
    }
    $overlay = Assert-KmgQualifiedLegacyRuntimeOverlay `
        -LiveDirectory $live -ExpectedFiles @($deployment.files) `
        -ExpectedSettingsSha256 $deployment.featureModuleSettingsSha256 `
        -ExpectedDllSha256 $expectedDllSha -ExpectedDllMvid $expectedDllMvid
    Write-Host ('Qualified legacy artifact verified: producerCommit={0};version={1};package={2};dll={3};mvid={4};settings={5};settingsMode={6};runtimeOverlay={7}' -f
        $expectedCommit, $expectedVersion, $expectedPackageSha,
        $expectedDllSha, $expectedDllMvid, $overlay.SettingsSha256,
        $overlay.SettingsMode, ($overlay.RuntimeGeneratedFiles -join ','))
    return [pscustomobject]@{
        Deployment = $deployment
        PackagePath = $package
        DeploymentManifestPath = $deploymentPath
        Version = $expectedVersion
        DllSha256 = $expectedDllSha
        SettingsExists = $overlay.SettingsExists
        SettingsSha256 = $overlay.SettingsSha256
        SettingsMode = $overlay.SettingsMode
        RuntimeGeneratedFiles = $overlay.RuntimeGeneratedFiles
    }
}

function Read-KmgBuildLocalManifest {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        # Names the one development purpose this caller may consume. Without
        # it, a development candidate is refused; with it, only a development
        # candidate built for that purpose is accepted.
        [string]$DevelopmentPurpose,
        # Admission for staging (Deploy-Local, the loader harness): the candidate's material inputs must equal its
        # producing commit. Callers name an explicitly historical control with -HistoricalReleaseControl instead.
        [switch]$RequireCurrentMaterialInputs
    )
    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    if (-not [string]::IsNullOrEmpty($DevelopmentPurpose)) {
        $receipt = Read-KmgDevelopmentCandidate -PackagePath $package `
            -RepositoryRoot $RepositoryRoot -Purpose $DevelopmentPurpose
        $measured = & (Join-Path $PSScriptRoot 'validate-package.ps1') -PackagePath $package -PassThru | Select-Object -Last 1
        Assert-KmgReceiptDescribesPackagedDll -Receipt $receipt -Measured $measured
        if ($RequireCurrentMaterialInputs) { Assert-KmgCandidateMaterialInputsCurrent -RepositoryRoot $RepositoryRoot -Commit ([string]$receipt.commit) }
        $receipt | Add-Member -NotePropertyName measuredDll -NotePropertyValue $measured -Force
        return $receipt
    }
    Assert-KmgNotDevelopmentCandidate -PackagePath $package -RepositoryRoot $RepositoryRoot
    $manifestPath = Get-KmgPackageManifestPath -PackagePath $package
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "Build-Local manifest is missing: $manifestPath"
    }
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $info = Get-KmgModInfo -RepositoryRoot $RepositoryRoot
    if ($manifest.schemaVersion -ne 1 -or $manifest.packagePath -ne $package -or
        $manifest.generator -ne 'scripts/Build-Local.ps1' -or
        $manifest.validated -ne $true -or
        $manifest.sourceStateSha256 -cnotmatch '^[0-9a-f]{64}$' -or
        [string]::IsNullOrWhiteSpace([string]$manifest.dllMvid) -or
        [string]::IsNullOrWhiteSpace([string]$manifest.firearmManifestSha256) -or
        [string]::IsNullOrWhiteSpace([string]$manifest.firearmSoundBankSha256)) {
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
    $measured = & (Join-Path $PSScriptRoot 'validate-package.ps1') -PackagePath $package -PassThru | Select-Object -Last 1
    Assert-KmgReceiptDescribesPackagedDll -Receipt $manifest -Measured $measured
    if ($RequireCurrentMaterialInputs) { Assert-KmgCandidateMaterialInputsCurrent -RepositoryRoot $RepositoryRoot -Commit ([string]$manifest.commit) }
    $manifest | Add-Member -NotePropertyName measuredDll -NotePropertyValue $measured -Force
    return $manifest
}

# The DLL inside the finished ZIP must be the build its receipt names (SHA-256 and MVID), and carry the receipt's source
# stamp (the producing commit in its informational version) when the build recorded one.
function Assert-KmgReceiptDescribesPackagedDll {
    param([Parameter(Mandatory = $true)]$Receipt, $Measured)
    if ($null -eq $Measured) { throw 'Package validation returned no measured DLL identity.' }
    if ([string]$Receipt.dllSha256 -cne [string]$Measured.sha256 -or [string]$Receipt.dllMvid -cne [string]$Measured.mvid) {
        throw "The DLL inside the package is not the build its receipt names (receipt $($Receipt.dllSha256)/$($Receipt.dllMvid), packaged $($Measured.sha256)/$($Measured.mvid))."
    }
    # A DLL that carries its source stamp (AssemblyMetadata GitCommit, written by the exact-reference builder) must have
    # been built from the receipt's producing commit.
    $embedded = if ($Measured.PSObject.Properties['gitCommit'] -and $Measured.gitCommit) { [string]$Measured.gitCommit } else { '' }
    if ($embedded -and $embedded -cne [string]$Receipt.commit) {
        throw "The DLL inside the package was built from $embedded, not the receipt's producing commit $($Receipt.commit)."
    }
    # A receipt that records the packaged DLL or the embedded source stamp must agree with the measured DLL; a recorded
    # stamp requires the measured DLL to carry it.
    if ($Receipt.PSObject.Properties['packagedDllSha256'] -and [string]$Receipt.packagedDllSha256 -cne [string]$Measured.sha256) {
        throw "The DLL inside the package is not the packaged DLL its receipt records ($($Receipt.packagedDllSha256))."
    }
    if ($Receipt.PSObject.Properties['embeddedGitCommit']) {
        if (-not $embedded) { throw "The receipt records the source stamp $($Receipt.embeddedGitCommit), but the DLL inside the package carries none." }
        if ([string]$Receipt.embeddedGitCommit -cne $embedded -or [string]$Receipt.embeddedGitCommit -cne [string]$Receipt.commit) {
            throw "The receipt's recorded source stamp $($Receipt.embeddedGitCommit) is not the packaged DLL's stamp $embedded or its producing commit."
        }
    }
    # A receipt that records the exact compiler output must name that same DLL.
    if ($Receipt.PSObject.Properties['compilerDllSha256'] -and
        ([string]$Receipt.compilerDllSha256 -cne [string]$Measured.sha256 -or [string]$Receipt.compilerDllMvid -cne [string]$Measured.mvid)) {
        throw "The DLL inside the package is not the exact compiler output its receipt records ($($Receipt.compilerDllSha256))."
    }
}

# An explicitly identified historical release, admitted only by its committed release record
# (scripts/historical-release-records.json): the package bytes must be the recorded release package, the release tag
# must resolve to the recorded commit, and the packaged DLL must be the recorded DLL built from that commit. A
# development package or any other build of the same version is refused.
function Read-KmgHistoricalReleasePackage {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$Version
    )
    # The trusted record is the one committed at HEAD; a working-tree record that differs from it is refused, so an
    # uncommitted edit can never authorize another package.
    $previous = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    try { $committedText = (@(& git -C $RepositoryRoot show 'HEAD:scripts/historical-release-records.json' 2>$null) -join "`n"); $showExit = $LASTEXITCODE }
    finally { $ErrorActionPreference = $previous }
    if ($showExit -ne 0 -or -not $committedText) { throw 'The historical release record is not committed at HEAD.' }
    $records = $committedText | ConvertFrom-Json
    $workingRecords = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'scripts\historical-release-records.json') -Raw | ConvertFrom-Json
    if (($workingRecords | ConvertTo-Json -Depth 8 -Compress) -cne ($records | ConvertTo-Json -Depth 8 -Compress)) { throw 'The working-tree historical release record differs from the committed record; commit or revert it.' }
    if ($records.schemaVersion -ne 1) { throw 'The historical release record has an unknown schema.' }
    $record = @($records.releases | Where-Object { [string]$_.version -ceq $Version })
    if ($record.Count -ne 1) { throw "No historical release record names version '$Version'." }
    $record = $record[0]
    $package = (Resolve-Path -LiteralPath $PackagePath).ProviderPath
    $packageSha = Get-KmgSha256 -Path $package
    if ($packageSha -cne [string]$record.packageSha256) {
        throw "The package is not the recorded v$Version release package (package $packageSha, recorded $($record.packageSha256))."
    }
    $previous = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    try { $tagCommit = (& git -C $RepositoryRoot rev-parse --verify --quiet ("refs/tags/$($record.tag)^{commit}") 2>$null | Select-Object -First 1) }
    finally { $ErrorActionPreference = $previous }
    if ([string]$tagCommit -cne [string]$record.tagCommit) { throw "The release tag $($record.tag) does not resolve to the recorded commit $($record.tagCommit)." }
    $extraction = Join-Path ([IO.Path]::GetTempPath()) ('KmgHistoricalRelease-' + [Guid]::NewGuid().ToString('N'))
    [void][IO.Directory]::CreateDirectory($extraction)
    try {
        $modDirectory = Expand-KmgPackageEntries -ZipPath $package -Destination $extraction -ModId 'KingmakerGunslinger'
        $info = Get-Content -LiteralPath (Join-Path $modDirectory 'Info.json') -Raw | ConvertFrom-Json
        if ([string]$info.Version -cne $Version) { throw "The package declares version $($info.Version), not $Version." }
        $identity = Get-KmgPackagedDllIdentity -DllPath (Join-Path $modDirectory 'KingmakerGunslinger.dll')
    }
    finally { Remove-Item -LiteralPath $extraction -Recurse -Force -ErrorAction SilentlyContinue }
    if ([string]$identity.sha256 -cne [string]$record.dllSha256 -or [string]$identity.mvid -cne [string]$record.dllMvid -or
        [string]$identity.gitCommit -cne [string]$record.tagCommit) {
        throw "The packaged DLL is not the recorded v$Version release DLL built from $($record.tagCommit)."
    }
    return [pscustomobject]@{ historicalRelease = $true; version = $Version; releaseTag = [string]$record.tag; commit = [string]$record.tagCommit
        packagePath = $package; packageSha256 = $packageSha; dllSha256 = [string]$identity.sha256; dllMvid = [string]$identity.mvid; measuredDll = $identity }
}

# Deploy-Local admits the candidate again under the runtime lease; it must be the candidate the first admission accepted.
function Assert-KmgSameAdmission {
    param([Parameter(Mandatory = $true)]$First, [Parameter(Mandatory = $true)]$Second)
    foreach ($field in @('packageSha256', 'dllSha256', 'dllMvid', 'commit', 'version')) {
        if ([string]$Second.$field -cne [string]$First.$field) { throw "The candidate changed between admission and the deployment lease ($field); nothing was deployed." }
    }
    if ([bool]($First.PSObject.Properties['historicalRelease'] -and $First.historicalRelease) -ne [bool]($Second.PSObject.Properties['historicalRelease'] -and $Second.historicalRelease)) {
        throw 'The candidate changed between admission and the deployment lease (admission mode); nothing was deployed.'
    }
}

# What a deployment expects of the extracted package and records, from its admission. A Build-Local candidate's audio
# digests come from its receipt; a historical release has no receipt, so its audio is the audio inside the digest-bound
# release package its committed record admitted, and its record names the release tag. Every record field must exist.
function Get-KmgDeploymentExpectations {
    param(
        [Parameter(Mandatory = $true)]$Admission,
        [switch]$HistoricalRelease,
        [Parameter(Mandatory = $true)][string]$PackagedFirearmManifestSha256,
        [Parameter(Mandatory = $true)][string]$PackagedFirearmSoundBankSha256
    )
    $isHistorical = [bool]($Admission.PSObject.Properties['historicalRelease'] -and $Admission.historicalRelease)
    if ([bool]$HistoricalRelease -ne $isHistorical) { throw 'The deployment mode does not match the admission.' }
    if ($isHistorical) {
        $expected = [ordered]@{ firearmManifestSha256 = $PackagedFirearmManifestSha256; firearmSoundBankSha256 = $PackagedFirearmSoundBankSha256
            branch = 'tag:' + [string]$Admission.releaseTag; sourceStateSha256 = 'historical-release ' + [string]$Admission.releaseTag }
        if ([string]::IsNullOrEmpty([string]$Admission.releaseTag)) { throw 'A deployment-record field is missing for this admission (releaseTag); nothing was deployed.' }
    }
    else {
        $expected = [ordered]@{ firearmManifestSha256 = [string]$Admission.firearmManifestSha256; firearmSoundBankSha256 = [string]$Admission.firearmSoundBankSha256
            branch = [string]$Admission.branch; sourceStateSha256 = [string]$Admission.sourceStateSha256 }
    }
    if ($PackagedFirearmManifestSha256 -cne $expected.firearmManifestSha256 -or $PackagedFirearmSoundBankSha256 -cne $expected.firearmSoundBankSha256) {
        throw 'Extracted package firearm audio differs from its immutable build manifest.'
    }
    foreach ($field in @('branch', 'sourceStateSha256')) { if ([string]::IsNullOrEmpty([string]$expected[$field])) { throw "A deployment-record field is missing for this admission ($field); nothing was deployed." } }
    foreach ($field in @('packagePath', 'packageSha256', 'commit', 'version', 'dllSha256', 'dllMvid')) {
        if (-not $Admission.PSObject.Properties[$field] -or [string]::IsNullOrEmpty([string]$Admission.$field)) { throw "A deployment-record field is missing for this admission ($field); nothing was deployed." }
    }
    return [pscustomobject]$expected
}

# Staging takes an admitted package from a private copy: the copy must have the admitted package digest, so bytes that
# replace the supplied path after admission are never staged.
function New-KmgAdmittedPackageCopy {
    param([Parameter(Mandatory = $true)]$Admission, [Parameter(Mandatory = $true)][string]$Destination)
    if (Test-Path -LiteralPath $Destination) { throw "The admitted-copy directory already exists: $Destination" }
    [void][IO.Directory]::CreateDirectory($Destination)
    $copy = Join-Path $Destination 'admitted-package.zip'
    [IO.File]::Copy([string]$Admission.packagePath, $copy)
    if ((Get-KmgSha256 -Path $copy) -cne [string]$Admission.packageSha256) {
        Remove-Item -LiteralPath $Destination -Recurse -Force
        throw 'The package changed after admission; nothing was staged.'
    }
    return $copy
}
# Extracts the admitted private copy (checked entries) into an empty destination and requires the extracted DLL to be
# the admitted DLL (SHA-256 and MVID) before anything is replaced. Returns the extracted mod directory.
function Expand-KmgAdmittedPackage {
    param([Parameter(Mandatory = $true)]$Admission, [Parameter(Mandatory = $true)][string]$AdmittedCopy, [Parameter(Mandatory = $true)][string]$Destination)
    if ((Get-KmgSha256 -Path $AdmittedCopy) -cne [string]$Admission.packageSha256) { throw 'The admitted package copy changed; nothing was staged.' }
    $modDirectory = Expand-KmgPackageEntries -ZipPath $AdmittedCopy -Destination $Destination -ModId 'KingmakerGunslinger'
    $dll = Join-Path $modDirectory 'KingmakerGunslinger.dll'
    if ((Get-KmgSha256 -Path $dll) -cne [string]$Admission.dllSha256 -or (Get-KmgDllMvid -Path $dll) -cne [string]$Admission.dllMvid) {
        throw 'The extracted DLL is not the admitted DLL; nothing was staged.'
    }
    return $modDirectory
}

function Assert-KmgExplicitCandidateIdentity {
    param($Manifest, $Git, [string]$SourceStateSha256, [string]$ExpectedVersion, [switch]$AllowDirtyGit)
    if ($Manifest.version -cne $ExpectedVersion -or
        ($Git.Status.Count -ne 0 -and -not $AllowDirtyGit) -or
        $Manifest.commit -cne $Git.Commit -or
        $Manifest.sourceStateSha256 -cne $SourceStateSha256) {
        throw 'Explicit candidate requires its exact version, current source state and commit before deployment.'
    }
}

function Assert-KmgReleaseReproducibilityIdentity {
    param($Manifest, $Git, [string]$SourceStateSha256, [string]$ExpectedVersion, [object[]]$References)
    Assert-KmgExplicitCandidateIdentity -Manifest $Manifest -Git $Git `
        -SourceStateSha256 $SourceStateSha256 -ExpectedVersion $ExpectedVersion
    if (-not $Manifest.PSObject.Properties['completeDomainSuite'] -or
        $Manifest.completeDomainSuite -ne $true -or
        -not $Manifest.PSObject.Properties['reproducibilityBuild'] -or
        $Manifest.reproducibilityBuild -ne $false) {
        throw 'Reproducibility requires the original complete-domain primary build receipt.'
    }
    $original = @($Manifest.references.files | ForEach-Object { $_.relativePath + '|' + $_.sha256 } | Sort-Object)
    $current = @($References | ForEach-Object { $_.relativePath + '|' + $_.sha256 } | Sort-Object)
    if ($original.Count -eq 0 -or ($original -join "`n") -cne ($current -join "`n")) {
        throw 'Reproducibility reference identities differ from the primary build.'
    }
}

function Assert-KmgReleaseScratch {
    param([string]$RepositoryRoot, $Layout, [string]$PrimaryArchive, [switch]$Reproducibility)
    [void](Assert-KmgPathWithin -Path $Layout.Root -Root (Join-Path $RepositoryRoot 'artifacts\local-runtime'))
    [void](Assert-KmgPathWithin -Path $Layout.ScratchRoot -Root (Join-Path $RepositoryRoot 'artifacts\obj\release-exact'))
    if ($PrimaryArchive) {
        [void](Assert-KmgPathWithin -Path $PrimaryArchive -Root (Join-Path $RepositoryRoot 'artifacts\local-runtime'))
    }
    # The Python compiler replaces its output tree. Check the exact bounded
    # paths and every existing ancestor before admitting that operation.
    foreach ($path in @($Layout.Root, $Layout.ScratchRoot, $PrimaryArchive)) {
        if ([string]::IsNullOrEmpty($path)) { continue }
        $cursor = Assert-KmgPathWithin -Path $path -Root $RepositoryRoot
        while ($cursor -and $cursor.TrimEnd('\') -ine $RepositoryRoot.TrimEnd('\')) {
            if ((Test-Path -LiteralPath $cursor) -and
                ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
                throw 'Release build output or ancestor is a reparse point.'
            }
            $cursor = Split-Path -Parent $cursor
        }
    }
    if (Test-Path -LiteralPath $Layout.Root) {
        throw 'This exact final candidate output already exists; preserve its bytes.'
    }
    if (-not $Reproducibility) {
        if (Test-Path -LiteralPath $Layout.ScratchRoot) { throw 'Preserve existing primary build scratch from an incomplete attempt.' }
        return
    }
    $scratch = @(Get-KmgReleaseOutputRows $Layout.ScratchRoot)
    $archived = @(Get-KmgReleaseOutputRows $PrimaryArchive)
    if ($scratch.Count -eq 0 -or ($scratch -join "`n") -cne ($archived -join "`n")) {
        throw 'Scratch differs from the preserved primary archive; refuse replacement.'
    }
}

function Get-KmgGitState {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot)
    $commit = (& git -C $RepositoryRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Unable to read Git commit.' }
    # Git emits no stdout for a detached frozen checkout. Preserve that valid
    # empty branch identity; commit, status and fingerprints still bind reuse.
    $branchOutput = @(& git -C $RepositoryRoot branch --show-current)
    if ($LASTEXITCODE -ne 0) { throw 'Unable to read Git branch.' }
    $branch = ($branchOutput -join "`n").Trim()
    $status = @(& git -C $RepositoryRoot status --porcelain)
    if ($LASTEXITCODE -ne 0) { throw 'Unable to read Git status.' }
    return [ordered]@{ Commit = $commit; Branch = $branch; Status = @($status) }
}

function Get-KmgSourceStateFingerprint {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot)
    $root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $git = Get-KmgGitState -RepositoryRoot $root
    $diff = @(& git -C $root -c core.safecrlf=false diff --no-ext-diff `
        --binary HEAD --)
    if ($LASTEXITCODE -ne 0) { throw 'Unable to read the tracked Git diff.' }
    $untracked = @(& git -C $root -c core.quotepath=false ls-files `
        --others --exclude-standard | Sort-Object)
    if ($LASTEXITCODE -ne 0) { throw 'Unable to enumerate untracked files.' }
    $payload = New-Object Text.StringBuilder
    [void]$payload.Append($git.Commit).Append("`nstatus`n")
    foreach ($line in $git.Status) {
        [void]$payload.Append($line).Append("`n")
    }
    [void]$payload.Append("tracked-diff`n")
    foreach ($line in $diff) {
        [void]$payload.Append($line).Append("`n")
    }
    [void]$payload.Append("untracked`n")
    foreach ($relativePath in $untracked) {
        $path = Join-Path $root $relativePath
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "Untracked source-state file is missing: $relativePath"
        }
        [void]$payload.Append($relativePath).Append('|').Append(
            (Get-KmgSha256 -Path $path)).Append("`n")
    }
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes($payload.ToString())
        return ([BitConverter]::ToString($sha.ComputeHash($bytes)) `
            -replace '-', '').ToLowerInvariant()
    }
    finally { $sha.Dispose() }
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
