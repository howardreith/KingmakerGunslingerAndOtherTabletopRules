[CmdletBinding()]
param(
    [string]$MSBuildPath,
    [string]$ReferenceBundleDir,
    [string]$KingmakerInstallDir = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    # Prints a descriptive plan only; it does not prove actual child counts
    # or qualify an artifact. Tests must observe the executed nested calls.
    [switch]$PlanOnly,
    # Used only by a second reproducibility compilation of unchanged inputs,
    # after the release publisher has already run the unchanged domain suite.
    [switch]$SkipDomainTests,
    # Builds a development candidate for this one named purpose instead of a
    # release candidate. See DevelopmentCandidate.Common.ps1: it replaces only
    # the active version's release validator with the development validator
    # and the complete suite with -DevelopmentTestFilter's named selection,
    # writes to its own development-only location under its own name, and
    # records a development receipt that no release path accepts.
    [string]$DevelopmentPurpose,
    [string]$DevelopmentTestFilter
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'ReferenceProvenance.Common.ps1')
. (Join-Path $PSScriptRoot 'KmgGate.Common.ps1')

$development = -not [string]::IsNullOrEmpty($DevelopmentPurpose) -or
    -not [string]::IsNullOrEmpty($DevelopmentTestFilter)
if ($development) {
    if ([string]::IsNullOrEmpty($DevelopmentPurpose) -or
        [string]::IsNullOrWhiteSpace($DevelopmentTestFilter)) {
        throw 'A development candidate requires both -DevelopmentPurpose and a named -DevelopmentTestFilter.'
    }
    if ($SkipDomainTests) {
        throw 'A development candidate records its own selected tests; -SkipDomainTests belongs to release reproducibility.'
    }
    [void](Assert-KmgDevelopmentPurpose $DevelopmentPurpose)
    $DevelopmentTestFilter = Resolve-KmgGateFilter -Level Focused -Filter $DevelopmentTestFilter
}

# The plan is descriptive only. Do not require any machine-local reference,
# installed game or compiler just to enumerate it. The real path below still
# verifies every reference before validation/build/package work can start.
$script:KmgBuildLocalPlan = @(
    'repository-wrapper'
    'complete-domain-suite'
    'exact-reference-release-build'
    'package-assembly'
    'strict-package-validation'
)
if ($development) {
    $script:KmgBuildLocalPlan = @(
        'development-repository-wrapper'
        'selected-domain-tests'
        'exact-reference-release-build'
        'package-assembly'
        'strict-package-validation'
        'development-receipt'
    )
}
if ($PlanOnly) {
    $script:KmgBuildLocalPlan
    return
}

$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$info = Get-KmgModInfo -RepositoryRoot $root
if ($info.Version -notin @('0.0.166','0.0.167')) { throw "Build-Local supports released166 development or the selected167 public candidate, observed $($info.Version)." }
$msbuild = Resolve-KmgMsBuild -ExplicitPath $MSBuildPath
Write-Host "MSBuild: $msbuild"
$git = Get-KmgGitState -RepositoryRoot $root
$sourceStateSha256 = Get-KmgSourceStateFingerprint -RepositoryRoot $root
if (-not $ReferenceBundleDir) {
    $cursor = [IO.DirectoryInfo]$root
    for ($depth = 0; $depth -lt 6 -and $cursor; $depth++) {
        $candidate = Join-Path $cursor.FullName 'private\extracted-references\KingmakerGunslinger-private-build-references'
        if (Test-Path -LiteralPath $candidate -PathType Container) {
            $ReferenceBundleDir = $candidate
            break
        }
        $cursor = $cursor.Parent
    }
}
if (-not (Test-Path -LiteralPath $ReferenceBundleDir -PathType Container)) {
    throw "Qualified private reference bundle is missing: $ReferenceBundleDir"
}
$referenceIdentity = @(Assert-KmgReferenceBundleMatchesInstall `
    -ReferenceBundleDir $ReferenceBundleDir `
    -KingmakerInstallDir $KingmakerInstallDir)

$python = (Get-Command python -ErrorAction Stop).Source
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$dotnetRoot = Split-Path $dotnet
$csc = Get-ChildItem -LiteralPath (Join-Path $dotnetRoot 'sdk') -Filter csc.dll -Recurse -File |
    Where-Object { $_.FullName -like '*\Roslyn\bincore\csc.dll' } |
    Sort-Object FullName -Descending | Select-Object -First 1
