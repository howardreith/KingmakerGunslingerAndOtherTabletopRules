[CmdletBinding()]
param()

# Offline behavior checks for the pinned Unity Mod Manager build floor:
# declared minimum, pin verification, receipt identity and package refusal.
# Fixture files only; no download, build, deployment or game launch.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ReferenceProvenance.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
$checks = 0
function Assert-That([bool]$Condition, [string]$Name) {
    $script:checks++
    if (-not $Condition) { $failures.Add($Name) }
}
function Get-Refusal([scriptblock]$Action) {
    try { & $Action | Out-Null; return $null } catch { return $_.Exception.Message }
}
function Get-Sha([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }

$root = Split-Path $PSScriptRoot -Parent

# The shipped contract: one declared floor, equal to the pinned build floor.
$realPins = Get-KmgLoaderReferencePins
$realInfo = Get-Content -LiteralPath (Join-Path $root 'Info.json') -Raw | ConvertFrom-Json
Assert-That ($realPins.MinimumManagerVersion -ceq '0.31.1') 'pinned-floor-is-0.31.1'
Assert-That ([string]$realInfo.ManagerVersion -ceq $realPins.MinimumManagerVersion) 'info-json-declares-the-pinned-floor'
$umm = @($realPins.References | Where-Object relativePath -ceq 'UnityModManager/UnityModManager.dll')
Assert-That ($umm.Count -eq 1 -and $umm[0].assemblyVersion -ceq '0.31.1.0' -and
    $umm[0].sha256 -ceq '2e038ff23ab01d538dfef792096e1f5c04802d1edac3660db20d0d54c167c2c4') 'umm-pin-is-the-verified-0.31.1-nuget-build'

$fixture = Join-Path ([IO.Path]::GetTempPath()) ('KmgLoaderPins-' + [Guid]::NewGuid().ToString('N'))
try {
    $loader = Join-Path $fixture 'loader'
    $bundle = Join-Path $fixture 'bundle'
    $install = Join-Path $fixture 'install'
    $rows = [Collections.Generic.List[object]]::new()
    foreach ($relative in $script:KmgPinnedLoaderReferencePaths) {
        $path = Join-Path $loader $relative
        New-Item -ItemType Directory -Path (Split-Path $path) -Force | Out-Null
        [IO.File]::WriteAllText($path, "pinned 0.31.1 $relative")
        $rows.Add([ordered]@{ relativePath = $relative.Replace('\', '/'); assemblyVersion = $(if ($relative -like '*0Harmony12.dll') { '1.2.0.1' } else { '0.31.1.0' })
            url = 'https://fixture.invalid/never-fetched'; archiveSha256 = $null; archiveEntry = $null
            sha256 = Get-Sha $path })
    }
    $pinsPath = Join-Path $fixture 'loader-reference-pins.json'
    ([ordered]@{ schemaVersion = 1; minimumManagerVersion = '0.31.1'; references = @($rows) } |
        ConvertTo-Json -Depth 4) | Set-Content -LiteralPath $pinsPath -Encoding UTF8
    foreach ($relative in $script:KmgPrivateReferencePaths) {
        foreach ($path in @((Join-Path $bundle "Managed\$relative"), (Join-Path $install "Kingmaker_Data\Managed\$relative"))) {
            New-Item -ItemType Directory -Path (Split-Path $path) -Force | Out-Null
            # The bundle and install carry a newer local loader (0.33.0).
            [IO.File]::WriteAllText($path, "installed 0.33.0 $relative")
        }
    }

    # Present and matching: no download (the fixture URL cannot resolve).
    $identity = @(Initialize-KmgPinnedLoaderReferences -LoaderReferenceDir $loader -PinsPath $pinsPath)
    Assert-That ($identity.Count -eq 2 -and @($identity | Where-Object role -ceq 'pinned-loader').Count -eq 2) 'present-pins-verified-without-download'

    $matched = @(Assert-KmgReferenceBundleMatchesInstall -ReferenceBundleDir $bundle `
        -KingmakerInstallDir $install -LoaderReferenceDir $loader -PinsPath $pinsPath)
    $expectedPaths = @($script:KmgPrivateReferencePaths | ForEach-Object { $_.Replace('\', '/') })
    Assert-That ((@($matched | ForEach-Object { $_.relativePath }) -join '|') -ceq ($expectedPaths -join '|')) 'receipt-rows-keep-the-reference-order'
    $ummRow = @($matched | Where-Object { $_.relativePath -ceq 'UnityModManager/UnityModManager.dll' })[0]
    $installedUmm = Join-Path $install 'Kingmaker_Data\Managed\UnityModManager\UnityModManager.dll'
    Assert-That ($ummRow.sha256 -ceq (Get-Sha (Join-Path $loader 'UnityModManager\UnityModManager.dll')) -and
        $ummRow.sha256 -cne (Get-Sha $installedUmm) -and $ummRow.role -ceq 'pinned-loader' -and
        $ummRow.assemblyVersion -ceq '0.31.1.0' -and $ummRow.pinsSha256 -ceq (Get-Sha $pinsPath)) 'receipt-names-the-pinned-loader-not-the-installed-one'
    Assert-That (@($matched | Where-Object { $_.relativePath -ceq 'Assembly-CSharp.dll' -and $_.role -ceq 'installed-game' }).Count -eq 1) 'game-references-keep-their-install-identity'

    # A newer local loader cannot change the baseline.
    [IO.File]::WriteAllText($installedUmm, 'installed 0.34.0 UnityModManager')
    [IO.File]::WriteAllText((Join-Path $bundle 'Managed\UnityModManager\UnityModManager.dll'), 'bundled 0.35.0 UnityModManager')
    $again = @(Assert-KmgReferenceBundleMatchesInstall -ReferenceBundleDir $bundle `
        -KingmakerInstallDir $install -LoaderReferenceDir $loader -PinsPath $pinsPath)
    Assert-That (((@($again | ForEach-Object { $_.relativePath + '=' + $_.sha256 })) -join '|') -ceq
        ((@($matched | ForEach-Object { $_.relativePath + '=' + $_.sha256 })) -join '|')) 'newer-local-loader-does-not-change-the-build-baseline'

    # The game-reference protection is unchanged.
    $cloth = Join-Path $bundle 'Managed\UnityEngine.ClothModule.dll'
    [IO.File]::AppendAllText($cloth, 'mismatch')
    Assert-That ((Get-Refusal { Assert-KmgReferenceBundleMatchesInstall -ReferenceBundleDir $bundle `
        -KingmakerInstallDir $install -LoaderReferenceDir $loader -PinsPath $pinsPath }) -like '*differs from the installed Steam runtime*') 'game-reference-mismatch-still-refused'
    [IO.File]::WriteAllText($cloth, 'installed 0.33.0 UnityEngine.ClothModule.dll')

    # Wrong, tampered or missing loader references are refused, not replaced.
    $pinnedUmm = Join-Path $loader 'UnityModManager\UnityModManager.dll'
    $pinnedBytes = [IO.File]::ReadAllBytes($pinnedUmm)
    [IO.File]::WriteAllText($pinnedUmm, 'installed 0.33.0 UnityModManager')
    Assert-That ((Get-Refusal { Assert-KmgReferenceBundleMatchesInstall -ReferenceBundleDir $bundle `
        -KingmakerInstallDir $install -LoaderReferenceDir $loader -PinsPath $pinsPath }) -like '*differs from its pin*') 'wrong-loader-reference-refused'
    Assert-That ((Get-Refusal { Initialize-KmgPinnedLoaderReferences -LoaderReferenceDir $loader -PinsPath $pinsPath }) -like '*differs from its pin*') 'tampered-cache-not-silently-replaced'
    Assert-That ([IO.File]::ReadAllText($pinnedUmm) -ceq 'installed 0.33.0 UnityModManager') 'tampered-file-left-for-inspection'
    [IO.File]::WriteAllBytes($pinnedUmm, $pinnedBytes)
    $pinnedHarmony = Join-Path $loader 'UnityModManager\0Harmony12.dll'
    $harmonyBytes = [IO.File]::ReadAllBytes($pinnedHarmony)
    Remove-Item -LiteralPath $pinnedHarmony
    Assert-That ((Get-Refusal { Assert-KmgPinnedLoaderReferences -LoaderReferenceDir $loader -PinsPath $pinsPath }) -like '*Pinned loader reference is missing*') 'missing-loader-reference-refused'
    Assert-That ((Get-Refusal { Initialize-KmgPinnedLoaderReferences -LoaderReferenceDir $loader -PinsPath $pinsPath }) -ne $null -and
        -not (Test-Path -LiteralPath $pinnedHarmony)) 'unavailable-pinned-source-fails-without-substitute'
    [IO.File]::WriteAllBytes($pinnedHarmony, $harmonyBytes)

    $badPins = Join-Path $fixture 'bad-pins.json'
    ([ordered]@{ schemaVersion = 1; minimumManagerVersion = '0.31.1'; references = @($rows[0]) } |
        ConvertTo-Json -Depth 4) | Set-Content -LiteralPath $badPins -Encoding UTF8
    Assert-That ((Get-Refusal { Get-KmgLoaderReferencePins -PinsPath $badPins }) -like '*must name exactly*') 'incomplete-pins-refused'

    # Receipts: the JSON round trip of the build's rows is accepted; a receipt
    # naming the installed loader, or lacking a loader row, is refused.
    function ConvertTo-ReceiptRows { $parsed = ConvertFrom-Json (ConvertTo-Json -InputObject @($matched) -Depth 4); return ,@($parsed) }
    $receiptRows = ConvertTo-ReceiptRows
    Assert-That ($null -eq (Get-Refusal { Assert-KmgReceiptLoaderReferencesMatchPins -ReferenceFiles $receiptRows -PinsPath $pinsPath })) 'receipt-with-pinned-rows-accepted'
    $installedReceipt = ConvertTo-ReceiptRows
    ($installedReceipt | Where-Object relativePath -ceq 'UnityModManager/UnityModManager.dll').sha256 = Get-Sha $installedUmm
    Assert-That ((Get-Refusal { Assert-KmgReceiptLoaderReferencesMatchPins -ReferenceFiles $installedReceipt -PinsPath $pinsPath }) -like '*does not match the current pin*') 'receipt-naming-installed-loader-refused'
    $legacyReceipt = @($receiptRows | Where-Object relativePath -cne 'UnityModManager/0Harmony12.dll')
    Assert-That ((Get-Refusal { Assert-KmgReceiptLoaderReferencesMatchPins -ReferenceFiles $legacyReceipt -PinsPath $pinsPath }) -like '*exactly one pinned loader row*') 'receipt-without-loader-row-refused'
    $legacyShape = @($receiptRows | ForEach-Object { [pscustomobject]@{ relativePath = $_.relativePath; sha256 = $_.sha256 } })
    Assert-That ((Get-Refusal { Assert-KmgReceiptLoaderReferencesMatchPins -ReferenceFiles $legacyShape -PinsPath $pinsPath }) -like '*lacks*') 'pre-pin-receipt-shape-refused'
    [IO.File]::AppendAllText($pinsPath, ' ')
    Assert-That ((Get-Refusal { Assert-KmgReceiptLoaderReferencesMatchPins -ReferenceFiles $receiptRows -PinsPath $pinsPath }) -like '*does not match the current pin*') 'changed-pins-invalidate-existing-receipts'

    # Package: declared floor equals the pin; no copied runtime dependency.
    $mod = Join-Path $fixture 'package\KingmakerGunslinger'
    New-Item -ItemType Directory -Path (Join-Path $mod 'assets') -Force | Out-Null
    # Real fixture assemblies (the .NET Framework compiler): stub loaders of known identity and product DLLs compiled
    # against them, so the package check reads actual AssemblyRefs.
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    $stubs = Join-Path $fixture 'stubs'; New-Item -ItemType Directory -Path $stubs | Out-Null
    function New-FixtureAssembly([string]$Out, [string]$Code, [string[]]$References = @()) {
        $source = $Out + '.cs'; [IO.File]::WriteAllText($source, $Code)
        $o = & $csc /nologo /target:library "/out:$Out" @($References | ForEach-Object { "/reference:$_" }) $source 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Fixture compile failed: $($o -join ' ')" }
    }
    New-Item -ItemType Directory -Path (Join-Path $stubs 'pinned'), (Join-Path $stubs 'newer') | Out-Null
    New-FixtureAssembly (Join-Path $stubs 'pinned\UnityModManager.dll') '[assembly: System.Reflection.AssemblyVersion("0.31.1.0")] namespace UnityModManagerNet { public class UnityModManager { } }'
    New-FixtureAssembly (Join-Path $stubs 'pinned\0Harmony12.dll') '[assembly: System.Reflection.AssemblyVersion("1.2.0.1")] namespace Harmony12 { public class HarmonyInstance { } }'
    New-FixtureAssembly (Join-Path $stubs 'newer\UnityModManager.dll') '[assembly: System.Reflection.AssemblyVersion("0.33.0.0")] namespace UnityModManagerNet { public class UnityModManager { } }'
    $probe = 'public static class Probe { public static System.Type[] Types = { typeof(UnityModManagerNet.UnityModManager), typeof(Harmony12.HarmonyInstance) }; }'
    New-Item -ItemType Directory -Path (Join-Path $stubs 'product-pinned'), (Join-Path $stubs 'product-newer') | Out-Null
    New-FixtureAssembly (Join-Path $stubs 'product-pinned\KingmakerGunslinger.dll') $probe @((Join-Path $stubs 'pinned\UnityModManager.dll'), (Join-Path $stubs 'pinned\0Harmony12.dll'))
    New-FixtureAssembly (Join-Path $stubs 'product-newer\KingmakerGunslinger.dll') $probe @((Join-Path $stubs 'newer\UnityModManager.dll'), (Join-Path $stubs 'pinned\0Harmony12.dll'))
    Copy-Item -LiteralPath (Join-Path $stubs 'product-pinned\KingmakerGunslinger.dll') -Destination (Join-Path $mod 'KingmakerGunslinger.dll')
    $infoPath = Join-Path $mod 'Info.json'
    function Set-FixtureManager([string]$Version) {
        ([ordered]@{ Id = 'KingmakerGunslinger'; Version = '0.0.187'; ManagerVersion = $Version
            AssemblyName = 'KingmakerGunslinger.dll'; EntryMethod = 'KingmakerGunslinger.Main.Load' } |
            ConvertTo-Json) | Set-Content -LiteralPath $infoPath -Encoding UTF8
    }
    Set-FixtureManager '0.31.1'
    Assert-That ($null -eq (Get-Refusal { Assert-KmgPackagedLoaderContract -ModDirectory $mod -PinsPath $pinsPath })) 'package-at-pinned-floor-accepted'
    foreach ($version in @('0.32.4', '0.33.0', '0.27.10', '')) {
        Set-FixtureManager $version
        Assert-That ((Get-Refusal { Assert-KmgPackagedLoaderContract -ModDirectory $mod -PinsPath $pinsPath }) -like '*pinned loader floor*') "package-declaring-$version-refused"
    }
    Set-FixtureManager '0.31.1'
    foreach ($dependency in @('UnityModManager.dll', '0Harmony12.dll', 'assets\0Harmony.dll', '0Harmony-1.2.dll', 'dnlib.dll', 'Assembly-CSharp.dll')) {
        $copied = Join-Path $mod $dependency
        [IO.File]::WriteAllText($copied, 'copied runtime dependency')
        Assert-That ((Get-Refusal { Assert-KmgPackagedLoaderContract -ModDirectory $mod -PinsPath $pinsPath }) -like '*exactly one binary*') "package-with-copied-$([IO.Path]::GetFileName($dependency))-refused"
        Remove-Item -LiteralPath $copied
    }
    Assert-That ($null -eq (Get-Refusal { Assert-KmgPackagedLoaderContract -ModDirectory $mod -PinsPath $pinsPath })) 'package-accepted-again-after-removal'
    # The packaged DLL's actual references: compiled against a newer loader, or unreadable, it is refused.
    Copy-Item -LiteralPath (Join-Path $stubs 'product-newer\KingmakerGunslinger.dll') -Destination (Join-Path $mod 'KingmakerGunslinger.dll') -Force
    Assert-That ((Get-Refusal { Assert-KmgPackagedLoaderContract -ModDirectory $mod -PinsPath $pinsPath }) -like '*UnityModManager, Version=0.33.0.0*not exactly the pinned loader assemblies*') 'package-dll-compiled-against-newer-loader-refused'
    [IO.File]::WriteAllText((Join-Path $mod 'KingmakerGunslinger.dll'), 'not an assembly')
    Assert-That ((Get-Refusal { Assert-KmgPackagedLoaderContract -ModDirectory $mod -PinsPath $pinsPath }) -like '*could not be read*') 'package-dll-unreadable-refused'
    Copy-Item -LiteralPath (Join-Path $stubs 'product-pinned\KingmakerGunslinger.dll') -Destination (Join-Path $mod 'KingmakerGunslinger.dll') -Force
    Assert-That ($null -eq (Get-Refusal { Assert-KmgPackagedLoaderContract -ModDirectory $mod -PinsPath $pinsPath })) 'package-dll-with-pinned-references-accepted'
}
finally {
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}

if ($failures.Count -ne 0) { throw "Loader reference pin checks failed: $($failures -join ', ')" }
Write-Host "Loader reference pin checks: PASS=$checks FAIL=0. Offline fixtures only."
