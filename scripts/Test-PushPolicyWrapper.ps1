<#
.SYNOPSIS
Isolated behaviour tests for the real push-policy wrapper.

.DESCRIPTION
The wrapper at C:/Dev/KingmakerGunslingerLab/codex-policy lives outside this
repository, so it has never had a behaviour test. Keyword presence is not a
behaviour test: these cases run the ACTUAL script against a controlled fake
Git executable placed first on PATH, capture every requested argv, and assert
both the accept/refuse decision and the exact push refspec.

Nothing here contacts a remote, performs a push, or touches the real
repository. The fake Git never reaches the network; every answer is scripted.
If the wrapper is absent the script reports SKIP rather than inventing a pass.

Run: powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-PushPolicyWrapper.ps1
#>
[CmdletBinding()]
param(
    [string]$PolicyPath =
        'C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1',
    # The permitted root is the real Sprint 25 worktree, exercised through the
    # wrapper's existing lab-boundary authorization rather than by widening it.
    [string]$PermittedRoot = 'C:/Dev/KingmakerGunslingerLab/wt/s25',
    [string]$LabRoot = 'C:/Dev/KingmakerGunslingerLab'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $PolicyPath)) {
    Write-Host "Push policy wrapper SKIP: not present at $PolicyPath"
    exit 0
}

