[CmdletBinding()]
param(
    [string]$LogPath =
        'C:\Users\howar\AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\output_log.txt',
    [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,119}$')]
    [string]$ConfigurationId,
    [string]$KingmakerInstallDir =
        'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-Sha256([string]$Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
}

function Get-Matches([string]$Text, [string]$Pattern) {
    return @([regex]::Matches($Text, $Pattern,
        [Text.RegularExpressions.RegexOptions]::IgnoreCase))
}

function Get-GroupCounts([object[]]$Matches, [int]$GroupIndex) {
    return @($Matches | ForEach-Object { $_.Groups[$GroupIndex].Value } |
        Group-Object | Sort-Object Name | ForEach-Object {
            [ordered]@{ fingerprint = $_.Name; count = $_.Count }
        })
}

function Get-DllMvid([string]$Path) {
    try {
        $assembly = [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
        return $assembly.ManifestModule.ModuleVersionId.ToString('D')
    }
    catch { return '<unavailable>' }
}

$evidenceRoot = [IO.Path]::GetFullPath(
    'C:\Dev\KingmakerGunslingerLab\runtime-evidence').TrimEnd('\')
$evidence = [IO.Path]::GetFullPath($EvidenceDirectory).TrimEnd('\')
if (-not $evidence.StartsWith($evidenceRoot + '\',
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "Evidence directory must be beneath $evidenceRoot"
}
if (-not (Test-Path -LiteralPath $evidence -PathType Container)) {
    throw "Evidence directory is missing: $evidence"
}
if (-not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
    throw "Kingmaker output log is missing: $LogPath"
}
if (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -ne 0) {
    throw 'Kingmaker must have exited before the attribution log is collected.'
}

$rawPath = Join-Path $evidence ("output_log-$ConfigurationId.txt")
[IO.File]::Copy((Resolve-Path -LiteralPath $LogPath).Path, $rawPath, $true)
$text = [IO.File]::ReadAllText($rawPath)

$unsupported = @(Get-Matches $text `
    "WARNING: Shader Unsupported: '([^']+)' - All passes removed"
)
$fallback = @(Get-Matches $text `
    "WARNING: Shader Unsupported: '([^']+)' - Setting to default shader\."
)
$gpu = @(Get-Matches $text `
    'ERROR: Shader Shader is not supported on this GPU \(none of subshaders/fallbacks are suitable\)'
)
$particleMesh = @(Get-Matches $text `
    'Mesh used in Particle System Shape Module is not valid, possibly due to missing read/write flag'
)
$missingScript = @(Get-Matches $text `
    "The referenced script on this Behaviour \(Game Object '([^']*)'\) is missing!"
)
$lightmap = @(Get-Matches $text `
    'The loaded level has a different lightmaps mode than the current one\.[^\r\n]*'
)
$zeroArea = @(Get-Matches $text 'zero surface area')
$mainTex = @(Get-Matches $text `
    "Material doesn't have a texture property '_MainTex'"
)

$modsPath = Join-Path $KingmakerInstallDir 'Mods'
$mods = @()
if (Test-Path -LiteralPath $modsPath -PathType Container) {
    foreach ($directory in @(Get-ChildItem -LiteralPath $modsPath -Directory |
        Sort-Object Name)) {
        $infoPath = @('Info.json', 'info.json') | ForEach-Object {
            Join-Path $directory.FullName $_
        } | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
            Select-Object -First 1
        if ($null -eq $infoPath) { continue }
        try { $info = Get-Content -Raw -LiteralPath $infoPath | ConvertFrom-Json }
        catch { continue }
        $dlls = @(Get-ChildItem -LiteralPath $directory.FullName -File -Filter '*.dll' |
            Sort-Object Name | ForEach-Object {
                [ordered]@{
                    name = $_.Name
                    sha256 = Get-Sha256 $_.FullName
                    mvid = Get-DllMvid $_.FullName
                }
            })
        $mods += [ordered]@{
            directory = $directory.Name
            ummId = [string]$info.Id
            version = [string]$info.Version
            infoSha256 = Get-Sha256 $infoPath
            assemblies = $dlls
        }
    }
}

$kmg = Join-Path $modsPath 'KingmakerGunslinger'
$featurePath = Join-Path $kmg 'FeatureModules.json'
$bundleNames = @('kingmakergunslinger.firearms',
    'kingmakergunslinger.elvenbranchedspear',
    'kingmakergunslinger.easternweapons')
$bundles = @($bundleNames | ForEach-Object {
    $path = Join-Path $kmg "assets\bundles\$_"
    [ordered]@{
        name = $_
        exists = Test-Path -LiteralPath $path -PathType Leaf
        sha256 = if (Test-Path -LiteralPath $path -PathType Leaf) {
            Get-Sha256 $path
        } else { '<missing>' }
        byteLength = if (Test-Path -LiteralPath $path -PathType Leaf) {
            (Get-Item -LiteralPath $path).Length
        } else { 0 }
    }
})

$favoredJson = @(
    'bonus_charmed_life.json', 'bonus_panache.json',
    'arcane_archer.json', 'deadeye_devotee.json') | ForEach-Object {
        [ordered]@{
            file = $_
            loadMentions = @(Get-Matches $text ([regex]::Escape($_))).Count
        }
    }

$summary = [ordered]@{
    schemaVersion = 1
    configurationId = $ConfigurationId
    capturedUtc = [DateTime]::UtcNow.ToString('o')
    sourceLog = [ordered]@{
        path = (Resolve-Path -LiteralPath $LogPath).Path
        sha256 = Get-Sha256 $rawPath
        byteLength = (Get-Item -LiteralPath $rawPath).Length
        retainedPath = $rawPath
    }
    featureModules = [ordered]@{
        exists = Test-Path -LiteralPath $featurePath -PathType Leaf
        sha256 = if (Test-Path -LiteralPath $featurePath -PathType Leaf) {
            Get-Sha256 $featurePath
        } else { '<missing>' }
        byteLength = if (Test-Path -LiteralPath $featurePath -PathType Leaf) {
            (Get-Item -LiteralPath $featurePath).Length
        } else { 0 }
        state = if (Test-Path -LiteralPath $featurePath -PathType Leaf) {
            Get-Content -Raw -LiteralPath $featurePath | ConvertFrom-Json
        } else { $null }
    }
    installedMods = $mods
    kmgBundles = $bundles
    counts = [ordered]@{
        unsupportedShaderAllPassesRemoved = $unsupported.Count
        unsupportedShaderFallback = $fallback.Count
        unsupportedShaderGpuError = $gpu.Count
        invalidParticleMeshReadWrite = $particleMesh.Count
        missingSerializedScript = $missingScript.Count
        lightmapModeMismatch = $lightmap.Count
        zeroSurfaceArea = $zeroArea.Count
        missingMainTexProperty = $mainTex.Count
        favoredClassComponentAppliedOnceOnLevelUp =
            @(Get-Matches $text 'ZFavoredClass\.NewMechanics\.ComponentAppliedOnceOnLevelUp\.OnFactActivate').Count
        polymorphTransition = @(Get-Matches $text 'Polymorph\.Transition').Count
        polymorphTryReplaceView = @(Get-Matches $text 'Polymorph\.TryReplaceView').Count
        polymorphRestoreView = @(Get-Matches $text 'Polymorph\.RestoreView').Count
        polymorphOnFactActivate = @(Get-Matches $text 'Polymorph\.OnFactActivate').Count
        polymorphOnFactDeactivate = @(Get-Matches $text 'Polymorph\.OnFactDeactivate').Count
        unitDescriptorDispose = @(Get-Matches $text 'UnitDescriptor\.Dispose').Count
        unitFxVisibilityManagerUpdate =
            @(Get-Matches $text 'UnitFxVisibilityManager\.Update').Count
    }
    shaderFingerprints = [ordered]@{
        allPassesRemoved = Get-GroupCounts $unsupported 1
        fallback = Get-GroupCounts $fallback 1
    }
    missingScriptFingerprints = Get-GroupCounts $missingScript 1
    lightmapFingerprints = @($lightmap | ForEach-Object { $_.Value } |
        Group-Object | Sort-Object Name | ForEach-Object {
            [ordered]@{ fingerprint = $_.Name; count = $_.Count }
        })
    favoredJsonFiles = $favoredJson
    runtimeResult = [ordered]@{
        exists = Test-Path -LiteralPath (Join-Path $evidence 'runtime-result.json')
        sha256 = if (Test-Path -LiteralPath (Join-Path $evidence `
            'runtime-result.json') -PathType Leaf) {
            Get-Sha256 (Join-Path $evidence 'runtime-result.json')
        } else { '<missing>' }
    }
}

$summaryPath = Join-Path $evidence `
    "compatibility-attribution-log-$ConfigurationId.json"
$json = $summary | ConvertTo-Json -Depth 12
[IO.File]::WriteAllText($summaryPath, $json + [Environment]::NewLine,
    (New-Object Text.UTF8Encoding($false)))
[pscustomobject]@{
    configurationId = $ConfigurationId
    rawLogPath = $rawPath
    rawLogSha256 = Get-Sha256 $rawPath
    summaryPath = $summaryPath
    summarySha256 = Get-Sha256 $summaryPath
}
