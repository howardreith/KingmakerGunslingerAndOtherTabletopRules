<#
.SYNOPSIS
Isolated behaviour tests for the real push-policy wrapper.

.DESCRIPTION
The wrapper at C:/Dev/KingmakerGunslingerLab/codex-policy lives outside this
repository, so it has never had a behaviour test. Keyword presence is not a
behaviour test: these cases run the ACTUAL script against a controlled fake
Git executable placed first on PATH, capture every requested argv with its
argument boundaries intact, and assert the decision, the failure stage and the
exact push argument vector.

Two properties matter more than the pass/fail decision and are asserted
directly:

* A precondition refusal must occur BEFORE any mutation. Every refusal case
  declares the exact number of push attempts it is allowed to make, and the
  precondition cases declare zero. A wrapper that pushed and then reported the
  expected failure would fail here, which the harness-negative control at the
  end proves.
* Branch names are compared case-sensitively. Git refs differing only in case
  can be distinct refs, so a differently cased spelling is not the authorized
  ref and must be refused.

Nothing here contacts a remote, performs a push, or touches the real
repository. The fake Git never reaches the network and every answer is
scripted; the harness refuses to run if `git` does not resolve to the fake.
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
    [string]$LabRoot = 'C:/Dev/KingmakerGunslingerLab',
    # Run ONLY the harness-negative control, which proves the
    # refusal-before-mutation assertion rejects a wrapper that pushes and then
    # reports the expected failure. The full run always includes this control
    # as well; this switch just makes it separately addressable.
    [switch]$SelfTestNegativeControl
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
    if ($allowed -cnotcontains $name) {
        throw "Intended Sprint 25 branch missing from the allow-list: $name"
    }
}
$wildcards = @($allowed | Where-Object { $_ -match '[\*\?\[\]]' })
if ($wildcards.Count -ne 0) {
    throw "Allow-list contains a pattern entry: $($wildcards -join ', ')"
}

