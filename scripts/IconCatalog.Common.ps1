Set-StrictMode -Version Latest

# Build/package metadata only. The game uses explicit compiled owned mappings.
function Get-KmgIntegratedIconRecords {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot)
    $catalog = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'assets-source/original-icons/icon-catalog.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    $manifests = @{}
    $seen = @{}
    foreach ($concept in $catalog.concepts) {
        if (-not $concept.PSObject.Properties['runtimeExport'] -or -not $concept.runtimeExport) { continue }
        $key = [string]$concept.key
        if ($key -cnotmatch '^[a-z][a-z0-9-]+$' -or $seen.ContainsKey($key)) { throw "Invalid/duplicate integrated icon key: $key" }
        $seen[$key] = $true
        if ($concept.runtimeExport.path -cne "assets/game/icons/$key.png" -or
            $concept.runtimeExport.installedPath -cne "assets/icons/$key.png" -or
            $concept.runtimeExport.cacheKey -cne $key) { throw "Integrated icon destination mismatch: $key" }
        $authority = [string]$concept.assetAuthority.manifest
        if ($authority -cnotin @(
            'assets-source/original-icons/icon-overhaul-v2/pilot/pilot-manifest.json',
            'assets-source/original-icons/icon-overhaul-v2/production/production-manifest.json')) {
            throw "Unsupported integrated icon authority: $key"
        }
        if (-not $manifests.ContainsKey($authority)) {
            $manifests[$authority] = Get-Content -LiteralPath (Join-Path $RepositoryRoot $authority) -Raw -Encoding UTF8 | ConvertFrom-Json
        }
        $records = @($manifests[$authority].records | Where-Object { $_.key -ceq $key })
        if ($records.Count -ne 1 -or $records[0].exportSha256 -cnotmatch '^[0-9a-f]{64}$') {
            throw "Missing exact integrated icon export record: $key"
        }
        [pscustomobject]@{
            Key = $key
            SourceRelativePath = [string]$concept.runtimeExport.path
            InstalledRelativePath = [string]$concept.runtimeExport.installedPath
            Sha256 = [string]$records[0].exportSha256
        }
    }
}

function Assert-KmgIntegratedIconFiles {
    param(
        [Parameter(Mandatory = $true)][string]$ModDirectory,
        [Parameter(Mandatory = $true)][object[]]$Records
    )
    foreach ($record in $Records) {
        $path = Join-Path $ModDirectory $record.InstalledRelativePath
        if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or
            (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $record.Sha256) {
            throw "Integrated icon is missing or has stale pixels: $($record.Key)"
        }
    }
}
