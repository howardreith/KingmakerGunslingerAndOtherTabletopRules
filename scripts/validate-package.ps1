[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$AllowMissingFirearmSoundBank
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# All validation writes are confined to the private temporary extraction.
# A caller's -WhatIf must not suppress module-scoped hashing or cleanup.
$originalGlobalWhatIfPreference = $global:WhatIfPreference
$global:WhatIfPreference = $false
try {

. (Join-Path $PSScriptRoot 'common.ps1')
. (Join-Path $PSScriptRoot 'IconCatalog.Common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$info = Get-KmgModInfo -RepositoryRoot $repositoryRoot

if (-not (Test-Path -LiteralPath $PackagePath -PathType Leaf)) {
    throw "Package does not exist: $PackagePath"
}

$tempDirectory = Join-Path ([IO.Path]::GetTempPath()) ("KmgPackageValidation-" + [Guid]::NewGuid().ToString('N'))
# Strict validation must use and remove only its private temp extraction even
# when the caller is a dry-run wrapper. No installation path is mutated.
New-Item -ItemType Directory -Path $tempDirectory -Force -WhatIf:$false | Out-Null

try {
    # The destination is unique and empty. Avoid the Force overwrite cleanup,
    # which can race on a just-extracted file.
    Expand-Archive -LiteralPath $PackagePath -DestinationPath $tempDirectory -WhatIf:$false
    $roots = @(Get-ChildItem -LiteralPath $tempDirectory -Directory)
    if ($roots.Count -ne 1 -or $roots[0].Name -ne $info.Id) {
        throw "Package must contain exactly one root directory named '$($info.Id)'."
    }

    $modDirectory = $roots[0].FullName
    $expected = @(
        'CHANGELOG.md',
        'Info.json',
        'INSTALLATION-COMPATIBILITY.md',
        'KingmakerGunslinger.dll',
        'LICENSE',
        'README.md',
        'SMOKE-TEST-GUIDE.md',
        'docs\WEAPON-FINDABILITY-RECOVERY.md',
        'docs\WEAPON-FINDABILITY-QUALIFICATION.md',
        'planning\PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md',
        'validation\weapon-findability-native-reference.json',
        'validation\weapon-findability-scene-corrections.json',
        'validation\weapon-findability-runtime-qualification.json',
        'docs\RELEASE-NOTES-0.0.145.md',
        'docs\RELEASE-NOTES-0.0.146.md',
        'docs\RELEASE-NOTES-0.0.151.md',
        'docs\RELEASE-NOTES-0.0.152.md',
        'docs\RELEASE-NOTES-0.0.153.md',
        'docs\RELEASE-NOTES-0.0.155.md',
        'docs\RELEASE-NOTES-0.0.156.md',
        'docs\RELEASE-NOTES-0.0.157.md',
        'docs\RELEASE-NOTES-0.0.158.md',
        'docs\RELEASE-NOTES-0.0.159.md',
        'docs\RELEASE-NOTES-0.0.160.md',
        'docs\RELEASE-NOTES-0.0.161.md',
        'docs\RELEASE-NOTES-0.0.162.md',
        'docs\RELEASE-NOTES-0.0.163.md',
        'docs\RELEASE-NOTES-0.0.164.md',
        'docs\RELEASE-NOTES-0.0.165.md',
        'docs\RELEASE-NOTES-0.0.166.md',
        'docs\RELEASE-NOTES-0.0.167.md',
        'docs\RELEASE-NOTES-0.0.168.md',
        'docs\RELEASE-NOTES-0.0.169.md',
        'THIRD-PARTY-ASSETS.md',
        'assets\bundles\kingmakergunslinger.firearms',
        'assets\bundles\kingmakergunslinger.elvenbranchedspear',
        'assets\bundles\kingmakergunslinger.easternweapons',
        'assets\bundles\asset-bundle-manifest.json',
        # The Pteranodon replacement visual ships as mesh data plus its
        # painted albedo rather than an AssetBundle: no bind poses, no
        # material, no editor dependency.
        'assets\pteranodon\pteranodon-mesh.json',
        'assets\pteranodon\pteranodon-albedo.png',
        'assets\flying-animals\dire-bat-mesh.json',
        'assets\flying-animals\dire-bat-albedo.png',
        'assets\flying-animals\eagle-mesh.json',
        'assets\flying-animals\eagle-albedo.png',
        'assets\flying-animals\giant-wasp-mesh.json',
        'assets\flying-animals\giant-wasp-albedo.png',
        'assets\flying-animals\stirge-mesh.json',
        'assets\flying-animals\stirge-albedo.png',
        'assets\ungulates\gorgon-mesh.json',
        'assets\ungulates\gorgon-albedo.png',
        'assets\ungulates\gorgon-parameters.png',
        'assets\ungulates\aurochs-mesh.json',
        'assets\ungulates\aurochs-albedo.png',
        'assets\ungulates\bison-mesh.json',
        'assets\ungulates\bison-albedo.png',
        'assets\ungulates\rhinoceros-mesh.json',
        'assets\ungulates\rhinoceros-albedo.png',
        'assets\ungulates\woolly-rhinoceros-mesh.json',
        'assets\ungulates\woolly-rhinoceros-albedo.png',
        'assets\sprint12-quadrupeds\dire-rat-mesh.json',
        'assets\sprint12-quadrupeds\dire-rat-albedo.png',
        'assets\sprint12-quadrupeds\hyena-mesh.json',
        'assets\sprint12-quadrupeds\hyena-albedo.png',
        'assets\sprint12-quadrupeds\goblin-dog-mesh.json',
        'assets\sprint12-quadrupeds\goblin-dog-albedo.png',
        'assets\sprint13-creatures\wolverine-mesh.json',
        'assets\sprint13-creatures\wolverine-albedo.png',
        'assets\sprint13-creatures\shadow-mastiff-mesh.json',
        'assets\sprint13-creatures\shadow-mastiff-albedo.png',
        'assets\sprint13-creatures\poisonous-frog-mesh.json',
        'assets\sprint13-creatures\poisonous-frog-albedo.png',
        'assets\sprint14-insects\fire-beetle-mesh.json',
        'assets\sprint14-insects\fire-beetle-albedo.png',
        'assets\sprint14-insects\giant-ant-worker-mesh.json',
        'assets\sprint14-insects\giant-ant-worker-albedo.png',
        'assets\sprint14-insects\giant-ant-soldier-mesh.json',
        'assets\sprint14-insects\giant-ant-soldier-albedo.png',
        'assets\sprint14-insects\giant-ant-drone-mesh.json',
        'assets\sprint14-insects\giant-ant-drone-albedo.png',
        'assets\sprint14-insects\giant-stag-beetle-mesh.json',
        'assets\sprint14-insects\giant-stag-beetle-albedo.png',
        'assets\sprint16-crocodilians\crocodile-mesh.json',
        'assets\sprint16-crocodilians\crocodile-albedo.png',
        'assets\sprint16-crocodilians\dire-crocodile-mesh.json',
        'assets\sprint16-crocodilians\dire-crocodile-albedo.png',
        'assets\sprint17-serpents\viper-mesh.json',
        'assets\sprint17-serpents\viper-albedo.png',
        'assets\sprint17-serpents\constrictor-snake-mesh.json',
        'assets\sprint17-serpents\constrictor-snake-albedo.png',
        'assets\sprint17-serpents\salamander-mesh.json',
        'assets\sprint17-serpents\salamander-human-mesh.json',
        'assets\sprint17-serpents\salamander-albedo.png',
        'assets\sprint32-support\lillend-mesh.json',
        'assets\sprint32-support\lillend-albedo.png',
        'assets\sprint33-shadow-outsiders\invisible-stalker-mesh.json',
        'assets\sprint33-shadow-outsiders\invisible-stalker-albedo.png',
        'assets\sprint33-shadow-outsiders\shadow-demon-mesh.json',
        'assets\sprint33-shadow-outsiders\shadow-demon-albedo.png',
        'assets\sprint18-primates\ape-mesh.json',
        'assets\sprint18-primates\ape-albedo.png',
        'assets\sprint18-primates\dire-ape-mesh.json',
        'assets\sprint18-primates\dire-ape-albedo.png',
        'assets\sprint19-fourarmed\girallon-mesh.json',
        'assets\sprint19-fourarmed\girallon-albedo.png',
        'assets\sprint19-fourarmed\xill-mesh.json',
        'assets\sprint19-fourarmed\xill-albedo.png',
        'assets\sprint20-arachnids\giant-scorpion-mesh.json',
        'assets\sprint20-arachnids\giant-scorpion-albedo.png',
        'assets\sprint21-arthropods\giant-crab-mesh.json',
        'assets\sprint21-arthropods\giant-crab-albedo.png',
        'assets\sprint21-arthropods\bebelith-mesh.json',
        'assets\sprint21-arthropods\bebelith-albedo.png',
        'assets\ankylosaurus\ankylosaurus-mesh.json',
        'assets\ankylosaurus\ankylosaurus-albedo.png',
        'assets\sprint23-armored-dinosaurs\triceratops-mesh.json',
        'assets\sprint23-armored-dinosaurs\triceratops-albedo.png',
        'assets\sprint23-armored-dinosaurs\stegosaurus-mesh.json',
        'assets\sprint23-armored-dinosaurs\stegosaurus-albedo.png',
        'assets\outsider-celestial\hound-archon-mesh.json',
        'assets\outsider-celestial\hound-archon-albedo.png',
        'assets\outsider-celestial\dretch-mesh.json',
        'assets\outsider-celestial\dretch-albedo.png',
        'assets\outsider-celestial\lemure-mesh.json',
        'assets\outsider-celestial\lemure-albedo.png',
        'assets\sprint24-theropods\deinonychus-mesh.json',
        'assets\sprint24-theropods\deinonychus-albedo.png',
        'assets\sprint24-theropods\tyrannosaurus-mesh.json',
        'assets\sprint24-theropods\tyrannosaurus-albedo.png',
        'assets\sprint25-giant-bodies\brachiosaurus-mesh.json',
        'assets\sprint25-giant-bodies\brachiosaurus-albedo.png',
        'assets\sprint25-giant-bodies\elephant-mesh.json',
        'assets\sprint25-giant-bodies\elephant-albedo.png',
        'assets\sprint27-griffon\griffon-mesh.json',
        'assets\sprint27-griffon\griffon-albedo.png',
        'blueprints\blueprints.json',
        'blueprints\blueprints.schema.json'
    )
    function Assert-KmgProtectedBodyExports {
    param([Parameter(Mandatory=$true)][string]$ModDirectory)
    # Curated mesh/painting pairs admitted by technical export review. Cleanup 2
    # replaces the Girallon, Crab and Griffon pins and adds the other revised
    # Stag Beetle, Stegosaurus, Elephant, Brachiosaurus and Gorgon pairs. This checks
    # exact export integrity; owner visual approval remains pending separately.
    # All unrelated protected pairs retain their existing exact identities.
    $protectedBodyExports = @{
        'assets\ungulates\gorgon-mesh.json' = '2d524e002371fe10ca3f89ac2b59a32de823711cc3d05fc2e174104bb6f53370'
        'assets\ungulates\gorgon-albedo.png' = '8974f854b188eb5f40589b214a6babca9df7fb002c2038f56109a46199075ffe'
        'assets\ungulates\gorgon-parameters.png' = '241c44e0ef86feac0acde88abd835c29ee138791e2b31af4d3b92133ea7d1351'
        'assets\sprint27-griffon\griffon-mesh.json' = 'e3db5d71b8f4c2bf4647bbcfdcea1b2438f4a060ad3d6c1ddc3ec6649af3eab9'
        'assets\sprint27-griffon\griffon-albedo.png' = '1ccd2ade87b48b1162910270395c32c294c7e84c20c3f961b8e720b40eddd124'

        'assets\sprint19-fourarmed\girallon-mesh.json' = 'd6a160c6b336cc0dc812f62ce03c90c5faa5baf611fcb46c7e81130ad71ca119'
        'assets\sprint19-fourarmed\girallon-albedo.png' = '3070d2d51d578060fd74bcc7b85ef074491699b0701768707ebc655fa06cc841'
        'assets\sprint19-fourarmed\xill-mesh.json' = 'd7a0cb0ade14f00e6df09bc8b5a57654083cbd1ebe23599fcdb37ec497f1a243'
        'assets\sprint19-fourarmed\xill-albedo.png' = 'eca0206533fad6c67cb2cbce79c5ff4eac7987b3311229c8d9f6b883522999fa'
        'assets\sprint20-arachnids\giant-scorpion-mesh.json' = '1b48a74e4296f4fc0e4903b3fa9ec1b776aae1b85f512d91ebc833e3619bba7f'
        'assets\sprint20-arachnids\giant-scorpion-albedo.png' = 'c4a5a29d54ee52c3c1679de55a6c0f3d7e46e8ad56d02b0c25e51c43f55feb1a'
        'assets\sprint21-arthropods\giant-crab-mesh.json' = '75a6d5d625f8e88d9cfccec0de3d411610123be0f94f4655d5fc9eeadaf05472'
        'assets\sprint21-arthropods\giant-crab-albedo.png' = 'ef06b21b0654bcee40ca893cfff28c20e5fea469402cadb1d7a3a524f8e48112'
        'assets\sprint21-arthropods\bebelith-mesh.json' = '7ff4e4dd8fc8edf4beb9dd983f245c329f40d94c751111888e8c5c658d1ab68e'
        'assets\sprint21-arthropods\bebelith-albedo.png' = 'afd98003bcc96ae32db4c3da63fdf3377d84f07141148ef7742cf8d3742fd554'
        'assets\ankylosaurus\ankylosaurus-mesh.json' = '3eb7585e6f410711c7bf84bf4e23ff0df2c04fbe06ec226a0552a715c9fa9ca9'
        'assets\ankylosaurus\ankylosaurus-albedo.png' = '2cdd8bc7e9139e748a9a88ec52e8a360245b6ca5ecb7829eec78a57d5a598bac'
        'assets\sprint18-primates\ape-albedo.png' = '85c7f480eeac7dddb125961061f0e62ba2460505fcee8198e88c208a95c6c7d9'
        'assets\sprint18-primates\ape-mesh.json' = '2f0196eca297019770894b66d69cb474e9bba59c449a95590326f888086631ee'
        'assets\sprint18-primates\dire-ape-albedo.png' = '37bdc23e705bfa0771105b768d1cd052157dfdc1d2c6bbd2a99236cf8268e219'
        'assets\sprint18-primates\dire-ape-mesh.json' = 'd0c1d0324cf7a12bbbc9b64874dfefb3ba010fb82020b87f72a47c9d9658700a'

        'assets\sprint14-insects\giant-stag-beetle-mesh.json' = '2ecf222413eb857051b70df8b5fd61592a939108c7661ccdfb96f20fbcea794d'
        'assets\sprint14-insects\giant-stag-beetle-albedo.png' = '967d506b96019914fd40363cc80b21b0ce08f80662ef96ef259f5e0a8caecde4'
        'assets\sprint23-armored-dinosaurs\stegosaurus-mesh.json' = '3f9e1a219de1372bfec4db521a6de02e04771ea8e2bfc53d64a8435f440d466e'
        'assets\sprint23-armored-dinosaurs\stegosaurus-albedo.png' = 'ab6d343e35377b0cb9d73a8b5a2ab19e70b4cfe1c6a0cb21f109673fa883d6d6'
        'assets\sprint25-giant-bodies\elephant-mesh.json' = '5afe5464331a083c0de63d7b4bee4a1266d71376fb8360f7d96b0bf7a4f13028'
        'assets\sprint25-giant-bodies\elephant-albedo.png' = '5a973962a26dbf38208b66feeb5ffd154d469ac69b25137ae487b9d6767f2919'
        'assets\sprint25-giant-bodies\brachiosaurus-mesh.json' = '73f3689c5abdda09fb68f9e08182cf671c6a753966bedaf21ebb16877c04c57c'
        'assets\sprint25-giant-bodies\brachiosaurus-albedo.png' = '160e4b71ae1335d6df37a0d8eac491e863b82dc272cfe24e42173a6d40107ea4'
    }
    foreach ($entry in $protectedBodyExports.GetEnumerator()) {
        $path = Join-Path $ModDirectory $entry.Key
        if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or (Get-KmgSha256 -Path $path) -cne $entry.Value) {
            throw "A packaged protected original body differs from its current technical export: $($entry.Key)"
        }
    }
    }
    Assert-KmgProtectedBodyExports -ModDirectory $modDirectory
    $lillendExports = @{
        'lillend-mesh.json'='2734cb0dc2de27e1eb7c6884d8eab598260bed513f170e4573f83a3d3230afec'
        'lillend-albedo.png'='561609b114d83e8d4ab4c48d66dab920f115c6f2a80eaf8d8d0302f1c8a3b5c4'
    }
    foreach ($entry in $lillendExports.GetEnumerator()) {
        if ((Get-KmgSha256 -Path (Join-Path $modDirectory ('assets\sprint32-support\'+$entry.Key))) -cne $entry.Value) {
            throw 'Packaged Lillend original export differs from its exact candidate input.'
        }
    }
    $shadowOutsiderExports = @{
        'invisible-stalker-mesh.json'='778a3943afad71acc94fd76b8ef157caa85e954110f01c1e440d0707254092b3'
        'invisible-stalker-albedo.png'='d113b4d7a011c04d9741cdf166f73a8456f5c3132b8c5e66393d2fb9184dcd51'
        'shadow-demon-mesh.json'='6085202d2e50968e57ab43dece08626756f5f44028b0b39cca4a22143704c507'
        'shadow-demon-albedo.png'='a78f6d42b435e4cd99a1a3ecdf73e2b25cfb77fbbe048258f0a9957303ccbb3c'
    }
    foreach ($entry in $shadowOutsiderExports.GetEnumerator()) {
        if ((Get-KmgSha256 -Path (Join-Path $modDirectory ('assets\sprint33-shadow-outsiders\'+$entry.Key))) -cne $entry.Value) {
            throw 'Packaged Sprint 33 original export differs from its candidate input.'
        }
    }
    $humanMesh = Join-Path $modDirectory 'assets\sprint17-serpents\salamander-human-mesh.json'
    if ((Get-KmgSha256 -Path $humanMesh) -cne 'b5524a694cac0657aaf37ce995e3143f1e0a18e8aa98d799d7fe17ed901c0f19') {
        throw 'The separately packaged Salamander human/tail mesh differs from its reviewed original export.'
    }
    $iconNames = @('gunslinger-class','firearm-proficiency','gunsmithing','grit',
        'deeds','nimble','bonus-feat','gun-training','true-grit','rapid-reload',
        'weapon-focus-firearm','deadeye','gunslingers-dodge','quick-clear','reload-firearm',
        'firearm-monogram-pistol','firearm-monogram-musket',
        'firearm-monogram-blunderbuss',
        'repair-firearm','overhaul-firearm','early-pistol','musket','blunderbuss',
        'rifle','revolver','lead-ball','black-powder','repair-kit',
        'gunsmith-kit','overhaul-kit','paper-cartridge','focused-aim',
        'cord-of-stubborn-resolve','shield-other','elven-branched-spear',
        'wakizashi','katana','nodachi','night-without-moon',
        'heavens-measure','world-tree-severer')
    $integratedIcons = @(Get-KmgIntegratedIconRecords -RepositoryRoot $repositoryRoot)
    $iconNames = @($iconNames + @($integratedIcons | ForEach-Object { $_.Key }) | Select-Object -Unique)
    Assert-KmgIntegratedIconFiles -ModDirectory $modDirectory -Records $integratedIcons
    $expected += @($iconNames | ForEach-Object { "assets\icons\$_.png" })
    foreach ($name in @('firearm-monogram-rifle','firearm-monogram-revolver')) {
        $retiredPath = Join-Path $modDirectory "assets\icons\$name.png"
        if (Test-Path -LiteralPath $retiredPath) {
            throw "Retired player-facing selector exists in package: $retiredPath"
        }
    }
    $summonManifest = Get-KmgSummonIconManifest -RepositoryRoot $repositoryRoot
    $expected += 'assets\icons\expanded-summoning\icon-manifest.json'
    foreach ($icon in $summonManifest.icons) {
        $relative = "assets\icons\expanded-summoning\$($icon.file)"
        $expected += $relative
        $packagedIcon = Join-Path $modDirectory $relative
        if (-not (Test-Path -LiteralPath $packagedIcon -PathType Leaf) -or
            (Get-KmgSha256 -Path $packagedIcon).ToLowerInvariant() -cne $icon.sha256) {
            throw "Expanded Summoning packaged icon hash mismatch: $($icon.key)."
        }
    }
    $sourceManifest=Join-Path $repositoryRoot 'assets\soundbanks\firearm-soundbank-manifest.json'
    $sourceBank=Join-Path $repositoryRoot 'assets\soundbanks\KMG_Firearms.bnk'
    $packagedManifest=Join-Path $modDirectory 'assets\soundbanks\firearm-soundbank-manifest.json'
    $packagedBank=Join-Path $modDirectory 'assets\soundbanks\KMG_Firearms.bnk'
    if(Test-Path -LiteralPath $packagedBank -PathType Leaf){
        $expected += @('assets\soundbanks\KMG_Firearms.bnk','assets\soundbanks\firearm-soundbank-manifest.json')
        if(-not (Test-Path -LiteralPath $packagedManifest -PathType Leaf)){
            throw 'Release package is missing the firearm SoundBank manifest.'
        }
        if((Get-KmgSha256 -Path $sourceManifest) -cne (Get-KmgSha256 -Path $packagedManifest)){
            throw 'Source and packaged firearm manifests differ.'
        }
        $manifest=Get-Content -LiteralPath $packagedManifest -Raw | ConvertFrom-Json
        if((Get-KmgSha256 -Path $sourceBank).ToUpperInvariant() -cne $manifest.sha256){throw 'Source firearm SoundBank hash mismatch.'}
        if((Get-KmgSha256 -Path $packagedBank).ToUpperInvariant() -cne $manifest.sha256){throw 'Packaged firearm SoundBank hash mismatch.'}
        $productionValidator=Join-Path $repositoryRoot `
            "artifacts\tests\$Configuration\KingmakerGunslinger.DomainTests\KingmakerGunslinger.DomainTests.exe"
        if(-not (Test-Path -LiteralPath $productionValidator -PathType Leaf)){
            throw "Production C# manifest validator is missing: $productionValidator"
        }
        $productionValidationOutput=@(& $productionValidator `
            '--validate-firearm-artifact' $packagedManifest $packagedBank)
        $productionValidationExitCode=$LASTEXITCODE
        foreach($line in $productionValidationOutput){Write-Host $line}
        if($productionValidationExitCode -ne 0){
            throw "Packaged firearm artifacts failed production C# validation with exit code $productionValidationExitCode."
        }
    } elseif(-not $AllowMissingFirearmSoundBank){throw 'Release package is missing authentic KMG_Firearms.bnk.'}
    $actual = @(
        Get-ChildItem -LiteralPath $modDirectory -Recurse -File |
            ForEach-Object { $_.FullName.Substring($modDirectory.Length).TrimStart('\', '/') } |
            Sort-Object
    )
    $expectedSorted = @($expected | Sort-Object)
    if (($actual -join "`n") -ne ($expectedSorted -join "`n")) {
        throw "Package entries do not match the strict release allowlist.`nExpected:`n$($expectedSorted -join [Environment]::NewLine)`nActual:`n$($actual -join [Environment]::NewLine)"
    }

    $packagedInfo = Get-Content -LiteralPath (Join-Path $modDirectory 'Info.json') -Raw | ConvertFrom-Json
    if ($packagedInfo.Id -ne $info.Id -or $packagedInfo.Version -ne $info.Version) {
        throw 'Packaged Info.json does not match the repository mod ID and version.'
    }
    if ($packagedInfo.AssemblyName -ne 'KingmakerGunslinger.dll') {
        throw 'Packaged Info.json names an unexpected mod assembly.'
    }

    $binaryFiles = @(
        Get-ChildItem -LiteralPath $modDirectory -Recurse -File |
            Where-Object { $_.Extension -in @('.dll', '.exe', '.pdb', '.mdb') }
    )
    if ($binaryFiles.Count -ne 1 -or $binaryFiles[0].Name -ne 'KingmakerGunslinger.dll') {
        throw 'The standalone UMM package must contain exactly one binary: KingmakerGunslinger.dll.'
    }

    $forbiddenNames = @(
        '0Harmony.dll',
        '0Harmony12.dll',
        'Assembly-CSharp.dll',
        'Assembly-CSharp-firstpass.dll',
        'Newtonsoft.Json.dll',
        'UnityEngine.dll',
        'UnityModManager.dll'
    )
    foreach ($name in $forbiddenNames) {
        if (Get-ChildItem -LiteralPath $modDirectory -Recurse -File -Filter $name -ErrorAction SilentlyContinue) {
            throw "Install package contains a private or foreign runtime assembly: $name"
        }
    }
    $audioArtifacts=@(Get-ChildItem -LiteralPath $modDirectory -Recurse -File | Where-Object {$_.Extension -in @('.bnk','.wem')})
    if($audioArtifacts.Count -gt 1 -or ($audioArtifacts.Count -eq 1 -and $audioArtifacts[0].Name -cne 'KMG_Firearms.bnk')){throw 'Package contains forbidden or unexpected Wwise artifacts.'}
    if(Get-ChildItem -LiteralPath $modDirectory -Recurse -File | Where-Object {$_.Name -ieq 'Init.bnk'}){throw 'Package contains forbidden Init.bnk.'}
}
finally {
    if (Test-Path -LiteralPath $tempDirectory) {
        # Package validation owns this temporary extraction even when a caller
        # is in -WhatIf mode; keep that preference from leaking the scratch.
        Remove-Item -LiteralPath $tempDirectory -Recurse -Force -WhatIf:$false
    }
}

Write-Host "Strict standalone UMM package validation passed: $PackagePath"
} finally {
    $global:WhatIfPreference = $originalGlobalWhatIfPreference
}