$root = Join-Path ([IO.Path]::GetTempPath()) ('kmg-push-audit-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
$argvLog = Join-Path $root 'argv.jsonl'
$scenarioPath = Join-Path $root 'scenario.json'

# The fake Git. It records every argv as a JSON array, so argument boundaries
# survive into the assertions instead of being flattened into one string.
#
# Deliberately no param block: `git cat-file -e ...` would otherwise bind -e as
# an ambiguous PowerShell parameter name instead of a remaining argument. The
# cmd shim also eats the caret in `<sha>^{commit}`, which is harmless because
# no case inspects that argument; the push refspec and @{u} pass through intact.
$responder = Join-Path $root 'fake-git.ps1'
@'
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSCommandPath
$scenario = Get-Content -Raw -LiteralPath (Join-Path $root 'scenario.json') | ConvertFrom-Json
$record = [ordered]@{ argv = @($args) }
Add-Content -LiteralPath (Join-Path $root 'argv.jsonl') `
    -Value ($record | ConvertTo-Json -Depth 3 -Compress)
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

# A deliberately bad wrapper: it pushes first and only then refuses. The
# harness-negative control runs the same assertions against it and requires
# them to FAIL, which is what proves the refusal-before-mutation check has
# teeth rather than passing vacuously.
$badPolicy = Join-Path $root 'bad-wrapper.ps1'
@'
$ErrorActionPreference = 'Stop'
$root = (& git rev-parse --show-toplevel | Select-Object -First 1).Trim()
& git -C $root push --set-upstream origin 'HEAD:refs/heads/codex/not-authorized' | Out-Null
throw "Refusing to push non-allowlisted branch 'codex/not-authorized'."
'@ | Set-Content -LiteralPath $badPolicy -Encoding UTF8

# The read-only verbs the wrapper is allowed to use. Anything else, and any
# second push, is a mutation this audit must catch.
$readOnlyVerbs = @('rev-parse', 'branch', 'remote', 'cat-file', 'merge-base', 'status')

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
        [Parameter(Mandatory = $true)][int]$ExpectedPushCalls,
        [string]$ExpectMessage,
        [string]$WorkingDirectory,
        [string]$OverridePolicy,
        # The harness-negative control expects its own assertions to fail.
        [switch]$ExpectAuditFailure
    )
    Set-Content -LiteralPath $script:argvLog -Value $null
    (New-Scenario $Scenario) | ConvertTo-Json -Depth 5 |
        Set-Content -LiteralPath $script:scenarioPath -Encoding UTF8
    $previousPath = $env:PATH
    $env:PATH = $script:root + ';' + $previousPath
    # Prove the fake is what will run. An absolute real-Git fallback would
    # silently turn this audit into a live-fire exercise.
    $resolved = (Get-Command git -ErrorAction SilentlyContinue)
    if ($null -eq $resolved -or
        -not [string]::Equals($resolved.Source, $script:shim,
            [StringComparison]::OrdinalIgnoreCase)) {
        $env:PATH = $previousPath
        throw "Refusing to run: git resolves to '$($resolved.Source)', not the fake at $($script:shim)."
    }
    $cwd = if ($WorkingDirectory) { $WorkingDirectory } else { $script:permittedFull }
    $target = if ($OverridePolicy) { $OverridePolicy } else { $script:policyFull }
    Push-Location $cwd
    # A refused case writes its reason to stderr, which Windows PowerShell 5.1
    # turns into a NativeCommandError. Under Stop that would terminate this
    # harness instead of recording the refusal, so the child's exit code is the
    # authority here exactly as it is inside the wrapper.
    $previousPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $output = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $target 2>&1 |
            ForEach-Object { $_.ToString() }
        $code = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousPreference
        Pop-Location
        $env:PATH = $previousPath
    }
    $rendered = ($output -join "`n")

    # Argument boundaries survive: each line is one invocation's argv array.
    $calls = @()
    if (Test-Path -LiteralPath $script:argvLog) {
        foreach ($line in (Get-Content -LiteralPath $script:argvLog)) {
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            $calls += , @(($line | ConvertFrom-Json).argv)
        }
    }
    $pushCalls = @($calls | Where-Object { $_ -ccontains 'push' })

    $ok = $true
    $detail = ''
    # The refusal-before-mutation contract, asserted for every case.
    if ($pushCalls.Count -ne $ExpectedPushCalls) {
        $ok = $false
        $detail = "expected $ExpectedPushCalls push call(s), saw $($pushCalls.Count)"
    }
    if ($Expect -eq 'push') {
        if ($code -ne 0) { $ok = $false; $detail = (($detail + " expected success, exit=$code").Trim()) }
    }
    else {
        if ($code -eq 0) { $ok = $false; $detail = (($detail + ' expected refusal, but exit was 0').Trim()) }
        if ($ExpectMessage -and $rendered -notmatch [regex]::Escape($ExpectMessage)) {
            $ok = $false
            $detail = (($detail + " message lacked '$ExpectMessage'").Trim())
        }
    }
    # The accepted push is compared as a complete ordinal argument vector,
    # including element count and case, not as a substring of a joined string.
    if ($ExpectedPushCalls -ge 1 -and $pushCalls.Count -ge 1) {
        $branchName = if ($Scenario.ContainsKey('branch')) { $Scenario['branch'] } else { $sprint25[0] }
        $expectedRoot = if ($Scenario.ContainsKey('toplevel')) { $Scenario['toplevel'] } else { $script:permittedFull }
        $expectedArgv = @('-C', $expectedRoot, 'push', '--set-upstream', 'origin',
            ('HEAD:refs/heads/' + $branchName))
        $actual = @($pushCalls[0])
        if ($actual.Count -ne $expectedArgv.Count) {
            $ok = $false
            $detail = (($detail + " push argv count $($actual.Count) != $($expectedArgv.Count)").Trim())
        }
        else {
            for ($i = 0; $i -lt $expectedArgv.Count; $i++) {
                if (-not [string]::Equals($actual[$i], $expectedArgv[$i],
                        [StringComparison]::Ordinal)) {
                    $ok = $false
                    $detail = (($detail + " push argv[$i] '$($actual[$i])' != '$($expectedArgv[$i])'").Trim())
                    break
                }
            }
        }
    }
    # Nothing outside the read-only verbs and the single accounted push may run.
    foreach ($call in $calls) {
        $argv = @($call)
        $verb = if ($argv.Count -ge 3 -and $argv[0] -ceq '-C') { $argv[2] }
                elseif ($argv.Count -ge 1) { $argv[0] } else { '' }
        if ($verb -ceq 'push') { continue }
        if ($script:readOnlyVerbs -cnotcontains $verb) {
            $ok = $false
            $detail = (($detail + " unexpected git verb '$verb'").Trim())
        }
    }
    foreach ($call in $calls) {
        foreach ($argument in @($call)) {
            foreach ($forbidden in @('--force', '-f', '--delete', '--mirror',
                    '--tags', '--prune', '+refs', '--all')) {
                if ([string]::Equals($argument, $forbidden, [StringComparison]::Ordinal)) {
                    $ok = $false
                    $detail = (($detail + " forbidden argument '$forbidden'").Trim())
                }
            }
            if ($argument -clike '*refs/tags/*') {
                $ok = $false
                $detail = (($detail + ' tag namespace in argv').Trim())
            }
        }
    }

    if ($ExpectAuditFailure) {
        # Invert: this control passes only when the assertions above rejected it.
        if ($ok) {
            $script:failures++
            [void]$script:results.Add([pscustomobject]@{
                Case = $Name; Expected = 'audit-must-fail'; Exit = $code
                GitCalls = $calls.Count; Result = 'FAIL'
                Detail = 'a push-before-refusal wrapper was accepted' })
            return
        }
        [void]$script:results.Add([pscustomobject]@{
            Case = $Name; Expected = 'audit-must-fail'; Exit = $code
            GitCalls = $calls.Count; Result = 'PASS'; Detail = 'correctly rejected' })
        return
    }

    if (-not $ok) { $script:failures++ }
    [void]$script:results.Add([pscustomobject]@{
        Case = $Name
        Expected = "$Expect/$ExpectedPushCalls"
        Exit = $code
        GitCalls = $calls.Count
        Result = if ($ok) { 'PASS' } else { 'FAIL' }
        Detail = $detail
    })
}

