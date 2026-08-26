Set-StrictMode -Version Latest

$script:KmgPrivateReferencePaths = @(
    'Assembly-CSharp.dll',
    'Assembly-CSharp-firstpass.dll',
    'Newtonsoft.Json.dll',
    'UnityEngine.dll',
    'UnityEngine.AnimationModule.dll',
    'UnityEngine.AudioModule.dll',
    'UnityEngine.AssetBundleModule.dll',
    'UnityEngine.CoreModule.dll',
    'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll',
    'UnityEngine.TextRenderingModule.dll',
    'UnityModManager\UnityModManager.dll',
    'UnityModManager\0Harmony12.dll'
)

function Assert-KmgReferenceBundleMatchesInstall {
    param(
        [Parameter(Mandatory = $true)][string]$ReferenceBundleDir,
        [Parameter(Mandatory = $true)][string]$KingmakerInstallDir
    )

    $bundleManaged = Join-Path $ReferenceBundleDir 'Managed'
    $installedManaged = Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed'
    $result = [Collections.Generic.List[object]]::new()
    foreach ($relativePath in $script:KmgPrivateReferencePaths) {
        $bundlePath = Join-Path $bundleManaged $relativePath
        $installedPath = Join-Path $installedManaged $relativePath
        if (-not (Test-Path -LiteralPath $bundlePath -PathType Leaf)) {
            throw "Private reference is missing: $bundlePath"
        }
        if (-not (Test-Path -LiteralPath $installedPath -PathType Leaf)) {
            throw "Installed runtime reference is missing: $installedPath"
        }
        $bundleHash = (Get-FileHash -LiteralPath $bundlePath -Algorithm SHA256).Hash.ToLowerInvariant()
        $installedHash = (Get-FileHash -LiteralPath $installedPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($bundleHash -ne $installedHash) {
            throw "Private reference differs from the installed Steam runtime: " +
                "$relativePath; privateSha256=$bundleHash; installedSha256=$installedHash"
        }
        $result.Add([ordered]@{
            relativePath = $relativePath.Replace('\', '/')
            sha256 = $bundleHash
        })
    }
    return @($result)
}
