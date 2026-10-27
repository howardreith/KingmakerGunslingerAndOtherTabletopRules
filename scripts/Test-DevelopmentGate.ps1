<# Executes current gate/build/test wrappers against compiler/process fixtures.
   Offline selection/orchestration checks only, never product qualification. #>
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$boundary = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) ('kmg-development-gate-' + [Guid]::NewGuid().ToString('N'))))
$fixtureScripts = Join-Path $boundary 'scripts'
$oldTrace = $env:KMG_GATE_FIXTURE_TRACE
$oldFilter = $env:KMG_TEST_FILTER
try {
    New-Item -ItemType Directory -Path $fixtureScripts,(Join-Path $boundary 'artifacts\tests\Release\KingmakerGunslinger.DomainTests') -Force | Out-Null
    foreach ($name in @('Invoke-KmgGate.ps1','build.ps1','test-domain.ps1','common.ps1','KmgGate.Common.ps1')) { Copy-Item (Join-Path $PSScriptRoot $name) $fixtureScripts }
    '<Project />' | Set-Content (Join-Path $boundary 'GamePath.props')
    '{"Id":"KingmakerGunslinger","Version":"0.0.150"}' | Set-Content (Join-Path $boundary 'Info.json')
    '<Project><PropertyGroup><KmgInformationalVersion>0.0.150-fixture-patch</KmgInformationalVersion></PropertyGroup></Project>' | Set-Content (Join-Path $boundary 'Directory.Build.props')
    $env:KMG_GATE_FIXTURE_TRACE = Join-Path $boundary 'trace.jsonl'
    $env:KMG_TEST_FILTER = 'prior'
    $compiler = Join-Path $fixtureScripts 'msbuild-fixture.ps1'
    @'
param([string]$Project,[Parameter(ValueFromRemainingArguments=$true)][string[]]$Rest)
@{event='compile';project=$Project;arguments=$Rest} | ConvertTo-Json -Compress | Add-Content $env:KMG_GATE_FIXTURE_TRACE
$global:LASTEXITCODE = 0
'@ | Set-Content $compiler
    foreach ($name in @('validate-repository.ps1','validate-build-output.ps1','validate-package.ps1','package.ps1')) { "throw 'Unexpected broad child: $name'" | Set-Content (Join-Path $fixtureScripts $name) }
    $exe = Join-Path $boundary 'artifacts\tests\Release\KingmakerGunslinger.DomainTests\KingmakerGunslinger.DomainTests.exe'
    Add-Type -OutputAssembly $exe -OutputType ConsoleApplication -TypeDefinition @'
using System;
using System.IO;
public class GateFixtureRunner {
    public static int Main() {
        string filter = Environment.GetEnvironmentVariable("KMG_TEST_FILTER");
        File.AppendAllText(Environment.GetEnvironmentVariable("KMG_GATE_FIXTURE_TRACE"), "{\"event\":\"runner\",\"filter\":\"" + filter + "\"}" + Environment.NewLine);
        return filter == "no-match" ? 1 : 0;
    }
}
'@
    & git -C $boundary init --quiet
    & git -C $boundary -c user.name=WorkflowFixture -c user.email=fixture@example.invalid commit --allow-empty --quiet -m fixture
    if ($LASTEXITCODE -ne 0) { throw 'Could not create bounded Git fixture.' }
    $gate = Join-Path $fixtureScripts 'Invoke-KmgGate.ps1'
    $refused = $false
    try { & $gate -MSBuildPath $compiler } catch { $refused = $_.Exception.Message -like '*non-empty*' }
    if (-not $refused -or (Test-Path $env:KMG_GATE_FIXTURE_TRACE)) { throw 'Blank focused selection was not refused before children.' }
    & $gate -Filter 'selected-behavior' -MSBuildPath $compiler
    $trace = @(Get-Content $env:KMG_GATE_FIXTURE_TRACE | ForEach-Object { $_ | ConvertFrom-Json })
    $compiles = @($trace | Where-Object event -eq compile)
    $runs = @($trace | Where-Object event -eq runner)
    if ($compiles.Count -ne 2 -or $compiles[0].project -notlike '*\src\KingmakerGunslinger\KingmakerGunslinger.csproj' -or
        $compiles[0].arguments -notcontains '/p:Platform=AnyCPU' -or $compiles[1].project -notlike '*DomainTests.csproj' -or
        $runs.Count -ne 1 -or $runs[0].filter -cne 'selected-behavior' -or $env:KMG_TEST_FILTER -cne 'prior') { throw 'Focused production/runner propagation or environment restoration failed.' }
    $refused = $false
    try { & $gate -Filter no-match -MSBuildPath $compiler } catch { $refused = $_.Exception.Message -like '*Domain tests failed*' }
    if (-not $refused -or $env:KMG_TEST_FILTER -cne 'prior') { throw 'Runner failure did not propagate and restore its filter.' }
    # Observe the normal build path too: one runner compile and one production
    # compile, rather than rebuilding the runner inside a solution build.
    { Add-Content $env:KMG_GATE_FIXTURE_TRACE '{"event":"repository-validation"}' }.ToString() | Set-Content (Join-Path $fixtureScripts 'validate-repository.ps1')
    { Add-Content $env:KMG_GATE_FIXTURE_TRACE '{"event":"output-validation"}' }.ToString() | Set-Content (Join-Path $fixtureScripts 'validate-build-output.ps1')
    $beforeNormal = @(Get-Content $env:KMG_GATE_FIXTURE_TRACE).Count
    & (Join-Path $fixtureScripts 'build.ps1') -Configuration Release -MSBuildPath $compiler
    $normal = @(Get-Content $env:KMG_GATE_FIXTURE_TRACE | Select-Object -Skip $beforeNormal | ForEach-Object { $_ | ConvertFrom-Json })
    if (@($normal | Where-Object event -eq compile).Count -ne 2 -or
        @($normal | Where-Object event -eq runner).Count -ne 1 -or
        @($normal | Where-Object event -eq repository-validation).Count -ne 1 -or
        @($normal | Where-Object { $_.event -eq 'compile' -and $_.project -like '*.sln' }).Count -ne 0) { throw 'Normal build repeated repository, runner or solution compilation.' }
    # Subsequent package guard checks stop at this external validator boundary.
    "throw 'Unexpected broad child: validate-build-output.ps1'" | Set-Content (Join-Path $fixtureScripts 'validate-build-output.ps1')
    # Execute the actual publisher build/package function, without its GitHub
    # publication entry. Private Build-Local is the external compiler boundary.
    @'
param($ReferenceBundleDir,$MSBuildPath,$KingmakerInstallDir,[switch]$SkipDomainTests)
@{event='candidate';skipDomainTests=[bool]$SkipDomainTests;filter=$env:KMG_TEST_FILTER} | ConvertTo-Json -Compress | Add-Content $env:KMG_GATE_FIXTURE_TRACE
$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root 'artifacts\bin\Release\KingmakerGunslinger'
. (Join-Path $PSScriptRoot 'common.ps1')
$package = (Get-KmgLocalCandidateLayout -RepositoryRoot $root -Commit ((& git -C $root rev-parse HEAD).Trim()) -Id 'KingmakerGunslinger' -Version '0.0.150').PackagePath
New-Item -ItemType Directory -Path $output,(Split-Path $package) -Force | Out-Null
'fixture DLL' | Set-Content (Join-Path $output 'KingmakerGunslinger.dll')
'fixture package' | Set-Content $package
'@ | Set-Content (Join-Path $fixtureScripts 'Build-Local.ps1')
    $tokens = $null; $errors = $null
    $ast = [Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Publish-Release.ps1'), [ref]$tokens, [ref]$errors)
    $function = @($ast.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq 'Invoke-QualifiedBuildAndPackage'}, $true))
    if ($errors.Count -or $function.Count -ne 1) { throw 'Could not resolve current publisher build function.' }
    $publisherFixture = @'
