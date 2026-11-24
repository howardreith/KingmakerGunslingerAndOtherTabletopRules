[CmdletBinding()]
param(
    # The commit before this change (default: origin/master), built once to record the loader the old project defaults
    # compiled against on this machine.
    [string]$BaselineCommit = 'origin/master',
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
$baseline = (& git -C $source rev-parse $BaselineCommit).Trim()
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
    $before = Invoke-Native 'powershell.exe' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $build, '-Configuration', 'Release', '-CompileOnly')
    $beforeRefs = if (Test-Path -LiteralPath $dll) { Get-LoaderRefs $dll } else { 'no DLL' }
    $installedUmm = [Reflection.AssemblyName]::GetAssemblyName((Join-Path $installedLoader 'UnityModManager.dll')).Version.ToString()
    Add-Case 'before-change-defaults-compile-against-installed-loader' ($before.ExitCode -eq 0 -and $beforeRefs -match "UnityModManager $([regex]::Escape($installedUmm))") "baseline $($baseline.Substring(0, 10)): build.ps1 -CompileOnly exit $($before.ExitCode); DLL references $beforeRefs (installed UnityModManager $installedUmm)"
    Remove-Item -LiteralPath (Join-Path $clone 'artifacts') -Recurse -Force -ErrorAction SilentlyContinue

    # After: this commit. build.ps1 fetches and verifies the pins; the DLL references the pinned versions.
    & git -C $clone checkout -q --detach $commit
    $after = Invoke-Native 'powershell.exe' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $build, '-Configuration', 'Release', '-CompileOnly')
    $afterRefs = if (Test-Path -LiteralPath $dll) { Get-LoaderRefs $dll } else { 'no DLL' }
    Add-Case 'build-ps1-compiles-against-pinned-loader' ($after.ExitCode -eq 0 -and $after.Output -match 'Pinned loader references verified' -and $afterRefs -ceq '0Harmony12 1.2.0.1, UnityModManager 0.31.1.0') "exit $($after.ExitCode); DLL references $afterRefs"
    $dllSha = if (Test-Path -LiteralPath $dll) { Get-Sha $dll } else { '' }

    # Direct MSBuild with the default (pinned cache present): accepted.
    $direct = & $msbuildDirect @()
    Add-Case 'direct-msbuild-default-uses-pinned-cache' ($direct.ExitCode -eq 0 -and $direct.Output -match 'Pinned loader references verified') "exit $($direct.ExitCode)"

    # Direct MSBuild pointed at the installed loader: refused before compilation; the DLL is not rebuilt.
    $installed = & $msbuildDirect @("/p:UnityModManagerDir=$installedLoader")
    Add-Case 'direct-msbuild-installed-loader-refused' ($installed.ExitCode -ne 0 -and $installed.Output -match 'KMGPIN: Pinned loader reference differs from its pin: UnityModManager/UnityModManager\.dll' -and $installed.Output -notmatch 'KingmakerGunslinger -> ' -and (Get-Sha $dll) -ceq $dllSha) ([regex]::Match($installed.Output, 'KMGPIN: [^;]*').Value)

    # A copy of the pinned files with one byte changed in 0Harmony12: refused.
    $tampered = Join-Path $temp 'tampered loader\UnityModManager'
    New-Item -ItemType Directory -Path $tampered -Force | Out-Null
    Copy-Item -Path (Join-Path $clone 'artifacts\references\loader\UnityModManager\*.dll') -Destination $tampered
    $bytes = [IO.File]::ReadAllBytes((Join-Path $tampered '0Harmony12.dll')); $bytes[$bytes.Length - 1] = $bytes[$bytes.Length - 1] -bxor 0xFF
    [IO.File]::WriteAllBytes((Join-Path $tampered '0Harmony12.dll'), $bytes)
    $t = & $msbuildDirect @("/p:UnityModManagerDir=$tampered")
    Add-Case 'direct-msbuild-altered-pinned-copy-refused' ($t.ExitCode -ne 0 -and $t.Output -match 'KMGPIN: Pinned loader reference differs from its pin: UnityModManager/0Harmony12\.dll') ([regex]::Match($t.Output, 'KMGPIN: [^;]*').Value)

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

    # A second UnityModManager reference injected by a Directory.Build.targets (pointing at the installed loader):
    # refused by the resolved-input check before compilation.
    $extraTargets = Join-Path $clone 'src\Directory.Build.targets'
    [IO.File]::WriteAllText($extraTargets, "<Project><ItemGroup><Reference Include=`"UnityModManagerExtra`"><HintPath>$([Security.SecurityElement]::Escape((Join-Path $installedLoader 'UnityModManager.dll')))</HintPath><Private>False</Private></Reference></ItemGroup></Project>")
    $x = & $msbuildDirect @()
    Remove-Item -LiteralPath $extraTargets
    Add-Case 'stray-second-unitymodmanager-reference-refused' ($x.ExitCode -ne 0 -and $x.Output -match 'Exactly one UnityModManager compiler input is required|Loader compiler references must be the verified pinned files' -and $x.Output -notmatch 'KingmakerGunslinger -> ') ([regex]::Match($x.Output, '(Exactly one UnityModManager compiler input is required|Loader compiler references must be the verified pinned files)[^\n]{0,160}').Value)

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
