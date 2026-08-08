[CmdletBinding()]
param(
    [string]$ReferenceRoot = 'C:\Dev\KingmakerGunslingerLab\examples',
    [string]$OutputRoot,
    [string]$ApprovedRoot = 'C:\Dev\KingmakerGunslingerLab\examples'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-ExistingDirectory([string]$Path, [string]$Label) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "$Label does not exist: $Path"
    }
    return (Get-Item -LiteralPath $Path -Force).FullName.TrimEnd('\')
}

function Test-DescendantOrSame([string]$Path, [string]$Root) {
    return $Path.Equals($Root, [StringComparison]::OrdinalIgnoreCase) -or
        $Path.StartsWith($Root + '\', [StringComparison]::OrdinalIgnoreCase)
}

function Get-RelativePath([string]$Root, [string]$Path) {
    $rootUri = [Uri]($Root.TrimEnd('\') + '\')
    return [Uri]::UnescapeDataString($rootUri.MakeRelativeUri([Uri]$Path).ToString()).Replace('/', '\')
}

function Get-Sha256([string]$Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Read-Info([IO.FileInfo]$File) {
    if ($null -eq $File) { return $null }
    try { return (Get-Content -LiteralPath $File.FullName -Raw | ConvertFrom-Json) }
    catch { return [ordered]@{ parseError = $_.Exception.Message } }
}

function Read-Assembly([IO.FileInfo]$File) {
    try {
        $name = [Reflection.AssemblyName]::GetAssemblyName($File.FullName)
        $assembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($File.FullName)
        return [ordered]@{
            path = $File.FullName
            fileVersion = $File.VersionInfo.FileVersion
            assemblyName = $name.Name
            assemblyVersion = $name.Version.ToString()
            fullIdentity = $name.FullName
            mvid = $assembly.ManifestModule.ModuleVersionId.ToString('D')
            sha256 = Get-Sha256 $File.FullName
            inspectionError = $null
        }
    } catch {
        return [ordered]@{
            path = $File.FullName
            fileVersion = $File.VersionInfo.FileVersion
            assemblyName = $null
            assemblyVersion = $null
            fullIdentity = $null
            mvid = $null
            sha256 = Get-Sha256 $File.FullName
            inspectionError = $_.Exception.Message
        }
    }
}

$approved = Resolve-ExistingDirectory $ApprovedRoot 'Approved root'
$reference = Resolve-ExistingDirectory $ReferenceRoot 'Reference root'
if (-not (Test-DescendantOrSame $reference $approved)) {
    throw "Reference root escapes the approved root: $reference"
}

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
if (-not $OutputRoot) {
    $stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ')
    $OutputRoot = Join-Path $repositoryRoot "artifacts\compatibility\reference-inventory\$stamp"
}
$outputParent = Split-Path -Parent $OutputRoot
if (-not (Test-Path -LiteralPath $outputParent -PathType Container)) {
    New-Item -ItemType Directory -Path $outputParent -Force | Out-Null
}
$output = [IO.Path]::GetFullPath($OutputRoot)
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts\compatibility'))
if (-not (Test-DescendantOrSame $output $artifactRoot)) {
    throw "Output root must remain beneath repository artifacts/compatibility: $output"
}
New-Item -ItemType Directory -Path $output -Force | Out-Null

$records = @()
foreach ($directory in Get-ChildItem -LiteralPath $reference -Directory -Force | Sort-Object Name) {
    $files = @(Get-ChildItem -LiteralPath $directory.FullName -File -Recurse -Force |
        Where-Object { $_.FullName -notmatch '[\\/]\.git[\\/]' } | Sort-Object FullName)
    $infos = @($files | Where-Object { $_.Name.Equals('Info.json', [StringComparison]::OrdinalIgnoreCase) })
    $rootInfo = @($infos | Where-Object { $_.DirectoryName.Equals($directory.FullName, [StringComparison]::OrdinalIgnoreCase) })
    $infoFile = if ($rootInfo.Count -eq 1) { $rootInfo[0] } elseif ($infos.Count -eq 1) { $infos[0] } else { $null }
    $info = Read-Info $infoFile
    $dlls = @($files | Where-Object Extension -eq '.dll')
    $sources = @($files | Where-Object Extension -eq '.cs')
    $projects = @($files | Where-Object Extension -in '.csproj', '.sln')
    $declaredAssembly = if ($info -and $info.PSObject.Properties['AssemblyName']) { [string]$info.AssemblyName } else { $null }
    $declaredDll = @(if ($declaredAssembly) { $dlls | Where-Object Name -eq $declaredAssembly } else { $dlls })
    $entry = if ($info -and $info.PSObject.Properties['EntryMethod']) { [string]$info.EntryMethod } else { $null }
    $valid = $null -ne $infoFile -and -not $info.PSObject.Properties['parseError'] -and
        -not [string]::IsNullOrWhiteSpace([string]$info.Id) -and
        -not [string]::IsNullOrWhiteSpace($entry) -and $declaredDll.Count -eq 1
    $classification = if ($valid) { 'LOADABLE-UMM-ROOT' }
        elseif ($sources.Count -gt 0 -or $projects.Count -gt 0) { 'SOURCE-REFERENCE-ONLY' }
        elseif ($dlls.Count -gt 0 -or $infos.Count -gt 0) { 'INVALID-LOADABLE-REFERENCE' }
        else { 'ASSET-REFERENCE-ONLY' }
    $manifest = @($files | ForEach-Object {
        [ordered]@{ path = Get-RelativePath $directory.FullName $_.FullName; length = $_.Length; sha256 = Get-Sha256 $_.FullName }
    })
    $records += [ordered]@{
        folderName = $directory.Name
        canonicalPath = $directory.FullName
        classification = $classification
        totalFileCount = $files.Count
        totalByteCount = [long](($files | Measure-Object Length -Sum).Sum)
        infoJsonPaths = @($infos | ForEach-Object FullName)
        info = if ($info) { [ordered]@{
            id = $info.Id; displayName = $info.DisplayName; version = $info.Version
            managerVersion = $info.ManagerVersion; assemblyName = $declaredAssembly
            entryMethod = $entry
            requirements = if ($info.PSObject.Properties['Requirements']) { @($info.Requirements) } else { @() }
        }} else { $null }
        candidateAssemblies = @($dlls | ForEach-Object { Read-Assembly $_ })
        sourceFileCount = $sources.Count
        projectFiles = @($projects | ForEach-Object FullName)
        likelyBuildOutputs = @($files | Where-Object { $_.FullName -match '[\\/](bin|release|releases)[\\/]' } | ForEach-Object FullName)
        provenanceFiles = @($files | Where-Object { $_.Name -match '^(LICENSE|COPYING|README|Repository)(\..*)?$' } | ForEach-Object FullName)
        validLoadableUmmRoot = [bool]$valid
        sourceReferenceOnly = $classification -eq 'SOURCE-REFERENCE-ONLY'
        assetReferenceOnly = $classification -eq 'ASSET-REFERENCE-ONLY'
        exclusions = @('.git directory metadata and objects')
        relevantFileManifest = $manifest
    }
}

$result = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    referenceRoot = $reference
    approvedRoot = $approved
    records = $records
}
$jsonPath = Join-Path $output 'reference-inventory.json'
$result | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $jsonPath -Encoding UTF8
$summaryPath = Join-Path $output 'reference-inventory-summary.txt'
@($records | ForEach-Object {
    $recordId = if ($null -ne $_.info) { $_.info.id } else { '' }
    $recordVersion = if ($null -ne $_.info) { $_.info.version } else { '' }
    "$($_.folderName)`t$($_.classification)`tfiles=$($_.totalFileCount)`tbytes=$($_.totalByteCount)`tid=$recordId`tversion=$recordVersion"
}) |
    Set-Content -LiteralPath $summaryPath -Encoding UTF8
Write-Host "Reference inventory: $jsonPath"
Write-Host "Immediate children: $($records.Count)"
Write-Output $jsonPath
