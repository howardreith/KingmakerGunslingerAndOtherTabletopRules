[CmdletBinding()]
param(
    # The private reference bundle Build-Local compiles against (the lab's extracted bundle by default).
    [string]$ReferenceBundleDir = 'C:\Dev\KingmakerGunslingerLab\private\extracted-references\KingmakerGunslinger-private-build-references',
    # A released TRE DLL compiled against a newer loader (v0.0.187 references UnityModManager 0.33.0.0), read only.
    [Parameter(Mandatory = $true)][string]$NewerCompiledDll,
    [string]$ReportPath
)

# Candidate admission on a real development candidate built by Build-Local in a temporary clone of the committed HEAD
# (GamePath.props and the verified loader cache copied, so nothing is downloaded). Covers the standalone package
# validator (the DLL's actual loader references, the complete archive entry set), the receipt bound to the DLL inside the
# ZIP, stale candidates after a material-input change at the same version, and the loader harness preflight. Nothing is
# deployed or launched; this checkout and the installation are untouched.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
. (Join-Path $PSScriptRoot 'common.ps1')
$source = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$commit = (& git -C $source rev-parse HEAD).Trim()
$purpose = 'umm-0311-admission'
$cases = [Collections.Generic.List[object]]::new()
function Add-Case([string]$Name, [bool]$Passed, [string]$Detail) {
    $cases.Add([ordered]@{ name = $Name; passed = $Passed; detail = $Detail })
    Write-Host ("{0} {1} :: {2}" -f $(if ($Passed) { 'PASS' } else { 'FAIL' }), $Name, $Detail)
}
function Invoke-PS([string[]]$Arguments) {
    $previous = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    try { $output = (($null | & powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass @Arguments 2>&1) | ForEach-Object { "$_" }) -join "`n" } finally { $ErrorActionPreference = $previous }
    return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -replace '\s+', ' ') }
}
function Get-Sha([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Get-Reason([string]$Text) { $m = [regex]::Match($Text, '(?i)(refus|stale|not the build|not exactly|outside the|must|not match)[^.]{0,220}'); if ($m.Success) { $m.Value } else { $Text.Substring([Math]::Max(0, $Text.Length - 240)) } }
function New-ZipVariant([string]$From, [string]$To, [hashtable]$Replace = @{}, [hashtable]$Add = @{}) {
    $in = [IO.Compression.ZipFile]::OpenRead($From); $out = [IO.Compression.ZipFile]::Open($To, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($entry in $in.Entries) {
            $target = $out.CreateEntry($entry.FullName); $writer = $target.Open()
            try { if ($Replace.ContainsKey($entry.FullName)) { $b = [byte[]]$Replace[$entry.FullName]; $writer.Write($b, 0, $b.Length) } else { $reader = $entry.Open(); try { $reader.CopyTo($writer) } finally { $reader.Dispose() } } }
            finally { $writer.Dispose() }
        }
        foreach ($name in $Add.Keys) { $target = $out.CreateEntry($name); $writer = $target.Open(); try { $b = [byte[]]$Add[$name]; $writer.Write($b, 0, $b.Length) } finally { $writer.Dispose() } }
    } finally { $out.Dispose(); $in.Dispose() }
}

$temp = Join-Path ([IO.Path]::GetTempPath()) ('kmg-candidate-admission-' + [Guid]::NewGuid().ToString('N'))
$clone = Join-Path $temp 'clone with space'
try {
    & git clone -q --no-hardlinks $source $clone
    & git -C $clone checkout -q -B candidate-admission $commit
    if ((& git -C $clone rev-parse HEAD).Trim() -cne $commit) { throw 'The clone did not select the commit under test.' }
    Copy-Item -LiteralPath (Join-Path $source 'GamePath.props') -Destination $clone
    New-Item -ItemType Directory -Path (Join-Path $clone 'artifacts\references\loader\UnityModManager') -Force | Out-Null
    Copy-Item -Path (Join-Path $source 'artifacts\references\loader\UnityModManager\*.dll') -Destination (Join-Path $clone 'artifacts\references\loader\UnityModManager')
    $git = { param([string[]]$a) & git -C $clone -c user.name=candidate-admission -c user.email=candidate-admission@invalid @a }

    # A real development candidate (Build-Local, development purpose, one named loader test).
    $build = Invoke-PS @('-File', (Join-Path $clone 'scripts\Build-Local.ps1'), '-ReferenceBundleDir', $ReferenceBundleDir,
        '-DevelopmentPurpose', $purpose, '-DevelopmentTestFilter', 'loader.production-dll-references-pinned-loader')
    $package = [regex]::Match($build.Output, 'DEVELOPMENT CANDIDATE \(never a release\): (.+?\.zip)').Groups[1].Value
    Add-Case 'development-candidate-built' ($build.ExitCode -eq 0 -and $package -and (Test-Path -LiteralPath $package)) "exit $($build.ExitCode); $package"
    if (-not $package) { throw "Build-Local produced no candidate: $($build.Output.Substring([Math]::Max(0, $build.Output.Length - 600)))" }
    $receiptPath = [regex]::Match($build.Output, 'Receipt: (.+?\.json)').Groups[1].Value
    $harness = Join-Path $clone 'scripts\RuntimeHarness.Common.ps1'
    $admit = { param([string]$zip) Invoke-PS @('-Command', ". '$($harness.Replace("'", "''"))'; `$r = Read-KmgBuildLocalManifest -PackagePath '$($zip.Replace("'", "''"))' -RepositoryRoot '$($clone.Replace("'", "''"))' -DevelopmentPurpose '$purpose' -RequireCurrentMaterialInputs; 'ADMITTED ' + `$r.dllSha256") }
    $r = & $admit $package
    Add-Case 'matching-candidate-admitted' ($r.ExitCode -eq 0 -and $r.Output -match 'ADMITTED [0-9a-f]{64}') "exit $($r.ExitCode)"

    # Standalone validator: the genuine ZIP passes; a newer-compiled TRE DLL in an otherwise valid 0.31.1 ZIP and extra
    # runtime DLLs at the archive root or inside the mod tree are refused.
    $validator = Join-Path $clone 'scripts\validate-package.ps1'
    $v = Invoke-PS @('-File', $validator, '-PackagePath', $package)
    Add-Case 'validator-genuine-baseline-zip-accepted' ($v.ExitCode -eq 0) "exit $($v.ExitCode)"
    $variants = Join-Path $temp 'variants'; New-Item -ItemType Directory -Path $variants | Out-Null
    $dllEntry = @([IO.Compression.ZipFile]::OpenRead($package).Entries | Where-Object { $_.FullName.Replace('\', '/') -ceq 'KingmakerGunslinger/KingmakerGunslinger.dll' })[0].FullName
    $newer = Join-Path $variants 'newer-compiled-dll.zip'
    New-ZipVariant $package $newer @{ $dllEntry = [IO.File]::ReadAllBytes($NewerCompiledDll) }
    $v = Invoke-PS @('-File', $validator, '-PackagePath', $newer)
    Add-Case 'validator-newer-compiled-tre-dll-refused-for-its-assemblyref' ($v.ExitCode -ne 0 -and $v.Output -match 'UnityModManager, Version=0\.33\.0\.0.*not exactly the pinned loader assemblies') (Get-Reason $v.Output)
    foreach ($extra in @(@('archive-root', '0Harmony.dll'), @('mod-tree', 'KingmakerGunslinger/assets/0Harmony.dll'))) {
        $variant = Join-Path $variants ('extra-' + $extra[0] + '.zip')
        New-ZipVariant $package $variant @{} @{ $extra[1] = [Text.Encoding]::ASCII.GetBytes('runtime') }
        $v = Invoke-PS @('-File', $validator, '-PackagePath', $variant)
        Add-Case "validator-extra-runtime-dll-at-$($extra[0])-refused" ($v.ExitCode -ne 0) (Get-Reason $v.Output)
    }

    # The receipt must describe the DLL inside the ZIP: a valid-format MVID or DLL hash from another build is refused
    # before staging while the ZIP digest still matches.
    $receiptOriginal = [IO.File]::ReadAllText($receiptPath)
    foreach ($edit in @(@('mvid', 'dllMvid', ([Guid]::NewGuid().ToString())), @('dll-hash', 'dllSha256', ('ab' * 32)))) {
        $j = $receiptOriginal | ConvertFrom-Json; $j.($edit[1]) = $edit[2]
        [IO.File]::WriteAllText($receiptPath, ($j | ConvertTo-Json -Depth 12))
        $r = & $admit $package
        Add-Case "receipt-with-another-builds-$($edit[0])-refused" ($r.ExitCode -ne 0 -and $r.Output -match 'not the build its receipt names') (Get-Reason $r.Output)
        [IO.File]::WriteAllText($receiptPath, $receiptOriginal)
    }

    # Stale candidates: a committed change to a material input with the version held constant refuses the old candidate;
    # reverting it (material inputs equal to the producing commit again) admits it.
    foreach ($material in @(@('project', 'src/KingmakerGunslinger/KingmakerGunslinger.csproj', '<!-- material change -->'), @('reference-helper', 'scripts/ReferenceProvenance.Common.ps1', '# material change'))) {
        $path = Join-Path $clone $material[1]
        $original = [IO.File]::ReadAllText($path)
        [IO.File]::WriteAllText($path, $original + "`r`n" + $material[2] + "`r`n")
        & $git @('commit', '-q', '-am', "test: $($material[0]) change") | Out-Null
        $r = & $admit $package
        Add-Case "candidate-after-$($material[0])-change-refused" ($r.ExitCode -ne 0 -and $r.Output -match 'candidate is stale: material inputs changed') (Get-Reason $r.Output)
        [IO.File]::WriteAllText($path, $original)
        & $git @('commit', '-q', '-am', "test: revert $($material[0]) change") | Out-Null
    }
    $r = & $admit $package
    Add-Case 'candidate-admitted-again-after-reverts' ($r.ExitCode -eq 0) "exit $($r.ExitCode)"

    # Loader harness preflight: the same admission before anything is acquired.
    $harnessDir = Join-Path $temp 'harness'; New-Item -ItemType Directory -Path $harnessDir | Out-Null
    foreach ($name in @('KmgLoaderCompatHarness.dll', 'Info.json')) { Set-Content -LiteralPath (Join-Path $harnessDir $name) -Value 'fixture' }
    $lc = Join-Path $clone 'scripts\compatibility\loader-compat'
    $preflight = { param([string]$zip) Invoke-PS @('-Command', ". '$((Join-Path $lc 'LoaderCompat.Common.ps1').Replace("'", "''"))'; . '$((Join-Path $lc 'LoaderCompat.Transaction.ps1').Replace("'", "''"))'; 'PREFLIGHT ' + (Assert-LcSessionInputs '$($zip.Replace("'", "''"))' '$($harnessDir.Replace("'", "''"))' '$purpose')") }
    $pf = & $preflight $package
    Add-Case 'loader-preflight-admits-matching-candidate' ($pf.ExitCode -eq 0 -and $pf.Output -match 'PREFLIGHT [0-9a-f]{64}') "exit $($pf.ExitCode)"
    $pf = & $preflight $newer
    Add-Case 'loader-preflight-refuses-mismatched-zip' ($pf.ExitCode -ne 0 -and $pf.Output -match 'Candidate admission refused') (Get-Reason $pf.Output)

    $failed = @($cases | Where-Object { -not $_.passed })
    $summary = [ordered]@{ schema = 'kmg.candidate-admission-tests.v1'; commit = $commit; purpose = $purpose; package_sha256 = Get-Sha $package
        newer_compiled_dll_sha256 = Get-Sha $NewerCompiledDll; passed = $cases.Count - $failed.Count; total = $cases.Count; cases = @($cases) }
    if ($ReportPath) { $summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $ReportPath -Encoding UTF8 }
    Write-Host ("RESULT {0}/{1} candidate admission checks passed" -f $summary.passed, $summary.total)
    if ($failed.Count -gt 0) { throw "$($failed.Count) candidate admission check(s) failed." }
}
finally { Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction SilentlyContinue }