if (-not $csc) { throw 'Roslyn csc.dll was not found beneath the installed dotnet SDK.' }
$net47 = 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7'
if (-not (Test-Path -LiteralPath (Join-Path $net47 'mscorlib.dll') -PathType Leaf)) {
    throw ".NET Framework 4.7 reference assemblies are missing: $net47"
}

if ($development) {
    if ($git.Status.Count -ne 0) {
        throw 'A development candidate is built only from an exactly clean commit.'
    }
    $developmentLayout = Get-KmgDevelopmentCandidateLayout -RepositoryRoot $root `
        -Purpose $DevelopmentPurpose -Commit $git.Commit -Id $info.Id -Version $info.Version
    if (Test-Path -LiteralPath $developmentLayout.ReceiptPath) {
        throw "This commit's '$DevelopmentPurpose' development candidate already exists and is immutable: $($developmentLayout.PackagePath)"
    }
}

# All read-only prerequisites and immutable-output refusals precede admission.
# The bounded S30-31 worktree has persistent cumulative dispatch authority.
# PlanOnly exits above; compile-only editing uses build.ps1, not this pipeline.
if ([IO.Path]::GetFullPath($root).TrimEnd('\') -ieq 'C:\Dev\KingmakerGunslingerLab\wt\s30-31') {
    . (Join-Path $PSScriptRoot 'Sprints3031Budget.Common.ps1')
    [void](Invoke-KmgSprints3031Budget -Path (Join-Path $root 'artifacts/closeout-triage/CLOSEOUT-BUDGET.json') `
        -Action Build -Stage ('candidate:' + $git.Commit) -MinimumRemainingMinutes 20)
}

