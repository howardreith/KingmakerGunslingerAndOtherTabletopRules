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

# Summon counts come from the hash-pinned authoring authority, rather than a
# second roster total in each build/package consumer.
function Get-KmgSummonIconManifest {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot)
    $relative = 'assets-source/original-icons/expanded-summoning/icon-manifest.json'
    $catalog = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'assets-source/original-icons/icon-catalog.json') -Raw | ConvertFrom-Json
    $pins = @($catalog.delegatedManifests | Where-Object { $_.path -ceq $relative })
    $sourcePath = Join-Path $RepositoryRoot $relative
    if ($pins.Count -ne 1 -or (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant() -cne $pins[0].sha256) {
        throw 'Expanded Summoning icon authoring authority is missing or stale.'
    }
    $source = Get-Content -LiteralPath $sourcePath -Raw | ConvertFrom-Json
    $runtime = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'assets/game/icons/expanded-summoning/icon-manifest.json') -Raw | ConvertFrom-Json
    if ($source.schemaVersion -ne 1 -or $runtime.schemaVersion -ne 1 -or $source.count -le 0 -or
        @($source.icons).Count -ne $source.count -or $runtime.count -ne $source.count -or
        @($runtime.icons).Count -ne $source.count -or @($runtime.icons.key | Sort-Object -Unique).Count -ne $source.count) {
        throw 'Expanded Summoning runtime icon manifest is malformed.'
    }
    foreach ($icon in $runtime.icons) {
        $match = @($source.icons | Where-Object { $_.key -ceq $icon.key })
        if ($match.Count -ne 1 -or $icon.file -cne ($icon.key + '.png') -or
            $match[0].productionFile -cne ('assets/game/icons/expanded-summoning/' + $icon.file) -or
            $icon.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $icon.sha256 -cne $match[0].outputSha256 -or
            $icon.width -ne 128 -or $icon.height -ne 128) {
            throw "Expanded Summoning runtime icon authority mismatch: $($icon.key)."
        }
    }
    return $runtime
}