. (Join-Path $PSScriptRoot 'common.ps1')
. (Join-Path $PSScriptRoot 'KmgGate.Common.ps1')
function Resolve-RepositoryPath($RepositoryRoot,$Path) { [IO.Path]::GetFullPath($Path) }
function Assert-FileExists($Path,$Label) { if (-not (Test-Path $Path -PathType Leaf)) { throw "Missing $Label" } }
'@ + "`r`n" + $function[0].Extent.Text + "`r`n" + @'
$root = Split-Path -Parent $PSScriptRoot
$first = Invoke-QualifiedBuildAndPackage -RepositoryRoot $root -ConfiguredReferenceBundleDir 'fixture-references'
$second = Invoke-QualifiedBuildAndPackage -RepositoryRoot $root -ConfiguredReferenceBundleDir 'fixture-references' -SkipDomainTests
if ([IO.Path]::GetFileName($second.PackagePath) -cne 'KingmakerGunslinger-0.0.150-fixture-patch.zip' -or
    $first.PackageSha256 -cne $second.PackageSha256) { throw 'Publisher changed the asset name or candidate bytes.' }
'@
    $publisherPath = Join-Path $fixtureScripts 'publisher-build-fixture.ps1'
    $publisherFixture | Set-Content $publisherPath
    & $publisherPath
    $candidates = @(Get-Content $env:KMG_GATE_FIXTURE_TRACE | ForEach-Object { $_ | ConvertFrom-Json } | Where-Object event -eq candidate)
    if ($candidates.Count -ne 2 -or $candidates[0].skipDomainTests -or -not $candidates[1].skipDomainTests -or
        @($candidates | Where-Object { -not [string]::IsNullOrEmpty($_.filter) }).Count -or $env:KMG_TEST_FILTER -cne 'prior') { throw 'Publisher repeated domain tests or inherited/leaked a focused filter.' }
    $actualPackage = Join-Path $fixtureScripts 'actual-package.ps1'
    Copy-Item (Join-Path $PSScriptRoot 'package.ps1') $actualPackage
    foreach ($output in @((Join-Path $boundary 'outside.zip'), (Join-Path $boundary 'artifacts\wrong-extension.txt'))) {
        $refused = $false
        try { & $actualPackage -PackagePath $output } catch { $refused = $_.FullyQualifiedErrorId -like 'KMG_PACKAGE_OUTPUT_PATH*' }
        if (-not $refused -or (Test-Path $output)) { throw 'Package output escaped its bounded ZIP contract.' }
    }
    $reachedOutputValidator = $false
    try { & $actualPackage -PackagePath (Join-Path $boundary 'artifacts\custom.zip') } catch { $reachedOutputValidator = $_.Exception.Message -ceq 'Unexpected broad child: validate-build-output.ps1' }
    if (-not $reachedOutputValidator) { throw 'Valid package path did not retain output validation.' }
    # Observe the actual default-name calculation at the existing external
    # output-validator boundary, without building a product package.
    @'