try {
    if ($SelfTestNegativeControl) {
        Invoke-Case -Name 'control: push-before-refusal wrapper is rejected' `
            -Scenario @{ branch = 'codex/not-authorized' } `
            -Expect refuse -ExpectedPushCalls 0 `
            -ExpectMessage 'Refusing to push non-allowlisted branch' `
            -OverridePolicy $badPolicy -ExpectAuditFailure
        $results | Format-Table -AutoSize | Out-String -Width 200 | Write-Host
        if ($failures -ne 0) { throw 'Harness-negative control FAILED.' }
        Write-Host 'Harness-negative control ran and passed: a push-before-refusal wrapper is rejected.'
        return
    }
    # --- Accepted: exactly one push, exact argv ---
    Invoke-Case -Name 'sprint27 product branch in permitted worktree' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint27-griffon' } -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'sprint27 records branch in permitted worktree' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint27-release-records' } -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'sprint27 differently cased branch is refused' `
        -Scenario @{ branch = 'codex/Expanded-Summoning-Sprint27-Griffon' } `
        -Expect refuse -ExpectedPushCalls 0 -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'sprint28 product branch in permitted worktree' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint28-hill-stone-fire-giants' } -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'sprint28 records branch in permitted worktree' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint28-release-records' } -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'sprint28 differently cased branch is refused' `
        -Scenario @{ branch = 'codex/Expanded-Summoning-Sprint28-Hill-Stone-Fire-Giants' } `
        -Expect refuse -ExpectedPushCalls 0 -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'sprint29 product branch in permitted worktree' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint29-cloud-storm-ettin' } -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'sprint29 records branch in permitted worktree' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint29-release-records' } -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'sprint29 differently cased branch is refused' `
        -Scenario @{ branch = 'codex/Expanded-Summoning-Sprint29-Cloud-Storm-Ettin' } `
        -Expect refuse -ExpectedPushCalls 0 -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'sprint25 product branch in permitted worktree' `
        -Scenario @{ branch = $sprint25[0] } -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'sprint25 records branch in permitted worktree' `
        -Scenario @{ branch = $sprint25[1] } -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'existing allow-listed sprint24 branch' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint24-deinonychus-tyrannosaurus' } `
        -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'lab-root repository is inside the boundary' `
        -Scenario @{ toplevel = $labFull; branch = $sprint25[0] } `
        -Expect push -ExpectedPushCalls 1
    Invoke-Case -Name 'dirty worktree warns but pushes committed HEAD' `
        -Scenario @{ branch = $sprint25[0]; status = @(' M some/file.cs') } `
        -Expect push -ExpectedPushCalls 1

    # --- Precondition refusals: the mutation must never be reached ---
    Invoke-Case -Name 'unallowlisted branch' `
        -Scenario @{ branch = 'codex/not-authorized' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'allow-listed name with a suffix' `
        -Scenario @{ branch = $sprint25[0] + '-extra' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'truncated prefix of an allow-listed name' `
        -Scenario @{ branch = 'codex/expanded-summoning-sprint25-brachiosaurus' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'allow-listed name under a different namespace' `
        -Scenario @{ branch = 'wip/' + $sprint25[0] } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push non-allowlisted branch'
    # Git refs differing only in case can be distinct refs, so a differently
    # cased spelling is not the authorized ref.
    Invoke-Case -Name 'case variant of an allow-listed name is refused' `
        -Scenario @{ branch = 'Codex/Expanded-Summoning-Sprint25-Brachiosaurus-Elephant' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'upper-case variant of a sprint24 branch is refused' `
        -Scenario @{ branch = 'CODEX/EXPANDED-SUMMONING-SPRINT24-DEINONYCHUS-TYRANNOSAURUS' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push non-allowlisted branch'
    Invoke-Case -Name 'detached HEAD' -Scenario @{ branch = '' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push from a detached HEAD'
    Invoke-Case -Name 'repository outside the lab' `
        -Scenario @{ toplevel = 'C:\Dev\SomeOtherRepository' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push a repository outside'
    Invoke-Case -Name 'sibling directory sharing the lab name prefix' `
        -Scenario @{ toplevel = $labFull + 'Evil' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push a repository outside'
    Invoke-Case -Name 'unexpected origin remote' `
        -Scenario @{ origin = 'git@github.com:someone-else/fork.git' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing unexpected origin'
    Invoke-Case -Name 'repository lookup fails' `
        -Scenario @{ toplevelExit = 128 } `
        -Expect refuse -ExpectedPushCalls 0 -ExpectMessage 'failed with exit code 128'
    Invoke-Case -Name 'branch lookup fails' `
        -Scenario @{ branchExit = 128 } `
        -Expect refuse -ExpectedPushCalls 0 -ExpectMessage 'failed with exit code 128'
    Invoke-Case -Name 'qualified baseline object missing' `
        -Scenario @{ catFileExit = 1 } `
        -Expect refuse -ExpectedPushCalls 0 -ExpectMessage 'failed with exit code 1'
    Invoke-Case -Name 'HEAD does not descend from the qualified baseline' `
        -Scenario @{ ancestorExit = 1 } `
        -Expect refuse -ExpectedPushCalls 0 -ExpectMessage 'failed with exit code 1'

    # --- Post-attempt failures: exactly one attempt, then an honest failure ---
    Invoke-Case -Name 'upstream push rejected' -Scenario @{ pushExit = 1 } `
        -Expect refuse -ExpectedPushCalls 1 -ExpectMessage 'failed with exit code 1'
    Invoke-Case -Name 'upstream verification mismatch' `
        -Scenario @{ upstream = '2222222222222222222222222222222222222222' } `
        -Expect refuse -ExpectedPushCalls 1 -ExpectMessage 'upstream verification differs'
    Invoke-Case -Name 'upstream read fails after push' `
        -Scenario @{ upstreamExit = 128 } `
        -Expect refuse -ExpectedPushCalls 1 -ExpectMessage 'failed with exit code 128'

    # --- Harness-negative control ---
    # A wrapper that pushes and only then reports the expected refusal must be
    # rejected by the assertions above. Without this, "zero push calls" could
    # pass vacuously.
    Invoke-Case -Name 'control: push-before-refusal wrapper is rejected' `
        -Scenario @{ branch = 'codex/not-authorized' } `
        -Expect refuse -ExpectedPushCalls 0 `
        -ExpectMessage 'Refusing to push non-allowlisted branch' `
        -OverridePolicy $badPolicy -ExpectAuditFailure
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
$results | Format-Table -AutoSize | Out-String -Width 220 | Write-Host
if ($failures -ne 0) {
    throw "Push policy wrapper behaviour audit FAILED: $failures of $($results.Count) cases."
}
Write-Host ("Push policy wrapper behaviour audit passed: " +
    "$($results.Count) isolated cases; fake Git only; no remote contacted and no push performed. " +
    'Refusal-before-mutation and exact push argv are asserted, and the negative control proves it.')