if ($development) {
    if (Test-Path -LiteralPath $developmentLayout.Root) {
        # An earlier attempt for this exact commit stopped before its receipt.
        $partial = Assert-KmgPathWithin -Path $developmentLayout.Root `
            -Root (Join-Path $root $script:KmgDevelopmentCandidateRoot)
        Remove-Item -LiteralPath $partial -Recurse -Force
    }
    New-Item -ItemType Directory -Path $developmentLayout.Root -Force | Out-Null
}

if ($development) {
    # The development wrapper: every fixture and offline contract of the
    # repository wrapper, with the development validator in place of the
    # active version's release validator. Then the named selection, once.
    & (Join-Path $PSScriptRoot 'validate-repository.ps1') `
        -DevelopmentPurpose $DevelopmentPurpose `
        -DevelopmentSummaryPath $developmentLayout.ValidationSummaryPath
    Invoke-KmgWithTestFilter -Filter $DevelopmentTestFilter -Body {
        & (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release -Clean `
            -MSBuildPath $msbuild -SkipRepositoryValidation `
            -RunnerLogPath $developmentLayout.TestLogPath
    }
    if ((Get-Item -LiteralPath "$($developmentLayout.TestLogPath).stderr").Length -ne 0) {
        throw 'The selected domain-test runner wrote to standard error.'
    }
    $selectedTests = Read-KmgDomainTestEvidence -LogPath $developmentLayout.TestLogPath `
        -Filter $DevelopmentTestFilter
} else {
& (Join-Path $PSScriptRoot 'validate-repository.ps1')
# The scenario preflight was reachable only by hand, so the one test that
# proves the C# catalog, the PowerShell metadata table and the documented
# scenario union are the same set ran at no gate level - which is how a
# scenario the guarded harness would refuse as "not allowlisted" passed a
# full Sprint gate. It is source-only: no elevation, no install, no launch,
# WhatIf alone, so it belongs beside the repository wrapper rather than in
# a handoff instruction.
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Test-RuntimeScenarioPreflight.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Runtime scenario preflight failed.' }
# The repository wrapper has just run. test-domain.ps1 would otherwise run it
# again, which is the duplication the 2026-10-02 amendment called out: a
# tranche gate for one immutable commit must validate the repository once.
if (-not $SkipDomainTests) {
    & (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration Release -Clean `
        -MSBuildPath $msbuild -SkipRepositoryValidation
}
}

$localRoot = Join-Path $root "artifacts\local-runtime\$($info.Version)"
# The pilot can repair a final observer without overwriting any earlier
# exact candidate, manifest, compile archive or evidence-bound path.
if (-not $development -and $info.Version -ceq '0.0.167') {
    $localRoot = Join-Path $localRoot $git.Commit.Substring(0, 12)
    if (Test-Path -LiteralPath $localRoot) { throw 'This exact final candidate output already exists; preserve its bytes.' }
}
$exactRoot = if ($development) { $developmentLayout.ExactBuildRoot } else { Join-Path $localRoot 'exact-build' }
& $python (Join-Path $root 'tools\build_mod_from_private_references.py') `
    --reference-bundle-dir $ReferenceBundleDir --dotnet $dotnet `
    --csc $csc.FullName --net47-ref-dir $net47 --output-dir $exactRoot `
    --configuration Release --git-commit $git.Commit
if ($LASTEXITCODE -ne 0) { throw "Exact-reference Release build failed with exit code $LASTEXITCODE." }
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $PSScriptRoot 'Test-SupplyItemIcons.ps1') `
    -ReferenceBundleDir $ReferenceBundleDir
if ($LASTEXITCODE -ne 0) { throw 'Focused supply-icon validation failed.' }
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $PSScriptRoot 'Test-GunslingerAnimalCompanionEligibility.ps1') `
    -ManagedDir (Join-Path $ReferenceBundleDir 'Managed') `
    -ModAssemblyPath (Join-Path $exactRoot 'bin\KingmakerGunslinger.dll')
if ($LASTEXITCODE -ne 0) { throw 'Gunslinger animal-companion eligibility validation failed.' }

$buildOutput = Join-Path $root 'artifacts\bin\Release\KingmakerGunslinger'
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'blueprints') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\icons') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\icons\expanded-summoning') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\bundles') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $exactRoot 'bin\KingmakerGunslinger.dll') -Destination $buildOutput -Force
Copy-Item -LiteralPath (Join-Path $exactRoot 'bin\KingmakerGunslinger.pdb') -Destination $buildOutput -Force
Copy-Item -LiteralPath (Join-Path $root 'Info.json') -Destination $buildOutput -Force
Copy-Item -LiteralPath (Join-Path $root 'blueprints\blueprints.json') -Destination (Join-Path $buildOutput 'blueprints') -Force
Copy-Item -LiteralPath (Join-Path $root 'blueprints\blueprints.schema.json') -Destination (Join-Path $buildOutput 'blueprints') -Force
Copy-Item -Path (Join-Path $root 'assets\game\icons\*.png') -Destination (Join-Path $buildOutput 'assets\icons') -Force
Copy-Item -Path (Join-Path $root 'assets\game\icons\expanded-summoning\*') -Destination (Join-Path $buildOutput 'assets\icons\expanded-summoning') -Force
# The Pteranodon replacement visual: mesh data plus its painted albedo, the
# same two files package.ps1 ships and validate-build-output.ps1 requires.
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\pteranodon') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'assets\pteranodon\pteranodon-mesh.json') -Destination (Join-Path $buildOutput 'assets\pteranodon') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\pteranodon\pteranodon-albedo.png') -Destination (Join-Path $buildOutput 'assets\pteranodon') -Force
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\flying-animals') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'assets\flying-animals\dire-bat-mesh.json') -Destination (Join-Path $buildOutput 'assets\flying-animals') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\flying-animals\dire-bat-albedo.png') -Destination (Join-Path $buildOutput 'assets\flying-animals') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\flying-animals\eagle-mesh.json') -Destination (Join-Path $buildOutput 'assets\flying-animals') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\flying-animals\eagle-albedo.png') -Destination (Join-Path $buildOutput 'assets\flying-animals') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\flying-animals\giant-wasp-mesh.json') -Destination (Join-Path $buildOutput 'assets\flying-animals') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\flying-animals\giant-wasp-albedo.png') -Destination (Join-Path $buildOutput 'assets\flying-animals') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\flying-animals\stirge-mesh.json') -Destination (Join-Path $buildOutput 'assets\flying-animals') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\flying-animals\stirge-albedo.png') -Destination (Join-Path $buildOutput 'assets\flying-animals') -Force
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\ungulates') -Force | Out-Null
foreach ($kind in @('aurochs','bison','rhinoceros','woolly-rhinoceros','gorgon')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\ungulates\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\ungulates') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\ungulates\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\ungulates') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint12-quadrupeds') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'assets\ungulates\gorgon-parameters.png') -Destination (Join-Path $buildOutput 'assets\ungulates') -Force
foreach ($kind in @('dire-rat','hyena','goblin-dog')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint12-quadrupeds\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint12-quadrupeds') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint12-quadrupeds\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint12-quadrupeds') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint13-creatures') -Force | Out-Null
foreach ($kind in @('wolverine','shadow-mastiff','poisonous-frog')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint13-creatures\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint13-creatures') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint13-creatures\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint13-creatures') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint14-insects') -Force | Out-Null
foreach ($kind in @('fire-beetle','giant-ant-worker','giant-ant-soldier',
                          'giant-ant-drone','giant-stag-beetle')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint14-insects\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint14-insects') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint14-insects\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint14-insects') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint16-crocodilians') -Force | Out-Null
foreach ($kind in @('crocodile','dire-crocodile')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint16-crocodilians\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint16-crocodilians') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint16-crocodilians\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint16-crocodilians') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint17-serpents') -Force | Out-Null
foreach ($kind in @('viper','constrictor-snake','salamander')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint17-serpents\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint17-serpents') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint17-serpents\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint17-serpents') -Force
}
Copy-Item -LiteralPath (Join-Path $root 'assets\sprint17-serpents\salamander-human-mesh.json') -Destination (Join-Path $buildOutput 'assets\sprint17-serpents') -Force
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint18-primates') -Force | Out-Null
foreach ($kind in @('ape','dire-ape')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint18-primates\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint18-primates') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint18-primates\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint18-primates') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint19-fourarmed') -Force | Out-Null
foreach ($kind in @('girallon','xill')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint19-fourarmed\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint19-fourarmed') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint19-fourarmed\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint19-fourarmed') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint20-arachnids') -Force | Out-Null
foreach ($kind in @('giant-scorpion')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint20-arachnids\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint20-arachnids') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint20-arachnids\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint20-arachnids') -Force
}
# Sprint 21's two original bodies. The Bebelith's is the first in this
# series authored for a creature that already ships wearing something, so a
# release that omitted it would quietly put the Doomspider back.
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint21-arthropods') -Force | Out-Null
foreach ($kind in @('giant-crab','bebelith')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint21-arthropods\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint21-arthropods') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint21-arthropods\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint21-arthropods') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\ankylosaurus') -Force | Out-Null
# The refined (VIS-028) body replaced the v0.0.151 export, whose CRLF bytes
# this script once reproduced; the new export ships as committed (LF), like
# every other original body, and validate-package.ps1 pins its bytes.
foreach ($part in @('ankylosaurus-mesh.json','ankylosaurus-albedo.png')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\ankylosaurus\$part") -Destination (Join-Path $buildOutput 'assets\ankylosaurus') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint23-armored-dinosaurs') -Force | Out-Null
foreach ($kind in @('triceratops','stegosaurus')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint23-armored-dinosaurs\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint23-armored-dinosaurs') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint23-armored-dinosaurs\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint23-armored-dinosaurs') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\outsider-celestial') -Force | Out-Null
foreach ($part in @('lemure-mesh.json','lemure-albedo.png','dretch-mesh.json','dretch-albedo.png','hound-archon-mesh.json','hound-archon-albedo.png')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\outsider-celestial\$part") -Destination (Join-Path $buildOutput 'assets\outsider-celestial') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint24-theropods') -Force | Out-Null
foreach ($kind in @('deinonychus','tyrannosaurus')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint24-theropods\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint24-theropods') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint24-theropods\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint24-theropods') -Force
}
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint25-giant-bodies') -Force | Out-Null
foreach ($kind in @('brachiosaurus','elephant')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint25-giant-bodies\$kind-mesh.json") -Destination (Join-Path $buildOutput 'assets\sprint25-giant-bodies') -Force
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint25-giant-bodies\$kind-albedo.png") -Destination (Join-Path $buildOutput 'assets\sprint25-giant-bodies') -Force
}
$bundleManifest = Get-Content -LiteralPath (Join-Path $root 'assets\bundles\asset-bundle-manifest.json') -Raw | ConvertFrom-Json
$bundleSource = 'C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1\Builds\Windows\kingmakergunslinger.firearms'
if (-not (Test-Path -LiteralPath $bundleSource -PathType Leaf)) {
    # Use the unchanged checked-in artifact only when the same qualified hash
    # below proves it is identical; no Unity asset rebuild or installation.
    $bundleSource = Join-Path $root 'assets\bundles\kingmakergunslinger.firearms'
}
if (-not (Test-Path -LiteralPath $bundleSource -PathType Leaf)) { throw "Qualified firearm AssetBundle is missing: $bundleSource" }
if ((Get-KmgSha256 -Path $bundleSource) -ne $bundleManifest.sha256) { throw 'Firearm AssetBundle hash does not match the qualified manifest.' }
Copy-Item -LiteralPath $bundleSource -Destination (Join-Path $buildOutput 'assets\bundles\kingmakergunslinger.firearms') -Force
$spearBundle = Join-Path $root 'assets\bundles\kingmakergunslinger.elvenbranchedspear'
$easternBundle = Join-Path $root 'assets\bundles\kingmakergunslinger.easternweapons'
$spearManifest = @($bundleManifest.bundles | Where-Object { $_.name -ceq 'kingmakergunslinger.elvenbranchedspear' })
$easternManifest = @($bundleManifest.bundles | Where-Object { $_.name -ceq 'kingmakergunslinger.easternweapons' })
if ($spearManifest.Count -ne 1 -or $easternManifest.Count -ne 1 -or
    (Get-KmgSha256 -Path $spearBundle) -ne $spearManifest[0].sha256 -or
    (Get-KmgSha256 -Path $easternBundle) -ne $easternManifest[0].sha256) {
    throw 'Original custom-weapon bundle hash does not match the qualified manifest.'
}
Copy-Item -LiteralPath $spearBundle -Destination (Join-Path $buildOutput 'assets\bundles') -Force
Copy-Item -LiteralPath $easternBundle -Destination (Join-Path $buildOutput 'assets\bundles') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets\bundles\asset-bundle-manifest.json') -Destination (Join-Path $buildOutput 'assets\bundles') -Force
New-Item -ItemType Directory -Path (Join-Path $buildOutput 'assets\sprint27-griffon') -Force | Out-Null
foreach ($part in @('griffon-mesh.json','griffon-albedo.png')) {
    Copy-Item -LiteralPath (Join-Path $root "assets\sprint27-griffon\$part") -Destination (Join-Path $buildOutput 'assets\sprint27-griffon') -Force
}
# package.ps1 owns output validation, one assembly and strict validation.
$packagePath = if ($development) { $developmentLayout.PackagePath } else {
    Join-Path $localRoot "$($info.Id)-$($info.Version)-local-runtime.zip"
}
& (Join-Path $PSScriptRoot 'package.ps1') -Configuration Release -PackagePath $packagePath
if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) {
    throw "Package assembly failed with exit code $LASTEXITCODE."
}
if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf) -or
    -not (Test-Path -LiteralPath ($packagePath + '.sha256') -PathType Leaf)) {
    throw 'Package assembly did not produce the ZIP and checksum.'
}

# The build above was produced for $git.Commit. If the tree moved underneath
# this run - a concurrent commit, a checkout - the package would carry one
# commit's binaries under another commit's name, which is exactly the
# confusion a candidate hash exists to prevent.
$packagingCommit = Get-KmgGitState -RepositoryRoot $root
if ($packagingCommit.Commit -ne $git.Commit) {
    throw ("The repository moved during the build: started at $($git.Commit), " +
        "now at $($packagingCommit.Commit). The candidate would not describe " +
        'one commit.')
}

$stagedMod = Join-Path $root 'artifacts\staging\install\KingmakerGunslinger'

$dllPath = Join-Path $buildOutput 'KingmakerGunslinger.dll'
$firearmManifestPath = Join-Path $stagedMod `
    'assets\soundbanks\firearm-soundbank-manifest.json'
$firearmSoundBankPath = Join-Path $stagedMod `
    'assets\soundbanks\KMG_Firearms.bnk'
if ($development) {
    if ((Get-KmgSourceStateFingerprint -RepositoryRoot $root) -cne $sourceStateSha256) {
        throw 'The source state changed during the development build.'
    }
    [xml]$developmentProps = Get-Content -LiteralPath (Join-Path $root 'Directory.Build.props') -Raw
    $validationSummary = Get-Content -LiteralPath $developmentLayout.ValidationSummaryPath -Raw |
        ConvertFrom-Json
    $receipt = [ordered]@{
        schemaVersion = 1
        candidateKind = 'development'
        releaseQualification = $false
        generator = $script:KmgDevelopmentGenerator
        purpose = $DevelopmentPurpose
        generatedAtUtc = [DateTime]::UtcNow.ToString('o')
        repositoryRoot = $root
        commit = $git.Commit
        branch = $git.Branch
        sourceStateSha256 = $sourceStateSha256
        version = $info.Version
        informationalVersion = [string]$developmentProps.Project.PropertyGroup.KmgInformationalVersion
        packagePath = $packagePath
        packageSha256 = Get-KmgSha256 -Path $packagePath
        dllSha256 = Get-KmgSha256 -Path $dllPath
        dllMvid = Get-KmgDllMvid -Path $dllPath
        firearmManifestSha256 = Get-KmgSha256 -Path $firearmManifestPath
        firearmSoundBankSha256 = Get-KmgSha256 -Path $firearmSoundBankPath
        references = [ordered]@{
            bundleDirectory = $ReferenceBundleDir
            installDirectory = $KingmakerInstallDir
            files = @($referenceIdentity)
        }
        validation = [ordered]@{
            wrapper = 'scripts/validate-repository.ps1 -DevelopmentPurpose'
            validator = $validationSummary.validator
            purpose = $validationSummary.purpose
            releaseQualification = $validationSummary.releaseQualification
            baselineCommit = $validationSummary.baselineCommit
            baselineVersion = $validationSummary.baselineVersion
            releasedIdentities = $validationSummary.releasedIdentities
            identities = $validationSummary.identities
            appendedIdentities = $validationSummary.appendedIdentities
            ledgerSha256 = $validationSummary.ledgerSha256
            summarySha256 = Get-KmgSha256 -Path $developmentLayout.ValidationSummaryPath
        }
        selectedTests = $selectedTests
        toolchain = [ordered]@{ msbuild = $msbuild; python = $python; dotnet = $dotnet; csc = $csc.FullName }
    }
    $receipt | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $developmentLayout.ReceiptPath -Encoding UTF8
    # Read the receipt back through the consumer's own checks.
    $null = Read-KmgDevelopmentCandidate -PackagePath $packagePath -RepositoryRoot $root `
        -Purpose $DevelopmentPurpose
    Write-Host "DEVELOPMENT CANDIDATE (never a release): $packagePath"
    Write-Host "Purpose: $DevelopmentPurpose"
    Write-Host "Receipt: $($developmentLayout.ReceiptPath)"
    Write-Host "Selected tests: $($selectedTests.selected) of $($selectedTests.registered) registered; filter '$DevelopmentTestFilter'"
    Write-Host "Package SHA-256: $($receipt.packageSha256)"
    Write-Host "DLL SHA-256: $($receipt.dllSha256)"
    Write-Host 'No deployment was performed.'
    return
}
$manifest = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    generator = 'scripts/Build-Local.ps1'
    repositoryRoot = $root
    commit = $git.Commit
    branch = $git.Branch
    sourceStateSha256 = $sourceStateSha256
    version = $info.Version
    packagePath = $packagePath
    packageSha256 = Get-KmgSha256 -Path $packagePath
    dllSha256 = Get-KmgSha256 -Path $dllPath
    dllMvid = Get-KmgDllMvid -Path $dllPath
    firearmManifestSha256 = Get-KmgSha256 -Path $firearmManifestPath
    firearmSoundBankSha256 = Get-KmgSha256 -Path $firearmSoundBankPath
    validated = $true
}
# The named S30-31 reuse contract compares exact installed reference identities
# on the final artifact as well as on original development receipts.
if ($git.Branch -ceq 'codex/expanded-summoning-sprints30-31-outsider-celestial-pilot') {
    $manifest.references = [ordered]@{
        bundleDirectory = $ReferenceBundleDir
        installDirectory = $KingmakerInstallDir
        files = @($referenceIdentity)
    }
}
$manifestPath = Get-KmgPackageManifestPath -PackagePath $packagePath
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
Write-Host "Package: $packagePath"
Write-Host "Version: $($info.Version)"
Write-Host "Package SHA-256: $($manifest.packageSha256)"
Write-Host "DLL SHA-256: $($manifest.dllSha256)"
Write-Host 'No deployment was performed.'