@{event='default-package';path=$packagePath} | ConvertTo-Json -Compress | Add-Content $env:KMG_GATE_FIXTURE_TRACE
throw 'Fixture output validation boundary'
'@ | Set-Content (Join-Path $fixtureScripts 'validate-build-output.ps1')
    $reachedOutputValidator = $false
    try { & $actualPackage } catch { $reachedOutputValidator = $_.Exception.Message -ceq 'Fixture output validation boundary' }
    $defaults = @(Get-Content $env:KMG_GATE_FIXTURE_TRACE | ForEach-Object { $_ | ConvertFrom-Json } | Where-Object event -eq default-package)
    if (-not $reachedOutputValidator -or $defaults.Count -ne 1 -or
        $defaults[0].path -cne (Join-Path $boundary 'artifacts\packages\KingmakerGunslinger-0.0.150-fixture-patch.zip')) {
        throw 'Default package name did not follow the declared informational version.'
    }
    $propsPath = Join-Path $boundary 'Directory.Build.props'
    $propsBefore = [IO.File]::ReadAllBytes($propsPath)
    try {
        '<Project><PropertyGroup><KmgInformationalVersion>0.0.149-fixture-patch</KmgInformationalVersion></PropertyGroup></Project>' | Set-Content $propsPath
        $refused = $false
        try { & $actualPackage } catch { $refused = $_.Exception.Message -ceq 'Package name requires the source version and its declared informational suffix.' }
        $afterDefaults = @(Get-Content $env:KMG_GATE_FIXTURE_TRACE | ForEach-Object { $_ | ConvertFrom-Json } | Where-Object event -eq default-package)
        if (-not $refused -or $afterDefaults.Count -ne 1) { throw 'Mismatched informational version reached output validation.' }
    } finally { [IO.File]::WriteAllBytes($propsPath,$propsBefore) }
    # The isolated Build-Local fixture writes a ZIP but no validated receipt.
    # A full gate must refuse it even when that child returned normally.
    $missingManifestRefused = $false
    try { & $gate -Level Sprint -MSBuildPath $compiler } catch {
        $missingManifestRefused = $_.Exception.Message -like '*validated candidate ZIP and manifest*'
    }
    if (-not $missingManifestRefused -or $env:KMG_TEST_FILTER -cne 'prior') {
        throw 'Full gate accepted an unreceipted package or leaked its filter.'
    }
    # A validated package at the exact shared layout must be consumed by the
    # actual Sprint gate; a mismatched receipt must still be rejected.
    . (Join-Path $fixtureScripts 'common.ps1')
    $head = (& git -C $boundary rev-parse HEAD).Trim()
    $candidatePath = (Get-KmgLocalCandidateLayout -RepositoryRoot $boundary -Commit $head -Id 'KingmakerGunslinger' -Version '0.0.150').PackagePath
    $receipt = @{commit=$head;version='0.0.150';validated=$true;packagePath=$candidatePath;packageSha256=(Get-FileHash $candidatePath -Algorithm SHA256).Hash.ToLowerInvariant()}
    $receipt | ConvertTo-Json | Set-Content ($candidatePath + '.build-local.json')
    & $gate -Level Sprint -MSBuildPath $compiler
    $receipt.commit = 'b' * 40
    $receipt | ConvertTo-Json | Set-Content ($candidatePath + '.build-local.json')
    $staleRefused = $false
    try { & $gate -Level Sprint -MSBuildPath $compiler } catch { $staleRefused = $_.Exception.Message -like '*receipt does not match*' }
    if (-not $staleRefused) { throw 'Full gate accepted a stale candidate receipt.' }
    Write-Host 'Development gate: PASS=13 FAIL=0; observed production/test children, publisher reuse/filter isolation, default names and package output guards. Offline orchestration only.'
} finally {
    $env:KMG_GATE_FIXTURE_TRACE = $oldTrace
    $env:KMG_TEST_FILTER = $oldFilter
    $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
    if (-not $boundary.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup path.' }
    Remove-Item -LiteralPath $boundary -Recurse -Force
}
