# Shared only by guarded teleportation qualification. No save is written here.
Set-StrictMode -Version Latest

function Open-KmgProtectedSaveCatalog {
    param([Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [string]$SaveDirectory = (Join-Path (Split-Path -Parent ([Environment]::GetFolderPath('LocalApplicationData'))) 'LocalLow\Owlcat Games\Pathfinder Kingmaker\Saved Games'))
    $root = (Resolve-Path -LiteralPath $SaveDirectory).Path
    if (((Get-Item -LiteralPath $root -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw 'The save catalog root cannot be a reparse point.'
    }
    $entries = @(Get-ChildItem -LiteralPath $root -Force -Recurse)
    if (@($entries | Where-Object { ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 }).Count -ne 0) {
        throw 'A save catalog containing a reparse point is ambiguous.'
    }
    $leases = New-Object 'System.Collections.Generic.List[System.IO.FileStream]'
    $records = New-Object 'System.Collections.Generic.List[object]'
    try {
        foreach ($file in @($entries | Where-Object { -not $_.PSIsContainer } | Sort-Object FullName)) {
            $stream = [IO.File]::Open($file.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
            $leases.Add($stream)
            $sha = [Security.Cryptography.SHA256]::Create()
            try { $hash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '').ToLowerInvariant() }
            finally { $sha.Dispose(); $stream.Position = 0 }
            $file.Refresh()
            $records.Add([ordered]@{ path = $file.FullName; length = $file.Length; sha256 = $hash
                creationUtcTicks = $file.CreationTimeUtc.Ticks; writeUtcTicks = $file.LastWriteTimeUtc.Ticks
                attributes = [int]$file.Attributes })
        }
        $snapshot = [ordered]@{ schemaVersion = 1; capturedUtc = [DateTime]::UtcNow.ToString('o')
            directory = $root; files = @($records.ToArray()); writeDeleteRenameDenied = $true }
        if (-not (Test-Path -LiteralPath $EvidenceDirectory)) { [void](New-Item -ItemType Directory -Path $EvidenceDirectory) }
        $path = Join-Path $EvidenceDirectory 'preexisting-saves.json'
        if (Test-Path -LiteralPath $path) { throw 'Protected catalog evidence cannot be overwritten.' }
        [IO.File]::WriteAllText($path, ($snapshot | ConvertTo-Json -Depth 10), (New-Object Text.UTF8Encoding($false)))
        return [pscustomobject]@{ Directory = $root; Files = $records.ToArray(); Leases = $leases; EvidenceDirectory = $EvidenceDirectory }
    }
    catch { foreach ($lease in $leases) { $lease.Dispose() }; throw }
}

function Assert-KmgProtectedSaveCatalog {
    param([Parameter(Mandatory = $true)]$Catalog, [string[]]$OwnedPaths = @())
    $failures = New-Object 'System.Collections.Generic.List[string]'
    foreach ($before in $Catalog.Files) {
        if (-not (Test-Path -LiteralPath $before.path -PathType Leaf)) { $failures.Add('missing:' + $before.path); continue }
        $now = Get-Item -LiteralPath $before.path -Force
        if ($now.Length -ne $before.length -or $now.LastWriteTimeUtc.Ticks -ne $before.writeUtcTicks -or
            $now.CreationTimeUtc.Ticks -ne $before.creationUtcTicks -or [int]$now.Attributes -ne $before.attributes -or
            (Get-FileHash -LiteralPath $before.path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $before.sha256) {
            $failures.Add('changed:' + $before.path)
        }
    }
    $known = @($Catalog.Files | ForEach-Object { $_.path }) + @($OwnedPaths)
    foreach ($now in @(Get-ChildItem -LiteralPath $Catalog.Directory -File -Force -Recurse)) {
        if ($known -cnotcontains $now.FullName) { $failures.Add('unowned-new-save:' + $now.FullName) }
    }
    $result = [ordered]@{ schemaVersion = 1; checkedUtc = [DateTime]::UtcNow.ToString('o')
        preexistingCount = $Catalog.Files.Count; ownedPaths = @($OwnedPaths)
        failures = @($failures.ToArray()); passed = ($failures.Count -eq 0) }
    [IO.File]::WriteAllText((Join-Path $Catalog.EvidenceDirectory 'save-preservation.json'),
        ($result | ConvertTo-Json -Depth 10), (New-Object Text.UTF8Encoding($false)))
    if ($failures.Count -ne 0) { throw ('Save preservation failed: ' + ($failures -join '; ')) }
    return $result
}

function Close-KmgProtectedSaveCatalog {
    param([Parameter(Mandatory = $true)]$Catalog)
    foreach ($lease in $Catalog.Leases) { $lease.Dispose() }
}
