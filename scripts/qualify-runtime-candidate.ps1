[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$KingmakerInstallDir,

    [Parameter(Mandatory = $true)]
    [string]$Storefront,

    [Parameter(Mandatory = $true)]
    [string]$DisplayedGameVersion,

    [string]$MSBuildPath,

    [string[]]$EnabledMods = @(),

    [switch]$IncludeSymbols
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$info = Get-KmgModInfo -RepositoryRoot $repositoryRoot

if ($IncludeSymbols) {
    throw 'The standalone UMM package contract permits exactly one binary. PDB symbols must not be included.'
}
if (-not (Test-Path -LiteralPath $KingmakerInstallDir -PathType Container)) {
    throw "KingmakerInstallDir does not exist: $KingmakerInstallDir"
}
$KingmakerInstallDir = (Resolve-Path -LiteralPath $KingmakerInstallDir).Path

$qualificationRoot = Join-Path $repositoryRoot "artifacts\qualification\$($info.Id)-$($info.Version)-s28"
$environmentPath = Join-Path $qualificationRoot 'environment.json'
$contractsPath = Join-Path $qualificationRoot 'runtime-contracts.json'
$testsDirectory = Join-Path $qualificationRoot 'tests'
$compileDirectory = Join-Path $qualificationRoot 'compile'
$candidateName = "$($info.Id)-$($info.Version)-complete-maintenance-loop-smoke-test.zip"
$candidatePath = Join-Path $qualificationRoot $candidateName
$candidateChecksumPath = "$candidatePath.sha256"
$qualificationJsonPath = Join-Path $qualificationRoot 'runtime-candidate.json'
$qualificationMarkdownPath = Join-Path $qualificationRoot 'RUNTIME-CANDIDATE.md'
$bundlePath = Join-Path $repositoryRoot "artifacts\qualification\$($info.Id)-$($info.Version)-s28-bundle.zip"
$bundleChecksumPath = "$bundlePath.sha256"

if (Test-Path -LiteralPath $qualificationRoot) {
    Remove-Item -LiteralPath $qualificationRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $testsDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $compileDirectory -Force | Out-Null

Write-Host 'Step 1/8: validating the Sprint 29 source repository.'
& (Join-Path $PSScriptRoot 'validate-repository.ps1')

Write-Host 'Step 2/8: validating the explicit Kingmaker installation path.'
$managedDirectory = Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed'
if (-not (Test-Path -LiteralPath (Join-Path $managedDirectory 'Assembly-CSharp.dll') -PathType Leaf)) {
    throw "Assembly-CSharp.dll was not found under the supplied Kingmaker installation: $managedDirectory"
}

Write-Host 'Step 3/8: capturing the exact local environment and current runtime contracts.'
& (Join-Path $PSScriptRoot 'fingerprint-environment.ps1') `
    -KingmakerInstallDir $KingmakerInstallDir `
    -Storefront $Storefront `
    -DisplayedGameVersion $DisplayedGameVersion `
    -EnabledMods $EnabledMods `
    -OutputPath $environmentPath
& (Join-Path $PSScriptRoot 'inspect-runtime-contracts.ps1') `
    -KingmakerInstallDir $KingmakerInstallDir `
    -OutputPath $contractsPath
$contracts = Get-Content -LiteralPath $contractsPath -Raw | ConvertFrom-Json
if (-not $contracts.contractPassed) {
    throw 'The retained Kingmaker attack, roll, state, save, and damage runtime-contract report did not pass.'
}

Write-Host 'Step 4/8: building the dependency-free test harness.'
& (Join-Path $PSScriptRoot 'test-domain.ps1') `
    -Configuration Release `
    -MSBuildPath $MSBuildPath `
    -Clean
$testExecutable = Join-Path $repositoryRoot 'artifacts\tests\Release\KingmakerGunslinger.DomainTests\KingmakerGunslinger.DomainTests.exe'
if (-not (Test-Path -LiteralPath $testExecutable -PathType Leaf)) {
    throw "Domain test executable was not produced: $testExecutable"
}

Write-Host 'Step 5/8: executing the full test suite three times with byte-identical output.'
$testOutputHashes = @()
for ($run = 1; $run -le 3; $run++) {
    $runOutput = @(& $testExecutable 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "Domain test run $run failed with exit code $LASTEXITCODE."
    }
    $runText = ($runOutput -join [Environment]::NewLine) + [Environment]::NewLine
    $runPath = Join-Path $testsDirectory ("run{0}.stdout.txt" -f $run)
    [IO.File]::WriteAllText($runPath, $runText, (New-Object Text.UTF8Encoding($false)))
    if (-not $runText.TrimEnd().EndsWith('Completed 611 tests; failures=0.')) {
        throw "Domain test run $run did not report the expected 611-case zero-failure summary."
    }
    $testOutputHashes += Get-KmgSha256 -Path $runPath
}
if (@($testOutputHashes | Select-Object -Unique).Count -ne 1) {
    throw 'The three domain-test outputs were not byte-identical.'
}

Write-Host 'Step 6/8: compiling twice at the same Release output path and comparing DLL and PDB bytes.'
$buildArguments = @{
    KingmakerInstallDir = $KingmakerInstallDir
    MSBuildPath = $MSBuildPath
}
& (Join-Path $PSScriptRoot 'Build-Local.ps1') @buildArguments
$modDllPath = Join-Path $repositoryRoot 'artifacts\bin\Release\KingmakerGunslinger\KingmakerGunslinger.dll'
$modPdbPath = Join-Path $repositoryRoot 'artifacts\bin\Release\KingmakerGunslinger\KingmakerGunslinger.pdb'
if (-not (Test-Path -LiteralPath $modDllPath -PathType Leaf)) {
    throw "The first Release build did not produce the mod DLL: $modDllPath"
}
if (-not (Test-Path -LiteralPath $modPdbPath -PathType Leaf)) {
    throw "The first Release build did not produce the mod PDB: $modPdbPath"
}
$firstDllHash = Get-KmgSha256 -Path $modDllPath
$firstPdbHash = Get-KmgSha256 -Path $modPdbPath
Set-Content -LiteralPath (Join-Path $compileDirectory 'first-dll.sha256') -Value "$firstDllHash  KingmakerGunslinger.dll" -Encoding ASCII
Set-Content -LiteralPath (Join-Path $compileDirectory 'first-pdb.sha256') -Value "$firstPdbHash  KingmakerGunslinger.pdb" -Encoding ASCII

& (Join-Path $PSScriptRoot 'Build-Local.ps1') @buildArguments
if (-not (Test-Path -LiteralPath $modDllPath -PathType Leaf)) {
    throw "The second Release build did not produce the mod DLL: $modDllPath"
}
if (-not (Test-Path -LiteralPath $modPdbPath -PathType Leaf)) {
    throw "The second Release build did not produce the mod PDB: $modPdbPath"
}
$secondDllHash = Get-KmgSha256 -Path $modDllPath
$secondPdbHash = Get-KmgSha256 -Path $modPdbPath
Set-Content -LiteralPath (Join-Path $compileDirectory 'second-dll.sha256') -Value "$secondDllHash  KingmakerGunslinger.dll" -Encoding ASCII
Set-Content -LiteralPath (Join-Path $compileDirectory 'second-pdb.sha256') -Value "$secondPdbHash  KingmakerGunslinger.pdb" -Encoding ASCII
if ($firstDllHash -ne $secondDllHash) {
    throw 'Same-output-path deterministic compilation failed: the two mod DLL hashes differ.'
}
if ($firstPdbHash -ne $secondPdbHash) {
    throw 'Same-output-path deterministic compilation failed: the two mod PDB hashes differ.'
}

Write-Host 'Step 7/8: producing and validating the strict standalone UMM ZIP.'
& (Join-Path $PSScriptRoot 'package.ps1') -Configuration Release
$builtPackagePath = Join-Path $repositoryRoot "artifacts\packages\$candidateName"
if (-not (Test-Path -LiteralPath $builtPackagePath -PathType Leaf)) {
    throw "The expected UMM package was not produced: $builtPackagePath"
}
& (Join-Path $PSScriptRoot 'validate-package.ps1') -PackagePath $builtPackagePath
Copy-Item -LiteralPath $builtPackagePath -Destination $candidatePath
$candidateSha256 = Get-KmgSha256 -Path $candidatePath
Set-Content -LiteralPath $candidateChecksumPath -Value "$candidateSha256  $candidateName" -Encoding ASCII

Write-Host 'Step 8/8: sealing qualification evidence and instructions.'
$blueprintManifestPath = Join-Path $repositoryRoot 'blueprints\blueprints.json'
$qualification = [ordered]@{
    schemaVersion = 1
    createdAtUtc = [DateTime]::UtcNow.ToString('o')
    modId = $info.Id
    modVersion = $info.Version
    informationalVersion = '0.0.29-s29-complete-maintenance-loop'
    classification = 'READY FOR KINGMAKER Sprint 29 complete maintenance-loop smoke test; runtime acceptance pending'
    readyForKingmakerSmokeTest = $true
    sprint29Blocked = $true
    targetGame = $DisplayedGameVersion
    environmentSha256 = Get-KmgSha256 -Path $environmentPath
    runtimeContracts = [ordered]@{
        sha256 = Get-KmgSha256 -Path $contractsPath
        passed = [bool]$contracts.contractPassed
        requiredRuleEventSignature = 'System.Void OnTrigger(Kingmaker.RuleSystem.RulebookEventContext)'
        requiredMainRollSetterSignature = 'private System.Void set_Roll(Kingmaker.RuleSystem.RulebookEvent+RollEntry)'
        requiredSuccessEvaluatorSignature = 'public System.Boolean IsSuccessRoll(System.Int32)'
        authoritativeStateCarrier = 'item-owned inert BlueprintWeaponEnchantment token'
        uniqueIdVaultRequired = $false
    }
    domainTests = [ordered]@{
        declaredAndExecuted = 611
        runs = 3
        failures = 0
        repeatedOutputIdentical = $true
        executableSha256 = Get-KmgSha256 -Path $testExecutable
        outputSha256 = $testOutputHashes[0]
    }
    compile = [ordered]@{
        configuration = 'Release'
        targetFramework = '.NET Framework 4.7'
        languageVersion = '7.3'
        warningsAsErrors = $true
        sameOutputPathDeterministic = $true
        modDllSha256 = $secondDllHash
        modPdbSha256 = $secondPdbHash
    }
    package = [ordered]@{
        fileName = $candidateName
        sha256 = $candidateSha256
        validatedAsStrictStandaloneUmmPackage = $true
        entryCount = 8
        binaryCount = 1
    }
    blueprintManifestSha256 = Get-KmgSha256 -Path $blueprintManifestPath
    runtimeAcceptance = 'Pending the complete SMOKE-TEST-GUIDE-0.0.29.md gate; do not begin Sprint 30 yet.'
}
$qualification | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $qualificationJsonPath -Encoding UTF8

Copy-Item -LiteralPath (Join-Path $repositoryRoot 'SMOKE-TEST-GUIDE-0.0.29.md') -Destination $qualificationRoot
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'SPRINT-28-REPORT.md') -Destination $qualificationRoot

$markdown = @"
# Kingmaker Gunslinger 0.0.29 runtime candidate

> **READY FOR KINGMAKER — INSTALL ``$candidateName`` THROUGH UNITY MOD MANAGER**

This standalone package passed source validation, the current installed-runtime contract inspection, 611 tests three times with zero failures and byte-identical output, two same-output-path deterministic Release compiles with byte-identical DLL and PDB output, build-output validation, and strict eight-file UMM-package validation.

It is not yet runtime-accepted. Use only a disposable campaign and follow ``SMOKE-TEST-GUIDE-0.0.29.md``. Sprint 30 remains blocked until the complete live gate passes with no KMG or Harmony fault.

- Package SHA-256: ``$candidateSha256``
- Mod DLL SHA-256: ``$secondDllHash``
- Mod PDB SHA-256: ``$secondPdbHash``
- Test output SHA-256: ``$($testOutputHashes[0])``
- Runtime-contract report SHA-256: ``$(Get-KmgSha256 -Path $contractsPath)``

The adjacent ``environment.json`` contains the local installation path and should not be published without review. No private game, Unity, UMM, Harmony, or Newtonsoft assembly is included in the standalone ZIP.
"@
Set-Content -LiteralPath $qualificationMarkdownPath -Value $markdown -Encoding UTF8

if (Test-Path -LiteralPath $bundlePath) {
    Remove-Item -LiteralPath $bundlePath -Force
}
if (Test-Path -LiteralPath $bundleChecksumPath) {
    Remove-Item -LiteralPath $bundleChecksumPath -Force
}
Compress-Archive -LiteralPath $qualificationRoot -DestinationPath $bundlePath -CompressionLevel Optimal
Set-Content -LiteralPath $bundleChecksumPath `
    -Value "$(Get-KmgSha256 -Path $bundlePath)  $([IO.Path]::GetFileName($bundlePath))" `
    -Encoding ASCII

Write-Host ''
Write-Host 'READY FOR KINGMAKER'
Write-Host "Install through Unity Mod Manager: $candidatePath"
Write-Host "Qualification bundle: $bundlePath"
Write-Host 'Runtime acceptance remains pending; Sprint 30 is still blocked.'