$policyFull = [IO.Path]::GetFullPath($PolicyPath)
$permittedFull = [IO.Path]::GetFullPath($PermittedRoot)
$labFull = [IO.Path]::GetFullPath($LabRoot).TrimEnd('\')
$reviewedHash = (Get-FileHash -LiteralPath $policyFull -Algorithm SHA256).Hash.ToLowerInvariant()

$errors = $null
$tokens = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile(
    $policyFull, [ref]$tokens, [ref]$errors)
if ($errors.Count -ne 0) {
    throw "Push policy wrapper does not parse: $($errors.Count) error(s)."
}

# The wrapper's own allow-list, read out of the reviewed source.
$allowed = @()
$lines = [IO.File]::ReadAllLines($policyFull)
$inList = $false
foreach ($line in $lines) {
    if ($line -match '^\$AllowedBranches\s*=\s*@\(') { $inList = $true; continue }
    if ($inList) {
        if ($line -match '^\)') { break }
        if ($line -match "^\s*'(.+)'\s*$") { $allowed += $Matches[1] }
    }
}
if ($allowed.Count -lt 2) { throw 'Could not read the wrapper allow-list.' }

$sprint25 = @('codex/expanded-summoning-sprint25-brachiosaurus-elephant',
              'codex/expanded-summoning-sprint25-release-records')
foreach ($name in $sprint25) {
    if ($allowed -notcontains $name) {
        throw "Intended Sprint 25 branch missing from the allow-list: $name"
    }
}
$wildcards = @($allowed | Where-Object { $_ -match '[\*\?\[\]]' })
if ($wildcards.Count -ne 0) {
    throw "Allow-list contains a pattern entry: $($wildcards -join ', ')"
}

$root = Join-Path ([IO.Path]::GetTempPath()) ('kmg-push-audit-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
$argvLog = Join-Path $root 'argv.log'
$scenarioPath = Join-Path $root 'scenario.json'

# The fake Git. It records every argv and answers from the scenario file.
#
# Deliberately no param block: `git cat-file -e ...` would otherwise bind -e as
# an ambiguous PowerShell parameter name instead of a remaining argument. The
# cmd shim also eats the caret in `<sha>^{commit}`, which is harmless because
# no case inspects that argument; the push refspec and @{u} pass through intact,
# and those are what the assertions read.
$responder = Join-Path $root 'fake-git.ps1'
@'
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSCommandPath
$scenario = Get-Content -Raw -LiteralPath (Join-Path $root 'scenario.json') | ConvertFrom-Json
Add-Content -LiteralPath (Join-Path $root 'argv.log') -Value ($args -join ' ')
# Strip a leading -C <dir>.
$args2 = @($args)
if ($args2.Count -ge 2 -and $args2[0] -eq '-C') { $args2 = $args2[2..($args2.Count - 1)] }
$verb = if ($args2.Count -gt 0) { $args2[0] } else { '' }
$rest = if ($args2.Count -gt 1) { $args2[1..($args2.Count - 1)] } else { @() }
function Emit($text) { if ($null -ne $text -and $text -ne '') { Write-Output $text } }
switch ($verb) {
    'rev-parse' {
        if ($rest -contains '--show-toplevel') {
            if ($scenario.toplevelExit -ne 0) { Write-Output 'fatal: not a git repository'; exit $scenario.toplevelExit }
            Emit $scenario.toplevel; exit 0
        }
        if ($rest -contains '@{u}') {
            if ($scenario.upstreamExit -ne 0) { Write-Output 'fatal: no upstream'; exit $scenario.upstreamExit }
            Emit $scenario.upstream; exit 0
        }
        Emit $scenario.head; exit 0
    }
    'branch' {
        if ($scenario.branchExit -ne 0) { Write-Output 'fatal: branch lookup failed'; exit $scenario.branchExit }
        # Real `git branch --show-current` prints an empty line on a detached
        # HEAD rather than nothing, so emit unconditionally to stay faithful.
        Write-Output $scenario.branch; exit 0
    }
    'remote' { Emit $scenario.origin; exit 0 }
    'cat-file' { exit $scenario.catFileExit }
    'merge-base' { exit $scenario.ancestorExit }
    'status' { foreach ($row in $scenario.status) { Emit $row }; exit 0 }
    'push' {
        Write-Output 'remote: progress on stderr-like channel'
        if ($scenario.pushExit -ne 0) { Write-Output 'error: failed to push some refs'; exit $scenario.pushExit }
        exit 0
    }
    default { Write-Output ("fake-git: unexpected verb '" + $verb + "'"); exit 99 }
}
'@ | Set-Content -LiteralPath $responder -Encoding UTF8

# A .bat shim so the wrapper's bare `git` resolves to the fake via PATHEXT.
$shim = Join-Path $root 'git.bat'
@"
@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$responder" %*
"@ | Set-Content -LiteralPath $shim -Encoding ASCII

$head = '1111111111111111111111111111111111111111'
function New-Scenario {
    param([hashtable]$Overrides)
    $scenario = [ordered]@{
        toplevel = $permittedFull
        toplevelExit = 0
        branch = $sprint25[0]
        branchExit = 0
        origin = 'git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git'
        head = $head
        upstream = $head
        upstreamExit = 0
        catFileExit = 0
        ancestorExit = 0
        pushExit = 0
        status = @()
    }
    foreach ($key in $Overrides.Keys) { $scenario[$key] = $Overrides[$key] }
    return $scenario
}

$results = New-Object System.Collections.ArrayList
$failures = 0

function Invoke-Case {
    param(
        [string]$Name,
        [hashtable]$Scenario,
        [ValidateSet('push', 'refuse')][string]$Expect,
        [string]$ExpectMessage,
        [string]$WorkingDirectory
    )
    Set-Content -LiteralPath $script:argvLog -Value $null
    (New-Scenario $Scenario) | ConvertTo-Json -Depth 5 |
        Set-Content -LiteralPath $script:scenarioPath -Encoding UTF8
    $previousPath = $env:PATH
    $env:PATH = $script:root + ';' + $previousPath
    $cwd = if ($WorkingDirectory) { $WorkingDirectory } else { $script:permittedFull }
    Push-Location $cwd
    # A refused case writes its reason to stderr, which Windows PowerShell 5.1
    # turns into a NativeCommandError. Under Stop that would terminate this
    # harness instead of recording the refusal, so the child's exit code is the
    # authority here exactly as it is inside the wrapper.
    $previousPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $output = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script:policyFull 2>&1 |
            ForEach-Object { $_.ToString() }
        $code = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousPreference
        Pop-Location
        $env:PATH = $previousPath
    }
    $rendered = ($output -join "`n")
    $argv = @()
    if (Test-Path -LiteralPath $script:argvLog) {
        $argv = @(Get-Content -LiteralPath $script:argvLog | Where-Object { $_ -ne '' })
    }
    $pushCalls = @($argv | Where-Object { $_ -match '(^|\s)push(\s|$)' })

    $ok = $true
    $detail = ''
    if ($Expect -eq 'push') {
        if ($code -ne 0) { $ok = $false; $detail = "expected success, exit=$code" }
        elseif ($pushCalls.Count -ne 1) { $ok = $false; $detail = "expected exactly one push call, saw $($pushCalls.Count)" }
        else {
            $expectedRefspec = 'HEAD:refs/heads/' + $Scenario['branch']
            if (-not $Scenario.ContainsKey('branch')) { $expectedRefspec = 'HEAD:refs/heads/' + $sprint25[0] }
            if ($pushCalls[0] -notmatch [regex]::Escape('push --set-upstream origin ' + $expectedRefspec)) {
                $ok = $false; $detail = "unexpected refspec: $($pushCalls[0])"
            }
        }
    }
    else {
        if ($code -eq 0) { $ok = $false; $detail = 'expected refusal, but the wrapper exited 0' }
        if ($ExpectMessage -and $rendered -notmatch [regex]::Escape($ExpectMessage)) {
            $ok = $false
            $detail = ($detail + " message did not contain '$ExpectMessage'").Trim()
        }
    }
    # No case, accepted or refused, may ever request a destructive or wildcard
    # operation.
    foreach ($forbidden in @('--force', '--delete', '--mirror', '--tags', '--prune',
            '+refs', 'refs/tags', ' -f ', 'reset', 'clean', 'rebase', 'filter-branch',
            'update-ref', '*')) {
        $hit = @($argv | Where-Object { $_.Contains($forbidden) })
        if ($hit.Count -gt 0) {
            $ok = $false
            $detail = ($detail + " forbidden token '$forbidden' in argv: $($hit[0])").Trim()
        }
    }

    if (-not $ok) { $script:failures++ }
    [void]$script:results.Add([pscustomobject]@{
        Case = $Name
        Expected = $Expect
        Exit = $code
        GitCalls = $argv.Count
        Result = if ($ok) { 'PASS' } else { 'FAIL' }
        Detail = $detail
    })
}

try {
    # --- Accepted: the two intended Sprint 25 branches, in the real worktree ---
    Invoke-Case -Name 'sprint25 product branch in permitted worktree' `
        -Scenario @{ branch = $sprint25[0] } -Expect push
    Invoke-Case -Name 'sprint25 records branch in permitted worktree' `
        -Scenario @{ branch = $sprint25[1] } -Expect push

    # --- Accepted: a pre-existing allow-listed branch still works ---
    Invoke-Case -Name 'existing allow-listed sprint24 branch' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint24-deinonychus-tyrannosaurus' } `
        -Expect push

    # --- Accepted: the lab root itself, not only a worktree below it ---
    Invoke-Case -Name 'lab-root repository is inside the boundary' `
        -Scenario @{ toplevel = $labFull; branch = $sprint25[0] } -Expect push

    # --- Refused: not on the allow-list ---
    Invoke-Case -Name 'unallowlisted branch' `
        -Scenario @{ branch = 'codex/not-authorized' } `
        -Expect refuse -ExpectMessage 'Refusing to push non-allowlisted branch'

    # --- Refused: near-match and prefix collisions in both directions ---
    Invoke-Case -Name 'allow-listed name with a suffix' `
        -Scenario @{ branch = $sprint25[0] + '-extra' } `
        -Expect refuse -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'truncated prefix of an allow-listed name' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint25-brachiosaurus' } `
        -Expect refuse -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'allow-listed name under a different namespace' `
        -Scenario @{ branch = 'wip/' + $sprint25[0] } `
        -Expect refuse -ExpectMessage 'Refusing to push non-allowlisted branch'

    # --- Refused: detached HEAD ---
    Invoke-Case -Name 'detached HEAD' -Scenario @{ branch = '' } `
        -Expect refuse -ExpectMessage 'Refusing to push from a detached HEAD'

    # --- Refused: outside the lab boundary, including the sibling-prefix trap ---
    Invoke-Case -Name 'repository outside the lab' `
        -Scenario @{ toplevel = 'C:\Dev\SomeOtherRepository' } `
        -Expect refuse -ExpectMessage 'Refusing to push a repository outside'
    Invoke-Case -Name 'sibling directory sharing the lab name prefix' `
        -Scenario @{ toplevel = $labFull + 'Evil' } `
        -Expect refuse -ExpectMessage 'Refusing to push a repository outside'

    # --- Refused: unexpected origin ---
    Invoke-Case -Name 'unexpected origin remote' `
        -Scenario @{ origin = 'git@github.com:someone-else/fork.git' } `
        -Expect refuse -ExpectMessage 'Refusing unexpected origin'

    # --- Propagated: lookup failures ---
    Invoke-Case -Name 'repository lookup fails' `
        -Scenario @{ toplevelExit = 128 } `
        -Expect refuse -ExpectMessage 'failed with exit code 128'
    Invoke-Case -Name 'branch lookup fails' `
        -Scenario @{ branchExit = 128 } `
        -Expect refuse -ExpectMessage 'failed with exit code 128'

    # --- Propagated: the qualified-baseline lineage guard ---
    Invoke-Case -Name 'qualified baseline object missing' `
        -Scenario @{ catFileExit = 1 } `
        -Expect refuse -ExpectMessage 'failed with exit code 1'
    Invoke-Case -Name 'HEAD does not descend from the qualified baseline' `
        -Scenario @{ ancestorExit = 1 } `
        -Expect refuse -ExpectMessage 'failed with exit code 1'

    # --- Propagated: an honest upstream push failure ---
    Invoke-Case -Name 'upstream push rejected' -Scenario @{ pushExit = 1 } `
        -Expect refuse -ExpectMessage 'failed with exit code 1'

    # --- Refused after pushing: upstream does not match what we pushed ---
    Invoke-Case -Name 'upstream verification mismatch' `
        -Scenario @{ upstream = '2222222222222222222222222222222222222222' } `
        -Expect refuse -ExpectMessage 'upstream verification differs'
    Invoke-Case -Name 'upstream read fails after push' `
        -Scenario @{ upstreamExit = 128 } `
        -Expect refuse -ExpectMessage 'failed with exit code 128'

    # --- Accepted with a dirty tree: warns, pushes only committed HEAD ---
    Invoke-Case -Name 'dirty worktree warns but pushes committed HEAD' `
        -Scenario @{ branch = $sprint25[0]; status = @(' M some/file.cs') } -Expect push

    # --- Measured latitude, recorded rather than assumed ---
    # PowerShell's -notcontains compares strings case-insensitively, so a local
    # branch whose name differs from an authorized one only in letter case is
    # accepted. It is pinned here so the behaviour is measured: the destination
    # is still refs/heads/<that exact local name>, it is still a non-force push
    # of HEAD, and no unrelated branch, tag namespace or deletion is reachable.
    # Treat it as latitude in the allow-list comparison, not as a second
    # authorized branch.
    Invoke-Case -Name 'case-variant of an allow-listed name is accepted (latitude)' `
        -Scenario @{ branch = 'Codex/Expanded-Summoning-Sprint25-Brachiosaurus-Elephant' } `
        -Expect push
}
finally {
    try { Remove-Item -Recurse -Force -LiteralPath $root -ErrorAction Stop } catch { }
}

Write-Host ''
Write-Host "Reviewed wrapper   : $policyFull"
Write-Host "Reviewed SHA-256   : $reviewedHash"
Write-Host "Parser errors      : 0; statements $($ast.EndBlock.Statements.Count)"
Write-Host "Allow-list entries : $($allowed.Count); pattern entries 0"
Write-Host "Permitted root     : $permittedFull"
Write-Host ''
$results | Format-Table -AutoSize | Out-String -Width 200 | Write-Host
if ($failures -ne 0) {
    throw "Push policy wrapper behaviour audit FAILED: $failures of $($results.Count) cases."
}
Write-Host ("Push policy wrapper behaviour audit passed: " +
    "$($results.Count) isolated cases; fake Git only; no remote contacted and no push performed.")
