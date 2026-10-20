[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$boundary = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) ('KmgCollectorReuse-' + [Guid]::NewGuid().ToString('N'))))
$evidenceRoot = Join-Path $boundary 'evidence'
$scripts = Join-Path $boundary 'scripts'
$package = Join-Path $boundary 'candidate.zip'
$receiptPath = $package + '.development-receipt.json'
$deploymentPath = Join-Path $evidenceRoot 'deployments/deployment.json'
$live = Join-Path $boundary 'live'
$game = Join-Path $boundary 'game'
$priorFixture = $env:KMG_COLLECTOR_REUSE_FIXTURE_ROOT
try {
    New-Item -ItemType Directory -Path $scripts,$evidenceRoot,(Split-Path $deploymentPath),$live,$game -Force | Out-Null
    $env:KMG_COLLECTOR_REUSE_FIXTURE_ROOT = $boundary
    $collector = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Collect-Runtime-Evidence.ps1'))
    $collector = $collector.Replace('C:\Dev\KingmakerGunslingerLab\runtime-evidence',$evidenceRoot)
    [IO.File]::WriteAllText((Join-Path $scripts 'Collect-Runtime-Evidence.ps1'),$collector,[Text.UTF8Encoding]::new($false))
    @'
function Get-KmgRepositoryRoot { param($ScriptDirectory) Split-Path -Parent $ScriptDirectory }
function Get-KmgModInfo { param($RepositoryRoot) [pscustomobject]@{Version='0.0.161'} }
function Get-KmgGitState { param($RepositoryRoot) [pscustomobject]@{Commit='fixture-commit';Branch='codex/fixture';Status=@()} }
function Assert-KmgPathWithin {
    param($Path,$Root,[switch]$AllowRoot)
    $p=[IO.Path]::GetFullPath($Path)
    $r=[IO.Path]::GetFullPath($Root).TrimEnd('\')
    if(-not $p.StartsWith($r+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Fixture path escaped evidence root'}
    $p
}
function Get-KmgSha256 { param($Path) (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant() }
function Get-KmgDevelopmentReceiptPath { param($PackagePath) $PackagePath+'.development-receipt.json' }
function Assert-KmgReusableDeployment {
    param($DeploymentManifestPath,$PackagePath,$RepositoryRoot,$ExpectedVersion,$DevelopmentPurpose)
    $root=$env:KMG_COLLECTOR_REUSE_FIXTURE_ROOT
    $exact=Join-Path $root 'evidence/deployments/deployment.json'
    if($DeploymentManifestPath -cne $exact){throw 'Wrong fixture deployment path'}
    $d=Get-Content -Raw -LiteralPath $exact|ConvertFrom-Json
    $r=Get-Content -Raw -LiteralPath ($PackagePath+'.development-receipt.json')|ConvertFrom-Json
    $actual=(Get-FileHash -Algorithm SHA256 -LiteralPath $PackagePath).Hash.ToLowerInvariant()
    if($DevelopmentPurpose -cne 'sprint28-native-prototype' -or
        $ExpectedVersion -cne $r.version -or
        $d.packagePath -cne $PackagePath -or
        $d.packageSha256 -cne $actual -or
        $r.packageSha256 -cne $actual -or
        $d.dllSha256 -cne $r.dllSha256 -or
        $d.dllSha256 -cne (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $root 'live/KingmakerGunslinger.dll')).Hash.ToLowerInvariant()) {
        throw 'Fixture exact package/deployment/live DLL binding failed'
    }
    [pscustomobject]@{Build=$r;Deployment=$d}
}
function Read-KmgDevelopmentCandidate {
    param($PackagePath,$RepositoryRoot,$Purpose)
    if($Purpose -cne 'sprint28-native-prototype'){throw 'Wrong development purpose'}
    Get-Content -Raw -LiteralPath ($PackagePath+'.development-receipt.json')|ConvertFrom-Json
}
'@ | Set-Content -LiteralPath (Join-Path $scripts 'RuntimeHarness.Common.ps1') -Encoding UTF8
    [IO.File]::WriteAllText($package,'fixture immutable package')
    [IO.File]::WriteAllText((Join-Path $live 'KingmakerGunslinger.dll'),'fixture DLL')
    [IO.File]::WriteAllText((Join-Path $game 'Kingmaker.exe'),'fixture game')
    $packageHash=(Get-FileHash -Algorithm SHA256 -LiteralPath $package).Hash.ToLowerInvariant()
    $dllHash=(Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $live 'KingmakerGunslinger.dll')).Hash.ToLowerInvariant()
    [pscustomobject]@{version='0.0.161';packagePath=$package;packageSha256=$packageHash;dllSha256=$dllHash} |
        ConvertTo-Json | Set-Content -LiteralPath $receiptPath -Encoding UTF8
    [pscustomobject]@{packagePath=$package;packageSha256=$packageHash;dllSha256=$dllHash} |
        ConvertTo-Json | Set-Content -LiteralPath $deploymentPath -Encoding UTF8
    $positive=Join-Path $evidenceRoot 'positive'
    New-Item -ItemType Directory -Path $positive | Out-Null
    $collectArgs=@{EvidenceDirectory=$positive;EvidenceRoot=$evidenceRoot;PackagePath=$package;
        LiveModDirectory=$live;GameDirectory=$game;DevelopmentPurpose='sprint28-native-prototype';
        ValidatedDeploymentManifestPath=$deploymentPath}
    & (Join-Path $scripts 'Collect-Runtime-Evidence.ps1') @collectArgs | Out-Null
    $manifest=Get-Content -Raw -LiteralPath (Join-Path $positive 'runtime-evidence.json') | ConvertFrom-Json
    if($manifest.packageSha256 -cne $packageHash -or
        $manifest.builtDllSha256 -cne $dllHash -or
        $manifest.deployedDllSha256 -cne $dllHash -or
        $manifest.candidateKind -cne 'development' -or
        $manifest.releaseQualification -ne $false) {
        throw 'Positive exact deployment collector readback failed'
    }
    [IO.File]::WriteAllText($package,'tampered package')
    $negative=Join-Path $evidenceRoot 'negative'
    New-Item -ItemType Directory -Path $negative | Out-Null
    $collectArgs.EvidenceDirectory=$negative
    $refused=$false
    try { & (Join-Path $scripts 'Collect-Runtime-Evidence.ps1') @collectArgs | Out-Null }
    catch { $refused=$true }
    if(-not $refused -or (Test-Path -LiteralPath (Join-Path $negative 'runtime-evidence.json'))) {
        throw 'Tampered package reached the collector manifest'
    }
    Write-Host 'Validated deployment collector PASS: exact positive readback and changed-package refusal; no game or installation access.'
} finally {
    $env:KMG_COLLECTOR_REUSE_FIXTURE_ROOT=$priorFixture
    $temp=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
    if($boundary.StartsWith($temp+'\',[StringComparison]::OrdinalIgnoreCase) -and
        [IO.Path]::GetFileName($boundary).StartsWith('KmgCollectorReuse-') -and
        (Test-Path -LiteralPath $boundary)) {
        if(@(Get-ChildItem -LiteralPath $boundary -Recurse -Force |
            Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count -ne 0) {
            throw 'Fixture cleanup refused reparse point'
        }
        Remove-Item -LiteralPath $boundary -Recurse -Force
    }
}
