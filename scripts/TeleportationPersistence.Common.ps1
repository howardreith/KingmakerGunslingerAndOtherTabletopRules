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

function Get-PersistenceModsInventory {
    $root = (Resolve-Path -LiteralPath $modsRoot).Path
    $entries = @(Get-ChildItem -LiteralPath $root -Recurse -Force | Sort-Object FullName)
    if (@($entries | Where-Object { ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 }).Count -ne 0) { throw 'Mods inventory crosses a reparse point.' }
    return @($entries | ForEach-Object {
        [ordered]@{ path = $_.FullName.Substring($root.Length + 1); directory = $_.PSIsContainer
            length = if ($_.PSIsContainer) { 0 } else { $_.Length }
            sha256 = if ($_.PSIsContainer) { $null } else { (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() } }
    })
}
function Wait-PersistenceExit {
    $deadline = [DateTime]::UtcNow.AddSeconds(50)
    while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 250 }
    if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) { throw 'Guarded game process did not terminate; no cleanup or settings restoration may run while it is alive.' }
}
function Restore-PersistenceSettings {
    [IO.File]::WriteAllBytes($settingsPath, $settingsBytes)
    $item = Get-Item -LiteralPath $settingsPath
    $item.CreationTimeUtc = $settingsTimes[0]; $item.LastWriteTimeUtc = $settingsTimes[1]; $item.Attributes = $settingsTimes[2]
    if ([Convert]::ToBase64String([IO.File]::ReadAllBytes($settingsPath)) -cne [Convert]::ToBase64String($settingsBytes)) { throw 'Settings byte restoration failed.' }
}
function Restore-PersistenceSidecars {
    param([object[]]$AuthorizedSettingsStates = @())
    $removed = New-Object 'System.Collections.Generic.List[object]'
    if (Test-Path -LiteralPath $previousPath -PathType Leaf) {
        $currentBytes = [IO.File]::ReadAllBytes($previousPath)
        $same = $null -ne $previousBytes -and [Convert]::ToBase64String($currentBytes) -ceq [Convert]::ToBase64String($previousBytes)
        if (-not $same) {
            $candidate = [Text.Encoding]::UTF8.GetString($currentBytes) | ConvertFrom-Json
            # A boundary-matrix caller may supply only the exact settings states
            # it wrote. Persistence/coexistence callers retain the single-module
            # default and cannot authorize an unrelated settings change.
            $matrixMatch = $false
            foreach ($known in $AuthorizedSettingsStates) {
                if (@($candidate.PSObject.Properties).Count -ne @($known.PSObject.Properties).Count) { continue }
                $equal = $true
                foreach ($property in $known.PSObject.Properties) {
                    if ($null -eq $candidate.PSObject.Properties[$property.Name] -or
                        ($candidate.PSObject.Properties[$property.Name].Value | ConvertTo-Json -Depth 20 -Compress) -cne
                        ($property.Value | ConvertTo-Json -Depth 20 -Compress)) { $equal = $false; break }
                }
                if ($equal) { $matrixMatch = $true; break }
            }
            if (-not $matrixMatch) {
                if (@($candidate.PSObject.Properties).Count -ne @($settingsOriginal.PSObject.Properties).Count -or
                    $candidate.'teleportation-spells' -isnot [bool]) { throw 'Unproven settings sidecar content.' }
                $candidate.'teleportation-spells' = $settingsOriginal.'teleportation-spells'
                foreach ($property in $settingsOriginal.PSObject.Properties) {
                    if ($null -eq $candidate.PSObject.Properties[$property.Name] -or
                        ($candidate.PSObject.Properties[$property.Name].Value | ConvertTo-Json -Depth 20 -Compress) -cne
                        ($property.Value | ConvertTo-Json -Depth 20 -Compress)) { throw 'Settings sidecar differs outside the authorized single module transaction.' }
                }
            }
        }
        if ($null -eq $previousBytes) {
            $removed.Add([ordered]@{ path = $previousPath; kind = 'transaction-settings-backup'; sha256 = (Get-FileHash -LiteralPath $previousPath -Algorithm SHA256).Hash.ToLowerInvariant() })
            Remove-Item -LiteralPath $previousPath
        }
    }
    if ($null -ne $previousBytes) {
        [IO.File]::WriteAllBytes($previousPath, $previousBytes)
        $item = Get-Item -LiteralPath $previousPath
        $item.CreationTimeUtc = $previousTimes[0]; $item.LastWriteTimeUtc = $previousTimes[1]; $item.Attributes = $previousTimes[2]
    }
    $kmgDirectory = [IO.Path]::GetFullPath((Join-Path $modsRoot 'KingmakerGunslinger'))
    foreach ($file in @(Get-ChildItem -LiteralPath $kmgDirectory -File -Filter '*.cache')) {
        $relative = 'KingmakerGunslinger\' + $file.Name
        if (@($modsBefore | Where-Object path -CEQ $relative).Count -ne 0) { continue }
        $path = [IO.Path]::GetFullPath($file.FullName)
        $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ([IO.Path]::GetDirectoryName($path) -cne $kmgDirectory -or
            $file.Name -cnotmatch '^KingmakerGunslinger\.dll\.[0-9]{1,5}\.cache$' -or $hash -cne $dllSha) {
            throw 'A new loader cache is not the exact transaction DLL; cleanup refused.'
        }
        $removed.Add([ordered]@{ path = $path; kind = 'UMM-timestamp-version-cache'; sha256 = $hash })
        Remove-Item -LiteralPath $path
    }
    Write-PersistenceEvidence 'owned-mod-sidecar-cleanup.json' @($removed.ToArray())
}

# Maps only the existing module catalog; preserves explicit OFF and the native
# default-ON semantics of absent keys without rewriting the captured document.
function Get-KmgOriginalModuleRuntimeParameters {
    param([Parameter(Mandatory = $true)]$Settings)
    $parameters = @{}
    foreach ($module in @(Get-KmgFeatureModuleCatalog)) {
        $property = $Settings.PSObject.Properties[$module.JsonKey]
        if ($null -ne $property -and $property.Value -isnot [bool]) { throw 'Original module setting is not a boolean.' }
        $parameters[$module.RuntimeParameter] = if ($null -eq $property) { $true } else { $property.Value }
    }
    return $parameters
}
