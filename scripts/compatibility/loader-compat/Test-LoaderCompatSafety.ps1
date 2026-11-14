[CmdletBinding()]
param()
# Offline checks of the loader-compatibility session guards. Temporary fixture
# directories only; no game, loader, registry or owner file is touched.
. (Join-Path $PSScriptRoot 'LoaderCompat.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
$checks = 0
function Assert-That([bool]$Condition, [string]$Name) { $script:checks++; if (-not $Condition) { $failures.Add($Name) } }
function Get-Refusal([scriptblock]$Action) { try { & $Action; return $null } catch { return $_.Exception.Message } }

$fixture = Join-Path ([IO.Path]::GetTempPath()) ('KmgLcSafety-' + [Guid]::NewGuid().ToString('N'))
$savedRoot = $script:LcCopiesRoot
try {
    $owner = Join-Path $fixture 'owner'
    $copies = Join-Path $fixture 'game-copies'
    foreach ($dir in @($owner, (Join-Path $copies 'good'), (Join-Path $copies 'noexe'), (Join-Path $fixture 'elsewhere'))) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    foreach ($exe in @((Join-Path $owner 'Kingmaker.exe'), (Join-Path $copies 'good\Kingmaker.exe'), (Join-Path $fixture 'elsewhere\Kingmaker.exe'))) { 'x' | Set-Content $exe }
    $script:LcCopiesRoot = $copies

    Assert-That ((Assert-LcGameCopy (Join-Path $copies 'good') $owner) -eq (Join-Path $copies 'good')) 'copy-under-root-accepted'
    Assert-That ((Get-Refusal { Assert-LcGameCopy $owner $owner }) -like "*owner's installation*") 'owner-install-refused'
    Assert-That ((Get-Refusal { Assert-LcGameCopy (Join-Path $owner 'Mods') $owner }) -like "*owner's installation*") 'path-inside-owner-install-refused'
    Assert-That ((Get-Refusal { Assert-LcGameCopy (Join-Path $fixture 'elsewhere') $owner }) -like '*must live under*') 'copy-outside-root-refused'
    Assert-That ((Get-Refusal { Assert-LcGameCopy (Join-Path $copies 'good\..\..\elsewhere') $owner }) -like '*must live under*') 'traversal-outside-root-refused'
    Assert-That ((Get-Refusal { Assert-LcGameCopy (Join-Path $copies 'noexe') $owner }) -like '*lacks Kingmaker.exe*') 'copy-without-game-refused'

    $paths = Get-LcCopyPaths (Join-Path $copies 'good')
    New-Item -ItemType Directory -Force -Path (Join-Path $paths.Pristine 'Mods\KingmakerGunslinger') | Out-Null
    Assert-That ((Get-Refusal { Set-LcGameCopyMods $paths @('KingmakerGunslinger') 'none.zip' $fixture }) -like '*candidate ZIP*') 'owner-copy-of-product-refused'
    Assert-That ((Get-Refusal { Set-LcGameCopyMods $paths @('NotInstalled') 'none.zip' $fixture }) -like '*not found in the pristine set*') 'unknown-owner-mod-refused'

    $before = @('a|1|x', 'b|2|y')
    Assert-That ((Compare-LcManifest $before $before).Count -eq 0) 'unchanged-manifest-equal'
    Assert-That ((Compare-LcManifest $before @('a|1|x', 'b|2|z')).Count -eq 2) 'changed-file-detected'
    Assert-That ((Compare-LcManifest $before @('a|1|x')).Count -eq 1) 'removed-file-detected'
    Assert-That ((Compare-LcManifest $before @('a|1|x', 'b|2|y', 'c|3|w')).Count -eq 1) 'new-file-detected'

    $notCopy = Get-Process -Id $PID
    Assert-That ((Get-Refusal { Stop-LcOwnedProcess $notCopy $paths }) -like '*not the game copy*') 'foreign-process-never-stopped'
}
finally {
    $script:LcCopiesRoot = $savedRoot
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
if ($checks -lt 13) { throw "Loader-compatibility safety checks did not run ($checks)." }
if ($failures.Count) { throw "Loader-compatibility safety checks failed: $($failures -join ', ')" }
Write-Host "Loader-compatibility safety checks: PASS=$checks FAIL=0. Offline fixtures only."
