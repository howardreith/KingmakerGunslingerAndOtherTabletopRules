[CmdletBinding()]
param(
    # The recorded commit before this change (master at 35763430ac, release 0.0.195 records), an immutable control built
    # once to record the loader the old project defaults compiled against on this machine.
    [ValidatePattern('^[0-9a-f]{40}$')][string]$BaselineCommit = '35763430acc9eefc306b861045526a444690bc77',
    [string]$ReportPath
)

# MSBuild behavior of the pinned loader references on a temporary clone of the committed HEAD (its path contains a
# space; GamePath.props is copied from this checkout). Each MSBuild runs in its own process. Nothing is deployed,
# launched or written to the installation; this checkout's artifacts are not touched.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$source = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$commit = (& git -C $source rev-parse HEAD).Trim()
$baseline = $BaselineCommit
& git -C $source cat-file -e ($baseline + '^{commit}')
if ($LASTEXITCODE -ne 0) { throw "Baseline commit $baseline is not in this repository." }
$msbuild = Resolve-KmgMsBuild
[xml]$gamePath = Get-Content -LiteralPath (Join-Path $source 'GamePath.props') -Raw
$installDir = [string](@($gamePath.Project.PropertyGroup) | Where-Object { $_.PSObject.Properties['KingmakerInstallDir'] } | Select-Object -First 1).KingmakerInstallDir
$installedLoader = Join-Path $installDir 'Kingmaker_Data\Managed\UnityModManager'
$cases = [Collections.Generic.List[object]]::new()
function Add-Case([string]$Name, [bool]$Passed, [string]$Detail) {
    $cases.Add([ordered]@{ name = $Name; passed = $Passed; detail = $Detail })
    Write-Host ("{0} {1} :: {2}" -f $(if ($Passed) { 'PASS' } else { 'FAIL' }), $Name, $Detail)
}
function Invoke-Native([string]$File, [string[]]$Arguments) {
    $previous = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    try { $output = (($null | & $File @Arguments 2>&1) | ForEach-Object { "$_" }) -join "`n" } finally { $ErrorActionPreference = $previous }
    return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output }
}
function Get-LoaderRefs([string]$Dll) {
    $result = Invoke-Native 'powershell.exe' @('-NoProfile', '-Command', "[Reflection.Assembly]::ReflectionOnlyLoad([IO.File]::ReadAllBytes('$($Dll.Replace("'", "''"))')).GetReferencedAssemblies() | Where-Object { `$_.Name -match '^(UnityModManager|0Harmony12|0Harmony)$' } | Sort-Object Name | ForEach-Object { `$_.Name + ' ' + `$_.Version }")
    return (($result.Output -split "`n") | Where-Object { $_ }) -join ', '
}
function Get-Sha([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }

$temp = Join-Path ([IO.Path]::GetTempPath()) ('kmg-pinned-loader-' + [Guid]::NewGuid().ToString('N'))
$clone = Join-Path $temp 'clone with space'
try {
    & git clone -q --no-hardlinks $source $clone
    if ($LASTEXITCODE -ne 0) { throw 'git clone failed.' }
    Copy-Item -LiteralPath (Join-Path $source 'GamePath.props') -Destination $clone
    $project = Join-Path $clone 'src\KingmakerGunslinger\KingmakerGunslinger.csproj'
    $dll = Join-Path $clone 'artifacts\bin\Release\KingmakerGunslinger\KingmakerGunslinger.dll'
    $build = Join-Path $clone 'scripts\build.ps1'
    $msbuildDirect = { param([string[]]$Extra) Invoke-Native $msbuild (@($project, '/nologo', '/v:m', '/nr:false', '/t:Build', '/p:Configuration=Release', '/p:Platform=AnyCPU') + $Extra) }

    # Before: the commit preceding this change, compiled with the project's own defaults.
    & git -C $clone checkout -q --detach $baseline
    if ($LASTEXITCODE -ne 0 -or (& git -C $clone rev-parse HEAD).Trim() -cne $baseline) { throw 'The clone did not select the baseline commit.' }
    $before = Invoke-Native 'powershell.exe' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $build, '-Configuration', 'Release', '-CompileOnly')
    $beforeRefs = if (Test-Path -LiteralPath $dll) { Get-LoaderRefs $dll } else { 'no DLL' }
    $installedUmm = [Reflection.AssemblyName]::GetAssemblyName((Join-Path $installedLoader 'UnityModManager.dll')).Version.ToString()
    Add-Case 'before-change-defaults-compile-against-installed-loader' ($before.ExitCode -eq 0 -and $beforeRefs -match "UnityModManager $([regex]::Escape($installedUmm))") "baseline $($baseline.Substring(0, 10)): build.ps1 -CompileOnly exit $($before.ExitCode); DLL references $beforeRefs (installed UnityModManager $installedUmm)"
    Remove-Item -LiteralPath (Join-Path $clone 'artifacts') -Recurse -Force -ErrorAction SilentlyContinue

    # After: this commit. build.ps1 fetches and verifies the pins; the DLL references the pinned versions.
    & git -C $clone checkout -q --detach $commit
    if ($LASTEXITCODE -ne 0 -or (& git -C $clone rev-parse HEAD).Trim() -cne $commit) { throw 'The clone did not select the commit under test.' }
    $after = Invoke-Native 'powershell.exe' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $build, '-Configuration', 'Release', '-CompileOnly')
    $afterRefs = if (Test-Path -LiteralPath $dll) { Get-LoaderRefs $dll } else { 'no DLL' }
    Add-Case 'build-ps1-compiles-against-pinned-loader' ($after.ExitCode -eq 0 -and $after.Output -match 'Pinned loader references verified' -and $afterRefs -ceq '0Harmony12 1.2.0.1, UnityModManager 0.31.1.0') "exit $($after.ExitCode); DLL references $afterRefs"
    $dllSha = if (Test-Path -LiteralPath $dll) { Get-Sha $dll } else { '' }

    # Direct MSBuild with the default (pinned cache present): accepted.
    $direct = & $msbuildDirect @()
    Add-Case 'direct-msbuild-default-uses-pinned-cache' ($direct.ExitCode -eq 0 -and $direct.Output -match 'Pinned loader references verified') "exit $($direct.ExitCode)"

    # Direct MSBuild pointed at the installed loader: judged by its bytes, never by its location. Byte-identical to the
    # pins it is valid; otherwise (UMM 0.33.0 on the development machine) it is refused before compilation.
    $pinnedUmm = Join-Path $clone 'artifacts\references\loader\UnityModManager\UnityModManager.dll'
    $pinnedHarmony = Join-Path $clone 'artifacts\references\loader\UnityModManager\0Harmony12.dll'
    # The installed-set decision, judged by bytes: the expected refusal names the first component that differs from the
    # pins (UnityModManager.dll, else 0Harmony12.dll); a byte-identical set is accepted. Exercised on controlled reference
    # directories (exact, UMM-only mismatch, Harmony-only mismatch) and on the actual installation (read only).
    function Get-ExpectedLoaderVerdict([string]$Directory) {
        if ((Get-Sha (Join-Path $Directory 'UnityModManager.dll')) -cne (Get-Sha $pinnedUmm)) { return 'UnityModManager/UnityModManager.dll' }
        if ((Get-Sha (Join-Path $Directory '0Harmony12.dll')) -cne (Get-Sha $pinnedHarmony)) { return 'UnityModManager/0Harmony12.dll' }
        return 'accepted'
    }
    function Test-LoaderSetVerdict([string]$Name, [string]$Directory) {
        $expected = Get-ExpectedLoaderVerdict $Directory
        $result = & $msbuildDirect @("/p:UnityModManagerDir=$Directory")
        $ok = if ($expected -eq 'accepted') { $result.ExitCode -eq 0 } else { $result.ExitCode -ne 0 -and $result.Output -match ('KMGPIN: Pinned loader reference differs from its pin: ' + [regex]::Escape($expected)) }
        Add-Case $Name $ok "expected $expected; exit $($result.ExitCode)"
    }
    $controlled = Join-Path $temp 'controlled sets'
    foreach ($set in @(@('exact', $false, $false), @('umm-only-mismatch', $true, $false), @('harmony-only-mismatch', $false, $true))) {
        $dir = Join-Path $controlled "$($set[0])\UnityModManager"; [void][IO.Directory]::CreateDirectory($dir)
        Copy-Item -LiteralPath $pinnedUmm -Destination (Join-Path $dir 'UnityModManager.dll'); Copy-Item -LiteralPath $pinnedHarmony -Destination (Join-Path $dir '0Harmony12.dll')
        if ($set[1]) { $b = [IO.File]::ReadAllBytes((Join-Path $dir 'UnityModManager.dll')); $b[$b.Length - 1] = $b[$b.Length - 1] -bxor 0x5A; [IO.File]::WriteAllBytes((Join-Path $dir 'UnityModManager.dll'), $b) }
        if ($set[2]) { $b = [IO.File]::ReadAllBytes((Join-Path $dir '0Harmony12.dll')); $b[$b.Length - 1] = $b[$b.Length - 1] -bxor 0x5A; [IO.File]::WriteAllBytes((Join-Path $dir '0Harmony12.dll'), $b) }
        Test-LoaderSetVerdict "direct-msbuild-controlled-$($set[0])-set" $dir
    }
    Test-LoaderSetVerdict 'direct-msbuild-installed-loader-judged-by-bytes' $installedLoader

    # A copy of the pinned files with one byte changed in 0Harmony12: refused.
    $tampered = Join-Path $temp 'tampered loader\UnityModManager'
    New-Item -ItemType Directory -Path $tampered -Force | Out-Null
    Copy-Item -Path (Join-Path $clone 'artifacts\references\loader\UnityModManager\*.dll') -Destination $tampered
    $bytes = [IO.File]::ReadAllBytes((Join-Path $tampered '0Harmony12.dll')); $bytes[$bytes.Length - 1] = $bytes[$bytes.Length - 1] -bxor 0xFF
    [IO.File]::WriteAllBytes((Join-Path $tampered '0Harmony12.dll'), $bytes)
    $t = & $msbuildDirect @("/p:UnityModManagerDir=$tampered")
    Add-Case 'direct-msbuild-altered-pinned-copy-refused' ($t.ExitCode -ne 0 -and $t.Output -match 'KMGPIN: Pinned loader reference differs from its pin: UnityModManager/0Harmony12\.dll') ([regex]::Match($t.Output, 'KMGPIN: [^;]*').Value)

    # A source manifest that does not declare the pinned floor: a direct build is refused before compilation.
    $infoFile = Join-Path $clone 'Info.json'
    $infoOriginal = [IO.File]::ReadAllText($infoFile)
    [IO.File]::WriteAllText($infoFile, ($infoOriginal -replace '"ManagerVersion":\s*"0\.31\.1"', '"ManagerVersion": "0.33.0"'))
    $mf = & $msbuildDirect @()
    [IO.File]::WriteAllText($infoFile, $infoOriginal)
    Add-Case 'direct-msbuild-mismatched-source-manifest-refused' ($mf.ExitCode -ne 0 -and $mf.Output -match "Info\.json ManagerVersion '0\.33\.0' must equal the pinned loader floor 0\.31\.1" -and $mf.Output -notmatch 'KingmakerGunslinger -> ') ([regex]::Match($mf.Output, 'Info\.json ManagerVersion[^.]*').Value)

    # An exact copy of the pinned files in another folder named UnityModManager: accepted (identity is by content).
    $copy = Join-Path $temp 'verified copy\UnityModManager'
    New-Item -ItemType Directory -Path $copy -Force | Out-Null
    Copy-Item -Path (Join-Path $clone 'artifacts\references\loader\UnityModManager\*.dll') -Destination $copy
    $c = & $msbuildDirect @("/p:UnityModManagerDir=$copy")
    Add-Case 'direct-msbuild-verified-copy-accepted' ($c.ExitCode -eq 0 -and (Get-LoaderRefs $dll) -ceq '0Harmony12 1.2.0.1, UnityModManager 0.31.1.0') "exit $($c.ExitCode)"

    # A folder not named UnityModManager: refused.
    $odd = Join-Path $temp 'odd\Loader'
    New-Item -ItemType Directory -Path $odd -Force | Out-Null
    Copy-Item -Path (Join-Path $clone 'artifacts\references\loader\UnityModManager\*.dll') -Destination $odd
    $o = & $msbuildDirect @("/p:UnityModManagerDir=$odd")
    Add-Case 'direct-msbuild-folder-not-named-unitymodmanager-refused' ($o.ExitCode -ne 0 -and $o.Output -match 'UnityModManagerDir must be the UnityModManager folder') "exit $($o.ExitCode)"

    # A stray HintPath: a Directory.Build.targets replaces the project's UnityModManager reference with a byte-identical
    # copy of the pinned file in another folder. The folder check passes (the pinned folder is unchanged), so only the
    # resolved-input check can refuse it, before compilation.
    $strayDir = Join-Path $temp 'stray copy'
    New-Item -ItemType Directory -Path $strayDir -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $clone 'artifacts\references\loader\UnityModManager\UnityModManager.dll') -Destination $strayDir
    $extraTargets = Join-Path $clone 'src\Directory.Build.targets'
    [IO.File]::WriteAllText($extraTargets, "<Project><ItemGroup><Reference Remove=`"UnityModManager`" /><Reference Include=`"UnityModManager`"><HintPath>$([Security.SecurityElement]::Escape((Join-Path $strayDir 'UnityModManager.dll')))</HintPath><Private>False</Private></Reference></ItemGroup></Project>")
    $dllBeforeStray = Get-Sha $dll
    $x = & $msbuildDirect @()
    Remove-Item -LiteralPath $extraTargets
    Add-Case 'stray-hintpath-to-unitymodmanager-copy-refused' ($x.ExitCode -ne 0 -and $x.Output -match 'Loader compiler references must be the verified pinned files' -and $x.Output -match [regex]::Escape((Join-Path $strayDir 'UnityModManager.dll')) -and $x.Output -notmatch 'KingmakerGunslinger -> ' -and (Get-Sha $dll) -ceq $dllBeforeStray) ([regex]::Match($x.Output, 'Loader compiler references must be the verified pinned files[^\n]{0,200}').Value)

    $failed = @($cases | Where-Object { -not $_.passed })
    $summary = [ordered]@{
        schema = 'kmg.pinned-loader-msbuild-tests.v1'
        commit = $commit
        baseline_commit = $baseline
        installed_loader_measured = [ordered]@{ unityModManager = $installedUmm; sha256 = Get-Sha (Join-Path $installedLoader 'UnityModManager.dll') }
        product_dll_sha256 = $dllSha
        passed = $cases.Count - $failed.Count
        total = $cases.Count
        cases = @($cases)
    }
    if ($ReportPath) { $summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $ReportPath -Encoding UTF8 }
    Write-Host ("RESULT {0}/{1} pinned-loader MSBuild checks passed" -f $summary.passed, $summary.total)
    if ($failed.Count -gt 0) { throw "$($failed.Count) pinned-loader MSBuild check(s) failed." }
}
finally { Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction SilentlyContinue }
