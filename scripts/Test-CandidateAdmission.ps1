[CmdletBinding()]
param(
    # The private reference bundle Build-Local compiles against (the lab's extracted bundle by default).
    [string]$ReferenceBundleDir = 'C:\Dev\KingmakerGunslingerLab\private\extracted-references\KingmakerGunslinger-private-build-references',
    # A released TRE DLL compiled against a newer loader (v0.0.187 references UnityModManager 0.33.0.0), read only.
    [Parameter(Mandatory = $true)][string]$NewerCompiledDll,
    # The published v0.0.188 release ZIP (an identified older release with a committed release record), read only.
    [Parameter(Mandatory = $true)][string]$HistoricalReleaseZip,
    # Another baseline-compatible build's DLL (the released v0.0.195 DLL: UnityModManager 0.31.1.0, another source stamp).
    [Parameter(Mandatory = $true)][string]$OtherBuildDll,
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

# Build-Local's repository validation requires a checkout inside the lab, so the disposable clone lives under the lab's
# worktrees folder (and is removed afterwards).
$lab = Split-Path -Parent (Split-Path -Parent $source)
$temp = Join-Path $lab ('worktrees\candidate-admission-' + [Guid]::NewGuid().ToString('N'))
$clone = Join-Path $temp 'clone with space'
try {
    & git clone -q --no-hardlinks $source $clone
    # A detached checkout (round 5): Build-Local records the empty branch of a detached build in the receipt.
    & git -C $clone checkout -q --detach $commit
    if ((& git -C $clone rev-parse HEAD).Trim() -cne $commit) { throw 'The clone did not select the commit under test.' }
    Copy-Item -LiteralPath (Join-Path $source 'GamePath.props') -Destination $clone
    New-Item -ItemType Directory -Path (Join-Path $clone 'artifacts\references\loader\UnityModManager') -Force | Out-Null
    Copy-Item -Path (Join-Path $source 'artifacts\references\loader\UnityModManager\*.dll') -Destination (Join-Path $clone 'artifacts\references\loader\UnityModManager')
    $git = { param([string[]]$a) & git -C $clone -c user.name=candidate-admission -c user.email=candidate-admission@invalid @a }

    # A real development candidate (Build-Local, development purpose, one named loader test).
    $build = Invoke-PS @('-File', (Join-Path $clone 'scripts\Build-Local.ps1'), '-ReferenceBundleDir', $ReferenceBundleDir,
        '-DevelopmentPurpose', $purpose, '-DevelopmentTestFilter', 'model-d.ItemIdentityEconomicsEnchantmentsAndCraftingUnchanged')
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
    $packageArchive = [IO.Compression.ZipFile]::OpenRead($package)
    try { $dllEntry = @($packageArchive.Entries | Where-Object { $_.FullName.Replace('\', '/') -ceq 'KingmakerGunslinger/KingmakerGunslinger.dll' })[0].FullName }
    finally { $packageArchive.Dispose() }
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
    # A receipt carrying only a compiler MVID (no compiler hash) that is not the packaged DLL's: refused. The earlier
    # receipt format (no compiler or packaged-DLL fields, the stamp field kept) with the genuine DLL: admitted.
    $j = $receiptOriginal | ConvertFrom-Json; [void]$j.PSObject.Properties.Remove('compilerDllSha256'); $j.compilerDllMvid = [Guid]::NewGuid().ToString()
    [IO.File]::WriteAllText($receiptPath, ($j | ConvertTo-Json -Depth 12))
    $r = & $admit $package
    [IO.File]::WriteAllText($receiptPath, $receiptOriginal)
    Add-Case 'receipt-with-only-an-inconsistent-compiler-mvid-refused' ($r.ExitCode -ne 0 -and $r.Output -match 'not the exact compiler output its receipt records \(MVID') (Get-Reason $r.Output)
    $j = $receiptOriginal | ConvertFrom-Json; foreach ($field in @('compilerDllSha256', 'compilerDllMvid', 'packagedDllSha256')) { [void]$j.PSObject.Properties.Remove($field) }
    [IO.File]::WriteAllText($receiptPath, ($j | ConvertTo-Json -Depth 12))
    $r = & $admit $package
    [IO.File]::WriteAllText($receiptPath, $receiptOriginal)
    Add-Case 'earlier-format-receipt-with-the-genuine-dll-admitted' ($r.ExitCode -eq 0 -and $r.Output -match 'ADMITTED [0-9a-f]{64}') "exit $($r.ExitCode)"
    # R1-F4: the producer's recorded source state must be an exactly clean checkout of its commit. An inconsistent
    # fingerprint is refused; so is the fingerprint a build records with a modified shipped input (computed by the
    # production fingerprint function in that modified state), after that input was restored.
    $j = $receiptOriginal | ConvertFrom-Json; $j.sourceStateSha256 = ('ab' * 32)
    [IO.File]::WriteAllText($receiptPath, ($j | ConvertTo-Json -Depth 12))
    $r = & $admit $package
    [IO.File]::WriteAllText($receiptPath, $receiptOriginal)
    Add-Case 'receipt-with-an-inconsistent-producer-fingerprint-refused' ($r.ExitCode -ne 0 -and $r.Output -match 'is not an exactly clean checkout of its commit') (Get-Reason $r.Output)
    $shippedForFingerprint = Join-Path $clone 'README.md'
    $shippedForFingerprintText = [IO.File]::ReadAllText($shippedForFingerprint)
    [IO.File]::WriteAllText($shippedForFingerprint, $shippedForFingerprintText + "`r`n<!-- modified while building -->`r`n")
    $dirtyFingerprint = [regex]::Match((Invoke-PS @('-Command', ". '$($harness.Replace("'", "''"))'; 'FINGERPRINT ' + (Get-KmgSourceStateFingerprint -RepositoryRoot '$($clone.Replace("'", "''"))')")).Output, 'FINGERPRINT ([0-9a-f]{64})').Groups[1].Value
    [IO.File]::WriteAllText($shippedForFingerprint, $shippedForFingerprintText)
    $j = $receiptOriginal | ConvertFrom-Json; $j.sourceStateSha256 = $dirtyFingerprint
    [IO.File]::WriteAllText($receiptPath, ($j | ConvertTo-Json -Depth 12))
    $r = & $admit $package
    [IO.File]::WriteAllText($receiptPath, $receiptOriginal)
    Add-Case 'package-from-a-modified-then-restored-shipped-input-refused' ($dirtyFingerprint -cmatch '^[0-9a-f]{64}$' -and $r.ExitCode -ne 0 -and $r.Output -match 'is not an exactly clean checkout of its commit') ("modified-state fingerprint $dirtyFingerprint; " + (Get-Reason $r.Output))
    $r = & $admit $package
    Add-Case 'clean-producer-receipt-admitted' ($r.ExitCode -eq 0 -and $r.Output -match 'ADMITTED [0-9a-f]{64}' -and ($receiptOriginal | ConvertFrom-Json).branch -ceq '') "exit $($r.ExitCode); receipt branch '$(($receiptOriginal | ConvertFrom-Json).branch)' (detached)"

    # The receipt's other recorded identities: a different packaged-DLL digest or source stamp (the primary DLL fields
    # unchanged) is refused.
    foreach ($edit in @(@('packaged-dll-hash', 'packagedDllSha256', ('cd' * 32), 'not the packaged DLL its receipt records'), @('source-stamp', 'embeddedGitCommit', ('0' * 40), 'recorded source stamp 0{40} is not the packaged DLL'))) {
        $j = $receiptOriginal | ConvertFrom-Json; $j.($edit[1]) = $edit[2]
        [IO.File]::WriteAllText($receiptPath, ($j | ConvertTo-Json -Depth 12))
        $r = & $admit $package
        Add-Case "receipt-with-another-$($edit[0])-refused" ($r.ExitCode -ne 0 -and $r.Output -match $edit[3]) (Get-Reason $r.Output)
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
    $preflight = { param([string]$zip) Invoke-PS @('-Command', ". '$((Join-Path $lc 'LoaderCompat.Common.ps1').Replace("'", "''"))'; . '$((Join-Path $lc 'LoaderCompat.Transaction.ps1').Replace("'", "''"))'; `$a = Assert-LcSessionInputs '$($zip.Replace("'", "''"))' '$($harnessDir.Replace("'", "''"))' '$purpose'; 'PREFLIGHT ' + `$a.packageSha256; Remove-LcAdmittedCopy `$a") }
    $pf = & $preflight $package
    Add-Case 'loader-preflight-admits-matching-candidate' ($pf.ExitCode -eq 0 -and $pf.Output -match 'PREFLIGHT [0-9a-f]{64}') "exit $($pf.ExitCode)"
    # The admitted identity the session carries (mode, version, producer commit) and a verified private staging tree; a
    # session that stops before staging (here, the preflight refusing a running game) removes the private copy.
    $carryProbe = Join-Path $temp 'carry-probe.ps1'
    [IO.File]::WriteAllText($carryProbe, @'
param([string]$Lc, [string]$Package, [string]$HarnessDir, [string]$Purpose)
$ErrorActionPreference = 'Stop'
. (Join-Path $Lc 'LoaderCompat.Common.ps1')
$a = Assert-LcSessionInputs $Package $HarnessDir $Purpose
$dll = Join-Path ([string]$a.extractedModDirectory) 'KingmakerGunslinger.dll'
'CARRIED ' + $a.admissionMode + ' ' + $a.version + ' ' + $a.producerCommit + ' ' + ((Get-LcSha256 $dll) -ceq [string]$a.dllSha256)
$copyDirectory = Split-Path -Parent ([string]$a.privateCopy)
# The session's pre-staging step (Enter-LcSessionForAdmission) with a refusing preflight (a running game): nothing is
# acquired and the admitted copy is removed.
function Assert-LcPreflight { throw 'Kingmaker is running (fixture).' }
try { $null = Enter-LcSessionForAdmission -Admitted $a -SessionId 'fixture-prestaging'; 'PREFLIGHT-ACCEPTED' } catch { 'PREFLIGHT-REFUSED ' + $_.Exception.Message }
'COPY-REMAINS ' + (Test-Path -LiteralPath $copyDirectory)
'@)
    $cp = Invoke-PS @('-File', $carryProbe, '-Lc', $lc, '-Package', $package, '-HarnessDir', $harnessDir, '-Purpose', $purpose)
    Add-Case 'loader-admission-carries-the-admitted-identity-and-a-verified-staging-tree' ($cp.Output -match ('CARRIED development \S+ ' + $commit + ' True')) ([regex]::Match($cp.Output, 'CARRIED .{0,120}').Value)
    Add-Case 'loader-admitted-copy-removed-when-the-session-stops-before-staging' ($cp.Output -match 'PREFLIGHT-REFUSED Kingmaker is running' -and $cp.Output -match 'COPY-REMAINS False') ([regex]::Match($cp.Output, 'COPY-REMAINS \w+').Value)
    $pf = & $preflight $newer
    Add-Case 'loader-preflight-refuses-mismatched-zip' ($pf.ExitCode -ne 0 -and $pf.Output -match 'Candidate admission refused') (Get-Reason $pf.Output)

    # --- Package entries: one resolved path; a relative path after a PowerShell location change; a noncanonical entry
    #     whose destination falls beside the mod directory; a non-string packaged minimum.
    $relative = Invoke-PS @('-Command', "Set-Location -LiteralPath '$((Split-Path -Parent $package).Replace("'", "''"))'; & '$($validator.Replace("'", "''"))' -PackagePath '.\$((Split-Path -Leaf $package).Replace("'", "''"))'")
    Add-Case 'validator-relative-path-after-location-change-accepted' ($relative.ExitCode -eq 0 -and $relative.Output -match 'Strict standalone UMM package validation passed') "exit $($relative.ExitCode)"
    $beside = Join-Path $variants 'noncanonical-entry.zip'
    New-ZipVariant $package $beside @{} @{ 'KingmakerGunslinger/../KingmakerGunslinger-notes.md' = [Text.Encoding]::ASCII.GetBytes('notes') }
    $v = Invoke-PS @('-File', $validator, '-PackagePath', $beside)
    Add-Case 'validator-noncanonical-entry-beside-mod-directory-refused' ($v.ExitCode -ne 0 -and $v.Output -match 'outside the .KingmakerGunslinger. mod root: KingmakerGunslinger/\.\./KingmakerGunslinger-notes\.md') (Get-Reason $v.Output)
    $infoEntry = 'KingmakerGunslinger/Info.json'
    $infoArchive = [IO.Compression.ZipFile]::OpenRead($package)
    try { $infoReader = [IO.StreamReader]::new($infoArchive.GetEntry($infoEntry).Open()); try { $infoText = $infoReader.ReadToEnd() } finally { $infoReader.Dispose() } }
    finally { $infoArchive.Dispose() }
    $arrayInfo = $infoText.TrimStart([char]0xFEFF) | ConvertFrom-Json; $arrayInfo.ManagerVersion = @([string]$arrayInfo.ManagerVersion)
    $arrayZip = Join-Path $variants 'array-minimum.zip'
    New-ZipVariant $package $arrayZip @{ $infoEntry = [Text.Encoding]::UTF8.GetBytes(($arrayInfo | ConvertTo-Json -Depth 6)) }
    $v = Invoke-PS @('-File', $validator, '-PackagePath', $arrayZip)
    Add-Case 'validator-array-valued-packaged-minimum-refused' ($v.ExitCode -ne 0 -and $v.Output -match 'Packaged ManagerVersion must equal the pinned loader floor') (Get-Reason $v.Output)

    # --- The receipt records the exact compiler output, which equals the DLL inside the ZIP and carries the producer's
    #     stamp; another baseline-compatible build supplying both the loose and the packaged DLL is refused while the
    #     receipt names the original producer.
    $receiptNow = Get-Content -LiteralPath $receiptPath -Raw | ConvertFrom-Json
    $measuredGenuine = Invoke-PS @('-Command', "`$m = & '$($validator.Replace("'", "''"))' -PackagePath '$($package.Replace("'", "''"))' -PassThru | Select-Object -Last 1; 'MEASURED ' + `$m.sha256 + ' ' + `$m.mvid + ' ' + `$m.gitCommit")
    $mm = [regex]::Match($measuredGenuine.Output, 'MEASURED ([0-9a-f]{64}) ([0-9a-f-]{36}) ([0-9a-f]{40})')
    Add-Case 'receipt-records-matching-compiler-and-packaged-identities' ($mm.Success -and $receiptNow.compilerDllSha256 -ceq $mm.Groups[1].Value -and $receiptNow.dllSha256 -ceq $mm.Groups[1].Value -and
        $receiptNow.compilerDllMvid -ceq $mm.Groups[2].Value -and $receiptNow.embeddedGitCommit -ceq $commit -and $mm.Groups[3].Value -ceq $commit) "compiler $($receiptNow.compilerDllSha256) / packaged $($mm.Groups[1].Value) / stamp $($mm.Groups[3].Value)"
    $otherMvid = (Invoke-PS @('-Command', "[Reflection.Assembly]::ReflectionOnlyLoad([IO.File]::ReadAllBytes('$($OtherBuildDll.Replace("'", "''"))')).ManifestModule.ModuleVersionId.ToString()")).Output.Trim()
    $otherSha = Get-Sha $OtherBuildDll
    $packageBytes = [IO.File]::ReadAllBytes($package)
    $sidecar = "$package.sha256"; $sidecarText = if (Test-Path -LiteralPath $sidecar) { [IO.File]::ReadAllText($sidecar) } else { $null }
    $loose = Join-Path $clone 'artifacts\bin\Release\KingmakerGunslinger\KingmakerGunslinger.dll'; $looseBytes = if (Test-Path -LiteralPath $loose) { [IO.File]::ReadAllBytes($loose) } else { $null }
    $otherZip = Join-Path $variants 'other-build.zip'
    New-ZipVariant $package $otherZip @{ $dllEntry = [IO.File]::ReadAllBytes($OtherBuildDll) }
    try {
        [IO.File]::Copy($otherZip, $package, $true)
        if ($null -ne $looseBytes) { [IO.File]::WriteAllBytes($loose, [IO.File]::ReadAllBytes($OtherBuildDll)) }
        $forged = $receiptOriginal | ConvertFrom-Json
        $forged.packageSha256 = Get-Sha $package; $forged.dllSha256 = $otherSha; $forged.dllMvid = $otherMvid
        $forged.compilerDllSha256 = $otherSha; $forged.compilerDllMvid = $otherMvid
        [IO.File]::WriteAllText($receiptPath, ($forged | ConvertTo-Json -Depth 12))
        if ($null -ne $sidecarText) { [IO.File]::WriteAllText($sidecar, $sidecarText.Replace($forged.packageSha256, $forged.packageSha256).Replace(([regex]::Match($sidecarText, '[0-9a-fA-F]{64}').Value), $forged.packageSha256)) }
        $r = & $admit $package
        Add-Case 'other-baseline-build-with-original-producer-receipt-refused' ($r.ExitCode -ne 0 -and $r.Output -match "was built from ef0e1fe15d59fc6f04ef4956337690f14de3c528, not the receipt's producing commit") (Get-Reason $r.Output)
    }
    finally {
        [IO.File]::WriteAllBytes($package, $packageBytes); [IO.File]::WriteAllText($receiptPath, $receiptOriginal)
        if ($null -ne $sidecarText) { [IO.File]::WriteAllText($sidecar, $sidecarText) }
        if ($null -ne $looseBytes) { [IO.File]::WriteAllBytes($loose, $looseBytes) }
    }

    # --- Stale candidates: shipped inputs and packaging helpers outside the earlier path list; documentation under docs/
    #     only keeps the candidate admissible.
    $shippedDocument = [string]@((Get-Content -LiteralPath (Join-Path $clone 'scripts\package-documents.json') -Raw | ConvertFrom-Json).documents | Where-Object { $_ -like 'docs/*' })[0]
    $shippedPath = Join-Path $clone $shippedDocument.Replace('/', '\')
    $shippedOriginal = [IO.File]::ReadAllText($shippedPath)
    [IO.File]::WriteAllText($shippedPath, $shippedOriginal + "`r`n<!-- uncommitted shipped document change -->`r`n")
    $r = & $admit $package
    [IO.File]::WriteAllText($shippedPath, $shippedOriginal)
    Add-Case 'candidate-with-an-uncommitted-shipped-document-change-refused' ($r.ExitCode -ne 0 -and $r.Output -match ('candidate is stale: material inputs changed.*' + [regex]::Escape($shippedDocument))) (Get-Reason $r.Output)
    foreach ($material in @(@('shipped-readme', 'README.md', '<!-- shipped change -->'), @('packaging-helper', 'scripts/common.ps1', '# packaging helper change'), @('shipped-docs-document', $shippedDocument, '<!-- shipped document change -->'))) {
        $path = Join-Path $clone $material[1]
        $original = [IO.File]::ReadAllText($path)
        [IO.File]::WriteAllText($path, $original + "`r`n" + $material[2] + "`r`n")
        & $git @('commit', '-q', '-am', "test: $($material[0]) change") | Out-Null
        $r = & $admit $package
        Add-Case "candidate-after-$($material[0])-change-refused" ($r.ExitCode -ne 0 -and $r.Output -match 'candidate is stale: material inputs changed.*' + [regex]::Escape($material[1])) (Get-Reason $r.Output)
        [IO.File]::WriteAllText($path, $original)
        & $git @('commit', '-q', '-am', "test: revert $($material[0]) change") | Out-Null
    }
    $reviewNote = Join-Path $clone 'docs\agent-reviews\candidate-admission-note.md'
    [void][IO.Directory]::CreateDirectory((Split-Path -Parent $reviewNote)); [IO.File]::WriteAllText($reviewNote, "review documentation only`r`n")
    & $git @('add', 'docs/agent-reviews/candidate-admission-note.md') | Out-Null; & $git @('commit', '-q', '-m', 'test: review documentation only') | Out-Null
    $r = & $admit $package
    Add-Case 'candidate-after-review-documentation-change-admitted' ($r.ExitCode -eq 0 -and $r.Output -match 'ADMITTED [0-9a-f]{64}') "exit $($r.ExitCode)"

    # --- Historical release control: admitted only by the committed release record (an identified older release);
    #     a changed archive and a same-version development package are refused; conflicting modes are refused first.
    $historical = { param([string]$zip, [string]$version) Invoke-PS @('-Command', ". '$($harness.Replace("'", "''"))'; `$h = Read-KmgHistoricalReleasePackage -PackagePath '$($zip.Replace("'", "''"))' -RepositoryRoot '$($clone.Replace("'", "''"))' -Version '$version'; 'HISTORICAL ' + `$h.releaseTag + ' ' + `$h.commit + ' ' + `$h.packageSha256") }
    $h = & $historical $HistoricalReleaseZip '0.0.188'
    Add-Case 'historical-identified-older-release-admitted' ($h.ExitCode -eq 0 -and $h.Output -match 'HISTORICAL v0\.0\.188 097e522f9fa0a8fdc8a773fb51fc3cfbf91d115c 64d83955') "exit $($h.ExitCode)"
    $changedRelease = Join-Path $variants 'changed-release.zip'
    New-ZipVariant $HistoricalReleaseZip $changedRelease @{} @{ 'KingmakerGunslinger/NOTES.txt' = [Text.Encoding]::ASCII.GetBytes('changed') }
    $h = & $historical $changedRelease '0.0.188'
    Add-Case 'historical-changed-archive-refused' ($h.ExitCode -ne 0 -and $h.Output -match 'is not the recorded v0\.0\.188 release package') (Get-Reason $h.Output)
    $recordsPath = Join-Path $clone 'scripts\historical-release-records.json'
    $recordsOriginal = [IO.File]::ReadAllText($recordsPath)
    [IO.File]::WriteAllText($recordsPath, $recordsOriginal.Replace('"0.0.188"', '"0.0.188-edited"'))
    $h = & $historical $HistoricalReleaseZip '0.0.188'
    [IO.File]::WriteAllText($recordsPath, $recordsOriginal)
    Add-Case 'historical-uncommitted-record-edit-refused' ($h.ExitCode -ne 0 -and $h.Output -match 'working-tree historical release record differs from the committed record') (Get-Reason $h.Output)
    $h = & $historical $package '0.0.195'
    Add-Case 'historical-same-version-development-package-refused' ($h.ExitCode -ne 0 -and $h.Output -match 'is not the recorded v0\.0\.195 release package') (Get-Reason $h.Output)
    $deploy = Join-Path $clone 'scripts\Deploy-Local.ps1'
    $fixtureLive = Join-Path $temp 'not-the-live-mod'
    $d1 = Invoke-PS @('-File', $deploy, '-PackagePath', $package, '-LiveModDirectory', $fixtureLive, '-HistoricalReleaseControl', '-HistoricalReleaseVersion', '0.0.188', '-DevelopmentPurpose', $purpose, '-WhatIf')
    $d2 = Invoke-PS @('-File', $deploy, '-PackagePath', $package, '-LiveModDirectory', $fixtureLive, '-HistoricalReleaseControl', '-WhatIf')
    $pfConflict = Invoke-PS @('-Command', ". '$((Join-Path $lc 'LoaderCompat.Common.ps1').Replace("'", "''"))'; Assert-LcSessionInputs '$($package.Replace("'", "''"))' '$($harnessDir.Replace("'", "''"))' '$purpose' -HistoricalReleaseControl -HistoricalReleaseVersion 0.0.188")
    Add-Case 'historical-conflicting-mode-arguments-refused' ($d1.ExitCode -ne 0 -and $d1.Output -match 'mutually exclusive admission modes' -and $d2.ExitCode -ne 0 -and $d2.Output -match 'requires -HistoricalReleaseVersion' -and
        $pfConflict.ExitCode -ne 0 -and $pfConflict.Output -match 'mutually exclusive') "Deploy-Local: $(Get-Reason $d1.Output) | $(Get-Reason $d2.Output); preflight: $(Get-Reason $pfConflict.Output)"
    $pfHistorical = Invoke-PS @('-Command', ". '$((Join-Path $lc 'LoaderCompat.Common.ps1').Replace("'", "''"))'; `$a = Assert-LcSessionInputs '$($HistoricalReleaseZip.Replace("'", "''"))' '$($harnessDir.Replace("'", "''"))' '' -HistoricalReleaseControl -HistoricalReleaseVersion 0.0.188; 'PREFLIGHT ' + `$a.packageSha256 + ' ' + `$a.dllSha256; Remove-Item -LiteralPath (Split-Path -Parent `$a.privateCopy) -Recurse -Force")
    Add-Case 'loader-preflight-admits-recorded-historical-release' ($pfHistorical.ExitCode -eq 0 -and $pfHistorical.Output -match 'PREFLIGHT 64d83955[0-9a-f]{56} 0eb2f002') "exit $($pfHistorical.ExitCode)"

    # --- Deployment decisions on real admissions: the under-lease admission must be the first candidate; a historical
    #     release's expectations come from its digest-bound package and its record names the release tag.
    $deployProbe = Join-Path $temp 'deploy-probe.ps1'
    [IO.File]::WriteAllText($deployProbe, @'
param([string]$Harness, [string]$Clone, [string]$Package, [string]$Purpose, [string]$Historical, [string]$Work)
$ErrorActionPreference = 'Stop'
. $Harness
$dev = Read-KmgBuildLocalManifest -PackagePath $Package -RepositoryRoot $Clone -DevelopmentPurpose $Purpose -RequireCurrentMaterialInputs
$hist = Read-KmgHistoricalReleasePackage -PackagePath $Historical -RepositoryRoot $Clone -Version '0.0.188'
try { Assert-KmgSameAdmission -First $dev -Second $dev; 'SAME-ACCEPTED' } catch { 'SAME-REFUSED ' + $_.Exception.Message }
try { Assert-KmgSameAdmission -First $dev -Second $hist; 'CHANGED-ACCEPTED' } catch { 'CHANGED-REFUSED ' + $_.Exception.Message }
$changed = $dev | ConvertTo-Json -Depth 12 | ConvertFrom-Json; $changed.dllMvid = [Guid]::NewGuid().ToString()
try { Assert-KmgSameAdmission -First $dev -Second $changed; 'MVID-ACCEPTED' } catch { 'MVID-REFUSED ' + $_.Exception.Message }
# The historical release extracted from its admitted private copy, as Deploy-Local does.
$copy = New-KmgAdmittedPackageCopy -Admission $hist -Destination (Join-Path $Work 'hist-copy')
$staging = Join-Path $Work 'hist-staging'; [void][IO.Directory]::CreateDirectory($staging)
$mod = Expand-KmgAdmittedPackage -Admission $hist -AdmittedCopy $copy -Destination $staging
$fm = Get-KmgSha256 -Path (Join-Path $mod 'assets\soundbanks\firearm-soundbank-manifest.json')
$fb = Get-KmgSha256 -Path (Join-Path $mod 'assets\soundbanks\KMG_Firearms.bnk')
$e = Get-KmgDeploymentExpectations -Admission $hist -HistoricalRelease -PackagedFirearmManifestSha256 $fm -PackagedFirearmSoundBankSha256 $fb
'HISTORICAL-EXPECTATIONS branch=' + ($null -eq $e.branch) + ' sourceState=' + ($null -eq $e.sourceStateSha256) + ' available=' + $e.sourceStateAvailable + ' kind=' + $e.candidateKind + ' tag=' + $e.releaseTag + ' | ' + ($e.firearmManifestSha256 -ceq $fm) + ' ' + ($e.firearmSoundBankSha256 -ceq $fb)
'HISTORICAL-RECORD ' + ((New-KmgDeploymentRecordIdentity -Admission $hist -Expectations $e) | ConvertTo-Json -Compress)
try { Get-KmgDeploymentExpectations -Admission $hist -PackagedFirearmManifestSha256 $fm -PackagedFirearmSoundBankSha256 $fb | Out-Null; 'MODE-ACCEPTED' } catch { 'MODE-REFUSED ' + $_.Exception.Message }
$d = Get-KmgDeploymentExpectations -Admission $dev -PackagedFirearmManifestSha256 ([string]$dev.firearmManifestSha256) -PackagedFirearmSoundBankSha256 ([string]$dev.firearmSoundBankSha256)
'DEVELOPMENT-EXPECTATIONS ' + ($d.branch -ceq [string]$dev.branch) + ' ' + ($d.sourceStateSha256 -ceq [string]$dev.sourceStateSha256) + ' detached=' + ($dev.branch -ceq '')
$devRecord = New-KmgDeploymentRecordIdentity -Admission $dev -Expectations $d
'DETACHED-RECORD branch=[' + $devRecord.branch + '] sourceState=' + ($devRecord.sourceStateSha256 -ceq [string]$dev.sourceStateSha256) + ' commit=' + $devRecord.commit + ' available=' + $devRecord.sourceStateAvailable
$named = $dev | ConvertTo-Json -Depth 12 | ConvertFrom-Json; $named.branch = 'candidate-admission'
$n = Get-KmgDeploymentExpectations -Admission $named -PackagedFirearmManifestSha256 ([string]$dev.firearmManifestSha256) -PackagedFirearmSoundBankSha256 ([string]$dev.firearmSoundBankSha256)
'NAMED-RECORD branch=[' + (New-KmgDeploymentRecordIdentity -Admission $named -Expectations $n).branch + ']'
try { Get-KmgDeploymentExpectations -Admission $dev -PackagedFirearmManifestSha256 ('e' * 64) -PackagedFirearmSoundBankSha256 ([string]$dev.firearmSoundBankSha256) | Out-Null; 'AUDIO-ACCEPTED' } catch { 'AUDIO-REFUSED ' + $_.Exception.Message }
$noBranch = $dev | ConvertTo-Json -Depth 12 | ConvertFrom-Json; [void]$noBranch.PSObject.Properties.Remove('branch')
try { Get-KmgDeploymentExpectations -Admission $noBranch -PackagedFirearmManifestSha256 ([string]$dev.firearmManifestSha256) -PackagedFirearmSoundBankSha256 ([string]$dev.firearmSoundBankSha256) | Out-Null; 'FIELD-ACCEPTED' } catch { 'FIELD-REFUSED ' + $_.Exception.Message }
'@)
    $dp = Invoke-PS @('-File', $deployProbe, '-Harness', $harness, '-Clone', $clone, '-Package', $package, '-Purpose', $purpose, '-Historical', $HistoricalReleaseZip, '-Work', (Join-Path $temp 'deploy-work'))
    Add-Case 'deploy-under-lease-admission-must-be-the-first-candidate' ($dp.Output -match 'SAME-ACCEPTED' -and $dp.Output -match 'CHANGED-REFUSED The candidate changed between admission and the deployment lease \(packageSha256\)' -and $dp.Output -match 'MVID-REFUSED The candidate changed between admission and the deployment lease \(dllMvid\)') ([regex]::Matches($dp.Output, '(SAME|CHANGED|MVID)-\w+') -join ', ')
    Add-Case 'deploy-historical-release-expectations-from-its-package-and-tag' ($dp.Output -match 'HISTORICAL-EXPECTATIONS branch=True sourceState=True available=False kind=historical-release tag=v0\.0\.188 \| True True' -and $dp.Output -match 'HISTORICAL-RECORD \{[^}]*"branch":null,"sourceStateSha256":null,"sourceStateAvailable":false[^}]*"candidateKind":"historical-release","releaseTag":"v0\.0\.188"' -and $dp.Output -match 'MODE-REFUSED The deployment mode does not match the admission') ([regex]::Match($dp.Output, 'HISTORICAL-EXPECTATIONS [^|]+\| \w+ \w+').Value)
    Add-Case 'deploy-detached-receipt-expectations-and-record' ($dp.Output -match 'DEVELOPMENT-EXPECTATIONS True True detached=True' -and $dp.Output -match ('DETACHED-RECORD branch=\[\] sourceState=True commit=' + $commit + ' available=True') -and $dp.Output -match 'NAMED-RECORD branch=\[candidate-admission\]') ([regex]::Matches($dp.Output, '(DEVELOPMENT-EXPECTATIONS [^ ]+ [^ ]+ [^ ]+|DETACHED-RECORD [^ ]+|NAMED-RECORD [^ ]+)') -join ', ')
    Add-Case 'deploy-development-expectations-from-its-receipt' ($dp.Output -match 'AUDIO-REFUSED Extracted package firearm audio differs' -and $dp.Output -match 'FIELD-REFUSED A deployment-record field is missing for this admission \(branch\)') ([regex]::Matches($dp.Output, '(AUDIO-\w+|FIELD-\w+)') -join ', ')

    # --- The checked extractor: directory records carry no data, appear once, and never share a destination with a file.
    $extractProbe = Join-Path $temp 'extract-probe.ps1'
    [IO.File]::WriteAllText($extractProbe, @'
param([string]$Common, [string]$Zip, [string]$Work)
$ErrorActionPreference = 'Stop'
. $Common
$destination = Join-Path $Work ([Guid]::NewGuid().ToString('N')); [void][IO.Directory]::CreateDirectory($destination)
try { $m = Expand-KmgPackageEntries -ZipPath $Zip -Destination $destination -ModId 'KingmakerGunslinger'; 'EXTRACTED ' + (Split-Path -Leaf $m) } catch { 'REFUSED ' + $_.Exception.Message }
'@)
    $common = Join-Path $clone 'scripts\common.ps1'
    $extractWork = Join-Path $temp 'extract-work'; [void][IO.Directory]::CreateDirectory($extractWork)
    $emptyDirectory = [byte[]]@()
    $extractVariants = @(
        @('data-bearing-directory-record', @{ 'KingmakerGunslinger/notes/' = [Text.Encoding]::ASCII.GetBytes('hidden data') }, 'data-bearing directory record: KingmakerGunslinger/notes/'),
        @('duplicate-directory-record', $(& { $h = [Collections.Hashtable]::new([StringComparer]::Ordinal); $h['KingmakerGunslinger/notes/'] = $emptyDirectory; $h['KingmakerGunslinger/NOTES/'] = $emptyDirectory; $h }), 'duplicate directory record'),
        @('directory-record-at-a-file-destination', @{ 'KingmakerGunslinger/Info.json/' = $emptyDirectory }, 'directory record and a file at the same destination: KingmakerGunslinger/Info\.json'),
        @('file-that-is-the-parent-of-another-entry', @{ 'KingmakerGunslinger/Info.json/child.txt' = [Text.Encoding]::ASCII.GetBytes('child') }, 'file that is also the parent of other entries: KingmakerGunslinger/Info\.json'))
    foreach ($variantCase in $extractVariants) {
        $variantZip = Join-Path $variants ("extract-" + $variantCase[0] + '.zip')
        New-ZipVariant $package $variantZip @{} $variantCase[1]
        $x = Invoke-PS @('-File', $extractProbe, '-Common', $common, '-Zip', $variantZip, '-Work', $extractWork)
        Add-Case "extractor-$($variantCase[0])-refused" ($x.Output -match ('REFUSED Package contains a ' + $variantCase[2])) ([regex]::Match($x.Output, '(REFUSED|EXTRACTED) .{0,200}').Value)
    }
    $belowFileZip = Join-Path $variants 'directory-record-below-a-file.zip'
    New-ZipVariant $package $belowFileZip @{} @{ 'KingmakerGunslinger/Info.json/sub/' = $emptyDirectory }
    $x = Invoke-PS @('-File', $extractProbe, '-Common', $common, '-Zip', $belowFileZip, '-Work', $extractWork)
    $v = Invoke-PS @('-File', $validator, '-PackagePath', $belowFileZip)
    Add-Case 'extractor-and-validator-directory-record-below-a-file-refused' ($x.Output -match 'REFUSED Package contains a file that is also the parent of other entries: KingmakerGunslinger/Info\.json' -and $v.ExitCode -ne 0 -and $v.Output -match 'file that is also the parent of other entries') ("probe: " + [regex]::Match($x.Output, '(REFUSED|EXTRACTED) .{0,120}').Value + "; validator exit $($v.ExitCode)")
    $ordinaryDirZip = Join-Path $variants 'ordinary-directory-record.zip'
    New-ZipVariant $package $ordinaryDirZip @{} @{ 'KingmakerGunslinger/docs/' = $emptyDirectory }
    $v = Invoke-PS @('-File', $validator, '-PackagePath', $ordinaryDirZip)
    Add-Case 'validator-ordinary-directory-record-accepted' ($v.ExitCode -eq 0 -and $v.Output -match 'Strict standalone UMM package validation passed') "exit $($v.ExitCode)"
    $emptyDirZip = Join-Path $variants 'extract-empty-directory-record.zip'
    New-ZipVariant $package $emptyDirZip @{} @{ 'KingmakerGunslinger/notes/' = $emptyDirectory }
    $x = Invoke-PS @('-File', $extractProbe, '-Common', $common, '-Zip', $emptyDirZip, '-Work', $extractWork)
    Add-Case 'extractor-single-empty-directory-record-accepted' ($x.Output -match 'EXTRACTED KingmakerGunslinger') ([regex]::Match($x.Output, '(REFUSED|EXTRACTED) .{0,200}').Value)

    # --- Staging carries the admitted identity: bytes replacing the supplied path after admission are never staged
    #     (refused before the private copy, or the original bytes staged from the copy).
    $stagingProbe = Join-Path $temp 'staging-probe.ps1'
    [IO.File]::WriteAllText($stagingProbe, @'
param([string]$Harness, [string]$Clone, [string]$Package, [string]$Purpose, [string]$Replacement, [string]$Work, [string]$Lc, [string]$HarnessDir)
$ErrorActionPreference = 'Stop'
. $Harness
$original = [IO.File]::ReadAllBytes($Package)
try {
    $m = Read-KmgBuildLocalManifest -PackagePath $Package -RepositoryRoot $Clone -DevelopmentPurpose $Purpose -RequireCurrentMaterialInputs
    [IO.File]::Copy($Replacement, $Package, $true)
    try { New-KmgAdmittedPackageCopy -Admission $m -Destination (Join-Path $Work 'copy-after-change') | Out-Null; 'BEFORE-COPY STAGED' } catch { 'BEFORE-COPY REFUSED ' + $_.Exception.Message }
    [IO.File]::WriteAllBytes($Package, $original)
    $m = Read-KmgBuildLocalManifest -PackagePath $Package -RepositoryRoot $Clone -DevelopmentPurpose $Purpose -RequireCurrentMaterialInputs
    $copy = New-KmgAdmittedPackageCopy -Admission $m -Destination (Join-Path $Work 'copy')
    [IO.File]::Copy($Replacement, $Package, $true)
    $staging = Join-Path $Work 'staging'; [void][IO.Directory]::CreateDirectory($staging)
    $mod = Expand-KmgAdmittedPackage -Admission $m -AdmittedCopy $copy -Destination $staging
    'AFTER-COPY STAGED ' + (Get-KmgSha256 -Path (Join-Path $mod 'KingmakerGunslinger.dll')) + ' ADMITTED ' + $m.dllSha256
    # The loader harness: the admitted private copy stages even though the supplied path now holds other bytes.
    [IO.File]::WriteAllBytes($Package, $original)
    . (Join-Path $Lc 'LoaderCompat.Common.ps1')
    $admitted = Assert-LcSessionInputs $Package $HarnessDir $Purpose
    [IO.File]::Copy($Replacement, $Package, $true)
    $paths = [pscustomobject]@{ ModsDir = (Join-Path $Work 'lc-mods'); Pristine = (Join-Path $Work 'lc-pristine') }
    [void][IO.Directory]::CreateDirectory((Join-Path $paths.Pristine 'Mods'))
    $staged = Set-LcGameCopyMods $paths @() $admitted $HarnessDir
    'LOADER STAGED ' + $staged['KingmakerGunslinger'].dllSha256 + ' ADMITTED ' + $admitted.dllSha256
}
finally { [IO.File]::WriteAllBytes($Package, $original) }
'@)
    $packageShaBeforeStaging = Get-Sha $package
    $sp = Invoke-PS @('-File', $stagingProbe, '-Harness', $harness, '-Clone', $clone, '-Package', $package, '-Purpose', $purpose, '-Replacement', $newer, '-Work', (Join-Path $temp 'staging-work'), '-Lc', $lc, '-HarnessDir', $harnessDir)
    $after = [regex]::Match($sp.Output, 'AFTER-COPY STAGED ([0-9a-f]{64}) ADMITTED ([0-9a-f]{64})')
    $loaderStaged = [regex]::Match($sp.Output, 'LOADER STAGED ([0-9a-f]{64}) ADMITTED ([0-9a-f]{64})')
    Add-Case 'staging-refuses-a-package-replaced-before-the-private-copy' ($sp.Output -match 'BEFORE-COPY REFUSED The package changed after admission') (Get-Reason $sp.Output)
    Add-Case 'staging-uses-only-the-admitted-bytes-after-the-supplied-path-changes' ($after.Success -and $after.Groups[1].Value -ceq $after.Groups[2].Value -and $loaderStaged.Success -and $loaderStaged.Groups[1].Value -ceq $loaderStaged.Groups[2].Value -and $sp.ExitCode -eq 0 -and (Get-Sha $package) -ceq $packageShaBeforeStaging) "deploy staging $($after.Groups[1].Value); loader staging $($loaderStaged.Groups[1].Value)"

    # --- Staging admission verifies the producer claim: a baseline-compatible MSBuild-route compile of this commit (no
    #     source stamp) under a receipt in the earlier format (without the stamp field) is refused, whose digests match.
    $looseDll = Join-Path $clone 'artifacts\bin\Release\KingmakerGunslinger\KingmakerGunslinger.dll'
    $looseBeforeUnstamped = if (Test-Path -LiteralPath $looseDll) { [IO.File]::ReadAllBytes($looseDll) } else { $null }
    $unstampedBuild = Invoke-PS @('-File', (Join-Path $clone 'scripts\build.ps1'), '-Configuration', 'Release', '-CompileOnly')
    $unstampedDll = Join-Path $variants 'unstamped.dll'
    [IO.File]::WriteAllBytes($unstampedDll, [IO.File]::ReadAllBytes($looseDll))
    if ($null -ne $looseBeforeUnstamped) { [IO.File]::WriteAllBytes($looseDll, $looseBeforeUnstamped) }
    $unstampedMvid = (Invoke-PS @('-Command', "[Reflection.Assembly]::ReflectionOnlyLoad([IO.File]::ReadAllBytes('$($unstampedDll.Replace("'", "''"))')).ManifestModule.ModuleVersionId.ToString()")).Output.Trim()
    $unstampedStamp = (Invoke-PS @('-Command', ". '$($harness.Replace("'", "''"))'; (Get-KmgPackagedDllIdentity -DllPath '$($unstampedDll.Replace("'", "''"))').gitCommit")).Output.Trim()
    $unstampedZip = Join-Path $variants 'unstamped-build.zip'
    New-ZipVariant $package $unstampedZip @{ $dllEntry = [IO.File]::ReadAllBytes($unstampedDll) }
    $packageBytes2 = [IO.File]::ReadAllBytes($package); $receipt2 = [IO.File]::ReadAllText($receiptPath)
    $sidecar2 = "$package.sha256"; $sidecarText2 = if (Test-Path -LiteralPath $sidecar2) { [IO.File]::ReadAllText($sidecar2) } else { $null }
    try {
        [IO.File]::Copy($unstampedZip, $package, $true)
        $earlier = $receipt2 | ConvertFrom-Json
        foreach ($field in @('compilerDllSha256', 'compilerDllMvid', 'packagedDllSha256', 'embeddedGitCommit')) { [void]$earlier.PSObject.Properties.Remove($field) }
        $earlier.packageSha256 = Get-Sha $package; $earlier.dllSha256 = Get-Sha $unstampedDll; $earlier.dllMvid = $unstampedMvid
        [IO.File]::WriteAllText($receiptPath, ($earlier | ConvertTo-Json -Depth 12))
        if ($null -ne $sidecarText2) { [IO.File]::WriteAllText($sidecar2, $sidecarText2.Replace(([regex]::Match($sidecarText2, '[0-9a-fA-F]{64}').Value), $earlier.packageSha256)) }
        $r = & $admit $package
        Add-Case 'unstamped-compile-under-an-earlier-format-receipt-refused-for-staging' ($unstampedBuild.ExitCode -eq 0 -and -not $unstampedStamp -and $r.ExitCode -ne 0 -and $r.Output -match 'carries no source stamp, so its producing commit') ("build exit $($unstampedBuild.ExitCode); stamp '$unstampedStamp'; " + (Get-Reason $r.Output))
    }
    finally {
        [IO.File]::WriteAllBytes($package, $packageBytes2); [IO.File]::WriteAllText($receiptPath, $receipt2)
        if ($null -ne $sidecarText2) { [IO.File]::WriteAllText($sidecar2, $sidecarText2) }
    }
    $r = & $admit $package
    Add-Case 'candidate-admitted-again-after-the-unstamped-case' ($r.ExitCode -eq 0 -and $r.Output -match 'ADMITTED [0-9a-f]{64}') "exit $($r.ExitCode)"

    # --- Loader staging verifies the whole admitted tree before Mods changes (R2-F3). A disposable target whose Mods
    #     holds a sentinel: a changed non-DLL file, and separately a removed Info.json, in the extracted tree after
    #     admission are refused with the sentinel and the supplied ZIP intact; the genuine tree then stages completely.
    $treeProbe = Join-Path $temp 'tree-probe.ps1'
    [IO.File]::WriteAllText($treeProbe, @'
param([string]$Lc, [string]$Package, [string]$HarnessDir, [string]$Purpose, [string]$Work)
$ErrorActionPreference = 'Stop'
. (Join-Path $Lc 'LoaderCompat.Common.ps1')
$paths = [pscustomobject]@{ ModsDir = (Join-Path $Work 'copy\Mods'); Pristine = (Join-Path $Work 'pristine') }
[void][IO.Directory]::CreateDirectory($paths.ModsDir); [void][IO.Directory]::CreateDirectory((Join-Path $paths.Pristine 'Mods'))
[IO.File]::WriteAllText((Join-Path $paths.ModsDir 'sentinel.txt'), 'previous Mods')
$zipBefore = Get-LcSha256 $Package
function Show-State([string]$Label) {
    $label + ' sentinel=' + (Test-Path -LiteralPath (Join-Path $paths.ModsDir 'sentinel.txt')) + ' modsEntries=' + @(Get-ChildItem -LiteralPath $paths.ModsDir -Force).Count +
        ' zipIntact=' + ((Get-LcSha256 $Package) -ceq $zipBefore) + ' leftovers=' + @(Get-ChildItem -LiteralPath (Split-Path $paths.ModsDir) -Directory -Filter 'Mods.kmg-*').Count
}
$a = Assert-LcSessionInputs $Package $HarnessDir $Purpose
[IO.File]::AppendAllText((Join-Path $a.extractedModDirectory 'README.md'), "`nchanged after admission`n")
try { $null = Set-LcGameCopyMods $paths @() $a $HarnessDir; 'CHANGED-FILE STAGED' } catch { 'CHANGED-FILE REFUSED ' + $_.Exception.Message }
Show-State 'AFTER-CHANGED-FILE'
$a = Assert-LcSessionInputs $Package $HarnessDir $Purpose
Remove-Item -LiteralPath (Join-Path $a.extractedModDirectory 'Info.json')
try { $null = Set-LcGameCopyMods $paths @() $a $HarnessDir; 'REMOVED-INFO STAGED' } catch { 'REMOVED-INFO REFUSED ' + $_.Exception.Message }
Show-State 'AFTER-REMOVED-INFO'
$a = Assert-LcSessionInputs $Package $HarnessDir $Purpose
$expected = (Get-LcArchiveTreeManifest ([string]$a.privateCopy) 'KingmakerGunslinger') -join "`n"
$staged = Set-LcGameCopyMods $paths @() $a $HarnessDir
'GENUINE STAGED tree=' + ((Get-LcTreeManifest (Join-Path $paths.ModsDir 'KingmakerGunslinger')) -join "`n" -ceq $expected) + ' dll=' + ($staged['KingmakerGunslinger'].dllSha256 -ceq [string]$a.dllSha256) + ' harness=' + (Test-Path -LiteralPath (Join-Path $paths.ModsDir 'KmgLoaderCompatHarness\Info.json'))
Show-State 'AFTER-GENUINE'
'@)
    $tp = Invoke-PS @('-File', $treeProbe, '-Lc', $lc, '-Package', $package, '-HarnessDir', $harnessDir, '-Purpose', $purpose, '-Work', (Join-Path $temp 'tree-work'))
    Add-Case 'loader-staging-refuses-a-changed-non-dll-file-with-the-previous-mods-intact' ($tp.Output -match 'CHANGED-FILE REFUSED The extracted candidate tree is not the admitted archive' -and $tp.Output -match 'AFTER-CHANGED-FILE sentinel=True modsEntries=1 zipIntact=True leftovers=0') ([regex]::Match($tp.Output, 'AFTER-CHANGED-FILE [^A]+').Value)
    Add-Case 'loader-staging-refuses-a-removed-info-json-with-the-previous-mods-intact' ($tp.Output -match 'REMOVED-INFO REFUSED The extracted candidate tree is not the admitted archive' -and $tp.Output -match 'AFTER-REMOVED-INFO sentinel=True modsEntries=1 zipIntact=True leftovers=0') ([regex]::Match($tp.Output, 'AFTER-REMOVED-INFO [^A]+').Value)
    Add-Case 'loader-staging-stages-the-genuine-tree-completely' ($tp.ExitCode -eq 0 -and $tp.Output -match 'GENUINE STAGED tree=True dll=True harness=True' -and $tp.Output -match 'AFTER-GENUINE sentinel=False modsEntries=2 zipIntact=True leftovers=0') (([regex]::Match($tp.Output, 'GENUINE STAGED [^A]+').Value) + ' ' + [regex]::Match($tp.Output, 'AFTER-GENUINE .*').Value)

    # --- The admission mode for each route (R4-F3): the child result for the development and historical routes, the
    #     mode function for all three (an ordinary Build-Local release receipt cannot be produced on this development
    #     branch), and the report projection of offline summaries for the three modes and an older summary.
    $modeProbe = Join-Path $temp 'mode-probe.ps1'
    [IO.File]::WriteAllText($modeProbe, @'
param([string]$Lc, [string]$Package, [string]$Historical, [string]$HarnessDir, [string]$Purpose)
$ErrorActionPreference = 'Stop'
. (Join-Path $Lc 'LoaderCompat.Common.ps1')
. (Join-Path (Split-Path (Split-Path $Lc)) 'RuntimeHarness.Common.ps1')
$d = Assert-LcSessionInputs $Package $HarnessDir $Purpose; 'CHILD development=' + $d.admissionMode + ' tag=[' + $d.releaseTag + ']'; Remove-LcAdmittedCopy $d
$h = Assert-LcSessionInputs $Historical $HarnessDir '' -HistoricalReleaseControl -HistoricalReleaseVersion 0.0.188; 'CHILD historical=' + $h.admissionMode + ' tag=[' + $h.releaseTag + ']'; Remove-LcAdmittedCopy $h
'MODES ' + (Get-KmgAdmissionMode -DevelopmentPurpose $Purpose) + ' ' + (Get-KmgAdmissionMode -HistoricalReleaseVersion '0.0.188') + ' ' + (Get-KmgAdmissionMode)
foreach ($mode in @('build-local', 'development', 'historical-release')) {
    $summary = ('{"sessionId":"x","admission":{"mode":"' + $mode + '","version":"0.0.195"}}') | ConvertFrom-Json
    'REPORT ' + $mode + ' -> ' + (Get-LcReportHead $summary).admission.mode
}
'REPORT older -> ' + (Get-LcReportHead ('{"sessionId":"x"}' | ConvertFrom-Json)).admission.mode
'@)
    $mp = Invoke-PS @('-File', $modeProbe, '-Lc', $lc, '-Package', $package, '-Historical', $HistoricalReleaseZip, '-HarnessDir', $harnessDir, '-Purpose', $purpose)
    Add-Case 'admission-mode-recorded-for-each-route-and-projected-by-the-report' ($mp.Output -match 'CHILD development=development tag=\[\]' -and $mp.Output -match 'CHILD historical=historical-release tag=\[v0\.0\.188\]' -and $mp.Output -match 'MODES development historical-release build-local' -and
        $mp.Output -match 'REPORT build-local -> build-local' -and $mp.Output -match 'REPORT development -> development' -and $mp.Output -match 'REPORT historical-release -> historical-release' -and $mp.Output -match 'REPORT older -> not recorded') ([regex]::Matches($mp.Output, '(CHILD \S+ \S+|MODES [a-z\- ]+|REPORT \S+ -> \S+)') -join '; ')

    # --- Package measurement is an opt-in structured return (R4-F4): with it, the object's digest is the final ZIP's and
    #     its checksum file's; without it, the default output is still the package path.
    $packageScript = Join-Path $clone 'scripts\package.ps1'
    $measuredZip = Join-Path $variants 'measured-package.zip'
    $pm = Invoke-PS @('-Command', "`$o = @(& '$($packageScript.Replace("'", "''"))' -Configuration Release -PackagePath '$($measuredZip.Replace("'", "''"))' -PassThruMeasurement) | Select-Object -Last 1; 'MEASUREMENT ' + `$o.packageSha256 + ' ' + `$o.dll.sha256 + ' ' + (`$o.packagePath -ceq '$($measuredZip.Replace("'", "''"))')")
    $pmMatch = [regex]::Match($pm.Output, 'MEASUREMENT ([0-9a-f]{64}) ([0-9a-f]{64}) True')
    $checksumText = if (Test-Path -LiteralPath "$measuredZip.sha256") { [IO.File]::ReadAllText("$measuredZip.sha256") } else { '' }
    $defaultZip = Join-Path $variants 'default-package.zip'
    $pd = Invoke-PS @('-Command', "`$o = @(& '$($packageScript.Replace("'", "''"))' -Configuration Release -PackagePath '$($defaultZip.Replace("'", "''"))') | Select-Object -Last 1; 'DEFAULT-OUTPUT ' + (`$o -is [string]) + ' ' + (`$o -ceq '$($defaultZip.Replace("'", "''"))')")
    Add-Case 'package-measurement-return-leaves-the-zip-equal-to-its-checksum' ($pm.ExitCode -eq 0 -and $pmMatch.Success -and $pmMatch.Groups[1].Value -ceq (Get-Sha $measuredZip) -and $checksumText -match ('^' + $pmMatch.Groups[1].Value) -and $pd.Output -match 'DEFAULT-OUTPUT True True') ("measured $($pmMatch.Groups[1].Value); file $(Get-Sha $measuredZip); checksum file '$($checksumText.Trim())'; " + [regex]::Match($pd.Output, 'DEFAULT-OUTPUT \w+ \w+').Value)

    # --- The development purpose is data: an apostrophe gets the ordinary purpose-validation error before any child runs.
    $badPurpose = "umm-0311-admission'"
    $pfBad = Invoke-PS @('-Command', ". '$((Join-Path $lc 'LoaderCompat.Common.ps1').Replace("'", "''"))'; Assert-LcSessionInputs '$($package.Replace("'", "''"))' '$($harnessDir.Replace("'", "''"))' '$($badPurpose.Replace("'", "''"))'")
    Add-Case 'loader-preflight-purpose-with-apostrophe-refused-by-purpose-validation' ($pfBad.ExitCode -ne 0 -and $pfBad.Output -match 'Development purpose must be a lower-case hyphenated name' -and $pfBad.Output -notmatch 'Candidate admission refused') (Get-Reason $pfBad.Output)

    $failed = @($cases | Where-Object { -not $_.passed })
    $summary = [ordered]@{ schema = 'kmg.candidate-admission-tests.v1'; commit = $commit; purpose = $purpose; package_sha256 = Get-Sha $package
        newer_compiled_dll_sha256 = Get-Sha $NewerCompiledDll; historical_release_zip_sha256 = Get-Sha $HistoricalReleaseZip; other_build_dll_sha256 = Get-Sha $OtherBuildDll; passed = $cases.Count - $failed.Count; total = $cases.Count; cases = @($cases) }
    if ($ReportPath) { $summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $ReportPath -Encoding UTF8 }
    Write-Host ("RESULT {0}/{1} candidate admission checks passed" -f $summary.passed, $summary.total)
    if ($failed.Count -gt 0) { throw "$($failed.Count) candidate admission check(s) failed." }
}
finally {
    # The owned temporary clone is removed; a failure to remove it is surfaced.
    if (Test-Path -LiteralPath $temp) {
        try { Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction Stop }
        catch { Write-Warning "The temporary clone could not be removed: $temp ($($_.Exception.Message))" }
        if (Test-Path -LiteralPath $temp) { throw "The temporary clone remains: $temp" }
    }
}
